using Maple.Blackboards;
using Maple.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Maple
{
    [System.Serializable]
    public class BehaviourTree : ScriptableObject
    {
        public Agent Agent { get; private set; }

        [SerializeField]
        private List<BaseNode> m_Nodes = new List<BaseNode>();
        
        [SerializeField]
        private Root m_Root;

        [SerializeField]
        private Blackboard m_Blackboard;

        public List<BaseNode> Nodes { get => m_Nodes; }
        public Root Root { get => m_Root; }
        public Blackboard Blackboard { get => m_Blackboard; }

        public bool Stopped { get; private set; } = true;
        public bool Paused { get; private set; } = false;

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Maple AI/Create/Behaviour Tree")]
        public static BehaviourTree CreateAsset()
        {
            // Create a default/empty tree instance
            var tree = Create("New BehaviourTree", null);

            Utilities.Utilities.CreateAssetFromItem(tree);
            UnityEditor.AssetDatabase.AddObjectToAsset(tree.Root, tree);
            UnityEditor.AssetDatabase.SaveAssets();

            return tree;
        }
#endif

        public static BehaviourTree Create(string name, Agent owner, Root root = null)
        {
            var behaviourTree = ScriptableObject.CreateInstance<BehaviourTree>();
            behaviourTree.name = name;
            behaviourTree.SetAgent(owner);

            if (root != null)
                behaviourTree.m_Root = root;
            else
                behaviourTree.m_Root = BaseNode.Create<Root>(behaviourTree);

            return behaviourTree;
        }

        public void Start() => Stopped = false;
        public void Stop() => Stopped = false;
        public void Pause() => Paused = true;
        public void Resume() => Paused = false;

        public void Tick()
        {
            if (Root == null)
            {
                Debug.LogWarning($"({this.name}): Cannot run tree - root node is null!");
                return;
            }

            if (!Stopped && !Paused)
                Root.Tick();
        }

        public BehaviourTree Clone(string name, Agent owner, bool toDisk = false)
        {
            // Shallow copy the tree
            BehaviourTree clone = ScriptableObject.Instantiate(this);
            clone.name = name;
            clone.SetAgent(owner);

            // Clear the nodes linked to the original tree and its nodes
            clone.ClearNodes();

            if (toDisk)
            {
                // Save the tree to disk
                Utilities.Utilities.CreateAssetFromItem(clone);
            }

            // Add shallow copies of original nodes
            Nodes.ForEach(node =>
            {
                var newNode = ScriptableObject.Instantiate(node);

                // Ensures (Clone) isn't in the name
                newNode.name = node.name;
                
                clone.AddNode(newNode);
            });

            // Deep copy all nodes
            clone.Nodes.ForEach(node =>
            {
                // Assign node owner to cloned tree
                node.Owner = clone;

                // Attempt node cast as a root
                var root = node as Root;
                if (root != null)
                {
                    // Set the root node of the cloned tree
                    clone.m_Root = root;

                    root.ClearChild();

                    // Find the cloned child of this node
                    if (m_Root.GetChild())
                    {
                        var childNode = clone.Nodes.Find(itr => itr.Guid == m_Root.GetChild().Guid);

                        // Connect the cloned nodes
                        root.SetChild(childNode);
                    }
                }

                // Attempt node cast as a composite
                var composite = node as Composite;
                if (composite != null)
                {
                    // Find the cloned children of this node
                    var match = Nodes.Find(n => n.Guid == composite.Guid) as Composite;
                    var children = clone.Nodes.Where(itr => match.GetChildren().Any(child => child.Guid == itr.Guid)).ToArray();

                    // Connect the cloned nodes
                    composite.GetChildren().Clear();
                    composite.AddChildren(children);
                }

                // Attempt node cast as a decorator
                var decorator = node as Decorator;
                if (decorator != null)
                {
                    // Find the cloned child of this node
                    var childNode = clone.Nodes.Find(itr => itr.Guid == decorator.GetChild().Guid);

                    // Connect the cloned nodes
                    decorator.ClearChild();
                    decorator.SetChild(childNode);
                }
            });

            return clone;
        }

        public void SetBlackboard(Blackboard blackboard)
        {
            if (m_Blackboard != blackboard)
                m_Blackboard = blackboard;
        }

        public void SetAgent(Agent agent)
        {
            if (Agent != null)
            {
                // Transfer tree to another agent
                TransferTo(agent);
                return;
            }

            agent?.AttachTree(this);
            Agent = agent;
        }

        public void ClearAgent()
        {
            Agent?.DetachTree();
            Agent = null;
        }

        public void TransferTo(Agent agent)
        {
            if (agent == null)
            {
                Debug.LogWarning("Transfer aborted: target agent was null!");
                return;
            }

            if (Agent == agent)
            {
                Debug.LogWarning($"Transfer aborted: tree already attached to this agent! ({Agent.gameObject.name})");
                return;
            }

            Agent?.DetachTree();
            Agent = agent;
            Agent.AttachTree(this);
        }

        public void ClearNodes()
        {
            for (int i = m_Nodes.Count - 1; i >= 0; i--)
                RemoveNode(m_Nodes[i]);

            m_Nodes.Clear();
        }

        public void AddNode(BaseNode node)
        {
            if (node == null || m_Nodes.Contains(node))
                return;

#if UNITY_EDITOR
            bool isAsset = UnityEditor.AssetDatabase.GetAssetPath(this).Length > 0;
            if (isAsset)
                UnityEditor.Undo.RecordObject(this, "(Behaviour Tree): Node added");
#endif

            node.Owner = this;
            m_Nodes.Add(node);
            
#if UNITY_EDITOR
            if (isAsset)
            {
                UnityEditor.AssetDatabase.AddObjectToAsset(node, this);
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.SaveAssets();
            }
#endif
        }

        public void RemoveNode(BaseNode node)
        {
            if (!m_Nodes.Contains(node))
                return;

#if UNITY_EDITOR
            bool isAsset = UnityEditor.AssetDatabase.GetAssetPath(this).Length > 0;
            if (isAsset)
                UnityEditor.Undo.RecordObject(this, "(Behaviour Tree): Node removed");
#endif

            node.Owner = null;
            m_Nodes.Remove(node);

#if UNITY_EDITOR
            if (isAsset)
            {
                UnityEditor.AssetDatabase.RemoveObjectFromAsset(node);
                UnityEditor.AssetDatabase.SaveAssets();
            }
#endif
        }
    }
}
