using UnityEngine;

namespace Maple.Nodes
{
    public sealed class Cooldown : Decorator
    {
        [Tooltip("The amount of time this node will be locked for after exiting with success or failure."), SerializeField]
        private float m_CooldownTime = 2.5f;

        private float m_CachedTime = 0.0f;

        protected override void OnEnter() { }
        protected override void OnExit() { }

        protected override NodeResult OnTick()
        {
            // If there is no child node, this node always fails
            if (!m_Child)
            {
                // If the current time has passed the cached cooldown time tick the child node
                if (Time.time >= m_CachedTime)
                {
                    NodeResult result = m_Child.Tick();

                    // Only update the cooldown period if the node exited
                    if (result != NodeResult.Running)
                        m_CachedTime = Time.time + m_CooldownTime;

                    return result;
                }
            }

            return NodeResult.Failure;
        }
    }
}
