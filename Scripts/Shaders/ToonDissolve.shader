/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* Toon Shading with dissolve effect
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.1
* Created:  2019-03-26
*
***************************************************************************************************/

Shader "HYLR/Toon/ToonDissolve"
{
    Properties
    {
        _MainTex ("Base Texture (RGB)", 2D) = "white" {}
        _SSSTex ("SSS Texture (RGB)", 2D) = "white" {}
        _ILMTex ("ILM Texture (RGB)", 2D) = "white" {}

        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(0, 1)) = 0.01
        _OutlineBlend ("Outline Blend Weight", Range(0, 1)) = 0.5

        _ShadowContrast ("Shadow Contrast", Range(0, 20)) = 1
        _DarkenInnerLine ("Darken Inner Line", Range(0, 1)) = 0.2
        _BloomMaskThreshod ("Bloom Mask Threshod", Range(0, 5)) = 0

        [Space(10)]
        [Header(Rim Light Settings)]
        [Space(5)]
        [HideInInspector][Toggle(_RIM_LIGHT)] _RimLight("Enable Rim Light", int) = 0
        [HideInInspector] _RimIntensity("Rim Intensity", Range(0, 5)) = 1
        [HideInInspector][HDR] _RimColor ("Rim Color (HDR)", Color) = (1, 1, 1, 1)

        [Space(10)]
        [Header(Default Rim Light Settings)]
        [Space(5)]
        [Toggle(_DEF_RIM_LIGHT)] _DefRimLight("Enable Default Rim Light", int) = 0
        _DefRimIntensity ("Default Rim Intensity", Range(0, 5)) = 1
        [HDR] _DefRimColor ("Default Rim Color (HDR)", Color) = (1, 1, 1, 1)

        [Space(10)]
        [Header(Dissolve Settings)]
        [Space(5)]
        _DissolveMaskTex ("Dissolve Mask Texture (RGB)", 2D) = "white" {}
        _DissolveThreshold ("Dissolve Threshod", Range(0, 1)) = 0.5
        _DissolveEdge ("Dissolve Edge", Range(0, 1)) = 0.85
        _DissolveEdgeWidth ("Dissolve Edge Width", Range(0, 0.2)) = 0.1
        _DissolveEdgePower ("Dissolve Edge Power", Range(0, 3)) = 0.5
        [HDR] _DissolveEdgeColorA ("Dissolve Edge Color A (HDR)", Color) = (2.3, 2, 0, 1)
        [HDR] _DissolveEdgeColorB ("Dissolve Edge Color B (HDR)", Color) = (2, 0, 0, 1)
    }

    CGINCLUDE

    #pragma shader_feature _RIM_LIGHT
    #pragma shader_feature _DEF_RIM_LIGHT
    #pragma shader_feature _DISSOLVE

    #include "UnityCG.cginc"
    #include "BaseCG.cg"
    #include "DissolveCG.cg"

    sampler2D _MainTex;
    float4 _MainTex_ST;

    sampler2D _DissolveMaskTex;
    float4 _DissolveMaskTex_ST;
    half _DissolveThreshold;

    ENDCG

    SubShader 
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Transparent" }

        Pass { Cull Back ZWrite On ColorMask 0 }

        Pass
        {
            Cull Back ZWrite Off

            CGPROGRAM

            fixed  _OutlineWidth;
            fixed4 _OutlineColor;
            fixed  _OutlineBlend;

            #include "ToonOutline.cg"

            #pragma vertex vert_o
            #pragma fragment frag_o
       
            ENDCG
        }

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }

            Cull Back

            CGPROGRAM

            sampler2D _SSSTex;
            sampler2D _ILMTex;
            half _ShadowContrast;
            fixed _DarkenInnerLine;
            half4 _RimColor;
            float _RimIntensity;
            half4 _DefRimColor;
            float _DefRimIntensity;
            half _BloomMaskThreshod;

            fixed _DissolveEdge;
            fixed _DissolveEdgeWidth;
            half _DissolveEdgePower;
            half4 _DissolveEdgeColorA;
            half4 _DissolveEdgeColorB;

            #include "ToonColor.cg"

            #pragma vertex vert_c
            #pragma fragment frag_c
            
            #pragma multi_compile_fwdbase

            ENDCG
        }
    }

    FallBack "Diffuse"
}
