using UnityEditor;
using UnityEngine;

namespace DEF
{
    [CustomPropertyDrawer(typeof(DictionaryBase_DoNotUse), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        private const float MARGIN = 3f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.type == typeof(DictionaryBase_DoNotUse).Name) return;

            EditorGUI.BeginProperty(position, label, property);

            Rect expandedRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            SerializedProperty expandedProperty = property.FindPropertyRelative("m_editorExpanded");
            bool expandedValue = expandedProperty.boolValue;
            EditorGUI.BeginChangeCheck();
            expandedValue = EditorGUI.Foldout(expandedRect, expandedValue, new GUIContent(label.text, label.tooltip));
            if (EditorGUI.EndChangeCheck())
            {
                expandedProperty.boolValue = expandedValue;
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }

            if (expandedValue)
            {
                EditorGUI.indentLevel++;

                SerializedProperty keysProperty = property.FindPropertyRelative("m_keys");
                SerializedProperty valuesProperty = property.FindPropertyRelative("m_values");
                int keysCount = keysProperty.arraySize;
                int valuesCount = valuesProperty.arraySize;
                int count = Mathf.Min(keysCount, valuesCount);

                Rect sizeRect = new Rect(position.x, position.y + LineHeightWithMargin, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.BeginChangeCheck();
                count = EditorGUI.IntField(sizeRect, new GUIContent("Size"), count);
                if (EditorGUI.EndChangeCheck())
                {
                    count = Mathf.Max(count, 0);
                    Undo.RecordObject(property.serializedObject.targetObject, "Resized Dictionary");
                    keysProperty.arraySize = count;
                    valuesProperty.arraySize = count;
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }

                Rect leftRect = new Rect(position.x, sizeRect.y + MARGIN, position.width * 0.5f, EditorGUIUtility.singleLineHeight);
                Rect rightRect = new Rect(position.x + position.width * 0.5f, sizeRect.y + MARGIN, position.width * 0.5f, EditorGUIUtility.singleLineHeight);

                for (int i = 0; i < count; i++)
                {
                    leftRect.y += LineHeightWithMargin;
                    rightRect.y += LineHeightWithMargin;
                    SerializedProperty keyProperty = keysProperty.GetArrayElementAtIndex(i);
                    SerializedProperty valueProperty = valuesProperty.GetArrayElementAtIndex(i);
                    EditorGUI.PropertyField(leftRect, keyProperty, GUIContent.none);
                    EditorGUI.PropertyField(rightRect, valueProperty, GUIContent.none);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty expandedProperty = property.FindPropertyRelative("m_editorExpanded");
            bool expandedValue = expandedProperty.boolValue;

            if (!expandedValue) return EditorGUIUtility.singleLineHeight;

            SerializedProperty keysProperty = property.FindPropertyRelative("m_keys");
            SerializedProperty valuesProperty = property.FindPropertyRelative("m_values");
            int keysCount = keysProperty.arraySize;
            int valuesCount = valuesProperty.arraySize;
            int count = Mathf.Min(keysCount, valuesCount);

            return EditorGUIUtility.singleLineHeight + (LineHeightWithMargin * (count + 1)) + MARGIN;
        }

        private float LineHeightWithMargin => EditorGUIUtility.singleLineHeight + MARGIN;
    }
}
