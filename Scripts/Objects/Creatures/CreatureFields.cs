/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Creature properties
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-08-15
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;

public partial class Creature
{
    #region Public properties

    /// <summary>
    /// 配置表ID
    /// </summary>
    public int configID
    {
        get { return m_configID; }
        set
        {
            if (m_configID == value) return;
            m_configID = value;

            m_stateMachine?.SetParam(StateMachineParam.configID, m_configID);
        }
    }
    /// <summary>
    /// 角色头像
    /// 显示在 UI 上的头像 ID
    /// </summary>
    public string avatar;
    /// <summary>
    /// 角色头像框 ID
    /// </summary>
    public int avatarBox;
    /// <summary>
    /// 角色ID
    /// </summary>
    public ulong roleId;
    /// <summary>
    /// 角色职业
    /// </summary>
    public int roleProto;
    /// <summary>
    /// 显示在 UI 上的名称
    /// 区别于 name
    /// </summary>
    public string uiName;
    /// <summary>
    /// 性别 0 = 女性  1 = 男性
    /// </summary>
    public int gender;
    /// <summary>
    /// 图标名称 test
    ///</summary>
    public string icon;
    /// <summary>
    /// 角色描述
    ///</summary>
    public string info;
    /// <summary>
    /// 初始经验
    ///</summary>
    public int baseExp;
    /// <summary>
    /// 生物类型
    ///</summary>
    public int type;
    /// <summary>
    /// 当前连击数
    /// </summary>
    public int comboCount;
    /// <summary>
    /// 元素类型 等同于 CreatureElementTypes
    /// </summary>
    public int elementType
    {
        get { return (int)m_fields[(int)CreatureFields.ElementType]; }
        set { _SetField(CreatureFields.ElementType, value); }
    }
    /// <summary>
    /// 元素类型
    /// </summary>
    public CreatureElementTypes eelementType
    {
        get { return (CreatureElementTypes)(int)m_fields[(int)CreatureFields.ElementType]; }
        set { _SetField(CreatureFields.ElementType, (int)value); }
    }
    /// <summary>
    /// 标准移动速度（米/秒）
    /// </summary>
    public double standardRunSpeed;
    /// <summary>
    /// 主武器 ID
    /// </summary>
    public int weaponID
    {
        get { return m_weaponID; }
        set
        {
            if (m_weaponID == value) return;
            UpdateStateMachine(value);
        }
    }
    /// <summary>
    /// 副武器 ID
    /// </summary>
    public int offWeaponID
    {
        get { return m_offWeaponID; }
        set
        {
            if (m_offWeaponID == value) return;
            m_offWeaponID = value;
        }
    }
    /// <summary>
    /// 主武器道具 ID
    /// </summary>
    public int weaponItemID
    {
        get { return m_weaponItemID; }
        set
        {
            if (m_weaponItemID == value) return;
            m_weaponItemID = value;

            m_stateMachine.SetParam(StateMachineParam.weaponItemID, m_weaponItemID);
            behaviour.UpdateWeaponModel(0);
        }
    }
    /// <summary>
    /// 副武器道具 ID
    /// </summary>
    public int offWeaponItemID
    {
        get { return m_offWeaponItemID; }
        set
        {
            if (m_offWeaponItemID == value) return;
            m_offWeaponItemID = value;

            m_stateMachine.SetParam(StateMachineParam.offWeaponItemID, m_offWeaponItemID);
            behaviour.UpdateWeaponModel(1);
        }
    }
    /// <summary>
    /// 当前生命值
    ///</summary>
    public int health
    {
        get { return (int)m_fields[(int)CreatureFields.Health]; }
        set
        {
            value = Mathf.Clamp(value, 0, maxHealth);

            var idx = (int)CreatureFields.Health;
            var oldVal = m_fields[idx];

            if (oldVal == value) return;
            m_fields[idx] = value;

            #region Debug log
            #if (DEVELOPMENT_BUILD || UNITY_EDITOR) && DETAIL_BATTLE_LOG
            Logger.LogWarning("[{1}:{2}-{3}], Creature {0} health changed to {4} from {5}", name, Level.levelTime, frameTime, frameCount, value, oldVal);
            #endif

            #if AI_LOG
            Module_AI.LogBattleMsg(this, "[health changed to {0} from {1}]", value, oldVal);
            #endif
            #endregion

            DispatchEvent(CreatureEvents.FIELD_CHANGED, Event_.Pop(CreatureFields.Health, oldVal, (double)value));
            DispatchEvent(CreatureEvents.HEALTH_CHANGED, Event_.Pop(oldVal, value, false));

            if (value == 0)
            {
                DispatchEvent(CreatureEvents.Dying);

                isDead = health < 1;
                realDead = false;

                if (isDead)
                {
                    m_stateMachine.SetParam(StateMachineParam.isDead, true);
                    m_stateMachine.SetParam(StateMachineParam.key, 0); // Clear key input

                    behaviour.hitCollider.UpdateDeadCombo(true);

                    DispatchEvent(CreatureEvents.DEAD);
                }
            }
            else if (oldVal == 0)
            {
                if (m_stateMachine != null)
                {
                    m_stateMachine.SetParam(StateMachineParam.isDead, false);
                    m_stateMachine.SetParam(StateMachineParam.realDead, false);
                    if (isDead) m_stateMachine.TranslateTo(StateMachineState.STATE_IDLE);
                    behaviour.UpdateAllColliderState(true);
                }
                isDead   = false;
                realDead = false;
            }
        }
    }
    /// <summary>
    /// 最大生命值
    /// </summary>
    public int maxHealth
    {
        get { return (int)m_fields[(int)CreatureFields.MaxHealth]; }
        set
        {
            if (value < 0) value = 0;

            var idx = (int)CreatureFields.MaxHealth;
            var oldVal = m_fields[idx];

            if (oldVal == value) return;
            m_fields[idx] = value;

            if (health > value) health = value;

            DispatchEvent(CreatureEvents.FIELD_CHANGED, Event_.Pop(CreatureFields.MaxHealth, oldVal, (double)value));
        }
    }
    /// <summary>
    /// 当前怒气
    ///</summary>
    public double rage
    {
        get { return m_fields[(int)CreatureFields.Rage]; }
        set
        {
            value = value < 0 ? 0 : value > maxRage ? maxRage : value;

            var idx = (int)CreatureFields.Rage;
            var oldVal = m_fields[idx];

            if (oldVal == value) return;
            m_fields[idx] = value;

            m_stateMachine?.SetParam(StateMachineParam.rage, (float)(value + 0.0001));

            DispatchEvent(CreatureEvents.FIELD_CHANGED, Event_.Pop(CreatureFields.Rage, oldVal, value));
            DispatchEvent(CreatureEvents.RAGE_CHANGED, Event_.Pop(oldVal, value));
        }
    }
    /// <summary>
    /// 最大怒气
    /// </summary>
    public double maxRage
    {
        get { return m_fields[(int)CreatureFields.MaxRage]; }
        set
        {
            if (value < 0) value = 0;

            var idx = (int)CreatureFields.MaxRage;
            var oldVal = m_fields[idx];

            if (oldVal == value) return;
            m_fields[idx] = value;

            if (rage > value) rage = value;

            DispatchEvent(CreatureEvents.FIELD_CHANGED, Event_.Pop(CreatureFields.MaxRage, oldVal, value));
        }
    }
    /// <summary>
    /// 每秒回复生命值
    /// </summary>
    public double regenHealth
    {
        get { return m_fields[(int)CreatureFields.RegenHealth]; }
        set { _SetField(CreatureFields.RegenHealth, value); }
    }
    /// <summary>
    /// 每秒回复怒气值
    /// </summary>
    public double regenRage
    {
        get { return m_fields[(int)CreatureFields.RegenRage]; }
        set { _SetField(CreatureFields.RegenRage, value); }
    }
    /// <summary>
    /// 每秒回复生命百分比（基于最大生命）
    /// </summary>
    public double regenHealthPercent
    {
        get { return m_fields[(int)CreatureFields.RegenHealthPercent]; }
        set { _SetField(CreatureFields.RegenHealthPercent, value); }
    }
    /// <summary>
    /// 每秒回复怒气百分比（基于最大怒气）
    /// </summary>
    public double regenRagePercent
    {
        get { return m_fields[(int)CreatureFields.RegenRagePercent]; }
        set { _SetField(CreatureFields.RegenRagePercent, value); }
    }
    /// <summary>
    /// 攻击力
    /// </summary>
    public double attack
    {
        get { return m_fields[(int)CreatureFields.Attack]; }
        set { _SetField(CreatureFields.Attack, value); }
    }
    /// <summary>
    /// 副武器攻击力
    /// </summary>
    public double offAttack
    {
        get { return m_fields[(int)CreatureFields.OffAttack]; }
        set { _SetField(CreatureFields.OffAttack, value); }
    }
    /// <summary>
    /// 元素攻击力
    /// </summary>
    public double elementAttack
    {
        get { return m_fields[(int)CreatureFields.ElementAttack]; }
        set { _SetField(CreatureFields.ElementAttack, value); }
    }
    /// <summary>
    /// 风元素防御
    /// </summary>
    public double elementDefenseWind
    {
        get { return m_fields[(int)CreatureFields.ElementDefenseWind]; }
        set { _SetField(CreatureFields.ElementDefenseWind, value); }
    }
    /// <summary>
    /// 火元素防御
    /// </summary>
    public double elementDefenseFire
    {
        get { return m_fields[(int)CreatureFields.ElementDefenseFire]; }
        set { _SetField(CreatureFields.ElementDefenseFire, value); }
    }
    /// <summary>
    /// 水元素防御
    /// </summary>
    public double elementDefenseWater
    {
        get { return m_fields[(int)CreatureFields.ElementDefenseWater]; }
        set { _SetField(CreatureFields.ElementDefenseWater, value); }
    }
    /// <summary>
    /// 雷元素防御
    /// </summary>
    public double elementDefenseThunder
    {
        get { return m_fields[(int)CreatureFields.ElementDefenseThunder]; }
        set { _SetField(CreatureFields.ElementDefenseThunder, value); }
    }
    /// <summary>
    /// 冰元素防御
    /// </summary>
    public double elementDefenseIce
    {
        get { return m_fields[(int)CreatureFields.ElementDefenseIce]; }
        set { _SetField(CreatureFields.ElementDefenseIce, value); }
    }
    /// <summary>
    /// 防御
    /// </summary>
    public double defense
    {
        get { return m_fields[(int)CreatureFields.Defense]; }
        set { _SetField(CreatureFields.Defense, value); }
    }
    /// <summary>
    /// 铁骨
    /// </summary>
    public double firm
    {
        get { return m_fields[(int)CreatureFields.Firm]; }
        set { _SetField(CreatureFields.Firm, value); }
    }
    /// <summary>
    /// 残暴
    /// </summary>
    public double brutality
    {
        get { return m_fields[(int)CreatureFields.Brutality]; }
        set { _SetField(CreatureFields.Brutality, value); }
    }
    /// <summary>
    /// 暴击概率（百分比）
    /// </summary>
    public double crit
    {
        get { return m_fields[(int)CreatureFields.Crit]; }
        set { _SetField(CreatureFields.Crit, value); }
    }
    /// <summary>
    /// 暴击伤害加成（百分比）
    /// </summary>
    public double critMul
    {
        get { return m_fields[(int)CreatureFields.CritMul]; }
        set { _SetField(CreatureFields.CritMul, value); }
    }
    /// <summary>
    /// 韧性
    /// </summary>
    public double resilience
    {
        get { return m_fields[(int)CreatureFields.Resilience]; }
        set { _SetField(CreatureFields.Resilience, value); }
    }
    /// <summary>
    /// 攻击怒气回复加成（百分比）
    /// </summary>
    public double attackRageMul
    {
        get { return m_fields[(int)CreatureFields.AttackRageMul]; }
        set { _SetField(CreatureFields.AttackRageMul, value); }
    }
    /// <summary>
    /// 受击怒气怒气回复加成（百分比）
    /// </summary>
    public double attackedRageMul
    {
        get { return m_fields[(int)CreatureFields.AttackedRageMul]; }
        set { _SetField(CreatureFields.AttackedRageMul, value); }
    }
    /// <summary>
    /// 怒气回复加成（百分比）
    /// </summary>
    public double regenRageMul
    {
        get { return m_fields[(int)CreatureFields.RegenRageMul]; }
        set { _SetField(CreatureFields.RegenRageMul, value); }
    }
    /// <summary>
    /// 生命回复加成（百分比）
    /// </summary>
    public double regenHealthMul
    {
        get { return m_fields[(int)CreatureFields.RegenHealthMul]; }
        set { _SetField(CreatureFields.RegenHealthMul, value); }
    }
    /// <summary>
    /// 造成伤害加成（百分比）
    /// </summary>
    public double damageIncrease
    {
        get { return m_fields[(int)CreatureFields.DamageIncrease]; }
        set { _SetField(CreatureFields.DamageIncrease, value); }
    }
    /// <summary>
    /// 受到伤害减免（百分比）
    /// </summary>
    public double damageReduce
    {
        get { return m_fields[(int)CreatureFields.DamageReduce]; }
        set { _SetField(CreatureFields.DamageReduce, value); }
    }
    /// <summary>
    /// 等级
    /// </summary>
    public int level { get { return (int)GetField(CreatureFields.Level); } }

