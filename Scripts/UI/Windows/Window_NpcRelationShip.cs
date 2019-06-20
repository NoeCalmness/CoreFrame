// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-12-11      10:47
//  *LastModify：2018-12-11      10:47
//  ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_NpcRelationShip : Window
{
    private NpcAwake_Detail _detailWindow;
    private NpcAwake_Star   _starWindow;

    private Transform       _root;
    private Transform       _templete;

    private ToggleGroup     _group;

    private Dictionary<NpcTypeID, Toggle> _npcDict = new Dictionary<NpcTypeID, Toggle>();

    protected override void OnOpen()
    {
        base.OnOpen();
        InitComponent();
        MultiLangrage();

        AddListener();

        CreateNpc();
    }

    private void CreateNpc()
    {
        _npcDict.Clear();
        for (var i = NpcTypeID.None + 1; i < NpcTypeID.StoryNpc; i++)
        {
            var t = _root.AddNewChild(_templete);
            t.SafeSetActive(true);
            t.name = i.ToString();
            var npcInfo = ConfigManager.Get<NpcInfo>((int)i);
            AtlasHelper.SetAvatar(t.GetComponent<Transform>("head_icon"), npcInfo?.icon);
            var npc = moduleNpc.GetTargetNpc(i);
            t.GetComponent<Transform>("avatar_topbg_img").SafeSetActive(npc.maxFetterLv == npc.fetterLv);
            _npcDict.Add(i, t.GetComponent<Toggle>());
        }
    }

    private void AddListener()
    {
        moduleAwake.AddEventListener(Module_Awake.Event_NodeGetFocus    , OnNodeGetFocus);
        moduleAwake.AddEventListener(Module_Awake.Event_NodeLoseFocus   , OnNodeLoseFocus);
        moduleAwake.AddEventListener(Module_Awake.Event_CloseToNode     , OnCloseToNode);
        moduleAwake.AddEventListener(Module_Awake.Event_StartCloseToNode, OnStartCloseToNode);
        moduleAwake.AddEventListener(Module_Awake.Event_FarAwayNode     , OnFarAwayNode);
        moduleAwake.AddEventListener(Module_Awake.Event_StartFarAwayNode, OnStartFarAwayNode);

        _group.onAnyToggleStateOn.AddListener(onToggleOn);
    }

    private void onToggleOn(Toggle rToggle)
    {
        foreach (var kv in _npcDict)
        {
            if (kv.Value == rToggle)
                moduleNpc.DispatchEvent(Module_Npc.FocusNpcEvent, Event_.Pop(kv.Key));
        }
    }

    private void OnStartCloseToNode(Event_ e)
    {
        moduleGlobal.ShowGlobalLayerDefault(-1, false);

        _detailWindow.UnInitialize();

        _root.SafeSetActive(false);
    }

    private void OnStartFarAwayNode(Event_ e)
    {
        _starWindow.UnInitialize();
    }

    private void OnCloseToNode(Event_ e)
    {
        var npcId = (NpcTypeID)e.param1;
        var watcher = TimeWatcher.Watch("show star window");
        _starWindow.Initialize(npcId);
        watcher.See("Initialize");
        watcher.Stop();
        watcher.UnWatch();
    }

    private void OnFarAwayNode(Event_ e)
    {
        var npcId = (NpcTypeID)e.param1;

        var watcher = TimeWatcher.Watch("show detail window");
        _detailWindow.Initialize(npcId);
        watcher.See("Initialize");
        watcher.Stop();
        watcher.UnWatch();
        moduleGlobal.ShowGlobalLayerDefault(1, false);

        _root.SafeSetActive(true);
    }

    private void OnNodeGetFocus(Event_ e)
    {
        var npcId = (NpcTypeID)e.param1;

        var watcher = TimeWatcher.Watch("show detail window2");
        _detailWindow.Initialize(npcId);
        watcher.See("Initialize");
        watcher.Stop();
        watcher.UnWatch();

        if (_npcDict.ContainsKey(npcId))
            _npcDict[npcId].isOn = true;
    }

    private void OnNodeLoseFocus(Event_ e)
    {
        _detailWindow.UnInitialize();
    }

    private void InitComponent()
    {
        _detailWindow = SubWindowBase.CreateSubWindow<NpcAwake_Detail>(this, GetComponent<Transform>("npcInfo_Panel")?.gameObject);
        _starWindow   = SubWindowBase.CreateSubWindow<NpcAwake_Star>(this, GetComponent<Transform>("starPower_Panel")?.gameObject);

        _root       = GetComponent<Transform>("npc_panel");
        _templete   = GetComponent<Transform>("npc_panel/avatar");
        _group      = GetComponent<ToggleGroup>("npc_panel");
        _templete.SafeSetActive(false);
    }

    private void MultiLangrage()
    {

    }


    protected override void OnClose()
    {
        base.OnClose();
        _detailWindow?.Destroy();
    }

    protected override void OnHide(bool forward)
    {
        moduleHome.DispatchEvent(Module_Home.EventSwitchCameraMode, Event_.Pop(CameraShowType.Home));
        Module_Awake.instance.isInitedAccompany = false;
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault(1, false);
        moduleHome.DispatchEvent(Module_Home.EventSwitchCameraMode, Event_.Pop(CameraShowType.NpcAwake));
    }

}
