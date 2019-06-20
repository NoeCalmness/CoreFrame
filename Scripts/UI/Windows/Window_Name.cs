/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-01-13
 * 
 ***************************************************************************************************/

using System;
using System.Text.RegularExpressions;
using UnityEngine.UI;

public class Window_Name : Window,IBackToStory
{
    private static char[] SPECIAL_CHARACTOR = new char[] { '·' };
    public const string MATCH_STRING = @"^[a-zA-Z0-9·\u4e00-\u9fa5]+$";
    public const string MATCH_NUM = @"^[0-9]+$";

    private InputField roleName;
    private Button rollName;
    private Button confirmBtn;

    #region interface property
    public Action onBackToStory { get; set; }
    #endregion

    /// <summary>
    /// Called when window added to level
    /// </summary>
    protected override void OnOpen()
    {
        roleName = GetComponent<InputField>("creatRolePanel/rolePanel/roleName");
        roleName.characterLimit = GeneralConfigInfo.MAX_NAME_LEN;
        rollName = GetComponent<Button>("creatRolePanel/rolePanel/shaizi");
        confirmBtn = GetComponent<Button>("creatRolePanel/rolePanel/confirm");

        rollName.onClick.RemoveAllListeners();
        rollName.onClick.AddListener(OnDiceClick);
        confirmBtn.onClick.RemoveAllListeners();
        confirmBtn.onClick.AddListener(OnConfirmClick);
        InitializeText();
    }

    private void InitializeText()
    {
        var t = ConfigManager.Get<ConfigText>((int)TextForMatType.NameUIText);
        if (!t) return;

        Util.SetText(GetComponent<Text>("creatRolePanel/detail_panel/bg/equip_prop/top/equipinfo"), t[0]);
        Util.SetText(GetComponent<Text>("creatRolePanel/detail_panel/bg/equip_prop/top/english"), t[1]);
        Util.SetText(GetComponent<Text>("creatRolePanel/rolePanel/confirm/Text"), t[2]);
        Util.SetText(GetComponent<Text>("creatRolePanel/rolePanel/roleName/Placeholder"), t[3]);
        Util.SetText(GetComponent<Text>("creatRolePanel/rolePanel/roleName/Text"), t[4]);
        Util.SetText(GetComponent<Text>("creatRolePanel/rolePanel/Text"), Util.Format(t[4], GeneralConfigInfo.MIN_NAME_LEN, GeneralConfigInfo.MAX_NAME_LEN));
    }

    private void OnDiceClick()
    {
        roleName.text = NameConfigInfo.GetRandomName(moduleLogin.createGender);
    }

    private void OnConfirmClick()
    {
        if (string.IsNullOrEmpty(roleName.text))
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 14));
            return;
        }

        if (roleName.text.Length < GeneralConfigInfo.MIN_NAME_LEN || roleName.text.Length > GeneralConfigInfo.MAX_NAME_LEN)
        {
            moduleGlobal.ShowMessage(Util.Format(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 46), GeneralConfigInfo.MIN_NAME_LEN, GeneralConfigInfo.MAX_NAME_LEN));
            return;
        }

        var onlyNum = Regex.IsMatch(roleName.text, MATCH_NUM);
        if (onlyNum)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.NameUIText, 6));
            roleName.text = string.Empty;
            return;
        }

        var match = Regex.IsMatch(roleName.text, MATCH_STRING);
        if (!match)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 15));
            roleName.text = string.Empty;
            return;
        }

        if(Util.ContainsSensitiveWord(roleName.text))
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.NameUIText, 5));
            return;
        }

        if (moduleStory.debugStory) OnCreateSucess();
        else
        {
            moduleLogin.createName = roleName.text;
            moduleLogin.CreatePlayer();
        }
    }

    private bool IsValidChar(string s)
    {
        foreach (var c in s)
        {
            
            if (SPECIAL_CHARACTOR.Contains(c)) continue;

            if (!char.IsLetter(c) && !char.IsNumber(c)) return false;
        }

        return true;
    }

    private void OnCreateSucess()
    {
        Hide(true);
        onBackToStory?.Invoke();
    }

    void _ME(ModuleEvent<Module_Login> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Login.EventCreatPlayerSuccess:
                {
                    OnCreateSucess();
                    break;
                }
            case Module_Login.EventCreatePlayerFailed:
                {
                    var p = e.msg as ScCreateRole;
                    if (p.result == 1)
                    {
                        moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 15));
                        roleName.text = string.Empty;
                    }
                    else if (p.result == 2)
                    {
                        moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 16));
                        roleName.text = string.Empty;
                    }
                    break;
                }
        }
    }
}
