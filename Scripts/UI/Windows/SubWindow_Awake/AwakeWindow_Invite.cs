// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-07-31      16:22
//  * LastModify：2018-07-31      16:23
//  ***************************************************************************************************/

#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#endregion


public class AwakeWindow_Invite : SubWindowBase
{
    private ScrollView  scrollView;
    private Button      inviteButton;
    private Button      closeButton;

    private List<PPlayerInfo> Check = new List<PPlayerInfo>(5);//选中的

    protected override void InitComponent()
    {
        base.InitComponent();
        scrollView      = WindowCache.GetComponent<ScrollView>  ("invite_panel/person_frame/scrollView");
        inviteButton    = WindowCache.GetComponent<Button>      ("invite_panel/person_frame/invite_btn");
        closeButton     = WindowCache.GetComponent<Button>      ("invite_panel/person_frame/bg/close_btn");
    }

    public override bool Initialize(params object[] p)
    {
        if (base.Initialize(p))
        {
            var list = new List<SourceFriendInfo>();
            foreach (var rPlayer in moduleFriend.FriendList.FindAll(info => info.state != 0))
            {
                list.Add(rPlayer);
            }
            foreach (var rPlayer in moduleUnion.m_unionPlayer)
            {
                if (rPlayer.info.roleId == modulePlayer.id_)
                    continue;
                //筛选离线玩家
                if (rPlayer.info.state == 0)
                    continue;
                var sitem = list.Find(item => item.PlayerInfo.roleId == rPlayer.info.roleId);
                if (sitem == null)
                    list.Add(rPlayer.info);
            }
            new DataSource<SourceFriendInfo>(list, scrollView,
                OnSetData, OnClick);

            inviteButton    ?.onClick.AddListener(OnInviteClick);
            closeButton     ?.onClick.AddListener(() => UnInitialize());
            Check.Clear();
            return true;
        }
        return false;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (base.UnInitialize(hide))
        {
            inviteButton?.onClick.RemoveAllListeners();
            closeButton ?.onClick.RemoveAllListeners();
            return true;
        }
        return false;
    }

    private void OnInviteClick()
    {
        string s = ConfigText.GetDefalutString(9803);
        var content = Util.Format(s, 
            moduleAwakeMatch.CurrentTask.taskConfigInfo.name, 
            moduleAwakeMatch.CurrentTask.taskConfigInfo.ID, 
            modulePlayer.id_, 
            moduleAwakeMatch.roomId, 
            AssetBundles.AssetManager.dataHash,
            1);
        content = content.Replace('[', '{');
        content = content.Replace(']', '}');
        var num = 0;
        for (int i = 0; i < Check.Count; i++)
        {
            var roleId = Check[i].roleId;
            if (Check[i].state == 0)
            {
                moduleGlobal.ShowMessage(Util.Format(ConfigText.GetDefalutString(9851), Check[i].name));
                continue;
            }
            if (moduleAwakeMatch.InviteRecordDict.ContainsKey(roleId))
            {
                var record = moduleAwakeMatch.InviteRecordDict[roleId];
                var md = content.GetMD5();
                if (record.MD5.Equals(md))
                {
                    moduleGlobal.ShowMessage(9852);
                    continue;
                }
                record.MD5 = md;
            }
            else
                moduleAwakeMatch.InviteRecordDict.Add(roleId, new Module_AwakeMatch.InviteRecord(content.GetMD5()));
            moduleChat.SendFriendMessage(content, 0, roleId, 1);
            num ++;
        }
        if(num > 0)
            moduleGlobal.ShowMessage( Util.Format(ConfigText.GetDefalutString(9853) , num));
        UnInitialize();
    }

    private void OnSetData(RectTransform node, SourceFriendInfo data)
    {
        FriendPrecast Presct = node.gameObject.GetComponentDefault<FriendPrecast>();
        Presct.DelayAddData(data.PlayerInfo, 1);
        Presct.SetToggleState(Check.Exists(p => p.roleId == data.PlayerInfo.roleId));
        Presct.onToggle = (a, b) => { OnToggle(a.playerInfo, b); };
    }

    private void OnClick(RectTransform node, SourceFriendInfo data)
    {
        GameObject select = node.gameObject.transform.Find("selectbox").gameObject;
        if (select.activeInHierarchy)
        {
            select.gameObject.SetActive(false);
            Check.Remove(data.PlayerInfo);
        }
        else
        {
            if (Check.Count >= Check.Capacity)
            {
                moduleGlobal.ShowMessage(9810);
                return;
            }
            select.gameObject.SetActive(true);
            Check.Add(data.PlayerInfo);
        }
    }

    private void OnToggle( PPlayerInfo data, bool isOn)
    {
        if (!isOn)
        {
            Check.Remove(data);
        }
        else
        {
            if (Check.Count >= Check.Capacity)
            {
                moduleGlobal.ShowMessage(9810);
                return;
            }
            Check.Add(data);
        }
    }


    public override bool OnReturn()
    {
        if (this.isInit)
        {
            this.UnInitialize();
            return true;
        }
        return false;
    }

}

public enum CommonRelation
{
    Npc = 1,        //npc
    Friend,         //好友
    UnionMember,    //工会成员
    Stranger,       //陌生人
}

public interface ISourceItem
{
    CommonRelation Relation { get; }
    PPlayerInfo PlayerInfo { get; }
    INpcMessage NpcInfo { get; }

    int addPoint { get; }
}

public interface IScrollViewData<T>
{
    T GetItemData();
}

public class SourceFriendInfo : ISourceItem
{
    public CommonRelation Relation { get; protected set; }
    public PPlayerInfo PlayerInfo { get; set; }
    public virtual INpcMessage NpcInfo { get { return null; } }

    public virtual int addPoint { get { return 0; } }

    public SourceFriendInfo(PPlayerInfo rInfo)
    {
        if (rInfo != null)
        {
            Relation = Module_Friend.instance.IsFriend(rInfo.roleId)
                ? CommonRelation.Friend
                : Module_Union.instance.IsUnionMember(rInfo.roleId) ? CommonRelation.UnionMember : CommonRelation.Stranger;
        }
        PlayerInfo = rInfo;
    }

    public static implicit operator SourceFriendInfo(PPlayerInfo rInfo)
    {
        return new SourceFriendInfo(rInfo);
    }
}