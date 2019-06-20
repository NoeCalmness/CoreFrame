/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Utility class for global scene object management.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-05
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;

public class Root : SingletonBehaviour<Root>
{
    #region Static functions

    public static bool appFocused { get { return instance.m_appFocused; } }
    public static bool appPaused { get { return instance.m_appPaused; } }

    public static ServerConfigInfo serverInfo { get { return m_server; } }
    public static string fullHost { get { return m_server.fullHost; } }
    public static string alias { get { return m_server.alias; } }

    public static float leftSafeInset
    {
        get
        {
            if (!instance) return 0;

            var idx = orientation == DeviceOrientation.LandscapeLeft ? 0 : orientation == DeviceOrientation.LandscapeRight ? 1 : -1;

            return idx < 0 ? 0 : SettingsManager.notch ? hasNotch ? instance.m_safeInset[idx] : idx == 0 ? 0.04f : 0 : 0;
        }
    }
    public static float rightSafeInset
    {
        get
        {
            if (!instance) return 0;

            var idx = orientation == DeviceOrientation.LandscapeLeft ? 1 : orientation == DeviceOrientation.LandscapeRight ? 0 : -1;

            return idx < 0 ? 0 : SettingsManager.notch ? hasNotch ? instance.m_safeInset[idx] : idx == 0 ? 0.04f : 0 : 0;
        }
    }
    /// <summary>
    /// Current screen orientation
    /// </summary>
    public static DeviceOrientation orientation
    {
        get
        {
            #if UNITY_EDITOR
            if (instance && instance.m_screenOrientation != DeviceOrientation.Unknown) return instance.m_screenOrientation;
            #endif
            return instance ? instance.m_orientation : Input.deviceOrientation;
        }
    }
    public static bool hasNotch { get; private set; }

    public static bool showDebugPanel   = true;
    public static bool showFPS          = true;
    public static bool hideAllGameUI    = false;
    public static bool hideAllColliders = false;

    private static ServerConfigInfo m_server = null;

    public static int defaultServer
    {
        get { return m_defaultServer; }
        set
        {
            if (m_defaultServer == value) return;
            m_defaultServer = value;

            if (instance) instance.__defaultServer = m_defaultServer;

            UpdateServerInfo();
        }
    }
    private static int m_defaultServer = 1;

    public static bool loggerTimeTest
    {
        get { return m_loggerTimeTest; }
        set
        {
            if (m_loggerTimeTest == value) return;
            m_loggerTimeTest = value;

            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (instance) instance.e_LoggerTimeTest = m_loggerTimeTest;
            #endif

            UpdateLoggerState();
        }
    }
    public static bool m_loggerTimeTest = false;

    public static bool disableLoggerTime
    {
        get { return m_disableLoggerTime; }
        set
        {
            if (m_disableLoggerTime == value) return;
            m_disableLoggerTime = value;

            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (instance) instance.e_DisableLoggerTime = m_disableLoggerTime;
            #endif

            UpdateLoggerState();
        }
    }
    public static bool m_disableLoggerTime = false;

    public static void UpdateServerInfo()
    {
        if (m_server == null) m_server = new ServerConfigInfo();

        var si = ServerConfigInfo.Get(m_defaultServer);

        m_server.CopyFrom(si ?? ServerConfigInfo.defaultServer);
        m_server.ID = m_defaultServer;

        Logger.LogDetail("Set default server to <color=#00DDFF><b>[{0}]</b></color>", m_server);
    }

    public static void UpdateLoggerState()
    {
        Logger.SetLogState(6, true);
        Logger.LogDetail("Set logger state to <b><color=#00EEAA>{0}</color></b>", m_disableLoggerTime ? "Normal" : m_loggerTimeTest ? "TimeTest" : "TimeTest + Normal");

        if (m_disableLoggerTime)
        {
            Logger.SetLogState(-1, true);
            if (m_disableLoggerTime) Logger.SetLogState(10, false);
        }
        else
        {
            Logger.SetLogState(-1, !m_loggerTimeTest);
            Logger.SetLogState(10, true);
        }
    }

    #endregion

    public int _defaultServer
    {
        get { return __defaultServer; }
        set
        {
            if (__defaultServer == m_defaultServer && value == m_defaultServer) return;
            __defaultServer = value;

            defaultServer = __defaultServer;
        }
    }
    [SerializeField, Set("_defaultServer")]
    private int __defaultServer = 1;

