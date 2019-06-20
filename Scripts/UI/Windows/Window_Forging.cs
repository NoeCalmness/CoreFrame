/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-11-15
 * 
 ***************************************************************************************************/

using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;

public class Window_Forging : Window
{
    private DataSource<PItem> m_weaponData;
    private ScrollView m_view;
    private Transform m_checkGroup;

    private List<Toggle> m_lableList = new List<Toggle>();


    protected override void OnOpen()
    {
        m_view = GetComponent<ScrollView>("intitial_Panel/scrollView");
        m_weaponData = new DataSource<PItem>(null, m_view, SetDataInfo, SetDataClick);
        m_checkGroup = GetComponent<RectTransform>("intitial_Panel/checkBox");

        SetText();
        CheckBtn();
    }
    private void SetText()
    {
        Util.SetText(GetComponent<Text>("intitial_panel/decoration1/biaoti/text1"), 224, 0);
        Util.SetText(GetComponent<Text>("intitial_panel/decoration1/biaoti/text2"), 224, 1);
        Util.SetText(GetComponent<Text>("intitial_Panel/checkBox/weapon/Text"), 224, 47);
        Util.SetText(GetComponent<Text>("intitial_Panel/checkBox/weapon/xz/weapon_text"), 224, 47);
        Util.SetText(GetComponent<Text>("intitial_Panel/checkBox/qiangxie/Text"), 224, 48);
        Util.SetText(GetComponent<Text>("intitial_Panel/checkBox/qiangxie/xz/qiangxie_text"), 224, 48);
        Util.SetText(GetComponent<Text>("intitial_Panel/checkBox/armor/Text"), 224, 49);
        Util.SetText(GetComponent<Text>("intitial_Panel/checkBox/armor/xz/armor_text"), 224, 49);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();//top工具条的显示
        if (m_subTypeLock != -1)
        {
            if (m_subTypeLock <= m_lableList.Count) moduleForging.ClickType = (EquipType)m_subTypeLock;
        }

        if ((int)moduleForging.ClickType > m_lableList.Count || moduleForging.ClickType == EquipType.None) return;
        m_view.progress = 0;
        ToggleShow();
    }

    private void CheckBtn()
    {
        m_lableList.Clear();
        foreach (Transform item in m_checkGroup)
        {
            Toggle t = item.gameObject.GetComponent<Toggle>();
            m_lableList.Add(t);
        }
        for (int i = 0; i < m_lableList.Count; i++)
        {
            EquipType type = (EquipType)(i + 1);
            Toggle tt = m_lableList[i];
            var hint = tt.transform.Find("mark");
            hint.gameObject.SetActive(moduleCangku.TypeHint(type));
            tt.onValueChanged.AddListener(delegate
            {
                if (tt.isOn && moduleForging.ClickType != type)
                {
                    moduleCangku.RemveNewItem(moduleForging.ClickType);
                    moduleForging.ClickType = type;
                    ToggleShow();
                }
            });
        }
    }

    private void ToggleShow()
    {
        var index = (int)moduleForging.ClickType - 1;
        if (index == -1 || index >= m_lableList.Count) return;

        m_weaponData.SetItems(moduleForging.GetAllItems(moduleForging.ClickType));
        m_lableList[index].isOn = true;
        ToggleHint();
    }

    private void ToggleHint()
    {
        for (int i = 0; i < m_lableList.Count; i++)
        {
            var hint = m_lableList[i].transform.Find("mark");
            hint.gameObject.SetActive(moduleCangku.TypeHint((EquipType)(i + 1)));
            if (m_lableList[i].isOn) hint.gameObject.SetActive(false);
        }
    }

    private void SetDataInfo(RectTransform rt, PItem info)
    {
        if (info == null) return;
        var level = info.GetIntentyLevel();
        var prop = ConfigManager.Get<PropItemInfo>(info.itemTypeId);
        if (prop == null) return;
        int star = prop.quality;

        Util.SetItemInfo(rt, prop, level, 0, true, star);

        GameObject wear = rt.Find("equiped").gameObject;

        bool wearShow = moduleEquip.currentDressClothes.Exists(a => a.itemId == info.itemId);
        if (moduleEquip.weapon != null && moduleEquip.offWeapon != null)
            if (info.itemId == moduleEquip.weapon.itemId || info.itemId == moduleEquip.offWeapon.itemId)
                wearShow = true;

        wear.gameObject.SetActive(wearShow);

        Text txt = rt.Find("equiped/Text").GetComponent<Text>();
        Util.SetText(txt, ConfigText.GetDefalutString(224, 30));

        Image news = rt.Find("new").GetComponent<Image>();
        news.gameObject.SetActive(moduleCangku.NewsProp(info.itemId));

        Image locks = rt.Find("lock").GetComponent<Image>();
        locks.gameObject.SetActive(info.isLock == 1);

        Image max = rt.Find("levelmax").GetComponent<Image>();
        bool maxShow = false;
        var t = moduleEquip.GetPeiviewEuipeTypeByItem(info);
        if (t == PreviewEquipType.None || t == PreviewEquipType.Enchant) maxShow = true;
        max.gameObject.SetActive(maxShow);
    }

    private void SetDataClick(RectTransform rt, PItem info)
    {
        if (info == null) return;
        moduleCangku.RemveNewItem(info.itemId);
        Image news = rt.Find("new").GetComponent<Image>();
        news.gameObject.SetActive(false);
        moduleCangku.m_detailItem = info;
        ShowAsync<Window_Equipinfo>();
    }

    protected override void OnReturn()
    {
        base.OnReturn();
        for (int i = 0; i < m_lableList.Count; i++)
        {
            m_lableList[i].gameObject.SetActive(false);
            m_lableList[i].isOn = false;
            m_lableList[i].gameObject.SetActive(true);
        }
        moduleCangku.RemveNewItem(moduleForging.ClickType);
    }
}

