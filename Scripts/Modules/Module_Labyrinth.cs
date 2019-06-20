/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-04
 * 
 ***************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class Module_Labyrinth : Module<Module_Labyrinth>
{
    #region const definition

    #region 事件名称

    public const string EventOpenTime               = "EventOpenTime";
    public const string EventEnterlabyrinth         = "EventEnterlabyrinth";
    public const string EventRefreshSelf            = "EventRefreshSelf";
    public const string EventRefreshPlayers         = "EventRefreshPlayers";
    public const string EventRefreshProps           = "EventRefreshProps";
    public const string EventPropChange             = "EventPropChange";
    public const string EventChallengeLabyrinth     = "EventChallengeLabyrinth";
    public const string EventChallengeOver          = "EventChallengeOver";
    public const string EventSneakPlayer            = "EventSneakPlayer";
    public const string EventSneakOver              = "EventSneakOver";
    public const string EventBattleReport           = "EventBattleReport";
    public const string EventRefreshReport          = "EventRefreshReport";
    public const string EventEnterSettlementStep    = "EventEnterSettlementStep";
    public const string EventRefreshSelfAttackProp  = "EventRefreshSelfAttackProp";
    public const string EventRefreshUseButtonState  = "EventRefreshUseButtonState";
    public const string EventTriggerSceneCollider   = "EventTriggerSceneCollider";
    public const string EventCreatePlayers          = "EventCreatePlayers";
    public const string EventPlayerStateChange      = "EventPlayerStateChange";
    public const string EventSetPlayerVisible       = "EventSetPlayerVisible";
    public const string EventPlayerPosChange        = "EventPlayerPosChange";
    public const string EventLabyrinthTimeRefresh   = "EventLabyrinthTimeRefresh";
    #endregion

    public static int LABYRINTH_LEVEL_ID = 9;

    public readonly static TimeSpan ONE_SECOND_TIME_SPAN = new TimeSpan(0, 0, 1);
    #endregion

    #region property

    #region 时间相关
    /// <summary>
    /// 收到的迷宫开始时间
    /// </summary>
    public ScMazeOpenTime labyrinthOpenTime;

    /// <summary>
    /// 收到迷宫开始时间的时刻
    /// </summary>
    private float recvOpenMsgTime;

    //获取活动开始倒计时
    public long openTimeInterval
    {
        get
        {
            if (currentLabyrinthStep == EnumLabyrinthTimeStep.Ready) return currentStepRestTime;
            else if (currentLabyrinthStep == EnumLabyrinthTimeStep.Chanllenge || currentLabyrinthStep == EnumLabyrinthTimeStep.Rest) return 0;
            else return -1;
        }
    }

    /// <summary>
    /// 当前阶段
    /// </summary>
    public EnumLabyrinthTimeStep currentLabyrinthStep { get; private set; } = EnumLabyrinthTimeStep.Close;

    /// <summary>
    /// 当前阶段剩余时间
    /// </summary>
    public long currentStepRestTime { get; private set; } = 0;
    public TimeSpan currentStepRestTimespan;

    public string[] timeFormat = new string[]
    {
        ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText,0),//时分秒
        ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText,49),//天
    };

    public string[] stepFormatForNormal = new string[]
    {
        ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText,45),//准备阶段
        ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText,46),//攻略阶段
        ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText,47),//修整阶段
        ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText,48),//结算阶段
    };

    public string[] stepFormatForMaze = new string[]
    {
        ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText,50),//准备阶段
        ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText,51),//攻略阶段
        ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText,52),//修整阶段
        ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText,53),//结算阶段
    };

    #endregion

    #region 道具相关

    private Dictionary<ushort, PItem2> m_labyrinthPropDic = new Dictionary<ushort, PItem2>();
    #endregion

    #region 地图相关
    //按照升级，保级，降级的顺序返回
    public Dictionary<EnumLabyrinthArea, List<PItem2>> areaRewardDic = new Dictionary<EnumLabyrinthArea, List<PItem2>>();
    public int currentLabyrinthID { get { return pmazeInfo == null ? 1 : pmazeInfo.mazeLevel; } }
    public byte mazeMaxLayer { get { return pmazeInfo == null ? (byte)0 : pmazeInfo.mazeMaxLayer; } }

    public PMazeInfo pmazeInfo;
    public LabyrinthInfo labyrinthInfo { get; private set; }
    #endregion

    #region 偷袭相关
    /// <summary>
    /// 偷袭的玩家信息
    /// </summary>
    public ScMazeSneak sneakPlayerDetail;

    public PMazePlayer lastSneakPlayer;

    /// <summary>
    /// 是否偷袭成功
    /// </summary>
    public bool isSneakSuccess = false;
    #endregion

    #region 新迷宫界面

    public int moveMentKey { get; set; }

    private bool m_playerVisible = true;
    public bool playerVisible
    {
        get { return m_playerVisible; }
        set
        {
            if (value == m_playerVisible) return;

            m_playerVisible = value;
            DispatchModuleEvent(EventSetPlayerVisible, m_playerVisible);
        }
    }
    
    #endregion

    /// <summary>
    /// 迷宫玩家自己的信息
    /// </summary>
    public ScMazeSelfInfo labyrinthSelfInfo { get { return m_labyrinthSelfInfo; } }
    private ScMazeSelfInfo m_labyrinthSelfInfo;

    /// <summary>
    /// 每个迷宫区域的玩家
    /// 1.降级区，2.保级区，3.晋级区
    /// </summary>
    public Dictionary<EnumLabyrinthArea, List<PMazePlayer>> areaPlayerDic { get; private set; } = new Dictionary<EnumLabyrinthArea, List<PMazePlayer>>();

    /// <summary>
    /// 上一次发送的结算消息
    /// </summary>
    private CsMazeChallengeOver m_lastSendOverMsg;

    private bool m_isSendBattleReport;
    /// <summary>
    /// 战报信息(便于动态扩充)
    /// </summary>
    public List<PMazeReport> labyrinthReports { get; private set; } = new List<PMazeReport>();

    /// <summary>
    /// 存储自己在迷宫每一层所放置的陷阱道具
    /// </summary>
    private Dictionary<int, int> m_labyrinthTrapDic = new Dictionary<int, int>();
    private CsUseTrapProp m_lastUseTrapProp;
    private bool m_sendUnlockMsg = false;
