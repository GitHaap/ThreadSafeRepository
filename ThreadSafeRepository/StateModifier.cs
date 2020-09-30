using System;
using System.Collections.Generic;
using System.Text;

namespace ThreadSafe
{
	public class StateModifier<T>
	{
		private readonly Repository<T> m_repos;

		/// <summary>
		/// Get/Set WIP state. Modify this state directly.
		/// </summary>
		public T WorkingState { get; set; }

		/// <summary>
		/// Get revision of WorkingState.
		/// </summary>
		public uint WorkingRevision { get; private set; }


		private Repository<T>.Revision m_originalRevision;


		internal StateModifier(Repository<T> repos, Repository<T>.Revision revision)
		{
			m_repos = repos;
			m_originalRevision = revision;
			WorkingState = m_originalRevision.CurrentStateClone;
			WorkingRevision = m_originalRevision.RevisionNumber;
		}

		/// <summary>
		/// Get "deep-copied" original state.
		/// </summary>
		public T OriginalStateClone
		{
			get
			{
				lock (m_repos.m_syncRoot)
				{
					if (m_originalRevision is null)
					{
						return default(T);
					}

					return m_originalRevision.CurrentStateClone;
				}
			}
		}

		/// <summary>
		/// Commit WorkingState. CurrentState of the repository will be updated.
		/// </summary>
		public bool Commit()
		{
			lock (m_repos.m_syncRoot)
			{
				var committedRevision = m_repos.Commit(WorkingState, WorkingRevision);
				if (committedRevision != null)
				{
					// success
					m_originalRevision = committedRevision;
					WorkingRevision = committedRevision.RevisionNumber;
					return true;
				}
				else
				{
					// failure
					return false;
				}
			}
		}

		/// <summary>
		/// Revert original state.
		/// </summary>
		public void Revert()
		{
			lock (m_repos.m_syncRoot)
			{
				WorkingState = m_originalRevision.CurrentStateClone;
				WorkingRevision = m_originalRevision.RevisionNumber;
			}
		}
	}
}
