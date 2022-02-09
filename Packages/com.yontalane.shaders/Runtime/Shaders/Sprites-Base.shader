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
        
        [KeywordEnum(Normal, Premultiplied, Multiply, LinearBurn, Lighten, Screen, LinearDodge)] _MaterialBlendMode("Sprite Blend Mode", float) = 0
        [KeywordEnum(Normal, Darken, Multiply, ColorBurn, LinearBurn, DarkerColor, Lighten, Screen, ColorDodge, LinearDodge, LighterColor, Overlay, SoftLight, HardLight, VividLight, LinearLight, PinLight, HardMix, Difference, Exclusion, Subtract, Divide, Hue, Saturation, Color, Luminosity)] _TintBlendMode("Tint Blend Mode", float) = 2
        
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
            // Darken
            #pragma shader_feature _ _TINTBLENDMODE_DARKEN
            #pragma shader_feature _ _TINTBLENDMODE_MULTIPLY
            #pragma shader_feature _ _TINTBLENDMODE_COLORBURN
            #pragma shader_feature _ _TINTBLENDMODE_LINEARBURN
            #pragma shader_feature _ _TINTBLENDMODE_DARKERCOLOR
            // Lighten
            #pragma shader_feature _ _TINTBLENDMODE_LIGHTEN
            #pragma shader_feature _ _TINTBLENDMODE_SCREEN
            #pragma shader_feature _ _TINTBLENDMODE_COLORDODGE
            #pragma shader_feature _ _TINTBLENDMODE_LINEARDODGE
            #pragma shader_feature _ _TINTBLENDMODE_LIGHTERCOLOR
            // Contrast
            #pragma shader_feature _ _TINTBLENDMODE_OVERLAY
            #pragma shader_feature _ _TINTBLENDMODE_SOFTLIGHT
            #pragma shader_feature _ _TINTBLENDMODE_HARDLIGHT
            #pragma shader_feature _ _TINTBLENDMODE_VIVIDLIGHT
            #pragma shader_feature _ _TINTBLENDMODE_LINEARLIGHT
            #pragma shader_feature _ _TINTBLENDMODE_PINLIGHT
            #pragma shader_feature _ _TINTBLENDMODE_HARDMIX
            // Inversion
            #pragma shader_feature _ _TINTBLENDMODE_DIFFERENCE
            #pragma shader_feature _ _TINTBLENDMODE_EXCLUSION
            // Cancelation
            #pragma shader_feature _ _TINTBLENDMODE_SUBTRACT
            #pragma shader_feature _ _TINTBLENDMODE_DIVIDE
            // Component
            #pragma shader_feature _ _TINTBLENDMODE_HUE
            #pragma shader_feature _ _TINTBLENDMODE_SATURATION
            #pragma shader_feature _ _TINTBLENDMODE_COLOR
            #pragma shader_feature _ _TINTBLENDMODE_LUMINOSITY
            // Extras
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
            #include "BlendModes.cginc"

            #if (_USE_STROKE)
                half4 _MainTex_TexelSize;
                half4 _StrokeColor;
                half _StrokeWidth;

                half AdjacentAlpha(float2 uv, float x, float y)
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
                half4 _ColorReplaceSource;
                half4 _ColorReplaceTarget;
                half _ColorReplaceFuzziness;
            #endif

            #if (_USE_DUOCHROME)
                half4 _DuochromeMin;
                half4 _DuochromeMax;
            #endif

            half4 ExtrasBeforeTint(v2f IN, half4 c)
            {
                #if (_USE_DUOCHROME)
                    half4 duochrome = lerp(_DuochromeMin, _DuochromeMax, c.r);
                    duochrome.a *= c.a;
                    c = duochrome;
                #endif

                return c;
            }

            half4 ExtrasAfterTint(v2f IN, half4 c)
            {
                #if (_USE_STROKE)
                    half test = 0.5 * IN.color.a;
                    if (c.a < test)
                    {
                        half4 stroke = half4(_StrokeColor.r, _StrokeColor.g, _StrokeColor.b, _StrokeColor.a * IN.color.a);
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

            // Darken

            #if (_TINTBLENDMODE_DARKEN)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendDarken(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_MULTIPLY)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendMultiply(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_COLORBURN)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendColorBurn(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_LINEARBURN)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendLinearBurn(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_DARKERCOLOR)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendDarkerColor(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
                
            // Lighten

            #elif (_TINTBLENDMODE_LIGHTEN)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendLighten(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_SCREEN)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendScreen(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_COLORDODGE)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendColorDodge(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_LINEARDODGE)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendLinearDodge(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_LIGHTERCOLOR)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendLighterColor(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
                
            // Contrast

            #elif (_TINTBLENDMODE_OVERLAY)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendOverlay(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_SOFTLIGHT)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendSoftLight(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_HARDLIGHT)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendHardLight(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_VIVIDLIGHT)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendVividLight(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_LINEARLIGHT)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendLinearLight(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_PINLIGHT)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendPinLight(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_HARDMIX)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendHardMix(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
                
            // Inversion

            #elif (_TINTBLENDMODE_DIFFERENCE)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendDifference(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_EXCLUSION)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendExclusion(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }

            // Cancelation

            #elif (_TINTBLENDMODE_SUBTRACT)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendSubtract(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_DIVIDE)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendDivide(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
                
            // Component

            #elif (_TINTBLENDMODE_HUE)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendHue(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_SATURATION)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendSaturation(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_COLOR)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendColor(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #elif (_TINTBLENDMODE_LUMINOSITY)
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendLuminosity(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
                
            // Normal

            #else
                half4 AdjustColor(half4 c, v2f IN)
                {
                    c.rgb = BlendNormal(IN.color.rgb, c.rgb, 1.0);
                    c.a *= IN.color.a;
                    return c;
                }
            #endif

            half4 Frag(v2f IN) : SV_Target
            {
                half4 c = SampleSpriteTexture (IN.texcoord);
                c = ExtrasBeforeTint(IN, c);
                if (c.a != 0.0 && IN.color.a != 0.0)
                    c = AdjustColor(c, IN);
                c.rgb *= c.a;
                c = ExtrasAfterTint(IN, c);
                return c;
            }
        ENDCG
        }
    }
}
