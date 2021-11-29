using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Maple.Editor
{
    public class TreeEditorWindow : EditorWindow
    {
        public static TreeEditorWindow Instance { get; private set; }

        private ToolbarMenu m_FileMenu;
        private ToolbarMenu m_CurrentTreeField;
        private List<BehaviourTree> m_AvailableTrees;

        [MenuItem("Tools/Maple AI/Tree Editor")]
        public static void OpenWindow()
        {
            Instance = GetWindow<TreeEditorWindow>("Maple AI Editor");
        }

        private void OnEnable()
        {
            Instance = this;

            // Import UXML
            var visualTree = Resources.Load<VisualTreeAsset>("UI Documents/AITreeEditor");
            visualTree.CloneTree(rootVisualElement);

            // Import style sheet
            rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("Styles/AITreeEditor"));

            Rebuild();
        }

        private void OnDisable()
        {
            rootVisualElement.Clear();

            Instance = null;
        }

        private void Rebuild()
        {
            Instance.minSize = new Vector2(400, 360);

            CreateGraphView();
            CreateToolbar();
            CreatePropertiesPanel();
        }

        private void CreateToolbar()
        {
            m_FileMenu = rootVisualElement.Q<ToolbarMenu>("FileMenu");

            // Clear graph
            m_FileMenu.menu.AppendAction("New", ctx => {  });
            
            // Save the current tree as a new behaviour tree asset
            m_FileMenu.menu.AppendAction("Save As New", ctx => {  });

            m_CurrentTreeField = rootVisualElement.Q<ToolbarMenu>("TreeSelectionField");
            m_CurrentTreeField.RegisterCallback<ClickEvent>(evt =>
            {
                UpdateAvailableTrees();
                m_CurrentTreeField.ShowMenu();
            });
        }

        private void CreatePropertiesPanel()
        {
            // Query menu toggle buttons
            ToolbarToggle inspector = rootVisualElement.Q<ToolbarToggle>("InspectorToggle");
            ToolbarToggle blackboard = rootVisualElement.Q<ToolbarToggle>("BlackboardToggle");

            if (blackboard != null)
            {
                // Ensure when blackboard toggle is clicked all other toggles are disabled
                blackboard.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                        inspector.value = false;
                    else if (!inspector.value)
                        blackboard.SetValueWithoutNotify(true);
                });
            }

            if (inspector != null)
            {
                // Ensure when inspector toggle is clicked all other toggles are disabled
                inspector.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                        blackboard.value = false;
                    else if (!blackboard.value)
                        inspector.SetValueWithoutNotify(true);
                });

                // Make inspector active by default
                inspector.value = true;
            }
        }

        private void CreateGraphView()
        {
            var graphView = rootVisualElement.Q<TreeGraphView>();
            if (graphView != null)
                graphView.Construct();
        }

        private void UpdateAvailableTrees()
        {
            // Clear tree menu
            m_CurrentTreeField.menu.MenuItems().Clear();

            // Add tree assets to menu list
            foreach (var tree in FindAllTrees())
                m_CurrentTreeField.menu.AppendAction(tree.name, ctx => TreeGraphView.Instance.LoadTree(tree));

            // Get runtime trees
            var runtimeTrees = FindAllTrees(true);
            if (runtimeTrees.Count > 0)
            {
                // Add a separator
                m_CurrentTreeField.menu.AppendSeparator();

                // Add runtime trees to menu list
                foreach (var tree in runtimeTrees)
                    m_CurrentTreeField.menu.AppendAction(tree.name, ctx => TreeGraphView.Instance.LoadTree(tree));
            }
        }

        private List<BehaviourTree> FindAllTrees(bool runtime = false)
        {
            // Initialise empty list
            List<BehaviourTree> assets = new List<BehaviourTree>();

            if (!runtime)
            {
                // Find assets in the project panel using editor based searching
                assets = Utilities.Utilities.FindAssetsOfType<BehaviourTree>();
            }
            else if (EditorApplication.isPlaying)
            {
                // Find all scripts/components of type or subclass type of agent
                // This assumes runtime trees will only be in the agent class or
                // classes that inherit from agent.
                List<Object> agents = FindObjectsOfType(typeof(Agent)).ToList();
                agents.ForEach(item =>
                {
                    var treeProperties = item.GetType().GetProperties().ToList();

                    // Attempt to find matching properties in each script of type "BehaviourTree" and called "RuntimeTree"
                    // This would need to be more dynamic as the user might want to override the variable type (field, property, etc)
                    // and the name which would break this detection system entirely.
                    var matches = treeProperties.Find(field => field.PropertyType == typeof(BehaviourTree) && field.Name == "RuntimeTree");

                    if (matches != null)
                    {
                        // If property is found, get the value and add it to the list
                        var tree = matches.GetValue(item);
                        assets.Add(tree as BehaviourTree);
                    }
                });
            }

            return assets;
        }
    }
}
