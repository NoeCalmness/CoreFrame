// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-23      19:57
//  * LastModify：2018-08-23      19:58
//  ***************************************************************************************************/

#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class Window_ItemTip : Window
{
    private int m_dii = 0, m_dpi = 0, m_dpc = 0; // 被分解的道具 ID 得到的碎片 ID 以及碎片数量

    #region static functions

    private static List<PItem2> datas;
    private static List<ItemPair> items;
    private static string title;

    private static PItem2 GetPItem2(ushort _itemTypeId, uint _num, byte _level = 0, byte _star = 1, ushort _source = 0)
    {
        var item = PacketObject.Create<PItem2>();
        item.itemTypeId = _itemTypeId;
        item.num = _num;
        item.level = _level;
        item.star = _star;
        item.source = _source;

        return item;
    }

    public static void Show(string rTitle, PReward reward, bool doubleReward = false)
    {
        List<PItem2> list = new List<PItem2>();
        var rReward = reward.Clone();
        if (rReward.diamond > 0)
        {
            var p = PacketObject.Create<PItem2>();
            p.itemTypeId = 2;
            p.star = 1;
            p.level = 0;
            p.num = (uint)(rReward.diamond * (doubleReward ? 2 : 1));
            list.Add(p);
        }

        if (rReward.coin > 0)
        {
            var p = PacketObject.Create<PItem2>();
            p.itemTypeId = 1;
            p.star = 1;
            p.level = 0;
            p.num = (uint)(rReward.coin * (doubleReward ? 2 : 1));
            list.Add(p);
        }

        if (rReward.fatigue > 0)
        {
            var p = PacketObject.Create<PItem2>();
            p.itemTypeId = 15;
            p.star = 1;
            p.level = 0;
            p.num = (uint)(rReward.fatigue * (doubleReward ? 2 : 1));
            list.Add(p);
        }
        if (rReward.activePoint > 0)
        {
            var p = PacketObject.Create<PItem2>();
            p.itemTypeId = 14;
            p.star = 1;
            p.level = 0;
            p.num = (uint)(rReward.activePoint * (doubleReward ? 2 : 1));
            list.Add(p);
        }

        list.AddRange(rReward.rewardList);
        if (doubleReward)
            list.AddRange(rReward.rewardList);

        Show(rTitle, list);
    }

    public static void Show(string rTitle, List<PItem2> rList)
    {
        datas = rList;
        title = rTitle;
        ShowAsync<Window_ItemTip>();
    }

    public static void Show(string rTitle, params PItem2[] rItems)
    {
        List<PItem2> list = new List<PItem2>();
        list.AddRange(rItems);
        Show(rTitle, list);
    }

    public static void Show(string rTitle, List<ItemPair> rList)
    {
        items = rList;
        title = rTitle;
        ShowAsync<Window_ItemTip>();
    }

    public static void Show(string rTitle, PShopItem item)
    {
        if (item == null) return;
        title = rTitle;
        datas = new List<PItem2>();
        datas.Add(GetPItem2(item.itemTypeId, item.num));
        ShowAsync<Window_ItemTip>();
    }

    public static void Show(string rTitle, List<ushort> itemTypeId)
    {
        if (itemTypeId == null || itemTypeId.Count < 1) return;
        title = rTitle;
        datas = new List<PItem2>();
        for (int i = 0; i < itemTypeId.Count; i++)
            datas.Add(GetPItem2(itemTypeId[i], 1));

        ShowAsync<Window_ItemTip>();
    }

    #endregion

    private Button    clickButton;
    private Transform itemRoot;
    private Transform itemTemplete;
    private readonly Text[]    titleText = new Text[4];
    private List<Transform> itemCache = new List<Transform>();

    protected override void OnOpen()
    {
        base.OnOpen();
        InitComponent();
        isFullScreen = false;
    }
    protected void InitComponent()
    {
        clickButton  = GetComponent<Button>();
        itemRoot     = GetComponent<Transform>("content");
        itemTemplete = GetComponent<Transform>("0");
        titleText[0] = GetComponent<Text>     ("content/back/title/up_h/up_1");
        titleText[1] = GetComponent<Text>     ("content/back/title/up_h/up_2");
        titleText[2] = GetComponent<Text>     ("content/back/title/up_h/up_3");
        titleText[3] = GetComponent<Text>     ("content/back/title/up_h/up_4");

        clickButton?.onClick.RemoveAllListeners();
        clickButton?.onClick.AddListener(() => { Hide(); });
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        m_dpi = m_dii = m_dpc = 0;

        itemTemplete.SafeSetActive(false);
        CreateItems(datas);
        CreateItems(items);

        for (int i = 0; i < titleText.Length; i++)
        {
            if (titleText[i] != null && title.Length > i)
                Util.SetText(titleText[i], title.Substring(i, 1));
        }
    }


    protected override void OnHide(bool forward)
    {
        if (datas != null)
        {
            foreach (var data in datas)
                data.Destroy();
        }
        datas?.Clear();
        items?.Clear();
        title = string.Empty;
        ClearItemCache();

        var i = m_dii; m_dii = 0;
        if (i > 0) moduleGlobal.ShowItemDecomposeInfo(i, m_dpi, m_dpc);  // 如果获得了分解道具

    }

    private void ClearItemCache()
    {
        foreach (var t in itemCache)
            Object.Destroy(t.gameObject);
        itemCache.Clear();
    }

    private void CreateItems(List<PItem2> rList)
    {
        if (rList == null || rList.Count == 0)
            return;

        ClearItemCache();
        Sort(rList);
        for (var i = 0; i < rList.Count; i++)
        {
            var item = rList[i];
            if (item.num <= 0) continue;
            var itemInfo = PropItemInfo.Get(item.itemTypeId);
            if (!itemInfo || !itemInfo.IsValidVocation(modulePlayer.proto)) continue;

            var t = itemRoot.AddNewChild(itemTemplete);
            t.SafeSetActive(true);
            itemCache.Add(t);
            Util.SetItemInfo(t, itemInfo, item.level, (int)item.num, true, item.star);
        }
    }

    private void Sort(List <PItem2 > rList)
    {
        rList.Sort((l, r) =>
          1000 * ((PropItemInfo.Get(r.itemTypeId) != null ? PropItemInfo.Get(r.itemTypeId).quality  : 0).CompareTo(PropItemInfo.Get(l.itemTypeId) != null ? PropItemInfo.Get(l.itemTypeId).quality : 0)) +
           100 * ((PropItemInfo.Get(l.itemTypeId) != null ? PropItemInfo.Get(l.itemTypeId).itemType  : 0).CompareTo(PropItemInfo.Get(r.itemTypeId) != null ? PropItemInfo.Get(r.itemTypeId).itemType  : 0)) +
            10 *((PropItemInfo.Get(l.itemTypeId)!=null ? PropItemInfo.Get(l.itemTypeId).subType : 0).CompareTo(PropItemInfo.Get(r.itemTypeId)!=null ? PropItemInfo.Get(r.itemTypeId).subType : 0))+
                 l.itemTypeId .CompareTo (r.itemTypeId));
    }

    private void CreateItems(List<ItemPair> rList)
    {
        if (rList == null || rList.Count == 0)
            return;

        for (var i = 0; i < rList.Count; i++)
        {
            var item = rList[i];
            if (item.count <= 0) continue;
            var itemInfo = PropItemInfo.Get((ushort)item.itemId);
            if (!itemInfo || !itemInfo.IsValidVocation(modulePlayer.proto)) continue;

            var t = itemRoot.AddNewChild(itemTemplete);
            t.SafeSetActive(true);
            itemCache.Add(t);

            Util.SetItemInfoSimple(t, itemInfo);
            Util.SetText(t.GetComponent<Text>("numberdi/count"), item.count.ToString());
        }
    }

    //获得了可分解道具
    void _ME(ModuleEvent<Module_Welfare> e)
    {
        if (e.moduleEvent == Module_Welfare.EventItemDecompose)
        {
            if (!actived) return;

            var msg = e.msg as ScEquipWeaponDecompose;
            if (msg == null) return;

            m_dii = msg.weaponId;
            m_dpi = msg.pieceId;
            m_dpc = msg.pieceNum;
        }
    }
    
}