/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   wangyifan <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-07-19
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Liulangshangdian : Window
{
    private Transform randomPanel;
    private Button refreshBtn;
    private Text DiamondBySpend_text;
    private Image payTypeImage;
    private Text nextRefreshTime;
    private Transform otherCurrency;

    private Transform togglePrefab;
    private Transform toggleParent;
    private List<Toggle> toggles=new List<Toggle>();
    private Dictionary<Toggle, ushort> travalToggles = new Dictionary<Toggle, ushort>();

    private Button payBtn;
    private Text pay_tip;

    private Image refreshTip;
    private Button refresh;
    private Image refreshImage;
    private Text remainText;
    private TimeSpan timeNow;//现在的倒计时

    private DataSource<PShopItem> dataSource;
    private ScrollView view;

    List<ushort> costId = new List<ushort>();

    protected override void OnOpen()
    {
        randomPanel = GetComponent<Transform>("right/randomPanel");
        refreshBtn = GetComponent<Button>("right/randomPanel/chongzhi");
        DiamondBySpend_text = GetComponent<Text>("right/randomPanel/myDiamond/myDiamond_text");
        payTypeImage = GetComponent<Image>("right/randomPanel/myDiamond/yongmu");
        nextRefreshTime = GetComponent<Text>("right/randomPanel/nextRefreshTime/time");
        otherCurrency = GetComponent<Transform>("right/otherCurrency");

        toggleParent = GetComponent<Transform>("right/checkBox");
        togglePrefab = GetComponent<Transform>("right/1100");
        togglePrefab.SafeSetActive(false);

        payBtn = GetComponent<Button>("right/maimaianniu");
        pay_tip = GetComponent<Text>("right/text_confirm");

        refreshTip = GetComponent<Image>("center/refreshTip");
        refresh = GetComponent<Button>("center/refreshTip/bg/yes");
        refreshImage = GetComponent<Image>("center/refreshTip/bg/xiaohao_tip/zuan");
        remainText = GetComponent<Text>("center/refreshTip/bg/xiaohao_tip/remain");

        refresh.onClick.RemoveAllListeners();
        refresh.onClick.AddListener(OnClickRefreshYes);
        refreshBtn.onClick.RemoveAllListeners();
        refreshBtn.onClick.AddListener(OnClickRefreshBtn);
        payBtn.onClick.AddListener(() => { OnClickPayBtn(moduleShop.curClickItem); });

        view = GetComponent<ScrollView>("right/qkt");
        dataSource = new DataSource<PShopItem>(null, view, OnSetItemInfo, OnClickShopItem);

        InitText();
    }

    #region ScrollView

    private void OnClickShopItem(RectTransform node, PShopItem data)
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
        if (moduleShop.curClickItem != null && moduleShop.curClickItem == data) return;

        moduleShop.lastClickItem = moduleShop.curClickItem;
        moduleShop.curClickItem = data;

        OnSetItemInfo(node, data);
        //刷之前选中框
        if (moduleShop.curItmes != null && moduleShop.lastClickItem != null)
        {
            var lastIndex = moduleShop.curItmes.FindIndex((p) => p.itemTypeId == moduleShop.lastClickItem.itemTypeId && p.num == moduleShop.lastClickItem.num);
            dataSource.SetItem(moduleShop.lastClickItem, lastIndex);
        }

        payBtn.interactable = true;
        pay_tip.gameObject.SetActive(true);
        var info = ConfigManager.Get<PropItemInfo>(data.itemTypeId);
        if (info) Util.SetText(pay_tip, (int)TextForMatType.PublicUIText, 20, info.itemName);
    }

    private void OnSetItemInfo(RectTransform node, PShopItem data)
    {
        ShopItem item = node.GetComponentDefault<ShopItem>();
        item.RefreshUiData(moduleShop.curShopMsg, data);
    }
    #endregion

    #region window_base

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();

        moduleShop.SetCurShopPos(ShopPos.Traval);
    }

    protected override void OnReturn()
    {
        base.OnReturn();
        costId.Clear();
    }

    protected override void OnHide(bool forward)
    {
        moduleShop.SetCurShopPos(ShopPos.None);
        moduleShop.curClickItem = null;
        moduleShop.lastClickItem = null;
        moduleShop.SetCurrentShop(null);

        for (int i = 0; i < toggles.Count; i++)
        {
            toggles[i].SafeSetActive(false);
            toggles[i].isOn = false;
            toggles[i].SafeSetActive(true);
        }
    }

    public override void OnRenderUpdate()
    {
        if (moduleShop.curShopMsg == null || !moduleShop.curShopMsg.isRandom) return;
        int times = moduleShop.GetRefreshTime(moduleShop.curShopMsg.shopId);
        if (times == -1) return;
        timeNow = new TimeSpan(0, 0, times);
        nextRefreshTime.text = Util.Format("{0}:{1}:{2}", timeNow.Hours.ToString("D2"), timeNow.Minutes.ToString("D2"), timeNow.Seconds.ToString("D2"));
    }

    private void InitText()
    {
        var travalText = ConfigManager.Get<ConfigText>((int)TextForMatType.TravalShopUIText);
        Util.SetText(GetComponent<Text>("bg/biaoti/title_big"), travalText[0]);
        Util.SetText(GetComponent<Text>("bg/biaoti/title_small"), travalText[1]);
        Util.SetText(GetComponent<Text>("right/randomPanel/nextRefreshTime/wenzi"), travalText[5]);
        Util.SetText(GetComponent<Text>("center/refreshTip/bg/detail_content"), travalText[6]);

        var publicText = ConfigManager.Get<ConfigText>((int)TextForMatType.PublicUIText);
        Util.SetText(GetComponent<Text>("right/maimaianniu/buy_text"), publicText[3]);
        Util.SetText(GetComponent<Text>("center/refreshTip/bg/yes/yes_text"), publicText[4]);
        Util.SetText(GetComponent<Text>("center/refreshTip/bg/nobtn/no_text"), publicText[5]);
        Util.SetText(GetComponent<Text>("center/refreshTip/bg/equipinfo"), publicText[6]);
        Util.SetText(GetComponent<Text>("center/refreshTip/bg/xiaohao_tip/xiaohao"), publicText[15]);
    }

    private void EnterShop(int index)
    {
        if (toggles == null || toggles.Count < 1) return;
        if (index == -1) index = 0;

        if (!toggles[index].isOn) toggles[index].isOn = true;
        else OnValueChangeCheck(true);
    }

    private void CreateToggles(List<ShopMessage> msgs)
    {
        travalToggles.Clear();
        toggles.Clear();
        for (int i = 0; i < msgs.Count; i++)
        {
            var tran = toggleParent.childCount > i ? toggleParent.GetChild(i) : null;
            if (tran == null) tran = toggleParent.AddNewChild(togglePrefab);
            if (!tran) continue;

            tran.SafeSetActive(true);
            tran.name = msgs[i].shopId.ToString();
            AtlasHelper.SetIcons(tran.Find("icon"), msgs[i].icon);
            Util.SetText(tran.Find("Text")?.gameObject, msgs[i].name);
            Util.SetText(tran.Find("xz/_text")?.gameObject, msgs[i].name);

            var toggle = tran.GetComponentDefault<Toggle>();
            toggles.Add(toggle);
            if (!travalToggles.ContainsKey(toggle)) travalToggles.Add(toggle, msgs[i].shopId);

            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(OnValueChangeCheck);
        }

        for (int i = 0; i < toggles.Count; i++)
        {
            toggles[i].SafeSetActive(false);
            toggles[i].isOn = false;
            toggles[i].SafeSetActive(true);
        }
    }

    #endregion

    #region OnClick

    private void OnValueChangeCheck(bool arg0)
    {
        if (arg0)
        {
            for (int i = 0; i < toggles.Count; i++)
            {
                if (toggles[i].isOn)
                {
                    var id = travalToggles.Get(toggles[i]);
                    var msg = moduleShop.GetTargetShop(id);
                    if (msg != null)
                    {
                        moduleShop.SetCurrentShop(msg);
                        break;
                    }
                }
            }

            SetRefreshBtnState();//设置刷新按钮和货币的状态
            if (moduleShop.curShopMsg.isRandom)
                RefreshExpense();//设置刷新购买面板

            SetSelectDefault();
        }
    }

    private void SetRefreshBtnState()
    {
        bool isRandom = moduleShop.curShopMsg.isRandom;

        randomPanel.SafeSetActive(isRandom);
        if (isRandom)
        {
            ushort id = moduleShop.curShopMsg.refreshCurrency;
            var prop = ConfigManager.Get<PropItemInfo>(id);
            if (prop)
            {
                uint count = 0;
                if (prop.itemType == PropType.Currency)
                    count = modulePlayer.GetMoneyCount((CurrencySubType)prop.subType);
                else count = (uint)moduleEquip.GetPropCount(id);

                refreshBtn.interactable = count >= moduleShop.curShopMsg.refreshNum;
            }
        }

        otherCurrency.SafeSetActive(!isRandom);
        if (!isRandom) RefreshOtherCurrency();
    }

    private void RefreshOtherCurrency()
    {
        var list = moduleShop.curShopMsg.items;
        if (list == null || list.Length < 1) return;

        costId.Clear();
        for (int i = 0; i < list.Length; i++)
        {
            if (costId.Contains(list[i].currencyType)) continue;
            costId.Add(list[i].currencyType);
        }

        for (int i = 0; i < otherCurrency.childCount; i++)
        {
            var tran = otherCurrency.GetChild(i);
            if (tran == null) continue;
            if (i < costId.Count)
            {
                int id = costId[i];
                var prop = ConfigManager.Get<PropItemInfo>(id);

                if (!prop)
                {
                    tran.SafeSetActive(false);
                    continue;
                }

                bool isCurrency = prop.itemType == PropType.Currency && ((CurrencySubType)prop.subType == CurrencySubType.Gold || (CurrencySubType)prop.subType == CurrencySubType.Diamond);
                if (isCurrency)
                {
                    tran.SafeSetActive(false);
                    continue;
                }

                uint count = 0;
                isCurrency = prop.itemType == PropType.Currency && (CurrencySubType)prop.subType > CurrencySubType.None && (CurrencySubType)prop.subType < CurrencySubType.Count;
                if (isCurrency) count = modulePlayer.GetMoneyCount((CurrencySubType)prop.subType);
                else count = (uint)moduleEquip.GetPropCount(id);

                tran.SafeSetActive(true);
                AtlasHelper.SetIcons(tran.Find("icon"), prop.icon);
                Util.SetText(tran.Find("num")?.gameObject, "×" + count);
                var btn = tran.GetComponentDefault<Button>();
                if (btn)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => moduleGlobal.UpdateGlobalTip((ushort)id, true));
                }
            }
            else tran.SafeSetActive(false);
        }
    }

    private void RefreshExpense()
    {
        var info = ConfigManager.Get<PropItemInfo>(moduleShop.curShopMsg.refreshCurrency);
        if (info != null) AtlasHelper.SetItemIcon(payTypeImage, info);

        Util.SetText(DiamondBySpend_text, "×" + moduleShop.curShopMsg.refreshNum);
    }

    private void OnClickPayBtn(PShopItem item)
    {
        if (item == null)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.PublicUIText, 14));
            return;
        }

        ushort id = moduleShop.curShopMsg.shopId;
        moduleShop.SendBuyInfo(id, item.itemTypeId, item.num);

        payBtn.interactable = false;
    }

    private void OnClickRefreshBtn()
    {
        if (!moduleShop.curShopMsg.isRandom) return;

        BtnRemainText();
        refreshTip.gameObject.SetActive(true);
    }

    private void BtnRemainText()
    {
        int id = moduleShop.curShopMsg.refreshCurrency;
        var prop = ConfigManager.Get<PropItemInfo>(id);
        if (!prop) return;

        uint currency = 0;
        if (prop.itemType == PropType.Currency) currency = modulePlayer.GetMoneyCount((CurrencySubType)prop.subType);
        else currency = (uint)moduleEquip.GetPropCount(id);

        uint cost = moduleShop.curShopMsg.refreshNum;
        if (currency < cost)
        {            
            moduleGlobal.ShowMessage(Util.Format(ConfigText.GetDefalutString(TextForMatType.PublicUIText, 31), prop.itemName));
            return;
        }

        AtlasHelper.SetIcons(refreshImage, prop.icon);
        var str = Util.Format(ConfigText.GetDefalutString(TextForMatType.TravalShopUIText, 11), cost, currency);
        Util.SetText(remainText, str);
    }

    private void OnClickRefreshYes()
    {
        if (!moduleShop.curShopMsg.isRandom) return;

        moduleShop.SendRefreshShop(moduleShop.curShopMsg.shopId);
        refreshTip.gameObject.SetActive(false);
    }

    #endregion

    #region other

    private void SetSelectDefault()
    {
        moduleShop.curClickItem = null;
        moduleShop.lastClickItem = null;
        pay_tip.SafeSetActive(false);
        payBtn.interactable = false;
        refreshTip.SafeSetActive(false);
    }

    #endregion

    #region _ME

    void _ME(ModuleEvent<Module_Shop> e)
    {
        if (!actived || moduleShop.curShopPos != ShopPos.Traval) return;
        ShopMessage msg = null;
        switch (e.moduleEvent)
        {
            case Module_Shop.EventShopData:
                var list = e.param1 as List<ShopMessage>;
                if (list == null || list.Count < 1) break;
                CreateToggles(list);
                EnterShop(m_subTypeLock);
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
                payBtn.interactable = true;
                SetRefreshBtnState();
                Window_ItemTip.Show(ConfigText.GetDefalutString(TextForMatType.TravalShopUIText, 13), moduleShop.curClickItem);
                if (moduleShop.curShopMsg.isRandom) SetSelectDefault();
                int page = view.currentPage;
                dataSource.UpdateItems();
                view.ScrollToPage(page);
                break;
            case Module_Shop.EventPromoChanged:
                msg = e.param1 as ShopMessage;
                if (msg != null && msg.pos == ShopPos.Traval) OnValueChangeCheck(true);
                break;
            default: break;
        }
    }

    //void _ME(ModuleEvent<Module_Player> e)
    //{
    //    if (e.moduleEvent == Module_Player.EventCurrencyChanged && actived)
    //        SetRefreshBtnState();
    //}

    #endregion
}