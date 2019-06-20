/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* Multiply Addtive Particle Shader
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.2
* Created:  2018-03-29
*
***************************************************************************************************/

Shader "HYLR/Particles/Additive Multiply"
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
        Blend One OneMinusSrcAlpha
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
                    UNITY_VERTEX_INPUT_INSTANCE_ID
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

                    o.color = v.color;
                    o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {

                    fixed4 tex = tex2D(_MainTex, i.texcoord);
                    fixed4 col;
                    col.rgb = _TintColor.rgb * tex.rgb * i.color.rgb * 2.0f;
                    col.a = (1 - tex.a) * (_TintColor.a * i.color.a * 2.0f);
                    return col;
                }

                ENDCG
            }
        }
    }
}
