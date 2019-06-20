// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-09-17      14:30
//  * LastModify：2018-09-17      14:48
//  ***************************************************************************************************/

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using AssetBundles;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Profiling;

#endregion

public static class FightRecordManager
{
    /// <summary>
    /// 是否自动保存游戏录像
    /// </summary>
    public static bool bAutoSaveRecord = false;
    /// <summary>
    /// 是否开启日志记录
    /// </summary>
    public static bool bRecordLog = true;
    /// <summary>
    /// 如果开启日志记录，是否记录堆栈信息
    /// </summary>
    public static bool bLogStack = true;
    /// <summary>
    /// debug 调试帧。
    /// </summary>
    public static int  DebugFrame = -1;
    /// <summary>
    /// 游戏开局时的帧数，记录相对帧数
    /// </summary>
    public static int Frame;
    /// <summary>
    /// 接收到的帧更新消息序列
    /// </summary>
    public static int MessageIndex;
    /// <summary>
    /// 是否处于录像播放中
    /// </summary>
    public static bool IsRecovering;
    /// <summary>
    /// 本局游戏是否已经处于战斗中
    /// </summary>
    public static bool IsFighting;
    /// <summary>
    /// 录像播放速度
    /// </summary>
    public static float PlaySpeed = 0.2f;

    public  static readonly GameRecover  gameRecover  = new GameRecover();

    public static RecordLogThread thread;


    private static void SaveToFile(string rFileName = "", bool rPost = false)
    {
        //录像播放 会自动保存战斗日志。不需要手动存储
        thread.SaveToFile(rFileName);

        if(rPost)
            thread.PostToServer(rFileName);
    }

    [Conditional("FIGHT_LOG")]
    public static void Record(PacketObject rPacket)
    {
        if (thread == null)
            return;

        if (IsRecovering)
            return;

        if (rPacket is ScPing)
            return;
        Profiler.BeginSample("Record");

        thread.Record(rPacket, MessageIndex);
        Profiler.EndSample();
    }

    [Conditional("FIGHT_LOG")]
    public static void RecordLog<T>(Action<T> fillParams, bool rStack = false) where T : PacketObject
    {
        if (thread == null)
            return;
        Profiler.BeginSample("RecordLogT");

        var p = PacketObject.Create<T>();
        fillParams.Invoke(p);
        thread.RecordLogSync(p, rStack && bLogStack, Frame);
        Profiler.EndSample();
    }

    public static void RecordLogEx<T>(Action<T> fillParams, bool rStack = false) where T : PacketObject
    {
        if (thread == null)
            return;

        var p = PacketObject.Create<T>();
        fillParams.Invoke(p);
        thread.RecordLog(p, rStack && bLogStack, Frame);
    }

    [Conditional("FIGHT_LOG")]
    public static void StartRecord()
    {
        if (thread == null)
            return;

        thread.Start();

        Frame = 0;
        MessageIndex = 0;
        IsFighting = true;

        RecordLog<LogString>(log =>
        {
            log.tag = (byte)TagType.StartRecord;
            log.value = AssetManager.dataHash;
        });

        PhysicsManager.instance?.LogColliders();

        Logger.LogWarning("开始记录");
    }

    [Conditional("FIGHT_LOG")]
    public static void EndRecord(bool rSave, bool rPost, string rFileName = "")
    {
        if (thread == null || !IsFighting)
            return;

        thread.Stop();
        gameRecover.End();

        RecordLog<LogEmpty>(log =>
        {
            log.tag = (byte)TagType.StartRecord;
        });

        if (IsRecovering)
        {
            //录像播放自动保存日志但不上传
            SaveToFile(NowTimeToFileName(), false);
        }
        else if (rSave)
        {
            SaveToFile(rFileName, rPost);
        }

        Frame = 0;
        IsRecovering = false;
        IsFighting = false;
        Logger.LogWarning("结束记录");
    }

    [Conditional("FIGHT_LOG")]
    public static void InstanceHandle<T>() where T : IGameRecordData
    {
        GameRecorder recorder = new GameRecorder();
        if (recorder.InstanceHandle<T>())
        {
            thread = !IsRecovering ? new RecordLogThread(recorder) : new RecordLogThread(null);
            if (!IsRecovering)
            {
                if (typeof (T) == typeof (GameRecordDataPvp))
                    FightStatistic.AddPvpFightTimes();
                else
                    FightStatistic.AddPveFightTimes();
            }
        }
        else
            thread = null;
    }

    [Conditional("FIGHT_LOG")]
    public static void Set<T>(T rMsg) where T : PacketObject<T>
    {
        thread?.gameRecorder?.Set(rMsg);
    }

    [Conditional("FIGHT_LOG")]
    internal static void SetMatchInfo(PMatchInfo[] infoList)
    {
        if (thread?.gameRecorder == null)
            return;

        var info = PacketObject.Create<ScMatchInfo>();
        info.isRobot = false;
        infoList.CopyTo(ref info.infoList);
        thread.gameRecorder.Set(info);
    }

    internal static string NowTimeToFileName()
    {
        var date = DateTime.Now;
#if UNITY_EDITOR
        return $"{date.Year}{date.Month:00}{date.Day:00}{date.Hour:00}{date.Minute:00}";
#else
        return $"m{date.Year}{date.Month:00}{date.Day:00}{date.Hour:00}{date.Minute:00}";
#endif
    }

    [Conditional("FIGHT_LOG")]
    public static void FrameUpdate()
    {
        if(IsFighting)
            Frame++;
#if UNITY_EDITOR
        if (Frame == DebugFrame)
            UnityEditor.EditorApplication.isPaused = true;
#endif
    }

    [Conditional("FIGHT_LOG")]
    public static void UpdateMessageIndex()
    {
        if(IsFighting)
            MessageIndex++;
    }

    /// <summary>
    /// 开始播放一局录像
    /// </summary>
    public static GameRecover.EnumPlayError StartRecover(string rPath)
    {
        var error = gameRecover.StartLoad(rPath);
        if (error == GameRecover.EnumPlayError.None)
        {
            IsRecovering = true;
            gameRecover.Start();
        }

        return error;
    }

    public static List<PacketObject> SimulateUpdate(float delta)
    {
        if (!IsFighting)
            return null;
        return gameRecover.GameRecordDataHandle.SimulateUpdate(delta);
    }

    public static GameRecover.EnumPlayError TransformToJson(string rPath)
    {
        var error = gameRecover.StartLoad(rPath);
        if (error == GameRecover.EnumPlayError.None)
        {
            gameRecover.SaveToJson();
        }
        return error;
    }
}

