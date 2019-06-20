/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Auto protocol file generator for custom protocol format.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-04-05
 * 
 ***************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public interface ILanguageGenerator
{
    string[] fileExtensions { get; }

    string GetFileName(int group, string name);
    string ParseType(string type);
    string[] ParseDesc(string desc, ProtocolGenerator g);
    string[] Generate(PacketDef pkt, ProtocolGenerator g);
    void OnComplete(string[] code, PacketDef[] gps, ProtocolGenerator g);
    void OnAllComplete(ProtocolGenerator[] groups, PacketDef[] packets, string tar);
}

public struct FieldDef
{
    /// <summary>
    /// Global keywords
    /// </summary>
    public static readonly string[] keywords =
    {
        "friend",
        "signed",
        "unsigned"
    };

    public string type;      // field type definition  e.g, uint32[], string, single...
    public string typeName;  // field type name        e.g, uint32, string
    public string name;      // field name
    public string comment;   // field comment
    public bool   custom;    // field is custom type
    public bool   array;     // field is array type
    public int    group;     // field defines in which group ?
    public string groupName; // field group name

    public FieldDef(string _type, string _name, string _comment, bool _custom, int _group, string _groupName)
    {
        type        = _type;
        name        = _name + (Array.IndexOf(keywords, _name) > -1 ? "_" : "");
        comment     = _comment;
        array       = type.IndexOf("[]") > -1;
        custom      = _custom;
        typeName    = array ? type.Substring(0, type.Length - 2) : type;
        group       = _group;
        groupName   = _groupName;
    }
}

public class PacketDef
{
    public string     file;
    public string     name;
    public int        group;
    public string     groupName;
    public int        id;
    public string     router;
    public bool       earlySend;
    public string     comment;
    public string     content;
    public int[]      fl;
    public FieldDef[] fields;

    public PacketDef(string _file, string _name, int _group, string _groupName, int _id, string _router, bool _earlySend, string _comment, string _content, FieldDef[] _fields)
    {
        file      = _file;
        name      = _name;
        group     = _group;
        groupName = _groupName;
        id        = _id;
        router    = _router;
        earlySend = _earlySend;
        comment   = _comment;
        content   = _content;
        fields    = _fields;
    }
}

public class ProtocolGenerator
{
    #region Static funtions

    public static readonly string fileExtension = "pdef";
    public static readonly string[] primitiveTypes = new string[] { "bool", "int8", "uint8", "int16", "uint16", "int32", "uint32", "int64", "uint64", "single", "double", "string" };

    public static readonly Hashtable languages = new Hashtable
    {
        { "C#",     typeof(CSGenerator) },
        { "C++",    typeof(CPPGenerator) },
        { "Erlang", typeof(ErlGenerator) },
        { "Go",     null },
        { "Java",   null },
    };

    public static bool LanguageImplemented(string language)
    {
        if (language == null || language.Length < 2) return false;
        language = language.Substring(0, 1).ToUpper() + language.Substring(1).ToLower();
        return languages.ContainsKey(language) && languages[language] != null;
    }

    public static Type GetGeneratorType(string language)
    {
        if (language == null || language.Length < 2) return null;
        language = language.Substring(0, 1).ToUpper() + language.Substring(1).ToLower();
        return languages.ContainsKey(language) ? (Type)languages[language] : null;
    }

    public static ILanguageGenerator GetGenerator(string language)
    {
        if (language == null || language.Length < 2) return null;
        language = language.Substring(0, 1).ToUpper() + language.Substring(1).ToLower();
        var type = GetGeneratorType(language);
        if (type == null || !typeof(ILanguageGenerator).IsAssignableFrom(type)) return null;
        var l = Activator.CreateInstance(type) as ILanguageGenerator;
        return l;
    }

    public static T GetGenerator<T>() where T : ILanguageGenerator
    {
        return Activator.CreateInstance<T>();
    }

    #endregion

    public int group { get { return m_group; } }
    public string name { get { return m_name; } }
    public string data { get { return m_data; } set { m_data = value; ParseInfo(); ParsePackets(); } }
    public string cts { get { return m_cts; } set { m_cts = value; } }
    public string file { get { return m_file; } set { m_file = value; } }
    public List<PacketDef> packets { get { return m_pkts; } }

