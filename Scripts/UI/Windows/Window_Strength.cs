/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-10-31
 * 
 ***************************************************************************************************/

using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Window_Strength : Window
{
    private PItem m_data;
    private IntentifyPanel m_intentifyPanel;
    private IntentySuccess m_intentSuccessPanel;
    private NpcMono m_npcMono;

    /// <summary>
    /// Called when window added to level
    /// </summary>
    protected override void OnOpen()
    {
        SelectIntentyPropPanel selectPanel = new SelectIntentyPropPanel(transform.Find("tip"));
        m_intentifyPanel = new IntentifyPanel(transform, selectPanel);
        m_intentSuccessPanel = new IntentySuccess(transform.Find("success_Panel"));
        m_intentSuccessPanel.beforePanelVisible = OnSuccessVisibleChangeBefore;
        m_intentSuccessPanel.afterPanelVisible = OnSuccessVisibleChangeAfter;
        m_npcMono = GetComponent<NpcMono>("npcInfo");

        enableUpdate = true;
        InitializeText();
    }

    private void InitializeText()
    {
        var t = ConfigManager.Get<ConfigText>((int)TextForMatType.EquipIntentyUI);
        if (!t) return;

        Util.SetText(GetComponent<Text>("decoration1/biaoti/text1"), (int)TextForMatType.EquipIntentyUI, 0);
        Util.SetText(GetComponent<Text>("decoration1/biaoti/text2"), (int)TextForMatType.EquipIntentyUI, 1);
        Util.SetText(GetComponent<Text>("decoration1/biaoti/english"), (int)TextForMatType.EquipIntentyUI, 2);

        Util.SetText(GetComponent<Text>("middle/level"), (int)TextForMatType.EquipIntentyUI, 4);
        
        Util.SetText(GetComponent<Text>("intentify_Panel/comsume_1/consume_txt_02"), (int)TextForMatType.EquipIntentyUI, 5);
        Util.SetText(GetComponent<Text>("intentify_Panel/exp/total_tip"), (int)TextForMatType.EquipIntentyUI, 6);
        Util.SetText(GetComponent<Text>("intentify_Panel/btn_strenthen_1/Text"), (int)TextForMatType.EquipIntentyUI, 7);
        
        Util.SetText(GetComponent<Text>("success_Panel/title_txt"), (int)TextForMatType.EquipIntentyUI, 8);

        Util.SetText(GetComponent<Text>("tip/equipinfo"), (int)TextForMatType.EquipIntentyUI,10);
        Util.SetText(GetComponent<Text>("tip/levelchange"), (int)TextForMatType.EquipIntentyUI, 11);
        Util.SetText(GetComponent<Text>("tip/confirm_btn/Text"), (int)TextForMatType.EquipIntentyUI, 13);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();

        m_data = moduleEquip.operateItem;

        if (m_data == null) Logger.LogError("cannot finded a valid opreate pitem at Window_Strength :: OnBecameVisible");
        else m_intentifyPanel.RefreshPanel(m_data);
        m_intentSuccessPanel?.SetPanelVisible(false);
    }

    private void OnSuccessVisibleChangeBefore(bool visible)
    {
        if (visible)
        {
            moduleHome.HideOthers();
            //打开成功面板之前要关闭外层npc显示，之后重新打开是为了保证npc位置被刷新
            m_npcMono.SafeSetActive(false);
        }
    }

    private void OnSuccessVisibleChangeAfter(bool visible)
    {
        //在成功面板关闭之后再打开npc显示，避免mainnode被冲突掉
        if (!visible) m_npcMono.SafeSetActive(true);
    }

    public override void OnUpdate(int diff)
    {
        base.OnUpdate(diff);
        if (m_intentSuccessPanel.enable) m_intentSuccessPanel.Update();
    }

    private void _ME(ModuleEvent<Module_Player> e)
    {
        if (e.moduleEvent == Module_Player.EventCurrencyChanged)
        {
            var t = (CurrencySubType)e.param1;
            if (t == CurrencySubType.Gold && m_intentifyPanel != null && m_intentifyPanel.enable)
            {
                m_intentifyPanel.RefreshTotalExp();
            }
        }
    }

    private void _ME(ModuleEvent<Module_Equip> e)
    {
        if (e.moduleEvent == Module_Equip.EventOperateSuccess && actived)
        {
            EnumOperateEquipType t = (EnumOperateEquipType)e.param1;
            if (t == EnumOperateEquipType.Intenty) OnIntentySuccess((PItem)e.param2,(PItem)e.param3);
        } 
        else if(e.moduleEvent == Module_Equip.EventUpdateBagProp && actived)
        {
            if (m_intentifyPanel != null && m_intentifyPanel.enable) m_intentifyPanel.RefreshSelf();
        }
    }

    private void OnIntentySuccess(PItem tar,PItem lastCache)
    {
        if(tar == null || lastCache == null)
        {
            Logger.LogError("intenty success callback error, tar is {0} last cache is {1}",tar == null ? "error data" : "valid", lastCache == null ? "error data" : "valid");
            return;
        }

        m_intentSuccessPanel.RefreshPanel(tar,lastCache);
        m_intentifyPanel.RefreshPanel(m_data);
        m_intentifyPanel.sendStreng = true;
    }
}

