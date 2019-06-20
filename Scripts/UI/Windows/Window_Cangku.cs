/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-01-19
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Cangku : Window
{
    ToggleGroup grounp;
    List<Transform> m_hibtList = new List<Transform>();

    private Button m_sortBtn;
    static Toggle m_decomBtn;

    private Image nothing;
    private ScrollView m_view;
    private RectTransform m_normal;
    DataSource<PItem> m_dataInfo;
    List<Toggle> m_toggles = new List<Toggle>();

    private DecomposePackage decomposePackage;
    
    protected override void OnOpen()
    {
        nothing = GetComponent<Image>("nothing");
        m_normal = GetComponent<RectTransform>("normal");
        m_view = GetComponent<ScrollView>("normal/props");

        grounp = GetComponent<ToggleGroup>("checkBox");
        GetAllToggle();
        m_decomBtn = GetComponent<Toggle>("disintegration");
        m_sortBtn = GetComponent<Button>("order");
        decomposePackage = SubWindowBase.CreateSubWindow<DecomposePackage, Window_Cangku>(this, GetComponent<Transform>("disassembly")?.gameObject);
        m_decomBtn.onValueChanged.AddListener(delegate
        {
            //分解
            if (m_decomBtn.isOn)
            {
                decomposePackage.Initialize();
                moduleCangku.chickType = WareType.None;
            }
        });
        m_sortBtn.onClick.AddListener(delegate
        {
            if (decomposePackage.Root.activeInHierarchy)
                decomposePackage.RevertOrder();
            else if (moduleCangku.sortType == 0) moduleCangku.sortType = 1;
            else if (moduleCangku.sortType == 1) moduleCangku.sortType = 0;

            RefreshInfo(false);
        });
        
        SetDataInfo();
        InitText();
    }

    private void GetAllToggle()
    {
        m_toggles.Clear();
        foreach (Transform item in grounp.transform)
        {
            Toggle t = item.gameObject.GetComponent<Toggle>();
            m_toggles.Add(t);
        }
    }

    private void InitText()
    {
        Util.SetText(GetComponent<Text>("disassembly/left/nothing/Text"), 205, 24);
        Util.SetText(GetComponent<Text>("bg/title"), 205, 9);

        Util.SetText(GetComponent<Text>("checkBox/items/Text"), 205, 16);
        Util.SetText(GetComponent<Text>("checkBox/items/xz/items_text"), 205, 16);
        Util.SetText(GetComponent<Text>("checkBox/equipment/Text"), 205, 17);
        Util.SetText(GetComponent<Text>("checkBox/equipment/xz/equipment_text"), 205, 17);
        Util.SetText(GetComponent<Text>("checkBox/rune/Text"), 205, 18);
        Util.SetText(GetComponent<Text>("checkBox/rune/xz/rune_text"), 205, 18);
        Util.SetText(GetComponent<Text>("checkBox/material/Text"), 205, 19);
        Util.SetText(GetComponent<Text>("checkBox/material/xz/material_text"), 205, 19);
        Util.SetText(GetComponent<Text>("checkBox/splinter/Text"), 205, 20);
        Util.SetText(GetComponent<Text>("checkBox/splinter/xz/splinter_text"), 205, 20);
        Util.SetText(GetComponent<Text>("checkBox/order/Text"), 205, 21);
        Util.SetText(GetComponent<Text>("disintegration/Text"), 205, 22);
        Util.SetText(GetComponent<Text>("nothing/Text"), 205, 12);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();
        m_normal.gameObject.SetActive(false);

        if (m_subTypeLock != -1 && moduleCangku.UseSubType)
        {
            if (m_subTypeLock == 6) moduleCangku.chickType = WareType.None ;
            else moduleCangku.chickType = (WareType)m_subTypeLock;
        }
        if (moduleCangku.chickType == WareType.None) m_decomBtn.isOn = true;
        else RefreshInfo();
        
        moduleCangku.UseSubType = true;
        m_view.progress = 0;

        if (moduleGlobal.targetMatrial.isProcess)
        {
            if (moduleGlobal.targetMatrial.isFinish)
            {
               moduleGlobal.UpdateGlobalTip((PItem)moduleGlobal.targetMatrial.data);
            }
            moduleGlobal.targetMatrial.Clear();
        }

    }
    protected override void OnHide(bool forward)
    {
        moduleCangku.UseSubType = false;
    }
    protected override void OnClose()
    {
        base.OnClose();
        decomposePackage.UnInitialize();
        decomposePackage?.Destroy();
    }

    private void SetDataInfo()
    {
        m_dataInfo = new DataSource<PItem>(null, m_view, SetItemInfo);
        m_hibtList.Clear();
        if (m_toggles != null && m_toggles.Count > 0)
        {
            for (int i = 0; i < m_toggles.Count; i++)
            {
                m_toggles[i].onValueChanged.RemoveAllListeners();
                m_toggles[i].onValueChanged.AddListener(OnValueChange);
            }
        }
    }

    private void OnValueChange(bool isTrue)
    {
        if (!isTrue) return;

        if (m_toggles != null && m_toggles.Count > 0)
        {
            for (int i = 0; i < m_toggles.Count; i++)
            {
                if (m_toggles[i] && !m_toggles[i].gameObject.activeInHierarchy) continue;

                decomposePackage.ClearControl();
                WareType type = (WareType)(i + 1);
                if (m_toggles[i].isOn && type != moduleCangku.chickType)
                {
                    m_view.progress = 0;
                    moduleCangku.RemveNewItem(moduleCangku.chickType);
                    moduleCangku.chickType = type;
                    RefreshInfo();
                }
            }
        }
    }

    private void SetItemInfo(RectTransform rt, PItem info)
    {
        BindItemInfo(rt, info);
    }

    public static void BindItemInfo(RectTransform rt, PItem info)
    {
        if (info == null) return;
        var prop = ConfigManager.Get<PropItemInfo>(info.itemTypeId);
        if (prop == null) return;

        int star = 0;
        int level = 0;
        bool wearShow = false;
        bool maxShow = false;

        RectTransform normal = rt.Find("0").GetComponent<RectTransform>();
        RectTransform rune = rt.Find("1").GetComponent<RectTransform>();
        var ctNormal = normal.Find("numberdi/count").GetComponent<Text>();
        var ctRune = rune.Find("numberdi/count").GetComponent<Text>();

        if (prop.itemType == PropType.Rune)
        {
            if (info.growAttr != null)
            {
                level = info.growAttr.runeAttr.level;
                star = info.growAttr.runeAttr.star;
                maxShow = moduleRune.IsMaxLv(info.growAttr.runeAttr.level);
                wearShow = moduleRune.currentEquip.Exists(a => a.itemId == info.itemId);
            }

            Util.SetItemInfo(rune, prop, level, 0, true, star);
            ctRune.gameObject.SetActive(false);
        }
        else if ((prop.attributes != null && prop.attributes.Length > 0) || prop.itemType == PropType.Weapon)
        {
            star = prop.quality;
            level = info.GetIntentyLevel();

            var t = moduleEquip.GetPeiviewEuipeTypeByItem(info);
            if (t == PreviewEquipType.None || t == PreviewEquipType.Enchant) maxShow = true;
            wearShow = moduleEquip.currentDressClothes.Exists(a => a.itemId == info.itemId);
            if (moduleEquip.weapon != null && moduleEquip.offWeapon != null)
                if (info.itemId == moduleEquip.weapon.itemId || info.itemId == moduleEquip.offWeapon.itemId)
                    wearShow = true;

            Util.SetItemInfo(normal, prop, level, 0, true, star);
            ctNormal.gameObject.SetActive(false);
        }
        else
        {
            star = prop.quality;
            Util.SetItemInfo(normal, prop, 0, (int)info.num, false);
            ctNormal.gameObject.SetActive(true);
        }

        var rtChild = normal;
        normal.gameObject.SetActive(true);
        rune.gameObject.SetActive(false);
        if (prop.itemType == PropType.Rune)
        {
            rtChild = rune;
            rune.gameObject.SetActive(true);
            normal.gameObject.SetActive(false);
        }

        var wear = rtChild.Find("get"); //是否佩戴
        var hint = rtChild.Find("mark"); //是否可合成
        var mews = rtChild.Find("new");
        var max = rtChild.Find("levelmax"); //最高级

        Image locks = rtChild.Find("lock").GetComponent<Image>();
        locks.gameObject.SetActive(info.isLock == 1);
        wear.SafeSetActive(wearShow);

        var equipText = rtChild.Find("get/Text")?.GetComponent<Text>();
        if (equipText) equipText.color = moduleRune.GetCurQualityColor(GeneralConfigInfo.defaultConfig.qualitys, star);
        Util.SetText(equipText, (int)TextForMatType.RuneUIText, 48);
        max.SafeSetActive(maxShow);
        mews.SafeSetActive(false);
        hint.SafeSetActive(false);
        ctNormal.color = GeneralConfigInfo.defaultConfig.WareHouse;
        if ((WareType)prop.lableType == WareType.Debris)
        {
            Compound compose = ConfigManager.Get<Compound>(prop.compose);//合成/分解信息
            if (compose == null) return;
            var fenmu = compose.sourceNum;
            var fenzi = info.num;
            var str = fenzi.ToString() + "/" + fenmu.ToString();
            Util.SetText(ctNormal, str);
            if (fenzi < fenmu) Util.SetText(ctNormal, GeneralConfigInfo.GetNoEnoughColorString(str));

        }
        if (!m_decomBtn.isOn)
        {
            moduleCangku.GetCanCompse();
            var cPos = moduleCangku.AllCompse.Find(a => a.itemId == info.itemId);
            hint.SafeSetActive(cPos != null);
            var isNew = moduleCangku.newProp.Find(a => a.itemId == info.itemId);
            mews.SafeSetActive(isNew != null);
            var bt = rtChild.gameObject.GetComponentDefault<Button>();
            bt.onClick.RemoveAllListeners();
            bt.onClick.AddListener(delegate
            {
                SetItemClick(bt, info);
            });
        }
    }
    
    private static void SetItemClick(Button rtChild, PItem info)
    {
        GameObject mews = rtChild.transform.Find("new").gameObject;
        mews.gameObject.SetActive(false);
        moduleCangku.RemveNewItem(info.itemId);

        var prop = ConfigManager.Get<PropItemInfo>(info.itemTypeId);
        if (prop == null) return;

        if (prop.itemType == PropType.Rune)
        {
            if (!moduleGuide.IsActiveFunction(HomeIcons.Rune)) return;
            
            SetWindowParam<Window_RuneMain>(info, 1);
            ShowAsync<Window_RuneMain>();
        }
        else if ((prop.itemType == PropType.FashionCloth && prop.subType == 8) || prop.itemType == PropType.Weapon)
        {
            moduleCangku.m_detailItem = info;
            ShowAsync<Window_Equipinfo>();
        }
        else
        {
            if ((WareType)prop.lableType == WareType.Debris)
            {
                Compound compose = ConfigManager.Get<Compound>(prop.compose);//合成/分解信息
                if (compose == null) return;
                moduleGlobal.SetTargetMatrial(info.itemTypeId, compose.sourceNum, info, false);
            }
            else moduleGlobal.UpdateGlobalTip(info);
        }
    }

    private void RefreshInfo(bool check = true)
    {
        var index = (int)moduleCangku.chickType - 1;
        if (index == -1 || index >= m_toggles.Count) return;

        m_normal.gameObject.SetActive(true);
        nothing.gameObject.SetActive(moduleCangku.NothingShow(moduleCangku.chickType));
        var list = moduleCangku.WareLableInfo(moduleCangku.chickType, moduleCangku.sortType);
        m_dataInfo.SetItems(list);

        m_toggles[index].isOn = true;

        if (moduleCangku.chickType == WareType.Debris)
        {
            //合成
            moduleCangku.GetCanCompse();
            moduleCangku.CanCompose.Clear();
            moduleCangku.CanCompose.AddRange(moduleCangku.AllCompse);
        }
        if (check) SetToggleHint();
    }
    
    private void SetToggleHint()
    {
        if (m_toggles == null) return;
        for (int i = 0; i < m_toggles.Count; i++)
        {
            Transform hint = m_toggles[i].transform.Find("new");
            hint.gameObject.SetActive(moduleCangku.TypeHint((WareType)(i + 1)));

            if (m_toggles[i].isOn) hint.gameObject.SetActive(false);
            if ((WareType)(i + 1) == WareType.Debris && moduleCangku.ShowComposHint())
                hint.gameObject.SetActive(true);
        }
    }

    protected override void OnReturn()
    {
        moduleHome.UpdateIconState(HomeIcons.Bag, false);
        moduleCangku.RemveNewItem(moduleCangku.chickType);
        if (grounp) grounp.SetAllTogglesOff();
        base.OnReturn();
    }

    void _ME(ModuleEvent<Module_Equip> e)
    {
        if (!actived) return;
        switch (e.moduleEvent)
        {
            case Module_Equip.EvenSynthesisSucced:
                var compose = e.msg as ScRoleItemCompose;
                if (compose != null && compose.items != null && compose.items.Length > 0)
                {
                    Window_ItemTip.Show(ConfigText.GetDefalutString(TextForMatType.WarehouseUiText, 23), compose.items);
                }
                RefreshInfo();
                break;
            case Module_Equip.EventRefreshCangku:
                RefreshInfo();
                SetToggleHint();
                break;
            default:
                break;
        }
    }

    void _ME(ModuleEvent<Module_Pet> e)
    {
        if (!actived) return;
        if (e.moduleEvent == Module_Pet.EventGetNewPet)
        {
            moduleCangku.RemveNewItem(moduleCangku.chickType);

            var canshow = moduleGuide.IsActiveFunction(HomeIcons.Pet);
            if (!canshow) return;

            //跳转到宠物界面播解锁动画
            // @TODO: Check with new Window System
            //Window.PopTo<Window_Home>();
            var petId = ((PetInfo)e.param1)?.ID ?? 0;
            ShowAsync<Window_Sprite>(null, w => ((Window_Sprite)w)?.SetSelect(petId));
        }
    }

    void _ME(ModuleEvent<Module_Player> e)
    {
        if (actived && e.moduleEvent == Module_Player.EventUseProp)
        {
            if (moduleGlobal.currentOpenInfo.itemType == PropType.UseableProp &&
                moduleGlobal.currentOpenInfo.subType == (int)UseablePropSubType.PropBag ||
                moduleGlobal.currentOpenInfo.subType == (int)UseablePropSubType.CoinBag)
            {
                var sucess = e.msg as ScRoleUseProp;
                if (sucess != null)
                {
                    List<PItem2> items = new List<PItem2>();
                    items.AddRange(sucess.rewards);
                    if (items.Count < 1) return;
                    string str = ConfigText.GetDefalutString(TextForMatType.SettlementUIText, 1);
                    Window_ItemTip.Show(str, items);
                }
            }
        }
    }

    void _ME(ModuleEvent<Module_Cangku> e)
    {
        if (e.moduleEvent == Module_Cangku.EventCangkuItemLock && actived)
        {
            var itemId = Util.Parse<ulong>(e.param1.ToString());
            var item = moduleCangku.GetItemByGUID(itemId);
            WareType type = moduleCangku.GetWareType(item);
            if (moduleCangku.chickType != type) return;
            if (moduleCangku.chickType != WareType.Rune) return;
            for (int i = 0; i < moduleCangku.RuneItemList.Count; i++)
            {
                if (itemId == moduleCangku.RuneItemList[i].itemId)
                {
                    moduleCangku.RuneItemList[i].isLock = item.isLock;
                    m_dataInfo.UpdateItem(i);
                    return;
                }
            }
        }
    }

}

