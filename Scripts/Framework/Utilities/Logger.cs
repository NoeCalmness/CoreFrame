/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Log system.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-02-27
 * 
 ***************************************************************************************************/

using System;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum LogCategory { GAME = 0 };
public enum LogType { INFO = 0, WARNING = 1, ERROR = 2, BATTLE = 3, SEND = 4, RECV = 5, DETAIL = 6, EXCEPTION = 7, CHAT = 8, CPP = 9 };

public static class Logger
{
    public static bool enabled = true;

    private static bool m_initialized  = false;

    private static Dictionary<int, string> m_catPrefix    = null;
    private static Dictionary<int, string> m_typeColors   = null;
    private static Dictionary<int, string> m_typePrefix   = null;
    private static Dictionary<int, bool>   m_disabled     = new Dictionary<int, bool>();

    private static Stack<StringBuilder> m_sbs = new Stack<StringBuilder>();
    private static Stack<List<string>> m_lss = new Stack<List<string>>();

    private static string[] m_validTags = { "color", "b", "size" };

    static Logger()
    {
        Initialize();

#if UNITY_EDITOR
        m_instanceID = AssetDatabase.LoadAssetAtPath<MonoScript>(m_loggerPath).GetInstanceID();
        m_stackFrams.Clear();

        GetListView();
#endif
    }

    #region Helper

    public static void SetLogState(int type, bool state)
    {
        if (type < 0)
        {
            m_disabled.Clear();
            if (state) return;
            foreach (var t in m_typePrefix.Keys) m_disabled.Add(t, false);
        }
        else
        {
            m_disabled.Remove(type);
            if (!state) m_disabled.Add(type, false);
        }
    }

    public static void SetLogCategory(int category, string prefix)
    {
        if (category < 1)
        {
            _Log((int)LogCategory.GAME, (int)LogType.ERROR, "Custom log category must > 0. [{0}, {1}]", category, prefix);
            return;
        }

        m_catPrefix.Remove(category);
        m_catPrefix.Add(category, prefix);
    }

    public static void SetLogType(int type, string prefix, string color = "white")
    {
        if (type < (int)LogType.CPP)
        {
            _Log((int)LogCategory.GAME, (int)LogType.ERROR, "Custom log category must > {3}. [{0}, {1}, {2}]", type, prefix, color, (int)LogType.CPP);
            return;
        }

        m_typePrefix.Remove(type);
        m_typeColors.Remove(type);
        m_typePrefix.Add(type, prefix);
        m_typeColors.Add(type, color);
    }

    private static string FixEditorStyle(string msg)
    {
        #if !UNITY_EDITOR
        return msg;
        #endif

        if (string.IsNullOrEmpty(msg)) return msg;

        List<string> tagss, tagse;
        lock (m_lss)
        {
            tagss = m_lss.Count > 0 ? m_lss.Pop() : new List<string>();
            tagse = m_lss.Count > 0 ? m_lss.Pop() : new List<string>();
        }

        var lines = 0;
        string tag = null;
        for (int i = 0, l = msg.Length; i < l; ++i)
        {
            var c = msg[i];

            if (c != '\n')
            {
                if (c == '<') { tag = "<"; }
                else if (c != '\r' && c != ' ' && tag != null)
                {
                    tag += c;
                    if (c == '>')
                    {
                        var end = tag[1] == '/';
                        var valid = false;
                        foreach (var vt in m_validTags) if (valid = string.Compare(tag, end ? 2 : 1, vt, 0, vt.Length, true) == 0) break;
                        if (tag.Length > 2 && valid)
                        {
                            if (end) { if (tagss.Count > 1) { tagss.RemoveAt(tagss.Count - 1); tagse.RemoveAt(0); } }
                            else
                            {
                                tagss.Add(tag);
                                var sidx = tag.IndexOf('=');
                                tagse.Insert(0, "</" + (sidx < 0 ? tag.Substring(1) : tag.Substring(1, sidx - 1) + ">"));
                            }
                        }
                        tag = null;
                    }
                }

                continue;
            }

            if (++lines > 1)
            {
                if (i == l - 1) break;
                var s = msg.Substring(0, i);
                foreach (var t in tagse) s += t;
                s += "\n";
                foreach (var t in tagss) s += t;
                s += msg.Substring(i + 1);

                msg = s;
                break;
            }
        }

        tagss.Clear(); tagse.Clear();
        lock (m_lss) { m_lss.Push(tagss); m_lss.Push(tagse); }

        return msg;
    }