#region custom class

public sealed class IntentifyPanel : PreviewBasePanel
{
    public IntentifyPanel(Transform trans, SelectIntentyPropPanel panel) : base(trans)
    {
        m_selectPanel = panel;
        m_selectPanel.onConfirmClick = AfterSelectItem;
    }

    private AttributeItem m_levelItem;
    private Text m_expText;
    private List<Image> m_expBars;

    private List<AttributeItem> m_attriList = new List<AttributeItem>();
    private string[] m_attriNames = new string[] { "attribute_01", "attribute_02", "attribute_03" };

    private Button m_intenBtn;
    private Text m_intenCostText;
    private Text m_swallowText;

    private bool m_maxLevel = false;

    private IntentifyEquipInfo m_previewIntentifyInfo;
    private IntentifyEquipInfo m_maxLimitIntentifyInfo;
    private Dictionary<PItem, int> m_swallowMats = new Dictionary<PItem, int>();
    private int m_swallowExp;
    private SelectIntentyPropPanel m_selectPanel;
    public bool sendStreng;

    public override void InitComponent()
    {
        base.InitComponent();

        sendStreng = true;
        Transform t = transform.Find("middle/progressbar");
        m_expBars = Util.GetExpBars(t);
        m_expText = transform.GetComponent<Text>("middle/level_bar_text");

        t = transform.Find("middle/level");
        m_levelItem = new AttributeItem(t);
        m_intenBtn = transform.GetComponent<Button>("intentify_Panel/btn_strenthen_1");
        m_intenCostText = transform.GetComponent<Text>("intentify_Panel/comsume_1/consume_txt");
        m_swallowText = transform.GetComponent<Text>("intentify_Panel/exp/total_swallow");

        t = transform.Find("material_list");
        for (int i = 0; i < t.childCount; i++)
        {
            m_matList.Add(t.GetChild(i).GetComponentDefault<PreviewMatItem>());
        }

        m_attriList.Clear();
        foreach (var item in m_attriNames)
        {
            m_attriList.Add(new AttributeItem(transform.Find(Util.Format("top/{0}", item))));
        }
        m_intenBtn.onClick.RemoveAllListeners();
        m_intenBtn.onClick.AddListener(OnIntentClick);
    }
    
    protected override void InitPanel()
    {
        foreach (var item in m_matList)
        {
            item.InitSelectableExpItem();
        }
        ResetAllAttriItems();
        m_swallowMats.Clear();
        m_swallowExp = 0;
        m_intenCostText.text = "0";
    }

