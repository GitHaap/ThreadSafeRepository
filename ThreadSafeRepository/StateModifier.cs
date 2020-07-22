using System;
using System.Collections.Generic;
using System.Text;

namespace ThreadSafe
{
	public class StateModifier<T>
    {
		readonly Repository<T> m_repos;

        /// <summary>
        /// Get/Set WIP state. Modify this state directly.
        /// </summary>
        public T WorkingState { get; set; }


        internal StateModifier(Repository<T> repos)
        {
            m_repos = repos;
            WorkingState = m_repos.CurrentState;
        }

        /// <summary>
        /// Commit WorkingState. CurrentState of the repository will be updated.
        /// </summary>
        public void Commit()
        {
            m_repos.CurrentState = WorkingState;
        }
    }
}