    #endregion

    #region Quicklink

    [Conditional("NLog")]
    public static void NLog(string msg, params object[] args)
    {
        if (!enabled) return;
        _Log((int)LogCategory.GAME, (int)LogType.INFO, Time.time + msg, args);
    }

    [Conditional("NLog")]
    public static void NLogError(string msg, params object[] args)
    {
        if (!enabled) return;
        _Log((int)LogCategory.GAME, (int)LogType.ERROR, Time.time + msg, args);
    }

    public static void LogInfo(string msg, params object[] args)
    {
        if (!enabled) return;
        _Log((int)LogCategory.GAME, (int)LogType.INFO, msg, args);
    }

    public static void LogWarning(string msg, params object[] args)
    {
        if (!enabled) return;
        _Log((int)LogCategory.GAME, (int)LogType.WARNING, msg, args);
    }

    public static void LogError(string msg, params object[] args)
    {
        if (!enabled) return;
        _Log((int)LogCategory.GAME, (int)LogType.ERROR, msg, args);
    }

    public static void LogDetail(string msg, params object[] args)
    {
        if (!enabled) return;
        _Log((int)LogCategory.GAME, (int)LogType.DETAIL, msg, args);
    }

    public static void LogException(string msg, params object[] args)
    {
        if (!enabled) return;
        _Log((int)LogCategory.GAME, (int)LogType.EXCEPTION, msg, args);
    }

    public static void LogException(Exception e)
    {
        if (!enabled) return;
        if (e == null) _Log((int)LogCategory.GAME, (int)LogType.EXCEPTION, "Unknow Exception.");
        else _Log((int)LogCategory.GAME, (int)LogType.EXCEPTION, e.ToString());
    }

    public static void LogChat(string msg, params object[] args)
    {
        if (!enabled) return;
        _Log((int)LogCategory.GAME, (int)LogType.CHAT, msg, args);
    }

    public static void Log(LogType type, string msg, params object[] args)
    {
        if (!enabled) return;
        _Log((int)LogCategory.GAME, (int)type, msg, args);
    }

    public static void Log(LogCategory category, LogType type, string msg, params object[] args)
    {
        if (!enabled) return;
        _Log((int)category, (int)type, msg, args);
    }

    public static void Log(int category, int type, string msg, params object[] args)
    {
        if (!enabled) return;
        _Log(category, type, msg, args);
    }

    #endregion

    private static void Initialize()
    {
        if (m_initialized) return;
        m_initialized = true;

        m_catPrefix  = new Dictionary<int, string>() { { 0, "GAME" } };
        m_typeColors = new Dictionary<int, string>() { { 0, "lime" }, { 1, "yellow" }, { 2, "red" },{ 3, "aqua" }, { 4, "#FFFF80" }, { 5, "#00FFFF" },  { 6, "orange" }, { 7, "red" }, { 8, "#FFA5FCFF" }, { 9, "#54FFFFFF" } };
        m_typePrefix = new Dictionary<int, string>() { { 0, "INF"  }, { 1, "WAN"    }, { 2, "ERR" },{ 3, "BAT"  }, { 4, "SEND"    }, { 5, "RECV" },     { 6, "DET"    }, { 7, "EXC" }, { 8, "CHAT" },      { 9, "PACK"} };

        Application.SetStackTraceLogType(UnityEngine.LogType.Log,       StackTraceLogType.ScriptOnly);
        Application.SetStackTraceLogType(UnityEngine.LogType.Assert,    StackTraceLogType.ScriptOnly);
        Application.SetStackTraceLogType(UnityEngine.LogType.Error,     StackTraceLogType.ScriptOnly);
        Application.SetStackTraceLogType(UnityEngine.LogType.Exception, StackTraceLogType.ScriptOnly);
        Application.SetStackTraceLogType(UnityEngine.LogType.Warning,   StackTraceLogType.ScriptOnly);

        if (m_sbs == null) m_sbs = new Stack<StringBuilder>();
        else m_sbs.Clear();

        if (m_lss == null) m_lss = new Stack<List<string>>();
        else m_lss.Clear();

        #if !DEVELOPMENT_BUILD && !UNITY_EDITOR
        enabled = false;
        #endif
    }

