using UnityEngine;

namespace Maple.Blackboards
{
    [System.Serializable]
    public class BlackboardKeyGameObject : BlackboardKey
    {
        private GameObject m_Value = null;

        public override void UpdateEntryInfo(BlackboardEntry entry)
        {
            if (!(entry.Value is GameObject))
            {
                Debug.LogError($"({Name}): Cannot update BlackboardKey - type mismatch!");
                return;
            }

            Name = entry.Name;
            m_Value = (GameObject)entry.Value;
        }

        public override T GetValue<T>()
        {
            Debug.Assert(typeof(T) == typeof(GameObject), $"(BlackboardKey {Name}): Value type mismatch!");

            // ew.
            return (T)(object)m_Value;
        }
    }
}
