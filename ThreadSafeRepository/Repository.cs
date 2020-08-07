using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ThreadSafe
{
    public class Repository<T>
    {
        private byte[] m_stateBytes;
        private uint m_currentRevision;

        private readonly object m_syncRoot = new object();

        private LinkedList<byte[]> m_undoBuffer = new LinkedList<byte[]>();
        private LinkedList<byte[]> m_redoBuffer = new LinkedList<byte[]>();

        public Repository(T state, int historyBufferMaxSize = 10)
        {
            CurrentState = state;
            HistoryBufferMaxSize = historyBufferMaxSize;
            m_currentRevision = 1;
        }

        public int HistoryBufferMaxSize { get; private set; }


        public T CurrentState
        {
            get
            {
                lock (m_syncRoot)
                {
                    //TODO: オブジェクトプールを使いたい
                    return MessagePackSerializer.Deserialize<T>(m_stateBytes);
                }
            }
            internal set
            {
                lock (m_syncRoot)
                {
                    m_stateBytes = MessagePackSerializer.Serialize<T>(value);
                }
            }
        }
        public uint CurrentRevision
        {
            get
            {
                lock (m_syncRoot)
                {
                    return m_currentRevision;
                }
            }
            private set
            {
                lock (m_syncRoot)
                {
                    m_currentRevision = value;
                }
            }
        }

        public StateModifier<T> GetModifier()
        {
            return new StateModifier<T>(this);
        }

        internal long Commit(T workingState, uint workingRevision)
        {
            lock (m_syncRoot)
            {
                if (CurrentRevision != workingRevision)
                {
                    return -1;
                }

                // push prev state to undo buffer
                m_undoBuffer.AddLast(m_stateBytes);

                // remove oldest state
                if (m_undoBuffer.Count > HistoryBufferMaxSize)
                {
                    m_undoBuffer.RemoveFirst();
                }

                CurrentState = workingState;
                CurrentRevision = workingRevision + 1; // revision up
                return CurrentRevision;
            }
        }

        public bool Undo()
        {
            lock (m_syncRoot)
            {
                if (m_undoBuffer.Count == 0)
                {
                    return false;
                }

                // pull prev state
                byte[] prevBytes = m_undoBuffer.Last();
                m_undoBuffer.RemoveLast();

                // push current state to redo buffer
                m_redoBuffer.AddFirst(m_stateBytes);

                // update current
                m_stateBytes = prevBytes;
                m_currentRevision--; // revision down

                return true;
            }
        }

        public bool Redo()
        {
            lock (m_syncRoot)
            {
                if (m_redoBuffer.Count == 0)
                {
                    return false;
                }

                // pull next state
                byte[] nextBytes = m_redoBuffer.First();
                m_redoBuffer.RemoveFirst();

                // push current state to undo buffer
                m_undoBuffer.AddLast(m_stateBytes);

                // update current
                m_stateBytes = nextBytes;
                m_currentRevision++; // revision up

                return true;
            }
        }
    }
}
