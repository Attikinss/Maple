using Maple.Blackboards;
using Maple.Nodes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Maple.Editor
{
    public class BlackboardInspectorView : UnityEditor.Editor
    {
        private Blackboard m_Target;
        private bool m_AddEntry = false;

        public override void OnInspectorGUI()
        {
            // Try get and update blackboard object
            if (TreeGraphView.Instance.CurrentTree == null)
            {
                EditorGUILayout.HelpBox("Select a behaviour tree to access blackboards!", MessageType.Warning);
                return;
            }

            // Begin drawing style helpbox to contain elements
            GUILayout.BeginVertical(EditorStyles.helpBox);

            m_Target = TreeGraphView.Instance.CurrentTree.Blackboard;
            m_Target = EditorGUILayout.ObjectField("Blackboard", m_Target, typeof(Blackboard), false) as Blackboard;

            if (m_Target != null)
            {
                // Draw "Entries" label with a unique style
                GUIStyle fontStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
                fontStyle.normal.textColor = Color.white;
                EditorGUILayout.LabelField("Entries", fontStyle);

                // Draw a list element per entry
                if (m_Target.Entries != null && m_Target.Entries.Count > 0)
                {
                    for (int i = 0; i < m_Target.Entries.Count; i++)
                        DrawEntry(m_Target.Entries[i]);
                }

                // Create button for adding more blackboard entries
                if (GUILayout.Button("Add Entry"))
                    m_AddEntry = !m_AddEntry;

                if (m_AddEntry)
                {
                    if (Event.current.keyCode == KeyCode.Escape)
                        m_AddEntry = false;
                    else
                    {
                        BlackboardEntryType entryType = BlackboardEntryType.None;

                        GUILayout.BeginVertical(EditorStyles.helpBox);

                        if (GUILayout.Button($"{BlackboardEntryType.Bool}"))
                            entryType = BlackboardEntryType.Bool;
                        else if (GUILayout.Button($"{BlackboardEntryType.Float}"))
                            entryType = BlackboardEntryType.Float;
                        else if (GUILayout.Button($"{BlackboardEntryType.GameObject}"))
                            entryType = BlackboardEntryType.GameObject;
                        else if (GUILayout.Button($"{BlackboardEntryType.Int}"))
                            entryType = BlackboardEntryType.Int;
                        else if (GUILayout.Button($"{BlackboardEntryType.String}"))
                            entryType = BlackboardEntryType.String;
                        else if (GUILayout.Button($"{BlackboardEntryType.Vector}"))
                            entryType = BlackboardEntryType.Vector;

                        GUILayout.EndVertical();

                        if (entryType != BlackboardEntryType.None)
                        {
                            CreateEntry(entryType);
                            m_AddEntry = false;
                        }
                    }
                }
            }
            
            GUILayout.EndVertical();

            if (Event.current.isMouse)
                m_AddEntry = false;

            TreeGraphView.Instance.CurrentTree.SetBlackboard(m_Target);
        }

        private void CreateEntry(BlackboardEntryType type)
        {
            BlackboardEntry entry = null;

            switch (type)
            {
                case BlackboardEntryType.Bool:
                    entry = BlackboardEntry.Create<bool>("New Entry", false, m_Target);
                    break;

                case BlackboardEntryType.Float:
                    entry = BlackboardEntry.Create<float>("New Entry", 0.0f, m_Target);
                    break;

                case BlackboardEntryType.GameObject:
                    entry = BlackboardEntry.Create<GameObject>("New Entry", null, m_Target);
                    break;

                case BlackboardEntryType.Int:
                    entry = BlackboardEntry.Create<int>("New Entry", 0, m_Target);
                    break;

                case BlackboardEntryType.String:
                    entry = BlackboardEntry.Create<string>("New Entry", "", m_Target);
                    break;

                case BlackboardEntryType.Vector:
                    entry = BlackboardEntry.Create<Vector3>("New Entry", Vector3.zero, m_Target);
                    break;

                default:
                    break;
            }

            if (entry != null)
            {
                entry.name = entry.Name;
                m_Target.AddEntry(entry);
            }
        }

        private void DrawEntry(BlackboardEntry entry)
        {
            bool deleteEntry = false;
            FieldInfo[] fields = entry.GetType().GetFields();
            SerializedObject so = new SerializedObject(entry);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Draw each entry element as a dropdown headergroup
            GUIStyle headerStyle = "DropDownButton";
            entry.Expand = EditorGUILayout.BeginFoldoutHeaderGroup(entry.Expand, entry.Name, headerStyle, null, headerStyle);
            
            if (entry.Expand)
            {
                Utilities.DrawLineSeparator();
                entry.SetName(EditorGUILayout.TextField("Name", entry.Name));

                foreach (var field in fields)
                {
                    SerializedProperty sp = so.FindProperty(field.Name);

                    if (sp != null)
                        EditorGUILayout.PropertyField(sp, new GUIContent(field.Name.TrimStart('m', '_')), false);
                }

                deleteEntry = GUILayout.Button("Delete");
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(entry);

            if (deleteEntry)
                m_Target.RemoveEntry(entry.Name, entry.ValueType);
        }
    }
}

