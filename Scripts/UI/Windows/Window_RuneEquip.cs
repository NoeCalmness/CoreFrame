/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-02-22
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_RuneEquip : Window
{
    Image runeIcon;
    SubWindow_Equip chooseItem;
    SubWindow_OperateRune opRune;

    protected override void OnOpen()
    {
        runeIcon = GetComponent<Image>("lingpo_img");
        chooseItem = SubWindowBase.CreateSubWindow<SubWindow_Equip, Window_RuneEquip>(this, GetComponent<Transform>("equip_panel")?.gameObject);
        opRune = SubWindowBase.CreateSubWindow<SubWindow_OperateRune, Window_RuneEquip>(this, GetComponent<Transform>("inten_evo_panel")?.gameObject);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();
        RefreshRune(moduleRune.curOpItem);
        SwitchSubPanel(moduleRune.runeOpType, moduleRune.curOpItem);
    }

    protected override void OnClose()
    {
        chooseItem?.Destroy();
        opRune?.Destroy();
    }

    void RefreshRune(PItem item)
    {
        runeIcon.SafeSetActive(item != null);
        if (item == null) return;
        var prop = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
        if (prop == null || prop.mesh == null || prop.mesh.Length < 2) return;
        UIDynamicImage.LoadImage(runeIcon?.transform, prop.mesh[1]);
    }

    void SwitchSubPanel(RuneInWhichPanel panel, PItem item)
    {
        chooseItem.UnInitialize();
        opRune.UnInitialize();
        if (panel == RuneInWhichPanel.Equip) chooseItem.Initialize(item);
        else opRune.Initialize(panel, item);
    }
}

public class SubWindow_Equip : SubWindowBase<Window_RuneEquip>
{
    Image runeIcon;
    Transform[] attrPanel;
    Transform[] otherAttrPanel;
    Text twoSuiteDesc;
    Text fourSuiteDesc;
    Text subType;
    Text runeName;
    Text runeLv;
    Image[] stars;
    ScrollView view;
    Button equipBtn;
    Text btnText;

    PItem curItem;
    PItem lastItem;
    DataSource<PItem> dataSource = null;
    List<PItem> currentList = new List<PItem>();

    protected override void InitComponent()
    {
        runeIcon = WindowCache.GetComponent<Image>("lingpo_img");
        var attr_1 = WindowCache.GetComponent<Transform>("equip_panel/left/panel1/count1");
        var attr_2 = WindowCache.GetComponent<Transform>("equip_panel/left/panel1/count2");
        var attr_3 = WindowCache.GetComponent<Transform>("equip_panel/left/panel1/count3");
        var attr_4 = WindowCache.GetComponent<Transform>("equip_panel/left/panel1/count4");
        var attr_5 = WindowCache.GetComponent<Transform>("equip_panel/left/panel1/count5");
        var attr_6 = WindowCache.GetComponent<Transform>("equip_panel/left/panel1/count6");
        attrPanel = new Transform[] { attr_1, attr_2, attr_3 };
        otherAttrPanel = new Transform[] { attr_4, attr_5, attr_6 };
        twoSuiteDesc = WindowCache.GetComponent<Text>("equip_panel/left/panel2/two_txt/Text");
        fourSuiteDesc = WindowCache.GetComponent<Text>("equip_panel/left/panel2/four_txt/Text");
        subType = WindowCache.GetComponent<Text>("equip_panel/right_panel/title/rome_txt");
        runeName = WindowCache.GetComponent<Text>("equip_panel/right_panel/title/lingpo_name");
        runeLv = WindowCache.GetComponent<Text>("equip_panel/right_panel/title/level");
        stars = WindowCache.GetComponent<Transform>("equip_panel/right_panel/title/star_parent")?.GetComponentsInChildren<Image>(true);
        equipBtn = WindowCache.GetComponent<Button>("equip_panel/right_panel/equip_Button");
        btnText = WindowCache.GetComponent<Text>("equip_panel/right_panel/equip_Button/Text");
        view = WindowCache.GetComponent<ScrollView>("equip_panel/right_panel/middle_panel/scrollView");
        dataSource = new DataSource<PItem>(null, view, OnSetRuneData, OnClickRune);
    }

    private void OnClickRune(RectTransform node, PItem data)
    {
        if (curItem != null && data.itemId == curItem.itemId) return;
        lastItem = curItem;
        curItem = data;

        var index = currentList.FindIndex(p => p.itemId == data.itemId);
        dataSource.SetItem(data, index);

        if (lastItem != null)
        {
            index = currentList.FindIndex(p => p.itemId == lastItem.itemId);
            dataSource.SetItem(lastItem, index);
        }

        var prop = ConfigManager.Get<PropItemInfo>(data.itemTypeId);
        if (prop == null || prop.mesh == null || prop.mesh.Length < 2) return;
        UIDynamicImage.LoadImage(runeIcon?.transform, prop.mesh[1]);
    }

    private void OnSetRuneData(RectTransform node, PItem data)
    {
        if (data == null || data.growAttr == null || data.growAttr.runeAttr == null) return;
        var prop = ConfigManager.Get<PropItemInfo>(data.itemTypeId);
        if (prop == null) return;
        Util.SetItemInfo(node, prop, data.growAttr.runeAttr.level, 0, true, data.growAttr.runeAttr.star);

        var lockImg = node.Find("lock");
        lockImg.SafeSetActive(data.isLock == 1);
        var select = node.Find("selectBox");
        select.SafeSetActive(data.itemId == curItem.itemId);
        var max = node.Find("levelmax");
        max.SafeSetActive(moduleRune.IsMaxLv(data.growAttr.runeAttr.level));
        var equip = node.Find("get");
        equip.SafeSetActive(moduleRune.IsEquip(data));
        var equipText = node.Find("get/Text")?.GetComponent<Text>();
        if (equipText) equipText.color = moduleRune.GetCurQualityColor(GeneralConfigInfo.defaultConfig.qualitys, data.growAttr.runeAttr.star);
        Util.SetText(equipText, (int)TextForMatType.RuneUIText, 48);
        var mark = node.Find("mark");
        mark.SafeSetActive(false);
        var mask = node.Find("mask");
        mask.SafeSetActive(false);

        if (data.itemId == curItem.itemId) RefreshClickItem(data);
        var newProp = node.Find("new");
        bool isNew = moduleCangku.NewsProp(data.itemId);
        newProp.SafeSetActive(isNew);
    }

