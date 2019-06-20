/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Combat window script.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-07
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class Window_Combat : Window
{
    private GameObject m_awakeRoot; //新手引导会强制显示引导解锁按钮。所以需要给按钮加一层父节点来控制显示/隐藏
    private Button m_btnShoot = null, m_btnBurst = null, m_btnAwake = null, m_btnPause = null;
    private Creature m_hero1 = null, m_hero2 = null, m_player = null;

    private Image m_awakeBar = null, m_state499Bar = null;
    private CanvasRenderer m_awakeCR = null, m_awakeBarCR = null;

    private Text m_battleTimer = null, m_info = null, m_state499Info = null, m_btnBurstInfo = null;
    private AvatarCombat m_avatarLeft = null, m_avatarRight = null;
    private TweenBase m_infoTween = null, m_state499Tween = null;
    private GameObject m_timeInfinityObj;
    
    private bool m_isPvP = false, m_isTrain = false, m_isPvE = false;
    private RuneBuff[] m_runeBuffs = new RuneBuff[2];

    private StateMachineState m_state499 = null, m_state10 = null;
    private int m_state499CD = 0, m_state10CD = 0;

    private bool showAwakeButton
    {
        get
        {
            return moduleGuide.awakeUnlocked && !m_isPvP &&
                   (m_isTrain || m_isPvE && modulePVE.reopenPanelType != PVEReOpenPanel.Labyrinth);
        }
    }

    #region Combo
    private GameObject m_comboObj;
    private ComboNumber m_comboNum;
    private int m_comboCount = 0;
    #endregion
    
    public class RuneBuff
    {
        private readonly Transform miniIconNode;
        private readonly Transform iconNode;
        private readonly int       listenerBuff;
        private readonly Creature  listener;

        public RuneBuff(Transform rRoot, Creature rListener, int rBuffId)
        {
            if (rRoot == null)
                return;
            rRoot.SafeSetActive(true);
            listener = rListener;
            listenerBuff = rBuffId;
            var info = ConfigManager.Get<BuffInfo>(rBuffId);
            Util.SetText(rRoot.GetComponent<Text>("rune_buff_03/rune_txt"), info?.name ?? string.Empty);
            miniIconNode = rRoot.GetComponent<Transform>("rune_buff_01");
            iconNode = rRoot.GetComponent<Transform>("rune_buff_03");

            miniIconNode.SafeSetActive(info != null);
            iconNode    .SafeSetActive(false);

            if (info?.runeIcons != null)
            {
                if(info.runeIcons.Length > 0 && !string.IsNullOrEmpty(info.runeIcons[0]))
                    AtlasHelper.SetRune(rRoot.GetComponent<Image>("rune_buff_01"), info.runeIcons[0]);
                if(info.runeIcons.Length > 1 && !string.IsNullOrEmpty(info.runeIcons[1]))
                    AtlasHelper.SetRune(rRoot.GetComponent<Image>("rune_buff_03"), info.runeIcons[1]);
            }
            listener?.AddEventListener(CreatureEvents.BUFF_TRIGGER, OnBuffTrigger);
            listener?.AddEventListener(CreatureEvents.BUFF_REMOVE , OnBuffRemove);
        }
        public void Destory()
        {
            listener?.RemoveEventListener(CreatureEvents.BUFF_TRIGGER, OnBuffTrigger);
            listener?.RemoveEventListener(CreatureEvents.BUFF_REMOVE , OnBuffRemove);
            miniIconNode.SafeSetActive(false);
            iconNode.SafeSetActive(false);
        }
         
        private void OnBuffRemove(Event_ e)
        {
            var buff = e.param1 as Buff;
            if (buff?.ID != listenerBuff)
                return;
            miniIconNode?.SetGray(true);
        }


        private void OnBuffTrigger(Event_ e)
        {
            var buff = e.param1 as Buff;
            if (buff?.ID != listenerBuff)
                return;
            iconNode?.SafeSetActive(true);
        }
    }

    protected override void OnOpen()
    {
        #region Initialize

        m_behaviour.globalLock = false;

        m_isPvP   = Level.current && Level.current.isPvP;
        m_isTrain = !m_isPvP && Level.currentLevel == GeneralConfigInfo.sTrainLevel;
        m_isPvE   = !m_isPvP && !m_isTrain && Level.current && Level.current.isPvE;

        moduleGlobal.ShowGlobalLayer(false);

        Level.current.AddEventListener(LevelEvents.BATTLE_TIMER, OnBattleTimer);
        Level.current.AddEventListener(LevelEvents.PREPARE,      OnBattlePrepare);
        Level.current.AddEventListener(LevelEvents.BATTLE_ENTER_WATCH_STATE,      OnBattleEnterWatchState);

        EventManager.AddEventListener(CreatureEvents.PLAYER_ADD_TO_SCENE, OnPlayerAddToScene);

        if (m_isPvE) EventManager.AddEventListener(Level_PVE.EventInitPVERightAvatar, InitPVERightAvatar);

        #endregion

        #region Avatar

        m_avatarLeft = GetComponent<AvatarCombat>("left");
        m_avatarRight = GetComponent<AvatarCombat>("right");

        #endregion

        #region Buttons & Infos

        m_btnShoot     = GetComponent<Button>("qiangji");
        m_btnBurst     = GetComponent<Button>("baoqi");
        m_btnBurstInfo = GetComponent<Text>("baoqi/Text");
        m_awakeRoot    = GetComponent<Transform>("awakeRoot")?.gameObject;
        m_btnAwake     = GetComponent<Button>("awakeRoot/awake");
        m_awakeBar     = GetComponent<Image>("awakeRoot/awake/progressbar");
        m_awakeCR      = GetComponent<CanvasRenderer>("awakeRoot/awake");
        m_awakeBarCR   = GetComponent<CanvasRenderer>("awakeRoot/awake/progressbar");

        m_awakeRoot?.SetActive(showAwakeButton);

        moduleBattle.SetKeyState("E", m_btnAwake.gameObject.activeSelf);

        m_effectNode = GetComponent<RectTransform>("effectNode");

        m_battleTimer = GetComponent<Text>("jishi_pai/number");
        m_btnPause = GetComponent<Button>("jishi_pai/pause");
        m_timeInfinityObj = transform.Find("jishi_pai/endless").gameObject;

        m_btnShoot.onClick.AddListener(OnBtnShoot);
        m_btnBurst.onClick.AddListener(OnBtnBurst);
        m_btnAwake.onClick.AddListener(OnBtnAwake);

        m_info = GetComponent<Text>("info");
        m_infoTween = GetComponent<TweenColor>("info");
        m_state499Info = GetComponent<Text>("state499/Text");
        m_state499Bar = GetComponent<Image>("state499/progressbar");
        m_state499Tween = GetComponent<TweenAlpha>("state499");

        m_state499CD = 0;
        m_state10CD = 0;

        m_btnBurstInfo.enabled = false;

        m_state499Tween.gameObject.SetActive(false);

        m_btnPause.interactable = true;
        m_btnPause.onClick.AddListener(OnBtnPause);
        
        var now = Level.current is Level_Battle ? (Level.current as Level_Battle).battleTimeSec : CombatConfig.sdefaultTimeLimit / 1000;
        OnBattleTimer(Event_.Pop(now));

        #endregion

        #region Movement & Touch

        m_movementPad = GetComponent<UIMovementPad>("movementPad");
        m_touchGuard = GetComponent<RectTransform>("touchGuard");
        m_moveCR = m_movementPad.GetComponent<CanvasRenderer>();
        m_touchPad = GetComponent<UITouchPad>("touchPad");
        m_btnLeft = GetComponent<CanvasRenderer>("movementPad/left");
        m_btnRight = GetComponent<CanvasRenderer>("movementPad/right");

        m_movementPad.onTouchBegin.AddListener(OnMovementPadBegin);
        m_movementPad.onTouchMove.AddListener(OnMovementPadMove);
        m_movementPad.onTouchEnd.AddListener(OnMovementPadEnd);
        m_touchPad.onTouchBegin.AddListener(OnTouchPadBegin);
        m_touchPad.onTouchEnd.AddListener(OnTouchPadEnd);
        m_touchPad.onTouchMove.AddListener(OnTouchPadMove);
        m_touchPad.onTouchStay.AddListener(OnTouchPadStay);

        m_movementPos = m_movementPad.rectTransform().anchoredPosition;
        m_guardSize = m_touchGuard.sizeDelta;

        InitTouchAndMoveAlpha();
        UpdateInputSettings();

        EventManager.AddEventListener(Events.INPUT_SETTINGS_CHANGED, UpdateInputSettings);

        #endregion

        #region Combo

        m_comboObj = transform.Find("lianji").gameObject;
        m_comboNum = GetComponent<ComboNumber>("lianji/number");
        m_comboObj.SetActive(false);

        #endregion

        #region Camera Shot

        m_shotShowHide = 0;
        EventManager.AddEventListener(Events.CAMERA_SHOT_STATE,    OnCameraShotState);
        EventManager.AddEventListener(Events.CAMERA_SHOT_UI_STATE, OnCameraShotUIState);

        #endregion

        #region Creatures

        ObjectManager.Foreach<Creature>(c =>
        {
            if (m_isPvP && c.teamIndex == 0 || !m_isPvP && c.isPlayer)
            {
                m_hero1 = c;
                return !m_hero2;
            }

            if (!c.visible) return true;

            if ((moduleMatch.isMatchRobot || moduleLabyrinth.lastSneakPlayer != null) && c.isRobot ||
                m_isPvP && c.teamIndex > 0 || !m_isPvP && c.creatureCamp == CreatureCamp.MonsterCamp && (!(c is MonsterCreature) || !(c as MonsterCreature).isDestructible))
            {
                m_hero2 = c;
                return !m_hero1;
            }

            return true;
        });

        InitCreature(0, m_hero1);
        InitCreature(1, m_hero2);

        UpdateBurstButtonState();
        UpdateShootButtonState();
        UpdateAwakeButtonState();

        #endregion

        #region Debug

        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        var btnKillMe = Object.Instantiate(Resources.Load<GameObject>("ui_button")).rectTransform();
        btnKillMe.name = "btnKillMe";
        btnKillMe.GetComponent<Text>("label").text = "Kill Self";
        Util.AddChild(transform, btnKillMe.transform);

        btnKillMe.pivot = btnKillMe.anchorMax = btnKillMe.anchorMin = new Vector2(1, 0.5f);
        btnKillMe.sizeDelta = new Vector2(90, 50);
        btnKillMe.anchoredPosition = new Vector2(0, 55);

        if (Level.current is Level_Test || Level.current is Level_PVP) btnKillMe.GetComponent<Button>().onClick.AddListener(() => { ObjectManager.enableUpdate = true; Game.GoHome(); });
        else btnKillMe.GetComponent<Button>().onClick.AddListener(() => { if (m_player) m_player.Kill(); });

        RectTransform btnVictory = null, randomInput = null;

        if (!(Level.current is Level_Test || Level.current is Level_PVP))
        {
            btnVictory = Object.Instantiate(Resources.Load<GameObject>("ui_button")).rectTransform();
            btnVictory.name = "btnVictory";
            btnVictory.GetComponent<Text>("label").text = "Victory!";
            Util.AddChild(transform, btnVictory.transform);

            btnVictory.pivot = btnVictory.anchorMax = btnVictory.anchorMin = new Vector2(1, 0.5f);
            btnVictory.sizeDelta = new Vector2(90, 50);
            btnVictory.anchoredPosition = new Vector2(0, 0);

            btnVictory.GetComponent<Button>().onClick.AddListener(() =>
            {
                modulePVE.DebugPveGameData();
                modulePVE.SendPVEState(PVEOverState.Success, m_player);
                modulePVEEvent.ClearAll();
            });

            randomInput = Object.Instantiate(Resources.Load<GameObject>("ui_button")).rectTransform();
            randomInput.name = "randomInput";
            randomInput.GetComponent<Text>("label").text = InputManager.instance.randomKey ? "cancel random" : "random Input";
            Util.AddChild(transform, randomInput.transform);

            randomInput.pivot = randomInput.anchorMax = randomInput.anchorMin = new Vector2(1, 0.5f);
            randomInput.sizeDelta = new Vector2(90, 50);
            randomInput.anchoredPosition = new Vector2(0, 165);

            randomInput.GetComponent<Button>().onClick.AddListener(() =>
            {
                InputManager.instance.randomKey = !InputManager.instance.randomKey;
                randomInput.GetComponent<Text>("label").text = InputManager.instance.randomKey ? "cancel random" : "random Input";
            });
        }

        btnKillMe.SafeSetActive(!Root.simulateReleaseMode);
        randomInput.SafeSetActive(!Root.simulateReleaseMode);
        btnVictory.SafeSetActive(!Root.simulateReleaseMode);

        EventManager.AddEventListener("EditorSimulateReleaseMode", () =>
        {
            btnKillMe.SafeSetActive(!Root.simulateReleaseMode);
            randomInput.SafeSetActive(!Root.simulateReleaseMode);
            btnVictory.SafeSetActive(!Root.simulateReleaseMode);
        });

        #if UNITY_EDITOR
        EventManager.AddEventListener("EditorReloadConfig", OnEditorReloadConfig);
        #endif
        #endif

        #endregion
    }

    /// <summary>
    /// Init creature info
    /// </summary>
    /// <param name="index">0 = left  1 = right</param>
    private void InitCreature(int index, Creature c)
    {
        var old = index == 0 ? m_hero1 : m_hero2;
        if (old) old.RemoveEventListener(this);

        var isPlayer = old == m_player && old || c && c.isPlayer;
        
        if (index == 0) m_hero1 = c;
        else m_hero2 = c;

        var a = index == 0 ? m_avatarLeft : m_avatarRight;
        if (a) a.creature = c;

        if (c)
        {
            c.AddEventListener(CreatureEvents.DEAD_ANIMATION_END, OnCreatureRealDead);

            if (!c.isPlayer)
            {
                if (m_isPvP || m_isTrain)
                {
                    c.AddEventListener(CreatureEvents.ENTER_STATE,      OnEnermyEnterState);
                    c.AddEventListener(CreatureEvents.HIT_INFO_CLEARED, OnClearComboState);
                }
            }
            else m_player = c;
        }

        if (isPlayer)
        {
            if (m_player)
            {
                m_btnShoot.ignoreInteractable = true;
                m_btnAwake.ignoreInteractable = true;

                m_player.AddEventListener(CreatureEvents.ATTACK,               OnCreatureAttack);
                m_player.AddEventListener(CreatureEvents.COMBO_BREAK,          OnClearComboState);
                m_player.AddEventListener(CreatureEvents.DEAL_DAMAGE,          OnPlayerDealDamage);
                m_player.AddEventListener(CreatureEvents.DEAL_UI_DAMAGE,       OnPlayerDealUIDamage);
                m_player.AddEventListener(CreatureEvents.BATTLE_UI_INFO,       OnBattleUIInfo);
                m_player.AddEventListener(CreatureEvents.RAGE_CHANGED,         UpdateBurstButtonState);
                m_player.AddEventListener(CreatureEvents.BULLET_COUNT_CHANGED, UpdateShootButtonState);
                m_player.AddEventListener(CreatureEvents.ATTACK_HIT_FAILED,    OnAttackHitFailed);
                m_player.AddEventListener(CreatureEvents.QUIT_STATE,           OnPlayerQuitState);

                if (m_awakeRoot.activeSelf)
                {
                    m_player.AddEventListener(CreatureEvents.ENERGY_CHANGED, UpdateAwakeButtonState);
                    m_player.AddEventListener(CreatureEvents.MORPH_PROGRESS, UpdateAwakeProgress);
                }

                if (m_player.pet)
                    m_player.pet.AddEventListener(CreatureEvents.ENTER_STATE, OnPetEnterState);
            }
            else
            {
                m_btnShoot.interactable = false;
                m_btnBurst.interactable = false;
                m_btnAwake.interactable = false;
            }
        }

        var bn = GetComponent<Transform>(index == 0 ? "rune_buff/rune_buff_left" : "rune_buff/rune_buff_right");
        var bb = index == 0 ? m_runeBuffs[0] : m_runeBuffs[1];

        InitRuneBuff(c, bn, ref bb);
    }

    private void InitRuneBuff(Creature rCreature, Transform rNode, ref RuneBuff rRuneBuff)
    {
        var creature = rCreature as MonsterCreature;
        if (creature != null)
        {
            if (!creature.isBoss)
                return;
        }

        if (rRuneBuff != null)
        {
            rRuneBuff.Destory();
            rRuneBuff = null;
        }
        var buffs = rCreature?.GetBuffList();
        if (buffs != null && buffs.Count > 0)
        {
            foreach (var b in buffs)
            {
                if (b.info.runeIcons != null && b.info.runeIcons.Length > 0)
                {
                    rRuneBuff = new RuneBuff(rNode, rCreature, b.ID);
                    break;
                }
            }
        }
    }

    private void OnPlayerAddToScene(Event_ e)
    {
        var c = e.sender as Creature;
        if (!c) return;

        var index = !m_isPvP || c.teamIndex == 0 ? 0 : 1;
        InitCreature(index, c);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        UpdateAwakeButtonState();
    }

    public override void OnRenderUpdate()
    {
        UpdateStateProgress();
    }

    private void InitTouchAndMoveAlpha()
    {
        m_btnLeft.SetAlpha(0.6f);
        m_btnRight.SetAlpha(0.6f);
        m_moveCR.SetAlpha(0.5f);
    }

    private void OnPetEnterState(Event_ e)
    {
        var pet = e.sender as PetCreature;
        var state = e.param2 as StateMachineState;
        if (state != null && state.IsSkill)
        {
            var show = CombatConfig.sshowText;
            if (show.showSkillName)
            {
                var skill = pet?.GetSkillByState(state.name);
                show.showText = skill ? Util.Format(ConfigText.GetDefalutString(skill.SkillName), pet.petInfo.AdditiveLevel) : string.Empty;
            }
            if(!string.IsNullOrWhiteSpace(show.showText))
                ControllerEffect_ShowText.Create(show, pet?.transform);
        }
    }

    protected override void OnClose()
    {
        m_player = null;
        m_hero1 = null;
        m_hero1 = null;

        m_runeBuffs[0]?.Destory();
        m_runeBuffs[1]?.Destory();

        EventManager.RemoveEventListener(this);
    }

    private void UpdateInputSettings()
    {
        m_touchPad.moveFix = SettingsManager.touchSensitivity;

        if (m_touchPad.moveFix < 0) m_touchPad.moveFix = 0;

        var moff = CombatConfig.smovementTypeOffset[SettingsManager.movementType];
        m_movementPad.rectTransform().anchoredPosition = new Vector2(m_movementPos.x + moff, m_movementPos.y);
        m_touchGuard.sizeDelta = new Vector2(m_guardSize.x + moff, m_guardSize.y);

    }

    private void OnBattleTimer(Event_ e)
    {
        var sec = (int)e.param1;
        m_timeInfinityObj.SetActive(sec < 0);
        m_battleTimer.gameObject.SetActive(sec >= 0);
        m_battleTimer.text = sec.ToString();
    }

    private void OnBattleUIInfo(Event_ e)
    {
        var s = Util.GetString(9200, (int)e.param1);
        if (string.IsNullOrEmpty(s)) return;

        m_info.text = s;
        m_infoTween.PlayForward();
    }

    private void OnEnermyEnterState(Event_ e)
    {
        var state = e.param2 as StateMachineState;
        if (!state || !state.isProtect) return;

        var s = Util.GetString(9211);
        if (string.IsNullOrEmpty(s)) return;

        m_info.text = s;
        m_infoTween.PlayForward();
    }

    private void OnAttackHitFailed(Event_ e)
    {
        var target = e.param1 as Creature;
        var s = Util.GetString(9211, 1, target ? target.uiName : string.Empty);
        if (string.IsNullOrEmpty(s)) return;

        m_info.text = s;
        m_infoTween.PlayForward();
    }

    private void OnClearComboState()
    {
        m_comboCount = 0;
        m_comboObj.SafeSetActive(false);

        if (m_comboNum) m_comboNum.ResetAllParameters();
    }

    #region Touch input

    private CanvasRenderer m_btnLeft = null, m_btnRight = null, m_moveCR = null;
    private UIMovementPad m_movementPad = null;
    private UITouchPad m_touchPad = null;
    private RectTransform m_touchGuard = null;
    private Vector2 m_movementPos = Vector2.zero, m_guardSize = Vector2.zero;

    //private int[] m_touchKeys = new int[] { 2, 3, 4, 5 };

    private int m_lastMovement = 0;
    private bool m_touchStarted = false, m_touchParsed = false, m_touchMove = false;

    private const float sqrt2 = 0.7071f;

    private void OnMovementPadBegin(Vector2 dir)
    {
        if (!m_player) return;

        var s = 0;
        s = dir.x < 0 ? 1 : dir.x > 0 ? 2 : 0;

        //Logger.LogWarning(dir.ToXml());

        InputManager.SetTouchState(TouchID.Movement, 0, s == 1 ? 1 << 1 : 1, false);
        InputManager.SetTouchState(TouchID.Movement, 1, s == 1 ? 1 : 1 << 1, false);

        if (m_lastMovement != s)
        {
            m_btnLeft.SetAlpha(s == 1 ? 1 : 0.6f);
            m_btnRight.SetAlpha(s == 2 ? 1 : 0.6f);

            m_lastMovement = s;
        }
        //Logger.LogError("i == {0}", iiii);
    }

    private void OnMovementPadMove(Vector2 dir, Vector2 delta)
    {
        if (!m_player) return;

        var s = 0;
        s = dir.x < 0 ? 1 : dir.x > 0 ? 2 : 0;

        //Logger.LogDetail(dir.ToXml());

        if (m_lastMovement != s)
        {
            InputManager.SetTouchState(TouchID.Movement, 0, s == 1 ? 1 << 1 : 1, false);
            InputManager.SetTouchState(TouchID.Movement, 1, s == 1 ? 1 : 1 << 1, false);
            InputManager.SetTouchState(TouchID.Movement, 3, s == 1 ? 1 : 1 << 1, false);

            m_btnLeft.SetAlpha(s == 1 ? 1 : 0.6f);
            m_btnRight.SetAlpha(s == 2 ? 1 : 0.6f);

            m_lastMovement = s;
        }
    }

    private void OnMovementPadEnd(Vector2 dir)
    {
        m_lastMovement = 0;

        m_btnLeft.SetAlpha(0.6f);
        m_btnRight.SetAlpha(0.6f);

        InputManager.SetTouchState(TouchID.Movement, 0, -1);
    }
    //int iiii = 0;
    //public override void OnPostRenderUpdate()
    //{
    //    ++iiii;
    //}

    private void OnTouchPadBegin(Vector2 p)
    {
        m_touchStarted = true;
        m_touchParsed = false;
        m_touchMove = false;

        InputManager.SetTouchState(TouchID.Touch, 1, 1 | 1 << 5, false);

        //Logger.LogError("i == {0}", iiii);
    }

    private void OnTouchPadEnd(Vector2 p)
    {
        if (!m_touchStarted) return;
        m_touchStarted = false;
        m_touchMove = false;

        if (!m_touchParsed) InputManager.SetTouchState(TouchID.Touch, 2);
        InputManager.SetTouchState(TouchID.Touch, 2, 1 << 5, false);

        //Logger.LogError("i == {0}", iiii);
    }

    private void OnTouchPadStay(float delta)
    {
        if (!m_touchStarted || delta < 0.001f) return;

        InputManager.SetTouchState(TouchID.Touch, 3, 1 | 1 << 5, false);
    }

    private void OnTouchPadMove(Vector2 dir, Vector2 delta)
    {
        if (!m_touchStarted) return;

        if (m_touchParsed)
        {
            if (m_touchMove) OnTouchPadStay(0.1f); // Simulate touch stay event
            return;
        }

        m_touchParsed = true;

        var split = sqrt2 * CombatConfig.stouchSplitRatio;

        var d = dir.normalized;
        var x = d.x;
        var y = d.y;
        var k = x > 0 ? x >= split ? 1 : y > 0 ? 2 : 3 : x <= -split ? 4 : y > 0 ? 2 : 3;

        InputManager.SetTouchState(TouchID.Touch, 1, 1 << k);

        m_touchMove = true;
    }

    #endregion

    #region Effects

    private RectTransform m_effectNode;

    private void OnBattlePrepare(Event_ e)
    {
        var effect = (string)e.param1;
        if (string.IsNullOrEmpty(effect)) return;

        UIEffectManager.PlayEffectAsync(m_effectNode, effect, null, 0.5f);
    }

    #endregion

    #region Buttons

    private void UpdateBurstButtonState()
    {
        if (!m_player)
        {
            m_btnBurst.interactable = false;
            m_btnBurst.gameObject.SetActive(false);
        }
        else
        {
            m_btnBurst.gameObject.SetActive(true);
            m_btnBurst.interactable = !m_player.isDead && !m_btnBurstInfo.enabled && m_player.rage >= CombatConfig.sdefaultBurstButtonRage;
        }
    }

    private void UpdateShootButtonState()
    {
        m_btnShoot.interactable = Level.currentLevel == GeneralConfigInfo.sTrainLevel || m_player && m_player.bulletCount > 0;
    }

    private void UpdateAwakeButtonState()
    {
        m_awakeRoot.SetActive(showAwakeButton);

        if (!m_awakeRoot.activeSelf || !m_player) return;

        m_btnAwake.enabled = true;
        m_awakeBar.fillAmount = m_player.energyRate;
        m_btnAwake.interactable = m_awakeBar.fillAmount >= 1.0f;

        var alpha = m_btnAwake.interactable ? CombatConfig.sawakeButtonState.x : CombatConfig.sawakeButtonState.y;
        m_awakeCR.SetAlpha(alpha);
        m_awakeBarCR.SetAlpha(alpha);
    }

    private void UpdateAwakeProgress(Event_ e)
    {
        if (!m_awakeRoot.activeSelf) return;

        var buff = e.param2 as Buff;
        if (buff == null) return;

        m_awakeBar.fillAmount = buff.infinity ? 1.0f : (float)buff.duration / buff.length;
        m_btnAwake.enabled = buff.destroyed;

        if (m_btnAwake.enabled) UpdateAwakeButtonState();
        else
        {
            m_awakeCR.SetAlpha(1.0f);
            m_awakeBarCR.SetAlpha(1.0f);
        }
    }

    private void UpdateStateProgress()
    {
        if (m_state10)    //  Cool group 1
        {
            var cd = m_state10.coolingTime;
            var t = Mathf.CeilToInt(cd * 0.001f);
            if (m_state10CD != t)
            {
                m_state10CD = t;
                Util.SetText(m_btnBurstInfo, 9210, 2, t);
            }

            if (t <= 0)
            {
                m_btnBurstInfo.enabled = false;
                m_state10 = null;
            }

            m_btnBurst.interactable = !m_player.isDead && !m_btnBurstInfo.enabled && m_player.rage >= CombatConfig.sdefaultBurstButtonRage;
        }

        if (m_state499)   // StateSidestep
        {
            var cd = m_state499.coolingTime;
            var t = Mathf.CeilToInt(cd * 0.001f);
            if (m_state499CD != t)
            {
                m_state499CD = t;
                Util.SetText(m_state499Info, 9210, 1, t);
            }

            var p = m_state499.info.coolDown > 0 ? (float)cd / m_state499.info.coolDown : 0;
            m_state499Bar.fillAmount = p;

            if (t <= 0)
            {
                m_state499Tween.PlayReverse();
                m_state499 = null;
            }
        }
    }

    private void OnBtnShoot()
    {
        if (!m_player || m_player.currentState.passive || m_player.stateMachine.freezing || !moduleGuide.afterPause) return;

        if (Level.currentLevel != GeneralConfigInfo.sTrainLevel && m_player.bulletCount < 1) moduleGlobal.ShowMessage(9201);
        else
            InputManager.SetCustomButtonState("O", true);
    }

    private void OnBtnBurst()
    {
        InputManager.SetCustomButtonState("Q", true);
    }

    private void OnBtnAwake()
    {
        if (m_player.maxEnergy < 1 || m_player.energy < m_player.maxEnergy) moduleGlobal.ShowMessage(9210);
        else InputManager.SetCustomButtonState("E", true);
    }

    private void OnBtnPause()
    {
        if (m_isPvP) moduleGlobal.ShowMessage(9209, 0);
        else if (Level.current && Level.current is Level_Sneak) moduleGlobal.ShowMessage(9209, 2);
        else if (m_isPvE && !(Level.current as Level_PVE).canPause) moduleGlobal.ShowMessage(9209, 1);
        else
        {
            SetWindowParam<Window_Settings>(1);
            ShowAsync<Window_Settings>();
        }
    }

    #endregion

    #region 连击数字

    private void OnCreatureAttack(Event_ e)
    {
        m_comboCount = m_player.comboCount;

        if (m_comboCount > moduleBattle.maxCombo)
        {
            moduleBattle.maxCombo = m_comboCount;
            if (Level.current.isPvE) modulePVE.SetPveGameData(EnumPVEDataType.Combo, m_comboCount);
        }
        m_comboObj.SetActive(true);
        m_comboNum.PlayComboTween(m_comboCount);

        if (Level.current.isPvE)
        {
            var victim = e.param1 as Creature;
            if (m_avatarRight.creature != victim)
            {
                var m = m_avatarRight.creature as MonsterCreature;
                if (!m || m.isDead || !m.isBoss || !m.visible)
                {
                    m_avatarRight.creature = victim;

                    if (victim is MonsterCreature && (victim as MonsterCreature).isBoss)
                    {
                        InitRuneBuff(victim, GetComponent<Transform>("rune_buff/rune_buff_right"), ref m_runeBuffs[1]);
                    }
                }
            }
        }
    }

    private void OnPlayerDealDamage(Event_ e)
    {
        var damage = e.param2 as DamageInfo;
        modulePVE.AddPveGameData(EnumPVEDataType.Attack, damage.finalDamage);
    }

    private void OnPlayerDealUIDamage(Event_ e)
    {
        var damage = e.param2 as DamageInfo;
        if (damage?.buffEffectFlag != BuffInfo.EffectFlags.Heal)
            modulePVE.AddPveGameData(EnumPVEDataType.Attack, damage?.finalDamage ?? 0);
    }

    private void OnPlayerQuitState(Event_ e)
    {
        var state = e.param1 as StateMachineState;
        if (!state) return;

        if (state.info.coolGroup == 1)    //  CoolGroup 1
        {
            m_state10 = state;
            m_btnBurstInfo.enabled = true;
        }

        if (state.ID == 499)   // StateSidestep
        {
            m_state499 = state;
            m_state499Tween.PlayForward();
        }

        UpdateStateProgress();
    }

    #endregion

    #region 结算处理

    private void DefaultEndState()
    {
        m_touchPad.enabled = false;
        m_movementPad.enabled = false;

        m_hero1.moveState = 0;
        m_hero2.moveState = 0;

        enableUpdate = false;
    }

    #endregion
    
    #region Creature events

    /// <summary>
    /// 初始化PVE右侧的显示（第一只怪或者BOSS就刷新）
    /// </summary>
    /// <param name="monster"></param>
    private void InitPVERightAvatar(Event_ e)
    {
        var c = e.param1 as Creature;
        if (!Level.current.isPvE || !c) return;

        //缓存该角色，如果该角色是BOSS则不会再刷新小怪血量
        m_hero2 = c;
        var m = m_hero2 as MonsterCreature;
        if (!m || m.isDestructible) return;

        if (!m_avatarRight.creature || m && m.isBoss)
        {
            m_avatarRight.creature = m_hero2;
            InitRuneBuff(m_hero2, GetComponent<Transform>("rune_buff/rune_buff_right"), ref m_runeBuffs[1]);
        }
    }

    private void OnCreatureRealDead(Event_ e)
    {
        if (m_isPvE) return;
        DefaultEndState();
    }

    private void OnTransportScene(STransportSceneBehaviour b)
    {
        InitTouchAndMoveAlpha();
    }
    
    #endregion

    #region 消息回调

    private void _ME(ModuleEvent<Module_PVE> e)
    {
        switch (e.moduleEvent)
        {
            case Module_PVE.EventTransportSceneToUI:
                bool begin = (bool)e.param1;
                if(m_btnPause) m_btnPause.interactable = !begin;
                m_touchPad.enabled = !begin;
                m_movementPad.enabled = !begin;
                break;
        }
    }

    private void _ME(ModuleEvent<Module_PVP> e)
    {
        switch (e.moduleEvent)
        {
            case Module_PVP.EventRoleOffLine:
                DefaultEndState();
                break;
            case Module_PVP.EventLoadingComplete:
                enableUpdate = true;
                break;
            default:
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Guide> e)
    {
        if (e.moduleEvent == Module_Guide.EventGuidePositionSuccess) OnMovementPadEnd(Vector2.zero);
    }

    #endregion

    #region Camera shot events

    private int m_shotShowHide = 0;

    protected override void _Show(bool forward, bool immediately)
    {
        m_shotShowHide = 0;
        base._Show(forward, immediately);
    }

    protected override void _Hide(bool forward, bool immediately, bool destroy)
    {
        m_shotShowHide = 0;
        base._Hide(forward, immediately, destroy);
    }

    protected override void OnAnimationComplete(bool show)
    {
        if (m_shotShowHide != 0)
        {
            gameObject.SetActive(true);
            m_shotShowHide = show ? 2 : 1;
            return;
        }
        base.OnAnimationComplete(show);
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

        m_btnPause?.SetInteractable(!hide);

        if (hide && (m_behaviour.hiding || ns == 1) || !hide && (m_behaviour.showing || ns == 2)) return;

        var alphaOne = Math.Abs(m_behaviour.canvasGroup.alpha - 1) < float.Epsilon;

        if (hide)
        {
            if(actived && Math.Abs(m_behaviour.canvasGroup.alpha) > float.Epsilon)
                m_behaviour.Hide(false);
        }
        else if (!actived || !alphaOne)
        {
            m_behaviour.Show(false);
        }
    }

    #endregion

    #region 观战切换UI状态

    private void OnBattleEnterWatchState(Event_ e)
    {
        var isWatch = (bool)e.param1;

        m_btnShoot.SafeSetActive(!isWatch);
        m_btnBurst.SafeSetActive(!isWatch);
        m_awakeRoot.SetActive(!isWatch && showAwakeButton);
        m_movementPad.SafeSetActive(!isWatch);
    }

    #endregion

    #region Editor helper

#if UNITY_EDITOR
    private void OnEditorReloadConfig(Event_ e)
    {
        var config = (string)e.param1;
        if (config != "config_combatconfigs") return;

        UpdateInputSettings();
    }
    #endif

    #endregion
}
