// /**************************************************************************************************
//  * Copyright (C) 2017-2018 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-05-28      10:03
//  *LastModify：2019-05-29      10:58
//  ***************************************************************************************************/

#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

public class Module_FactionBattle : Module<Module_FactionBattle>
{
    public enum Faction
    {
        None,
        Red,
        Blue
    }

    public enum State
    {
        /// <summary>
        /// 活动未开启
        /// </summary>
        Close,
        /// <summary>
        /// 普通状态，可报名
        /// </summary>
        Sign,
        /// <summary>
        /// 准备倒计时，停止报名
        /// </summary>
        Readly,
        /// <summary>
        /// 进行中
        /// </summary>
        Processing,
        /// <summary>
        /// 结算状态
        /// </summary>
        Settlement,
        /// <summary>
        /// 活动结束
        /// </summary>
        End
    }

    public const int MESSAGE_MAX = 20;
    private readonly List<string>       m_message = new List<string>();
    private ScFactionBattleInfo         m_battleInfo;
    private PActiveTime                 m_currentActiveTime;        //当前活动的时间段

    private ScFactionInfos              m_info;
    private ScFactionSelfBattleInfo     m_selfInfo;

    private ScFactionMatchInfo          m_factionMatchInfo;

    public ScFactionMatchInfo factionMatchInfo => m_factionMatchInfo;

    public IReadOnlyList<string> messageList => m_message;

    public State state
    {
        get { return m_info == null ? State.Close : (State)m_info.state; }
    }

    /// <summary>
    /// 是否是可报名状态
    /// </summary>
    public bool IsSignState => state == State.Sign;

    public bool IsActive    => state >= State.Sign && state < State.End;

    public bool IsProcessing => state == State.Processing;

    public bool IsChatActive => state >= State.Sign && state <= State.End;

    public bool IsSignIn    => m_info?.signIn ?? false;

    /// <summary>
    /// 活动倒计时
    /// </summary>
    public string CountDown
    {
        get
        {
            if (m_currentActiveTime == null)
                RefreshActiveTime();
            return null == m_currentActiveTime ? string.Empty :  Util.GetTimeFromSec(m_currentActiveTime.endTime - Util.GetTimeStamp(false, true), ":");
        }
    }

    /// <summary>
    /// 报名倒计时
    /// </summary>
    public string SignCountDown
    {
        get
        {
            if (m_currentActiveTime == null)
                RefreshActiveTime();
            return null == m_currentActiveTime ? string.Empty : Util.GetTimeFromSec(m_currentActiveTime.suEndTime - Util.GetTimeStamp(false, true), ":");
        }
    }

    /// <summary>
    /// 准备倒计时
    /// </summary>
    public string ReadlyCountDown
    {
        get
        {
            if (m_currentActiveTime == null)
                RefreshActiveTime();
            return null == m_currentActiveTime ? string.Empty : Util.GetTimeFromSec(m_currentActiveTime.startTime - Util.GetTimeStamp(false, true), ":");
        }
    }

    public int ReliveCountDown
    {
        get
        {
            var t = m_selfInfo.activeMatchTime - Util.GetTimeStamp(false, true);
            if (t < 0)
                return 0;
            return t;
        }
    }

    public string ActiveTime
    {
        get
        {
            if (m_info == null)
                return string.Empty;
            var s = string.Empty;
            Array.Sort(m_info.activeTime, (a, b) => a.startTime.CompareTo(b.startTime));
            for (var i = 0; i < m_info.activeTime.Length; i++)
            {
                if (!string.IsNullOrEmpty(s))
                    s += ";";
                var date = Util.GetDateTime(m_info.activeTime[i].startTime);
                var dateEnd = Util.GetDateTime(m_info.activeTime[i].endTime);
                s += ConfigText.GetDefalutString(559, (int)date.DayOfWeek);
                s += $"{date.Hour:00}:{date.Minute:00}-{dateEnd.Hour:00}:{dateEnd.Minute:00}";
            }
            return s;
        }
    }

    public float FactionPower
    {
        get
        {
            var scoreLeft = GetFactionInfo(Faction.Red)?.score ?? 0;
            var scoreRight = GetFactionInfo(Faction.Blue)?.score ?? 0;
            var baseValue = Mathf.Max(0, GeneralConfigInfo.sfactionScoreBase);
            var a = baseValue > 0 ? (float) (scoreLeft + baseValue) / baseValue : scoreLeft;
            var b = baseValue > 0 ? (float) (scoreRight + baseValue) / baseValue : scoreRight;

            return a / (a + b);
        }
    }

    public string SelfFactionName => FactionName(SelfFaction);

    public Faction SelfFaction => (Faction)(m_selfInfo?.self?.faction ?? 0);

