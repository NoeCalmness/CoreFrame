/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Server list info
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-11-16
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class ServerConfigInfo
{
    #region Static functions

    public static readonly ServerConfigInfo defaultServer = new ServerConfigInfo() { host = "127.0.0.1", port = 12345, ID = 0, comment = "#default server", isHttp = false };

    private static List<ServerConfigInfo> m_servers = null;

    public static ServerConfigInfo Get(int ID)
    {
        if (ID == 0) return defaultServer;
        return m_servers?.Find(s => s.ID == ID);
    }

    public static void LoadServerInfos()
    {
        if (m_servers == null) m_servers = new List<ServerConfigInfo>();
        else m_servers.Clear();

        var texts = Resources.Load<TextAsset>("internal_servers");

        string[] ss = null;
        if (texts)
        {
            ss = Util.ParseString<string>(texts.text.Replace("\r\n", "\n"), false, '\n');
            Resources.UnloadAsset(texts);
        }

        if (ss == null || ss.Length < 1)
        {
            Logger.LogError("Could not load servers, load <b>[Resources/internal_servers.text]</b> failed.");
            Root.UpdateServerInfo();
            return;
        }

        foreach (var s in ss)
        {
            var vs = s.TrimStart(' ').TrimEnd(' ');
            if (string.IsNullOrEmpty(vs) || vs.Length > 0 && vs[0] == '#') continue;

            var comment = string.Empty;
            var sharp = vs.LastIndexOf('#');
            if (sharp > -1)
            {
                comment = vs.Substring(sharp);
                vs = vs.Remove(sharp).TrimEnd(' ');
            }

            var ps = Util.ParseString<string>(vs, false, ' ');
            if (ps.Length < 2)
            {
                Logger.LogError("Invalid server config info <color=#00DDFF><b>[{0}]</b></color>", s);
                continue;
            }

            var id = Util.Parse<int>(ps[0]);
            if (id <= 0)
            {
                Logger.LogError("Invalid server config info <color=#00DDFF><b>[{0}]</b></color>, invalid id <color=#00DDFF><b>[{1}]</b></color>.", s, ps[0]);
                continue;
            }

            var host = ps[1];
            var www  = host.StartsWith("http://") ? "http://" : host.StartsWith("https://") ? "https://" : string.Empty;
            var http = www != string.Empty;

            host = http ? host.TrimEnd('/').Replace(www, "") : host.TrimEnd('/');

            var hs = Util.ParseString<string>(host, false, ':');
            if (hs.Length < 1 || hs.Length > 2)
            {
                Logger.LogError("Invalid server config info <color=#00DDFF><b>[{0}]</b></color>, invalid host <color=#00DDFF><b>[{1}]</b></color>.", s, ps[1]);
                continue;
            }
            
            var port = hs.Length == 1 ? http ? 80 : 0 : Util.Parse<int>(hs[1]);
            if (port < 1 || port > 65535)
            {
                Logger.LogError("Invalid server config info <color=#00DDFF><b>[{0}]</b></color>, invalid port <color=#00DDFF><b>[{1}]</b></color>", s, hs.Length > 1 ? hs[1] : port.ToString());
                continue;
            }

            var idx = m_servers.FindIndex(si => si.ID == id);
            if (idx > -1)
            {
                Logger.LogWarning("Duplicated server config <color=#00DDFF><b>[{0}]</b></color>, use newer one.", vs);

                m_servers.RemoveAt(idx);
            }

            var alias = ps.Length > 2 ? ps[2] : string.Empty;

            m_servers.Add(new ServerConfigInfo(id, www, hs[0], port, alias, comment));
        }

        if (m_servers.Count < 1) Logger.LogError("Load 0 servers, load <b>[Resources/internal_servers.text]</b> is empty.");
        else
        {
            var sss = string.Empty;
            foreach (var server in m_servers) sss += server.ToString("{0}\t{1}\t{2}\t{3}") + '\n';
            Logger.LogDetail("Server List:\n{0}", sss);
        }

        Root.UpdateServerInfo();
    }

    #endregion

    public int ID;
    public string host;
    public int port;
    public string alias;
    public string comment;
    public string fullHost;

    public bool isHttp { get; private set; } = false;

    public ServerConfigInfo() { }

    public ServerConfigInfo(int _ID, string www, string _host, int _port, string _alias = "", string _comment = "")
    {
        isHttp = www != string.Empty;

        ID      = _ID;
        host    = _host;
        port    = _port;
        alias   = _alias;
        comment = _comment ?? string.Empty;

        fullHost = isHttp ? www + host + (port == 80 ? "" : ":" + port) + "/" : host + ":" + port;

    }

    public void CopyFrom(ServerConfigInfo other)
    {
        if (other == null) return;

        ID       = other.ID;
        host     = other.host;
        port     = other.port;
        alias    = other.alias;
        comment  = other.comment;
        fullHost = other.fullHost;
        isHttp   = other.isHttp;
    }

    public override string ToString()
    {
        return Util.Format("{0} {1}{2}{3}", ID, fullHost, string.IsNullOrEmpty(alias) ? "" : " " + alias, string.IsNullOrEmpty(comment) ? "" : " " + comment);
    }

    public string ToString(string format)
    {
        return Util.Format(format, ID, fullHost, alias, comment);
    }
}