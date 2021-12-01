using Maple.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Maple.Editor
{
    /// <summary>A container of information to display entries in a search window.</summary>
    public struct NodeEntry
    {
        /// <summary>The display name of the node entry in the search window.</summary>
        public string[] Category;

        /// <summary>The node that will be created on the graph.</summary>
        public GraphNode Node;
        
        /// <summary>The name/ID of which port(s) of the displayed nodes can be connected to.</summary>
        public string CompatiblePortID;
    }

    public class SearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        /// <summary>The port that will connect to the port of any nodes created via the search window.</summary>
        public Port ConnectedPort { get; set; }

        /// <summary>Used to emulate indented items in the search window.</summary>
        private Texture2D m_Icon;

        /// <summary>Temporary list containing ports to connect to of a given node.</summary>
        private List<Port> m_Ports = new List<Port>();

        /// <summary>Sets up the connections between the provider, the editor window and graph view.</summary>
        public void Initialise()
        {
            // Transparent icon to emulate indentation of items in the search window
            // This came from source code so if something better comes up, switch this out
            m_Icon = new Texture2D(1, 1);
            m_Icon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            m_Icon.Apply();
        }

        private void OnDestroy()
        {
            if (m_Icon != null)
            {
                DestroyImmediate(m_Icon);
                m_Icon = null;
            }
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            // First build up temporary data structure containing group & title as an array of strings (the last one is the actual title) and associated node type.
            var nodeEntries = new List<NodeEntry>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(BaseNode)))
                    {
                        var attrs = type.GetCustomAttributes(typeof(NodeCategoryAttribute), false) as NodeCategoryAttribute[];
                        if (attrs != null && attrs.Length > 0)
                        {
                            var node = (BaseNode)ScriptableObject.CreateInstance(type);
                            var nodeView = GraphNode.Construct(node);
                            AddEntries(nodeView, attrs[0].Category, nodeEntries);
                        }
                    }
                }
            }

            // Sort the entries lexicographically by group then title with the requirement that items always comes before sub-groups in the same group.
            // Example result:
            // - Composite/User
            // - Composite/User/RandomSelector
            // - Composite/User/RandomSequence
            nodeEntries.Sort((entry1, entry2) =>
            {
                for (var i = 0; i < entry1.Category.Length; i++)
                {
                    if (i >= entry2.Category.Length)
                        return 1;
                    var value = entry1.Category[i].CompareTo(entry2.Category[i]);
                    if (value != 0)
                    {
                        // Make sure that leaves go before nodes
                        if (entry1.Category.Length != entry2.Category.Length && (i == entry1.Category.Length - 1 || i == entry2.Category.Length - 1))
                            return entry1.Category.Length < entry2.Category.Length ? -1 : 1;
                        return value;
                    }
                }
                return 0;
            });

            //* Build up the data structure needed by SearchWindow.

            // `groups` contains the current group path we're in.
            var groups = new List<string>();

            // First item in the tree is the title of the window.
            var tree = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
            };

            foreach (var nodeEntry in nodeEntries)
            {
                // `createIndex` represents from where we should add new group entries from the current entry's group path.
                var createIndex = int.MaxValue;

                // Compare the group path of the current entry to the current group path.
                for (var i = 0; i < nodeEntry.Category.Length; i++)
                {
                    var group = nodeEntry.Category[i];
                    if (i >= groups.Count)
                    {
                        // The current group path matches a prefix of the current entry's group path, so we add the
                        // rest of the group path from the currrent entry.
                        createIndex = i;
                        break;
                    }
                    if (groups[i] != group)
                    {
                        // A prefix of the current group path matches a prefix of the current entry's group path,
                        // so we remove everyfrom from the point where it doesn't match anymore, and then add the rest
                        // of the group path from the current entry.
                        groups.RemoveRange(i, groups.Count - i);
                        createIndex = i;
                        break;
                    }
                }

                // Create new group entries as needed.
                // If we don't need to modify the group path, `createIndex` will be `int.MaxValue` and thus the loop won't run.
                for (var i = createIndex; i < nodeEntry.Category.Length; i++)
                {
                    var group = nodeEntry.Category[i];
                    groups.Add(group);
                    tree.Add(new SearchTreeGroupEntry(new GUIContent(group)) { level = i + 1 });
                }

                // Finally, add the actual entry.
                tree.Add(new SearchTreeEntry(new GUIContent(nodeEntry.Node.RuntimeNode.GetType().Name, m_Icon)) { level = nodeEntry.Category.Length + 1, userData = nodeEntry });
            }

            return tree;
        }

        private void AddEntries(GraphNode node, string[] category, List<NodeEntry> nodeEntries)
        {
            if (ConnectedPort == null)
            {
                nodeEntries.Add(new NodeEntry
                {
                    Node = node,
                    Category = category,
                    CompatiblePortID = ""
                });
                return;
            }

            m_Ports.Clear();
            m_Ports = node.GetPorts();

            var incompatiblePorts = m_Ports.Where(port => !PortCompatible(port)).ToList();
            m_Ports.RemoveAll(port => port == null || incompatiblePorts.Contains(port));
            bool hasSingleSlot = m_Ports.Count == 1;

            if (hasSingleSlot && m_Ports.Count == 1)
            {
                nodeEntries.Add(new NodeEntry
                {
                    Node = node,
                    Category = category,
                    CompatiblePortID = m_Ports.First().portName
                });
                return;
            }

            foreach (var port in m_Ports)
            {
                if (port == null)
                    continue;

                var entryTitle = new string[category.Length];
                category.CopyTo(entryTitle, 0);
                //entryTitle[entryTitle.Length - 1] += ": " + port.Description.m_DisplayName;
                nodeEntries.Add(new NodeEntry
                {
                    Category = entryTitle,
                    Node = node,
                    CompatiblePortID = port.portName
                });
            }
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            var editorWindow = TreeEditorWindow.Instance;
            var graphView = TreeGraphView.Instance;

            Vector2 windowMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, context.screenMousePosition - editorWindow.position.position);
            Vector2 graphMousePosition = graphView.contentViewContainer.WorldToLocal(windowMousePosition);

            NodeEntry nodeEntry = (NodeEntry)entry.userData;
            GraphNode newNode = GraphNode.Construct(BaseNode.Create(nodeEntry.Node.RuntimeNode), AssetDatabase.GetAssetPath(Resources.Load<VisualTreeAsset>("UI Documents/GraphNode")));
            newNode.SetPosition(new Rect(graphMousePosition, Vector2.zero));

            if (ConnectedPort != null)
            {
                bool clearConnections = false;
                if (ConnectedPort.capacity == Port.Capacity.Single)
                {
                    clearConnections = true;
                    graphView.DeleteElements(ConnectedPort.connections);
                }

                if (ConnectedPort.direction == Direction.Input)
                {
                    var runtimeNode = (ConnectedPort.node as GraphNode).RuntimeNode;

                    if (clearConnections)
                    {
                        // Get node type of dragged node's parent
                        var parRoot = runtimeNode.Parent as Root;
                        var parComposite = runtimeNode.Parent as Composite;

                        // Remove child of previous connection
                        if (parRoot)
                            parRoot.ClearChild();
                        else if (parComposite)
                            parComposite.RemoveChild(runtimeNode);
                    }

                    // Get node type of new node
                    var root = newNode.RuntimeNode as Root;
                    var composite = newNode.RuntimeNode as Composite;

                    // Add edge to graph
                    graphView.AddElement(ConnectedPort.ConnectTo(newNode.OutputPort));

                    // Add/set child to new connection
                    if (root)
                        root.SetChild(runtimeNode);
                    else if (composite)
                        composite.AddChild(runtimeNode);
                }
                else
                {
                    graphView.AddElement(ConnectedPort.ConnectTo(newNode.InputPort));

                    var root = (ConnectedPort.node as GraphNode).RuntimeNode as Root;
                    var composite = (ConnectedPort.node as GraphNode).RuntimeNode as Composite;

                    if (root)
                        root.SetChild(newNode.RuntimeNode);
                    else if (composite)
                        composite.AddChild(newNode.RuntimeNode);
                }
            }

            graphView.AddNode(newNode);

            return true;
        }

        private bool PortCompatible(Port port)
        {
            if (port == null)
                return false;

            return port.direction != ConnectedPort.direction && port.node != ConnectedPort.node;
        }
    }
}