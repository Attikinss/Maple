namespace Maple.Nodes
{
    public sealed class ForceSuccess : Decorator
    {
        protected override void OnEnter() { }
        protected override void OnExit() { }

        protected override NodeResult OnTick()
        {
            // If there is no child node, this node always fails
            if (!m_Child) return NodeResult.Failure;

            NodeResult result = m_Child.Tick();

            // The only time success is not returned is if the node is running or aborted
            if (result == NodeResult.Aborted || result == NodeResult.Running)
                return result;

            return NodeResult.Success;
        }
    }
}
