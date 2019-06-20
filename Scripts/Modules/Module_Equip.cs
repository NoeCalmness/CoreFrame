/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-05-23
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using URandom = UnityEngine.Random;
using UnityEngine;

public class Module_Equip : Module<Module_Equip>
{
    #region const field

    public const int SIGNET_PROP_ID = 3;
    public const int ADVENTURE_PROP_ID = 4;
    public const int EVOLVE_LEVEL = 10;

    public const string EventRefreshCangku = "EventRefreshCangku";
    public const string EventUpdateBagProp = "EventUpdateBagProp";
    public const string EventUseWantedPropSuccess = "EventUseWantedPropSuccess";
    public const string EvenSynthesisSucced = "EvenSynthesisSucced";//武器合成成功
    public const string EventBagInfo = "EventBagInfo";//背包数据准备完成
    public const string EventRefreshCurrentDress = "EventRefreshCurrentDress";//换装刷新
    public const string EventItemDataChange = "EventItemDataChange";//任意背包数据有更新
    public const string EventOpenSubPanel = "EventOpenSubPanel";//打开指定的子界面
    public const string EventCameraView = "EventCameraView";//开启摄像机镜头
    public const string EventOperateSuccess = "EventOperateSuccess";//装备操作成功的回调(强化和进阶)
    public const string EventCameraPause = "EventCameraPause";//停止相机和任务动画

    public static Dictionary<PropType, List<int>> HIDE_PROPS_FLAG = new Dictionary<PropType, List<int>>
    {
        //结构：{需要取消在背包显示的道具类型 ,需要在背包中取消显示的子类型，如果添加或者添加为-1则表示不进行指定的子类型检测},
        { PropType.Currency,new List<int>{(int)CurrencySubType.Gold, (int)CurrencySubType.Diamond , (int)CurrencySubType.HeartSoul }},
        { PropType.LabyrinthProp,new List<int>()},
        { PropType.HeadAvatar,new List<int>()},
        { PropType.BordGrade,new List<int>() },
        { PropType.AwakeCurrency, new List<int>() },
        { PropType.SkillPoint, new List<int>() },
        { PropType.Rune, new List<int>() }
    };

    #endregion

    #region static function

    /// <summary>
    /// 根据性别和职业随机装备
    /// </summary>
    /// <param name="gender"></param>
    /// <returns></returns>
    public static int[] SelectRandomEquipments(int gender, int proto)
    {
        var cs = ConfigManager.FindAll<PropItemInfo>(i => (i.sex == 2 || i.sex == gender) && i.IsValidVocation(proto) && i.itemType == PropType.FashionCloth);
        var hs = cs.FindAll(i => i.subType == (int)FashionSubType.Hair);
        var ss = cs.FindAll(i => i.subType == (int)FashionSubType.FourPieceSuit);
        var hd = cs.FindAll(i => i.subType == (int)FashionSubType.HeadDress);
        var hr = cs.FindAll(i => i.subType == (int)FashionSubType.HairDress);
        var fd = cs.FindAll(i => i.subType == (int)FashionSubType.FaceDress);
        var nd = cs.FindAll(i => i.subType == (int)FashionSubType.NeckDress);

        var es = new int[] { 0, 0, 0, 0, 0, 0 };
        
        es[0] = hs.Count < 1 ? 0 : hs[URandom.Range(0, hs.Count)].ID;
        es[2] = ss.Count < 1 ? 0 : ss[URandom.Range(0, ss.Count)].ID;

        var ramdom = URandom.Range(0, 2);
        es[1] = ramdom == 0 ? 0 : hd.Count < 1 ? 0 : hd[URandom.Range(0, hd.Count)].ID;
        ramdom = URandom.Range(0, 2);
        es[3] = ramdom == 0 ? 0 : hr.Count < 1 ? 0 : hr[URandom.Range(0, hr.Count)].ID;
        ramdom = URandom.Range(0, 2);
        es[4] = ramdom == 0 ? 0 : fd.Count < 1 ? 0 : fd[URandom.Range(0, fd.Count)].ID;
        ramdom = URandom.Range(0, 2);
        es[5] = ramdom == 0 ? 0 : nd.Count < 1 ? 0 : nd[URandom.Range(0, nd.Count)].ID;
        
        return es;
    }

    /// <summary>
    /// 根据性别和职业随机装备
    /// </summary>
    /// <param name="gender"></param>
    /// <returns></returns>
    public static List<PItem> SelectRandomPEquipments(int gender, int proto)
    {
        var es = SelectRandomEquipments(gender, proto);
        var el = new List<PItem>();
        for (var ii = 0; ii < es.Length; ++ii)
        {
            var pi = PacketObject.Create<PItem>();
            pi.itemId = (ulong)ii;
            pi.itemTypeId = (ushort)es[ii];
            pi.num = 1;
            pi.timeLimit = -1;
        }

        return el;
    }

    /// <summary>
    /// 从列表中获取指定类型和子类型的道具
    /// </summary>
    /// <param name="list"></param>
    /// <param name="type"></param>
    /// <param name="subType">道具子类型，如果配置为-1，表示没限制</param>
    /// <returns></returns>
    public static List<PItem> GetPropItemsFromList(List<PItem> list, PropType type, int subType = -1)
    {
        return list?.FindAll(o =>
        {
            var info = o.GetPropItem();
            if (!info) return false;
            return info.itemType == type && (subType == -1 || info.subType == subType);
        });
    }

    /// <summary>
    /// 从列表中获取指定类型并且过滤指定子类型的道具列表
    /// </summary>
    /// <param name="list"></param>
    /// <param name="type"></param>
    /// <param name="subType">道具子类型，如果配置为-1，表示没限制</param>
    /// <returns></returns>
    public static List<PItem> GetPropItemsFromListExcept(List<PItem> list, PropType type, int subType)
    {
        if (subType == -1) return GetPropItemsFromList(list, type);
        return list?.FindAll(o =>
        {
            var info = o.GetPropItem();
            if (!info) return false;
            return info.itemType == type && info.subType != subType;
        });
    }

    /// <summary>
    /// 从指定列表中获取指定道具的总数量
    /// </summary>
    /// <param name="list"></param>
    /// <param name="propId"></param>
    /// <returns></returns>
    public static int GetPropNumber(List<PItem> list, int propId)
    {
        var number = 0;
        var l = list?.FindAll(o => o.itemTypeId == propId);
        if (l != null && l.Count > 0)
        {
            foreach (var item in l) number += (int)item.num;
        }
        return number;
    }

    /// <summary>
    /// 分发打开子界面的事件
    /// </summary>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    /// <param name="param4"></param>
    public static void OpenEquipSubWindow(EnumSubEquipWindowType type, object param2 = null, object param3 = null, object param4 = null)
    {
        selectEquipType = type;
        if (param2 == null) selectEquipSubType = EquipType.None;
        else selectEquipSubType = (EquipType)param2;
        instance.DispatchModuleEvent(EventOpenSubPanel, type, param2, param3, param4);
    }

    /// <summary>
    /// 设置道具镜头显示 curItemTypeId:当前的武器ID:为0时,返回默认位置,gotoItemTypeId:跳转的武器ID
    /// </summary>
    public static void SetCameraView(ushort curItemTypeId = 0, ushort gotoItemTypeId = 0)
    {
        instance.DispatchModuleEvent(EventCameraView, curItemTypeId, gotoItemTypeId);
    }

    public static void PauseCameraAnimation()
    {
        instance.DispatchModuleEvent(EventCameraPause);
    }

    /// <summary>
    /// 当前选中的装备界面类型
    /// </summary>
    public static EnumSubEquipWindowType selectEquipType;
    /// <summary>
    /// 当前选中的装备界面子类型
    /// </summary>
    public static EquipType selectEquipSubType;

    public static int accesssReturnWay;
    #endregion

    #region field/properties

