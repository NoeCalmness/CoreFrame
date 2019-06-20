/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Global window script
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-04-07
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections;
using Object = UnityEngine.Object;
using cn.sharesdk.unity3d;

public class Window_Global : Window
{
    #region custom class
    public class PropAttributeTipPanel : CustomSecondPanel
    {
        public PropAttributeTipPanel(Transform trans) : base(trans) { }

        private Text m_tipText;
        private Transform m_wupin;
        private DataSource<ItemAttachAttr> m_dataSource;
        private ScrollView m_scroll;

        public override void InitComponent()
        {
            base.InitComponent();
            m_tipText = transform.GetComponent<Text>("text_back/detail_viewport/detail_content");
            m_wupin = transform.Find("middle");
            m_scroll = transform.GetComponent<ScrollView>("middle/scrollView");
            m_dataSource = new DataSource<ItemAttachAttr>(new List<ItemAttachAttr>(), m_scroll, OnItemRefresh);
        }

        public void RefreshAttributeProp(PropItemInfo info)
        {
            if (info == null) return;

            var TextInfo = ConfigManager.Get<ConfigText>(info.desc);
            Util.SetItemInfo(m_wupin, info);
            m_tipText.text = TextInfo ? TextInfo.text[0].Replace("\\n", "\n") : string.Empty;
            m_dataSource?.SetItems(info.attributes);
            m_dataSource?.UpdateItems();
        }

        private void OnItemRefresh(RectTransform rt, ItemAttachAttr attr)
        {
            if (attr == null) return;

            var t = rt.GetComponent<Text>("type");
            var v = rt.GetComponent<Text>("value");

            Util.SetText(t, ConfigText.GetDefalutString(TextForMatType.AllAttributeText, attr.id));
            string s = GeneralConfigInfo.IsPercentAttribute(attr.id) ? attr.value.ToString("p") : attr.value.ToString();
            Util.SetText(v, 32, 3, s);
        }
    }
    #endregion

    #region Global locker

    private Image m_background;
    private RectTransform m_lockerIcon;
    private GameObject m_locker;
    private Text m_lockerInfo;

    private Color m_backColor;
    private Color m_hideColor = new Color(0, 0, 0, 0);

    #endregion

    #region pve locker

    private Image m_pveLockerBg;
    private RectTransform m_pveLockerIcon;
    private GameObject m_pveLocker;
    private Text m_pveLockerText;
    private Tween m_pveLockerTween;
    private bool m_showPveLocker;
    #endregion

    #region Global window layer

    private GameObject m_globalLayer, m_rightBar;
    private TweenAlpha m_tweenGlobalAlpha;
    private TweenPosition m_tweenGlobalPosition;

    // Avatar
    private Button m_avatar, m_buff;
    private Image m_expBar;
    private Text m_txtLevel, m_txtName;
    private Button m_monthCard, m_seasonCard;

    // Dating Npc Avatar
    private Transform m_tfDatingNpcAvatar;
    private Image m_npcAvatar, m_npcSlider;
    private Text m_txtNpcName, m_txtNpcExpNumber, m_txtNpcGoodFeelingLv, m_txtNpcGoodFeelingName, m_txtNpcAddGoodFeelingVal;

    // Right bar
    private Button m_btnAddVitality, m_btnAddGold, m_btnAddGem, m_btnAddMood, m_btnAddBodyPower, m_btnFriend, m_btnMail;
    private Text m_txtVitality, m_txtGold, m_txtGem, m_txtMood, m_txtBodyPower;
    private Transform m_tfNormalBar, m_tfDatingBar;

    private List<MarkableIcon> m_globalIcons = new List<MarkableIcon>();

    private Button m_return;
    private Canvas m_globalCanvas;
    private int m_defaultGlobalIndex = 1;

    //friend
    private Button m_friendBtn;
    #endregion

    #region Popup message box

    private GameObject m_messageBox;
    private Action<bool> m_messageBoxCallback;

    #endregion

    #region Mask

    private Image m_screenMask;
    private Image m_storyMask;
    private int m_storyMaskDelayEventId;

    #endregion

    #region 浮动提示 yaoqing

    private GameObject m_messBoxPlane;
    /*框体*/
    private GameObject m_noticeK;
    private TweenScale m_animation;
    private GameObject m_weimanzu;
    private Text mes_txt;
    private RectTransform mes_pos;

    private Image m_tishikuangBack;
    private RectTransform award_pos;

    private Image m_paomadeng;
    private RectTransform notice_pos;
    private Transform m_Mask;

    private int a;

    List<GameObject> clone_award = new List<GameObject>();

    private GameObject mesbox;
    private RectTransform mes_panel;//接收到邀请
    private Text mes_const;
    private Text Invate_Id;
    private Button const_ok;
    private Button const_no;

    private RectTransform return_panel;
    private Text mes_return;
    private Text return_btn;
    private Button return_ok;
    private Button m_closeInvate;

    private float time_finish;
    private float time_sart = 0;
    private bool is_be_invate = false;
    #endregion

    #region global_tip
    private GameObject m_globalTip;
    private RectTransform detailTipPanel;
    private RectTransform exchangePanel;

    //没有按钮的tip
    private RectTransform m_noUseTipPanel;

    //有按钮的tip
    private RectTransform m_usePlane;//使用
    private RectTransform m_compPlane;//合成
    private RectTransform avalible_info;
    private Button avalible_btn;
    private Text avalibleBtn_text;
    private Button compose_btn;
    private Text composeBtn_text;
    private Button m_getNumReduce;//减少
    private Button m_getNumAdd;//添加
    private Button m_getNumMax;//最大
    private Text m_getNum;

    //兑换的tip
    private Text tittle_tip;
    private Transform energy_contentPanel;
    private Text content_tip;
    private Text remainEnergy;
    private Text other_contentTip;
    private Text current_Desc;
    private Transform costItem;
    private Text costNumber;
    private Transform getItem;
    private Text getNumber;
    private Button exChangeBtn;
    private Text exChangeBtn_text;

    #region 合成武器
    private Text fenzi_num;
    private Text fenmu_num;
    private Text WeaponName;
    private Text SureComText;
    private Text m_tipTtile;
    private Text m_tipContent;
    private Button m_comSureBtn;//确认合成按钮
    private Button m_comAddBtn;
    private Button m_comReduceBtn;
    private Button m_comMaxBtn;
    private Text m_comNumber;
    private RectTransform debrisleft;
    private RectTransform weponrignt;

    #endregion

    #endregion

    #region drop tip
    private GameObject m_dropPlane;
    private GameObject m_canGet;
    private GameObject m_noGet;
    private ScrollView m_dropView;

    private DataSource<DropInfo> m_allDropList;
    private GameObject m_dropObj;//掉落碎片
    #endregion

    #region fight 战斗力
    private GameObject m_fightPlane;
    private Text m_fightValue;
    private GameObject m_fightUp;
    private GameObject m_fightDown;
    private Transform m_fightUpParent;
    private Transform m_fightDownParent;

    private Sequence m_seqience;
    private bool m_fightOpen;
    private float m_fightTime;
    private float m_fightDelay;
    #endregion

    #region globalShare
    private GameObject m_globalShare;
    private Button m_wechatMoment;
    private Button m_wechat;
    private Button m_qqZone;
    private Button m_qq;
    private Button m_sina;
    private Coroutine m_showUIForSeconds = null;
    #endregion

    #region Initialize