public class DecomposePackage : SubWindowBase<Window_Cangku>
{
    public ScrollView scrollView;
    public ScrollView previewScrollView;
    public Button decomposeButton;
    public RectTransform propPlane;
    private RectTransform nothing;
    public DataSource<PItem> dataSource;
    public DataSource<ItemPair> previewDataSource;

    private bool descendingOrder;

    private readonly Dictionary<ulong, DecomposePair> decomposeDict = new Dictionary<ulong, DecomposePair>();
    private readonly Dictionary<int, ItemPair> previewDict = new Dictionary<int, ItemPair>();

    public void SetOrder(bool order)
    {
        if (order == descendingOrder)
            return;
        descendingOrder = order;
        Sort();
    }

    public void RevertOrder()
    {
        descendingOrder = !descendingOrder;
        Sort();
    }

    protected override void InitComponent()
    {
        base.InitComponent();
        scrollView = WindowCache.GetComponent<ScrollView>("disassembly/left/props");
        previewScrollView = WindowCache.GetComponent<ScrollView>("disassembly/right/props");
        decomposeButton = WindowCache.GetComponent<Button>("disassembly/disassembly_Btn");
        nothing = WindowCache.GetComponent<RectTransform>("disassembly/left/nothing");
        propPlane = WindowCache.GetComponent<RectTransform>("disassembly/left/props");
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
        {
            Sort();
            return false;
        }
        descendingOrder = false;
        ClearControl();

        decomposeButton?.onClick.AddListener(OnDecompose);
        var list = moduleFurnace.GetAllItems(FurnaceType.Decompose, item => !moduleEquip.IsDressOn(item) && item.isLock == 0);
        list = moduleCangku.SortQuity(list, descendingOrder ? 0 : 1);
        dataSource = new DataSource<PItem>(list, scrollView, OnSetData, null);
        nothing.SafeSetActive(list.Count == 0);
        previewDataSource = new DataSource<ItemPair>(null, previewScrollView, OnPreviewSetData, OnPreviewItemClick);
        return true;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;
        decomposeButton?.onClick.RemoveAllListeners();
        return true;
    }

