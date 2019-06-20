/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* Terrain
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.1
* Created:  2019-04-23
*
***************************************************************************************************/

Shader "HYLR/Scene/Terrain Mask"
{
	Properties
	{
        _Mask ("Mask Texture", 2D) = "white" {}
        _Layer1 ("Mask (R) Channel", 2D) = "white"{}
        _Layer2 ("Mask (G) Channel", 2D) = "white"{}
        _Layer3 ("Mask (B) Channel", 2D) = "white"{}
        _Layer4 ("Mask (A) Channel", 2D) = "white"{}
	}

	SubShader
	{
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
				float2 uvl    : TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 uvml   : TEXCOORD0;
				float4 uv12   : TEXCOORD1;
				float4 uv34   : TEXCOORD2;
			};

			sampler2D _Mask;
			sampler2D _Layer1;
			sampler2D _Layer2;
			sampler2D _Layer3;
			sampler2D _Layer4;

			half4 _Mask_ST;
            half4 _Layer1_ST;
            half4 _Layer2_ST;
            half4 _Layer3_ST;
            half4 _Layer4_ST;
			
			v2f vert(a2v v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);

				o.uvml = float4(TRANSFORM_TEX(v.uv, _Mask), v.uvl * unity_LightmapST.xy + unity_LightmapST.zw);
				o.uv12 = float4(TRANSFORM_TEX(v.uv, _Layer1), TRANSFORM_TEX(v.uv, _Layer2));
                o.uv34 = float4(TRANSFORM_TEX(v.uv, _Layer3), TRANSFORM_TEX(v.uv, _Layer4));

				return o;
			}
			
			half4 frag(v2f i) : SV_Target
			{
				half4 mask = tex2D(_Mask, i.uvml.xy);

				half4 layer1 = tex2D(_Layer1, i.uv12.xy);
                half4 layer2 = tex2D(_Layer2, i.uv12.zw);
                half4 layer3 = tex2D(_Layer3, i.uv34.xy);
                half4 layer4 = tex2D(_Layer4, i.uv34.zw);

                half4 color = layer1 * mask.r + layer2 * mask.g + layer3 * mask.b + layer4 * mask.a;

				half3 light = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uvml.zw));

				return half4(color * light, 1.0);
			}

			ENDCG
	    }
	}
}