    void RefreshClickItem(PItem item)
    {
        if (item == null) return;
        RefreshBaseRune(item);
        RefreshSuitePanel(item);
        RefreshAttrPanel(item);
        RefreshBtnMessage(item);
        moduleCangku.RemveNewItem(item.itemId);
    }

    public override void MultiLanguage()
    {
        var index = (int)TextForMatType.RuneUIText;
        Util.SetText(WindowCache.GetComponent<Text>("equip_panel/left/panel2/two_txt"), index, 11);
        Util.SetText(WindowCache.GetComponent<Text>("equip_panel/left/panel2/four_txt"), index, 12);
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p)) return false;

        if (p.Length > 0) curItem = p[0] as PItem;

        DefaultState();
        if (moduleRune.runeOpType != RuneInWhichPanel.Equip) return true;
        RefreshDataList(curItem);

        return true;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide)) return false;

        curItem = null;
        lastItem = null;
        return true;
    }

    void RefreshDataList(PItem item, bool isScroll = true)
    {
        if (item == null) return;
        var prop = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
        if (!prop) return;

        moduleRune.UpdateRead(1, true, prop.subType);

        if (moduleRune.allSubTypeDic.ContainsKey((RuneType)prop.subType)) currentList = moduleRune.allSubTypeDic[(RuneType)prop.subType];
        dataSource?.SetItems(currentList);
        if (!isScroll) return;
        if (item != null)
        {
            var index = currentList.FindIndex(o => o.itemId == item.itemId);
            view.ScrollToData(index);
        }
    }

    void RefreshAttrDefaultPanel()
    {
        if (attrPanel != null)
        {
            for (int i = 0; i < attrPanel.Length; i++)
            {
                Util.SetText(attrPanel[i].Find("name")?.GetComponent<Text>(), "");
                Util.SetText(attrPanel[i].Find("Text1")?.GetComponent<Text>(), "");
            }
        }

        if (otherAttrPanel != null)
        {
            for (int i = 0; i < otherAttrPanel.Length; i++)
            {
                Util.SetText(otherAttrPanel[i].Find("name")?.GetComponent<Text>(), "");
                Util.SetText(otherAttrPanel[i].Find("Text1")?.GetComponent<Text>(), "");
            }
        }
    }

    void RefreshAttrPanel(PItem item)
    {
        RefreshAttrDefaultPanel();
        if (item == null || item.growAttr == null || item.growAttr.runeAttr == null || item.growAttr.runeAttr.randAttrs == null) return;
        var attrs = item.growAttr.runeAttr.randAttrs;

        var prop = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
        if (prop == null) return;

        var _equip = moduleRune.currentEquip.Find(p => p.GetPropItem()?.subType == prop.subType);
        var isEquip = moduleRune.IsEquip(item);

        var curTrans = isEquip || _equip == null ? attrPanel : otherAttrPanel;
        if (curTrans != null)
        {
            for (int i = 0; i < curTrans.Length; i++)
            {
                curTrans[i].SafeSetActive(i < attrs.Length);
                if (i >= attrs.Length) continue;

                var attrId = moduleRune.GetCurAttrId(attrs[i].itemAttrId);
                Util.SetText(curTrans[i].Find("name")?.GetComponent<Text>(), Util.GetString((int)TextForMatType.AttributeUIText, attrId));

                string value = "";
                if (GeneralConfigInfo.defaultConfig.percentAttriIds.Contains(attrId)) value = attrs[i].attrVal.ToString("P2");
                else value = attrs[i].attrVal.ToString("F0");

                Util.SetText(curTrans[i].Find("Text1")?.GetComponent<Text>(), value);
            }
        }

        if (!isEquip && _equip != null && attrPanel != null)
        {
            if (_equip.growAttr != null && _equip.growAttr.runeAttr != null && _equip.growAttr.runeAttr.randAttrs != null)
            {
                attrs = _equip.growAttr.runeAttr.randAttrs;

                for (int i = 0; i < attrPanel.Length; i++)
                {
                    attrPanel[i].SafeSetActive(i < attrs.Length);
                    if (i >= attrs.Length) continue;

                    var attrId = moduleRune.GetCurAttrId(attrs[i].itemAttrId);
                    Util.SetText(attrPanel[i].Find("name")?.GetComponent<Text>(), Util.GetString((int)TextForMatType.AttributeUIText, attrId));

                    string value = "";
                    if (GeneralConfigInfo.defaultConfig.percentAttriIds.Contains(attrId)) value = attrs[i].attrVal.ToString("P2");
                    else value = attrs[i].attrVal.ToString("F0");

                    Util.SetText(attrPanel[i].Find("Text1")?.GetComponent<Text>(), value);
                }
            }
        }
    }

    void RefreshSuitePanel(PItem item)
    {
        if (item == null) return;
        var prop = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
        if (prop == null) return;

        Util.SetText(twoSuiteDesc, moduleRune.GetSuiteDesc(prop.suite, 2));
        Util.SetText(fourSuiteDesc, moduleRune.GetSuiteDesc(prop.suite, 4));
    }

    void RefreshBaseRune(PItem item)
    {
        if (item == null) return;
        var prop = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
        if (prop == null) return;

        Util.SetText(subType, moduleRune.GetRomaString(prop));
        Util.SetText(runeName, prop.itemName);
        Util.SetText(runeLv, item.growAttr?.runeAttr?.level > 0 ? $"Lv.{item.growAttr?.runeAttr?.level}" : "");
        if (stars != null)
        {
            for (int i = 0; i < stars.Length; i++)
                stars[i].SafeSetActive(i < item.growAttr?.runeAttr?.star);
        }
    }

    void RefreshBtnMessage(PItem item)
    {
        if (item == null) return;
        var isEquip = moduleRune.IsEquip(item);
        Util.SetText(btnText, isEquip ? Util.GetString((int)TextForMatType.RuneUIText, 17) : Util.GetString((int)TextForMatType.RuneUIText, 18));

        equipBtn.SafeSetActive(true);
        equipBtn.onClick.RemoveAllListeners();
        equipBtn.onClick.AddListener(() => OnChangeRune(item, !isEquip));
    }

    private void OnChangeRune(PItem item, bool isEquip)
    {
        if (item == null) return;
        moduleRune.ChangeDressData(item, isEquip);
        equipBtn.interactable = false;
    }

    void DefaultState()
    {
        if (attrPanel != null)
        {
            for (int i = 0; i < attrPanel.Length; i++)
                attrPanel[i].SafeSetActive(false);
        }
        Util.SetText(twoSuiteDesc, "");
        Util.SetText(fourSuiteDesc, "");
        Util.SetText(subType, "");
        Util.SetText(runeName, "");
        Util.SetText(runeLv, "");
        if (stars != null)
        {
            for (int i = 0; i < stars.Length; i++)
                stars[i].SafeSetActive(false);
        }
        equipBtn.SafeSetActive(false);
    }

    void _ME(ModuleEvent<Module_Rune> e)
    {
        if (WindowCache.actived && e.moduleEvent == Module_Rune.EventChangeEquip)
        {
            var type = (int)e.param1;
            var result = (sbyte)e.param2;
            if (type == 1)//卸下
            {
                if (result == 0) moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 21));
                else moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 22));
            }
            else if (type == 2)//换装
            {
                if (result == 0)
                {
                    //moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 19));
                    Window.SkipBackTo<Window_RuneStart>();
                }
                else moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 20));
            }

            equipBtn.interactable = true;
            RefreshDataList(curItem, false);
        }
    }
}

