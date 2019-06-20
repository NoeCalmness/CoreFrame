// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-02-25      17:33
//  *LastModify：2019-02-25      17:33
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class NpcGaiden_Story : SubWindowBase<Window_NpcGaiden>
{
    private NpcMono npcMono;
    private Text    remainCountText;
    private Text    nameText;
    private Text    descText;
    private Text    fatiguiText;
    private Text    npcPointText;
    private Transform starParent;
    private Transform resetRoot;

    private Button  resetTimeButton;
    private Button  startButton;
    private ScrollView scrollView;

    private ChaseTask _currentTask;
    private DataSource<int> _dataSource;

    public ChaseTask CurrentTask { get { return _currentTask; } }


    protected override void InitComponent()
    {
        base.InitComponent();
        remainCountText = WindowCache.GetComponent<Text>("npc_storypanel/bottom/remainchallengeCount/remainCount_Txt");
        nameText        = WindowCache.GetComponent<Text>("npc_storypanel/story_panel/title/Text");
        descText        = WindowCache.GetComponent<Text>("npc_storypanel/story_panel/Text");
        fatiguiText     = WindowCache.GetComponent<Text>("npc_storypanel/bottom/start_Btn/Text");
        npcPointText    = WindowCache.GetComponent<Text>("npc_storypanel/story_panel/value/number_text");
        npcMono         = WindowCache.GetComponent<NpcMono>("npc_storypanel/npcInfo");
        resetTimeButton = WindowCache.GetComponent<Button>("npc_storypanel/bottom/remainchallengeCount/resetBtn");
        startButton     = WindowCache.GetComponent<Button>("npc_storypanel/bottom/start_Btn");
        starParent      = WindowCache.GetComponent<Transform>("npc_storypanel/story_panel/title/star_parent");
        scrollView      = WindowCache.GetComponent<ScrollView>("npc_storypanel/story_panel/reward_partent/props");
        resetRoot       = WindowCache.GetComponent<Transform>("npc_storypanel/bottom/remainchallengeCount");
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;

        parentWindow.EnableMainPanel(false);
        resetTimeButton ?.onClick.AddListener(OnResetTimeClick);
        startButton     ?.onClick.AddListener(OnStartClick);
        return true;
    }

    protected override void RefreshData(params object[] p)
    {
        if (p.Length <= 0) return;
        _currentTask = p[0] as ChaseTask;
        RefreshData();
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;


        resetTimeButton ?.onClick.RemoveAllListeners();
        startButton     ?.onClick.RemoveAllListeners();
        return true;
    }
    private void RefreshData()
    {
        npcMono?.SwitchNpc((NpcTypeID)(moduleNpcGaiden.GetNpcFromTask(_currentTask)?.npcId ?? 0));
        Util.SetText(nameText, _currentTask.taskConfigInfo.name);
        Util.SetText(descText, _currentTask.taskConfigInfo.desc);
        Util.SetText(fatiguiText, $"(     ×{_currentTask.taskConfigInfo.fatigueCount})");
        Util.SetText(npcPointText, $"+{_currentTask.stageInfo.addNpcExpr}");

        var stageInfo = ConfigManager.Get<StageInfo>(_currentTask.stageId);
        _dataSource = new DataSource<int>(stageInfo.previewRewardItemId, scrollView, OnSetData, OnClick);

        resetRoot.SafeSetActive(_currentTask.taskConfigInfo.maxChanllengeTimes != 1);
        starParent.SafeSetActive(_currentTask.taskConfigInfo.maxChanllengeTimes != 1);

        if (starParent?.gameObject.activeSelf ?? false)
        {
            for (var i = 0; i < starParent?.childCount; i++)
                starParent.GetChild(i).SafeSetActive(_currentTask.star > i);
        }

        if (resetRoot?.gameObject.activeSelf ?? false)
        {
            RefreshTimes();
        }
    }

    private void RefreshTimes()
    {
        resetTimeButton.SafeSetActive(_currentTask.canEnterTimes <= 0);
        Util.SetText(remainCountText, $"{_currentTask.canEnterTimes}/{_currentTask.taskConfigInfo.challengeCount}");
    }

    private void OnSetData(RectTransform node, int rItemTypeID)
    {
        var prop = ConfigManager.Get<PropItemInfo>(rItemTypeID);
        if (null == prop)
            return;
        Util.SetItemInfoSimple(node, prop);
    }

    private void OnClick(RectTransform node, int rItemTypeID)
    {
        moduleGlobal.UpdateGlobalTip((ushort)rItemTypeID);
    }

    private void OnStartClick()
    {
        if (null == _currentTask) return;

        if (modulePlayer.roleInfo.fatigue < _currentTask.taskConfigInfo.fatigueCount)
        {
            moduleGlobal.OpenExchangeTip(TipType.BuyEnergencyTip);
            return;
        }

        if (_currentTask.canEnterTimes == 0 && _currentTask.CanResetTimes)
        {
            OnResetTimeClick();
            return;
        }

        moduleChase.SendStartChase(_currentTask);
    }

    private void OnResetTimeClick()
    {
        if (null == _currentTask) return;
        Window.SetWindowParam<Window_ResetTimes>(_currentTask);
        Window.ShowAsync<Window_ResetTimes>();
    }

    private void _ME(ModuleEvent<Module_Chase> e)
    {
        if (e.moduleEvent == Module_Chase.EventResetTimesSuccess)
            RefreshTimes();
    }
}