    #region weapon / off weapon
    /// <summary>
    /// 主武器
    /// </summary>
    public PItem weapon
    {
        get
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            return m_lockedWeapon ?? m_weapon;
#else
            return m_weapon;
#endif
        }
    }
    private PItem m_weapon;
    /// <summary>
    /// 主武器 ID
    /// </summary>
    public int weaponID
    {
        get
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            return m_lockedWeaponID < 1 ? m_weaponID : m_lockedWeaponID;
#else
            return m_weaponID;
#endif
        }
    }
    private int m_weaponID;
    /// <summary>
    /// 主武器道具 ID
    /// </summary>
    public int weaponItemID { get { return weapon != null ? weapon.itemTypeId : 0; } }
    /// <summary>
    /// 副武器
    /// </summary>
    public PItem offWeapon { get; private set; }
    /// <summary>
    /// 副武器 ID
    /// </summary>
    public int offWeaponID { get; private set; }
    /// <summary>
    /// 副武器道具 ID
    /// </summary>
    public int offWeaponItemID { get { return offWeapon != null ? offWeapon.itemTypeId : 0; } }
    #endregion

    #region bag collection

    /// <summary>
    /// 当前穿戴的时装
    /// </summary>
    public List<PItem> currentDressClothes
    {
        get
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            return modulePlayer.lockedGenderDifference || modulePlayer.lockedClassDifference ? SelectRandomPEquipments(modulePlayer.gender, modulePlayer.proto) : m_bagCollections?.Get(BagCollectionType.CurrentDress);
#else
            return m_bagCollections?.Get(BagCollectionType.CurrentDress);
#endif
        }
    }

    /// <summary>
    /// 当前背包的时装和武器
    /// </summary>
    public List<PItem> currentBagClothes { get { return m_bagCollections?.Get(BagCollectionType.BagDress); } }

    /// <summary>
    /// 当前背包的道具
    /// </summary>
    public List<PItem> currentBagProp { get { return m_bagCollections?.Get(BagCollectionType.BagProps); } }

    /// <summary>
    /// 当前不显示在背包中的道具
    /// </summary>
    public List<PItem> currentHideProp { get { return m_bagCollections?.Get(BagCollectionType.HideProps); } }

    /// <summary>
    /// 所有背包数据的基本信息集合
    /// </summary>
    public Dictionary<BagCollectionType, List<PItem>> m_bagCollections { get; set; } = new Dictionary<BagCollectionType, List<PItem>>();
    #endregion

    /// <summary>
    /// 缓存需要替换的服装
    /// </summary>
    private List<PItem> tempChangeCloths = new List<PItem>();

    /// <summary>
    /// 缓存需要脱掉的服装
    /// </summary>
    private List<PItem> tempDownCloths = new List<PItem>();

    /// <summary>
    /// 强化信息汇总
    /// </summary>
    private Dictionary<EquipType, IntentifyDetail> m_intentDetailDic = new Dictionary<EquipType, IntentifyDetail>();

    /// <summary>
    /// 进阶信息汇总
    /// </summary>
    private Dictionary<EquipType, EvolveDetail> m_evolveDetailDic = new Dictionary<EquipType, EvolveDetail>();
    private List<PItem> m_countDownItems { get; set; } = new List<PItem>();

    /// <summary>
    /// 徽记数量
    /// </summary>
    public int signetCount { get { return GetPropCount(SIGNET_PROP_ID); } }

    /// <summary>
    /// 冒险币数量
    /// </summary>
    public int adventureCount { get { return GetPropCount(ADVENTURE_PROP_ID); } }

    /// <summary>
    /// 可以合成武器的碎片列表
    /// </summary>
    //public List<PItem> compoundWeaponList { get; set; } = new List<PItem>();
    public EnumOperateEquipType lastOperateType { get; set; } = EnumOperateEquipType.None;
    /// <summary>
    /// 上一次强化道具的缓存
    /// </summary>
    public PItem lastIntentyCacheItem;

    public uint bloodCardNum
    {
        get
        {
            var item = currentBagProp.Find(o =>
            {
                var prop = o.GetPropItem();
                if (!prop) return false;
                return prop.itemType == PropType.UseableProp && prop.subType == (int)UseablePropSubType.WantedWithBlood;
            });
            return item == null ? 0 : item.num;
        }
    }


    /// <summary>
    /// 当前需要直接打开界面操作的item
    /// 包含强化/进阶/附魔 
    /// </summary>
    public PItem operateItem { get; set; } = null;

    /// <summary>
    /// 上一次换装的道具id
    /// </summary>
    public ushort lastItemTypeId { get; set; }
    #endregion

    #region bag collection

    private void InitAllBagCollection()
    {
        for (int i = (int)BagCollectionType.Default, count = (int)BagCollectionType.Count; i < count; i++)
        {
            if (m_bagCollections.ContainsKey((BagCollectionType)i))
            {
                m_bagCollections[(BagCollectionType)i].Clear();
                continue;
            }

            m_bagCollections.Add((BagCollectionType)i, new List<PItem>());
        }
    }

    /// <summary>
    /// 初始化容器数据
    /// </summary>
    private void ClearSelectCollection(RequestItemType type, PItem[] get = null)
    {
        switch (type)
        {
            case RequestItemType.All:
                foreach (var item in m_bagCollections) item.Value?.Clear();
                break;
            case RequestItemType.DressedClothes:
                currentDressClothes?.Clear();
                RemoveRune(get);
                break;
            case RequestItemType.InBag:
                foreach (var item in m_bagCollections)
                {
                    if (item.Key == BagCollectionType.CurrentDress || item.Key == BagCollectionType.RuneProps) continue;
                    item.Value?.Clear();
                }
                RemoveRune(get);
                break;
        }
    }

    private void RemoveRune(PItem[] items)
    {
        if (items == null) return;
        for (int i = 0; i < items.Length; i++)
        {
            var info = items[i]?.GetPropItem();
            if (info == null || info.itemType != PropType.Rune) continue;

            var _item = m_bagCollections[BagCollectionType.RuneProps].Find(a => a.itemId == items[i].itemId);
            if (_item != null) m_bagCollections[BagCollectionType.RuneProps].Remove(_item);
        }
    }

    private void AddToBagCollection(PItem[] items, bool dressed, bool isNew = false)
    {
        foreach (var item in items) AddItemToBagCollection(item, dressed, false, isNew);

        //sort all
        SortBagCollections();
    }

    private void SortBagCollections()
    {
        if (m_bagCollections.Count < 1) return;
        foreach (var item in m_bagCollections)
            item.Value.Sort((a, b) => a.itemTypeId.CompareTo(b.itemTypeId));
    }

    private void AddItemToBagCollection(PItem item, bool dressed, bool checkContain = false, bool isNew = false)
    {
        var info = item?.GetPropItem();
        if (!info) return;

        sbyte roleGender = (sbyte)modulePlayer.gender;
        if (item == null || ((info.itemType == PropType.FashionCloth && info.sex != (sbyte)GenderRole.All && info.sex != roleGender) && !info.IsValidVocation(modulePlayer.proto))) return;

        if (info.itemType == PropType.Pet)
        {
            modulePet.AddPet(item, isNew);
            return;
        }
        BagCollectionType t = GetBagCollectionTypeFromPItem(item, dressed);

        if (!m_bagCollections.ContainsKey(t) || m_bagCollections[t] == null)
        {
            Logger.LogError("item id = {0}, find type = {1} cannot be finded in bag collection", item.itemTypeId, t);
            return;
        }

        if (!checkContain)
        {
            var i = m_bagCollections[t].Find(a => a.itemId == item.itemId);
            if (i == null) m_bagCollections[t].Add(item);
        }
        else
        {
            List<PItem> list = m_bagCollections[t];
            PItem alreadyPItem = list.Find(o => o.itemId == item.itemId);
            if (alreadyPItem == null) list.Add(item);
            else alreadyPItem.num += item.num;
        }
    }

    public BagCollectionType GetBagCollectionTypeFromPItem(PItem item, bool dressed)
    {
        if (item == null) return BagCollectionType.Default;
        var info = item?.GetPropItem();
        if (!info) return BagCollectionType.Default;

        if (info.itemType == PropType.Rune) return BagCollectionType.RuneProps;

        if (info.itemType == PropType.Weapon || info.itemType == PropType.FashionCloth) return dressed ? BagCollectionType.CurrentDress : BagCollectionType.BagDress;
        else if (HIDE_PROPS_FLAG.ContainsKey(info.itemType))
        {
            List<int> tarList = HIDE_PROPS_FLAG.Get(info.itemType);
            if (tarList == null || tarList.Count == 0 || tarList[0] < 0) return BagCollectionType.HideProps;
            else return tarList.Contains((int)info.subType) ? BagCollectionType.HideProps : BagCollectionType.BagProps;
        }
        else return BagCollectionType.BagProps;
    }

    public BagCollectionType GetBagCollectionTypeFromID(int propId, bool dressed)
    {
        PropItemInfo prop = ConfigManager.Get<PropItemInfo>(propId);
        return GetBagCollectionTypeFromProp(prop, dressed);
    }

    public BagCollectionType GetBagCollectionTypeFromProp(PropItemInfo prop, bool dressed)
    {
        if (prop == null) return BagCollectionType.Default;

        var type = prop.itemType;
        if (type == PropType.Rune) return BagCollectionType.RuneProps;
        else if (type == PropType.Weapon || type == PropType.FashionCloth) return dressed ? BagCollectionType.CurrentDress : BagCollectionType.BagDress;
        else if (HIDE_PROPS_FLAG.ContainsKey(type))
        {
            List<int> tarList = HIDE_PROPS_FLAG.Get(type);
            if (tarList == null || tarList.Count == 0 || tarList[0] < 0) return BagCollectionType.HideProps;
            else return tarList.Contains(prop.subType) ? BagCollectionType.HideProps : BagCollectionType.BagProps;
        }
        else return BagCollectionType.BagProps;
    }

    #endregion

    #region public functions

    #region 从数据集中获取Pitem

    /// <summary>
    /// 获取指定的所有道具（从背包穿戴，当前穿戴和当前仓库中去获取）
    /// </summary>
    /// <param name="type"></param>
    /// <param name="subType">如果传入 -1 表示无限制</param>
    /// <returns></returns>
    public List<PItem> GetPropItems(PropType type, int subType = -1)
    {
        List<PItem> list = new List<PItem>();
        List<PItem> temp;
        foreach (var item in m_bagCollections)
        {
            temp = item.Value.FindAll(o =>
            {
                var info = o.GetPropItem();
                if (!info) return false;
                return info.itemType == type && (subType == -1 || info.subType == subType);
            });
            if (temp != null && temp.Count > 0) list.AddRange(temp);
        }
        return list;
    }

    public List<PItem> GetPropItems(Predicate<PItem> match)
    {
        List<PItem> list = new List<PItem>();
        foreach (var kv in m_bagCollections)
        {
            foreach (var item in kv.Value)
                if (match(item))
                    list.Add(item);
        }
        return list;
    }

    /// <summary>
    /// 获取指定的type类型道具，并且过滤子类型为subtype的道具
    /// </summary>
    /// <param name="type"></param>
    /// <param name="subType">如果传入 -1 表示不进行过滤</param>
    /// <returns></returns>
    public List<PItem> GetPropsItemsExcept(PropType type, int subType = -1)
    {
        if (subType == -1) return GetPropItems(type);
        else
        {
            List<PItem> list = new List<PItem>();
            List<PItem> temp;
            foreach (var item in m_bagCollections)
            {
                if (item.Value == null || item.Value.Count == 0) continue;

                temp = item.Value.FindAll(o =>
                {
                    var info = o.GetPropItem();
                    if (!info) return false;
                    return info.itemType == type && info.subType != subType;
                });
                if (temp != null && temp.Count > 0) list.AddRange(temp);
            }
            return list;
        }

    }

    /// <summary>
    /// 获取指定背包中的道具
    /// </summary>
    /// <param name="type"></param>
    /// <param name="subType"></param>
    /// <param name="includeHide">是否包含不显示在背包中的道具</param>
    /// <returns></returns>
    public List<PItem> GetBagProps(PropType type, int subType = -1, bool includeHide = false)
    {
        if (type == PropType.Weapon || type == PropType.FashionCloth) Logger.LogWarning("please called func with name [GetTargetBagDress] Instead of [GetTargetBagProps]");
        List<PItem> list = new List<PItem>();
        list.AddRange(GetPropItemsFromList(currentBagProp, type, subType));
        if (includeHide) list.AddRange(GetPropItemsFromList(currentHideProp, type, subType));
        return list;
    }

    /// <summary>
    ///  获取指定背包道具
    /// </summary>
    /// <param name="type"></param>
    /// <param name="subType"></param>
    /// <param name="includeHide">是否包含不显示在背包中的道具</param>
    /// <returns></returns>
    public List<PItem> GetBagPropsExcept(PropType type, int subType, bool includeHide = false)
    {
        if (type == PropType.Weapon || type == PropType.FashionCloth) Logger.LogWarning("please called func with name [GetTargetBagDressExcept] Instead of [GetTargetBagPropsExcept]");
        List<PItem> list = new List<PItem>();
        list.AddRange(GetPropItemsFromListExcept(currentBagProp, type, subType));
        if (includeHide) list.AddRange(GetPropItemsFromListExcept(currentHideProp, type, subType));
        return list;
    }

    /// <summary>
    /// 获取指定背包穿戴的装备
    /// </summary>
    /// <param name="type">指定的装备主类型</param>
    /// <param name="subType">-1 表示不进行子类型检索</param>
    /// <returns></returns>
    public List<PItem> GetBagDress(PropType type, int subType = -1)
    {
        if (type != PropType.Weapon && type != PropType.FashionCloth) Logger.LogWarning("please called func with name [GetTargetBagProps] Instead of [GetTargetBagDress]");
        return GetPropItemsFromList(currentBagClothes, type, subType);
    }

    /// <summary>
    /// 获取指定背包穿戴的装备
    /// </summary>
    /// <param name="type">指定的装备主类型,并且过滤指定的subtype</param>
    /// <param name="subType">-1 表示不进行子类型检索</param>
    /// <returns></returns>
    public List<PItem> GetBagDressExcept(PropType type, int subType)
    {
        if (type != PropType.Weapon && type != PropType.FashionCloth) Logger.LogWarning("please called func with name [GetTargetBagPropsExcept] Instead of [GetTargetBagDressExcept]");
        return GetPropItemsFromListExcept(currentBagClothes, type, subType);
    }

    public List<PItem> GetBagDress(EquipType type)
    {
        switch (type)
        {
            case EquipType.Weapon: return GetBagDressExcept(PropType.Weapon, (int)WeaponSubType.Gun);
            case EquipType.Gun: return GetBagDress(PropType.Weapon, (int)WeaponSubType.Gun);
            case EquipType.Cloth: return GetBagDress(PropType.FashionCloth, (int)FashionSubType.FourPieceSuit);
            case EquipType.Hair: return GetBagDress(PropType.FashionCloth, (int)FashionSubType.Hair);
            case EquipType.HeadDress: return GetBagDress(PropType.FashionCloth, (int)FashionSubType.HeadDress);
            case EquipType.HairDress: return GetBagDress(PropType.FashionCloth, (int)FashionSubType.HairDress);
            case EquipType.FaceDress: return GetBagDress(PropType.FashionCloth, (int)FashionSubType.FaceDress);
            case EquipType.NeckDress: return GetBagDress(PropType.FashionCloth, (int)FashionSubType.NeckDress);
            default: return new List<PItem>();
        }
    }

    /// <summary>
    /// 通过道具ID获取数据
    /// </summary>
    /// <param name="propId"></param>
    /// <returns></returns>
    public PItem GetProp(int propId)
    {
        PItem pitem;
        foreach (var item in m_bagCollections)
        {
            pitem = item.Value.Find(o => o.itemTypeId == propId);
            if (pitem != null) return pitem;
        }
        return null;
    }

    /// <summary>
    /// 通过道具uid获取道具
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public PItem GetProp(ulong itemId)
    {
        PItem pitem;
        foreach (var item in m_bagCollections)
        {
            pitem = item.Value.Find(o => o.itemId == itemId);
            if (pitem != null) return pitem;
        }
        return null;
    }

    /// <summary>
    /// 通过道具ID获取数据
    /// </summary>
    /// <param name="propId"></param>
    /// <returns></returns>
    public PItem GetPropFromBag(int propId)
    {
        return currentBagClothes?.Find(o => o.itemTypeId == propId);
    }

    /// <summary>
    /// 通过道具uid获取道具
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public PItem GetPropFromBag(ulong itemId)
    {
        return currentBagClothes?.Find(o => o.itemId == itemId);
    }

    /// <summary>
    /// 通过道具ID获取数据
    /// </summary>
    /// <param name="propId"></param>
    /// <returns></returns>
    public PItem GetPropFromHide(int propId)
    {
        return currentHideProp?.Find(o => o.itemTypeId == propId);
    }

    /// <summary>
    /// 通过道具uid获取道具
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public PItem GetPropFromHide(ulong itemId)
    {
        return currentHideProp?.Find(o => o.itemId == itemId);
    }

    /// <summary>
    /// 获取指定穿戴道具
    /// </summary>
    /// <param name="type">指定的道具主类型</param>
    /// <param name="subType">指定的道具子类型</param>
    /// <returns></returns>
    public PItem GetDressedProp(PropType type, int subType)
    {
        if (subType == -1)
        {
            Logger.LogError("call GetTargetDressProp has invalid subtype......type is {0} ,subtype is {1}", type, subType);
            return null;
        }
        return currentDressClothes?.Find(o =>
        {
            var info = o.GetPropItem();
            if (!info) return false;
            return info.itemType == type && info.subType == subType;
        });
    }

    /// <summary>
    /// 获取当前装备的武器，过滤指定的子类型
    /// </summary>
    /// <param name="type"></param>
    /// <param name="subType"></param>
    /// <returns></returns>
    public PItem GetDressedPropExcept(PropType type, int subType)
    {
        if (subType == -1)
        {
            Logger.LogError("call GetTargetDressPropExcept has invalid subtype......type is {0} ,subtype is {1}", type, subType);
            return null;
        }
        return currentDressClothes?.Find(o =>
        {
            var info = o.GetPropItem();
            if (!info) return false;
            return info.itemType == type && info.subType != subType;
        });
    }

    public PItem GetDressedProp(EquipType type)
    {
        switch (type)
        {
            case EquipType.Weapon: return GetDressedPropExcept(PropType.Weapon, (int)WeaponSubType.Gun);
            case EquipType.Gun: return GetDressedProp(PropType.Weapon, (int)WeaponSubType.Gun);
            case EquipType.Cloth: return GetDressedProp(PropType.FashionCloth, (int)FashionSubType.FourPieceSuit);
            case EquipType.Hair: return GetDressedProp(PropType.FashionCloth, (int)FashionSubType.Hair);
            case EquipType.HeadDress: return GetDressedProp(PropType.FashionCloth, (int)FashionSubType.HeadDress);
            case EquipType.HairDress: return GetDressedProp(PropType.FashionCloth, (int)FashionSubType.HairDress);
            case EquipType.FaceDress: return GetDressedProp(PropType.FashionCloth, (int)FashionSubType.FaceDress);
            case EquipType.NeckDress: return GetDressedProp(PropType.FashionCloth, (int)FashionSubType.NeckDress);
            default: return null;
        }
    }

    #endregion

    #region 从数据集中删除Pitem

    private void RemoveProp(ulong itemId, int _num)
    {
        bool removeComplete = false;
        foreach (var item in m_bagCollections)
        {
            List<PItem> list = item.Value;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].itemId == itemId)
                {
                    removeComplete = true;

                    //to protcted ushort stackNum
                    if (_num > list[i].num) list[i].num = 0;
                    else list[i].num -= (ushort)_num;

                    //remove item
                    if (list[i].num == 0) list.RemoveAt(i);

                    break;
                }
            }

            if (removeComplete) break;
        }
    }

    #endregion

    #region hasitem or has dressed

    /// <summary>
    /// 根据道具ID来判断是否拥有该物体
    /// </summary>
    /// <param name="itemTypeId"></param>
    /// <returns></returns>
    public bool HasProp(int itemTypeId)
    {
        BagCollectionType dressT = GetBagCollectionTypeFromID(itemTypeId, true);
        BagCollectionType undressT = GetBagCollectionTypeFromID(itemTypeId, false);

        if (dressT == undressT)
        {
            List<PItem> list = m_bagCollections.Get(dressT);
            return list?.Find(o => o.itemTypeId == itemTypeId) != null;
        }
        else
        {
            List<PItem> list = m_bagCollections.Get(dressT);
            PItem item = list?.Find(o => o.itemTypeId == itemTypeId);
            if (item != null) return true;

            list = m_bagCollections.Get(undressT);
            item = list?.Find(o => o.itemTypeId == itemTypeId);
            return item != null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="subType"></param>
    /// <returns></returns>
    public bool HasDressed(PropType type, int subType)
    {
        switch (type)
        {
            case PropType.Weapon:
                if (subType == (int)WeaponSubType.Gun) return GetDressedProp(PropType.Weapon, (int)WeaponSubType.Gun) != null;
                else return GetDressedPropExcept(PropType.Weapon, (int)WeaponSubType.Gun) != null;
            case PropType.FashionCloth: return GetDressedProp(type, subType) != null;
            default: return false;
        }
    }

    public bool HasDressed(EquipType type)
    {
        return GetDressedProp(type) != null;
    }

    #endregion

    #region other logic

    /// <summary>
    /// 获取背包指定道具的数量
    /// </summary>
    /// <param name="propId"></param>
    /// <returns></returns>
    public int GetPropCount(int propId)
    {
        return GetPropNumber(currentBagProp, propId) + GetPropNumber(currentHideProp, propId);
    }

    //根据pitem 获得显示当前状态
    public PreviewEquipType GetPeiviewEuipeTypeByItem(PItem item)
    {
        var type = GetEquipTypeByItem(item);
        return GetCurrentPreType(type, item.GetIntentyLevel(), item.HasEvolved());
    }

    public static EquipType GetEquipTypeByItem(PItem item)
    {
        return GetEquipTypeByID(item == null ? ushort.MinValue : item.itemTypeId);
    }

    public static EquipType GetEquipTypeByID(ushort _itemTypeId)
    {
        var info = ConfigManager.Get<PropItemInfo>(_itemTypeId);
        return GetEquipTypeByInfo(info);
    }

    public static EquipType GetEquipTypeByInfo(PropItemInfo info)
    {
        if (info == null) return EquipType.None;

        if (info.itemType == PropType.Weapon)
        {
            if (info.subType == (byte)WeaponSubType.Gun) return EquipType.Gun;
            else return EquipType.Weapon;
        }
        else if (info.itemType == PropType.FashionCloth)
        {
            if (info.subType == (byte)FashionSubType.HeadDress) return EquipType.HeadDress;
            else if (info.subType == (byte)FashionSubType.HairDress) return EquipType.HairDress;
            else if (info.subType == (byte)FashionSubType.FaceDress) return EquipType.FaceDress;
            else if (info.subType == (byte)FashionSubType.NeckDress) return EquipType.NeckDress;
            else if (info.subType == (byte)FashionSubType.Hair) return EquipType.Hair;
            else if (info.subType == (byte)FashionSubType.ClothGuise) return EquipType.Guise;
            else return EquipType.Cloth;
        }
        return EquipType.None;
    }

    public PreviewEquipType GetCurrentPreType(EquipType type, int level, bool hasEvo)
    {
        if (level == 0) return PreviewEquipType.Intentify;

        //获得最大进阶等级
        int maxEvoLevel = GetMaxEvoLevel(type);
        int maxIntenLevel = GetMaxIntenLevel(type);
        int evolevel = level / EVOLVE_LEVEL;

        if (level == 0) return PreviewEquipType.Intentify;
        else if (level % EVOLVE_LEVEL == 0)
        {
            if (level >= maxIntenLevel && evolevel >= maxEvoLevel) return PreviewEquipType.Enchant;
            else if (!hasEvo && evolevel <= maxEvoLevel) return PreviewEquipType.Evolve;
            else return PreviewEquipType.Intentify;
        }
        else return PreviewEquipType.Intentify;
    }
    #endregion

    #endregion

    #region weapon compose / forge / msg

    public ushort Compose_ID { get; set; }//要合成的道具ID

    public int ChangeToItemTypeID(ulong itemID)
    {
        PItem item = GetProp(itemID);
        return item == null ? 0 : item.itemTypeId;
    }

    public ulong ChangeToItemID(int itemTypeId)
    {
        PItem item = GetProp(itemTypeId);
        return item == null ? 0 : item.itemId;
    }

    /// <summary>
    /// 获取灵石<ID,NUM>字典
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, int> GetElementNumDic()
    {
        Dictionary<int, int> dic = new Dictionary<int, int>();
        List<PItem> elements = GetBagProps(PropType.ElementType);
        foreach (var item in elements)
        {
            if (!dic.ContainsKey(item.itemTypeId)) dic.Add(item.itemTypeId, 0);
            dic[item.itemTypeId] += (int)item.num;
        }
        return dic;
    }

    #endregion

    #region global compse
    public void SendComposeAnyOne(int number)
    {
        CsRoleItemCompose p = PacketObject.Create<CsRoleItemCompose>();
        p.composeId = Compose_ID;
        p.num = (ushort)number;
        session.Send(p);
    }

    void _Packet(ScRoleItemCompose p)
    {
        if (p.result == 0)
        {
            //AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
            DispatchModuleEvent(EvenSynthesisSucced, p);
        }
        else if (p.result == 1) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(216, 61));
        else if (p.result == 2) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(216, 62));
        else if (p.result == 3) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(216, 63));
        else moduleGlobal.ShowMessage(ConfigText.GetDefalutString(216, 48));
    }

    #endregion

    #region prop msg
    /// <summary>
    /// 发送道具请求
    /// </summary>
    /// <param name="type">需要请求的不同类型（背包/已经穿上的）</param>
    public void SendRequestProp(RequestItemType type)
    {
        CsRoleBagInfo p = PacketObject.Create<CsRoleBagInfo>();
        p.itemType = (sbyte)type;
        session.Send(p);
    }

    void _Packet(ScRoleBagInfo bagClothes)
    {
        PItem[] items = null;
        bagClothes.itemInfo.CopyTo(ref items);

        ClearSelectCollection(RequestItemType.InBag, items);
        AddToBagCollection(items, false);

        moduleCangku.ClearSelectCollection(1, items);
        moduleCangku.AddToWareCollection(items);
        moduleCangku.HintShow();

        DispatchModuleEvent(EventBagInfo);
    }

    public void GMCreateEquip(ScRoleEquipedItems equipments)
    {
#if UNITY_EDITOR
        _Packet(equipments);
#endif
    }

    void _Packet(ScRoleEquipedItems equipments)
    {
        if (equipments != null)
        {
            PItem[] items = null;
            equipments.itemInfo.CopyTo(ref items);
            ClearSelectCollection(RequestItemType.DressedClothes);
            AddToBagCollection(items, true);

            moduleCangku.ClearSelectCollection(1, items);
            moduleCangku.AddToWareCollection(items);
            moduleCangku.HintShow();

            UpdateWeapons();
        }
    }

    void _Packet(ScRoleAddItem p)
    {
        PropItemInfo info = ConfigManager.Get<PropItemInfo>(p.itemTypeId);
        if (info == null) return;

        PItem pitem = moduleCangku.GetNewPItem(p.itemId, p.itemTypeId, p.num, p.Clone().growAttr);

        if (info.itemType == PropType.Sundries && info.subType == (byte)SundriesSubType.HelpFighting)
        {
            List<PItem> items = new List<PItem>();
            items.Add(pitem);
            moduleGlobal.Showawardmes(items);
        }

        AddItemToBagCollection(pitem, false, true, true);
        SortBagCollections();
        moduleCangku.AddItemToWareCollection(pitem);
        moduleCangku.HintShow();

        DispatchModuleEvent(EventUpdateBagProp);
        DispatchModuleEvent(EventRefreshCangku, true);

    }

    private bool IsNewAddItemToCangku(PItem item)
    {
        PropItemInfo info = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
        if (info == null) return false;

        if (info.itemType == PropType.Weapon || info.itemType == PropType.FashionCloth || info.itemType == PropType.Rune) return false;

        if (HIDE_PROPS_FLAG.ContainsKey(info.itemType))
        {
            var targetList = HIDE_PROPS_FLAG.Get(info.itemType);
            if (targetList != null && targetList.Count > 0)
                return targetList.Contains(info.subType) ? false : true;
        }

        return true;
    }

    private bool IsNewAddItemToEquip(PItem item)
    {
        PropItemInfo info = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
        if (info == null) return false;

        if (info.itemType == PropType.Weapon || info.itemType == PropType.FashionCloth)
        {
            //如果已经在背包界面，则不显示状态了
            var w = Window.GetOpenedWindow<Window_Equip>();
            return !(w && w.actived);
        }

        return false;
    }

    //玩家道具消耗，推送消息
    void _Packet(ScRoleConsumeItem accept)
    {
        RemoveProp(accept.itemId, (int)accept.num);
        moduleCangku.WareRemoveProp(accept.itemId, accept.num);
        DispatchModuleEvent(EventUpdateBagProp, currentBagProp);
        DispatchModuleEvent(EventRefreshCangku);

        var info = ConfigManager.Get<PropItemInfo>(accept.itemTypeId);
        if (info.itemType == PropType.Sundries && info.subType == (int)SundriesSubType.NpcgoodFeeling) moduleGift.GetGiftList();
    }

    #endregion

    #region change cloth msg

    public void SendChangeClothes(List<ulong> newClothes)
    {
        if (newClothes == null || newClothes.Count <= 0)
        {
            Logger.LogDetail("当前没有需要替换的时装");
            return;
        }

        tempChangeCloths.Clear();
        for (int i = 0; i < newClothes.Count; i++)
        {
            PItem targetItem = currentBagClothes.Find((o) => { return o.itemId == newClothes[i]; });
            if (targetItem != null) tempChangeCloths.Add(targetItem);
        }

        CsEquipChange p = PacketObject.Create<CsEquipChange>();
        p.itemIds = newClothes.ToArray();
        session.Send(p);
    }

    void _Packet(ScEquipChange changedClothes)
    {
        if (changedClothes.result == 0) AfterChangeCloth(false,tempChangeCloths);
        else Logger.LogError("换装失败");
    }

    private void AfterChangeCloth(bool isDown,List<PItem> list)
    {
        foreach (var item in list)
        {
            var info = item.GetPropItem();
            if (!info) continue;
            if (info.itemType == PropType.Weapon || info.itemType == PropType.FashionCloth)
                AddAndRemoveData(item, isDown);
        }
        list.Clear();
    }

    public void SendTakeOffClothes(List<ulong> dressedClothes)
    {
        if (dressedClothes == null || dressedClothes.Count <= 0)
        {
            Logger.LogDetail("当前没有脱下的时装");
            return;
        }

        tempDownCloths .Clear();
        for (int i = 0; i < dressedClothes.Count; i++)
        {
            PItem targetItem = currentDressClothes.Find((o) => { return o.itemId == dressedClothes[i]; });
            if (targetItem != null) tempDownCloths.Add(targetItem);
        }

        CsEquipDown p = PacketObject.Create<CsEquipDown>();
        p.itemIds = dressedClothes.ToArray();
        session.Send(p);
    }

    void _Packet(ScEquipDown dressedClothes)
    {
        if (dressedClothes.result == 0) AfterChangeCloth(true, tempDownCloths);
        else Logger.LogError("脱下时装失败");
    }
    #endregion

    #region use prop msg

    /// <summary>
    /// 使用染血的通缉令道具
    /// </summary>
    /// <param name="item"></param>
    public void SendUseChaseProp(PItem item)
    {
        //如果该道具是染血的通缉令
        var info = item.GetPropItem();
        if (!info) return;

        if (info.itemType == PropType.UseableProp && info.subType == (byte)UseablePropSubType.WantedWithBlood)
        {
            CsChaseTaskAccept p = PacketObject.Create<CsChaseTaskAccept>();
            p.itemId = item.itemId;
            session.Send(p);
        }
    }

    void _Packet(ScChaseTaskAccept accept)
    {
        if (accept.result != 0) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(400, accept.result));
        else
        {
            //create a new emergycyTask to send to module_chase
            moduleChase.AddNewEmergencyTask(accept.taskId, accept.curstar);
            DispatchModuleEvent(EventUseWantedPropSuccess, accept.taskId);
        }
    }
    #endregion

    #region module equip logic
    public void PrepareEquipData()
    {
        if (currentBagClothes == null) SendRequestProp(RequestItemType.InBag);
        else DispatchModuleEvent(EventBagInfo);
    }

    public void UpdateWeapons()
    {
        if (currentDressClothes == null) return;

        for (int i = 0; i < currentDressClothes.Count; i++)
        {
            var info = currentDressClothes[i].GetPropItem();
            if (!info) continue;

            if (info.itemType == PropType.Weapon)
            {
                if (info.subType != (byte)WeaponSubType.Gun)
                {
                    var item = weapon;
                    m_weapon = currentDressClothes[i];

                    var ii = weapon.GetPropItem();

                    m_weaponID = ii.subType;

                    if (item == null || item.GetPropItem()?.subType != ii.subType) moduleSkill.SendSkillInfo(); // 第一次赋值主武器时,要建立对应的技能信息表和等级表

                    if (item != null) // 当元素类型改变时,要建立新的技能状态表
                    {
                        var before = ConfigManager.Get<WeaponAttribute>(item.itemTypeId);
                        var now = ConfigManager.Get<WeaponAttribute>(weapon.itemTypeId);
                        if (before && now && before.elementType != now.elementType) moduleSkill.OnAddDataToSkillStateDic();
                    }
                }
                else
                {
                    offWeapon = currentDressClothes[i];
                    offWeaponID = (int)offWeapon?.GetPropItem()?.subType;
                }
            }
        }
    }

    public PItem GetNewPItem(ulong itemId, ushort itemTypeId)
    {
        PropItemInfo itemInfo = ConfigManager.Get<PropItemInfo>((int)itemTypeId);
        if (itemInfo == null)
            return null;

        PItem pitem = PacketObject.Create<PItem>();
        pitem.itemId = itemId;
        pitem.itemTypeId = (ushort)itemInfo.ID;
        string str = null;
        if (itemInfo.mesh != null && itemInfo.mesh.Length > 1)
        {
            for (int i = 0; i < itemInfo.mesh.Length; i++)
            {
                if (i == 0) str = itemInfo.mesh[0];
                else
                {
                    string itemStr = ";" + itemInfo.mesh[i];
                    str = str + itemStr;
                }
            }
        }
        pitem.timeLimit = (int)itemInfo.timeLimit * 86400;
        return pitem;
    }


    public void AddAndRemoveData(PItem item, bool isDown)
    {
        //切换的方式 要把身上的同类型替换下来添加到背包中去,把背包中的删除,添加到身上穿的中去
        if (isDown)//卸下
        {
            var removePItems = currentDressClothes.FindAll(o => o.itemId == item.itemId);
            if (removePItems != null && removePItems.Count > 0)
            {
                for (int i = 0; i < removePItems.Count; i++)
                {
                    currentDressClothes.Remove(removePItems[i]);
                }
            }
            currentBagClothes.Add(item);
        }
        else//换装
        {
            EquipType type = GetEquipTypeByID(item.itemTypeId);
            PItem operateItem = currentDressClothes.Find((p) => { return GetEquipTypeByID(p.itemTypeId) == type; });//理论上除了配饰有可能找不到意外,其他部件都能找到
            if (operateItem != null)
            {
                lastItemTypeId = operateItem.itemTypeId;
                var removePItems = currentDressClothes.FindAll(o => o.itemId == operateItem.itemId);
                if (removePItems != null && removePItems.Count > 0)
                {
                    for (int i = 0; i < removePItems.Count; i++)
                    {
                        currentDressClothes.Remove(removePItems[i]);
                    }
                }
                currentDressClothes.Add(item);

                currentBagClothes.Add(operateItem);
                currentBagClothes.Remove(item);
            }
            else//处理配饰
            {
                currentDressClothes.Add(item);
                currentBagClothes.Remove(item);
            }
        }
        //更换武器信息
        UpdateWeapons();

        DispatchModuleEvent(EventRefreshCurrentDress);
    }
    #endregion

    #region override functions

    protected override void OnModuleCreated()
    {
        base.OnModuleCreated();
        InitAllBagCollection();

        #region Debug
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        EventManager.AddEventListener("OnGmLockClass", OnGMLockClass);
#endif
        #endregion
    }

    protected override void OnEnterGame()
    {
        base.OnEnterGame();

        InitIntenAndEvoDic();
        selectEquipSubType = EquipType.None;
        selectEquipType = EnumSubEquipWindowType.MainPanel;
        accesssReturnWay = 0;
        SendRequestProp(RequestItemType.InBag);
    }

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        m_countDownItems.Clear();
        UpLoadRead.Clear();
        InsoulRead.Clear();
        IntentyRead.Clear();
        EvolveRead.Clear();
        EquipNewRead.Clear();
        EquipUpRead.Clear();
        EquipSpriteRead.Clear();
        EquipSublimaRead.Clear();
        EquipInsoulRead.Clear();
        EquipStrengRead.Clear();
        EquipAdvanceRead.Clear();
        FirstEnter = true;
        lastOperateType = EnumOperateEquipType.None;
        selectEquipSubType = EquipType.None;
        selectEquipType = EnumSubEquipWindowType.MainPanel;
        accesssReturnWay = 0;
        m_weapon = null;
        offWeapon = null;
        m_weaponID = 0;
        offWeaponID = 0;
        operateItem = null;
        lastItemTypeId = 0;

        #region Debug
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        m_lockedWeapon = null;
        m_lockedWeaponID = -1;
