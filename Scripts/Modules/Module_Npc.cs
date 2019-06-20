/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-13
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

public class Module_Npc : Module<Module_Npc>
{
    #region NPCMESSAGE

    public class NpcMessage : INpcMessage
    {
        public static NpcMessage defaultNpc { get; private set; }

        public Creature curNpcCreature { get; private set; }
        public NpcInfo npcInfo { get; private set; }
        public NpcActionInfo actionInfo { get; private set; }

        private PNpcEngagement m_engagementMsg;

        private PNpcInfo m_npcInfo;

        public bool isNull { get { return m_npcInfo == null; } }

        /// <summary> npc的羁绊等级:1-21</summary>
        public int fetterLv { get { return m_npcInfo != null ? m_npcInfo.currentLv : 0; } }

        private int m_lastFetterValue = 0;
        /// <summary> npc上一级的羁绊等级:1-21</summary>
        public int lastFetterLv{
            get
            {
                if (m_lastFetterValue == 0) m_lastFetterValue = m_npcInfo != null ? m_npcInfo.currentLv : 0;
                return m_lastFetterValue;
            }
            private set { m_lastFetterValue = value; }
        }

        public int maxFetterLv { get { return NpcGoodFeelingInfo.npcExps != null ? NpcGoodFeelingInfo.npcExps[NpcGoodFeelingInfo.npcExps.Count - 1].ID : 21; } }

        /// <summary> 7个大阶段:0-6</summary>
        public int fetterStage { get { return (int)((float)(fetterLv - 1) / 3); } }

        /// <summary> 上一阶段 7个大阶段:0-6</summary>
        public int lastFetterStage { get { return (int)((float)(lastFetterLv - 1) / 3); } }

        /// <summary>满足约会的大阶段:0-6</summary>
        public int datingFetterStage { get { return (int)((float)(npcInfo.unlockLv - 1) / 3); } }

        /// <summary> 最大阶段:理论为6</summary>
        public int maxFetterStage { get { return (int)((float)(maxFetterLv - 1) / 3); } }

        /// <summary> 当前等级的名字</summary>
        public string curLvName { get { return GetLvName(fetterStage, fetterLv); } }

        /// <summary> 上一等级的名字</summary>
        public string lastLvName { get { return GetLvName(lastFetterStage, lastFetterLv); } }

        /// <summary> 当前阶段的名字</summary>
        public string curStageName { get { return GetStageName(fetterStage); } }

        /// <summary> 上一阶段的名字</summary>
        public string lastStageName { get { return GetStageName(lastFetterStage); } }

        /// <summary> 当前阶段下的各个等级名字</summary>
        public string[] belongStageName { get { return GetCurStageNames(fetterStage); } }

        /// <summary> 4个状态机阶段:0-3</summary>
        public int stateStage
        {
            get
            {
                int s = (int)Math.Ceiling(((float)(fetterStage + 1) / 2)) - 1;
                return s;
            }
        }

        /// <summary> npc的当前的总羁绊值</summary>
        public int totalFetterValue
        {
            get
            {
                if (m_npcInfo == null || NpcGoodFeelingInfo.npcExps == null || NpcGoodFeelingInfo.npcExps.Count < 1) return 0;

                int value = (int)NpcGoodFeelingInfo.npcExps.Find(p => p.ID == m_npcInfo.currentLv)?.exp;
                return nowFetterValue + value;
            }
        }

        /// <summary> npc的当前的羁绊值</summary>
        public int nowFetterValue { get { return m_npcInfo != null ? m_npcInfo.currentExp : 0; } }

        public int lastFetterValue { get; private set; }

        /// <summary> 下一级需要的羁绊值</summary>
        public int toFetterValue
        {
            get
            {
                if (m_npcInfo == null || NpcGoodFeelingInfo.npcExps == null || NpcGoodFeelingInfo.npcExps.Count < 1) return 0;
                if (m_npcInfo.currentLv >= maxFetterLv)
                    return 0;

                uint exp = (uint)NpcGoodFeelingInfo.npcExps.Find(p => p.ID == m_npcInfo.currentLv + 1)?.exp;
                uint _exp = (uint)NpcGoodFeelingInfo.npcExps.Find(p => p.ID == m_npcInfo.currentLv)?.exp;
                return (int)(exp - _exp);
            }
        }

