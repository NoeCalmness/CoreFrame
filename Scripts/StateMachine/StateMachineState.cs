/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Description
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StateMachineState : PoolObject<StateMachineState>
{
    #region Static functions

    public const int STATE_PROTECT = 1;
    public const int STATE_IDLE    = 3;
    public const int STATE_RUN     = 4;
    public const int STATE_RUSH    = 5;
    public const int STATE_DIE     = 8;
    public const int STATE_REVIVE  = 89;
    public const int STATE_VICTORY = 889;
    public const int STATE_STAND   = 901;
    public const int STATE_LAYDOWN = 999;

    public static readonly Vector3_ defaultFallSpeed = new Vector3_(0, -9.8, 0);

    public static StateMachineState Create(StateMachine stateMachine, StateMachineInfo parent, StateMachineInfo.StateDetail info, SkilLEntry skill)
    {
        var state = Create(false);

        state.stateMachine = stateMachine;
        state.parent       = parent;
        state.info         = info;

        state.OnInitialize();

        if (skill.valid)
        {
            state.m_skill = skill.skillId;
            state.m_level = skill.level;

            state.m_skillDamageMul = state.m_skill > -1 && state.m_level > -1 ? UpSkillInfo.GetOverrideDamage(state.m_skill, state.m_level) : 0;
            state.m_skillDamageMul *= 1 + skill.addtion; // @TODO: ??
        }

        return state;
    }

    #endregion

    public string name = "";
    public int ID { get { return m_ID; } }
    public int skill { get { return m_skill; } }
    public int level { get { return m_level; } }
    public int stateLevel { get { return m_stateLevel; } }
    public double damageMul { get { return m_stateDamageMul + m_skillDamageMul; } }
    public double skillDamageMul { get { return m_skillDamageMul; } }
    public bool waitToDefault { get { return m_waitToDefault; } }
    public bool waitToLayDown { get { return m_waitToLayDown; } }
    public bool waitToLoop { get { return m_waitToLoop; } }
    public bool ended { get { return m_ended; } }

    public int stateNameHash { get; private set; }
    public float normalizedTime { get { return m_normalizedTime; } }
    public int currentFrame{ get { return m_currentFrame; } }
    public int totalFrame{ get { return m_totalFrame; } }
    public int loopCount{ get { return m_loopCount; } }
    public int length { get { return m_length; } }
    public int time { get { return m_time; } }
    public float floatLength { get { return m_floatLength; } }

    public bool isProtect { get; private set; }
    public bool isIdle { get; private set; }
    public bool isRun { get; private set; }
    public bool isDie { get; private set; }
    public bool isVictory { get; private set; }
    public bool isLaydown { get; private set; }
    public bool isExecution { get; private set; }
    public bool isUltimate { get; private set; }
    public bool ignoreElementTrigger { get; private set; }
    public double speed { get { return m_speed; } }
    public float animationSpeed { get { return m_animationSpeed; } }

    public bool acceptInput { get { return m_acceptInput; } }

    public bool preventTurn { get { return info.preventTurn || m_preventNextTurn; } }

    public bool preventNextTurn { get { return m_preventNextTurn; } set { m_preventNextTurn = value; } }

    public bool showOffWeapon { get { return m_showOffWeapon; } }

    public StateMachineInfo.FrameData[] bindInfo { get { return m_bindInfo; } }

    /// <summary>
    /// 是否是技能 判断条件：动作上配了buff或者带攻击盒子就认为是技能
    /// </summary>
    public bool IsSkill { get { return (info.buffs != null && info.buffs.Length > 0) || info.hasAttackBox; } }

    public StateMachineInfo parent;
    public StateMachineInfo.StateDetail info;
    public int frameCount;
    public bool loop;
    public bool noDefaultTrans;
    public bool passive;
    public bool off;

    [NonSerialized]
    public StateMachine stateMachine;

    public List<StateMachineTransition> transitions = new List<StateMachineTransition>();

    public int hideHpSliderCount { get; private set; }
    public int invincibleCount { get; private set; }
    public int weakCount { get; private set; }
    public int etherealCount { get; private set; }
    public int passiveEtherealCount { get; private set; }
    public int rimCount { get; private set; }
    public int darkCount { get; private set; }
    public bool fatal { get; private set; }
    public int lastExitTime { get; private set; }
    public bool coolingDown { get { return info != null && Level.levelTime - lastExitTime < info.coolDown; } }
    public int coolingTime
    {
        get
        {
            var cd = info != null ? info.coolDown - (Level.levelTime - lastExitTime) : 0;
            return cd < 0 ? 0 : cd;
        }
    }

    [SerializeField] private int    m_ID;
    [SerializeField] private int    m_skill              = -1;
    [SerializeField] private int    m_level              = -1;
    [SerializeField] private int    m_stateLevel         = -1;
    [SerializeField] private double m_stateDamageMul     = 1.0;
    [SerializeField] private double m_skillDamageMul     = 1.0;
    [SerializeField] private double m_speed              = 1.0;
    [SerializeField] private float  m_animationSpeed     = 1.0f;
    [SerializeField] private int    m_time               = 0;
    [SerializeField] private int    m_length             = 1;
    [SerializeField] private float  m_floatLength        = 0.001f;
    [SerializeField] private float  m_normalizedTime     = 0;
    [SerializeField] private int    m_currentFrame       = 0;
    [SerializeField] private int    m_totalFrame         = 1;
    [SerializeField] private int    m_loopCount          = 0;
    [SerializeField] private int    m_acceptKeys         = 0;
    [SerializeField] private bool   m_acceptInput        = true;
    [SerializeField] private bool   m_waitToResetKey     = false;
    [SerializeField] private bool   m_waitToDefault      = false;
    [SerializeField] private bool   m_waitToLayDown      = false;
    [SerializeField] private bool   m_waitToLoop         = false;
    [SerializeField] private bool   m_ended              = false;
    [SerializeField] private bool   m_preventNextTurn    = false;
    [SerializeField] private bool   m_showOffWeapon      = false;
    [SerializeField] private bool   m_rimLight           = true;
    [SerializeField] private int    m_holdRimCount       = 0;

    [Space(5)]
    [SerializeField] private bool m_inMotion = false;
    [SerializeField] private bool m_falling  = false;
    [SerializeField] private int  m_fallTime = 0;

    private readonly List<int> m_toughs = new List<int>();
    private readonly List<int> m_inheritEffectsFrame = new List<int>();
    private readonly List<Effect> m_inheritEffects = new List<Effect>();
    private readonly List<int> m_audios = new List<int>();

    private AnimMotionInfo m_motion;
    private Vector4_ m_motionFrame;

    private StateMachineInfo.FrameData[] m_bindInfo = null;

    private int m_curSectionIdx;
    private int m_curInvisibleIndex;
    private int m_curDarkIndex;
    private int m_curRimIndex;
    private int m_curWeakIndex;
    private int m_curToughIdx;
    private int m_curBuffIndex;
    private int m_curEffectIdx;
    private int m_curFlyingEffectIdx;
    private int m_curSoundEffectIdx;
    private int m_curShakeIdx;
    private int m_curHideIdx;
    private int m_curHideOffIdx;
    private int m_curHidePetIdx;
    private int m_curActorIdx;

    protected StateMachineState() { }

    protected override void OnInitialize()
    {
        m_speed              = 1.0;
        m_animationSpeed     = 1.0f;
        m_time               = 0;
        m_normalizedTime     = 0;
        m_currentFrame       = 0;
        m_totalFrame         = 1;
        m_loopCount          = 0;
        m_acceptKeys         = 0;
        m_acceptInput        = true;
        m_waitToResetKey     = false;
        m_waitToDefault      = false;
        m_waitToLayDown      = false;
        m_waitToLoop         = false;
        m_ended              = false;
        m_preventNextTurn    = false;
        m_rimLight           = true;
        m_holdRimCount       = 0;

        name               = info.state;
        stateNameHash      = info.hash;
        m_ID               = info.ID;
        m_stateLevel       = info.level;
        m_stateDamageMul   = info.damageMul;
        m_skill            = -1;
        m_level            = -1;
        m_skillDamageMul   = 0;
        m_loopCount        = 0;
        m_length           = 1;
        m_floatLength      = 0.001f;
        frameCount         = 1;
        loop               = false;

        m_acceptKeys = 0;

        isProtect   = m_ID == STATE_PROTECT;
        isIdle      = m_ID == STATE_IDLE || m_ID == STATE_STAND;
        isRun       = m_ID == STATE_RUN;
        isDie       = m_ID == STATE_DIE;
        isVictory   = m_ID == STATE_VICTORY;
        isLaydown   = m_ID == STATE_LAYDOWN;

        isExecution = info.execution;
        isUltimate  = info.ultimate;

        off     = parent is StateMachineInfoOff;
        passive = parent is StateMachineInfoPassive;

        ignoreElementTrigger = info.ignoreElementTrigger;

        m_showOffWeapon = info.showOffweapon || off;

        noDefaultTrans = info.noDefaultTrans;

        lastExitTime = -info.coolDown;

        m_bindInfo = off ? stateMachine.info.offWeaponBinds : passive ? stateMachine.info.passiveWeaponBinds : stateMachine.info.defaultWeaponBinds;

        AnimClipInfo c = null;
        if (stateMachine.creature.gender == 0)
        {
            var an = info.animation.Replace("_nan_", "_nv_");
            c = ConfigManager.Find<AnimClipInfo>(a => a.name == an);
        }

        if (!c) c = ConfigManager.Find<AnimClipInfo>(a => a.name == info.animation);
        if (c)
        {
            frameCount = c.frameCount;
            loop = c.loop;
            m_length = frameCount * 33;
            m_floatLength = m_length * 0.001f;
        }

        ResetMotion();

#if UNITY_EDITOR
        EventManager.AddEventListener("EditorReloadConfig", OnEditorReloadConfig);
#endif
    }

    protected override void OnDestroy()
    {
        foreach (var trans in transitions) trans.Destroy();
        transitions.Clear();

        m_inheritEffectsFrame.Clear();
        m_inheritEffects.Clear();
        m_audios.Clear();

        ClearSpecialStates();

        m_bindInfo = null;

#if UNITY_EDITOR
        EventManager.RemoveEventListener(this);
#endif
    }

    public void UpdateTransitions(List<TransitionInfo> groups)
    {
        transitions.Clear();

        m_acceptKeys = 0;

        foreach (var group in groups)
        {
            var trans = group.GetTransitions(info.ID);
            if (trans == null) continue;

            foreach (var tran in trans)
            {
                var t = StateMachineTransition.Create(stateMachine, tran);
                if (!t) continue;

                transitions.Add(t);

                if (t.info.acceptKeyMask == 0) continue;

                m_acceptKeys |= t.info.acceptKeyMask;
            }
        }
    }

    public bool AcceptKey(int key)
    {
        return m_acceptKeys.BitMask(key);
    }

    public virtual void Enter()
    {
        #region Debug log
        #if (DEVELOPMENT_BUILD || UNITY_EDITOR) && DETAIL_BATTLE_LOG
        if (!m_waitToLoop) Logger.LogDetail("[{2}:{3}-{4}], Creature {0} enter state [{1}]", stateMachine.creature.name, name, Level.levelTime, stateMachine.creature.frameTime, stateMachine.creature.frameCount);
        #endif

        #if AI_LOG
        if (!m_waitToLoop) Module_AI.LogBattleMsg(stateMachine.creature,true, "[enter state [{0}]", name);
        #endif
        #endregion

        var c = stateMachine.creature;

        m_speed = isRun ? c.speedRun : !passive && !info.ignoreAttackSpeed && !isIdle && !isDie ? c.speedAttack : 1.0;
        m_animationSpeed = (float)(stateMachine.speed * m_speed);

        var overflow = 0;

        if (!m_waitToLoop)
        {
            m_loopCount          = 0;
            m_totalFrame         = 0;
        }
        else
        {
            overflow = m_time % m_length;
            ++m_loopCount;
        }

        m_time               = 0;
        m_normalizedTime     = 0;
        m_currentFrame       = 0;
        m_curSectionIdx      = 0;
        m_curInvisibleIndex  = 0;
        m_curDarkIndex       = 0;
        m_curRimIndex        = 0;
        m_curWeakIndex       = 0;
        m_curToughIdx        = 0;
        m_curBuffIndex       = 0;
        m_curEffectIdx       = 0;
        m_curFlyingEffectIdx = 0;
        m_curSoundEffectIdx  = 0;
        m_curShakeIdx        = 0;
        m_curHideIdx         = 0;
        m_curHideOffIdx      = 0;
        m_curHidePetIdx      = 0;
        m_curActorIdx        = 0;

        if (stateMachine.GetInt(StateMachineParam.state) != m_ID)
        {
            m_inheritEffectsFrame.Clear();
            foreach (var effect in m_inheritEffects) effect.Destroy();
            m_inheritEffects.Clear();
        }

        if (isLaydown)
        {
            m_length = stateMachine.GetInt(StateMachineParam.nextLayDownTime);
            if (m_length < 1) m_length = 33;
            frameCount = m_length / 33;
            m_floatLength = m_length * 0.001f;

            etherealCount = stateMachine.GetInt(StateMachineParam.prevEthreal);
            c.etherealCount += etherealCount;

            passiveEtherealCount = stateMachine.GetInt(StateMachineParam.prevPassiveEthreal);
            c.passiveEtherealCount += passiveEtherealCount;
        }
        else if (!m_waitToLoop)
        {
            etherealCount = 0;
            passiveEtherealCount = 0;
        }

        stateMachine.SetParam(StateMachineParam.totalFrame, m_totalFrame);
        stateMachine.SetParam(StateMachineParam.loopCount, m_loopCount);
        stateMachine.SetParam(StateMachineParam.stateType, off ? 1 : passive ? 2 : 0);
        stateMachine.SetParam(StateMachineParam.prevGroupMask, stateMachine.GetLong(StateMachineParam.groupMask));
        stateMachine.SetParam(StateMachineParam.groupMask, info.groupMask);
        stateMachine.SetParam(StateMachineParam.targetGroupMask, 0);
        stateMachine.SetParam(StateMachineParam.targetHitGroupMask, 0);
        stateMachine.SetParam(StateMachineParam.targetConfigID, 0);
        stateMachine.SetParam(StateMachineParam.forcePassive, 0);
        stateMachine.SetParam(StateMachineParam.frame, 0);
        stateMachine.SetParam(StateMachineParam.section, 0);
        stateMachine.SetParam(StateMachineParam.process, 0);
        stateMachine.SetParam(StateMachineParam.key, 0);
        stateMachine.SetParam(StateMachineParam.state, m_ID);
        stateMachine.SetParam(StateMachineParam.exit, info.processes.y < 1);
        stateMachine.SetParam(StateMachineParam.moveBreak, false);
        stateMachine.SetParam(StateMachineParam.hit, false);
        stateMachine.SetParam(StateMachineParam.hited, 0);
        stateMachine.SetParam(StateMachineParam.immuneDeath, false);
        stateMachine.SetParam(StateMachineParam.nextLayDownTime, 0);
        stateMachine.SetParam(StateMachineParam.prevEthreal, 0);
        stateMachine.SetParam(StateMachineParam.prevPassiveEthreal, 0);
        stateMachine.SetParam(StateMachineParam.hasLaydownTime, info.layDownTime > 0);
        stateMachine.SetParam(StateMachineParam.catchState, info.catchState);

        m_toughs.Clear();

        invincibleCount = 0;
        weakCount       = 0;
        rimCount        = 0;
        darkCount       = 0;
        fatal           = false;

        m_rimLight      = true;
        m_holdRimCount  = 0;

        m_acceptInput   = true;

        fallSpeed       = defaultFallSpeed;
        m_falling       = false;
        m_fallTime      = 0;
        m_inMotion      = m_motion;

        currentMotionPos = m_waitToLoop ? Vector3_.zero : motionOffset;
        if (!stateMachine.creature.isForward) currentMotionPos.x = -currentMotionPos.x;

        currentMotionPos += stateMachine.creature.position_;

        m_motionIndex = m_motionFrame.y < 1 ? 0 : (int)m_motionFrame.x;

        m_waitToResetKey = false;
        m_waitToDefault  = false;
        m_waitToLayDown  = false;
        m_waitToLoop     = false;
        m_ended          = false;

        var cc = c.behaviour;

        cc.attackCollider.SetAttackBox(StateMachineInfo.AttackBox.empty, StateMachineInfo.Effect.empty, c.isForward);

        cc.SetWeaponVisibility(-1, m_showOffWeapon, true);
        cc.SetWeaponVisibility(-1, true, false);

        if (c.pet) c.pet.enabledAndVisible = c.visible;

        cc.SetWeaponBind(m_bindInfo);

        if (off) cc.SetWeaponBind(parent.defaultWeaponBinds, true);

        c.SetLayer(-1);

        if (c.shadow) c.shadow.gameObject.SetActive(!info.hideShadow);

        if (c.springManager)
        {
            c.springManager.SetState(c.useSpringBone && !info.disableSpringManager);
            c.springManager.SetExtendColliderState(isVictory);
        }

        HandleFrameEvent(); // Initialize 0 frame actions

        if (overflow > 0) _Update(overflow);

        if (isLaydown) stateMachine.creature.UpdateAnimatorForce();
    }

    public virtual void Quit()
    {
        ClearSpecialStates();

        foreach (var audio in m_audios) AudioManager.Stop(audio);
        m_audios.Clear();

        lastExitTime = Level.levelTime;

        if (info.coolGroup != 0)
        {
            var g = stateMachine.GetCoolGroup(info.coolGroup);
            if (g == null) return;

            foreach (var s in g)
            {
                if (s == this) continue;
                s.lastExitTime = lastExitTime;
            }
        }
    }

    public void Update(int diff)
    {
        if (m_speed != 1.0) diff = (int)(diff * m_speed);

        _Update(diff);
    }

    private void _Update(int diff)
    {
        if (info.fatalRadius > 0 && stateMachine.passiveSource)
        {
            var fatal_ = Vector3_.Distance(stateMachine.passiveSource.position_, stateMachine.creature.position_) < info.fatalRadius;
            if (fatal ^ fatal_)
                stateMachine.passiveSource.stateMachine.SetParam(StateMachineParam.fatal, fatal_ ? 1 : -1, true);
            fatal = fatal_;
        }

        if (m_waitToResetKey)
        {
            m_waitToResetKey = false;
            stateMachine.SetParam(StateMachineParam.key, 0);
            stateMachine.SetParam(StateMachineParam.process, 2);
            stateMachine.SetParam(StateMachineParam.moveBreak, true);
        }

        if (!stateMachine.playable) return;

        if (m_falling)
        {
            if (!stateMachine.creature.onGround)
            {
                m_fallTime += diff;
                return;
            }

            m_falling = false;
            m_fallTime = 0;
        }

        m_time += diff;

        var frames = m_time / 33 - m_currentFrame;
        m_normalizedTime = (float)m_time / m_length;

        IncreaseFrame(frames);

        UpdateAnimMotion();

        if (loop && m_currentFrame >= frameCount) m_waitToLoop = true;

        if (ended && !noDefaultTrans && !stateMachine.GetBool(StateMachineParam.isDead)) m_waitToDefault = true;
    }

    public void RefreshCurrentFrameState()
    {
        var c = stateMachine.creature;
        var cc = c.behaviour;

        var attackBox = m_curSectionIdx < 1 ? StateMachineInfo.AttackBox.empty : info.sections[m_curSectionIdx - 1].attackBox;

        cc.attackCollider.SetAttackBox(attackBox, m_curSectionIdx < 1 ? StateMachineInfo.Effect.empty : stateMachine.GetHitEffect(off), c.isForward);

        if (m_curHideIdx < 1) cc.SetWeaponVisibility(-1, true, false);
        else
        {
            var hide = info.hideWeapons[m_curHideIdx - 1].disable;
            cc.SetWeaponVisibility(-1, !hide, false);
        }

        if (m_curHideOffIdx < 1) cc.SetWeaponVisibility(-1, m_showOffWeapon, true);
        else
        {
            var hide = info.hideOffWeapons[m_curHideOffIdx - 1].disable;
            cc.SetWeaponVisibility(-1, m_showOffWeapon && !hide, true);
        }

        if (c.pet) c.pet.visible = m_curHidePetIdx < 1 || !info.hidePets[m_curHidePetIdx - 1].disable;

        cc.SetWeaponBind(m_bindInfo);

        if (off) cc.SetWeaponBind(parent.defaultWeaponBinds, true);

        if (m_curInvisibleIndex < 1) c.SetLayer(-1);
        else
        {
            var invisible = info.invisibles[m_curInvisibleIndex - 1].disable;
            c.SetLayer(invisible ? Layers.INVISIBLE : -1, invisible);
        }

        if (c.shadow) c.shadow.gameObject.SetActive(!info.hideShadow);

        if (c.springManager)
        {
            c.springManager.SetState(c.useSpringBone && !info.disableSpringManager);
            c.springManager.SetExtendColliderState(isVictory);
        }
    }

    private void IncreaseFrame(int count)
    {
        if (count < 1) return;
        for (var i = 0; i < count; ++i)
        {
            m_ended = ++m_currentFrame == frameCount;

            FightRecordManager.RecordLog<LogStateFrame>(l =>
            {
                l.tag = (byte) TagType.StateFrame;
                l.frame = (ushort)m_currentFrame;
            });

            stateMachine.SetParam(StateMachineParam.frame, m_currentFrame);
            stateMachine.SetParam(StateMachineParam.totalFrame, m_totalFrame++);

            if (ended)
            {
                if (!isLaydown && passive && !loop && info.layDownTime > 0)
                {
                    m_waitToLayDown = true;
                    stateMachine.SetParam(StateMachineParam.nextLayDownTime, info.layDownTime);
                }

                stateMachine.onStateEnded?.Invoke(this);

                break;
            }

            HandleFrameEvent();    // m_currentFrame starts with 1, Frame 0 already handled in Enter()
        }
    }

    private void HandleFrameEvent()
    {
        var creature = stateMachine.creature;

        if (m_currentFrame == info.processes.x)
        {
            stateMachine.SetParam(StateMachineParam.exit, true);
            stateMachine.SetParam(StateMachineParam.process, 1);
        }

        if (m_currentFrame == info.processes.y) m_waitToResetKey = true; // Clear key input next update

        if (info.ignoreInput.y > 0 && info.ignoreInput.x != info.ignoreInput.y)
        {
            if      (m_currentFrame == info.ignoreInput.x) m_acceptInput = false;
            else if (m_currentFrame == info.ignoreInput.y) m_acceptInput = true;
        }

        if (m_currentFrame == info.landingFrame && !creature.onGround)
        {
            m_falling = true;
            if (m_motionIndex > 1)
            {
                var velocity = (m_motion.points[m_motionIndex] - m_motion.points[m_motionIndex - 1]) * 0.3;
                if (velocity.magnitude == 0) velocity = defaultFallSpeed;
                else velocity = new Vector3_(!stateMachine.creature.isForward ? -velocity.z : velocity.z, velocity.y >= 0 ? defaultFallSpeed.y : velocity.y, -velocity.x);

                #region Debug log
                #if DEVELOPMENT_BUILD || UNITY_EDITOR
                if (velocity.z != 0)
                    Logger.LogError("<color=#00DDFF><b>[{6}:{5}]</b></color>, state <color=#00DDFF><b>[{0}:{1}]</b></color> has invalid landing frame <color=#00DDFF><b>[{2}]</b></color>, fallSpeed has Z velocity <color=#00DDFF><b>[{3}]</b></color>, motion ID: <color=#00DDFF><b>[{4}]</b></color>, last frame: <color=#00DDFF><b>[{7}]</b></color>",
                        ID, name, info.landingFrame, velocity.z, m_motion.ID, parent.ID, parent.GetType(), m_motion.points[m_motionIndex - 1]);
                #endif
                #endregion

                velocity.z = 0; // @TODO: Fix velocity validation
                fallSpeed = velocity;
            }
        }

        if (m_curSectionIdx < info.sections.Length && info.sections[m_curSectionIdx].startFrame == m_currentFrame)
        {
            creature.behaviour.attackCollider.SetAttackBox(info.sections[m_curSectionIdx].attackBox, stateMachine.GetHitEffect(off), creature.isForward);

            ++m_curSectionIdx;
            stateMachine.SetParam(StateMachineParam.section, m_curSectionIdx);
        }

        if (m_curInvisibleIndex < info.invisibles.Length && info.invisibles[m_curInvisibleIndex].startFrame == m_currentFrame)
        {
            var invisible = info.invisibles[m_curInvisibleIndex].disable;
            creature.SetLayer(invisible ? Layers.INVISIBLE : -1, invisible);

            ++m_curInvisibleIndex;
        }
        
        if (m_curWeakIndex < info.weaks.Length && info.weaks[m_curWeakIndex].startFrame == m_currentFrame)
        {
            if (info.weaks[m_curWeakIndex].disable)
            {
                creature.weakCount++;
                weakCount++;
            }
            else if (weakCount > 0)
            {
                creature.weakCount--;
                weakCount--;
            }

            ++m_curWeakIndex;
        }

        if (m_curToughIdx < info.toughs.Length && info.toughs[m_curToughIdx].startFrame == m_currentFrame)
        {
            var idx = m_toughs.Count - 1;
            if (idx > -1)
            {
                creature.RemoveToughState(m_toughs[idx]);
                m_toughs.RemoveAt(idx);

                UpdateRimLightState(-1);
            }

            var tlvl = info.toughs[m_curToughIdx].intValue0;

            if (tlvl > 0)
            {
                m_toughs.Add(creature.AddToughState(tlvl));

                UpdateRimLightState(1);
            }

            ++m_curToughIdx;
        }

        if (info.invincible.y > 0)
        {
            if (m_currentFrame == info.invincible.x)
            {
                creature.invincibleCount++;
                invincibleCount++;

                UpdateRimLightState(1);
            }
            if (m_currentFrame == info.invincible.y + 1)
            {
                creature.invincibleCount--;
                invincibleCount--;

                UpdateRimLightState(-1);
            }
        }

        if (info.hideHpSlider.y > 0)
        {
            if (m_currentFrame == info.hideHpSlider.x)
            {
                hideHpSliderCount++;
                creature.hpVisiableCount--;
            }
            if (m_currentFrame == info.hideHpSlider.y + 1)
            {
                hideHpSliderCount--;
                creature.hpVisiableCount++;
            }
        }

        if (info.ethereal.y > 0)
        {
            if (m_currentFrame == info.ethereal.x)
            {
                creature.etherealCount++;
                etherealCount++;
            }
            if (m_currentFrame == info.ethereal.y + 1)
            {
                creature.etherealCount--;
                etherealCount--;
            }
        }

        if (info.passiveEthereal.y > 0)
        {
            if (m_currentFrame == info.passiveEthereal.x)
            {
                creature.passiveEtherealCount++;
                passiveEtherealCount++;
            }
            if (m_currentFrame == info.passiveEthereal.y + 1)
            {
                creature.passiveEtherealCount--;
                passiveEtherealCount--;
            }
        }

        while (m_curBuffIndex < info.buffs.Length && info.buffs[m_curBuffIndex].startFrame == m_currentFrame)
        {
            var b = info.buffs[m_curBuffIndex];

            if (b.doubleValue0 == 0 || Module_Battle.Range() < b.doubleValue0)
            {
                var pet = creature as PetCreature;
                if (pet && b.intValue0 <= 0)
                {
                    var skill = pet.GetSkillByState(name);
                    if (skill)
                    {
                        var count = skill.skillInfo.buffs.Length;
                        for (int i = 0; i < count; i++)
                            Buff.Create(skill.skillInfo.buffs[i], creature, creature, 0, -1, pet.petInfo.AdditiveLevel);
                    }
                }
                else
                    Buff.Create(b.intValue0, creature, null, b.intValue1);
            }

            ++m_curBuffIndex;
        }

        var effects = info.effects;
        while (m_curEffectIdx < effects.Length && effects[m_curEffectIdx].startFrame == m_currentFrame)
        {
            if (m_inheritEffectsFrame.Contains(m_curEffectIdx))
            {
                ++m_curEffectIdx;
                continue;
            }

            if (!creature.isPlayer && effects[m_curEffectIdx].self)
            {
                ++m_curEffectIdx;
                continue;
            }
            
            var eff = creature.behaviour.effects.PlayEffect(effects[m_curEffectIdx]);
            if (eff)
            {
                eff.localTimeScale = animationSpeed;
                if (eff.name == "effect_ringring" && info.fatalRadius > 0)
                {
                    eff.lifeTime = -1;
                    eff.localScale = Vector3.one * (float)info.fatalRadius;
                }
            }

            if (effects[m_curEffectIdx].inherit)
            {
                m_inheritEffectsFrame.Add(m_curEffectIdx);
                m_inheritEffects.Add(eff);
            }

            ++m_curEffectIdx;
        }

        while (m_curFlyingEffectIdx < info.flyingEffects.Length && info.flyingEffects[m_curFlyingEffectIdx].startFrame == m_currentFrame)
        {
            creature.behaviour.effects.PlayEffect(info.flyingEffects[m_curFlyingEffectIdx], stateMachine.GetHitEffect(off));

            ++m_curFlyingEffectIdx;
        }

        while (m_curSoundEffectIdx < info.soundEffects.Length && info.soundEffects[m_curSoundEffectIdx].startFrame == m_currentFrame)
        {
            var se = info.soundEffects[m_curSoundEffectIdx];
            ++m_curSoundEffectIdx;

            if (se.isEmpty || !se.global && !creature.isPlayer || UnityEngine.Random.Range(0, 1.0f) > se.rate)
                continue;

            var s = se.SelectSound(creature.roleProto, creature.gender);
            if (!s.isEmpty)
            {
                if (!se.interrupt) AudioManager.PlayAudio(s.sound, se.isVoice || s.isVoice ? AudioTypes.Voice : AudioTypes.Sound, false, se.overrideType);
                else AudioManager.PlayAudio(s.sound, se.isVoice || s.isVoice ? AudioTypes.Voice : AudioTypes.Sound, false, se.overrideType, h => { if (h) m_audios.Add(h.id); });
            }
        }

        if (m_curDarkIndex < info.darkScreens.Length && info.darkScreens[m_curDarkIndex].startFrame == m_currentFrame)
        {
            var alpha = (float)info.darkScreens[m_curDarkIndex].doubleValue0;
            var draw = alpha > 0;

            darkCount += draw ? 1 : -1;
            Camera_Combat.Dark(draw ? 1 : -1, alpha);

            ++m_curDarkIndex;
        }

        if (m_curRimIndex < info.rimLights.Length && info.rimLights[m_curRimIndex].startFrame == m_currentFrame)
        {
            m_rimLight = !info.rimLights[m_curRimIndex].disable;

            UpdateRimLightState();

            ++m_curRimIndex;
        }

        if (m_curShakeIdx < info.cameraShakes.Length && info.cameraShakes[m_curShakeIdx].startFrame == m_currentFrame)
        {
            var shake = ConfigManager.Get<CameraShakeInfo>(info.cameraShakes[m_curShakeIdx].intValue0);
            if (shake) Camera_Combat.Shake(shake.intensity, shake.duration * 0.001f, shake.range, creature);

            ++m_curShakeIdx;
        }

        if (m_curHideIdx < info.hideWeapons.Length && info.hideWeapons[m_curHideIdx].startFrame == m_currentFrame)
        {
            var hide = info.hideWeapons[m_curHideIdx].disable;
            creature.behaviour.SetWeaponVisibility(-1, !hide, false);

            ++m_curHideIdx;
        }

        if (m_curHideOffIdx < info.hideOffWeapons.Length && info.hideOffWeapons[m_curHideOffIdx].startFrame == m_currentFrame)
        {
            var hide = info.hideOffWeapons[m_curHideOffIdx].disable;
            creature.behaviour.SetWeaponVisibility(-1, m_showOffWeapon && !hide, true);

            ++m_curHideOffIdx;
        }

        if (m_curHidePetIdx < info.hidePets.Length && info.hidePets[m_curHidePetIdx].startFrame == m_currentFrame)
        {
            if (creature.pet)
            {
                var hidePet = info.hidePets[m_curHidePetIdx].disable;
                creature.pet.enabledAndVisible = !hidePet;
            }

            ++m_curHidePetIdx;
        }

        if (m_curActorIdx < info.sceneActors.Length && info.sceneActors[m_curActorIdx].frame == m_currentFrame)
        {
            var a = info.sceneActors[m_curActorIdx];
            var actor = (Level.current as Level_Battle)?.CreateSceneActor(
                a.actorID, 
                creature.position_ + a.offset * (creature.direction == CreatureDirection.FORWARD ? 1 :-1),  
                creature.direction, a.groupID, 
                a.level);
            ++m_curActorIdx;
            if (actor == null) return;

            actor.SetCreatureCamp(creature.creatureCamp);
            BuffContext c = new BuffContext
            {
                id = 444,
                level = 1,
                owner = actor,
                delay = a.lifeTime
            };
            Buff.Create(c);
        }
    }

    public bool falling { get { return m_falling; } }
    public Vector3_ fallSpeed { get; protected set; }
    public int fallTime { get { return m_fallTime; } }
    public bool inMotion { get { return m_inMotion; } }

    private int m_motionIndex;

    private void UpdateAnimMotion()
    {
        var idx = (int)m_motionFrame.x + m_currentFrame;
        var max = (int)m_motionFrame.y;

        if (!m_motion || m_falling || idx < 1 || idx > max && m_motionIndex >= max)
        {
            m_inMotion = false;
            return;
        }

        m_inMotion = true;

        if (idx > max) idx = max;

        m_motionIndex = idx;

        var next = m_motion.points[m_motionIndex];
        var prev = m_motion.points[m_motionIndex - 1];
        var prog = idx == max ? 1.0 : (m_time % 33) / 33.0;
        var tar = (Vector3_.Lerp(prev, next, prog) - m_motion.points[0]) * 0.01;

        var c = stateMachine.creature;
        currentMotionPos = new Vector3_(!c.isForward ? -tar.z : tar.z, tar.y, -tar.x) + c.motionOrigin;

        if (currentMotionPos.y < 0)
        {
            FightRecordManager.RecordLog<LogVector3>(l =>
            {
                l.tag = (byte)TagType.motionOrigin;
                l.pos = new double[] { tar.x, tar.y, tar.z };
            });

            FightRecordManager.RecordLog<LogVector3>(l =>
            {
                l.tag = (byte)TagType.motionOrigin;
                l.pos = new double[] { c.motionOrigin.x, c.motionOrigin.y, c.motionOrigin.z };
            });

            FightRecordManager.RecordLog<LogVector3>(l =>
            {
                l.tag = (byte) TagType.currentMotionPos;
                l.pos = new double[3];
                l.pos[0] = currentMotionPos.x;
                l.pos[1] = currentMotionPos.y;
                l.pos[2] = currentMotionPos.z;
            });
            currentMotionPos.y = 0;
        }
    }

    public Vector3 SimulateMotion(float time, CreatureDirection dir, Vector3_ origin)
    {
        if (!m_motion || m_motionFrame.x < 1) return stateMachine.creature.position_;

        if (time < 0) time = 0;

        var itime = (int)(time * 1000);
        var frame = itime / 33;
        var prog  = (itime % 33) / 33.0;

        var idx = (int)m_motionFrame.x + frame;

        idx = Mathd.Clamp(idx, (int)m_motionFrame.x, (int)m_motionFrame.y);

        var next = m_motion.points[idx];
        var prev = m_motion.points[idx - 1];

        var tar = (Vector3_.Lerp(prev, next, prog) - m_motion.points[0]) * 0.01;

        tar = new Vector3_(dir == CreatureDirection.BACK ? -tar.z : tar.z, tar.y, -tar.x) + origin;

        if (tar.y < 0) tar.y = 0;

        return tar;
    }

    public Vector3_ currentMotionPos;
    public Vector3_ motionOffset;

    private void ResetMotion()
    {
        m_motion = ConfigManager.Get<AnimMotionInfo>((int)(info.animMotion.x));

        motionOffset = Vector3_.zero;

        if (m_motion)
        {
            var count = m_motion.points.Length;

            if (count < 2) count = 0;

            m_motionFrame.x = Mathd.Clamp(info.animMotion.y, 1, count - 1);
            m_motionFrame.y = info.animMotion.z  < 1 ? count - 1 : Mathd.Clamp(info.animMotion.z, info.animMotion.x, count - 1);
            m_motionFrame.z = (m_motionFrame.y - m_motionFrame.x + 1);
            m_motionFrame.w = m_motionFrame.z * 0.01; // cm -> m

            if (m_motion.offset != Vector3_.zero)
                motionOffset += m_motion.offset * 0.01;

            motionOffset.Set(motionOffset.z, motionOffset.y, motionOffset.x);
        }
        else m_motionFrame = Vector4_.zero;
    }

    private void ClearSpecialStates()
    {
        var c = stateMachine.creature;

        c.RemoveToughState(m_toughs);
        m_toughs.Clear();

        if (invincibleCount      > 0) c.invincibleCount      -= invincibleCount;
        if (weakCount            > 0) c.weakCount            -= weakCount;
        if (etherealCount        > 0) c.etherealCount        -= etherealCount;
        if (passiveEtherealCount > 0) c.passiveEtherealCount -= passiveEtherealCount;
        if (rimCount             > 0) c.rimCount             -= rimCount;
        if (hideHpSliderCount    > 0) c.hpVisiableCount      += hideHpSliderCount;

        if (darkCount > 0) Camera_Combat.Dark(-darkCount, 0);

        ClearSourcePassive(stateMachine.passiveSource);

        stateMachine.SetParam(StateMachineParam.prevEthreal,        etherealCount);
        stateMachine.SetParam(StateMachineParam.prevPassiveEthreal, passiveEtherealCount);

        hideHpSliderCount    = 0;
        invincibleCount      = 0;
        weakCount            = 0;
        etherealCount        = 0;
        passiveEtherealCount = 0;
        rimCount             = 0;
        darkCount            = 0;
        fatal                = false;

        m_holdRimCount       = 0;
        m_rimLight           = true;
    }

    public void ClearSourcePassive(Creature source)
    {
        if (fatal && source)
            source.stateMachine.SetParam(StateMachineParam.fatal, -1, true);
    }

    private void UpdateRimLightState(int hold = 0)
    {
        m_holdRimCount += hold;

        var c = stateMachine.creature;
        if (!c) return;

        if (m_rimLight)
        {
            if (rimCount > 0) c.rimCount -= rimCount;

            c.rimCount += m_holdRimCount;
            rimCount = m_holdRimCount;
        }
        else
        {
            c.rimCount -= m_holdRimCount;
            rimCount = 0;
        }
    }

#if UNITY_EDITOR
    private void OnEditorReloadConfig(Event_ e)
    {
        var config = (string)e.param1;
        if (config == "config_animmotioninfos") ResetMotion();
        if (config == "config_upskillinfos")    m_skillDamageMul = m_skill > -1 && m_level > -1 ? UpSkillInfo.GetOverrideDamage(m_skill, m_level) : 0;
    }
#endif
}