    public override void RefreshPanel(PItem item, EquipType type)
    {
        base.RefreshPanel(item, type);

        int intentLevel = data.GetIntentyLevel();
        Util.SetText(m_levelText,(int)TextForMatType.EquipIntentyUI, 3, intentLevel);
        m_levelText.SafeSetActive(intentLevel > 0);

        m_levelItem.RefreshDetail(intentLevel, intentLevel, false);
        RefreshMatItem(moduleEquip.GetBagProps(PropType.IntentyProp), OnSelectItemClick, OnDelItemClick);
        PreparePrevirewData();
        //fill mats of one level
        FillMaterails();
        RefreshAfterOperate();
    }

    public void RefreshSelf()
    {
        if(data != null)
        {
            var type = Module_Equip.GetEquipTypeByItem(data);
            RefreshPanel(data, type);
        }
    }

    private void PreparePrevirewData()
    {
        int maxlevel = moduleEquip.GetMaxIntenLevel(equipType);
        m_maxLevel = data.GetIntentyLevel() >= maxlevel;
        m_maxLimitIntentifyInfo = moduleEquip.GetLimitIntenInfo(equipType, data.GetIntentyLevel(), data.HasEvolved());
        m_previewIntentifyInfo = null;
    }

    private void FillMaterails()
    {
        if (m_maxLimitIntentifyInfo == null) return;

        int needExp = m_maxLimitIntentifyInfo.exp - data.GetCurrentTotalExp();
        m_swallowMats = ExpUtil.GetValidPItems(moduleEquip.GetBagProps((PropType.IntentyProp)), (uint)needExp);
    }

    private void RefreshAfterOperate()
    {
        //must before CheckCanIntentify
        RefreshSwallowDisplay();
        RefreshTotalExp();
        RefreshAttributePreview();
        RefreshLevel();
        RefreshBar();
    }

    private void RefreshSwallowDisplay()
    {
        foreach (var item in m_matList)
        {
            item.InitSelectableExpItem();
        }

        m_swallowExp = 0;
        int index = 0;
        foreach (var item in m_swallowMats)
        {
            if (index >= m_matList.Count) break;

            m_matList[index].RefreshSelectableExpItem(item.Key, item.Value);
            var info = item.Key?.GetPropItem();
            m_swallowExp += item.Value * (info == null ? 0 : (int)info.swallowedExpr);
            index++;
        }
    }

    private void RefreshBar()
    {
        int left = data.GetCurrentLevelExp() + m_swallowExp;
        int right = 0;
        IntentifyEquipInfo next = moduleEquip.GetIntentifyInfo(equipType, data.GetIntentyLevel() + 1);
        IntentifyEquipInfo current = moduleEquip.GetIntentifyInfo(equipType, data.GetIntentyLevel());

        if (next == null)
        {
            next = moduleEquip.GetIntentifyInfo(equipType, data.GetIntentyLevel() - 1);
            right = current.exp - next.exp;
            Util.SetText(m_expText, Util.Format(ConfigText.GetDefalutString(TextForMatType.EquipIntentyUI, 16), left, right));
            Util.SetExpBar(m_expBars, 0, left, right);
        }
        else
        {
            right = next.exp - (current == null ? 0 : current.exp);
            Util.SetText(m_expText, Util.Format(ConfigText.GetDefalutString(TextForMatType.EquipIntentyUI, 16), left, right));
            int level = m_previewIntentifyInfo == null ? 0 : m_previewIntentifyInfo.level - data.GetIntentyLevel();
            if (level < 0) level = 0;
            Util.SetExpBar(m_expBars, level, left, right);
        }
    }

    public void RefreshTotalExp()
    {
        List<int> expDetails = Module_Equip.GetReallyExpDetail(data.GetCurrentTotalExp(), m_swallowExp, m_maxLimitIntentifyInfo);
        int totalExp = expDetails.GetValue(0);
        m_swallowExp = expDetails.GetValue(1);
        m_swallowText.text = m_swallowExp.ToString();
        m_previewIntentifyInfo = moduleEquip.GetLimitIntenLevelByExp(equipType, data.GetIntentyLevel(), totalExp, data.HasEvolved());
        RefreshCoinCost(totalExp);
    }

    #region attribute

    private void ResetAllAttriItems()
    {
        foreach (var item in m_attriList)
        {
            item.SetPanelVisible(false);
        }
    }

