/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-05-12
 * 
 ***************************************************************************************************/

using System;

public class Module_PVP : Session
{
    #region Static functions

    public new static Module_PVP instance { get { return m_instance; } }
    private static Module_PVP m_instance = null;

    #endregion

    #region Session

    private Module_PVP() : base(true)
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

    public const string EventLoadingStart       = "EventPVPLoadingStart";
    public const string EventLoadingProgress    = "EventPVPLoadingProgress";
    public const string EventLoadingComplete    = "EventPVPLoadingComplete";
    public const string EventEnterMatchRoom     = "EventPVPEnterMatchRoom";
    public const string EventRoomOver           = "EventPVPRoomOver";
    public const string EventRoomLostConnection = "EventPVPRoomLostConnection";
    public const string EventRoleOffLine        = "EventPVPRoleOffLine";   //玩家掉线

    #endregion

    #region Net

    private bool m_useGameSession = false;

    public void Connect(PRoomInfo room)
    {
        if (room == null)
        {
            Logger.LogError("Stream::Connect: Invalid room!");
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
        Logger.LogInfo("Stream connected to server [{0}:{1}], use game session: {2}", host, port, m_useGameSession);

        var p = PacketObject.Create<CsRoomEnter>();
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

        m_progress = 1;
        m_lastProgress = 1;

        ObjectManager.enableUpdate = true;

        session.RemoveEventListener(Events.SESSION_LOST_CONNECTION, OnLostConnection);
        EventManager.RemoveEventListener(Events.SCENE_LOAD_PROGRESS, OnLoadingProgress);

        DispatchModuleEvent(EventRoomLostConnection);

        m_useGameSession = false;
    }

    #endregion

    #region Basic

    public bool useGameSession { get { return m_useGameSession; } }
    public bool entered { get; private set; }
    public bool loading { get; private set; }
    public bool started { get { return entered && !loading; } }
    public ulong RoomId { get { return m_room.room_key; } }
    public bool ended { get; private set; }

    private PRoomInfo m_room     = null;
    private sbyte m_lastProgress = 0;
    private sbyte m_progress     = 0;

    public OpenWhichPvP opType { get; set; } = OpenWhichPvP.None;

    /// <summary>
    /// 开启 UI
    /// </summary>
    /// <param name="type"></param>
    public void Enter(OpenWhichPvP type)
    {
        modulePVP.opType = type;
        if (type == OpenWhichPvP.LolPvP && !moduleGlobal.IsStayLoLTime())
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.ActiveUIText,13));
            return;
        }
        else Window.ShowAsync<Window_PVP>();
    }

    public override void OnRootUpdate(float diff)
    {
        base.OnRootUpdate(diff);

        if (loading)
        {
            if (m_lastProgress != m_progress)
            {
                m_lastProgress = m_progress;

                if (FightRecordManager.IsRecovering)
                {
                    if (m_lastProgress == 100)
                    {
                        Logger.LogError("录像回放开始");
                        FightRecordManager.gameRecover?.GameRecordDataHandle?.OnLoadComplete();
                    }
                }
                else
                {
                    var p = PacketObject.Create<CsRoomLoading>();
                    p.progress = m_lastProgress;
                    Send(p);
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

    #endregion

    #region Packet handlers

    void _Packet(ScRoomEnter p)
    {
        EventManager.AddEventListener(Events.SCENE_LOAD_PROGRESS, OnLoadingProgress);

        FightRecordManager.Set(p);

        entered = p.result == 0;
        loading  = false;
        ended    = false;
        m_reward = null;
        m_settlementData = null;

        DispatchModuleEvent(EventEnterMatchRoom);
    }

    void _Packet(ScRoomLoading p)
    {
        Logger.LogInfo("Player {0}'s loading progress update to: {1}", p.guid, p.progress);

        DispatchModuleEvent(EventLoadingProgress, p);
    }

    void _Packet(ScRoomStart p)
    {
        Logger.LogInfo("PVP loading completed!");

        //rt = -1;
        //t = -1;

        loading = false;

        DispatchModuleEvent(EventLoadingComplete);

        ObjectManager.enableUpdate = false;

        Level.levelTime = 0;

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

    void _Packet(ScRoomStartLoading p)
    {
        FightRecordManager.Set(p);

        loading = true;

        m_progress     = 0;
        m_lastProgress = 0;

        Logger.LogInfo("Room start loading!");

        DispatchModuleEvent(EventLoadingStart);
    }

    void _Packet(ScPing p)
    {
        if ((p.type & 0x02) == 0) return;

        ping = Level.realTime - m_lastPing;
        if (!m_useGameSession) m_waitPing = pingInterval;
    }

    //float t = -1;
    //float rt = -1;
    //void _Packet(ScFrameUpdate p)
    //{
    //    //if (t < 0)
    //    //{
    //    //    t = Time.fixedTime;
    //    //    rt = p.diff * 0.001f;
    //    //    return;
    //    //}

    //    //var dd = Time.fixedTime - t;
    //    //rt += dd;

    //    //if (dd < 0.001f) Logger.LogError("frame = {4}, {0}, diff = {1}, nt: {2}, rt: {3}", (int)(dd * 1000), p.diff, (Level.levelTime * 0.001f).ToString("F2"), rt.ToString("F2"), lf);
    //    //t = Time.fixedTime;
    //}

    //void _Packet(ScFrameUpdateInput p)
    //{
    //    //if (t < 0)
    //    //{
    //    //    t = Time.fixedTime;
    //    //    rt = p.diff * 0.001f;
    //    //    return;
    //    //}

    //    //var dd = Time.fixedTime - t;
    //    //rt += dd;

    //    //if (dd < 0.001f) Logger.LogError("frame = {4}, {0}, diff = {1}, nt: {2}, rt: {3}", (int)(dd * 1000), p.diff, (Level.levelTime * 0.001f).ToString("F2"), rt.ToString("F2"), lf);
    //    //t = Time.fixedTime;
    //}

    #endregion

    #region PVP结算相关

    public PReward reward { get { return m_reward; } }
    private PReward m_reward = null;
    public ScFactionSelfBattleInfo settlementData { get { return m_settlementData; } }
    private ScFactionSelfBattleInfo m_settlementData;

    public bool isWinner { get; private set; }

    public bool isMatchAgain;//点击结算再来一局时,是否再次匹配
    public bool isInvation;//是否是从好友邀请来进行战斗的,如果是,则没有再来一局

    void _Packet(ScRoomRoleOffline p)
    {
        DispatchModuleEvent(EventRoleOffLine);
    }

    void _Packet(ScRoomOver p)
    {
        isWinner = p.winner == 0 || p.winner == modulePlayer.id_;

        ended = true;

        modulePVP.loading = false;

        Logger.LogInfo("Battle End: isWinner: {0}, reason: {1}", isWinner, p.reason);

        ObjectManager.enableUpdate = true;

        Disconnect();

        DispatchModuleEvent(EventRoomOver);
    }

    void _Packet(ScRoomReward p)
    {
        if (p.reward == null)
            m_reward = PacketObject.Create<PReward>();
        else
            p.reward.CopyTo(ref m_reward);
        FightRecordManager.Set(p);
    }

    void _Packet(ScFactionSelfBattleInfo p)
    {
        FightRecordManager.Set(p);
        p.CopyTo(ref m_settlementData);
    }

    protected override void OnGameDataReset()
    {
        m_reward           = null;
        m_settlementData   = null;
        isMatchAgain       = false;
        isInvation         = false;
        opType = OpenWhichPvP.None;

        ObjectManager.enableUpdate = true;
    }

    #endregion
}
