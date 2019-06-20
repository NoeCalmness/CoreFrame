/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * UI manager.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-05
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("HYLR/Utilities/UI Manager")]
public class UIManager : SingletonBehaviour<UIManager>
{
    #region Static functions

    public static bool visible { get; private set; }

    /// <summary>
    /// Get/Set current window UI input state
    /// </summary>
    public static bool inputState
    {
        get { return !instance || instance.eventSystem.enabled; }
        set
        {
            var module = instance?.eventSystem?.currentInputModule;
            if (module) module.disableDownEvent = !value;
        }
    }

    public static Camera worldCamera { get { return instance.m_canvas.worldCamera; } }
    public static Camera fixCamera { get { return instance._fixCamera; } }
    public static Canvas canvas { get {return instance.m_canvas;} }
    public static CanvasScaler canvasScaler { get { return instance.m_canvasScaler; } }
    public static Vector2 realViewResolution { get { return instance.m_realViewResolution; } }
    public static Vector2 viewResolution { get { return instance.m_viewResolution; } }
    public static Vector2 referenceResolution { get { return instance.m_referenceResolution; } }
    public static Vector2 maxResolution { get { return instance.m_maxResolution; } }
    public static float aspect { get { return instance.m_aspect; } }
    public static float refAspect { get { return instance.m_refAspect; } }
    public static float heightFix { get { return instance.m_heightFix; } }

    public static void Hide(bool immediately = false)
    {
        if (!instance) return;
        instance._Hide(immediately);
    }

    public static void Show(bool immediately = false)
    {
        if (!instance) return;
        instance._Show(immediately);
    }

    public static void SetCamera(Camera camera = null)
    {
        if (!instance) return;
        instance._SetCamera(camera);
    }

    public static void FixCameraRect(Camera camera)
    {
        instance?._FixCameraRect(camera);
    }

    /// <summary>
    /// Set fix camera state 0 = disable  1 = set layer to 1 (over scene cameras) -1 = default (before any camera)
    /// </summary>
    /// <param name="state"></param>
    public static void SetFixCameraState(int state)
    {
        var c = instance?._fixCamera;
        if (!c) return;

        c.depth = state;
        if (state == 0) c.enabled = false;
    }

    public static void SetCameraLayer(params int[] layers)
    {
        if (!worldCamera) return;

        worldCamera.cullingMask &= 0;
        foreach (var item in layers)
        {
            if (item < 0 || item > 31) continue;

            worldCamera.cullingMask |= 1 << item;
        }
        Logger.LogInfo("Set UI camera culling mask to <b>[{0}]</b>", layers.PrettyPrint());
    }

    #endregion

    public Canvas _canvas { get { return m_canvas; } }
    public CanvasScaler _canvasScaler { get { return m_canvasScaler; } }

    public float        fadeDuration          = 0.2f;
    public Camera       defaultCamera         = null;
    public Camera       _fixCamera            = null;
    public GameObject   window_defaultLoading = null;
    public GameObject   window_global         = null;
    public EventSystem  eventSystem           = null;

    private Canvas       m_canvas;
    private CanvasScaler m_canvasScaler;
    private CanvasGroup  m_canvasGroup;
    private TweenAlpha   m_tween;

    [Space(10)]
    [SerializeField] private Vector2 m_realViewResolution;
    [SerializeField] private Vector2 m_viewResolution;
    [SerializeField] private Vector2 m_referenceResolution;
    [SerializeField] private Vector2 m_maxResolution;
    [SerializeField] private float m_aspect;
    [SerializeField] private float m_refAspect;
    [SerializeField] private float m_heightFix;

    [Space(10)]
    [SerializeField] private string m_lastVoice;
    [SerializeField] private string m_lastMusic;

    private Camera _worldCamera { get { return m_canvas.worldCamera; } }

    protected override void Awake()
    {
        ExecuteEvents.onPreExecute += OnPreExecute;

        base.Awake();
        
        m_canvas       = this.GetComponentDefault<Canvas>();
        m_canvasScaler = this.GetComponentDefault<CanvasScaler>();
        m_canvasGroup  = this.GetComponentDefault<CanvasGroup>();

        m_canvasGroup.alpha = 0.0f;

        CreateTween();

        Logger.LogInfo("Detecting Device Resolution...");

        m_referenceResolution = m_canvasScaler.referenceResolution;
        m_refAspect = m_referenceResolution.x / m_referenceResolution.y;

        Logger.LogDetail("Designed Resolution: <color=#00DDFF>Size: <color=#00FF00><b>[{0},{1}]</b></color>, Aspect: <color=#00FF00><b>[{2}]</b></color></color>", (int)m_referenceResolution.x, (int)m_referenceResolution.y, m_refAspect);

        OnResolutionChanged();

        _Show();

        EventManager.AddEventListener(Events.RESOLUTION_CHANGED, OnResolutionChanged);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        ExecuteEvents.onPreExecute -= OnPreExecute;
    }

