Shader "Yontalane/Water"
{
    Properties
    {
        _Color ("Color", Color) = (0,0.5,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0

        _DepthTest("Depth Test", float) = 1.5

        _FoamColor ("Foam Color", Color) = (1,1,1,1)
        _FoamCutoff("Foam Depth", float) = 0.1
        _FoamAnimTex ("Foam Animation", 2D) = "black" {}

        _FogCutoffStart("Fog Start", float) = 0.75
        _FogCutoffEnd("Fog End", float) = 0.95
        _FogColor ("Fog Color", Color) = (0,0.5,0.75,1)

        _CausticsTex ("Caustics", 2D) = "black" {}
        _CausticsColor ("Caustics Color", Color) = (0.25,0.75,1,1)

		_HeightTex ("Height", 2D) = "black" {}
		_HeightScalar ("Height Scalar", float) = 0.125
		_HeightCoordScale ("Height Coord Scalar", float) = 0.5
		_HeightTimeScale ("Height Time Scalar", float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard vertex:vert alpha
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _CameraDepthTexture;
        float4 _CameraDepthTexture_TexelSize;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
            float3 worldPos;
        };

		sampler2D _HeightTex;
		half _HeightScalar;
		half _HeightCoordScale;
		half _HeightTimeScale;

		void vert (inout appdata_full v)
		{
			half3 coordA = v.vertex.xyz * _HeightCoordScale;
			coordA.x += _Time.x * _HeightTimeScale;
			coordA.y += _Time.x * 0.75 * _HeightTimeScale;
			coordA.z -= _Time.y * _HeightTimeScale;
			half valA = tex2Dlod (_HeightTex, half4(coordA.x, coordA.z, coordA.x, coordA.z)).r;

			half3 coordB = v.vertex.xyz * _HeightCoordScale;
			coordB.y += _Time.x * _HeightTimeScale;
			coordB.z += _Time.x * 0.75 * _HeightTimeScale;
			coordB.x -= _Time.y * _HeightTimeScale;
			half valB = tex2Dlod (_HeightTex, half4(coordB.x, coordB.z, coordB.x, coordB.z)).r;

			half val = valA * valB;

			v.vertex.xyz += v.normal * _HeightScalar * (val * 2 - 1);
		}

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        float _DepthTest;

        fixed4 _FoamColor;
        half _FoamCutoff;
        sampler2D _FoamAnimTex;

        float _FogCutoffStart;
        float _FogCutoffEnd;
        fixed4 _FogColor;

        sampler2D _CausticsTex;
        fixed4 _CausticsColor;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            c.a = _Color.a;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;

            half causticsA = tex2D (_CausticsTex, IN.uv_MainTex + float2(frac(_Time.x), 1)).r;
            half causticsB = tex2D (_CausticsTex, IN.uv_MainTex * 2 - float2(frac(_Time.x * 2), 1)).r + 0.075;
            half causticsC = tex2D (_CausticsTex, IN.uv_MainTex + float2(1, frac(_Time.x * 1.5))).r;
            half caustics = causticsA * causticsB * causticsC;
            caustics = saturate(lerp(0.5, caustics + 0.4, 5));
            caustics += caustics > 0.1125 ? caustics : 0;
            o.Albedo = lerp(o.Albedo, _CausticsColor, caustics * _CausticsColor.a);

            half depthTest = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));
            depthTest = LinearEyeDepth(depthTest);
            depthTest = saturate((depthTest - IN.screenPos.w) / _DepthTest);

            half foamCutoff = _FoamCutoff * lerp(0.75, 1, _SinTime.w);

            if (depthTest < foamCutoff)
            {
                half foamMask = tex2D (_FoamAnimTex, half2(frac(_Time.x), depthTest / foamCutoff)).r;
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
                o.Alpha += caustics * 0.1;
            }

            o.Albedo = lerp(1, o.Albedo, lerp(depthTest, 1, 0.5));
            o.Alpha = lerp(1, o.Alpha, lerp(depthTest, 1, 0.5));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
