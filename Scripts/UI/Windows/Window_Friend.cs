/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-07-26
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Window_Friend : Window
{
    #region  UI

    //朋友窗体
    private Text friendNow;
    private Text friendTop;

    private Toggle m_chatToogle;
    private Toggle addFriend;
    private Toggle friendRequests;
    private Toggle Lately_Btn;
    private Toggle m_blackToggle;
    private ScrollView m_friendScroll;
    private ScrollView m_lateScroll;
    private ScrollView m_blackScroll;
    private TweenSize m_friendSize;
    private TweenSize m_lateSize;
    private TweenSize m_blackSize;

    //好友列表
    private Toggle friendBtn;

    //添加
    private RectTransform selectfriend;
    private Button select_Btn;

    private RectTransform recommend_panel;//默认界面 true
    private InputField select_Id;
    private Transform RecommendObj;

    private Button switch_Btn;
    private RectTransform selectok_panel;//搜索后成功的界面
    private GameObject selectok_bg;
    private Button details_selectok;//跳转详情页面
    private Text selectok_name;
    private Text selectok_level;
    private Text selectok_IDFalse;
    private Button selectok_add;
    private RectTransform selectfalse_panel;//搜索后失败的界面

    //申请列表
    private RectTransform requestslist;
    private Image Apply_hint;

    //聊天页
    private RectTransform Chat_Panel;
    private Text Chat_name;
    private Text Chat_level;
    private GameObject Chat_headbg;
    private Text Chat_id;
    private Button Chat_details;
    private Button Chat_deleted;
    private Button m_chatBlack;//移除
    private Button m_blackRemoveBtn;//拉黑
    private Button m_chatAddBtn;
    private RectTransform m_blackLOck;

    //聊天对话
    private Button record_Btn;//录音按钮
    private Button keyboard_btn;//键盘
    private Button face_Btn;//表情按钮
    private Button send_Btn;//发送按钮
    private GameObject input_mes;//输入的文字背景
    private Text m_inputShow;
    private NewInputFile mes_add;//发送的文字
    private Button voice_enable;//按住录音
    private GameObject chat_content;//父对象
    private Scrollbar friend_value;
    private GameObject my_speak_obj;
    private GameObject other_speak_obj;

    //表情包界面
    private GameObject emoticons;
    private Button return_Btn;

    //最近
    private GameObject m_lateHint;

    private GameObject ChatNothing;
    private GameObject FlistNothing;
    private GameObject ApplyNothing;

    private GameObject Character_details;
    private Button m_detailAdd;
    #endregion
    //界面中的文字
    private ConfigText friend_text;

    private float AllHeight;
    private VerticalLayoutGroup m_chatGroup;
    Queue<GameObject> chat_obj = new Queue<GameObject>();
    List<GameObject> Recommend = new List<GameObject>();
    List<FaceName> EmjioList = new List<FaceName>();

    private DataSource<PPlayerInfo> FrinednewList;//好友
    private DataSource<PPlayerInfo> RecentlynewList;//最近
    private DataSource<PPlayerInfo> ApplynewList;//申请
    private DataSource<PPlayerInfo> BlackList;
    private DataSource<FaceName> emjioList;

    //提示新消息文本
    private Button m_newThisObj;//这个人的新消息
    private Text m_newThisTxt;

    private VerticalLayoutGroup _layOut;
    private RectTransform m_template;
    private RectTransform m_listbgTop;

    private bool m_fLateChange = false;
    private bool m_ShowSort = false;

    protected override void OnOpen()
    {
        Character_details = GetComponent<RectTransform>("Character details").gameObject;
        m_detailAdd = GetComponent<Button>("Character details/Rawback/add");

        friend_text = ConfigManager.Get<ConfigText>((int)TextForMatType.FriendUIText);

        if (friend_text == null)
        {
            friend_text = ConfigText.emptey;
            Logger.LogError("Window_Friend:: Load ConfigText id <b><color=#6C53FF>[{0}]</color></b> failed!", (int)TextForMatType.FriendUIText);
        }
        SetText();

        #region 聊天

        emoticons = GetComponent<RectTransform>("emoticons").gameObject;
        return_Btn = GetComponent<Button>("emoticons/Image/Image");
        friend_value = GetComponent<Scrollbar>("fiend_Panel/background/Panle_list/chat/chat_room/Scroll View/Scrollbar Vertical");
        other_speak_obj = GetComponent<RectTransform>("fiend_Panel/background/Panle_list/chat/chat_room/other_speak").gameObject;
        my_speak_obj = GetComponent<RectTransform>("fiend_Panel/background/Panle_list/chat/chat_room/my_speak").gameObject;
        chat_content = GetComponent<RectTransform>("fiend_Panel/background/Panle_list/chat/chat_room/Scroll View/Viewport/Content").gameObject;
        record_Btn = GetComponent<Button>("fiend_Panel/background/Panle_list/chat/input_btn/record");
        keyboard_btn = GetComponent<Button>("fiend_Panel/background/Panle_list/chat/input_btn/ keyboard");
        face_Btn = GetComponent<Button>("fiend_Panel/background/Panle_list/chat/input_btn/face ");
        send_Btn = GetComponent<Button>("fiend_Panel/background/Panle_list/chat/input_btn/send");
        input_mes = GetComponent<RectTransform>("fiend_Panel/background/Panle_list/chat/input_btn/input").gameObject;
        m_inputShow = GetComponent<Text>("fiend_Panel/background/Panle_list/chat/input_btn/input/add");
        mes_add = GetComponent<NewInputFile>("fiend_Panel/background/Panle_list/chat/input_btn/input");
        voice_enable = GetComponent<Button>("fiend_Panel/background/Panle_list/chat/input_btn/voice_btn");
        record_Btn.interactable = false;

        return_Btn.onClick.AddListener(delegate { emoticons.gameObject.SetActive(false); });
        friend_value.onValueChanged.AddListener(delegate { CanshowHint(); });

        record_Btn.onClick.AddListener(delegate
        {
            input_mes.gameObject.SetActive(false);//输入
            face_Btn.gameObject.SetActive(false);//表情
            voice_enable.gameObject.SetActive(true);//录音框
            record_Btn.gameObject.SetActive(false);//录音
            keyboard_btn.gameObject.SetActive(true);//键盘
            send_Btn.interactable = false;
        });
        keyboard_btn.onClick.AddListener(delegate
        {
            input_mes.gameObject.SetActive(true);
            face_Btn.gameObject.SetActive(true);//表情
            voice_enable.gameObject.SetActive(false);
            record_Btn.gameObject.SetActive(true);
            keyboard_btn.gameObject.SetActive(false);
            send_Btn.interactable = true;
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
                var message = Util.ValidateSensitiveWords(str);
                WordMySend(0, message);
            }
            else moduleGlobal.ShowMessage(friend_text[35]);

        });
        voice_enable.SetPressDelay(0.2f);
        voice_enable.onPressed().AddListener(a =>
        {
            if (a) { }
            if (!a)
            {
                Debug.Log("松开发送录音");
                string str = string.Empty;
                WordMySend(2, str);
            }
        });

        #endregion

        #region path
        m_chatGroup = GetComponent<VerticalLayoutGroup>("fiend_Panel/background/Panle_list/chat/chat_room/Scroll View/Viewport/Content");
        AllHeight = m_chatGroup.padding.top;
        ChatNothing = GetComponent<RectTransform>("fiend_Panel/background/Panle_list/no_chat_panel").gameObject;
        FlistNothing = GetComponent<RectTransform>("fiend_Panel/background/Panle_list/nothing").gameObject;
        ApplyNothing = GetComponent<RectTransform>("fiend_Panel/background/Panle_list/no_request_panel").gameObject;

        m_newThisObj = GetComponent<Button>("fiend_Panel/background/Panle_list/chat/downBtn");

        m_newThisTxt = GetComponent<Text>("fiend_Panel/background/Panle_list/chat/downBtn/Text");

        m_lateHint = GetComponent<Image>("fiend_Panel/background/content/lateList/latehint").gameObject;
        Lately_Btn = GetComponent<Toggle>("fiend_Panel/background/content/lateList/packUp");

        Chat_Panel = GetComponent<RectTransform>("fiend_Panel/background/Panle_list/chat");
        Chat_name = GetComponent<Text>("fiend_Panel/background/Panle_list/chat/chatname/textname");
        Chat_level = GetComponent<Text>("fiend_Panel/background/Panle_list/chat/chatlevel/textlevel");
        Chat_headbg = GetComponent<RectTransform>("fiend_Panel/background/Panle_list/chat/bg/mask").gameObject;
        Chat_id = GetComponent<Text>("fiend_Panel/background/Panle_list/chat/chatid");
        friendNow = GetComponent<Text>("fiend_Panel/background/content/friendList/fnow");
        friendTop = GetComponent<Text>("fiend_Panel/background/content/friendList/fnow/ftop");

        Chat_details = GetComponent<Button>("fiend_Panel/background/Panle_list/chat/detailsButton");
        Chat_deleted = GetComponent<Button>("fiend_Panel/background/Panle_list/chat/deleteButton");
        m_chatBlack = GetComponent<Button>("fiend_Panel/background/Panle_list/chat/blackBtn");
        m_blackRemoveBtn = GetComponent<Button>("fiend_Panel/background/Panle_list/chat/removeBtn");
        m_chatAddBtn = GetComponent<Button>("fiend_Panel/background/Panle_list/chat/addButton");
        m_blackLOck = GetComponent<RectTransform>("fiend_Panel/background/Panle_list/chat/lockImg");
        select_Btn = GetComponent<Button>("fiend_Panel/background/Panle_list/selectfriend/selectback/select");
        m_chatToogle = GetComponent<Toggle>("fiend_Panel/background/myfriend");
        addFriend = GetComponent<Toggle>("fiend_Panel/background/addfriend");
        friendRequests = GetComponent<Toggle>("fiend_Panel/background/friendrequests");
        friendBtn = GetComponent<Toggle>("fiend_Panel/background/content/friendList/packUp");

        selectfriend = GetComponent<RectTransform>("fiend_Panel/background/Panle_list/selectfriend");
        select_Id = GetComponent<InputField>("fiend_Panel/background/Panle_list/selectfriend/selectback/idselect");
        recommend_panel = GetComponent<RectTransform>("fiend_Panel/background/Panle_list/selectfriend/recommend_panel");
        RecommendObj = GetComponent<RectTransform>("fiend_Panel/background/Panle_list/selectfriend/recommend_panel/random");

        switch_Btn = GetComponent<Button>("fiend_Panel/background/Panle_list/selectfriend/recommend_panel/switch");
        selectok_panel = GetComponent<RectTransform>("fiend_Panel/background/Panle_list/selectfriend/selectok_panel");
        details_selectok = GetComponent<Button>("fiend_Panel/background/Panle_list/selectfriend/selectok_panel/selectok");

        selectok_bg = GetComponent<RectTransform>("fiend_Panel/background/Panle_list/selectfriend/selectok_panel/selectok/bg/mask").gameObject;
        selectok_name = GetComponent<Text>("fiend_Panel/background/Panle_list/selectfriend/selectok_panel/selectok/name");
        selectok_level = GetComponent<Text>("fiend_Panel/background/Panle_list/selectfriend/selectok_panel/selectok/level");
        selectok_IDFalse = GetComponent<Text>("fiend_Panel/background/Panle_list/selectfriend/selectok_panel/selectok/ID");
        selectok_add = GetComponent<Button>("fiend_Panel/background/Panle_list/selectfriend/selectok_panel/selectok/add"); //用来控制发送添加请求
        selectfalse_panel = GetComponent<RectTransform>("fiend_Panel/background/Panle_list/selectfriend/selectfalse_panel");

        requestslist = GetComponent<RectTransform>("fiend_Panel/background/Panle_list/requestslist");

        Apply_hint = GetComponent<Image>("fiend_Panel/background/friendrequests/hint");// 默认应该是隐藏的 只有在服务器发来好友申请时出现

        m_blackToggle = GetComponent<Toggle>("fiend_Panel/background/content/blackList/packUp");

        Apply_hint.gameObject.SetActive(false);
        selectfriend.gameObject.SetActive(false);
        recommend_panel.gameObject.SetActive(true);
        selectok_panel.gameObject.SetActive(false);
        selectfalse_panel.gameObject.SetActive(false);
        requestslist.gameObject.SetActive(false);

        m_friendScroll = GetComponent<ScrollView>("fiend_Panel/background/content/friendList/itemList");
        m_lateScroll = GetComponent<ScrollView>("fiend_Panel/background/content/lateList/itemList");
        m_blackScroll = GetComponent<ScrollView>("fiend_Panel/background/content/blackList/itemList");
        m_friendScroll.gameObject.SetActive(false);
        m_lateScroll.gameObject.SetActive(false);
        m_blackScroll.gameObject.SetActive(false);
        m_template = GetComponent<RectTransform>("fiend_Panel/background/template");
        m_listbgTop = GetComponent<RectTransform>("fiend_Panel/background/content/friendList/bg");
        _layOut = GetComponent<VerticalLayoutGroup>("fiend_Panel/background/content");

        m_friendSize = GetComponent<TweenSize>("fiend_Panel/background/content/friendList");
        m_lateSize = GetComponent<TweenSize>("fiend_Panel/background/content/lateList");
        m_blackSize = GetComponent<TweenSize>("fiend_Panel/background/content/blackList");

        #endregion

        #region onclick
        m_blackToggle.onValueChanged.AddListener(delegate
        {
            if (!m_blackToggle.isOn)
            {
                m_blackSize.PlayReverse();
                return;
            }
            m_fLateChange = true;
            m_chatToogle.isOn = true;
            m_fLateChange = false;
            SetFromTo(m_blackSize, moduleFriend.BlackList, m_blackScroll);
            BlackBtn();
            m_blackScroll.progress = 0;
        });
        Lately_Btn.onValueChanged.AddListener(delegate
        {
            if (!Lately_Btn.isOn)
            {
                m_lateSize.PlayReverse();
                return;
            }
            m_fLateChange = true;
            m_chatToogle.isOn = true;
            m_fLateChange = false;
            SetFromTo(m_lateSize, moduleChat.Late_ListAllInfo, m_lateScroll);

            Late_Btn();
            m_lateHint.gameObject.SetActive(false);
            m_lateScroll.progress = 0;
        });

        friendBtn.onValueChanged.AddListener(delegate
       {
           if (!friendBtn.isOn)
           {
               m_friendSize.PlayReverse();
               return;
           }

           m_fLateChange = true;
           m_chatToogle.isOn = true;
           m_fLateChange = false;
           SetFromTo(m_friendSize, moduleFriend.FriendList, m_friendScroll);
           SetChatBtnClick();
           ApplyNothing.gameObject.SetActive(false);
           selectfriend.gameObject.SetActive(false);
           requestslist.gameObject.SetActive(false);
           emoticons.gameObject.SetActive(false);
           m_friendScroll.progress = 0;
       });

        m_chatToogle.onValueChanged.AddListener(delegate
        {
            if (!m_chatToogle.isOn) return;
            SetChatBtnClick();
            m_friendScroll.progress = 0;
        });

        addFriend.onValueChanged.AddListener(delegate
       {
           if (!addFriend.isOn) return;

           emoticons.gameObject.SetActive(false);
           ApplyNothing.gameObject.SetActive(false);
           selectok_add.interactable = true;
           moduleFriend.SendSwitchMes();
           select_Id.text = null;

       });
        friendRequests.onValueChanged.AddListener(delegate
       {
           if (!friendRequests.isOn) return;

           emoticons.gameObject.SetActive(false);
           Chat_Panel.gameObject.SetActive(false);
           ChatNothing.gameObject.SetActive(false);
           selectfriend.gameObject.SetActive(false);
           requestslist.gameObject.SetActive(true);
           selectok_add.interactable = true;
           ApplyHint();
           ShowApplyNothing();
       });

        Chat_details.onClick.AddListener(delegate
        {
            emoticons.gameObject.SetActive(false);
            var playerId = Util.Parse<ulong>(Chat_id.text);
            moduleFriend.SendLookDetails(playerId);
        });

        m_newThisObj.onClick.AddListener(delegate
        {
            friend_value.value = 0;
            m_newThisObj.gameObject.SetActive(false);
        });
        details_selectok.onClick.AddListener(delegate
        {
            ulong playerId = Util.Parse<ulong>(selectok_IDFalse.text);
            moduleFriend.SendLookDetails(playerId);
        });

        selectok_add.onClick.AddListener(Selectokadd_show);

        Chat_deleted.onClick.AddListener(delegate
        {
            var playerId = Util.Parse<ulong>(Chat_id.text);
            if (Module_Active.instance.CheckCoop(playerId)) moduleGlobal.ShowMessage(friend_text[63]);
            else Window_Alert.ShowAlert(friend_text[64], true, true, true, () => { moduleFriend.SendDeleteMes(playerId); }, null, "", "");
        });
        m_chatBlack.onClick.AddListener(delegate
        {
            var playerId = Util.Parse<ulong>(Chat_id.text);
            if (Module_Active.instance.CheckCoop(playerId)) moduleGlobal.ShowMessage(friend_text[63]);
            else Window_Alert.ShowAlert(friend_text[65], true, true, true, () => { moduleFriend.SendShieDing(playerId); }, null, "", "");
        });
        m_blackRemoveBtn.onClick.AddListener(delegate
        {
            var playerId = Util.Parse<ulong>(Chat_id.text);
            Window_Alert.ShowAlert(friend_text[66], true, true, true, () => { moduleFriend.RecoverShieDing(playerId); }, null, "", "");
        });
        m_chatAddBtn.onClick.AddListener(delegate
        {
            var playerId = Util.Parse<ulong>(Chat_id.text);
            moduleFriend.SendAddMes(playerId);
            m_chatAddBtn.interactable = false;
        });


        select_Btn.onClick.AddListener(OnselectFriend);

        switch_Btn.onClick.AddListener(delegate
        {
            selectok_add.interactable = true;
            moduleFriend.SendSwitchMes();
        });

        #endregion

        EmjioList = ConfigManager.GetAll<FaceName>();
        ApplynewList = new DataSource<PPlayerInfo>(moduleFriend.Apply_playerList, GetComponent<ScrollView>("fiend_Panel/background/Panle_list/requestslist/scrollView"), ApplyObjInfo);
        FrinednewList = new DataSource<PPlayerInfo>(moduleFriend.FriendList, m_friendScroll, MyFriendList);
        RecentlynewList = new DataSource<PPlayerInfo>(moduleChat.Late_ListAllInfo, m_lateScroll, LastFriendList);
        BlackList = new DataSource<PPlayerInfo>(moduleFriend.BlackList, m_blackScroll, SetBlackInfo);
        emjioList = new DataSource<FaceName>(EmjioList, GetComponent<ScrollView>("emoticons/Image/jpg_img"), EmojiSet, EmojiClick);
        moduleFriend.windowisopen = true;
        Friend_topClamp();//获取下最多好友限制
        Expression();
        SetRecommend();
    }

    private void SetRecommend()
    {
        Recommend.Clear();
        foreach (Transform item in RecommendObj)
        {
            item.gameObject.SetActive(false);
            Recommend.Add(item.gameObject);
        }
    }

    private void SetText()
    {
        Util.SetText(GetComponent<Text>("fiend_Panel/background/Panle_list/chat/blackBtn/delete_text"), friend_text[67]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/Panle_list/chat/removeBtn/delete_text"), friend_text[68]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/Panle_list/chat/lockImg/Text"), friend_text[69]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/content/blackList/preaward_Txt"), friend_text[72]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/Panle_list/selectfriend/selectok_panel/recommend/recommend_text"), friend_text[50]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/Panle_list/chat/Button/Text"), friend_text[49]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/Panle_list/chat/tip_txt"), friend_text[48]);
        Util.SetText(GetComponent<Text>("RawImage/title"), friend_text[0]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/myfriend/Text"), friend_text[21]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/addfriend/Text"), friend_text[22]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/friendrequests/Text"), friend_text[23]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/content/friendList/preaward_Txt"), friend_text[0]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/content/lateList/preaward_Txt"), friend_text[20]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/myfriend/selected/Text"), friend_text[21]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/addfriend/selected/Text"), friend_text[22]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/friendrequests/selected/Text"), friend_text[23]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/Panle_list/no_chat_panel/Text"), friend_text[32]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/Panle_list/no_request_panel/name"), friend_text[33]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/Panle_list/chat/input_btn/send/Text"), friend_text[3]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/Panle_list/chat/input_btn/voice_btn/Text"), friend_text[9]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/Panle_list/chat/deleteButton/delete_text"), friend_text[24]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/Panle_list/selectfriend/selectback/select/Text"), friend_text[25]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/Panle_list/selectfriend/selectfalse_panel/name"), friend_text[34]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/Panle_list/requestslist/recommend/recommend_text"), friend_text[19]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/Panle_list/selectfriend/recommend_panel/switch/Text"), friend_text[7]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/Panle_list/selectfriend/selectback/idselect/placeholder"), friend_text[6]);
        Util.SetText(GetComponent<Text>("fiend_Panel/background/Panle_list/selectfriend/recommend_panel/recommend/recommend_text"), friend_text[5]);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault(1, false);

        FriendColor();
        m_lateHint.gameObject.SetActive(false);
        m_lateHint.gameObject.SetActive(moduleChat.ShowLateHint());
        ApplyHint();

        if (moduleFriend.FriendList.Count > 0)
        {
            var have = moduleFriend.TeamInvate.Exists(a => a == moduleFriend.FriendList[0].roleId);
            if (have) moduleFriend.TeamInvate.Remove(moduleFriend.FriendList[0].roleId);
            Chat_id.text = moduleFriend.FriendList[0].roleId.ToString();
        }
        else Chat_id.text = string.Empty;

        if (moduleFriend.m_friendOpenType == OpenFriendType.Union)
        {
            if (moduleUnion.m_unionChatID == 0) return;
            Chat_id.text = moduleUnion.m_unionChatID.ToString();
        }
        else if (moduleFriend.m_friendOpenType == OpenFriendType.World)
        {
            if (moduleChat.word_play_details == null) return;
            Chat_id.text = moduleChat.word_play_details.roleId.ToString();
        }

        if (m_chatToogle.isOn) SetChatBtnClick();
        else m_chatToogle.isOn = true;
        moduleFriend.m_friendOpenType = OpenFriendType.Normal;

        moduleChat.HavePastMes = false;//红点要消失了main 里
        
        m_newThisObj.gameObject.SetActive(false);
        m_ShowSort = true;
    }

    private void SetChatBtnClick()
    {
        if (m_fLateChange) return;

        if (m_ShowSort) moduleFriend.FriendList = moduleFriend.SortWay();

        friendBtn.isOn = true;
        selectok_add.interactable = true;

        ChatNothing.gameObject.SetActive(false);
        FlistNothing.gameObject.SetActive(false);
        ApplyNothing.gameObject.SetActive(false);
        // chat界面
        if (moduleFriend.FriendList.Count > 0)
        {
            int length = chat_obj.Count;
            for (int i = 0; i < length; i++)
            {
                GameObject.Destroy(chat_obj.Dequeue());
            }
            chat_obj.Clear();
            if (moduleFriend.m_friendOpenType == OpenFriendType.Normal)
            {
                Chat_id.text = moduleFriend.FriendList[0].roleId.ToString();
            }
            change_chat_friend(Util.Parse<ulong>(Chat_id.text));
        }
        else
        {
            Chat_Panel.gameObject.SetActive(false);
            ChatNothing.gameObject.SetActive(true);
            FlistNothing.gameObject.SetActive(true);
        }
        FrinednewList.SetItems(moduleFriend.FriendList);
        FriendColor();
    }

    #region 加载表情资源

    public void Expression()
    {
        FriendColor();
        emjioList.UpdateItems();
    }

    private void EmojiSet(RectTransform rt, FaceName Info)
    {
        GameObject a = Level.GetPreloadObject(Info.head_icon);
        if (a == null) return;
        Util.AddChild(rt, a.transform);

    }
    private void EmojiClick(RectTransform rt, FaceName info)
    {
        if (moduleChat.CanChatWord && info != null)
        {
            WordMySend(1, info.head_icon);
            emoticons.SetActive(false);
        }
    }
    #endregion

    #region 聊天的发送 聊天创建

    private void WordMySend(int type, string content, ulong sdf = 0)
    {
        if (modulePlayer.BanChat == 2)
        {
            moduleGlobal.ShowMessage(630, 0);
            return;
        }
        if (string.IsNullOrEmpty(Chat_id.text))
        {
            moduleGlobal.ShowMessage(friend_text[70]);
            return;
        }

        sdf = Util.Parse<ulong>(Chat_id.text);
        moduleChat.SendFriendMessage(content, type, sdf);
        position_clear(true, type, content, my_speak_obj, modulePlayer.name_, modulePlayer.avatar, sdf, modulePlayer.gender, modulePlayer.avatarBox, modulePlayer.proto, 0);
    }

    private void position_clear(bool mysend, int type, string matter, GameObject objclone, string word_name, string word_head, ulong id_key, int gender, int headbox, int proto, int playerSend)
    {
        //0 文本 1 图片2 语音 type
        // //0 世界接受 //1世界我发 2好友接收的 3 好友我发的 只有在0的头像才可以点击(mes_type)

        GameObject obj = GameObject.Instantiate(objclone);

        if (chat_obj.Count >= moduleChat.ChatNum)
        {
            GameObject destoryobj = chat_obj.Dequeue();
            if (destoryobj != null)
            {
                ChatMes hight = destoryobj.GetComponent<ChatMes>();
                ContentHeight(hight.This_height, false);//减小content高度
                GameObject.Destroy(destoryobj);
            }
        }
        chat_obj.Enqueue(obj);

        bool is_cun = moduleChat.Friend_Id_key.Exists(a => a == id_key);
        if (!is_cun) moduleChat.Friend_Id_key.Add(id_key);//不存在填进去

        ChatMes chat_mes = obj.GetComponent<ChatMes>();
        obj.transform.SetParent(chat_content.transform, false);//加进
        obj.SetActive(true);

        chat_mes.show_details(mysend, word_name, word_head, gender, id, proto);
        if (playerSend == 0 || playerSend == 2) playerSend = 0;
        chat_mes.caht_show(type, matter, mysend, headbox, playerSend);

        ContentHeight(chat_mes.This_height, true);//增大content高度

        if (mysend || moduleChat.friend_chat_change)//如果是切换好友 或者自己发送 则每次都要刷到最下面
        {
            mes_add.text = string.Empty;
            if (Lately_Btn.isOn) RecentlynewList.SetItems(moduleChat.Late_ListAllInfo);
            ContentSort();
        }
    }

    private void ContentHeight(float itemheight, bool isadd)//手动更改content高度
    {
        VerticalLayoutGroup val = chat_content.GetComponent<VerticalLayoutGroup>();
        RectTransform o = chat_content.GetComponent<RectTransform>();
        if (isadd)
        {
            AllHeight += val.spacing;
            AllHeight += itemheight;
        }
        else
        {
            AllHeight -= val.spacing;
            AllHeight -= itemheight;
        }
        o.sizeDelta = new Vector2(0, AllHeight);
    }

    private void ContentSort()//是否拉到最低处
    {
        var scroll = chat_content.transform.parent.parent.GetComponent<ScrollRect>();
        if (scroll != null)
        {
            scroll.verticalNormalizedPosition = 0;//每次我发消息或者切换时候调用最下
            m_newThisObj.gameObject.SetActive(false);
        }
    }

    #endregion

    #region 收到好友消息时候的红点以及信息变化

    private void ReciveMes(ScChatPrivate other)//收到好友消息
    {
        if (Lately_Btn.isOn)
        {
            RecentlynewList.SetItems(moduleChat.Late_ListAllInfo);
            if (moduleFriend.RefreshLateState) DeletListState();
        }
        else if (friendBtn.isOn) FrinednewList.SetItems(moduleFriend.FriendList);

        PPlayerInfo friend_other = moduleFriend.FriendList.Find(a => a.roleId == other.sendId);
        if (friend_other == null) friend_other = moduleChat.LatePlayerInfo(other.sendId);
        if (friend_other != null)
        {
            string zzzzz = null;
            if (!string.IsNullOrEmpty(Chat_id.text)) zzzzz = Chat_id.text;

            //不是正在聊天的好友或者是当前好友但是没在底部
            if (other.sendId.ToString() != zzzzz)
            {
                string bbb = other.sendId.ToString();
                ChangeUpdate(FrinednewList, bbb, moduleFriend.FriendList);
                ChangeUpdate(RecentlynewList, Chat_id.text, moduleChat.Late_ListAllInfo);
            }
            else
            {
                position_clear(false, other.type, other.content, other_speak_obj, friend_other.name, friend_other.avatar, friend_other.roleId, friend_other.gender, friend_other.headBox, friend_other.proto, Util.Parse<int>(other.tag));//如果id与自己id不同
                CanshowHint(true); //出现有新消息的提示   
            }
        }
        if (!Lately_Btn.isOn) m_lateHint.gameObject.SetActive(moduleChat.ShowLateHint());

    }

    private void CanshowHint(bool recv = false)//是否拉到了最低时候 content的高度大于320并且高度减去posy
    {
        //cht_id就对应的预制红点小消失，并判断未读里是否存在 存在则清除
        //是正在聊天的人要不要显示红点 height-posy>320显示

        float lerp = GeneralConfigInfo.defaultConfig.friendLerp;
        if (recv) lerp = 0;

        if (friend_value.gameObject.activeInHierarchy && friend_value.value >= lerp && friend_value.value != 1)
        {
            m_newThisObj.gameObject.SetActive(true);

            bool Noread = moduleChat.Past_mes.Exists(a => a == Util.Parse<ulong>(Chat_id.text));
            if (Noread) Util.SetText(m_newThisTxt, ConfigText.GetDefalutString(222, 14));
            else Util.SetText(m_newThisTxt, ConfigText.GetDefalutString(222, 15));

        }
        else if (friend_value.value == 0)
        {
            bool Noread = moduleChat.Past_mes.Exists(a => a == Util.Parse<ulong>(Chat_id.text));
            if (Noread)
                moduleChat.Past_mes.Remove(Util.Parse<ulong>(Chat_id.text));

            m_newThisObj.gameObject.SetActive(false);
        }

        //最近和好友列表都要判断
        ChangeUpdate(FrinednewList, Chat_id.text, moduleFriend.FriendList);
        ChangeUpdate(RecentlynewList, Chat_id.text, moduleChat.Late_ListAllInfo);
    }

    private void ChangeUpdate(DataSource<PPlayerInfo> data, string nowid, List<PPlayerInfo> ListPlayer)
    {
        for (int i = 0; i < ListPlayer.Count; i++)
        {
            if (nowid == ListPlayer[i].roleId.ToString())
            {
                data.UpdateItem(i);
            }
        }
    }

    #endregion

    #region 友情点
    private void UpDatePoint(ulong roleId, bool friend)
    {
        if (friend)
        {
            var index1 = moduleFriend.FriendList.FindIndex(a => a.roleId == roleId);
            if (index1 != -1) FrinednewList.UpdateItem(index1);
        }
        else FrinednewList.SetItems(moduleFriend.FriendList);
        var index2 = moduleChat.Late_ListAllInfo.FindIndex(a => a.roleId == roleId);
        if (index2 != -1) RecentlynewList.UpdateItem(index2);
    }
    private void SetPointState(RectTransform rt, PPlayerInfo info, bool show = true)
    {
        //友情点的状态
        var givingBtn = rt.Find("givingBtn").GetComponent<Button>();
        var failurBtn = rt.Find("failureBtn");
        var recrivedBtn = rt.Find("receivedBtn").GetComponent<Button>();
        if (!show)
        {
            givingBtn.gameObject.SetActive(false);
            failurBtn.gameObject.SetActive(false);
            recrivedBtn.gameObject.SetActive(false);
            return;
        }

        int state = -1;
        var have = moduleFriend.AllFriendPoint.Exists(a => a == info.roleId);
        if (have) state = 1;
        else
        {
            var already = moduleFriend.AlreadySendFriend.Exists(a => a == info.roleId);
            state = already == false ? 0 : 2;
        }
        var f = moduleFriend.FriendList.Find(a => a.roleId == info.roleId);
        if (f == null) state = -1;

        givingBtn.gameObject.SetActive(state == 0);
        failurBtn.gameObject.SetActive(state == 2);
        recrivedBtn.gameObject.SetActive(state == 1);
        givingBtn.onClick.RemoveAllListeners();
        givingBtn.onClick.AddListener(delegate { moduleFriend.SendFriendPoint(info.roleId); });
        recrivedBtn.onClick.RemoveAllListeners();
        recrivedBtn.onClick.AddListener(delegate { moduleFriend.GetFriendPoint(info.roleId); });
    }
    #endregion

    #region 好友列表 (整体创建，点击事件，好友切换，聊天页信息的更改)

    private void MyFriendList(RectTransform rt, PPlayerInfo info)
    {
        if (moduleFriend.FriendList.Count == 0) return;
        FriendPrecast pracest = rt.gameObject.GetComponentDefault<FriendPrecast>();
        int i = 1;
        if (info.roleId == moduleFriend.FriendList[0].roleId) i = 0;
        pracest.DelayAddData(info, i, 0);
        SetPointState(rt, info);

        Button click = rt.Find("onclick").GetComponentDefault<Button>();
        click.onClick.RemoveAllListeners();
        click.onClick.AddListener(delegate { Onclick(rt, info); });
        TeamHint(rt, info.roleId);

        if (info.state != 0 || info.offLineTime <= 0) return;
        var line = rt.Find("offline/offline_text").GetComponent<Text>();
        SetTimeTxt(info.offLineTime, line);

    }
    private void SetTimeTxt(int recordTime, Text timeTxt)
    {
        DateTime old = Util.GetDateTime(recordTime);
        DateTime now = Util.GetServerLocalTime();
        TimeSpan span = now - old;
        if (span.Days < 0)
        {
            Logger.LogError("Window_Unionreward record info time {0}", old);
            return;
        }

        if (span.Days > 30) Util.SetText(timeTxt, ConfigText.GetDefalutString(631, 40));
        else if (span.Days > 0) Util.SetText(timeTxt, ConfigText.GetDefalutString(631, 15), span.Days);
        else if (span.Days == 0 && span.Hours > 0) Util.SetText(timeTxt, ConfigText.GetDefalutString(631, 41), span.Hours);
        else if (span.Days == 0 && span.Hours == 0) Util.SetText(timeTxt, ConfigText.GetDefalutString(631, 42), span.Minutes);
    }

    private void TeamHint(RectTransform rt, ulong roleId)//组队提示信息
    {
        RectTransform teamHint = rt.GetComponent<RectTransform>("team");
        var have = moduleFriend.TeamInvate.Exists(a => a == roleId);
        teamHint.SafeSetActive(have);
    }

    private void Onclick(RectTransform rt, PPlayerInfo this_Info)//点击事件
    {
        m_fLateChange = true;
        m_chatToogle.isOn = true;
        m_fLateChange = false;

        ApplyNothing.gameObject.SetActive(false);
        selectfriend.gameObject.SetActive(false);
        requestslist.gameObject.SetActive(false);
        Chat_Panel.gameObject.SetActive(true);
        ChatNothing.gameObject.SetActive(false);

        string this_id = this_Info.roleId.ToString();
        if (Chat_id.text == this_id) return;

        if (this_id != Chat_id.text)
        {
            Chat_id.text = this_id.ToString();
            change_chat_friend(Util.Parse<ulong>(this_id));//每次点击切换好友聊天界面
        }
        GameObject friend_hint = rt.gameObject.transform.Find("hint_").gameObject;
        friend_hint.gameObject.SetActive(false);

        bool dd = moduleChat.Past_mes.Exists(a => a == Util.Parse<ulong>(this_id));
        if (dd) moduleChat.Past_mes.Remove(Util.Parse<ulong>(this_id));//点击肯定消除

        var have = moduleFriend.TeamInvate.Exists(a => a == this_Info.roleId);
        if (have) moduleFriend.TeamInvate.Remove(this_Info.roleId);
        RectTransform teamHint = rt.GetComponent<RectTransform>("team");
        teamHint?.gameObject.SetActive(false);

    }
    private void change_chat_friend(ulong id_change)//切换好友
    {
        if (moduleFriend.checkPlayer != null)
        {
            moduleFriend.lastCheckPlayer = moduleFriend.checkPlayer;
        }
        PPlayerInfo info = moduleFriend.FriendList.Find(a => a.roleId == id_change);
        if (info == null) info = moduleChat.LatePlayerInfo(id_change);
        if (info == null) info = moduleFriend.BlackList.Find(a => a.roleId == id_change);
        if (info == null) return;
        moduleFriend.checkPlayer = info;

        //先更换选择的对象再进行更新
        if (moduleFriend.checkPlayer != null)
        {
            if (friendBtn.isOn) ChangeUpdate(FrinednewList, moduleFriend.checkPlayer.roleId.ToString(), moduleFriend.FriendList);
            else if (Lately_Btn.isOn) ChangeUpdate(RecentlynewList, moduleFriend.checkPlayer.roleId.ToString(), moduleChat.Late_ListAllInfo);
            else if (m_blackToggle.isOn) ChangeUpdate(BlackList, moduleFriend.checkPlayer.roleId.ToString(), moduleFriend.BlackList);
        }
        if (moduleFriend.lastCheckPlayer != null)
        {
            if (friendBtn.isOn) ChangeUpdate(FrinednewList, moduleFriend.lastCheckPlayer.roleId.ToString(), moduleFriend.FriendList);
            else if (Lately_Btn.isOn) ChangeUpdate(RecentlynewList, moduleFriend.lastCheckPlayer.roleId.ToString(), moduleChat.Late_ListAllInfo);
            else if (m_blackToggle.isOn) ChangeUpdate(BlackList, moduleFriend.lastCheckPlayer.roleId.ToString(), moduleFriend.BlackList);
        }

        AllHeight = m_chatGroup.padding.top;
        int length = chat_obj.Count;
        for (int i = 0; i < length; i++)
        {
            GameObject.Destroy(chat_obj.Dequeue());
        }
        chat_obj.Clear();

        ChatBaseInfo(id_change);//更改聊天页基础信息
        var black = moduleFriend.BlackList.Exists(a => a.roleId == id_change);
        if (!black) moduleChat.ChangeFriend(id_change);//更改聊天记录

        bool dd = moduleChat.Past_mes.Exists(a => a == id_change);
        if (dd) moduleChat.Past_mes.Remove(id_change);//点击肯定消除

        if (!Lately_Btn.isOn) m_lateHint.gameObject.SetActive(moduleChat.ShowLateHint());

        mes_add.text = null;
        Chat_Panel.gameObject.SetActive(true);
        ChatNothing.gameObject.SetActive(false);
        FlistNothing.gameObject.SetActive(false);
        input_mes.gameObject.SetActive(true);
        face_Btn.gameObject.SetActive(true);//表情
        voice_enable.gameObject.SetActive(false);
        record_Btn.gameObject.SetActive(true);
        keyboard_btn.gameObject.SetActive(false);
        emoticons.gameObject.SetActive(false);
        m_newThisObj.gameObject.SetActive(false);

    }

    private void ChatBaseInfo(ulong playerId)//聊天页 信息
    {
        var friendchat = moduleFriend.PlayerInfo(playerId);
        if (friendchat == null) return;

        Module_Avatar.SetClassAvatar(Chat_headbg, friendchat.proto, false, friendchat.gender);
        GameObject chatbox = Chat_headbg.transform.parent.gameObject;
        headBoxFriend chatboxs = chatbox.GetComponentDefault<headBoxFriend>();
        chatboxs.HeadBox(friendchat.headBox);

        Util.SetText(Chat_name, friendchat.name);
        var str = string.Format(ConfigText.GetDefalutString(218, 37), friendchat.level.ToString());
        Util.SetText(Chat_level, str);
        Util.SetText(Chat_id, friendchat.roleId.ToString());

        var type = moduleFriend.PlayerType(playerId);
        Chat_deleted.gameObject.SetActive(type == 0);
        m_chatBlack.gameObject.SetActive(type != 2);
        m_blackRemoveBtn.gameObject.SetActive(type == 2);
        m_chatAddBtn.SafeSetActive(type != 0 && type != 2);
        m_blackLOck.gameObject.SetActive(type == 2);
        m_inputShow.text = type == 1 ? friend_text[71] : friend_text[31];
        send_Btn.enabled = type == 2 ? false : true;
        send_Btn.interactable = type == 2 ? false : true;

        var add = moduleFriend.AddApplyID.Exists(a => a == friendchat.roleId);
        m_chatAddBtn.interactable = !add;

    }

    #endregion

    #region 限制判断（好友最大值，好友数颜色判断，能否再添加）

    private void Add_overflow(ulong ID_now)//是否能发送添加请求
    {
        if (moduleFriend.FriendList.Count < moduleFriend.FriendNumTop)
        {
            moduleFriend.SendAddMes(ID_now);
            SetCommedInter();
        }
        else moduleGlobal.ShowMessage(friend_text[13]);//提示 好友已满
    }
    private void SetCommedInter()
    {
        for (int i = 0; i < moduleFriend.AddApplyID.Count; i++)
        {
            for (int j = 0; j < moduleFriend.Recommend.Count; j++)
            {
                if (moduleFriend.AddApplyID[i] == moduleFriend.Recommend[j].roleId)
                {
                    var add = Recommend[j].transform.Find("add").GetComponentDefault<Button>();
                    add.interactable = false;
                }
            }
            if (moduleFriend.SearchList.Count > 0)
            {
                if (moduleFriend.SearchList[0].roleId == moduleFriend.AddApplyID[i])
                {
                    selectok_add.interactable = false;
                }
            }
        }
    }

    private void Friend_topClamp()//设置最高的人数限制
    {
        friendTop.text = string.Format("/{0}", moduleFriend.FriendNumTop.ToString());
    }

    private void FriendColor()//颜色是否变化
    {
        friendNow.text = moduleFriend.FriendList.Count.ToString();
        if (moduleFriend.FriendList.Count < moduleFriend.FriendNumTop)
            friendNow.color = GeneralConfigInfo.defaultConfig.FriendNormal;
        else friendNow.color = GeneralConfigInfo.defaultConfig.FriendTop;
    }

    #endregion

    #region 判断搜素格式 搜索玩家结果 随机玩家

    private void Selectokadd_show()//搜索之后添加按钮判断
    {
        ulong playerId = Util.Parse<ulong>(selectok_IDFalse.text);

        var black = moduleFriend.CanAddPlayer(playerId);
        if (black) return;
        Add_overflow(playerId);
        selectok_add.interactable = false;
    }

    private void OnselectFriend()//判断搜索输入是否正确
    {
        selectok_add.interactable = true;

        //搜索好友请求
        //for (int i = 0; i < select_Id.text.Length; i++)
        //{
        //    if (!Char.IsNumber(select_Id.text, i)) isaccord = false;
        //    else isaccord = true;
        //}
        if (!string.IsNullOrEmpty(select_Id.text))
        {
            moduleFriend.SendSelectMes(select_Id.text);
            //moduleGlobal.ShowMessage(friend_text[14]);
        }
        else
        {
            select_Id.text = null;
            moduleGlobal.ShowMessage(friend_text[15]);
        }
    }

    private void FriendSearchReplay(int isselcet_result, PPlayerInfo isselect_playerinfo)//搜索的结果
    {
        if (isselcet_result == 2 || isselcet_result == 3)
        {
            recommend_panel.gameObject.SetActive(false);//当前页面隐藏
            selectok_panel.gameObject.SetActive(false);
            selectfalse_panel.gameObject.SetActive(true);
            //该玩家不存在
        }
        else if (isselcet_result == 4 || isselcet_result == 5)
        {
            moduleGlobal.ShowMessage(friend_text[17]);
        }
        else if (isselcet_result == 0)
        {
            recommend_panel.gameObject.SetActive(false);//当前页面隐藏

            selectok_panel.gameObject.SetActive(true);
            selectfalse_panel.gameObject.SetActive(false);
            selectok_add.gameObject.SetActive(true);//添加按钮隐藏
            SearchFriendInfo(isselect_playerinfo);
        }
        else if (isselcet_result == 1)
        {
            recommend_panel.gameObject.SetActive(false);//当前页面隐藏

            selectok_panel.gameObject.SetActive(true);
            selectfalse_panel.gameObject.SetActive(false);
            selectok_add.gameObject.SetActive(false);//添加按钮隐藏
            SearchFriendInfo(isselect_playerinfo);
        }
        else if (isselcet_result == 6)
        {
            moduleGlobal.ShowMessage(friend_text[18]);
        }

        if (isselect_playerinfo == null) return;
        selectok_add.interactable = true;
        var apply = moduleFriend.AddApplyID.Exists(a => a == isselect_playerinfo.roleId);
        if (apply) selectok_add.interactable = false;
    }

    private void SearchFriendInfo(PPlayerInfo SearchInfo) // 搜索成功的信息
    {
        if (SearchInfo == null) return;
        Module_Avatar.SetClassAvatar(selectok_bg, SearchInfo.proto, false, SearchInfo.gender);
        GameObject selectbox = selectok_bg.transform.parent.gameObject;
        headBoxFriend seleboxs = selectbox.GetComponentDefault<headBoxFriend>();
        seleboxs.HeadBox(SearchInfo.headBox);

        selectok_name.text = SearchInfo.name;
        selectok_level.text = string.Format(ConfigText.GetDefalutString(218, 37), SearchInfo.level.ToString());
        selectok_IDFalse.text = SearchInfo.roleId.ToString();

    }

    private void FriendRecommend() //三个玩家的信息 
    {
        for (int i = 0; i < Recommend.Count; i++)
        {
            Recommend[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < moduleFriend.Recommend.Count; i++)
        {
            if (i >= Recommend.Count || Recommend[i] == null) continue;
            Recommend[i].gameObject.SetActive(true);
            AroundInfo(moduleFriend.Recommend[i], Recommend[i]);
        }
    }

    private void AroundInfo(PPlayerInfo info, GameObject randomobj)//随机的信息
    {
        Button lookdetail = randomobj.GetComponent<Button>();
        GameObject headbg = randomobj.transform.Find("bg/mask").gameObject;
        Text level = randomobj.transform.Find("level").GetComponent<Text>();
        Text name = randomobj.transform.Find("name").GetComponent<Text>();
        Text IDFalse = randomobj.transform.Find("ID").GetComponent<Text>();
        Button addBtn = randomobj.transform.Find("add").GetComponent<Button>();

        Module_Avatar.SetClassAvatar(headbg, info.proto, false, info.gender);
        GameObject random3box = headbg.transform.parent.gameObject;
        headBoxFriend ranbox3 = random3box.GetComponentDefault<headBoxFriend>();
        ranbox3.HeadBox(info.headBox);

        name.text = info.name;
        level.text = string.Format(ConfigText.GetDefalutString(218, 37), info.level.ToString());
        IDFalse.text = info.roleId.ToString();
        lookdetail.onClick.RemoveAllListeners();
        lookdetail.onClick.AddListener(delegate
        {
            ulong playerId = Util.Parse<ulong>(IDFalse.text);
            moduleFriend.SendLookDetails(playerId);
        });

        addBtn.interactable = true;
        bool haveAdd = moduleFriend.AddApplyID.Exists(a => a == info.roleId);
        var black = moduleFriend.BlackList.Exists(a => a.roleId == info.roleId);
        if (black || haveAdd) addBtn.interactable = false;

        addBtn.onClick.RemoveAllListeners();
        addBtn.onClick.AddListener(delegate
        {
            var blacks = moduleFriend.CanAddPlayer(info.roleId);
            if (blacks) return;
            Add_overflow(info.roleId);
            addBtn.interactable = false;
        });

    }

    #endregion

    #region 删除好友 被好友删除 申请被同意

    private void DeleteFriendIsOk(PPlayerInfo DedeltefriendOk, bool friend, bool late)//删除好友
    {
        if (DedeltefriendOk == null) return;
        //删除好友成功
        if (Chat_id.text == DedeltefriendOk.roleId.ToString()) Chat_id.text = string.Empty;

        Character_details.gameObject.SetActive(false);
        FriendDetailInfo detials = Character_details.GetComponentDefault<FriendDetailInfo>();
        detials.SetButtonShow(false);

        if (m_chatToogle.isOn) Chat_Panel.gameObject.SetActive(false);
        //判断该id与slectok的id是否一致 一致则显示add按钮
        if (DedeltefriendOk.ToString() == selectok_IDFalse.text) selectok_add.gameObject.SetActive(true);

        DeletSame(DedeltefriendOk, friend, late);
        FriendColor();
    }

    private void delete_friend_replay(PPlayerInfo response_deleted, bool friend, bool late)//被删除
    {
        if (response_deleted == null) return;
        FriendDetailInfo detials = Character_details.GetComponentDefault<FriendDetailInfo>();
        if (Chat_id.text == detials.PlayerId.ToString()) detials.SetButtonShow(false);

        //if (Chat_id.text == response_deleted.roleId.ToString())
        //{
        //    Chat_id.text = string.Empty;
        //    return;
        //}
        Chat_id.text = string.Empty;
        Chat_Panel.gameObject.SetActive(false);
        DeletSame(response_deleted, friend, late);
        FriendColor();
    }
    private void DeletSame(PPlayerInfo deletplay, bool friend, bool late)
    {
        FriendColor();

        if (friend) FrinednewList.RemoveItem(deletplay);
        if (late) RecentlynewList.RemoveItem(deletplay);

        if (!m_chatToogle.isOn) return;
        if (moduleFriend.FriendList.Count <= 0)
        {
            FlistNothing.gameObject.SetActive(true);
            ChatNothing.gameObject.SetActive(true);
        }
        else
        {
            ChatNothing.gameObject.SetActive(false);
            FlistNothing.gameObject.SetActive(false);
        }

    }

    private void AddFriendReplayOk() // 被同意申请
    {
        //发送刷新好友的请求 好友列表 添加了好友时
        FrinednewList.SetItems(moduleFriend.FriendList);
        FlistNothing.gameObject.SetActive(false);
        FriendColor();
    }
    #endregion

    #region 申请 （同意申请，拒绝申请 ，申请的红点及背景板显示）

    private void ApplyObjInfo(RectTransform rt, PPlayerInfo Info)
    {
        ApplyFriendInfo apply = rt.gameObject.GetComponentDefault<ApplyFriendInfo>();
        apply.InitItem(Info.roleId, agree_overflow);
        apply.SetInfo(Info);
    }
    private void agree_overflow(ulong Agree_ID)//是否能发送同意请求
    {
        if (moduleFriend.FriendList.Count < moduleFriend.FriendNumTop)
            moduleFriend.SendReplyAgreeMes(Agree_ID);
        else moduleGlobal.ShowMessage(friend_text[13]);//提示 好友已满        
    }

    private void ArgeeFriendIsOk(PPlayerInfo Agreefriend_Play)//同意好友成功
    {
        if (Agreefriend_Play.roleId.ToString() == selectok_IDFalse.text)
        {
            selectok_add.gameObject.SetActive(false);
        }
        FriendColor();
        ApplyHint();
        ShowApplyNothing();//判断还有没有申请

        FlistNothing.gameObject.SetActive(false);

        //好友列表的改变 添加了好友时（申请被移除）
        ApplynewList.SetItems(moduleFriend.Apply_playerList);
        FrinednewList.SetItems(moduleFriend.FriendList);
        RecentlynewList.SetItems(moduleChat.Late_ListAllInfo);

        FriendDetailInfo friendDetailAdd = Character_details.GetComponentDefault<FriendDetailInfo>();
        if (friendDetailAdd.PlayerId == Agreefriend_Play.roleId)
        {
            friendDetailAdd.SetButtonShow(false);
        }
    }

    private void RefuseFriendIsOk()//拒绝好友成功
    {
        ApplynewList.SetItems(moduleFriend.Apply_playerList);
        ApplyHint();
        ShowApplyNothing();//判断还有没有申请
    }

    private void ShowApplyNothing()
    {
        if (!friendRequests.isOn) return;
        if (moduleFriend.Apply_playerList.Count > 0) ApplyNothing.gameObject.SetActive(false);
        else ApplyNothing.gameObject.SetActive(true);
    }

    private void ApplyHint()//申请红点
    {
        if (moduleFriend.Apply_playerList.Count <= 0 || moduleFriend.FriendList.Count >= moduleFriend.FriendNumTop)
        {
            Apply_hint.gameObject.SetActive(false);
        }
        else Apply_hint.gameObject.SetActive(true);
    }
    #endregion

    #region 最近列表

    private void LastFriendList(RectTransform rt, PPlayerInfo info)
    {
        if (moduleChat.Late_ListAllInfo.Count == 0) return;
        FriendPrecast latge = rt.gameObject.GetComponentDefault<FriendPrecast>();
        int index = 1;
        if (info.roleId == moduleChat.Late_ListAllInfo[0].roleId) index = 0;
        latge.DelayAddData(info, index, 0);
        SetPointState(rt, info);
        TeamHint(rt, info.roleId);

        Button click = rt.Find("onclick").GetComponentDefault<Button>();
        click.onClick.RemoveAllListeners();
        click.onClick.AddListener(delegate { Onclick(rt, info); });
    }
    private void Late_Btn()//最近按钮
    {
        RecentlynewList.SetItems(moduleChat.Late_ListAllInfo);

        ApplyNothing.gameObject.SetActive(false);
        selectfriend.gameObject.SetActive(false);
        requestslist.gameObject.SetActive(false);
        emoticons.gameObject.SetActive(false);

        if (moduleChat.Late_ListAllInfo.Count > 0)
        {
            change_chat_friend(moduleChat.Late_ListAllInfo[0].roleId);
        }
        else
        {
            Chat_Panel.gameObject.SetActive(false);
            ChatNothing.gameObject.SetActive(true);
            FlistNothing.gameObject.SetActive(true);
        }
    }

    #endregion

    #region 黑名单

    private void SetBlackInfo(RectTransform rt, PPlayerInfo info)
    {
        if (info == null) return;
        FriendPrecast f = rt.GetComponentDefault<FriendPrecast>();
        f.DelayAddData(info, -1, 0);
        TeamHint(rt, info.roleId);
        SetPointState(rt, info, false);
        Button click = rt.Find("onclick").GetComponentDefault<Button>();
        click.onClick.RemoveAllListeners();
        click.onClick.AddListener(delegate { Onclick(rt, info); });

    }
    private void BlackBtn()//黑名单按钮
    {
        BlackList.SetItems(moduleFriend.BlackList);

        ApplyNothing.gameObject.SetActive(false);
        selectfriend.gameObject.SetActive(false);
        requestslist.gameObject.SetActive(false);
        emoticons.gameObject.SetActive(false);

        if (moduleFriend.BlackList.Count > 0)
        {
            change_chat_friend(moduleFriend.BlackList[0].roleId);
        }
        else
        {
            Chat_Panel.gameObject.SetActive(false);
            ChatNothing.gameObject.SetActive(true);
            FlistNothing.gameObject.SetActive(true);
        }
    }

    #endregion

    #region 动画位置

    private void SetFromTo(TweenSize tween, List<PPlayerInfo> info, ScrollView scroll)
    {
        float to = m_listbgTop.sizeDelta.y;
        var num = info.Count;
        if (info.Count > 4) num = 4;
        float listHeight = num * m_template.sizeDelta.y + (num - 1) * scroll.padding;
        if (listHeight < 0) listHeight = 0;

        if (info.Count > 0) to += listHeight;

        RectTransform rt = scroll.gameObject.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(tween.from.x, listHeight);
        tween.to.y = to;
        tween.Play();
    }

    private void DeletListState()
    {
        //当拉黑/取消拉黑 增加好友/减少好友时候 重新计算
        if (friendBtn.isOn) SetFromTo(m_friendSize, moduleFriend.FriendList, m_friendScroll);
        else if (Lately_Btn.isOn) SetFromTo(m_lateSize, moduleChat.Late_ListAllInfo, m_lateScroll);
        else if (m_blackToggle.isOn) SetFromTo(m_blackSize, moduleFriend.BlackList, m_blackScroll);
        moduleFriend.RefreshLateState = false;
    }

    #endregion


    #region Event

    private void _ME(ModuleEvent<Module_Chat> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Chat.EventChatFriendChange:
                ScChatPrivate xc = e.param1 as ScChatPrivate;
                long id_xx = Int64.Parse(e.param2.ToString());
                PPlayerInfo friend_caht = moduleFriend.FriendList.Find(a => a.roleId == (ulong)id_xx);
                if (friend_caht == null) friend_caht = moduleChat.LatePlayerInfo((ulong)id_xx);
                if (friend_caht == null) return;

                if (xc.sendId != modulePlayer.id_) position_clear(false, xc.type, xc.content, other_speak_obj, friend_caht.name, friend_caht.avatar, friend_caht.roleId, friend_caht.gender, friend_caht.headBox, friend_caht.proto, Util.Parse<int>(xc.tag));//如果id与自己id不同
                else position_clear(true, xc.type, xc.content, my_speak_obj, modulePlayer.name_, modulePlayer.avatar, friend_caht.roleId, modulePlayer.gender, modulePlayer.avatarBox, modulePlayer.proto, Util.Parse<int>(xc.tag));//如果id与自己id相同
                break;
            case Module_Chat.EventChatRecFriendMes:
                ScChatPrivate other = e.msg as ScChatPrivate;
                ReciveMes(other);
                if (other.tag == "1" || other.tag == "3") FrinednewList.SetItems(moduleFriend.FriendList);
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Union> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Union.EventUnionPlayerExit:

                if (!Lately_Btn.isOn) return;
                ulong r = Util.Parse<ulong>(e.param1.ToString());
                if (moduleFriend.checkPlayer.roleId == r || moduleChat.Late_ListAllInfo.Count == 0) Late_Btn();
                else RecentlynewList.SetItems(moduleChat.Late_ListAllInfo);

                break;
        }
    }

    private void _ME(ModuleEvent<Module_Friend> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Friend.EventFriendAddApply:
                var result = Util.Parse<int>(e.param1.ToString());
                if (result != 0)
                {
                    m_chatAddBtn.interactable = true;
                    m_detailAdd.SetInteractable(true);
                }
                break;
            case Module_Friend.EventFriendAddBlack://黑名单
                var black = e.param1 as PPlayerInfo;
                var bfriend = (bool)e.param2;
                var blate = (bool)e.param3;
                DeletListState();
                DeleteFriendIsOk(black, bfriend, blate);
                if (m_blackToggle.isOn) BlackList.SetItems(moduleFriend.BlackList);
                if (addFriend.isOn)
                {
                    var playerId = Util.Parse<ulong>(selectok_IDFalse.text);
                    if (black?.roleId == playerId) selectok_add.interactable = false;
                    FriendRecommend();
                }
                if (friendRequests.isOn) RefuseFriendIsOk();
                if (!Lately_Btn.isOn) m_lateHint.gameObject.SetActive(moduleChat.ShowLateHint());

                break;
            case Module_Friend.EventFriendRemoveBlack:
                Character_details.gameObject.SetActive(false);
                Chat_Panel.gameObject.SetActive(false);
                DeletListState();
                if (addFriend.isOn)
                {
                    ulong playerId = Util.Parse<ulong>(selectok_IDFalse.text);
                    var have = moduleFriend.BlackList.Exists(a => a.roleId == playerId);
                    if (!have) selectok_add.interactable = true;
                    FriendRecommend();
                }
                if (m_blackToggle.isOn) BlackList.SetItems(moduleFriend.BlackList);
                break;
            case Module_Friend.EventFriendAllBlack:
                DeletListState();
                BlackList.SetItems(moduleFriend.BlackList);
                break;

            case Module_Friend.EventFriendApplyList:
                ApplynewList.SetItems(moduleFriend.Apply_playerList);
                ApplyHint();//红点
                break;
            case Module_Friend.EventFriendAgree:
                var AgreefriendOk = e.msg as PPlayerInfo;
                DeletListState();
                ArgeeFriendIsOk(AgreefriendOk);
                break;
            case Module_Friend.EventFriendResufed:
                RefuseFriendIsOk();
                break;

            //删除添加好友相关
            case Module_Friend.EventFriendDelete:
                var deltefriendOk = e.param1 as PPlayerInfo;
                bool friend1 = (bool)e.param2;
                bool late1 = (bool)e.param3;
                DeletListState();
                DeleteFriendIsOk(deltefriendOk, friend1, late1);
                if (!Lately_Btn.isOn) m_lateHint.gameObject.SetActive(moduleChat.ShowLateHint());
                break;
            case Module_Friend.EventAddFriendReply:
                DeletListState();
                AddFriendReplayOk();
                break;

            //创建好友列表
            case Module_Friend.EventFriendAllList:
                FriendColor();
                DeletListState();
                FrinednewList.SetItems(moduleFriend.FriendList);//好友
                RecentlynewList.SetItems(moduleChat.Late_ListAllInfo);//最近
                break;
            //上下线
            case Module_Friend.EventFreindISonlie:
                FrinednewList.UpdateItems();//全部刷新
                break;

            //搜索 以及随机好友
            case Module_Friend.EventFriendSearch:
                var isselcet = Util.Parse<int>(e.param1.ToString());
                var isselcet_player = e.param2 as PPlayerInfo;
                FriendSearchReplay(isselcet, isselcet_player);
                break;
            case Module_Friend.EventFriendRecommend:
                FriendRecommend();
                break;
            //玩家详情
            case Module_Friend.EventFriendDetails:
                var friend_info = e.msg as PPlayerInfo;
                Character_details.gameObject.SetActive(true);
                FriendDetailInfo frienDetail = Character_details.GetComponentDefault<FriendDetailInfo>();
                frienDetail.IsfriendDetails(friend_info, false, WordMySend);
                break;
            case Module_Friend.EventFriendDeletedReplay:
                var deleted_replay = e.param1 as PPlayerInfo;
                bool friend2 = (bool)e.param2;
                bool late2 = (bool)e.param3;
                DeletListState();
                delete_friend_replay(deleted_replay, friend2, late2);
                if (!Lately_Btn.isOn) m_lateHint.gameObject.SetActive(moduleChat.ShowLateHint());
                break;
            case Module_Friend.EventFreindHintistrue:
                ApplyHint();
                break;

            //友情点
            case Module_Friend.EventFriendSendPoint:
                var send = Util.Parse<ulong>(e.param1.ToString());
                UpDatePoint(send, true);
                break;
            case Module_Friend.EventFriendGetPoint:
                var get = Util.Parse<ulong>(e.param1.ToString());
                UpDatePoint(get, false);
                break;
            case Module_Friend.EventFriendRecPoint:
                var receive = Util.Parse<ulong>(e.param1.ToString());
                UpDatePoint(receive, false);
                break;
            case Module_Friend.EventFriendResetPoint:
                FrinednewList.UpdateItems();
                RecentlynewList.UpdateItems();
                break;
        }
    }

    #endregion

    protected override void OnReturn()//退出的时候的一些操作
    {
        moduleHome.UpdateIconState(HomeIcons.Friend, false);
        moduleFriend.windowisopen = false;
        if (moduleFriend.FriendList.Count > 0) Chat_id.text = moduleFriend.FriendList[0].roleId.ToString();
        mes_add.text = null;
        if (m_ShowSort) moduleFriend.FriendList = moduleFriend.SortWay();
        m_ShowSort = false;
        Hide(true);
    }
}

