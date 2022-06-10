Shader "Yontalane/Water"
{
    Properties
    {
        _Color ("Color", Color) = (0,0.5,1,1)
        [Toggle] _UseMain ("Common", Float) = 0
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0

        _DepthTest("Depth Test", float) = 1.5

        [Toggle] _UseFoam ("Foam", Float) = 0
        _FoamColor ("Color", Color) = (1,1,1,1)
        _FoamCutoff("Depth", float) = 0.1
        [Toggle] _UseFoamAnimTex ("Animation", Float) = 0
        [NoScaleOffset] _FoamAnimTex ("Texture", 2D) = "black" {}

        [Toggle] _UseFog ("Fog", Float) = 0
        _FogCutoffStart("Near", float) = 0.75
        _FogCutoffEnd("Far", float) = 0.95
        _FogColor ("Color", Color) = (0,0.5,0.75,1)

        [Toggle] _UseCaustics ("Caustics", Float) = 0
        [NoScaleOffset] _CausticsTex ("Texture", 2D) = "black" {}
        [HDR] _CausticsColor ("Color", Color) = (0.25,0.75,1,1)

        [Toggle] _UseHeight ("Height", Float) = 0
		[NoScaleOffset] _HeightTex ("Texture", 2D) = "black" {}
		_HeightScalar ("Height Scale", float) = 0.125
		_HeightCoordScalar ("UV Scale", float) = 0.5
		_HeightTimeScalar ("Time Scale", float) = 0.5
    }

    CustomEditor "Yontalane.Shaders.WaterGUI"

    SubShader
    {
        Tags
		{
			"RenderType"="Transparent"
			"Queue"="Transparent"
			"ForceNoShadowCasting"="True"
		}
		
        ZWrite Off
        LOD 200

        CGPROGRAM
            #pragma shader_feature _ _USE_MAIN
            #pragma shader_feature _ _USE_FOAM
            #pragma shader_feature _ _USE_FOAM_ANIM_TEX
            #pragma shader_feature _ _USE_FOG
            #pragma shader_feature _ _USE_CAUSTICS
            #pragma shader_feature _ _USE_HEIGHT
			
			#pragma surface surf Standard vertex:vert alpha
			#pragma target 3.0
			
			#if (_USE_MAIN)
				sampler2D _MainTex;
			#endif
			#if (_USE_FOAM || _USE_FOG)
				sampler2D _CameraDepthTexture;
				float4 _CameraDepthTexture_TexelSize;
			#endif

			struct Input
			{
				#if (_USE_MAIN)
					float2 uv_MainTex;
				#endif
				#if (_USE_CAUSTICS)
					float2 uv_CausticsTex;
				#endif
				float4 screenPos;
				float3 worldPos;
			};

			#if (_USE_HEIGHT)
				sampler2D _HeightTex;
				half _HeightScalar;
				half _HeightCoordScalar;
				half _HeightTimeScalar;

				void vert (inout appdata_full v)
				{
					half3 coordA = v.vertex.xyz * _HeightCoordScalar;
					coordA.x = coordA.x + _Time.x * _HeightTimeScalar;
					coordA.y = coordA.y - _Time.y * _HeightTimeScalar;
					half valA = tex2Dlod (_HeightTex, half4(coordA.xy, 0, 0)).r;

					half3 coordB = v.vertex.xyz * _HeightCoordScalar;
					coordB.x = coordB.x - _Time.y * _HeightTimeScalar;
					coordB.y = coordB.z + _Time.x * _HeightTimeScalar;
					half valB = tex2Dlod (_HeightTex, half4(coordB.xy, 0, 0)).r;

					half val = valA * valB;

					v.vertex.xyz += v.normal * _HeightScalar * (val * 2 - 1);
				}
			#else
				void vert (inout appdata_full v)
				{
				}
			#endif

			fixed4 _Color;
			
			#if (_USE_MAIN)
				half _Glossiness;
				half _Metallic;
			#endif

			#if (_USE_FOAM || _USE_FOG)
				float _DepthTest;
			#endif

			#if (_USE_FOAM)
				fixed4 _FoamColor;
				half _FoamCutoff;
				#if (_USE_FOAM_ANIM_TEX)
					sampler2D _FoamAnimTex;
				#endif
			#endif

			#if (_USE_FOG)
				float _FogCutoffStart;
				float _FogCutoffEnd;
				fixed4 _FogColor;
			#endif

			#if (_USE_CAUSTICS)
				sampler2D _CausticsTex;
				fixed4 _CausticsColor;
			#endif

			void surf (Input IN, inout SurfaceOutputStandard o)
			{
				#if (_USE_MAIN)
					fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
					c.a = _Color.a;
					o.Albedo = c.rgb;
					o.Metallic = _Metallic;
					o.Smoothness = _Glossiness;
					o.Alpha = c.a;
				#else
					fixed4 c = _Color;
					o.Albedo = c.rgb;
					o.Alpha = c.a;
				#endif

				#if (_USE_CAUSTICS)
					half causticsA = tex2D (_CausticsTex, IN.uv_CausticsTex + float2(frac(_Time.x), 1)).r;
					half causticsB = tex2D (_CausticsTex, IN.uv_CausticsTex * 2 - float2(frac(_Time.x * 2), 1)).r + 0.075;
					half causticsC = tex2D (_CausticsTex, IN.uv_CausticsTex + float2(1, frac(_Time.x * 1.5))).r;
					half caustics = causticsA * causticsB * causticsC;
					caustics = saturate(lerp(0.5, caustics + 0.4, 5));
					caustics += caustics > 0.1125 ? caustics : 0;
					o.Albedo = lerp(o.Albedo, _CausticsColor, caustics * _CausticsColor.a);
				#endif

				#if (_USE_FOAM || _USE_FOG)
					half depthTest = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));
					depthTest = LinearEyeDepth(depthTest);
					depthTest = saturate((depthTest - IN.screenPos.w) / _DepthTest);
				#endif

				#if (_USE_FOAM && _USE_FOG)
					half foamCutoff = _FoamCutoff * lerp(0.75, 1, _SinTime.w);
					if (depthTest < foamCutoff)
					{
						#if (_USE_FOAM_ANIM_TEX)
							half foamMask = tex2D (_FoamAnimTex, half2(frac(_Time.x), depthTest / foamCutoff)).r;
						#else
							half foamMask = 1;
						#endif
						o.Albedo = lerp(o.Albedo, _FoamColor, foamMask * _FoamColor.a);
						o.Alpha = lerp(o.Alpha, 1, foamMask);
					}
					else
					{
						half t = depthTest - _FogCutoffStart;
						t /= (_FogCutoffEnd - _FogCutoffStart);
						t = saturate(t);
						o.Albedo = lerp(o.Albedo, _FogColor, t * _FogColor.a);
						o.Alpha = lerp(o.Alpha, 1, t);
					}
				#elif (_USE_FOAM)
					half foamCutoff = _FoamCutoff * lerp(0.75, 1, _SinTime.w);
					if (depthTest < foamCutoff)
					{
						#if (_USE_FOAM_ANIM_TEX)
							half foamMask = tex2D (_FoamAnimTex, half2(frac(_Time.x), depthTest / foamCutoff)).r;
						#else
							half foamMask = 1;
						#endif
						o.Albedo = lerp(o.Albedo, _FoamColor, foamMask * _FoamColor.a);
						o.Alpha = lerp(o.Alpha, 1, foamMask);
					}
				#elif (_USE_FOG)
					half t = depthTest - _FogCutoffStart;
					t /= (_FogCutoffEnd - _FogCutoffStart);
					t = saturate(t);
					o.Albedo = lerp(o.Albedo, _FogColor, t * _FogColor.a);
					o.Alpha = lerp(o.Alpha, 1, t);
				#endif
				
				#if (_USE_CAUSTICS)
					o.Alpha = saturate(o.Alpha + caustics * 0.1);
				#endif

				#if (_USE_FOAM || _USE_FOG)
					o.Albedo = lerp(1, o.Albedo, lerp(depthTest, 1, 0.5));
					o.Alpha = lerp(1, o.Alpha, lerp(depthTest, 1, 0.5));
				#endif
			}
        ENDCG
    }
    FallBack "Diffuse"
}
