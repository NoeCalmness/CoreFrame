/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Lee
 * Version:  0.1
 * Created:  2017-07-21
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ChaseTask : PoolObject<ChaseTask>
{
    #region static functions
    public static ChaseTask Create(PChaseTask taskData, TaskInfo taskConfigInfo = null)
    {
        var task = Create();

        if (taskData != null) task.taskData = taskData.Clone();
        task.taskConfigInfo = taskConfigInfo;

        if (!task.taskConfigInfo && taskData != null) task.taskConfigInfo = ConfigManager.Get<TaskInfo>(taskData.taskId);
        if (!task.taskConfigInfo) Logger.LogError("未找到关卡配置{0}", taskData == null ? "pchasetask data is null" : taskData.taskId.ToString());
        else
            task.activeTaskInfo = ConfigManager.Find<ActiveTaskInfo>(item => item.taskLv == task.taskConfigInfo.level);

        return task;
    }

    public static ChaseTask Create(TaskInfo taskConfigInfo)
    {
        if (!taskConfigInfo)
        {
            Logger.LogError("非法关卡配置,创建 ChaseTask 失败");
            return null;
        }
        return Create(null,taskConfigInfo);
    }
    #endregion

    private ChaseTask() { }

    public PChaseTask taskData;
    public TaskInfo taskConfigInfo;
    public ActiveTaskInfo activeTaskInfo;
    public StageInfo stageInfo
    {
        get
        {
            return ConfigManager.Get<StageInfo>(taskConfigInfo.stageId);
        }
    }

    public bool CanEnter { get { return taskData != null && taskData.state != 3; } }

    public TaskType taskType
    {
        get
        {
            return taskConfigInfo ? Module_Chase.instance.GetCurrentTaskType(taskConfigInfo) : TaskType.Count;
        }
    }

    public bool isFirst { get; set; }

    public int star { get { return taskData == null ? 0 : taskData.GetStarCount(); } }

    public int stageId { get { return taskConfigInfo == null ? 0 : taskConfigInfo.stageId; } }

    public int enterTimes { get { return taskData == null ? 0 : (int)taskData.times; } }

    public int canEnterTimes
    {
        get
        {
            if (activeTaskInfo != null)
                return activeTaskInfo.crossLimit - Module_Chase.instance.activeChallengeCount[activeTaskInfo.taskLv];
            
            if (!taskConfigInfo || null == taskData) return 0;

            if (taskConfigInfo.isGaidenTask && taskConfigInfo.maxChanllengeTimes > 0)
                return taskConfigInfo.maxChanllengeTimes - (int)taskData.successTimes;

            var resetTime = Mathf.Max(0, taskData.resetTimes);
            if (taskType == TaskType.Difficult || taskType == TaskType.Nightmare || taskConfigInfo.isGaidenTask)
                return taskConfigInfo.challengeCount + resetTime * taskConfigInfo.challengeCount - taskData.dailyOverTimes;
            if (taskType == TaskType.Awake)
            {
                if(Module_AwakeMatch.dependTask?.taskLv == taskConfigInfo.level)
                    return taskConfigInfo.challengeCount + resetTime * taskConfigInfo.challengeCount - taskData.dailyOverTimes;
                return taskConfigInfo.challengeCount + resetTime - taskData.dailyOverTimes;
            }
            return 0;
        }
    }

    public int EnterTimeMax
    {
        get
        {
            if (activeTaskInfo != null)
            {
                return activeTaskInfo.crossLimit;
            }
            return taskConfigInfo.challengeCount;
        }
    }

    public bool CanResetTimes
    {
        get
        {
            if (activeTaskInfo != null)
                return false;
            return canEnterTimes == 0;
        }
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        taskData = null;
        taskConfigInfo = null;
    }

    public bool IsStarActive(int index)
    {
        if (taskData == null) return false;
        return taskData.IsStarActive(index);
    }

    public bool CanSweep()
    {
        switch (stageInfo.rush)
        {
            case 0:
                return false;
            case 1:
                return taskData.times > 0;
            case 2:
                return star == 3;
        }
        return false;
    }
}

[Serializable]
public class Chase_PageItem
{
    public List<TaskInfo> current_Tasks = new List<TaskInfo>();

    public int current_Index { get; private set; }

    public int current_Level { get; private set; }

    public Chase_PageItem() { }

    public Chase_PageItem(List<TaskInfo> tasks, int index, int level)
    {
        current_Tasks.Clear();
        current_Tasks = tasks;
        current_Index = index;
        current_Level = level;
    }
}

public class Module_Chase : Module<Module_Chase>
{
    #region Event
    public const string EventSuccessToPVE              = "EventSuccessToPVE";
    public const string EventIsSpecial                 = "EventIsSpecial";//特殊关卡
    public const string EventRefreshChaseTask          = "EventRefreshChaseTask";//刷新关卡
    public const string EventRefreshRewardPanel        = "EventRefreshRewardPanel";//刷新领取奖励的界面
    public const string EventGetChaseInfo              = "EventGetChaseInfo";//获得数据后刷新
    public const string EventRefreshMainActivepanel    = "EventRefreshMainActivepanel";//刷新主界面活动显示
    public const string EventRefreshActiveAddAndRemove = "EventRefreshActiveAddAndRemove";
    public const string EventOpenStarRewardPanel       = "EventOpenStarRewardPanel";//打开星级奖励界面
    public const string EventSkiptargetChaseTask       = "EventSkiptargetChaseTask";//跳转到目标chasetask
    public const string ResponseMoppingUp              = "ResponseMoppingUp";//扫荡回调
    public const string EventResetTimesSuccess         = "EventChaseResetTimesSuccess";//重置副本次数成功
    #endregion

    public const int PAGENUMBER = 6;
    private bool isFirstSpecailTask;
    private bool isReceiveActive;
    private float m_recvChaseInfoTime;
    private StringBuilder tip = new StringBuilder();

    #region property
    public ScChaseInfo chaseInfo { get; private set; }
    public TaskType currentSelectType { get; private set; } = TaskType.Count;
    public int currentClickLevel { get; private set; }
    public int firstMoreMax { get; private set; }
    public ChaseTask lastStartChase { get; set; }
    public ChaseTask targetTaskFromForge { get; private set; }

    /// <summary>
    /// 上一次邀请战斗的人
    /// </summary>
    public PAssistInfo LastComrade;
    public int lastSelectIndex { get; set; } = 0;
    //跳转到噩梦难度时,要不要隐藏detailPanel
    public bool isShowDetailPanel { get; set; }
    public bool isAddNewEmer { get; set; }
    public int restBuyFatigueCount
    {
        get
        {
            int num = 0;
            if (chaseInfo == null) return num;
            num = chaseInfo.fatigueBuyLimit - chaseInfo.fatigueBuyNum;
            num = num < 0 ? 0 : num;
            return num;
        }
    }
    public int restTimeSpan
    {
        get
        {
            if (chaseInfo != null)
            {
                int time = (int)chaseInfo.restTime - Mathf.RoundToInt(Time.realtimeSinceStartup - m_recvChaseInfoTime);
                return time > 0 ? time : 0;
            }
            return 0;
        }
    }
    public byte currentGetRewardID { get; set; }

