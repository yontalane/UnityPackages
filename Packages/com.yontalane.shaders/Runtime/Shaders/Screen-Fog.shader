Shader "Hidden/Yontalane/Screen/Fog"
{
	Properties
	{
		_MainTex("Screen Texture", 2D) = "white" { }
		_Start("Start", Range(0.0, 1.0)) = 0.75
		_End("End", Range(0.0, 1.0)) = 0.25
		_Color("Color", Color) = ( 1.0, 1.0, 1.0, 1.0 )
	}

	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			sampler2D _CameraDepthTexture;
			float _Start;
			float _End;
			fixed4 _Color;

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv     : TEXCOORD0;
			};

			v2f vert(appdata_img IN)
			{
				v2f OUT;
				#if UNITY_UV_STARTS_AT_TOP
					if (_MainTex_TexelSize.y < 0)
						IN.texcoord.y = 1 - IN.texcoord.y;
				#endif

				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.texcoord;

				return OUT;
			}

			fixed4 frag(v2f IN) : COLOR
			{
				fixed4 c = tex2D(_MainTex, IN.uv);
				float val = tex2D(_CameraDepthTexture, IN.uv).r;
				val = clamp(val, _End, _Start);
				val -= _End;
				val /= (_Start - _End);
				c = lerp(_Color, c, val);
				return c;
			}
			ENDCG
		}
	}
}