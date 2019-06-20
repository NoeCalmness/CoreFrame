/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-08-07
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System;

public class Window_Settlement : Window
{
    #region 助战添加好友子界面

    private Image addPointIcon;
    private Text addPointName;
    private Text addPointValue;

    #endregion

    #region 公共
    private Button bgBtn;
    private Button m_againBtn;
    private Button tipCancleBtn;
    #endregion

    #region 胜利界面
    private RectTransform win_Panel;
    private Text win_levelText;
    private Image win_expbar;
    private float win_DelayTime;
    private Text win_expAddText;
    private Text win_expShowText;
    private Text win_goldAddText;
    private DataSource<PItem2> winReward;
    private ChaseStarPanel chaseStarpanel;
    private TargetMatrialBehaviour target;
    #endregion

    #region 失败界面
    private RectTransform lose_Panel;
    private RectTransform lose_pve;
    private Button lose_runeBtnForPvE;
    private Button lose_weaponBtnForPvE;
    private DataSource<PItem2> loseReward;

    private RectTransform lose_pvp;
    private Text lose_levelText;
    private Image lose_expbar;
    private float lose_DelayTime;
    private Text lose_expAddText;
    private Text lose_expShowText;
    private Text lose_goldAddText;
    #endregion

    #region 段位赛
    //胜利积分
    private RectTransform win_score;
    private Text win_last_score;
    private Text win_add_score;
    private Text win_dis_nextScore;
    private Text win_current_danLv;
    private Transform win_logos_parent;

    //失败积分
    private RectTransform lose_score;
    private Text lose_last_score;
    private Text lose_add_score;
    private Text lose_dis_nextScroe;
    private Text lose_current_danLv;
    private Transform lose_logos_parent;

    //积分面板
    private RectTransform upDanlvPanel;
    private Text currentlolLv;
    private Text currentScore;
    private Text nextLv;
    private Text nextScore;
    private Transform upDanLvPanelLogos;
    private Transform upDanLvPanelLogosTo;
    #endregion

    #region btnPanel
    private RectTransform btnPanel;

    //迷宫胜利
    private RectTransform labyrinthBtnParentPanel;
    private Button labyContinueBtn;
    private Button labyRestBtn;
    private Text m_labyProcessText;

    #endregion

    #region global_uplv
    private RectTransform m_uplvPanel;
    private Text beforeLv;
    private Text currentLv;
    private Text beforePotential;
    private Text currentPotential;
    private Text beforeFatigue;
    private Text currentFatigue;
    private Text beforeFatigueLimit;
    private Text currentFatigueLimit;

    private byte oldLv;
    private ushort oldPoint;
    private ushort oldFatigue;
    private ushort oldMaxFatigue;
    #endregion

    #region boss
    private bool m_boss = false;
    private float m_bossTime;
    private int m_bossLossTime;
    private GameObject m_bossWin;
    private Text m_myWinHurt;
    private Text m_allWinHurt;
    private Image m_WinValueBar;
    private GameObject m_bossLose;
    private Text m_myLoseHurt;
    private Text m_allLoseHurt;
    private Image m_loseValueBar;
    private Button m_lossRune;
    private Button m_lossForing;
    private Text m_lossTime;
    private List<GameObject> m_winList = new List<GameObject>();
    private List<GameObject> m_loseList = new List<GameObject>();
    #endregion

