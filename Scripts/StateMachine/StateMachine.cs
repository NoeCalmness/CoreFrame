/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * FSM class for battle system.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-11
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnStateEnded(StateMachineState state);
public delegate void OnEnterState(StateMachineState old, StateMachineState now, float overrideBlendTime = -1);
public delegate void OnQuitState(StateMachineState old, StateMachineState now, float overrideBlendTime = -1);
public delegate void OnRebuild();

/// <summary>
/// FSM class for battle system.
/// Custom implement of AnimatorStateMachine
/// </summary>
[Serializable]
public class StateMachine : PoolObject<StateMachine>
{
    #region Static functions

    public const int MAX_PARAMS_COUNT = 50;
    public const int PASSIVE_CATCH = 0, PASSIVE_TOUGH = 1, PASSIVE_WEAK = 2;

    public static StateMachine Create(StateMachineInfo info, Creature creature, bool isAI, params StateMachineInfo[] subStateMachines)
    {
        var sm = Create(false);
        sm.info = info;
        sm.creature = creature;
        sm.animator = creature.animator;
        sm.transtionGroupId = info == null ? 0 : isAI ? info.aiTransitionGroup : info.transitionGroup;

        sm.subStateMachines.AddRange(subStateMachines);
        sm.subStateMachines.RemoveAll(s => !s);

        sm.OnInitialize();

        return sm;
    }

    #endregion

    public double speed = 1.0;

    /// <summary>
    /// 是否可播放
    /// 等同于 !freezing && !pending
    /// </summary>
    public bool playable
    {
        get { return m_playable; }
        set
        {
            if (m_playable == value) return;
            m_playable = value;

            #region Debug log
            #if (DEVELOPMENT_BUILD || UNITY_EDITOR) && DETAIL_BATTLE_LOG
            Logger.LogInfo("[{3}:{4}-{5}], [{6}:{7}] Freez state changed: {0}, {1}, {2}", m_playable, m_freezCount, m_pending, Level.levelTime, creature.frameTime, creature.frameCount, creature.id, creature.name);
            #endif

            #if AI_LOG
            Module_AI.LogBattleMsg(creature, "[ Freez state changed: {0}, {1}, {2}]", m_playable, m_freezCount, m_pending);
            #endif
            #endregion

            creature.DispatchEvent(CreatureEvents.STATE_PLAYABLE, Event_.Pop(m_playable));
        }
    }
    /// <summary>
    /// 是否处于冻结状态
    /// 冻结状态下，状态不会执行帧更新，除被动外的所有迁移默认都无法执行 除非设置忽略
    /// </summary>
    public bool freezing { get; protected set; }
    /// <summary>
    /// 冻结数量
    /// 数量大于 0 时 表示处于冻结状态
    /// </summary>
    public int freezCount
    {
        get { return m_freezCount; }
        set
        {
            var old = freezing;
            m_freezCount = value < 0 ? 0 : value;
            freezing = m_freezCount > 0;

            if (old ^ freezing)
            {
                SetParam(StateMachineParam.freezing, freezing);
                creature.DispatchEvent(CreatureEvents.FREEZ_STATE, Event_.Pop(freezing));
            }

            playable = m_freezCount < 1 && !m_pending;
        }
    }
    /// <summary>
    /// 是否处于挂起状态
    /// 挂起状态下，状态不会执行帧更新
    /// </summary>
    public bool pending
    {
        get { return m_pending; }
        protected set
        {
            m_pending = value;

            if (!m_pending) m_pendingTime = 0;

            creature.behaviour.Shake(!m_preventShake && (creature.tough || currentState.passive) && m_pending);

            playable = m_freezCount < 1 && !m_pending;
        }
    }

    [SerializeField]
    private bool m_playable = true;
    [SerializeField, Set("freezCount")]
    private int m_freezCount = 0;
    [SerializeField, Set("pending")]
    private bool m_pending = false;
    [SerializeField]
    private int m_pendingTime = 0;
    [SerializeField]
    private int m_nextPendingTime = 0;
    [NonSerialized]
    private int m_passiveMask = 0;

