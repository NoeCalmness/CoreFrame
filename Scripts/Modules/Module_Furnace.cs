// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-10      11:03
//  * LastModify：2018-10-10      19:43
//  ***************************************************************************************************/
#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

public enum FurnaceType
{
    Decompose,
    Sublimation,
    Soul
}

public class DecomposePair
{
    public ushort count;
    public PItem  item;

    public static PDecomposePair[] Convert(List<DecomposePair> rList)
    {
        var arr = new PDecomposePair[rList.Count];
        var i = 0;
        foreach (var pair in rList)
        {
            var m = PacketObject.Create<PDecomposePair>();
            m.id = pair.item.itemId;
            m.count = pair.count;
            arr[i++] = m;
        }
        return arr;
    }
}

public class Module_Furnace : Module<Module_Furnace>
{
    #region Events

    public const string ResponseDecomposeItem       = "ResponseDecomposeItem";
    public const string ResponseComfirmSoul         = "ResponseComfirmSoul";
    public const string ResponseSoul                = "ResponseSoul";
    public const string ResponseSublimationExcute   = "ResponseSublimationExcute";
    public const string ResponseSublimationClear    = "ResponseSublimationClear";

    #endregion

    #region Request

    public void RequestComposeItem(List<DecomposePair> rItemIds)
    {
        var p = PacketObject.Create<CsItemDecompose>();
        p.composeArr = DecomposePair.Convert(rItemIds);
        session.Send(p);
    }

    public void RequestApplySoul(bool rApply)
    {
        var msg = PacketObject.Create<CsEquipSoulComfirm>();
        msg.itemId = currentSoulItem.itemId;
        msg.isApply = rApply;
        session.Send(msg);
    }

    public void RequestSoul()
    {
        moduleGlobal.LockUI(0.5f, 0, 0x02);
        var msg = PacketObject.Create<CsEquipSoul>();
        msg.itemId = currentSoulItem.itemId;
        session.Send(msg);
    }

    public void RequestSublimationExcute(ulong rDrawingId)
    {
        var msg = PacketObject.Create<CsSublimationExcute>();
        msg.itemId = currentSublimationItem?.itemId ?? 0;
        msg.drawingId = rDrawingId;
        session.Send(msg);
    }

    public void RequestSublimationClear(ulong rItemId)
    {
        var msg = PacketObject.Create<CsSublimationClear>();
        msg.itemId = rItemId;
        session.Send(msg);
    }

    #endregion

    #region Packet

    private void _Packet(ScItemDecompose msg)
    {
        DispatchModuleEvent(ResponseDecomposeItem, msg);
    }

    private void _Packet(ScEquipSoulComfirm msg)
    {
        DispatchModuleEvent(ResponseComfirmSoul, msg);
    }

    private void _Packet(ScEquipSoul msg)
    {
        DispatchModuleEvent(ResponseSoul, msg);
        moduleGlobal.UnLockUI();
    }

    private void _Packet(ScSublimationExcute msg)
    {
        DispatchModuleEvent(ResponseSublimationExcute, msg);
    }

    private void _Packet(ScSublimationClear msg)
    {
        DispatchModuleEvent(ResponseSublimationClear, msg);
    }

    #endregion

    #region private functions

