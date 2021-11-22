using UnityEngine;

namespace Maple.Nodes
{
    public sealed class IsAtLocation : Decorator
    {
        [Tooltip("The goal position that the agent must be at/close to for this node to exit successfully."), SerializeField]
        private Vector3 m_Position;

        [Tooltip("How far from the target position the agent can be for the node to still exit successfully."), SerializeField]
        private float m_Threshold;

        protected override void OnEnter() { }

        protected override void OnExit() { }

        protected override NodeResult OnTick()
        {
            // If there is no child node, this node always fails
            if (m_Child)
            {
                if (Vector3.Distance(Owner.Agent.transform.position, m_Position) <= m_Threshold)
                    return m_Child.Tick();
            }

            return NodeResult.Failure;
        }
    }
}
