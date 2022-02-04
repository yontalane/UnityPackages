Shader "Yontalane/Sprites/Stroke"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_Stroke ("Stroke", Color) = (1,0,0,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

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
		Fog { Mode Off }
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DUMMY PIXELSNAP_ON
			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color	: COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color	: COLOR;
				half2 texcoord  : TEXCOORD0;
			};

			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			fixed4 _MainTex_TexelSize;
			fixed4 _Stroke;

			float AdjacentAlpha(float2 uv, float x, float y)
			{
				return tex2D
				(
					_MainTex,
					float2
					(
						clamp(uv.x + _MainTex_TexelSize.x * x, 0.0, 1.0),
						clamp(uv.y + _MainTex_TexelSize.y * y, 0.0, 1.0)
					)
				).a;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
				c.rgb *= c.a;
				float testA = 0.5 * IN.color.a;
				if (c.a < testA)
				{
					fixed4 stroke = fixed4(_Stroke.r, _Stroke.g, _Stroke.b, _Stroke.a * IN.color.a);
					stroke.rgb *= stroke.a;
					if (AdjacentAlpha(IN.texcoord, -1, 0) > testA) return stroke;
					if (AdjacentAlpha(IN.texcoord, 1, 0) > testA) return stroke;
					if (AdjacentAlpha(IN.texcoord, 0, -1) > testA) return stroke;
					if (AdjacentAlpha(IN.texcoord, 0, 1) > testA) return stroke;

					// if (AdjacentAlpha(IN.texcoord, 1, 1) > testA) return stroke;
					// if (AdjacentAlpha(IN.texcoord, -1, -1) > testA) return stroke;
					// if (AdjacentAlpha(IN.texcoord, 1, -1) > testA) return stroke;
					// if (AdjacentAlpha(IN.texcoord, -1, 1) > testA) return stroke;
				}
				return c;
			}
			ENDCG
		}
	}
}
