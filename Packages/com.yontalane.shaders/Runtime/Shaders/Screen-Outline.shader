Shader "Hidden/Yontalane/Screen/Outline"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}

        _Color ("Color", Color) = (0,0,0,1)

        _NormalMultiplier ("Normal Multiplier", Range(0,4)) = 1
        _NormalBias ("Normal Bias", Range(1,4)) = 1

        _DepthMultiplier ("Depth Multiplier", Range(0,4)) = 1
        _DepthBias ("Depth Bias", Range(1,4)) = 1

        _Cutoff ("Cutoff", float) = 0
    }

    SubShader
    {
        Cull Off
        ZWrite Off 
        ZTest Always

        Pass
        {
            CGPROGRAM
                #include "UnityCG.cginc"

                #pragma vertex vert
                #pragma fragment frag

                sampler2D _MainTex;
                sampler2D _CameraDepthNormalsTexture;
                float4 _CameraDepthNormalsTexture_TexelSize;

                float4 _Color;
                float _NormalMultiplier;
                float _NormalBias;
                float _DepthMultiplier;
                float _DepthBias;
                float _Cutoff;

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 position : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                v2f vert(appdata v)
                {
                    v2f OUT;
                    OUT.position = UnityObjectToClipPos(v.vertex);
                    OUT.uv = v.uv;
                    return OUT;
                }

                void Compare(inout float depthOutline, inout float normalOutline, float baseDepth, float3 baseNormal, float2 uv, float2 offset)
                {
                    float4 neighborDepthNormal = tex2D(_CameraDepthNormalsTexture, uv + _CameraDepthNormalsTexture_TexelSize.xy * offset);
                    float neighborDepth;
                    float3 neighborNormal;
                    DecodeDepthNormal(neighborDepthNormal, neighborDepth, neighborNormal);
                    neighborDepth = neighborDepth * _ProjectionParams.z;

                    float depthDiff = baseDepth - neighborDepth;
                    depthOutline = depthOutline + depthDiff;

                    float3 normalDiff = baseNormal - neighborNormal;
                    normalDiff = normalDiff.r + normalDiff.g + normalDiff.b;
                    normalOutline = normalOutline + normalDiff;
                }

                fixed4 frag(v2f IN) : SV_TARGET
                {
                    float4 depthNormal = tex2D(_CameraDepthNormalsTexture, IN.uv);
                    float depth;
                    float3 normal;
                    DecodeDepthNormal(depthNormal, depth, normal);

                    depth = depth * _ProjectionParams.z;

                    float depthDiff = 0;
                    float normalDiff = 0;

                    Compare(depthDiff, normalDiff, depth, normal, IN.uv, float2(1, 0));
                    Compare(depthDiff, normalDiff, depth, normal, IN.uv, float2(0, 1));
                    Compare(depthDiff, normalDiff, depth, normal, IN.uv, float2(0, -1));
                    Compare(depthDiff, normalDiff, depth, normal, IN.uv, float2(-1, 0));

                    depthDiff = depthDiff * _DepthMultiplier;
                    depthDiff = saturate(depthDiff);
                    depthDiff = pow(depthDiff, _DepthBias);

                    normalDiff = normalDiff * _NormalMultiplier;
                    normalDiff = saturate(normalDiff);
                    normalDiff = pow(normalDiff, _NormalBias);

                    float outline = normalDiff + depthDiff;
                    outline = outline > _Cutoff ? outline : 0;
                    float4 c = tex2D(_MainTex, IN.uv);
                    c = lerp(c, _Color, outline);
                    return c;
                }
            ENDCG
        }
    }
}