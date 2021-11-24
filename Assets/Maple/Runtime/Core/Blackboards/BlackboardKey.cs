using UnityEngine;

namespace Maple.Blackboards
{
    // TODO: Refactor blackboard keys to ONLY be a key which looks up into an attached blackboard and pulls a value

    [System.Serializable]
    public class BlackboardKey<T>
    {
        public BlackboardEntryType KeyType = BlackboardEntryType.None;

        // TODO: Make this a dropdown field of matching blackboard entries
        public string Name;

        // TODO: Make this not visible
        public T Value;

        public void UpdateEntryInfo(BlackboardEntry entry)
        {
            if (entry.Value.GetType() != typeof(T))
            {
                Debug.LogError($"({Name}): Cannot update BlackboardKey - type mismatch!");
                return;
            }

            Name = entry.Name;
            Value = (T)entry.Value;
            KeyType = entry.ValueType;
        }
    }
}
