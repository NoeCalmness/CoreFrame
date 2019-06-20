/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* Projector version shadow
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.1
* Created:  2018-02-11
*
***************************************************************************************************/

Shader "HYLR/CharacterShadow"
{
    Properties
    {
        _Shadow ("Shadow", 2D) = "black" {}
        _Falloff ("FallOff", 2D) = "white" {}
    }

    Subshader
    {
        Tags { "Queue" = "Geometry+101" }

        Pass
        {
            ZWrite Off ColorMask RGB Blend SrcAlpha OneMinusSrcAlpha Offset -1, -1

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            
            struct v2f
            {
                float4 uvShadow  : TEXCOORD0;
                float4 uvFalloff : TEXCOORD1;
                UNITY_FOG_COORDS(2)
                float4 pos : SV_POSITION;
            };
            
            float4x4 unity_Projector;
            float4x4 unity_ProjectorClip;
            
            v2f vert (float4 vertex : POSITION)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(vertex);
                o.uvShadow = mul(unity_Projector, vertex);
                o.uvFalloff = mul(unity_ProjectorClip, vertex);

                UNITY_TRANSFER_FOG(o, o.pos);

                return o;
            }
            
            sampler2D _Shadow;
            sampler2D _Falloff;
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texS = tex2Dproj (_Shadow, UNITY_PROJ_COORD(i.uvShadow));
                fixed4 texF = tex2Dproj (_Falloff, UNITY_PROJ_COORD(i.uvFalloff));
                fixed4 res  = lerp(fixed4(1,1,1,0), texS, texF.a);

                UNITY_APPLY_FOG_COLOR(i.fogCoord, res, fixed4(1,1,1,1));

                return res;
            }

            ENDCG
        }
    }
}
