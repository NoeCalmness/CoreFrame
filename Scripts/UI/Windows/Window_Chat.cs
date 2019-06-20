/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-10-11
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Chat : Window
{
    private Button record_Btn;//录音按钮
    private Button keyboard_btn;//键盘
    private Button face_Btn;//表情按钮
    private Button send_Btn;//发送按钮
    private GameObject input_mes;//输入的文字背景
    private NewInputFile mes_add;
    private Button input_false;
    private Button voice_enable;
    private Toggle sys_mes;//切换系统消息
    private Toggle word_mes;//切换世界聊天消息
    private Toggle m_unionBtn;
    private Toggle m_teamToggle;
    private Button m_close;

    //世界聊天消息界面
    private Button roomchange_btn;
    private Text roomchange_btn_txt;//切换按钮上的文字
    private Button roomchange_plane;
    private Button roomchange_close;
    private Text room_all;
    private Button change_sure;
    private InputField room_num;
    private GameObject other_speak;
    private GameObject my_speak;
    private GameObject chat_content;
    private Scrollbar chat_value;
    private GameObject system_mes;
    private GameObject m_tipObj;
    private Text m_tipText;
    private GameObject m_detailPlane;
    private Button m_detailAdd;
    private VerticalLayoutGroup m_chatGroup;
    private GameObject emoticons;//表情包界面
    private Button emoreturn_Btn;

    private Text m_inputTipTxt;
    private float ContentAllheigh;
    private ConfigText chat_text;
    private Queue<GameObject> Word_chat_record = new Queue<GameObject>();//在window进行存储 世界聊天

    protected override void OnOpen()
    {
        isFullScreen = false;

        Word_chat_record.Clear();

        chat_text = ConfigManager.Get<ConfigText>((int)TextForMatType.ChatUIText);
        if (chat_text == null)
        {
            chat_text = ConfigText.emptey;
            Logger.LogError("this id can not");
        }
        SetText();

        #region  path
        m_tipText = GetComponent<Text>("downBtn/Text");
        m_tipObj = GetComponent<RectTransform>("downBtn").gameObject;
        m_detailPlane = GetComponent<RectTransform>("Character details").gameObject;
        m_detailAdd = GetComponent<Button>("Character details/Rawback/button_group/add");

        chat_content = GetComponent<RectTransform>("bg/chat_room/Scroll View/Viewport/Content").gameObject;
        chat_value = GetComponent<Scrollbar>("bg/chat_room/Scroll View/Scrollbar Vertical");

        record_Btn = GetComponent<Button>("bg/input_btn/record");
        keyboard_btn = GetComponent<Button>("bg/input_btn/ keyboard");
        face_Btn = GetComponent<Button>("bg/input_btn/face ");
        m_inputTipTxt= GetComponent<Text>("bg/input_btn/InputField/add");
        input_mes = GetComponent<RectTransform>("bg/input_btn/InputField").gameObject;
        send_Btn = GetComponent<Button>("bg/input_btn/send");
        mes_add = GetComponent<NewInputFile>("bg/input_btn/InputField");
        input_false = GetComponent<Button>("bg/input_btn/input__false");
        voice_enable = GetComponent<Button>("bg/input_btn/voice_btn");
        sys_mes = GetComponent<Toggle>("bg/system_btn");
        word_mes = GetComponent<Toggle>("bg/chat_btn");
        m_unionBtn = GetComponent<Toggle>("bg/gonghui_btn");
        m_teamToggle = GetComponent<Toggle>("bg/team_btn");
        m_close = GetComponent<Button>("bg/close");

        roomchange_btn = GetComponent<Button>("bg/changeroom");
        roomchange_btn_txt = GetComponent<Text>("bg/changeroom/room_num");
        room_all = GetComponent<Text>("tip_change/change_room/room_all");
        roomchange_plane = GetComponent<Button>("tip_change");
        roomchange_close = GetComponent<Button>("tip_change/change_room/top/button");
        change_sure = GetComponent<Button>("tip_change/change_room/sure");
        room_num = GetComponent<InputField>("tip_change/change_room/num_room/input");

        other_speak = GetComponent<RectTransform>("bg/other_speak").gameObject;
        my_speak = GetComponent<RectTransform>("bg/my_speak").gameObject;
        system_mes = GetComponent<RectTransform>("bg/sys-img").gameObject;

        emoticons = GetComponent<RectTransform>("emoticons").gameObject;
        emoreturn_Btn = GetComponent<Button>("emoticons/Image/Image/return");

        m_chatGroup = GetComponent<VerticalLayoutGroup>("bg/chat_room/Scroll View/Viewport/Content");
        ContentAllheigh = m_chatGroup.padding.top;
        #endregion

        #region onclick

        record_Btn.onClick.AddListener(delegate
        {
            mes_add.text = string.Empty;
            input_mes.gameObject.SetActive(false);//输入
            face_Btn.gameObject.SetActive(false);//表情
            voice_enable.gameObject.SetActive(true);//录音框
            record_Btn.gameObject.SetActive(false);//录音
            keyboard_btn.gameObject.SetActive(true);//键盘
            send_Btn.SetInteractable(false);
        });
        keyboard_btn.onClick.AddListener(delegate
        {
            input_mes.gameObject.SetActive(true);
            face_Btn.gameObject.SetActive(true);//表情
            voice_enable.gameObject.SetActive(false);
            record_Btn.gameObject.SetActive(true);
            keyboard_btn.gameObject.SetActive(false);
            send_Btn.SetInteractable(true);
        });
        face_Btn.onClick.AddListener(delegate
        {
            if (emoticons.activeInHierarchy) emoticons.SetActive(false);
            else emoticons.SetActive(true);
        });
        send_Btn.onClick.AddListener(delegate
        {
            mes_add.Send();
            string str = mes_add.sendtxt.text.Replace("\\n", "\n");
            if (!string.IsNullOrEmpty(str))
            {
                if (moduleChat.opChatType == OpenWhichChat.TeamChat)
                {
                    moduleChat.SendFriendMessage(Util.ValidateSensitiveWords(str), 0, moduleAwakeMatch.TeamFriend, 2);
                    return;
                }
                var message = Util.ValidateSensitiveWords(str);
                WordMySend(0, message);

            }
        });
        mes_add.onValueChanged.AddListener(delegate
        {
            if (modulePlayer.roleInfo.level < moduleChat.ChatLevel && word_mes.isOn)
            {
                var str = Util.Format((chat_text[18]), moduleChat.ChatLevel);
                moduleGlobal.ShowMessage(str);
                mes_add.text = string.Empty;
            }
        }); 

        voice_enable.SetPressDelay(0.2f);
        voice_enable.onPressed().AddListener(a =>
        {
            if (a) { }
            else
            {
                string str = string.Empty;
                WordMySend(2, str);
            }
        });
        roomchange_btn.onClick.AddListener(delegate
        {
            if (moduleChat.Chat_list.Count == 0) return;
            room_num.text = roomchange_btn_txt.text;
            roomchange_plane.gameObject.SetActive(true);
        });

        roomchange_plane.onClick.AddListener(delegate { roomchange_plane.gameObject.SetActive(false); });
        roomchange_close.onClick.AddListener(delegate { roomchange_plane.gameObject.SetActive(false); });

        change_sure.onClick.AddListener(delegate
        {
            if (room_num.text == roomchange_btn_txt.text) moduleGlobal.ShowMessage(chat_text[12]);
            else
            {
                if (Util.Parse<int>(room_num.text) > moduleChat.Chat_list.Count) moduleGlobal.ShowMessage(chat_text[10]);
                else
                {
                    int index = Util.Parse<int>(room_num.text.ToString());
                    ulong a = moduleChat.Chat_list[index - 1];//获取长id
                    moduleChat.SendChangeRoom(a);//切换请求
                }
            }
        });

        emoreturn_Btn.onClick.AddListener(delegate { emoticons.gameObject.SetActive(false); });
        m_close.onClick.AddListener(delegate
        {
            mes_add.text = string.Empty;
            Hide(true);
        });

        chat_value.onValueChanged.AddListener(delegate
        {
            CanShowTip(chat_value, moduleChat.opChatType);
        });

        SetToggleHide();
        m_teamToggle.onValueChanged.RemoveAllListeners();
        m_unionBtn.onValueChanged.RemoveAllListeners();
        sys_mes.onValueChanged.RemoveAllListeners();
        word_mes.onValueChanged.RemoveAllListeners();
        m_teamToggle.onValueChanged.AddListener(delegate { if (m_teamToggle.isOn) TeamChatShow(); });
        m_unionBtn.onValueChanged.AddListener(delegate { if (m_unionBtn.isOn) UnionChatShow(); });
        sys_mes.onValueChanged.AddListener(delegate { if (sys_mes.isOn) SysChatShow(); });
        word_mes.onValueChanged.AddListener(delegate { if (word_mes.isOn) { WorldChatShow(); } });

        #endregion

        CloneAllEmoj();

        roomchange_btn.interactable = false;
        roomchange_btn_txt.text = "1";
        moduleChat.SendChatRoomList();//请求聊天室列表

    }
    
    private void SetText()
    {
        Util.SetText(GetComponent<Text>("bg/team_btn/team"), chat_text[16]);
        Util.SetText(m_tipText, 218, 48);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/Panle_list/chat/Button/Text"), 218, 49);
        Util.SetText(GetComponent<Text>("bg/chat_btn/word"), chat_text[0]);
        Util.SetText(GetComponent<Text>("bg/system_btn/system"), chat_text[1]);
        Util.SetText(GetComponent<Text>("bg/gonghui_btn/system"), chat_text[2]);
        Util.SetText(GetComponent<Text>("bg/chat_room/room/Text"), chat_text[3]);
        Util.SetText(GetComponent<Text>("bg/input_btn/send/send_text"), chat_text[4]);
        Util.SetText(GetComponent<Text>("tip_change/change_room/top/equipinfo"), chat_text[5]);
        Util.SetText(GetComponent<Text>("tip_change/change_room/Text"), chat_text[6]);
        Util.SetText(GetComponent<Text>("tip_change/change_room/sure/Text"), chat_text[7]);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        m_teamToggle.SafeSetActive(moduleAwakeMatch.IsInTeam);

        m_unionBtn.interactable = false;
        if (modulePlayer.roleInfo.leagueID != 0) m_unionBtn.interactable = true;
        if (!moduleChat.connected) moduleChat.WorldSession();

        if (m_subTypeLock != -1)
        {
            if (m_subTypeLock > 0 && m_subTypeLock <= (int)OpenWhichChat.TeamChat)
                moduleChat.opChatType = (OpenWhichChat)m_subTypeLock;
            else moduleChat.opChatType = OpenWhichChat.WorldChat;
        }

        AddChatInfo(moduleChat.opChatType);
    }

    #region return

    private void SetToggleHide()
    {
        SetToggleState(m_unionBtn);
        SetToggleState(sys_mes);
        SetToggleState(word_mes);
        SetToggleState(m_teamToggle);
    }

    private void SetToggleState(Toggle toggle)
    {
        toggle.gameObject.SetActive(false);
        toggle.isOn = false;
        toggle.gameObject.SetActive(true);
    }

    protected override void OnHide(bool forward)
    {
        SetToggleHide();
    }
    #endregion

    #region 打开重置

    public void AddChatInfo(OpenWhichChat type)
    {
        moduleChat.m_newSysMes = false;
        moduleChat.m_newWordMes = false;
        moduleChat.m_newUnionMes = false;
        moduleChat.m_newTeamMes = false;

        switch (type)
        {
            case OpenWhichChat.WorldChat: word_mes.isOn = true; break;
            case OpenWhichChat.SysChat: sys_mes.isOn = true; break;
            case OpenWhichChat.UnionChat: m_unionBtn.isOn = true; break;
            case OpenWhichChat.TeamChat: m_teamToggle.isOn = true; break;
        }
    }
    private void DeletePrefab(Queue<GameObject> objList)
    {
        var length = objList.Count;
        for (int i = 0; i < length; i++)
        {
            GameObject.Destroy(objList.Dequeue());//清空预制体
        }
        objList.Clear();
    }

    private void WorldChatShow()//当进入打开世界界面时
    {
        moduleChat.opChatType = OpenWhichChat.WorldChat;
        BtnState();
        AddWorldInfo();
    }

    private void SysChatShow()//当进入消息界面时
    {
        moduleChat.opChatType = OpenWhichChat.SysChat;
        BtnState();
        AddSystemInfo();
    }

    private void UnionChatShow()//当进入公会聊天界面
    {
        moduleChat.opChatType = OpenWhichChat.UnionChat;
        BtnState();
        AddUnionInfo();
    }

    private void TeamChatShow()//当进入组队聊天界面
    {
        moduleChat.opChatType = OpenWhichChat.TeamChat;
        BtnState();
        AddTeamInfo();
        moduleChat.DispatchModuleEvent(Module_Chat.EventChatSeeTeamMsg);
    }

    private void BtnState()
    {
        DeletePrefab(Word_chat_record);
        ContentAllheigh = m_chatGroup.padding.top;
        emoticons.SetActive(false);
        ContentSortLast();
        roomchange_btn.gameObject.SetActive(moduleChat.opChatType == OpenWhichChat.WorldChat);
        face_Btn.SetInteractable(moduleChat.opChatType != OpenWhichChat.SysChat);
        send_Btn.SetInteractable(moduleChat.opChatType != OpenWhichChat.SysChat);
        record_Btn.SetInteractable(moduleChat.opChatType != OpenWhichChat.SysChat);
        mes_add.interactable = moduleChat.opChatType == OpenWhichChat.SysChat ? false : true;

        if (moduleChat.opChatType == OpenWhichChat.SysChat) Util.SetText(m_inputTipTxt, chat_text[20]);
        else Util.SetText(m_inputTipTxt, chat_text[19]);

        input_mes.gameObject.SetActive(true);
        input_false.gameObject.SetActive(false);
        face_Btn.gameObject.SetActive(true);//表情
        voice_enable.gameObject.SetActive(false);
        record_Btn.gameObject.SetActive(true);
        keyboard_btn.gameObject.SetActive(false);
    }

    #endregion

    #region 加载聊天信息
    private void AddWorldInfo()
    {
        foreach (var item in moduleChat.word_chat_mes)
        {
            if (item == null) return;

            if (item.sendId == 110) SysAdd(item.content);
            else if (item.sendId == modulePlayer.id_) SetClone(true, item.type, item.content, modulePlayer.id_);
            else
            {
                string[] infos = item.tag.Split('/');
                if (infos.Length < 5) return;
                SetClone(false, item.type, item.content, item.sendId, infos);
            }
        }
        ContentSortLast();
    }

    private void AddSystemInfo()
    {
        foreach (var item in moduleChat.sys_chat_mes)
        {
            if (item == null) return;
            SysAdd(item.content);
        }
        ContentSortLast();
    }

    private void AddUnionInfo()
    {
        foreach (var item in moduleChat.m_unionChat)
        {
            if (item == null) return;
            AddUnionMes(item);
        }
        ContentSortLast();
    }

    private void AddTeamInfo()
    {
        foreach (var msg in moduleChat.team_chat_record)
        {
            if (msg is CsChatPrivate)
                SendPrivateMsg(msg as CsChatPrivate);
            else if (msg is ScChatPrivate)
                RecieveMsg(msg as ScChatPrivate);
        }
        ContentSortLast();
    }
    #endregion

    #region 载入表情资源

    private void CloneAllEmoj()
    {
        var emjio = ConfigManager.GetAll<FaceName>();
        new DataSource<FaceName>(emjio, GetComponent<ScrollView>("emoticons/Image/scrollView"), EmojiSet, EmojiClick);
    }
    private void EmojiSet(RectTransform rt, FaceName Info)
    {
        // Logger.LogError("Window_Chat:: Load emoji asset <b><color=#6C53FF>[{0}]</color></b> failed!");        
        GameObject a = Level.GetPreloadObject(Info.head_icon);
        if (a == null) return;
        Util.AddChild(rt, a.transform);

    }
    private void EmojiClick(RectTransform rt, FaceName info)
    {
        if (info == null) return;

        if (m_teamToggle.isOn)
        {
            moduleChat.SendFriendMessage(info.head_icon, 1, moduleAwakeMatch.TeamFriend, 2);
            emoticons.SetActive(false);
            return;
        }
        WordMySend(1, info.head_icon);
        emoticons.SetActive(false);
        
    }
    #endregion

    #region 世界聊天 切换房间 查看详情 添加好友

    private void WordMySend(int type, string content)
    {
        if (sys_mes.isOn) return;

        if (modulePlayer.BanChat == 2)
        {
            moduleGlobal.ShowMessage(630, 0);
            return;
        }
        
        var sendtype = OpenWhichChat.WorldChat;
        if (word_mes.isOn)
        {
            if (!moduleChat.CanChatWord)
            {
                moduleGlobal.ShowMessage(chat_text[17]);
                return;
            }
            else if (modulePlayer.roleInfo.level < moduleChat.ChatLevel)
            {
                var str = Util.Format((chat_text[18]), moduleChat.ChatLevel);
                moduleGlobal.ShowMessage(str);
                return;
            }
        }
        else if (m_unionBtn.isOn) sendtype = OpenWhichChat.UnionChat;
        else if (m_teamToggle.isOn) sendtype = OpenWhichChat.TeamChat;

        SetClone(true, type, content, modulePlayer.id_);
        moduleChat.SendWordMessage(content, type, sendtype);
    }

    private void ChangeRoom(int result)//切换房间
    {
        roomchange_plane.gameObject.SetActive(false);
        if (result == 0)
        {
            DeletePrefab(Word_chat_record);
            ContentAllheigh = m_chatGroup.padding.top;

            roomchange_btn_txt.text = room_num.text.ToString();
            string mes = chat_text[8] + room_num.text + chat_text[9];
            moduleGlobal.ShowMessage(mes);
        }
        else if (result == 1)
        {
            moduleChat.SendChatRoomList();
            moduleGlobal.ShowMessage(chat_text[10]);
        }
        else if (result == 2) moduleGlobal.ShowMessage(chat_text[11]);
        else if (result == 3) Logger.LogChat("正在进入中，请等待");
    }

    private void Player_show(ulong id_this)//查看玩家信息
    {
        moduleChat.Show_word_details(id_this);
    }
    private void SetInvate(int type, string cont, ulong fId)
    {
        if (modulePlayer.BanChat == 2)
        {
            moduleGlobal.ShowMessage(630, 0);
            return;
        }
        moduleChat.SendFriendMessage(cont, type, fId);
    }
    
    #endregion

    #region 系统消息

    private void SysAdd(string sys_mes)
    {
        GameObject sysobj = GameObject.Instantiate(system_mes);
        //存进去
        OverReduceContent(sysobj);

        SetSysInfo(sysobj, sys_mes);
        RectTransform sysobj_height = sysobj.GetComponent<RectTransform>();
        ContentChange(sysobj_height.sizeDelta.y, true);

        bool down = true;
        if (chat_value.value >= GeneralConfigInfo.defaultConfig.chatLerp) down = false;
        ContentSortLast(down);
    }

    private void SetSysInfo(GameObject sysobj, string sys_mes)
    {
        GameObject txtobj = sysobj.transform.Find("mes").gameObject;
        txtobj.GetComponentDefault<Chathyperlink>().gettxt = sys_mes;
        Chathyperlink Pic = txtobj.GetComponentDefault<Chathyperlink>();

        Pic.gameObject.SetActive(true);

        Pic.text = sys_mes;
        Pic.Set();

        Pic.text = Pic.gettxt;
        float width = Pic.preferredWidth;
        float height = Pic.preferredHeight;

        RectTransform sysobj_height = sysobj.GetComponent<RectTransform>();
        if (width <= 423f)
        {
            sysobj_height.sizeDelta = new Vector2(sysobj_height.sizeDelta.x, 33f);//设置图片的宽高
        }
        else
        {
            ContentSizeFitter a = Pic.gameObject.GetComponent<ContentSizeFitter>();
            a.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            sysobj_height.sizeDelta = new Vector2(sysobj_height.sizeDelta.x, height + 8f);//设置图片的宽高
        }
        SetPostion(sysobj);
    }

    #endregion

    #region 公会聊天

    private void AddUnionMes(ScChatGroup union)
    {
        if (union == null) return;

        if (union.sendId == 111) SysAdd(union.content);
        else
        {
            if (string.IsNullOrEmpty(union.tag))
            {
                Logger.LogError("no tag");
                return;
            }
            if (union.sendId == modulePlayer.id_)
            {
                string[] unionMes = union.tag.Split('/');
                if (unionMes.Length < 4) return;
                SetClone(true, union.type, union.content, union.sendId);
            }
            else
            {
                string[] unionMes = union.tag.Split('/');
                if (unionMes.Length < 5) return;
                SetClone(false, union.type, union.content, modulePlayer.id_, unionMes);
            }
        }
    }

    #endregion

    #region 组队聊天

    private void RecieveMsg(ScChatPrivate msg)
    {
        if (!moduleAwakeMatch.IsTeamMember(msg.sendId)) return;
        var info = moduleAwakeMatch.GetMatchInfo(msg.sendId);

        var tag = Util.Parse<int>(msg.tag);
        if (tag == 2) tag = 0;
        AllChatMesClone(false, msg.type, msg.content, other_speak, info.roleName, info.avatar, info.roleId, info.gender, info.headBox, info.roleProto, tag);

        moduleChat.DispatchModuleEvent(Module_Chat.EventChatSeeTeamMsg);
    }
    private void SendPrivateMsg(CsChatPrivate msg)
    {
        var tag = Util.Parse<int>(msg.tag);
        if (tag == 2) tag = 0;
        AllChatMesClone(true, msg.type, msg.content, my_speak, modulePlayer.name_, modulePlayer.avatar, modulePlayer.id_, modulePlayer.gender, modulePlayer.avatarBox, modulePlayer.proto, tag);
        moduleChat.DispatchModuleEvent(Module_Chat.EventChatSeeTeamMsg);
    }
    #endregion

    #region 聊天的显示

    private void SetClone(bool mysend, int sysType, string str, ulong sendId, string[] info = null)
    {
        if (mysend) AllChatMesClone(mysend, sysType, str, my_speak, modulePlayer.name_, modulePlayer.avatar, sendId, modulePlayer.gender, modulePlayer.avatarBox, modulePlayer.proto);
        else if (info != null) AllChatMesClone(false, sysType, str, other_speak, info[0], info[2], sendId, Util.Parse<int>(info[1]), Util.Parse<int>(info[3]), Util.Parse<int>(info[4]));
    }

    private void AllChatMesClone(bool mysend, int systype, string matter, GameObject objclone, string word_name,
        string word_head, ulong id, int gender, int headboxid, int proto, int rich = 0)
    {
        //0 文本 1 图片2 语音 type 3 动图
        GameObject obj = GameObject.Instantiate(objclone);
        OverReduceContent(obj);
        SetPostion(obj);

        ChatMes chat_mes = obj.GetComponent<ChatMes>();
        chat_mes.show_details(mysend, word_name, word_head, gender, id, proto, Player_show);
        chat_mes.caht_show(systype, matter, mysend, headboxid, rich);

        ContentChange(chat_mes.This_height, true);

        if (mysend)
        {
            mes_add.text = string.Empty;
            chat_value.value = 0;
            ContentSortLast();
        }
    }

    private void SetPostion(GameObject obj)
    {
        obj.transform.SetParent(chat_content.transform);
        obj.transform.localScale = new Vector3(1, 1, 1);
        obj.transform.localPosition = new Vector3(0, 0, 0);
        obj.SetActive(true);
    }

    #endregion

    #region  提示当前有新的消息

    private void SetTipTxtShow(OpenWhichChat type)
    {
        if (!chat_content.activeInHierarchy) return;
        SetTip(type);

        if (chat_value.value >= GeneralConfigInfo.defaultConfig.chatLerp && chat_value.gameObject.activeInHierarchy)
        {
            m_tipObj.gameObject.SetActive(true);
        }
        else m_tipObj.gameObject.SetActive(false);
    }
    private void CanShowTip(Scrollbar thisBar, OpenWhichChat type)
    {
        if (thisBar.value >= GeneralConfigInfo.defaultConfig.chatLerp && thisBar.gameObject.activeInHierarchy)
        {
            m_tipObj.gameObject.SetActive(true);
            SetTip(type);
        }
        else
        {
            m_tipObj.gameObject.SetActive(false);
            moduleChat.m_newSysMes = false;
            moduleChat.m_newWordMes = false;
            moduleChat.m_newUnionMes = false;
            moduleChat.m_newTeamMes = false;
        }
    }
    private void SetTip(OpenWhichChat type)
    {
        if (type == OpenWhichChat.WorldChat) SetTipTxt(moduleChat.m_newWordMes);
        else if (type == OpenWhichChat.SysChat) SetTipTxt(moduleChat.m_newSysMes);
        else if (type == OpenWhichChat.UnionChat) SetTipTxt(moduleChat.m_newUnionMes);
        else if (type == OpenWhichChat.TeamChat) SetTipTxt(moduleChat.m_newTeamMes);
    }
    private void SetTipTxt(bool newMes)
    {
        if (newMes) Util.SetText(m_tipText, chat_text[14]);
        else Util.SetText(m_tipText, chat_text[15]);
    }

    #endregion

    #region 计算 content

    private void ContentChange(float itemheight, bool isadd)//手动更改系統content高度
    {
        RectTransform o = chat_content.GetComponent<RectTransform>();
        VerticalLayoutGroup lay = chat_content.GetComponent<VerticalLayoutGroup>();
        if (isadd)
        {
            ContentAllheigh += lay.spacing;
            ContentAllheigh += itemheight;
        }
        else
        {
            ContentAllheigh -= lay.spacing;
            ContentAllheigh -= itemheight;
        }

        o.sizeDelta = new Vector2(0, ContentAllheigh);
    }

    private void ContentSortLast(bool down = true)//是否拉到最低处
    {
        var scroll = chat_content.transform.parent.parent.GetComponent<ScrollRect>();
        if (scroll != null)
        {
            scroll.verticalNormalizedPosition = 0;//每次我发消息或者切换时候调用最下
            m_tipObj.gameObject.SetActive(false);
        }
    }

    private void OverReduceContent(GameObject newObj)//超过最大限制时候 content变小 
    {
        if (Word_chat_record.Count >= moduleChat.ChatNum)
        {
            GameObject a = Word_chat_record.Dequeue();
            if (a != null)
            {
                RectTransform uu = a.GetComponent<RectTransform>();
                ContentChange(uu.sizeDelta.y, false);
                GameObject.Destroy(a);
            }
        }
        Word_chat_record.Enqueue(newObj);
    }

    #endregion

    #region Event

    private void _ME(ModuleEvent<Module_Chat> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Chat.EventChatRecWordMes:
                if (!word_mes.isOn) return;
                ScChatRoomMessage wordmes = e.msg as ScChatRoomMessage;
                string[] info = wordmes.tag.Split('/');
                if (info.Length < 5) return;
                SetClone(false, wordmes.type, wordmes.content, wordmes.sendId, info);
                SetTipTxtShow(OpenWhichChat.WorldChat);
                break;
            case Module_Chat.EventChatRecSysMes:
                if (!sys_mes.isOn) return;
                string sysmes = e.param1.ToString();
                SysAdd(sysmes);
                SetTipTxtShow(OpenWhichChat.SysChat);
                break;
            case Module_Chat.EventChatRecUnionMes:
                if (!m_unionBtn.isOn) return;
                ScChatGroup union = e.msg as ScChatGroup;
                AddUnionMes(union);
                SetTipTxtShow(OpenWhichChat.UnionChat);
                break;
            case Module_Chat.EventChatRecTeamMes:
                RecieveMsg(e.msg as ScChatPrivate);
                break;
            case Module_Chat.EventChatSendTeamMes:
                SendPrivateMsg(e.msg as CsChatPrivate);
                break;

            case Module_Chat.EventChatChangeRoom:
                int result = Util.Parse<int>(e.param1.ToString());
                ChangeRoom(result);
                break;
            case Module_Chat.EventChatRoomList:
                roomchange_btn.interactable = true;
                roomchange_btn_txt.text = moduleChat.RoomChatNum.ToString();
                room_all.text = moduleChat.Chat_list.Count.ToString();
                break;
            case Module_Chat.EventChatPlayerDetails:
                var playerInfo = e.msg as PPlayerInfo;
                FriendDetailInfo playerDetail = m_detailPlane.GetComponentDefault<FriendDetailInfo>();
                playerDetail.IsfriendDetails(playerInfo, true, SetInvate);
                m_detailPlane.gameObject.SetActive(true);
                break;
            case Module_Chat.EventChatWindowHide:
                if (actived) Hide(true);
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Union> e)
    {
        if (e.moduleEvent == Module_Union.EventUnionSelfExit || e.moduleEvent == Module_Union.EventUnionDissolution)
        {
            if (!actived) return;
            if (m_unionBtn.isOn) word_mes.isOn = true;
            m_unionBtn.isOn = false;
            m_unionBtn.interactable = false;
        }
    }

    private void _ME(ModuleEvent<Module_AwakeMatch> e)
    {
        if (e.moduleEvent == Module_AwakeMatch.Response_ExitRoom)
        {
            if (!actived) return;
            if (m_teamToggle.isOn) word_mes.isOn = true;
            m_teamToggle.isOn = false;
            m_teamToggle.SafeSetActive(false);
        }
    }
    private void _ME(ModuleEvent<Module_Friend> e)
    {
        if (!actived) return;
        if (e.moduleEvent == Module_Friend.EventFriendAddApply)
        {
            var result = Util.Parse<int>(e.param1.ToString());
            if(result !=0) m_detailAdd.SetInteractable(true);
        }
    }
    #endregion

}
