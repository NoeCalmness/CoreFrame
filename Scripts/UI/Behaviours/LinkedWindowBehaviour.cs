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

using UnityEngine;

public class LinkedWindowBehaviour : MonoBehaviour
{
    public bool initialized { get { return m_initialized; } }
    public Window linkedWindow { get { return m_linkedWindow; } }

    protected bool m_initialized = false;
    protected Window m_linkedWindow = null;

    public void Initialize(Window window = null)
    {
        if (m_initialized) return;

        m_linkedWindow = window ?? DetectLinkedWindow();
        if (!m_linkedWindow) return;

        m_initialized = true;

        m_linkedWindow.AddEventListener(Events.UI_WINDOW_SHOW, OnWindowShow);
        m_linkedWindow.AddEventListener(Events.UI_WINDOW_HIDE, OnWindowHide);

        SettingsManager.instance?.AddEventListener(Events.VEDIO_SETTINGS_CHANGED, OnVedioSettingsChanged);

        OnInitialize();
    }

    private Window DetectLinkedWindow()
    {
        var p = transform;
        WindowBehaviour wb = null;
        while (p && !(wb = p.GetComponent<WindowBehaviour>()) && (p = p.parent)) ;

        return wb?.window ?? null;
    }

    protected virtual void Awake() { Initialize(); }
    protected virtual void OnInitialize() { }
    protected virtual void OnWindowShow() { }
    protected virtual void OnWindowHide() { }
    protected virtual void OnVedioSettingsChanged() { }

    private void OnDestroy()
    {
        m_linkedWindow = null;
        EventManager.RemoveEventListener(this);
    }
}
