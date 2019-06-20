using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChargeWindow_Sale : SubWindowBase<Window_Welfare>
{
    //新增充值类
    private Image m_icon;
    private Button m_buyBtn;
    private Text m_oldPrice;
    private Text m_salePrice;
    private Text m_buyTxt;

    private ScrollView m_scroll;
    private DataSource<ItemPair> m_data;
    private PChargeItem m_chargeItem;

    protected override void InitComponent()
    {
        base.InitComponent();
        m_icon      = Root.GetComponent<Image>("title");
        m_buyBtn    = Root.GetComponent<Button>("reward_panel/Button");
        m_buyTxt    = Root.GetComponent<Text>("reward_panel/Button/Text");
        m_oldPrice  = Root.GetComponent<Text>("reward_panel/Text");
        m_salePrice = Root.GetComponent<Text>("reward_panel/old");
        m_scroll    = Root.GetComponent<ScrollView>("reward_panel/scrollView");
        m_data      = new DataSource<ItemPair>(null, m_scroll, SetInfo, SetClick);
    }
    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p)) return false;
        m_buyBtn.onClick.RemoveAllListeners();
        m_buyBtn.onClick.AddListener(delegate
        {
            if (m_chargeItem == null) return;
            moduleCharge.RequestBuyProduct(m_chargeItem);
        });
        return true;
    }
    protected override void RefreshData(params object[] p)
    {
        SetProductType();
        RefreshInfo();
    }

    private void RefreshInfo()
    {
        if (m_chargeItem == null) return;

        var list = parentWindow.ItemList(m_chargeItem.info.reward);
        list.Sort((a, b) => a.itemId.CompareTo(b.itemId));
        m_data.SetItems(list);
        
        UIDynamicImage.LoadImage(m_icon.transform, m_chargeItem.info.icon);

        var old = Util.GetChargeCurrencySymbol((ChargeCurrencyTypes)m_chargeItem.info.currencyType) + m_chargeItem.info.price.ToString();
        var sale = Util.GetChargeCurrencySymbol((ChargeCurrencyTypes)m_chargeItem.info.currencyType) + m_chargeItem.info.cost.ToString();
        Util.SetText(m_oldPrice, old); ;
        Util.SetText(m_salePrice, sale);
        SetBtnState();
    }

    private void SetBtnState()
    {
        bool buy = (m_chargeItem.DailyBuyTimes() < m_chargeItem.info.cycleTotalBuyLimit && m_chargeItem.TotalBuyTimes() < m_chargeItem.info.allTotalBuyLimit);

        m_buyBtn.interactable = buy;
        if (buy) Util.SetText(m_buyTxt, 246, 35);
        else Util.SetText(m_buyTxt, 246, 36);
    }

    private void SetProductType()
    {
        var charge = moduleCharge.GetChargeItemListById(parentWindow.GetChargeID());
        if (charge != null && charge.Count > 0) m_chargeItem = charge[0];
    }

    private void SetInfo(RectTransform rt, ItemPair item)
    {
        if (item == null) return;
        var prop = ConfigManager.Get<PropItemInfo>(item.itemId);
        Util.SetItemInfo(rt, prop, 0, item.count);
    }
    private void SetClick(RectTransform rt, ItemPair item)
    {
        if (item == null) return;
        moduleGlobal.UpdateGlobalTip((ushort)item.itemId, true, false);
    }

    private void _ME(ModuleEvent<Module_Charge> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Charge.NoticeChargeBuySuccess:
                Window_ItemTip.Show(ConfigText.GetDefalutString(246, 37), moduleCharge.CurrentDeal.info.reward);
                SetBtnState();
                break;
            case Module_Charge.NoticeChargeCountChange:
                SetBtnState();
                break;
        }
    }

}
