/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Combat Ultimate Spell UI Mask
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-07-04
 * 
 ***************************************************************************************************/

using UnityEngine;

public class Window_UltimateMask : Window
{
    private float m_hideDelay = -1;

    protected override void OnOpen()
    {
        isFullScreen = false;
        inputState = false;

        EventManager.AddEventListener(Events.CAMERA_SHOT_STATE,    OnCameraShotState);
        EventManager.AddEventListener(Events.CAMERA_SHOT_UI_STATE, OnCameraShotUIState);

        defaultHide = true;

        m_hideDelay = -1;
    }

    private void OnCameraShotState(Event_ e)
    {
        if (!actived) return;

        var shotEnabled = (bool)e.param1;
        if (!shotEnabled)
        {
            m_hideDelay = -1;
            Hide();
        }
    }

    private void OnCameraShotUIState(Event_ e)
    {
        var asset = (string)e.param2;
        var show = !string.IsNullOrEmpty(asset);
        var duration = (float)e.param3;

        if (show && duration > 0)
        {
            UIDynamicImage.LoadImage(GetComponent<RectTransform>("Image"), asset);

            m_hideDelay = duration;
            Show();
        }
    }

    public override void OnRenderUpdate()
    {
        if (m_hideDelay > 0 && (m_hideDelay -= Time.smoothDeltaTime) <= 0)
        {
            m_hideDelay = -1;
            Hide();
        }
    }
}
