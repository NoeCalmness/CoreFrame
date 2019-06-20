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

public class Window_Evolution : Window
{
    private PItem m_data;
    private EvolvePanel m_evolvePanel;
    private EvolveSuccess m_evolveSuccessPanel;
    private Transform m_npcNode;

    /// <summary>
    /// Called when window added to level
    /// </summary>
    protected override void OnOpen()
    {
        m_evolvePanel = new EvolvePanel(transform);
        m_evolveSuccessPanel = new EvolveSuccess(transform.Find("success_Panel"));
        m_evolveSuccessPanel.SetPanelVisible(false);
        m_evolveSuccessPanel.afterPanelVisible = OnSuccessVisibleChange;
        InitializeText();

        m_npcNode = GetComponent<Transform>("npcInfo");
    }

    private void InitializeText()
    {
        var t = ConfigManager.Get<ConfigText>((int)TextForMatType.EquipEvolveUI);
        if (!t) return;

        Util.SetText(GetComponent<Text>("decoration1/biaoti/text1"), (int)TextForMatType.EquipEvolveUI, 0);
        Util.SetText(GetComponent<Text>("decoration1/biaoti/text2"), (int)TextForMatType.EquipEvolveUI, 1);
        Util.SetText(GetComponent<Text>("decoration1/biaoti/english"), (int)TextForMatType.EquipEvolveUI, 2);

        Util.SetText(GetComponent<Text>("top/levellimit"), (int)TextForMatType.EquipEvolveUI, 4);
        
        Util.SetText(GetComponent<Text>("intentify_Panel/comsume_1/consume_txt_02"), (int)TextForMatType.EquipEvolveUI, 5);
        Util.SetText(GetComponent<Text>("intentify_Panel/btn_strenthen_1/Text"), (int)TextForMatType.EquipEvolveUI, 6);
       
        Util.SetText(GetComponent<Text>("success_Panel/title_txt"), (int)TextForMatType.EquipEvolveUI, 7);
        Util.SetText(GetComponent<Text>("success_Panel/Evo/level_txt"), (int)TextForMatType.EquipEvolveUI, 8);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();
        //ShowNpc();

        m_data = moduleEquip.operateItem;

        if (m_data == null) Logger.LogError("cannot finded a valid opreate pitem at Window_Evolution :: OnBecameVisible");
        else m_evolvePanel.RefreshPanel(m_data);
    }

    private void OnSuccessVisibleChange(bool visible)
    {
        if (visible) moduleHome.HideOthers();
        else
        {
            m_npcNode.SafeSetActive(true);
        }
    }

    private void _ME(ModuleEvent<Module_Player> e)
    {
        if (e.moduleEvent == Module_Player.EventCurrencyChanged)
        {
            var t = (CurrencySubType)e.param1;
            if (t == CurrencySubType.Gold && m_evolvePanel != null && m_evolvePanel.enable) m_evolvePanel.RefreshSelf();
        }
    }

    private void _ME(ModuleEvent<Module_Equip> e)
    {
        if (e.moduleEvent == Module_Equip.EventOperateSuccess && actived)
        {
            EnumOperateEquipType t = (EnumOperateEquipType)e.param1;
            if (t == EnumOperateEquipType.Evolve) OnEvolveSuccess((PItem)e.param2, (PItem)e.param3);
        }
        else if(e.moduleEvent == Module_Equip.EventUpdateBagProp && m_evolvePanel != null && m_evolvePanel.enable) m_evolvePanel.RefreshSelf();
    }

    private void OnEvolveSuccess(PItem tar, PItem lastCache)
    {
        if (tar == null || lastCache == null)
        {
            Logger.LogError("intenty success callback error, tar is {0} last cache is {1}", tar == null ? "error data" : "valid", lastCache == null ? "error data" : "valid");
            return;
        }

        m_evolveSuccessPanel.RefreshPanel(tar, lastCache);
        m_evolvePanel.RefreshPanel(m_data);
        m_npcNode.SafeSetActive(false);
        m_evolvePanel.SendEvolve = true;
    }
}

#region custom class

public sealed class EvolvePanel : PreviewBasePanel
{
    public EvolvePanel(Transform trans) : base(trans) { }

    private Button m_evoBtn;
    private Text m_evoCostText;
    private AttributeItem m_levelLimitItem;
    private List<AttributeItem> m_attriList = new List<AttributeItem>();
    private string[] m_attriNames = new string[] { "attribute_01", "attribute_02", "attribute_03" };

    public bool SendEvolve;
    public override void InitComponent()
    {
        base.InitComponent();

        SendEvolve = true;
        m_evoBtn = transform.GetComponent<Button>("intentify_Panel/btn_strenthen_1");
        m_evoCostText = transform.GetComponent<Text>("intentify_Panel/comsume_1/consume_txt");
        m_attriList.Clear();
        foreach (var item in m_attriNames)
        {
            m_attriList.Add(new AttributeItem(transform.Find(Util.Format("top/{0}", item))));
        }
        m_levelLimitItem = new AttributeItem(transform.Find("top/levellimit"));

        m_evoBtn.onClick.RemoveAllListeners();
        m_evoBtn.onClick.AddListener(OnEvolveClick);
    }
    
