Shader "Shader/RaindropFX/BTS" {
	Properties{
		_CullMask("CullMask", 2D) = "white" {}
	}

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

	int isGrab;

	TEXTURE2D_X(_GrabTex);
	TEXTURE2D(_CullMask);
	TEXTURE2D(_Origin);

	SAMPLER(sampler_CullMask);
	SAMPLER(sampler_Origin);

	TEXTURE2D(_MainTex);
	SAMPLER(sampler_MainTex);
	float4 CustomPostProcess(Varyings input) : SV_Target{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float4 outColor;
		
		float2 positionSS = input.texcoord * _ScreenSize.xy;
		if (isGrab > 0) {
			outColor = LOAD_TEXTURE2D_X(_GrabTex, positionSS);
		} else {
			float4 bgCol = SAMPLE_TEXTURE2D(_Origin, sampler_Origin, input.texcoord);
			outColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord);
			float4 mask = SAMPLE_TEXTURE2D(_CullMask, sampler_CullMask, input.texcoord);
			outColor = lerp(bgCol, outColor, mask);
		}

		return outColor;
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