Shader "Yontalane/Screen/Fade"
{
	Properties
	{
		_MainTex("Screen Texture", 2D) = "white" {}
		_Dark("Darken", Range(0.0, 1.0)) = 0.5
		_Sat("Saturation", Range(0.0, 1.0)) = 0.5
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
			float _Dark;
			float _Sat;

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
				float avg = (c.r + c.g + c.b) * 0.333;
				fixed4 gray = fixed4(
					avg,
					avg,
					avg,
					c.a
				);
				c = lerp(gray, c, _Sat);
				c = lerp(c, fixed4(0, 0, 0, c.a), _Dark);
				return c;
			}
			ENDCG
		}
	}
}