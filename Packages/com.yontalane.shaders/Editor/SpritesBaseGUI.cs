using System.Collections.Generic;using UnityEditor;using UnityEngine;using UnityEngine.Rendering;public class SpriteBaseGUI : ShaderGUI{    protected Material m_targetMaterial = null;    override public void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)    {        m_targetMaterial = materialEditor.target as Material;

        //MaterialProperty mainTexProperty = FindProperty("_MainTex", properties);
        //materialEditor.ShaderProperty(mainTexProperty, new GUIContent(mainTexProperty.displayName));

        MaterialProperty colorProperty = FindProperty("_Color", properties);        materialEditor.ShaderProperty(colorProperty, new GUIContent(colorProperty.displayName));        MaterialProperty pixelSnapProperty = FindProperty("PixelSnap", properties);        materialEditor.ShaderProperty(pixelSnapProperty, new GUIContent(pixelSnapProperty.displayName));

        EditorGUILayout.Space();

        MaterialProperty materialBlendModeProperty = FindProperty("_MaterialBlendMode", properties);        EditorGUI.BeginChangeCheck();        materialEditor.ShaderProperty(materialBlendModeProperty, new GUIContent(materialBlendModeProperty.displayName));
        if (EditorGUI.EndChangeCheck())
        {
            MaterialProperty srcBlendProperty = FindProperty("_SrcBlend", properties);
            MaterialProperty dstBlendProperty = FindProperty("_DstBlend", properties);
            switch (Mathf.RoundToInt(materialBlendModeProperty.floatValue))
            {
                case 0:
                    srcBlendProperty.floatValue = (int)BlendMode.One;
                    dstBlendProperty.floatValue = (int)BlendMode.OneMinusSrcAlpha;
                    break;
                case 1:
                    srcBlendProperty.floatValue = (int)BlendMode.DstColor;
                    dstBlendProperty.floatValue = (int)BlendMode.OneMinusSrcAlpha;
                    break;
                case 2:
                    srcBlendProperty.floatValue = (int)BlendMode.One;
                    dstBlendProperty.floatValue = (int)BlendMode.One;
                    break;
            }
            EditorUtility.SetDirty(m_targetMaterial);        }

        MaterialProperty tintBlendModeProperty = FindProperty("_TintBlendMode", properties);        EditorGUI.BeginChangeCheck();        materialEditor.ShaderProperty(tintBlendModeProperty, new GUIContent(tintBlendModeProperty.displayName));
        if (EditorGUI.EndChangeCheck())        {            string[] array = m_targetMaterial.shaderKeywords;            List<string> list = new List<string>(array);            switch (Mathf.RoundToInt(tintBlendModeProperty.floatValue))
            {
                case 0:
                    list.Remove("_TINTBLENDMODE_ADDITIVE");
                    list.Remove("_TINTBLENDMODE_OVERLAY");
                    break;
                case 1:
                    list.Remove("_TINTBLENDMODE_OVERLAY");
                    list.Add("_TINTBLENDMODE_ADDITIVE");
                    break;
                case 2:
                    list.Remove("_TINTBLENDMODE_ADDITIVE");
                    list.Add("_TINTBLENDMODE_OVERLAY");
                    break;
            }            m_targetMaterial.shaderKeywords = list.ToArray();            EditorUtility.SetDirty(m_targetMaterial);        }

        EditorGUILayout.Space();

        MaterialProperty useStrokeProperty = FindProperty("_UseStroke", properties);        EditorGUI.BeginChangeCheck();        materialEditor.ShaderProperty(useStrokeProperty, new GUIContent(useStrokeProperty.displayName));
        if (EditorGUI.EndChangeCheck())        {            string[] array = m_targetMaterial.shaderKeywords;            List<string> list = new List<string>(array);            if (useStrokeProperty.floatValue > 0.5f) list.Add("_USE_STROKE");            else list.Remove("_USE_STROKE");            m_targetMaterial.shaderKeywords = list.ToArray();            EditorUtility.SetDirty(m_targetMaterial);        }
        if (useStrokeProperty.floatValue > 0.5)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            MaterialProperty strokeColorProperty = FindProperty("_StrokeColor", properties);
            materialEditor.ShaderProperty(strokeColorProperty, new GUIContent(strokeColorProperty.displayName));            MaterialProperty strokeWidthProperty = FindProperty("_StrokeWidth", properties);
            materialEditor.ShaderProperty(strokeWidthProperty, new GUIContent(strokeWidthProperty.displayName));            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        MaterialProperty useColorReplaceProperty = FindProperty("_UseColorReplace", properties);        EditorGUI.BeginChangeCheck();        materialEditor.ShaderProperty(useColorReplaceProperty, new GUIContent(useColorReplaceProperty.displayName));
        if (EditorGUI.EndChangeCheck())        {            string[] array = m_targetMaterial.shaderKeywords;            List<string> list = new List<string>(array);            if (useColorReplaceProperty.floatValue > 0.5f) list.Add("_USE_COLORREPLACE");            else list.Remove("_USE_COLORREPLACE");            m_targetMaterial.shaderKeywords = list.ToArray();            EditorUtility.SetDirty(m_targetMaterial);        }
        if (useColorReplaceProperty.floatValue > 0.5)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            MaterialProperty colorReplaceSourceProperty = FindProperty("_ColorReplaceSource", properties);
            materialEditor.ShaderProperty(colorReplaceSourceProperty, new GUIContent(colorReplaceSourceProperty.displayName));            MaterialProperty colorReplaceTargetProperty = FindProperty("_ColorReplaceTarget", properties);
            materialEditor.ShaderProperty(colorReplaceTargetProperty, new GUIContent(colorReplaceTargetProperty.displayName));            MaterialProperty colorReplaceFuzzinessProperty = FindProperty("_ColorReplaceFuzziness", properties);
            materialEditor.ShaderProperty(colorReplaceFuzzinessProperty, new GUIContent(colorReplaceFuzzinessProperty.displayName));            EditorGUILayout.EndVertical();        }
    }}