    private bool m_preventShake = false;
    private bool m_forceTransitionUpdate = false;

    public int transtionGroupId;
    public StateMachineInfo info;
    public List<StateMachineInfo> subStateMachines = new List<StateMachineInfo>();
    public List<StateMachineInfo.Effect> hitEffects = new List<StateMachineInfo.Effect>();
    public List<StateMachineInfo.Effect> deadEffects = new List<StateMachineInfo.Effect>();

    [NonSerialized]
    public List<StateMachineState> states = new List<StateMachineState>();
    public List<StateMachineTransition> preStateTransitions = new List<StateMachineTransition>();
    public List<StateMachineTransition> anyStateTransitions = new List<StateMachineTransition>();
    public StateMachineState currentState;

    [HideInInspector]
    public Animator animator;

    public Creature creature;

    public int passiveMask
    {
        get { return m_passiveMask; }
        set
        {
            m_passiveMask = value;
            SetParam(StateMachineParam.passiveCatchState, m_passiveMask.BitMask(PASSIVE_CATCH));
        }
    }

    public Creature passiveSource
    {
        get { return m_passiveSource; }
        set
        {
            if (m_passiveSource != value)
            {
                //更换触发被动的目标之前，需要把目标的状态清掉，否则断开被动连接后，目标的状态会一直存在
                currentState.ClearSourcePassive(m_passiveSource);
            }
            m_passiveSource = value;
        }
    }

    public int keyIndex => m_keyIndex;
    public bool keyIsFirstCheck => m_keyIsFirstCheck;

    private int m_keyIndex = 0;
    private bool m_keyIsFirstCheck = true;

    public OnEnterState onEnterState;
    public OnQuitState  onQuitState;
    public OnStateEnded onStateEnded;
    public OnRebuild    onRebuild;

    private StateMachineState m_laydown;
    private Creature m_passiveSource;

    [SerializeField]
    private StateMachineParam[] m_parameters = new StateMachineParam[MAX_PARAMS_COUNT];

    private int m_cachedParamIndex;
    private Dictionary<string, int> m_cachedParams = new Dictionary<string, int>();

    private Dictionary<int, int> m_passiveHash = new Dictionary<int, int>();

    private Dictionary<int, int> m_globalKeys = new Dictionary<int, int>();

    private Dictionary<int, List<StateMachineState>> m_coolGroups = new Dictionary<int, List<StateMachineState>>();

    protected StateMachine()
    {
        for (var i = 0; i < MAX_PARAMS_COUNT; ++i)
            m_parameters[i] = new StateMachineParam();
    }

    protected override void OnInitialize()
    {
        speed = 1.0;
        m_forceTransitionUpdate = false;
        m_preventShake = false;
        m_pending = false;
        m_playable = true;

        m_freezCount = 0;
        m_pendingTime = 0;
        m_nextPendingTime = 0;
        m_passiveMask = 0;
        m_passiveSource = null;

        freezing = false;

        if (!info) info = StateMachineInfo.empty;

        if (animator)
        {
            if (!animator.isInitialized && animator.gameObject.activeInHierarchy) animator.Update(0);
            animator.enabled = false;
        }
    }

    protected override void OnDestroy()
    {
        foreach (var param in m_parameters) param.Clear();

        states.Clear(true);
        preStateTransitions.Clear(true);
        anyStateTransitions.Clear(true);
        subStateMachines.Clear();
        hitEffects.Clear();
        deadEffects.Clear();

        m_coolGroups.Clear();
        m_passiveHash.Clear();

        m_laydown     = null;

        creature      = null;
        passiveSource = null;
        passiveMask   = 0;

        info          = null;
        onStateEnded  = null;
        onEnterState  = null;
        onQuitState   = null;
        onRebuild     = null;
    }

    #region Parameters

    public int AddParam(string name, StateMachineParamTypes paramType)
    {
        var index = -1;
        if (!m_cachedParams.TryGetValue(name, out index))
        {
            index = m_cachedParamIndex++;
            m_cachedParams.Add(name, index);

            var param = m_parameters[index];
            param.name = name;
            param.Reset(paramType);
        }
        return index;
    }

