/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-05-24
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

public class Module_Match : Module<Module_Match>
{
    public const string EventMatchSuccessed = "EventMatchSuccessed";
    public const string EventMatchInfo = "EventMathchInfo";
    public const string EventMatchCancle = "EventMatchCancle";
    public const string EventMatchFailed = "EventMatchFailed";

    public const string EventInvationSend = "EventInvationSend";   //邀请申请是否发送成功 跟匹配成功一样流程
    public const string EventInvationFailed = "EventInvationFailed"; //邀请好友失败
    public const string EventInvationsucced = "EventInvationsucced"; //邀请好友成功
    public const string EventFriendnMatchCancle = "EventFriendnMatchCancle";//取消好友邀请

    public const string EventComeBackHomeScence = "EventComeBackHomeScence";//从其他场景回到home场景 并打开pvp

    #region 排位赛
    public const string EventDetailRankingForRequest = "EventDetailRankingForRequest";    //请求的详细排名
    public const string EventRankingStart = "EventRankingStart";//段位赛开始
    public const string EventRankingEnd = "EventRankingEnd";//段位赛结束

    public ScWorldRankInfo rankInfo { get { return m_rankInfo; } }
    private ScWorldRankInfo m_rankInfo;

    public List<PRank> detailRanking { get { return m_detailRanking; } }
    private List<PRank> m_detailRanking = new List<PRank>();

    public bool isUpDanlv { get; set; }//是否升段位
    #endregion

    public int timeLimit { get; private set; } = CombatConfig.sdefaultTimeLimit;

    public bool isMatchRobot { get; private set; }

    /// <summary>
    /// Ordered players order by room index
    /// </summary>
    public PMatchInfo[] players { get { return m_players; } }
    private PMatchInfo[] m_players = null;

    private int m_playerIndex = -1;

    #region 邀请

    public bool isbaning = true;

    private int playerfriend;

    public bool beiInvated { get; set; }

   public List<PPlayerInfo> m_invateCheck = new List<PPlayerInfo>();//选中的

    public List<PPlayerInfo> m_friendOnline = new List<PPlayerInfo>();//在线的 （打开界面时）

    public Dictionary<ulong, int> m_remainTime = new Dictionary<ulong, int>();// key remain

    public ScMatchInviteSuccess Info_sss;

    void _Packet(ScMatchInviteSuccess p)
    {
        //邀请成功
        p.CopyTo(ref Info_sss);

        modulePlayer.roleInfo.friendPvpTimes++;//好友pvpc次数加一

        Logger.LogInfo("Match info: players: {0}, room: [{1}:{2}, {3}]", p.infoList.Length, p.room.host, p.room.port, p.room.room_key);

        p.infoList.CopyTo(ref m_players);

        for (var i = 0; i < m_players.Length; ++i)
            Logger.LogInfo("index: {2}, role: {1}[{0}],weapon: {3} gun: {4}", m_players[i].roleId, m_players[i].roleName, i, m_players[i].fashion.weapon, m_players[i].fashion.gun);

        m_playerIndex = m_players.FindIndex(pi => pi.roleId == modulePlayer.id_);

        Logger.LogDetail("Player roomIndex: {0}", m_playerIndex);
        FightRecordManager.InstanceHandle<GameRecordDataPvp>();
        FightRecordManager.SetMatchInfo(p.infoList);

        ScMatchInviteSuccess info = null;
        p.CopyTo(ref info);
        DispatchModuleEvent(EventInvationsucced, info);
        isbaning = false;
        isMatchRobot = false;
    }

    void _Packet(ScRefuseInvite p)
    {
        playerfriend--;
        if (playerfriend == 0)
        {
            Friedreplay();
        }
    }

    void _Packet(ScMatchInvite p)
    {
        if (p.result != 0)
        {
            playerfriend--;
            if (playerfriend == 0)
            {
                Friedreplay(p.result == 10);
            }
        }
    }

    public void Friedreplay(bool isMd5Diffrent = false)
    {
        //成功加载后就不能再处理加载失败操作了。（如果对方同意邀请后，再次收到邀请会返回邀请失败）
        if (modulePVP.loading)
            return;

        //没有好友同意到我的邀请
        isbaning = true;
        DispatchModuleEvent(EventInvationFailed, isMd5Diffrent);
        if (isMd5Diffrent)
        {
            var ct = ConfigManager.Get<ConfigText>(25);
            if (moduleGlobal.CheckSourceMd5(false))
                Window_Alert.ShowAlertDefalut(ct[6], () => { });
            else
                Window_Alert.ShowAlertDefalut(ct[7], Game.Quit, ()=> {}, ct[4], ct[8], false);
        }
    }

