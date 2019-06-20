/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Loading module
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-15
 * 
 ***************************************************************************************************/

public class Module_Loading : Module<Module_Loading>
{
    /// <summary>
    /// 设置当前加载窗口的提示文本
    /// param1 = 加载信息 null 表示使用默认加载信息
    /// </summary>
    public const string EventSetLoadingInfo = "EventChangeLoadingText";

    /// <summary>
    /// Default true, set to false after first level load complete
    /// </summary>
    public bool firstLoading { get; private set; } = true;

    private Window_DefaultLoading m_defaultLoadingWindow = null;
    private Window m_curLoadingWindow = null;

    private int m_delayEventID = -1;

    protected override void OnModuleCreated()
    {
        m_defaultLoadingWindow = Window.Open<Window_DefaultLoading>(UIManager.instance.window_defaultLoading);
        m_defaultLoadingWindow.markedGlobal = true;
        m_defaultLoadingWindow.isFullScreen = false;

        EventManager.AddEventListener(Events.SCENE_LOAD_START,    OnSceneLoadStart);
        EventManager.AddEventListener(Events.SCENE_LOAD_COMPLETE, OnSceneLoadComplete);
        EventManager.AddEventListener(Events.SHOW_LOADING_WINDOW, OnOtherLoadingState);
    }

    private void OnSceneLoadStart(Event_ e)
    {
        var l = e.sender as Level;
        ShowLoadingWindow(l.loadingWindow, l.loadingWindowMode);
    }

    private void OnSceneLoadComplete()
    {
        DelayEvents.Remove(m_delayEventID);

        firstLoading = false;
        m_curLoadingWindow?.Hide(false, !m_curLoadingWindow.Equals(m_defaultLoadingWindow));
    }

    private void OnOtherLoadingState(Event_ e)
    {
        var show = e.param1 == null || (bool)e.param1;
        var window = e.param2 as string;
        var delay = e.param3 == null ? 0 : (float)e.param3;
        var mode = e.param4 == null ? 0 : (int)e.param4;

        DelayEvents.Remove(m_delayEventID);

        if (!show) m_curLoadingWindow?.Hide(false, !m_curLoadingWindow.Equals(m_defaultLoadingWindow));
        else
        {
            if (delay <= 0f) ShowLoadingWindow(window, mode);
            else
            {
                m_delayEventID = DelayEvents.Add(() =>
                {
                    ShowLoadingWindow(window, mode);
                }, delay);
            }
        }
    }

    private void ShowLoadingWindow(string n, int mode)
    {
        if (string.IsNullOrEmpty(n)) n = Window_DefaultLoading.defaultName;

        DelayEvents.Remove(m_delayEventID);
        Window.SetWindowParam(n, mode);

        var o = m_curLoadingWindow;
        if (string.IsNullOrEmpty(n) || n == Window_DefaultLoading.defaultName)
        {
            m_curLoadingWindow = m_defaultLoadingWindow;
            m_defaultLoadingWindow.Show(true);
        }
        else
        {
            Window.ShowImmediatelyAsync(n, w =>
            {
                m_curLoadingWindow = w;
                if (!m_curLoadingWindow)
                {
                    m_curLoadingWindow = m_defaultLoadingWindow;
                    m_curLoadingWindow.Show(true);
                }
                else
                {
                    m_curLoadingWindow.markedGlobal = true;
                    m_curLoadingWindow.isFullScreen = false;
                }
            });
        }

        if (o && !o.Equals(m_curLoadingWindow)) o.Hide(true, o.hideDestroy);
    }

    public void SetLoadingInfo(string info)
    {
        DispatchModuleEvent(EventSetLoadingInfo, info);
    }

    public void SetLoadingInfo(int id, int index = 0)
    {
        SetLoadingInfo(Util.GetString(id, index));
    }
}