    public StateMachineParam GetParam(int index)
    {
        return m_parameters[index];
    }

    public void SetParam(int index, int value, bool modify = false)
    {
        var nv = m_parameters[index];
        if (modify) nv.Set(nv.longValue + value);
        else nv.Set(value);
    }

    public void SetParam(int index, long value, bool modify = false)
    {
        var nv = m_parameters[index];
        if (modify) nv.Set(nv.longValue + value);
        else nv.Set(value);
    }

    public void SetParam(int index, double value)
    {
        m_parameters[index].Set(value);
    }

    public void SetParam(int index, bool value)
    {
        m_parameters[index].Set(value);
    }

    public int GetInt(int index)
    {
        return (int)m_parameters[index].longValue;
    }

    public long GetLong(int index)
    {
        return m_parameters[index].longValue;
    }

    public double GetDouble(int index)
    {
        return m_parameters[index].doubleValue;
    }

    public bool GetBool(int index)
    {
        return m_parameters[index].boolValue;
    }

    #endregion

    public void Rebuild()
    {
        m_cachedParamIndex = 0;
        m_cachedParams.Clear();

        AddParam("section",                StateMachineParamTypes.Long);     // Current animation section
        AddParam("process",                StateMachineParamTypes.Long);     // Current process
        AddParam("key",                    StateMachineParamTypes.Long);     // The first valid input in current state
        AddParam("exit",                   StateMachineParamTypes.Bool);     // Current animation can exit ?
        AddParam("moveBreak",              StateMachineParamTypes.Bool);     // Current animation can break by movement ?
        AddParam("moving",                 StateMachineParamTypes.Bool);     // Are we in moving state ?
        AddParam("state",                  StateMachineParamTypes.Long);     // Current state
        AddParam("groupMask",              StateMachineParamTypes.Long);     // Current state group mask
        AddParam("prevGroupMask",          StateMachineParamTypes.Long);     // Prev state group mask
        AddParam("targetGroupMask",        StateMachineParamTypes.Long);     // Target group mask after hit (target passive state)
        AddParam("targetHitGroupMask",     StateMachineParamTypes.Long);     // Target group mask when hit
        AddParam("configID",               StateMachineParamTypes.Long);     // Config ID (If statemachine is player, configID is player class)
        AddParam("targetConfigID",         StateMachineParamTypes.Long);     // Target config ID
        AddParam("forcePassive",           StateMachineParamTypes.Long);     // Force translate passive state
        AddParam("isDead",                 StateMachineParamTypes.Bool);     // Creature dead (health < 1) ?
        AddParam("realDead",               StateMachineParamTypes.Bool);     // Creature real dead (health < 1 and dead animation ended) ?
        AddParam("hit",                    StateMachineParamTypes.Bool);     // Hit target ?
        AddParam("hited",                  StateMachineParamTypes.Long);     // Hited by other target ?
        AddParam("frame",                  StateMachineParamTypes.Long);     // Current frame
        AddParam("rage",                   StateMachineParamTypes.Double);   // Current rage
        AddParam("rageRate",               StateMachineParamTypes.Double);   // Current rage rate
        AddParam("weak",                   StateMachineParamTypes.Bool);     // Creature in weak state ?
        AddParam("tough",                  StateMachineParamTypes.Bool);     // Creature in tough state ?
        AddParam("fatal",                  StateMachineParamTypes.Long);     // Creature in other creature's fatal radius ?
        AddParam("catchState",             StateMachineParamTypes.Bool);     // Creature in catch state ?
        AddParam("passiveCatchState",      StateMachineParamTypes.Bool);     // Creature in passive catch state ?
        AddParam("immuneDeath",            StateMachineParamTypes.Bool);     // Immune death
        AddParam("speedRun",               StateMachineParamTypes.Double);   // Creature run speed
        AddParam("speedAttack",            StateMachineParamTypes.Double);   // Creature attack speed
        AddParam("weaponItemID",           StateMachineParamTypes.Long);     // Current weapon item id
        AddParam("offWeaponItemID",        StateMachineParamTypes.Long);     // Current off-hand weapon item id
        AddParam("hasLaydownTime",         StateMachineParamTypes.Bool);     // Current state has laydown time ?
        AddParam("nextLayDownTime",        StateMachineParamTypes.Long);     // Next laydown state time
        AddParam("prevEthreal",            StateMachineParamTypes.Long);     // Prev state ethreal count
        AddParam("prevPassiveEthreal",     StateMachineParamTypes.Long);     // Prev state passive ethreal count
        AddParam("stateType",              StateMachineParamTypes.Long);     // Current state type   0 = normal  1 = off  2 = passive
        AddParam("loopCount",              StateMachineParamTypes.Long);     // Current loop count, first loop is 0
        AddParam("totalFrame",             StateMachineParamTypes.Long);     // Current frame count
        AddParam("freezing",               StateMachineParamTypes.Bool);     // StateMachine freezing ?
        AddParam("onGround",               StateMachineParamTypes.Bool);     // Creature on ground ?
        AddParam("morph",                  StateMachineParamTypes.Long);     // Creature morph, see CreatureMorphs
        AddParam("energy",                 StateMachineParamTypes.Long);     // Current energy
        AddParam("energyRate",             StateMachineParamTypes.Double);   // Current energy rate
        
        CollectHitAndDeadEffects();
        CreateStates();
        CreateTransitions();

        currentState = states[0];
        currentState.Enter();

        onEnterState?.Invoke(null, currentState);
        onRebuild?.Invoke();
    }

