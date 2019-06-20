// /**************************************************************************************************
//  * Copyright (C) 2017-2018 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-04-30      9:40
//  *LastModify：2019-04-30      9:40
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class Window_FactionSign : Window
{
    private Button m_excuteButton;
    private Text m_excuteText;
    private Text m_countDownText;
    private Transform m_previewRewardNode;

    protected override void OnOpen()
    {
        base.OnOpen();
        InitComponents();

        m_excuteButton?.onClick.AddListener(() =>
        {
            moduleFactionBattle.RequestSignIn(true);
        });
    }

    private void InitComponents()
    {
        m_excuteButton      = GetComponent<Button>("right/Button");
        m_excuteText        = GetComponent<Text>("right/Button/Text");
        m_previewRewardNode = GetComponent<Transform>("reward_panel");
        m_countDownText     = GetComponent<Text>("right/countDown");

        GetComponent<Transform>("right")?.GetComponentDefault<FactionRuleBehaviour>();
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        ignoreStack = false;
        moduleGlobal.ShowGlobalLayerDefault();
        base.OnBecameVisible(oldState, forward);
        RefreshButtonState();
        m_previewRewardNode.GetComponentDefault<PreviewRewards>();
        AutoSkip();
    }

    public override void OnRenderUpdate()
    {
        base.OnRenderUpdate();
        switch (moduleFactionBattle.state)
        {
            case Module_FactionBattle.State.Sign:
                Util.SetText(m_countDownText, Util.Format(ConfigText.GetDefalutString(558, 6), moduleFactionBattle.SignCountDown));
                break;
            case Module_FactionBattle.State.Readly:
                Util.SetText(m_countDownText, Util.Format(ConfigText.GetDefalutString(558, 7), moduleFactionBattle.ReadlyCountDown));
                break;
            default:
                Util.SetText(m_countDownText, string.Empty);
                break;
        }
    }

    private void _ME(ModuleEvent<Module_FactionBattle> e)
    {
        if (!actived)
            return;

        switch (e.moduleEvent)
        {
            case Module_FactionBattle.EventStateChange:
            case Module_FactionBattle.EventSignStateChange:
                RefreshButtonState();
                AutoSkip();
                break;
        }
    }

    private void AutoSkip()
    {
        if (moduleFactionBattle.IsProcessing)
        {
            ignoreStack = true;
            Window.ShowAsync<Window_FactionBattle>();
        }
    }

    private void RefreshButtonState()
    {
        Util.SetText(m_excuteText, ConfigText.GetDefalutString(TextForMatType.FactionSignUI, 
            moduleFactionBattle.state == Module_FactionBattle.State.Close 
            ? 7 : moduleFactionBattle.state == Module_FactionBattle.State.End 
                  ? 8 : moduleFactionBattle.IsSignIn 
                        ? 1 : moduleFactionBattle.IsSignState 
                              ? 0 : 2));
        m_excuteButton.SetInteractable(moduleFactionBattle.IsSignState && !moduleFactionBattle.IsSignIn);
    }
}
