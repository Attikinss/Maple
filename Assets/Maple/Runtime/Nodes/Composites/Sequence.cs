namespace Maple.Nodes
{
    [NodeCategory("Composite")]
    public sealed class Sequence : Composite
    {
        private BaseNode m_RunningNode;
        private int m_RunningNodeIndex = 0;

        protected override void OnEnter()
        {
            foreach (var child in m_Children)
                child.State = NodeResult.Inactive;
        }

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
                if (m_RunningNode)
                {
                    // Tick running node
                    result = m_RunningNode.Tick();
                    
                    // Clear running node if it's no longer running
                    if (result != NodeResult.Running)
                    {
                        m_RunningNode = null;

                        // This node will run again unless there are no child nodes left to tick
                        if (m_RunningNodeIndex + 1 < m_Children.Count)
                        {
                            result = NodeResult.Running;
                            m_RunningNodeIndex++;
                        }
                        else
                            result = NodeResult.Success;
                    }
                }
                else
                {
                    // Ensure index is not out of bounds
                    if (m_RunningNodeIndex < m_Children.Count)
                    {
                        // Tick the next node
                        result = m_Children[m_RunningNodeIndex].Tick();

                        switch (result)
                        {
                            case NodeResult.Running:
                                {
                                    // If node is still running set it as the running node
                                    m_RunningNode = m_Children[m_RunningNodeIndex];
                                    break;
                                }

                            case NodeResult.Success:
                                {
                                    // If there are any more child nodes, tick this node
                                    // again, otherwise exit this node with success status
                                    if (m_RunningNodeIndex + 1 < m_Children.Count)
                                    {
                                        result = NodeResult.Running;
                                        m_RunningNodeIndex++;
                                    }
                                    else
                                        result = NodeResult.Success;

                                    break;
                                }

                            default: break;
                        }
                    }
                    else
                    {
                        // Somehow if this is executed, the ticked all child nodes but tried to tick again
                        return NodeResult.Success;
                    }
                }
            }

            return result;
        }
    }
}