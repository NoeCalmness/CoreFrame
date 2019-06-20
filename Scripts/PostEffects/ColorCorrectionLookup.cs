/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Color correction LUT
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-08-11
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[AddComponentMenu ("HYLR/PostEffects/Color Correction (3D Lookup Texture)")]
public class ColorCorrectionLookup : PostEffect
{
    public Shader shader;
    private Material material;

    // serialize this instead of having another 2d texture ref'ed
    public Texture3D converted3DLut = null;
    public string basedOnTempTex = "";

    protected override void Start()
    {
        base.Start();

        if (!enabled) return;

        CheckSupport(false);

        if (!shader)
            shader = Shader.Find("Hidden/HYLR/PostEffects/ColorCorrection3DLut");
        material = CheckShaderAndCreateMaterial(shader, material);

        if (!isSupported || !SystemInfo.supports3DTextures)
            ReportAutoDisable();
    }

    void OnDisable ()
    {
        if (material)
        {
            DestroyImmediate(material);
            material = null;
        }
    }

    private void OnEnable()
    {
        if (!shader)
            shader = Shader.Find("Hidden/HYLR/PostEffects/ColorCorrection3DLut");
        if (!material) material = CheckShaderAndCreateMaterial(shader, material);
    }

    void OnDestroy()
    {
        if (converted3DLut)
            DestroyImmediate(converted3DLut);

        converted3DLut = null;
    }

    public void SetIdentityLut ()
    {
        int dim = 16;
        var newC = new Color[dim*dim*dim];
        float oneOverDim = 1.0f / (1.0f * dim - 1.0f);

        for(int i = 0; i < dim; i++)
            for(int j = 0; j < dim; j++)
                for(int k = 0; k < dim; k++)
                    newC[i + (j*dim) + (k*dim*dim)] = new Color((i*1.0f)*oneOverDim, (j*1.0f)*oneOverDim, (k*1.0f)*oneOverDim, 1.0f);

        if (converted3DLut)
            DestroyImmediate (converted3DLut);


        converted3DLut = new Texture3D (dim, dim, dim, TextureFormat.ARGB32, false);
        converted3DLut.SetPixels (newC);
        converted3DLut.Apply ();
        basedOnTempTex = "";
    }

    public bool ValidDimensions(Texture2D tex2d)
    {
        return tex2d && Mathf.FloorToInt(Mathf.Sqrt(tex2d.width)) == tex2d.height;
    }

    public void Convert( Texture2D temp2DTex, string path)
    {
        // conversion fun: the given 2D texture needs to be of the format
        //  w * h, wheras h is the 'depth' (or 3d dimension 'dim') and w = dim * dim

        if (temp2DTex)
        {
            int dim = temp2DTex.width * temp2DTex.height;
            dim = temp2DTex.height;

            if (!ValidDimensions(temp2DTex))
            {
                Logger.LogWarning("The given 2D texture {0} cannot be used as a 3D LUT.", temp2DTex.name);
                basedOnTempTex = "";
                return;
            }

            var c = temp2DTex.GetPixels();
            var newC = new Color[c.Length];

            for(int i = 0; i < dim; i++)
            {
                for(int j = 0; j < dim; j++)
                {
                    for(int k = 0; k < dim; k++)
                    {
                        int j_ = dim-j-1;
                        newC[i + (j*dim) + (k*dim*dim)] = c[k*dim+i+j_*dim*dim];
                    }
                }
            }

            if (converted3DLut)
                DestroyImmediate (converted3DLut);

            converted3DLut = new Texture3D (dim, dim, dim, TextureFormat.ARGB32, false);
            converted3DLut.SetPixels (newC);
            converted3DLut.Apply ();
            basedOnTempTex = path;
        }
        else
            Logger.LogError("Couldn't color correct with 3D LUT texture. Image Effect will be disabled.");
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!SystemInfo.supports3DTextures)
        {
            Graphics.Blit (source, destination);
            return;
        }

        if (converted3DLut == null)
        {
            SetIdentityLut ();
        }

        int lutSize = converted3DLut.width;
        converted3DLut.wrapMode = TextureWrapMode.Clamp;
        material.SetFloat("_Scale", (lutSize - 1) / (1.0f*lutSize));
        material.SetFloat("_Offset", 1.0f / (2.0f * lutSize));
        material.SetTexture("_ClutTex", converted3DLut);

        Graphics.Blit (source, destination, material, QualitySettings.activeColorSpace == ColorSpace.Linear ? 1 : 0);
    }
}