    protected override void OnOpen()
    {
        #region 公共
        bgBtn = GetComponent<Button>("end_btn");
        m_againBtn = GetComponent<Button>("again");
        tipCancleBtn = GetComponent<Button>("tips/top/button");
        tipCancleBtn.onClick.RemoveAllListeners();
        tipCancleBtn.onClick.AddListener(OnClickTipCancle);
        #endregion


        #region 胜利界面
        win_Panel = GetComponent<RectTransform>("win_panel");
        win_Panel.gameObject.SetActive(false);
        win_levelText = GetComponent<Text>("win_panel/content/center/exp/level");
        win_expbar = GetComponent<Image>("win_panel/content/center/exp/Fill Area/Fill");
        var win_tweenPosition = win_expbar.GetComponent<TweenPosition>();
        win_DelayTime = win_tweenPosition ? win_tweenPosition.delayStart : 0.8f;
        win_expAddText = GetComponent<Text>("win_panel/content/center/wxpreward/number_txt");
        win_expShowText = GetComponent<Text>("win_panel/content/center/exp/expcharactor");
        win_goldAddText = GetComponent<Text>("win_panel/content/preaward/gold_reward/number_text");
        winReward = new DataSource<PItem2>(null, GetComponent<ScrollView>("win_panel/content/preaward/reward"), OnSetRewardItem, OnClickRewardItem);
        chaseStarpanel = GetComponent<ChaseStarPanel>("starDesc");
        chaseStarpanel.gameObject.SetActive(false);

        target = GetComponentDefault<TargetMatrialBehaviour>("win_panel/content/target");

        addPointIcon = GetComponent<Image>("win_panel/content/preaward/friendshipPoint_reward/number_text/gold");
        addPointName = GetComponent<Text>("win_panel/content/preaward/friendshipPoint_reward/number_text/text");
        addPointValue = GetComponent<Text>("win_panel/content/preaward/friendshipPoint_reward/number_text");

        #endregion

        #region 失败界面
        //pve
        lose_Panel = GetComponent<RectTransform>("lose_panel");
        lose_Panel.gameObject.SetActive(false);
        lose_pve = GetComponent<RectTransform>("lose_panel/content/pvePanel");
        lose_pve.gameObject.SetActive(false);
        lose_runeBtnForPvE = GetComponent<Button>("lose_panel/content/pvePanel/rune_btn");
        lose_weaponBtnForPvE = GetComponent<Button>("lose_panel/content/pvePanel/weapon_btn");
        loseReward = new DataSource<PItem2>(null, GetComponent<ScrollView>("lose_panel/content/preaward/reward"), OnSetRewardItem, OnClickRewardItem);
        lose_runeBtnForPvE.onClick.RemoveAllListeners();
        lose_runeBtnForPvE.onClick.AddListener(OnOpenRuneWindow);
        lose_weaponBtnForPvE.onClick.RemoveAllListeners();
        lose_weaponBtnForPvE.onClick.AddListener(OnOpenWeaponWindow);

        //pvp
        lose_pvp = GetComponent<RectTransform>("lose_panel/content/pvpPanel");
        lose_pvp.gameObject.SetActive(false);
        lose_levelText = GetComponent<Text>("lose_panel/content/center/exp/level");
        lose_expbar = GetComponent<Image>("lose_panel/content/center/exp/Fill Area/Fill");
        var lose_tweenPosition = lose_expbar.GetComponent<TweenPosition>();
        lose_DelayTime = lose_tweenPosition ? lose_tweenPosition.delayStart : 0.8f;
        lose_expAddText = GetComponent<Text>("lose_panel/content/center/arrow1/exp_text");
        lose_expShowText = GetComponent<Text>("lose_panel/content/center/exp/expcharactor");
        lose_goldAddText = GetComponent<Text>("lose_panel/content/preaward/gold_reward/number_text");
        #endregion

        #region 段位赛
        //胜利积分
        win_score = GetComponent<RectTransform>("win_panel/content/preaward/pvpPanel");
        win_last_score = GetComponent<Text>("win_panel/content/preaward/pvpPanel/add_text/addscore");
        win_add_score = GetComponent<Text>("win_panel/content/preaward/pvpPanel/add_text/addscore/addscore_text");
        win_dis_nextScore = GetComponent<Text>("win_panel/content/preaward/pvpPanel/remain_text/remainscore");
        win_current_danLv = GetComponent<Text>("win_panel/content/preaward/pvpPanel/bg/currentrank");
        win_logos_parent = GetComponent<Transform>("win_panel/content/preaward/pvpPanel/logo");

        //失败积分
        lose_score = GetComponent<RectTransform>("lose_panel/content/pvpPanel/score");
        lose_last_score = GetComponent<Text>("lose_panel/content/pvpPanel/score/add_text/addscore");
        lose_add_score = GetComponent<Text>("lose_panel/content/pvpPanel/score/add_text/addscore/addscore_text");
        lose_dis_nextScroe = GetComponent<Text>("lose_panel/content/pvpPanel/score/remain_text/remainscore");
        lose_current_danLv = GetComponent<Text>("lose_panel/content/pvpPanel/score/currentrank");
        lose_logos_parent = GetComponent<Transform>("lose_panel/content/pvpPanel/score/logo");

        //积分rectTransform
        upDanlvPanel = GetComponent<RectTransform>("integralUpPanel");
        currentlolLv = GetComponent<Text>("integralUpPanel/nowLol");
        currentScore = GetComponent<Text>("integralUpPanel/nowIntegral");
        nextLv = GetComponent<Text>("integralUpPanel/nextLol");
        nextScore = GetComponent<Text>("integralUpPanel/nextIntegral");
        upDanLvPanelLogos = GetComponent<Transform>("integralUpPanel/logo");
        upDanLvPanelLogosTo = GetComponent<Transform>("integralUpPanel/logo_to");
        #endregion

        #region btnPanel
        btnPanel = GetComponent<RectTransform>("tips");

        //迷宫
        labyrinthBtnParentPanel = GetComponent<RectTransform>("tips/migong_win");
        labyrinthBtnParentPanel.gameObject.SetActive(false);
        labyContinueBtn = GetComponent<Button>("tips/migong_win/continue_btn");
        labyRestBtn = GetComponent<Button>("tips/migong_win/exit_btn");
        m_labyProcessText = GetComponent<Text>("tips/migong_win/progress_Txt");
        #endregion

        #region global_uplv
        m_uplvPanel = GetComponent<RectTransform>("upLvPanel");
        beforeLv = GetComponent<Text>("upLvPanel/dengji/Text");
        currentLv = GetComponent<Text>("upLvPanel/dengji/Text1");
        beforePotential = GetComponent<Text>("upLvPanel/qiannengdian/Text");
        currentPotential = GetComponent<Text>("upLvPanel/qiannengdian/Text1");
        beforeFatigue = GetComponent<Text>("upLvPanel/tili/Text");
        currentFatigue = GetComponent<Text>("upLvPanel/tili/Text1");
        beforeFatigueLimit = GetComponent<Text>("upLvPanel/tiliLimit/Text");
        currentFatigueLimit = GetComponent<Text>("upLvPanel/tiliLimit/Text1");

        m_uplvPanel.gameObject.SetActive(false);
        #endregion

        #region boss
        m_boss = false;
        m_bossWin = GetComponent<RectTransform>("winBoss_panel").gameObject;
        m_myWinHurt = GetComponent<Text>("winBoss_panel/content/center/arrow1/damage01_text");
        m_allWinHurt = GetComponent<Text>("winBoss_panel/content/preaward/arrow2/damage02_text");
        m_WinValueBar = GetComponent<Image>("winBoss_panel/content/center/bossHpBarFilled_Img");

        m_bossLose = GetComponent<RectTransform>("loseBoss_panel").gameObject;
        m_myLoseHurt = GetComponent<Text>("loseBoss_panel/content/center/arrow1/exp_text");
        m_allLoseHurt = GetComponent<Text>("loseBoss_panel/content/preaward/gold_reward/number_text");
        m_loseValueBar = GetComponent<Image>("loseBoss_panel/content/center/bossHpBarFilled_Img");
        m_lossTime = GetComponent<Text>("loseBoss_panel/content/timeremain/gold_reward/number_text");

        m_lossRune = GetComponent<Button>("loseBoss_panel/content/pvePanel/rune_btn");
        m_lossForing = GetComponent<Button>("loseBoss_panel/content/pvePanel/weapon_btn");

        GetComponent<Image>("loseBoss_panel/content/preaward/gold_reward/gold").gameObject.SetActive(false);
        GetComponent<Text>("loseBoss_panel/content/preaward/gold_reward/number_text/text").gameObject.SetActive(false);

        m_lossRune.onClick.RemoveAllListeners();
        m_lossRune.onClick.AddListener(OnOpenRuneWindow);
        m_lossForing.onClick.RemoveAllListeners();
        m_lossForing.onClick.AddListener(OnOpenWeaponWindow);
        #endregion

        IniteText();
    }