#endif
        #endregion
    }

    #endregion

    #region intentfy / evolve / msg

    #region static functions
    public static bool GetMaterialEnoughState(EvolveEquipInfo.EvolveMaterial[] datas, List<PItem> evloveProps)
    {
        if (datas == null) return true;

        //必须是填充的材料种类比实际拥有的材料种类少或者相等才进行检查
        if (evloveProps != null && datas.Length <= evloveProps.Count)
        {
            Dictionary<int, int> countDic = GetRealCountOfProp(evloveProps);
            int realCount = 0;
            foreach (var item in datas)
            {
                realCount = countDic.ContainsKey(item.propId) ? countDic[item.propId] : 0;
                if (realCount < item.num) return false;
            }
        }
        else return false;

        return true;
    }

    public static Dictionary<int, int> GetRealCountOfProp(List<PItem> bagProps)
    {
        Dictionary<int, int> dic = new Dictionary<int, int>();

        if (bagProps != null)
        {
            foreach (var item in bagProps)
            {
                if (!dic.ContainsKey(item.itemTypeId)) dic.Add(item.itemTypeId, 0);
                dic[item.itemTypeId] += (int)item.num;
            }
        }
        return dic;
    }

    public static int GetCoinCost(EquipType equipType, int currentExp, int totalExp, IntentifyEquipInfo limitinfo)
    {
        int exp = limitinfo == null ? int.MaxValue : limitinfo.exp;
        int realExp = totalExp > exp ? exp : totalExp;
        int realCost = instance.GetIntentifyCost(equipType, realExp);
        int alreadyCost = instance.GetIntentifyCost(equipType, currentExp);
        return realCost - alreadyCost;
    }

    public static List<int> GetReallyExpDetail(int currentExp, int swallowExp, IntentifyEquipInfo limitinfo)
    {
        int totalExp = currentExp + swallowExp;
        //fix total exp by limit info
        int exp = limitinfo == null ? int.MaxValue : limitinfo.exp;
        totalExp = totalExp > exp ? exp : totalExp;
        int realSwallow = totalExp - currentExp;

        return new List<int>() { totalExp, realSwallow };
    }
    #endregion

    #region config detail
    private void InitIntenAndEvoDic()
    {
        InitIntentifyDetail();
        InitEvolveDetail();
    }

    private void InitIntentifyDetail()
    {
        m_intentDetailDic.Clear();
        List<IntentifyEquipInfo> list = ConfigManager.GetAll<IntentifyEquipInfo>();
        for (EquipType i = EquipType.Weapon; i < EquipType.Hair; i++)
        {
            m_intentDetailDic.Add(i, new IntentifyDetail(i));
        }

        foreach (var item in list)
        {
            if (!m_intentDetailDic.ContainsKey(item.type)) continue;
            m_intentDetailDic[item.type].AddConfigData(item);
        }
    }

    public IntentifyDetail GetDetailConfig(EquipType type)
    {
        return m_intentDetailDic.ContainsKey(type) ? m_intentDetailDic[type] : null;
    }

    public int GetLimitIntenLevel(EquipType type, int level, bool hasEvo = false)
    {
        int deltaLevel = level % EVOLVE_LEVEL;
        //如果当前已经处于升阶等级，并且还没有升阶时，直接返回level
        if (level > 0 && !hasEvo && deltaLevel == 0) return level;

        int maxlevel = GetMaxIntenLevel(type);
        int evolevel = level / EVOLVE_LEVEL;

        int tarLevel = (evolevel + 1) * EVOLVE_LEVEL;
        tarLevel = Mathf.Clamp(tarLevel, tarLevel, maxlevel);
        return tarLevel;
    }

    public IntentifyEquipInfo GetLimitIntenInfo(EquipType type, int level, bool hasEvo = false)
    {
        int tarLevel = GetLimitIntenLevel(type, level, hasEvo);
        return GetIntentifyInfo(type, tarLevel);
    }

    /// <summary>
    /// 通过总经验获取当前的最大升级信息(受限于进阶)
    /// </summary>
    /// <param name="type"></param>
    /// <param name="level"></param>
    /// <param name="totalExp"></param>
    /// <param name="hasEvo"></param>
    /// <returns></returns>
    public IntentifyEquipInfo GetLimitIntenLevelByExp(EquipType type, int level, int totalExp, bool hasEvo = false)
    {
        var limitInfo = GetLimitIntenInfo(type, level, hasEvo);

        IntentifyEquipInfo expInfo = null;
        var maxInfo = GetDetailConfig(type)?.GetMaxIntentyInfo();
        if (maxInfo != null && totalExp >= maxInfo.exp) expInfo = maxInfo;
        if (expInfo == null) expInfo = GetIntentifyInfoByExp(type, totalExp);

        if (limitInfo == null || expInfo == null)
        {
            Logger.LogWarning("GetLimitIntenLevelByExp failed, limitInfo = {0} expinfo = {1}", limitInfo == null ? "null" : limitInfo.ToString(), expInfo == null ? "null" : expInfo.ToString());
            return null;
        }
        return expInfo.level < limitInfo.level ? expInfo : limitInfo;
    }

    public int GetMaxIntenLevel(EquipType type)
    {
        return m_intentDetailDic.ContainsKey(type) ? m_intentDetailDic[type].maxLevel : 0;
    }

    public IntentifyEquipInfo GetIntentifyInfo(EquipType type, int level)
    {
        return m_intentDetailDic.ContainsKey(type) ? m_intentDetailDic[type].datas.Find(o => o.level == level) : null;
    }

    public IntentifyEquipInfo GetIntentifyInfoByExp(EquipType type, int exp)
    {
        if (!m_intentDetailDic.ContainsKey(type)) return null;

        List<IntentifyEquipInfo> list = m_intentDetailDic[type].datas;
        IntentifyEquipInfo cur = null, last = null;
        int lastIndex = 0;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            cur = list[i];
            lastIndex = i - 1;
            last = lastIndex >= 0 ? list[lastIndex] : null;

            if (cur.exp >= exp && exp >= (last == null ? 0 : last.exp)) return last;
        }
        return null;
    }

    public int GetIntentifyCost(EquipType type, int exp)
    {
        if (!m_intentDetailDic.ContainsKey(type)) return 0;

        List<IntentifyEquipInfo> datas = m_intentDetailDic[type].datas;
        int cost = 0;
        IntentifyEquipInfo m_lastItem = null;
        foreach (var item in datas)
        {
            if (exp >= item.exp)
            {
                cost += item.costNumber;
                m_lastItem = item;
            }
            else
            {
                int lastLevelExp = m_lastItem == null ? 0 : m_lastItem.exp;
                float delta = (exp - lastLevelExp) * 1.0f / (item.exp - lastLevelExp) * 1.0f;
                cost += Mathf.RoundToInt(delta * item.costNumber);
                break;
            }
        }
        return cost;
    }

    public List<IntentifyEquipInfo> GetGetIntentifyInfos(EquipType type, int beginlevel, int endlevel)
    {
        List<IntentifyEquipInfo> list = new List<IntentifyEquipInfo>();
        if (beginlevel == endlevel) return m_intentDetailDic.ContainsKey(type) ? m_intentDetailDic[type].datas.FindAll(o => o.level == beginlevel) : list;
        return m_intentDetailDic.ContainsKey(type) ? m_intentDetailDic[type].datas.FindAll(o => o.level > beginlevel && o.level <= endlevel) : list;
    }

    private void InitEvolveDetail()
    {
        m_evolveDetailDic.Clear();

        List<EvolveEquipInfo> list = ConfigManager.GetAll<EvolveEquipInfo>();
        for (EquipType i = EquipType.Weapon; i < EquipType.Hair; i++)
        {
            m_evolveDetailDic.Add(i, new EvolveDetail(i));
        }

        foreach (var item in list)
        {
            if (!m_evolveDetailDic.ContainsKey(item.type)) continue;
            m_evolveDetailDic[item.type].AddConfigData(item);
        }
    }

    public int GetMaxEvoLevel(EquipType type)
    {
        return m_evolveDetailDic.ContainsKey(type) ? m_evolveDetailDic[type].maxLevel : 0;
    }

    public EvolveEquipInfo GetEvolveInfo(EquipType type, int level)
    {
        return m_evolveDetailDic.ContainsKey(type) ? m_evolveDetailDic[type].datas.Find(o => o.level == level) : null;
    }

    #endregion

    #region msg

    public void SendIntentiEquip(PItem m, Dictionary<PItem, int> mats)
    {
        if (mats == null || mats.Count == 0) return;

        m.CopyTo(ref lastIntentyCacheItem);
        lastOperateType = EnumOperateEquipType.Intenty;
        CsEquipStrength p = PacketObject.Create<CsEquipStrength>();
        p.materials = GetMatetial(mats);
        p.equipUid = m.itemId;
        session.Send(p);
    }

    void _Packet(ScEquipStrength p)
    {
        Logger.LogInfo("after intenty {0}", p.result);
        if (p.result != 0) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(10100, p.result));
    }

    private PMaterial[] GetMatetial(Dictionary<PItem, int> mats)
    {
        List<PMaterial> list = new List<PMaterial>();
        if (mats == null) return list.ToArray();

        foreach (var item in mats)
        {
            if (item.Value <= 0) continue;

            PMaterial p = PacketObject.Create<PMaterial>();
            p.uid = item.Key.itemId;
            p.num = (ushort)item.Value;
            list.Add(p);
        }
        return list.ToArray();
    }

    public void SendEvolveEquip(PItem m)
    {
        m.CopyTo(ref lastIntentyCacheItem);

        lastOperateType = EnumOperateEquipType.Evolve;
        CsEquipAdvance p = PacketObject.Create<CsEquipAdvance>();
        p.equipUid = m.itemId;
        session.Send(p);
    }

    void _Packet(ScEquipAdvance p)
    {
        Logger.LogInfo("after envolve {0}", p.result);
        if (p.result != 0) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(10101, p.result));
    }

    void _Packet(ScRoleItemInfo p)
    {
        if (p.item == null) return;

        PItem tar = currentDressClothes.Find(o => o.itemId == p.item.itemId);
        if (tar == null) tar = currentBagClothes.Find(o => o.itemId == p.item.itemId);
        if (tar == null) tar = currentBagProp.Find(o => o.itemId == p.item.itemId);

        if (tar == null) return;

        p.item.CopyTo(tar);
        DispatchModuleEvent(EventItemDataChange, tar);

        //在此处才开始分发事件，因为强化或者进阶成功之后没有数据，真实数据在此处返回
        if (lastOperateType > EnumOperateEquipType.None && lastOperateType < EnumOperateEquipType.Count)
        {
            DispatchModuleEvent(EventOperateSuccess, lastOperateType, tar, lastIntentyCacheItem);
        }
        lastOperateType = EnumOperateEquipType.None;
    }

    public void GMAddIntentProps()
    {
        List<PropItemInfo> props = ConfigManager.GetAll<PropItemInfo>();
        List<PropItemInfo> addProps = props.FindAll(o => o.itemType == PropType.IntentyProp || o.itemType == PropType.EvolveProp);
        foreach (var item in addProps)
        {
            moduleGm.SendAddProp((ushort)item.ID, 400);
        }
    }

    #endregion

    #region load weapon 
    public void LoadModel(PItem item, int layer = Layers.WEAPON, bool hidePlayer = true, int type = 0)
    {
        if (item == null || !(Level.current is Level_Home)) return;

        Level level = Level.current;
        List<string> models = GetItemAllAssets(item);
        if (hidePlayer) moduleHome.HideOthers();

        moduleGlobal.LockUI("", 0.5f);
        Level.PrepareAssets(models, (r) =>
        {
            if (!r)
            {
                moduleGlobal.UnLockUI();
                return;
            }

            var data = GetShowInfoData(item, type);
            var equipType = GetEquipTypeByItem(item);

            var dic = GetItemModelsAndEffects(item);
            int index = 0;
            foreach (var me in dic)
            {
                GameObject go = level.startPos.Find(me.Key)?.gameObject;
                if (!go)
                {
                    go = Level.GetPreloadObject<GameObject>(me.Key);
                    if (go == null) Logger.LogError("equip with propId {1} and asset name {0} cannot be loaded,please check config", me.Key, item.itemTypeId);
                    go?.transform.SetParent(level.startPos.transform);
                }
                SetWeaponInfo(go, data.GetValue<ShowCreatureInfo.SizeAndPos>(index), layer, equipType);
                UpdateWeaponEffects(go, me.Value, layer);
                index++;
            }

            moduleGlobal.UnLockUI();
            //when loaded complete ,must reget models
            models = GetItemAllAssets(item);
            if (!hidePlayer) models.Add("player");
            moduleHome.HideOthers(models.ToArray());
        });
    }

    private List<string> GetItemAllAssets(PItem item)
    {
        List<string> l = new List<string>();
        if (item == null) return l;

        EquipType type = GetEquipTypeByItem(item);
        var info = item.GetPropItem();
        if (type == EquipType.Cloth)
        {
            string model = info?.previewModel;
            if (!string.IsNullOrEmpty(model)) l.Add(model);
        }
        else if (type == EquipType.Gun || type == EquipType.Weapon)
        {
            var weapon = WeaponInfo.GetWeapon(info.subType, info.ID);
            weapon.GetAllAssets(l);
        }
        return l;
    }

    private Dictionary<string, List<string>> GetItemModelsAndEffects(PItem item)
    {
        if (item == null) return null;

        var dic = new Dictionary<string, List<string>>();

        EquipType type = GetEquipTypeByItem(item);
        var info = item.GetPropItem();
        if (type == EquipType.Cloth)
        {
            string model = info?.previewModel;
            if (!string.IsNullOrEmpty(model)) dic.Add(model, null);
        }
        else if (type == EquipType.Gun || type == EquipType.Weapon)
        {
            var weapon = WeaponInfo.GetWeapon(info.subType, info.ID);
            if (weapon == null || weapon.singleWeapons == null) return dic;

            foreach (var w in weapon.singleWeapons)
            {
                if (!string.IsNullOrEmpty(w.model))
                {
                    if (dic.ContainsKey(w.model))
                    {
                        Logger.LogWarning("weapon id = [0] has duplicate model", weapon.weaponID);
                        continue;
                    }

                    dic.Add(w.model, new List<string>());
                    if (!string.IsNullOrEmpty(w.effects))
                    {
                        var es = Util.ParseString<string>(w.effects);
                        dic[w.model].AddRange(es);
                    }
                }
            }
        }
        return dic;
    }

    private ShowCreatureInfo.SizeAndPos[] GetShowInfoData(PItem item, int wType)
    {
        PropItemInfo info = item.GetPropItem();
        if (!info) return null;

        EquipType type = GetEquipTypeByItem(item);
        int showId = type == EquipType.Cloth ? 300 : info.subType + 100;
        if (wType == 1) showId = type == EquipType.Cloth ? 600 : info.subType + 400;

        var showInfo = ConfigManager.Get<ShowCreatureInfo>(showId);
        if (showInfo == null)
        {
            Logger.LogError("can not find config showCreatureInfo, please check config [{0}]", showId);
            return null;
        }

        var showData = showInfo.GetDataByIndex(info.ID);
        if (showData == null)
        {
            showData = showInfo.forData.Length > 0 ? showInfo.forData[0] : null;
            Logger.LogWarning("cause ShowCreatureInfo.Id = [{0}] with itemTypeId = [{1}] cannot be finded,we use itemTypeId = [{2}] to instead!", showId, info.ID, showData == null ? "null" : showData.index.ToString());
        }
        return showData.data;
    }

    private void SetWeaponInfo(GameObject weaponInstance, ShowCreatureInfo.SizeAndPos data, int layer, EquipType type = EquipType.Weapon)
    {
        if (weaponInstance == null || data == null) return;

        weaponInstance.transform.SetParent(Level.current.startPos);
        weaponInstance.SetActive(true);
        //为了避免跟人物用相同的
        Util.SetLayer(weaponInstance, layer);
        weaponInstance.transform.localPosition = data.pos;
        weaponInstance.transform.localEulerAngles = data.rotation;
        WeaponRotation c = weaponInstance.GetComponentDefault<WeaponRotation>();
        c.type = type;
    }

    private void UpdateWeaponEffects(GameObject weaponObj, List<string> effects, int layer)
    {
        if (effects == null || effects.Count == 0 || !weaponObj) return;

        foreach (var e in effects)
        {
            var et = Util.FindChild(weaponObj.transform, e);
            if (et) continue;

            var eff = Level.GetPreloadObjectFromPool(e);
            if (!eff) Logger.LogError("Could not load weapon effect [{0}]", e);
            else
            {
                var i = eff.GetComponent<EffectInvertHelper>();
                if (!i)
                {
                    Logger.LogError("Invalid weapon effect [{0}]", e);
                    Level.BackEffect(eff);
                    continue;
                }

                eff.name = e;
                eff.SetActive(true);

                var ad = eff.GetComponent<AutoDestroy>();
                if (ad) ad.enabled = false;

                Util.SetLayer(eff, layer);
                Util.AddChild(weaponObj.transform, eff.transform);
            }
        }
    }
    #endregion

    #endregion

    #region Debug helper