    private void RefreshAttributePreview()
    {
        ResetAllAttriItems();
        List<AttributePreviewDetail> attrs = GetAttributePreview();
        for (int i = 0; i < attrs.Count; i++)
        {
            if (i >= m_attriList.Count)
            {
                Logger.LogWarning("config attributes count : {0},prefab item count {1}", attrs.Count, m_attriList.Count);
                break;
            }
            m_attriList[i].RefreshDetail(attrs[i]);
        }
    }

    private List<AttributePreviewDetail> GetAttributePreview()
    {
        var list = new List<AttributePreviewDetail>();
        //add preview attirbute change
        if (m_previewIntentifyInfo != null)
        {
            List<IntentifyEquipInfo> allIntents = moduleEquip.GetGetIntentifyInfos(equipType, data.GetIntentyLevel(), m_previewIntentifyInfo.level);

            foreach (var item in allIntents)
            {
                if (item.attributes == null) continue;
                foreach (var attri in item.attributes)
                {
                    //获取道具的初始属性
                    var originalAttri = PropItemInfo.GetBaseAttribute(itemInfo, attri.id);
                    //计算属性(百分比或者累加)
                    var value = m_previewIntentifyInfo.level == data.GetIntentyLevel() ? 0 : ItemAttachAttr.CalculateAttributeValue(originalAttri, attri);

                    //未包含就把初始值填充进去
                    if (!ContainSameAttirbutePreview(list, attri.id))
                    {
                        double oldValue = data.GetFixedAttirbute(attri.id);
                        if (oldValue <= 0 && value <= 0) continue;

                        var de = new AttributePreviewDetail(attri.id);
                        de.oldValue = oldValue;
                        //must before set newValue
                        de.FixPercentAttribute();
                        de.newValue = de.oldValue + value;
                        list.Add(de);
                    }
                    //已经添加了就开始操作缓存
                    else
                    {
                        var d = list.Find(o => o.id == attri.id);
                        d.newValue += value;
                    }
                }
            }
        }
        //添加初始化属性
        else
        {
            if (data.GetIntentyLevel() > 0 && !data.InvalidGrowAttr())
            {
                PItemAttachAttr[] attris = data.growAttr.equipAttr.fixedAttrs;
                foreach (var item in attris)
                {
                    if (item.type == 2) continue;

                    var de = new AttributePreviewDetail(item.id);
                    de.oldValue = item.value;
                    de.newValue = item.value;
                    de.FixPercentAttribute();
                    list.Add(de);
                }
            }
            else
            {
                PropItemInfo info = data.GetPropItem();
                if (info && info.attributes != null)
                {
                    foreach (var item in info.attributes)
                    {
                        if (item.type == 2) continue;

                        var de = new AttributePreviewDetail(item.id);
                        de.oldValue = item.value;
                        de.newValue = item.value;
                        de.FixPercentAttribute();
                        list.Add(de);
                    }
                }
            }
        }

        return list;
    }
    #endregion

    private void RefreshCoinCost(int totalExp)
    {
        int totalNum = Module_Equip.GetCoinCost(equipType, data.GetCurrentTotalExp(), totalExp, m_maxLimitIntentifyInfo);
        bool moneyEnough = Module_Player.instance.roleInfo.coin > totalNum;
        Util.SetText(m_intenCostText, moneyEnough ? totalNum.ToString() : GeneralConfigInfo.GetNoEnoughColorString(totalNum.ToString()));
        //m_intenBtn.SetInteractable();
        m_intenBtn.interactable = moneyEnough && !m_maxLevel && m_swallowExp > 0 && sendStreng;
    }

    private void RefreshLevel()
    {
        m_levelItem.RefreshDetail(data.GetIntentyLevel(), m_previewIntentifyInfo == null ? data.GetIntentyLevel() : m_previewIntentifyInfo.level, false);
    }

    #region click event
    private void OnIntentClick()
    {
        sendStreng = false;
        moduleEquip.SendIntentiEquip(data, m_swallowMats);
        m_intenBtn.interactable = false;
    }
    #endregion

