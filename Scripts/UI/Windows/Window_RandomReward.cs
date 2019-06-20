/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-01-23
 * 
 ***************************************************************************************************/

using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Window_RandomReward : Window
{
    private const float REWARD_SHOW_TIME = 1.2f;
    private static readonly string[] rewardNames = new string[] { "rewardcopperchest", "rewardsilverchest", "rewardgoldchest" };
    public static string ASSET_NAME { get { return Game.GetDefaultName<Window_RandomReward>().ToLower(); } }

    private GameObject m_getRewardPanel;
    private List<GameObject> m_rewardAnimList = new List<GameObject>();

    private GameObject m_rewardShowPanel;
    private CanvasGroup m_rewardShowCG;
    private TweenAlpha m_rewardShowTween;
    private GameObject m_rewardItem;
    private GameObject m_rewardParent;
    private float m_animEndTime = 0f;
    private bool useAiCache = false;

    /// <summary>
    /// Called when window added to level
    /// </summary>
    protected override void OnOpen()
    {
        isFullScreen = false;

        m_getRewardPanel = transform.Find("reward_tip").gameObject;
        m_rewardShowPanel = transform.Find("reward_show").gameObject;
        m_rewardParent = transform.Find("reward_show/reward").gameObject;
        m_rewardShowCG = m_rewardShowPanel.GetComponent<CanvasGroup>();
        m_rewardShowTween = m_rewardShowPanel.GetComponent<TweenAlpha>();
        m_rewardItem = transform.Find("reward_show/item").gameObject;

        m_rewardAnimList.Clear();
        foreach (var item in rewardNames)
        {
            GameObject o = m_getRewardPanel.transform.Find(item).gameObject;
            if (!o) Logger.LogError("{0}/{1} connot be finded", m_getRewardPanel.name, item);
            if (o) o.SetActive(false);
            m_rewardAnimList.Add(o);
        }

        m_rewardItem.SetActive(false);
        AddEvent();
        InitializeText();
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        var levelPve = Level.current as Level_PVE;
        if (levelPve != null && modulePVE.isStandalonePVE)
        {
            useAiCache = levelPve.player.useAI;
            levelPve.player.useAI = false;
        }
    }

    protected override void OnHide(bool forward)
    {
        base.OnHide(forward);
        var levelPve = Level.current as Level_PVE;
        if (levelPve != null && modulePVE.isStandalonePVE) levelPve.player.useAI = useAiCache;
    }

    private void AddEvent()
    {
        if (m_rewardShowTween)
        {
            m_rewardShowTween.onComplete.RemoveListener(OnShowRewardAnimationEnd);
            m_rewardShowTween.onComplete.AddListener(OnShowRewardAnimationEnd);
        }
        EventTriggerListener.Get(m_rewardShowPanel).onClick = OnConfirmBtnClick;
    }

    private void InitializeText()
    {
        Util.SetText(GetComponent<Text>("reward_tip/tip2"), (int)TextForMatType.BorderlandUIText, 19);
        Util.SetText(GetComponent<Text>("reward_show/signsucced/up"), (int)TextForMatType.BorderlandUIText, 20);
        Util.SetText(GetComponent<Text>("reward_show/reward/item/level_bg/level"), (int)TextForMatType.BorderlandUIText, 21);
        Util.SetText(GetComponent<Text>("reward_show/reward/item/name"), (int)TextForMatType.BorderlandUIText, 22);
        Util.SetText(GetComponent<Text>("reward_show/notice"), (int)TextForMatType.BorderlandUIText, 23);
    }

    private void SetGetRewardPanelVisible()
    {
        m_getRewardPanel.SetActive(true);
        int index = GetMonsterRewardIndex();
        m_rewardAnimList[index].SetActive(true);
        m_rewardShowCG.alpha = 0f;
        m_animEndTime = 0f;
    }

    private int GetMonsterRewardIndex()
    {
        //pve里面用金宝箱
        if (Level.current && Level.current is Level_PVE) return rewardNames.Length - 1;

        if (moduleBordlands.lastChallengeMonster == null) return 0;

        switch ((Module_Bordlands.EnumBordlandMonsterType)moduleBordlands.lastChallengeMonster.type)
        {
            case Module_Bordlands.EnumBordlandMonsterType.NormalType: return 0;
            case Module_Bordlands.EnumBordlandMonsterType.BossType: return 1;
        }

        return 0;
    }

    public void RefreshReward(PReward settleReward)
    {
        if (settleReward == null) return;
        
        List<PItem2> rewards = new List<PItem2>();
        if (settleReward.coin > 0) rewards.Add(GetMoneyPitem(settleReward.coin, CurrencySubType.Gold));
        if (settleReward.diamond > 0) rewards.Add(GetMoneyPitem(settleReward.diamond, CurrencySubType.Diamond));
        if (settleReward.rewardList != null)
        {
            for (int i = 0; i < settleReward.rewardList.Length; i++)
            {
                rewards.Add(settleReward.rewardList[i]);
            }
        }
        RefreshReward(rewards);
    }

    public void RefreshReward(PItem2[] bossReward)
    {
        if (bossReward == null) return;
        
        List<PItem2> rewards = new List<PItem2>();
        rewards.AddRange(bossReward);
        RefreshReward(rewards);
    }

    public void RefreshReward(List<PItem2> rewards)
    {
        if (rewards == null) return;

        SetGetRewardPanelVisible();
        for (int i = 0; i < rewards.Count; i++)
        {
            if (rewards[i].num == 0) continue;

            PropItemInfo propInfo = ConfigManager.Get<PropItemInfo>(rewards[i].itemTypeId);
            if (propInfo == null)
                continue;

            Transform itemTrans = m_rewardParent.transform.AddNewChild(m_rewardItem);
            itemTrans.gameObject.SetActive(true);

            bool isRune = propInfo.itemType == PropType.Rune;
            Util.SetItemInfo(itemTrans, propInfo, 0, 0, propInfo.itemType != PropType.Currency, isRune ? rewards[i].star : propInfo.quality);

            Transform t = itemTrans.Find("name");
            t?.gameObject.SetActive(true);
            Text c = itemTrans.GetComponent<Text>("numberdi/count");
            if (c) Util.SetText(c, ConfigText.GetDefalutString(TextForMatType.BorderlandUIText, 13), rewards[i].num);
        }
    }


    private PItem2 GetMoneyPitem(int money, CurrencySubType currency)
    {
        if (money <= 0)
            return null;
        PItem2 moneyItem = PacketObject.Create<PItem2>();
        moneyItem.num = (uint)money;
        moneyItem.itemTypeId = (ushort)currency;
        return moneyItem;
    }

    private void OnConfirmBtnClick(GameObject sender)
    {
        if (m_animEndTime == 0 || Time.realtimeSinceStartup - m_animEndTime < REWARD_SHOW_TIME) return;
        m_animEndTime = 0f;

        if (Level.current && Level.current is Level_Bordlands)
        {
            moduleBordlands.bordlandsSettlementReward = null;
            moduleBordlands.lastChallengeMonster = null;
        }
        Hide(true);
        moduleBordlands.DispatchModuleEvent(Module_Bordlands.EventShowRewardOver);
    }

    private void OnShowRewardAnimationEnd(bool reverse)
    {
        //Logger.LogWarning("{0} :: OnShowRewardAnimationEnd.......called", Time.realtimeSinceStartup);
        m_animEndTime = Time.realtimeSinceStartup;
    }
}
