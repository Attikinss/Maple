using UnityEngine;

namespace Maple.Nodes
{
    public sealed class Breakpoint : Task
    {
        public static bool BreakpointsEnabled { get; set; } = true;

        protected override void OnEnter() { }
        protected override void OnExit() { }

        protected override NodeResult OnTick()
        {
            if (BreakpointsEnabled)
            {
                Debug.Log($"({Owner.Agent.gameObject.name}): Breakpoint hit!");
                Debug.Break();
                SetSceneFocus(Owner.Agent.gameObject);
            }

            return NodeResult.Success;
        }

        // TODO: Move this function into a global utilities
        private void SetSceneFocus(GameObject obj)
        {
#if UNITY_EDITOR
            // Set scene selection
            UnityEditor.Selection.activeGameObject = obj;

            // Check if any scene views are active
            UnityEditor.SceneView sceneView = null;
            if (UnityEditor.SceneView.sceneViews.Count == 0)
            {
                // Create and add new scene view
                sceneView = UnityEditor.SceneView.CreateWindow<UnityEditor.SceneView>();
                UnityEditor.SceneView.sceneViews.Add(sceneView);
            }
            else
            {
                // Get the first scene view available
                sceneView = (UnityEditor.SceneView)UnityEditor.SceneView.sceneViews[0];
            }
            
            // Focus the scene view (for if the another panel is active over the top of it)
            sceneView.Focus();

            // Smoothly frame the current selection
            sceneView.Frame(new Bounds(UnityEditor.Tools.handlePosition, obj.transform.lossyScale), false);
#endif
        }
    }
}