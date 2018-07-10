Shader "Tessellation" 
{
    Properties 
	{
		[Header(VertexDisplace)]
		_VDist("Vert Distplace", Range(0,1)) = 0.1

		[Header(Color)]
		_FillColor("FillColor", Color) = (0,0,0,0)
        _TessEdge ("Edge Tess", Range(1,64)) = 2
		_Freq ("Freq", Range(1,200)) = 20
    }
    SubShader 
	{
    	Pass 
		{
			Tags { "DisableBatching " = "True" }
     
    		CGPROGRAM
    		#pragma target 4.6
     
    		#pragma vertex VS
    		#pragma fragment PS

			#ifdef UNITY_CAN_COMPILE_TESSELLATION
				#pragma hull HS
				#pragma domain DS
			#endif
     
    		//#include "UnityCG.cginc"
			float _VDist;
    		float _TessEdge;
			float _Freq;
			uniform fixed4 _FillColor;

    		struct VS_In
    		{
        		float4 vertex : POSITION;
				float4 nor : NORMAL;
    		};
     
    		struct HS_In
    		{
        		float4 pos   : INTERNALTESSPOS;
				float4 nor : NORMAL;
    		};
     
	 		#ifdef UNITY_CAN_COMPILE_TESSELLATION
				struct HS_ConstantOut
				{
					float TessFactor[3]    : SV_TessFactor;
					float InsideTessFactor : SV_InsideTessFactor;
				};
			#endif
     
    		struct HS_Out
    		{
        		float4 pos    : INTERNALTESSPOS;
				float4 nor : NORMAL;
    		};

    		struct DS_Out
    		{
        		float4 pos    : SV_POSITION;
				float4 nor : NORMAL;
				float4 color : COLOR;
    		};

    		struct FS_Out
    		{		
        		fixed4 color      : SV_Target;
    		};     
     
    		HS_In VS( VS_In i )
    		{
        		HS_In o;
				o.pos = i.vertex;
				o.nor = i.nor;
        		return o;
    		}
    
			#ifdef UNITY_CAN_COMPILE_TESSELLATION
				HS_ConstantOut HSConstant( InputPatch<HS_In, 3> i )
				{
					HS_ConstantOut o = (HS_ConstantOut)0;
					o.TessFactor[0] = o.TessFactor[1] = o.TessFactor[2] = _TessEdge;
					o.InsideTessFactor = _TessEdge;    
					return o;
				}

				[UNITY_domain("tri")]
				[UNITY_partitioning("integer")]
				[UNITY_outputtopology("triangle_cw")]
				[UNITY_patchconstantfunc("HSConstant")]
				[UNITY_outputcontrolpoints(3)]
				HS_Out HS( InputPatch<HS_In, 3> i, uint uCPID : SV_OutputControlPointID )
				{
					HS_Out o = (HS_Out)0;
					o.pos = i[uCPID].pos;
					o.nor = i[uCPID].nor;
					return o;
				}
		
				[UNITY_domain("tri")]
				DS_Out DS( HS_ConstantOut HSConstantData, 
							const OutputPatch<HS_Out, 3> i, 
							float3 BarycentricCoords : SV_DomainLocation)
				{
					DS_Out o = (DS_Out)0;
		
					float fU = BarycentricCoords.x;
					float fV = BarycentricCoords.y;
					float fW = BarycentricCoords.z;

					float3 nor = i[0].nor * fU + i[1].nor * fV + i[2].nor * fW;
					o.nor = float4(nor.xyz, 0.0);

					float4 pos = i[0].pos * fU + i[1].pos * fV + i[2].pos * fW;

					//Vertex displacement
					pos.z += sin( pos*_Freq + _Time.w ) * _VDist;

					o.pos = UnityObjectToClipPos(pos);
					o.color = pos;
					return o;
				}
			#endif

    		FS_Out PS(DS_Out i)
    		{
        		FS_Out o;

				o.color = _FillColor * i.color;
 
       			return o;
    		}
     
    		ENDCG
    	}
    }
}