// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-21      10:20
//  * LastModify：2018-08-21      15:07
//  ***************************************************************************************************/

#region

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class ChargeWindow_Growth : SubWindowBase<Window_Welfare>
{
    private Button                  buyButton;
    private Transform               buyNode;
    private DataSource<PGrowthFund> dataSource;
    private PChargeItem             growthFund;
    private ScrollView              scorllView;
    private RechargeGrowthTemplete  current;

    private ScrollView              defaultScrollView;
    private DataSource<string>      defaultDataSource;


    private void OnSetData(RectTransform node, PGrowthFund data)
    {
        var temp = node.GetComponentDefault<RechargeGrowthTemplete>();
        temp.InitData(data);

        temp.OnShowDetail = (t) => { parentWindow.ShowDetail(t.Data.reward); };
        temp.onSelect += (t) =>
        {
            if (current)
                current.selected = false;
            current = t;
        };
    }

    protected override void InitComponent()
    {
        base.InitComponent();

        var charge = moduleCharge.GetChargeItemListById(parentWindow.GetChargeID());
        if (charge != null && charge.Count > 0)
            growthFund = charge[0];

        scorllView        = Root.GetComponent<ScrollView>("scrollView");
        buyNode           = Root.GetComponent<Transform> ("default_Panel");
        buyButton         = Root.GetComponent<Button>    ("default_Panel/buy_Btn");
        defaultScrollView = Root.GetComponent<ScrollView>("default_Panel/scrollView");
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p) || growthFund == null)
            return false;

        buyButton.onClick.AddListener(OnBuyGrowthFund);

        RefreshBuyNode();
        return true;
    }

    private void RefreshBuyNode()
    {
        buyNode.SafeSetActive(growthFund.TotalBuyTimes() == 0);

        if (growthFund.TotalBuyTimes() > 0)
        {
            defaultDataSource?.Clear();
            var list = new List<PGrowthFund>(moduleCharge.GetGrowthFund());
            list.Sort(SortHandle);
            dataSource = new DataSource<PGrowthFund>(list, scorllView, OnSetData);
        }
        else
        {
            dataSource?.Clear();
            defaultDataSource = new DataSource<string>(ConfigManager.Get<ConfigText>(603).text, defaultScrollView,
                OnDefaultSetData);
        }
    }

    private void OnDefaultSetData(RectTransform node, string data)
    {
        Util.SetText(node.GetComponent<Text>(), data);
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;

        buyButton.onClick.RemoveAllListeners();
        return true;
    }

    private void OnBuyGrowthFund()
    {
        if (growthFund == null)
            return;
        moduleCharge.RequestBuyProduct(growthFund);
    }


    void _ME(ModuleEvent<Module_Charge> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Charge.ResponseGetGrowthFund:
                ResponseGetGrowthFund(e.msg as ScChargeDrawGrowthFund);
                break;
            case Module_Charge.NoticeChargeBuySuccess:
            case Module_Charge.NoticeChargeCountChange:
                RefreshBuyNode();
                break;
        }
    }

    private void ResponseGetGrowthFund(ScChargeDrawGrowthFund msg)
    {
        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9303,msg.result);
            return;
        }

        var list = new List<PGrowthFund>(moduleCharge.GetGrowthFund());
        list.Sort(SortHandle);
        dataSource.SetItems(list);

        var fund = Array.Find(moduleCharge.GetGrowthFund(), item => item.id == msg.drawId);
        if(fund != null)
            Window_ItemTip.Show(ConfigText.GetDefalutString(TextForMatType.RechargeUIText, 10), fund.reward);
    }

    private int SortHandle(PGrowthFund a, PGrowthFund b)
    {
        if (a.draw == b.draw)
            return a.level.CompareTo(b.level);
        return a.draw.CompareTo(b.draw);
    }
}
