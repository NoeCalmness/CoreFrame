// /**************************************************************************************************
//  * Copyright (C) 2017-2018 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-04-30      9:39
//  *LastModify：2019-04-30      9:39
//  ***************************************************************************************************/

using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Window_FactionBattle : Window
{
    private Slider m_factionScore;
    private Text m_redScore;
    private Text m_blueScore;
    private Text m_redName;
    private Text m_blueName;
    private Text m_countDownText;
    private Text m_selfFaction;
    private Text m_selfRank;
    private Text m_selfScore;
    private Text m_selfComboKill;
    private Text m_selfMaxComboKill;
    private Image m_selfApplique;
    private Text m_reliveCountDown;
    private Transform m_reliveRoot;
    private ScrollView m_redRank;
    private ScrollView m_blueRank;
    private Transform m_messageRoot;
    private Transform m_messageTemp;
    private Text m_matchText;
    private Text m_commonHintText;
    private Button m_matchButton;
    private Button m_rewardButton;
    private Button m_ruleButton;
    private Button m_chatButton;
    private Transform m_rewardRoot;
    private ScrollRect m_scrollRect;
    private Transform m_ruleRoot;
    /// <summary>
    /// 0-倒计时 1-跳转结束
    /// </summary>
    private Transform[] m_countDown;

    private FactionWindow_Result m_resultWindow;

    private float m_matchTime;

    private DataSource<PFactionPlayerInfo> m_redDataSource;
    private DataSource<PFactionPlayerInfo> m_blueDataSource;

    private Queue<GameObject> m_messageList = new Queue<GameObject>();

    protected override void OnOpen()
    {
        base.OnOpen();
        InitComponents();
        MultLanguage();

        m_matchButton?.onClick.AddListener(OnMatchButtonClick);
        m_rewardRoot?.GetComponentDefault<PreviewRewards>();
        m_rewardButton?.onClick.AddListener(() =>
        {
            m_rewardRoot.SafeSetActive(true);
        });
        m_ruleButton.onClick.AddListener(() =>
        {
            m_ruleRoot.gameObject.GetComponentDefault<FactionRuleBehaviour>("right");
            m_ruleRoot.gameObject.SetActive(true);
        });
        m_chatButton.onClick.AddListener(() =>
        {
            moduleChat.opChatType = OpenWhichChat.FactionChat;
            ShowAsync("window_chat");
        });

        m_messageTemp.SafeSetActive(false);
        Util.SetText(m_selfComboKill,    string.Empty);
        Util.SetText(m_selfFaction,      string.Empty);
        Util.SetText(m_selfRank,         string.Empty);
        Util.SetText(m_selfScore,        string.Empty);
        Util.SetText(m_selfMaxComboKill, string.Empty);
    }

    private void OnMatchButtonClick()
    {
        if (moduleFactionBattle.IsMatchCooling)
        {
            moduleGlobal.ShowMessage((int) TextForMatType.FactionBattleUI, 5);
            return;
        }
        moduleFactionBattle.RequestMatch(!moduleFactionBattle.Matching);
    }

    private void InitComponents()
    {
        m_factionScore  = GetComponent<Slider>("factionbattle/title/slider");
        m_redScore      = GetComponent<Text>("factionbattle/title/left/score");
        m_blueScore     = GetComponent<Text>("factionbattle/title/right/score");
        m_redName       = GetComponent<Text>("factionbattle/title/left/faction_name");
        m_blueName      = GetComponent<Text>("factionbattle/title/right/faction_name");
        m_countDownText = GetComponent<Text>("factionbattle/title/count_down/time/time_txt");
        m_redRank       = GetComponent<ScrollView>("factionbattle/middle_panel/left_scrollview");
        m_blueRank      = GetComponent<ScrollView>("factionbattle/middle_panel/right_scrollview");
        m_messageRoot   = GetComponent<Transform>("factionbattle/bottom_panel/content/Viewport/Content");
        m_messageTemp   = GetComponent<Transform>("factionbattle/bottom_panel/content/Viewport/Text");
        m_scrollRect    = GetComponent<ScrollRect>("factionbattle/bottom_panel/content");
        m_matchButton   = GetComponent<Button>("factionbattle/bottom_panel/start_btn");
        m_rewardButton  = GetComponent<Button>("factionbattle/bottom_panel/reward_btn");
        m_ruleButton    = GetComponent<Button>("factionbattle/bottom_panel/rule_btn");
        m_matchText     = GetComponent<Text>("factionbattle/bottom_panel/start_btn/Text");
        m_chatButton    = GetComponent<Button>("chat");

        m_selfApplique  = GetComponent<Image>("factionbattle/bottom_panel/settlement/degree_txt/degree/kill_icon");
        m_selfFaction   = GetComponent<Text>("factionbattle/bottom_panel/settlement/myfaction");
        m_selfRank      = GetComponent<Text>("factionbattle/bottom_panel/settlement/rank");
        m_selfScore     = GetComponent<Text>("factionbattle/bottom_panel/settlement/score");
        m_selfComboKill = GetComponent<Text>("factionbattle/bottom_panel/settlement/degree_txt/txt");
        m_selfMaxComboKill = GetComponent<Text>("factionbattle/bottom_panel/settlement/degree_txt/degree/Text");
        m_reliveCountDown  = GetComponent<Text>("factionbattle/resurrect/Text");
        m_commonHintText   = GetComponent<Text>("factionbattle/resurrect/Text (1)");

        m_rewardRoot = GetComponent<Transform>("reward_panel");
        m_ruleRoot = GetComponent<Transform>("rule_panel");

        m_countDown = new Transform[2];
        m_countDown[0] = GetComponent<Transform>("factionbattle/title/count_down/time");
        m_countDown[1] = GetComponent<Transform>("factionbattle/title/count_down/over");

        m_reliveRoot = GetComponent<Transform>("factionbattle/resurrect");

        m_resultWindow = SubWindowBase.CreateSubWindow<FactionWindow_Result>(this, GetComponent<Transform>("factionbattle/result"));
    }

    private void MultLanguage()
    {
        Util.SetText(m_redName, Module_FactionBattle.FactionName(Module_FactionBattle.Faction.Red));
        Util.SetText(m_blueName, Module_FactionBattle.FactionName(Module_FactionBattle.Faction.Blue));
        Util.SetText(GetComponent<Text>("factionbattle/title/count_down/over/over_txt"), ConfigText.GetDefalutString(TextForMatType.FactionBattleUI, 6));
        Util.SetText(GetComponent<Text>("factionbattle/bottom_panel/rule_btn/Text"    ), ConfigText.GetDefalutString(TextForMatType.FactionBattleUI, 7));
        Util.SetText(GetComponent<Text>("factionbattle/bottom_panel/reward_btn/Text"  ), ConfigText.GetDefalutString(TextForMatType.FactionBattleUI, 8));

        Util.SetText(GetComponent<Text>("rule_panel/right/time_txt"), moduleFactionBattle.ActiveTime);
        Util.SetText(GetComponent<Text>("rule_panel/right/rule_txt"), ConfigText.GetDefalutString(TextForMatType.FactionSignUI, 3));

        var ct = ConfigManager.Get<ConfigText>((int)TextForMatType.FactionSignUI);
        if (null == ct)
            return;
        Util.SetText(GetComponent<Text>("rule_panel/right/titile01/Text"), ct[5]);
        Util.SetText(GetComponent<Text>("rule_panel/right/titile01/Text"), ct[6]);

    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();
        Level.current.mainCamera.enabled = false;
        base.OnBecameVisible(oldState, forward);

        moduleFactionBattle.RequestBattleInfo();
        moduleFactionBattle.RequestInWindow(true);

        if (moduleFactionBattle.AutoMatch)
        {
            moduleFactionBattle.AutoMatch = false;
            OnMatchButtonClick();
        }

        for (var i = 0; i < moduleFactionBattle.messageList.Count; i++)
        {
            RefreshMessage(moduleFactionBattle.messageList[i]);
        }
    }

    protected override void OnHide(bool forward)
    {
        base.OnHide(forward);
        if(!forward)
            Level.current.mainCamera.enabled = true;

        if (moduleFactionBattle.Matching)
            moduleFactionBattle.RequestMatch(false);

        Util.ClearChildren(m_messageRoot);
        m_messageList?.Clear();

        if(!forward)
            SendQuitWindow();
    }

    protected override void OnClose()
    {
        base.OnClose();
        if(Level.current is Level_Home && Level.current.mainCamera)
            Level.current.mainCamera.enabled = true;

        if (moduleFactionBattle.Matching)
            moduleFactionBattle.RequestMatch(false);

        if (!actived)
            return;
        
        SendQuitWindow();
    }

    protected override void OnReturn()
    {
        if (m_rewardRoot.gameObject.activeSelf)
        {
            m_rewardRoot.gameObject.SetActive(false);
            return;
        }

        if (m_ruleRoot.gameObject.activeSelf)
        {
            m_ruleRoot.gameObject.SetActive(false);
            return;
        }

        base.OnReturn();
    }

    private void SendQuitWindow()
    {
        if (Level.next != null && Level.next is Level_Battle)
            return;

        if (Level.current != null && Level.current is Level_Battle)
            return;

        moduleFactionBattle.RequestInWindow(false);
    }

    public override void OnRenderUpdate()
    {
        base.OnRenderUpdate();

        if(moduleFactionBattle.IsProcessing)
            Util.SetText(m_countDownText, moduleFactionBattle.CountDown);
        

        var cooling = moduleFactionBattle.IsMatchCooling;
        var matching = moduleFactionBattle.Matching;
        m_reliveRoot.SafeSetActive(cooling || matching);
        if (cooling)
        {
            Util.SetText(m_commonHintText, ConfigText.GetDefalutString(TextForMatType.FactionBattleUI, 9));
            Util.SetText(m_reliveCountDown, moduleFactionBattle.MatchCoolTime);
        }
        else if (matching)
        {
            Util.SetText(m_commonHintText, ConfigText.GetDefalutString(TextForMatType.FactionBattleUI, 12));
            Util.SetText(m_reliveCountDown, Util.GetTimeFromSec(Mathf.RoundToInt(Time.realtimeSinceStartup - m_matchTime), ":"));
        }

        m_matchButton.SetInteractable(moduleFactionBattle.state == Module_FactionBattle.State.Processing && !moduleFactionBattle.IsMatchCooling && moduleFactionBattle.IsSignIn);

    }

    private void _ME(ModuleEvent<Module_FactionBattle> e)
    {
        switch (e.moduleEvent)
        {
            case Module_FactionBattle.EventBattleInfosChange:
                RefreshBattleInfos();
                RefreshBattleState();
                break;
            case Module_FactionBattle.EventStateChange:
                RefreshBattleState();
                break;
            case Module_FactionBattle.EventMessageListChange:
                RefreshMessage((string)e.param1);
                break;
            case Module_FactionBattle.EventSelfInfoChange:
                RefreshSelfInfo();
                break;
            case Module_FactionBattle.ResponseStartMatch:
                RefreshMatchButton();
                m_matchTime = Time.realtimeSinceStartup;
                break;
        }
    }

    void _ME(ModuleEvent<Module_Match> e)
    {
        if (!actived)
            return;
        switch (e.moduleEvent)
        {
            case Module_Match.EventMatchInfo:
                Window.SetWindowParam<Window_FactionStart>(e.msg as ScMatchInfo);
                break;
        }
    }
    
    void _ME(ModuleEvent<Module_PVP> e)
    {
        if (!actived)
            return;
        switch (e.moduleEvent)
        {
            case Module_PVP.EventLoadingStart:
                Window.ShowAsync<Window_FactionStart>();
                moduleFactionBattle.Matching = false;
                break;
            default: break;
        }
    }

    private void RefreshMatchButton()
    {
        Util.SetText(m_matchText, ConfigText.GetDefalutString(TextForMatType.FactionBattleUI, 
            moduleFactionBattle.IsSignIn 
            ?   moduleFactionBattle.state == Module_FactionBattle.State.End 
                ? 4 
                : moduleFactionBattle.Matching ? 3 : 2
            :11));
    }

    private void RefreshBattleState()
    {
        RefreshMatchButton();

        m_countDown[0].SafeSetActive(moduleFactionBattle.state == Module_FactionBattle.State.Processing);
        m_countDown[1].SafeSetActive(moduleFactionBattle.state == Module_FactionBattle.State.End);

        if (moduleFactionBattle.state == Module_FactionBattle.State.Settlement)
        {
            if(moduleFactionBattle.SelfFaction != Module_FactionBattle.Faction.None)
                m_resultWindow.Initialize();
        }
        else if (moduleFactionBattle.state == Module_FactionBattle.State.End)
        {
            if(moduleFactionBattle.SelfFaction != Module_FactionBattle.Faction.None)
                m_resultWindow.Initialize(true);
        }
        else if (moduleFactionBattle.state < Module_FactionBattle.State.Processing)
        {
            Hide();
        }
    }

    private void RefreshSelfInfo()
    {
        Util.SetText(m_selfFaction, moduleFactionBattle.SelfFactionName);
        Util.SetText(m_selfRank, Module_FactionBattle.GetRankLabel(moduleFactionBattle.SelfRank));
        Util.SetText(m_selfScore, moduleFactionBattle.SelfScore.ToString());
        Util.SetText(m_selfComboKill, Module_FactionBattle.GetKillString(moduleFactionBattle.ComboKill));
        Util.SetText(m_selfMaxComboKill, Module_FactionBattle.GetKillString(moduleFactionBattle.MaxComboKill));
        m_selfMaxComboKill?.transform.parent.SafeSetActive(!string.IsNullOrEmpty(m_selfMaxComboKill?.text));
        var info = ConfigManager.Get<FactionKillRewardInfo>(moduleFactionBattle.MaxComboKill);
        if (!string.IsNullOrEmpty(info?.applique))
            AtlasHelper.SetIcons(m_selfApplique, info.applique);
    }

    private void RefreshBattleInfos()
    {
        SliderAnimControlCenter.PlayUniformSpeed<SliderAnim3>(m_factionScore, m_factionScore.value, moduleFactionBattle.FactionPower,1);

        var redInfo = moduleFactionBattle.GetFactionInfo(Module_FactionBattle.Faction.Red);
        if (redInfo != null)
        {
            Util.SetText(m_redScore, redInfo.score.ToString());
            var list = new List<PFactionPlayerInfo>(redInfo.members);
            list.Sort((a, b) => a.battleInfo.rank.CompareTo(b.battleInfo.rank));
            if (m_redDataSource == null)
                m_redDataSource = new DataSource<PFactionPlayerInfo>(list, m_redRank, OnSetData);
            else
                m_redDataSource.SetItems(list);
        }

        var blueInfo = moduleFactionBattle.GetFactionInfo(Module_FactionBattle.Faction.Blue);
        if (blueInfo != null)
        {
            Util.SetText(m_blueScore, blueInfo.score.ToString());

            var list = new List<PFactionPlayerInfo>(blueInfo.members);
            list.Sort((a, b) => a.battleInfo.rank.CompareTo(b.battleInfo.rank));
            if (m_blueDataSource == null)
                m_blueDataSource = new DataSource<PFactionPlayerInfo>(list, m_blueRank, OnSetData);
            else
                m_blueDataSource.SetItems(list);
        }
    }

    private void OnSetData(RectTransform node, PFactionPlayerInfo data)
    {
        var b = node.GetComponentDefault<FactionRankBehaviour>();
        b.BindData(data);
    }

    private void RefreshMessage(string msg)
    {
        var t = m_messageRoot.AddNewChild(m_messageTemp);
        Util.SetText(t.gameObject, msg);
        t.SafeSetActive(true);

        m_messageList.Enqueue(t.gameObject);

        if (m_messageList.Count > Module_FactionBattle.MESSAGE_MAX)
            Object.Destroy(m_messageList.Dequeue());

        if (m_scrollRect != null)
        {
            DelayEvents.Add(() =>
            {
                DOTween.To(() => m_scrollRect.verticalNormalizedPosition,
                    v => m_scrollRect.verticalNormalizedPosition = v, 0, 0.3f);
            }, 0.1f);
        }
    }
}
