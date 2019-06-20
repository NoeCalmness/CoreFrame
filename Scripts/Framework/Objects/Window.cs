/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Base window class.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-05
 * 
 ***************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetBundles;
using Object = UnityEngine.Object;

public abstract class Window : SceneObject, IRenderObject
{
    /// <summary>
    /// Window data holder
    /// </summary>
    public sealed class WindowHolder : PoolObject<WindowHolder>
    {
        public const int MAX_DATA_COUNT = 6;

        public static WindowHolder empty => m_empty;
        private static WindowHolder m_empty = Create();

        private static int m_index = 0;

        public static WindowHolder Create(string _window)
        {
            var t = Game.GetType(_window);
            if (t == null)
            {
                Logger.LogError($"WindowHolder::Create: Invalid window name <b><color=#9CE8FF>[{_window}]</color></b>");

                return null;
            }

            var holder = Create(false);

            holder.m_windowName = _window;
            holder.m_holdIndex  = m_index++;

            holder.subTypeLock  = -1;

            holder.OnInitialize();
            holder.SetData();

            return holder;
        }

        public string windowName => m_windowName;

        public int subTypeLock = -1;

        public int index => m_holdIndex;

        private string m_windowName;
        private object[] m_datas = new object[MAX_DATA_COUNT];

        private int m_holdIndex = -1;

        private WindowHolder() { }

        public T GetData<T>(int index)
        {
            if (index < 0 || index >= MAX_DATA_COUNT)
            {
                Logger.LogError($"WindowHolder::GetData: Index must between <b>[0-{MAX_DATA_COUNT - 1}]</b>. Window:<b>{m_windowName}</b>");
                return default(T);
            }

            var data = m_datas[index];

            if (data == null) return default(T);
            if (data is T) return (T)data;

            Logger.LogWarning($"WindowHolder::GetData: Can not convert to type <b>{typeof(T)}</b>. Window:<b>{m_windowName}</b>, index:<b>{index}</b>");
            return default(T);
        }

        public T GetData<T>(int index, T defaultValue)
        {
            if (index < 0 || index >= MAX_DATA_COUNT)
            {
                Logger.LogError($"WindowHolder::GetData: Index must between <b>[0-{MAX_DATA_COUNT-1}]</b>. Window:<b>{m_windowName}</b>");
                return defaultValue;
            }

            var data = m_datas[index];

            if (data == null) return defaultValue;
            if (data is T) return (T)data;

            Logger.LogWarning($"WindowHolder::GetData: Can not convert to type <b>{typeof(T)}</b>. Window:<b>{m_windowName}</b>, index:<b>{index}</b>");
            return defaultValue;
        }

        public void SetData(object _data0 = null, object _data1 = null, object _data2 = null, object _data3 = null, object _data4 = null, object _data5 = null)
        {
            m_datas[0] = _data0;
            m_datas[1] = _data1;
            m_datas[2] = _data2;
            m_datas[3] = _data3;
            m_datas[4] = _data4;
            m_datas[5] = _data5;

            Logger.LogDetail($"WindowHolder: Holder data <b><color=#9CE8FF>[{m_windowName}:{m_holdIndex}]</color></b> set to :<b><color=#9CE8FF>[subType:{subTypeLock}|{_data0}|{_data1}|{_data2}|{_data3}|{_data4}|{_data5}]</color></b>");
        }

        public override string ToString()
        {
            return destroyed ? "null" : $"{m_holdIndex}:{m_windowName}";
        }

        #region Debug

        #if DEVELOPMENT_BUILD || UNITY_EDITOR

        protected override void OnInitialize()
        {
            Logger.LogDetail($"WindowHolder: Holder <b><color=#9CE8FF>[{m_windowName}:{m_holdIndex}]</color> <color=#00FF00>created</color></b>!");
        }

        protected override void OnDestroy()
        {
            Logger.LogDetail($"WindowHolder: Holder <b><color=#9CE8FF>[{m_windowName}:{m_holdIndex}]</color> <color=#FF0000>destroyed</color></b>! Params:<b><color=#9CE8FF>[{subTypeLock}|{m_datas[0]}|{m_datas[1]}|{m_datas[2]}|{m_datas[3]}|{m_datas[4]}|{m_datas[5]}]</color></b>");
        }

        #endif

        #endregion
    }

    #region Static functions

    public enum InOutAction { Animated = 0, Immediately = 1, OnInOut = 2 }

    /// <summary>
    /// Current actived full screen window
    /// </summary>
    public static Window current { get { return m_current; } }
    private static Window m_current = null;

    /// <summary>
    /// Get current window stack list
    /// <para></para>
    /// Warning: Modify window stack may cause unexpected exceptions
    /// </summary>
    public static List<WindowHolder> stack => m_stack;
    private static List<WindowHolder> m_stack = new List<WindowHolder>();

    private static List<Event_> m_params = new List<Event_>();
    private static List<Window> m_windows = new List<Window>();

    /// <summary>
    /// Prepare window assets or any based assets before we open target window.<para>
    /// By default, we use normal loading indicator (a circle) when loading window assets</para>
    /// We will use Level Loading Window as indicator when loading these assets
    /// </summary>
    /// <param name="type">Target window type</param>
    /// <param name="assets">Based assets</param>
    /// <param name="onComplete">Callback when assets prepared.</param>
    /// <param name="delay">Loading window show delay</param>
    public static void PrepareWindowAssetsAsLevel(Type type, List<string> assets, Action<bool> onComplete, float delay = 0.2f)
    {
        var name = Game.GetDefaultName(type);
        if (string.IsNullOrEmpty(name))
        {
            Logger.LogError("Window::PrepareWindowAssetsAsLevel: Unknow window type [{0}]", type.Name);
            onComplete?.Invoke(false);
        }

        _PrepareWindowAssetsAsLevel(name, assets, onComplete, delay);
    }

