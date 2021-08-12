// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hidden/BOXOPHOBIC/The Vegetation Engine/Elements/Custom"
{
	Properties
	{
		[StyledBanner(Custom Element)]_Banner("Banner", Float) = 0
		[StyledMessage(Info, Custom elements can be used to write to Custom buffer types and they can be used to create special effects such as tire or snow trails., 0,0)]_Message("Message", Float) = 0
		[StyledCategory(Element Settings)]_ElementCat("[ Element Cat ]", Float) = 0
		[NoScaleOffset][Space(10)]_MainTex("Element Texture", 2D) = "white" {}
		[StyledRemapSlider(_MainTexMinValue, _MainTexMaxValue, 0, 1)]_MainTexRemap("Element Remap", Vector) = (0,0,0,0)
		[HideInInspector]_MainTexMinValue("Element Min", Range( 0 , 1)) = 0
		[HideInInspector]_MainTexMaxValue("Element Max", Range( 0 , 1)) = 1
		_MainUVs("Element UVs", Vector) = (1,1,0,0)
		[HDR][Gamma]_MainColor("Element Color", Color) = (0.5019608,0.5019608,0.5019608,1)
		[StyledCategory(Advanced)]_AdvancedCat("[ Advanced Cat ]", Float) = 0
		[StyledMessage(Info, Use this feature to fade out elements at a distance to the camera to avoid rendering issues when the element is close to a volume edge. The Fade Start and End values are set on the Global Volume gameobject. Available in play mode only., _ElementFadeSupport, 1, 2, 10)]_ElementFadeMessage("Enable Fade Message", Float) = 0
		[ASEEnd][StyledToggle]_ElementFadeSupport("Enable Distance Fade Support", Float) = 1
		[HideInInspector]_ElementLayer("_ElementLayer", Float) = 0
		[HideInInspector]_IsCustomShader("_IsCustomShader", Float) = 1

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
		Cull Off
		ColorMask RGBA
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
			#include "UnityShaderVariables.cginc"
			#define ASE_NEEDS_FRAG_WORLD_POSITION


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
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform half _Banner;
			uniform half _Message;
			uniform half _AdvancedCat;
			uniform half4 _MainTexRemap;
			uniform half _ElementCat;
			uniform half _ElementFadeMessage;
			uniform half _ElementLayer;
			uniform half _IsCustomShader;
			uniform half4 _MainColor;
			uniform sampler2D _MainTex;
			uniform half4 _MainUVs;
			uniform half _MainTexMinValue;
			uniform half _MainTexMaxValue;
			uniform half TVE_ElementsFadeEnd;
			uniform half TVE_ElementsFadeStart;
			uniform half _ElementFadeSupport;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				o.ase_color = v.color;
				
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
				half temp_output_7_0_g18856 = _MainTexMinValue;
				half4 temp_cast_0 = (temp_output_7_0_g18856).xxxx;
				half temp_output_7_0_g18855 = TVE_ElementsFadeEnd;
				half Enable_Fade_Support197 = _ElementFadeSupport;
				half lerpResult186 = lerp( 1.0 , saturate( ( ( distance( WorldPosition , _WorldSpaceCameraPos ) - temp_output_7_0_g18855 ) / ( TVE_ElementsFadeStart - temp_output_7_0_g18855 ) ) ) , Enable_Fade_Support197);
				half FadeOut_Mask189 = lerpResult186;
				half4 appendResult199 = (half4(1.0 , 1.0 , 1.0 , FadeOut_Mask189));
				
				
				finalColor = ( ( _MainColor * saturate( ( ( tex2D( _MainTex, ( ( ( 1.0 - i.ase_texcoord1.xy ) * (_MainUVs).xy ) + (_MainUVs).zw ) ) - temp_cast_0 ) / ( _MainTexMaxValue - temp_output_7_0_g18856 ) ) ) ) * appendResult199 * i.ase_color );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "TVEShaderElementGUI"
	
	
}
/*ASEBEGIN
Version=18806
1920;1;1906;1021;947.8159;890.5292;1;True;False
Node;AmplifyShaderEditor.WorldPosInputsNode;194;-2144,1008;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TexCoordVertexDataNode;172;-2176,-512;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;169;-2176,-384;Half;False;Property;_MainUVs;Element UVs;7;0;Create;False;0;0;0;False;0;False;1,1,0,0;1,1,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldSpaceCameraPos;192;-2144,1168;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;198;-2176,126.9188;Inherit;False;Property;_ElementFadeSupport;Enable Distance Fade Support;11;0;Create;False;0;0;0;False;1;StyledToggle;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;173;-1968,-512;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;187;-1760,1168;Inherit;False;Global;TVE_ElementsFadeEnd;TVE_ElementsFadeEnd;43;0;Create;True;0;0;0;False;0;False;0;10000;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;185;-1760,1008;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;190;-1760,1232;Inherit;False;Global;TVE_ElementsFadeStart;TVE_ElementsFadeStart;43;0;Create;True;0;0;0;False;0;False;0;9000;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;175;-1968,-384;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;197;-1152,128;Half;False;Enable_Fade_Support;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;191;-1248,1008;Inherit;False;Remap To 0-1;-1;;18855;5eda8a2bb94ebef4ab0e43d19291cd8b;0;3;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;180;-1792,-512;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;167;-1968,-304;Inherit;False;FLOAT2;2;3;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;176;-1600,-512;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;193;-992,1008;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;188;-608,1200;Inherit;False;197;Enable_Fade_Support;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;179;-1024,-192;Half;False;Property;_MainTexMaxValue;Element Max;6;1;[HideInInspector];Create;False;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;186;-224,1008;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;166;-1024,-512;Inherit;True;Property;_MainTex;Element Texture;3;1;[NoScaleOffset];Create;False;0;0;0;False;1;Space(10);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;168;-1024,-256;Half;False;Property;_MainTexMinValue;Element Min;5;1;[HideInInspector];Create;False;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;189;352,1008;Half;False;FadeOut_Mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;178;-704,-256;Inherit;False;Remap To 0-1;-1;;18856;5eda8a2bb94ebef4ab0e43d19291cd8b;0;3;6;COLOR;0,0,0,0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;182;-1024,-768;Half;False;Property;_MainColor;Element Color;8;2;[HDR];[Gamma];Create;False;0;0;0;False;0;False;0.5019608,0.5019608,0.5019608,1;0.5019608,0.5019608,0.5019608,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;177;-512,-256;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;200;-256,-256;Inherit;False;189;FadeOut_Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;183;-256,-512;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;207;48,-208;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;199;41.58765,-363.5484;Inherit;False;FLOAT4;4;0;FLOAT;1;False;1;FLOAT;1;False;2;FLOAT;1;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;97;0,-1024;Half;False;Property;_Banner;Banner;0;0;Create;True;0;0;0;True;1;StyledBanner(Custom Element);False;0;0;1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;201;256,-512;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;202;-1920,-1408;Half;False;Property;_AdvancedCat;[ Advanced Cat ];9;0;Create;True;0;0;0;True;1;StyledCategory(Advanced);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;174;-1280,-256;Half;False;Property;_MainTexRemap;Element Remap;4;0;Create;False;0;0;0;True;1;StyledRemapSlider(_MainTexMinValue, _MainTexMaxValue, 0, 1);False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;203;-2176,-1408;Half;False;Property;_ElementCat;[ Element Cat ];2;0;Create;True;0;0;0;True;1;StyledCategory(Element Settings);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;196;-2176,192;Half;False;Property;_ElementFadeMessage;Enable Fade Message;10;0;Create;False;0;0;0;True;1;StyledMessage(Info, Use this feature to fade out elements at a distance to the camera to avoid rendering issues when the element is close to a volume edge. The Fade Start and End values are set on the Global Volume gameobject. Available in play mode only., _ElementFadeSupport, 1, 2, 10);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;206;640,-1024;Half;False;Property;_ElementLayer;_ElementLayer;12;1;[HideInInspector];Create;False;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;184;384,-1024;Half;False;Property;_IsCustomShader;_IsCustomShader;13;1;[HideInInspector];Create;True;0;0;0;True;0;False;1;0;1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;100;160,-1024;Half;False;Property;_Message;Message;1;0;Create;True;0;0;0;True;1;StyledMessage(Info, Custom elements can be used to write to Custom buffer types and they can be used to create special effects such as tire or snow trails., 0,0);False;0;0;1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;640,-512;Half;False;True;-1;2;TVEShaderElementGUI;0;1;BOXOPHOBIC/The Vegetation Engine/Elements/Custom;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;False;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;2;False;-1;True;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;0;False;-1;True;False;0;False;-1;0;False;-1;True;3;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;PreviewType=Plane;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
Node;AmplifyShaderEditor.CommentaryNode;195;-2144,880;Inherit;False;2687.203;100;Fade out element by distance from cam;0;;0.1843137,0.6841286,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;205;-2176,-1536;Inherit;False;1025.473;100;Drawers;0;;1,0.252,0,1;0;0
WireConnection;173;0;172;0
WireConnection;185;0;194;0
WireConnection;185;1;192;0
WireConnection;175;0;169;0
WireConnection;197;0;198;0
WireConnection;191;6;185;0
WireConnection;191;7;187;0
WireConnection;191;8;190;0
WireConnection;180;0;173;0
WireConnection;180;1;175;0
WireConnection;167;0;169;0
WireConnection;176;0;180;0
WireConnection;176;1;167;0
WireConnection;193;0;191;0
WireConnection;186;1;193;0
WireConnection;186;2;188;0
WireConnection;166;1;176;0
WireConnection;189;0;186;0
WireConnection;178;6;166;0
WireConnection;178;7;168;0
WireConnection;178;8;179;0
WireConnection;177;0;178;0
WireConnection;183;0;182;0
WireConnection;183;1;177;0
WireConnection;199;3;200;0
WireConnection;201;0;183;0
WireConnection;201;1;199;0
WireConnection;201;2;207;0
WireConnection;0;0;201;0
ASEEND*/
//CHKSM=0F0AB1C677E9325F93021A18B75C2CAA7A5E30A2