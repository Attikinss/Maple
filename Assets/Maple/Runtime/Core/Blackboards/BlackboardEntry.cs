using System;
using System.Collections.Generic;
using UnityEngine;

namespace Maple.Blackboards
{
    public enum BlackboardEntryType { None = 0, Bool, Float, GameObject, Int, String, Vector }

    [System.Serializable]
    public sealed class BlackboardEntry : ScriptableObject
    {
        public string Name { get; private set; }
        public object Value { get => m_Value != null ? m_Value : GetDefaultValue(); }
        public bool Expand { get; set; }

        private object m_Value;

        public BlackboardEntryType ValueType = BlackboardEntryType.None;
        public Blackboard Owner;

        private List<BlackboardKey> m_Listeners = new List<BlackboardKey>();

        public static BlackboardEntry Create<T>(string name, T value, Blackboard owner)
        {
            // Ensure valid type is used
            if (!TypeValid(value))
            {
                Debug.LogError($"(Blackboard Entry - {name}): Cannot create entry - type not supported! [{typeof(T).Name}]");
                return null;
            }

            // Ensure an owner blackboard is specified
            if (owner == null)
            {
                Debug.LogError($"(Blackboard Entry {name}): Cannot create entry - no owner (Blackboard) specified!");
                return null;
            }

            // Create instance and update values
            var entry = ScriptableObject.CreateInstance<BlackboardEntry>();
            entry.Name = name;
            entry.name = name;
            entry.ValueType = EnumFromType(value.GetType());
            entry.Owner = owner;

            // Ensure a value is assigned, even if it is a default value
            entry.m_Value = value == null ? default(T) : value;

            return entry;
        }

        public void SetName(string name)
        {
            if (Name == name || string.IsNullOrWhiteSpace(name))
                return;

            // Gross way of updating name in project panel
#if UNITY_EDITOR
            bool matchFound = false;
            var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(UnityEditor.AssetDatabase.GetAssetPath(this));
            foreach (var a in assets)
            {
                if (a == this)
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(this);
                    matchFound = true;
                    break;
                }
            }
#endif

            Name = name;
            this.name = name;

#if UNITY_EDITOR
            if (matchFound)
            {
                UnityEditor.AssetDatabase.AddObjectToAsset(this, Owner);
                UnityEditor.AssetDatabase.SaveAssets();
            }
#endif

            // Update listeners BlackboardKey name
            foreach (var listener in m_Listeners)
                listener.Name = name;
        }

        public void SetValue<T>(T value, bool setWithoutNotify = false)
        {
            // Ensure types match
            if (value.GetType() != Value.GetType())
            {
                Debug.LogError($"(Blackboard Entry - {Name}): Cannot change value - type mismatch!");
                return;
            }

            // Ensure type is valid
            if (!TypeValid(value))
            {
                Debug.LogError($"(Blackboard Entry - {Name}): Cannot change value - type not supported! [{typeof(T).Name}]");
                return;
            }

            // Change value and update listeners if need be
            if (setWithoutNotify)
                m_Value = value;
            else
                OnValueChanged(value);
        }

        public void AddListener(BlackboardKey key)
        {
            if (!m_Listeners.Contains(key))
            {
                key.UpdateEntryInfo(this);
                m_Listeners.Add(key);
            }
        }

        public void RemoveListener(BlackboardKey key)
        {
            if (m_Listeners.Contains(key))
            {
                // Disconnect the listener
                m_Listeners.Remove(key);
            }
        }

        public void ClearListeners()
        {
            // Disconnect the listeners
            m_Listeners.Clear();
        }

        public object GetDefaultValue()
        {
            switch (ValueType)
            {
                case BlackboardEntryType.Bool:          return false;
                case BlackboardEntryType.Float:         return 0.0f;
                case BlackboardEntryType.GameObject:    return null;
                case BlackboardEntryType.Int:           return 0;
                case BlackboardEntryType.String:        return "";
                case BlackboardEntryType.Vector:        return Vector3.zero;
                default:                                return null;
            }
        }

        private void OnValueChanged(object value)
        {
            m_Value = value;

            foreach (var listener in m_Listeners)
                listener.UpdateEntryInfo(this);
        }

        private static bool TypeValid<T>(T obj)
        {
            var type = obj.GetType();
            return type == typeof(bool) ||
                   type == typeof(float) ||
                   type == typeof(GameObject) ||
                   type == typeof(int) ||
                   type == typeof(string) ||
                   type == typeof(Vector3);
        }

        public static System.Type TypeFromEnum(BlackboardEntryType enumType)
        {
            switch (enumType)
            {
                case BlackboardEntryType.Bool:
                    return typeof(bool);

                case BlackboardEntryType.Float:
                    return typeof(float);

                case BlackboardEntryType.GameObject:
                    return typeof(GameObject);

                case BlackboardEntryType.Int:
                    return typeof(int);

                case BlackboardEntryType.String:
                    return typeof(string);

                case BlackboardEntryType.Vector:
                    return typeof(Vector3);

                default:
                    return null;
            }
        }

        public static BlackboardEntryType EnumFromType(System.Type type)
        {
            if (type == typeof(bool))
                return BlackboardEntryType.Bool;

            if (type == typeof(float))
                return BlackboardEntryType.Float;

            if (type == typeof(GameObject))
                return BlackboardEntryType.GameObject;

            if (type == typeof(int))
                return BlackboardEntryType.Int;

            if (type == typeof(string))
                return BlackboardEntryType.String;

            if (type == typeof(Vector3))
                return BlackboardEntryType.Vector;

            return BlackboardEntryType.None;
        }
    }
}
