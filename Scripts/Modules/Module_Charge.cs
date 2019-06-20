/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-02-26
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

public enum ChargeCurrencyTypes
{
    /// <summary>
    /// 人民币
    /// </summary>
    RMB = 0,
    /// <summary>
    /// 美元
    /// </summary>
    USD = 1,
}

public enum ProductType
{
    Diamond = 1,
    GrowthFund,
    MonthCard,
    SeasonCard,
    Gift,
    WishStone,
    SummonStone,
    DailySale,
    WeekSale,
}

public enum LockState
{
    None,
    Lock,
    Unlock,
}

public class Module_Charge : Module<Module_Charge>
{
    #region Events

    /// <summary>
    /// 商店物品列表刷新
    /// </summary>
    public const string EventItemList = "EventChargeItemList";
    /// <summary>
    /// 购买物品返回  msg = ScChargeBuyItem
    /// </summary>
    public const string EventBuyItem = "EventChargeBuyItem";

    public const string ResponseGetTotal = "ResponseGetTotal";
    public const string ResponseGetGrowthFund = "ResponseGetGrowthFund";
    public const string NoticeChargeInfoChange = "NoticeChargeInfoChange";
    public const string NoticeChargeBuySuccess = "NoticeChargeBuySuccess";
    public const string NoticeChargeCountChange = "NoticeChargeCountChange";
    public const string NoticeChargeGetSign = "NoticeChargeGetSign";
    public const string NoticeChargeDailyRebateUpdate = "NoticeChargeDailyRebateUpdate";
    #endregion



    private ScChargeItemList m_chargeItemList;
    private LockState m_lockState;

    #region properties
    public PChargeItem CurrentDeal { get; private set; }
    public bool HasMonthCard { get { return CalcCardDays(MonthEndTime) > 0; } }
    public bool HasSeasonCard {  get { return CalcCardDays(SeasonEndTime) > 0; } }
    public ulong MonthEndTime{ get; private set; }
    public ulong SeasonEndTime { get; private set; }
    public ulong totalCost { get; private set; }
    public PChargeItem[] chargeItems { get { return GetChargeItemList(); } }

    #endregion


    protected override void OnGameDataReset()
    {
        m_chargeItemList?.Destroy();
        m_chargeItemList = null;

        CurrentDeal?.Destroy();
        CurrentDeal = null;
    }

    protected override void OnEnterGame()
    {
        base.OnEnterGame();

        RequestChargeInfo();
    }

    #region Request 

    public void RequestChargeInfo()
    {
        var msg = PacketObject.Create<CsChargeInfo>();
        session.Send(msg);
    }

    public void RequestBuyProduct(PChargeItem item)
    {
        if (item == null) return;

        moduleGlobal.LockUI();

        var msg = PacketObject.Create<CsChargeBuyItem>();
        msg.productId = item.productId;
        session.Send(msg);

        CurrentDeal = item;
    }

    public void RequestGetTotal(byte rTotalId)
    {
        var msg = PacketObject.Create<CsChargeDrawReward>();
        msg.drawId = rTotalId;
        session.Send(msg);
    }

    public void RequestGetGrowthFund(byte rGrowthId)
    {
        var msg = PacketObject.Create<CsChargeDrawGrowthFund>();
        msg.drawId = rGrowthId;
        session.Send(msg);
    }

    public void RequestGetSign(int rProductID, int rDay)
    {
        moduleGlobal.LockUI(0.5f, 0, 0x02);

        var msg = PacketObject.Create<CsChargeDailyRebateGet>();
        msg.productId = (ushort)rProductID;
        msg.whichday = (byte)rDay;
        session.Send(msg);
    }
    
    #endregion


    #region Packet

