// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:False,qofs:0,qpre:2,rntp:3,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:1,fgcg:0.7411765,fgcb:0.3529412,fgca:1,fgde:0.002,fgrn:4.51,fgrf:1142.46,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:33519,y:32920,varname:node_3138,prsc:2|diff-180-RGB,transm-4129-OUT,lwrap-4980-OUT,clip-180-A,voffset-578-OUT;n:type:ShaderForge.SFN_Tex2d,id:180,x:32581,y:32530,ptovrint:False,ptlb:node_180,ptin:_node_180,varname:node_180,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:ba02ca16437e11d41a50d34a9e15755a,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Vector3,id:4129,x:32581,y:32727,varname:node_4129,prsc:2,v1:1,v2:1,v3:1;n:type:ShaderForge.SFN_Vector3,id:4980,x:32581,y:32843,varname:node_4980,prsc:2,v1:1,v2:1,v3:1;n:type:ShaderForge.SFN_Vector3,id:1222,x:31860,y:33095,varname:node_1222,prsc:2,v1:1,v2:0,v3:0;n:type:ShaderForge.SFN_FragmentPosition,id:6280,x:31860,y:33215,varname:node_6280,prsc:2;n:type:ShaderForge.SFN_Dot,id:1834,x:32131,y:33158,varname:node_1834,prsc:2,dt:0|A-1222-OUT,B-6280-XYZ;n:type:ShaderForge.SFN_Multiply,id:9368,x:32323,y:33098,varname:node_9368,prsc:2|A-8746-OUT,B-1834-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8746,x:32119,y:33044,ptovrint:False,ptlb:node_8746,ptin:_node_8746,varname:node_8746,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.1;n:type:ShaderForge.SFN_Add,id:3121,x:32530,y:33172,varname:node_3121,prsc:2|A-9368-OUT,B-3691-T;n:type:ShaderForge.SFN_Time,id:3691,x:32323,y:33311,varname:node_3691,prsc:2;n:type:ShaderForge.SFN_Sin,id:8379,x:32712,y:33282,varname:node_8379,prsc:2|IN-3121-OUT;n:type:ShaderForge.SFN_Dot,id:470,x:32297,y:33529,varname:node_470,prsc:2,dt:0|A-445-RGB,B-1752-OUT;n:type:ShaderForge.SFN_Vector3,id:1752,x:31876,y:33622,varname:node_1752,prsc:2,v1:1,v2:0,v3:0;n:type:ShaderForge.SFN_Multiply,id:1228,x:32640,y:33605,varname:node_1228,prsc:2|A-470-OUT,B-5831-OUT;n:type:ShaderForge.SFN_ValueProperty,id:5831,x:32297,y:33740,ptovrint:False,ptlb:node_5831,ptin:_node_5831,varname:node_5831,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.2;n:type:ShaderForge.SFN_Multiply,id:9071,x:32955,y:33426,varname:node_9071,prsc:2|A-8379-OUT,B-1228-OUT;n:type:ShaderForge.SFN_Multiply,id:578,x:33173,y:33523,varname:node_578,prsc:2|A-9071-OUT,B-888-OUT;n:type:ShaderForge.SFN_Vector3,id:888,x:32955,y:33640,varname:node_888,prsc:2,v1:1,v2:0,v3:0;n:type:ShaderForge.SFN_VertexColor,id:445,x:31876,y:33485,varname:node_445,prsc:2;proporder:180-8746-5831;pass:END;sub:END;*/

Shader "HYLR/dynamic_grass" {
    Properties {
        _node_180 ("node_180", 2D) = "white" {}
        _node_8746 ("node_8746", Float ) = 0.1
        _node_5831 ("node_5831", Float ) = 0.2
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "Queue"="AlphaTest"
            "RenderType"="TransparentCutout"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _node_180; uniform float4 _node_180_ST;
            uniform float _node_8746;
            uniform float _node_5831;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float4 vertexColor : COLOR;
                LIGHTING_COORDS(3,4)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 node_3691 = _Time;
                v.vertex.xyz += ((sin(((_node_8746*dot(float3(1,0,0),mul(unity_ObjectToWorld, v.vertex).rgb))+node_3691.g))*(dot(o.vertexColor.rgb,float3(1,0,0))*_node_5831))*float3(1,0,0));
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float4 _node_180_var = tex2D(_node_180,TRANSFORM_TEX(i.uv0, _node_180));
                clip(_node_180_var.a - 0.5);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = dot( normalDirection, lightDirection );
                float3 w = float3(1,1,1)*0.5; // Light wrapping
                float3 NdotLWrap = NdotL * ( 1.0 - w );
                float3 forwardLight = max(float3(0.0,0.0,0.0), NdotLWrap + w );
                float3 backLight = max(float3(0.0,0.0,0.0), -NdotLWrap + w ) * float3(1,1,1);
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = (forwardLight+backLight) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float3 diffuseColor = _node_180_var.rgb;
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _node_180; uniform float4 _node_180_ST;
            uniform float _node_8746;
            uniform float _node_5831;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float4 vertexColor : COLOR;
                LIGHTING_COORDS(3,4)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 node_3691 = _Time;
                v.vertex.xyz += ((sin(((_node_8746*dot(float3(1,0,0),mul(unity_ObjectToWorld, v.vertex).rgb))+node_3691.g))*(dot(o.vertexColor.rgb,float3(1,0,0))*_node_5831))*float3(1,0,0));
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float4 _node_180_var = tex2D(_node_180,TRANSFORM_TEX(i.uv0, _node_180));
                clip(_node_180_var.a - 0.5);
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = dot( normalDirection, lightDirection );
                float3 w = float3(1,1,1)*0.5; // Light wrapping
                float3 NdotLWrap = NdotL * ( 1.0 - w );
                float3 forwardLight = max(float3(0.0,0.0,0.0), NdotLWrap + w );
                float3 backLight = max(float3(0.0,0.0,0.0), -NdotLWrap + w ) * float3(1,1,1);
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = (forwardLight+backLight) * attenColor;
                float3 diffuseColor = _node_180_var.rgb;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse;
                return fixed4(finalColor * 1,0);
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _node_180; uniform float4 _node_180_ST;
            uniform float _node_8746;
            uniform float _node_5831;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                float4 node_3691 = _Time;
                v.vertex.xyz += ((sin(((_node_8746*dot(float3(1,0,0),mul(unity_ObjectToWorld, v.vertex).rgb))+node_3691.g))*(dot(o.vertexColor.rgb,float3(1,0,0))*_node_5831))*float3(1,0,0));
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                float4 _node_180_var = tex2D(_node_180,TRANSFORM_TEX(i.uv0, _node_180));
                clip(_node_180_var.a - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