    public void _SetCamera(Camera camera = null)
    {
        if (!camera) camera = defaultCamera;

        if (m_canvas.worldCamera == camera) return;

        m_canvas.worldCamera = camera;

        defaultCamera.enabled = m_canvas.worldCamera == defaultCamera;

        OnResolutionChanged();

        DispatchEvent(Events.UI_CAMERA_CHANGED);
    }

    public void _FixCameraRect(Camera camera)
    {
        if (!camera || camera.targetTexture) return;    // Ignore render textures, they are rendered in other place

        camera.rect = m_aspect < m_refAspect ? new Rect(0, (1 - m_heightFix) * 0.5f, 1, m_heightFix) : new Rect(0, 0, 1, 1);
    }

    public void _Hide(bool immediately = false)
    {
        visible = false;

        m_tween.enabled = !immediately;
        if (immediately) OnFadeComplete(false);
        else m_tween.PlayReverse();
    }

    public void _Show(bool immediately = false)
    {
        visible = true;

        m_tween.enabled = !immediately;
        if (immediately) OnFadeComplete(true);
        else m_tween.PlayForward();
    }

    private void OnFadeComplete(bool show)
    {
        m_canvasGroup.alpha = show ? 1.0f : 0.0f;
        m_tween.enabled = false;

        DispatchEvent(show ? Events.UI_FADE_IN : Events.UI_FADE_OUT);
    }

    private void OnResolutionChanged()
    {
        var c = m_canvas.worldCamera;
        if (!c) return;

        c.rect = new Rect(0, 0, 1, 1);

        m_realViewResolution = new Vector2(c.pixelWidth, c.pixelHeight);
        m_aspect = m_realViewResolution.x / m_realViewResolution.y;
        m_heightFix = 0;

        if (m_aspect < m_refAspect)
        {
            m_canvasScaler.matchWidthOrHeight = 0;
            m_heightFix = m_realViewResolution.x / m_refAspect / m_realViewResolution.y;
            c.rect = new Rect(0, (1 - m_heightFix) * 0.5f, 1, m_heightFix);

            m_viewResolution = new Vector2(c.pixelWidth, c.pixelHeight);
            m_referenceResolution.x = m_referenceResolution.y * m_viewResolution.x / m_viewResolution.y;
        }
        else
        {
            m_canvasScaler.matchWidthOrHeight = 1;
            c.rect = new Rect(0, 0, 1, 1);

            m_viewResolution = new Vector2(c.pixelWidth, c.pixelHeight);
            m_referenceResolution.x = m_referenceResolution.y * m_viewResolution.x / m_viewResolution.y;
        }

        m_maxResolution = new Vector2(Mathf.CeilToInt(m_viewResolution.y * GeneralConfigInfo.smaxAspectRatio), m_viewResolution.y);

        Logger.LogDetail("Resolution Changed: <color=#00DDFF>View: <color=#00FF00><b>[{0},{1}]</b></color>, Fixed View: <color=#00FF00><b>[{2},{3}]</b></color>, Ref: <color=#00FF00><b>[{4},{5}]</b></color>, Max: <color=#00FF00><b>[{7},{8}]</b></color>, Aspect: <color=#00FF00><b>[{6:F2}]</b></color></color>",
            (int)m_realViewResolution.x, (int)m_realViewResolution.y, (int)m_viewResolution.x, (int)m_viewResolution.y, (int)m_referenceResolution.x, (int)m_referenceResolution.y, m_aspect, (int)m_maxResolution.x, (int)m_maxResolution.y);

        DispatchEvent(Events.UI_CANVAS_FIT);
    }

    private void CreateTween()
    {
        m_tween = GetComponent<TweenAlpha>();
        if (!m_tween)
        {
            m_tween = this.GetComponentDefault<TweenAlpha>();
            m_tween.ease = DG.Tweening.Ease.Linear;
            m_tween.duration = fadeDuration;
        }

        m_tween.enabled         = false;
        m_tween.autoStart       = false;
        m_tween.delayStart      = 0;
        m_tween.loop            = false;
        m_tween.startVisible    = true;
        m_tween.currentAsFrom   = true;
        m_tween.ignoreTimeScale = true;
        m_tween.from            = 0;
        m_tween.to              = 1;

        m_tween.onComplete.AddListener(OnFadeComplete);
    }

