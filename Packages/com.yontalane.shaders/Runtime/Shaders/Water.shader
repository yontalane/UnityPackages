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

        _CausticsTex ("Caustics", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard alpha
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

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        float _DepthTest;

        fixed4 _FoamColor;
        float _FoamCutoff;
        sampler2D _FoamAnimTex;

        float _FogCutoffStart;
        float _FogCutoffEnd;

        sampler2D _CausticsTex;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            c.a = _Color.a;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;

            float causticsA = tex2D (_CausticsTex, IN.uv_MainTex + float2(frac(_Time.x), 1)).r;
            float causticsB = tex2D (_CausticsTex, IN.uv_MainTex * 2 - float2(frac(_Time.x * 2), 1)).r + 0.075;
            float causticsC = tex2D (_CausticsTex, IN.uv_MainTex + float2(1, frac(_Time.x * 1.5))).r;
            float caustics = causticsA * causticsB * causticsC;
            caustics = saturate(lerp(0.5, caustics + 0.4, 5));
            caustics += caustics > 0.1125 ? caustics : 0;
            o.Albedo += caustics;

            float depthTest = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));
            depthTest = LinearEyeDepth(depthTest);
            depthTest = saturate((depthTest - IN.screenPos.w) / _DepthTest);

            float foamCutoff = _FoamCutoff * lerp(0.75, 1, _SinTime.w);

            if (depthTest < foamCutoff)
            {
                float bla = tex2D (_FoamAnimTex, half2(frac(_Time.x), depthTest / foamCutoff)).r;
                o.Albedo = lerp(o.Albedo, _FoamColor, bla);
                o.Alpha = lerp(o.Alpha, 1, bla);
            }
            else
            {
                float t = depthTest - _FogCutoffStart;
                t /= (_FogCutoffEnd - _FogCutoffStart);
                o.Alpha = lerp(o.Alpha, 1, saturate(t));
                o.Alpha += caustics * 0.1;
            }

            o.Albedo = lerp(1, o.Albedo, lerp(depthTest, 1, 0.5));
            o.Alpha = lerp(1, o.Alpha, lerp(depthTest, 1, 0.5));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
