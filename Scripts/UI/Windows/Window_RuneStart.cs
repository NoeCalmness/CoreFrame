/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-02-20
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_RuneStart : Window
{
    Button bagBtn;
    Toggle checkAttrTog;
    Transform tipParent;
    Text tipConent;
    List<SubWindow_RunePanel> runes = new List<SubWindow_RunePanel>();
    SubWindow_AttriPanel attrPanel;
    List<SubWindow_SuitePanel> suites = new List<SubWindow_SuitePanel>();

    protected override void OnOpen()
    {
        bagBtn       = GetComponent<Button>("bottom_panel/pack/pack_icon");
        checkAttrTog = GetComponent<Toggle>("bottom_panel/change_btn");
        tipParent    = GetComponent<Transform>("bottom_panel/tip_panel");
        tipConent = GetComponent<Text>("bottom_panel/tip_panel/Text");

        var runeParent = GetComponent<Transform>("lingpo");
        if (runeParent)
        {
            runes.Clear();
            for (int i = 0; i < runeParent.childCount; i++)
            {
                var _gameObject = runeParent.GetChild(i)?.gameObject;
                runes.Add(SubWindowBase.CreateSubWindow<SubWindow_RunePanel, Window_RuneStart>(this, _gameObject));
            }
        }

        attrPanel = SubWindowBase.CreateSubWindow<SubWindow_AttriPanel, Window_RuneStart>(this, GetComponent<Transform>("bottom_panel/count_panel")?.gameObject);

        var suiteParent = GetComponent<Transform>("bottom_panel/lingpo_panel");
        if (suiteParent)
        {
            suites.Clear();
            for (int i = 0; i < suiteParent.childCount; i++)
            {
                var _gameObject = suiteParent.GetChild(i)?.gameObject;
                suites.Add(SubWindowBase.CreateSubWindow<SubWindow_SuitePanel, Window_RuneStart>(this, _gameObject));
            }
        }

        checkAttrTog?.onValueChanged.RemoveAllListeners();
        checkAttrTog?.onValueChanged.AddListener(OnChangeAttrPanel);
        bagBtn?.onClick.RemoveAllListeners();
        bagBtn?.onClick.AddListener(OnOpenCangku);

        MultiLanguage();
    }

    private void OnOpenCangku()
    {
        moduleCangku.chickType = WareType.Rune;
        ShowAsync<Window_Cangku>();
    }

    private void OnChangeAttrPanel(bool arg)
    {
        attrPanel?.UnInitialize();
        for (int i = 0; i < suites.Count; i++)
            suites[i]?.UnInitialize();

        if (arg)
        {
            tipParent.SafeSetActive(false);
            var dic = moduleRune.GetNewAttributes();
            attrPanel?.Initialize(dic);
        }
        else
        {
            tipParent.SafeSetActive(moduleRune.currentEquip == null || moduleRune.currentEquip.Count < 1 || moduleRune.suiteDic == null || moduleRune.suiteDic.Count < 1);
            RefreshSuitePanel();
        }
    }

    void MultiLanguage()
    {
        var index = (int)TextForMatType.RuneUIText;
        Util.SetText(GetComponent<Text>("title/title_big"), index, 0);
        Util.SetText(GetComponent<Text>("title/title_small"), index, 1);
        Util.SetText(GetComponent<Text>("bottom_panel/tip_panel/Text"), index, 2);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();
        moduleRune.UpdateRead(0, true);

        attrPanel?.UnInitialize();
        RefreshRunePanel();
        RefreshSuitePanel();
        if (checkAttrTog) checkAttrTog.isOn = false;
    }

    protected override void OnReturn()
    {
        if (moduleRune.sourceType != 1) Hide();
        else SkipBackTo<Window_Home>();
    }

    protected override void OnClose()
    {
        if (runes != null)
        {
            for (int i = 0; i < runes.Count; i++)
                runes[i]?.Destroy();
        }

        attrPanel?.Destroy();

        if (suites != null)
        {
            for (int i = 0; i < suites.Count; i++)
                suites[i].Destroy();
        }
    }

    void RefreshRunePanel()
    {
        var equipList = moduleRune.currentEquip;
        for (int i = 0; i < runes.Count; i++)
        {
            var rune = equipList?.Find(p => p.GetPropItem()?.subType == i + 1);
            runes[i]?.UnInitialize();
            runes[i]?.Initialize(rune, i);
        }

        tipParent.SafeSetActive(equipList == null || equipList.Count < 1 || moduleRune.suiteDic == null || moduleRune.suiteDic.Count < 1);
    }

    void RefreshSuitePanel()
    {
        int count = moduleRune.suiteDic.Count;

        for (int i = 0; i < suites.Count; i++)
            suites[i]?.UnInitialize();

        int index = 0;
        foreach (var item in moduleRune.suiteDic)
        {
            if (item.Value < 2) continue;
            if (item.Value == 6)
            {
                if (suites.Count > 0) suites[0]?.Initialize(item.Key, 2, 0);
                if (suites.Count > 1) suites[1]?.Initialize(item.Key, 2, 1);
                if (suites.Count > 2) suites[2]?.Initialize(item.Key, 4, 2);
                continue;
            }

            if (item.Value == 4)
            {
                if (suites.Count > index)
                {
                    suites[index]?.Initialize(item.Key, 2, index);
                    index++;
                }
                if (suites.Count > index) suites[index]?.Initialize(item.Key, 4, index);
                continue;
            }

            if (item.Value == 2)
            {
                suites[index].Initialize(item.Key, 2, index);
                index++;
            }
        }
    }
}

