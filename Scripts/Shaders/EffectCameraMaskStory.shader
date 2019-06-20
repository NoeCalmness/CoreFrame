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

Shader "HYLR/Effect/CameraMaskStory"
{
    Properties
    {
        _Color ("Mask Color", Color) = (0, 0, 0, 0.6)
    }

    SubShader
    {
        Tags { "Queue" = "Geometry+100" "RenderType" = "Geometry+110" "PreviewType" = "Plane" "IgnoreProjector" = "True" }

        Cull Back ZWrite Off Lighting Off
        Blend Zero SrcColor

        CGPROGRAM

        #pragma surface surf Lambert vertex:vert nofog nolightmap nodynlightmap keepalpha noinstancing

        half4 _Color;

        struct Input
        {
            half4 color;
        };
        
        void vert(inout appdata_full v, out Input o)
        {            
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.color.rgb = _Color.rgb;
            o.color.a   = v.color.a;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 c = IN.color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }

        ENDCG
    }

    Fallback "Transparent/VertexLit"
}
