// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-09-26      15:00
//  * LastModify：2018-09-26      15:00
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class Window_TeamMatch : Window
{
    public static bool IsChooseStage;

    private Transform teamPanel;
    private AwakeWindow_Team teamWindow;

    protected override void OnOpen()
    {
        ignoreStack = true; // 不允许通过返回按钮返回到当前窗口

        InitComponent();
        MultiLangrage();

        teamWindow = SubWindowBase.CreateSubWindow<AwakeWindow_Team>(this, teamPanel?.gameObject);
        teamWindow.Set(false);
    }

    protected override void OnClose()
    {
        base.OnClose();
        teamWindow?.Destroy();

        if (moduleAwakeMatch.enterGame)
            return;
        if (moduleAwakeMatch.StageId != 0)
        {
            moduleAwakeMatch.Request_ExitRoom(modulePlayer.id_);
            moduleAwakeMatch.ClearMatchInfo();
        }
    }

    private void InitComponent()
    {
        teamPanel = GetComponent<Transform>("team_panel");
    }

    private void MultiLangrage()
    {
        var t = ConfigManager.Get<ConfigText>((int)TextForMatType.AwakeStage);
        if (!t) return;
        Util.SetText(GetComponent<Text>("init_panel/difficultyBox/txt"), t[19]);
        Util.SetText(GetComponent<Text>("team_panel/starReward_Panel/starReward_Txt"), t[20]);
        Util.SetText(GetComponent<Text>("team_panel/starReward_Panel/starReward02_Txt"), t[21]);
        Util.SetText(GetComponent<Text>("team_panel/remainchallengeCount/remainTime_Txt"), t[22]);
        Util.SetText(GetComponent<Text>("team_panel/preaward_Panel/preaward_Txt"), t[23]);
        Util.SetText(GetComponent<Text>("team_panel/captainFrame_Img/captainFrameTitle_Img/captainFrameTitle_Txt"), t[24]);
        Util.SetText(GetComponent<Text>("team_panel/memberFrame_Img/ready_Img/ready_Txt"), t[25]);
        Util.SetText(GetComponent<Text>("team_panel/invite_Panel/inviteMember_Txt"), t[26]);
        Util.SetText(GetComponent<Text>("team_panel/ready_Btn/start_text"), t[27]);
        Util.SetText(GetComponent<Text>("team_panel/cancel_Btn/start_text"), t[28]);
        Util.SetText(GetComponent<Text>("team_panel/open_Toggle/opencheck_Txt"), t[29]);
        Util.SetText(GetComponent<Text>("invite_panel/person_frame/titleBg/title_Txt"), t[37]);
        Util.SetText(GetComponent<Text>("invite_panel/person_frame/invite_btn/Image"), t[38]);
        Util.SetText(GetComponent<Text>("tip/detail/content"), t[39]);
        Util.SetText(GetComponent<Text>("tip/detail/equipinfo"), t[40]);
        Util.SetText(GetComponent<Text>("tip/detail/cancel_btn/Text"), t[41]);
        Util.SetText(GetComponent<Text>("tip/detail/confirm_btn/Text"), t[42]);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleAwakeMatch.enterGame = false;

        if (!teamWindow.Initialize())
        {
            Window_Alert.ShowAlertDefalut(Util.Format(ConfigText.GetDefalutString(68001), moduleAwakeMatch.StageId), ()=> { Hide(); }, null, string.Empty, string.Empty, false);
            return;
        }

        moduleGlobal.ShowGlobalLayerDefault(1, false);
    }

    protected override void OnHide(bool forward)
    {
        if (!forward) return;

        //通过跳转窗口切出去，需要先清本地数据再通知服务器。不然退回上一层会因为有组队信息继续进组队界面
        if (!IsChooseStage && moduleAwakeMatch.StageId != 0)
        {
            moduleAwakeMatch.Request_ExitRoom(modulePlayer.id_);
            moduleAwakeMatch.ClearMatchInfo();
        }
    }

    protected override void OnReturn()
    {
        teamWindow.OnReturn();
    }
}
