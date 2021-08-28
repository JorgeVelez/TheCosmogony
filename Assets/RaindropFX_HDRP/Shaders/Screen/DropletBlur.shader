Shader "Hidden/Custom/DropletBlur" {
	Properties{
		_MainTex("Base", 2D) = "" {}
	}

	CGINCLUDE

	#include "UnityCG.cginc"

	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};

	sampler2D _MainTex;
	sampler2D _BlurredMainTex;
	sampler2D _MaskMap;
	//sampler2D _DetailMask;

	float _focalize;
	float _smooth;

	v2f vert(appdata_img v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		return o;
	}

	half4 frag(v2f i) : COLOR{
		half4 mask = tex2D(_MaskMap, i.uv);
		//half4 detailMask = tex2D(_DetailMask, i.uv);

		//float greyScale = clamp(pow(mask.r, _focalize / 3.0) * 10.0, 0, 1);
		float greyScale = clamp(pow(mask.r, _focalize / 3.0) * _smooth, 0, 1);
		//float4 mainColor = lerp(tex2D(_BlurredMainTex, i.uv), tex2D(_MainTex, i.uv), (1.0 - greyScale) * detailMask.r);
		float4 mainColor = lerp(tex2D(_MainTex, i.uv), tex2D(_BlurredMainTex, i.uv), greyScale);
		return mainColor;
	}

	ENDCG

	Subshader {
		Pass{
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