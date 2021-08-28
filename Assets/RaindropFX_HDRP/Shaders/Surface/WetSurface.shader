Shader "Custom/RaindropFX/WetSurface" {
	Properties{
		_BumpAmt("Distortion", range(0,64)) = 10
		_IOR("IOR", range(0,1)) = 0.1
		_BumpDetailAmt("DetailDistortion", range(0,1)) = 0.5
		_TintAmt("Tint Amount", Range(0,1)) = 0.1
		_Roughness("Roughness", Range(0,1)) = 1.0
		_Reflect("Reflect", Range(0,1)) = 0.3
		_FogAmt("Fog", Range(0,1)) = 0
		_FogItr("FogIteration", Range(0,10)) = 1

		_FogCol("FogColor", Color) = (1, 1, 1, 1)

		_MainTex("TintColor(RGB)", 2D) = "white" {}
		_BumpMap("NormalMap", 2D) = "bump" {}
		_FogMaskMap("WetMap", 2D) = "white" {}
		_Cube("Enviroment", Cube) = "_Skybox"{}
		//_RefWeight("ReflectMap (Grayscale)", 2D) = "black" {}
	}

	Category{
		Tags { "Queue" = "Transparent" "RenderType" = "Opaque" }

		SubShader {
			Pass {
				Name "BASE"
				Tags { "LightMode" = "Always" }

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord: TEXCOORD0;
					float3 normal : NORMAL;
				};

				struct v2f {
					float4 vertex : POSITION;
					float4 uvgrab : TEXCOORD0;
					float3 reflex : NORMAL;
					float2 uvbump : TEXCOORD1;
					float2 uvmain : TEXCOORD2;
					UNITY_FOG_COORDS(3)
				};

				float _IOR;
				float _FogAmt;
				float _FogItr;
				float _Reflect;
				float _Roughness;
				float _BumpAmt;
				float _BumpDetailAmt;
				half _TintAmt;
				//float4 _RefWeight_ST;
				float4 _BumpMap_ST;
				float4 _MainTex_ST;
				float4 _FogCol;
				samplerCUBE _Cube;
				float _Relect;

				v2f vert(appdata_t v) {
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#if UNITY_UV_STARTS_AT_TOP
					float scale = -1.0;
					#else
					float scale = 1.0;
					#endif
					o.vertex = UnityObjectToClipPos(v.vertex);
					float3 worldNormal = UnityObjectToWorldNormal(v.normal);
					float3 worldViewDir = WorldSpaceViewDir(v.vertex);
					o.reflex = reflect(-worldViewDir, worldNormal);
					
					o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
					o.uvgrab.zw = o.vertex.zw;
					o.uvbump = TRANSFORM_TEX(v.texcoord, _BumpMap);
					o.uvmain = TRANSFORM_TEX(v.texcoord, _MainTex);
					UNITY_TRANSFER_FOG(o,o.vertex);
					
					return o;
				}

				sampler2D _GrabBlurTexture;
				sampler2D _GrabOriginTexture;
				float4 _GrabBlurTexture_TexelSize;
				sampler2D _BumpMap;
				//sampler2D _RefWeight;
				sampler2D _FogMaskMap;
				sampler2D _MainTex;

				float4 frag(v2f i) : SV_Target {
					float3 bump = UnpackNormal(tex2D(_BumpMap, i.uvbump)).rgb;
					float4 tint = tex2D(_MainTex, i.uvmain);
					float2 offset = bump * _BumpAmt * 10.0 * _GrabBlurTexture_TexelSize.xy + float2(1.0, 1.0) * (tint * _BumpDetailAmt + _IOR);
					i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;

					float fogMask = tex2D(_FogMaskMap, i.uvbump);
					float4 col = tex2Dproj(_GrabBlurTexture, UNITY_PROJ_COORD(i.uvgrab));
					float4 oriCol = tex2Dproj(_GrabOriginTexture, UNITY_PROJ_COORD(i.uvgrab));
					col = lerp(col, oriCol, fogMask);
					col = lerp(col, oriCol, 1.0 - _Roughness);

					//-----------------------------------------------------------------
					float4 ref = texCUBE(_Cube, i.reflex + bump * clamp(fogMask + 0.2, 0, 1) * _BumpAmt * 0.05);
					
					//col = lerp(col, ref, tex2D(_RefWeight, i.uvmain));
					//col = lerp(col, ref, _Reflect);
					float4 fcol = lerp(col, ref, _Reflect);
					fcol = lerp(fcol, tint, _TintAmt);
					col = lerp(col, tint, _TintAmt);

					float4 wet = clamp(pow(tex2D(_FogMaskMap, i.uvbump), 0.5) * _FogItr, 0, 1);
					col = lerp(col, col * wet + (_FogCol + col * 0.5) * (1.0 - wet), _FogAmt);
					col = lerp(col, ref, _Reflect * clamp(wet * wet, 0, 1));
					col = lerp(col, fcol, 1.0 - clamp(_FogAmt * 5, 0, 1));

					//col = lerp(col, _FogCol, _FogAmt / 10.0 * (1.0 - pow(tex2D(_FogMaskMap, i.uvbump), _FogItr)));
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG
			}
		}

	}

}