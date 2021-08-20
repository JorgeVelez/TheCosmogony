Shader "LUTSSAMPLE/LUTBlendShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _LUT("LUT", 2D) = "white" {}
        _LUT2("LUT2", 2D) = "white" {}
        _Contribution("Contribution", Range(0, 1)) = 1
    }
        SubShader
        {
            // No culling or depth
            Cull Off ZWrite Off ZTest Always

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                #define COLORS 32.0

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                sampler2D _MainTex;
                sampler2D _LUT;
                sampler2D _LUT2;
                float4 _LUT_TexelSize;
                float4 _LUT2_TexelSize;
                float _Contribution;

                fixed4 frag(v2f i) : SV_Target
                {
                    float maxColor = COLORS - 1.0;
                    fixed4 col = saturate(tex2D(_MainTex, i.uv));
                    float threshold = maxColor / COLORS;

                    // LUT 1 texel size
                    float halfColX = 0.5 / _LUT_TexelSize.z;
                    float halfColY = 0.5 / _LUT_TexelSize.w;

                    // LUT 2 texel size
                    float halfColX2 = 0.5 / _LUT2_TexelSize.z;
                    float halfColY2 = 0.5 / _LUT2_TexelSize.w;

                    // LUT 1 color calculation
                    float xOffset = halfColX + col.r * threshold / COLORS;
                    float yOffset = halfColY + col.g * threshold;
                    float cell = floor(col.b * maxColor);

                    // LUT 2 color calculation
                    float xOffset2 = halfColX2 + col.r * threshold / COLORS;
                    float yOffset2 = halfColY2 + col.g * threshold;
                    float cell2 = floor(col.b * maxColor);
                    
                    // Finalise LUT 1
                    float2 lutPos = float2(cell / COLORS + xOffset, yOffset);
                    float4 gradedCol = tex2D(_LUT, lutPos);
                    
                    // Finalise LUT 2
                    float2 lutPos2 = float2(cell2 / COLORS + xOffset2, yOffset2);
                    float4 gradedCol2 = tex2D(_LUT2, lutPos);

                    return lerp(gradedCol, gradedCol2, _Contribution);
                }
                ENDCG
            }
        }
}