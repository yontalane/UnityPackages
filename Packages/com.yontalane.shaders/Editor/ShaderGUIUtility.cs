using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Yontalane.Shaders
{
    internal static class ShaderGUIUtility
    {
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
    }
}
