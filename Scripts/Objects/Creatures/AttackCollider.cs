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
using UnityEngine;

public enum AttackColliderTypes
{
    Attack = 0,
    Effect = 1,
    Bullet = 2
}

public abstract class AttackCollider : BoxSphereCollider
{
    public Creature creature;

    public StateMachine stateMachine => creature ? creature.stateMachine : null;
    public StateMachineState currentState => creature ? creature.currentState : null;

    public AttackColliderTypes type => m_type;
    protected AttackColliderTypes m_type = AttackColliderTypes.Attack;

    /// <summary>
    /// Source direction when collider created  -1 = left  1 = right  0 = failed
    /// </summary>
    public abstract int startDirection { get; }
    /// <summary>
    /// Collider owner current direction  -1 = left  1 = right  0 = failed
    /// </summary>
    public abstract int direction { get; }

    public int attackInfo => m_attackInfo;
    public int bulletAttackInfo => m_bulletAttackInfo;
    public int targetCount => m_targetCount;

    private int m_attackInfo, m_bulletAttackInfo, m_targetCount;

    #region Snapshot

    public const int MAX_SNAPSHOT_COUNT = 10;

    #region Internal struct

    protected struct SnapShot
    {
        public StateMachineInfo.AttackBox attackBox; public StateMachineInfo.Effect hit;
        public bool isForward;

        public void Set(StateMachineInfo.AttackBox _attackBox, StateMachineInfo.Effect _hit, bool _isForward)
        {
            attackBox = _attackBox; hit = _hit;
            isForward = _isForward;
        }
    }

    #endregion

    #region data

    protected SnapShot[] m_snapShots = new SnapShot[MAX_SNAPSHOT_COUNT];

    public StateMachineInfo.Effect hitEff { get { return m_hitEff; } }

    public bool isForward { get { return m_forward; } }
    private bool m_forward;

    [SerializeField]
    protected StateMachineInfo.AttackBox m_attackBox;
    [SerializeField]
    protected StateMachineInfo.Effect m_hitEff;

    #endregion

    [SerializeField]
    protected int m_snapShotCount = 0;
    [SerializeField]
    protected int m_currentSnapshot = -1;

    public override bool UpdateSnapshot(int idx)
    {
        if (idx == 0 && m_snapShotCount == 0) return true;  // Atleast we need to execute collision check onece
        if (idx >= m_snapShotCount) return false;

        RestoreSnapshot(idx);

        return true;
    }

    protected void CreateSnapShot()
    {
        if (!m_enableSnapshot) return;
        if (m_snapShotCount >= MAX_SNAPSHOT_COUNT)
        {
            for (int i = 0, c = m_snapShots.Length - 1; i < c; ++i)
                m_snapShots[i] = m_snapShots[i + 1];
            --m_snapShotCount;
        }
        m_snapShots[m_snapShotCount++].Set(m_attackBox, m_hitEff, m_forward);
    }

    protected void RestoreSnapshot(int idx)
    {
        var snap = m_snapShots[idx];
        m_forward = snap.isForward;
        m_targetCount = 0;

        m_currentSnapshot = idx;

        _SetAttackBox(snap.attackBox, snap.hit, m_currentSnapshot);
    }

    public override void OnQuitFrame()
    {
        m_snapShotCount = 0;
    }

    #endregion

    protected Dictionary<int, int> m_cols = null;

    public int AttackedCount(Collider_ other)
    {
        return m_cols.Get(other.GetInstanceID());
    }

    public void Clear()
    {
        _Clear();
        CreateSnapShot();

        #if (DEVELOPMENT_BUILD || UNITY_EDITOR) && DETAIL_BATTLE_LOG
        if (creature && creature.isPlayer) Logger.LogWarning("[{1}:{2}-{3}], Creature {0} attack box set to empty", creature.name, Level.levelTime, creature.frameTime, creature.frameCount);
        #endif
    }

