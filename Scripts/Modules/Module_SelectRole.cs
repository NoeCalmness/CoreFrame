// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-02-23      10:12
//  *LastModify：2019-02-23      10:12
//  ***************************************************************************************************/

using System;
using UnityEngine;

public class Module_SelectRole : Module<Module_SelectRole>
{
    public const string Notice_SwitchRole     = "SwitchRoleEvent";
    public const string ChangeSelectRoleEvent = "ChangeSelectRoleEvent";
    public const string Response_DeleteRole   = "ResponseDeleteRole";
    public const string RoleListChangeEvent   = "RoleListChangeEvent";

    public PRoleSummary[] roleList;

    private ulong operatorRole = 0;

    public int RoleCount
    {
        get { return roleList?.Length ?? 0; }
    }

    public string lastEnterRoleSaveKey
    {
        get { return $"lastEnterRole-{Root.serverInfo.ID}-{moduleLogin.account.acc_name}"; }
    }

    public PRoleSummary this[int rIndex]
    {
        get { return roleList.GetValue<PRoleSummary>(rIndex); }
    }

    public int LastEnterRoleIndex
    {
        get
        {
            if (moduleLogin.account == null)
                return 0;
            if(PlayerPrefs.HasKey(lastEnterRoleSaveKey))
                return Mathf.Max(0, FindIndex(Util.Parse<ulong>(PlayerPrefs.GetString(lastEnterRoleSaveKey))));
            return 0;
        }
    }

    public void SetRoleSummary(PRoleSummary[] rArray)
    {
        if (rArray == null || rArray.Length == 0)
            return;
        rArray.CopyTo(ref roleList);
        Array.Sort(roleList, (a, b) => a.roleId.CompareTo(b.roleId));
    }

    public PRoleSummary GetRoleSummary(ulong rRoleId)
    {
        for (var i = 0; i < roleList?.Length; i++)
        {
            if (roleList[i].roleId == rRoleId)
                return roleList[i];
        }
        return null;
    }

    public ulong GetRoleIdByName(string rName)
    {
        for (var i = 0; i < roleList?.Length; i++)
        {
            if (string.Equals(roleList[i].name, rName))
                return roleList[i].roleId;
        }
        return 0;
    }

    public int FindIndex(ulong currentRoleId)
    {
        for (var i = 0; i < roleList?.Length; i++)
        {
            if (roleList[i].roleId == currentRoleId)
                return i;
        }
        return -1;
    }

    public void RoleStartEnterGame(int rIndex, bool isFirstEnter = false)
    {
        Game.WillEnterGame();

        rIndex = Mathf.Clamp(rIndex, 0, roleList.Length - 1);

        var p = PacketObject.Create<CsEnterGame>();
        p.roleId = roleList[rIndex].roleId;
        p.gameVersion = Launch.Updater.currentVersion;
        p.sourceHash  = AssetBundles.AssetManager.dataHash;
        p.machineNumber = SystemInfo.deviceUniqueIdentifier;
        p.producer = SystemInfo.deviceModel;
        session.Send(p);

        PlayerPrefs.SetString(lastEnterRoleSaveKey, p.roleId.ToString());
        PlayerPrefs.Save();

        if(!isFirstEnter)
            Game.GoHome();

        DispatchEvent(SDKEvent.GLOBAL, SDKEvent.PopSdk(SDKEvent.SELECT_ROLE, roleList[rIndex].roleId));
    }

    public void DeleteRole(ulong rRoleId)
    {
        moduleGlobal.LockUI();
        operatorRole = rRoleId;
        var p = PacketObject.Create<CsDropRole>();
        p.roleId = rRoleId;
        session.Send(p);
    }

    public void CreateRole()
    {
        Module_Story.ShowStory(1001, EnumStoryType.TheatreStory);
    }

    public void SwitchRole()
    {
        session.Send(PacketObject.Create<CsSwitchRole>());
    }

    void _Packet(ScDropRole p)
    {
        moduleGlobal.UnLockUI();
        if (p.result == 0)
        {
            var index = roleList.FindIndex(r => r.roleId == operatorRole);
            if (index != -1)
            {
                var arr = new PRoleSummary[roleList.Length - 1];
                var m = 0;
                for (var i = 0; i < roleList.Length; i++)
                {
                    if (i != index)
                        arr[m++] = roleList[i];
                    else
                        roleList[i].Destroy();
                }
                roleList = arr;
                DispatchEvent(RoleListChangeEvent);
            }
        }
        DispatchModuleEvent(Response_DeleteRole, p);
    }

    void _Packet(ScEnterGame p)
    {
        Game.EnterGame();
    }

    private void _Packet(ScSwitchRole msg)
    {
        Game.ResetGameData();

        SetRoleSummary(msg.roleList);
        moduleGlobal.Hidepao();

        DispatchEvent(Notice_SwitchRole);
    }

    public void OnCreateRoleSuccess(PRoleSummary role)
    {
        Array.Resize(ref roleList, RoleCount + 1);
        roleList[RoleCount - 1] = role;
        DispatchEvent(RoleListChangeEvent);
    }

    public void OnReturnSwitchRole()
    {
        moduleGlobal.LockUI(0.3f, 0, 0x02);
        moduleSelectRole.RemoveEventListener(Notice_SwitchRole, OnSwitchRole);
        moduleSelectRole.AddEventListener(Notice_SwitchRole, OnSwitchRole);

        moduleSelectRole.SwitchRole();
    }

    private void OnSwitchRole()
    {
        moduleGlobal.UnLockUI();
        Game.LoadLevel(GeneralConfigInfo.sroleLevel);
        moduleSelectRole.RemoveEventListener(Notice_SwitchRole, OnSwitchRole);
    }
}
