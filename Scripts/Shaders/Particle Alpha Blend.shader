/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* Support invert alpha blend particle effect
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.1
* Created:  2018-03-08
*
***************************************************************************************************/

Shader "HYLR/Particles/Alpha Blended"
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
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
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
                    o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);

                    return o;
                }

                UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
                float _InvFade;

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = 2.0f * i.color * tex2D(_MainTex, i.texcoord);
                    return col;
                }

                ENDCG
            }
        }
    }
}
