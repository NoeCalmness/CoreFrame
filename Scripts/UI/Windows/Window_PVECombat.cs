/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-01-23
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class Window_PVECombat : Window
{
    public PveCombat_CountDown countDownPanel;

    #region custom class

    /// <summary>
    /// boss出场提示
    /// </summary>
    public class BossNotice : CustomSecondPanel
    {
        public sealed class BossNoticeItem : CustomSecondPanel
        {
            public Image icon;
            public Text name;
            public BossNoticeItem(Transform trans) : base(trans) { }

            public override void InitComponent()
            {
                base.InitComponent();
                icon = transform.GetComponent<Image>("bossavatar");
                name = transform.GetComponent<Text>("bossavatar/name");
            }

            public void RefreshBossItem(MonsterInfo info)
            {
                SetPanelVisible(info != null);
                if (!info) return;

                UIDynamicImage.LoadImage(icon.transform, info.montserLargeIcon);
                name.text = info.name;
            }
        }

        public BossNotice(Transform trans) : base(trans) { }

        private TweenBase m_tween;
        private GameObject m_avatatParent;
        private BossNoticeItem[] m_bossItems;

        public override void InitComponent()
        {
            base.InitComponent();

            m_tween = gameObject.GetComponent<TweenBase>();
            if (m_tween) m_tween.onComplete.AddListener(OnTweenComplete);
            Transform t = transform.Find("avatars");
            m_avatatParent = t.gameObject;
            m_bossItems = new BossNoticeItem[t.childCount];
            for (int i = 0; i < m_bossItems.Length; i++) m_bossItems[i] = new BossNoticeItem(t.GetChild(i));
        }

        public void RefreshBossNotice(int bossId1, int bossId2)
        {
            foreach (var item in m_bossItems)
            {
                item.SetPanelVisible(false);
            }

            MonsterInfo m = ConfigManager.Get<MonsterInfo>(bossId1);

            SetPanelVisible(m != null);
            m_avatatParent.SetActive(m != null);

            m_bossItems[0].RefreshBossItem(m);
            m = ConfigManager.Get<MonsterInfo>(bossId2);
            m_bossItems[1].RefreshBossItem(m);
            m_tween.PlayForward();
        }

        private void OnTweenComplete(bool reverse)
        {
            SetPanelVisible(false);
        }
    }

    /// <summary>
    /// 玩家复活面板
    /// </summary>
    public class PlayerRevivePanel : CustomSecondPanel
    {
        /// <summary>
        /// 放弃复活回调
        /// </summary>
        public Action onExitClick;

        /// <summary>
        /// 放弃复活回调
        /// </summary>
        public Action onCancelClick;

        /// <summary>
        /// 复活
        /// </summary>
        public Action onReviveClick;

        /// <summary>
        /// 上次复活的消费数量
        /// </summary>
        public int currentReviveCostCount { get; private set; }

        private Button m_reviveBtn;
        private GameObject m_cancelBtn, m_exitBtn;
        private Image m_costIcon;
        private Text m_costCountText, m_restReviveText;
        private string restCountTip, restReviveTip;
        private Text m_contentText;
        private PItem2 m_reviveCoinItem;

        public PlayerRevivePanel(Transform trans) : base(trans)
        {
        }

        public override void InitComponent()
        {
            base.InitComponent();

            m_reviveBtn = transform.Find("kuang/fuhuo").GetComponent<Button>();
            m_cancelBtn = transform.Find("kuang/closebutton").gameObject;
            m_exitBtn = transform.Find("kuang/fangqi").gameObject;
            m_costIcon = transform.Find("kuang/cost/icon").GetComponent<Image>();
            m_costCountText = transform.Find("kuang/cost/icon/now").GetComponent<Text>();
            m_restReviveText = transform.Find("kuang/fuhuoText").GetComponent<Text>();
            m_contentText = transform.Find("kuang/content_tip").GetComponent<Text>();
            InitData();
        }

        public override void AddEvent()
        {
            base.AddEvent();

            m_reviveBtn?.onClick.AddListener(OnReviveClick);
            EventTriggerListener.Get(m_cancelBtn).onClick = OnCancelClick;
            EventTriggerListener.Get(m_exitBtn).onClick = OnExitClick;
        }

        private void InitData()
        {
            currentReviveCostCount = 0;

            restReviveTip = ConfigText.GetDefalutString(TextForMatType.CombatUIText, 4);
            restCountTip = ConfigText.GetDefalutString(TextForMatType.CombatUIText, 6);

            var reviveCoin =
                ConfigManager.Find<PropItemInfo>(
                    o => o.itemType == PropType.Sundries && o.subType == (int) SundriesSubType.Revive);
            if (reviveCoin)
            {
                m_reviveCoinItem = PacketObject.Create<PItem2>();
                m_reviveCoinItem.itemTypeId = (ushort) reviveCoin.ID;
                m_reviveCoinItem.num = 1;
            }
        }

        public override void SetPanelVisible(bool visible = true)
        {
            base.SetPanelVisible(visible);

            if (!modulePVE.isTeamMode) modulePVEEvent.pauseCountDown = visible;
        }

        public bool OpenRevivePanel()
        {
            if (modulePVE.isStandalonePVE &&
                (modulePVE.rebornItemDic == null || !modulePVE.canRevive || modulePVE.rebornItemDic.Count <= 0))
                return false;

            PItem2 reviveItem = null;
            int reviveCoin = m_reviveCoinItem != null ? moduleEquip.GetPropCount(m_reviveCoinItem.itemTypeId) : 0;

            if (reviveCoin > 0) reviveItem = m_reviveCoinItem;
            if (reviveItem == null) reviveItem = modulePVE.rebornItemDic.Get(modulePVE.currentReviveTimes);

            if (reviveItem == null) return false;

            currentReviveCostCount = (int) reviveItem.num;
            SetPanelVisible(true);

            //替换公共图片
            var info = ConfigManager.Get<PropItemInfo>(reviveItem.itemTypeId);
            if (info.itemType == PropType.Currency)
                AtlasHelper.SetCurrencyIcon(m_costIcon, (CurrencySubType) info.subType);
            else AtlasHelper.SetIcons(m_costIcon, info.icon);

            Util.SetText(m_contentText, (int) TextForMatType.CombatUIText, 3, info.itemName);

            int hasReviveCostCount = 0;
            if (info.itemType == PropType.Currency)
                hasReviveCostCount = info.subType == (byte) CurrencySubType.Diamond
                    ? (int) modulePlayer.roleInfo.diamond
                    : (int) modulePlayer.roleInfo.coin;
            else if (info.itemType == PropType.Sundries && info.subType == (int) SundriesSubType.Revive)
                hasReviveCostCount = reviveCoin;
            else hasReviveCostCount = moduleEquip.GetPropCount(info.ID);

            m_restReviveText.text = string.Format(restReviveTip,
                (modulePVE.maxReviveTimes - modulePVE.currentReviveTimes));
            m_costCountText.text = string.Format(restCountTip, currentReviveCostCount, hasReviveCostCount);

            bool isReviveBtnEnable = hasReviveCostCount >= currentReviveCostCount && modulePVE.canRevive;
            m_reviveBtn.SetInteractable(isReviveBtnEnable);

            //组队模式不暂停AI
            if (!modulePVE.isTeamMode) moduleAI.SetAllAIPauseState(true);
            return true;
        }

        private void OnReviveClick()
        {
            onReviveClick?.Invoke();
            modulePVE.SendReborn();
        }

        private void OnCancelClick(GameObject sender)
        {
            SetPanelVisible(false);
            onCancelClick?.Invoke();
        }

        private void OnExitClick(GameObject sender)
        {
            SetPanelVisible(false);
            onExitClick?.Invoke();
        }
    }

    /// <summary>
    /// 迷宫陷阱道具提示
    /// </summary>
    public class LabyrinthTipPanel : CustomSecondPanel
    {
        private Text tipText;
        public double aliveTime { get; private set; }

        public LabyrinthTipPanel(Transform trans) : base(trans)
        {
        }

        public override void InitComponent()
        {
            base.InitComponent();
            tipText = transform.Find("text").GetComponent<Text>();
        }

        public void RefreshTip(PropItemInfo prop)
        {
            aliveTime = 5.0;
            tipText.text = string.Format(ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 21), prop.itemName);
            SetPanelVisible(true);
        }

        public void RefreshTip(ConfigText text, double duractionTime)
        {
            aliveTime = duractionTime;
            tipText.text = text[0];

            SetPanelVisible(true);
        }

        public void CheckCountDownEnd(double deltaTime)
        {
            aliveTime -= deltaTime;
            if (aliveTime <= 0) aliveTime = 0;
            if (aliveTime == 0) CloseTip();
        }

        private void CloseTip()
        {
            SetPanelVisible(false);
        }
    }

    /// <summary>
    /// 怪物提示
    /// </summary>
    public class MonsterTip : CustomSecondPanel
    {
        public MonsterTip(Transform trans, bool left) : base(trans)
        {
            m_left = left;
        }

        private bool m_left;
        private GameObject m_bossTip;
        private GameObject m_monTip;
        private GameObject m_npcObj;

        public override void InitComponent()
        {
            base.InitComponent();

            m_bossTip = transform.Find("bossarrow").gameObject;
            m_monTip = transform.Find("monsterarrow").gameObject;
            m_npcObj = transform.Find("npcarrow").gameObject;
        }

        public void SetMonsterTipVisible(EnumMonsterTipArrow tip = EnumMonsterTipArrow.None)
        {
            gameObject.SetActive(tip != EnumMonsterTipArrow.None);
            if (m_left) SetLeftMonsterTip(tip);
            else SetRightMonsterTip(tip);
        }

        private void SetLeftMonsterTip(EnumMonsterTipArrow tip)
        {
            m_bossTip.SetActive((tip & EnumMonsterTipArrow.BossLeft) > 0);
            m_monTip.SetActive((tip & EnumMonsterTipArrow.MonserLeft) > 0);
            m_npcObj.SetActive((tip & EnumMonsterTipArrow.NpcLeft) > 0);
        }

        private void SetRightMonsterTip(EnumMonsterTipArrow tip)
        {
            m_bossTip.SetActive((tip & EnumMonsterTipArrow.BossRight) > 0);
            m_monTip.SetActive((tip & EnumMonsterTipArrow.MonsterRight) > 0);
            m_npcObj.SetActive((tip & EnumMonsterTipArrow.NpcRight) > 0);
        }
    }

    #endregion

    #region 工会boss

    private Text m_unionBossTime; //工会boss倒计时
    private GameObject m_bossTimeObj;
    private int m_bossRemainTime;
    private float m_bossTime;
    public bool m_bossEnd = false;

    #endregion

    #region 助战

    private Image assistHpSlider;
    private Transform assistRoot;
    private Image assistIcon;
    private Transform assistNotice;
    private Creature assistMember;
    private Image assistIcon2;
    private Text assistName;
    private bool isAssistComeOn;

    #endregion

    #region pve field

    private GameObject m_pveEventObj, m_autoParent;
    private Text m_pveEventTimeText;
    private Button m_autoBattleOnBtn, m_autoBattleOffBtn;
    private EnumPVEAutoBattleState m_autoBtnState;
    private BossNotice m_bossNotice;
    private MonsterTip m_leftMonTip, m_rightMonTip;
    //切换场景的tip
    private GameObject m_transportTip, m_leftTransortTip, m_rightTransportTip;
    private int m_lastTransportDir = 0;
    private GameObject m_bossWhiteObj;
    private int m_bossDelayEvent;
    private Button m_reviveButton;

    #endregion

    private Creature m_hero1 = null, m_hero2 = null, m_player = null;
    private PlayerRevivePanel m_pvePlayerRevivePanel;
    private LabyrinthTipPanel m_labyrinthTipPanel;
    private Transform m_mainPanelNode;

    private int m_shotShowHide = 0;

    /// <summary>
    /// 放弃复活，直接结算
    /// </summary>
    private void OnGiveUpRevive()
    {
        modulePVE.SendPVEState(PVEOverState.GameOver, m_player);
        countDownPanel.enableUpdate = false;
    }

    /// <summary>
    /// 暂时不复活。如果可以观战进入观战。否则视为放弃复活，直接结算
    /// </summary>
    private void OnWaitRevive()
    {
        if (modulePVE.isStandalonePVE)
        {
            countDownPanel.enableUpdate = false;
            modulePVE.SendPVEState(PVEOverState.GameOver, m_player);
            return;
        }
        var levelBattle = (Level.current as Level_Battle);
        if (levelBattle == null) return;

        countDownPanel.DisableTimer();
        levelBattle.EnterState(BattleStates.Watch);
        countDownPanel.SetToggle(true);
    }

