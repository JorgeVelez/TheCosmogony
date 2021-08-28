Shader "Shader/RaindropFX/Subtract" {
    Properties {
		_MainTex("Base", 2D) = "" {}
		_WipeTex("Wipe", 2D) = "" {}
	}

	CGINCLUDE

	#include "UnityCG.cginc"

	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};

	sampler2D _MainTex;
	sampler2D _WipeTex;

	v2f vert(appdata_img v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		return o;
	}

	half4 frag(v2f i) : COLOR {
		float4 mainColor = tex2D(_MainTex, i.uv);
		float4 wipeColor = tex2D(_WipeTex, i.uv);

		float dist = mainColor.r - wipeColor.r + 
			mainColor.g - wipeColor.g + 
			mainColor.b - wipeColor.b;

		return (dist > 0.02 ? 1.0 : 0.0);
	}

	ENDCG

	Subshader {
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog {
				Mode off
			}

			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}

	Fallback off
}