    protected override void OnOpen()
    {
        markedGlobal = true;
        isFullScreen = false;

        #region drop 

        m_dropPlane = AssetBundles.AssetManager.GetLoadedGlobalAsset<GameObject>("global_diaoluoxinxi");
        m_dropPlane.gameObject.SetActive(false);
        Util.AddChild(transform, m_dropPlane.transform);
        m_canGet             = GetComponent<RectTransform>("global_diaoluoxinxi/havesources_Panel").gameObject;
        m_noGet              = GetComponent<RectTransform>("global_diaoluoxinxi/nosources_Txt").gameObject;
        m_dropObj            = GetComponent<RectTransform>("global_diaoluoxinxi/middle").gameObject;
        m_dropView           = GetComponent<ScrollView>("global_diaoluoxinxi/havesources_Panel");
        m_allDropList = new DataSource<DropInfo>(moduleGlobal.m_dropList, m_dropView, SetDropInfo, DropOnClick);
        #endregion

        #region Global window layer

        m_globalLayer = AssetBundles.AssetManager.GetLoadedGlobalAsset<GameObject>("global_layer");
        
        m_rightBar = m_globalLayer.transform.Find("bar").gameObject;

        m_globalCanvas = m_globalLayer.GetComponent<Canvas>();
        m_defaultGlobalIndex = m_globalCanvas.sortingOrder;

        Util.AddChild(transform, m_globalLayer.transform);
        m_globalLayer.rectTransform().anchoredPosition = new Vector2(0, 0);

        m_tweenGlobalAlpha = GetComponent<TweenAlpha>("global_layer");
        m_tweenGlobalPosition = GetComponent<TweenPosition>("global_layer");

        m_tfNormalBar = GetComponent<RectTransform>("global_layer/bar/normal");
        m_tfDatingBar = GetComponent<RectTransform>("global_layer/bar/dating");

        m_btnAddVitality = GetComponent<Button>("global_layer/bar/normal/fatigue/add"); m_btnAddVitality.onClick.AddListener(() => { UpdateExchangeTipPanel(TipType.BuyEnergencyTip); });
        m_btnAddGold = GetComponent<Button>("global_layer/bar/normal/gold/add"); m_btnAddGold.onClick.AddListener(() => { UpdateExchangeTipPanel(TipType.BuyGoldTip); });
        m_btnAddGem = GetComponent<Button>("global_layer/bar/normal/diamond/add"); m_btnAddGem.onClick.AddListener(() => ShowAsync<Window_Charge>());
        m_btnAddMood = GetComponent<Button>("global_layer/bar/dating/mood/add"); m_btnAddMood.onClick.AddListener(() => { UpdateDatingGiftPanel(EnumDatingGiftType.Mood); });
        m_btnAddBodyPower = GetComponent<Button>("global_layer/bar/dating/energy/add"); m_btnAddBodyPower.onClick.AddListener(() => { UpdateDatingGiftPanel(EnumDatingGiftType.BodyPower); });

        m_txtVitality = GetComponent<Text>("global_layer/bar/normal/fatigue/text");
        m_txtGold = GetComponent<Text>("global_layer/bar/normal/gold/text");
        m_txtGem = GetComponent<Text>("global_layer/bar/normal/diamond/text");
        m_txtMood = GetComponent<Text>("global_layer/bar/dating/mood/text");
        m_txtBodyPower = GetComponent<Text>("global_layer/bar/dating/energy/text");

        m_avatar = GetComponentDefault<Button>("global_layer/avatar"); m_avatar.onClick.AddListener(() => { ShowAsync<Window_System>(); });
        m_buff = GetComponent<Button>("global_layer/avatar/buff"); m_buff.onClick.AddListener(() => modulePlayer.ClickExpBtn());
        m_expBar = GetComponent<Image>("global_layer/avatar/exp/expbar/bar");
        m_txtLevel = GetComponent<Text>("global_layer/avatar/lvl");
        m_txtName = GetComponent<Text>("global_layer/avatar/name");
        m_monthCard = GetComponent<Button>("global_layer/avatar/card1");
        m_seasonCard = GetComponent<Button>("global_layer/avatar/card2");

        // Npc Dating Avatar
        m_tfDatingNpcAvatar = GetComponent<RectTransform>("global_layer/npcAvatar");
        m_npcAvatar = GetComponent<Image>("global_layer/npcAvatar/avatar");
        m_npcSlider = GetComponent<Image>("global_layer/npcAvatar/slider/topSlider");
        m_txtNpcName = GetComponent<Text>("global_layer/npcAvatar/npcName");
        m_txtNpcExpNumber = GetComponent<Text>("global_layer/npcAvatar/slider/expNumber");
        m_txtNpcGoodFeelingLv = GetComponent<Text>("global_layer/npcAvatar/slider/level/Text");
        m_txtNpcGoodFeelingName = GetComponent<Text>("global_layer/npcAvatar/slider/text");
        m_txtNpcAddGoodFeelingVal = GetComponent<Text>("global_layer/npcAvatar/slider/expNumber_preview");

        m_monthCard?.onClick.AddListener(() => UpdateMonthCard(1011, Module_Charge.CalcCardDays(moduleCharge.MonthEndTime)));
        m_seasonCard?.onClick.AddListener(() => UpdateMonthCard(1012, Module_Charge.CalcCardDays(moduleCharge.SeasonEndTime)));

        m_return = GetComponent<Button>("global_layer/return"); m_return.onClick.AddListener(() => { moduleGlobal.OnGlobalReturnButton(); });

        m_btnMail = GetComponent<Button>("global_layer/bar/mailbox");
        m_btnFriend = GetComponent<Button>("global_layer/bar/friend");

        m_globalIcons.Add(MarkableIcon.Create(2, m_btnMail, () => ShowAsync<Window_Mailbox>()));
        m_globalIcons.Add(MarkableIcon.Create(6, m_btnFriend, () => moduleAnnouncement.OpenWindow((int)HomeIcons.Friend)));

        m_globalLayer.SetActive(false);

        #endregion

        #region Global notice info

        var globalNotice = AssetBundles.AssetManager.GetLoadedGlobalAsset<GameObject>("global_notice");
        Util.AddChild(transform, globalNotice.transform);

        m_globalNotice = globalNotice.GetComponent<TweenAlpha>();
        m_globalNoticeText = m_globalNotice.transform.Find("message").GetComponent<Text>();

        m_globalNotice.gameObject.SetActive(false);

        #endregion

        #region Global message box -- Need remove

        mesbox = AssetBundles.AssetManager.GetLoadedGlobalAsset<GameObject>("global_mesbox");
        mesbox.name = "mesbox";

        Util.AddChild(transform, mesbox.transform);

        mesbox.gameObject.SetActive(false);

        mes_panel = GetComponent<RectTransform>("mesbox/mesconst");
        mes_const = GetComponent<Text>("mesbox/mesconst/Text");
        Invate_Id = GetComponent<Text>("mesbox/mesconst/invateID");
        const_ok = GetComponent<Button>("mesbox/mesconst/ok_btn");
        const_no = GetComponent<Button>("mesbox/mesconst/no_btn");

        return_panel = GetComponent<RectTransform>("mesbox/mesreturn");
        mes_return = GetComponent<Text>("mesbox/mesreturn/Text");
        return_btn = GetComponent<Text>("mesbox/mesreturn/ok_btn/Image");
        return_ok = GetComponent<Button>("mesbox/mesreturn/ok_btn");
        m_closeInvate = GetComponent<Button>("mesbox/background/tip_prop/top/quit_btn");

        const_ok.onClick.AddListener(delegate { overtime(0); });
        const_no.onClick.AddListener(delegate { overtime(1); });
        m_closeInvate.onClick.AddListener(delegate { overtime(1); });

        return_ok.onClick.AddListener(delegate
        {
            mesbox.gameObject.SetActive(false);
            mes_panel.gameObject.SetActive(true);
            return_panel.gameObject.SetActive(false);
        });

        #endregion

        #region Global decompose tips -- Need remove

        var decomposeInfo = AssetBundles.AssetManager.GetLoadedGlobalAsset<GameObject>("global_decompose_info");
        Util.AddChild(transform, decomposeInfo.transform);

        m_decomposeInfo = decomposeInfo.GetComponent<TweenAlpha>();

        GetComponent<Button>("global_decompose_info/Image/sure").onClick.AddListener(() => HideDecomposeInfo(0));
        GetComponent<Button>("global_decompose_info/top/button").onClick.AddListener(() => HideDecomposeInfo(1));

        #endregion

        #region 战斗力
        m_fightOpen = true;
        m_fightPlane = AssetBundles.AssetManager.GetLoadedGlobalAsset<GameObject>("global_combateffective");
        Util.AddChild(transform, m_fightPlane.transform);
        m_fightValue = GetComponent<Text>("global_combateffective/value");
        m_fightUp = GetComponent<Text>("global_combateffective/upnode/value_up").gameObject;
        m_fightDown = GetComponent<Text>("global_combateffective/downnode/value_down").gameObject;
        m_fightUpParent = GetComponent<RectTransform>("global_combateffective/upnode");
        m_fightDownParent = GetComponent<RectTransform>("global_combateffective/downnode");
        m_fightPlane?.gameObject.SetActive(false);
        #endregion

        #region global_tip

        m_globalTip = AssetBundles.AssetManager.GetLoadedGlobalAsset<GameObject>("global_tip");

        Util.AddChild(transform, m_globalTip.transform);
        m_globalTip.rectTransform().sizeDelta = Vector2.zero;
        m_globalTip.rectTransform().anchoredPosition = Vector3.zero;

        detailTipPanel = GetComponent<RectTransform>("global_tip/tip");
        detailTipPanel.gameObject.SetActive(false);
        exchangePanel = GetComponent<RectTransform>("global_tip/charge");
        exchangePanel.gameObject.SetActive(false);

        m_noUseTipPanel = GetComponent<RectTransform>("global_tip/tip/tip_nouse");


        m_usePlane = GetComponent<RectTransform>("global_tip/tip/tip_canuse");
        m_compPlane = GetComponent<RectTransform>("global_tip/tip/tip_comp");
        avalible_info = GetComponent<RectTransform>("global_tip/tip/tip_canuse/bottom/use");
        avalible_btn = GetComponent<Button>("global_tip/tip/tip_canuse/bottom/use/useBtn");
        avalibleBtn_text = GetComponent<Text>("global_tip/tip/tip_canuse/bottom/use/useBtn/Text");
        compose_btn = GetComponent<Button>("global_tip/tip/tip_canuse/bottom/compBtn");
        composeBtn_text = GetComponent<Text>("global_tip/tip/tip_canuse/bottom/compBtn/Text");
        m_getNumReduce = GetComponent<Button>("global_tip/tip/tip_canuse/bottom/use/minus");
        m_getNumAdd = GetComponent<Button>("global_tip/tip/tip_canuse/bottom/use/add");
        m_getNumMax = GetComponent<Button>("global_tip/tip/tip_canuse/bottom/use/max");
        m_getNum = GetComponent<Text>("global_tip/tip/tip_canuse/bottom/use/back/number");

        tittle_tip = GetComponent<Text>("global_tip/charge/exChangePanel/equipinfo");
        energy_contentPanel = GetComponent<Transform>("global_tip/charge/exChangePanel/energyTip");
        energy_contentPanel.gameObject.SetActive(false);
        content_tip = GetComponent<Text>("global_tip/charge/exChangePanel/energyTip/content_tip");
        remainEnergy = GetComponent<Text>("global_tip/charge/exChangePanel/energyTip/content");
        other_contentTip = GetComponent<Text>("global_tip/charge/exChangePanel/otherTip");
        other_contentTip.gameObject.SetActive(false);
        current_Desc = GetComponent<Text>("global_tip/charge/exChangePanel/current_text");
        costItem = GetComponent<Transform>("global_tip/charge/exChangePanel/costItem");
        costNumber = GetComponent<Text>("global_tip/charge/exChangePanel/costItem/numberdi/count");
        getItem = GetComponent<Transform>("global_tip/charge/exChangePanel/getItem");
        getNumber = GetComponent<Text>("global_tip/charge/exChangePanel/getItem/numberdi/count");
        exChangeBtn = GetComponent<Button>("global_tip/charge/exChangePanel/yes_button");
        exChangeBtn_text = GetComponent<Text>("global_tip/charge/exChangePanel/yes_button/duihuan_text");

        m_noUseTipPanel.gameObject.SetActive(false);
        m_globalTip.SetActive(false);

        EventManager.AddEventListener(Events.SCENE_LOAD_START, OnSceneLoadStart);

        #region 合成武器
        debrisleft = GetComponent<RectTransform>("global_tip/tip/tip_comp/floor/Image/debris");
        weponrignt = GetComponent<RectTransform>("global_tip/tip/tip_comp/floor/Image/weapon");
        fenzi_num = GetComponent<Text>("global_tip/tip/tip_comp/floor/Image/debris/number/fenz");
        fenmu_num = GetComponent<Text>("global_tip/tip/tip_comp/floor/Image/debris/number/fenm");
        SureComText = GetComponent<Text>("global_tip/tip/tip_comp/floor/sure_btn/Text");
        m_comSureBtn = GetComponent<Button>("global_tip/tip/tip_comp/bottom/compBtn");
        WeaponName = GetComponent<Text>("global_tip/tip/tip_comp/floor/Image/weapon/name");

        m_tipTtile = GetComponent<Text>("global_tip/tip/tip_comp/tip_nouse/top/equipinfo");
        m_tipContent = GetComponent<Text>("global_tip/tip/tip_comp/content_tip");

        m_comReduceBtn = GetComponent<Button>("global_tip/tip/tip_comp/bottom/use/minus");
        m_comAddBtn = GetComponent<Button>("global_tip/tip/tip_comp/bottom/use/add");
        m_comMaxBtn = GetComponent<Button>("global_tip/tip/tip_comp/bottom/use/max");
        m_comNumber = GetComponent<Text>("global_tip/tip/tip_comp/bottom/use/back/number");

        m_comSureBtn.onClick.AddListener(delegate
        {
            Compound compinfo = ConfigManager.Get<Compound>(moduleEquip.Compose_ID);//合成/分解信息
            var mulit = Util.Parse<int>(m_comNumber.text);
            int num = moduleEquip.GetPropCount(compinfo.sourceTypeId);
            if (num >= compinfo.sourceNum * mulit) moduleEquip.SendComposeAnyOne(mulit);
            else ShowMessage(ConfigText.GetDefalutString(204, 46));
        });
        #endregion             
        #endregion

        #region Global message

        var message = AssetBundles.AssetManager.GetLoadedGlobalAsset<GameObject>("global_message");
        message.name = "message";
        message.gameObject.SetActive(true);

        Util.AddChild(transform, message.transform);
        m_messBoxPlane = GetComponent<RectTransform>("message").gameObject;
        mes_pos = GetComponent<RectTransform>("message/1");
        award_pos = GetComponent<RectTransform>("message/2");
        notice_pos = GetComponent<RectTransform>("message/3");
        m_animation = GetComponent<TweenScale>("message/1/Text");
        m_weimanzu = GetComponent<RectTransform>("message/1/Text").gameObject;//文本背景
        mes_txt = GetComponent<Text>("message/1/Text");
        m_tishikuangBack = GetComponent<Image>("message/2/tishikuangti");//背景

        //克隆xia面四个
        m_paomadeng = GetComponent<Image>("message/3/paomadengkuang");//公告背景
        m_Mask = GetComponent<RectTransform>("message/3/paomadengkuang/mask");
        m_noticeK = GetComponent<Text>("message/3/paomadengkuang/mask/Text").gameObject;
        mes_pos.localPosition = new Vector3(0, 0, 0);

        m_noticeK.gameObject.SetActive(false);
        mes_pos.gameObject.SetActive(true);
        award_pos.gameObject.SetActive(true);
        notice_pos.gameObject.SetActive(true);

        m_tishikuangBack.gameObject.SetActive(false);
        m_paomadeng.gameObject.SetActive(false);
        m_weimanzu.gameObject.SetActive(false);

        //缓存池
        for (int i = 0; i < 10; i++)
        {
            GameObject obj = Object.Instantiate(m_tishikuangBack.gameObject);
            obj.transform.SetParent(award_pos, false);//位置会移动
            obj.transform.localPosition = new Vector3(0, 0, 0);
            clone_award.Add(obj);
        }

        #endregion

        #region Fade inout

        var maskNode = transform.AddUINodeStrech("screenMask");
        m_screenMask = maskNode.GetComponentDefault<Image>();
        m_screenMask.color = Color.black.SetAlpha(0);
        m_screenMask.gameObject.SetActive(false);

        #endregion

        #region story mask

        maskNode = transform.AddUINodeStrech("storyMask");
        m_storyMask = maskNode.GetComponentDefault<Image>();
        m_storyMask.color = Color.black.SetAlpha(0);
        m_storyMask.gameObject.SetActive(false);

        UIManager.instance.AddEventListener(Events.UI_WINDOW_OPEN_ERROR, CloseStoryMask);

        #endregion

        #region Global locker

        m_locker = AssetBundles.AssetManager.GetLoadedGlobalAsset<GameObject>("global_locker");
        m_locker.name = "locker";

        Util.AddChild(transform, m_locker.transform);

        m_background = GetComponent<Image>("locker/background");
        m_lockerIcon = GetComponent<RectTransform>("locker/info/mark");
        m_lockerInfo = GetComponent<Text>("locker/info");
        m_backColor = m_background.color;

        m_locker.SetActive(false);

        #endregion

        #region pve locker
        m_pveLocker = AssetBundles.AssetManager.GetLoadedGlobalAsset<GameObject>("global_locker");
        m_pveLocker.name = "pve_locker";
        Util.AddChild(transform, m_pveLocker.transform);
        m_pveLockerBg = GetComponent<Image>("pve_locker/background");
        m_pveLockerIcon = GetComponent<RectTransform>("pve_locker/info/mark");
        m_pveLockerText = GetComponent<Text>("pve_locker/info");
        m_pveLocker.SetActive(false);
        #endregion

        #region Global message box

        m_messageBox = AssetBundles.AssetManager.GetLoadedGlobalAsset<GameObject>("global_message_box");
        m_messageBox.name = "messageBox";

        Util.AddChild(transform, m_messageBox.transform);

        m_messageBox.SetActive(false);

        GetComponent<Button>("messageBox/panel/btnOK").onClick.AddListener(() => OnMessageBoxCallback(true));

        #endregion

        #region Debug

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        var btnDebug = Object.Instantiate(Resources.Load<GameObject>("ui_button")).rectTransform();
        btnDebug.name = "btnDebug";
        btnDebug.GetComponent<Text>("label").text = "Debug";
        Util.AddChild(transform, btnDebug.transform);

        btnDebug.pivot = btnDebug.anchorMax = btnDebug.anchorMin = new Vector2(1, 0.5f);
        btnDebug.sizeDelta = new Vector2(90, 50);
        btnDebug.anchoredPosition = new Vector2(0, 110);

        btnDebug.GetComponent<Button>().onClick.AddListener(() => { ShowAsync<Window_Gm>(); });
        btnDebug.SafeSetActive(!Root.simulateReleaseMode);

        EventManager.AddEventListener("EditorSimulateReleaseMode", () => btnDebug.SafeSetActive(!Root.simulateReleaseMode));
#endif

        #endregion

        #region Global Share
        m_globalShare = AssetBundles.AssetManager.GetLoadedGlobalAsset<GameObject>("global_share");
        m_globalShare.name = "shareTool";
        Util.AddChild(transform, m_globalShare.transform);
        m_globalShare.SetActive(false);

        m_wechat = GetComponent<Button>("shareTool/top_middle/weixin");
        m_wechat.onClick.AddListener(delegate
        {
            ShareToFriends(PlatformType.WeChat);
        });
        m_wechatMoment = GetComponent<Button>("shareTool/top_middle/circle");
        m_wechatMoment.onClick.AddListener(delegate
        {
            ShareToFriends(PlatformType.WeChatMoments);
        });
        m_qq = GetComponent<Button>("shareTool/top_middle/qq");
        m_qq.onClick.AddListener(delegate
        {
            ShareToFriends(PlatformType.QQ);
        });
        m_qqZone = GetComponent<Button>("shareTool/top_middle/qqzone");
        m_qqZone.onClick.AddListener(delegate
        {
            ShareToFriends(PlatformType.QZone);
        });
        m_sina = GetComponent<Button>("shareTool/top_middle/weibo");
        m_sina.onClick.AddListener(delegate
        {
            ShareToFriends(PlatformType.SinaWeibo);
        });
        #endregion

        IniteTextCompent();
    }

