using UnityEditor;
using UnityEngine;
using Yontalane;

namespace YontalaneEditor
{
    [CustomEditor(typeof(LeashTransform))]
    internal class LeashTransformEditor : Editor
    {
        SerializedProperty m_target = null;
        SerializedProperty m_positionConfig = null;
        SerializedProperty m_positionConfigName = null;
        SerializedProperty m_rotationConfig = null;
        SerializedProperty m_rotationConfigName = null;
        SerializedProperty m_scaleConfig = null;
        SerializedProperty m_scaleConfigName = null;
        SerializedProperty m_updateType = null;
        SerializedProperty m_useRigidbody = null;
        SerializedProperty m_useBounds = null;
        SerializedProperty m_bounds = null;

        private void OnEnable()
        {
            m_target = serializedObject.FindProperty("m_target");
            m_positionConfig = serializedObject.FindProperty("m_positionConfig");
            m_positionConfigName = m_positionConfig.FindPropertyRelative("m_name");
            m_rotationConfig = serializedObject.FindProperty("m_rotationConfig");
            m_rotationConfigName = m_rotationConfig.FindPropertyRelative("m_name");
            m_scaleConfig = serializedObject.FindProperty("m_scaleConfig");
            m_scaleConfigName = m_scaleConfig.FindPropertyRelative("m_name");
            m_updateType = serializedObject.FindProperty("m_updateType");
            m_useRigidbody = serializedObject.FindProperty("m_useRigidbody");
            m_useBounds = serializedObject.FindProperty("m_useBounds");
            m_bounds = serializedObject.FindProperty("m_bounds");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (m_positionConfigName.stringValue != "Position" || m_rotationConfigName.stringValue != "Rotation" || m_scaleConfigName.stringValue != "Scale")
            {
                m_positionConfigName.stringValue = "Position";
                m_rotationConfigName.stringValue = "Rotation";
                m_scaleConfigName.stringValue = "Scale";
                EditorUtility.SetDirty(serializedObject.targetObject);
            }

            EditorGUILayout.PropertyField(m_target, new GUIContent(m_target.displayName, $"What is this object being leashed to?"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_positionConfig);
            EditorGUILayout.PropertyField(m_rotationConfig);
            EditorGUILayout.PropertyField(m_scaleConfig);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_updateType, new GUIContent(m_updateType.displayName, $"How frequently to update the leashed object's position."));
            EditorGUILayout.PropertyField(m_useRigidbody, new GUIContent(m_useRigidbody.displayName, $"Whether to leash using Rigidbody, if one is present."));
            EditorGUILayout.PropertyField(m_useBounds, new GUIContent(m_useBounds.displayName, $"Whether or not to bound positional leashing."));
            if (m_useBounds.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_bounds, new GUIContent(m_bounds.displayName, $"Stop positional leashing if target is outside these world bounds."));
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
