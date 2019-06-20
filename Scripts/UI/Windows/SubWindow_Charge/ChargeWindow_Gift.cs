// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-21      10:21
//  * LastModify：2018-08-21      16:07
//  ***************************************************************************************************/

#region

using UnityEngine;
using UnityEngine.UI;

#endregion

public class ChargeWindow_Gift : SubWindowBase<Window_Welfare>
{
    private Button                  buyButton;

    private Text                    confirmText;

    private PChargeItem             currentItem;

    private ScrollView              scrollView;
    private RechargeGiftTemplete    current;
    private bool                    defaultSelect;

    protected DataSource<PChargeItem> dataSource;

    protected override void InitComponent()
    {
        base.InitComponent();

        scrollView      = Root.GetComponent<ScrollView>  ("scrollView");
        confirmText     = Root.GetComponent<Text>        ("text_confirm");
        buyButton       = Root.GetComponent<Button>      ("buy_btn");

    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;


        dataSource = new DataSource<PChargeItem>(moduleCharge.GetChargeItemListById(parentWindow.GetChargeID()), scrollView, OnSetData);
        defaultSelect = false;
        buyButton?.onClick.RemoveAllListeners();
        buyButton?.onClick.AddListener(onBuyGift);
        return true;
    }

    private void onBuyGift()
    {
        if (currentItem == null)
            return;
        moduleCharge.RequestBuyProduct(currentItem);
    }

    private void OnSetData(RectTransform node, PChargeItem data)
    {
        var temp = node.GetComponentDefault<RechargeGiftTemplete>();
        temp?.InitData(data);
        if (temp)
        {
            temp.selected = currentItem?.productId == data.productId;
            temp.onSelect = (t) =>
            {
                if (current) current.selected = false;
                current = t;
                RefreshCurrent(t.Data);
            };

            temp.onDetail = (t) =>
            {
                parentWindow.ShowDetail(t.previewList);
            };

            if (!defaultSelect) defaultSelect = temp.selected = true;
        }
    }

    private void RefreshCurrent(PChargeItem data)
    {
        currentItem = data;

        Util.SetText(confirmText, Util.Format(ConfigText.GetDefalutString(TextForMatType.RechargeUIText, 5),
                Util.GetChargeCurrencySymbol(data.info.currencyType) + data.info.cost, data.info.name));
    }

    private void _ME(ModuleEvent<Module_Charge> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Charge.NoticeChargeBuySuccess:
                Window_ItemTip.Show(ConfigText.GetDefalutString(TextForMatType.RechargeUIText, 11), moduleCharge.CurrentDeal.info.reward);
                current?.RefreshCount();
                break;
            case Module_Charge.NoticeChargeCountChange:
                current?.RefreshCount();
                break;
        }
    }
}
