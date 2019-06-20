/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-04-02
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_GiftDating : Window
{
    #region public
    private Button closeBtn;
    private Button closeBtn2;
    private GiftPanel type;
    private Text tittle;
    //经验条相关
    private Transform sliderParent;
    private Image topImage;
    private Text lv;
    private Text addExp;
    private Text exp_text;
    //tip相关
    private Text tip_text;
    private Transform tipParent;
    //toggles
    private List<Toggle> toggles = new List<Toggle>();
    #endregion

    #region 送礼界面
    private DataSource<PItem> giftData;
    private ScrollView giftView;
    private Button giveGiftBtn;

    private Transform noGiftRect;
    #endregion


    #region NPC时装界面
    private DataSource<PShopItem> suiteData;
    private List<PShopItem> itemList = new List<PShopItem>();
    private ScrollView suiteView;
    private Button unlockBtn;
    private Button equipBtn;
    private Image check_shopimage;
    private Text check_shoptext;
    private Text check_xzshoptext;
    private Transform unlockTip;
    private Text unlockInfo;
    private Text costDiamond;
    private Button unlockYes;
    private string curMesh;
    #endregion

    public Canvas giftCavas { get; private set; }

    protected override void OnOpen()
    {
        isFullScreen = false;

        #region public
        closeBtn2 = GetComponent<Button>("frame/top/closebtn");
        tittle = GetComponent<Text>("frame/top/title");
        sliderParent = GetComponent<Transform>("frame/slider");
        topImage = GetComponent<Image>("frame/slider/topSlider");
        exp_text = GetComponent<Text>("frame/slider/expNumber");
        lv = GetComponent<Text>("frame/slider/level/Text");
        addExp = GetComponent<Text>("frame/slider/expNumber_preview");
        tip_text = GetComponent<Text>("frame/tip/Text");
        tipParent = GetComponent<Transform>("frame/tip");

        closeBtn2?.onClick.RemoveAllListeners();
        closeBtn2?.onClick.AddListener(OnCloseGift);

        toggles.Clear();
        Toggle giftToggle = GetComponent<Toggle>("frame/checkBox/npcInfo_Toggle");
        Toggle npcCloth = GetComponent<Toggle>("frame/checkBox/npcSuit_Toggle");
        toggles.Add(giftToggle);
        toggles.Add(npcCloth);

        for (int i = 0; i < toggles.Count; i++)
        {
            toggles[i].SafeSetActive(false);
            toggles[i].isOn = false;
            toggles[i].SafeSetActive(true);

            toggles[i].onValueChanged.RemoveAllListeners();
            toggles[i].onValueChanged.AddListener(OnValueChanged);
        }
        #endregion

        #region giftPanel

        giveGiftBtn = GetComponent<Button>("gift_Panel/haveGoods/giveBtn");
        noGiftRect = GetComponent<Transform>("gift_Panel/noneGoods");
        giftView = GetComponent<ScrollView>("gift_Panel/haveGoods/itemList");
        giftData = new DataSource<PItem>(null, giftView, OnSetGiftItem, OnClickGiftItem);

        giveGiftBtn.onClick.RemoveAllListeners();
        giveGiftBtn.onClick.AddListener(OnGiveGift);
        #endregion

        giftCavas = GetComponent<Canvas>();
        IniteText();
    }

    private void OnUnlockYes()
    {
        if (moduleShop.curClickItem == null || moduleShop.curShopMsg == null) return;

        moduleShop.SendBuyInfo(moduleShop.curShopMsg.shopId, moduleShop.curClickItem.itemTypeId, moduleShop.curClickItem.num);
    }

    private void OnClickSuiteItem(RectTransform node, PShopItem data)
    {
        var shopItem = node.GetComponent<ShopItem>();
        if (shopItem == null) return;

        if (moduleShop.curClickItem != null && moduleShop.curClickItem == data) return;

        moduleShop.lastClickItem = moduleShop.curClickItem;
        moduleShop.curClickItem = data;

        OnSetNpcSuiteItem(node, data);
        //刷之前选中框
        if (itemList != null && moduleShop.lastClickItem != null)
        {
            var lastIndex = itemList.FindIndex((p) => p.itemTypeId == moduleShop.lastClickItem.itemTypeId && p.num == moduleShop.lastClickItem.num);
            suiteData.SetItem(moduleShop.lastClickItem, lastIndex);
        }

        var prop = ConfigManager.Get<PropItemInfo>(data.itemTypeId);
        if (prop)
        {
            var mode = prop.mesh != null && prop.mesh.Length > 0 ? prop.mesh[0] : "";
            ChangeNpcMode(mode);
        }

        RefreshNpcPanel(shopItem.isEquip ? 0 : shopItem.isHaved ? 2 : 1);
    }

    private void OnSetNpcSuiteItem(RectTransform node, PShopItem data)
    {
        var shopItem = node.GetComponentDefault<ShopItem>();
        if (moduleShop.curShopMsg != null) shopItem.RefreshUiData(moduleShop.curShopMsg, data);

        if (shopItem.isEquip && moduleShop.lastClickItem == null) RefreshNpcPanel();
    }

    private void OnClickGiftItem(RectTransform node, PItem data)
    {
        if (moduleGiftDating.curGift != null && moduleGiftDating.curGift.itemId == data.itemId) return;

        moduleGiftDating.curGift = data;
        giftData.UpdateItems();
        RefreshGiftPanel();
    }

    private void OnSetGiftItem(RectTransform node, PItem data)
    {
        if (data == null) return;
        var info = ConfigManager.Get<PropItemInfo>(data.itemTypeId);
        Util.SetItemInfo(node, info, 0, (int)data.num, false);
        var select = node.Find("selectBox");
        select.SafeSetActive(moduleGiftDating.curGift != null && moduleGiftDating.curGift.itemId == data.itemId);
    }

    private void OnValueChanged(bool arg0)
    {
        if (!arg0 || moduleNPCDating.curDatingNpcMsg == null) return;

        DefaultState();
        for (int i = 0; i < toggles.Count; i++)
        {
            if (toggles[i].isOn)
            {
                type = (GiftPanel)i;

                if (type == GiftPanel.Gift)
                {
                    Util.SetText(tittle, (int)TextForMatType.GiftUIText, 11);
                    moduleGiftDating.GetGiftList();

                    //moduleHome.HideOthers(moduleNPCDating.curDatingNpcMsg.uiName);
                }
                else
                {
                    var str = moduleShop.curShopMsg != null ? moduleShop.curShopMsg.name : Util.GetString((int)TextForMatType.GiftUIText, 14);
                    Util.SetText(tittle, str);

                    if (moduleShop.npcShop.ContainsKey(moduleNPCDating.curDatingNpcMsg.npcId))
                    {
                        itemList = moduleShop.npcShop[moduleNPCDating.curDatingNpcMsg.npcId];
                        suiteData.SetItems(itemList);
                    }
                    else suiteData.SetItems(null);

                    RefreshNpcPanel();
                }
            }
        }
    }

    private void DefaultState()
    {
        moduleGiftDating.curGift = null;
        moduleShop.curClickItem = null;
        moduleShop.lastClickItem = null;
    }

    private void IniteText()
    {
        var id = (int)TextForMatType.GiftUIText;
        Util.SetText(GetComponent<Text>("frame/tipAndSlider/slider/text"), id, 1);
        Util.SetText(GetComponent<Text>("gift_Panel/haveGoods/giveBtn/Text"), id, 3);
        Util.SetText(GetComponent<Text>("gift_Panel/noneGoods/tipback/Text"), id, 4);
        Util.SetText(GetComponent<Text>("frame/checkBox/npcInfo_Toggle/Text"), id, 11);
        Util.SetText(GetComponent<Text>("frame/checkBox/npcInfo_Toggle/xz/_text"), id, 11);
        Util.SetText(GetComponent<Text>("npcSuit_Panel/haveGoods/equipBtn/Text"), id, 12);
        Util.SetText(GetComponent<Text>("npcSuit_Panel/haveGoods/unlockBtn/Text"), id, 13);

        Util.SetText(GetComponent<Text>("popup/top/equipinfo"), 200, 6);
        Util.SetText(GetComponent<Text>("popup/content/cost"), 200, 15);
        Util.SetText(GetComponent<Text>("popup/yes/Text"), 200, 4);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleShop.SetCurShopPos(ShopPos.Npc);
        OnEnterState();
    }

    private void OnEnterState()
    {
        if (toggles == null || toggles.Count < 1 || moduleNPCDating.curDatingNpcMsg == null) return;
        curMesh = moduleNPCDating.curDatingNpcMsg.mode;

        if (!toggles[0].isOn) toggles[0].isOn = true;
        else OnValueChanged(true);
    }

    private void OnCloseGift()
    {
        Hide();
        moduleNPCDating.ContinueBehaviourCallBack();
    }

    protected override void OnHide(bool forward)
    {
        DispatchEvent(Module_Gift.NpcPlayGiveGiftEvent);

        OnHideTodo();
        //删除多余模型
        RestoreModes();

        moduleNPCDating.ContinueBehaviourCallBack();
    }

    private void OnHideTodo()
    {
        moduleGiftDating.curGift = null;

        moduleShop.SetCurShopPos(ShopPos.None);
        moduleShop.curClickItem = null;
        moduleShop.lastClickItem = null;
        moduleShop.SetCurrentShop(null);

        var window_liu = GetOpenedWindow<Window_Liulangshangdian>();
        if (window_liu != null && window_liu.actived)
        {
            moduleShop.SetCurShopPos(ShopPos.Traval, false);
            moduleShop.HideGiftToDo();
        }

        if (toggles == null || toggles.Count < 1) return;
        for (int i = 0; i < toggles.Count; i++)
        {
            toggles[i].SafeSetActive(false);
            toggles[i].isOn = false;
            toggles[i].SafeSetActive(true);
        }

        //if (moduleNPCDating.curDatingNpcMsg != null) moduleHome.HideOthers(moduleNPCDating.curDatingNpcMsg.uiName);
    }

    /// <summary>
    /// 送礼
    /// </summary>
    private void OnGiveGift()
    {
        if (moduleGiftDating.curGift == null)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.GiftUIText, 5));
            return;
        }
        if (moduleNPCDating.curDatingNpcMsg == null) return;

        if (moduleNPCDating.curDatingNpcMsg.fetterLv >= moduleNPCDating.curDatingNpcMsg.maxFetterLv)
        {
            moduleGlobal.ShowMessage(Util.Format(ConfigText.GetDefalutString(TextForMatType.GiftUIText, 6), moduleNPCDating.curDatingNpcMsg.name));
            OnCloseGift();
            return;
        }
        moduleGiftDating.SendUseGift(moduleNPCDating.curDatingNpcMsg.npcId, moduleGiftDating.curGift.itemId, 1);
    }

    private void RefreshGiftPanel()
    {
        if (moduleNPCDating.curDatingNpcMsg == null) return;
        sliderParent.SafeSetActive(moduleGiftDating.gifts.Count > 0);
        noGiftRect.SafeSetActive(moduleGiftDating.gifts.Count < 1);
        closeBtn2?.SafeSetActive(moduleGiftDating.gifts.Count < 1);
        if (moduleGiftDating.gifts.Count > 0)
        {
            RefreshBtnState(moduleGiftDating.curGift != null);
            if (moduleGiftDating.curGift != null)
            {
                var gift = moduleGiftDating.gifts.Find(p => p.itemId == moduleGiftDating.curGift.itemId);
                tipParent.SafeSetActive(gift != null);

                int exp = moduleGiftDating.GetCurrentExp(moduleNPCDating.curDatingNpcMsg.npcId, moduleGiftDating.curGift.itemTypeId);
                Util.SetText(addExp, gift != null && exp != 0 ? "+" + exp : "");

                var prop = ConfigManager.Get<PropItemInfo>(moduleGiftDating.curGift.itemTypeId);
                if (prop != null) Util.SetText(tip_text, (int)TextForMatType.GiftUIText, 7, prop.itemName, exp);
            }
            else
            {
                tipParent.SafeSetActive(false);
                Util.SetText(addExp, "");
            }

            RefreshSliderAndLv(moduleNPCDating.curDatingNpcMsg);
        }
        else
        {
            tipParent.SafeSetActive(false);
            giveGiftBtn.SafeSetActive(false);
        }
    }

    private void RefreshBtnState(bool isTrue)
    {
        giveGiftBtn.interactable = isTrue;
        var btnImage = giveGiftBtn.GetComponent<Image>();
        if (btnImage) btnImage.saturation = isTrue ? 1 : 0;
        var text = giveGiftBtn.GetComponentInChildren<Text>();
        if (text) text.saturation = isTrue ? 1 : 0;
        var inImage = giveGiftBtn.GetComponentInChildren<Image>();
        if (inImage) inImage.saturation = isTrue ? 1 : 0;
        giveGiftBtn.SafeSetActive(false);
        giveGiftBtn.SafeSetActive(true);
    }

    /// <summary>
    /// 1 是解锁 2是换装
    /// </summary>
    /// <param name="showWhatBtn"></param>
    private void RefreshNpcPanel(int showWhatBtn = 0)
    {
        if (moduleNPCDating.curDatingNpcMsg == null) return;
        tipParent.SafeSetActive(moduleShop.curClickItem != null);
        Util.SetText(addExp, "");
        if (moduleShop.curClickItem != null)
        {
            var prop = ConfigManager.Get<PropItemInfo>(moduleShop.curClickItem.itemTypeId);
            if (prop != null) Util.SetText(tip_text, prop.desc);
            if (moduleNPCDating.curDatingNpcMsg._mode != moduleShop.curClickItem.itemTypeId)
            {
                unlockBtn.interactable = true;
                equipBtn.interactable = true;
            }
        }

        sliderParent.SafeSetActive(true);
        RefreshSliderAndLv(moduleNPCDating.curDatingNpcMsg);

        unlockBtn.SafeSetActive(showWhatBtn == 1);
        equipBtn.SafeSetActive(showWhatBtn == 2);
    }

    private void RefreshSliderAndLv(Module_Npc.NpcMessage npcInfo)
    {
        //已满级
        if (npcInfo.fetterLv >= npcInfo.maxFetterLv)
        {
            Util.SetText(lv, npcInfo.fetterLv.ToString());
            topImage.fillAmount = 1;
            Util.SetText(exp_text, (int)TextForMatType.RuneUIText, 16);
            return;
        }

        //未满级
        topImage.fillAmount = npcInfo.fetterProgress;

        Util.SetText(exp_text, Util.Format("{0}/{1}", npcInfo.nowFetterValue, npcInfo.toFetterValue));
        Util.SetText(lv, npcInfo.fetterLv.ToString());
    }

    void _ME(ModuleEvent<Module_GiftDating> e)
    {
        if (!actived) return;
        switch (e.moduleEvent)
        {
            case Module_GiftDating.EventGiftUpdate:
                giftData.SetItems(moduleGiftDating.gifts);
                RefreshGiftPanel();
                break;
            case Module_GiftDating.EventGiveGiftSuccess:break;
            default:
                break;
        }
    }

    void _ME(ModuleEvent<Module_Npc> e)
    {
        if (!actived) return;
        switch (e.moduleEvent)
        {
            case Module_Npc.NpcAddExpSuccessEvent: OnCloseGift(); break;
            case Module_Npc.NpcLvUpEvent:
                if (moduleNPCDating.curDatingNpcMsg != null) RefreshSliderAndLv(moduleNPCDating.curDatingNpcMsg);
                break;
            default:
                break;
        }
    }

    private void RefreshNpcShop()
    {
        var curShop = moduleShop.curShopMsg;
        if (toggles.Count > 1)
        {
            if (moduleNPCDating.curDatingNpcMsg == null && moduleShop.npcShop == null) toggles[1].SafeSetActive(false);
            //else //Npc时装先不做
            //    toggles[1].SafeSetActive(moduleShop.npcShop.ContainsKey(moduleNPCDating.curDatingNpcMsg.npcId) && moduleShop.npcShop[moduleNPCDating.curDatingNpcMsg.npcId].Count > 0);
        }
        if (curShop == null) return;

        Util.SetText(check_shoptext, curShop.name);
        Util.SetText(check_xzshoptext, curShop.name);
        AtlasHelper.SetIcons(check_shopimage, curShop.icon);
    }

    void _ME(ModuleEvent<Module_Shop> e)
    {
        if (!actived || moduleShop.curShopPos != ShopPos.Npc) return;
        switch (e.moduleEvent)
        {
            case Module_Shop.EventShopData:
                var list = e.param1 as List<ShopMessage>;
                if (list == null || list.Count < 1) break;
                moduleShop.SetCurrentShop(list[0]);
                break;
            case Module_Shop.EventTargetShopData:
                RefreshNpcShop();
                break;
            case Module_Shop.EventPaySuccess:
                unlockTip.SafeSetActive(false);
                Window_ItemTip.Show(ConfigText.GetDefalutString(TextForMatType.TravalShopUIText, 13), moduleShop.curClickItem);
                suiteData.UpdateItems();
                unlockBtn.interactable = true;
                unlockBtn.SafeSetActive(false);
                break;
            case Module_Shop.EventPromoChanged:
                var msg = e.param1 as ShopMessage;
                if (msg != null && msg.pos == ShopPos.Maze)
                {
                    DefaultState();
                    int page = suiteView.currentPage;
                    suiteData.UpdateItems();
                    suiteView.ScrollToPage(page);
                }
                break;
            case Module_Shop.EventNpcChangeEquip:
                var result = (sbyte)e.param1;
                if (result != 0) equipBtn.interactable = true;
                else
                {
                    if (moduleNPCDating.curDatingNpcMsg != null) ChangeNpcMode(moduleNPCDating.curDatingNpcMsg.mode);
                    suiteData.UpdateItems();
                }
                break;
            default:
                break;
        }
    }

    private void ChangeNpcMode(string mesh)
    {
        /*if (moduleNPCDating.curDatingNpcMsg == null || moduleNPCDating.curDatingNpcMsg.curNpcCreature == null || string.IsNullOrEmpty(mesh)) return;

        CharacterEquip.ChangeNpcFashion(moduleNPCDating.curDatingNpcMsg.curNpcCreature, mesh);

        curMesh = mesh;*/
    }

    private void RestoreModes()
    {
        if (moduleNPCDating.curDatingNpcMsg == null || string.IsNullOrEmpty(moduleNPCDating.curDatingNpcMsg.mode)) return;
        if (moduleNPCDating.curDatingNpcMsg.mode == curMesh) return;

        CharacterEquip.ChangeNpcFashion(moduleNPCDating.curDatingNpcMsg.curNpcCreature, moduleNPCDating.curDatingNpcMsg.mode);
    }


}
