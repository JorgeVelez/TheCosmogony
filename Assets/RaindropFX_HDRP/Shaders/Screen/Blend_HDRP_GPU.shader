Shader "Shader/RaindropFX/Blend_HDRP_GPU" {
	HLSLINCLUDE
	#pragma target 4.5
	#pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"

	struct Attributes {
		uint vertexID : SV_VertexID;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct Varyings {
		float4 positionCS : SV_POSITION;
		float2 texcoord   : TEXCOORD0;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	Varyings Vert(Attributes input) {
		Varyings output;
		UNITY_SETUP_INSTANCE_ID(input);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

		output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
		output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
		return output;
	}

	float2 _MainTex_TexelSize;
	float4 _TintColor;
	float4 _FogTint;
	float _PixelSize;
	float _FogIntensity;
	float _Distortion;
	int _FogIteration;
	int _IsUseFog;
	int _IsUseWipe;

	float _inBlack;
	float _inWhite;
	float _outWhite;
	float _outBlack;

	TEXTURE2D_X(_MainTex);
	TEXTURE2D(_HeightMap);
	TEXTURE2D(_WetMap);
	TEXTURE2D(_WipeMap);
	TEXTURE2D(_CullMask);

	SAMPLER(sampler_HeightMap);
	SAMPLER(sampler_WetMap);
	SAMPLER(sampler_WipeMap);
	SAMPLER(sampler_CullMask);

	// color level
	float GetPixelLevel(float inPixel) {
		return (((inPixel * 255.0) - _inBlack) / (_inWhite - _inBlack) * (_outWhite - _outBlack) + _outBlack) / 255.0;
	}

	float4 HeightToNormal(float2 nowPos) {
		nowPos.x -= 1.0 / _MainTex_TexelSize.x;
		float4 leftColor = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, nowPos);
		float xLeft = GetPixelLevel(max(leftColor.r, max(leftColor.g, leftColor.b)));

		nowPos.x += 2.0 / _MainTex_TexelSize.x;
		float4 rightColor = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, nowPos);
		float xRight = GetPixelLevel(max(rightColor.r, max(rightColor.g, rightColor.b)));

		nowPos.x -= 1.0 / _MainTex_TexelSize.x;
		nowPos.y += 1.0 / _MainTex_TexelSize.y;
		float4 upColor = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, nowPos);
		float yUp = GetPixelLevel(max(upColor.r, max(upColor.g, upColor.b)));

		nowPos.y -= 2.0 / _MainTex_TexelSize.y;
		float4 downColor = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, nowPos);
		float yDown = GetPixelLevel(max(downColor.r, max(downColor.g, downColor.b)));

		float xDelta = ((xLeft - xRight) + 1.0) * 0.5;
		float yDelta = ((yUp - yDown) + 1.0) * 0.5;

		return float4(clamp(xDelta, 0.0, 1.0), clamp(yDelta, 0.0, 1.0), 1.0f, 1.0f);
	}

	float4 CustomPostProcess(Varyings input) : SV_Target{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float4 mainColor;

		//float2 nowPos = input.texcoord;
		float ratioX = (int)(input.texcoord.x * _PixelSize) / _PixelSize;
		float ratioY = (int)(input.texcoord.y * _PixelSize) / _PixelSize;

		float2 nowPos = float2(ratioX, ratioY);
		if (_PixelSize < 0) nowPos = input.texcoord;

		/*nowPos.x -= 1.0 / _MainTex_TexelSize.x;
		float4 leftColor = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, nowPos);
		float xLeft = GetPixelLevel(max(leftColor.r, max(leftColor.g, leftColor.b)));

		nowPos.x += 2.0 / _MainTex_TexelSize.x;
		float4 rightColor = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, nowPos);
		float xRight = GetPixelLevel(max(rightColor.r, max(rightColor.g, rightColor.b)));

		nowPos.x -= 1.0 / _MainTex_TexelSize.x;
		nowPos.y += 1.0 / _MainTex_TexelSize.y;
		float4 upColor = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, nowPos);
		float yUp = GetPixelLevel(max(upColor.r, max(upColor.g, upColor.b)));

		nowPos.y -= 2.0 / _MainTex_TexelSize.y;
		float4 downColor = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, nowPos);
		float yDown = GetPixelLevel(max(downColor.r, max(downColor.g, downColor.b)));

		float xDelta = ((xLeft - xRight) + 1.0) * 0.5;
		float yDelta = ((yUp - yDown) + 1.0) * 0.5;*/

		//mainColor = float4(clamp(xDelta, 0.0, 1.0), clamp(yDelta, 0.0, 1.0), 1.0f, 1.0f);
		mainColor = HeightToNormal(nowPos);

		//return SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, nowPos);

		float2 bump = (mainColor * 2 - 1).rg;
		float2 positionSS = (input.texcoord.xy + bump * _Distortion *
			SAMPLE_TEXTURE2D(_CullMask, sampler_CullMask, input.texcoord).xy) * _ScreenSize.xy;
		if (positionSS.x > _ScreenSize.x) {
			while (positionSS.x > _ScreenSize.x) positionSS.x -= _ScreenSize.x;
			positionSS.x = _ScreenSize.x - positionSS.x;
		} else if (positionSS.x < 0) {
			while (positionSS.x < 0) positionSS.x += _ScreenSize.x;
			positionSS.x = _ScreenSize.x - positionSS.x;
		}
		if (positionSS.y > _ScreenSize.y) {
			while (positionSS.y > _ScreenSize.y) positionSS.y -= _ScreenSize.y;
			positionSS.y = _ScreenSize.y - positionSS.y;
		} else if (positionSS.y < 0) {
			while (positionSS.y < 0) positionSS.y += _ScreenSize.y;
			positionSS.y = _ScreenSize.y - positionSS.y;
		}
		if (positionSS.x == 0) positionSS.x += 1.0 / _ScreenSize.x;
		else if (positionSS.x == _ScreenSize.x) positionSS.x -= 1.0 / _ScreenSize.x;
		if (positionSS.y == 0) positionSS.y += 1.0 / _ScreenSize.y;
		else if (positionSS.y == _ScreenSize.y) positionSS.y -= 1.0 / _ScreenSize.y;
		mainColor = float4(LOAD_TEXTURE2D_X(_MainTex, positionSS).xyz, 1.0f);

		// tint
		mainColor = lerp(mainColor, mainColor * _TintColor, SAMPLE_TEXTURE2D(_WetMap, sampler_WetMap, input.texcoord));

		if (_IsUseFog) {
			float4 wet = clamp(pow(SAMPLE_TEXTURE2D(_WetMap, sampler_WetMap, input.texcoord) + 
				(_IsUseWipe ? SAMPLE_TEXTURE2D(_WipeMap, sampler_WipeMap, input.texcoord) : 0), 
				0.5) * _FogIteration, 0, 1);
			mainColor = lerp(mainColor, _FogTint, (1.0 - wet) * _FogIntensity);
		}
		return mainColor;
	}

	ENDHLSL
	SubShader {
		Pass{
			Name "GrayScale"
			ZWrite Off
			ZTest Always
			Blend Off
			Cull Off
			HLSLPROGRAM
				#pragma fragment CustomPostProcess
				#pragma vertex Vert
			ENDHLSL
		}
	}

	Fallback Off
}