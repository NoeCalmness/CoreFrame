/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * UI render character helper class.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-01-03
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

[AddComponentMenu("HYLR/Utilities/UICharacter")]
[RequireComponent(typeof(RawImage))]
public class UICharacter : LinkedWindowBehaviour, IDragHandler
{
    #region Static functions

    public static RenderTexture GetCachedTexture(Camera camera, bool create = true)
    {
        if (!camera) return null;
        return GetCachedTexture(camera.GetInstanceID(), create);
    }

    public static RenderTexture GetCachedTexture(int camera, bool create = true)
    {
        var rt = m_cachedRTs.Get(camera);
        if (!rt)
        {
            rt = GetTempRenderTexture((int)UIManager.maxResolution.x, (int)UIManager.maxResolution.y);
            rt.name += "_" + camera;
            m_cachedRTs.Add(camera, rt);
        }
        return rt;
    }

    public static void RemoveCachedTexture(Camera camera)
    {
        if (!camera) return;
        RemoveCachedTexture(camera.GetInstanceID());
    }

    public static void RemoveCachedTexture(int camera)
    {
        RenderTexture rt = null;
        if (!m_cachedRTs.TryGetValue(camera, out rt)) return;
        m_cachedRTs.Remove(camera);

        if (rt) RenderTexture.ReleaseTemporary(rt);
    }

    public static void ClearCache()
    {
        foreach (var rt in m_cachedRTs.Values)
        {
            if (!rt) continue;
            RenderTexture.ReleaseTemporary(rt);
        }

        m_cachedRTs.Clear();
    }

    private static Dictionary<int, RenderTexture> m_cachedRTs = new Dictionary<int, RenderTexture>();
    private static Dictionary<int, List<int>> m_instanceCount = new Dictionary<int, List<int>>();
    private static int s_refs = 0;
    
    private static int GetInstanceCount(Camera camera)
    {
        if (!camera) return 0;
        var id = camera.GetInstanceID();
        var refs = m_instanceCount.Get(id);

        return refs != null ? refs.Count : 0;
    }

    private static void AddInstance(Camera camera, int instance)
    {
        if (!camera) return;
        var id = camera.GetInstanceID();
        var refs = m_instanceCount.GetDefault(id);
        refs.Remove(instance);
        refs.Add(instance);

//        Logger.LogError($"{camera.name } add {instance} {refs.Count}");
    }

    private static void RemoveInstance(Camera camera, int instance)
    {
        var id = camera ? camera.GetInstanceID() : -1;
        if (id < 0) return;

        var refs = m_instanceCount.Get(id);
        if (refs == null) return;

        refs.Remove(instance);

//        Logger.LogError($"{camera.name } remove {instance} {refs.Count}");

        if (refs.Count < 1)
        {
            camera.targetTexture = null;

            RemoveCachedTexture(id);
            UIManager.FixCameraRect(camera);
        }
    }
    
    private static RenderTexture GetTempRenderTexture(int width, int height, int msaa = -1, int depthBuffer = 24, RenderTextureReadWrite rw = RenderTextureReadWrite.Default, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp, string name = "TempUIRT")
    {
        msaa = msaa < 0 ? SettingsManager.msaaLevel : msaa < 2 ? 1 : msaa < 4 ? 2 : msaa < 8 ? 4 : 8;
        var rt = RenderTexture.GetTemporary(width, height, depthBuffer, Application.isMobilePlatform ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default, rw, msaa < 1 ? 1 : msaa);

        rt.filterMode   = filterMode;
        rt.wrapMode     = wrapMode;
        rt.name         = name;

        return rt;
    }

    #endregion

    #region Fields