    private string m_file    = "";
    private string m_data    = "";
    private string m_cts     = "";
    private int    m_group   = 0;
    private string m_name    = "";
    private string m_desc    = "";

    private List<PacketDef> m_pkts  = new List<PacketDef>();
    private List<PacketDef> m_gpkts = null;

    public ProtocolGenerator(string _file, string _data = "", List<PacketDef> gps = null)
    {
        m_data  = _data;
        m_file  = _file;
        m_gpkts = gps;

        if (!string.IsNullOrEmpty(m_data))
        {
            ParseInfo();
            ParsePackets();
        }
    }

    public string[] Generate(ILanguageGenerator language)
    {
        CheckPrimitives(language);

        var code = language.ParseDesc(m_desc, this);
        foreach (var pkt in m_pkts)
        {
            var gc = language.Generate(pkt, this);
            for (var i = 0; i < code.Length; ++i) code[i] += string.IsNullOrEmpty(gc[i]) ? "" : gc[i] + "\r\n";
        }

        if (m_pkts.Count > 0)
            for (var i = 0; i < code.Length; ++i) code[i] = code[i].Length > 2 ? code[i].Substring(0, code[i].Length - 2) : code[i];

        return code;
    }

    public PacketDef GetPacket(int id, string name)
    {
        var pkt = m_pkts.Find(p => p.id == id || p.name == name);
        if (pkt == null && m_gpkts != null) pkt = m_gpkts.Find(p => p.name == name || p.id == id);
        return pkt;
    }

    private void CheckPrimitives(ILanguageGenerator language)
    {
        foreach (var t in primitiveTypes)
        {
            if (string.IsNullOrEmpty(language.ParseType(t)))
                throw new Exception(string.Format("Generator {0} does not implement full type support, missing type: {1}", language.GetType().Name, t));
        }
    }

    private void ParseInfo()
    {
        m_name  = "";
        m_group = 0;
        m_desc  = "";

        var r = new Regex(@"#\s*Begin\s+(\w+)((?:.|\s)+)#\s*End\s+\1", RegexOptions.Singleline);
        var match = r.Match(m_data);
        while (match.Success)
        {
            var block   = match.Groups[1].Value.ToUpper();
            var content = match.Groups[2].Value;

            switch (block)
            {
                case "DEFINE":
                {
                    var c = Regex.Match(content, @"Group\s*:\s*(\d+)");
                    if (!c.Success) throw new Exception("Could not find Group definition!");
                    m_group = Util.Parse<int>(c.Groups[1].Value);
                    if (m_group < 0 || m_group > byte.MaxValue) throw new Exception(string.Format("Invalid group, group must in [{0}, {1}], current: {2}", 0, byte.MaxValue, m_group));
                    c = Regex.Match(content, @"Name\s*:\s*(\w+)");
                    if (!c.Success || string.IsNullOrEmpty(c.Groups[1].Value)) throw new Exception("Could not find Name definition or name is null!");
                    m_name = Util.FirstUpper(c.Groups[1].Value);
                    break;
                }
                case "DESC": m_desc = Regex.Replace(content = Regex.Replace(Regex.Replace(content, "\r\n", "\n").TrimStart('\n').TrimEnd('\n'), "\n{3,}", "\n\n"), @"\n\s*\n\s*\n", "\n"); break;
                default: break;
            }

            match = match.NextMatch();
        }

        if (m_group < 0 || m_name == "") throw new Exception("Could not find #Define block!");

        m_desc = string.Format("Protocol script auto Generated by {{0}}, do not manually modify this file, any modifications will\nlost after the protocol definition files changed!\n\nProtocol Group ID: {0}, Protocol Group Name: {1}{2}", m_group, m_name, string.IsNullOrEmpty(m_desc) ? "" : "\n\nDescription:\n\n") + m_desc;
    }