    /// <summary>
    /// Prepare window assets or any based assets before we open target window.<para>
    /// By default, we use normal loading indicator (a circle) when loading window assets</para>
    /// We will use Level Loading Window as indicator when loading these assets
    /// </summary>
    /// <param name="name">Target window name</param>
    /// <param name="assets">Based assets</param>
    /// <param name="onComplete">Callback when assets prepared.</param>
    /// <param name="delay">Loading window show delay</param>
    public static void PrepareWindowAssetsAsLevel(string name, List<string> assets, Action<bool> onComplete, float delay = 0.2f)
    {
        var type = Game.GetType(name);
        if (type == null)
        {
            Logger.LogError("Window::PrepareWindowAssetsAsLevel: Unknow window name [{0}]", name);
            onComplete?.Invoke(false);
        }

        _PrepareWindowAssetsAsLevel(name, assets, onComplete, delay);
    }

    /// <summary>
    /// Prepare window assets or any based assets before we open target window.<para>
    /// By default, we use normal loading indicator (a circle) when loading window assets</para>
    /// We will use Level Loading Window as indicator when loading these assets
    /// </summary>
    /// <param name="assets">Based assets</param>
    /// <param name="onComplete">Callback when assets prepared.</param>
    /// <param name="delay">Loading window show delay</param>
    public static void PrepareWindowAssetsAsLevel<T>(List<string> assets, Action<bool> onComplete, float delay = 0.2f) where T : Window
    {
        PrepareWindowAssetsAsLevel(typeof(T), assets, onComplete, delay);
    }

    private static void _PrepareWindowAssetsAsLevel(string name, List<string> assets, Action<bool> onComplete, float delay = 0.2f)
    {
        var watcher = TimeWatcher.Watch("Window._PrepareWindowAssetsAsLevel");

        UIManager.instance.DispatchEvent(Events.SHOW_LOADING_WINDOW, Event_.Pop(true, null, delay));

        assets.Add(name.ToLower());
        Level.PrepareAssets(assets, f =>
        {
            watcher.UnWatch();

            UIManager.instance.DispatchEvent(Events.SHOW_LOADING_WINDOW, Event_.Pop(false));
            onComplete?.Invoke(f);
        }, p => UIManager.instance.DispatchEvent(Events.SCENE_LOAD_PROGRESS, Event_.Pop(p)));
    }

    /// <summary>
    /// Open a window async and show it with custom fade animation after loaded
    /// </summary>
    /// <typeparam name="T">The window type</typeparam>
    /// <param name="speed">Animation speed</param>
    /// <param name="onOpen">callback when window open, if failed, param is null</param>
    /// <param name="onShow">callback when window becom visible (after show animation end)</param>
    public static void ShowAsync<T>(Action<Window> onOpen = null, Action<Window> onShow = null) where T : Window
    {
        var name = Game.GetDefaultName<T>();
        if (string.IsNullOrEmpty(name))
        {
            Logger.LogError("Window::ShowAsync: Unknow window type [{0}]", typeof(T).Name);
            onOpen?.Invoke(null);
            return;
        }

        UIManager.instance.StartCoroutine(_PrepareWindow(name, true, true, false, -1, onOpen, onShow));
    }

    /// <summary>
    /// Open a window async and show it immediately after loaded
    /// </summary>
    /// <typeparam name="T">The window type</typeparam>
    /// <param name="onOpen">callback when window open, if failed, param is null</param>
    /// <param name="onShow">callback when window becom visible (after show animation end)</param>
    public static void ShowImmediatelyAsync<T>(Action<Window> onOpen = null, Action<Window> onShow = null) where T : Window
    {
        var name = Game.GetDefaultName<T>();
        if (string.IsNullOrEmpty(name))
        {
            Logger.LogError("Window::ShowImmediatelyAsync: Unknow window type [{0}]", typeof(T).Name);
            onOpen?.Invoke(null);
            return;
        }

        UIManager.instance.StartCoroutine(_PrepareWindow(name, true, true, true, -1, onOpen, onShow));
    }

    /// <summary>
    /// Open a window async and show it with default fade animation after loaded
    /// </summary>
    /// <param name="name">window name</param>
    /// <param name="onOpen">callback when window open, if failed, param is null</param>
    /// <param name="onShow">callback when window becom visible (after show animation end)</param>
    /// <param name="args">arguments</param>
    public static void ShowAsync(string name, Action<Window> onOpen = null, Action<Window> onShow = null)
    {
        UIManager.instance.StartCoroutine(_PrepareWindow(name, true, true, false, -1, onOpen, onShow));
    }

    /// <summary>
    /// Open a window async and show it immediately after loaded
    /// </summary>
    /// <param name="name">window name</param>
    /// <param name="onOpen">callback when window open, if failed, param is null</param>
    /// <param name="onShow">callback when window becom visible (after show animation end)</param>
    public static void ShowImmediatelyAsync(string name, Action<Window> onOpen = null, Action<Window> onShow = null)
    {
        UIManager.instance.StartCoroutine(_PrepareWindow(name, true, true, true, -1, onOpen, onShow));
    }

    /// <summary>
    /// Skip back to a stack window, and remove all stack record created later than target window
    /// If target window not in stack list, will show it normaly
    /// </summary>
    /// <param name="name">window name</param>
    /// <param name="onOpen">callback when window open, if failed, param is null</param>
    /// <param name="onShow">callback when window becom visible (after show animation end)</param>
    public static void SkipBackTo(string name, Action<Window> onOpen = null, Action<Window> onShow = null)
    {
        var idx = m_stack.FindIndex(0, w => w.windowName == name);
        if (idx < 0) ShowAsync(name, onOpen, onShow);
        else
        {
            if (idx > 0) m_stack.RemoveRange(0, idx);
            if (m_current) m_current._Hide(false, false, false);
            else
                _PrepareWindowInternal(name, true, false, false, -1, onOpen, onShow);
        }
    }

