/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-01-17
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class Module_Active : Module<Module_Active>
{
    #region Event
    public const string EventActiveDayInfo = "EventActiveDayInfo";
    public const string EventActiveDayReach = "EventActiveDayReach";
    public const string EventActiveDayGet = "EventActiveDayGet";
    public const string EventActiveDayValue = "EventActiveDayValue";
    public const string EventActiveDayOpen = "EventActiveDayOpen";
    public const string EventActiveDayClose = "EventActiveDayClose";

    public const string EventActiveDailyCanGet = "EventActiveDailyCanGet";
    public const string EventActiveDailyGet = "EventActiveDailyGet";
    public const string EventActiveWeekCanGet = "EventActiveWeekCanGet";
    public const string EventActiveWeekGet = "EventActiveWeekGet";

    public const string EventActiveAchieveInfo = "EventActiveAchieveInfo";
    public const string EventActiveAchieveCanGet = "EventActiveAchieveCanGet";
    public const string EventActiveAchieveGet = "EventActiveAchieveGet";
    public const string EventActiveAchieveOpen = "EventActiveAchieveOpen";
    public const string EventActiveAchieveValue = "EventActiveAchieveValue";

    public const string EventActiveCoopInvate = "EventActiveCoopInvate";
    public const string EventActiveCoopInfo = "EventActiveCoopInfo";
    public const string EventActiveCoopInvateSuc = "EventActiveCoopInvateSuc";
    public const string EventActiveCoopKiced = "EventActiveCoopKiced";
    public const string EventActiveCoopBeKiced = "EventActiveCoopBeKiced";
    public const string EventActiveCoopValue = "EventActiveCoopValue";
    public const string EventActiveCoopCanGet = "EventActiveCoopCanGet";
    public const string EventActiveCoopGet = "EventActiveCoopGet";
    public const string EventActiveCoopApply = "EventActiveCoopApply";
    #endregion

    #region List

    public int ActiveClick = 0;
    public bool ShowDialyHint;
    public bool ShowAchieveHint;
    public bool ShowCoopHint;

    /// <summary>
    /// 日常任务
    /// </summary>
    public List<PDailyInfo> DailyList = new List<PDailyInfo>();
    /// <summary>
    /// 开启的日常任务
    /// </summary>
    public List<PDailyInfo> DailyOpenList = new List<PDailyInfo>();
    /// <summary>
    /// 日常任务配置
    /// </summary>
    public List<PDailyTask> DailyListTask = new List<PDailyTask>();
    /// <summary>
    /// 每日活跃度宝箱
    /// </summary>
    public List<PActiveBox> ActiveValue = new List<PActiveBox>();
    /// <summary>
    /// 每周活跃度宝箱
    /// </summary>
    public List<PActiveBox> WeekActiveValue = new List<PActiveBox>();
    /// <summary>
    /// 活跃度宝箱状态
    /// </summary>
    public Dictionary<int, EnumActiveState> Activestae = new Dictionary<int, EnumActiveState>();
    /// <summary>
    /// 成就列表
    /// </summary>
    public List<PAchieve> Achieveinfo = new List<PAchieve>();
    public Dictionary<int, bool> CanGetList = new Dictionary<int, bool>();
    public Dictionary<int, float> LossTime = new Dictionary<int, float>();

    /// <summary>
    /// 得到协作任务时候的时间
    /// </summary>
    public float GetCoopTaskTime;
    /// <summary>
    /// 当前选中的task uid
    /// </summary>
    public ulong CheckTaskID { get; set; }
    public int CoopTaskTime;
    /// <summary>
    /// 协作任务
    /// </summary>
    public List<PCooperateInfo> CoopTaskList = new List<PCooperateInfo>();
    /// <summary>
    /// 协作任务配置
    /// </summary>
    public List<CooperationTask> coopTaskBase = new List<CooperationTask>();
    public List<PPlayerInfo> CoopInvateList = new List<PPlayerInfo>();

    public List<ulong> m_coopCheckList = new List<ulong>();
    public Dictionary<ulong, int> coopTaskState = new Dictionary<ulong, int>();

    #endregion

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        ActiveClick = 0;
        ShowDialyHint = false;
        ShowAchieveHint = false;
        ShowCoopHint = false;
        DailyList.Clear();
        DailyOpenList.Clear();
        DailyListTask.Clear();
        ActiveValue.Clear();
        Activestae.Clear();
        WeekActiveValue.Clear();
        LossTime.Clear();
        Achieveinfo.Clear();
        CanGetList.Clear();
        coopTaskBase.Clear();
        InitCoop();
        GetAllDailyTask();
        GetallActiveValue();
        GetallAchieveTask();
        GetAllCooperation();
    }

    public string CutString(string str, int len)
    {
        if (len == -1) return str;
        int tempLen = 0;
        string tempString = "";
        System.Text.ASCIIEncoding ascii = new System.Text.ASCIIEncoding();
        byte[] s = ascii.GetBytes(str);
        for (int i = 0; i < s.Length; i++)
        {
            if ((int)s[i] == 63) tempLen += 2;
            else tempLen += 1;

            if (i < str.Length) tempString += str.Substring(i, 1);

            if (tempLen >= len) break;
        }
        byte[] mybyte = System.Text.Encoding.Default.GetBytes(str);
        if (mybyte.Length > len) tempString += "…";

        return tempString;
    }

    public uint AlreadyLossTime(float thistime)
    {
        return (uint)(Time.realtimeSinceStartup - thistime);
    }

    #region 日常任务
    public void GetAllDailyTask()
    {
        //向服务器发送获取所有日常任务
        CsDailyTasks p = PacketObject.Create<CsDailyTasks>();
        session.Send(p);
    }

    void _Packet(ScDailyTasks p)
    {
        ScDailyTasks pp = p.Clone();
        DailyListTask.Clear();
        DailyList.Clear();
        GetAllDailyTaskSate();
        for (int i = 0; i < p.tasks.Length; i++)
        {
            bool have = DailyListTask.Exists(a => a.id == p.tasks[i].id);
            if (!have)
            {
                DailyListTask.Add(pp.tasks[i]);
                PDailyInfo info = PacketObject.Create<PDailyInfo>();
                info.taskId = p.tasks[i].id;
                info.state = 0;
                info.finishVal = 0;
                info.restTime = 0;
                info.isOpen = false;
                bool dailyHave = DailyList.Exists(a => a.taskId == p.tasks[i].id);
                if (!dailyHave)
                {
                    DailyList.Add(info);
                }
            }
        }
    }


    public void GetAllDailyTaskSate()
    {
        //向服务器发送获取所有日常任务的状态
        CsDailyTaskInfo p = PacketObject.Create<CsDailyTaskInfo>();
        session.Send(p);
    }

    void _Packet(ScDailyTaskInfo p)
    {
        ScDailyTaskInfo stateinfp = p.Clone();
        int length = DailyList.Count;

        LossTime.Clear();
        for (int i = 0; i < stateinfp.infoList.Length; i++)
        {
            for (int j = 0; j < length; j++)
            {
                if (stateinfp.infoList[i].taskId == DailyList[j].taskId)
                {
                    if (!LossTime.ContainsKey(DailyList[j].taskId)) LossTime.Add(DailyList[j].taskId, Time.realtimeSinceStartup);//所有任务的时间流失
                    if (stateinfp.infoList[i].state >= 2)
                    {
                        stateinfp.infoList[i].isOpen = false;
                    }
                    DailyList[j].state = stateinfp.infoList[i].state;
                    DailyList[j].finishVal = stateinfp.infoList[i].finishVal;
                    DailyList[j].isOpen = stateinfp.infoList[i].isOpen;
                    DailyList[j].restTime = stateinfp.infoList[i].restTime;

                    PDailyInfo thisinfo;
                    thisinfo = DailyList[j];
                    if (DailyList[j].state == 1 && DailyList[j].isOpen)
                    {
                        DailyList.Remove(DailyList[j]);
                        DailyList.Insert(0, thisinfo);
                    }
                }
            }
        }
        moduleHome.UpdateIconState(HomeIcons.Quest, ShowHomestate(0));
        moduleHome.UpdateIconState(HomeIcons.Quest, ShowHomestate(1), 1);
        OpenTask();
        DispatchModuleEvent(EventActiveDayInfo);
    }

    void _Packet(ScDailyStateChange p)
    {
        ScDailyStateChange changeinfo = p.Clone();
        //状态改变日常任务达成  首先要把这个信息找出来 然后移除 然后添加到第一位 状态变为可领取 
        PDailyInfo thisinfo;
        for (int i = 0; i < DailyList.Count; i++)
        {
            if (DailyList[i].taskId == changeinfo.taskId)
            {
                thisinfo = DailyList[i];
                DailyList[i].state = changeinfo.newState;
                if (changeinfo.newState == 1)
                {
                    //可领取奖励
                    DailyList.Remove(thisinfo);
                    DailyList.Insert(0, thisinfo);
                    OpenTask();
                    DispatchModuleEvent(EventActiveDayReach, thisinfo);
                }
                //changeinfo.newState >= 2 领取成功)
            }
        }
        ShowDialyHint = true;
        moduleHome.UpdateIconState(HomeIcons.Quest, true);
        moduleHome.UpdateIconState(HomeIcons.Quest, ShowHomestate(1), 1);
    }

    void _Packet(ScDailyValChange p)
    {
        //进度值有改变
        for (int i = 0; i < DailyList.Count; i++)
        {
            if (DailyList[i].taskId == p.taskId)
            {
                DailyList[i].finishVal = p.newVal;
                OpenTask();
                DispatchModuleEvent(EventActiveDayValue, DailyList[i]);
            }
        }
    }

    public void SendGetDaward(int DawardID)
    {
        //发送 要领取日常任务奖励
        CsDailyTaskReward p = PacketObject.Create<CsDailyTaskReward>();
        p.taskId = (ushort)DawardID;
        session.Send(p);
        moduleGlobal.LockUI(0, 0.5f);
    }

    void _Packet(ScDailyTaskReward p)
    {
        moduleGlobal.UnLockUI();
        if (p.result == 0)
        {
            for (int i = 0; i < DailyList.Count; i++)
            {
                if (DailyList[i].taskId == p.taskId)
                {
                    DailyList[i].isOpen = false;
                }
            }
            moduleHome.UpdateIconState(HomeIcons.Quest, ShowHomestate(1), 1);

            PDailyInfo Dinfo = DailyOpenList.Find(a => a.taskId == p.taskId);
            PDailyTask Tinfo = DailyListTask.Find(a => a.id == p.taskId);
            OpenTask();

            DispatchModuleEvent(EventActiveDayGet, Dinfo, Tinfo); //ID是日常任务的（通过ID找到对应的gameobject进行删除 ）
        }
        else if (p.result == 1) moduleGlobal.ShowMessage(223, 21);
        else if (p.result == 2) moduleGlobal.ShowMessage(223, 22);
        else if (p.result == 3) moduleGlobal.ShowMessage(223, 23);
        else if (p.result == 4) moduleGlobal.ShowMessage(223, 24);
        else Logger.LogError("reward get error" + p.result);
    }

    void _Packet(ScDailyTaskOpen p)
    {
        ScDailyTaskOpen clonp = p.Clone();
        //某个任务开启  这个id对应的gameobject 要进行显示
        for (int j = 0; j < clonp.tasks.Length; j++)
        {
            for (int i = 0; i < DailyList.Count; i++)
            {
                if (DailyList[i].taskId == clonp.tasks[j].taskId)
                {
                    //从新计算开始时间
                    LossTime[DailyList[i].taskId] = Time.realtimeSinceStartup;

                    DailyList[i].state = clonp.tasks[j].state;
                    DailyList[i].finishVal = clonp.tasks[j].finishVal;
                    DailyList[i].restTime = clonp.tasks[j].restTime;
                    DailyList[i].isOpen = clonp.tasks[j].isOpen;
                    if (DailyList[i].isOpen) ShowDialyHint = true;
                }
            }
        }

        OpenTask();

        DispatchModuleEvent(EventActiveDayOpen);
        if (ShowDialyHint) moduleHome.UpdateIconState(HomeIcons.Quest, true);
        moduleHome.UpdateIconState(HomeIcons.Quest, ShowHomestate(1), 1);
    }

    void _Packet(ScDailyTaskClosed p)
    {
        ScDailyTaskClosed clonp = p.Clone();
        for (int i = 0; i < clonp.tasks.Length; i++)
        {
            for (int j = 0; j < DailyList.Count; j++)
            {
                if (DailyList[j].taskId == clonp.tasks[i].taskId)
                {
                    DailyList[j].state = clonp.tasks[i].state;
                    DailyList[j].finishVal = clonp.tasks[i].finishVal;
                    DailyList[j].restTime = clonp.tasks[i].restTime;
                    DailyList[j].isOpen = clonp.tasks[i].isOpen;
                    OpenTask();

                    DispatchModuleEvent(EventActiveDayClose, clonp.tasks[i]);
                }
            }
        }
        moduleHome.UpdateIconState(HomeIcons.Quest, ShowHomestate(0));
        moduleHome.UpdateIconState(HomeIcons.Quest, ShowHomestate(1), 1);
    }

    public void OpenTask()
    {
        DailyOpenList.Clear();
        for (int i = 0; i < DailyList.Count; i++)
        {
            if (DailyList[i].isOpen && DailyList[i].state < 2)
            {
                DailyOpenList.Add(moduleActive.DailyList[i]);
            }
        }

    }
    #endregion

    #region 活跃度
    public void GetallActiveValue()
    {
        //发送获得所有奖励的信息
        CsActiveBoxInfo p = PacketObject.Create<CsActiveBoxInfo>();
        session.Send(p);
    }
    void _Packet(ScActiveBoxInfo p)
    {
        ScActiveBoxInfo pp = p.Clone();
        Activestae.Clear();
        ActiveValue.Clear();
        WeekActiveValue.Clear();
        for (int i = 0; i < pp.boxes.Length; i++)
        {
            if (p.boxes[i].type == 1) ActiveValue.Add(pp.boxes[i]);//大于等于五就是周活跃度
            else WeekActiveValue.Add(pp.boxes[i]);
        }
        ActiveState();
    }
    private void ActiveState()
    {
        byte Dstate = modulePlayer.roleInfo.dapRewardState;
        byte Wstate = modulePlayer.roleInfo.wapRewardState;
        for (int i = 0; i < ActiveValue.Count; i++)
        {
            Jisuan(modulePlayer.roleInfo.dayActivePoint, ActiveValue[i], Dstate, i);
        }
        for (int i = 0; i < WeekActiveValue.Count; i++)
        {
            Jisuan(modulePlayer.roleInfo.weekActivePoint, WeekActiveValue[i], Wstate, i);
        }
    }

    private void Jisuan(int point, PActiveBox boxinfo, byte State, int i)
    {
        float statype = Mathf.Pow(2, i);
        if (point >= boxinfo.activePoint)
        {
            //可以领取
            if ((State & (byte)statype) > 0) Activestae.Add(boxinfo.id, EnumActiveState.AlreadPick);//已经领取
            else Activestae.Add(boxinfo.id, EnumActiveState.CanPick);//可领取状态未领取
        }
        else Activestae.Add(boxinfo.id, EnumActiveState.NotPick);//不可领取状态
    }
    void _Packet(ScRoleActivePoint p)
    {
        //日常活跃度更改
        modulePlayer.roleInfo.dayActivePoint = p.dailyPoint;
        modulePlayer.roleInfo.weekActivePoint = p.weekPoint;
        for (int i = 0; i < ActiveValue.Count; i++)
        {
            if (modulePlayer.roleInfo.dayActivePoint >= ActiveValue[i].activePoint)
            {
                if (Activestae[ActiveValue[i].id] == EnumActiveState.NotPick)//服务器发来的数据链表里（活跃度）状态是不可领取
                {
                    Activestae[ActiveValue[i].id] = EnumActiveState.CanPick;
                    ShowDialyHint = true;
                    //状态在这儿变为可领取
                    DispatchModuleEvent(EventActiveDailyCanGet, ActiveValue[i]);//i 是活跃度的 
                }
            }
        }
        for (int i = 0; i < WeekActiveValue.Count; i++)
        {
            if (modulePlayer.roleInfo.weekActivePoint >= WeekActiveValue[i].activePoint)
            {
                if (Activestae[WeekActiveValue[i].id] == EnumActiveState.NotPick)
                {
                    Activestae[WeekActiveValue[i].id] = EnumActiveState.CanPick;
                    ShowDialyHint = true;
                    DispatchModuleEvent(EventActiveWeekCanGet, WeekActiveValue[i]);
                }
            }
        }
        if (ShowDialyHint) moduleHome.UpdateIconState(HomeIcons.Quest, true);
    }

    public void SendGetActiveaward(int DawardID)
    {
        //发送 要领取活跃度奖励
        CsDrawActiveReward p = PacketObject.Create<CsDrawActiveReward>();
        p.boxId = (byte)DawardID;
        session.Send(p);

    }
    void _Packet(ScDrawActiveReward p)
    {
        if (p.result == 0)
        {
            int trueid = p.boxId;
            Activestae[trueid] = EnumActiveState.AlreadPick;
            PActiveBox Dactive = ActiveValue.Find(a => a.id == trueid);
            PActiveBox Wactive = WeekActiveValue.Find(a => a.id == trueid);

            if (Dactive != null) DispatchModuleEvent(EventActiveDailyGet, Dactive);
            if (Wactive != null)  DispatchModuleEvent(EventActiveWeekGet, Wactive);
        }
        else if (p.result == 1) moduleGlobal.ShowMessage(223, 25);
        else if (p.result == 2) moduleGlobal.ShowMessage(223, 26);
        else if (p.result == 3) moduleGlobal.ShowMessage(223, 27);
        else Logger.LogError("fail, is id or other");
    }

    #endregion

    #region 成就
    public void GetallAchieveTask()
    {
        CsHistoryAchieves p = PacketObject.Create<CsHistoryAchieves>();
        session.Send(p);
    }

    void _Packet(ScHistoryAchieves p)
    {
        Achieveinfo.Clear();
        CanGetList.Clear();
        // GetAllAchieveTaskSate();
        ScHistoryAchieves ss = p.Clone();
        for (int i = 0; i < ss.achieves.Length; i++)
        {
            Achieveinfo.Add(ss.achieves[i]);
            PCanget(ss.achieves[i].finishVal, ss.achieves[i].condition, ss.achieves[i].id);
        }
        moduleHome.UpdateIconState(HomeIcons.Quest, ShowHomestate(0));
        SortAchieve();
        DispatchModuleEvent(EventActiveAchieveInfo);
    }

    void _Packet(ScHistoryNewAchieves p)//成就任务开启
    {
        PAchieve[] info = p.Clone().achieves;
        for (int i = 0; i < p.achieves.Length; i++)
        {
            PCanget(info[i].finishVal, info[i].condition, info[i].id);
            if (info[i].condition >= info[i].finishVal)
            {
                ShowAchieveHint = true;
                moduleHome.UpdateIconState(HomeIcons.Quest, true);
            }
        }
        Achieveinfo.AddRange(info);
        SortAchieve();
        DispatchModuleEvent(EventActiveAchieveOpen);
    }
    private void PCanget(int a, int b, int ididi)
    {
        if (!CanGetList.ContainsKey(ididi)) CanGetList.Add(ididi, a >= b);
        else CanGetList[ididi] = a >= b;
    }
    void _Packet(ScHistoryAchieveProgress p)//成就进度值有改变
    {
        ScHistoryAchieveProgress info = p.Clone();
        for (int i = 0; i < Achieveinfo.Count; i++)
        {
            //当该id进度值改变并且hasbar为true时才会做改变处理 false直接忽略
            if (Achieveinfo[i].id == info.id && Achieveinfo[i].hasBar)
            {
                Achieveinfo[i].finishVal = info.finishVal;

                //进度值改变
                DispatchModuleEvent(EventActiveAchieveValue, Achieveinfo[i]);
                return;
            }
        }
    }

    void _Packet(ScHistoryAchieveFinish p)//不可领取变为可领取
    {
        ScHistoryAchieveFinish info = p.Clone();
        for (int i = 0; i < Achieveinfo.Count; i++)
        {
            if (Achieveinfo[i].id == info.id)
            {
                CanGetList[info.id] = true;//这个id的成就现在已经可以领取 
                ShowAchieveHint = true;
                Achieveinfo[i].finishVal = info.finishVal;
                //满足了 改变状态
                PAchieve infoa = Achieveinfo[i];

                Achieveinfo.Remove(Achieveinfo[i]);
                Achieveinfo.Insert(0, infoa);
                DispatchModuleEvent(EventActiveAchieveCanGet, infoa);
                moduleHome.UpdateIconState(HomeIcons.Quest, true);//判断是否要有红点
            }
        }
    }

    public void GetAchevereward(ushort ID) 
    {
        CsHistoryAchieveReward p = PacketObject.Create<CsHistoryAchieveReward>();
        p.id = ID;
        session.Send(p);
        moduleGlobal.LockUI(0, 0.5f);
    }

    void _Packet(ScHistoryAchieveReward p)//成就奖励领取结果
    {
        moduleGlobal.UnLockUI();
        if (p.result == 0)//成功
        {
            for (int i = 0; i < Achieveinfo.Count; i++)
            {
                if (p.id == Achieveinfo[i].id) Achieveinfo[i].isDraw = true;//已领取状态
            }
            SortAchieve();
            PAchieve pinfo = Achieveinfo.Find(a => a.id == p.id);
            DispatchModuleEvent(EventActiveAchieveGet, pinfo);
        }
        else if (p.result == 1) moduleGlobal.ShowMessage(223, 28);
        else if (p.result == 2) moduleGlobal.ShowMessage(223, 29);
        else if (p.result == 3) moduleGlobal.ShowMessage(223, 30);
        else Logger.LogError("Get Achieve Award Error");
    }

    private void SortAchieve()//成就的 显示排序 判断红点 是否出现
    {
        List<PAchieve> CanGet = new List<PAchieve>();
        List<PAchieve> NoGet = new List<PAchieve>();
        List<PAchieve> alreadly = new List<PAchieve>();
        for (int i = 0; i < Achieveinfo.Count; i++)
        {
            //当可领取的时候要放在上面 
            //已经领取
            if (Achieveinfo[i].isDraw) alreadly.Add(Achieveinfo[i]);
            else
            {
                //可领取未领取
                if (Achieveinfo[i].finishVal >= Achieveinfo[i].condition) CanGet.Add(Achieveinfo[i]);
                else NoGet.Add(Achieveinfo[i]);
            }
        }
        Achieveinfo.Clear();
        Achieveinfo.AddRange(CanGet);
        Achieveinfo.AddRange(NoGet);
        Achieveinfo.AddRange(alreadly);

    }
    #endregion

    #region 协作任务

    private void InitCoop()
    {
        CoopTaskList.Clear();
        CoopInvateList.Clear();
        m_coopCheckList.Clear();
        coopTaskState.Clear();
        CoopTaskTime = 0;
        CheckTaskID = 0;
    }

    private void GetAllCooperation()
    {
        coopTaskBase = ConfigManager.GetAll<CooperationTask>();

        //获取所有我拥有的协作任务
        CsCooperateTask p = PacketObject.Create<CsCooperateTask>();
        session.Send(p);
    }
    void _Packet(ScCooperateTask p)
    {
        GetCoopTaskTime = Time.realtimeSinceStartup;
        if (p.cooList == null || p.cooList?.Length <= 0)  return;
        
        InitCoop();
        CoopTaskList.AddRange(p.Clone().cooList);
        CoopTaskTime = (int)CoopTaskList[0].remainTime;
        SortCoopList();
        GetAllcollStage();
        GetCoopState();
        moduleHome.UpdateIconState(HomeIcons.Quest, ShowHomestate(0));
        DispatchModuleEvent(EventActiveCoopInfo);
    }

    public void GetCanInvate(int taskId)
    {
        m_coopCheckList.Clear();
        //获取我可以邀请的好友
        CsCooperateFriendList p = PacketObject.Create<CsCooperateFriendList>();
        p.taskId = taskId;
        session.Send(p);

    }
    void _Packet(ScCooperateFriendList p)
    {
        CoopInvateList.Clear();
        ulong[] list = p.Clone().friendList;
        //进行排序 只显示在线的 等级排序
        for (int i = 0; i < list.Length; i++)
        {
            var black = moduleFriend.BlackList.Exists(a => a.roleId == list[i]);
            if (black) continue;
            var info = moduleFriend.FriendList.Find(a => a.roleId == list[i]);
            if (info != null && info.state != 0) CoopInvateList.Add(info);
        }
        CoopInvateList.Sort((a, b) => b.level.CompareTo(a.level));
        DispatchModuleEvent(EventActiveCoopInvate);
    }

    public void SendInvatePlayer(int name, int num)
    {
        for (int i = 0; i < m_coopCheckList.Count; i++)
        {
            SetInvateMes(num, ConfigText.GetDefalutString(name), m_coopCheckList[i]);
        }
    }
    private void SetInvateMes(int num, string monName, ulong friendId)
    {
        //发送邀请消息 
        var str = string.Format(ConfigText.GetDefalutString(223, 44), num, monName, CheckTaskID, modulePlayer.id_);
        str = str.Replace("[", "{");
        str = str.Replace("]", "}");
        str = str.Replace("(", "<");
        str = str.Replace(")", ">");
        str = str.Replace("%", "</color>");
        moduleChat.SendFriendMessage(str, 0, friendId, 3);
    }
    void _Packet(ScCooperateInviteSucced p)
    {
        //邀请成功
        var have = CoopTaskList.Find(a => a.uid == p.uid);
        if (have == null)
        {
            Logger.LogError("do not have this task ");
            return;
        }
        have.friendId = p.friendId;
        var index = CoopTaskList.FindIndex(a => a.uid == p.uid);
        if (index == -1) return;
        DispatchModuleEvent(EventActiveCoopInvateSuc, index);
    }

    public void SendCoopInvateApply(ulong uid)
    {
        if (!moduleGuide.IsActiveFunction(125))//是否解锁赏金任务
        {
            moduleGlobal.ShowMessage(223, 12);
            return;
        }
        //同意协作邀请
        CsCooperateAgree p = PacketObject.Create<CsCooperateAgree>();
        p.uid = uid;
        session.Send(p);
        moduleAnnouncement.OpenWindow(4, 2);
        //成功失败都 要跳转到协作界面
    }
    void _Packet(ScCooperateAgree p)
    {
        if (p.result == 0)
        {
            PCooperateInfo info = p.cooperate.Clone();
            if (info == null) return;

            CoopTaskList.Add(info);
            SortCoopList();
            GetAllcollStage();
            DispatchModuleEvent(EventActiveCoopApply);
        }
        //else if (p.result == 1) moduleGlobal.ShowMessage(223, 49);
        //else if (p.result == 2) moduleGlobal.ShowMessage(223, 50);
        //else if (p.result == 3) moduleGlobal.ShowMessage(223, 51);
        //else if (p.result == 4) moduleGlobal.ShowMessage(223, 52);
    }

    public void KickedOutFriend(ulong uid, ulong friendId)
    {
        //踢出协作好友
        CsCooperateKickedOut p = PacketObject.Create<CsCooperateKickedOut>();
        p.uid = uid;
        p.playerId = friendId;
        session.Send(p);

    }
    void _Packet(ScCooperateKickedOut p)
    {
        //踢出好友结果
        if (p.result == 0)
        {
            //重置进度
            var info = CoopTaskList.Find(a => a.uid == p.uid);
            if (info == null)
            {
                Logger.LogError("not have this id task");
                return;
            }
            info.friendId = 0;
            info.selfFinishVal = 0;
            info.assistFinishVal = 0;

            var index = CoopTaskList.FindIndex(a => a.uid == p.uid);
            if (index == -1) return;
            DispatchModuleEvent(EventActiveCoopKiced, index);
        }
        else if (p.result == 1) moduleGlobal.ShowMessage(223, 53);
    }

    void _Packet(ScCooperateValue p)
    {
        //任务进度更改
        var info = CoopTaskList.Find(a => a.uid == p.uid);
        if (info == null)
        {
            Logger.LogError("not have this id task");
            return;
        }
        if ((p.isSelf && modulePlayer.id_ == info.ownerId) || (!p.isSelf && modulePlayer.id_ != info.ownerId))
            info.selfFinishVal = p.value;
        else info.assistFinishVal = p.value;

        var index = CoopTaskList.FindIndex(a => a.uid == p.uid);
        if (index == -1) return;
        DispatchModuleEvent(EventActiveCoopValue, index);
    }
    void _Packet(ScCooperateFinish p)
    {
        //任务奖励达成
        var info = CoopTaskList.Find(a => a.uid == p.uid);
        var index = CoopTaskList.FindIndex(a => a.uid == p.uid);
        if (info == null || index == -1)
        {
            Logger.LogError("not have this id task");
            return;
        }
        info.state = 1;
        GetCoopState();
        ShowCoopHint = true;
        moduleHome.UpdateIconState(HomeIcons.Quest, true);
        DispatchModuleEvent(EventActiveCoopCanGet, index);
    }

    public void GetCoopReward(ulong uId)
    {
        //领取协作奖励
        CsCooperateRewardGet p = PacketObject.Create<CsCooperateRewardGet>();
        p.uid = uId;
        session.Send(p);
    }

    void _Packet(ScCooperateRewardGet p)
    {
        if (p.result == 0)
        {
            var info = CoopTaskList.Find(a => a.uid == p.uid);
            if (info == null)
            {
                Logger.LogError("not have this id task");
                return;
            }
            info.state = 2;
            GetCoopState();
            var index = CoopTaskList.FindIndex(a => a.uid == p.uid);
            if (index == -1) return;
            DispatchModuleEvent(EventActiveCoopGet, index, p.reward.Clone());
        }
        else if (p.result == 1) moduleGlobal.ShowMessage(223, 54);
        else if (p.result == 2) moduleGlobal.ShowMessage(223, 55);
        else if (p.result == 3) moduleGlobal.ShowMessage(223, 56);
        else if (p.result == 4) moduleGlobal.ShowMessage(223, 57);
        else if (p.result == 5) moduleGlobal.ShowMessage(223, 58);
    }

    void _Packet(ScCooperateBeKicked p)
    {
        var info = CoopTaskList.Find(a => a.uid == p.uid);
        if (info == null)
        {
            Logger.LogError("not have this id task");
            return;
        }
        CoopTaskList.Remove(info);
        SortCoopList();
        GetAllcollStage();
        GetCoopState();
        moduleHome.UpdateIconState(HomeIcons.Quest, ShowHomestate(0));
        DispatchModuleEvent(EventActiveCoopBeKiced);
    }

    private void GetCoopState()
    {
        coopTaskState.Clear();
        for (int i = 0; i < CoopTaskList.Count; i++)
        {
            if (coopTaskState.ContainsKey(CoopTaskList[i].uid)) continue;
            coopTaskState.Add(CoopTaskList[i].uid, CoopTaskList[i].state);
        }
    }

    private void SortCoopList()
    {
        List<PCooperateInfo> coopList = new List<PCooperateInfo>();
        for (int i = 0; i < CoopTaskList.Count; i++)
        {
            var info = coopTaskBase.Find(a => a.ID == CoopTaskList[i].taskId);
            if (info != null && info.type == 1) coopList.Add(CoopTaskList[i]);
        }
        coopList.Sort((a, b) => a.taskId.CompareTo(b.taskId));
        for (int i = 0; i < coopList.Count; i++)
        {
            CoopTaskList.Remove(coopList[i]);
        }
        CoopTaskList.Sort((a, b) => a.taskId.CompareTo(b.taskId));
        CoopTaskList.InsertRange(0, coopList);
    }

    //判断是否拥有协作关系
    public bool CheckCoop(ulong playerId)
    {
        for (int i = 0; i < CoopTaskList.Count; i++)
        {
            if (CoopTaskList[i] == null) continue;
            var ids = CoopTaskList[i].friendId;
            if (modulePlayer.id_ != CoopTaskList[i].ownerId)
            {
                ids = CoopTaskList[i].ownerId;
            }
            if (ids == playerId) return true;
        }
        return false;
    }

    #endregion

    #region 击杀怪物关卡提示
    public Dictionary<int, List<TaskInfo>> collTask = new Dictionary<int, List<TaskInfo>>();//所有出现的关卡 
    public List<DropInfo> CoopShowList = new List<DropInfo>();

    public void GetAllcollStage()
    {
        collTask.Clear();
        for (int i = 0; i < CoopTaskList.Count; i++)
        {
            if (CoopTaskList[i] == null) continue;
            var task = coopTaskBase.Find(a => a.ID == CoopTaskList[i].taskId);
            if (task == null || task.conditions == null) continue;
            var index = 0;
            if (CoopTaskList[i].ownerId != modulePlayer.id_) index = 1;
            if (index >= task.conditions.Length)
            {
                Logger.LogError("XML PCooperateInfo id {0} condition is short", CoopTaskList[i].taskId);
                continue;
            }
            int[] ids = task.conditions[index]?.monsterId;
            GetMonsterShow(task.ID, ids);
        }
    }
    public void GetMonsterShow(int coopId, int[] monsterId)
    {
        if (collTask.ContainsKey(coopId) || monsterId.Length <= 0) return;
        List<TaskInfo> task = new List<TaskInfo>();

        for (int i = 0; i < moduleChase.allTasks.Count; i++)
        {
            var info = moduleChase.allTasks[i];
            if (info == null) continue;
            StageInfo stage = ConfigManager.Get<StageInfo>(info.stageId);
            if (stage == null) continue;

            var s = ConfigManager.Get<SceneEventInfo>(stage.sceneEventId);
            if (s == null) continue;
            var scene = s.GetAllTranScenceID();//所有跳转场景id
            var have = GetHaveMonster(stage.sceneEventId, monsterId);
            for (int j = 0; j < scene.Count; j++)
            {
                if (have) break;
                have = GetHaveMonster(scene[j], monsterId);
            }
            if (have)
            {
                var add = task.Find(a => a.ID == info.ID);
                if (add == null) task.Add(info);
            }
        }
        collTask.Add(coopId, task);
    }
    private bool GetHaveMonster(int scenId, int[] monsterId)
    {
        var s = ConfigManager.Get<SceneEventInfo>(scenId);
        if (s == null) return false;
        var monsterList = s.GetAllMonsters();

        if (monsterList == null) return false;
        for (int i = 0; i < monsterList.Count; i++)
        {
            for (int j = 0; j < monsterId.Length; j++)
            {
                if (monsterList[i] == monsterId[j]) return true;
            }
        }
        return false;
    }

    public void GetAllMonsterStage(int coopId)
    {
        if (!collTask.ContainsKey(coopId)) return;
        CoopShowList.Clear();
        for (int i = 0; i < collTask[coopId].Count; i++)
        {
            var info = collTask[coopId][i];
            if (info == null) continue;

            var type = moduleChase.GetCurrentTaskType(info);

            DropInfo showInfo = moduleGlobal.GetNewDrop(-1, "", false);

            if (type == TaskType.Active) showInfo = moduleGlobal.SetActive(info, false);
            else if (type == TaskType.Emergency) showInfo = moduleGlobal.SetEmer(info, false);
            else if (type == TaskType.Awake) showInfo = moduleGlobal.SetAwake(info, false);
            else if (type == TaskType.Easy || type == TaskType.Difficult || type == TaskType.Nightmare)
            {
                showInfo = moduleGlobal.SetDiffcut(info, false);
            }
            else continue;

            if (showInfo == null || showInfo.open == false) continue;
            CoopShowList.Add(showInfo);
        }
    }

    #endregion

    #region hint
    public bool ShowHomestate(int type)//判断main界面状态
    {
        bool Canget = false;//0 有可领取的奖励 1 有未完成的任务
        bool havetask = false;
        bool daily = DailyHint();
        bool achieve = AchieveHint();
        bool coop = CoopHint();

        for (int i = 0; i < DailyList.Count; i++)
        {
            if (DailyList[i].isOpen && DailyList[i].state == 0) havetask = true;
        }
        var open = moduleGuide.IsActiveFunction(125);
        if (open)
        {
            if (achieve || coop || daily) Canget = true;
        }
        else
        {
            if (achieve || daily) Canget = true;
        }
        if (type == 0) return Canget;
        else if (type == 1) return havetask;
        return false;
    }
    public bool CoopHint()
    {
        foreach (var item in coopTaskState)
        {
            if (item.Value == 1)
            {
                ShowCoopHint = true;
                return true;
            }
        }
        return false;
    }
    public bool AchieveHint()
    {
        for (int i = 0; i < Achieveinfo.Count; i++)
        {
            if (!Achieveinfo[i].isDraw)//未领取 且 现在值 大于目标值
            {
                if (Achieveinfo[i].finishVal >= Achieveinfo[i].condition)
                {
                    ShowAchieveHint = true;
                    return true;
                }

            }
        }
        return false;
    }
    public bool DailyHint()
    {
        for (int i = 0; i < DailyList.Count; i++)
        {
            if (DailyList[i].isOpen && DailyList[i].state == 1)
            {
                ShowDialyHint = true;
                return true;
            }
        }
        foreach (var item in Activestae)
        {
            if (item.Value == EnumActiveState.CanPick)
            {
                ShowDialyHint = true;
                return true;
            }
        }
        return false;
    }
    #endregion

    #region combo
    public void SendMaxCombo()
    {
        if (moduleBattle.maxCombo <= 0) return;
        if (!Level.current || Level.current is Level_Test) return;

        var p = PacketObject.Create<CsHistoryMaxCombo>();
        p.combo = (ushort)moduleBattle.maxCombo;
        session.Send(p);
        moduleBattle.maxCombo = 0;
    }
    #endregion

    #region 结算击杀数据 
    //加载data中
    public List<PMonsterData> GetMonsterData()
    {
        List<PMonsterData> data = new List<PMonsterData>();
        foreach (var item in modulePVE.totalMonsterDeathDic)
        {
            PMonsterData d = PacketObject.Create<PMonsterData>(); ;
            d.monsterId = (uint)item.Key;
            d.num = (ushort)item.Value;
            data.Add(d);
        }
        return data;
    }
    #endregion

}