    public void FriendFree(List<ulong> friendcheck)
    {
        //发送选中的好友  
        playerfriend = friendcheck.Count;
        for (int i = 0; i < friendcheck.Count; i++)
        {
            var p = PacketObject.Create<CsMatchInvite>();
            p.targetId = friendcheck[i];
            p.pvpType = 1;
            session.Send(p);

            //邀请的最新时间
            var key = "invation" + modulePlayer.id_.ToString() + friendcheck[i].ToString();
            PlayerPrefs.SetString(key, Util.GetServerLocalTime().ToString());
        }
        DispatchModuleEvent(EventInvationSend);
        isbaning = false;//我在邀请状态了不能接受他人邀请
        m_invateCheck.Clear();

        if (playerfriend == 0) Friedreplay();
    }
    public void AllRemainTime()
    {
        m_remainTime.Clear();
        for (int i = 0; i < m_friendOnline.Count; i++)
        {
            var rtime = CanInvation(m_friendOnline[i].roleId);
            m_remainTime.Add(m_friendOnline[i].roleId, rtime);
        }
    }

    public void OnlineFriend()
    {
        m_friendOnline.Clear();
        List<PPlayerInfo> thisInfo = moduleFriend.FriendList.FindAll(p => p.state != 0);
        for (int i = 0; i < thisInfo.Count; i++)
        {
            PPlayerInfo info = null;
            thisInfo[i].CopyTo(ref info);
            if (info == null)
            {
                Logger.LogError(" error");
                continue;
            }
            m_friendOnline.Add(info);
        }
    }

    //判断是否可以进行邀请
    public int CanInvation(ulong playerId)
    {
        int remainTime = 0;
        var str = "invation" + modulePlayer.id_.ToString() + playerId.ToString();
        var value = PlayerPrefs.GetString(str);
        if (string.IsNullOrEmpty(value)) return 0;
        else
        {
            DateTime lastTime = DateTime.Parse(value);
            DateTime nowTime = Util.GetServerLocalTime();
            TimeSpan clrpTime = nowTime - lastTime;

            int seconds = clrpTime.Hours * 3600 + clrpTime.Minutes * 60 + clrpTime.Seconds;
            int poor = GeneralConfigInfo.defaultConfig.invateInterval;//间隔
            remainTime = poor - seconds;
        }
        return remainTime;
    }
    public void CancleFriendInvate()
    {
        //取消好友邀请
        CsInviteCancel p = PacketObject.Create<CsInviteCancel>();
        session.Send(p);
    }
    void _Packet(ScInviteCancel p)
    {
        if (p.result == 0)
        {
            isbaning = true;
            playerfriend = 0;
            DispatchModuleEvent(EventFriendnMatchCancle);
        }
        Logger.LogInfo("Cancle match: {0}", 0);
    }
    #endregion


    public PMatchInfo GetPlayer(int index = -1)
    {
        if (index < 0) index = m_playerIndex;
        return m_players == null || index < 0 ? null : m_players[index];
    }

    public byte GetPlayerIndex(ulong roleID)
    {
        return (byte)m_players.FindIndex(p => p.roleId == roleID);
    }

    public void Match()
    {
        session.Send(PacketObject.Create<CsMatchSingle>());
    }

    public void CancleMatch()
    {
        session.Send(PacketObject.Create<CsMatchCancel>());
    }

    void _Packet(ScMatchSingle p)
    {
        Logger.LogInfo("Match result: {0}, {1}", p.result, p.estimated);

        if (p.result == 0) DispatchModuleEvent(EventMatchSuccessed, p.estimated);
        else DispatchModuleEvent(EventMatchFailed, p.result);
    }

    void _Packet(ScMatchInfo p)
    {
        Logger.LogInfo("Match info: isRobot: {5}, timeLimit: {4}, players: {0}, room: [{1}:{2}, {3}]", p.infoList.Length, p.room.host, p.room.port, p.room.room_key, p.timeLimit, p.isRobot);

        isMatchRobot = p.isRobot;
        timeLimit = p.timeLimit;

        p.infoList.CopyTo(ref m_players);

        m_playerIndex = m_players.FindIndex(pi => pi.roleId == modulePlayer.id_);

        Logger.LogDetail("Player roomIndex: {0}", m_playerIndex);

        modulePVP.Connect(p.room);

        ScMatchInfo matchInfo = null;
        p.CopyTo(ref matchInfo);

        DispatchModuleEvent(EventMatchInfo, matchInfo);

        if (!isMatchRobot)
        {
            FightRecordManager.InstanceHandle<GameRecordDataPvp>();
            FightRecordManager.Set(p);
        }
    }

