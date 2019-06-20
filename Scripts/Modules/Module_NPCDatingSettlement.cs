/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-12-29
 * 
 ***************************************************************************************************/

using UnityEngine;

public class Module_NPCDatingSettlement : Module<Module_NPCDatingSettlement>
{
    public const string EVENT_REFRESH_WINDOW_DATA = "EventRrefreshWindowData";

    public SettlementData datingSettlement { private set; get; }

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        datingSettlement = null;
    }

    /// <summary>
    /// 获取结算数据
    /// </summary>
    public void SendGetData()
    {
        var p = PacketObject.Create<CsNpcDatingSettlement>();
        session.Send(p);
    }
    void _Packet(ScNpcDatingSettlement msg)
    {
        if (datingSettlement == null) datingSettlement = new SettlementData();
        datingSettlement.finishedEventIds = msg.finishedEventIds;
        datingSettlement.moodValue = msg.moodValue;
        datingSettlement.lvIconName = msg.lvIconName;
        datingSettlement.addGoodFeelValue = msg.addGoodFeelValue;
        DispatchModuleEvent(EVENT_REFRESH_WINDOW_DATA);
    }

    public class SettlementData
    {
        /// <summary>
        /// 本次约会中完成的所有事件
        /// </summary>
        public int[] finishedEventIds;
        /// <summary>
        /// 本次约会的心情值
        /// </summary>
        public int moodValue;
        /// <summary>
        /// 评级图标名称
        /// </summary>
        public string lvIconName;
        /// <summary>
        /// 增加的羁绊值
        /// </summary>
        public int addGoodFeelValue;
    }
}