    public void Pending(int duration, bool nextState = false, bool preventShake = false)
    {
        if (nextState)
        {
            m_nextPendingTime = duration;
            m_preventShake = preventShake;
        }
        else
        {
            m_pendingTime = duration;
            m_preventShake = preventShake;
            pending = true;
        }
    }

    public void AddSubStateMachine(StateMachineInfo subInfo)
    {
        var sub = subStateMachines.Find(s => s.hash == subInfo.hash && s.ID == subInfo.ID);
        if (sub) subStateMachines.Remove(sub);

        subStateMachines.Add(info);

        Rebuild();
    }

    public void RemoveSubStateMachine(int hash, int ID)
    {
        var sub = subStateMachines.Find(s => s.hash == hash && s.ID == ID);
        if (sub) subStateMachines.Remove(sub);

        Rebuild();
    }

    public bool AcceptKeyNow(int key)
    {
        var k = key < 0 ? -key : key;
        if (!currentState.acceptInput || k < 1 || k > 0x0D) return false;

        var ng = currentState.info.groupMask;
        foreach (var pair in m_globalKeys)
        {
            if (pair.Key != 0 && !ng.BitMask(pair.Key)) continue;
            if (pair.Value.BitMask(k)) return true;
        }

        return currentState.AcceptKey(k);
    }

    public StateMachineInfo.Effect GetHitEffect(bool off = false, int itemID = -1)
    {
        if (itemID < 0) itemID = GetInt(off ? StateMachineParam.offWeaponItemID : StateMachineParam.weaponItemID);
        var eff = StateMachineInfo.Effect.empty;
        foreach (var e in hitEffects)
        {
            if (e.itemID == itemID) return e;
            if (e.itemID == 0) eff = e;
        }
        return eff;
    }

    public StateMachineInfo.Effect GetDeadEffect(int itemID = -1)
    {
        return deadEffects.Count > 0 ? deadEffects[0] : StateMachineInfo.Effect.empty;
    }

    public StateMachineState GetState(string name)
    {
        foreach (var state in states)
            if (state.name.Equals(name)) return state;
        return null;
    }

    public StateMachineState GetState(int ID)
    {
        return states.Find(s => s.ID == ID);
    }

    public int GetStateMask(string name)
    {
        var state = states.Find(s => s.name == name);
        return state ? state.info.groupMask : 0;
    }

    public int GetStateMask(int ID)
    {
        var state = states.Find(s => s.ID == ID);
        return state ? state.info.groupMask : 0;
    }

