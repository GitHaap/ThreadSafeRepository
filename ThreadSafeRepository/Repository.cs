using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ThreadSafe
{
    public class Repository<T>
    {
        byte[] m_stateBytes;
        uint m_currentRevision;

        readonly object m_syncRoot = new object();

        LinkedList<byte[]> m_undoList = new LinkedList<byte[]>();
        LinkedList<byte[]> m_redoList = new LinkedList<byte[]>();

        public Repository(T state, int historyBufferLength = 10)
        {
            CurrentState = state;
            HistoryBufferLength = 10;
            m_currentRevision = 1;
        }

        public int HistoryBufferLength { get; private set; }


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
                m_undoList.AddLast(m_stateBytes);

                // remove oldest state
                if (m_undoList.Count > HistoryBufferLength)
                {
                    m_undoList.RemoveFirst();
                }

                CurrentState = workingState;
                CurrentRevision = workingRevision + 1; // revision up
                return CurrentRevision;
            }
        }

        public void Undo()
        {
            lock (m_syncRoot)
            {
                if (m_undoList.Count == 0)
                {
                    return;
                }

                // pull prev state
                byte[] prevBytes = m_undoList.Last();
                m_undoList.RemoveLast();

                // push current state to redo buffer
                m_redoList.AddFirst(m_stateBytes);

                // update current
                m_stateBytes = prevBytes;
                m_currentRevision--; // revision down
            }
        }

        public void Redo()
        {
            lock (m_syncRoot)
            {
                if (m_redoList.Count == 0)
                {
                    return;
                }

                // pull next state
                byte[] nextBytes = m_redoList.First();
                m_redoList.RemoveFirst();

                // push current state to undo buffer
                m_undoList.AddLast(m_stateBytes);

                // update current
                m_stateBytes = nextBytes;
                m_currentRevision++; // revision up
            }
        }

        public void Rollback()
        {
            //TODO: 世代管理する
        }
        public void History()
        {
            //TODO: 世代管理する
        }
    }
}
