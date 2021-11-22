using UnityEngine;

namespace Maple.Nodes
{
    public abstract class Decorator : BaseNode
    {
        protected BaseNode m_Child;

        public BaseNode GetChild() => m_Child;

        public void SetChild(BaseNode node)
        {
            if (node == null)
            {
                // Notify user that the action failed
                Debug.LogError($"({Owner.Agent.gameObject.name}): Cannot set child for [{Title}] - node is null!");
                return;
            }

            ClearChild();

            // Link this node and the new child node
            m_Child = node;
            m_Child.SetParent(this);
        }

        public void SetChild(string nodeGuid)
        {
            // Set the new child's parent to this node
            var node = Owner.Nodes.Find(itr => itr.Guid == nodeGuid);

            SetChild(node);
        }

        public void ClearChild()
        {
            // Disconnect existing child node from this
            m_Child?.SetParent(null);

            m_Child = null;
        }
    }
}