    public int GetStateIndex(string name)
    {
        var idx = states.FindIndex(s => s.name == name);
        return idx;
    }

    public List<StateMachineState> GetCoolGroup(int group)
    {
        return m_coolGroups.Get(group);
    }

    public bool TranslateTo(string name, bool checkCooldown = false)
    {
		var s = GetState(name);

        if (s == null) return false;

        return TranslateTo(s, 1, checkCooldown);
    }

    public bool TranslateToID(int id, bool checkCooldown = false)
    {
        var s = GetState(id);

        if (s == null) return false;

        return TranslateTo(s, 2, checkCooldown);
    }

    public bool TranslateTo(int index, bool checkCooldown = false)
    {
        return TranslateTo(states[index], 3, checkCooldown);
    }

    public void Update(int diff)
    {
        if (speed != 1.0) diff = (int)(diff * speed);

        UpdateTransitions();   // First we check transitions, then update current state

        if (m_forceTransitionUpdate) UpdateTransitions();

        if (m_pending && (m_pendingTime -= ObjectManager.globalDeltaTime) <= 0)  // Pending state ignore local timeScale, should update before current logic update
            pending = false;

        currentState.Update(diff);
    }

    /// <summary>
    /// Check transitions when enter frame, because transition conditions are based on last state logic update
    /// </summary>
    public bool UpdateTransitions()
    {
        m_forceTransitionUpdate = false;

        // Bitmask: 6 = non-instant key  5 = negative ? 0 - 4 = value
        var key = GetInt(StateMachineParam.key);
        var ncc = key >> 28 & 0x07;

        key &= 0xFFFFFFF;  // Only 28 bit stored 4 key input, 28-30 bit stored new key index, currently the last 1 bit is unused

        var dr    = (int)TransitionCheckResults.FAILED;
        var r     = dr;

        //bool kka = false;
        //var m_state = GetInt(StateMachineParam.key);
        //var n = "";
        //for (var pp = 0; pp < 4; ++pp)
        //{
        //    var npp = (m_state >> 7 * pp) & 0x7F;
        //    var kk = npp & 0xF;
        //    var v = npp.BitMask(6);
        //    if (npp.BitMask(5)) kk = -kk;

        //    n += kk + ", ";

        //    if (kk != 0) kka = true;
        //}

        //if (kka && creature.isPlayer) Logger.LogWarning("before: " + n);

        for (m_keyIndex = 0; m_keyIndex < 4;)
        {
            m_keyIsFirstCheck = m_keyIndex < ncc;

            var c = CheckTransition();
            var lr = c.Low8();
            if (lr == 0)
            {
                if (key.BitMask(6 + m_keyIndex * 7)) SetParam(StateMachineParam.key, key >> 7 * m_keyIndex & 0x7F);
                r = c;
                break;
            }
            else if (lr < dr) r = c;
            
            if ((key >> 7 * ++m_keyIndex) == 0) break;
        }

        //m_state = GetInt(StateMachineParam.key);
        //n = "";
        //for (var pp = 0; pp < 4; ++pp)
        //{
        //    var npp = (m_state >> 7 * pp) & 0x7F;
        //    var kk = npp & 0xF;
        //    var v = npp.BitMask(6);
        //    if (npp.BitMask(5)) kk = -kk;

        //    n += kk + ", ";

        //    if (kk != 0) kka = true;
        //}

        //if (kka && creature.isPlayer) Logger.LogWarning("end: " + n);

        var rr = r.Low8();
        if (rr != 0)
        {
            if (rr < dr) creature.DispatchEvent(CreatureEvents.BATTLE_UI_INFO, Event_.Pop(rr, 0));

            SetParam(StateMachineParam.key, key);
            return CheckLateTransition();
        }
        else if (r.High8() == 1) SetParam(StateMachineParam.key, key);

        return true;
    }

