/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Camera script for combat.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-08
 * 
 ***************************************************************************************************/

using UnityEngine;
using ShotState = CameraShotInfo.ShotState;

[AddComponentMenu("HYLR/Cameras/CombatCamera")]
[RequireComponent(typeof(AudioListener))]
public class Camera_Combat : MonoBehaviour
{
    #region Static functions

    public static Camera_Combat current { get { return m_current; } set { m_current = value; } }
    public static Camera currentCamera { get { return m_current ? m_current.m_camera : null; } }

    /// <summary>
    /// Enable or disable Camera Shot System
    /// </summary>
    public static bool enableShotSystem
    {
        get { return m_current && m_current._enableShotSystem; }
        set { if (m_current) m_current._enableShotSystem = value; }
    }

    public static void Shake(float intensity, float duration, float range = -1, Creature source = null)
    {
        if (!m_current) return;
        var c = m_current.m_creature;
        if (range < 0 || range == 0 && c && c == source) m_current._Shake(intensity, duration);
        else if (range > 0 && source) Shake(intensity, duration, range, source.position);
    }

    public static void Shake(float intensity, float duration, float range, Vector3 source, Creature caster)
    {
        if (!m_current) return;
        var c = m_current.m_creature;
        if (range < 0 || range == 0 && c && c == caster) m_current._Shake(intensity, duration);
        else Shake(intensity, duration, range, source);
    }

    public static void Shake(float intensity, float duration, float range, Vector3 source)
    {
        if (!m_current) return;

        var p = m_current.m_creature ? m_current.m_creature.position : m_current.transform.position;

        p.z      = 0;  // Always ignore z position
        source.z = 0;

        if (Vector3.Distance(p, source) > range) return;

        m_current._Shake(intensity, duration);
    }

    public static void Dark(int drawCount, float alpha, bool fade = true)
    {
        if (!m_current) return;
        m_current._Dark(drawCount, alpha, fade);
    }

    public static void EnableDisableAudioListener(bool enable)
    {
        if (!m_current) return;
        m_current._EnableDisableAudioListener(enable);
    }

    public static void SetMaskColor(double r, double g, double b, double a)
    {
        if (!m_current) return;
        m_current.maskColor = new Color((float)r, (float)g, (float)b, (float)a);
    }

    public static void ResetMaskColor()
    {
        if (!m_current) return;
        m_current.maskColor = m_current.m_maskColorCache;
    }

    public static void ForceMaskRepaint(Camera camera, int layer = 0)
    {
        if (!camera || !m_maskMaterial || !m_maskMesh || !m_current || !m_current.m_fading && m_current.m_drawMask < 1) return;
        var t = camera.transform;
        var c = currentCamera;
        if (c && camera.fieldOfView != c.fieldOfView)
        {
            SetMaskVertices(camera, true);
            Graphics.DrawMesh(m_maskMeshCopy, t.position + t.forward * (camera.nearClipPlane + 0.001f), t.rotation, m_maskMaterial, layer, camera);
        }
        else Graphics.DrawMesh(m_maskMesh, t.position + t.forward * (camera.nearClipPlane + 0.001f), t.rotation, m_maskMaterial, layer, camera);
    }

    private static void SetMaskVertices(Camera camera, bool copy)
    {
        if (!camera) return;

        var t = camera.transform;
        var a = t.InverseTransformPoint(camera.ViewportToWorldPoint(new Vector3(1, 0, camera.nearClipPlane + 0.005f)));
        var b = t.InverseTransformPoint(camera.ViewportToWorldPoint(new Vector3(0, 1, camera.nearClipPlane + 0.005f)));
        var c = t.InverseTransformPoint(camera.ViewportToWorldPoint(new Vector3(0, 0, camera.nearClipPlane + 0.005f)));
        var d = t.InverseTransformPoint(camera.ViewportToWorldPoint(new Vector3(1, 1, camera.nearClipPlane + 0.005f)));

        a.z = b.z = c.z = d.z = 0;

        if (!m_maskMesh)
        {
            m_maskMesh = new Mesh();
            m_maskMesh.hideFlags = HideFlags.HideAndDontSave;
            m_maskMesh.vertices = new Vector3[] { a, b, c, d };
            m_maskMesh.SetIndices(new int[] { 0, 2, 1, 1, 3, 0 }, MeshTopology.Triangles, 0);
        }
        if (copy && !m_maskMeshCopy) m_maskMeshCopy = Instantiate(m_maskMesh);

        var mesh = copy ? m_maskMeshCopy : m_maskMesh;
        mesh.vertices = new Vector3[] { a, b, c, d };
    }

