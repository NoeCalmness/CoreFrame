/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Lee
 * Version:  0.1
 * Created:  2017-07-21
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class Window_Chase : Window
{
    /// <summary>
    /// 追捕任务的困难模式按钮ID(对应unlockfunction表)
    /// </summary>
    public const int CHASE_DIFFICULT_BTN_ID = 102;
    /// <summary>
    /// 追捕任务的噩梦模式按钮ID(对应unlockfunction表)
    /// </summary>
    public const int CHASE_NIGHT_BTN_ID = 115;

    #region 高能预警
    public class SpecialTaskPanel
    {
        private Transform mapIcon;
        private Text taskName;
        private Image[] stars;
        private Transform task_trans;
        Transform trans;

        public SpecialTaskPanel(Transform parent)
        {
            trans = parent;
            var backGround = trans.Find("bg").GetComponent<Button>();
            task_trans = trans.Find("special_task_panel");
            task_trans.SafeSetActive(false);
            mapIcon = trans.Find("special_task_panel/kuang/person/icon");
            taskName = trans.Find("special_task_panel/kuang/renwuText").GetComponent<Text>();
            stars = trans.Find("special_task_panel/kuang/nandukuang/parent").GetComponentsInChildren<Image>(true);

            backGround.onClick.RemoveAllListeners();
            backGround.onClick.AddListener(() =>
            {
                if (!task_trans.gameObject.activeInHierarchy) return;
                trans.SafeSetActive(false);
                task_trans.SafeSetActive(false);
            });
        }

        public void ResfreshSpecial(ChaseTask task)
        {
            trans.SafeSetActive(true);
            task_trans.SafeSetActive(true);
            //icon
            string[] str = task.stageInfo.icon.Split(';');
            if (str.Length >= 1)
                UIDynamicImage.LoadImage(mapIcon, str[0]);
            //name
            taskName.text = task.taskConfigInfo.name;
            //星级
            for (int k = 0; k < stars.Length; k++)
                stars[k].SafeSetActive(k < task.star);
        }
    }
    #endregion

    #region 弹窗界面
    //高能界面
    private Transform gaoneng;
    private Transform gaonengSpecial;
    private SpecialTaskPanel specialpanel;

    //重置次数
    private Transform payCountPanel;
    private Text contentTip;
    private Text currentDayCount;
    private Text costCount;
    private Image costIcon;
    private Button sureBtn;

    private ChaseTask currentTask;
    #endregion

    #region 星级奖励Panel
    private Transform normalStar;
    private Transform diffStar;
    private Button starRewardEnter;
    private Transform canGetReward;
    private Text currentLvStarNumber;
    private Transform starRewardPanel;
    Text starRewardLevelTittle;
    Text currentStarCount;
    ScrollView view;
    DataSource<ChaseLevelStarInfo> starData;
    #endregion

    #region 入口界面
    private TaskType initType = TaskType.Count;
    private Transform enterPanel;
    private Transform npcTrans;
    private Transform trainNotice;
    private Button toEmergencyBtn;
    private Button toEasyBtn;
    private Button toAwakeBtn;
    private Button toTrainBtn;
    private Button toActiveBtn;
    private Button npcStoryBtn;
    private Image[] activeImages;
    private Image[] emergencyImages;
    private Text[] activeTexts;
    private Text[] emergencyTexts;
    #endregion

    #region 紧急任务
    private Transform emergency_panel;
    private EmergencyItem emergency;
    #endregion

    #region tittlePanel
    private Transform tittlePanel;
    private Transform selectHardTran;
    private Transform checkShadow;
    private bool isPull;
    //简单
    private Button selectEasy;
    private CanvasGroup easyCanvasGroup;
    private TweenAlpha easyTweenAlpha;
    private TweenPosition easyTweenPos;
    //困难
    private Button selectDiff;
    private Image diffImage;
    private Text diffText;
    private CanvasGroup diffCanvasGroup;
    private TweenAlpha diffTweenAlpha;
    private TweenPosition diffTweenPos;
    //噩梦
    private Button selectNight;
    private Image nightImage;
    private Text nightText;
    private CanvasGroup nightCanvasGroup;
    private TweenAlpha nightTweenAlpha;
    private TweenPosition nightTweenPos;
    //章节按钮
    private DataSource<int> dataSelect;
    private ScrollView selectBoxParent;
    private Transform selectBox;
    private TweenPosition selectTween;
    private Text selectText;
    //活动
    private Transform activeTittle;
    private Transform emergencyTittle;
    #endregion

    #region select_active
    private Transform selectActivePanel;
    private Transform active_preview;
    private Transform rewardsParent;
    private Transform rewardItem;
    private Text challengeCount;
    private ScrollView activeView;
    DataSource<ActiveTaskInfo> activeData;
    private Button leftBtn;
    private Button rigBtn;
    private bool isShowBtn;
    #endregion

    #region tasksScroll
    private Transform taskPanel;
    private Transform normalBg;
    private Transform normalBackGround;
    private Transform activeBg;

    ScrollView scrollPage;
    DataSource<Chase_PageItem> dataPage;

    //翻页
    private Transform easyDireTran;
    private Button easyUpBtn;
    private Button easyDownBtn;
    private Transform diffDireTran;
    private Button diffUpBtn;
    private Button diffDownBtn;
    #endregion

    #region window_base

    protected override void OnOpen()
    {
        #region 高能预警
        gaoneng        = GetComponent<Transform>("middle_centre/task_gaoneng");
        gaonengSpecial = GetComponent<Transform>("middle_centre/task_gaoneng/special_task_panel");
        specialpanel   = new SpecialTaskPanel(gaoneng);
        #endregion

        #region 二级弹窗
        Transform tran    = GetComponent<Transform>("middle_centre");
        tran.SafeSetActive(true);

        payCountPanel     = GetComponent<Transform>("middle_centre/payChallengeCount");
        contentTip        = GetComponent<Text>("middle_centre/payChallengeCount/kuang/content_tip");
        currentDayCount   = GetComponent<Text>("middle_centre/payChallengeCount/kuang/currentDayCount");
        costCount         = GetComponent<Text>("middle_centre/payChallengeCount/kuang/cost/icon/now");
        costIcon          = GetComponent<Image>("middle_centre/payChallengeCount/kuang/cost");
        sureBtn           = GetComponent<Button>("middle_centre/payChallengeCount/kuang/sureBtn");
        #endregion

        #region 星级奖励
        starRewardEnter       = GetComponent<Button>("tasksPanel/reward_Panel");
        normalStar            = GetComponent<Transform>("tasksPanel/reward_Panel/box_normal");
        diffStar              = GetComponent<Transform>("tasksPanel/reward_Panel/box_hard");
        currentLvStarNumber   = GetComponent<Text>("tasksPanel/reward_Panel/number");
        canGetReward          = GetComponent<Transform>("tasksPanel/reward_Panel/mark");
        starRewardLevelTittle = GetComponent<Text>("middle_centre/starRewardPanel/substance/smalltittle/level_text");
        currentStarCount      = GetComponent<Text>("middle_centre/starRewardPanel/substance/smalltittle/star_number");
        starRewardPanel       = GetComponent<Transform>("middle_centre/starRewardPanel");
        starRewardPanel.SafeSetActive(false);
        view                  = GetComponent<ScrollView>("middle_centre/starRewardPanel/substance/scrollView");
        starData              = new DataSource<ChaseLevelStarInfo>(null, view, OnSetStarRewardInfo, null);
        #endregion

        #region 入口界面
        enterPanel      = GetComponent<Transform>("enterPanel");
        npcTrans        = GetComponent<Transform>("middle_left");
        trainNotice     = GetComponent<Transform>("enterPanel/sprite_mission/mark");
        toEmergencyBtn  = GetComponent<Button>("enterPanel/emergencyBtn");
        toEasyBtn       = GetComponent<Button>("enterPanel/easyBtn");
        toAwakeBtn      = GetComponent<Button>("enterPanel/awakenBtn");
        toTrainBtn      = GetComponent<Button>("enterPanel/sprite_mission");
        toActiveBtn     = GetComponent<Button>("enterPanel/activeBtn");
        npcStoryBtn     = GetComponent<Button>("enterPanel/npc_storybtn");
        activeImages    = toActiveBtn.GetComponentsInChildren<Image>(true);
        emergencyImages = toEmergencyBtn.GetComponentsInChildren<Image>(true);
        activeTexts     = toActiveBtn.GetComponentsInChildren<Text>(true);
        emergencyTexts  = toEmergencyBtn.GetComponentsInChildren<Text>(true);

        toEasyBtn.     onClick.RemoveAllListeners();
        toEasyBtn.     onClick.AddListener(OnToEasyMode);
        toEmergencyBtn.onClick.RemoveAllListeners();
        toEmergencyBtn.onClick.AddListener(OnToEmergencyMode);
        toActiveBtn.   onClick.RemoveAllListeners();
        toActiveBtn.   onClick.AddListener(OnClickAtciveBtn);
        npcStoryBtn.   onClick.RemoveAllListeners();
        npcStoryBtn.   onClick.AddListener(OnClickNpcStory);
        toTrainBtn.    onClick.RemoveAllListeners();
        toTrainBtn.    onClick.AddListener(() => ShowAsync<Window_Train>());
        toAwakeBtn.    onClick.RemoveAllListeners();
        toAwakeBtn.    onClick.AddListener(() => ShowAsync<Window_Awaketeam>());
        #endregion

        #region 紧急panel
        emergency_panel    = GetComponent<Transform>("emergencyPanel");
        var emerGameObject = GetComponent<Transform>("emergencyPanel/emergencyItem");
        emergency          = SubWindowBase.CreateSubWindow<EmergencyItem, Window_Chase>(this, emerGameObject?.gameObject);
        #endregion

        #region tittlePanel
        tittlePanel      = GetComponent<Transform>("tittlePanel");
        selectHardTran   = GetComponent<Transform>("tittlePanel/mainTittles");
        checkShadow      = GetComponent<Transform>("tittlePanel/mainTittles/shadow");
        checkShadow.SafeSetActive(false);
        selectEasy       = GetComponent<Button>("tittlePanel/mainTittles/01_img");
        easyCanvasGroup  = selectEasy.gameObject.GetComponentDefault<CanvasGroup>();
        easyTweenAlpha   = selectEasy.GetComponent<TweenAlpha>();
        easyTweenPos     = selectEasy.GetComponent<TweenPosition>();
        selectDiff       = GetComponent<Button>("tittlePanel/mainTittles/02_img");
        diffImage        = selectDiff.GetComponent<Image>();
        diffText         = selectDiff.GetComponentInChildren<Text>();
        diffCanvasGroup  = selectDiff.gameObject.GetComponentDefault<CanvasGroup>();
        diffTweenAlpha   = selectDiff.GetComponent<TweenAlpha>();
        diffTweenPos     = selectDiff.GetComponent<TweenPosition>();
        selectNight      = GetComponent<Button>("tittlePanel/mainTittles/03_img");
        nightImage       = selectNight.GetComponent<Image>();
        nightText        = selectNight.GetComponentInChildren<Text>();
        nightCanvasGroup = selectNight?.gameObject.GetComponentDefault<CanvasGroup>();
        nightTweenAlpha  = selectNight?.GetComponent<TweenAlpha>();
        nightTweenPos    = selectNight?.GetComponent<TweenPosition>();

        selectEasy. onClick.RemoveAllListeners();
        selectEasy. onClick.AddListener(OnClickEasyBtn);
        selectDiff. onClick.RemoveAllListeners();
        selectDiff. onClick.AddListener(OnClickDifficultBtn);
        selectNight.onClick.RemoveAllListeners();
        selectNight.onClick.AddListener(OnClickNightmareBtn);

        selectBoxParent = GetComponent<ScrollView>("tittlePanel/scrollView");
        dataSelect      = new DataSource<int>(null, selectBoxParent, OnSetSelectBox, OnClickSelectBox);
        selectBox       = GetComponent<Transform>("tittlePanel/scrollView/selectBox_img");
        selectTween     = selectBox?.GetComponent<TweenPosition>();
        selectText      = GetComponent<Text>("tittlePanel/scrollView/selectBox_img/selectBox_txt");

        activeTittle = GetComponent<Transform>("tittlePanel/activeTittle");
        emergencyTittle = GetComponent<Transform>("tittlePanel/emergencyTittle");
        #endregion

        #region 活动选择界面
        selectActivePanel = GetComponent<Transform>("activePanel");
        active_preview    = GetComponent<Transform>("tasksPanel/active_reward_Preview");
        rewardsParent     = GetComponent<Transform>("tasksPanel/active_reward_Preview/pre_content");
        rewardItem        = GetComponent<Transform>("tasksPanel/active_reward_Preview/item_modle");
        rewardItem.SafeSetActive(false);
        challengeCount    = GetComponent<Text>("tasksPanel/active_reward_Preview/challengeCount");
        activeView        = GetComponent<ScrollView>("activePanel/scrollView");
        activeData        = new DataSource<ActiveTaskInfo>(null, activeView, OnSetActiveItems, OnClickActiveItem);
        leftBtn           = GetComponent<Button>("activePanel/Scroll_btn/left");
        rigBtn            = GetComponent<Button>("activePanel/Scroll_btn/right");

        activeView.onIndexChanged.RemoveAllListeners();
        activeView.onIndexChanged.AddListener(OnIndexChanged);
        activeView.onProgress.AddListener(OnProgressChanged);
        leftBtn.   onClick.RemoveAllListeners();
        leftBtn.   onClick.AddListener(() => { OnGoToDirction(true); });
        rigBtn.    onClick.RemoveAllListeners();
        rigBtn.    onClick.AddListener(() => { OnGoToDirction(false); });
        #endregion

        #region taskScroll
        taskPanel  = GetComponent<Transform>("tasksPanel");
        normalBg   = GetComponent<Transform>("tasksPanel/background/normal_bg");
        normalBackGround = GetComponent<Transform>("tasksPanel/background/normal_bg/bg/normal_img_bg");
        activeBg   = GetComponent<Transform>("tasksPanel/background/active_bg");
        scrollPage = GetComponent<ScrollView>("tasksPanel/scrollview");
        dataPage   = new DataSource<Chase_PageItem>(null, scrollPage, OnSetPageItem, null);
        scrollPage.onProgress.RemoveAllListeners();
        scrollPage.onProgress.AddListener(SetToggleState);

        //翻页
        easyDireTran   = GetComponent<Transform>("tasksPanel/directionBtns/normal");
        easyUpBtn      = GetComponent<Button>("tasksPanel/directionBtns/normal/changepage_top_02");
        easyDownBtn    = GetComponent<Button>("tasksPanel/directionBtns/normal/changepage_bottom_02");
        diffDireTran   = GetComponent<Transform>("tasksPanel/directionBtns/diffcult");
        diffUpBtn      = GetComponent<Button>("tasksPanel/directionBtns/diffcult/changepage_top_01");
        diffDownBtn    = GetComponent<Button>("tasksPanel/directionBtns/diffcult/changepage_bottom_01");
        #endregion
        
        IniteText();

        DefaultState();

        moduleAwakeMatch.Request_TeamTaskInfo(1);
    }

    private void OnClickNpcStory()
    {
        ShowAsync<Window_NpcGaiden>();
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault(1, enterPanel.gameObject.activeInHierarchy);
        if (forward)
        {
            DefaultState();
            moduleChase.SendChaseInfo();
            moduleChase.SendActiveState();

            moduleGuide.CreateTempCondition();
            moduleGuide.RecreateAllTaskCondition();
        }

        SwitchToSubWindow();

        trainNotice.SafeSetActive(modulePet.AnyTaskFinish);

        if (oldState)
            m_subTypeLock = -1;
    }

    protected override void GrabRestoreData(WindowHolder holder)
    {
        if (enterPanel && enterPanel.gameObject.activeInHierarchy) holder.SetData(null);
        else holder.SetData(currentTask);
    }

    protected override void ExecuteRestoreData(WindowHolder holder)
    {
        var targetTask = holder.GetData<ChaseTask>(0);
        if (moduleChase.targetTaskFromForge == null && targetTask != null) moduleChase.SetTargetTask(targetTask);
    }

    public void GoToSubWindow(TaskType taskType)
    {
        if (taskType == TaskType.None||taskType == TaskType.Count) DefaultState();
        else if (taskType == TaskType.Emergency) OnToEmergencyMode();
        else if (taskType == TaskType.Easy) OnToEasyMode();
        else if (taskType == TaskType.Difficult) OnClickDifficultBtn();
        else if (taskType == TaskType.Active) OnClickAtciveBtn();
        else if (taskType == TaskType.Nightmare) OnClickNightmareBtn();
    }

    public void SwitchToSubWindow()
    {
        //默认是追捕的主界面
        var taskType = TaskType.None;

        if (moduleChase.targetTaskFromForge != null)
        {
            taskType = moduleChase.targetTaskFromForge.taskType;
            moduleChase.SkipTargetTask(moduleChase.targetTaskFromForge);
        }
        //跳转到指定type
        else if (m_subTypeLock > -1 && (TaskType)m_subTypeLock <= TaskType.Count)
        {
            taskType = (TaskType)m_subTypeLock;

            if (taskType == TaskType.None) DefaultState();
            else if (taskType == TaskType.Emergency) OnToEmergencyMode();
            else if (taskType == TaskType.Easy) OnToEasyMode();
            else if (taskType == TaskType.Difficult) OnClickDifficultBtn();
            else if (taskType == TaskType.Active) OnClickAtciveBtn();
            else if (taskType == TaskType.Nightmare) OnClickNightmareBtn();
            else moduleChase.SkipTargetTypeTasks(taskType, 100, false);
            //m_subTypeLock = -1;
        }
        //跳转到上次打的关卡的type和level
        else if (moduleChase.lastStartChase != null && moduleGlobal.targetMatrial == null)
        {
            taskType = moduleChase.lastStartChase.taskType;
            if (taskType == TaskType.Awake)
            {
                moduleGuide.AddWindowChaseEvent(taskType);
                return;
            }
            if (taskType >= TaskType.GaidenChapterOne)
            {
                moduleGuide.AddWindowChaseEvent(TaskType.None);
                return;
            }
            moduleChase.SkipLastTypeAndLevel();
            moduleChase.lastStartChase = null;
        }
        moduleGuide.AddWindowChaseEvent(taskType);
    }

    protected override void OnClose()
    {
        base.OnClose();

        emergency?.Destroy();
    }

    protected override void OnReturn()
    {
        if (m_subTypeLock != -1)
        {
            Hide(true);
            m_subTypeLock = -1;
            currentTask = null;
            return;
        }

        if (Window_TeamMatch.IsChooseStage)
        {
            ShowAsync<Window_TeamMatch>();
            return;
        }

        if (gaoneng.gameObject.activeInHierarchy)
        {
            gaoneng.SafeSetActive(false);
            gaonengSpecial.SafeSetActive(false);
        }
        else if (enterPanel.gameObject.activeInHierarchy)
        {
            Hide(true);
            currentTask = null;
            moduleGlobal.targetMatrial?.Clear();
        }
        else if (active_preview.gameObject.activeInHierarchy) CreateActiveContains();
        else
        {
            DefaultState();
            moduleChase.SetTargetTask(null);

            moduleGuide.AddWindowChaseEvent(TaskType.None);
        }
    }

    protected override void OnHide(bool forward)
    {
        if (forward) return;
        moduleChase.ClearDataOnReturn();
        //targetTask = null;
    }

    private void OnClickAtciveBtn()
    {
        if (moduleChase.chaseInfo == null) return;
        if (moduleChase.openActiveItems.Count < 1)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 37));
            return;
        }

        CreateActiveContains();
    }

    private void EasyTween()
    {
        if (diffCanvasGroup.alpha != 1 && nightCanvasGroup.alpha != 1)
        {
            PlayTween(easyTweenAlpha, easyTweenPos, true);

            PlayTween(diffTweenAlpha, diffTweenPos, true);
            bool isTrue = moduleChase.diffTasksToMaxDic == null || moduleChase.diffTasksToMaxDic.Count < 1 || !moduleGuide.IsActiveFunction(CHASE_DIFFICULT_BTN_ID);
            RefreshSaturation(diffImage, diffText, isTrue);

            PlayTween(nightTweenAlpha, nightTweenPos, true);
            bool _isTrue = moduleChase.nightmareTasksToMax == null || moduleChase.nightmareTasksToMax.Count < 1 || !moduleGuide.IsActiveFunction(CHASE_NIGHT_BTN_ID);
            RefreshSaturation(nightImage, nightText, _isTrue);

            checkShadow.SafeSetActive(true);
            isPull = true;
            return;
        }
        PlayTween(diffTweenAlpha, diffTweenPos, false);
        PlayTween(nightTweenAlpha, nightTweenPos, false);
        checkShadow.SafeSetActive(false);
        isPull = false;

        easyTweenPos.PlayReverse();
        selectEasy.transform.SetAsLastSibling();
    }

    private void OnClickEasyBtn()
    {
        EasyTween();
        moduleChase.SetTargetTask(null);
        moduleChase.SkipTargetTypeTasks(TaskType.Easy);
    }

    private void NightmareTween()
    {
        if (moduleChase.nightmareTasksToMax == null || moduleChase.nightmareTasksToMax.Count < 1 || !moduleGuide.IsActiveFunction(CHASE_NIGHT_BTN_ID))
        {
            if (moduleChase.allNightmareTasks != null && moduleChase.allNightmareTasks.Count > 0)
                moduleChase.ClickUnlock(moduleChase.allNightmareTasks[0]);
            else moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 36));
            return;
        }

        if (easyCanvasGroup.alpha != 1 && diffCanvasGroup.alpha != 1)
        {
            PlayTween(nightTweenAlpha, nightTweenPos, true);

            PlayTween(easyTweenAlpha, easyTweenPos, true);

            PlayTween(diffTweenAlpha, diffTweenPos, true);
            bool isTrue = moduleChase.diffTasksToMaxDic == null || moduleChase.diffTasksToMaxDic.Count < 1 || !moduleGuide.IsActiveFunction(CHASE_DIFFICULT_BTN_ID);
            RefreshSaturation(diffImage, diffText, isTrue);

            checkShadow.SafeSetActive(true);
            isPull = true;
            return;
        }
        PlayTween(easyTweenAlpha, easyTweenPos, false);
        PlayTween(diffTweenAlpha, diffTweenPos, false);
        checkShadow.SafeSetActive(false);
        isPull = false;

        nightTweenPos.PlayReverse();
        selectNight.transform.SetAsLastSibling();
    }

    private void OnClickNightmareBtn()
    {
        NightmareTween();
        if (!moduleGuide.IsActiveFunction(CHASE_NIGHT_BTN_ID))
            return;
        moduleChase.SetTargetTask(null);
        moduleChase.SkipTargetTypeTasks(TaskType.Nightmare);
    }

    private void DifficultTween()
    {
        if (moduleChase.diffTasksToMaxDic == null || moduleChase.diffTasksToMaxDic.Count < 1 || !moduleGuide.IsActiveFunction(CHASE_DIFFICULT_BTN_ID))
        {
            if (moduleChase.allDiffTasks != null && moduleChase.allDiffTasks.Count > 0)
                moduleChase.ClickUnlock(moduleChase.allDiffTasks[0]);
            else moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 36));
            return;
        }

        if (easyCanvasGroup.alpha != 1 && nightCanvasGroup.alpha != 1)
        {
            PlayTween(diffTweenAlpha, diffTweenPos, true);

            PlayTween(easyTweenAlpha, easyTweenPos, true);

            PlayTween(nightTweenAlpha, nightTweenPos, true);

            bool _isTrue = moduleChase.nightmareTasksToMax == null || moduleChase.nightmareTasksToMax.Count < 1 || !moduleGuide.IsActiveFunction(CHASE_NIGHT_BTN_ID);
            RefreshSaturation(nightImage, nightText, _isTrue);

            checkShadow.SafeSetActive(true);
            isPull = true;
            return;
        }
        PlayTween(easyTweenAlpha, easyTweenPos, false);
        PlayTween(nightTweenAlpha, nightTweenPos, false);
        checkShadow.SafeSetActive(false);
        isPull = false;

        diffTweenPos.PlayReverse();
        selectDiff.transform.SetAsLastSibling();
    }

    private void OnClickDifficultBtn()
    {
        DifficultTween();
        if (!moduleGuide.IsActiveFunction(CHASE_DIFFICULT_BTN_ID))
            return;
        moduleChase.SetTargetTask(null);
        moduleChase.SkipTargetTypeTasks(TaskType.Difficult);
    }

    private void PlayTween(TweenAlpha alpha, TweenPosition pos, bool isForward)
    {
        if (!alpha || !pos) return;
        if (isForward)
        {
            alpha.PlayForward();
            pos.PlayForward();
            return;
        }
        alpha.PlayReverse();
        pos.PlayReverse();
    }

    private void RefreshSaturation(Image image, Text text, bool isTrue)
    {
        if (!image || !text) return;

        image.saturation = isTrue ? 0 : 1;
        text.saturation = isTrue ? 0 : 1;
        image.SafeSetActive(false);
        image.SafeSetActive(true);
    }

    private void OnToEasyMode()
    {
        if (moduleChase.easyTasksToMaxDic.Count < 1)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 4));
            return;
        }
        moduleChase.SkipTargetTypeTasks(TaskType.Easy);
    }

    private void OnToEmergencyMode()
    {
        if (moduleChase.emergencyList == null || moduleChase.emergencyList.Count < 1)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 4));
            return;
        }

        if (modulePlayer.level < moduleChase.emergencyList[0].taskConfigInfo.unlockLv)
        {
            var str = Util.Format(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 38), moduleChase.emergencyList[0].taskConfigInfo.unlockLv);
            moduleGlobal.ShowMessage(str);
            return;
        }

        moduleChase.SkipTargetTypeTasks(TaskType.Emergency);
    }

    private void IniteText()
    {
        ConfigText chaseText = ConfigManager.Get<ConfigText>((int)TextForMatType.ChaseUIText);

        Util.SetText(GetComponent<Text>("middle_centre/task_gaoneng/text_image/text"), chaseText[14]);
        Util.SetText(GetComponent<Text>("middle_centre/task_confirm_panel/kuang/zhuibu/zhuibu_text"), chaseText[17]);
        Util.SetText(GetComponent<Text>("middle_centre/task_confirm_panel/kuang/bg/missioninfo"), chaseText[18]);
        Util.SetText(GetComponent<Text>("middle_centre/task_gaoneng/special_task_panel/kuang/specialTask"), chaseText[19]);
        Util.SetText(GetComponent<Text>("middle_centre/task_gaoneng/special_task_panel/kuang/nandukuang/Text"), chaseText[20]);
        Util.SetText(GetComponent<Text>("tittlePanel/mainTittles/01_img/txt"), chaseText[40]);
        Util.SetText(GetComponent<Text>("tittlePanel/mainTittles/02_img/txt"), chaseText[41]);
        Util.SetText(GetComponent<Text>("tittlePanel/mainTittles/03_img/txt"), chaseText[54]);
        Util.SetText(GetComponent<Text>("tittlePanel/emergencyTittle/txt"), chaseText[42]);
        Util.SetText(GetComponent<Text>("tittlePanel/activeTittle/txt"), chaseText[43]);
        Util.SetText(GetComponent<Text>("middle_centre/starRewardPanel/back/tittle/tittle_text"), chaseText[45]);
        Util.SetText(GetComponent<Text>("enterPanel/biaoti/title_big"), chaseText[48]);
        Util.SetText(GetComponent<Text>("enterPanel/biaoti/title_small"), chaseText[49]);
        Util.SetText(GetComponent<Text>("activePanel/bottom_img/Text"), chaseText[51]);

        Util.SetText(GetComponent<Text>("enterPanel/sprite_mission/Text"), (int)TextForMatType.PetTrainText, 0);
    }

    private void DefaultState()
    {
        moduleGlobal.ShowGlobalLayerDefault();

        enterPanel.SafeSetActive(true);
        taskPanel.SafeSetActive(false);
        selectActivePanel.SafeSetActive(false);
        if (emergency_panel.gameObject.activeInHierarchy) emergency.UnInitialize();
        emergency_panel.SafeSetActive(false);
        tittlePanel.SafeSetActive(false);
        npcTrans.SafeSetActive(true);

        gaoneng.SafeSetActive(false);
        gaonengSpecial.SafeSetActive(false);
        starRewardPanel.SafeSetActive(false);
        payCountPanel.SafeSetActive(false);

        selectBox.SetParent(selectBoxParent?.transform);
        selectBoxParent.SafeSetActive(false);
        checkShadow.SafeSetActive(false);
        initType = TaskType.Count;
    }

    private void ToDifferentModel(TaskType type)
    {
        if (type == TaskType.Easy || type == TaskType.Difficult || type == TaskType.Nightmare)
        {
            if (type == TaskType.Easy)
            {
                if (isPull) EasyTween();
                else
                {
                    easyCanvasGroup.alpha = 1;
                    diffCanvasGroup.alpha = 0;
                    nightCanvasGroup.alpha = 0;
                    selectEasy.transform.SetAsLastSibling();
                }                
            }
            else if (type == TaskType.Difficult)
            {
                if (isPull) DifficultTween();
                else
                {
                    easyCanvasGroup.alpha = 0;
                    diffCanvasGroup.alpha = 1;
                    nightCanvasGroup.alpha = 0;
                    selectDiff.transform.SetAsLastSibling();
                }                
            }
            else
            {
                if (isPull) NightmareTween();
                else
                {
                    easyCanvasGroup.alpha = 0;
                    diffCanvasGroup.alpha = 0;
                    nightCanvasGroup.alpha = 1;
                    selectNight.transform.SetAsLastSibling();
                }                
            }
        }
    }

    #endregion

    #region checkBox-part

    private void InitLevelCheckBox(TaskType type)
    {
        if (!moduleChase.haveLvDic.ContainsKey(type)) return;
        initType = type;

        dataSelect.SetItems(moduleChase.haveLvDic[type]);
    }

    private void OnClickSelectBox(RectTransform node, int data)
    {
        var type = moduleChase.currentSelectType;

        if (data == moduleChase.firstMoreMax)
        {
            if (type == TaskType.Emergency)
            {
                int unLockLv = moduleChase.emerLockLv.ContainsKey(data) ? moduleChase.emerLockLv[data] : 0;
                var currentEm = moduleChase.emergencyList.Find(p => p.taskConfigInfo.difficult == data);
                if (modulePlayer.level < unLockLv) moduleGlobal.ShowMessage(Util.Format(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 38), unLockLv));
                else if (currentEm == null)
                {
                    int lastlevel = data - 1 < 0 ? 0 : data - 1;
                    string name = Util.GetString(296, lastlevel - 1 < 0 ? 0 : lastlevel - 1);
                    string tip = Util.Format(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 35), name);
                    moduleGlobal.ShowMessage(tip);
                }
            }
            else if (type == TaskType.Easy || type == TaskType.Difficult || type == TaskType.Nightmare)
                OnClickCheckBtn(type, data);

            return;
        }

        moduleChase.GetTargetChaseList(type, data);

        if (data == moduleChase.currentClickLevel)
        {
            selectBox.SetParent(node);
            selectTween?.Play();

            SetSelectText(type, data);
        }
    }

    private void OnClickCheckBtn(TaskType type, int maxlv)
    {
        var next = moduleChase.allTasks.FindAll(p => moduleChase.GetCurrentTaskType(p) == type && p.level == maxlv);
        if (next.Count > 0) moduleChase.ClickUnlock(next[0]);
    }

    private void OnSetSelectBox(RectTransform node, int data)
    {
        BaseRestrain.SetRestrainData(node.gameObject, data);

        var text = node.Find("chapter_01_text")?.GetComponent<Text>();
        var type = moduleChase.currentSelectType;

        //设置icon
        if (type == TaskType.Easy || type == TaskType.Difficult || type == TaskType.Nightmare)
        {
            var icon = node.Find("icon");
            var task = moduleChase.allTasks.Find(p => moduleChase.GetCurrentTaskType(p) == type && data == p.level);
            AtlasHelper.SetShared(icon, task?.checkBoxIcon);
        }
        else if (type == TaskType.Emergency)
        {
            var icon = node.Find("icon");
            var task = moduleChase.emergencyList.Find(t => t.taskConfigInfo.difficult == data);
            AtlasHelper.SetShared(icon, task?.taskConfigInfo.checkBoxIcon);
        }

        //设置选中框状态
        if (data == moduleChase.currentClickLevel)
        {
            selectBox.SetParent(node);
            selectTween?.Play();

            if (type == TaskType.Easy || type == TaskType.Difficult)
            {
                moduleChase.SetRewardTypeAndLevel(type, data);
                moduleChase.GetLevelStarInfo(type, data);
                SetStarEnterState(type, data);
            }

            SetSelectText(type, data);
        }

        //刷新checkbox的文字显示
        if (type == TaskType.Emergency)
        {
            string _s = ConfigText.GetDefalutString(296, data - 1);
            int unLockLv = moduleChase.emerLockLv.ContainsKey(data) ? moduleChase.emerLockLv[data] : 0;

            if (data < moduleChase.firstMoreMax)
            {
                Util.SetText(text, _s);
                RefreshBtnIntercable(node, true);
            }
            else if (data == moduleChase.firstMoreMax)
            {
                RefreshBtnIntercable(node, false);
                string str = ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 33);
                string str_n = ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 52);

                if (modulePlayer.level < unLockLv) Util.SetText(text, str + str_n, unLockLv);
                else Util.SetText(text, str);
            }
            else node.SafeSetActive(false);
        }
        else if (type == TaskType.Easy || type == TaskType.Difficult || type == TaskType.Nightmare)
        {
            if (data < moduleChase.firstMoreMax)
            {
                RefreshBtnIntercable(node, true);
                Util.SetText(text, moduleChase.GetCurrentLevelString(data));
            }
            else if (data == moduleChase.firstMoreMax)
            {
                RefreshBtnIntercable(node, false);

                string str = ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 33);

                var chaseTask = moduleChase.allChaseTasks.Find(p => p.taskConfigInfo.isSpecialTask && p.taskConfigInfo.level == data - 1 && p.taskData.state == (byte)EnumChaseTaskFinishState.Finish);
                var next = moduleChase.allTasks.FindAll(p => moduleChase.GetCurrentTaskType(p) == type && p.level == data);

                if (chaseTask != null && next != null && next.Count > 0)
                {
                    next.Sort((a, b) => a.ID.CompareTo(b.ID));
                    if (modulePlayer.level < next[0].unlockLv)
                    {
                        string str_n = ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 52);
                        Util.SetText(text, str + str_n, next[0].unlockLv);
                    }
                    else Util.SetText(text, str);
                }
                else Util.SetText(text, str);
            }
            else node.SafeSetActive(false);
        }
    }

    private void SetSelectText(TaskType type, int level)
    {
        string _text = "";
        if (type == TaskType.Emergency)
        {
            _text = Util.GetString(296, level - 1);
            if (_text.Length > 1) _text = _text.Substring(0, 1);
        }
        else _text = level.ToString();
        Util.SetText(selectText, _text);
    }

    private void RefreshBtnIntercable(Transform node, bool isTrue)
    {
        if (node == null) return;
        node.SafeSetActive(true);
        var images = node.GetComponentsInChildren<Image>(true);
        if (images != null)
        {
            for (int i = 0; i < images.Length; i++)
                images[i].saturation = isTrue ? 1 : 0;
        }
    }

    private void SetStarEnterState(TaskType type, int level)
    {
        normalStar.SafeSetActive(type == TaskType.Easy);
        diffStar.SafeSetActive(type == TaskType.Difficult);
        currentLvStarNumber.text = Util.Format("{0}/{1}", moduleChase.GetCurrentLevelGetStar(type, level), moduleChase.GetCurrentLevelTotalStar(type, level));
        moduleChase.SendStarPanelState();
    }

    private void ClickGoToBtnState(TaskType type, int level)
    {
        AnyPanelState();

        taskPanel.SafeSetActive(type != TaskType.Emergency && type != TaskType.Awake);
        tittlePanel.SafeSetActive(type != TaskType.Awake);

        normalBg.SafeSetActive(type == TaskType.Easy || type == TaskType.Difficult || type == TaskType.Nightmare);
        if (type == TaskType.Easy || type == TaskType.Difficult || type == TaskType.Nightmare)
        {
            var task = moduleChase.allTasks.Find(p => moduleChase.GetCurrentTaskType(p) == type && p.level == level);
            if (task)
                AtlasHelper.SetShared(normalBackGround, task.bgIcon);
        }
        activeBg.SafeSetActive(type == TaskType.Active);

        scrollPage.SafeSetActive(type != TaskType.Emergency);
        emergency_panel.SafeSetActive(type == TaskType.Emergency);
        selectHardTran.SafeSetActive(type != TaskType.Emergency && type != TaskType.Active);       
        activeTittle.SafeSetActive(type == TaskType.Active);
        emergencyTittle.SafeSetActive(type == TaskType.Emergency);
        starRewardEnter.SafeSetActive(type == TaskType.Easy || type == TaskType.Difficult);
        active_preview.SafeSetActive(type == TaskType.Active);

        easyDireTran.SafeSetActive(type == TaskType.Easy||type==TaskType.Active);
        diffDireTran.SafeSetActive(type == TaskType.Difficult || type == TaskType.Nightmare);

        starRewardPanel.SafeSetActive(false);
        selectActivePanel.SafeSetActive(false);
        ToDifferentModel(type);
    }

    private void AnyPanelState()
    {
        moduleGlobal.ShowGlobalLayerDefault(1, false);
        enterPanel.SafeSetActive(false);
        npcTrans.SafeSetActive(false);
    }
    #endregion

    #region other functions

    public void RefreshPayPanel(ChaseTask info)
    {
        payCountPanel.SafeSetActive(true);

        var costs = info.taskConfigInfo.cost;
        int count = costs.Length - 1;
        for (int i = count; i >= 0; i--)
        {
            if (string.IsNullOrEmpty(costs[i]))
            {
                Array.Resize(ref costs, count);
                break;
            }
        }

        int index = 0;
        index = info.taskData.resetTimes >= costs.Length - 1 ? costs.Length - 1 : info.taskData.resetTimes;
        if (index < 0 || index > costs.Length - 1) return;

        string[] str = costs[index].Split('-');
        if (str.Length < 2)
            return;
        var prop = ConfigManager.Get<PropItemInfo>(Util.Parse<int>(str[0]));

        contentTip.text = Util.Format(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 30), str[1], prop.itemName);
        int remainCount = info.taskConfigInfo.dayRemainCount - info.taskData.resetTimes;
        currentDayCount.text = Util.Format(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 31), remainCount >= 0 ? remainCount : 0);
        AtlasHelper.SetItemIcon(costIcon, prop);

        bool canPay = modulePlayer.roleInfo.diamond >= Util.Parse<int>(str[1]);
        string colorStr = GeneralConfigInfo.GetNoEnoughColorString(modulePlayer.roleInfo.diamond.ToString());
        Util.SetText(costCount, (int)TextForMatType.ChaseUIText, 32, str[1], canPay ? modulePlayer.roleInfo.diamond.ToString() : colorStr);

        sureBtn.interactable = canPay && remainCount > 0;
        sureBtn.onClick.RemoveAllListeners();
        sureBtn.onClick.AddListener(() =>
        {
            moduleChase.SendRestChallenge((ushort)info.taskConfigInfo.ID);
            payCountPanel.SafeSetActive(false);
        });
    }

    #endregion

    #region module event
    //紧急任务,创建房间成功
    private void OnMatchSuccess(ScTeamPveMatchSuccess msg)
    {
        //这个窗口只接受直接创建房间的协议，加入其它房间的协议由Window_CreateRoom去处理
        var w = Window.GetOpenedWindow<Window_CreateRoom>();
        if (w && w.actived) return;

        if (msg.result == 3)
        {
            Window_Alert.ShowAlertDefalut(ConfigText.GetDefalutString((int)TextForMatType.AwakeStage, 10),
            () => { moduleAwakeMatch.Request_EnterRoom(false, currentTask); },
            () => { });
            return;
        }

        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9806, msg.result);
            return;
        }

        ShowAsync<Window_TeamMatch>();
    }

    private void _ME(ModuleEvent<Module_AwakeMatch> e)
    {
        if (!actived) return;
        switch (e.moduleEvent)
        {
            case Module_AwakeMatch.Notice_MatchSuccess:
                OnMatchSuccess(e.msg as ScTeamPveMatchSuccess);
                break;
            case Module_AwakeMatch.Response_SwitchStage:
                if (Window_TeamMatch.IsChooseStage)
                    ShowAsync<Window_TeamMatch>();
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Chase> e)
    {
        if (!actived) return;

        switch (e.moduleEvent)
        {
            case Module_Chase.EventIsSpecial://刷新特殊界面
            {
                ChaseTask specialTask = e.param1 as ChaseTask;
                if (specialTask != null) specialpanel.ResfreshSpecial(specialTask);
                break;
            }
            case Module_Chase.EventRefreshChaseTask://刷新所有任务
            {
                var type = (TaskType)e.param1;
                var level = (int)e.param2;
               
                bool isType = type == TaskType.Easy || type == TaskType.Difficult || type == TaskType.Nightmare || type == TaskType.Emergency;
                bool isInitialized = type != initType;
                selectBoxParent.SafeSetActive(isType);

                ClickGoToBtnState(type, level);
                if (isType && isInitialized)
                {
                    selectBox.SetParent(selectBoxParent?.transform);
                    InitLevelCheckBox(type);
                }
                else
                {
                    if(isType) dataSelect.UpdateItems();
                }

                if (type != TaskType.Emergency)
                {
                    var list = e.param3 as List<Chase_PageItem>;

                    if (type != TaskType.Emergency) RefreshScollPage(list);
                    SetUpAndDownImageState();

                    if (type == TaskType.Active) SetActiveState(level);
                }
                else
                {
                    var target = e.param3 as ChaseTask;
                    currentTask = target;
                    emergency.UnInitialize();
                    emergency.Initialize(target);
                }               
                break;
            }
            case Module_Chase.EventRefreshRewardPanel://领取成功后刷新星级奖励界面
            {
                starData.UpdateItems();
                moduleChase.SendStarPanelState();
                break;
            }
            case Module_Chase.EventOpenStarRewardPanel://打开领取星级奖励界面
            {
                if (!actived) break;
                SetRewardMark(moduleChase.currentStarRewardInfo);
                starRewardEnter.onClick.RemoveAllListeners();
                starRewardEnter.onClick.AddListener(OnOpenStarRewardPanel);
                break;
            }
            //获得数据后刷新
            case Module_Chase.EventGetChaseInfo: RefreshEmergencySaturation(); break;
            case Module_Chase.EventRefreshMainActivepanel: RefreshActiveSaturation(); break;
            case Module_Chase.EventRefreshActiveAddAndRemove:
            {
                RefreshActiveSaturation();
                if (selectActivePanel.gameObject.activeInHierarchy)
                    RefreshActive();
                break;
            }
            //case Module_Chase.EventSkiptargetChaseTask: if (actived) moduleChase.SkipTargetTask(moduleChase.targetTaskFromForge); break;
            default: break;
        }
    }

    private void _ME(ModuleEvent<Module_Story> e)
    {
        if (!actived) return;
        if (e.moduleEvent == Module_Story.EventStoryEnd)
        {
            moduleGlobal.ShowGlobalLayerDefault(1, enterPanel.gameObject.activeInHierarchy);
        }

    }

    private void OnSetPageItem(RectTransform node, Chase_PageItem data)
    {
        var item = node.GetComponentDefault<ChasePageItem>();
        item.RefreshPage(data, OnItemClick);
    }

    private void RefreshScollPage(List<Chase_PageItem> chaseTasks)
    {
        if (chaseTasks.Count > 0)
        {
            dataPage.SetItems(chaseTasks);
            var task = moduleChase.targetTaskFromForge;
            var last = moduleChase.lastStartChase;

            int index = 0;
            if (last == null || last.isFirst) index = chaseTasks[chaseTasks.Count - 1].current_Index;
            else if (last != null)
            {
                var pageItem = chaseTasks.Find(p => p.current_Tasks.Find(o => o.ID == last.taskData.taskId));
                if (pageItem != null) index = pageItem.current_Index;
            }

            if (task != null)
            {
                var pageItem = chaseTasks.Find(p => p.current_Tasks.Find(o => o.ID == task.taskData.taskId));
                if (pageItem != null) index = pageItem.current_Index;
            }

            if (scrollPage.currentPage != index) scrollPage.ScrollToPage(index);

            if (task == null) return;
            if (task.taskType == TaskType.Easy || task.taskType == TaskType.Difficult || task.taskType == TaskType.Nightmare || task.taskType == TaskType.Active)
            {
                if (moduleChase.isShowDetailPanel)
                {
                    OnItemClick(task);
                    moduleChase.isShowDetailPanel = false;
                }
            }
        }
    }

    private void RefreshActiveSaturation()
    {
        if (activeImages != null && activeImages.Length > 0)
        {
            for (int i = 0; i < activeImages.Length; i++)
                activeImages[i].saturation = moduleChase.openActiveItems.Count > 0 ? 1 : 0;
        }

        if (activeTexts != null && activeTexts.Length > 0)
        {
            for (int i = 0; i < activeTexts.Length; i++)
                activeTexts[i].saturation = moduleChase.openActiveItems.Count > 0 ? 1 : 0;
        }
    }

    private void RefreshEmergencySaturation()
    {
        if (emergencyImages != null && emergencyImages.Length > 0)
        {
            bool isFalse = moduleChase.emergencyList == null || moduleChase.emergencyList.Count < 1;
            for (int i = 0; i < emergencyImages.Length; i++)
                emergencyImages[i].saturation = isFalse ? 0 : 1;
        }

        if (emergencyTexts != null && emergencyTexts.Length > 0)
        {
            bool isFalse = moduleChase.emergencyList == null || moduleChase.emergencyList.Count < 1;
            for (int i = 0; i < emergencyTexts.Length; i++)
                emergencyTexts[i].saturation = isFalse ? 0 : 1;
        }
    }

    private void SetToggleState(float progress)
    {
        SetUpAndDownImageState();
    }

    private void SetUpAndDownImageState()
    {
        int count = 0;
        int maxPage = 0;
        var type = moduleChase.currentSelectType;
        var level = moduleChase.currentClickLevel;
        switch (type)
        {
            case TaskType.Easy:
            {
                if (!moduleChase.easyTasksToMaxDic.ContainsKey(level)) return;
                count = moduleChase.easyTasksToMaxDic[level].Count;
                maxPage = count > 0 ? count - 1 : 0;
                if (scrollPage == null) return;
                easyUpBtn.SafeSetActive(count > 0 && scrollPage.currentPage > 0);
                easyUpBtn.onClick.RemoveAllListeners();
                easyUpBtn.onClick.AddListener(() => { OnClickUpOrDownBtn(true, maxPage); });

                easyDownBtn.SafeSetActive(count > 0 && scrollPage.currentPage < maxPage);
                easyDownBtn.onClick.RemoveAllListeners();
                easyDownBtn.onClick.AddListener(() => { OnClickUpOrDownBtn(false, maxPage); });
                break;
            }
            case TaskType.Difficult:
            {
                if (!moduleChase.diffTasksToMaxDic.ContainsKey(level)) return;
                count = moduleChase.diffTasksToMaxDic[level].Count;
                maxPage = count > 0 ? count - 1 : 0;
                if (scrollPage == null) return;
                diffUpBtn.SafeSetActive(count > 0 && scrollPage.currentPage > 0);
                diffUpBtn.onClick.RemoveAllListeners();
                diffUpBtn.onClick.AddListener(() => { OnClickUpOrDownBtn(true, maxPage); });

                diffDownBtn.SafeSetActive(count > 0 && scrollPage.currentPage < maxPage);
                diffDownBtn.onClick.RemoveAllListeners();
                diffDownBtn.onClick.AddListener(() => { OnClickUpOrDownBtn(false, maxPage); });
                break;
            }
            case TaskType.Nightmare:
            {
                if (!moduleChase.nightmareTasksToMax.ContainsKey(level)) return;
                count = moduleChase.nightmareTasksToMax[level].Count;
                maxPage = count > 0 ? count - 1 : 0;
                if (scrollPage == null) return;
                diffUpBtn.SafeSetActive(count > 0 && scrollPage.currentPage > 0);
                diffUpBtn.onClick.RemoveAllListeners();
                diffUpBtn.onClick.AddListener(() => { OnClickUpOrDownBtn(true, maxPage); });

                diffDownBtn.SafeSetActive(count > 0 && scrollPage.currentPage < maxPage);
                diffDownBtn.onClick.RemoveAllListeners();
                diffDownBtn.onClick.AddListener(() => { OnClickUpOrDownBtn(false, maxPage); });
                break;
            }
            case TaskType.Active:
            {
                if (!moduleChase.activeToMaxDic.ContainsKey(level)) return;
                count = moduleChase.activeToMaxDic[level].Count;
                maxPage = count > 0 ? count - 1 : 0;
                if (scrollPage == null) return;
                easyUpBtn.SafeSetActive(count > 0 && scrollPage.currentPage > 0);
                easyUpBtn.onClick.RemoveAllListeners();
                easyUpBtn.onClick.AddListener(() => { OnClickUpOrDownBtn(true, maxPage); });

                easyDownBtn.SafeSetActive(count > 0 && scrollPage.currentPage < maxPage);
                easyDownBtn.onClick.RemoveAllListeners();
                easyDownBtn.onClick.AddListener(() => { OnClickUpOrDownBtn(false, maxPage); });
                break;
            }
            default: break;
        }
    }

    private void OnClickUpOrDownBtn(bool isUp, int maxPage)
    {
        int index = scrollPage.currentPage;
        if (isUp)
        {
            index--;
            index = index < 0 ? 0 : index;
        }
        else
        {
            index++;
            index = index > maxPage ? maxPage : index;
        }

        if (scrollPage.currentPage != index) scrollPage.ScrollToPage(index);
    }

    public void OnItemClick(ChaseTask info)
    {
        currentTask = info;
        if (Window_TeamMatch.IsChooseStage)
        {
            bool isSend = moduleAwakeMatch.Request_SwitchStage(info.taskConfigInfo.ID);
            if (!isSend) ShowAsync<Window_TeamMatch>();
        }
        else if (moduleChase.currentSelectType == TaskType.Nightmare)
        {
            SetWindowParam<Window_CreateRoom>(0, info);
            ShowAsync<Window_CreateRoom>();
            return;
        }
        SetWindowParam<Window_Assist>(info);
        ShowAsync<Window_Assist>();
    }
    #endregion

    #region active

    private void OnProgressChanged(float arg0)
    {
        leftBtn.SafeSetActive(activeView.progress > 0.01f && isShowBtn);
        rigBtn.SafeSetActive(activeView.progress < 0.99f && isShowBtn);
    }

    private void OnIndexChanged(int arg0)
    {
        moduleChase.lastSelectIndex = arg0;
    }

    private void OnGoToDirction(bool isLeft)
    {
        if (isLeft)
        {
            moduleChase.lastSelectIndex--;
            if (moduleChase.lastSelectIndex < 0)
            {
                moduleChase.lastSelectIndex = 0;
                return;
            }
        }
        else
        {
            moduleChase.lastSelectIndex++;
            int mostRig = activeData.count - activeView.viewCount;
            if (moduleChase.lastSelectIndex > mostRig)
            {
                moduleChase.lastSelectIndex = mostRig;
                return;
            }
        }
        activeView.ScrollToIndex(moduleChase.lastSelectIndex, 0.4f);
    }

    private void OnClickActiveItem(RectTransform node, ActiveTaskInfo data)
    {
        if (modulePlayer.roleInfo.level < data.openLv)
        {
            moduleGlobal.ShowMessage(Util.Format(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 38), data.openLv));
            return;
        }

        if (!moduleChase.activeChallengeCount.ContainsKey(data.taskLv)) return;

        moduleChase.lastSelectIndex = moduleChase.openActiveItems.FindIndex(p => p.ID == data.ID);
        moduleChase.SkipTargetTypeTasks(TaskType.Active, data.taskLv);
    }

    private void OnSetActiveItems(RectTransform node, ActiveTaskInfo data)
    {
        UIDynamicImage.LoadImage(node.Find("icon"), data.activeIcon);
        UIDynamicImage.LoadImage(node.Find("icon/npcimg"), data.activeSubIcon);
        Util.SetText(node.Find("bg/Text")?.gameObject, data.activeName);
        Util.SetText(node.Find("bg/Text (1)")?.gameObject, ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 53));
        Util.SetText(node.Find("line/Text")?.gameObject, data.activeDesc);
        Util.SetText(node.Find("opentime")?.gameObject, data.openTimeDesc);

        var shadow = node.Find("line/Text").GetComponent<Shadow>();
        if (shadow && GeneralConfigInfo.defaultConfig.activeShadowColors.Length > data.ID - 1)
            shadow.effectColor = GeneralConfigInfo.defaultConfig.activeShadowColors[data.ID - 1];

        Transform off = node.Find("off");
        UIDynamicImage.LoadImage(off, data.activeIcon);
        bool isOff = modulePlayer.roleInfo.level < data.openLv;
        off.SafeSetActive(isOff);
        string str = ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 38);
        if (isOff) Util.SetText(node.Find("off/off_bg/Text")?.gameObject, str, data.openLv.ToString());

        var color1 = node.Find("line/Image (1)")?.GetComponent<Image>();
        if (color1) color1.color = data.activeTextColor != null && data.activeTextColor.Length > 0 ? data.activeTextColor[0] : Color.white;
        var color2 = node.Find("line/Image (2)")?.GetComponent<Image>();
        if (color2) color2.color = data.activeTextColor != null && data.activeTextColor.Length > 1 ? data.activeTextColor[1] : Color.white;
        var color3 = node.Find("line/Image")?.GetComponent<Image>();
        if (color3) color3.color = data.activeTextColor != null && data.activeTextColor.Length > 2 ? data.activeTextColor[2] : Color.white;
    }

    private void CreateActiveContains()
    {
        AnyPanelState();
        selectActivePanel.SafeSetActive(true);
        taskPanel.SafeSetActive(false);
        tittlePanel.SafeSetActive(true);
        selectHardTran.SafeSetActive(false);
        selectBoxParent.SafeSetActive(false);
        emergencyTittle.SafeSetActive(false);
        activeTittle.SafeSetActive(true);

        RefreshActive();
    }

    private void RefreshActive()
    {
        activeData.SetItems(moduleChase.openActiveItems);
        int index = moduleChase.lastSelectIndex;

        isShowBtn = moduleChase.openActiveItems.Count > activeView.viewCount;

        if (index >= 0 && index < activeData.count)
            activeView.ScrollToIndex(index, 0.4f);

        OnProgressChanged(activeView.progress);
    }

    private void SetActiveState(int level)
    {
        ActiveTaskInfo info = moduleChase.allActiveItems.Find(p => p.taskLv == level);
        if (!info) return;

        if (rewardsParent.childCount > 0)
        {
            for (int i = 0; i < rewardsParent.childCount; i++)
                rewardsParent.GetChild(i).SafeSetActive(false);
        }

        //可能掉落
        int[] rewards = info.dropProps;
        for (int i = 0; i < rewards.Length; i++)
        {
            ushort id = (ushort)rewards[i];
            var prop = ConfigManager.Get<PropItemInfo>(id);
            if (!prop || !prop.IsValidVocation(modulePlayer.proto)) continue;

            Transform tr = rewardsParent.childCount > i ? rewardsParent.GetChild(i) : null;
            if (tr == null) tr = rewardsParent.AddNewChild(rewardItem);
            tr.SafeSetActive(true);
            Util.SetItemInfoSimple(tr, prop);
            var btn = tr.GetComponentDefault<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => moduleGlobal.UpdateGlobalTip(id, true));
        }

        //挑战次数
        int number = 0;
        if (moduleChase.activeChallengeCount != null && moduleChase.activeChallengeCount.ContainsKey(info.taskLv))
            number = info.crossLimit - moduleChase.activeChallengeCount[info.taskLv];

        string str = GeneralConfigInfo.GetNoEnoughColorString(number < 0 ? "0" : number.ToString());
        challengeCount.text = $"{(number > 0 ? number.ToString() : str)}/{info.crossLimit}";
    }

    #endregion

    #region starReward

    private void OnOpenStarRewardPanel()
    {
        var type = moduleChase.rewardType;
        var level = moduleChase.rewardLevel;
        if (!(type == TaskType.Easy || type == TaskType.Difficult)) return;

        starRewardPanel.SafeSetActive(true);
        starRewardLevelTittle.text = Util.Format(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 34), moduleChase.GetCurrentLevelString(level));

        int currentGetStarNumber = moduleChase.GetCurrentLevelGetStar(type, level);
        int totalStarNumber = moduleChase.GetCurrentLevelTotalStar(type, level);
        currentStarCount.text = $"{currentGetStarNumber}/{totalStarNumber}";
        starData.SetItems(moduleChase.currentStarRewardInfo);
        view.progress = 0;
    }

    private void OnSetStarRewardInfo(RectTransform node, ChaseLevelStarInfo data)
    {
        Util.SetText(node.Find("condition_text_left")?.GetComponent<Text>(), (int)TextForMatType.ChaseUIText, 44);
        Util.SetText(node.Find("condition_text")?.GetComponent<Text>(), data.starCount.ToString());
        Transform item_parent = node.Find("rewardParent");
        Transform goodsObj = node.Find("0");
        goodsObj.SafeSetActive(false);
        Button getBtn = node.Find("getBtn")?.GetComponent<Button>();
        Util.SetText(node.Find("getBtn/get_text")?.GetComponent<Text>(), (int)TextForMatType.ChaseUIText, 46);
        Transform alreadGet = node.Find("alreadyGet");

        //奖品
        int index = 0;
        for (int i = 0; i < data.rewards.Length; i++)
        {
            string[] strs = data.rewards[i].Split('-');
            if (strs.Length != 4 || Util.Parse<int>(strs[0]) <= 0)
            {
                index = i;
                continue;
            }

            ushort id = (ushort)Util.Parse<int>(strs[0]);
            var prop = ConfigManager.Get<PropItemInfo>(id);
            if (!prop || !(prop.proto.Contains(CreatureVocationType.All) || prop.proto.Contains((CreatureVocationType)modulePlayer.proto))) continue;
            Transform newItem = item_parent.childCount > index ? item_parent.GetChild(index) : null;
            if (newItem == null) newItem = item_parent.AddNewChild(goodsObj);
            newItem.SafeSetActive(true);
            if (prop.itemType == PropType.Rune || prop.itemType == PropType.Weapon ||
                (prop.itemType == PropType.FashionCloth && (FashionSubType)prop.subType == FashionSubType.FourPieceSuit))
            {
                Util.SetItemInfo(newItem, prop, prop.itemType == PropType.Rune ? Util.Parse<int>(strs[1]) : 0, 0, true, Util.Parse<int>(strs[2]));
            }
            else Util.SetItemInfoSimple(newItem, prop);

            var btn = newItem.GetComponentDefault<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => moduleGlobal.UpdateGlobalTip(id, true));

            Util.SetText(newItem.Find("numberdi/count")?.GetComponent<Text>(), "×" + strs[3]);
            index++;
        }

        var type = data.isRare == 0 ? TaskType.Easy : TaskType.Difficult;
        var level = data.level;
        int currentGetStarNumber = moduleChase.GetCurrentLevelGetStar(type, level);

        if (moduleChase.getRewardState.ContainsKey((byte)data.ID))
        {
            if (getBtn != null)
            {
                getBtn.SafeSetActive(!moduleChase.getRewardState[(byte)data.ID]);
                getBtn.interactable = currentGetStarNumber >= data.starCount;
                alreadGet.SafeSetActive(moduleChase.getRewardState[(byte)data.ID]);
                if (!moduleChase.getRewardState[(byte)data.ID])
                {
                    getBtn.onClick.RemoveAllListeners();
                    getBtn.onClick.AddListener(() =>
                    {
                        moduleChase.currentGetRewardID = (byte)data.ID;
                        moduleChase.SendGetReward((byte)data.ID);
                    });
                }
            }
        }
    }

    private void SetRewardMark(List<ChaseLevelStarInfo> rewardsList)
    {
        if (rewardsList == null || rewardsList.Count < 1) return;

        var type = rewardsList[0].isRare == 0 ? TaskType.Easy : TaskType.Difficult;
        var level = rewardsList[0].level;
        int num = moduleChase.GetCurrentLevelGetStar(type, level);

        bool isMark = false;
        for (int i = 0; i < rewardsList.Count; i++)
        {
            if (moduleChase.getRewardState != null && moduleChase.getRewardState.ContainsKey((byte)rewardsList[i].ID))
            {
                bool isDraw = moduleChase.getRewardState[(byte)rewardsList[i].ID];
                if (num >= rewardsList[i].starCount && !isDraw)
                {
                    isMark = true;
                    break;
                }
            }
        }
        canGetReward.SafeSetActive(isMark);
    }
    #endregion
}