    private void IniteText()
    {
        ConfigText publicText = ConfigManager.Get<ConfigText>((int)TextForMatType.PublicUIText);
        Util.SetText(GetComponent<Text>("tips/top/equipinfo"), publicText[6]);
        Util.SetText(GetComponent<Text>("integralUpPanel/text"), publicText[22]);
        Util.SetText(GetComponent<Text>("upLvPanel/text"), publicText[22]);
        Util.SetText(GetComponent<Text>("end_btn/text_tip"), publicText[29]);

        ConfigText settlementText = ConfigManager.Get<ConfigText>((int)TextForMatType.SettlementUIText);
        Util.SetText(GetComponent<Text>("win_panel/content/center/arrow1/Text"), settlementText[0]);
        Util.SetText(GetComponent<Text>("lose_panel/content/center/arrow1/Text"), settlementText[0]);
        Util.SetText(GetComponent<Text>("win_panel/content/preaward/arrow2/Text"), settlementText[1]);
        Util.SetText(GetComponent<Text>("lose_panel/content/preaward/gold_reward/number_text/text"), settlementText[2]);
        Util.SetText(GetComponent<Text>("win_panel/content/preaward/gold_reward/number_text/text"), settlementText[2]);
        Util.SetText(GetComponent<Text>("win_panel/content/pvpPanel/add_text"), settlementText[3]);
        Util.SetText(GetComponent<Text>("lose_panel/content/pvpPanel/score/add_text"), settlementText[3]);
        Util.SetText(GetComponent<Text>("win_panel/content/pvpPanel/remain_text"), settlementText[4]);
        Util.SetText(GetComponent<Text>("lose_panel/content/pvpPanel/score/remain_text"), settlementText[4]);
        Util.SetText(GetComponent<Text>("win_panel/content/pvpPanel/remain_text/remainscore/addscore_text"), settlementText[5]);
        Util.SetText(GetComponent<Text>("lose_panel/content/pvpPanel/score/remain_text/remainscore/addscore_text"), settlementText[5]);
        Util.SetText(GetComponent<Text>("lose_panel/content/pvpPanel/score/currentrank/Text"), settlementText[6]);
        Util.SetText(GetComponent<Text>("integralUpPanel/nowLol_text"), settlementText[6]);
        Util.SetText(GetComponent<Text>("integralUpPanel/nextLol_text"), settlementText[6]);
        Util.SetText(GetComponent<Text>("win_panel/content/pvpPanel/bg/currentrank/Text"), settlementText[6]);
        Util.SetText(GetComponent<Text>("lose_panel/content/preaward/arrow2/Text"), settlementText[7]);
        Util.SetText(GetComponent<Text>("integralUpPanel/bg/description"), settlementText[8]);
        Util.SetText(GetComponent<Text>("tips/pvp/migong_text"), settlementText[10]);
        Util.SetText(GetComponent<Text>("tips/pvp/exit_btn/Text"), settlementText[11]);
        Util.SetText(GetComponent<Text>("tips/pvp/continue_btn/Text"), settlementText[12]);
        Util.SetText(GetComponent<Text>("tips/migong_win/migong_text"), settlementText[13]);
        Util.SetText(GetComponent<Text>("tips/migong_win/exit_btn/Text"), settlementText[14]);
        Util.SetText(GetComponent<Text>("tips/migong_win/continue_btn/Text"), settlementText[15]);
        Util.SetText(GetComponent<Text>("upLvPanel/dengji/dengjishangxain"), settlementText[17]);
        Util.SetText(GetComponent<Text>("upLvPanel/qiannengdian/dengjishangxain"), settlementText[18]);
        Util.SetText(GetComponent<Text>("lose_panel/content/pvePanel/rune_btn/Text"), settlementText[21]);
        Util.SetText(GetComponent<Text>("lose_panel/content/pvePanel/weapon_btn/Text"), settlementText[22]);
        Util.SetText(GetComponent<Text>("upLvPanel/bg/description"), settlementText[23]);
        Util.SetText(GetComponent<Text>("upLvPanel/bg/description_01"), settlementText[24]);
        Util.SetText(GetComponent<Text>("win_panel/content/target/arrow3/Text"), settlementText[28]);
        Util.SetText(GetComponent<Text>("win_panel/content/target/get_txt"), settlementText[29]);
        Util.SetText(GetComponent<Text>("win_panel/content/target/need_txt"), settlementText[30]);
        Util.SetText(GetComponent<Text>("again/text_tip"), settlementText[31]);

        ConfigText attrText = ConfigManager.Get<ConfigText>((int)TextForMatType.AttributeUIText);
        Util.SetText(GetComponent<Text>("upLvPanel/liliang/dengjishangxain"), attrText[2]);
        Util.SetText(GetComponent<Text>("upLvPanel/jiqiao/dengjishangxain"), attrText[3]);
        Util.SetText(GetComponent<Text>("upLvPanel/tili/dengjishangxain"), attrText[4]);

        Util.SetText(GetComponent<Text>("winBoss_panel/content/center/arrow1/Text"), 242,163);
        Util.SetText(GetComponent<Text>("winBoss_panel/content/preaward/arrow2/Text"), 242, 164);
        Util.SetText(GetComponent<Text>("winBoss_panel/content/notice_Img/notice_Txt"), 242, 165);
        Util.SetText(GetComponent<Text>("loseBoss_panel/content/center/arrow1/Text"), 242, 163);
        Util.SetText(GetComponent<Text>("loseBoss_panel/content/preaward/arrow2/Text"), 242, 164);
        Util.SetText(GetComponent<Text>("loseBoss_panel/content/pvePanel/rune_btn/Text"), settlementText[21]);
        Util.SetText(GetComponent<Text>("loseBoss_panel/content/pvePanel/weapon_btn/Text"), settlementText[22]);
        Util.SetText(GetComponent<Text>("loseBoss_panel/content/timeremain/arrow2/Text"),242,162);

    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        if (modulePlayer.isLevelUp)
        {
            var _params = GetWindowParam<Window_Settlement>();
            if (_params != null)
            {
                oldLv = (byte)_params.param1;
                oldPoint = (ushort)_params.param2;
                oldFatigue = (ushort)_params.param3;
                oldMaxFatigue = (ushort)_params.param4;
            }
        }

        bgBtn.interactable = false;
        RefreshSettlementPanel();

        addPointValue?.transform.parent.SafeSetActive(modulePVE.addFriendPoint > 0 || modulePVE.addNpcPoint > 0);
        if (modulePVE.addFriendPoint > 0)
        {
            Util.SetText(addPointValue, $"+{modulePVE.addFriendPoint}");
            Util.SetText(addPointName, PropItemInfo.FriendPointProp.itemName);
            AtlasHelper.SetIcons(addPointIcon, PropItemInfo.FriendPointProp.icon);
        }
        else if (modulePVE.addNpcPoint > 0)
        {

            Util.SetText(addPointValue, $"+{modulePVE.addNpcPoint}");
            Util.SetText(addPointName, ConfigText.GetDefalutString(228, 1));
            AtlasHelper.SetIcons(addPointIcon, "s_npcrelationship");
        }
    }

