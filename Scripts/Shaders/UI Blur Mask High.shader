/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* A simple UI Blur mask effect
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.1
* Created:  2018-01-18
*
***************************************************************************************************/

Shader "HYLR/UI/BlurMaskHigh"
{
    Properties
    {
        _Blur ("Blur", Range(0, 0.05)) = 0

        [HideInInspector] _Color("Tint", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }

        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        CGINCLUDE

        #include "UnityCG.cginc"
        #include "UnityUI.cginc"

        struct a2v
        {
            float4 vertex   : POSITION;
            float4 color    : COLOR;
            float2 texcoord : TEXCOORD0;
        };

        struct v2f
        {
            float4 vertex   : SV_POSITION;
            fixed4 color    : COLOR0;
            float4 grabPos  : COLOR1;
        };

        fixed _Blur;
        fixed4 _Color;

        v2f vert(a2v v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);

            float4 grabUV = ComputeGrabScreenPos(o.vertex);
            o.grabPos = grabUV;
            o.color   = v.color * _Color;

            return o;
        }

        half3 blur(sampler2D tex, float4 uv, fixed2 offset)
        {
            // Kernel width 35 x 35

            half3 color = (0);
            half2 off = offset.xy * 0.66293;

            color += (tex2Dproj(tex, float4(uv.xy + off, uv.zw)).rgb + tex2Dproj(tex, float4(uv.xy - off, uv.zw)).rgb) * 0.10855; off = offset.xy * 2.47904;
            color += (tex2Dproj(tex, float4(uv.xy + off, uv.zw)).rgb + tex2Dproj(tex, float4(uv.xy - off, uv.zw)).rgb) * 0.13135; off = offset.xy * 4.46232;
            color += (tex2Dproj(tex, float4(uv.xy + off, uv.zw)).rgb + tex2Dproj(tex, float4(uv.xy - off, uv.zw)).rgb) * 0.10406; off = offset.xy * 6.44568;
            color += (tex2Dproj(tex, float4(uv.xy + off, uv.zw)).rgb + tex2Dproj(tex, float4(uv.xy - off, uv.zw)).rgb) * 0.07216; off = offset.xy * 8.42917;
            color += (tex2Dproj(tex, float4(uv.xy + off, uv.zw)).rgb + tex2Dproj(tex, float4(uv.xy - off, uv.zw)).rgb) * 0.04380; off = offset.xy * 10.41281;
            color += (tex2Dproj(tex, float4(uv.xy + off, uv.zw)).rgb + tex2Dproj(tex, float4(uv.xy - off, uv.zw)).rgb) * 0.02328; off = offset.xy * 12.39664;
            color += (tex2Dproj(tex, float4(uv.xy + off, uv.zw)).rgb + tex2Dproj(tex, float4(uv.xy - off, uv.zw)).rgb) * 0.01083; off = offset.xy * 14.38070;
            color += (tex2Dproj(tex, float4(uv.xy + off, uv.zw)).rgb + tex2Dproj(tex, float4(uv.xy - off, uv.zw)).rgb) * 0.00441; off = offset.xy * 16.36501;
            color += (tex2Dproj(tex, float4(uv.xy + off, uv.zw)).rgb + tex2Dproj(tex, float4(uv.xy - off, uv.zw)).rgb) * 0.00157;

            return color;
        }

        ENDCG

        GrabPass { }

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            sampler2D _GrabTexture;

            half4 frag(v2f i) : SV_Target
            {
                half3 color = (0);

                color += blur(_GrabTexture, i.grabPos, fixed2( _Blur,  _Blur));
                color += blur(_GrabTexture, i.grabPos, fixed2( _Blur, -_Blur));

                color *= i.color.rgb * 0.5;

                return half4(color, i.color.a);
            }

            ENDCG
        }

        GrabPass { }

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            sampler2D _GrabTexture;

            fixed4 frag(v2f i) : SV_Target
            {
                half3 color = (0);

                color += blur(_GrabTexture, i.grabPos, fixed2(_Blur, 0));
                color += blur(_GrabTexture, i.grabPos, fixed2(0, _Blur));

                color *= i.color.rgb * 0.5;

                return half4(color.rgb, i.color.a);
            }

            ENDCG
        }

        GrabPass { }

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            sampler2D _GrabTexture;

            fixed4 frag(v2f i) : SV_Target
            {
                half3 color = (0);

                color += blur(_GrabTexture, i.grabPos, fixed2(_Blur * 0.7, 0));
                color += blur(_GrabTexture, i.grabPos, fixed2(0, _Blur * 0.7));

                color *= i.color.rgb * 0.5;

                return half4(color.rgb, i.color.a);
            }

            ENDCG
        }

        GrabPass { }

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            sampler2D _GrabTexture;

            fixed4 frag(v2f i) : SV_Target
            {
                half3 color = (0);

                color += blur(_GrabTexture, i.grabPos, fixed2(_Blur * 0.4, _Blur * 0.2));
                color += blur(_GrabTexture, i.grabPos, fixed2(_Blur * 0.4, -_Blur * 0.2));

                color *= i.color.rgb * 0.5;

                return half4(color.rgb, i.color.a);
            }

            ENDCG
        }

        //GrabPass { }

        //Pass
        //{
        //    CGPROGRAM

        //    #pragma vertex vert
        //    #pragma fragment frag
        //    #pragma target 2.0

        //    sampler2D _GrabTexture;

        //    fixed4 frag(v2f i) : SV_Target
        //    {
        //        half3 color = (0);

        //        color += blur(_GrabTexture, i.grabPos, fixed2(_Blur * 0.2, _Blur * 0.1));
        //        color += blur(_GrabTexture, i.grabPos, fixed2(_Blur * 0.2, -_Blur * 0.1));

        //        color *= i.color.rgb * 0.5;

        //        return half4(color.rgb, i.color.a);
        //    }

        //    ENDCG
        //}
    }
}