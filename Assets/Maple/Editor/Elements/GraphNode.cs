using Maple.Nodes;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Maple.Editor
{
    public class GraphNode : Node
    {
        public GraphNode Parent { get; private set; }
        public BaseNode RuntimeNode { get => m_RuntimeNode; }
        public NodePort InputPort { get; private set; }
        public NodePort OutputPort { get; private set; }

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

        public override void OnSelected()
        {
            base.OnSelected();
            TreeEditorWindow.Instance?.OnSelection(this);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            m_RuntimeNode.Position.x = newPos.xMin;
            m_RuntimeNode.Position.y = newPos.yMin;
        }

        public bool IsMouseOver(Vector2 mousePos)
        {
            var position = GetPosition();
            return (mousePos.x > position.xMin && mousePos.y > position.yMin &&
                mousePos.x < position.xMax && mousePos.y < position.yMax);
        }

        public void SetRuntimeNode(BaseNode node)
        {
            if (node == null)
                return;

            m_RuntimeNode = node;
            SetPosition(new Rect(node.Position, Vector2.zero));
            viewDataKey = node.Guid;
            name = node.GetType().Name.ToString();
            title = name;
        }

        private void CreatePorts()
        {
            if (m_RuntimeNode is Root)
            {
                // No parent, single children
                InputPort = null;
                OutputPort = NodePort.Create(Direction.Output, Port.Capacity.Single);
            }
            else if (m_RuntimeNode is Composite)
            {
                // Single parent, multiple children
                InputPort = NodePort.Create(Direction.Input, Port.Capacity.Single);
                OutputPort = NodePort.Create(Direction.Output, Port.Capacity.Multi);
            }
            else if (m_RuntimeNode is Task)
            {
                // Single parent, no children
                InputPort = NodePort.Create(Direction.Input, Port.Capacity.Single);
                OutputPort = null;
            }

            if (InputPort != null)
            {
                InputPort.portName = "";
                inputContainer.Add(InputPort);
            }

            if (OutputPort != null)
            {
                OutputPort.portName = "";
                outputContainer.Add(OutputPort);
            }

            // TODO: Figure out how to handle decorators
        }

        public List<Port> GetPorts()
        {
            return new List<Port>() { InputPort, OutputPort };
        }
    }
}
