using System.Collections.Generic;
using UnityEngine;

namespace Maple
{
    public sealed class AgentManager : MonoBehaviour
    {
        public static AgentManager Instance { get; private set; }

        public List<Agent> Agents { get; private set; } = new List<Agent>();

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Debug.LogWarning("An AgentManager already exists in the scene! New instance destroyed.");
                Destroy(this);
                return;
            }

            Instance = this;
        }
    }
}