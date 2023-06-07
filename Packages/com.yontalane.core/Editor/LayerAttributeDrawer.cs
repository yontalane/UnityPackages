using UnityEditor;
using UnityEngine;
using Yontalane;

namespace YontalaneEditor
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    internal class LayerAttributeDrawer : PropertyDrawer
    {
        #region Styles
        private static GUIStyle m_warningStyle = null;
        private static GUIStyle WarningStyle
        {
            get
            {
                if (m_warningStyle == null)
                {
                    m_warningStyle = new GUIStyle();
                    m_warningStyle.normal.textColor = Color.red;
                    m_warningStyle.alignment = TextAnchor.MiddleRight;

                }
                return m_warningStyle;
            }
        }
        #endregion

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, property);

            // if incorrectly placed
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                Rect leftRect = new Rect(position.x, position.y, position.width * 0.5f, position.height);
                Rect rightRect = new Rect(position.x + position.width * 0.5f, position.y, position.width * 0.5f, position.height);

                EditorGUI.PrefixLabel(leftRect, label);
                EditorGUI.LabelField(rightRect, new GUIContent($"Incorrect Attribute", "Only use LayerAttribute on an integer."), WarningStyle);
                return;
            }

            // positions
            Rect labelRect = new Rect(position.x, position.y, position.width * 0.5f, position.height);
            Rect menuRect = new Rect(position.x + position.width * 0.5f, position.y, position.width * 0.5f, position.height);

            // properties
            int value = property.intValue;

            EditorGUI.PrefixLabel(labelRect, label);

            EditorGUI.BeginChangeCheck();
            value = EditorGUI.LayerField(menuRect, value);
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = value;
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
    }
}
