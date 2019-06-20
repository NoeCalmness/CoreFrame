// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-06      16:40
//  * LastModify：2018-08-07      15:59
//  ***************************************************************************************************/
#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

public class Module_AwakeMatch : Module<Module_AwakeMatch>
{
    #region static functions

    /// <summary>
    /// 检测某一个任务的条件。返回一个掩码值。0 表示可做 1 体力不足 2进入次数不足 4 人数不足 8 人数过多
    /// </summary>
    /// <param name="rData"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static int CheckTask(ChaseTask data)
    {
        var mask = 0;

        if (data.taskConfigInfo.teamType == TeamType.Double && !moduleAwakeMatch.HasTeamMember)
            mask |= 4;
        else if (data.taskConfigInfo.teamType == TeamType.Single && moduleAwakeMatch.HasTeamMember)
            mask |= 8;
        //客户端都不做预检测了。因为服务器不发数据过来了
//        if (taskInfo.limitEnter)
//        {
//            //体力消耗只检测了自己。客户端只是预检测，最终由服务器去判断
//            if (taskInfo.costItem != null)
//            {
//                for (var c = 0; c < taskInfo.costItem.Length; c++)
//                {
//                    var itemPair = taskInfo.costItem[c];
//                    if (!moduleAwakeMatch.IsMaterialEnough(itemPair))
//                        mask |= 1;
//                }
//            }
//            if (moduleAwakeMatch.matchInfos != null)
//            {
//                if (data.Item2.type == AwakeTaskInfo.eAwakeTaskType.Normal)
//                {
//                    for (var c = 0; c < moduleAwakeMatch.matchInfos.Length; c++)
//                    {
//                        var p = moduleAwakeMatch.matchInfos[c];
//                        if (p.enterTimes <= 0)
//                            mask |= 2;
//                    }
//                }
//                else
//                {
//                    for (var c = 0; c < moduleAwakeMatch.matchInfos.Length; c++)
//                    {
//                        var chaseData = moduleAwakeMatch.GetTaskData(moduleAwakeMatch.matchInfos[c].roleId, data.Item1.ID);
//                        if (data.Item1.challengeCount - chaseData.dailyOverTimes <= 0)
//                            mask |= 2;
//                    }
//                }
//            }
//        }
        return mask;
    }

    private bool IsMaterialEnough(ItemPair pair)
    {
        var item = ConfigManager.Get<PropItemInfo>(pair.itemId);
        if (item == null)
            return true;

        switch (item.itemType)
        {
            case PropType.Currency:
                return modulePlayer.GetMoneyCount((CurrencySubType)item.subType) >= pair.count;
        }

        var list = moduleEquip.GetBagProps(item.itemType, item.subType);
        uint num = 0;
        if (list != null && list.Count > 0)
        {
            foreach (var m in list)
                num += m.num;
        }

        return num >= pair.count;
    }


