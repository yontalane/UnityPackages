Shader "Yontalane/Surface"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

		[Toggle] _UseOutline ("Outline", Float) = 0.0
        _OutlineColor ("Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Width", Float) = 1.0

    }

    CustomEditor "YontalaneEditor.Shaders.SurfaceBaseGUI"

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
		Blend Off

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input
		{
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG

		Pass
		{
			Name "Outline"
			Blend Off
			Cull Front

			CGPROGRAM

	        #pragma shader_feature _ _USE_OUTLINE

			#pragma vertex vert
			#pragma fragment frag

			#if (_USE_OUTLINE)

				half4 _OutlineColor;
				half _OutlineWidth;

				float4 vert(float4 position : POSITION, float3 normal : NORMAL) : SV_POSITION
				{
					float4 clipPosition = UnityObjectToClipPos(position);
					float3 clipNormal = mul((float3x3) UNITY_MATRIX_VP, mul((float3x3) UNITY_MATRIX_M, normal));

					float2 offset = normalize(clipNormal.xy) / _ScreenParams.xy * _OutlineWidth * _OutlineColor.a * clipPosition.w * 2;
					clipPosition.xy += offset;

					return clipPosition;
				}

				half4 frag() : SV_TARGET
				{
					return _OutlineColor;
				}

			#else

				float4 vert(float4 position : POSITION, float3 normal : NORMAL) : SV_POSITION
				{
					return float4(0,0,0,0);
				}

				half4 frag() : SV_TARGET
				{
					return half4(0,0,0,0);
				}
				
			#endif
			ENDCG
		}
    }
    FallBack "Diffuse"
}