public class SubWindow_RunePanel : SubWindowBase<Window_RuneStart>
{
    Button replaceBtn;
    Button iconBtn;
    Transform buttom;
    Button emptyBtn;
    Image clock;
    Transform runeStarParent;
    Image[] runeStars;
    Image runeIcon;
    Text runeLevel;
    Text romaText;
    Text runeName;
    Image redPoint;

    Image textBgColor;
    Image bgColor;
    Image outsideColor;
    Image replaceColor;
    Outline levelColor;
    Outline romaColor;

    PItem curRune;
    int subType;

    protected override void InitComponent()
    {
        replaceBtn       = Root.GetComponent<Button>("chage_btn");
        emptyBtn         = Root.GetComponent<Button>("icon");
        buttom           = Root.GetComponent<Transform>("bottom");
        clock            = Root.GetComponent<Image>("bottom/clock");
        runeStarParent   = Root.GetComponent<Transform>("bottom/star_parent");
        runeStars        = runeStarParent?.GetComponentsInChildren<Image>(true);
        runeIcon         = Root.GetComponent<Image>("lingpo_icon");
        iconBtn          = runeIcon?.GetComponent<Button>();
        runeLevel        = Root.GetComponent<Text>("bottom/level_Text");
        romaText         = Root.GetComponent<Text>("bottom/roma_txt");
        runeName         = Root.GetComponent<Text>("bottom/lingpo_name");
        textBgColor      = Root.GetComponent<Image>("bottom/bg");
        bgColor          = Root.GetComponent<Image>("bg");
        outsideColor     = Root.GetComponent<Image>("bg/outside");
        redPoint         = Root.GetComponent<Image>("bg/outside/mark");
        replaceColor     = replaceBtn.GetComponent<Image>();
        levelColor       = runeLevel.GetComponent<Outline>();
        romaColor        = romaText.GetComponent<Outline>();
    }

