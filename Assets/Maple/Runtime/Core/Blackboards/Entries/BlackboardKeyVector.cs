using UnityEngine;

namespace Maple.Blackboards
{
    [System.Serializable]
    public class BlackboardKeyVector : BlackboardKey
    {
        private Vector3 m_Value;

        public override void UpdateEntryInfo(BlackboardEntry entry)
        {
            if (entry.Value is Vector3)
            {
                Debug.LogError($"({Name}): Cannot update BlackboardKey - type mismatch!");
                return;
            }

            Name = entry.Name;
            m_Value = (Vector3)entry.Value;
        }
    }
}