        /// <summary> 上一级需要的羁绊值</summary>
        public int lastToFetterValue
        {
            get
            {
                if (m_npcInfo == null || NpcGoodFeelingInfo.npcExps == null || NpcGoodFeelingInfo.npcExps.Count < 1) return 0;
                if (lastFetterLv >= maxFetterLv)
                    return 0;

                uint exp = (uint)NpcGoodFeelingInfo.npcExps.Find(p => p.ID == lastFetterLv + 1)?.exp;
                uint _exp = (uint)NpcGoodFeelingInfo.npcExps.Find(p => p.ID == lastFetterLv)?.exp;
                return (int)(exp - _exp);
            }
        }

        /// <summary> 羁绊进度值</summary>
        public float fetterProgress { get { return (float)nowFetterValue / toFetterValue; } }

        /// <summary> NPC的ID</summary>
        public ushort npcId { get { return m_npcInfo != null ? (ushort)m_npcInfo.npcId : (ushort)1; } }

        /// <summary> NPC的type</summary>
        public NpcTypeID npcType { get { return (NpcTypeID)npcId; } }

        /// <summary> npc的icon</summary>
        public string icon { get { return m_npcInfo != null && npcInfo != null ? npcInfo.icon : ""; } }

        /// <summary> npc的name</summary>
        public string name { get { return m_npcInfo != null && npcInfo != null ? Util.GetString(npcInfo.name) : ""; } }

        public string uiName { get { return npcType.ToString(); } }

        public ushort _mode { get { return m_npcInfo.mode; } }

        /// <summary> npc的mode</summary>
        public string mode
        {
            get
            {
                if (m_npcInfo == null) return "npc_04";

                var prop = ConfigManager.Get<PropItemInfo>(m_npcInfo.mode);
                if (prop == null || prop.mesh == null || prop.mesh.Length < 1)
                {
                    var info = ConfigManager.Get<NpcInfo>(m_npcInfo.npcId);
                    return info != null ? info.mode : "npc_04";
                }
                return prop.mesh[0];
            }
        }

        /// <summary> npc的星力等级</summary>
        public int starLv
        {
            get { return Mathf.Max(1, m_npcInfo.starStLv); }
            set { if (m_npcInfo != null) m_npcInfo.starStLv = (ushort)value; }
        }

        public bool starIsMax
        {
            get
            {
                var info = ConfigManager.Get<NpcPerfusionInfo>(starLv + 1);
                return info == null;
            }
        }

        /// <summary> npc的星力等级</summary>
        public uint starExp
        {
            get { return m_npcInfo != null ? m_npcInfo.starExp : 0; }
            set { if (m_npcInfo != null) m_npcInfo.starExp = value; }
        }

        public float starProcess
        {
            get
            {
                var data = ConfigManager.Get<NpcPerfusionInfo>(starLv);
                if (!data || data.exp == 0) return 0;
                if (starExp > data.exp)
                {
                    Logger.LogError($"数据异常，到达升级点还没升级 当前等级：{starLv}-经验进度({starExp}/{data.exp})");
                }
                return ((float)starExp) / data.exp;
            }
        }

        /// <summary> npc的心情</summary>
        public int mood { get { return m_engagementMsg != null ? m_engagementMsg.mood : 0; } }

        /// <summary> npc的上一次的心情值</summary>
        public int lastMood { get; private set; }

        /// <summary> npc的体力值</summary>
        public int bodyPower { get { return m_engagementMsg != null ? m_engagementMsg.bodyPower : 0; } }

        /// <summary> npc的上一次体力值</summary>
        public int lastBodyPower { get; private set; }

        /// <summary> npc的体力上限</summary>
        public int maxBodyPower { get { return moduleGlobal.system.maxNpcBobyPower; } }

        /// <summary> 是否解锁了约会</summary>
        public bool isUnlockEngagement { get { return fetterLv >= npcInfo.unlockLv; } }

        /// <summary> 当前NPC是否正在约会</summary>
        public bool isCurEngagement { get; private set; }

        public NpcMessage(PNpcInfo _npcinfo, PNpcEngagement engagement = null)
        {
            m_npcInfo = _npcinfo;
            npcInfo = ConfigManager.Get<NpcInfo>(m_npcInfo.npcId);
            if (npcInfo == null) return;
            actionInfo = ConfigManager.Get<NpcActionInfo>(m_npcInfo.npcId);

            m_engagementMsg = engagement;
            isCurEngagement = engagement != null;
            if (m_engagementMsg != null)
            {
                Logger.LogDetail("m_engagementMsg.npcid={0}", m_engagementMsg.npcId);
                lastMood = m_engagementMsg.mood;
                lastBodyPower = m_engagementMsg.bodyPower;
            }
        }