    public override void MultiLanguage()
    {
        Util.SetText(WindowCache.GetComponent<Text>("lingpo/1/icon/Text"), 10103, 0);
        Util.SetText(WindowCache.GetComponent<Text>("lingpo/2/icon/Text"), 10103, 1);
        Util.SetText(WindowCache.GetComponent<Text>("lingpo/3/icon/Text"), 10103, 2);
        Util.SetText(WindowCache.GetComponent<Text>("lingpo/4/icon/Text"), 10103, 3);
        Util.SetText(WindowCache.GetComponent<Text>("lingpo/5/icon/Text"), 10103, 4);
        Util.SetText(WindowCache.GetComponent<Text>("lingpo/6/icon/Text"), 10103, 5);
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p)) return false;

        if (p.Length > 1)
        {
            curRune = p[0] as PItem;
            subType = (int)p[1] + 1;
        }

        replaceBtn?.onClick.AddListener(OnOpenEquipWindow);
        iconBtn?.onClick.AddListener(() => OnOpenDetailWindow(curRune));
        emptyBtn?.onClick.AddListener(() => OnOpenEquipWindow(subType));

        Refresh(curRune);
        return true;
    }

    private void OnOpenDetailWindow(PItem curRune)
    {
        if (curRune == null) return;
        Window.SetWindowParam<Window_RuneMain>(curRune);
        Window.ShowAsync<Window_RuneMain>();
    }

    private void OnOpenEquipWindow()
    {
        if (curRune == null) return;

        moduleRune.runeOpType = RuneInWhichPanel.Equip;

        moduleRune.SetCurOpItem(curRune);
        Window.ShowAsync<Window_RuneEquip>();
    }

    private void OnOpenEquipWindow(int subType)
    {
        if (subType == 0) return;

        List<PItem> list = new List<PItem>();
        if (moduleRune.allSubTypeDic.ContainsKey((RuneType)subType)) list = moduleRune.allSubTypeDic[(RuneType)subType];

        if (list.Count < 1)
        {
            moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 6));
            return;
        }

        if (curRune == null) curRune = list[0];
        moduleRune.runeOpType = RuneInWhichPanel.Equip;
        moduleRune.SetCurOpItem(curRune);
        Window.ShowAsync<Window_RuneEquip>();
    }

    void Refresh(PItem rune)
    {
        replaceBtn.SafeSetActive(rune != null);
        emptyBtn.SafeSetActive(rune == null);
        runeIcon.SafeSetActive(rune != null);
        buttom.SafeSetActive(rune != null);
        clock.SafeSetActive(rune != null && rune.isLock == 1);
        runeLevel.SafeSetActive(rune != null && rune.growAttr?.runeAttr?.level > 0);
        runeName.SafeSetActive(rune != null);
        romaText.SafeSetActive(rune != null);
        runeStarParent.SafeSetActive(rune != null);

        var _color = moduleRune.GetColorByIndex(GeneralConfigInfo.defaultConfig.norRColor, 0);
        if (textBgColor) textBgColor.color = _color.SetAlpha(textBgColor.color.a);
        if (bgColor) bgColor.color = _color.SetAlpha(bgColor.color.a);
        if (replaceColor) replaceColor.color = _color.SetAlpha(replaceColor.color.a);
        if (levelColor) levelColor.effectColor = _color.SetAlpha(levelColor.effectColor.a);
        if (romaColor) romaColor.effectColor = _color.SetAlpha(romaColor.effectColor.a);

        var outColor = moduleRune.GetColorByIndex(GeneralConfigInfo.defaultConfig.norRColor, 1);
        if (outsideColor) outsideColor.color = outColor.SetAlpha(outsideColor.color.a);

        redPoint.SafeSetActive(moduleRune.GetCurSubHintBool((RuneType)subType));
        if (rune == null) return;

        var prop = ConfigManager.Get<PropItemInfo>(rune.itemTypeId);
        if (prop == null) return;

        if (prop.mesh != null && prop.mesh.Length > 0) AtlasHelper.SetRune(runeIcon, prop.mesh[0]);

        Util.SetText(runeLevel, $"Lv.{rune.growAttr?.runeAttr?.level}");

        var s = moduleRune.GetRomaString(prop);
        Util.SetText(romaText, s);
        Util.SetText(runeName, prop.itemName);

        if (runeStars != null)
        {
            for (int i = 0; i < runeStars.Length; i++)
                runeStars[i].SafeSetActive(i < rune?.growAttr?.runeAttr?.star);
        }

        var color = moduleRune.GetColorByIndex(moduleRune.GetCurSuiteColor(prop), 0);
        if (textBgColor) textBgColor.color = color.SetAlpha(textBgColor.color.a);
        if (bgColor) bgColor.color = color.SetAlpha(bgColor.color.a);
        if (replaceColor) replaceColor.color = color.SetAlpha(replaceColor.color.a);
        if (levelColor) levelColor.effectColor = color.SetAlpha(levelColor.effectColor.a);
        if (romaColor) romaColor.effectColor = color.SetAlpha(romaColor.effectColor.a);

        var _outSilde = moduleRune.GetColorByIndex(moduleRune.GetCurSuiteColor(prop), 1);
        if (outsideColor) outsideColor.color = _outSilde.SetAlpha(outsideColor.color.a);
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide)) return false;

        replaceBtn?.onClick.RemoveAllListeners();
        iconBtn?.onClick.RemoveAllListeners();
        emptyBtn?.onClick.RemoveAllListeners();
        return false;
    }
}

public class SubWindow_AttriPanel : SubWindowBase<Window_RuneStart>
{
    Text hp;
    Text attack;
    Text defense;
    Text knock;
    Text knockRate;
    Text artifice;
    Text attackSpeed;
    Text moveSpeed;
    Text bone;
    Text brutal;

    protected override void InitComponent()
    {
        hp          = Root.GetComponent<Text>("Text1/Text");
        attack      = Root.GetComponent<Text>("Text2/Text");
        defense     = Root.GetComponent<Text>("Text3/Text");
        knock       = Root.GetComponent<Text>("Text4/Text");
        knockRate   = Root.GetComponent<Text>("Text5/Text");
        artifice    = Root.GetComponent<Text>("Text6/Text");
        attackSpeed = Root.GetComponent<Text>("Text7/Text");
        moveSpeed   = Root.GetComponent<Text>("Text8/Text");
        bone        = Root.GetComponent<Text>("Text9/Text");
        brutal      = Root.GetComponent<Text>("Text10/Text");
    }

