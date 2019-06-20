// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-10      16:48
//  * LastModify：2018-10-10      19:42
//  ***************************************************************************************************/
#region

using System;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class FurnaceWindow_SoulSuccess : SubWindowBase
{
    private Button    applyButton;
    private Button    discardButton;
    private Text      discardCost;
    private Image     discardCostIcon;
    private Transform itemRoot;
    private SoulEntry leftEntry;
    private SoulEntry rightEntry;
    private Image     leftPortrait;
    private Image     rightPortrait;


    protected override void InitComponent()
    {
        base.InitComponent();
        itemRoot        = WindowCache.GetComponent<Transform>   ("result_Panel/item");
        discardButton   = WindowCache.GetComponent<Button>      ("result_Panel/revert_Btn");
        applyButton     = WindowCache.GetComponent<Button>      ("result_Panel/save_Btn");
        discardCost     = WindowCache.GetComponent<Text>        ("result_Panel/revert_Btn/consume_Txt");
        discardCostIcon = WindowCache.GetComponent<Image>       ("result_Panel/revert_Btn/icon");
        leftPortrait    = WindowCache.GetComponent<Image>       ("result_Panel/left/sprite");
        rightPortrait   = WindowCache.GetComponent<Image>       ("result_Panel/right/sprite");
        leftEntry       = new SoulEntry(WindowCache.GetComponent<Transform>("result_Panel/before"));
        rightEntry      = new SoulEntry(WindowCache.GetComponent<Transform>("result_Panel/after"));
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;

        var item = moduleFurnace.currentSoulItem;
        var soulAttr = item.growAttr.soulAttr?.activeAttr;

        var prevSoulAttr = item.growAttr.soulAttr?.attr;
        leftEntry.Init(prevSoulAttr);
        rightEntry.Init(soulAttr);

        var prevSoulInfo = ConfigManager.Get<SoulInfo>(prevSoulAttr?.soulId ?? 0);
        var soulInfo = ConfigManager.Get<SoulInfo>(soulAttr?.soulId ?? 0);
        if(null != prevSoulInfo)
            UIDynamicImage.LoadImage(leftPortrait?.transform, prevSoulInfo.portrait);
        if(null != soulInfo)
            UIDynamicImage.LoadImage(rightPortrait?.transform, soulInfo.portrait);


        Util.SetItemInfo(itemRoot, item.GetPropItem());

        discardCost  ?.SafeSetActive(null != prevSoulAttr);
        discardButton?.SafeSetActive(null != prevSoulAttr);
        if (null != prevSoulAttr)
        {
            discardButton?.onClick.AddListener(OnDiscard);
            RefreshDiscardCost(prevSoulAttr);
        }
        applyButton  ?.onClick.AddListener(OnApply);

        moduleGlobal.ShowGlobalLayerDefault(-1);
        return true;
    }

    private void RefreshDiscardCost(PSoulAttr prevSoulAttr)
    {
        var soulInfo = ConfigManager.Get<SoulInfo>(prevSoulAttr.soulId);
        if (null == soulInfo?.discardCosts)
            return;
        for (var i = 0; i < soulInfo.discardCosts.Length; i++)
        {
            var prop = ConfigManager.Get<PropItemInfo>(soulInfo.discardCosts[i].itemId);
            if(prop)
                AtlasHelper.SetIcons(discardCostIcon, prop.icon);

            if (soulInfo.discardCosts[i].itemId == 2)
            {
                Util.SetText(discardCost, soulInfo.discardCosts[i].count.ToString());
                discardCost.color = ColorGroup.GetColor(ColorManagerType.IsMoneyEnough,
                    soulInfo.discardCosts[i].count <= modulePlayer.gemCount);
                return;
            }
            else if (soulInfo.discardCosts[i].itemId == 1)
            {
                Util.SetText(discardCost, soulInfo.discardCosts[i].count.ToString());
                discardCost.color = ColorGroup.GetColor(ColorManagerType.IsMoneyEnough,
                    soulInfo.discardCosts[i].count <= modulePlayer.coinCount);
                return;
            }
            else
            {
                Util.SetText(discardCost, soulInfo.discardCosts[i].count.ToString());
                discardCost.color = ColorGroup.GetColor(ColorManagerType.IsMatrialEnough,
                    soulInfo.discardCosts[i].count <= moduleEquip.GetPropCount(soulInfo.discardCosts[i].count));
            }
        }
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;

        discardButton?.onClick.RemoveAllListeners();
        applyButton?.onClick.RemoveAllListeners();
        moduleGlobal.ShowGlobalLayerDefault();
        return true;
    }

    private void OnApply()
    {
        moduleFurnace.RequestApplySoul(true);
    }

    private void OnDiscard()
    {
        moduleFurnace.RequestApplySoul(false);
    }


    private void _ME(ModuleEvent<Module_Furnace> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Furnace.ResponseComfirmSoul:
                ResponseComfirmSoul(e.msg as ScEquipSoulComfirm);
                break;
        }
    }

    private void ResponseComfirmSoul(ScEquipSoulComfirm msg)
    {
        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9751, msg.result);
            return;
        }
        UnInitialize();
    }

}