    /// <summary>
    /// Skip back to a stack window, and remove all stack record created later than target window
    /// If target window not in stack list, will show it normaly
    /// </summary>
    /// <param name="name">window name</param>
    /// <param name="onOpen">callback when window open, if failed, param is null</param>
    /// <param name="onShow">callback when window becom visible (after show animation end)</param>
    public static void SkipBackToImmediately(string name, Action<Window> onOpen = null, Action<Window> onShow = null)
    {
        var idx = m_stack.FindIndex(0, w => w.windowName == name);
        if (idx < 0) ShowImmediatelyAsync(name, onOpen, onShow);
        else
        {
            if (idx > 0) m_stack.RemoveRange(0, idx);
            if (m_current) m_current._Hide(false, true, false);
            else
                _PrepareWindowInternal(name, true, false, true, -1, onOpen, onShow);
        }
    }

    /// <summary>
    /// Skip back to a stack window, and remove all stack record created later than target window
    /// If target window not in stack list, will show it normaly
    /// </summary>
    /// <param name="name">window name</param>
    /// <param name="onOpen">callback when window open, if failed, param is null</param>
    /// <param name="onShow">callback when window becom visible (after show animation end)</param>
    public static void SkipBackTo<T>(Action<Window> onOpen = null, Action<Window> onShow = null)
    {
        var name = Game.GetDefaultName<T>();
        if (string.IsNullOrEmpty(name))
        {
            Logger.LogError("Window::SkipBackTo: Unknow window type [{0}]", typeof(T).Name);
            onOpen?.Invoke(null);
            return;
        }

        SkipBackTo(name, onOpen, onShow);
    }

    /// <summary>
    /// Skip back to a stack window, and remove all stack record created later than target window
    /// If target window not in stack list, will show it normaly
    /// </summary>
    /// <param name="name">window name</param>
    /// <param name="onOpen">callback when window open, if failed, param is null</param>
    /// <param name="onShow">callback when window becom visible (after show animation end)</param>
    public static void SkipBackToImmediately<T>(Action<Window> onOpen = null, Action<Window> onShow = null)
    {
        var name = Game.GetDefaultName<T>();
        if (string.IsNullOrEmpty(name))
        {
            Logger.LogError("Window::SkipBackToImmediately: Unknow window type [{0}]", typeof(T).Name);
            onOpen?.Invoke(null);
            return;
        }

        SkipBackToImmediately(name, onOpen, onShow);
    }

    /// <summary>
    /// Open target window and skip to target subwindow
    /// </summary>
    /// <param name="type">Target window sub panel type</param>
    public static void GotoSubWindow(string name, int type, Action<Window> onOpen = null, Action<Window> onShow = null)
    {
        _PrepareWindowInternal(name, true, true, false, type, onOpen, onShow);
    }

    /// <summary>
    /// Open target window and skip to target subwindow
    /// </summary>
    /// <param name="type">Target window sub panel type</param>
    public static void GotoSubWindowImmediately(string name, int type, Action<Window> onOpen = null, Action<Window> onShow = null)
    {
        _PrepareWindowInternal(name, true, true, true, type, onOpen, onShow);
    }

    /// <summary>
    /// Open target window and skip to target subwindow
    /// </summary>
    /// <param name="type">Target window sub panel type</param>
    public static void GotoSubWindow<T>(int type, Action<Window> onOpen = null, Action<Window> onShow = null)
    {
        var name = Game.GetDefaultName<T>();
        if (string.IsNullOrEmpty(name))
        {
            Logger.LogError($"Window::GotoSubWindow: Unknow window type [{typeof(T).Name}], SubType:{type}");
            onOpen?.Invoke(null);
            return;
        }

        GotoSubWindow(name, type, onOpen, onShow);
    }

    /// <summary>
    /// Open target window and skip to target subwindow
    /// </summary>
    /// <param name="type">Target window sub panel type</param>
    public static void GotoSubWindowImmediately<T>(int type, Action<Window> onOpen = null, Action<Window> onShow = null)
    {
        var name = Game.GetDefaultName<T>();
        if (string.IsNullOrEmpty(name))
        {
            Logger.LogError($"Window::GotoSubWindowImmediately: Unknow window type [{typeof(T).Name}], SubType:{type}");
            onOpen?.Invoke(null);
            return;
        }

        GotoSubWindowImmediately(name, type, onOpen, onShow);
    }

    private static void _PrepareWindowInternal(string name, bool show, bool forward, bool immediately, int subType = -1, Action<Window> onOpen = null, Action<Window> onShow = null)
    {
        UIManager.instance.StartCoroutine(_PrepareWindow(name, show, forward, immediately, subType, onOpen, onShow));
    }

