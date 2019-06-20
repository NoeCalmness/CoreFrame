// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-01      16:27
//  * LastModify：2018-08-02      10:39
//  ***************************************************************************************************/

#region

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class Window_Awaketeam : Window
{
    private float                   countDown;
    private List<Transform>         frames;
    private List<Transform>         specialFrames;
    private readonly List<Tuple<TaskInfo, AwakeTaskItem>> stageItemList = new List<Tuple<TaskInfo, AwakeTaskItem>>();

    private Transform               templete;
    private Transform               specialTemplete;
    private ChaseTask               currentTask;
    
    protected override void OnOpen()
    {
        InitComponent();
    }

    private void InitComponent()
    {
        var normalPos       = GetComponent<Transform>("init_panel/content/normalPosition");
        var specialPos      = GetComponent<Transform>("init_panel/content/specialPosition");
        templete            = GetComponent<Transform>("init_panel/content/normalPosition/awakeNoramlIcon_Img");
        specialTemplete     = GetComponent<Transform>("init_panel/content/specialPosition/awakeSpecialIcon_Img");

        frames              = normalPos.GetChildList();
        specialFrames       = specialPos.GetChildList();
        frames       .RemoveAll(t => !t.name.Contains("frame"));
        specialFrames.RemoveAll(t => !t.name.Contains("frame"));

        templete        .SafeSetActive(false);
        specialTemplete .SafeSetActive(false);
    }

    protected override void OnShow(bool forward)
    {
        CreateNormalTask();
        CreateSpecialTask();

        if (Window_TeamMatch.IsChooseStage)
        {
            foreach (var t in stageItemList)
            {
                t.Item2.RefreshFlag();
            }
        }
    }

    protected override void OnHide(bool forward)
    {
        for (var i = 0; i < specialFrames.Count; i++)
            Util.ClearChildren(specialFrames[i]);

        for (var i = 0; i < frames.Count; i++)
            Util.ClearChildren(frames[i]);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault(1, false);
    }

    protected override void OnClose()
    {
        base.OnClose();
        moduleAwakeMatch.matchInfos = null;
        //moduleAwakeMatch.StageId = 0;
    }

    private void CreateSpecialTask()
    {
        if (moduleAwake == null)
            return;

        for (var i = 0; i < moduleAwakeMatch.CanEnterActiveList.Count && i < specialFrames.Count; i++)
        {
            Util.ClearChildren(specialFrames[i]);
            var task = moduleAwakeMatch.CanEnterActiveList[i];
            var go = specialFrames[i].AddNewChild(specialTemplete);
            var item = go.GetComponentDefault<AwakeTaskItem>();
            item.RefreshTaskItem(true, task, OnItemClick);
            go.SafeSetActive(true);
            stageItemList.Add(Tuple.Create(task.taskConfigInfo, item));
        }
    }

    private void CreateNormalTask()
    {
        if (moduleAwakeMatch.DependTaskInfoList == null) return;
        for (var i = 0; i < moduleAwakeMatch.DependTaskInfoList.Count && i < frames.Count; i++)
        {
            Util.ClearChildren(frames[i]);
            var task = moduleAwakeMatch.DependTaskInfoList[i];
            var go = frames[i].AddNewChild(templete);
            var item = go.GetComponentDefault<AwakeTaskItem>();
            var chaseTask = moduleAwakeMatch.CanEnterDependList.Find(t => t.taskConfigInfo.ID == task.Item1.ID);
            if (chaseTask != null)
            {
                item.RefreshTaskItem(false, chaseTask, OnItemClick);
                stageItemList.Add(Tuple.Create(task.Item1, item));
            }
            else
                item.RefreshTaskItem(task.Item1);
            go.SetGray(chaseTask == null);
            go.SafeSetActive(true);
        }
    }

    private void OnItemClick(ChaseTask task)
    {
        if (Window_TeamMatch.IsChooseStage)
        {
            bool isSend = moduleAwakeMatch.Request_SwitchStage(task.taskConfigInfo.ID);
            if (!isSend) ShowAsync<Window_TeamMatch>();
        }
        else
        {
            currentTask = task;
            if (currentTask.taskConfigInfo.teamType == TeamType.Single)
            {
                SetWindowParam<Window_Assist>(currentTask);
                ShowAsync<Window_Assist>();
            }
            else
            {
                SetWindowParam<Window_CreateRoom>(0, currentTask);
                ShowAsync<Window_CreateRoom>();
            }
        }
    }


    protected override void OnReturn()
    {
        if (Window_TeamMatch.IsChooseStage)
        {
            ShowAsync<Window_TeamMatch>();
            return;
        }

        if (moduleAwakeMatch.CurrentTask != null)
            moduleAwakeMatch.Request_CancelMatch();

        base.OnReturn();
    }

    private void _ME(ModuleEvent<Module_AwakeMatch> e)
    {
        if (!actived) return;
        switch (e.moduleEvent)
        {
            case Module_AwakeMatch.Response_SwitchStage:
                ResponseSwitchStage(e.msg as ScTeamPveSwitchStage);
                break;
        }
    }

    private void ResponseSwitchStage(ScTeamPveSwitchStage msg)
    {
        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9807, msg.result);
            return;
        }
        if (Window_TeamMatch.IsChooseStage)
            ShowAsync<Window_TeamMatch>();
    }

    public void SetCurrentTask(ChaseTask task)
    {
        currentTask = task;
        SetWindowParam<Window_CreateRoom>(0, currentTask);
        ShowAsync<Window_CreateRoom>();
    }
}
