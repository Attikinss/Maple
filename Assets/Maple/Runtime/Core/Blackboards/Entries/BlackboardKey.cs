using UnityEngine;

namespace Maple.Blackboards
{
    // TODO: Refactor blackboard keys to ONLY be a key which looks up into an attached blackboard and pulls a value

    [System.Serializable]
    public abstract class BlackboardKey
    {
        // TODO: Make this a dropdown field of matching blackboard entries
        public string Name;
        
        public BlackboardEntryType KeyType = BlackboardEntryType.None;

        public abstract void UpdateEntryInfo(BlackboardEntry entry);
    }
}
