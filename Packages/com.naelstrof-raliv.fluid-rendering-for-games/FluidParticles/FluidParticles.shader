// Made with Amplify Shader Editor v1.9.6.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FluidParticles"
{
	Properties
	{
		_MainTex("_MainTex", 2D) = "white" {}
		_ParticleSize("ParticleSize", Range( 0.001 , 0.1)) = 0.001
		_Opacity("Opacity", Range( 0 , 0.1)) = 0

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend One One
		AlphaToMask Off
		Cull Off
		ColorMask RGBA
		ZWrite Off
		ZTest LEqual
		
		
		
		Pass
		{
			Name "Unlit"

			CGPROGRAM

			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define VERTEXID_SEMANTIC SV_VertexID
			#define INSTANCEID_SEMANTIC SV_InstanceID


			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "Packages/com.naelstrof-raliv.fluid-rendering-for-games/FluidParticles/FluidParticle.cginc"
			#pragma instancing_options procedural:ASEProceduralSetup


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				uint ase_vertexId : VERTEXID_SEMANTIC;
				uint ase_instanceId : INSTANCEID_SEMANTIC;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform float _ParticleSize;
			uniform sampler2D _MainTex;
			uniform float _Opacity;
			void ASEProceduralSetup() { }

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float localGetParticle22 = ( 0.0 );
				uint vertexID22 =(uint)v.ase_vertexId;
				uint instanceID22 =(uint)v.ase_instanceId;
				float particleSize22 = _ParticleSize;
				float3 localPosition22 = float3( 0,0,0 );
				float3 localNormal22 = float3( 0,0,0 );
				float2 uv22 = float2( 0,0 );
				float opacity22 = 0.0;
				GetParticle( vertexID22 , instanceID22 , particleSize22 , localPosition22 , localNormal22 , uv22 , opacity22 );
				
				float2 vertexToFrag239 = uv22;
				o.ase_texcoord1.xy = vertexToFrag239;
				float vertexToFrag265 = opacity22;
				o.ase_texcoord1.z = vertexToFrag265;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.w = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = localPosition22;
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
				float4 color240 = IsGammaSpace() ? float4(1,0,0,1) : float4(1,0,0,1);
				float2 vertexToFrag239 = i.ase_texcoord1.xy;
				float vertexToFrag265 = i.ase_texcoord1.z;
				
				
				finalColor = ( color240 * tex2D( _MainTex, vertexToFrag239 ).r * _Opacity * vertexToFrag265 );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19603
Node;AmplifyShaderEditor.VertexIdVariableNode;83;224,464;Inherit;False;0;1;INT;0
Node;AmplifyShaderEditor.InstanceIdNode;24;192,544;Inherit;False;True;True;0;1;INT;0
Node;AmplifyShaderEditor.RangedFloatNode;242;96,640;Inherit;False;Property;_ParticleSize;ParticleSize;1;0;Create;True;0;0;0;False;0;False;0.001;0.03;0.001;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;22;432,512;Inherit;False;GetGrassInstance(instanceID, vertexID, grassMat, localPosition, localNormal, uv)@;7;File;7;True;vertexID;OBJECT;0;In;uint;Inherit;False;True;instanceID;OBJECT;0;In;uint;Inherit;False;True;particleSize;FLOAT;0;In;;Inherit;False;True;localPosition;FLOAT3;0,0,0;Out;;Inherit;False;True;localNormal;FLOAT3;0,0,0;Out;;Inherit;False;True;uv;FLOAT2;0,0;Out;;Inherit;False;True;opacity;FLOAT;0;Out;;Inherit;False;GetParticle;False;False;0;be738ec41f05d8b4ba53d30aa46319dc;False;8;0;FLOAT;0;False;1;OBJECT;0;False;2;OBJECT;0;False;3;FLOAT;0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT2;0,0;False;7;FLOAT;0;False;5;FLOAT;0;FLOAT3;5;FLOAT3;6;FLOAT2;7;FLOAT;8
Node;AmplifyShaderEditor.VertexToFragmentNode;239;736,416;Inherit;False;False;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;11;1008,400;Inherit;True;Property;_MainTex;_MainTex;0;0;Create;True;0;0;0;False;0;False;-1;None;ae408e3a460eedc4eb89657b63168525;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.VertexToFragmentNode;265;752,592;Inherit;False;False;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;243;1152,688;Inherit;False;Property;_Opacity;Opacity;2;0;Create;True;0;0;0;False;0;False;0;0.1;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;240;1248,176;Inherit;False;Constant;_Color0;Color 0;1;0;Create;True;0;0;0;False;0;False;1,0,0,1;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;241;1584,432;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;244;2176,448;Float;False;True;-1;2;ASEMaterialInspector;100;5;FluidParticles;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;4;1;False;;1;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;True;True;2;False;;True;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;True;2;False;;True;3;False;;True;False;0;False;;0;False;;True;2;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;0;638626460561459182;0;1;True;False;;False;0
WireConnection;22;1;83;0
WireConnection;22;2;24;0
WireConnection;22;3;242;0
WireConnection;239;0;22;7
WireConnection;11;1;239;0
WireConnection;265;0;22;8
WireConnection;241;0;240;0
WireConnection;241;1;11;1
WireConnection;241;2;243;0
WireConnection;241;3;265;0
WireConnection;244;0;241;0
WireConnection;244;1;22;5
ASEEND*/
//CHKSM=381674084C503B2089966E25F90F88B31E63A9D1