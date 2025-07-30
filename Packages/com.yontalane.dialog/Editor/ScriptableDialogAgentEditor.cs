using UnityEditor;
using Yontalane.Dialog;
using UnityEngine;

namespace YontalaneEditor.Dialog
{
    [CustomEditor(typeof(ScriptableDialogAgent))]
    [CanEditMultipleObjects]
    public class ScriptableDialogAgentEditor : Editor
    {
        private SerializedProperty m_displayName;
        private SerializedProperty m_inputType;
        private SerializedProperty m_data;
        private SerializedProperty m_textDataStart;
        private SerializedProperty m_textData;
        private SerializedProperty m_json;
        private SerializedProperty m_staticText;
        private SerializedProperty m_keywords;
        private SerializedProperty m_inlineImageReplacementInfo;
        private GUIContent m_dataLabel;
        private GUIContent m_textDataStartLabel;
        private GUIContent m_textDataLabel;
        private GUIContent m_jsonLabel;
        private GUIContent m_staticTextLabel;

        private void OnEnable()
        {
            m_displayName = serializedObject.FindProperty("m_displayName");
            m_inputType = serializedObject.FindProperty("m_inputType");
            m_data = serializedObject.FindProperty("m_data");
            m_textDataStart = serializedObject.FindProperty("m_textDataStart");
            m_textData = serializedObject.FindProperty("m_textData");
            m_json = serializedObject.FindProperty("m_json");
            m_staticText = serializedObject.FindProperty("m_staticText");
            m_keywords = serializedObject.FindProperty("m_keywords");
            m_inlineImageReplacementInfo = serializedObject.FindProperty("m_inlineImageReplacementInfo");

            m_dataLabel = new GUIContent(string.Empty, m_data.tooltip);
            m_textDataStartLabel = new GUIContent("Starting Node", m_textDataStart.tooltip);
            m_textDataLabel = new GUIContent(string.Empty, m_textData.tooltip);
            m_jsonLabel = new GUIContent(string.Empty, m_json.tooltip);
            m_staticTextLabel = new GUIContent(string.Empty, m_staticText.tooltip);
        }

        public override void OnInspectorGUI()
        {
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
                default:
                    EditorGUILayout.PropertyField(m_textData, m_textDataLabel);
                    EditorGUILayout.PropertyField(m_textDataStart, m_textDataStartLabel);
                    break;
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(m_keywords);
            EditorGUILayout.PropertyField(m_inlineImageReplacementInfo);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
