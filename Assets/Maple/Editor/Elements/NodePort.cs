using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Maple.Editor
{
    public class NodePort : Port
    {
        protected NodePort(Direction direction, Capacity capacity) : base(Orientation.Vertical, direction, capacity, typeof(bool))
        {
            m_EdgeConnector = new EdgeConnector<Edge>(TreeGraphView.Instance.EdgeConnectorListener);
            this.AddManipulator(m_EdgeConnector);
        }

        public static NodePort Create(Direction direction, Capacity capacity)
        {
            return new NodePort(direction, capacity);
        }
    }
}