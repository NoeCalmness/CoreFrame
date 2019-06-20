/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 主要用于标识所有PVE的枚举
 * 
 * Author:   Lee
 * Version:  0.1
 * Created:  2017-09-05
 * 
 ***************************************************************************************************/

public enum EnumLabyrinthPlayerState
{
    None = 0,

    Idle = 1,               //空闲状态

    Battle = 2,             //战斗状态（只要进入了迷宫界面就是该状态）

    SneakProtect = 4,       //重伤状态（被偷袭成功后的保护状态）

    ForceRest = 8,          //重伤状态（血量为0后的重伤状态,强制恢复）

    ProtectByBell = 16,     //铃铛保护状态

    InPveBattle = 32,       //正在pve战斗中
}
public enum EnumActiveState
{
    CanPick = 0,             //可领取但未领取 (没领取)

    AlreadPick = 1,          //已领取状态

    NotPick = 2,            //不可领取

    Null = 3,            //不可领取

}

public enum EnumLabyrinthArea
{
    /// <summary>
    /// 降级区
    /// </summary>
    DemotionArea = 1,

    /// <summary>
    /// 保级区
    /// </summary>
    RelegationArea,

    /// <summary>
    /// 晋级区
    /// </summary>
    PromotionArea,
}

public enum EnumLabyrinthReportType
{
    /// <summary>
    /// 其他玩家（非自己）使用了陷阱道具
    /// 参数：道具ID
    /// </summary>
    OtherUseTrapProp = 0,   
    
    /// <summary>
    /// 其他玩家对自己使用攻击道具
    /// 参数：道具ID
    /// </summary>
    OtherAttackSelf,

    /// <summary>
    /// 自己被别人偷袭成功
    /// 参数：下降的层数
    /// </summary>
    BeSneakedSuccess,

    /// <summary>
    /// 自己被别人偷袭失败
    /// 参数：无参
    /// </summary>
    BeSneakFailed,

    /// <summary>
    /// 其他玩家使用了猎人铃铛
    /// 参数：玩家ID
    /// </summary>
    OtherUseHunterBell,

    /// <summary>
    /// 自己使用了陷阱道具
    /// 参数：迷宫ID（客户端处理），陷阱层数，道具ID
    /// </summary>
    SelfUseTrapProp,

    /// <summary>
    /// 自己对其他玩家使用攻击道具
    /// 参数：玩家ID，道具ID
    /// </summary>
    SelfAttackOther,

    /// <summary>
    /// 自己偷袭其他玩家成功
    /// 参数：玩家ID，其他玩家被偷袭后的层数（比如从40层掉到了38层，此时传回来的参数值应该是38）
    /// </summary>
    SelfSneakSuccess,

    /// <summary>
    /// 自己偷袭其他玩家失败
    /// 参数：玩家ID
    /// </summary>
    SelfSneakFailed,

    /// <summary>
    /// 自己使用了保护性道具
    /// 参数：道具ID
    /// </summary>
    SelfUseProtectProp,

    /// <summary>
    /// 自己在使用了猎狗道具的前提下触发陷阱
    /// 参数：触发的陷阱道具ID
    /// </summary>
    SelfBeTrappedAfterUseDog,

    /// <summary>
    /// 自己进入了强制休整
    /// 参数：无参
    /// </summary>
    SelfForceRest,

    /// <summary>
    /// 自己从强制休整中恢复
    /// 参数：无参
    /// </summary>
    SelfRecoveryFormRest,

    /// <summary>
    /// 自己身上炸弹爆炸
    /// 参数：掉落的层数
    /// </summary>
    SelfBombExplode,

    TotalCount,
}

public enum EnumLabyrinthTimeStep
{
    /// <summary>
    /// 开服前几天，迷宫活动根本不会开启，也不会产生倒计时
    /// </summary>
    None = 0,

    /// <summary>
    /// 关闭状态,此时会返回活动开启倒计时
    /// </summary>
    Close,
    
    /// <summary>
    /// 迷宫准备阶段,此时会返回活动开启倒计时
    /// </summary>
    Ready,

    /// <summary>
    /// 挑战阶段 返回剩余挑战时间
    /// </summary>
    Chanllenge,

    /// <summary>
    /// 修整阶段，此时还能进入迷宫，但不能进行挑战，返回剩余的修整时间
    /// </summary>
    Rest,

    /// <summary>
    /// 结算阶段,服务器准备发放奖励，返回剩余发奖时间
    /// </summary>
    SettleMent,
}

/// <summary>
/// the state when pve is over 
/// </summary>
public enum PVEOverState
{
    None = -1,

    Success = 0,

    RoleDead,

    GameOver,
}

/// <summary>
/// which panel should be opened when we exit from pve level
/// </summary>
public enum PVEReOpenPanel
{
    None = -1,

    //追捕
    ChasePanel,

    //无主之地
    Borderlands,

    //迷宫
    Labyrinth,

    //符文
    RunePanel,

    //打造武器界面
    EquipPanel,

    //宠物关卡
    PetPanel,

    //公会boss
    UnionBoss,

    //觉醒
    Awake,

    //npc约会
    Dating,

    Count,
}


/// <summary>
/// PVE内部统计数据的类型
/// </summary>
public enum EnumPVEDataType
{
    Invalid = -1,

    Health = 0,         //血量
    
    Rage,               //怒气

    Combo,              //连击数

    BeHitedTimes,       //受击次数

    GameTime,           //游戏时间

    UltimateTimes,      //大招次数

    ExecutionTimes,     //处决次数

    Attack,             //攻击伤害

    Count,
}

/// <summary>
/// 自动战斗条件类型
/// </summary>
public enum EnumPVEAutoBattleType
{
    Forbidden = 0,          //禁止自动战斗

    NoCondition,            //无条件自动战斗

    StageClear,             //关卡通关（对任意PVE模式有效）

    ThreeStars,             //三星通关（针对追捕）
}

public enum EnumPVEAutoBattleState
{
    Disable,                //隐藏
    Enable,                 //正常操作
    EnableNotStageClear,    //显示按钮但是提示：通关后才能使用自动战斗
    EnableNotThreeStars,    //显示按钮但是提示：获得三星才能使用自动战斗
}

public enum EnumPVETimerType
{
    NormalTimer = 0,        //普通的计时器操作
    BossComing,             //BOSS出现之前的预警动画
}

/// <summary>
/// 怪物左右箭头的提示
/// </summary>
public enum EnumMonsterTipArrow
{
    None = 0,

    MonserLeft = 1,
    MonsterRight = 2,

    BossLeft = 4,
    BossRight = 8,

    NpcLeft = 16,
    NpcRight = 32,
}

public enum EnumPveLevelType
{
    NormalPVE,      //普通状态

    UnionBoss,      //工会BOSS

    TeamLevel,     //组队模式
}

public enum EnumPVELoadingType
{
    None = 0,
    Initialize,         //初始化加载场景
    TransportScene,     //切换场景
}

/// <summary>
/// 场景实体物体的状态类型
/// </summary>
public enum EnumActorStateType
{
    EnterState = 1,     //状态开始

    ExitState = 2,      //状态结束
}

/// <summary>
/// 场景实体物体的状态类型
/// </summary>
public enum EnumMonsterType
{
    NormalType = 0,     //普通怪物

    SceneActor = 1,      //场景实物

    Destructible = 2,   //可破坏物
}

/// <summary>
/// 怪物锁敌类型
/// </summary>
public enum EnumLockMonsterType
{
    Close = -1,              //不能被AI选中
    LockEnermyCamp = 0,     //只能被敌对目标选中
}
