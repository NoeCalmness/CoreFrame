/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Team module
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-08-16
 * 
 ***************************************************************************************************/
 
using System;
using System.Collections.Generic;

/// <summary>
/// 队伍模式，用以区分是属于何种模块的组队
/// 比如 觉醒 剧情 武器神化等
/// </summary>
public enum TeamMode
{
    /// <summary>
    /// 觉醒
    /// </summary>
    Awake = 0,
    /// <summary>
    /// 剧情
    /// </summary>
    Story = 1,
}

public enum EnumTeamQuitState
{
    /// <summary>
    /// 成功退出
    /// </summary>
    QuitSuccess = 0,

    /// <summary>
    /// 退出失败
    /// </summary>
    QuitFailed = 1,

    /// <summary>
    /// 游戏中途掉线
    /// </summary>
    DroppedDuringGame = 2,

    /// <summary>
    /// 游戏还在加载掉线
    /// </summary>
    DroppedDuringLoad = 3,

    /// <summary>
    /// 提前结算
    /// </summary>
    EarlySettlement = 4,
}

/// <summary>
/// Module_Team 只会接管多人组队的开始和结束，
/// </summary>
public class Module_Team : Session
{
    #region Static functions

    public new static Module_Team instance { get { return m_instance; } }
    private static Module_Team m_instance = null;

    #endregion

    #region Session

    private Module_Team() : base(true)
    {
        if (m_instance != null)
            throw new Exception("Can not create " + GetType().Name + " twice.");

        m_instance = this;

        pingInterval = 1.0f;
    }

    protected override void OnModuleCreated()
    {
        m_room = PacketObject.Create<PRoomInfo>();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        m_instance = null;
    }

    #endregion

    #region Events

    /// <summary>
    /// 开始连接房间服务器 msg = PRoomInfo
    /// </summary>
    public const string EventConnecting         = "EventTeamConnecting";
    /// <summary>
    /// 连接房间服务器失败 param1 = reason 参考 ScTeamRoomCancled.reason
    /// </summary>
    public const string EventRoomCancled        = "EventTeamRoomCancled";
    /// <summary>
    /// 成功连接房间服务器
    /// </summary>
    public const string EventRoomConnected      = "EventTeamRoomConnected";
    /// <summary>
    /// 从房间服务器断开连接
    /// </summary>
    public const string EventRoomLostConnection = "EventTeamRoomLostConnection";
    /// <summary>
    /// 开始加载场景
    /// </summary>
    public const string EventLoadingStart       = "EventTeamLoadingStart";
    /// <summary>
    /// 队伍成员加载进度变化 msg = ScTeamLoading
    /// </summary>
    public const string EventLoadingProgress    = "EventTeamLoadingProgress";
    /// <summary>
    /// 战斗开始
    /// 所有队伍成员都准备就绪
    /// </summary>
    public const string EventStart              = "EventTeamStart";
    /// <summary>
    /// 所有玩家都已经触发TransportScene
    /// </summary>
    public const string EventTransportStart     = "EventTransportStart";
    /// <summary>
    /// 战斗恢复
    /// 所有队伍成员都转移场景完毕
    /// </summary>
    public const string EventTransportOver      = "EventTransportOver";
    /// <summary>
    /// 战斗结束
    /// </summary>
    public const string EventEnd                = "EventTeamEnd";
    /// <summary>
    /// 退出队伍 msg = ScTeamQuit
    /// </summary>
    public const string EventQuit               = "EventTeamQuit";
    /// <summary>
    /// 队伍成员复活 param1 = PTeamMemberInfo(复活的成员信息)
    /// </summary>
    public const string EventMemberReborn       = "EventMemberReborn";

    #endregion

    #region Net

    private bool m_useGameSession = false;

    public void Connect(PRoomInfo room)
    {
        if (room == null)
        {
            Logger.LogError("Module_Team::Connect: Room can not be null!");
            return;
        }

        room.CopyTo(m_room);

        UpdateServer(m_room.host, m_room.port);

        session.RemoveEventListener(Events.SESSION_LOST_CONNECTION, OnLostConnection);

        m_useGameSession = false;
        if (host == session.host && port == session.port)
        {
            m_useGameSession = true;

            session.AddEventListener(Events.SESSION_LOST_CONNECTION, OnLostConnection);
            OnConnected();
        }
        else Connect();
    }