    protected override void OnEnterGame()
    {
        OnGameDataReset();
    }

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        QualityHintStateKey = "QualityHintState" + modulePlayer.id_;
        if (PlayerPrefs.HasKey(QualityHintStateKey))
            hintMask = PlayerPrefs.GetInt(QualityHintStateKey);
        else
            hintMask = 0xFF & ~1;
    }

    private int SublimationCompareHandle(PItem a, PItem b)
    {
        var aDress = moduleEquip.currentDressClothes.Exists(item => item.itemId == a.itemId);
        var bDress = moduleEquip.currentDressClothes.Exists(item => item.itemId == b.itemId);

        if (aDress == bDress)
            return -DecomposeCompareHandle(a, b);

        return aDress.CompareTo(bDress);
    }

    private int DecomposeCompareHandle(PItem a, PItem b)
    {
        var p1 = a.GetPropItem();
        var p2 = b.GetPropItem();
        return p1.quality.CompareTo(p2.quality);
    }

    #endregion

    #region Properties

    public PItem currentSublimationItem { get; set; }
    public PItem currentSoulItem { get; set; }

    #endregion

    #region Fields

    private int hintMask;
    public string QualityHintStateKey = "QualityHintState";

    #endregion

    #region public functions

    public void SetQualityHintState(int rQuality, bool rState)
    {
        if (rQuality <= 0)
            return;

        if(rState)
            hintMask |= 1 << (rQuality - 1);
        else
            hintMask = hintMask & ~(1 << (rQuality - 1));
    }

    public void SaveHintMask()
    {
        PlayerPrefs.SetInt(QualityHintStateKey, hintMask);
    }

    public bool GetQualityHintState(int rQuality)
    {
        if (rQuality <= 0)
            return false;
        return (hintMask & (1 << (rQuality - 1))) > 0;
    }

    public bool IsNeedQualityHint(List<PropItemInfo> rList)
    {
        foreach (var rProp in rList)
        {
            if (GetQualityHintState(rProp.quality))
                return true;
        }
        return false;
    }

    public bool IsNeedQualityHint(List<DecomposePair> rItemIdArr)
    {
        if (null == rItemIdArr || rItemIdArr.Count == 0)
            return false;
        for (var i = 0; i < rItemIdArr.Count; i++)
        {
            var item = moduleEquip.GetProp(rItemIdArr[i].item.itemId);
            if (null == item) continue;
            var prop = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
            if (null == prop) continue;
            if (GetQualityHintState(prop.quality))
                return true;
        }
        return false;
    }

    public List<PItem> GetAllItems(FurnaceType rType, Predicate<PItem> match = null)
    {
        List<PItem> list = null;
        switch (rType)
        {
            case FurnaceType.Decompose:
                if (match == null)
                    match = item => !moduleEquip.currentDressClothes.Exists(t => t.itemId == item.itemId);;
                list = moduleEquip.GetPropItems(item => item.GetPropItem().decompose > 0 && match(item));
                list.Sort(DecomposeCompareHandle);
                return list;

            case FurnaceType.Sublimation:
                list = moduleEquip.GetPropItems(item => item.GetPropItem().sublimation);
                list.Sort(SublimationCompareHandle);
                return list;

            case FurnaceType.Soul:
                list = moduleEquip.GetPropItems(item => item.GetPropItem().soulable);
                list.Sort(SublimationCompareHandle);
                return list;
        }
        return new List<PItem>();
    }

    /// <summary>
    /// 获得所有当前道具可使用的图纸
    /// </summary>
    /// <param name="rItem"></param>
    /// <returns></returns>
    public List<PItem> GetAllCanUseDrawing(PItem rItem)
    {
        var soulInfo = rItem == null ? null : ConfigManager.Get<SuitInfo>(rItem.growAttr.suitId);
        if(null != soulInfo)
            return moduleEquip.GetPropItems(item => Array.Exists(soulInfo.nextSuitId, suitId => suitId == item?.itemTypeId));

        var list = moduleEquip.GetBagProps(PropType.Drawing);
        list.RemoveAll(item =>
        {
            var suitInfo = ConfigManager.Find<SuitInfo>(t => t.drawingId == item.itemTypeId);
            return suitInfo?.prevSuitId != 0;
        });
        return list;
    }

    /// <summary>
    /// 获得所有图纸列表
    /// </summary>
    /// <param name="rItem"></param>
    /// <returns></returns>
    public List<ItemPair> GetAllDrawingList(PItem rItem)
    {
        List<ItemPair> result = new List<ItemPair>();
        var soulInfo = rItem == null ? null : ConfigManager.Get<SuitInfo>(rItem.growAttr.suitId);
        List<PropItemInfo> propList = null;
        if (null != soulInfo)
        {
            propList =
                ConfigManager.FindAll<PropItemInfo>(
                    info => Array.Exists(soulInfo.nextSuitId, suitId => suitId == info.ID));
        }
        else
        {
            propList = ConfigManager.FindAll<PropItemInfo>(info => info.itemType == PropType.Drawing);
            propList.RemoveAll(item =>
            {
                var suitInfo = ConfigManager.Find<SuitInfo>(t => t.drawingId == item.ID);
                return suitInfo?.prevSuitId != 0;
            });
        }

        if (propList != null && propList.Count > 0)
        {
            foreach (var prop in propList)
                result.Add(new ItemPair { itemId = prop.ID, count = moduleEquip.GetPropCount(prop.ID) });
        }
        return result;
    }

    public bool IsSublimationMax(PItem currentItem)
    {
        if (null == currentItem)
            return false;
        var suitInfo = ConfigManager.Get<SuitInfo>(currentItem.growAttr?.suitId ?? 0);
        if (null == suitInfo)
            return false;
        return !Array.Exists(suitInfo.nextSuitId, suitId => suitId > 0);
    }

    #endregion

    #region GM

    public void GmGetSublimationMatrials()
    {
        if (null == currentSublimationItem)
            return;
        var suitInfo = ConfigManager.Get<SuitInfo>(currentSublimationItem.growAttr.suitId);
        if (suitInfo == null)
        {
            var suitList = ConfigManager.FindAll<SuitInfo>(suit => suit?.prevSuitId == 0);
            //获得所有一级图纸
            foreach (var suit in suitList)
            {
                moduleGm.SendAddProp((ushort) suit.drawingId, 1);
                foreach (var m in suit.costs)
                    moduleGm.AddProp((ushort)m.itemId, m.count);
            }
        }
        else
        {
            //获得当前套装升级图纸
            suitInfo = ConfigManager.Get<SuitInfo>(Array.Find(suitInfo.nextSuitId, item => item > 0));
            List<ItemPair> list = new List<ItemPair>();
            while (suitInfo != null)
            {
                moduleGm.SendAddProp((ushort)suitInfo.drawingId, 1);
                list.AddRange(suitInfo.costs);
                suitInfo = ConfigManager.Get<SuitInfo>(Array.Find(suitInfo.nextSuitId, suitId => suitId > 0));
            }

            foreach (var pair in list)
                moduleGm.AddProp((ushort)pair.itemId, pair.count);
        }

    }
    #endregion

}