    public override void MultiLanguage()
    {
        Util.SetText(Root.GetComponent<Text>("Text1"), 212, 5);
        Util.SetText(Root.GetComponent<Text>("Text2"), 212, 7);
        Util.SetText(Root.GetComponent<Text>("Text3"), 212, 8);
        Util.SetText(Root.GetComponent<Text>("Text4"), 212, 9);
        Util.SetText(Root.GetComponent<Text>("Text5"), 212, 10);
        Util.SetText(Root.GetComponent<Text>("Text6"), 212, 11);
        Util.SetText(Root.GetComponent<Text>("Text7"), 212, 12);
        Util.SetText(Root.GetComponent<Text>("Text8"), 212, 13);
        Util.SetText(Root.GetComponent<Text>("Text9"), 212, 14);
        Util.SetText(Root.GetComponent<Text>("Text10"), 212, 15);
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p)) return false;
        if (p.Length > 0) Refresh(p[0] as Dictionary<uint, double>);

        return true;
    }

    void Refresh(Dictionary<uint, double> attrs)
    {
        RefreshDefault();
        foreach (var item in attrs)
            RefreshOneAttribute(item.Key, item.Value);
    }

    void RefreshDefault()
    {
        Util.SetText(hp,          "+0");
        Util.SetText(attack,      "+0");
        Util.SetText(defense,     "+0");
        Util.SetText(knock,       "+0%");
        Util.SetText(knockRate,   "+0%");
        Util.SetText(artifice,    "+0%");
        Util.SetText(attackSpeed, "+0%");
        Util.SetText(moveSpeed,   "+0%");
        Util.SetText(bone,        "+0");
        Util.SetText(brutal,      "+0");
    }

    void RefreshOneAttribute(uint id, double value)
    {
        switch (id)
        {
            case 5: Util.SetText(hp,           "+" + value.ToString("F0")); break;
            case 7: Util.SetText(attack,       "+" + value.ToString("F0")); break;
            case 8: Util.SetText(defense,      "+" + value.ToString("F0")); break;
            case 9: Util.SetText(knock,        "+" + value.ToString("P2")); break;
            case 10: Util.SetText(knockRate,   "+" + value.ToString("P2")); break;
            case 11: Util.SetText(artifice,    "+" + value.ToString("P2")); break;
            case 12: Util.SetText(attackSpeed, "+" + value.ToString("P2")); break;
            case 13: Util.SetText(moveSpeed,   "+" + value.ToString("P2")); break;
            case 14: Util.SetText(bone,        "+" + value.ToString("F0")); break;
            case 15: Util.SetText(brutal,      "+" + value.ToString("F0")); break;
            default:
                break;
        }
    }
}

public class SubWindow_SuitePanel : SubWindowBase<Window_RuneStart>
{
    Text tittleText;
    Text descText;
    Image bgImageColor;
    Image outsideColor;
    Image icon;

    protected override void InitComponent()
    {
        tittleText       = Root.GetComponent<Text>("tittle_txt");
        descText         = Root.GetComponent<Text>("Text");
        bgImageColor     = Root.GetComponent<Image>("bg");
        outsideColor     = Root.GetComponent<Image>("bg/outside");
        icon             = Root.GetComponent<Image>("bg/icon");
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p)) return false;

        if (p.Length > 2) Refresh((ushort)p[0], (int)p[1], (int)p[2]);

        return true;
    }

    void Refresh(ushort suite, int count, int index)
    {
        Root.SafeSetActive(suite != 0);
        if (suite == 0) return;

        var prop = moduleRune.allProps.Find(p => p.itemType == PropType.Rune && p.suite == suite);
        if (prop) AtlasHelper.SetRune(icon, prop.icon);

        var s = Util.GetString((int)TextForMatType.RuneUIText, count == 2 ? 3 : 4);
        Util.SetText(tittleText, s);
        Util.SetText(descText, moduleRune.GetSuiteDesc(suite, count));

        Color color = Color.white;
        if (count == 4) color = moduleRune.GetColorByIndex(GeneralConfigInfo.defaultConfig.fourSColor, 0);
        else color = moduleRune.GetColorByIndex(moduleRune.GetCurSuiteColor(index), 0);

        if (bgImageColor) bgImageColor.color = color.SetAlpha(bgImageColor.color.a);
        if (outsideColor) outsideColor.color = color.SetAlpha(outsideColor.color.a);
        if (tittleText) tittleText.color = color.SetAlpha(tittleText.color.a);
    }
}