    void _Packet(ScMatchCancel p)
    {
        if (p.result == 0)
        {
            m_players = null;
            isMatchRobot = false;
            DispatchModuleEvent(EventMatchCancle);
        }
        Logger.LogInfo("Cancle match: {0}", p.result);
    }

    #region 排位赛相关

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        SendDetailRanking();
    }

    void _Packet(ScWorldRankStart p)
    {
        DispatchModuleEvent(EventRankingStart);
    }

    void _Packet(ScWorldRankEnded p)
    {
        DispatchModuleEvent(EventRankingEnd);
    }

    public void RepareRankData()
    {
        if (m_rankInfo != null)
            DispatchModuleEvent(EventDetailRankingForRequest);
        else
            SendDetailRanking();
    }

    public void SendPvPRankMatch()
    {
        CsMatchPvprank p = PacketObject.Create<CsMatchPvprank>();
        session.Send(p);
    }

    void _Packet(ScMatchPvprank p)
    {
        if (p.result == 0) DispatchModuleEvent(EventMatchSuccessed, p.estimated);
        else DispatchModuleEvent(EventMatchFailed, p.result);

        switch (p.result)
        {
            case 1: moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.MatchUIText, 33)); break;
            case 2: moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.MatchUIText, 34)); break;
            case 3: moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.MatchUIText, 35)); break;
            case 4: moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.MatchUIText, 36)); break;
            default: break;
        }
    }

    private void SendDetailRanking()
    {
        CsWorldRankInfo p = PacketObject.Create<CsWorldRankInfo>();
        session.Send(p);
    }

    void _Packet(ScWorldRankInfo p)
    {
        if (p.ranks != null)
        {
            PRank[] ranks = null;
            p.CopyTo(ref m_rankInfo);
            p.ranks.CopyTo(ref ranks);

            m_detailRanking.Clear();
            m_detailRanking.AddRange(ranks);
            if (m_detailRanking.Count > 1) m_detailRanking.Sort((a, b) => a.rank.CompareTo(b.rank));

            DispatchModuleEvent(EventDetailRankingForRequest);
        }
    }

    void _Packet(ScWorldRankUpdate p)
    {
        if (p.ranks != null && p.ranks.Length > 0)
        {
            PRank[] ranks = null;
            p.ranks.CopyTo(ref ranks);
            m_detailRanking.Clear();
            m_detailRanking.AddRange(ranks);
            if (m_detailRanking.Count > 1) m_detailRanking.Sort((a, b) => a.rank.CompareTo(b.rank));

            for (int i = 0; i < m_detailRanking.Count; i++)
            {
                if (m_detailRanking[i].roleId == modulePlayer.roleInfo.roleId)
                {
                    if (m_rankInfo != null)
                    {
                        m_rankInfo.rank = m_detailRanking[i].rank;
                        break;
                    }
                }
            }
        }
    }

    void _Packet(ScRoleRankDan p)
    {
        if (p != null && m_rankInfo != null)
        {
            isUpDanlv = m_rankInfo.danLv < p.danLv;
            m_rankInfo.danLv = p.danLv;
        }
    }

    void _Packet(ScRoleRankScore p)
    {
        if (p != null && m_rankInfo != null)
            m_rankInfo.score = (uint)p.score;
    }

    //测试
    public void CreatTestForRanking()
    {
        _Packet(RankingTest());
    }

    private ScWorldRankInfo RankingTest()
    {
        ScWorldRankInfo p = PacketObject.Create<ScWorldRankInfo>();

        p.rank = 3;
        p.danLv = 5;
        p.score = 1600;

        List<PRank> rankings = new List<PRank>();
        for (int i = 0; i < 100; i++)
        {
            PRank ranking = PacketObject.Create<PRank>();
            ranking.rank = (ushort)(i + 1);
            ranking.danLv = (byte)UnityEngine.Random.Range(1, 9);
            ranking.name = Util.Format("女生鳝变 {0}", i);
            ranking.guild = Util.Format("鳝变的工会 {0}", i);
            ranking.score = (ushort)(1000 + (500 - i));
            rankings.Add(ranking);
        }
        p.ranks = rankings.ToArray();
        return p;
    }

    #endregion

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        m_rankInfo = null;
        m_detailRanking.Clear();
        isUpDanlv = false;
        m_players = null;
        Info_sss = null;
        isbaning = true;
        beiInvated = false;
    }
}