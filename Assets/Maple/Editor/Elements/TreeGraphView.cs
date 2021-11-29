using System;
using System.Collections.Generic;
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

            m_CurrentTree.Nodes.ForEach(node =>
            {
                var newNode = GraphNode.Construct(node);
                AddElement(newNode);
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
                    {
                        // Delete node
                    }

                    var edge = element as Edge;
                    if (edge != null)
                    {
                        // Delete edge
                    }
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
    }
}
