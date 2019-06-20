/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* Support invert alpha blend particle effect
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.1
* Created:  2018-03-08
*
***************************************************************************************************/

Shader "HYLR/Dissolve/DissolveAlphaBlend"
{
    Properties
    {
        _TintColor ("Tint Color", Color) = (1, 1, 1, 1)
        _MainTex ("Particle Texture", 2D) = "white" {}

        [Toggle(_INVERTED)] _Inverted("Inverted", int) = 0

        [Space(10)]
        [Header(Dissolve Settings)]
        [Space(5)]
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", int) = 2
        [Toggle(_DISSOLVE_ALPHA)] _DissolveAlpha("Use Alpha as Threshold", int) = 0
        [Toggle(_DISSOLVE_ALPHA_FADE)] _DissolveAlphaFade("Use Alpha as Threshold (With Alpha Blend)", int) = 0
        [Toggle(_DISSOLVE_CLIP_CENTER)] _DissolveClipCenter("Render Edge Only", int) = 0

        _DissolveMaskTex ("Dissolve Mask Texture (RGB)", 2D) = "white" {}
        _DissolveThreshold ("Dissolve Threshod", Range(0, 1)) = 0.5
        _DissolveEdge ("Dissolve Edge", Range(0, 1)) = 0.85
        _DissolveEdgeWidth ("Dissolve Edge Width", Range(0, 0.2)) = 0.1
        _DissolveEdgePower ("Dissolve Edge Power", Range(0, 3)) = 0.5
        [HDR] _DissolveEdgeColorA ("Dissolve Edge Color A (HDR)", Color) = (2.3, 2, 0, 1)
        [HDR] _DissolveEdgeColorB ("Dissolve Edge Color B (HDR)", Color) = (2, 0, 0, 1)

        [Space(10)]
        [Header(Dissolve Noise)]
        [Space(5)]
        [Toggle(_NOISE)] _Noise("Enable Noise", int) = 0
        _DissolveNoiseSpeed ("Dissolve Noise Speed", Vector) = (0, 0, 0, 0)
        _DissolveNoiseScale ("Dissolve Noise Scale", Range(0, 20)) = 1
        _DissolveNoiseBias ("Dissolve Noise Bias", Range(0.01, 1)) = 0.01
        [Space(10)]
        [Toggle(_NOISE_EDGE)] _NoiseEdge("Enable Edge Noise", int) = 0
        _DissolveEdgeNoiseSpeed ("Dissolve Edge Noise Speed", Vector) = (0, 0, 0, 0)
        _DissolveEdgeNoiseScale ("Dissolve Edge Noise Scale", Range(0, 20)) = 1
        _DissolveEdgeNoiseBias ("Dissolve Edge Noise Bias", Range(0.01, 0.5)) = 0.01
    }

    Category
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull [_CullMode] Lighting Off ZWrite Off

        SubShader
        {
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0
                #pragma multi_compile_particles
                #pragma multi_compile __ _INVERTED
                #pragma multi_compile __ _DISSOLVE_ALPHA
                #pragma multi_compile __ _DISSOLVE_ALPHA_FADE
                #pragma multi_compile __ _DISSOLVE_CLIP_CENTER
                #pragma multi_compile __ _NOISE
                #pragma multi_compile __ _NOISE_EDGE

                #define _DISSOLVE 1

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
                half4 _DissolveNoiseSpeed;
                half _DissolveNoiseScale;
                half _DissolveNoiseBias;
                half4 _DissolveEdgeNoiseSpeed;
                half _DissolveEdgeNoiseScale;
                half _DissolveEdgeNoiseBias;

                v2f_wp vert(a2v v)
                {
                    v2f_wp o;

                    float4 wp = mul(unity_ObjectToWorld, v.vertex);
                    #if _INVERTED
                        wp.z = -wp.z;
                    #endif

                    o.wp = wp;
                    o.vertex = UnityWorldToClipPos(wp);
                    o.color = v.color * _TintColor;
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                    DISSOLVE_TEXCOORD;

                    return o;
                }

                UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

                fixed4 frag(v2f_wp i) : SV_Target
                {
                    #if _DISSOLVE_ALPHA
                        _DissolveThreshold += (1 - _DissolveThreshold) * (1 - i.color.a);
                    #if !_DISSOLVE_ALPHA_FADE
                        i.color.a = _TintColor.a;
                    #endif
                    #endif
                    DISSOLVE_CLIP_NOISE;
                    DISSOLVE_CLIP_CENTER;

                    fixed4 col = i.color * tex2D(_MainTex, i.uv);

                    DISSOLVE_TRANSFORM(col);

                    return col;
                }

                ENDCG
            }
        }
    }
}