/// <summary>
    /// Called when window added to level
    /// </summary>
    protected override void OnOpen()
    {
        isFullScreen = false;

        countDownPanel = SubWindowBase.CreateSubWindow<PveCombat_CountDown>(this, GetComponent<Transform>("count_tip"));
        countDownPanel.onToggleValueChange.AddListener(isOn => m_pvePlayerRevivePanel.SetPanelVisible(!isOn));
        countDownPanel.onTimeEnd.AddListener(mode =>
        {
            if (mode == PveCombat_CountDown.Mode.ReviveCountDown)
                OnGiveUpRevive();
            else if (mode == PveCombat_CountDown.Mode.WatchCountDown)
                OnWaitRevive();

            if(m_pvePlayerRevivePanel.enable)
                m_pvePlayerRevivePanel.SetPanelVisible(false);
        });

        m_pvePlayerRevivePanel = new PlayerRevivePanel(transform.Find("revivepanel"));
        m_pvePlayerRevivePanel.onExitClick = OnGiveUpRevive;
        m_pvePlayerRevivePanel.onCancelClick = OnWaitRevive;
        m_pvePlayerRevivePanel.onReviveClick = () => { countDownPanel.enableUpdate = false; };
        m_pvePlayerRevivePanel.enable = false;

        m_labyrinthTipPanel = new LabyrinthTipPanel(transform.Find("trap_tip_panel"));
        m_labyrinthTipPanel.SetPanelVisible(false);

        m_bossNotice = new BossNotice(transform.Find("bossnotice"));
        m_bossNotice.SetPanelVisible(false);

        var isPvp = Level.current && Level.current.isPvP;

        m_hero1 = ObjectManager.FindObject<Creature>(c => c.teamIndex == 0 && isPvp || !isPvp && c.isPlayer);  // First created creature always use left avatar in pvp mode, but in pve mode left avatar is always player
        m_hero2 = ObjectManager.FindObject<Creature>(c => c != m_hero1 && !(c is PetCreature));

        //如果初始化选中了可破坏物,则重新选择
        if (Level.current && Level.current.isPvE && m_hero2 && m_hero2 is MonsterCreature && (m_hero2 as MonsterCreature).isDestructible)
        {
            m_hero2 = ObjectManager.FindObject<MonsterCreature>(c => !c.isDestructible && c.creatureCamp == CreatureCamp.MonsterCamp);
        }
        if (moduleMatch.isMatchRobot || moduleLabyrinth.lastSneakPlayer != null) m_hero2 = ObjectManager.FindObject<RobotCreature>(c => c.isRobot);
        if (m_hero1)
        {
            m_player = m_hero1.isPlayer ? m_hero1 : m_hero2;
        }

        m_mainPanelNode = transform.Find("main_panel");
        m_unionBossTime = m_mainPanelNode.GetComponent<Text>("Uniontime/number");
        m_bossTimeObj = m_mainPanelNode.GetComponent<RectTransform>("Uniontime").gameObject;
        m_bossTimeObj.SafeSetActive(false);

        m_pveEventObj = m_mainPanelNode.Find("eventtime").gameObject;
        m_pveEventTimeText = m_mainPanelNode.GetComponent<Text>("eventtime/number");
        m_pveEventObj.SafeSetActive(false);
        m_pveEventTimeText.text = string.Empty;

        m_autoParent = m_mainPanelNode.Find("auto_battle").gameObject;
        if (m_autoParent)
        {
            m_autoParent.SetActive(Level.current is Level_PVE);
            m_autoBattleOnBtn = m_autoParent.GetComponent<Button>("auto_battle_on");
            m_autoBattleOffBtn = m_autoParent.GetComponent<Button>("auto_battle_off");
        }
        InitAutoBattleBtn();

        Transform monsterTip = m_mainPanelNode.Find("position_node");
        if (monsterTip)
        {
            monsterTip.gameObject.SetActive(Level.current is Level_PVE);
            Transform t = monsterTip.Find("left");
            m_leftMonTip = new MonsterTip(t, true);
            t = monsterTip.Find("right");
            m_rightMonTip = new MonsterTip(t, false);
            m_leftMonTip.SetMonsterTipVisible(EnumMonsterTipArrow.None);
            m_rightMonTip.SetMonsterTipVisible(EnumMonsterTipArrow.None);
        }

        m_transportTip = m_mainPanelNode.Find("transport_node")?.gameObject;
        m_leftTransortTip = m_transportTip.transform.Find("left").gameObject;
        m_rightTransportTip = m_transportTip.transform.Find("right").gameObject;
        m_transportTip.SafeSetActive(false);

        m_bossWhiteObj = m_mainPanelNode.Find("shanping_eff")?.gameObject;

        if (Level.current.isPvE)
        {
            PropItemInfo trapInfo = modulePVE.GetTrapPropInfo();
            if (trapInfo != null)
            {
                m_labyrinthTipPanel.RefreshTip(trapInfo);
            }

            enableUpdate = true;
        }

        #region 助战

        assistHpSlider = GetComponent<Image>("assistpart/healthbar");
        assistRoot = GetComponent<Transform>("assistpart");
        assistIcon = GetComponent<Image>("assistpart/mask/assistavatar");
        assistIcon2 = GetComponent<Image>("assistnotice/bg/mask");
        assistNotice = GetComponent<Transform>("assistnotice");
        assistName = GetComponent<Text>("assistnotice/playername");

        assistRoot.SafeSetActive(false);
        isAssistComeOn = false;
        #endregion

        InitHero2();
        AddPVEListner();
        InitializePVEText();
    }

    private void InitializePVEText()
    {
        var t = ConfigManager.Get<ConfigText>((int)TextForMatType.CombatUIText);
        if (!t) return;

        Util.SetText(GetComponent<Text>("eventtime/text"), t[0]);
        Util.SetText(GetComponent<Text>("revivepanel/kuang/equipinfo"), t[1]);
        Util.SetText(GetComponent<Text>("revivepanel/kuang/english"), t[2]);
        Util.SetText(GetComponent<Text>("revivepanel/kuang/cost"), t[5]);
        Util.SetText(GetComponent<Text>("revivepanel/kuang/fangqi/Image"), t[7]);
        Util.SetText(GetComponent<Text>("revivepanel/kuang/fuhuo/Image"), t[1]);
        Util.SetText(GetComponent<Text>("Uniontime/text"), 242, 162);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        if (moduleUnion.m_isUnionBossTask)
        {
            m_bossEnd = true;
            m_bossTimeObj.gameObject.SetActive(true);
            SetBossTime();
        }

        modulePVEEvent.AddCondition(new SWindowCombatVisibleConditon());
    }

    protected override void OnClose()
    {
        base.OnClose();

        m_player = null;
        m_hero1 = null;
        m_hero1 = null;
        DelayEvents.Remove(m_bossDelayEvent);
        EventManager.RemoveEventListener(this);
    }

    protected override void OnReturn()
    {

    }

    public override void OnUpdate(int diff)
    {
        if (Level.current.isPvE)
        {
            SetUnionTime();
            // must before m_labyrinthTipPanel.CheckCountDownEnd
            UpdateShowMsg();

            var dt = diff * 0.001;
            if (m_labyrinthTipPanel.enable) m_labyrinthTipPanel.CheckCountDownEnd(dt);
        }
    }

    private void InitHero2()
    {
        if (!m_hero2) return;

        if(modulePVE.reopenPanelType == PVEReOpenPanel.Labyrinth) m_hero2.AddEventListener(CreatureEvents.DEAD_ANIMATION_END, OnCreatureRealDead);
    }

    private void AddPVEListner()
    {
        if (!Level.current.isPvE) return;

//        EventManager.AddEventListener(CreatureEvents.HEALTH_CHANGED, OnCreatureHealthChanged);
        if (m_player)
        {
            m_player.AddEventListener(CreatureEvents.ATTACKED, OnPvePlayerAttacted);
            m_player.AddEventListener(CreatureEvents.RAGE_CHANGED, OnPveRageChange);
            m_player.AddEventListener(CreatureEvents.HEALTH_CHANGED, OnPveHealthChange);
            m_player.AddEventListener(CreatureEvents.ENTER_STATE, OnPlayerEnterState);
        }

        EventManager.AddEventListener(CreatureEvents.DEAD_ANIMATION_END, OnCreatureRealDead);
        Level.current.AddEventListener(LevelEvents.BATTLE_ENTER_WATCH_STATE, OnBattleEnterWatchState);
        EventManager.AddEventListener(Events.CAMERA_SHOT_STATE, OnCameraShotState);
        EventManager.AddEventListener(Events.CAMERA_SHOT_UI_STATE, OnCameraShotUIState);
    }

    private void OnCameraShotState(Event_ e)
    {
        if (!Camera_Combat.enableShotSystem) return;

        if (m_shotShowHide == 0 && actived) return;
        m_shotShowHide = 0;

        var shotEnabled = (bool)e.param1;
        if (!shotEnabled) m_behaviour.Show(false);
    }

    private void OnCameraShotUIState(Event_ e)
    {
        if (!Camera_Combat.enableShotSystem) return;

        var hide = (bool)e.param1 | moduleBattle.inDialog | Level_PVE.PVEOver;
        var ns = m_shotShowHide;
        m_shotShowHide = hide ? 1 : 2;

        if (hide && (m_behaviour.hiding || ns == 1) || !hide && (m_behaviour.showing || ns == 2)) return;

        var alphaOne = Math.Abs(m_behaviour.canvasGroup.alpha - 1) < float.Epsilon;

        if (hide)
        {
            if (actived && Math.Abs(m_behaviour.canvasGroup.alpha) > float.Epsilon)
                m_behaviour.Hide(false);
        }
        else if (!actived || !alphaOne)
        {
            m_behaviour.Show(false);
        }
    }

    private void OnBattleEnterWatchState(Event_ e)
    {
        var isWatch = (bool)e.param1;

        m_autoParent.SafeSetActive(!isWatch);
        m_reviveButton.SafeSetActive(isWatch);
    }

    #region PVE行为回调

    //    private void OnCreatureHealthChanged(Event_ e)
    //    {
    //        var monster = e.sender as MonsterCreature;
    //        if (!monster) return;
    //        monster.RefreshMonsterHealthBar();
    //    }

    private void OnCreatureRealDead(Event_ e)
    {
        var creature = (Creature)e.sender;
        if (!Level.current.isPvE || Level.current is Level_Train)
            return;
            //不是迷宫的偷袭发送玩家死亡
        if (creature.isMonster || moduleLabyrinth.lastSneakPlayer != null)
            return;

        //迷宫不发送玩家死亡消息
        if (creature == m_player && modulePVE.reopenPanelType != PVEReOpenPanel.Labyrinth)
        {
            modulePVE.SendPVEState(PVEOverState.RoleDead);
        }

        var levelBattle = Level.current as Level_Battle;
        if (modulePVE.isTeamMode && levelBattle != null && levelBattle.IsPlayerAllDead())
        {
            if (m_pvePlayerRevivePanel.OpenRevivePanel())
                countDownPanel.Initialize(PveCombat_CountDown.Mode.ReviveCountDown);
            else
                OnGiveUpRevive();
        }
        else if (creature == m_player)
        {
            //如果不能复活了，直接发送gameover消息
            if (!m_pvePlayerRevivePanel.OpenRevivePanel()) modulePVE.SendPVEState(PVEOverState.GameOver, m_player);
            else
            {
                if(modulePVE.isStandalonePVE)
                    countDownPanel.Initialize(PveCombat_CountDown.Mode.ReviveCountDown);
                else
                    countDownPanel.Initialize(PveCombat_CountDown.Mode.WatchCountDown);
            }
        }
    }

    private void UpdateShowMsg()
    {
        if (actived && modulePVEEvent.validCheckShowMsgbehaviour)
        {
            SShowMessageBehaviour be = modulePVEEvent.GetShowMessageBehaviour();
            if (be != null)
            {
                if (m_labyrinthTipPanel.activeInHierarchy)
                {
                    var canvas = m_labyrinthTipPanel.transform.GetComponentDefault<CanvasGroup>();
                    DOTween.To(() => canvas.alpha, (a) => canvas.alpha = a, 0, 0.5f).OnComplete(() =>
                    {
                        OnRefreshTip(be);
                        DOTween.To(() => canvas.alpha, (a) => canvas.alpha = a, 1, 0.5f);
                    });
                }
                else
                    OnRefreshTip(be);
            }
        }
    }

    private void OnRefreshTip(SShowMessageBehaviour behaviour)
    {
        if (behaviour == null) return;

        ConfigText text = ConfigManager.Get<ConfigText>(behaviour.textId);
        if (text == null)
        {
            Logger.LogError("On ShowMessage Event,config Text ID = {0} connot be loaded,please check out!", behaviour.textId);
            return;
        }
        m_labyrinthTipPanel.RefreshTip(text, behaviour.duraction * 0.001);
    }

    private void OnStageCountDown(int sec)
    {
        if (m_pveEventTimeText) m_pveEventTimeText.text = sec.ToString();
        m_pveEventObj?.SetActive(sec > 0);
    }

    private void OnBossComing(SBossComingBehaviour b)
    {
        if (m_bossNotice != null) m_bossNotice.RefreshBossNotice(b.bossId1, b.bossId2);
    }

    private void OnTransportScene(STransportSceneBehaviour b)
    {
        //重置界面参数
        m_pveEventObj.SetActive(false);
    }

    private void OnBossCameraAnimWhiteFadeIn()
    {
        if (!m_bossWhiteObj) return;

        DelayEvents.Remove(m_bossDelayEvent);
        m_bossWhiteObj.SafeSetActive(true);
        (m_bossWhiteObj.transform as RectTransform).SetAsLastSibling();
        m_bossDelayEvent = DelayEvents.Add(() =>
        {
            m_bossWhiteObj.SafeSetActive(false);
        }, (float)(CombatConfig.spveBossAnimWhiteFadeIn + CombatConfig.spveBossAnimWhiteFadeOut));
    }

    private void OnShowBossReward(PItem2[] p)
    {
        ShowAsync<Window_RandomReward>(null, (w) =>
        {
            var rewardWindow = w as Window_RandomReward;
            rewardWindow.RefreshReward(p);
        });
    }

    private void OnBossRewardEnd()
    {
        modulePVEEvent.pauseEvent = false;
    }
    #endregion
    
    #region pve data collect

    private void OnPveHealthChange(Event_ e)
    {
        modulePVE.SetPveGameData(EnumPVEDataType.Health, m_player.healthRate);
    }

    private void OnPveRageChange(Event_ e)
    {
        modulePVE.SetPveGameData(EnumPVEDataType.Rage, m_player.rageRate);
    }

    private void OnPvePlayerAttacted(Event_ e)
    {
        modulePVE.AddPveGameData(EnumPVEDataType.BeHitedTimes, 1.0f);
    }

    private void OnPlayerEnterState(Event_ e)
    {
        var newState = e.param2 as StateMachineState;
        if (!newState) return;

        if (newState.isUltimate) modulePVE.AddPveGameData(EnumPVEDataType.UltimateTimes, 1);
        else if (newState.isExecution) modulePVE.AddPveGameData(EnumPVEDataType.ExecutionTimes, 1);
    }

    #endregion
    
    #region auto battle controller
    private void InitAutoBattleBtn()
    {
        m_autoParent.SetActive(false);
        m_autoBtnState = modulePVE.GetAutoBattleState();
        m_autoParent.SetActive(m_autoBtnState != EnumPVEAutoBattleState.Disable);

        m_autoBattleOnBtn.interactable = m_autoBtnState == EnumPVEAutoBattleState.Enable;
        EventTriggerListener.Get(m_autoBattleOnBtn.gameObject).onClick = OnAutoOnClick;
        EventTriggerListener.Get(m_autoBattleOffBtn.gameObject).onClick = OnAutoOffClick;

        ChangeAutoBattle(modulePVE.recordAutoBattle & m_autoBtnState == EnumPVEAutoBattleState.Enable);
    }

    private void OnAutoOnClick(GameObject sender)
    {
        switch (m_autoBtnState)
        {
            case EnumPVEAutoBattleState.Enable:
                ChangeAutoBattle(true);
                modulePVE.recordAutoBattle = true;
                break;
            case EnumPVEAutoBattleState.EnableNotStageClear: moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.CombatUIText, 10)); break;
            case EnumPVEAutoBattleState.EnableNotThreeStars: moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.CombatUIText, 11)); break;
        }
    }

    private void OnAutoOffClick(GameObject sender)
    {
        ChangeAutoBattle(false);
        modulePVE.recordAutoBattle = false;
    }

    private void ChangeAutoBattle(bool useAI)
    {
        if (m_autoBattleOffBtn) m_autoBattleOffBtn.gameObject.SetActive(useAI);
        if (m_autoBattleOnBtn) m_autoBattleOnBtn.gameObject.SetActive(!useAI);

        if (!m_player) return;

        m_player.useAI = useAI;
        ApplyAutoBattle();
    }

    private void ApplyAutoBattle()
    {
        if (modulePVE.isTeamMode) return;

        if (m_player.useAI)
        {
            moduleAI.AddPlayerAI(m_player);
            moduleAI.ChangeLockEnermy(m_player, true);
        }
        else moduleAI.RemovePlayerAI(m_player);
    }
    #endregion

    #region union boss time
    private void SetBossTime()
    {
        m_bossRemainTime = (int)(moduleUnion.BossInfo.opentime - moduleUnion.BossLossTime(moduleUnion.m_bossCloseTime));
        if (m_bossRemainTime > 0)
        {
            SetTime(m_bossRemainTime, m_unionBossTime);
        }
        else
        {
            m_bossTimeObj.gameObject.SetActive(false);
        }
    }

    private void SetUnionTime()
    {
        if (moduleUnion.m_isUnionBossTask && m_bossEnd)
        {
            m_bossTime += Time.unscaledDeltaTime;
            if (m_bossTime > 1)
            {
                m_bossTime = 0;
                m_bossRemainTime--;
                SetTime(m_bossRemainTime, m_unionBossTime);
                if (m_bossRemainTime < 60)
                {
                    //m_unionBossTime开始动画
                }
            }
        }
    }
    private void SetTime(int timeNow, Text thisText)
    {
        var strTime = Util.GetTimeMarkedFromSec(timeNow);
        Util.SetText(thisText, strTime);
    }
    private void _ME(ModuleEvent<Module_Union> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Union.EventUnionBossClose:
                m_bossTimeObj.gameObject.SetActive(false);
                m_bossEnd = false;
                break;
        }
    }
    #endregion
    
    #region 助战

    public override void OnEnable()
    {
        base.OnEnable();
        assistNotice?.SafeSetActive(false);
        EventManager.AddEventListener(LevelEvents.CREATE_ASSIST, OnAssistMemberCommonOn);
        var level = Level.current as Level_Battle;
        if (level != null && !isAssistComeOn)
        {
            AssistMemberCommonOn(level.assistCreature);
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        assistNotice?.SafeSetActive(false);
        moduleBattle.RemoveEventListener(LevelEvents.CREATE_ASSIST, OnAssistMemberCommonOn);
    }

    private void OnAssistMemberCommonOn(Event_ e)
    {
        AssistMemberCommonOn(e.param1 as Creature);
    }

    private void AssistMemberCommonOn(Creature rCreature)
    {
        assistMember = rCreature;
        if (assistMember == null)
            return;
        isAssistComeOn = true;
        Module_Avatar.SetClassAvatar(assistIcon.gameObject, assistMember);
        Module_Avatar.SetClassAvatar(assistIcon2.gameObject, assistMember);
        assistHpSlider.fillAmount = assistMember.healthRate;
        Util.SetText(assistName, Util.Format(ConfigText.GetDefalutString(198), GetAssistName()));

        assistRoot.SafeSetActive(true);
        assistIcon.saturation = 1;

        assistNotice.SafeSetActive(true);
        var tween = assistNotice?.GetComponent<TweenBase>();
        tween?.Play();

        assistMember.AddEventListener(CreatureEvents.HEALTH_CHANGED, OnAssistHealthChange);
    }

    private string GetAssistName()
    {
        if (modulePVE.assistMemberInfo == null)
            return string.Empty;

        if (modulePVE.assistMemberInfo.type == 1)
            return moduleNpc.GetTargetNpc((NpcTypeID)modulePVE.assistMemberInfo.npcId)?.name ?? string.Empty;

        return modulePVE.assistMemberInfo.memberInfo.roleName;
    }

    private void OnAssistHealthChange(Event_ e)
    {
        assistHpSlider.fillAmount = assistMember.healthRate;

        if (assistMember.isDead)
            assistIcon.saturation = 0;
    }

    #endregion

    #region module events

    private void _ME(ModuleEvent<Module_PVE> e)
    {
        switch (e.moduleEvent)
        {
            //复活
            case Module_PVE.EventRebornState:
                bool isSuccess = (bool)e.param1;
                if (isSuccess)
                    countDownPanel.UnInitialize();
                else
                {
                    var levelBattle = Level.current as Level_Battle;
                    countDownPanel.enableUpdate = levelBattle?.IsPlayerAllDead() ?? false;
                }
                //复活成功处理
                if (isSuccess && !modulePVE.isTeamMode)
                {
                    //复活次数+1
                    modulePVE.currentReviveTimes++;
                    m_pvePlayerRevivePanel.SetPanelVisible(false);
                    m_hero1.Revive();
                    modulePVEEvent.pauseEvent = false;
                }
                break;

            case Module_PVE.EventMonsterTip:
                EnumMonsterTipArrow tip = (EnumMonsterTipArrow)e.param1;
                if (m_leftMonTip != null) m_leftMonTip.SetMonsterTipVisible(tip);
                if (m_rightMonTip != null) m_rightMonTip.SetMonsterTipVisible(tip);
                break;

            case Module_PVE.EventTransportSceneToUI:
                bool begin = (bool)e.param1;
                if (begin)
                {
                    if (m_pvePlayerRevivePanel.enable) m_pvePlayerRevivePanel?.SetPanelVisible(false);
                    if (m_labyrinthTipPanel.enable) m_labyrinthTipPanel?.SetPanelVisible(false);
                }
                else if (!begin && m_player)
                {
                    if (m_player.health == 0)
                    {
                        var levelBattle = Level.current as Level_Battle;
                        if(!modulePVE.isTeamMode)
                            m_pvePlayerRevivePanel?.OpenRevivePanel();
                        else if(levelBattle != null)
                        {
                            if(levelBattle.IsPlayerAllDead())
                            {
                                m_pvePlayerRevivePanel?.OpenRevivePanel();
                                countDownPanel.Initialize(PveCombat_CountDown.Mode.ReviveCountDown);
                            }

                            if (levelBattle.IsState(BattleStates.Watch))
                            {
                                levelBattle.QuitState(BattleStates.Watch);
                                levelBattle.EnterState(BattleStates.Watch);
                            }
                        }
                    }
                    else if (m_player.health > 0) ApplyAutoBattle();
                }
                break;

            case Module_PVE.EventTriggerTransportTip:
                int dir = (int)e.param1;
                if (dir != m_lastTransportDir)
                {
                    m_lastTransportDir = dir;
                    m_transportTip.SetActive(dir != 0);
                    m_leftTransortTip.SetActive((dir & (int)EnumMonsterTipArrow.MonserLeft) > 0);
                    m_rightTransportTip.SetActive((dir & (int)EnumMonsterTipArrow.MonsterRight) > 0);
                }
                break;

            case Module_PVE.EventBossCameraAnimWhiteFadeIn: OnBossCameraAnimWhiteFadeIn(); break;
            case Module_PVE.EventDisplayBossReward: OnShowBossReward((PItem2[])e.param1); break;
        }
    }

    private void _ME(ModuleEvent<Module_PVEEvent> e)
    {
        switch (e.moduleEvent)
        {
            case Module_PVEEvent.EventBossComing: OnBossComing(e.param1 as SBossComingBehaviour); break;
            case Module_PVEEvent.EventCountDown: OnStageCountDown((int)e.param1); break;
            case Module_PVEEvent.EventTransportScene: OnTransportScene(e.param1 as STransportSceneBehaviour); break;
        }
    }

    void _ME(ModuleEvent<Module_Team> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Team.EventMemberReborn:
                var member = e.msg as PTeamMemberInfo;
                if (member != null)
                {
                    //self reborn
                    if (member.roleId == modulePlayer.id_)
                    {
                        //复活次数+1
                        modulePVE.currentReviveTimes++;
                        m_pvePlayerRevivePanel.SetPanelVisible(false);
                        countDownPanel.UnInitialize();
                    }
                    else
                    {
                        //队友复活停止倒计时
                        countDownPanel.enableUpdate = false;
                    }
                }
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Bordlands> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Bordlands.EventShowRewardOver: OnBossRewardEnd(); break;
        }
    }

    #endregion
}
