namespace Maple.Nodes
{
    public sealed class Invertor : Decorator
    {
        protected override void OnEnter() { }
        protected override void OnExit() { }

        protected override NodeResult OnTick()
        {
            // If there is no child node, this node always fails
            if (!m_Child) return NodeResult.Failure;

            NodeResult result = m_Child.Tick();

            // Invert success
            if (result == NodeResult.Success)
                return NodeResult.Failure;

            // Invert failure
            if (result == NodeResult.Failure)
                return NodeResult.Success;

            return result;
        }
    }
}
