using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Maple.Editor
{
    public class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }

        private NodeInspectorView m_NodeInspector;
        private BlackboardInspectorView m_BlackboardInspector;
        private UnityEditor.Editor m_CurrentInspector;

        private ToolbarToggle m_NodeInspectorToggle;
        private ToolbarToggle m_BlackboardToggle;

        private IMGUIContainer m_InspectorContainer;

        private Nodes.BaseNode m_Selection;

        public void Initialise()
        {
            // Destroy node inspector
            Object.DestroyImmediate(m_NodeInspector);

            // Create new inspector with selection (null)
            m_NodeInspector = UnityEditor.Editor.CreateEditor(Nodes.BaseNode.Create<Nodes.Root>(null), typeof(NodeInspectorView)) as NodeInspectorView;
            m_NodeInspector.UpdateTarget(null);

            // Destroy blackboard inspector
            Object.DestroyImmediate(m_BlackboardInspector);

            // Create new inspector with selection (null)
            m_BlackboardInspector = UnityEditor.Editor.CreateEditor(Blackboards.Blackboard.Create(""), typeof(BlackboardInspectorView)) as BlackboardInspectorView;

            // Query menu toggle buttons
            m_NodeInspectorToggle = TreeEditorWindow.Instance?.rootVisualElement.Q<ToolbarToggle>("InspectorToggle");
            m_BlackboardToggle = TreeEditorWindow.Instance?.rootVisualElement.Q<ToolbarToggle>("BlackboardToggle");

            if (m_BlackboardToggle != null)
            {
                // Ensure when blackboard toggle is clicked all other toggles are disabled
                m_BlackboardToggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        m_NodeInspectorToggle.value = false;
                        SelectPanel<BlackboardInspectorView>();
                    }
                    else if (!m_NodeInspectorToggle.value)
                        m_BlackboardToggle.SetValueWithoutNotify(true);
                });
            }

            if (m_NodeInspectorToggle != null)
            {
                // Ensure when inspector toggle is clicked all other toggles are disabled
                m_NodeInspectorToggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        m_BlackboardToggle.value = false;
                        SelectPanel<NodeInspectorView>();
                    }
                    else if (!m_BlackboardToggle.value)
                        m_NodeInspectorToggle.SetValueWithoutNotify(true);
                });

                // Make inspector active by default
                m_NodeInspectorToggle.value = true;
            }
        }

        public void UpdateSelection(GraphNode node)
        {
            m_NodeInspector.UpdateTarget(node?.RuntimeNode);
        }

        public void Update()
        {
            m_NodeInspector?.Repaint();
        }

        public void SelectPanel<T>() where T : UnityEditor.Editor
        {
            if (typeof(T) == typeof(NodeInspectorView))
            {
                if (m_Selection != null)
                {
                    // Update node inspector target
                    m_NodeInspector?.UpdateTarget(m_Selection);
                }

                m_CurrentInspector = m_NodeInspector;
                UpdateInspector(() => m_CurrentInspector?.OnInspectorGUI());
            }
            else if (typeof(T) == typeof(BlackboardInspectorView))
            {
                m_CurrentInspector = m_BlackboardInspector;
                UpdateInspector(() => m_CurrentInspector?.OnInspectorGUI());
            }
            else
            {
                m_CurrentInspector = null;
                UpdateInspector(null);
            }
        }

        private void UpdateInspector(System.Action func)
        {
            if (m_InspectorContainer != null && Contains(m_InspectorContainer))
                Remove(m_InspectorContainer);

            m_InspectorContainer = new IMGUIContainer(func);

            m_InspectorContainer.style.marginTop = 5;
            m_InspectorContainer.style.marginLeft = 5;
            m_InspectorContainer.style.marginRight = 5;
            Add(m_InspectorContainer);
        }
    }
}