    private void RefreshSettlementPanel()
    {
        var task = ConfigManager.Get<TaskInfo>(moduleGlobal.targetMatrial.dropInfo?.chaseId ?? 0);
        target.SafeSetActive(moduleGlobal.targetMatrial.isProcess &&
                             task?.stageId == modulePVE.stageId);

        #region pve
        if (Level.current.isPvE)
        {
            PReward reward = modulePVE.settlementReward;
            if (moduleLabyrinth.lastSneakPlayer != null) ShowSneakSettlement(reward);
            else if (moduleUnion.m_isUnionBossTask) ShowUnionBossSettlement(reward);
            else ShowNormalPVESettlement(reward);
        }
        #endregion

        #region pvp
        if (Level.current.isPvP)
        {
            lose_Panel.gameObject.SetActive(!modulePVP.isWinner);
            lose_pve.gameObject.SetActive(false);
            lose_pvp.gameObject.SetActive(!modulePVP.isWinner);
            win_Panel.gameObject.SetActive(modulePVP.isWinner);

            if (!modulePVP.isInvation)
            {
                RefreshReward(modulePVP.reward, modulePVP.isWinner);

                if (modulePVP.isWinner && modulePVP.opType == OpenWhichPvP.FreePvP)
                    modulePlayer.roleInfo.pvpWinTimes++;
            }
            else
            {
                RefreshReward(null, modulePVP.isWinner);
                RefreshForNoExp(modulePVP.isWinner);
            }

            //总场次+1
            if (!modulePVP.isInvation && modulePVP.opType == OpenWhichPvP.FreePvP)
                modulePlayer.pvpTimes++;

            OnInComplete();

            m_againBtn.SafeSetActive(!modulePVP.isInvation && modulePlayer.pvpTimes > 1);
            m_againBtn?.onClick.RemoveAllListeners();
            m_againBtn?.onClick.AddListener(OnOnceAgain);
        }
        #endregion

    }

    #region pve settlement

    private void ShowSneakSettlement(PReward reward)
    {
        lose_Panel.gameObject.SetActive(!moduleLabyrinth.isSneakSuccess);
        win_Panel.gameObject.SetActive(moduleLabyrinth.isSneakSuccess);

        lose_pvp.gameObject.SetActive(!moduleLabyrinth.isSneakSuccess);
        lose_pve.gameObject.SetActive(false);

        RefreshForNoExp(moduleLabyrinth.isSneakSuccess);
        RefreshReward(reward, moduleLabyrinth.isSneakSuccess);
        //必须在最后执行该方法
        moduleLabyrinth.OnSneakSettleOver();
        OnInComplete();
        m_againBtn.SafeSetActive(false);
    }

    private void ShowNormalPVESettlement(PReward reward)
    {
        if (modulePVE.isSendWin && modulePVE.needShowStar)
        {
            bgBtn.interactable = true;

            if (modulePVE.settlementTask.taskStarDetails?.Length > 0)
            {
                chaseStarpanel.gameObject.SetActive(true);

                chaseStarpanel.RefreshPanel(modulePVE.settlementTask, modulePVE.settlementStar);

                bgBtn.onClick.RemoveAllListeners();
                bgBtn.onClick.AddListener(() =>
                {
                    OpenExceptChasePanel(reward);
                    chaseStarpanel.gameObject.SetActive(false);
                    bgBtn.interactable = false;
                    OnInComplete();
                });
            }
            else
            {
                OpenExceptChasePanel(reward);
                OnInComplete();
            }
        }
        else
        {
            OpenExceptChasePanel(reward);
            OnInComplete();
        }

        m_againBtn.SafeSetActive((modulePVE?.currrentStage?.again ?? false) && Module_Chase.CheckChaseCondition(moduleChase.lastStartChase));
        m_againBtn?.onClick.RemoveAllListeners();
        m_againBtn?.onClick.AddListener(() =>
        {
            moduleChase.SendStartChase(moduleChase.lastStartChase);
        });
    }

    private void ShowUnionBossSettlement(PReward reward)
    {
        lose_Panel.gameObject.SetActive(false);
        win_Panel.gameObject.SetActive(false);
        lose_pvp.gameObject.SetActive(false);
        lose_pve.gameObject.SetActive(false);
        bgBtn.interactable = true;
        bgBtn.onClick.RemoveAllListeners();
        bgBtn.onClick.AddListener(() =>
        {
            m_boss = false;
            OnOpenNextPanel();
        });
        m_bossWin.gameObject.SetActive(modulePVE.isSendWin);
        m_bossLose.gameObject.SetActive(!modulePVE.isSendWin);
        SetBossInfo();
        m_againBtn.SafeSetActive(false);
    }

