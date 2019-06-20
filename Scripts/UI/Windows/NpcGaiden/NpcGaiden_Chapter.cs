// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-02-25      17:30
//  *LastModify：2019-02-25      17:30
//  ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NpcGaiden_Chapter : SubWindowBase<Window_NpcGaiden>
{
    private Transform unitTemplete;
    private readonly List<Transform> chapters = new List<Transform>();
    private TaskType _currentType;
    private TweenEntry[] tweens;
    private Button arrowButton;
    private bool fold = false;

    public TaskType CurrentType { get { return _currentType; } }

    protected override void InitComponent()
    {
        base.InitComponent();

        unitTemplete = WindowCache.GetComponent<Transform>("npc_chapterpanel/frame");
        unitTemplete.SafeSetActive(false);
        var t = WindowCache.GetComponent<Transform>("npc_chapterpanel");
        chapters.Clear();
        for (var i = 0; i < t.childCount; i++)
        {
            var c = t.GetChild(i);
            if (!c.name.Contains("chapter")) continue;
            chapters.Add(c);
        }

        arrowButton = WindowCache.GetComponentDefault<Button>("npc_chapterpanel/mainTittles/titlearrow");

        var tweenRoot = WindowCache.GetComponent<Transform>("npc_chapterpanel/mainTittles/tween");
        tweens = new TweenEntry[tweenRoot?.childCount ?? 0];
        for (var i = 0; i < tweenRoot?.childCount; i++)
        {
            var tw = tweenRoot.GetChild(i);
            var type = TaskType.GaidenChapterOne + i;
            var info = moduleNpcGaiden.GetGaidenInfo(type);
            if (info != null)
                Util.SetText(tw.GetComponent<Text>("txt"), info.gaidenNameId);
            else
            {
                tw.SafeSetActive(false);
                continue;
            }
            if (null == tw) continue;
            tweens[i] = new TweenEntry();
            tweens[i].position = tw.GetComponent<TweenPosition>();
            tweens[i].alpha    = tw.GetComponent<TweenAlpha>();
            tweens[i].taskType = type;
            tweens[i].RegiestClick(tw.GetComponent<Button>(), OnToggleClick);
        }
    }

    private void OnToggleClick(TaskType rType)
    {
        if (fold)
        {
            var info = moduleNpcGaiden.GetGaidenInfo(rType);
            if (info == null)
            {
                Logger.LogError($"没有配置Level = {(int) rType} 的NpcGaidenInfo");
                return;
            }
            if (info.openLv > modulePlayer.level || info.openLv <= 0)
            {
                moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.NpcGaidenUI, info.openLv > 0 ? 2 : 0, info.openLv));
                return;
            }
            SwitchType(rType);
        }
        else
            PlayTweens(rType, true);
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;
        parentWindow.EnableMainPanel(false);
        arrowButton?.onClick.AddListener(() => PlayTweens(_currentType, true));
        return true;
    }

    protected override void RefreshData(params object[] p)
    {
        if (p.Length < 1 || p[0] == null)
            return;
        _currentType = (TaskType)p[0];
        SwitchType(_currentType);
        PlayTweens(_currentType, false);
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;

        arrowButton?.onClick.RemoveAllListeners();
        return true;
    }

    public void PlayTweens(TaskType rType, bool forward)
    {
        fold = forward;
        for (var i = 0; i < tweens?.Length; i++)
        {
            if (tweens[i] == null) continue;
            if (forward)
            {
                tweens[i].position?.PlayForward();
                tweens[i].alpha?.PlayForward();
            }
            else
            {
                tweens[i].position?.PlayReverse();
                if(rType != tweens[i].taskType)
                    tweens[i].alpha?.PlayReverse();
            }
        }
    }

    private void SwitchType(TaskType rType)
    {
        PlayTweens(rType, false);
        var index = rType - TaskType.GaidenChapterOne;
        for (var i = 0; i < chapters.Count; i++)
        {
            if (i != index)
                chapters[i].SafeSetActive(false);
            else
            {
                HandleTasks(chapters[i], rType);
                chapters[i].SafeSetActive(true);
            }
        }
    }

    private void HandleTasks(Transform rRoot, TaskType rType)
    {
        if (null == rRoot) return;
        var positionRoot = rRoot.Find("position");

        var tasks = moduleNpcGaiden.GetGaidenTasks(rType);
        for (var i = 0; i < tasks?.Count; i++)
        {
            if (i >= positionRoot?.childCount) break;

            NpcChapterItem item = null;
            var p = positionRoot?.GetChild(i);
            if (p == null)
                continue;

            if (p.childCount > 0)
            {
                item = p.GetChild(0)?.GetComponentDefault<NpcChapterItem>();
            }
            else
            {
                var t = p.AddNewChild(unitTemplete);
                item = t.GetComponentDefault<NpcChapterItem>();
                t.SafeSetActive(true);
            }
            item?.BindData(tasks[i], (chase) => { parentWindow.OpenStoryWindow(chase); });
        }
    }


    private void _ME(ModuleEvent<Module_NpcGaiden> e)
    {
        switch (e.moduleEvent)
        {
            case Module_NpcGaiden.EventRefreshGaidenTask:
                SwitchType(_currentType);
                break;
            case Module_NpcGaiden.EventRefreshTargetTask:
                
                break;
        }
    }

    private class TweenEntry
    {
        public TaskType      taskType;
        public TweenPosition position;
        public TweenAlpha    alpha;

        private Action<TaskType> _onClick;

        public void RegiestClick(Button rButton, Action<TaskType> onClick)
        {
            if (null == rButton)
                return;
            _onClick = onClick;
            rButton.onClick.RemoveAllListeners();
            rButton.onClick.AddListener(() => _onClick?.Invoke(taskType));
        }
    }
}
