// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-21      10:17
//  * LastModify：2018-08-21      10:17
//  ***************************************************************************************************/


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChargeWindow_Recharge : SubWindowBase<Window_Charge>
{
    private ScrollView scrollView;
    private Text       confirmText;
    private Button     buyButton;
    private PChargeItem currentItem;
    private ChargeItemInfo current;

    private bool defaultSelect;

    private Dictionary<ushort, ChargeItemInfo> tempCache = new Dictionary<ushort, ChargeItemInfo>();
   
    protected override void InitComponent()
    {
        base.InitComponent();
        scrollView  = WindowCache.GetComponent<ScrollView>  ("1_panel/scrollView");
        confirmText = WindowCache.GetComponent<Text>        ("1_panel/text_confirm");
        buyButton   = WindowCache.GetComponent<Button>      ("1_panel/buy_btn");
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;
        RefreshItemList();

        buyButton?.onClick.RemoveAllListeners();
        buyButton?.onClick.AddListener(this.OnBuyClick);
        return true;
    }

    private void RefreshItemList()
    {
        defaultSelect = false;
        new DataSource<PChargeItem>(moduleCharge.GetChargeItemList(ProductType.Diamond), scrollView, OnSetData);
    }

    private void OnBuyClick()
    {
        if (currentItem == null) return;
        
        moduleCharge.RequestBuyProduct(currentItem);

        moduleGlobal.LockUI(string.Empty, 0.5f);
    }
    
    private void OnSetData(RectTransform node, PChargeItem data)
    {
        var temp = node.GetComponentDefault<ChargeItemInfo>();
        temp.item = data;
        temp.onSelect += OnSelectChange;

        if (!defaultSelect) defaultSelect = temp.selected = true;

        if (!tempCache.ContainsKey(data.productId))
            tempCache.Add(data.productId, temp);
        else
            tempCache[data.productId] = temp;
    }

    private void OnSelectChange(ChargeItemInfo obj)
    {
        if (current)
            current.selected = false;

        current = obj;
        currentItem = current.item;

        Util.SetText(confirmText, Util.Format(ConfigText.GetDefalutString(TextForMatType.RechargeUIText, 4),
                Util.GetChargeCurrencySymbol(current.item.info.currencyType) + current.item.info.cost, current.item.info.reward.diamond));
    }

    void _ME(ModuleEvent<Module_Charge> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Charge.NoticeChargeBuySuccess:
                ResponseBuyItem(e.msg as ScChargeBuySuccess);
                break;
            case Module_Charge.EventItemList:
                RefreshItemList();
                break;
            case Module_Charge.NoticeChargeCountChange:
                tempCache[moduleCharge.CurrentDeal.productId].UpdateItemInfo();
                break;
        }
    }

    private void ResponseBuyItem(ScChargeBuySuccess msg)
    {
        if (tempCache.ContainsKey(msg.productId))
            tempCache[msg.productId].UpdateItemInfo();

        Window_ItemTip.Show(ConfigText.GetDefalutString(TextForMatType.RechargeUIText, 12), moduleCharge.CurrentDeal.info.reward, msg.hasTotalBuyCount == 1);
        moduleGlobal.UnLockUI();
    }
}