/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-12-21
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class ShopMessage
{
    public ushort shopId { get; protected set; }
    public string icon { get; protected set; }
    public int type { get; protected set; }
    public bool isRandom { get; protected set; }
    public ShopPos pos { get; protected set; }
    public string name { get; protected set; }
    public uint refreshCount { get; protected set; }
    public ushort refreshCurrency { get; protected set; }
    public ushort refreshNum { get; protected set; }
    public uint nextRefreshTime { get; protected set; }
    public PShopItem[] items { get; protected set; }

    public static implicit operator ShopMessage(PShopInfo p)
    {
        return new ShopInfo(p);
    }

    public static implicit operator ShopMessage(ScRandomShopInfo p)
    {
        return new RandomShopInfo(p);
    }
}

public class ShopInfo : ShopMessage
{
    public ShopInfo(PShopInfo info)
    {
        shopId          = info.shopId;
        icon            = info.icon;
        type            = info.type;
        isRandom        = info.type == 2;
        pos             = (ShopPos)info.pos;
        name            = Util.GetString((int)info.nameId);
        refreshCount    = info.refreshCount;
        refreshCurrency = info.refreshCurrencyType;
        refreshNum      = info.refreshCurrencyNum;
        nextRefreshTime = info.nextRefreshTimeSec;
        items           = info.itemList;
    }
}

public class RandomShopInfo : ShopMessage
{
    public RandomShopInfo(ScRandomShopInfo info)
    {
        shopId = info.shopId;
        type = 2;
        isRandom = true;

        var msg = Module_Shop.instance.GetTargetShop(info.shopId);
        if (msg != null)
        {
            icon = msg.icon;
            pos = msg.pos;
            name = msg.name;
        }
        refreshCount    = info.refreshCount;
        refreshCurrency = info.refreshCurrencyType;
        refreshNum      = info.refreshCurrencyNum;
        nextRefreshTime = info.nextRefreshTimeSec;
        items           = info.itemList;
    }
}

public class Module_Shop : Module<Module_Shop>
{
    #region EVENT
    /// <summary>所有商店数据</summary>
    public const string EventShopData = "EventShopData";
    /// <summary>一个指定商店数据</summary>
    public const string EventTargetShopData = "EventTargetShopData";
    /// <summary>购买成功</summary>
    public const string EventPaySuccess = "EventPaySuccess";
    /// <summary>购买失败</summary>
    public const string EventPayFailed = "EventPayFailed";
    /// <summary>促销改变</summary>
    public const string EventPromoChanged = "EventPromoChanged";
    /// <summary>促销改变</summary>
    public const string EventNpcChangeEquip = "EventNpcChangeEquip";
    /// <summary>自由购买道具响应</summary>
    public const string ResponseFreeBuy = "ResponseFreeBuy";
    #endregion

    /// <summary>所有商店信息</summary>
    public Dictionary<ShopPos, List<ShopMessage>> allShops { get { return m_allShops; } }
    private Dictionary<ShopPos, List<ShopMessage>> m_allShops = new Dictionary<ShopPos, List<ShopMessage>>();

    /// <summary>所有促销信息</summary>
    public Dictionary<int, PShopPromotion> allPromo { get { return m_allPromo; } }
    private Dictionary<int, PShopPromotion> m_allPromo = new Dictionary<int, PShopPromotion>();

    #region 公共

    /// <summary>设置当前商店的类型,并刷新当前类型下的所有商店</summary>
    public ShopPos curShopPos { get { return m_curShopPos; } }
    private ShopPos m_curShopPos;

    /// <summary>当前选中的商店 </summary>
    public ShopMessage curShopMsg { get { return m_curShopMsg; } }
    private ShopMessage m_curShopMsg;

    /// <summary>当前选中的商品 </summary>
    public PShopItem curClickItem { get; set; }
    /// <summary>上次选中的商品 </summary>
    public PShopItem lastClickItem { get; set; }
    /// <summary>当前商品列表 </summary>
    public PShopItem[] curItmes { get; private set; }

    /// <summary>上一次选中的商店 </summary>
    private ShopMessage lastShopMsg;
    #endregion

    #region 时装店相关数据

    /// <summary>时装店数据</summary>
    public Dictionary<FashionType, List<PShopItem>> fashionShop { get { return m_FashionShop; } }
    private Dictionary<FashionType, List<PShopItem>> m_FashionShop = new Dictionary<FashionType, List<PShopItem>>();