    public virtual int buffLevel { get { return 1; } }

    /// <summary>
    /// 碰撞盒子大小（半径）
    /// </summary>
    public double colliderSize
    {
        get { return m_fields[(int)CreatureFields.ColliderSize]; }
        set
        {
            if (value < 0) value = 0;

            var idx = (int)CreatureFields.ColliderSize;
            var oldVal = m_fields[idx];

            if (oldVal == value) return;
            m_fields[idx] = value;

            behaviour.collider_.size = new Vector2_(value * 2.0, colliderHeight);
        }
    }
    /// <summary>
    /// 碰撞盒子高度
    /// </summary>
    public double colliderHeight
    {
        get { return m_fields[(int)CreatureFields.ColliderHeight]; }
        set
        {
            if (value < 0) value = 0;

            var idx = (int)CreatureFields.ColliderHeight;
            var oldVal = m_fields[idx];

            if (oldVal == value) return;
            m_fields[idx] = value;

            behaviour.collider_.size = new Vector2_(colliderSize * 2.0, value);
            behaviour.collider_.offset = new Vector2_(0, value * 0.5 + colliderOffset);
        }
    }
    /// <summary>
    /// 碰撞盒子 Y 偏移
    /// </summary>
    public double colliderOffset
    {
        get { return m_fields[(int)CreatureFields.ColliderOffset]; }
        set
        {
            var idx = (int)CreatureFields.ColliderOffset;
            var oldVal = m_fields[idx];

            if (oldVal == value) return;
            m_fields[idx] = value;

            behaviour.collider_.offset = new Vector2_(0, colliderHeight * 0.5 + value);
        }
    }
    /// <summary>
    /// 受击盒子大小（半径）
    /// </summary>
    public double hitColliderSize
    {
        get { return m_fields[(int)CreatureFields.HitColliderSize]; }
        set
        {
            if (value < 0) value = 0;

            var idx = (int)CreatureFields.HitColliderSize;
            var oldVal = m_fields[idx];

            if (oldVal == value) return;
            m_fields[idx] = value;

            behaviour.hitCollider.size = new Vector2_(value * 2.0, height);

            behaviour.UpdateRenderGuard();
        }
    }
    /// <summary>
    /// 受击盒子高度
    /// </summary>
    public double height
    {
        get { return m_fields[(int)CreatureFields.Height]; }
        set
        {
            if (value < 0) value = 0;

            var idx = (int)CreatureFields.Height;
            var oldVal = m_fields[idx];

            if (oldVal == value) return;
            m_fields[idx] = value;

            behaviour.hitCollider.size = new Vector2_(hitColliderSize * 2.0, value);
            behaviour.hitCollider.offset = new Vector2_(0, value * 0.5 + hitColliderOffset);

            behaviour.UpdateRenderGuard();
        }
    }
    /// <summary>
    /// 受击盒子 Y 偏移
    /// </summary>
    public double hitColliderOffset
    {
        get { return m_fields[(int)CreatureFields.HitColliderOffset]; }
        set
        {
            var idx = (int)CreatureFields.HitColliderOffset;
            var oldVal = m_fields[idx];

            if (oldVal == value) return;
            m_fields[idx] = value;

            behaviour.hitCollider.offset = new Vector2_(0, height * 0.5 + value);
        }
    }
    /// <summary>
    /// 移动速度（基于标准速度的倍率）
    /// </summary>
    public double speedRun
    {
        get { return m_fields[(int)CreatureFields.MoveSpeed]; }
        set
        {
            if (value < 0) value = 0;

            var idx = (int)CreatureFields.MoveSpeed;
            var oldVal = m_fields[idx];

            if (oldVal == value) return;
            m_fields[idx] = value;

            m_stateMachine.SetParam(StateMachineParam.speedRun, value);

            DispatchEvent(CreatureEvents.FIELD_CHANGED, Event_.Pop(CreatureFields.MoveSpeed, oldVal, value));
        }
    }
    /// <summary>
    /// 攻击速度（基于标准速度的倍率）
    /// </summary>
    public double speedAttack
    {
        get { return m_fields[(int)CreatureFields.AttackSpeed]; }
        set
        {
            if (value < 0) value = 0;

            var idx = (int)CreatureFields.AttackSpeed;
            var oldVal = m_fields[idx];

            if (oldVal == value) return;
            m_fields[idx] = value;

            m_stateMachine.SetParam(StateMachineParam.speedAttack, value);

            DispatchEvent(CreatureEvents.FIELD_CHANGED, Event_.Pop(CreatureFields.AttackSpeed, oldVal, value));
        }
    }
    /// <summary>
    /// 蓄力速度
    ///</summary>
    public double chargeSpeed;
    /// <summary>
    /// 吸血
    ///</summary>
    public double leech;
    /// <summary>
    /// 子弹数量
    /// </summary>
    public int bulletCount
    {
        get { return m_bulletCount; }
        set
        {
            if (value < 0) value = 0;
            if (m_bulletCount == value) return;

            var e = Event_.Pop(m_bulletCount);
            m_bulletCount = value;

            DispatchEvent(CreatureEvents.BULLET_COUNT_CHANGED, e);
        }
    }
    /// <summary>
    /// 能量（用于变身）
    /// </summary>
    public int energy
    {
        get { return m_energy; }
        set
        {
            value = value < 0 ? 0 : value > m_maxEnergy ? (int)m_maxEnergy : value;
            if (m_energy == value) return;

            var e = Event_.Pop(m_energy);
            m_energy = value;

            if (m_stateMachine)
            {
                m_stateMachine.SetParam(StateMachineParam.energy, m_energy);
                m_stateMachine.SetParam(StateMachineParam.energyRate, energyRateL);
            }
            DispatchEvent(CreatureEvents.ENERGY_CHANGED, e);
        }
    }
    /// <summary>
    /// 能量上限
    /// </summary>
    public int maxEnergy
    {
        get { return m_maxEnergy; }
        set
        {
            if (value < 0) value = 0;
            if (m_maxEnergy == value) return;

            var e = Event_.Pop(m_maxEnergy);
            m_maxEnergy = value;

            if (m_energy > m_maxEnergy) energy = m_maxEnergy;
            else m_stateMachine?.SetParam(StateMachineParam.energyRate, energyRateL);

            DispatchEvent(CreatureEvents.ENERGY_CHANGED, e);
        }
    }

