using UnityEditor;
using UnityEngine;

namespace Yontalane
{
    [CustomPropertyDrawer(typeof(SerializableDictionaryBase_DoNotUse), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        private const float MARGIN = 3f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty keysProperty = property.FindPropertyRelative("m_keys");
            SerializedProperty valuesProperty = property.FindPropertyRelative("m_values");

            if (keysProperty == null || valuesProperty == null)
            {
                Debug.LogError($"Attempting to use object type {typeof(SerializableDictionaryBase_DoNotUse).Name}. Do not do this.");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            Rect expandedRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            bool expandedValue = EditorPrefs.GetBool(ExpandedPrefKey(property), false);
            EditorGUI.BeginChangeCheck();
            expandedValue = EditorGUI.Foldout(expandedRect, expandedValue, new GUIContent(label.text, label.tooltip));
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(ExpandedPrefKey(property), expandedValue);
            }

            if (expandedValue)
            {
                EditorGUI.indentLevel++;

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

                Rect leftRect = new Rect(position.x, sizeRect.y + MARGIN, position.width * 0.333f, EditorGUIUtility.singleLineHeight);
                Rect middleRect = new Rect(position.x + position.width * 0.333f, sizeRect.y + MARGIN, position.width * 0.333f, EditorGUIUtility.singleLineHeight);
                Rect rightRect = new Rect(position.x + position.width * 0.667f, sizeRect.y + MARGIN, position.width * 0.333f, EditorGUIUtility.singleLineHeight);

                for (int i = 0; i < count; i++)
                {
                    leftRect.y += LineHeightWithMargin;
                    middleRect.y += LineHeightWithMargin;
                    rightRect.y += LineHeightWithMargin;
                    SerializedProperty keyProperty = keysProperty.GetArrayElementAtIndex(i);
                    SerializedProperty valueProperty = valuesProperty.GetArrayElementAtIndex(i);
                    EditorGUI.LabelField(leftRect, $"Element {i}");
                    EditorGUI.PropertyField(middleRect, keyProperty, GUIContent.none);
                    EditorGUI.PropertyField(rightRect, valueProperty, GUIContent.none);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty keysProperty = property.FindPropertyRelative("m_keys");
            SerializedProperty valuesProperty = property.FindPropertyRelative("m_values");

            if (keysProperty == null || valuesProperty == null)
            {
                return 0f;
            }

            bool expandedValue = EditorPrefs.GetBool(ExpandedPrefKey(property), false);

            if (!expandedValue) return EditorGUIUtility.singleLineHeight;

            int keysCount = keysProperty.arraySize;
            int valuesCount = valuesProperty.arraySize;
            int count = Mathf.Min(keysCount, valuesCount);

            return EditorGUIUtility.singleLineHeight + (LineHeightWithMargin * (count + 1)) + MARGIN;
        }

        private float LineHeightWithMargin => EditorGUIUtility.singleLineHeight + MARGIN;

        private string ExpandedPrefKey(SerializedProperty property) => $"{property.type}_{property.serializedObject.targetObject.GetType()}_Expanded";
    }
}
