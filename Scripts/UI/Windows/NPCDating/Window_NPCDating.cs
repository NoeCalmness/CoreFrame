/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-12-26
 * 
 ***************************************************************************************************/

using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Window_NPCDating : Window
{
    #region Fields
    private Image m_NpcAvatar;

    //任务列表面板
    private Transform m_tfPanelTask;
    private Button m_btnTaskPanel,m_btnCloseTaskPanel;
    private DataSource<Task> m_taskInfoDataSource;
    private ScrollView taskItemScrollView;
    private Text m_textTaskName, m_textTaskDesc, m_textTaskTip;
    private ToggleGroup m_togMissionGroup;

    //任务提示面板
    private Transform m_tfMissionTipPanel;

    private ScrollView m_svMissionReceive, m_svMissionFinished;
    private DataSource<Task> m_dsMissionReceive, m_dsMissionFinished;

    //好感度提升提示
    private Transform m_tfGoodFeelingUpTip;

    //NPC属性
    private Transform m_tfPMoodLevel;
    private RectTransform m_rtfMoodSliderBg;
    private RectTransform m_rtfNpcMoodProgress;

    private Text m_textNpcPower;
    private Image m_imgNpcPowerProgress;

    //对话回顾
    private Button m_btnRecordDialogue;

    //约会场景二级界面
    private Transform m_tfSecondScenePanel, m_tfSceneOpenEvent, m_tfSceneImage;
    private Button m_btnCloseScenePanel, m_btnEnterScene;
    private Text m_textSceneName, m_textSceneOpenTime, m_textSceneDesc, m_textConsumePower;
    private Transform m_tfSceneBottomImage;
    private Image m_imgSceneTopImage;

    //随机独白框
    private Transform m_tfRandomDialogue;

    TweenAlpha m_taShowUI;

    //主界面滑动
    private Level_Home m_levelHome;
    private ScrollRect m_srMap;

    //任务推荐
    private Button m_btnTaskRecommend;
    private Image m_imgTaskRecommendIcon;
    private Text m_txtTaskRecommendName;

    //游玩指南
    private Text m_textGuideContent;

    //uiData
    private bool m_bOpenMissionList = false;//在当前界面是否已经打开过任务列表面板
    private Task m_curClickTask;

    private float m_curContentPos,m_lastContentPos,m_aspect;
    private bool m_bClickState = true;
    private Vector3 m_v3CameraPos,m_datingCamLeftPos, m_datingCamRightPos, m_datingMaxCamLeftPos, m_datingMaxCamRightPos;

    private DatingSceneConfig m_curOpenSceneData;//当前打开的场景二级界面数据

    #endregion

    #region Override method
    protected override void OnOpen()
    {
        m_levelHome = Level.current as Level_Home;

        //约会NPC头像
        m_NpcAvatar = GetComponent<Image>("avatar");

        //任务列表面板
        taskItemScrollView = GetComponent<ScrollView>("panel_task/taskItemScrollView");
        m_taskInfoDataSource = new DataSource<Task>(null, taskItemScrollView, SetTaskItemData, ClickTaskItem);
        m_tfPanelTask = GetComponent<RectTransform>("panel_task");
        m_btnTaskPanel = GetComponent<Button>("task_btn");
        m_btnTaskPanel.onClick.AddListener(OnClickTaskPanel);
        m_btnCloseTaskPanel = GetComponent<Button>("panel_task/btnClose");
        m_btnCloseTaskPanel.onClick.AddListener(()=> m_tfPanelTask.SafeSetActive(false));
        m_textTaskName = GetComponent<Text>("panel_task/taskName");
        m_textTaskDesc = GetComponent<Text>("panel_task/taskDesc");
        m_textTaskTip = GetComponent<Text>("panel_task/taskTip");
        m_togMissionGroup = GetComponent<ToggleGroup>("panel_task/taskItemScrollView");

        //任务提示面板
        m_tfMissionTipPanel = GetComponent<RectTransform>("taskTipPanel");
        m_svMissionReceive = GetComponent<ScrollView>("taskTipPanel/scrollViewReceive");
        m_dsMissionReceive = new DataSource<Task>(null, m_svMissionReceive, OnSetMissionReceiveData, null);
        m_svMissionFinished = GetComponent<ScrollView>("taskTipPanel/scrollViewFinished");
        m_dsMissionFinished = new DataSource<Task>(null, m_svMissionFinished, OnSetMissionFinishedData, null);

        //好感度提升提示
        m_tfGoodFeelingUpTip = GetComponent<RectTransform>("goodFeelingUpPanel");

        //NPC属性
        m_tfPMoodLevel = GetComponent<RectTransform>("moodProgressBar/levelInfo");
        m_rtfNpcMoodProgress = GetComponent<RectTransform>("moodProgressBar/Fill");
        m_rtfMoodSliderBg = GetComponent<RectTransform>("moodProgressBar/bg");

        m_textNpcPower = GetComponent<Text>("energy/energyNumber");
        m_imgNpcPowerProgress = GetComponent<Image>("energy/energyFill");
        //对话回顾
        m_btnRecordDialogue = GetComponent<Button>("history_btn"); m_btnRecordDialogue.onClick.AddListener(() => moduleNPCDating.OpenReviewWindow());

        //打开场景二级界面
        m_tfSecondScenePanel = GetComponent<RectTransform>("sceneContent");
        m_tfSceneOpenEvent = GetComponent<RectTransform>("sceneContent/dailyEvent/dailyEventGroup");
        m_btnCloseScenePanel = GetComponent<Button>("sceneContent/closeBtn"); m_btnCloseScenePanel.onClick.AddListener(OnClickCloseScenePanel);
        m_btnEnterScene = GetComponent<Button>("sceneContent/confirmBtn"); m_btnEnterScene.onClick.AddListener(OnClickEnterScene);
        m_tfSceneImage = GetComponent<RectTransform>("sceneContent/back/map");
        m_textSceneName = GetComponent<Text>("sceneContent/title");
        m_textSceneOpenTime = GetComponent<Text>("sceneContent/serviceTime/Text");
        m_textSceneDesc = GetComponent<Text>("sceneContent/content");
        m_textConsumePower = GetComponent<Text>("sceneContent/consume/content");
        m_tfSceneBottomImage = GetComponent<RectTransform>("sceneContent/decBottom");
        m_imgSceneTopImage = GetComponent<Image>("sceneContent/decTop");

        //随机独白
        m_tfRandomDialogue = GetComponent<RectTransform>("randomDialogue");

        //控制UI组件显隐的动画组件
        m_taShowUI = GetComponent<TweenAlpha>("uiTweenAlpha");

        //主界面滑动
        m_srMap = GetComponent<ScrollRect>("DatingMapScroll"); m_srMap.onValueChanged.AddListener(OnScrollRectValueChanged);
        var srMapTrigger = m_srMap.GetComponentDefault<EventTriggerListener>();
        srMapTrigger.onDown += OnScrollRectDown;
        srMapTrigger.onUp += OnScrollRectUp;
        srMapTrigger.onPressBegin += OnScrollRectBeginDrag;
        srMapTrigger.onPressEnd += OnScrollRectEndDrag;

        //任务推荐
        m_btnTaskRecommend = GetComponent<Button>("missionTip"); m_btnTaskRecommend.onClick.AddListener(OnClickTaskPanel);
        m_imgTaskRecommendIcon = GetComponent<Image>("missionTip/missionIcon");
        m_txtTaskRecommendName = GetComponent<Text>("missionTip/missionTitle");

        //游玩指南
        m_textGuideContent = GetComponent<Text>("tip_notice/viewport/content");

        m_aspect = UIManager.instance._canvasScaler.referenceResolution.x / UIManager.referenceResolution.x;
        m_datingCamLeftPos = GeneralConfigInfo.sdatingMapCamera.leftPos;
        m_datingCamRightPos = GeneralConfigInfo.sdatingMapCamera.rightPos;
        m_datingMaxCamLeftPos = GeneralConfigInfo.sdatingMapCamera.maxLeftPos;
        m_datingMaxCamRightPos = GeneralConfigInfo.sdatingMapCamera.maxRightPos;

        InitText();
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        //从服务器获取界面数据
        moduleNPCDating.SendOpenDatingWindow();

        moduleGlobal.ShowGlobalLayerDefault(1, false);

        Init();

        moduleNPCDating.SetCurDatingScene(EnumNPCDatingSceneType.DatingHall);

        if (m_levelHome.datingScence.GetComponentDefault<DatingMapCtrl>() == null) m_levelHome.datingScence.gameObject.AddComponent<DatingMapCtrl>();
        moduleHome.DispatchEvent(Module_Home.EventSwitchCameraMode, Event_.Pop(CameraShowType.Dating));
    }

    #endregion

    #region Init

    private void Init()
    {
        m_srMap.StopMovement();
        m_srMap.content.anchoredPosition = Vector2.zero;
        m_curContentPos = 0;
        m_lastContentPos = 0;

        m_v3CameraPos = m_datingCamLeftPos;

        HideNpcIcon();
        MarkScene();//隐藏场景所有红点标记
        HideMissionTipPanel();
        HideGoodFeelingPanel();

        m_bOpenMissionList = false;

        OnClickCloseScenePanel();

        HideMissonRecommend();

        m_bClickState = true;
        m_rtfNpcMoodProgress.sizeDelta = new Vector2(0, m_rtfNpcMoodProgress.sizeDelta.y);
    }

    private void InitText()
    {
        var textData = ConfigManager.Get<ConfigText>((int)TextForMatType.NpcDating);
        if (textData == null) return;
        Util.SetText(GetComponent<Text>("task_btn/Text"), textData[0]);
        Util.SetText(GetComponent<Text>("history_btn/Text"), textData[1]);
        Util.SetText(GetComponent<Text>("panel_task/title"), textData[7]);

        //场景二级界面
        Util.SetText(GetComponent<Text>("sceneContent/serviceTime"), textData[12]);
        Util.SetText(GetComponent<Text>("sceneContent/dailyEvent"), textData[13]);
        Util.SetText(GetComponent<Text>("sceneContent/confirmBtn/Text"), textData[14]);

        //任务推荐
        Util.SetText(GetComponent<Text>("missionTip/inProgress"), textData[16]);
        //游玩指南
        Util.SetText(GetComponent<Text>("guide_btn/Text"), textData[17]);
        Util.SetText(GetComponent<Text>("tip_notice/top/equipinfo"), textData[18]);
        Util.SetText(m_textGuideContent, textData[19]);

    }
    #endregion

    #region Receive ModuleEvent
    void _ME(ModuleEvent<Module_NPCDating> e)
    {
        if (!actived) return;
        switch (e.moduleEvent)
        {
            case Module_NPCDating.EventEnterDatingPanel:
                if (moduleNPCDating.forceSettlement)
                {
                    moduleNPCDating.forceSettlement = false;
                    Window_Alert.ShowAlert(ConfigText.GetDefalutString(TextForMatType.NpcDating, 24), true, false, false, () => { Window.ShowAsync<Window_Home>(); });
                    RefreshPanel();
                    moduleNPCDating.ClearDatingData();
                }
                else if (moduleNPCDating.isNpcBattelSettlement)
                {
                    moduleNPCDating.isNpcBattelSettlement = false;
                    //助战导致约会结算，开始一个特殊的结算事件流程,特殊id需要配表
                    moduleNPCDating.DoDatingEvent(GeneralConfigInfo.defaultConfig.datingEndEventId);
                }
                else
                {
                    bool needReconnect = moduleNPCDating.CheckDatingReconnect(EnumNPCDatingSceneType.DatingHall);
                    if (!needReconnect) moduleNPCDating.ContinueBehaviourCallBack(RefreshPanel);
                }
                break;
            case Module_NPCDating.EventRealEnterDatingScene:
                //点击娱乐街场景按钮
                switch ((sbyte)e.param1)
                {
                    case 0:
                        //成功
                        int sceneId = moduleNPCDating.enterSceneData.sceneId;
                        Game.LoadLevel(sceneId);
                        moduleNPCDating.isEnterSceneMark = true;
                        break;
                    case 1:
                        //当前没有约会Npc，请选择Npc
                        moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.NpcDating, 4));
                        break;
                    case 2:
                        //已经进入过该场景
                        moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.NpcDating, 5));
                        break;
                    case 3:
                        //约会已经结束
                        moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.NpcDating, 6));
                        break;
                    case 4:
                        //图书馆答错，未去其他场景再进图书馆提示
                        moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.NpcDating, 20));
                        break;
                    case 5:
                        //Npc体力不足，无法进行约会
                        string str = ConfigText.GetDefalutString(TextForMatType.NpcDating, 23);
                        moduleGlobal.ShowMessage(Util.Format(str, moduleNPCDating.curDatingNpcMsg == null ? "": moduleNPCDating.curDatingNpcMsg.name));
                        break;
                    default:
                        break;
                }
                break;
            case Module_NPCDating.EventClickDatingBuild: OpenSecondScenePanel((EnumNPCDatingSceneType)e.param1); break;
        }

    }

    void _ME(ModuleEvent<Module_Story> e)
    {
        if (!actived) return;
        if (e.moduleEvent == Module_Story.EventStoryClosed)
        {
            moduleGlobal.ShowGlobalLayerDefault(1, false);
            RefreshPanel();
        }
    }
    #endregion

    #region Button Click
    private void OnClickTaskPanel()
    {
        ClearMissionListPanel();
        List<Task> tempList = moduleNPCDating.allMissionsList;
        if (tempList == null || tempList.Count == 0) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.NpcDating, 22));
        else
        {
            //打开任务列表面板
            m_tfPanelTask.SafeSetActive(true);
            RefreshTaskPanel(tempList);
        }
    }

    #endregion

    #region Dating Logic

    #region refresh main dating panel

    private void RefreshPanel()
    {
        SetNpcMoodGrade();
        if (moduleNPCDating.curDatingNpcMsg != null)
        {
            SetNpcIcon(moduleNPCDating.curDatingNpcMsg);
            SetNpcProperty(moduleNPCDating.curDatingNpcMsg, !moduleNPCDating.isFirstDating);
        }

        //第一次约会刷新界面
        if (moduleNPCDating.isFirstDating)
        {
            moduleNPCDating.isFirstDating = false;

            ShowMissionTipPanel();
            RefreshMissonRecommend();

            OpenRandomDialogue();
        }
        else if (moduleNPCDating.isDating)//正在约会
        {
            if (moduleNPCDating.isEnterSceneMark)
            {
                m_taShowUI?.PlayForward();
                moduleNPCDating.isEnterSceneMark = false;
            }

            m_taShowUI?.PlayReverse();
            m_tfGoodFeelingUpTip.SafeSetActive(moduleNPCDating.isFetterUp);
            moduleNPCDating.isFetterUp = false;
            ShowMissionTipPanel();
            RefreshMissonRecommend();
            OpenRandomDialogue();
        }
        else//约会结束
        {
            MarkScene();
            HideGoodFeelingPanel();
            m_tfRandomDialogue.SafeSetActive(false);
            HideMissonRecommend();
        } 
    }
    #endregion

    #region common function

    private void SetNpcIcon(Module_Npc.NpcMessage npc)
    {
        m_NpcAvatar.SafeSetActive(true);

        UIDynamicImage.LoadImage(m_NpcAvatar.transform, npc.npcInfo.datingAvatar);
    }

    private void HideNpcIcon()
    {
        m_NpcAvatar.SafeSetActive(false);
    }

    private void SetNpcProperty(Module_Npc.NpcMessage npc,bool bRoll)
    {
        //心情值
        int lastMood = npc.lastMood;
        int curMood = npc.mood;
        int maxMood = GeneralConfigInfo.defaultConfig.datingNpcMaxMood;

        //体力值
        int lastPower = npc.lastBodyPower;
        int curPower = npc.bodyPower;
        int maxPower = npc.maxBodyPower;

        //滚动
        if (bRoll)
        {
            //心情
            var fromMoodNum = m_rtfMoodSliderBg.sizeDelta.x * (maxMood <= 0 ? 0 : (float)lastMood / maxMood > 1 ? 1 : (float)lastMood / maxMood);
            var toMoodNum = m_rtfMoodSliderBg.sizeDelta.x * (maxMood <= 0 ? 0 : (float)curMood / maxMood > 1 ? 1 : (float)curMood / maxMood);
            AddRollNumber(m_rtfNpcMoodProgress, fromMoodNum, toMoodNum);

            //体力值
            AddRollNumber(m_textNpcPower, lastPower, curPower,2.0f);
            float fromPowerProNum = maxPower <= 0 ? 0 : (float)lastPower / npc.maxBodyPower;
            float toPowerProNum = maxPower <= 0 ? 0 : (float)curPower / npc.maxBodyPower;
            AddRollNumber(m_imgNpcPowerProgress, fromPowerProNum, toPowerProNum,2.0f);
        }
        else
        {
            //心情
            var toMoodNum = m_rtfMoodSliderBg.sizeDelta.x * (maxMood <= 0 ? 0 : (float)curMood / maxMood > 1 ? 1 : (float)curMood / maxMood);
            AddRollNumber(m_rtfNpcMoodProgress, toMoodNum, toMoodNum);

            //体力值
            AddRollNumber(m_textNpcPower, curPower, curPower);
            float toPowerProNum = maxPower <= 0 ? 0 : (float)curPower / npc.maxBodyPower;
            AddRollNumber(m_imgNpcPowerProgress, toPowerProNum, toPowerProNum);
        }

    }

    /// <summary>
    /// 设置心情的几个等级值
    /// </summary>
    private void SetNpcMoodGrade()
    {
        if (moduleNPCDating.openWindowData != null)
        {
            for (int i = 0; i < m_tfPMoodLevel.childCount; i++)
            {
                Transform level = m_tfPMoodLevel.GetChild(i);
                Text textVal = level.Find("level_number").GetComponent<Text>();
                if (i <= moduleNPCDating.openWindowData.moodGrades.Length - 1) Util.SetText(textVal, moduleNPCDating.openWindowData.moodGrades[i].ToString());
            }
        }
    }

    private void MarkScene(List<Task> tasks = null)
    {
        var listSceneIds = new List<int>();
        if (tasks != null)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                if (tasks[i].taskState == EnumTaskState.Begin || tasks[i].taskState == EnumTaskState.Runing)
                {
                    var tmpList = new List<int>(tasks[i].taskBelongScene);
                    listSceneIds.AddRange(tmpList);
                }
            }
            listSceneIds.Distinct();
        }

        moduleNPCDating.DispatchEvent(Module_NPCDating.EventNotifyDatingMapObject, Event_.Pop(EnumDatingNotifyType.RefreshRedDot, listSceneIds));
    }

    private void HideMissionTipPanel()
    {
        m_tfMissionTipPanel.SafeSetActive(false);
        m_svMissionReceive.SafeSetActive(false);
        m_svMissionFinished.SafeSetActive(false);
    }

    private void HideGoodFeelingPanel()
    {
        m_tfGoodFeelingUpTip.SafeSetActive(false);
    }

    /// <summary>
    /// 打开NPC随机独白
    /// </summary>
    private void OpenRandomDialogue()
    {
        m_tfRandomDialogue.SafeSetActive(true);
        //调用随机独白接口
        var rm = m_tfRandomDialogue?.GetComponentDefault<RandomMonologue>();

        var npcMsg = moduleNpc.GetTargetNpc((NpcTypeID)moduleNPCDating.curDatingNpc.npcId);
        if (npcMsg != null) rm.InitializedData(npcMsg.npcInfo.randomDatingMonologue);
    }

    /// <summary>
    /// 隐藏任务推荐面板
    /// </summary>
    private void HideMissonRecommend()
    {
        m_btnTaskRecommend.SafeSetActive(false);
    }

    /// <summary>
    /// 刷新任务推荐面板
    /// </summary>
    private void RefreshMissonRecommend()
    {
        var list = moduleNPCDating.allMissionsList;
        if (list == null) return;
        bool bShow = false;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].taskState != EnumTaskState.Finish)
            {
                bShow = true;
                Util.SetText(m_txtTaskRecommendName, list[i].taskNameID);
                AtlasHelper.SetNpcDateInfo(m_imgTaskRecommendIcon, list[i].taskIconID);
                break;
            }
        }
        m_btnTaskRecommend.SafeSetActive(bShow);
    }