    public int AwakeDuration
    {
        get { return m_awakeDuration; }
        set { m_awakeDuration = value; }
    }
    /// <summary>
    /// 当前形态
    /// </summary>
    public CreatureMorph morph { get { return m_morph; } }

    public PRoleAttrItem[] awakeChangeAttr;

    #region Special states

    /// <summary>
    /// 是否无敌状态
    /// </summary>
    public bool invincible { get; private set; }
    /// <summary>
    /// 是否霸体状态
    /// </summary>
    public bool tough { get; private set; }
    /// <summary>
    /// 是否虚弱状态
    /// </summary>
    public bool weak { get; private set; }
    /// <summary>
    /// 是否隐身状态（可穿越其他障碍）
    /// </summary>
    public bool ethereal { get; private set; }
    /// <summary>
    /// 是否隐身状态（可被穿越）
    /// </summary>
    public bool passiveEthereal { get; private set; }
    /// <summary>
    /// 阻挡标识，表示当前角色具有的阻挡类型，可以用来扩展碰撞系统
    /// <para>一个角色最多可以包含 32 （0-31）个阻挡类型</para>
    /// <para>
    /// 当 obstacleMask 为非 0 值的时候，只有目标的 <see cref="ignoreObstacleMask"/> 包含所有当前角色的阻挡类型时，目标才能穿越当前角色
    /// </para>
    /// <para>
    /// 比如，当前 obstacleMask = 15 (0x01 | 0x02 | 0x04 | 0x08)，表示阻挡类型 0 1 2 3，只有对方的 <see cref="ignoreObstacleMask"/> 同时包含了 0 1 2 3 时对方才能穿过
    /// </para>
    /// <para>See <see cref="ethereal"/> and <seealso cref="passiveEthereal"/></para>
    /// </summary>
    public int obstacleMask { get; set; }
    /// <summary>
    /// 忽略阻挡标识，表示当前角色在碰撞检测时忽略指定类型的阻挡物
    /// <para>该标志配合<see cref="obstacleMask"/>来扩展碰撞系统</para>
    /// <para>注意：obstacle 和 ignoreObstacle 的碰撞和原有的 <see cref="ethereal"/> 和 <seealso cref="passiveEthereal"/> 是相互独立的，在做碰撞检测时会同时检测两个条件</para>
    /// </summary>
    public int ignoreObstacleMask { get; set; }
    /// <summary>
    /// 是否显示边缘光特效
    /// 只有在 rim 为 true 并且包含边缘光效果时角色边缘光才可见
    /// </summary>
    public bool rim { get; private set; }
    /// <summary>
    /// 是否无效化攻击盒子
    /// 攻击盒子无效化时 所有盒子都无法命中目标
    /// </summary>
    public bool clearAttack { get; private set; }

