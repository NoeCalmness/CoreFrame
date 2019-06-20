/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-02-28
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Wish : Window
{
    /// <summary>
    ///  抽卡 NPC 状态 ID
    /// </summary>
    private const int CREATURE_STATE_WISH = 3001;
    /// <summary>
    ///  抽卡 NPC 站立 状态 ID
    /// </summary>
    private const int CREATURE_STATE_IDLE = 2001;

    private Button m_btnAddWishCoin, m_btnWish,m_btnWishTen, m_popOK,m_ruleBtn;
    private Button m_btnWish2, m_btnWishTen2;
    private GameObject m_itemTemplate, m_wish;
    private Creature m_npc;
    private WishItemInfo m_sss;
    private TweenAlpha m_ta, m_ta2, m_tad;
    private TweenColor m_tc;
    private TweenAlphaParticle m_tb;
    private DataSource<PWishItemDropInfo> m_dropInfo;
    private RewardShow m_rewardShow;
    private ScWishWish m_msgCache;
    private Button m_btnSkipAnimation;
    private List<PItem2> m_itemList;
    private Text m_popUpContext;
    private uint currentTimes;
    private byte wishTimes;
    private bool m_skipAnimation;
    /// <summary>
    /// 是否处于许愿展示状态
    /// </summary>
    private bool m_showing;

    protected override void OnOpen()
    {
        m_npc = null;

        GetComponent<NpcMono>("content/role_render")?.InitAction(c =>
        {
            m_npc = c;
            if (m_npc) m_npc.AddEventListener(CreatureEvents.ENTER_STATE, OnNpcEnterState);
        });

        m_btnSkipAnimation     = GetComponent<Button>("mask");
        m_btnAddWishCoin       = GetComponent<Button>("topLayer/wishCoin/add");
        m_btnWish              = GetComponent<Button>("content/wish_Btn");
        m_btnWishTen           = GetComponent<Button>("content/wish_Btn02");
        m_ruleBtn              = GetComponent<Button>("content/activity_Btn");
        m_wish                 = GetComponent<Transform>("content/wish_Btn").gameObject;
        m_popOK                = GetComponent<Button>("topLayer/popup/yes");
        m_sss                  = GetComponent<WishItemInfoSSS>("topLayer/tips/jipin");
        m_itemTemplate         = GetComponent<RectTransform>("content/dropInfo/info/inner/items/template").gameObject;
        m_ta                   = GetComponent<TweenAlpha>("content");
        m_ta2                  = GetComponent<TweenAlpha>("topLayer/wishCoin");
        m_tb                   = GetComponent<TweenAlphaParticle>("content/wish_Btn/effect");
        m_tc                   = GetComponent<TweenColor>("npc");
        m_tad                  = GetComponent<TweenAlpha>("content/dropInfo");
        m_rewardShow           = GetComponent<RewardShow>("topLayer/tips");
        m_popUpContext         = GetComponent<Text>("topLayer/popup/content1/info");

        m_btnWish2             = GetComponent<Button>("topLayer/tips/wish_goods/GameObject/wish_Btn");
        m_btnWishTen2          = GetComponent<Button>("topLayer/tips/wish_goods/GameObject/wish_Btn02");

        m_itemTemplate.SetActive(false);

        if(null != m_rewardShow)
        {
            m_rewardShow.OnClose += () =>
            {
                HighlightNpc(false);
                Module_Guide.AddSpecialTweenEndCondition();
                moduleGlobal.ShowGlobalTopRightBar(false);
            };
            m_rewardShow.onAnimEnd.AddListener(() =>
            {
                m_ta2.Play(false);
                m_btnWish2.transform.parent.SafeSetActive(!moduleGuide.inGuideProcess);
            });
        }
        m_popOK?.onClick.AddListener(() =>
        {
            if (wishTimes > 0)
            {
                moduleWish.ImFeelingLucky(wishTimes, true);
                wishTimes = 0;
            }
            else
                moduleWish.BuyWishCoin(currentTimes, true);
        });
        m_ruleBtn.onClick.AddListener(()=> { m_wish.SetActive(false); });

        m_btnAddWishCoin?.onClick.AddListener(() =>
        {
            UpdateGemText(1, false);
        });

        m_btnWish?.onClick.AddListener(() => { OnWishClick(1); });
        m_btnWish2?.onClick.AddListener(() => { OnWishClick(1); });
        m_btnWishTen?.onClick.AddListener(() => { OnWishClick(10); });
        m_btnWishTen2?.onClick.AddListener(() => { OnWishClick(10); });

        m_btnSkipAnimation.SafeSetActive(false);
        m_btnSkipAnimation?.onClick.AddListener(() =>
        {
            m_skipAnimation = true;
            m_npc.stateMachine.TranslateToID(CREATURE_STATE_IDLE);
        });

        InitializeText();
        UpdateWishCoinText();

        m_rewardShow?.Regirest(IsSSS, m_sss);

        m_dropInfo = new DataSource<PWishItemDropInfo>(moduleWish.dropInfo, GetComponent<ScrollView>("content/dropInfo/info/inner/items"), SetDropItem);
    }

    private void OnWishClick(byte rTimes)
    {
        if (modulePlayer.wishCoinCount < moduleWish.wishCost * rTimes)
        {
            wishTimes = 0;
            m_btnAddWishCoin.onClick.Invoke();
            UpdateGemText((uint)moduleWish.wishCost * rTimes - modulePlayer.wishCoinCount);
        }
        else
        {
            wishTimes = rTimes;
            UpdateWishCoinText();
            moduleWish.ImFeelingLucky(rTimes, true);
            wishTimes = 0;
        }
    }

    private bool IsSSS(PItem2 arg)
    {
        var item = m_msgCache.items.Find(p => p.item.itemTypeId.Equals(arg?.itemTypeId));
        return item != null && item.sss;
    }


    private void SetDropItem(RectTransform rt, PWishItemDropInfo data)
    {
        var item = ConfigManager.Get<PropItemInfo>(data.itemID);

        Util.SetItemInfoSimple(rt, item);
        Util.SetText(rt.GetComponent<Text>("rate"), 9406, (data.dropRate * 100).ToString("F2"));
    }

    protected override void OnReturn()
    {
        if (m_showing)
        {
            m_btnSkipAnimation.onClick.Invoke();
            return;
        }

        if (m_rewardShow?.gameObject?.activeSelf ?? false)
        {
            m_rewardShow.OnReturn();
            return;
        }

        if (m_tad.name != "1")
        {
            Hide();
//            GetComponent<TweenAlphaParticle>("content/wish_Btn/effect").PlayForward();
        }
        else
        {
            m_tad.name = "0";
            m_tad.PlayReverse();
        }
        m_wish.SetActive(true);
    }

    protected override void _Show(bool forward, bool immediately)
    {
        base._Show(forward, immediately);
        GetComponent<TweenAlphaParticle>("content/wish_Btn/effect")?.PlayReverse();
    }

    private void InitializeText()
    {
        var t = ConfigManager.Get<ConfigText>(9400);
        if (!t) return;

        Util.SetText(GetComponent<Text>("content/activity_Btn/Text"), t[0]);
        Util.SetText(GetComponent<Text>("content/wish_Btn/Text"),     t[1]);
        Util.SetText(GetComponent<Text>("content/wish_Btn02/Text"),     t[7]);
        Util.SetText(GetComponent<Text>("content/wishInfo"),          t[2]);
        Util.SetText(GetComponent<Text>("topLayer/tips/wish_goods/GameObject/wishInfo"),          t[2]);

        Util.SetText(GetComponent<Text>("content/dropInfo/info/title"),                                            t[0]);
        Util.SetText(GetComponent<Text>("content/dropInfo/info/inner/wish_rules/Viewport/Content/rulestitle"),     t[4]);
        Util.SetText(GetComponent<Text>("content/dropInfo/info/inner/wish_rules/Viewport/Content/rulescontent"),   t[5]);
        Util.SetText(GetComponent<Text>("content/dropInfo/info/inner/dropTitle"),                                  t[6]);

        t = ConfigManager.Get<ConfigText>(9401);
        if (!t) return;
        Util.SetText(m_popOK.gameObject.GetComponent<Text>("Text"),       t[0]);
        Util.SetText(GetComponent<Text>("topLayer/popup/content0/info"),  t[1]);
        Util.SetText(GetComponent<Text>("topLayer/popup/content0/cost"),  t[3]);
        Util.SetText(GetComponent<Text>("topLayer/popup/content1/cost"),  t[3]);

        t = ConfigManager.Get<ConfigText>(9404);
        Util.SetText(GetComponent<Text>("topLayer/tips/wish_goods/back/title/up_h/up_1"), t[1].Substring(0, 1));
        Util.SetText(GetComponent<Text>("topLayer/tips/wish_goods/back/title/up_h/up_2"), t[1].Substring(1, 1));
        Util.SetText(GetComponent<Text>("topLayer/tips/wish_goods/back/title/up_h/up_3"), t[1].Substring(2, 1));
        Util.SetText(GetComponent<Text>("topLayer/tips/wish_goods/back/title/up_h/up_4"), t[1].Substring(3, 1));
        Util.SetText(GetComponent<Text>("topLayer/tips/wish_goods/back/des"), t[4]);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault(1, false);
    }

    private void HighlightNpc(bool highlight = true)
    {
//        inputState = !highlight;

        m_ta?.Play(highlight);
        m_ta2?.Play(highlight);
        m_tb?.Play(highlight);
        m_tc?.Play(highlight);
        moduleGlobal.OnGlobalTween(highlight, 1);
    }

    private void OnNpcEnterState(Event_ e)
    {
        var old = e.param1 as StateMachineState;
        if (!m_rewardShow || !old || old.ID != CREATURE_STATE_WISH) return;
        m_showing = false;

        m_rewardShow.Show(m_itemList, false, m_skipAnimation);
        m_btnSkipAnimation.SafeSetActive(false);
    }

    private void UpdateWishCoinText()
    {
        Util.SetText(GetComponent<Text>("topLayer/wishCoin/text"), modulePlayer.wishCoinCount.ToString());
        Util.SetText(GetComponent<Text>("topLayer/popup/content0/cost/icon/now"), 9401, moduleWish.wishCost * wishTimes > modulePlayer.wishCoinCount ? 5 : 4, moduleWish.wishCost * wishTimes, modulePlayer.wishCoinCount);
        Util.SetText(GetComponent<Text>("content/wishInfo/cost"), 9400, 3, moduleWish.wishCost);
        Util.SetText(GetComponent<Text>("topLayer/tips/wish_goods/GameObject/wishInfo/cost"), 9400, 3, moduleWish.wishCost);
    }

    private void UpdateGemText(uint rTimes, bool checkEnough = true)
    {
        currentTimes = rTimes;
        var str = checkEnough && moduleWish.wishCost * rTimes > modulePlayer.wishCoinCount
            ? ConfigText.GetDefalutString(9401, 6)
            : string.Empty;
        Util.SetText(m_popUpContext, Util.Format( str + ConfigText.GetDefalutString(9401, 2),currentTimes));
        Util.SetText(GetComponent<Text>("topLayer/popup/content1/cost/icon/now"), 9401, moduleWish.buyWishCoinCost * rTimes <= modulePlayer.gemCount ? 4 : 5, moduleWish.buyWishCoinCost * rTimes, modulePlayer.gemCount);
    }

    private void ShowWishResult(ScWishWish result)
    {
        if (result == null || result.result > 1)
        {
            if (result != null) moduleGlobal.ShowMessage(9403, result.result - 2);
            return;
        }


        m_showing = true;

        if (m_itemList == null)
            m_itemList = new List<PItem2>();
        else
            m_itemList.Clear();

        foreach (var item in result.items)
            m_itemList.Add(item.item);

        result.CopyTo(ref m_msgCache);

        m_btnWish2.transform.parent.SafeSetActive(false);
        if (m_rewardShow.gameObject.activeSelf)
        {
            m_rewardShow.Clear();
            moduleGlobal.OnGlobalTween(true, 1);
            m_ta2.Play(true);
            m_rewardShow.Show(m_itemList);
            m_showing = false;
            return;
        }

        m_skipAnimation = false;
        if (m_npc)
        {
            m_npc.stateMachine.TranslateToID(CREATURE_STATE_WISH);
            m_btnSkipAnimation.SafeSetActive(true);
            HighlightNpc();
        }
        else
        {
            m_rewardShow.Show(m_itemList);
        }
    }

    #region Module event handlers

    void _ME(ModuleEvent<Module_Wish> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Wish.EventWishInfoUpdate: UpdateGemText(currentTimes); UpdateWishCoinText(); break;
            case Module_Wish.EventBuyWishCoin:
            {
                int code = (int)e.param1;
                if (code == 0) AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
                moduleGlobal.UnLockUI();
                moduleGlobal.ShowMessage(9402, code);
                break;
            }
            case Module_Wish.EventWishResult: moduleGlobal.UnLockUI(); ShowWishResult(e.msg as ScWishWish); break;
            case Module_Wish.EventDropInfoUpdate: m_dropInfo.SetItems(moduleWish.dropInfo); break;
            default: break;
        }
    }

    void _ME(ModuleEvent<Module_Player> e)
    {
        if (e.moduleEvent == Module_Player.EventCurrencyChanged)
        {
            var t = (CurrencySubType)e.param1;
            if (t == CurrencySubType.WishCoin) UpdateWishCoinText();
            if (t == CurrencySubType.Diamond)  UpdateGemText(currentTimes);
            
            return;
        }
    }

    #endregion
}
