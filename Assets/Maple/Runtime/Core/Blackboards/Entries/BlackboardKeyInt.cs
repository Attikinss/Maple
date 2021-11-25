using UnityEngine;

namespace Maple.Blackboards
{
    [System.Serializable]
    public class BlackboardKeyInt : BlackboardKey
    {
        private int m_Value = 0;

        public override void UpdateEntryInfo(BlackboardEntry entry)
        {
            if (!(entry.Value is int))
            {
                Debug.LogError($"({Name}): Cannot update BlackboardKey - type mismatch!");
                return;
            }

            Name = entry.Name;
            m_Value = (int)entry.Value;
        }

        public override T GetValue<T>()
        {
            Debug.Assert(typeof(T) == typeof(int), $"(BlackboardKey {Name}): Value type mismatch!");

            // ew.
            return (T)(object)m_Value;
        }
    }
}