    private static Camera_Combat m_current;

    private static int m_colorID = Shader.PropertyToID("_Color");
    private static int m_alphaID = Shader.PropertyToID("_Alpha");

    private static Mesh m_maskMesh = null, m_maskMeshCopy = null;
    private static Material m_maskMaterial = null;

    #endregion

    #region Properties

    public Vector3 euler { get { return m_euler; } set { if (m_euler == value) return; m_euler = value; } }
    public Color maskColor
    {
        get { return m_maskColor; }
        set
        {
            if (m_maskColor == value) return;
            m_maskColor = value;

            if (m_maskMaterial) m_maskMaterial.SetColor(m_colorID, m_maskColor);
        }
    }
    public bool moving { get { return m_moving; } }
    public float fieldOfView { get { return m_camera.fieldOfView; } }
    public ShotState currentShot { get { return m_currentShot; } }

    public Vector3 initOffset { get { return m_initOffset; } }
    public Vector3 initEuler { get { return m_initEuler; } }
    public float initFOV { get { return m_initFOV; } }

    public bool updatePosition = true;
    public bool updateRotation = true;

    [Header("Offset")]
    public Vector3 offset = Vector3.zero;

    [SerializeField, Set("euler")]
    private Vector3 m_euler = Vector3.zero;
    [Space(20)]
    public Transform follow;
    
    [Header("Shake"), SerializeField]
    private float m_shakeAmount = 0f;
    [SerializeField]
    private float m_shakeIntensity = 0f;
    [SerializeField]
    private float m_shakeDuration = 0f;
    [SerializeField]
    private Vector3 m_shakeDirection;

    private float m_shakeStep = -0.5f;

    [Header("Smooth"), SerializeField]
    private bool m_falling = false;
    [SerializeField]
    private float m_lastHeight = 0;
    [SerializeField]
    private float m_fallTime = 0;
    [SerializeField]
    private float m_currentSmooth = 0;
    [SerializeField]
    private float m_overrideSmooth = -1;

    [Header("Mask"), SerializeField, Set("maskColor")]
    private Color m_maskColor = new Color(0, 0, 0, 0.6f);

    private Color m_maskColorCache;
    [SerializeField]
    private int m_drawMask = 0;

    [Header("Fading"), SerializeField]
    private bool m_fading = false;
    [SerializeField]
    private float m_fadeStart = 0;
    [SerializeField]
    private float m_fadeEnd = 0;
    [SerializeField]
    private float m_fadeTime = 0;
    [SerializeField]
    private float m_currentFade = 0;
    [SerializeField]
    private float m_currentFadeDuration = 0;
    [SerializeField]
    private float m_overrideFadeDuration = -1;

    [Space(10)]
    [SerializeField]
    private Vector2 m_edge = new Vector2(1, 0);
    public bool removeCameraEdge = false;
    public bool lockZ = false;
    [SerializeField]
    private bool m_moving = false;

    private Vector3 m_levelEdge;
    private float m_startZ;

    private Camera m_camera;
    private Creature m_creature = null;

    private float m_z;
    private float m_fov;
    private bool m_fovChanged = false;   // FOV changed current frame ?
    private bool m_forceRecalculateEdge = false;
    private AudioListener m_audioListener = null;

    private Transform m_t;

    private Vector3 m_initOffset, m_initEuler;
    private float m_initFOV;
    private float m_diff;
    private bool m_shotValid;
    private bool m_initialized;

    public bool canDestroyCurrent { get; set; } = true;

    #endregion

    #region Public interface

    public void SetFollowTransform(Transform trans)
    {
        follow = trans;
        OverrideSmooth(trans == null ? -1f : 0.3f);
        UpdateMoveState(true);
    }