    public string cameraName
    {
        get { return m_cameraName; }
        set
        {
            if (m_cameraName == value) return;
            m_cameraName = value;

            m_valid = !(string.IsNullOrEmpty(m_cameraName) || string.IsNullOrWhiteSpace(m_cameraName));
            UpdateCamera();
        }
    }
    public string dragTargetName
    {
        get { return m_dragTargetName; }
        set
        {
            if (m_dragTargetName == value) return;
            m_dragTargetName = value;

            m_dragValid = !(string.IsNullOrEmpty(m_dragTargetName) || string.IsNullOrWhiteSpace(m_dragTargetName));
            UpdateCamera();
        }
    }
    public bool enableDrag
    {
        get { return m_enableDrag; }
        set
        {
            if (m_enableDrag == value) return;
            m_enableDrag = value;

            if (m_image) m_image.raycastTarget = m_enableDrag;
        }
    }
    public Vector2 offset
    {
        get { return m_offset; }
        set
        {
            if (m_offset == value) return;
            m_offset = value;

            if (m_camera && m_camera.targetTexture)
                UpdateUVRect();
        }
    }
    public Vector2 pivot
    {
        get { return m_pivot; }
        set
        {
            if (m_pivot == value) return;
            m_pivot = value;

            if (m_camera && m_camera.targetTexture)
                UpdateUVRect();
        }
    }
    public float scale
    {
        get { return m_scale; }
        set
        {
            if (m_scale == value) return;
            m_scale = value;

            if (m_camera && m_camera.targetTexture)
                UpdateUVRect();
        }
    }

    public bool realTime
    {
        get { return m_realTime; }
        set
        {
            if (m_realTime == value)
                return;
            m_realTime = value;
            if(Application.isPlaying)
                UpdateCamera();
        }
    }

    public new Camera camera { get { return m_camera; } }
    public Transform dragTarget { get { return m_dragTarget; } }

    public bool setCamera
    {
        get { return m_setCamera; }
        set
        {
            if (m_setCamera == value) return;
            m_setCamera = value;

            UpdateCameraConfig();
        }
    }
    public Vector3 cameraPosition
    {
        get { return m_cameraPosition; }
        set
        {
            if (m_cameraPosition == value) return;
            m_cameraPosition = value;

            UpdateCameraConfig();
        }
    }
    public Vector3 cameraRotation
    {
        get { return m_cameraRotation; }
        set
        {
            if (m_cameraRotation == value) return;
            m_cameraRotation = value;

            UpdateCameraConfig();
        }
    }
    public float fov
    {
        get { return m_fov; }
        set
        {
            if (m_fov == value) return;
            m_fov = value;

            UpdateCameraConfig();
        }
    }
    public string excludeLayers
    {
        get { return m_excludeLayers; }
        set
        {
            if (m_excludeLayers == value) return;
            m_excludeLayers = value;

            UpdateExcludeLayers();
        }
    }

    public float dragSpeed = 1.0f;

    [SerializeField, Set("realTime"), Space(15)]
    public bool m_realTime = false;
    public RawImage[] bg;
    public string sceneName;

    [SerializeField, Set("offset")]
    private Vector2 m_offset = new Vector2(0, 0);
    [SerializeField, Set("pivot"), Space(15)]
    private Vector2 m_pivot = new Vector2(0.5f, 0.5f);
    [SerializeField, _Range(0.1f, 10.0f, "scale"), Space(15)]
    private float m_scale = 1.0f;
    [SerializeField, Set("cameraName"), Space(15)]
    private string m_cameraName = "main";
    [SerializeField, Set("dragTargetName")]
    private string m_dragTargetName = "player";
    [SerializeField, Set("enableDrag")]
    private bool m_enableDrag = true;

    public bool tweenAnimation = true;
    public float tweenDuration = 0.5f;

    [Header("Camera Options"), SerializeField, Set("setCamera")]
    private bool m_setCamera = false;
    [SerializeField, Set("cameraPosition")]
    private Vector3 m_cameraPosition;
    [SerializeField, Set("cameraRotation"), Space(15)]
    private Vector3 m_cameraRotation;
    [SerializeField, Set("fov"), Space(20)]
    private float m_fov;
    [SerializeField, Set("excludeLayers")]
    private string m_excludeLayers = "mainNode";