    #region select panel callback

    private void AfterSelectItem(Dictionary<PItem, int> selectDic)
    {
        m_swallowMats = selectDic;
        RefreshAfterOperate();
    }

    private void OnSelectItemClick(PreviewMatItem item)
    {
        List<PItem> items = moduleEquip.GetBagProps(PropType.IntentyProp);

        //没有道具就弹出警告窗口
        if (items == null || items.Count == 0) Window_Alert.ShowAlert(ConfigText.GetDefalutString(TextForMatType.AlertUIText, 4));
        else m_selectPanel.RefreshPanel(data, equipType, m_swallowMats, items);
    }

    private void OnDelItemClick(PreviewMatItem item)
    {
        if (m_swallowMats.ContainsKey(item.item))
        {
            m_swallowMats.Remove(item.item);
            RefreshAfterOperate();
        }
    }

    #endregion
}

public sealed class SelectIntentyPropPanel : CustomSecondPanel
{
    public SelectIntentyPropPanel(Transform trans) : base(trans) { }

    private DataSource<PItem> m_dataSource;
    private AttributeItem m_levelPreview;
    private List<Image> m_expBars;
    private Text m_expText;
    private Text m_cost;
    private Text m_swallowExpText;
    private Button m_confirmBtn;
    private Button m_exitBtn;
    private Module_Equip moduleEquip;

    private Dictionary<PItem, int> m_matList = new Dictionary<PItem, int>();
    private Dictionary<GameObject, PItem> m_cacheItems = new Dictionary<GameObject, PItem>();
    #region properties
    public PItem data { get; private set; }
    public EquipType equipType { get; private set; }
    public int swallowExp { get; private set; } = 0;
    public int totalExp { get; private set; } = 0;
    private IntentifyEquipInfo m_previewIntentifyInfo;
    private IntentifyEquipInfo m_limitIntentifyInfo;
    #endregion
    public Action<Dictionary<PItem, int>> onConfirmClick { get; set; }

    public override void InitComponent()
    {
        base.InitComponent();

        moduleEquip = Module_Equip.instance;
        m_levelPreview = new AttributeItem(transform.Find("levelchange"));
        m_expBars = Util.GetExpBars(transform.Find("progressbar"));
        m_expText = transform.GetComponent<Text>("level_bar_text");
        m_cost = transform.GetComponent<Text>("consume_txt");
        m_swallowExpText = transform.GetComponent<Text>("exp_txt");
        m_confirmBtn = transform.GetComponent<Button>("confirm_btn");
        m_exitBtn = transform.GetComponent<Button>("close_button");
        ScrollView s = transform.GetComponent<ScrollView>("scrollView");
        m_dataSource = new DataSource<PItem>(new List<PItem>(), s, RefreshItem, OnItemClick);
    }

    public void RefreshPanel(PItem data, EquipType type, Dictionary<PItem, int> currentSelect, List<PItem> items)
    {
        this.data = data;
        equipType = type;

        SetPanelVisible(true);
        InitSelectData(currentSelect, items);
        m_dataSource?.SetItems(items);
        m_dataSource?.UpdateItems();
        m_limitIntentifyInfo = moduleEquip.GetLimitIntenInfo(equipType, data.GetIntentyLevel(), data.HasEvolved());
        RefreshDetail();
    }

    private void InitSelectData(Dictionary<PItem, int> currentSelect, List<PItem> items)
    {
        m_matList.Clear();
        int count = 0;
        foreach (var item in items)
        {
            count = currentSelect.ContainsKey(item) ? currentSelect[item] : 0;
            m_matList.Add(item, count);
        }
    }