    private static IEnumerator _PrepareWindow(string name, bool show, bool forward, bool immediately, int subType = -1, Action<Window> onOpen = null, Action<Window> onShow = null)
    {
        if (show) DispathWillOpenEvent(name);

        var window = CheckCachedWindow(name, show, forward, immediately, subType, onOpen, onShow);
        if (window) yield break;

        CanvasGroup cg = null;
        Animator am = null;
        var an = name.ToLower();
        var obj = Level.current ? Level.GetPreloadObject(an) : null;
        if (obj)
        {
            cg = obj.GetComponent<CanvasGroup>();
            if (cg) cg.alpha = 0.0f;  // Set invisible

            am = obj.GetComponent<Animator>();
            if (am) am.enabled = false;

            yield return new WaitForEndOfFrame();

            window = Open(name, obj);
            if (window) window.m_subTypeLock = subType;

            onOpen?.Invoke(window);

            if (window && show)
            {
                if (!forward && window.isFullScreen && !m_current) m_current = window;

                window.m_onShow += onShow;
                if (!window.defaultHide) window._Show(forward, immediately);
            }

            yield break;
        }

        var type = Game.GetType(name);
        if (type == null)
        {
            Logger.LogWarning("Could not open window [{0}], unknow window type.", name);

            if (show) DispathOpenErrorEvent(name);

            onOpen?.Invoke(null);
            yield break;
        }

        moduleGlobal.LockUI();

        var watcher = TimeWatcher.Watch("Window._ShowAsync");
        var op = AssetManager.LoadAssetAsync(an, an, typeof(GameObject));
        yield return op;
        watcher.See("AssetManager.LoadAssetAsync({0})", an);

        obj = op?.GetAsset<GameObject>();

        if (obj) obj = Object.Instantiate(obj);
        AssetManager.UnloadAssetBundle(an);
        watcher.See("Object.Instantiate");

        if (!obj)
        {
            Logger.LogWarning("Open window {0} failed", name);

            if (show) DispathOpenErrorEvent(name);

            onOpen?.Invoke(null);
            watcher.UnWatch(false);
            yield break;
        }

        cg = obj.GetComponent<CanvasGroup>();
        if (cg) cg.alpha = 0.0f;  // Set invisible

        am = obj.GetComponent<Animator>();
        if (am) am.enabled = false;

        yield return new WaitForEndOfFrame();

        window = Open(name, obj);
        if (window) window.m_subTypeLock = subType;

        onOpen?.Invoke(window);

        if (window && show)
        {
            if (!forward && window.isFullScreen && !m_current) m_current = window;

            window.m_onShow += onShow;
            if (!window.defaultHide) window._Show(forward, immediately);
        }

        moduleGlobal.UnLockUI();
        watcher.See("Initialize Window");
        watcher.UnWatch(false);
    }

    /// <summary>
    /// Open a window and show it immediately
    /// </summary>
    /// <param name="name">window name</param>
    /// <param name="obj">The window gameobject</param>
    /// <param name="args">arguments</param>
    public static Window ShowImmediately(string name, GameObject obj)
    {
        var window = CheckCachedWindow(name, true, true, true);
        if (window)
        {
            if (obj) Object.DestroyImmediate(obj);

            return window;
        }

        window = Open(name, obj);
        if (window && !window.defaultHide) window.Show(true);

        return window;
    }

    /// <summary>
    /// Open a window and show it immediately
    /// </summary>
    /// <typeparam name="T">The window type</typeparam>
    /// <param name="obj">The window gameobject</param>
    /// <param name="args">arguments</param>
    public static Window ShowImmediately<T>(GameObject obj) where T : Window
    {
        var name = Game.GetDefaultName<T>();
        if (string.IsNullOrEmpty(name))
        {
            Logger.LogError("Window::ShowImmediately: Unknow window type <b>[{0}]</b>", typeof(T).Name);
            return null;
        }

        return ShowImmediately(name, obj);
    }

    /// <summary>
    /// Open a window
    /// </summary>
    /// <typeparam name="T">The window type</typeparam>
    /// <param name="obj">The window gameobject</param>
    public static T Open<T>(GameObject obj) where T : Window
    {
        var name = Game.GetDefaultName<T>();
        if (string.IsNullOrEmpty(name))
        {
            Logger.LogError("Window::Open: Unknow window type <b>[{0}]</b>", typeof(T).Name);
            return null;
        }

        return (T)Open(name, obj);
    }

    /// <summary>
    /// Open a window
    /// </summary>
    /// <param name="name">window name</param>
    /// <param name="obj">The window gameobject</param>
    public static Window Open(string name, GameObject obj)
    {
        Window window = null;

        if (window = m_windows.Find(w => w.name == name))
        {
            if (obj) Object.DestroyImmediate(obj);

            return window;
        }

        var type = Game.GetType(name);
        if (type == null)
        {
            Logger.LogWarning("Could not open window <b>[{0}]</b>, unknow window type.", name);

            if (obj) Object.DestroyImmediate(obj);

            return window;
        }

        window = (Window)Create(name, type, obj, false);

        if (!window)
        {
            Logger.LogWarning("Could not open window <b>[{0}]</b>, create window failed.", name);

            if (obj) Object.DestroyImmediate(obj);
        }

        return window;
    }

    /// <summary>
    /// Hide a window
    /// </summary>
    /// <param name="name">window name</param>
    /// <param name="immediately">Hide immediately (no trasition animation) ?</param>
    public static void Hide(string name, bool immediately = false)
    {
        var window = m_windows.Find(w => w.name == name);
        if (window) window.Hide(immediately);
    }

    /// <summary>
    /// Hide a window by Type
    /// </summary>
    /// <typeparam name="T">Window Type</typeparam>
    /// <param name="immediately">Hide immediately (no trasition animation) ?</param>
    public static void Hide<T>(bool immediately = false) where T : Window
    {
        var window = m_windows.Find(w => w is T);
        if (window) window.Hide(immediately);
    }

    /// <summary>
    /// Hide all non-full screen windows
    /// </summary>
    /// <param name="except">Except windows</param>
    /// <param name="immediately"></param>
    /// <param name="ignoreGlobal">Ignore global windows. See <see cref="markedGlobal"/></param>
    public static void HideAllNonFullScreenWindows(string except = null, bool immediately = false, bool ignoreGlobal = true)
    {
        foreach (var w in m_windows)
        {
            if (!w || !w.actived || w.isFullScreen || ignoreGlobal && w.markedGlobal || w.name == except) continue;
            w._Hide(true, immediately, false);
        }
    }

