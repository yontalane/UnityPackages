using UnityEditor;
using UnityEngine;
using Yontalane;

namespace YontalaneEditor
{
    [CustomPropertyDrawer(typeof(FloatRange))]
    internal class FloatRangeDrawer : PropertyDrawer
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
            EditorGUI.LabelField(minRect, new GUIContent(string.Empty, "Min"));
            EditorGUI.LabelField(maxRect, new GUIContent(string.Empty, "Max"));

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
    }

    [CustomPropertyDrawer(typeof(IntRange))]
    internal class IntRangeDrawer : PropertyDrawer
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
            EditorGUI.LabelField(minRect, new GUIContent(string.Empty, "Min"));
            EditorGUI.LabelField(maxRect, new GUIContent(string.Empty, "Max"));

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
    }

    [CustomPropertyDrawer(typeof(ClampAttribute))]
    internal class ClampAttributeDrawer : PropertyDrawer
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

        private static string m_textColorHex = null;
        private static string TextColorHex
        {
            get
            {
                if (string.IsNullOrEmpty(m_textColorHex))
                {
                    m_textColorHex = ColorUtility.ToHtmlStringRGB(EditorStyles.label.normal.textColor);
                }
                return m_textColorHex;
            }
        }

        private static GUIStyle m_richTextLabelStyle = null;
        private static GUIStyle RichTextLabelStyle
        {
            get
            {
                if (m_richTextLabelStyle == null)
                {
                    m_richTextLabelStyle = new GUIStyle();
                    m_richTextLabelStyle.normal.textColor = EditorStyles.label.normal.textColor;
                    m_richTextLabelStyle.richText = true;
                }
                return m_richTextLabelStyle;
            }
        }
        #endregion
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, property);

            // attribute
            ClampAttribute clampAttribute = attribute as ClampAttribute;

            // positions
            SerializedProperty minProp = property.FindPropertyRelative("min");
            SerializedProperty maxProp = property.FindPropertyRelative("max");

            // if incorrectly placed
            if (minProp == null || maxProp == null)
            {
                Rect leftRect = new Rect(position.x, position.y, position.width * 0.5f, position.height);
                Rect rightRect = new Rect(position.x + position.width * 0.5f, position.y, position.width * 0.5f, position.height);

                EditorGUI.PrefixLabel(leftRect, new GUIContent($"{label.text}"));
                EditorGUI.LabelField(rightRect, new GUIContent($"Incorrect Attribute", "Only use ClampAttribute on a FloatRange or IntRange."), WarningStyle);
                return;
            }

            // positions
            Rect labelRect = new Rect(position.x, position.y, position.width * 0.5f, position.height);
            Rect sliderRect = new Rect(position.x + position.width * 0.5f, position.y, position.width * 0.5f, position.height);

            // style
            Color color = EditorStyles.label.normal.textColor;

            // properties
            switch (minProp.propertyType)
            {
                case SerializedPropertyType.Float:

                    float minFloat = minProp.floatValue;
                    float maxFloat = maxProp.floatValue;

                    EditorGUI.PrefixLabel(labelRect, new GUIContent($"{label.text} <color=#{TextColorHex}88>{minFloat:0.##}, {maxFloat:0.##}</color>", label.tooltip), RichTextLabelStyle);

                    EditorGUI.BeginChangeCheck();
                    EditorGUI.MinMaxSlider(sliderRect, ref minFloat, ref maxFloat, clampAttribute.min, clampAttribute.max);
                    if (EditorGUI.EndChangeCheck())
                    {
                        minProp.floatValue = minFloat;
                        maxProp.floatValue = maxFloat;
                        EditorUtility.SetDirty(property.serializedObject.targetObject);
                    }

                    break;

                case SerializedPropertyType.Integer:

                    float minInt = minProp.intValue;
                    float maxInt = maxProp.intValue;

                    EditorGUI.PrefixLabel(labelRect, new GUIContent($"{label.text} <color=#{TextColorHex}88>{Mathf.RoundToInt(minInt)}, {Mathf.RoundToInt(maxInt)}</color>", label.tooltip), RichTextLabelStyle);

                    EditorGUI.BeginChangeCheck();
                    EditorGUI.MinMaxSlider(sliderRect, ref minInt, ref maxInt, clampAttribute.min, clampAttribute.max);
                    if (EditorGUI.EndChangeCheck())
                    {
                        minProp.intValue = Mathf.RoundToInt(minInt);
                        maxProp.intValue = Mathf.RoundToInt(maxInt);
                        EditorUtility.SetDirty(property.serializedObject.targetObject);
                    }

                    break;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
    }
}