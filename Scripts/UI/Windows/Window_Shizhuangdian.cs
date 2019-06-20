/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   wangyifan <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-07-10
 * 
 ***************************************************************************************************/

using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Shizhuangdian : Window
{
    private Button changeBig;
    private Button changeSmall;
    private Slider changeSlider;
    private Button m_reset;
    private Button m_pay;
    private Text pay_tip;

    private Text m_titleTxt;
    private Button m_goEquipCloth;

    private List<Toggle> toggles = new List<Toggle>();

    private int level;
    Creature creatureInFashion;
    Camera uiCamera;

    DataSource<PShopItem> dataSource;
    private ScrollView view;
    private List<PShopItem> itemlist = new List<PShopItem>();

    protected override void OnOpen()
    {
        changeBig = GetComponent<Button>("left/_jiahao");
        changeSmall = GetComponent<Button>("left/_jianhao");
        changeSlider = GetComponent<Slider>("left/slider");
        changeSlider.interactable = false;
        m_reset = GetComponent<Button>("left/chongzhi");
        m_pay = GetComponent<Button>("right/maimaianniu");
        pay_tip = GetComponent<Text>("right/payTip_text");
        m_titleTxt = GetComponent<Text>("title_txt");
        m_goEquipCloth = GetComponent<Button>("left/changeCloth");
        m_goEquipCloth.onClick.RemoveAllListeners();
        m_goEquipCloth.onClick.AddListener(delegate
        {
            moduleAnnouncement.OpenWindow(9, (int)EnumSubEquipWindowType.ChangeNonIntentyEquipPanel);
        });
        
        toggles.Clear();
        Toggle cloth = GetComponent<Toggle>("right/checkBoxce/taozhuang");
        Toggle head = GetComponent<Toggle>("right/checkBoxce/head");
        Toggle hair = GetComponent<Toggle>("right/checkBoxce/hair");
        Toggle face = GetComponent<Toggle>("right/checkBoxce/face");
        Toggle neck = GetComponent<Toggle>("right/checkBoxce/neck");
        Toggle limited = GetComponent<Toggle>("right/checkBoxce/xianshi");
        toggles.Add(cloth);
        toggles.Add(head);
        toggles.Add(hair);
        toggles.Add(face);
        toggles.Add(neck);
        toggles.Add(limited);

        for (int i = 0; i < toggles.Count; i++)
        {
            toggles[i].SafeSetActive(false);
            toggles[i].isOn = false;
            toggles[i].SafeSetActive(true);

            toggles[i].onValueChanged.RemoveAllListeners();
            toggles[i].onValueChanged.AddListener(OnCheckBoxValueChange);
        }

        changeBig.onClick.RemoveAllListeners();
        changeBig.onClick.AddListener(OnChangeBig);
        changeSmall.onClick.RemoveAllListeners();
        changeSmall.onClick.AddListener(OnChangeSmall);
        m_reset.onClick.RemoveAllListeners();
        m_reset.onClick.AddListener(OnReset);
        m_pay.onClick.RemoveAllListeners();
        m_pay.onClick.AddListener(() => { OnPay(moduleShop.curClickItem); });

        creatureInFashion = ObjectManager.FindObject<Creature>(c => c.isPlayer);

        uiCamera = Level.current.mainCamera;

        view = GetComponent<ScrollView>("right/qkt");
        dataSource = new DataSource<PShopItem>(null, view, OnSetItemInfo, OnClickItem);

        IniteText();
    }

    private void IniteText()
    {
        var fashionText = ConfigManager.Get<ConfigText>((int)TextForMatType.FashionShopUIText);
        Util.SetText(GetComponent<Text>("right/checkBoxce/taozhuang/taozhuang_text"), fashionText[0]);
        Util.SetText(GetComponent<Text>("right/checkBoxce/taozhuang/xz/taozhuang_text"), fashionText[0]);
        Util.SetText(GetComponent<Text>("right/checkBoxce/faxing/faxing_text"), fashionText[1]);
        Util.SetText(GetComponent<Text>("right/checkBoxce/faxing/xz/faxing_text"), fashionText[1]);

        Util.SetText(GetComponent<Text>("right/checkBoxce/head/shipin_text"), fashionText[2]);
        Util.SetText(GetComponent<Text>("right/checkBoxce/head/xz/shipin_text"), fashionText[2]);
        Util.SetText(GetComponent<Text>("right/checkBoxce/hair/shipin_text"), fashionText[9]);
        Util.SetText(GetComponent<Text>("right/checkBoxce/hair/xz/shipin_text"), fashionText[9]);
        Util.SetText(GetComponent<Text>("right/checkBoxce/face/shipin_text"), fashionText[10]);
        Util.SetText(GetComponent<Text>("right/checkBoxce/face/xz/shipin_text"), fashionText[10]);
        Util.SetText(GetComponent<Text>("right/checkBoxce/neck/shipin_text"), fashionText[11]);
        Util.SetText(GetComponent<Text>("right/checkBoxce/neck/xz/shipin_text"), fashionText[11]);

        Util.SetText(GetComponent<Text>("right/checkBoxce/xianshi/zhekou_text"), fashionText[3]);
        Util.SetText(GetComponent<Text>("right/checkBoxce/xianshi/xz/zhekou_text"), fashionText[3]);
        Util.SetText(GetComponent<Text>("left/chongzhi/chongzhi_text"), fashionText[4]);
        Util.SetText(GetComponent<Text>("bg/biaoti/title_big"), fashionText[7]);
        Util.SetText(GetComponent<Text>("bg/biaoti/title_small"), fashionText[8]);

        var publicText = ConfigManager.Get<ConfigText>((int)TextForMatType.PublicUIText);
        Util.SetText(GetComponent<Text>("right/maimaianniu/buy_text"), publicText[3]);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();
        moduleShop.SetCurShopPos(ShopPos.Fashion);

        moduleShop.OnAddDefaultData();
        moduleHome.HideOthers("player"); // 显示玩家角色
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

        if (creatureInFashion) moduleShop.ChangeDefaultCloth(creatureInFashion);
    }

    private void EnterShop(int index)
    {
        if (index == -1) index = 0;
        if (!toggles[index].isOn) toggles[index].isOn = true;
        else OnCheckBoxValueChange(true);
        
        level = GeneralConfigInfo.defaultConfig.defaultLevel;
        changeSlider.value = 0.25f * level;
        ChangeCameraData(level, true);
    }

    private void OnSetItemInfo(RectTransform node, PShopItem data)
    {
        ShopItem item = node.GetComponentDefault<ShopItem>();
        item.RefreshUiData(moduleShop.curShopMsg, data);
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
        if (moduleShop.curClickItem != null && moduleShop.curClickItem == data) return;

        moduleShop.lastClickItem = moduleShop.curClickItem;
        moduleShop.curClickItem = data;
        moduleShop.ChangeNewFashion(data, creatureInFashion);

        OnSetItemInfo(node, data);
        //刷之前选中框
        if (itemlist != null && moduleShop.lastClickItem != null)
        {
            var lastIndex = itemlist.FindIndex((p) => p.itemTypeId == moduleShop.lastClickItem.itemTypeId && p.num == moduleShop.lastClickItem.num);
            dataSource.SetItem(moduleShop.lastClickItem, lastIndex);
        }

        m_pay.interactable = true;
        pay_tip.gameObject.SetActive(true);
        var info = ConfigManager.Get<PropItemInfo>(data.itemTypeId);
        if (info) Util.SetText(pay_tip, (int)TextForMatType.PublicUIText, 20, info.itemName);
    }

    private void SetSelectDefault()
    {
        moduleShop.curClickItem = null;
        moduleShop.lastClickItem = null;
        pay_tip.gameObject.SetActive(false);
        m_pay.interactable = false;
    }

    #region 调整角色大小
    private void OnReset()
    {
        if (creatureInFashion) moduleShop.ChangeDefaultCloth(creatureInFashion);

        OnCheckBoxValueChange(true);

        level = GeneralConfigInfo.defaultConfig.defaultLevel;
        ChangeCameraData(level);
    }

    private void OnChangeSmall()
    {
        level--;
        if (level > 4)
        {
            level = 4;
            return;
        }
        else if (level < 0)
        {
            level = 0;
            return;
        }
        ChangeCameraData(level);
    }

    private void OnChangeBig()
    {
        level++;
        if (level > 4)
        {
            level = 4;
            return;
        }
        else if (level < 0)
        {
            level = 0;
            return;
        }
        ChangeCameraData(level);
    }

    private void ChangeCameraData(int level, bool enter = false)
    {
        DOTween.Kill(uiCamera);

        var info = ConfigManager.Get<ShowCreatureInfo>(modulePlayer.proto);
        if (!info || info.forData.Length < 0) return;

        ShowCreatureInfo.SizeAndPos data = null;
        for (int i = 0; i < info.forData.Length; i++)
        {
            if (info.forData[i].index == level)
            {
                data = info.forData[i].data[0];
                break;
            }
        }
        if (enter)
        {
            uiCamera.transform.position = data.pos;
            uiCamera.transform.eulerAngles = data.rotation;
        }
        else
        {
            uiCamera.transform.DOLocalMove(data.pos, 0.2f).SetEase(Ease.Linear);
            uiCamera.transform.DOLocalRotate(data.rotation, 0.2f).SetEase(Ease.Linear);
        }

        changeSlider.value = 0.25f * level;
    }

    #endregion

    #region 界面逻辑

    private void OnCheckBoxValueChange(bool arg0)
    {
        if (arg0)
        {
            SetSelectDefault();

            FashionType type = FashionType.None;
            for (int i = 0; i < toggles.Count; i++)
            {
                if (toggles[i].isOn)
                {
                    type = (FashionType)(i + 1);
                    break;
                }
            }
            moduleShop.curFashionType = type;

            if (type == FashionType.Cloth) Util.SetText(m_titleTxt, 208, 0);
            else if (type == FashionType.HeadDress) Util.SetText(m_titleTxt, 208, 2);
            else if (type == FashionType.HairDress) Util.SetText(m_titleTxt, 208, 9);
            else if (type == FashionType.FaceDress) Util.SetText(m_titleTxt, 208, 10);
            else if (type == FashionType.NeckDress) Util.SetText(m_titleTxt, 208, 11);
            else if (type == FashionType.Limited) Util.SetText(m_titleTxt, 208, 3);

            if (moduleShop.fashionShop.ContainsKey(type))
            {
                itemlist = moduleShop.fashionShop[type];
                dataSource.SetItems(itemlist);
            }
            else dataSource.SetItems(null);
            view.progress = 0;
        }
    }

    private void OnPay(PShopItem p)
    {
        if (p == null)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.PublicUIText, 14));
            return;
        }

        ushort id = moduleShop.curShopMsg.shopId;
        moduleShop.SendBuyInfo(id, p.itemTypeId, p.num);

        m_pay.interactable = false;
    }

    void _ME(ModuleEvent<Module_Shop> e)
    {
        if (!actived || moduleShop.curShopPos != ShopPos.Fashion) return;
        switch (e.moduleEvent)
        {
            case Module_Shop.EventShopData:
                var list = e.param1 as List<ShopMessage>;
                if (list == null || list.Count < 1) break;
                moduleShop.SetCurrentShop(list[0]);
                break;
            case Module_Shop.EventTargetShopData:
                EnterShop(m_subTypeLock);
                break;
            case Module_Shop.EventPaySuccess:
                Window_ItemTip.Show(ConfigText.GetDefalutString(TextForMatType.TravalShopUIText, 13), moduleShop.curClickItem);
                SetSelectDefault();
                int page = view.currentPage;
                dataSource.UpdateItems();
                view.ScrollToPage(page);
                break;
            case Module_Shop.EventPromoChanged:
                var msg = e.param1 as ShopMessage;
                if (msg != null && msg.pos == ShopPos.Fashion) OnCheckBoxValueChange(true);
                break;
            default: break;
        }
    }

    #endregion
}
