/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.1
* Created:  2017-06-29
*
***************************************************************************************************/

Shader "HYLR/Particles/Mask Alpha Blend"
{
    Properties
    {
        _TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)

        _MainTex("Particle Texture (A = Transparency)", 2D) = "white"{}
        _MaskTex ("Masked Texture", 2D) = "gray" {}
        _InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0

        [Toggle(_INVERTED)] _Inverted("Inverted", int) = 0
    }

    Category
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        AlphaTest Greater .01
        Cull Off Lighting Off ZWrite Off

        SubShader
        {
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest
                #pragma multi_compile_particles
                #pragma multi_compile __ _INVERTED

                #include "UnityCG.cginc"

                sampler2D _MainTex;
                sampler2D _MaskTex;
                fixed4 _TintColor;

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                    float2 texcoord1 : TEXCOORD1;
                };

                struct v2f
                {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;            
                    float2 texcoord1 : TEXCOORD1;    
                    #ifdef SOFTPARTICLES_ON
                    float4 projPos : TEXCOORD2;                
                    #endif
                };

                float4 _MainTex_ST;
                float4 _MaskTex_ST;

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

                    #ifdef SOFTPARTICLES_ON
                    o.projPos = ComputeScreenPos (o.vertex);
                    COMPUTE_EYEDEPTH(o.projPos.z);
                    #endif

                    o.color = v.color;
                    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.texcoord1 = TRANSFORM_TEX(v.texcoord1, _MaskTex);
                    return o;
                }

                sampler2D _CameraDepthTexture;
                float _InvFade;

                fixed4 frag(v2f i) : COLOR
                {
                    #ifdef SOFTPARTICLES_ON
                    float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
                    float partZ = i.projPos.z;
                    float fade = saturate (_InvFade * (sceneZ-partZ));
                    i.color.a *= fade;
                    #endif
                
                    float4 col = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
                    col.a *= tex2D(_MaskTex, i.texcoord1).r;
                    return col;
                }

                ENDCG
            }
        }
    }
}