    private static void _Log(int category, int type, string msg, params object[] args)
    {
        if (!enabled || m_disabled.ContainsKey(type)) return;

        #if UNITY_EDITOR
        var stackTrace = new StackTrace(true);
        var stackFrame = stackTrace.GetFrame(2);
        m_stackFrams.Add(stackFrame);
        #endif

        string cp = null, tp = null, co = null;

        if (!m_catPrefix.TryGetValue(category, out cp)) cp = "CAT_" + category;
        if (!m_typePrefix.TryGetValue(type,    out tp)) tp = "TYPE_" + type;
        if (!m_typeColors.TryGetValue(type,    out co)) co = "white";

        StringBuilder sbuilder;
        lock (m_sbs) { sbuilder = m_sbs.Count > 0 ? m_sbs.Pop() : new StringBuilder(); }
        {
            sbuilder.AppendFormat("<color={0}><b><color=#FFFFFF>{3}{4:000}</color> {1}::{2}: </b>{5}</color>", co, cp, tp, $"{DateTime.Now.ToString("HH:mm:ss")}.", DateTime.Now.Millisecond, Util.Format(msg, args));

            msg = FixEditorStyle(sbuilder.ToString());
            sbuilder.Clear();
        }
        lock (m_sbs) { m_sbs.Push(sbuilder); }

        switch (type)
        {
            case (int)LogType.WARNING:   UnityEngine.Debug.LogWarning(msg); break;
            case (int)LogType.ERROR:     UnityEngine.Debug.LogError(msg);   break;
            case (int)LogType.EXCEPTION: UnityEngine.Debug.LogError(msg);   break;
            default:                     UnityEngine.Debug.Log(msg);        break;
        }
    }

    #region Editor VisualStudio log callback

#if UNITY_EDITOR
    private static int     m_instanceID = 0;
    private static string  m_loggerPath = "Assets/Scripts/Framework/Utilities/Logger.cs";
    private static List<StackFrame> m_stackFrams = new List<StackFrame>();

    private static object     m_consoleWindow;
    private static object     m_listView;
    private static FieldInfo  m_listViewRows;
    private static FieldInfo  m_listViewRow;
    private static MethodInfo m_getEntries;
    private static object     m_entry;
    private static FieldInfo  m_entryCond;

    private static void GetListView()
    {
        if (m_listView == null)
        {
            var assembly    = Assembly.GetAssembly(typeof(EditorWindow));
            var windowType  = assembly.GetType("UnityEditor.ConsoleWindow");
            var entriesType = assembly.GetType("UnityEditor.LogEntries");
            var entryType   = assembly.GetType("UnityEditor.LogEntry");
            var fieldInfo   = windowType.GetField("m_ListView", BindingFlags.Instance | BindingFlags.NonPublic);

            m_consoleWindow  = windowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            if (m_consoleWindow == null) return;

            m_listView       = fieldInfo.GetValue(m_consoleWindow);
            m_listViewRows   = fieldInfo.FieldType.GetField("totalRows", BindingFlags.Instance | BindingFlags.Public);
            m_listViewRow    = fieldInfo.FieldType.GetField("row", BindingFlags.Instance | BindingFlags.Public);
            m_getEntries     = entriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);
            m_entry          = Activator.CreateInstance(entryType);
            m_entryCond      = entryType.GetField("condition", BindingFlags.Instance | BindingFlags.Public);
        }
    }

    private static StackFrame GetStackFrame()
    {
        if (m_listView == null) return null;

        var rows = (int)m_listViewRows.GetValue(m_listView);
        var row  = (int)m_listViewRow.GetValue(m_listView);

        var customLogs = 0;
        for (int i = rows - 1; i >= row; --i)
        {
            m_getEntries.Invoke(null, new object[] { i, m_entry });
            var cond = m_entryCond.GetValue(m_entry) as string;
            if (cond.Contains("</color>"))
                ++customLogs;
        }

        if (customLogs < 1) return null;

        while (m_stackFrams.Count > rows)
            m_stackFrams.RemoveAt(0);

        if (m_stackFrams.Count >= customLogs)
            return m_stackFrams[m_stackFrams.Count - customLogs];
        return null;
    }

    [UnityEditor.Callbacks.OnOpenAsset(0)]
    public static bool OnOpenAsset0(int instanceID, int line)
    {
        if (instanceID != m_instanceID) return false;

        var stackFrame = GetStackFrame();
        if (stackFrame != null)
        {
            var fileName  = stackFrame.GetFileName();
            var assetPath = fileName.Substring(fileName.IndexOf("Assets"));
            if (assetPath.Replace("\\", "/") == m_loggerPath) return false;
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath), stackFrame.GetFileLineNumber());
            return true;
        }

        return false;
    }
#endif

    #endregion
}