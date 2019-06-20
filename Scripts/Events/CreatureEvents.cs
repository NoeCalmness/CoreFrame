/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Global Events definitions.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-01
 * 
 ***************************************************************************************************/

public static class CreatureEvents
{
    /// <summary>
    /// sender : Creature          argument: Event
    /// trigger: 玩家角色创建后
    /// </summary>
    public const string PLAYER_ADD_TO_SCENE       = "OnPlayerAddToScene";
    /// <summary>
    /// sender : Creature          argument: Event[bool] 播放状态
    /// trigger: 当角色动画播放状态变化时
    /// </summary>
    public const string STATE_PLAYABLE            = "OnCreatureStatePlayable";
    /// <summary>
    /// sender : Creature          argument: Event[bool] 冰冻状态
    /// trigger: 当角色动画锁定状态变化时
    /// </summary>
    public const string FREEZ_STATE               = "OnCreatureFreezState";
    /// <summary>
    /// sender : Creature          argument: Event
    /// trigger: 当角色被杀死，即将死亡（即生命值变为 0）时
    /// </summary>
    public const string Dying                     = "OnCreatureDying";
    /// <summary>
    /// sender : Creature          argument: Event
    /// trigger: 当角色死亡（即生命值变为 0）时
    /// </summary>
    public const string DEAD                      = "OnCreatureDead";
    /// <summary>
    /// sender : Creature          argument: Event
    /// trigger: 当角色死亡动画播放结束时
    /// </summary>
    public const string DEAD_ANIMATION_END        = "OnCreatureDeadAnimationEnd";
    /// <summary>
    /// sender : Creature          argument: Event[int,int,bool] 改变前值,改变后值,是否是由伤害触发的
    /// trigger: 当角色生命值变化时
    /// </summary>
    public const string HEALTH_CHANGED            = "OnCreatureHealthChanged";
    /// <summary>
    /// sender : Creature          argument: Event[double,double] 改变前值,改变后值
    /// trigger: 当角色怒气值变化时
    /// </summary>
    public const string RAGE_CHANGED              = "OnCreatureRageChanged";
    /// <summary>
    /// sender : Creature          argument: Event[CreatureFields,double,double] 改变的属性枚举,改变前值,改变后值
    /// trigger: 当角色任意属性变化时
    /// </summary>
    public const string FIELD_CHANGED             = "OnCreatureFieldChanged";
    /// <summary>
    /// sender : Creature          argument: Event[Creature,DamageInfo] 受伤目标,伤害信息
    /// trigger: 当角色对其他目标施加伤害时
    /// </summary>
    public const string DEAL_DAMAGE               = "OnCreatureDealDamage";
    /// <summary>
    /// sender : Creature          argument: Event[Creature,DamageInfo] 伤害来源,伤害信息
    /// trigger: 当角色造成间接间接伤害时，比如反弹伤害或者buff伤害等
    /// </summary>
    public const string DEAL_UI_DAMAGE            = "OnCreatureDealUIDamage";
    /// <summary>
    /// sender : Creature          argument: Event[Creature,DamageInfo,int] 伤害来源,伤害信息,反弹的伤害
    /// trigger: 当角色受到伤害时
    /// </summary>
    public const string TAKE_DAMAGE               = "OnCreatureTakeDamage";
    /// <summary>
    /// sender : Creature          argument: Event[Creature,DamageInfo] 伤害来源,伤害信息
    /// trigger: 当角色即将受到伤害，但尚未计算伤害减免时
    /// </summary>
    public const string WILL_TAKE_DAMAGE          = "OnCreatureWillTakeDamage";
    /// <summary>
    /// sender : Creature          argument: Event[Creature,AttackInfo] 目标,攻击信息
    /// trigger: 当角色计算针对其他角色的伤害时
    /// </summary>
    public const string CALCULATE_DAMAGE          = "OnCreatureCalculateDamage";
    /// <summary>
    /// sender : Creature          argument: Event[Creature,AttackInfo] 受击者,攻击信息
    /// trigger: 当角色对其他角色执行攻击时
    /// </summary>
    public const string ATTACK                    = "OnCreatureAttack";
    /// <summary>
    /// sender : Creature          argument: Event[Creature,AttackInfo] 攻击者,攻击信息
    /// trigger: 当角色被攻击时
    /// </summary>
    public const string ATTACKED                  = "OnCreatureAttacked";
    /// <summary>
    /// sender : Creature          argument: Event[Creature,AttackInfo] 攻击者,攻击信息
    /// trigger: 当角色被枪击时
    /// </summary>
    public const string SHOOTED                   = "OnCreatureShooted";
    /// <summary>
    /// sender : Creature          argument: Event[Creature,AttackInfo] 击杀目标,攻击信息
    /// trigger: 击杀目标时
    /// </summary>
    public const string KILL                      = "OnCreatureKill";