    /// <summary>
    /// 霸体等级
    /// 当前霸体等级
    /// 注意，等级不能用来判定当前角色的霸体状态，因为等级可以是任意数字
    /// 使用 tough 来判定是否处于霸体状态
    /// </summary>
    public int toughLevel
    {
        get { return m_toughLevel; }
        set
        {
            if (m_toughLevel.Equals(value)) return;
            m_toughLevel = value;
        }
    }
    public int breakToughLevel
    {
        get { return m_breakToughLevel; }
        set
        {
            if (m_breakToughLevel.Equals(value)) return;
            m_breakToughLevel = value;
        }
    }

    private int m_hpVisiableCount;
    public int hpVisiableCount
    {
        get { return m_hpVisiableCount; }
        set
        {
            var b = hpVisiableCount > 0;
            var v = value > 0;
            m_hpVisiableCount = value;
            if (b == v)
                return;
            DispatchEvent(CreatureEvents.SET_HEALTH_BAR_VISIABLE_UI, Event_.Pop(v));
        }
    }

    public bool hpVisiable
    {
        get { return m_hpVisiableCount > 0 && visible; }
    }

    /// <summary>
    /// 无敌数量
    /// 数量大于 0 时表示无敌状态
    /// </summary>
    public int invincibleCount
    {
        get { return m_invincibleCount; }
        set
        {
            if (m_invincibleCount == value) return;
            m_invincibleCount = value < 0 ? 0 : value;
            invincible = m_invincibleCount > 0;

            UpdateRimLight();
        }
    }
    /// <summary>
    /// 霸体数量
    /// 数量大于 0 时表示霸体状态
    /// </summary>
    public int toughCount { get { return m_toughCount; } }
    /// <summary>
    /// 虚弱数量
    /// 数量大于 0 时表示虚弱状态
    /// </summary>
    public int weakCount
    {
        get { return m_weakCount; }
        set
        {
            if (m_weakCount == value) return;
            m_weakCount = value < 0 ? 0 : value;
            weak = m_weakCount > 0;
            m_stateMachine.SetParam(StateMachineParam.weak, weak);
        }
    }

