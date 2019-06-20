/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-11-08
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class Module_Set : Module<Module_Set>
{
    public const string EventHeadChange = "EventHeadChange";        //更换头像框消息
    public const string EventIntroudChange = "EventIntroudChange";  //更换个人介绍消息
    public const string EventNameChange = "EventNameChange";        //更换名字消息
    public const string EventSavePushState = "EventSavePushState";  //保存推送状态

    /// <summary>
    /// 账号绑定 param1 = code
    /// </summary>
    public const string EventBindAccount = "EventBindAccount";
    public const string EventBangAward = "EventBangAward";      //兑换奖励消息
    public const string EventHeadsInfo = "EventHeadsInfo";      //接收头像详细信息

    public bool account_IS { get; set; }//账号是否已经绑定    

    public bool changedname { get; set; }//是否更过名字

    public PPrice spend_num { get; set; }

    public short SelectBoxID { get; set; }//选中的头像框ID;

    public bool openChangeName = false;
    #region 头像框

    public List<PropItemInfo> AllheadItems { get { return allheadItems; } set { allheadItems = value; } }

    private List<PropItemInfo> allheadItems = new List<PropItemInfo>();

    public void GetAllHead()
    {
        AllheadItems.Clear();
        List<PropItemInfo> allItems = ConfigManager.GetAll<PropItemInfo>();
        for (int i = 0; i < allItems.Count; i++)
        {
            if (allItems[i].itemType == PropType.HeadAvatar)
            {
                AllheadItems.Add(allItems[i]);
            }
        }
    }
    #endregion

    public void GetReward(string code)
    {
        CsSystemCodeReward p = PacketObject.Create<CsSystemCodeReward>();
        p.code = code;
        session.Send(p);
    }
    void _Packet(ScSystemCodeReward p)
    {
        //11 中文礼包码 您已经领取过该礼包
        if (p.result == 1) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(221, 35));
        else if (p.result == 2) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(221, 61));
        else if (p.result == 3) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(221, 62));
        else if (p.result == 4) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(221, 63));
        else if (p.result == 5) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(221, 64));
        else if (p.result == 6) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(221, 65));
        else if (p.result == 7) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(221, 66));
        else if (p.result == 8) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(221, 67));
        else if (p.result == 9) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(221, 68));
        else if (p.result == 10) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(221, 71));
        else if (p.result == 11) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(221, 73));

        DispatchModuleEvent(EventBangAward, p.Clone().reward, p.result);
    }

    void _Packet(ScRoleInfo p)
    {
        changedname = p.role.updateNameTimes != 0;
    }

    void _Packet(ScRoleHeadChange p)
    {
        if (p.result == 0)
        {
            modulePlayer.roleInfo.headBox = SelectBoxID;
            DispatchModuleEvent(EventHeadChange);
            DispatchEvent(EventHeadChange);
        }
        else
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.SetUIText, 32));
    }

    public void Change_HeadKuang()//更换头像框
    {
        var p = PacketObject.Create<CsRoleHeadChange>();
        p.head = SelectBoxID;
        session.Send(p);
    }

    public void Change_Introud(string introus_str)//更换个人介绍
    {
        var p = PacketObject.Create<CsRoleIntroUpdate>();
        p.newIntro = introus_str;
        session.Send(p);
    }

    void _Packet(ScRoleIntroUpdate p)
    {
        DispatchModuleEvent(EventIntroudChange, p.result);
    }

    public void Change_Name(string name)//更换名字
    {
        var p = PacketObject.Create<CsRoleNameUpdate>();
        p.newName = name;
        session.Send(p);
    }

    void _Packet(ScRoleNameUpdate p)
    {
        if (p.result == 0)
        {
            changedname = true;
        }
        DispatchModuleEvent(EventNameChange, p.Clone().newName, p.result);
    }

    public void Bang_Acount(string account, string password, bool lockUI = true)//发送账号绑定消息
    {
        int id = (int)moduleLogin.account.acc_id;
        if (lockUI) moduleGlobal.LockUI("", 0.1f);

        WebRequestHelper.BindAcount(id, account, password, "", reply =>
        {
            if (lockUI) moduleGlobal.UnLockUI();

            if (reply.code == 0) account_IS = true;
            DispatchModuleEvent(EventBindAccount, reply.code);
        });
    }

    public string SetSignTxt(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return ConfigText.GetDefalutString(221, 72);
        }
        else return str;
    }

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        openChangeName = false;
    }

    #region push-module

    protected override void OnGameDataReset()
    {
        m_pushState.Clear();
        switchInfo = null;
    }

    public Dictionary<SwitchType, uint> pushState => m_pushState;
    private Dictionary<SwitchType, uint> m_pushState = new Dictionary<SwitchType, uint>();
    PSwitchRecord[] switchInfo = null;
    void _Packet(ScSystemSwitch p)
    {
        m_pushState.Clear();
        p.info.CopyTo(ref switchInfo);   
    }

    private void SetSwitchState()
    {
        if (switchInfo == null || switchInfo.Length < 1) return;
        for (int i = 0; i < switchInfo.Length; i++)
        {
            var _id = switchInfo[i].switchId;
            if (_id == 0) continue;

            var type = (SwitchType)_id;
            var state = switchInfo[i].value;
            if (m_pushState.ContainsKey(type))
            {
                m_pushState[type] = state;
                continue;
            }
            m_pushState.Add(type, state);

            if (type == SwitchType.UnionBoss)
                DispatchEvent(SDKEvent.GLOBAL, SDKEvent.PopSdk(SDKEvent.UNION_CHANGE, modulePlayer.roleInfo.leagueID, state));

            if (type == SwitchType.UnionBoss || type == SwitchType.SystemPao) continue;

            if (type == SwitchType.Labyrinth || type == SwitchType.SkillPoint || type == SwitchType.RoyalPvp)
            {
                var _type = type == SwitchType.Labyrinth ? HomeIcons.Labyrinth : type == SwitchType.RoyalPvp ? HomeIcons.Match : HomeIcons.Skill;
                var canIn = moduleGuide.IsActiveFunction(_type);
                if (!canIn) continue;
            }

            DispatchEvent(SDKEvent.GLOBAL, SDKEvent.PopSdk(SDKEvent.TAG, state, _id));
        }
    }

    void _Packet(ScGuideInfo p)
    {
        SetSwitchState();
    }

    public void ChangePushState(SwitchType _typeId, uint _state)
    {
        CsSystemChangeSwitch p = PacketObject.Create<CsSystemChangeSwitch>();
        p.switchId = (byte)_typeId;
        p.value = _state;
        session.Send(p);
    }

    void _Packet(ScSystemChangeSwitch p)
    {
        if (p.result == 0)
        {
            var type= (SwitchType)p.switchId;
            if (m_pushState.ContainsKey(type))
            {
                m_pushState[type] = p.value;

                if (type != SwitchType.UnionBoss && type != SwitchType.SystemPao)
                {
                    var canIn = true;
                    if (type == SwitchType.Labyrinth || type == SwitchType.SkillPoint || type == SwitchType.RoyalPvp)
                    {
                        var _type = type == SwitchType.Labyrinth ? HomeIcons.Labyrinth : type == SwitchType.RoyalPvp ? HomeIcons.Match : HomeIcons.Skill;
                        canIn = moduleGuide.IsActiveFunction(_type);
                    }
                    if (canIn) DispatchEvent(SDKEvent.GLOBAL, SDKEvent.PopSdk(SDKEvent.TAG, p.value, p.switchId));
                }

                if (type == SwitchType.Fatigue || type == SwitchType.SkillPoint)
                    DispatchEvent(SDKEvent.GLOBAL, SDKEvent.PopSdk(SDKEvent.LOCAL_NOTIFY, type));

                if (type == SwitchType.UnionBoss)
                    DispatchEvent(SDKEvent.GLOBAL, SDKEvent.PopSdk(SDKEvent.UNION_CHANGE, modulePlayer.roleInfo.leagueID, p.value));
            }
            DispatchModuleEvent(EventSavePushState);
        }
        else moduleGlobal.ShowMessage(Util.GetString(9212, 8));
    }

    #endregion
}
