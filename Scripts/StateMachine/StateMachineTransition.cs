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

using UnityEngine;

public enum ConditionMode
{
    None       = 0,
    If         = 1,
    IfNot      = 2,
    Greater    = 3,
    Less       = 4,
    Equals     = 6,
    NotEqual   = 7,
    Contains   = 8,
    Except     = 9,
    Between    = 10,
    NotBetween = 11,
    GEquals    = 12,
    LEquals    = 13,
}

public enum TransitionCheckResults
{
    /// <summary>
    /// Lucky!
    /// </summary>
    Passed             = 0,
    NOT_ENOUGH_RAGE    = 1,
    NOT_ENOUGH_ENERGY  = 2,
    SPELL_COOLING_DOWN = 3,
    /// <summary>
    /// Other failed reason
    /// </summary>
    FAILED             = 255
}

public class StateMachineTransition : PoolObject<StateMachineTransition>
{
    #region Static functions

    public const int MAX_CONDITION_COUNT = 15;

    private const int PASSED = 0, NOT_ENOUGH_RAGE = 1, NOT_ENOUGH_ENERGY = 2, SPELL_COOLING_DOWN = 3, FAILED = 255;

    public static StateMachineTransition Create(StateMachine stateMachine, TransitionInfo.Transition info)
    {
        var target = stateMachine.GetState(info.to);

        if (!target)
        {
            Logger.LogWarning("StateMachineTransition::Create: Create transition [Weapon:{0}:{1}] failed, target [{2}] state is null.", stateMachine.info.ID, info.ID, info.to);
            return null;
        }

        var trans = Create(false);
        trans.stateMachine = stateMachine;
        trans.info = info;
        trans.target = target;

        trans.OnInitialize();

        return trans;
    }

    #endregion

    public TransitionInfo.Transition info;

    [HideInInspector]
    public StateMachine stateMachine;

    public StateMachineState target { get; private set; }
    public bool fromAny { get; private set; }
    /// <summary>
    /// Transition has "key" condition ?
    /// </summary>
    public bool hasKeyCondition { get { return m_hasKeyCondition; } }
    /// <summary>
    /// This transition accept inverted key input ?
    /// </summary>
    public bool acceptInverseKey { get { return m_acceptInverseKey; } }
    public bool forceTurn { get; private set; }
    /// <summary>
    /// Override blend time
    /// negative means use default
    /// </summary>
    public float blendTime { get; private set; }

    public double rageCost = 0;
    public double rageRateCost = 0;
    public int    energyCost = 0;
    public double energyRateCost = 0;

    private bool m_hasKeyCondition = false;
    private bool m_acceptInverseKey = false;
    private bool m_dismissKeyCondition = false;

    private int[] m_paramIndex = new int[MAX_CONDITION_COUNT];

    protected StateMachineTransition() { }

    protected override void OnInitialize()
    {
        rageCost = 0;
        rageRateCost = 0;
        energyCost = 0;
        energyRateCost = 0;

        m_hasKeyCondition = false;
        m_acceptInverseKey = false;
        m_dismissKeyCondition = false;

        fromAny = string.IsNullOrEmpty(info.from) || info.from == "any";

        var cs = info.conditions;
        for (var i = 0; i < cs.Length; ++i)
        {
            var c = cs[i];
            if (c.mode == ConditionMode.None) continue;

            m_paramIndex[i] = stateMachine.AddParam(c.param, c.paramType);

            if (c.isRage && c.mode == ConditionMode.Greater) rageCost = c.doubleValue;
            if (c.isRageRate && (c.mode == ConditionMode.Greater || c.mode == ConditionMode.GEquals)) rageRateCost = c.doubleValue;
            if (c.isEnergy && (c.mode == ConditionMode.Greater || c.mode == ConditionMode.GEquals)) energyCost = c.mode == ConditionMode.Greater ? c.intValue + 1 : c.intValue;
            if (c.isEnergyRate && (c.mode == ConditionMode.Greater || c.mode == ConditionMode.GEquals)) energyRateCost = c.doubleValue;
            if (c.isKey) m_hasKeyCondition = true;
        }

        if (m_hasKeyCondition && !info.preventInputTurn) m_acceptInverseKey = true;

        m_dismissKeyCondition = m_hasKeyCondition && !info.forceKeyCondition;

        blendTime = info.blendTime < 0 ? 0 : info.blendTime == 0 ? -1 : info.blendTime;
    }

    protected override void OnDestroy()
    {
        target = null;
        stateMachine = null;
        info = null;
    }

