Shader "Triller/Block"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_FadeTex ("Fade Mask", 2D) = "white" {}
		_Fade ("Fade", Range(0, 1)) = 0
        _Blink ("Blink", Range(0, 1)) = 0
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
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _FadeTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			fixed _Fade;
			fixed _Blink;

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.pos);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                fixed4 mask = tex2D(_FadeTex, i.uv);
                // col.a = (1 - _Fade) *  mask.r;
                // col.a = 1 - mask.r - _Fade;
                // clip(col.a - 0.001);
                clip(1 - mask.r - _Fade);

                col.a = 1 - _Blink;
				return col;
			}
			ENDCG
		}
	}
}