    #region Pre UI event wrapper

    public delegate void OnUIEvent(GameObject target);
    public delegate void OnGlobalUIEvent(GameObject target, EventInterfaceTypes eventType);

    public static OnUIEvent onPointerEnter;
    public static OnUIEvent onPointerExit;
    public static OnUIEvent onPointerDown;
    public static OnUIEvent onPointerUp;
    public static OnUIEvent onPointerClick;
    public static OnUIEvent onInitializePotentialDrag;
    public static OnUIEvent onBeginDrag;
    public static OnUIEvent onDrag;
    public static OnUIEvent onEndDrag;
    public static OnUIEvent onDrop;
    public static OnUIEvent onScroll;
    public static OnUIEvent onUpdateSelected;
    public static OnUIEvent onSelect;
    public static OnUIEvent onDeselect;
    public static OnUIEvent onMove;
    public static OnUIEvent onSubmit;
    public static OnUIEvent onCancel;

    public static OnGlobalUIEvent onGlobalUIEvent;

    private static void OnPreExecute(GameObject target, EventInterfaceTypes eventType)
    {
        onGlobalUIEvent?.Invoke(target, eventType);

        var tag = target.tag;
        var s = string.IsNullOrEmpty(tag) ? UISoundEffect.empty : UISoundEffect.GetSound(tag, target.name);
        string[] ss = null;

        switch (eventType)
        {
            case EventInterfaceTypes.PointerEnter:              ss = s.onPointerEnter;             onPointerEnter?.Invoke(target);            break;
            case EventInterfaceTypes.PointerExit:               ss = s.onPointerExit;              onPointerExit?.Invoke(target);             break;
            case EventInterfaceTypes.PointerDown:               ss = s.onPointerDown;              onPointerDown?.Invoke(target);             break;
            case EventInterfaceTypes.PointerUp:                 ss = s.onPointerUp;                onPointerUp?.Invoke(target);               break;
            case EventInterfaceTypes.PointerClick:              ss = s.onPointerClick;             onPointerClick?.Invoke(target);            break;
            case EventInterfaceTypes.BeginDrag:                 ss = s.onBeginDrag;                onBeginDrag?.Invoke(target);               break;
            case EventInterfaceTypes.InitializePotentialDrag:   ss = s.onInitializePotentialDrag;  onInitializePotentialDrag?.Invoke(target); break;
            case EventInterfaceTypes.Drag:                      ss = s.onDrag;                     onDrag?.Invoke(target);                    break;
            case EventInterfaceTypes.EndDrag:                   ss = s.onEndDrag;                  onEndDrag?.Invoke(target);                 break;
            case EventInterfaceTypes.Drop:                      ss = s.onDrop;                     onDrop?.Invoke(target);                    break;
            case EventInterfaceTypes.Scroll:                    ss = s.onScroll;                   onScroll?.Invoke(target);                  break;
            case EventInterfaceTypes.UpdateSelected:            ss = s.onUpdateSelected;           onUpdateSelected?.Invoke(target);          break;
            case EventInterfaceTypes.Select:                    ss = s.onSelect;                   onSelect?.Invoke(target);                  break;
            case EventInterfaceTypes.Deselect:                  ss = s.onDeselect;                 onDeselect?.Invoke(target);                break;
            case EventInterfaceTypes.Move:                      ss = s.onMove;                     onMove?.Invoke(target);                    break;
            case EventInterfaceTypes.Submit:                    ss = s.onSubmit;                   onSubmit?.Invoke(target);                  break;
            case EventInterfaceTypes.Cancel:                    ss = s.onCancel;                   onCancel?.Invoke(target);                  break;
            case EventInterfaceTypes.Default:
            default: break;
        }

        var a = UISoundEffect.GetSound(ss);
        if (!string.IsNullOrEmpty(a))
        {
            if (s.type != AudioTypes.Sound)
            {
                var c = s.type == AudioTypes.Voice ? instance.m_lastVoice : instance.m_lastMusic;
                if (!string.IsNullOrEmpty(c))
                {
                    if (c == a && !s.@override && AudioManager.IsInPlayListOrLoading(a)) return;
                    AudioManager.Stop(c);
                }

                if (s.type == AudioTypes.Voice)
                {
                    instance.m_lastVoice = a;
                    AudioManager.PlayVoice(a);
                }
                else
                {
                    instance.m_lastMusic = a;
                    AudioManager.PlayMusic(a);
                }
            }
            else AudioManager.PlaySound(a);
        }
    }

