// Made with Amplify Shader Editor v1.9.6.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FluidParticles"
{
	Properties
	{
		_MainTex("_MainTex", 2D) = "white" {}

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		
		Pass
		{
			
			Name "FluidColor"

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
				float3 localPosition22 = float3( 0,0,0 );
				float3 localNormal22 = float3( 0,0,0 );
				float2 uv22 = float2( 0,0 );
				float4 color22 = float4( 0,0,0,0 );
				float heightStrength22 = 0.0;
				GetParticle( vertexID22 , instanceID22 , localPosition22 , localNormal22 , uv22 , color22 , heightStrength22 );
				
				float4 vertexToFrag265 = color22;
				o.ase_texcoord1 = vertexToFrag265;
				
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
				float4 vertexToFrag265 = i.ase_texcoord1;
				
				
				finalColor = vertexToFrag265;
				return finalColor;
			}
			ENDCG
		}
		
		Pass
		{
			Name "FluidHeight"
			
			Blend One One
			AlphaToMask Off
			Cull Off
			ColorMask RGBA
			ZWrite Off
			ZTest LEqual

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

			uniform sampler2D _MainTex;
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
				float3 localPosition22 = float3( 0,0,0 );
				float3 localNormal22 = float3( 0,0,0 );
				float2 uv22 = float2( 0,0 );
				float4 color22 = float4( 0,0,0,0 );
				float heightStrength22 = 0.0;
				GetParticle( vertexID22 , instanceID22 , localPosition22 , localNormal22 , uv22 , color22 , heightStrength22 );
				
				float2 vertexToFrag239 = uv22;
				o.ase_texcoord1.xy = vertexToFrag239;
				float vertexToFrag272 = heightStrength22;
				o.ase_texcoord1.z = vertexToFrag272;
				
				
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
				float4 color271 = IsGammaSpace() ? float4(1,0,0,1) : float4(1,0,0,1);
				float2 vertexToFrag239 = i.ase_texcoord1.xy;
				float vertexToFrag272 = i.ase_texcoord1.z;
				
				
				finalColor = ( color271 * ( tex2D( _MainTex, vertexToFrag239 ).r * vertexToFrag272 ) );
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
Node;AmplifyShaderEditor.CustomExpressionNode;22;432,512;Inherit;False;GetGrassInstance(instanceID, vertexID, grassMat, localPosition, localNormal, uv)@;7;File;7;True;vertexID;OBJECT;0;In;uint;Inherit;False;True;instanceID;OBJECT;0;In;uint;Inherit;False;True;localPosition;FLOAT3;0,0,0;Out;;Inherit;False;True;localNormal;FLOAT3;0,0,0;Out;;Inherit;False;True;uv;FLOAT2;0,0;Out;;Inherit;False;True;color;FLOAT4;0,0,0,0;Out;;Inherit;False;True;heightStrength;FLOAT;0;Out;;Inherit;False;GetParticle;False;False;0;be738ec41f05d8b4ba53d30aa46319dc;False;8;0;FLOAT;0;False;1;OBJECT;0;False;2;OBJECT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT2;0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT;0;False;6;FLOAT;0;FLOAT3;4;FLOAT3;5;FLOAT2;6;FLOAT4;7;FLOAT;8
Node;AmplifyShaderEditor.VertexToFragmentNode;239;816,912;Inherit;False;False;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;11;1056,896;Inherit;True;Property;_MainTex;_MainTex;0;0;Create;True;0;0;0;False;0;False;-1;None;ae408e3a460eedc4eb89657b63168525;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.VertexToFragmentNode;272;816,672;Inherit;False;False;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;271;1360,720;Inherit;False;Constant;_Color0;Color 0;1;0;Create;True;0;0;0;False;0;False;1,0,0,1;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;268;1408,944;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;241;1600,912;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexToFragmentNode;265;816,448;Inherit;False;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;269;1824,448;Float;False;True;-1;2;ASEMaterialInspector;100;18;FluidParticles;b199d6c4625f78a44954409d87f32159;True;FluidColor;0;0;FluidColor;2;True;True;2;5;False;;10;False;;3;1;False;;10;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;0;638626588965330292;0;2;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;270;1824,576;Float;False;False;-1;2;ASEMaterialInspector;100;18;New Amplify Shader;b199d6c4625f78a44954409d87f32159;True;FluidHeight;0;1;FluidHeight;2;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;True;4;1;False;;1;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;False;False;True;2;False;;True;3;False;;False;False;False;False;0;;0;0;Standard;0;False;0
WireConnection;22;1;83;0
WireConnection;22;2;24;0
WireConnection;239;0;22;6
WireConnection;11;1;239;0
WireConnection;272;0;22;8
WireConnection;268;0;11;1
WireConnection;268;1;272;0
WireConnection;241;0;271;0
WireConnection;241;1;268;0
WireConnection;265;0;22;7
WireConnection;269;0;265;0
WireConnection;269;1;22;4
WireConnection;270;0;241;0
WireConnection;270;1;22;4
ASEEND*/
//CHKSM=D25D5EBFD9C4B0396CC2C0EB599BC214C0997122