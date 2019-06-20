/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* UIRenderTexture
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.2
* Created:  2017-12-07
*
***************************************************************************************************/

Shader "HYLR/UI/RenderTexture"
{
    Properties
    {
        [PerRendererData]_MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        Pass
        {
            Blend One OneMinusSrcAlpha
            Cull Off Lighting Off ZWrite Off

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            #define ACES_LUMINANCE(x) (x.r * 0.2126 + x.g * 0.7152 + x.b * 0.0722).xxx
            #define ACES_CONTRUST(x, y) (x - 0.4135884) * y + 0.4135884

            struct a2v
            {
                float4 vertex : POSITION;
                float4 color  : COLOR0;
                float2 uv : TEXCOORD0;
                float2 grayscale : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color  : COLOR0;
                float4 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            v2f vert(a2v v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = float4(TRANSFORM_TEX(v.uv, _MainTex).xy, v.grayscale);

                return o;
            }
            
            half4 frag(v2f i) : SV_Target
            {
                if (i.uv.x < 0 || i.uv.x > 1 || i.uv.y < 0 || i.uv.y > 1) discard;   // Clamp

                half4 color = tex2D(_MainTex, i.uv);

                color.rgb *= i.color;
                color *= i.color.a;

                half2 satcon = i.uv.zw;

                //color.rgb = ACES_CONTRUST(color.rgb, IN.texcoord.w);

                if (satcon.x != 1)
                {
                    half3 lum = ACES_LUMINANCE(color);
                    color.rgb = lum + satcon.x * (color.rgb - lum);
                }

                return color;
            }

            ENDCG
        }
    }
}
