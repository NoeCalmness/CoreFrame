/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* Window background mesh
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.1
* Created:  2018-12-28
*
***************************************************************************************************/

Shader "HYLR/Effect/Window Background"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _AddTex("Add Texture", 2D) = "black"{}
        _Rect ("Add Texture UV in Main", Vector) = (0, 0, 0, 0)
        _AddTexColor("Add Texture Alpha", Color) = (1,1,1,1)
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _Brightness ("Brightness", Range(0, 5)) = 1
        _Saturation ("Saturation", Range(-1, 2)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry+120" "IgnoreProjector" = "True" }
        
        ZWrite Off

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            #define ACES_LUMINANCE(x) (x.r * 0.2126 + x.g * 0.7152 + x.b * 0.0722).xxx
            #define ACES_CONTRUST(x, y) (x - 0.4135884) * y + 0.4135884

            fixed _Brightness;
            fixed4 _Color;
            half _Saturation;
            sampler2D _MainTex;
            sampler2D _AddTex;
            float4 _MainTex_ST;
            float4 _AddTex_ST;
            fixed4 _Rect;
            fixed4 _AddTexColor;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };

            v2f vert (appdata_t v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                float x = v.texcoord.x - _Rect.x;
                float y = v.texcoord.y - _Rect.y;
                float rx = _Rect.z - _Rect.x;
                float ry = _Rect.w - _Rect.y;
                o.texcoord1 = TRANSFORM_TEX(float2(x / rx ,y / ry), _AddTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.texcoord);
                if (i.texcoord.x >= _Rect.x && i.texcoord.x <= _Rect.z &&
                    i.texcoord.y >= _Rect.y && i.texcoord.y <= _Rect.w)
                {
                    fixed4 ac = tex2D(_AddTex, i.texcoord1);
                    color.rgb = color.rgb * (1 - ac.a) + ac.rgb * _AddTexColor.rgb * ac.a * _AddTexColor.a;
                }

                if (_Saturation != 1)
                {
                    half3 lum = ACES_LUMINANCE(color);
                    color.rgb = lum + _Saturation * (color.rgb - lum);
                }
                color.rgb *= _Color.rgb * _Brightness;

                return color;
            }

            ENDCG
        }
    }
}