    private void RefreshDetail()
    {
        RefreshSwallowDisplay();
        List<int> expDetails = Module_Equip.GetReallyExpDetail(data.GetCurrentTotalExp(), swallowExp, m_limitIntentifyInfo);
        totalExp = expDetails.GetValue(0);
        swallowExp = expDetails.GetValue(1);
        Util.SetText(m_swallowExpText, Util.Format(ConfigText.GetDefalutString(TextForMatType.EquipIntentyUI, 15), swallowExp));
        m_previewIntentifyInfo = moduleEquip.GetLimitIntenLevelByExp(equipType, data.GetIntentyLevel(), totalExp, data.HasEvolved());
        int level = data.GetIntentyLevel();
        int nextLevel = m_previewIntentifyInfo == null ? level : m_previewIntentifyInfo.level;
        m_levelPreview.RefreshDetail(level, nextLevel, false);
        RefreshBar();
        RefreshCoinCost(totalExp);
    }

    private void RefreshSwallowDisplay()
    {
        swallowExp = 0;
        foreach (var mat in m_matList)
        {
            if (mat.Value <= 0) continue;
            PropItemInfo info = mat.Key?.GetPropItem();
            if (info == null) continue;
            swallowExp += mat.Value * (int)info.swallowedExpr;
        }
    }

    private void RefreshBar()
    {
        int left = data.GetCurrentLevelExp() + swallowExp;
        int right = 0;
        IntentifyEquipInfo next = moduleEquip.GetIntentifyInfo(equipType, data.GetIntentyLevel() + 1);
        IntentifyEquipInfo current = moduleEquip.GetIntentifyInfo(equipType, data.GetIntentyLevel());

        if (next == null)
        {
            next = moduleEquip.GetIntentifyInfo(equipType, data.GetIntentyLevel() - 1);
            right = current.exp - next.exp;
            Util.SetText(m_expText, Util.Format(ConfigText.GetDefalutString(TextForMatType.EquipIntentyUI, 16), left, right));
            Util.SetExpBar(m_expBars, 0, left, right);
        }
        else
        {
            right = next.exp - (current == null ? 0 : current.exp);
            int deltaLevel = m_previewIntentifyInfo == null ? 0 : m_previewIntentifyInfo.level - data.GetIntentyLevel();
            if (deltaLevel < 0) deltaLevel = 0;
            Util.SetText(m_expText, Util.Format(ConfigText.GetDefalutString(TextForMatType.EquipIntentyUI, 16), left, right));
            Util.SetExpBar(m_expBars, deltaLevel, left, right);
        }
    }

    private void RefreshCoinCost(int totalExp)
    {
        int totalNum = Module_Equip.GetCoinCost(equipType, data.GetCurrentTotalExp(), totalExp, m_limitIntentifyInfo);
        bool moneyEnough = Module_Player.instance.roleInfo.coin >= totalNum;
        Util.SetText(m_cost,(int)TextForMatType.EquipIntentyUI, 14, moneyEnough ? totalNum.ToString() : GeneralConfigInfo.GetNoEnoughColorString(totalNum.ToString()));
    }

    #region click event
    public override void AddEvent()
    {
        base.AddEvent();
        EventTriggerListener.Get(m_confirmBtn.gameObject).onClick = OnConfirmClick;
        EventTriggerListener.Get(m_exitBtn.gameObject).onClick = OnExitClick;
    }

    private void OnConfirmClick(GameObject sender)
    {
        Dictionary<PItem, int> dic = new Dictionary<PItem, int>();
        foreach (var item in m_matList)
        {
            if (item.Value > 0) dic.Add(item.Key, item.Value);
        }
        onConfirmClick?.Invoke(dic);
        SetPanelVisible(false);
    }

    private void OnExitClick(GameObject sender)
    {
        SetPanelVisible(false);
    }
    #endregion

    #region item control

    private void RefreshItem(RectTransform rt, PItem item)
    {
        if (m_cacheItems.ContainsKey(rt.gameObject)) m_cacheItems[rt.gameObject] = item;
        else m_cacheItems.Add(rt.gameObject, item);

        PropItemInfo info = item?.GetPropItem();
        int count = moduleEquip.GetPropCount(item.itemTypeId);
        Util.SetItemInfo(rt, info, 0, count, false);
        Text countText = rt.GetComponent<Text>("numberdi/count");
        Util.SetText(countText, Util.Format(ConfigText.GetDefalutString(TextForMatType.EquipIntentyUI, 17), count));

        UpdateSelectCount(rt.gameObject, m_matList.Get(item));
    }

