using Maple.Nodes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Maple
{
    public class BehaviourTree : ScriptableObject
    {
        public Agent Agent { get; private set; }
        public List<BaseNode> Nodes = new List<BaseNode>();

        public Root Root;

        public bool Stopped { get; private set; } = true;
        public bool Paused { get; private set; } = false;

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

        public BehaviourTree Clone(string name)
        {
            // Shallow copy the tree
            BehaviourTree clone = ScriptableObject.Instantiate(this);
            clone.name = $"[{name}] Behaviour Tree";

            // Clear the nodes linked to the original tree and its nodes
            clone.Nodes.Clear();

            // Add shallow copies of original nodes
            Nodes.ForEach(node => clone.Nodes.Add(ScriptableObject.Instantiate(node)));

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
                    clone.Root = root;

                    // Find the cloned child of this node
                    var childNode = clone.Nodes.Find(itr => itr.Guid == root.GetChild().Guid);

                    // Connect the cloned nodes
                    root.ClearChild();
                    root.SetChild(childNode);
                }

                // Attempt node cast as a composite
                var composite = node as Composite;
                if (composite != null)
                {
                    // Find the cloned children of this node
                    var children = clone.Nodes.Where(itr => composite.GetChildren().Any(child => itr.Guid == child.Guid)).ToArray();

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

        public void SetAgent(Agent agent)
        {
            if (Agent != null)
            {
                // Transfer tree to another agent
                TransferTo(agent);
                return;
            }

            agent.AttachTree(this);
            Agent = agent;
        }

        public void ClearAgent()
        {
            Agent?.DetachTree();
            Agent = null;
        }

        public void SetRoot(Root root)
        {
            // TODO: Find a way to disconnect previous root and store
            //       it somewhere so that it can be resused elsewhere

            if (Root == null)
            {
                Root = root;
                Root.Owner = this;
            }
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
    }
}