    /// <summary>
    /// Hide all non-full screen windows
    /// </summary>
    /// <param name="except">Except windows</param>
    /// <param name="immediately"></param>
    /// <param name="ignoreGlobal">Ignore global windows. See <see cref="markedGlobal"/></param>
    public static void HideAllNonFullScreenWindowsBut(string[] except, bool immediately = false, bool ignoreGlobal = true)
    {
        foreach (var w in m_windows)
        {
            if (!w || !w.actived || w.isFullScreen || ignoreGlobal && w.markedGlobal || except != null && except.Contains(w.name)) continue;
            w._Hide(true, immediately, false);
        }
    }

    /// <summary>
    /// Hide all actived full screen windows
    /// </summary>
    /// <param name="immediately">Hide immediately (no trasition animation) ?</param>
    /// <param name="except">Excep window names</param>
    public static void HideAllWindowsBut(string except = null, bool forward = true, bool immediately = true)
    {
        foreach (var w in m_windows)
        {
            if (!w || !w.actived || !w.isFullScreen || w.name == except) continue;
            w._Hide(forward, immediately, false);
        }
    }

    /// <summary>
    /// Hide all actived full screen windows 
    /// </summary>
    /// <param name="immediately">Hide immediately (no trasition animation) ?</param>
    /// <param name="except">Excep window instance</param>
    public static void HideAllWindowsBut(Window except, bool forward = true, bool immediately = true)
    {
        foreach (var w in m_windows)
        {
            if (!w || !w.actived || !w.isFullScreen || w == except) continue;
            w._Hide(forward, immediately, false);
        }
    }

    /// <summary>
    /// Hide all actived full screen windows not in except list
    /// </summary>
    /// <param name="immediately">Hide immediately (no trasition animation) ?</param>
    /// <param name="except">Exceps window names</param>
    public static void HideAllWindows(bool forward = true, bool immediately = true, params string[] except)
    {
        foreach (var w in m_windows)
        {
            if (!w || !w.actived || !w.isFullScreen || Array.IndexOf(except, w.name) > -1) continue;
            w._Hide(forward, immediately, false);
        }
    }

    /// <summary>
    /// Hide all actived windows not in except list
    /// </summary>
    /// <param name="immediately">Hide immediately (no trasition animation) ?</param>
    /// <param name="except">Exceps window names</param>
    public static void ForceHideAllWindows(bool forward = true, bool immediately = true, params string[] except)
    {
        foreach (var w in m_windows)
        {
            if (!w || !w.actived || Array.IndexOf(except, w.name) > -1) continue;
            w._Hide(forward, immediately, false);
        }
    }

    /// <summary>
    /// Get an opened window
    /// </summary>
    /// <typeparam name="T">window type</typeparam>
    /// <param name="name">window name</param>
    /// <returns></returns>
    public static T GetOpenedWindow<T>(string name) where T : Window
    {
        var window = m_windows.Find(w => w && w is T && w.name == name);
        return (T)window;
    }

    /// <summary>
    /// Get an opened window
    /// </summary>
    /// <typeparam name="T">window type</typeparam>
    /// <returns></returns>
    public static T GetOpenedWindow<T>() where T : Window
    {
        var window = m_windows.Find(w => w && w is T);
        return (T)window;
    }

    /// <summary>
    /// Get an opened window
    /// </summary>
    /// <param name="name">window name</param>
    /// <returns></returns>
    public static Window GetOpenedWindow(string name)
    {
        var window = m_windows.Find(w => w && w.name == name);
        return window;
    }

    /// <summary>
    /// Destroy a window
    /// </summary>
    /// <param name="name">window name</param>
    public static void DestroyWindow(string name)
    {
        var window = m_windows.Find(w => w.name == name);
        if (window) window.Destroy();
    }

    /// <summary>
    /// Destroy all windows
    /// </summary>
    public static void DestroyWindows()
    {
        foreach (var p in m_params) Event_.Back(p);
        m_params.Clear();

        m_stack.Clear(true);

        for (var i = m_windows.Count - 1; i > -1; --i)
        {
            var window = m_windows[i];
            if (window.markedGlobal) continue;

            try { window.Destroy(); }
            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            catch (Exception e)
            {
                Logger.LogException("Window::DestroyWindows: Exception when destroying window <b><color=#F5FFC9>[{0}]</color</b>");
                Logger.LogException(e);
            }
            #else
            catch { }
            #endif
        }
    }

    /// <summary>
    /// If current window exists, grab restore data
    /// <para>
    /// If <see cref="ignoreStack"/> is true, remove it from stack
    /// </para>
    /// </summary>
    public static void GrabCurrentRestoreData()
    {
        if (!m_current || m_stack.Count < 1 || m_stack[0].windowName != m_current.name) return;
        if (m_current.ignoreStack) m_stack.RemoveAt(0, true);
        else m_current._GrabRestoreData(m_stack[0]);
    }

    /// <summary>
    /// Set window param
    /// <para></para>
    /// Params will clear after next window show event
    /// <see cref="GetWindowParam(string)"/>
    /// </summary>
    /// <param name="window">Target window</param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    /// <param name="param4"></param>
    public static void SetWindowParam(string window, object param1, object param2 = null, object param3 = null, object param4 = null)
    {
        var e = m_params.Find(p => p.name == window);
        if (e == null)
        {
            e = Event_.Pop(param1, param2, param3, param4);
            e.name = window;

            m_params.Add(e);
        }
        else e.SetParams(param1, param2, param3, param4);
    }

    /// <summary>
    /// Get window param
    /// <para></para>
    /// <see cref="SetWindowParam(string, object, object, object, object)"/>
    /// </summary>
    /// <param name="window">Target window</param>
    public static Event_ GetWindowParam(string window)
    {
        var e = m_params.Find(p => p.name == window);
        return e;
    }

