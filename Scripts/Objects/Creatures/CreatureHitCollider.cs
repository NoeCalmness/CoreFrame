/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Description
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.0
 * Created:  2017-
 * 
 ***************************************************************************************************/

using System.Collections.Generic;

public class CreatureHitCollider : Box2DCollider
{
    #region Static functions

    public static int CalculateHitDirection(AttackCollider c, AttackInfo a, double hitX)
    {
        var reference = a.directionReference[0];
        var type      = a.directionReference[1];
        var direction = 0;
        var refX      = 0.0;

        switch (reference)
        {
            case 0:   // self  (creature or effect)
            {
                direction = c.direction;
                refX      = c.center.x;
                break;
            }
            case 1:   // created (source when attack collider created)
            {
                direction = c.startDirection;
                refX      = c.center.x;
                break;
            }
            case 2:   // source (source when hited)
            {
                direction = c.creature ? c.creature.direction.ToInt() : 0;
                refX = c.creature ? c.creature.x : c.center.x;
                break;
            }
            default:  return 0;  // none
        }

        if (direction == 0 || type == 1) return direction;
        if (type == 0) return direction * -1;
        if (type == 2) return refX < hitX ? -1 : refX > hitX ? 1 : 0;
        if (type == 3) return refX < hitX ? 1 : refX > hitX ? -1 : 0;

        return direction * -1;
    }

    #endregion

    public Creature creature
    {
        get { return m_creature; }
        set
        {
            m_creature = value;
            m_stateMachine = m_creature ? m_creature.stateMachine : null;
            m_behaviour = m_creature ? m_creature.behaviour : null;
        }
    }
    public CreatureBehaviour behaviour { get { return m_behaviour; } }

    private Creature m_creature;
    private StateMachine m_stateMachine;
    private CreatureBehaviour m_behaviour;

    private Dictionary<uint, int> m_attinfos = new Dictionary<uint, int>();

    private int m_deadCombo = 0;

    protected override void OnDestroy()
    {
        base.OnDestroy();

        m_creature = null;
        m_stateMachine = null;
    }

    public void Clear()
    {
        m_attinfos.Clear();

        m_creature.DispatchEvent(CreatureEvents.HIT_INFO_CLEARED);

        #region Debug log
        #if (DEVELOPMENT_BUILD || UNITY_EDITOR) && DETAIL_BATTLE_LOG
        Logger.LogInfo("[{1}:{2}-{3}], [{0}] hit info cleared.", m_creature.name, Level.levelTime, creature.frameTime, creature.frameCount);
        #endif

        #if AI_LOG
        Module_AI.LogBattleMsg(m_creature, "[hit info cleared.]");
        #endif
        #endregion
    }

    public void UpdateDeadCombo(bool kill = false)
    {
        if (kill) m_deadCombo = 0;
        else ++m_deadCombo;

        enabled = m_deadCombo < CombatConfig.sdeadComboLimit;
    }

