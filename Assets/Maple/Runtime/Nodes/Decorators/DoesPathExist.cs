using UnityEngine;

namespace Maple.Nodes
{
    public sealed class DoesPathExist : Decorator
    {
        [Tooltip("The goal position that the agent will calculate a path towards."), SerializeField]
        private Vector3 m_TargetPosition;

        private bool m_HasPath = false;

        protected override void OnEnter()
        {
            m_HasPath = Owner.Agent.TargetReachable(m_TargetPosition);
        }

        protected override void OnExit() { }

        protected override NodeResult OnTick()
        {
            // If there is no child node, this node always fails
            if (m_Child)
            {
                if (m_HasPath)
                    return m_Child.Tick();
            }

            return NodeResult.Failure;
        }
    }
}
