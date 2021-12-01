using System.Collections.Generic;
using UnityEngine;

namespace Maple.Nodes
{
    public abstract class Composite : BaseNode
    {
        protected List<BaseNode> m_Children = new List<BaseNode>();

        public List<BaseNode> GetChildren() => m_Children;

        public void AddChild(BaseNode node)
        {
            if (node == null)
            {
                // Notify user that the action failed
                Debug.LogError($"({Owner.Agent?.gameObject.name}): Cannot add child to [{name}] - node is null!");
                return;
            }

            if (m_Children.Contains(node))
            {
                // Notify user that the action failed
                Debug.LogError($"({Owner.Agent?.gameObject.name}): Cannot add child to [{name}] - node is already a child!");
                return;
            }

            m_Children.Add(node);
            node.SetParent(this);
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
                Debug.LogError($"({Owner.Agent?.gameObject.name}): Cannot remove child from [{name}] - node is null!");
                return;
            }

            if (!m_Children.Contains(node))
            {
                // Notify user that the action failed
                Debug.LogError($"({Owner.Agent?.gameObject.name}): Cannot remove child from [{name}] - node isn't a child!");
                return;
            }

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