    protected override void OnConnected()
    {
        Logger.LogInfo("Team connected to server [{0}:{1}], use game session: {2}", host, port, m_useGameSession);

        var p = PacketObject.Create<CsTeamRoomEnter>();
        p.roleId = modulePlayer.roleInfo.roleId;
        p.roomKey = m_room.room_key;

        Send(p);

        if (!m_useGameSession) SendPing();

        #region NetStat statistic
        #if NETSTAT
            var r = m_useGameSession ? session.receiver : receiver;
            if (r != null) r.netStat = true;
        #endif
        #endregion
    }

    protected override void OnLostConnection()
    {
        entered = false;
        loading = false;
        m_loadingType = EnumPVELoadingType.None;

        stage = -1;
        allowQuit = true;
        timeLimit = -1;

        m_progress = 1;
        m_lastProgress = 1;

        m_playerIndex = -1;
        m_members = null;
        onlineMembers.Clear();
        result = -1;
        sendTeamEnd = false;

        ObjectManager.enableUpdate = true;

        session.RemoveEventListener(Events.SESSION_LOST_CONNECTION, OnLostConnection);
        EventManager.RemoveEventListener(Events.SCENE_LOAD_PROGRESS, OnLoadingProgress);

        DispatchModuleEvent(EventRoomLostConnection);

        m_useGameSession = false;
    }

    void _Packet(ScPing p)
    {
        if (p.type != 4) return;

        ping = Level.realTime - m_lastPing;
        if (!m_useGameSession) m_waitPing = pingInterval;
    }

    #endregion

    #region Basic

    public bool useGameSession { get { return m_useGameSession; } }
    public bool entered { get; private set; }
    public bool loading { get; private set; }
    public bool started { get { return entered && !loading; } }
    public bool ended { get; private set; }
    /// <summary>
    /// 战斗结果，参考 CsTeamRequestEnd.result
    /// </summary>
    public int result { get; private set; }
    /// <summary>
    /// 当前正在进行的 stage 若不在队伍状态，stage 为 -1
    /// </summary>
    public int stage { get; private set; }
    /// <summary>
    /// 当前的队伍模式
    /// </summary>
    public TeamMode mode { get; private set; }
    /// <summary>
    /// 当前队伍关卡的时间限制
    /// </summary>
    public int timeLimit { get; private set; }
    /// <summary>
    /// 是否允许主动退出
    /// </summary>
    public bool allowQuit { get; private set; }

    /// <summary>
    /// Ordered team members order by room index
    /// </summary>
    public PTeamMemberInfo[] members { get { return m_members; } }
    private PTeamMemberInfo[] m_members = null;

    private int m_playerIndex = -1;

    private PRoomInfo m_room     = null;
    private sbyte m_lastProgress = 0;
    private sbyte m_progress     = 0;
    private EnumPVELoadingType m_loadingType = EnumPVELoadingType.None;

    /// <summary>
    /// 还在线的玩家，用于判定是否应该更换成单机pve模式
    /// </summary>
    public List<PTeamMemberInfo> onlineMembers { get; private set; } = new List<PTeamMemberInfo>();
    public bool sendTeamEnd { get; private set; } = false;

    public ulong RoomID { get { return m_room?.room_key ?? 0; } }

    public override void OnRootUpdate(float diff)
    {
        base.OnRootUpdate(diff);

        if (loading && m_loadingType != EnumPVELoadingType.None)
        {
            if (m_lastProgress != m_progress)
            {
                m_lastProgress = m_progress;

                if (!FightRecordManager.IsRecovering)
                {
                    var p = PacketObject.Create<CsTeamRoomLoading>();
                    p.progress = m_lastProgress;
                    p.type = (sbyte)m_loadingType;
                    Send(p);
                }

                if (m_lastProgress == 100)
                {
                    if (FightRecordManager.IsRecovering)
                    {
                        if(!FightRecordManager.gameRecover?.GameRecordDataHandle?.OnLoadComplete() ?? true)
                            FightRecordManager.gameRecover?.GameRecordDataHandle?.OnTransportComplete();
                    }
                    moduleLoading.SetLoadingInfo(8);
                }
            }
        }
    }

