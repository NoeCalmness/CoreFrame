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
using UnityEngine;

public enum StateMachineParamTypes
{
    Long    = 0,
    Double  = 1,
    Bool    = 2,
}

[Serializable]
public class StateMachineParam
{
    #region Default params

    public const int section                = 0;         // Current animation section
    public const int process                = 1;         // Current process
    public const int key                    = 2;         // The first valid input in current state
    public const int exit                   = 3;         // Current animation can exit ? Same as stage > 0
    public const int moveBreak              = 4;         // Current animation can break by movement ? Same as stage > 1
    public const int moving                 = 5;         // Are we in moving state ?
    public const int state                  = 6;         // Current state
    public const int groupMask              = 7;         // Current state group mask
    public const int prevGroupMask          = 8;         // Prev state group mask
    public const int targetGroupMask        = 9;         // Target group mask after hit (target passive state)
    public const int targetHitGroupMask     = 10;        // Target group mask when hit
    public const int configID               = 11;        // Config ID (If statemachine is player, configID is player class)
    public const int targetConfigID         = 12;        // Target config ID
    public const int forcePassive           = 13;        // Force translate passive state
    public const int isDead                 = 14;        // Creature dead (health < 1) ?
    public const int realDead               = 15;        // Creature real dead (health < 1 and dead animation ended) ?
    public const int hit                    = 16;        // Hit target ?
    public const int hited                  = 17;        // Hited by other target ?
    public const int frame                  = 18;        // Current frame
    public const int rage                   = 19;        // Current rage
    public const int rageRate               = 20;        // Current rage rate
    public const int weak                   = 21;        // Creature in weak state ?
    public const int tough                  = 22;        // Creature in tough state ?
    public const int fatal                  = 23;        // Creature in other creature's fatal radius ?
    public const int catchState             = 24;        // Creature in catch state ?
    public const int passiveCatchState      = 25;        // Creature in passive catch state ?
    public const int immuneDeath            = 26;        // Immune death
    public const int speedRun               = 27;        // Creature run speed
    public const int speedAttack            = 28;        // Creature attack speed
    public const int weaponItemID           = 29;        // Current weapon item id
    public const int offWeaponItemID        = 30;        // Current off-hand weapon item id
    public const int hasLaydownTime         = 31;        // Current state has laydown time ?
    public const int nextLayDownTime        = 32;        // Next laydown state time
    public const int prevEthreal            = 33;        // Prev state ethreal count
    public const int prevPassiveEthreal     = 34;        // Prev state passive ethreal count
    public const int stateType              = 35;        // Current state type   0 = normal  1 = off   2 = passive
    public const int loopCount              = 36;        // Current loop count, first loop is 0
    public const int totalFrame             = 37;        // Current frame count
    public const int freezing               = 38;        // StateMachine freezing ?
    public const int onGround               = 39;        // Creature on ground ?
    public const int morph                  = 40;        // Creature morph, see CreatureMorphs
    public const int energy                 = 41;        // Current energy
    public const int energyRate             = 42;        // Current energy rate

    #endregion

    public StateMachineParamTypes paramType { get { return m_paramType; } set { m_paramType = value; } }

    public double doubleValue { get { return m_doubleValue; } }
    public long longValue { get { return m_longValue; } }
    public bool boolValue { get { return m_boolValue; } }

    [HideInInspector]
    public string name = "";

    [SerializeField]
    private StateMachineParamTypes m_paramType;
    [SerializeField]
    private long m_longValue;
    [SerializeField]
    private double m_doubleValue;
    [SerializeField]
    private bool m_boolValue;

    public StateMachineParam()
    {
        m_paramType   = StateMachineParamTypes.Long;
        m_longValue   = 0;
        m_doubleValue = 0;
        m_boolValue   = false;
    }

    public StateMachineParam(double value) { Set(value); }

    public StateMachineParam(bool value) { Set(value); }

    public StateMachineParam(long value) { Set(value); }

    public void Set(long value)
    {
        m_longValue   = value;
        m_doubleValue = value;
        m_boolValue   = value != 0;
    }

    public void Set(double value)
    {
        m_longValue   = (long)value;
        m_doubleValue = value;
        m_boolValue   = value != 0;
    }

    public void Set(bool value)
    {
        m_longValue   = value ? 1 : 0;
        m_doubleValue = m_longValue;
        m_boolValue   = value;
    }

    public void Reset(StateMachineParamTypes type, long lv = 0, double dv = 0, bool bv = false)
    {
        m_paramType   = type;
        m_longValue   = lv;
        m_doubleValue = dv;
        m_boolValue   = bv;
    }

    public void Clear()
    {
        name          = "";
        paramType     = StateMachineParamTypes.Long;
        m_longValue   = 0;
        m_doubleValue = 0;
        m_boolValue   = false;
    }
}
