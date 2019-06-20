/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-01-08
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class Window_CreateRoom : Window
{
    private Transform taskConfirmPanel;
    private AwakeWindow_TaskComfirm taskConfirmWindow;

    private Transform matchPanel;
    private AwakeWindow_Match matchWindow;

    private ChaseTask currentTask;

    protected override void OnOpen()
    {
        isFullScreen = false;

        taskConfirmPanel = GetComponent<Transform>("task_confirm_panel");
        matchPanel = GetComponent<Transform>("waiting_panel");

        taskConfirmWindow = SubWindowBase.CreateSubWindow<AwakeWindow_TaskComfirm, Window>(this, taskConfirmPanel?.gameObject);
        matchWindow = SubWindowBase.CreateSubWindow<AwakeWindow_Match>(this, matchPanel?.gameObject);

        MultiLangrage();
    }

    private void MultiLangrage()
    {
        var t = ConfigManager.Get<ConfigText>((int)TextForMatType.AwakeStage);
        if (!t) return;
        Util.SetText(GetComponent<Text>("task_confirm_panel/kuang/bg/missioninfo"), t[30]);
        Util.SetText(GetComponent<Text>("task_confirm_panel/kuang/starDesc/tittle/Text"), t[31]);
        Util.SetText(GetComponent<Text>("task_confirm_panel/kuang/Text"), t[32]);
        Util.SetText(GetComponent<Text>("task_confirm_panel/kuang/awake/join_Btn/join_Txt"), t[33]);
        Util.SetText(GetComponent<Text>("task_confirm_panel/kuang/awake/create_Btn/create_Txt"), t[34]);
        Util.SetText(GetComponent<Text>("waiting_panel/back/Text"), t[35]);
        Util.SetText(GetComponent<Text>("waiting_panel/ditu/wating_Txt"), t[36]);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        var args = GetWindowParam(name);
        if (args == null) return;

        var type = (int)args.param1;
        currentTask = args.param2 as ChaseTask;

        if (type == 0)
        {
            if (taskConfirmWindow)
            {
                taskConfirmWindow.UnInitialize();
                taskConfirmWindow.Initialize(currentTask);
            }
        }
        else
        {
            taskConfirmWindow.UnInitialize();
            if (matchWindow)
                matchWindow.UnInitialize();
        }
    }

    protected override void OnHide(bool forward)
    {
        if (taskConfirmWindow) taskConfirmWindow.UnInitialize();
        if (matchWindow) matchWindow.UnInitialize();
    }

    private void _ME(ModuleEvent<Module_AwakeMatch> e)
    {
        if (!actived) return;
        switch (e.moduleEvent)
        {
            case Module_AwakeMatch.Notice_MatchSuccess:
                OnMatchSuccess(e.msg as ScTeamPveMatchSuccess);
                break;
            case Module_AwakeMatch.Response_StartMatch:
                ResponseStartMatch(e.msg as ScTeamPveStartMatch);
                break;
            case Module_AwakeMatch.Response_CancelMatch:
                ResponseCancelMatch(e.msg as ScTeamPveCancelMatch);
                break;
            default:break;
        }
    }

    private void OnMatchSuccess(ScTeamPveMatchSuccess msg)
    {
        if (msg.result == 3)
        {
            Window_Alert.ShowAlertDefalut(ConfigText.GetDefalutString((int)TextForMatType.AwakeStage, 10),
            () => { moduleAwakeMatch.Request_EnterRoom(false, currentTask); },
            ()=>Hide());
            return;
        }

        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9806, msg.result);
            return;
        }
        Hide();

        ShowAsync<Window_TeamMatch>();
    }

    private void ResponseStartMatch(ScTeamPveStartMatch msg)
    {
        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9805, msg.result);
            if (!taskConfirmWindow.isInit)
                Hide();
            return;
        }

        if (taskConfirmWindow) taskConfirmWindow.UnInitialize();

        matchWindow.Initialize(msg.countDown + Time.realtimeSinceStartup);
    }

    private void ResponseCancelMatch(ScTeamPveCancelMatch msg)
    {
        Hide();

        if (msg.result != 0)
            moduleGlobal.ShowMessage(9804, msg.result);
    }
}
