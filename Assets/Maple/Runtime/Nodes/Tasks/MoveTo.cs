using UnityEngine;

namespace Maple.Nodes
{
    public sealed class MoveTo : Task
    {
        [Tooltip("The goal position the agent will try navigating to."), SerializeField]
        private Vector3 m_Position;

        [Tooltip("If true the agent's stopping distance will include its radius."), SerializeField]
        private bool m_ReachTestIncludesAgent = false;

        [Tooltip("If true the agent will navigate to the closest point to the goal if a complete path is not found."), SerializeField]
        private bool m_AllowPartialPath = false;

        [Tooltip("If true the agent will recalculate its path when the goal position changes."), SerializeField]
        private bool m_TrackMovingGoal = false;

        private bool m_AgentMoving = false;

        protected override void OnEnter()
        {
            // Begin moving agent
            m_AgentMoving = Owner.Agent.MoveTo(m_Position, m_AllowPartialPath);
        }

        protected override void OnExit() { }

        protected override NodeResult OnTick()
        {
            // If goal tracking is enabled and the position has
            // updated, re-evaluate the agent's destination
            if (m_TrackMovingGoal && Owner.Agent.Destination != m_Position)
                m_AgentMoving = Owner.Agent.MoveTo(m_Position, m_AllowPartialPath);
                
            if (m_AgentMoving)
            {
                // Return running until agent is at/near target position
                return Owner.Agent.AtTarget(m_Position, m_ReachTestIncludesAgent) ? NodeResult.Success : NodeResult.Running;
            }
            
            // Agent cannot reach destination with current parameters
            return NodeResult.Failure;
        }
    }
}