// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-07-24      20:52
//  * LastModify：2018-07-26      14:49
//  ***************************************************************************************************/
#region

using System;
using System.Collections.Generic;

#endregion


public class Module_Awake : Module<Module_Awake>
{
    public Module_Awake()
    {
        currentAwake = new ushort[(int)AwakeType.Max][];
    }

    #region Events Define

    public const string Response_HeartSoulExchange      = "ResponseHeartSoulExchange";
    public const string Response_SkillOpen              = "ResponseSkillOpen";
    public const string Response_NpcActive              = "ResponseNpcPerfusion";

    public const string Notice_AwakeInfoChange          = "AwakeInfoChange";
    public const string Notice_ShowSkillOpenPanel       = "ShowSkillOpenPanel";

    public const string Event_NodeGetFocus              = "Event_NodeGetFocus";
    public const string Event_NodeLoseFocus             = "Event_NodeLoseFocus";

    public const string Event_CloseToNode               = "Event_CloseToNode";
    public const string Event_StartCloseToNode          = "Event_StartCloseToNode";
    public const string Event_StartFarAwayNode          = "Event_StartFarAwayNode";
    public const string Event_FarAwayNode               = "Event_FarAwayNode";

    #endregion

    #region static functions

    public static AwakeInfo PrevAwakeInfo(int id)
    {
        if (id == 0)
            return null;
        var info = ConfigManager.Get<AwakeInfo>(id);
        return PrevAwakeInfo(info);
    }

    public static AwakeInfo PrevAwakeInfo(AwakeInfo info)
    {
        if (info != null && info.dependId != 0)
            return ConfigManager.Get<AwakeInfo>(info.dependId);
        return null;
    }

    public static List<AwakeInfo> NextAwakeInfo(int id)
    {
        var info = ConfigManager.Get<AwakeInfo>(id);
        return NextAwakeInfo(info);
    }

    public static List<AwakeInfo> NextAwakeInfo(AwakeInfo info)
    {
        if (!info)
            return new List<AwakeInfo>(0);
        return info.nextInfoList;
    }

    #endregion

    #region Request

    public void Request_NpcPerfusion(NpcTypeID rNpcId, int rTimes)
    {
        moduleGlobal.LockUI(string.Empty, 0.5f);

        var p = PacketObject.Create<CsNpcPerfusion>();
        p.npcId = (byte)rNpcId;
        p.times = (ushort)rTimes;
        session.Send(p);
    }

    public void Request_NpcActive(NpcTypeID rNpcId, bool rActive = true)
    {
        moduleGlobal.LockUI(string.Empty, 0.5f);

        var p = PacketObject.Create<CsNpcActive>();
        p.npcId = (byte)rNpcId;
        p.active = rActive;
        session.Send(p);
    }

    public void RequestAwakeInfo()
    {
        var msg = PacketObject.Create<CsAwakeInfo>();
        session.Send(msg);

        moduleGlobal.LockUI(string.Empty, 0.5f);
    }


    public void RequestAwakeSkill(AwakeInfo rInfo)
    {
        if (!rInfo) return;
        RequestAwakeSkill(rInfo.ID);

        moduleGlobal.LockUI();
    }

    public void RequestAwakeSkill(int rKey)
    {
        //        var m = PacketObject.Create<ScAwakeSkillOpen>();
        //        m.result = 0;
        //        m.lastAwakeSkill = rKey;
        //        this._Packet(m);
        //        return;
        moduleGlobal.LockUI();

        var msg = PacketObject.Create<CsAwakeSkillOpen>();
        msg.id = rKey;
        session.Send(msg);
    }

    #endregion

    #region functions

    public uint GetBeadCount(AwakeType rType)
    {
        return GetBeadCount((int)rType - 1);
    }

    public uint GetBeadCount(int subType)
    {
        uint num = 0;
        var items = moduleEquip.GetPropItems(PropType.AwakeCurrency, subType);
        if (items != null && items.Count >= 0)
        {
            for (int i = 0; i < items.Count; i++)
                num += items[i].num;
        }
        return num;
    }

    public List<AwakeInfo> GetInfosOnLayer(AwakeType rType, int layer)
    {
        var list = ConfigManager.FindAll<AwakeInfo>(info => info.type == rType && info.layer == layer);
        list.RemoveAll(item => item.protoId != modulePlayer.proto && item.protoId != 0);
        list.Sort((a, b) => a.index.CompareTo(b.index));
        return list;
    }

    public bool IsFinishLayer(AwakeType rType, int layer)
    {
        var infos = GetInfosOnLayer(rType, layer);
        foreach (var info in infos)
        {
            if (!Check(info))
                return false;
        }
        return true;
    }

    public bool CanEnterNextLayer(AwakeType rType, int layer)
    {
        var infos = GetInfosOnLayer(rType, layer);
        foreach (var info in infos)
        {
            if (!Check(info) && NextAwakeInfo(info.ID).Count > 0)
                return false;
        }

        //没有下一层的数据必定不能跳转到下一层
        return GetInfosOnLayer(rType, layer + 1).Count > 0;
    }

    /// <summary>
    /// 检测某一个觉醒点的前置关卡是否完成
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool CheckPrev(int id)
    {
        var info = ConfigManager.Get<AwakeInfo>(id);
        if (info == null || info.layer < 0)
        {
            Logger.LogError("错误觉醒点 ID = {0}", id);
            return false;
        }
        return CheckPrev(info);
    }