    public override void Send(PacketObject packet)
    {
        if (m_useGameSession) session.Send(packet);
        else base.Send(packet);
    }

    private void OnLoadingProgress(Event_ e)
    {
        if (!loading) return;
        if (m_progress < 100)
            m_progress = (sbyte)Math.Floor((float)e.param1 * 100.0f);
    }

    protected override void OnGameDataReset()
    {
        if (!m_useGameSession) Disconnect();

        ObjectManager.enableUpdate = true;
    }

    #endregion

    #region Public interface

    public PTeamMemberInfo GetMember(int index = -1)
    {
        if (index < 0) index = m_playerIndex;
        return m_members == null || index < 0 ? null : m_members[index];
    }

    public int GetMemberIndex(ulong roleID)
    {
        return m_members != null ? m_members.FindIndex(p => p.roleId == roleID) : -1;
    }

    public void SendEndRequest(PVEOverState state)
    {
        sendTeamEnd = true;
        var p = PacketObject.Create<CsTeamRequestEnd>();
        p.pveState = (byte)state;
        p.overData = modulePVE.GetPveDatas();

        var level = Level.current as Level_Battle;
        p.hp = level ? level.GetHpForVerification() : new double[] { 0 };

        Send(p);
    }

    public void QuitTeam(bool force = false)
    {
        if (!force && !allowQuit)
        {
            moduleGlobal.ShowMessage(26);
            return;
        }

        if (modulePVE.isTeamMode && !sendTeamEnd)
        {
            var p = PacketObject.Create<CsTeamQuit>();
            Send(p);
        }

        if (!useGameSession) Disconnect();
    }

    public void StartTransportScene()
    {
        var p = PacketObject.Create<CsTeamTransportScene>();
        Send(p);
    }

    public bool IsOnlineMember(ulong roleId)
    {
        if (onlineMembers == null || onlineMembers.Count == 0 || !modulePVE.isTeamMode) return true;
        return onlineMembers.FindIndex(o => o.roleId == roleId) >= 0;
    }
    
    public void SendStoryStep(int id, int index, EnumContextStep step)
    {
        CScTeamBehaviour p = PacketObject.Create<CScTeamBehaviour>();
        p.intParams = new int[4];
        p.intParams[0] = (int)Module_Battle.FrameAction.StoryChangeStep;
        p.intParams[1] = id;
        p.intParams[2] = index;
        p.intParams[3] = (int)step;

        Send(p);
    }

    #endregion

    #region Packet handlers

    void _Packet(ScTeamRoom p)
    {
        moduleAwakeMatch.enterGame = true;
        FightRecordManager.InstanceHandle<GameRecordDataTeam>();
        Connect(p.room);
    }

    void _Packet(ScTeamRoomEnter p)
    {
        Logger.LogInfo("Enter team room: {0}", p.result);

        entered  = p.result == 0;
        loading  = false;
        m_loadingType = EnumPVELoadingType.None;
        ended    = false;
        result   = -1;

        DispatchModuleEvent(EventRoomConnected);
    }

    void _Packet(ScTeamRoomCancled p)
    {
        DispatchModuleEvent(EventRoomCancled, p.reason);
        if (connected) Disconnect();
    }

