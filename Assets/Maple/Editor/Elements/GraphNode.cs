using Maple.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Maple.Editor
{
    public class GraphNode : Node
    {
        public GraphNode Parent { get; protected set; }
        public BaseNode RuntimeNode { get => m_RuntimeNode; }
        public Port InputPort { get; private set; }
        public Port OutputPort { get; private set; }

        private BaseNode m_RuntimeNode;

        private GraphNode() { }
        private GraphNode(string uiFile) : base(uiFile) { }

        public static GraphNode Construct(BaseNode node, string uiFile = "")
        {
            var newNode = uiFile.Length == 0 ? new GraphNode() : new GraphNode(uiFile);

            var root = node as Root;
            if (root)
            {
                newNode.capabilities = newNode.capabilities & ~Capabilities.Deletable;
                newNode.AddToClassList("root");
            }

            var composite = node as Composite;
            if (composite)
            {
                string className = "";
                if (composite is Parallel)
                    className = "parallel";
                else if (composite is Parallel)
                    className = "selector";
                else if (composite is Parallel)
                    className = "sequence";

                if (!string.IsNullOrWhiteSpace(className))
                    newNode.AddToClassList(className);
            }

            var task = node as Task;
            if (task)
            {
                newNode.AddToClassList("task");
            }

            newNode.viewDataKey = node.Guid;
            newNode.m_RuntimeNode = node;
            newNode.title = !string.IsNullOrWhiteSpace(newNode.m_RuntimeNode.name) ? newNode.m_RuntimeNode.name : newNode.m_RuntimeNode.GetType().Name;
            newNode.name = newNode.title;

            // Get the styling for the node
            //styleSheets.Add(Resources.Load<StyleSheet>("Styles/GraphNode"));

            // Set a default size
            newNode.style.left = newNode.m_RuntimeNode.Position.x;
            newNode.style.top = newNode.m_RuntimeNode.Position.y;

            newNode.CreatePorts();

            return newNode;
        }

        public void Move(Rect position, bool record)
        {
            if (record)
                Undo.RecordObject(m_RuntimeNode, $"{m_RuntimeNode.GetType().Name} (Set Position)");

            SetPosition(position);
            EditorUtility.SetDirty(m_RuntimeNode);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            m_RuntimeNode.Position.x = newPos.xMin;
            m_RuntimeNode.Position.y = newPos.yMin;
        }

        private void CreatePorts()
        {
            if (m_RuntimeNode is Root)
            {
                // No parent, single children
                InputPort = null;
                OutputPort = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));

                OutputPort.name = "";
            }
            else if (m_RuntimeNode is Composite)
            {
                // Single parent, multiple children
                InputPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                OutputPort = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));

                InputPort.name = "";
                OutputPort.name = "";
            }
            else if (m_RuntimeNode is Task)
            {
                // Single parent, no children
                InputPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                OutputPort = null;

                InputPort.name = "";
            }

            // TODO: Figure out how to handle decorators
        }
    }
}
