/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* Story background mesh
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.1
* Created:  2018-12-28
*
***************************************************************************************************/

Shader "HYLR/Effect/Story Background"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
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
            float4 _MainTex_ST;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert (appdata_t v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.texcoord);
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