    public void Sort()
    {
        if (!Root.activeInHierarchy)
            return;

        var list = moduleFurnace.GetAllItems(FurnaceType.Decompose, item => !moduleEquip.IsDressOn(item) && item.isLock == 0);
        list = moduleCangku.SortQuity(list, descendingOrder ? 0 : 1);
        dataSource.SetItems(list);
        nothing.SafeSetActive(list.Count == 0);
    }

    private void OnPreviewItemClick(RectTransform node, ItemPair data)
    {
        moduleGlobal.UpdateGlobalTip((ushort)data.itemId, true);
    }

    private void OnPreviewSetData(RectTransform node, ItemPair data)
    {
        Util.SetItemInfo(node, ConfigManager.Get<PropItemInfo>(data.itemId));
        Util.SetText(node.GetComponent<Text>("numberdi/count"), data.count.ToString());
    }

    private void OnSetData(RectTransform node, PItem data)
    {
        if (data == null) return;
        var prop = ConfigManager.Get<PropItemInfo>(data.itemTypeId);
        if (prop == null) return;

        var rt = node.Find("0");
        if (prop.itemType == PropType.Rune) rt = node.Find("1");
        
        var item = rt.GetComponentDefault<DecomposeItem>();

        if (decomposeDict.ContainsKey(data.itemId))
            item.Init(data, decomposeDict[data.itemId].count);
        else
            item.Init(data, 0);

        item.onChange = (pair, change) =>
        {
            var itemId = pair.item.itemId;
            if (pair.count == 0)
            {
                decomposeDict.Remove(itemId);
            }
            else
            {
                if (!decomposeDict.ContainsKey(itemId))
                {
                    decomposeDict.Add(itemId, pair);
                }
                else
                {
                    decomposeDict[itemId].count = pair.count;
                }
            }

            RefreshMatrial(moduleEquip.CalcDecomposeMatrial(pair.item, change));
        };
    }

