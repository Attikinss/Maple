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

        public GraphNode Root { get => m_Root; }

        public SearchWindowProvider SearchWindowProvider { get; private set; }

        public EdgeConnectorListener EdgeConnectorListener { get; private set; }

        private BehaviourTree m_CurrentTree;

        private TextField m_TreeNameField;

        private GraphNode m_Root;

        public void Construct()
        {
            // Set callback for key down events
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<MouseDownEvent>(OnMouseDown);

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

            // Add function to handle when changes are made on the graph
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

            // Create and add a default root node
            m_Root = GraphNode.Construct(Nodes.BaseNode.Create<Nodes.Root>(null),
                AssetDatabase.GetAssetPath(Resources.Load<VisualTreeAsset>("UI Documents/GraphNode")));
            AddElement(m_Root);

            CreateElements();
        }

        public void CreateElements()
        {
            CreateTreeNameField();
        }

        public void LoadTree(BehaviourTree tree)
        {
            // Don't do anything
            if (tree == null || m_CurrentTree == tree)
                return;

            // Prepare graph for new tree
            ClearGraph();
            m_CurrentTree = tree;
            m_TreeNameField.SetValueWithoutNotify(tree.name);

            // Create graph nodes from tree
            m_CurrentTree.Nodes.ForEach(node =>
            {
                var newNode = GraphNode.Construct(node, AssetDatabase.GetAssetPath(Resources.Load<VisualTreeAsset>("UI Documents/GraphNode")));
                AddElement(newNode);
            });

            // Connect graph nodes
            nodes.ForEach(node =>
            {
                // Cast the current node
                var graphNode = node as GraphNode;

                // Try to cast its runtime node
                var root = graphNode.RuntimeNode as Nodes.Root;
                var composite = graphNode.RuntimeNode as Nodes.Composite;
                var task = graphNode.RuntimeNode as Nodes.Task;

                if (root != null)
                {
                    m_Root = graphNode;

                    // Try to find the root's child, connect it, and add it to the graph
                    var child = nodes.FirstOrDefault(n => (n as GraphNode).RuntimeNode == root.GetChild());
                    if (child != null)
                        AddElement(graphNode.OutputPort.ConnectTo((child as GraphNode).InputPort));
                }
                else if (composite != null)
                {
                    // Try to find the composite's children, connect them, and add them to the graph
                    var children = nodes.Where(n => composite.GetChildren().Any(child => child == (n as GraphNode).RuntimeNode)).ToList();
                    foreach (var child in children)
                        AddElement(graphNode.OutputPort.ConnectTo((child as GraphNode).InputPort));
                }
            });
        }

        public void NewTree()
        {
            // Create a blank behaviour tree asset
            // on disk and immediately load it
            LoadTree(BehaviourTree.CreateAsset());
        }

        public void SaveGraphAsNew()
        {
            BehaviourTree tree = null;

            if (CurrentTree)
            {
                // Clone the current tree if one is active
                tree = CurrentTree.Clone(m_TreeNameField.value, null, true);
            }
            else
            {
                // Create a tree from the nodes currently on the graph
                tree = BehaviourTree.Create(m_TreeNameField.value, null, m_Root.RuntimeNode as Nodes.Root);

                // Save the tree to disk
                Maple.Utilities.Utilities.CreateAssetFromItem(tree);

                // Add all of the created/cloned nodes to the new tree's node list
                nodes.ForEach(node =>
                {
                    // Skip node addition as it automatically
                    // gets added to the tree on creation
                    tree.AddNode((node as GraphNode).RuntimeNode);
                });
            }

            // Reload the newly created/cloned tree (not necessary
            // but it's probably easier to just not question it)
            LoadTree(tree);
        }

        public void ClearGraph(bool removeFromTree = false)
        {
            
            if (removeFromTree)
            {
                // Delete nodes from the selected tree
                DeleteElements(graphElements);
            }
            else
            {
                // Prevent nodes from being removed from current tree
                graphViewChanged -= OnGraphViewChange;
                
                // Make root node deletable so that it can be replaced with another root
                if (m_Root != null)
                    m_Root.capabilities = m_Root.capabilities | Capabilities.Deletable;
                
                // Delete all nodes (only from the graph)
                DeleteElements(graphElements);

                // Null out root node for safety
                m_Root = null;

                // Reenable custom graph change detections
                graphViewChanged += OnGraphViewChange;
            }
        }

        public void OnKeyDown(KeyDownEvent evt)
        {
            // Select all with 'Ctrl + A'
            if (evt.ctrlKey && evt.keyCode == KeyCode.A)
                SelectAll();
            // Clear selection with 'Esc'
            else if (!evt.shiftKey && !evt.altKey && !evt.ctrlKey && !evt.commandKey && evt.keyCode == KeyCode.Escape)
                ClearSelection();
            // Focus on elements in the graoh using 'F'
            else if (evt.keyCode == KeyCode.F && !evt.shiftKey && !evt.altKey && !evt.ctrlKey && !evt.commandKey)
            {
                // Focus on selected elements
                if (selection.Count > 0)
                    FrameSelection();
                // Focus on all elements
                else
                    FrameAll();
            }
        }

        public void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                // Convert mouse position
                Vector3 screenMousePosition = evt.localMousePosition;
                Vector2 worldMousePosition = screenMousePosition - contentViewContainer.transform.position;
                worldMousePosition *= 1 / contentViewContainer.transform.scale.x;

                // Check if mouse is over node and update node inspector
                bool mouseOverNode = nodes.Any(node => (node as GraphNode).IsMouseOver(worldMousePosition));
                if (!mouseOverNode)
                    TreeEditorWindow.Instance.Inspector.UpdateSelection(null);
            }
        }

        /// <summary>Selects all elements in the graph.</summary>
        private void SelectAll()
        {
            // Add all elements in the graph to the selection
            foreach (var element in graphElements.ToList())
                AddToSelection(element);
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
                        // Try to cast the edge's nodes
                        GraphNode parent = edge.output.node as GraphNode;
                        GraphNode child = edge.input.node as GraphNode;

                        // Remove child from the node based on its type
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

                    // Try to cast the edge's nodes
                    GraphNode parent = edge.output.node as GraphNode;
                    GraphNode child = edge.input.node as GraphNode;

                    // Convoluted way of checking if the nodes are already connected
                    if (child.OutputPort != null && child.OutputPort.connections.FirstOrDefault(e => e.input.node == parent) != null ||
                        parent.InputPort != null && parent.InputPort.connections.FirstOrDefault(e => e.output.node == child) != null)
                        continue;

                    // Add child to the node based on its type
                    var root = parent.RuntimeNode as Nodes.Root;
                    root?.SetChild(child.RuntimeNode);

                    var composite = parent.RuntimeNode as Nodes.Composite;
                    composite?.AddChild(child.RuntimeNode);
                }
            }

            if (change.movedElements != null)
            {
                foreach (var element in change.movedElements)
                {
                    var movedNode = (element as GraphNode)?.RuntimeNode;
                    
                    // Find the parent of this moved node and sort its children (i.e moved node and its sublings)
                    var parent = nodes.ToList().Find(n => movedNode.Parent != null && n.viewDataKey == movedNode.Parent.Guid) as GraphNode;
                    parent?.SortChildren();
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

        public void UpdateNodeStates()
        {
            if (!m_CurrentTree)
                return;

            // Update state of all nodes on the graph so long as a tree is selected
            foreach (var node in nodes)
                (node as GraphNode)?.UpdateState();
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
