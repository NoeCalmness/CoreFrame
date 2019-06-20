/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-10-11
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Equipinfo : Window
{
    public const int SUB_TYPE_CLEAR = 1;
    #region UI
    private RectTransform m_leftTopPlane;
    private Text m_leftDesc;

    private Text m_spriteNo;
    private SoulEntry soulEntry;

    private GameObject m_attriPrefab;
    private Transform m_attriParent;
    private List<GameObject> m_attrList = new List<GameObject>();

    private RectTransform m_suitHave;
    private Text m_suitNo;
    private Text m_suitState;
    private List<GameObject> m_suitList = new List<GameObject>();

    private SuitProperty m_suitProperty;
    private Transform m_btnGroup;
    private Button m_btnClearSuit;
    private EquipInfoWindow_ClearSuit m_clearSuit;

    private Button m_strengBtn;//强化
    private GameObject m_strengHint;
    private Button m_advancedBtn;//升级
    private GameObject m_advancedHint;
    private Button m_soulBtn;//入魂
    private GameObject m_soulHint;
    private Button m_orderBtn;//升阶
    private GameObject m_orderHint;
    private Button m_sublimaBtn;//升华
    private GameObject m_sublimaHint;
    private Button m_spiritBtn;//器灵
    private GameObject m_spiritHint;

    public PItem item;
    #endregion

    //保留几位小数
    private int digit = 0;

    protected override void OnOpen()
    {
        m_attrList.Clear();
        m_suitList.Clear();

        m_leftTopPlane = GetComponent<RectTransform>("left");
        m_leftDesc = GetComponent<Text>("left/bottom/mask/content");
        //器灵
        m_spriteNo = GetComponent<Text>("right/bottom/nothing");

        m_attriPrefab = GetComponent<RectTransform>("right/top/content/type").gameObject;
        m_attriParent = GetComponent<RectTransform>("right/top/content");

        m_suitHave = GetComponent<RectTransform>("right/middle/have");
        m_suitNo = GetComponent<Text>("right/middle/nothing");
        m_suitState = GetComponent<Text>("right/middle/have/state");
        m_btnClearSuit = GetComponent<Button>("right/middle/have/clear_Btn");

        m_btnGroup = GetComponent<RectTransform>("buttons");

        m_strengBtn = GetComponent<Button>("buttons/0");
        m_advancedBtn = GetComponent<Button>("buttons/1");
        m_soulBtn = GetComponent<Button>("buttons/2");
        m_orderBtn = GetComponent<Button>("buttons/3");
        m_sublimaBtn = GetComponent<Button>("buttons/4");
        m_spiritBtn = GetComponent<Button>("buttons/5");
        m_strengHint = GetComponent<Image>("buttons/0/mark").gameObject;
        m_advancedHint = GetComponent<Image>("buttons/1/mark").gameObject;
        m_soulHint = GetComponent<Image>("buttons/2/mark").gameObject;
        m_orderHint = GetComponent<Image>("buttons/3/mark").gameObject;
        m_sublimaHint = GetComponent<Image>("buttons/4/mark").gameObject;
        m_spiritHint = GetComponent<Image>("buttons/5/mark").gameObject;

        m_suitProperty = new SuitProperty(GetComponent<Transform>("right/middle/have"));
        m_clearSuit = SubWindowBase.CreateSubWindow<EquipInfoWindow_ClearSuit, Window_Equipinfo>(this, GetComponent<Transform>("tip")?.gameObject);
        m_clearSuit.Set(false);
        soulEntry = new SoulEntry(GetComponent<Transform>("right/bottom"));
        SetText();
    }

    protected override void OnClose()
    {
        base.OnClose();
        m_clearSuit.Destroy();
    }

    private void SetText()
    {
        Util.SetText(GetComponent<Text>("left/bottom/have/attr"), 249, 0);
        Util.SetText(m_spriteNo, 249, 1);
        Util.SetText(GetComponent<Text>("right/top/title"), 249, 2);
        Util.SetText(GetComponent<Text>("right/middle/title"), 249, 3);
        Util.SetText(m_suitNo, 249, 4);
        Util.SetText(m_suitState, 249, 5);
        Util.SetText(GetComponent<Text>("right/bottom/title"), 249, 6);
        Util.SetText(GetComponent<Text>("buttons/0/text"), 249, 8);
        Util.SetText(GetComponent<Text>("buttons/1/text"), 249, 9);
        Util.SetText(GetComponent<Text>("buttons/2/text"), 249, 10);
        Util.SetText(GetComponent<Text>("buttons/3/text"), 249, 11);
        Util.SetText(GetComponent<Text>("buttons/4/text"), 249, 12);
        Util.SetText(GetComponent<Text>("buttons/5/text"), 249, 13);
        Util.SetText(GetComponent<Text>("buttons/6/text"), 249, 14);
        Util.SetText(GetComponent<Text>("tip/content/equipinfo"), 249, 15);
        Util.SetText(GetComponent<Text>("tip/content/consume"), 249, 16);
        Util.SetText(GetComponent<Text>("tip/content/clear_Btn/Text"), 249, 17);
        Util.SetText(GetComponent<Text>("tip/content/content_tip"), 249, 18);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();
        item = moduleCangku.GetItemByGUID(moduleCangku.m_detailItem.itemId);
        if (item == null) return;
        SetBtnClick();
        SetLeft();
        RefreshAttributePreview();
        moduleEquip.LoadModel(item, Layers.WEAPON, true, 1);
        SetSprite();
        SetSuitAttr(item);

        if (moduleGlobal.targetMatrial.isProcess)
        {
            if (moduleGlobal.targetMatrial.isFinish)
            {
                if (moduleGlobal.targetMatrial.data != null && (int) moduleGlobal.targetMatrial.data == SUB_TYPE_CLEAR)
                {
                    ClearSuit();
                }
            }
            else
                moduleGlobal.targetMatrial.Clear();
        }
    }

    #region Btn click hint

    private void SetBtnClick()
    {
        foreach (Transform obj in m_btnGroup)
        {
            obj.GetComponent<Button>().interactable = true;
            obj.Find("mark").gameObject.SetActive(false);
            obj.GetComponent<Button>().onClick.RemoveAllListeners();
        }

        m_strengBtn.onClick.AddListener(delegate
        {
            if (m_strengHint.activeInHierarchy)
            {
                var max = moduleEquip.GetLimitIntenLevelByExp(Module_Equip.GetEquipTypeByItem(item), item.GetIntentyLevel(), moduleEquip.GetBagTotalIntentyExp() + item.GetCurrentTotalExp(), item.HasEvolved());
                if (max == null) return;
                if (moduleEquip.IntentyRead.ContainsKey(item.itemId)) moduleEquip.IntentyRead[item.itemId] = max.level;
                else moduleEquip.IntentyRead.Add(item.itemId, max.level);
            }

            moduleEquip.operateItem = item;
            ShowAsync<Window_Strength>();
        });
        m_advancedBtn.onClick.AddListener(delegate
        {
            if (m_advancedHint.activeInHierarchy)
            {
                int leftLevel = item.GetIntentyLevel();
                int nextLimitLevel = moduleEquip.GetLimitIntenLevel(Module_Equip.GetEquipTypeByItem(item), leftLevel, true);
                if (moduleEquip.EvolveRead.ContainsKey(item.itemId)) moduleEquip.EvolveRead[item.itemId] = nextLimitLevel;
                else moduleEquip.EvolveRead.Add(item.itemId, nextLimitLevel);
            }

            moduleEquip.operateItem = item;
            ShowAsync<Window_Evolution>();
        });
        m_soulBtn.onClick.AddListener(delegate
        {
            if (m_soulHint.activeInHierarchy)
            {
                var wea = ConfigManager.Get<WeaponAttribute>(item.itemTypeId);
                if (wea != null)
                {
                    var read = moduleEquip.InsoulLevel(wea.elementId, (int)item.growAttr.equipAttr.expr, item.growAttr.equipAttr.level);
                    if (moduleEquip.InsoulRead.ContainsKey(item.itemId)) moduleEquip.InsoulRead[item.itemId] = read;
                    else moduleEquip.InsoulRead.Add(item.itemId, read);
                    //记录为已读状态 存储当前可达到的最高级
                }
            }
            moduleForging.InsoulItem = item.Clone();
            ShowAsync<Window_Merge>();
        });
        m_orderBtn.onClick.AddListener(delegate
        {
            if (m_orderHint.activeInHierarchy)
            {
                var have = moduleEquip.UpLoadRead.Exists(a => a == item.itemId);
                if (!have) moduleEquip.UpLoadRead.Add(item.itemId);
            }
            moduleForging.UpLoadItem = item.Clone();
            ShowAsync<Window_Upload>();
        });
        m_sublimaBtn.onClick.AddListener(delegate
        {
            var currentItem = moduleEquip.GetProp(item.itemId);
            if (moduleFurnace.IsSublimationMax(currentItem))
            {
                moduleGlobal.ShowMessage(249, 20);
                return;
            }
            moduleFurnace.currentSublimationItem = currentItem;
            ShowAsync<Window_Sublimation>();
        });
        m_spiritBtn.onClick.AddListener(delegate
        {
            moduleFurnace.currentSoulItem = moduleEquip.GetProp(item.itemId);
            ShowAsync<Window_Soul>();
        });

        m_btnClearSuit?.onClick.AddListener(delegate
        {
            m_clearSuit.Initialize(moduleEquip.GetProp(item.itemId));
        });

        SetBtnHint();
    }

    public void ClearSuit()
    {
        m_btnClearSuit?.onClick.Invoke();
    }

    private void SetBtnHint()
    {
        var prop = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
        if (prop == null) return;

        SetUpStreng();
        SetSpriteSuit();

        //武器的入魂和升级
        m_orderBtn.interactable = true;
        m_soulHint.gameObject.SetActive(false);
        m_orderHint.gameObject.SetActive(moduleEquip.CheckDegreeElevation(item));
        if (prop.itemType == PropType.Weapon && prop.subType != 100)
        {
            m_soulBtn.interactable = true;
            m_soulHint.gameObject.SetActive(moduleEquip.CheckInSoul(item));
        }
        else m_soulBtn.interactable = false;

        var wea = ConfigManager.Get<WeaponAttribute>(item.itemTypeId);
        if (wea == null)
        {
            m_soulBtn.interactable = false;
            m_orderBtn.interactable = false;
        }
        else
        {
            var pro = ConfigManager.Get<PropItemInfo>(wea.elementId);
            if (pro == null) m_soulBtn.interactable = false;
            if (wea.quality == null || wea.quality?.Length <= 0 || wea.quality[0].attributes == null || wea.quality[0].attributes.Length <= 0)
            {
                m_orderBtn.interactable = false;
            }
        }
    }

    //强化和升级
    private void SetUpStreng()
    {
        var t = moduleEquip.GetPeiviewEuipeTypeByItem(item);
        //装备的强化和进化
        m_strengBtn.interactable = true;
        m_strengBtn.gameObject.SetActive(t == PreviewEquipType.Intentify);
        m_advancedBtn.gameObject.SetActive(t == PreviewEquipType.Evolve);
        if (t != PreviewEquipType.Intentify && t != PreviewEquipType.Evolve)
        {
            m_strengBtn.gameObject.SetActive(true);
            m_strengBtn.interactable = false;
        }

        if (t == PreviewEquipType.Intentify) m_strengHint.gameObject.SetActive(moduleEquip.CheckIntenty(item));
        else if (t == PreviewEquipType.Evolve) m_advancedHint.gameObject.SetActive(moduleEquip.CheckEvolve(item));
    }

    //器灵和升华
    private void SetSpriteSuit()
    {
        m_spiritHint.gameObject.SetActive(moduleEquip.NeedNoticeSoul(item));
        m_sublimaHint.gameObject.SetActive(moduleEquip.NeedNoticeSublimation(item));
    }

    #endregion

    private void SetLeft()
    {
        ForgePreviewPanel leftTopInfo = m_leftTopPlane.GetComponentDefault<ForgePreviewPanel>();
        leftTopInfo.ForingItem(item);

        var prop = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
        if (prop == null) return;

        Util.SetText(m_leftDesc, ConfigText.GetDefalutString(prop.desc));

    }

    #region base atter

    private void RefreshAttributePreview()
    {
        List<PItemAttachAttr> attrs = GetAttributePreview(item);
        SetParent(m_attriParent, m_attriPrefab, attrs.Count - m_attrList.Count, m_attrList);

        for (int i = 0; i < attrs.Count; i++)
            SetAtteText(m_attrList[i], attrs[i]);
    }

    private void SetAtteText(GameObject a, PItemAttachAttr attr)
    {
        a.SetActive(true);
        Text t = a.transform.GetComponent<Text>();
        Text v = a.transform.GetComponent<Text>("value");
        var vaule = Math.Round(attr.value, digit, MidpointRounding.AwayFromZero);
        Util.SetText(t, ConfigText.GetDefalutString(TextForMatType.AllAttributeText, attr.id));
        string s = GeneralConfigInfo.IsPercentAttribute(attr.id) ? attr.value.ToString("p") : vaule.ToString();
        Util.SetText(v, 32, 3, s);
    }

    private List<PItemAttachAttr> GetAttributePreview(PItem data)
    {
        List<PItemAttachAttr> list = new List<PItemAttachAttr>();

        if (data.GetIntentyLevel() > 0 && !data.InvalidGrowAttr())
        {
            PItemAttachAttr[] attris = data.growAttr.equipAttr.fixedAttrs;
            foreach (var attr in attris)
            {
                list.Add(attr);
            }
        }
        else
        {
            PropItemInfo info = data.GetPropItem();
            if (info && info.attributes != null)
            {
                foreach (var a in info.attributes)
                {
                    PItemAttachAttr attr = PacketObject.Create<PItemAttachAttr>();
                    attr.id = a.id;
                    attr.value = a.value;
                    list.Add(attr);
                }
            }
        }

        var nowLevel = item.growAttr.equipAttr.level;
        if (nowLevel > 0)
        {
            var value = moduleForging.Insoul_Info[nowLevel].attribute;
            PItemAttachAttr a = PacketObject.Create<PItemAttachAttr>();
            a.id = 18;
            a.value = value;
            list.Add(a);
        }
        return list;
    }

    #endregion

    //套装
    private void SetSuitAttr(PItem rItem)
    {
        rItem = moduleEquip.GetProp(rItem.itemId);
        bool have = rItem.growAttr.suitId > 0;
        m_suitHave.gameObject.SetActive(have);
        m_suitNo.gameObject.SetActive(!have);
        if (have)
            m_suitProperty.Init(rItem.growAttr.suitId, moduleEquip.GetSuitNumber(rItem.growAttr.suitId), moduleEquip.IsDressOn(rItem));
    }

    //器灵
    private void SetSprite()
    {
        var rItem = moduleEquip.GetProp(item.itemId);
        soulEntry.Init(rItem.growAttr.soulAttr.attr);
    }

    private void SetParent(Transform parent, GameObject child, int count, List<GameObject> objList)
    {
        foreach (Transform item in parent)
        {
            item.gameObject.SetActive(false);
        }
        if (count <= 0) return;
        for (int i = 0; i < count; i++)
        {
            Transform t = parent.AddNewChild(child);
            t.gameObject.SetActive(false);
            objList.Add(t.gameObject);
        }
    }

    private void _ME(ModuleEvent<Module_Furnace> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Furnace.ResponseSublimationClear:
                SetSuitAttr(item);
                m_sublimaHint.gameObject.SetActive(moduleEquip.CheckSublimation(item));
                break;
        }
    }
    void _ME(ModuleEvent<Module_Player> e)
    {
        if (e.moduleEvent == Module_Player.EventCurrencyChanged && actived)
        {
            var type = (CurrencySubType)e.param1;
            if (type == CurrencySubType.Gold)
                m_soulHint.gameObject.SetActive(moduleEquip.CheckInSoul(item));
        }
    }
    void _ME(ModuleEvent<Module_Equip> e)
    {
        if (e.moduleEvent == Module_Equip.EventRefreshCangku && actived)
            m_soulHint.gameObject.SetActive(moduleEquip.CheckInSoul(item));
    }
}
