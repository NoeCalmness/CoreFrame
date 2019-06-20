/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-01-04
 * 
 ***************************************************************************************************/
using System;
using System.Collections.Generic;


public class Module_NpcGaiden : Module<Module_NpcGaiden>
{
    public const string EventRefreshGaidenTask = "EventRefreshGaidenTask";
    public const string EventRefreshTargetTask = "EventRefreshTargetTask";

    public Dictionary<GaidenTaskInfo, bool> gaidenInfoDic { get; private set; } = new Dictionary<GaidenTaskInfo, bool>();
    public Dictionary<TaskType, List<ChaseTask>> gaidenTaskDic { get; private set; } = new Dictionary<TaskType, List<ChaseTask>>();
    public TaskType skipType { get; set; } = TaskType.Count;
    public ChaseTask cacheResetTask { get; private set; }

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        InitAllGaidenTask();
        SendGaidenTaskInfo();
    }

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();

        gaidenInfoDic.Clear();
        gaidenTaskDic.Clear();
        skipType = TaskType.Count;
    }

    private void InitAllGaidenTask()
    {
        var allGaidenInfos = ConfigManager.GetAll<GaidenTaskInfo>();
        var allTasks = ConfigManager.GetAll<TaskInfo>();

        gaidenTaskDic.Clear();
        foreach (var item in allGaidenInfos)
        {
            //init info
            if (!gaidenInfoDic.ContainsKey(item)) gaidenInfoDic.Add(item,false);

            //init task list
            if (!gaidenTaskDic.ContainsKey(item.taskType)) gaidenTaskDic.Add(item.taskType, new List<ChaseTask>());
            var ts = allTasks.FindAll(t => t.level == (int)item.taskType);
            if (ts != null && ts.Count > 0)
            {
                foreach (var task in ts)
                {
                    gaidenTaskDic[item.taskType].Add(ChaseTask.Create(task));
                }
            }
        }
    }

    public GaidenTaskInfo GetGaidenInfo(TaskType type)
    {
        foreach (var item in gaidenInfoDic)
        {
            if (item.Key.taskType == type) return item.Key;
        }

        return null;
    }

    public GaidenTaskInfo GetGaidenInfo(int gaidenId)
    {
        foreach (var item in gaidenInfoDic)
        {
            if (item.Key.ID == gaidenId) return item.Key;
        }

        return null;
    }

    public GaidenTaskInfo GetGaidenInfo(ChaseTask task)
    {
        if (task == null)
        {
            Logger.LogError("cannot load GaidenTaskInfo,because chasetask is null");
            return null;
        }

        return GetGaidenInfo(task.taskType);
    }

    public Module_Npc.NpcMessage GetNpcFromTask(ChaseTask task)
    {
        var gaiden = GetGaidenInfo(task);
        if (gaiden == null) return null;

        return moduleNpc.GetTargetNpc((NpcTypeID)gaiden.ID);
    }

    public Module_Npc.NpcMessage GetNpcFromTaskType(TaskType type)
    {
        var gaiden = GetGaidenInfo(type);
        if(gaiden == null)
        {
            Logger.LogError("cannot find valid gaiden info from tasktype {0}", type);
            return null;
        }

        return moduleNpc.GetTargetNpc((NpcTypeID)gaiden.ID);
    }

    public List<ChaseTask> GetGaidenTasks(TaskType type)
    {
        var gaiden = GetGaidenInfo(type);
        if (gaiden == null || !gaidenInfoDic.ContainsKey(gaiden) || !gaidenInfoDic[gaiden]) return null;

        return gaidenTaskDic.Get(type);
    }

    public List<ChaseTask> GetGaidenTasks(GaidenTaskInfo gaiden)
    {
        if (gaiden == null || !gaidenInfoDic.ContainsKey(gaiden) || !gaidenInfoDic[gaiden]) return null;

        return gaidenTaskDic.Get(gaiden.taskType);
    }

    public List<ChaseTask> GetGaidenTasks()
    {
        List<ChaseTask> list = new List<ChaseTask>();
        foreach (var kv in gaidenTaskDic)
        {
            list.AddRange(kv.Value);
        }
        return list;
    }

    public ChaseTask GetGaidenTask(int taskId)
    {
        var taskInfo = ConfigManager.Get<TaskInfo>(taskId);
        var type = taskInfo ? (TaskType)taskInfo.level : TaskType.Count;
        if (!gaidenTaskDic.ContainsKey(type)) return null;

        var list = gaidenTaskDic.Get(type);
        return list.Find(t => t.taskData != null && t.taskData.taskId == taskId);
    }
    
    #region msg

    public void SendGaidenTaskInfo()
    {
        var p = PacketObject.Create<CsNpcTaskInfo>();
        session.Send(p);
    }

    void _Packet(ScNpcTaskInfo p)
    {
        if(p.npcCopyInfo != null)
        {
            PNpcCopy[] gaidens = null;
            p.npcCopyInfo.CopyTo(ref gaidens);
            foreach (var item in gaidens)
            {
                HandleGaidenInfo(item.npcId);
                HandleTasks(item.stageInfo);
            }
            DispatchModuleEvent(EventRefreshGaidenTask);
        }
    }

    private void HandleGaidenInfo(int gaideId)
    {
        var info = GetGaidenInfo(gaideId);
        if (info && gaidenInfoDic.ContainsKey(info)) gaidenInfoDic[info] = true;
    }

    private void HandleTasks(PChaseTask[] tasks)
    {
        for (var i = 0; i < tasks.Length; i++)
        {
            var t = ConfigManager.Get<TaskInfo>(tasks[i].taskId);
            if (t == null) continue;
            var type = moduleChase.GetCurrentTaskType(t);

            //handle task list
            if (t.isGaidenTask)
            {
                if (!gaidenTaskDic.ContainsKey(type)) gaidenTaskDic.Add(type, new List<ChaseTask>());
                var task = gaidenTaskDic[type].Find<ChaseTask>(item => item.taskConfigInfo.ID == t.ID);
                if (task == null)
                {
                    Logger.LogError($"GaidenTaskInfo 里没有配置Level = {t.level}的关卡");
                    continue;
                }
                task.taskData = tasks[i];
            }

            //handle info
            var info = GetGaidenInfo(type);
            if (info && gaidenInfoDic.ContainsKey(info)) gaidenInfoDic[info] = true;
        }
    }
    
    void _Packet(ScChaseTaskUnlock p)
    {
        PChaseTask[] tasks = null;
        p.chaseList.CopyTo(ref tasks);

        HandleTasks(tasks);
        DispatchModuleEvent(EventRefreshGaidenTask);
    }

    public void SendResetChallenge(ChaseTask task)
    {
        if (task?.taskData == null) return;

        cacheResetTask = task;
        CsChaseResetOverTimes p = PacketObject.Create<CsChaseResetOverTimes>();
        p.taskId = cacheResetTask.taskData.taskId;
        session.Send(p);
    }

    public void ResetTimesSuccess(int chaseId)
    {
        var task = GetGaidenTask(chaseId);
        if (task != null)
        {
            task.taskData.resetTimes++;
        }
    }

    void _Packet(ScChaseStateChange p)
    {
        ChaseTask targetTask = GetGaidenTask(p.taskId);

        if (targetTask == null) return;
         
        //replace task data
        targetTask.taskData.state = p.state;
        DispatchModuleEvent(EventRefreshTargetTask, targetTask);
    }

    #endregion
}
