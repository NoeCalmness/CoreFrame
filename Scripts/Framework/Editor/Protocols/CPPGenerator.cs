/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * C++ protocol script generator.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-04-05
 * 
 ***************************************************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

public class CPPGenerator : ILanguageGenerator
{
    #region Type definitions

    private Hashtable m_types = new Hashtable()
    {
        { "bool",        "bool"                        },
        { "int8",        "int8"                        },
        { "uint8",       "uint8"                       },
        { "int8[]",      "std::vector<int8>"           },
        { "uint8[]",     "std::vector<uint8>"          },
        { "int16",       "int16"                       },
        { "uint16",      "uint16"                      },
        { "int16[]",     "std::vector<int16>"          },
        { "uint16[]",    "std::vector<uint16>"         },
        { "int32",       "int32"                       },
        { "uint32",      "uint32"                      },
        { "int32[]",     "std::vector<int32>"          },
        { "uint32[]",    "std::vector<uint32>"         },
        { "int64",       "int64"                       },
        { "uint64",      "uint64"                      },
        { "int64[]",     "std::vector<int64>"          },
        { "uint64[]",    "std::vector<uint64>"         },
        { "single",      "float"                       },
        { "single[]",    "std::vector<float>"          },
        { "double",      "double"                      },
        { "double[]",    "std::vector<double>"         },
        { "string",      "std::string"                 },
        { "string[]",    "std::vector<std::string>"    },
    };

    #endregion

    private string des = "";

    public string[] fileExtensions { get { return new string[] { ".h" }; } }

    public string GetFileName(int group, string n) { return "Packet" + n; }

    public string ParseType(string type) { return m_types.ContainsKey(type) ? (string)m_types[type] : ""; }

    public string ParsePacketName(string name, string fns = "", string ns = "")
    {
        name = Regex.Replace(name.Replace("sc_", "Sc_").Replace("cs_", "Cs_"), @"(\w|\d)_(\w|\d)", "$1|$2");
        var ss = Util.ParseString<string>(name, false, '|');
        name = "";
        foreach (var s in ss) name += Util.FirstUpper(s);
        return string.IsNullOrEmpty(ns) || fns == ns ? name : ns + "::" + name;
    }

    public static string ParseComment(string comment)
    {
        var lines = Util.ParseString<string>(comment, true, '\n');
        if (lines.Length < 1) return "";
        var s = "";
        for (var i = 0; i < lines.Length; ++i)
        {
            var line = Regex.Replace(lines[i], @"^\s*(?:(?:\/\/)|\*)(.*)\s*$", "$1");
            line = line.StartsWith(" ") ? line.Substring(1) : line;
            if (i == lines.Length - 1 && string.IsNullOrEmpty(line)) continue;
            s += "    // " + line + "\r\n";
        }
        return s;
    }

    public string[] ParseDesc(string desc, ProtocolGenerator g)
    {
        var lines = Util.ParseString<string>(desc, true, '\n');
        var d = "/*************************************************************************************************************\r\n * \r\n";
        for (var i = 0; i < lines.Length; ++i)
        {
            var line = lines[i]; line = line.StartsWith(" ") ? line.Substring(1) : line;
            if (i == 0) line = string.Format(line, GetType().Name);
            d += " * " + line + "\r\n";
        }
        d += " * \r\n *************************************************************************************************************/\r\n\r\n";
        var dh = d + string.Format("#ifndef {0}\r\n#define {0}\r\n\r\n#include \"Packet.h\"\r\n{{0}}\r\nnamespace Packets::{1}\r\n{{{{\r\n", "_CM_GAME_CLASS_PACKETS_" + g.name.ToUpper() + "_H_", g.name);
        var dc = d + string.Format("#include <CMLog.h>\r\n#include \"WorldSession.h\"\r\n#include \"{0}.h\"{{0}}\r\n\r\n", GetFileName(g.group, g.name));
        return new string[] { dh, dc, "" };  // class defines / handlers / external include protocol groups
    }