#if DEVELOPMENT_BUILD || UNITY_EDITOR
    public bool lockedWeaponDifference { get { return m_lockedWeaponID > -1 && m_lockedWeaponID != m_weaponID; } }

    private PItem m_lockedWeapon;
    private int m_lockedWeaponID;

    private void OnGMLockClass(Event_ e)
    {
        var c = (int)e.param1;
        var i = (int)e.param2;

        if (m_lockedWeapon == null) m_lockedWeapon = PacketObject.Create<PItem>();

        var w = WeaponInfo.GetWeapon(c > 0 ? c : m_weaponID, c == m_weaponID && i < 1 && m_weapon != null ? m_weapon.itemTypeId : i);

        if (!w.isEmpty) m_lockedWeapon.itemTypeId = (ushort)w.weaponItemId;
        else m_weapon?.CopyTo(ref m_lockedWeapon);

        m_lockedWeaponID = (int)m_lockedWeapon?.GetPropItem().subType;
    }
#endif

    #endregion

    public bool IsDressOn(PItem data)
    {
        return currentDressClothes.Exists(item => item.itemId == data?.itemId);
    }

    public int GetSuitNumber(int suitId)
    {
        if (0 == suitId)
            return 0;
        var list = currentDressClothes?.FindAll(item => item.growAttr?.suitId == suitId);
        return list?.Count ?? 0;
    }

    public void ChangeStateLock(ulong itemId, int type)
    {
        foreach (List<PItem> item in m_bagCollections.Values)
        {
            for (int i = 0; i < item.Count; i++)
            {
                if (item[i].itemId == itemId)
                {
                    item[i].isLock = (sbyte)type;
                    break;
                }
            }
        }

        var rune = moduleRune.currentInBag.Find(p => p.itemId == itemId);
        if (rune == null) rune = moduleRune.currentEquip.Find(p => p.itemId == itemId);
        if (rune != null) rune.isLock = (sbyte)type;
    }
    public void ChangeItemAttr(PItem p)
    {
        foreach (List<PItem> item in m_bagCollections.Values)
        {
            for (int i = 0; i < item.Count; i++)
            {
                if (item[i].itemId == p.itemId)
                {
                    item[i].growAttr = p.growAttr.Clone();
                    break;
                }
            }
        }
    }

    public uint CalcStrengthExp(PItem rItem)
    {
        if (null == rItem?.growAttr?.equipAttr)
            return 0;
        uint num = 0;
        EquipType type = GetEquipTypeByItem(rItem);
        var info = ConfigManager.Find<IntentifyEquipInfo>(item => item.type == type && item.level == rItem.growAttr.equipAttr.strength);
        if (null != info)
            num += (uint)info.exp;
        num += rItem.growAttr.equipAttr.strengthExpr;
        return num;
    }

    public IReadOnlyList<ItemPair> CalcSoulBack(PItem rItem, int count)
    {
        if (rItem?.growAttr?.equipAttr == null)
            return Util.EmptyList<ItemPair>();

        WeaponAttribute global = ConfigManager.Get<WeaponAttribute>(rItem.itemTypeId);

        if (global == null)
            return Util.EmptyList<ItemPair>();

        var list = new List<ItemPair>();
        var datas = ConfigManager.GetAll<Insoul>();
        var num = 0;
        var e = 0;
        var level = rItem.growAttr.equipAttr.level;
        for (var i = 0; i < level; i++)
        {
            var needExp = datas[i].exp - e;
            var n = Mathf.CeilToInt((float)needExp / datas[i].exp_one);

            e = datas[i].exp_one * n - needExp;

            num += n * datas[i].lingshi;
        }

        num += ((int)rItem.growAttr.equipAttr.expr - e) / datas[level].exp_one * datas[level].lingshi;


        list.Add(new ItemPair() { itemId = global.elementId, count = num * count });

        return list;
    }

    public List<ItemPair> CalcDecomposeMatrial(PItem rItem, int count)
    {
        uint exp = CalcStrengthExp(rItem);
        exp = (uint)Mathf.FloorToInt(exp * moduleGlobal.system.decomposeRate);
        var itemPool = new int[] { 10105, 10104, 10103, 10102, 10101 };
        var list = new List<ItemPair>();
        for (var i = 0; i < itemPool.Length && exp > 0; i++)
        {
            var info = ConfigManager.Get<PropItemInfo>(itemPool[i]);
            var c = exp / info.swallowedExpr;
            if (c > 0)
            {
                list.Add(new ItemPair() { itemId = itemPool[i], count = (int)(c * count) });
                exp -= info.swallowedExpr * c;
            }
        }

        var compound = ConfigManager.Get<Compound>(rItem.GetPropItem().decompose);
        if (compound?.items != null && compound.items.Length > 0)
        {
            for (int i = 0; i < compound.items.Length; i++)
                list.Add(new ItemPair() { itemId = compound.items[i].itemId, count = compound.items[i].count * count });
        }
        list.AddRange(CalcSoulBack(rItem, count));
        return list;
    }

    #region 装备操作判断

    public List<ulong> UpLoadRead = new List<ulong>(); //已读的进化武器
    public Dictionary<ulong, int> InsoulRead = new Dictionary<ulong, int>();//已读的入魂武器及其可入魂等级
    public Dictionary<ulong, int> IntentyRead = new Dictionary<ulong, int>();//点击时 可达到的强化等级
    public Dictionary<ulong, int> EvolveRead = new Dictionary<ulong, int>();//点击时 可达到的升华等级

    #region check intentfy(装备强化)

    /// <summary>
    /// 强化判断是否可以升满一级
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public bool CheckIntenty(List<PItem> items)
    {
        bool canIntenty = false;
        int totalExp = GetBagTotalIntentyExp();
        foreach (var item in items)
        {
            canIntenty = CheckIntenty(item, totalExp);
            if (canIntenty) break;
        }
        return canIntenty;
    }

    public bool CheckIntenty(PItem item)
    {
        return CheckIntenty(item, GetBagTotalIntentyExp());
    }

    public bool CheckIntenty(PItem item, int totalExp)
    {
        if (item == null) return false;

        var previewType = GetPeiviewEuipeTypeByItem(item);
        if (previewType != PreviewEquipType.Intentify) return false;

        EquipType type = GetEquipTypeByItem(item);
        if (type == EquipType.Weapon || type == EquipType.Gun || type == EquipType.Cloth)
        {
            int neexExp = int.MaxValue;
            var equipType = GetEquipTypeByItem(item);

            var info = item.GetPropItem();
            if (!info) return false;

            var max = moduleEquip.GetLimitIntenLevelByExp(equipType, item.GetIntentyLevel(), totalExp + item.GetCurrentTotalExp(), item.HasEvolved());
            if (max == null) return false;

            var nextIntentyInfo = GetIntentifyInfo(equipType, item.GetIntentyLevel() + 1); //一级就显示
            //var nextIntentyInfo = GetIntentifyInfo(equipType, max.level);
            //如果不能强化了，就不检查了
            if (nextIntentyInfo == null) return false;
            
            if (IntentyRead.ContainsKey(item.itemId))
            {
                if (max.level <= IntentyRead[item.itemId]) return false;
            }
            neexExp = nextIntentyInfo.exp - item.GetCurrentTotalExp();

            if (totalExp >= neexExp)//现在可以达到的经验  下一级需要的经验
            {
                int cost = GetCoinCost(type, item.GetCurrentTotalExp(), nextIntentyInfo.exp, nextIntentyInfo);
                return modulePlayer.coinCount >= cost;
            }
        }

        return false;
    }

    public int GetBagTotalIntentyExp()
    {
        int total = 0;
        var props = GetBagProps(PropType.IntentyProp);
        if (props == null || props.Count == 0) return total;

        foreach (var item in props)
        {
            var info = item.GetPropItem();
            if (!info) continue;

            total += (int)(info.swallowedExpr * item.num);
        }
        return total;
    }

    #endregion

    #region check evolve(装备进阶)

    /// <summary>
    /// 进阶判断是否材料足够
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public bool CheckEvolve(List<PItem> items)
    {
        bool canEvolve = false;
        var bagEvolveItems = GetBagProps(PropType.EvolveProp);
        foreach (var item in items)
        {
            canEvolve = CheckEvolve(item, bagEvolveItems);
            if (canEvolve) break;
        }
        return canEvolve;
    }

    public bool CheckEvolve(PItem item)
    {
        return CheckEvolve(item, GetBagProps(PropType.EvolveProp));
    }

    public bool CheckEvolve(PItem item, List<PItem> evolveItems)
    {
        if (item == null) return false;

        var previewType = GetPeiviewEuipeTypeByItem(item);
        if (previewType != PreviewEquipType.Evolve) return false;

        var equipType = GetEquipTypeByItem(item);
        var info = item.GetPropItem();
        if (!info) return false;

        var evolveInfo = GetEvolveInfo(equipType, item.GetEvolveLevel() + 1);
        if (evolveInfo == null) return false;

        int leftLevel = item.GetIntentyLevel();
        int nextLimitLevel = moduleEquip.GetLimitIntenLevel(equipType, leftLevel, true);
        if (EvolveRead.ContainsKey(item.itemId))
        {
            if (nextLimitLevel <= EvolveRead[item.itemId]) return false;
        }

        bool materailEnough = GetMaterialEnoughState(evolveInfo.materials, evolveItems);
        //检测金币
        if (materailEnough) return modulePlayer.coinCount >= evolveInfo.costNumber;
        else return materailEnough;
    }

    #endregion

    #region check insoul(入魂)

    public bool CheckInSoul(List<PItem> items)
    {
        foreach (var item in items)
        {
            if (CheckInSoul(item)) return true;
        }

        return false;
    }

    public bool CheckInSoul(PItem item)
    {
        if (item == null) return false;
        if (!moduleGuide.IsActiveFunction(106)) return false;

        var propinfo = item.GetPropItem();
        if (!propinfo || propinfo.itemType != PropType.Weapon || (propinfo.itemType != PropType.Weapon && propinfo.subType == (int)WeaponSubType.Gun)) return false;

        WeaponAttribute DeInfos = ConfigManager.Get<WeaponAttribute>(item.itemTypeId);
        if (DeInfos == null || item.growAttr == null) return false;

        var insouLevel = item.InvalidGrowAttr() ? 0 : item.growAttr.equipAttr.level;
        if (insouLevel >= moduleForging.Insoul_Info.Count - 1) return false;

        var inSoul = moduleForging.Insoul_Info.GetValue(insouLevel);
        if (!inSoul) return false;

        var nextlevel = InsoulLevel(DeInfos.elementId, (int)item.growAttr.equipAttr.expr, insouLevel);
        if (InsoulRead.ContainsKey(item.itemId))
        {
            if (nextlevel <= InsoulRead[item.itemId]) return false;
        }

        return insouLevel < nextlevel ? true : false;
    }

    public int InsoulLevel(int eleId, int lastOver, int insouLevel)//入魂可达到的等级
    {
        var level = insouLevel;

        var elementDic = moduleEquip.GetElementNumDic();
        int Elenum = elementDic.Get(eleId);
        int icon = (int)modulePlayer.coinCount;
        var times = 0;

        for (int i = insouLevel; i < moduleForging.Insoul_Info.Count; i++)
        {
            var show = moduleForging.Insoul_Info.GetValue(i);
            if (!show) continue;
            times = Mathf.CeilToInt((show.exp - lastOver) / show.exp_one);
            lastOver = times * show.exp_one - show.exp;
            level = i;
            if (icon < show.gold * times || Elenum < show.lingshi * times) break;
            icon -= show.gold * times;
            Elenum -= show.lingshi * times;
        }
        return level;
    }

    #endregion

    #region check DegreeElevation(武器进化)
    public bool CheckDegreeElevation(List<PItem> items)
    {
        foreach (var item in items)
        {
            if (CheckDegreeElevation(item)) return true;
        }

        return false;
    }

    public bool CheckDegreeElevation(PItem item)
    {
        if (item == null || item?.growAttr == null) return false;
        if (!moduleGuide.IsActiveFunction(107)) return false;

        var propinfo = item.GetPropItem();
        if (!propinfo) return false;//|| propinfo.itemType != PropType.Weapon || (propinfo.itemType != PropType.Weapon && propinfo.subType == (int)WeaponSubType.Gun)

        WeaponAttribute deInfos = ConfigManager.Get<WeaponAttribute>(item.itemTypeId);
        if (deInfos == null || deInfos.quality == null || deInfos.quality?.Length <= 0 || deInfos.quality[0].attributes == null || deInfos.quality[0].attributes.Length <= 0) return false;

        int upLevel = item.InvalidGrowAttr() ? 0 : item.growAttr.equipAttr.star;
        if (upLevel >= deInfos.quality.Length) return false;

        var info = deInfos.quality.GetValue<WeaponAttribute.WeaponLevel>(upLevel - 1);
        if (info == null) return false;

        var read = UpLoadRead.Exists(a => a == item.itemId);
        if (read) return false;

        int suipian = moduleEquip.GetPropCount(deInfos.debrisId);
        return suipian >= info.debrisNum;
    }

    #endregion

    #region check Sublimation（升华）

    public bool CheckSublimation(List<PItem> items)
    {
        foreach (var item in items)
        {
            if (CheckSublimation(item)) return true;
        }

        return false;
    }

    public bool NeedNoticeSublimation(PItem item)
    {
        var key = new NoticeDefaultKey(NoticeType.Sublimation);
        moduleNotice.SetNoticeState(key, CheckSublimation(item));
        return moduleNotice.IsNeedNotice(key);
    }

    public bool CheckSublimation(PItem item)
    {
        if (item == null) return false;
        if (!moduleGuide.IsActiveFunction(108)) return false;

        var propinfo = item.GetPropItem();
        if (!propinfo || !propinfo.sublimation) return false;

        int suitId = item.growAttr == null ? 0 : item.growAttr.suitId;

        //获取所有可以升华的套装信息
        var suits = ConfigManager.FindAll<SuitInfo>(o => o.prevSuitId == suitId);
        if (suits == null || suits.Count == 0) return false;

        bool valid = true;
        //检查每个套装信息
        foreach (var suit in suits)
        {
            //先检查是否拥有图纸
            valid = GetPropCount(suit.drawingId) > 0;
            if (!valid) continue;

            //拥有了图纸，检查其他消耗品
            valid = EnoughPropFromCheckItemPair(suit.costs);
            if (valid) return true;
        }

        return false;
    }

    public bool EnoughPropFromCheckItemPair(ItemPair[] costs, int rTimes = 1)
    {
        if (costs == null || costs.Length == 0) return true;

        bool valid = true;
        foreach (var cost in costs)
        {
            if (cost.itemId == 1) valid = modulePlayer.coinCount >= cost.count * rTimes;
            else if (cost.itemId == 2) valid = modulePlayer.gemCount >= cost.count * rTimes;
            else
            {
                int propCount = GetPropCount(cost.itemId);
                valid = propCount >= cost.count * rTimes;
            }

            if (!valid) break;
        }
        return valid;
    }
    #endregion

    #region check SoulSprite（器灵）
    public bool CheckSoulSprite(List<PItem> items)
    {
        foreach (var item in items)
        {
            if (CheckSoulSprite(item)) return true;
        }

        return false;
    }

    public bool CheckSoulSprite(PItem item)
    {
        if (item == null || item?.growAttr == null || item?.growAttr?.soulAttr == null) return false;
        if (item.growAttr.soulAttr.attr != null) return false;
        if (!moduleGuide.IsActiveFunction(109)) return false;

        var propinfo = item.GetPropItem();
        if (!propinfo || !propinfo.soulable) return false;

        var soulSpriteCost = ConfigManager.Get<SoulCost>(propinfo.quality);
        if (!soulSpriteCost) return false;

        return EnoughPropFromCheckItemPair(soulSpriteCost.costs);
    }

    public bool NeedNoticeSoul(PItem item)
    {
        var key = new NoticeDefaultKey(NoticeType.Soul);
        moduleNotice.SetNoticeState(key, CheckSoulSprite(item));
        return moduleNotice.IsNeedNotice(key);
    }

    #endregion

    #region authority check(操作权限检查)

    public EnumEquipOperationAuthority GetAllEquipOperationAuthority(PItem item)
    {
        EnumEquipOperationAuthority authority = EnumEquipOperationAuthority.None;
        if (CheckIntenty(item)) authority |= EnumEquipOperationAuthority.Intenty;
        if (CheckEvolve(item)) authority |= EnumEquipOperationAuthority.Evolve;
        if (CheckInSoul(item)) authority |= EnumEquipOperationAuthority.InSoul;
        if (CheckDegreeElevation(item)) authority |= EnumEquipOperationAuthority.DegreeElevation;
        if (NeedNoticeSublimation(item)) authority |= EnumEquipOperationAuthority.Sublimation;
        if (NeedNoticeSoul(item)) authority |= EnumEquipOperationAuthority.SoulSprite;
        return authority;
    }

    public static bool HasEquipOperation(PItem item, EnumEquipOperationAuthority tar)
    {
        var authority = instance.GetAllEquipOperationAuthority(item);
        return (authority & tar) > 0;
    }

    public static bool HasAnyEquipOperation(PItem item)
    {
        var authority = instance.GetAllEquipOperationAuthority(item);
        return authority != EnumEquipOperationAuthority.None;
    }

    #endregion

    #region other public funcsions

    public bool FirstEnter = true;

    public bool CheckHomeEquipMark()
    {
        return CheckCurrentEquipOperation() || CheckAnyNewEquipInBag();
    }
    public bool CheckCurrentEquipOperation()
    {
        bool canOperation = false;
        PropItemInfo info = null;

        foreach (var item in currentDressClothes)
        {
            info = item.GetPropItem();
            if (!info) continue;
            if (info.itemType == PropType.Weapon || (info.itemType == PropType.FashionCloth && info.subType == (int)FashionSubType.FourPieceSuit))
            {
                canOperation = HasAnyEquipOperation(item);
                if (canOperation) break;
            }
        }
        return canOperation;
    }

    /// <summary>
    /// 检测背包所有类型是否含有新品
    /// </summary>
    /// <returns></returns>
    public bool CheckAnyNewEquipInBag()
    {
        bool isNew = false;

        for (int i = (int)EquipType.Weapon; i <= (int)EquipType.Guise; i++)
        {
            isNew |= moduleCangku.TypeHint((EquipType)i);
            if (isNew) break;
        }

        return isNew;
    }
    #endregion

    #region hint

    //在每次点开装备时候就要重置
    public List<ulong> EquipNewRead = new List<ulong>();
    public List<ulong> EquipUpRead = new List<ulong>(); //进化
    public List<ulong> EquipSpriteRead = new List<ulong>(); //器灵
    public List<ulong> EquipSublimaRead = new List<ulong>(); //升华
    public Dictionary<ulong, int> EquipInsoulRead = new Dictionary<ulong, int>();//入魂等级
    public Dictionary<ulong, int> EquipStrengRead = new Dictionary<ulong, int>();//强化等级
    public Dictionary<ulong, int> EquipAdvanceRead = new Dictionary<ulong, int>();//进阶等级

    public void RefreshEquipHintInfo()//进武器界面刷新当前的可操作 (刚进游戏也要刷新)
    {
        GetNewEquipNow();
        GetNotCanInfo();
    }

    public bool AddRefrshEquipHint()//获得新物品刷新equip 红点
    {
        if (CheckEquipState() || CheckNewHint()) return true;
        return false;
    }

    private void GetNewEquipNow()//获取当前未看过的新的的装备
    {
        EquipNewRead.Clear();
        for (int i = 0; i < moduleCangku.newProp.Count; i++)
        {
            var p = GetEquipTypeByItem(moduleCangku.newProp[i]);
            bool isEquip = false;
            for (int j = (int)EquipType.Weapon; j <= (int)EquipType.Guise; j++)
            {
                if (p == (EquipType)j)
                {
                    isEquip = true;
                    break;
                }
            }
            if (isEquip) EquipNewRead.Add(moduleCangku.newProp[i].itemId);
        }
    }

    private void GetNotCanInfo()
    {
        //判断刚上线武器是否可以进行操作每一个;
        PropItemInfo info = null;
        int total = GetBagTotalIntentyExp();
        foreach (var item in currentDressClothes)
        {
            info = item.GetPropItem();
            if (!info) continue;
            if (info.itemType == PropType.Weapon || (info.itemType == PropType.FashionCloth && info.subType == (int)FashionSubType.FourPieceSuit))
            {
                var type = GetEquipTypeByItem(item);
                var level = item.GetIntentyLevel();
                info = item.GetPropItem();

                if (EquipStrengRead.ContainsKey(item.itemId)) EquipStrengRead.Remove(item.itemId);
                if (EquipAdvanceRead.ContainsKey(item.itemId)) EquipAdvanceRead.Remove(item.itemId);
                if (EquipInsoulRead.ContainsKey(item.itemId)) EquipInsoulRead.Remove(item.itemId);
                if (EquipNewRead.Exists(a => a == item.itemId)) EquipNewRead.Remove(item.itemId);
                if (EquipSublimaRead.Exists(a => a == item.itemId)) EquipSublimaRead.Remove(item.itemId);
                if (EquipSpriteRead.Exists(a => a == item.itemId)) EquipSpriteRead.Remove(item.itemId);

                if (CheckIntenty(item))
                {
                    var max = GetLimitIntenLevelByExp(type, level, total + item.GetCurrentTotalExp(), item.HasEvolved());
                    EquipStrengRead.Add(item.itemId, max.level);
                }
                if (CheckEvolve(item))
                {
                    int max = GetLimitIntenLevel(type, level, true);
                    EquipAdvanceRead.Add(item.itemId, max);
                }
                if (CheckInSoul(item))
                {
                    WeaponAttribute DeInfos = ConfigManager.Get<WeaponAttribute>(item.itemTypeId);
                    var insouLevel = item.InvalidGrowAttr() ? 0 : item.growAttr.equipAttr.level;
                    var max = InsoulLevel(DeInfos.elementId, (int)item.growAttr.equipAttr.expr, insouLevel);
                    EquipInsoulRead.Add(item.itemId, max);
                }
                if (CheckDegreeElevation(item)) EquipNewRead.Add(item.itemId);
                if (CheckSublimation(item)) EquipSublimaRead.Add(item.itemId);
                if (CheckSoulSprite(item)) EquipSpriteRead.Add(item.itemId);
            }
        }
    }

    private bool CheckEquipState()
    {
        //判断当前是否可以重新显示红点
        foreach (var item in currentDressClothes)
        {
            if (CheckEquipHave(item, EquipStrengRead, IntentyRead, 1)) return true;
            if (CheckEquipHave(item, EquipAdvanceRead, EvolveRead, 2)) return true;
            if (CheckEquipHave(item, EquipInsoulRead, InsoulRead, 3)) return true;
            if (CheckEquipHave(item, EquipUpRead, UpLoadRead, 4))  return true;
            if (CheckEquipHave(item, EquipSublimaRead, null, 5))  return true;
            if (CheckEquipHave(item, EquipSpriteRead , null, 6))  return true;
        }
        return false;
    }

    private bool CheckEquipHave(PItem item, List<ulong> action, List<ulong> read, int type)
    {
        // 1 强化 2 进阶 3 入魂 4 进化 5 升华 6 器灵 
        var t = action.Exists(a => a == item.itemId);
        var m = true;
        if (type == 5)
        {
            var key = new NoticeDefaultKey(NoticeType.Soul);
            m = moduleNotice.GetCacheRead(key);
        }
        else if (type == 6)
        {
            var key = new NoticeDefaultKey(NoticeType.Sublimation);
            m = moduleNotice.GetCacheRead(key);
        }
        else m = read.Exists(a => a == item.itemId);

        if (t && !m) return false;

        if (type == 4 && CheckDegreeElevation(item)) return true;
        else if (type == 5 && NeedNoticeSublimation(item)) return true;
        else if (type == 6 && NeedNoticeSoul(item)) return true;

        return false;
    }

    private bool CheckEquipHave(PItem item, Dictionary<ulong, int> action, Dictionary<ulong, int> read, int type)
    {
        if (action.ContainsKey(item.itemId) && !read.ContainsKey(item.itemId)) return false;

        if (type == 1 && CheckIntenty(item)) return true;
        else if (type == 2 && CheckEvolve(item)) return true;
        else if (type == 3 && CheckInSoul(item)) return true;

        return false;
    }

    private bool CheckNewHint()
    {
        for (int i = 0; i < moduleCangku.newProp .Count ; i++)
        {
            var info = moduleCangku.newProp[i].GetPropItem();
            if (info.itemType == PropType.Weapon || info.itemType == PropType.FashionCloth)
            {
                var have = EquipNewRead.Exists(a => a == moduleCangku.newProp[i].itemId);
                if (!have) return true;
            }
        }
        return false;
    }
    #endregion

    #endregion

}