    public uint SelfRank => m_selfInfo?.self.rank ?? 0;
    public uint SelfScore => m_selfInfo?.self.score ?? 0;

    public byte ComboKill => m_selfInfo?.self.comboKill ?? 0;
    public byte MaxComboKill => m_selfInfo?.self.maxCombokill ?? 0;

    /// <summary>
    /// 是否处于匹配中
    /// </summary>
    public bool Matching { get; set; }

    /// <summary>
    /// 进入阵营战窗口时，是否自动开始匹配
    /// </summary>
    public bool AutoMatch { get; set; }

    /// <summary>
    /// 是否处于匹配冷却倒计时
    /// </summary>
    public bool IsMatchCooling
    {
        get
        {
            if (m_selfInfo == null)
                return false;
            return m_selfInfo.activeMatchTime > Util.GetTimeStamp(false, true);
        }
    }

    public string MatchCoolTime
    {
        get
        {
            if (!IsMatchCooling)
                return string.Empty;
            return Util.GetTimeFromSec(m_selfInfo.activeMatchTime - Util.GetTimeStamp(false, true), ":");
        }
    }

    public bool IsWin
    {
        get
        {
            if (state != State.End && state != State.Settlement)
                return false;

            var redInfo = GetFactionInfo(Faction.Red);
            var blueInfo = GetFactionInfo(Faction.Blue);
            return redInfo.score >= blueInfo.score ? Faction.Red == SelfFaction : Faction.Blue == SelfFaction;
        }
    }

    public PBattleFactionInfo GetFactionInfo(Faction rFaction)
    {
        if (m_battleInfo == null)
            return null;
        if (m_battleInfo.info.Length < (int)rFaction || rFaction <= 0)
            return null;
        return m_battleInfo.info[(int)rFaction - 1];
    }

    /// <summary>
    /// 请求对战详情数据
    /// </summary>
    public void RequestBattleInfo()
    {
        var p = PacketObject.Create<CsFactionBattleInfo>();
        session.Send(p);
    }

    /// <summary>
    /// 请求报名
    /// </summary>
    /// <param name="rSignIn">报名/取消报名</param>
    public void RequestSignIn(bool rSignIn)
    {
        moduleGlobal.LockUI(0.5f);
        var p = PacketObject.Create<CsFactionSignIn>();
        p.signIn = rSignIn;
        session.Send(p);
    }

    /// <summary>
    /// 开始匹配
    /// </summary>
    public void RequestMatch(bool rMatch)
    {
        if (rMatch && IsMatchCooling)
            return;

        moduleGlobal.LockUI(0.5f);
        var p = PacketObject.Create<CsFactionMatch>();
        p.match = rMatch;
        session.Send(p);
    }

    public void RequestInWindow(bool rInWindow)
    {
        if (!IsProcessing)
            return;

        var p = PacketObject.Create<CsFactionInFactionWindow>();
        p.inWindow = rInWindow;
        session.Send(p);
    }

    private void AssertDataContainer()
    {
        if (m_info == null)
            m_info = PacketObject.Create<ScFactionInfos>();
    }

    /// <summary>
    /// 确定使用哪个时间段的活动
    /// </summary>
    private void RefreshActiveTime()
    {
        var t = Util.GetTimeStamp(false, true);
        if (state < State.Processing)
        {
            var offset = int.MaxValue;
            for (var i = 0; i < m_info.activeTime.Length; i++)
            {
                if (t <= m_info.activeTime[i].startTime)
                {
                    var o = m_info.activeTime[i].startTime - t;
                    if (o < offset)
                    {
                        offset = o;
                        m_currentActiveTime = m_info.activeTime[i];
                    }
                }
            }
        }
        else if (state == State.Processing)
        {
            for (var i = 0; i < m_info.activeTime.Length; i++)
            {
                if (t >= m_info.activeTime[i].startTime && t <= m_info.activeTime[i].endTime)
                {
                    m_currentActiveTime = m_info.activeTime[i];
                    break;
                }
            }
        }
    }

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();

        m_battleInfo?.Destroy();
        m_battleInfo = null;

        m_selfInfo?.Destroy();
        m_selfInfo = null;

        m_info?.Destroy();
        m_info = null;

        m_currentActiveTime?.Destroy();
        m_currentActiveTime = null;

