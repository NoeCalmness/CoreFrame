/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-07-27
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

public class Module_Rune : Module<Module_Rune>
{
    public const string DataForRuneConsume = "DataForRuneConsume";//符文消耗
    public const string IntentifySuccess = "IntentifySuccess";//强化成功
    public const string EventChangeEquip = "EventChangeEquip";//换符文或者卸下
    #region RuneProperty

    /// <summary>
    /// 0:其他界面 1:仓库界面
    /// 决定从window_runestart返回到哪里
    /// </summary>
    public int sourceType { get; set; }

    public PItem curOpItem { get; private set; }

    public RuneInWhichPanel runeOpType { get; set; }

    public Dictionary<RuneType, List<PItem>> allSubTypeDic { get { return m_allSubTypeDic; } }
    private Dictionary<RuneType, List<PItem>> m_allSubTypeDic = new Dictionary<RuneType, List<PItem>>();

    /// <summary>
    /// 当前背包的符文
    /// </summary>
    public List<PItem> currentInBag { get { return m_currentInBag; } }
    private List<PItem> m_currentInBag = new List<PItem>();

    /// <summary>
    ///当前穿戴的符文
    /// </summary>
    public List<PItem> currentEquip { get { return m_currentEquip; } }
    private List<PItem> m_currentEquip = new List<PItem>();

    /// <summary>
    /// 缓存升级字典
    /// </summary>
    public List<PItem> upLevelList { get { return m_upLevelList; } }
    private List<PItem> m_upLevelList = new List<PItem>();

    /// <summary>
    /// 新手引导强化时需要排在前几位的符文链表
    /// </summary>
    public List<ushort> excludeSortID { get; set; } = new List<ushort>();
    /// <summary>
    /// 新手引导进化前强行装备的符文
    /// </summary>
    public PItem2 forceRune;

    /// <summary>
    /// 缓存升星字典
    /// </summary>
    public List<PItem> upStarList { get { return m_upStarList; } }
    private List<PItem> m_upStarList = new List<PItem>();

    /// <summary>
    /// 套装
    /// </summary>
    public Dictionary<ushort, int> suiteDic { get { return m_suiteDic; } } //ushort:套装ID,int:数量
    private Dictionary<ushort, int> m_suiteDic = new Dictionary<ushort, int>();

    /// <summary>
    /// 套装效果,用于计算角色属性变化
    /// </summary>
    public Dictionary<int, Dictionary<uint, double>> runeSuiteEffect { get { return m_runeSuiteEffect; } }//int 增加type(1为+,2位*),ushort 属性id, float 套装加的值
    private Dictionary<int, Dictionary<uint, double>> m_runeSuiteEffect = new Dictionary<int, Dictionary<uint, double>>();

    public List<ulong> swallowedIdList { get; set; } = new List<ulong>();//吞噬符文的列表
    public List<ulong> evolveList { get; set; } = new List<ulong>();//进化符文的列表

    private PItem changePitem;//切换的符文

    public List<UpStarInfo> starInfo { get { return m_starInfo; } }
    private List<UpStarInfo> m_starInfo = new List<UpStarInfo>();

    public List<LevelExpInfo> expInfo { get { return m_expInfo; } }
    private List<LevelExpInfo> m_expInfo = new List<LevelExpInfo>();

    public List<GrowAttributeInfo> growInfo { get { return m_growInfo; } }
    private List<GrowAttributeInfo> m_growInfo = new List<GrowAttributeInfo>();

    public List<PropItemInfo> allProps { get { return m_allProps; } }
    private List<PropItemInfo> m_allProps = new List<PropItemInfo>();

    public Dictionary<ushort, List<LevelExpInfo>> newExpInfo { get { return m_newExpInfo; } }
    private Dictionary<ushort, List<LevelExpInfo>> m_newExpInfo = new Dictionary<ushort, List<LevelExpInfo>>();

    /// <summary>
    /// 外部的已读属性
    /// </summary>
    private bool isRead;

    /// <summary>
    /// 内部的状态
    /// </summary>
    private Dictionary<RuneType, bool> subState = new Dictionary<RuneType, bool>();

    private Dictionary<RuneType, bool> subRead = new Dictionary<RuneType, bool>();

    private int readSubNum;
    private int changeSubNum;
    /// <summary> 当处于强化过程中金币变化在强化界面不刷新变化 </summary>
    public bool stayIntentify { get; private set; }
    #endregion

