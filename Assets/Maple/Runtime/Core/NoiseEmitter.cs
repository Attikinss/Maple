using UnityEngine;

namespace Maple
{
    public sealed class NoiseEmitter : MonoBehaviour, INoiseEmitter
    {
        public void EmitNoise(float loudness)
        {
            // "Notify" all agents of the sound
            foreach (var agent in AgentManager.Instance.Agents)
                agent.DetectNoise(this, loudness);
        }
    }
}
