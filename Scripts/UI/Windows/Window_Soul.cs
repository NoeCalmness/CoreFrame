// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-10      11:03
//  * LastModify：2018-10-10      20:19
//  ***************************************************************************************************/
#region

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class Window_Soul : Window
{
    public Transform attributeRoot;
    public Transform attributeTemp;
    public Text      costCoin;
    public Text      costItem;
    public Transform equipState;
    public Button    excuteButton;
    public Transform rightIconRoot;
    public Text      itemName;
    public Text      costItemName;
    public Image     costItemIcon;
    public Image     portrait;
    public readonly List<Image> typeIcons = new List<Image>();

    private FurnaceWindow_SoulSuccess successWindow;
    private FurnaceWindow_SoulSummon summonWindow;
    private FurnaceWindow_SoulTip tipWindow;
    private SoulEntry soulEntry;

    protected override void OnOpen()
    {
        base.OnOpen();
        InitCompoent();
        MultiLangrage();

        excuteButton?.onClick.AddListener(OnExcute);
    }

    private void MultiLangrage()
    {
        var ct = ConfigManager.Get<ConfigText>((int)TextForMatType.SoulUI);
        Util.SetText(GetComponent<Text>("summon_Btn/summon_Txt"), ct[0]);
        Util.SetText(GetComponent<Text>("left/bottom/Own/namePart"),  ct[1]);
        Util.SetText(GetComponent<Text>("left/bottom/Own/awakePart"), ct[2]);
        Util.SetText(GetComponent<Text>("left/bottom/Own/levelPart"), ct[3]);
        Util.SetText(GetComponent<Text>("left/bottom/Own/attrPart"),  ct[4]);
        Util.SetText(GetComponent<Text>("left/None/desc"),            ct[12]);
        Util.SetText(GetComponent<Text>("left/base/equiped_img/equiped"),   ct[5]);
        Util.SetText(GetComponent<Text>("result_Panel/save_Btn/save_Txt"),      ct[6]);
        Util.SetText(GetComponent<Text>("result_Panel/revert_Btn/revert_Txt"),  ct[7]);
        Util.SetText(GetComponent<Text>("result_Panel/before/Own/namePart"),    ct[1]);
        Util.SetText(GetComponent<Text>("result_Panel/before/Own/awakePart"),   ct[2]);
        Util.SetText(GetComponent<Text>("result_Panel/before/Own/attr"),        ct[4]);
        Util.SetText(GetComponent<Text>("result_Panel/before/Own/levelPart"),   ct[3]);
        Util.SetText(GetComponent<Text>("result_Panel/after/Own/namePart"),     ct[1]);
        Util.SetText(GetComponent<Text>("result_Panel/after/Own/awakePart"),    ct[2]);
        Util.SetText(GetComponent<Text>("result_Panel/after/Own/attr"),         ct[4]);
        Util.SetText(GetComponent<Text>("result_Panel/after/Own/levelPart"),    ct[3]);
        Util.SetText(GetComponent<Text>("result_Panel/before/state/state_Txt"), ct[8]);
        Util.SetText(GetComponent<Text>("result_Panel/after/state/state_Txt"),  ct[9]);
        Util.SetText(GetComponent<Text>("xiaohao/comsume/consume_Txt_02"),      ct[11]);
        Util.SetText(GetComponent<Text>("result_Panel/before/None/desc"),       ct[13]);
        Util.SetText(GetComponent<Text>("result_Panel/after/None/desc"),        ct[13]);
        Util.SetText(GetComponent<Text>("summon_Panel/Text"),                   ct[14]);
        Util.SetText(GetComponent<Text>("jipin/title"),                         ct[15]);
        Util.SetText(GetComponent<Text>("jipin/item/Own/namePart"), ct[1]);
        Util.SetText(GetComponent<Text>("jipin/item/Own/awakePart"), ct[2]);
        Util.SetText(GetComponent<Text>("jipin/item/Own/levelPart"), ct[3]);
        Util.SetText(GetComponent<Text>("jipin/item/Own/attrPart"), ct[4]);
        Util.SetText(GetComponent<Text>("result_Panel/title"), ct[16]);
    }

    private void InitCompoent()
    {
        itemName        = GetComponent<Text>        ("left/base/name");
        costItemName    = GetComponent<Text>        ("xiaohao/exp/total_tip");
        costItemIcon    = GetComponent<Image>       ("xiaohao/exp/exp_img");
        equipState      = GetComponent<Transform>   ("left/base/equiped_img");
        rightIconRoot   = GetComponent<Transform>   ("item");
        excuteButton    = GetComponent<Button>      ("summon_Btn");
        costCoin        = GetComponent<Text>        ("xiaohao/comsume/consume_Txt");
        costItem        = GetComponent<Text>        ("xiaohao/exp/total_swallow");
        portrait        = GetComponent<Image>       ("sprite");
//        attributeTemp   = GetComponent<Transform>   ("left/top/layoutGroup/attribute");
//        attributeRoot   = attributeTemp?.parent;
//        attributeTemp.SafeSetActive(false);

        var typeIconRoot = GetComponent<Transform>("left/base/name/icon");
        var iconNames = new string[]
        {
            "sword", "katana", "axe", "fist", "pistol", "suit" 
        };
        typeIcons.Clear();
        for (var i = 0; i < iconNames.Length; i++)
            typeIcons.Add(typeIconRoot.GetComponent<Image>(iconNames[i]));

        successWindow = SubWindowBase             .CreateSubWindow<FurnaceWindow_SoulSuccess>(this, GetComponent<Transform>("result_Panel")?.gameObject);
        summonWindow  = SubWindowBase<Window_Soul>.CreateSubWindow<FurnaceWindow_SoulSummon> (this, GetComponent<Transform>("summon_Panel")?.gameObject);
        tipWindow     = SubWindowBase<Window_Soul>.CreateSubWindow<FurnaceWindow_SoulTip>    (this, GetComponent<Transform>("jipin")?.gameObject);
        soulEntry     = new SoulEntry(GetComponent<Transform>("left"));
    }

    protected override void OnClose()
    {
        base.OnClose();
        successWindow?.Destroy();
        summonWindow ?.Destroy();
        tipWindow    ?.Destroy();
    }

    public void ShowTipWindow()
    {
        var soulAttr = moduleFurnace.currentSoulItem?.growAttr?.soulAttr;
        tipWindow.Initialize(soulAttr?.activeAttr ?? soulAttr?.attr);
    }

    public void ShowSuccessWindow()
    {
        var soulAttr = moduleFurnace.currentSoulItem?.growAttr?.soulAttr;
        if (soulAttr?.activeAttr != null)
        {
            successWindow.Initialize();
        }
        else
            moduleGlobal.ShowGlobalLayerDefault(1);
    }

    private void OnExcute()
    {
        moduleFurnace.RequestSoul();
    }

    protected override void OnReturn()
    {
        if (successWindow.OnReturn())
            return;

        var key = new NoticeDefaultKey(NoticeType.Soul);
        moduleNotice.SetNoticeState(key, moduleEquip.CheckSoulSprite(moduleFurnace.currentSoulItem));
        moduleNotice.SetNoticeReadState(key);
        base.OnReturn();
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        if (moduleFurnace.currentSoulItem == null)
            return;
        moduleGlobal.ShowGlobalLayerDefault();
        if (moduleFurnace.currentSoulItem.growAttr.soulAttr?.activeAttr != null &&
            moduleFurnace.currentSoulItem.growAttr.soulAttr?.activeAttr.soulId > 0)
        {
            successWindow.Initialize(moduleFurnace.currentSoulItem,
                moduleFurnace.currentSoulItem.growAttr.soulAttr.activeAttr);
        }

        RefreshItemInfo();
    }
    
    private void RefreshItemInfo()
    {
        var prop = moduleFurnace.currentSoulItem?.GetPropItem();

        if (prop == null)
            return;

        Util.SetText(itemName, prop.itemName);
        Util.SetEquipTypeIcon(typeIcons, prop.itemType, prop.subType);

        var equipAttribute = moduleFurnace.currentSoulItem.growAttr?.equipAttr;
        equipState.SafeSetActive(moduleEquip.IsDressOn(moduleFurnace.currentSoulItem));
        if (null != equipAttribute)
        {
            Util.SetItemInfo(rightIconRoot, prop, equipAttribute.strength, 0, true, equipAttribute.star);

//            attributeRoot.RemoveChildren();
//            var arr = equipAttribute.fixedAttrs;
//            for (var i = 0; i < arr.Length; i++)
//            {
//                var t = attributeRoot.AddNewChild(attributeTemp);
//                t.SafeSetActive(true);
//                t.GetComponentDefault<AttributeGrowDisplay>()?.Init(arr[i]);
//            }
        }
        else
        {
            Util.SetItemInfo(rightIconRoot, prop);
        }

        RefreshCost();

        RefreshItemSoulInfo();
    }

    private void RefreshCost()
    {
        var prop = moduleFurnace.currentSoulItem?.GetPropItem();
        var soulCost = ConfigManager.Get<SoulCost>(prop?.quality ?? 0);
        if (soulCost != null)
        {
            for (var i = 0; i < soulCost.costs.Length; i++)
            {
                if (soulCost.costs[i].itemId == 1)
                {
                    Util.SetText(costCoin, soulCost.costs[i].count.ToString());
                    costCoin.color = ColorGroup.GetColor(ColorManagerType.IsMoneyEnough,
                        soulCost.costs[i].count <= modulePlayer.coinCount);
                }
                else
                {
                    var own = moduleEquip.GetPropCount(soulCost.costs[i].itemId);
                    Util.SetText(costItem, Util.Format(Util.GetString((int)TextForMatType.SoulUI, 17),soulCost.costs[i].count.ToString(), own));
                    costItem.color = ColorGroup.GetColor(ColorManagerType.IsMatrialEnough, soulCost.costs[i].count <= own);
                    var costProp = ConfigManager.Get<PropItemInfo>(soulCost.costs[i].itemId);
                    if (null != costProp)
                    {
                        Util.SetText(costItemName, costProp.itemName);
                        AtlasHelper.SetIcons(costItemIcon, costProp.icon);
                        costItemIcon.GetComponentDefault<Button>()?
                            .onClick.AddListener(() => moduleGlobal.UpdateGlobalTip((ushort)costProp.ID));
                    }
                }
            }
        }
        else
        {
            Util.SetText(costCoin, "0");
            Util.SetText(costItem, "0");
        }
    }

    private void RefreshItemSoulInfo()
    {
        var soulInfo = ConfigManager.Get<SoulInfo>(moduleFurnace.currentSoulItem?.growAttr.soulAttr?.attr?.soulId ?? 0);
        if(null != soulInfo)
            UIDynamicImage.LoadImage(portrait?.transform, soulInfo.portrait);
        portrait.SafeSetActive(null != soulInfo);

        soulEntry.Init(moduleFurnace.currentSoulItem?.growAttr.soulAttr?.attr);
    }

    private void _ME(ModuleEvent<Module_Equip> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Equip.EventItemDataChange:
                RefreshItemSoulInfo();
                break;
            case Module_Equip.EventUpdateBagProp:
                RefreshCost();
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Furnace> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Furnace.ResponseSoul:
                ResponseSoul(e.msg as ScEquipSoul);
                break;
        }
    }
    private void _ME(ModuleEvent<Module_Player> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Player.EventCurrencyChanged:
                RefreshCost();
                break;
        }
    }

    private void ResponseSoul(ScEquipSoul msg)
    {
        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9752, msg.result);
            return;
        }
        moduleFurnace.currentSoulItem = moduleEquip.GetProp(moduleFurnace.currentSoulItem.itemId);
        summonWindow.Initialize(msg.soulAttr);
    }
}
