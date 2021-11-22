using System.Collections.Generic;
using UnityEngine;

namespace Maple.Nodes
{
    public abstract class Composite : BaseNode
    {
        protected List<BaseNode> m_Children = new List<BaseNode>();

        public List<BaseNode> GetChildren() => m_Children;

        public void AddChild(BaseNode node, int executionOrder = -1)
        {
            if (node == null)
            {
                // Notify user that the action failed
                Debug.LogError($"({Owner.Agent.gameObject.name}): Cannot add child to [{Title}] - node is null!");
                return;
            }

            if (m_Children.Contains(node))
            {
                // Notify user that the action failed
                Debug.LogError($"({Owner.Agent.gameObject.name}): Cannot add child to [{Title}] - node is already a child!");
                return;
            }

            // Link this node and the new child node
            if (executionOrder >= 0)
            {
                // Clamp specified execution order
                int nodePlacement = Mathf.Clamp(executionOrder, 0, m_Children.Count - 1);

                // Insert node relative to the specified execution priority
                m_Children.Insert(nodePlacement, node);

                // Update execution orders of all nodes after node being added
                for (int i = nodePlacement + 1; i < m_Children.Count; i++)
                    m_Children[i].ExecutionOrder++;
            }
            else
            {
                // Set execution order to last
                // (the -1 is used to allow code to balance out both cases later with the +1)
                executionOrder = m_Children.Count - 1;

                // Place node at end
                m_Children.Add(node);
            }

            node.SetParent(this);
            node.SetExecutionOrder(executionOrder + 1);
        }

        public void AddChildren(params BaseNode[] nodes)
        {
            foreach (var node in nodes)
                AddChild(node);
        }

        public void RemoveChild(BaseNode node)
        {
            if (node == null)
            {
                // Notify user that the action failed
                Debug.LogError($"({Owner.Agent.gameObject.name}): Cannot remove child from [{Title}] - node is null!");
                return;
            }

            if (!m_Children.Contains(node))
            {
                // Notify user that the action failed
                Debug.LogError($"({Owner.Agent.gameObject.name}): Cannot remove child from [{Title}] - node isn't a child!");
                return;
            }

            // Get index of the node being removed
            int index = m_Children.IndexOf(node);

            // Update execution orders of all nodes after node being removed
            for (int i = index; i < m_Children.Count; i++)
                m_Children[i].ExecutionOrder--;

            // Disconnect nodes
            m_Children.Remove(node);
            node.SetParent(null);
        }

        public void RemoveChildren(params BaseNode[] nodes)
        {
            foreach (var node in nodes)
                RemoveChild(node);
        }

        public void RemoveChildren(params string[] guids)
        {
            foreach (var guid in guids)
            {
                // Find node in children with matching guid
                var node = m_Children.Find(itr => itr.Guid == guid);

                RemoveChild(node);
            }
        }
    }
}