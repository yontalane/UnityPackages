using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Yontalane.Shaders
{
    internal class SurfaceBaseGUI : ShaderGUI
    {
        public Material TargetMaterial { get; protected set; } = null;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            TargetMaterial = materialEditor.target as Material;

            ShaderGUIUtility.BeginSection(new GUIContent("Main", "Base material properties."));

            MaterialProperty color = FindProperty("_Color", properties);
            materialEditor.ShaderProperty(color, new GUIContent(color.displayName));

            MaterialProperty mainTex = FindProperty("_MainTex", properties);
            materialEditor.ShaderProperty(mainTex, new GUIContent(mainTex.displayName));

            MaterialProperty glossiness = FindProperty("_Glossiness", properties);
            materialEditor.ShaderProperty(glossiness, new GUIContent(glossiness.displayName));

            MaterialProperty metallic = FindProperty("_Metallic", properties);
            materialEditor.ShaderProperty(metallic, new GUIContent(metallic.displayName));

            ShaderGUIUtility.EndSection();

            MaterialProperty outline = FindProperty("_UseOutline", properties);
            if (ShaderGUIUtility.SectionHeaderToggle(new GUIContent(outline.displayName, "Add an outline around the object."), outline, TargetMaterial, "_USE_OUTLINE"))
            {
                ShaderGUIUtility.BeginSection();

                MaterialProperty outlineColor = FindProperty("_OutlineColor", properties);
                materialEditor.ShaderProperty(outlineColor, new GUIContent(outlineColor.displayName));

                MaterialProperty outlineWidth = FindProperty("_OutlineWidth", properties);
                materialEditor.ShaderProperty(outlineWidth, new GUIContent(outlineWidth.displayName));

                ShaderGUIUtility.EndSection();
            }
        }
    }
}