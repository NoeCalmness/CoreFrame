// /**************************************************************************************************
//  * Copyright (C) 2017-2018 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-06-18      15:22
//  *LastModify：2019-06-18      15:23
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class FactionRuleBehaviour : AssertOnceBehaviour
{
    private ScrollView m_winRewards;
    private ScrollView m_loseRewards;
    private Text       m_rulerText;
    private Text       m_openTimeText;

    private void Start()
    {
        AssertInit();
    }

    protected override void Init()
    {
        base.Init();

        MultiLangrage();

        m_rulerText         = transform.GetComponent<Text>("ScorllRect/Viewport/rule_txt");
        m_winRewards        = transform.GetComponent<ScrollView>("reward/rewardList01");
        m_loseRewards       = transform.GetComponent<ScrollView>("reward/rewardList02");
        m_openTimeText      = transform.GetComponent<Text>("time_txt");

        Util.SetText(m_rulerText, ConfigText.GetDefalutString(TextForMatType.FactionSignUI, 3));
        Util.SetText(m_openTimeText, Module_FactionBattle.instance.ActiveTime);

        new DataSource<ItemPair>(ConfigManager.Get<FactionResultRewardInfo>(1).reward.ToItemPairList(), m_winRewards, OnSetData);
        new DataSource<ItemPair>(ConfigManager.Get<FactionResultRewardInfo>(2).reward.ToItemPairList(), m_loseRewards, OnSetData);
    }

    private void MultiLangrage()
    {
        var ct = ConfigManager.Get<ConfigText>((int)TextForMatType.FactionSignUI);
        if (null == ct)
            return;

        Util.SetText(transform.GetComponent<Text>("titile01/Text"), ct[5]);
        Util.SetText(transform.GetComponent<Text>("titile02/Text"), ct[6]);
        Util.SetText(transform.GetComponent<Text>("reward/txt01"), ct[9]);
        Util.SetText(transform.GetComponent<Text>("reward/txt02"), ct[10]);
    }

    private void OnSetData(RectTransform node, ItemPair data)
    {
        var prop = ConfigManager.Get<PropItemInfo>(data.itemId);
        if (prop == null)
        {
            Logger.LogError($"无效的道具ID = {data.itemId}");
            return;
        }
        Util.SetItemInfo(node, prop, 0, data.count, false);

        var b = node.GetComponentDefault<Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() => { Module_Global.instance.UpdateGlobalTip((ushort)data.itemId, true, false); });
    }

}