        public static void CreateDefault(int id)
        {
            var npc = PacketObject.Create<PNpcInfo>();
            npc.npcId = (ushort)id;
            npc.currentLv = 1;
            npc.currentExp = 0;
            npc.starStLv = 1;
            npc.starExp = 0;

            var info = ConfigManager.Get<NpcInfo>(id);
            if (info == null)
            {
                Logger.LogError("config_npcInfo do not have the msg id=={0}", id);
                npc.mode = 39001;
            }
            npc.mode = (ushort)info.cloth;

            defaultNpc = new NpcMessage(npc);
        }

        public void UpdateEngagement(PNpcEngagement engagementMsg)
        {
            if (m_engagementMsg == null) m_engagementMsg = engagementMsg;
            isCurEngagement = true;
        }

        public void UpdateFetterLv(sbyte lv)
        {
            if (m_npcInfo == null) return;
            lastFetterLv = m_npcInfo.currentLv;
            m_npcInfo.currentLv = lv;
        }

        public void UpdateFetterValue(int value)
        {
            if (m_npcInfo == null) return;
            lastFetterValue = m_npcInfo.currentExp;
            m_npcInfo.currentExp = value;
        }

        public void UpdateMode(ushort _itemTypeId)
        {
            if (m_npcInfo == null) return;
            m_npcInfo.mode = _itemTypeId;
        }

        public void UpdateMood(int value)
        {
            if (m_engagementMsg == null) return;
            lastMood = m_engagementMsg.mood;
            m_engagementMsg.mood = value;
        }

        public void UpdateBobyPower(int value)
        {
            if (m_engagementMsg == null) return;
            lastBodyPower = m_engagementMsg.bodyPower;
            m_engagementMsg.bodyPower = value;
        }

        public string GetStageName(int stage)
        {
            return Util.GetString(169, stage);
        }

        public string[] GetCurStageNames(int stage)
        {
            string[] names = new string[3];
            var str = ConfigManager.Get<ConfigText>(170 + fetterStage);
            if (str == null) return names;
            names = str.text;
            return names;
        }

        public string GetLvName(int stage, int lv)
        {
            var names = GetCurStageNames(stage);

            int index = lv % 3;
            index = index == 1 ? 0 : index == 2 ? 1 : 2;

            return names[index];
        }

        public void UpdateCurCreature(Creature c)
        {
            if (c == null) return;
            curNpcCreature = c;
        }
    }

    #endregion

    public const string NpcAddExpSuccessEvent       = "NpcAddExpSuccessEvent";      //增加经验成功--不升级的情况下从这刷新进度条
    public const string NpcLvUpEvent                = "NpcLvUpEvent";               //升级的情况下 从这刷新进度条
    public const string NpcPerfusionChangeEvent     = "NpcPerfusionChangeEvent";    //经验进度更改
    public const string Response_NpcPerfusionChange = "Response_NpcPerfusionChange";//星力值改变
    public const string FocusNpcEvent               = "FocusNpcEvent";              //发送消息自动聚焦到某Npc
    public const string EventRefreshNpcMood         = "EventRefreshNpcMood";        //约会npc心情改变
    public const string EventRefreshNpcBodyPower    = "EventRefreshNpcBodyPower";   //约会npc体力改变
    public const string EventReceiveLoginNpcInfo    = "EventReceiveLoginNpcInfo";   //登录的时候获取NpcInfo

    public List<NpcMessage> allNpcs { get { return m_allNpcs; } }
    private List<NpcMessage> m_allNpcs = new List<NpcMessage>();

    public Dictionary<NpcTypeID, NpcMessage> allNpcInfo { get { return m_allNpcInfo; } }
    private Dictionary<NpcTypeID, NpcMessage> m_allNpcInfo = new Dictionary<NpcTypeID, NpcMessage>();

    /// <summary>当前window界面所在的NPC</summary>
    public NpcMessage curNpc { get; private set; }

    private int eventIndex;

    public bool isNpcLv { get; set; }
    public NpcMessage lvupNpc { get; private set; }

    /// <summary>当前约会的NPC</summary>
    public NpcMessage curEngagementNpc
    {
        get
        {
            foreach (var item in m_allNpcInfo)
            {
                if (item.Value.isCurEngagement)
                    return item.Value;
            }
            return null;
        }
    }

    #region 获取相应的npc

