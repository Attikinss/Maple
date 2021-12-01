using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Maple.Blackboards
{
    [System.Serializable]
    public class Blackboard : ScriptableObject
    {
        public string Name { get; private set; }
        public List<BlackboardEntry> Entries = new List<BlackboardEntry>();

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Maple AI/Create/Blackboard")]
        public static Blackboard CreateAsset()
        {
            // Create a default/empty tree instance
            var blackboard = Create("New Blackboard");

            Utilities.Utilities.CreateAssetFromItem(blackboard);

            return blackboard;
        }
#endif

        public static Blackboard Create(string name)
        {
            var blackboard = ScriptableObject.CreateInstance<Blackboard>();
            blackboard.Name = name;
            blackboard.name = name;

            return blackboard;
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
                return;
            }

            entry = BlackboardEntry.Create(name, entryValue, this);
            Entries.Add(entry);

            AddToAsset(entry);
        }

        public void AddEntry(BlackboardEntry entry)
        {
            if (Entries.Contains(entry))
            {
                Debug.LogError($"BlackboardEntry ({entry.Name}): Cannot add entry - entry already exists!");
                return;
            }

            Entries.Add(entry);
            AddToAsset(entry);
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
            Entries.Remove(entry);
            RemoveFromAsset(entry);
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
            foreach (var entry in Entries)
            {
                // Return the entry if the name and value type matches
                if (entry.Name == name && entry.Value.GetType() == valueType)
                    return entry;
            }

            return null;
        }


        private void AddToAsset(BlackboardEntry entry)
        {
#if UNITY_EDITOR
            if (UnityEditor.AssetDatabase.GetAssetPath(this).Length > 0)
            {
                UnityEditor.AssetDatabase.AddObjectToAsset(entry, this);
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.SaveAssets();
            }
#endif
        }


        private void RemoveFromAsset(BlackboardEntry entry)
        {
#if UNITY_EDITOR
            if (UnityEditor.AssetDatabase.GetAssetPath(this).Length > 0)
            {
                UnityEditor.AssetDatabase.RemoveObjectFromAsset(entry);
                UnityEditor.AssetDatabase.SaveAssets();
            }
#endif
        }
    }
}