    private void RefreshMatrial(List<ItemPair> items)
    {
        foreach (var item in items)
        {
            if (previewDict.ContainsKey(item.itemId))
                previewDict[item.itemId].count += item.count;
            else
                previewDict.Add(item.itemId, item);

            if (previewDict[item.itemId].count == 0)
                previewDict.Remove(item.itemId);
            else if (previewDict[item.itemId].count <= 0)
                Logger.LogError("there is some Error for decompose matrials");
        }
        var list = new List<ItemPair>(previewDict.Values);
        list.Sort((a, b) =>
        {
            var propA = ConfigManager.Get<PropItemInfo>(a.itemId);
            var propB = ConfigManager.Get<PropItemInfo>(b.itemId);
            if (propA == null)
            {
                Logger.LogError($"分解表中的预览道具ID={a.itemId}无效。道具表中未查到有此道具!");
                return 0;
            }
            if (propB == null)
            {
                Logger.LogError($"分解表中的预览道具ID={b.itemId}无效。道具表中未查到有此道具!");
                return 0;
            }
            if (propA.quality == propB.quality)
                return -propA.swallowedExpr.CompareTo(propB.swallowedExpr);
            return -propA.quality.CompareTo(propB.quality);
        });
        previewDataSource.SetItems(list);
    }

