Shader "Yontalane/Sprites/Base"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,0)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        
        [KeywordEnum(Normal, Premultiplied, Multiply, LinearBurn, Lighten, Screen, LinearDodge)] _MaterialBlendMode("Material Blend Mode", float) = 0
        [KeywordEnum(Normal, Darken, Multiply, ColorBurn, LinearBurn, DarkerColor, Lighten, Screen, ColorDodge, LinearDodge, LighterColor, Overlay, SoftLight, HardLight, VividLight, LinearLight, Difference, Subtract, Divide)] _TintBlendMode("Tint Blend Mode", float) = 2
        
        [Toggle] _UseStroke ("Stroke", Float) = 0
        _StrokeColor ("Color", Color) = (0,0,0,1)
        _StrokeWidth ("Width", Int) = 1

        [Toggle] _UseColorReplace ("Color Replace", Float) = 0
        _ColorReplaceSource ("Replace this", Color) = (0,0,0,1)
        _ColorReplaceTarget ("With this", Color) = (1,1,1,1)
        _ColorReplaceFuzziness ("Fuzziness", Range(0.0, 1.0)) = 0.01

        [Toggle] _UseDuochrome ("Duochrome", Float) = 0
        _DuochromeMin ("Black becomes", Color) = (0,0,0,1)
        _DuochromeMax ("White becomes", Color) = (1,1,1,1)

        [Enum(UnityEngine.Rendering.BlendModeOp)] _BlendOp ("Blend Op", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source Blend Mode", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Destination Blend Mode", Float) = 10
    }

    CustomEditor "SpriteBaseGUI"

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        BlendOp [_BlendOp]
        Blend [_SrcBlend] [_DstBlend]

        Pass
        {
        CGPROGRAM
            #pragma shader_feature _ _TINTBLENDMODE_DARKEN
            #pragma shader_feature _ _TINTBLENDMODE_MULTIPLY
            #pragma shader_feature _ _TINTBLENDMODE_COLORBURN
            #pragma shader_feature _ _TINTBLENDMODE_LINEARBURN
            #pragma shader_feature _ _TINTBLENDMODE_DARKERCOLOR

            #pragma shader_feature _ _TINTBLENDMODE_LIGHTEN
            #pragma shader_feature _ _TINTBLENDMODE_SCREEN
            #pragma shader_feature _ _TINTBLENDMODE_COLORDODGE
            #pragma shader_feature _ _TINTBLENDMODE_LINEARDODGE
            #pragma shader_feature _ _TINTBLENDMODE_LIGHTERCOLOR

            #pragma shader_feature _ _TINTBLENDMODE_OVERLAY
            #pragma shader_feature _ _TINTBLENDMODE_SOFTLIGHT
            #pragma shader_feature _ _TINTBLENDMODE_HARDLIGHT
            #pragma shader_feature _ _TINTBLENDMODE_VIVIDLIGHT
            #pragma shader_feature _ _TINTBLENDMODE_LINEARLIGHT

            #pragma shader_feature _ _TINTBLENDMODE_DIFFERENCE
            #pragma shader_feature _ _TINTBLENDMODE_SUBTRACT
            #pragma shader_feature _ _TINTBLENDMODE_DIVIDE

            #pragma shader_feature _ _USE_STROKE
            #pragma shader_feature _ _USE_COLORREPLACE
            #pragma shader_feature _ _USE_DUOCHROME

            #pragma vertex SpriteVert
            #pragma fragment Frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            #if (_USE_STROKE)
                fixed4 _MainTex_TexelSize;
                fixed4 _StrokeColor;
                float _StrokeWidth;

                float AdjacentAlpha(float2 uv, float x, float y)
                {
                    return tex2D
                    (
                        _MainTex,
                        float2
                        (
                            clamp(uv.x + _MainTex_TexelSize.x * x * _StrokeWidth, 0.0, 1.0),
                            clamp(uv.y + _MainTex_TexelSize.y * y * _StrokeWidth, 0.0, 1.0)
                        )
                    ).a;
                }
            #endif

            #if (_USE_COLORREPLACE)
                fixed4 _ColorReplaceSource;
                fixed4 _ColorReplaceTarget;
                float _ColorReplaceFuzziness;
            #endif

            #if (_USE_DUOCHROME)
                fixed4 _DuochromeMin;
                fixed4 _DuochromeMax;
            #endif

            fixed4 ExtrasBeforeTint(v2f IN, fixed4 c)
            {
                #if (_USE_DUOCHROME)
                    half4 duochrome = lerp(_DuochromeMin, _DuochromeMax, c.r);
                    duochrome.a *= c.a;
                    c = duochrome;
                #endif

                return c;
            }

            fixed4 ExtrasAfterTint(v2f IN, fixed4 c)
            {
                #if (_USE_STROKE)
                    float test = 0.5 * IN.color.a;
                    if (c.a < test)
                    {
                        fixed4 stroke = fixed4(_StrokeColor.r, _StrokeColor.g, _StrokeColor.b, _StrokeColor.a * IN.color.a);
                        stroke.rgb *= stroke.a;
                        if (AdjacentAlpha(IN.texcoord, -1, 0) > test) return stroke;
                        if (AdjacentAlpha(IN.texcoord, 1, 0) > test) return stroke;
                        if (AdjacentAlpha(IN.texcoord, 0, -1) > test) return stroke;
                        if (AdjacentAlpha(IN.texcoord, 0, 1) > test) return stroke;
                    }
                #endif

                #if (_USE_COLORREPLACE)
                    if (c.a > 0.0 && abs(c.r - _ColorReplaceSource.r) < _ColorReplaceFuzziness && abs(c.g - _ColorReplaceSource.g) < _ColorReplaceFuzziness && abs(c.b - _ColorReplaceSource.b) < _ColorReplaceFuzziness)
                    {
                        c.r = _ColorReplaceTarget.r;
                        c.g = _ColorReplaceTarget.g;
                        c.b = _ColorReplaceTarget.b;
                        c.a = _ColorReplaceTarget.a;
                    }
                #endif

                return c;
            }

            #if (_TINTBLENDMODE_MULTIPLY)
                fixed4 AdjustColor(fixed4 c, v2f IN) : SV_Target
                {
                    c *= IN.color;
                    return c;
                }
            #elif (_TINTBLENDMODE_LINEARDODGE)
                float AdjustChannel(float target, float blend)
                {
                    return 1.0 - (1.0 - 2.0 * (target - 0.5)) * (1.0 - blend);
                }

                fixed4 AdjustColor(fixed4 c, v2f IN) : SV_Target
                {
                    c.r = AdjustChannel(c.r, IN.color.r);
                    c.g = AdjustChannel(c.g, IN.color.g);
                    c.b = AdjustChannel(c.b, IN.color.b);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_OVERLAY)
                float AdjustChannel(float target, float blend)
                {
                    if (target >= 0.5)
                    {
                        return 1.0 - (1.0 - 2.0 * (target - 0.5)) * (1.0 - blend);
                    }
                    else
                    {
                        return (2.0 * target) * blend;
                    }
                }

                fixed4 AdjustColor(fixed4 c, v2f IN) : SV_Target
                {
                    c.r = AdjustChannel(c.r, IN.color.r);
                    c.g = AdjustChannel(c.g, IN.color.g);
                    c.b = AdjustChannel(c.b, IN.color.b);
                    c.a *= IN.color.a;
                    return c;
                }
            #else // Normal
                fixed4 AdjustColor(fixed4 c, v2f IN) : SV_Target
                {
                    c.rgb = IN.color.rgb;
                    return c;
                }
            #endif

            fixed4 Frag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture (IN.texcoord);
                c = ExtrasBeforeTint(IN, c);
                c = AdjustColor(c, IN);
                c.rgb *= c.a;
                c = ExtrasAfterTint(IN, c);
                return c;
            }
        ENDCG
        }
    }
}
