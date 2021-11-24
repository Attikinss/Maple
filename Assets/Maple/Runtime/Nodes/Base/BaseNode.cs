using Maple.Blackboards;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Maple.Nodes
{
    public abstract class BaseNode : ScriptableObject, IComparer<BaseNode>, IComparable<BaseNode>
    {
        /// <summary>Used to debug nodes at runtime and display their names in the graph tool.</summary>
        public string Title;

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
        /// The saved dimensions and position of the node when re-loaded and displayed in the graph tool.
        /// <br>TODO: Move this somewhere else</br>
        /// </summary>
        public Rect GraphDimensions;

        /// <summary>The behaviour tree in which the node belongs to.</summary>
        public BehaviourTree Owner;

        public virtual string IconPath { get; } = "Icons/User";

        private List<BlackboardKey> m_BlackboardKeys = new List<BlackboardKey>();

        protected abstract void OnEnter();
        protected abstract void OnExit();
        protected abstract NodeResult OnTick();

        public static BaseNode Create<T>() where T : BaseNode
        {
            var node = ScriptableObject.CreateInstance<T>();
            node.Initialise();

            return node;
        }

        private void Initialise()
        {
            if (Owner.Blackboard == null)
                return;

            var bbkFields = GetType().GetFields().Where(field => field.FieldType.IsSubclassOf(typeof(BlackboardKey))).ToList();

            foreach (var field in bbkFields)
            {
                var keyValue = field.GetValue(this) as BlackboardKey;
                if (keyValue != null)
                {
                    // Add node's blackboard key to collection
                    m_BlackboardKeys.Add(keyValue);

                    // Find corresponding blackboard entry
                    var bbEntry = Owner.Blackboard.Entries.Find(entry => entry.Name == keyValue.Name && entry.ValueType == keyValue.KeyType);

                    // Add blackboard key as a listener for on value change updates
                    if (bbEntry != null)
                        bbEntry.AddListener(keyValue);
                }
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