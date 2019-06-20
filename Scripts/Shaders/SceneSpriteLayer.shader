/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* Basic scene sprite layer shading
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.1
* Created:  2017-06-29
*
***************************************************************************************************/

Shader "HYLR/Scene/SceneSpriteLayer"
{
    Properties
    {
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _Brightness ("Brightness", Range(0, 5)) = 2.5
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1, 1, 1, 1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        { 
            "Queue"             = "Transparent" 
            "RenderType"        = "Transparent" 
            "PreviewType"       = "Plane"
            "IgnoreProjector"   = "True" 
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        ZWrite Off
        Lighting Off
        Blend One OneMinusSrcAlpha

        CGPROGRAM

        #pragma surface surf Lambert vertex:vert nofog nolightmap nodynlightmap keepalpha noinstancing
        #pragma multi_compile _ PIXELSNAP_ON
        #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

        #include "UnitySprites.cginc"

        fixed _Brightness;

        struct Input
        {
            float2 uv_MainTex;
            fixed4 color;
        };
        
        void vert(inout appdata_full v, out Input o)
        {
            v.vertex.xy *= _Flip.xy;

            #if defined(PIXELSNAP_ON)
            v.vertex = UnityPixelSnap (v.vertex);
            #endif
            
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.color = v.color * _Color * _RendererColor;
            o.color.rgb *= _Brightness;
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = SampleSpriteTexture (IN.uv_MainTex) * IN.color;
            o.Albedo = c.rgb * c.a;
            o.Alpha = c.a;
        }

        ENDCG
    }

    Fallback "Transparent/VertexLit"
}
