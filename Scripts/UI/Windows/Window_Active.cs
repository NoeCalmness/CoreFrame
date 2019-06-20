/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-01-17
 * 
 ***************************************************************************************************/

using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Window_Active : Window
{
    /// <summary>
    /// Called when window added to level
    /// </summary>
    /// 
    private Toggle dialy_btn;//日常按钮
    private Toggle achieve_btn;//成就按钮
    private RectTransform dialy_plane;
    private RectTransform achieve_plane;
    private RectTransform award_plane;

    private Text daily_active;
    private Text Weekly_active;
    private Text Week_activenum1;
    private Text Week_activenum2;

    private Button wureward_no;
    private Button wureward_yes;
    private Image wureward_alreadly;
    private Button qianreward_no;
    private Button qianreward_yes;
    private Image qianreward_alreadly;

    private Slider acctive_progress;
    private RectTransform state_group;

    //奖励界面
    private GameObject rewardGroup;

    private Text my_active;
    private Text should_active;
    private Button receive_btn;
    private Button close_btn;
    private GameObject Nothing;

    private Transform m_dailyHint;
    private Transform m_achieveHint;

    private Dictionary<int, GameObject> state = new Dictionary<int, GameObject>();//state里面的所有
    private List<GameObject> ShowBox = new List<GameObject>();//展示盒子里奖励的预制;

    private ConfigText active_text;
    private ConfigText Border_text;

    private DataSource<PDailyInfo> DailyAllInfo;
    private DataSource<PAchieve> AchieveAllInfo;
    private DataSource<PCooperateInfo> CoopAllInfo;
    private ScrollView m_coopView;
    private ScrollView m_dailyView;
    private ScrollView m_achieveView;

    #region 协作
    private Toggle m_coopBtn;
    private Transform m_coopPlane;
    private Text m_remainText;
    private Transform m_coopHint;
    private Transform m_monShowPlane;
    private Transform m_invatePlane;
    private Button m_invateBtn;
    private Text m_monDesc;
    private Transform m_monIcon;
    private ScrollView m_monsterView;
    private Image m_noApper;
    private Text m_monsterName;
    private DataSource<DropInfo> monsterData;
    private ScrollView m_invateView;
    private DataSource<PPlayerInfo> invateData;
    private RectTransform m_coopNothing;

    private float m_times = 0;
    private bool m_openUpdate;
    #endregion

    protected override void OnOpen()
    {
        active_text = ConfigManager.Get<ConfigText>((int)TextForMatType.ActiveUIText);
        Border_text = ConfigManager.Get<ConfigText>((int)TextForMatType.BorderlandUIText);
        if (active_text == null)
        {
            active_text = ConfigText.emptey;
            Logger.LogError("this id can not");
        }
        if (Border_text == null)
        {
            Border_text = ConfigText.emptey;
            Logger.LogError("this id can not");
        }

        SetText();

        #region path

        m_dailyHint = GetComponent<RectTransform>("checkbox/weekly/mark");
        m_achieveHint = GetComponent<RectTransform>("checkbox/achievement/mark");

        Nothing = GetComponent<RectTransform>("dailymission/nothing").gameObject;
        dialy_btn = GetComponent<Toggle>("checkbox/weekly");
        achieve_btn = GetComponent<Toggle>("checkbox/achievement");
        dialy_plane = GetComponent<RectTransform>("dailymission");
        achieve_plane = GetComponent<RectTransform>("achieve_panel");
        award_plane = GetComponent<RectTransform>("preview_panel");

        Week_activenum1 = GetComponent<Text>("dailymission/reward_wubai/reward_num1");
        Week_activenum2 = GetComponent<Text>("dailymission/reward_yiqian/reward_num2");
        daily_active = GetComponent<Text>("dailymission/daily_number/dailynumber_text");
        Weekly_active = GetComponent<Text>("dailymission/weekly_number/dailynumber_text");
        wureward_no = GetComponent<Button>("dailymission/reward_wubai/reward_no");
        wureward_no.onClick.AddListener(delegate
        {
            Award_show(2, moduleActive.WeekActiveValue[0].id);
        });
        wureward_yes = GetComponent<Button>("dailymission/reward_wubai/reward_yes");
        wureward_yes.onClick.AddListener(delegate
        {
            Award_show(2, moduleActive.WeekActiveValue[0].id);
        });
        wureward_alreadly = GetComponent<Image>("dailymission/reward_wubai/reward_already");
        qianreward_no = GetComponent<Button>("dailymission/reward_yiqian/reward_no");
        qianreward_no.onClick.AddListener(delegate
        {
            Award_show(2, moduleActive.WeekActiveValue[1].id);
        });
        qianreward_yes = GetComponent<Button>("dailymission/reward_yiqian/reward_yes");
        qianreward_yes.onClick.AddListener(delegate
        {
            Award_show(2, moduleActive.WeekActiveValue[1].id);
        });
        qianreward_alreadly = GetComponent<Image>("dailymission/reward_yiqian/reward_already");

        acctive_progress = GetComponent<Slider>("dailymission/dailyreward_panel/progress_slider");
        state_group = GetComponent<RectTransform>("dailymission/dailyreward_panel/state_group");

        rewardGroup = GetComponent<RectTransform>("preview_panel/rewardgroup").gameObject;

        my_active = GetComponent<Text>("preview_panel/have_num/current_text");
        should_active = GetComponent<Text>("preview_panel/have_num/max_text");
        receive_btn = GetComponent<Button>("preview_panel/get_button");
        close_btn = GetComponent<Button>("preview_panel/bg/equip_prop/top/button");

        dialy_btn.onValueChanged.AddListener(delegate
        {
            if (dialy_btn.isOn)
            {
                moduleActive.ActiveClick = 0;
                moduleActive.ShowDialyHint = false;
                m_dailyHint.gameObject.SetActive(false);
                m_dailyView.progress = 0;
                dialy_plane.gameObject.SetActive(true);
                achieve_plane.gameObject.SetActive(false);
                m_coopPlane.gameObject.SetActive(false);
            }
        });
        achieve_btn.onValueChanged.AddListener(delegate
        {
            if (achieve_btn.isOn)
            {
                moduleActive.ActiveClick = 1;
                moduleActive.ShowAchieveHint = false;
                m_achieveHint.gameObject.SetActive(false);
                m_achieveView.progress = 0;
                dialy_plane.gameObject.SetActive(false);
                achieve_plane.gameObject.SetActive(true);
                m_coopPlane.gameObject.SetActive(false);
            }
        });
        close_btn.onClick.AddListener(delegate
        {
            //award_plane.gameObject.SetActive(false);
        });
        #endregion

        #region 协作
        m_coopNothing = GetComponent<RectTransform>("assist_panel/invite_panel/person_frame/nothing");
        m_coopBtn = GetComponent<Toggle>("checkbox/assist");
        m_coopPlane = GetComponent<RectTransform>("assist_panel");
        m_remainText = GetComponent<Text>("assist_panel/time/time_txt");
        m_coopHint = GetComponent<Transform>("checkbox/assist/mark");
        m_monShowPlane = GetComponent<RectTransform>("assist_panel/monsterStage");
        m_invatePlane = GetComponent<RectTransform>("assist_panel/invite_panel");
        m_monIcon = GetComponent<RectTransform>("assist_panel/monsterStage/background/bg/boss");
        m_monDesc = GetComponent<Text>("assist_panel/monsterStage/background/bg/descPlane/bg/Text");
        m_monsterName = GetComponent<Text>("assist_panel/monsterStage/background/bg/bg1/Text (1)");
        m_noApper = GetComponent<Image>("assist_panel/monsterStage/background/bg/noappear");
        m_monsterView = GetComponent<ScrollView>("assist_panel/monsterStage/background/bg/havesources_Panel2");
        m_invateView = GetComponent<ScrollView>("assist_panel/invite_panel/person_frame/scrollView");
        m_invateBtn = GetComponent<Button>("assist_panel/invite_panel/person_frame/invite_btn");
        m_coopBtn.onValueChanged.AddListener(delegate
        {
            if (m_coopBtn.isOn)
            {
                moduleActive.ActiveClick = 2;
                moduleActive.ShowCoopHint = false;
                m_coopHint.gameObject.SetActive(false);
                m_coopPlane.gameObject.SetActive(true);
                dialy_plane.gameObject.SetActive(false);
                achieve_plane.gameObject.SetActive(false);
            }
        });
        #endregion

        m_dailyView = GetComponent<ScrollView>("dailymission/scrollView");
        m_achieveView = GetComponent<ScrollView>("achieve_panel/scrollView");
        m_coopView = GetComponent<ScrollView>("assist_panel/scrollView");
        DailyAllInfo = new DataSource<PDailyInfo>(moduleActive.DailyOpenList, m_dailyView, DailyInfoSet);
        AchieveAllInfo = new DataSource<PAchieve>(moduleActive.Achieveinfo, m_achieveView, AchieveInfoSet);
        CoopAllInfo = new DataSource<PCooperateInfo>(moduleActive.CoopTaskList, m_coopView, SetCoopInfo);

        monsterData = new DataSource<DropInfo>(null, m_monsterView, SetShowInfo, SetShowClick);
        invateData = new DataSource<PPlayerInfo>(null, m_invateView, SetInvateInfo, SetInvateClick);

        SetToggleHide(achieve_btn);
        SetToggleHide(dialy_btn);
        SetToggleHide(m_coopBtn);

        GetChildObj();
        Getallstate();
        Weekactive();
    }

    private void SetText()
    {
        Util.SetText(GetComponent<Text>("assist_panel/invite_panel/person_frame/nothing/Text"), active_text[61]);
        Util.SetText(GetComponent<Text>("assist_panel/monsterStage/background/bg/noappear/Text"), active_text[60]);
        Util.SetText(GetComponent<Text>("assist_panel/time/Text"), active_text[31]);
        Util.SetText(GetComponent<Text>("assist_panel/monsterStage/top/equipinfo"), active_text[32]);
        Util.SetText(GetComponent<Text>("assist_panel/invite_panel/person_frame/titleBg/title_Txt"), active_text[33]);
        Util.SetText(GetComponent<Text>("assist_panel/invite_panel/person_frame/invite_btn/Image"), active_text[34]);
        Util.SetText(GetComponent<Text>("checkbox/assist/assist_text"), active_text[35]);
        Util.SetText(GetComponent<Text>("checkbox/assist/check/assist_text1check"), active_text[35]);

        Util.SetText(GetComponent<Text>("checkbox/weekly/weekly_text"), active_text[0]);
        Util.SetText(GetComponent<Text>("checkbox/achievement/daily_text"), active_text[1]);
        Util.SetText(GetComponent<Text>("dailymission/daily_number/daily_text"), active_text[2]);
        Util.SetText(GetComponent<Text>("dailymission/weekly_number/daily_text"), active_text[3]);
        Util.SetText(GetComponent<Text>("dailymission/reward_wubai/reward_text"), active_text[4]);
        Util.SetText(GetComponent<Text>("dailymission/reward_yiqian/reward_text"), active_text[4]);
        Util.SetText(GetComponent<Text>("preview_panel/bg/equip_prop/top/equipinfo"), active_text[5]);
        Util.SetText(GetComponent<Text>("preview_panel/get_button/get"), active_text[6]);
    }

    private void GetChildObj()//获取预制
    {
        ShowBox.Clear();
        foreach (Transform item in rewardGroup.transform)
        {
            ShowBox.Add(item.gameObject);
        }
        dialy_btn.isOn = false;
        achieve_btn.isOn = false;
        m_coopBtn.isOn = false;
        m_achieveHint.gameObject.SetActive(false);
        m_dailyHint.gameObject.SetActive(false);
        m_coopHint.gameObject.SetActive(false);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        award_plane.gameObject.SetActive(false);//每次打开宝箱界面要处于关闭状态

        moduleGlobal.ShowGlobalLayerDefault();
        SetHint();
        NoDailyTask();

        if (m_subTypeLock != -1) moduleActive.ActiveClick = m_subTypeLock;

        CoopAllInfo.SetItems(moduleActive.CoopTaskList);
        m_remainText.transform.parent.SafeSetActive(moduleActive.CoopTaskList.Count > 0);
        dialy_btn.isOn = moduleActive.ActiveClick == 0 ? true : false;
        achieve_btn.isOn = moduleActive.ActiveClick == 1 ? true : false;
        m_coopBtn.isOn = moduleActive.ActiveClick == 2 ? true : false;
        SetRemainTime();
    }

    #region 活跃度 


    private void Weekactive()
    {
        //是固定的值
        if (moduleActive.WeekActiveValue.Count > 1)
        {
            Week_activenum1.text = moduleActive.WeekActiveValue[0].activePoint.ToString();
            Week_activenum2.text = moduleActive.WeekActiveValue[1].activePoint.ToString();
            WhichShua(moduleActive.WeekActiveValue[0].id);
            WhichShua(moduleActive.WeekActiveValue[1].id);
            ActiveNum();
        }
    }

    private void ActiveNum()
    {
        daily_active.text = modulePlayer.roleInfo.dayActivePoint.ToString();
        Weekly_active.text = modulePlayer.roleInfo.weekActivePoint.ToString();
        if (moduleActive.ActiveValue.Count > 4)
        {
            float max = moduleActive.ActiveValue[4].activePoint;
            if ((float)modulePlayer.roleInfo.dayActivePoint > max)
            {
                acctive_progress.value = 1.0f;
            }
            else
            {
                acctive_progress.value = (float)modulePlayer.roleInfo.dayActivePoint / max;
            }
        }
    }//活跃度值  在完成日常任务和刚进去时候调用

    private void Getallstate()
    {
        state.Clear();
        //获取所有子对象
        int i = 0;
        foreach (Transform item in state_group.transform)
        {
            state.Add(moduleActive.ActiveValue[i].id, item.gameObject);
            i++;
        }
        Activetask();
    }
    private void Activetask()
    {
        //活跃度奖励 领取刷新
        for (int i = 0; i < state.Count; i++)
        {
            //这里设置活跃度配置表的东西
            ActiveShua(moduleActive.ActiveValue[i]);//填服务器发来的状态
        }
    }
    private void ActiveShua(PActiveBox boxinfo)
    {
        if (boxinfo == null) return;
        GameObject stateobj = state[boxinfo.id];
        if (stateobj == null)
        {
            Logger.LogError("can not find award state obj");
            return;
        }
        ActiveInfo active = stateobj.GetComponentDefault<ActiveInfo>();
        active.Click(boxinfo.id, Award_show);
        active.Show(moduleActive.Activestae[boxinfo.id], boxinfo.activePoint);
    }

    private void WhichShua(int Iid)
    {
        if (moduleActive.WeekActiveValue.Count >= 2)
        {
            if (Iid == moduleActive.WeekActiveValue[0].id)//第一个
            {
                WeekWuAwardGet(moduleActive.Activestae[Iid]);
            }
            else if (Iid == moduleActive.WeekActiveValue[1].id)//第二个
            {
                WeekQianAwardGet(moduleActive.Activestae[Iid]);
            }
        }
    }

    private void WeekWuAwardGet(EnumActiveState state)
    {
        wureward_no.enabled = true;
        wureward_no.gameObject.SetActive(false);
        wureward_yes.gameObject.SetActive(false);
        wureward_alreadly.gameObject.SetActive(false);
        //0 不可领取 1 可领取 2 已经领取过了 
        if (state == EnumActiveState.NotPick)
        {
            wureward_no.gameObject.SetActive(true);
        }
        else if (state == EnumActiveState.CanPick)
        {
            wureward_no.gameObject.SetActive(true);
            wureward_yes.gameObject.SetActive(true);
        }
        else if (state == EnumActiveState.AlreadPick)
        {
            wureward_no.enabled = false;
            wureward_alreadly.gameObject.SetActive(true);
        }
    }

    private void WeekQianAwardGet(EnumActiveState state)
    {
        qianreward_no.enabled = true;
        qianreward_no.gameObject.SetActive(false);
        qianreward_yes.gameObject.SetActive(false);
        qianreward_alreadly.gameObject.SetActive(false);
        //0 不可领取 1 可领取 2 已经领取过了 
        if (state == EnumActiveState.NotPick)
        {
            qianreward_no.gameObject.SetActive(true);
        }
        else if (state == EnumActiveState.CanPick)
        {
            qianreward_yes.gameObject.SetActive(true);
        }
        else if (state == EnumActiveState.AlreadPick)
        {
            qianreward_no.gameObject.SetActive(true);
            qianreward_no.enabled = false;
            qianreward_alreadly.gameObject.SetActive(true);//已经领取过
        }
    }
    #endregion

    #region 活跃度奖励显示

    private void Award_show(int type, int idid)//显示奖励
    {
        receive_btn.gameObject.SetActive(false);
        award_plane.gameObject.SetActive(true);
        //用来打开奖励提示界面 只有活跃度和周奖励能有这个提示
        if (type == 1)
        {
            my_active.text = modulePlayer.roleInfo.dayActivePoint.ToString();
            PActiveBox Info = moduleActive.ActiveValue.Find(a => a.id == idid);
            SetWuInfo(Info);
        }
        else if (type == 2)
        {
            my_active.text = modulePlayer.roleInfo.weekActivePoint.ToString();
            PActiveBox Info = moduleActive.WeekActiveValue.Find(a => a.id == idid);
            SetWuInfo(Info);
        }
        receive_btn.onClick.RemoveAllListeners();
        receive_btn.onClick.AddListener(delegate
        {
            moduleActive.SendGetActiveaward(idid);
        });
    }

    private void SetWuInfo(PActiveBox Info)
    {
        if (Info == null)
        {
            Logger.LogError("award box info is null");
            return;
        }
        AwardGetSucced succed = rewardGroup.GetComponentDefault<AwardGetSucced>();
        succed.SetAward(Info.reward, ShowBox);

        should_active.text = Info.activePoint.ToString();
        receive_btn.gameObject.SetActive(false);//是否能够领取
        if (Util.Parse<int>(my_active.text) >= Info.activePoint)
        {
            receive_btn.gameObject.SetActive(true);
        }
    }

    #endregion

    #region 日常任务

    private void DailyInfoSet(RectTransform rt, PDailyInfo Dinfo)
    {
        DailyInfo dailyInfo = rt.gameObject.GetComponentDefault<DailyInfo>();
        PDailyTask info = moduleActive.DailyListTask.Find(a => a.id == Dinfo.taskId);//ID换掉
        if (info == null)
        {
            Logger.LogError("daily task can not find in modulelist" + Dinfo.taskId);
            return;
        }
        dailyInfo.Click(Dinfo.taskId, DailyGet, Dailygo);
        uint TimeS = 0;
        if (moduleActive.LossTime.ContainsKey(Dinfo.taskId))
        {
            TimeS = moduleActive.AlreadyLossTime(moduleActive.LossTime[Dinfo.taskId]);
        }

        dailyInfo.DetailsInfo(Dinfo, info, TimeS);
    }

    private void DailyGet(int DailyID)
    {
        moduleActive.SendGetDaward(DailyID);
        //日常任务领取发送的协议
    }

    private void Dailygo(int type)
    {
        //日常任务前往 根据type前往不同的界面
        switch (type)
        {
            case 2: moduleAnnouncement.OpenWindow(15); break;
            case 3: moduleAnnouncement.OpenWindow(15, 0); break;
            case 4: moduleAnnouncement.OpenWindow(17); break;
            case 5: moduleAnnouncement.OpenWindow(18); break;
            case 6: moduleAnnouncement.OpenWindow(15); break;
            case 7: moduleAnnouncement.OpenWindow(10); break;
            case 8: moduleAnnouncement.OpenWindow(19); break;
            case 9: moduleAnnouncement.OpenWindow(23); break;
            case 10: moduleGlobal.OpenExchangeTip(TipType.BuyEnergencyTip); break;
            case 11: moduleGlobal.OpenExchangeTip(TipType.BuyGoldTip); break;
            case 12: moduleAnnouncement.OpenWindow(24); break;
            case 13: moduleAnnouncement.OpenWindow(20); break;
            case 14: moduleAnnouncement.OpenWindow(21); break;
            case 15: moduleAnnouncement.OpenWindow(11, 6); break;
            case 16: moduleAnnouncement.OpenWindow(103, 0); break;
            case 17: moduleAnnouncement.OpenWindow(15); break;
            case 19: moduleAnnouncement.OpenWindow(15); break;
            case 20: moduleAnnouncement.OpenWindow(15, 3); break;
            case 21: moduleAnnouncement.OpenWindow(26); break;
            case 22: moduleAnnouncement.OpenWindow(9); break;
            case 23: moduleAnnouncement.OpenWindow(25); break;
            case 24: moduleAnnouncement.OpenWindow(6); break;
            case 25: m_coopBtn.isOn = true; break;
            case 26: moduleAnnouncement.OpenWindow(9); break;
        }
    }

    private void NoDailyTask()//日常任务做完了吗
    {
        bool state = true;
        if (moduleActive.DailyOpenList.Count > 0)
        {
            state = false;
        }
        Nothing.gameObject.SetActive(state);
        m_dailyView.gameObject.SetActive(!state);
    }

    private int GetDIndex(int id)
    {
        int index = -1;
        for (int i = 0; i < moduleActive.DailyOpenList.Count; i++)
        {
            if (moduleActive.DailyOpenList[i].taskId == id)
            {
                index = i;
            }
        }
        return index;
    }
    #endregion

    #region 成就任务

    private void AchieveInfoSet(RectTransform rt, PAchieve Dinfo)
    {
        AchievementInfo achieveInfo = rt.gameObject.GetComponentDefault<AchievementInfo>();
        achieveInfo.Click(Achievementsend, Dinfo.id);
        achieveInfo.SetInfo(Dinfo);
    }

    private int GetPIndex(int id)
    {
        int index = -1;
        for (int i = 0; i < moduleActive.Achieveinfo.Count; i++)
        {
            if (moduleActive.Achieveinfo[i].id == id)
            {
                index = i;
            }
        }
        return index;
    }
    private void Achievementsend(ushort AchieveID)
    {
        //成就任务领取发送的协议
        moduleActive.GetAchevereward(AchieveID);
    }
    #endregion

    #region 协作
    private void SetCoolPlane()
    {
        m_invatePlane.gameObject.SetActive(false);
        m_monShowPlane.gameObject.SetActive(false);
        m_remainText.transform.parent.SafeSetActive(moduleActive.CoopTaskList.Count > 0);
        CoopAllInfo.SetItems(moduleActive.CoopTaskList);
        SetRemainTime();
    }
    private void SetCoopInfo(RectTransform rt, PCooperateInfo Cinfo)
    {
        if (Cinfo == null) return;
        CooperationInfo coop = rt.gameObject.GetComponentDefault<CooperationInfo>();
        coop.SetInfo(Cinfo, SetMonShow);
    }

    //邀请
    private void OpenInvatePlane()
    {
        var info = moduleActive.CoopTaskList.Find(a => a.uid == moduleActive.CheckTaskID);
        if (info == null)
        {
            Logger.LogError("list not have this id task");
            return;
        }
        var task = moduleActive.coopTaskBase.Find(a => a.ID == info.taskId);
        if (task == null)
        {
            Logger.LogError("config not have this id task");
            return;
        }
        moduleGlobal.LockUI();
        m_coopNothing.SafeSetActive(moduleActive.CoopInvateList.Count == 0);
        m_invateView.SafeSetActive(moduleActive.CoopInvateList.Count != 0);
        invateData.SetItems(moduleActive.CoopInvateList);
        m_invatePlane.gameObject.SetActive(true);
        moduleGlobal.UnLockUI();
        m_invateBtn.onClick.RemoveAllListeners();
        m_invateBtn.onClick.AddListener(delegate
        {
            if (moduleActive.m_coopCheckList.Count <= 0) moduleGlobal.ShowMessage(active_text[36]);
            else
            {
                if (task.conditions == null || task.conditions.Length < 1)
                {
                    Logger.LogError("config invate info is error");
                    return;
                }
                m_invatePlane.gameObject.SetActive(false);
                moduleActive.SendInvatePlayer(task.conditions[1].text, task.conditions[1].number);
            }
        });
    }
    private void SetInvateInfo(RectTransform rt, PPlayerInfo info)
    {
        if (info == null) return;
        FriendPrecast fInfo = rt.GetComponentDefault<FriendPrecast>();
        fInfo.DelayAddData(info, 1, 2, 12);
        GameObject checkbg = rt.gameObject.transform.Find("check_Img").gameObject;
        bool have = moduleActive.m_coopCheckList.Exists(a => a == info.roleId);
        checkbg.gameObject.SetActive(have);

    }
    private void SetInvateClick(RectTransform rt, PPlayerInfo info)
    {
        if (info == null) return;
        GameObject select = rt.gameObject.transform.Find("selectbox").gameObject;
        GameObject checkbg = rt.gameObject.transform.Find("check_Img").gameObject;
        bool have = moduleActive.m_coopCheckList.Exists(a => a == info.roleId);
        if (!select) return;
        if (have)
        {
            moduleActive.m_coopCheckList.Remove(info.roleId);
            select.gameObject.SetActive(false);
            checkbg.gameObject.SetActive(false);
        }
        else
        {
            if (moduleActive.m_coopCheckList.Count > GeneralConfigInfo.defaultConfig.coopInvateTop) moduleGlobal.ShowMessage(active_text[37]);
            else
            {
                moduleActive.m_coopCheckList.Add(info.roleId);
                select.gameObject.SetActive(true);
                checkbg.gameObject.SetActive(true);
            }
        }
    }

    //怪物出现的关卡
    private void SetMonShow(CooperationTask info)
    {
        m_monShowPlane.gameObject.SetActive(true);
        UIDynamicImage.LoadImage(m_monIcon, info.icon);
        Util.SetText(m_monDesc, info.desc);
        Util.SetText(m_monsterName, info.name);
        m_noApper.gameObject.SetActive(false);
        m_monsterView.gameObject.SetActive(false);

        if (moduleActive.CoopShowList.Count <= 0) m_noApper.gameObject.SetActive(true);
        else
        {
            m_monsterView.gameObject.SetActive(true);
            monsterData.SetItems(moduleActive.CoopShowList);
        }
    }
    private void SetShowInfo(RectTransform rt, DropInfo info)
    {
        if (info == null) return;
        Text str = rt.gameObject.transform.Find("bg/name").GetComponent<Text>();
        str.text = "<color=#81E2E7FF>" + info.desc + "</color>";
        var btn = rt.GetComponentDefault<Button>();
        btn.SetPressDelay(0);
        btn.onPressed().AddListener(a =>
        {
            if (a) str.text = info.desc;
            if (!a) str.text = "<color=#81E2E7FF>" + info.desc + "</color>";
        });
    }
    private void SetShowClick(RectTransform rt, DropInfo info)
    {
        if (!info.open) return;
        if (!moduleGuide.HasFinishGuide(GeneralConfigInfo.defaultConfig.dropOpenID))
        {
            moduleGlobal.ShowMessage(204, 40);
            return;
        }

        moduleGlobal.SetGoToDrop(info);
        m_monShowPlane.gameObject.SetActive(false);
    }

    #endregion

    #region _ME

    void _ME(ModuleEvent<Module_Active> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Active.EventActiveCoopInvate:
                //接到可邀请列表
                OpenInvatePlane();
                break;
            case Module_Active.EventActiveCoopInfo:
                //所有任务
                SetCoolPlane();
                SetHint();
                break;
            case Module_Active.EventActiveCoopInvateSuc:
                //邀请成功
                var invate = Util.Parse<int>(e.param1.ToString());
                CoopAllInfo.UpdateItem(invate);
                break;
            case Module_Active.EventActiveCoopKiced:
                //踢出成功
                var kiced = Util.Parse<int>(e.param1.ToString());
                CoopAllInfo.UpdateItem(kiced);
                break;
            case Module_Active.EventActiveCoopBeKiced:
                //被踢出
                CoopAllInfo.SetItems(moduleActive.CoopTaskList);
                break;
            case Module_Active.EventActiveCoopValue:
                //进度变化
                var value = Util.Parse<int>(e.param1.ToString());
                CoopAllInfo.UpdateItem(value);
                break;
            case Module_Active.EventActiveCoopCanGet:
                //可领取
                var get = Util.Parse<int>(e.param1.ToString());
                CoopAllInfo.UpdateItem(get);
                SetHint();
                break;
            case Module_Active.EventActiveCoopGet:
                //领取成功
                var succed = Util.Parse<int>(e.param1.ToString());
                PItem2[] reward = e.param2 as PItem2[];
                CoopAllInfo.UpdateItem(succed);
                if (actived && reward != null) Window_ItemTip.Show(active_text[59], reward);
                break;
            case Module_Active.EventActiveCoopApply:
                CoopAllInfo.SetItems(moduleActive.CoopTaskList);
                break;

            case Module_Active.EventActiveDayInfo:
                //上线接收日常详情
                DailyAllInfo.SetItems(moduleActive.DailyOpenList);
                NoDailyTask();
                SetHint();
                break;
            case Module_Active.EventActiveDayValue:
                // 日常进度值有变化
                PDailyInfo info = e.msg as PDailyInfo;
                int index = GetDIndex(info.taskId);
                if (index != -1) DailyAllInfo.SetItem(info, index);
                break;
            case Module_Active.EventActiveDayReach:
                //日常任务达成可以领取奖励
                PDailyInfo cinfo = e.msg as PDailyInfo;
                DailyAllInfo.RemoveItem(cinfo);
                DailyAllInfo.AddItem(cinfo, 0);
                DailyAllInfo.UpdateItems();
                SetHint();
                break;
            case Module_Active.EventActiveDayGet:
                //日常任务奖励领取成功
                PDailyInfo dinfo = e.param1 as PDailyInfo;
                PDailyTask tinfo = e.param2 as PDailyTask;
                DailyAllInfo.RemoveItem(dinfo);
                GetAwardShow(tinfo.reward);
                ActiveNum();//更改今日活跃度进度值
                NoDailyTask();
                break;
            case Module_Active.EventActiveDayOpen:
                //日常任务开启类似早午餐那种
                DailyAllInfo.UpdateItems();
                NoDailyTask();
                SetHint();
                break;
            case Module_Active.EventActiveDayClose:
                //日常任务过期了 直接删掉
                PDailyInfo overinfo = e.param1 as PDailyInfo;
                DailyAllInfo.RemoveItem(overinfo);
                NoDailyTask();
                SetHint();
                break;

            case Module_Active.EventActiveAchieveInfo:
                //上线接收成就详情每次都刷新
                AchieveAllInfo.SetItems(moduleActive.Achieveinfo);
                SetHint();
                break;
            case Module_Active.EventActiveAchieveValue:
                //进度值变化
                PAchieve pinfo = e.msg as PAchieve;
                int pindex = GetPIndex(pinfo.id);
                if (pindex != -1) AchieveAllInfo.SetItem(pinfo, pindex);
                break;
            case Module_Active.EventActiveAchieveCanGet:
                //成就达成可领取奖励调用同一个方法
                PAchieve ainfo = e.msg as PAchieve;
                AchieveAllInfo.RemoveItem(ainfo);
                AchieveAllInfo.AddItem(ainfo, 0);
                SetHint();
                break;
            case Module_Active.EventActiveAchieveGet:
                //成就奖励领取完毕调用同一个方法
                PAchieve ainfo1 = e.msg as PAchieve;
                GetAwardShow(ainfo1.reward, true);
                AchieveAllInfo.SetItems(moduleActive.Achieveinfo);
                break;
            case Module_Active.EventActiveAchieveOpen:
                //某些活动开启 删掉重新创建
                AchieveAllInfo.UpdateItems();
                SetHint();
                break;

            case Module_Active.EventActiveDailyCanGet:
                // 日活跃度可以领取奖励
                PActiveBox activeindo = e.msg as PActiveBox;
                ActiveShua(activeindo);//可领取
                SetHint();
                break;
            case Module_Active.EventActiveDailyGet:
                //日常活跃度奖励领取成功
                award_plane.gameObject.SetActive(false);
                PActiveBox activeinfos = e.msg as PActiveBox;
                ActiveShua(activeinfos);//可领取
                Activetask();
                GetAwardShow(activeinfos.reward);
                break;
            case Module_Active.EventActiveWeekCanGet:
                //周活跃度达成可以领取奖励
                PActiveBox WCanInfo = e.msg as PActiveBox;
                WhichShua(WCanInfo.id);
                SetHint();
                break;
            case Module_Active.EventActiveWeekGet:
                //周活跃度奖励领取成功
                PActiveBox WInfo = e.msg as PActiveBox;
                award_plane.gameObject.SetActive(false);
                WhichShua(WInfo.id);
                GetAwardShow(WInfo.reward);
                break;
            default:
                break;
        }
    }

    #endregion

    #region 红点提示

    private void SetHint()
    {
        //1 所有  0 日常 2 协作 3 成就
        if (actived)
        {
            if (dialy_btn.isOn) moduleActive.ShowDialyHint = false;
            else if (achieve_btn.isOn) moduleActive.ShowAchieveHint = false;
            else if (m_coopBtn.isOn) moduleActive.ShowCoopHint = false;
        }
        m_achieveHint.gameObject.SetActive(moduleActive.ShowAchieveHint);
        m_dailyHint.gameObject.SetActive(moduleActive.ShowDialyHint);
        m_coopHint.gameObject.SetActive(moduleActive.ShowCoopHint);
    }

    #endregion

    protected override void OnReturn()
    {
        SetToggleHide(achieve_btn);
        SetToggleHide(dialy_btn);
        SetToggleHide(m_coopBtn);
        moduleHome.UpdateIconState(HomeIcons.Quest, false);
        Hide(true);
    }

    private void SetToggleHide(Toggle t)
    {
        t.gameObject.SetActive(false);
        t.isOn = false;
        t.gameObject.SetActive(true);
    }

    private void GetAwardShow(PReward Info, bool isachieve = false)//领取之后的提示
    {
        if (isachieve) Window_ItemTip.Show(active_text[19], Info);
        else Window_ItemTip.Show(active_text[20], Info);
    }

    public override void OnRenderUpdate()
    {
        base.OnRenderUpdate();
        if (m_openUpdate)
        {
            m_times += Time.unscaledDeltaTime;
            if (m_times > 1)
            {
                SetRemainTime();
                m_times = 0;
            }
        }
    }
    private void SetRemainTime()
    {
        m_openUpdate = false;
        m_times = 0;
        var rTime = moduleActive.CoopTaskTime - (int)moduleActive.AlreadyLossTime(moduleActive.GetCoopTaskTime);
        if (rTime >= 0) m_openUpdate = true;
        else rTime = 0;
        Util.SetText(m_remainText, Util.GetTimeMarkedFromSec(rTime));
    }
}