    /// <summary>
    /// 根据ID获取对应的NPC的信息
    /// </summary>
    /// <param name="npcid"></param>
    /// <returns></returns>
    public NpcMessage GetTargetNpc(NpcTypeID npcid)
    {
        if (!m_allNpcInfo.ContainsKey(npcid))
        {
            if (npcid == NpcTypeID.None) return null;

            NpcMessage.CreateDefault((int)npcid);
            m_allNpcInfo.Add(npcid, NpcMessage.defaultNpc);
            return NpcMessage.defaultNpc;
        }
        return m_allNpcInfo.Get(npcid);
    }

    /// <summary>
    /// 设置当前界面的NPCTYPE,主要是用在有NPC模型展示的window界面
    /// </summary>
    /// <param name="npcid">npcmono上的NPCID</param>
    public void SetCurNpc(NpcTypeID npcid)
    {
        if (npcid == NpcTypeID.None) npcid = NpcTypeID.TravalShopNpc;

        curNpc = GetTargetNpc(npcid);
        if (curNpc != null)
            Logger.LogDetail("curNpc= {0} type = {1}", curNpc.npcId, curNpc.npcType);
    }

    #endregion

    #region 初始请求

    /// <summary>
    /// NPC信息
    /// </summary>
    /// <param name="p"></param>
    void _Packet(ScNpcInfo p)
    {
        if (p != null && p.npcinfos != null && p.npcinfos.Length > 0)
        {
            m_allNpcs.Clear();
            m_allNpcInfo.Clear();

            PNpcInfo[] infos = null;
            p.npcinfos.CopyTo(ref infos);
            List<PNpcInfo> list = new List<PNpcInfo>(infos);
            list.Sort((a, b) => a.npcId.CompareTo(b.npcId));

            PNpcEngagement engagement = null;
            if (p.engagementNpc != null) p.engagementNpc.CopyTo(ref engagement);

            for (int i = 0; i < list.Count; i++)
            {
                NpcMessage msg = null;
                if (engagement != null && engagement.npcId == list[i].npcId) msg = new NpcMessage(list[i], engagement);
                else msg = new NpcMessage(list[i]);

                if (msg.isNull || msg.npcInfo == null || msg.npcInfo.type == 2) continue;

                m_allNpcs.Add(msg);
                m_allNpcInfo.Add((NpcTypeID)list[i].npcId, msg);
            }

            DispatchModuleEvent(EventReceiveLoginNpcInfo);
        }
    }

    void _Packet(ScNpcDatingSelectNpc p)
    {
        if (p.result != 0) return;

        var npc = GetTargetNpc((NpcTypeID)p.npc.npcId);
        if (npc == null) return;
        PNpcEngagement engagementMsg = null;
        p.npc.CopyTo(ref engagementMsg);
        npc.UpdateEngagement(engagementMsg);
    }

    /// <summary>
    /// 升级新等级
    /// </summary>
    /// <param name="p"></param>
    void _Packet(ScNpcLv p)
    {
        if (p == null) return;
        if (moduleGift.isClickSend)
        {
            moduleGift.isGiveGift = true;
            moduleGift.isClickSend = false;
        }

        int lastLV = 0;
        var type = (NpcTypeID)p.npcId;
        var npc = GetTargetNpc(type);

        if (npc == null) return;

        lastLV = npc.fetterLv;
        npc.UpdateFetterLv(p.newLv);
        npc.UpdateFetterValue((int)p.curLvExpr);

        if (!(Level.current is Level_Home))
        {
            lvupNpc = npc;
            isNpcLv = true;
        }

        bool upLv = lastLV != npc.fetterLv;
        bool upStage = lastLV % 3 == 0 && p.newLv % 3 == 1;

        if (!upLv) return;
        DispatchEvent(NpcLvUpEvent, Event_.Pop(p, upLv, upStage));
        DispatchModuleEvent(NpcLvUpEvent, p);
    }
    #endregion

    #region 触摸请求
    /// <summary>
    /// 摸部位
    /// </summary>
    /// <param name="npcID"></param>
    /// <param name="posID"></param>
    public void SendAddExp(ushort npcID, sbyte posID)
    {
        var npc = GetTargetNpc((NpcTypeID)npcID);
        if (npc == null || npc.fetterLv >= npc.maxFetterLv) return;
        CsNpcTouch click = PacketObject.Create<CsNpcTouch>();
        click.npcId = npcID;
        click.body = posID;
        session.Send(click);
    }

