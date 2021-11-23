using Maple.Nodes;
using System.Collections.Generic;
using UnityEngine;

namespace Maple.Blackboard
{
    [System.Serializable]
    public class Blackboard
    {
        public string Name { get; private set; }
        public List<BlackboardEntry> m_Entries = new List<BlackboardEntry>();

        public Blackboard(string name = "New Blackboard")
        {
            Name = name;
        }

        public void AddEntry<T>(string name, T entryValue)
        {
            // Entries with the same name are allowed only if their value type is different
            var entry = FindEntryByName<T>(name);
            if (entry != null)
            {
                // If an entry with the same name and value
                // type already exists notify user and return
                Debug.LogError($"BlackboardEntry ({name}): Cannot add entry - entry already exists!");
            }

            m_Entries.Add(BlackboardEntry.Create(name, entryValue, this));
        }

        public void AddEntry(BlackboardEntry entry)
        {
            if (m_Entries.Contains(entry))
            {
                Debug.LogError($"BlackboardEntry ({entry.Name}): Cannot add entry - entry already exists!");
                return;
            }

            m_Entries.Add(entry);
        }

        public void RemoveEntry<T>(string name) => RemoveEntry(name, typeof(T));

        public void RemoveEntry(string name, System.Type valueType)
        {
            var entry = FindEntryByName(name, valueType);
            if (entry == null)
            {
                // If an entry with the same name and value
                // type doesn't exist notify user and return
                Debug.LogError($"BlackboardEntry ({name}): Cannot remove entry - entry doesn't exist!");
            }

            entry.ClearListeners();
            m_Entries.Remove(entry);
        }

        public void UpdateEntryValue<T>(string name, T newValue)
        {
            var entry = FindEntryByName<T>(name);
            if (entry == null)
            {
                // If an entry with the same name and value
                // type doesn't exist notify user and return
                Debug.LogError($"BlackboardEntry ({name}): Cannot update entry - entry doesn't exist!");
                return;
            }

            entry.SetValue(newValue);
        }

        private BlackboardEntry FindEntryByName<T>(string name) => FindEntryByName(name, typeof(T));

        private BlackboardEntry FindEntryByName(string name, System.Type valueType)
        {
            foreach (var entry in m_Entries)
            {
                // Return the entry if the name and value type matches
                if (entry.Name == name && entry.Value.GetType() == valueType)
                    return entry;
            }

            return null;
        }
    }
}