/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Creature Enum definitions
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-08-15
 * 
 ***************************************************************************************************/

 /// <summary>
 /// 朝向
 /// 朝向仅有两个，分别对应 左边(Back)和右边(Forward)
 /// 它们对应了 Vector3.left 和 Vector3.right
 /// </summary>
public enum CreatureDirection
{
    FORWARD = 0,
    BACK    = 1
}

/// <summary>
/// 阵营设置
/// </summary>
public enum CreatureCamp
{
    None,

    /// <summary>
    /// 怪物阵营
    /// </summary>
    MonsterCamp,

    /// <summary>
    /// 玩家阵营，包含NPC
    /// </summary>
    PlayerCamp,

    /// <summary>
    /// 中立阵营
    /// </summary>
    NeutralCamp,
}

/// <summary>
/// 元素类型
/// </summary>
public enum CreatureElementTypes
{
    None    = 0,
    /// <summary>
    /// 风元素
    /// </summary>
    Wind    = 1,
    /// <summary>
    /// 火元素
    /// </summary>
    Fire    = 2,
    /// <summary>
    /// 水元素
    /// </summary>
    Water   = 3,
    /// <summary>
    /// 雷元素
    /// </summary>
    Thunder = 4,
    /// <summary>
    /// 冰元素
    /// </summary>
    Ice     = 5,
    Count,
}

/// <summary>
/// 属性枚举
/// </summary>
public enum CreatureFields
{
    /// <summary>
    /// Unused
    /// </summary>
    Unused0               = 0,
    /// <summary>
    /// 类型
    /// </summary>
    Type                  = 1,
    /// <summary>
    /// 力量
    /// </summary>
    Strength              = 2,
    /// <summary>
    /// 技巧
    /// </summary>
    Artifice              = 3,
    /// <summary>
    /// 体力
    /// </summary>
    Vitality              = 4,
    /// <summary>
    /// 最大生命值
    /// </summary>
    MaxHealth             = 5,
    /// <summary>
    /// 最大怒气
    /// </summary>
    MaxRage               = 6,
    /// <summary>
    /// 攻击力
    /// </summary>
    Attack                = 7,
    /// <summary>
    /// 副武器攻击
    /// </summary>
    OffAttack             = 17,
    /// <summary>
    /// 元素攻击
    /// </summary>
    ElementAttack         = 18,
    /// <summary>
    /// 风元素防御
    /// </summary>
    ElementDefenseWind    = 19,
    /// <summary>
    /// 火元素防御
    /// </summary>
    ElementDefenseFire    = 20,
    /// <summary>
    /// 水元素防御
    /// </summary>
    ElementDefenseWater   = 21,
    /// <summary>
    /// 雷元素防御
    /// </summary>
    ElementDefenseThunder = 22,
    /// <summary>
    /// 冰元素防御
    /// </summary>
    ElementDefenseIce     = 23,
    /// <summary>
    /// 防御力
    /// </summary>
    Defense               = 8,
    /// <summary>
    /// 暴击加成
    /// </summary>
    CritMul               = 9,
    /// <summary>
    /// 暴击概率
    /// </summary>
    Crit                  = 10,
    /// <summary>
    /// 韧性
    /// </summary>
    Resilience            = 11,
    /// <summary>
    /// 攻击速度
    /// </summary>
    AttackSpeed           = 12,
    /// <summary>
    /// 移动速度
    /// </summary>
    MoveSpeed             = 13,
    /// <summary>
    /// 铁骨
    /// </summary>
    Firm                  = 14,
    /// <summary>
    /// 残暴
    /// </summary>
    Brutality             = 15,
    /// <summary>
    /// 每秒怒气回复
    /// </summary>
    RegenRage             = 16,
    /// <summary>
    /// 元素类型   CreatureElementTypes
    /// </summary>
    ElementType           = 24,
    /// <summary>
    /// 每秒生命回复
    /// </summary>
    RegenHealth           = 25,
    /// <summary>
    /// 等级
    /// </summary>
    Level                 = 26,
    /// <summary>
    /// 当前生命
    /// </summary>
    Health                = 27,
    /// <summary>
    /// 当前怒气
    /// </summary>
    Rage                  = 28,
    /// <summary>
    /// 碰撞大小（半径）
    /// </summary>
    ColliderSize          = 29,
    /// <summary>
    /// 攻击怒气加成
    /// </summary>
    AttackRageMul         = 30,
    /// <summary>
    /// 受击怒气加成
    /// </summary>
    AttackedRageMul       = 31,
    /// <summary>
    /// 怒气回复加成
    /// </summary>
    RegenRageMul          = 32,
    /// <summary>
    /// 生命回复加成
    /// </summary>
    RegenHealthMul        = 33,
    /// <summary>
    /// 造成伤害加成
    /// </summary>
    DamageIncrease        = 34,
    /// <summary>
    /// 受到伤害减免
    /// </summary>
    DamageReduce          = 35,
    /// <summary>
    /// 每秒回复怒气百分比（基于最大怒气）
    /// </summary>
    RegenRagePercent      = 36,
    /// <summary>
    /// 每秒回复生命百分比（基于最大怒气）
    /// </summary>
    RegenHealthPercent    = 37,
    /// <summary>
    /// 受击盒子大小（半径）
    /// </summary>
    HitColliderSize       = 38,
    /// <summary>
    /// 受击盒子高度
    /// </summary>
    Height                = 39,
    /// <summary>
    /// 碰撞盒子高度
    /// </summary>
    ColliderHeight        = 40,
    /// <summary>
    /// 碰撞盒子 Y 偏移
    /// </summary>
    ColliderOffset        = 41,
    /// <summary>
    /// 受击盒子 Y 偏移
    /// </summary>
    HitColliderOffset     = 42,
    /// <summary>
    /// 属性数量
    /// </summary>
    Count,
}

/// <summary>
/// 玩家职业类型枚举
/// </summary>
public enum CreatureVocationType
{
    All         = 0,            //通用
    Vocation1   = 1,            //职业1
    Vocation2   = 2,            //职业2
    Vocation3   = 3,            //职业3
    Vocation4   = 4,            //职业4
    Vocation5   = 5,            //职业5
    Count
}

/// <summary>
/// 形态
/// 用于变身
/// </summary>
public enum CreatureMorph
{
    /// <summary>
    /// 普通形态
    /// </summary>
    Normal   = 0,
    /// <summary>
    /// 觉醒形态
    /// </summary>
    Awake    = 1,
    Count
}