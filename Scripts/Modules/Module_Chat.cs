/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-10-11
 * 
 ***************************************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class Module_Chat : Session
{
    #region Evevt

    /// <summary>
    /// 聊天服务器断线时触发
    /// </summary>
    public const string EventChatLostConnection = "EventChatLostConnection";

    /// <summary>
    /// //接收到组队消息
    /// </summary>
    public const string EventChatRecTeamMes = "EventChatRecTeamMes";

    public const string EventChatChangeRoom = "EventChatChangeRoom";
    public const string EventChatRecSysMes = "EventChatRecSysMes";
    public const string EventChatRecWordMes = "EventChatRecWordMes";
    public const string EventChatRecFriendMes = "EventChatRecFriendMes";
    public const string EventChatSendTeamMes = "EventChatSendTeamMes";
    public const string EventChatPlayerDetails = "EventChatPlayerDetails";
    public const string EventChatFriendChange = "EventChatFriendChange";
    public const string EventChatRoomList = "EventChatRoomList";
    public const string EventChatRecUnionMes = "EventChatRecUnionMes";
    public const string EventChatSeeTeamMsg = "EventSeeTeamMsg";
    public const string EventChatWindowHide = "EventChatWindowHide";

    #endregion

    #region List or Data

    /// <summary>
    /// 是否重新获取服务器所有聊天
    /// </summary>
    public bool CanGetAllChat;

    public Dictionary<ulong, Queue<ScChatPrivate>> Friend_chat_record { get { return friend_chat_record; } set { friend_chat_record = value; } }

    public List<ulong> Friend_Id_key { get { return friend_Id_key; } set { friend_Id_key = value; } }

    public List<ulong> Past_mes { get { return past_mes; } set { past_mes = value; } }

    public List<ulong> Chat_list { get { return chat_list; } set { chat_list = value; } }

    public List<PPlayerInfo> Late_ListAllInfo { get { return late_ListAllInfo; } set { late_ListAllInfo = value; } }

    public Queue<ScChatRoomMessage> word_chat_mes = new Queue<ScChatRoomMessage>();
    public Queue<ScChatRoomMessage> sys_chat_mes = new Queue<ScChatRoomMessage>();// id为110
    public readonly Queue<PacketObject> team_chat_record = new Queue<PacketObject>();

    private Dictionary<ulong, Queue<ScChatPrivate>> friend_chat_record = new Dictionary<ulong, Queue<ScChatPrivate>>();// 所有聊过天的聊天记录
    private List<ulong> friend_Id_key = new List<ulong>();//用来存聊过天的好友id
    private List<ulong> past_mes = new List<ulong>();//在friend未打开时候所接收到的消息 离线，在线忙碌都是 点开的时候把这个id去掉
    private List<PPlayerInfo> late_ListAllInfo = new List<PPlayerInfo>();////最近的列表所有信息（是有顺序的）
    private List<ulong> chat_list = new List<ulong>();//存聊天室的列表和id

    private ulong m_serverRoleId;
    /// <summary>
    /// 工会聊天信息
    /// </summary>
    public Queue<ScChatGroup> m_unionChat = new Queue<ScChatGroup>();

    /// <summary>
    /// 现在是否能够发送世界消息
    /// </summary>
    public bool CanChatWord { get; set; }
    public int ChatLevel;
    public int ChatCD;
    private int IsWhich;

    public bool friend_chat_change { get; set; }
    private ulong RoomChatId;//聊天室真实id
    /// <summary>
    /// 聊天室显示id
    /// </summary>
    public int RoomChatNum { get; set; }

    public bool HavePastMes { get; set; }

    public PPlayerInfo word_play_details { get; set; }

    public ScChatAllMessage localMessage { get; set; }

    private int chatContentTimes = 0;

    #endregion
    /// <summary>
    /// 聊天信息最多条数
    /// </summary>
    public int ChatNum = 100;

    public bool m_newWordMes { get; set; } = false;
    public bool m_newSysMes { get; set; } = false;
    public bool m_newUnionMes { get; set; } = false;
    public bool m_newTeamMes { get; set; } = false;

    public OpenWhichChat opChatType { get; set; } = OpenWhichChat.WorldChat;

    private void _Packet(ScSystemSetting p)
    {
        ChatCD = p.worldCD;
        ChatLevel = p.worldLevel;
    }

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        word_chat_mes.Clear();
        sys_chat_mes.Clear();
        Friend_chat_record.Clear();
        friend_Id_key.Clear();
        past_mes.Clear();
        Late_ListAllInfo.Clear();
        chat_list.Clear();
        CanChatWord = true;
        HavePastMes = false;
        friend_chat_change = false;
    }

    #region 最近联系人

    public void GetLateInfo()
    {
        CsChatRecent p = PacketObject.Create<CsChatRecent>();
        Send(p);
    }

    void _Packet(ScChatRecent p)
    {
        late_ListAllInfo.Clear();
        PPlayerInfo[] info = p.Clone().recentList;
        late_ListAllInfo.AddRange(info);
        LateBlackCheck();
    }

    public void LateBlackCheck()
    {
        for (int i = 0; i < moduleFriend.BlackList.Count; i++)
        {
            var have = moduleChat.Late_ListAllInfo.Find(a => a.roleId == moduleFriend.BlackList[i].roleId);
            if (have != null) moduleChat.Late_ListAllInfo.Remove(have);
        }
    }
    public bool ShowLateHint()
    {
        for (int i = 0; i < late_ListAllInfo.Count; i++)
        {
            var past = past_mes.Exists(a => a == late_ListAllInfo[i].roleId);
            if (past) return true;
        }
        return false;
    }

    public void Late_reset(ulong lastid)//每一条消息（排序最近的列表）
    {
        var black = moduleFriend.BlackList.Exists(a => a.roleId == lastid);
        if (black) return;

        PPlayerInfo isIn = late_ListAllInfo.Find(a => a.roleId == lastid);
        PPlayerInfo info = moduleFriend.FriendList.Find(a => a.roleId == lastid);
        if (isIn == null && late_ListAllInfo.Count < 4) moduleFriend.RefreshLateState = true;
        if (info == null) info = LatePlayerInfo(lastid);
        if (isIn != null) late_ListAllInfo.Remove(isIn);
        if (info != null) late_ListAllInfo.Insert(0, info);
        if (late_ListAllInfo.Count > 10) late_ListAllInfo.Remove(late_ListAllInfo[10]);//如果现在超过十个了 就直接移除最后一个
    }

    public PPlayerInfo LatePlayerInfo(ulong lateId)
    {
        PPlayerInfo player = late_ListAllInfo.Find(a => a.roleId == lateId);
        if (player != null) return player;
        PUnionPlayer chat = moduleUnion.m_unionPlayer.Find(a => a.info.roleId == lateId);
        if (chat != null) player = chat.info;
        return player;
    }

    #endregion

    #region 聊天记录

    public void Get_ExitMes()//获取离线消息
    {
        CsChatLeaveMessage p = PacketObject.Create<CsChatLeaveMessage>();
        Send(p);
    }

    void _Packet(ScChatLeaveMessage p)
    {
        PMessage[] notice = p.Clone().messages;
        for (int i = 0; i < notice.Length; i++)
        {
            ulong Chatid = notice[i].from;
            if (notice[i].from == modulePlayer.roleInfo.roleId)
            {
                Chatid = notice[i].to;
            }
            bool isfriend = moduleFriend.FriendList.Exists(a => a.roleId == Chatid);
            if (isfriend) Islevel(notice[i]);
        }
    }

    #endregion

    #region 世界聊天相关

    #region 信息发送接收(世界及系统 公会)

    public void SendWordMessage(string content, int sysType, OpenWhichChat type)//systype (文字 图片 语音) fromType(0 世界 1 公会 2组队等)
    {
        if (modulePlayer.BanChat == 2)
        {
            moduleGlobal.ShowMessage(630, 0);
            return;
        }
        if (type == OpenWhichChat.WorldChat)
        {
            moduleChat.CanChatWord = false;
            var b = moduleChat.SetWorldSend(content, sysType);
            if (word_chat_mes.Count > ChatNum) word_chat_mes.Dequeue();

            word_chat_mes.Enqueue(b);

            CsChatRoomMessage p = SetWorldSession(content, sysType);
            Send(p);
        }
        else if (type == OpenWhichChat.UnionChat)
        {
            var b = moduleChat.SetUnionSend(content, sysType);
            if (m_unionChat.Count > ChatNum) word_chat_mes.Dequeue();
            m_unionChat.Enqueue(b);

            CsChatGroup p = SetUnionSession(content, sysType);
            Send(p);
        }
    }
    private CsChatRoomMessage SetWorldSession(string content, int sysType)//systype (文字 图片 语音)世界聊天
    {
        CsChatRoomMessage p = PacketObject.Create<CsChatRoomMessage>();
        p.content = content;
        p.type = (sbyte)sysType;
        p.tag = ChatTag();
        return p;
    }
    public ScChatRoomMessage SetWorldSend(string content, int sysType)//systype (文字 图片 语音) 
    {
        ScChatRoomMessage p = PacketObject.Create<ScChatRoomMessage>();
        p.sendId = modulePlayer.id_;
        p.type = (sbyte)sysType;
        p.tag = ChatTag();
        p.content = content;
        return p;
    }

    private CsChatGroup SetUnionSession(string content, int sysType)//systype (文字 图片 语音) 工会聊天
    {
        CsChatGroup p = PacketObject.Create<CsChatGroup>();
        p.groupId = modulePlayer.roleInfo.leagueID;
        p.type = (sbyte)sysType;
        p.tag = ChatTag();
        p.content = content;
        return p;
    }
    public ScChatGroup SetUnionSend(string content, int sysType)//systype (文字 图片 语音)
    {
        ScChatGroup p = PacketObject.Create<ScChatGroup>();
        p.sendId = modulePlayer.id_;
        p.type = (sbyte)sysType;
        p.tag = ChatTag();
        p.content = content;
        return p;
    }
    private string ChatTag()
    {
        return modulePlayer.name_ + "/" + modulePlayer.gender + "/" + modulePlayer.avatar + "/" + modulePlayer.avatarBox + "/" + modulePlayer.proto; //名字 性别 头像 头像框
    }

    void _Packet(ScChatGroup p)
    {
        var black = moduleFriend.BlackList.Exists(a => a.roleId == p.sendId);
        if (black) return;

        ScChatGroup info = p.Clone();
        string[] mes = info.tag.Split('/');
        if (mes.Length < 5)
        {
            Logger.LogError("chat mes error");
            return;
        }
        m_newUnionMes = true;
        if (m_unionChat.Count >= ChatNum)
        {
            m_unionChat.Dequeue();
        }
        m_unionChat.Enqueue(info);
        DispatchModuleEvent(EventChatRecUnionMes, info);
    }

    void _Packet(ScChatRoomMessage p)
    {
        ScChatRoomMessage info = p.Clone();

        string[] mes = info.tag.Split('/');
        if (mes.Length < 5)
        {
            Logger.LogError("chat mes error");
            return;
        }
        m_newWordMes = true;
        if (word_chat_mes.Count >= ChatNum)
        {
            word_chat_mes.Dequeue();
        }
        word_chat_mes.Enqueue(info);// 本地 载入界面的时候进行调用

        DispatchModuleEvent(EventChatRecWordMes, info);
    }

    public void SysChatMes(ScChatRoomMessage info, int sysType)
    {
        if (info.sendId == 110)
        {
            m_newSysMes = true;

            if (sys_chat_mes.Count >= ChatNum) sys_chat_mes.Dequeue();
            sys_chat_mes.Enqueue(info);

            if (sysType != (int)SysMesType.Wish && sysType != (int)SysMesType.Team)
            {
                if (word_chat_mes.Count >= ChatNum) word_chat_mes.Dequeue();
                word_chat_mes.Enqueue(info);
            }
            DispatchModuleEvent(EventChatRecSysMes, info.content);
        }
    }

    #endregion

    #region 世界房间列表及切换

    public void SendChatRoomList()
    {
        CsChatRooms p = PacketObject.Create<CsChatRooms>();
        Send(p);
    }

    void _Packet(ScChatRooms p)
    {
        ScChatRooms pp = p.Clone();
        chat_list.Clear();
        RoomChatNum = 1;
        //这里存一个字典类
        for (int i = 0; i < p.rooms.Length; i++)
        {
            ulong id = pp.rooms[i];
            if (id == RoomChatId)
            {
                RoomChatNum = i + 1;//显示在界面上的数字
            }
            chat_list.Add(id);
        }
        DispatchModuleEvent(EventChatRoomList);
    }

    public void SendChangeRoom(ulong roomId)
    {
        CsChatRoomChange p = PacketObject.Create<CsChatRoomChange>();
        p.roomId = roomId;
        Send(p);
    }

    void _Packet(ScChatRoomChange p)
    {
        if (p.result == 0)
        {
            RoomChatId = p.roomId;
            word_chat_mes.Clear();
        }
        DispatchModuleEvent(EventChatChangeRoom, p.result);
    }

    #endregion

    #region 查看发言玩家详情

    public void Show_word_details(ulong playId)
    {
        CsRoleDetailView p = PacketObject.Create<CsRoleDetailView>();
        p.roleId = playId;
        session.Send(p);
    }

    void _Packet(ScRoleDetailView p)
    {
        var playerInfo = p.Clone().info;
        if (playerInfo == null) return;
        var is_not = 0;
        bool isfriend = moduleFriend.FriendList.Exists(a => a.roleId == playerInfo.roleId);
        if (isfriend) is_not = 1;

        word_play_details = playerInfo;
        IsWhich = is_not;
        DispatchModuleEvent(EventChatPlayerDetails, playerInfo);//这里直接换成pplayerinfo
    }

    #endregion

    #region 公会聊天相关

    public void AddChatList(string str, bool addChat)
    {
        //110 系统 111 是公会
        ScChatGroup p = PacketObject.Create<ScChatGroup>();
        p.sendId = 111;
        p.type = 0;
        p.content = str;
        if (m_unionChat.Count > ChatNum) m_unionChat.Dequeue();

        if (addChat)
        {
            m_unionChat.Enqueue(p);
            DispatchModuleEvent(EventChatRecUnionMes, p);
        }
    }

    #endregion


    #endregion

    #region 好友聊天相关

    #region 离线留言及消息记录

    public void GetServerMes()
    {
        moduleFriend.GetAllChatInfo = false;
        CsChatAllMessage p = PacketObject.Create<CsChatAllMessage>();
        Send(p);
    }

    void _Packet(ScChatAllMessage p)//从服务器获得所有聊天信息
    {
        localMessage = p.Clone();
        if (localMessage != null) GetChatjilu();
        moduleFriend.FriendList = moduleFriend.SortWay();
    }

    public void GetChatjilu()
    {
        friend_chat_record.Clear();
        friend_Id_key.Clear();
        past_mes.Clear();

        PMessage[] meslist = localMessage.messages;
        for (int i = 0; i < meslist.Length; i++)
        {
            if (meslist[i].tag == "2") continue;

            ulong Chatid = meslist[i].from;
            if (meslist[i].from == modulePlayer.roleInfo.roleId) Chatid = meslist[i].to;

            //如果是离线消息则加进去
            if (meslist[i].leave) Islevel(meslist[i]);
            else
            {
                ScChatPrivate scf = SetPrivate(meslist[i].from, meslist[i].type, meslist[i].content, meslist[i].tag);

                Queue<ScChatPrivate> past;
                friend_chat_record.TryGetValue(Chatid, out past);
                if (past == null) past = new Queue<ScChatPrivate>();
                else
                {
                    friend_chat_record.Remove(Chatid);
                    friend_Id_key.Remove(Chatid);
                }
                if (past.Count >= ChatNum) past.Dequeue();
                past.Enqueue(scf);
                friend_chat_record.Add(Chatid, past);
                friend_Id_key.Add(Chatid);
            }
        }
    }

    private void Islevel(PMessage mess)//如果是离线消息
    {
        if (mess.leave)
        {
            ulong ididid = mess.from;
            if (mess.from == modulePlayer.roleInfo.roleId) ididid = mess.to;
            
            bool o = friend_Id_key.Exists(a => a == ididid);

            Queue<ScChatPrivate> chat;
            ScChatPrivate scchat = SetPrivate(ididid, mess.type, mess.content, mess.tag);

            bool canaddlevel = past_mes.Exists(a => a == ididid);
            if (!canaddlevel)
            {
                moduleHome.UpdateIconState(HomeIcons.Friend, true);
                HavePastMes = true;
                past_mes.Add(ididid);
            }

            if (o)
            {
                friend_chat_record.TryGetValue(ididid, out chat);
                if (chat != null)
                {
                    if (chat.Count >= ChatNum) chat.Dequeue();
                    chat.Enqueue(scchat);
                    friend_chat_record.Remove(ididid);
                    friend_chat_record.Add(ididid, chat);
                }
            }
            else
            {
                Queue<ScChatPrivate> ppp = new Queue<ScChatPrivate>();
                ppp.Enqueue(scchat);
                friend_chat_record.Add(ididid, ppp);
                friend_Id_key.Add(ididid);
            }
            // Record(ididid.ToString(), typea.ToString(), conten, ididid.ToString());
        }
    }

    #endregion

    public void ChangeFriend(ulong ids)
    {
        friend_chat_change = true;
        if (friend_chat_record.ContainsKey(ids))
        {
            ScChatPrivate[] info = friend_chat_record[ids].ToArray();
            for (int i = 0; i < info.Length; i++)
            {
                DispatchModuleEvent(EventChatFriendChange, info[i], ids);
            }
        }
        friend_chat_change = false;
    }

    #region 好友聊天

    public void SendFriendMessage(string content, int type, ulong playerid, int tag = 0)//tag=0 好友之间私聊 1组队邀请显示 2组队之间聊天不显示 3 协作邀请 4 公会悬赏消息
    {
        if (modulePlayer.BanChat == 2)
        {
            if (tag == 0 || tag == 2)
            {
                moduleGlobal.ShowMessage(630, 0);
                return;
            }
        }

        if (playerid == 0)
        {
            if (moduleAwakeMatch.IsInTeam)
            {
                CsChatPrivate m = PacketObject.Create<CsChatPrivate>();
                m.content = content;
                m.type = (sbyte)type;
                m.recvId = playerid;
                m.tag = tag.ToString();
                DispatchModuleEvent(EventChatSendTeamMes, m.Clone());
                team_chat_record.Enqueue(m);
                m.Destroy();
            }
            else Logger.LogError("target playerid is 0");
            return;
        }

        if (tag != 2)
        {
            bool have = Friend_Id_key.Exists(a => a == playerid);
            if (!have)
            {
                Friend_Id_key.Add(playerid);//不存在填进去
            }
            //我发了一条消息 所以是一条置顶自己发的加入到聊天记录
            Late_reset(playerid);
            AddSelfChat(type, content, playerid, tag.ToString());
        }

        CsChatPrivate p = PacketObject.Create<CsChatPrivate>();
        p.content = content;
        p.type = (sbyte)type;
        p.recvId = playerid;
        p.tag = tag.ToString();
        if (moduleAwakeMatch.IsInTeam || tag == 1 || tag == 2)
        {
            DispatchModuleEvent(EventChatSendTeamMes, p);
            team_chat_record.Enqueue(p);
        }

        var black = moduleFriend.BlackList.Exists(a => a.roleId == playerid);
        if (!black) Send(p);

        //playerid 在哪个好友的聊天记录里 type 发送的内容形式 matter 发送的具体内容 send_id 是我发的还是对方发的
        //Record(playerid.ToString(), type.ToString(), content, modulePlayer.roleInfo.roleId.ToString());
    }

    void _Packet(ScChatPrivate p)
    {
        var black = moduleFriend.BlackList.Exists(a => a.roleId == p.sendId);
        if (black) return;

        ScChatPrivate pp = p.Clone();
        var tag = Util.Parse<int>(p.tag);
        if (tag == 1 || tag == 3)
        {
            //组队邀请图标出现
            moduleHome.UpdateIconState(HomeIcons.Friend, true, 1);
            //记录 存储为有组队信息
            moduleFriend.TeamHintCD = GeneralConfigInfo.defaultConfig.TeamHintCD;//重置时间
            moduleFriend.TimeHintShow = true;
            var team = moduleFriend.TeamInvate.Exists(a => a == pp.sendId);
            if (!team)
            {
                moduleFriend.TeamInvate.Add(pp.sendId);
                moduleFriend.FriendList = moduleFriend.SortWay();
            }
        }
        else if (tag == 2)
        {
            if (moduleAwakeMatch.IsTeamMember(pp.sendId)) team_chat_record.Enqueue(pp);
            m_newTeamMes = true;
            DispatchModuleEvent(EventChatRecTeamMes, pp);
            return;
        }

        moduleHome.UpdateIconState(HomeIcons.Friend, true);
        HavePastMes = true;

        ChatJilu(pp.sendId, pp);

        Late_reset(pp.sendId);//我收到一条消息 所以是一条置顶
        //playerid 在哪个好友的聊天记录里 type 发送的内容形式 matter 发送的具体内容 send_id 是我发的还是对方发的
        //moduleChat.Record(cc.ToString(), pp.type.ToString(), pp.content, cc.ToString());
        bool canaddlevel = past_mes.Exists(a => a == pp.sendId);
        if (!canaddlevel) past_mes.Add(pp.sendId);

        DispatchModuleEvent(EventChatRecFriendMes, pp);

    }

    public void ChatJilu(ulong cc, ScChatPrivate p)
    {
        bool is_cun = moduleChat.friend_Id_key.Exists(a => a == cc);
        if (!is_cun)
        {
            friend_Id_key.Add(cc);//不存在填进去
            Queue<ScChatPrivate> privateinfo = new Queue<ScChatPrivate>();
            privateinfo.Enqueue(p);
            friend_chat_record.Add(cc, privateinfo);
        }
        else
        {
            Queue<ScChatPrivate> outpinfo;
            friend_chat_record.TryGetValue(cc, out outpinfo);
            if (outpinfo != null)
            {
                friend_chat_record.Remove(cc);
                if (outpinfo.Count >= ChatNum) outpinfo.Dequeue();
                outpinfo.Enqueue(p);
                friend_chat_record.Add(cc, outpinfo);
            }
        }
    }

    public void AddSelfChat(int type, string content, ulong chatId, string tag)
    {
        ScChatPrivate player = SetPrivate(modulePlayer.id_, type, content, tag);

        bool is_cun = moduleChat.Friend_Id_key.Exists(a => a == chatId);
        Queue<ScChatPrivate> aa = new Queue<ScChatPrivate>();
        if (is_cun)
        {
            moduleChat.Friend_chat_record.TryGetValue(chatId, out aa);
            if (aa != null)
            {
                moduleChat.Friend_chat_record.Remove(chatId);
                if (aa.Count >= ChatNum) aa.Dequeue();
                aa.Enqueue(player);
            }
            else
            {
                aa = new Queue<ScChatPrivate>();
                aa.Enqueue(player);
            }
        }
        else aa.Enqueue(player);

        moduleChat.Friend_chat_record.Add(chatId, aa);
    }
    
    public ScChatPrivate SetPrivate(ulong id, int type, string cont, string tag)
    {
        ScChatPrivate player = PacketObject.Create<ScChatPrivate>();
        player.sendId = id;
        player.type = (sbyte)type;
        player.content = cont;
        player.tag = tag;
        return player;
    }
    #endregion

    #endregion

    public void HideChatWindow()
    {
        DispatchModuleEvent(EventChatWindowHide);
    }

    #region 链接聊天服务器

    void _Packet(ScWorldServerInfo p)
    {
        moduleFriend.GetAllChatInfo = true;
        m_serverRoleId = 0;
        if (p.roleId == 0) Logger.LogError("roleid is {0}", p.roleId);
        m_serverRoleId = p.roleId;
        Connect(p);
    }

    public void WorldSession()
    {
        CsWorldSession p = PacketObject.Create<CsWorldSession>();
        p.roleId = m_serverRoleId;
        Send(p);
    }

    void _Packet(ScWorldSession p)
    {
        if (p.code == 1)
        {
            chatContentTimes++;
            if (chatContentTimes > 15)
            {
                DelayEvents.Remove("chat_content");
                Logger.LogError("chat packet can not content !!");
            }
            else if (chatContentTimes > 6)
            {
                DelayEvents.Add(() => { WorldSession(); }, 60f, "chat_content");
            }
            else
            {
                DelayEvents.Add(() => { WorldSession(); }, 1f, "chat_content");
            }
        }
        else if (p.code == 0)
        {
            Logger.LogChat("Connect chat server succed!");
            chatContentTimes = 0;
            if (moduleFriend.GetAllChatInfo)
            {
                moduleChat.GetLateInfo();
                moduleChat.GetServerMes();
            }
        }
        else Logger.LogError("Connect chat server info is error");
    }

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();

        if (!m_useGameSession) Disconnect();

        chatContentTimes = 0;
        DelayEvents.Remove("chat_content");
    }
    #endregion

    #region Session

    public new static Module_Chat instance { get { return m_instance; } }

    private static Module_Chat m_instance = null;

    private ScWorldServerInfo m_server = null;

    private Module_Chat() : base(true)
    {
        if (m_instance != null)
            throw new Exception("Can not create " + GetType().Name + " twice.");

        m_instance = this;

        pingInterval = 20.0f;
    }

    protected override void OnModuleCreated()
    {
        m_server = PacketObject.Create<ScWorldServerInfo>();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        m_instance = null;
    }

    #endregion

    #region Net

    private bool m_useGameSession = false;

    public void Connect(ScWorldServerInfo server)
    {
        if (server == null)
        {
            Logger.LogError("Module_Chat::Connect: Invalid server!");
            return;
        }

        server.CopyTo(m_server);

        UpdateServer(m_server.host, m_server.port);

        session.RemoveEventListener(Events.SESSION_LOST_CONNECTION, OnLostConnection);

        m_useGameSession = false;
        if (host == session.host && port == session.port)
        {
            m_useGameSession = true;

            session.AddEventListener(Events.SESSION_LOST_CONNECTION, OnLostConnection);
            OnConnected();
        }
        else Connect();
    }

    public override void Send(PacketObject packet)
    {
        if (m_useGameSession) session.Send(packet);
        else base.Send(packet);
    }

    protected override void OnConnected()
    {
        Logger.LogChat("Connected to chat server [{0}:{1}], use game session: {2}", host, port, m_useGameSession);

        WorldSession();
        if (!m_useGameSession) SendPing();
    }

    protected override void OnLostConnection()
    {
        m_useGameSession = false;
        if (!moduleChat.connected && moduleLogin.ok) WorldSession();

        session.RemoveEventListener(Events.SESSION_LOST_CONNECTION, OnLostConnection);

        DispatchModuleEvent(EventChatLostConnection);
    }

    void _Packet(ScPing p)
    {
        if (p.type != 3) return;

        ping = Level.realTime - m_lastPing;
        if (!m_useGameSession) m_waitPing = pingInterval;
    }

    #endregion
}
