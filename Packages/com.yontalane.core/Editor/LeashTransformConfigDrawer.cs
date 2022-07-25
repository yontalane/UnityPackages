using UnityEditor;
using UnityEngine;
using Yontalane;

namespace YontalaneEditor
{
    [CustomPropertyDrawer(typeof(LeashTransform.Config))]
    internal class LeashTransformConfigDrawer : PropertyDrawer
    {
        private const int MARGIN = 2;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float y = position.y;

            string name = property.FindPropertyRelative("m_name").stringValue.ToLower();
            bool shouldLeash = property.FindPropertyRelative("shouldLeash").boolValue;
            EditorGUI.BeginChangeCheck();
            shouldLeash = EditorGUI.ToggleLeft(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight),
                new GUIContent($" {name.ToUpper()}", $"Leash {name}?"),
                shouldLeash);
            if (EditorGUI.EndChangeCheck())
            {
                property.FindPropertyRelative("shouldLeash").boolValue = shouldLeash;
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }

            if (shouldLeash)
            {
                EditorGUI.indentLevel++;

                y += (LineHeight + MARGIN) * 1.5f;

                EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight),
                    property.FindPropertyRelative("offsetType"),
                    new GUIContent($"Offset", $"Should the offset between the target object and the leashed object be determined by their starting {name}s or should it be manually set?"));
                if (property.FindPropertyRelative("offsetType").enumValueIndex == 1)
                {
                    y += LineHeight + MARGIN;

                    EditorGUI.indentLevel++;
                    Vector3 offset = property.FindPropertyRelative("offset").vector3Value;
                    EditorGUI.BeginChangeCheck();
                    offset = EditorGUI.Vector3Field(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight),
                        new GUIContent(string.Empty, $"The {name} offset between the target object and the leashed object."),
                        offset);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.FindPropertyRelative("offset").vector3Value = offset;
                        EditorUtility.SetDirty(property.serializedObject.targetObject);
                    }
                    EditorGUI.indentLevel--;
                }

                y += LineHeight + MARGIN;

                bool enabled = GUI.enabled;
                if (name == "scale")
                {
                    GUI.enabled = false;
                    if (property.FindPropertyRelative("space").enumValueIndex != 0)
                    {
                        property.FindPropertyRelative("space").enumValueIndex = 0;
                        EditorUtility.SetDirty(property.serializedObject.targetObject);
                    }
                }

                EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight),
                    property.FindPropertyRelative("space"),
                    new GUIContent($"Space", $"Should the leashing use world space (most common) or local space?"));

                if (name == "scale")
                {
                    GUI.enabled = enabled;
                }

                y += (LineHeight + MARGIN) * 1.5f;

                EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight),
                    property.FindPropertyRelative("slack"),
                    new GUIContent($"Slack", $"If the objects are this close, don't leash."));

                y += LineHeight + MARGIN;

                EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight),
                    property.FindPropertyRelative("smoothTime"),
                    new GUIContent($"Smooth Damp", $"Should the leashed object snap to the desired {name} or should it move there smoothly? Set this to zero for snapping."));

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.FindPropertyRelative("shouldLeash").boolValue)
            {
                return property.FindPropertyRelative("offsetType").enumValueIndex == 1 ? (LineHeight + MARGIN) * 7.5f : (LineHeight + MARGIN) * 6.5f;
            }
            else
            {
                return (LineHeight + MARGIN) * 1.25f;
            }
        }

        private float LineHeight => EditorGUIUtility.singleLineHeight;
    }
}
