// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-21      10:22
//  * LastModify：2018-08-21      17:15
//  ***************************************************************************************************/

#region

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class ChargeWindow_TotalRecharge : SubWindowBase<Window_Charge>
{
    private PTotalCostReward currentItem;

    private DataSource<PTotalCostReward> dataSource;
    private Button GoBuy;
    private Text lessText;

    private ScrollView scrollView;

    private Text totalText;

    private RechargeAccumulativeTemplete current;

    protected override void InitComponent()
    {
        base.InitComponent();
        scrollView = WindowCache.GetComponent<ScrollView>("5_panel/scrollView");
        totalText  = WindowCache.GetComponent<Text>      ("5_panel/alreadycharge_Txt/number_Txt");
        lessText   = WindowCache.GetComponent<Text>      ("5_panel/text_confirm");
        GoBuy      = WindowCache.GetComponent<Button>    ("5_panel/buy_btn");
    }

    private int SortHandle(PTotalCostReward a, PTotalCostReward b)
    {
        if (a.draw == b.draw)
            return a.total.CompareTo(b.total);
        return a.draw.CompareTo(b.draw);
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;
        GoBuy?.onClick.RemoveAllListeners();
        GoBuy?.onClick.AddListener(OnGoBuy);

        var rList = new List<PTotalCostReward>(moduleCharge.GetTotalReward());
        rList.Sort(SortHandle);
        dataSource = new DataSource<PTotalCostReward>(rList, scrollView, OnSetData, OnItemClick);

        RefreshCurrentItem(rList.Find(item => item.total >= moduleCharge.totalCost));
        return true;
    }

    private void OnItemClick(RectTransform node, PTotalCostReward data)
    {
        var temp = node.GetComponentDefault<RechargeAccumulativeTemplete>();
        if (temp.state == RechargeAccumulativeTemplete.State.Got)
            return;
        if (temp.state == RechargeAccumulativeTemplete.State.CanGot)
        {
            Module_Charge.instance.RequestGetTotal(data.id);
            return;
        }
        OnClick(temp);
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;
        currentItem = null;
        dataSource.Clear();
        return true;
    }

    private void OnGoBuy()
    {
        var w = WindowCache as Window_Charge;
        w?.ShowWindow(Window_Charge.ChargeType.Recharge);
    }

    private void OnSetData(RectTransform node, PTotalCostReward data)
    {
        var temp = node.GetComponentDefault<RechargeAccumulativeTemplete>();
        temp?.InitData(data);
        if (temp)
        {
            temp.selected = false;
            temp.onClick = OnClick;
            temp.onShowDetail = (t) => { parentWindow.ShowDetail(t.Data.reward); };
        }

        if (data.total > moduleCharge.totalCost && temp)
        {
            if (currentItem == null)
                RefreshItem(temp);
            else
                temp.selected = data.id == currentItem.id;
        }
    }

    private void OnClick(RechargeAccumulativeTemplete temp)
    {
        if (temp.state == RechargeAccumulativeTemplete.State.CanGot)
        {
            Module_Charge.instance.RequestGetTotal(temp.Data.id);
        }
        else if(temp.state == RechargeAccumulativeTemplete.State.Process)
        {
            RefreshItem(temp);
        }
    }

    private void RefreshItem(RechargeAccumulativeTemplete temp)
    {
        if (current) current.selected = false;
        if (temp) temp.selected = true;
        current = temp;
        currentItem = temp.Data;
        if (currentItem == null)
            return;

        RefreshCurrentItem(currentItem);
    }

    private void RefreshCurrentItem(PTotalCostReward p)
    {
        Util.SetText(totalText, moduleCharge.totalCost.ToString());
        lessText.SafeSetActive(p != null);
        if (p != null)
            Util.SetText(lessText, Util.Format(ConfigText.GetDefalutString(TextForMatType.RechargeUIText, 8),
                (p.total - moduleCharge.totalCost)));
    }

    private void _ME(ModuleEvent<Module_Charge> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Charge.ResponseGetTotal:
                ResponseGetTotal(e.msg as ScChargeDrawReward);
                break;
        }
    }

    private void ResponseGetTotal(ScChargeDrawReward msg)
    {
        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9304, msg.result);
            return;
        }

        var rList = new List<PTotalCostReward>(moduleCharge.GetTotalReward());
        rList.Sort(SortHandle);
        dataSource.SetItems(rList);

        var rewardItem = Array.Find(moduleCharge.GetTotalReward(), item => item.id == msg.drawId);
        if(rewardItem != null)
            Window_ItemTip.Show(ConfigText.GetDefalutString(TextForMatType.RechargeUIText, 9), rewardItem.reward);
    }
}