#endregion

    /// <summary>
    /// 退出的时候清空
    /// </summary>
    private void InitLabyrinthData()
    {
        areaPlayerDic.Clear();
        m_labyrinthPropDic.Clear();
        labyrinthReports.Clear();
        m_labyrinthTrapDic.Clear();

        //初始化的时候清空该数据
        m_isSendBattleReport = false;
        pmazeInfo = null;
        areaRewardDic.Clear();
        m_labyrinthSelfInfo = null;
        OnSneakSettleOver();
        m_sendUnlockMsg = false;
        moveMentKey = 0;
    }

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        InitLabyrinthData();
        enableUpdate = false;
        currentStepRestTime = 0;
        currentStepRestTimespan = TimeSpan.Zero;
        recvOpenMsgTime = 0;
    }

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        Level_3DClick.Init3DDefalutData();
    }

    public override void OnUpdate(int diff)
    {
        base.OnUpdate(diff);
        UpdatelabyrinthStep();
    }

    public PMazePlayer GetSelfMazeData()
    {
        return GetTargetPlayer(modulePlayer.roleInfo.roleId);
    }

    public PMazePlayer GetTargetPlayer(ulong roleId)
    {
        foreach (var item in areaPlayerDic)
        {
            foreach (var p in item.Value)
            {
                if (p.roleId == roleId) return p;
            }
        }

        return null;
    }

    public int GetPlayerRank(ulong roleId)
    {
        if (areaPlayerDic == null) return 0;

        int rank = 0;
        bool success = false;

        var list = areaPlayerDic.Get(EnumLabyrinthArea.PromotionArea);
        if (list != null && rank == 0)
        {
            int index = list.FindIndex(0, list.Count, o => o.roleId == roleId);
            success = index >= 0;
            rank += success ? index + 1 : list.Count;
        }

        list = areaPlayerDic.Get(EnumLabyrinthArea.RelegationArea);
        if (list != null && !success)
        {
            int index = list.FindIndex(0, list.Count, o => o.roleId == roleId);
            success = index >= 0;
            rank += success ? index + 1 : list.Count;
        }

        list = areaPlayerDic.Get(EnumLabyrinthArea.DemotionArea);
        if (list != null && !success)
        {
            int index = list.FindIndex(0, list.Count, o => o.roleId == roleId);
            success = index >= 0;
            rank += success ? index + 1 : list.Count;
        }

        return rank;
    }

    public EnumLabyrinthArea GetPlayerArea(ulong roleId)
    {
        foreach (var item in areaPlayerDic)
        {
            foreach (var p in item.Value)
            {
                if (p.roleId == roleId) return item.Key;
            }
        }

        return EnumLabyrinthArea.DemotionArea;
    }

    public void OnSneakSettleOver()
    {
        lastSneakPlayer = null;
        sneakPlayerDetail = null;
        isSneakSuccess = false;
    }

    public List<EnumLabyrinthPlayerState> GetLabyPlayerAllStates(byte state)
    {
        List<EnumLabyrinthPlayerState> list = new List<EnumLabyrinthPlayerState>();
        if ((state & (byte)EnumLabyrinthPlayerState.Idle) > 0)              list.Add(EnumLabyrinthPlayerState.Idle);
        if ((state & (byte)EnumLabyrinthPlayerState.Battle) > 0)            list.Add(EnumLabyrinthPlayerState.Battle);
        if ((state & (byte)EnumLabyrinthPlayerState.SneakProtect) > 0)      list.Add(EnumLabyrinthPlayerState.SneakProtect);
        if ((state & (byte)EnumLabyrinthPlayerState.ForceRest) > 0)         list.Add(EnumLabyrinthPlayerState.ForceRest);
        if ((state & (byte)EnumLabyrinthPlayerState.ProtectByBell) > 0)     list.Add(EnumLabyrinthPlayerState.ProtectByBell);
        return list;
    }

    public string GetLabyPlayerAllStateStr(byte state)
    {
        string str = string.Empty;
        List<EnumLabyrinthPlayerState> states = GetLabyPlayerAllStates(state);
        for (int j = 0; j < states.Count; j++)
        {
            str += states[j].ToString() + "   ";
        }
        return str;
    }

    public bool HasState(byte state, EnumLabyrinthPlayerState type)
    {
        if ((state & (byte)type) > 0) return true;

        return false;
    }

    public LabyrinthCreature.LabyrinthCreatureType GetLabyrinthCreatureType(ulong roleId,byte state)
    {
        if (roleId == modulePlayer.roleInfo.roleId) return LabyrinthCreature.LabyrinthCreatureType.Self;
        else if (HasState(state, EnumLabyrinthPlayerState.InPveBattle)) return LabyrinthCreature.LabyrinthCreatureType.InPveBattle;
        else if (HasState(state, EnumLabyrinthPlayerState.Battle)) return LabyrinthCreature.LabyrinthCreatureType.InLabyrinth;
        else return LabyrinthCreature.LabyrinthCreatureType.OutLabyrinth;
    }

    private void ShowErrorTip(int configId, sbyte result)
    {
        if (result == 0)
            return;

        Logger.LogDetail("错误提示 config_text_id = {0},result = {1}", configId, result);
        ConfigText text = ConfigManager.Get<ConfigText>(configId);
        if (text)
        {
            string str = text[result - 1];
            if (!string.IsNullOrEmpty(str))
                moduleGlobal.ShowMessage(str);
        }
    }

    #region 迷宫时间阶段相关
    public void SendLabyrinthOpenTime()
    {
        var p = PacketObject.Create<CsMazeOpenTime>();
        session.Send(p);
    }

    void _Packet_999(ScMazeOpenTime p)
    {
        p.CopyTo(ref labyrinthOpenTime);
        DispatchModuleEvent(EventOpenTime, labyrinthOpenTime);
        
        currentLabyrinthStep = (EnumLabyrinthTimeStep)labyrinthOpenTime.mazeStep;
        currentStepRestTime = labyrinthOpenTime.restTime;
        currentStepRestTimespan = new TimeSpan(0,0, (int)currentStepRestTime);
        recvOpenMsgTime = Time.realtimeSinceStartup;
        enableUpdate = currentLabyrinthStep != EnumLabyrinthTimeStep.None;
        DispatchModuleEvent(EventLabyrinthTimeRefresh, currentLabyrinthStep, currentStepRestTime, currentStepRestTimespan);
    }

    private void UpdatelabyrinthStep()
    {
        if(recvOpenMsgTime != 0 && Time.realtimeSinceStartup - recvOpenMsgTime >= 1.0f)
        {
            currentStepRestTime -= 1;
            currentStepRestTime = currentStepRestTime < 0 ? 0 : currentStepRestTime;

            currentStepRestTimespan -= ONE_SECOND_TIME_SPAN;
            if (currentStepRestTimespan < TimeSpan.Zero) currentStepRestTimespan = TimeSpan.Zero;

            //当前阶段时间到了，重新请求时间
            if (currentStepRestTime <= 0)
            {
                recvOpenMsgTime = 0;
                SendLabyrinthOpenTime();
            }

            recvOpenMsgTime = Time.realtimeSinceStartup;
            //Logger.LogInfo("step is {0} restTime is {1}", currentLabyrinthStep, currentStepRestTime);
            DispatchModuleEvent(EventLabyrinthTimeRefresh,currentLabyrinthStep,currentStepRestTime, currentStepRestTimespan);
        }
    }

    public string GetStepAndTimeString(bool home = true)
    {
        string stepFat = GetStepFormat(currentLabyrinthStep, home);
        if (string.IsNullOrEmpty(stepFat)) return GetTimeString(currentStepRestTimespan);
        else return Util.Format(stepFat, GetTimeString(currentStepRestTimespan));
    }

    public string GetStepFormat(EnumLabyrinthTimeStep step,bool home)
    {
        switch (step)
        {
            case EnumLabyrinthTimeStep.Ready: return home ? stepFormatForNormal[0] : stepFormatForMaze[0];
            case EnumLabyrinthTimeStep.Chanllenge: return home ? stepFormatForNormal[1] : stepFormatForMaze[1];
            case EnumLabyrinthTimeStep.Rest: return home ? stepFormatForNormal[2] : stepFormatForMaze[2];

            case EnumLabyrinthTimeStep.Close:
            case EnumLabyrinthTimeStep.SettleMent: return home ? stepFormatForNormal[3] : stepFormatForMaze[3];

        }
        return string.Empty;
    }

    public string GetTimeString(long time)
    {
        TimeSpan span = new TimeSpan(0, 0, (int)currentStepRestTime);
        return GetTimeString(span);
    }

    public string GetTimeString(TimeSpan span)
    {
        if (span.Days > 0) return Util.Format(timeFormat[1], span.Days);
        else return Util.Format(timeFormat[0], span.Hours, span.Minutes, span.Seconds);
    }

    #endregion

    #region 发送请求

    public void SendUnlockLabyrinth()
    {
        //每次一个账号进入时，只发送一次解锁
        if (m_sendUnlockMsg) return;

        m_sendUnlockMsg = true;
        var p = PacketObject.Create<CsMazeUnlock>();
        session.Send(p);
    }

    public void SendLabyrinthEnter()
    {
        //如果没有退出或者还没进入的时候，不再发送进入请求
        if (pmazeInfo != null)
        {
            var msg = PacketObject.Create<ScMazeEnterScene>();
            msg.result = 0;
            pmazeInfo.CopyTo(ref msg.mazeInfo);
            _Packet(msg);
            return;
        }

        //enter labyrinth directly
        if (openTimeInterval != 0)
        {
            Window.ShowAsync<Window_Countdown>();
            return;
        }

        if (Level.current && Level.current is Level_Labyrinth)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 44));
            return;
        }

        InitLabyrinthData();
        var p = PacketObject.Create<CsMazeEnterScene>();
        session.Send(p);
    }


    /// <summary>
    /// 请求自己的信息，服务器默认会推送回来
    /// </summary>
    public void SendLabyrinthSelfInfo()
    {
        var p = PacketObject.Create<CsMazeSelfInfo>();
        session.Send(p);
    }

    /// <summary>
    /// 请求玩家列表，服务器默认会推送回来
    /// </summary>
    public void SendLabyrinthPlayers()
    {
        var p = PacketObject.Create<CsMazeScenePlayer>();
        session.Send(p);
    }

    public void SendLabyrinthProps()
    {
        //当前已经有道具就不再请求
        if (m_labyrinthPropDic != null && m_labyrinthPropDic.Count > 0)
        {
            DispatchModuleEvent(EventRefreshProps, m_labyrinthPropDic);
            return;
        }

        var p = PacketObject.Create<CsMazePropInfo>();
        session.Send(p);
    }

    public void SendUseTrapProp(ushort propId)
    {
        var p = PacketObject.Create<CsUseTrapProp>();
        p.propId = propId;
        p.mazeLayer = labyrinthSelfInfo.mazeLayer;

        p.CopyTo(ref m_lastUseTrapProp);

        session.Send(p);
    }

    public void SendUsePlayerProp(ushort propId, ulong roleId)
    {
        var p = PacketObject.Create<CsUsePlayerProp>();
        p.propId = propId;
        p.roleId = roleId;
        session.Send(p);
    }

    public void SendChallengeLabyrinth()
    {
        if(currentLabyrinthStep == EnumLabyrinthTimeStep.Rest)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(10010, 6));
            return;
        }

        var p = PacketObject.Create<CsMazeChallenge>();
        session.Send(p);
    }

    public void SendChallengeLabyrinthOver(float health, byte anger, PVEOverState overState)
    {
        var p = PacketObject.Create<CsMazeChallengeOver>();
        p.overData = modulePVE.GetPveDatas();
        p.overState = (byte)overState;
        //失败了要清空怒气
        if (overState == PVEOverState.GameOver) p.overData.anger = 0;

        //拷贝发送的消息作为缓存
        p.CopyTo(ref m_lastSendOverMsg);
        session.Send(p);
    }

    public void SendLabyrinthSneak(PMazePlayer player)
    {
        var p = PacketObject.Create<CsMazeSneak>();
        p.uid = player.roleId;

        lastSneakPlayer = player;
        session.Send(p);
    }

    public void SendLabyrinthSneakOver(PVEOverState state)
    {
        var p = PacketObject.Create<CsMazeSceneSneakOver>();
        p.overState = (byte)state;
        Logger.LogDetail("偷袭完成，发送的偷袭状态是{0}", state);
        //通过发送消息来决定是否是偷袭成功
        isSneakSuccess = state == PVEOverState.Success;
        session.Send(p);
    }

    public void SendExitLabyrinth()
    {
        InitLabyrinthData();
        var p = PacketObject.Create<CsMazeSceneCancel>();
        session.Send(p);
    }

    public void SendRequestBattleReports()
    {
        //已经有战报信息的话，直接分发事件
        if (m_isSendBattleReport)
        {
            DispatchModuleEvent(EventBattleReport, labyrinthReports);
            return;
        }

        m_isSendBattleReport = true;
        var p = PacketObject.Create<CsMazeBattleReport>();
        session.Send(p);
    }

    #endregion 

    #region 接收消息

    void _Packet(ScMazeUnlock p)
    {
        SendLabyrinthOpenTime();
        m_sendUnlockMsg = p.result == 0 || p.result == 1;
    }
    
    void _Packet(ScMazeEnterScene p)
    {
        Logger.LogInfo("收到enter 回复 result = {0}", p.result);
        if (p.result == 0)
        {
            areaRewardDic.Clear();
            for (int i = (int)EnumLabyrinthArea.DemotionArea; i <= (int)EnumLabyrinthArea.PromotionArea; i++)
            {
                areaRewardDic.Add((EnumLabyrinthArea)i,new List<PItem2>());
            }

            if (p.mazeInfo.rewardList != null)
            {
                for (int i = 0; i < p.mazeInfo.rewardList.Length; i++)
                {
                    var r = p.mazeInfo.rewardList[i];
                    if (r == null) continue;

                    var area = (EnumLabyrinthArea)((int)EnumLabyrinthArea.PromotionArea - i);
                    if (!areaRewardDic.ContainsKey(area)) continue;
                    
                    areaRewardDic[area].AddRange(GetPItem2FromPreward(r));
                }
            }

            pmazeInfo = null;
            p.mazeInfo.CopyTo(ref pmazeInfo);
            Logger.LogDetail("maze id ={0},maxLayer = {1},rise ={2},keep = {3},down = {4}  level  = {5}", pmazeInfo.mazeId,
                              pmazeInfo.mazeMaxLayer, pmazeInfo.riseNum, pmazeInfo.keepNum, pmazeInfo.downNum, pmazeInfo.mazeLevel);

            labyrinthInfo = ConfigManager.Get<LabyrinthInfo>(currentLabyrinthID);
            if(labyrinthInfo == null)
            {
                labyrinthInfo = ConfigManager.GetAll<LabyrinthInfo>().GetValue(0);
                Logger.LogError("cannot finded a labyrinth info with Id {0},we will use id = {1} to instead of", currentLabyrinthID, labyrinthInfo ? labyrinthInfo.ID : -1);
            }
            LABYRINTH_LEVEL_ID = labyrinthInfo.levelId;
            Game.LoadLevel(LABYRINTH_LEVEL_ID);
        }
        else
        {
            ShowErrorTip(10007, p.result);
        }

        DispatchModuleEvent(EventEnterlabyrinth, p.result, true);
    }

    public List<PItem2> GetPItem2FromPreward(PReward reward)
    {
        List<PItem2> l = new List<PItem2>();
        //金币
        if (reward.coin > 0)
        {
            var item = PacketObject.Create<PItem2>();
            item.itemTypeId = 1;
            item.num = (uint)reward.coin;
            l.Add(item);
        }

        //钻石
        if (reward.diamond > 0)
        {
            var item = PacketObject.Create<PItem2>();
            item.itemTypeId = 2;
            item.num = (uint)reward.diamond;
            l.Add(item);
        }

        //积分
        if (reward.score > 0)
        {
            var item = PacketObject.Create<PItem2>();
            item.itemTypeId = 8;
            item.num = (uint)reward.coin;
            l.Add(item);
        }

        //体力
        if (reward.fatigue > 0)
        {
            var item = PacketObject.Create<PItem2>();
            item.itemTypeId = 15;
            item.num = (uint)reward.coin;
            l.Add(item);
        }

        if (reward.rewardList != null) l.AddRange(reward.rewardList);
        return l;
    }

    void _Packet(ScMazeSelfInfo p)
    {
        p.CopyTo(ref m_labyrinthSelfInfo);
        Logger.LogDetail("收到自己的信息，hp ={0},anger = {1},layer = {2},all state = {3}", p.healthRate, p.angerRate, p.mazeLayer, GetLabyPlayerAllStateStr(p.mazeState));
        if (labyrinthSelfInfo.attackProps != null)
        {
            string str = string.Empty;
            for (int i = 0; i < labyrinthSelfInfo.attackProps.Length; i++)
            {
                PropItemInfo item = ConfigManager.Get<PropItemInfo>(labyrinthSelfInfo.attackProps[i]);
                str += Util.Format("{0} {1} {2}  ", item.ID, (LabyrinthPropSubType)item.subType, item.itemName);
            }
            Logger.LogDetail("收到自己的信息，props ={0}", str);
        }

        InitLabyrinthTrapDic(p.trapProps);
        UpdateSelfLayerToPlayerPanel();
        DispatchModuleEvent(EventRefreshPlayers, areaPlayerDic);
        DispatchModuleEvent(EventRefreshSelf, labyrinthSelfInfo);
    }

    void _Packet(ScMazePlayerHp p)
    {
        labyrinthSelfInfo.healthRate = p.curHp;
        Logger.LogInfo("玩家血量推送   当前血量{0}%", labyrinthSelfInfo.healthRate);
        DispatchModuleEvent(EventRefreshSelf, labyrinthSelfInfo);
    }

    /// <summary>
    /// 服务器推送玩家信息，，推送回来的可能只是修改后的玩家信息
    /// </summary>
    /// <param name="p"></param>
    void _Packet(ScMazeScenePlayer p)
    {
        PMazePlayer[] players = null;
        p.playerList.CopyTo(ref players);

        //对玩家进行排序,并重新填充
        List<PMazePlayer> allPlayerList = new List<PMazePlayer>();

        //先填充已有数据
        if (areaPlayerDic != null)
        {
            Dictionary<EnumLabyrinthArea, List<PMazePlayer>>.Enumerator e = areaPlayerDic.GetEnumerator();
            while (e.MoveNext())
            {
                allPlayerList.AddRange(e.Current.Value);
            }
        }

        //再覆盖老数据
        List<PMazePlayer> needAddList = new List<PMazePlayer>();
        for (int i = 0; i < players.Length; i++)
        {
            PMazePlayer pp = players[i];
            bool isContain = false;
            for (int j = 0; j < allPlayerList.Count; j++)
            {
                if (pp.roleId == allPlayerList[j].roleId)
                {
                    allPlayerList[j] = pp;
                    isContain = true;
                    break;
                }
            }

            if (!isContain) needAddList.Add(pp);
        }

        //将新增的数据直接填充到列表中
        allPlayerList.AddRange(needAddList);

        //将收到的玩家列表中自己的数据更新
        if (labyrinthSelfInfo != null)
        {
            for (int i = 0; i < allPlayerList.Count; i++)
            {
                if (allPlayerList[i].roleId == modulePlayer.id_)
                {
                    allPlayerList[i].mazeLayer = labyrinthSelfInfo.mazeLayer;
                    break;
                }
            }
        }

        foreach (var item in allPlayerList)
        {
            if(item.fashion == null)
            {
                Logger.LogError("roleId = {0} roleName = {1} fashion data is null",item.roleId,item.roleName);
                if (item.roleId == modulePlayer.id_) item.fashion = GetSelfFashion();
                else item.fashion = GetRandomFashion(item.gender,item.roleProto);
            }
        }

        //协议异常到连自己的信息都没有了
        if(allPlayerList.FindIndex(o=>o.roleId == modulePlayer.id_) < 0)
        {
            var se = PacketObject.Create<PMazePlayer>();
            se.roleId = modulePlayer.id_;
            se.roleName = modulePlayer.roleInfo.roleName;
            se.roleProto = (byte)modulePlayer.proto;
            se.gender = (byte)modulePlayer.gender;
            se.mazeLayer = m_labyrinthSelfInfo == null ? (byte)0 : m_labyrinthSelfInfo.mazeLayer;
            se.lastMazeLevel = 0;
            se.mazeState = (byte)EnumLabyrinthPlayerState.Battle;
            se.fashion = GetSelfFashion();
            if (modulePet.FightingPet != null) se.pet = modulePet.FightingPet.Item;
            allPlayerList.Add(se);
        }

        allPlayerList.Sort((a, b) =>
        {
            //层数相同先过关的玩家优先显示
            if (a.mazeLayer == b.mazeLayer)
                return a.layerTime.CompareTo(b.layerTime);

            return -a.mazeLayer.CompareTo(b.mazeLayer);
        });

        RefreshAreaPlayers(allPlayerList);
        DispatchModuleEvent(EventRefreshPlayers, areaPlayerDic);
        DispatchModuleEvent(EventCreatePlayers);
    }

    public PFashion GetSelfFashion()
    {
        PFashion fashion = PacketObject.Create<PFashion>();
        for (int i = 0, count = moduleEquip.currentDressClothes.Count; i < count; i++)
        {
            PItem item = moduleEquip.currentDressClothes[i];
            PropItemInfo itemInfo = item?.GetPropItem();
            if (itemInfo == null) continue;

            switch (itemInfo.itemType)
            {
                case PropType.Weapon:
                    if (itemInfo.subType != (byte)WeaponSubType.Gun)
                        fashion.weapon = item.itemTypeId;
                    else
                        fashion.gun = item.itemTypeId;
                    break;

                case PropType.FashionCloth:
                    switch ((FashionSubType)itemInfo.subType)
                    {
                        case FashionSubType.UpperGarment:
                        case FashionSubType.FourPieceSuit:
                        case FashionSubType.TwoPieceSuit:
                            fashion.clothes = item.itemTypeId;
                            break;
                        case FashionSubType.Pants:
                            fashion.trousers = item.itemTypeId;
                            break;
                        case FashionSubType.Glove:
                            fashion.glove = item.itemTypeId;
                            break;
                        case FashionSubType.Shoes:
                            fashion.shoes = item.itemTypeId;
                            break;
                        case FashionSubType.Hair:
                            fashion.hair = item.itemTypeId;
                            break;
                        case FashionSubType.ClothGuise:
                            fashion.guiseId = item.itemTypeId;
                            break;
                        case FashionSubType.HeadDress:
                            fashion.headdress = item.itemTypeId;
                            break;
                        case FashionSubType.HairDress:
                            fashion.hairdress = item.itemTypeId;
                            break;
                        case FashionSubType.FaceDress:
                            fashion.facedress = item.itemTypeId;
                            break;
                        case FashionSubType.NeckDress:
                            fashion.neckdress = item.itemTypeId;
                            break;
                    }
                    break;
            }
        }

        return fashion;
    }

    public PFashion GetRandomFashion(int gender,int proto)
    {
        var infos = ConfigManager.GetAll<PropItemInfo>().FindAll(o=>o.itemType == PropType.Weapon || o.itemType == PropType.FashionCloth);
        PFashion fashion = PacketObject.Create<PFashion>();
        
        var items = infos.FindAll(o => o.itemType == PropType.Weapon && o.subType == proto && o.IsValidVocation(proto));
        PropItemInfo weapon = items.GetValue(Random.Range(0, items.Count));
        if (!weapon)
        {
            Logger.LogError("cannot finded a valid weapon with proto {0}",proto);
            return fashion;
        }
        fashion.weapon = (ushort)weapon?.ID;

        items = infos.FindAll(o => o.itemType == PropType.Weapon && o.subType == (int)WeaponSubType.Gun && (o.sex == gender || o.sex == 2));
        fashion.gun = (ushort)items.GetValue(Random.Range(0, items.Count))?.ID;

        items = infos.FindAll(o => o.itemType == PropType.FashionCloth && o.subType == (int)FashionSubType.FourPieceSuit && o.IsValidVocation(proto));
        fashion.clothes = (ushort)items.GetValue(Random.Range(0, items.Count))?.ID;
        fashion.trousers = fashion.glove = fashion.shoes = fashion.clothes;

        items = infos.FindAll(o => o.itemType == PropType.FashionCloth && o.subType == (int)FashionSubType.Hair && o.IsValidVocation(proto));
        fashion.hair = (ushort)items.GetValue(Random.Range(0, items.Count))?.ID;

        items = infos.FindAll(o => o.itemType == PropType.FashionCloth && o.subType == (int)FashionSubType.HeadDress && o.IsValidVocation(proto));
        fashion.headdress = (ushort)items.GetValue(Random.Range(0, items.Count))?.ID;

        items = infos.FindAll(o => o.itemType == PropType.FashionCloth && o.subType == (int)FashionSubType.HairDress && o.IsValidVocation(proto));
        fashion.hairdress = (ushort)items.GetValue(Random.Range(0, items.Count))?.ID;
        
        items = infos.FindAll(o => o.itemType == PropType.FashionCloth && o.subType == (int)FashionSubType.FaceDress && o.IsValidVocation(proto));
        fashion.facedress = (ushort)items.GetValue(Random.Range(0, items.Count))?.ID;

        items = infos.FindAll(o => o.itemType == PropType.FashionCloth && o.subType == (int)FashionSubType.NeckDress && o.IsValidVocation(proto));
        fashion.neckdress = (ushort)items.GetValue(Random.Range(0, items.Count))?.ID;

        return fashion;
    }

    private void RefreshAreaPlayers(List<PMazePlayer> allPlayers)
    {
        if (allPlayers.Count == 0 || pmazeInfo == null)
            return;
        
        areaPlayerDic.Clear();
        areaPlayerDic.Add(EnumLabyrinthArea.PromotionArea, new List<PMazePlayer>());
        areaPlayerDic.Add(EnumLabyrinthArea.RelegationArea, new List<PMazePlayer>());
        areaPlayerDic.Add(EnumLabyrinthArea.DemotionArea, new List<PMazePlayer>());

        //先填充升级区
        for (int i = 0; i < pmazeInfo.riseNum; i++)
        {
            //可能出现人数不足的情况
            if (i >= allPlayers.Count)
                break;

            //只有打过迷宫的玩家才能被选中
            if (allPlayers[i].mazeLayer > 0)
                areaPlayerDic[EnumLabyrinthArea.PromotionArea].Add(allPlayers[i]);
        }

        //只能获取实际填充数量，然后从该索引开始继续装填其他区域的玩家(因为最后一个迷宫根本没有升级区)
        int realTillCount = areaPlayerDic[EnumLabyrinthArea.PromotionArea].Count;
        Logger.LogDetail(" 晋级区有 {0}人  ", areaPlayerDic[EnumLabyrinthArea.PromotionArea].Count);

        //保级区数量可能会等于20人（在第一关）
        for (int i = realTillCount; i < pmazeInfo.keepNum + realTillCount; i++)
        {
            //当筛选超过所有玩家数量的时候，停止筛选
            if (i == allPlayers.Count)
                break;

            //只有打过迷宫的玩家或者进入最低级迷宫的玩家才能被选中
            if (allPlayers[i].mazeLayer > 0 || pmazeInfo.mazeLevel == 1)
                areaPlayerDic[EnumLabyrinthArea.RelegationArea].Add(allPlayers[i]);
        }
        Logger.LogDetail(" 保级区有 {0}人  ", areaPlayerDic[EnumLabyrinthArea.RelegationArea].Count);

        //重新计算实际填充区域
        realTillCount += areaPlayerDic[EnumLabyrinthArea.RelegationArea].Count;

        for (int i = realTillCount; i < allPlayers.Count; i++)
        {
            //其余所有玩家全部进入降级区
            areaPlayerDic[EnumLabyrinthArea.DemotionArea].Add(allPlayers[i]);
        }
        Logger.LogDetail(" 降级区有 {0}人  ", areaPlayerDic[EnumLabyrinthArea.DemotionArea].Count);
    }

    void _Packet(ScMazePlayerState p)
    {
        bool isSelfStateChange = false;
        bool isRefreshState = false;
        for (int i = 0; i < p.state.Length; i++)
        {
            isRefreshState = false;
            PMazePlayerState playerState = p.state[i];
            Dictionary<EnumLabyrinthArea, List<PMazePlayer>>.Enumerator e = areaPlayerDic.GetEnumerator();

            while (e.MoveNext())
            {
                List<PMazePlayer> player = e.Current.Value;
                for (int j = 0; j < player.Count; j++)
                {
                    if (modulePlayer.roleInfo.roleId == playerState.roleId)
                    {
                        isSelfStateChange = true;
                        labyrinthSelfInfo.mazeState = (byte)playerState.state;
                    }

                    if (player[j].roleId == playerState.roleId)
                    {
                        player[j].mazeState = (byte)playerState.state;
                        Logger.LogDetail("{0}  的状态修改成 {1}", player[j].roleName, GetLabyPlayerAllStateStr(player[j].mazeState));
                        isRefreshState = true;
                        break;
                    }
                }

                //如果修改了玩家的信息了，就停止while循环
                if (isRefreshState)
                    break;
            }
        }

        //如果自己的状态改变
        if (isSelfStateChange)
            DispatchModuleEvent(EventRefreshSelf, labyrinthSelfInfo);

        DispatchModuleEvent(EventRefreshPlayers, areaPlayerDic);
        DispatchModuleEvent(EventPlayerStateChange,p);
    }

    void _Packet(ScMazePlayerLayer p)
    {
        //刷新自己的面板显示
        if (labyrinthSelfInfo != null && p.roleId == modulePlayer.roleInfo.roleId)
        {
            labyrinthSelfInfo.mazeLayer = p.curLayer;
            DispatchModuleEvent(EventRefreshSelf, labyrinthSelfInfo);
        }

        //对玩家进行排序,并重新填充
        List<PMazePlayer> allPlayerList = new List<PMazePlayer>();
        //填充数据
        if (areaPlayerDic != null)
        {
            Dictionary<EnumLabyrinthArea, List<PMazePlayer>>.Enumerator e = areaPlayerDic.GetEnumerator();
            while (e.MoveNext())
            {
                allPlayerList.AddRange(e.Current.Value);
            }
        }

        for (int i = 0; i < allPlayerList.Count; i++)
        {
            if (allPlayerList[i].roleId == p.roleId)
            {
                allPlayerList[i].mazeLayer = p.curLayer;
                allPlayerList[i].layerTime = (int)p.time;
                break;
            }
        }

        allPlayerList.Sort((a, b) =>
        {
            //层数相同先过关的玩家优先显示
            if (a.mazeLayer == b.mazeLayer)
                return a.layerTime.CompareTo(b.layerTime);

            return -a.mazeLayer.CompareTo(b.mazeLayer);
        });

        RefreshAreaPlayers(allPlayerList);
        DispatchModuleEvent(EventRefreshPlayers, areaPlayerDic);
    }

    void _Packet(ScMazePropInfo p)
    {
        PItem2[] items = null;
        if (p.mazeProps != null)
        {
            p.mazeProps.CopyTo(ref items);
            for (int i = 0; i < items.Length; i++)
            {
                if (!m_labyrinthPropDic.ContainsKey(items[i].itemTypeId))
                    m_labyrinthPropDic.Add(items[i].itemTypeId, null);

                m_labyrinthPropDic[items[i].itemTypeId] = items[i];
            }
        }
        DispatchModuleEvent(EventRefreshProps, m_labyrinthPropDic);
    }

    void _Packet(ScUseTrapProp p)
    {
        if (p.result != 0)
        {
            ShowErrorTip(10009, p.result);
        }
        else
        {
            AddTrapToLabyrinth();
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.PublicUIText, 28));
            AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
        }

    }

    void _Packet(ScUsePlayerProp p)
    {
        if (p.result != 0)
        {
            ShowErrorTip(10008, p.result);
        }
        else
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.PublicUIText, 28));
            AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
        }
    }

    void _Packet(ScMazePropChange p)
    {
        if (m_labyrinthPropDic == null)
            m_labyrinthPropDic = new Dictionary<ushort, PItem2>();

        if (!m_labyrinthPropDic.ContainsKey(p.propId))
        {
            PItem2 newItem = PacketObject.Create<PItem2>();
            newItem.itemTypeId = p.propId;
            newItem.num = p.num;
            m_labyrinthPropDic.Add(newItem.itemTypeId, newItem);
        }
        else
        {
            m_labyrinthPropDic[p.propId].num = p.num;

            //移除使用完了的道具
            if (m_labyrinthPropDic[p.propId].num <= 0)
                m_labyrinthPropDic.Remove(p.propId);
        }

        DispatchModuleEvent(EventPropChange, m_labyrinthPropDic);
    }

    void _Packet(ScMazeChallenge p)
    {
        if (p.result == 0)
        {
            if (p.propIds != null)
            {
                string str = string.Empty;
                for (int i = 0; i < p.propIds.Length; i++)
                {
                    PropItemInfo item = ConfigManager.Get<PropItemInfo>(p.propIds[i]);
                    str += Util.Format("{0} {1} {2}  ", item.ID, (LabyrinthPropSubType)item.subType, item.itemName);
                }
                Logger.LogDetail("挑战触发的道具有 props ={0}", str);
            }
            modulePVE.OnScLabyrinthChallenge(p.stageId, p.propIds,p.overTimes);
            DispatchModuleEvent(EventChallengeLabyrinth, p.stageId);
        }
        else
        {
            ShowErrorTip(10005, p.result);
        }
    }

    void _Packet(ScMazeChallengeOver p)
    {
        if (p.result == 0)
        {
            modulePVE.SetPVESettlement(p.reward);

            //挑战成功的时候
            if (m_lastSendOverMsg != null && m_lastSendOverMsg.overState == (byte)PVEOverState.Success)
            {
                labyrinthSelfInfo.healthRate = m_lastSendOverMsg.overData.health;
                labyrinthSelfInfo.angerRate = m_lastSendOverMsg.overData.anger;
                Logger.LogDetail("结算消息重置血量和怒气health = {0},anger = {1}", labyrinthSelfInfo.healthRate, labyrinthSelfInfo.angerRate);

                //重置玩家自己的排名
                UpdateSelfLayerToPlayerPanel();
            }
        }
        else
        {
            ShowErrorTip(10004, p.result);
        }

        DispatchModuleEvent(EventChallengeOver);

        //无论消息成功还是失败，都清空消息缓存和陷阱道具数组
        m_lastSendOverMsg = null;
        if (modulePVE.labyrinthBuffPropIds != null) modulePVE.labyrinthBuffPropIds.Clear();
    }

    private void UpdateSelfLayerToPlayerPanel()
    {
        if (areaPlayerDic == null)
            return;

        Dictionary<EnumLabyrinthArea, List<PMazePlayer>>.Enumerator enumer = areaPlayerDic.GetEnumerator();
        while (enumer.MoveNext())
        {
            List<PMazePlayer> player = enumer.Current.Value;
            for (int i = 0; i < player.Count; i++)
            {
                if (player[i].roleId == modulePlayer.roleInfo.roleId)
                {
                    player[i].mazeLayer = labyrinthSelfInfo.mazeLayer;
                    break;
                }
            }
        }
        RefreshCurrentAllPlayers();
    }

    void _Packet(ScMazeSneak p)
    {
        if (p.result == 0)
        {
            //偷袭成功的时候需要重置状态
            modulePVE.isSendWin = false;
            isSneakSuccess = false;
            modulePVE.reopenPanelType = PVEReOpenPanel.Labyrinth;

            p.CopyTo(ref sneakPlayerDetail);
            DispatchModuleEvent(EventSneakPlayer);
        }
        else
        {
            ShowErrorTip(10006, p.result);
        }
    }

    void _Packet(ScMazeSceneSneakOver p)
    {
        byte overLayer = p.curLayer;
        //通过发送的消息是否成功来决定是否是直接将当前层数替换玩家层数
        //因为玩家的层数可以为0，所以不可以通过overlayer来决定是成功还是失败
        if (isSneakSuccess)
        {
            Dictionary<EnumLabyrinthArea, List<PMazePlayer>>.Enumerator enumer = areaPlayerDic.GetEnumerator();
            while (enumer.MoveNext())
            {
                List<PMazePlayer> player = enumer.Current.Value;
                for (int i = 0; i < player.Count; i++)
                {
                    if (player[i].roleId == lastSneakPlayer.roleId)
                    {
                        Logger.LogDetail("偷袭成功，玩家{0} 从{1}掉落到{2}层", player[i].roleName, player[i].mazeLayer, overLayer);
                        player[i].mazeLayer = overLayer;
                        break;
                    }
                }
            }
        }
        //只有偷袭失败才覆盖之前的血量
        else
        {
            labyrinthSelfInfo.healthRate = p.curHp;
        }

        Logger.LogDetail("偷袭后，自己的血量变为{0}%", labyrinthSelfInfo.healthRate);
        RefreshCurrentAllPlayers();
        DispatchModuleEvent(EventSneakOver);
    }

    private void RefreshCurrentAllPlayers()
    {
        //对玩家进行排序,并重新填充
        List<PMazePlayer> allPlayerList = new List<PMazePlayer>();
        Dictionary<EnumLabyrinthArea, List<PMazePlayer>>.Enumerator e = areaPlayerDic.GetEnumerator();
        while (e.MoveNext())
        {
            allPlayerList.AddRange(e.Current.Value);
        }
        allPlayerList.Sort((a, b) =>
        {
            //层数相同先过关的玩家优先显示
            if (a.mazeLayer == b.mazeLayer)
                return a.layerTime.CompareTo(b.layerTime);

            return -a.mazeLayer.CompareTo(b.mazeLayer);
        });

        //重置玩家的显示
        RefreshAreaPlayers(allPlayerList);
    }

    void _Packet(ScMazeBattleReport p)
    {
        PMazeReport[] reports = null;
        p.reports.CopyTo(ref reports);
        labyrinthReports.Clear();
        for (int i = 0; i < reports.Length; i++)
        {
            labyrinthReports.Add(reports[i]);
        }

        DispatchModuleEvent(EventBattleReport, labyrinthReports);
    }

    void _Packet(ScMazeNewReport p)
    {
        PMazeReport report = null;
        p.report.CopyTo(ref report);
        labyrinthReports.Insert(0, report);

        //如果是自己身上的炸弹爆炸了,就请求刷新一次自己的数据
        if (p.report.reportType == (byte)EnumLabyrinthReportType.SelfBombExplode)
        {
            Logger.LogDetail("收到自己身上的炸弹爆炸了....");
            SendLabyrinthSelfInfo();
        }

        DispatchModuleEvent(EventRefreshReport, labyrinthReports);
    }

    void _Packet(ScMazeStageChange p)
    {
        //状态改变了之后，重新请求迷宫时间
        SendLabyrinthOpenTime();
        //收到结算阶段，需要退出迷宫界面
        if (p.currentStage == (byte)EnumLabyrinthTimeStep.SettleMent)
        {
            DispatchModuleEvent(EventEnterSettlementStep);
            InitLabyrinthData();
        }
    }

    void _Packet(ScMazeEffectPropChange p)
    {
        if (p.propIds == null || p.propIds.Length == 0) return;

        ushort[] pr = new ushort[p.propIds.Length];
        System.Array.Copy(p.propIds,pr,p.propIds.Length);
        //更新自己的道具信息
        if (m_labyrinthSelfInfo == null)
        {
            Logger.LogError("recv ScMazeEffectPropChange msg but the self info is null.......");
            return;
        }
        
        m_labyrinthSelfInfo.attackProps = pr;
        DispatchModuleEvent(EventRefreshSelfAttackProp, m_labyrinthSelfInfo);


        if (m_labyrinthSelfInfo.attackProps != null)
        {
            string str = string.Empty;
            for (int i = 0; i < labyrinthSelfInfo.attackProps.Length; i++)
            {
                PropItemInfo item = ConfigManager.Get<PropItemInfo>(labyrinthSelfInfo.attackProps[i]);
                str += Util.Format("{0} {1} {2}  ", item.ID, (LabyrinthPropSubType)item.subType, item.itemName);
            }
            Logger.LogInfo("ScMazeEffectPropChange，props ={0}", str);
        }
    }

    #endregion

    #region 添加道具

    public void SendRequestTestData()
    {
        List<PropItemInfo> props = ConfigManager.GetAll<PropItemInfo>();
        List<string> items = new List<string>();

        foreach (var item in props)
        {
            if (item.itemType == PropType.LabyrinthProp)
                items.Add(Util.Format("{0},{1}", item.ID, 10));
        }

        CsRoleGm p = PacketObject.Create<CsRoleGm>();
        //添加道具
        p.gmType = 3;
        p.args = items.ToArray();
        session.Send(p);
    }

    #endregion

    #region 使用陷阱道具相关

    private void InitLabyrinthTrapDic(PMazeTrapInfo[] traps)
    {
        if (traps == null || traps.Length == 0) return;

        foreach (var item in traps)
        {
            if (!IsAlreadySetTrap(item.mazelayer)) m_labyrinthTrapDic.Add(item.mazelayer,0);
            m_labyrinthTrapDic[item.mazelayer] = item.trapProp;
        }
        
        DebugLabyrinthTrapDic("init.......");
    }

    public bool IsAlreadySetTrap(int layer)
    {
        return m_labyrinthTrapDic.ContainsKey(layer);
    }

    private void AddTrapToLabyrinth()
    {
        if (m_lastUseTrapProp == null) return;

        if (!IsAlreadySetTrap(m_lastUseTrapProp.mazeLayer)) m_labyrinthTrapDic.Add(m_lastUseTrapProp.mazeLayer,0);
        m_labyrinthTrapDic[m_lastUseTrapProp.mazeLayer] = m_lastUseTrapProp.propId;
        DebugLabyrinthTrapDic(Util.Format("add trap to layer = {0},trap prop = {1}...", m_lastUseTrapProp.mazeLayer, m_lastUseTrapProp.propId));
        DispatchModuleEvent(EventRefreshUseButtonState);
        m_lastUseTrapProp = null;
    }

    void _Packet(ScMazeTrapChange p)
    {
        int layer = p.trapInfo.mazelayer;
        if (IsAlreadySetTrap(layer)) m_labyrinthTrapDic.Remove(layer);
        DebugLabyrinthTrapDic(Util.Format("remove layer = {0},trap prop = {1}...",layer,p.trapInfo.trapProp));
    }

    private void DebugLabyrinthTrapDic(string tipMsg)
    {
#if UNITY_EDITOR
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(tipMsg);
        foreach (var item in m_labyrinthTrapDic)
        {
            PropItemInfo prop = ConfigManager.Get<PropItemInfo>(item.Value);
            sb.AppendFormat("layer = {0},trap id is {1},name is {2}\n",item.Key,prop.ID,prop.itemName);
        }
        Logger.LogInfo(sb.ToString());
#endif
    }

    public void SendPlayerPos(Vector3 pos)
    {
        CScMazeScenePlayerMove p = PacketObject.Create<CScMazeScenePlayerMove>();
        p.roleId = modulePlayer.roleInfo.roleId;
        p.pos = pos.ToPPostion();
        session.Send(p);
    }

    void _Packet(CScMazeScenePlayerMove p)
    {
        PMazePlayer player = GetTargetPlayer(p.roleId);
        if (player != null) player.pos = p.pos;

        DispatchModuleEvent(EventPlayerPosChange, p);
    }

    #endregion

    #region 触发器行为

    public void DispatchClickEvent(LabyrinthCreature c)
    {
        DispatchModuleEvent(EventTriggerSceneCollider, c);
    }

    #endregion
}
