using UnityEngine;
using Maple.Blackboards;

namespace Maple.Nodes
{
    [NodeCategory("Task")]
    public sealed class Log : Task
    {
        public static bool LoggingEnabled { get; set; } = true;

        [Tooltip("The message that will be printed to the console if logging is enabled."), SerializeField]
        public BlackboardKeyString m_Message;

        protected override void OnEnter() { }

        protected override void OnExit() { }

        protected override NodeResult OnTick()
        {
            if (LoggingEnabled)
                Debug.Log($"({Owner.Agent?.gameObject.name}): {m_Message.GetValue<string>()}");

            return NodeResult.Success;
        }
    }
}