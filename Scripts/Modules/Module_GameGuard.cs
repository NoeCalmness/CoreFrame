// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 防外挂系统
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-04-28      9:55
//  *LastModify：2019-04-28      9:55
//  ***************************************************************************************************/

using System;
using System.Security.Cryptography;
using UnityEngine;

public class Module_GameGuard : Module<Module_GameGuard>
{
    public const int EncryptSeed = 197692472;
    public const ulong EncryptSeedUlong = 197692472;
    public const int WarningThreshold = 4;

    private static MD5 s_md5Generater;
    /// <summary>
    /// 是否开启内存加密
    /// </summary>
    public static bool enableMemoryEncrypt;
    /// <summary>
    /// 是否开启MD5校验
    /// </summary>
    public static bool enableMd5Check;

    public static string GetMD5Code(byte[] rArr)
    {
        if (s_md5Generater == null) s_md5Generater = MD5.Create();
        return Convert.ToBase64String(s_md5Generater.ComputeHash(rArr));
    }

    protected bool m_startCheckPing;
    protected bool m_initStamp;
    protected bool m_focus;
    protected int m_lastLocalStamp;
    protected int m_lastServerStamp;
    protected int m_times;

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        enableMemoryEncrypt = true;
        enableMd5Check = true;
        m_focus = true;
        EventManager.AddEventListener(Events.SCENE_LOAD_START, OnSceneLoadStart);
        EventManager.AddEventListener(Events.SCENE_DESTROY, OnBattleEnd);
        EventManager.AddEventListener(Events.EVENT_PING_UPDATE, PingUpdate);
        EventManager.AddEventListener(Events.APPLICATION_FOCUS_CHANGED, OnFocusStateChange);
    }

    private void OnFocusStateChange(Event_ e)
    {
        m_focus = (bool)e.param1;
        if (!m_focus)
            m_initStamp = false;
    }

    private void OnSceneLoadStart(Event_ e)
    {
        var level = e.sender as Level_Battle;
        if (!level)
            return;
        level.AddEventListener(LevelEvents.START_FIGHT, OnStartFight);
        level.AddEventListener(LevelEvents.BATTLE_END, OnBattleEnd);
//        Logger.LogError("添加监听");
    }

    private void OnBattleEnd(Event_ e)
    {
        var level = e.sender as Level_Battle;
        if (!level || !m_startCheckPing)
            return;
        EndCheckTime();
        EventManager.RemoveEventListener(LevelEvents.START_FIGHT, OnStartFight);
        EventManager.RemoveEventListener(LevelEvents.BATTLE_END, OnBattleEnd);
//        Logger.LogError("移除监听");
    }

    private void OnStartFight()
    {
        StartCheckTime();
    }

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        enableMemoryEncrypt = false;
        enableMd5Check = false;
        m_startCheckPing = false;
    }

    public void StartCheckTime()
    {
        m_startCheckPing = !FightRecordManager.IsRecovering;
        m_initStamp = false;
//        Logger.LogError("开始监控加速齿轮");
    }

    public void EndCheckTime()
    {
        m_startCheckPing = false;
//        Logger.LogError("停止监控加速齿轮");
    }

    private void PingUpdate()
    {
        if (!m_startCheckPing || !m_focus)
            return;

        var currentLocalStamp = Util.GetTimeStamp(true);
        var currentServerStamp = Util.GetTimeStamp(false);
        if (!m_initStamp)
        {
            m_lastLocalStamp = currentLocalStamp;
            m_lastServerStamp = currentServerStamp;
            m_initStamp = true;
            m_times = 0;
            return;
        }

        var d1 = currentLocalStamp - m_lastLocalStamp;
        var d2 = currentServerStamp - m_lastServerStamp;

        if (Mathf.Abs(d1 - d2) > 5)
        {
            m_times ++;
            Logger.LogWarning(
                $"{currentLocalStamp}-{m_lastLocalStamp} = {d1} ? {d2} = {currentServerStamp}-{m_lastServerStamp}");
            if (m_times >= WarningThreshold)
                ShowWarningDialog("系统检测到游戏运行环境非法，游戏将强制退出");
        }
    }

    /// <summary>
    /// 非法操作
    /// </summary>
    public static void InValid()
    {
        if (!enableMd5Check)
            return;
        ShowWarningDialog("系统检测到非法串改数据，游戏将强制退出");
        enableMd5Check = false;
    }

    private static void ShowWarningDialog(string rMessage)
    {
#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying)
#endif
            Window_Alert.ShowAlertDefalut(rMessage,
                () => { Module_Login.instance.LogOut(false); }, null, "", "", false);
        Logger.LogError(rMessage);
    }
}