    /// <summary>
    /// 是否是宠物关卡(禁止还原状态值)
    /// </summary>
    public bool isPetLevelTask { get; set; }
    #endregion

    #region 全部
    /// <summary>
    /// 所有追捕任务(包括未领取的任务)(包括紧急任务)
    /// </summary>
    public List<TaskInfo> allTasks { get { return m_allTasks; } }
    private List<TaskInfo> m_allTasks = new List<TaskInfo>();

    /// <summary>
    /// 所有已领取的追捕任务(包括紧急任务)
    /// </summary>
    public List<ChaseTask> allChaseTasks { get { return m_allChaseTasks; } }
    private List<ChaseTask> m_allChaseTasks = new List<ChaseTask>();

    /// <summary>
    /// 关卡名字 key是taskID
    /// </summary>
    public Dictionary<int, string> allTasks_Name { get { return m_allTasks_Name; } }
    private Dictionary<int, string> m_allTasks_Name = new Dictionary<int, string>();
    #endregion

    #region 普通和困难和噩梦
    /// <summary>
    /// 不同类型的关卡拥有的等级
    /// </summary>
    public Dictionary<TaskType, List<int>> haveLvDic { get { return m_haveLVDic; } }
    private Dictionary<TaskType, List<int>> m_haveLVDic = new Dictionary<TaskType, List<int>>();

    /// <summary>
    /// 不同模式的最高等级
    /// </summary>
    public Dictionary<TaskType, int> typeMaxLv { get { return m_typeMaxLv; } }
    private Dictionary<TaskType, int> m_typeMaxLv = new Dictionary<TaskType, int>();

    /// <summary>
    /// 所以普通任务(包括未领取)
    /// </summary>
    private List<TaskInfo> allNormalTasks = new List<TaskInfo>();

    /// <summary>
    /// 简单模式下的不同等级的任务
    /// </summary>
    public Dictionary<int, List<Chase_PageItem>> easyTasksToMaxDic { get { return m_easyTasksToMaxDic; } }
    private Dictionary<int, List<Chase_PageItem>> m_easyTasksToMaxDic = new Dictionary<int, List<Chase_PageItem>>();

    //#################################################################################################################

    /// <summary>
    /// 所有困难任务(包括未领取)
    /// </summary>
    public List<TaskInfo> allDiffTasks { get { return m_allDiffTasks; } }
    private List<TaskInfo> m_allDiffTasks = new List<TaskInfo>();

    /// <summary>
    /// 困难模式下不同等级的任务
    /// </summary>
    public Dictionary<int, List<Chase_PageItem>> diffTasksToMaxDic { get { return m_diffTasksToMaxDic; } }
    private Dictionary<int, List<Chase_PageItem>> m_diffTasksToMaxDic = new Dictionary<int, List<Chase_PageItem>>();

    /// <summary>
    /// 当前重置困难关卡的ID
    /// </summary>
    public ushort currentResetDiffTaskId { get; private set; }

    //#################################################################################################################

    /// <summary>
    /// 所有噩梦任务(包括未领取)
    /// </summary>
    public List<TaskInfo> allNightmareTasks { get { return m_allNightmareTasks; } }
    private List<TaskInfo> m_allNightmareTasks = new List<TaskInfo>();

    public List<ChaseTask> CanEnterNightmareList { get; private set; } = new List<ChaseTask>();

    /// <summary>
    /// 恶魔模式下不同等级的任务
    /// </summary>
    public Dictionary<int, List<Chase_PageItem>> nightmareTasksToMax { get { return m_nightmareTasksToMax; } }
    private Dictionary<int, List<Chase_PageItem>> m_nightmareTasksToMax = new Dictionary<int, List<Chase_PageItem>>();
    #endregion

    #region 紧急
    /// <summary>
    /// 紧急任务
    /// </summary>
    public List<ChaseTask> emergencyList { get { return m_emergencyList; } }
    private List<ChaseTask> m_emergencyList = new List<ChaseTask>();

    public Dictionary<int, int> emerLockLv { get { return m_emerLockLv; } }
    private Dictionary<int, int> m_emerLockLv = new Dictionary<int, int>();

    private List<TaskInfo> m_allEmerTasks = new List<TaskInfo>();

    #endregion

    #region 活动关卡
    /// <summary>
    /// 所有活动任务
    /// </summary>
    private List<TaskInfo> allActiveTasks = new List<TaskInfo>();

    /// <summary>
    /// 所有活动关卡
    /// </summary>
    public List<ActiveTaskInfo> allActiveItems { get { return m_allActiveItems; } }
    private List<ActiveTaskInfo> m_allActiveItems = new List<ActiveTaskInfo>();

    /// <summary>
    /// 开启的活动关卡
    /// </summary>
    public List<ActiveTaskInfo> openActiveItems { get { return m_openActiveItems; } }
    private List<ActiveTaskInfo> m_openActiveItems = new List<ActiveTaskInfo>();

    /// <summary>
    /// 每个活动关卡对应的任务
    /// </summary>
    public Dictionary<int, List<Chase_PageItem>> activeToMaxDic { get { return m_activeToMaxDic; } }
    private Dictionary<int, List<Chase_PageItem>> m_activeToMaxDic = new Dictionary<int, List<Chase_PageItem>>();

    /// <summary>
    /// 不同活动的通关次数 key 是关卡的level int是次数
    /// </summary>
    public Dictionary<int, int> activeChallengeCount { get { return m_activeChallengeCount; } }
    private Dictionary<int, int> m_activeChallengeCount = new Dictionary<int, int>();
    #endregion

    #region 章节奖励
    /// <summary>
    /// 关卡章节奖励
    /// </summary>
    public List<ChaseLevelStarInfo> allStarRewardInfo { get { return m_allStarRewardInfo; } }
    private List<ChaseLevelStarInfo> m_allStarRewardInfo = new List<ChaseLevelStarInfo>();

    /// <summary>
    /// 当前点击的章节奖励
    /// </summary>
    public List<ChaseLevelStarInfo> currentStarRewardInfo => m_currentStarRewardInfo;
    private List<ChaseLevelStarInfo> m_currentStarRewardInfo = new List<ChaseLevelStarInfo>();

    /// <summary>
    /// 章节奖励界面领取的状态
    /// </summary>
    public Dictionary<byte, bool> getRewardState { get { return m_getRewardState; } }
    private Dictionary<byte, bool> m_getRewardState = new Dictionary<byte, bool>();

    public TaskType rewardType { get; private set; }
    public int rewardLevel { get; private set; }
    #endregion

    #region module-base
    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        InitLevelForType();

