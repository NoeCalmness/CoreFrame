// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-11-02      13:31
//  *LastModify：2018-11-06      15:51
//  ***************************************************************************************************/
#region

using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
#endregion

public abstract class SubWindowBase : Window_BindWidget
{
    public    bool       isInit;
    public    GameObject Root;
    protected Window     WindowCache;
    protected Transform  parent;
    protected string     assetName;
    public    SubWindowBehaviour behaviour;

    private bool autoUninitalize;
    private bool         isInitComponent;
    private object[]     param;

    public void Set(Window rWindow, GameObject rRoot)
    {
        BindWidget(this, rWindow?.transform);
        Root = rRoot;
        Root.SafeSetActive(false);
        WindowCache = rWindow;

        UIManager.instance.AddEventListener(Events.UI_WINDOW_ON_DESTORY, OnWindowDestory);
    }

    public void Set(Window rWindow, Transform rParent, string rAssetName)
    {
        BindWidget(this, rWindow?.transform);

        Root = null;
        parent = rParent;
        assetName = rAssetName;
        WindowCache = rWindow;
        UIManager.instance.AddEventListener(Events.UI_WINDOW_ON_DESTORY, OnWindowDestory);
    }

    public void Set(bool rAutoUninitalize)
    {
        autoUninitalize = rAutoUninitalize;
        if (behaviour)
            behaviour.Set(autoUninitalize);
    }

    protected sealed override void OnInitialized()
    {
        base.OnInitialized();
        isInit = false;
    }

    private void OnWindowDestory(Event_ e)
    {
        if (e.param1 != null && e.param1.Equals(WindowCache))
        {
            CollectModuleCallbacks(true);
        }
    }


    private void CollectModuleCallbacks(bool revert = false)
    {
        var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        foreach (var method in methods)
        {
            if (method.Name != "_ME") continue;

            var ps = method.GetParameters();
            if (ps.Length != 1 || !ps[0].ParameterType.IsSubclassOf(typeof(ModuleEvent)) || !ps[0].ParameterType.IsGenericType)
            {
                Logger.LogWarning("Window::CollectModuleCallbacks: ModuleEventHandler has invalid parameters: [window: {0}, method: {1}, paramCount: {2}, paramType: {3}]", name, method.Name, ps.Length, ps.Length > 0 ? ps[0].ParameterType.Name : "");
                continue;
            }

            var handler = Delegate.CreateDelegate(typeof(ModuleHandler<>).MakeGenericType(ps[0].ParameterType.GetGenericArguments()[0]), this, method, false);
            if(revert)
                EventManager.RemoveModuleListener(handler);
            else
                EventManager.AddModuleListener(ModuleEvent.GLOBAL, handler);
        }
    }

    public virtual void MultiLanguage() { }

    protected virtual void InitComponent() { }

    protected virtual void RefreshData(params object[] p) { }

    protected void InitComponentInternal()
    {
        if (isInitComponent)
            return;

        isInitComponent = true;
        InitComponent();
        MultiLanguage();

        behaviour = Root?.GetComponentDefault<SubWindowBehaviour>();
        autoUninitalize = true;
        behaviour?.Set(this, autoUninitalize);
    }

    public virtual bool OnReturn()
    {
        return UnInitialize();
    }

    public virtual bool Initialize(params object[] p)
    {
        if (isInit)
        {
            RefreshData(p);
            return false;
        }
        SetEnable(true);
        CollectModuleCallbacks();
        isInit = true;
        RefreshData(p);
        return true;
    }

    public void Initialize_Async(params object[] p)
    {
        if (this.Root != null)
        {
            Initialize(p);
            return;
        }
        param = p;
        Level.PrepareAsset<GameObject>(assetName, OnLoadComplete);
    }

    private void OnLoadComplete(GameObject go)
    {
        if (!go)
        {
            Logger.LogError("there is no asset name as " + assetName);
            return;
        }
        Root = Object.Instantiate(go);
        Root.transform.SetParent(parent);
        Root.transform.localPosition = Vector3.zero;
        Root.transform.localScale = Vector3.one;
        Root.rectTransform().sizeDelta = Vector2.zero;
        InitComponentInternal();
        Initialize(param);
    }

    public virtual bool UnInitialize( bool hide = true)
    {
        if (!isInit) return false;
        CollectModuleCallbacks(true);
        isInit = false;
        if (hide) SetEnable(false);
        return true;
    }

    public void SetEnable(bool rEnable)
    {
        if (!Root) return;
        Root.SetActive(rEnable);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UIManager.instance.RemoveEventListener(Events.UI_WINDOW_ON_DESTORY);
        UnInitialize();
        isInitComponent = false;
    }

    public static T CreateSubWindow<T>(Window rWindow, GameObject rRoot) where T : SubWindowBase
    {
        var w = _Create<T>();
        w.Set(rWindow, rRoot);
        w.InitComponentInternal();
        return w;
    }
    public static T CreateSubWindow<T>(Window rWindow, Transform rRoot) where T : SubWindowBase
    {
        var w = _Create<T>();
        w.Set(rWindow, rRoot?.gameObject);
        w.InitComponentInternal();
        return w;
    }

    public static T CreateSubWindow<T, W>(W rWindow, GameObject rRoot) 
        where W : Window 
        where T : SubWindowBase<W>
    {
        var w = _Create<T>();
        w.Set(rWindow, rRoot);
        w.parentWindow = rWindow;
        w.InitComponentInternal();
        return w;
    }

    public static T CreateSubWindow<T>(Window rWindow, Transform rParent, string assetName) where T : SubWindowBase
    {
        var w = _Create<T>();
        w.Set(rWindow, rParent, assetName);
        return w;
    }

    public static T CreateSubWindow<T, W>(W rWindow, Transform rParent, string assetName)
        where W : Window
        where T : SubWindowBase<W>
    {
        var w = _Create<T>();
        w.Set(rWindow, rParent, assetName);
        w.parentWindow = rWindow;
        return w;
    }
}

public abstract class SubWindowBase<T> : SubWindowBase where T : Window
{
    public T parentWindow;

    public static W CreateSubWindow<W>(T rWindow, GameObject rRoot) where W : SubWindowBase<T>
    {
        var w = _Create<W>();
        w.Set(rWindow, rRoot);
        w.parentWindow = rWindow;
        w.InitComponentInternal();
        return w;
    }

    public static W CreateSubWindow<W>(T rWindow, Transform rParent, string assetName) where W : SubWindowBase<T>
    {

        var w = _Create<W>();
        w.Set(rWindow, rParent, assetName);
        w.parentWindow = rWindow;
        return w;
    }
}
