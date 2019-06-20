/****************************************************************************************************
* Copyright (C) 2017-2019 FengYunChuanShuo
*
* Base PostEffect class
*
* Author:   Y.Moon <chglove@live.cn>
* Version:  0.1
* Created:  2017-06-29
*
***************************************************************************************************/

using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public abstract class PostEffect : MonoBehaviour
{
    protected bool  supportHDRTextures = true;
    protected bool  supportDX11 = false;
    protected bool  isSupported = true;

    protected Material CheckShaderAndCreateMaterial(Shader shader, Material material)
    {
        if (!shader)
        {
            Logger.LogWarning("Missing shader in {0}", ToString());
            enabled = false;
            return null;
        }

        if (shader.isSupported && material && material.shader == shader)
            return material;

        if (!shader.isSupported)
        {
            NotSupported ();
            Logger.LogWarning("The shader {0} on effect {1} is not supported on this platform!", shader.ToString(), ToString());
            return null;
        }

        material = new Material (shader);
        material.hideFlags = HideFlags.DontSave;

        return material;
    }

    protected Material CreateMaterial(Shader s, Material m2Create)
    {
        if (!s)
        {
            Debug.Log ("Missing shader in " + ToString ());
            return null;
        }

        if (m2Create && (m2Create.shader == s) && (s.isSupported))
            return m2Create;

        if (!s.isSupported)
        {
            return null;
        }
        else
        {
            m2Create = new Material (s);
            m2Create.hideFlags = HideFlags.DontSave;
            if (m2Create)
                return m2Create;
            else return null;
        }
    }

    void OnEnable()
    {
        isSupported = true;
    }

    protected bool CheckSupport()
    {
        return CheckSupport(false);
    }

    protected virtual void Start()
    {
        if (!SystemInfo.supportsImageEffects)
        {
            Logger.LogWarning("PostEffect: Current platform does not support image effects...");
            enabled = false;
        }
    }

    protected bool CheckSupport(bool needDepth)
    {
        isSupported = true;
        supportHDRTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
        supportDX11 = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;

        if (!SystemInfo.supportsImageEffects)
        {
            NotSupported ();
            return false;
        }

        if (needDepth && !SystemInfo.SupportsRenderTextureFormat (RenderTextureFormat.Depth))
        {
            NotSupported ();
            return false;
        }

        if (needDepth)
            GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;

        return true;
    }

    protected bool CheckSupport(bool needDepth,  bool needHdr)
    {
        if (!CheckSupport(needDepth))
            return false;

        if (needHdr && !supportHDRTextures)
        {
            NotSupported ();
            return false;
        }

        return true;
    }

    public bool Dx11Support()
    {
        return supportDX11;
    }

    protected void ReportAutoDisable()
    {
        Logger.LogWarning("The image effect {0} has been disabled as it's not supported on the current platform.", ToString());
    }

    protected void NotSupported()
    {
        enabled = false;
        isSupported = false;
    }

    protected void DrawBorder(RenderTexture dest, Material material)
    {
        float x1;
        float x2;
        float y1;
        float y2;

        RenderTexture.active = dest;
        bool  invertY = true; // source.texelSize.y < 0.0ff;
        // Set up the simple Matrix
        GL.PushMatrix();
        GL.LoadOrtho();

        for (int i = 0; i < material.passCount; i++)
        {
            material.SetPass(i);

            float y1_; float y2_;
            if (invertY)
            {
                y1_ = 1.0f; y2_ = 0.0f;
            }
            else
            {
                y1_ = 0.0f; y2_ = 1.0f;
            }

            // left
            x1 = 0.0f;
            x2 = 0.0f + 1.0f/(dest.width*1.0f);
            y1 = 0.0f;
            y2 = 1.0f;
            GL.Begin(GL.QUADS);

            GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

            // right
            x1 = 1.0f - 1.0f/(dest.width*1.0f);
            x2 = 1.0f;
            y1 = 0.0f;
            y2 = 1.0f;

            GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

            // top
            x1 = 0.0f;
            x2 = 1.0f;
            y1 = 0.0f;
            y2 = 0.0f + 1.0f/(dest.height*1.0f);

            GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

            // bottom
            x1 = 0.0f;
            x2 = 1.0f;
            y1 = 1.0f - 1.0f/(dest.height*1.0f);
            y2 = 1.0f;

            GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

            GL.End();
        }

        GL.PopMatrix();
    }
}