#region custom class

#region evolve and intentify
[Serializable]
public class ItemAttachAttr
{
    /// <summary>
    /// 属性id
    /// </summary>
    public ushort id;
    /// <summary>
    /// 类型，1 普通值， 2 百分比
    /// </summary>
    public byte type;
    /// <summary>
    /// 属性值
    /// </summary>
    public double value;
    
    public static ItemAttachAttr operator +(ItemAttachAttr a, ItemAttachAttr b)
    {
        if(a != null && b != null && a.id == b.id && a.type == b.type)
            a.value += b.value;
        return a;
    }
    
    /// <summary>
    /// 计算成长值。把百分比转换为数值
    /// </summary>
    /// <param name="attr"></param>
    /// <returns></returns>
    public ItemAttachAttr CalcGrow(ItemAttachAttr attr)
    {
        if (null == attr || this.id != attr.id)
            return null;
        var att = new ItemAttachAttr();
        att.id = this.id;
        att.type = 1;
        if (attr.type == 2)
            att.value = this.value*attr.value;
        else
            att.value = attr.value;
        return att;
    }

    #region static functions
    public static ItemAttachAttr CalculateAttribute(ItemAttachAttr ori, ItemAttachAttr tar)
    {
        //new attribute
        if(ori == null && tar != null && tar.type == 1) return new ItemAttachAttr() { id = tar.id, type = 1, value = tar.value };

        if (!CanCalculateAttribute(ori, tar)) return null;

        var newAttr = new ItemAttachAttr();
        newAttr.id = ori.id;
        newAttr.type = 1;
        
        if (ori.type != tar.type) newAttr.value = ori.value * tar.value;
        else newAttr.value = ori.value + tar.value;
        return newAttr;
    }