    #if SHADOW_PACK
    public static string shadowAppName => m_shadowAppName ?? GeneralConfigInfo.sappName;
    private static string m_shadowAppName = null;
    #endif

    public GameObject  ui;
    public EventSystem eventSystem;

    #if UNITY_EDITOR
    [SerializeField]
    private DeviceOrientation m_screenOrientation = DeviceOrientation.Unknown;
    #endif

    [SerializeField]
    private float[] m_safeInset = new float[2];   // Distance between safearea and screen edge

    private bool m_appFocused = true;
    private bool m_appPaused = false;

    protected override void Awake()
    {
        base.Awake();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Time.timeScale = 1.0f;

#if SHADOW_PACK
        if (string.IsNullOrEmpty(m_shadowAppName))
        {
            Logger.LogWarning("Shadow app name can not be null! Use default name from config.");
            m_shadowAppName = null;
        }
#endif

#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
        Debug.unityLogger.logEnabled = false;
#else
        m_loggerTimeTest = e_LoggerTimeTest;
        m_disableLoggerTime = e_DisableLoggerTime;
        UpdateLoggerState();
#if !UNITY_EDITOR
        this.GetComponentDefault<DebugLogger>();
#endif
#endif

        m_defaultServer = __defaultServer;
        ServerConfigInfo.LoadServerInfos();

        var n = SDKManager.GetSafeInsets();
        m_safeInset = Util.ParseString<float>(n, false);
        if (m_safeInset.Length != 2) m_safeInset = new float[2] { 0, 0 };

        hasNotch = m_safeInset[0] > 0 || m_safeInset[1] > 0;

        Logger.LogDetail("Safe Inset: <color=#00FF00><b>[top:{0:F2},bottom:{1:F2}]</b></color>", m_safeInset[0], m_safeInset[1]);
    }

    private void OnApplicationFocus(bool focus)
    {
        if (m_appFocused == focus) return;
        m_appFocused = focus;

        DispatchEvent(Events.APPLICATION_FOCUS_CHANGED, Event_.Pop(m_appFocused));
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (m_appPaused == pauseStatus) return;
        m_appPaused = pauseStatus;

        DispatchEvent(Events.APPLICATION_PAUSE_CHANGED, Event_.Pop(m_appPaused));
    }

    #region Mobile device key/rotate event

    private bool m_waittingQuit = false;
    private DeviceOrientation m_orientation = DeviceOrientation.LandscapeLeft;

    private void CheckDeviceKeyInput()
    {
        if (!Game.started) return;  // Do not handle key input if game not started

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            var t = GeneralConfigInfo.swaitToQuit;
            if (m_waittingQuit || t < 0.1f) Game.Quit();
            else
            {
                m_waittingQuit = true;
                DelayEvents.Add(() => m_waittingQuit = false, t);
                Module_Global.instance.ShowGlobalNotice(31, t * 0.65f, t * 0.35f);
            }
        }
    }

    private void CheckOrientation()
    {
        var current = Input.deviceOrientation;
        #if UNITY_EDITOR
        if (m_screenOrientation != DeviceOrientation.Unknown) current = m_screenOrientation;
        #endif

        if (current != m_orientation && (current == DeviceOrientation.LandscapeLeft || current == DeviceOrientation.LandscapeRight))
        {
            var o = m_orientation;
            m_orientation = current;

            DispatchEvent(Events.APPLICATION_ORIENTATION, Event_.Pop(o, m_orientation));

            Logger.LogDetail("Orientation changed to <b><color=#00DDFF>[{0}]</color></b> from <b><color=#00DDFF>[{1}]</color></b>", m_orientation, o);
        }
    }

    #endregion

    #region Logger & Debug info

    [Space(10)]
    [SerializeField]
    private int e_PingGame = 0;
    [SerializeField]
    private int e_PingRoom = 0;
    [SerializeField]
    private float e_Fps;
    [SerializeField]
    private float e_FpsDuration = 0.1f;

    [SerializeField, Set("_loggerTimeTest"), Space(10)]
    private bool e_LoggerTimeTest = false;
    [SerializeField, Set("_disableLoggerTime")]
    private bool e_DisableLoggerTime = false;

#if DEVELOPMENT_BUILD || UNITY_EDITOR

#if DEBUG_LOG && (UNITY_ANDROID || UNITY_IOS)  // We only create debug log file on mobile platform
    private System.Collections.Generic.List<string> m_logs = new System.Collections.Generic.List<string>();
    private object m_guard = new object();
    private string m_logPath;
