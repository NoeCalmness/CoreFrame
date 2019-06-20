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

Shader "HYLR/CharacterShadowSelf"
{
    Properties
    {
        _Outter ("Outter Shadow", 2D) = "black" {}
        _Inner ("Inner Shadow", 2D) = "black" {}
        _Falloff ("FallOff", 2D) = "white" {}

        _OutterGlow ("Outter Glow Strength", Range(-1, 1)) = 0.25
        _InnerGlow ("Inner Glow Strength", Range(-1, 1)) = 0.25

        [Space(10)]
        [Header(Rotation)]
        _OutterSpeed ("Outter Speed", Range(-2, 2)) = 0.3
        _InnerSpeed("Inner Speed", Range(-2, 2)) = -0.3
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
                float4 uvOutter  : TEXCOORD0;
                float4 uvInner   : TEXCOORD1;
                float4 uvFalloff : TEXCOORD2;
                UNITY_FOG_COORDS(3)
                float4 pos : SV_POSITION;
            };
            
            float4x4 unity_Projector;
            float4x4 unity_ProjectorClip;
            half _OutterSpeed;
            half _InnerSpeed;
            
            v2f vert (float4 vertex : POSITION)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(vertex);
                o.uvFalloff = mul(unity_ProjectorClip, vertex);

                float4 uv = mul(unity_Projector, vertex);
                o.uvOutter = uv;
                o.uvInner = uv;

                float s = sin(_OutterSpeed * _Time.w);
                float c = cos(_OutterSpeed * _Time.w);
                float2x2 rot = float2x2(c, -s, s, c);

                rot *= 0.5;
                rot += 0.5;
                rot = rot * 2 - 1;

                o.uvOutter.xy -= 0.5;
                o.uvOutter.xy = mul(o.uvOutter.xy, rot);
                o.uvOutter.xy += 0.5;

                s = sin(_InnerSpeed * _Time.w);
                c = cos(_InnerSpeed * _Time.w);
                rot = float2x2(c, -s, s, c);

                rot *= 0.5;
                rot += 0.5;
                rot = rot * 2 - 1;

                o.uvInner.xy -= 0.5;
                o.uvInner.xy = mul(o.uvInner.xy, rot);
                o.uvInner.xy += 0.5;

                UNITY_TRANSFER_FOG(o, o.pos);

                return o;
            }
            
            sampler2D _Outter;
            sampler2D _Inner;
            sampler2D _Falloff;
            half _OutterGlow;
            half _InnerGlow;
            
            half4 frag (v2f i) : SV_Target
            {
                half4 texO = tex2Dproj(_Outter, UNITY_PROJ_COORD(i.uvOutter)) * (1 + _OutterGlow);
                half4 texI = tex2Dproj(_Inner, UNITY_PROJ_COORD(i.uvInner)) * (1 + _InnerGlow);
                half4 texF = tex2Dproj(_Falloff, UNITY_PROJ_COORD(i.uvFalloff));

                half ia = (1 - texO.a) * texI.a;
                half4 tex = half4(texO.rgb * texO.a + texI.rgb * ia, texO.a + ia);

                tex = lerp(fixed4(1, 1, 1, 0), tex, texF.a);

                UNITY_APPLY_FOG_COLOR(i.fogCoord, tex, half4(1, 1, 1, 1));

                return tex;
            }

            ENDCG
        }
    }
}
