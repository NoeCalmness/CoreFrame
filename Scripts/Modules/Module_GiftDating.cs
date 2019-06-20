/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-04-02
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;

public class Module_GiftDating : Module<Module_GiftDating>
{
    public const string EventGiveGiftSuccess = "EventGiftDatingGiveGiftSuccess";//����ɹ�
    public const string EventGiftUpdate = "EventGiftDatingGiftUpdate";//���������б�
    public const string EventNpcPlayGiveGift = "EventGiftDatingNpcPlayGiveGift";//�����񶯻�

    /// <summary>
    /// �Ƿ��н�������
    /// </summary>
    public bool haveSettlementGift
    {
        get
        {
            return moduleEquip.GetBagProps(PropType.Sundries, (int)SundriesSubType.NpcgoodFeeling).Count > 0;
        }
    }

    /// <summary>
    /// �øж��Ƿ�����
    /// </summary>
    public bool isFetterFullLv{get { return moduleNPCDating.curDatingNpcMsg == null ? true : moduleNPCDating.curDatingNpcMsg.fetterLv >= moduleNPCDating.curDatingNpcMsg.maxFetterLv; }}

    public List<PItem> gifts { get { return m_gifts; } }
    private List<PItem> m_gifts = new List<PItem>();

    public PItem curGift { get; set; }

    public bool isGiveGift { get; set; }//�Ӿ����Ƿ�����Ϊ����

    public bool isClickSend { get; set; }//�Ƿ���������ť ��Ϊȷ������ɹ��ǴӼӾ�������������ж�,��Ϊ����Э�����ڼӾ��������Э�� �����ڸı������״̬ʱ ����ʱ

    public void GetGiftList()
    {
        m_gifts.Clear();
        m_gifts.AddRange(moduleEquip.GetBagProps(PropType.Sundries, (int)SundriesSubType.NpcgoodFeeling));
        DispatchModuleEvent(EventGiftUpdate);
    }

    public void SendUseGift(ushort npcID, ulong itemID, ushort num)
    {
        CsNpcGift use = PacketObject.Create<CsNpcGift>();
        use.npcId = (ushort)npcID;
        use.itemId = itemID;
        use.number = num;
        session.Send(use);

        isClickSend = true;
    }

    void _Packet(ScNpcGift p)
    {
        switch (p.result)
        {
            case 0: if (moduleNPCDating.curDatingNpcMsg != null) DispatchModuleEvent(EventGiveGiftSuccess, moduleNPCDating.curDatingNpcMsg.npcId, curGift.itemTypeId); break;
            case 1: moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.GiftUIText, 8)); break;
            case 2: moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.GiftUIText, 9)); break;
            //case 3: moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.GiftUIText, 15)); break;
            case 4: moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.GiftUIText, 10)); break;
            default:
                break;
        }
    }

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        m_gifts.Clear();
        isClickSend = false;
        curGift = null;
    }

    public int GetCurrentExp(int npcId, ushort itemTypeId)
    {
        var info = ConfigManager.Get<PropItemInfo>(itemTypeId);
        if (info == null) return 0;
        var exps = info.npcPropExp.Split(';');
        if (exps == null || exps.Length < 1) return 0;
        for (int i = exps.Length - 1; i >= 0; i--)
        {
            if (string.IsNullOrEmpty(exps[i]))
            {
                Array.Resize(ref exps, exps.Length - 1);
                break;
            }
        }

        string[] npc_exp = null;
        for (int i = 0; i < exps.Length; i++)
        {
            if (!exps[i].Contains(",")) continue;

            npc_exp = exps[i].Split(',');
            if (npc_exp == null || npc_exp.Length < 2) continue;

            var npcid = Util.Parse<int>(npc_exp[0]);
            if (npcid == npcId) return Util.Parse<int>(npc_exp[1]);
        }

        return 0;
    }
}
