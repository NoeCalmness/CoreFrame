/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-08-09
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;


#region 二级弹窗基类
/// <summary>
/// UI自定义界面（二级弹窗）
/// </summary>
public class CustomSecondPanel
{
    public GameObject gameObject;
    public RectTransform rectTransform;
    public Transform transform;
    public bool enableUpdate { get; set; } = false;

    public bool canUpdate { get { return enable && enableUpdate; } }
    public bool enable
    {
        get { return gameObject == null ? false : gameObject.activeSelf; } 
        set { gameObject?.SetActive(value); }
    }

    public bool activeInHierarchy { get { return gameObject ? gameObject.activeInHierarchy : false; } }

    public CustomSecondPanel(Transform trans)
    {
        if (trans == null)
        {
            Logger.LogError("new custom panel is null,please check out");
            return;
        }

        transform = trans;
        gameObject = trans.gameObject;
        rectTransform = trans.rectTransform();
        
        InitComponent();
        AddEvent();
    }

    public virtual void InitComponent() { }

    public virtual void AddEvent() { }

    public virtual void RefreshButtonMark() { }

    public void OnBecameVisible()
    {
        if (!activeInHierarchy) return;

        _OnBecameVisible();
    }

    protected virtual void _OnBecameVisible() { }

    public virtual void Destory() { }

    public virtual void OnReturnClick() { }
    
    public virtual void SetPanelVisible(bool visible = true)
    {
        if (gameObject == null) return;

        gameObject.SetActive(visible);
        if (visible)
        {
            RefreshButtonMark();
            OnBecameVisible();
        }
    }

    public virtual void Update() { }
}
#endregion

public class Window_Bordlands : Window
{
    #region custom ui panel class

    /// <summary>
    /// 制裁界面
    /// </summary>
    public class SanctionPanel : CustomSecondPanel
    {
        private Button m_sanctionBtn;

        private Button m_quitBtn;

        public PNmlMonster monsterData { get; set; }

        public SanctionPanel(Transform trans) : base(trans) { }

        public override void InitComponent()
        {
            base.InitComponent();
            m_sanctionBtn = rectTransform.Find("sanction_btn").GetComponent<Button>();
            m_quitBtn = rectTransform.Find("quit_btn").GetComponent<Button>();
        }

        public override void AddEvent()
        {
            base.AddEvent();
            EventTriggerListener.Get(m_sanctionBtn.gameObject).onClick = OnSanctionBtnClick;
            EventTriggerListener.Get(m_quitBtn.gameObject).onClick     = OnQuitBtnClick;
        }

        private void OnSanctionBtnClick(GameObject sender)
        {
            StageInfo info = ConfigManager.Get<StageInfo>(monsterData.stageId);
            if (info == null)
            {
                Logger.LogError("id = {0} stage info is null ,please check out!!", monsterData.stageId);
                return;
            }
            moduleBordlands.SendEnterStage(monsterData.uid, monsterData.stageId);
            moduleBordlands.lastChallengeMonster = monsterData;
        }

        public void OnQuitBtnClick(GameObject sender)
        {
            SetPanelVisible(false);
        }
    }

    /// <summary>
    /// 侦查界面
    /// </summary>
    public class DetectPanel : CustomSecondPanel
    {
        private Button m_detectBtn;
        private Button m_quitBtn;
        private Text m_costNumText;
        private Text m_restNumText;
        private string m_restNumTip;
        private Image m_costImage;
        private Text m_remainText;
        private string m_remainTip;

        public DetectPanel(Transform trans) : base(trans) { }

        public override void InitComponent()
        {
            base.InitComponent();

            m_detectBtn     = transform.GetComponent<Button>("detect_btn"); 
            m_quitBtn       = transform.GetComponent<Button>("quit_btn");
            m_costNumText   = transform.GetComponent<Text>("cost/cost_number");
            m_restNumText   = transform.GetComponent<Text>("rest_times"); 
            m_costImage     = transform.GetComponent<Image>("cost/Image"); 
            m_remainText    = transform.GetComponent<Text>("cost/remain");

            m_restNumTip      = ConfigText.GetDefalutString(TextForMatType.BorderlandUIText, 14);
            m_remainTip       = ConfigText.GetDefalutString(TextForMatType.BorderlandUIText, 28);
        }