    private bool m_isMainCamera = false;
    private bool m_valid = false;
    private bool m_dragValid = false;

    private Camera m_camera;
    private Transform m_dragTarget;
    private RawImage m_image;
    private Material m_mat;

    private RenderTexture m_snapshot;

    private int m_excludeMask = 0;
    private int m_cameraMask  = 0;
    private int m_dof = -1;   // @TODO: Fix DoF settings and remove this temp fix

    #endregion

    #region Linked window

    protected override void OnInitialize()
    {
        m_image               = this.GetComponentDefault<RawImage>();
        m_image.raycastTarget = m_enableDrag;
        m_valid               = !(string.IsNullOrEmpty(m_cameraName) || string.IsNullOrWhiteSpace(m_cameraName));
        m_dragValid           = !(string.IsNullOrEmpty(m_dragTargetName) || string.IsNullOrWhiteSpace(m_dragTargetName));

        if (!m_mat) m_mat = new Material(Shader.Find("HYLR/UI/RenderTexture"));

        m_image.material = m_mat;

        UpdateExcludeLayers();

        if (enabled)
        {
            ReleaseSnapshot();
            UpdateCamera();
            UpdateDragTarget();
        }

        EventManager.AddEventListener(Events.SCENE_LOAD_START, OnSceneStartLoad);
    }

    private void OnSceneStartLoad()
    {
        m_valid = false;
    }

    protected override void OnWindowShow()
    {
        UpdateCamera();
        UpdateDragTarget();
    }

    protected override void OnWindowHide()
    {
        if (!m_camera || realTime) return;

        CreateSnapshot();
        RemoveInstance();

        m_image.texture = m_snapshot;
        m_image.enabled = true;
    }

    protected override void OnVedioSettingsChanged()
    {
        if (m_camera)
        {
            var target = GetCachedTexture(m_camera);
            if (target == m_image.texture)
            {
                RemoveCachedTexture(m_camera);
                m_camera.targetTexture = GetCachedTexture(m_camera);
                m_image.texture = m_camera.targetTexture;
            }
            else m_image.texture = target;

            UpdateUVRect();
        }
    }

    #endregion

    protected virtual void OnEnable()
    {
        if (!m_initialized) return;

        ReleaseSnapshot();
        UpdateCamera();
        UpdateDragTarget();
    }

    protected virtual void OnDisable()
    {
        if (!m_initialized) return;

        ReleaseSnapshot();
        RemoveInstance();
    }

    protected virtual void OnDestroy()
    {
        RemoveInstance();
        ReleaseSnapshot();

        Destroy(m_mat);

        m_dragTarget = null;
        m_image      = null;
        
        EventManager.RemoveEventListener(this);
    }

    private void UpdateExcludeLayers()
    {
        var layers = Util.ParseString<string>(m_excludeLayers);
        m_excludeMask = LayerMask.GetMask(layers);

        m_excludeMask |= 1 << Layers.SceneObject;

        if (m_camera) m_camera.cullingMask &= ~m_excludeMask;

        Logger.LogDetail("UICharacter {0} exclude layer mask set to {1}, layers: {2}", name, m_excludeMask, m_excludeLayers);
    }

    private void CreateSnapshot()
    {
        if (m_camera && !m_snapshot && m_camera.targetTexture)
        {
            m_snapshot = GetTempRenderTexture((int)UIManager.maxResolution.x, (int)UIManager.maxResolution.y);
            m_snapshot.name += "_" + GetInstanceID() + "_Snapshot";
            Graphics.Blit(m_camera.targetTexture, m_snapshot);
        }
    }

    private void ReleaseSnapshot()
    {
        if (m_snapshot)
        {
            if (m_image.texture == m_snapshot) m_image.texture = null;
            RenderTexture.ReleaseTemporary(m_snapshot);
        }

        m_snapshot = null;
    }