public class SubWindow_OperateRune : SubWindowBase<Window_RuneEquip>
{
    ScrollView view;
    Transform noItmesTip;
    DataSource<PItem> dataSource;
    Transform tipPanel;
    Button yesBtn;

    PItem curItem;
    RuneInWhichPanel _type;

    #region 强化
    Transform intentify_panel;
    Text subType;
    Text runeLv;
    Image[] stars;
    Text originalLv;
    Text newLv;
    Text expDesc;
    RectTransform preFillamount;
    RectTransform fillamount;
    Transform[] attrPanel;
    Text twoSuiteDesc;
    Text fourSuiteDesc;
    Text costCoin;
    Button upBtn;

    uint swallowedExp;//吞噬的经验(用于计算消耗金币的)
    uint expTotal;//总经验(需要截断溢出经验,用于计算升级的等级)
    PreviewSlider pres;

    #endregion

    #region 进化
    Transform evo_panel;
    PItem beforeItem;
    Text _subType;
    Text _runeLv;
    Image[] _stars;
    Image[] _midStars;
    Transform[] _attrPanel;
    Text _twoSuiteDesc;
    Text _fourSuiteDesc;
    Text _costCoin;
    Button _evolveBtn;
    Text _evolveLv;
    Transform[] _previewItems;
    Image _starSlider;
    Transform _successPanel;
    Transform _leftItem;
    Transform _rightItem;
    Text _leftLevel;
    Text _rightLevel;
    Button successBtn;
    #endregion