public class SoulEntry
{
    private readonly Transform ownNode;
    private readonly Transform noneNode;
    private readonly Transform attributeGroup;
    private readonly Transform attributeTemplate;
    private readonly Text awakePercent;
    private readonly Text grade;
    private readonly Text name;
    private readonly Text desc;
    private readonly Image icon;

    public SoulEntry(Transform rRoot)
    {
        ownNode             = rRoot.GetComponent<Transform>("Own");
        noneNode            = rRoot.GetComponent<Transform>("None");
        name                = rRoot.GetComponent<Text>     ("Own/namePart");
        desc                = rRoot.GetComponent<Text>     ("Own/desPart");
        grade               = rRoot.GetComponent<Text>     ("Own/levelPart/level");
        awakePercent        = rRoot.GetComponent<Text>     ("Own/awakePart/awake");
        attributeGroup      = rRoot.GetComponent<Transform>("Own/attrPart/attr_group");
        attributeTemplate   = rRoot.GetComponent<Transform>("Own/attrPart/attr_group/attr_item");
        icon                = rRoot.GetComponent<Image>    ("Own/avatarPart/avatar");

        attributeTemplate.SafeSetActive(false);
    }

    public void Init(PSoulAttr rSoulAttr)
    {
        var soulInfo = ConfigManager.Get<SoulInfo>(rSoulAttr?.soulId ?? 0);
        ownNode.SafeSetActive(soulInfo != null);
        noneNode.SafeSetActive(soulInfo == null);
        if (soulInfo == null)
            return;

        var t = GetSoulInfo(rSoulAttr);
        Util.SetText(name, soulInfo.nameId);
        Util.SetText(awakePercent, t.Item1);
        Util.SetText(grade, t.Item2);
        Util.SetText(desc, soulInfo.descId);
        AtlasHelper.SetShared(icon, soulInfo.icon);

        Util.ClearChildren(attributeGroup);
        for (var i = 0; i < t.Item3.Length; i++)
        {
            var a = attributeGroup.AddNewChild(attributeTemplate);
            a.SafeSetActive(true);
            Util.SetText(a?.gameObject, t.Item3[i]);
        }
    }

    public static Tuple<string, string, string[]> GetSoulInfo(PSoulAttr rSoulAttr)
    {
        if (null == rSoulAttr) return Tuple.Create(string.Empty, string.Empty, new string[0]);

        var percent = rSoulAttr.myriadRate * 0.0001f;
        var value = Util.RemapValue(percent, 0, 1,
            GeneralConfigInfo.defaultConfig.soulMapRange[0], GeneralConfigInfo.defaultConfig.soulMapRange[1]);
        var item1 = $"{value:P2}";

        var ct = ConfigManager.Get<ConfigText>(8000);
        var index = (int)(percent * ct.text.Length);
        index = Mathf.Clamp(index, 0, ct.text.Length - 1);
        var item2 = ct[index];

        string[] arr = new string[rSoulAttr.soulAttrs.Length];
        for (var i = 0; i < arr.Length; i++)
        {
            arr[i] = $"{ rSoulAttr.soulAttrs[i].TypeString()}+" +
                     $"{ rSoulAttr.soulAttrs[i].ValueString()}";
        }


        return Tuple.Create(item1, item2, arr);
    }
}