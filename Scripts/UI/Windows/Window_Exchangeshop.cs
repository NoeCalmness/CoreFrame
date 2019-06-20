/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-10-10
 * 
 ***************************************************************************************************/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Exchangeshop : Window
{
    private Button m_buyBtn;
    private Button m_cancleBtn;
    private GameObject m_maskObj;
    private GameObject m_buyInfoTip;
    private Text m_buyInfoText;
    //徽章
    private Text m_emblemText;
    private Image m_emblemImage;
    private Text m_advCoinText;

    ScrollView view;
    DataSource<PShopItem> dataSource;

    protected override void OnOpen()
    {
        isFullScreen = false;

        m_maskObj = transform.Find("mask").gameObject;
        m_buyBtn = GetComponent<Button>("buy_btn");
        m_cancleBtn = GetComponent<Button>("cancel_btn");
        m_buyInfoTip = transform.Find("text_kuang").gameObject;
        m_buyInfoText = GetComponent<Text>("text_kuang/text");
        m_emblemText = GetComponent<Text>("emblem/text");
        m_emblemImage = GetComponent<Image>("emblem/yongmu");
        m_advCoinText = GetComponent<Text>("coin/text");

        EventTriggerListener.Get(m_maskObj).onClick = OnMaskClick;
        m_buyBtn.onClick.RemoveAllListeners();
        m_buyBtn.onClick.AddListener(OnBuyBtnClick);
        EventTriggerListener.Get(m_cancleBtn.gameObject).onClick = OnMaskClick;

        AtlasHelper.SetCurrencyIcon(m_emblemImage, CurrencySubType.Emblem);

        view = GetComponent<ScrollView>("insideframe/scrollView");
        dataSource = new DataSource<PShopItem>(null, view, OnSetShopItemInfo, OnClickItem);
        IniteText();
    }

    private void OnClickItem(RectTransform node, PShopItem data)
    {
        var shopItem = node.GetComponent<ShopItem>();
        if (shopItem == null) return;
        if (shopItem.isHaved)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.PublicUIText, 12));
            return;
        }
        if (shopItem.isSelled)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.PublicUIText, 11));
            return;
        }

        //选中
        if (moduleShop.curClickItem== null || data != moduleShop.curClickItem)
        {
            moduleShop.lastClickItem = moduleShop.curClickItem;
            moduleShop.curClickItem = data;

            //刷之前选中框
            if (moduleShop.curItmes != null && moduleShop.lastClickItem != null)
            {
                var lastIndex = moduleShop.curItmes.FindIndex((p) => p.itemTypeId == moduleShop.lastClickItem.itemTypeId && p.num == moduleShop.lastClickItem.num);
                dataSource.SetItem(moduleShop.lastClickItem, lastIndex);
            }

            m_buyBtn.interactable = ReturnCanBuy(data);
            m_buyInfoTip.SetActive(true);

            PropItemInfo info = ConfigManager.Get<PropItemInfo>(data.itemTypeId);
            if (info) Util.SetText(m_buyInfoText, (int)TextForMatType.ExChangeShopUIText, 1, info.itemName);
        }
        //取消选中
        else if (moduleShop.curClickItem == data)
            InitExchangePanel();

        OnSetShopItemInfo(node, data);
    }

    private void OnSetShopItemInfo(RectTransform node, PShopItem data)
    {
        ShopItem item = node.GetComponentDefault<ShopItem>();
        item.RefreshUiData(moduleShop.curShopMsg, data);
    }

    private void IniteText()
    {
        var exChangeText = ConfigManager.Get<ConfigText>((int)TextForMatType.ExChangeShopUIText);
        Util.SetText(GetComponent<Text>("title"), exChangeText[0]);
        Util.SetText(GetComponent<Text>("titleBg/title_shadow"), exChangeText[0]);
        Util.SetText(GetComponent<Text>("buy_btn/Image (4)"), exChangeText[2]);
        Util.SetText(GetComponent<Text>("cancel_btn/Image (4)"), exChangeText[3]);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleShop.SetCurShopPos(ShopPos.Maze);

        SetNumber();
    }

    protected override void OnHide(bool forward)
    {
        moduleShop.SetCurShopPos(ShopPos.None);
        moduleShop.curClickItem = null;
        moduleShop.lastClickItem = null;
        moduleShop.SetCurrentShop(null);
    }

    private void SetNumber()
    {
        Util.SetText(m_emblemText, moduleEquip.signetCount.ToString());
        Util.SetText(m_advCoinText, moduleEquip.adventureCount.ToString());
    }

    private void OnMaskClick(GameObject sender)
    {
        Hide(true);
        InitExchangePanel();
    }

    private void OnBuyBtnClick()
    {
        if (moduleShop.curClickItem == null)
            return;

        moduleShop.SendBuyInfo(moduleShop.curShopMsg.shopId, moduleShop.curClickItem.itemTypeId, moduleShop.curClickItem.num);
        m_buyBtn.interactable = false;
    }

    private bool ReturnCanBuy(PShopItem clickItem)
    {
        if (clickItem == null) return false;

        var prop = ConfigManager.Get<PropItemInfo>(clickItem.currencyType);
        if (!prop) return false;

        uint currencyNow = 0;
        if (prop.itemType == PropType.Currency) currencyNow = modulePlayer.GetMoneyCount((CurrencySubType)prop.subType);
        else currencyNow = (uint)moduleEquip.GetPropCount(clickItem.currencyType);

        return currencyNow >= clickItem.currencyNum;
    }

    private void InitExchangePanel()
    {
        moduleShop.curClickItem = null;
        moduleShop.lastClickItem = null;
        m_buyBtn.interactable = false;
        m_buyInfoTip.SetActive(false);
    }

    private void _ME(ModuleEvent<Module_Equip> e)
    {
        if (actived && e.moduleEvent == Module_Equip.EventUpdateBagProp)
            SetNumber();
    }

    void _ME(ModuleEvent<Module_Shop> e)
    {
        if (!actived || moduleShop.curShopPos != ShopPos.Maze) return;
        ShopMessage msg = null;
        switch (e.moduleEvent)
        {
            case Module_Shop.EventShopData:
                var list = e.param1 as List<ShopMessage>;
                if (list == null || list.Count < 1) break;
                moduleShop.SetCurrentShop(list[0]);
                InitExchangePanel();
                break;
            case Module_Shop.EventTargetShopData:
                msg = e.param1 as ShopMessage;
                if (msg != null)
                {
                    dataSource.SetItems(msg.items);
                    view.progress = 0;
                }
                else dataSource.SetItems(null);
                break;
            case Module_Shop.EventPaySuccess:
                Window_ItemTip.Show(ConfigText.GetDefalutString(TextForMatType.TravalShopUIText, 13), moduleShop.curClickItem);
                InitExchangePanel();
                dataSource.UpdateItems();
                break;
            case Module_Shop.EventPromoChanged:
                msg = e.param1 as ShopMessage;
                if (msg != null && msg.pos == ShopPos.Maze)
                {
                    InitExchangePanel();
                    int page = view.currentPage;
                    dataSource.UpdateItems();
                    view.ScrollToPage(page);
                }
                break;
            default: break;
        }
    }
}
