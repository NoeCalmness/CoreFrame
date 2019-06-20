/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Default loading window.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-05
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Window_Debug : Window
{
    private Text          m_log           = null;
    private Button        m_btnHide       = null;
    private Button        m_btnClose      = null;
    private InputField    m_input         = null;
    private RectTransform m_content       = null;
    private bool          m_contentState  = true;
    private StringBuilder m_sb            = new StringBuilder();
    private List<string>  m_history       = new List<string>();
    private int           m_currHistory   = -1;
    private RectTransform m_panel         = null;

    protected override void OnOpen()
    {
        markedGlobal = true;
        isFullScreen = false;

        m_log      = GetComponent<Text>("panel/view/content/log/view/txt");
        m_btnHide  = GetComponent<Button>("panel/title/btnHide");
        m_btnClose = GetComponent<Button>("panel/title/btnClose");
        m_input    = GetComponent<InputField>("panel/view/content/command");
        m_content  = GetComponent<RectTransform>("panel/view/content");
        m_panel    = GetComponent<RectTransform>("panel");

        m_btnHide.onClick.AddListener(HideContent);
        m_btnClose.onClick.AddListener(() => { Hide(); });
        m_input.onEndEdit.AddListener(OnCommand);
        m_input.keyEvent().AddListener(KeyCode.UpArrow,   OnHistory);
        m_input.keyEvent().AddListener(KeyCode.DownArrow, OnHistory);
    }

    private void HideContent()
    {
        if (!DOTween.IsTweening(m_panel))
        {
            if (m_contentState) m_panel.name = m_panel.sizeDelta.y.ToString();
            else m_panel.sizeDelta = new Vector2(m_panel.sizeDelta.x, Util.Parse<float>(m_panel.name));
        }

        m_contentState = !m_contentState;
        m_btnHide.transform.DORotate(new Vector3(0, 0, !m_contentState ? 0 : -90.0f), 0.2f);

        m_content.DOAnchorPosY(m_contentState ? 0 : m_content.rect.height, 0.2f).OnComplete(() => { if (!m_contentState) m_panel.sizeDelta = new Vector2(m_panel.sizeDelta.x, 26); });
    }

    private void OnCommand(string command)
    {
        if (string.IsNullOrEmpty(command)) return;

        var args = Util.ParseString<string>(command);
        var cmd  = args[0];
        var tmp  = cmd;
        var info = ConfigManager.Find<CommandInfo>(i => i.command == cmd);
        if (info == null) cmd = "";
        m_sb.Append(m_log.text);
        var ol = m_sb.Length;

        switch (cmd)
        {
            case "help":
            {
                var infos = ConfigManager.GetAll<CommandInfo>();
                var cmds  = "";
                foreach (var i in infos) if (i.ID != 0) cmds += "\n    <b>" + i.command + ":</b> " + i.comment;
                m_sb.AppendFormat("<b>Commands:</b> {0}", cmds);
                break;
            }
            case "clear": m_sb.Remove(0, m_sb.Length); break;
            case "open":
            {
                if (args.Length < 3) m_sb.Append(info.usage);
                else
                {
                    string t = args[1], n = args[2];
                    switch (t)
                    {
                        case "w":
                            case "window": Window.ShowAsync(n); break;
                        case "l":
                        case "level": var id = 0; if (int.TryParse(n, out id)) Game.LoadLevel(id); else Game.LoadLevel(n); break;
                        default: m_sb.Append(info.usage); break;
                    }
                }
                break;
            }
            case "destroy":
            {
                if (args.Length < 2) m_sb.Append(info.usage);
                else
                {
                    var o = ObjectManager.FindObject(args[1]);
                    if (o) o.Destroy();
                    else m_sb.Append("Could not find any LogicObject named [" + args[1] + "]");
                }
                break;
            }
            case "set": OnSetCommand(args, info); break;
            case "session": OnSessionCommand(args, info); break;
            case "pvp": OnPVPCommand(args, info); break;
            default:
            {
                info = ConfigManager.Get<CommandInfo>(0);
                m_sb.AppendFormat(info != null ? info.comment : "Unknow command <b>[{0}]</b>, please use <help> to show command list.", tmp);
                break;
            }
        }

        if (ol != m_sb.Length && m_sb.Length > 0) m_sb.Append("\n");
        m_log.text = m_sb.ToString();
        m_sb.Remove(0, m_sb.Length);
        m_input.text = "";
        m_currHistory = -1;

        m_input.ActivateInputField();

        if (info && info.ID != 0) m_history.Insert(0, command);
        if (m_history.Count > 100) m_history.RemoveRange(100, m_history.Count - 100);
    }

    private void OnHistory(KeyCode key)
    {
        if (m_history.Count < 1) return;
        m_currHistory += key == KeyCode.UpArrow ? 1 : -1;
        m_currHistory = m_currHistory >= m_history.Count ? 0 : m_currHistory < 0 ? m_history.Count - 1 :  m_currHistory;

        m_input.text = m_history[m_currHistory];

        m_input.caretPosition = m_input.text.Length;
    }

    private void OnSetCommand(string[] subcommands, CommandInfo info)
    {
        if (subcommands.Length < 3) { m_sb.Append(info.usage); return; }

        var t = subcommands[1];
        var v = subcommands[2];

        switch (t)
        {
            case "server":
            {
                var vi = Util.Parse<int>(v);
                if (vi != 0)
                {
                    var si = ServerConfigInfo.Get(vi);
                    if (si != null) Root.defaultServer = vi;
                    else
                    {
                        m_sb.AppendFormat("Could not find server [{0}] from config", vi);
                        return;
                    }
                }
                var p  = subcommands.Length > 3 ? Util.Parse<int>(subcommands[3]) : 12345;
                if (p == 0) p = 12345;
                session.UpdateServer(v, p); break;
            }
            case "log": Logger.enabled = v == "on"; break;
            default: m_sb.Append(info.usage); break;
        }
    }

    private void OnSessionCommand(string[] subcommands, CommandInfo info)
    {
        if (subcommands.Length < 2) { m_sb.Append(info.usage); return; }

        var c = subcommands[1];
        var h = subcommands.Length > 2 ? subcommands[2] : string.Empty;
        var p = subcommands.Length > 3 ? Util.Parse<int>(subcommands[3]) : 12345;

        switch (c)
        {
            case "connect": if (!string.IsNullOrEmpty(h)) session.Connect(h, p); else session.Connect(); break;
            case "disconnect": session.Disconnect(); break;
            default: m_sb.Append(info.usage); break;
        }
    }

    private void OnPVPCommand(string[] subcommands, CommandInfo info)
    {
        if (subcommands.Length < 2) return;
        switch (subcommands[1])
        {
            //case "log": AntiLog.SetLogState(AntiLogType.ARENA_NET, subcommands.Length > 3 && subcommands[3] == "true" ? true : false); break;
            case "connect": if (!modulePVP.connected) modulePVP.Connect(modulePVP.host, modulePVP.port); break;
            case "kill": if (modulePVP.connected) modulePVP.Disconnect(); break;
            case "test":
                var pp6 = PacketObject.Create<CsEnterGame>();
                modulePVP.Send(pp6);
                break;
            default: break;
        }
    }
}