    public static double CalculateAttributeValue(ItemAttachAttr ori, ItemAttachAttr tar)
    {
        //new attribute
        if (ori == null && tar != null && tar.type == 1)
        {
            double r = GeneralConfigInfo.IsPercentAttribute(tar.id) ? 100 : 1;
            return tar.value * r;
        }

        //ori and tar attirbute is based on  percent（type == 2）
        if (!CanCalculateAttribute(ori,tar)) return 0;

        double rate = GeneralConfigInfo.IsPercentAttribute(ori.id) ? 100 : 1;
        double value = 0;
        if (ori.type != tar.type) value = ori.value * tar.value * rate;
        else value = tar.value * rate;
        return value;
    }

    public static bool CanCalculateAttribute(ItemAttachAttr ori, ItemAttachAttr tar)
    {
        return ori != null && tar != null && ori.id == tar.id && !(ori.type == 2 && tar.type == 2);
    }
    #endregion
}

public abstract class IEDetailBase
{
    public EquipType type;
    public int minLevel { get; protected set; } = -1;
    public int maxLevel { get; protected set; } = -1;

    public IEDetailBase(EquipType t)
    {
        type = t;
    }

    public void SetLevel(int level)
    {
        if (minLevel == -1) minLevel = level;
        if (level < minLevel) minLevel = level;

        if (maxLevel == -1) maxLevel = level;
        if (level > maxLevel) maxLevel = level;
    }
}

