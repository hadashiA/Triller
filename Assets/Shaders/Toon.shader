Shader "Heipu/Toon"
{
	Properties
	 {
	    _Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" { }
		_ShadeThreshold ("Shade Threshold", Range(0.1, 0.9)) = 0.25
	}

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Name "BASE"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _ShadeThreshold;
            float4 _LightColor0;

            struct appdata
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.pos);
                o.normal = v.normal;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                float3 lightDir = mul(unity_WorldToObject, _WorldSpaceLightPos0).xyz;
                float diffuse = max(0, dot(i.normal, lightDir));

                float4 c = tex2D(_MainTex, i.uv) * _Color * _LightColor0;
                if (diffuse < _ShadeThreshold)
                {
                    c *= (1 - _ShadeThreshold);
                }
                return c;
            }
            ENDCG
        }
	}
}
