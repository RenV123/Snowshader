Shader "custom/MeshDeformation"
{
	Properties
	{
		_MaxDepth("MaxDepth", Range(-50,0)) = -3 // sliders
		_Radius("Radius", Range(1,100)) = 5 // sliders
		_Strength("Strength", Range(0,500)) = 1
		_MainTex("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			Tags{ "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			#pragma target 5.0
			#pragma vertex VS_Main
			#pragma fragment FS_Main //pixel shader
			#pragma geometry GS_Main
			#include "UnityCG.cginc" 

		// **************************************************************
		// Data structures												*
		// **************************************************************
		struct GS_INPUT
		{
			float4	pos		: POSITION;
			float3	normal	: NORMAL;
			float2  tex0	: TEXCOORD0;
		};

		struct FS_INPUT
		{
			float4	pos		: POSITION;
			float2  tex0	: TEXCOORD0;
			float3  normal : NORMAL;
		};


		// **************************************************************
		// Vars															*
		// **************************************************************

		float _MaxDepth;
		float _Radius;
		float _Strength;

		Texture2D _MainTex;
		SamplerState sampler_MainTex;
		StructuredBuffer<float4> _spherePosBuffer;


		// **************************************************************
		// Shader Programs												*
		// **************************************************************

		// Vertex Shader ------------------------------------------------
		GS_INPUT VS_Main(appdata_base v)
		{
			GS_INPUT output = (GS_INPUT)0;
			output.pos = v.vertex;
			output.normal = v.normal;
			output.tex0 = v.texcoord.xy;
			return output;
		}

		// Geometry Shader -----------------------------------------------------
		[maxvertexcount(4)]
		void GS_Main(triangle GS_INPUT p[3], inout TriangleStream<FS_INPUT> triStream)
		{
			uint numPos, stride;
			_spherePosBuffer.GetDimensions(numPos,stride); //Get number of positions

			for (int i = 0; i < 3; i++)
			{
				FS_INPUT pIn;
				float4 worldPos = mul(_Object2World, p[i].pos);

				float shortestDistance = 1000000;
				for (uint a = 0; a < numPos; a++)
				{
					float distance = length(worldPos.xz - _spherePosBuffer[a].xz);
					if (distance < shortestDistance)
						shortestDistance = distance;
				}

				if (shortestDistance < _Radius && worldPos.y > _MaxDepth)
				{
					float amount = (1 - shortestDistance / _Radius) * _Strength;
					pIn.pos = mul(UNITY_MATRIX_VP, float4(worldPos.x, worldPos.y - amount, worldPos.z, worldPos.w));
				}
				else
				{
					pIn.pos = mul(UNITY_MATRIX_VP, worldPos);
				}

				pIn.tex0 = p[i].tex0;
				pIn.normal = mul(p[i].normal, _Object2World);
				triStream.Append(pIn);
			}
			triStream.RestartStrip();
		}

		// Fragment Shader (pixelShader) ---------------------------------------
		float4 FS_Main(FS_INPUT input) : COLOR
		{
			return _MainTex.Sample(sampler_MainTex, input.tex0);
		}

			ENDCG
		}
	}
}