    public void ResetTransitions(bool ai)
    { 
        if (!info) return;

        transtionGroupId = ai ? info.aiTransitionGroup : info.transitionGroup;

        if (transtionGroupId == 0)
        {
            Logger.LogWarning("creature : {0} switch {1} state failed, ai transition group is 0",creature == null ? "null" : creature.name,ai ? "ai":"normal");
            return;
        }

        Logger.LogInfo("creature : {0} switch {1} state success, ai transition group is {2}", creature == null ? "null" : creature.name, ai ? "ai" : "normal", transtionGroupId);

        CreateTransitions();
    }

    private int CheckTransition()
    {
        int dr = (int)TransitionCheckResults.FAILED, r = dr;
        var c = CheckTransitionFrom(preStateTransitions);
        var lr = c.Low8();
        if (lr == 0)
        {
            var attacker = GetLong(StateMachineParam.hited);
            passiveMask = 0;
            passiveSource = attacker != 0 ? ObjectManager.FindObject<Creature>(cc => cc && cc.id == attacker) : null;
            return c;
        }
        else if (lr < dr) r = c;

        var mask = GetLong(StateMachineParam.forcePassive);
        if (mask != 0)
        {
            var idx = -1;
            var passive = (int)(mask & 0xFFFFFFFF);
            if (m_passiveHash.TryGetValue(passive, out idx))
            {
                var attacker  = (int)(mask >> 32 & 0xFFFF);
                passiveMask = (int)(mask >> 48);
                passiveSource = attacker != 0 ? ObjectManager.FindObject<Creature>(cc => cc && cc.id == attacker) : null;
                TranslateTo(states[idx], -4);
                return 0;
            }
            else SetParam(StateMachineParam.forcePassive, 0);
        }

        c = CheckTransition(anyStateTransitions);
        lr = c.Low8();
        if (lr == 0) return c;
        if (lr < dr) r = c;

        c = CheckTransition(currentState.transitions);
        lr = c.Low8();
        if (lr == 0) return c;
        if (lr < dr) r = c;
        
        return r;
    }

    private bool CheckLateTransition()
    {
        if (currentState.waitToLoop)
        {
            TranslateTo(currentState, 0); // To enable logging, change type to -1
            return true;
        }

        if (currentState.waitToLayDown)
        {
            TranslateTo(m_laydown, -2);
            return true;
        }

        if (currentState.waitToDefault)
        {
            TranslateTo(states[0], -3);
            return true;
        }

        return false;
    }

    private bool TranslateTo(StateMachineState state, int type = 0, bool checkCooldown = false, float blendTime = -1)
    {
        if (checkCooldown && state.coolingDown) return false;

        #region Debug log

        #if (DEVELOPMENT_BUILD || UNITY_EDITOR) && DETAIL_BATTLE_LOG
        if (type != 0 && creature.isCombat)
        {
            var msg = Util.Format("[{2}:{3}-{4}], [{5}:{6}] Enter state {0} from {1}, type:{7}", state.name, currentState.name, Level.levelTime, creature.frameTime, creature.frameCount, creature.id, creature.name, type);
            if (type > 0) Logger.LogChat(msg);
            else Logger.LogWarning(msg);
        }
        #endif

        #if AI_LOG
        if (type != 0 && creature.isCombat)
        {
            Module_AI.LogBattleMsg(creature,true, "[Enter state {0} from {1}, type:{2}", state.name, currentState.name, type);
        }
        #endif
        #endregion

        FightRecordManager.RecordLog<LogStateTranslate>(l =>
        {
            l.tag = (byte) TagType.StateTranslate;
            l.form = currentState.name;
            l.roleId = creature.Identify;
            l.to = state.name;
            l.currentFrame = (ushort)currentState.currentFrame;
            l.frameCount = (ushort) currentState.frameCount;
        }, true);

        var old = currentState;
        old.Quit();

        onQuitState?.Invoke(old, state, blendTime);

        currentState = state;
        currentState.Enter();

        onEnterState?.Invoke(old, currentState, blendTime);

        if (m_nextPendingTime > 0)
        {
            Pending(m_nextPendingTime, false, m_preventShake);
            m_nextPendingTime = 0;
        }
        else pending = false;

        return true;
    }