    void _Packet(ScNpcTouch p)
    {
        if (p.result == 1)
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.NpcUIText, 0));
        else if (p.result == 2)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.NpcUIText, 1));
            //todo:出进度条,但是不加经验,在进度条上方显示一个不再加经验的提示,全局提示不要
        }
    }

    /// <summary>
    /// 加经验消息
    /// </summary>
    /// <param name="p"></param>
    void _Packet(ScNpcExpr p)
    {
        if (p == null) return;

        var type = (NpcTypeID)p.npcId;
        var npc = GetTargetNpc(type);
        if (npc != null)
            npc.UpdateFetterValue(npc.nowFetterValue + p.addExpr);

        if (moduleGift.isClickSend)
        {
            moduleGift.isGiveGift = true;
            moduleGift.isClickSend = false;
        }
        DispatchEvent(NpcAddExpSuccessEvent, Event_.Pop((uint)p.addExpr));
        DispatchModuleEvent(NpcAddExpSuccessEvent, p.addExpr);
    }

    void _Packet(ScNpcExpLock p)
    {
        if (p == null) return;
        var npc = GetTargetNpc((NpcTypeID)p.npcId);
        if (npc == null) return;
        moduleGlobal.ShowMessage(Util.Format(Util.GetString(210, 10), npc.name));

        var task = npc.npcInfo.tasks.Find(o => o.fetterLv == npc.fetterLv);
        var hintID = task.hintStoryID;
        if (hintID <= 0) return;
        //触发对话--引导玩家完成羁绊进阶任务
        var window = Window.GetOpenedWindow<Window_NPCDatingSettlement>();
        var settle = Window.GetOpenedWindow<Window_Settlement>();
        if (window != null && window.actived)
        {
            DelayEvents.Remove(eventIndex);
            eventIndex = DelayEvents.Add(() => moduleNPCDating.DoDatingEvent(hintID), GeneralConfigInfo.defaultConfig.wstOnEngagement);
        }
        else if (settle != null && settle.actived)
        {
            DelayEvents.Remove(eventIndex);
            eventIndex = DelayEvents.Add(() => moduleNPCDating.DoDatingEvent(hintID), GeneralConfigInfo.defaultConfig.wstOnPve);
        }
        else
            moduleNPCDating.DoDatingEvent(hintID);
    }

    void _Packet(ScNpcChangeMood p)
    {
        if (p == null) return;

        var type = (NpcTypeID)p.npcId;
        var npc = GetTargetNpc(type);
        if (npc != null) npc.UpdateMood(p.moodValue);

        DispatchModuleEvent(EventRefreshNpcMood, npc.mood);
    }

    void _Packet(ScNpcChangeBodyPower p)
    {
        if (p == null) return;

        var type = (NpcTypeID)p.npcId;
        var npc = GetTargetNpc(type);
        if (npc != null) npc.UpdateBobyPower(p.bodyValue);

        DispatchModuleEvent(EventRefreshNpcBodyPower, npc.bodyPower);
    }

    private void _Packet(ScNpcPerfusion msg)
    {
        moduleGlobal.UnLockUI();
        INpcMessage npcInfo = GetTargetNpc((NpcTypeID)msg.npcId);
        npcInfo.starLv = msg.starLv;
        npcInfo.starExp = msg.startExp;
        DispatchEvent(NpcPerfusionChangeEvent);
        DispatchModuleEvent(Response_NpcPerfusionChange, msg);
    }

    #endregion

    public void UnLockFetter(ushort _npcId)
    {
        CsNpcUnlockFetter p = PacketObject.Create<CsNpcUnlockFetter>();
        p.npcId = _npcId;
        session.Send(p);
    }

    void _Packet(ScNpcUnlockFetter p)
    {
        var npc = GetTargetNpc((NpcTypeID)p.npcId);
        var task = GetTargetTask(npc);
        if (task == null || task.taskState != EnumTaskState.Finish) return;

        //解锁失败
        if (p.result != 0) moduleGlobal.ShowMessage((int)TextForMatType.NpcUIText, p.result + 5);
    }

    public Task GetTargetTask(NpcMessage npc, byte taskId = 0)
    {
        if (npc == null || npc.npcInfo == null || npc.npcInfo.tasks == null) return null;

        var tasks = npc.npcInfo.tasks;
        for (int i = 0; i < tasks.Length; i++)
        {
            if (tasks[i].fetterLv == npc.fetterLv)
            {
                //调用任务module接口,返回是否有这个任务
                if (taskId != 0 && tasks[i].taskId != taskId) continue;
                return moduleTask.GetTask((byte)tasks[i].taskId);
            }
        }
        return null;
    }

    #region module_base

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        m_allNpcInfo.Clear();
        m_allNpcs.Clear();
        curNpc = null;
        DelayEvents.Remove(eventIndex);
        isNpcLv = false;
        lvupNpc = null;
    }
    #endregion
}