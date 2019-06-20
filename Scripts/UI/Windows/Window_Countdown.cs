/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-19
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

public class Window_Countdown : Window
{
    private Button m_confirmBtn;
    private Button m_exitBtn;
    private Text m_restTimeText;
    private string m_noActivityStr;

    protected override void OnOpen()
    {
        isFullScreen = false;
        m_confirmBtn    = GetComponent<Button>("panel/confirm_btn");
        m_exitBtn       = GetComponent<Button>("panel/button");
        m_restTimeText  = GetComponent<Text>("panel/time");
        m_noActivityStr = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 1);

        EventTriggerListener.Get(m_confirmBtn.gameObject).onClick   = OnConfirmClick;
        EventTriggerListener.Get(m_exitBtn.gameObject).onClick      = OnConfirmClick;
        IniteText();
    }

    private void IniteText()
    {
        GetComponent<Text>("panel/equipinfo").text          = ConfigText.GetDefalutString(TextForMatType.PublicUIText, 25);
        GetComponent<Text>("panel/confirm_btn/Text").text   = ConfigText.GetDefalutString(TextForMatType.PublicUIText, 4);
        GetComponent<Text>("panel/tip").text                = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 34);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        long interVal = moduleLabyrinth.openTimeInterval;
        if(interVal < 0) m_restTimeText.text = m_noActivityStr;
        else if(interVal > 0 )RefreshRestTimeSpan(moduleLabyrinth.currentLabyrinthStep);
    }

    private void OnConfirmClick(GameObject sender)
    {
        Hide(true);
    }

    private void RefreshRestTimeSpan(EnumLabyrinthTimeStep step)
    {
        //打开面板直到倒计时结束，则关闭面板显示
        if (step == EnumLabyrinthTimeStep.Chanllenge || step == EnumLabyrinthTimeStep.Rest) OnConfirmClick(null);

        if (step != EnumLabyrinthTimeStep.Ready) return;

        Util.SetText(m_restTimeText, moduleLabyrinth.GetTimeString(moduleLabyrinth.currentStepRestTimespan));
    }

    void _ME(ModuleEvent<Module_Labyrinth> e)
    {
        if (e.moduleEvent == Module_Labyrinth.EventLabyrinthTimeRefresh) RefreshRestTimeSpan((EnumLabyrinthTimeStep)e.param1);
    }
}
