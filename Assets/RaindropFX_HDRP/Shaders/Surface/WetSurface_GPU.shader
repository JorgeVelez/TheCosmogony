Shader "Custom/RaindropFX/WetSurface_GPU" {
    Properties {
		_tint("Global Tint", Color) = (1, 1, 1, 1)

		_BumpAmt("Rain Distortion", range(0,32)) = 10
		_NrmAmt("Normal Weight", range(0,16)) = 1
		_DetailBumpAmt("Detail Nrm Weight", range(0,16)) = 0.5

		_TintAmt("Tint Amount", Range(0,1)) = 0.1

		_Reflect("Reflect", Range(0,1)) = 0.3

		_IOR("IOR", range(1,1.1)) = 1
		_Roughness("Roughness", Range(0,1)) = 0.0

		_WetMulti("Wet Multiply", Range(0,128)) = 1
		_WetItr("Wet Iteration", Range(0.01,2)) = 1

		_FogCol("Fog Color", Color) = (1, 1, 1, 1)
		_FogAmt("Fog", Range(0,1)) = 0

		_DropCol("Droplet Color", Color) = (1, 1, 1, 1)
		_DropColMulti("Drop Col Weight", Range(0,2)) = 0

		_MainTex("Tint Color(RGB)", 2D) = "white" {}
		_RoughMap("Rough Map(grayscale)", 2D) = "white" {}
		_DetailBump("Detail Normal", 2D) = "bump" {}
		_BumpMap("Normal Map", 2D) = "bump" {}
		_HeightMap("Rain Texture (AUTO)", 2D) = "black" {}
		_WetMap("Wet Map (AUTO)", 2D) = "black" {}
		_WipeMap("Wipe Map (AUTO)", 2D) = "white" {}
		_Cube("Environment", Cube) = "_Skybox"{}
    }

    HLSLINCLUDE
	#define SHADOW_HIGH
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"

    CBUFFER_START(RaindorpFX)
	float _IsUseWipe;
    float _TintAmt;
    float _Reflect;
    float _Roughness;
	float _WetItr;
	float _WetMulti;
    float4 _FogCol;
    float4 _tint;
	float _FogAmt;
    float4 _DropCol;
	float _DropColMulti;
    float4 _BumpMap_ST;
    float4 _MainTex_ST;
	float _BumpAmt;
	float _NrmAmt;
	float _DetailBumpAmt;
	float _IOR;
    float4 _ColorPyramidTexture_TexelSize;
	float2 _MainTex_TexelSize;

	int _debug;
    CBUFFER_END
   
    TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
    TEXTURE2D(_RoughMap); SAMPLER(sampler_RoughMap);
    TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
    TEXTURE2D(_DetailBump); SAMPLER(sampler_DetailBump);
	TEXTURE2D(_HeightMap); SAMPLER(sampler_HeightMap);
    TEXTURE2D(_WetMap); SAMPLER(sampler_WetMap);
    TEXTURE2D(_WipeMap); SAMPLER(sampler_WipeMap);
    TEXTURE2D(_ScreenColor); SAMPLER(sampler_ScreenColor);
    TEXTURECUBE(_Cube); SAMPLER(sampler_Cube);
    SAMPLER(sampler_ColorPyramidTexture);
    ENDHLSL

    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Lighting Off

        Pass {
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct VertexInput {
                float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
            };

            struct VertexOutput {
                float4 clipPosition : SV_POSITION;
				float3 reflex : NORMAL;

				float2 mainUV : TEXCOORD0;
                float4 grabUV : TEXCOORD1;
                float2 normalUV : TEXCOORD2;
            };

            VertexOutput vert(VertexInput v) {
                VertexOutput o;
               
                float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                o.clipPosition = TransformWorldToHClip(positionWS);
               
                #if UNITY_UV_STARTS_AT_TOP
                float scale = -1;
                #else
                float scale = 1;
                #endif
               
                o.grabUV.xy = (float2(o.clipPosition.x, o.clipPosition.y * scale) + o.clipPosition.w) * 0.5;
                o.grabUV.zw = o.clipPosition.zw + _IOR - 1.0;
				o.mainUV = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                o.normalUV = v.texcoord.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;

				float3 worldNormal = TransformObjectToWorldNormal(v.normal);
				float3 worldViewDir = positionWS - _WorldSpaceCameraPos.xyz;
				o.reflex = reflect(worldViewDir, worldNormal);
               
                return o;
            }

			float GetPixelLevel(float inPixel) {
				float _inBlack = 5.0, _inWhite = 255.0, _outWhite = 255.0, _outBlack = 0.0;
				return (((inPixel * 255.0) - _inBlack) / (_inWhite - _inBlack) * (_outWhite - _outBlack) + _outBlack) / 255.0;
			}

			float3 HeightToNormal(float2 nowPos) {
				nowPos.x -= 1.0 / _MainTex_TexelSize.x;
				float4 leftColor = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, nowPos);
				float xLeft = max(leftColor.b, GetPixelLevel(max(leftColor.r, leftColor.g)));

				nowPos.x += 2.0 / _MainTex_TexelSize.x;
				float4 rightColor = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, nowPos);
				float xRight = max(rightColor.b, GetPixelLevel(max(rightColor.r, rightColor.g)));

				nowPos.x -= 1.0 / _MainTex_TexelSize.x;
				nowPos.y += 1.0 / _MainTex_TexelSize.y;
				float4 upColor = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, nowPos);
				float yUp = max(upColor.b, GetPixelLevel(max(upColor.r, upColor.g)));

				nowPos.y -= 2.0 / _MainTex_TexelSize.y;
				float4 downColor = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, nowPos);
				float yDown = max(downColor.b, GetPixelLevel(max(downColor.r, downColor.g)));

				//float xDelta = clamp((xLeft - xRight) * _BumpAmt, 0.0, 1.0);
				//float yDelta = clamp((yUp - yDown) * _BumpAmt, 0.0, 1.0);

				float xDelta = clamp(((xLeft - xRight) * _BumpAmt + 1.0) * 0.5, 0.0, 1.0);
				float yDelta = clamp(((yUp - yDown) * _BumpAmt + 1.0) * 0.5, 0.0, 1.0);

				return UnpackNormal(float4(xDelta, yDelta, 1.0f, 1.0f));
				//return float3(xDelta, yDelta, 1.0f);
			}

            float4 frag (VertexOutput IN) : SV_Target {
				float2 nrmRaw = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.normalUV)).rg * _NrmAmt;

				if (_debug > 0) {
					float4 hCol = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, IN.mainUV);
					return lerp(float4(nrmRaw, 1.0, 1.0), float4(hCol.rgb, 1.0), 0.9);
					//return float4(nrmRaw, 1.0);
				}
				
				float2 rainBump = HeightToNormal(IN.mainUV).rg;
                float2 detailBump = UnpackNormal(SAMPLE_TEXTURE2D(_DetailBump, sampler_DetailBump, IN.mainUV)).rg * _DetailBumpAmt;

				float roughMask = SAMPLE_TEXTURE2D(_RoughMap, sampler_RoughMap, IN.mainUV).g;
				float4 rawMask = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, IN.mainUV);
				float rainMask = max(rawMask.b, GetPixelLevel(max(rawMask.r, rawMask.g)));
				float wetMask = clamp(
					pow(
						abs(
							SAMPLE_TEXTURE2D(_WetMap, sampler_WetMap, IN.mainUV).r * _WetMulti
						), _WetItr
					), 0, 1
				);
				float wipeMask = 1.0 - (_IsUseWipe > 0 ? SAMPLE_TEXTURE2D(_WipeMap, sampler_WipeMap, IN.mainUV).r : 0.0);

				float4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.mainUV + lerp(detailBump, rainBump, rainMask) + nrmRaw);
				float3 refVec = IN.reflex;

				/*if (rainMask > 0) {
					IN.grabUV.xy += rainBump * _BumpAmt * IN.grabUV.z;
					refVec += float3(rainBump, 0) * _BumpAmt / 4.0;
				} else {
					float2 dbump = detailBump * (1.0 - wetMask) * _DetailBumpAmt;
					IN.grabUV.xy += dbump * IN.grabUV.z;
					refVec += float3(dbump, 0);
				}*/

				float2 dbump = detailBump * (1.0 - wetMask);
				IN.grabUV.xy += (lerp(dbump, rainBump, rainMask) + nrmRaw) * IN.grabUV.z;
				refVec += float3(lerp(dbump, rainBump, rainMask) + nrmRaw, 0);

				float4 ref = SAMPLE_TEXTURECUBE_LOD(_Cube, sampler_Cube, refVec, 0);

				float4 grabColor = SAMPLE_TEXTURE2D_X_LOD(
					_ColorPyramidTexture, 
					sampler_ColorPyramidTexture,
					IN.grabUV.xy / IN.grabUV.w, 0
				);
				/*float4 grabColor = SAMPLE_TEXTURE2D_LOD(
					_ScreenColor, 
					sampler_ScreenColor, 
					IN.grabUV.xy / IN.grabUV.w,
					0
				);*/

				int plv = clamp(_Roughness * 3.0 * roughMask, 0, 4);
				int kernelSize = clamp(plv * 8.0, 0, 24);
				float4 blurredColor = SAMPLE_TEXTURE2D_X_LOD(
					_ColorPyramidTexture, 
					sampler_ColorPyramidTexture,
					IN.grabUV.xy / IN.grabUV.w, 
					plv
				);
				/*float4 blurredColor = SAMPLE_TEXTURE2D_LOD(
					_ScreenColor,
					sampler_ScreenColor,
					IN.grabUV.xy / IN.grabUV.w,
					plv
				); */
				if (kernelSize > 1) {
					for (int x = -kernelSize; x <= kernelSize; x++) {
						for (int y = -kernelSize; y <= kernelSize; y++) {
							float4 kColor = SAMPLE_TEXTURE2D_X_LOD(
								_ColorPyramidTexture, 
								sampler_ColorPyramidTexture, (
									IN.grabUV.xy + float2(x, y) * 
									_ColorPyramidTexture_TexelSize.xy
								) / IN.grabUV.w, plv
							);
							/*float4 kColor = SAMPLE_TEXTURE2D_LOD(
								_ScreenColor,
								sampler_ScreenColor, (
									sampler_ColorPyramidTexture, (
										IN.grabUV.xy + float2(x, y) * 
										_ColorPyramidTexture_TexelSize.xy
									) / IN.grabUV.w, plv
							);*/
							blurredColor += kColor;
						}
					}
					blurredColor /= pow(abs(kernelSize * 2 + 1), 2.0);
				}

				float4 mixAlbedo = lerp(lerp(grabColor, blurredColor, wipeMask), albedo, _TintAmt);
				float4 surface = lerp(mixAlbedo, ref, _Reflect);
				float4 fog = (1.0 - wetMask) * _FogCol * wipeMask; fog.a = 1.0;
				float4 dropCol = _DropColMulti > 0 ? lerp(lerp(surface, fog, fog * _FogAmt), _DropCol,
					pow(abs(rainMask), 2.01 - _DropColMulti)) : lerp(surface, fog, fog * _FogAmt);

                //return dropCol;
				//return dropCol * float4(_tint.r * 2.0, _tint.g * 2.0, _tint.b * 2.0, 1.0);
				return dropCol * float4(_tint.r, _tint.g, _tint.b, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