    public void _EnableDisableAudioListener(bool enable)
    {
        m_audioListener.enabled = enable;
    }

    /// <summary>
    /// Make a fullscreen dark mask to all scene ovjects except creatures and effects
    /// </summary>
    public void _Dark(int drawCount, float alpha, bool fade = true)
    {
        m_drawMask += drawCount;

        if (!m_camera) return;

        m_overrideFadeDuration = -1;

        if (!m_maskMaterial) m_maskMaterial = new Material(Shader.Find("HYLR/Effect/CameraMask"));
        m_maskMaterial.SetColor(m_colorID, m_maskColor);

        if (m_drawMask < 1)
        {
            if (!fade) m_fading = false;
            else
            {
                if (!m_fading)
                {
                    m_fading = true;
                    m_fadeStart = m_maskMaterial.GetFloat(m_alphaID);
                    m_fadeEnd = 0;
                    m_fadeTime = 0;
                }
                else if (m_fadeStart < m_fadeEnd)
                {
                    var fadeStart = m_fadeStart;
                    m_fadeStart = m_fadeEnd;
                    m_fadeEnd = fadeStart;
                }
            }
            return;
        }

        if (drawCount < 0) return;

        if (!fade)
        {
            m_fading = false;
            m_maskMaterial.SetFloat(m_alphaID, alpha);
        }
        else
        {
            if (!m_fading)
            {
                m_fading = true;
                m_fadeStart = m_maskMaterial.GetFloat(m_alphaID);
                m_fadeEnd = alpha;
                m_fadeTime = 0;
            }
            else if (m_fadeStart > m_fadeEnd)
            {
                var fadeEnd = m_fadeEnd;
                m_fadeEnd = m_fadeStart;
                m_fadeStart = fadeEnd;
            }
        }

        SetMaskVertices(m_camera, false);
    }

    public void _Shake(float intensity, float duration)
    {
        if (duration <= 0) return;

        m_shakeIntensity = intensity;
        m_shakeAmount    = duration;
        m_shakeDuration  = 1.0f / m_shakeAmount;
        m_shakeStep      = 0.5f;

        m_shakeDirection = m_creature ? m_creature.transform.TransformDirection(0, 0.4f, 0.35f) : m_t.TransformDirection(0.35f, 0.4f, 0);
        m_shakeDirection = m_shakeStep * m_shakeIntensity * m_shakeDirection.normalized;
    }

    public void LookTo(Vector3 pos)
    {
        m_t.position = pos + offset;
    }

    public void OverrideSmooth(float smooth)
    {
        m_overrideSmooth = smooth;
    }

    #endregion

    #region Initialize

    private void Awake()
    {
        m_t       = transform;
        m_current = this;
        m_camera  = GetComponent<Camera>();
        m_moving  = false;

        m_audioListener = this.GetComponentDefault<AudioListener>();
        m_audioListener.enabled = true;

        m_z = m_t.position.z;
        m_fov = m_camera.fieldOfView;
        m_camera.cullingMask |= Layers.MASK_DARK_MASK;

#if UNITY_EDITOR
        if (!Game.started)
        {
            offset = m_t.position;
            euler = m_t.eulerAngles;
        }
        else
#endif
        m_creature = Module_Battle.instance.current;

        if (m_creature) return;

        m_creature = null;

        EventManager.AddEventListener(CreatureEvents.PLAYER_ADD_TO_SCENE, OnPlayerAddToScene);
    }

    private void Start()
    {
        m_maskColorCache = m_maskColor;
        if (!m_creature) return;
        Initialize();
        BindToPlayer();
    }

    private void OnDestroy()
    {
        m_creature = null;
        m_fading = false;
        follow = null;
        lockZ = false;

        if (canDestroyCurrent) m_current = null;

        EventManager.RemoveEventListener(this);
    }

    private void Initialize()
    {
        if (m_initialized) return;
        m_initialized = true;

        offset   = m_t.position;
        euler    = m_t.eulerAngles;

        m_initOffset = offset;
        m_initEuler  = euler;
        m_initFOV    = m_camera.fieldOfView;

        m_initStartState.Set(m_initOffset, m_initEuler, m_initFOV, 0);
        m_initEndState = m_initStartState;
        m_initEndState.time = 1.0f;
        m_initEndState.SetBlendType(CameraBlendType.Linear, null);
    }

