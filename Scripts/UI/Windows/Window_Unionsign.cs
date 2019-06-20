/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-03-12
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Unionsign : Window
{
    RectTransform m_rulePlane;
    Text m_chickTxt;
    Button m_ruleBtn;
    Button m_finishBtn;
    Button m_nextBtn;
    RectTransform m_cardGroup;
    CanvasGroup m_group;
    Text m_timeTxt;
    Text m_typeTxt;
    Text m_rewardTxt;
    Text m_refreshTxt;
    RectTransform m_cardBgGroup;
    RectTransform m_cardShowGroup;
    Button m_cardChangeBtn;
    List<Toggle> m_itemList = new List<Toggle>();
    List<ushort> m_tChick = new List<ushort>();//选中要翻牌的
    List<GameObject> m_itemBg = new List<GameObject>();
    List<GameObject> m_cardShow = new List<GameObject>();
    Dictionary<int, GameObject> m_cardObj = new Dictionary<int, GameObject>();
    DataSource<CardTypeInfo> m_introduceData;
    bool returnMain = true;
    protected override void OnOpen()
    {
        returnMain = true;
        m_group = GetComponent<CanvasGroup>("cardGroup");
        m_cardGroup = GetComponent<RectTransform>("cardGroup");
        m_cardBgGroup = GetComponent<RectTransform>("GameObject");
        m_cardShowGroup = GetComponent<RectTransform>("signRulePlane/rune_plane/cardgroup");

        m_rulePlane = GetComponent<RectTransform>("signRulePlane");
        m_ruleBtn = GetComponent<Button>("ruleBtn");
        m_finishBtn = GetComponent<Button>("finishBtn");
        m_nextBtn = GetComponent<Button>("next_btn");
        m_timeTxt = GetComponent<Text>("timeTxt");
        m_typeTxt = GetComponent<Text>("cardType");
        m_rewardTxt = GetComponent<Text>("otherTxt");
        m_refreshTxt = GetComponent<Text>("Text");
        m_chickTxt = GetComponent<Text>("click_txt");
        m_cardChangeBtn = GetComponent<Button>("changeBtn");

        if (moduleUnion.TypeList.Count <= 0) moduleUnion.GetAllInfo();
        var scoll = GetComponent<ScrollView>("signRulePlane/info/ruleList");
        m_introduceData = new DataSource<CardTypeInfo>(moduleUnion.TypeList, scoll, SetRulePlane);

        m_ruleBtn.onClick.RemoveAllListeners();
        m_ruleBtn.onClick.AddListener(delegate
        {
            returnMain = false;
            m_introduceData.UpdateItems();
            m_rulePlane.gameObject.SetActive(true);
        });
        m_nextBtn.onClick.RemoveAllListeners();
        m_nextBtn.onClick.AddListener(delegate
        {
            moduleUnion.GetNextCard();
            m_tChick.Clear();
        });
        m_finishBtn.onClick.RemoveAllListeners();
        m_finishBtn.onClick.AddListener(delegate
        {
            moduleUnion.GetCardReward();
            m_tChick.Clear();
        });
        m_cardChangeBtn.onClick.RemoveAllListeners();
        m_cardChangeBtn.onClick1.AddListener(delegate
        {
            SetAnimation(false);
        });
        m_cardChangeBtn.onClick.AddListener(delegate
        {
            if (m_tChick.Count == 0) moduleGlobal.ShowMessage(629, 18);
            else
            {
                SetAnimation(true);
                moduleUnion.ChangeCard(m_tChick.ToArray());
            }
        });

        GetButton();
        SetText();
    }

    private void SetAnimation(bool click)
    {
        if (m_tChick.Count <= 0) return;
        for (int i = 0; i < m_tChick.Count; i++)
        {
            var btn = m_cardObj[m_tChick[i]];
            TweenSize size = btn.gameObject.GetComponent<TweenSize>();
            if (click)
            {
                TweenScale scale = btn.gameObject.GetComponent<TweenScale>();
                size?.onStart.AddListener(delegate { m_group.interactable = false; });
                size?.onComplete.AddListener(delegate
                {
                    if (moduleUnion.CardSignInfo.changeTimes > 0) m_group.interactable = true;
                });
                if (moduleUnion.CardSignInfo.changeTimes > 0)
                {
                    size?.PlayForward();
                    scale?.PlayForward();
                }
            }
            else size.PlayReverse();
        }
        if (!click) m_tChick.Clear();
    }

    private void GetButton()
    {
        m_itemList.Clear();
        foreach (Transform item in m_cardGroup)
        {
            var t = item.gameObject.GetComponent<Toggle>();
            m_itemList.Add(t);
        }
        m_itemBg.Clear();
        foreach (Transform item in m_cardBgGroup)
        {
            m_itemBg.Add(item.gameObject);
        }
        m_cardShow.Clear();
        foreach (Transform item in m_cardShowGroup)
        {
            m_cardShow.Add(item.gameObject);
        }
    }

    private void SetText()
    {
        Util.SetText(GetComponent<Text>("changeBtn/Text"), 629, 19);
        Util.SetText(GetComponent<Text>("next_btn/Text"), 629, 12);
        Util.SetText(GetComponent<Text>("info/title"), 629, 0);
        Util.SetText(GetComponent<Text>("rewardTxt"), 629, 1);
        Util.SetText(GetComponent<Text>("ruleBtn/Text"), 629, 2);
        Util.SetText(GetComponent<Text>("finishBtn/Text"), 629, 3);
        Util.SetText(GetComponent<Text>("signRulePlane/info/Text"), 629, 14);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();
        if (m_subTypeLock == -1) returnMain = true;
        SetSignInfo();
    }

    private void SetSignInfo()
    {
        m_tChick.Clear();
        if (moduleUnion.CardSignInfo == null) return;
        string signKey = "unionSign" + modulePlayer.id_.ToString() + modulePlayer.roleInfo?.leagueID.ToString();

        m_cardBgGroup.SafeSetActive(false);
        var first = moduleWelfare.FrirstOpen(signKey);
        if (first)
        {
            m_cardBgGroup.SafeSetActive(true);
            for (int i = 0; i < m_itemBg.Count; i++)
            {
                if (m_itemBg[i] == null) continue;
                TweenRotation rotation = m_itemBg[i].GetComponent<TweenRotation>();
                TweenAlpha alpha = m_itemBg[i].GetComponent<TweenAlpha>();
                TweenPosition position = m_itemBg[i].GetComponent<TweenPosition>();
                rotation?.Play();
                alpha?.Play();
                position?.Play();
            }
        }

        SetTimesTxt();
    }

    private void SetTimesTxt()
    {
        if (moduleUnion.CardSignInfo == null) return;
        m_chickTxt.SafeSetActive(moduleUnion.CardSignInfo.changeTimes > 0);

        Util.SetText(m_refreshTxt, ConfigText.GetDefalutString(629, 17), moduleUnion.CardSignInfo.getTimes);
        m_group.interactable = false;
        if (moduleUnion.CardSignInfo.changeTimes > 0) m_group.interactable = true;

        SetCardTypeShow();

        var type = string.Empty;
        for (int i = 0; i < moduleUnion.CardSignInfo.typeInfo.Length; i++)
        {
            var key = moduleUnion.CardSignInfo.typeInfo[i].cardType;
            var info = ConfigManager.Get<CardTypeInfo>(key);
            if (info == null) continue;
            type += string.Format(ConfigText.GetDefalutString(629, 8), ConfigText.GetDefalutString(info.nameId), info.point);
        }
        Util.SetText(m_typeTxt, type);
        Util.SetText(m_rewardTxt, ConfigText.GetDefalutString(629, 5), moduleUnion.CardSignInfo.points);

        m_finishBtn.SafeSetActive(moduleUnion.CardSignInfo.changeTimes > 0);
        m_nextBtn.SafeSetActive(moduleUnion.CardSignInfo.changeTimes == 0 && moduleUnion.CardSignInfo.getTimes != 0);
        m_refreshTxt.SafeSetActive(moduleUnion.CardSignInfo.changeTimes > 0 || moduleUnion.CardSignInfo.getTimes > 0);
        m_cardChangeBtn.SafeSetActive(moduleUnion.CardSignInfo.changeTimes > 0);

        if (moduleUnion.CardSignInfo.changeTimes > 0)
        {
            m_finishBtn.interactable = true;
            Util.SetText(m_timeTxt, ConfigText.GetDefalutString(629, 4), moduleUnion.CardSignInfo.changeTimes);
        }
        else
        {
            m_finishBtn.interactable = false;
            Util.SetText(m_timeTxt, ConfigText.GetDefalutString(629, 7));
        }
    }

    private void SetCardTypeShow()
    {
        m_cardObj.Clear();
        for (int i = 0; i < m_itemList.Count; i++)
        {
            if (m_itemList[i] == null) continue;
            SetCardInfo(m_itemList[i], i);
            var key = moduleUnion.CardSignInfo.cardId[i];
            if (m_cardObj.ContainsKey(key))
                continue;
            m_cardObj.Add(key, m_itemList[i].gameObject);
        }

        var card = CardTypeList();
        foreach (var item in m_cardObj)
        {
            var group = item.Value.transform.Find("typeGroup");
            List<Text> All = new List<Text>();
            foreach (RectTransform rt in group)
            {
                All.Add(rt.GetComponent<Text>());
                rt.SafeSetActive(false);
            }
            if (!card.ContainsKey(item.Key)) continue;
            for (int i = 0; i < card[item.Key].Count; i++)
            {
                if (i >= All.Count) continue;
                All[i].SafeSetActive(true);
                var info = ConfigManager.Get<CardTypeInfo>(card[item.Key][i]);
                if (info == null) continue;
                Util.SetText(All[i], ConfigText.GetDefalutString(info.nameId));
            }
        }
    }

    private Dictionary<int, List<int>> CardTypeList()
    {
        Dictionary<int, List<int>> types = new Dictionary<int, List<int>>();
        for (int i = 0; i < moduleUnion.CardSignInfo.typeInfo.Length; i++)
        {
            var m = moduleUnion.CardSignInfo.typeInfo[i];
            if (m == null) continue;
            for (int j = 0; j < m.cardId.Length; j++)
            {
                var key = m.cardId[j];
                if (types.ContainsKey(key))
                {
                    var have = types[key].Exists(a => a == m.cardType);
                    if (!have) types[key].Add(m.cardType);
                }
                else
                {
                    List<int> t = new List<int>();
                    t.Add(m.cardType);
                    types.Add(key, t);
                }
            }
        }
        return types;
    }

    private void SetCardInfo(Toggle btn, int index)
    {
        if (btn == null || moduleUnion.CardSignInfo == null) return;
        var key = moduleUnion.CardSignInfo.cardId[index];
        var info = ConfigManager.Get<CardInfo>(key);
        btn.onValueChanged.RemoveAllListeners();
        AtlasHelper.SetHuazha(btn.gameObject, info?.icon);
        btn.isOn = m_tChick.Exists(a => a == key);
        btn.onValueChanged.AddListener(delegate
        {
            var t = m_tChick.Exists(a => a == key);
            if (!btn.isOn && t) m_tChick.Remove(key);
            else if (btn.isOn && !t) m_tChick.Add(key);
        });
    }

    private void SetRulePlane(RectTransform rt, CardTypeInfo info)
    {
        if (info == null) return;

        var desc = rt.Find("desc").GetComponent<Text>();
        Util.SetText(desc, info.descId);
        var point = rt.Find("bg/time_txt").GetComponent<Text>();
        var pTxt = string.Format(ConfigText.GetDefalutString(629, 16), info.point);
        var title = string.Format("{0} {1}", ConfigText.GetDefalutString(info.nameId), pTxt);
        Util.SetText(point, title);
        List<RectTransform> typeList = new List<RectTransform>();
        var typeGroup = rt.Find("itemGroup").GetComponent<RectTransform>();
        foreach (RectTransform item in typeGroup)
        {
            item.gameObject.SetActive(false);
            typeList.Add(item);
        }
        for (int i = 0; i < info.cardId.Length; i++)
        {
            if (i >= typeList.Count) continue;
            typeList[i].SafeSetActive(true);
            var card = ConfigManager.Get<CardInfo>(info.cardId[i]);
            if (card == null) continue;
            var c = typeList[i].Find("card");
            AtlasHelper.SetHuazha(c.gameObject, card.icon);
        }
        var btn = typeGroup.GetComponentDefault<Button>();
        btn.onClick.AddListener(delegate
        {
            SetCardPlane(info.cardId);
        });
    }

    private void SetCardPlane(int[] cardId)
    {
        for (int i = 0; i < m_cardShow.Count; i++)
        {
            m_cardShow[i].SafeSetActive(false);
        }
        for (int i = 0; i < cardId.Length; i++)
        {
            if (i >= m_cardShow.Count) continue;
            if (m_cardShow[i] == null) continue;
            m_cardShow[i].SafeSetActive(true);
            var card = ConfigManager.Get<CardInfo>(cardId[i]);
            if (card == null) continue;
            AtlasHelper.SetHuazha(m_cardShow[i], card.icon);
        }
    }

    void _ME(ModuleEvent<Module_Union> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Union.EventUnionCardSign:
                if (!actived) return;
                moduleUnion.RemoveFirstSign();
                SetSignInfo();
                break;
            case Module_Union.EventUnionCardChange:
                SetTimesTxt();
                break;
            case Module_Union.EventUnionCardReward:
                var point = Util.Parse<int>(e.param1.ToString());
                DelayEvents.Add(() =>
                {
                    moduleGlobal.ShowMessage(string.Format(ConfigText.GetDefalutString(629, 15), point));
                }, 0.5f);
                SetTimesTxt();
                break;
        }
    }
    protected override void OnReturn()
    {
        if (!returnMain)
        {
            returnMain = true;
            m_rulePlane.gameObject.SetActive(false);
        }
        else base.OnReturn();
    }
}
