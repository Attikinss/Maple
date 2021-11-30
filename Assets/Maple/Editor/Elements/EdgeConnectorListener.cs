using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Maple.Editor
{
    public class EdgeConnectorListener : IEdgeConnectorListener
    {
        /// <summary>Collection of changes in the graph view.</summary>
        private GraphViewChange m_GraphViewChange;

        /// <summary>Collection of edges to create in the graph view.</summary>
        private List<Edge> m_EdgesToCreate;

        /// <summary>Collection of edges to delete from the graph view.</summary>
        private List<GraphElement> m_EdgesToDelete;

        /// <summary>Provider for the pop up node search window.</summary>
        private SearchWindowProvider m_SearchWindowProvider;

        /// <summary>Edge connector listener constructor.</summary>
        /// <param name="searchWindowProvider">The search window provider for the pop up search window.</param>
        public EdgeConnectorListener(SearchWindowProvider searchWindowProvider)
        {
            m_SearchWindowProvider = searchWindowProvider;

            m_EdgesToCreate = new List<Edge>();
            m_EdgesToDelete = new List<GraphElement>();

            m_GraphViewChange.edgesToCreate = m_EdgesToCreate;
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            // Reset the create list
            m_EdgesToCreate.Clear();
            m_EdgesToCreate.Add(edge);

            // Clear the delete list
            m_EdgesToDelete.Clear();

            // If the edge's input port is single capacity, delete all other
            // edges connected to the node before the new edge can connect
            if (edge.input != null && edge.input.capacity == Port.Capacity.Single)
            {
                // Iterate through all node connections of the edge
                foreach (var edgeToDelete in edge.input.connections)
                {
                    // Add any disconnected edges to the delete list
                    if (edgeToDelete != edge)
                        m_EdgesToDelete.Add(edgeToDelete);
                }
            }

            // If the edge's output port is single capacity, delete all other
            // edges connected to the node before the new edge can connect
            if (edge.output != null && edge.output.capacity == Port.Capacity.Single)
            {
                // Iterate through all node connections of the edge
                foreach (var edgeToDelete in edge.output.connections)
                {
                    // Add any disconnected edges to the delete list
                    if (edgeToDelete != edge)
                        m_EdgesToDelete.Add(edgeToDelete);
                }
            }

            // Delete any newly disconnected edges
            if (m_EdgesToDelete.Count > 0)
                graphView.DeleteElements(m_EdgesToDelete);

            // Create new list so we're not modifying the original list
            var edgesToCreate = m_EdgesToCreate;

            // Use the graphView's changes if they exist
            if (graphView.graphViewChanged != null)
                edgesToCreate = graphView.graphViewChanged(m_GraphViewChange).edgesToCreate;

            foreach (var newEdge in edgesToCreate)
            {
                var parent = newEdge.output?.node as GraphNode;
                var child = newEdge.input?.node as GraphNode;

                if (parent == null || child == null)
                    continue;

                // Convoluted way of checking if the nodes are already connected
                if (child.OutputPort != null && child.OutputPort.connections.FirstOrDefault(e => e.input.node == parent) != null ||
                    parent.InputPort != null && parent.InputPort.connections.FirstOrDefault(e => e.output.node == child) != null)
                    continue;

                // Add new edge to the graph
                graphView.AddElement(newEdge);

                // Connect the edges input port to the new edge
                edge.input.Connect(newEdge);
                
                // Connect the edges outut port to the new edge
                edge.output.Connect(newEdge);
            }
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            // Get the port from with the edge was dragged from
            var draggedPort = (edge.output != null ? edge.output.edgeConnector.edgeDragHelper.draggedPort : null) ??
                              (edge.input != null ? edge.input.edgeConnector.edgeDragHelper.draggedPort : null);

            // Set the connected port of the search window provider so that
            // it can auto connect the from node to any newly created nodes
            m_SearchWindowProvider.ConnectedPort = (draggedPort as NodePort);

            // Open the node creation/search window
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), m_SearchWindowProvider);
        }
    }
}
