using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Yontalane.Shaders
{
    internal class RetoneBaseGUI : ShaderGUI
    {
        public Material TargetMaterial { get; protected set; } = null;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            TargetMaterial = materialEditor.target as Material;

            ShaderGUIUtility.BeginSection(new GUIContent("Main", "Base material properties."));

            OnMainGUI(materialEditor, properties, TargetMaterial);

            ShaderGUIUtility.EndSection();
            ShaderGUIUtility.BeginSection(new GUIContent("Tone", "Draw textures on the material depending on the color from the base properties."));

            OnToneGUI(materialEditor, properties, TargetMaterial);

            ShaderGUIUtility.EndSection();

            MaterialProperty post = FindProperty("_UsePost", properties);
            if (ShaderGUIUtility.SectionHeaderToggle(new GUIContent(post.displayName, "Add a color tint after drawing the tone textures."), post, TargetMaterial, "_USE_POST"))
            {
                ShaderGUIUtility.BeginSection();
                OnTintGUI(materialEditor, properties, TargetMaterial);
                ShaderGUIUtility.EndSection();
            }

            OnExtraGUI(materialEditor, properties, TargetMaterial);
        }

        protected virtual void OnMainGUI(MaterialEditor materialEditor, MaterialProperty[] properties, Material material)
        {
            MaterialProperty color = FindProperty("_Color", properties);
            materialEditor.ShaderProperty(color, new GUIContent(color.displayName));
        }

        protected virtual void OnToneGUI(MaterialEditor materialEditor, MaterialProperty[] properties, Material material)
        {
            MaterialProperty blendMode = FindProperty("_BlendMode", properties);
            int blendModeValue = Mathf.FloorToInt(blendMode.floatValue);
            EditorGUI.BeginChangeCheck();
            blendModeValue = EditorGUILayout.IntPopup(blendMode.displayName, blendModeValue, new string[] { "Hide", "Normal", "Multiply", "Add" }, new int[] { 0, 1, 2, 3 });
            if (EditorGUI.EndChangeCheck())
            {
                blendMode.floatValue = blendModeValue;
                string[] array = TargetMaterial.shaderKeywords;
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
                TargetMaterial.shaderKeywords = list.ToArray();
                EditorUtility.SetDirty(TargetMaterial);
            }

            if (blendModeValue > 0.5f)
            {
                EditorGUI.indentLevel++;
                MaterialProperty blendAmount = FindProperty("_BlendAmount", properties);
                EditorGUI.BeginChangeCheck();
                blendAmount.floatValue = EditorGUILayout.Slider(blendAmount.displayName, blendAmount.floatValue, 0f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(TargetMaterial);
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
                string[] array = TargetMaterial.shaderKeywords;
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
                TargetMaterial.shaderKeywords = list.ToArray();
                EditorUtility.SetDirty(TargetMaterial);
            }

            MaterialProperty pixelScale = FindProperty("_PixelScale", properties);
            int pixelScaleValue = Mathf.FloorToInt(pixelScale.floatValue);
            EditorGUI.BeginChangeCheck();
            pixelScaleValue = EditorGUILayout.IntField(pixelScale.displayName, pixelScaleValue);
            if (EditorGUI.EndChangeCheck())
            {
                pixelScale.floatValue = Mathf.Max(pixelScaleValue, 1);
                EditorUtility.SetDirty(TargetMaterial);
            }

            MaterialProperty toneCount = FindProperty("_ToneCount", properties);
            int toneCountValue = Mathf.FloorToInt(toneCount.floatValue);
            EditorGUI.BeginChangeCheck();
            toneCountValue = EditorGUILayout.IntPopup(toneCount.displayName, toneCountValue, new string[] { "2", "3", "5", "7", "9" }, new int[] { 0, 1, 2, 3, 4 });
            if (EditorGUI.EndChangeCheck())
            {
                toneCount.floatValue = toneCountValue;
                string[] array = TargetMaterial.shaderKeywords;
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
                TargetMaterial.shaderKeywords = list.ToArray();
                EditorUtility.SetDirty(TargetMaterial);
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
        }

        protected virtual void OnTintGUI(MaterialEditor materialEditor, MaterialProperty[] properties, Material material)
        {
            MaterialProperty postNormal = FindProperty("_PostNormal", properties);
            materialEditor.ShaderProperty(postNormal, new GUIContent(postNormal.displayName));

            MaterialProperty postMultiply = FindProperty("_PostMultiply", properties);
            materialEditor.ShaderProperty(postMultiply, new GUIContent(postMultiply.displayName));

            MaterialProperty postAdd = FindProperty("_PostAdd", properties);
            materialEditor.ShaderProperty(postAdd, new GUIContent(postAdd.displayName));
        }

        protected virtual void OnExtraGUI(MaterialEditor materialEditor, MaterialProperty[] properties, Material material)
        {
        }

        private void ToneEditor(int index, MaterialEditor materialEditor, MaterialProperty[] properties, bool includeMin = true)
        {
            EditorGUILayout.BeginHorizontal();

            MaterialProperty tone = FindProperty($"_Tone0{index}", properties);
            EditorGUI.BeginChangeCheck();
            tone.textureValue = EditorGUILayout.ObjectField(tone.textureValue, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight)) as Texture2D;
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(TargetMaterial);
            }

            if (includeMin)
            {
                MaterialProperty toneMin = FindProperty($"_Tone0{index}_Min", properties);
                EditorGUI.BeginChangeCheck();
                toneMin.floatValue = EditorGUILayout.Slider(toneMin.floatValue, 0f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(TargetMaterial);
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