        SendChaseInfo();
        //moduleAwakeMatch.Request_TeamTaskInfo(3);
        moduleAwakeMatch.Request_TeamTaskInfo(1);
        SendStarPanelState();
        SendActiveState();
    }

    public void InitLevelForType()
    {
        m_allTasks = ConfigManager.GetAll<TaskInfo>();
        m_allActiveItems = ConfigManager.GetAll<ActiveTaskInfo>();
        m_allStarRewardInfo = ConfigManager.GetAll<ChaseLevelStarInfo>();

        if (m_allActiveItems != null && m_allActiveItems.Count > 0)
            m_allActiveItems.Sort((a, b) => a.taskLv.CompareTo(b.taskLv));

        OnAddName();

        allNormalTasks = m_allTasks.FindAll(p => GetCurrentTaskType(p) == TaskType.Easy);
        m_allDiffTasks = m_allTasks.FindAll(p => GetCurrentTaskType(p) == TaskType.Difficult);
        allActiveTasks = m_allTasks.FindAll(p => GetCurrentTaskType(p) == TaskType.Active);
        m_allNightmareTasks = m_allTasks.FindAll(p => GetCurrentTaskType(p) == TaskType.Nightmare);
        m_allEmerTasks = m_allTasks.FindAll(p => GetCurrentTaskType(p) == TaskType.Emergency);

        GetLevelForType();
    }

    private void SortAllTasks()
    {
        m_allTasks.Sort((a, b) =>
        {
            int type_a = (int)GetCurrentTaskType(a);
            int type_b = (int)GetCurrentTaskType(b);

            int result = type_a.CompareTo(type_b);
            if (result == 0)
            {
                result = a.level.CompareTo(b.level);
                if (result == 0)
                    return a.ID.CompareTo(b.ID);
            }
            return result;
        });
    }

    private void OnAddName()
    {
        SortAllTasks();

        m_allTasks_Name.Clear();

        TaskType type = TaskType.Count;
        int num = 0;
        int _level = 0;
        string name = "";
        for (int i = 0; i < m_allTasks.Count; i++)
        {
            var _type = GetCurrentTaskType(m_allTasks[i]);
            if (_type != type)
            {
                type = _type;
                num = 0;
            }
            else
            {
                if (m_allTasks[i].level != _level)
                    num = 0;
            }

            _level = m_allTasks[i].level;
            num++;

            name = _level + "-" + num;
            m_allTasks_Name.Add(m_allTasks[i].ID, name);
        }
    }

    private void GetLevelForType()
    {
        m_haveLVDic.Clear();
        //普通关卡的等级
        if (allNormalTasks.Count > 0)
        {
            if (!m_haveLVDic.ContainsKey(TaskType.Easy)) m_haveLVDic.Add(TaskType.Easy, new List<int>());
            else m_haveLVDic[TaskType.Easy].Clear();

            for (int i = 0; i < allNormalTasks.Count; i++)
            {
                int lv = allNormalTasks[i].level;
                if (!m_haveLVDic[TaskType.Easy].Contains(lv)) m_haveLVDic[TaskType.Easy].Add(lv);
            }
            m_haveLVDic[TaskType.Easy].Sort((a, b) => a.CompareTo(b));
        }
        //困难关卡的等级
        if (m_allDiffTasks.Count > 0)
        {
            if (!m_haveLVDic.ContainsKey(TaskType.Difficult)) m_haveLVDic.Add(TaskType.Difficult, new List<int>());
            else m_haveLVDic[TaskType.Difficult].Clear();

            for (int i = 0; i < m_allDiffTasks.Count; i++)
            {
                int lv = m_allDiffTasks[i].level;
                if (!m_haveLVDic[TaskType.Difficult].Contains(lv)) m_haveLVDic[TaskType.Difficult].Add(lv);
            }
            m_haveLVDic[TaskType.Difficult].Sort((a, b) => a.CompareTo(b));
        }
        //噩梦关卡等级
        if (m_allNightmareTasks.Count > 0)
        {
            if (!m_haveLVDic.ContainsKey(TaskType.Nightmare)) m_haveLVDic.Add(TaskType.Nightmare, new List<int>());
            else m_haveLVDic[TaskType.Nightmare].Clear();

            for (int i = 0; i < m_allDiffTasks.Count; i++)
            {
                int lv = m_allNightmareTasks[i].level;
                if (!m_haveLVDic[TaskType.Nightmare].Contains(lv)) m_haveLVDic[TaskType.Nightmare].Add(lv);
            }
            m_haveLVDic[TaskType.Nightmare].Sort((a, b) => a.CompareTo(b));
        }
        //紧急任务
        if (m_allEmerTasks.Count > 0)
        {
            if (!m_haveLVDic.ContainsKey(TaskType.Emergency)) m_haveLVDic.Add(TaskType.Emergency, new List<int>());
            else m_haveLVDic[TaskType.Emergency].Clear();

            m_emerLockLv.Clear();
            int lv = -1;
            for (int i = 0; i < m_allEmerTasks.Count; i++)
            {
                if (m_allEmerTasks[i].difficult == lv) continue;

                lv = m_allEmerTasks[i].difficult;
                if (!m_haveLVDic[TaskType.Emergency].Contains(lv)) m_haveLVDic[TaskType.Emergency].Add(lv);

                if (m_emerLockLv.ContainsKey(lv)) m_emerLockLv[lv] = m_allEmerTasks[i].unlockLv;
                else m_emerLockLv.Add(lv, m_allEmerTasks[i].unlockLv);
            }

            m_haveLVDic[TaskType.Emergency].Sort((a, b) => a.CompareTo(b));
        }
    }

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        InitChaseData();
        moduleChase.LastComrade = null;
    }

    public void InitChaseData()
    {
        isFirstSpecailTask = false;
        m_haveLVDic.Clear();
        chaseInfo = null;
        targetTaskFromForge = null;
        m_recvChaseInfoTime = 0f;
        m_emergencyList.Clear();
        m_allTasks.Clear();
        m_allTasks_Name.Clear();
        m_allChaseTasks.Clear();
        m_typeMaxLv.Clear();
        currentClickLevel = -1;
        currentSelectType = TaskType.Count;
        m_getRewardState.Clear();
        currentResetDiffTaskId = 0;
        m_easyTasksToMaxDic.Clear();
        m_diffTasksToMaxDic.Clear();
        allNormalTasks.Clear();
        m_allDiffTasks.Clear();
        lastStartChase = null;
        currentGetRewardID = 0;
        m_activeToMaxDic.Clear();
        m_allActiveItems.Clear();
        allActiveTasks.Clear();
        m_activeChallengeCount.Clear();
        m_openActiveItems.Clear();
        isReceiveActive = false;
        isPetLevelTask = false;
        m_allStarRewardInfo.Clear();
        lastSelectIndex = 0;
        m_allNightmareTasks.Clear();
        m_nightmareTasksToMax.Clear();
        CanEnterNightmareList.Clear();
        m_emerLockLv.Clear();
        isShowDetailPanel = false;
        m_allEmerTasks.Clear();
        isAddNewEmer = false;
        rewardLevel = 0;
        rewardType = TaskType.None;
    }
    #endregion

    #region PacketMessage
    public void SendStartChase(ChaseTask info, PAssistInfo rAssistInfo = null)
    {
//        if (rAssistInfo == null)
//        {
//            rAssistInfo = PacketObject.Create<PAssistInfo>();
//            rAssistInfo.type = 1;
//            rAssistInfo.npcId = 3;
//        }

        lastStartChase = info;
        isPetLevelTask = false;
        moduleUnion.m_isUnionBossTask = false;

        CsChaseTaskStart p = PacketObject.Create<CsChaseTaskStart>();
        p.taskId = info.taskData.taskId;
        p.assistInfo = rAssistInfo;
        session.Send(p);

        rAssistInfo?.CopyTo(ref LastComrade);
    }

    void _Packet(ScChaseTaskStart chaseTasks)
    {
        if (chaseTasks.result != 0 || isPetLevelTask)
        {
            if(!isPetLevelTask)
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(401, chaseTasks.result));
            lastStartChase = null;
            LastComrade = null;
        }
        else
        {
            AudioManager.PlaySound(AudioInLogicInfo.audioConst.enterStageSucc);
            modulePVE.OnScRoleStartChase(chaseTasks);

            if (lastStartChase != null)
            {
                lastStartChase.isFirst = chaseTasks.isFirst == 1;

                DispatchModuleEvent(EventSuccessToPVE, lastStartChase.stageInfo.levelInfoId);
            }
        }
    }

    public void SendChaseInfo()
    {
        if (chaseInfo == null)
        {
            CsChaseInfo p = PacketObject.Create<CsChaseInfo>();
            session.Send(p);
        }
        else DispatchModuleEvent(EventGetChaseInfo);
    }

    void _Packet(ScChaseInfo p)
    {
        ScChaseInfo info = null;
        p.CopyTo(ref info);
        m_recvChaseInfoTime = Time.realtimeSinceStartup;
        chaseInfo = info;
        HandleTasks(chaseInfo.chaseList);

        DispatchModuleEvent(EventGetChaseInfo);
    }

    void _Packet(ScChaseTaskUnlock p)
    {
        Logger.LogDetail("recv ScChaseTaskUnlock msg..new task count is {0}", p.chaseList == null ? 0 : p.chaseList.Length);
        PChaseTask[] tasks = null;
        p.chaseList.CopyTo(ref tasks);
       
        HandleTasks(tasks);

        for (int i = 0; i < tasks.Length; i++)
        {
            var info = ConfigManager.Get<TaskInfo>(tasks[i].taskId);
            if (info != null)
            {
                var type = GetCurrentTaskType(info);
                if (type == TaskType.Easy && info.isSpecialTask && tasks[i].state == (sbyte)EnumChaseTaskFinishState.Accept)
                {
                    isFirstSpecailTask = true;
                    break;
                }
            }
        }
    }

    public void SendStarPanelState()
    {
        if (m_getRewardState == null || m_getRewardState.Count < 1)
        {
            CsChaseStarRewards p = PacketObject.Create<CsChaseStarRewards>();
            session.Send(p);
        }
        else
            DispatchModuleEvent(EventOpenStarRewardPanel);
    }

    void _Packet(ScChaseStarRewards p)
    {
        if (p != null && p.rewards != null && p.rewards.Length > 0)
        {
            PChaseStarReward[] stars = null;
            p.rewards.CopyTo(ref stars);

            m_getRewardState.Clear();
            for (int i = 0; i < stars.Length; i++)
                m_getRewardState.Add(stars[i].id, stars[i].isDraw);
        }
        DispatchModuleEvent(EventOpenStarRewardPanel);
    }

    public void SendGetReward(byte _id)
    {
        CsChaseStarRewardDraw p = PacketObject.Create<CsChaseStarRewardDraw>();
        p.id = _id;
        session.Send(p);
    }

    void _Packet(ScChaseStarRewardDraw p)
    {
        if (p.result == 0)
        {
            if (m_getRewardState.ContainsKey(currentGetRewardID) && !m_getRewardState[currentGetRewardID])
            {
                m_getRewardState[currentGetRewardID] = true;
                DispatchModuleEvent(EventRefreshRewardPanel);
                List<PItem2> lists = new List<PItem2>();
                if (m_currentStarRewardInfo.Count > 0)
                {
                    for (int i = 0; i < m_currentStarRewardInfo.Count; i++)
                    {
                        if (m_currentStarRewardInfo[i].ID != currentGetRewardID) continue;
                        string[] str = m_currentStarRewardInfo[i].rewards;
                        for (int j = 0; j < str.Length; j++)
                        {
                            string[] chars = str[j].Split('-');
                            if (chars.Length == 4)
                            {
                                var id = Util.Parse<ushort>(chars[0]);
                                var info = ConfigManager.Get<PropItemInfo>(id);
                                if (!info || !info.IsValidVocation(modulePlayer.proto)) continue;

                                PItem2 item = PacketObject.Create<PItem2>();
                                item.itemTypeId = id;
                                item.level = info.itemType == PropType.Rune ? Util.Parse<byte>(chars[1]) : (byte)0;
                                item.star = Util.Parse<byte>(chars[2]);
                                item.num = Util.Parse<uint>(chars[3]);
                                lists.Add(item);
                            }
                        }
                    }
                    string title = ConfigText.GetDefalutString(TextForMatType.SettlementUIText, 1);
                    Window_ItemTip.Show(title, lists);
                }
            }
        }
        else
        {
            if (p.result == 1) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 23));
            else if (p.result == 2) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 24));
            else if (p.result == 3) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 25));
        }
    }

    public void GetLevelStarInfo(TaskType type, int level)
    {
        int _isRare = type == TaskType.Easy ? 0 : type == TaskType.Difficult ? 1 : 2;
        m_currentStarRewardInfo = m_allStarRewardInfo.FindAll(p => p.level == level && p.isRare == _isRare);
    }

    public void SetRewardTypeAndLevel(TaskType type, int _level)
    {
        rewardType = type;
        rewardLevel = _level;
    }

    public void SendRestChallenge(ushort task_id)
    {
        CsChaseResetOverTimes p = PacketObject.Create<CsChaseResetOverTimes>();
        p.taskId = task_id;
        session.Send(p);
        currentResetDiffTaskId = task_id;
    }

    void _Packet(ScChaseResetOverTimes p)
    {
        if (p.result == 0)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 26));
            ChaseTask task = GetTargetTask(currentResetDiffTaskId);
            if (task != null)
            {
                task.taskData.resetTimes++;
                //task.taskData.dailyOverTimes = 0;

                GetTargetChaseList(task.taskType, task.taskConfigInfo.level);
            }
            moduleNpcGaiden.ResetTimesSuccess(currentResetDiffTaskId);
            moduleAwakeMatch.ResetTimesSuccess(currentResetDiffTaskId);
            DispatchModuleEvent(EventResetTimesSuccess, currentResetDiffTaskId);
        }
        else
        {
            if (p.result == 1) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 27));
            else if (p.result == 2) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 28));
            else if (p.result == 3) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 29));
        }
    }

    void _Packet(ScChaseStateChange p)
    {
        ChaseTask targetTask = GetTargetTask(p.taskId);

        if (targetTask == null || targetTask.taskType != TaskType.Awake)
        {
            //Logger.LogError("{0} connot find task from loaded task...", p.taskId);
            return;
        }
        //replace task data
        targetTask.taskData.state = p.state;
    }

    public void SendActiveState()
    {
        if (!isReceiveActive)
        {
            CsChaseActiveState p = PacketObject.Create<CsChaseActiveState>();
            session.Send(p);
        }
        else DispatchModuleEvent(EventRefreshMainActivepanel);
    }

    void _Packet(ScChaseActiveState p)
    {
        isReceiveActive = true;

        PChaseActives[] openActiveStates = null;
        p.states.CopyTo(ref openActiveStates);

        List<PChaseTask> chasetasks = new List<PChaseTask>();
        if (openActiveStates != null && openActiveStates.Length > 0)
        {
            m_activeChallengeCount.Clear();
            m_openActiveItems.Clear();
            for (int i = 0; i < openActiveStates.Length; i++)
            {
                var info = ConfigManager.Get<ActiveTaskInfo>(openActiveStates[i].activeId);
                if (info == null) continue;
                if (!m_activeChallengeCount.ContainsKey(info.taskLv))
                {
                    m_activeChallengeCount.Add(info.taskLv, openActiveStates[i].crossCount);
                    m_openActiveItems.Add(info);

                    chasetasks.AddRange(openActiveStates[i].chaseInfo);                    
                }
            }
            HandleTasks(chasetasks.ToArray());
        }

        DispatchModuleEvent(EventRefreshMainActivepanel);
    }

    void _Packet(ScChaseActiveOpen p)
    {
        for (int i = 0; i < p.activeIds.Length; i++)
        {
            var info = ConfigManager.Get<ActiveTaskInfo>(p.activeIds[i]);
            if (info == null) return;

            var tt = m_openActiveItems.Find(s => s.ID == p.activeIds[i]);
            if (tt == null) m_openActiveItems.Add(info);
        }
        DispatchModuleEvent(EventRefreshActiveAddAndRemove);
    }

    void _Packet(ScChaseActiveClose p)
    {
        for (int i = 0; i < p.activeIds.Length; i++)
        {
            var info = ConfigManager.Get<ActiveTaskInfo>(p.activeIds[i]);
            if (info == null) return;
            var task = m_openActiveItems.Find(a => a.taskLv == info.taskLv);
            if (task) m_openActiveItems.Remove(task);
        }
        DispatchModuleEvent(EventRefreshActiveAddAndRemove);
    }

    void _Packet(ScTeamPveTaskInfo p)
    {
        if (p.type == 1)
        {
            HandleTasks(p.stageInfo);

            if (moduleAwakeMatch.CurrentTask != null && moduleAwakeMatch.CurrentTask.taskType == TaskType.Emergency)
            {
                if (typeMaxLv.ContainsKey(TaskType.Emergency) && typeMaxLv[TaskType.Emergency] > moduleAwakeMatch.CurrentTask.taskConfigInfo.difficult)
                    isAddNewEmer = true;
            }
        }
    }

    #endregion

    #region follow-message

    public void AddNewEmergencyTask(ushort taskId,byte star)
    {
        var task = PacketObject.Create<PChaseTask>();
        task.taskId = taskId;
        task.state = (sbyte)EnumChaseTaskFinishState.Accept;
        task.taskLv = (ushort)TaskType.Emergency;
        task.curStar = star;

        HandleTasks(new[] { task });
    }

    void HandleTasks(PChaseTask[] tasks)
    {
        if (tasks == null || tasks.Length < 1) return;
        bool isInEasy = false;
        bool isInDifficult = false;
        bool isInActive = false;
        bool isNightmare = false;

        if (!m_typeMaxLv.ContainsKey(TaskType.Easy)) m_typeMaxLv.Add(TaskType.Easy, 1);
        if (!m_typeMaxLv.ContainsKey(TaskType.Difficult)) m_typeMaxLv.Add(TaskType.Difficult, 1);
        if (!m_typeMaxLv.ContainsKey(TaskType.Nightmare)) m_typeMaxLv.Add(TaskType.Nightmare, 1);
        if (!m_typeMaxLv.ContainsKey(TaskType.Emergency)) m_typeMaxLv.Add(TaskType.Emergency, 1);

        for (int i = 0; i < tasks.Length; i++)
        {
            PChaseTask taskData = tasks[i];
            TaskInfo ti = ConfigManager.Get<TaskInfo>(taskData.taskId);

            if (ti == null)
            {
                Logger.LogError("server task_id = {0} connot be finded,please check out", taskData.taskId);
                continue;
            }

            ChaseTask task = ChaseTask.Create(taskData, ti);
            TaskType _type = GetCurrentTaskType(ti);

            switch (_type)
            {
                case TaskType.Emergency:
                    var _task = m_emergencyList.Find(p => p.taskConfigInfo?.ID == task.taskConfigInfo?.ID);
                    if (_task == null) m_emergencyList.Add(task);
                    break;
                case TaskType.Easy:      isInEasy      = true; break;
                case TaskType.Difficult: isInDifficult = true; break;
                case TaskType.Active:    isInActive    = true; break;
                case TaskType.Nightmare: isNightmare   = true; break;
                default: break;
            }

            if (_type == TaskType.Easy || _type == TaskType.Difficult || _type == TaskType.Nightmare)
            {
                //简单模式和困难模式的最大等级               
                if (task.taskData.taskLv > m_typeMaxLv[_type])
                    m_typeMaxLv[_type] = task.taskData.taskLv;
            }
            else if (_type == TaskType.Emergency)
            {
                if (task.taskConfigInfo.difficult > m_typeMaxLv[_type])
                    m_typeMaxLv[_type] = task.taskConfigInfo.difficult;
            }

            //添加到总任务
            var isContains = m_allChaseTasks.Find(p => p.taskConfigInfo.ID == task.taskConfigInfo.ID);
            if (isContains == null) m_allChaseTasks.Add(task);
        }

        m_emergencyList.Sort((a, b) => a.taskConfigInfo.unlockLv - b.taskConfigInfo.unlockLv);

        if (isInEasy) OnAddDataToNormalDic();
        if (isInDifficult) OnAddDataToDiffDic();
        if (isInActive) OnAddDataToActiveDic();
        if (isNightmare) OnAddDataToNightMare();
    }

    private void OnAddDataToNormalDic()
    {
        //普通关卡
        m_easyTasksToMaxDic.Clear();
        int totalCount = m_haveLVDic != null && m_haveLVDic.ContainsKey(TaskType.Easy) ? m_haveLVDic[TaskType.Easy].Count : 0;
        for (int i = 0; i < totalCount; i++)
        {
            List<TaskInfo> m_TempList = allNormalTasks.FindAll(p => p.level == m_haveLVDic[TaskType.Easy][i]);
            OnAddData(m_TempList, m_haveLVDic[TaskType.Easy][i], m_easyTasksToMaxDic);
        }
    }

    private void OnAddDataToDiffDic()
    {
        //困难关卡
        m_diffTasksToMaxDic.Clear();
        int totalCount = m_haveLVDic != null && m_haveLVDic.ContainsKey(TaskType.Difficult) ? m_haveLVDic[TaskType.Difficult].Count : 0;
        for (int i = 0; i < totalCount; i++)
        {
            List<TaskInfo> m_TempList = m_allDiffTasks.FindAll(p => p.level == m_haveLVDic[TaskType.Difficult][i]);
            OnAddData(m_TempList, m_haveLVDic[TaskType.Difficult][i], m_diffTasksToMaxDic);            
        }
    }

    private void OnAddData(List<TaskInfo> resource, int level, Dictionary<int, List<Chase_PageItem>> typeDic)
    {
        if (resource == null || resource.Count < 1 || typeDic == null) return;

        resource.Sort((a, b) => a.ID.CompareTo(b.ID));

        int pageCount = resource.Count % PAGENUMBER == 0 ? resource.Count / PAGENUMBER : (resource.Count / PAGENUMBER) + 1;

        for (int k = 0; k < pageCount; k++)
        {
            List<TaskInfo> k_list = new List<TaskInfo>();
            int count = (k * PAGENUMBER + 6) > resource.Count ? resource.Count : k * PAGENUMBER + 6;

            for (int j = k * PAGENUMBER; j < count; j++)
                k_list.Add(resource[j]);

            bool canAdd = false;
            for (int h = 0; h < k_list.Count; h++)
            {
                ChaseTask haveChase = m_allChaseTasks.Find(p => p.taskConfigInfo.ID == k_list[h].ID);
                if (haveChase != null)
                {
                    canAdd = true;
                    break;
                }
            }
            if (!canAdd) break;

            Chase_PageItem item = new Chase_PageItem(k_list, k, level);
            if (!typeDic.ContainsKey(level))
                typeDic.Add(level, new List<Chase_PageItem>());

            typeDic[level].Add(item);
        }
    }

    private void OnAddDataToNightMare()
    {
        m_nightmareTasksToMax.Clear();
        CanEnterNightmareList.Clear();
        CanEnterNightmareList = m_allChaseTasks.FindAll(p => p.taskType == TaskType.Nightmare);
        int count = m_haveLVDic != null && m_haveLVDic.ContainsKey(TaskType.Nightmare) ? m_haveLVDic[TaskType.Nightmare].Count : 0;

        for (int i = 0; i < count; i++)
        {
            List<TaskInfo> m_tempList = m_allNightmareTasks.FindAll(p => p.level == m_haveLVDic[TaskType.Nightmare][i]);
            OnAddData(m_tempList, m_haveLVDic[TaskType.Nightmare][i], m_nightmareTasksToMax);
        }
    }

    public void OnChaseTaskFinish(byte overStarNumber)
    {
        if (lastStartChase == null) return;

        lastStartChase.taskData.times++;
        //set task finish
        if (lastStartChase.taskData.state == (byte)EnumChaseTaskFinishState.Accept && modulePVE.isSendWin) lastStartChase.taskData.state = 2;

        if (lastStartChase.taskType == TaskType.Active && lastStartChase.taskData.state == (byte)EnumChaseTaskFinishState.Finish && modulePVE.isSendWin && m_activeChallengeCount.ContainsKey(lastStartChase.taskConfigInfo.level))
            m_activeChallengeCount[lastStartChase.taskConfigInfo.level]++;

        if (lastStartChase.taskData.curStar < overStarNumber) lastStartChase.taskData.curStar = overStarNumber;

        if (modulePVE.isSendWin)
        {
            lastStartChase.taskData.dailyOverTimes += 1;
            lastStartChase.taskData.successTimes++;
            if (lastStartChase.taskConfigInfo.maxChanllengeTimes > 0 && lastStartChase.taskData.successTimes >= lastStartChase.taskConfigInfo.maxChanllengeTimes)
                lastStartChase.taskData.state = 3;
        }

        if (targetTaskFromForge != null) targetTaskFromForge = null;

        moduleGuide.UpdateTaskChanllengeCondition(lastStartChase);
    }

    public void ProcessTimes(ChaseTask task, int rTime)
    {
        if (null == task) return;
        task.taskData.times += (uint)rTime;
        task.taskData.successTimes += (uint)rTime;
        if (task.taskConfigInfo.maxChanllengeTimes > 0 && task.taskData.successTimes >= task.taskConfigInfo.maxChanllengeTimes)
            task.taskData.state = 3;
        if (task.taskType == TaskType.Active && m_activeChallengeCount.ContainsKey(task.taskConfigInfo.level))
            m_activeChallengeCount[task.taskConfigInfo.level] += rTime;
        else
        {
            task.taskData.dailyOverTimes += rTime;
            GetTargetChaseList(task.taskType, task.taskConfigInfo.level);
        }
    }
    #endregion

    #region toolFunction

    /// <summary>
    /// 获取目标task的类型
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    public TaskType GetCurrentTaskType(TaskInfo form)
    {
        if (form.level == 0)
            return TaskType.Emergency;

        var activeTask = m_allActiveItems.Find(active => active.taskLv == form.level);
        if (activeTask != null)
            return TaskType.Active;

        if (Module_AwakeMatch.dependTask?.taskLv == form.level ||
            Module_AwakeMatch.activeTask?.taskLv == form.level)
            return TaskType.Awake;
        if (form.level >= 1 && form.level <= 6) return form.isRare == 1 ? TaskType.Difficult : form.isRare == 2 ? TaskType.Nightmare : TaskType.Easy;
        else if (form.isGaidenTask) return (TaskType)form.level;

        return TaskType.Count;      
    }

    /// <summary>
    /// 获得当前type当前level的星星总数
    /// </summary>
    /// <param name="type"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public int GetCurrentLevelTotalStar(TaskType type, int level)
    {
        if (type == TaskType.Easy || type == TaskType.Difficult || type == TaskType.Nightmare)
        {
            var tasks = allTasks.FindAll(p => GetCurrentTaskType(p) == type && p.level == level);
            if (tasks == null || tasks.Count < 1) return 0;
            return tasks.Count * 3;
        }
        return 0;
    }

    /// <summary>
    /// 获取当前type当前level的星星获得数量
    /// </summary>
    /// <param name="type"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public int GetCurrentLevelGetStar(TaskType type, int level)
    {
        if (type == TaskType.Easy || type == TaskType.Difficult || type == TaskType.Nightmare)
        {
            var tasks = m_allChaseTasks.FindAll(p => GetCurrentTaskType(p.taskConfigInfo) == type && p.taskConfigInfo.level == level);
            if (tasks == null || tasks.Count < 1) return 0;
            int number = 0;
            for (int i = 0; i < tasks.Count; i++)
                number += tasks[i].star;
            return number;
        }
        return 0;
    }

    /// <summary>
    /// 获取指定的chaseTask
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    public ChaseTask GetTargetTask(int taskId)
    {
        return m_allChaseTasks.Find(p => p.taskConfigInfo.ID == taskId);
    }

    /// <summary>
    /// 获取当前购买体力的钻石消耗数量
    /// </summary>
    /// <returns></returns>
    public int GetCurrentDiamondCost()
    {
        int index = chaseInfo.fatigueBuyNum >= chaseInfo.fatigueBuyCost.Length ? chaseInfo.fatigueBuyCost.Length - 1 : chaseInfo.fatigueBuyNum;
        return chaseInfo.fatigueBuyCost[index];
    }

    public string GetCurrentLevelString(int level)
    {
        switch (level)
        {
            case 0: return ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 5);
            case 1: return ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 6);
            case 2: return ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 7);
            case 3: return ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 8);
            case 4: return ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 9);
            case 5: return ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 10);
            case 6: return ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 11);
            default: return string.Empty;
        }
    }
    #endregion

    #region DispatchModuleEvent

    public void GetTargetChaseList(TaskType type, int level)
    {
        currentSelectType = type;
        currentClickLevel = level;
        firstMoreMax = GetFirstMoreMax(type);

        List<Chase_PageItem> target = new List<Chase_PageItem>();
        switch (type)
        {
            case TaskType.Easy     : if (m_easyTasksToMaxDic.ContainsKey(level)) target = m_easyTasksToMaxDic[level]; break;
            case TaskType.Difficult: if (m_diffTasksToMaxDic.ContainsKey(level)) target = m_diffTasksToMaxDic[level]; break;
            case TaskType.Nightmare: if (m_nightmareTasksToMax.ContainsKey(level)) target = m_nightmareTasksToMax[level]; break;
            case TaskType.Emergency:
            {
                var chase = m_emergencyList.Find(p => p.taskConfigInfo.difficult == level);
                DispatchModuleEvent(EventRefreshChaseTask, type, level, chase);
                break;
            }
            case TaskType.Active   : if (m_activeToMaxDic.ContainsKey(level))target = m_activeToMaxDic[level]; break;
            default                : break;
        }

        if (type != TaskType.Emergency)
            DispatchModuleEvent(EventRefreshChaseTask, type, level, target);

        #region 触发高能任务
        //如果是特殊任务要触发事件
        if (type == TaskType.Easy)
        {
            if (m_easyTasksToMaxDic == null) return;
            if (m_easyTasksToMaxDic != null && !m_easyTasksToMaxDic.ContainsKey(level)) return;
            for (int i = 0; i < m_easyTasksToMaxDic[level].Count; i++)
            {
                TaskInfo taskInfo = m_easyTasksToMaxDic[level][i].current_Tasks.Find(p => p.isSpecialTask);
                if (taskInfo)
                {
                    ChaseTask specialTask = m_allChaseTasks.Find(p => p.taskConfigInfo.ID == taskInfo.ID);
                    if (specialTask != null)
                    {
                        if (isFirstSpecailTask && specialTask.taskData.state == (byte)EnumChaseTaskFinishState.Accept)
                        {
                            DispatchModuleEvent(EventIsSpecial, specialTask);
                            isFirstSpecailTask = false;
                            break;
                        }
                    }
                }
            }
        }
        #endregion
    }

    private int FindFirstAcceptIndex(List<Chase_PageItem> target)
    {
        if (target != null && target.Count > 0)
        {
            for (int i = 0; i < target.Count; i++)
            {
                List<TaskInfo> tasksInfo = target[i].current_Tasks;
                for (int k = 0; k < tasksInfo.Count; k++)
                {
                    ChaseTask task = m_allChaseTasks.Find(p => p.taskConfigInfo.ID == tasksInfo[k].ID);
                    if (task == null || task.taskData.state == (byte)EnumChaseTaskFinishState.Accept)
                        return target[i].current_Index;
                }
            }
        }
        return 0;
    }

    private int FindTargetTaskIndex(List<Chase_PageItem> target, ChaseTask from)
    {
        if (target != null && target.Count > 0)
        {
            for (int i = 0; i < target.Count; i++)
            {
                List<TaskInfo> tasksInfo = target[i].current_Tasks;
                for (int k = 0; k < tasksInfo.Count; k++)
                {
                    if (tasksInfo[k].ID == from.taskConfigInfo.ID)
                        return target[i].current_Index;
                }
            }
        }
        Logger.LogError("can not find tartget task in allTasks !!! task id == {0}", from.taskConfigInfo.ID);
        return 0;
    }

    #endregion

    #region skip-Part

    /// <summary>
    /// 跳转到目标type
    /// </summary>
    /// <param name="type"></param>
    public void SkipTargetTypeTasks(TaskType type, int active_type = 100, bool addGuideEvent = true)
    {
        if (chaseInfo == null || type == TaskType.Count) return;

        int level = 0;
        if (type == TaskType.Easy || type == TaskType.Difficult || type == TaskType.Nightmare || type == TaskType.Emergency)
        {
            if (!m_typeMaxLv.ContainsKey(type)) return;
            level = m_typeMaxLv[type];
        }
        else if (type == TaskType.Active) level = active_type;
        
        GetTargetChaseList(type, level);

        if (addGuideEvent) moduleGuide.AddWindowChaseEvent(type);
    }

    /// <summary>
    /// 设置跳转目标
    /// </summary>
    /// <param name="target"></param>
    public void SetTargetTask(ChaseTask task)
    {
        targetTaskFromForge = task;

        if (targetTaskFromForge != null)
            EventManager.AddEventListener(Events.UI_WINDOW_VISIBLE, OnWindowVisiable);
        else
            EventManager.RemoveEventListener(Events.UI_WINDOW_VISIBLE, OnWindowVisiable);

        //if (targetTaskFromForge != null) DispatchModuleEvent(EventSkiptargetChaseTask);
    }

    private void OnWindowVisiable(Event_ e)
    {
        var w = e.param1 as Window;
        if ((w?.isFullScreen ?? false) && w?.GetType() != typeof(Window_Chase))
        {
            SetTargetTask(null);
        }
    }

    /// <summary>
    /// 跳转到目标task
    /// </summary>
    /// <param name="targetTask"></param>
    public void SkipTargetTask(ChaseTask targetTask)
    {
        if (targetTask != null)
        {
            if (targetTask.taskType == TaskType.Emergency) GetTargetChaseList(targetTask.taskType, targetTask.taskConfigInfo.difficult);
            else GetTargetChaseList(targetTask.taskType, targetTask.taskConfigInfo.level);
        }
    }

    /// <summary>
    /// 跳转到上次结束时的模式和关卡
    /// </summary>
    public void SkipLastTypeAndLevel()
    {
        if (lastStartChase == null) return;

        int level = 0;
        if (lastStartChase.taskType == TaskType.Difficult || lastStartChase.taskType == TaskType.Easy || lastStartChase.taskType == TaskType.Nightmare)
        {
            if (lastStartChase.isFirst) level = m_typeMaxLv[lastStartChase.taskType];
            else level = lastStartChase.taskConfigInfo.level;

            GetTargetChaseList(lastStartChase.taskType, level);
        }
        else if (lastStartChase.taskType == TaskType.Emergency)
        {
            level = lastStartChase.taskConfigInfo.difficult;
            GetTargetChaseList(lastStartChase.taskType, level);
        }
        else GetTargetChaseList(lastStartChase.taskType, lastStartChase.taskConfigInfo.level);

        lastStartChase = null;
    }

    private int GetFirstMoreMax(TaskType type)
    {
        if (type == TaskType.Awake || type == TaskType.Active) return 0;

        if (!m_haveLVDic.ContainsKey(type)) return 0;
        int isFirst = 0;
        var list = m_haveLVDic[type];

        if (type == TaskType.Emergency)
        {
            bool isSkip = false;
            for (int i = 0; i < list.Count; i++)
            {
                var unlockLv = m_emerLockLv.ContainsKey(list[i]) ? m_emerLockLv[list[i]] : 0;
                var currentEm = m_emergencyList.Find(p => p.taskConfigInfo.difficult == list[i]);

                if (modulePlayer.level < unlockLv || currentEm == null)
                {
                    isFirst = list[i];
                    isSkip = true;
                    break;
                }
            }
            if (!isSkip) isFirst = m_typeMaxLv.ContainsKey(type) ? m_typeMaxLv[type] + 1 : 0;
        }
        else if (type == TaskType.Easy || type == TaskType.Difficult || type == TaskType.Nightmare)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var level = m_typeMaxLv.ContainsKey(type) ? m_typeMaxLv[type] : 0;
                if (list[i] == level)
                {
                    isFirst = list[i] + 1;
                    break;
                }
            }
        }

        return isFirst;
    }
    #endregion

    #region clearData

    public void ClearDataOnReturn()
    {
        currentClickLevel = -1;
        currentSelectType = TaskType.Count;
        currentResetDiffTaskId = 0;
        lastStartChase = null;
        targetTaskFromForge = null;
    }
    #endregion

    #region active

    private void OnAddDataToActiveDic()
    {
        m_activeToMaxDic.Clear();
        int totalCount = m_openActiveItems != null ? m_openActiveItems.Count : 0;
        for (int i = 0; i < totalCount; i++)
        {
            List<TaskInfo> m_TempList = allActiveTasks.FindAll(p => p.level == m_openActiveItems[i].taskLv);
            OnAddData(m_TempList, m_openActiveItems[i].taskLv, m_activeToMaxDic);
        }
    }

    #endregion

    public void ClickUnlock(TaskInfo info)
    {
        if (info == null) return;
        tip.Clear();

        var depend = info.dependId;
        bool isTaskOn = false;
        int appendTimes = 0;
        for (int i = 0; i < depend.Length; i++)
        {
            if (depend[i] == 0) continue;

            var task = ConfigManager.Get<TaskInfo>(depend[i]);
            if (task == null) continue;
            var type = GetCurrentTaskType(task);

            ChaseTask chaseTask = null;
            if (type == TaskType.Awake) chaseTask = moduleAwakeMatch.CanEnterList.Find(p => p.taskConfigInfo.ID == depend[i]);
            else if (type != TaskType.Awake && type != TaskType.Count) chaseTask = m_allChaseTasks.Find(p => p.taskConfigInfo.ID == depend[i]);

            if (chaseTask != null && chaseTask.taskData.state == (byte)EnumChaseTaskFinishState.Finish) continue;

            isTaskOn = true;
            var s = Util.GetString(295, (int)type);
            if (appendTimes > 0) tip.Append(";");
            tip.Append(s);
            if (type == TaskType.Awake || type == TaskType.Emergency) tip.Append(info.name);
            else if (type != TaskType.Count)
            {
                if (m_allTasks_Name != null && m_allTasks_Name.ContainsKey(depend[i]))
                {
                    var name = m_allTasks_Name[depend[i]];
                    tip.Append(name);
                }
            }
            appendTimes++;
        }

        if (!isTaskOn)//前置关卡都打完
        {
            if (modulePlayer.level < info.unlockLv)
                moduleGlobal.ShowMessage(Util.Format(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 38), info.unlockLv));
            else
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 36));
        }
        else
            moduleGlobal.ShowMessage(Util.Format(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 35), tip));
    }

    public static bool CheckChaseCondition(ChaseTask info)
    {
        if (null == info)
            return false;
        bool isEnough = modulePlayer.roleInfo.fatigue >= info.taskConfigInfo.fatigueCount;
        if (!isEnough)
        {
            return false;
        }
        if (info.taskType == TaskType.Difficult)
        {
            if (info.taskData.dailyOverTimes >= info.taskConfigInfo.challengeCount)
                return false;
        }
        else if (info.taskType == TaskType.Active)
        {
            ActiveTaskInfo activeInfo = moduleChase.allActiveItems.Find(p => p.taskLv == info.taskConfigInfo.level);
            return activeInfo.crossLimit > moduleChase.activeChallengeCount[activeInfo.taskLv];
        }

        return true;
    }

    public bool CanInCurType(TaskType type)
    {
        if (!moduleGuide.IsActiveFunction(15)) return false;
        switch (type)
        {
            case TaskType.Emergency: return m_emergencyList.Count > 0 && moduleGuide.IsActiveFunction(110);
            case TaskType.Easy: return m_easyTasksToMaxDic.Count > 0;
            case TaskType.Difficult: return m_diffTasksToMaxDic.Count > 0 && moduleGuide.IsActiveFunction(102);
            case TaskType.Active: return m_activeToMaxDic.Count > 0 && moduleGuide.IsActiveFunction(101);
            case TaskType.Awake:
                if (!moduleGuide.IsActiveFunction(105)) return false;
                else return moduleAwakeMatch.CanEnterList.Count > 0 || moduleAwakeMatch.CanEnterActiveList.Count > 0;
            case TaskType.Nightmare: return CanEnterNightmareList.Count > 0 && moduleGuide.IsActiveFunction(115);
            case TaskType.GaidenChapterOne:
            case TaskType.GaidenChapterTwo:
            case TaskType.GaidenChapterThree:
            case TaskType.GaidenChapterFour:
            case TaskType.GaidenChapterFive:
            case TaskType.GaidenChapterSix:
                return true;
        }
        return false;
    }

    #region 扫荡

    public void RequestMoppingUp(ushort taskId, int rTimes)
    {
        var p = PacketObject.Create<CsChaseMoppingUp>();
        p.taskId = taskId;
        p.times = (byte)rTimes;
        session.Send(p);
    }

    private void _Packet(ScChaseMoppingUp msg)
    {
        DispatchModuleEvent(ResponseMoppingUp, msg);
    }

    public static MoppingUpException CheckMoppingUp(ChaseTask info, int rTime)
    {
        if (info.taskType == TaskType.Difficult)
        {
            if (info.taskConfigInfo.challengeCount < rTime)
                return MoppingUpException.MaxNotEnough;
            if (info.canEnterTimes < rTime)
                return MoppingUpException.NoChallengeTimes;
        }

        if (moduleEquip.GetPropCount(602) < rTime)
            return MoppingUpException.NoMatrial;

        bool isEnough = modulePlayer.roleInfo.fatigue >= info.taskConfigInfo.fatigueCount * rTime;
        if (!isEnough)
            return MoppingUpException.NoEnergy;

        return MoppingUpException.None;
    }
    #endregion

}

public enum MoppingUpException
{
    None,
    NoChallengeTimes,
    NoMatrial,
    NoEnergy,
    MaxNotEnough,
}