    private void CreateStates()
    {
        states.Clear();
        m_passiveHash.Clear();
        m_coolGroups.Clear();

        var ss = info.states;
        foreach (var s in ss) CreateState(s, info);

        StateMachineInfoPassive psm = null;
        foreach (var sub in subStateMachines)
        {
            if (psm == null && sub is StateMachineInfoPassive) psm = sub as StateMachineInfoPassive;

            foreach (var s in sub.states) CreateState(s, sub);
        }

        m_laydown = GetState(StateMachineState.STATE_LAYDOWN);

        if (!m_laydown)
        {
            var p = psm ?? info;
            CreateState(StateMachineInfo.GetDefaultLaydownState(p.GetType().GetHashCode(), p.ID), p);
        }

        if (states.Count < 1) states.Add(StateMachineState.Create(this, info, StateMachineInfo.emptyState, SkilLEntry.empty));
    }

    private bool CreateState(StateMachineInfo.StateDetail s, StateMachineInfo parent)
    {
        if (!s.valid) return false;

        StateMachineState state = null;

        var skillID = SkillToStateInfo.GetSkillID(creature.weaponID, s.ID);
        if (skillID < 0)
        {
            var list = AwakeInfo.GetSkillIDList(s.hash);
            if (list != null && list.Count > 0)
            {
                var ss = new List<StateMachineInfo.StateDetail>();
                foreach (var id in list)
                {
                    var lv = creature.GetSkillLevel(id);
                    if (lv > 0)
                    {
                        var ainfo = ConfigManager.Get<AwakeInfo>(id);
                        var stateLevel = ainfo?.states.Find(Item => Item.state == s.state);
                        if (stateLevel != null) lv = stateLevel.level;
                    }

                    if (lv >= 0)
                    {
                        var os = StateOverrideInfo.GetOriginOverrideState(info, s, lv);
                        if (os != null) ss.Add(os);
                    }
                }
                s = StateOverrideInfo.StateOverride.Combin(s, ss);
            }

            if (!s.valid) return false;

            state = StateMachineState.Create(this, parent, s, SkilLEntry.empty);
        }
        else
        {
            var skill = creature.GetSkillEntry(skillID);
            var level = skill.valid ? skill.level : 0;
            var ls    = level <= 0 ? s : StateOverrideInfo.GetOverrideState(info, s, level);

            if (!ls.valid) return false;

            state = StateMachineState.Create(this, parent, ls, skill);
        }

        states.Add(state);

        if (state.passive) m_passiveHash.Set(s.ID, states.Count - 1);
        if (s.coolGroup != 0) m_coolGroups.GetDefault(s.coolGroup).Add(state);

        return true;
    }

    private void CreateTransitions()
    {
        anyStateTransitions.Clear();
        preStateTransitions.Clear();
        m_globalKeys.Clear();

        var groups = new List<TransitionInfo>();
        var g = ConfigManager.Get<TransitionInfo>(transtionGroupId);
        if (g) groups.Add(g);

        foreach (var sub in subStateMachines)
        {
            g = ConfigManager.Get<TransitionInfo>(sub.transitionGroup);
            if (g) groups.Add(g);
        }

        foreach (var group in groups)
        {
            var pre = group.GetTransitions(-1);
            foreach (var tran in pre)
            {
                var t = StateMachineTransition.Create(this, tran);
                if (!t) continue;

                preStateTransitions.Add(t);

                if (t.info.acceptKeyMask == 0) continue;

                if (t.info.acceptGroupMask == 0)
                {
                    var nk = m_globalKeys.Get(0);
                    m_globalKeys.Set(0, nk | t.info.acceptKeyMask);
                }
                else
                {
                    var gm = t.info.acceptGroupMask;
                    for (var i = 1; i < 64; ++i)
                    {
                        if (!gm.BitMask(i)) continue;
                        var nk = m_globalKeys.Get(i);
                        m_globalKeys.Set(i, nk | t.info.acceptKeyMask);
                    }
                }
            }

            var any = group.GetTransitions(0);
            foreach (var tran in any)
            {
                var t = StateMachineTransition.Create(this, tran);
                if (!t) continue;

                anyStateTransitions.Add(t);

                if (t.info.acceptKeyMask == 0) continue;

                if (t.info.acceptGroupMask == 0)
                {
                    var nk = m_globalKeys.Get(0);
                    m_globalKeys.Set(0, nk | t.info.acceptKeyMask);

                    nk = m_globalKeys.Get(0);
                }
                else
                {
                    var gm = t.info.acceptGroupMask;
                    for (var i = 1; i < 64; ++i)
                    {
                        if (!gm.BitMask(i)) continue;
                        var nk = m_globalKeys.Get(i);
                        m_globalKeys.Set(i, nk | t.info.acceptKeyMask);
                    }
                }
            }
        }

        foreach (var state in states)
            state.UpdateTransitions(groups);
    }

