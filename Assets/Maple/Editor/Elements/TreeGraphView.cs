using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Maple.Editor
{
    public class TreeGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<TreeGraphView, GraphView.UxmlTraits> { }

        public static TreeGraphView Instance { get; private set; }

        public BehaviourTree CurrentTree { get => m_CurrentTree; }

        public SearchWindowProvider SearchWindowProvider { get; private set; }

        public EdgeConnectorListener EdgeConnectorListener { get; private set; }

        private BehaviourTree m_CurrentTree;

        private TextField m_TreeNameField;

        public void Construct()
        {
            // Stretch to parent window's size
            this.StretchToParentSize();

            // Add content manipulators for graph interaction
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());

            // Set up default zooming functionality
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // Apply grid styling for grid background
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/GraphView"));

            // Create and add grid to graphview
            var background = new GridBackground();
            background.StretchToParentSize();
            Insert(0, background);

            graphViewChanged += OnGraphViewChange;

            // Initialise the search window provider
            SearchWindowProvider = ScriptableObject.CreateInstance<SearchWindowProvider>();
            SearchWindowProvider.Initialise();

            // Initialse the edge connection listener
            EdgeConnectorListener = new EdgeConnectorListener(SearchWindowProvider);

            // Set up callback for node creation requests
            nodeCreationRequest = ctx =>
            {
                // Ensure the port isn't selected
                SearchWindowProvider.ConnectedPort = null;

                // Open node search window
                SearchWindow.Open(new SearchWindowContext(ctx.screenMousePosition), SearchWindowProvider);
            };

            Instance = this;

            CreateElements();
        }

        public void CreateElements()
        {
            CreateTreeNameField();
        }

        public void LoadTree(BehaviourTree tree)
        {
            if (tree == null)
                return;

            if (m_CurrentTree != tree)
            {
                ClearGraph();
            
                m_CurrentTree = tree;
                m_TreeNameField.SetValueWithoutNotify(tree.name);
            }

            // Create graph nodes from tree
            m_CurrentTree.Nodes.ForEach(node =>
            {
                var newNode = GraphNode.Construct(node);
                AddElement(newNode);
            });

            // Connect graph nodes
            nodes.ForEach(node =>
            {
                var graphNode = node as GraphNode;

                var root = graphNode.RuntimeNode as Nodes.Root;
                var composite = graphNode.RuntimeNode as Nodes.Composite;
                var task = graphNode.RuntimeNode as Nodes.Task;

                if (root != null)
                {
                    // Try to find the root's child
                    var child = nodes.FirstOrDefault(n => (n as GraphNode).RuntimeNode == root.GetChild());
                    if (child != null)
                        AddElement(graphNode.OutputPort.ConnectTo((child as GraphNode).InputPort));
                }
                else if (composite != null)
                {
                    // Try to find the root's child
                    var children = nodes.Where(n => composite.GetChildren().Any(child => child == (n as GraphNode).RuntimeNode)).ToList();
                    foreach (var child in children)
                        AddElement(graphNode.OutputPort.ConnectTo((child as GraphNode).InputPort));
                }
            });
        }

        public void ClearGraph(bool removeFromTree = false)
        {
            if (removeFromTree)
                DeleteElements(graphElements);
            else
            {
                graphViewChanged -= OnGraphViewChange;
                DeleteElements(graphElements);
                graphViewChanged += OnGraphViewChange;
            }
        }

        private void CreateTreeNameField()
        {
            if (!TreeEditorWindow.Instance)
                return;

            // Try getting text field for behaviour tree name
            m_TreeNameField = TreeEditorWindow.Instance.rootVisualElement.Q<TextField>("TreeNameField");

            if (m_TreeNameField != null)
            {
                // Register value change event
                m_TreeNameField.RegisterValueChangedCallback(evt =>
                {
                    // Ensure value isn't empty or just whitespace
                    if (string.IsNullOrWhiteSpace(evt.newValue))
                        return;

                    if (m_CurrentTree != null)
                    {
                        // Ensure values aren't the same
                        if (m_CurrentTree.name != evt.newValue)
                        {
                            m_TreeNameField.SetValueWithoutNotify(evt.newValue);

                            // Update behaviour tree name
                            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(m_CurrentTree), evt.newValue);
                        }
                    }
                });

                // If no tree is selected, default the name
                if (!m_CurrentTree)
                    m_TreeNameField.SetValueWithoutNotify("New BehaviourTree");
            }
        }

        private GraphViewChange OnGraphViewChange(GraphViewChange change)
        {
            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    var node = element as GraphNode;
                    if (node != null)
                        m_CurrentTree?.RemoveNode(node.RuntimeNode);

                    var edge = element as Edge;
                    if (edge != null)
                    {
                        GraphNode parent = edge.output.node as GraphNode;
                        GraphNode child = edge.input.node as GraphNode;

                        var root = parent.RuntimeNode as Nodes.Root;
                        root?.ClearChild();

                        var composite = parent.RuntimeNode as Nodes.Composite;
                        composite?.RemoveChild(child.RuntimeNode);
                    }
                }
            }

            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    // Ensure null references are ignored
                    if (edge.output == null || edge.input == null)
                        continue;

                    GraphNode parent = edge.output.node as GraphNode;
                    GraphNode child = edge.input.node as GraphNode;

                    // Convoluted way of checking if the nodes are already connected
                    if (child.OutputPort != null && child.OutputPort.connections.FirstOrDefault(e => e.input.node == parent) != null ||
                        parent.InputPort != null && parent.InputPort.connections.FirstOrDefault(e => e.output.node == child) != null)
                        continue;

                    var root = parent.RuntimeNode as Nodes.Root;
                    root?.SetChild(child.RuntimeNode);

                    var composite = parent.RuntimeNode as Nodes.Composite;
                    composite?.AddChild(child.RuntimeNode);
                }
            }

            return change;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            // Create list for all found ports
            var compatiblePorts = new List<Port>();

            // Linq based for loop
            ports.ForEach((port) =>
            {
                // Add ports to list if they have nothing in common with the start port
                if (startPort != port && startPort.node != port.node)
                    compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        public void AddNode(GraphNode node)
        {
            m_CurrentTree?.AddNode(node.RuntimeNode);
            AddElement(node);
        }

        public void RemoveNode(GraphNode node)
        {
            m_CurrentTree?.RemoveNode(node.RuntimeNode);
            RemoveElement(node);
        }
    }
}
