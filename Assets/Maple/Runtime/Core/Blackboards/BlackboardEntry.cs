using Maple.Nodes;
using System.Collections.Generic;
using UnityEngine;

namespace Maple.Blackboards
{
    public enum BlackboardEntryType { None = 0, Bool, Float, GameObject, Int, String, Vector }

    [System.Serializable]
    public sealed class BlackboardEntry
    {
        public string Name { get; private set; }
        public object Value { get; private set; }
        public BlackboardEntryType ValueType = BlackboardEntryType.None;
        public Blackboard Owner;

        private List<BlackboardKey> m_Listeners = new List<BlackboardKey>();

        private BlackboardEntry(string name, object value, Blackboard owner)
        {
            Name = name;
            Value = value;
            Owner = owner;

            EnumFromType(value.GetType());
        }

        public static BlackboardEntry Create<T>(string name, T value, Blackboard owner)
        {
            if (!TypeValid<T>())
            {
                Debug.LogError($"(Blackboard Entry - {name}): Cannot create entry - type not supported! [{typeof(T).Name}]");
                return null;
            }

            return new BlackboardEntry(name, value, owner);
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.LogError($"(Blackboard Entry - {Name}): Cannot change name - empty values not permitted!");
                return;
            }

            Name = name;

            // TODO: Update linked blackboard keys to reflect change
        }

        public void SetValue<T>(T value, bool setWithoutNotify = false)
        {
            if (value.GetType() != Value.GetType())
            {
                Debug.LogError($"(Blackboard Entry - {Name}): Cannot change value - type mismatch!");
                return;
            }

            if (!TypeValid<T>())
            {
                Debug.LogError($"(Blackboard Entry - {Name}): Cannot change value - type not supported! [{typeof(T).Name}]");
                return;
            }

            if (setWithoutNotify)
                Value = value;
            else
                OnValueChanged(value);
        }

        public void AddListener(BlackboardKey key)
        {
            if (!m_Listeners.Contains(key))
                m_Listeners.Add(key);
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

        private void OnValueChanged(object value)
        {
            Value = value;

            foreach (var listener in m_Listeners)
                listener.UpdateEntryInfo(this);
        }

        private static bool TypeValid<T>()
        {
            var type = typeof(T);
            return type == typeof(bool) ||
                   type == typeof(float) ||
                   type == typeof(GameObject) ||
                   type == typeof(int) ||
                   type == typeof(string) ||
                   type == typeof(Vector3);
        }

        private static System.Type TypeFromEnum(BlackboardEntryType enumType)
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

        private static BlackboardEntryType EnumFromType(System.Type type)
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