    public bool HawkEye { get { return m_hawkEyeCount > 0; } }
    public int m_hawkEyeCount;

    public int HawkEyeCount
    {
        get { return m_hawkEyeCount; }
        set { m_hawkEyeCount = Math.Max(0, value); }
    }

    /// <summary>
    /// 虚灵数量
    /// 数量大于 0 时表示虚灵状态  虚灵状态可以穿过其它角色
    /// </summary>
    public int etherealCount
    {
        get { return m_etherealCount; }
        set
        {
            if (m_etherealCount == value) return;
            m_etherealCount = value < 0 ? 0 : value;
            ethereal = m_etherealCount > 0;
            behaviour.collider_.enabled = !isDead && !ethereal;
        }
    }
    /// <summary>
    /// 被动虚灵数量
    /// 数量大于 0 时表示被动虚灵状态  虚灵状态可以被其它角色穿过
    /// </summary>
    public int passiveEtherealCount
    {
        get { return m_passiveEtherealCount; }
        set
        {
            if (m_passiveEtherealCount == value) return;
            m_passiveEtherealCount = value < 0 ? 0 : value;
            passiveEthereal = m_passiveEtherealCount > 0;
        }
    }
    /// <summary>
    /// 边缘光数量
    /// 数量大于 0 时将会显示边缘光
    /// </summary>
    public int rimCount
    {
        get { return m_rimCount; }
        set
        {
            if (m_rimCount == value) return;
            m_rimCount = value < 0 ? 0 : value;
            rim = m_rimCount > 0;

            UpdateRimLight();
        }
    }
    /// <summary>
    /// 当前攻击盒子无效化数量
    /// 数量大于 0 时表示攻击盒子无效
    /// </summary>
    public int clearAttackCount
    {
        get { return m_clearAttackCount; }
        set
        {
            if (m_clearAttackCount == value) return;
            m_clearAttackCount = value < 0 ? 0 : value;
            clearAttack = m_clearAttackCount > 0;
        }
    }

