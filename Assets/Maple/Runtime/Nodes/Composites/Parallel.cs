using System.Collections.Generic;
using System.Linq;

namespace Maple.Nodes
{
    [NodeCategory("Composite")]
    public sealed class Parallel : Composite
    {
        private Dictionary<string, int> m_Results = new Dictionary<string, int>();

        protected override void OnEnter()
        {
            // TODO: Replace the contant adding and clearing
            //       with a more static solution which assumes
            //       node counts won't change during runtime.
            //       Create list once then update on each tick

            foreach (var child in m_Children)
            {
                child.State = NodeResult.Running;
                m_Results.Add(child.Guid, (int)NodeResult.Running);
            }
        }

        protected override void OnExit()
        {
            m_Results.Clear();
        }

        protected override NodeResult OnTick()
        {
            foreach (var child in m_Children)
            {
                // Skip node if already completed
                if (child.State == NodeResult.Success)
                    continue;

                // Get node result
                NodeResult result = child.Tick();

                // Fail the whole node
                if (result == NodeResult.Failure)
                    return result;

                // Update result in collection
                m_Results[child.Guid] = (int)result;
            }

            // If all result values are uniform at this point we can assume
            // they are all "Success", otherwise continue running this node
            if (m_Results.Values.Distinct().Count() == 1)
                return NodeResult.Success;
            else
                return NodeResult.Running;
        }
    }
}