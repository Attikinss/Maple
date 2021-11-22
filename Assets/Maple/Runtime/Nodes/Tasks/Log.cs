using UnityEngine;

namespace Maple.Nodes
{
    public sealed class Log : Task
    {
        public static bool LoggingEnabled { get; set; } = true;

        [Tooltip("The message that will be printed to the console if logging is enabled."), SerializeField]
        private string m_Message;

        protected override void OnEnter() { }
        protected override void OnExit() { }

        protected override NodeResult OnTick()
        {
            if (LoggingEnabled)
                Debug.Log($"({Owner.Agent.gameObject.name}): {m_Message}");

            return NodeResult.Success;
        }
    }
}