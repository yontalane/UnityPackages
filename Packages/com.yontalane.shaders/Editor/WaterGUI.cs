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
			
            ShaderGUIUtility.BeginSection(new GUIContent(string.Empty));

            MaterialProperty color = FindProperty("_Color", properties);
            materialEditor.ShaderProperty(color, new GUIContent(color.displayName));

            if (ShaderGUIUtility.SectionHeaderToggle(FindProperty("_UseMain", properties), TargetMaterial, "_USE_MAIN"))
            {
				ShaderGUIUtility.BeginSection(new GUIContent("Main", "Base material properties."));

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
			
            if (ShaderGUIUtility.SectionHeaderToggle(FindProperty("_UseDepth", properties), TargetMaterial, "_USE_DEPTH"))
            {
				ShaderGUIUtility.BeginSection(new GUIContent("Depth", ""));

				MaterialProperty depthTest = FindProperty("_DepthTest", properties);
				materialEditor.ShaderProperty(depthTest, new GUIContent(depthTest.displayName));

				ShaderGUIUtility.EndSection();
			}
			
			// foam
			
            if (ShaderGUIUtility.SectionHeaderToggle(FindProperty("_UseFoam", properties), TargetMaterial, "_USE_FOAM"))
            {
				ShaderGUIUtility.BeginSection(new GUIContent("Foam", ""));

				MaterialProperty foamColor = FindProperty("_FoamColor", properties);
				materialEditor.ShaderProperty(foamColor, new GUIContent(foamColor.displayName));

				MaterialProperty foamDepth = FindProperty("_FoamCutoff", properties);
				materialEditor.ShaderProperty(foamDepth, new GUIContent(foamDepth.displayName));

				EditorGUILayout.Space();

				if (ShaderGUIUtility.SectionHeaderToggle(FindProperty("_UseFoamAnimTex", properties), TargetMaterial, "_USE_FOAM_ANIM_TEX"))
				{
					ShaderGUIUtility.BeginSection(new GUIContent("Animation", ""));

					MaterialProperty foamAnimation = FindProperty("_FoamAnimTex", properties);
					ImageField(TargetMaterial, foamAnimation, false);

					ShaderGUIUtility.EndSection();
				}

				ShaderGUIUtility.EndSection();
			}

            // fog

            if (ShaderGUIUtility.SectionHeaderToggle(FindProperty("_UseFog", properties), TargetMaterial, "_USE_FOG"))
            {
				ShaderGUIUtility.BeginSection(new GUIContent("Fog", ""));

				MaterialProperty fogStart = FindProperty("_FogCutoffStart", properties);
				materialEditor.ShaderProperty(fogStart, new GUIContent(fogStart.displayName));

				MaterialProperty fogEnd = FindProperty("_FogCutoffEnd", properties);
				materialEditor.ShaderProperty(fogEnd, new GUIContent(fogEnd.displayName));

				MaterialProperty fogColor = FindProperty("_FogColor", properties);
				materialEditor.ShaderProperty(fogColor, new GUIContent(fogColor.displayName));

				ShaderGUIUtility.EndSection();
			}
			
			// caustics
			
            if (ShaderGUIUtility.SectionHeaderToggle(FindProperty("_UseCaustics", properties), TargetMaterial, "_USE_CAUSTICS"))
            {
				ShaderGUIUtility.BeginSection(new GUIContent("Caustics", ""));

				MaterialProperty causticsTex = FindProperty("_CausticsTex", properties);
				ImageField(TargetMaterial, causticsTex, false);

				MaterialProperty causticsColor = FindProperty("_CausticsColor", properties);
				materialEditor.ShaderProperty(causticsColor, new GUIContent(causticsColor.displayName));

				ShaderGUIUtility.EndSection();
			}
			
			// height
			
            if (ShaderGUIUtility.SectionHeaderToggle(FindProperty("_UseHeight", properties), TargetMaterial, "_USE_HEIGHT"))
            {
				ShaderGUIUtility.BeginSection(new GUIContent("Height", ""));

				MaterialProperty heightTex = FindProperty("_HeightTex", properties);
				ImageField(TargetMaterial, heightTex, false);

				MaterialProperty heightScalar = FindProperty("_HeightScalar", properties);
				FloatField(TargetMaterial, heightScalar, 0f, null, 100f);

				MaterialProperty heightCoordScalar = FindProperty("_HeightCoordScalar", properties);
				FloatField(TargetMaterial, heightCoordScalar, 0f, null, 0.01f);

				MaterialProperty heightTimeScalar = FindProperty("_HeightTimeScalar", properties);
				FloatField(TargetMaterial, heightTimeScalar, 0f, null, 10f);

				ShaderGUIUtility.EndSection();
			}
        }

        private bool ImageField(Material material, GUIContent label, MaterialProperty prop, bool large = true)
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

        private bool ImageField(Material material, string label, MaterialProperty prop, bool large = true) => ImageField(material, new GUIContent(label), prop, large);

        private bool ImageField(Material material, MaterialProperty prop, bool large = true) => ImageField(material, prop.displayName, prop, large);

        private bool FloatField(Material material, GUIContent label, MaterialProperty prop, float? min, float? max, float? scalar)
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

        private bool FloatField(Material material, string label, MaterialProperty prop, float? min, float? max, float? scalar) => FloatField(material, new GUIContent(label), prop, min, max, scalar);

        private bool FloatField(Material material, MaterialProperty prop, float? min, float? max, float? scalar) => FloatField(material, prop.displayName, prop, min, max, scalar);
    }
}