    private void RemoveInstance()
    {
        if (!m_camera) return;

        RemoveInstance(m_camera, GetInstanceID());

//        Logger.LogError(Util.GetHierarchy(gameObject));

        if (realTime)
        {
            s_refs--;
            if (string.IsNullOrEmpty(sceneName))
                Module_Home.instance.DispatchEvent(Module_Home.EventSetTexture, Event_.Pop(0));
            else
                Module_Home.instance.DispatchEvent(Module_Home.EventSetScene, Event_.Pop(sceneName, false));
        }

        if (m_cameraMask != 0 && camera.targetTexture == null && s_refs == 0)
            m_camera.cullingMask = m_cameraMask;
        m_cameraMask = 0;

        if (m_dof != -1 && camera.targetTexture == null)
        {
            var b = m_camera.GetComponent<UnityEngine.PostProcessing.PostProcessingBehaviour>();
            var p = b?.profile ?? null;
            if (p)  p.depthOfField.enabled = m_dof == 1;
        }
        m_dof = -1;

        m_camera = null;

        m_image.texture = null;
        m_image.enabled = false;
    }

    private void UpdateCameraConfig()
    {
        if (!m_setCamera || !m_camera || !m_image.enabled) return;

        m_camera.transform.localPosition = m_cameraPosition;
        m_camera.transform.localEulerAngles = m_cameraRotation;
        m_camera.fieldOfView = m_fov;
    }

    private void UpdateCameraConfig_RealTimeMode()
    {
        if (!m_setCamera || !m_camera) return;

        m_camera.transform.localPosition = m_cameraPosition;
        m_camera.transform.localEulerAngles = m_cameraRotation;
        m_camera.fieldOfView = m_fov;
        m_camera.targetTexture = null;

        if(string.IsNullOrWhiteSpace(sceneName))
            Module_Home.instance.DispatchEvent(Module_Home.EventUpdateMesh, Event_.Pop(0, m_camera));
    }

    private void UpdateCamera()
    {
        RemoveInstance();

        m_isMainCamera = m_cameraName == "main" || m_cameraName == "MainCamera";
        m_image.texture = null;
        m_image.enabled = false;

        if (realTime)
        {
            if(gameObject.activeInHierarchy && enabled)
                CheckCamera_RealTimeMode();
        }
        else
            CheckCamera();
    }

    private void UpdateDragTarget()
    {
        m_dragTarget = null;

        CheckDragTarget();
    }

    [ContextMenu("UpdateUV Rect")]
    private void UpdateUVRect()
    {
        if (realTime) return;

        var tsz = new Vector2(m_camera.targetTexture.width, m_camera.targetTexture.height);
        var isz = tsz;
        var rect = transform as RectTransform;
        if (rect)
        {
            var ss = RectTransformUtility.CalculateRelativeRectTransformBounds(rect).size;
            isz.Set(ss.x, ss.y);
            tsz *= ss.y / tsz.y;
        }

        var relative = new Vector2((m_offset.x + isz.x * m_pivot.x) / tsz.x, (m_offset.y +  isz.y * m_pivot.y) / tsz.y);

        tsz *= scale;

        var off = new Vector2(tsz.x * relative.x - isz.x * m_pivot.x, tsz.y * relative.y - isz.y * m_pivot.y);
        m_image.uvRect = new Rect(off.x / tsz.x, off.y / tsz.y, isz.x / tsz.x, isz.y / tsz.y);
    }