    public bool CheckPrev(AwakeInfo info)
    {
        if (info == null)
            return false;
        //没有前置关卡就可以直接点亮
        if (info.dependId == 0)
            return true;
        return Check(info.dependId);
    }

    /// <summary>
    /// 检测某一个觉醒点是否完成
    /// </summary>
    /// <param name="rType"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool Check(int id)
    {
        var info = ConfigManager.Get<AwakeInfo>(id);
        if (info == null || info.layer < 0)
        {
            Logger.LogError("错误觉醒点 ID = {0}", id);
            return false;
        }
        return Check(info);
    }

    public bool Check(AwakeInfo info)
    {
        if (info == null) return false;
        var data = currentAwake[(int)info.type];
        if (data == null || info.layer >= data.Length)
            return false;

        int mark = data[info.layer];
        return mark.BitMask(info.index);
    }
    /// <summary>
    /// 检测激活状态 -1 已经激活 0 可以激活 1 不能激活 2等级不足不能激活
    /// </summary>
    /// <param name="info"></param>
    /// <returns>-1 已经激活 0 可以激活 1 不能激活 2等级不足不能激活</returns>
    public int CheckAwakeState(AwakeInfo info)
    {
        if (CheckPrev(info))
        {
            if (Check(info))
                return -1;
            if (modulePlayer.level >= info.dependlv)
                return 0;
            return 2;
        }
        return 1;
    }

    public int PrecentLayer(AwakeType rType)
    {
        if ((int)rType >= currentAwake.Length)
            return 0;

        var data = currentAwake[(int)rType];
        //没有任何数据返回初始层
        if (data == null || data.Length == 0)
            return 0;

        var layer = data.Length - 1;
        if (CanEnterNextLayer(rType, layer))
        {
            var nextInfo = GetInfosOnLayer(rType, layer + 1);
            if(nextInfo != null && nextInfo.Count > 0)
                return layer + 1;
        }

        return layer;
    }


    private void FillCurrentAwake(int id)
    {
        if (null == currentAwake)
            currentAwake = new ushort[(int)AwakeType.Max][];

        var info = ConfigManager.Get<AwakeInfo>(id);
        if (null != info)
        {
            var t = (int) info.type;
            if (t >= currentAwake.Length || t < 0)
            {
                Logger.LogError("未知的觉醒类型 type = {0}", info.type);
                return;
            }

            if (currentAwake[t] == null) currentAwake[t] = new ushort[0];
            if (info.layer >= currentAwake[t].Length)
                Array.Resize(ref currentAwake[t], info.layer + 1);
            currentAwake[t][info.layer] |= (ushort)(1 << info.index);
        }
        else
            Logger.LogError("there is no awake config id = {0}", id);
    }

    public bool CanAwake(AwakeType rType)
    {
        var infos = GetInfosOnLayer(rType, PrecentLayer(rType));
        foreach (var info in infos)
        {
            if (CheckAwakeState(info) == 0)
            {
                uint own = 0;
                var prop = ConfigManager.Get<PropItemInfo>(info.cost.itemId);
                if (prop.itemType == PropType.Currency)
                    own = modulePlayer.GetMoneyCount((CurrencySubType)prop.subType);
                else
                    own = moduleAwake.GetBeadCount(prop.subType);
                return info.cost.count <= own;
            }
        }
        return false;
    }

    public bool NeedNoticeType(AwakeType rAwakeType)
    {
        var key = new NoticeDefaultKey(NoticeType.AwakeHeart + (int) rAwakeType - 1);
        moduleNotice.SetNoticeState(key, CanAwake(rAwakeType));
        return moduleNotice.IsNeedNotice(key);
    }

    #endregion

    #region Packet

    private void _Packet(ScNpcActive msg)
    {
        moduleGlobal.UnLockUI();
        if (msg.result == 0)
        {
            //已经激活的为取消，否则为激活
            if (activeNpc == (NpcTypeID) msg.npcId)
                activeNpc = NpcTypeID.None;
            else
                activeNpc = (NpcTypeID)msg.npcId;
        }
        DispatchModuleEvent(Response_NpcActive, msg);
    }

    private void _Packet(ScAwakeInfo rInfo)
    {
        moduleGlobal.UnLockUI();

        currentAwake[(int) AwakeType.Heart]     = rInfo.awakeHeart;
        currentAwake[(int) AwakeType.Skill]     = rInfo.awakeSkill;
        currentAwake[(int) AwakeType.Energy]    = rInfo.awakeBody;
        currentAwake[(int) AwakeType.Accompany] = rInfo.awakeTrip;

        DispatchModuleEvent(Notice_AwakeInfoChange, rInfo);
    }

    

    private void _Packet(ScAwakeSkillOpen msg)
    {
        moduleGlobal.UnLockUI();
        if (msg.result == 0)
        {
            FillCurrentAwake(msg.lastAwakeSkill);
        }

        DispatchModuleEvent(Response_SkillOpen, msg);
    }

    #endregion

    #region Fields

    public ushort[][] currentAwake;

    /// <summary>
    /// 当前激活命星的Npc
    /// </summary>
    public NpcTypeID activeNpc;
    
    /// <summary>
    /// 是否初始化了Npc相伴的自动聚焦
    /// </summary>
    public bool isInitedAccompany;

    #endregion

    #region Property

    public bool NeedNotice
    {
        get { return NeedNoticeType(AwakeType.Accompany) || NeedNoticeType(AwakeType.Energy) || NeedNoticeType(AwakeType.Heart) || NeedNoticeType(AwakeType.Skill); }
    }
    #endregion

}