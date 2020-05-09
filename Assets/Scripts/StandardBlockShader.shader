// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Minecraft/Blocks" {
	Properties{
		_MainTex("Block Texture Atlas", 2D) = "white" {}
	}

		SubShader{
			Tags {"Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

		LOD 100
		Lighting Off
		ZWrite Off

		Pass {
		Blend Off
		ZWrite On

			CGPROGRAM
				#pragma vertex vertFunction
				#pragma fragment fragFunction
				#pragma target 2.0

				#include "UnityCG.cginc"

				struct appdata {
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
				};

				sampler2D _MainTex;
				float GlobalLightLevel;

				v2f vertFunction(appdata v) {
					v2f o;

					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					o.color = v.color;

					return o;
				}

				fixed4 fragFunction(v2f i) : SV_Target {
					fixed4 col = tex2D(_MainTex, i.uv);
				if (col.a < 1)
					discard;

				float localLightLevel = clamp(GlobalLightLevel * i.color.a, 0.1, 1);
				col = lerp(float4(0, 0, 0, col.a), col, localLightLevel);

				return col;
		}

		ENDCG
}

Pass {
			Blend SrcAlpha OneMinusSrcAlpha

	CGPROGRAM
		#pragma vertex vertFunction
		#pragma fragment fragFunction
		#pragma target 2.0

		#include "UnityCG.cginc"

		struct appdata {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			float4 color : COLOR;
		};

		struct v2f {
			float4 vertex : SV_POSITION;
			float2 uv : TEXCOORD0;
			float4 color : COLOR;
		};

		sampler2D _MainTex;
		float GlobalLightLevel;

		v2f vertFunction(appdata v) {
			v2f o;

			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv;
			o.color = v.color;

			return o;
		}

		fixed4 fragFunction(v2f i) : SV_Target {
			fixed4 col = tex2D(_MainTex, i.uv);

		if (col.a >= 1)
			discard;

		float localLightLevel = GlobalLightLevel;
		col = lerp(float4(0, 0, 0, col.a), col, localLightLevel);

		return col;
		}

ENDCG
}
	}
}