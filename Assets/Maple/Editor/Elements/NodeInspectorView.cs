using Maple.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Maple.Editor
{
    public class NodeInspectorView : UnityEditor.Editor
    {
        private BaseNode m_SelectedNode;
        private List<FieldInfo> m_FieldElements;

        public void UpdateTarget(BaseNode targetNode)
        {
            m_SelectedNode = targetNode;

            var fields = m_SelectedNode?.GetType().GetFields().ToList();
            m_FieldElements?.Clear();
            m_FieldElements = new List<FieldInfo>();

            if (fields != null)
            {
                foreach (var field in fields)
                {
                    if ((field.IsPublic && !field.CustomAttributes.Any(attrib => attrib.AttributeType == typeof(HideInInspector))) ||
                        (!field.IsPublic && field.CustomAttributes.Any(attrib => attrib.AttributeType == typeof(SerializeField))))
                    {
                        // Ensure base node fields are displayed before subclass fields
                        if (m_SelectedNode.GetType().GetField(field.Name) == null)
                            m_FieldElements.Add(field);
                        else
                            m_FieldElements.Insert(0, field);
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            if (m_SelectedNode == null)
                return;

            // Begin drawing the styled helpbox for all fields
            GUILayout.BeginVertical(EditorStyles.helpBox);

            foreach (var field in m_FieldElements)
            {
                if (!IsBlackboardKeyField(field))
                {
                    // Remove hungarian notation for presentation
                    string formattedName = field.Name.TrimStart('m', '_');

                    // Obtain an editable property version of the current field
                    SerializedObject serialisedObj = new SerializedObject(m_SelectedNode);
                    SerializedProperty property = serialisedObj.FindProperty(field.Name);

                    // Draw the field and update value
                    EditorGUILayout.PropertyField(property, new GUIContent(formattedName), true);
                    serialisedObj.ApplyModifiedProperties();
                }
            }

            Debug.Log("BlackboardKeys: " + m_SelectedNode.BlackboardKeys.Count);
            foreach (var key in m_SelectedNode.BlackboardKeys)
            {
                var matchingBlackboardEntries = m_SelectedNode.Owner?.Blackboard?.GetEntriesOfType(key.KeyType);

                string fieldName = "";
                foreach (var field in m_FieldElements)
                {
                    var fieldVal = field.GetValue(m_SelectedNode) as Blackboards.BlackboardKey;
                    if (fieldVal != null && fieldVal.Name == key.Name)
                        fieldName = field.Name.TrimStart('m', '_');
                }

                GUILayout.BeginVertical(fieldName, "window");
                CreateBlackboardField(key, matchingBlackboardEntries);
                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();
        }

        private void CreateBlackboardField(Blackboards.BlackboardKey member, List<Blackboards.BlackboardEntry> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                string[] dummy = { "-" };
                EditorGUILayout.Popup("Value", 0, dummy);
            }
            else
            {
                List<string> menuChoices = new List<string>();

                foreach (var item in entries)
                    menuChoices.Add(item.Name);

                FieldInfo selectionInfo = member?.GetType()?.GetField("Selection");
                if (selectionInfo != null)
                {
                    int selection = (int)selectionInfo.GetValue(member);
                    selectionInfo.SetValue(member, EditorGUILayout.Popup("Value", selection, menuChoices.ToArray()));

                    FieldInfo valueInfo = member.GetType().GetField("m_Value");
                    valueInfo.SetValue(member, entries[selection].Value);

                    FieldInfo typeInfo = member.GetType().GetField("KeyType");
                    typeInfo.SetValue(member, entries[selection].ValueType); 

                    FieldInfo nameInfo = member.GetType().GetField("Name");
                    nameInfo.SetValue(member, entries[selection].Name);
                }
            }
        }

        private bool IsBlackboardKeyField(FieldInfo field)
        {
            return field.FieldType.Name.Contains("BlackboardKey");
        }
    }
}
