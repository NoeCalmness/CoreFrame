/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* Toon Shading with self shadowmap
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.1
* Created:  2017-08-08
*
***************************************************************************************************/

Shader "HYLR/Toon/Toon"
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
        _DefRimIntensity("Default Rim Intensity", Range(0, 5)) = 1
        [HDR] _DefRimColor ("Default Rim Color (HDR)", Color) = (1, 1, 1, 1)
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "BaseCG.cg"

    #pragma shader_feature _RIM_LIGHT
    #pragma shader_feature _DEF_RIM_LIGHT

    sampler2D _MainTex;
    float4 _MainTex_ST;

    ENDCG

    SubShader 
    {
        Tags{ "RenderType" = "Opaque" "Queue" = "Geometry+101" }

        Pass
        {
            Cull Front

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

            #include "ToonColor.cg"

            #pragma vertex vert_c
            #pragma fragment frag_c
            
            #pragma multi_compile_fwdbase

            ENDCG
        }
    }

    FallBack "Diffuse"
}
