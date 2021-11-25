using UnityEngine;

namespace Maple.Blackboards
{
    [System.Serializable]
    public class BlackboardKeyVector : BlackboardKey
    {
        private Vector3 m_Value = Vector3.zero;

        public override void UpdateEntryInfo(BlackboardEntry entry)
        {
            if (!(entry.Value is Vector3))
            {
                Debug.LogError($"({Name}): Cannot update BlackboardKey - type mismatch!");
                return;
            }

            Name = entry.Name;
            m_Value = (Vector3)entry.Value;
        }

        public override T GetValue<T>()
        {
            Debug.Assert(typeof(T) == typeof(Vector3), $"(BlackboardKey {Name}): Value type mismatch!");

            // ew.
            return (T)(object)m_Value;
        }
    }
}
