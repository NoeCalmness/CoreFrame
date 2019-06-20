// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  *             任务系统
//  * 
//  * Author:     T.Moon
//  * Version:    0.1
//  * Created:    2018-12-20      13:45
//  ***************************************************************************************************/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Module_Task : Module<Module_Task>
{
    /// <summary>
    /// 任务开始消息
    /// </summary>
    public const string EventTaskBeginMessgage = "EventTaskBeginMessgage";
    /// <summary>
    /// 任务完成通知消息
    /// </summary>
    public const string EventTaskFinishMessage = "EventTaskFinishMessage";
    /// <summary>
    /// 任务进度更新消息
    /// </summary>
    public const string EventTaskNotificationProgess = "EventTaskNotificationProgess";
    /// <summary>
    /// 送礼任务
    /// </summary>
    public const string EventTaskGiftMessage = "EventTaskGiftMessage";
    /// <summary>
    /// 选择答案任务
    /// </summary>
    public const string EventTaskChooseAnswerMessage = "EventTaskChooseAnswerMessage";

    List<Task> m_taskList = new List<Task>();

    private bool IsExistTask(int id)
    {
        return m_taskList.Find((p) => p.taskID == id) != null ? true : false;
    }
  
    private void CreateTask(PMission info)
    {
        Task task = new Task(info.missionId);
        if (!task.InitTaskData(info.conditions))
            return;
        //同一任务是否可以接多次？
        if (!IsExistTask(info.missionId))
        {
            m_taskList.Add(task);
            task.TaskBegin();
        }
    }
    private void CreateTask(int id)
    {
        Task task = new Task((byte)id);
        if (!task.InitTaskData())
            return;
        if(!IsExistTask(id))
        {
            m_taskList.Add(task);
            task.TaskBegin();
        }
    }
    public Task GetTask(byte id)
    {
        return m_taskList.Find((p) => p.taskID == id);
    }
    /// <summary>
    /// 通过参数NPCID 获得所有与该NPC的任务
    /// </summary>
    /// <param name="npctypeid"></param>
    /// <returns></returns>
    public List<Task> GetTask(NpcTypeID npctypeid)
    {
        return m_taskList.FindAll((p)=>p.taskTargetID==npctypeid);
    }
    /// <summary>
    /// 所有的约会任务
    /// </summary>
    /// <returns></returns>
    public List<Task>GetTasks(EnumTaskType tasktype)
    {
        return m_taskList.FindAll((p) => p.taskType == tasktype);
    }
    void UpdateTaskConditionState(PMission info)
    {
        Task task = m_taskList.Find(((p) => p.taskID == info.missionId));
        if (task != null)
        {
            for (byte i = 0; i < info.conditions.Length; i++)
            {
                task.UpdateCondition(i, (EnumTaskCondition)(info.conditions[i].conditionId), info.conditions[i].progress);
            }
        }
        else
        {
            Logger.Log(LogType.ERROR, "Dont Find Task By ID :{0}", info.missionId);
        }
        
    }
    protected override void OnModuleCreated()
    {
        base.OnModuleCreated();
    }
    /// <summary>
    /// 任务系统状态更新
    /// </summary>
    /// <param name="diff"></param>
    public override void OnUpdate(int diff)
    {
        for (int i = 0; i < m_taskList.Count; i++)
        {
            if (m_taskList[i].taskState != EnumTaskState.Finish)
                m_taskList[i].UpdateState();
        }
    }
    /// <summary>
    /// 掉线后 重新登陆后需要请求服务器是否存在任务
    /// </summary>
    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        enableUpdate = true;
        m_taskList.Clear();
        SendTaskInfo();
    }
    #region 网络消息模块
    /// <summary>
    /// 这里只有在断线了，再上线时，服务器发送任务ID
    /// </summary>
    private void SendTaskInfo()
    {
        CsNpcMission p = PacketObject.Create<CsNpcMission>();
        session.Send(p);
    }
    /// <summary>
    /// 发送请求创建任务 通过任务ID
    /// </summary>
    /// <param name="id"></param>
    public void SendAccputTask(byte id)
    {
        CsNpcAcceptMission p = PacketObject.Create<CsNpcAcceptMission>();
        p.missionId = id;
        session.Send(p);
    }
    /// <summary>
    /// 发送请求 通知服务器任务流程已完成 通过ID进行标识
    /// </summary>
    /// <param name="id"></param>
    public void SendTaskFinish(byte id)
    {
        CsNpcCompleteMission p = PacketObject.Create<CsNpcCompleteMission>();
        p.missionId = id;
        session.Send(p);
    }
    /// <summary>
    /// 接受服务器下发接受任务消息 创建任务实例
    /// </summary>
    /// <param name="p"></param>
    void _Packet(ScNpcAcceptMission p)
    {
        if (p != null&&p.missionId > 0)
        {
            CreateTask(p.missionId);
        }
    }
    /// <summary>
    /// 服务器下发任务进行重新创建任务实例
    /// </summary>
    /// <param name="p"></param>
    void _Packet(ScNpcMission p)
    {
        if (p != null && p.missions != null && p.missions.Length > 0)
        {
            for (int i = 0; i < p.missions.Length; i++)
            {
                PMission info = null;
                p.missions[i].CopyTo(ref info);
                if (info != null)
                {
                    CreateTask(info);
                }
            }
        }
    }
/// <summary>
/// 接受任务最新进度变化
/// </summary>
/// <param name="p"></param>
    void _Packet(ScNpcMissionChange p)
    {
        if (p != null && p.missions != null )
        {
            UpdateTaskConditionState(p.missions);
        }
    }
    /// <summary>
    /// 任务完成信息进行同步
    /// </summary>
    /// <param name="p"></param>
    void _Packet(ScNpcCompleteMission p)
    {
        if (p != null && p.missionId > 0)
        {
            Task tk = m_taskList.Find((s) => s.taskID == p.missionId);
            //标记任务完成,通过状态去标记
            if (p.result == 0)
            {
                tk.UpdateState("Finish");
            }
        }
    }
    #endregion
}