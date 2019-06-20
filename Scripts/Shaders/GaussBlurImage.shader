/********************************************************************
  
 FileName:   GaussBlurImage
 Created:    2018/11/23
 Author:     T.Moon
 Description: 高斯模糊 针对UI图像
  
 风云传说 Copyright(C), 2018-2019
*********************************************************************/
Shader "NineMoon/GaussBlurImage"
{
    Properties
    {
       [PerRendererData]_MainTex ("Texture", 2D) = "white" {}
       _BlurRadius ("BlurRadius", Range(1, 15)) = 8
       _TextureSize ("TextureSize", Float) = 512
    }
    SubShader
    {

        Cull Off ZWrite Off ZTest Always

        Tags
           { 
             "Queue"="Transparent" 
             "IgnoreProjector"="True" 
             "RenderType"="Transparent" 
             "PreviewType"="Plane"
             "CanUseSpriteAtlas"="True"
           }

           Fog{ Mode Off }
            Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM


            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            int _BlurRadius;
            float  _TextureSize;

           //高斯权重数据 通过二维高斯公式所得
            float GetGaussWeight(float x, float y, float sigma)
            {
                float sigma2 = pow(sigma, 2.0f);
                float left = 1 / (2 * sigma2 * 3.1415926f);
                float right = exp(-(x*x+y*y)/(2*sigma2));
                return left * right;
            }

            fixed4 GaussBlur(float2 uv)
            {
                //因为高斯函数中3σ以外的点的权重已经很小了，因此σ取半径r/3的值
                float sigma = (float)_BlurRadius / 3.0f;
                float4 col = float4(0, 0, 0, 0);
                for (int x = - _BlurRadius; x <= _BlurRadius; ++x)
                  {
                    for (int y = - _BlurRadius; y <= _BlurRadius; ++y)
                     {
                       float4 color = tex2D(_MainTex, uv + float2(x / _TextureSize, y / _TextureSize));

                       float weight = GetGaussWeight(x, y, sigma);
                       //计算此点的最终颜色
                       col += color * weight;
                      }
                  }
                return col;
            }

            v2f vert (appdata v)
             {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
              }

            fixed4 frag (v2f i) : SV_Target
             {

               fixed4 col = _BlurRadius<=2 ? tex2D(_MainTex,i.uv):  GaussBlur(i.uv);
               return col;
             }
            ENDCG
       }
  }
}