#endregion

    #region mission list panel
    private void RefreshTaskPanel(List<Task> taskList)
    {
        if (taskList == null || taskList.Count <= 0) return;
        //为了减少性能开销，如果当前界面没有被销毁，再次打开列表仅刷新数据
        if (m_bOpenMissionList)
        {
            m_taskInfoDataSource.UpdateItems();
        }
        else
        {
            m_taskInfoDataSource.SetItems(taskList);
            m_bOpenMissionList = true;
        }
    }

    private void SetTaskItemData(Transform node, Task data)
    {
        Transform markIcon = node.Find("markIcon").GetComponent<Transform>();
        Image icon = node.Find("icon").GetComponent<Image>();
        AtlasHelper.SetNpcDateInfo(icon, data.taskIconID);
        bool bShowMark = data.taskState == EnumTaskState.Finish;
        markIcon.SafeSetActive(bShowMark);
        if (m_curClickTask == null) ClickTaskItem(node, data);
        if (m_curClickTask.taskID == data.taskID) AtlasHelper.SetNpcDateInfo(icon, data.taskPressIcon);
    }

    private void ClickTaskItem(Transform node, Task info)
    {
        m_curClickTask = info;

        Util.SetText(m_textTaskName, info.taskNameID);
        Util.SetText(m_textTaskDesc, info.taskDescID);
        m_taskInfoDataSource.UpdateItems();
    }

    private void ClearMissionListPanel()
    {
        m_textTaskName.text = "";
        m_textTaskDesc.text = "";
        m_textTaskTip.text = "";
        m_curClickTask = null;
    }

    #endregion

    #region mission tips

    /// <summary>
    /// 显示约会场景中完成或者接收到的任务提示
    /// </summary>
    private void ShowMissionTipPanel()
    {
        var finishedList = moduleNPCDating.dicSceneFinishedTask.ContainsKey(moduleNPCDating.lastDatingScene)? moduleNPCDating.dicSceneFinishedTask[moduleNPCDating.lastDatingScene]:null;
        var receivedList = moduleNPCDating.dicSceneAcceptTask.ContainsKey(moduleNPCDating.lastDatingScene) ? moduleNPCDating.dicSceneAcceptTask[moduleNPCDating.lastDatingScene] : null;

        m_tfMissionTipPanel.SafeSetActive(finishedList != null || receivedList != null);
        m_svMissionFinished.SafeSetActive(finishedList != null);
        m_svMissionReceive.SafeSetActive(receivedList != null);

        m_dsMissionFinished.Clear();
        m_dsMissionReceive.Clear();

        m_dsMissionFinished.SetItems(finishedList);
        m_dsMissionReceive.SetItems(receivedList);

        //标记接到任务的场景红点
        MarkScene(receivedList);
    }

    private void OnSetMissionReceiveData(Transform node, Task data)
    {
        Text t = node.Find("missionName").GetComponent<Text>();
        Image icon = node.Find("missionName/missionIcon").GetComponent<Image>();

        Util.SetText(t, data.taskNameID);

        AtlasHelper.SetNpcDateInfo(icon, data.taskIconID);
    }

    private void OnSetMissionFinishedData(Transform node, Task data)
    {
        Text t = node.Find("missionName").GetComponent<Text>();
        Image icon = node.Find("missionName/missionIcon").GetComponent<Image>();

        Util.SetText(t, data.taskNameID);

        AtlasHelper.SetNpcDateInfo(icon, data.taskIconID);
    }
