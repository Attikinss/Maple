using UnityEngine;

namespace Maple.Blackboards
{
    [System.Serializable]
    public class BlackboardKeyFloat : BlackboardKey
    {
        private float m_Value = 0.0f;

        public override void UpdateEntryInfo(BlackboardEntry entry)
        {
            if (!(entry.Value is float))
            {
                Debug.LogError($"({Name}): Cannot update BlackboardKey - type mismatch!");
                return;
            }

            Name = entry.Name;
            m_Value = (float)entry.Value;
        }

        public override T GetValue<T>()
        {
            Debug.Assert(typeof(T) == typeof(float), $"(BlackboardKey {Name}): Value type mismatch!");

            // ew.
            return (T)(object)m_Value;
        }
    }
}