    void _Packet(ScChargeDailyRebateUpdate msg)
    {
        for (int i = 0, length = m_chargeItemList.dailyGetReward.Length; i < length; i++)
        {
            if (m_chargeItemList.dailyGetReward[i].proId == msg.update.proId)
            {
                m_chargeItemList.dailyGetReward[i].Destroy();
                msg.update.CopyTo(ref m_chargeItemList.dailyGetReward[i]);
                DispatchModuleEvent(NoticeChargeDailyRebateUpdate);
                break;
            }
        }
    }

    void _Packet(ScChargeDailyRebateGet msg)
    {
        moduleGlobal.UnLockUI();
        DispatchModuleEvent(NoticeChargeGetSign, msg);
    }

    void _Packet(ScChargeProductNumberChange msg)
    {
        msg.items.CopyTo(ref m_chargeItemList.items);

        DispatchModuleEvent(EventItemList);
    }

    void _Packet(ScChargeInfo msg)
    {
        SeasonEndTime = msg.seasonEndTime;
        MonthEndTime  = msg.monthEndTime;
        totalCost     = msg.total;

        DispatchModuleEvent(NoticeChargeInfoChange);
    }

    void _Packet(ScChargeItemList p)
    {
        p.CopyTo(ref m_chargeItemList);

        DispatchModuleEvent(EventItemList);
    }

    void _Packet(ScChargeBuyItem p)
    {
        moduleGlobal.UnLockUI();

        if (p.result == 0 && CurrentDeal != null)
        {
            m_lockState = LockState.None;
            LockDeal(true);

            moduleGm.SendCharge(CurrentDeal.productId);
        }

        DispatchModuleEvent(EventBuyItem, p);
    }

    void _Packet(ScChargeDrawGrowthFund msg)
    {
        if (msg.result == 0)
        {
            var grow = Array.Find(m_chargeItemList.growthFund, item => item.id == msg.drawId);
            grow.draw = true;
        }
        DispatchModuleEvent(ResponseGetGrowthFund, msg);
    }

    void _Packet(ScChargeDrawReward msg)
    {
        if (msg.result == 0)
        {
            var grow = Array.Find(m_chargeItemList.costReward, item => item.id == msg.drawId);
            grow.draw = true;
        }
        DispatchModuleEvent(ResponseGetTotal, msg);
    }

    void _Packet(ScChargeBuySuccess msg)
    {
        var product = GetProduct(msg.productId);
        if (product != null)
        {
            product.hasTotalBuyCount = msg.hasTotalBuyCount;
            product.hasBuyCount = msg.hasBuyCount;

            LockDeal(false);
        }

        DispatchModuleEvent(NoticeChargeBuySuccess, msg);

        SDKManager.Setryzf(msg.orderUid.ToString(), msg.paymentType.ToString(), product != null && product.info != null ? product.info.currencyType : (byte)0, msg.cost);
    }
    #endregion


    #region public functions


    public PChargeItem[] GetChargeItemList()
    {
        var items = m_chargeItemList?.items ?? new PChargeItem[] { };

        return items;
    }

    public PChargeItem[] GetChargeItemList(ProductType type)
    {
        return Array.FindAll(chargeItems, item => item.info.type == (int)type && IsValidProduct(item.info)
#if UNITY_EDITOR
#elif UNITY_ANDROID
        && (item.info.platform == 0 || item.info.platform == 1)
#elif UNITY_IOS
        && (item.info.platform == 0 || item.info.platform == 2)
#else
        && item.info.platform == 0
#endif
        );
    }

    public List<PChargeItem> GetChargeItemListById(List<int> rId)
    {
        List<PChargeItem> itemList = new List<PChargeItem>();
        for (int i = 0; i < chargeItems.Length; i++)
        {
            if (chargeItems[i] == null || !IsValidProduct(chargeItems[i]?.info)) continue;
            for (int j = 0; j < rId.Count; j++)
            {
                if (rId[j] == (int)chargeItems[i].productId)
                {
                    if (!itemList.Contains(chargeItems[i])) itemList.Add(chargeItems[i]);
                }
            }
        }
        return itemList;
    }