    private void UpdateSelectCount(GameObject obj, int selectCount)
    {
        Button descBtn = obj.GetComponent<Button>("cancel_btn");
        EventTriggerListener.Get(descBtn.gameObject).onClick = OnDescBtnClick;
        descBtn.gameObject.SetActive(selectCount > 0);
        Image img = obj.GetComponent<Image>("count_tip");
        if (img) img.enabled = selectCount > 0;
        Text selectCountText = obj.GetComponent<Text>("count_tip/mark");
        selectCountText.supportRichText = true;
        selectCountText.gameObject.SetActive(selectCount > 0);
        Util.SetText(selectCountText, Util.Format(ConfigText.GetDefalutString(TextForMatType.EquipIntentyUI, 18), selectCount));
    }

    private void OnItemClick(RectTransform rt, PItem item)
    {
        if (!m_matList.ContainsKey(item)) return;

        int limitExp = m_limitIntentifyInfo == null ? 0 : m_limitIntentifyInfo.exp;
        if (limitExp == 0 || totalExp >= limitExp)
        {
            Module_Global.instance.ShowMessage(ConfigText.GetDefalutString(TextForMatType.EquipIntentyUI, 19));
            return;
        }

        int count = moduleEquip.GetPropCount(item.itemTypeId);
        if (m_matList[item] >= count)
        {
            Module_Global.instance.ShowMessage(ConfigText.GetDefalutString(TextForMatType.EquipIntentyUI, 20));
            return;
        }

        m_matList[item]++;
        UpdateSelectCount(rt.gameObject, m_matList[item]);
        RefreshDetail();
    }

    private void OnDescBtnClick(GameObject sender)
    {
        GameObject obj = sender.transform.parent.gameObject;
        PItem item = m_cacheItems.Get(obj);
        if (item == null || !m_matList.ContainsKey(item)) return;

        m_matList[item]--;
        UpdateSelectCount(obj, m_matList[item]);
        RefreshDetail();
    }

    #endregion

}

public class IntentySuccess : BaseSuccessPanel
{
    public IntentySuccess(Transform trans) : base(trans) { }

    private Text m_levelText;
    private Text m_expText;
    private List<Image> m_expBars;

    private TweenBase m_maxTimeTween;

    //操作的时间间隔
    private float m_deltaTime = 0.02f;
    // 上一次操作的时间
    private float m_lastOperateTime;
    //每次操作的增量
    private int m_expDelta;

    //缓存：超过当前等级的升级经验
    private int m_cacheCurExp;
    //缓存：当前等级
    private int m_cacheLevel;

    //缓存：升到下一级的总经验
    private int m_cacheLevelUpExp;
    //当前需要操作的经验条索引
    private int m_expImgIndex;

    //升级到最后的经验值
    private int m_targetExp;
    //升到的目标等级
    private int m_targetLevel;

    public override void InitComponent()
    {
        base.InitComponent();
        Transform node = transform.Find("strength");

        m_nameText = node.GetComponent<Text>("name_txt");
        m_levelText = node.transform.GetComponent<Text>("txt_level_01");
        m_expText = node.transform.GetComponent<Text>("level_bar_text");
        Transform process = node.transform.Find("progressbar");
        m_expBars = Util.GetExpBars(process);
        TweenBase[] ts = process.GetComponents<TweenBase>();
        if (ts != null)
        {
            float time = 0;
            foreach (var item in ts)
            {
                if (item.loopType == LoopType.Yoyo) continue;

                if (item.duration + item.delayStart > time)
                {
                    m_maxTimeTween = item;
                    time = item.duration + item.delayStart;
                }
            }
            if (m_maxTimeTween) m_maxTimeTween.onComplete.AddListener(OnBaseTweenComplete);
        }
    }

