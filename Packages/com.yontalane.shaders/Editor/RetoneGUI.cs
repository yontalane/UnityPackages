using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Yontalane.Shaders
{
    public class RetoneGUI : ShaderGUI
    {
        private static GUIStyle s_style = null;
        private static GUIStyle Style
        {
            get
            {
                if (s_style == null)
                {
                    s_style = new GUIStyle
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
                return s_style;
            }
        }

        protected Material m_targetMaterial = null;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            m_targetMaterial = materialEditor.target as Material;

            EditorGUILayout.LabelField(new GUIContent("MAIN", "Base material properties."), EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(Style);

            MaterialProperty mainTex = FindProperty("_MainTex", properties);
            EditorGUI.BeginChangeCheck();
            mainTex.textureValue = EditorGUILayout.ObjectField(mainTex.displayName, mainTex.textureValue, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight)) as Texture2D;
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_targetMaterial);
            }

            MaterialProperty color = FindProperty("_Color", properties);
            materialEditor.ShaderProperty(color, new GUIContent(color.displayName));

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent("TONE", "Draw textures on the material depending on the color from the base properties."), EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(Style);

            MaterialProperty blendMode = FindProperty("_BlendMode", properties);
            int blendModeValue = Mathf.FloorToInt(blendMode.floatValue);
            EditorGUI.BeginChangeCheck();
            blendModeValue = EditorGUILayout.IntPopup(blendMode.displayName, blendModeValue, new string[] { "Hide", "Normal", "Multiply", "Add" }, new int[] { 0, 1, 2, 3 });
            if (EditorGUI.EndChangeCheck())
            {
                blendMode.floatValue = blendModeValue;
                string[] array = m_targetMaterial.shaderKeywords;
                List<string> list = new List<string>(array);
                list.Remove("_BLEND_MODE_NORMAL");
                list.Remove("_BLEND_MODE_MULTIPLY");
                list.Remove("_BLEND_MODE_ADD");
                switch (blendModeValue)
                {
                    case 1:
                        list.Add("_BLEND_MODE_NORMAL");
                        break;
                    case 2:
                        list.Add("_BLEND_MODE_MULTIPLY");
                        break;
                    case 3:
                        list.Add("_BLEND_MODE_ADD");
                        break;
                }
                m_targetMaterial.shaderKeywords = list.ToArray();
                EditorUtility.SetDirty(m_targetMaterial);
            }

            if (blendModeValue > 0.5f)
            {
                EditorGUI.indentLevel++;
                MaterialProperty blendAmount = FindProperty("_BlendAmount", properties);
                EditorGUI.BeginChangeCheck();
                blendAmount.floatValue = EditorGUILayout.Slider(blendAmount.displayName, blendAmount.floatValue, 0f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(m_targetMaterial);
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            MaterialProperty sourceChannel = FindProperty("_SourceChannel", properties);
            int sourceChannelValue = Mathf.FloorToInt(sourceChannel.floatValue);
            EditorGUI.BeginChangeCheck();
            sourceChannelValue = EditorGUILayout.IntPopup(sourceChannel.displayName, sourceChannelValue, new string[] { "R", "G", "B", "A", "RGB" }, new int[] { 0, 1, 2, 3, 4 });
            if (EditorGUI.EndChangeCheck())
            {
                sourceChannel.floatValue = sourceChannelValue;
                string[] array = m_targetMaterial.shaderKeywords;
                List<string> list = new List<string>(array);
                list.Remove("_SOURCE_R");
                list.Remove("_SOURCE_G");
                list.Remove("_SOURCE_B");
                list.Remove("_SOURCE_A");
                list.Remove("_SOURCE_RGB");
                switch (sourceChannelValue)
                {
                    case 0:
                        list.Add("_SOURCE_R");
                        break;
                    case 1:
                        list.Add("_SOURCE_G");
                        break;
                    case 2:
                        list.Add("_SOURCE_B");
                        break;
                    case 3:
                        list.Add("_SOURCE_A");
                        break;
                    default:
                        list.Add("_SOURCE_RGB");
                        break;
                }
                m_targetMaterial.shaderKeywords = list.ToArray();
                EditorUtility.SetDirty(m_targetMaterial);
            }

            MaterialProperty pixelScale = FindProperty("_PixelScale", properties);
            int pixelScaleValue = Mathf.FloorToInt(pixelScale.floatValue);
            EditorGUI.BeginChangeCheck();
            pixelScaleValue = EditorGUILayout.IntField(pixelScale.displayName, pixelScaleValue);
            if (EditorGUI.EndChangeCheck())
            {
                pixelScale.floatValue = Mathf.Max(pixelScaleValue, 1);
                EditorUtility.SetDirty(m_targetMaterial);
            }

            MaterialProperty toneCount = FindProperty("_ToneCount", properties);
            int toneCountValue = Mathf.FloorToInt(toneCount.floatValue);
            EditorGUI.BeginChangeCheck();
            toneCountValue = EditorGUILayout.IntPopup(toneCount.displayName, toneCountValue, new string[] { "2", "3", "5", "7", "9" }, new int[] { 0, 1, 2, 3, 4 });
            if (EditorGUI.EndChangeCheck())
            {
                toneCount.floatValue = toneCountValue;
                string[] array = m_targetMaterial.shaderKeywords;
                List<string> list = new List<string>(array);
                list.Remove("_TONE_COUNT_2");
                list.Remove("_TONE_COUNT_3");
                list.Remove("_TONE_COUNT_5");
                list.Remove("_TONE_COUNT_7");
                list.Remove("_TONE_COUNT_9");
                switch (toneCountValue)
                {
                    case 0:
                        list.Add("_TONE_COUNT_2");
                        break;
                    case 1:
                        list.Add("_TONE_COUNT_3");
                        break;
                    case 2:
                        list.Add("_TONE_COUNT_5");
                        break;
                    case 3:
                        list.Add("_TONE_COUNT_7");
                        break;
                    default:
                        list.Add("_TONE_COUNT_9");
                        break;
                }
                m_targetMaterial.shaderKeywords = list.ToArray();
                EditorUtility.SetDirty(m_targetMaterial);
            }

            EditorGUILayout.Space();

            switch (toneCountValue)
            {
                case 0:
                    ToneEditor(0, materialEditor, properties, false);
                    ToneEditor(8, materialEditor, properties);
                    break;
                case 1:
                    ToneEditor(0, materialEditor, properties, false);
                    ToneEditor(4, materialEditor, properties);
                    ToneEditor(8, materialEditor, properties);
                    break;
                case 2:
                    ToneEditor(0, materialEditor, properties, false);
                    ToneEditor(3, materialEditor, properties);
                    ToneEditor(4, materialEditor, properties);
                    ToneEditor(5, materialEditor, properties);
                    ToneEditor(8, materialEditor, properties);
                    break;
                case 3:
                    ToneEditor(0, materialEditor, properties, false);
                    ToneEditor(2, materialEditor, properties);
                    ToneEditor(3, materialEditor, properties);
                    ToneEditor(4, materialEditor, properties);
                    ToneEditor(5, materialEditor, properties);
                    ToneEditor(6, materialEditor, properties);
                    ToneEditor(8, materialEditor, properties);
                    break;
                default:
                    ToneEditor(0, materialEditor, properties, false);
                    ToneEditor(1, materialEditor, properties);
                    ToneEditor(2, materialEditor, properties);
                    ToneEditor(3, materialEditor, properties);
                    ToneEditor(4, materialEditor, properties);
                    ToneEditor(5, materialEditor, properties);
                    ToneEditor(6, materialEditor, properties);
                    ToneEditor(7, materialEditor, properties);
                    ToneEditor(8, materialEditor, properties);
                    break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            MaterialProperty usePost = FindProperty("_UsePost", properties);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("TINT", "Add a color tint after drawing the tone textures."), EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            usePost.floatValue = EditorGUILayout.Toggle(usePost.floatValue > 0.5f, GUILayout.Width(EditorGUIUtility.singleLineHeight)) ? 1f : 0f;
            if (EditorGUI.EndChangeCheck())
            {
                string[] array = m_targetMaterial.shaderKeywords;
                List<string> list = new List<string>(array);
                if (usePost.floatValue > 0.5f) list.Add("_USE_POST");
                else list.Remove("_USE_POST");
                m_targetMaterial.shaderKeywords = list.ToArray();
                EditorUtility.SetDirty(m_targetMaterial);
            }
            EditorGUILayout.EndHorizontal();

            if (usePost.floatValue > 0.5f)
            {
                EditorGUILayout.BeginVertical(Style);

                MaterialProperty postScreen = FindProperty("_PostScreen", properties);
                materialEditor.ShaderProperty(postScreen, new GUIContent(postScreen.displayName));

                MaterialProperty postMultiply = FindProperty("_PostMultiply", properties);
                materialEditor.ShaderProperty(postMultiply, new GUIContent(postMultiply.displayName));

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            MaterialProperty useOutline = FindProperty("_UseOutline", properties);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("OUTLINE", "Draw an outline around the object."), EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            useOutline.floatValue = EditorGUILayout.Toggle(useOutline.floatValue > 0.5f, GUILayout.Width(EditorGUIUtility.singleLineHeight)) ? 1f : 0f;
            if (EditorGUI.EndChangeCheck())
            {
                string[] array = m_targetMaterial.shaderKeywords;
                List<string> list = new List<string>(array);
                if (useOutline.floatValue > 0.5f) list.Add("_USE_OUTLINE");
                else list.Remove("_USE_OUTLINE");
                m_targetMaterial.shaderKeywords = list.ToArray();
                EditorUtility.SetDirty(m_targetMaterial);
            }
            EditorGUILayout.EndHorizontal();

            if (useOutline.floatValue > 0.5f)
            {
                EditorGUILayout.BeginVertical(Style);

                MaterialProperty outlineColor = FindProperty("_OutlineColor", properties);
                materialEditor.ShaderProperty(outlineColor, new GUIContent(outlineColor.displayName));

                MaterialProperty outlineWidth = FindProperty("_OutlineWidth", properties);
                int outlineWidthValue = Mathf.FloorToInt(outlineWidth.floatValue);
                EditorGUI.BeginChangeCheck();
                outlineWidthValue = EditorGUILayout.IntField(outlineWidth.displayName, outlineWidthValue);
                if (EditorGUI.EndChangeCheck())
                {
                    outlineWidth.floatValue = Mathf.Max(outlineWidthValue, 0);
                    EditorUtility.SetDirty(m_targetMaterial);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void ToneEditor(int index, MaterialEditor materialEditor, MaterialProperty[] properties, bool includeMin = true)
        {
            EditorGUILayout.BeginHorizontal();

            MaterialProperty tone = FindProperty($"_Tone0{index}", properties);
            EditorGUI.BeginChangeCheck();
            tone.textureValue = EditorGUILayout.ObjectField(tone.textureValue, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight)) as Texture2D;
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_targetMaterial);
            }

            if (includeMin)
            {
                MaterialProperty toneMin = FindProperty($"_Tone0{index}_Min", properties);
                EditorGUI.BeginChangeCheck();
                toneMin.floatValue = EditorGUILayout.Slider(toneMin.floatValue, 0f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(m_targetMaterial);
                }
            }
            else
            {
                GUI.enabled = false;
                EditorGUILayout.Slider(0f, 0f, 1f);
                GUI.enabled = true;
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}