#endregion

    #region second Scene Panel
    private void OpenSecondScenePanel(EnumNPCDatingSceneType t)
    {
        m_tfSecondScenePanel.SafeSetActive(true);

        var data = moduleNPCDating.GetDatingSceneData(t);
        if (data == null) return;
        m_curOpenSceneData = data;
        Util.SetText(m_textSceneName, data.nameID);
        Util.SetText(m_textSceneOpenTime, data.startTime + "-" + data.endTime);
        Util.SetText(m_textSceneDesc, data.sceneDescID);
        Util.SetText(m_textConsumePower, Util.Format(ConfigText.GetDefalutString(TextForMatType.NpcDating, 15), data.consumePower));
        UIDynamicImage.LoadImage(m_tfSceneImage, data.sceneIcon);
        UIDynamicImage.LoadImage(m_tfSceneBottomImage, data.strBottomMarkImage);
        AtlasHelper.SetNpcDateInfo(m_imgSceneTopImage, data.strTopMarkImage, null, true);

        int[] openEventNameIds = new int[] { };
        for (int i = 0; i < moduleNPCDating.openWindowData.sceneOpenEvent.Length; i++)
        {
            if (moduleNPCDating.openWindowData.sceneOpenEvent[i].datingScene == data.levelId)
            {
                openEventNameIds = moduleNPCDating.openWindowData.sceneOpenEvent[i].openEventIds;
                break;
            }
        }

        for (int i = 0; i < m_tfSceneOpenEvent.childCount; i++)
        {
            if (i <= openEventNameIds.Length - 1)
            {
                m_tfSceneOpenEvent.GetChild(i).SafeSetActive(true);
                Text eventText = m_tfSceneOpenEvent.GetChild(i).GetComponent<Text>();
                Util.SetText(eventText, openEventNameIds[i]);
            }
            else m_tfSceneOpenEvent.GetChild(i).SafeSetActive(false);
        }

    }

    private void OnClickCloseScenePanel()
    {
        m_tfSecondScenePanel.SafeSetActive(false);
        ClearSecondScenePanel();
    }

    private void OnClickEnterScene()
    {
        //发送进入场景消息
        moduleNPCDating.SendEnterScene(m_curOpenSceneData.levelId);
    }

    private void ClearSecondScenePanel()
    {

    }
    #endregion

    #endregion

    #region Other Logic
    private void AddRollNumber(Text t, int fromNum, int toNum, float time = 1.0f, Action callback = null)
    {
        if (fromNum == toNum || Util.Parse<int>(t.text) == toNum)
        {
            Util.SetText(t, toNum.ToString());
            callback?.Invoke();
        }
        else
        {
            Sequence mScoreSequence = DOTween.Sequence();
            mScoreSequence.SetAutoKill(false);
            mScoreSequence.Append(
                DOTween.To((value) =>
                {
                    var temp = Math.Floor(value).ToString();
                    Util.SetText(t, temp);
                }, fromNum, toNum, time)).AppendCallback(() => callback?.Invoke());
        }
    }

    private void AddRollNumber(Image img, float fromNum, float toNum, float time = 1.0f, Action callback = null)
    {
        if (fromNum == toNum || img.fillAmount == toNum)
        {
            img.fillAmount = toNum;
            callback?.Invoke();
        }
        else
        {
            Sequence mScoreSequence = DOTween.Sequence();
            mScoreSequence.SetAutoKill(false);
            mScoreSequence.Append(
                DOTween.To((value) =>
                {
                    img.fillAmount = value;
                }, fromNum, toNum, time)).AppendCallback(() => callback?.Invoke());
        }
    }

    /// <summary>
    /// 滑动条滚动
    /// </summary>
    private void AddRollNumber(RectTransform rt, float fromNum, float toNum, float time = 1.5f, Action callback = null)
    {
        float deltaY = rt.sizeDelta.y;
        if (fromNum == toNum || rt.sizeDelta.x == toNum)
        {
            rt.sizeDelta = new Vector2(toNum, deltaY);
            callback?.Invoke();
        }
        else
        {
            Sequence mScoreSequence = DOTween.Sequence();
            mScoreSequence.SetAutoKill(false);
            mScoreSequence.Append(
                DOTween.To((value) =>
                {
                    rt.sizeDelta = new Vector2(value, deltaY);
                }, fromNum, toNum, time)).AppendCallback(() => callback?.Invoke());
        }
    }
    #endregion

    #region Scroll Rect
    private void OnScrollRectValueChanged(Vector2 v2)
    {
        if (m_levelHome.datingCamera == null) return;

        var content = m_srMap.content;
        m_curContentPos = content.anchoredPosition.x;
        float s = m_curContentPos - m_lastContentPos;
        var cameraOffset = Math.Abs(m_datingCamLeftPos.x) + Math.Abs(m_datingCamRightPos.x * m_aspect);
        float offsetX = s * cameraOffset / (content.sizeDelta.x - UIManager.referenceResolution.x);
        m_v3CameraPos.x += offsetX;
        if (m_v3CameraPos.x > m_datingMaxCamLeftPos.x)
        {
            m_levelHome.datingCamera.transform.position = m_datingMaxCamLeftPos;
        }
        else if (m_v3CameraPos.x < m_datingMaxCamRightPos.x * m_aspect)
        {
            m_levelHome.datingCamera.transform.position = new Vector3(m_datingMaxCamRightPos.x * m_aspect, m_datingMaxCamRightPos.y, m_datingMaxCamRightPos.z);
        }
        else
        {
            m_levelHome.datingCamera.transform.position = m_v3CameraPos;
        }

        m_lastContentPos = m_curContentPos;

    }

    private void OnScrollRectDown(GameObject go)
    {
        if (m_bClickState) moduleNPCDating.DispatchEvent(Module_NPCDating.EventNotifyDatingMapObject, Event_.Pop(EnumDatingNotifyType.ClickBuildDown, Input.mousePosition));
    }

    private void OnScrollRectUp(GameObject go)
    {
        if (m_bClickState) moduleNPCDating.DispatchEvent(Module_NPCDating.EventNotifyDatingMapObject, Event_.Pop(EnumDatingNotifyType.ClickBuildUp, Input.mousePosition));
    }

    private void OnScrollRectBeginDrag(PointerEventData go)
    {
        m_bClickState = false;
    }

    private void OnScrollRectEndDrag(PointerEventData go)
    {
        m_bClickState = true;
        moduleNPCDating.DispatchEvent(Module_NPCDating.EventNotifyDatingMapObject, Event_.Pop(EnumDatingNotifyType.OnEndDrag, go.pointerCurrentRaycast.screenPosition));
    }

#endregion
}