#endif

    private float e_lastTime;
    private float e_frameCount;

    public bool _loggerTimeTest
    {
        get { return e_LoggerTimeTest; }
        set
        {
            if (e_LoggerTimeTest == m_loggerTimeTest && value == e_LoggerTimeTest) return;
            e_LoggerTimeTest = value;

            loggerTimeTest = e_LoggerTimeTest;
        }
    }

    public bool _disableLoggerTime
    {
        get { return e_DisableLoggerTime; }
        set
        {
            if (e_DisableLoggerTime == m_disableLoggerTime && value == e_DisableLoggerTime) return;
            e_DisableLoggerTime = value;

            disableLoggerTime = e_DisableLoggerTime;
        }
    }

    private string m_sfps = "<color=#00FF00><size=20>FPS: 1</size></color>";
    private string m_spingGame = "<color=#00FF00><size=20>G ping: 0 ms</size></color>";
    private string m_spingRoom = "<color=#00FF00><size=20>R ping: 0 ms</size></color>";

#if DEBUG_LOG && (UNITY_ANDROID || UNITY_IOS)
    private void Start()
    {
        m_logPath = Application.persistentDataPath + "/debugLog.htm";
        if (System.IO.File.Exists(m_logPath)) System.IO.File.Delete(m_logPath);

        Application.logMessageReceived += HandleLog;

        m_logs.Add("<body bgcolor=\"#000000\"/>");

        Logger.LogInfo("{0} Debug log created...", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DispatchEvent(Events.APPLICATION_EXIT);
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string condition, string stackTrace, UnityEngine.LogType type)
    {
        if (type == UnityEngine.LogType.Error || type == UnityEngine.LogType.Exception) condition = "<font color=red><b>GAME::ERR: </b>" + condition + "</font></br>";

        lock (m_guard)
        {
            m_logs.Add(condition.Replace("<color", "<font color").Replace("</color>", "</font></br>"));
            if (type == UnityEngine.LogType.Error || type == UnityEngine.LogType.Exception)
                m_logs.Add("<font color=red><b>GAME::ERR: </b>" + stackTrace + "</font></br>");
        }
    }
#else
    protected override void OnDestroy()
    {
        DispatchEvent(Events.APPLICATION_EXIT);
        base.OnDestroy();
    }
#endif

    void LateUpdate()
    {
#if DEBUG_LOG && (UNITY_ANDROID || UNITY_IOS)
        lock (m_guard)
        {
            if (m_logs.Count > 0)
            {
                using (var w = new System.IO.StreamWriter(m_logPath, true, System.Text.Encoding.UTF8))
                {
                    for (var i = 0; i < m_logs.Count; ++i)
                        w.WriteLine(m_logs[i]);
                }
                m_logs.Clear();
            }
        }
#endif

#if UNITY_EDITOR
        if (e_DelayLogicPaused != -1)
        {
            _logicPaused = e_DelayLogicPaused == 1;
            e_DelayLogicPaused = -1;
        }
#endif

        ++e_frameCount;

        var d = Time.realtimeSinceStartup - e_lastTime;
        if (d >= e_FpsDuration)
        {
            e_Fps = e_frameCount / d;
            e_lastTime = Time.realtimeSinceStartup;

            e_frameCount = 0;

            m_sfps = Util.Format("<color=#00FF00><size=20>FPS: {0:F2}</size></color>", e_Fps);
        }

        if (!Game.started) return;

        if (e_PingGame != Session.instance.ping || e_PingRoom != Module_PVP.instance.ping)
        {
            e_PingGame = Session.instance.ping;
            e_PingRoom = Module_PVP.instance.ping;

            m_spingGame = Util.Format("<color=#00FF00><size=20>G ping: {0} ms</size></color>", e_PingGame);
            m_spingRoom = Util.Format("<color=#00FF00><size=20>R ping: {0} ms</size></color>", e_PingRoom);
        }
    }

    void OnGUI()
    {
        if (e_SimulateReleaseMode) return;

        if (SettingsManager.notch)
        {
            for (var i = 0; i < 2; ++i)
            {
                var d = i == 0 ? leftSafeInset : rightSafeInset;
                if (d == 0) continue;

                var res = UIManager.realViewResolution;

                var h = res.y * 0.3f;
                var y = res.y * 0.5f - h * 0.5f;
                var w = d * res.x;
                var r = new Rect(i == 0 ? 0 : res.x - w, y, w, h);

                GUI.DrawTexture(r, Texture2D.whiteTexture);
            }
        }

        if (showFPS) GUI.Label(new Rect(5, 240, 200, 35), m_sfps);

        if (!showDebugPanel || hideAllGameUI) return;

        GUI.Label(new Rect(5, 270, 500, 30), m_spingGame);
        GUI.Label(new Rect(5, 300, 500, 30), m_spingRoom);
    }
#else
    protected override void OnDestroy()
    {
        DispatchEvent(Events.APPLICATION_EXIT);
        base.OnDestroy();
    }
#endif

    #endregion

    #region Editor helper

    [Header("Editor Debug Info"), SerializeField, Set("_simulateReleaseMode")]
    private bool e_SimulateReleaseMode = false;

    private bool e_LogicPaused = false;

    [Space(5)]
    public bool e_ShowDebugPanel = true;
    public bool e_ShowFPS = true;

    [SerializeField]
    private float e_LevelTime;

    public float e_GamePingInterval = 5.0f;
    public float e_RoomPingInterval = 1.0f;

    public bool e_AppPaused = false;
    public bool e_AppFocused = true;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>
    /// Simulate release mode, hide all debug infos
    /// Warning: Only avaliable in Editor mode or Development Build
    /// </summary>
    public static bool simulateReleaseMode
    {
        get { return instance && instance._simulateReleaseMode; }
        set { if (instance) instance._simulateReleaseMode = value; }
    }

    private bool _simulateReleaseMode
    {
        get { return e_SimulateReleaseMode; }
        set
        {
            if (e_SimulateReleaseMode == value) return;
            e_SimulateReleaseMode = value;

            DispatchEvent("EditorSimulateReleaseMode", Event_.Pop(e_SimulateReleaseMode));
        }
    }
#endif

#if UNITY_EDITOR
    /// <summary>
    /// Warning: Only avaliable in Editor mode
    /// </summary>
    public static void ForceButtonState(string button, bool fired, bool oneShot = true)
    {
        if (!instance) return;

        var p = instance._logicPaused;
        instance._logicPaused = false;

        InputManager.SetCustomButtonState(button, fired, oneShot);

        instance._logicPaused = p;
    }

    /// <summary>
    /// Warning: Only avaliable in Editor mode
    /// </summary>
    public static bool logicPaused
    {
        get { return instance && instance._logicPaused; }
        set { if (instance) instance.e_DelayLogicPaused = value ? 1 : 0; }
    }

    private bool _logicPaused
    {
        get { return e_LogicPaused; }
        set
        {
            if (e_LogicPaused == value) return;
            e_LogicPaused = value;

            ObjectManager.enableUpdate = !e_LogicPaused;
        }
    }

    private int e_DelayLogicPaused = -1;
    private bool e_ShowDebugPanel_old = true;
    private bool e_ShowFPS_old = true;

    private float e_GamePingInterval_old = 5.0f;
    private float e_RoomPingInterval_old = 1.0f;

    private float[] e_SafeInset = new float[2];

    private void Update()
    {
        CheckDeviceKeyInput();
        CheckOrientation();

        e_LevelTime = Level.levelTime;

        if (e_ShowFPS_old != e_ShowFPS) showFPS = e_ShowFPS;
        else e_ShowFPS = showFPS;
        e_ShowFPS_old = e_ShowFPS;

        if (e_ShowDebugPanel_old != e_ShowDebugPanel) showDebugPanel = e_ShowDebugPanel;
        else e_ShowDebugPanel = showDebugPanel;
        e_ShowDebugPanel_old = e_ShowDebugPanel;

        if (e_GamePingInterval_old != e_GamePingInterval)
        {
            e_GamePingInterval_old = e_GamePingInterval;
            if (Game.started) Session.instance.pingInterval = e_GamePingInterval;
        }

        if (e_RoomPingInterval_old != e_RoomPingInterval)
        {
            e_RoomPingInterval_old = e_RoomPingInterval;
            if (Game.started) Module_PVP.instance.pingInterval = e_RoomPingInterval;
        }

        e_AppFocused = m_appFocused;
        e_AppPaused  = m_appPaused;

        if (e_SafeInset[0] != m_safeInset[0] || e_SafeInset[1] != m_safeInset[1])
        {
            e_SafeInset[0] = m_safeInset[0];
            e_SafeInset[1] = m_safeInset[1];

            hasNotch = m_safeInset[0] > 0 || m_safeInset[1] > 0;
        }
    }
#else
    private void Update()
    {
        CheckDeviceKeyInput();
        CheckOrientation();
    }
#endif

    #endregion
}
