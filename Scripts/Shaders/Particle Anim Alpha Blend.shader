/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* Alpha Blend Shader
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.2
* Created:  2018-03-29
*
***************************************************************************************************/

Shader "HYLR/Particles/Anim Alpha Blended"
{
    Properties
    {
        _TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
        _MainTex ("Particle Texture", 2D) = "white" {}

        [Toggle(_INVERTED)] _Inverted("Inverted", int) = 0
    }

    Category
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
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
                #pragma multi_compile __ _INVERTED

                #include "UnityCG.cginc"

                sampler2D _MainTex;
                fixed4 _TintColor;

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                    float4 texcoords : TEXCOORD0;
                    float texcoordBlend : TEXCOORD1;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                    float2 texcoord2 : TEXCOORD1;
                    fixed blend : TEXCOORD2;
                };

                float4 _MainTex_ST;

                v2f vert(appdata_t v)
                {
                    v2f o;

                    #if _INVERTED
                        float4 wp = mul(unity_ObjectToWorld, v.vertex);
                        wp.z = -wp.z;
                        o.vertex = UnityWorldToClipPos(wp);
                    #else
                        o.vertex = UnityObjectToClipPos(v.vertex);
                    #endif

                    o.color = v.color * _TintColor;
                    o.texcoord = TRANSFORM_TEX(v.texcoords.xy,_MainTex);
                    o.texcoord2 = TRANSFORM_TEX(v.texcoords.zw,_MainTex);
                    o.blend = v.texcoordBlend;

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 colA = tex2D(_MainTex, i.texcoord);
                    fixed4 colB = tex2D(_MainTex, i.texcoord2);
                    fixed4 col = 2.0f * i.color * lerp(colA, colB, i.blend);
                    return col;
                }

                ENDCG
            }
        }
    }
}