    /// <summary>
    /// Clear target window param
    /// </summary>
    /// <param name="window">Target window</param>
    public static void ClearWindowParam(string window)
    {
        for (var i = m_params.Count - 1; i > -1; --i)
        {
            var e = m_params[i];
            if (e.name == window)
            {
                m_params.RemoveAt(i);
                Event_.Back(e);
            }
        }
    }

    /// <summary>
    /// Set window param
    /// <para></para>
    /// Params will clear after next window show event
    /// <see cref="GetWindowParam(string)"/>
    /// </summary>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    /// <param name="param4"></param>
    public static void SetWindowParam<T>(object param1, object param2 = null, object param3 = null, object param4 = null) where T : Window
    {
        var window = Game.GetDefaultName<T>();
        if (string.IsNullOrEmpty(window))
        {
            Logger.LogError("Window::SetWindowParam: Unknow window type [{0}]", typeof(T).Name);
            return;
        }

        SetWindowParam(window, param1, param2, param3, param4);
    }

    /// <summary>
    /// Get window param
    /// <para></para>
    /// <see cref="SetWindowParam<T>(object, object, object, object)"/>
    /// </summary>
    public static Event_ GetWindowParam<T>() where T : Window
    {
        var window = Game.GetDefaultName<T>();
        if (string.IsNullOrEmpty(window))
        {
            Logger.LogError("Window::GetWindowParam: Unknow window type [{0}]", typeof(T).Name);
            return null;
        }

        return GetWindowParam(window);
    }

    /// <summary>
    /// Clear target window param
    /// </summary>
    public static void ClearWindowParam<T>()
    {
        var window = Game.GetDefaultName<T>();
        if (string.IsNullOrEmpty(window))
        {
            Logger.LogError("Window::ClearWindowParam: Unknow window type [{0}]", typeof(T).Name);
            return;
        }

        ClearWindowParam(window);
    }

    private static Window CheckCachedWindow(string name, bool show, bool forward, bool immediately, int subType = -1, Action<Window> onOpen = null, Action<Window> onShow = null)
    {
        var window = m_windows.Find(w => w.name == name);
        if (window)
        {
            window.m_subTypeLock = subType;

            onOpen?.Invoke(window);

            if (show)
            {
                if (!forward && window.isFullScreen && !m_current) m_current = window;

                window.m_onShow += onShow;
                window._Show(forward, immediately);
            }
        }
        return window;
    }

    private static void DispathWillOpenEvent(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        UIManager.instance.DispatchEvent(Events.UI_WINDOW_WILL_OPEN, Event_.Pop(name));
    }

    private static void DispathOpenErrorEvent(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        UIManager.instance.DispatchEvent(Events.UI_WINDOW_OPEN_ERROR, Event_.Pop(name));
    }

    #endregion

    #region Public fields

    /// <summary>
    /// Window actived ?
    /// </summary>
    public bool actived => gameObject && gameObject.activeSelf;
    /// <summary>
    /// In/Out animation type
    /// 0 = Ease 1 = Animator
    /// </summary>
    public int animationType => m_behaviour ? m_behaviour.animationType : 0;
    /// <summary>
    /// Is window default hidden ?
    /// </summary>
    public bool defaultHide { get; protected set; }
    /// <summary>
    /// Do not create stack record when skip to other windows from this window
    /// </summary>
    public bool ignoreStack { get; protected set; }
    /// <summary>
    /// Is this window global ?
    /// Global windows will not destroy even we load a new level
    /// </summary>
    public bool markedGlobal { get; set; }
    /// <summary>
    /// Is this window full screen ?
    /// When show a full screen window, any other full screen will be hidden.
    /// </summary>
    public bool isFullScreen { get; set; } = true;
    /// <summary>
    /// Destroy after hiding animation ?
    /// </summary>
    public bool hideDestroy { get { return m_hideDestroy; } }
    /// <summary>
    /// Window is in show/hide animation (Showing or Hiding)
    /// </summary>
    public bool animating { get { return m_behaviour && m_behaviour.animating; } }
    /// <summary>
    /// Window is showing
    /// </summary>
    public bool showing { get { return m_behaviour && m_behaviour.showing; } }
    /// <summary>
    /// Window is hiding
    /// </summary>
    public bool hiding { get { return m_behaviour && m_behaviour.hiding; } }
    /// <summary>
    /// Get/Set current window UI input state
    /// </summary>
    public bool inputState { get { return !m_behaviour || m_behaviour.inputState; } set { if (m_behaviour) m_behaviour.inputState = value; } }
    /// <summary>
    /// Transition animation speed
    /// <para></para>
    /// Reset when window trigger a show/hide event
    /// </summary>
    public float animationSpeed { get { return !m_behaviour ? 1.0f : m_behaviour.animationSpeed; } set { if (m_behaviour) m_behaviour.animationSpeed = value; } }

    #endregion

    #region Private fields

    protected override Type behaviourType { get { return typeof(WindowBehaviour); } }

    /// <summary>
    /// Current locked window subtype
    /// <para>
    /// If m_subTypeLock is greater or equals to 0, global return callback will ignore OnReturn event and hide current window
    /// </para>
    /// See <see cref="_OnReturn"/>
    /// and <seealso cref="GotoSubWindow(string, int, Action{Window}, Action{Window})"/>
    /// </summary>
    protected int m_subTypeLock = -1;

    protected WindowBehaviour m_behaviour = null;

    protected bool m_hideDestroy = false;

    private bool m_showLock = false;
    private bool m_forwardShowHide = true;
    private List<LinkedWindowBehaviour> m_linkedWindowBehaviours = new List<LinkedWindowBehaviour>(); // Prevent GC alloc

    private Action<Window> m_onOpen = null;
    private Action<Window> m_onShow = null;

    protected InOutAction m_inAction = InOutAction.Animated, m_outAction = InOutAction.Animated;

    #endregion

