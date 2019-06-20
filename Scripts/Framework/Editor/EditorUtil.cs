/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Editor mode utility functions.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-17
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.EditorApplication;
using Object = UnityEngine.Object;

#region Comparers

public class ObjectSortByName : IComparer<Object>
{
    public int Compare(Object a, Object b)
    {
        if (!a || !b) return 0;
        string sa = a.name, sb = b.name;

        for (int i = 0, c = Math.Min(sa.Length, sb.Length); i < c; ++i)
        {
            if (sa[i] == sb[i]) continue;
            return sa[i] - sb[i];
        }

        return 0;
    }
}

public class SortByAlpha : IComparer<string>
{
    public int Compare(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0;

        for (int i = 0, c = Math.Min(a.Length, b.Length); i < c; ++i)
        {
            if (a[i] == b[i]) continue;
            return a[i] - b[i];
        }

        return 0;
    }
}

#endregion

public static partial class EditorUtil
{
    private static string m_projectPath = "";
    private static string m_projectName = "";

    private static Dictionary<KeyValuePair<object, string>, FieldInfo> m_fieldInfoFromParent = new Dictionary<KeyValuePair<object, string>, FieldInfo>();

    private static Dictionary<Type, Dictionary<string, FieldInfo>> m_fieldInfoHierarchy = new Dictionary<Type, Dictionary<string, FieldInfo>>();