    public void SetAttackBox(StateMachineInfo.AttackBox attackBox, StateMachineInfo.Effect hit, bool forward)
    {
        Clear();

        m_forward = forward;

        attackBox.start *= 0.1;
        attackBox.size  *= 0.1;

        _SetAttackBox(attackBox, hit);

        CreateSnapShot();
    }

    private void _Clear()
    {
        m_targetCount = 0;

        m_attackBox = StateMachineInfo.AttackBox.empty;
        m_hitEff = StateMachineInfo.Effect.empty;

        m_bulletAttackInfo = 0;
        m_attackInfo = 0;
        m_forward = true;

        gameObject.SetActive(false);

        m_cols?.Clear();
    }

    private void _SetAttackBox(StateMachineInfo.AttackBox attackBox, StateMachineInfo.Effect hit, int snapShotIndex = -1)
    {
        m_attackBox = attackBox;
        m_hitEff    = hit;

        m_hitEff.markedHit = true;

        m_bulletAttackInfo = m_attackBox.bulletAttackInfo;
        m_attackInfo       = m_attackBox.attackInfo;

        gameObject.SetActive(!m_attackBox.isEmpty);

        if (m_attackBox.isEmpty) return;

        if (m_attackBox.type == StateMachineInfo.AttackBox.AttackBoxType.Box)
        {
            var off = m_attackBox.start + m_attackBox.size * 0.5;
            if (!m_forward) off.x = -off.x;

            offset     = off;
            boxSize    = m_attackBox.size;
            radius     = 0;
        }
        else
        {
            var off = m_attackBox.start;
            if (!m_forward) off.x = -off.x;

            boxSize      = Vector2_.zero;
            sphereOffset = off;
            radius       = m_attackBox.size.x;
        }

        #region Debug log
        #if (DEVELOPMENT_BUILD || UNITY_EDITOR) && DETAIL_BATTLE_LOG
        if (creature) Logger.LogWarning("[{4}:{5}-{6}], Creature {0} attack box set to [{1},{2},{3},{7}], snap [{8},{9}]", creature.name, m_attackBox.attackInfo, m_attackBox.type, m_forward, Level.levelTime, creature.frameTime, creature.frameCount, snapShotIndex, enableSnapshot, m_snapShotCount);
        #endif

        #if AI_LOG
        Module_AI.LogBattleMsg(creature, "[attack box set to [{0},{1},{2},{3}], snap [{4},{5}]", m_attackBox.attackInfo, m_attackBox.type, m_forward, snapShotIndex, enableSnapshot, m_snapShotCount);
        #endif
        #endregion
    }

    public virtual void OnRealHitTarget(CreatureHitCollider target, AttackInfo a)
    {
        ++m_targetCount;

        if (m_targetCount == 1)
            creature.DispatchEvent(CreatureEvents.FIRST_HIT, Event_.Pop(target.creature));

        if (!a || a.targetLimit < 1 || m_targetCount < a.targetLimit) return;

        _Clear();
        if (m_currentSnapshot > -1) m_snapShots[m_currentSnapshot].Set(m_attackBox, m_hitEff, m_forward);
    }

    protected override void Awake()
    {
        base.Awake();

        m_cols = new Dictionary<int, int>();

        _Clear();
    }

    protected override void OnCollisionBegin(Collider_ other)
    {
        var c = 0;
        var k = other.GetInstanceID();
        if (m_cols.TryGetValue(k, out c)) m_cols[k] = c + 1;
        else m_cols.Add(k, 1);

        #region Debug log
        #if (DEVELOPMENT_BUILD || UNITY_EDITOR) && DETAIL_BATTLE_LOG
        if (creature) Logger.LogWarning("[{2}:{3}-{4}], Creature {0} attack box collision begin with [{1}]", creature.name, m_cols[k], Level.levelTime, creature.frameTime, creature.frameCount);
        #endif

        #if AI_LOG
        Module_AI.LogBattleMsg(creature, "[attack box collision begin with {0}]", m_cols[k]);
        #endif
        #endregion
    }
}
