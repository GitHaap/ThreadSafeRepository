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


        internal StateModifier(Repository<T> repos)
        {
            m_repos = repos;
            WorkingState = m_repos.CurrentState;
            WorkingRevision = m_repos.CurrentRevision;
        }

        /// <summary>
        /// Commit WorkingState. CurrentState of the repository will be updated.
        /// </summary>
        public bool Commit()
        {
            lock (m_repos.m_syncRoot)
            {
                long committedRevision = m_repos.Commit(WorkingState, WorkingRevision);
                if (committedRevision != -1)
                {
                    // success
                    WorkingRevision = (uint)committedRevision; // revision up
                    return true;
                }
                else
                {
                    // failure
                    return false;
                }
            }
        }
    }
}
