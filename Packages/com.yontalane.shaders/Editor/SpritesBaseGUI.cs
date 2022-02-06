using System.Collections.Generic;using UnityEditor;using UnityEngine;using UnityEngine.Rendering;public class SpriteBaseGUI : ShaderGUI{
    //private static readonly string[] s_materialBlendModes = new string[] { "Normal", "Normal (Pre-multiplied)", "", "Multiply", "Linear Burn", "", "Screen", "Lighten", "Additive" };

    protected Material m_targetMaterial = null;    override public void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)    {        m_targetMaterial = materialEditor.target as Material;

        //MaterialProperty mainTex = FindProperty("_MainTex", properties);
        //materialEditor.ShaderProperty(mainTex, new GUIContent(mainTex.displayName));

        MaterialProperty color = FindProperty("_Color", properties);        materialEditor.ShaderProperty(color, new GUIContent(color.displayName));        MaterialProperty pixelSnap = FindProperty("PixelSnap", properties);        materialEditor.ShaderProperty(pixelSnap, new GUIContent(pixelSnap.displayName));

        EditorGUILayout.Space();

        MaterialProperty materialBlendMode = FindProperty("_MaterialBlendMode", properties);        EditorGUI.BeginChangeCheck();        materialEditor.ShaderProperty(materialBlendMode, new GUIContent(materialBlendMode.displayName));
        if (EditorGUI.EndChangeCheck())
        {
            MaterialProperty blendOp = FindProperty("_BlendOp", properties);
            MaterialProperty srcBlend = FindProperty("_SrcBlend", properties);
            MaterialProperty dstBlend = FindProperty("_DstBlend", properties);
            switch (Mathf.RoundToInt(materialBlendMode.floatValue))
            {
                case 0: // Normal
                    blendOp.floatValue = (int)BlendOp.Add;
                    srcBlend.floatValue = (int)BlendMode.SrcAlpha;
                    dstBlend.floatValue = (int)BlendMode.OneMinusSrcAlpha;
                    break;
                case 1: // Normal (Pre-Multiplied)
                    blendOp.floatValue = (int)BlendOp.Add;
                    srcBlend.floatValue = (int)BlendMode.One;
                    dstBlend.floatValue = (int)BlendMode.OneMinusSrcAlpha;
                    break;
                case 2: // Multiply
                    blendOp.floatValue = (int)BlendOp.Add;
                    srcBlend.floatValue = (int)BlendMode.DstColor;
                    dstBlend.floatValue = (int)BlendMode.OneMinusSrcAlpha;
                    break;
                case 3: // Linear Burn
                    blendOp.floatValue = (int)BlendOp.ReverseSubtract;
                    srcBlend.floatValue = (int)BlendMode.One;
                    dstBlend.floatValue = (int)BlendMode.One;
                    break;
                case 4: // Screen
                    blendOp.floatValue = (int)BlendOp.Add;
                    srcBlend.floatValue = (int)BlendMode.OneMinusDstColor;
                    dstBlend.floatValue = (int)BlendMode.One;
                    break;
                case 5: // Lighten
                    blendOp.floatValue = (int)BlendOp.Max;
                    srcBlend.floatValue = (int)BlendMode.One;
                    dstBlend.floatValue = (int)BlendMode.One;
                    break;
                case 6: // Additive
                    blendOp.floatValue = (int)BlendOp.Add;
                    srcBlend.floatValue = (int)BlendMode.One;
                    dstBlend.floatValue = (int)BlendMode.One;
                    break;
            }
            EditorUtility.SetDirty(m_targetMaterial);        }

        MaterialProperty tintBlendMode = FindProperty("_TintBlendMode", properties);        EditorGUI.BeginChangeCheck();        materialEditor.ShaderProperty(tintBlendMode, new GUIContent(tintBlendMode.displayName));
        if (EditorGUI.EndChangeCheck())        {            string[] array = m_targetMaterial.shaderKeywords;            List<string> list = new List<string>(array);            switch (Mathf.RoundToInt(tintBlendMode.floatValue))
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

        MaterialProperty useStroke = FindProperty("_UseStroke", properties);        EditorGUI.BeginChangeCheck();        materialEditor.ShaderProperty(useStroke, new GUIContent(useStroke.displayName));
        if (EditorGUI.EndChangeCheck())        {            string[] array = m_targetMaterial.shaderKeywords;            List<string> list = new List<string>(array);            if (useStroke.floatValue > 0.5f) list.Add("_USE_STROKE");            else list.Remove("_USE_STROKE");            m_targetMaterial.shaderKeywords = list.ToArray();            EditorUtility.SetDirty(m_targetMaterial);        }
        if (useStroke.floatValue > 0.5)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            MaterialProperty strokeColor = FindProperty("_StrokeColor", properties);
            materialEditor.ShaderProperty(strokeColor, new GUIContent(strokeColor.displayName));            MaterialProperty strokeWidth = FindProperty("_StrokeWidth", properties);
            materialEditor.ShaderProperty(strokeWidth, new GUIContent(strokeWidth.displayName));            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        MaterialProperty useColorReplace = FindProperty("_UseColorReplace", properties);        EditorGUI.BeginChangeCheck();        materialEditor.ShaderProperty(useColorReplace, new GUIContent(useColorReplace.displayName));
        if (EditorGUI.EndChangeCheck())        {            string[] array = m_targetMaterial.shaderKeywords;            List<string> list = new List<string>(array);            if (useColorReplace.floatValue > 0.5f) list.Add("_USE_COLORREPLACE");            else list.Remove("_USE_COLORREPLACE");            m_targetMaterial.shaderKeywords = list.ToArray();            EditorUtility.SetDirty(m_targetMaterial);        }
        if (useColorReplace.floatValue > 0.5)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            MaterialProperty colorReplaceSource = FindProperty("_ColorReplaceSource", properties);
            materialEditor.ShaderProperty(colorReplaceSource, new GUIContent(colorReplaceSource.displayName));            MaterialProperty colorReplaceTarget = FindProperty("_ColorReplaceTarget", properties);
            materialEditor.ShaderProperty(colorReplaceTarget, new GUIContent(colorReplaceTarget.displayName));            MaterialProperty colorReplaceFuzziness = FindProperty("_ColorReplaceFuzziness", properties);
            materialEditor.ShaderProperty(colorReplaceFuzziness, new GUIContent(colorReplaceFuzziness.displayName));            EditorGUILayout.EndVertical();            EditorGUILayout.Space();
        }

        MaterialProperty useDuochrome = FindProperty("_UseDuochrome", properties);        EditorGUI.BeginChangeCheck();        materialEditor.ShaderProperty(useDuochrome, new GUIContent(useDuochrome.displayName));
        if (EditorGUI.EndChangeCheck())        {            string[] array = m_targetMaterial.shaderKeywords;            List<string> list = new List<string>(array);            if (useDuochrome.floatValue > 0.5f) list.Add("_USE_DUOCHROME");            else list.Remove("_USE_DUOCHROME");            m_targetMaterial.shaderKeywords = list.ToArray();            EditorUtility.SetDirty(m_targetMaterial);        }
        if (useDuochrome.floatValue > 0.5)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            MaterialProperty duochromeMin = FindProperty("_DuochromeMin", properties);
            materialEditor.ShaderProperty(duochromeMin, new GUIContent(duochromeMin.displayName));            MaterialProperty duochromeMax = FindProperty("_DuochromeMax", properties);
            materialEditor.ShaderProperty(duochromeMax, new GUIContent(duochromeMax.displayName));            EditorGUILayout.EndVertical();        }
    }}