    private void CheckCamera()
    {
        if (!m_valid) return;

        if (!m_camera)
        {
            m_camera = m_isMainCamera ? Camera.main : Util.FindActivedCamera(m_cameraName);
            if (!m_camera) return;
            if (!m_camera.enabled) m_camera.enabled = true;

            m_cameraMask = m_camera.cullingMask;
            m_camera.cullingMask &= ~m_excludeMask;

            m_dof = -1;
            var b = m_camera.GetComponent<UnityEngine.PostProcessing.PostProcessingBehaviour>();
            if (b && b.profile)
            {
                m_dof = b.profile.depthOfField.enabled ? 1 : 0;
                b.profile.depthOfField.enabled = false;
            }

            AddInstance(m_camera, GetInstanceID());

//            Logger.LogError(Util.GetHierarchy(gameObject));

            m_camera.rect = new Rect(0, 0, 1, 1);
            m_camera.targetTexture = GetCachedTexture(m_camera);
        }

        for (var i = 0; i < bg?.Length; i++)
            bg[i].SafeSetActive(true);
        SetRenderTexture();
    }

    private void CheckCamera_RealTimeMode()
    {
        if (!m_valid) return;

        if (!m_camera)
        {
            m_camera = m_isMainCamera ? Camera.main : Util.FindActivedCamera(m_cameraName);
            if (!m_camera) return;
            if (!m_camera.enabled) m_camera.enabled = true;

            m_cameraMask = m_camera.cullingMask;
            m_camera.cullingMask &= ~m_excludeMask;

            m_dof = -1;
            var b = m_camera.GetComponent<UnityEngine.PostProcessing.PostProcessingBehaviour>();
            if (b && b.profile)
            {
                m_dof = b.profile.depthOfField.enabled ? 1 : 0;
                b.profile.depthOfField.enabled = false;
            }

            m_camera.rect = new Rect(0, 0, 1, 1);

            s_refs++;
            if (string.IsNullOrEmpty(sceneName))
                Module_Home.instance.DispatchEvent(Module_Home.EventSetTexture, Event_.Pop(0, bg));
            else
                Module_Home.instance.DispatchEvent(Module_Home.EventSetScene, Event_.Pop(sceneName, true));

            for (var i = 0; i < bg?.Length; i++)
                bg[i].SafeSetActive(false);
        }

        SetRenderTexture_RealTimeMode();
    }

    private void CheckDragTarget()
    {
        if (!m_dragValid || !Level.current || !Level.current.startPos) return;

        var root = Level.current.startPos;
        if (!m_dragTarget)
        {
            if (m_dragTargetName.Equals(root.name))
                m_dragTarget = root;
            else
                m_dragTarget = root.Find(m_dragTargetName);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!m_dragTarget|| !m_enableDrag) return;

        var r = m_dragTarget.localEulerAngles;
        r.y += -eventData.delta.x * dragSpeed;

        m_dragTarget.localEulerAngles = r;
    }

    private void LateUpdate()
    {
        if (!m_initialized) return;

        if (m_valid && (!m_image.enabled && !realTime)) CheckCamera();

        if (m_dragValid && !m_dragTarget) CheckDragTarget();

        if (m_valid && realTime && !m_camera) CheckCamera_RealTimeMode();
    }

    private void SetRenderTexture()
    {
        var e = m_image.enabled;
        m_image.texture = m_camera.targetTexture;
        m_image.enabled = true;
        var c = m_image.color;
        c.a = 1;
        m_image.color = c;

        UpdateUVRect();
        UpdateCameraConfig();

        if (!e && tweenAnimation)
        {
            m_image.color = Util.BuildColor(m_image.color, 0);
            m_image.DOFade(1.0f, tweenDuration);
        }
    }

    private void SetRenderTexture_RealTimeMode()
    {
        m_image.texture = m_camera.targetTexture;
        m_image.enabled = true;
        var c = m_image.color;
        c.a = 0;
        m_image.color = c;

        UpdateCameraConfig_RealTimeMode();
    }

    [ContextMenu("List References")]
    public void ListReferences()
    {
        foreach (var pair in m_cachedRTs)
            Logger.LogDetail("RT:{0} Ref Count: {1}", pair.Value?.name, m_instanceCount.Get(pair.Key)?.Count);

        if (m_snapshot) Logger.LogDetail("RT:{0} Ref Count: {1}", m_snapshot.name, 1);
    }
}