    #endregion

    #region NetStat statistic

#if NETSTAT
    private bool m_showNetworkPanel = false;
    private bool m_showParsePanel = false;

    private string m_server = "";

    private void OnGUI()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Root.simulateReleaseMode) return;
        #endif

        if (m_server == "") m_server = Root.defaultServer.ToString();

        var vv = viewResolution;

        m_showNetworkPanel = GUI.Toggle(new Rect(vv.x * 0.5f - 200, 0, 100, 60), m_showNetworkPanel, "Network Panel", GUI.skin.button);
        if (m_showNetworkPanel) m_showParsePanel = false;
        m_showParsePanel   = GUI.Toggle(new Rect(vv.x * 0.5f - 300, 0, 100, 60), m_showParsePanel, "Parse Panel", GUI.skin.button);
        if (m_showParsePanel) m_showNetworkPanel = false;

        if (!Session.instance || !m_showNetworkPanel && !m_showParsePanel) return;
        var receiver = Session.instance.receiver;

        var mb = Module_Battle.instance;
        if (mb.teamMode)
        {
            if (!mb.isPvP && !Module_Team.instance.useGameSession) receiver = Module_Team.instance.receiver;
            else if (mb.isPvP && !Module_PVP.instance.useGameSession) receiver = Module_PVP.instance.receiver;
        }

        if (receiver == null || receiver.destroyed)
        {
            var ns = GUI.TextField(new Rect(300, 100, 100, 60), m_server);
            if (ns != m_server)
            {
                m_server = ns;
                Session.instance?.UpdateServer(Util.Parse<int>(m_server));
            }
            return;
        }

        GUI.Label(new Rect(0, 0, vv.x, 400), "", GUI.skin.box);

        var w = vv.x * 0.001f;
        System.Collections.Generic.List<int> delays = null;
        var _delays = m_showNetworkPanel ? receiver.networkDelay : receiver.parseDelay;
        lock (_delays)
        {
            delays = new System.Collections.Generic.List<int>(_delays.Count);
            delays.AddRange(_delays);
        }
        var max = m_showParsePanel ? receiver.maxParse : receiver.maxNet;
        for (var i = 0; i < delays.Count; ++i)
        {
            var diff = delays[i];
            var h = diff / max * 370.0f;

            if (h > 370) h = 370;

            var c = GUI.color;
            GUI.color = m_showNetworkPanel ? diff < 55 ? Color.green : diff < 105 ? Color.yellow : diff < 505 ? Color.cyan : Color.red : diff > 5 ? Color.yellow : Color.green;
            GUI.DrawTexture(new Rect(w * i, 400 - h, w, h), Texture2D.whiteTexture, ScaleMode.StretchToFill);

            diff -= 5;
            if (diff >= (m_showNetworkPanel ? 50 : 1)) GUI.Label(new Rect(w * i, 400 - h, 50, 20), diff.ToString());

            GUI.color = c;
        }

        if (m_showNetworkPanel) receiver.pauseNetStatistic = GUI.Toggle(new Rect(100, 400, 100, 60), receiver.pauseNetStatistic, "Pause", GUI.skin.button);
        else receiver.pauseParseStatistic = GUI.Toggle(new Rect(100, 400, 100, 60), receiver.pauseParseStatistic, "Pause", GUI.skin.button);

        if (GUI.Button(new Rect(200, 400, 100, 60), "Clear")) receiver.ClearStat(m_showNetworkPanel ? 0 : 1);

        var s = GUI.TextField(new Rect(300, 400, 100, 60), m_server);
        if (s != m_server)
        {
            m_server = s;
            Session.instance?.UpdateServer(Util.Parse<int>(m_server));
        }
    }
#endif

    #endregion

    #region Editor helper

    #if UNITY_EDITOR

    [Space(5)]
    [SerializeField]
    private string e_Current = string.Empty;
    [SerializeField]
    private System.Collections.Generic.List<string> e_WindowStack = new System.Collections.Generic.List<string>();
    private System.Collections.Generic.List<Window.WindowHolder> e_Stack = new System.Collections.Generic.List<Window.WindowHolder>();

    private void LateUpdate()
    {
        e_Current = Window.current?.name;
        var stack = Window.stack;
        if (stack.SequenceEqual(e_Stack)) return;

        e_Stack.Clear();
        e_Stack.AddRange(stack);

        e_WindowStack.Clear();
        foreach (var w in e_Stack)
            e_WindowStack.Add($"[{w.index}:{w.windowName}]");
    }

    #endif

    #endregion
}
