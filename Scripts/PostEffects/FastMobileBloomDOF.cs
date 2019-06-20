/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Simple bloom and DOF effects for mobile platform
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-08-11
 * 
 ***************************************************************************************************/

using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[AddComponentMenu("HYLR/PostEffects/FastMobileBloom" )]
public class FastMobileBloomDOF : PostEffect
{
    public bool EnableBloom = true;
    public bool EnableDOF = true;
    public bool EnableRadialBlur = true;

    [Range( 0.25f, 5.5f )]
    public float BlurSize = 1.0f;
    [Range( 1, 4 )]
    public int BlurIterations = 2;
    [Range( 0.0f, 1.5f )]
    public float bloomThreshold = 0.25f;
    [Range( 0.0f, 2.5f )]
    public float BloomIntensity = 1.0f;

    public Transform DOFFocalTransform;              // 聚焦物体
    public float DOFFocalFix    = 0f;
    public float DOFFocalLength = 3f;                // 焦点距离（焦点到相机的距离）
    public float DOFFocalSize = 0.2f;                // 景深大小
    public float DOFAperture = 2f;                   // 光圈（景深系数，光圈越大景深越浅）

    [Range( 0.0f, 1.0f )]
    public float RadialBlurCenterX = 0.5f;            // 径向模糊中心屏幕横坐标
    [Range( 0.0f, 1.0f )]
    public float RadialBlurCenterY = 0.5f;            // 径向模糊中心屏幕纵坐标
    [Range( -0.5f, 0.5f )]
    public float RadialBlurSampleDistance = 0.1f;     // 径向模糊采样距离
    [Range( 0.0f, 8.0f )]
    public float RadialBlurStrength = 3.0f;           // 径向模糊强度

    Camera _camera;
    Shader _shader;
    Material _material;


    public RenderTexture alphaChannel;

    public void CopyFrom(FastMobileBloomDOF other)
    {
        if (!other) return;

        EnableBloom              = other.EnableBloom;
        EnableDOF                = other.EnableDOF;
        EnableRadialBlur         = other.EnableRadialBlur;

        BlurSize                 = other.BlurSize;
        BlurIterations           = other.BlurIterations;
        bloomThreshold           = other.bloomThreshold;
        BloomIntensity           = other.BloomIntensity;

        DOFFocalTransform        = other.DOFFocalTransform;

        DOFFocalLength           = other.DOFFocalLength;
        DOFFocalSize             = other.DOFFocalSize;
        DOFAperture              = other.DOFAperture;

        RadialBlurCenterX        = other.RadialBlurCenterX;
        RadialBlurCenterY        = other.RadialBlurCenterY;

        RadialBlurSampleDistance = other.RadialBlurSampleDistance;
        RadialBlurStrength       = other.RadialBlurStrength;
}

    void OnEnable()
    {
        _camera = GetComponent<Camera>();

        if (!_shader)
            _shader = Shader.Find("Hidden/HYLR/PostEffects/FastMobilePostProcessing");

        if (!_material) _material = CheckShaderAndCreateMaterial(_shader, _material);
    }

    void OnDisable()
    {
        if( _material )
            DestroyImmediate( _material );
    }

    protected override void Start()
    {
        base.Start();

        if (!enabled) return;

        if(!EnableBloom && !EnableDOF && !(EnableRadialBlur && RadialBlurStrength > 0f))
        {
            enabled = false;
            return;
        }

        CheckSupport(EnableDOF);

        if(!_shader)
            _shader = Shader.Find("Hidden/HYLR/PostEffects/FastMobilePostProcessing");

        _material = CheckShaderAndCreateMaterial(_shader, _material);

        if(!isSupported ) ReportAutoDisable();
    }

