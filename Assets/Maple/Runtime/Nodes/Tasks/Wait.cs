using UnityEngine;

namespace Maple.Nodes
{
    [NodeCategory("Task")]
    public sealed class Wait : Task
    {
        [Tooltip("How long to wait before exiting the node."), SerializeField]
        private float m_Duration = 1.0f;
        private float m_StartTime = 0.0f;

        protected override void OnEnter()
        {
            m_StartTime = Time.time;
        }

        protected override void OnExit()
        {
            m_StartTime = 0.0f;
        }

        protected override NodeResult OnTick()
        {
            if (Time.time - m_StartTime >= m_Duration)
                return NodeResult.Success;

            return NodeResult.Running;
        }
    }
}