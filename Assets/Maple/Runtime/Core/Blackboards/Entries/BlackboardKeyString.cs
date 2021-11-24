using UnityEngine;

namespace Maple.Blackboards
{
    [System.Serializable]
    public class BlackboardKeyString : BlackboardKey
    {
        private string m_Value;

        public override void UpdateEntryInfo(BlackboardEntry entry)
        {
            if (entry.Value is string)
            {
                Debug.LogError($"({Name}): Cannot update BlackboardKey - type mismatch!");
                return;
            }

            Name = entry.Name;
            m_Value = (string)entry.Value;
        }
    }
}