    /// <summary>
    /// sender : Creature          argument: Event[Creature,AttackInfo] 击杀者,攻击信息
    /// trigger: 被击杀时
    /// </summary>
    public const string BE_KILL                   = "OnCreatureBeKill";
    /// <summary>
    /// sender : Creature          argument: Event[Creature] 击杀目标
    /// trigger: 击杀目标,目标播放死亡动作结束时
    /// </summary>
    public const string KILL_TARGET_DEAD_ANIMATION_END = "OnKillTargetDeadAnimationEnd";
    /// <summary>
    /// sender : Creature          argument: Event[StateMachineState,StateMachineState] 旧状态,新状态
    /// trigger: 当角色切换到其它状态时
    /// </summary>
    public const string ENTER_STATE               = "OnCreatureEnterState";
    /// <summary>
    /// sender : Creature          argument: Event[StateMachineState,StateMachineState] 旧状态,新状态
    /// trigger: 当角色退出状态时
    /// </summary>
    public const string QUIT_STATE                = "OnCreatureQuitState";
    /// <summary>
    /// sender : Creature          argument: Event
    /// trigger: 当角色单次受击连招结束清除受击信息时
    /// </summary>
    public const string HIT_INFO_CLEARED          = "OnCreatureHitInfoCleared";
    /// <summary>
    /// sender : Creature          argument: Event
    /// trigger: 当角色朝向更改时
    /// </summary>
    public const string DIRECTION_CHANGED         = "OnCreatureDirectionChanged";
    /// <summary>
    /// sender : Creature          argument: Event[int] 更改前的子弹数量
    /// trigger: 当角色持有子弹数量更改时
    /// </summary>
    public const string BULLET_COUNT_CHANGED      = "OnCreatureBulletCountChanged";
    /// <summary>
    /// sender : Creature          argument: Event[int] 更改前的能量
    /// trigger: 当角色能量发生变化时
    /// </summary>
    public const string ENERGY_CHANGED            = "OnCreatureEnergyChanged";
    /// <summary>
    /// sender : Creature          argument: Event[int,int] 提示类型,参数
    /// trigger: 当角色行为需要 UI 提示时
    /// </summary>
    public const string BATTLE_UI_INFO            = "OnCreatureBattleUIInfo";
    /// <summary>
    /// sender : Creature          argument: Event[Creature] 原本要命中的目标
    /// trigger: 当角色成功执行攻击行为，但命中失败时
    /// </summary>
    public const string ATTACK_HIT_FAILED         = "OnCreatureAttackHitFailed";
    /// <summary>
    /// sender : Creature          argument: Event
    /// trigger: 当攻击盒子首次命中目标时
    /// </summary>
    public const string FIRST_HIT                 = "OnCreatureFirstHit";
    /// <summary>
    /// sender : Creature
    /// trigger: 当角色连招中断时
    /// </summary>
    public const string COMBO_BREAK               = "OnCreatureComboBreak";
    /// <summary>
    /// sender : Creature          argument: Event[CreatureMorph,CreatureMorph] 旧形态,新型态
    /// trigger: 当角色形态发生更改时
    /// </summary>
    public const string MORPH_CHANGED             = "OnCreatureMorphChanged";
    /// <summary>
    /// sender : Creature          argument: Event[CreatureMorph,CreatureMorph] 旧形态,新型态
    /// trigger: 当角色形态发生更改时
    /// </summary>
    public const string MORPH_MODEL_CHANGED       = "OnCreatureMorphModelChanged";
    /// <summary>
    /// sender : Creature          argument: Event[CreatureMorph,Buff] 当前形态,变身 Buff
    /// trigger: 当角色变身形态 BUFF 更新时
    /// </summary>
    public const string MORPH_PROGRESS            = "OnCreatureMorphProgress";
    /// <summary>
    /// sender : Creature          argument: Event[Buff] buff触发
    /// trigger: 当角色身上 BUFF 效果触发时
    /// </summary>
    public const string BUFF_TRIGGER              = "OnCreatureBuffTrigger";
    /// <summary>
    /// sender : Creature          argument: Event[Buff] buff触发
    /// trigger: 当角色身上 BUFF 移除时
    /// </summary>
    public const string BUFF_REMOVE               = "OnCreatureBuffRemove";
    /// <summary>
    /// sender : Creature          argument: Event[Buff] buff触发
    /// trigger: 当角色身上 添加BUFF时
    /// </summary>
    public const string BUFF_CREATE               = "OnCreatureBuffCreate";
    /// <summary>
    /// sender : Creature          argument: Event
    /// trigger: 当 Creature.ResetLayer 被调用时
    /// </summary>
    public const string RESET_LAYERS              = "OnCreatureResetLayers";
    /// <summary>
    /// sender : Creature          argument: Event
    /// trigger: 当 useAI 状态变化时 被调用时
    /// </summary>
    public const string AUTO_BATTLE_CHANGE        = "OnCreatureAutoBattleStateChange";
    /// <summary>
    /// sender : Creature          argument: Event(bool)
    /// trigger: 当 需要显示隐藏血条时调用 被调用时，接受方处理实际显示
    /// </summary>
    public const string SET_HEALTH_BAR_VISIABLE_UI    = "OnSetHealthBarVisiable";

}