    public PChargeItem[] GetChargeItemListByTab(int rTab)
    {
        return Array.FindAll(chargeItems, item => item.info.tabs == rTab && IsValidProduct(item.info)
#if UNITY_EDITOR
#elif UNITY_ANDROID
        && (item.info.platform == 0 || item.info.platform == 1)
#elif UNITY_IOS
        && (item.info.platform == 0 || item.info.platform == 2)
#else
        && item.info.platform == 0
#endif
        );
    }

    public bool IsTabActive(int rTab)
    {
        var items = GetChargeItemListByTab(rTab);
        if (items == null || items.Length == 0)
            return false;
        ulong t = (ulong)Util.GetTimeStamp();
        for (var i = 0; i < items.Length; i++)
        {
            var info = items[i].info;
            if (info.startTime == 0 || info.endTime == 0)
                return true;
            if (t >= info.startTime && t <= info.endTime)
                return true;
        }
        return false;
    }

    public PGrowthFund[] GetGrowthFund()
    {
        if (m_chargeItemList?.growthFund == null)
            return new PGrowthFund[0];
        return m_chargeItemList.growthFund;
    }

    public PChargeDailyGetReward GetSignInfos(PChargeItem item)
    {
        if (item == null) return null;
        return m_chargeItemList?.dailyGetReward?.Find(a => a.proId == item.productId);
    }

    public PTotalCostReward[] GetTotalReward()
    {
        if (m_chargeItemList?.costReward == null)
            return new PTotalCostReward[0];
        return m_chargeItemList.costReward;
    }

    public PChargeItem GetProduct(ushort productId)
    {
        if (m_chargeItemList == null) return null;
        return Array.Find(m_chargeItemList.items, item => item.productId == productId);
    }
    /// <summary>
    /// 支付成功后不知道服务器什么时候响应。所以客户端需要先做一个加锁操作，防止限购商品多次购买
    /// </summary>
    /// <param name="rLock"> true 加锁操作。 false 解锁</param>
    public void LockDeal(bool rLock)
    {
        if (null == CurrentDeal) return;

        if (rLock && m_lockState != LockState.None)
        {
            m_lockState = LockState.Lock;
            return;
        }

        if (!rLock && m_lockState != LockState.Lock)
        {
            m_lockState = LockState.Unlock;
            return;
        }

        var value = rLock ? 1 : -1;

        var key = $"{Module_Player.instance.id_}{CurrentDeal.productId }-hasTotalBuyCount";
        if (PlayerPrefs.HasKey(key))
            PlayerPrefs.SetInt(key, PlayerPrefs.GetInt(key) + value);
        else if(rLock)
            PlayerPrefs.SetInt(key, value);

        key = $"{Module_Player.instance.id_}{CurrentDeal.productId }-hasBuyCount";
        if (PlayerPrefs.HasKey(key))
            PlayerPrefs.SetInt(key, PlayerPrefs.GetInt(key) + value);
        else if(rLock)
            PlayerPrefs.SetInt(key, value);

        DispatchModuleEvent(NoticeChargeCountChange);

        m_lockState = rLock ? LockState.Lock : LockState.Unlock;
    }
    #endregion

    #region static functions


    public static int CalcCardDays(double endTime)
    {
        TimeSpan t = TimeSpan.FromSeconds(endTime);
        TimeSpan t2 = TimeSpan.FromSeconds(Util.GetTimeStamp(false, true));
        return t.Days - t2.Days;
    }

    public static bool IsValidProduct(PProductInfo rInfo)
    {
        if (rInfo == null) return false;
        if (rInfo.startTime > 0 && rInfo.endTime > 0)
        {
            ulong t = (ulong)Util.GetTimeStamp(false, true);
            return rInfo.startTime <= t && rInfo.endTime >= t;
        }
        return true;
    }

    #endregion

}