        m_message.Clear();
    }

    protected override void OnModuleCreated()
    {
        base.OnModuleCreated();
        EventManager.AddEventListener(LevelEvents.BATTLE_END, OnFightEnd);
    }

    #region Events

    public const string EventGotFactionInfo    = "EventFactionGotFactionInfo";
    public const string EventStateChange       = "EventFactionStateChange";
    public const string EventSignStateChange   = "EventFactionSignStateChange";
    public const string EventBattleInfosChange = "EventFactionBattleInfoChange";
    public const string EventMessageListChange = "EventFactionMessageListChange";
    public const string ResponseStartMatch     = "EventFactionStartMatch";
    public const string EventSelfInfoChange    = "EventFactionSelfInfoChange";

    #endregion

    #region public functions

    public void OnFightEnd()
    {
        m_factionMatchInfo?.Destroy();
        m_factionMatchInfo = null;
    }
    #endregion

    #region static functions

    public static string FactionName(Faction rFaction)
    {
        switch (rFaction)
        {
            case Faction.Red:
                return ConfigText.GetDefalutString(TextForMatType.FactionBattleUI, 0);
            case Faction.Blue:
                return ConfigText.GetDefalutString(TextForMatType.FactionBattleUI, 1);
            case Faction.None:
                return ConfigText.GetDefalutString(TextForMatType.FactionBattleUI, 10);
        }
        return string.Empty;
    }

    public static string GetKillString(int rKillNum)
    {
        var info = ConfigManager.Find<FactionKillRewardInfo>(item => item.kill == rKillNum);
        if (info == null || info.name == 0)
            return string.Empty;
        return ConfigText.GetDefalutString(info.name);
    }

    public static string GetRankLabel(uint rRank)
    {
        if (rRank <= 3)
            return ConfigText.GetDefalutString(67, (int)rRank);
        return rRank.ToString();
    }

    #endregion

    #region Message

    private void _Packet(ScFactionMatchInfo rMatchInfo)
    {
        rMatchInfo.CopyTo(ref m_factionMatchInfo);
    }

    private void _Packet(ScFactionMatch msg)
    {
        moduleGlobal.UnLockUI();
        Matching = msg.matchState;
        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9859, msg.result);
        }

        DispatchModuleEvent(ResponseStartMatch, msg);
    }

    private void _Packet(ScFactionSignIn msg)
    {
        moduleGlobal.UnLockUI();
        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9858, msg.result);
        }

        AssertDataContainer();
        if (m_info.signIn == msg.signIn)
            return;
        m_info.signIn = msg.signIn;
        DispatchModuleEvent(EventSignStateChange);
    }

    private void _Packet(ScFactionInfos msg)
    {
        msg.CopyTo(ref m_info);
        RefreshActiveTime();
        DispatchModuleEvent(EventGotFactionInfo);
    }

    private void _Packet(ScFactionSelfBattleInfo msg)
    {
        msg.CopyTo(ref m_selfInfo);

        DispatchModuleEvent(EventSelfInfoChange);
    }

    private void _Packet(ScFactionBattleInfo msg)
    {
        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9857, msg.result);
            return;
        }
        AssertDataContainer();

        msg.CopyTo(ref m_battleInfo);

        if (m_selfInfo != null)
        {
            //排名中有自己，需要更新自己的数据
            var info = GetFactionInfo(SelfFaction);
            var selfData = info?.members.Find(p => p.info.roleId == modulePlayer.id_);
            if (selfData != null)
            {
                selfData.battleInfo.CopyTo(m_selfInfo.self);
                DispatchModuleEvent(EventSelfInfoChange);
            }
        }

        DispatchModuleEvent(EventBattleInfosChange);
    }

    private void _Packet(ScFactionStateChange msg)
    {
        AssertDataContainer();
        m_info.state = msg.state;
        RefreshActiveTime();
        DispatchModuleEvent(EventStateChange);

        if (m_info.state == (byte)State.Settlement && Matching)
            RequestMatch(false);
    }

    private void _Packet(ScFactionMessage p)
    {
        string msg = null;
        if (p.killCntId > 0)
        {
            var info = ConfigManager.Get<FactionKillRewardInfo>(p.killCntId);
            if (info == null)
                return;
            var objs = new List<object>(p.msg.Length);
            for (var i = 0; i < p.msg.Length; i++)
                objs.Add(p.msg[i]);

            if (p.getOrOver == 1)
                objs.Add(ConfigText.GetDefalutString(info.name));
            else
                objs.Add(p.killCntId);

            msg = Util.Format(ConfigText.GetDefalutString(p.getOrOver == 1 ? info.getMsg : info.overMsg), objs.ToArray());
        }
        else if (p.msg.Length > 0)
            msg = p.msg[0];

        if (!string.IsNullOrEmpty(msg))
        {
            do
            {
                var mr = Module_Global.factionRegex.Match(msg);
                if (!mr.Success) break;
                msg = msg.Replace(mr.Groups[0].Value,
                    FactionName((Faction)Util.Parse<int>(mr.Groups[1].Value)));
            } while (true);
        }

        m_message.Add(msg);

        if (m_message.Count > MESSAGE_MAX)
            m_message.RemoveAt(0);

        DispatchModuleEvent(EventMessageListChange, msg);
    }

    #endregion
}
