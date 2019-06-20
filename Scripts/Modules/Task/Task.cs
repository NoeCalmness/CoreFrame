using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public class Task
{
   /// <summary>
   /// 任务ID
   /// </summary>
    private byte m_taskID;
    /// <summary>
    /// 任务状态管理器
    /// </summary>
    private TaskStateManger m_taskSM;
    /// <summary>
    /// 任务条件列表
    /// </summary>
    private List<TaskCondition> m_taskCondition = new List<TaskCondition>();
    /// <summary>
    /// 默认为所有条件都完成，任务才能完成
    /// </summary>
    public EnumTaskConditionType taskConditionType = EnumTaskConditionType.FinishAll; 
    /// <summary>
    /// 该任务对应目标NPCID 为零时表示当前约会对象
    /// </summary>
    public NpcTypeID taskTargetID { set; get; }
    public bool taskFinished { set; get; }
    public byte taskID { get { return m_taskID; } }
    public EnumTaskType taskType { set; get; }
    public EnumTaskState taskState
    {
        get
        {
            return m_taskSM.curTaskState.CurState;
        }
    }
    public int taskNameID { set; get; }
    public string taskIconID { set; get; }
    /// <summary>点击这个任务是显示的图片</summary>
    public string taskPressIcon { set; get; }
    public int taskDescID { set; get; }
    public int[] taskBelongScene { set; get; } //作为场景标注当前场景是否有任务信息
    /// <summary>
    /// 当前任务完成进度值（0/1）
    /// </summary>
    public int taskCurProgess
    {
        get
        {
            int count = 0;
            for(int i = 0;i< m_taskCondition.Count;i++)
            {
                if (m_taskCondition[i].FinishedCondition)
                    count++;
            }
            return count;
        }
    }
    public int taskMaxProgess
    {
        get
        {
            return m_taskCondition.Count;
        }
    }
    /// <summary>
    /// 通知界面是否需要更新数据
    /// </summary>
    public bool taskNotificationUI { set; get; }
    public Task(byte id)
    {
        m_taskID = id;
    }

    public bool InitTaskData(PCondition[] condition = null)
    {
        TaskConfig taskconfig = ConfigManager.Get<TaskConfig>(m_taskID);
        if (taskconfig == null)
        {
            Logger.LogError("The taskid:{0} configuration information does not exist on the client.", m_taskID);
            return false;
        }
        this.taskConditionType = taskconfig.taskFinishType;
        this.taskFinished = taskconfig.taskFinishType == EnumTaskConditionType.FinishAll ? true : false;
        this.taskTargetID = (NpcTypeID)taskconfig.targetID;
        this.taskType = taskconfig.taskType;
        this.taskNameID = taskconfig.taskNameID;
        this.taskIconID = taskconfig.taskIconID;
        this.taskPressIcon = taskconfig.taskPressIcon;
        this.taskDescID = taskconfig.taskDescID;
        this.taskBelongScene = taskconfig.markSceneID;
        for (int m = 0; m < taskconfig.taskFinishConditions.Length; m++)
        {
            taskconfig.taskFinishConditions[m].progress = 0;
            this.AddCondition(taskconfig.taskFinishConditions[m]);
        }
        if (condition != null)
        {
            if (!SetConditionValue(condition))
            {
                Logger.LogError("The taskid:{0}  The client configuration Condition Not Equal Server Condition.", m_taskID);
                return false;
            }
               
        }
        m_taskSM = new TaskStateManger();
        m_taskSM.Region("Begin", new BeginTaskState(m_taskSM, this));
        m_taskSM.Region("Run", new RunningTaskState(m_taskSM, this));
        m_taskSM.Region("PreFinish", new PreFinishTaskState(m_taskSM, this));
        m_taskSM.Region("Finish", new FinishTaskState(m_taskSM, this));
        return true;
    }
 
    private bool SetConditionValue(PCondition[] condition)
    {
        if (m_taskCondition.Count != condition.Length)
            return false;
        for (int i = 0; i < condition.Length; i++)
        {
            if ((EnumTaskCondition)condition[i].conditionId == m_taskCondition[i].m_taskCondition)
                m_taskCondition[i].m_curValue = condition[i].progress;
        }
        return true;
    }
    public void UpdateState()
    {
        m_taskSM.UpdateState();
    }
    /// <summary>
    /// 强制设置任务状态
    /// </summary>
    /// <param name="taskstate"></param>
    public void  UpdateState(string state)
    {
        m_taskSM.ChangeState(state);
    }
    private void AddCondition(TaskConfig.TaskFinishCondition condition)
    {
        TaskCondition cond = null;
        switch (condition.condition)
        {
            case EnumTaskCondition.MoodValue:
                {
                    IFactroyCondition factroy = new MoodFactoryCondition();
                    cond = factroy.CreateCondition(condition.value);
                }
                break;
            case EnumTaskCondition.EnergyValue:
                {
                    IFactroyCondition factroy = new EnergFactoryCondition();
                    cond = factroy.CreateCondition(condition.value);
                }
                break;
            case EnumTaskCondition.FinishAllEvent:
                {
                    IFactroyCondition factroy = new FinishAllEventFactroyCondition();
                    cond = factroy.CreateCondition(condition.value);
                }break;
            case EnumTaskCondition.ConsumeGold:
                {
                    IFactroyCondition factroy = new ConsumeGoldConditionFactroy();
                    cond = factroy.CreateCondition(condition.value);
                }
                break;
            case EnumTaskCondition.ChooseGoodAnswer:
                {
                    IFactroyCondition factroy = new ChooseGoodAnswerFactroy();
                    cond = factroy.CreateCondition(condition.value);
                }
                break;
            case EnumTaskCondition.Gift:
                {
                    IFactroyCondition factroy = new GiftFactroy();
                    cond = factroy.CreateCondition(condition.value);
                }
                break;
            case EnumTaskCondition.DateScore:
                {
                    IFactroyCondition factroy = new DateScoreFactroy();
                    cond = factroy.CreateCondition(condition.value);
                }
                break;
            case EnumTaskCondition.FinishSceneEvent:
                {
                    IFactroyCondition factroy = new FinishSceneEventFactroy();
                    cond = factroy.CreateCondition(condition.value);
                }
                break;
            case EnumTaskCondition.None:
                {
                    IFactroyCondition factroy = new NoneConditionFactory();
                    cond = factroy.CreateCondition(condition.value);
                }
                break;
            default:
                break;
        }
        if(cond!=null)
        {
            cond.m_targetID = taskTargetID;
            cond.m_taskCondition = condition.condition;
            cond.m_curValue = condition.progress;
        }
        m_taskCondition.Add(cond);
    }
    /// <summary>
    /// 更新进度值
    /// </summary>
    /// <param name="index"></param>
    /// <param name="condition"></param>
    /// <param name="value"></param>
    public void UpdateCondition(byte index,EnumTaskCondition condition,uint value)
    {
        if(this.m_taskCondition.Count<index)
        {
            Logger.Log(LogType.ERROR, "Mission conditions are out of bounds :{0}", index);
            return;
        }
        if (this.m_taskCondition[index].m_taskCondition == condition)
        {
            this.m_taskCondition[index].m_curValue = value;
            this.taskNotificationUI = true;
        }
    }
    /// <summary>
    /// 执行任务条件判断
    /// </summary>
    public bool Execute()
    {
        bool taskRet = taskFinished;
        for (int i = 0; i < m_taskCondition.Count; i++)
        {
            if (taskConditionType == EnumTaskConditionType.FinishAll)
            {
                taskRet &= m_taskCondition[i].Execute();
            }
            else
            {
                taskRet |= m_taskCondition[i].Execute();
            }
        }
        return taskRet;
    }
    public void TaskBegin()
    {
        m_taskSM.SetDefault("Begin");
    }
   
}