/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Global utility functions.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.0
 * Created:  2017-02-28
 * 
 ***************************************************************************************************/

using System;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static partial class Util
{
    public static readonly char[] defaultSeparator = { ' ', ';' };

    private static Camera[] m_cachedCameras = new Camera[20]; // Max allowed cameras, 20 ? are you kidding me ?...

    private static string[] m_excludeLayerTarget = new string[] { "Bip001" };

    private static NumberStyles m_ns = NumberStyles.Number | NumberStyles.Float;

    private static System.Text.RegularExpressions.Regex m_regEscape   = new System.Text.RegularExpressions.Regex(@"\\?(\\(0x[a-fA-F0-9]{2}))");
    private static System.Text.RegularExpressions.Regex m_regTagColor = new System.Text.RegularExpressions.Regex(@"\[([a-fA-F0-9]{1,6})\]");
    private static System.Text.RegularExpressions.Regex m_regTagSize  = new System.Text.RegularExpressions.Regex(@"\[(?:s(\d+))\]");

    private static SensitiveKeywordInfo m_sensitiveInfo = null;

    #region Sensitive keyword validation

    /// <summary>
    /// 将指定文本的所有敏感字替换为指定字符串并返回新的字符串
    /// 注意，无论匹配到的敏感字长度如何，都将替换为指定字符串
    /// </summary>
    /// <param name="input">要检查的字符串</param>
    /// <param name="replace">替换的字符串</param>
    /// <returns>替换后的字符串</returns>
    public static string ValidateSensitiveWords(string input, string replace = null)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input)) return input;

        if (!m_sensitiveInfo) m_sensitiveInfo = SensitiveKeywordInfo.defaultConfig;
        #if !UNITY_EDITOR
        if (string.IsNullOrEmpty(replace))
        #endif
            replace = GeneralConfigInfo.ssensitiveReplace;

        var words = m_sensitiveInfo.keywords;
        foreach (var word in words)
        {
            if (input.Length < word.Length) continue;

            int kl = word.Length, il = input.Length, l = -1, r = -1;

            for (int i = 0, c = il - kl + 1; i < c;)
            {
                if (IsSymbol(input[i]) || !ApproximatelyEquals(word, input, i, kl, il, ref l, ref r))
                {
                    ++i;
                    continue;
                }

                if (l < 1) input = r < il - 1 ? string.Concat(replace, input.Substring(r + 1)) : replace;
                else input = r < il - 1 ? string.Concat(input.Substring(0, l), replace, input.Substring(r + 1)) : string.Concat(input.Substring(0, l), replace);

                i = l + replace.Length;  // Update string length and iteration position after replace
                il = input.Length;
                c = il - word.Length;

            }
        }

        return input;
    }

    /// <summary>
    /// 检查指定文本是否包含敏感字
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool ContainsSensitiveWord(string input)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input)) return false;

        if (!m_sensitiveInfo) m_sensitiveInfo = SensitiveKeywordInfo.defaultConfig;
        var words = m_sensitiveInfo.keywords;
        foreach (var word in words)
        {
            if (input.Length < word.Length) continue;

            int kl = word.Length, il = input.Length, l = -1, r = -1;
            
            for (int i = 0, c = il - kl + 1; i < c; ++i)
                if (!IsSymbol(input[i]) && ApproximatelyEquals(word, input, i, kl, il, ref l, ref r)) return true;
        }
        return false;
    }

    private static bool ApproximatelyEquals(string key, string input, int start, int keyLen, int inputLen, ref int left, ref int right)
    {
        for (var i = 0; i < keyLen; ++i)
        {
            var matched = false;
            var k = key[i];
            var upper = IsChar(k);
            while (start < inputLen)
            {
                var c = input[start++];
                if (IsSymbol(c)) continue;
                if (c == k || upper == 1 && c - 0x20 == k || upper == -1 && c + 0x20 == k)
                {
                    matched = true;

                    if (i == 0) left = start - 1;
                    if (i == keyLen - 1) right = start - 1;

                    break;
                }

                return false;
            }

            if (!matched) return false;
        }

        return true;
    }
    
    public static bool IsSymbol(char c)
    {
        return c >= 0x20 && c <= 0x40 || c >= 0x5B && c <= 0x60 || c >= 0x7B && c <= 0x7E;
    }

    /// <summary>
    /// Is c a character ?
    /// 1 = upper  -1 = lower  0 = c is not a character
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static int IsChar(char c)
    {
        return c >= 0x41 && c <= 0x5A ? 1 : c >= 0x61 && c <= 0x7A ? -1 : 0;
    }

    #endregion

    #region String/Parse helpers

    public static string FirstUpper(string w)
    {
        if (string.IsNullOrEmpty(w) || w.Length < 2) return w.ToUpper();
        return w[0].ToString().ToUpper() + w.Substring(1);
    }

    public static string FirstLower(string w)
    {
        if (string.IsNullOrEmpty(w) || w.Length < 2) return w.ToLower();
        return char.ToLower(w[0]) + w.Substring(1);
    }

    public static string FirstWordLower(string w)
    {
        if (string.IsNullOrEmpty(w) || w.Length < 2) return w.ToLower();
        for (var i = 1; i < w.Length - 1; ++i)
        {
            if (w[i].isUpperCase() && !w[i + 1].isUpperCase())
                return w.Substring(0, i).ToLower() + w.Substring(i);
        }
        return w.ToLower();
    }

    /// <summary>
    /// 将一个字符串转化为指定枚举类型
    /// 可以使用枚举名称，也可以使用枚举值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="str"></param>
    /// <returns></returns>
    public static T ParseEnum<T>(string str) where T : struct
    {
        if (string.IsNullOrEmpty(str)) return default(T); // Logger always ignore null string

        var type = typeof(T);
        if (!type.IsEnum)
        {
            Logger.LogError("Util::ParseEnum: Type {0} is not an Enum type. str: {1}", type, str);
            return default(T);
        }

        T e;
        if (Enum.TryParse(str, true, out e)) return e;

        Logger.LogError("Util::ParseEnum: {0} is not a member of enum type {1}", str, type);

        return default(T);
    }

    /// <summary>
    /// 将一个字符串转化为指定枚举类型
    /// 可以使用枚举名称，也可以使用枚举值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="str"></param>
    /// <returns></returns>
    public static object ParseEnum(Type type, string str)
    {
        if (string.IsNullOrEmpty(str)) return Activator.CreateInstance(type); // Logger always ignore null string

        if (!type.IsEnum)
        {
            Logger.LogError("Util::ParseEnum: Type {0} is not an Enum type. str: {1}", type, str);
            return Activator.CreateInstance(type);
        }

        try
        {
            var e = Enum.Parse(type, str);
            return e;
        }
        catch (ArgumentException)
        {
            Logger.LogError("Util::ParseEnum: {0} is not a member of enum type {1}", str, type);
            return Activator.CreateInstance(type);
        }
    }

    /// <summary>
    /// 将一个字符串转换为指定类型
    /// </summary>
    public static object Parse(Type type, string str)
    {
        var typeCode = Type.GetTypeCode(type);

        if (string.IsNullOrEmpty(str)) return type.IsValueType? Activator.CreateInstance(type) : typeCode == TypeCode.String ? "" : null;

        var hex = false;
        if (str.Length > 0 && str[0] == '#')
        {
            hex = true;
            str = str.Length > 1 ? str.Substring(1) : "";
        }

        if (str.Length > 1 && str[0] == '0' && (str[1] == 'x' || str[1] == 'X'))
        {
            hex = true;
            str = str.Length > 2 ? str.Substring(2) : "";
        }

        switch (typeCode)
        {
            case TypeCode.Boolean:
            {
                bool value = false;
                if (!bool.TryParse(str, out value))
                {
                    var i = 0;
                    if (int.TryParse(str, out i))
                        value = i != 0;
                }
                return value;
            }
            case TypeCode.Char:    { char    value    = '\0';    char.TryParse(str,    out value);  return value; }
            case TypeCode.String:  { string  value    = str;                                        return value; }
            // numbers can use hex format string
            case TypeCode.Byte:    { byte    value    = 0;       byte.TryParse(str,   hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return value; }
            case TypeCode.SByte:   { sbyte   value    = 0;       sbyte.TryParse(str,  hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return value; }
            case TypeCode.Int16:   { short   value    = 0;       short.TryParse(str,  hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return value; }
            case TypeCode.UInt16:  { ushort  value    = 0;       ushort.TryParse(str, hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return value; }
            case TypeCode.Int32:   { int     value    = 0;       int.TryParse(str,    hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return value; }
            case TypeCode.UInt32:  { uint    value    = 0;       uint.TryParse(str,   hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return value; }
            case TypeCode.Int64:   { long    value    = 0;       long.TryParse(str,   hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return value; }
            case TypeCode.UInt64:  { ulong   value    = 0;       ulong.TryParse(str,  hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return value; }
            case TypeCode.Single:  { float   value    = 0;       float.TryParse(str,  hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return value; }
            case TypeCode.Double:  { double  value    = 0;       double.TryParse(str, hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return value; }
            default:
            {
                Logger.LogWarning("Util::Parse: Try to convert unsupported type[" + type.Name + "].");
                return type.IsValueType ? Activator.CreateInstance(type) : null;
            }
        }
    }

    /// <summary>
    /// 将一个字符串转换为指定类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="str"></param>
    /// <returns></returns>
    public static T Parse<T>(string str)
    {
        var typeCode = Type.GetTypeCode(typeof(T));

        if (string.IsNullOrEmpty(str)) return typeCode == TypeCode.String ? (T)(object)"" : default(T);

        var hex = false;
        if (str.Length > 0 && str[0] == '#')
        {
            hex = true;
            str = str.Substring(1);
        }

        if (str.Length > 1 && str[0] == '0' && (str[1] == 'x' || str[1] == 'X'))
        {
            hex = true;
            str = str.Substring(2);
        }

        switch (typeCode)
        {
            case TypeCode.Boolean:
            {
                bool value = false;
                if (!bool.TryParse(str, out value))
                {
                    var i = 0;
                    if (int.TryParse(str, out i))
                        value = i != 0;
                }
                return (T)(object)value;
            }
            case TypeCode.Char:    { char    value    = '\0';    char.TryParse(str,  out value);  return (T)(object)value; }
            case TypeCode.String:  { string  value    = str;                                      return (T)(object)value; }
            // numbers can use hex format string
            case TypeCode.Byte:    { byte    value    = 0;       byte.TryParse(str,   hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return (T)(object)value; }
            case TypeCode.SByte:   { sbyte   value    = 0;       sbyte.TryParse(str,  hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return (T)(object)value; }
            case TypeCode.Int16:   { short   value    = 0;       short.TryParse(str,  hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return (T)(object)value; }
            case TypeCode.UInt16:  { ushort  value    = 0;       ushort.TryParse(str, hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return (T)(object)value; }
            case TypeCode.Int32:   { int     value    = 0;       int.TryParse(str,    hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return (T)(object)value; }
            case TypeCode.UInt32:  { uint    value    = 0;       uint.TryParse(str,   hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return (T)(object)value; }
            case TypeCode.Int64:   { long    value    = 0;       long.TryParse(str,   hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return (T)(object)value; }
            case TypeCode.UInt64:  { ulong   value    = 0;       ulong.TryParse(str,  hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return (T)(object)value; }
            case TypeCode.Single:  { float   value    = 0;       float.TryParse(str,  hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return (T)(object)value; }
            case TypeCode.Double:  { double  value    = 0;       double.TryParse(str, hex ? NumberStyles.HexNumber : m_ns, CultureInfo.InvariantCulture, out value);  return (T)(object)value; }
            default:
            {
                Logger.LogWarning("Util::Parse: Try to convert unsupported type[" + typeof(T).Name + "].");
                return default(T);
            }
        }
    }

    /// <summary>
    /// 将一个字符串转换为指定类型的数组
    /// 注意，对于无法转换的数据类型，将使用 T 的默认值，这意味着返回结果中可能包含 null
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="str">原字符串</param>
    /// <param name="hasEmpty">是否包含空项</param>
    /// <param name="separator">分隔符</param>
    /// <returns></returns>
    public static T[] ParseString<T>(string str, bool hasEmpty = false, params char[] separator)
    {
        if (string.IsNullOrEmpty(str)) return new T[] { };
        var parsedStr = str.Split(separator.Length < 1 ? defaultSeparator : separator, hasEmpty ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries);
        if (typeof (T) == typeof (string))
            return parsedStr as T[];
        var list = new List<T>(parsedStr.Length);
        foreach (var item in parsedStr) list.Add(Parse<T>(item));
        return list.ToArray();
    }

    /// <summary>
    /// 解析指定 string 的特殊 tag 标签，包括特殊字符和 [][-] 标签
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ParseStringTags(string input)
    {
        #region Editor helper
        #if UNITY_EDITOR
        if (m_regEscape   == null) m_regEscape   = new System.Text.RegularExpressions.Regex(@"\\?(\\(0x[a-fA-F0-9]{2}))");
        if (m_regTagColor == null) m_regTagColor = new System.Text.RegularExpressions.Regex(@"\[([a-fA-F0-9]{1,6})\]");
        if (m_regTagSize  == null) m_regTagSize  = new System.Text.RegularExpressions.Regex(@"\[(?:s(\d+))\]");
        #endif
        #endregion

        if (string.IsNullOrEmpty(input)) return string.Empty;

        var output = input;
        try
        {
            output = m_regEscape.Replace(input, e =>
            {
                var g = e.Groups;
                return g[0].Value == g[1].Value ? g[0].Value.Replace(g[1].Value, ((char)Parse<int>(g[2].Value)).ToString()) : g[0].Value;
            });

            output = m_regTagColor.Replace(output.Replace("\\n", "\n").Replace("[-]", "</color>"), "<color=#$1>");
            output = m_regTagSize.Replace(output.Replace("[s]", "</size>"), "<size=$1>");

            #if SHADOW_PACK
            if (!string.IsNullOrEmpty(output)) output = output.Replace(GeneralConfigInfo.sappName, Root.shadowAppName);
            #endif
        }
        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        catch (Exception e)
        {
            Logger.LogException("Util::ParseStringTags: input: {0}", input);
            Logger.LogException(e);
        }
        #else
        catch { }
        #endif
        return  output;
    }

    public static string PadStringLeft(string str, int count, char pad = ' ')
    {
        return str.PadLeft(str.Length + count, pad);
    }

    public static string PadStringRight(string str, int count, char pad = ' ')
    {
        return str.PadRight(str.Length + count, pad);
    }

    public static string GetString(int id)
    {
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null)
        {
            Logger.LogWarning("Could not find ConfigText {0}", id);
            return string.Empty;
        }

        return c[0];
    }

    public static string GetString(int id, object arg0)
    {
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null)
        {
            Logger.LogWarning("Could not find ConfigText {0}", id);
            return string.Empty;
        }

        return Format(c[0], arg0);
    }

    public static string GetString(int id, object arg0, object arg1)
    {
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null)
        {
            Logger.LogWarning("Could not find ConfigText {0}", id);
            return string.Empty;
        }

        return Format(c[0], arg0);
    }

    public static string GetString(int id, object arg0, object arg1, object arg2)
    {
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null)
        {
            Logger.LogWarning("Could not find ConfigText {0}", id);
            return string.Empty;
        }

        return Format(c[0], arg0, arg1, arg2);
    }

    public static string GetString(int id, params object[] args)
    {
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null)
        {
            Logger.LogWarning("Could not find ConfigText {0}", id);
            return string.Empty;
        }

        return Format(c[0], args);
    }

    public static string GetString(int id, int index)
    {
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null)
        {
            Logger.LogWarning("Could not find ConfigText {0}", id);
            return string.Empty;
        }

        return c[index];
    }

    public static string GetString(int id, int index, object arg0)
    {
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null)
        {
            Logger.LogWarning("Could not find ConfigText {0}", id);
            return string.Empty;
        }

        return Format(c[index], arg0);
    }

    public static string GetString(int id, int index, object arg0, object arg1)
    {
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null)
        {
            Logger.LogWarning("Could not find ConfigText {0}", id);
            return string.Empty;
        }

        return Format(c[index], arg0, arg1);
    }

    public static string GetString(int id, int index, object arg0, object arg1, object arg2)
    {
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null)
        {
            Logger.LogWarning("Could not find ConfigText {0}", id);
            return string.Empty;
        }

        return Format(c[index], arg0, arg1, arg2);
    }

    public static string GetString(int id, int index, params object[] args)
    {
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null)
        {
            Logger.LogWarning("Could not find ConfigText {0}", id);
            return string.Empty;
        }

        return Format(c[index], args);
    }

    #endregion

    #region Transform helpers

    public static Transform AddChild(Transform parent, Transform child, bool worldPositionStays = false)
    {
        if (!parent || !child) return child;
        child.SetParent(parent, worldPositionStays);

        if (!worldPositionStays)
        {
            child.localPosition = Vector3.zero;
            child.localEulerAngles = Vector3.zero;
            child.localScale = Vector3.one;
        }

        return child;
    }

    public static Transform AddChildStayLocal(Transform parent, Transform child)
    {
        if (!parent || !child) return child;

        Vector3 p = child.localPosition, e = child.localEulerAngles, s = child.localScale;

        child.SetParent(parent, false);

        child.localPosition    = p;
        child.localEulerAngles = e;
        child.localScale       = s;

        return child;
    }

    public static Transform AddChildAt(Transform parent, Transform child, int index, bool worldPositionStays = false)
    {
        if (!parent || !child) return child;

        index = Mathf.Clamp(index, 0, parent.childCount);

        var ll = new List<Transform>();
        for (int i = index, c = parent.childCount; i < c;)
        {
            var cc = parent.GetChild(i);
            cc.SetParent(null, true);
            ll.Add(cc);
            --c;
        }
        child.SetParent(parent, worldPositionStays);

        foreach (var cc in ll)
            cc.SetParent(parent, false);

        if (!worldPositionStays)
        {
            child.localPosition    = Vector3.zero;
            child.localEulerAngles = Vector3.zero;
            child.localScale       = Vector3.one;
        }

        return child;
    }

    public static Transform AddChild(Transform parent, Transform child, Vector3 pos, Vector3 scale, Vector3 rot)
    {
        if (!parent || !child) return child;
        child.SetParent(parent);
        child.localPosition = pos;
        child.localEulerAngles = rot;
        child.localScale = scale;

        return child;
    }

    public static Transform FindChild(Transform node, string name, bool includeInactive = true)
    {
        if (!node || string.IsNullOrEmpty(name)) return null;

        Transform child = null;
        for (int i = 0, c = node.childCount; i < c; ++i)
        {
            child = node.GetChild(i);
            if (string.Equals(child.name, name) && (includeInactive || child.gameObject.activeSelf)) break;
            else
            {
                child = FindChild(child, name, includeInactive);
                if (child) break;
            }
        }

        return child;
    }

    public static Transform FindChild(Transform node, Func<Transform, bool> match, bool includeInactive = true)
    {
        if (!node || match == null) return null;

        Transform child = null;
        for (int i = 0, c = node.childCount; i < c; ++i)
        {
            child = node.GetChild(i);
            if (!includeInactive && !child.gameObject.activeSelf) continue;
            else if (match(child)) return child;
            else
            {
                child = FindChild(child, match, includeInactive);
                if (child) break;
            }
        }

        return child;
    }

    /// <summary>
    /// Copy all children from src transform to dst transform
    /// </summary>
    /// <param name="src">Source transform</param>
    /// <param name="dst">Dest transform</param>
    /// <param name="positionType">0 = reset to default (worldPositionStays = false)  1 = use current (worldPositionStays = true)  2 = use current local</param>
    /// <param name="clearSrc">clear src transform ?</param>
    public static void CopyChilren(Transform src, Transform dst, int positionType = 0, bool clearSrc = false)
    {
        if (src == dst) return;

        for (int i = 0, c = src.childCount; i < c;)
        {
            var oc = src.GetChild(clearSrc ? 0 : i++);
            var nc = Object.Instantiate(oc);
            
            nc.name = oc.name;

            if (clearSrc) Object.Destroy(oc);

            if (positionType == 2)
            {
                var p = nc.localPosition;
                var r = nc.localEulerAngles;
                var s = nc.localScale;

                nc.SetParent(dst);

                nc.localPosition    = p;
                nc.localEulerAngles = r;
                nc.localScale       = s;
            }
            else nc.SetParent(dst, positionType == 1);
        }
    }

    /// <summary>
    /// Move all children from src transform to dst transform
    /// </summary>
    /// <param name="src">Source transform</param>
    /// <param name="dst">Dest transform</param>
    /// <param name="positionType">0 = reset to default (worldPositionStays = false)  1 = use current (worldPositionStays = true)  2 = use current local</param>
    public static void MoveChilren(Transform src, Transform dst, int positionType = 0)
    {
        if (src == dst) return;

        while (src.childCount > 0)
        {
            var c = src.GetChild(0);

            if (positionType == 2)
            {
                var p = c.localPosition;
                var r = c.localEulerAngles;
                var s = c.localScale;

                c.SetParent(dst);

                c.localPosition = p;
                c.localEulerAngles = r;
                c.localScale = s;
            }
            else c.SetParent(dst, positionType == 1);
        }
    }

    /// <summary>
    /// 查找指定节点的子节点，仅搜索当前节点的根目录，忽略深度大于 1 的子节点
    /// </summary>
    /// <param name="node"></param>
    /// <param name="name"></param>
    /// <param name="includeInactive"></param>
    /// <returns></returns>
    public static Transform FindSelfChild(Transform node, string name, bool includeInactive = true)
    {
        if (!node || string.IsNullOrEmpty(name)) return null;

        for (int i = 0, c = node.childCount; i < c; ++i)
        {
            var child = node.GetChild(i);
            if (string.Equals(child.name, name, StringComparison.Ordinal) && (includeInactive || child.gameObject.activeSelf)) return child;
        }

        return null;
    }

    /// <summary>
    /// 隐藏指定节点下的所有子节点
    /// </summary>
    /// <param name="node">根节点</param>
    public static void DisableAllChildren(Transform node)
    {
        if (!node) return;
        for (int i = 0, c = node.childCount; i < c; ++i)
            node.GetChild(i).gameObject.SetActive(false);
    }

    /// <summary>
    /// 隐藏指定节点下的所有子节点 允许指定前缀
    /// </summary>
    /// <param name="node">根节点</param>
    public static void DisableAllChildren(Transform node, char prefix, bool withPrefix = true)
    {
        if (!node) return;
        for (int i = 0, c = node.childCount; i < c; ++i)
        {
            var child = node.GetChild(i);
            var hasPrefix = child.name.Length > 0 && child.name[0] == prefix;
            if (hasPrefix == withPrefix) child.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 隐藏指定节点下的所有子节点，允许设置一个例外
    /// </summary>
    /// <param name="node">根节点</param>
    /// <param name="exclude">要排除的子节点</param>
    /// <param name="showExclude">如果排除的子节点不可见，是否强制其可见？</param>
    public static void DisableAllChildren(Transform node, string exclude, bool showExclude = true)
    {
        if (!node) return;
        for (int i = 0, c = node.childCount; i < c; ++i)
        {
            var child = node.GetChild(i);
            if (child.name != exclude) child.gameObject.SetActive(false);
            else if (showExclude) child.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 隐藏指定节点下的所有子节点，允许设置一组例外 并强制显示例外元素
    /// </summary>
    /// <param name="node"></param>
    /// <param name="include"></param>
    public static void DisableAllChildren(Transform node, params string[] excludes)
    {
        DisableAllChildren(node, excludes, true);
    }

    /// <summary>
    /// 隐藏指定节点下的所有子节点，允许设置一组例外
    /// </summary>
    /// <param name="node">根节点</param>
    /// <param name="excludes">要排除的子节点列表</param>
    /// <param name="showExclude">如果排除的子节点不可见，是否强制其可见？</param>
    public static void DisableAllChildren(Transform node, string[] excludes, bool showExclude)
    {
        if (excludes == null || excludes.Length < 1)
        {
            DisableAllChildren(node);
            return;
        }

        if (!node) return;
        for (int i = 0, c = node.childCount; i < c; ++i)
        {
            var child = node.GetChild(i);
            if (!excludes.Contains(child.name)) child.gameObject.SetActive(false);
            else if (showExclude) child.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 销毁指定节点下的所有子节点
    /// </summary>
    /// <param name="root"></param>
    public static void ClearChildren(Transform root, bool includeInactive = false)
    {
        if (!root) return;
        for (var i = root.childCount - 1; i > -1; --i)
        {
            var child = root.GetChild(i);

            if (!includeInactive && !child.gameObject.activeSelf) continue;
            Object.Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 销毁指定节点下的所有子节点，允许指定一组例外
    /// </summary>
    /// <param name="root"></param>
    /// <param name="excludes"></param>
    public static void ClearChildren(Transform root, params string[] excludes)
    {
        if (!root) return;
        for (var i = root.childCount - 1; i > -1; --i)
        {
            var child = root.GetChild(i);
            if (!child || excludes.Contains(child.name)) continue;

            Object.Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 销毁指定节点下的所有子节点，允许指定一组例外
    /// </summary>
    /// <param name="root"></param>
    /// <param name="excludes"></param>
    public static void ClearChildrenImmediate(Transform root, params string[] excludes)
    {
        if (!root) return;
        for (var i = root.childCount - 1; i > -1; --i)
        {
            var child = root.GetChild(i);
            if (!child || excludes.Contains(child.name)) continue;

            Object.DestroyImmediate(child.gameObject);
        }
    }


    #endregion

    #region Camera/Layer helpers

    /// <summary>
    /// 设置指定对象及其所有子节点的 layer
    /// </summary>
    /// <param name="obj">要设置的对象</param>
    /// <param name="layer">layer id</param>
    /// <param name="excludes">要排除的子节点名称</param>
    public static void SetLayer(GameObject obj, int layer, string[] excludes = null)
    {
        var t = obj?.transform;
        if (!obj) return;

        if (excludes == null) excludes = m_excludeLayerTarget;

        var m = obj.name;
        if (!excludes.Contains(m)) obj.layer = layer;
        
        for (int i = 0, c = t.childCount; i < c; ++i)
            SetLayer(t.GetChild(i).gameObject, layer, excludes);
    }

    private static void _SetLayer(GameObject obj, int layer, string[] excludes)
    {
        var m = obj.name;
        if (!excludes.Contains(m)) obj.layer = layer;

        var t = obj.transform;
        for (int i = 0, c = t.childCount; i < c; ++i)
            SetLayer(t.GetChild(i).gameObject, layer, excludes);
    }

    /// <summary>
    /// 设置目标特效的 SortingLayer 和 SortingOrder
    /// 参考 Layers 中的 Sorting Layer 常量
    /// </summary>
    /// <param name="effect">目标特效根节点对象</param>
    /// <param name="sortingLayer">要设置的 LayerName</param>
    /// <param name="sortingOrder">要设置的 OrderID</param>
    public static void SetEffectSortingLayer(GameObject effect, string sortingLayer, int sortingOrder = 1)
    {
        if (!effect) return;
        var renders = effect.GetComponentsInChildren<Renderer>(true);
        foreach (var render in renders)
        {
            render.sortingLayerName = sortingLayer;
            render.sortingOrder = sortingOrder;
        }
    }

    /// <summary>
    /// 设置指定对象及其所有子节点的 layer
    /// 如果某一个节点在排除列表中，该节点及其所有子节点都将被排除
    /// </summary>
    /// <param name="node">要设置的对象节点</param>
    /// <param name="layer">layer id</param>
    /// <param name="excludes">要排除的子节点名称</param>
    public static void SetLayer(Transform node, int layer, string[] excludes = null)
    {
        if (excludes == null) excludes = m_excludeLayerTarget;

        if (!node || excludes.Contains(node.name)) return;

        node.gameObject.layer = layer;

        for (int i = 0, count = node.childCount; i < count; ++i)
            _SetLayer(node.GetChild(i), layer, excludes);
    }

    private static void _SetLayer(Transform node, int layer, string[] excludes)
    {
        if (excludes.Contains(node.name)) return;

        node.gameObject.layer = layer;

        for (int i = 0, count = node.childCount; i < count; ++i)
            _SetLayer(node.GetChild(i), layer, excludes);
    }

    /// <summary>
    /// 从当前所有激活的场景相机中寻找指定名称的相机
    /// </summary>
    /// <param name="name">要寻找的相机名称</param>
    /// <returns></returns>
    public static Camera FindActivedCamera(string name)
    {
        var count = Camera.GetAllCameras(m_cachedCameras);
        Camera c = null;
        for (int i = 0; i < count; ++i)
        {
            var cc = m_cachedCameras[i];
            if (string.Compare(cc.name, name, StringComparison.Ordinal) == 0)
            {
                c = cc;
                break;
            }
        }

        Array.Clear(m_cachedCameras, 0, m_cachedCameras.Length);

        return c;
    }

    #endregion

    #region Time and number helpers

    /// <summary>
    /// 将给定的时间截断到毫秒级别
    /// </summary>
    /// <param name="time">原始时间</param>
    /// <param name="round">是否使用四舍五入</param>
    /// <returns></returns>
    public static float GetMillisecondsTime(float time, bool round = false)
    {
        var it = round ? Mathd.CeilToInt(time * 1000.0) : (int)(time * 1000.0);
        return it * 0.001f;
    }

    /// <summary>
    /// 将给定的时间截断到毫秒级别
    /// </summary>
    /// <param name="time">原始时间</param>
    /// <param name="round">是否使用四舍五入</param>
    /// <returns></returns>
    public static int GetMillisecondsTimeMS(float time, bool round = false)
    {
        var it = round ? Mathd.CeilToInt(time * 1000.0) : (int)(time * 1000.0);
        return it;
    }

    /// <summary>
    /// 获取当前服务器时间戳
    /// 如果当前客户端尚未连接，将返回本地时间戳
    /// </summary>
    /// <param name="local">是否使用客户端的本地时间</param>
    /// <param name="universal">是否使用 UTC 时间</param>
    /// <returns></returns>
    public static int GetTimeStamp(bool local = false, bool universal = false)
    {
        var s = Session.instance;
        return local || !s || !s.connected ? GetTimeStamp(universal ? DateTime.UtcNow : DateTime.Now) : universal ? s.serverTimeStamp : s.serverLocalTimeStamp;
    }

    /// <summary>
    /// 获取服务器本地时间
    /// </summary>
    /// <returns></returns>
    public static DateTime GetServerLocalTime()
    {
        return GetDateTime(GetTimeStamp(), false);
    }

    /// <summary>
    /// 获取指定日期的时间戳
    /// </summary>
    /// <param name="dateTime">日期</param>
    /// <returns></returns>
    public static int GetTimeStamp(DateTime dateTime)
    {
        return (int)((dateTime.Ticks - 621355968000000000) * 0.0000001);
    }

    /// <summary>
    /// 将时间戳转换为本地 DateTime 格式
    /// </summary>
    /// <param name="timeStamp">时间戳</param>
    /// <param name="universal">该时间戳是否是标准时间戳，如果是，转换之前会追加服务器当前的时区误差</param>
    /// <returns>时间</returns>
    public static DateTime GetDateTime(long timeStamp, bool universal = true)
    {
        var offset   = !universal || !Session.instance ? 0 : Session.instance.serverTimeZoneDiff;
        var dateTime = new DateTime(1970, 1, 1);
        dateTime = dateTime.AddSeconds(timeStamp + offset);
        return dateTime;
    }

    /// <summary>
    /// 截取服务器时间今天的秒数
    /// </summary>
    /// <returns></returns>
    public static int GetTimeOfDay()
    {
        return GetTimeStamp() % 86400;
    }

    /// <summary>
    /// 截取一个时间戳的时间部分
    /// </summary>
    /// <returns></returns>
    public static int GetTimeOfDay(int timeStamp)
    {
        return timeStamp % 86400;
    }

    /// <summary>
    /// 将给定的秒数转换为一个类似 01:14:35 的时间字符串 （时分秒）
    /// 注意 小时数为 0 时将只显示 分秒
    /// </summary>
    /// <param name="s">秒数</param>
    /// <param name="split">时间单位的分隔符</param>
    /// <returns></returns>
    public static string GetTimeFromSec(int s, string split = ":")
    {
        if (s <= 0) return "00:00";
        var h = s / 3600;  s %= 3600;
        var m = s / 60;    s %= 60;

        if (h < 1) return Format("{1:00}{0}{2:00}", split, m, s);
        return Format("{1:00}{0}{2:00}{0}{3:00}", split, h, m, s);
    }

    /// <summary>
    /// 将给定的秒数转换为一个带标签的时间字符串 最低显示到分钟 [string:50]
    /// 比如 02天02小时02分
    /// 注意 最高单位为 0 时不显示 （02小时32分）最高单位为分且为零则不显示
    /// </summary>
    /// <param name="s">秒数</param>
    /// <param name="second">是否精确到秒数</param>
    /// <returns></returns>
    public static string GetTimeMarkedFromSec(int s, bool second = true)
    {
        var text = ConfigText.time0.text;
        if (s <= 0) Format(text[3], "0");

        var d = s / 86400; s %= 86400;
        var h = s / 3600; s %= 3600;
        var m = s / 60; s %= 60;

        string sd = Format(text[0], d), sh = Format(text[1], h), sm = Format(text[2], m);

        if (!second) return Format(d < 1 ? h < 1 ? "{0}" : "{0}{1}" : "{0}{1}{2}", d < 1 ? h < 1 ? new object[] { sm } : new object[] { sh, sm } : new object[] { sd, sh, sm });

        var ss = Format(text[3], s);
        return Format(d < 1 ? h < 1 ? m < 1 ? "{0}" : "{0}{1}" : "{0}{1}{2}" : "{0}{1}{2}{3}", d < 1 ? h < 1 ? m < 1 ? new object[] { ss } : new object[] { sm, ss } : new object[] { sh, sm, ss } : new object[] { sd, sh, sm, ss });
    }

    /// <summary>
    /// 将给定的秒数转换成一个类似 00:15:32的时间字符串
    /// 默认显示秒数
    /// </summary>
    /// <param name="s">秒数</param>
    /// <param name="split">时间分隔符</param>
    /// <param name="second">是否显示秒数</param>
    /// <returns></returns>
    public static string GetTimeFromSec(int s, bool second = true, string split = ":")
    {
        if (s <= 0) return "00:00";
        var h = s / 3600; s %= 3600;
        var m = s / 60; s %= 60;

        return second ? Format("{1:00}{0}{2:00}{0}{3:00}", split, h, m, s) : Format("{1:00}{0}{2:00}", split, h, m);
    }

    /// <summary>
    /// 将一个秒数到另一个秒数 转换成 00:15:35-15:32:32的格式
    /// 默认不显示秒数
    /// </summary>
    /// <param name="timeFrom">起始秒数</param>
    /// <param name="timeTo">终止秒数</param>
    /// <param name="second">是否显示秒数</param>
    /// <returns></returns>
    public static string GetTimeFromSecDuration(int timeFrom, int timeTo, bool second = false)
    {
        return " " + GetTimeFromSec(timeFrom, second) + "-" + GetTimeFromSec(timeTo, second);
    }

    /// <summary>
    /// 将指定时间戳转换为 X天 X小时 X分钟 的格式 取最大单位 丢弃多余部分 [string:50]
    /// 该时间表示指定时间戳与服务器当前时间戳的差异
    /// 比如差异为 3700 秒转换为 1小时
    /// </summary>
    /// <param name="timeStamp">Unix 时间戳</param>
    /// <param name="second">是否精确到秒数</param>
    /// <returns></returns>
    public static string GetTimeStampDiff(int timeStamp, bool second = false)
    {
        return GetTimeDuration(timeStamp - GetTimeStamp(false, true), second);
    }

    /// <summary>
    /// 将指定秒数转换为 X天 X小时 X分钟 的格式 取最大单位 丢弃多余部分 [string:50]
    /// 比如 3700 秒转换为 1小时
    /// </summary>
    /// <param name="s">秒数</param>
    /// <param name="second">是否显示到秒数</param>
    /// <returns></returns>
    public static string GetTimeDuration(int s, bool second = false)
    {
        var text = ConfigText.time0.text;
        if (s >= 86400) return Format(text[0], s / 86400);
        if (s >= 3600)  return Format(text[1], s / 3600);

        if (s >= 60 || !second) return Format(text[2], s < 0 ? 0 : s / 60);

        return Format(text[3], s < 0 ? 0 : s);
    }

    /// <summary>
    /// 获取当前日期字符串 [string:51]
    /// 示例：1989年10月2日 12:25:35
    /// </summary>
    /// <param name="detal">包含小时和分钟？</param>
    /// <param name="second">包含秒数？</param>
    /// <param name="year">包含年份？</param>
    /// <returns></returns>
    public static string GetTimeDate(bool detail, bool second = false, bool year = false)
    {
        return GetTimeDate(GetTimeStamp(false, true), detail, second, year);
    }

    /// <summary>
    /// 根据指定时间戳获取一个日期字符串 [string:51]
    /// 示例：1989年10月2日 12:25:35
    /// </summary>
    /// <param name="timeStamp">时间戳</param>
    /// <param name="detail">是否包含小时和分钟</param>
    /// <param name="second">是否包含秒数（仅 detail 下有效）</param>
    /// <param name="year">是否包含年份</param>
    /// <returns></returns>
    public static string GetTimeDate(int timeStamp, bool detail = true, bool second = false, bool year = false)
    {
        var date = GetDateTime(timeStamp);
        var text = ConfigText.time1;

        var sM = Format(text[1], date.Month);
        var sd = Format(text[2], date.Day);

        var sl = year ? Format(text[0], date.Year) + sM + sd : sM + sd;

        if (!detail) return sl;

        var sh = Format(text[3], date.Hour);
        var sm = Format(text[4], date.Minute);

        var ss = second ? sh + ":" + sm + ":" + Format(text[5], date.Second) : sh + ":" + sm;

        return sl + " " + ss;
    }

    /// <summary>
    /// 给定两个时间戳，返回一个日期间隔字符串 [string:51]
    /// 示例：3月1日 - 4月1日
    /// </summary>
    /// <param name="timeFrom">起始时间戳</param>
    /// <param name="timeTo">结束时间戳</param>
    /// <param name="detail">是否包含小时和分钟</param>
    /// <param name="second">是否包含秒数（仅 detail 下有效）</param>
    /// <param name="year">是否包含年份</param>
    /// <returns></returns>
    public static string GetTimeDateDuration(int timeFrom, int timeTo, bool detail = true, bool second = false, bool year = false)
    {
        return GetTimeDate(timeFrom, detail, second, year) + " - " + GetTimeDate(timeTo, detail, second, year);
    }

    /// <summary>
    /// 将给定的数字转换为 XX万 或者 XX亿 的字符串 [string:52]
    /// </summary>
    /// <param name="num">要添加标签的数字</param>
    /// <param name="limit">大于此限制才添加标签</param>
    /// <param name="pr">保留的小数位</param>
    /// <returns></returns>
    public static string GetNumberTaged(long num, int limit = 10000, int pr = 1)
    {
        var text = ConfigText.number0;
        if (num < limit) return Format(text[0], num);
        if (num > 100000000 && num % 100000000 == 0 || num % 10000 == 0) pr = 0;
        return Format(num > 100000000 ? text[2] : text[1], (num / (num > 100000000 ? 100000000.0 : 10000.0)).ToString("F" + pr));
    }

    #endregion

    #region Color helpers

    /// <summary>
    /// Build Color from color with new alpha value
    /// </summary>
    /// <param name="color"></param>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public static Color BuildColor(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    /// <summary>
    /// Build Color from a RGBA format string
    /// "#FDA60150" or "0xFDA60150"
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static Color BuildColor(string str)
    {
        var val = Parse<int>(str);
        return BuildColor(val);
    }

    /// <summary>
    /// Build Color from a int value (RGBA)
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static Color BuildColor(int val)
    {
        return BuildColor(val >> 24 & 0xFF, val >> 16 & 0xFF, val >> 8 & 0xFF, val & 0xFF);
    }

    /// <summary>
    /// Build Color from a int value (RGBA)
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static Color BuildColor(uint val)
    {
        return BuildColor((int)(val >> 24 & 0xFF), (int)(val >> 16 & 0xFF), (int)(val >> 8 & 0xFF), (int)(val & 0xFF));
    }

    /// <summary>
    /// Build Color from r, g, b and a channel
    /// Each channel should between 0 - 255
    /// </summary>
    /// <param name="r">The red channel</param>
    /// <param name="g">The green channel</param>
    /// <param name="b">The blue channel</param>
    /// <param name="a">The alpha channel</param>
    /// <returns></returns>
    public static Color BuildColor(int r, int g, int b, int a = 255)
    {
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
    }

    /// <summary>
    /// Convert a RGBA format color to int value
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static int ColorToInt(Color color)
    {
        return (int)(color.a * 255) & 0xFF | ((int)(color.b * 255) & 0xFF) << 8 | ((int)(color.g * 255) & 0xFF) << 16 | ((int)(color.r * 255) & 0xFF) << 24;
    }

    #endregion

    #region Charge curreny helpers

    private static ConfigText m_chargeCurrencyName = null;
    private static ConfigText m_chargeCurrencyShortName = null;
    private static ConfigText m_chargeCurrencySymbol = null;

    /// <summary>
    /// 获取指定货比的名称
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetChargeCurrencyName(ChargeCurrencyTypes type)
    {
        return GetChargeCurrencyName((int)type);
    }

    /// <summary>
    /// 获取指定货币的缩写
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetChargeCurrencyShortName(ChargeCurrencyTypes type)
    {
        return GetChargeCurrencyShortName((int)type);
    }

    /// <summary>
    /// 获取指定货币的符号
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetChargeCurrencySymbol(ChargeCurrencyTypes type)
    {
        return GetChargeCurrencySymbol((int)type);
    }

    /// <summary>
    /// 获取指定货比的名称
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetChargeCurrencyName(int type)
    {
        if (m_chargeCurrencyName == null) m_chargeCurrencyName = ConfigManager.Get<ConfigText>(60);

        return m_chargeCurrencyName == null ? "" : m_chargeCurrencyName[type];
    }

    /// <summary>
    /// 获取指定货币的缩写
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetChargeCurrencyShortName(int type)
    {
        if (m_chargeCurrencyShortName == null) m_chargeCurrencyShortName = ConfigManager.Get<ConfigText>(61);

        return m_chargeCurrencyShortName == null ? "" : m_chargeCurrencyShortName[type];
    }

    /// <summary>
    /// 获取指定货币的符号
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetChargeCurrencySymbol(int type)
    {
        if (m_chargeCurrencySymbol == null) m_chargeCurrencySymbol = ConfigManager.Get<ConfigText>(62);

        return m_chargeCurrencySymbol == null ? "" : m_chargeCurrencySymbol[type];
    }


    #endregion

    #region Creature helpers

    /// <summary>
    /// 选择在 source 正面并且离 source 最近的目标
    /// </summary>
    /// <param name="source">参考对象</param>
    /// <param name="maxDistance">最大选择距离 默认不限制</param>
    /// <param name="sameCamp">0 = 任意阵营  1 = 不同阵营  其它 = 相同阵营</param>
    /// <returns></returns>
    public static Creature SelectFrontNearestTarget(Creature source, double maxDistance = 0, int sameCamp = 1)
    {
        Creature target = null;
        var minD = double.MaxValue;

        if (maxDistance <= 0) maxDistance = minD;

        ObjectManager.Foreach<Creature>(c =>
        {
            if (c == source || !source.TargetInFront(c)) return true;  // ignore self

            var d = (source.position_ - c.position_).magnitude;

            if (d > minD || d >= maxDistance) return true;   // distance check

            if (sameCamp != 0 && (sameCamp == 1 && c.SameCampWith(source) || sameCamp != 1 && !c.SameCampWith(source))) return true; // camp check

            minD = d;
            target = c;

            return true;
        });

        return target;
    }

    /// <summary>
    /// 选择在 source 背面并且离 source 最近的目标
    /// </summary>
    /// <param name="source">参考对象</param>
    /// <param name="maxDistance">最大选择距离 默认不限制</param>
    /// <param name="sameCamp">0 = 任意阵营  1 = 不同阵营  其它 = 相同阵营</param>
    /// <returns></returns>
    public static Creature SelectBackNearestTarget(Creature source, double maxDistance = 0, int sameCamp = 1)
    {
        Creature target = null;
        var minD = double.MaxValue;

        if (maxDistance <= 0) maxDistance = minD;

        ObjectManager.Foreach<Creature>(c =>
        {
            if (c == source || source.TargetInFront(c)) return true;  // ignore self

            var d = (source.position_ - c.position_).magnitude;

            if (d > minD || d >= maxDistance) return true;   // distance check

            if (sameCamp != 0 && (sameCamp == 1 && c.SameCampWith(source) || sameCamp != 1 && !c.SameCampWith(source))) return true; // camp check

            minD = d;
            target = c;

            return true;
        });

        return target;
    }

    /// <summary>
    /// 选择离 source 最近的目标
    /// </summary>
    /// <param name="source">参考对象</param>
    /// <param name="maxDistance">最大选择距离 默认不限制</param>
    /// <param name="frontFirst">优先选择前方的目标</param>
    /// <param name="sameCamp">0 = 任意阵营  1 = 不同阵营  其它 = 相同阵营</param>
    /// <returns></returns>
    public static Creature SelectNearestTarget(Creature source, double maxDistance = 0, bool frontFirst = false, int sameCamp = 1)
    {
        Creature target0 = null, target1 = null; // Back and Front target
        var minD0 = double.MaxValue;   // Back min distance
        var minD1 = minD0;             // Front min distance

        if (maxDistance <= 0) maxDistance = minD0;

        ObjectManager.Foreach<Creature>(c =>
        {
            if (c == source) return true;  // ignore self

            var d = (source.position_ - c.position_).magnitude;

            if (d >= maxDistance) return true;   // distance check

            var front = source.TargetInFront(c);
            if (front && d > minD1 || !front && d > minD0) return true;

            if (sameCamp != 0 && (sameCamp == 1 && c.SameCampWith(source) || sameCamp != 1 && !c.SameCampWith(source))) return true; // camp check

            if (front)
            {
                minD1 = d;
                target1 = c;
            }
            else
            {
                minD0 = d;
                target0 = c;
            }

            return true;
        });

        return target1 ? target1 : target0;
    }

    #endregion

    #region UI helpers

    #region Set UI text helper

    #region Formatter

    public static string Format(string s, object arg0)
    {
        try { s = string.Format(s, arg0); }
        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        catch (FormatException e)
        {
            Logger.LogException("Util::Format: Invalid string format. s: {0} args:[{1}]", s, arg0);
            Logger.LogException(e);
        }
        catch (Exception e)
        {
            Logger.LogException("Util::Format: s: {0} args:[{1}]", s, arg0);
            Logger.LogException(e);
        }
        #else
        catch { }
        #endif
        return s;
    }

    public static string Format(string s, object arg0, object arg1)
    {
        try { s = string.Format(s, arg0, arg1); }
        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        catch (FormatException e)
        {
            Logger.LogException("Util::Format: Invalid string format. s: {0} args:[{1},{2}]", s, arg0, arg1);
            Logger.LogException(e);
        }
        catch (Exception e)
        {
            Logger.LogException("Util::Format: s: {0} args:[{1},{2}]", s, arg0, arg1);
            Logger.LogException(e);
        }
        #else
        catch { }
        #endif
        return s;
    }

    public static string Format(string s, object arg0, object arg1, object arg2)
    {
        try { s = string.Format(s, arg0, arg1, arg2); }
        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        catch (FormatException e)
        {
            Logger.LogException("Util::Format: Invalid string format. s: {0} args:[{1},{2},{3}]", s, arg0, arg1, arg2);
            Logger.LogException(e);
        }
        catch (Exception e)
        {
            Logger.LogException("Util::Format: s: {0} args:[{1},{2},{3}]", s, arg0, arg1, arg2);
            Logger.LogException(e);
        }
        #else
        catch { }
        #endif
        return s;
    }

    public static string Format(string s, params object[] args)
    {
        try { s = string.Format(s, args); }
        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        catch (FormatException e)
        {
            var ss = args.Length < 1 ? string.Empty : " args:[";
            for (int i = 0, c = args.Length; i < c; ++i) ss += (args[i]?.ToString() ?? "null") + (i == c - 1 ? "]" : ",");

            Logger.LogException("Util::Format: Invalid string format. s: {0}{1}", s, ss);
            Logger.LogException(e);
        }
        catch (Exception e)
        {
            var ss = args.Length < 1 ? string.Empty : " args:[";
            for (int i = 0, c = args.Length; i < c; ++i) ss += (args[i]?.ToString() ?? "null") + (i == c - 1 ? "]" : ",");

            Logger.LogException("Util::Format: s: {0}{1}", s, ss);
            Logger.LogException(e);
        }
        #else
        catch { }
        #endif
        return s;
    }

    #endregion

    public static void SetText(GameObject obj, string s)
    {
        var t = obj?.GetComponent<Text>();
        if (!t) return;

        t.supportRichText = s != null && (s.Contains("<color=") || s.Contains("<size="));
        t.text = s;
    }

    public static void SetText(GameObject obj, string s, object arg0)
    {
        var t = obj?.GetComponent<Text>();
        if (!t) return;

        s = Format(s, arg0);
        t.supportRichText = s != null && (s.Contains("<color=") || s.Contains("<size="));
        t.text = s;
    }

    public static void SetText(GameObject obj, string s, object arg0, object arg1)
    {
        var t = obj?.GetComponent<Text>();
        if (!t) return;

        s = Format(s, arg0, arg1);
        t.supportRichText = s != null && (s.Contains("<color=") || s.Contains("<size="));
        t.text = s;
    }

    public static void SetText(GameObject obj, string s, object arg0, object arg1, object arg2)
    {
        var t = obj?.GetComponent<Text>();
        if (!t) return;

        s = Format(s, arg0, arg1, arg2);
        t.supportRichText = s != null && (s.Contains("<color=") || s.Contains("<size="));
        t.text = s;
    }

    public static void SetText(GameObject obj, string s, params object[] args)
    {
        var t = obj?.GetComponent<Text>();
        if (!t) return;

        s = Format(s, args);
        t.supportRichText = s != null && (s.Contains("<color=") || s.Contains("<size="));
        t.text = s;
    }

    public static void SetText(GameObject obj, int id)
    {
        var t = obj?.GetComponent<Text>();
        if (!t) return;

        var s = "";
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null) Logger.LogWarning("Could not find ConfigText {0}", id);
        else s = c[0];

        t.supportRichText = s.Contains("<color=") || s.Contains("<size=");
        t.text = s;
    }

    public static void SetText(GameObject obj, int id, object arg0)
    {
        var t = obj?.GetComponent<Text>();
        if (!t) return;

        var s = "";
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null) Logger.LogWarning("Could not find ConfigText {0}", id);
        else s = c[0];

        s = Format(s, arg0);
        t.supportRichText = s.Contains("<color=") || s.Contains("<size=");
        t.text = s;
    }

    public static void SetText(GameObject obj, int id, params object[] args)
    {
        var t = obj?.GetComponent<Text>();
        if (!t) return;

        var s = "";
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null) Logger.LogWarning("Could not find ConfigText {0}", id);
        else s = c[0];

        s = Format(s, args);
        t.supportRichText = s.Contains("<color=") || s.Contains("<size=");
        t.text = s;
    }

    public static void SetText(GameObject obj, int id, int index)
    {
        var t = obj?.GetComponent<Text>();
        if (!t) return;

        var s = "";
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null) Logger.LogWarning("Could not find ConfigText {0}", id);
        else s = c[index];

        t.supportRichText = s.Contains("<color=") || s.Contains("<size=");
        t.text = s;
    }

    public static void SetText(GameObject obj, int id, int index, object arg0)
    {
        var t = obj?.GetComponent<Text>();
        if (!t) return;

        var s = "";
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null) Logger.LogWarning("Could not find ConfigText {0}", id);
        else s = c[index];

        s = Format(s, arg0);
        t.supportRichText = s.Contains("<color=") || s.Contains("<size=");
        t.text = s;
    }

    public static void SetText(GameObject obj, int id, int index, object arg0, object arg1)
    {
        var t = obj?.GetComponent<Text>();
        if (!t) return;

        var s = "";
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null) Logger.LogWarning("Could not find ConfigText {0}", id);
        else s = c[index];

        s = Format(s, arg0, arg1);
        t.supportRichText = s.Contains("<color=") || s.Contains("<size=");
        t.text = s;
    }

    public static void SetText(GameObject obj, int id, int index, object arg0, object arg1, object arg2)
    {
        var t = obj?.GetComponent<Text>();
        if (!t) return;

        var s = "";
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null) Logger.LogWarning("Could not find ConfigText {0}", id);
        else s = c[index];

        s = Format(s, arg0, arg1, arg2);
        t.supportRichText = s.Contains("<color=") || s.Contains("<size=");
        t.text = s;
    }

    public static void SetText(GameObject obj, int id, int index, params object[] args)
    {
        var t = obj?.GetComponent<Text>();
        if (!t) return;

        var s = "";
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null) Logger.LogWarning("Could not find ConfigText {0}", id);
        else s = c[index];

        s = Format(s, args);
        t.supportRichText = s.Contains("<color=") || s.Contains("<size=");
        t.text = s;
    }

    public static void SetText(Text t, string s)
    {
        if (!t) return;
        
        t.supportRichText = s != null && s.Contains("<color=") || s.Contains("<size=");
        t.text = s;
    }

    public static void SetText(Text t, string s, object arg0)
    {
        if (!t) return;

        s = Format(s, arg0);
        t.supportRichText = s != null && (s.Contains("<color=") || s.Contains("<size="));
        t.text = s;
    }

    public static void SetText(Text t, string s, object arg0, object arg1)
    {
        if (!t) return;

        s = Format(s, arg0, arg1);
        t.supportRichText = s != null && (s.Contains("<color=") || s.Contains("<size="));
        t.text = s;
    }

    public static void SetText(Text t, string s, object arg0, object arg1, object arg2)
    {
        if (!t) return;

        s = Format(s, arg0, arg1, arg2);
        t.supportRichText = s != null && (s.Contains("<color=") || s.Contains("<size="));
        t.text = s;
    }

    public static void SetText(Text t, string s, params object[] args)
    {
        if (!t) return;

        s = Format(s, args);
        t.supportRichText = s != null && (s.Contains("<color=") || s.Contains("<size="));
        t.text = s;
    }

    public static void SetText(Text t, int id)
    {
        if (!t) return;

        var s = "";
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null) Logger.LogWarning("Could not find ConfigText {0}", id);
        else s = c[0];

        t.supportRichText = s.Contains("<color=") || s.Contains("<size=");
        t.text = s;
    }

    public static void SetText(Text t, int id, object arg0)
    {
        if (!t) return;

        var s = "";
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null) Logger.LogWarning("Could not find ConfigText {0}", id);
        else s = c[0];

        s = Format(s, arg0);
        t.supportRichText = s.Contains("<color=") || s.Contains("<size=");
        t.text = s;
    }

    public static void SetText(Text t, int id, params object[] args)
    {
        if (!t) return;

        var s = "";
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null) Logger.LogWarning("Could not find ConfigText {0}", id);
        else s = c[0];

        s = Format(s, args);
        t.supportRichText = s.Contains("<color=") || s.Contains("<size=");
        t.text = s;
    }

    public static void SetText(Text t, int id, int index)
    {
        if (!t) return;

        var s = "";
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null) Logger.LogWarning("Could not find ConfigText {0}", id);
        else s = c[index];

        t.supportRichText = s.Contains("<color=") || s.Contains("<size=");
        t.text = s;
    }

    public static void SetText(Text t, int id, int index, object arg0)
    {
        if (!t) return;

        var s = "";
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null) Logger.LogWarning("Could not find ConfigText {0}", id);
        else s = c[index];

        s = Format(s, arg0);
        t.supportRichText = s.Contains("<color=") || s.Contains("<size=");
        t.text = s;
    }

    public static void SetText(Text t, int id, int index, object arg0, object arg1)
    {
        if (!t) return;

        var s = "";
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null) Logger.LogWarning("Could not find ConfigText {0}", id);
        else s = c[index];

        s = Format(s, arg0, arg1);
        t.supportRichText = s.Contains("<color=") || s.Contains("<size=");
        t.text = s;
    }

    public static void SetText(Text t, int id, int index, params object[] args)
    {
        if (!t) return;

        var s = "";
        var c = ConfigManager.Get<ConfigText>(id);
        if (c == null) Logger.LogWarning("Could not find ConfigText {0}", id);
        else s = c[index];

        s = Format(s, args);
        t.supportRichText = s.Contains("<color=") || s.Contains("<size=");
        t.text = s;
    }

    #endregion

    #region Set item info

    private static ConfigText m_itemInfoText = null;
    private static int[] runeDebris = new int[] { 21, 22, 23, 24, 25, 26 };

    /// <summary>
    /// 设置道具信息
    /// 不现实等级 数量以及星星
    /// </summary>
    /// <param name="o"></param>
    /// <param name="item"></param>
    public static void SetItemInfoSimple(Transform t, PropItemInfo itemInfo)
    {
        if (!t) return;
        SetItemInfoSimple(t.gameObject, itemInfo);
    }

    /// <summary>
    /// 设置道具信息
    /// 不现实等级 数量以及星星
    /// </summary>
    /// <param name="o"></param>
    /// <param name="item"></param>
    public static void SetItemInfoSimple(GameObject o, PropItemInfo itemInfo)
    {
        SetItemInfo(o, itemInfo, 0, 0, false);
    }

    /// <summary>
    /// 设置道具信息
    /// </summary>
    /// <param name="o"></param>
    /// <param name="item"></param>
    public static void SetItemInfo(Transform t, PropItemInfo itemInfo, int level = 0, int count = 0, bool showStar = true, int star = 1)
    {
        if (!t || null == itemInfo) return;
        SetItemInfo(t.gameObject, itemInfo, level, count, showStar, star);
    }

    public static void SetPetInfo(Transform t, PetInfo petInfo)
    {
        if (petInfo == null || !t) return;
        SetItemInfo(t, petInfo.CPetInfo);
        //头像不要使用道具头像。要使用星级配置的头像
        AtlasHelper.SetPetIcon(t.GetComponent<Image>("icon"), petInfo.UpGradeInfo.icon);
    }

    public static void SetPetSimpleInfo(Transform t, PetInfo petInfo)
    {
        if (petInfo == null || !t || petInfo.CPetInfo == null) return;
        var q = t?.Find("quality");
        DisableAllChildren(q, petInfo.CPetInfo.quality.ToString());
        //头像不要使用道具头像。要使用星级配置的头像
        AtlasHelper.SetPetIcon(t.GetComponent<Image>("icon"), petInfo.UpGradeInfo.icon);
    }

    /// <summary>
    /// 设置道具信息
    /// </summary>
    /// <param name="o"></param>
    /// <param name="item"></param>
    public static void SetItemInfo(GameObject o, PropItemInfo itemInfo, int level = 0, int count = 0, bool showStar = true, int star = 1)
    {
        if (!o) return;

        if (!itemInfo) itemInfo = PropItemInfo.empty;

        if (!m_itemInfoText)
        {
            m_itemInfoText = ConfigManager.Get<ConfigText>(32);
            if (!m_itemInfoText) m_itemInfoText = new ConfigText() { ID = 32, text = new string[] { "×{0}", "{0}", "{0} ×{1}", "+{0}", "Lv.{0}" } };
        }

        //set guide restrain data
        BaseRestrain.SetRestrainData(o, itemInfo, level, star);

        var isRune = itemInfo.itemType == PropType.Rune;
        var ss = star.ToString();

        Transform t = o.transform, q = t?.Find("quality"), r = t?.Find("qualityRune"), s = t?.Find("stars"), n = t?.Find("intentify");

        var ct = o.GetComponent<Text>("numberdi/count");
        SetText(ct, m_itemInfoText[0], count);

        if (ct || count < 1) SetText(o.GetComponent<Text>("name"), m_itemInfoText[1], itemInfo.itemName);
        else SetText(o.GetComponent<Text>("name"), m_itemInfoText[2], itemInfo.itemName, count);

        q?.gameObject.SetActive(true);
        DisableAllChildren(q, isRune ? ss : itemInfo.quality.ToString());

        var isRuneDebris = itemInfo.itemType == PropType.Debris && runeDebris.Contains(itemInfo.subType);
        var _quality = isRune ? star : itemInfo.quality;
        var _color = Module_Rune.instance.GetCurQualityColor(GeneralConfigInfo.defaultConfig.qualitys, _quality);

        r.SafeSetActive(isRune || isRuneDebris);
        if ((isRuneDebris || isRune) && r)
        {
            var romaBg = r.Find("1")?.GetComponent<Image>();
            if (romaBg) romaBg.color = _color.SetAlpha(romaBg.color.a);

            var roma = r.Find("txt")?.GetComponent<Text>();
            var index = isRune ? itemInfo.subType - 1 : itemInfo.subType - 21;
            index = index < 0 ? 0 : index;
            SetText(roma, GetString(10103, index));
            var textColor = Module_Rune.instance.GetCurQualityColor(GeneralConfigInfo.defaultConfig.textQualitys, _quality);
            if (roma) roma.color = textColor.SetAlpha(roma.color.a);
        }

        if (n)
        {
            n.SafeSetActive(level > 0);

            var img = n.GetComponent<Image>();
            if (img) img.color = _color.SetAlpha(img.color.a);

            var txt = n.Find("mark")?.GetComponent<Text>();
            SetText(txt, isRune ? m_itemInfoText[4] : m_itemInfoText[3], level);

            var outline = txt?.GetComponent<Outline>();
            if (outline) outline.effectColor = _color.SetAlpha(outline.effectColor.a);
        }

        AtlasHelper.SetItemIcon(t.Find("icon"), itemInfo);

        var id = t.GetComponent<Text>("id");
        if (id)
        {
            id.text = itemInfo.ID.ToString();
            id.gameObject.SetActive(false);
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            id.gameObject.SetActive(true);
            #endif
        }

        if (s)
        {
            if (!showStar) s.gameObject.SetActive(false);
            else
            {
                var c = s.Find("count");
                s.gameObject.SetActive(c);
                if (c)
                {
                    var sl = isRune ? star + 1 : itemInfo.quality + 1;

                    for (int i = 1, l = c.childCount; i <= l; ++i)
                    {
                        var sn = c.Find(i.ToString());
                        if (sn) sn.gameObject.SetActive(i < sl);
                    }
                }
            }
        }
    }

    #endregion

    #region set equip type icon
    public static void SetEquipTypeIcon(List<Image> icons,PropType type,int subType)
    {
        int index = 0;
        if (type == PropType.FashionCloth) index = 5;
        else
        {
            switch ((WeaponSubType)subType)
            {
                case WeaponSubType.LongSword: index = 0; break;
                case WeaponSubType.Katana: index = 1; break;
                case WeaponSubType.GiantAxe: index = 2; break;
                case WeaponSubType.Gloves: index = 3; break;
                case WeaponSubType.Gun: index = 4; break;
            }
        }

        for (int i = 0; i < icons.Count; i++)
        {
            icons[i]?.gameObject.SetActive(i == index);
        }
    }
    #endregion

    #region set exp bar

    public static void SetExpBar(List<Image> m_expBars, int level, int left,int right)
    {
        if (m_expBars == null) return;

        foreach (var item in m_expBars)
        {
            item?.gameObject?.SetActive(false);
        }

        int index = (level - 2) % m_expBars.Count;
        if (index >= 0)
        {
            m_expBars[index].gameObject.SetActive(true);
            m_expBars[index].rectTransform.SetAsLastSibling();
            m_expBars[index].fillAmount = 1;
        }

        index = (level - 1) % m_expBars.Count;
        if (index < 0) index = 0;
        m_expBars[index].gameObject.SetActive(true);
        m_expBars[index].rectTransform.SetAsLastSibling();
        m_expBars[index].fillAmount = left * 1.0f / right;
    }

    public static void SetExpBar(Transform expParent,int level, int left, int right)
    {
        List<Image> images = GetExpBars(expParent);
        SetExpBar(images, level,left, right);
    }

    public static List<Image> GetExpBars(Transform expParent)
    {
        if (!expParent) return null;

        string[] m_barNames = new string[] { "progressbar_filled_01", "progressbar_filled_02", "progressbar_filled_03", "progressbar_filled_04" };
        var l = new List<Image>();
        foreach (var item in m_barNames)
        {
            l.Add(expParent.GetComponent<Image>(item));
        }
        return l;
    }


    #endregion

    #region Math helpers

    #region Quaternion helpers

    /// <summary>
    /// This is a slerp that mimics a camera operator's movement in that it chooses a path that avoids the lower hemisphere, as defined by the up param
    /// </summary>
    /// <param name="from">First euler angles</param>
    /// <param name="to">Second euler angles</param>
    /// <param name="t">Interpolation amoun t</param>
    public static Vector3 Lerp(Vector3 from, Vector3 to, float t)
    {
        return new Vector3(from.x + Mathf.Lerp(0, Mathf.DeltaAngle(from.x, to.x), t), from.y + Mathf.Lerp(0, Mathf.DeltaAngle(from.y, to.y), t), from.z + Mathf.Lerp(0, Mathf.DeltaAngle(from.z, to.z), t));
    }

    /// <summary>
    /// This is a slerp that mimics a camera operator's movement in that it chooses a path that avoids the lower hemisphere, as defined by the up param
    /// </summary>
    /// <param name="from">First direction</param>
    /// <param name="to">Second direction</param>
    /// <param name="t">Interpolation amoun t</param>
    public static Vector3 Lerp(Quaternion from, Quaternion to, float t)
    {
        return Lerp(from.eulerAngles, to.eulerAngles, t);
    }

    #endregion

    #endregion

    #endregion

    #region Charge

    public static int DailyBuyTimes(this PChargeItem rItem)
    {
        var key = $"{Module_Player.instance.id_}{rItem.productId }-hasBuyCount";
        if (PlayerPrefs.HasKey(key))
            return PlayerPrefs.GetInt(key) + rItem.hasBuyCount;
        return rItem.hasBuyCount;
    }

    public static int TotalBuyTimes(this PChargeItem rItem)
    {
        var key = $"{Module_Player.instance.id_}{rItem.productId }-hasTotalBuyCount";
        if (PlayerPrefs.HasKey(key))
            return PlayerPrefs.GetInt(key) + rItem.hasTotalBuyCount;
        return rItem.hasTotalBuyCount;
    }

    #endregion

    #region Misc

    /// <summary>
    /// 处理相同类型的数据加法，所有基础数据都会加。非基础数据保持a不变
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static object SameTypeObjectAdd(object a, object b)
    {
        if (a == null && b == null)
            return null;
        var type = a?.GetType() ?? b?.GetType();

        if (a != null && b != null)
        {
            var aType = a.GetType();
            var bType = b.GetType();
            if (aType != bType)
                return a;
        }
        
        var tCode = Type.GetTypeCode(type);
        if (tCode == TypeCode.Byte)
            return ((byte?) a ?? default(byte)) + ((byte?) b ?? default(byte));
        if(tCode == TypeCode.Double)
            return ((double?)a ?? default(double)) + ((double?)b ?? default(double));
        if (tCode == TypeCode.Char)
            return ((char?)a ?? default(char)) + ((char?)b ?? default(char));
        if (tCode == TypeCode.Int16)
            return ((short?)a ?? default(short)) + ((short?)b ?? default(short));
        if (tCode == TypeCode.Int32)
            return ((int?)a ?? default(int)) + ((int?)b ?? default(int));
        if (tCode == TypeCode.Int64)
            return ((long?)a ?? default(long)) + ((long?)b ?? default(long));
        if (tCode == TypeCode.UInt16)
            return ((ushort?)a ?? default(ushort)) + ((ushort?)b ?? default(ushort));
        if (tCode == TypeCode.UInt32)
            return ((uint?)a ?? default(uint)) + ((uint?)b ?? default(uint));
        if (tCode == TypeCode.UInt64)
            return ((ulong?)a ?? default(ulong)) + ((ulong?)b ?? default(ulong));
        if (tCode == TypeCode.Single)
            return ((float?)a ?? default(float)) + ((float?)b ?? default(float));
        if (typeof(System.Collections.IList).IsAssignableFrom(type))
        {
            if (type.IsArray)
            {
                var aArr = a as Array;
                var bArr = b as Array;
                var aLen = aArr?.Length ?? 0;
                var bLen = bArr?.Length ?? 0;
                System.Reflection.FieldInfo specialField = null;
                if(aLen > 0)
                    specialField = aArr.GetValue(0)?.GetType().GetField("startFrame");
                HashSet<object> common = new HashSet<object>();
                HashSet<object> bFrames = new HashSet<object>();
                
                if(specialField != null)
                {
                    for (var i = 0; i < aLen; i++)
                        common.Add(specialField.GetValue(aArr.GetValue(i)));

                    for (var i = 0; i < bLen; i++)
                        bFrames.Add(specialField.GetValue(bArr.GetValue(i)));
                }
                common.IntersectWith(bFrames);

                var arr = Array.CreateInstance(type.GetElementType(), aLen + bLen - common.Count);

                var count = 0;
                for(var i = 0; i < Math.Max(aLen, bLen); i++)
                {
                    if(i < aLen && count < arr.Length)
                    {
                        if (specialField == null || !common.Contains(specialField.GetValue(aArr.GetValue(i))))
                            arr.SetValue(aArr?.GetValue(i), count++);
                    }
                    if (i < bLen && count < arr.Length)
                        arr.SetValue(bArr?.GetValue(i), count++);
                }

                return arr;
            }
            else
            {
                var aList = a as System.Collections.IList;
                var bList = b as System.Collections.IList;
                var list = Activator.CreateInstance(type) as System.Collections.IList;
                if (aList != null && list != null)
                {
                    foreach (var item in aList)
                        list.Add(item);
                }
                if (bList != null && list != null)
                {
                    foreach (var item in bList)
                        list.Add(item);
                }
                return list;
            }
        }
        try
        {
            if (tCode == TypeCode.Object)
            {
                var fields = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                var item = Activator.CreateInstance(type);
                foreach (var field in fields)
                {
                    field.SetValue(item, SameTypeObjectAdd(field.GetValue(a), field.GetValue(b)));
                }
                return item;
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
        }
        return a;
    }

    public static float RemapValue(float rValue, float rStart, float rEnd, float rNewStart, float rNewEnd )
    {
        var percent = (rValue - rStart)/(rEnd - rStart);
        return rNewStart + (rNewEnd - rNewStart)*percent;
    }

    #endregion

    public static string GetHierarchy(GameObject go)
    {
        return GetHierarchy(go?.transform);
    }

    public static string GetHierarchy(Transform t)
    {
        if (!t) return string.Empty;
        var sb = new StringBuilder();
        sb.Append(t.name);
        while (t.parent != null)
        {
            sb.Insert(0, t.parent.name + "/");
            t = t.parent;
        }
        return sb.ToString();
    }
}
