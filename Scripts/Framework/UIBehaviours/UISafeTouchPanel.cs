/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * UI Safe Touch Panel
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-05-21
 * 
 ***************************************************************************************************/

using UnityEngine;

[AddComponentMenu("HYLR/UI/Safe Touch Panel")]
public class UISafeTouchPanel : MonoBehaviour
{
    public bool left   = true;
    public bool right  = false;
    public bool top    = false;
    public bool bottom = false;

    private RectTransform m_rc;

    public void Start()
    {
        #if !UNITY_EDITOR
        if (!Root.hasNotch) return;
        #endif

        m_rc = this.rectTransform();
        if (!m_rc) return;

        EventManager.AddEventListener(Events.APPLICATION_ORIENTATION, CheckNotchScreen);
        EventManager.AddEventListener(Events.VEDIO_SETTINGS_CHANGED,  CheckNotchScreen);

        CheckNotchScreen();
    }

    private void OnDestroy()
    {
        EventManager.RemoveEventListener(this);
    }

    private void CheckNotchScreen()
    {
        var li = Root.leftSafeInset;
        var ri = Root.rightSafeInset;

        var min = m_rc.anchorMin;
        var max = m_rc.anchorMax;

        if (left)  min.x = li;
        if (right) max.x = 1.0f - ri;

        m_rc.anchorMin = min;
        m_rc.anchorMax = max;
    }
}