    #region SceneObject

    protected sealed override void OnAddedToScene()
    {
        m_windows.Add(this);

        markedGlobal      = false;
        isFullScreen      = true;
        defaultHide       = false;
        m_hideDestroy     = false;
        m_forwardShowHide = true;
        m_subTypeLock     = -1;

        m_behaviour = m_baseBehaviour as WindowBehaviour;
        m_behaviour.window = this;
        m_behaviour.onAnimationComplete = OnAnimationComplete;

        m_inAction  = m_behaviour.animationType == 1 ? InOutAction.Immediately : InOutAction.Animated;
        m_outAction = m_behaviour.animationType == 1 ? InOutAction.OnInOut : InOutAction.Animated;

        gameObject.transform.SetParent(UIManager.instance.transform, false);

        CollectLinkedWindowBehaviours();

        Module.CollectModuleCallbacks(this);

        Window_BindWidget.BindWidget(this, transform);

        m_onOpen?.Invoke(this);
        m_onOpen = null;

        OnOpen();
    }

    protected sealed override void OnDestroy()
    {
        UIManager.instance?.DispatchEvent(Events.UI_WINDOW_ON_DESTORY, Event_.Pop(this));

        OnClose();

        moduleGlobal.RemoveReturnCallback(_OnReturn);

        m_onOpen = null;
        m_onShow = null;
        m_behaviour.window = null;

        m_linkedWindowBehaviours.Clear();

        m_windows.Remove(this);

        base.OnDestroy();

        if (current != this) return;

        if (m_stack.Count > 0)
        {
            var w = m_stack[0];
            if (w.windowName == name)
            {
                m_stack.RemoveAt(0, true);

                w = m_stack.Count > 0 ? m_stack[0] : null;

                var cached = w != null ? GetOpenedWindow(w.windowName) : null;
                if (cached)
                {
                    m_current = cached;
                    DispathWillOpenEvent(m_current.name);
                    m_current._Show(false, false);
                }
                else
                {
                    m_current = null;
                    if (w != null) _PrepareWindowInternal(w.windowName, true, false, false);
                }
            }
        }
        else m_current = null;
    }

    #endregion

    #region Window

    /// <summary>
    /// Show window
    /// </summary>
    /// <param name="immediately">Show immediately (no trasition animation) ?</param>
    public void Show(bool immediately = false)
    {
        _Show(m_current != this, immediately);
    }

    /// <summary>
    /// Hide window
    /// </summary>
    /// <param name="immediately">Hide immediately (no trasition animation) ?</param>
    /// <param name="destroy">destroy after hide</param>
    /// <param name="immediately">Hide immediately (no trasition animation) ?</param>
    public void Hide(bool immediately = false, bool destroy = false)
    {
        _Hide(false, immediately, destroy);
    }

    #endregion

    #region Virtual functions

    protected virtual void _Show(bool forward, bool immediately)
    {
        if (m_showLock)
        {
            Logger.LogError($"Window::_Show: Try to show the same window when target window is in show state. <b><color=#FFFF11>[window:{name} forward:{forward} immediately:{immediately}]</color></b>, ignored.");
            return;
        }

        m_showLock = true;

        m_forwardShowHide = forward;
        m_hideDestroy = false;

        var oldState = actived;

        _ExecuteRestoreData(!m_forwardShowHide && m_stack.Count > 0 && (m_stack[0].windowName == name) ? m_stack[0] : null);

        Logger.LogDetail($"Window: <color=#00FF00>Showing</color> window <b><color=#9CE8FF>[window:{name}, forward:{forward}, subType:{m_subTypeLock}], fullScreen:{isFullScreen}, ignoreStack:{ignoreStack}, oldState:{oldState}</color></b>!");

        DispatchEvent(Events.UI_WINDOW_SHOW);
        
        if (isFullScreen && m_forwardShowHide && m_current && m_current != this && m_inAction != InOutAction.OnInOut) m_current.DispatchEvent(Events.UI_WINDOW_HIDE, Event_.Pop(true));

        OnWillBecameVisible(oldState, m_forwardShowHide);

        moduleGlobal.RemoveReturnCallback(_OnReturn);
        moduleGlobal.AddReturnCallback(_OnReturn);
      
        m_behaviour.Show(immediately || oldState && !animating);

        if (!isFullScreen) transform.SetAsLastSibling();
        else if (m_forwardShowHide)
        {
            transform.SetAsLastSibling();

            var old = m_current;

            m_current = this;

            m_stack.RemoveAll(ww => ww.windowName == name, true);
            m_stack.Insert(0, WindowHolder.Create(name));

            if (m_inAction != InOutAction.OnInOut && old && old != m_current)
                old._Hide(m_forwardShowHide, old.animationType == 1 || m_inAction == InOutAction.Immediately, false);
        }
        DispatchEvent(Events.UI_WINDOW_VISIBLE, Event_.Pop(this));
        OnBecameVisible(oldState, m_forwardShowHide);

        ClearWindowParam(name);

        m_showLock = false;
    }

    protected virtual void _Hide(bool forward, bool immediately, bool destroy)
    {
        m_forwardShowHide = forward;

        Window cached = null;
        if (isFullScreen && !m_forwardShowHide && m_current == this)
        {
            var next = m_stack.Count > 0 && m_stack[0].windowName != name ? m_stack[0] : m_stack.Count > 1 ? m_stack[1] : null;
            if (next && !string.IsNullOrEmpty(next.windowName))
            {
                cached = GetOpenedWindow(next.windowName);
                if (!cached)
                {
                    _PrepareWindowInternal(next.windowName, false, false, false, -1, w =>
                    {
                        if (!w) return;
                        
                        _HideInternal(immediately, destroy, w);
                    });
                    return;
                }
            }
        }

        _HideInternal(immediately, destroy, cached);
    }