    /// <summary>时装店选中的标签类型</summary>
    public FashionType curFashionType { get; set; }
    /// <summary>是否换装</summary>
    public bool changedCloth { get; private set; } = false;

    private List<PItem> m_changeFashion = new List<PItem>();

    #endregion

    #region 随机商店的数据

    public Dictionary<ushort, float> receiveTime { get { return m_receiveTime; } }
    private Dictionary<ushort, float> m_receiveTime = new Dictionary<ushort, float>();

    #endregion

    #region NPC商店的数据

    /// <summary>各个NPC对应的装备数据</summary>
    public Dictionary<int, List<PShopItem>> npcShop { get { return m_npcShop; } }
    private Dictionary<int, List<PShopItem>> m_npcShop = new Dictionary<int, List<PShopItem>>();

    #endregion

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        SendTypeShop();       
    }

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        m_curShopPos = ShopPos.None;
        curFashionType = FashionType.None;
        m_allShops.Clear();
        m_allPromo.Clear();
        m_FashionShop.Clear();
        m_changeFashion.Clear();
        m_receiveTime.Clear();
        curClickItem = null;
        lastClickItem = null;
        m_curShopMsg = null;
        changedCloth = false;
        curItmes?.Clear();
        m_npcShop.Clear();
        lastShopMsg = null;
    }

    public void SetCurShopPos(ShopPos pos, bool dispatch = true)
    {
        m_curShopPos = pos;
        Logger.LogDetail("当前商店的位置={0}", m_curShopPos.ToString());
        if (dispatch && m_allShops.ContainsKey(m_curShopPos))
            DispatchModuleEvent(EventShopData, m_allShops[m_curShopPos]);
    }

    public void SetCurrentShop(ShopMessage msg)
    {
        if (msg == null) return;
        lastShopMsg = m_curShopMsg;
        m_curShopMsg = msg;
        curItmes = msg.items;
        Logger.LogDetail("当前商店的ID={0},位置={1},", m_curShopMsg.shopId, m_curShopMsg.pos);
        DispatchModuleEvent(EventTargetShopData, m_curShopMsg);
    }

    public void HideGiftToDo()
    {
        SetCurrentShop(lastShopMsg);
    }

    public ShopMessage GetTargetShop(ushort _shopId)
    {
        ShopMessage msg = null;
        foreach (var item in m_allShops)
        {
            var list = item.Value;
            msg = list.Find(p => p.shopId == _shopId);
            if (msg != null) return msg;
        }

        return null;
    }

    #region 商店相关通讯

    /// <summary>
    /// 发送商店类型()
    /// </summary>
    /// <param name="type"></param>
    public void SendTypeShop()
    {
        CsShopInfoAll p = PacketObject.Create<CsShopInfoAll>();
        session.Send(p);
    }

    void _Packet(ScShopInfoAll p)
    {
        if (p.shops == null || p.shops.Length < 1) return;

        ScShopInfoAll info = null;
        p.CopyTo(ref info);

        for (int i = 0; i < info.shops.Length; i++)
        {
            var msg = (ShopMessage)info.shops[i];
            if (msg == null) continue;

            if (!m_allShops.ContainsKey(msg.pos)) m_allShops.Add(msg.pos, new List<ShopMessage>());
            m_allShops[msg.pos].Add(msg);

            //加到时装商店的字典里
            if (msg.pos == ShopPos.Fashion) OnAddFashionData(msg.items);

            //加随机商店的时间
            if (msg.isRandom)
            {
                if (!m_receiveTime.ContainsKey(msg.shopId)) m_receiveTime.Add(msg.shopId, 0);
                m_receiveTime[msg.shopId] = Time.realtimeSinceStartup;
            }

            //加到NPC商店的字典里
            if (msg.pos == ShopPos.Npc) OnAddNpcShopData(msg.items);
        }
        
        //流浪商店排序
        if (m_allShops.ContainsKey(ShopPos.Traval))
        {
            m_allShops[ShopPos.Traval].Sort((a, b) =>
            {
                int result = b.type.CompareTo(a.type);
                if (result == 0)
                {
                    return a.shopId.CompareTo(b.shopId);
                }
                return result;
            });
        }       

        //请求促销数据
        SendPromotion();
    }

    private void OnAddFashionData(PShopItem[] items)
    {
        if (items == null && items.Length < 1) return;

        m_FashionShop.Clear();
        for (int i = 0; i < items.Length; i++)
        {
            var info = ConfigManager.Get<PropItemInfo>(items[i].itemTypeId);
            if (!info || info.itemType != PropType.FashionCloth || !info.IsValidVocation(modulePlayer.proto) 
                || !(items[i].sex == modulePlayer.gender || items[i].sex == 2)) continue;

            switch (info.subType)
            {
                case (int)FashionSubType.HeadDress: AddFashionData(FashionType.HeadDress, items[i]); break;
                case (int)FashionSubType.HairDress: AddFashionData(FashionType.HairDress, items[i]); break;
                case (int)FashionSubType.FaceDress: AddFashionData(FashionType.FaceDress, items[i]); break;
                case (int)FashionSubType.NeckDress: AddFashionData(FashionType.NeckDress, items[i]); break;
                default: AddFashionData(FashionType.Cloth, items[i]); break;
            }
        }
    }

    private void AddFashionData(FashionType type, PShopItem item)
    {
        if (!m_FashionShop.ContainsKey(type)) m_FashionShop.Add(type, new List<PShopItem>());
        m_FashionShop[type].Add(item);
    }
    
    private void OnAddNpcShopData(PShopItem[] items)
    {
        if (items == null || items.Length < 1) return;

        var npcs = moduleNpc.allNpcs;
        if (npcs == null || npcs.Count < 1) return;

        m_npcShop.Clear();
        for (int i = 0; i < npcs.Count; i++)
        {
            for (int k = 0; k < items.Length; k++)
            {
                var prop = ConfigManager.Get<PropItemInfo>(items[k].itemTypeId);
                if (prop == null || prop.itemType != PropType.NpcFashion || prop.subType != npcs[i].npcId) continue;

                if (!m_npcShop.ContainsKey(npcs[i].npcId)) m_npcShop.Add(npcs[i].npcId, new List<PShopItem>());
                m_npcShop[npcs[i].npcId].Add(items[k]);
            }

            if (!m_npcShop.ContainsKey(npcs[i].npcId)) continue;
            m_npcShop[npcs[i].npcId].Sort((a, b) =>
            {
                var c = ConfigManager.Get<PropItemInfo>(a.itemTypeId);
                var d = ConfigManager.Get<PropItemInfo>(b.itemTypeId);

                if (c == null || d == null) return 0;

                var result = 0;
                result = c.quality.CompareTo(d.quality);
                if (result == 0) result = c.ID.CompareTo(d.ID);

                return result;
            });

            var npc = moduleNpc.GetTargetNpc((NpcTypeID)npcs[i].npcId);
            if (npc == null) continue;

            var equip = m_npcShop[npcs[i].npcId].Find(p => p.itemTypeId == npc.npcInfo.cloth);
            if (equip == null) continue;

            m_npcShop[npcs[i].npcId].Remove(equip);
            m_npcShop[npcs[i].npcId].Insert(0, equip);
        }
    }

    /// <summary>
    /// 刷新商店 shopid=商店的id
    /// </summary>
    /// <param name="shopid"></param>
    public void SendRefreshShop(ushort _shopid)
    {
        CsRefreshShop p = PacketObject.Create<CsRefreshShop>();
        p.shopId = _shopid;
        session.Send(p);
    }

    void _Packet(ScRefreshShop p)
    {
        if (p.result == 0)
        {
            AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.TravalShopUIText, 7));

            return;
        }

        moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.TravalShopUIText, 8));
    }

    /// <summary>
    /// 刷新随机商店
    /// </summary>
    /// <param name="p"></param>
    void _Packet(ScRandomShopInfo p)
    {
        if (p == null) return;

        ScRandomShopInfo info = null;
        p.CopyTo(ref info);

        var msg = (ShopMessage)info;
        if (msg == null) return;

        foreach (var item in m_allShops)
        {
            var idx = item.Value.FindIndex(o => o.shopId == info.shopId);
            if (idx > -1)
            { 
                item.Value[idx] = msg;
            }
        }

        //记录刷新时间
        if (m_receiveTime.ContainsKey(msg.shopId)) m_receiveTime[msg.shopId] = Time.realtimeSinceStartup;
        else m_receiveTime.Add(msg.shopId, Time.realtimeSinceStartup);

        SetCurrentShop(msg);
    }

    /// <summary>
    /// 发送促销
    /// </summary>
    public void SendPromotion()
    {
        CsShopPromotion p = PacketObject.Create<CsShopPromotion>();
        session.Send(p);
    }

    void _Packet(ScShopPromotion p)
    {
        ScShopPromotion promo = null;
        p.CopyTo(ref promo);

        var list = p.promotionList;
        if (list == null || list.Length < 1) return;

        m_allPromo.Clear();
        for (int i = 0; i < list.Length; i++)
        {
            int hash = GetPromoHash(list[i].shopId, list[i].itemTypeId, list[i].num);
            if (m_allPromo.ContainsKey(hash)) m_allPromo.Remove(hash);

            m_allPromo.Add(hash, list[i]);
        }

        for (int i = 0; i < list.Length; i++)
        {
            var msg = GetTargetShop(list[i].shopId);
            if (msg == null) continue;

            if (msg.pos == ShopPos.Fashion)
            {
                OnAddFashionDiscountData();
                break;
            }
        }
    }

    /// <summary>
    /// 添加到时装店的折扣标签里
    /// </summary>
    /// <param name="shopId"></param>
    private void OnAddFashionDiscountData()
    {
        var list = m_allShops.ContainsKey(ShopPos.Fashion) ? m_allShops[ShopPos.Fashion] : new List<ShopMessage>();
        if (list.Count < 0) return;

        //往折扣标签里面添数据
        if (!m_FashionShop.ContainsKey(FashionType.Limited)) m_FashionShop.Add(FashionType.Limited, new List<PShopItem>());
        m_FashionShop[FashionType.Limited].Clear();

        for (int i = 0; i < list.Count; i++)
        {
            var msg = list[i];
            if (msg == null || msg.items == null || msg.items.Length < 1) continue;

            for (int k = 0; k < msg.items.Length; k++)
            {
                int _hash = GetPromoHash(msg.shopId, msg.items[k].itemTypeId, msg.items[k].num);
                if (m_allPromo.ContainsKey(_hash))
                    m_FashionShop[FashionType.Limited].Add(msg.items[k]);
            }
        }
    }

    public int GetPromoHash(ushort shopId, ushort itemTypeId, ushort num)
    {
        var str = shopId + itemTypeId + num;
        return str.GetHashCode();
    }

    void _Packet(ScShopPromotionStart p)
    {
        if (p == null || p.promotion == null) return;
        PShopPromotion start = null;
        p.promotion.CopyTo(ref start);

        int hash = GetPromoHash(start.shopId, start.itemTypeId, start.num);
        if (m_allPromo.ContainsKey(hash)) m_allPromo[hash] = start;
        else m_allPromo.Add(hash, start);

        var msg = GetTargetShop(start.shopId);
        if (msg == null) return;

        if (msg.pos == ShopPos.Fashion) OnAddFashionDiscountData();
        DispatchModuleEvent(EventPromoChanged, msg);
    }

    void _Packet(ScShopPromotionEnded p)
    {
        if (p == null) return;
        int hash = GetPromoHash(p.shopId, p.itemTypeId, p.num);

        if (!m_allPromo.ContainsKey(hash)) return;
        m_allPromo.Remove(hash);

        var msg = GetTargetShop(p.shopId);
        if (msg == null) return;

        if (msg.pos == ShopPos.Fashion) OnAddFashionDiscountData();

        DispatchModuleEvent(EventPromoChanged, msg);
    }

    /// <summary>
    /// 购买商品
    /// </summary>
    public void SendBuyInfo(ushort shopID, ushort itemID, uint numb)
    {
        CsShopBuy p = PacketObject.Create<CsShopBuy>();
        p.shopId = shopID;
        p.itemTypeId = itemID;
        p.num = numb;
        session.Send(p);
    }

    void _Packet(ScShopBuy p)
    {
        // 0 = 成功, 1 = 商品不存在， 2 = money不够, 3 = 商品已经购买
        if (p.result == 0)
        {
            ChangeCloth(p.itemId, p.itemTypeId);
            if (curClickItem != null && m_curShopMsg != null)
            {
                var shopItme = m_curShopMsg.items.Find(o => o.itemTypeId == curClickItem.itemTypeId && o.num == curClickItem.num);
                if (shopItme != null) shopItme.buy = 1;
            }

            AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
            DispatchModuleEvent(EventPaySuccess);
        }
        else
        {
            switch (p.result)
            {
                case 1: moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.PublicUIText, 8)); break;
                case 2:
                    {
                        if (curClickItem == null) break;

                        var prop = ConfigManager.Get<PropItemInfo>(curClickItem.currencyType);
                        if (prop) moduleGlobal.ShowMessage(Util.Format(ConfigText.GetDefalutString(TextForMatType.PublicUIText, 31), prop.itemName));
                        break;
                    }
                case 3: moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.PublicUIText, 10)); break;
                case 4: moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.PublicUIText, 12)); break;
                default: break;
            }

            DispatchModuleEvent(EventPayFailed);
        }
    }

    public void ChangeCloth(ulong itemId, int itemTypeId)
    {
        PropItemInfo prop = ConfigManager.Get<PropItemInfo>(itemTypeId);
        if (prop == null || prop.itemType != PropType.FashionCloth) return;

        List<ulong> put = new List<ulong>();
        List<ulong> drop = new List<ulong>();

        PItem have = moduleEquip.currentDressClothes.Find((p) =>
        {
            var _info = p.GetPropItem();
            if (!_info) return false;
            return _info.itemType == PropType.FashionCloth && prop.subType == _info.subType;
        });

        if (have != null)drop.Add(itemId);
        put.Add(itemId);

        if (put.Count > 0) moduleEquip.SendChangeClothes(put);
        if (drop.Count > 0) moduleEquip.SendTakeOffClothes(drop);
    }

    #endregion

    #region 时装店换装

    public void OnAddDefaultData()
    {
        m_changeFashion.Clear();
        m_changeFashion.AddRange(moduleEquip.currentDressClothes);
    }
    
    public void ChangeNewFashion(PShopItem item, Creature creature)
    {
        PItem newItem = moduleEquip.GetNewPItem(item.itemTypeId, item.itemTypeId);
        PropItemInfo info = ConfigManager.Get<PropItemInfo>(newItem.itemTypeId);

        PItem containItem = m_changeFashion.Find((p) =>
        {
            var _info = p.GetPropItem();
            if (!_info) return false;
            return _info.itemType == PropType.FashionCloth && info.subType == _info.subType;
        });
        if (containItem != null)
        {
            m_changeFashion.Remove(containItem);
            m_changeFashion.Add(newItem);
        }
        else
            m_changeFashion.Add(newItem);

        CharacterEquip.ChangeCloth(creature, m_changeFashion);
        changedCloth = true;
    }

    public void ChangeDefaultCloth(Creature creature)
    {
        CharacterEquip.ChangeCloth(creature, moduleEquip.currentDressClothes);
        OnAddDefaultData();
        changedCloth = false;
    }

    #endregion

    #region 流浪商店 
    public int GetRefreshTime(ushort id)
    {
        if (!m_receiveTime.ContainsKey(id)) return -1;

        var msg = GetTargetShop(id);
        if (msg == null) return -1;

        var nextTime = msg.nextRefreshTime;
        var reTime = m_receiveTime.Get(id);

        int time = (int)(nextTime - (Time.realtimeSinceStartup - reTime));
        return time > 0 ? time : 0;
    }

    #endregion

    #region NPC商店

    public void SendChangeNpcEquip(ushort npcId, ulong _itemId)
    {
        CsNpcChangeEquip p = PacketObject.Create<CsNpcChangeEquip>();
        p.npcId = npcId;
        p.itemId = _itemId;

        session.Send(p);
    }

    void _Packet(ScNpcChangeEquip p)
    {
        if (p.result == 0)
        {
            moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.GiftUIText, 19));
            if (moduleNpc.curNpc != null) moduleNpc.curNpc.UpdateMode(moduleShop.curClickItem.itemTypeId);
        }
        else
            moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.GiftUIText, 20));

        DispatchModuleEvent(EventNpcChangeEquip, p.result);
    }

    #endregion

    #region 购买协议

    public void FreeBuy(ushort rItemTypeId, uint rCount)
    {
        var p = PacketObject.Create<CsFreeBuy>();
        p.itemTypeId = rItemTypeId;
        p.num = rCount;
        session.Send(p);
    }

    private void _Packet(ScFreeBuy msg)
    {
        DispatchModuleEvent(ResponseFreeBuy, msg);
    }

    #endregion

}
