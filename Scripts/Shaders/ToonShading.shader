/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Basic toon shading
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-06-29
 * 
 ***************************************************************************************************/

Shader "HYLR/Toon Shading"
{
    Properties
    {
        _Color ("Main Color", Color) = (1, 1, 1, 1)
        _Texture ("Main Texture", 2D) = "white" {}
        _Ramp ("Ramp Texture", 2D) = "white" {}
        _Outline ("Outline", Range(0, 1)) = 0.1
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry+3" }
        
        Pass
        {
            NAME "OUTLINE"
            
            Cull Front
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            float _Outline;
            fixed4 _OutlineColor;
            
            struct a2v
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
            };
            
            v2f vert (a2v v)
            {
                v2f o;
                
                float4 pos = mul(UNITY_MATRIX_MV, v.vertex); 
                float3 normal = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);  
                normal.z = -0.1;
                pos = pos + float4(normalize(normal), 0) * _Outline;
                o.pos = mul(UNITY_MATRIX_P, pos);
                
                return o;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                return float4(_OutlineColor.rgb, 1);               
            }
            
            ENDCG
        }
        
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            
            Cull Back
        
            CGPROGRAM
        
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_fwdbase
        
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "UnityShaderVariables.cginc"
            
            fixed4 _Color;
            sampler2D _Texture;
            float4 _Texture_ST;
            sampler2D _Ramp;

            struct a2v
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
                float4 tangent : TANGENT;
            }; 
        
            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                SHADOW_COORDS(3)
            };
            
            v2f vert (a2v v)
            {
                v2f o;
                
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX (v.texcoord, _Texture);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                TRANSFER_SHADOW(o);
                
                return o;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                fixed3 worldNormal = normalize(i.worldNormal);
                fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));

                fixed4 c = tex2D (_Texture, i.uv);
                fixed3 albedo = c.rgb * _Color.rgb;
                
                //fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb * albedo;
                
                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
                
                fixed diff = dot(worldNormal, worldLightDir);
                diff = (diff * 0.5 + 0.5) * atten;
                
                fixed3 diffuse = 1.18 * _LightColor0.rgb * albedo * tex2D(_Ramp, float2(diff, diff)).rgb;
                
                return fixed4(/*ambient + */diffuse, 1.0);
            }
        
            ENDCG
        }
    }
    FallBack "Diffuse"
}
