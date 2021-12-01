using Maple.Nodes;
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
                    if (m_SelectedNode.GetType().BaseType.GetField(field.Name) == null)
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
                if (IsBlackboardKeyField(field))
                {

                }
                else
                {

                }
            }

            GUILayout.EndVertical();
        }

        private bool IsBlackboardKeyField(FieldInfo field)
        {
            return field.FieldType.Name.Contains("BlackboardKey");
        }
    }
}
