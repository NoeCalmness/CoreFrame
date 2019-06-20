/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* Support falloff alpha blend particle effect
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.1
* Created:  2018-03-08
*
***************************************************************************************************/

Shader "HYLR/Particles/Falloff Alpha"
{
    Properties
    {
        _RimColor ("Rim Color", Color) = (0.5,0.5,0.5,0.5)
        _InnerColor ("Inner Color", Color) = (0.5,0.5,0.5,0.5)
        _InnerColorPower ("Inner Color Power", Range(0.0,1.0)) = 0.5
        _RimPower ("Rim Power", Range(0.0,5.0)) = 2.5
        _AlphaPower ("Alpha Rim Power", Range(0.0,8.0)) = 4.0
        _AllPower ("All Power", Range(0.0, 10.0)) = 1.0

        [Toggle(_INVERTED)] _Inverted("Inverted", int) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" }

        Cull Off

        CGPROGRAM
        #pragma surface surf Lambert alpha vertex:vert
        #pragma multi_compile __ _INVERTED

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        float4 _RimColor;
        float _RimPower;
        float _AlphaPower;
        float _AlphaMin;
        float _InnerColorPower;
        float _AllPower;
        float4 _InnerColor;

        void vert(inout appdata_full v)
        {
            #if _INVERTED
                float4 wp = mul(unity_ObjectToWorld, v.vertex);
                wp.z = -wp.z;
                v.vertex = mul(unity_WorldToObject, wp);
            #endif
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            float3 cp = _WorldSpaceCameraPos.xyz;
            float3 wn = normalize(IN.worldNormal);

            #if _INVERTED
                wn.z = -wn.z;
                cp.z = -cp.z;
            #endif

            float v = dot(normalize(cp - IN.worldPos), wn);
            if (v < 0) discard;

            half rim = 1.0 - saturate(v);

            o.Albedo =  _RimColor.rgb * pow(rim, _RimPower) *_AllPower + _InnerColor.rgb * _InnerColorPower * 2;
            o.Alpha = pow(rim, _AlphaPower) * _AllPower;
        }

        ENDCG
    }

    Fallback "VertexLit"
} 