    #endregion

    #endregion

    #region Protected values

    protected int m_weaponID;
    protected int m_offWeaponID;
    protected int m_weaponItemID;
    protected int m_offWeaponItemID;

    protected CreatureMorph m_morph = CreatureMorph.Normal;

    protected int m_comboBreakCountDown;

    #endregion

    #region Private values

    private int _health
    {
        get { return (int)m_fields[(int)CreatureFields.Health]; }
        set
        {
            value = Mathf.Clamp(value, 0, maxHealth);

            var idx = (int)CreatureFields.Health;
            var oldVal = m_fields[idx];

            if (oldVal == value) return;
            m_fields[idx] = value;

            #region Debug log
            #if (DEVELOPMENT_BUILD || UNITY_EDITOR) && DETAIL_BATTLE_LOG
            Logger.LogWarning("[{1}:{2}-{3}], Creature {0} health changed to {4} from {5}", name, Level.levelTime, frameTime, frameCount, value, oldVal);
            #endif

            #if AI_LOG
            Module_AI.LogBattleMsg(this, "[health changed to {0} from {1}]", value, oldVal);
            #endif
            #endregion

            DispatchEvent(CreatureEvents.FIELD_CHANGED, Event_.Pop(CreatureFields.Health, oldVal, (double)value));
            DispatchEvent(CreatureEvents.HEALTH_CHANGED, Event_.Pop(oldVal, value, true));

            if (value == 0)
            {
                DispatchEvent(CreatureEvents.Dying);

                isDead = health < 1;
                realDead = false;

                if (isDead)
                {
                    m_stateMachine.SetParam(StateMachineParam.isDead, true);
                    m_stateMachine.SetParam(StateMachineParam.key, 0); // Clear key input

                    behaviour.hitCollider.UpdateDeadCombo(true);

                    DispatchEvent(CreatureEvents.DEAD);
                }
            }
            else if (oldVal == 0)
            {
                if (m_stateMachine != null)
                {
                    m_stateMachine.SetParam(StateMachineParam.isDead, false);
                    m_stateMachine.SetParam(StateMachineParam.realDead, false);
                    if (isDead) m_stateMachine.TranslateTo(StateMachineState.STATE_IDLE);
                    behaviour.UpdateAllColliderState(true);
                }
                isDead   = false;
                realDead = false;
            }
        }
    }

