using UnityEngine;

namespace Maple.Nodes
{
    public sealed class Timeout : Decorator
    {
        [Tooltip("The amount of time given for the node to complete before it times out and fails."), SerializeField]
        private float m_TimeLimit = 2.5f;

        [Tooltip("If true the node will keep ticking until it is successful or the time limit is met/exceeded."), SerializeField]
        private bool m_RetryUntilTimeLimit = false;

        private float m_CurrentTime = 0.0f;

        protected override void OnEnter() { }

        protected override void OnExit()
        {
            m_CurrentTime = 0.0f;
        }

        protected override NodeResult OnTick()
        {
            // If there is no child node, this node always fails
            if (m_Child)
            {
                // If the timelimit hasn't been exceeded yet tick the child node
                if (m_CurrentTime <= m_TimeLimit)
                {
                    NodeResult result = m_Child.Tick();

                    // Update time
                    m_CurrentTime += Time.deltaTime;

                    // If the child node failed but retry is checked and the time limit hasn't been
                    // exceeded yet, return running so that the child node can be ticked again.
                    // Otherwise just return the result
                    if (result == NodeResult.Failure && m_RetryUntilTimeLimit && m_CurrentTime < m_TimeLimit)
                        return NodeResult.Running;
                    else
                        return result;
                }
            }

            return NodeResult.Running;
        }
    }
}
