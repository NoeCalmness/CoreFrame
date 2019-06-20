// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-11-13      16:15
//  *LastModify：2018-11-13      16:15
//  ***************************************************************************************************/
#if AUTOMATIC
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Automatic : Singleton<Automatic>
{
    private Dictionary<Type,Action<Window>> actions = new Dictionary<Type, Action<Window>>();
    private bool isStart;
    private bool isInited;
    public ulong targetRoleId;

    private Coroutine inviteCoroutine;

    public void Init()
    {
        if (isInited)
            return;
        isInited = true;

        actions.Add(typeof (Window_Home), OnWindowHomeVisible);
        actions.Add(typeof (Window_PVP), OnWindowPVPVisible);
        actions.Add(typeof (Window_Settlement), OnWindowSettlementVisible);

        EventManager.AddEventListener(Events.UI_WINDOW_VISIBLE, onWindowVisible);
        EventManager.AddEventListener(LevelEvents.START_FIGHT, OnFightStart);
        EventManager.AddEventListener(ModuleEvent.GLOBAL, OnModuleEvent);
    }

    private void OnModuleEvent(Event_ e)
    {
        var me = e as ModuleEvent;
        if (me?.moduleEvent == Module_Global.EventBeinvited)
            BeInvited(e);
        else if (me?.moduleEvent == Module_Match.EventInvationFailed)
            InvationFailed(e);
        else if (me?.moduleEvent == Module_Match.EventInvationsucced)
            Invationsucced(e);
    }

    private void OnFightStart()
    {
        if (FightRecordManager.IsRecovering || !Module_PVP.instance.connected)
            return;

        var players = Module_Match.instance.players;
        targetRoleId = players[0].roleId == Module_Player.instance.id_ ? players[1].roleId : 0;
        Start();
        Module_PVP.instance.opType = OpenWhichPvP.FreePvP;

        var lp = Level.current as Level_PVP;
        if (lp == null) return;
        foreach (var c in lp.players)
        {
            Module_AI.instance.AddPlayerAI(c);
            Module_AI.instance.AddCreatureToCampDic(c);
        }
    }

    private void Invationsucced(Event_ e)
    {
        if (inviteCoroutine != null)
            Root.instance.StopCoroutine(inviteCoroutine);
        inviteCoroutine = null;
    }

    private void InvationFailed(Event_ e)
    {
        if (!isStart)
            return;

        if (targetRoleId <= 0)
            return;

        DelayEvents.Add(() =>
        {
            var wg = Window.GetOpenedWindow<Window_Global>();
            wg?.overtime(2);
        }, 1);
    }

    private void BeInvited(Event_ e)
    {
        if (!isStart)
            return;

        if (targetRoleId > 0)
            return;
        DelayEvents.Add(() =>
        {
            var wg = Window.GetOpenedWindow<Window_Global>();
            wg?.overtime(0);
        }, 1);
    }

    private void OnWindowPVPVisible(Window rWindow)
    {
        if (null != inviteCoroutine)
            Root.instance.StopCoroutine(inviteCoroutine);
        if (targetRoleId <= 0)
            return;


        inviteCoroutine = Root.instance.StartCoroutine(Invite_Async());
    }

    private IEnumerator Invite_Async()
    {
        for(var i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(3);

            if (null == inviteCoroutine)
                yield break;

            var list = new List<ulong> { targetRoleId };
            Module_Match.instance.FriendFree(list);
        }
    }

    private void OnWindowSettlementVisible(Window rWindow)
    {
        DelayEvents.Add(() =>
        {
            Module_PVP.instance.isInvation = false;
            Module_PVP.instance.isMatchAgain = false;
            Module_Match.instance.isbaning = true;
            Game.GoHome();
        }, 2);
    }

    private void OnWindowHomeVisible(Window rWindow)
    {
        if (targetRoleId <= 0)
            return;
        DelayEvents.Add(() =>
        {
            Window.ShowAsync<Window_PVP>();
        }, 10);
    }

    private void onWindowVisible(Event_ e)
    {
        if (!isStart)
            return;
        var w = e.param1 as Window;
        if (w && actions.ContainsKey(w.GetType()))
            actions[w.GetType()].Invoke(w);
    }

    public void Start()
    {
        isStart = true;
    }

    public void Stop()
    {
        isStart = false;
    }
}
#endif