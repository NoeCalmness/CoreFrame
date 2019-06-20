/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-02-20
 * 
 ***************************************************************************************************/

using System.Collections.Generic;

public class Module_DatingGift : Module<Module_DatingGift>
{
    public static string EVENT_SENDGIFT_SUCEESE_NAME = "EventSendGiftSuceeseName";

    private EnumDatingGiftType giftType = EnumDatingGiftType.UnUsed;

    public List<PItem> ItemDatas
    {
        get
        {
            var bagPropList = moduleEquip.currentBagProp;
            if (bagPropList == null) return null;

            List<PItem> tmpList = new List<PItem>();
            for (int i = 0; i < bagPropList.Count; i++)
            {
                PropItemInfo item = bagPropList[i].GetPropItem();
                if (item && item.itemType == PropType.DatingProp && item.subType == (int)giftType) tmpList.Add(bagPropList[i]);
            }
            return tmpList;
        }
    }

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();

    }

    public void ShowAsyncPanel(EnumDatingGiftType type)
    {
        giftType = type;

        Window.ShowAsync<Window_DatingGift>();
    }

    public void SendGift(int itemTypeId)
    {
        var p = PacketObject.Create<CsNpcEngagementGift>();
        p.itemTypeId = itemTypeId;
        session.Send(p);
    }

    void _Packet(ScNpcEngagementGift msg)
    {
        if (msg.result == 0)
        {
            DispatchModuleEvent(EVENT_SENDGIFT_SUCEESE_NAME);
        }
    }

}
