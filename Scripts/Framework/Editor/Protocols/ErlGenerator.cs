/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Erlang protocol script generator.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-04-05
 * 
 ***************************************************************************************************/

using System;
using System.Collections;
using System.Text.RegularExpressions;

public class ErlGenerator : ILanguageGenerator
{
    #region Type definitions

    private Hashtable m_types = new Hashtable()
    {
        { "bool",        "int8"      },
        { "bool[]",      "int8"      },
        { "int8",        "int8"      },
        { "uint8",       "uint8"     },
        { "int8[]",      "int8"      },
        { "uint8[]",     "uint8"     },
        { "int16",       "int16"     },
        { "uint16",      "uint16"    },
        { "int16[]",     "int16"     },
        { "uint16[]",    "uint16"    },
        { "int32",       "int32"     },
        { "uint32",      "uint32"    },
        { "int32[]",     "int32"     },
        { "uint32[]",    "uint32"    },
        { "int64",       "int64"     },
        { "uint64",      "uint64"    },
        { "int64[]",     "int64"     },
        { "uint64[]",    "uint64"    },
        { "single",      "float"     },
        { "single[]",    "float"     },
        { "double",      "double"    },
        { "double[]",    "double"    },
        { "string",      "string"    },
        { "string[]",    "string"    },
    };

    #endregion

    public string[] fileExtensions { get { return new string[] { ".proto" }; } }

    public string GetFileName(int group, string name) { return "Proto_" + group + "_" + name; }

    public string ParseType(string type) { return m_types.ContainsKey(type) ? (string)m_types[type] : ""; }

    public static string ParseComment(string comment)
    {
        var s = "";
        var lines = Util.ParseString<string>(comment, true, '\n');
        for (var i = 0; i < lines.Length; ++i)
        {
            var line = Regex.Replace(lines[i], @"^\s*(?:(?:\/\/)|\*)(.*)\s*$", "$1");
            line = line.StartsWith(" ") ? line.Substring(1) : line;
            if (i == lines.Length - 1 && string.IsNullOrEmpty(line)) continue;
            s += "// " + line + "\r\n";
        }
        return s;
    }

    public string[] ParseDesc(string desc, ProtocolGenerator g)
    {
        var lines = Util.ParseString<string>(desc, true, '\n');
        var d = "//*************************************************************************************************************\r\n//\r\n";
        for (var i = 0; i < lines.Length; ++i)
        {
            var line = lines[i]; line = line.StartsWith(" ") ? line.Substring(1) : line;
            if (i == 0) line = string.Format(line, GetType().Name);
            d += "// " + line + "\r\n";
        }
        d += "//\r\n//*************************************************************************************************************\r\n\r\n";

        // Add group info for erlang
        d += "// global route info\r\n";
        d += string.Format("group: {0}\r\nname : {1}\r\n\r\n", g.group, g.name.ToLower());

        return new string[] { d };
    }

    public string[] Generate(PacketDef pkt, ProtocolGenerator g)
    {
        var code = ParseComment(pkt.comment);
        code += string.Format("message {0}[id={1}{2}]\r\n{{\r\n", pkt.name, pkt.id, string.IsNullOrEmpty(pkt.router) ? "" : ", route=" + pkt.router);

        var fs = pkt.fields;
        for (var i = 0; i < fs.Length; ++i)
        {
            var f = fs[i];
            code += string.Format("    {0}    {1,-" + (pkt.fl[0] + 2) + "} {2,-" + (pkt.fl[1] + 2) + "} = " + (string.IsNullOrEmpty(f.comment) ? "{3}" : "{3, -8}// ") + "{4}\r\n", f.array ? "repeated" : "required", f.custom? f.typeName: ParseType(f.type), f.name, (i + 1) + ";", f.comment);
        }

        code += "}\r\n";

        return new string[] { code };
    }

    public void OnComplete(string[] code, PacketDef[] gps, ProtocolGenerator g) { }
    public void OnAllComplete(ProtocolGenerator[] groups, PacketDef[] packets, string tar) { }
}