/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * C# protocol script generator.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-04-05
 * 
 ***************************************************************************************************/

using System.Collections;
using System.Text.RegularExpressions;

public class CSGenerator : ILanguageGenerator
{
    #region Type definitions

    private Hashtable m_types = new Hashtable()
    {
        { "bool",        "bool"       },
        { "bool[]",      "bool[]"     },
        { "int8",        "sbyte"      },
        { "uint8",       "byte"       },
        { "int8[]",      "sbyte[]"    },
        { "uint8[]",     "byte[]"     },
        { "int16",       "short"      },
        { "uint16",      "ushort"     },
        { "int16[]",     "short[]"    },
        { "uint16[]",    "ushort[]"   },
        { "int32",       "int"        },
        { "uint32",      "uint"       },
        { "int32[]",     "int[]"      },
        { "uint32[]",    "uint[]"     },
        { "int64",       "long"       },
        { "uint64",      "ulong"      },
        { "int64[]",     "long[]"     },
        { "uint64[]",    "ulong[]"    },
        { "single",      "float"      },
        { "single[]",    "float[]"    },
        { "double",      "double"     },
        { "double[]",    "double[]"   },
        { "string",      "string"     },
        { "string[]",    "string[]"   },
    };
    private Hashtable m_readNames = new Hashtable()
    {
        { "bool",        "Boolean"         },
        { "sbyte",       "SByte"           },
        { "byte",        "Byte"            },
        { "short",       "Int16"           },
        { "ushort",      "UInt16"          },
        { "int",         "Int32"           },
        { "uint",        "UInt32"          },
        { "long",        "Int64"           },
        { "ulong",       "UInt64"          },
        { "float",       "Single"          },
        { "double",      "Double"          },
        { "string",      "String"          },
    };

    #endregion

    public string[] fileExtensions { get { return new string[] { ".cs" };  } }

    public string GetFileName(int group, string name) { return "Proto_" + group + "_" + name; }

    public string ParseType(string type) { return m_types.ContainsKey(type) ? (string)m_types[type] : ""; }

    public string ParseReadName(FieldDef field)
    {
        var t = ParseType(field.type);
        if (string.IsNullOrEmpty(t)) return string.Format("PacketObject{0}<{1}>", field.array ? "Array" : "", ParsePacketName(field.typeName));
        return string.Format((string)m_readNames[field.array ? t.Substring(0, t.Length - 2) : t] + "{0}", field.array ? "Array" : "");
    }

    public string ParsePacketName(string name)
    {
        name = Regex.Replace(name.Replace("sc_", "Sc_").Replace("cs_", "Cs_"), @"(\w|\d)_(\w|\d)", "$1|$2");
        var ss = Util.ParseString<string>(name, false, '|');
        name = "";
        foreach (var s in ss) name += Util.FirstUpper(s);
        return name;
    }

    public static string ParseComment(string comment)
    {
        var lines = Util.ParseString<string>(comment, true, '\n');
        if (lines.Length < 1) return "";
        var s = "/// <summary>\r\n";
        for (var i = 0; i < lines.Length; ++i)
        {
            var line = Regex.Replace(lines[i], @"^\s*(?:(?:\/\/)|\*)(.*)\s*$", "$1");
            line = line.StartsWith(" ") ? line.Substring(1) : line;
            if (i == lines.Length - 1 && string.IsNullOrEmpty(line)) continue;
            s += "/// " + line + "\r\n";
        }
        s += "/// </summary>\r\n";
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

        return new string[] { d };
    }

    public string[] Generate(PacketDef pkt, ProtocolGenerator g)
    {
        var code = ParseComment(pkt.comment);
        var pn   = ParsePacketName(pkt.name);
        //code += string.Format("[PacketInfo(group = {0}, ID = {1}, early = {2})]\r\npublic class {3} : PacketObject<{3}>\r\n{{\r\n", pkt.group, pkt.id, pkt.earlySend ? "true" : "false", ParsePacketName(pkt.name));
        code += string.Format("[PacketInfo(group = {0}, ID = {1})]\r\npublic class {2} : PacketObject<{2}>\r\n{{\r\n", pkt.group, pkt.id, ParsePacketName(pkt.name));

        string rc = "", wc = "", dc0 = "", dc1 = "", cc0 = "", cc1 = "";

        var fs = pkt.fields;
        foreach (var f in fs)
        {
            code += string.IsNullOrEmpty(f.comment) ? "" : "    /// <summary>\r\n    /// " + f.comment + "\r\n    /// </summary>\r\n";
            code += string.Format("    public {0} {1};\r\n", f.custom ? ParsePacketName(f.type) : ParseType(f.type), f.name);

            rc += string.Format("        {0, -" + (pkt.fl[1] + 2) + "} = p.Read{1}();\r\n", f.name, ParseReadName(f));
            wc += "        p.Write(" + f.name + ");\r\n";
            if (f.custom || f.array)
            {
                dc0 += f.custom && f.array ? string.Format("        BackArray({0});\r\n", f.name) : "";
                dc1 += string.Format("        {0, -" + (pkt.fl[2] + 2) + "} = null;\r\n", f.name);
            }

            if (!f.custom) cc0 += string.Format("        dst.{0, -" + (pkt.fl[1] + 2) + "} = {0};\r\n", f.name);
            else cc1 += string.Format("        if ({0, -" + (pkt.fl[2] + 1) + "} != null) {0}.CopyTo(ref dst.{0});\r\n", f.name);
        }

        var dc = dc0 + (string.IsNullOrEmpty(dc0) || string.IsNullOrEmpty(dc1) ? "" : "\r\n") + dc1;
        var cc = cc0 + (string.IsNullOrEmpty(cc0) || string.IsNullOrEmpty(cc1) ? "" : "\r\n") + cc1;

        if (!string.IsNullOrEmpty(cc)) cc = "\r\n" + cc;

        code += (fs.Length > 0 ? "\r\n" : "") + "    private " + pn + "() { }\r\n";
        code += string.IsNullOrEmpty(rc) ? "" : "\r\n    public override void ReadFromPacket(Packet p)\r\n    {\r\n" + rc + "    }\r\n";
        code += string.IsNullOrEmpty(wc) ? "" : "\r\n    public override void WriteToPacket(Packet p)\r\n    {\r\n" + wc + "    }\r\n";
        code += string.IsNullOrEmpty(cc) ? "" : "\r\n    public override void CopyTo(ref " + pn + " dst)\r\n    {\r\n        base.CopyTo(ref dst);\r\n" + cc + "    }\r\n";
        code += string.IsNullOrEmpty(dc) ? "" : "\r\n    public override void Clear()\r\n    {\r\n" + dc + "    }\r\n";
        code += "}\r\n";

        return new string[] { code };
    }

    public void OnComplete(string[] code, PacketDef[] gps, ProtocolGenerator g) { }
    public void OnAllComplete(ProtocolGenerator[] groups, PacketDef[] packets, string tar) { }
}