    private void ParsePackets()
    {
        m_pkts.Clear();

        var r = new Regex(@"((?:\/\*(?:(?!\*\/)(?:\s|.))+\*\/(?:\r?\n)+)|(?:\s*\/{2,}.*(?:\r?\n)+)*)?protocol\s+(\w{1,50})\[(\d+)(?:,\s*(\w+))?(?:,\s*(\w+))?\](?:\r?\n?\{)([^\}]*)\}");
        var match = r.Match(m_data);
        if (!match.Success)
        {
            Logger.LogWarning("Could not find any packet definition!");
            return;
        }

        m_cts = "";
        while (match.Success)
        {
            var id = Util.Parse<int>(match.Groups[3].Value);
            var p0 = match.Groups[4].Value;
            var p1 = match.Groups[5].Value;

            string route = "";
            var earlySend = false;
            const string mark = "early";
            if (!string.IsNullOrEmpty(p0) && !string.IsNullOrEmpty(p1))
            {
                route = p0 == mark ? p1 : p0;
                earlySend = p0 == mark || p1 == mark;
            }
            else if (!string.IsNullOrEmpty(p0))
            {
                if (p0 == mark) earlySend = true;
                else route = p0;
            }
            else if (!string.IsNullOrEmpty(p1))
            {
                if (p1 == mark) earlySend = true;
                else route = p1;
            }

            if (string.IsNullOrWhiteSpace(route)) route = "";

            var pname = match.Groups[2].Value;
            if (id < 1 || id > ushort.MaxValue)
            {
                Logger.LogError("Packt id invalid, packet id must int [1, 65535]. Packet: [group: {0}, id: {1}, name:{2]", group, id, pname);
                match = match.NextMatch();
                continue;
            }

            var op = GetPacket(id, pname);
            if (op != null)
            {
                Logger.LogError("Packt name or id duplicated. Old packet: [file: {0}, group: {1}, id: {2}, name:{3}], New packet: [file: {4}, group: {5}, id: {6}, name:{7}]", op.file, op.group, op.id, op.name, file, group, id, pname);
                match = match.NextMatch();
                continue;
            }

            var comment = Regex.Replace(Regex.Replace(match.Groups[1].Value, @"(\r?\n)+", "\n"), @"^[\n\s]*(?:\/\*)?((?:.|\s)*?)(?:[\s\n]*\*\/)?[\n\s]*$", "$1", RegexOptions.Multiline);
            var pkt = new PacketDef(file, pname, group, Util.FirstUpper(name), id, route, earlySend, comment, match.Groups[6].Value, null);
            m_cts += "|" + pkt.name;

            m_pkts.Add(pkt);
            match = match.NextMatch();
        }

        ParseFields();
    }

    public void ParseFields()
    {
        foreach (var p in m_pkts) _ParseFields(p);
    }

    private void _ParseFields(PacketDef pkt)
    {
        var r = new Regex(@"((?:(?:[u]?int(?:8|16|32|64))|bool|single|double|string" + m_cts + @")(?:\[\])?)\s+(\w{1,30})\s*;(?:(?:\s*\/\*\s?((?!\r?\n).+?)\*\/)|(?:\s*\/\/\s?((?!\r?\n).+)))?");
        var match = r.Match(pkt.content);

        var fields = new List<FieldDef>();

        int tl = 0, nl = 0, dl = 0;
        while (match.Success)
        {
            var def    = match.Groups[1].Value;
            var t      = def.Replace("[]", "");
            var cpkt   = GetPacket(0, t);
            var custom = cpkt != null;

            var fn = match.Groups[2].Value;
            if (fields.FindIndex(f => f.name == fn) > -1)
            {
                Logger.LogError("Packet field name duplicated. Packet: [group: {0}, id: {1}, name:{2}, filed: {3}]", pkt.group, pkt.id, pkt.name, fn);
                match = match.NextMatch();
                continue;
            }

            var comment = !string.IsNullOrEmpty(match.Groups[3].Value) ? match.Groups[3].Value : !string.IsNullOrEmpty(match.Groups[4].Value) ? match.Groups[4].Value : "";
            comment = Regex.Replace(comment, @"[\r\n]*", "").TrimStart(' ');

            var field = new FieldDef(def, fn, comment, custom, cpkt != null ? cpkt.group : 0, cpkt != null ? cpkt.groupName : "");
            fields.Add(field);

            if (def.Length > tl) tl = def.Length;
            if (fn.Length > nl) nl = fn.Length;
            if (fn.Length > dl && (field.custom || field.array)) dl = fn.Length;

            match = match.NextMatch();
        }

        pkt.fl     = new int[] { tl, nl, dl };
        pkt.fields = fields.ToArray();
    }
}
