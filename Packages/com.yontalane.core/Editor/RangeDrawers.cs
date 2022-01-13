using UnityEditor;
using UnityEngine;

namespace Yontalane
{
    [CustomPropertyDrawer(typeof(FloatRange))]
    public class FloatRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // positions
            float width = Mathf.Floor(position.width * 0.5f) - 3f;
            Rect minRect = new Rect(position.x, position.y, width, position.height);
            Rect maxRect = new Rect(position.x + width + 5f, position.y, width, position.height);

            // properties
            EditorGUI.PropertyField(minRect, property.FindPropertyRelative("min"), GUIContent.none);
            EditorGUI.PropertyField(maxRect, property.FindPropertyRelative("max"), GUIContent.none);

            // tooltips
            EditorGUI.LabelField(minRect, new GUIContent("", "Min"));
            EditorGUI.LabelField(maxRect, new GUIContent("", "Max"));

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(IntRange))]
    public class IntRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // positions
            float width = Mathf.Floor(position.width * 0.5f) - 3f;
            Rect minRect = new Rect(position.x, position.y, width, position.height);
            Rect maxRect = new Rect(position.x + width + 5f, position.y, width, position.height);

            // properties
            EditorGUI.PropertyField(minRect, property.FindPropertyRelative("min"), GUIContent.none);
            EditorGUI.PropertyField(maxRect, property.FindPropertyRelative("max"), GUIContent.none);

            // tooltips
            EditorGUI.LabelField(minRect, new GUIContent("", "Min"));
            EditorGUI.LabelField(maxRect, new GUIContent("", "Max"));

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}