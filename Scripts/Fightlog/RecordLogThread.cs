// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-03-14      14:13
//  *LastModify：2019-03-16      13:41
//  ***************************************************************************************************/

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Profiling;

#endregion

public enum SaveState
{
    Begain,
    Wait,
    Complete
}

public class RecordLogThread
{
    private class LogEntry
    {
        private static readonly ThreadSafePool<LogEntry> m_pool = new ThreadSafePool<LogEntry>(256);

        public PacketObject packet;
        public int          frame;
        public StackTrace   stackTrace;
        public static LogEntry Create(PacketObject rPacket, int rFrame, StackTrace rStackTrace)
        {
            var entry = m_pool.Pop();
            entry.packet = rPacket;
            entry.frame = rFrame;
            entry.stackTrace = rStackTrace;
            return entry;
        }

        public void Free()
        {
            packet = null;
            frame = 0;
            stackTrace = null; 
            m_pool.Back(this);
        }
    }

    private class ProtocolEntry
    {
        private static readonly ThreadSafePool<ProtocolEntry> m_pool = new ThreadSafePool<ProtocolEntry>(256);

        public PacketObject packet;
        public int frame;

        public static ProtocolEntry Create(PacketObject rPacket, int rFrame)
        {
            var entry = m_pool.Pop();
            entry.packet = rPacket;
            entry.frame = rFrame;
            return entry;
        }

        public void Free()
        {
            packet = null;
            frame = 0;
            m_pool.Back(this);
        }
    }


    private static readonly Dictionary<string, SaveState> s_stateDict = new Dictionary<string, SaveState>();
    public readonly FightLog       gameLogger = new FightLog();

    public readonly GameRecorder   gameRecorder;

    private readonly Queue<LogEntry>       logQueue = new Queue<LogEntry>();
    private readonly Queue<ProtocolEntry>  packets  = new Queue<ProtocolEntry>();

    private  Thread m_logThread;
    private  Thread m_packetThread;

    private bool m_pending = true;
    private  Thread m_saveThread;

    private bool m_useThread = true;

    public RecordLogThread(GameRecorder rGameRecorder)
    {
        gameRecorder = rGameRecorder;
        Util.GetDataPath();
        Util.GetExternalDataPath();
    }

    public static void SetSaveState(string rFileName, SaveState rState)
    {
        s_stateDict.Set(rFileName, rState);
    }

    public static bool IsFileSaveComplete(string rFileName)
    {
        if (s_stateDict.ContainsKey(rFileName))
            return s_stateDict[rFileName] == SaveState.Complete;
        return true;
    }

    public void Start()
    {
        m_pending = false;
        m_saveThread = null;

        if (FightRecordManager.bRecordLog)
        {
            m_logThread    = new Thread(_ExcuteLog);
            if (!m_logThread.IsAlive)
                m_logThread.Start();
            gameLogger.Start();
        }

        if (gameRecorder != null)
        {
            m_packetThread = new Thread(_ExcutePacket);
            if (!m_packetThread.IsAlive)
                m_packetThread.Start();
            gameRecorder.Start();
        }
    }

    public void Stop()
    {
        m_pending = true;
        gameRecorder?.End();
        gameLogger?.End();
    }

    #region 文件保存

    public void SaveToFile(string rFileName = "")
    {
        if (m_useThread && m_logThread == null && m_packetThread == null)
            return;

        if (m_saveThread != null)
            return;

        m_saveThread = new Thread(_SaveToFile);
        m_saveThread.Start(rFileName);

        if(m_logThread != null && m_logThread.IsAlive)
            SetSaveState(rFileName + ".log", SaveState.Wait);
        if(m_packetThread != null && m_packetThread.IsAlive)
            SetSaveState(rFileName + ".gr" , SaveState.Wait);
    }

