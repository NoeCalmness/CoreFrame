// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-10      9:46
//  * LastModify：2018-08-10      9:50
//  ***************************************************************************************************/

#region

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class AwakeTaskItem : MonoBehaviour
{
    public enum TaskState
    {
        NoTouch,
        CanChallenge,
        Pass,

        ActiveChallenge,
        ActiveNoTouch,
    }
    public Action<ChaseTask> OnClick;
    public ChaseTask Data;
    public TaskInfo taskInfo;

    private bool inited = false;
    private Button itemButton;
    private Transform Flag;
    private Dictionary<TaskState, Transform> stateMap = new Dictionary<TaskState, Transform>();
    private Tuple<Transform, Text>[] states = new Tuple<Transform, Text>[3];
    private void InitComponent()
    {
        if (inited) return;
        itemButton = GetComponent<Button>();
        var t = transform.GetComponent<Transform>("1");
        if (t)
        {
            stateMap.Add(TaskState.NoTouch, t);
            stateMap.Add(TaskState.ActiveNoTouch, t);
            states[0] = new Tuple<Transform, Text>(t, t.GetComponent<Text>("Name_Txt"));
        }

        t = transform.GetComponent<Transform>("2");
        if (t)
        {
            stateMap.Add(TaskState.CanChallenge, t);
            stateMap.Add(TaskState.ActiveChallenge, t);
            states[1] = new Tuple<Transform, Text>(t, t.GetComponent<Text>("Name_Txt"));
        }

        t = transform.GetComponent<Transform>("3");
        if (t)
        {
            stateMap.Add(TaskState.Pass, t);
            states[2] = new Tuple<Transform, Text>(t, t.GetComponent<Text>("Name_Txt"));
        }

        Flag = transform.GetComponent<Transform>("Flag");
    }

    public void RefreshTaskItem(bool isActiveTask, ChaseTask chaseTask, Action<ChaseTask> onItemClick)
    {
        InitComponent();
        SetName(chaseTask.taskConfigInfo);
        OnClick = onItemClick;
        Data = chaseTask;
        taskInfo = chaseTask?.taskConfigInfo;
        BaseRestrain.SetRestrainData(gameObject, taskInfo ? taskInfo.ID : 0);

        itemButton.onClick.RemoveAllListeners();
        itemButton.onClick.AddListener(() =>
        {
            OnClick?.Invoke(Data);
        });

        if (isActiveTask && Module_AwakeMatch.instance.TaskInfoDict.ContainsKey(chaseTask.taskConfigInfo.ID))
        {
            SetState(chaseTask.taskData.state == 3 ? TaskState.ActiveNoTouch : TaskState.ActiveChallenge);
        }
        else
        {
            if (chaseTask.taskData == null)
                SetState(TaskState.NoTouch);
            else if (chaseTask.taskData.state == 3)
                SetState(TaskState.Pass);
            else
                SetState(TaskState.CanChallenge);
        }
        RefreshFlag();
    }

    public void RefreshFlag()
    {
        if (Data != null)
            Flag.SafeSetActive(Data.taskConfigInfo.ID == Module_AwakeMatch.instance.StageId);
        else
            Flag.SafeSetActive(false);
    }

    private void SetName(TaskInfo rInfo)
    {
        for (int i = 0; i < states.Length; i++)
        {
            var t = states[i];
            if (t != null && t.Item2) Util.SetText(t.Item2, rInfo.name);
        }
    }

    public void RefreshTaskItem(TaskInfo info)
    {
        taskInfo = info;
        BaseRestrain.SetRestrainData(gameObject, taskInfo ? taskInfo.ID : 0);
        InitComponent();
        SetName(info);
        SetState(TaskState.NoTouch);
        RefreshFlag();

        itemButton.onClick.RemoveAllListeners();
        itemButton.onClick.AddListener(() =>
        {
            string str = string.Empty;
            for (var i = 0; i < taskInfo.dependId.Length; i++)
            {
                var task = ConfigManager.Get<TaskInfo>(taskInfo.dependId[i]);
                if (null == task) continue;
                if (!string.IsNullOrEmpty(str))
                    str += ",";
                str += task.name;
            }
            Module_Global.instance.ShowMessage(Util.Format(ConfigText.GetDefalutString(TextForMatType.AwakeStage, 45), str));
        });
    }

    private void SetState(TaskState rState)
    {
        for (int i = 0; i < states.Length; i++)
        {
            var t = states[i];
            if (t != null && t.Item1) t.Item1.SafeSetActive(false);
        }

        if (stateMap.ContainsKey(rState))
            stateMap[rState].SafeSetActive(true);

        if (rState == TaskState.NoTouch)
        {
            states[0].Item2.SafeSetActive(taskInfo.maxChanllengeTimes != 1);
        }
    }
}