    private void BindToPlayer()
    {
        if (!m_creature) return;

        var np = m_creature.position + offset;

        RecalculateEdge(ref np);

        m_t.position = np;
        m_t.eulerAngles = m_euler;

        m_startZ = m_t.position.z;

        m_creature.AddEventListener(CreatureEvents.ENTER_STATE, OnPlayerEnterState);
    }

    private void OnPlayerAddToScene(Event_ e)
    {
        var c = e.sender as Creature;
        if (!c) return;

        if (m_creature) m_creature.RemoveEventListener(this);

        m_creature = c;

        Initialize();
        BindToPlayer();

        OnPlayerEnterState();
    }

    #endregion

    #region Logic update

    public void UpdatePosition(bool useSmooth = true)
    {
        if (!updatePosition) return;

        var tp = Vector3.zero;
        var np = m_t.position;

        if           (follow)     tp = follow.position;
        else if      (m_creature) tp = m_creature.position;
        else return;

        m_currentSmooth = m_overrideSmooth;

        if (m_currentSmooth < 0)
        {
            m_currentSmooth = CombatConfig.sgroundSmooth;

            var v = tp.y - m_lastHeight;
            m_lastHeight = tp.y;

            if (v < -0.01f)
            {
                m_falling = true;
                m_fallTime += m_diff;

                m_currentSmooth = CombatConfig.sfallSmooth * (1 + (v < -1 ? -1 : v));
            }
            else
            {
                m_falling = false;
                m_fallTime = 0;
            }

            if (tp.y < CombatConfig.sjumpFollowHeight) tp.y = 0;
            else
            {
                tp.y = tp.y - CombatConfig.sjumpFollowHeight + CombatConfig.sfixedFollowHeight;
                if (!m_falling) m_currentSmooth = CombatConfig.sjumpSmooth * (1 - (v > 1 ? 1 : v));
            }
        }

        tp += offset;

        if (lockZ) tp.z = m_startZ;

        RecalculateEdge(ref tp);

        np = !useSmooth || m_currentSmooth == 0 ? tp : Vector3.Lerp(np, tp, m_diff * 0.85f / m_currentSmooth);

        if (m_shakeAmount > 0)
        {
            m_shakeAmount -= Time.deltaTime;
            if (m_shakeAmount < 0) m_shakeAmount = 0;
            var rd = m_shakeAmount * m_shakeDuration * m_shakeStep * m_shakeDirection;
            m_shakeStep *= -0.5f;

            np += rd;
        }

        UpdateMoveState(Mathf.Abs(np.x - m_t.position.x) > 0.01f);
        m_t.position = np;
    }

    public void UpdateRotation(bool useSmooth = true)
    {
        if (!updateRotation) return;

        var angles = m_t.eulerAngles.DeltaAnglesTo(m_euler);
        if (angles.magnitude > ExtendedMethods.Epsilon)
        {
            angles = !useSmooth || m_currentSmooth == 0 ? m_euler : Util.Lerp(m_t.eulerAngles, m_euler, m_diff * 0.85f / m_currentSmooth);
            m_t.eulerAngles = angles;
        }
    }

    private void UpdateMoveState(bool _moving)
    {
        if (m_moving == _moving) return;
        m_moving = _moving;

        Root.instance?.DispatchEvent(Events.COMBAT_CAMERA_MOVE_CHANGE, Event_.Pop(m_moving));
    }

    private void LateUpdate()
    {
        m_diff = Time.smoothDeltaTime;
        #if UNITY_EDITOR
        if (Root.logicPaused) m_diff = 0;
        #endif

        m_fovChanged = m_fov != m_camera.fieldOfView;
        m_fov = m_camera.fieldOfView;
        m_shotValid = m_enableShotSystem && m_creature && m_shotInfo.valid;

        UpdateCameraShot();
        UpdateShotBlend();
        UpdatePosition();
        UpdateRotation();

        DrawMask();
    }

