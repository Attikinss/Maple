using Maple.Blackboards;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Maple.Nodes
{
    public abstract class BaseNode : ScriptableObject, IComparer<BaseNode>, IComparable<BaseNode>
    {
        /// <summary>The unique ID used to handle manipulation of nodes both in code and in the graph tool.</summary>
        public string Guid;
        
        /// <summary>Defines the current activity state of a node hierarchy; if a node is active and running, then its parent will be too.</summary>
        public bool Active;

        /// <summary>Defines the current state of a signle node. (see NodeResult for further information)</summary>
        public NodeResult State;

        /// <summary>The parent of which the node is linked/childed to in the node hierarchy.</summary>
        public BaseNode Parent;

        /// <summary>The order in which the node is executed relative to its siblings. (i.e the other child nodes of the same parent)
        /// <br>Execution priority begins at 1 and increases with each node.</br>
        /// </summary>
        public int ExecutionOrder;

        /// <summary>
        /// The saved position of the node when re-loaded and displayed in the graph tool.
        /// <br>TODO: Move this somewhere else</br>
        /// </summary>
        public Vector2 Position;

        /// <summary>The behaviour tree in which the node belongs to.</summary>
        public BehaviourTree Owner;

        public virtual string IconPath { get; } = "Icons/User";

        private List<BlackboardKey> m_BlackboardKeys = new List<BlackboardKey>();

        protected abstract void OnEnter();
        protected abstract void OnExit();
        protected abstract NodeResult OnTick();

        public static T Create<T>(BehaviourTree owner, string title = "") where T : BaseNode
        {
            var node = ScriptableObject.CreateInstance<T>();

            owner?.AddNode(node);
            node.Owner = owner;
            node.name = string.IsNullOrWhiteSpace(title) ? typeof(T).Name : title;
            node.Initialise(); 

            return node;
        }

        public static BaseNode Create(BaseNode node)
        {
            var newNode = CreateInstance(node.GetType()) as BaseNode;
            newNode.name = !string.IsNullOrWhiteSpace(node.name) ? node.name : newNode.GetType().Name;
            newNode.Guid = System.Guid.NewGuid().ToString();

            return newNode;
        }

        public static BaseNode Create(Type type)
        {
            if (!type.IsSubclassOf(typeof(BaseNode)))
            {
                Debug.LogError($"");
                return null;
            }

            var node = (BaseNode)ScriptableObject.CreateInstance(type);
            node.name = type.Name;
            node.Initialise();

            return node;
        }

        public void Initialise()
        {
            Guid = System.Guid.NewGuid().ToString();

            if (Owner.Blackboard == null)
                return;

            var bbkFields = GetType().GetFields().Where(field => field.FieldType.IsSubclassOf(typeof(BlackboardKey))).ToList();

            foreach (var field in bbkFields)
            {
                var keyValue = field.GetValue(this) as BlackboardKey;
                if (keyValue == null)
                {
                    // Gross.
                    var fieldType = field.FieldType;
                    if (fieldType == typeof(BlackboardKeyBool))
                    {
                        keyValue = new BlackboardKeyBool();
                        keyValue.KeyType = BlackboardEntryType.Bool;
                    }
                    else if (fieldType == typeof(BlackboardKeyFloat))
                    {
                        keyValue = new BlackboardKeyFloat();
                        keyValue.KeyType = BlackboardEntryType.Float;
                    }
                    else if (fieldType == typeof(BlackboardKeyGameObject))
                    {
                        keyValue = new BlackboardKeyGameObject();
                        keyValue.KeyType = BlackboardEntryType.GameObject;
                    }
                    else if (fieldType == typeof(BlackboardKeyInt))
                    {
                        keyValue = new BlackboardKeyInt();
                        keyValue.KeyType = BlackboardEntryType.Int;
                    }
                    else if (fieldType == typeof(BlackboardKeyString))
                    {
                        keyValue = new BlackboardKeyString();
                        keyValue.KeyType = BlackboardEntryType.String;
                    }
                    else if (fieldType == typeof(BlackboardKeyVector))
                    {
                        keyValue = new BlackboardKeyVector();
                        keyValue.KeyType = BlackboardEntryType.Vector;
                    }

                    field.SetValue(this, keyValue);
                }

                // Add node's blackboard key to collection
                m_BlackboardKeys.Add(keyValue);

                // Find corresponding blackboard entry
                var bbEntry = Owner.Blackboard.Entries.Find(entry => entry.Name == keyValue.Name && entry.ValueType == keyValue.KeyType);

                // Add blackboard key as a listener for on value change updates
                if (bbEntry != null)
                    bbEntry.AddListener(keyValue);
            }
        }

        public NodeResult Tick()
        {
            if (!Active)
            {
                // Mark node as active and execute node's "start-up" behaviour
                Active = true;
                OnEnter();
            }

            // Execute the node's primary behaviour
            State = OnTick();

            if (State != NodeResult.Running)
            {
                // Mark node as inactive and execute node's "clean-up" behaviour
                Active = false;
                OnExit();
            }

            return State;
        }

        public void SetExecutionOrder(int order) => ExecutionOrder = order;
        public void SetParent(BaseNode node) => Parent = node;
        public int Compare(BaseNode a, BaseNode b) => a.ExecutionOrder - b.ExecutionOrder;
        public int CompareTo(BaseNode other) => ExecutionOrder - other.ExecutionOrder;
    }
}