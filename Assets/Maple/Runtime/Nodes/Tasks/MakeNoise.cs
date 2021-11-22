using UnityEngine;

namespace Maple.Nodes
{
    public sealed class MakeNoise : Task
    {
        [Tooltip("Defines how loud the sound is/the range of the sound in the environment."), SerializeField]
        private float m_Loudness = 1.0f;

        protected override void OnEnter() { }
        protected override void OnExit() { }

        protected override NodeResult OnTick()
        {
            if (!Owner.Agent.NoiseEmitter)
            {
                Debug.LogError($"({Owner.Agent.gameObject.name}): Cannot make noise - no NoiseEmitter attached to agent!");
                return NodeResult.Failure;
            }

            Owner.Agent.NoiseEmitter.EmitNoise(m_Loudness);
            return NodeResult.Success;
        }
    }
}