/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-10-10
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class Module_Cangku : Module<Module_Cangku>
{
    public const string EventCangkuInfo = "EventCangkuInfo";
    /// <summary>
    /// param1 = PItem, param2 = add count
    /// </summary>
    public const string EventCangkuAddItem = "EventCangkuAddItem";
    /// <summary>
    /// param1 = PItem, param2 = remove count
    /// </summary>
    public const string EventCangkuRemoveItem = "EventCangkuRemoveItem";

    public const string EventCangkuItemLock = "EventCangkuItemLock";//锁定改变

    public WareType chickType = WareType.Prop;
    public bool UseSubType = true;
    public int sortType = 0;
    private bool FirstShowMain = true;
    
    public PItem m_detailItem { get; set; }

    public List<PItem> newProp = new List<PItem>();
    /// <summary>
    /// 碎片数据
    /// </summary>
    private List<PItem> m_canCompse { get { return m_wareCollections?.Get(WareType.Debris); } }

    /// <summary>
    /// 符文
    /// </summary>
    public List<PItem> RuneItemList { get { return m_wareCollections?.Get(WareType.Rune); } }

    /// <summary>
    /// 所有仓库数据集合
    /// </summary>
    private Dictionary<WareType, List<PItem>> m_wareCollections { get; set; } = new Dictionary<WareType, List<PItem>>();

    public Dictionary<ulong, float> PitemTime = new Dictionary<ulong, float>();
    
    private List<PItem> m_allItems = new List<PItem>();

    private List<PropItemInfo> m_allProp = new List<PropItemInfo>();
    private List<PropItemInfo> m_allFashion = new List<PropItemInfo>();

    /// <summary>
    /// 所有可以合成的物品
    /// </summary>
    public List<PItem> AllCompse = new List<PItem>();
    /// <summary>
    /// 当前看到的所有可合成
    /// </summary>
    public List<PItem> CanCompose = new List<PItem>();


    public List<PItem> WareLableInfo(WareType type, int sort = 0)
    {
        return SortQuity(m_wareCollections?.Get(type), sort);
    }

    public bool NothingShow(WareType type)
    {
        List<PItem> pitem = m_wareCollections?.Get(type);
        return pitem.Count == 0 ? true : false;
    }

    public void SetLock(ulong itemId, int type)
    {
        //0 解锁 1 上锁
        var p = PacketObject.Create<CsSystemPropLock>();
        p.itemId = itemId;
        p.isLock = (sbyte)type;
        session.Send(p);

        moduleEquip.ChangeStateLock(itemId, type);

        var i = GetItemByGUID(itemId);
        if (i != null) i.isLock = (sbyte)type;

        DispatchModuleEvent(EventCangkuItemLock, itemId);
    }
    
    #region get pitem  num

    public int GetItemCount(int itemTypeId)
    {
        if (m_allItems.Count == 0) return 0;
        var c = 0;
        for (int i = 0; i < m_allItems.Count; i++)
            if (m_allItems[i] != null && m_allItems[i].itemTypeId == itemTypeId) c += (int)m_allItems[i].num;

        return c;
    }

    public int GetItemCount(ulong itemId)
    {
        if (m_allItems.Count == 0) return 0;
        var c = 0;
        for (int i = 0; i < m_allItems.Count; i++)
            if (m_allItems[i] != null && m_allItems[i].itemId == itemId) c += (int)m_allItems[i].num;

        return c;
    }

    public PItem GetItemByID(int itemID)
    {
        if (m_allItems.Count == 0) return null;
        return m_allItems.Find(i => i?.itemTypeId == itemID);
    }

    public PItem GetItemByGUID(ulong guid)
    {
        if (m_allItems.Count == 0) return null;
        return m_allItems.Find(i => i?.itemId == guid);
    }
    #endregion

    #region Get all info

    private void InitAllWareInfo()
    {
        for (int i = (int)WareType.None, count = (int)WareType.Count; i < count; i++)
        {
            if (m_wareCollections.ContainsKey((WareType)i))
            {
                m_wareCollections[(WareType)i].Clear();
                continue;
            }

            m_wareCollections.Add((WareType)i, new List<PItem>());
        }
        GetAllFashionProp();
    }
    public PItem GetNewPItem(ulong itemId, ushort itemTypeId, uint num, PItemGrowAttr attr)
    {
        PropItemInfo itemInfo = ConfigManager.Get<PropItemInfo>((int)itemTypeId);
        if (itemInfo == null)
            return null;

        PItem pitem = PacketObject.Create<PItem>();
        pitem.itemId = itemId;
        pitem.itemTypeId = (ushort)itemInfo.ID;
        pitem.num = num;
        pitem.growAttr = attr;
        pitem.isLock = 0;
        pitem.timeLimit = (int)itemInfo.timeLimit * 86400;
        return pitem;
    }

    public void AddToWareCollection(PItem[] items)
    {
        foreach (var item in items) AddItemToWareCollection(item);
    }

    public void AddItemToWareCollection(PItem item)
    {
        if (item == null || item.itemId == 0) return;

        var info = item.GetPropItem();
        if (info == null || !info.IsValidVocation(modulePlayer.proto)) return;

        WareType t = GetWareType(item);

        if (!m_wareCollections.ContainsKey(t) || m_wareCollections[t] == null)
        {
            Logger.LogError("item id = {0}, find lableType = {1} cannot be finded in propritem lableType", info.itemNameId, t);
            return;
        }
        if (t == WareType.None) return;

        var _item = m_wareCollections[t].Find(p => p.itemId == item.itemId);
        if (_item == null) m_wareCollections[t].Add(item);
    }

    public WareType GetWareType(PItem item)
    {
        if (item == null) return WareType.None;
        var info = item?.GetPropItem();
        if (!info) return WareType.None;
        return (WareType)info.lableType;
    }

    public void WareRemoveProp(ulong itemId, uint _num)
    {
        bool removeComplete = false;
        foreach (var item in m_wareCollections)
        {
            List<PItem> list = item.Value;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].itemId == itemId)
                {
                    removeComplete = true;
                    if (list[i].num <= 0) list.RemoveAt(i);
                    break;
                }
            }
            if (removeComplete) break;
        }
    }

    public void ClearSelectCollection(int type, PItem[] list)
    {
        switch (type)
        {
            case 0:
                foreach (var item in m_wareCollections) item.Value?.Clear();
                break;
            case 1:
                foreach (var item in m_wareCollections)
                {
                    for (int i = 0; i < list.Length; i++)
                    {
                        var _item = item.Value?.Find(a => a.itemId == list[i].itemId);
                        if (_item != null)
                        {
                            item.Value?.Remove(_item);
                        }
                    }
                }
                break;
        }
    }

    private void GetAllFashionProp()
    {
        m_allProp.Clear();
        m_allFashion.Clear();
        m_allProp = ConfigManager.GetAll<PropItemInfo>();
        for (int i = 0; i < m_allProp.Count; i++)
        {
            if (m_allProp[i] == null || m_allProp[i].itemType != PropType.FashionCloth) continue;
            m_allFashion.Add(m_allProp[i]);
        }
    }

    public List<string> FashionShow(FashionSubType type)
    {
        if (type == FashionSubType.TwoPieceSuit || type == FashionSubType.FourPieceSuit) return null;
        List<string> nameList = new List<string>();
        for (int i = 0; i < m_allFashion.Count; i++)
        {
            PropItemInfo p = m_allFashion[i];
            if (p == null || !p.IsValidVocation(modulePlayer.proto) || p.mesh == null || p.mesh.Length <= 0) continue;
            if (p.itemType != PropType.FashionCloth || (int)type != p.subType) continue;
            nameList.Add(p.mesh[0]);
        }
        return nameList;
    }
    #endregion

    #region Sort

    // 0 降序 1 升序
    public List<PItem> SortQuity(List<PItem> item, int type)
    {
        if (item == null) return null;

        for (int i = 0; i < item.Count; i++)
        {
            for (int j = i + 1; j < item.Count; j++)
            {
                bool exchange = false;
                PropItemInfo a = ConfigManager.Get<PropItemInfo>(item[i].itemTypeId);
                PropItemInfo b = ConfigManager.Get<PropItemInfo>(item[j].itemTypeId);
                if (a == null || b == null) continue;

                if (a.itemType == PropType.Rune && a.itemType == PropType.Rune)
                {
                    if (item[i].growAttr.runeAttr == null || item[j].growAttr.runeAttr == null) continue;
                    if (type == 0 && (item[i].growAttr.runeAttr.star < item[j].growAttr.runeAttr.star)) exchange = true;
                    else if (type == 1 && (item[i].growAttr.runeAttr.star > item[j].growAttr.runeAttr.star)) exchange = true;
                    if (item[i].growAttr.runeAttr.star == item[j].growAttr.runeAttr.star)
                    {
                        if (a.ID > b.ID) exchange = true;
                        else if (a.ID == b.ID)
                        {
                            if (item[i].growAttr.runeAttr.level < item[i].growAttr.runeAttr.level) exchange = true;
                        }
                    }
                }
                else
                {
                    if (type == 0 && (a.quality < b.quality)) exchange = true;
                    else if (type == 1 && (a.quality > b.quality)) exchange = true;
                    if (a.quality == b.quality)
                    {
                        if (item[i].itemTypeId > item[j].itemTypeId) exchange = true;
                        else if (item[i].itemTypeId == item[j].itemTypeId)
                        {
                            if ((a.itemType == PropType.FashionCloth && a.subType == 8) || a.itemType == PropType.Weapon &&
                         (b.itemType == PropType.FashionCloth && b.subType == 8) || b.itemType == PropType.Weapon)
                            {
                                if (item[i].growAttr.equipAttr == null || item[j].growAttr.equipAttr == null) continue;
                                if (item[i].GetIntentyLevel() < item[j].GetIntentyLevel()) exchange = true;
                            }
                        }
                    }
                }
                if (exchange)
                {
                    PItem t = item[i];
                    item[i] = item[j];
                    item[j] = t;
                }
            }
        }
        return item;
    }

    #endregion

    #region hint 

    public bool ShowComposHint()
    {
        //是否有新的可合成是否出现
        for (int i = 0; i < AllCompse.Count; i++)
        {
            var have = CanCompose.Find(a => a.itemId == AllCompse[i].itemId);
            if (have != null) continue;
            else return true;
        }
        return false;
    }

    public void GetCanCompse()
    {
        AllCompse.Clear();
        for (int i = 0; i < m_canCompse.Count; i++)
        {
            PropItemInfo p = m_canCompse[i]?.GetPropItem();
            if (p == null) continue;

            Compound com = ConfigManager.Get<Compound>(p.compose);//合成/分解信息
            if (com == null) continue;

            int comId = com?.items[0]?.itemId ?? 0;
            PropItemInfo prop = ConfigManager.Get<PropItemInfo>(comId);
            if (prop == null) continue;
            if (prop.itemType == PropType.Pet)
            {
                var rlist = modulePet.PetList.Find(a => a.Item.itemTypeId == (ulong)comId);
                if (rlist != null) continue;
            }

            int num = moduleEquip.GetPropCount(com.sourceTypeId);//当前拥有的碎片数量
            if (num >= com.sourceNum)
            {
                AllCompse.Add(m_canCompse[i]);
            }
        }
    }

    public void GetNewProp(PItem item)
    {
        PropItemInfo p = item?.GetPropItem();
        if (p == null || !p.IsValidVocation(modulePlayer.proto)) return;
        
        var have = newProp.Find(a => a.itemId == item.itemId);
        if (have != null) return;
        newProp.Add(item);

        SetLocalNew(item.itemId);
    }

    public void HintShow()
    {
        GetCanCompse();
        moduleHome.UpdateIconState(HomeIcons.Bag, false);
        if (ShowComposHint() || newProp.Count > 0)
        {
            moduleHome.UpdateIconState(HomeIcons.Bag, true);
        }
    }

    public void NewHintShow()
    {
        GetCanCompse();
        if (AllCompse.Count <= 0 && newProp.Count <= 0)  moduleHome.UpdateIconState(HomeIcons.Bag, false);
    }
    #endregion

    #region overrid init

    protected override void OnModuleCreated()
    {
        base.OnModuleCreated();
        InitAllWareInfo();

    }
    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        InitAllWareInfo();
        sortType = 0;
        UseSubType = true;
        newProp.Clear();
        PitemTime.Clear();
        CanCompose.Clear();
        m_detailItem = null;
        FirstShowMain = true;
        m_allItems.Clear();
    }

    #endregion

    #region remove new 

    public bool NewsProp(ulong itemId)
    {
        var have = newProp.Find(a => a.itemId == itemId);
        return have == null ? false : true;
    }

    public void RemveNewItem(FashionSubType type)
    {
        if (type == FashionSubType.None) return;
        List<PItem> equip = FashionShow( type, newProp);
        RemoveItem(equip);
        NewHintShow();
    }

    public void RemveNewItem(WareType type)
    {
        var have = WareShow(type);
        RemoveItem(have);
        NewHintShow();
    }
    public void RemveNewItem( ulong ItemId)
    {
        var have = newProp.Find(a => a.itemId == ItemId);
        if (have == null) return;
        newProp.Remove(have);
        SetLocalNew(ItemId, false);
        NewHintShow();
    }

    public void RemveNewItem(EquipType type)
    {
        List<PItem> equip = EquipShow( type);
        RemoveItem(equip);
        NewHintShow();
    }

    public void RemveNewItem(RuneType type)
    {
        List<PItem> rune = RuneShow(type);
        RemoveItem(rune);
        NewHintShow();
    }
    public bool TypeHint(WareType type)
    {
        List<PItem> ware = WareShow(type);
        if (ware == null) return false;
        else return ware.Count > 0 ? true : false;
    }

    public bool TypeHint(FashionSubType type)
    {
        List<PItem> equip = FashionShow(type, newProp);
        if (equip == null) return false;
        else return equip.Count > 0 ? true : false;
    }

    public bool TypeHint(EquipType type)
    {
        List<PItem> equip = EquipShow(type);
        if (equip == null) return false;
        else return equip.Count > 0 ? true : false;
    }

    public bool TypeHint(RuneType type)
    {
        List<PItem> equip = RuneShow(type);
        if (equip == null) return false;
        else return equip.Count > 0 ? true : false;
    }
    private List<PItem> WareShow(WareType type)
    {
        List<PItem> ware = new List<PItem>();
            for (int i = 0; i < newProp.Count; i++)
        {
            var p = GetWareType(newProp[i]);
            if (type == p) ware.Add(newProp[i]);
        }

        return ware;
    }

    private List<PItem> FashionShow(FashionSubType type, List<PItem> newProp)
    {
        List<PItem> equip = new List<PItem>();
        for (int i = 0; i < newProp.Count; i++)
        {
            PropItemInfo p = newProp[i]?.GetPropItem();
            if (p == null || !p.IsValidVocation(modulePlayer.proto)) continue;
            if (p.itemType != PropType.FashionCloth) continue;
            if ((int)type == p.subType) equip.Add(newProp[i]);
        }

        return equip;
    }

    private List<PItem> EquipShow(EquipType type)
    {
        List<PItem> equip = new List<PItem>();
            for (int i = 0; i < newProp.Count; i++)
            {
                var p = Module_Equip.GetEquipTypeByItem(newProp[i]);
                if (type == p) equip.Add(newProp[i]);
            }
        
        return equip;
    }

    private List<PItem> RuneShow(RuneType type)
    {
        List<PItem> rune = new List<PItem>();
        for (int i = 0; i < newProp.Count; i++)
        {
            PropItemInfo p = newProp[i]?.GetPropItem();
            if (p == null || !p.IsValidVocation(modulePlayer.proto) || p.itemType != PropType.Rune) continue;
            var s = (RuneType)(p.subType);
            if (type == s) rune.Add(newProp[i]);
        }

        return rune;
    }

    private void RemoveItem(List<PItem> r)
    {
        for (int i = 0; i < r.Count; i++)
        {
            var have = newProp.Exists(a => a.itemId == r[i].itemId);
            if (have) newProp.Remove(r[i]);
            SetLocalNew(r[i].itemId, false);

            var have1 = moduleEquip.EquipNewRead.Exists(a => a == r[i].itemId);
            if (have1) moduleEquip.EquipNewRead.Remove(r[i].itemId);
        }
    }

    #endregion

    #region  local data

    private void SetLocalNew(ulong itemId, bool add = true)
    {
        var key = modulePlayer.id_.ToString() + itemId;
        var have = PlayerPrefs.GetString(key);

        if (add && string.IsNullOrEmpty (have))
        {
            PlayerPrefs.SetString(key, "1");
        }
        else if (have != null && !add)
        {
            PlayerPrefs.DeleteKey(key);
        }
    }

    public void GetAllNew()
    {
        moduleCangku.UseSubType = true;

        if (!FirstShowMain) return;

        newProp.Clear();

        for (int i = 0; i < m_allItems.Count; i++)
        {
            if (m_allItems[i] == null) continue;
            var key = modulePlayer.id_.ToString() + m_allItems[i].itemId;
            var have = PlayerPrefs.GetString(key);
            if (string.IsNullOrEmpty(have)) continue;
            var p = GetWareType(m_allItems[i]);
            if (p == WareType.None) continue;
            newProp.Add(m_allItems[i]);
        }
        HintShow();
        FirstShowMain = false;
    }

    #endregion

    #region Packet handlers

    private void SetLossTime(PItem[] items, PItem item = null)
    {
        if (item != null) GetTime(item);

        if (items == null) return;
        for (int i = 0; i < items.Length; i++)
            GetTime(items[i]);
    }

    private void GetTime(PItem item)
    {
        if (item == null || item.timeLimit == -1) return; ;

        var lossTime = Time.realtimeSinceStartup;
        if (PitemTime.ContainsKey(item.itemId))
        {
            PitemTime[item.itemId] = lossTime;
            return; ;
        }
        PitemTime.Add(item.itemId, lossTime);

    }

    void _Packet(ScRoleBagInfo p)
    {
        if (p.itemInfo != null)
        {
            PItem[] items = null;
            p.itemInfo.CopyTo(ref items);

            SetLossTime(items);

            if (m_allItems.Count == 0) m_allItems.AddRange(items);
            else
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] == null) continue;
                    var d = GetItemByGUID(items[i].itemId);
                    if (d == null) m_allItems.Add(items[i]);
                }
            }
        }

        DispatchModuleEvent(EventCangkuInfo);
    }

    void _Packet(ScRoleEquipedItems equipments)
    {
        if (equipments != null)
        {
            PItem[] items = null;
            equipments.itemInfo.CopyTo(ref items);

            SetLossTime(items);
            
            for (int i = 0; i < items.Length; i++)
            {
                var d = GetItemByGUID(items[i].itemId);
                if (d == null) m_allItems.Add(items[i]);
            }
        }
    }
    
    void _Packet(ScRoleAddItem p)
    {
        var i = GetItemByGUID(p.itemId);
        if (i == null)
        {
            i = GetNewPItem(p.itemId, p.itemTypeId, p.num, p.Clone().growAttr);
            if (i != null)
            {
                m_allItems.Add(i);
                moduleCangku.GetNewProp(i);
            }
        }
        else
        {
            i.num += p.num;
            i.itemTypeId = p.itemTypeId;
            p.growAttr?.CopyTo(ref i.growAttr);
        }

        SetLossTime(null, i);
        DispatchModuleEvent(EventCangkuAddItem, i, p.num);
    }

    void _Packet(ScRoleConsumeItem p)
    {
        var i = GetItemByGUID(p.itemId);
        if (i == null || i.itemTypeId != p.itemTypeId) return;
        i.num -= p.num;
        if (i.num < 1)
        {
            RemveNewItem(p.itemId);
            m_allItems.Remove(i);
        }

        DispatchModuleEvent(EventCangkuRemoveItem, i, p.num);
    }

    void _Packet(ScEquipWeaponEvolved p)//武器进化
    {
        var i = GetItemByGUID(p.weaponUId);
        if (i == null || p.result != 0 || i.growAttr.equipAttr == null) return;
        i.growAttr.equipAttr.star++;
    }

    void _Packet(ScEquipWeaponGrowth p)//武器入魂
    {
        var i = GetItemByGUID(p.weaponUId);
        if (i == null || p.result != 0 || i.growAttr.equipAttr == null) return;
        i.growAttr.equipAttr.level = p.level;
        i.growAttr.equipAttr.expr = p.curLvExpr;
    }

    void _Packet(ScRoleItemInfo p)//灵珀强化 升星 武器升级 进阶  器灵 升华
    {
        if (p.item == null) return;
        var i = GetItemByGUID(p.item.itemId);
        if (i == null) return;
        p.item.CopyTo(i);
        moduleEquip.ChangeItemAttr(i);
    }
    #endregion

    public PropItemInfo GetAllPropItem(ushort _itemTypeId)
    {
        var info = ConfigManager.Get<PropItemInfo>(_itemTypeId);
        if (_itemTypeId == 14) info = PropItemInfo.activepoint;
        else if (_itemTypeId == 13) info = PropItemInfo.exp;

        return info;
    }
}