public class IntentifyDetail : IEDetailBase
{
    public List<IntentifyEquipInfo> datas { get; set; } = new List<IntentifyEquipInfo>();

    public IntentifyDetail(EquipType t) : base(t)
    {
        datas.Clear();
    }

    public void AddConfigData(IntentifyEquipInfo d)
    {
        if (datas.Contains(d)) return;

        SetLevel(d.level);
        datas.Add(d);
    }

    public IntentifyEquipInfo GetMaxIntentyInfo()
    {
        return datas.Find(o=>o.level == maxLevel);
    }
}

public class EvolveDetail : IEDetailBase
{
    public List<EvolveEquipInfo> datas { get; set; } = new List<EvolveEquipInfo>();
    public EvolveDetail(EquipType t) : base(t)
    {
        datas.Clear();
    }

    public void AddConfigData(EvolveEquipInfo d)
    {
        if (datas.Contains(d)) return;

        SetLevel(d.level);
        datas.Add(d);
    }
}

/// <summary>
/// 属性预览变化 
/// </summary>
public class AttributePreviewDetail
{
    /// <summary>
    /// 属性id
    /// </summary>
    public ushort id;
    /// <summary>
    /// 
    /// </summary>
    public double oldValue;
    /// <summary>
    /// 属性值
    /// </summary>
    public double newValue;