    protected _Double[] m_fields       = new _Double[(int)CreatureFields.Count];
    protected _Double[] m_tmpFields    = new _Double[(int)CreatureFields.Count];

    private _Int m_toughLevel;
    private _Int m_breakToughLevel;

    private _Int m_invincibleCount;
    private _Int m_weakCount;
    private _Int m_toughCount;
    private _Int m_etherealCount;
    private _Int m_passiveEtherealCount;
    private _Int m_rimCount;
    private _Int m_clearAttackCount;

    private _Int m_bulletCount;
    private _Int m_energy;
    private _Int m_maxEnergy;
    private _Int m_configID;
    private _Int m_awakeDuration;

    #endregion

    #region Modify/Get/Set interface

    /// <summary>
    /// Get target element defense
    /// </summary>
    /// <param name="elementType">Target element type</param>
    /// <returns></returns>
    public int GetElementDefense(int elementType)
    {
        if (elementType == 0 || elementType > Ice) return 0;
        return (int)m_fields[ElementStartIndex + elementType];
    }

    /// <summary>
    /// Get target element defense
    /// </summary>
    /// <param name="elementType">Target element type</param>
    /// <returns></returns>
    public int GetElementDefense(CreatureElementTypes elementType)
    {
        return GetElementDefense((int)elementType);
    }

