// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-03-08      13:58
//  *LastModify：2019-03-08      13:58
//  ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SceneTextureMesh : MonoBehaviour
{
    public const string EventSetTexture = "EventSetTexture";
    public const string EventUpdateMesh = "EventUpdateMesh";
    public int ID;
    public float distanceForFarClipPlane = 3;

    public bool loadOnAwake = true;

    public Camera  defaultCamera;
    public string textureName;

    #region Camera fields
    private MeshFilter m_uiMesh;
    private MeshRenderer m_uiRenderer;
    private float m_cameraFarClip = -1;
    private float m_cameraFoV = -1;
    private float m_cameraSize = -1;
    #endregion

    private int m_uiMeshRef = 0;
    protected Vector3[] m_tmpVertices = new Vector3[4];
    private static readonly List<Vector2> m_tmpUVs = new List<Vector2>() { new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 1) };
    private static readonly int[] m_tmpIndices = new int[] { 0, 2, 1, 1, 3, 0 };

    public void Awake()
    {
        m_uiMesh     = GetComponent<MeshFilter>();
        m_uiRenderer = GetComponent<MeshRenderer>();

        EventManager.AddEventListener(Module_Home.EventSetTexture, OnSetTexture);
        EventManager.AddEventListener(Module_Home.EventUpdateMesh, OnUpdateMesh);
        EventManager.AddEventListener(Module_Home.EventSetScene,    OnSetScene);

        ResetCameraCache();

        if (defaultCamera)
            UpdateBackgroundQuad(defaultCamera, true);

        if (loadOnAwake) Load();
        else EventManager.AddEventListener(Events.SCENE_LOAD_COMPLETE, Load);
    }

    public void Load()
    {
        EventManager.RemoveEventListener(Events.SCENE_LOAD_COMPLETE, Load);

        if (string.IsNullOrEmpty(textureName)) return;

        var assests = new List<string>();
        assests.Add(textureName);
        Level.PrepareAssets(assests, b =>
        {
            var t = Level.GetPreloadObject<Texture>(textureName, false);
            SetTexture(t);
        });
    }

    private void OnSetScene(Event_ e)
    {
        var show = (bool)e.param2;
        if(show)
            m_uiMesh.SafeSetActive(false);
    }

    public void OnDestroy()
    {
        EventManager.RemoveEventListener(this);
    }


    /// <summary>
    /// 重置相机缓存。确保切换场景后第一次必定能刷新网格
    /// </summary>
    private void ResetCameraCache()
    {
        m_cameraFarClip = -1;
        m_cameraFoV = -1;
        m_cameraSize = -1;

        m_uiMeshRef = 0;
    }

    private void OnUpdateMesh(Event_ e)
    {
        if (e.param1 == null || (int) (e.param1) != ID)
            return;
        var camera = e.param2 as Camera;
        if (null == camera) return;

        if(e.param3 == null)
            UpdateBackgroundQuad(camera, true);
        else
            UpdateBackgroundQuad(camera, (bool)(e.param3));
    }

    private void OnSetTexture(Event_ e)
    {
        if (e.param1 == null || (int)(e.param1) != ID)
            return;

        if (null == e.param2)
        {
            if (m_uiMeshRef > 0)
                --m_uiMeshRef;
            else
                Logger.LogError($"m_uiMeshRef 引用异常");
            m_uiMesh.SafeSetActive(m_uiMeshRef > 0);
        }
        else
        {
            ++m_uiMeshRef;
            m_uiMesh.SafeSetActive(m_uiMeshRef > 0);

            var bgArr = (RawImage[])e.param2;
            if (bgArr?.Length > 0)
                SetTexture(bgArr[0].texture);
            SetAddTexture(bgArr[0], bgArr?.Length > 1 ? bgArr[1] : null);
        }

        Logger.LogDetail($"SceneTextureMesh: <b>{name}</b> ref count set to <b>{m_uiMeshRef}</b>");
    }

    private void SetTexture(Texture rTexture)
    {
        if (!m_uiRenderer.material)
            return;
        m_uiRenderer.material.SetTexture("_MainTex", rTexture);
    }

    private void SetAddTexture(RawImage rMain, RawImage rAdd)
    {
        if (!m_uiRenderer.material || !rMain)
            return;
        if (!rAdd)
        {
            m_uiRenderer.material.SetTexture("_AddTex", null);
            m_uiRenderer.material.SetVector("_Rect", Vector4.zero);
            return;
        }
        var ar = rAdd.rectTransform();
        Vector4 v = new Vector4();
        var m = new Vector2(1584.0f, 720.0f);
        v.x = (ar.offsetMin.x + m.x * 0.5f)/m.x;
        v.y = (ar.offsetMin.y + m.y * 0.5f)/ m.y;
        v.z = v.x + ar.rect.width/m.x;
        v.w = v.y + ar.rect.height/ m.y;

        m_uiRenderer.material.SetTexture("_AddTex", rAdd.texture);
        m_uiRenderer.material.SetVector("_Rect", v);
        m_uiRenderer.material.SetColor("_AddTexColor", rAdd.color);
    }

    private void UpdateBackgroundQuad(Camera rCamera, bool forceUpdate = false)
    {
        if (!rCamera || !(m_uiMesh?.mesh)) return;

        if (!forceUpdate
            && m_cameraFarClip == rCamera.farClipPlane
            && m_cameraFoV == rCamera.fieldOfView
            && m_cameraSize == rCamera.orthographicSize) return; // Nothing changed...

        if (m_uiMesh.mesh == null)
            m_uiMesh.mesh = new Mesh();

        m_cameraSize = rCamera.orthographicSize;
        m_cameraFarClip = rCamera.farClipPlane;
        m_cameraFoV = rCamera.fieldOfView;

        var fix = (GeneralConfigInfo.smaxAspectRatio / UIManager.aspect - 1.0f) * 0.5f;
        var far = rCamera.farClipPlane - distanceForFarClipPlane; // Make far distance a little smaller than clip distance, to avoid clipping
        var t = rCamera.transform;
        var p = t.position;
        var r = t.rotation;

        t.position = Vector3.zero;
        t.rotation = Quaternion.identity;
        rCamera.ResetWorldToCameraMatrix();

        if (m_tmpVertices == null) m_tmpVertices = new Vector3[4];

        // Now calculate far clip plane quad
        var a = rCamera.ViewportToWorldPoint(new Vector3(1 + fix, 0, far));
        var b = rCamera.ViewportToWorldPoint(new Vector3(0 - fix, 1, far));
        var c = rCamera.ViewportToWorldPoint(new Vector3(0 - fix, 0, far));
        var d = rCamera.ViewportToWorldPoint(new Vector3(1 + fix, 1, far));

        a.z = b.z = c.z = d.z = 0; // Vertices do not need Z position, we set it to tansform node's local position
        // Reset camera matrix
        t.position = p;
        t.rotation = r;

        m_tmpVertices[0] = a;
        m_tmpVertices[1] = b;
        m_tmpVertices[2] = c;
        m_tmpVertices[3] = d;

        m_uiMesh.mesh.vertices = m_tmpVertices;
        m_uiMesh.mesh.SetIndices(m_tmpIndices, MeshTopology.Triangles, 0);
        m_uiMesh.mesh.SetUVs(0, m_tmpUVs);

        if (m_uiRenderer)
        {
            m_uiRenderer.transform.localPosition = new Vector3(0, 0, far);
        }
    }
}