        public override void AddEvent()
        {
            base.AddEvent();

            EventTriggerListener.Get(m_detectBtn.gameObject).onClick = OnDetectBtnClick;
            EventTriggerListener.Get(m_quitBtn.gameObject).onClick = OnQuitBtnClick;
        }

        private void OnDetectBtnClick(GameObject sender)
        {
            if (moduleBordlands.isHasAnyBoss)
            {
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.BorderlandUIText,34));
                return;
            }


            PItem2 cost = moduleBordlands.detectCostDic.Get(moduleBordlands.currentDetectTimes);
            //次数满了
            if (cost == null)
            {
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.BorderlandUIText, 35));
                return;
            }
            
            PropItemInfo prop = ConfigManager.Get<PropItemInfo>(cost.itemTypeId);
            if (prop == null)
            {
                Logger.LogError("detect cost type error,id = {0}", cost.itemTypeId);
                return;
            }
            uint remian = modulePlayer.GetMoneyCount((CurrencySubType)prop.subType);
            if(cost.num > remian)
            {
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.BorderlandUIText, 36));
                return;
            }

            moduleBordlands.SendRefreshBoss();
        }

        public void OnQuitBtnClick(GameObject sender)
        {
            SetPanelVisible(false);
        }

        public override void SetPanelVisible(bool visible = true)
        {
            base.SetPanelVisible(visible);

            if(visible) RefreshText();
        }

        private void RefreshText()
        {
            m_detectBtn.interactable = false;
            m_restNumText.text = Util.Format(m_restNumTip, moduleBordlands.detectCostDic.Count - moduleBordlands.currentDetectTimes);

            uint remian = modulePlayer.GetMoneyCount(CurrencySubType.Diamond);
            m_remainText.text = Util.Format(m_remainTip, remian);

            PItem2 cost = moduleBordlands.detectCostDic.Get(moduleBordlands.currentDetectTimes);
            //次数满了
            if(cost == null)
            {
                //读取上一条数据
                PItem2 lastCost = moduleBordlands.detectCostDic.Get(moduleBordlands.currentDetectTimes - 1);
                if(lastCost != null)
                {
                    string ss = Util.Format(ConfigText.GetDefalutString(TextForMatType.BorderlandUIText, 13), lastCost.num);
                    Util.SetText(m_costNumText, lastCost.num <= remian ? ss : GeneralConfigInfo.GetNoEnoughColorString(ss));
                    m_detectBtn.interactable = false;
                }
                return;
            }

            PropItemInfo prop = ConfigManager.Get<PropItemInfo>(cost.itemTypeId);
            if (prop == null)
            {
                Logger.LogError("detect cost type error,id = {0}", cost.itemTypeId);
                return;
            }

            AtlasHelper.SetCurrencyIcon(m_costImage, (CurrencySubType)prop.subType);
            remian = modulePlayer.GetMoneyCount((CurrencySubType)prop.subType);
            Util.SetText(m_remainText, m_remainTip, remian);
            bool detectBtnVisible = cost.num <= remian && !moduleBordlands.isHasAnyBoss;
            m_detectBtn.interactable = detectBtnVisible;
            string str = Util.Format(ConfigText.GetDefalutString(TextForMatType.BorderlandUIText, 13), cost.num);
            Util.SetText(m_costNumText, cost.num <= remian ? str : GeneralConfigInfo.GetNoEnoughColorString(str));
        }
    }
    #endregion

    public const float MOVEPAD_THRESHOLD = 0.15f;

    private GameObject m_oldRoleObj;
    private Button m_detectBtn,m_helpBtn,m_returnBtn,m_chatBtn,m_hidePlayerBtn,m_showPlayerBtn;

    private UIMovementPad m_movementPad;
    private CanvasRenderer m_leftCR,m_rightCR,m_upCR,m_bottomCR;

    private Text m_scoreText;
    private Button m_gradeRankBtn;
    private Image m_gradeIcon;
    private Transform m_petIcon;

    private GameObject m_unionShow;
    private Text m_unionScore;
    private Image m_unionIcon;
    private GameObject m_unionHint;
    
    private SanctionPanel m_sanctionPanel;
    private DetectPanel m_detectPanel;

    private int m_lastMoveKey;

    #region init
    protected override void OnOpen()
    {
        m_oldRoleObj        = GetComponent<RectTransform>("top/role").gameObject;
        m_detectBtn         = GetComponent<Button>("top/detect_btn");
        m_helpBtn           = GetComponent<Button>("top/help_btn");
        m_returnBtn         = GetComponent<Button>("top/exit_btn");
        m_chatBtn           = GetComponent<Button>("bottom/chat_btn");
        m_showPlayerBtn     = GetComponent<Button>("bottom/show_player_btn");
        m_hidePlayerBtn     = GetComponent<Button>("bottom/hide_player_btn");

        m_scoreText         = GetComponent<Text>("top/gradegroup/grade/grade_text");
        m_gradeIcon         = GetComponent<Image>("top/gradegroup/grade/icon");
        m_gradeRankBtn      = GetComponent<Button>("top/rank_btn");

        m_unionScore        = GetComponent<Text>("top/gradegroup/uniongrade/grade_text");
        m_unionIcon         = GetComponent<Image>("top/gradegroup/uniongrade/icon");
        m_unionShow         = GetComponent<RectTransform>("top/gradegroup/uniongrade").gameObject;
        m_unionHint         = GetComponent<RectTransform>("top/rank_btn/hint").gameObject;

        m_movementPad       = GetComponent<UIMovementPad>("bottom/movementPad");
        m_leftCR            = GetComponent<CanvasRenderer>("bottom/movementPad/left");
        m_rightCR           = GetComponent<CanvasRenderer>("bottom/movementPad/right");
        m_upCR              = GetComponent<CanvasRenderer>("bottom/movementPad/up");
        m_bottomCR          = GetComponent<CanvasRenderer>("bottom/movementPad/bottom");
        m_petIcon           = GetComponent<Transform>("top/sprite_avatar");

        SetCanvasRenderAlpha(0.6f);
        SetHidePlayerBtnVisible(moduleBordlands.playerVisible);

        //二级面板
        Transform t = transform.Find("sanction_panel");
        m_sanctionPanel = new SanctionPanel(t);
        t = transform.Find("detect_panel");
        m_detectPanel = new DetectPanel(t);
        t = transform.Find("reward_panel");
        m_sanctionPanel.SetPanelVisible(false);
        m_detectPanel.SetPanelVisible(false);
        
        EventTriggerListener.Get(m_detectBtn.gameObject).onClick     = OnDetectBtnClick;
        EventTriggerListener.Get(m_helpBtn.gameObject).onClick       = OnHelpBtnClick;
        EventTriggerListener.Get(m_returnBtn.gameObject).onClick     = OnReturnBtnClick;
        EventTriggerListener.Get(m_chatBtn.gameObject).onClick       = OnChatBtnClick;
        EventTriggerListener.Get(m_showPlayerBtn.gameObject).onClick = OnShowPlayerBtnClick;
        EventTriggerListener.Get(m_hidePlayerBtn.gameObject).onClick = OnHidePlayerBtnClick;
        EventTriggerListener.Get(m_gradeRankBtn.gameObject).onClick  = OnGradeBtnClick;

        m_movementPad.onTouchMove.AddListener(OnMovementPadMove);
        m_movementPad.onTouchEnd.AddListener(OnMovementPadStop);

        m_oldRoleObj.SafeSetActive(false);
        CheckOpenRewardPanel();
        CheckOpenQuitPanel();

        enableUpdate = true;
        InitializeText();
        RefreshGradeIcon();
    }

    private void InitializeText()
    {
        var t = ConfigManager.Get<ConfigText>((int)TextForMatType.BorderlandUIText);
        if (!t) return;

        Util.SetText(GetComponent<Text>("top/role/lv_back/lv"), t[0]);
        Util.SetText(GetComponent<Text>("top/exit_btn/Text"), t[1]);
        Util.SetText(GetComponent<Text>("top/help_btn/Text"), t[2]);
        Util.SetText(GetComponent<Text>("top/detect_btn/Text"), t[3]);
        Util.SetText(GetComponent<Text>("top/gradegroup/grade/tip"), t[31]);
        Util.SetText(GetComponent<Text>("top/gradegroup/uniongrade/tip"), t[32]);
        //Util.SetText(GetComponent<Text>("top/grade/rank_btn/Text"), t[32]);
        Util.SetText(GetComponent<Text>("bottom/chat_btn/Text"), t[4]);
        Util.SetText(GetComponent<Text>("bottom/show_player_btn/Text"), t[5]);
        Util.SetText(GetComponent<Text>("bottom/hide_player_btn/Text"), t[6]);
        Util.SetText(GetComponent<Text>("sanction_panel/tip_prop/top/equipinfo"), t[29]);
        Util.SetText(GetComponent<Text>("sanction_panel/tip_prop/top/english"), t[8]);
        Util.SetText(GetComponent<Text>("sanction_panel/content"), t[9]);
        Util.SetText(GetComponent<Text>("sanction_panel/sanction_btn/Text"), t[10]);
        Util.SetText(GetComponent<Text>("detect_panel/tip_prop/top/equipinfo"), t[30]);
        Util.SetText(GetComponent<Text>("detect_panel/tip_prop/top/english"), t[8]);
        Util.SetText(GetComponent<Text>("detect_panel/content"), t[11]);
        Util.SetText(GetComponent<Text>("detect_panel/cost/cost"), t[12]);
        Util.SetText(GetComponent<Text>("detect_panel/cost/cost_number"), t[13]);
        Util.SetText(GetComponent<Text>("detect_panel/rest_times"), t[14]);
        Util.SetText(GetComponent<Text>("detect_panel/detect_btn/Text"), t[15]);
        //Util.SetText(GetComponent<Text>("quit_panel/tip_prop/top/equipinfo"), t[1]);
        Util.SetText(GetComponent<Text>("quit_panel/tip_prop/top/english"), t[8]);
        Util.SetText(GetComponent<Text>("quit_panel/content"), t[16]);
        Util.SetText(GetComponent<Text>("quit_panel/time"), t[17]);
        Util.SetText(GetComponent<Text>("quit_panel/confirm_btn/Text"), t[18]);
        Util.SetText(GetComponent<Text>("reward_panel/reward_tip/tip2"), t[19]);
        Util.SetText(GetComponent<Text>("reward_panel/reward_show/signsucced/up"), t[20]);
        Util.SetText(GetComponent<Text>("reward_panel/reward_show/reward/item/level_bg/level"), t[21]);
        Util.SetText(GetComponent<Text>("reward_panel/reward_show/reward/item/name"), t[22]);
        Util.SetText(GetComponent<Text>("reward_panel/reward_show/notice"), t[23]);
        Util.SetText(GetComponent<Text>("rule_panel/info/title"), t[24]);
        Util.SetText(GetComponent<Text>("rule_panel/info/inner/wish_rules/Viewport/Content/rulestitle"), t[25]);
        Util.SetText(GetComponent<Text>("rule_panel/info/inner/wish_rules/Viewport/Content/rulescontent"), t[26]);

        Util.SetText(GetComponent<Text>("top/uniongrade/tip"), ConfigText.GetDefalutString(232, 11));
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault(0, false);

        Util.SetText(m_scoreText, moduleBordlands.selfScore.ToString());

        m_unionHint.gameObject.SetActive(false);
        m_unionShow.gameObject.SetActive(false);
        if (modulePlayer.roleInfo.leagueID != 0) moduleUnion.GetSelfIntergel();
    }
    #endregion

    #region button click event

    private void OnDetectBtnClick(GameObject sender)
    {
        m_detectPanel.SetPanelVisible(true);
    }

    private void OnHelpBtnClick(GameObject sender)
    {

    }

    private void OnReturnBtnClick(GameObject sender)
    {
        Window_Alert.ShowAlertDefalut(ConfigText.GetDefalutString(TextForMatType.AlertUIText,5), () =>
        {
            moduleBordlands.SendExitBordland();
        }, () => { });
    }

    private void OnChatBtnClick(GameObject sender)
    {
        moduleChat.opChatType = OpenWhichChat.WorldChat;
        ShowAsync<Window_Chat>();
    }

    private void OnShowPlayerBtnClick(GameObject sender)
    {
        SetPlayerVisible(true);
    }

    private void OnHidePlayerBtnClick(GameObject sender)
    {
        SetPlayerVisible(false);
    }

    private void SetPlayerVisible(bool isVisible)
    {
        SetHidePlayerBtnVisible(isVisible);
        moduleBordlands.SetPlayerVisible(isVisible);
    }

    private void OnGradeBtnClick(GameObject sender)
    {
        ShowAsync<Window_Bordrank>();
        moduleUnion.GetAllIntergel();
        if (moduleUnion.m_selfInter == null) return;
        if (modulePlayer.roleInfo.leagueID != 0 && moduleUnion.m_selfInter.intergl>0)
        {
            m_unionHint.gameObject.SetActive(false);
            var keyStr = "unionRank" + modulePlayer.id_;
            PlayerPrefs.SetString(keyStr, Util.GetServerLocalTime().ToString());
        }
    }

    #endregion

    #region touch event

    private void OnMovementPadMove(Vector2 dir, Vector2 delta)
    {
        moduleBordlands.moveMentKey = 0x00;
        if (dir.x < -MOVEPAD_THRESHOLD) moduleBordlands.moveMentKey |= 0x02;
        if (dir.x >  MOVEPAD_THRESHOLD) moduleBordlands.moveMentKey |= 0x08;
        if (dir.y >  MOVEPAD_THRESHOLD) moduleBordlands.moveMentKey |= 0x01;
        if (dir.y < -MOVEPAD_THRESHOLD) moduleBordlands.moveMentKey |= 0x04;

        if (m_lastMoveKey == moduleBordlands.moveMentKey) return;
        
        m_lastMoveKey = moduleBordlands.moveMentKey;
        if (moduleBordlands.moveMentKey != 0)
        {
            m_leftCR.   SetAlpha(dir.x < -MOVEPAD_THRESHOLD ? 1 : 0.6f);
            m_rightCR.  SetAlpha(dir.x >  MOVEPAD_THRESHOLD ? 1 : 0.6f);
            m_upCR.     SetAlpha(dir.y >  MOVEPAD_THRESHOLD ? 1 : 0.6f);
            m_bottomCR. SetAlpha(dir.y <- MOVEPAD_THRESHOLD ? 1 : 0.6f);
        }
    }

    private void OnMovementPadStop(Vector2 p)
    {
        SetCanvasRenderAlpha(0.6f);
        moduleBordlands.moveMentKey = 0;
        m_lastMoveKey = 0;
    }

    private void SetCanvasRenderAlpha(float alpha)
    {
        m_leftCR.SetAlpha(alpha);
        m_rightCR.SetAlpha(alpha);
        m_upCR.SetAlpha(alpha);
        m_bottomCR.SetAlpha(alpha);
    }

    #endregion
    
    #region ui logic
    private void RefreshGradeIcon()
    {
        var info = ConfigManager.Find<PropItemInfo>(o=>o.itemType == PropType.BordGrade);
        if (info) AtlasHelper.SetItemIcon(m_gradeIcon, info);

        if (modulePet.FightingPet != null)
        {
            Util.SetPetSimpleInfo(this.m_petIcon, modulePet.FightingPet);
            this.m_petIcon.SafeSetActive(true);
        }
        else
            this.m_petIcon.SafeSetActive(false);
    }

    private void SetHidePlayerBtnVisible(bool visible)
    {
        m_showPlayerBtn.gameObject.SetActive(!visible);
        m_hidePlayerBtn.gameObject.SetActive(visible);
    }

    private void CheckOpenRewardPanel()
    {
        if(moduleBordlands.bordlandsSettlementReward != null && moduleBordlands.bordlandsSettlementReward.rewardList.Length > 0)
        {
            ShowAsync<Window_RandomReward>(null,(w)=>{
                var rewardWindow = w as Window_RandomReward;
                rewardWindow.RefreshReward(moduleBordlands.bordlandsSettlementReward);
                //使用完成后就清除，避免下次再使用
                moduleBordlands.bordlandsSettlementReward = null;
            });
        }
    }
    
    private void CheckOpenQuitPanel()
    {
        var rewardWindow = GetOpenedWindow<Window_RandomReward>();

        if (!moduleBordlands.isValidBordland && (!rewardWindow || !rewardWindow.actived))
        {
            string msg = ConfigText.GetDefalutString(TextForMatType.BorderlandUIText, 16);
            Window_Alert.ShowAlert(msg,true,false,false,()=> { Game.GoHome();});
        }
    }
    #endregion

    #region union score
    private void SetUnionIntegral()
    {
        m_unionHint.gameObject.SetActive(false);
        m_unionShow.gameObject.SetActive(false);
        if (modulePlayer.roleInfo.leagueID != 0)
        {
            var info = ConfigManager.Find<PropItemInfo>(o => o.itemType == PropType.BordGrade);
            if (info) AtlasHelper.SetItemIcon(m_unionIcon, info);

            var keyStr = "unionRank" + modulePlayer.id_;
            bool firstOpen = moduleWelfare.FrirstOpen(keyStr, false);
   
            if (moduleUnion.m_selfInter == null) return;
            if (firstOpen && moduleUnion.m_selfInter.intergl > 0)
            {
                m_unionHint.gameObject.SetActive(true);
            }

            if (moduleUnion.m_selfInter != null)
            {
                m_unionShow.gameObject.SetActive(true);
                m_unionScore.text = moduleUnion.m_selfInter.intergl.ToString();
            }
        }
    }
    void _ME(ModuleEvent<Module_Union> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Union.EventUnionInterglSelf:
                SetUnionIntegral();
                break;
        }
    }
    #endregion

    #region module event
    private void _ME(ModuleEvent<Module_Bordlands> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Bordlands.EventDetectMonster:
                m_detectPanel.OnQuitBtnClick(null);
                break;

            case Module_Bordlands.EventFightMonster:
                m_sanctionPanel.OnQuitBtnClick(null);
                ScNmlSceneFightMonster fight = e.msg as ScNmlSceneFightMonster;
                if (fight.result != 0)
                {
                    //todo 错误显示
                    Logger.LogError("fight error ,result is {0}",fight.result);
                    return;
                }
                break;

            case Module_Bordlands.EventBordlandOver: CheckOpenQuitPanel(); break;
            case Module_Bordlands.EventRefreshSelfRank: m_scoreText.text = moduleBordlands.selfScore.ToString(); break;
            case Module_Bordlands.EventClickScenePlayerSuccess:
                if (!actived) return;
                BordlandsCreature bc = e.param1 as BordlandsCreature;
                if(bc && bc.monsterData != null)
                {
                    if(bc.isInBattle)
                    {
                        string msg = ConfigText.GetDefalutString(TextForMatType.BorderlandUIText, 27);
                        moduleGlobal.ShowMessage(msg);
                    }
                    else
                    {
                        m_sanctionPanel.SetPanelVisible(true);
                        m_sanctionPanel.monsterData = bc.monsterData;
                    }
                }
                break;

            case Module_Bordlands.EventShowRewardOver: CheckOpenQuitPanel(); break;
        }
    }
    #endregion
}