    /// <summary>
    /// Check current state transition
    /// </summary>
    /// <param name="now">Current state</param>
    /// <returns>High 8 bit: mask Low 8 bit: TransitionCheckResults</returns>
    public int Check(StateMachineState now)
    {
        if (!m_hasKeyCondition && stateMachine.keyIndex > 0) return FAILED;

        forceTurn = false;

        if (target.level == 0 || // state locked
            info.hasExitTime && !now.ended || // exit time check
            info.noSelfTransition && target == now || // self transition check
            !info.noExitCondition && !stateMachine.GetBool(StateMachineParam.exit) || // exit frame check
            !info.noDeadCondition && stateMachine.GetBool(StateMachineParam.isDead) || // dead check
            !info.noFreezCondition && stateMachine.freezing || // freezing state check
            m_dismissKeyCondition && stateMachine.GetLong(StateMachineParam.process) > 1 // ignore key transition check
        ) return FAILED;

        var cool = target.coolingDown;
        if (!m_hasKeyCondition && cool) return FAILED; // pre cooldown check

        var infoResult = m_hasKeyCondition && stateMachine.keyIsFirstCheck;

        var cs = info.conditions;
        var paramIdx = -1;
        StateMachineParam param = null;
        TransitionInfo.Condition failed = null;

        failed = CheckConditions(cs, 0, info.itemConditionEnd, ref paramIdx, ref param); // Check group and item requirement
        if (failed != null) return FAILED;

        failed = CheckConditions(cs, info.itemConditionEnd, info.keyConditionEnd, ref paramIdx, ref param);   // Check key condition
        if (failed != null) return FAILED;

        if (cool) return infoResult ? SPELL_COOLING_DOWN : FAILED;

        failed = CheckConditions(cs, info.keyConditionEnd, cs.Length, ref paramIdx, ref param);

        if (failed != null) return infoResult ? failed.isRage && rageCost > 0 || failed.isRageRate && rageRateCost > 0 ? NOT_ENOUGH_RAGE : failed.isEnergy && energyCost > 0 || failed.isEnergyRate && energyRateCost > 0 ? NOT_ENOUGH_ENERGY : info.forceUIResult != 0 ? info.forceUIResult : FAILED : FAILED;

        var trace = target.info.trackDistance * 0.1; // dm -> m
        if (trace > 0)
        {
            var c = stateMachine.creature;
            var tar = Util.SelectNearestTarget(c, trace, true);
            if (!tar) return info.forceUIResult != 0 ? info.forceUIResult : FAILED;
            else
            {
                if (!c.TargetInFront(tar)) c.TurnBack();

                c.position_ = tar.position_;
            }
        }

        return info.preventInputReset ? (1 << 8) : PASSED;
    }

    private bool CheckCondition(TransitionInfo.Condition condition, long lv, double dv, bool bv)
    {
        switch (condition.mode)
        {
            case ConditionMode.If:         return bv;
            case ConditionMode.IfNot:      return !bv;
            case ConditionMode.Greater:    return condition.CheckGreater(dv, lv);
            case ConditionMode.Less:       return condition.CheckLess(dv, lv);
            case ConditionMode.Equals:     return condition.CheckEquals(dv, lv);
            case ConditionMode.NotEqual:   return condition.CheckNotEquals(dv, lv);
            case ConditionMode.Contains:   return condition.isKey ? condition.CheckContainsKey(lv) : condition.CheckContains(lv);
            case ConditionMode.Except:     return condition.isKey ? condition.CheckExceptKey(lv) : condition.CheckExcept(lv);
            case ConditionMode.Between:    return condition.CheckBetween(dv, lv);
            case ConditionMode.NotBetween: return condition.CheckNotBetween(dv, lv);
            case ConditionMode.GEquals:    return condition.CheckGEquals(dv, lv);
            case ConditionMode.LEquals:    return condition.CheckLEquals(dv, lv);
            default: return true;   // Invalid condition mode always return true because we ignore it
        }
    }

    private TransitionInfo.Condition CheckConditions(TransitionInfo.Condition[] conditions, int s, int e, ref int paramIdx, ref StateMachineParam param)
    {
        for (int i = s; i < e; ++i) // Check item requirement
        {
            var c = conditions[i];

            var idx = m_paramIndex[i];
            if (idx != paramIdx)
            {
                param    = stateMachine.GetParam(idx);
                paramIdx = idx;
            }

            var check = c.isKey ? CheckKeyCondition(c, (int)param.longValue) : CheckCondition(c, param.longValue, param.doubleValue, param.boolValue); ;
            if (!check) return c;
        }
        return null;
    }

    private bool CheckKeyCondition(TransitionInfo.Condition condition, int key)
    {
        var nk = key >> 7 * stateMachine.keyIndex & 0x7F;

        var k = condition.isMask ? key : nk & 0x1F;
        var passed = CheckCondition(condition, k, k, k != 0);

        if (!passed) return false;

        if (m_acceptInverseKey && nk.BitMask(5) && !target.info.preventTurn)
            forceTurn = true;

        return true;
    }
}
