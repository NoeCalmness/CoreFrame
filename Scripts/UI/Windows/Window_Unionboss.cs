/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-08-02
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Unionboss : Window
{
    #region 主界面
    private Text m_bossName;
    // left 
    private GameObject m_hurtPlane;//伤害数值
    private Text m_selfHurtMain;
    private Text m_allHurt;
    private GameObject m_introducePlane;//boss介绍
    private Text m_nextTime;
    private Text m_introduceTxt;
    private Image m_bossBolldBar;//boss血量条
    private Button m_playBtn;
    private Button m_rankBtn;
    //right
    private GameObject m_openBossObj;
    private Image m_openIcon;
    private GameObject m_closeBossObj;
    private Image m_closeIcon;
    private GameObject m_diffectGroup;

    private Text m_rightTipTxt;
    private GameObject m_rightTip;
    private Button m_openBossBtn;//只有管理者看到
    private Button m_enterBossBtn;//倒计时时候普通成员看到的
    private Text m_enterTxt;
    private Button m_bossSet;
    private GameObject m_rightSet; 
    #endregion

    #region 二级
    //boxshow
    private GameObject m_previewPlane;
    private Text m_canGetTxt;//多少血量可领取
    private GameObject m_rewardGroup;
    private Button m_getRewardBtn;
    //rank Plane
    private GameObject m_rankPlane;//排行榜
    private GameObject m_myHurtInfo;
    private Text m_selfHurtNormal;//自己伤害不在前三
    private Image m_selfHurtBefor;//自己伤害在前三
    private Text m_selfName;//自己名字 
    private Text m_myHurt;//自己伤害
    //
    private GameObject m_setPlane;
    //drop plane
    private GameObject m_playExplainPlane;
    private Text m_playTxt;
    #endregion

    #region effect
    private Transform m_tipPlane;
    private ScrollView m_effectView;
    private Text effectName;
    private Text effectDesc;
    private Text effectCost;
    private Button effectCancle;
    private Button effectSure;
    private DataSource<BossEffectInfo> effectProp;
    #endregion

    private List<GameObject> m_rewaardShow = new List<GameObject>();//左边宝箱
    private Dictionary<int, GameObject> m_rewardObj = new Dictionary<int, GameObject>();

    private List<GameObject> m_diffcutType = new List<GameObject>();//难度

    private List<GameObject> m_rewardShowList = new List<GameObject>();//奖励预览
    private DataSource<PPlayerHurt> m_hurtSort;//伤害排行
    
    private bool m_diedEnd = false;
    private float m_endTime = 0;
    private int m_endTimeNow;

    private bool m_downOpen = false;
    private float m_openTime = 0;
    private int m_openTimeNow;

    private bool m_return;

    public string m_tipTxt;

    private int AnmiaDelayTimes = 1;
    protected override void OnOpen()
    {
        #region 主界面
        m_bossName = GetComponent<Text>("left/bossName");
        //left
        m_hurtPlane = GetComponent<RectTransform>("left/bossInfo_Panel01").gameObject;
        m_selfHurtMain = GetComponent<Text>("left/bossInfo_Panel01/personalTotalDamageNumber_Txt");
        m_allHurt = GetComponent<Text>("left/bossInfo_Panel01/unionTotalDamageNumber_Txt");
        m_introducePlane = GetComponent<RectTransform>("left/bossInfo_Panel02").gameObject;
        m_nextTime = GetComponent<Text>("left/bossInfo_Panel02/nextActiceTime_Txt");
        m_introduceTxt = GetComponent<Text>("left/bossInfo_Panel02/bossInto_Txt");
        m_bossBolldBar = GetComponent<Image>("left/bossHpBarFilled_Img");
        m_playBtn = GetComponent<Button>("left/leftBottom_Btns/info_Btn");
        m_rankBtn = GetComponent<Button>("left/leftBottom_Btns/rank_Btn");
        //right
        m_openBossObj = GetComponent<RectTransform>("right/bossIcon_Panel").gameObject;
        m_openIcon = GetComponent<Image>("right/bossIcon_Panel/bossIcon_Img");
        m_closeBossObj = GetComponent<RectTransform>("right/bossIconDisable_Panel").gameObject;
        m_closeIcon = GetComponent<Image>("right/bossIconDisable_Panel/bossIconDisable_Img");
        m_diffectGroup = GetComponent<RectTransform>("right/diffculttype").gameObject;
        m_openBossBtn = GetComponent<Button>("right/fightDisable_Btn");
        m_enterBossBtn = GetComponent<Button>("right/fightAvailable_Btn");
        m_enterTxt = GetComponent<Text>("right/fightAvailable_Btn/fightAvailable_Txt");
        m_rightTip = GetComponent<RectTransform>("right/openenter_txt").gameObject;
        m_rightTipTxt = GetComponent<Text>("right/openenter_txt/reviveCountDown_Img/reviveCountDown_Txt");
        m_bossSet = GetComponent<Button>("right/rightBottom_Btn/setting_Btn");
        m_rightSet = GetComponent<RectTransform>("right/rightBottom_Btn").gameObject;
        #endregion

        #region 二级
        //boxshow
        m_previewPlane = GetComponent<RectTransform>("preview_panel").gameObject;
        m_canGetTxt = GetComponent<Text>("preview_panel/bg/active_text");
        m_rewardGroup = GetComponent<RectTransform>("preview_panel/rewardgroup").gameObject;
        m_getRewardBtn = GetComponent<Button>("preview_panel/get_button");
        //rank Plane
        m_rankPlane = GetComponent<RectTransform>("rank_Panel").gameObject;
        m_selfHurtNormal = GetComponent<Text>("rank_Panel/self_rank/rank_txt");
        m_myHurtInfo= GetComponent<RectTransform>("rank_Panel/self_rank").gameObject;
        m_selfHurtBefor = GetComponent<Image>("rank_Panel/self_rank/rank_img");
        m_myHurt = GetComponent<Text>("rank_Panel/self_rank/integral_text");
        m_selfName = GetComponent<Text>("rank_Panel/self_rank/name_text");
        m_hurtSort = new DataSource<PPlayerHurt>(moduleUnion.m_playerHurt, GetComponent<ScrollView>("rank_Panel/scrollView"), SortHurt);
        m_setPlane = GetComponent<RectTransform>("bossActiceSetting_Panel").gameObject;
        //tip
        m_playExplainPlane = GetComponent<RectTransform>("dropInfo").gameObject;
        m_playTxt = GetComponent<Text>("dropInfo/info/inner/labyrinth_rules/Viewport/Content/rulescontent");
        #endregion

        #region effect
        m_tipPlane = GetComponent<RectTransform>("tip");           
        m_effectView = GetComponent<ScrollView>("effectBg/scrollView");
        effectName = GetComponent<Text>("tip/kuang/equipinfo");
        effectDesc = GetComponent<Text>("tip/kuang/content_tip");
        effectCost = GetComponent<Text>("tip/kuang/cost/icon/now");
        effectSure = GetComponent<Button>("tip/kuang/confirm");
        effectCancle = GetComponent<Button>("tip/kuang/cancel"); 
        effectProp = new DataSource<BossEffectInfo>(moduleUnion.AllBossBuff, m_effectView, EffectInfo, EffectClick);
        #endregion

        SetBtnClick();
        SetText();
        GetChildObj();
    }

    private void SetText()
    {
        Util.SetText(GetComponent<Text>("tip/kuang/cancel/Image"), 9, 1);
        Util.SetText(GetComponent<Text>("tip/kuang/confirm/Image"), 242, 219);
        Util.SetText(GetComponent<Text>("tip/kuang/cost"), 200, 15);
        Util.SetText(GetComponent<Text>("effectBg/titile"), 242, 217);
        Util.SetText(m_playTxt, 90100);//玩法说明文本

        Util.SetText(GetComponent<Text>("preview_panel/get_button/get"), 223, 6);
        Util.SetText(GetComponent<Text>("left/bossInfo_Panel02/nextActice_Txt"), 242, 166);
        GetComponent<Text>("left/bossInfo_Panel02/nextActice_Txt").gameObject.SetActive(false);
        Util.SetText(GetComponent<Text>("right/fightDisable_Btn/fightDisable_Txt"), ConfigText.GetDefalutString(242, 118));
        Util.SetText(GetComponent<Text>("right/diffculttype/diff01_Img/diff01_Txt"), ConfigText.GetDefalutString(242, 84));
        Util.SetText(GetComponent<Text>("right/diffculttype/diff02_Img/diff02_Txt"), ConfigText.GetDefalutString(242, 85));
        Util.SetText(GetComponent<Text>("right/diffculttype/diff03_Img/diff03_Txt"), ConfigText.GetDefalutString(242, 86));
        Util.SetText(GetComponent<Text>("right/diffculttype/diff04_Img/diff04_Txt"), ConfigText.GetDefalutString(242, 87));
        Util.SetText(GetComponent<Text>("left/bossInfo_Panel01/personalTotalDamage_Txt"), ConfigText.GetDefalutString(242, 89));
        Util.SetText(GetComponent<Text>("left/leftBottom_Btns/info_Btn/info_Txt"), ConfigText.GetDefalutString(242, 90));
        Util.SetText(GetComponent<Text>("left/leftBottom_Btns/rank_Btn/rank_Txt"), ConfigText.GetDefalutString(242, 91));

        Util.SetText(GetComponent<Text>("bossActiceSetting_Panel/title_Txt"), ConfigText.GetDefalutString(242, 92));
        Util.SetText(GetComponent<Text>("bossActiceSetting_Panel/bossDiff_Txt"), ConfigText.GetDefalutString(242, 93));
        Util.SetText(GetComponent<Text>("bossActiceSetting_Panel/bossDiff_group/0/0_Text"), ConfigText.GetDefalutString(242, 84));
        Util.SetText(GetComponent<Text>("bossActiceSetting_Panel/bossDiff_group/1/1_Text"), ConfigText.GetDefalutString(242, 85));
        Util.SetText(GetComponent<Text>("bossActiceSetting_Panel/bossDiff_group/2/2_Text"), ConfigText.GetDefalutString(242, 86));
        Util.SetText(GetComponent<Text>("bossActiceSetting_Panel/bossDiff_group/3/3_Text"), ConfigText.GetDefalutString(242, 87));
        Util.SetText(GetComponent<Text>("bossActiceSetting_Panel/openAutoBeginActice_Toggle/openAutoBeginActice_Txt"), ConfigText.GetDefalutString(242, 94));
        Util.SetText(GetComponent<Text>("bossActiceSetting_Panel/bossWeek_group/0/0_Text"), ConfigText.GetDefalutString(242, 95));
        Util.SetText(GetComponent<Text>("bossActiceSetting_Panel/bossWeek_group/1/1_Text"), ConfigText.GetDefalutString(242, 96));
        Util.SetText(GetComponent<Text>("bossActiceSetting_Panel/bossWeek_group/2/2_Text"), ConfigText.GetDefalutString(242, 97));
        Util.SetText(GetComponent<Text>("bossActiceSetting_Panel/bossWeek_group/3/3_Text"), ConfigText.GetDefalutString(242, 98));
        Util.SetText(GetComponent<Text>("bossActiceSetting_Panel/bossWeek_group/4/4_Text"), ConfigText.GetDefalutString(242, 99));
        Util.SetText(GetComponent<Text>("bossActiceSetting_Panel/bossWeek_group/5/5_Text"), ConfigText.GetDefalutString(242, 100));
        Util.SetText(GetComponent<Text>("bossActiceSetting_Panel/bossWeek_group/6/6_Text"), ConfigText.GetDefalutString(242, 101));

        Util.SetText(GetComponent<Text>("bossActiceSetting_Panel/canceltimeSetting_Btn/cancelJoinCondition_Txt"), ConfigText.GetDefalutString(242, 102));
        Util.SetText(GetComponent<Text>("bossActiceSetting_Panel/savetimeSetting_Btn/saveJoinCondition_Txt"), ConfigText.GetDefalutString(242, 103));

        Util.SetText(GetComponent<Text>("preview_panel/bg/equip_prop/top/equipinfo"), ConfigText.GetDefalutString(242, 104));

        Util.SetText(GetComponent<Text>("rank_Panel/bg/titleBg/title_shadow"), ConfigText.GetDefalutString(242, 105));
        Util.SetText(GetComponent<Text>("rank_Panel/bg/bg_frame/paiming_text"), ConfigText.GetDefalutString(242, 106));
        Util.SetText(GetComponent<Text>("rank_Panel/bg/bg_frame/nicheng_text"), ConfigText.GetDefalutString(242, 107));
        Util.SetText(GetComponent<Text>("rank_Panel/bg/bg_frame/jifen_text"), ConfigText.GetDefalutString(242, 108));

        Util.SetText(GetComponent<Text>("dropInfo/info/title"), ConfigText.GetDefalutString(242, 90));
        Util.SetText(GetComponent<Text>("dropInfo/info/inner/rulestitle"), ConfigText.GetDefalutString(242, 109));
        Util.SetText(GetComponent<Text>("dropInfo/info/inner/rewardTitle"), ConfigText.GetDefalutString(242, 110));

        Util.SetText(GetComponent<Text>("tipsignsccced/success/signsucced/up_h/up"), ConfigText.GetDefalutString(242, 111));
        Util.SetText(GetComponent<Text>("tipsignsccced/success/signsucced/up_h/up (1)"), ConfigText.GetDefalutString(242, 112));
        Util.SetText(GetComponent<Text>("tipsignsccced/success/signsucced/up_h/up (2)"), ConfigText.GetDefalutString(242, 113));
        Util.SetText(GetComponent<Text>("tipsignsccced/success/signsucced/up_h/up (3)"), ConfigText.GetDefalutString(242, 114));

        Util.SetText(GetComponent<Text>("rank_Panel/bg/title"), ConfigText.GetDefalutString(242, 105));
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleUnion.OpenBossWindow = false;
        m_return = false;
        moduleGlobal.ShowGlobalLayerDefault();//top工具条的显示
        if (moduleUnion.BossInfo != null) moduleUnion.SetNewBoss();

        SetLeft();
        SetRight();
    }

    private void SetBtnClick()
    {
        m_playBtn.onClick.AddListener(SetPlayPlane);
        m_rankBtn.onClick.AddListener(SetRankPlane);
        m_bossSet.onClick.AddListener(SetBossSet);
    }

    private void GetChildObj()
    {
        m_rewardShowList.Clear();
        m_rewaardShow.Clear();
        m_diffcutType.Clear();
        m_rewardObj.Clear();
        foreach (Transform item in m_rewardGroup.transform)
        {
            m_rewardShowList.Add(item.gameObject);
        };
        foreach (Transform item in m_bossBolldBar.gameObject.transform)
        {
            m_rewaardShow.Add(item.gameObject);
        }
        foreach (Transform item in m_diffectGroup.transform)
        {
            m_diffcutType.Add(item.gameObject);
        }
        for (int i = 0; i < m_rewaardShow.Count; i++)
        {
            if (i < moduleUnion.m_bossReward.Count)
            {
                m_rewardObj.Add(moduleUnion.m_bossReward[i].ID, m_rewaardShow[moduleUnion.m_bossReward[i].boxPos - 1]);
            }
        }
    }

    #region 主界面
    private void SetLeft()
    {
        if (moduleUnion.m_bossStage == null || moduleUnion.m_bossReward == null || moduleUnion.BossInfo == null)
        {
            Logger.LogError("confige is error");
            return;
        }
        Util.SetText(m_bossName, moduleUnion.m_bossStage.nameId);
        float nowblood = 0;
        if (moduleUnion.BossInfo.bossstate == 0)//未在开启时间
        {
            m_hurtPlane.gameObject.SetActive(false);
            m_introducePlane.gameObject.SetActive(true);
            Util.SetText(m_introduceTxt, moduleUnion.m_bossStage.descId);
            nowblood = 1.0f;
            // GetNextTime();
        }
        else if (moduleUnion.BossInfo.bossstate == 1)//在开启时间
        {
            var allhurt = moduleUnion.m_bossStage.bossHP - moduleUnion.BossInfo.remianblood;
            if (moduleUnion.m_onlyInfo.hurt > allhurt) moduleUnion.m_onlyInfo.hurt = allhurt;

            m_selfHurtMain.text = moduleUnion.m_onlyInfo.hurt.ToString();
            m_allHurt.text = allhurt.ToString();
            m_hurtPlane.gameObject.SetActive(true);
            m_introducePlane.gameObject.SetActive(false);
            nowblood = (float)moduleUnion.BossInfo.remianblood / moduleUnion.m_bossStage.bossHP;
            //取boss对应的配置
        }

        m_bossBolldBar.fillAmount = nowblood;
        var index = -1;
        AnmiaDelayTimes = 0;
        for (int i = 0; i < m_rewaardShow.Count; i++)
        {
            if (i < moduleUnion.m_bossReward.Count)
            {
                if (moduleUnion.m_bossReward[i] == null) continue;
                SetBoxState(moduleUnion.m_bossReward[i].ID, m_rewaardShow[moduleUnion.m_bossReward[i].boxPos - 1]);//从配置取
                if (moduleUnion.m_boxStae[moduleUnion.m_bossReward[i].ID] != EnumActiveState.NotPick) index = i;
            }
        }
        m_nextTime.gameObject.SetActive(false);

        SetEffect();

        if (index < GeneralConfigInfo.defaultConfig.UnionBossBlood.Length - 1)
            m_bossBolldBar.color = GeneralConfigInfo.defaultConfig.UnionBossBlood[index + 1];
    }

    private void GetNextTime()//下次开启时间
    {
        if (moduleUnion.BossSet == null)
        {
            Logger.LogError("null");
            return;
        }
        if (moduleUnion.BossSet.bossautomatic == 0)
        {
            m_nextTime.gameObject.SetActive(false);
        }
        else if (moduleUnion.BossSet.bossautomatic == 1)
        {
            m_nextTime.gameObject.SetActive(true);

            int hour = moduleUnion.BossSet.opentime / 3600;
            int mintue = (moduleUnion.BossSet.opentime % 3600) / 60;
            //先判断今天过没过
            int nowWeek = (int)Util.GetServerLocalTime().DayOfWeek;
            int nextTime = nowWeek;
            DateTime nowTime = Util.GetServerLocalTime();
            if ((nowTime.Hour > hour)|| moduleUnion.BossInfo.remaintimes == 1)
            {
                nextTime++;
            }
            else if (nowTime.Hour == hour && nowTime.Minute >= mintue)
            {
                nextTime++;
            }
            
            List<sbyte> openDay = new List<sbyte>();
            openDay.AddRange(moduleUnion.BossSet.openday);
            openDay.Sort();
            
            if (openDay.Count == 0) return;
            if (nextTime > openDay[openDay.Count - 1] || nextTime < openDay[0])
            {
                nextTime = openDay[0];
            }
            else
            {
                for (int i = 0; i < openDay.Count; i++)
                {
                    if (nextTime <= openDay[i])
                    {
                        nextTime = openDay[i];
                        break;
                    }
                }
            }
            SetNextShow(nextTime, hour.ToString(), mintue.ToString());
        }
    }
    private void SetNextShow(int nextweek, string hour, string mintue)
    {
        string times = "";
        switch (nextweek)
        {
            case 1: times = ConfigText.GetDefalutString(242, 95); break;
            case 2: times = ConfigText.GetDefalutString(242, 96); break;
            case 3: times = ConfigText.GetDefalutString(242, 97); break;
            case 4: times = ConfigText.GetDefalutString(242, 98); break;
            case 5: times = ConfigText.GetDefalutString(242, 99); break;
            case 6: times = ConfigText.GetDefalutString(242, 100); break;
            case 7: times = ConfigText.GetDefalutString(242, 101); break;
        }
        string str = times + hour + ConfigText.GetDefalutString(242, 115) + mintue + ConfigText.GetDefalutString(242, 116);
        Util.SetText(m_nextTime, str);
    }
    
    private void CountDown(int type)//倒计时 0 未开启的倒计时 1 复活冷却的倒计时
    {
        if (moduleUnion.BossInfo == null)
        {
            return;
        }

        if (type == 0 && moduleUnion.BossInfo.bossautomatic == 1)
        {
            m_downOpen = true;
            m_openTimeNow = (int)(moduleUnion.BossInfo.opentime - moduleUnion.BossLossTime(moduleUnion.m_openScenceTime));
            SetTime(m_openTimeNow, m_rightTipTxt);

            if (m_openTimeNow < 1800 && m_openTimeNow > 0)
            {
                m_rightTip.gameObject.SetActive(true);
            }
            else
            {
                m_rightTip.gameObject.SetActive(false);
            }
        }
        else if (type == 1)
        {
            m_endTimeNow = (int)(moduleUnion.m_onlyInfo.cooltime - moduleUnion.BossLossTime(moduleUnion.m_killTime));
            SetTime(m_endTimeNow, m_rightTipTxt);

            if (m_endTimeNow > 0)
            {
                m_diedEnd = true;
            }
            else
            {
                m_rightTip.gameObject.SetActive(false);
                m_enterBossBtn.interactable = true;
            }
        }
    }
    
    private void SetBoxState(int boxId, GameObject objs)
    {
        //设置宝箱的位置和状态
        Button cangeton = objs.transform.Find("chest_Btn").GetComponent<Button>();
        Button cangetyes = objs.transform.Find("chest_Btn02").GetComponent<Button>();
        Image getalready = objs.transform.Find("chest_Btn03").GetComponent<Image>();
        Text leveltxt = objs.transform.Find("bossHpScale_Txt").GetComponent<Text>();
        Image lockImg = objs.transform.Find("lock").GetComponent<Image>();
        RectTransform pos = objs.GetComponent<RectTransform>();
        cangeton.gameObject.SetActive(false);
        cangetyes.gameObject.SetActive(false);
        getalready.gameObject.SetActive(false);
        lockImg.gameObject.SetActive(false);
        cangeton.enabled = true;
        if (moduleUnion.m_boxStae[boxId] == EnumActiveState.NotPick)
        {
            cangeton.gameObject.SetActive(true);
        }
        else if (moduleUnion.m_boxStae[boxId] == EnumActiveState.CanPick)
        {
            cangetyes.gameObject.SetActive(true);
            SetAnimation(boxId, lockImg);
        }
        else if (moduleUnion.m_boxStae[boxId] == EnumActiveState.AlreadPick)
        {
            cangeton.enabled = false;
            getalready.gameObject.SetActive(true);
        }
        BossBoxReward reward = moduleUnion.m_bossReward.Find(a => a.ID == boxId);
        leveltxt.text = "hp" + reward.condition + "%";
        
        float x = (((float)reward.condition / 100) * 462) - 6;
        pos.anchoredPosition = new Vector3(x, 0, 0);

        cangeton.onClick.RemoveAllListeners();
        cangetyes.onClick.RemoveAllListeners();
        cangeton.onClick.AddListener(delegate
        {
            ShowBoxReward(reward);
        });
        cangetyes.onClick.AddListener(delegate
        {
            ShowBoxReward(reward);
        });
    }
    private void SetAnimation(int boxId, Image lockImg)
    {
        if (!actived) return;
        var key = "union" + modulePlayer.id_.ToString() + boxId.ToString();
        var show = moduleWelfare.FrirstOpen(key, false);
        if (show)
        {
            DelayEvents.Add(() =>
            {
                lockImg.gameObject.SetActive(true);
                PlayerPrefs.SetString(key, Util.GetServerLocalTime().ToString());
            }, 1.5f * AnmiaDelayTimes);
            AnmiaDelayTimes++;
        }
    }
    
    private void SetRight()
    {
        m_downOpen = false;
        m_diedEnd = false;
        m_rightTip.gameObject.SetActive(false);
        m_openBossBtn.gameObject.SetActive(false);
        m_enterBossBtn.gameObject.SetActive(false);
        m_closeBossObj.gameObject.SetActive(false);
        m_openBossObj.gameObject.SetActive(false);
        m_enterBossBtn.interactable = false;

        if (moduleUnion.m_bossStage == null || moduleUnion.m_bossReward == null || moduleUnion.BossInfo == null)
        {
            Logger.LogError("confige is error");
            return;
        }
        //取boss对应的配置
        for (int i = 0; i < m_diffcutType.Count; i++)
        {
            m_diffcutType[i].SetActive(i == (moduleUnion.m_bossStage.diffcut - 1));
        }
        UIDynamicImage.LoadImage(m_openIcon.gameObject, moduleUnion.m_bossStage.bossIcon);
        UIDynamicImage.LoadImage(m_closeIcon.gameObject, moduleUnion.m_bossStage.bossIcon);

        if (moduleUnion.BossInfo.bossstate == 0)//未开启
        {
            m_getRewardBtn.gameObject.SetActive(false);
            m_closeBossObj.gameObject.SetActive(true);
            m_openBossObj.gameObject.SetActive(false);
            
            if (moduleUnion.inUnion == 0 || moduleUnion.inUnion == 1)//如果自己是管理层
            {
                m_openBossBtn.gameObject.SetActive(true);
                if (moduleUnion.BossInfo.remaintimes == 0)//0 今日可开 1 今日不可开
                {
                    m_openBossBtn.interactable = true;
                }
            }
            else
            {
                Util.SetText(m_enterTxt, ConfigText.GetDefalutString(242, 193));
                m_enterBossBtn.gameObject.SetActive(true);
            }

            if (moduleUnion.BossInfo.remaintimes == 1)//0 未打过 1 打过
            {
                m_rightTip.gameObject.SetActive(true);
                m_openBossBtn.interactable = false;
                m_enterBossBtn.interactable = false;
                Util.SetText(m_rightTipTxt, ConfigText.GetDefalutString(242, 174));
            }
            else
            {
                m_tipTxt = ConfigText.GetDefalutString(242, 121);
                CountDown(0);
            }
        }
        else if (moduleUnion.BossInfo.bossstate == 1)//开启
        {
            m_closeBossObj.gameObject.SetActive(false);
            m_openBossObj.gameObject.SetActive(true);

            m_enterBossBtn.gameObject.SetActive(true);
            if (moduleUnion.m_onlyInfo.cooltime > 0)//如果在死亡冷却时间
            {
                m_enterBossBtn.interactable = false;
                m_rightTip.gameObject.SetActive(true);
                Util.SetText(m_enterTxt, ConfigText.GetDefalutString(242, 119));

                m_tipTxt = ConfigText.GetDefalutString(242, 122);
                CountDown(1);
            }
            else
            {
                Util.SetText(m_enterTxt, ConfigText.GetDefalutString(242, 119));
                m_enterBossBtn.interactable = true;
            }

            if (moduleUnion.BossInfo.remianblood <= 0)
                m_enterBossBtn.interactable = false;

        }
        m_openBossBtn.onClick.RemoveAllListeners();
        m_enterBossBtn.onClick.RemoveAllListeners();
        m_openBossBtn.onClick.AddListener(delegate
        {
            Window_Alert.ShowAlert(ConfigText.GetDefalutString(242, 173), true, true, true,()=>{ moduleUnion.OpenBoss(); }, null, "", "");

            //管理层开启boss活动 
            ;
        });
        m_enterBossBtn.onClick.AddListener(delegate
        {
            //请求进入打boss
            moduleUnion.EnterBoss();
        });
        m_rightSet.gameObject.SetActive(false);
        if (moduleUnion.inUnion == 0 || moduleUnion.inUnion == 1)//如果自己是管理层
        {
            m_rightSet.gameObject.SetActive(true);
        }
    }
    #endregion

    #region 二级提示

    private void ShowBoxReward(BossBoxReward wrad)
    {
        if (wrad == null)
        {
            Logger.LogError("confige is error");
            return;
        }
        m_previewPlane.gameObject.SetActive(true);
        
        Util.SetText(m_canGetTxt, ConfigText.GetDefalutString(wrad.descID));

        AwardGetSucced reward = m_rewardGroup.GetComponentDefault<AwardGetSucced>();
        if (wrad != null)
        {
            reward.SetUnionAward(m_rewardShowList, wrad.preview);
        }

        float nowblood = (float)(moduleUnion.m_bossStage.bossHP * wrad.condition) / 100f;
        m_getRewardBtn.gameObject.SetActive(false);
        if (moduleUnion.BossInfo.remianblood <= nowblood)
        {
            m_getRewardBtn.gameObject.SetActive(true);
        }
        m_getRewardBtn.onClick.RemoveAllListeners();
        m_getRewardBtn.onClick.AddListener(delegate
        {
            //发送领取奖励
            moduleUnion.GetBoxReward(wrad.ID);
        });
    }
    //rank
    private void SetRankPlane()
    {
        if (moduleUnion.BossInfo == null)
        {
            Logger.LogError("confige is error");
            return;
        }
        if (moduleUnion.BossInfo.bossstate == 0)
        {
            SetRankInfo();
        }
        else if (moduleUnion.BossInfo.bossstate == 1)
        {
            moduleUnion.SendGetHurtSort();
        }
    }

    private void SetRankInfo()
    {
        if (moduleUnion.m_onlyInfo == null)
        {
            Logger.LogError("confige is error");
            return;
        }
        m_myHurtInfo.gameObject.SetActive(true);
        PPlayerHurt info = moduleUnion.m_playerHurt.Find(a => a.roleid == modulePlayer.roleInfo.roleId);
        if (info == null)
        {
            info = PacketObject.Create<PPlayerHurt>();
            info.roleid = modulePlayer.id_;
            info.name = modulePlayer.name_;
            info.hurt = 0;
        }
        RefreshIntergl refresh = m_myHurtInfo.GetComponentDefault<RefreshIntergl>();
        refresh.SetUnionHurt(info, false);
        m_hurtSort.SetItems(moduleUnion.m_playerHurt);
    }
    private void SortHurt(RectTransform rt, PPlayerHurt info)
    {
        if (info == null) return;
        RefreshIntergl refresh = rt.gameObject.GetComponentDefault<RefreshIntergl>();
        refresh.SetUnionHurt(info, true);
    }
    
    //drop item
    private void SetPlayPlane()
    {
        m_return = true;
    }

    //set plane

    private void SetBossSet()
    {
        if (moduleUnion.BossInfo.bossstate == 1)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 175));
        }
        else
        {
            m_setPlane.gameObject.SetActive(true);
            UnionBossSet set = m_setPlane.GetComponentDefault<UnionBossSet>();
            set.BossSetInfo();
        }
    }

    //succed tip
    private void ShowSuccedReward(PItem2[] info,int boxId)
    {
        m_previewPlane.gameObject.SetActive(false);
        Window_ItemTip.Show(ConfigText.GetDefalutString(242, 194), info);
        AnmiaDelayTimes = 0;
        SetBoxState(boxId, m_rewardObj[boxId]);
    }
    #endregion

    #region effect
    private void SetEffect()
    {
        effectProp.SetItems(moduleUnion.AllBossBuff);
    }

    private void EffectInfo(RectTransform rt, BossEffectInfo info)
    {
        if (info == null) return;
        
        var time = 0;
        var eff = moduleUnion.BossBuffInfo.Find(a => a.effectId == info.ID);
        if (eff != null) time = eff.times;

        var img = rt.Find("icon").GetComponent<Image>();
        AtlasHelper.SetShared(img, info.icon);
        var eName = rt.Find("name").GetComponent<Text>();
        Util.SetText(eName, info.nameId);
        var open = rt.Find("open").GetComponent<Text>();
        var openBg = rt.Find("openBg");
        open.gameObject.SetActive(time > 0);
        openBg.gameObject.SetActive(time > 0);
        img.SafeSetActive(false);
        img.saturation = time > 0 ? 1 : 0;
        img.SafeSetActive(true);
        Util.SetText(open, time.ToString ());
    }
    private void EffectClick(RectTransform rt, BossEffectInfo info)
    {
        if (info == null) return;
        m_tipPlane.gameObject.SetActive(true);
        SetTipPlane(info.ID);
    }
    private void SetTipPlane(int effId)
    {
        BossEffectInfo eff = ConfigManager.Get<BossEffectInfo>(effId);
        if (eff == null) return;
        Util.SetText(effectDesc, eff.descId);
        Util.SetText(effectName, eff.nameId);
        effectSure.onClick.RemoveAllListeners();
        effectSure.onClick.AddListener(delegate
        {
            moduleUnion.SendBuyEffect(effId);
        });
        if (moduleUnion.BossInfo != null)
        {
            effectSure.SafeSetActive(moduleUnion.BossInfo.bossstate == 1);
            effectCancle.SafeSetActive(moduleUnion.BossInfo.bossstate == 1);
        }
        if (eff.cost == null || eff.cost?.Length <= 0) effectCost.gameObject.SetActive(false);
        else
        {
            effectCost.gameObject.SetActive(true);
            Util.SetText(effectCost, ConfigText.GetDefalutString(242, 211), eff.cost[0].count, modulePlayer.roleInfo.diamond);
        }
    }

    #endregion

    void _ME(ModuleEvent<Module_Union> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Union.EventUnionBuyEffect:
                var effId = Util.Parse<int>(e.param1.ToString());
                if(m_tipPlane.gameObject.activeInHierarchy) SetTipPlane(effId);
                SetEffect();
                CountDown(1);
                break;
            case Module_Union.EventUnionBossOpen:
                //开启
                SetLeft();
                SetRight();
                m_rightTip.gameObject.SetActive(false);
                break;
            case Module_Union.EventUnionBossClose:
                //关闭
                if (!actived || moduleUnion.EnterBossTask) return;
                moduleUnion.SetNewBoss();
                SetLeft();
                SetRight();
                break;
            case Module_Union.EventUnionBossChange:
                //设置更改
                SetBossSet();
                SetLeft();
                SetRight();
                m_setPlane.gameObject.SetActive(false);
                break;
            case Module_Union.EventUnionBossHurt:
                //刷新伤害值
                SetLeft();
                break;
            case Module_Union.EventUnionBossCanGet:
                //有奖励发生变化
                SetLeft();
                break;
            case Module_Union.EventUnionBossSortHurt:
                //查看排行榜
                SetRankInfo();
                break;
            case Module_Union.EventUnionBossKillCool:
                //冷却时间完成
                //m_diedEnd = false;
                //m_enterBossBtn.interactable = true;
                //m_rightTip.gameObject.SetActive(false);
                //m_openBossBtn.gameObject.SetActive(false);
                //m_enterBossBtn.gameObject.SetActive(true);
                break;
            case Module_Union.EventUnionRewardGet:
                //奖励领取成功
                var reward = e.param1 as PItem2[];
                var boxid = Util.Parse<int>(e.param2.ToString());
                ShowSuccedReward(reward,boxid);
                break;
            case Module_Union.EventUnionBossOver:
                m_diedEnd = true;
                break;

            case Module_Union.EventUnionSelfExit:
                //自己被踢出或者自己退出
                var type = Util.Parse<int>(e.param1.ToString());
                var pos = Util.Parse<int>(e.param2.ToString());

                if (type != 0 || pos == 0) GetMyUnionExit();
                else if (actived)
                {
                    Window_Alert.ShowAlertDefalut(ConfigText.GetDefalutString(242, 210),
                        () => { GetMyUnionExit(); }, () => { GetMyUnionExit(); });
                }
                break;
        }
    }

    private void GetMyUnionExit()
    {
        m_playExplainPlane.gameObject.SetActive(false);
        m_return = false;
        Hide();
    }
    
    public override void OnRenderUpdate()
    {
        base.OnRenderUpdate();
        if (m_downOpen)
        {
            m_openTime += Time.unscaledDeltaTime;
            if (m_openTime > 1)
            {
                m_openTime = 0;
                m_openTimeNow--;

                SetTime(m_openTimeNow, m_rightTipTxt);
                if (m_openTimeNow == 0)
                {
                    m_downOpen = false;
                    m_rightTip.gameObject.SetActive(false);
                    m_openBossBtn.gameObject.SetActive(false);
                    m_enterBossBtn.gameObject.SetActive(true);
                    moduleUnion.BossInfo.bossstate = 1;
                }
            }
        }
        if (m_diedEnd)
        {
            m_endTime += Time.unscaledDeltaTime;
            if (m_endTime > 1)
            {
                m_endTime = 0;
                m_endTimeNow--;
                SetTime(m_endTimeNow, m_rightTipTxt);
                if (m_endTimeNow == 0)
                {
                    moduleUnion.m_onlyInfo.cooltime = 0;
                    m_diedEnd = false;
                    m_enterBossBtn.interactable = true;
                    m_rightTip.gameObject.SetActive(false);
                    m_openBossBtn.gameObject.SetActive(false);
                    m_enterBossBtn.gameObject.SetActive(true);

                    if (moduleUnion.BossInfo.remianblood <= 0)
                        m_enterBossBtn.interactable = false;
                }

            }
        }
    }
    private void SetTime(int timeNow, Text thisText)
    {
        var strTime = m_tipTxt + Util.GetTimeMarkedFromSec(timeNow);
        Util.SetText(thisText, strTime);
    }

    protected override void OnReturn()
    {
        if (m_return)
        {
            m_playExplainPlane.gameObject.SetActive(false);
            m_return = false;
        }
        else
        {
            base.OnReturn();
        }
    }
}