    #region module_base
    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        m_currentInBag.Clear();
        m_currentEquip.Clear();
        m_upLevelList.Clear();
        m_upStarList.Clear();
        m_suiteDic.Clear();
        swallowedIdList.Clear();
        evolveList.Clear();
        m_runeSuiteEffect.Clear();
        excludeSortID.Clear();
        m_starInfo.Clear();
        m_expInfo.Clear();
        m_newExpInfo.Clear();
        curOpItem = null;
        m_growInfo.Clear();
        m_allProps.Clear();
        m_allSubTypeDic.Clear();
        isRead = false;
        subState.Clear();
        readSubNum = 0;
        changeSubNum = 0;
        subRead.Clear();
        sourceType = 0;
        stayIntentify = false;
    }

    private void OnInitConfig()
    {
        m_starInfo = ConfigManager.GetAll<UpStarInfo>();
        m_expInfo = ConfigManager.GetAll<LevelExpInfo>();
        m_growInfo = ConfigManager.GetAll<GrowAttributeInfo>();
        m_allProps = ConfigManager.GetAll<PropItemInfo>();

        subState.Clear();
        subRead.Clear();
        for (var i = RuneType.One; i <= RuneType.Six; i++)
        {
            subState.Add(i, false);
            subRead.Add(i, false);
        }
    }

    #endregion

    #region packet
    void _Packet(ScRoleBagInfo bagClothes)
    {
        PItem[] items = null;
        bagClothes.itemInfo.CopyTo(ref items);

        m_currentInBag.Clear();
        for (int i = 0, length = items.Length; i < length; i++)
        {
            if (items[i].GetPropItem().IsValidVocation(modulePlayer.proto) && items[i].GetPropItem().itemType == PropType.Rune)
            {
                var isContains = Contains(m_currentInBag, items[i]);
                if (!isContains) m_currentInBag.Add(items[i]);
            }
        }
        ListSortAll();
    }

    void _Packet(ScRoleEquipedItems p)
    {
        PItem[] items = null;
        p.itemInfo.CopyTo(ref items);

        m_currentEquip.Clear();

        for (int i = 0, length = items.Length; i < length; i++)
        {
            //筛选符文
            if (items[i].GetPropItem().itemType == PropType.Rune)
            {
                var isContains = Contains(m_currentEquip, items[i]);
                if (!isContains) m_currentEquip.Add(items[i]);
            }
        }
        ListSortAll();
        FindSuite(m_currentEquip);//找套装
    }

    void _Packet(ScRoleConsumeItem accept)
    {
        var info = ConfigManager.Get<PropItemInfo>(accept.itemTypeId);

        if (info != null && info.itemType == PropType.Rune)
        {
            OperateRemoveData(m_currentInBag, accept.itemId);

            if (runeOpType == RuneInWhichPanel.Intentify)
                OperateRemoveData(m_upLevelList, accept.itemId);
            else if (runeOpType == RuneInWhichPanel.Evolve)
                OperateRemoveData(m_upStarList, accept.itemId);

            ListSortAll();
        }
    }

    void _Packet(ScEquipRuneEnhance p)//强化
    {
        stayIntentify = false;
        if (p.result == 0)
            AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
        else
        {
            switch (p.result)
            {
                case 1: moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 33)); break;
                case 2: moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 34)); modulePlayer.DispatchModuleEvent(Module_Player.EventBuySuccessCoin); break;
                case 3: moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 35)); break;
                case 4: moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 36)); break;
                default: break;
            }
        }
    }

    void _Packet(ScEquipRuneEvolved p)//升星
    {
        stayIntentify = false;
        if (p.result == 0)
            AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
        else
        {
            switch (p.result)
            {
                case 1: moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 37)); break;
                case 2: moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 38)); modulePlayer.DispatchModuleEvent(Module_Player.EventBuySuccessCoin); break;
                case 3: moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 39)); break;
                case 4: moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 40)); break;
                case 5: moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 41)); break;
                case 6: moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 42)); break;
                case 7: moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 43)); break;
                default: break;
            }
        }
    }

    void _Packet(ScRoleItemInfo p)
    {
        if (curOpItem == null) return;

        if (curOpItem.itemId != p.item.itemId) return;
        curOpItem.growAttr = p.item.growAttr;

        PItem itemInBag = m_currentInBag.Find(o => o.itemId == curOpItem.itemId);
        if (itemInBag != null)
            itemInBag.growAttr = curOpItem.growAttr;
        else
        {
            PItem itemEquip = m_currentEquip.Find(o => o.itemId == curOpItem.itemId);
            if (itemEquip != null)
                itemEquip.growAttr = curOpItem.growAttr;
            else
                Logger.LogError("curOpItem is not in equip and bag,please check curOpItem");
        }

        DispatchModuleEvent(IntentifySuccess);
    }

    void _Packet(ScEquipDown p)
    {
        if (p.result == 0)
        {
            if (changePitem != null && changePitem.GetPropItem().itemType == PropType.Rune)
                AddAndRemoveData(changePitem, false);
        }

        DispatchModuleEvent(EventChangeEquip, 1, p.result);
    }

    void _Packet(ScEquipChange p)
    {
        if (p.result == 0)
        {
            if (changePitem != null && changePitem.GetPropItem().itemType == PropType.Rune)
                AddAndRemoveData(changePitem, true);
        }

        DispatchModuleEvent(EventChangeEquip, 2, p.result);
    }

    void _Packet(ScRoleAddItem p)
    {
        PropItemInfo info = ConfigManager.Get<PropItemInfo>(p.itemTypeId);
        if (info == null || info.itemType != PropType.Rune) return;

        PItem pitem = moduleEquip.GetNewPItem(p.itemId, p.itemTypeId);
        pitem.growAttr = p.growAttr;
        pitem.num = (ushort)p.num;
        AddNewRune(pitem);
    }

    void _Packet(ScSystemSetting p)
    {
        OnInitConfig();
        GetNewExpDIC(p.runeRate);
    }

    #endregion

    #region publicFunction

    public void UpdateRead(int index, bool _isRead, int subType = 0)
    {
        if (index == 0)
        {
            isRead = _isRead;
            UpdateSubNewState();

            readSubNum = RedPointNumber();
        }
        else if (index == 1 && subType > 0)
        {
            var type = (RuneType)subType;
            if (_isRead) moduleCangku.RemveNewItem(type);

            if (subRead.ContainsKey(type)) subRead[type] = _isRead;          
        }
    }

    private int RedPointNumber()
    {
        int number = 0;
        for (var i = RuneType.One; i <= RuneType.Six; i++)
        {
            var s= GetCurSubHintBool(i);
            if (s) number++;
        }
        return number;
    }

    /// <summary>
    /// 外部红点规则
    /// </summary>
    /// <returns></returns>
    public bool IsShowNotice()
    {
        if (!isRead)
        {
            UpdateSubNewState();

            return GetHaveRedPoint();
        }
        else
        {
            UpdateSubNewState();
            changeSubNum = RedPointNumber();
            return changeSubNum > readSubNum;
        }
    }

    private bool GetHaveRedPoint()
    {
        for (var i = RuneType.One; i <= RuneType.Six; i++)
        {
            var _bool = GetCurSubHintBool(i);
            if (_bool) return true;
        }
        return false;
    }

    /// <summary>
    /// 内部红点规则
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool GetCurSubHintBool(RuneType type)
    {
        if (!subRead.ContainsKey(type)) return false;
        if (subRead[type]) return false;//如果已读了,直接返回false;

        if (!subState.ContainsKey(type)) return false;
        return subState[type];
    }

    private void UpdateSubNewState()
    {
        for (var i = RuneType.One; i <= RuneType.Six; i++)
        {
            if (!subState.ContainsKey(i)) continue;
            var _equip = IsEquipThisSubType(i);
            var subCount = m_allSubTypeDic.ContainsKey(i) && m_allSubTypeDic[i].Count > 0;
            if (!_equip && subCount) subState[i] = true;
            else subState[i] = false;
        }
    }

    public bool IsEquipThisSubType(RuneType _subType)
    {
        var equip = m_currentEquip.Find(p => p.GetPropItem()?.subType == (int)_subType);
        return equip != null;
    }

    public uint GetCurSwallowExp(PItem swItem)
    {
        if (swItem == null || swItem.growAttr == null || swItem.growAttr.runeAttr == null) return 0;

        var prop = ConfigManager.Get<PropItemInfo>(swItem.itemTypeId);
        if (prop == null) return 0;

        var now = m_expInfo.Find(p => p.ID == swItem.growAttr.runeAttr.level);

        if (now) return (uint)(prop.swallowedExpr + (swItem.growAttr.runeAttr.expr + now.exp) * moduleGlobal.system.baseStarSwallowed);

        return 0;
    }

    public uint GetMaxExp(PItem intentify)
    {
        if (intentify == null || intentify.growAttr == null || intentify.growAttr.runeAttr == null) return 0;

        var star = m_starInfo.Find(o => o.ID == intentify.growAttr.runeAttr.star);
        if (star == null) return 0;

        var maxLv = star.Lv;
        LevelExpInfo max = null;
        LevelExpInfo now = null;
        if (m_newExpInfo.ContainsKey(intentify.itemTypeId))
        {
            var list = m_newExpInfo[intentify.itemTypeId];
            max = list.Find(p => p.ID == maxLv);
            now = list.Find(p => p.ID == intentify.growAttr.runeAttr.level);
        }
        else
        {
            max = m_expInfo.Find(p => p.ID == maxLv);
            now = m_expInfo.Find(p => p.ID == intentify.growAttr.runeAttr.level);
        }

        if (max && now)
            return max.exp - (now.exp + intentify.growAttr.runeAttr.expr);

        return 0;
    }

    /// <summary>
    /// 获取升星字典
    /// </summary>
    public void GetUpStarList(PItem except)
    {
        if (except == null) return;
        List<PItem> items = CopyList(m_currentInBag);

        var same = items.Find(o => o.itemId == except.itemId);
        if (same != null) items.Remove(same);
        AddUpStarList(except, items);
    }

    public void SortGuideItem()
    {
        if (excludeSortID == null || excludeSortID.Count < 1) return;

        for (int i = excludeSortID.Count - 1; i >= 0; i--)
        {
            var _item = m_upStarList.Find(p => p.itemTypeId == excludeSortID[i]);
            if (_item == null) continue;

            m_upStarList.Remove(_item);
            m_upStarList.Insert(0, _item);
        }

        excludeSortID.Clear();
    }

    /// <summary>
    /// 获取升级字典
    /// </summary>
    /// <param name="items"></param>
    public void GetUpLvList(PItem except)
    {
        if (except == null) return;

        List<PItem> items = CopyList(m_currentInBag);
        var same = items.Find(o => o.itemId == except.itemId);
        if (same != null) items.Remove(same);

        AddUpLevelList(items);
    }

    public void SendSwallowed(ulong intentifyId, List<ulong> SwallowedList)
    {
        CsEquipRuneEnhance p = PacketObject.Create<CsEquipRuneEnhance>();
        p.enhanceId = intentifyId;
        p.swallowedIdList = SwallowedList.ToArray();

        session.Send(p);
        stayIntentify = true;
    }

    public void SendEvolved(ulong intentifyId, List<ulong> evolvedList)
    {
        CsEquipRuneEvolved p = PacketObject.Create<CsEquipRuneEvolved>();
        p.evolvedId = intentifyId;
        p.swallowedIdList = evolvedList.ToArray();

        session.Send(p);
        stayIntentify = true;
    }

    /// <summary>
    /// 装备或者卸下
    /// </summary>
    /// <param name="item"></param>
    /// <param name="isAdd"></param>
    public void ChangeDressData(PItem item, bool isAdd)
    {
        changePitem = item;
        List<ulong> changeSubtype = new List<ulong>();
        changeSubtype.Add(item.itemId);
        //装备
        if (isAdd)
            moduleEquip.SendChangeClothes(changeSubtype);
        //卸下
        else
            moduleEquip.SendTakeOffClothes(changeSubtype);
    }

    public double[] GetChangeAttrs(PItem target, PItem equip, ushort attrId)
    {
        var ori = GetCurRuneAttrNumber(equip, attrId);
        var now = GetCurRuneAttrNumber(target, attrId);

        return new double[] { ori, now, now - ori };
    }

    public int GetNewLv(PItem item, uint expTotal = 0)
    {
        if (item == null || item.growAttr == null || item.growAttr.runeAttr == null) return 0;
        if (expTotal == 0) return item.growAttr.runeAttr.level;

        int lv = 0;
        List<LevelExpInfo> expList = null;
        if (m_newExpInfo.ContainsKey(item.itemTypeId)) expList = m_newExpInfo[item.itemTypeId];
        else expList = m_expInfo;

        var maxLv = GetCurMaxLv(item.growAttr.runeAttr.star);
        var max = expList.Find(p => p.ID == maxLv);
        var maxExp = max != null ? max.exp : 0;

        var _exp = expList.Find(p => p.ID == item.growAttr.runeAttr.level);
        if (_exp == null) return 0;
        expTotal = item.growAttr.runeAttr.expr + _exp.exp + expTotal;

        if (expTotal >= maxExp) lv = maxLv;
        else
        {
            var exp = expList.FindAll(p => expTotal >= p.exp);
            if (exp != null && exp.Count > 0) lv = exp[exp.Count - 1].ID;
        }

        return lv < 0 ? 0 : lv;
    }

    public ushort GetCurAttrId(int itemAttrId)
    {
        var grow = ConfigManager.Get<GrowAttributeInfo>(itemAttrId);
        if (!grow) return 0;
        return (ushort)grow.attrId;
    }

    public double GetCurRuneAttrNumber(PItem item, ushort attrid)
    {
        if (item == null || item.growAttr == null || item.growAttr.runeAttr == null || item.growAttr.runeAttr.randAttrs == null) return 0;
        var attrs = m_growInfo.FindAll(p => p.attrId == attrid);

        double number = 0;
        for (int i = 0; i < attrs.Count; i++)
        {
            var attr = GetContainsAttrByID(item, (ushort)attrs[i].ID);
            if (attr == null) continue;
            number += attr.attrVal;
        }

        return number;
    }

    /// <summary>
    /// attrid是属性id
    /// </summary>
    /// <param name="item"></param>
    /// <param name="attrid"></param>
    /// <returns></returns>
    public string GetCurRuneAttr_(PItem item, ushort attrid)
    {
        if (item == null || item.growAttr == null || item.growAttr.runeAttr == null || item.growAttr.runeAttr.randAttrs == null) return "";
        var attrs = m_growInfo.FindAll(p => p.attrId == attrid);

        double number = 0;
        for (int i = 0; i < attrs.Count; i++)
        {
            var attr = GetContainsAttrByID(item, (ushort)attrs[i].ID);
            if (attr == null) continue;
            number += attr.attrVal;
        }

        if (GeneralConfigInfo.defaultConfig.percentAttriIds.Contains(attrid)) return number != 0 ? number.ToString("P2") : "";
        return number != 0 ? number.ToString("F0") : "";
    }

    /// <summary>
    /// attrid是属性库中的id
    /// </summary>
    /// <param name="curItem"></param>
    /// <param name="attrId"></param>
    /// <returns></returns>
    public string GetCurRuneAttr(PItem curItem, ushort attrId)
    {
        if (curItem == null || curItem.growAttr == null || curItem.growAttr.runeAttr == null || curItem.growAttr.runeAttr.randAttrs == null) return "";
        var attr = Array.Find(curItem.growAttr.runeAttr.randAttrs, p => p.itemAttrId == attrId);
        if (attr == null) return "";

        var _attrid = GetCurAttrId(attrId);
        if (GeneralConfigInfo.defaultConfig.percentAttriIds.Contains(_attrid)) return attr.attrVal.ToString("P2");
        return attr.attrVal.ToString("F0");
    }

    public string GetNewAttrInUp(PItem curItem, ushort attrId, int targetLv)
    {
        if (curItem == null || curItem.growAttr == null || curItem.growAttr.runeAttr == null || curItem.growAttr.runeAttr.randAttrs == null) return "";
        var attr = Array.Find(curItem.growAttr.runeAttr.randAttrs, p => p.itemAttrId == attrId);
        if (attr == null) return "";

        var grow = ConfigManager.Get<GrowAttributeInfo>(attrId);
        if (grow == null) return "";

        double value = 0;
        if (attr.itemAttrType == 1)
        {
            if (grow.growType == 1)
            {
                var number = targetLv - curItem.growAttr.runeAttr.level;
                number = number <= 0 ? 0 : number;
                value = grow.growValue * number;
            }
            else
                value = attr.initVal * (1 + targetLv * grow.growValue) - attr.attrVal;
        }

        if (GeneralConfigInfo.defaultConfig.percentAttriIds.Contains((ushort)grow.attrId)) return value == 0 ? "+0%" : "+" + value.ToString("P2");
        return value == 0 ? "+0" : "+" + value.ToString("F0");
    }

    public string GetNewAttrInEvo(PItem curItem, ushort attrId)
    {
        if (curItem == null || curItem.growAttr == null || curItem.growAttr.runeAttr == null || curItem.growAttr.runeAttr.randAttrs == null) return "";
        var attr = Array.Find(curItem.growAttr.runeAttr.randAttrs, p => p.itemAttrId == attrId);
        if (attr == null) return "";

        var grow = ConfigManager.Get<GrowAttributeInfo>(attrId);
        if (grow == null) return "";

        double value = 0;
        if (attr.itemAttrType == 2)
        {
            if (grow.growType == 1)
                value = grow.growValue;
            else
                value = attr.initVal * (1 + curItem.growAttr.runeAttr.star * grow.growValue) - attr.attrVal;
        }

        if (GeneralConfigInfo.defaultConfig.percentAttriIds.Contains((ushort)grow.attrId)) return value == 0 ? "+0%" : "+" + value.ToString("P2");
        return value == 0 ? "+0" : "+" + value.ToString("F0");
    }

    public void SetCurOpItem(PItem item)
    {
        curOpItem = item;
    }

    public string GetRomaString(PropItemInfo prop)
    {
        if (prop == null) return "";

        var index = prop.subType - 1 < 0 ? 0 : prop.subType - 1;

        return Util.GetString(10103, index);
    }

    public Color[] GetCurSuiteColor(PropItemInfo prop)
    {
        if (prop == null) return GeneralConfigInfo.defaultConfig.norRColor;

        int number = 0;
        if (!m_suiteDic.TryGetValue(prop.suite, out number)) return GeneralConfigInfo.defaultConfig.norRColor;
        //如果是大于4件套,显示黄色
        if (number >= 4) return GeneralConfigInfo.defaultConfig.fourSColor;

        var index = GetSuiteIndex(prop);
        switch (index)
        {
            case -1: return GeneralConfigInfo.defaultConfig.norRColor;
            case 0: return GeneralConfigInfo.defaultConfig.twoSOColor;
            case 1: return GeneralConfigInfo.defaultConfig.twoSTColor;
            case 2: return GeneralConfigInfo.defaultConfig.twoSThColor;
        }

        return GeneralConfigInfo.defaultConfig.norRColor;
    }

    public Color[] GetCurSuiteColor(int index)
    {
        switch (index)
        {
            case 0: return GeneralConfigInfo.defaultConfig.twoSOColor;
            case 1: return GeneralConfigInfo.defaultConfig.twoSTColor;
            case 2: return GeneralConfigInfo.defaultConfig.twoSThColor;
        }
        return GeneralConfigInfo.defaultConfig.norRColor;
    }

    public Color GetCurQualityColor(Color[] colors, int star)
    {
        Color _color = Color.white;
        if (colors == null) return _color;
        if (colors.Length >= star)
        {
            var index = star - 1;
            index = index < 0 ? 0 : index;
            _color = colors[index];
        }
        return _color;
    }

    public Color GetColorByIndex(Color[] color, int index)
    {
        if (color.Length > index)
            return color[index];
        return Color.white;
    }

    public string GetSuiteDesc(ushort suiteId, int count)
    {
        var buff = ConfigManager.Get<RuneBuffInfo>(suiteId);
        if (buff == null) return "";

        if (count == 4)
        {
            var _buff = ConfigManager.Get<BuffInfo>(buff.buffId);
            return _buff != null ? _buff.desc : "";
        }

        var id = (int)TextForMatType.RuneUIText;
        if (count == 2)
        {
            var attrName = Util.GetString((int)TextForMatType.AttributeUIText, (int)buff.attrId);
            if (buff.addType == 1)
            {
                if (GeneralConfigInfo.defaultConfig.percentAttriIds.Contains((ushort)buff.attrId))
                    return Util.GetString(id, 5, attrName, buff.value.ToString("P2"));
                else return Util.GetString(id, 5, attrName, buff.value);
            }
            else return Util.GetString(id, 5, attrName, buff.value.ToString("P2"));
        }
        return "";
    }

    public Dictionary<uint, double> GetNewAttributes()
    {
        var selfAttr = new Dictionary<uint, double>();

        for (int i = 0; i < m_currentEquip.Count; i++)
        {
            var ranAttrs = m_currentEquip[i].growAttr?.runeAttr?.randAttrs;
            for (int k = 0; k < ranAttrs.Length; k++)
            {
                var info = ConfigManager.Get<GrowAttributeInfo>(ranAttrs[k].itemAttrId);
                if (info == null) continue;
                if (!selfAttr.ContainsKey((uint)info.attrId)) selfAttr.Add((uint)info.attrId, ranAttrs[k].attrVal);
                else selfAttr[(uint)info.attrId] += ranAttrs[k].attrVal;
            }
        }

        var addDic = new Dictionary<uint, double>();//递增值,type=1
        var perDic = new Dictionary<uint, double>();//百分比值,type=2

        foreach (var item in m_suiteDic)
        {
            if (item.Value < 2) continue;
            int count = item.Value == 6 ? 2 : 1;

            var info = ConfigManager.Get<RuneBuffInfo>(item.Key);
            if (info == null) continue;

            if (info.addType == 1)
            {
                if (!addDic.ContainsKey(info.attrId)) addDic.Add(info.attrId, info.value * count);
                else addDic[info.attrId] += info.value * count;
            }
            else if (info.addType == 2)
            {
                if (!perDic.ContainsKey(info.attrId)) perDic.Add(info.attrId, info.value * count);
                else perDic[info.attrId] += info.value * count;
            }
        }

        for (int i = 0; i < m_currentEquip.Count; i++)
        {
            var prop = ConfigManager.Get<PropItemInfo>(m_currentEquip[i].itemTypeId);
            if (prop == null) continue;
            var attrs = prop.attributes;
            if (attrs == null || attrs.Length < 1) continue;

            for (int k = 0; k < attrs.Length; k++)
            {
                var attr = attrs[i];
                if (attr.type == 1)
                {
                    if (!addDic.ContainsKey(attr.id)) addDic.Add(attr.id, attr.value);
                    else addDic[attr.id] += attr.value;
                }
                else if (attr.type == 2)
                {
                    if (!perDic.ContainsKey(attr.id)) perDic.Add(attr.id, attr.value);
                    else perDic[attr.id] += attr.value;
                }
            }
        }

        foreach (var item in addDic)
        {
            if (!selfAttr.ContainsKey(item.Key)) selfAttr.Add(item.Key, item.Value);
            else selfAttr[item.Key] += item.Value;
        }

        foreach (var item in perDic)
        {
            var roleAttr = GetRoleAttribute(item.Key);
            if (!selfAttr.ContainsKey(item.Key)) selfAttr.Add(item.Key, roleAttr * item.Value);
            else selfAttr[item.Key] += roleAttr * item.Value;
        }

        return selfAttr;
    }

    public bool IsEquip(PItem item)
    {
        if (item == null) return false;
        var now = m_currentEquip.Find(p => p.itemId == item.itemId);
        return now != null;
    }

    public PItem GetEquipSameSubType(PItem item)
    {
        if (IsEquip(item)) return null;
        var now = m_currentEquip.Find(p => p.GetPropItem()?.subType == item.GetPropItem()?.subType);
        return now;
    }

    public bool IsMaxLv(int level)
    {
        if (m_starInfo == null) return false;
        return level >= m_starInfo[m_starInfo.Count - 1].Lv;
    }

    public bool IsCurMaxLv(int level, int star)
    {
        if (m_starInfo == null) return false;
        var _star = m_starInfo.Find(p => p.ID == star);
        return level >= _star.Lv;
    }

    public int GetCurMaxLv(int star)
    {
        if (m_starInfo == null) return 0;
        var _star = m_starInfo.Find(p => p.ID == star);
        return _star != null ? (int)_star.Lv : 0;
    }

    public int GetTotalExp(PItem item, int expTotal)
    {
        if (item == null || item.growAttr == null || item.growAttr.runeAttr == null) return 0;

        if (IsCurMaxLv(item.growAttr.runeAttr.level, item.growAttr.runeAttr.star)) return 1;

        List<LevelExpInfo> expList = null;
        if (m_newExpInfo.ContainsKey(item.itemTypeId)) expList = m_newExpInfo[item.itemTypeId];
        else expList = m_expInfo;

        var now = expList.Find(p => p.ID == item.growAttr.runeAttr.level);
        if (now == null) return 0;
        return (int)(now.exp + item.growAttr.runeAttr.expr + expTotal);
    }

    public float GetExpProgress(PItem item, int _level = 0, int expTotal = 0)
    {
        if (item == null || item.growAttr == null || item.growAttr.runeAttr == null) return 0;

        var level = _level != 0 ? _level : item.growAttr.runeAttr.level;
        if (IsCurMaxLv(level, item.growAttr.runeAttr.star)) return 0;

        List<LevelExpInfo> expList = null;
        if (m_newExpInfo.ContainsKey(item.itemTypeId)) expList = m_newExpInfo[item.itemTypeId];
        else expList = m_expInfo;

        var now = expList.Find(p => p.ID == level);
        var next = expList.Find(p => p.ID == level + 1);

        if (expTotal == 0) return (float)item.growAttr?.runeAttr?.expr / (next.exp - now.exp);
        else
        {
            expTotal = GetTotalExp(item, expTotal);
            return (float)(expTotal - now.exp) / (next.exp - now.exp);
        }
    }

    public string GetExpDescString(PItem item, int _level = 0, int expTotal = 0)
    {
        if (item == null || item.growAttr == null || item.growAttr.runeAttr == null) return "";

        var level = _level != 0 ? _level : item.growAttr.runeAttr.level;
        if (IsCurMaxLv(level, item.growAttr.runeAttr.star)) return Util.GetString((int)TextForMatType.RuneUIText, 16);

        List<LevelExpInfo> expList = null;
        if (m_newExpInfo.ContainsKey(item.itemTypeId)) expList = m_newExpInfo[item.itemTypeId];
        else expList = m_expInfo;

        var now = expList.Find(p => p.ID == level);
        var next = expList.Find(p => p.ID == level + 1);

        if (expTotal == 0) return $"{item.growAttr?.runeAttr?.expr}/{next.exp - now.exp}";
        else
        {
            expTotal = GetTotalExp(item, expTotal);
            return $"{expTotal - now.exp}/{next.exp - now.exp}";
        }
    }

    #endregion

    #region priveteFunction

    /// <summary>
    /// 升级字典
    /// </summary>
    private void AddUpLevelList(List<PItem> _currentInBag)
    {
        if (_currentInBag == null) return;
        m_upLevelList.Clear();

        ListSortUp(_currentInBag);
        m_upLevelList.AddRange(_currentInBag);//此时all对应星级
    }

    private void AddUpStarList(PItem except, List<PItem> _currentInBag)
    {
        if (_currentInBag == null) return;
        m_upStarList.Clear();

        var starList = _currentInBag.FindAll(p => p.growAttr?.runeAttr?.star == except.growAttr?.runeAttr?.star);

        if (starList == null) return;
        starList.Sort((a, b) =>
        {
            if (a == null || a.growAttr == null || a.growAttr.runeAttr == null) return 0;
            if (b == null || b.growAttr == null || b.growAttr.runeAttr == null) return 0;

            int result = a.growAttr.runeAttr.level.CompareTo(b.growAttr.runeAttr.level);
            if (result == 0) result = a.itemTypeId.CompareTo(b.itemTypeId);
            return result;
        });

        m_upStarList.AddRange(starList);//此时all对应星级
    }

    private void ListSortUp(List<PItem> targetList)
    {
        if (targetList == null) return;
        targetList.Sort((a, b) =>
        {
            if (a == null || a.growAttr == null || a.growAttr.runeAttr == null) return 0;
            if (b == null || b.growAttr == null || b.growAttr.runeAttr == null) return 0;

            int result = a.growAttr.runeAttr.star.CompareTo(b.growAttr.runeAttr.star);
            if (result == 0)
            {
                result = a.growAttr.runeAttr.level.CompareTo(b.growAttr.runeAttr.level);
                if (result == 0)
                    result = a.itemTypeId.CompareTo(b.itemTypeId);
            }
            return result;
        });
    }

    private void ListSortAll()
    {
        var list = new List<PItem>();
        list.AddRange(m_currentInBag);
        list.AddRange(m_currentEquip);
        list.Sort((a, b) =>
        {
            if (a == null || a.growAttr == null || a.growAttr.runeAttr == null) return 0;
            if (b == null || b.growAttr == null || b.growAttr.runeAttr == null) return 0;

            int result = b.growAttr.runeAttr.star.CompareTo(a.growAttr.runeAttr.star);
            if (result == 0)
            {
                result = b.growAttr.runeAttr.level.CompareTo(a.growAttr.runeAttr.level);
                if (result == 0)
                    result = a.itemTypeId.CompareTo(b.itemTypeId);
            }
            return result;
        });

        AddDataToAllSubTypeDic(list);
    }

    private void AddDataToAllSubTypeDic(List<PItem> items)
    {
        m_allSubTypeDic.Clear();
        for (int i = 0; i < items.Count; i++)
        {
            var prop = ConfigManager.Get<PropItemInfo>(items[i].itemTypeId);
            if (!prop) continue;

            if (!m_allSubTypeDic.ContainsKey((RuneType)prop.subType)) m_allSubTypeDic.Add((RuneType)prop.subType, new List<PItem>());

            m_allSubTypeDic[(RuneType)prop.subType].Add(items[i]);
        }
    }

    private void GetNewExpDIC(PRuneExpRate[] expRate)
    {
        if (expRate == null || expRate.Length < 1) return;
        m_newExpInfo.Clear();
        for (int i = 0; i < expRate.Length; i++)
        {
            if (!m_newExpInfo.ContainsKey(expRate[i].runeTypeId)) m_newExpInfo.Remove(expRate[i].runeTypeId);
            var explist = CopyNewExpInfo(m_expInfo, expRate[i].rate);
            m_newExpInfo.Add(expRate[i].runeTypeId, explist);
        }
    }

    private List<LevelExpInfo> CopyNewExpInfo(List<LevelExpInfo> list, float rate)
    {
        List<LevelExpInfo> refList = new List<LevelExpInfo>();
        if (list == null || list.Count < 1) return refList;

        for (int i = 0; i < list.Count; i++)
        {
            LevelExpInfo newExp = new LevelExpInfo();
            newExp.ID = list[i].ID;
            newExp.exp = (uint)(list[i].exp * rate);

            refList.Add(newExp);
        }
        return refList;
    }

    private void AddNewRune(PItem addItem)
    {
        if (addItem == null) return;
        var isContains = Contains(m_currentInBag, addItem);
        if (!isContains) m_currentInBag.Add(addItem);

        ListSortAll();

        var prop = ConfigManager.Get<PropItemInfo>(addItem.itemTypeId);
        if (prop == null) return;

        UpdateSubNewState();
        UpdateRead(1, false, prop.subType);//如果有新增的,重置subtype的read为false
    }

    private PItemRandomAttr GetContainsAttrByID(PItem target, ushort attrId)
    {
        if (target == null || target.growAttr == null || target.growAttr.runeAttr == null
            || target.growAttr.runeAttr.randAttrs == null || target.growAttr.runeAttr.randAttrs.Length < 1) return null;

        var attrs = target.growAttr.runeAttr.randAttrs;
        return Array.Find(attrs, p => p.itemAttrId == attrId);
    }

    private double GetRoleAttribute(uint id)
    {
        switch (id)
        {
            case 5: return moduleAttribute.attrInfo.maxHp[2];
            case 7: return moduleAttribute.attrInfo.attack[2];
            case 8: return moduleAttribute.attrInfo.defense[2];
            case 9: return moduleAttribute.attrInfo.knock[2];
            case 10: return moduleAttribute.attrInfo.knockRate[2];
            case 11: return moduleAttribute.attrInfo.artifice[2];
            case 12: return moduleAttribute.attrInfo.attackSpeed[2];
            case 13: return moduleAttribute.attrInfo.moveSpeed[2];
            case 14: return moduleAttribute.attrInfo.bone[2];
            case 15: return moduleAttribute.attrInfo.brutal[2];
            case 16: return moduleAttribute.attrInfo.angerSec[2];
            case 17: return moduleAttribute.attrInfo.gunAttack[2];
        }
        return 0;
    }

    private int GetSuiteIndex(PropItemInfo prop)
    {
        if (prop == null) return -1;
        var index = 0;

        if (m_suiteDic.ContainsKey(prop.suite))
        {
            foreach (var item in m_suiteDic)
            {
                if (item.Value < 2) continue;
                if (item.Key != prop.suite) index++;
                else return index;
            }
        }

        return -1;
    }

    private bool Contains(List<PItem> items, PItem target)
    {
        if (target == null || items == null) return true;
        if (items.Count < 1) return false;

        var contains = items.Find(p => p.itemId == target.itemId);
        return contains != null;
    }

    private List<PItem> CopyList(List<PItem> target)
    {
        List<PItem> tar = new List<PItem>();

        for (int i = 0; i < target.Count; i++)
        {
            PItem item = null;
            target[i].CopyTo(ref item);
            tar.Add(item);
        }
        return tar;
    }

    private void OperateData(List<PItem> target, PItem data, bool isAdd)
    {
        var _data = target.Find(p => p.itemId == data.itemId);
        if (isAdd)
        {
            if (_data == null) target.Add(data);
        }
        else
        {
            if (_data != null) target.Remove(_data);
        }
    }

    private void OperateRemoveData(List<PItem> target, ulong _itemId)
    {
        var _data = target.Find(p => p.itemId == _itemId);
        if (_data != null) target.Remove(_data);
    }

    private void AddAndRemoveData(PItem item, bool isEquip)
    {
        //穿符文
        if (isEquip)
        {
            PItem operateItem = m_currentEquip.Find((p) => { return p.GetPropItem().subType == changePitem.GetPropItem().subType; });
            if (operateItem != null)
            {
                OperateData(m_currentEquip, operateItem, false);
                OperateData(m_currentInBag, operateItem, true);
            }
            OperateData(m_currentEquip, item, true);
            OperateData(m_currentInBag, item, false);
        }
        else
        {
            OperateData(m_currentEquip, item, false);
            OperateData(m_currentInBag, item, true);
        }
        changePitem = null;

        FindSuite(m_currentEquip);//找套装
    }

    /// <summary>
    /// 套装筛选
    /// </summary>
    /// <param name="target"></param>
    private void FindSuite(List<PItem> target)
    {
        m_suiteDic.Clear();
        Dictionary<ushort, int> tempDic = new Dictionary<ushort, int>();
        for (int i = 0; i < target.Count; i++)
        {
            var propItem = ConfigManager.Get<PropItemInfo>(target[i].itemTypeId);
            if (tempDic.ContainsKey(propItem.suite))
            {
                int temp = tempDic[propItem.suite];

                tempDic[propItem.suite] = temp + 1;
            }
            else
                tempDic.Add(propItem.suite, 1);
        }

        foreach (var item in tempDic)
        {
            if (item.Value < 2) continue;
            if (item.Value >= 2 && item.Value < 4) m_suiteDic.Add(item.Key, 2);
            else if (item.Value >= 4 && item.Value < 6) m_suiteDic.Add(item.Key, 4);
            else if (item.Value == 6) m_suiteDic.Add(item.Key, 6);
        }

        var lists = new List<KeyValuePair<ushort, int>>(m_suiteDic);
        lists.Sort((a, b) =>
        {
            int result = a.Value.CompareTo(b.Value);
            if (result == 0) result = a.Key.CompareTo(b.Key);
            return result;
        });
        m_suiteDic.Clear();
        foreach (var item in lists)
            m_suiteDic.Add(item.Key, item.Value);

        GetRuneAddAttribute();
    }

    private void GetRuneAddAttribute()
    {
        if (m_suiteDic == null) return;
        m_runeSuiteEffect.Clear();

        foreach (var item in suiteDic)
        {
            if (item.Value <= 1) continue;
            int count = item.Value == 6 ? 2 : 1;
            var info = ConfigManager.Get<RuneBuffInfo>(item.Key);
            if (info != null)
            {
                if (!m_runeSuiteEffect.ContainsKey(info.addType))
                    m_runeSuiteEffect.Add(info.addType, new Dictionary<uint, double>());

                if (!m_runeSuiteEffect[info.addType].ContainsKey(info.attrId)) m_runeSuiteEffect[info.addType].Add(info.attrId, info.value * count);
                else m_runeSuiteEffect[info.addType][info.attrId] += info.value * count;
            }
        }
    }
    #endregion

    public void TestRun()
    {
        string[] str1 = new string[] { "0", "1" };
        string[] str2 = new string[] { "1", "2", "3", "4" };
        string[] str3 = new string[] { "1", "2", "3", "4", "5", "6" };

        string str = Util.Format("4{0}{1}0{2}", str1[UnityEngine.Random.Range(0, str1.Length)], str2[UnityEngine.Random.Range(0, str2.Length)], str3[UnityEngine.Random.Range(0, str3.Length)]);

        CsRoleGm gm = PacketObject.Create<CsRoleGm>();
        gm.gmType = 3;
        gm.args = new string[] { str };
        session.Send(gm);

        moduleEquip.SendRequestProp(RequestItemType.InBag);
    }
}

public enum RuneInWhichPanel
{
    Equip,

    Intentify,

    Evolve,
}
