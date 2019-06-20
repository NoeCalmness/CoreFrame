/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-01-04
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum GaidenSubWindowNpc
{
    Panel = 0,
    Chapter,
    Story,
    Count,
}
public class Window_NpcGaiden : Window
{
    private ScrollView                  m_scroll;
    private DataSource<GaidenTaskInfo>  m_dataSource;
    public Action<GaidenTaskInfo>       itemClick;

    private NpcGaiden_Chapter           chapterWindow;
    private NpcGaiden_Story             storyWindow;
    private Transform                   m_mainPanel;


    protected override void OnOpen()
    {
        base.OnOpen();
        InitComponent();
        MultiLangrage();
    }

    protected void InitComponent()
    {
        m_scroll = GetComponent<ScrollView>("npc_panel/scrollView");
        m_mainPanel = GetComponent<Transform>("npc_panel");
        var list = new List<GaidenTaskInfo>(moduleNpcGaiden.gaidenInfoDic.Keys);
        list.Sort((a, b) => a.order.CompareTo(b.order));
        m_dataSource = new DataSource<GaidenTaskInfo>(list, m_scroll, OnRefreshItem,
            OnItemClick);

        chapterWindow = SubWindowBase.CreateSubWindow<NpcGaiden_Chapter, Window_NpcGaiden>(this, GetComponent<Transform>("npc_chapterpanel")?.gameObject);
        storyWindow   = SubWindowBase.CreateSubWindow<NpcGaiden_Story, Window_NpcGaiden>  (this, GetComponent<Transform>("npc_storypanel").gameObject);
    }

    private void MultiLangrage()
    {
        Util.SetText(GetComponent<Text>("npc_panel/bottom_img/Text"),
            ConfigText.GetDefalutString((int) TextForMatType.NpcGaidenUI, 1));
    }

    public void EnableMainPanel(bool rEnable)
    {
        m_mainPanel.SafeSetActive(rEnable);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault(1, false);
        m_dataSource.UpdateItems();
        var e = GetWindowParam<Window_NpcGaiden>();
        if (e?.param1 != null)
            _currentType = (TaskType)e.param1;
        if (e?.param2 != null)
            _currentTask = (ChaseTask)e.param2;

        var enableMain = true;
        if (_chapterIsInit || m_subTypeLock == (int)GaidenSubWindowNpc.Chapter)
        {
            chapterWindow.Initialize(_currentType);
            enableMain = false;
        }
        else
            chapterWindow.UnInitialize();

        if (_detailIsInit || m_subTypeLock == (int)GaidenSubWindowNpc.Story)
        {
            storyWindow.Initialize(_currentTask);
            enableMain = false;
        }
        else
            storyWindow.UnInitialize();
        EnableMainPanel(enableMain);

        if (e?.param3 != null && (bool) e.param3)
            m_subTypeLock = -1;
    }

    public void GotoSubWindow(GaidenSubWindowNpc gSubWindow, TaskType rType)
    {
        if (gSubWindow == GaidenSubWindowNpc.Panel)
            return;
        else if (gSubWindow == GaidenSubWindowNpc.Chapter)
        {
            _currentType = rType;
            chapterWindow.Initialize(rType);
        }
    }

    protected override void OnReturn()
    {
        if (storyWindow.OnReturn())
        {
            chapterWindow.Initialize(_currentType);
            return;
        }
        if (chapterWindow.OnReturn())
        {
            EnableMainPanel(true);
            return;
        }
        base.OnReturn();
    }

    protected override void OnHide(bool forward)
    {
        base.OnHide(forward);
        _chapterIsInit = false;
        _detailIsInit = false;
        _enterFight = false;
    }

