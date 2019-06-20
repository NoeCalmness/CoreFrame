/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* Toon shading for awake state
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.1
* Created:  2018-11-23
*
***************************************************************************************************/

Shader "HYLR/Toon/ToonAwakeInverted"
{
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)

        [Space(10)]
        [Header(Dissolve Settings)]
        [Space(5)]
        [Toggle(_DISSOLVE)] _Dissolve("Enable Dissolve Effect", int) = 0
        _DissolveMaskTex ("Dissolve Mask Texture (RGB)", 2D) = "white" {}
        _DissolveThreshold ("Dissolve Threshod", Range(0, 1)) = 0.5
        _DissolveEdge ("Dissolve Edge", Range(0, 1)) = 0.85
        _DissolveEdgeWidth ("Dissolve Edge Width", Range(0, 0.2)) = 0.1
        _DissolveEdgePower ("Dissolve Edge Power", Range(0, 3)) = 0.5
        [HDR] _DissolveEdgeColorA ("Dissolve Edge Color A (HDR)", Color) = (2.3, 2, 0, 1)
        [HDR] _DissolveEdgeColorB ("Dissolve Edge Color B (HDR)", Color) = (2, 0, 0, 1)
    }

    Category
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off Lighting Off ZWrite Off

        SubShader
        {
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0
                #pragma shader_feature _DISSOLVE

                #define _INVERTED 1

                #include "UnityCG.cginc"
                #include "BaseCG.cg"
                #include "DissolveCG.cg"

                sampler2D _MainTex;
                float4 _MainTex_ST;
                fixed4 _TintColor;

                sampler2D _DissolveMaskTex;
                float4 _DissolveMaskTex_ST;
                half _DissolveThreshold;

                fixed _DissolveEdge;
                fixed _DissolveEdgeWidth;
                half _DissolveEdgePower;
                half4 _DissolveEdgeColorA;
                half4 _DissolveEdgeColorB;

                v2f_wp vert(a2v v)
                {
                    v2f_wp o;

                    #if _INVERTED
                        float4 wp = mul(unity_ObjectToWorld, v.vertex);
                        wp.z = -wp.z;
                        o.vertex = UnityWorldToClipPos(wp);
                    #else
                        o.vertex = UnityObjectToClipPos(v.vertex);
                    #endif

                    o.color = v.color * _TintColor;
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                    #if _DISSOLVE
                        DISSOLVE_TEXCOORD;
                    #endif

                    return o;
                }

                fixed4 frag(v2f_wp i) : SV_Target
                {
                    #if _DISSOLVE
                        DISSOLVE_CLIP;
                    #endif

                    fixed4 col = 2.0f * i.color * tex2D(_MainTex, i.uv);

                    #if _DISSOLVE
                        DISSOLVE_TRANSFORM(col);
                    #endif

                    return col;
                }

                ENDCG
            }
        }
    }
}
