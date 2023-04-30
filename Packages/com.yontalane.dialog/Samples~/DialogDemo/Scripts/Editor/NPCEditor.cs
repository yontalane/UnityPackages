using UnityEditor;
using UnityEngine;

namespace Yontalane.Demos.Dialog
{
    [CustomEditor(typeof(NPC))]
    [CanEditMultipleObjects]
    public class NPCEditor : Editor
    {
        private SerializedProperty m_desiredItem;
        private SerializedProperty m_displayName;
        private SerializedProperty m_inputType;
        private SerializedProperty m_data;
        private SerializedProperty m_json;
        private SerializedProperty m_staticText;
        private SerializedProperty m_keywords;
        private GUIContent m_dataLabel;
        private GUIContent m_jsonLabel;
        private GUIContent m_staticTextLabel;

        private void OnEnable()
        {
            m_desiredItem = serializedObject.FindProperty("m_desiredItem");

            m_displayName = serializedObject.FindProperty("m_displayName");
            m_inputType = serializedObject.FindProperty("m_inputType");
            m_data = serializedObject.FindProperty("m_data");
            m_json = serializedObject.FindProperty("m_json");
            m_staticText = serializedObject.FindProperty("m_staticText");
            m_keywords = serializedObject.FindProperty("m_keywords");

            m_dataLabel = new GUIContent(string.Empty, m_data.tooltip);
            m_jsonLabel = new GUIContent(string.Empty, m_json.tooltip);
            m_staticTextLabel = new GUIContent(string.Empty, m_staticText.tooltip);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_desiredItem);

            EditorGUILayout.PropertyField(m_displayName);

            EditorGUILayout.PropertyField(m_inputType);

            EditorGUI.indentLevel++;
            switch (m_inputType.enumValueIndex)
            {
                case 0:
                    EditorGUILayout.PropertyField(m_data, m_dataLabel);
                    break;
                case 1:
                    EditorGUILayout.PropertyField(m_json, m_jsonLabel);
                    break;
                case 2:
                    EditorGUILayout.PropertyField(m_staticText, m_staticTextLabel);
                    break;
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(m_keywords);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
