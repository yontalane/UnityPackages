using UnityEditor;
using UnityEngine;

namespace Yontalane.Shaders
{
    [CustomPropertyDrawer(typeof(ScreenEffectConfig))]
    public class ScreenEffectConfigDrawer : PropertyDrawer
    {
        private const int MARGIN = 2;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float w = position.width * 0.5f;
            float h = GetPropertyHeight(property, label);

            Rect rectA = new Rect(position.x, position.y, w - MARGIN, h);
            Rect rectB = new Rect(position.x + w, position.y, w, h);

            SerializedProperty userSetShader = property.FindPropertyRelative("userSetShader");
            SerializedProperty shader = property.FindPropertyRelative("shader");
            SerializedProperty material = property.FindPropertyRelative("material");

            EditorGUI.BeginChangeCheck();
            userSetShader.boolValue = EditorGUI.IntPopup(rectA, userSetShader.boolValue ? 1 : 0, new string[] { "Shader", "Material" }, new int[] { 1, 0 }) == 1;
            if (EditorGUI.EndChangeCheck() && property.serializedObject != null && property.serializedObject.targetObject)
            {
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }

            if (userSetShader.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(rectB, shader, GUIContent.none);
                if (EditorGUI.EndChangeCheck() && !Application.isPlaying)
                {
                    material.objectReferenceValue = null;
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(rectB, material, GUIContent.none);
                if (EditorGUI.EndChangeCheck() && !Application.isPlaying)
                {
                    shader.objectReferenceValue = null;
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
            }

            EditorGUI.EndProperty();
        }
    }
}