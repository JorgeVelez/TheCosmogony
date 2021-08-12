// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "BOXOPHOBIC/The Vegetation Engine/Elements/Default/Motion Wind Power"
{
	Properties
	{
		[HideInInspector]_IsMotionShader("_IsMotionShader", Float) = 1
		[StyledBanner(Wind Power Element)]_Banner("Banner", Float) = 0
		[StyledMessage(Info, Use the Wind Power elements to control the global wind power. Element Texture R is used as alpha mask. Particle Color R is used as values multiplier and Alpha as Element Intensity multiplier., 0,0)]_Message("Message", Float) = 0
		[StyledCategory(Render Settings)]_RenderCat("[ Render Cat ]", Float) = 0
		_ElementIntensity("Element Intensity", Range( 0 , 1)) = 1
		[StyledMessage(Info, When using a higher Layer number the Global Volume will create more render textures to render the elements. Try using fewer layers when posibble., _ElementLayerMessage, 1, 10, 10)]_ElementLayerMessage("Element Layer Message", Float) = 0
		[StyledEnum(Default _Layer 1 _Layer 2 _Layer 3 _Layer 4 _Layer 5 _Layer 6 _Layer 7 _Layer 8)]_ElementLayerValue("Element Layer", Float) = 0
		[Enum(Constant,0,Seasons,1)]_ElementMode("Element Mode", Float) = 0
		[StyledCategory(Element Settings)]_ElementCat("[ Element Cat ]", Float) = 0
		[NoScaleOffset][StyledTextureSingleLine]_MainTex("Element Texture", 2D) = "white" {}
		[StyledSpace(10)]_MainTexSpace("#MainTex Space", Float) = 0
		[StyledRemapSlider(_MainTexMinValue, _MainTexMaxValue, 0, 1)]_MainTexRemap("Element Remap", Vector) = (0,0,0,0)
		[HideInInspector]_MainTexMinValue("Element Min", Range( 0 , 1)) = 0
		[HideInInspector]_MainTexMaxValue("Element Max", Range( 0 , 1)) = 1
		_MainUVs("Element UVs", Vector) = (1,1,0,0)
		_MainValue("Element Value", Range( 0 , 1)) = 1
		_AdditionalValue1("Winter Value", Range( 0 , 1)) = 1
		_AdditionalValue2("Spring Value", Range( 0 , 1)) = 1
		_AdditionalValue3("Summer Value", Range( 0 , 1)) = 1
		_AdditionalValue4("Autumn Value", Range( 0 , 1)) = 1
		[StyledRemapSlider(_NoiseMinValue, _NoiseMaxValue, 0, 1)]_NoiseRemap("Noise Remap", Vector) = (0,0,0,0)
		[StyledCategory(Advanced)]_AdvancedCat("[ Advanced Cat ]", Float) = 0
		[StyledMessage(Info, Use this feature to fade out elements close to a volume edges to avoid rendering issues when the element is exiting the volume., _ElementFadeSupport, 1, 2, 10)]_ElementFadeMessage("Enable Fade Message", Float) = 0
		[ASEEnd][StyledToggle]_ElementFadeSupport("Enable Edge Fade Support", Float) = 0
		[HideInInspector]_IsVersion("_IsVersion", Float) = 400
		[HideInInspector]_IsElementShader("_IsElementShader", Float) = 1

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Transparent" "Queue"="Transparent" "PreviewType"="Plane" }
	LOD 0

		CGINCLUDE
		#pragma target 2.0
		ENDCG
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaToMask Off
		Cull Back
		ColorMask B
		ZWrite Off
		ZTest LEqual
		
		
		
		Pass
		{
			Name "Unlit"

			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#define ASE_NEEDS_FRAG_COLOR
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			// Element Type Define
			#define TVE_IS_MOTION_SHADER


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_color : COLOR;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform half _RenderCat;
			uniform half4 _NoiseRemap;
			uniform half4 _MainTexRemap;
			uniform half _ElementFadeMessage;
			uniform half _ElementCat;
			uniform half _AdvancedCat;
			uniform half _IsElementShader;
			uniform half _IsVersion;
			uniform half _MainTexSpace;
			uniform half _ElementLayerMessage;
			uniform half _ElementLayerValue;
			uniform half _IsMotionShader;
			uniform half _Message;
			uniform half _Banner;
			uniform half _MainValue;
			uniform half4 TVE_SeasonOptions;
			uniform half _AdditionalValue1;
			uniform half _AdditionalValue2;
			uniform half TVE_SeasonLerp;
			uniform half _AdditionalValue3;
			uniform half _AdditionalValue4;
			uniform half _ElementMode;
			uniform sampler2D _MainTex;
			uniform half4 _MainUVs;
			uniform half _MainTexMinValue;
			uniform half _MainTexMaxValue;
			uniform half _ElementIntensity;
			uniform half4 TVE_ColorsCoord;
			uniform half4 TVE_ExtrasCoord;
			uniform half4 TVE_MotionCoord;
			uniform half4 TVE_ReactCoord;
			uniform half TVE_ElementsFadeValue;
			uniform half _ElementFadeSupport;
			half4 IS_ELEMENT( half4 Colors, half4 Extras, half4 Motion, half4 React )
			{
				#if defined (TVE_IS_COLORS_SHADER)
				return Colors;
				#elif defined (TVE_IS_EXTRAS_SHADER)
				return Extras;
				#elif defined (TVE_IS_MOTION_SHADER)
				return Motion;
				#elif defined (TVE_IS_REACT_SHADER)
				return React;
				#else
				return Colors;
				#endif
			}
			

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_color = v.color;
				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				half Value_Main157_g18633 = _MainValue;
				half TVE_SeasonOptions_X50_g18633 = TVE_SeasonOptions.x;
				half Value_Winter158_g18633 = _AdditionalValue1;
				half Value_Spring159_g18633 = _AdditionalValue2;
				half TVE_SeasonLerp54_g18633 = TVE_SeasonLerp;
				half lerpResult168_g18633 = lerp( Value_Winter158_g18633 , Value_Spring159_g18633 , TVE_SeasonLerp54_g18633);
				half TVE_SeasonOptions_Y51_g18633 = TVE_SeasonOptions.y;
				half Value_Summer160_g18633 = _AdditionalValue3;
				half lerpResult167_g18633 = lerp( Value_Spring159_g18633 , Value_Summer160_g18633 , TVE_SeasonLerp54_g18633);
				half TVE_SeasonOptions_Z52_g18633 = TVE_SeasonOptions.z;
				half Value_Autumn161_g18633 = _AdditionalValue4;
				half lerpResult166_g18633 = lerp( Value_Summer160_g18633 , Value_Autumn161_g18633 , TVE_SeasonLerp54_g18633);
				half TVE_SeasonOptions_W53_g18633 = TVE_SeasonOptions.w;
				half lerpResult165_g18633 = lerp( Value_Autumn161_g18633 , Value_Winter158_g18633 , TVE_SeasonLerp54_g18633);
				half Element_Mode55_g18633 = _ElementMode;
				half lerpResult181_g18633 = lerp( Value_Main157_g18633 , ( ( TVE_SeasonOptions_X50_g18633 * lerpResult168_g18633 ) + ( TVE_SeasonOptions_Y51_g18633 * lerpResult167_g18633 ) + ( TVE_SeasonOptions_Z52_g18633 * lerpResult166_g18633 ) + ( TVE_SeasonOptions_W53_g18633 * lerpResult165_g18633 ) ) , Element_Mode55_g18633);
				half Base_Extras_RGB213_g18633 = ( lerpResult181_g18633 * i.ase_color.r );
				half3 appendResult582_g18633 = (half3(0.0 , 0.0 , Base_Extras_RGB213_g18633));
				half3 Final_Wind_RGB566_g18633 = appendResult582_g18633;
				half4 tex2DNode17_g18633 = tex2D( _MainTex, ( ( ( 1.0 - i.ase_texcoord1.xy ) * (_MainUVs).xy ) + (_MainUVs).zw ) );
				half temp_output_7_0_g19287 = _MainTexMinValue;
				half4 temp_cast_0 = (temp_output_7_0_g19287).xxxx;
				half4 break469_g18633 = saturate( ( ( tex2DNode17_g18633 - temp_cast_0 ) / ( _MainTexMaxValue - temp_output_7_0_g19287 ) ) );
				half MainTex_R73_g18633 = break469_g18633.r;
				half4 Colors37_g19292 = TVE_ColorsCoord;
				half4 Extras37_g19292 = TVE_ExtrasCoord;
				half4 Motion37_g19292 = TVE_MotionCoord;
				half4 React37_g19292 = TVE_ReactCoord;
				half4 localIS_ELEMENT37_g19292 = IS_ELEMENT( Colors37_g19292 , Extras37_g19292 , Motion37_g19292 , React37_g19292 );
				half4 temp_output_35_0_g19293 = localIS_ELEMENT37_g19292;
				half temp_output_7_0_g19294 = TVE_ElementsFadeValue;
				half2 temp_cast_1 = (temp_output_7_0_g19294).xx;
				half2 temp_output_851_0_g18633 = saturate( ( ( abs( (( (temp_output_35_0_g19293).zw + ( (temp_output_35_0_g19293).xy * (WorldPosition).xz ) )*2.0 + -1.0) ) - temp_cast_1 ) / ( 1.0 - temp_output_7_0_g19294 ) ) );
				half2 break852_g18633 = ( temp_output_851_0_g18633 * temp_output_851_0_g18633 );
				half Enable_Fade_Support454_g18633 = _ElementFadeSupport;
				half lerpResult842_g18633 = lerp( 1.0 , ( 1.0 - saturate( ( break852_g18633.x + break852_g18633.y ) ) ) , Enable_Fade_Support454_g18633);
				half FadeOut_Mask656_g18633 = lerpResult842_g18633;
				half Element_Intensity56_g18633 = ( _ElementIntensity * i.ase_color.a * FadeOut_Mask656_g18633 );
				half Final_Wind_A564_g18633 = ( MainTex_R73_g18633 * Element_Intensity56_g18633 );
				half4 appendResult573_g18633 = (half4(Final_Wind_RGB566_g18633 , Final_Wind_A564_g18633));
				
				
				finalColor = appendResult573_g18633;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "TVEShaderElementGUI"
	
	
}
/*ASEBEGIN
Version=18909
1920;7;1906;1015;1095.101;2087.039;1;True;False
Node;AmplifyShaderEditor.FunctionNode;131;-256,-1536;Inherit;False;Define ELEMENT MOTION;0;;19304;6eebc31017d99e84e811285e6a5d199d;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;120;128,-1536;Half;False;Property;_Message;Message;3;0;Create;True;0;0;0;True;1;StyledMessage(Info, Use the Wind Power elements to control the global wind power. Element Texture R is used as alpha mask. Particle Color R is used as values multiplier and Alpha as Element Intensity multiplier., 0,0);False;0;0;1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;115;0,-1536;Half;False;Property;_Banner;Banner;2;0;Create;True;0;0;0;True;1;StyledBanner(Wind Power Element);False;0;0;1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;143;-256,-1280;Inherit;False;Base Element;4;;18633;0e972c73cae2ee54ea51acc9738801d0;6,477,2,478,0,145,3,481,3,576,1,491,1;0;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;80,-1280;Half;False;True;-1;2;TVEShaderElementGUI;0;1;BOXOPHOBIC/The Vegetation Engine/Elements/Default/Motion Wind Power;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;False;True;2;5;False;-1;10;False;-1;0;2;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;True;False;False;True;False;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;0;False;-1;True;False;0;False;-1;0;False;-1;True;3;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;PreviewType=Plane;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;0;0;143;0
ASEEND*/
//CHKSM=49F4B42D96EC0D59E227A52C09DA875208717E6A