    private void OnRefreshItem(RectTransform rt, GaidenTaskInfo info)
    {
        PropItemInfo pinfo = new PropItemInfo();
        pinfo.ID = (int)info.taskType;
        BaseRestrain.SetRestrainData(rt.gameObject, pinfo, 0, 0);
        //动态更换图片
        Transform t = rt.Find("bg");
        UIDynamicImage.LoadImage(t, info.bgIcon, null, true);

        t = rt.Find("bg/npc");
        UIDynamicImage.LoadImage(t, info.icon, null, true);

        if (info.lampstand.Length > 0)
        {
            t = rt.Find("bottom_circle");
            UIDynamicImage.LoadImage(t, info.lampstand[0], null, true);
        }

        if (info.lampstand.Length > 1)
        {
            t = rt.Find("bottom_circle/bottom_bg");
            UIDynamicImage.LoadImage(t, info.lampstand[1], null, true);
        }

        var text = rt.GetComponent<Text>("bg/chapter/Text");
        Util.SetText(text, info.gaidenNameId);
        //应用字体颜色
        if(text && info.nameColor.Length > 0)
            text.color = info.nameColor[0];
        //应用描边颜色
        if (info.nameColor.Length > 1)
        {
            var outline = text.GetComponent<Outline>();
            if (outline) outline.effectColor = info.nameColor[1];
            var descBg = rt.GetComponent<Image>("bg/chapter/bg");
            if (descBg) descBg.color = info.nameColor[1];
        }

        text = rt.GetComponent<Text>("bg/white_bg/Text");
        Util.SetText(text, info.descId);
        //应用描述字体颜色
        if (text)
            text.color = info.descColor;
        //设置加锁状态
        var lockState = rt.Find("off");
        lockState.SafeSetActive(modulePlayer.level < info.openLv || info.openLv <= 0);
        text = lockState.GetComponent<Text>("off_bg/Text");
        Util.SetText(text, Util.Format(ConfigText.GetDefalutString(TextForMatType.NpcGaidenUI, info.openLv > 0 ? 2 : 0), info.openLv));
    }

    private void OnItemClick(RectTransform rt, GaidenTaskInfo info)
    {
        if (info.openLv > modulePlayer.level || info.openLv <= 0)
        {
            moduleGlobal.ShowMessage(Util.GetString((int) TextForMatType.NpcGaidenUI, info.openLv > 0 ? 2 : 0, info.openLv));
            return;
        }
        _currentType = info.taskType;
        chapterWindow.Initialize(info.taskType);
    }

    public void OpenStoryWindow(ChaseTask rTask)
    {
        if (storyWindow.Initialize(rTask))
        {
            chapterWindow.UnInitialize();
        }
    }

    private void _ME(ModuleEvent<Module_Player> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Player.EventLevelChanged:
                m_dataSource.UpdateItems();
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Chase> e)
    {
        if (e.moduleEvent == Module_Chase.EventSuccessToPVE)
        {
            _enterFight = true;
        }
    }

    #region 窗口数据恢复

    private bool _chapterIsInit;
    private bool _detailIsInit;
    private TaskType _currentType;
    private ChaseTask _currentTask;
    private bool _enterFight;
    protected override void GrabRestoreData(WindowHolder holder)
    {
        if (_enterFight && storyWindow.CurrentTask?.taskConfigInfo.maxChanllengeTimes == 1)
        {
            holder.SetData(true, false, _currentType, storyWindow.CurrentTask);

            if (m_subTypeLock == (int) GaidenSubWindowNpc.Story)
                m_subTypeLock = (int) GaidenSubWindowNpc.Chapter;
        }
        else
            holder.SetData(chapterWindow.isInit, storyWindow.isInit, _currentType, storyWindow.CurrentTask);
    }

    protected override void ExecuteRestoreData(WindowHolder holder)
    {
        _chapterIsInit = holder.GetData<bool>(0);
        _detailIsInit = holder.GetData<bool>(1);
        _currentType = holder.GetData<TaskType>(2);
        _currentTask = holder.GetData<ChaseTask>(3);

    }
    #endregion
}