    public static FieldInfo GetFieldInfoFromPath(object source, string path)
    {
        FieldInfo field = null;
        var kvp = new KeyValuePair<object, string>(source, path);

        if (!m_fieldInfoFromParent.TryGetValue(kvp, out field))
        {
            var splittedPath = path.Split('.');
            var type = source.GetType();

            foreach (var t in splittedPath)
            {
                field = type.GetField(t, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                if (field == null)
                    break;

                type = field.FieldType;
            }

            m_fieldInfoFromParent.Add(kvp, field);
        }

        return field;
    }

    public static FieldInfo GetFieldInfoHierarchy<T>(string name)
    {
        return GetFieldInfoHierarchy(typeof(T), name);
    }

    public static FieldInfo GetFieldInfoHierarchy(Type type, string name)
    {
        if (type == null) return null;

        var c = m_fieldInfoHierarchy.GetDefault(type);
        var f = c.Get(name);
        if (f != null) return f;

        while (type != null)
        {
            f = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            type = type.BaseType;
            if (f != null) break;
        }

        c.Set(name, f);
        return f;
    }

    public static object GetParentObject(string path, object obj)
    {
        var fields = path.Split('.');

        if (fields.Length == 1)
            return obj;

        var info = obj.GetType().GetField(fields[0], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        obj = info.GetValue(obj);

        return GetParentObject(string.Join(".", fields, 1, fields.Length - 1), obj);
    }

    public static string GetProjectPath()
    {
        if (string.IsNullOrEmpty(m_projectName))
        {
            var m = Regex.Match(Application.dataPath, @"(.+\/)(\w+)\/Assets");
            m_projectPath = m.Groups[1].Value;
            m_projectName = m.Groups[2].Value;
        }

        return m_projectPath;
    }

    public static string GetProjectName(bool fullPath = false)
    {
        if (string.IsNullOrEmpty(m_projectName))
        {
            var m = Regex.Match(Application.dataPath, @"(.+\/)(\w+)\/Assets");
            m_projectPath = m.Groups[1].Value;
            m_projectName = m.Groups[2].Value;
        }
        return fullPath ? m_projectPath + m_projectName + "/" : m_projectName;
    }

    public static string GetProjectFileName()
    {
        return GetProjectName(true) + m_projectName + ".csproj";
    }

    public static long GetFileLength(string path)
    {
        if (!File.Exists(path)) return 0;

        try
        {
            var file = new FileInfo(path);
            return file.Length;
        }
        catch
        {
            return 0;
        }
    }

    public static string LoadFile(string path, bool log = true)
    {
        var file = new FileInfo(path);
        return LoadFile(path, out file, log);
    }

    public static byte[] LoadFileBinary(string path, bool log = true)
    {
        var file = new FileInfo(path);
        return LoadFileBinary(path, out file, log);
    }

    /// <summary>
    /// 以文本方式加载指定路径的文件
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="file">用以保存文件对象</param>
    /// <returns></returns>
    public static string LoadFile(string path, out FileInfo file, bool log = true)
    {
        file = new FileInfo(path);

        if (!file.Exists)
        {
            if (log) Logger.LogWarning("Util:LoadFile: Could not load file[" + path + "], file not found.");
            return string.Empty;
        }

        var reader = file.OpenText();
        var data = reader.ReadToEnd();
        reader.Close();
        reader = null;

        return data;
    }

    /// <summary>
    /// 以二进制方式加载指定路径的文件
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="file">用以保存文件对象</param>
    /// <returns></returns>
    public static byte[] LoadFileBinary(string path, out FileInfo file, bool log = true)
    {
        file = new FileInfo(path);

        if (!file.Exists)
        {
            if (log) Logger.LogWarning("Util:LoadFile: Could not load file[" + path + "], file not found.");
            return null;
        }

        var reader = file.OpenRead();
        var data = new byte[reader.Length];
        reader.Read(data, 0, data.Length);
        reader.Close();
        reader = null;

        return data;
    }

    /// <summary>
    /// 以文本方式保存数据到文件中
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="data">要保存的数据</param>
    /// <param name="lineEnding">0 = 不检测  1 = 转换为 LF  2 = 转换为 CR  3 = 转换为 CRLF</param>
    public static void SaveFile(string path, string data, int lineEnding = 0)
    {
        var dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var file = new FileInfo(path);
        SaveFile(file, data, lineEnding);
    }

    /// <summary>
    /// 以二进制方式保存数据到文件中
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="data">要保存的数据</param>
    public static void SaveFile(string path, byte[] data)
    {
        var dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var file = new FileInfo(path);
        SaveFile(file, data);
    }

    /// <summary>
    /// 以文本方式保存数据到文件中
    /// </summary>
    /// <param name="file">要保存的文件对象</param>
    /// <param name="data">要保存的数据</param>
    /// <param name="lineEnding">0 = 不检测  1 = 转换为 LF  2 = 转换为 CR  3 = 转换为 CRLF</param>
    public static void SaveFile(FileInfo file, string data, int lineEnding = 0)
    {
        if (file == null) return;

        if (lineEnding != 0)
        {
            var le = lineEnding == 1 ? "\n" : lineEnding == 2 ? "\r" : "\r\n";
            data = data.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", le);
        }

        var bs = System.Text.Encoding.UTF8.GetBytes(data);
        var writer = file.OpenWrite();
        writer.SetLength(0);
        writer.Write(bs, 0, bs.Length);
        writer.Close();
    }

    /// <summary>
    /// 以二进制方式方式保存数据到文件中
    /// </summary>
    /// <param name="file">要保存的文件对象</param>
    /// <param name="data">要保存的数据</param>
    public static void SaveFile(FileInfo file, byte[] data)
    {
        if (file == null) return;

        var writer = file.OpenWrite();
        writer.SetLength(0);
        writer.Write(data, 0, data.Length);
        writer.Close();
    }

    /// <summary>
    /// 定位到指定目录
    /// </summary>
    /// <param name="path"></param>
    public static void AllocateToDir(string path)
    {
        var psi = new ProcessStartInfo("explorer.exe");
        psi.Arguments = "/e," + (string.IsNullOrEmpty(Path.GetFileName(path)) ? "/root," : "/select,") + path.Replace("/", "\\");
        Process.Start(psi);
    }

    /// <summary>
    /// 显示一个弹出的输入框
    /// </summary>
    public static void ShowInputWindow(UnityAction<string> onInput, string title = "", string desc = "", string def = "")
    {
        var w = EditorWindow.GetWindow<InputWindow>(title, true);
        w.data = def;
        w.onInput = onInput;
        w.desc = desc;
    }

    /// <summary>
    /// 显示一个弹出的输入框
    /// </summary>
    public static void ShowInputWindow(Func<string, bool> onValidate, string title = "", string desc = "", string def = "")
    {
        var w = EditorWindow.GetWindow<InputWindow>(title, true);
        w.data = def;
        w.onValidate = onValidate;
        w.desc = desc;
    }


    #region Editor GUI helper

    private static GUIStyle m_headerStyle;
    private static Color m_defaultLineColor = new Color(0.8f, 0.8f, 0.8f);

    public static GUIStyle GetEditorGUIStyle(string name, bool clone = true)
    {
        var styles = GUI.skin.customStyles;
        foreach (var style in styles) if (style.name == name) return clone ?new GUIStyle(style) : style;
        return new GUIStyle();
    }

    public static bool DrawHeader(string title, bool state, GUIStyle style = null)
    {
        if (m_headerStyle == null) m_headerStyle = GetEditorGUIStyle("Foldout");
        m_headerStyle.fontStyle = FontStyle.Bold;
        return GUILayout.Toggle(state, title, style == null ? m_headerStyle : style);
    }

    public static void DrawSplitLine(Rect rect, Color color)
    {
        EditorGUI.DrawRect(rect, color);
    }

    public static void DrawSplitLine(Rect rect)
    {
        DrawSplitLine(rect, m_defaultLineColor);
    }

    public static void DrawSplitLineHorizontal(float contentWidth, float height, bool strech, float padding = 0, float offsetX = 0, float offsetY = 0)
    {
        var rect = EditorGUILayout.GetControlRect(false, 0);

        if (strech) rect.x = padding;
        else rect.x += offsetX;

        rect.y += offsetY;
        rect.width = contentWidth - rect.x - padding;
        rect.height = height;

        DrawSplitLine(rect);
    }

    public static void DrawSplitLineHorizontal(float contentWidth)
    {
        DrawSplitLineHorizontal(contentWidth, 1, true);
    }

    public static void DrawSplitLineHorizontal(float contentWidth, float padding)
    {
        DrawSplitLineHorizontal(contentWidth, 1, true, padding);
    }

    public static void DrawSplitLineVertical(float contentHeight, float width, bool strech, float padding = 0, float offsetX = 0, float offsetY = 0)
    {
        var rect = EditorGUILayout.GetControlRect(false, 0);

        if (strech) rect.y = padding;
        else rect.y += offsetY;

        rect.x += offsetX;
        rect.width = width;
        rect.height = contentHeight - rect.y - padding;

        DrawSplitLine(rect);
    }

    public static void DrawSplitLineVertical(float contentHeight)
    {
        DrawSplitLineVertical(contentHeight, 1, true);
    }

    public static void DrawSplitLineVertical(float contentHeight, float padding)
    {
        DrawSplitLineVertical(contentHeight, 1, true, padding);
    }

    public static void DrawRect(float width, float height, Color color)
    {
        var rect = EditorGUILayout.GetControlRect(false, 0);

        rect.width  = width;
        rect.height = height;

        EditorGUI.DrawRect(rect, color);
    }

    public static void DrawRect(float x, float width, float height, Color color)
    {
        var rect = EditorGUILayout.GetControlRect(false, 0);

        rect.x      = x;
        rect.width  = width;
        rect.height = height;

        EditorGUI.DrawRect(rect, color);
    }

    public static void DrawRect(float x, float y, float width, float height, Color color)
    {
        var rect = new Rect(x, y, width, height);
        
        EditorGUI.DrawRect(rect, color);
    }

    public static string GetFileName(string rPath, bool rWithSuffix)
    {
        var dotIndex = rPath.LastIndexOf('.');
        rPath = rPath.Replace('\\', '/');
        var splitIndex = rPath.LastIndexOf('/');

        if (splitIndex < 0 || dotIndex < 0 || dotIndex < splitIndex)
            return string.Empty;

        if (rWithSuffix)
            return rPath.Substring(splitIndex + 1);

        return rPath.Substring(splitIndex + 1, dotIndex - splitIndex - 1);
    }

    #endregion

    #region Editor queue event

    private class QueueEvent
    {
        public Delegate handler;
        public bool frameDelay;
        public int frameCount;
        public object arg0, arg1, arg2, arg3;
        public int argCount;
        public double s;
        public float delay;
        public string name;

        public void Set(Delegate _handler, double _s, float _delay, string _name, bool _frameDelay)
        {
            Set(_handler, _s, _delay, _name, 0, null, null, null, null, _frameDelay);
        }

        public void Set(Delegate _handler, double _s, float _delay, string _name, object _arg0, bool _frameDelay)
        {
            Set(_handler, _s, _delay, _name, 1, _arg0, null, null, null, _frameDelay);
        }

        public void Set(Delegate _handler, double _s, float _delay, string _name, object _arg0, object _arg1, bool _frameDelay)
        {
            Set(_handler, _s, _delay, _name, 2, _arg0, _arg1, null, null, _frameDelay);
        }

        public void Set(Delegate _handler, double _s, float _delay, string _name, object _arg0, object _arg1, object _arg2, bool _frameDelay)
        {
            Set(_handler, _s, _delay, _name, 3, _arg0, _arg1, _arg2, null, _frameDelay);
        }

        public void Set(Delegate _handler, double _s, float _delay, string _name, object _arg0, object _arg1, object _arg2, object _arg3, bool _frameDelay)
        {
            Set(_handler, _s, _delay, _name, 3, _arg0, _arg1, _arg2, _arg3, _frameDelay);
        }

        public QueueEvent(Delegate _handler, double _s, float _delay, string _name = null, int _argCount = 0, object _arg0 = null, object _arg1 = null, object _arg2 = null, object _arg3 = null, bool _frameDelay = false)
        {
            Set(_handler, _s, _delay, _name, _argCount, _arg0, _arg1, _arg2, _arg3, _frameDelay);
        }

        private void Set(Delegate _handler, double _s, float _delay, string _name, int _argCount, object _arg0, object _arg1, object _arg2, object _arg3, bool _frameDelay)
        {
            handler = _handler;
            s = _s;
            delay = _delay;
            frameCount = 0;
            frameDelay = _frameDelay;
            argCount = _argCount;
            name = _name;
            arg0 = _arg0;
            arg1 = _arg1;
            arg2 = _arg2;
            arg3 = _arg3;
        }

        public void Update()
        {
            if (frameDelay && ++frameCount < (int)delay || !frameDelay && timeSinceStartup - s <= delay) return;
            update -= Update;

            switch (argCount)
            {
                case 0:  handler.DynamicInvoke(); break;
                case 1:  handler.DynamicInvoke(arg0); break;
                case 2:  handler.DynamicInvoke(arg0, arg1); break;
                case 3:  handler.DynamicInvoke(arg0, arg1, arg2); break;
                default: handler.DynamicInvoke(arg0, arg1, arg2, arg3); break;
            }
        }
    }

    /// <summary>
    /// 增加一个延迟事件到编辑器更新队列中
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="delay"></param>
    /// <param name="frameDelay">是否使用帧延时，使用帧延时，则 delay 被当作帧数处理</param>
    public static void QueueDelayEvent(Action handler, float delay, string name = null, bool frameDelay = false)
    {
        if (name != null)
        {
            var n = update.GetInvocationList();
            foreach (var i in n)
            {
                var qe = i.Target as QueueEvent;
                if (qe == null || qe.name == null || qe.name != name) continue;
                qe.Set(handler, timeSinceStartup, delay, name, frameDelay);
                return;
            }
        }

        var e = new QueueEvent(handler, timeSinceStartup, delay, name, 0, null, null, null, null, frameDelay);

        update += e.Update;
    }

    /// <summary>
    /// 增加一个延迟事件到编辑器更新队列中
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="delay"></param>
    /// <param name="frameDelay">是否使用帧延时，使用帧延时，则 delay 被当作帧数处理</param>
    public static void QueueDelayEvent<T0>(Action<T0> handler, T0 arg0, float delay, string name = null, bool frameDelay = false)
    {
        if (name != null)
        {
            var n = update.GetInvocationList();
            foreach (var i in n)
            {
                var qe = i.Target as QueueEvent;
                if (qe == null || qe.name == null || qe.name != name) continue;
                qe.Set(handler, timeSinceStartup, delay, name, arg0, frameDelay);
                return;
            }
        }

        var e = new QueueEvent(handler, timeSinceStartup, delay, name, 1, arg0, frameDelay);

        update += e.Update;
    }

    /// <summary>
    /// 增加一个延迟事件到编辑器更新队列中
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="delay"></param>
    /// <param name="frameDelay">是否使用帧延时，使用帧延时，则 delay 被当作帧数处理</param>
    public static void QueueDelayEvent<T0, T1>(Action<T0, T1> handler, T0 arg0, T1 arg1, float delay, string name = null, bool frameDelay = false)
    {
        if (name != null)
        {
            var n = update.GetInvocationList();
            foreach (var i in n)
            {
                var qe = i.Target as QueueEvent;
                if (qe == null || qe.name == null || qe.name != name) continue;
                qe.Set(handler, timeSinceStartup, delay, name, arg0, arg1, frameDelay);
                return;
            }
        }

        var e = new QueueEvent(handler, timeSinceStartup, delay, name, 2, arg0, arg1, frameDelay);

        update += e.Update;
    }

    /// <summary>
    /// 增加一个延迟事件到编辑器更新队列中
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="delay"></param>
    /// <param name="frameDelay">是否使用帧延时，使用帧延时，则 delay 被当作帧数处理</param>
    public static void QueueDelayEvent<T0, T1, T2>(Action<T0, T1, T2> handler, T0 arg0, T1 arg1, T2 arg2, float delay, string name = null, bool frameDelay = false)
    {
        if (name != null)
        {
            var n = update.GetInvocationList();
            foreach (var i in n)
            {
                var qe = i.Target as QueueEvent;
                if (qe == null || qe.name == null || qe.name != name) continue;
                qe.Set(handler, timeSinceStartup, delay, name, arg0, arg1, arg2, frameDelay);
                return;
            }
        }

        var e = new QueueEvent(handler, timeSinceStartup, delay, name, 3, arg0, arg1, arg2, frameDelay);

        update += e.Update;
    }

    /// <summary>
    /// 增加一个延迟事件到编辑器更新队列中
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="delay"></param>
    /// <param name="frameDelay">是否使用帧延时，使用帧延时，则 delay 被当作帧数处理</param>
    public static void QueueDelayEvent<T0, T1, T2, T3>(Action<T0, T1, T2, T3> handler, T0 arg0, T1 arg1, T2 arg2, T3 arg3, float delay, string name = null, bool frameDelay = false)
    {
        if (name != null)
        {
            var n = update.GetInvocationList();
            foreach (var i in n)
            {
                var qe = i.Target as QueueEvent;
                if (qe == null || qe.name == null || qe.name != name) continue;
                qe.Set(handler, timeSinceStartup, delay, name, arg0, arg1, arg2, arg3, frameDelay);
                return;
            }
        }

        var e = new QueueEvent(handler, timeSinceStartup, delay, name, 4, arg0, arg1, arg2, arg3, frameDelay);

        update += e.Update;
    }

    #endregion
}