    public string[] Generate(PacketDef pkt, ProtocolGenerator g)
    {
        var code = ParseComment(pkt.comment).Replace("{", "{{").Replace("}", "}}"); ;
        var pn   = ParsePacketName(pkt.name);
        var sp   = pn.StartsWith("Sc");
        var cpp  = sp ? "" : string.Format("void WorldSession::Handle{0}({1} &packet)\r\n{{{{\r\n}}}}\r\n", pn.Replace("Cs", ""), pn);
        var refs = "";
        code += string.Format("    class {0} final : public {1}\r\n    {{{{\r\n", pn, sp ? "ServerPacket" : "ClientPacket");

        string rc = "", wc = "", dc = "";

        var fs     = pkt.fields;
        var nl     = pkt.fl[0];
        var i      = 0;
        var ct     = -1;
        var custom = false;

        foreach (var f in fs)
        {
            var fieldType = f.custom ? f.array ? "std::vector<" + ParsePacketName(f.typeName, pkt.groupName, f.groupName) + "*>" : ParsePacketName(f.typeName, pkt.groupName, f.groupName) + "*" : ParseType(f.type);
            if (fieldType.Length > nl) nl = fieldType.Length;

            if (f.custom && f.groupName != pkt.groupName)
            {
                var mk = f.groupName + ":" + ParsePacketName(f.typeName);
                if (!refs.Contains(mk)) refs += mk + "\r\n";
            }

            if (!custom && f.custom) custom = true;
        }

        foreach (var f in fs)
        {
            if (i == 0) code += "        public:\r\n";

            var fieldType = f.custom ? f.array ? "std::vector<" + ParsePacketName(f.typeName, pkt.groupName, f.groupName) + "*>" : ParsePacketName(f.typeName, pkt.groupName, f.groupName) + "*" : ParseType(f.type);
            code += string.Format("            {0,-" + (nl + 4) + "} {1, -" + (pkt.fl[1] + 4) + "}{2}\r\n", fieldType, f.name + ";", string.IsNullOrEmpty(f.comment) ? "" : "// " + f.comment);

            var t = f.custom ? 1 : 0;
            if (t == 1 && ct == 0 || ct < 0)
            {
                rc += string.Format("{0}{1} ", i == 0 ? "                " : ";\r\n                ", "w");
                wc += string.Format("{0}{1} ", i == 0 ? "                " : ";\r\n                ", "w");
            }
            ct = t;

            rc += string.Format(">> {0}{1}{2}", f.custom && !f.array ? "&" : "", f.name, i == fs.Length - 1 ? ";\r\n" : " ");
            wc += string.Format("<< {0}{1}", f.name, i == fs.Length - 1 ? ";\r\n" : " ");
            if (f.custom || f.array) dc += string.Format("    {0}({1});\r\n", f.custom ? f.array ? "VECTOR_DELETE" : "SAFE_DELETE" : "VECTOR_CLEAR", f.name);

            ++i;
        }

        code += string.Format("{0}        public:\r\n            {1}() : {2}({3}) {{{{ }}}}\r\n", fs.Length > 0 ? "\r\n" : "", pn, sp ? "ServerPacket" : "ClientPacket", pkt.id);
        code += string.Format("            {0}{1}\r\n", string.IsNullOrEmpty(dc) ? "" : "~" + pn + "()", string.IsNullOrEmpty(dc) ? "" : custom ? ";\r\n" : "\r\n            {{\r\n" + dc.Replace("    ", "                ") + "            }}\r\n");
        code += "            void Read(WorldPacket &w) override" + (string.IsNullOrEmpty(rc) ? " {{ }}\r\n\r\n" : "\r\n            {{\r\n" + rc + "            }}\r\n\r\n");
        code += "            void Write(WorldPacket &w) const override" + (string.IsNullOrEmpty(wc) ? " {{ }}\r\n" : "\r\n            {{\r\n" + wc + "            }}\r\n");
        des  += !custom || string.IsNullOrEmpty(dc) ? "" : pn + "::~" + pn + "()\r\n{\r\n" + dc + "}\r\n\r\n";
        code += "    }};\r\n";

        return new string[] { code, cpp, refs };
    }

    public void OnComplete(string[] code, PacketDef[] gps, ProtocolGenerator g)
    {
        code[0] += "}}\r\n\r\nusing namespace Packets::" + g.name + ";\r\n\r\n#endif";

        var refs = Util.ParseString<string>(code[2].Replace("\r\n", "\n").Replace("\n\n", "\n"), false, '\n');

        string def = "", inc = "";
        for (var i = 0; i < refs.Length; ++i)
        {
            var r = Util.ParseString<string>(refs[i], false, ':');
            def += "\r\nclass Packets::" + r[0] + "::" + r[1] + ";";
            inc += inc.Contains(GetFileName(0, r[0]) + ".h") ? "" : "\r\n#include \"" + GetFileName(0, r[0]) + ".h\"";
        }

        if (!string.IsNullOrEmpty(def)) def += "\r\n";

        code[0] = string.Format(code[0], def);
        code[1] = string.Format(code[1], inc);
    }

    public void OnAllComplete(ProtocolGenerator[] groups, PacketDef[] packets, string tar)
    {
        if (tar == null) return;

        var hc = "";
        foreach (var g in groups)
            hc += "\r\n#include \"" + GetFileName(g.group, g.name) + ".h\"";

        if (!string.IsNullOrEmpty(hc)) hc = "\r\n" + hc + "\r\n";

        EditorUtil.SaveFile(tar + "AllPackets.h", string.Format(headers, hc));
        EditorUtil.SaveFile(tar + "AllPackets.cpp", string.Format(destruct, string.IsNullOrEmpty(des) ? "" : "\r\n\r\n" + des));
    }

#region Auto generated code

    private static readonly string headers =
@"#ifndef _CM_GAME_CLASS_ALLPACKETS_H_
#define _CM_GAME_CLASS_ALLPACKETS_H_{0}
#endif";

    private static readonly string destruct =
@"#include ""AllPackets.h""{0}";

#endregion
}