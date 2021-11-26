namespace Maple.Nodes
{
    public sealed class Selector : Composite
    {
        private BaseNode m_RunningNode;
        private int m_RunningNodeIndex = 0;

        protected override void OnEnter() { }

        protected override void OnExit()
        {
            m_RunningNode = null;
            m_RunningNodeIndex = 0;
        }

        protected override NodeResult OnTick()
        {
            // Default behaviour is failure
            NodeResult result = NodeResult.Failure;

            if (m_Children.Count > 0)
            {
                // Check if there is a running node
                if (m_RunningNode)
                {
                    // Tick running node
                    result = m_RunningNode.Tick();

                    // Clear running node if it's no longer running
                    if (result == NodeResult.Failure)
                    {
                        m_RunningNode = null;

                        // This node will run again unless there are no child nodes left to tick
                        return (++m_RunningNodeIndex < m_Children.Count) ? NodeResult.Running : NodeResult.Failure;
                    }
                }
                else
                {
                    // Ensure index is not out of bounds
                    if (m_RunningNodeIndex < m_Children.Count)
                    {
                        // Tick the next node
                        result = m_Children[m_RunningNodeIndex].Tick();

                        if (result == NodeResult.Running)
                        {
                            // If node is still running set it as the running node
                            m_RunningNode = m_Children[m_RunningNodeIndex];
                        }
                        else if (result == NodeResult.Failure)
                        {
                            // This node will run again unless there are no child nodes left to tick
                            return (++m_RunningNodeIndex < m_Children.Count) ? NodeResult.Running : NodeResult.Failure;
                        }
                    }
                }
            }

            return result;
        }
    }
}