    public void OnHit(AttackCollider c, AttackInfo a, int t)
    {
        if (null == m_creature) return;
        var alive = !m_creature.isDead;

        var ts = t == byte.MaxValue ? AttackInfo.hitProtectTrans : a.targetStates[t];
        var cc = c.creature;
        var ss = c.currentState;
        var passive = ss.info.ignoreTough || (!m_creature.tough || a.breakToughLevel > m_creature.toughLevel) || m_creature.weak && a.fromBullet;  // can we enter passive state ?
        var toState = -1;
        if (passive)
        {
            var pt = 0L;
            var breakTough = m_creature.tough && (ss.info.ignoreTough || a.breakToughLevel > m_creature.toughLevel || cc.breakToughLevel > m_creature.toughLevel);
            var isCatch = ss.info.catchState > 0;
            if (isCatch) pt = pt.BitMask(StateMachine.PASSIVE_CATCH, true);
            if (breakTough) pt = pt.BitMask(StateMachine.PASSIVE_TOUGH, true);
            if (a.fromBullet && (m_creature.weak || c.creature.HawkEye)) pt = pt.BitMask(StateMachine.PASSIVE_WEAK, true);

            var opt = m_stateMachine.GetLong(StateMachineParam.forcePassive) >> 48 & 0xFFFF;
            if (opt == 0 || pt.BitMask(StateMachine.PASSIVE_CATCH) || !opt.BitMask(StateMachine.PASSIVE_CATCH) && pt.BitMask(StateMachine.PASSIVE_WEAK))
            {
                var tid = breakTough && m_creature.HasState(ts.toToughStateID) ? ts.toToughStateID : ts.toStateID;
                toState = tid;

                var p = pt << 48 | (long)cc.id << 32 | (uint)tid;
                m_stateMachine.SetParam(StateMachineParam.forcePassive, p);

                if (pt.BitMask(StateMachine.PASSIVE_CATCH))
                    m_creature.RemoveBuff(BuffInfo.EffectTypes.Freez);

                if (c.type == AttackColliderTypes.Attack)
                {
                    var mask = m_stateMachine.GetStateMask(toState);
                    cc.stateMachine.SetParam(StateMachineParam.targetGroupMask, mask);
                    cc.stateMachine.SetParam(StateMachineParam.targetHitGroupMask, m_stateMachine.GetLong(StateMachineParam.groupMask));
                    cc.stateMachine.SetParam(StateMachineParam.targetConfigID, m_creature.configID);
                    if (isCatch && !mask.BitMasks(ss.info.ignoreCatchPosGroups) && m_creature.HasState(toState))
                    {
                        if (ss.info.catchState == 1) m_creature.position_ = cc.position_;
                        else cc.position_ = m_creature.position_;
                    }
                }
            }
            else passive = false;
        }

        if (c.type == AttackColliderTypes.Attack) cc.stateMachine.SetParam(StateMachineParam.hit, true);

        m_stateMachine.SetParam(StateMachineParam.hited, cc.id);

        cc.DispatchAttackEvent(m_creature, a);
        if (!m_creature.isDead) cc.rage += a.selfRageReward * (1 + cc.attackRageMul);  // ignore rage reward if target is dead

        m_creature.DispatchEvent(CreatureEvents.ATTACKED, Event_.Pop(cc, a));
        if (a.fromBullet) m_creature.DispatchEvent(CreatureEvents.SHOOTED, Event_.Pop(m_creature, a));
        if (!m_creature.isDead) // ignore rage reward if target is dead
        {
            m_creature.rage += a.targetRageReward * (1 + m_creature.attackedRageMul);
            m_creature.TakeDamage(cc, cc.CalculateDamage(a, m_creature, ss.damageMul));
        }
        else if (!alive) UpdateDeadCombo();

        if (a.freezTime > 0)
        {
            m_stateMachine.Pending(a.freezTime, passive, alive && m_creature.isDead);
            if (!a.fromEffect) c.stateMachine.Pending(a.freezTime);
        }

        if (passive)
        {
            if (a.preventTurn == 0)
            {
                if (a.fromBullet) m_creature.FaceTo(cc);
                else
                {
                    var direction = CalculateHitDirection(c, a, m_creature.x);
                    if (direction != 0) m_creature.direction = direction < 0 ? CreatureDirection.BACK : CreatureDirection.FORWARD;
                }
            }
            if (a.acttackSlide != 0) m_creature.Knockback(m_creature.isForward ? -a.acttackSlide * 0.1 : a.acttackSlide * 0.1);
        }

        var s = a.soundEffect.SelectSound(m_creature.roleProto, m_creature.gender);
        if (!s.isEmpty) AudioManager.PlayAudio(s.sound, a.soundEffect.isVoice || s.isVoice ? AudioTypes.Voice : AudioTypes.Sound, false, a.soundEffect.overrideType);

        if (c.type == AttackColliderTypes.Attack && !a.ignoreHitEffect)
            behaviour.effects.PlayEffect(c.hitEff);

        if (!m_creature.isDead)  // do not trigger buffs if target is dead
        {
            var level = 1;
            if (cc is PetCreature)
                level = ((PetCreature)cc).petInfo.AdditiveLevel;

            var b = a.selfBuff;
            if (b.intValue0 != 0 && (b.doubleValue0 == 0 || Module_Battle.Range() <= b.doubleValue0))
                Buff.Create(b.intValue0, cc, null, b.intValue1, -1, level);

            b = a.targetBuff;
            if (b.intValue0 != 0 && (b.doubleValue0 == 0 || Module_Battle.Range() <= b.doubleValue0))
                Buff.Create(b.intValue0, m_creature, cc, b.intValue1, -1, level);
        }

        #region Debug log
        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        Logger.LogInfo("[{3}:{4}-{5}], [{0}] hited by attackInfo [{1}], translate to state [{2}]", m_creature.gameObject.name, a.ID, toState == ts.toStateID ? ts.toState : toState == ts.toToughStateID ? ts.toToughState : string.Empty, Level.levelTime, creature.frameTime, creature.frameCount);
        #endif

        #if AI_LOG
        Module_AI.LogBattleMsg(m_creature, "[m_creature.tough = {0},currentState is {1} ss.info.ignoreTough = {2} attackinfo.breaktoughLevel = {3} m_creature.toughLevel = {4} cc.breakToughLevel = {5}", m_creature.tough, m_creature.stateMachine.currentState.name, ss.info.ignoreTough, a.breakToughLevel, m_creature.toughLevel, cc.breakToughLevel);
        Module_AI.LogBattleMsg(m_creature, "[[{0}] hited by attackInfo [{1}], translate to state [{2}]", m_creature.gameObject.name, a.ID, toState == ts.toStateID ? ts.toState : toState == ts.toToughStateID ? ts.toToughState : string.Empty);
        #endif
        #endregion
    }

