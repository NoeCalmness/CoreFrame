/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* Simplified Diffuse shader.
* Fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.1
* Created:  2017-06-29
*
***************************************************************************************************/

Shader "HYLR/Particles/Diffuse"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        [Toggle(_INVERTED)] _Inverted("Inverted", int) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 150
        Cull Off

        CGPROGRAM

        #pragma surface surf Lambert noforwardadd vertex:vert
        #pragma multi_compile __ _INVERTED

        sampler2D _MainTex;
        fixed _Brightness;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            float2 uv_MainTex;
        };

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

            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }

        ENDCG
    }

    Fallback "Mobile/VertexLit"
}
