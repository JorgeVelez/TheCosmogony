Shader "Custom/RaindropFX/Invert" {
    Properties {
		
    }

    HLSLINCLUDE
	#define SHADOW_LOW
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"

    CBUFFER_START(RaindorpFX)
    float4 _MainTex_ST;
    float4 _ColorPyramidTexture_TexelSize;
    CBUFFER_END
   
    TEXTURE2D(_MainTex);
    SAMPLER(sampler_ColorPyramidTexture);
    ENDHLSL

    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent"}
        Lighting Off

        Pass {
            ZWrite Off
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
                half2 uv : TEXCOORD0;
                //float4 grabUV : TEXCOORD1;
				//float3 reflex : NORMAL;
            };

            VertexOutput vert(VertexInput v) {
                VertexOutput o;
               
                float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                o.clipPosition = TransformWorldToHClip(positionWS);
                o.uv = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
               
                /*#if UNITY_UV_STARTS_AT_TOP
                float scale = -1;
                #else
                float scale = 1;
                #endif*/
               
                //o.grabUV.xy = (float2(o.clipPosition.x, o.clipPosition.y * scale) + o.clipPosition.w) * 0.5;
                //o.grabUV.zw = o.clipPosition.zw;

				//float3 worldNormal = TransformObjectToWorldNormal(v.normal);
				//float3 worldViewDir = positionWS - _WorldSpaceCameraPos.xyz;
				//o.reflex = reflect(worldViewDir, worldNormal);
               
                return o;
            }

            float4 frag (VertexOutput IN) : SV_Target {
				//float4 grabColor = SAMPLE_TEXTURE2D_X_LOD(
				//	_ColorPyramidTexture, 
				//	sampler_ColorPyramidTexture,
				//	IN.grabUV.xy / IN.grabUV.w, 0
				//);

                //return float4(1.0 - grabColor.r, 1.0 - grabColor.g, 1.0 - grabColor.b, 1.0);
				return float4(1, 1, 1, 1);
            }
            ENDHLSL
        }
    }
       FallBack Off
}
