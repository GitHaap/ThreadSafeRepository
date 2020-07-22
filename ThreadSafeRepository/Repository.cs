using MessagePack;
using System;
using System.Collections.Generic;

namespace ThreadSafe
{
    public class Repository<T>
    {
        byte[] m_stateBytes;

        readonly object m_syncRoot = new object();

        public Repository(T state)
        {
            CurrentState = state;
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

        public StateModifier<T> GetModifier()
        {
            return new StateModifier<T>(this);
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
