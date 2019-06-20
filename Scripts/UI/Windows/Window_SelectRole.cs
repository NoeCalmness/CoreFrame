// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-02-25      11:19
//  *LastModify：2019-02-25      14:55
//  ***************************************************************************************************/

#region

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#endregion

public class Window_SelectRole : Window
{
    private int _currentIndex = -1;
    private ulong _currentRoleId;
    private Button confirmButton;
    private Button createButton;

    private SelectRole_Delete deletePanel;
    private Transform[] roleInfoBorad;
    private ToggleGroup toggleGroup;
    private Toggle[] toggles;

    public int CurrentIndex
    {
        get
        {
            return _currentIndex;
        }
        set
        {
            if (_currentIndex == value)
                return;

            if (value >= 0)
            {
                var roleInfo = moduleSelectRole.roleList.GetValue<PRoleSummary>(value);
                _currentRoleId = roleInfo?.roleId ?? 0;
            }
            else
                _currentRoleId = 0;
            moduleSelectRole.DispatchEvent(Module_SelectRole.ChangeSelectRoleEvent, Event_.Pop(_currentRoleId));
            _currentIndex = value;
        }
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        MultiLangrage();
        confirmButton    = GetComponent<Button>("confirmBtn");
        toggleGroup      = GetComponent<ToggleGroup>("charaterList");
        createButton     = GetComponent<Button>("creatCharacter/Btn");

        roleInfoBorad    = new Transform[toggleGroup.transform.childCount];
        for (var i = 0; i < roleInfoBorad.Length; i++)
            roleInfoBorad[i] = toggleGroup?.transform?.GetChild(i);

        toggles = new Toggle[roleInfoBorad.Length];
        for (var i = 0; i < toggles.Length; i++)
            toggles[i] = roleInfoBorad[i]?.GetComponent<Toggle>();

        deletePanel = SubWindowBase.CreateSubWindow<SelectRole_Delete>(this,
            GetComponent<Transform>("deletePanel")?.gameObject);

        confirmButton.onClick.AddListener(OnConfirmLogin);
        createButton.onClick.AddListener(OnCreateRoleClick);
        toggleGroup?.onAnyToggleStateOn.AddListener(OnAnyToggleStateOn);
    }

    private void MultiLangrage()
    {
        var ct = ConfigManager.Get<ConfigText>((int)TextForMatType.SelectRoleUI);
        Util.SetText(GetComponent<Text>("titleTxt")                                         , ct[0]);
        Util.SetText(GetComponent<Text>("confirmBtn/confirmTxt")                            , ct[1]);
        Util.SetText(GetComponent<Text>("deleteBtn/deleteTxt")                              , ct[2]);
        Util.SetText(GetComponent<Text>("deletePanel/changename/cnamesurefree/free_sure")   , ct[4]);
        Util.SetText(GetComponent<Text>("deletePanel/changename/cancel/sure")               , ct[5]);
        Util.SetText(GetComponent<Text>("deletePanel/changename/inputname/placeholder")     , ct[4]);
    }

    private void OnCreateRoleClick()
    {
        Window_Alert.ShowAlertDefalut(ConfigText.GetDefalutString(TextForMatType.SelectRoleUI, 8), () => { moduleSelectRole.CreateRole(); });
    }

    private void OnAnyToggleStateOn(Toggle rToggle)
    {
        CurrentIndex = Array.FindIndex(toggles, t => t == rToggle);
    }

    private void OnDeleteRole(int rIndex)
    {
        var role = moduleSelectRole[rIndex];
        if (role == null) return;
        deletePanel.Initialize(role.roleId);
    }

