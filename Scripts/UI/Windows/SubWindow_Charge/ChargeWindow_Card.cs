// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-21      10:18
//  * LastModify：2018-08-21      10:18
//  ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

public class ChargeWindow_Card : SubWindowBase<Window_Welfare>
{
    private struct CardEntry
    {
        public Button buyButton;
        public Button reorderButton;
        public Text remainTimeText;
        public Text price;
        public Text[] tips;
        private GameObject buyNode;
        private GameObject reorderNode;
        private PChargeItem chargeItem;

        public void Init(int textId)
        {
            buyNode     = buyButton?.transform.parent?.gameObject;
            reorderNode = reorderButton?.transform.parent?.gameObject;

            var t = ConfigManager.Get<ConfigText>(textId);
            if (!t) return;
            for (int i = 0; i < tips.Length; i++)
            {
                if (i < t.text.Length)
                    Util.SetText(tips[i], t.text[i]);
                else
                    Util.SetText(tips[i], string.Empty);
            }
        }

        public void Set(PChargeItem item, bool has, ulong endTime)
        {
            if (item == null) return;

            chargeItem = item;
            buyNode.SafeSetActive(!has);
            reorderNode.SafeSetActive(has);
            

            Util.SetText(price, Util.GetChargeCurrencySymbol(chargeItem.info.currencyType) + chargeItem.info.cost);
            if (has)
            {
                Util.SetText(remainTimeText, Util.Format(ConfigText.time0.text[0], Module_Charge.CalcCardDays(endTime)));
            }
        }
    }

    private readonly CardEntry[] Cards = new CardEntry[2];

    private PChargeItem monthChargeItem;
    private PChargeItem seasonChargeItem;

    protected override void InitComponent()
    {
        base.InitComponent();
        Cards[0].tips            = new Text[3];
        Cards[0].buyButton       = Root.GetComponent<Button>("ticket_01/buy/confirm_Btn");
        Cards[0].reorderButton   = Root.GetComponent<Button>("ticket_01/reorder/reorder_Btn");
        Cards[0].remainTimeText  = Root.GetComponent<Text>  ("ticket_01/reorder/remaintime_Txt");
        Cards[0].price           = Root.GetComponent<Text>  ("ticket_01/price_Txt");
        Cards[0].tips[0]         = Root.GetComponent<Text>  ("ticket_01/content01_Txt");
        Cards[0].tips[1]         = Root.GetComponent<Text>  ("ticket_01/content02_Txt");
        Cards[0].tips[2]         = Root.GetComponent<Text>  ("ticket_01/content03_Txt");
        Cards[0].Init(601);

        Cards[1].tips            = new Text[3];
        Cards[1].buyButton       = Root.GetComponent<Button>("ticket_02/buy/confirm_Btn");
        Cards[1].reorderButton   = Root.GetComponent<Button>("ticket_02/reorder/reorder_Btn");
        Cards[1].remainTimeText  = Root.GetComponent<Text>  ("ticket_02/reorder/remaintime_Txt");
        Cards[1].price           = Root.GetComponent<Text>  ("ticket_02/price_Txt");
        Cards[1].tips[0]         = Root.GetComponent<Text>  ("ticket_02/content01_Txt");
        Cards[1].tips[1]         = Root.GetComponent<Text>  ("ticket_02/content02_Txt");
        Cards[1].tips[2]         = Root.GetComponent<Text>  ("ticket_02/content03_Txt");
        Cards[1].Init(602);
    }

    //    public override void MultiLanguage()
    //    {
    //        base.MultiLanguage();
    //        Util.SetText(WindowCache.GetComponent<Text>("2_panel/ticket_01/buy/buy_Txt"), 1);
    //    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;

        var monthChargeItems = moduleCharge.GetChargeItemListById(parentWindow.GetChargeID());
        if(monthChargeItems != null && monthChargeItems.Count > 0)
            monthChargeItem = monthChargeItems[0];
        if (monthChargeItem != null)
        {
            Cards[0].Set(monthChargeItem, moduleCharge.HasMonthCard, moduleCharge.MonthEndTime);
            Cards[0].buyButton    ?.onClick.RemoveAllListeners();
            Cards[0].buyButton    ?.onClick.AddListener(OnBuyMonthCard);
            Cards[0].reorderButton?.onClick.RemoveAllListeners();
            Cards[0].reorderButton?.onClick.AddListener(OnReorderMonthCard);
        }

        var seasonChargeItems = moduleCharge.GetChargeItemListById(parentWindow.GetChargeID());
        if (seasonChargeItems != null && seasonChargeItems.Count > 0)
            seasonChargeItem = seasonChargeItems[0];
        if (seasonChargeItem != null)
        {
            Cards[1].Set(seasonChargeItem, moduleCharge.HasSeasonCard, moduleCharge.SeasonEndTime);
            Cards[1].buyButton    ?.onClick.RemoveAllListeners();
            Cards[1].buyButton    ?.onClick.AddListener(OnBuySeasonCard);
            Cards[1].reorderButton?.onClick.RemoveAllListeners();
            Cards[1].reorderButton?.onClick.AddListener(OnReorderSeasonCard);
        }

        return true;
    }

    private void OnBuySeasonCard()
    {
        if (seasonChargeItem == null) return;
        moduleCharge.RequestBuyProduct(seasonChargeItem);
    }

    private void OnReorderSeasonCard()
    {
        if (seasonChargeItem == null) return;
        moduleCharge.RequestBuyProduct(seasonChargeItem);
    }

    private void OnBuyMonthCard()
    {
        if (monthChargeItem == null) return;
        moduleCharge.RequestBuyProduct(monthChargeItem);
    }

    private void OnReorderMonthCard()
    {
        if (monthChargeItem == null) return;
        moduleCharge.RequestBuyProduct(monthChargeItem);
    }

    void _ME(ModuleEvent<Module_Charge> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Charge.NoticeChargeInfoChange:
                if(monthChargeItem != null)
                    Cards[0].Set(monthChargeItem, moduleCharge.HasMonthCard, moduleCharge.MonthEndTime);
                if(seasonChargeItem != null)
                    Cards[1].Set(seasonChargeItem, moduleCharge.HasSeasonCard, moduleCharge.SeasonEndTime);
                break;
            case Module_Charge.NoticeChargeBuySuccess:
                ResponseBuyCard(e.msg as ScChargeBuySuccess);
                break;
        }
    }

    private void ResponseBuyCard(ScChargeBuySuccess msg)
    {
        var product = moduleCharge.GetProduct(msg.productId);
        if (product.info.type == (int) ProductType.MonthCard)
        {
             var title = ConfigText.GetDefalutString(TextForMatType.RechargeUIText, Module_Charge.CalcCardDays(moduleCharge.MonthEndTime) > 0 ? 24 : 23);
             Window_ItemTip.Show(title, monthChargeItem.info.reward);
        }
        else if (product.info.type == (int) ProductType.SeasonCard)
        {
             var title = ConfigText.GetDefalutString(TextForMatType.RechargeUIText, Module_Charge.CalcCardDays(moduleCharge.SeasonEndTime) > 0 ? 24 : 23);
             Window_ItemTip.Show(title, seasonChargeItem.info.reward);
        }
    }
}
