using UnityEditor;
using UnityEngine;

namespace Yontalane.Shaders
{
    public class WaterGUI : ShaderGUI
    {
        public Material TargetMaterial { get; protected set; } = null;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            TargetMaterial = materialEditor.target as Material;
			
			// main
			
            ShaderGUIUtility.BeginSection(new GUIContent("Main"));

            MaterialProperty color = FindProperty("_Color", properties);
            materialEditor.ShaderProperty(color, new GUIContent(color.displayName));

            if (ShaderGUIUtility.SectionHeaderToggle(FindProperty("_UseMain", properties), TargetMaterial, "_USE_MAIN"))
            {
				ShaderGUIUtility.BeginSection();

				MaterialProperty mainTex = FindProperty("_MainTex", properties);
				materialEditor.ShaderProperty(mainTex, new GUIContent(mainTex.displayName));

				MaterialProperty glossiness = FindProperty("_Glossiness", properties);
				materialEditor.ShaderProperty(glossiness, new GUIContent(glossiness.displayName));

				MaterialProperty metallic = FindProperty("_Metallic", properties);
				materialEditor.ShaderProperty(metallic, new GUIContent(metallic.displayName));

				ShaderGUIUtility.EndSection();
			}

            ShaderGUIUtility.EndSection();

            // depth

            bool useDepth = false;

			// foam
			
            if (ShaderGUIUtility.SectionHeaderToggle(FindProperty("_UseFoam", properties), TargetMaterial, "_USE_FOAM"))
            {
                useDepth = true;

				ShaderGUIUtility.BeginSection();

				MaterialProperty foamColor = FindProperty("_FoamColor", properties);
				materialEditor.ShaderProperty(foamColor, new GUIContent(foamColor.displayName));

				MaterialProperty foamDepth = FindProperty("_FoamCutoff", properties);
				materialEditor.ShaderProperty(foamDepth, new GUIContent(foamDepth.displayName));

				EditorGUILayout.Space();

				if (ShaderGUIUtility.SectionHeaderToggle(FindProperty("_UseFoamAnimTex", properties), TargetMaterial, "_USE_FOAM_ANIM_TEX"))
				{
					ShaderGUIUtility.BeginSection();

					MaterialProperty foamAnimation = FindProperty("_FoamAnimTex", properties);
					ShaderGUIUtility.ImageField(TargetMaterial, foamAnimation, false);

					ShaderGUIUtility.EndSection();
				}

				ShaderGUIUtility.EndSection();
			}

            // fog

            if (ShaderGUIUtility.SectionHeaderToggle(FindProperty("_UseFog", properties), TargetMaterial, "_USE_FOG"))
            {
                useDepth = true;

                ShaderGUIUtility.BeginSection();

				MaterialProperty fogStart = FindProperty("_FogCutoffStart", properties);
				materialEditor.ShaderProperty(fogStart, new GUIContent(fogStart.displayName));

				MaterialProperty fogEnd = FindProperty("_FogCutoffEnd", properties);
				materialEditor.ShaderProperty(fogEnd, new GUIContent(fogEnd.displayName));

				MaterialProperty fogColor = FindProperty("_FogColor", properties);
				materialEditor.ShaderProperty(fogColor, new GUIContent(fogColor.displayName));

				ShaderGUIUtility.EndSection();
			}

            // depth

            if (useDepth)
            {
                ShaderGUIUtility.BeginSection(new GUIContent("Depth", ""));

                MaterialProperty depthTest = FindProperty("_DepthTest", properties);
				ShaderGUIUtility.FloatField(TargetMaterial, depthTest, 0f, null, null);

                ShaderGUIUtility.EndSection();
            }

            // caustics

            if (ShaderGUIUtility.SectionHeaderToggle(FindProperty("_UseCaustics", properties), TargetMaterial, "_USE_CAUSTICS"))
            {
				ShaderGUIUtility.BeginSection();

				MaterialProperty causticsTex = FindProperty("_CausticsTex", properties);
				ShaderGUIUtility.ImageField(TargetMaterial, causticsTex, false);

				MaterialProperty causticsColor = FindProperty("_CausticsColor", properties);
				materialEditor.ShaderProperty(causticsColor, new GUIContent(causticsColor.displayName));

				ShaderGUIUtility.EndSection();
			}
			
			// height
			
            if (ShaderGUIUtility.SectionHeaderToggle(FindProperty("_UseHeight", properties), TargetMaterial, "_USE_HEIGHT"))
            {
				ShaderGUIUtility.BeginSection();

				MaterialProperty heightTex = FindProperty("_HeightTex", properties);
				ShaderGUIUtility.ImageField(TargetMaterial, heightTex, false);

				MaterialProperty heightScalar = FindProperty("_HeightScalar", properties);
				ShaderGUIUtility.FloatField(TargetMaterial, heightScalar, 0f, null, 100f);

				MaterialProperty heightCoordScalar = FindProperty("_HeightCoordScalar", properties);
				ShaderGUIUtility.FloatField(TargetMaterial, heightCoordScalar, 0f, null, 0.01f);

				MaterialProperty heightTimeScalar = FindProperty("_HeightTimeScalar", properties);
				ShaderGUIUtility.FloatField(TargetMaterial, heightTimeScalar, 0f, null, 10f);

				ShaderGUIUtility.EndSection();
			}
        }
    }
}