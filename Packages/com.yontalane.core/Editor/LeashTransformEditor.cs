using UnityEditor;
using UnityEngine;

namespace Yontalane
{
    [CustomEditor(typeof(LeashTransform))]
    public class LeashTransformEditor : Editor
    {
        SerializedProperty m_target = null;
        SerializedProperty m_positionConfig = null;
        SerializedProperty m_positionConfigName = null;
        SerializedProperty m_rotationConfig = null;
        SerializedProperty m_rotationConfigName = null;
        SerializedProperty m_updateType = null;

        private void OnEnable()
        {
            m_target = serializedObject.FindProperty("m_target");
            m_positionConfig = serializedObject.FindProperty("m_positionConfig");
            m_positionConfigName = m_positionConfig.FindPropertyRelative("m_name");
            m_rotationConfig = serializedObject.FindProperty("m_rotationConfig");
            m_rotationConfigName = m_rotationConfig.FindPropertyRelative("m_name");
            m_updateType = serializedObject.FindProperty("m_updateType");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (m_positionConfigName.stringValue != "Position" || m_rotationConfigName.stringValue != "Rotation")
            {
                m_positionConfigName.stringValue = "Position";
                m_rotationConfigName.stringValue = "Rotation";
                EditorUtility.SetDirty(serializedObject.targetObject);
            }

            EditorGUILayout.PropertyField(m_target, new GUIContent(m_target.displayName, $"What is this object being leashed to?"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_positionConfig);
            EditorGUILayout.PropertyField(m_rotationConfig);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_updateType, new GUIContent(m_updateType.displayName, $"How frequently to update the leashed object's position."));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
