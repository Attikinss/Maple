using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Maple.Editor
{
    public class TreeEditorWindow : EditorWindow
    {
        public static TreeEditorWindow Instance { get; private set; }

        [MenuItem("Tools/Maple AI/Tree Editor")]
        public static void OpenWindow()
        {
            Instance = GetWindow<TreeEditorWindow>("Maple AI Editor");
            Instance.minSize = new Vector2(256, 256);
        }

        private void OnEnable()
        {
            CreateGraphView();
        }

        private void OnDisable()
        {
            
        }

        private void CreateGraphView()
        {
            rootVisualElement.Add(new TreeGraphView());
        }
    }
}
