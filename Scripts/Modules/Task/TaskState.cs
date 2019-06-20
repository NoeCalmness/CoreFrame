using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum EnumTaskState
{
    Begin,
    Runing,
    PreFinish,
    Finish,
    None
}

public class TaskStateManger
{
    /// <summary>
    /// 利用字典存储各种状态
    /// </summary>
    Dictionary<string, TaskState> taskStateList = new Dictionary<string, TaskState>();
    /// <summary>
    /// 当前状态
    /// </summary>
    TaskState m_curState;
    /// <summary>
    /// 注册状态
    /// </summary>
    /// <param name="statename"></param>
    /// <param name="state"></param>
    public void Region(string statename, TaskState state)
    {
        if (!taskStateList.ContainsKey(statename))
        {
            taskStateList.Add(statename, state);
        }
    }

    public TaskState curTaskState
    {
        get
        {
            return m_curState;
        }
    }
    /// <summary>
    /// 设置默认状态
    /// </summary>
    /// <param name="statename"></param>
    public void SetDefault(string statename)
    {
        if (taskStateList.ContainsKey(statename))
        {
            m_curState = taskStateList[statename];
            m_curState.EnterState();
        }
    }

    /// <summary>
    /// 改变状态
    /// </summary>
    /// <param name="statename"></param>
    public void ChangeState(string statename)
    {
        if (taskStateList.ContainsKey(statename))
        {
            if (m_curState != null)
            {
                m_curState.ExitState();
                m_curState = taskStateList[statename];
                m_curState.EnterState();
            }
        }
    }
    /// <summary>
    /// 更新状态
    /// </summary>
    public void UpdateState()
    {
        if (m_curState != null)
        {
            m_curState.UpdateState();
        }
    }

}
/// <summary>
/// 任务状态基类
/// </summary>
public abstract class TaskState
{
    /// <summary>
    /// 状态控制机
    /// </summary>
    protected TaskStateManger m_taskSM;
    /// <summary>
    /// 任务
    /// </summary>
    protected Task m_task;

    protected EnumTaskState m_taskState;
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sm"></param>
    public TaskState(TaskStateManger sm,Task tk)
    {
        m_taskSM = sm;
        m_task = tk;
    }
    /// <summary>
    /// 进入状态方法
    /// </summary>
    public abstract void EnterState();
    /// <summary>
    /// 离开状态方法
    /// </summary>
    public abstract void ExitState();
    /// <summary>
    /// 更新状态方法
    /// </summary>
    public abstract void UpdateState();
    /// <summary>
    /// 当前处于状态机的那个状态
    /// </summary>
    public EnumTaskState CurState
    {
        get { return m_taskState; }
    }
}


public class BeginTaskState:TaskState
{
    public BeginTaskState(TaskStateManger sm,Task tk) : base(sm,tk)
    {
        m_taskState = EnumTaskState.Begin;
    }
    public override void EnterState()
    {
        //任务接成功了
#if GUIDE_LOG
        Logger.LogInfo("NPCID:{0}约会任务ID:{1}当前状态:{2}", (int)m_task.taskTargetID, m_task.taskID, m_taskState);
#endif
        Module_Guide.AddCondition(new NpcDatingCondition(m_task.taskID, false));
        Module_Guide.CheckTriggerGuide();
        Module_Task.instance.DispatchEvent(Module_Task.EventTaskBeginMessgage, Event_.Pop(m_task.taskID, m_task));

        m_taskSM.ChangeState("Run");
    }

    public override void ExitState()
    {
      
    }

    public override void UpdateState()
    {
       
    }

}


public class RunningTaskState:TaskState
{
    public RunningTaskState(TaskStateManger sm,Task tk):base(sm,tk)
    {
        m_taskState = EnumTaskState.Runing;
    }

    public override void EnterState()
    {
       
    }

    public override void ExitState()
    {
       
    }

    public override void UpdateState()
    {
        if (m_task.Execute())
            m_taskSM.ChangeState("PreFinish");
        if (m_task.taskNotificationUI)
        {
            Module_Task.instance.DispatchEvent(Module_Task.EventTaskNotificationProgess, Event_.Pop(m_task.taskID));
            m_task.taskNotificationUI = false;
        }
    }
}

public class PreFinishTaskState:TaskState
{
    private int count = 0;
    public PreFinishTaskState(TaskStateManger sm, Task tk) :base(sm,tk)
    {
        m_taskState = EnumTaskState.PreFinish;
    }
    public override void EnterState()
    {
        //发送完成消息
        Module_Task.instance.SendTaskFinish(m_task.taskID);
    }

    public override void ExitState()
    {
        count = 0;
    }

    public override void UpdateState()
    {
        count++;
        if (count > 1000)
            m_taskSM.ChangeState("Finish");
    }
}
/// <summary>
/// 任务结束状态
/// </summary>
public class FinishTaskState: TaskState
{
    public FinishTaskState(TaskStateManger sm, Task tk) : base(sm, tk)
    {
        m_taskState = EnumTaskState.Finish;
    }
    public override void EnterState()
    {
        //服务器标记该任务真实完成 
        Module_Task.instance.DispatchEvent(Module_Task.EventTaskFinishMessage, Event_.Pop(m_task.taskID,m_task));
#if GUIDE_LOG
        Logger.LogInfo("约会任务ID:{0}当前状态:{1}", m_task.taskID, m_taskState);
#endif
        Module_Guide.RemoveCondition(new NpcDatingCondition(m_task.taskID, false));
        Module_Guide.AddCondition(new NpcDatingCondition(m_task.taskID, true));
    }

    public override void ExitState()
    {
    }

    public override void UpdateState()
    {
    }
}