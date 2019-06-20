// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-01-07      15:50
//  *LastModify：2019-01-07      15:50
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class Window_ApplyFriend : Window
{
    private Button addButton;
    private Button closeButton;
    private Button blackButton;
    private Text m_addTxt;
    private Text addPointText;
    private Text name_;
    private Text uid_;
    private Text level;
    private Text icon;
    private Text introduce;
    private Image protoHint;
    private headBoxFriend friendBox;
    private Transform Head_img;

    private PPlayerInfo playerInfo;

    protected override void OnOpen()
    {
        base.OnOpen();
        InitComponent();
        MultiLangrage();
        isFullScreen = false;
        addButton?.onClick.AddListener(() => moduleFriend.SendAddMes(playerInfo.roleId));
        closeButton?.onClick.AddListener(() => Hide());
        blackButton?.onClick.AddListener(() => moduleFriend.SendShieDing(playerInfo.roleId));
    }

    private void MultiLangrage()
    {
        var ct = ConfigManager.Get<ConfigText>((int)TextForMatType.ApplyFriendUI).text;
        if (null == ct) return;
        Util.SetText(GetComponent<Text>("person_frame/titled_bg/title_Txt"), ct[0]);
        Util.SetText(GetComponent<Text>("person_frame/black_btn/Image"), ct[4]);
    }

    protected void InitComponent()
    {
        addButton   = GetComponent<Button>      ("person_frame/invite_btn");
        closeButton = GetComponent<Button>      ("person_frame/titled_bg/close_btn");
        blackButton = GetComponent<Button>      ("person_frame/black_btn");
        m_addTxt    = GetComponent<Text>        ("person_frame/invite_btn/Image");
        name_       = GetComponent<Text>        ("person_frame/content/infoList/name_Txt");
        level       = GetComponent<Text>        ("person_frame/content/infoList/level_Txt");
        uid_        = GetComponent<Text>        ("person_frame/content/infoList/id_Txt");
        introduce   = GetComponent<Text>        ("person_frame/content/individual/Text");
        protoHint   = GetComponent<Image>       ("person_frame/content/frameInside/framePro");
        friendBox   = GetComponent<Transform>   ("person_frame/content/bg")?.gameObject.AddComponent<headBoxFriend>();
        Head_img    = transform.Find("person_frame/content/bg/mask");
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        playerInfo = Window.GetWindowParam<Window_ApplyFriend>().param1 as PPlayerInfo;
        if (playerInfo == null)
            return;

        Util.SetText(name_, playerInfo.name);
        Util.SetText(uid_, "<color=#CDFDFFFF>UID:</color>{0}", playerInfo.index);
        Util.SetText(level, $"LV.{playerInfo.level}");
        Util.SetText(introduce, moduleSet.SetSignTxt(playerInfo.intro));

        AtlasHelper.SetShared(protoHint, "ui_invitefriend_" + playerInfo.proto);

        friendBox.HeadBox(playerInfo.headBox);
        Module_Avatar.SetClassAvatar(Head_img?.gameObject, playerInfo.proto, false, playerInfo.gender);

        bool apply = moduleFriend.AddApplyID.Exists(a => a == playerInfo.roleId);
        if (apply) Util.SetText(m_addTxt, 263, 5);
        else Util.SetText(m_addTxt, 263, 1);
        addButton.interactable = !apply;

    }

    protected override void OnHide(bool forward)
    {
        moduleChase.LastComrade = null;
    }

    private void _ME(ModuleEvent<Module_Friend> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Friend.EventFriendAddApply:
                var result = Util.Parse<int>(e.param1.ToString());
                if (result == 0)
                {
                    Hide();
                }
                break;
            case Module_Friend.EventFriendAgree:
                Hide();
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString((int)TextForMatType.ApplyFriendUI, 3));
                break;
            case Module_Friend.EventFriendAddBlack:
                Hide();
                break;
        }
    }
}