    private void CollectHitAndDeadEffects()
    {
        hitEffects.Clear();
        deadEffects.Clear();

        hitEffects.AddRange(info.hitEffects);

        foreach (var sub in subStateMachines)
        {
            if (sub is StateMachineInfoPassive)
            {
                deadEffects.Add(sub.deadEffect);
                continue;
            }

            hitEffects.AddRange(sub.hitEffects);
        }
    }

    private int CheckTransitionFrom(List<StateMachineTransition> transitions)
    {
        int dr = (int)TransitionCheckResults.FAILED, r = dr;
        foreach (var trans in transitions)
        {
            if (!trans.fromAny && trans.info.from != currentState.name) continue;

            var c = CheckTransition(trans);
            var lr = c.Low8();
            if (lr == 0) return c;
            if (lr < dr) r = c;
        }

        return r;
    }

    private int CheckTransition(List<StateMachineTransition> transitions)
    {
        int dr = (int)TransitionCheckResults.FAILED, r = dr;
        foreach (var trans in transitions)
        {
            var c = CheckTransition(trans);
            var lr = c.Low8();
            if (lr == 0) return c;
            if (lr < dr) r = c;
        }

        return r;
    }

    private int CheckTransition(StateMachineTransition trans)
    {
        var r = trans.Check(currentState);
        if (r.Low8() != 0) return r;

        #region Debug log
        #if (DEVELOPMENT_BUILD || UNITY_EDITOR) && DETAIL_BATTLE_LOG
        if (creature.isCombat)
        {
            var ps = "";
            foreach (var pair in m_cachedParams)
                ps += pair.Key + ": " + GetDouble(pair.Value) + "\n";

            Logger.LogWarning("[{3}:{4}-{5}], [{6}:{7}] Enter state {0} from {1}, type:{8}, trans:\n{2}", trans.target.name, currentState.name, trans.info.ToXml(), Level.levelTime, creature.frameTime, creature.frameCount, creature.id, creature.name, 0);
            Logger.LogWarning("Params: {0}", ps);
        }
        #endif

        #if AI_LOG
        //if (creature.isCombat)
        //{
        //    var dps = "";
        //    foreach (var pair in m_cachedParams)
        //        dps += pair.Key + ": " + GetDouble(pair.Value) + "\n";

        //    Module_AI.LogBattleMsg(creature,true, "[Enter state {0} from {1}, type:{3}, trans:\n{2}", trans.target.name, currentState.name, trans.info.ToXml(), 0);
        //    Module_AI.LogBattleMsg(creature, "[Params: {0}]", dps);
        //}
        #endif
        #endregion

        if (trans.rageCost > 0) creature.rage -= trans.rageCost;
        if (trans.rageRateCost > 0) creature.rage -= creature.maxRage * trans.rageRateCost;
        if (trans.energyCost > 0) creature.energy -= trans.energyCost;
        if (trans.energyRateCost > 0) creature.energy -= (int)(creature.maxEnergy * trans.energyRateCost);

        if (trans.forceTurn)
        {
            trans.target.preventNextTurn = true;
            creature.TurnBack();
        }

        TranslateTo(trans.target, 0, false, trans.blendTime);
        trans.target.preventNextTurn = false;

        if (trans.info.preventInputReset) m_forceTransitionUpdate = true;

        return r;
    }
}
