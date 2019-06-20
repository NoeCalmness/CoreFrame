/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-06
 * 
 ***************************************************************************************************/

using UnityEngine;

public class Module_Wish : Module<Module_Wish>
{
    #region Events

    /// <summary>
    /// 抽卡信息变更（抽卡花费 购买祈愿币花费等）
    /// </summary>
    public const string EventWishInfoUpdate = "EventWishWishInfoUpdate";
    /// <summary>
    /// 购买祈愿币返回 param1 = 购买结果 参考 ScBuyWishCoin.result
    /// </summary>
    public const string EventBuyWishCoin    = "EventWishBuyWishCoin";
    /// <summary>
    /// 抽卡返回 msg = ScRoleWish
    /// </summary>
    public const string EventWishResult     = "EventWishWishResult";
    /// <summary>
    /// 抽卡掉落信息更新
    /// </summary>
    public const string EventDropInfoUpdate = "EventWishDropInfoUpdate";

    #endregion

    /// <summary>
    /// 单次抽卡消耗的祈愿币数量
    /// </summary>
    public int wishCost { get { return m_wishCost; } }
    /// <summary>
    /// 单次购买祈愿币消耗的钻石数量
    /// </summary>
    public int buyWishCoinCost { get { return m_buyWishCoinCost; } }
    /// <summary>
    /// 抽卡掉落信息
    /// </summary>
    public PWishItemDropInfo[] dropInfo { get { return m_dropInfo != null && m_dropInfo.dropinfo != null ? m_dropInfo.dropinfo : new PWishItemDropInfo[] { }; } }

    private ScWishItemDropInfo m_dropInfo;

    private int m_wishCost = 1, m_buyWishCoinCost = 200;

    public void BuyWishCoin(uint rCount, bool lockUI = false)
    {
        if (lockUI) moduleGlobal.LockUI("", 0.2f);
        var msg = PacketObject.Create<CsWishBuyWishCoin>();
        msg.count = (byte)rCount;
        session.Send(msg);
    }

    public void ImFeelingLucky(byte rTimes, bool lockUI = false)
    {
        if (lockUI) moduleGlobal.LockUI("", 0.2f);

        var msg = PacketObject.Create<CsWishWish>();
        msg.times = rTimes;
        session.Send(msg);

        #region Simulate

        //else
        //{
        //    var p = PacketObject.Create<ScWishWish>();
        //    p.result = (sbyte)Random.Range(0, 2);
        //    p.item = PacketObject.Create<PItem2>();
        //    p.item.itemTypeId = new ushort[] { 1101, 1203, 1403, 1501, 40905, 41203, 41602 }[Random.Range(0, 7)];
        //    p.item.level = (byte)Random.Range(0, 35);
        //    p.item.star = (byte)Random.Range(0, 7);
        //    p.item.num = (uint)Random.Range(1, 15);

        //    _Packet(p);

        //    p.Destroy();
        //}

        #endregion
    }

    #region Packet handlers

    void _Packet(ScWishWish p)
    {
        DispatchModuleEvent(EventWishResult, p);
    }

    void _Packet(ScSystemSetting p)
    {
        m_wishCost        = p.wishCost;
        m_buyWishCoinCost = p.buyWishCoinCost;

        DispatchModuleEvent(EventWishInfoUpdate);
    }

    void _Packet(ScWishBuyWishCoin p)
    {
        DispatchModuleEvent(EventBuyWishCoin, (int)p.result);
    }

    void _Packet(ScWishItemDropInfo p)
    {
        p.CopyTo(ref m_dropInfo);
        DispatchModuleEvent(EventDropInfoUpdate);
    }

    #endregion
}
