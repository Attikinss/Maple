using UnityEngine;

namespace Maple.Blackboards
{
    [System.Serializable]
    public class BlackboardKeyInt : BlackboardKey
    {
        private int m_Value;

        public override void UpdateEntryInfo(BlackboardEntry entry)
        {
            if (entry.Value is int)
            {
                Debug.LogError($"({Name}): Cannot update BlackboardKey - type mismatch!");
                return;
            }

            Name = entry.Name;
            m_Value = (int)entry.Value;
        }
    }
}
