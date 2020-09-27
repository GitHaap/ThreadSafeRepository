using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ThreadSafe
{
	/// <summary>
	/// Thread-safe data manager. You can modify/undo/redo the managed data exclusively.
	/// </summary>
	public class Repository<T>
	{
		internal class Revision
		{
			public Revision(uint revisionId, byte[] stateBytes, DateTime committedTime)
			{
				RevisionNumber = revisionId;
				StateBytes = stateBytes;
				CommittedTime = committedTime;
			}

			public uint RevisionNumber { get; set; }

			public byte[] StateBytes { get; set; }

			public DateTime CommittedTime { get; set; }

			public T CurrentStateClone
			{
				get
				{
					if (StateBytes is null)
					{
						return default(T);
					}
					//TODO: オブジェクトプールを使いたい
					return MessagePackSerializer.Deserialize<T>(StateBytes);
				}
			}
		}

		private Revision m_currentRevision;
		private LinkedList<Revision> m_undoBuffer = new LinkedList<Revision>();
		private LinkedList<Revision> m_redoBuffer = new LinkedList<Revision>();
		private uint m_nextRevisionNumber = 1;

		internal readonly object m_syncRoot = new object();

		/// <summary>
		/// Initializes a new instance of the ThreadSafe.Repository class.
		/// </summary>
		/// <param name="state">State you want to manage</param>
		/// <param name="historyBufferMaxLength">Number that can undo</param>
		public Repository(T state, uint historyBufferMaxLength = 10)
		{
			m_currentRevision = new Revision(m_nextRevisionNumber, MessagePackSerializer.Serialize<T>(state), DateTime.Now);
			m_nextRevisionNumber++;

			HistoryBufferMaxLength = historyBufferMaxLength;
		}

		public uint HistoryBufferMaxLength { get; private set; }


		/// <summary>
		/// Get "deep-copied" current state.
		/// </summary>
		public T CurrentStateClone
		{
			get
			{
				lock (m_syncRoot)
				{
					if (m_currentRevision is null)
					{
						return default(T);
					}

					//TODO: オブジェクトプールを使いたい
					return m_currentRevision.CurrentStateClone;
				}
			}
		}
		/// <summary>
		/// Get revision of CurrentState.
		/// </summary>
		public uint CurrentRevision
		{
			get
			{
				lock (m_syncRoot)
				{
					if (m_currentRevision is null)
					{
						return 0;
					}

					return m_currentRevision.RevisionNumber;
				}
			}
		}

		/// <summary>
		/// Can be modified state via StateModifier.
		/// </summary>
		public StateModifier<T> GetModifier()
		{
			lock (m_syncRoot)
			{
				return new StateModifier<T>(this, m_currentRevision);
			}
		}

		internal Revision Commit(T workingState, uint workingRevision)
		{
			lock (m_syncRoot)
			{
				if (CurrentRevision != workingRevision)
				{
					return null;
				}

				// push prev revision to undo buffer
				m_undoBuffer.AddLast(m_currentRevision);

				// remove oldest revision
				if (m_undoBuffer.Count > HistoryBufferMaxLength)
				{
					m_undoBuffer.RemoveFirst();
				}

				// clear redo bufs
				m_redoBuffer.Clear();

				// create new current (revision up)
				m_currentRevision = new Revision(m_nextRevisionNumber, MessagePackSerializer.Serialize<T>(workingState), DateTime.Now);
				m_nextRevisionNumber++;

				return m_currentRevision;
			}
		}

		/// <summary>
		/// Change to head(latest) revision.
		/// </summary>
		public void Update()
		{
			lock (m_syncRoot)
			{
				while (Redo()) ;
			}
		}

		/// <summary>
		/// Change to previous revision.
		/// </summary>
		public bool Undo()
		{
			lock (m_syncRoot)
			{
				if (m_undoBuffer.Count == 0)
				{
					return false;
				}

				// pull prev revision
				var prevRevision = m_undoBuffer.Last();
				m_undoBuffer.RemoveLast();

				// push current revision to redo buffer
				m_redoBuffer.AddFirst(m_currentRevision);

				// undo current (revision down)
				m_currentRevision = prevRevision;

				return true;
			}
		}

		/// <summary>
		/// Change to next revision.
		/// </summary>
		public bool Redo()
		{
			lock (m_syncRoot)
			{
				if (m_redoBuffer.Count == 0)
				{
					return false;
				}

				// pull next revision
				var nextRevision = m_redoBuffer.First();
				m_redoBuffer.RemoveFirst();

				// push current revision to undo buffer
				m_undoBuffer.AddLast(m_currentRevision);

				// redo current
				m_currentRevision = nextRevision;

				return true;
			}
		}
	}
}