    private void OnEvolveClick()
    {
        moduleEquip.SendEvolveEquip(data);
        SendEvolve = false;
        m_evoBtn.SetInteractable(false);
    }

    protected override void InitPanel()
    {
        base.InitPanel();
        m_evoCostText.text = "0";
        m_evoBtn.SetInteractable(false);
        foreach (var item in m_attriList) item.SetPanelVisible(false);
    }

    public override void RefreshPanel(PItem item, EquipType type)
    {
        base.RefreshPanel(item, type);

        int level = data.GetIntentyLevel();
        Util.SetText(m_levelText, (int)TextForMatType.EquipEvolveUI, 3, level.ToString());
        m_levelText.SafeSetActive(level > 0);

        level = data.GetEvolveLevel();
        //获取的是下一级的进阶显示
        EvolveEquipInfo info = moduleEquip.GetEvolveInfo(equipType, level + 1);
        if (info == null) return;

        List<PItem> currenProps = moduleEquip.GetBagProps(PropType.EvolveProp);
        RefreshMatItem(info.materials, currenProps);
        RefreshLevel();
        RefreshAttributes(info);
        bool moneyEnough = Module_Player.instance.roleInfo.coin > info.costNumber;
        Util.SetText(m_evoCostText, moneyEnough ? info.costNumber.ToString() : GeneralConfigInfo.GetNoEnoughColorString(info.costNumber.ToString()));

        bool materialEnough = Module_Equip.GetMaterialEnoughState(info.materials, currenProps);
        m_evoBtn.SetInteractable(moneyEnough && materialEnough && SendEvolve);
    }

    public void RefreshSelf()
    {
        if (data != null) RefreshPanel(data, equipType);
    }

    private void RefreshLevel()
    {
        int leftLevel = data.GetIntentyLevel();
        int nextLimitLevel = moduleEquip.GetLimitIntenLevel(equipType, leftLevel, true);
        m_levelLimitItem.RefreshDetail(leftLevel, nextLimitLevel);
    }

    private void RefreshAttributes(EvolveEquipInfo info)
    {
        List<AttributePreviewDetail> list = GetAttributeDetails(info);

        for (int i = 0; i < list.Count; i++)
        {
            if (i >= m_attriList.Count) break;
            m_attriList[i].RefreshDetail(list[i]);
        }
    }

    private List<AttributePreviewDetail> GetAttributeDetails(EvolveEquipInfo info)
    {
        var list = new List<AttributePreviewDetail>();
        if (info.attributes == null) return list;

        foreach (var item in info.attributes)
        {
            //获取道具的初始属性
            var originalAttri = PropItemInfo.GetBaseAttribute(itemInfo, item.id);
            //计算属性(百分比或者累加)
            var value = ItemAttachAttr.CalculateAttributeValue(originalAttri, item);

            if (!ContainSameAttirbutePreview(list, item.id))
            {
                double oldValue = data.GetFixedAttirbute(item.id);
                if (oldValue == 0 && value == 0) continue;

                var de = new AttributePreviewDetail(item.id);
                de.oldValue = oldValue;
                //must before set newValue
                de.FixPercentAttribute();
                de.newValue = de.oldValue + value;
                list.Add(de);
            }
            else
            {
                var de = list.Find(o => o.id == item.id);
                de.newValue += value;
            }

        }

        return list;
    }
}

public class EvolveSuccess : BaseSuccessPanel
{
    public EvolveSuccess(Transform trans) : base(trans) { }
    private Text m_currentLevel;
    private Text m_nextLimitLevel;
    private TweenBase m_t;

    public override void InitComponent()
    {
        base.InitComponent();

        Transform node = transform.Find("Evo");

        node.SafeSetActive(true);
        m_nameText = node.GetComponent<Text>("name_txt");
        m_currentLevel = node.GetComponent<Text>("left_txt");
        m_nextLimitLevel = node.GetComponent<Text>("right_txt");

        TweenBase[] ts = node.GetComponentsInChildren<TweenBase>(true);
        if (ts != null)
        {
            float time = 0f;
            foreach (var t in ts)
            {
                if (t.loopType == LoopType.Yoyo) continue;

                if (t.duration + t.delayStart > time)
                {
                    time = t.duration + t.delayStart;
                    m_t = t;
                }
            }
            if (m_t) m_t.onComplete.AddListener((forward) => { canClick = true; });
        }
    }

    public override void RefreshPanel(PItem item, PItem cache)
    {
        base.RefreshPanel(item, cache);
        if (item.InvalidGrowAttr())
        {
            canClick = true;
            return;
        }

        EquipType type = Module_Equip.GetEquipTypeByID(item.itemTypeId);
        int level = item.GetIntentyLevel();
        int limitLevel = moduleEquip.GetLimitIntenLevel(type, level, true);
        m_currentLevel.text = level.ToString();
        m_nextLimitLevel.text = limitLevel.ToString();
        if (!m_t) canClick = true;
    }

    public override void OnExitClickSuccess()
    {
        base.OnExitClickSuccess();
        Window.Hide<Window_Evolution>(true);
    }
}
#endregion