    private void DrawMask()
    {
        if (m_fading || m_drawMask > 0)
        {
            if (m_fading)
            {
                m_currentFadeDuration = m_overrideFadeDuration;
                if (m_currentFadeDuration < 0) m_currentFadeDuration = CombatConfig.sdarkFadeDuration;

                m_fadeTime += m_diff;
                if (m_fadeTime < m_currentFadeDuration) m_currentFade = Mathf.SmoothStep(m_fadeStart, m_fadeEnd, m_fadeTime / m_currentFadeDuration);
                else
                {
                    m_fading = false;
                    m_currentFade = m_fadeEnd;
                }
                if (m_maskMaterial) m_maskMaterial.SetFloat(m_alphaID, m_currentFade);
            }

            if (m_fovChanged) SetMaskVertices(m_camera, false);
            if (m_maskMaterial) Graphics.DrawMesh(m_maskMesh, m_t.position + m_t.forward * (m_camera.nearClipPlane + 0.001f), m_t.rotation, m_maskMaterial, Layers.DarkMask);
        }
    }

    private void RecalculateEdge(ref Vector3 np)
    {
#if UNITY_EDITOR
        if (!Level.current) return;
#endif
        if (removeCameraEdge) return;

        var edge = (Vector3)Level.current.edge;
        if (m_levelEdge != edge || m_z - np.z > 0.01f || m_fovChanged || m_forceRecalculateEdge)
        {
            m_forceRecalculateEdge = false;

            m_z = np.z;
            m_levelEdge = edge;

            var e = m_levelEdge;
            if (edge.x > edge.y) m_edge.Set(e.x, e.y);
            else
            {
                var proj = Mathf.Abs(Mathf.Tan(m_fov * 0.5f * Mathf.Deg2Rad) * m_z) * 2.0f;

                m_edge.x = e.x + proj - e.z;
                m_edge.y = e.y - proj + e.z;
            }
        }

        if (m_edge.x <= m_edge.y) np.x = Mathf.Clamp(np.x, m_edge.x, m_edge.y);
    }

    #endregion

    #region Camera shot system

    /// <summary>
    /// Enable or disable Camera Shot System
    /// </summary>
    [Header("Camera Shot System")]
    [SerializeField, Set("_enableShotSystem")]
    private bool m_enableShotSystem = true;

    public bool _enableShotSystem
    {
        get { return m_enableShotSystem; }
        set
        {
            if (m_enableShotSystem == value) return;
            m_enableShotSystem = value;

            if (!m_enableShotSystem)
            {
                offset = m_initOffset;
                euler = m_initEuler;

                m_overrideSmooth = -1;
                removeCameraEdge = false;
                m_forceRecalculateEdge = true;

                if (m_initFOV > 0) m_camera.fieldOfView = m_initFOV;

                Level.ShowHideSceneObjects(true);

                UpdatePosition(false);
                UpdateRotation(false);
            }
            else UpdateCurrentStateShot(true);

            Root.instance.DispatchEvent(Events.CAMERA_SHOT_STATE, Event_.Pop(m_enableShotSystem));
        }
    }

    private float m_stateTime = -1;
    private int m_shotIndex = 0;
    private CameraShotInfo m_shotInfo = CameraShotInfo.empty;
    private ShotState m_initStartState = ShotState.empty, m_initEndState = ShotState.empty, m_currentShot = ShotState.empty, m_prevShot = ShotState.empty, m_nextShot = ShotState.empty;

    private void OnPlayerEnterState()
    {
        UpdateCurrentStateShot();
    }

    private void UpdateCameraShot()
    {
        if (!m_shotValid) return;
        
        var s = m_creature.currentState;
        if (m_stateTime == s.normalizedTime) return;
        m_stateTime = s.normalizedTime;

        SwitchShot();
    }

    private void UpdateShotBlend()
    {
        if (!m_shotValid) return;

        ShotState.Lerp(ref m_currentShot, ref m_prevShot, ref m_nextShot, m_stateTime, m_currentSmooth);

        offset = m_currentShot.offset;
        euler  = m_currentShot.euler;

        m_overrideSmooth = m_currentShot.overrideSmooth;

        m_camera.fieldOfView = m_currentShot.fieldOfView;
    }

