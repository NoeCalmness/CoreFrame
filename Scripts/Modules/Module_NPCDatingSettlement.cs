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
    /// ��ȡ��������
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
        /// ����Լ������ɵ������¼�
        /// </summary>
        public int[] finishedEventIds;
        /// <summary>
        /// ����Լ�������ֵ
        /// </summary>
        public int moodValue;
        /// <summary>
        /// ����ͼ������
        /// </summary>
        public string lvIconName;
        /// <summary>
        /// ���ӵ��ֵ
        /// </summary>
        public int addGoodFeelValue;
    }
}
