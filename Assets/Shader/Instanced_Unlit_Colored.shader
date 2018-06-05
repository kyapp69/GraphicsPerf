﻿
Shader "GfxP/Instanced Unlit Colored"
{
	Properties
	{
		//_Color ("Color", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			//UNITY_INSTANCING_BUFFER_START (MyProperties)
            //UNITY_INSTANCING_BUFFER_END(MyProperties)
			
			v2f vert (appdata v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID (v);
                //UNITY_TRANSFER_INSTANCE_ID (v, o);

				o.color = v.vertex;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//UNITY_SETUP_INSTANCE_ID (i);
				return i.color;
			}
			ENDCG
		}
	}
}