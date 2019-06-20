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

Shader "HYLR/Effect/CameraMask"
{
    Properties
    {
        _Color ("Mask Color", Color) = (0, 0, 0, 0.6)
        _Alpha ("Mask Alpha", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "Queue" = "Geometry+100" "RenderType" = "Transparent" "PreviewType" = "Plane" "IgnoreProjector" = "True" }

        Cull Back ZWrite Off Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM

        #pragma surface surf Lambert vertex:vert nofog nolightmap nodynlightmap keepalpha noinstancing

        fixed4 _Color;
        fixed _Alpha;

        struct Input
        {
            fixed4 color;
        };
        
        void vert(inout appdata_full v, out Input o)
        {            
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.color.rgb = _Color.rgb * 2.5;
            o.color.a = _Alpha;
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = IN.color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }

        ENDCG
    }

    Fallback "Transparent/VertexLit"
}
