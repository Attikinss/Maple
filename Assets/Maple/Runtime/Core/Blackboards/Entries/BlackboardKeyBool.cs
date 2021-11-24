using UnityEngine;

namespace Maple.Blackboards
{
    [System.Serializable]
    public class BlackboardKeyBool : BlackboardKey
    {
        private bool m_Value;

        public override void UpdateEntryInfo(BlackboardEntry entry)
        {
            if (entry.Value is bool)
            {
                Debug.LogError($"({Name}): Cannot update BlackboardKey - type mismatch!");
                return;
            }

            Name = entry.Name;
            m_Value = (bool)entry.Value;
        }
    }
}
