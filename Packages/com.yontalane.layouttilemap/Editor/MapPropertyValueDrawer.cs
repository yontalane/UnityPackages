using UnityEditor;
using UnityEngine;
using Yontalane.LayoutTilemap;

namespace YontalaneEditor.LayoutTilemap
{
    /// <summary>
    /// Custom property drawer for MapPropertyValue, allowing selection of value type and editing of the corresponding value in the Unity Inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(MapPropertyValue))]
    internal class MapPropertyValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float width = position.width * 0.5f;

            Rect enumRect = new(position.x, position.y, width, position.height);
            SerializedProperty enumProp = property.FindPropertyRelative("type");
            EditorGUI.PropertyField(enumRect, enumProp, GUIContent.none);

            Rect valueRect = new(position.x + width, position.y, width, position.height);

            switch(enumProp.enumNames[enumProp.enumValueIndex])
            {
                case "String":
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("stringValue"), GUIContent.none);
                    break;
                case "Float":
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("floatValue"), GUIContent.none);
                    break;
                case "Int":
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("intValue"), GUIContent.none);
                    break;
                case "Bool":
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("boolValue"), GUIContent.none);
                    break;
                case "Object":
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("objectReference"), GUIContent.none);
                    break;
            }

            EditorGUI.EndProperty();
        }
    }
}