    /// <summary>
    /// Animation complete
    /// </summary>
    protected virtual void OnAnimationComplete(bool show)
    {
        if (show)
        {
            var s = m_onShow;
            m_onShow = null;

            if (m_inAction == InOutAction.OnInOut) HideAllWindowsBut(this, m_forwardShowHide);

            OnShow(m_forwardShowHide);

            s?.Invoke(this);
        }
        else
        {
            OnHide(m_forwardShowHide);

            if (isFullScreen && !m_forwardShowHide && m_outAction == InOutAction.OnInOut && m_current == this)
            {
                if (m_stack.Count > 0)
                {
                    var w = m_stack[0];
                    var cached = m_windows.Find(ww => ww && ww.name == w.windowName);

                    if (cached)
                    {
                        m_current = cached;
                        DispathWillOpenEvent(m_current.name);
                        m_current._Show(false, false);
                    }
                    else
                    {
                        m_current = null;
                        _PrepareWindowInternal(w.windowName, true, false, false);
                    }
                }
                else m_current = null;
            }

            if (m_hideDestroy) Destroy();
        }
    }
    /// <summary>
    /// Called when window added to scene
    /// </summary>
    protected virtual void OnOpen() { }
    /// <summary>
    /// Called when window will became visible (before fade animation start)
    /// See <paramref name="forward"/>
    /// </summary>
    /// <param name="oldState">Window is actived before ?</param>
    /// <param name="forward">Is current window popup from prev stack ?</param>
    protected virtual void OnWillBecameVisible(bool oldState, bool forward) { }
    /// <summary>
    /// Called when window became visible (before fade animation start)
    /// See <paramref name="forward"/>
    /// </summary>
    /// <param name="oldState">Window is actived before ?</param>
    /// <param name="forward">Is current window a new window or popup from stack ?</param>
    protected virtual void OnBecameVisible(bool oldState, bool forward) { }
    /// <summary>
    /// Called when window become visible (after fade animation end)
    /// <param name="forward">Is current window a new window or popup from prev stack ?</param>
    /// </summary>
    protected virtual void OnShow(bool forward) { }
    /// <summary>
    /// Called when window become invisible (after fade animation end)
    /// See <paramref name="forward"/>
    /// </summary>
    /// <param name="forward">Is next window a new window or popup from stack ?</param>
    protected virtual void OnHide(bool forward) { }
    /// <summary>
    /// Called when window received an return event
    /// Default window will call Hide() function
    /// </summary>
    protected virtual void OnReturn() { Hide(); }
    /// <summary>
    /// Called when window closed (OnDestroy)
    /// </summary>
    protected virtual void OnClose() { }
    /// <summary>
    /// Restore data when window pop back
    /// </summary>
    /// <param name="holder"></param>
    protected virtual void ExecuteRestoreData(WindowHolder holder) { }
    /// <summary>
    /// Create restore data when skip to other windows
    /// </summary>
    /// <param name="holder"></param>
    protected virtual void GrabRestoreData(WindowHolder holder) { }

    #endregion

    #region Internal actions

    private void _HideInternal(bool immediately, bool destroy, Window cached)
    {
        if (!this) return;

        Logger.LogDetail($"Window: <color=#FF0000>Hiding</color> window <b><color=#9CE8FF>[window:{name}, forward:{m_forwardShowHide}, subType:{m_subTypeLock}], fullScreen:{isFullScreen}, ignoreStack:{ignoreStack}, next:{cached?.name}]</color></b>!");

        var popBack = isFullScreen && !m_forwardShowHide;  // Back to a stack window from this
        var popUp   = isFullScreen && m_forwardShowHide;   // Open a new window from this

        var w = popBack && m_stack.Count > 0 && m_stack[0].windowName == name ? m_stack[0] : popUp && m_stack.Count > 1 && m_stack[1].windowName == name ? m_stack[1] : null;

        if (w != null && (popBack || popUp && ignoreStack))
            m_stack.Remove(w, true);

        DispatchEvent(Events.UI_WINDOW_HIDE, Event_.Pop(false));

        moduleGlobal.RemoveReturnCallback(_OnReturn);

        if (popUp && w) _GrabRestoreData(w);

        m_subTypeLock = -1;

        m_behaviour.Hide(immediately);

        if (popBack && m_outAction != InOutAction.OnInOut && m_current == this)
        {
            m_current = cached;
            if (m_current)
            {
                DispathWillOpenEvent(m_current.name);
                m_current._Show(false, false);
            }
        }

        if (gameObject.activeSelf) m_hideDestroy = destroy;
        else if (destroy) Destroy();
    }

    private void _OnReturn()
    {
        if (m_subTypeLock > -1)
        {
            m_subTypeLock = -1;

            Hide();
        }
        else OnReturn();
    }

    private void _ExecuteRestoreData(WindowHolder holder)
    {
        if (holder) m_subTypeLock = holder.subTypeLock;
        ExecuteRestoreData(holder ?? WindowHolder.empty);
    }

    private void _GrabRestoreData(WindowHolder holder)
    {
        holder.subTypeLock = m_subTypeLock;
        GrabRestoreData(holder);
    }

    private void CollectLinkedWindowBehaviours()
    {
        m_linkedWindowBehaviours.Clear();

        transform.GetComponentsInChildren(true, m_linkedWindowBehaviours);
        foreach (var behaviour in m_linkedWindowBehaviours) behaviour.Initialize(this);
    }

    #endregion

    #region IRenderObject

    /// <summary>
    /// Called every render frame (Behaviour.Update)
    /// </summary>
    public virtual void OnRenderUpdate() { }
    /// <summary>
    /// Called after every render frame (Behaviour.LateUpdate)
    /// </summary>
    public virtual void OnPostRenderUpdate() { }

    #endregion
}