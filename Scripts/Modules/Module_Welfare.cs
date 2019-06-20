/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-08-21
 * 
 ***************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Module_Welfare : Module<Module_Welfare>
{
    /// <summary>
    /// 接受到武器分解信息 param1 = ScEquipWeaponDecompose
    /// </summary>
    public const string EventItemDecompose = "EventSignItemDecompose";

    public const string EventWelfareSignData = "EventWelfareSignData";
    public const string EventWelfareSign = "EventWelfareSign";
    public const string EventWelfareOpen = "EventWelfareOpen";
    public const string EventWelfareClose = "EventWelfareClose";
    public const string EventWelfareCanGet = "EventWelfareCanGet";
    public const string EventWelfareGetSucced = "EventWelfareGetSucced";
    public const string EventWelfareMoneyChange = "EventWelfareMoneyChange";
    public const string EventWelfareAllInfo = "EventWelfareAllInfo";
    public const string EventWelfareSpecificInfo = "EventWelfareSpecificInfo";
    /// <summary>
    /// 活动剩余时间
    /// </summary>
    public const string EventRemainTimeRresh = "EventRemainTimeRresh";
    /// <summary>
    /// 领奖剩余时间刷新
    /// </summary>
    public const string EventWaiteTimeRresh = "EventWaiteTimeRresh";

    /// <summary>
    /// 新手拼图活动
    /// </summary>
    public List<PWeflareInfo> puzzleList { get { return m_puzzleList; } }
    /// <summary>
    /// 所有日常活动
    /// </summary>
    public List<PWeflareInfo> allWeflarInfo { get { return m_allWeflarInfo; } }
    /// <summary>
    /// 所有充值活动
    /// </summary>
    public List<PWeflareInfo> allChargeInfo { get { return m_allChargeInfo; } }
    /// <summary>
    /// 未点击过活动
    /// </summary>
    public List<int> notClick { get { return m_notClick; } }

    public PWeflareInfo chooseInfo { get; set; }

    float m_rTime = 0;
    float m_waiteTime = 0;
    bool m_check = true;
    float m_chatWaite = 0;
    float m_friendTeam = 0;
    List<PWeflareInfo> m_puzzleList = new List<PWeflareInfo>();
    List<string> m_canGet = new List<string>();
    List<PWeflareInfo> m_allWeflarInfo = new List<PWeflareInfo>();
    List<PWeflareInfo> m_allChargeInfo = new List<PWeflareInfo>();
    List<int> m_notClick = new List<int>();
    PWeflareInfo m_onlineInfo { get { return m_allWeflarInfo.Find(a => (WelfareType)a.type == WelfareType.WaitTime); } }

    #region 签到

    #region List or Data

    public List<SignStateInfo> SetInfo { get { return setInfo; } }
    private List<SignStateInfo> setInfo = new List<SignStateInfo>();
    private ScRoleSignInfo sign_info;

    public int already { get; set; }

    public bool isget { get; set; }

    public bool isfirst { get; set; }
    public PWeflareInfo m_signWefare { get; set; }
    #endregion

    public void SendSign()
    {
        var p = PacketObject.Create<CsRoleSign>();
        session.Send(p);
    }

    public void GetAllSign()
    {
        var p = PacketObject.Create<CsRoleSignInfo>();
        session.Send(p);
    }

    void _Packet(ScRoleSign p)
    {
        if (p.result == 0)
        {
            isget = true;//已经签到过了
            sign_info.state = 1;
            SetInfo[already].state = 1;

            DispatchModuleEvent(EventWelfareSign);
        }
        else if (p.result == 1) Logger.LogError(" sign is error");
    }

    void _Packet(ScRoleSignInfo p)//接收到刷新是否签到 
    {
        p.CopyTo(ref sign_info);//存进本地
        already = sign_info.signed_;
        GetList(sign_info);

        if (p.state == 0) isget = false;
        else if (p.state == 1)
        {
            isget = true;
            ShowHint();
        }
        DispatchModuleEvent(EventWelfareSignData);
    }

    void _Packet(ScEquipWeaponDecompose p)
    {
        DispatchModuleEvent(EventItemDecompose, p);
    }

    private void GetList(ScRoleSignInfo AllInfo)
    {
        SetInfo.Clear();
        for (int i = 0; i < AllInfo.signReward.Length; i++)
        {
            SignStateInfo info = new SignStateInfo();

            info.wupin = AllInfo.signReward[i].reward;

            if (AllInfo.state == 0 && i == AllInfo.signed_) info.state = 2;//该签到
            else if (i < AllInfo.signed_) info.state = 1;
            else if (i > AllInfo.signed_) info.state = 0;
            SetInfo.Add(info);
        }
    }

    #endregion

    #region 充值活动

    public void SendGetAllInfo()
    {
        CsWeflareInfo p = PacketObject.Create<CsWeflareInfo>();
        session.Send(p);
    }
    void _Packet(ScWeflareInfo p)
    {
        GetAllWelfareInfo(p.Clone().info);
        if (m_allWeflarInfo.Count <= 0) return;
        DispatchModuleEvent(EventWelfareAllInfo);
    }

    private void GetAllWelfareInfo(PWeflareInfo[] info)
    {
        m_allWeflarInfo.Clear();
        m_puzzleList.Clear();
        m_allChargeInfo.Clear();
        for (int i = 0; i < info.Length; i++)
        {
            if (info[i] == null) continue;
            if (info[i].type == (int)WelfareType.ActiveNewPuzzle) m_puzzleList.Add(info[i]);
            else if (info[i].type > 50) m_allChargeInfo.Add(info[i]);
            else
            {
                var have = m_allWeflarInfo.Find(a => a.id == info[i].id);
                if (have != null) m_allWeflarInfo.Remove(have);
                m_allWeflarInfo.Add(info[i]);
            }
        }
        m_allChargeInfo.Sort((a, b) => a.priority.CompareTo(b.priority));
        m_puzzleList.Sort((a, b) => a.priority.CompareTo(b.priority));
        if (m_puzzleList.Count > 0) m_allWeflarInfo.Add(m_puzzleList[0]);
        m_allWeflarInfo.Sort((a, b) => a.priority.CompareTo(b.priority));
        CheckLocalClick();
        SetRewardSate();
        ShowHint();
    }

    void _Packet(ScWeflareSpecificInfo p)
    {
        PWeflareInfo info = p.thisinfo.Clone();
        SetSpecificInfo(info, m_allWeflarInfo);
        SetSpecificInfo(info, m_puzzleList);
        SetSpecificInfo(info, m_allChargeInfo);
        DispatchModuleEvent(EventWelfareSpecificInfo, info);
    }

    private void SetSpecificInfo(PWeflareInfo info, List<PWeflareInfo> welfareList)
    {
        if (info == null) return;
        for (int i = 0; i < welfareList.Count; i++)
        {
            if (welfareList[i] == null) continue;
            if (info.id == welfareList[i].id)
            {
                welfareList[i] = info;
                for (int j = 0; j < welfareList[i].rewardid.Length; j++)
                {
                    SetCanGet(welfareList[i], j);
                    if (welfareList[i].rewardid[j]?.state == 1)
                        moduleHome.UpdateIconState(HomeIcons.Welfare, true);
                }
                return;
            }
        }
    }

    private void SetRewardSate()
    {
        m_canGet.Clear();
        SetWelfareState(m_allWeflarInfo);
        SetWelfareState(m_puzzleList);
    }

    private void SetWelfareState(List<PWeflareInfo> stateList)
    {
        for (int i = 0; i < stateList.Count; i++)
        {
            if (stateList[i] == null) continue;
            if ((WelfareType)stateList[i].type == WelfareType.Sign) m_signWefare = stateList[i];
            else
            {
                for (int j = 0; j < stateList[i].rewardid.Length; j++)
                {
                    SetCanGet(stateList[i], j);
                }
            }
        }
    }

    void _Packet(ScWeflareRewardStateChange p)
    {
        SetRewardState(p.id, p.index, p.state, m_allWeflarInfo);
        SetRewardState(p.id, p.index, p.state, m_puzzleList);
        var info = m_allWeflarInfo.Find(a => a.id == p.id);
        if (info == null) info = m_puzzleList.Find(a => a.id == p.id);
        if (info == null) return;
        ShowHint();
        DispatchModuleEvent(EventWelfareCanGet, info);
    }
    
    private void SetRewardState(int welfareId, int wIndex, int state, List<PWeflareInfo> weflareList)
    {
        for (int i = 0; i < weflareList.Count; i++)
        {
            if (weflareList[i] == null) continue;
            if (welfareId == weflareList[i].id)
            {
                if ((WelfareType)weflareList[i].type == WelfareType.Sign) continue;
                for (int j = 0; j < weflareList[i].rewardid.Length; j++)
                {
                    if (weflareList[i].rewardid[j].index == wIndex)
                    {
                        weflareList[i].rewardid[j].state = (sbyte)state;
                        SetCanGet(weflareList[i], j);
                    }
                }
            }
        }
    }
    //购买 购买成功发送领取

    public void SendBuyReward(int index)
    {
        CsWeflareBuy p = PacketObject.Create<CsWeflareBuy>();
        p.id = chooseInfo.id;
        p.index = index;
        session.Send(p);
        moduleGlobal.LockUI(0, 0.5f);
    }

    void _Packet(ScWeflareBuy p)
    {
        if (p.result == 0) SendGetReward(p.index, p.id);
        else if (p.result == 1) moduleGlobal.ShowMessage(211, 27);
        else if (p.result == 2) moduleGlobal.ShowMessage(211, 28);
    }

    //申请获得奖励
    public void SendGetReward(int index, int welfareId = -1)
    {
        if (welfareId == -1) welfareId = chooseInfo.id;
        CsWeflareGet p = PacketObject.Create<CsWeflareGet>();
        p.id = welfareId;
        p.index = index;
        session.Send(p);
        moduleGlobal.LockUI(0, 0.5f);
    }

    //获取结果
    void _Packet(ScWeflareGet p)
    {
        moduleGlobal.UnLockUI();
        if (p.result == 0)
        {
            SetWeflareGet(p.id, p.index, m_allWeflarInfo);
            SetWeflareGet(p.id, p.index, m_puzzleList);
            var info = m_allWeflarInfo.Find(a => a.id == p.id);
            if (info == null) info = m_puzzleList.Find(a => a.id == p.id);
            DispatchModuleEvent(EventWelfareGetSucced, info, p.index);
        }
        else moduleGlobal.ShowMessage(211, 17);
    }

    private void SetWeflareGet(int wId, int index, List<PWeflareInfo> welfareList)
    {
        for (int i = 0; i < welfareList.Count; i++)
        {
            if (welfareList[i] == null) continue;
            if (wId == welfareList[i].id)
            {
                if (welfareList[i] == null) continue;
                PWeflareInfo info = welfareList[i];
                if ((WelfareType)info.type == WelfareType.Sign) continue;

                string strId = string.Empty;
                for (int j = 0; j < info.rewardid.Length; j++)
                {
                    if (info.rewardid[j] == null) continue;
                    strId = wId.ToString() + info.rewardid[j].index.ToString();
                    if ((WelfareType)info.type == WelfareType.SpecifiedStreng)// 只有该类型index为倍数
                    {
                        info.rewardid[j].state = 0;
                        if (m_canGet.Contains(strId)) m_canGet.Remove(strId);
                    }
                    else if (index == info.rewardid[j].index)
                    {
                        info.rewardid[j].state = 2;
                        if (m_canGet.Contains(strId)) m_canGet.Remove(strId);
                    }
                }

                welfareList[i] = info;

                var remove = true;
                if (info.closetime == 0 || info.opentime == 0)
                {
                    if (info.type == (int)WelfareType.ActiveNewPuzzle) remove = GetPuzzleInfoState();
                    else
                    {
                        for (int j = 0; j < info.rewardid.Length; j++)
                        {
                            if (info.rewardid[j].state != 2) remove = false;
                        }
                    }
                    if (remove)
                    {
                        welfareList.Remove(info);
                        RemoveState(wId, info);
                    }
                }
                return;
            }
        }
    }

    private bool GetPuzzleInfoState()
    {
        bool remove = true;
        for (int i = 0; i < m_puzzleList.Count; i++)
        {
            if (m_puzzleList[i] == null || m_puzzleList[i]?.rewardid == null) continue;
            for (int j = 0; j < m_puzzleList[i].rewardid.Length; j++)
            {
                if (m_puzzleList[i].rewardid[j] == null) continue;
                if (m_puzzleList[i].rewardid[j].state != 2) remove = false;
            }
        }
        return remove;
    }

    //任务开启
    void _Packet(ScWeflareOpen p)
    {
        List<PWeflareInfo> wInfo = new List<PWeflareInfo>();
        wInfo.AddRange( p.Clone().openinfo);
        for (int i = 0; i < wInfo.Count; i++)
        {
            if (wInfo[i] == null) continue;
            PWeflareInfo info = m_allWeflarInfo.Find(a => a.id == wInfo[i].id);
            if (info != null) m_allWeflarInfo.Remove(info);
            PWeflareInfo puzzle = m_puzzleList.Find(a => a.id == wInfo[i].id);
            if (puzzle != null) m_puzzleList.Remove(puzzle);
            PWeflareInfo charge = m_allChargeInfo.Find(a => a.id == wInfo[i].id);
            if (charge != null) m_allChargeInfo.Remove(charge);

            if (wInfo[i].type == (int)WelfareType.ActiveNewPuzzle) m_puzzleList.Add(wInfo[i]);
            else if (wInfo[i].type > 50) m_allChargeInfo.Add(wInfo[i]);
            else m_allWeflarInfo.Add(wInfo[i]);

            var click = m_notClick.Find(a => a == wInfo[i].id);
            if (click > 0) m_notClick.Remove(click);
            if (wInfo[i].hinttype != 0) m_notClick.Add(wInfo[i].id);

            for (int j = 0; j < wInfo[i].rewardid.Length; j++)
            {
                SetCanGet(wInfo[i], j);
                if (wInfo[i].rewardid[j]?.state == 1) moduleHome.UpdateIconState(HomeIcons.Welfare, true);
            }
        }

        m_allChargeInfo.Sort((a, b) => a.priority.CompareTo(b.priority));
        m_puzzleList.Sort((a, b) => a.priority.CompareTo(b.priority));
        var have = m_allWeflarInfo.Find(a => a.type == (int)WelfareType.ActiveNewPuzzle);
        if (have != null) m_allWeflarInfo.Remove(have);
        if (m_puzzleList.Count > 0) m_allWeflarInfo.Add(m_puzzleList[0]);
        m_allWeflarInfo.Sort((a, b) => a.priority.CompareTo(b.priority));
        SetRewardSate();
        DispatchModuleEvent(EventWelfareOpen, wInfo);
    }

    //任务关闭
    void _Packet(ScWeflareClose p)
    {
        for (int i = 0; i < p.closeid.Length; i++)
        {
            PWeflareInfo info = m_allWeflarInfo.Find(a => a.id == p.closeid[i]);
            if (info != null)
            {
                m_allWeflarInfo.Remove(info);
                RemoveState(p.closeid[i], info);
            }
            PWeflareInfo puzzle = m_puzzleList.Find(a => a.id == p.closeid[i]);
            if (puzzle != null)
            {
                m_puzzleList.Remove(puzzle);
                RemoveState(p.closeid[i], puzzle);
            }
            PWeflareInfo charge = m_allChargeInfo.Find(a => a.id == p.closeid[i]);
            if (charge != null)
            {
                m_puzzleList.Remove(charge);
                RemoveState(p.closeid[i], charge);
            }
            if (m_dropUps.ContainsKey(p.closeid[i])) m_dropUps.Remove(p.closeid[i]);
        }

        var have = m_allWeflarInfo.Find(a => a.type == (int)WelfareType.ActiveNewPuzzle);
        if (have == null && m_puzzleList.Count > 0)
        {
            m_allWeflarInfo.Add(m_puzzleList[0]);
            m_allWeflarInfo.Sort((a, b) => a.priority.CompareTo(b.priority));
        }

        DispatchModuleEvent(EventWelfareClose);
        if (m_notClick.Count <= 0 && isget && m_canGet.Count <= 0) moduleHome.UpdateIconState(HomeIcons.Welfare, false);
    }

    private void RemoveState(int closeId, PWeflareInfo info)
    {
        bool not = m_notClick.Exists(a => a == closeId);
        if (not) m_notClick.Remove(closeId);

        if ((WelfareType)info.type == WelfareType.Sign) return;

        for (int i = 0; i < info.rewardid.Length; i++)
        {
            SetCanGet(info, i, false);
        }
    }

    //进度值有改变
    void _Packet(ScWeflareMoney p)
    {
        PWeflareInfo info = null;

        bool have = m_puzzleList.Exists(a => a.id == p.id);
        if (have) SetWelfarweChange(info, m_puzzleList, p);
        else SetWelfarweChange(info, m_allWeflarInfo, p);

        var puzzle = m_puzzleList.Find(a => a.id == p.id);
        var other = m_allWeflarInfo.Find(a => a.id == p.id);
        if (other != null && puzzle != null)
        {
            m_allWeflarInfo.Remove(other);
            m_allWeflarInfo.Add(puzzle);
            m_allWeflarInfo.Sort((a, b) => a.priority.CompareTo(b.priority));
        }

        if (chooseInfo == null || info == null) return;
        DispatchModuleEvent(EventWelfareMoneyChange, info);

    }

    private void SetWelfarweChange(PWeflareInfo info, List<PWeflareInfo> list, ScWeflareMoney p)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null) continue;
            if (list[i].id == p.id)
            {
                SetProgress(list[i], p.type, p.money, p.taskID);
                info = list[i];
                break;
            }
        }
    }

    private void SetCanGet(PWeflareInfo info, int index, bool add = true)
    {
        if (info == null || index >= info.rewardid.Length) return;
        if (info.rewardid[index] == null) return;

        string strId = info.id.ToString() + info.rewardid[index].index.ToString();
        if (m_canGet.Contains(strId)) m_canGet.Remove(strId);
        if (info.rewardid[index].state == 1 && add) m_canGet.Add(strId);
    }

    #endregion

    #region 查找对应的进度

    public bool GetTypeState(int type)
    {
        var click = false;

        for (int i = 0; i < notClick.Count; i++)
        {
            if (type == 0)
            {
                click = m_allWeflarInfo.Exists(a => a.id == notClick[i]);
                if (!click) m_puzzleList.Exists(a => a.id == notClick[i]);
                if (click) break;
            }
            else if (type == 1)
            {
                click = m_allChargeInfo.Exists(a => a.id == notClick[i]);
                if (click) break;
            }
        }
        if (type == 0)
        {
            var sign = m_allWeflarInfo.Exists(a => a.type == (int)WelfareType.Sign);
            if (sign) return !isget || click || m_canGet.Count > 0;
            else return click || m_canGet.Count > 0;
        }
        else if (type == 1) return click;
        return false;
    }

    public bool GeLableHint(PWeflareInfo info)
    {
        if (info == null) return false;

        if ((WelfareType)info.type >= WelfareType.ChargeDailySale)
        {
            for (int i = 0; i < m_allChargeInfo.Count; i++)
            {
                if (GetWelfareHint(m_allChargeInfo[i])) return true;
            }
        }

        if ((WelfareType)info.type == WelfareType.Sign) return !isget;

        if (info.type != (int)WelfareType.ActiveNewPuzzle) return GetWelfareHint(info);

        for (int i = 0; i < m_puzzleList.Count; i++)
        {
            if (GetWelfareHint(m_puzzleList[i])) return true;
        }
        return false;
    }

    public bool GetWelfareHint(PWeflareInfo info)
    {
        if (info == null) return false;
        var click = notClick.Exists(a => a == info.id);
        if (click) return true;
        for (int j = 0; j < info.rewardid.Length; j++)
        {
            if (info.rewardid[j].state == 1) return true;
        }
        return false;
    }

    public int GetCostMoney(PWeflareAward info)
    {
        for (int i = 0; i < info.reachnum.Length; i++)
        {
            if (info.reachnum == null) continue;
            if (info.reachnum[i].type == (int)WelfareReachType.CostIconThis)
                return info.reachnum[i].progress;
        }
        return 0;
    }

    public void SetProgress(PWeflareInfo Info, int type, int data, int taskId)
    {
        //玩家进度中的type 是 具体到要做什么事情 不可能相同

        AddWelfareData(Info, type, taskId);

        for (int i = 0; i < Info.roleData.Length; i++)
        {
            if (Info.roleData[i] == null) continue;
            if (Info.roleData[i].type == type)
            {
                if (taskId == 0) Info.roleData[i].progress += data;
                else
                {
                    for (int j = 0; j < Info.roleData[i].taskInfo.Length; j++)
                    {
                        var rInfo = Info.roleData[i].taskInfo;
                        if (rInfo == null) continue;
                        if (rInfo[j].taskId == taskId)
                        {
                            rInfo[j].times += data;
                        }
                    }
                }
            }
        }
    }

    public PWeflareData GetProgress(PWeflareInfo info, int type = -1)
    {
        if (info == null || info.roleData == null) return null;
        if (type == -1) type = GetWelfareType(info);
        for (int i = 0; i < info.roleData.Length; i++)
        {
            if (info.roleData[i] == null) continue;
            if (info.roleData[i].type == type)
            {
                return info.roleData[i];
            }
        }
        return null;
    }

    private int GetWelfareType(PWeflareInfo info, int index = 0)
    {
        int type = info.type;
        if (info.rewardid != null && info.rewardid?.Length > 0)
        {
            if (info.rewardid[index] != null && info.rewardid[index]?.reachnum.Length > 0)
            {
                if (info.rewardid[index].reachnum[0] != null) type = info.rewardid[index].reachnum[0].type;
            }
        }
        return type;
    }

    private void AddWelfareData(PWeflareInfo info, int type, int taskId = 0)
    {
        List<PWeflareData> list = new List<PWeflareData>();

        if (info.roleData == null) list.Add(CreateWeflareData(type, taskId));
        else
        {
            if (info.roleData.Length > 0) list.AddRange(info.roleData);
            var have = info.roleData.Find(a => a.type == type);
            if (have == null) list.Add(CreateWeflareData(type, taskId));
            else if (taskId != 0)
            {
                list.Remove(have);
                have.taskInfo = CreateChaseData(taskId, have.taskInfo);
                list.Add(have);
            }
        }
        info.roleData = list.ToArray();
    }

    private PWeflareData CreateWeflareData(int type, int taskId = 0, int progress = 0, int day = 0)
    {
        var data = PacketObject.Create<PWeflareData>();
        data.type = (sbyte)type;
        data.progress = progress;
        data.days = day;
        data.taskInfo = CreateChaseData(taskId);
        return data;
    }

    private PWeflareChase[] CreateChaseData(int taskId, PWeflareChase[] chase = null)
    {
        List<PWeflareChase> chaseList = new List<PWeflareChase>();
        if (chase != null) chaseList.AddRange(chase);

        if (taskId == 0) return chaseList.ToArray();

        var have = chaseList.Find(a => a.taskId == taskId);
        if (have != null) return chaseList.ToArray();

        var t = PacketObject.Create<PWeflareChase>();
        t.taskId = taskId;
        t.times = 0;
        chaseList.Add(t);

        return chaseList.ToArray();
    }

    public int ShowTxt(PWeflareData data, int type, int taskId = 0)
    {
        if (data == null) return 0;
        if (type == 1) return data.days;
        else if (type == 2) return data.progress;
        else if (type == 3)
        {
            for (int i = 0; i < data.taskInfo.Length; i++)
            {
                if (data.taskInfo == null) continue;
                if (data.taskInfo[i].taskId == taskId) return data.taskInfo[i].times;
            }
        }
        return 0;
    }
    /// <summary>
    /// 获取对应达成条件的玩家进度 
    /// </summary>
    /// <param name="data">玩家达成数据</param>
    /// <param name="taskId">通关id</param>
    /// <returns></returns>
    public int GetValueByReachType(PWeflareData data,int taskId)
    {
        switch ((WelfareReachType)data.type)
        {
            //达成条件取 day
            case WelfareReachType.StageOver:
            case WelfareReachType.PVPTimes:
            case WelfareReachType.StageType:
            case WelfareReachType.PVPWinTimes:
                return ShowTxt(data, 3, taskId);
            //达成条件取 progress
            case WelfareReachType.StrengEquip:
            case WelfareReachType.AdvanceEquip:
            case WelfareReachType.RuneLevel:
            case WelfareReachType.RuneStar:
            case WelfareReachType.AddFriend:
            case WelfareReachType.PetLevel:
            case WelfareReachType.GrowthEquip:
            case WelfareReachType.GrowthLevel:
            case WelfareReachType.PetTask:
            case WelfareReachType.StageTeam:
            case WelfareReachType.CooperateTask:
            case WelfareReachType.SoulEquip:
            case WelfareReachType.LevelEquip:
            case WelfareReachType.AwakeSkill:
                return ShowTxt(data, 2);
        }
        return 0;
    }
    public bool ShowPuzzleProgress(int type)
    {
        switch ((WelfareReachType)type)
        {
            //达成条件取 day
            case WelfareReachType.StageOver:
            case WelfareReachType.PVPTimes:
            case WelfareReachType.StageType:
            case WelfareReachType.PVPWinTimes:
            case WelfareReachType.StrengEquip:
            case WelfareReachType.AdvanceEquip:
            case WelfareReachType.RuneLevel:
            case WelfareReachType.RuneStar:
            case WelfareReachType.AddFriend:
            case WelfareReachType.PetLevel:
            case WelfareReachType.GrowthEquip:
            case WelfareReachType.GrowthLevel:
            case WelfareReachType.PetTask:
            case WelfareReachType.StageTeam:
            case WelfareReachType.CooperateTask:
            case WelfareReachType.SoulEquip:
            case WelfareReachType.LevelEquip:
            case WelfareReachType.AwakeSkill:return true;
        }
        return false;
    }
    #endregion

    #region 红点显示 今日是否第一次登录  优先级排序

    //检查本地点击状况
    private void CheckLocalClick()
    {
        m_notClick.Clear();
        SetClickInfo(m_allWeflarInfo);
        SetClickInfo(m_puzzleList);
        SetClickInfo(m_allChargeInfo);
    }

    private void SetClickInfo(List<PWeflareInfo> welfList)
    {
        for (int i = 0; i < welfList.Count; i++)
        {
            if (welfList[i] == null || welfList[i]?.hinttype == 0) continue;
            var have = m_notClick.Exists(a => a == welfList[i].id);
            if (have) continue;

            string ids = "welf" + welfList[i].id.ToString() + modulePlayer.roleInfo.roleId.ToString();
            string tt = PlayerPrefs.GetString(ids);
            if (string.IsNullOrEmpty(tt)) m_notClick.Add(welfList[i].id);
            else
            {
                if (welfList[i].hinttype == 2 && welfList[i].closetime == 0 && welfList[i].opentime == 0) continue;

                DateTime dtnow = DateTime.Parse(tt);
                DateTime dtend = Util.GetDateTime(welfList[i].closehint);
                DateTime dtopen = Util.GetDateTime(welfList[i].openhint);
                if (welfList[i].hinttype == 2 && welfList[i].closehint == 0 && welfList[i].openhint == 0)
                {
                    dtend = Util.GetDateTime(welfList[i].closetime);
                    dtopen = Util.GetDateTime(welfList[i].opentime);
                }
                if (DateTime.Compare(dtnow, dtopen) < 0 || DateTime.Compare(dtnow, dtend) > 0)
                {
                    m_notClick.Add(welfList[i].id);
                }
            }
        }
    }

    public void ShowHint()//红点的显示判断
    {
        var hint = false;
        var sign = m_allWeflarInfo.Exists(a => a.type == (int)WelfareType.Sign);
        if (!sign)
        {
            if (m_notClick.Count > 0 || m_canGet.Count > 0) hint = true;
        }
        else if (m_notClick.Count > 0 || isget == false || m_canGet.Count > 0) hint = true;

        moduleHome.UpdateIconState(HomeIcons.Welfare, hint);
    }

    public void IsFristEnter()
    {
        string keyStr = "sign" + modulePlayer.id_.ToString();

        isfirst = FrirstOpen(keyStr);

        //如果收到消息的时候已经打开了签到界面，则不再强行跳转签到界面
        Window_Welfare w = Window.GetOpenedWindow<Window_Welfare>();
        if (isfirst && !moduleGuide.inGuideProcess && moduleGuide.IsActiveFunction(HomeIcons.Welfare) && !(w && w.actived))
            moduleHome.SetWindowPanelAndIcon(Window_Home.Main, HomeIcons.Welfare);
    }

    public bool FrirstOpen(string key, bool set = true)
    {
        string date_this = PlayerPrefs.GetString(key);
        bool firstEnter = false;
        DateTime date_now = Util.GetServerLocalTime();
        DateTime check = Util.GetServerLocalTime();
        if (DateTime.TryParse(date_this, out check))
        {
            DateTime SignTime = Convert.ToDateTime(date_this);
            string month = "/" + SignTime.Month.ToString();
            if (SignTime.Month < 10)
            {
                month = "/" + "0" + SignTime.Month.ToString();
            }
            string days = "/" + SignTime.Day.ToString();
            if (SignTime.Day < 10)
            {
                days = "/" + "0" + SignTime.Day.ToString();
            }
            string refresh = SignTime.Year.ToString() + month + days + " " + "05:" + "00:" + "00";
            DateTime refreshtime = DateTime.ParseExact(refresh, "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            
            DateTime nexttime = refreshtime;
            if (SignTime.Hour >= 5)
            {
                nexttime = refreshtime.AddDays(1);
            }
            TimeSpan remain = nexttime - date_now;
            float alltime = remain.Hours * 3600 + remain.Minutes * 60 + remain.Seconds;

            if (alltime <= 0)//说明现在大于刷新的时间 
            {
                firstEnter = true;
                if (set) PlayerPrefs.SetString(key, date_now.ToString());
            }
        }
        else
        {
            firstEnter = true;
            if (set) PlayerPrefs.SetString(key, date_now.ToString());
        }
        return firstEnter;
    }

    #endregion
    
    public List<PItem2> CheckProto(PItem2[] Info)
    {
        List<PItem2> m_firstReward = new List<PItem2>();
        for (int i = 0; i < Info.Length; i++)
        {
            PropItemInfo prop = ConfigManager.Get<PropItemInfo>(Info[i].itemTypeId);
            if (!prop || !prop.IsValidVocation(modulePlayer.proto)) continue;
            m_firstReward.Add(Info[i]);
        }
        return m_firstReward;
    }



    public override void OnRootUpdate(float diff)
    {
        base.OnRootUpdate(diff);
        //好友私聊 组队图标的显示
        if (moduleFriend.TimeHintShow)
        {
            m_friendTeam += Time.unscaledDeltaTime;
            if (m_friendTeam >= 1f && moduleFriend.TeamHintCD > 0)
            {
                m_friendTeam = 0;
                moduleFriend.TeamHintCD--;
                moduleHome.UpdateIconState(HomeIcons.Friend, true, 1);
                if (moduleFriend.TeamHintCD <= 0)//组队邀请图标消失
                {
                    moduleFriend.TimeHintShow = false;
                    moduleHome.UpdateIconState(HomeIcons.Friend, false, 1);
                }
            }
        }

        //世界聊天限制
        if (!moduleChat.CanChatWord)
        {
            m_chatWaite += Time.unscaledDeltaTime;
            if (m_chatWaite >= 1f)
            {
                m_chatWaite = 0;
                moduleChat.ChatCD--;
                if (moduleChat.ChatCD <= 0)
                {
                    if (moduleGlobal.system != null)
                    {
                        moduleChat.ChatCD = moduleGlobal.system.worldCD;
                        moduleChat.CanChatWord = true;
                    }
                }
            }
        }

        //活动相关
        if (m_allWeflarInfo.Count > 0)
        {
            m_rTime += Time.unscaledDeltaTime;
            if (m_rTime >= 60f)
            {
                m_rTime = 0;
                DispatchModuleEvent(EventRemainTimeRresh);
            }
        }

        if (m_onlineInfo != null && m_check)
        {
            m_waiteTime += Time.unscaledDeltaTime;
            if (m_waiteTime >= 1f)
            {
                m_waiteTime = 0;
                m_check = false;
                for (int i = 0; i < m_onlineInfo.rewardid.Length; i++)
                {
                    if (m_onlineInfo.rewardid[i] == null) continue;
                    if (m_onlineInfo.rewardid[i].time < -1) continue;
                    m_onlineInfo.rewardid[i].time--;
                    if (m_onlineInfo.rewardid[i].time >= 0) m_check = true;
                }
                if (m_check && chooseInfo != null && chooseInfo?.type == (int)WelfareType.WaitTime)
                    DispatchModuleEvent(EventWaiteTimeRresh);
            }
        }
    }

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        m_check = true;
        m_allWeflarInfo.Clear();
        m_notClick.Clear();
        m_canGet.Clear();
        chooseInfo = null;
        GetAllSign();
        SendGetAllInfo();
    }

    #region 王一帆:掉率提升活动

    private Dictionary<int, List<PDroprateup>> m_dropUps = new Dictionary<int, List<PDroprateup>>();

    void _Packet(ScWeflareDroprateupInfo p)
    {
        if (p == null || p.dropups == null || p.dropups.Length < 1) return;

        m_dropUps.Clear();
        PDroprateupInfo info = null;
        for (int i = 0; i < p.dropups.Length; i++)
        {
            p.dropups[i].CopyTo(ref info);
            if (info == null) continue;
            if (!m_dropUps.ContainsKey(info.weflareId)) m_dropUps.Add(info.weflareId, new List<PDroprateup>());
            m_dropUps[info.weflareId].AddRange(info.drops);
        }
    }

    public PDroprateup GetCurDropUp(ushort _taskId)
    {
        var task = moduleChase.allTasks.Find(p => p.ID == _taskId);
        if (task == null || m_dropUps.Count < 1) return null;

        foreach (var item in m_dropUps)
        {
            var list = item.Value;
            var info = list.Find(p =>
            {
                if (p.type == 0) return false;

                if (p.subType == 1) return p.ids.Contains((ushort)task.level);
                else return p.ids.Contains((ushort)task.ID);
            });

            return info;
        }
        return null;
    }

    #endregion

    protected override void OnGameDataReset()
    {
        m_dropUps.Clear();
    }
}

public class SignStateInfo//构建类
{
    //0 未签到 1 已签到 2 该签到
    public int state;
    public PItem2 wupin;
}