    public double ModifyField(string fieldName, double value)
    {
        if (!System.Enum.IsDefined(typeof(CreatureFields), fieldName))
        {
            Logger.LogWarning("Creature::ModifyField: ModifyField field [{0}] failed, please check your formula.", fieldName);
            return 0;
        }

        return ModifyField((CreatureFields)System.Enum.Parse(typeof(CreatureFields), fieldName), value);
    }

    public double ModifyField(CreatureFields index, double value)
    {
        if (index < 0 || index >= CreatureFields.Count) return 0;

        var nv = GetField(index) + value;
        return _SetField(index, nv);
    }

    public double ModifyField(int index, double value)
    {
        if (index < 0 || index >= m_fields.Length) return 0;

        var nv = GetField(index) + value;
        return _SetField((CreatureFields)index, nv);
    }

    public double GetField(string fieldName)
    {
        if (!System.Enum.IsDefined(typeof(CreatureFields), fieldName))
        {
            Logger.LogWarning("Creature::GetField: Parse field [{0}] failed, please check your formula.", fieldName);
            return 0;
        }

        return GetField((CreatureFields)System.Enum.Parse(typeof(CreatureFields), fieldName));
    }

    public void SetField(string fieldName, double value)
    {
        if (!System.Enum.IsDefined(typeof(CreatureFields), fieldName))
        {
            Logger.LogWarning("Creature::GetField: Set field [{0}] failed, please check your formula.", fieldName);
            return;
        }

        _SetField((CreatureFields)System.Enum.Parse(typeof(CreatureFields), fieldName), value);
    }

    public double GetField(CreatureFields index)
    {
        return GetField((int)index);
    }

    public double GetField(int index)
    {
        var val = index < 0 || index >= m_fields.Length ? 0 : m_fields[index];
        return val;
    }

    public double SetField(CreatureFields index, double value)
    {
        if (index < 0 || index >= CreatureFields.Count) return 0;
        return _SetField(index, value);
    }

    public double SetField(int index, double value)
    {
        if (index < 0 || index >= m_fields.Length) return 0;
        return _SetField((CreatureFields)index, value);
    }

    protected double _SetField(CreatureFields index, double value)
    {
        var negative = false;

        var idx = (int)index;
        var oldVal = m_fields[idx];

        switch (index)
        {
            case CreatureFields.MaxHealth:         maxHealth        = (int)value;   return maxHealth        - oldVal;
            case CreatureFields.Health:            health           = (int)value;   return health           - oldVal;
            case CreatureFields.MaxRage:           maxRage          = value;        return maxRage          - oldVal;
            case CreatureFields.Rage:              rage             = value;        return rage             - oldVal;
            case CreatureFields.AttackSpeed:       speedAttack      = value;        return speedAttack      - oldVal;
            case CreatureFields.MoveSpeed:         speedRun         = value;        return speedRun         - oldVal;
            case CreatureFields.ColliderHeight:    colliderHeight   = value;        return colliderHeight   - oldVal;
            case CreatureFields.ColliderOffset:    colliderOffset   = value;        return colliderOffset   - oldVal;
            case CreatureFields.ColliderSize:      colliderSize     = value;        return colliderSize     - oldVal;
            case CreatureFields.HitColliderOffset: hitColliderOffset= value;        return hitColliderOffset- oldVal;
            case CreatureFields.HitColliderSize:   hitColliderSize  = value;        return hitColliderSize  - oldVal;
            case CreatureFields.RegenRageMul:
            case CreatureFields.RegenHealthMul:
            case CreatureFields.AttackRageMul:
            case CreatureFields.AttackedRageMul:
            case CreatureFields.DamageIncrease:
            case CreatureFields.DamageReduce:     negative = true; break;
            default: break;
        }

        if (negative && value < 0) value = 0;

        if (oldVal == value) return 0;
        m_fields[idx] = value;

        DispatchEvent(CreatureEvents.FIELD_CHANGED, Event_.Pop(index, oldVal, value));

        return value - oldVal;
    }

    #endregion
}