    private void OnConfirmLogin()
    {
        var role = moduleSelectRole[CurrentIndex];
        if (role == null) return;

        if (role.banState == 2)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.SelectRoleUI, 7));
            return;
        }

        moduleSelectRole.RoleStartEnterGame(CurrentIndex);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault(1, false);

        RfreshUILayout();

        _currentIndex = -1;
        CurrentIndex = moduleSelectRole.LastEnterRoleIndex;

        toggleGroup.SetAllTogglesOff();
        if (CurrentIndex >= 0)
        {
            var t = toggles.GetValue<Toggle>(CurrentIndex);
            if (t) t.isOn = true;
        }
    }

    protected override void OnReturn()
    {
        moduleLogin.LogOut(true);
    }

    private void RfreshUILayout()
    {
        for (var i = 0; i < roleInfoBorad.Length; i++)
            roleInfoBorad[i].SafeSetActive(false);

        for (var i = 0; i < moduleSelectRole.roleList?.Length; i++)
        {
            if (i >= roleInfoBorad.Length) break;
            var index = i;
            roleInfoBorad[i].SafeSetActive(true);
            RoleInfoBinder.Bind(roleInfoBorad[i], moduleSelectRole.roleList[i], ()=> { OnDeleteRole(index); });
        }

        if (!createButton) return;

        if (moduleSelectRole.RoleCount < roleInfoBorad.Length)
        {
            createButton.gameObject.SetActive(true);
            if(roleInfoBorad[moduleSelectRole.RoleCount])
                createButton.transform.parent.position = roleInfoBorad[moduleSelectRole.RoleCount].position;
        }
        else
            createButton.gameObject.SetActive(false);
    }


    private void _ME(ModuleEvent<Module_SelectRole> e)
    {
        switch (e.moduleEvent)
        {
            case Module_SelectRole.Response_DeleteRole:
                OnDeleteRole(e.msg as ScDropRole);
                break;
        }
    }

    private void OnDeleteRole(ScDropRole msg)
    {
        if (msg.result != 0)
        {
            return;
        }

        deletePanel.UnInitialize();
        RfreshUILayout();
        _currentIndex = -1;
        CurrentIndex = moduleSelectRole.FindIndex(_currentRoleId);
    }

    public class RoleInfoBinder
    {
        public static void Bind(Transform rRoot, PRoleSummary rRole, UnityAction rOnDeleteClick)
        {
            var profession = rRoot.GetComponent<Transform>("profession");
            AtlasHelper.SetShared(profession, $"proicon_0{rRole.proto}");

            Util.SetText(rRoot.GetComponent<Text>("name"), rRole.name);
            Util.SetText(rRoot.GetComponent<Text>("level"), $"Lv.{rRole.level}");
            Util.SetText(rRoot.GetComponent<Text>("raidValue"), ConfigText.GetDefalutString(204, 36) + rRole.power);

            rRoot.GetComponent<Button>("delete_btn")?.onClick.AddListener(rOnDeleteClick);
        }
    }
}


public class SelectRole_Delete : SubWindowBase
{
    private Button cancel;
    private Button close;
    private Button confirm;
    private InputField input;
    private Text noticeText;

    private ulong _deleteRoleId;

    protected override void InitComponent()
    {
        base.InitComponent();
        input       = WindowCache.GetComponent<InputField>("deletePanel/changename/inputname");
        confirm     = WindowCache.GetComponent<Button>    ("deletePanel/changename/cnamesurefree");
        cancel      = WindowCache.GetComponent<Button>    ("deletePanel/changename/cancel");
        close       = WindowCache.GetComponent<Button>    ("deletePanel/bg/equip_prop/top/button");
        noticeText  = WindowCache.GetComponent<Text>      ("deletePanel/changename/notice");

        input.characterLimit = GeneralConfigInfo.MAX_NAME_LEN;

    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;
        input.text = string.Empty;
        _deleteRoleId = (ulong)p[0];
        var roleInfo = moduleSelectRole.GetRoleSummary(_deleteRoleId);
        Util.SetText(noticeText,
            Util.Format(ConfigText.GetDefalutString(TextForMatType.SelectRoleUI, 3),
                roleInfo?.name));
        confirm?.onClick.AddListener(OnConfirm);
        cancel ?.onClick.AddListener(OnCancel);
        close  ?.onClick.AddListener(OnCancel);
        return true;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;

        confirm?.onClick.RemoveAllListeners();
        cancel ?.onClick.RemoveAllListeners();
        close  ?.onClick.RemoveAllListeners();

        return true;
    }

    private void OnConfirm()
    {
        if(input.text != ConfigText.GetDefalutString(TextForMatType.SelectRoleUI, 4))
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.SelectRoleUI, 6));
            return;
        }

        moduleSelectRole.DeleteRole(_deleteRoleId);
    }

    private void OnCancel()
    {
        UnInitialize();
    }
}