    #endregion

    private void OpenExceptChasePanel(PReward reward)
    {
        lose_Panel.gameObject.SetActive(!modulePVE.isSendWin);
        lose_pve.gameObject.SetActive(!modulePVE.isSendWin && !moduleUnion.m_isUnionBossTask);
        lose_pvp.gameObject.SetActive(false);
        win_Panel.gameObject.SetActive(modulePVE.isSendWin && !moduleUnion.m_isUnionBossTask);

        if (modulePVE.isSendWin && !moduleUnion.m_isUnionBossTask) RefreshReward(reward, true);
        else
        {
            RefreshForNoExp(false);

            lose_runeBtnForPvE.interactable = true;
            lose_weaponBtnForPvE.interactable = true;
        }
    }

    private void UpdateUpLvPanel()
    {
        m_uplvPanel.SafeSetActive(true);
        Util.SetText(beforeLv, oldLv.ToString());
        Util.SetText(currentLv, modulePlayer.level.ToString());
        Util.SetText(beforePotential, oldPoint.ToString());
        Util.SetText(currentPotential, modulePlayer.roleInfo.attrPoint.ToString());
        Util.SetText(beforeFatigue, oldFatigue.ToString());
        Util.SetText(currentFatigue, modulePlayer.roleInfo.fatigue.ToString());
        Util.SetText(beforeFatigueLimit, oldMaxFatigue.ToString());
        Util.SetText(currentFatigueLimit, modulePlayer.maxFatigue.ToString());

        modulePlayer.isLevelUp = false;
    }

    private void OnInComplete()
    {
        bgBtn.onClick.RemoveAllListeners();
        bgBtn.onClick.AddListener(() => OnOpenNextPanel());

        if (moduleNpc.isNpcLv && moduleNpc.lvupNpc != null)
        {
            moduleNpc.isNpcLv = false;
            //显示提升面板,要构建一个新的window,要传一个点击回调,在点击回调里调用剧情对话
            Window_NpcUpLv.ShowUpWindow(moduleNpc.lvupNpc, OnClickUpWindow);
        }

        if (win_Panel.gameObject.activeInHierarchy)
        {
            win_expbar.fillAmount = moduleLogin.lastExpProcess;
            if (moduleLogin.expProcess > moduleLogin.lastExpProcess)
                DOTween.To(x => win_expbar.fillAmount = x, moduleLogin.lastExpProcess, moduleLogin.expProcess, 0.5f).SetDelay(win_DelayTime).OnComplete(() =>
                {
                    moduleLogin.lastExpProcess = moduleLogin.expProcess;
                    bgBtn.interactable = true;
                });
            else if (moduleLogin.expProcess < moduleLogin.lastExpProcess)
            {
                DOTween.To(x => win_expbar.fillAmount = x, moduleLogin.lastExpProcess, 1, 0.25f).SetDelay(win_DelayTime).OnComplete(() =>
                {
                    AudioManager.PlaySound(AudioInLogicInfo.audioConst.sliderToFull);
                    DOTween.To(x => win_expbar.fillAmount = x, 0f, moduleLogin.expProcess, 0.25f).OnComplete(() =>
                    {
                        moduleLogin.lastExpProcess = moduleLogin.expProcess;
                        if (modulePlayer.isLevelUp) UpdateUpLvPanel();
                        bgBtn.interactable = true;
                    });
                });
            }
            else
            {
                DOTween.To(x => win_expbar.fillAmount = x, 0f, moduleLogin.expProcess, 0.5f).SetDelay(win_DelayTime).OnComplete(() =>
                {
                    bgBtn.interactable = true;
                });
            }
        }
        else if (lose_Panel.gameObject.activeInHierarchy)
        {
            lose_expbar.fillAmount = moduleLogin.lastExpProcess;
            if (moduleLogin.expProcess > moduleLogin.lastExpProcess)
                DOTween.To(x => lose_expbar.fillAmount = x, moduleLogin.lastExpProcess, moduleLogin.expProcess, 0.5f).SetDelay(lose_DelayTime).OnComplete(() =>
                {
                    moduleLogin.lastExpProcess = moduleLogin.expProcess;
                    bgBtn.interactable = true;
                });
            else if (moduleLogin.expProcess < moduleLogin.lastExpProcess)
            {
                DOTween.To(x => lose_expbar.fillAmount = x, moduleLogin.lastExpProcess, 1, 0.25f).SetDelay(lose_DelayTime).OnComplete(() =>
                {
                    AudioManager.PlaySound(AudioInLogicInfo.audioConst.sliderToFull);
                    DOTween.To(x => lose_expbar.fillAmount = x, 0f, moduleLogin.expProcess, 0.25f).OnComplete(() =>
                    {
                        moduleLogin.lastExpProcess = moduleLogin.expProcess;
                        if (modulePlayer.isLevelUp) UpdateUpLvPanel();
                        bgBtn.interactable = true;
                    });
                });
            }
            else
            {
                DOTween.To(x => lose_expbar.fillAmount = x, 0f, moduleLogin.expProcess, 0.5f).SetDelay(win_DelayTime).OnComplete(() =>
                {
                    bgBtn.interactable = true;
                });
            }
        }
    }

