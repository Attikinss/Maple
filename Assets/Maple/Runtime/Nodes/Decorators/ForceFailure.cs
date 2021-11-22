namespace Maple.Nodes
{
    public sealed class ForceFailure : Decorator
    {
        protected override void OnEnter() { }
        protected override void OnExit() { }

        protected override NodeResult OnTick()
        {
            // If there is no child node, this node always fails
            if (m_Child)
            {
                NodeResult result = m_Child.Tick();

                // The only time failure is not returned is if the node is running or aborted
                if (result == NodeResult.Aborted || result == NodeResult.Running)
                    return result;
            }

            return NodeResult.Failure;
        }
    }
}
