// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:32829,y:32706,varname:node_3138,prsc:2|alpha-2085-OUT,refract-2385-OUT;n:type:ShaderForge.SFN_Tex2d,id:7236,x:32079,y:32927,ptovrint:False,ptlb:Maintex,ptin:_Maintex,varname:node_7236,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:4991,x:32303,y:32991,varname:node_4991,prsc:2|A-7236-A,B-3223-OUT;n:type:ShaderForge.SFN_Multiply,id:2385,x:32549,y:33002,varname:node_2385,prsc:2|A-4991-OUT,B-3903-A,C-81-OUT,D-7555-OUT;n:type:ShaderForge.SFN_VertexColor,id:3903,x:32303,y:33121,varname:node_3903,prsc:2;n:type:ShaderForge.SFN_Slider,id:81,x:32213,y:33329,ptovrint:False,ptlb:Diffuse_power,ptin:_Diffuse_power,varname:node_81,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:5;n:type:ShaderForge.SFN_Vector1,id:2085,x:32537,y:32927,varname:node_2085,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector2,id:3223,x:32068,y:33139,varname:node_3223,prsc:2,v1:0.1,v2:-0.1;n:type:ShaderForge.SFN_Append,id:7555,x:32303,y:32858,varname:node_7555,prsc:2|A-7236-R,B-7236-G;proporder:7236-81;pass:END;sub:END;*/

Shader "HYLR/Particles/HeatDestrot_sy02"
{
    Properties
    {
        _Maintex ("Maintex", 2D) = "white" {}
        _Diffuse_power ("Diffuse_power", Range(0, 5)) = 1
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }

//    SubShader
//    {
//        Tags
//        {
//            "IgnoreProjector"="True"
//            "Queue"="Transparent"
//            "RenderType"="Transparent"
//        }
//
////        GrabPass{ }
//        Pass
//        {
//            Name "FORWARD"
//            Tags
//            {
//                "LightMode"="ForwardBase"
//            }
//            Blend SrcAlpha OneMinusSrcAlpha
//            Cull Off
//            ZWrite Off
//            
//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            #define UNITY_PASS_FORWARDBASE
//            #include "UnityCG.cginc"
//            #pragma multi_compile_fwdbase
//            #pragma only_renderers d3d9 d3d11 glcore gles n3ds wiiu 
//            #pragma target 3.0
//            uniform sampler2D _GrabTexture;
//            uniform sampler2D _Maintex; uniform float4 _Maintex_ST;
//            uniform float _Diffuse_power;
//            struct VertexInput {
//                float4 vertex : POSITION;
//                float2 texcoord0 : TEXCOORD0;
//                float4 vertexColor : COLOR;
//            };
//            struct VertexOutput {
//                float4 pos : SV_POSITION;
//                float2 uv0 : TEXCOORD0;
//                float4 screenPos : TEXCOORD1;
//                float4 vertexColor : COLOR;
//            };
//            VertexOutput vert (VertexInput v) {
//                VertexOutput o = (VertexOutput)0;
//                o.uv0 = v.texcoord0;
//                o.vertexColor = v.vertexColor;
//                o.pos = UnityObjectToClipPos( v.vertex );
//                o.screenPos = o.pos;
//                return o;
//            }
//            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
//                float isFrontFace = ( facing >= 0 ? 1 : 0 );
//                float faceSign = ( facing >= 0 ? 1 : -1 );
//                #if UNITY_UV_STARTS_AT_TOP
//                    float grabSign = -_ProjectionParams.x;
//                #else
//                    float grabSign = _ProjectionParams.x;
//                #endif
//                i.screenPos = float4( i.screenPos.xy / i.screenPos.w, 0, 0 );
//                i.screenPos.y *= _ProjectionParams.x;
//                float4 _Maintex_var = tex2D(_Maintex,TRANSFORM_TEX(i.uv0, _Maintex));
//                float2 sceneUVs = float2(1,grabSign)*i.screenPos.xy*0.5+0.5 + ((_Maintex_var.a*float2(0.1,-0.1))*i.vertexColor.a*_Diffuse_power*float2(_Maintex_var.r,_Maintex_var.g));
//                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
//////// Lighting:
//                float3 finalColor = 0;
//                return fixed4(lerp(sceneColor.rgb, finalColor,0.0),1);
//            }
//            ENDCG
//        }
//    }

    SubShader
    {
        AlphaTest Less 0
        Pass { }
    }
}
