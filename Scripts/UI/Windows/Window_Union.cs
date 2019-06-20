/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-07-19
 * 
 ***************************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Window_Union : Window
{
    #region no guild（join界面未设置）

    private GameObject m_noGuild;
    private Toggle m_reconnedBtn;
    private Toggle m_selectBtn;
    private Toggle m_createBtn;
    private GameObject m_recommendPlane;
    private GameObject m_searchPlane;
    private GameObject m_createPlane;
    private GameObject m_joinPlane;

    //推荐
    private GameObject m_randomList;
    private GameObject m_randomnNullList;
    public Button m_fastAddBtn;
    public Button m_refreshBtn;
    //搜索
    private InputField m_selectTxt;
    private Button m_slelctBtn;
    private GameObject m_selectListPlane;
    private GameObject m_selectNullPlane;
    //创建
    private InputField m_createText;
    private Button m_createUnionBtn;
    private GameObject m_expendDemand;
    private GameObject m_expendIcon;
    private GameObject m_expendLevel;
    private Text m_demandText;
    private Image m_demandImg;
    private Text m_iconText;
    private Image m_iconImg;
    private Text m_levelText;
    //申请界面

    private Text m_addName;
    private Text m_addLevel;
    private Text m_addNum;
    private Text m_addPrincesName;
    private Text m_addNotice;
    private Button m_addApply;//加入申请
    private Text m_addText;

    #endregion

    #region have guild 
    private GameObject m_haveGuild;

    //main plane
    private GameObject m_mainPlane;
    private Image m_senValueBar;
    private Text m_mainName;
    private Text m_mainLevel;
    private Text m_mainSentiment;
    private Text m_expendSentiment;
    private Text m_notice;
    private Button m_unionApply;
    private Text m_applyNum;
    private Button m_unionManage;
    private Text m_playerNowTop;
    private Button m_chatBtn;
    private Button m_openGuildBtn;
    private GameObject m_mainContent;
    private GameObject m_mainObj;
    //apply plane
    private GameObject m_applyPlane;
    private Text m_playerHave;
    private Text m_applyNumAll;
    private Button m_agreeAllBtn;
    private Button m_ignoreAllBtn;
    //join Condition Plane
    private GameObject m_joinConditionPlane;
    private Text m_joinLevel;
    private Slider m_joinSlider;
    private Toggle m_automaticState;//是否开启自动加入公会
    private Slider m_automaticSlider;
    private Text m_automaticLevel;
    private Button m_joinSetSave;
    private Button m_joinSetReturn;
    //baseinfo( info manager online)
    private GameObject m_guildBasePlane;
    private Toggle m_infoBtn;
    private Toggle m_playerBtn;
    private Toggle m_managerBtn;
    //guild
    private GameObject m_baseInfo;
    private Text m_baseNotice;
    private Text m_baseName;
    private Text m_baseID;
    private Text m_baseLevel;
    private Text m_basePresident;
    private Text m_baseNum;
    private Text m_baseAllSentiment;
    private Text m_baseProgress;
    private Text m_baseConsyme;
    private Image m_baseProgressValue;
    private Button m_quitButton;
    private GameObject m_baseSysContent;
    private GameObject m_baseObj;
    //player manager plane
    private GameObject m_playerPlane;
    private Text m_playerCurrentNum;
    private Text m_playerIsOnline;
    // manager plane
    private GameObject m_managerSetPlane;
    private Button m_mangerJoinBtn;
    private Button m_mangerChangeNoticeBtn;
    private Button m_mangerDissolutionBtn;
    private Button m_managerShowApply;
    private GameObject m_changeNoticePlane;//修改公告界面
    private Button m_changeNoticeBtn;
    private InputField m_changeNotice;

    //details plane
    private GameObject m_detailPlane;
    private Button m_detailAdd;
    #endregion

    public Button m_cardBtn;
    public GameObject m_cardEffect;
    public Button m_rewardBtn;
    public Text m_rewardText;

    private Text m_bossOpenTime;//boss入侵倒计时
    private Button m_bossOpen;
    private Button m_bossClose;

    Regex r = new Regex(@"^\d+$");

    private DataSource<PRefreshInfo> m_refreshInfo;
    private DataSource<PRefreshInfo> m_slectInfo;
    private DataSource<PPlayerInfo> m_applyInfo;
    private DataSource<PUnionPlayer> m_unionPlayerInfo;//成员管理

    public Queue<GameObject> m_dynamicMainObj = new Queue<GameObject>();//主界面纯消息的动态
    public Queue<GameObject> m_dynamicBaseObj = new Queue<GameObject>();//base界面纯消息的动态
    
    private bool m_closeEnd = false;
    private float m_closeTime = 0;
    private int m_closeTimeNow;

    protected override void OnOpen()
    {
        m_dynamicMainObj.Clear();
        m_dynamicBaseObj.Clear();
        m_bossOpenTime = GetComponent<Text>("haveGuild/main_Panel/stage01On_Btn/stage01On_Txt_03");
        m_bossOpen = GetComponent<Button>("haveGuild/main_Panel/stage01On_Btn");
        m_bossClose = GetComponent<Button>("haveGuild/main_Panel/stage01Off_Btn");

        m_cardEffect = GetComponent<RectTransform>("haveGuild/main_Panel/cardBtn/effect").gameObject;
        m_cardBtn = GetComponent<Button>("haveGuild/main_Panel/cardBtn");
        m_cardBtn.onClick.AddListener(delegate
        {
            ShowAsync<Window_Unionsign>();
        });
        m_rewardBtn = GetComponent<Button>("haveGuild/main_Panel/rightBottom/reward_Btn");
        m_rewardText= GetComponent<Text>("haveGuild/main_Panel/rightBottom/reward_Btn/content_Txt");
        m_rewardBtn.onClick.AddListener(delegate
        {
            ShowAsync<Window_Unionreward>();
        });

        #region no guild path

        m_noGuild = GetComponent<RectTransform>("noGuild").gameObject;

        m_reconnedBtn = GetComponent<Toggle>("noGuild/checkBox_01/1");
        m_selectBtn = GetComponent<Toggle>("noGuild/checkBox_01/2");
        m_createBtn = GetComponent<Toggle>("noGuild/checkBox_01/3");
        m_recommendPlane = GetComponent<RectTransform>("noGuild/recommend_Panel").gameObject;
        m_searchPlane = GetComponent<RectTransform>("noGuild/search_Panel").gameObject;
        m_createPlane = GetComponent<RectTransform>("noGuild/create_Panel").gameObject;
        m_joinPlane = GetComponent<RectTransform>("noGuild/joinInfo_Panel").gameObject;
        //推荐
        m_randomList = GetComponent<RectTransform>("noGuild/recommend_Panel/recommendList_ScrollView").gameObject;
        m_randomnNullList = GetComponent<RectTransform>("noGuild/recommend_Panel/noRecommend_Txt").gameObject;
        m_fastAddBtn = GetComponent<Button>("noGuild/recommend_Panel/autoJoin_Btn");
        m_refreshBtn = GetComponent<Button>("noGuild/recommend_Panel/refresh_Btn");
        m_refreshInfo = new DataSource<PRefreshInfo>(moduleUnion.m_refrshList, GetComponent<ScrollView>("noGuild/recommend_Panel/recommendList_ScrollView"), SetRecomedInfo);
        //搜索
        m_selectTxt = GetComponent<InputField>("noGuild/search_Panel/searchID_InputField");
        m_slelctBtn = GetComponent<Button>("noGuild/search_Panel/search_Btn");
        m_selectListPlane = GetComponent<RectTransform>("noGuild/search_Panel/searchList_ScrollView").gameObject;
        m_selectNullPlane = GetComponent<Image>("noGuild/search_Panel/searchnull_Img").gameObject;
        m_slectInfo = new DataSource<PRefreshInfo>(moduleUnion.selectUnionList, GetComponent<ScrollView>("noGuild/search_Panel/searchList_ScrollView"), SetSelectInfo);
        //创建
        m_createText = GetComponent<InputField>("noGuild/create_Panel/createName_InputField");
        m_createUnionBtn = GetComponent<Button>("noGuild/create_Panel/create_Btn");
        m_expendDemand = GetComponent<RectTransform>("noGuild/create_Panel/demand_01").gameObject;
        m_expendIcon = GetComponent<RectTransform>("noGuild/create_Panel/demand_02").gameObject;
        m_expendLevel = GetComponent<RectTransform>("noGuild/create_Panel/demand_03").gameObject;
        m_demandText = GetComponent<Text>("noGuild/create_Panel/demand_01/demand_Img/demand_Txt");
        m_demandImg = GetComponent<Image>("noGuild/create_Panel/demand_01/demand_Img");
        m_iconText = GetComponent<Text>("noGuild/create_Panel/demand_02/demand_Img/demand_Txt");
        m_iconImg = GetComponent<Image>("noGuild/create_Panel/demand_02/demand_Img");
        m_levelText = GetComponent<Text>("noGuild/create_Panel/demand_03/demand_Txt/demand_Txt (1)");

        //加入
        m_addName = GetComponent<Text>("noGuild/joinInfo_Panel/middle/name_Txt");
        m_addLevel = GetComponent<Text>("noGuild/joinInfo_Panel/middle/rate_Txt");
        m_addNotice = GetComponent<Text>("noGuild/joinInfo_Panel/text_back/detail_viewport/detail_content");
        m_addNum = GetComponent<Text>("noGuild/joinInfo_Panel/middle/member_Txt");
        m_addPrincesName = GetComponent<Text>("noGuild/joinInfo_Panel/middle/president_Txt");
        m_addApply = GetComponent<Button>("noGuild/joinInfo_Panel/middle/join_Btn");
        m_addText = GetComponent<Text>("noGuild/joinInfo_Panel/middle/join_Btn/join_Txt");
        SetNoCuildClick();
        #endregion

        #region have guild path
        m_haveGuild = GetComponent<RectTransform>("haveGuild").gameObject;

        //main
        m_mainPlane = GetComponent<RectTransform>("haveGuild/main_Panel").gameObject;
        m_senValueBar= GetComponent<Image>("haveGuild/main_Panel/guildInfo_Btn/guildInfoProgressFilled_Img");
        m_mainName = GetComponent<Text>("haveGuild/main_Panel/guildInfo_Btn/guildName_Txt");
        m_mainLevel = GetComponent<Text>("haveGuild/main_Panel/guildInfo_Btn/rate_Txt");
        m_mainSentiment = GetComponent<Text>("haveGuild/main_Panel/guildInfo_Btn/guildPopularityCurrent_Txt");
        m_expendSentiment = GetComponent<Text>("haveGuild/main_Panel/guildInfo_Btn/guildPopularityConsumed_Txt");
        m_notice = GetComponent<Text>("haveGuild/main_Panel/announcement_Toggle/announcementContent_Toggle_Txt");
        m_unionApply = GetComponent<Button>("haveGuild/main_Panel/rightBottom/apply_Btn");
        m_applyNum = GetComponent<Text>("haveGuild/main_Panel/rightBottom/apply_Btn/content_Txt");
        m_unionManage = GetComponent<Button>("haveGuild/main_Panel/rightBottom/management_Btn");
        m_playerNowTop = GetComponent<Text>("haveGuild/main_Panel/rightBottom/management_Btn/content_Txt");
        m_chatBtn = GetComponent<Button>("haveGuild/main_Panel/chat_Btn");
        m_mainContent = GetComponent<RectTransform>("haveGuild/main_Panel/state_ScrollView/Viewport/Content").gameObject;
        m_mainObj = GetComponent<RectTransform>("haveGuild/main_Panel/state_ScrollView/Viewport/sys-img").gameObject;
        m_openGuildBtn = GetComponent<Button>("haveGuild/main_Panel/guildInfo_Btn");
        //apply
        m_applyPlane = GetComponent<RectTransform>("haveGuild/applicationInfo_Panel").gameObject;
        m_playerHave = GetComponent<Text>("haveGuild/applicationInfo_Panel/currentMember_Txt");
        m_applyNumAll = GetComponent<Text>("haveGuild/applicationInfo_Panel/applicationNumber_Txt");
        m_agreeAllBtn = GetComponent<Button>("haveGuild/applicationInfo_Panel/agreeAll_Btn");
        m_ignoreAllBtn = GetComponent<Button>("haveGuild/applicationInfo_Panel/refuseAll_Btn");
        m_applyInfo = new DataSource<PPlayerInfo>(moduleUnion.m_unionApply, GetComponent<ScrollView>("haveGuild/applicationInfo_Panel/applicationList_ScrollView"), SetApplyInfo);
        //join Condition 
        m_joinConditionPlane = GetComponent<RectTransform>("haveGuild/joinCondition_Panel").gameObject;
        m_joinLevel = GetComponent<Text>("haveGuild/joinCondition_Panel/joinConditionBar/joinConditionBarPointer_Img/conditionBarPointer_Txt");
        m_joinSlider = GetComponent<Slider>("haveGuild/joinCondition_Panel/joinConditionBar");
        m_automaticState = GetComponent<Toggle>("haveGuild/joinCondition_Panel/openAutoJoinCondition_Toggle");
        m_automaticSlider = GetComponent<Slider>("haveGuild/joinCondition_Panel/autoJoinConditionBar");
        m_automaticLevel = GetComponent<Text>("haveGuild/joinCondition_Panel/autoJoinConditionBar/autoJoinConditionBarPointer_Img/autoJoinConditionBar_Txt");
        m_joinSetSave = GetComponent<Button>("haveGuild/joinCondition_Panel/saveJoinCondition_Btn");
        m_joinSetReturn = GetComponent<Button>("haveGuild/joinCondition_Panel/cancelJoinCondition_Btn");
        //( info manager online)
        m_guildBasePlane = GetComponent<RectTransform>("haveGuild/guildbaseinfo_plane").gameObject;
        m_infoBtn = GetComponent<Toggle>("haveGuild/guildbaseinfo_plane/checkBox_02/1");
        m_playerBtn = GetComponent<Toggle>("haveGuild/guildbaseinfo_plane/checkBox_02/2");
        m_managerBtn = GetComponent<Toggle>("haveGuild/guildbaseinfo_plane/checkBox_02/3");
        
        //guild
        m_baseInfo = GetComponent<RectTransform>("haveGuild/guildbaseinfo_plane/guildInfo_Panel").gameObject;
        m_baseNotice = GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/announcement_Panel/noticeText");
        m_baseName = GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/details_Panel/guildName_Txt/name_Txt");
        m_baseID = GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/details_Panel/guildID_Txt/ID_Txt");
        m_baseLevel = GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/details_Panel/guildRate_Txt/rate_Txt");
        m_basePresident = GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/details_Panel/guildOwner_Txt/owner_Txt");
        m_baseNum = GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/details_Panel/guildMember_Txt/member_Txt");
        m_baseAllSentiment = GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/details_Panel/guildPopularityCurrent_Txt/popularity_Txt");
        m_baseProgress = GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/details_Panel/progressRect/guildPopularityProgress_Txt");
        m_baseConsyme = GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/details_Panel/progressRect/guildPopularityConsume_Txt");
        m_baseProgressValue = GetComponent<Image>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/details_Panel/progressRect/guildPopularityProgress_Img");
        m_quitButton = GetComponent<Button>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/details_Panel/quit_Btn");
        m_baseSysContent = GetComponent<RectTransform>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/state_Panel/Scroll Rect/Viewport/Content").gameObject;
        m_baseObj = GetComponent<RectTransform>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/state_Panel/Scroll Rect/Viewport/sys-img").gameObject;
        //player manager
        m_playerPlane = GetComponent<RectTransform>("haveGuild/guildbaseinfo_plane/guildMemberOnline_Panel").gameObject;
        m_playerCurrentNum = GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildMemberOnline_Panel/memberInfo_Panel/currentMember_Txt");
        m_playerIsOnline = GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildMemberOnline_Panel/memberInfo_Panel/onlineMember_Txt");
        m_unionPlayerInfo = new DataSource<PUnionPlayer>(moduleUnion.m_unionPlayer, GetComponent<ScrollView>("haveGuild/guildbaseinfo_plane/guildMemberOnline_Panel/memberList_ScrollView"), ManagerPlayerInfo, ManagerPlayerClick);
        // manager 
        m_managerSetPlane = GetComponent<RectTransform>("haveGuild/guildbaseinfo_plane/guildManage_Panel").gameObject;
        m_mangerJoinBtn = GetComponent<Button>("haveGuild/guildbaseinfo_plane/guildManage_Panel/joinCondition_Btn");
        m_mangerChangeNoticeBtn = GetComponent<Button>("haveGuild/guildbaseinfo_plane/guildManage_Panel/modifyAnnouncement_Btn");
        m_mangerDissolutionBtn = GetComponent<Button>("haveGuild/guildbaseinfo_plane/guildManage_Panel/disbandGuild_Btn");
        m_managerShowApply = GetComponent<Button>("haveGuild/guildbaseinfo_plane/guildManage_Panel/applicationInfo_Btn");

        m_changeNoticePlane = GetComponent<RectTransform>("haveGuild/announcement_Panel").gameObject;
        m_changeNoticeBtn = GetComponent<Button>("haveGuild/announcement_Panel/introud_plane/sureAnnouncement_Btn");
        m_changeNotice = GetComponent<InputField>("haveGuild/announcement_Panel/introud_plane/announcement_InputField");

        //detail
        m_detailPlane = GetComponent<RectTransform>("haveGuild/characterDetails_panel").gameObject;
        m_detailAdd  = GetComponent<Button>("haveGuild/characterDetails_panel/notfriend/add_Btn");
        SethaveGuildBtn();
        #endregion
        
        SetText();
        moduleUnion.SetAllNotice();
        m_joinSetSave.interactable = true;
    }

    private void SetCloseTime()
    {
        if (moduleUnion.BossInfo == null)
        {
            Logger.LogError("no boss info");
            return;
        }

        if (moduleUnion.BossInfo.bossstate == 0)//关闭
        {
            m_closeEnd = false;
            m_bossOpen.gameObject.SetActive(false);
            m_bossClose.gameObject.SetActive(true);

        }
        else if (moduleUnion.BossInfo.bossstate == 1)
        {
            m_closeTimeNow = (int)(moduleUnion.BossInfo.opentime - moduleUnion.BossLossTime(moduleUnion.m_bossCloseTime));
            if (m_closeTimeNow > 0)
            {

                SetTime(m_closeTimeNow, m_bossOpenTime);
                m_bossOpen.gameObject.SetActive(true);
                m_bossClose.gameObject.SetActive(false);
                m_closeEnd = true;
            }
            else
            {
                m_closeEnd = false;
                m_closeTimeNow = 0;
            }
        }
    }

    private void SetText()
    {
        Util.SetText(GetComponent<Text>("noGuild/recommend_Panel/noRecommend_Txt"), 242, 136);
        Util.SetText(GetComponent<Text>("haveGuild/characterDetails_panel/button_group/removeBlack/delete_text"), 218, 68);
        Util.SetText(GetComponent<Text>("haveGuild/characterDetails_panel/button_group/blackBtn/delete_text"), 218, 67);
        Util.SetText(GetComponent<Text>("noGuild/joinInfo_Panel/middle/cancel_Btn/join_Txt"), 200, 5);
        Util.SetText(GetComponent<Text>("noGuild/search_Panel/searchnull_Img/searchnull"), 242, 188);

        Util.SetText(GetComponent<Text>("noGuild/create_Panel/demand_01/demand_expend"), 242, 180);
        Util.SetText(GetComponent<Text>("noGuild/create_Panel/demand_02/demand_expend"), 242, 181);

        Util.SetText(GetComponent<Text>("noGuild/create_Panel/demand_03/demand_Txt"), 242, 62);
        Util.SetText(GetComponent<Text>("haveGuild/main_Panel/stage01On_Btn/stage01On_Img_03/stage01On_Txt_04"), 242, 161);
        Util.SetText(GetComponent<Text>("haveGuild/main_Panel/announcement_Toggle/announcementTitle_Toggle_Txt"), 242, 0);
        Util.SetText(GetComponent<Text>("haveGuild/main_Panel/guildInfo_Btn/guildRate_Txt"), 242, 1);
        Util.SetText(GetComponent<Text>("haveGuild/main_Panel/stage01Off_Btn/stage01Off_Txt"), 242, 2);
        Util.SetText(GetComponent<Text>("haveGuild/main_Panel/rightBottom/apply_Btn/apply_Txt"), 242, 4);
        Util.SetText(GetComponent<Text>("haveGuild/main_Panel/rightBottom/management_Btn/management_Txt"), 242, 5);
        Util.SetText(GetComponent<Text>("haveGuild/main_Panel/chat_Btn/text"), 242, 6);
        Util.SetText(GetComponent<Text>("haveGuild/applicationInfo_Panel/agreeAll_Btn/agreeAll_Txt"), 242, 7);
        Util.SetText(GetComponent<Text>("haveGuild/applicationInfo_Panel/refuseAll_Btn/refuseAll_Txt"), 242, 8);
        Util.SetText(GetComponent<Text>("haveGuild/joinCondition_Panel/joinConditionBar/joinCondition_Txt"), 242, 9);
        Util.SetText(GetComponent<Text>("haveGuild/joinCondition_Panel/autoJoinConditionBar/autoJoinCondition_Txt"), 242, 10);
        Util.SetText(GetComponent<Text>("haveGuild/joinCondition_Panel/openAutoJoinCondition_Toggle/openAutoJoinCondition_Txt"), 242, 11);
        Util.SetText(GetComponent<Text>("haveGuild/joinCondition_Panel/saveJoinCondition_Btn/saveJoinCondition_Txt"), 242, 12);
        Util.SetText(GetComponent<Text>("haveGuild/joinCondition_Panel/cancelJoinCondition_Btn/cancelJoinCondition_Txt"), 242, 13);
        Util.SetText(GetComponent<Text>("haveGuild/main_Panel/stage01Off_Btn/stage01Off_Txt_01"), 242, 14);
        Util.SetText(GetComponent<Text>("haveGuild/main_Panel/stage01On_Btn/stage01Off_Txt_04"), 242, 192);
        Util.SetText(GetComponent<Text>("haveGuild/main_Panel/stage01Off_Btn/stage01Off_Txt_02"), 242, 192);
        Util.SetText(GetComponent<Text>("haveGuild/main_Panel/stage01On_Btn/stage01On_Txt_02"), 242, 15);
        Util.SetText(GetComponent<Text>("haveGuild/main_Panel/stage01On_Btn/stage01On_Txt_01"), 242, 14);

        Util.SetText(GetComponent<Text>("haveGuild/applicationInfo_Panel/title_Txt"), 242, 16);
        Util.SetText(GetComponent<Text>("haveGuild/applicationInfo_Panel/refuseAll_Btn/refuseAll_Txt"), 242, 8);
        Util.SetText(GetComponent<Text>("haveGuild/applicationInfo_Panel/agreeAll_Btn/agreeAll_Txt"), 242, 7);

        Util.SetText(GetComponent<Text>("haveGuild/joinCondition_Panel/title_Txt"), 242, 17);
        Util.SetText(GetComponent<Text>("haveGuild/joinCondition_Panel/joinConditionBar/joinCondition_Txt"), 242, 18);
        Util.SetText(GetComponent<Text>("haveGuild/joinCondition_Panel/autoJoinConditionBar/autoJoinCondition_Txt"), 242, 20);

        Util.SetText(GetComponent<Text>("haveGuild/joinCondition_Panel/joinConditionBar/conditionScale_01"), 242, 21);
        Util.SetText(GetComponent<Text>("haveGuild/joinCondition_Panel/joinConditionBar/conditionScale_11"), 242, 22);
        Util.SetText(GetComponent<Text>("haveGuild/joinCondition_Panel/autoJoinConditionBar/conditionScale_01"), 242, 21);
        Util.SetText(GetComponent<Text>("haveGuild/joinCondition_Panel/autoJoinConditionBar/conditionScale_11"), 242, 22);

        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/title_Txt"), 242, 23);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/announcement_Panel/announcementTitle_Panel_Txt"), 242, 24);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/state_Panel/stateTitle_Panel_Txt"), 242, 25);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/details_Panel/guildName_Txt"), 242, 26);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/details_Panel/guildID_Txt"), 242, 27);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/details_Panel/guildRate_Txt"), 242, 28);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/details_Panel/guildOwner_Txt"), 242, 29);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/details_Panel/guildMember_Txt"), 242, 30);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/details_Panel/guildPopularityCurrent_Txt"), 242, 31);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/details_Panel/progressRect/nextLevel_Txt"), 242, 32);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildInfo_Panel/details_Panel/quit_Btn/quit_Txt"), 242, 33);

        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildMemberOnline_Panel/title_Txt"), 242, 34);

        Util.SetText(GetComponent<Text>("haveGuild/characterDetails_panel/title_Txt"), 242, 35);
        Util.SetText(GetComponent<Text>("haveGuild/characterDetails_panel/president_Btn/delete_text"), 242, 36);
        Util.SetText(GetComponent<Text>("haveGuild/characterDetails_panel/vicepresident_Btn/delete_text"), 242, 37);
        Util.SetText(GetComponent<Text>("haveGuild/characterDetails_panel/button_group/remove_Btn/delete_text"), 242, 38);
        Util.SetText(GetComponent<Text>("haveGuild/characterDetails_panel/notfriend/add_Btn/add_text"), 242, 39);
        Util.SetText(GetComponent<Text>("haveGuild/characterDetails_panel/isfriend/chat_Btn/chat_text"), 242, 40);
        Util.SetText(GetComponent<Text>("haveGuild/characterDetails_panel/isfriend/compare_Btn/compare_text"), 242, 41);
        Util.SetText(GetComponent<Text>("haveGuild/characterDetails_panel/isfriend/delete_Btn/delete_text"), 242, 42);

        Util.SetText(GetComponent<Text>("haveGuild/announcement_Panel/bg/equip_prop/top/equipinfo"), 242, 23);
        Util.SetText(GetComponent<Text>("haveGuild/announcement_Panel/introud_plane/cancelAnnouncement_Btn/sureAnnouncement_Txt"), 200, 5);
        Util.SetText(GetComponent<Text>("haveGuild/announcement_Panel/introud_plane/sureAnnouncement_Btn/sureAnnouncement_Txt"), 200, 4);

        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildManage_Panel/title_Txt"), 242, 43);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildManage_Panel/disbandGuild_Btn/disbandGuild_Txt"), 242, 44);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildManage_Panel/modifyAnnouncement_Btn/modifyAnnouncement_Txt_02"), 242, 45);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildManage_Panel/modifyAnnouncement_Btn/modifyAnnouncement_Txt"), 242, 24);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildManage_Panel/joinCondition_Btn/joinCondition_Txt_02"), 242, 189);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildManage_Panel/joinCondition_Btn/joinCondition_Txt"), 242, 46);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildManage_Panel/applicationInfo_Btn/applicationInfo_Txt_02"), 242, 189);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/guildManage_Panel/applicationInfo_Btn/applicationInfo_Txt"), 242, 48);

        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/checkBox_02/1/xz_text"), 242, 47);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/checkBox_02/2/xz_text"), 242, 35);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/checkBox_02/3/xz_text"), 242, 50);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/checkBox_02/1/Text"), 242, 47);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/checkBox_02/2/Text"), 242, 35);
        Util.SetText(GetComponent<Text>("haveGuild/guildbaseinfo_plane/checkBox_02/3/Text"), 242, 50);

        Util.SetText(GetComponent<Text>("noGuild/recommend_Panel/autoJoin_Btn/autoJoin_Txt"), 242, 51);
        Util.SetText(GetComponent<Text>("noGuild/recommend_Panel/refresh_Btn/refresh_Txt"), 242, 52);
        Util.SetText(GetComponent<Text>("noGuild/checkBox_01/1/Text"), 242, 53);
        Util.SetText(GetComponent<Text>("noGuild/checkBox_01/1/xz_Txt"), 242, 53);
        Util.SetText(GetComponent<Text>("noGuild/checkBox_01/2/Text"), 242, 54);
        Util.SetText(GetComponent<Text>("noGuild/checkBox_01/2/xz_Txt"), 242, 54);
        Util.SetText(GetComponent<Text>("noGuild/checkBox_01/3/Text"), 242, 55);
        Util.SetText(GetComponent<Text>("noGuild/checkBox_01/3/xz_Txt"), 242, 55);
        Util.SetText(GetComponent<Text>("noGuild/search_Panel/searchID_InputField/searchIDPlaceholder_Txt"), 242, 56);
        Util.SetText(GetComponent<Text>("noGuild/search_Panel/search_Btn/search_Txt"), 242, 57);
        Util.SetText(GetComponent<Text>("noGuild/create_Panel/createName_InputField/createNamePlaceholder_Txt"), 242, 58);
        Util.SetText(GetComponent<Text>("noGuild/create_Panel/create_Btn/create_Txt"), 242, 59);
        Util.SetText(GetComponent<Text>("noGuild/joinInfo_Panel/middle/guildName_Txt"), 242, 26);
        Util.SetText(GetComponent<Text>("noGuild/joinInfo_Panel/middle/guildRate_Txt"), 242, 28);
        Util.SetText(GetComponent<Text>("noGuild/joinInfo_Panel/middle/guildMember_Txt"), 242, 30);
        Util.SetText(GetComponent<Text>("noGuild/joinInfo_Panel/middle/guildPresident_Txt"), 242, 29);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleUnion.m_inOtherPalne = false;
        moduleGlobal.ShowGlobalLayerDefault();//top工具条的显示
        m_detailPlane.gameObject.SetActive(false);
        if (modulePlayer.roleInfo.leagueID != 0 && moduleUnion.UnionBaseInfo != null && moduleUnion.BossInfo != null)
        {
            SetMainInfo();
            moduleUnion.GetNewSentiment();
            if (moduleUnion.OpenBossWindow)
            {
                ShowAsync<Window_Unionboss>();
                moduleUnion.OpenBossWindow = false;
            }
        }
        else
        {
            if (modulePlayer.roleInfo.leagueID != 0) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(248, 4));

            //这里没有请求刷新是因为会设置toggle (会发送);
            SetNoGuildStart();
        }
    }

    #region no guild set

    private void SetNoCuildClick()
    {
        m_reconnedBtn.onValueChanged.AddListener(delegate
        {
            if (m_reconnedBtn.isOn)
            {
                moduleUnion.RefreshUnionList();
            }
        });
        m_selectBtn.onValueChanged.AddListener(delegate
       {
           //每次重新进到搜索界面 清空上次搜索列表
           moduleUnion.selectUnionList.Clear();
           m_selectTxt.text = string.Empty;
           m_selectNullPlane.gameObject.SetActive(false);
           m_slectInfo.SetItems(moduleUnion.selectUnionList);
       });
        m_createBtn.onValueChanged.AddListener(delegate
       {
           if (m_createBtn.isOn)
           {
               SetCreateUnion();
           }
       });

        m_fastAddBtn.onClick.AddListener(delegate
        {
            if (moduleUnion.m_lossTimes == 0)
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 60));
            else
                moduleUnion.AddUnion(1);
        });
        m_refreshBtn.onClick.AddListener(delegate
        {
            moduleUnion.RefreshUnionList();
        });

        m_slelctBtn.onClick.AddListener(delegate
        {
            if (m_selectTxt.text == "" || m_selectTxt.text == null)
            {
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 187));
            }
            else
            {
                int type = 0;//0 名字 1 id
                if (r.Match(m_selectTxt.text).Success)
                {
                    type = 1;
                }
                moduleUnion.SelectUnion(m_selectTxt.text, type);
            }
        });

        m_createUnionBtn.onClick.AddListener(delegate
        {
            if (moduleUnion.createUnionInfo == null)
            {
                Logger.LogError("No create info");
                return;
            }
            if (string.IsNullOrEmpty(m_createText.text))
            {
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(221, 6));
                return;
            }
            if (Util.ContainsSensitiveWord(m_createText.text))//是否敏感词
            {
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.NameUIText, 5));
                m_createText.text = string.Empty;
                return;
            }
            var match = Regex.IsMatch(m_createText.text, Window_Name.MATCH_STRING);
            if (!match)
            {
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 15));
                m_createText.text = string.Empty;
                return;
            }
            if (r.Match(m_createText.text).Success)
            {
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 182));
                m_createText.text = string.Empty;
                return;
            }
            if (modulePlayer.roleInfo.level < moduleUnion.createUnionInfo[2].price)
            {
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 131));
                return;
            }
            else if (modulePlayer.roleInfo.coin < moduleUnion.createUnionInfo[0].price|| modulePlayer.roleInfo.diamond < moduleUnion.createUnionInfo[1].price)
            {
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 132));
                return;
            }
            string tips = Util.Format(ConfigText.GetDefalutString(242, 61),m_demandText.text , m_iconText.text , m_createText.text);
            Window_Alert.ShowAlert(tips, true, true, true, () => { moduleUnion.CreateUnion(m_createText.text); }, null, "", "");
        });
    }

    //创建
    private void SetCreateUnion()
    {
        m_createText.text = string.Empty;
        m_expendDemand.gameObject.SetActive(false);
        m_expendIcon.gameObject.SetActive(false);
        m_expendLevel.gameObject.SetActive(false);

        if (moduleUnion.createUnionInfo == null)
        {
            Logger.LogError("No create info");
            return;
        }
        if (moduleUnion.createUnionInfo.Length >= 1)
        {
            if (moduleUnion.createUnionInfo[0] == null) return;
            m_expendDemand.gameObject.SetActive(true);

            Util.SetText(m_demandText, moduleUnion.createUnionInfo[0].price.ToString());
            SetColor(0, m_demandText);

            PropItemInfo info1 = ConfigManager.Get<PropItemInfo>(moduleUnion.createUnionInfo[0].itemTypeId);
            if (info1 == null) return;
            AtlasHelper.SetItemIcon(m_demandImg, info1);
        }
        if (moduleUnion.createUnionInfo.Length >= 2)
        {
            if (moduleUnion.createUnionInfo[1] == null) return;
            m_expendIcon.gameObject.SetActive(true);
            Util.SetText(m_iconText, moduleUnion.createUnionInfo[1].price.ToString());
            SetColor(1, m_iconText);

            PropItemInfo info2 = ConfigManager.Get<PropItemInfo>(moduleUnion.createUnionInfo[1].itemTypeId);
            if (info2 == null) return;
            AtlasHelper.SetItemIcon(m_iconImg, info2);
        }
        if (moduleUnion.createUnionInfo.Length >= 3)
        {
            if (moduleUnion.createUnionInfo[1] == null) return;
            m_expendLevel.gameObject.SetActive(true);
            m_levelText.text = "LV" + moduleUnion.createUnionInfo[2].price.ToString();
            if (modulePlayer.roleInfo.level >= moduleUnion.createUnionInfo[2].price)
            {
                m_levelText.color = GeneralConfigInfo.defaultConfig.CreateUnionEnough;
            }
            else
            {
                m_levelText.color = GeneralConfigInfo.defaultConfig.CreateUnionLess;
            }
        }
    }
    private void SetColor(int index, Text txt)
    {
        if (moduleUnion.createUnionInfo == null || moduleUnion.createUnionInfo.Length < (index + 1)) return;
        if (moduleUnion.createUnionInfo[index].itemTypeId == 1)
        {
            if (modulePlayer.roleInfo.coin >= moduleUnion.createUnionInfo[0].price)
            {
                txt.color = GeneralConfigInfo.defaultConfig.CreateUnionEnough;
            }
            else
            {
                txt.color = GeneralConfigInfo.defaultConfig.CreateUnionLess;
            }
        }
        else if (moduleUnion.createUnionInfo[index].itemTypeId == 2)
        {
            if (modulePlayer.roleInfo.diamond >= moduleUnion.createUnionInfo[1].price)
            {
                txt.color = GeneralConfigInfo.defaultConfig.CreateUnionEnough;
            }
            else
            {
                txt.color = GeneralConfigInfo.defaultConfig.CreateUnionLess;
            }
        }

    }

    //推荐
    private void SetRecomedInfo(RectTransform rt, PRefreshInfo info)
    {
        GuildRefreshInfo refresh = rt.gameObject.GetComponentDefault<GuildRefreshInfo>();
        refresh.SetInfo(info, 0, OpenJionPlane);
    }
    
    //搜索
    private void SetSelectInfo(RectTransform rt, PRefreshInfo info)
    {
        GuildRefreshInfo refresh = rt.gameObject.GetComponentDefault<GuildRefreshInfo>();
        refresh.SetInfo(info, 1, OpenJionPlane);
    }

    private void OpenJionPlane(PRefreshInfo info, int type)
    {
        m_joinPlane.gameObject.SetActive(true);
        if (info.refreshinfo == null)
        {
            Logger.LogError("info is null");
            return;
        }
        m_addName.text = info.refreshinfo.unionname;
        Util.SetText(m_addPrincesName, info.refreshinfo.presidentname);
        Util.SetText(m_addLevel, moduleUnion.SetUnionLevelTxt( info.refreshinfo.level));

        string str = info.refreshinfo.playernum.ToString() + "/" + info.refreshinfo.playertop.ToString();
        Util.SetText(m_addNum,str);

        m_addNotice.text = info.refreshinfo.announcentment;
        if (string.IsNullOrEmpty(info.refreshinfo.announcentment))
        {
            Util.SetText(m_addNotice, ConfigText.GetDefalutString(242, 66));
        }
        var have = Module_Union.instance.ApplyUnionList.Exists(a => a == info.refreshinfo.id);
        if (!have)
        {
            Util.SetText(m_addText, ConfigText.GetDefalutString(242, 67));
            m_addApply.interactable = true;
        }
        else
        {
            Util.SetText(m_addText, ConfigText.GetDefalutString(242, 68));
            m_addApply.interactable = false;
        }
        m_addApply.onClick.RemoveAllListeners();
        m_addApply.onClick.AddListener(delegate
        {
            SendAddClick(info.refreshinfo.id, type);
        });
    }
    
    //发送添加申请
    private void SendAddClick(long unionId, int type)
    {
        if (moduleUnion.m_lossTimes == 0) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 60));
        else
        {
            moduleUnion.AddUnion(0, unionId);
            Util.SetText(m_addText, ConfigText.GetDefalutString(242, 68));
            m_addApply.interactable = false;
            if (type == 0)
            {
                var index = moduleUnion.UpdateInfo(unionId, moduleUnion.m_refrshList);
                if (index == -1) return;
                moduleUnion.m_refrshList[index].state = 1;
                m_refreshInfo.UpdateItem(index);
            }
            else if (type == 1)
            {
                var index = moduleUnion.UpdateInfo(unionId, moduleUnion.selectUnionList);
                if (index == -1) return;
                moduleUnion.selectUnionList[index].state = 1;
                m_slectInfo.UpdateItem(index);
            }
        }
        m_joinPlane.gameObject.SetActive(false);
    }
    #endregion

    #region have guild
    private void SethaveGuildBtn()
    {
        SetToogle();
        m_openGuildBtn.onClick.AddListener(delegate
        {
            moduleUnion.m_inOtherPalne = true;
            ManagerShow();
            if (!m_infoBtn.isOn) m_infoBtn.isOn = true;
            //设置公会动态消息
        });

        m_bossClose.onClick.AddListener(delegate
        {
            Window.ShowAsync("window_unionboss");
        });
        m_bossOpen.onClick.AddListener(delegate
        {
            Window.ShowAsync("window_unionboss");
        });
        m_unionApply.onClick.AddListener(delegate
       {
           moduleUnion.m_inOtherPalne = true;
           SetApplyPlane();
       });

        m_unionManage.onClick.AddListener(delegate
        {
            moduleUnion.m_inOtherPalne = true;
            ManagerShow();
            if (!m_playerBtn.isOn) m_playerBtn.isOn = true;
        });

        m_chatBtn.onClick.AddListener(delegate
        {
            //打开聊天界面
            moduleChat.opChatType = OpenWhichChat.UnionChat;
            ShowAsync("window_chat");
        });
        m_agreeAllBtn.onClick.AddListener(delegate
        {
            long[] ids = moduleUnion.ChangeApplyList();
            moduleUnion.SloveApply(1, ids);
        });
        m_ignoreAllBtn.onClick.AddListener(delegate
        {
            long[] ids = moduleUnion.ChangeApplyList();
            moduleUnion.SloveApply(3, ids);
            moduleUnion.m_unionApply.Clear();
            ApplySloveResult();
        });
        m_joinSetSave.onClick.AddListener(delegate
        {
            if (m_joinSlider.value == moduleUnion.UnionBaseInfo.minlevel && m_automaticSlider.value == moduleUnion.UnionBaseInfo.automaticlevel)
            {
                var state = 1;
                if (m_automaticState.isOn)
                    state = 0;
                if (state == moduleUnion.UnionBaseInfo.automatic)
                {
                    moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 69));
                    return;
                }
            }
            else if (((int)m_automaticSlider.value < (int)m_joinSlider.value) && m_automaticState.isOn)
            {
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 190));
                return;
            }

            //(自动加入 0 开启 1 关闭)
            if (m_automaticState.isOn)
                moduleUnion.SetUnionLevel((int)m_joinSlider.value, 0, (int)m_automaticSlider.value);
            else
                moduleUnion.SetUnionLevel((int)m_joinSlider.value, 1, 0);
        });
        m_joinSetReturn.onClick.AddListener(delegate
        {
            LevelSet(moduleUnion.UnionBaseInfo.minlevel, moduleUnion.UnionBaseInfo.automaticlevel, moduleUnion.UnionBaseInfo.automatic);
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 204));
        });
        m_joinSlider.onValueChanged.AddListener(delegate
        {
            m_joinSetReturn.interactable = SetAmticBtn();
            m_joinLevel.text = m_joinSlider.value.ToString();
        });
        m_automaticSlider.onValueChanged.AddListener(delegate
        {
            m_joinSetReturn.interactable = SetAmticBtn();
            m_automaticLevel.text = m_automaticSlider.value.ToString();
       });
        m_automaticState.onValueChanged.AddListener(delegate
        {
            m_joinSetReturn.interactable = SetAmticBtn();
            if (m_automaticState.isOn)
            {
                m_automaticSlider.value = moduleUnion.UnionBaseInfo.automaticlevel;//重置值
                if (moduleUnion.UnionBaseInfo.automaticlevel <= 0)
                {
                    m_automaticSlider.value = 1;
                    m_automaticLevel.text = m_automaticSlider.value.ToString();
                }

                if (m_automaticSlider.value < moduleUnion.UnionBaseInfo.minlevel)
                    m_automaticSlider.value = moduleUnion.UnionBaseInfo.minlevel;

                m_automaticSlider.gameObject.SetActive(true);
            }
            else
            {
                m_automaticSlider.gameObject.SetActive(false);
            }
        });
        m_quitButton.onClick.AddListener(delegate
        {
            //打开
            if (moduleUnion.m_unionPlayer.Count == 1)
            {
                //解散
                Window_Alert.ShowAlert(ConfigText.GetDefalutString(242, 70), true, true, true, () => { moduleUnion.DissolutionUnion(); }, null, "", "");
            }
            else
            {
                if (moduleUnion.inUnion == 0)
                {
                    Window_Alert.ShowAlert(ConfigText.GetDefalutString(242, 71), true, true, true, null, null, "", "");
                }
                else
                {
                    Window_Alert.ShowAlert(ConfigText.GetDefalutString(242, 70), true, true, true, () => { moduleUnion.ExitUnion(); }, null, "", "");
                }
            }
        });
        m_mangerJoinBtn.onClick.AddListener(delegate
        {
            moduleUnion.m_inOtherPalne = true;
            SetLevelInfo();
        });
        m_mangerChangeNoticeBtn.onClick.AddListener(delegate
        {
            //弹出公告修改框
            m_changeNoticePlane.gameObject.SetActive(true);
            m_changeNotice.text = moduleUnion.UnionBaseInfo.announcentment;
        });

        m_changeNoticeBtn.onClick.AddListener(delegate
        {
            if (string.IsNullOrEmpty(m_changeNotice.text))
            {
                moduleGlobal.ShowMessage(221, 6);
            }
            else
            {
                if (Util.ContainsSensitiveWord(m_changeNotice.text))
                {
                    moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.NameUIText, 5));
                }
                else
                {
                    moduleUnion.ChangeUnionNotice(m_changeNotice.text);//简介更改确定按钮发送给服务器
                }
            }
        });

        m_mangerDissolutionBtn.onClick.AddListener(delegate
        {
            //解散公会的弹窗
            Window_Alert.ShowAlert(ConfigText.GetDefalutString(242, 72), true, true, true, () => { moduleUnion.DissolutionUnion(); }, null, "", "");

        });
        m_managerBtn.onValueChanged.AddListener(delegate
        {
            if (m_managerBtn.isOn)  ManagerBtnShow();
        });
        m_playerBtn.onValueChanged.AddListener(delegate
        {
            if (m_playerBtn.isOn) SetManagerInfo();
        });
        m_infoBtn.onValueChanged.AddListener(delegate
        {
            if (m_infoBtn.isOn) SetBaseInfo();
        });
        m_managerShowApply.onClick.AddListener(delegate
        {
            if (Module.moduleUnion.m_unionApply.Count == 0)
            {
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 73));
            }
            else
            {
                m_applyPlane.gameObject.SetActive(true);
                SetApplyPlane();
            }
        });
    }
    private bool SetAmticBtn()
    {
        if (m_joinSlider.value == moduleUnion.UnionBaseInfo.minlevel && m_automaticSlider.value == moduleUnion.UnionBaseInfo.automaticlevel)
        {
            var state = m_automaticState.isOn ? 0 : 1;
            if (state == moduleUnion.UnionBaseInfo.automatic) return false ;
        }
        return true;
    }


    private void ManagerShow()
    {
        //管理界面是否显示
        if (moduleUnion.inUnion == 0)
        {
            m_managerBtn.gameObject.SetActive(true);
        }
        else if (moduleUnion.inUnion == 1)
        {
            m_managerBtn.gameObject.SetActive(true);
        }
        else
        {
            m_managerBtn.gameObject.SetActive(false);
        }
    }

    //main
    private void SetMainInfo()
    {
        m_haveGuild.gameObject.SetActive(true);
        m_noGuild.gameObject.SetActive(false);
        m_mainPlane.gameObject.SetActive(true);
        m_applyPlane.gameObject.SetActive(false);
        m_guildBasePlane.gameObject.SetActive(false);
        m_joinConditionPlane.gameObject.SetActive(false);

        SetCardSign();
        SetClalimsInfo();
        
        SetCloseTime();

        if (moduleUnion.UnionBaseInfo == null || moduleUnion.BossInfo == null)
        {
            Logger.LogError("have union ,but no info");
            return;
        }
        m_mainName.text = moduleUnion.UnionBaseInfo.unionname;

        Util.SetText(m_mainLevel, moduleUnion.SetUnionLevelTxt(moduleUnion.UnionBaseInfo.level));

        Util.SetText(m_expendSentiment, ConfigText.GetDefalutString(242, 74), moduleUnion.m_expendSentiment.ToString());
        m_notice.text = moduleUnion.UnionBaseInfo.announcentment;
        if (string.IsNullOrEmpty(moduleUnion.UnionBaseInfo.announcentment))
        {
            Util.SetText(m_notice, ConfigText.GetDefalutString(242, 66));
        }
        if (moduleUnion.BossInfo.bossstate == 0)
        {
            m_bossClose.gameObject.SetActive(true);
            m_bossOpen.gameObject.SetActive(false);
        }
        else if (moduleUnion.BossInfo.bossstate == 1)
        {
            m_bossClose.gameObject.SetActive(false);
            m_bossOpen.gameObject.SetActive(true);
        }

        if (moduleUnion.m_unionApply.Count == 0)
        {
            m_unionApply.gameObject.SetActive(false);
        }
        else
        {
            m_unionApply.gameObject.SetActive(true);
            string str1 = moduleUnion.m_unionApply.Count + ConfigText.GetDefalutString(242, 75);
            Util.SetText(m_applyNum, str1);
        }
        m_playerNowTop.text = moduleUnion.UnionBaseInfo.playernum.ToString() + "/" + moduleUnion.UnionBaseInfo.playertop.ToString();

        var leveIndex = moduleUnion.UnionBaseInfo.level + 1;
        if (leveIndex > 5) leveIndex = 5;
        Sentiment sent = ConfigManager.Get<Sentiment>(leveIndex);
        if (sent == null) return;

        SetSentiment(sent.sentimentTop);
        m_senValueBar.fillAmount = (float)moduleUnion.UnionBaseInfo.sentimentnow / (float)sent.sentimentTop;

        ContentSort(m_mainContent);
    }
    private void SetClalimsInfo()
    {
        if (moduleUnion.SelfClaims == null) Util.SetText(m_rewardText, 631, 28);
        else
        {
            var prop = ConfigManager.Get<PropItemInfo>(moduleUnion.SelfClaims.itemTypeId);
            if (prop == null)
            {
                Logger.LogError("XML PropItemInfo not have id {0} info", moduleUnion.SelfClaims.itemTypeId);
                return;
            }
            Util.SetText(m_rewardText, string.Format(ConfigText.GetDefalutString(631, 29), moduleUnion.SelfClaims.receivedNum, prop.rewardnum));
        }
    }
    private void SetCardSign()
    {
        if (moduleUnion.CardSignInfo != null)
            m_cardEffect.SafeSetActive(moduleUnion.CardSignInfo.changeTimes > 0 || (moduleUnion.CardSignInfo.changeTimes == 0 && moduleUnion.CardSignInfo.getTimes > 0));
        else moduleUnion.GetCardSignInfo();
    }

    private int GetOnlinePlayer()
    {
        int onLine = 0;
        //计算在线成员
        for (int i = 0; i < moduleUnion.m_unionPlayer.Count; i++)
        {
            if (moduleUnion.m_unionPlayer[i].info == null) continue;
            if (moduleUnion.m_unionPlayer[i].info.state == 1)
            {
                onLine++;
            }
        }
        return onLine;
    }

    //apply
    private void SetApplyPlane()
    {
        if (moduleUnion.UnionBaseInfo == null)
        {
            Logger.LogError("have union ,but no info");
            return;
        }
        string str = moduleUnion.UnionBaseInfo.playernum + "/" + moduleUnion.UnionBaseInfo.playertop;
        Util.SetText(m_playerHave, ConfigText.GetDefalutString(242, 65), str);
        Util.SetText(m_applyNumAll, ConfigText.GetDefalutString(242, 78), moduleUnion.m_unionApply.Count);

        m_applyInfo.SetItems(moduleUnion.m_unionApply);
    }

    private void CheckCanAddPlayer(long id)//判断是否能同意成员
    {
        if (moduleUnion.UnionBaseInfo.playernum >= moduleUnion.UnionBaseInfo.playertop)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 79));
        }
        else
        {
            long[] ids = new long[1];
            ids[0] = id;
            moduleUnion.SloveApply(0, ids);
        }
    }

    private void SetApplyInfo(RectTransform rt, PPlayerInfo info)
    {
        if (info == null)
        {
            Logger.LogDetail("null");
            return;
        }
        ApplyFriendInfo applyInfo = rt.GetComponentDefault<ApplyFriendInfo>();
        applyInfo.UnionInitItem((long)info.roleId, CheckCanAddPlayer);
        applyInfo.SetInfo(info, 1);
    }
    //level set
    private void SetLevelInfo()
    {
        LevelSet(moduleUnion.UnionBaseInfo.minlevel, moduleUnion.UnionBaseInfo.automaticlevel, moduleUnion.UnionBaseInfo.automatic);
    }

    private void LevelSet(int level, int automicLevel, int isOpen)
    {
        //0 开启 1 关闭
        m_joinLevel.text = level.ToString();
        m_joinSlider.value = level;
        m_automaticSlider.value = automicLevel;

        if (m_automaticSlider.value < moduleUnion.UnionBaseInfo.minlevel)
            m_automaticSlider.value = moduleUnion.UnionBaseInfo.minlevel;

        m_automaticLevel.text = automicLevel.ToString();
        if (isOpen == 0)
        {
            m_automaticState.isOn = true;
            m_automaticSlider.gameObject.SetActive(true);
        }
        else
        {
            m_automaticState.isOn = false;
            m_automaticSlider.gameObject.SetActive(false);
        }
        m_joinSetReturn.interactable = false;
    }

    //guild info
    private void SetBaseInfo()
    {
        if (moduleUnion.UnionBaseInfo == null)
        {
            Logger.LogError("have union ,but no info");
            return;
        }
        m_baseNotice.text = moduleUnion.UnionBaseInfo.announcentment;
        if (string.IsNullOrEmpty(moduleUnion.UnionBaseInfo.announcentment))
        {
            Util.SetText(m_baseNotice, ConfigText.GetDefalutString(242, 66));
        }
        m_baseName.text = moduleUnion.UnionBaseInfo.unionname;
        m_baseID.text = moduleUnion.UnionBaseInfo.id.ToString();
       
        Util.SetText(m_baseLevel, moduleUnion.SetUnionLevelTxt(moduleUnion.UnionBaseInfo.level));

        m_basePresident.text = moduleUnion.UnionBaseInfo.presidentname;
        m_baseNum.text = moduleUnion.UnionBaseInfo.playernum + "/" + moduleUnion.UnionBaseInfo.playertop;
        Util.SetText(m_baseConsyme, ConfigText.GetDefalutString(242, 80), moduleUnion.m_expendSentiment.ToString());

        GetAllStatiment(moduleUnion.UnionBaseInfo.level, moduleUnion.UnionBaseInfo.sentimentnow);

        var leveIndex = moduleUnion.UnionBaseInfo.level + 1;
        if (leveIndex > 5) leveIndex = 5;
        Sentiment sent = ConfigManager.Get<Sentiment>(leveIndex);
        if (sent == null) return;

        SetSentiment(sent.sentimentTop);
        m_baseProgressValue.fillAmount = (float)moduleUnion.UnionBaseInfo.sentimentnow / (float)sent.sentimentTop;
        
        ContentSort(m_baseSysContent);
    }
    //计算当前全部人气值
    private void GetAllStatiment(int level, long value)
    {
        //这里 先计算达到当前需要多少经验 然后加上该等级的当前值
        if (moduleUnion.m_UnionSentiment == null) return;
        long allValue = 0;
        for (int i = 1; i < level; i++)
        {
            allValue += moduleUnion.m_UnionSentiment[i].sentimentTop;
        }
        allValue += value;

        m_baseAllSentiment.text = allValue.ToString();
    }

    //manager player

    private void SetManagerInfo()
    {

        if (moduleUnion.UnionBaseInfo == null)
        {
            Logger.LogError("have union ,but no info");
            return;
        }
        m_unionPlayerInfo.SetItems(moduleUnion.m_unionPlayer);

        string str = moduleUnion.UnionBaseInfo.playernum.ToString() + "/" + moduleUnion.UnionBaseInfo.playertop.ToString();
        Util.SetText(m_playerCurrentNum, ConfigText.GetDefalutString(242, 82), str);
        Util.SetText(m_playerIsOnline, ConfigText.GetDefalutString(242, 83), GetOnlinePlayer().ToString());
        
    }

    private void ManagerPlayerInfo(RectTransform rt, PUnionPlayer info)
    {
        MangerUnionPlayer managerInfo = rt.GetComponentDefault<MangerUnionPlayer>();
        managerInfo.SetPlayerInfo(info);
    }

    private void ManagerPlayerClick(RectTransform rt, PUnionPlayer info)
    {
        //点击自己不出现反应
        if (info.info.roleId != modulePlayer.roleInfo.roleId)
        {
            //出现详情界面//设置详情页的具体信息
            m_detailPlane.gameObject.SetActive(true);
            PlayerDetailInfo detail = m_detailPlane.GetComponentDefault<PlayerDetailInfo>();
            detail.PlayerDetailsInfo(info.info, info.sentiment, info.title);
        }
    }
    // 
    private void ManagerBtnShow()
    {
        if (moduleUnion.inUnion == 0)
        {
            m_mangerJoinBtn.gameObject.SetActive(true);
            m_mangerChangeNoticeBtn.gameObject.SetActive(true);
            m_mangerDissolutionBtn.gameObject.SetActive(true);
            m_managerShowApply.gameObject.SetActive(true);
        }
        else if (moduleUnion.inUnion == 1)
        {
            m_mangerJoinBtn.gameObject.SetActive(true);
            m_mangerChangeNoticeBtn.gameObject.SetActive(true);
            m_mangerDissolutionBtn.gameObject.SetActive(false);
            m_managerShowApply.gameObject.SetActive(true);
        }
        else
        {
            m_mangerJoinBtn.gameObject.SetActive(false);
            m_mangerChangeNoticeBtn.gameObject.SetActive(false);
            m_mangerDissolutionBtn.gameObject.SetActive(false);
            m_managerShowApply.gameObject.SetActive(false);
        }
    }
    #endregion

    #region _ME

    void _ME(ModuleEvent<Module_Union> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Union.EventUnionClamisSelf:
                SetClalimsInfo();
                break;
            case Module_Union.EventUnionCardSign:
                m_cardEffect.SafeSetActive(moduleUnion.CardSignInfo.changeTimes > 0 || (moduleUnion.CardSignInfo.changeTimes == 0 && moduleUnion.CardSignInfo.getTimes > 0));
                break;

            case Module_Union.EventUnionBossClose:
                SetCloseTime();
                break;
            case Module_Union.EventUnionBossOpen:
                SetCloseTime();
                break;

            case Module_Union.EventUnionRefreshList:
                //刷新公会列表
                moduleUnion.m_inOtherPalne = false;
                RefreshResult();
                break;
            case Module_Union.EventUnionSelectResult:
                //搜索公会结果
                int selectResult = Util.Parse<int>(e.param1.ToString());
                SelectResult(selectResult);
                break;
            case Module_Union.EventUnionCreateSucced:
                //创建公会结果
                UnionCreateResult();
                break;
            case Module_Union.EventUnionAddSucced:
                //成功加入公会
                m_joinPlane.gameObject.SetActive(false);
                m_noGuild.gameObject.SetActive(false);
                m_haveGuild.gameObject.SetActive(true);
                SetMainInfo();
                break;
            case Module_Union.EventUnionApplyNotice:
                //有公会申请
                m_unionApply.gameObject.SetActive(true);
                string str = moduleUnion.m_unionApply.Count.ToString() + ConfigText.GetDefalutString(242, 75);
                Util.SetText(m_applyNum, str);
                m_applyInfo.SetItems(moduleUnion.m_unionApply);
                break;
            case Module_Union.EventUnionApplySolveResult:
                //申请处理结果
                ApplySloveResult();
                break;
            case Module_Union.EventUnionApplyOverdue:
                //申请过期
                var applyId = Util.Parse<ulong>(e.param1.ToString());
                var index = UpdateApplyInfo(applyId, moduleUnion.m_unionApply);
                if (index == -1)
                {
                    Logger.LogError("this apply is null ");
                }
                m_applyInfo.RemoveItem(index);
                break;
            case Module_Union.EventUnionRemoveSucced:
                //踢出成员成功
                m_detailPlane.gameObject.SetActive(false);
                break;
            case Module_Union.EventUnionTitleBeChange:
                //被更改职位(所有界面都要进行更改相应的)
                var myTitle = Util.Parse<int>(e.param1.ToString());
                var playerID = Util.Parse<ulong>(e.param2.ToString());
                SelfTitleBeChange(myTitle, playerID);
                break;
            case Module_Union.EventUnionPlayerAdd:
                //有人加入了公会（所有有关人数的地方都进行更改）
                SetAllPlayerNumInfo();
                break;
            case Module_Union.EventUnionPlayerExit:
                //有人离开了公会
                SetAllPlayerNumInfo();
                break;
            case Module_Union.EventUnionNoticeChange:
                //公告（被修改或者修改成功）
                SetAllNotice();
                break;
            case Module_Union.EventUnionPlayerState:
                //有人状态被修改
                SetPlayerLineChange();
                break;
            case Module_Union.EventUnionLevelChange:
                //公会等级更改
                SetLevelChange();
                break;
            case Module_Union.EventUnionSelfExit:
                //自己被踢出或者自己退出
                var type = Util.Parse<int>(e.param1.ToString());
                var pos = Util.Parse<int>(e.param2.ToString());

                if (type != 0 || pos == 0) SetNoGuildStart();
                else if (actived)
                {
                    Window_Alert.ShowAlertDefalut(ConfigText.GetDefalutString(242, 210),
                        () => { SetNoGuildStart(); }, () => { SetNoGuildStart(); });
                }
                break;
            case Module_Union.EventUnionSentimntExpend:
                //人气值消耗
                SetLevelChange();
                break;
            case Module_Union.EventUnionSetLevel:
                m_joinConditionPlane.gameObject.SetActive(false);
                m_joinSetReturn.interactable = false;
                break;

            case Module_Union.EventUnionSysUpdate:
                var str1 = e.param1.ToString();
                SystemUpdate(m_mainObj, m_mainContent, m_dynamicMainObj, str1);//主界面
                SystemUpdate(m_baseObj, m_baseSysContent, m_dynamicBaseObj, str1);//base 界面
                break;
            case Module_Union.EventUnionSentimentUpdate:
                SetSentimentUpdate();
                break;
            case Module_Union.EventUnionDissolution:
                //公会解散
                moduleUnion.m_inOtherPalne = false;
                SetNoGuildStart();
                break;
            case Module_Union.EventUnionAddFailed:
                var indexs = Util.Parse<int>(e.param2.ToString());
                var indexss = Util.Parse<int>(e.param3.ToString());
                m_addApply.interactable = true;

                if (indexs != -1) m_refreshInfo.UpdateItem(indexs);
                if (indexss != -1) m_slectInfo.UpdateItem(indexss);
                break;
        }
    }

    void _ME(ModuleEvent<Module_Player > e)
    {
        if (!actived) return;
        if (e.moduleEvent == Module_Player.EventCurrencyChanged)
            SetCreateUnion();
    }
    void _ME(ModuleEvent<Module_Friend> e)
    {
        if (!actived) return;

        if (e.moduleEvent == Module_Friend.EventFriendAddApply)
        {
            var result = Util.Parse<int>(e.param1.ToString());
            if (result != 0) m_detailAdd.SetInteractable(true);
        }
        else if (e.moduleEvent == Module_Friend.EventFriendAddBlack || e.moduleEvent == Module_Friend.EventFriendRemoveBlack)
        {
            PlayerDetailInfo info = m_detailPlane.GetComponentDefault<PlayerDetailInfo>();
            info.SetBlackShow();
        }
    }

    #endregion

    #region  event union result

    private void SetSentimentUpdate()
    {
        var leveIndex = moduleUnion.UnionBaseInfo.level + 1;
        if (leveIndex > 5) leveIndex = 5;
        Sentiment sent = ConfigManager.Get<Sentiment>(leveIndex);
        if (sent == null) return;
        SetSentiment(sent.sentimentTop);
        m_baseProgressValue.fillAmount = (float)moduleUnion.UnionBaseInfo.sentimentnow / (float)sent.sentimentTop;
        m_senValueBar.fillAmount = (float)moduleUnion.UnionBaseInfo.sentimentnow / (float)sent.sentimentTop;

    }

    private void SetNoGuildStart()
    {
        if (m_reconnedBtn.isOn == false)
        {
            m_reconnedBtn.isOn = true;//设为true 会直接发
            m_selectBtn.isOn = false;
            m_createBtn.isOn = false;
        }
        else
        {
            moduleUnion.RefreshUnionList();
        }
        m_haveGuild.gameObject.SetActive(false);
        m_noGuild.gameObject.SetActive(true);
        m_recommendPlane.gameObject.SetActive(true);
        m_searchPlane.gameObject.SetActive(false);
        m_createPlane.gameObject.SetActive(false);

        //清空以前消息的预制
        int mainLength = m_dynamicMainObj.Count;
        int baseLength = m_dynamicBaseObj.Count;
        for (int i = 0; i < mainLength; i++)
        {
            GameObject.Destroy(m_dynamicMainObj.Dequeue());
        }
        for (int i = 0; i < baseLength; i++)
        {
            GameObject.Destroy(m_dynamicBaseObj.Dequeue());
        }
    }

    private void RefreshResult()
    {
        if (moduleUnion.m_refrshList.Count == 0)
        {
            m_randomnNullList.gameObject.SetActive(true);
            m_randomList.gameObject.SetActive(false);
        }
        else
        {
            m_randomnNullList.gameObject.SetActive(false);
            m_randomList.gameObject.SetActive(true);
            m_refreshInfo.SetItems(moduleUnion.m_refrshList);
        }
    }

    private void SelectResult(int result)
    {
        m_selectListPlane.gameObject.SetActive(false);
        m_selectNullPlane.gameObject.SetActive(false);
        if (result == 0)
        {
            m_selectListPlane.gameObject.SetActive(true);
            m_slectInfo.SetItems(moduleUnion.selectUnionList);
        }
        else if (result == 1)
        {
            m_selectNullPlane.gameObject.SetActive(true);
        }
        else moduleGlobal.ShowMessage("error");
        
    }

    private void UnionCreateResult()
    {
        //变为拥有公会状态
        //设置公会信息
        SetMainInfo();

    }

    private void ApplySloveResult()
    {
        //0 单个成功 1 批量成功 num (成功的数量)
        SetApplyPlane();
        if (moduleUnion.m_unionApply.Count == 0)
        {
            m_unionApply.gameObject.SetActive(false);
            m_applyPlane.gameObject.SetActive(false);

            moduleUnion.m_inOtherPalne = false;
        }
        else
        {
            m_unionApply.gameObject.SetActive(true);
            m_applyPlane.gameObject.SetActive(true);
        }
        string str1 = moduleUnion.m_unionApply.Count + ConfigText.GetDefalutString(242, 75);
        Util.SetText(m_applyNum, str1);
    }

    private int UpdateApplyInfo(ulong applyId, List<PPlayerInfo> listInfo)
    {
        int index = -1;
        for (int i = 0; i < listInfo.Count; i++)
        {
            if (listInfo[i] == null) continue;
            if (applyId == listInfo[i].roleId)
            {
                index = i;
            }
        }
        return index;
    }

    private void TitleChangeSucced(ulong playerId, int type)
    {
        var index = UpdatePlayerInfo(playerId, moduleUnion.m_unionPlayer);
        if (index == -1)
        {
            Logger.LogError("this player is null ");
        }
        m_unionPlayerInfo.UpdateItem(index);
    }

    private int UpdatePlayerInfo(ulong playerId, List<PUnionPlayer> listInfo)
    {
        int index = -1;
        for (int i = 0; i < listInfo.Count; i++)
        {
            if (listInfo[i].info == null) continue;
            if (playerId == listInfo[i].info.roleId)
            {
                index = i;
            }
        }
        return index;
    }

    private void SelfTitleBeChange(int title, ulong playerId)
    {
        if (playerId == modulePlayer.roleInfo.roleId)
        {
            m_mangerDissolutionBtn.gameObject.SetActive(false);
            if (moduleUnion.m_unionApply.Count > 0)
            {
                m_unionApply.gameObject.SetActive(true);
            }
            if (title == 0)
            {
                m_basePresident.text = moduleUnion.UnionBaseInfo.presidentname;
                m_mangerDissolutionBtn.gameObject.SetActive(true);
            }
            else if (title == 2)
            {
                m_unionApply.gameObject.SetActive(false);
                m_managerBtn.gameObject.SetActive(false);
            }
        }
        PlayerDetailInfo info = m_detailPlane.GetComponentDefault<PlayerDetailInfo>();
        info.SetTitleNormal(title);
        var index = UpdatePlayerInfo(playerId, moduleUnion.m_unionPlayer);
        if (index == -1) return;
        m_unionPlayerInfo.UpdateItem(index);
    }

    private void SetAllPlayerNumInfo()//有人加入 有人离开
    {
        if (moduleUnion.UnionBaseInfo == null)
        {
            Logger.LogError("have union ,but no info");
            return;
        }
        //人数 以及刷新成员列表
        m_unionPlayerInfo.SetItems(moduleUnion.m_unionPlayer);
        m_playerNowTop.text = moduleUnion.UnionBaseInfo.playernum.ToString() + "/" + moduleUnion.UnionBaseInfo.playertop.ToString();

        var leveIndex = moduleUnion.UnionBaseInfo.level + 1;
        if (leveIndex > 5) leveIndex = 5;
        Sentiment sent = ConfigManager.Get<Sentiment>(leveIndex);
        if (sent == null) return;

        string str = moduleUnion.UnionBaseInfo.playernum + "/" + moduleUnion.UnionBaseInfo.playertop;
        Util.SetText(m_playerHave, ConfigText.GetDefalutString(242, 65), str);
        string str1 = moduleUnion.UnionBaseInfo.playernum.ToString() + "/" + moduleUnion.UnionBaseInfo.playertop.ToString();
        Util.SetText(m_playerCurrentNum, ConfigText.GetDefalutString(242, 82), str1);
        m_baseNum.text = moduleUnion.UnionBaseInfo.playernum.ToString() + "/" + moduleUnion.UnionBaseInfo.playertop.ToString();
    }

    private void SetAllNotice()
    {
        m_changeNoticePlane.gameObject.SetActive(false);
        m_changeNotice.text = string.Empty;
        m_notice.text = moduleUnion.UnionBaseInfo.announcentment;
        m_baseNotice.text = moduleUnion.UnionBaseInfo.announcentment;
    }

    private void SetPlayerLineChange()//成员上下线
    {
        if (moduleUnion.UnionBaseInfo == null)
        {
            Logger.LogError("baseinfo is null");
            return;
        } 
        //成员上下线(刷新成员 并更改主界面人数 详细页面的在线状况)
        m_unionPlayerInfo.SetItems(moduleUnion.m_unionPlayer);
        m_playerNowTop.text = moduleUnion.UnionBaseInfo.playernum.ToString() + "/" + moduleUnion.UnionBaseInfo.playertop.ToString();
        Util.SetText(m_playerIsOnline, ConfigText.GetDefalutString(242, 83), GetOnlinePlayer().ToString());

    }

    private void SetLevelChange()//公会等级
    {
        if (moduleUnion.UnionBaseInfo == null) return;
        var leveIndex = moduleUnion.UnionBaseInfo.level + 1;
        if (leveIndex > 5) leveIndex = 5;
        Sentiment sent = ConfigManager.Get<Sentiment>(leveIndex);
        if (sent == null) return;
        
        Util.SetText(m_mainLevel, moduleUnion.SetUnionLevelTxt(moduleUnion.UnionBaseInfo.level));

        SetSentiment(sent.sentimentTop);

        GetAllStatiment(moduleUnion.UnionBaseInfo.level, moduleUnion.UnionBaseInfo.sentimentnow);
        m_baseProgressValue.fillAmount = (float)moduleUnion.UnionBaseInfo.sentimentnow / (float)sent.sentimentTop;
        m_senValueBar.fillAmount = (float)moduleUnion.UnionBaseInfo.sentimentnow / (float)sent.sentimentTop;

    }

    #endregion

    private void SetSentiment(int sentTop)
    {
        string str = moduleUnion.UnionBaseInfo.sentimentnow + "/" + sentTop;
        Util.SetText(m_mainSentiment, ConfigText.GetDefalutString(242, 77), str);
        GetAllStatiment(moduleUnion.UnionBaseInfo.level, moduleUnion.UnionBaseInfo.sentimentnow);

        string str1 = moduleUnion.UnionBaseInfo.sentimentnow + "/" + sentTop;
        Util.SetText(m_baseProgress, ConfigText.GetDefalutString(242, 81), str1);
        if (moduleUnion.UnionBaseInfo.level >= 5)
        {
            Util.SetText(m_baseProgress, ConfigText.GetDefalutString(242, 81), moduleUnion.UnionBaseInfo.sentimentnow.ToString());
            Util.SetText(m_mainSentiment, ConfigText.GetDefalutString(242, 77), moduleUnion.UnionBaseInfo.sentimentnow.ToString());
        }
    }

    #region 公会信息更改

    private void SystemUpdate(GameObject childObj, GameObject parentObj, Queue<GameObject> objList, string str)
    {
        if (objList.Count > 200)
        {
            var a = objList.Dequeue();
            if (a != null) GameObject.Destroy(a);
        }
        CloneSystem(childObj, parentObj, objList, str);
        ContentSort(parentObj);
    }
    
    private void CloneSystem(GameObject childObj, GameObject parentObj, Queue<GameObject> objList, string str)
    {
        GameObject sysObj = GameObject.Instantiate(childObj);
        sysObj.gameObject.SetActive(true);
        sysObj.transform.SetParent(parentObj.transform);
        sysObj.transform.localScale = new Vector3(1, 1, 1);
        sysObj.transform.localPosition = Vector3.zero;
        objList.Enqueue(sysObj);
        SetSysInfo(sysObj, str);
    }

    private void SetSysInfo(GameObject sysObj, string str)
    {
        string sys_mes = "\u3000" + "\u3000" + str;
        GameObject txtobj = sysObj.transform.Find("mes").gameObject;
        txtobj.GetComponentDefault<Chathyperlink>().gettxt = sys_mes;
        Chathyperlink Pic = txtobj.GetComponentDefault<Chathyperlink>();

        Pic.gameObject.SetActive(true);

        Pic.text = sys_mes;
        Pic.Set();

        Pic.text = Pic.gettxt;
        float width = Pic.preferredWidth;
        float height = Pic.preferredHeight;

        RectTransform sysobj_height = sysObj.GetComponent<RectTransform>();
        if (width <= 380f)
        {
            sysobj_height.sizeDelta = new Vector2(385f, 33f);//设置背景的宽高
        }
        else
        {
            ContentSizeFitter a = Pic.gameObject.GetComponent<ContentSizeFitter>();
            a.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            sysobj_height.sizeDelta = new Vector2(385f, height + 8f);//设置背景的宽高
        }
    }
    
    #endregion
    
    protected override void OnReturn()
    {
        if (moduleUnion.m_inOtherPalne)
        {
            SetToogle();
            SetMainInfo();
            moduleUnion.m_inOtherPalne = false;
        }
        else base.OnReturn();
    }

    private void SetToogle()
    {
        m_infoBtn.isOn = false;
        m_playerBtn.isOn = false;
        m_managerBtn.isOn = false;
    }

    public override void OnRenderUpdate()
    {
        base.OnRenderUpdate();
        if (m_closeEnd)
        {
            m_closeTime += Time.unscaledDeltaTime;
            if (m_closeTime > 1)
            {
                m_closeTime = 0;
                m_closeTimeNow--;
                SetTime(m_closeTimeNow, m_bossOpenTime);
                if (m_closeTimeNow == 0)
                {
                    m_closeEnd = false;
                    m_bossOpen.gameObject.SetActive(false);
                    m_bossClose.gameObject.SetActive(true);
                }
            }
        }
    }

    private void SetTime(int timeNow, Text thisText)
    {
        var strTime = Util.GetTimeMarkedFromSec(timeNow);
        Util.SetText(thisText, strTime);
    }

    private void ContentSort(GameObject content)//是否拉到最低处
    {
        var scroll = content.transform.parent.parent.GetComponent<ScrollRect>();
        if (scroll != null) scroll.verticalNormalizedPosition = 0;//每次我发消息或者切换时候调用最下
    }
}
