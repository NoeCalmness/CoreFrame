// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-12-13      20:46
//  *LastModify：2018-12-14      14:50
//  ***************************************************************************************************/

#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class AssistWindow_Select : SubWindowBase<Window_Assist>
{
    private AssistMemberInfo _current;
    private ScrollView       _scrollView;
    private ToggleGroup      _toggleGroup;
    private Button           _startButton;

    protected DataSource<AssistMemberInfo> dataSource;

    #region Property
    private ChaseTask ChaseTask { get { return parentWindow.chaseTask; } }
    #endregion


    public AssistMemberInfo Current
    {
        get { return (_toggleGroup?.AnyTogglesOn() ?? false) ? _current : null; }
    }

    protected override void InitComponent()
    {
        base.InitComponent();
        _scrollView  = WindowCache.GetComponent<ScrollView> ("invite_panel/scrollView");
        _toggleGroup = WindowCache.GetComponent<ToggleGroup>("invite_panel/scrollView/viewport");
        _startButton = WindowCache.GetComponent<Button>     ("invite_panel/start_Btn");
    }

    public override void MultiLanguage()
    {
        base.MultiLanguage();
        var ct = ConfigManager.Get<ConfigText>((int) TextForMatType.AssistUI).text;
        Util.SetText(WindowCache.GetComponent<Text>("invite_panel/start_Btn/start_text"), ct[0]);
        Util.SetText(WindowCache.GetComponent<Text>("invite_panel/titleBg/title_Txt"), ct[5]);
        Util.SetText(WindowCache.GetComponent<Text>("checkbox/toggle_02/title"), ct[6]);
        Util.SetText(WindowCache.GetComponent<Text>("checkbox/toggle_01/title"), ct[7]);
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;

        RefreshAssistList();
        _toggleGroup?.onAnyToggleOn.AddListener(t =>
        {
            var dataInter = t.GetComponentInParent<IScrollViewData<ISourceItem>>();
            _current = dataInter?.GetItemData() as AssistMemberInfo;
        });

        _startButton?.onClick.AddListener(CheckStartChaseCondition);
        return true;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;

        _toggleGroup?.onAnyToggleOn.RemoveAllListeners();
        _startButton?.onClick.RemoveAllListeners();
        return true;
    }


    /// <summary>
    /// 检查当前开始关卡的条件(全部关卡有体力,困难关卡还有挑战次数限制)
    /// </summary>
    private void CheckStartChaseCondition()
    {
        var isEnough = modulePlayer.roleInfo.fatigue >= ChaseTask.taskConfigInfo.fatigueCount;
        if (!isEnough)
        {
            moduleGlobal.OpenExchangeTip(TipType.BuyEnergencyTip, true);
        }
        else if (ChaseTask.taskType == TaskType.Difficult)
        {
            if (ChaseTask.canEnterTimes == 0)
                parentWindow.RefreshPayPanel(ChaseTask);
            else moduleChase.SendStartChase(ChaseTask, parentWindow.CurrentAssistInfo);
        }
        else if (ChaseTask.taskType == TaskType.Active)
        {
            var info = moduleChase.allActiveItems.Find(p => p.taskLv == ChaseTask.taskConfigInfo.level);

            if (info != null && moduleChase.activeChallengeCount != null && moduleChase.activeChallengeCount.ContainsKey(ChaseTask.taskConfigInfo.level))
            {
                int crossNumber = info.crossLimit - moduleChase.activeChallengeCount[ChaseTask.taskConfigInfo.level];
                if (crossNumber < 1)
                {
                    moduleGlobal.ShowMessage(Util.GetString(401, 5));
                    return;
                }
            }
            moduleChase.SendStartChase(ChaseTask, parentWindow.CurrentAssistInfo);
        }
        else
            moduleChase.SendStartChase(ChaseTask, parentWindow.CurrentAssistInfo);
    }

    private void RefreshAssistList()
    {
        if (ChaseTask == null) return;
        _current = null;

        var list = new List<AssistMemberInfo>();
        if (moduleAssist.AssistList != null && moduleAssist.AssistList.Length > 0)
        {
            for (var i = 0; i < moduleAssist.AssistList.Length; i++)
            {
                var info = moduleAssist.AssistList[i];
                if (info.playerInfo == null && info.npcId == 0)
                    continue;

                //未通关关卡隐藏助战Npc
                if (info.type == 1)
                {
                    if( ChaseTask?.stageInfo.npcAssist == 0 ||
                       (ChaseTask?.stageInfo.npcAssist == 1 && ChaseTask?.taskData?.state == 1))
                        continue;
                }

                if (info.type == 1)
                {
                    var npcInfo = moduleNpc.GetTargetNpc((NpcTypeID)info.npcId);
                    if (null == npcInfo)
                    {
                        Logger.LogError($"未查到Npc相关信息。异常NpcID = {info.npcId}");
                        continue;
                    }
                }
                list.Add(new AssistMemberInfo(info));
            }
        }

        if (dataSource == null)
            dataSource = new DataSource<AssistMemberInfo>(list, _scrollView, OnSetData);
        else
            dataSource.SetItems(list);
    }

    private void OnSetData(RectTransform node, AssistMemberInfo data)
    {
        var t0 = node.transform.Find("player");
        var t1 = node.transform.Find("npc");
        if (data.Relation != CommonRelation.Npc)
        {
            t1.SafeSetActive(false);
            t0.SafeSetActive(true);
            var Presct = t0?.GetComponentDefault<FriendPrecast>();
            Presct?.DelayAddData(data, 1);
            Presct?.SetToggleGroup(_toggleGroup);
        }
        else
        {
            t0.SafeSetActive(false);
            t1.SafeSetActive(true);
            var Presct = t1?.GetComponentDefault<NpcPrecast>();
            Presct?.InitData(data);
            Presct?.SetToggleGroup(_toggleGroup);
        }
    }

    private void _ME(ModuleEvent<Module_Assist> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Assist.Notify_AssistList:
                ResponseAssistList();
                break;
        }
    }

    private void ResponseAssistList()
    {
        RefreshAssistList();
    }
}

public class AssistMemberInfo : SourceFriendInfo
{
    public readonly PAssistInfo AssistInfo;

    public AssistMemberInfo(PAssistInfo rInfo):base (rInfo.type == 1 ? null : rInfo.playerInfo)
    {
        AssistInfo       = rInfo;
        NpcInfo     = AssistInfo.type == 1 ? Module_Npc.instance.GetTargetNpc((NpcTypeID)rInfo.npcId): null;
        Relation    = (CommonRelation)rInfo.type;
    }

    public override INpcMessage NpcInfo { get; }

    public override int addPoint
    {
        get { return AssistInfo.addPoint; }
    }

    public static implicit operator AssistMemberInfo(PAssistInfo rInfo)
    {
        return new AssistMemberInfo(rInfo);
    }
}