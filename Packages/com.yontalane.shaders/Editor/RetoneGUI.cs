using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Yontalane.Shaders
{
    public class RetoneGUI : BaseRetoneGUI
    {
        protected override void OnMainGUI(MaterialEditor materialEditor, MaterialProperty[] properties, Material material)
        {
            MaterialProperty mainTex = FindProperty("_MainTex", properties);
            EditorGUI.BeginChangeCheck();
            mainTex.textureValue = EditorGUILayout.ObjectField(mainTex.displayName, mainTex.textureValue, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight)) as Texture2D;
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_targetMaterial);
            }

            base.OnMainGUI(materialEditor, properties, material);
        }

        protected override void OnExtraGUI(MaterialEditor materialEditor, MaterialProperty[] properties, Material material)
        {
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
    }
}