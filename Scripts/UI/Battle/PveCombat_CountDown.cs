// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-03-27      10:17
//  *LastModify：2019-03-27      10:17
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimeEndEvent : UnityEvent<PveCombat_CountDown.Mode>
{
    
}

public class ToggleValueChangeEvent : UnityEvent<bool> { }

public class PveCombat_CountDown : SubWindowBase
{
    public enum Mode
    {
        WatchCountDown,
        ReviveCountDown,
    }

    public TimeEndEvent onTimeEnd = new TimeEndEvent();

    public ToggleValueChangeEvent onToggleValueChange = new ToggleValueChangeEvent();

    /// <summary>
    /// 组队倒计时检测15秒
    /// </summary>
    private const double ALIVE_TIME = 15;
    /// <summary>
    /// 组队观战倒计时检测
    /// </summary>
    private const float WATCH_TIME = 5;

    private Text m_timeText;
    private Toggle m_watchToggle;
    private Text m_countDownText, m_watchCountText;
    private Transform m_timeRoot;

    private Mode m_mode;
    private double m_timer;

    public void SetToggle(bool isOn)
    {
        if (m_watchToggle)
            m_watchToggle.isOn = isOn;
    }

    protected override void InitComponent()
    {
        base.InitComponent();
        m_timeRoot          = WindowCache.GetComponent<Transform>("count_tip/time");
        m_timeText          = WindowCache.GetComponent<Text>("count_tip/time/Text");
        m_countDownText     = WindowCache.GetComponent<Text>("count_tip/time/countdown_Txt");
        m_watchCountText    = WindowCache.GetComponent<Text>("count_tip/time/closeCountDown_Txt");
        m_watchToggle       = WindowCache.GetComponent<Toggle>("count_tip/watch_battle");
    }

    public override void MultiLanguage()
    {
        base.MultiLanguage();

        var ct = ConfigManager.Get<ConfigText>((int)TextForMatType.CombatUIText);

        Util.SetText(WindowCache.GetComponent<Text>("count_tip/time/countdown_Txt"), ct[12]);
        Util.SetText(WindowCache.GetComponent<Text>("count_tip/time/closeCountDown_Txt"), ct[13]);
        Util.SetText(WindowCache.GetComponent<Text>("count_tip/watch_battle/revive"), ct[14]);
        Util.SetText(WindowCache.GetComponent<Text>("count_tip/watch_battle/Text"), ct[15]);
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
        {
            SwitchMode((Mode)p[0]);
            return false;
        }

        SwitchMode((Mode)p[0]);

        m_watchToggle.SafeSetActive(!modulePVE.isStandalonePVE);
        m_watchToggle.onValueChanged.AddListener(OnToggleValueChange) ;
        return true;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;

        enableUpdate = false;
        return true;
    }

    private void OnToggleValueChange(bool isOn)
    {
        onToggleValueChange?.Invoke(isOn);

        if (m_mode == Mode.WatchCountDown)
        {
            if(!isOn)
                SwitchMode(m_mode);
        }
        m_timeRoot.SafeSetActive(!isOn);
    }

    public void DisableTimer()
    {
        enableUpdate = false;
        m_timeRoot.SafeSetActive(false);
    }

    public void SwitchMode(Mode rMode)
    {
        m_mode = rMode;

        bool isWatch = !modulePVE.isStandalonePVE && m_mode == Mode.WatchCountDown;
        {
            m_watchCountText.SafeSetActive(isWatch);
            m_countDownText.SafeSetActive(!isWatch);
        }

        if (rMode == Mode.ReviveCountDown)
            m_timer = ALIVE_TIME;
        else
            m_timer = WATCH_TIME;
        enableUpdate = true;
        m_timeRoot.SafeSetActive(true);
    }

    public override void OnUpdate(int diff)
    {
        if (m_timer <= 0)
            return;

        if (m_timer > 0)
            m_timer -= diff*0.001;

        Util.SetText(m_timeText, Mathd.CeilToInt(m_timer).ToString());
        if (m_timer <= 0)
        {
            m_timeRoot.SafeSetActive(false);
            onTimeEnd?.Invoke(m_mode);
        }
    }
}