    public override void RefreshPanel(PItem item, PItem cache)
    {
        base.RefreshPanel(item, cache);
        
        foreach (var exp in m_expBars)
        {
            exp?.gameObject.SetActive(false);
        }

        m_targetExp = itemData.GetCurrentLevelExp();
        m_targetLevel = itemData.GetIntentyLevel();

        SetCacheLevelData(lastCache.GetIntentyLevel(), lastCache.GetCurrentLevelExp());
        RefreshLevelUpExp();
        RefreshExpDelta();
        RefreshUIComponent();

        if (!m_maxTimeTween) OnBaseTweenComplete(true);
    }

    private void OnBaseTweenComplete(bool forward)
    {
        enableUpdate = true;
    }

    private void SetCacheLevelData(int level, int exp)
    {
        m_cacheLevel = level;
        m_cacheCurExp = exp;
        m_expImgIndex = level % m_expBars.Count;
        m_expImgIndex = m_expImgIndex < 0 ? 0 : m_expImgIndex;
    }

    private void RefreshLevelUpExp()
    {
        IntentifyEquipInfo info = moduleEquip.GetIntentifyInfo(equipType, m_cacheLevel);
        IntentifyEquipInfo nextInfo = moduleEquip.GetIntentifyInfo(equipType, m_cacheLevel + 1);
        IntentifyEquipInfo lastInfo = moduleEquip.GetIntentifyInfo(equipType, m_cacheLevel - 1);
        //max level
        if (nextInfo == null) m_cacheLevelUpExp = info.exp - lastInfo.exp;
        else m_cacheLevelUpExp = nextInfo.exp - (info == null ? 0 : info.exp);
    }

    private void RefreshExpDelta()
    {
        m_expDelta = Mathf.CeilToInt((m_cacheLevelUpExp - m_cacheCurExp) / (GeneralConfigInfo.sexpbarUnitTime / m_deltaTime));
    }

    private void RefreshUIComponent()
    {
        Util.SetText(m_levelText, Util.Format(ConfigText.GetDefalutString(TextForMatType.EquipIntentyUI, 9), m_cacheLevel));
        Util.SetText(m_expText, Util.Format(ConfigText.GetDefalutString(TextForMatType.EquipIntentyUI, 16), m_cacheCurExp, m_cacheLevelUpExp));
        SetImageFillAmount(m_expImgIndex - 1, 1f);
        float fill = m_cacheCurExp * 1.0f / m_cacheLevelUpExp;
        SetImageFillAmount(m_expImgIndex, fill);
    }

    private void SetImageFillAmount(int index, float fillAmount)
    {
        if (index >= 0)
        {
            m_expBars[index].gameObject.SetActive(true);
            m_expBars[index].rectTransform.SetAsLastSibling();
            m_expBars[index].fillAmount = fillAmount;
        }
    }

    private void AddCurrentCacheExp()
    {
        if (Time.time - m_lastOperateTime >= m_deltaTime)
        {
            m_lastOperateTime = Time.time;

            //level tween finish
            if (m_cacheLevel == m_targetLevel && m_cacheCurExp == m_targetExp)
            {
                enableUpdate = false;
                canClick = true;
            }
            else if (m_cacheCurExp == m_cacheLevelUpExp) //ready to level up
            {
                SetCacheLevelData(m_cacheLevel + 1, 0);
                RefreshLevelUpExp();
                RefreshExpDelta();
                RefreshUIComponent();
                //todo level up audio
            }
            else
            {
                m_cacheCurExp += m_expDelta;
                if (m_cacheCurExp > m_cacheLevelUpExp) m_cacheCurExp = m_cacheLevelUpExp;
                if (m_cacheLevel == m_targetLevel && m_cacheCurExp > m_targetExp) m_cacheCurExp = m_targetExp;
                RefreshUIComponent();
            }
        }
    }

    public override void Update()
    {
        base.Update();
        AddCurrentCacheExp();
    }

    public override void OnExitClickSuccess()
    {
        base.OnExitClickSuccess();

        var t = moduleEquip.GetPeiviewEuipeTypeByItem(itemData);
        if (t != PreviewEquipType.Intentify) Window.Hide<Window_Strength>(true);
    }
}

#endregion