    void _Packet(ScTeamStartLoading p)
    {
        EventManager.RemoveEventListener(Events.SCENE_LOAD_PROGRESS, OnLoadingProgress);
        EventManager.AddEventListener(Events.SCENE_LOAD_PROGRESS, OnLoadingProgress);

        Logger.LogInfo("Team room start loading! Stage: [{0}]", p.stage);
        if (p.members.Length == 1)
            FightRecordManager.EndRecord(false, false);
        FightRecordManager.Set(p);

        loading = true;
        m_loadingType = EnumPVELoadingType.Initialize;
        m_progress = 0;
        m_lastProgress = 0;

        mode = (TeamMode)p.type;
        stage = p.stage;
        timeLimit = p.timeLimit;
        allowQuit = p.allowQuit;

        p.members.CopyTo(ref m_members);
        onlineMembers.Clear();
        onlineMembers.AddRange(m_members);

        FightRecordManager.RecordLog< LogInt>(log =>
        {
            log.tag = (byte)TagType.onlineMembersCount;
            log.value = onlineMembers?.Count ?? 0;
        });

        m_playerIndex = m_members.FindIndex(mi => mi.roleId == modulePlayer.id_);

        Logger.LogDetail("Player roomIndex: {0}", m_playerIndex);

        DispatchModuleEvent(EventLoadingStart);

        modulePVE.StartTeamLevel(p);
        loading = true;
    }

    void _Packet(ScTeamRoomLoading p)
    {
        Logger.LogInfo("Player {0}'s loading progress update to: {1} type is {2}", p.guid, p.progress,(EnumPVELoadingType)p.type);

        DispatchModuleEvent(EventLoadingProgress, p);
    }

    void _Packet(ScTeamStart p)
    {
        Logger.LogInfo("Team loading completed!");
        FightRecordManager.Set(p);
        FightRecordManager.StartRecord();

        loading = false;
        m_loadingType = EnumPVELoadingType.None;
        
        Level.levelTime = 0;
        ObjectManager.enableUpdate = false;

        DispatchModuleEvent(EventStart);
        
        #region NetStat statistic
        #if NETSTAT
        var r = m_useGameSession ? session.receiver : receiver;
        if (r != null)
        {
            r.ClearStat();
            r.pauseNetStatistic = false;
        }
        #endif
        #endregion
    }

    public void HandleStartTransportFrameEvent()
    {
        DispatchModuleEvent(EventTransportStart);
        loading = true;
        m_loadingType = EnumPVELoadingType.TransportScene;
        m_progress = 0;
        m_lastProgress = 0;
    }

    public void TransportOver()
    {
        m_loadingType = EnumPVELoadingType.None;
        loading = false;

        DispatchModuleEvent(EventTransportOver);
    }

    void _Packet(ScTeamQuit p)
    {
        var quitType = (EnumTeamQuitState)p.reason;
        Logger.LogDetail("guid = {0} Quit team, reason: {1}", p.guid, quitType);

        if(quitType != EnumTeamQuitState.QuitFailed)
        {
            var memberData = members.GetValue<PTeamMemberInfo>(p.guid);
            if (memberData == null)
            {
                Logger.LogError("Team member [{0}] cannot be finded", p.guid);
                return;
            }

            for (int i = 0; i < onlineMembers.Count; i++)
            {
                if (onlineMembers[i].roleId == memberData.roleId)
                {
                    onlineMembers.RemoveAt(i);
                    break;
                }
            }
            Logger.LogInfo("after memember quit , current online member count has {0}",onlineMembers.Count);
            DispatchModuleEvent(EventQuit, memberData, quitType);
        }
    }

    public void DispatchReborn(PTeamMemberInfo memberData)
    {
        DispatchModuleEvent(EventMemberReborn, memberData);
    }

    //void _Packet(ScTeamReborn p)
    //{
    //    Logger.LogDetail("Team member [{0}] reborn", p.guid);
    //    var memberData = members.GetValue<PTeamMemberInfo>(p.guid);
    //    if (memberData == null)
    //    {
    //        Logger.LogError("Team member [{0}] cannot be finded", p.guid);
    //        return;
    //    }
        
    //    DispatchModuleEvent(EventMemberReborn, memberData);
    //}

    void _Packet(ScTeamRequestEnd p)
    {
        result = p.result;

        ended = true;

        Logger.LogInfo("Team Battle End: result: {0}", p.result);

        ObjectManager.enableUpdate = true;

        DispatchModuleEvent(EventEnd);
    }
    #endregion
}
