/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-07-25
 * 
 ***************************************************************************************************/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Module_Friend : Module<Module_Friend>
{
    #region Event

    /// <summary>
    /// 自己同意别人申请成功
    /// </summary>
    public const string EventFriendAgree = "EventFriendAgree";
    /// <summary>
    /// 自己拒绝别人申请
    /// </summary>
    public const string EventFriendResufed = "EventFriendResufed";

    public const string EventFriendDelete = "EventFriendDelete";
    public const string EventFriendDeletedReplay = "EventFriendDeletedReplay";
    public const string EventAddFriendReply = "EventAddFriendReply";
    public const string EventFriendSearch = "EventFriendSearch";
    public const string EventFriendRecommend = "EventFriendRecommend";
    public const string EventFriendDetails = "EventFriendDetails";
    public const string EventFriendApplyList = "EventFriendApplyList";
    public const string EventFriendAllList = "EventFriendAllList";
    public const string EventFreindISonlie = "EventFreindISonlie";
    public const string EventFreindHintistrue = "EventFreindHintistrue";
    public const string EventFriendAddApply = "EventFriendAddApply";

    #endregion

    #region List or Data
    /// <summary>
    /// 是否从服务器获取所有聊天信息
    /// </summary>
    public bool GetAllChatInfo;
    /// <summary>
    /// 已经申请过的id
    /// </summary>
    public List<ulong> AddApplyID = new List<ulong>();
    /// <summary>
    /// 好友列表
    /// </summary>
    public List<PPlayerInfo> FriendList = new List<PPlayerInfo>();
    /// <summary>
    /// 申请列表
    /// </summary>
    public List<PPlayerInfo> Apply_playerList = new List<PPlayerInfo>();
    /// <summary>
    /// 随机列表
    /// </summary>
    public List<PPlayerInfo> recommend = new List<PPlayerInfo>();

    public List<PPlayerInfo> Recommend { get { return recommend; } }

    public List<PPlayerInfo> SearchList { get { return searchList; } }

    Dictionary<ulong, ulong> roleID_player = new Dictionary<ulong, ulong>();//key是roleid value是applyid
    Dictionary<ulong, PPlayerInfo> apply_player_player = new Dictionary<ulong, PPlayerInfo>();//key是applyid
    Dictionary<ulong, PPlayerInfo> My_FriendList = new Dictionary<ulong, PPlayerInfo>(); //我所有好友的通过id查找
    List<ulong> apply_player_id = new List<ulong>(); //好友申请列表； 这个id为applyid
    List<PPlayerInfo> searchList = new List<PPlayerInfo>(); //搜索是否成功的列表

    /// <summary>
    /// 当前选中的玩家
    /// </summary>
    public PPlayerInfo checkPlayer { get; set; }
    /// <summary>
    /// 上次选中的玩家
    /// </summary>
    public PPlayerInfo lastCheckPlayer { get; set; }
    public bool windowisopen { get; set; }

    public int CanInviteUnion = -1;

    public OpenFriendType m_friendOpenType { get; set; }

    public int FriendNumTop = 80;

    public bool RefreshLateState = false;

    public ulong ApplyPlayerID;

    #endregion

    public PPlayerInfo GetFriendInfo(ulong roleId)
    {
        return FriendList.Find(item => item.roleId == roleId);
    }

    public bool IsFriend(ulong roleId)
    {
        return FriendList.Exists(item => item.roleId == roleId);
    }

    #region 全部好友 申请(离线在线)

    public void SendFriendApplyList()
    {
        var p = PacketObject.Create<CsFriendApplyList>();
        session.Send(p);
    }

    public void SendFriendList()
    {
        var p = PacketObject.Create<CsFriendList>();
        session.Send(p);
    }

    void _Packet(ScFriendApplyList p)
    {
        PApplyInfo[] pp = p.Clone().applyList;

        Apply_playerList.Clear();
        apply_player_id.Clear();
        apply_player_player.Clear();
        roleID_player.Clear();
        if (pp.Length > 0)//如果传进来的长度大于0
        {
            for (int i = 0; i < pp.Length; i++)
            {
                bool iseaual = false;

                for (int j = 0; j < Apply_playerList.Count; j++)
                {
                    if (p.applyList[i].playerInfo.roleId == Apply_playerList[j].roleId) iseaual = true;
                }
                if (!iseaual) AddApply(pp[i]);
            }
            Apply_playerList.Reverse();//按接收顺序翻转所以就会是最早的在第一个
            DispatchModuleEvent(EventFriendApplyList);
        }
        FriendHintShow();
    }

    void _Packet(ScFriendApplyDetail p)
    {
        ScFriendApplyDetail pp = p.Clone();
        if (pp.applyInfo == null || pp.applyInfo?.playerInfo == null) return;
        var black = BlackList.Exists(a => a.roleId == pp.applyInfo.playerInfo.roleId);
        if (black) return;

        PPlayerInfo apply = Apply_playerList.Find(a => a.roleId == pp.applyInfo.playerInfo.roleId);
        if (apply != null) RemoveApply(apply);
        AddApply(pp.applyInfo);
        Apply_playerList.Reverse();//按接收顺序翻转所以就会是最早的在第一个
        DispatchModuleEvent(EventFriendApplyList);

        if (moduleFriend.FriendList.Count < FriendNumTop) moduleHome.UpdateIconState(HomeIcons.Friend, true);
    }

    private void RemoveApply(PPlayerInfo pp)
    {
        var t = pp.roleId;
        if (roleID_player.ContainsKey(pp.roleId)) t = roleID_player[pp.roleId];
        var p = Apply_playerList.Find(a => a.roleId == pp.roleId);
        if (p != null) Apply_playerList.Remove(p);
        apply_player_id.Remove(t);
        apply_player_player.Remove(t);
        roleID_player.Remove(pp.roleId);
    }
    private void AddApply(PApplyInfo pp)
    {
        Apply_playerList.Add(pp.playerInfo);
        apply_player_id.Add(pp.applyId);
        apply_player_player.Add(pp.applyId, pp.playerInfo);
        roleID_player.Add(pp.playerInfo.roleId, pp.applyId);
    }

    void _Packet(ScFriendList p)
    {
        PPlayerInfo[] pp = p.Clone().friends;

        FriendList.Clear();
        My_FriendList.Clear();
        bool idhand = false;
        if (p.friends.Length > 0)
        {
            for (int i = 0; i < p.friends.Length; i++)
            {
                for (int j = 0; j < FriendList.Count; j++)
                {
                    if (p.friends[i].roleId == FriendList[j].roleId) idhand = true;
                }
                if (!idhand)
                {
                    FriendList.Add(pp[i]);//我的好友列表
                    My_FriendList.Add(pp[i].roleId, pp[i]);// 我所有好友的通过id查找);
                }
            }
        }
        FriendHintShow();
        moduleFriend.FriendList = moduleFriend.SortWay();
        DispatchModuleEvent(EventFriendAllList);

        if (moduleChat.localMessage != null && moduleChat.Friend_Id_key.Count == 0) moduleChat.GetChatjilu();
    }

    #endregion

    #region 申请好友

    public bool CanAddPlayer(ulong PlayerId)
    {
        var black = moduleFriend.BlackList.Exists(a => a.roleId == PlayerId);
        if (black) moduleGlobal.ShowMessage(218, 69);
        return black;
    }

    public void SendAddMes(ulong playId)//发送添加请求
    {
        ApplyPlayerID = playId;
        var p = PacketObject.Create<CsFriendAddApply>();
        p.roleId = playId;
        bool is_apply = false;
        PPlayerInfo apply_add = Apply_playerList.Find(apply => apply.roleId == playId);

        if (apply_add != null) is_apply = true;
        if (is_apply)
        {
            SendReplyAgreeMes(playId);
            DispatchModuleEvent(EventFriendAddApply, 0);
        }
        else
        {
            session.Send(p);
            moduleGlobal.LockUI(string.Empty, 0.5f);
        }
        AddApplyID.Add(playId);

    }
    void _Packet(ScFriendAddApply p)
    {
        moduleGlobal.UnLockUI();
        if (p.result != 0) AddApplyID.Remove(ApplyPlayerID);

        if (p.result == 0) moduleGlobal.ShowMessage(218, 40);
        else if (p.result == 1) moduleGlobal.ShowMessage(218, 42);
        else if (p.result == 2) moduleGlobal.ShowMessage(218, 73);
        else if (p.result == 3) moduleGlobal.ShowMessage(218, 74);
        else moduleGlobal.ShowMessage(ConfigText.GetDefalutString(218, 41));

        DispatchModuleEvent(EventFriendAddApply, p.result);
    }
    #endregion

    #region 同意/拒绝好友申请 删除好友 被同意 被删除  

    public void SendReplyAgreeMes(ulong playId)
    {
        //发送同意申请
        if (!roleID_player.ContainsKey(playId)) return;
        var p = PacketObject.Create<CsFriendAgree>();
        p.applyId = roleID_player[playId];
        session.Send(p);
    }
    void _Packet(ScFriendAgree p)
    {
        //同意结果
        ScFriendAgree pp = p.Clone();
        if (pp.result == 0)
        {
            if (pp.friend_ == null) return;
            var apply = Apply_playerList.Find(a => a.roleId == pp.friend_.roleId);
            if (apply != null) RemoveApply(apply);

            //添加好友信息 每当我在申请列表里统一的时候
            FriendList.Add(pp.friend_);//好友的详细信息加进去了
            My_FriendList.Add(pp.friend_.roleId, pp.friend_);//把这个信息加进去

            ulong idid = pp.friend_.roleId;
            ScChatPrivate hjhj = moduleChat.SetPrivate(pp.friend_.roleId, 0, ConfigText.GetDefalutString(218, 44), "0");

            //填进聊天记录
            moduleChat.ChatJilu(idid, hjhj);//添加入friend_Id_key和friend_chat_recor

            if (modulePlayer.BanChat != 2)
                moduleChat.SendFriendMessage(hjhj.content, hjhj.type, pp.friend_.roleId);//发一条消息

            moduleChat.Late_reset(idid);//这里添加到最近里
            moduleFriend.FriendList = moduleFriend.SortWay();
            DispatchModuleEvent(EventFriendAgree, pp.friend_);
        }
        else if (pp.result == 1 || pp.result == 4)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(218, 52));
            SendReplyRefusedMes(p.applyId);
        }
        else if (pp.result == 2) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(218, 13));
        else if (pp.result == 3) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(218, 53));
        else moduleGlobal.ShowMessage(ConfigText.GetDefalutString(218, 44));
    }
    void _Packet(ScFriendAdd p)
    {
        //被同意申请
        bool is_myfriend = false;

        for (int i = 0; i < FriendList.Count; i++)
        {
            if (p.friend_.roleId == FriendList[i].roleId) is_myfriend = true;
        }
        if (!is_myfriend)
        {
            PPlayerInfo add = p.friend_.Clone();
            FriendList.Add(add);//好友的详细信息加进去了
            My_FriendList.Add(add.roleId, add);//把这个信息加进去
            FriendList = moduleFriend.SortWay();
            DispatchModuleEvent(EventAddFriendReply);
        }
    }

    public void SendReplyRefusedMes(ulong playId)
    {
        if (!roleID_player.ContainsKey(playId)) return;
        var p = PacketObject.Create<CsFriendRefuse>();
        p.applyId = roleID_player[playId];
        session.Send(p);
    }
    void _Packet(ScFriendRefuse p)//拒绝申请
    {
        if (p.target == 0)
        {
            ScFriendRefuse pp = p.Clone();
            for (int i = 0; i < apply_player_id.Count; i++)
            {
                if (pp.applyId == apply_player_id[i])
                {
                    if (!apply_player_player.ContainsKey(pp.applyId)) return;
                    PPlayerInfo infos = Apply_playerList.Find(a => a.roleId == apply_player_player[pp.applyId].roleId);
                    if (infos == null) Logger.LogError("Apply list not have this id {0}", pp.applyId);
                    RemoveApply(infos);
                }
            }
            DispatchModuleEvent(EventFriendResufed);
        }
        else if (p.target == 1)
        {
            if (AddApplyID.Exists(a => a == p.applyId)) AddApplyID.Remove(p.applyId);
        }
    }
    
    public void SendDeleteMes(ulong playId)
    {
        var p = PacketObject.Create<CsFriendDelete>();
        p.roleId = playId;
        session.Send(p);
    }
    void _Packet(ScFriendDelete p)//删除结果
    {
        if (p.result == 0) DeleteFriend(p.roleId, 0);
        else if (p.result == 1) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(218, 43));
    }
    void _Packet(ScFriendRemove p)//被删除
    {
        DeleteFriend(p.roleId, 1);
    }
    private void DeleteFriend(ulong roleid, int type, PPlayerInfo info = null) //删除/被删
    {
        //0 删除 1 被删 
        var point = AllFriendPoint.Exists(a => a == roleid);
        if (point) AllFriendPoint.Remove(roleid);

        bool applyAdd = AddApplyID.Exists(a => a == roleid);
        if (applyAdd) AddApplyID.Remove(roleid);

        PlayerPrefs.DeleteKey(roleid.ToString());//删除跟这个好友的本地聊天记录

        bool friend = false;
        bool late = false;
        PPlayerInfo deletINfo = FriendList.Find(a => a.roleId == roleid);
        if (deletINfo != null)
        {
            friend = true;
            FriendList.Remove(deletINfo);
        }
        PPlayerInfo have = moduleChat.Late_ListAllInfo.Find(a => a.roleId == roleid);
        if (have != null)
        {
            late = true;
            moduleChat.Late_ListAllInfo.Remove(have);
        }

        bool z = moduleChat.Friend_Id_key.Exists(a => a == roleid);
        if (z)
        {
            moduleChat.Friend_chat_record.Remove(roleid);
            moduleChat.Friend_Id_key.Remove(roleid);
        }

        PlayerPrefs.DeleteKey(roleid.ToString());
        if (My_FriendList.ContainsKey(roleid)) My_FriendList.Remove(roleid);//把这个信息删除

        if (type == 0) DispatchModuleEvent(EventFriendDelete, deletINfo, friend, late);
        else if (type == 1) DispatchModuleEvent(EventFriendDeletedReplay, deletINfo, friend, late);
        else if (type == 2) DispatchModuleEvent(EventFriendAddBlack, info, friend, late);
        //如果自己被删除拉黑或者删除拉黑好友时候进行悬赏任务删除
        moduleUnion.RemoveClaimsInfo(1, roleid);
    }

    #endregion

    #region 搜索 随机 

    public void SendSwitchMes()
    {
        var p = PacketObject.Create<CsFriendRecommend>();
        session.Send(p);
    }

    public void SendSelectMes(string playerKey)
    {
        // index 或者 name
        var p = PacketObject.Create<CsFriendSearch>();
        p.keyword = playerKey;
        session.Send(p);
    }

    void _Packet(ScFriendSearch p)
    {
        ScFriendSearch pp = p.Clone();
        searchList.Clear();
        //接收从服务器传进来的是否搜索成功
        int isresult = p.result;
        if (p.playerInfo != null)
        {
            searchList.Add(pp.playerInfo);
        }
        DispatchModuleEvent(EventFriendSearch, isresult, pp.playerInfo);
    }

    void _Packet(ScFriendRecommend p)
    {
        ScFriendRecommend pp = p.Clone();
        recommend.Clear();
        for (int i = 0; i < pp.playerInfo.Length; i++)
        {
            if (pp.playerInfo[i] == null) continue;
            var black = BlackList.Find(a => a.roleId == pp.playerInfo[i].roleId);
            if (black != null) continue;
            recommend.Add(pp.playerInfo[i]);
        }

        DispatchModuleEvent(EventFriendRecommend);
    }

    #endregion

    #region _Packet

    public void SendLookDetails(ulong playerId)
    {
        PPlayerInfo info = PlayerInfo(playerId);
        if (info != null) DispatchModuleEvent(EventFriendDetails, info);
        else
        {
            var p = PacketObject.Create<CsFriendStateView>();
            p.roleId = playerId;
            session.Send(p);
        }
    }
    void _Packet(ScFriendStateView p)//接收玩家详情
    {
        var applyinfo = Apply_playerList.Find(apply => apply.roleId == p.roleId);
        if (applyinfo == null) applyinfo = moduleChat.LatePlayerInfo(p.roleId);
        if (applyinfo != null)
        {
            applyinfo.state = p.state;
            DispatchModuleEvent(EventFriendDetails, applyinfo);
        }
    }

    void _Packet(ScSystemMailList p)
    {
        for (int i = 0; i < p.mailList.Length; i++)
        {
            if (p.mailList[i].type == 3)
            {
                moduleHome.UpdateIconState(HomeIcons.Friend, true);
                DispatchModuleEvent(EventFreindHintistrue);
                break;
            }
        }
    }

    void _Packet(ScFriendStateChange p)//状态改变
    {
        for (int i = 0; i < FriendList.Count; i++)
        {
            if (FriendList[i].roleId == p.roleId)
            {
                if (FriendList[i] == null) continue;
                FriendList[i].state = p.state;

                if (My_FriendList.ContainsKey(FriendList[i].roleId))
                    My_FriendList[FriendList[i].roleId].state = p.state;

                if (p.state == 0) FriendList[i].offLineTime = Util.GetTimeStamp(false, true);
            }
        }
        for (int i = 0; i < moduleChat.Late_ListAllInfo.Count; i++)
        {
            if (moduleChat.Late_ListAllInfo[i].roleId == p.roleId)
            {
                moduleChat.Late_ListAllInfo[i].state = p.state;
            }
        }

        moduleFriend.FriendList = moduleFriend.SortWay();
        DispatchModuleEvent(EventFreindISonlie);
    }

    #endregion

    #region sort  好友列表的排序

    public float TeamHintCD;//收到邀请显示的时间
    public bool TimeHintShow;
    public List<ulong> TeamInvate = new List<ulong>();//组队邀请

    public List<PPlayerInfo> SortWay()
    {
        //未读消息 moduleChat.Past_mes
        //未领取的友情点 AllFriendPoint
        List<PPlayerInfo> newlist = new List<PPlayerInfo>();//排序好的好友列表信息

        var team = PastList(TeamInvate, newlist, true);
        var normal = PastList(TeamInvate, newlist, false);
        newlist.AddRange(team);
        newlist.AddRange(normal);

        var past = PointList(moduleChat.Past_mes, newlist, true);
        var now = PointList(moduleChat.Past_mes, newlist, false);
        newlist.AddRange(past);
        newlist.AddRange(now);

        var point = LineInfo(AllFriendPoint, newlist, true);
        var npoint = LineInfo(AllFriendPoint, newlist, false);
        newlist.AddRange(point);
        newlist.AddRange(npoint);

        var FriendListOnline = OnlineList(FriendList, newlist, true);
        var FriendListNotline = OnlineList(FriendList, newlist, false);
        newlist.AddRange(FriendListOnline);
        newlist.AddRange(FriendListNotline);

        return newlist;
    }
    private List<PPlayerInfo> PastList(List<ulong> list, List<PPlayerInfo> newlist, bool past)
    {
        List<PPlayerInfo> pList = new List<PPlayerInfo>();
        var iList = IdList(list, newlist, moduleChat.Past_mes, past);
        var have = PointList(iList, newlist, true);
        var nhave = PointList(iList, newlist, false);

        pList.AddRange(have);
        pList.AddRange(nhave);
        return pList;
    }

    private List<PPlayerInfo> PointList(List<ulong> list, List<PPlayerInfo> newlist, bool have)
    {
        List<PPlayerInfo> point = new List<PPlayerInfo>();
        var ipoint = IdList(list, newlist, AllFriendPoint, have);
        var online = LineInfo(ipoint, newlist, true);
        var fline = LineInfo(ipoint, newlist, false);

        point.AddRange(online);
        point.AddRange(fline);
        return point;
    }

    private List<ulong> IdList(List<ulong> list, List<PPlayerInfo> newlist, List<ulong> elist, bool have)
    {
        List<ulong> iList = new List<ulong>();
        for (int i = 0; i < list.Count; i++)
        {
            var s = newlist.Find(a => a.roleId == list[i]);
            if (s != null) continue;
            var info = moduleFriend.FriendList.Find(a => a.roleId == list[i]);
            if (info == null) continue;
            var p = elist.Exists(a => a == list[i]);
            if (have && p) iList.Add(info.roleId);
            else if (!have && !p) iList.Add(info.roleId);
        }
        return iList;
    }

    private List<PPlayerInfo> LineInfo(List<ulong> list, List<PPlayerInfo> newlist, bool onLine)
    {
        List<PPlayerInfo> line = new List<PPlayerInfo>();//不在线好友详情列表

        for (int i = 0; i < list.Count; i++)
        {
            SetState(newlist, line, onLine, list[i]);
        }
        SortLineTime(line);
        return line;
    }
    private void SortLineTime(List<PPlayerInfo> line)//离线按时间和等级排序
    {
        for (int i = 0; i < line.Count - 1; i++)
        {
            for (int j = i + 1; j < line.Count; j++)
            {
                PPlayerInfo temp;
                if (line[i].offLineTime < line[j].offLineTime)
                {
                    temp = line[i];
                    line[i] = line[i + 1];
                    line[i + 1] = temp;
                }
                else if (line[i].offLineTime == line[j].offLineTime)
                {
                    if (line[i].level < line[j].level)
                    {
                        temp = line[i];
                        line[i] = line[i + 1];
                        line[i + 1] = temp;
                    }
                }
            }
        }
    }
    
    private List<PPlayerInfo> OnlineList(List<PPlayerInfo> list, List<PPlayerInfo> newlist, bool onLine)
    {
        List<PPlayerInfo> line = new List<PPlayerInfo>();//不在线好友详情列表

        for (int i = 0; i < list.Count; i++)
        {
            SetState(newlist, line, onLine, list[i].roleId);
        }
        SortLineTime(line);
        return line;
    }

    private void SetState(List<PPlayerInfo> newlist, List<PPlayerInfo> list, bool line, ulong ids)
    {
        var s = newlist.Find(a => a.roleId == ids);
        if (s != null) return;
        var info = moduleFriend.FriendList.Find(a => a.roleId == ids);
        if (info == null) return;
        if (info.state > 0 && line) list.Add(info);
        else if (info.state == 0 && !line) list.Add(info);
    }

    #endregion

    #region 查询好友是否拥有公会

    public void CheckFriendUnion(ulong roleId)
    {
        CsFriendUnion p = PacketObject.Create<CsFriendUnion>();
        p.roleId = roleId;
        session.Send(p);
    }
    void _Packet(ScFriendUnion p)//0 对方可以加工会 1 有工会或者在保护时间 2 在cd 3 等级不够不能加
    {
        CanInviteUnion = p.result;
    }

    #endregion

    #region 友情点
    public const string EventFriendSendPoint = "EventFriendSendPoint";
    public const string EventFriendGetPoint = "EventFriendGetPoint";
    public const string EventFriendRecPoint = "EventFriendRecPoint";
    public const string EventFriendResetPoint = "EventFriendResetPoint";
    /// <summary>
    /// 所有收到的友情点
    /// </summary>
    public List<ulong> AllFriendPoint = new List<ulong>();
    /// <summary>
    /// 已经赠送过的好友
    /// </summary>
    public List<ulong> AlreadySendFriend = new List<ulong>();

    public int offlineGetFriendPoint { get; set; }

    private void GetAllFriendPoint()
    {
        //获取所有互赠的友情点(跟助战完全没有关系)
        CsFriendAllPoint p = PacketObject.Create<CsFriendAllPoint>();
        session.Send(p);
    }

    private void GetAllAssistFriendPoint()
    {
        var p = PacketObject.Create<CsAssistFriendPoint>();
        session.Send(p);
    }

    private void _Packet(ScAssistFriendPoint msg)
    {
        offlineGetFriendPoint = msg.points;
    }

    void _Packet(ScFriendAllPoint p)
    {
        //所有获得的
        var pp = p.Clone();
        AllFriendPoint.Clear();
        AlreadySendFriend.Clear();
        AllFriendPoint.AddRange(pp.pointInfo);
        AlreadySendFriend.AddRange(pp.friendId);
        FriendHintShow();
        moduleFriend.FriendList = moduleFriend.SortWay();
    }
    public void SendFriendPoint(ulong friendId)
    {
        //赠送友情点
        CsFriendPoint p = PacketObject.Create<CsFriendPoint>();
        p.friendId = friendId;
        session.Send(p);
    }
    void _Packet(ScFriendPoint p)
    {
        if (p.result == 0)
        {
            if (p.pointInfo == null) return;
            PFriendPoint a = p.pointInfo.Clone();
            var have = AlreadySendFriend.Exists(o => o == a.playerId);
            if (!have) AlreadySendFriend.Add(a.playerId);

            moduleGlobal.ShowMessage(string.Format(ConfigText.GetDefalutString(218, 58), a.point));
            DispatchModuleEvent(EventFriendSendPoint, a.playerId);
        }
        else if (p.result == 1) moduleGlobal.ShowMessage(218, 60);
        else if (p.result == 2) moduleGlobal.ShowMessage(218, 61);
    }
    public void GetFriendPoint(ulong friendId)
    {
        //获取收到的友情点
        CsFriendPointGet p = PacketObject.Create<CsFriendPointGet>();
        p.friendId = friendId;
        session.Send(p);
    }
    void _Packet(ScFriendPointGet p)
    {
        if (p.result == 0)
        {
            if (p.pointInfo == null) return;
            var pId = p.pointInfo.playerId;
            var num = p.pointInfo.point;
            var have = AllFriendPoint.Exists(a => a == pId);
            if (have) AllFriendPoint.Remove(pId);
            moduleGlobal.ShowMessage(string.Format(ConfigText.GetDefalutString(218, 59), num));
            moduleFriend.FriendList = moduleFriend.SortWay();
            DispatchModuleEvent(EventFriendGetPoint, pId);
        }
        else if (p.result == 1) moduleGlobal.ShowMessage(218, 62);

    }
    void _Packet(ScFriendPointReceived p)
    {
        var have = AllFriendPoint.Exists(a => a == p.friendId);
        if (!have) AllFriendPoint.Add(p.friendId);

        FriendHintShow();
        moduleFriend.FriendList = moduleFriend.SortWay();
        DispatchModuleEvent(EventFriendRecPoint, p.friendId);
    }
    void _Packet(ScFriendTimeReset p)
    {
        AlreadySendFriend.Clear();
        moduleFriend.FriendList = moduleFriend.SortWay();
        DispatchModuleEvent(EventFriendResetPoint);
    }
    #endregion

    #region 同步好友信息

    void _Packet(ScFriendInfoSync p)
    {
        bool sort = false;
        for (int i = 0; i < p.friendList.Length; i++)
        {
            var info = p.friendList[i];
            if (info == null) continue;
            var fInfo = FriendList.Find(a => a.roleId == info.friendId);
            if (fInfo != null)
            {
                if (fInfo.level != (sbyte)info.level) sort = true;

                fInfo.level = (sbyte)info.level;
                fInfo.name = info.name;
                fInfo.headBox = info.headBox;
                fInfo.intro = info.sign;
            }
            if (modulePlayer.roleInfo?.leagueID != 0)
            {
                var uInfo = moduleUnion.m_unionPlayer.Find(a => a.info?.roleId == info.friendId);
                if (uInfo != null && uInfo?.info != null)
                {
                    uInfo.info.level = (sbyte)info.level;
                    uInfo.info.name = info.name;
                    uInfo.info.headBox = info.headBox;
                    uInfo.info.intro = info.sign;
                }
            }
        }
        if (sort) moduleFriend.FriendList = moduleFriend.SortWay();
    }
    #endregion

    #region 黑名单
    public const string EventFriendAllBlack = "EventFriendAllBlack";
    public const string EventFriendAddBlack = "EventFriendAddBlack";
    public const string EventFriendRemoveBlack = "EventFriendRemoveBlack";
    /// <summary>
    /// 黑名单列表
    /// </summary>
    public List<PPlayerInfo> BlackList = new List<PPlayerInfo>();

    public void GetAllBlock()
    {
        //获取黑名单
        var p = PacketObject.Create<CsFriendBlackList>();
        session.Send(p);
    }

    void _Packet(ScFriendBlackList p)
    {
        var pp = p.Clone();
        BlackList.Clear();
        //黑名单列表
        if (pp.black == null) return;
        BlackList.AddRange(pp.black);
        BlackList.Sort((a, b) => b.level.CompareTo(a.level));
        moduleChat.LateBlackCheck();
        DispatchModuleEvent(EventFriendAllBlack);
    }

    public void SendShieDing(ulong playerId)
    {
        //发送要拉黑的人
        var p = PacketObject.Create<CsPlayerBlack>();
        p.playerId = playerId;
        session.Send(p);
    }
    void _Packet(ScPlayerBlack p)
    {
        var pp = p.Clone();
        //拉黑结果 成功时候进行删除好友操作 并添加入黑名单列表
        if (pp.result == 0)
        {
            moduleGlobal.ShowMessage(218, 75);
            if (pp.player == null) return;
            var black = BlackList.Find(a => a.roleId == pp.player.roleId);
            if (black == null) BlackList.Add(pp.player);
            var apply = Apply_playerList.Find(a => a.roleId == pp.player.roleId);
            if (apply != null)
            {
                if (roleID_player.ContainsKey(apply.roleId)) RemoveApply(apply);
            }

            BlackList.Sort((a, b) => b.level.CompareTo(a.level));
            DeleteFriend(pp.player.roleId, 2, pp.player);//删除好友
        }
        else moduleGlobal.ShowMessage("error");
    }

    void _Packet(ScSelfBeBlack p)//自己被拉黑了
    {
        DeleteFriend(p.playerId, 0);//清空聊天记录
    }

    public void RecoverShieDing(ulong playerId)
    {
        //拉出黑名单
        var p = PacketObject.Create<CsRecoverBlack>();
        p.playerId = playerId;
        session.Send(p);
    }
    void _Packet(ScRecoverBlack p)
    {
        if (p.result == 0)
        {
            moduleGlobal.ShowMessage(218, 76);
            var black = BlackList.Find(a => a.roleId == p.playerId);
            if (black != null) BlackList.Remove(black);
            DispatchModuleEvent(EventFriendRemoveBlack);
        }
    }

    #endregion

    public int PlayerType(ulong playerId)
    {
        int type = -1;
        var info = FriendList.Find(a => a.roleId == playerId);
        if (info != null) return 0;
        info = BlackList.Find(a => a.roleId == playerId);
        if (info != null) return 2;
        info = recommend.Find(a => a.roleId == playerId);
        if (info != null) return 3;
        info = searchList.Find(a => a.roleId == playerId);
        if (info != null) return 4;
        info = moduleChat.LatePlayerInfo(playerId);
        if (info != null) return 1;
        return type;
    }

    public PPlayerInfo PlayerInfo(ulong playerId)
    {
        //聊天页
        PPlayerInfo info = FriendList.Find(a => a.roleId == playerId);
        if (info == null) info = BlackList.Find(a => a.roleId == playerId);
        if (info == null) info = searchList.Find(a => a.roleId == playerId);
        if (info == null) info = recommend.Find(a => a.roleId == playerId);
        if (info == null) info = moduleChat.LatePlayerInfo(playerId);
        return info;
    }

    public void FriendHintShow()
    {
        if (moduleChat.Past_mes.Count <= 0 && (moduleFriend.Apply_playerList.Count <= 0 || moduleFriend.FriendList.Count >= FriendNumTop) && AllFriendPoint.Count <= 0)
            moduleHome.UpdateIconState(HomeIcons.Friend, false);
        else moduleHome.UpdateIconState(HomeIcons.Friend, true);
    }

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        windowisopen = false;
        TimeHintShow = false;
        RefreshLateState = false;
        BlackList.Clear();
        TeamHintCD = -1;
        CanInviteUnion = -1;
        AddApplyID.Clear();
        Apply_playerList.Clear();
        apply_player_id.Clear();
        apply_player_player.Clear();
        roleID_player.Clear();
        FriendList.Clear();
        My_FriendList.Clear();
        recommend.Clear();
        AllFriendPoint.Clear();
        AlreadySendFriend.Clear();
        moduleFriend.SendFriendApplyList(); 
        moduleFriend.SendFriendList();
        GetAllFriendPoint();
        GetAllAssistFriendPoint();
        GetAllBlock();
    }
}
