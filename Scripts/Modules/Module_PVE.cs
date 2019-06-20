/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Lee
 * Version:  0.1
 * Created:  2017-07-21
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class Module_PVE : Module<Module_PVE>
{
    public static int TEST_SCENE_EVENT_ID = -1000;

    /// <summary>
    /// 游戏结算事件  argument: [PReward]
    /// </summary>
    public const string EventGameOverSettlement     = "EventGameOverSettlement";
    /// <summary>
    /// 玩家复活  argument: [bool] issuccess
    /// </summary>
    public const string EventRebornState            = "EventRebornState";
    /// <summary>
    /// 怪物箭头提示  argument: [EnumMonsterTipArrow]
    /// </summary>
    public const string EventMonsterTip             = "EventMonsterTip";
    /// <summary>
    /// 转移场景  argument: [bool] isbegin
    /// </summary>
    public const string EventTransportSceneToUI     = "EventTransportSceneToUI";
    /// <summary>
    /// 转移场景  argument: [int] -1 = left; 0 = in scene; 1 = right
    /// </summary>
    public const string EventTriggerTransportTip = "EventTriggerTransportTip";
    /// <summary>
    /// boss镜头动画  argument: null
    /// </summary>
    public const string EventBossCameraAnimWhiteFadeIn = "EventBossCameraAnimWhiteFadeIn";
    /// <summary>
    /// 收到boss奖励信息 argument: PReward
    /// </summary>
    public const string EventRecvBossReward = "EventRecvBossReward";
    /// <summary>
    /// 展示boss奖励信息 argument: PReward
    /// </summary>
    public const string EventDisplayBossReward = "EventDisplayBossReward";

    public StageInfo currrentStage { get; private set; }
    public ushort stageId { get { return currrentStage == null ? ushort.MinValue : (ushort)currrentStage.ID; } }
    public int stageEventId { get { return TEST_SCENE_EVENT_ID > 0 ? TEST_SCENE_EVENT_ID : currrentStage == null ?  0 : currrentStage.sceneEventId; } }
    public double reletivePos { get { return currrentStage == null ? 0f : currrentStage.playerPos; } }

    /// <summary>
    /// 是否是首次进入关卡（还未通关的话，都算是首次进入）
    /// </summary>
    public bool isFirstEnterStage { get; private set; } = false;

    /// <summary>
    /// 是否是第一次进入关卡（只要进入过关卡一次就失效）
    /// </summary>
    public bool enterForFirstTime { get; private set; } = false;
    
    /// <summary>
    /// 最大可复活次数
    /// </summary>
    public sbyte maxReviveTimes { get; private set; }

    /// <summary>
    /// 当前复活的次数
    /// </summary>
    public int currentReviveTimes { get; set; }

    public bool canRevive { get { return currentReviveTimes < maxReviveTimes; } }

    /// <summary>
    /// 复活的消耗
    /// </summary>
    public Dictionary<int, PItem2> rebornItemDic { get; private set; } = new Dictionary<int, PItem2>();


    private PVEReOpenPanel m_reopenPanel = PVEReOpenPanel.None;
    /// <summary>
    /// 从PVE界面退出来后需要打开的面板
    /// </summary>
    public PVEReOpenPanel reopenPanelType
    {
        get { return m_reopenPanel; }
        set
        {
            m_reopenPanel = value;
        }
    }

    public string GetReopenPanelName(PVEReOpenPanel type)
    {
        switch (m_reopenPanel)
        {
            case PVEReOpenPanel.ChasePanel:    return Game.GetDefaultName<Window_Chase>();
            case PVEReOpenPanel.RunePanel:     return Game.GetDefaultName<Window_RuneStart>();
            case PVEReOpenPanel.EquipPanel:    return Game.GetDefaultName<Window_Equip>();
            case PVEReOpenPanel.PetPanel:      return Game.GetDefaultName<Window_Sprite>();
            case PVEReOpenPanel.UnionBoss:     return Game.GetDefaultName<Window_Unionboss>();
            case PVEReOpenPanel.Awake:         return Game.GetDefaultName<Window_Awake>();
            case PVEReOpenPanel.Dating:        return Game.GetDefaultName<Window_NPCDating>();
            case PVEReOpenPanel.Labyrinth:
            case PVEReOpenPanel.Borderlands:
            case PVEReOpenPanel.None:
            case PVEReOpenPanel.Count:
            default: return null;
        }
    }

    public PAssistMemberInfo assistMemberInfo;

    /// <summary>
    /// 玩家需要触发Buff
    /// </summary>
    public List<int> labyrinthBuffPropIds { get; private set; } = new List<int>();
    public Dictionary<int, PropToBuffInfo> propToBuffDic = new Dictionary<int, PropToBuffInfo>();

    /// <summary>
    /// 统计游戏内的数据集合
    /// </summary>
    public float[] pveGameDatas;

    /// <summary>
    /// 是否使用单机的PVE模式
    /// </summary>
    public bool isStandalonePVE
    {
        get { return !isTeamMode || moduleTeam.onlineMembers == null || moduleTeam.onlineMembers.Count <= 1;  }
    }

    public bool duringTranspostScene { get; private set; }

    /// <summary>
    /// boss的预览奖励
    /// </summary>
    public PItem2[] bossRewards;
    private bool m_hasDisplayBossReward = false;
    
    /// <summary>
    /// 所有场景行为事件里面包含的怪物死亡信息
    /// </summary>
    public Dictionary<int, int> totalMonsterDeathDic { get; private set; } = new Dictionary<int, int>();

    #region pve 结算数据
    /// <summary>
    /// 结束时的奖励
    /// </summary>
    public PReward settlementReward;

    /// <summary>
    /// 本次战斗增加的友情点
    /// </summary>
    public int addFriendPoint;
    /// <summary>
    /// 本次战斗增加的友情点
    /// </summary>
    public int addNpcPoint;

    /// <summary>
    /// 结束时的星级显示
    /// </summary>
    public int settlementStar { get; private set; } = 0;

    /// <summary>
    /// 结算时显示星级条件的任务
    /// </summary>
    public TaskInfo settlementTask { get; private set; } = null;

    /// <summary>
    /// 是否需要显示星级奖励
    /// </summary>
    public bool needShowStar { get { return settlementTask != null && modulePVE.settlementTask.taskStarDetails?.Length > 0; } }

    /// <summary>
    /// 是否发送的成功消息
    /// </summary>
    public bool isSendWin { get; set; } = false;
    public PVEOverState lastSendState = PVEOverState.None;
    #endregion

    #region auto battle
    private const string AUTO_BATTLE = "AUTOBATTLE";

    public bool recordAutoBattle { get; set; } = false;

    #endregion

    #region team auto battle

    private bool m_gmAutoBattle = false;
    public bool gmAutoBattle
    {
        get { return m_gmAutoBattle; }
        set
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            m_gmAutoBattle = value;
#else
            m_gmAutoBattle = false;
#endif
        }
    }

    public bool useTeamAutoBattle
    {
        get
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            return gmAutoBattle & isTeamMode;
            #else
            return false;
            #endif
        }
    }

    #endregion

    protected override void OnGameStarted()
    {
        EventManager.AddEventListener(Events.SCENE_DESTROY, OnSceneDestroy);

        var finishTrain = PlayerPrefs.HasKey("FinishTrain");
        if (finishTrain) return;

        PrepareTrainLevel();
    }

    private void PrepareTrainLevel()
    {
        if (Game.started)
        {
            Logger.LogException("Module_PVE:: PrepareTrainLevel must before Game.started");
            return;
        }

        if (!OnPveStartInit(GeneralConfigInfo.sshowLevel, PVEReOpenPanel.None))
        {
            Logger.LogError("Could not load show level <b><color=#DDFF33>[{0}]</color></b>", GeneralConfigInfo.sshowLevel);
            return;
        }

        Game.startLevel = currrentStage.levelInfoId;
    }

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        ClearPveCache();
        currrentStage = null;
        isTeamMode = false;
    }

    public void OnScRoleStartChase(ScChaseTaskStart chaseTasks,bool chase=true)
    {
        moduleMatch.isbaning = false;
        assistMemberInfo = chaseTasks.assistInfo;
        if (chase)
        {
            OnPVEStart(chaseTasks.stageId, PVEReOpenPanel.ChasePanel);
        }
        else
        {
            OnPVEStart(chaseTasks.stageId, PVEReOpenPanel.PetPanel);
        }
        
        isFirstEnterStage = chaseTasks.isFirst != 0;
        isFirstEnterStage = TEST_SCENE_EVENT_ID <= 0 ? isFirstEnterStage : true;

        int enterTimes = GetChaseTaskEnterTimes(chaseTasks.stageId);
        enterForFirstTime = TEST_SCENE_EVENT_ID <= 0 ? enterTimes == 0 : true;

        RefreshRebornData(chaseTasks.rebornTime, chaseTasks.rebornItem);
    }

    private int GetChaseTaskEnterTimes(int stageId)
    {
        int times = -1;

        //从上次追捕任务开始检查
        if (moduleChase.lastStartChase != null && moduleChase.lastStartChase.stageId == stageId) times = moduleChase.lastStartChase.enterTimes;

        //如果未从当前追捕的任务找到,则从所有的追捕任务列表中检查
        if (times < 0 && moduleChase.allChaseTasks != null && moduleChase.allChaseTasks.Count > 0)
        {
            var t = moduleChase.allChaseTasks.Find<ChaseTask>(o => o.stageId == stageId);
            if (t != null) times = t.enterTimes;
        }

        //如果所有追捕相关的都不能找到该关卡，则从觉醒副本中查询
        if (times < 0 && moduleAwakeMatch.CanEnterList != null && moduleAwakeMatch.CanEnterList.Count > 0)
        {
            var t = moduleAwakeMatch.CanEnterList.Find(o=>o.stageId == stageId);
            if (t != null) times = t.enterTimes;
        }

        return times;
    }

    private void RefreshRebornData(sbyte rebornTimes, PItem2[] rebornItems)
    {
        maxReviveTimes = rebornTimes;
        PItem2[] items = null;
        rebornItems.CopyTo(ref items);
        if (items != null)
        {
            for (int i = 0; i < items.Length; i++)
            {
                rebornItemDic.Add(i, items[i]);
            }
        }
    }

    public void OnScRoleStartBordland(int stageId,uint overTimes)
    {
        OnPVEStart(stageId, PVEReOpenPanel.Borderlands);
        isFirstEnterStage = overTimes <= 0;
        enterForFirstTime = false;
    }

    public void OnScLabyrinthChallenge(int stageId,short[] buffProps, uint overTimes)
    {
        OnPVEStart(stageId, PVEReOpenPanel.Labyrinth);
        isFirstEnterStage = overTimes <= 0;
        enterForFirstTime = false;
        if (buffProps != null && buffProps.Length > 0)
        {
            for (int i = 0; i < buffProps.Length; i++)
            {
                labyrinthBuffPropIds.Add(buffProps[i]);
            }
        }

        InitBuffDic();
    }

    public void OnScStartAwake(ChaseTask task)
    {
        if (task == null || task.taskData == null)
        {
            Logger.LogError("start a awake task is wrong,task data is null");
            return;
        }

        TaskInfo t = ConfigManager.Get<TaskInfo>(task.taskData.taskId);
        if(!t)
        {
            Logger.LogError("start a awake task is wrong,task data is null");
            return;
        }

        int stageId = t.stageId;
        OnPVEStart(stageId, PVEReOpenPanel.Awake);
        isFirstEnterStage = task.taskData?.dailyOverTimes == 0;
        enterForFirstTime = task.taskData?.times == 0;
    }

    public void OnPVEStart(int stageId, PVEReOpenPanel type, bool teamMode = false)
    {
        if (!OnPveStartInit(stageId, type, teamMode)) return;

        Game.LoadLevel(currrentStage.levelInfoId);
    }

    private bool OnPveStartInit(int stageId, PVEReOpenPanel type, bool teamMode = false)
    {
        currrentStage = ConfigManager.Get<StageInfo>(stageId);
        if (!currrentStage)
        {
            Logger.LogError($"Start PVE failed, could not load stage <b>[{stageId}]</b>, teamMode:<b>{teamMode}</b>");
            return false;
        }

        isTeamMode = teamMode;

        InitPveGameData();
        GetLocalAutoBattleRecord();
        ClearPveCache();

        isFirstEnterStage = TEST_SCENE_EVENT_ID <= 0 ? isFirstEnterStage : true;
        enterForFirstTime = TEST_SCENE_EVENT_ID <= 0 ? false : true;

        reopenPanelType = type;

        Logger.LogDetail("PVE stage start! stage = {0}, sceneEventId = {1}", stageId, stageEventId);

        return true;
    }

    private void ClearPveCache()
    {
        moduleLabyrinth.isSneakSuccess = false;
        isSendWin = false;
        lastSendState = PVEOverState.None;
        isFirstEnterStage = false;
        enterForFirstTime = false;
        maxReviveTimes = -1;
        rebornItemDic.Clear();
        labyrinthBuffPropIds.Clear();
        propToBuffDic.Clear();
        currentReviveTimes = 0;
        settlementStar = 0;
        addFriendPoint = 0;
        addNpcPoint = 0;
        settlementReward = null;
        settlementTask = null;
        duringTranspostScene = false;
        totalMonsterDeathDic.Clear();
    }

    private void OnSceneDestroy(Event_ e)
    {
        if (!isTeamMode) return;

        var l = e.sender as Level_PveTeam;
        if (!l) return;

        moduleTeam.QuitTeam(true);
        isTeamMode = false;

        Logger.LogInfo("Quit team mode");
    }

    /// <summary>
    /// 缓冲配置表中的道具ID和buffid的映射
    /// </summary>
    private void InitBuffDic()
    {
        List<PropToBuffInfo> list = ConfigManager.GetAll<PropToBuffInfo>();
        for (int i = 0; i < list.Count; i++)
        {
            PropToBuffInfo item = list[i];
            if (propToBuffDic.ContainsKey(item.propId))
            {
                Logger.LogError("id = {0},prop id = {0} has repitition data",item.ID,item.propId);
                continue;
            }

            propToBuffDic.Add(item.propId,item);
        }
    }

    public List<string> FillLabyrithBuffAsset()
    {
        List<string> list = new List<string>();
        if (reopenPanelType == PVEReOpenPanel.Labyrinth && propToBuffDic != null && labyrinthBuffPropIds != null)
        {
            for (int i = 0; i < labyrinthBuffPropIds.Count; i++)
            {
                PropToBuffInfo info = propToBuffDic.Get(labyrinthBuffPropIds[i]);
                if (!info) continue;

                if (info.buffId > 0)
                {
                    BuffInfo buff = ConfigManager.Get<BuffInfo>(info.buffId);
                    if (buff) list.AddRange(buff.GetAllAssets());
                }

                if (!string.IsNullOrEmpty(info.effect)) list.Add(info.effect);
            }
        }

        return list;
    }

    public PropItemInfo GetTrapPropInfo()
    {
        if (labyrinthBuffPropIds == null || labyrinthBuffPropIds.Count == 0)
            return null;

        for (int i = 0; i < labyrinthBuffPropIds.Count; i++)
        {
            PropItemInfo info = ConfigManager.Get<PropItemInfo>(labyrinthBuffPropIds[i]);
            if (info.itemType == PropType.LabyrinthProp && info.subType == (int)LabyrinthPropSubType.TrapProp)
                return info;
        }

        return null;
    }

    public void SendPVEState(PVEOverState state, Creature player)
    {
        if (Level.current is Level_Train)
        {
            return;
        }
        //胜利或者失败就设置该关卡可以被销毁了
        if (Level.current && Level.current is Level_PVE && state != PVEOverState.RoleDead) (Level.current as Level_PVE).SetLevelBehaviourCanDestory();
        byte anger = (byte)Mathf.RoundToInt(player.rageRate * 100f);

        //如果是组队模式，则需要发送EndRequest
        modulePVE.SendPVEState(state, player.healthRate * 100, anger);
    }

    public void SendPVEAwake()
    {
        CsAwakeUse p = PacketObject.Create<CsAwakeUse>();
        p.use = true;
        session.Send(p);
    }

    public void SendPVEState(PVEOverState state,float health = 0,byte anger = 0)
    {
        Logger.LogDetail("pve end stats is {0}", state);
        isSendWin = state == PVEOverState.Success;
        lastSendState = state;

        if (isTeamMode)
        {
            //如果挑战失败，就退出组队房间 
            if (state == PVEOverState.GameOver) moduleAwakeMatch.Request_ExitRoom(modulePlayer.id_);
            moduleTeam.SendEndRequest(state);
        }
        else
        {
            switch (reopenPanelType)
            {
                case PVEReOpenPanel.ChasePanel:
                case PVEReOpenPanel.Awake:
                    SendChasePVEState(state);
                    break;

                case PVEReOpenPanel.PetPanel:
                    SendSpritePVEState(state);
                    break;

                case PVEReOpenPanel.UnionBoss:
                    moduleUnion.SendGetEnd(state);
                    break;

                case PVEReOpenPanel.Borderlands:
                    if (state == PVEOverState.Success) moduleBordlands.SendFightWin();
                    else if (state == PVEOverState.GameOver)
                    {
                        moduleBordlands.SendFightLose();
                        //暂时本地结算
                        moduleBordlands.OnBordlandStageFail();
                    }
                    break;

                case PVEReOpenPanel.Labyrinth:
                    moduleLabyrinth.SendChallengeLabyrinthOver(health, anger, state);
                    break;
                case PVEReOpenPanel.Dating:
                    moduleNPCDating.ChaseFinish(state);
                    SendChasePVEState(state);
                    break;
            }
        }
    }

    #region 宠物关卡

    //发送pve结束请求
    public void SendSpritePVEState(PVEOverState state)
    {
        CsSpriteTaskOver chaseMsg = PacketObject.Create<CsSpriteTaskOver>();
        chaseMsg.overState = (sbyte)state;
        chaseMsg.overdata = modulePVE.GetPveDatas();
        session.Send(chaseMsg);
    }

    void _Packet(ScSpriteTaskOver roleOver)
    {
        if (roleOver.result == 0 && roleOver.reward != null)
        {
            TaskInfo info = moduleChase.lastStartChase == null ? null : moduleChase.lastStartChase.taskConfigInfo;
            SetPVESettlement(roleOver.reward, info, roleOver.overStar);
            
            moduleChase.OnChaseTaskFinish(roleOver.overStar);
        }
        else if (roleOver.result != 0) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(402, roleOver.result));

        moduleChase.isPetLevelTask = false;
        DispatchModuleEvent(EventGameOverSettlement, settlementReward);
    }
    #endregion

    public void SendChasePVEState(PVEOverState state)
    {
        CsChaseTaskOver chaseMsg = PacketObject.Create<CsChaseTaskOver>();
        chaseMsg.state = (sbyte)state;
        chaseMsg.overData = modulePVE.GetPveDatas();
        session.Send(chaseMsg);        
    }

    public void SendReborn()
    {
        CsChaseRoleReborn p = PacketObject.Create<CsChaseRoleReborn>();
        session.Send(p);
    }
    
    void _Packet(ScChaseTaskOver roleOver)
    {
        FightRecordManager.Set(roleOver);
        FightRecordManager.EndRecord(false, false);
        if (roleOver.result == 0)
        {
            TaskInfo info = moduleChase.lastStartChase == null ? null : moduleChase.lastStartChase.taskConfigInfo;
            SetPVESettlement(roleOver.reward, info, roleOver.overStar, roleOver.friendPoint, roleOver.npcPoint);
            //change cache data
            moduleChase.OnChaseTaskFinish(roleOver.overStar);
        }
        else if (roleOver.result != 0) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(402, roleOver.result));

        DispatchModuleEvent(EventGameOverSettlement, settlementReward);

        ObjectManager.enableUpdate = true;
    }

    public void SetPVESettlement(PReward reward,TaskInfo chanllengeTask = null, int overStar = 0, int friendPoint = 0, int npcPoint = 0)
    {
        //set reward
        if (reward != null) reward.CopyTo(ref settlementReward);
        settlementTask = chanllengeTask;
        //set over star
        settlementStar = overStar;
        addFriendPoint = friendPoint;
        addNpcPoint = npcPoint;
    }

    void _Packet(ScChaseRoleReborn reborn)
    {
        bool isSuccess = reborn.result == 0;
        DispatchModuleEvent(EventRebornState,isSuccess);
        Logger.LogWarning("recv reborn msg, result is {0} ", reborn.result);
        if (reborn.result != 0) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(403,reborn.result));
    }

    public void SendBossReward(int bossId)
    {
        // 暂停分发事件
        if (!isTeamMode)
            modulePVEEvent.pauseEvent = true;
        m_hasDisplayBossReward = false;

        var p = PacketObject.Create<CsChaseBossReward>();
        p.bossId = (ushort)bossId;
        session.Send(p);
    }

    private void TestBossReward()
    {
        var props = ConfigManager.GetAll<PropItemInfo>();
        var reward = PacketObject.Create<ScChaseBossReward>();
        reward.reward = new PItem2[3];
        for (int i = 0; i < reward.reward.Length; i++)
        {
            reward.reward[i] = PacketObject.Create<PItem2>();
            reward.reward[i].itemTypeId = (ushort)props.GetValue(Random.Range(0, props.Count)).ID;
            reward.reward[i].num = (uint)Random.Range(1, 10);
        }
        _Packet(reward);
    }

    void _Packet(ScChaseBossReward p)
    {
        if(p.result == 0 && p.reward != null)
        {
            p.reward.CopyTo(ref bossRewards);
            DispatchModuleEvent(EventRecvBossReward, p.reward);
            if(m_hasDisplayBossReward) DispatchModuleEvent(EventDisplayBossReward, bossRewards);
        }
        else if(!isTeamMode)
        {
            Logger.LogError("获取随机boss奖励异常，result is {0} reward is {1}",p.result,p.reward == null ? "invalid":"valid");
            modulePVEEvent.pauseEvent = false;
        }
    }

    public void DisplayBossReward()
    {
        m_hasDisplayBossReward = true;
        if (bossRewards == null || bossRewards.Length == 0) return;

        DispatchModuleEvent(EventDisplayBossReward, bossRewards);
    }


    #region pve game data collection

    public void InitPveGameData()
    {
        if (pveGameDatas == null) pveGameDatas = new float[(int)EnumPVEDataType.Count];
        for (int i = 0,len = pveGameDatas.Length; i < len; i++)
        {
            pveGameDatas[i] = 0f;
        }
    }

    private bool InvalidPveCollection(EnumPVEDataType type)
    {
        //pve is over
        if (modulePVEEvent.isEnd) return true; 

        return pveGameDatas == null || type < EnumPVEDataType.Health || type >= EnumPVEDataType.Count;
    }

    public void SetPveGameData(EnumPVEDataType type,float value)
    {
        if (InvalidPveCollection(type)) return;

        pveGameDatas[(int)type] = value;
    }

    public void SetPveGameData(EnumPVEDataType type, int value)
    {
        SetPveGameData(type,(float)value);
    }

    public void AddPveGameData(EnumPVEDataType type,float value)
    {
        if (InvalidPveCollection(type)) return;

        pveGameDatas[(int)type] += value;
    }

    public void AddPveGameData(EnumPVEDataType type, int value)
    {
        AddPveGameData(type, (float)value);
    }

    public float GetPveGameData(EnumPVEDataType type)
    {
        if (InvalidPveCollection(type)) return 0f;

        if (type == EnumPVEDataType.Health || type == EnumPVEDataType.Rage) return Mathf.Round(pveGameDatas[(int)type] * 100f);
        else return pveGameDatas[(int)type];
    }

    public float GetSecondData(EnumPVEDataType type)
    {
        if (InvalidPveCollection(type)) return 0f;

        if (type == EnumPVEDataType.Health || type == EnumPVEDataType.Rage)
        {
            return Mathf.Round(pveGameDatas[(int)type] * 100f);
        }
        else
        {
            return pveGameDatas[(int)type];
        }
    }

    public PPveOverData GetPveDatas()
    {
        PPveOverData p = PacketObject.Create<PPveOverData>();
        //health and rage need multiply by 100
        p.health =                  GetPveGameData(EnumPVEDataType.Health);
        p.anger =                   GetPveGameData(EnumPVEDataType.Rage);
        p.combo =           (ushort)GetPveGameData(EnumPVEDataType.Combo);
        p.beHitTimes =      (ushort)GetPveGameData(EnumPVEDataType.BeHitedTimes);
        p.gameTime =                GetPveGameData(EnumPVEDataType.GameTime);
        p.ultimateTimes =   (ushort)GetPveGameData(EnumPVEDataType.UltimateTimes);
        p.executionTimes =  (ushort)GetPveGameData(EnumPVEDataType.ExecutionTimes);
        p.attack =          (uint)GetPveGameData(EnumPVEDataType.Attack);
        p.killMonsters =            moduleActive.GetMonsterData().ToArray();
        return p;
    }

    public void DebugPveGameData()
    {
#if UNITY_EDITOR

        System.Text.StringBuilder s = new System.Text.StringBuilder();
        s.Append("log pve game data:\n");
        for (int i = 0,len = pveGameDatas.Length; i < len; i++)
        {
            s.AppendFormat("type:[{0}] value is {1}\n",(EnumPVEDataType)i,pveGameDatas[i]);
        }
        Logger.LogInfo(s.ToString());
#endif
    }
    
    public void RefreshTotalDeathMonster(int deathMonsterId, int count)
    {
        if (count != 0)
        {
            if (!totalMonsterDeathDic.ContainsKey(deathMonsterId)) totalMonsterDeathDic.Add(deathMonsterId, 0);
            totalMonsterDeathDic[deathMonsterId] += count;
        }
    }

    #endregion

    #region auto battle

    private void GetLocalAutoBattleRecord()
    {
        recordAutoBattle = PlayerPrefs.GetInt(AUTO_BATTLE,1) > 0;
    }

    public void SaveLocalAutoBattleRecord()
    {
        PlayerPrefs.SetInt(AUTO_BATTLE,recordAutoBattle ? 1 : 0);
    }

    public EnumPVEAutoBattleState GetAutoBattleState()
    {
        if (!currrentStage || !(Level.current is Level_PVE)) return EnumPVEAutoBattleState.Disable;

        switch (currrentStage.autoFight)
        {
            case EnumPVEAutoBattleType.Forbidden:       return EnumPVEAutoBattleState.Disable;
            case EnumPVEAutoBattleType.NoCondition:     return EnumPVEAutoBattleState.Enable;
            case EnumPVEAutoBattleType.StageClear:      return isFirstEnterStage ? EnumPVEAutoBattleState.EnableNotStageClear : EnumPVEAutoBattleState.Enable;
            case EnumPVEAutoBattleType.ThreeStars:      return CheckThreeStarsAutoBattle() ? EnumPVEAutoBattleState.Enable : EnumPVEAutoBattleState.EnableNotThreeStars;
        }

        return EnumPVEAutoBattleState.Disable;
    }

    public bool CheckThreeStarsAutoBattle()
    {
        if (m_reopenPanel == PVEReOpenPanel.ChasePanel) return moduleChase.lastStartChase == null ? false : moduleChase.lastStartChase.star == 3;
        return false;
    }
    #endregion

    #region dispatch module_pve event

    public void DispathMonsterTip(EnumMonsterTipArrow tipDir)
    {
        DispatchModuleEvent(EventMonsterTip,tipDir);
    }

    public void DispathTransportSceneToUI(bool begin)
    {
        duringTranspostScene = begin;
        modulePVEEvent.readyToShowMsgLogic = !begin;
        DispatchModuleEvent(EventTransportSceneToUI, begin);
    }

    /// <summary>
    /// 切换场景的提示
    /// -1 = left
    /// 0 =trigger in the scene
    /// 1 = right
    /// </summary>
    /// <param name="dir"></param>
    public void DispathTransportTriggerTip(int dir)
    {
        DispatchModuleEvent(EventTriggerTransportTip, dir);
    }

    public void DispathBossWhiteAnim()
    {
        DispatchModuleEvent(EventBossCameraAnimWhiteFadeIn);
    }
    #endregion

    #region Team

    public bool isTeamMode { get; private set; }

    public void StartTeamLevel(ScTeamStartLoading p)
    {
        if (moduleTeam.state < 0)
        {
            Logger.LogError("PVE::StartTeamLevel: We are not in team mode!");
            return;
        }

        Logger.LogInfo("PVE::StartTeamLevel: Start new team stage [{0}], mode [{1}]", moduleTeam.stage, moduleTeam.mode);
        
        OnPVEStart(moduleTeam.stage, moduleTeam.mode == TeamMode.Awake ? PVEReOpenPanel.ChasePanel : PVEReOpenPanel.None, true);
        isFirstEnterStage = false;
        if(p.members != null)
        {
            foreach (var item in p.members)
            {
                //第一次进入关卡只跟队长有关
                if(item.teamLeader > 0)
                {
                    isFirstEnterStage = item.isFirst > 0;
                    break;
                }
            }
        }
        enterForFirstTime = false;
        RefreshRebornData(p.rebornTime, p.rebornItem);
    }
    
    public void SendStatrPVETeam()
    {
        var p = PacketObject.Create<CsTeamPveStart>();
        session.Send(p);
    }

    public void TestSendPveStartAsLeader()
    {
        SendStatrPVETeam();
    }

    public void TestSendPveStartAsMember()
    {
        var p = PacketObject.Create<CsSystemSetEvn>();
        p.key = p.value = "team_member";
        session.Send(p);
    }

    #endregion
}