    protected override void OnCollisionBegin(Collider_ other)
    {
        if (!enabled || m_creature == null) return;

        var c = other as AttackCollider;
        var cc = c ? c.creature : null;
        if (!cc || cc.clearAttack || m_creature.SameCampWith(cc)  || c.AttackedCount(this) > 1) return;

        if (!m_stateMachine) m_stateMachine = m_creature.stateMachine;

        var bullet = c.type == AttackColliderTypes.Bullet;
        var effect = c.type == AttackColliderTypes.Effect || c.type == AttackColliderTypes.Bullet;
        var ai = bullet && (m_stateMachine.GetBool(StateMachineParam.weak) || cc.HawkEye) ? c.bulletAttackInfo : c.attackInfo;
        var a = AttackInfo.Get(ai);
        if (!a || a.targetStates.Length < 1)
        {
            if (ai == 0) c.OnRealHitTarget(this, a);
            return;
        }

        if (!a.ignoreInvincible && m_creature.invincible) return;   // Invincible check
        if (a.lockTarget && (m_stateMachine.passiveSource != cc || !m_stateMachine.GetBool(StateMachineParam.passiveCatchState))) return;   // Lock target check

        a.fromBullet = bullet;
        a.fromEffect = effect;

        var mask = m_stateMachine.GetInt(StateMachineParam.groupMask);
        var tss = a.targetStates;
        var isCatch = cc.currentState.info.catchState != 0;
        for (int i = 0, count = tss.Length; i < count; ++i)
        {
            var ts = tss[i];
            if (mask.BitMask(ts.fromGroup))
            {
                var iid = (uint)c.creature.id << 16 | (uint)(a.ID & 0xFFFF);
                var ac = m_attinfos.Get(iid);
                if (!m_creature.tough || isCatch)
                {
                    ac += 1;
                    m_attinfos.Set(iid, ac);
                }

                var ti = (byte)i;

                if (ac > a.comboLimit)
                {
                    if (isCatch)
                    {
                        cc.DispatchEvent(CreatureEvents.ATTACK_HIT_FAILED, Event_.Pop(m_creature));
                        break;
                    }
                    ti = byte.MaxValue;
                }

                OnHit(c, a, ti);
                c.OnRealHitTarget(this, a);

                break;
            }
        }
    }
}
