/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Charge window
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-02-26
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Charge : Window
{
    private static ChargeType defaultType = ChargeType.Recharge;
    public static void Open(ChargeType rDefault)
    {
        defaultType = rDefault;
        ShowAsync<Window_Charge>();
    }
    public enum ChargeType
    {
        Recharge = 1,
        Card,
        Growth,
        Gift,
        TotalRecharge,
        WishStone,
        SummonStone,
    }

    private Transform rechargeRoot;
    private Transform totalRechargeRoot;
    private Transform detailRoot;

    private ToggleGroup group;

    readonly Dictionary<ChargeType, SubWindowBase> subWindows = new Dictionary<ChargeType, SubWindowBase>();
    private readonly Dictionary<Toggle, ChargeType> map = new Dictionary<Toggle, ChargeType>();

    private ChargeWindow_Detail detailWindow;
    protected override void OnOpen()
    {
        base.OnOpen();
        InitComponent();
        MultiLangrage();
        subWindows.Clear();
        subWindows.Add(ChargeType.Recharge,         SubWindowBase.CreateSubWindow<ChargeWindow_Recharge, Window_Charge>     (this, rechargeRoot     ?.gameObject));
        subWindows.Add(ChargeType.TotalRecharge,    SubWindowBase.CreateSubWindow<ChargeWindow_TotalRecharge, Window_Charge>(this, totalRechargeRoot?.gameObject));
        detailWindow = SubWindowBase.CreateSubWindow<ChargeWindow_Detail>(this, detailRoot?.gameObject);

        map.Add(GetComponent<Toggle>("checkBox/1"), ChargeType.Recharge);
        map.Add(GetComponent<Toggle>("checkBox/5"), ChargeType.TotalRecharge);

        group.onAnyToggleStateOn.AddListener(OnToggleOn);
    }

    private void MultiLangrage()
    {
        var ct = ConfigManager.Get<ConfigText>((int) TextForMatType.RechargeUIText);
        
        Util.SetText(GetComponent<Text>("checkBox/5/label_Txt"),                              ct[9]);
        Util.SetText(GetComponent<Text>("checkBox/1/label_Txt"),                              ct[12]);
        Util.SetText(GetComponent<Text>("1_panel/scrollView/template/0/title_Img/title_Txt"), ct[18]);
        Util.SetText(GetComponent<Text>("1_panel/buy_btn/Text"),                              ct[16]);
        Util.SetText(GetComponent<Text>("5_panel/buy_btn/Text"),                              ct[17]);
    }

    protected override void OnClose()
    {
        base.OnClose();
        foreach (var kv in subWindows)
        {
            kv.Value.Destroy();
        }
        subWindows.Clear();
        detailWindow.Destroy();
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();

        if (m_subTypeLock != -1 && m_subTypeLock < (int)ChargeType.SummonStone)
            defaultType = (ChargeType)(m_subTypeLock + 1);

        ShowWindow(defaultType);

        InitCheckBoxState();
    }

    private void InitCheckBoxState()
    {
        foreach (var kv in map)
        {
            kv.Key.SafeSetActive(moduleCharge.IsTabActive((int) kv.Value));
        }
    }

    private void InitComponent()
    {
        rechargeRoot        = GetComponent<Transform>("1_panel");
        totalRechargeRoot   = GetComponent<Transform>("5_panel");
        detailRoot          = GetComponent<Transform>("preview_panel");
        group               = GetComponent<ToggleGroup>("checkBox");

        rechargeRoot        ?.SafeSetActive(false);
        totalRechargeRoot   ?.SafeSetActive(false);
        detailRoot          ?.SafeSetActive(false);
    }

    private void OnToggleOn(Toggle t)
    {
        if(map.ContainsKey(t))
            ShowWindow(map[t]);
    }

    public void ShowWindow(ChargeType rType)
    {
        foreach (var window in subWindows)
            window.Value?.UnInitialize();

        if (subWindows.ContainsKey(rType))
            subWindows[rType].Initialize();

        foreach (var kv in map)
        {
            if (kv.Value == rType)
                kv.Key.isOn = true;
        }
    }


    public void ShowDetail(PChargeItem item)
    {
        ShowDetail(item.info.reward);

    }

    public void ShowDetail(PReward reward)
    {
        List<ItemPair> list = new List<ItemPair>();

        if (reward.diamond > 0)
        {
            var p = new ItemPair
            {
                itemId = 2,
                count = reward.diamond
            };
            list.Add(p);
        }

        if (reward.coin > 0)
        {
            var p = new ItemPair
            {
                itemId = 1,
                count = reward.coin
            };
            list.Add(p);
        }

        if (reward.fatigue > 0)
        {
            var p = new ItemPair
            {
                itemId = 15,
                count = reward.fatigue
            };
            list.Add(p);
        }
        foreach (var p in reward.rewardList)
        {
            list.Add(new ItemPair {itemId = p.itemTypeId, count = (int) p.num});
        }
        ShowDetail(list);
    }

    public void ShowDetail(List<ItemPair> list)
    {
        if (list == null || list.Count == 0)
            return;

        detailWindow.Initialize(list);
    }

    void _ME(ModuleEvent<Module_Charge> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Charge.EventBuyItem:
                OnBuyItem(e.msg as ScChargeBuyItem);
                break;
        }
    }

    private void OnBuyItem(ScChargeBuyItem msg)
    {
        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(9302, msg.result));
        }
    }

    protected override void OnHide(bool forward)
    {
        defaultType = ChargeType.Recharge;
    }

    protected override void OnReturn()
    {
        base.OnReturn();
        if (moduleSet.openChangeName) ShowAsync<Window_System>();
    }
}