    private void RefreshReward(PReward reward, bool isWin)
    {
        RefreshForNoExp(isWin);
        if (isWin)
        {
            win_levelText.text = modulePlayer.roleInfo.level.ToString();
            win_expbar.fillAmount = moduleLogin.lastExpProcess;

            RefreshExpTextState(win_expShowText);
            win_score.gameObject.SetActive(false);
            if (reward != null)
            {
                win_expAddText.text = reward.expr > 0 ? Util.Format("+{0}", reward.expr) : "+0";

                win_goldAddText.transform.parent.gameObject.SetActive(reward.coin > 0);
                win_goldAddText.text = Util.Format("+{0}", reward.coin);
            }
            if (reward?.score > 0)
            {
                if (moduleMatch.isUpDanlv) RefreshUpDanLvPanel();
                win_score.gameObject.SetActive(true);
                //没加之前的积分
                win_last_score.text = Util.Format("{0}", moduleMatch.rankInfo.score - reward.score);
                //加多少积分
                win_add_score.text = Util.Format("+{0}", reward.score);
                //当前段位
                win_current_danLv.text = moduleMatch.rankInfo.danLv.ToString();
                //还差多少积分晋级
                var info = ConfigManager.GetAll<PvpLolInfo>();
                int max_danLv = info == null ? 8 : info[info.Count - 1].ID;
                win_dis_nextScore.transform.parent.gameObject.SetActive(moduleMatch.rankInfo.danLv < max_danLv);
                if (moduleMatch.rankInfo.danLv < max_danLv && info != null)
                    win_dis_nextScore.text = Util.Format("{0}",
                        info[moduleMatch.rankInfo.danLv].integral - moduleMatch.rankInfo.score);
                //显示logo
                for (int i = 0; i < win_logos_parent.childCount; i++)
                    win_logos_parent.GetChild(i).gameObject.SetActive(i + 1 == moduleMatch.rankInfo.danLv);
            }
            RefreshRewardList(reward, true);
        }
        else
        {
            lose_levelText.text = modulePlayer.roleInfo.level.ToString();
            lose_expbar.fillAmount = moduleLogin.lastExpProcess;

            RefreshExpTextState(lose_expShowText);
            lose_score.gameObject.SetActive(false);

            if (reward != null)
            {
                lose_expAddText.text = reward.expr > 0 ? Util.Format("+{0}", reward.expr) : "+0";

                lose_goldAddText.transform.parent.gameObject.SetActive(reward.coin > 0);
                lose_goldAddText.text = Util.Format("+{0}", reward.coin);

                if (reward.score < 0)
                {
                    lose_score.gameObject.SetActive(true);
                    //没加之前的积分
                    lose_last_score.text = Util.Format("{0}", moduleMatch.rankInfo.score - reward.score);
                    //加多少积分
                    lose_add_score.text = Util.Format("{0}", reward.score);
                    //当前段位
                    lose_current_danLv.text = moduleMatch.rankInfo.danLv.ToString();
                    //还差多少积分晋级
                    var info = ConfigManager.GetAll<PvpLolInfo>();
                    int max_danLv = info == null ? 8 : info[info.Count - 1].ID;
                    lose_dis_nextScroe.transform.parent.gameObject.SetActive(moduleMatch.rankInfo.danLv < max_danLv);
                    if (moduleMatch.rankInfo.danLv < max_danLv && info != null)
                        lose_dis_nextScroe.text = Util.Format("{0}", info[moduleMatch.rankInfo.danLv].integral - moduleMatch.rankInfo.score);
                    //显示logo
                    for (int i = 0; i < lose_logos_parent.childCount; i++)
                        lose_logos_parent.GetChild(i).gameObject.SetActive(i + 1 == moduleMatch.rankInfo.danLv);
                }
            }
            RefreshRewardList(reward, false);
        }
    }

    private void RefreshRewardList(PReward reward, bool isWin)
    {
        if (reward == null) return;
        if (moduleGlobal.targetMatrial.isProcess)
            target.Init(reward, moduleGlobal.targetMatrial);

        List<PItem2> rewards = new List<PItem2>();

        if (reward.diamond > 0)
        {
            PItem2 item = PacketObject.Create<PItem2>();
            item.itemTypeId = (ushort)CurrencySubType.Diamond;
            item.num = (uint)reward.diamond;
            rewards.Add(item);
        }

        if (reward.rewardList != null && reward.rewardList.Length > 0)
            rewards.AddRange(reward.rewardList);

        if (isWin) winReward.SetItems(rewards);
        else loseReward.SetItems(rewards);
    }

    private void OnSetRewardItem(RectTransform node, PItem2 data)
    {
        if (data == null) return;
        var prop = ConfigManager.Get<PropItemInfo>(data.itemTypeId);
        if (prop == null) return;
        else if (prop.itemType == PropType.Rune)
            Util.SetItemInfo(node, prop, data.level, (int)data.num, true, data.star);
        else Util.SetItemInfo(node, prop, 0, (int)data.num, false);

        Util.SetText(node.Find("name")?.GetComponent<Text>(), "");
    }

    private void OnClickRewardItem(RectTransform node, PItem2 data)
    {
        if (data == null) return;
        moduleGlobal.UpdateGlobalTip(data.itemTypeId, true, false);
    }

    private void RefreshExpTextState(Text target)
    {
        var expList = ConfigManager.GetAll<CreatureEXPInfo>();
        int max_Creature_Lv = expList == null ? 60 : expList[expList.Count - 1].ID;
        if (modulePlayer.roleInfo.level < max_Creature_Lv)
        {
            if (expList != null)
                target.text = Util.Format("{0}/{1}",
                    modulePlayer.roleInfo.expr - expList[modulePlayer.roleInfo.level - 1].exp,
                    expList[modulePlayer.roleInfo.level].exp - expList[modulePlayer.roleInfo.level - 1].exp);
        }
        else
            Util.SetText(target, (int)TextForMatType.RuneUIText, 16);
    }

    private void RefreshUpDanLvPanel()
    {
        upDanlvPanel.gameObject.SetActive(true);
        currentlolLv.text = (moduleMatch.rankInfo.danLv - 1).ToString();
        string strBefore = Util.Format("level{0}", moduleMatch.rankInfo.danLv - 1);
        Util.DisableAllChildren(upDanLvPanelLogos, strBefore);
        Util.SetText(currentScore, (int)TextForMatType.SettlementUIText, 19, moduleMatch.rankInfo.score);
        var allPvpInfo = ConfigManager.GetAll<PvpLolInfo>();
        if (allPvpInfo != null)
        {
            bool isTrue = moduleMatch.rankInfo.danLv <= allPvpInfo[allPvpInfo.Count - 1].ID;
            nextLv.gameObject.SetActive(isTrue);
            nextScore.gameObject.SetActive(isTrue);

            if (isTrue)
            {
                nextLv.text = moduleMatch.rankInfo.danLv.ToString();
                var info = ConfigManager.Get<PvpLolInfo>(moduleMatch.rankInfo.danLv);
                if (info != null) Util.SetText(nextScore, (int)TextForMatType.SettlementUIText, 20, info.integral);
            }
        }
        string str = Util.Format("level{0}", moduleMatch.rankInfo.danLv);
        Util.DisableAllChildren(upDanLvPanelLogosTo, str);
        moduleMatch.isUpDanlv = false;
    }