    private void InitializeGlobalLayer()
    {
        UpdateTextLevel();
        UpdateTextName();
        UpdateTextVitality();
        UpdateTextGold();
        UpdateTextGem();
        UpdateTextMood();
        UpdateTextBodyPower();

        UpdateExpBar();

        UpdateCard();
    }

    private void IniteTextCompent()
    {
        Util.SetText(GetComponent<Text>("global_tip/tip/tip_comp/bottom/use/Text"), 204,43);
        Util.SetText(GetComponent<Text>("global_tip/tip/tip_comp/bottom/compBtn/Text"), 204,44);
        Util.SetText(GetComponent<Text>("global_tip/tip/tip_comp/tip_prop/top/equipinfo"), 204,45);
        Util.SetText(GetComponent<Text>("global_combateffective/type"), 204, 36);

        Util.SetText(GetComponent<Text>("global_tip/tip/tip_nouse/get_Btn/get_Txt"), 204, 31);
        Util.SetText(GetComponent<Text>("global_tip/tip/tip_canuse/get_Btn/get_Txt"), 204, 31);
        Util.SetText(GetComponent<Text>("global_tip/tip/tip_canuse/bottom/use/Text"), 204, 32);

        Util.SetText(GetComponent<Text>("global_diaoluoxinxi/top/equipinfo"), 224, 15);
        Util.SetText(GetComponent<Text>("global_diaoluoxinxi/Tips"), 224, 16);
        Util.SetText(GetComponent<Text>("global_diaoluoxinxi/nosources_Txt"), 224, 46);
        Util.SetText(GetComponent<Text>("global_diaoluoxinxi/PetFoodEntry/0/type"), 234, 10);
        Util.SetText(GetComponent<Text>("global_diaoluoxinxi/PetFoodEntry/0/name"), 234, 11);

        Util.SetText(GetComponent<Text>("messageBox/panel/btnOK/label"), 9, 0);
        Util.SetText(GetComponent<Text>("mesbox/mesreturn/ok_btn/Image"), 200, 4);
        Util.SetText(GetComponent<Text>("mesbox/mesreturn/Text"), 200, 14);
        Util.SetText(GetComponent<Text>("mesbox/mesconst/no_btn/Image"), 200, 5);
        Util.SetText(GetComponent<Text>("mesbox/mesconst/ok_btn/Image"), 200, 4);
        Util.SetText(GetComponent<Text>("global_tip/tip/tip_canuse/top/equipinfo"), 204, 0);
        Util.SetText(GetComponent<Text>("global_tip/tip/tip_prop/top/equipinfo"), 204, 0);
        Util.SetText(GetComponent<Text>("global_decompose_info/Image/decompose/Text"), 9500, 0);
        Util.SetText(GetComponent<Text>("global_decompose_info/Image/sure/Text"), 9500, 1);
    }

    #endregion

    #region Share

    IEnumerator ShowShareForSeconds()
    {
        yield return new WaitForSeconds(10);
        if(m_globalShare)
            m_globalShare.SetActive(false);
    }

    void ShareToFriends(PlatformType platformType)
    {
        if (platformType == PlatformType.WeChat || platformType == PlatformType.WeChatMoments)
            SDKManager.ShareImage(platformType, moduleGlobal.shareImagePath, moduleGlobal.shareText, (i, r) => { if (r == 1) moduleGlobal.SendGetShareAward(); }, ContentType.Image);
        else
            SDKManager.ShareImage(platformType, moduleGlobal.shareImagePath, moduleGlobal.shareText, (i, r) => { if (r == 1) moduleGlobal.SendGetShareAward(); });
    }

    #endregion

    #region 被邀请

    public override void OnRenderUpdate()
    {
        if (is_be_invate)
        {
            time_sart += Time.unscaledDeltaTime;
            if (time_sart > 1.0f)
            {
                time_finish--;
                time_sart = 0;
                if (time_finish == 0)
                {
                    is_be_invate = false;
                    enableUpdate = false;

                    mesbox.gameObject.SetActive(false);
                }
            }
        }

        if (m_delayMessage != string.Empty) ShowMessage(m_delayMessage, m_delayDuration);
        m_delayMessage = string.Empty;
    }