    private void _SaveToFile(object obj)
    {
        Logger.LogWarning($"开始文件保存线程:{m_saveThread.GetHashCode()}");

        while (m_logThread != null && m_logThread.IsAlive) Thread.Sleep(100);
        while (m_packetThread != null && m_packetThread.IsAlive) Thread.Sleep(100);
        try
        {
            var fileName = (string) obj;

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = FightRecordManager.NowTimeToFileName();
            }

            try
            {
                if (m_packetThread != null)
                {
                    gameRecorder._SaveToFile(fileName);
                    SetSaveState(fileName + ".gr", SaveState.Complete);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }

            try
            {
                if (m_logThread != null)
                {
                    gameLogger.SaveToFile(fileName);
                    SetSaveState(fileName + ".log", SaveState.Complete);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }

            m_saveThread.Interrupt();

        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
        }
        Logger.LogWarning($"结束文件保存线程:{m_saveThread.GetHashCode()}");
        m_saveThread = null;
    }

    #endregion

    #region 日志记录

    public void RecordLogSync(PacketObject rPacket, bool rStack, int rFrame)
    {
        if (m_pending)
            return;
        RecordLog(rPacket, rStack, rFrame);
    }

    public void RecordLog(PacketObject rPacket, bool rStack, int rFrame)
    {
        lock (logQueue)
        {
            StackTrace st = null;
            if (rStack)
                st = new StackTrace(false);
            if(m_useThread)
                logQueue.Enqueue(LogEntry.Create(rPacket, rFrame, st));
            else
                gameLogger.Log(rPacket, rFrame, st);
        }
    }

    private void _ExcuteLog()
    {
        Logger.LogWarning($"开始处理日志线程:{m_logThread.GetHashCode()}");
        while (m_logThread.IsAlive)
        {

            if (logQueue.Count > 0)
            {
                lock (logQueue)
                {
                    var t = logQueue.Dequeue();
                    gameLogger.Log(t.packet, t.frame, t.stackTrace);
                    t.Free();
                }
            }
            else if (m_pending)
            {
                m_logThread.Interrupt();
                break;
            }
            else
                Thread.Sleep(10);
        }

        Logger.LogWarning($"结束处理日志线程:{m_logThread.GetHashCode()}");
    }

    #endregion

    #region 协议记录

    public void Record(PacketObject rPacket, int rFrame)
    {
        if (m_pending)
            return;

        lock (packets)
        {
            var p = rPacket.BuildPacket();
            var packet = Packet.Build(p.header, p.bytes, false);
            var obj = PacketObject.Create(packet);
            if (m_useThread)
                packets.Enqueue(ProtocolEntry.Create(obj, rFrame));
            else
                gameRecorder.Record(obj, rFrame);
        }
    }

    private void _ExcutePacket()
    {
        Logger.LogWarning($"开始协议记录线程:{m_packetThread.GetHashCode()}");

        while (m_packetThread.IsAlive)
        {
            if (packets.Count > 0)
            {
                lock (packets)
                {
                    var t = packets.Dequeue();
                    gameRecorder.Record(t.packet, t.frame);
                    t.Free();
                }
            }
            else if (m_pending)
            {
                if(!gameRecorder.reward)
                    Thread.Sleep(3000);
                m_packetThread.Interrupt();
                break;
            }
            else
                Thread.Sleep(10);
        }
        Logger.LogWarning($"结束协议记录线程:{m_packetThread.GetHashCode()}");
    }

    #endregion

    #region 传送文件到http服务器

    public void PostToServer(string rFileName)
    {
        Root.instance.StartCoroutine(_PostToServer(rFileName));
    }

    public IEnumerator _PostToServer(string rFileName)
    {
        while (!IsFileSaveComplete(rFileName + ".log"))
            yield return 0;

        var account = Module_Login.instance?.account;
        WWWForm postForm = new WWWForm();

        var playerName = string.Empty;
        if (Module_Player.instance != null)
            playerName = Module_Player.instance.roleInfo.roleId.ToString();
        var path = gameLogger.GetFullPath(rFileName);
        var contents = Util.LoadFile(path);
        if (contents != null && contents.Length > 0)
        {
            postForm.AddBinaryData("logFile", contents, $"{rFileName }_{playerName}.log");
        }

        var request = UnityWebRequest.Post(WebAPI.FullApiUrl(WebAPI.RES_FIGHT_DATA), postForm);
        request.SetRequestHeader("Content-Type", postForm.headers["Content-Type"]);
        request.SetRequestHeader("Authorization", BasicAuth(account?.acc_name ?? "null", "123456"));
        request.SetRequestHeader("X-Game-Identity", $"kzwg/{rFileName}");

        request.timeout = 5;
        yield return request.SendWebRequest();
        if (request.isNetworkError)
        {
            Logger.LogWarning($"日志文件上传失败:{request.url}");
            yield break;
        }
        Logger.LogWarning($"日志文件上传成功:{WebAPI.FullApiUrl(WebAPI.RES_FIGHT_DATA)} 数据大小：{contents.Length}");
        request.Dispose();

        while (!IsFileSaveComplete(rFileName + ".gr"))
            yield return 0;

        postForm = new WWWForm();
        path = GameRecorder.GetFullPath(rFileName);
        contents = Util.LoadFile(path);
        if (contents != null && contents.Length > 0)
        {
            postForm.AddBinaryData("grFile", contents, $"{rFileName }_{playerName}.gr");
        }
        
        request = UnityWebRequest.Post(WebAPI.FullApiUrl(WebAPI.RES_FIGHT_DATA), postForm);
        request.SetRequestHeader("Content-Type", postForm.headers["Content-Type"]);
        request.SetRequestHeader("Authorization", BasicAuth(account?.acc_name ?? "null", "123456"));
        request.SetRequestHeader("X-Game-Identity", $"kzwg/{rFileName}");

        request.timeout = 5;
        yield return request.SendWebRequest();
        if (request.isNetworkError)
        {
            Logger.LogWarning($"录像文件上传失败:{request.url}");
            yield break;
        }
        Logger.LogWarning($"录像文件上传成功:{WebAPI.FullApiUrl(WebAPI.RES_FIGHT_DATA)} 数据大小：{contents.Length}");
        request.Dispose();
    }
    private string BasicAuth(string user, string password)
    {
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
        {
            throw new ArgumentNullException();
        }
        return "Basic " + Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(string.Format("{0}:{1}", user, password)));
    }
    #endregion
}