    private void OnOpenNextPanel()
    {
        btnPanel.gameObject.SetActive(true);

        if (Level.current.isPvP)
        {
            OnPvPOver();
        }
        else if (Level.current.isPvE)
        {
            if (moduleLabyrinth.lastSneakPlayer != null)
                OnSneakOver();
            else
                OnPVEExitCallback();
        }
    }

    private void OnClickTipCancle()
    {
        if (Level.current.isPvP)
        {
            win_Panel.gameObject.SetActive(modulePVP.isWinner);
            lose_Panel.gameObject.SetActive(!modulePVP.isWinner);
        }
        else if (Level.current.isPvE)
        {
            win_Panel.gameObject.SetActive(moduleLabyrinth.lastSneakPlayer != null && moduleLabyrinth.isSneakSuccess);
            lose_Panel.gameObject.SetActive(moduleLabyrinth.lastSneakPlayer != null && !moduleLabyrinth.isSneakSuccess);
        }
    }

    private void OnOpenWeaponWindow()
    {
        if (!GetChaseGuideOver())
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.SettlementUIText, 27));
            return;
        }

        bool canshow = moduleGuide.IsActiveFunction(HomeIcons.Equipment);

        if (canshow)
        {
            moduleChase.LastComrade = null;
            modulePVE.reopenPanelType = PVEReOpenPanel.EquipPanel;
            Hide("window_combat");
            Hide("window_settlement");
            OnPVEExitCallback();
            //点击强化需要离队
            moduleAwakeMatch.Request_ExitRoom(modulePlayer.id_);
        }
        else
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.ActiveUIText, 12));
    }

    private void OnOpenRuneWindow()
    {
        if(!GetChaseGuideOver())
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.SettlementUIText, 26));
            return;
        }

        bool canshow = moduleGuide.IsActiveFunction(HomeIcons.Rune);

        if (canshow)
        {
            moduleChase.LastComrade = null;
            modulePVE.reopenPanelType = PVEReOpenPanel.RunePanel;
            Hide("window_combat");
            Hide("window_settlement");
            OnPVEExitCallback();
            //点击灵柏需要离队
            moduleAwakeMatch.Request_ExitRoom(modulePlayer.id_);
        }
        else
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.ActiveUIText, 12));
    }

    private bool GetChaseGuideOver()
    {
        bool valid = true;
        if (moduleChase.lastStartChase != null && moduleChase.lastStartChase.taskConfigInfo != null)
        {
            int id = moduleChase.lastStartChase.taskConfigInfo.ID;
            valid = !GeneralConfigInfo.NeedForbideRuneAndForgeWhenSettlement(id);
            valid |= Module_Guide.skipGuide;
            if (moduleChase.lastStartChase.taskData != null) valid |= moduleChase.lastStartChase.taskData.state == (byte)EnumChaseTaskFinishState.Finish;
        }
        return valid;
    }

    private void OnSneakOver()
    {
        modulePVP.isInvation = false;
        Hide("window_combat");
        Hide("window_settlement");
        Game.GoHome();
    }

    private void RefreshForNoExp(bool isWin)
    {
        if (isWin)
        {
            win_levelText.text = modulePlayer.roleInfo.level.ToString();
            win_expbar.fillAmount = moduleLogin.expProcess;
            win_expAddText.text = "+0";
            RefreshExpTextState(win_expShowText);
            win_goldAddText.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            lose_levelText.text = modulePlayer.roleInfo.level.ToString();
            lose_expbar.fillAmount = moduleLogin.expProcess;
            lose_expAddText.text = "+0";
            RefreshExpTextState(lose_expShowText);
            lose_goldAddText.transform.parent.gameObject.SetActive(false);
        }
    }

    private void OnPvPOver()
    {
        modulePVP.isInvation = false;
        modulePVP.isMatchAgain = false;
        if (modulePVP.opType == OpenWhichPvP.FriendPvP)
            modulePVP.opType = OpenWhichPvP.FreePvP;
        moduleMatch.isbaning = true;
        Hide("window_combat");
        Hide("window_settlement");
        Game.GoHome();
    }

    private void OnOnceAgain()
    {
        modulePVP.isInvation = false;
        modulePVP.isMatchAgain = true;
        Hide("window_combat");
        Hide("window_settlement");
        Game.GoHome();
    }

    /// <summary>
    /// PVE退出回调
    /// </summary>
    private void OnPVEExitCallback()
    {
        if (moduleUnion.m_isUnionBossTask) moduleUnion.SetNewBoss();

        moduleUnion.EnterBossTask = false;
        moduleUnion.m_isUnionBossTask = false;

        if (target.gameObject.activeInHierarchy && moduleGlobal.targetMatrial.isFinish)
        {
            Game.GoHome();
            return;
        }
        //reset data        
        if (modulePVE.reopenPanelType == PVEReOpenPanel.Borderlands)
        {
            //没有结算奖励并且活动已经结束了,直接跳转到主城，交给主城弹出警告提示
            if (!moduleBordlands.isValidBordland && moduleBordlands.bordlandsSettlementReward == null)
            {
                Game.GoHome();
                return;
            }

            //直接跳转到无主之地场景，并且清空掉重启的关卡
            modulePVE.reopenPanelType = PVEReOpenPanel.None;
            Game.LoadLevel(Module_Bordlands.BORDERLAND_LEVEL_ID);
        }
        else if (modulePVE.reopenPanelType == PVEReOpenPanel.Labyrinth)
        {
            //当前挑战胜利的话，就打开继续挑战面板
            if (gameObject.activeInHierarchy && modulePVE.isSendWin)
            {
                labyrinthBtnParentPanel.gameObject.SetActive(true);
                win_Panel.gameObject.SetActive(false);
                lose_Panel.gameObject.SetActive(false);
                var selfLayer = moduleLabyrinth.labyrinthSelfInfo.mazeLayer;
                bool valid = selfLayer < moduleLabyrinth.mazeMaxLayer;
                labyContinueBtn.SetInteractable(valid);
                m_labyProcessText.gameObject.SetActive(valid);
                Util.SetText(m_labyProcessText,ConfigText.GetDefalutString(TextForMatType.SettlementUIText,25), selfLayer, moduleLabyrinth.mazeMaxLayer);

                labyContinueBtn.onClick.RemoveAllListeners();
                labyContinueBtn.onClick.AddListener(OnContinue);
                labyRestBtn.onClick.RemoveAllListeners();
                labyRestBtn.onClick.AddListener(OnRest);
            }
            else
            {
                //直接跳转到迷宫场景，并且清空掉重启的关卡
                modulePVE.reopenPanelType = PVEReOpenPanel.None;
                Game.LoadLevel(Module_Labyrinth.LABYRINTH_LEVEL_ID);
            }
        }
        else
        {
            Game.GoHome();
        }

    }

    private void OnRest()
    {
        Hide(true);
        modulePVE.reopenPanelType = PVEReOpenPanel.None;
        Game.LoadLevel(Module_Labyrinth.LABYRINTH_LEVEL_ID);
    }

    private void OnContinue()
    {
        moduleLabyrinth.SendChallengeLabyrinth();
    }

    #region Boss

    private void SetBossInfo()
    {
        BossObjList();
        if (modulePVE.isSendWin)
        {
            m_WinValueBar.fillAmount = (float)(moduleUnion.BossInfo.remianblood) / (float)moduleUnion.m_bossStage.bossHP;
            m_myWinHurt.text = moduleUnion.m_thisHurt.ToString();
            if (moduleUnion.m_thisHurt > moduleUnion.m_bossStage.bossHP)
            {
                m_myWinHurt.text = moduleUnion.m_bossStage.bossHP.ToString();
            }
            m_allWinHurt.text = (moduleUnion.m_bossStage.bossHP - moduleUnion.BossInfo.remianblood).ToString();
            for (int i = 0; i < moduleUnion.m_bossReward.Count; i++)
            {
                m_winList[i].SetActive(true);
                Text winTxt = m_winList[i].GetComponent<Text>();
                RectTransform rt = m_winList[i].GetComponent<RectTransform>();

                winTxt.text = moduleUnion.m_bossReward[i].condition.ToString() + "%";
                float x = ((float)moduleUnion.m_bossReward[i].condition / 100) * 403;
                rt.anchoredPosition = new Vector3(x, 17, 0);
            }
        }
        else
        {
            m_loseValueBar.fillAmount = (float)(moduleUnion.BossInfo.remianblood) / (float)moduleUnion.m_bossStage.bossHP;
            m_bossLossTime = (int)(moduleUnion.BossInfo.opentime - moduleUnion.BossLossTime(moduleUnion.m_bossCloseTime));
            if (m_bossLossTime > 0)
            {
                m_boss = true;
                m_bossTime = 0;
                SetTime(m_bossLossTime, m_lossTime);
            }
            else
            {
                Util.SetText(m_lossTime, ConfigText.GetDefalutString(242, 160), 0);
            }
            m_myLoseHurt.text = moduleUnion.m_thisHurt.ToString();

            m_allLoseHurt.text = (moduleUnion.m_bossStage.bossHP - moduleUnion.BossInfo.remianblood).ToString();
            for (int i = 0; i < moduleUnion.m_bossReward.Count; i++)
            {
                m_loseList[i].SetActive(true);
                Text loseTxt = m_loseList[i].GetComponent<Text>();
                RectTransform rt = m_loseList[i].GetComponent<RectTransform>();

                loseTxt.text = moduleUnion.m_bossReward[i].condition.ToString() + "%";
                float x = ((float)moduleUnion.m_bossReward[i].condition / 100) * 403;
                rt.anchoredPosition = new Vector3(x, 17, 0);
            }

            //刷新失败时候的按钮处理(符文和锻造)
            m_lossRune.SetInteractable(!moduleGuide.IsGuideTask());
            m_lossForing.SetInteractable(!moduleGuide.IsGuideTask());
        }
    }

    private void BossObjList()
    {
        m_winList.Clear();
        m_loseList.Clear();
        foreach (Transform item in m_WinValueBar.transform)
        {
            m_winList.Add(item.gameObject);
        }
        foreach (Transform item in m_loseValueBar.transform)
        {
            m_loseList.Add(item.gameObject);
        }
        for (int i = 0; i < m_winList.Count; i++)
        {
            m_winList[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < m_loseList.Count; i++)
        {
            m_loseList[i].gameObject.SetActive(false);
        }
    }

    public override void OnRenderUpdate()
    {
        base.OnRenderUpdate();
        if (m_boss)
        {
            m_bossTime += Time.unscaledDeltaTime;
            if (m_bossTime > 1)
            {
                m_bossTime = 0;
                m_bossLossTime--;
                SetTime(m_bossLossTime, m_lossTime);
                if (m_bossLossTime == 0)
                {
                    m_boss = false;
                }
            }
        }
    }

    private void SetTime(int timeNow, Text thisText)
    {
        var strTime = Util.GetTimeMarkedFromSec(timeNow);
        Util.SetText(thisText, strTime);
    }

    #endregion

    #region npcNewLvPanel

    private void OnClickUpWindow(Module_Npc.NpcMessage obj)
    {
        if (obj != null && obj.npcInfo != null && obj.fetterLv == obj.npcInfo.unlockLv)
            Module_NPCDating.instance.DoDatingEvent(obj.npcInfo.unlockStoryID);
    }

    #endregion
}