    void OnRenderImage( RenderTexture sourceRT, RenderTexture destinationRT)
    {
        if (!SystemInfo.supports3DTextures)
        {
            Graphics.Blit(sourceRT, destinationRT);
            return;
        }

        if (alphaChannel) Graphics.Blit(sourceRT, alphaChannel, _material, 5);

        _camera.depthTextureMode = EnableDOF ? DepthTextureMode.Depth : DepthTextureMode.None;

        RenderTexture blurredRT = null;
        RenderTexture radialBlurredRT = null;
        RenderTexture srcRT = sourceRT;
        RenderTexture destRT = destinationRT;

        if (EnableBloom || EnableDOF )
        {
            // Initial downsample
            blurredRT = RenderTexture.GetTemporary( sourceRT.width / 4, sourceRT.height / 4, 0, sourceRT.format );
            blurredRT.filterMode = FilterMode.Bilinear;

            _material.SetFloat( "_BlurSize", BlurSize );
            if( EnableBloom )
            {
                _material.SetFloat( "_BloomThreshold", bloomThreshold );
                _material.SetFloat( "_BloomIntensity", BloomIntensity );
            }
            Graphics.Blit( sourceRT, blurredRT, _material, 0 );

            // Downscale
            for( int i = 0; i < BlurIterations - 1; ++i )
            {
                RenderTexture blurredRT2 = RenderTexture.GetTemporary( blurredRT.width / 2, blurredRT.height / 2, 0, sourceRT.format );
                blurredRT2.filterMode = FilterMode.Bilinear;

                Graphics.Blit( blurredRT, blurredRT2, _material, 0 );

                RenderTexture.ReleaseTemporary( blurredRT );
                blurredRT = blurredRT2;
            }
            // Upscale
            for( int i = 0; i < BlurIterations - 1; ++i )
            {
                RenderTexture blurredRT2 = RenderTexture.GetTemporary( blurredRT.width * 2, blurredRT.height * 2, 0, sourceRT.format );
                blurredRT2.filterMode = FilterMode.Bilinear;

                Graphics.Blit( blurredRT, blurredRT2, _material, 1 );

                RenderTexture.ReleaseTemporary( blurredRT );
                blurredRT = blurredRT2;
            }

            _material.SetTexture( "_BlurredTex", blurredRT );

            if( EnableRadialBlur && RadialBlurStrength > 0f )
            {
                destRT = RenderTexture.GetTemporary( sourceRT.width, sourceRT.height, 0, sourceRT.format );
                destRT.filterMode = FilterMode.Bilinear;

                srcRT = destRT;
            }
        }

        if( EnableBloom )
        {
            _material.EnableKeyword( "BLOOM_ON" );
        }
        else
        {
            _material.DisableKeyword( "BLOOM_ON" );
        }
        if( EnableDOF )
        {
            if (DOFFocalTransform != null)
            {
                DOFFocalLength = _camera.WorldToScreenPoint( DOFFocalTransform.position ).z;
            }
            _material.SetFloat( "_FocalLength", (DOFFocalLength + DOFFocalFix) / _camera.farClipPlane );
            _material.SetFloat( "_FocalSize", DOFFocalSize );
            _material.SetFloat( "_Aperture", DOFAperture );
            _material.EnableKeyword( "DOF_ON" );
        }
        else
        {
            _material.DisableKeyword( "DOF_ON" );
        }

        Graphics.Blit( sourceRT, destRT, _material, 3 );
    
        RenderTexture.ReleaseTemporary( blurredRT );

        if( EnableRadialBlur && RadialBlurStrength > 0f )
        {
            radialBlurredRT = RenderTexture.GetTemporary( srcRT.width / 2, srcRT.height / 2, 0, srcRT.format );
            radialBlurredRT.filterMode = FilterMode.Bilinear;

            _material.SetFloat( "_RadialBlurCenterX", RadialBlurCenterX );
            _material.SetFloat( "_RadialBlurCenterY", RadialBlurCenterY );
            _material.SetFloat( "_RadialBlurSampleDistance", RadialBlurSampleDistance );
            _material.SetFloat( "_RadialBlurStrength", RadialBlurStrength );

            Graphics.Blit( srcRT, radialBlurredRT, _material, 2 );

            _material.SetTexture( "_RadialBlurredTex", radialBlurredRT );

            Graphics.Blit( srcRT, destinationRT, _material, 4 );

            srcRT = null;
            RenderTexture.ReleaseTemporary( destRT );
            RenderTexture.ReleaseTemporary( radialBlurredRT );
        }
    }
}
