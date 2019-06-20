/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* Bllom effect
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.1
* Created:  2017-06-29
*
***************************************************************************************************/

using UnityEngine;

namespace HYLR
{
    public class Bloom : PostEffect
    {
        public Shader bloomShader;
        private Material bloomMaterial = null;

        public Material material
        {
            get
            {
                bloomMaterial = CheckShaderAndCreateMaterial(bloomShader, bloomMaterial);
                return bloomMaterial;
            }  
        }

        // Blur iterations - larger number means more blur.
        [Range(0, 4)]
        public int iterations = 3;
    
        // Blur spread for each iteration - larger value means more blur
        [Range(0.2f, 3.0f)]
        public float blurSpread = 0.6f;

        [Range(1, 8)]
        public int downSample = 2;

        [Range(-2f, 2f)]
        public float luminanceThreshold = 0.6f;

        void OnRenderImage (RenderTexture src, RenderTexture dest)
        {
            if (material)
            {
                material.SetFloat("_LuminanceThreshold", luminanceThreshold);

                var rtW = src.width/downSample;
                var rtH = src.height/downSample;
            
                var buffer0 = RenderTexture.GetTemporary(rtW, rtH, 0);
                buffer0.filterMode = FilterMode.Bilinear;
            
                Graphics.Blit(src, buffer0, material, 0);
            
                for (var i = 0; i < iterations; i++)
                {
                    material.SetFloat("_BlurSize", 1.0f + i * blurSpread);
                
                    RenderTexture buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);
                
                    // Render the vertical pass
                    Graphics.Blit(buffer0, buffer1, material, 1);
                
                    RenderTexture.ReleaseTemporary(buffer0);
                    buffer0 = buffer1;
                    buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);
                
                    // Render the horizontal pass
                    Graphics.Blit(buffer0, buffer1, material, 2);
                
                    RenderTexture.ReleaseTemporary(buffer0);
                    buffer0 = buffer1;
                }

                material.SetTexture ("_Bloom", buffer0);  
                Graphics.Blit (src, dest, material, 3);  

                RenderTexture.ReleaseTemporary(buffer0);
            }
            else
                Graphics.Blit(src, dest);
        }

        #region Debug

    #if DEVELOPMENT_BUILD || UNITY_EDITOR
        private float m_iterations_f;
        private float m_downSample_f;

        protected override void Start()
        {
            base.Start();

            m_iterations_f = iterations;
            m_downSample_f = downSample;
        }

        private void OnGUI()
        {
            if (Root.simulateReleaseMode || !Root.showDebugPanel || Root.hideAllGameUI) return;

            GUI.Label(new Rect(5, 335, 250, 30), "<color=#00FF00><size=25>iterations:</size></color>");
            m_iterations_f = GUI.HorizontalSlider(new Rect(5, 370, 500, 30), m_iterations_f, 0, 4);
            iterations = (int)m_iterations_f;

            GUI.Label(new Rect(5, 385, 250, 30), "<color=#00FF00><size=25>blurSpread:</size></color>");
            blurSpread = GUI.HorizontalSlider(new Rect(5, 425, 410, 30), blurSpread, 0.2f, 3);

            GUI.Label(new Rect(5, 435, 250, 30), "<color=#00FF00><size=25>downSample:</size></color>");
            m_downSample_f = GUI.HorizontalSlider(new Rect(5, 470, 500, 30), m_downSample_f, 1, 8);
            downSample = (int)m_downSample_f;

            GUI.Label(new Rect(5, 485, 250, 30), "<color=#00FF00><size=25>luminanceThreshold:</size></color>");
            luminanceThreshold = GUI.HorizontalSlider(new Rect(5, 520, 500, 30), luminanceThreshold, -1.5f, 1.5f);
        }
    #endif

        #endregion
    }
}