    protected override void InitComponent()
    {
        view       = WindowCache.GetComponent<ScrollView>("inten_evo_panel/left_panel/scrollView");
        noItmesTip = WindowCache.GetComponent<Transform>("inten_evo_panel/left_panel/tip");
        dataSource = new DataSource<PItem>(null, view, OnSetItems, OnClickItem);
        tipPanel   = WindowCache.GetComponent<Transform>("inten_evo_panel/tipPanel");
        yesBtn     = WindowCache.GetComponent<Button>("inten_evo_panel/tipPanel/sure");

        #region 强化
        intentify_panel = WindowCache.GetComponent<Transform>("inten_evo_panel/intentify_panel");
        subType         = WindowCache.GetComponent<Text>("inten_evo_panel/intentify_panel/title/rome_txt");
        runeLv          = WindowCache.GetComponent<Text>("inten_evo_panel/intentify_panel/title/level");
        stars           = WindowCache.GetComponent<Transform>("inten_evo_panel/intentify_panel/title/star_parent")?.GetComponentsInChildren<Image>(true);
        originalLv      = WindowCache.GetComponent<Text>("inten_evo_panel/intentify_panel/middle_panel/level/level_1");
        newLv           = WindowCache.GetComponent<Text>("inten_evo_panel/intentify_panel/middle_panel/level/level_2");
        expDesc         = WindowCache.GetComponent<Text>("inten_evo_panel/intentify_panel/middle_panel/slider/Text");
        preFillamount   = WindowCache.GetComponent<RectTransform>("inten_evo_panel/intentify_panel/middle_panel/slider/preFill_img");
        fillamount      = WindowCache.GetComponent<RectTransform>("inten_evo_panel/intentify_panel/middle_panel/slider/fill_img");
        var attr_1      = WindowCache.GetComponent<Transform>("inten_evo_panel/intentify_panel/middle_panel/panel1/count1");
        var att_2       = WindowCache.GetComponent<Transform>("inten_evo_panel/intentify_panel/middle_panel/panel1/count2");
        var att_3       = WindowCache.GetComponent<Transform>("inten_evo_panel/intentify_panel/middle_panel/panel1/count3");
        attrPanel       = new Transform[] { attr_1, att_2, att_3 };
        twoSuiteDesc    = WindowCache.GetComponent<Text>("inten_evo_panel/intentify_panel/middle_panel/panel2/two_txt/Text");
        fourSuiteDesc   = WindowCache.GetComponent<Text>("inten_evo_panel/intentify_panel/middle_panel/panel2/four_txt/Text");
        costCoin        = WindowCache.GetComponent<Text>("inten_evo_panel/intentify_panel/gold/Text (1)");
        upBtn           = WindowCache.GetComponent<Button>("inten_evo_panel/intentify_panel/uplv_Button");
        upBtn?.onClick.RemoveAllListeners();
        upBtn?.onClick.AddListener(OnRuneUpLevel);

        pres = new PreviewSlider(new RuneSlider(fillamount, fillamount.parent.rectTransform(), onSetLevel), new RuneSlider2(preFillamount, fillamount.parent.rectTransform()));
        #endregion

        #region 进化
        evo_panel      = WindowCache.GetComponent<Transform>("inten_evo_panel/evolution_panel");
        _subType       = WindowCache.GetComponent<Text>("inten_evo_panel/evolution_panel/right_panel/title/rome_txt");
        _runeLv        = WindowCache.GetComponent<Text>("inten_evo_panel/evolution_panel/right_panel/title/level");
        _stars         = WindowCache.GetComponent<Transform>("inten_evo_panel/evolution_panel/right_panel/title/star_parent")?.GetComponentsInChildren<Image>(true);
        _midStars      = WindowCache.GetComponent<Transform>("inten_evo_panel/evolution_panel/right_panel/middle_panel/star_level/light_star")?.GetComponentsInChildren<Image>(true);
        var _attr_1    = WindowCache.GetComponent<Transform>("inten_evo_panel/evolution_panel/right_panel/middle_panel/panel1/count1");
        var _attr_2    = WindowCache.GetComponent<Transform>("inten_evo_panel/evolution_panel/right_panel/middle_panel/panel1/count2");
        var _attr_3    = WindowCache.GetComponent<Transform>("inten_evo_panel/evolution_panel/right_panel/middle_panel/panel1/count3");
        _attrPanel     = new Transform[] { _attr_1, _attr_2, _attr_3 };
        _twoSuiteDesc  = WindowCache.GetComponent<Text>("inten_evo_panel/evolution_panel/right_panel/middle_panel/panel3/two_txt/Text");
        _fourSuiteDesc = WindowCache.GetComponent<Text>("inten_evo_panel/evolution_panel/right_panel/middle_panel/panel3/four_txt/Text");
        _costCoin      = WindowCache.GetComponent<Text>("inten_evo_panel/evolution_panel/right_panel/gold/Text (1)");
        _evolveBtn     = WindowCache.GetComponent<Button>("inten_evo_panel/evolution_panel/right_panel/evolution_Button");
        _evolveLv      = WindowCache.GetComponent<Text>("inten_evo_panel/evolution_panel/right_panel/middle_panel/panel2/Text");
        var preItem_1  = WindowCache.GetComponent<Transform>("inten_evo_panel/evolution_panel/right_panel/middle_panel/panel2/material_panel/1");
        var preItem_2  = WindowCache.GetComponent<Transform>("inten_evo_panel/evolution_panel/right_panel/middle_panel/panel2/material_panel/2");
        var preItem_3  = WindowCache.GetComponent<Transform>("inten_evo_panel/evolution_panel/right_panel/middle_panel/panel2/material_panel/3");
        var preItem_4  = WindowCache.GetComponent<Transform>("inten_evo_panel/evolution_panel/right_panel/middle_panel/panel2/material_panel/4");
        _previewItems  = new Transform[] { preItem_1, preItem_2, preItem_3, preItem_4 };
        _starSlider    = WindowCache.GetComponent<Image>("inten_evo_panel/evolution_panel/right_panel/middle_panel/star_level/slider/fill_bg");
        _successPanel  = WindowCache.GetComponent<Transform>("inten_evo_panel/evolution_panel/suceessEvolvePanel");
        _leftItem      = WindowCache.GetComponent<Transform>("inten_evo_panel/evolution_panel/suceessEvolvePanel/0");
        _rightItem     = WindowCache.GetComponent<Transform>("inten_evo_panel/evolution_panel/suceessEvolvePanel/1");
        _leftLevel     = WindowCache.GetComponent<Text>("inten_evo_panel/evolution_panel/suceessEvolvePanel/dengji/Text");
        _rightLevel    = WindowCache.GetComponent<Text>("inten_evo_panel/evolution_panel/suceessEvolvePanel/dengji/Text1");
        successBtn     = _successPanel?.GetComponent<Button>();
        _evolveBtn?.onClick.RemoveAllListeners();
        _evolveBtn?.onClick.AddListener(_OnEvoTipTrue);

        #endregion
    }

