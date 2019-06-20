/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-05-24
 * 
 ***************************************************************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Window_PVP : Window
{
    private const string AudioCountDown_5 = "";//5秒倒计时音效
    private const string MINE_BG_IMAGE = "ui_pvp_left{0}bg";
    private const string ENEMY_BG_IMAGE = "ui_pvp_right{0}bg";

    #region 公共
    private RectTransform mainPanel;
    //left
    private Transform m_bgImage;
    private Text m_roleName;//玩家名
    private Text m_level;//玩家等级
    private Text m_totalMatchOrScore;//战斗总场次(in自由);积分(in排位赛)
    private Text m_winMatchOrRank;//胜利场次(in自由);排名(in排位赛)
    private Text m_winRate;//胜率(in自由);排位赛需隐藏
    private Transform m_scoreleft;
    private Transform m_rankleft;

    //right
    private Creature enemy;
    private Creature enemyPet;
    private Transform e_bagImage;
    private Text e_roleName;
    private Text e_level;
    private Text e_score;
    private Text e_rank;
    private Text e_winRate;

    //middle
    private RectTransform time_parent;
    private Text countDown_time;//倒计时 包括匹配30秒倒计时 --邀请好友倒计时
    private Text decriptionForTime;//索敌中 --准备战斗 
    private float timeForCountDown;
    private Button cancleMatchBtn;
    #endregion

    #region 自由匹配
    private RectTransform freeMatchPanel;
    private Button fastMatchBtn;//自由匹配
    private Button customMatchBtn;//自定义匹配
    private RectTransform invitePanel;
    private Transform vsTran;
    #endregion

    #region 排位赛
    private RectTransform logosInMainPanel;
    private Transform lolBtnPanel;
    private Button rankingInMainBtn;
    private Button danlvInMainBtn;
    private Button decriptInMainBtn;
    private Transform logosEnemy;

    //奖励界面
    List<PvpLolInfo> allPvpLolInfo = new List<PvpLolInfo>();
    ScrollView rewardView;

    private RectTransform settlementPanel;
    private Button rankingInSettlementBtn;
    private GameObject rewardItem;
    private RectTransform reward_logos;
    private Text reward_name;
    private Text reward_integral;
    private Text reward_ranking;

    //详细排名界面
    private RectTransform rankingPanel;
    DataSource<PRank> rankingData;
    ScrollView rankingView;
    #endregion

    #region 邀请好友
    private Button m_InviteFriend;
    private DataSource<PPlayerInfo> OnFriendList;
    private ScrollView m_inviteScroll;
    private RectTransform m_inviteNothing;
    private bool Is_Filed = false;
    public float m_remianOne;
    #endregion

    private enum State
    {
        Normal = 0,
        Matching = 1,
        MatchSuccess = 2,
    }
    private State state;

    protected override void OnOpen()
    {
        #region 公共
        mainPanel = GetComponent<RectTransform>("panel_loading");
        //left
        m_bgImage = GetComponent<Transform>("panel_loading/left/bg_left");
        m_roleName = GetComponent<Text>("panel_loading/left/bg_left/name_left/name");
        m_level = GetComponent<Text>("panel_loading/left/bg_left/name_left/level");

        m_totalMatchOrScore = GetComponent<Text>("panel_loading/left/bg_left/score_left/score");
        m_winMatchOrRank = GetComponent<Text>("panel_loading/left/bg_left/rank_left/ranklevel");
        m_winRate = GetComponent<Text>("panel_loading/left/bg_left/rank_left/ranklevel/Text");

        m_scoreleft = GetComponent<RectTransform>("panel_loading/left/bg_left/score_left");
        m_rankleft = GetComponent<RectTransform>("panel_loading/left/bg_left/rank_left");

        //right
        e_bagImage = GetComponent<Transform>("panel_loading/right/bg_right");
        e_roleName = GetComponent<Text>("panel_loading/right/bg_right/name_right/name");
        e_level = GetComponent<Text>("panel_loading/right/bg_right/name_right/level");
        e_score = GetComponent<Text>("panel_loading/right/bg_right/score_left/score");
        e_rank = GetComponent<Text>("panel_loading/right/bg_right/rank_left/ranklevel");
        e_winRate = GetComponent<Text>("panel_loading/right/bg_right/rank_left/ranklevel/Text");

        //middle
        time_parent = GetComponent<RectTransform>("panel_loading/timer_frame");
        countDown_time = GetComponent<Text>("panel_loading/timer_frame/Image/countDown_time");
        decriptionForTime = GetComponent<Text>("panel_loading/timer_frame/decription_text");
        cancleMatchBtn = GetComponent<Button>("panel_loading/timer_frame/cancel_Btn");

        cancleMatchBtn.onClick.RemoveAllListeners();
        cancleMatchBtn.onClick.AddListener(delegate
        {
            if (modulePVP.opType == OpenWhichPvP.FriendPvP) moduleMatch.CancleFriendInvate();
            else moduleMatch.CancleMatch();
        });
        #endregion

        #region 自由匹配
        freeMatchPanel = GetComponent<RectTransform>("panel_loading/pipeiPanel");
        fastMatchBtn = GetComponent<Button>("panel_loading/pipeiPanel/fast_matching");
        customMatchBtn = GetComponent<Button>("panel_loading/pipeiPanel/custom_matching");
        invitePanel = GetComponent<RectTransform>("panel_loading/pipeiPanel/invite_panel");
        vsTran = GetComponent<Transform>("panel_loading/ranking");
        var _tween = vsTran.GetComponent<TweenAlpha>();
        if (_tween)
        {
            _tween.onComplete.RemoveAllListeners();
            _tween.onComplete.AddListener((b) =>
            {
                AudioManager.PlaySound(AudioInLogicInfo.audioConst.countDown);
            });
        }

        fastMatchBtn.onClick.RemoveAllListeners();
        fastMatchBtn.onClick.AddListener(() =>
        {
            if (modulePVP.opType == OpenWhichPvP.LolPvP)
            {
                moduleMatch.SendPvPRankMatch();
                return;
            }
            moduleMatch.Match();
        });
        customMatchBtn.onClick.RemoveAllListeners();
        customMatchBtn.onClick.AddListener(delegate
        {
            moduleMatch.m_invateCheck.Clear();
            moduleMatch.OnlineFriend();
            moduleMatch.AllRemainTime();
            m_inviteScroll.SafeSetActive(moduleMatch.m_friendOnline.Count > 0);
            m_inviteNothing.SafeSetActive(moduleMatch.m_friendOnline.Count == 0);
            OnFriendList.SetItems(moduleMatch.m_friendOnline);
        });
        #endregion

        #region 排位赛
        logosInMainPanel = GetComponent<RectTransform>("panel_loading/left/bg_left/rank_left/ranklevel/logo");
        lolBtnPanel = GetComponent<Transform>("panel_loading/lolPanel");
        danlvInMainBtn = GetComponent<Button>("panel_loading/lolPanel/danlvBtn");
        rankingInMainBtn = GetComponent<Button>("panel_loading/lolPanel/rankingBtn");
        decriptInMainBtn = GetComponent<Button>("panel_loading/lolPanel/decriptionBtn");
        logosEnemy = GetComponent<Transform>("panel_loading/right/bg_right/rank_left/ranklevel/logo");
        logosEnemy.SafeSetActive(true);

        danlvInMainBtn.onClick.RemoveAllListeners();
        danlvInMainBtn.onClick.AddListener(OnOpenAwardPanel);
        rankingInMainBtn.onClick.RemoveAllListeners();
        rankingInMainBtn.onClick.AddListener(() => RankingState(moduleMatch.detailRanking));

        //奖励界面
        allPvpLolInfo.Clear();
        allPvpLolInfo = ConfigManager.GetAll<PvpLolInfo>();
        rewardView = GetComponent<ScrollView>("panel_settlement/scroll");

        new DataSource<PvpLolInfo>(allPvpLolInfo, rewardView, OnSetRewardInfo);

        settlementPanel = GetComponent<RectTransform>("panel_settlement");
        rankingInSettlementBtn = GetComponent<Button>("panel_settlement/rank_btn");
        rewardItem = GetComponent<RectTransform>("panel_settlement/scroll/m_lanel").gameObject;
        reward_logos = GetComponent<RectTransform>("panel_settlement/person_cups");
        reward_name = GetComponent<Text>("panel_settlement/person_name/name");
        reward_integral = GetComponent<Text>("panel_settlement/frame_lanel/Text");
        reward_ranking = GetComponent<Text>("panel_settlement/pai_frame/Text");
        rankingInSettlementBtn.onClick.RemoveAllListeners();
        rankingInSettlementBtn.onClick.AddListener(() => RankingState(moduleMatch.detailRanking));

        //详细排名
        rankingPanel = GetComponent<RectTransform>("panel_ranking");
        rankingView = GetComponent<ScrollView>("panel_ranking/ranking_scroll");
        rankingData = new DataSource<PRank>(null, rankingView, OnSetDetailRankingItem);
        #endregion

        #region 邀请好友
        GetComponent<RectTransform>("panel_loading/timer_frame/friendcancel_Btn")?.SafeSetActive(false);
        m_InviteFriend = GetComponent<Button>("panel_loading/pipeiPanel/invite_panel/person_frame/invite_btn");//给服务器发选中人列表
        m_inviteScroll = GetComponent<ScrollView>("panel_loading/pipeiPanel/invite_panel/person_frame/scrollView");
        m_inviteNothing = GetComponent<RectTransform>("panel_loading/pipeiPanel/invite_panel/person_frame/nothing");

        m_InviteFriend.onClick.RemoveAllListeners();
        m_InviteFriend.onClick.AddListener(Oncheckfried);

        OnFriendList = new DataSource<PPlayerInfo>(moduleMatch.m_friendOnline, GetComponent<ScrollView>("panel_loading/pipeiPanel/invite_panel/person_frame/scrollView"), FriendInfo, FriendClick);
        #endregion

        IniteText();
    }

    private void IniteText()
    {
        ConfigText pvpText = ConfigManager.Get<ConfigText>((int)TextForMatType.MatchUIText);
        if (pvpText == null)
        {
            pvpText = ConfigText.emptey;
            Logger.LogError("id not find");
        }
        Util.SetText(GetComponent<Text>("panel_loading/pipeiPanel/invite_panel/person_frame/nothing/Text"), 223, 61);
        Util.SetText(GetComponent<Text>("panel_loading/pipeiPanel/fast_matching/Image"), pvpText[0]);
        Util.SetText(GetComponent<Text>("panel_loading/pipeiPanel/custom_matching/Image"), pvpText[1]);
        Util.SetText(GetComponent<Text>("panel_loading/timer_frame/ban/invation_text"), pvpText[7]);
        Util.SetText(GetComponent<Text>("panel_loading/pipeiPanel/invite_panel/person_frame/invite_btn/Image"), pvpText[9]);
        Util.SetText(GetComponent<Text>("panel_loading/lolPanel/danlvBtn/Text"), pvpText[10]);
        Util.SetText(GetComponent<Text>("panel_settlement/bg/frameBg/title"), pvpText[10]);
        Util.SetText(GetComponent<Text>("panel_settlement/bg/frameBg/title_shadow"), pvpText[10]);
        Util.SetText(GetComponent<Text>("panel_loading/lolPanel/rankingBtn/Text"), pvpText[14]);
        Util.SetText(GetComponent<Text>("panel_settlement/rank_btn/Image"), pvpText[14]);
        Util.SetText(GetComponent<Text>("panel_loading/pipeiPanel/invite_panel/person_frame/titleBg/title_Txt"), pvpText[19]);
        Util.SetText(GetComponent<Text>("panel_loading/lolPanel/decriptionBtn/Text"), pvpText[20]);
        Util.SetText(GetComponent<Text>("panel_settlement/frame_lanel/now"), pvpText[21]);
        Util.SetText(GetComponent<Text>("panel_settlement/pai_frame/now"), pvpText[22]);
        Util.SetText(GetComponent<Text>("panel_ranking/bg/title"), pvpText[23]);
        Util.SetText(GetComponent<Text>("panel_ranking/bg/titleBg/title_shadow"), pvpText[23]);
        Util.SetText(GetComponent<Text>("panel_ranking/bg/bg_frame/paiming_text"), pvpText[24]);
        Util.SetText(GetComponent<Text>("panel_ranking/bg/bg_frame/duanwei_text"), pvpText[25]);
        Util.SetText(GetComponent<Text>("panel_ranking/bg/bg_frame/wanjiaxinxi_text"), pvpText[26]);
        Util.SetText(GetComponent<Text>("panel_ranking/bg/bg_frame/jifen_text"), pvpText[27]);
        Util.SetText(GetComponent<Text>("panel_ranking/bg/bg_frame/gonghuimingcheng_text"), pvpText[28]);
        Util.SetText(GetComponent<Text>("panel_loading/timer_frame/friendcancel_Btn/Image"), pvpText[40]);
        Util.SetText(GetComponent<Text>("tip_notice/top/equipinfo"), pvpText[20]);
        Util.SetText(GetComponent<Text>("tip_notice/viewport/content"), 282);

        Util.SetText(GetComponent<Text>("panel_loading/timer_frame/cancel_Btn/Image"), 9, 1);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault(1, false);

        HideOthers();
        if (modulePVP.opType == OpenWhichPvP.LolPvP) moduleHome.DispatchEvent(Module_Home.EventSwitchCameraMode, Event_.Pop(CameraShowType.Rank));
        else moduleHome.DispatchEvent(Module_Home.EventSwitchCameraMode, Event_.Pop(CameraShowType.Pvp));

        m_roleName.text = modulePlayer.roleInfo.roleName;
        m_level.text = Util.Format("LV:{0}", modulePlayer.roleInfo.level);
        state = State.Normal;

        switch (modulePVP.opType)
        {
            case OpenWhichPvP.FreePvP  : SetFreePvpState(state, OpenWhichPvP.FreePvP); break;
            case OpenWhichPvP.LolPvP   : moduleMatch.RepareRankData(); break;
            case OpenWhichPvP.FriendPvP: InvationISsend(); break;
            default: break;
        }
    }

    protected override void OnHide(bool forward)
    {
        moduleHome.DispatchEvent(Module_Home.EventSwitchCameraMode, Event_.Pop(CameraShowType.Home));
    }

    private void HideOthers()
    {
        var level = Level.current as Level_Home;
        if (level == null) return;
        if(!level.master)
        {
            level.rankCamera.cullingMask &= ~(Layers.MASK_MODEL | Layers.MASK_WEAPON | Layers.MASK_JEWELRY);
            level.pvpCamera.cullingMask &= ~(Layers.MASK_MODEL | Layers.MASK_WEAPON | Layers.MASK_JEWELRY);
            return;
        }

        List<string> excludes = new List<string>();
        excludes.Add(Module_Home.FIGHTING_PET_OBJECT_NAME);
        excludes.Add(Module_Home.PLAYER_OBJECT_NAME);
        if (enemy != null) excludes.Add(enemy.uiName);
        if (enemyPet != null) excludes.Add(enemyPet.uiName);
        
        var info = ConfigManager.Get<ShowCreatureInfo>(11);
        ShowCreatureInfo.SizeAndPos size = null;
        if (info != null)
        {
            for (int i = 0; i < info.forData.Length; i++)
            {
                if (info.forData[i].index == modulePlayer.proto)
                {
                    size = info.forData[i].data[0];
                    break;
                }
            }
        }

        moduleHome.DispatchEvent(Module_Home.EventSetMasterPosition, Event_.Pop(size));
        moduleHome.HideOthersBut(excludes.ToArray());
    }

    protected override void OnReturn()
    {
        if (!settlementPanel.gameObject.activeInHierarchy && !invitePanel.gameObject.activeInHierarchy && freeMatchPanel.gameObject.activeInHierarchy)
        {
            modulePVP.opType = OpenWhichPvP.None;
            Hide();
        }
        else if (state == State.Matching)
            moduleMatch.CancleMatch();
        else
            SwitchToDefault();
    }

    #region  自由匹配
    private void SetFreePvpState(State state, OpenWhichPvP type)
    {
        SetActive(state, type);

        //从PVP回来打开界面
        if (modulePVP.isMatchAgain && modulePVP.opType == OpenWhichPvP.FreePvP)
        {
            fastMatchBtn.onClick.Invoke();
            modulePVP.isMatchAgain = false;
        }
    }

    private void Set_MineTextInfo()
    {
        Util.SetText(m_totalMatchOrScore, (int)TextForMatType.MatchUIText, 2, modulePlayer.roleInfo.pvpTimes);
        Util.SetText(m_winMatchOrRank, (int)TextForMatType.MatchUIText, 3, modulePlayer.roleInfo.pvpWinTimes);
        string percent = modulePlayer.roleInfo.pvpTimes == 0 ? "0 %" : ((float)modulePlayer.roleInfo.pvpWinTimes / (float)modulePlayer.roleInfo.pvpTimes).ToString("P0");
        Util.SetText(m_winRate, (int)TextForMatType.MatchUIText, 16, percent);
    }

    private void SetActive(State type, OpenWhichPvP _type)
    {
        m_scoreleft.SafeSetActive(_type != OpenWhichPvP.FriendPvP);
        m_rankleft.SafeSetActive(_type != OpenWhichPvP.FriendPvP);
        //normal
        freeMatchPanel.SafeSetActive(type == State.Normal);
        fastMatchBtn.SafeSetActive(type == State.Normal);

        lolBtnPanel.SafeSetActive(type == State.Normal);
        rankingInMainBtn.SafeSetActive(type == State.Normal);
        danlvInMainBtn.SafeSetActive(type == State.Normal);
        decriptInMainBtn.SafeSetActive(type == State.Normal);
        customMatchBtn.SafeSetActive(type == State.Normal && _type != OpenWhichPvP.LolPvP);
        bool isMine = type == State.Normal || type == State.Matching || type == State.MatchSuccess;
        mainPanel.SafeSetActive(isMine);
        m_bgImage.SafeSetActive(isMine);
        if (isMine)
        {
            string bg = Util.Format(MINE_BG_IMAGE, modulePlayer.proto);
            UIDynamicImage.LoadImage(m_bgImage, bg);
        }
        e_bagImage.SafeSetActive(type == State.MatchSuccess);
        bool m_isTrue = type == State.Normal || type == State.MatchSuccess;
        m_totalMatchOrScore.transform.parent.SafeSetActive(m_isTrue);
        m_winMatchOrRank.transform.parent.SafeSetActive(m_isTrue);

        bool m_freeMatch = _type != OpenWhichPvP.LolPvP && m_isTrue;
        m_winRate.SafeSetActive(m_freeMatch);
        if (m_freeMatch)
        {
            for (int i = 0; i < logosInMainPanel.childCount; i++)
                logosInMainPanel.GetChild(i).SafeSetActive(false);

            for (int i = 0; i < logosEnemy.childCount; i++)
                logosEnemy.GetChild(i).SafeSetActive(false);

            Set_MineTextInfo();
        }

        //matching
        time_parent.SafeSetActive(type == State.Matching);

        settlementPanel.SafeSetActive(false);
        rankingPanel.SafeSetActive(false);

        //matchSuccess
        vsTran.SafeSetActive(type == State.MatchSuccess);
        cancleMatchBtn.SafeSetActive(modulePlayer.pvpTimes > 0 && type != State.MatchSuccess);
    }

    #endregion

    #region 排位赛

    private void RefreshLolPvpState()
    {
        SetActive(state, OpenWhichPvP.LolPvP);
        Util.SetText(m_totalMatchOrScore, (int)TextForMatType.MatchUIText, 4, moduleMatch.rankInfo.score);
        SetTargetRank(m_winMatchOrRank, moduleMatch.rankInfo.rank, true);

        for (int i = 0; i < logosInMainPanel.childCount; i++)
            logosInMainPanel.GetChild(i).SafeSetActive(i + 1 == moduleMatch.rankInfo.danLv);

        //从排位赛回来再来一局
        if (modulePVP.isMatchAgain && modulePVP.opType == OpenWhichPvP.LolPvP)
        {
            fastMatchBtn.onClick.Invoke();
            modulePVP.isMatchAgain = false;
        }
    }

    private void SetTargetRank(Text target, ushort rank, bool isInMain)
    {
        if (rank > 0 && rank <= 100)
        {
            if (isInMain) Util.SetText(target, (int)TextForMatType.MatchUIText, 38, rank);
            else Util.SetText(target, rank.ToString());
        }
        else if (rank == 0)
        {
            Util.SetText(target, (int)TextForMatType.MatchUIText, 18, 1000);
        }
        else
        {
            char[] chars = rank.ToString().ToCharArray();
            string rankStr = chars[0].ToString();
            for (int i = 0; i < chars.Length - 1; i++)
            {
                rankStr = rankStr + "0";
            }
            Util.SetText(target, (int)TextForMatType.MatchUIText, 18, rankStr);
        }
    }

    private void OnOpenAwardPanel()
    {
        moduleGlobal.ShowGlobalLayerDefault(1, false);
        rewardView.progress = 0;

        reward_name.text = modulePlayer.roleInfo.roleName;
        reward_integral.text = moduleMatch.rankInfo.score.ToString();

        SetTargetRank(reward_ranking, moduleMatch.rankInfo.rank, false);
        for (int i = 0; i < reward_logos.childCount; i++)
            reward_logos.GetChild(i).SafeSetActive(i + 1 == moduleMatch.rankInfo.danLv);
    }

    private void OnSetRewardInfo(RectTransform node, PvpLolInfo data)
    {
        //显示每段积分
        Text integral_text = node.Find("j_frame/Image")?.GetComponent<Text>();
        if (integral_text) integral_text.text = data.integral.ToString();
        //高亮
        Transform highLight = node.Find("j_frame/check");
        if (highLight) highLight.SafeSetActive(data.ID == moduleMatch.rankInfo.danLv);

        Transform grid = node.Find("j_frame/grid");

        //每段的奖励物品
        int index = 0;
        string[] props = data.propIdAndNumber;
        if (props != null && props.Length > 0)
        {
            for (int k = 0; k < props.Length; k++)
            {
                if (props[k] == "")
                {
                    index = k;
                    continue;
                }
                Transform item = grid.childCount > index ? grid.GetChild(index) : null;
                if (item == null) item = grid.AddNewChild(rewardItem);
                item.SafeSetActive(true);
                string[] prop = props[k].Split('-');
                if (prop != null && prop.Length == 4)
                {
                    var info = ConfigManager.Get<PropItemInfo>(Util.Parse<int>(prop[0]));
                    Util.SetItemInfoSimple(item, info);
                    Text count = item.Find("Text")?.GetComponent<Text>();
                    if (count) count.text = prop[3];
                }
                index++;
            }
        }
    }

    private void OnSetDetailRankingItem(RectTransform node, PRank data)
    {
        RankingItem item = node.GetComponentDefault<RankingItem>();
        item.RefreshItem(data);
    }

    private void RankingState(List<PRank> rankings)
    {
        moduleGlobal.ShowGlobalLayerDefault(1, false);
        rankingData.SetItems(rankings);
        rankingView.progress = 0;
    }

    #endregion

    #region 邀请好友
    private void InvationISsend()
    {
        modulePVP.opType = OpenWhichPvP.FriendPvP;
        if (moduleMatch.beiInvated && moduleMatch.Info_sss != null)
            Invation_succed(moduleMatch.Info_sss.infoList, moduleMatch.Info_sss.room);
        else
        {
            timeForCountDown = 15;
            Is_Filed = true;
            state = State.Matching;
            SetActive(state, modulePVP.opType);
            Util.SetText(decriptionForTime, (int)TextForMatType.MatchUIText, 11);
        }
        moduleGlobal.ShowGlobalLayerDefault(2, false);
    }

    private void InvationFailed()
    {
        Is_Filed = false;
        modulePVP.opType = OpenWhichPvP.FreePvP;
        SwitchToDefault();
    }

    private void Oncheckfried()
    {
        //判定选中的好友有哪些
        if (moduleMatch.m_invateCheck.Count > 0 && moduleMatch.m_invateCheck.Count <= 5)
        {
            List<ulong> SendId = new List<ulong>();
            for (int i = 0; i < moduleMatch.m_invateCheck.Count; i++)
            {
                PPlayerInfo playerInfo = moduleFriend.FriendList.Find(a => a.roleId == moduleMatch.m_invateCheck[i].roleId);
                if (playerInfo == null || playerInfo.state != 1) continue;
                SendId.Add(moduleMatch.m_invateCheck[i].roleId);
            }
            moduleMatch.FriendFree(SendId);
        }
        else
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.MatchUIText, 31));
    }

    private void FriendInfo(RectTransform rt, PPlayerInfo Info)
    {
        FriendPrecast Presct = rt.gameObject.GetComponentDefault<FriendPrecast>();
        Presct.DelayAddData(Info, 1, 1);
        rt.gameObject.GetComponentDefault<Button>().interactable = true;

        Text remainTxt = rt.Find("remain").GetComponent<Text>();
        remainTxt.SafeSetActive(false);

        int remain;
        moduleMatch.m_remainTime.TryGetValue(Info.roleId, out remain);

        if (remain > 0)
        {
            remainTxt.SafeSetActive(true);

            Util.SetText(remainTxt, ConfigText.GetDefalutString(219, 39), remain);
            rt.gameObject.GetComponentDefault<Button>().interactable = false;
        }

        GameObject selectbg = rt.gameObject.transform.Find("checkBg_Img").gameObject;
        GameObject checkbg = rt.gameObject.transform.Find("check_Img").gameObject;
        selectbg.SafeSetActive(false);
        checkbg.SafeSetActive(false);
        bool have = moduleMatch.m_invateCheck.Exists(a => a.roleId == Info.roleId);
        if (have)
        {
            selectbg.SafeSetActive(true);
            checkbg.SafeSetActive(true);
        }
    }

    private void FriendClick(RectTransform rt, PPlayerInfo Info)
    {
        GameObject select = rt.gameObject.transform.Find("selectbox").gameObject;
        GameObject selectbg = rt.gameObject.transform.Find("checkBg_Img").gameObject;
        GameObject checkbg = rt.gameObject.transform.Find("check_Img").gameObject;
        bool have = moduleMatch.m_invateCheck.Exists(a => a.roleId == Info.roleId);
        if (!select) return;
        if (select.activeInHierarchy)
        {
            select.SafeSetActive(false);
            selectbg.SafeSetActive(false);
            checkbg.SafeSetActive(false);
            if (have) moduleMatch.m_invateCheck.Remove(Info);
        }
        else
        {
            select.SafeSetActive(true);
            selectbg.SafeSetActive(true);
            checkbg.SafeSetActive(true);
            if (!have) moduleMatch.m_invateCheck.Add(Info);
        }
    }

    private void Invation_succed(PMatchInfo[] infolist, PRoomInfo room)
    {
        moduleMatch.beiInvated = false;
        moduleMatch.Info_sss = null;
        modulePVP.isInvation = true;
        Is_Filed = false;
        state = State.MatchSuccess;
        SetActive(state, modulePVP.opType);

        if (!modulePVP.connected || modulePVP.RoomId != room.room_key)
        {
            modulePVP.Connect(room);
            MatchSuccessState(infolist);
        }
    }

    #endregion

    void _ME(ModuleEvent<Module_Match> e)
    {
        if (!actived)
            return;
        switch (e.moduleEvent)
        {
            case Module_Match.EventComeBackHomeScence     : HideOthers(); break;
            case Module_Match.EventInvationSend           : InvationISsend(); break;
            case Module_Match.EventInvationFailed         : InvationFailed(); break;
            case Module_Match.EventFriendnMatchCancle     : InvationFailed(); break;
            case Module_Match.EventMatchInfo             : MatchSuccessState((e.msg as ScMatchInfo).infoList); break;
            case Module_Match.EventMatchCancle            : SwitchToDefault(); break;
            case Module_Match.EventMatchFailed            : moduleMatch.CancleMatch(); break;
            case Module_Match.EventDetailRankingForRequest: if (modulePVP.opType == OpenWhichPvP.LolPvP) RefreshLolPvpState(); break;
            case Module_Match.EventInvationsucced:
                var pp = e.msg as ScMatchInviteSuccess;
                Invation_succed(pp.infoList, pp.room);
                break;
            case Module_Match.EventMatchSuccessed://成功进入匹配状态
                if (state != State.MatchSuccess)
                {
                    state = State.Matching;
                    timeForCountDown = (ushort)e.param1;
                    Logger.LogInfo("server match time is {0}", timeForCountDown);

                    //set cancel button invisible when we in guide
                    cancleMatchBtn.SafeSetActive(modulePlayer.pvpTimes > 0);

                    moduleGlobal.ShowGlobalLayerDefault(2, false);
                    Util.SetText(decriptionForTime, (int)TextForMatType.MatchUIText, 5);
                }
                break;
            default: break;
        }
    }

    void _ME(ModuleEvent<Module_PVP> e)
    {
        if (!actived)
            return;
        switch (e.moduleEvent)
        {
            case Module_PVP.EventLoadingStart  : if (timeForCountDown < 1 || state != State.MatchSuccess) Game.LoadLevel(4); break;
            case Module_PVP.EventEnterMatchRoom: if (!modulePVP.entered) SwitchToDefault(); break;
            default: break;
        }
    }

    private void MatchSuccessState(PMatchInfo[] info)
    {
        moduleGlobal.ShowGlobalLayerDefault(2, false);
        state = State.MatchSuccess;

        SetActive(state, modulePVP.opType);
        timeForCountDown = 7;
        Util.SetText(decriptionForTime, (int)TextForMatType.MatchUIText, 6);

        for (int i = 0; i < info.Length; i++)
        {
            if (modulePlayer.id_ != info[i].roleId)
            {
                e_roleName.text = info[i].roleName;
                e_level.text = Util.Format("LV:{0}", info[i].level);
                string bg = Util.Format(ENEMY_BG_IMAGE, info[i].roleProto);
                UIDynamicImage.LoadImage(e_bagImage, bg);

                m_scoreleft.SafeSetActive(modulePVP.opType != OpenWhichPvP.FriendPvP);
                m_rankleft.SafeSetActive(modulePVP.opType != OpenWhichPvP.FriendPvP);
                if (modulePVP.opType == OpenWhichPvP.LolPvP)
                {
                    e_score.transform.parent.SafeSetActive(true);
                    e_rank.transform.parent.SafeSetActive(true);
                    Util.SetText(e_score, (int)TextForMatType.MatchUIText, 4, info[i].score);
                    SetTargetRank(e_rank, info[i].rank, true);
                    e_winRate.SafeSetActive(false);

                    RefreshEnemyLogo(info[i].score);
                }
                else if (modulePVP.opType == OpenWhichPvP.FreePvP)
                {
                    e_score.transform.parent.SafeSetActive(true);
                    e_rank.transform.parent.SafeSetActive(true);
                    Util.SetText(e_score, (int)TextForMatType.MatchUIText, 2, info[i].pvpTimes);
                    Util.SetText(e_rank, (int)TextForMatType.MatchUIText, 3, info[i].pvpWinTimes);
                    string percent = info[i].pvpTimes == 0 ? "0 %" : ((float)info[i].pvpWinTimes / (float)info[i].pvpTimes).ToString("P0");
                    Util.SetText(e_winRate, (int)TextForMatType.MatchUIText, 16, percent);
                    e_winRate.SafeSetActive(true);
                }
                else
                {
                    e_score.transform.parent.SafeSetActive(false);
                    e_rank.transform.parent.SafeSetActive(false);
                    e_winRate.SafeSetActive(false);
                }

                CreatEnemyCreature(info[i]);
                break;
            }
        }
    }

    private void RefreshEnemyLogo(uint secore)
    {
        var info= allPvpLolInfo.Find(p => secore < p.integral);
        int danlv = 0;
        if (info != null) danlv = info.ID - 1 < 1 ? 1 : info.ID;
        else danlv = allPvpLolInfo[allPvpLolInfo.Count - 1].ID;

        for (int i = 0; i < logosEnemy.childCount; i++)
            logosEnemy.GetChild(i).SafeSetActive(i + 1 == danlv);
    }

    private void CreatEnemyCreature(PMatchInfo info)
    {
        //创建敌人
        ShowCreatureInfo showInfo = ConfigManager.Get<ShowCreatureInfo>(10);
        Vector3_ pos = new Vector3_(1.3, 0, 0);
        Vector3 rot = new Vector3(0, -110, 0);
        if (showInfo != null)
        {
            for (int i = 0; i < showInfo.forData.Length; i++)
            {
                if (showInfo.forData[i].index == info.roleProto)
                {
                    pos = showInfo.forData[i].data[0].pos;
                    rot = showInfo.forData[i].data[0].rotation;
                    break;
                }
            }
        }

        var weaponInfo = ConfigManager.Get<PropItemInfo>(info.fashion.weapon);
        if (weaponInfo == null) return;

        moduleGlobal.LockUI("", 0.5f);
        Level.PrepareAssets(Module_Battle.BuildPlayerSimplePreloadAssets(info), (r) =>
        {
            if (!r) return;

            enemy = moduleHome.CreatePlayer(info, pos, CreatureDirection.BACK);
            enemy.transform.localEulerAngles = rot;
            moduleGlobal.UnLockUI();
        });
        if (info.pet != null && info.pet.itemTypeId != 0)
        {
            var rPet = PetInfo.Create(info.pet);
            var assets = new List<string>();
            Level.PrepareAssets(Module_Battle.BuildPetSimplePreloadAssets(rPet, assets, 2), b =>
            {
                var rGradeInfo = rPet.UpGradeInfo;
                var show = ConfigManager.Get<ShowCreatureInfo>(rPet.ID);
                if (show == null)
                {
                    Logger.LogError("没有配置config_showCreatureInfo表。宠物ID = {0}, 没有出生位置信息。宠物模型创建失败", rPet.ID);
                    return;
                }
                var showData = show.GetDataByIndex(0);
                var data = showData.data.GetValue<ShowCreatureInfo.SizeAndPos>(0);
                enemyPet = moduleHome.CreatePet(rGradeInfo, pos + data.pos, data.rotation, Level.current.startPos, true, Module_Home.FIGHTING_PET_OBJECT_NAME + "_Enemy");
                enemyPet.transform.localScale *= data.size;
                enemyPet.transform.localEulerAngles = data.rotation;
            });
        }
    }

    public override void OnRenderUpdate()
    {
        //刷新是否能选中
        if (invitePanel.gameObject.activeInHierarchy && OnFriendList != null)
        {
            m_remianOne += Time.deltaTime;
            if (m_remianOne > 1)
            {
                m_remianOne = 0;
                for (int i = 0; i < moduleMatch.m_friendOnline.Count; i++)
                    moduleMatch.m_remainTime[moduleMatch.m_friendOnline[i].roleId]--;
                OnFriendList.UpdateItems();
            }
        }
        else m_remianOne = 0;

        //是否选中好友
        bool checekfreind = false;
        m_InviteFriend.SetInteractable(false);

        if (moduleMatch.m_invateCheck.Count > 0)
        {
            checekfreind = true;
        }

        if (checekfreind)
            m_InviteFriend.SetInteractable(true);
        else
            m_InviteFriend.SetInteractable(false);

        if (state == State.Matching) //处于匹配状态
        {
            timeForCountDown -= Time.deltaTime * 0.8f;
            countDown_time.text = ((int)timeForCountDown).ToString();

            if (timeForCountDown < 1)
            {
                state = State.Normal;
                if (Is_Filed)
                {
                    moduleMatch.Friedreplay();
                    Is_Filed = false;
                    return;
                }
                moduleMatch.CancleMatch();
            }
        }
        else if (state == State.MatchSuccess)  //匹配成功
        {
            timeForCountDown -= Time.deltaTime;
            if (timeForCountDown <= 5)
            {
                time_parent.SafeSetActive(true);
                countDown_time.text = ((int)timeForCountDown).ToString();
            }
            if (timeForCountDown < 0)
            {
                if (modulePVP.loading)
                {
                    Game.LoadLevel(4);
                    state = State.Normal;
                }
                else
                {
                    moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.MatchUIText, 32));

                    AudioManager.Pause(AudioCountDown_5);
                    if (modulePVP.opType == OpenWhichPvP.FriendPvP)
                        modulePVP.opType = OpenWhichPvP.FreePvP;

                    if (modulePVP.opType == OpenWhichPvP.FreePvP)
                    {
                        modulePlayer.roleInfo.pvpWinTimes++;
                        modulePlayer.pvpTimes++;
                    }

                    SwitchToDefault();
                }
            }
        }
    }

    private void SwitchToDefault()
    {
        state = State.Normal;
        switch (modulePVP.opType)
        {
            case OpenWhichPvP.FriendPvP:
            case OpenWhichPvP.FreePvP: SetFreePvpState(state, OpenWhichPvP.FreePvP); break;//自由匹配
            case OpenWhichPvP.LolPvP : RefreshLolPvpState(); break;//排位赛
            default: break;
        }

        moduleGlobal.ShowGlobalLayerDefault(1, false);
        moduleHome.HideOthersBut(Module_Home.PLAYER_OBJECT_NAME, Module_Home.FIGHTING_PET_OBJECT_NAME);
        if (enemy) GameObject.Destroy(enemy.gameObject);
        if (enemyPet) GameObject.Destroy(enemyPet.gameObject);
        enemy = null;
        enemyPet = null;
    }
}