    public void overtime(int argee)
    {
        is_be_invate = false;
        enableUpdate = false;
        mesbox.gameObject.SetActive(false);

        ulong id_n = Util.Parse<ulong>(Invate_Id.text.ToString());
        if (id_n == 0) return;
        if (argee == 0)
        {
            moduleGlobal.Argee(id_n);
        }
        else if (argee == 1)
        {
            moduleGlobal.Refuse(id_n);
        }
    }
    void _ME(ModuleEvent<Module_Match> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Match.EventInvationFailed:
                if(e.param1 == null || !(bool)e.param1)
                    Invate_fail();
                break;
            case Module_Match.EventInvationsucced:
                roomkey();
                break;
        }
    }

    void _ME(ModuleEvent<Module_Charge> e)
    {
        if (e.moduleEvent == Module_Charge.NoticeChargeInfoChange)
        {
            UpdateCard();
        }
    }

    private void Invate_fail()
    {
        mesbox.gameObject.SetActive(true);
        mes_panel.gameObject.SetActive(false);
        return_panel.gameObject.SetActive(true);
        Util.SetText(return_btn, ConfigText.GetDefalutString(200, 4));
        Util.SetText(mes_return, ConfigText.GetDefalutString(204, 14));
    }
    private void Be_invation()
    {
        mesbox.gameObject.SetActive(true);
        enableUpdate = true;
        time_finish = 15f;
        is_be_invate = true;
        mes_panel.gameObject.SetActive(true);
        return_panel.gameObject.SetActive(false);
        Invate_Id.text = moduleGlobal.roleinfo.roleId.ToString();
        string consTxt = ConfigText.GetDefalutString(204, 11) + moduleGlobal.roleinfo.roleName + ConfigText.GetDefalutString(204, 12);
        Util.SetText(mes_const, consTxt);
    }

    private void agreeFiled(int result)
    {
        mesbox.gameObject.SetActive(true);
        mes_panel.gameObject.SetActive(false);
        return_panel.gameObject.SetActive(true);
        if (result == 1 || result == 4) Util.SetText(mes_return, ConfigText.GetDefalutString(204, 13));
        else if (result == 6 || result == 5) Util.SetText(mes_return, ConfigText.GetDefalutString(204, 33));
        else if (result == 3)Util.SetText(mes_return, ConfigText.GetDefalutString(204, 15));
        else if (result == 2)Util.SetText(mes_return, ConfigText.GetDefalutString(204, 34));
        else if (result == 7) Util.SetText(mes_return, ConfigText.GetDefalutString(204, 42));
    }

    private void agreesucced()
    {
        //关闭所有的窗口除了pvp
    }

    private void roomkey()
    {
        mes_panel.gameObject.SetActive(true);
        return_panel.gameObject.SetActive(false);
        modulePVP.opType = OpenWhichPvP.FriendPvP;
        if (!current.name.Equals("window_pvp"))
            ShowAsync<Window_PVP>();
    }
    #endregion

    #region Locker and ScreenMask

    private void UpdateLocker(string info, bool show = true, int maskType = 0x07)
    {
        m_locker.SetActive(moduleGlobal.isLocked);

        if (moduleGlobal.isLocked)
        {
            Util.SetText(m_lockerInfo, info);
            m_lockerIcon.anchoredPosition = new Vector2(0, !maskType.BitMask(2) || string.IsNullOrEmpty(info) ? 0 : m_lockerIcon.sizeDelta.y * 0.5f);
        }

        m_background.color = maskType.BitMask(0) ? m_backColor : m_hideColor;

        m_lockerInfo.gameObject.SetActive(show);
        m_background.gameObject.SetActive(moduleGlobal.isLocked);

        if (show)
        {
            m_lockerIcon.gameObject.SetActive(maskType.BitMask(1));
            m_lockerInfo.enabled = maskType.BitMask(2);
        }
    }

    /// <summary>
    /// 显示PVE的黑屏遮罩
    /// </summary>
    /// <param name="info"></param>
    /// <param name="show"> 小于等于0：开始淡出 ; 1:只显示黑屏;  2:显示黑屏、文字信息、旋转图标</param>
    /// <param name="duraction"></param>
    private void UpdatePVELocker(string info, int show, float duraction)
    {
        if (show <= 0 && !m_pveLocker.activeInHierarchy) return;

        m_pveLocker.SetActive(true);
        m_pveLockerIcon.gameObject.SetActive(show > 1);
        m_pveLockerText.gameObject.SetActive(show > 1);
        ChangePVELockerText(info);

        if (m_pveLockerTween != null) m_pveLockerTween.Kill();
        m_showPveLocker = show > 0;

        //set original color to background
        m_pveLockerBg.color = show > 0 ? m_hideColor : Color.black;
        float alpha = show > 0 ? 1 : 0;
        m_pveLockerTween = m_pveLockerBg.DOFade(alpha, duraction).OnComplete(() => {
            if (!m_showPveLocker) m_pveLocker.SetActive(false);
        });
    }

    private void ChangePVELockerText(string info)
    {
        Util.SetText(m_pveLockerText, info);
    }

    private void UpdateFadeMask(float targetAlpha, float duration)
    {
        m_screenMask.gameObject.SetActive(true);
        m_screenMask.DOFade(targetAlpha, duration);
    }

    private void ResetFadeMask()
    {
        m_screenMask.gameObject.SetActive(false);
        m_screenMask.color = new Color(0, 0, 0, 0);
    }

    #endregion

    #region Global window layer

    #region Text update

    public void UpdateTextLevel()
    {
        m_txtLevel.text = Util.GetString(9000, 0, modulePlayer.level);
    }

    public void UpdateTextName()
    {
        m_txtName.text = modulePlayer.name_;
    }

    public void UpdateTextVitality()
    {
        var role = modulePlayer.roleInfo;
        var text = "";// Util.GetString(9001);
        if (string.IsNullOrEmpty(text)) text = "{0}/{1}";

        m_txtVitality.text = Util.Format(text, role.fatigue, modulePlayer.maxFatigue);
    }
    void _ME(ModuleEvent<Module_Forging> e)
    {
        if (e.moduleEvent == Module_Forging.EventGoldChange && moduleForging.InsoulItem != null)
        {
            m_txtGold.text = moduleForging.InsoulGold.ToString();
        }
    }
    public void UpdateTextGold()
    {
        m_txtGold.text = modulePlayer.coinCount.ToString();
    }

    public void UpdateTextGem()
    {
        m_txtGem.text = modulePlayer.gemCount.ToString();
    }

    public void UpdateTextMood()
    {
        m_txtMood.text = moduleNPCDating.curDatingNpc == null ? "0" : moduleNPCDating.curDatingNpc.mood.ToString();
    }

    public void UpdateTextBodyPower()
    {
        m_txtBodyPower.text = moduleNPCDating.curDatingNpc == null ? "0" : moduleNPCDating.curDatingNpc.bodyPower.ToString();
    }
    #endregion

    private void UpdateCard()
    {
        bool disvisible = Level.current && (Level.current is Level_Bordlands || Level.current is Level_Bordlands);
        m_monthCard.SafeSetActive(moduleCharge.HasMonthCard & !disvisible);
        m_seasonCard.SafeSetActive(moduleCharge.HasSeasonCard & !disvisible);
    }

    public void UpdateExpBar()
    {
        m_expBar.fillAmount = modulePlayer.GetExpBarProgress();
    }

    /// <summary>
    /// 获取当前 Global Layer 的显示状态
    /// Mask: 0： layer 状态 1 - 2: 左边状态 3: 右边状态
    /// </summary>
    /// <returns></returns>
    public int GetGlobalLayerShowState()
    {
        var s = 0;
        s |= m_globalLayer.activeSelf ? 0x01 : 0x00;
        s |= (m_avatar.gameObject.activeSelf ? 0x00 : m_return.gameObject.activeSelf ? 0x01 : 0x02) << 1;
        s |= (m_rightBar.activeSelf ? 0x01 : 0x00) << 3;
        s |= (m_globalCanvas.sortingOrder & 0xFFFF) << 4;
        return s;
    }

    /// <summary>
    /// 还原通过 GetGlobalLayerShowState 获取的状态
    /// </summary>
    public void RestoreGlobalLayerState(int state)
    {
        UpdateGlobalLayerShowState(0, state & 0x01);
        UpdateGlobalLayerShowState(1, state >> 1 & 0x03);
        UpdateGlobalLayerShowState(2, state >> 3 & 0x01);
    }

    private void UpdateGlobalLayerShowState(int type, int state)
    {
        if (type == 0) m_globalLayer.SetActive(state == 1);
        else if (type == 1)
        {
            UpdateGlobalLayerShowAvatarState(state);
            m_buff.gameObject.SetActive(state == 0 && modulePlayer.isHaveExpCard);
            m_return.gameObject.SetActive(state == 1);
        }
        else m_rightBar.SetActive(state == 1);

        UpdateCard();
    }

    private void UpdateGlobalLayerShowAvatarState(int state)
    {
        bool disvisible = Level.current && (Level.current is Level_Bordlands);
        m_avatar.gameObject.SetActive(state == 0);
    }

    private void UpdateGlobalLayerIndex(int now = -1)
    {
        m_globalCanvas.sortingOrder = now < 0 ? m_defaultGlobalIndex : now + 1;
    }

    private void UpdateExpBuff(bool haveExp = false, PEffect exp = null)
    {
        m_buff.gameObject.SetActive(haveExp);
        if (haveExp && exp != null)
        {
            ushort itemTypeId = 0;
            switch (exp.id)
            {
                case 1: itemTypeId = 621; break;
                case 2: itemTypeId = 622; break;
                case 3: itemTypeId = 623; break;
                case 4: itemTypeId = 624; break;
                default:
                    break;
            }

            if (modulePlayer.isClickExpBtn)
            {
                SetActivePanel(TipType.NoUseTip);
                ExpTipItem expItem = m_noUseTipPanel.gameObject.GetComponentDefault<ExpTipItem>();
                expItem.RefreshTip(itemTypeId, exp.remainTime, SetjumpInfo);
                modulePlayer.isClickExpBtn = false;
            }
        }
    }

    private void UpdateMonthCard(ushort itemTypeId, int remain)
    {
        SetActivePanel(TipType.NoUseTip);
        ExpTipItem expItem = m_noUseTipPanel.gameObject.GetComponentDefault<ExpTipItem>();
        expItem.RefreshTip(itemTypeId, remain, SetjumpInfo);
    }

    private void UpdateDatingGiftPanel(EnumDatingGiftType type)
    {
        moduleDatingGift.ShowAsyncPanel(type);
    }
   
    #region switch avatar
    /// <summary>
    /// 切换更新头像  约会Npc头像、主角头像
    /// </summary>
    /// <param name="avatarState">avatarState:1 表示当前应该显示主角头像  2:当前应该显示Npc头像</param>
    private void UpdateGlobalLayerAvatar()
    {
        /*m_avatar.SafeSetActive(moduleGlobal.globalAvatarState == 1);

        m_tfNormalBar.SafeSetActive(moduleGlobal.globalAvatarState == 1);
        m_tfDatingBar.SafeSetActive(moduleGlobal.globalAvatarState == 2);

        UpdateDatingNpcAvatarPart();*/
    }

    private void UpdateDatingNpcAvatarPart()
    {
        /*if (moduleNPCDating.curDatingNpc == null) return;
        Module_Npc.NpcMessage npc = moduleNpc.GetTargetNpc((NpcTypeID)moduleNPCDating.curDatingNpc.npcId);
        if (npc == null) return;
        Util.SetText(m_txtNpcName, npc.name);
        AtlasHelper.SetAvatar(m_npcAvatar, npc.icon);

        Util.SetText(m_txtNpcGoodFeelingName, npc.curStageName);
        Util.SetText(m_txtNpcGoodFeelingLv, npc.fetterLv.ToString());
        Util.SetText(m_txtNpcExpNumber, "{0}/{1}", npc.nowFetterValue, npc.toFetterValue);

        if (npc.toFetterValue == 0) m_npcSlider.fillAmount = 0;
        else if (npc.fetterLv >= npc.maxFetterLv) m_npcSlider.fillAmount = 1;
        else m_npcSlider.fillAmount = (float)npc.nowFetterValue / npc.toFetterValue;

        //显示羁绊值功能暂时不做
        //if (npc.fetterLv >= npc.maxFetterLv) m_npcSlider.fillAmount = 1;//已满级
        //else m_npcSlider.fillAmount = npc.fetterProgress;
        //m_txtNpcAddGoodFeelingVal.SafeSetActive(false);
        //m_txtNpcExpNumber.SafeSetActive(npc.fetterLv < npc.maxFetterLv);*/
    }
    #endregion

    #endregion

    #region Global message box

    /// <summary>
    /// Show a global popup message box
    /// </summary>
    public void ShowMessageBox(int title, int titleIndex, int content, int contentIndex, int ok, int okIndex, int cancle, int cancleIndex, int buttonFlag, Action<bool> callback = null)
    {
        var t = Util.GetString(title, titleIndex);
        var c = Util.GetString(content, contentIndex);
        var o = Util.GetString(ok, okIndex);
        var a = Util.GetString(cancle, cancleIndex);

        ShowMessageBox(t, c, o, a, buttonFlag, callback);
    }

    /// <summary>
    /// Show a global popup message box
    /// </summary>
    /// <param name="title">Title</param>
    /// <param name="content">Content text</param>
    /// <param name="buttonFlag">0x00 = default, 0x01 = Close, 0x02 = OK, 0x04 = Cancle</param>
    /// <param name="callback"></param>
    public void ShowMessageBox(string title, string content, string ok, string cancle, int buttonFlag, Action<bool> callback = null)
    {
        Util.SetText(GetComponent<Text>("messageBox/panel/title"), title);
        Util.SetText(GetComponent<Text>("messageBox/panel/content"), content);
        Util.SetText(GetComponent<Text>("messageBox/panel/btnOK/label"), ok);

        m_messageBoxCallback = callback;

        if (m_messageBox.activeSelf)
        {
            var t = m_messageBox.GetComponent<TweenAlpha>();
            if (t) t.PlayForward();
        }
        else m_messageBox.SetActive(true);
    }

    private void OnMessageBoxCallback(bool ok)
    {
        var tmp = m_messageBoxCallback;
        m_messageBoxCallback = null;
        tmp?.Invoke(ok);
    }

    #endregion

    #region Global notice info

    private TweenAlpha m_globalNotice;
    private Text m_globalNoticeText;

    private void ShowGlobalNotice(string message, float stayDuration = 3.0f, float fadeDuration = 1.5f)
    {
        if (string.IsNullOrEmpty(message)) return;
        Util.SetText(m_globalNoticeText, message);

        m_globalNotice.delayStart = stayDuration;
        m_globalNotice.duration = fadeDuration;

        m_globalNotice.PlayForward();
    }

    #endregion

    #region Weapon decompose

    private TweenAlpha m_decomposeInfo;
    private Action onDecomposeInfoHide;

    private void ShowDecomposeInfo(int item, int piece, int count, Action onHide = null)
    {
        var w = ConfigManager.Get<PropItemInfo>(item);
        var p = ConfigManager.Get<PropItemInfo>(piece);

        GameObject ss = GetComponent<Image>("global_decompose_info/Image/item/img").gameObject;
        AtlasHelper.SetItemIcon(ss, p);

        Util.DisableAllChildren(GetComponent<RectTransform>("global_decompose_info/Image/item/quality"), p ? p.quality.ToString() : "1");

        Util.SetText(GetComponent<Text>("global_decompose_info/Image/item/name"), 9501, 0, p ? p.itemName : "null");
        Util.SetText(GetComponent<Text>("global_decompose_info/Image/item/num"), 9501, 1, count);
        Util.SetText(GetComponent<Text>("global_decompose_info/Image/tip/Text"), 9501, 2, w ? w.itemName : "null");

        onDecomposeInfoHide += onHide;

        m_decomposeInfo.PlayForward();
    }

    private void HideDecomposeInfo(int type)
    {
        m_decomposeInfo.gameObject.SetActive(false);
        var h = onDecomposeInfoHide;
        onDecomposeInfoHide = null;
        h?.Invoke();
    }

    #endregion

    #region Module event handlers

    void _ME(ModuleEvent<Module_Player> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Player.EventCurrencyChanged:
            {
                var type = (CurrencySubType)e.param1;
                if (type == CurrencySubType.Gold) UpdateTextGold();
                else if (type == CurrencySubType.Diamond) UpdateTextGem();
                break;
            }
            case Module_Player.EventFatigueChanged: UpdateTextVitality(); break;
            case Module_Player.EventBuySuccessFatigue: m_globalTip.SetActive(false); break;
            case Module_Player.EventBuySuccessCoin: UpdateExchangeTipPanel(TipType.BuyGoldTip); break;
            case Module_Player.EventBuySuccessSweep: UpdateExchangeTipPanel(TipType.MoppingUpTip); break;
            case Module_Player.EventExpChanged: UpdateExpBar(); break;
            case Module_Player.EventLevelChanged: UpdateTextLevel(); UpdateExpBar(); break;
            case Module_Player.EventNameChanged: UpdateTextName(); break;
            case Module_Player.EventHaveExpBuffList: PEffect exp = e.msg as PEffect; UpdateExpBuff(true, exp); break;
            case Module_Player.EventNoExpBuffList: UpdateExpBuff(); break;
            case Module_Player.EventCombatFightChnage:
                var newFight = Util.Parse<int>(e.param1.ToString());
                var oldFight = Util.Parse<int>(e.param2.ToString());
                SetCombatFight(newFight, oldFight);
                break;
            default: break;
        }
    }

    void _ME(ModuleEvent<Module_Global> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Global.EventShowDecomposeInfo: ShowDecomposeInfo((int)e.param1, (int)e.param2, (int)e.param3, (Action)e.param4); break;
            case Module_Global.EventUILockStateChanged: UpdateLocker(e.param2 as string, (bool)e.param1, (int)e.param3); break;
            case Module_Global.EventUIScreenFadeIn:
            case Module_Global.EventUIScreenFadeOut: UpdateFadeMask((float)e.param1, (float)e.param2); break;
            case Module_Global.EventUIScreenFadeInEnd:
            case Module_Global.EventUIScreenFadeOutEnd: if (e.param1 != null && (bool)e.param1) ResetFadeMask(); break;
            case Module_Global.EventPVELoadAssetLockState: UpdatePVELocker((string)e.param1, (int)e.param2, (float)e.param3); break;
            case Module_Global.EventPVEChangeLockText: ChangePVELockerText((string)e.param1); break;
            case Module_Global.EventGlobalLayerShowState: UpdateGlobalLayerShowState((int)e.param1, (int)e.param2); break;
            //case Module_Global.EventShowGlobalFriend: UpdateLayerFriendBtn(); break;
            case Module_Global.EventGlobalLayerInitialize: InitializeGlobalLayer(); break;
            case Module_Global.EventGlobalLayerIndexChanged: UpdateGlobalLayerIndex((int)e.param1); break;
            case Module_Global.EventGlobalLayerTweenAnimation:
            {
                var forward = (bool)e.param1;
                var t = (int)e.param2;
                if (t == 0 || t == 1) m_tweenGlobalAlpha.Play(forward);
                if (t != 1) m_tweenGlobalPosition.Play(forward);
                break;
            }
            case Module_Global.EventGlobalLayerIconAction:
            {
                var iid = (int)e.param1;
                var icon = m_globalIcons.Find(i => i && i.ID == iid);
                if (icon) icon.Invoke();
                break;
            }
            case Module_Global.EventShowMessage: ShowMessage(e.param1.ToString(), (float)e.param2, (bool)e.param3); break;
            case Module_Global.EventShowGlobalNotice: ShowGlobalNotice(e.param1 as string, (float)e.param2, (float)e.param3); break;
            case Module_Global.EventShowMessageBox:
            {
                var messages = e.param1 as string[];
                if (messages == null || messages.Length < 4) Logger.LogError("Window_Global::EventShowMessageBox: Invalid messages.");
                else ShowMessageBox(messages[0], messages[1], messages[2], messages[3], (int)e.param2, (Action<bool>)e.param3);
                break;
            }
            case Module_Global.EventShowawardmes:
            {
                List<PropItemInfo> infos = e.param1 as List<PropItemInfo>;
                List<int> num = e.param2 as List<int>;
                List<int> star = e.param3 as List<int>;

                List<string> name_num = new List<string>(); //文字和num的拼接
                List<Color> color = new List<Color>(); //颜色

                for (int i = 0; i < infos.Count; i++)
                {
                    string namenum = Util.Format("{0}{1}{2}", infos[i].itemName, "×", num[i]);
                    name_num.Add(namenum);

                    Color col = Color.white;
                    color.Add(col);
                }

                showWardmes(name_num, infos, color, star);
                break;
            }
            case Module_Global.EventShowNotice:
                notice_insText(e.param1.ToString());
                break;
            case Module_Global.EventCloseShowNotice:
                ClosePao();
                break;

            case Module_Global.EventBeinvited:
                Be_invation();
                break;
            case Module_Global.EventArgeesucced:
                agreesucced();
                break;
            case Module_Global.EventArgeeFailed:
                var result = Util.Parse<int>(e.param1.ToString());
                agreeFiled(result);
                break;
            case Module_Global.EventUpdateTip:
            {
                ushort tid = (ushort)e.param1;
                bool isShowBtn = (bool)e.param2;
                bool drop = (bool)e.param3;
                int num = Util.Parse<int>(e.param4.ToString());
                UpdateAvalibleTip(tid, isShowBtn, drop, null, num);
                break;
            }
            case Module_Global.EventUpdateItemTip:
            {
                var pi = (PItem2)e.param1;
                bool isShowBtn = (bool)e.param2;
                bool drop = (bool)e.param3;
                UpdateAvalibleTip(pi, isShowBtn, drop);
                break;
            }
            case Module_Global.EventComposeTip:
                SetActivePanel(TipType.CanUseTip);
                m_usePlane.gameObject.SetActive(false);
                m_compPlane.gameObject.SetActive(true);
                var _typeId = Util.Parse<ushort>(e.param1.ToString());
                CompoundProp(_typeId);
                break;
            case Module_Global.EventUpdateTimeTip:
                var item = (PItem)e.param1;
                var have = (bool)e.param2;
                bool dropShow = (bool)e.param3;
                UpdateAvalibleTip(item.itemTypeId, have, dropShow, item);
                break;
            case Module_Global.EventUpdateExchangeTip:
                TipType type = (TipType)e.param1;
                bool isChange = (bool)e.param2;
                UpdateExchangeTipPanel(type, isChange);
                break;
            case Module_Global.EventSkillTip:
                UpdateSkillTip(e.param1 as PetSkill.Skill, (int)e.param2, (EnumPetMood)e.param3);
                break;
            case Module_Global.EventShowDropChase:
                var itemTypeId = Util.Parse<int>(e.param1.ToString());
                SetjumpInfo(itemTypeId);
                break;
            case Module_Global.EventShowShareTool:
            {
                if (m_globalShare)
                {
                    m_globalShare.SetActive(true);
                    if (m_showUIForSeconds != null)
                        StopCoroutine(m_showUIForSeconds);

                    m_showUIForSeconds = StartCoroutine(ShowShareForSeconds());
                }
                        
                break;
            }
            case Module_Global.EventHideShareTool:
            {
                if (m_globalShare)
                    m_globalShare.SetActive(false);
                if (m_showUIForSeconds != null)
                    StopCoroutine(m_showUIForSeconds);
                break;
            }
            default: break;
        }
    }

    void _ME(ModuleEvent<Module_Home> e)
    {
        if (e.moduleEvent == Module_Home.EventIconState)
        {
            var iid = (int)(HomeIcons)e.param1;
            var icon = m_globalIcons.Find(i => i && i.ID == iid);
            MarkableIcon.UpdateIconState(icon, (int)e.param2, (bool)e.param3);
        }
        else if (e.moduleEvent == Module_Home.EventSwitchDatingModel)
        {
            UpdateGlobalLayerAvatar();
        }
    }

    

    void _ME(ModuleEvent<Module_Story> e)
    {
        if (e.moduleEvent == Module_Story.EventStoryMaskEnd) CloseStoryMask();
    }

    void _ME(ModuleEvent<Module_Guide> e)
    {
        if (e.moduleEvent == Module_Guide.EventWillTriggerGuideStory) OpenStoryMask();
        else if (e.moduleEvent == Module_Guide.EventTriggerDifferentGuideStory) CloseStoryMask();
    }

    void _ME(ModuleEvent<Module_Equip> e)
    {
        if (e.moduleEvent == Module_Equip.EvenSynthesisSucced)
        {
            var msg = e.msg as ScRoleItemCompose;
            if (msg != null && msg.items != null && msg.items.Length > 0)
            {
                bool isPet = false;
                for (int i = 0; i < msg.items.Length; i++)
                {
                    var prop = ConfigManager.Get<PropItemInfo>(msg.items[i].itemTypeId);
                    if (prop == null) continue;
                    if (prop.itemType == PropType.Pet)
                    {
                        isPet = true;
                        break;
                    }
                }
                if (isPet) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(204, 17));
            }
            m_globalTip.gameObject.SetActive(false);
        }
    }

    void _ME(ModuleEvent<Module_Skill> e)
    {
        if (e.moduleEvent == Module_Skill.EventBuySkillPointSuccess)
            UpdateExchangeTipPanel(TipType.BuySkillPointTip);
    }

    void _ME(ModuleEvent<Module_Npc> e)
    {
        if (e.moduleEvent == Module_Npc.EventRefreshNpcMood)
        {
            UpdateTextMood();
        }
        else if (e.moduleEvent == Module_Npc.EventRefreshNpcBodyPower)
        {
            UpdateTextBodyPower();
        }
        else if (e.moduleEvent == Module_Npc.NpcPerfusionChangeEvent)
        {
            UpdateDatingNpcAvatarPart();
        }
        else if (e.moduleEvent == Module_Npc.NpcLvUpEvent)
        {
            UpdateDatingNpcAvatarPart();
        }
    }

    void _ME(ModuleEvent<Module_NPCDating> e)
    {
        if (e.moduleEvent == Module_NPCDating.EventChooseDatingNpc)
        {
            UpdateTextBodyPower();
            UpdateTextMood();
        }
    }
    #endregion

    #region 浮动提示    

    #region 点击出现点击消失
    private float m_delayDuration = -1;
    private string m_delayMessage = string.Empty;
    private void ShowMessage(string message, float duration = -1, bool delay = false)
    {
        if (delay)
        {
            m_delayDuration = duration;
            m_delayMessage = message;
            return;
        }

        m_animation.duration = duration < 0 ? GeneralConfigInfo.sglobalMessageDuration : duration;

        m_messBoxPlane.gameObject.SetActive(true);
        if (m_weimanzu.activeInHierarchy)
        {
            m_animation.Play();
        }
        Util.SetText(mes_txt, message);
        m_weimanzu.gameObject.SetActive(true);
    }
    #endregion

    #region 奖励提示
    int length = -1;

    private void showWardmes(List<string> m_award, List<PropItemInfo> m_awardIcon, List<Color> award_Color, List<int> star)
    {
        m_messBoxPlane.gameObject.SetActive(true);
        for (int i = 0; i < m_award.Count; i += 2)
        {
            length++;
        }
        a = 0;

        StartCoroutine(InsAward(m_award, m_awardIcon, award_Color, star));//每0.1秒出现一个

    }

    IEnumerator InsAward(List<string> award, List<PropItemInfo> awardIcon, List<Color> awardColor, List<int> rune_star)
    {
        for (int x = 0; x < award.Count; x += 2)
        {
            int i = a;
            //克隆

            if (x > 0)
            {
                for (int z = 0; z < a; z++)
                {
                    if (clone_award[z].activeInHierarchy)
                    {
                        float mmm = clone_award[z].transform.localPosition.y;
                        clone_award[z].transform.localPosition = new Vector3(0, (mmm + 50f), 0);
                    }

                }
            }

            for (int b = 0; b < clone_award.Count; b++)
            {
                if (!clone_award[b].activeInHierarchy)
                {
                    clone_award[b].gameObject.SetActive(true);
                    AwardTip Awd_tips = clone_award[b].GetComponent<AwardTip>();
                    Awd_tips.lengthAdd(award[i], awardIcon[i], awardColor[i], rune_star[i]);

                    if (i + 1 < award.Count)
                    {
                        Awd_tips.lengthAdd(award[i + 1], awardIcon[i + 1], awardColor[i + 1], rune_star[i]);
                    }
                    Awd_tips.showdata();
                    Awd_tips.Anima(length * 50f);
                    length--;
                    if (length == -1)
                    {
                        StartCoroutine(isend());
                    }
                    break;
                }
            }
            yield return new WaitForSeconds(0.1f);
            a += 2;

        }
    }

    IEnumerator isend()
    {
        yield return new WaitForSeconds(1.0f);
        moduleGlobal.isEnd = true;
        if (moduleGlobal._type.Count > 0)
        {
            moduleGlobal.getAward(moduleGlobal._type.Dequeue(), moduleGlobal._num.Dequeue(), moduleGlobal._star.Dequeue());
        }

    }

    #endregion

    #region 跑马灯
    private void notice_insText(string notice)//用来克隆Text   
    {
        m_messBoxPlane.gameObject.SetActive(true);
        m_paomadeng.gameObject.SetActive(true);

        notice_pos.gameObject.SetActive(true);
        GameObject obj = GameObject.Instantiate(m_noticeK);
        obj.gameObject.SetActive(true);
        obj.transform.SetParent(m_Mask, false);

        Delaypaomadeng delayobj = obj.GetComponentDefault<Delaypaomadeng>();
        delayobj.Delay(notice);

    }
    private void ClosePao()
    {
        m_paomadeng.gameObject.SetActive(false);
        notice_pos.gameObject.SetActive(false);
    }
    #endregion

    #endregion

    #region global_tip

    protected override void OnReturn()
    {
        m_globalTip?.SetActive(false);
    }

    private void SetActivePanel(TipType type)
    {
        m_globalTip.SetActive(true);
        m_dropPlane.gameObject.SetActive(false);
        detailTipPanel.gameObject.SetActive(type == TipType.CanUseTip || type == TipType.NoUseTip || type == TipType.RuneTip);
        exchangePanel.gameObject.SetActive(type == TipType.BuyGoldTip || type == TipType.BuyEnergencyTip || type == TipType.BuySkillPointTip || type == TipType.MoppingUpTip);
        energy_contentPanel.gameObject.SetActive(type == TipType.BuyEnergencyTip || type == TipType.BuySkillPointTip || type == TipType.MoppingUpTip);
        other_contentTip.gameObject.SetActive(type == TipType.BuyGoldTip);
        m_noUseTipPanel.gameObject.SetActive(type == TipType.NoUseTip);
        m_usePlane.gameObject.SetActive(type == TipType.CanUseTip);
        m_compPlane.gameObject.SetActive(false);
    }

    private void UpdateSkillTip(PetSkill.Skill rSkill, int rLevel, EnumPetMood rMood)
    {
        SetActivePanel(TipType.NoUseTip);
        if (rSkill == null)
            return;

        ExpTipItem skillItem = m_noUseTipPanel.gameObject.GetComponentDefault<ExpTipItem>();
        skillItem.RefreshTip(rSkill, rLevel, rMood);
    }

    private void SetTimeShow(PItem item)
    {
        if (item.timeLimit == -1) return;

        //出现剩余时间(可使用tip 不可使用tip)
    }

    private void SetNumber(Transform obj, int count)
    {
        Text numberNo = obj.Find("numberdi/count").GetComponent<Text>();
        numberNo.gameObject.SetActive(count > 0);
        Util.SetText(numberNo, ConfigText.GetDefalutString(204, 30), count);
    }

    private void SetDropBtn(int jumpId, Button btn)
    {
        moduleGlobal.m_dropList = moduleGlobal.GetDropJump(jumpId);
        btn.gameObject.SetActive(moduleGlobal.m_dropList != null && moduleGlobal.m_dropList.Count > 0);
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(delegate { SetjumpInfo(jumpId); });
    }

    private ExpTipItem GetExpTipItem(bool isContainsBtn)
    {
        return (!isContainsBtn ? m_noUseTipPanel : m_usePlane).gameObject.GetComponentDefault<ExpTipItem>();
    }

    private void UpdateAvalibleTip(PItem2 item, bool isContainsBtn, bool drop)
    {
        if (item == null) return;
        UpdateAvalibleTip(item.itemTypeId, isContainsBtn, drop);
        var tipItem = GetExpTipItem(isContainsBtn);
        tipItem?.RefreshTip(item, SetNormalJump, drop);
    }

    private void UpdateAvalibleTip(ushort _itemTypeId, bool isContainsBtn, bool drop, PItem item = null, int nowNum = -1)
    {
        var info = PropItemInfo.Get(_itemTypeId);
        if (info == null) return;

        if (!isContainsBtn)
        {
            SetActivePanel(TipType.NoUseTip);
            ExpTipItem noItem = m_noUseTipPanel.gameObject.GetComponentDefault<ExpTipItem>();
            noItem.RefreshTip(_itemTypeId, item, SetjumpInfo, drop, nowNum);
            return;
        }

        SetActivePanel(TipType.CanUseTip);
        var haveNum = moduleCangku.GetItemCount(_itemTypeId);
        if (item != null) haveNum = moduleCangku.GetItemCount(item.itemId);
        if (nowNum != -1) haveNum = nowNum;
        SetUseInfo(haveNum);

        ExpTipItem showItem = m_usePlane.gameObject.GetComponentDefault<ExpTipItem>();
        showItem.RefreshTip(_itemTypeId, item, SetjumpInfo, drop, nowNum);

        avalible_info.gameObject.SetActive(info.itemType == PropType.UseableProp);
        compose_btn.gameObject.SetActive(false);
        if (info.itemType == PropType.UseableProp)
        {
            Util.SetText(avalibleBtn_text, ConfigText.GetDefalutString(200, 23));
            if (info.subType == (int)UseablePropSubType.PropBag || info.subType == (int)UseablePropSubType.CoinBag)
                Util.SetText(avalibleBtn_text, ConfigText.GetDefalutString(200, 30));

            avalible_btn.onClick.RemoveAllListeners();
            avalible_btn.onClick.AddListener(() =>
            {
                OnClickUseBtn(info, Util.Parse<ushort>(m_getNum.text));
                m_usePlane.gameObject.SetActive(false);
                m_globalTip.SetActive(false);
            });
            return;
        }

        bool isShowComposeBtn = info.compose > 0;
        compose_btn.gameObject.SetActive(isShowComposeBtn);
        if (isShowComposeBtn)
        {
            Util.SetText(composeBtn_text, ConfigText.GetDefalutString(200, 24));
            // compose_btn.transform.SetGray(HasPet(info));
            compose_btn.onClick.RemoveAllListeners();
            compose_btn.onClick.AddListener(() =>
            {
                if (HasPet(info))
                {
                    moduleGlobal.ShowMessage(9708);
                    return;
                }

                SetCompose(_itemTypeId);
            });
        }
    }
    private void SetCompose(ushort itemTypeId)
    {
        m_usePlane.gameObject.SetActive(false);
        m_compPlane.gameObject.SetActive(true);

        CompoundProp(itemTypeId);
    }

    private void SetUseInfo(int haveNum)
    {
        if (haveNum <= 0)
        {
            haveNum = 1;
            avalible_info.gameObject.SetActive(false);
        }
        else SetUseBtn(1, haveNum);

        m_getNumAdd.onClick.RemoveAllListeners();
        m_getNumReduce.onClick.RemoveAllListeners();
        m_getNumMax.onClick.RemoveAllListeners();

        m_getNumAdd.onClick.AddListener(delegate
        {
            int nums = Util.Parse<int>(m_getNum.text);
            nums++;
            m_getNum.text = nums.ToString();
            SetUseBtn(nums, haveNum);
        });
        m_getNumReduce.onClick.AddListener(delegate
        {
            int nums = Util.Parse<int>(m_getNum.text);
            nums--;
            m_getNum.text = nums.ToString();
            SetUseBtn(nums, haveNum);
        });
        m_getNumMax.onClick.AddListener(delegate
        {
            m_getNum.text = haveNum.ToString();
            SetUseBtn(haveNum, haveNum);
        });
    }

    private void SetUseBtn(int num, int haveNum)
    {
        m_getNum.text = num.ToString();
        m_getNumReduce.interactable = num <= 1 ? false : true;
        m_getNumAdd.interactable = haveNum > num ? true : false;
        m_getNumMax.interactable = haveNum < 1 ? false : true;
    }

    private static bool HasPet(PropItemInfo info)
    {
        var PropInfo = ConfigManager.Get<Compound>(info.compose); //合成/分解信息
        if (PropInfo != null)
        {
            foreach (var item in PropInfo.items)
            {
                var targetinfo = ConfigManager.Get<PropItemInfo>(item.itemId);
                if (targetinfo != null &&
                targetinfo.itemType == PropType.Pet &&
                modulePet.GetPet(targetinfo.ID) != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void OnSceneLoadStart()
    {
        m_globalTip?.SetActive(false);
    }

    #region 武器合成

    public void CompoundProp(ushort itemtypeid)
    {
        PropItemInfo debrisitems = ConfigManager.Get<PropItemInfo>(itemtypeid);//碎片信息

        if (debrisitems == null)
        {
            Logger.LogError("ProitemInfo is null");
            return;
        }
        Compound compinfo = ConfigManager.Get<Compound>(debrisitems.compose);//合成/分解信息

        PropItemInfo PropInfo = ConfigManager.Get<PropItemInfo>(compinfo?.items[0]?.itemId ?? 0);//得到的物品信息

        if (PropInfo == null)
        {
            Logger.LogError("PropInfo is null");
            return;
        }

        moduleEquip.Compose_ID = (ushort)debrisitems.compose;//这就是这次打开的碎片可以合成的武器的id

        Util.SetItemInfoSimple(debrisleft.transform, debrisitems);
        Util.SetItemInfoSimple(weponrignt.transform, PropInfo);

        var b = debrisleft?.GetComponentDefault<Button>();
        b?.onClick.RemoveAllListeners();
        b?.onClick.AddListener(() => moduleGlobal.SetTargetMatrial(itemtypeid, compinfo.sourceNum, itemtypeid, false,
            (w, ids) =>
            {
                SetActivePanel(TipType.CanUseTip);
                SetCompose((ushort)ids);
            }));

        int num = moduleEquip.GetPropCount(compinfo.sourceTypeId);
        Util.SetText(fenzi_num, num.ToString());
        Util.SetText(fenmu_num, compinfo.sourceNum.ToString());

        WeaponName.text = PropInfo.itemName;

        Util.SetText(m_tipTtile, ConfigText.GetDefalutString(204, 19));
        Util.SetText(SureComText, ConfigText.GetDefalutString(204, 16));
        var tt = Util.Format(ConfigText.GetDefalutString(204, 20), debrisitems.itemName, PropInfo.itemName);
        Util.SetText(m_tipContent, tt);

        fenzi_num.color = Color.red;
        fenmu_num.color = Color.red;
        if (num >= compinfo.sourceNum)
        {
            fenzi_num.color = Color.white;
            fenmu_num.color = Color.white;
        }
        var maxMulit = num / compinfo.sourceNum;
        if (maxMulit < 1) maxMulit = 1;

        if (PropInfo.itemType == PropType.Pet) maxMulit = 1;
        SetComBtnClick(maxMulit, PropInfo.itemType);
    }

    private void SetComBtnClick(int maxMultiple, PropType type)
    {
        SetComInter(1, maxMultiple);
        m_comMaxBtn.onClick.RemoveAllListeners();
        m_comAddBtn.onClick.RemoveAllListeners();
        m_comReduceBtn.onClick.RemoveAllListeners();

        m_comAddBtn.onClick.AddListener(delegate
        {
            if (type == PropType.Pet)
            {
                ShowMessage(ConfigText.GetDefalutString(204, 47));
                return;
            }
            var nums = Util.Parse<int>(m_comNumber.text);
            SetComInter(nums + 1, maxMultiple);
        });
        m_comReduceBtn.onClick.AddListener(delegate
        {
            int nums = Util.Parse<int>(m_comNumber.text);
            SetComInter(nums - 1, maxMultiple);
        });
        m_comMaxBtn.onClick.AddListener(delegate
        {
            if (type == PropType.Pet)
            {
                ShowMessage(ConfigText.GetDefalutString(204, 47));
                return;
            }
            SetComInter(maxMultiple, maxMultiple);
        });
    }

    private void SetComInter(int canMultiple, int maxMultiple)//当前可以达到的倍数 最大可以合成的倍数
    {
        m_comNumber.text = canMultiple.ToString();
        m_comReduceBtn.interactable = canMultiple <= 1 ? false : true;
        m_comAddBtn.interactable = maxMultiple > canMultiple ? true : false;
    }
    
    #endregion

    private void OnClickUseBtn(PropItemInfo info, ushort num)
    {
        if (!info) return;
        moduleGlobal.currentOpenInfo = info;
        if (info.itemType == PropType.UseableProp)
        {
            var useItem = moduleEquip.currentBagProp.Find((p) => p.itemTypeId == info.ID);
            if (useItem == null) return;
            switch (info.subType)
            {
                case (byte)UseablePropSubType.ExpCardProp:
                case (byte)UseablePropSubType.CoinBag:
                case (byte)UseablePropSubType.MonthCard:
                case (byte)UseablePropSubType.QuarterCard:
                case (byte)UseablePropSubType.PropBag: modulePlayer.SendUsePropBag(useItem.itemId, num); break;
                default: break;
            }
        }
    }

    private void UpdateEnergyPanel(bool isChangeColor = false)
    {
        Util.SetText(tittle_tip, ConfigText.GetDefalutString(204, 5));
        Util.SetText(content_tip, (int)TextForMatType.GlobalUIText, isChangeColor ? 18 : 6);
        Util.SetText(exChangeBtn_text, ConfigText.GetDefalutString(204, 8));

        //剩余次数
        var msg = GeneralConfigInfo.GetNoEnoughColorString(moduleChase.restBuyFatigueCount.ToString());
        Util.SetText(remainEnergy, (int)TextForMatType.ChaseUIText, 2, moduleChase.restBuyFatigueCount > 0 ? moduleChase.restBuyFatigueCount.ToString() : msg);

        Util.SetText(current_Desc, ConfigText.GetDefalutString(204, 7), modulePlayer.roleInfo.fatigue, modulePlayer.maxFatigue);
        var diamond = ConfigManager.Get<PropItemInfo>((int)CurrencySubType.Diamond);
        if (!diamond) return;
        Util.SetItemInfoSimple(costItem, diamond);
        int diamondCount = moduleChase.GetCurrentDiamondCost();
        Util.SetText(costNumber, modulePlayer.roleInfo.diamond >= diamondCount ? "×" + diamondCount : GeneralConfigInfo.GetNoEnoughColorString("×" + diamondCount));
        var fatigue_prop = ConfigManager.Get<PropItemInfo>(15);
        if (fatigue_prop) Util.SetItemInfoSimple(getItem, fatigue_prop);
        Util.SetText(getNumber, "×" + moduleChase.chaseInfo.fatigue.ToString());

        bool canBuy = moduleChase.restBuyFatigueCount > 0 && modulePlayer.roleInfo.diamond >= diamondCount;
        exChangeBtn.interactable = canBuy;
        exChangeBtn.onClick.RemoveAllListeners();
        exChangeBtn.onClick.AddListener(() =>
        {
            exChangeBtn.interactable = false;
            modulePlayer.SendBuyFatigue();
        });
    }

    private void UpdateGoldPanel()
    {
        Util.SetText(tittle_tip, ConfigText.GetDefalutString(204, 1));
        Util.SetText(other_contentTip, ConfigText.GetDefalutString(204, 2));
        Util.SetText(exChangeBtn_text, ConfigText.GetDefalutString(204, 8));

        Util.SetText(current_Desc, ConfigText.GetDefalutString(204, 3), modulePlayer.roleInfo.coin);
        var coin = ConfigManager.Get<PropItemInfo>((int)CurrencySubType.Gold);
        if (!coin) return;
        Util.SetItemInfoSimple(getItem, coin);
        Util.SetText(getNumber, "×" + moduleGlobal.system.buyCoinNum.ToString());

        var prices = moduleGlobal.system.buyCoinPrice;
        if (prices == null || prices.Length < 1) return;

        var currentDayTimes = modulePlayer.roleInfo.dayBuyCoinTimes;
        int index = currentDayTimes < prices.Length - 1 ? currentDayTimes : prices.Length - 1;
        var info = ConfigManager.Get<PropItemInfo>(prices[index].itemTypeId);
        if (!info) return;
        Util.SetItemInfoSimple(costItem, info);

        uint number = GetCurrentCostNumber(prices[index].itemTypeId);
        Util.SetText(costNumber, number >= prices[index].price ? "×" + prices[index].price.ToString() : GeneralConfigInfo.GetNoEnoughColorString("×" + prices[index].price));
        bool canBuy = number >= prices[index].price && (currentDayTimes <= moduleGlobal.system.buyCoinLimit || moduleGlobal.system.buyCoinLimit < 0) ? true : false;
        exChangeBtn.interactable = canBuy;
        exChangeBtn.onClick.RemoveAllListeners();
        exChangeBtn.onClick.AddListener(() =>
        {
            exChangeBtn.interactable = false;
            modulePlayer.SendBuyCoin();
        });
    }
    private void UpdateMoppingUpPanel()
    {
        Util.SetText(tittle_tip, ConfigText.GetDefalutString(204, 37));
        Util.SetText(content_tip, ConfigText.GetDefalutString(204, 39));
        Util.SetText(exChangeBtn_text, ConfigText.GetDefalutString(204, 8));

        Util.SetText(current_Desc, ConfigText.GetDefalutString(204, 38), moduleEquip.GetPropCount(602));
        var sweepItem = ConfigManager.Get<PropItemInfo>(602);
        if (!sweepItem) return;
        Util.SetItemInfoSimple(getItem, sweepItem);
        Util.SetText(getNumber, $"×{moduleGlobal.system.buySweepNum}");

        var prices = moduleGlobal.system.buySweepPrice;
        if (prices == null || prices.Length < 1) return;

        var currentDayTimes = modulePlayer.roleInfo.dayBuySweepTimes;
        int index = currentDayTimes < prices.Length - 1 ? currentDayTimes : prices.Length - 1;
        var info = ConfigManager.Get<PropItemInfo>(prices[index].itemTypeId);
        if (!info) return;
        Util.SetItemInfoSimple(costItem, info);

        uint number = GetCurrentCostNumber(prices[index].itemTypeId);
        Util.SetText(costNumber, number >= prices[index].price ? $"×{prices[index].price}" : GeneralConfigInfo.GetNoEnoughColorString($"×{prices[index].price}"));
        bool canBuy = (moduleGlobal.system.buySweepLimit <= 0 || currentDayTimes < moduleGlobal.system.buySweepLimit);
        exChangeBtn.interactable = canBuy;
        exChangeBtn.onClick.RemoveAllListeners();
        exChangeBtn.onClick.AddListener(() =>
        {
            exChangeBtn.interactable = false;
            modulePlayer.SendBuySweep();
        });
    }


    private uint GetCurrentCostNumber(ushort id)
    {
        if (id == 1) return modulePlayer.roleInfo.coin;
        if (id == 2) return modulePlayer.roleInfo.diamond;
        if (id == 3) return modulePlayer.roleInfo.signet;
        if (id == 5) return modulePlayer.roleInfo.summonStone;
        if (id == 7) return modulePlayer.roleInfo.wishCoin;

        return 0;
    }

    private void UpdateSkillPointPanel()
    {
        Util.SetText(tittle_tip, (int)TextForMatType.SkillUIText, 27);
        Util.SetText(content_tip, (int)TextForMatType.SkillUIText, 28);
        Util.SetText(exChangeBtn_text, ConfigText.GetDefalutString(204, 8));

        if (moduleSkill.skillInfo == null) return;
        Util.SetText(current_Desc, (int)TextForMatType.SkillUIText, 29, moduleSkill.skillInfo.skillPoint, moduleGlobal.system.skillPointLimit);
        var skill_prop = ConfigManager.Get<PropItemInfo>(16);
        if (skill_prop) Util.SetItemInfoSimple(getItem, skill_prop);
        getNumber.text = "×" + moduleGlobal.system.buySkillPointNum.ToString();

        PPrice[] prices = moduleGlobal.system.buySkillPointPrice;
        if (prices == null || prices.Length < 1) return;

        int currentDayTimes = moduleGlobal.system.buySkillPointLimit - moduleSkill.skillInfo.buyTimes;
        var msg = GeneralConfigInfo.GetNoEnoughColorString(currentDayTimes.ToString());
        Util.SetText(remainEnergy, (int)TextForMatType.ChaseUIText, 2, currentDayTimes > 0 ? currentDayTimes.ToString() : msg);

        int index = moduleSkill.skillInfo.buyTimes < prices.Length - 1 ? moduleSkill.skillInfo.buyTimes : prices.Length - 1;
        var info = ConfigManager.Get<PropItemInfo>(prices[index].itemTypeId);
        if (!info) return;
        Util.SetItemInfoSimple(costItem, info);

        uint number = GetCurrentCostNumber(prices[index].itemTypeId);
        Util.SetText(costNumber, number >= prices[index].price ? "×" + prices[index].price : GeneralConfigInfo.GetNoEnoughColorString("×" + prices[index].price));

        exChangeBtn.interactable = currentDayTimes <= 0 ? false : true;
        exChangeBtn.onClick.RemoveAllListeners();
        exChangeBtn.onClick.AddListener(() =>
        {
            if (CanBuySkillPoint()) moduleSkill.SendBuySkillPoint();
            else
            {
                m_globalTip.gameObject.SetActive(false);
                moduleAnnouncement.OpenWindow(32);
            }
        });
    }

    private bool CanBuySkillPoint()
    {
        PPrice[] prices = moduleGlobal.system.buySkillPointPrice;
        if (prices == null || prices.Length < 1) return false;
        int index = moduleSkill.skillInfo.buyTimes < prices.Length - 1 ? moduleSkill.skillInfo.buyTimes : prices.Length - 1;
        int currentDayTimes = moduleGlobal.system.buySkillPointLimit - moduleSkill.skillInfo.buyTimes;
        uint number = GetCurrentCostNumber(prices[index].itemTypeId);
        return number >= prices[index].price && currentDayTimes > 0 ? true : false;
    }

    private void UpdateExchangeTipPanel(TipType type, bool isChangeColor = false)
    {
        SetActivePanel(type);
        remainEnergy.gameObject.SetActive(type == TipType.BuyEnergencyTip || type == TipType.BuySkillPointTip);
        switch (type)
        {
            case TipType.BuyGoldTip:         UpdateGoldPanel(); break;
            case TipType.BuyEnergencyTip:    UpdateEnergyPanel(isChangeColor);break;
            case TipType.BuySkillPointTip:   UpdateSkillPointPanel();break;
            case TipType.MoppingUpTip:       UpdateMoppingUpPanel(); break;
            default: break;
        }
    }
    #endregion

    #region StoryMask

    private void OpenStoryMask()
    {
        ShowAnim(true);
        DelayEvents.Remove(m_storyMaskDelayEventId);
        m_storyMaskDelayEventId = DelayEvents.Add(CloseStoryMask, GeneralConfigInfo.sguideStoryMaskTime);
    }

    private void CloseStoryMask()
    {
        ShowAnim(false);
    }

    private void ShowAnim(bool black)
    {
        if (!m_storyMask) return;

        m_storyMask.DOKill();
        //mask become black immediately
        if (black)
        {
            m_storyMask.color = Color.black;
            m_storyMask.gameObject.SetActive(true);
        }
        else
        {
            m_storyMask.DOFade(0, StoryConst.BASE_DIALOG_ALPHA_DURACTION).OnComplete(() =>
            {
                m_storyMask.gameObject.SetActive(false);
            });
        }
    }

    #endregion

    #region drop

    private void SetjumpInfo(int dropId)//设置跳转的信息
    {
        SetNormalInfo(dropId);
    }

    private void SetNormalJump(PItem2 item)//设置跳转的信息
    {
        SetNormalInfo(item.itemTypeId, item);
    }

    private void SetNormalInfo(int dropId, PItem2 item = null)
    {
        PropItemInfo dropInfos = ConfigManager.Get<PropItemInfo>(dropId);// 武器碎片信息
        if (dropInfos == null)
        {
            Logger.LogError("can not find debris id from propitem " + dropId);
            return;
        }

        if (item != null) Util.SetItemInfo(m_dropObj, dropInfos, item.level, 0, true, item.star);
        else Util.SetItemInfo(m_dropObj, dropInfos);

        m_dropView.progress = 0;
        m_dropPlane.gameObject.SetActive(true);
        m_canGet.gameObject.SetActive(false);
        m_noGet.gameObject.SetActive(true);

        moduleGlobal.m_dropList = moduleGlobal.GetDropJump(dropId);
        if (moduleGlobal.m_dropList == null) return;
        m_allDropList.SetItems(moduleGlobal.m_dropList);
        if (moduleGlobal.m_dropList.Count > 0)
        {
            m_canGet.gameObject.SetActive(true);
            m_noGet.gameObject.SetActive(false);
        }
    }

    private void SetDropInfo(RectTransform rt, DropInfo info)
    {
        if (info == null) return;
        Button obj_btn = rt.gameObject.GetComponent<Button>();
        Text str = rt.gameObject.transform.Find("name").GetComponent<Text>();
        Transform img = rt.gameObject.transform.Find("Image");

        Util.SetText(str, info.desc);
        obj_btn.interactable = info.open;
        img.gameObject.SetActive(info.open);
    }

    private void DropOnClick(RectTransform rt, DropInfo info)
    {
        if (!info.open) return;
        if (!moduleGuide.HasFinishGuide(GeneralConfigInfo.defaultConfig.dropOpenID))
        {
            moduleGlobal.ShowMessage(204, 40);
            return;
        }

        if (!(Level.current is Level_Home))
        {
            if (Level.current is Level_Labyrinth)
                moduleLabyrinth.SendExitLabyrinth();
            Root.instance.StartCoroutine(GoHomeDrop(info));
            return;
        }

        DropSkip(info);
    }

    private IEnumerator GoHomeDrop(DropInfo info)
    {
        yield return Game.GoHome();
        DropSkip(info);
    }

    private void DropSkip(DropInfo info)
    {
        if (info.windowType > 1000)
        {
            var type = Util.Parse<int>(info.wName);
            UpdateExchangeTipPanel((TipType)type);
            return;
        }
        else moduleGlobal.SetGoToDrop(info);

        m_dropPlane.gameObject.SetActive(false);
        m_globalTip.gameObject.SetActive(false);
    }

    #endregion

    #region 战斗力
    private void SetCombatFight(int news, int olds)//战斗力发生变化
    {
        if (news == olds) return;
        m_fightTime = GeneralConfigInfo.defaultConfig.fightTime;
        m_fightDelay = GeneralConfigInfo.defaultConfig.fightDelayClose;
        m_fightPlane.gameObject.SetActive(true);
        if (m_fightOpen)
        {
            m_fightOpen = false;
            m_fightValue.text = olds.ToString();
            m_seqience = DOTween.Sequence();
            m_seqience.SetEase(Ease.Linear);
        }
        var start = Util.Parse<int>(m_fightValue.text);

        m_seqience.Append(DOTween.To(() => start, x => { start = Mathf.RoundToInt(x); m_fightValue.text = start.ToString(); }, news, m_fightTime));

        m_seqience.OnComplete(delegate
        {
            m_fightOpen = true;
            DelayEvents.Add(() =>
            {
                DelayEvents.Remove("fightEnd");
                if (m_fightOpen) m_fightPlane.gameObject.SetActive(false);
            }, m_fightDelay, "fightEnd");
        });

        if (news > olds)
        {
            var str = "+" + (news - olds).ToString();
            CloneFight(m_fightUp, str, m_fightUpParent);
        }
        else
        {
            var str = "-" + (olds - news).ToString();
            CloneFight(m_fightDown, str, m_fightDownParent);
        }
    }

    private void CloneFight(GameObject child, string value, Transform parent)
    {
        var obj = GameObject.Instantiate(child);
        var rt = obj.GetComponent<RectTransform>();
        obj.transform.SetParent(parent, false);
        rt.anchoredPosition = Vector3.zero;
        rt.localScale = new Vector3(1, 1, 1);
        obj.gameObject.SetActive(true);
        var t = obj.GetComponent<Text>();
        Util.SetText(t, value);
    }
    #endregion
}

public enum TipType
{
    None,

    CanUseTip,               //可使用或合成tip

    NoUseTip,                //不能使用和合成的tip

    BuyGoldTip,              //买金币的tip

    BuyEnergencyTip,         //买体力的tip

    BuySkillPointTip,         //购买技能点

    RuneTip,                    //符文

    MoppingUpTip,               //购买扫荡卷
}