    public AttributePreviewDetail(ushort id)
    {
        this.id = id;
    }

    public AttributePreviewDetail(ushort id,double oldV,double newV)
    {
        this.id = id;
        oldValue = oldV;
        newValue = newV;
        FixPercentAttribute();
    }
    
    public void FixPercentAttribute()
    {
        //修正百分比属性
        if(GeneralConfigInfo.IsPercentAttribute(id))
        {
            oldValue *= 100;
            newValue *= 100;
        }
    }

    public static List<AttributePreviewDetail> GetAllChangeAttributes(PItem ori, PItem other)
    {
        var l = new List<AttributePreviewDetail>();
        if (ori == null || other == null) return l;

        List<ushort> ids = new List<ushort>();
        ids.AddRange(ori.GetAllFixedAttributrIds());
        List<ushort> oi = other.GetAllFixedAttributrIds();
        foreach (var item in oi)
        {
            if (!ids.Contains(item)) ids.Add(item);
        }

        foreach (var id in ids)
        {
            double oldV = ori.GetFixedAttirbute(id);
            double newV = other.GetFixedAttirbute(id);
            if (oldV <= 0 && newV <= 0) continue;

            l.Add(new AttributePreviewDetail(id, oldV, newV));
        }

        //添加入魂的对比
        var oriLevel = ori.InvalidGrowAttr() ? 0 : ori.growAttr.equipAttr.level;
        var otherLevel = other.InvalidGrowAttr() ? 0 : other.growAttr.equipAttr.level;
        double oriValue = oriLevel > 0 ? Module_Forging.instance.Insoul_Info[oriLevel].attribute : 0;
        double otherValue = otherLevel > 0 ? Module_Forging.instance.Insoul_Info[otherLevel].attribute : 0;
        if(oriValue > 0 || otherValue > 0) l.Add(new AttributePreviewDetail(18, oriValue, otherValue));

        return l;
    }
}
#endregion

#endregion
