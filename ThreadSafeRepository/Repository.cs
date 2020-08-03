using MessagePack;
using System;
using System.Collections.Generic;

namespace ThreadSafe
{
    public class Repository<T>
    {
        byte[] m_stateBytes;
        uint m_currentRevision;

        readonly object m_syncRoot = new object();


        public Repository(T state)
        {
            CurrentState = state;
            m_currentRevision = 1;
        }

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

                CurrentState = workingState;
                CurrentRevision = workingRevision + 1; // revision up
                return CurrentRevision;
            }
        }

        public void Undo()
        {
            //TODO: 世代管理する
        }
        public void Redo()
        {
            //TODO: 世代管理する
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
