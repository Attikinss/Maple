using UnityEngine;

namespace Maple.Nodes
{
    public sealed class Loop : Decorator
    {
        [Tooltip("The number of times the node will loop/repeat."), Min(0), SerializeField]
        private int m_NumberOfLoops = 3;

        [Tooltip("If true the node will loop indefinitely regardless of its child node's return state."), SerializeField]
        private bool m_InfiniteLoop = false;

        [Tooltip("If InfiniteLoop is true, this node will either loop indefinitely if this value is negative or until this value is met if positive."),
            Min(-1.0f), SerializeField]
        private float m_InfiniteLoopTimeout = 5.0f;

        private int m_CurrentLoops = 0;
        private float m_CurrentTime = 0.0f;

        protected override void OnEnter() { }

        protected override void OnExit()
        {
            m_CurrentLoops = 0;
            m_CurrentTime = 0.0f;
        }

        protected override NodeResult OnTick()
        {
            // If there is no child node, this node always fails
            if (m_Child)
            {
                if (m_InfiniteLoop)
                {
                    if (m_CurrentTime <= m_InfiniteLoopTimeout)
                    {
                        NodeResult result = m_Child.Tick();

                        // Update time only if timeout is not negative
                        if (m_InfiniteLoopTimeout >= 0.0f)
                            m_CurrentTime += Time.deltaTime;

                        // Return running if infinite loop or timeout hasn't been met yet
                        if (m_InfiniteLoopTimeout < 0.0f || m_CurrentTime < m_InfiniteLoopTimeout)
                            return NodeResult.Running;
                        else
                            return result;
                    }
                }
                else
                {
                    NodeResult result = m_Child.Tick();

                    // Return running until loop count is met
                    return (++m_CurrentLoops < m_NumberOfLoops) ? NodeResult.Running : result;
                }
            }

            return NodeResult.Failure;
        }
    }
}
