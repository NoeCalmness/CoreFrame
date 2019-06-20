// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-04-19      14:08
//  *LastModify：2019-04-19      14:08
//  ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChargeWindow_Sign : SubWindowBase<Window_Welfare>
{
    private Button buyButton;
    private Transform buyNode;
    private ScrollView scrollView;
    private Text endTime;

    private DataSource<PChargeDailyOneInfo> m_dataSource;
    private PChargeItem m_chargeItem;
    private PChargeDailyGetReward m_rewards;

    private RechargeDailySign m_current;
    private RechargeDailySign m_currentControl;


    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;
        buyButton?.onClick.AddListener(OnBuyClick);
        return true;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;
        buyButton?.onClick.RemoveAllListeners();
        return true;
    }

    protected override void RefreshData(params object[] p)
    {
        RefreshNode();
        if (m_chargeItem == null) return;
        var t = Util.GetDateTime((long)m_chargeItem.info.endTime);
        Util.SetText(endTime, Util.Format(ConfigText.GetDefalutString(TextForMatType.RechargeUIText, 29),
            t.Month, t.Day));
    }

    public void SetProductType()
    {
        var charge = moduleCharge.GetChargeItemListById(parentWindow.GetChargeID());
        if (charge != null && charge.Count > 0)
            m_chargeItem = charge[0];

        m_rewards = moduleCharge.GetSignInfos(m_chargeItem);
    }


    protected override void InitComponent()
    {
        base.InitComponent();

        buyNode     = Root.GetComponent<Transform> ("default_Panel");
        buyButton   = Root.GetComponent<Button>    ("default_Panel/Button");
        endTime     = Root.GetComponent<Text>      ("Text");
        scrollView  = Root.GetComponent<ScrollView>("scrollView");
    }


    private void OnBuyClick()
    {
        if (m_chargeItem == null || m_rewards == null)
            return;

        var day = Module_Charge.CalcCardDays(m_chargeItem.info.endTime);

        if (day < m_rewards.oneinfo.Length)
        {
            var t = Util.GetDateTime((long)m_chargeItem.info.endTime);
            Window_Alert.ShowAlertDefalut(Util.Format(ConfigText.GetDefalutString(TextForMatType.RechargeUIText, 28),
                t.Month, t.Day), 
                () =>
                {
                    moduleCharge.RequestBuyProduct(m_chargeItem);
                }, ()=> {});
            return;
        }

        moduleCharge.RequestBuyProduct(m_chargeItem);
    }

    private void RefreshNode()
    {
        SetProductType();
        
        buyNode.SafeSetActive(m_chargeItem?.hasTotalBuyCount == 0);

        if (!buyNode.gameObject.activeInHierarchy)
        {
            RefreshList();
        }
    }

    private void RefreshList()
    {
        var list = new List<PChargeDailyOneInfo>();
        if (m_rewards != null)
            list.AddRange(m_rewards.oneinfo);
        list.Sort(SortHandle);
        if (m_dataSource == null)
            m_dataSource = new DataSource<PChargeDailyOneInfo>(list, scrollView, OnSetData);
        else
            m_dataSource.SetItems(list);
    }

    private void OnSetData(RectTransform node, PChargeDailyOneInfo data)
    {
        var temp = node.GetComponentDefault<RechargeDailySign>();
        temp.InitData(m_chargeItem, data, (int) m_rewards.whichday);

        temp.OnShowDetail = (t) => { parentWindow.ShowDetail(t.Info.reward); };
        temp.onSelect += (t) =>
        {
            if (m_current)
                m_current.selected = false;
            m_current = t;
        };

        if (m_rewards.whichday == data.id)
            m_currentControl = temp;
    }


    private int SortHandle(PChargeDailyOneInfo x, PChargeDailyOneInfo y)
    {
        return x.id .CompareTo(y.id);
    }

    private void _ME(ModuleEvent<Module_Charge> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Charge.NoticeChargeGetSign:
                ResponseChargeGetSign(e.msg as ScChargeDailyRebateGet);
                break;
            case Module_Charge.EventItemList:
            case Module_Charge.NoticeChargeDailyRebateUpdate:
                m_rewards = moduleCharge.GetSignInfos(m_chargeItem);
                RefreshNode();
                RefreshList();
                break;
            case Module_Charge.NoticeChargeBuySuccess:
            case Module_Charge.NoticeChargeCountChange:
                RefreshNode();
                break;
        }
    }

    private void ResponseChargeGetSign(ScChargeDailyRebateGet msg)
    {
        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9855, msg.result);
            return;
        }

        if (m_currentControl != null && m_currentControl.Info.id == msg.whichday)
        {
            m_currentControl.Info.draw = true;
            m_currentControl.InitData(m_chargeItem, m_currentControl.Info, msg.whichday);
            Window_ItemTip.Show(ConfigText.GetDefalutString(TextForMatType.RechargeUIText, 30), m_currentControl.Info.reward);
        }
        else
        {
            var dailyInfo = m_rewards.oneinfo.Find(item => item.id == msg.whichday);
            if (null != dailyInfo)
            {
                dailyInfo.draw = true;
                RefreshList();
                Window_ItemTip.Show(ConfigText.GetDefalutString(TextForMatType.RechargeUIText, 30), dailyInfo.reward);
            }
        }
    }
}