    private void UpdateCurrentStateShot(bool forceCut = false)
    {
        if (!m_enableShotSystem) return;

        var s = m_creature.currentState;
        if (!s)
            return;
        m_stateTime = s.normalizedTime;
        m_shotIndex = 0;

        m_currentShot.SetBlendType(m_nextShot.blend.blendType, null);

        ShotState shotState;
        if (!CameraShotInfo.GetShotState(m_creature.weaponID, s.ID, m_stateTime, m_shotIndex, out m_shotInfo, out shotState))
        {
            SwitchShot(ref m_initStartState, false, forceCut);
            return;
        }
        else
        {
            ++m_shotIndex;
            SwitchShot(ref shotState, false, forceCut);
        }

        SwitchShot(forceCut);
    }

    private void SwitchShot(bool forceCut = false)
    {
        ShotState shotState;
        while (m_shotInfo.GetShotState(m_stateTime, m_shotIndex, out shotState))
        {
            ++m_shotIndex;
            SwitchShot(ref shotState, true, forceCut);
        }
    }

    private void SwitchShot(ref ShotState shotState, bool checkCut = true, bool forceCut = false)
    {
        var cut = forceCut || shotState.forceCut || checkCut && m_prevShot.blend.blendType == CameraBlendType.Cut;

        m_prevShot = shotState;

        if (m_shotInfo) m_nextShot = m_shotInfo.GetSortedShotState(m_shotIndex);
        if (m_nextShot.isEmpty) m_nextShot = m_prevShot.time < 1.0f ? m_initEndState : m_prevShot;

        if (!m_creature.isForward)
        {
            m_prevShot.Inverse();
            m_nextShot.Inverse();
        }

        euler  = m_prevShot.euler;
        offset = m_prevShot.offset;

        m_overrideSmooth = m_prevShot.overrideSmooth;

        m_camera.fieldOfView = m_prevShot.fieldOfView;

        m_currentShot.Set(offset, euler, m_prevShot.blend.blendType);

        removeCameraEdge = m_prevShot.removeCameraEdge;
        if (!removeCameraEdge) m_forceRecalculateEdge = true;

        if (Level.ShowHideSceneObjects(!m_prevShot.hideScene) && !m_prevShot.hideScene && m_drawMask < 1)
        {
            m_fading    = true;
            m_fadeStart = 1.0f;
            m_fadeEnd   = 0;
            m_fadeTime  = 0;

            m_overrideFadeDuration = CombatConfig.smaskFadeDuration;

            if (m_maskMaterial) m_maskMaterial.SetColor(m_colorID, Color.black);
        }

        if (cut)
        {
            UpdatePosition(false);
            UpdateRotation(false);
        }

        Root.instance.DispatchEvent(Events.CAMERA_SHOT_UI_STATE, Event_.Pop(m_prevShot.hideCombatUI, m_prevShot.maskAsset, m_prevShot.maskDuration));
    }

    #endregion

    #region Editor helper

#if UNITY_EDITOR
    public void UpdateIfCurrentShot(int uid)
    {
        if (!m_shotInfo || m_shotInfo.uid != uid) return;
        var state = m_shotInfo.GetSortedShotState(m_shotIndex - 1);
        if (state.isEmpty) return;

        SwitchShot(ref state);
        UpdateShotBlend();

        UpdatePosition(false);
        UpdateRotation(false);
    }

    public void UpdateShotState()
    {
        var s = m_creature.currentState;
        if (!s) return;

        ShotState shotState;
        if (!CameraShotInfo.GetShotState(m_creature.weaponID, s.ID, m_stateTime, out m_shotInfo, out shotState))
        {
            offset = m_initOffset;
            euler  = m_initEuler;
            return;
        }

        SwitchShot();
    }

    private void OnDrawGizmos()
    {
        if (m_edge.x <= m_edge.y)
        {
            var el = new Vector3(m_edge.x, 0, 0);
            var er = new Vector3(m_edge.y, 0, 0);

            Gizmos.color = Color.magenta;

            Gizmos.DrawLine(el + Vector3.up * 3, el + Vector3.down);
            Gizmos.DrawLine(er + Vector3.up * 3, er + Vector3.down);
            Gizmos.DrawLine(el, er);
        }
    }
#endif

    #endregion
}