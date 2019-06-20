/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* Used for UI particle system
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.1
* Created:  2018-03-02
*
***************************************************************************************************/

Shader "HYLR/Particles/UI/Additive"
{
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "white" {}

        [Space(10)]
        [Header(UI Property)]
        _AlphaChannel ("Alpha channel", Range(0, 1)) = 1
        _Brightness ("Brightness", Range(0, 20)) = 2.0
    }

    Category
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend SrcAlpha One
        Cull Off Lighting Off ZWrite Off

        SubShader
        {
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0
                #pragma multi_compile_particles

                #include "UnityCG.cginc"

                sampler2D _MainTex;
                fixed _AlphaChannel;
                half _Brightness;

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    half4  color : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    half4  color : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                float4 _MainTex_ST;

                v2f vert(appdata_t v)
                {
                    v2f o;

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.color = v.color;
                    o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);

                    return o;
                }

                half4 frag(v2f i) : SV_Target
                {
                    half4 col = _Brightness * i.color * tex2D(_MainTex, i.texcoord);
                    col.a *= _AlphaChannel;
                    return col;
                }

                ENDCG
            }
        }
    }
}