    #endregion

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        IsOpen          = false;
        matchInfos      = null;
        StageId         = 0;
        enterTimes      = 0;
        maxEnterTimes   = 0;
        buyTimes        = 0;
        maxbuyTimes     = 0;
        LastStageId     = 0;
        lastBroadTime   = 0;
        canEnterList.Clear();
        canEnterDependList.Clear();
        canEnterActiveList.Clear();
    }

    protected override void OnModuleCreated()
    {
        base.OnModuleCreated();
        InitTaskData();
    }

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        Request_TeamTaskInfo(2);
    }

    public override void OnRootUpdate(float diff)
    {
        base.OnRootUpdate(diff);
        List<ulong> removeKey = null;
        foreach (var kv in InviteRecordDict)
        {
            kv.Value.Time -= diff;
            if (kv.Value.Time <= 0)
            {
                if (null == removeKey)
                    removeKey = new List<ulong>();
                removeKey.Add(kv.Key);
            }
        }
        if (null == removeKey)
            return;

        for (var i = 0; i < removeKey.Count; i++)
        {
            InviteRecordDict.Remove(removeKey[i]);
        }

    }

    #region public functions

    public void ResetTimesSuccess(ushort rTaskID)
    {
        var chase = canEnterList.Find(t => t.taskConfigInfo.ID == rTaskID);
        if (chase)
        {
            chase.taskData.resetTimes++;
        }
    }

    public bool CanEnter(ulong roleId, int stageId)
    {
        return GetTaskData(roleId, stageId) != null;
    }

    public PChaseTask GetTaskData(ulong roleId, int stageId)
    {
        for (var i = 0; i < matchInfos.Length; i++)
        {
            if (matchInfos[i].roleId == roleId)
            {
                var data = Array.Find(matchInfos[i].stageInfo, item => item.taskId == StageId);
                return data;
            }
        }
        return null;
    }

    public bool IsTeamMember(ulong roleId)
    {
        if (matchInfos == null || matchInfos.Length == 0)
            return false;
        for (int i = 0; i < matchInfos.Length; i++)
        {
            if (matchInfos[i].roleId == roleId)
                return true;
        }
        return false;
    }

    public PMatchProcessInfo GetMatchInfo(ulong roleId)
    {
        if (matchInfos == null || matchInfos.Length == 0)
            return null;
        for (int i = 0; i < matchInfos.Length; i++)
        {
            if (matchInfos[i].roleId == roleId)
                return matchInfos[i];
        }
        return null;
    }
    #endregion


    #region Properties

    public IReadOnlyList<ChaseTask> CanEnterList        { get { return canEnterList; } }

    public IReadOnlyList<ChaseTask> CanEnterDependList  { get { return canEnterDependList; } }

    public IReadOnlyList<ChaseTask> CanEnterActiveList  { get { return canEnterActiveList; } }

    public IReadOnlyList<Tuple<TaskInfo, AwakeTaskInfo>> DependTaskInfoList   { get { return dependTaskInfoList; } }

    public IReadOnlyDictionary<int, Tuple<TaskInfo, AwakeTaskInfo>> TaskInfoDict         { get { return taskInfoDict; } }

    public bool HasTeamMember { get { return matchInfos != null && matchInfos.Length > 1; } }

    public bool IsInTeam { get { return matchInfos != null; } }

    public ulong TeamFriend
    {
        get {
            if (HasTeamMember)
            {
                var p = Array.Find(matchInfos, (item) => item.roleId != modulePlayer.id_);
                return p?.roleId ?? 0;
            }
            return 0;
        }
    }

    public float Process { get { return CanEnterDependList.Count/(float) dependTaskInfoList.Count; } }

    public bool MasterIsCaptain
    {
        get
        {
            if (matchInfos == null || matchInfos.Length == 0)
                return false;
            return matchInfos[0].roleId == modulePlayer.id_;
        }
    }

    public bool MasterIsReady
    {
        get
        {
            if (matchInfos == null || matchInfos.Length == 0)
                return false;
            for (var i = 0; i < matchInfos.Length; i++)
            {
                if (matchInfos[i].roleId == modulePlayer.id_)
                    return matchInfos[i].state;
            }
            return false;
        }
    }

    public bool CurrentTaskIsActive { get { return CurrentTaskInfo?.Item2?.type == AwakeTaskInfo.eAwakeTaskType.Active; } }
    #endregion

    #region Events Define

    public const string Response_OpenRoom           = "OnResponseOpenRoom";
    public const string Response_StartMatch         = "OnStartMatch";
    public const string Response_SwitchStage        = "OnSwitchStage";
    public const string Response_ExitRoom           = "OnExitRoom";
    public const string Response_BuyEnterTime       = "ResponseBuyEnterTime";
    public const string Response_MatchInfo          = "ResponseMatchInfo";
    public const string Response_CancelMatch        = "ResponseCancelMatch";


    public const string Notice_ReadlyStateChange    = "OnReadlyStateChange";
    public const string Notice_MatchSuccess         = "OnMatchSuccess";
    public const string Notice_CurrentTaskChange    = "OnCurrentTaskChange";
    public const string Notice_TaskListChange       = "OnTaskListChange";
    public const string Notice_AwakeProcessChange   = "AwakeProcessChange";

    #endregion

    #region Fields

    public bool enterGame;

    public ulong roomId;

    public int enterTimes;

    public int maxEnterTimes;

    public int buyTimes;

    public int maxbuyTimes;

    public int activeBuyTimes;

    public int activeMaxbuyTimes;

    public byte process;
    public byte maxProcess;

    public bool IsOpen;
    //上一次广播的时间
    public float lastBroadTime;

    public ItemPair[] costItem;

    public class InviteRecord
    {
        public const float ValidTime = 60;
        public float    Time;
        private string   md5;

        public string MD5
        {
            get { return md5; }
            set { md5 = value; Time = ValidTime; }
        }

        public InviteRecord(string rMd5)
        {
            MD5 = rMd5;
        }
    }
    public Dictionary<ulong, InviteRecord> InviteRecordDict = new Dictionary<ulong, InviteRecord>();

    /// <summary>
    /// 是否是主动退出
    /// </summary>
    private bool isActiveExit;
    /// <summary>
    /// 是否需要广播
    /// </summary>
    public bool IsNeedBroad;

    public int m_stageId;

    public int StageId
    {
        get { return m_stageId; }
        set
        {
            if (m_stageId != value)
            {
                m_stageId = value;
                if (m_stageId == 0)
                {
                    CurrentTask = null;
                    return;
                }

                var t = ConfigManager.Get<TaskInfo>(m_stageId);
                if (t == null)
                {
                    Logger.LogError("don't find task in taskinfo, taskid={0}", m_stageId);
                    return;
                }
                var type = moduleChase.GetCurrentTaskType(t);
                if (type == TaskType.Awake)
                {
                    CurrentTask = CanEnterList.Find(item => item.taskConfigInfo.ID == m_stageId);
                    if (TaskInfoDict.ContainsKey(m_stageId)) CurrentTaskInfo = TaskInfoDict[m_stageId];
                }
                else if (type == TaskType.Nightmare) CurrentTask = moduleChase.CanEnterNightmareList.Find(item => item.taskConfigInfo.ID == m_stageId);
                else if (type == TaskType.Emergency) CurrentTask = moduleChase.emergencyList.Find(item => item.taskConfigInfo.ID == m_stageId);

                if (CurrentTask == null)
                {
                    Logger.LogError($"未能找到相关任务。type={type}  stageid = {m_stageId}");
                }
            }
        }
    }

    public bool CanBroad {
        get { return lastBroadTime <= 0 || BroadCountDown <= 0; }
    }

    public float BroadCountDown
    {
        get
        {
            if (lastBroadTime <= 0)
                return 0;
            return Mathf.Max(0, GeneralConfigInfo.defaultConfig.teamBroadCD + lastBroadTime - Time.realtimeSinceStartup);
        }
    }

    public int LastStageId { get; private set; }

    public ChaseTask CurrentTask { get; private set; }

    public Tuple<TaskInfo, AwakeTaskInfo> CurrentTaskInfo { get; private set; }

    public PMatchProcessInfo[] matchInfos;

    public PMatchInfo[] fightMatchInfos;

    private readonly List<ChaseTask> canEnterList = new List<ChaseTask>();

    private readonly List<ChaseTask> canEnterDependList = new List<ChaseTask>();

    private readonly List<ChaseTask> canEnterActiveList = new List<ChaseTask>();

    private readonly List<Tuple<TaskInfo, AwakeTaskInfo>> dependTaskInfoList = new List<Tuple<TaskInfo, AwakeTaskInfo>>();
            
    private readonly Dictionary<int, Tuple<TaskInfo, AwakeTaskInfo>> taskInfoDict = new Dictionary<int, Tuple<TaskInfo, AwakeTaskInfo>>();

    #endregion

    #region Request

    /// <summary>
    /// 请求组队任务信息 type=1紧急 2,觉醒 3,噩梦
    /// </summary>
    /// <param name="_type"></param>
    /// <returns></returns>
    public void Request_TeamTaskInfo(byte _type)
    {
        if (_type == 1 && moduleChase.emergencyList.Count > 0) return;
        else if (_type == 2 && CanEnterList.Count > 0) return;
        else if (_type == 3 && moduleChase.CanEnterNightmareList.Count > 0) return;

        var msg = PacketObject.Create<CsTeamPveTaskInfo>();
        msg.type = _type;
        session.Send(msg);
    }

    public void RequestBuyEnterTime()
    {
        if (null == CurrentTask) return;

        if (buyTimes > maxbuyTimes)
            return;

        var msg = PacketObject.Create<CsTeamPveBuyEnterTime>();
        msg.stageId = StageId;
        msg.roomId = roomId;
        session.Send(msg);

        moduleGlobal.LockUI(string.Empty, 0.5f);
    }

    public void Request_EnterRoom(bool enter, ChaseTask task)
    {
        if (task == null)
            return;

        if (!enter && !moduleGlobal.CheckSourceMd5(false))
        {
            var ct = ConfigManager.Get<ConfigText>(25);
            Window_Alert.ShowAlertDefalut(ct[2], () =>
            {
                Internal_RequestEnterRoom(enter, task);
            }, Game.Quit, ct[3], ct[4], false);
            return;
        }

        //if (task.Equals(CurrentTask))
        //    return;

        Internal_RequestEnterRoom(enter, task);
    }

    private void Internal_RequestEnterRoom(bool enter, ChaseTask task)
    {
        StageId = task.taskConfigInfo.ID;
        DispatchModuleEvent(Notice_CurrentTaskChange);

        var msg = PacketObject.Create<CsTeamPveEnterRoom>();
        msg.enterRoom = enter;
        msg.stageId = StageId;
        session.Send(msg);

        IsNeedBroad = !enter;

        moduleGlobal.LockUI(string.Empty, 0.5f);
    }

    public void Request_Agree( int stageId, ulong roleId, ulong room, bool invited, string md5)
    {
        if (roleId == modulePlayer.id_)
        {
            moduleGlobal.ShowMessage(9809);
            return;
        }

        if (!string.IsNullOrEmpty(AssetBundles.AssetManager.dataHash) && md5 != AssetBundles.AssetManager.dataHash)
        {
            moduleGlobal.CheckSourceMd5();
            return;
        }

        var msg = PacketObject.Create<CsTeamPveAgree>();
        msg.roleId = roleId;
        msg.stageId = stageId;
        msg.roomId = room;
        msg.invited = invited;
        msg.captainMD5 = md5;
        session.Send(msg);

        moduleGlobal.LockUI(string.Empty, 0.5f);
    }

    public void Request_ExitRoom(ulong roleId)
    {
        if (CurrentTask == null)
            return;

        var msg = PacketObject.Create<CsTeamPveExitRoom>();
        msg.roleId = roleId;
        msg.roomId = roomId;
        msg.stageId = StageId;
        session.Send(msg);

        isActiveExit = true;

        moduleGlobal.LockUI(string.Empty, 0.5f);
    }

    public void Request_OpenRoom(bool open)
    {
        var msg = PacketObject.Create<CsTeamPveOpenRoom>();
        msg.open = open;
        msg.roomId = roomId;
        msg.stageId = CurrentTask.taskConfigInfo.ID;
        session.Send(msg);

        moduleGlobal.LockUI(string.Empty, 0.5f);
    }

    public void Request_Broad()
    {
        if (!CanBroad)
            return;

        var msg = PacketObject.Create<CsBroadAssist>();
        msg.stageName = CurrentTask.taskConfigInfo.name;
        session.Send(msg);
        lastBroadTime = Time.realtimeSinceStartup;
    }

    public void Request_Readly(bool readly)
    {
        var msg = PacketObject.Create<CsTeamPveReady>();
        msg.roleId = modulePlayer.id_;
        msg.isReady = readly;
        msg.roomId = roomId;
        msg.stageId = StageId;
        session.Send(msg);

        moduleGlobal.LockUI(string.Empty, 0.5f);
    }

    public bool Request_SwitchStage(int stageId)
    {
        if (StageId == stageId) return false;
        var msg = PacketObject.Create<CsTeamPveSwitchStage>();
        msg.stageId = stageId;
        msg.nowStageId = StageId;
        msg.roomId = roomId;
        session.Send(msg);

        moduleGlobal.LockUI(string.Empty, 0.5f);
        return true;
    }

    public void Request_CancelMatch()
    {
        if (CurrentTask == null)
            return;
        var msg = PacketObject.Create<CsTeamPveCancelMatch>();
        msg.stageId = StageId;
        session.Send(msg);

        moduleGlobal.LockUI(string.Empty, 0.5f);
    }

    #endregion

    #region Response

    private void _Packet(ScTeamPveCancelMatch msg)
    {
        moduleGlobal.UnLockUI();
        if (msg.result == 0)
        {
            ClearMatchInfo();
        }

        DispatchModuleEvent(Response_CancelMatch, msg);
    }


    private void _Packet(ScTeamPveTaskInfo rInfo)
    {
        if (rInfo.type != 2) return;
        process        = rInfo.nowProcess;
        maxProcess     = rInfo.maxProcess;
        buyTimes       = rInfo.buyTimes;
        maxbuyTimes    = rInfo.maxbuyTimes;
        maxEnterTimes  = rInfo.maxenterTimes;
        enterTimes     = rInfo.enterTimes;

        if (costItem == null)
            costItem = new ItemPair[rInfo.cost.Length];
        else
            Array.Resize(ref costItem, rInfo.cost.Length);
        for (var i = 0; i < costItem.Length; i++)
            costItem[i] = new ItemPair() {itemId = rInfo.cost[i].itemTypeId, count = rInfo.cost[i].price};

        HandleTasks(rInfo.stageInfo);

        DispatchModuleEvent(Response_MatchInfo);
    }

    private void _Packet(ScAwakeProcessChange msg)
    {
        moduleGlobal.UnLockUI();
        process     = msg.nowProcess;
        maxProcess  = msg.maxProcess;

        DispatchEvent(Notice_AwakeProcessChange);
    }

    private void _Packet(ScTeamPveBuyEnterTime msg)
    {
        moduleGlobal.UnLockUI();
        if (msg.result == 0)
        {
            if (msg.roleId.Equals(modulePlayer.id_))
            {
                ChaseTask task = null;
                var info = ConfigManager.Get<TaskInfo>(msg.stageId);
                if (info == null)
                {
                    Logger.LogError("there is no id={0} in taskinfo!", msg.stageId);
                    return;
                }
                var type = moduleChase.GetCurrentTaskType(info);
                if (type == TaskType.Awake) task = CanEnterActiveList.Find(item => item.taskConfigInfo.ID == msg.stageId);
                else if (type == TaskType.Nightmare) task = moduleChase.CanEnterNightmareList.Find(item => item.taskConfigInfo.ID == msg.stageId);

                if (task != null)
                    task.taskData.resetTimes = msg.times;
                else
                {
                    buyTimes = msg.times;
                    enterTimes = msg.enterTimes;
                }
            }
            
            for (var i = 0; i < matchInfos.Length; i++)
            {
                if (matchInfos[i].roleId.Equals(msg.roleId))
                {
                    var data = GetTaskData(msg.roleId, msg.stageId);

                    if (data != null)
                    {
                        var info = ConfigManager.Get<TaskInfo>(msg.stageId);
                        if (info)
                        {
                            var type = moduleChase.GetCurrentTaskType(info);
                            if(type == TaskType.Awake)
                            {
                                data.resetTimes = msg.times;
                                if (!CurrentTaskIsActive)
                                    enterTimes = msg.enterTimes;
                            }
                            else if(type == TaskType.Nightmare)
                            {
                                data.resetTimes = msg.times;
                            }
                        }
                    }
                    else
                        matchInfos[i].enterTimes = msg.enterTimes;
                }
            }
        }
        
        DispatchModuleEvent(Response_BuyEnterTime, msg);
    }

    private void _Packet(ScTeamPveExitRoom msg)
    {
        moduleGlobal.UnLockUI();

        if (msg.result == 0)
        {
            if (CurrentTask != null && CurrentTask.taskType != TaskType.Awake)
            {
                if (CurrentTask.taskType == TaskType.Emergency && moduleChase.isAddNewEmer)
                {
                    var chase = moduleChase.emergencyList.Find(p => p.taskConfigInfo.difficult == CurrentTask.taskConfigInfo.difficult + 1);
                    if (chase != null) moduleChase.SetTargetTask(chase);
                    moduleChase.isAddNewEmer = false;
                }
                else
                    moduleChase.SetTargetTask(CurrentTask);
            }

            ClearMatchInfo();
            IsOpen = false;

            if (!isActiveExit)
                moduleGlobal.ShowMessage((int) TextForMatType.AwakeStage, 8);
            isActiveExit = false;
        }
        DispatchModuleEvent(Response_ExitRoom, msg);
    }

    private void _Packet(ScTeamPveAgree msg)
    {
        moduleGlobal.UnLockUI();

        if (msg.result == 10)
        {
            moduleGlobal.CheckSourceMd5();
            return;
        }

        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9809, msg.result);
            return;
        }
        StageId = msg.stageId;
        Window.ShowAsync<Window_TeamMatch>();
        moduleChat.HideChatWindow();
    }

    private void _Packet(ScTeamPveMatchSuccess msg)
    {
        moduleGlobal.UnLockUI();

        if (msg.result == 10)
        {
            moduleGlobal.CheckSourceMd5();
            return;
        }
        else if (msg.result == 0)
        {
            roomId = msg.roomId;
            StageId = msg.stageId;
            Array.Resize(ref matchInfos, msg.teamFriend.Length);
            for (var i = 0; i < msg.teamFriend.Length; i++)
            {
                matchInfos[i] = msg.teamFriend[i].Clone();
                if (matchInfos[i].roleId == modulePlayer.id_)
                {
                    if (CurrentTask?.taskType == TaskType.Awake)
                        HandleTasks(matchInfos[i].stageInfo);
                    else if(CurrentTask?.taskType == TaskType.Nightmare)
                    {
                        var task = Array.Find(matchInfos[i].stageInfo, item => item.taskId == CurrentTask.taskConfigInfo.ID);
                        if(task != null)
                        {
                            var t = moduleChase.CanEnterNightmareList.Find(item => item.taskConfigInfo.ID == CurrentTask.taskConfigInfo.ID);
                            if (t != null) t.taskData = task;
                        }
                    }
                    if (CurrentTask != null)
                    {
                        var _task = matchInfos[i].stageInfo.Find((p) => p.taskId == CurrentTask.taskData.taskId);
                        if (_task != null) CurrentTask.taskData = _task;
                    }

                    enterTimes = matchInfos[i].enterTimes;
                }
            }
        }
        else
        {
            ClearMatchInfo();
        }

        DispatchModuleEvent(Notice_MatchSuccess, msg);
    }

    private void _Packet(ScTeamPveStartMatch msg)
    {
        moduleGlobal.UnLockUI();

        DispatchModuleEvent(Response_StartMatch, msg);
    }

    private void _Packet(ScTeamPveSwitchStage msg)
    {
        if (msg.result == 0)
        {
            StageId = msg.stageId;
        }

        DispatchModuleEvent(Response_SwitchStage, msg);
        moduleGlobal.UnLockUI();
    }


    private void _Packet(ScChaseTaskUnlock p)
    {
        PChaseTask[] tasks = null;
        p.chaseList.CopyTo(ref tasks);

        for (var i = 0; i < tasks?.Length; i++)
        {
            TaskInfo ti = ConfigManager.Get<TaskInfo>(tasks[i].taskId);
            TaskType _type = moduleChase.GetCurrentTaskType(ti);
            if (_type != TaskType.Awake)
                continue;
            if (!canEnterList.Exists(item => item.taskConfigInfo.ID == ti.ID))
                canEnterList.Add(ChaseTask.Create(tasks[i]));
        }

        ClassifyList();
    }

    private void _Packet(ScTeamPveOpenRoom msg)
    {
        moduleGlobal.UnLockUI();

        if (msg.result == 0)
        {
            IsOpen = msg.open;
        }
        DispatchModuleEvent(Response_OpenRoom, msg);
    }

    private void _Packet(ScTeamPveReady msg)
    {
        moduleGlobal.UnLockUI();

        if (msg.result == 0)
        {
            for (var i = 0; i < matchInfos.Length; i++)
            {
                if (matchInfos[i] != null && matchInfos[i].roleId == msg.roleId)
                    matchInfos[i].state = msg.isReady;
            }
        }
        else
        {
            if (msg.result == 8)
            {
                moduleGlobal.OpenExchangeTip(TipType.BuyEnergencyTip, true);
                return;
            }
            if (msg.result != 0)
            {
                moduleGlobal.ShowMessage(9808, msg.result);
                return;
            }
        }
        DispatchModuleEvent(Notice_ReadlyStateChange, msg);
    }

    private void _Packet(ScSyncResult msg)
    {
        if (msg.result == 1)
        {
            if (Level.current.isPvP)
                FightStatistic.AddPvpAsyncFightTimes();
            else if(Level.current.isPvE)
                FightStatistic.AddPveAsyncFightTimes();
            FightRecordManager.EndRecord(true, true, msg.roomId.ToString());
        }
    }

    #endregion

    #region private functions

    private void HandleTasks(PChaseTask[] tasks)
    {
        if (tasks == null)
            return;
        canEnterList.Clear();
        for (var i = 0; i < tasks.Length; i++)
        {
            var t = ConfigManager.Get<TaskInfo>(tasks[i].taskId);
            if (t == null) continue;
            var type = moduleChase.GetCurrentTaskType(t);
            if (type != TaskType.Awake) continue;
            canEnterList.Add(ChaseTask.Create(tasks[i], t));
        }

        ClassifyList();

        //重新生成了ChaseTask对象，需要重新更新当前任务对象
        var ta = ConfigManager.Get<TaskInfo>(StageId);
        if (ta != null)
        {
            var type = moduleChase.GetCurrentTaskType(ta);
            if (type == TaskType.Awake)          CurrentTask = CanEnterList.Find(item => item.taskConfigInfo && item.taskConfigInfo.ID == StageId);
            else if (type == TaskType.Nightmare) CurrentTask = moduleChase.CanEnterNightmareList.Find(item => item.taskConfigInfo.ID == StageId);
            else if (type == TaskType.Emergency) CurrentTask = moduleChase.emergencyList.Find(item => item.taskConfigInfo.ID == StageId);
        }
    }

    private void ClassifyList()
    {
        canEnterDependList.Clear();
        canEnterActiveList.Clear();
        foreach (var t in canEnterList)
        {
            if (!t.taskConfigInfo)
                continue;
            if (dependTaskInfoList.Exists(item => item.Item1.ID == t.taskConfigInfo.ID))
            {
                LastStageId = t.taskConfigInfo.ID > LastStageId ? t.taskConfigInfo.ID : LastStageId;
                canEnterDependList.Add(t);
            }
            else
                canEnterActiveList.Add(t);
        }
    }

    private void InitTaskData()
    {
        dependTaskInfoList.Clear();
        taskInfoDict.Clear();

        if(null == dependTask)
            dependTask = new AwakeTaskInfo() { type = AwakeTaskInfo.eAwakeTaskType.Normal, taskLv = 120};
        {
            var list = ConfigManager.FindAll<TaskInfo>(t => t.level == dependTask.taskLv);
            foreach (var t in list)
            {
                var data = Tuple.Create(t, dependTask);
                dependTaskInfoList.Add(data);
                taskInfoDict.Add(t.ID, data);
            }
        }

        if(activeTask == null)
            activeTask = new AwakeTaskInfo() {type = AwakeTaskInfo.eAwakeTaskType.Active, taskLv = 121};
        {
            var list = ConfigManager.FindAll<TaskInfo>(t => t.level == activeTask.taskLv);
            foreach (var t in list)
                taskInfoDict.Add(t.ID, Tuple.Create(t, activeTask));
        }
    }

    public void ClearMatchInfo()
    {
//        Logger.NLogError("ClearmatchInfo");
        StageId = 0;
        IsOpen = false;
        matchInfos = null;
        moduleChat.team_chat_record.Clear();
    }
    #endregion

    public static AwakeTaskInfo dependTask = null;
    public static AwakeTaskInfo activeTask = null;
}


public class AwakeTaskInfo
{
    public enum eAwakeTaskType
    {
        Normal = 1,
        Active,
    }
    public eAwakeTaskType type;   //0-活动任务。 1-觉醒关卡
    public int taskLv;
}