using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Yontalane.Shaders
{
    public class RetoneGUI : RetoneBaseGUI
    {
        protected override void OnMainGUI(MaterialEditor materialEditor, MaterialProperty[] properties, Material material)
        {
            MaterialProperty mainTex = FindProperty("_MainTex", properties);
            EditorGUI.BeginChangeCheck();
            mainTex.textureValue = EditorGUILayout.ObjectField(mainTex.displayName, mainTex.textureValue, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight)) as Texture2D;
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(TargetMaterial);
            }

            base.OnMainGUI(materialEditor, properties, material);
        }

        protected override void OnExtraGUI(MaterialEditor materialEditor, MaterialProperty[] properties, Material material)
        {
            MaterialProperty useOutline = FindProperty("_UseOutline", properties);
            if (ShaderGUIUtility.SectionHeaderToggle(new GUIContent(useOutline.displayName, "Draw an outline around the object."), useOutline, TargetMaterial, "_USE_OUTLINE"))
            {
                ShaderGUIUtility.BeginSection();

                MaterialProperty outlineColor = FindProperty("_OutlineColor", properties);
                materialEditor.ShaderProperty(outlineColor, new GUIContent(outlineColor.displayName));

                MaterialProperty outlineWidth = FindProperty("_OutlineWidth", properties);
                int outlineWidthValue = Mathf.FloorToInt(outlineWidth.floatValue);
                EditorGUI.BeginChangeCheck();
                outlineWidthValue = EditorGUILayout.IntField(outlineWidth.displayName, outlineWidthValue);
                if (EditorGUI.EndChangeCheck())
                {
                    outlineWidth.floatValue = Mathf.Max(outlineWidthValue, 0);
                    EditorUtility.SetDirty(TargetMaterial);
                }

                ShaderGUIUtility.EndSection();
            }
        }
    }
}