    private void OnDecompose()
    {
        if (decomposeDict.Count == 0)
        {
            moduleGlobal.ShowMessage((int)TextForMatType.DecomposeUI, 4);
            return;
        }

        var num = 0;
        foreach (var kv in decomposeDict)
        {
            num += kv.Value.count;
        }
        Window_Alert.ShowAlertDefalut(Util.Format(ConfigText.GetDefalutString(TextForMatType.DecomposeUI, 2), num), () =>
        {
            moduleFurnace.RequestComposeItem(new List<DecomposePair>(decomposeDict.Values));
        },
        null,
        ConfigText.GetDefalutString(TextForMatType.DecomposeUI, 0),
        ConfigText.GetDefalutString(TextForMatType.DecomposeUI, 1));
    }

    private void _ME(ModuleEvent<Module_Furnace> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Furnace.ResponseDecomposeItem:
                ResponseDecomposeItem(e.msg as ScItemDecompose);
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Equip> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Equip.EventBagInfo:
            case Module_Equip.EventItemDataChange:
            case Module_Equip.EventUpdateBagProp:
                Sort();
                break;
        }
    }

    private void ResponseDecomposeItem(ScItemDecompose msg)
    {
        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9753, msg.result);
            return;
        }

        Window_ItemTip.Show(ConfigText.GetDefalutString((int)TextForMatType.DecomposeUI, 3), msg.items);
        ClearControl();
        dataSource.UpdateItems();
    }

    public void ClearControl()
    {
        previewDataSource?.SetItems(null);
        decomposeDict?.Clear();
        previewDict?.Clear();
    }
}