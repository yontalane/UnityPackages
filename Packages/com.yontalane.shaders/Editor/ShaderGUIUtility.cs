using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YontalaneEditor.Shaders
{
    internal static class ShaderGUIUtility
    {
        #region Style
        private static GUIStyle s_sectionStyle = null;
        private static GUIStyle SectionStyle
        {
            get
            {
                if (s_sectionStyle == null)
                {
                    s_sectionStyle = new GUIStyle
                    {
                        alignment = EditorStyles.helpBox.alignment,
                        border = EditorStyles.helpBox.border,
                        clipping = EditorStyles.helpBox.clipping,
                        contentOffset = EditorStyles.helpBox.contentOffset,
                        fixedHeight = EditorStyles.helpBox.fixedHeight,
                        fixedWidth = EditorStyles.helpBox.fixedWidth,
                        focused = EditorStyles.helpBox.focused,
                        font = EditorStyles.helpBox.font,
                        fontSize = EditorStyles.helpBox.fontSize,
                        fontStyle = EditorStyles.helpBox.fontStyle,
                        hover = EditorStyles.helpBox.hover,
                        imagePosition = EditorStyles.helpBox.imagePosition,
                        margin = EditorStyles.helpBox.margin,
                        name = EditorStyles.helpBox.name,
                        normal = EditorStyles.helpBox.normal,
                        overflow = EditorStyles.helpBox.overflow,
                        padding = new RectOffset(10, 10, 10, 10),
                        richText = EditorStyles.helpBox.richText,
                        stretchHeight = EditorStyles.helpBox.stretchHeight,
                        stretchWidth = EditorStyles.helpBox.stretchWidth,
                        wordWrap = EditorStyles.helpBox.wordWrap
                    };
                }
                return s_sectionStyle;
            }
        }
        #endregion

        #region Sections
        public static void BeginSection(GUIContent label)
        {
            if (label != null && !string.IsNullOrEmpty(label.text))
            {
                SectionHeader(label);
            }
            EditorGUILayout.BeginVertical(SectionStyle);
        }

        public static void BeginSection(string label) => BeginSection(new GUIContent(label));

        public static void BeginSection() => BeginSection(GUIContent.none);

        public static void EndSection(bool includeSpace = true)
        {
            EditorGUILayout.EndVertical();
            if (includeSpace)
            {
                EditorGUILayout.Space();
            }
        }

        public static bool SectionHeaderToggle(GUIContent label, MaterialProperty property, Material material, string keyword)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(label.text.ToUpper(), label.tooltip), EditorStyles.boldLabel);

            if (property != null)
            {
                GUILayout.FlexibleSpace();
                EditorGUI.BeginChangeCheck();
                property.floatValue = EditorGUILayout.Toggle(property.floatValue > 0.5f, GUILayout.Width(EditorGUIUtility.singleLineHeight)) ? 1f : 0f;
                bool wasChanged = EditorGUI.EndChangeCheck();
                EditorGUILayout.EndHorizontal();
                bool isOn = property.floatValue > 0.5f;
                if (wasChanged)
                {
                    string[] array = material.shaderKeywords;
                    List<string> list = new List<string>(array);
                    if (isOn) list.Add(keyword);
                    else list.Remove(keyword);
                    material.shaderKeywords = list.ToArray();
                    EditorUtility.SetDirty(material);
                }
                return isOn;
            }
            else
            {
                EditorGUILayout.EndHorizontal();
                return default;
            }
        }

        public static bool SectionHeaderToggle(string label, MaterialProperty property, Material material, string keyword) => SectionHeaderToggle(new GUIContent(label), property, material, keyword);

        public static bool SectionHeaderToggle(MaterialProperty property, Material material, string keyword) => SectionHeaderToggle(property.displayName, property, material, keyword);

        public static void SectionHeader(GUIContent label) => SectionHeaderToggle(label, null, null, null);

        public static void SectionHeader(string label) => SectionHeaderToggle(label, null, null, null);
        #endregion

        #region Fields
        public static bool ImageField(Material material, GUIContent label, MaterialProperty prop, bool large = true)
        {
            Texture value = prop.textureValue;
            EditorGUI.BeginChangeCheck();
            if (large)
            {
                value = EditorGUILayout.ObjectField(label, value, typeof(Texture), false) as Texture;
            }
            else
            {
                value = EditorGUILayout.ObjectField(label, value, typeof(Texture), false, GUILayout.Height(EditorGUIUtility.singleLineHeight)) as Texture;
            }
            if (EditorGUI.EndChangeCheck())
            {
                prop.textureValue = value;
                EditorUtility.SetDirty(material);
                return true;
            }
            return false;
        }

        public static bool ImageField(Material material, string label, MaterialProperty prop, bool large = true) => ImageField(material, new GUIContent(label), prop, large);

        public static bool ImageField(Material material, MaterialProperty prop, bool large = true) => ImageField(material, prop.displayName, prop, large);

        public static bool FloatField(Material material, GUIContent label, MaterialProperty prop, float? min, float? max, float? scalar)
        {
            float value = prop.floatValue * (scalar != null ? scalar.Value : 1);
            EditorGUI.BeginChangeCheck();
            if (min != null && max != null)
            {
                value = EditorGUILayout.Slider(label, value, min.Value, max.Value);
            }
            else
            {
                value = EditorGUILayout.FloatField(label, value);
            }
            if (EditorGUI.EndChangeCheck())
            {
                if (min != null)
                {
                    value = Mathf.Max(value, min.Value);
                }
                if (max != null)
                {
                    value = Mathf.Min(value, max.Value);
                }
                if (scalar != null)
                {
                    value /= scalar.Value;
                }
                prop.floatValue = value;
                EditorUtility.SetDirty(material);
                return true;
            }
            return false;
        }

        public static bool FloatField(Material material, string label, MaterialProperty prop, float? min, float? max, float? scalar) => FloatField(material, new GUIContent(label), prop, min, max, scalar);

        public static bool FloatField(Material material, MaterialProperty prop, float? min, float? max, float? scalar) => FloatField(material, prop.displayName, prop, min, max, scalar);
        #endregion
    }
}
