Shader "Hidden/Yontalane/Screen/Pixelate"
{
	Properties
	{
		_MainTex("Screen Texture", 2D) = "white" {}
		_Amount("Dimensions", Int) = 64
	}
	
	SubShader
	{
        Tags
        {
            "PreviewType"="Plane"
        }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			int _Amount;

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
				float2 uv = IN.uv;
				half amount = (half)_Amount;
				uv.x = floor(uv.x * amount) / amount + (1 / amount * 0.5);
				uv.y = floor(uv.y * amount) / amount + (1 / amount * 0.5);
				fixed4 tex_screen = tex2D(_MainTex, uv);
				return tex_screen;
			}
			ENDCG
		}
	}
}