    public override void MultiLanguage()
    {
        var index = (int)TextForMatType.RuneUIText;
        Util.SetText(WindowCache.GetComponent<Text>("inten_evo_panel/intentify_panel/title/Text"), index, 26);
        Util.SetText(WindowCache.GetComponent<Text>("inten_evo_panel/intentify_panel/gold/Text"), index, 27);
        Util.SetText(WindowCache.GetComponent<Text>("inten_evo_panel/intentify_panel/uplv_Button/Text"), index, 7);
        Util.SetText(WindowCache.GetComponent<Text>("inten_evo_panel/intentify_panel/middle_panel/panel2/two_txt"), index, 11);
        Util.SetText(WindowCache.GetComponent<Text>("inten_evo_panel/intentify_panel/middle_panel/panel2/four_txt"), index, 12);

        Util.SetText(WindowCache.GetComponent<Text>("inten_evo_panel/evolution_panel/right_panel/title/Text"), index, 28);
        Util.SetText(WindowCache.GetComponent<Text>("inten_evo_panel/evolution_panel/right_panel/gold/Text"), index, 27);
        Util.SetText(WindowCache.GetComponent<Text>("inten_evo_panel/evolution_panel/right_panel/evolution_Button/Text"), index, 8);
        Util.SetText(WindowCache.GetComponent<Text>("inten_evo_panel/evolution_panel/right_panel/middle_panel/panel3/two_txt"), index, 11);
        Util.SetText(WindowCache.GetComponent<Text>("inten_evo_panel/evolution_panel/right_panel/middle_panel/panel3/four_txt"), index, 12);
        Util.SetText(WindowCache.GetComponent<Text>("inten_evo_panel/evolution_panel/suceessEvolvePanel/bg/content/back/title/up_h/up_1"), index, 50);
        Util.SetText(WindowCache.GetComponent<Text>("inten_evo_panel/evolution_panel/suceessEvolvePanel/bg/content/back/title/up_h/up_2"), index, 51);
        Util.SetText(WindowCache.GetComponent<Text>("inten_evo_panel/evolution_panel/suceessEvolvePanel/bg/content/back/title/up_h/up_3"), index, 52);
        Util.SetText(WindowCache.GetComponent<Text>("inten_evo_panel/evolution_panel/suceessEvolvePanel/bg/content/back/title/up_h/up_4"), index, 53);
        Util.SetText(WindowCache.GetComponent<Text>("inten_evo_panel/evolution_panel/suceessEvolvePanel/dengji/dengjishangxain"), index, 47);
        Util.SetText(WindowCache.GetComponent<Text>("inten_evo_panel/evolution_panel/suceessEvolvePanel/text"), index, 54);
        Util.SetText(WindowCache.GetComponent<Text>("inten_evo_panel/left_panel/tip/Text"), index, 49);

        Util.SetText(WindowCache.GetComponent<Text>("inten_evo_panel/tipPanel/background/top/equipinfo"), (int)TextForMatType.PublicUIText, 6);
        Util.SetText(WindowCache.GetComponent<Text>("inten_evo_panel/tipPanel/sure/Text"), (int)TextForMatType.PublicUIText, 4);
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p)) return false;
        if (p.Length > 1)
        {
            var type = (RuneInWhichPanel)p[0];
            var _curItem = p[1] as PItem;
            ChooseWhitchPanle(type, _curItem);
        }
        return true;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide)) return false;

        expTotal = 0;
        curItem = null;
        swallowedExp = 0;
        moduleRune.swallowedIdList.Clear();
        moduleRune.evolveList.Clear();
        beforeItem = null;
        return false;
    }

    void ChooseWhitchPanle(RuneInWhichPanel type, PItem item)
    {
        if (type == RuneInWhichPanel.Equip) return;
        if (item == null || item.growAttr == null || item.growAttr.runeAttr == null) return;

        _successPanel.SafeSetActive(false);
        tipPanel.SafeSetActive(false);

        RefreshData(item, false, type);
    }

    void InitializeText(RuneInWhichPanel type)
    {
        var content = WindowCache.GetComponent<Text>("inten_evo_panel/tipPanel/Text");

        if (type == RuneInWhichPanel.Intentify) Util.SetText(content, (int)TextForMatType.RuneUIText, 31);
        else Util.SetText(content, (int)TextForMatType.RuneUIText, 32);
    }

    void RefreshPanelActive(RuneInWhichPanel type)
    {
        intentify_panel.SafeSetActive(type==RuneInWhichPanel.Intentify);
        evo_panel.SafeSetActive(type==RuneInWhichPanel.Evolve);

        InitializeText(type);
    }

    #region 强化

    void RefreshData(PItem item, bool success, RuneInWhichPanel type)
    {
        if (type == RuneInWhichPanel.Equip) return;
        _type = type;
        curItem = item;
        moduleRune.evolveList.Clear();
        moduleRune.swallowedIdList.Clear();

        if (view) view.progress = 0;

        RefreshPanelActive(type);
        if (type == RuneInWhichPanel.Intentify)
        {
            RefreshDefault(item);

            if (!success) RefreshNoChangePanel(item);
            else
            {
                Util.SetText(runeLv, $"Lv.{item.growAttr.runeAttr.level}");
                pres.PlayAnimToPreview(moduleRune.GetExpProgress(curItem) + curItem.growAttr.runeAttr.level);
                moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 30));
            }

            RefreshProgress();
            RefreshChangePanel(item);

            moduleRune.GetUpLvList(item);
            dataSource.SetItems(moduleRune.upLevelList);
            noItmesTip.SafeSetActive(moduleRune.upLevelList.Count < 1);
            return;
        }


        if (type == RuneInWhichPanel.Evolve)
        {
            _RefreshPreViewPanel(moduleRune.evolveList);
            RefreshCoin();
            _RefreshStar(item);

            if (!success)
            {
                curItem.CopyTo(ref beforeItem);
                _RefreshNoChange(item);
            }
            else _SuccessPanel();

            moduleRune.GetUpStarList(item);
            moduleRune.SortGuideItem();
            dataSource.SetItems(moduleRune.upStarList);
            noItmesTip.SafeSetActive(moduleRune.upStarList.Count < 1);
        }
    }

    void RefreshChangePanel(PItem item)
    {
        if (item == null || item.growAttr == null || item.growAttr.runeAttr == null || item.growAttr.runeAttr.randAttrs == null) return;

        var _newLv = moduleRune.GetNewLv(item, expTotal);
        Util.SetText(newLv, $"Lv.{_newLv}");

        if (attrPanel != null)
        {
            for (int i = 0; i < attrPanel.Length; i++)
            {
                attrPanel[i].SafeSetActive(i < item.growAttr.runeAttr.randAttrs.Length);
                if (i >= item.growAttr.runeAttr.randAttrs.Length) continue;

                var attrId = item.growAttr.runeAttr.randAttrs[i].itemAttrId;
                var _attrId = moduleRune.GetCurAttrId(attrId);
                Util.SetText(attrPanel[i]?.GetComponent<Text>("name"), (int)TextForMatType.AttributeUIText, _attrId);
                Util.SetText(attrPanel[i]?.GetComponent<Text>("Text1"), moduleRune.GetCurRuneAttr(item, attrId));
                Util.SetText(attrPanel[i]?.GetComponent<Text>("Text2"), moduleRune.GetNewAttrInUp(item, attrId, _newLv));
            }
        }
    }

    void RefreshNoChangePanel(PItem item)
    {
        if (item == null || item.growAttr == null || item.growAttr.runeAttr == null || item.growAttr.runeAttr.randAttrs == null) return;
        var prop = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
        if (prop == null) return;

        Util.SetText(subType, moduleRune.GetRomaString(prop));
        Util.SetText(runeLv, $"Lv.{item.growAttr.runeAttr.level}");
        Util.SetText(originalLv, $"Lv.{item.growAttr.runeAttr.level}");
        if (stars != null)
        {
            for (int i = 0; i < stars.Length; i++)
                stars[i].SafeSetActive(i < item.growAttr.runeAttr.star);
        }

        Util.SetText(twoSuiteDesc, moduleRune.GetSuiteDesc(prop.suite, 2));
        Util.SetText(fourSuiteDesc, moduleRune.GetSuiteDesc(prop.suite, 4));

        var _newlv = moduleRune.GetNewLv(curItem, expTotal);
        var progress = moduleRune.GetExpProgress(curItem, _newlv, (int)expTotal);
        pres.SetCurrent(progress + _newlv);
        pres.SetPreview(progress + _newlv);
    }

    private void OnRuneUpLevel()
    {
        if (curItem == null || _type != RuneInWhichPanel.Intentify || moduleRune.swallowedIdList == null || moduleRune.swallowedIdList.Count < 1) return;
        moduleRune.SendSwallowed(curItem.itemId, moduleRune.swallowedIdList);
        RefreshBtnState(upBtn, false);
    }

    private void onSetLevel(int arg0)
    {
        Util.SetText(originalLv, $"Lv.{arg0}");
    }

    void RefreshCoin(uint exp = 0)
    {
        if (_type == RuneInWhichPanel.Intentify)
        {
            if (moduleGlobal.system == null || moduleGlobal.system.rune_upLv == null || moduleGlobal.system.rune_upLv.Length < 2) return;
            var coin = exp * moduleGlobal.system.rune_upLv[1];

            if (exp != 0) RefreshBtnState(upBtn, modulePlayer.coinCount >= coin);
            Util.SetText(costCoin, modulePlayer.coinCount < coin ? GeneralConfigInfo.GetNoEnoughColorString(coin.ToString()) : coin.ToString());

            return;
        }

        if (_type == RuneInWhichPanel.Evolve)
        {
            if (curItem == null || curItem.growAttr == null || curItem.growAttr.runeAttr == null) return;
            if (moduleGlobal.system.rune_upStar == null || moduleGlobal.system.rune_upStar.Length <= curItem.growAttr.runeAttr.star - 1) return;

            var price = moduleGlobal.system.rune_upStar[curItem.growAttr.runeAttr.star - 1];
            if (price == null) return;
            var prop = ConfigManager.Get<PropItemInfo>(price.itemTypeId);
            if (prop == null) return;
            var number = prop.itemType == PropType.Currency ? modulePlayer.GetMoneyCount((CurrencySubType)prop.subType) : (uint)moduleEquip.GetPropCount(prop.itemNameId);

            Util.SetText(_costCoin, number < price.price ? GeneralConfigInfo.GetNoEnoughColorString(price.price.ToString()) : price.price.ToString());
            RefreshBtnState(_evolveBtn, number >= price.price);
        }
    }

    void AddOrSubRune(uint exp, PItem data, bool isAdd)
    {
        if (!isAdd) expTotal -= exp;
        else expTotal += exp;

        RefreshProgress();
        RefreshChangePanel(curItem);

        if (!isAdd) moduleRune.swallowedIdList.Remove(data.itemId);
        else moduleRune.swallowedIdList.Add(data.itemId);

        int index = moduleRune.upLevelList.FindIndex(p => p.itemId == data.itemId);
        dataSource.SetItem(data, index);

        RefreshBtnState(upBtn, moduleRune.swallowedIdList.Count > 0);
    }

    void RefreshProgress()
    {
        var _newlv = moduleRune.GetNewLv(curItem, expTotal);
        var progress = moduleRune.GetExpProgress(curItem, _newlv, (int)expTotal);
        pres.SetPreviewUniformAnim(progress + _newlv, 0.5f);

        var expdesc = moduleRune.GetExpDescString(curItem, _newlv, (int)expTotal);
        Util.SetText(expDesc, expdesc);
    }

    void RefreshDefault(PItem item)
    {
        if (item == null) return;
        expTotal = 0;
        swallowedExp = 0;
        RefreshCoin();
        RefreshBtnState(upBtn, false);
    }

    #endregion

    #region 进化

    private void _OnEvoTipTrue()
    {
        if (_type != RuneInWhichPanel.Evolve) return;
        if (curItem == null || curItem.growAttr == null || curItem.growAttr.runeAttr == null) return;

        if (curItem.growAttr.runeAttr.level < moduleRune.GetCurMaxLv(curItem.growAttr.runeAttr.star))
        {
            moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 41));
            return;
        }
        if (moduleRune.starInfo != null)
        {
            if (curItem.growAttr.runeAttr.star >= moduleRune.starInfo[moduleRune.starInfo.Count - 1].ID)
            {
                moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 45));
                return;
            }
        }
        if (moduleRune.evolveList.Count < 4)
        {
            moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 44));
            return;
        }

        tipPanel.SafeSetActive(true);
        yesBtn?.onClick.RemoveAllListeners();
        yesBtn?.onClick.AddListener(() =>
        {
            moduleRune.SendEvolved(curItem.itemId, moduleRune.evolveList);
            tipPanel.SafeSetActive(false);
        });
    }

    void _SuccessPanel()
    {
        if (beforeItem == null || curItem == null) return;
        if (beforeItem.growAttr == null || beforeItem.growAttr.runeAttr == null) return;
        if (curItem.growAttr == null || curItem.growAttr.runeAttr == null) return;

        _successPanel.SafeSetActive(true);

        RefreshRuneData(_leftItem?.rectTransform(), beforeItem);
        RefreshRuneData(_rightItem?.rectTransform(), curItem);
        Util.SetText(_leftLevel, moduleRune.GetCurMaxLv(beforeItem.growAttr.runeAttr.star).ToString());
        Util.SetText(_rightLevel, moduleRune.GetCurMaxLv(curItem.growAttr.runeAttr.star).ToString());
    }

    void _RefreshNoChange(PItem item)
    {
        if (item == null || item.growAttr == null || item.growAttr.runeAttr == null) return;
        var prop = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
        if (prop == null) return;

        Util.SetText(_runeLv, $"Lv.{item.growAttr.runeAttr.level}");
        Util.SetText(_subType, moduleRune.GetRomaString(prop));
        if (_stars != null)
        {
            for (int i = 0; i < _stars.Length; i++)
                _stars[i].SafeSetActive(i < item.growAttr.runeAttr.star);
        }

        Util.SetText(_twoSuiteDesc, moduleRune.GetSuiteDesc(prop.suite, 2));
        Util.SetText(_fourSuiteDesc, moduleRune.GetSuiteDesc(prop.suite, 4));

        var lv = moduleRune.GetCurMaxLv(item.growAttr.runeAttr.star + 1);
        lv = lv == 0 ? 40 : lv;
        Util.SetText(_evolveLv, (int)TextForMatType.RuneUIText, 29, lv);
    }

    void _RefreshChange(PItem item)
    {
        if (_attrPanel != null)
        {
            for (int i = 0; i < _attrPanel.Length; i++)
            {
                _attrPanel[i].SafeSetActive(i < item.growAttr.runeAttr.randAttrs.Length);
                if (i >= item.growAttr.runeAttr.randAttrs.Length) continue;

                var attrId = item.growAttr.runeAttr.randAttrs[i].itemAttrId;
                var _attrId = moduleRune.GetCurAttrId(attrId);
                Util.SetText(_attrPanel[i]?.GetComponent<Text>("name"), (int)TextForMatType.AttributeUIText, _attrId);
                Util.SetText(_attrPanel[i]?.GetComponent<Text>("Text1"), moduleRune.GetCurRuneAttr(item, attrId));

                var text = _attrPanel[i]?.GetComponent<Text>("Text2");
                if (moduleRune.evolveList.Count > 0) Util.SetText(text, moduleRune.GetNewAttrInEvo(item, attrId));
                else Util.SetText(text, "");
            }
        }
    }

    void _RefreshStar(PItem item)
    {
        if (item == null || item.growAttr == null || item.growAttr.runeAttr == null) return;
        if (_midStars != null)
        {
            for (int i = 0; i < _midStars.Length; i++)
            {
                _midStars[i].SafeSetActive(i < item.growAttr.runeAttr.star + 1);
                if (i == item.growAttr.runeAttr.star) EnableTween(_midStars[i]?.transform, true);
                else EnableTween(_midStars[i]?.transform, false);
            }
        }
        if (_starSlider) _starSlider.fillAmount = (float)item.growAttr.runeAttr.star / 5;
    }

    void _RefreshPreViewPanel(List<ulong> preList)
    {
        if (_previewItems != null)
        {
            for (int i = 0; i < _previewItems.Length; i++)
            {
                _previewItems[i].SafeSetActive(i < preList.Count);
                if (i >= preList.Count) continue;

                var data = moduleRune.upStarList.Find(p => p.itemId == preList[i]);
                RefreshRuneData(_previewItems[i]?.rectTransform(), data);
            }
        }

        _RefreshChange(curItem);
    }

    void EnableTween(Transform tran, bool enable)
    {
        if (tran == null) return;
        var scale = tran.GetComponent<TweenScale>();
        var alpha = tran.GetComponent<TweenAlpha>();
        var _alpha = tran.GetComponent<CanvasGroup>();

        if (scale != null) scale.enabled = enable;
        if (alpha != null) alpha.enabled = enable;
        if (_alpha != null && alpha) _alpha.alpha = enable ? alpha.from : 1;
        tran.localScale = Vector3.one;
    }

    #endregion

    private void OnClickItem(RectTransform node, PItem data)
    {
        if (data == null || data.growAttr == null || data.growAttr.runeAttr == null) return;
        if (curItem == null || curItem.growAttr == null || curItem.growAttr.runeAttr == null) return;
        moduleCangku.RemveNewItem(data.itemId);

        if (data.isLock == 1)
        {
            moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 24));
            return;
        }

        //强化
        if (_type == RuneInWhichPanel.Intentify)
        {
            var swExp = moduleRune.GetCurSwallowExp(data);
            var maxExp = moduleRune.GetMaxExp(curItem);
            var exp = swExp >= maxExp ? maxExp : swExp;
            //减符文
            if (moduleRune.swallowedIdList.Contains(data.itemId))
            {
                swallowedExp -= swExp;
                RefreshCoin(swallowedExp);

                AddOrSubRune(exp, data, false);
            }
            //加符文
            else
            {
                if (moduleRune.GetNewLv(curItem, expTotal) >= moduleRune.GetCurMaxLv(curItem.growAttr.runeAttr.star))
                {
                    moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 23));
                    return;
                }

                if (moduleRune.IsCurMaxLv(curItem.growAttr.runeAttr.level, curItem.growAttr.runeAttr.star))
                {
                    moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 23));
                    return;
                }

                if (data.growAttr.runeAttr.star >= 3)
                {
                    tipPanel.SafeSetActive(true);
                    yesBtn.onClick.RemoveAllListeners();
                    yesBtn.onClick.AddListener(() =>
                    {
                        swallowedExp += swExp;
                        RefreshCoin(swallowedExp);
                        AddOrSubRune(exp, data, true);
                        tipPanel.SafeSetActive(false);
                    });
                    return;
                }

                swallowedExp += swExp;
                RefreshCoin(swallowedExp);
                AddOrSubRune(exp, data, true);
            }
        }
        else //进化
        {
            if (curItem.growAttr.runeAttr.level < moduleRune.GetCurMaxLv(curItem.growAttr.runeAttr.star))
            {
                moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 25));
                return;
            }

            if (moduleRune.evolveList.Count >= 4 && !moduleRune.evolveList.Contains(data.itemId)) return;
            moduleCangku.RemveNewItem(data.itemId);

            if (moduleRune.evolveList.Contains(data.itemId))
            {
                moduleRune.evolveList.Remove(data.itemId);
                if (moduleRune.evolveList.Count == 3) dataSource.UpdateItems();
            }
            else
            {
                moduleRune.evolveList.Add(data.itemId);
                if (moduleRune.evolveList.Count == 4) dataSource.UpdateItems();
            }

            _RefreshPreViewPanel(moduleRune.evolveList);

            int index = moduleRune.upStarList.FindIndex(p => p.itemId == data.itemId);
            dataSource.SetItem(data, index);
        }
    }

    private void OnSetItems(RectTransform node, PItem data)
    {
        if (data == null) return;
        RefreshRuneData(node, data);
        var mask = node.Find("mask");
        mask.SafeSetActive(false);

        var select = node.Find("selectBox");
        if (_type == RuneInWhichPanel.Intentify) select.SafeSetActive(moduleRune.swallowedIdList.Contains(data.itemId));
        else
        {
            select.SafeSetActive(moduleRune.evolveList.Contains(data.itemId));
            mask.SafeSetActive(moduleRune.evolveList.Count >= 4 && !moduleRune.evolveList.Contains(data.itemId));
        }
    }

    private void RefreshRuneData(RectTransform node, PItem data)
    {
        if (data == null || data.growAttr == null || data.growAttr.runeAttr == null) return;
        var prop = ConfigManager.Get<PropItemInfo>(data.itemTypeId);
        if (prop == null) return;
        Util.SetItemInfo(node, prop, data.growAttr.runeAttr.level, 0, true, data.growAttr.runeAttr.star);

        var lockImg = node.Find("lock");
        lockImg.SafeSetActive(data.isLock == 1);
        var max = node.Find("levelmax");
        max.SafeSetActive(moduleRune.IsMaxLv(data.growAttr.runeAttr.level));
        var equip = node.Find("get");
        equip.SafeSetActive(moduleRune.IsEquip(data));
        var equipText = node.Find("get/Text")?.GetComponent<Text>();
        if (equipText) equipText.color = moduleRune.GetCurQualityColor(GeneralConfigInfo.defaultConfig.qualitys, data.growAttr.runeAttr.star);
        Util.SetText(equipText, (int)TextForMatType.RuneUIText, 48);
        var mark = node.Find("mark");
        mark.SafeSetActive(false);

        var newProp = node.Find("new");
        bool isNew = moduleCangku.NewsProp(data.itemId);
        newProp.SafeSetActive(isNew);
    }

    void RefreshBtnState(Button btn, bool isTrue)
    {
        if (btn == null) return;
        var im = btn?.GetComponent<Image>();
        if (im) im.saturation = isTrue ? 1 : 0;
        var text = btn?.transform.GetComponent<Text>("Text");
        if (text) text.saturation = isTrue ? 1 : 0;
        var outLine = btn?.transform.GetComponent<Image>("outside");
        if (outLine) outLine.saturation = isTrue ? 1 : 0;
        if (btn) btn.interactable = isTrue;
        btn.SafeSetActive(false);
        btn.SafeSetActive(true);
    }

    void _ME(ModuleEvent<Module_Rune> e)
    {
        if (!WindowCache.actived) return;
        switch (e.moduleEvent)
        {
            case Module_Rune.IntentifySuccess:
                if (_type == RuneInWhichPanel.Intentify)
                {
                    var isCurMax = moduleRune.IsCurMaxLv(curItem.growAttr.runeAttr.level, curItem.growAttr.runeAttr.star);
                    if (isCurMax)
                    {
                        moduleRune.runeOpType = RuneInWhichPanel.Evolve;
                        RefreshData(curItem, false, RuneInWhichPanel.Evolve);
                    }
                    else
                    {
                        moduleRune.runeOpType = RuneInWhichPanel.Intentify;
                        RefreshData(curItem, true, RuneInWhichPanel.Intentify);
                    }
                }
                else if(_type == RuneInWhichPanel.Evolve)
                {
                    RefreshData(curItem, true, RuneInWhichPanel.Evolve);
                    successBtn?.onClick.RemoveAllListeners();
                    successBtn.onClick.AddListener(() =>
                    {
                        moduleRune.runeOpType = RuneInWhichPanel.Intentify;
                        RefreshData(curItem, false, RuneInWhichPanel.Intentify);
                    });
                }
                break;
            default:
                break;
        }
    }

    void _ME(ModuleEvent<Module_Player> e)
    {
        if (WindowCache.actived && e.moduleEvent == Module_Player.EventCurrencyChanged && !moduleRune.stayIntentify)
            RefreshCoin(swallowedExp);
    }
}