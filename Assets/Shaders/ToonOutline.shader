Shader "Heipu/ToonOutline"
{
	Properties
	{
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" { }
        _ShadeThreshold ("Shade Threshold", Range(0.1, 0.9)) = 0.25
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _Outline ("Outline width", Range (.002, 0.03)) = .005
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }

		UsePass "Heipu/Toon/BASE"

		Pass
		{
		    Name "OUTLINE"
		    Tags { "LightMode" = "Always" }
		    Cull Front
		    ZWrite On
		    ColorMask RGB
		    Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 pos : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
            };

            uniform float _Outline;
            uniform float4 _OutlineColor;

            v2f vert(appdata v) {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.pos);

                float3 norm   = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
                float2 offset = TransformViewToProjection(norm.xy);

                #ifdef UNITY_Z_0_FAR_FROM_CLIPSPACE //to handle recent standard asset package on older version of unity (before 5.5)
                    o.pos.xy += offset * UNITY_Z_0_FAR_FROM_CLIPSPACE(o.pos.z) * _Outline;
                #else
                    o.pos.xy += offset * o.pos.z * _Outline;
                #endif
                o.color = _OutlineColor;
                return o;
            }

			fixed4 frag (v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
	}
}
