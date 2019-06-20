// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-12-13      19:36
//  *LastModify：2018-12-15      18:04
//  ***************************************************************************************************/
#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class Window_Assist : Window
{
    public enum SubWindowType
    {
        SelectWindow,
        SweepWindow
    }

    #region Property

    public PAssistInfo CurrentAssistInfo { get { return _windowGroup?.GetWindow<AssistWindow_Select>()?.Current?.AssistInfo; } }

    #endregion

    protected override void OnOpen()
    {
        base.OnOpen();

        ignoreStack = true;

        InitComponents();
        MultiLangrage();

        _windowGroup.SwitchWindow(SubWindowType.SelectWindow);

        _resetChallengeTimes?.onClick.AddListener(OnResetChallengeTimes);

        _chatButton?.onClick.AddListener(() =>
        {
            Window.ShowAsync<Window_Chat>();
        });
    }

    private void OnResetChallengeTimes()
    {
        _buyTimesWindow.Initialize(chaseTask);
    }

    private void InitComponents()
    {
        _chaseStar          = GetComponentDefault<ChaseStarPanel>("level_panel/infoScrollView/viewport/content/starReward_Panel");
        _taskIcon           = GetComponent<Image>                ("level_panel/awakeMissionImage_scrollView/template/0/map_Img");
        _stageName          = GetComponent<Text>                 ("level_panel/awakeMissionName_Txt");
        _energyCost         = GetComponent<Text>                 ("invite_panel/start_Btn/Text");
        _resetChallengeTimes= GetComponent<Button>               ("level_panel/remainchallengeCount/resetBtn");
        _challengeTimes     = GetComponent<Text>                 ("level_panel/remainchallengeCount/remainCount_Txt");
        _stageDesc          = GetComponent<Text>                 ("level_panel/infoScrollView/viewport/content/stageInfo_Panel/infoBg/content");
        _recommend          = GetComponent<Text>                 ("level_panel/recommend/Text");
        _layout             = GetComponent<VerticalLayoutGroup>  ("level_panel/infoScrollView/viewport/content");
        _chatButton         = GetComponent<Button>               ("level_panel/chat");
        _chatNotice         = GetComponent<Transform>            ("level_panel/chat/newinfo");

        GetComponent<Toggle>("level_panel/infoScrollView/viewport/content/stageInfo_Panel/packUp") ?.onValueChanged.AddListener(CalcContentSize);
        GetComponent<Toggle>("level_panel/infoScrollView/viewport/content/starReward_Panel/packUp")?.onValueChanged.AddListener(CalcContentSize);
        GetComponent<Toggle>("level_panel/infoScrollView/viewport/content/preaward_Panel/packUp")  ?.onValueChanged.AddListener(CalcContentSize);

        _stars[0]            = GetComponent<Transform>("level_panel/awakeMissionImage_scrollView/starframe/star_01");
        _stars[1]            = GetComponent<Transform>("level_panel/awakeMissionImage_scrollView/starframe/star_02");
        _stars[2]            = GetComponent<Transform>("level_panel/awakeMissionImage_scrollView/starframe/star_03");

        _rewardScrollView = GetComponent<ScrollView>("level_panel/infoScrollView/viewport/content/preaward_Panel/itemList");

        _buyTimesWindow     = SubWindowBase.CreateSubWindow<AssistWindow_BuyTimes, Window_Assist>(this, GetComponent<Transform>("level_panel/payChallengeCount")?.gameObject);

        _sweepToggle = GetComponent<Toggle>("checkbox/toggle_02");

        _windowGroup = new WindowGroup(GetComponent<ToggleGroup>("bg"));
        _windowGroup.Registe(SubWindowType.SelectWindow, GetComponent<Toggle>("checkbox/toggle_01"),
            SubWindowBase.CreateSubWindow<AssistWindow_Select, Window_Assist>(this, GetComponent<Transform>("invite_panel")?.gameObject));
        _windowGroup.Registe(SubWindowType.SweepWindow, _sweepToggle, 
            SubWindowBase.CreateSubWindow<AssistWindow_MoppingUp, Window_Assist>(this, GetComponent<Transform>("sweep_panel")?.gameObject));
    }

    private void CalcContentSize(bool arg0)
    {
        //重新计算content大小。没找到API，就用最笨的方法了
        _layout.enabled = false;
        _layout.enabled = true;
    }

    private void MultiLangrage()
    {
        var ct = ConfigManager.Get<ConfigText>((int)TextForMatType.AssistUI).text;
        Util.SetText(GetComponent<Text>("level_panel/infoScrollView/viewport/content/stageInfo_Panel/preaward_Txt"),ct[1]);
        Util.SetText(GetComponent<Text>("level_panel/infoScrollView/viewport/content/starReward_Panel/starReward_Txt"),ct[2]);
        Util.SetText(GetComponent<Text>("level_panel/infoScrollView/viewport/content/starReward_Panel/starReward02_Txt"),ct[3]);
        Util.SetText(GetComponent<Text>("level_panel/infoScrollView/viewport/content/preaward_Panel/preaward_Txt"),ct[4]);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleAssist.RequestAssistList();
        
        moduleGlobal.ShowGlobalLayerDefault();

        var args = GetWindowParam(name);

        if (args != null)
        {
            chaseTask = args.param1 as ChaseTask;
            RefreshTaskInfo();
            _windowGroup.SwitchWindow(SubWindowType.SelectWindow);

            _sweepToggle.SafeSetActive(chaseTask?.CanSweep() ?? false);
        }
        _chatNotice?.SafeSetActive(false);
    }
//    private void _ME(ModuleEvent<Module_Chat> e)
//    {
//        switch (e.moduleEvent)
//        {
//            case Module_Chat.EventRecFriendMes:
//            case Module_Chat.EventRecUnionMes:
//                _chatNotice.SafeSetActive(true);
//                break;
//            case Module_Chat.EventChatWindowHide:
//                _chatNotice.SafeSetActive(false);
//                break;
//        }
//    }

    protected override void OnClose()
    {
        base.OnClose();
        _buyTimesWindow ?.Destroy();
        _windowGroup    ?.Dispose();
    }

    protected override void OnHide(bool forward)
    {
        _buyTimesWindow.UnInitialize();
        _windowGroup.GetWindow<AssistWindow_MoppingUp>()?.ClearRecord();
    }


    private void RefreshTaskInfo()
    {
        if (null == chaseTask)
            return;
        RefreshChallengeTimes();

        _chaseStar?.RefreshPanel(chaseTask);
        var task = chaseTask.taskConfigInfo;
        if (task != null)
        {
            Util.SetText(_stageName, task.name);
            Util.SetText(_stageDesc, task.desc);
            Util.SetText(_energyCost, $"(     ×{task.fatigueCount})");
            Util.SetText(_recommend, Util.Format(ConfigText.GetDefalutString(TextForMatType.AssistUI, 9), task.recommend));
            _recommend.color = ColorGroup.GetColor(ColorManagerType.Recommend, modulePlayer.roleInfo.fight >= task.recommend);
            _recommend?.transform.parent.SafeSetActive(task.recommend > 0);
            var stageInfo = ConfigManager.Get<StageInfo>(task.stageId);
            var str = stageInfo.icon.Split(';');
            if (str.Length >= 1)
                UIDynamicImage.LoadImage(_taskIcon.transform, str[0]);
            List<int> reward = new List<int>(stageInfo.previewRewardItemId);
            reward.RemoveAll(item =>
            {
                var prop = ConfigManager.Get<PropItemInfo>(item);
                return !(prop && prop.proto != null && (prop.proto.Contains(CreatureVocationType.All) || prop.proto.Contains((CreatureVocationType)modulePlayer.proto)));
            });
            new DataSource<int>(reward, _rewardScrollView,  OnSetItem, OnItemClick);
        }

        for (var i = 0; i < _stars.Length; i++)
        {
            _stars[i].SafeSetActive(chaseTask.star >= i + 1);
        }
    }

    private void OnSetItem(RectTransform node, int data)
    {
        Util.SetItemInfoSimple(node, ConfigManager.Get<PropItemInfo>(data));
    }

    private void OnItemClick(RectTransform node, int data)
    {
        moduleGlobal.UpdateGlobalTip((ushort)data, true);
    }

    public void RefreshPayPanel(ChaseTask rTask)
    {
        _buyTimesWindow.Initialize(rTask);
    }

    public void RefreshChallengeTimes()
    {
        if (chaseTask == null) return;
        if (chaseTask.taskConfigInfo.challengeCount > 0)
        {
            _challengeTimes?.transform?.parent?.SafeSetActive(true);
            Util.SetText(_challengeTimes, $"{chaseTask.canEnterTimes}/{chaseTask.EnterTimeMax}");
        }
        else
            _challengeTimes?.transform?.parent?.SafeSetActive(false);

        _resetChallengeTimes.SafeSetActive(chaseTask.CanResetTimes);
    }

    private void _ME(ModuleEvent<Module_Chase> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Chase.EventResetTimesSuccess:
                RefreshChallengeTimes();
                break;
        }
    }

    #region Fields

    public ChaseTask chaseTask;


    private ChaseStarPanel              _chaseStar;
    private Image                       _taskIcon;
    private AssistWindow_BuyTimes       _buyTimesWindow;
    private Text                        _stageName;
    private Text                        _energyCost;
    private Text                        _challengeTimes;
    private Text                        _stageDesc;
    private Text                        _recommend;
    private Button                      _resetChallengeTimes;
    private readonly Transform[]        _stars = new Transform[3];
    private WindowGroup                 _windowGroup;
    private ScrollView                  _rewardScrollView;
    private VerticalLayoutGroup         _layout;
    private Button                      _chatButton;
    private Transform                   _chatNotice;
    private Toggle                      _sweepToggle;

    #endregion
}

