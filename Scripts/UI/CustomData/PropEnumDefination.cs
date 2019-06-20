/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 主要用于标识所有道具的枚举
 * 
 * Author:   Lee
 * Version:  0.1
 * Created:  2017-07-05
 * 
 ***************************************************************************************************/

 /// <summary>
 /// 向服务器请求道具信息的时候所需要的type
 /// </summary>
 public enum RequestItemType
{
    All = 0,            //全部道具
    DressedClothes,     //已经穿上的时装
    InBag,              //背包中道具
}

#region item type

/// <summary>
/// 道具基础类型
/// </summary>
public enum PropType
{
    None = 0,
    Weapon          = 1,        //武器
    Placeholder     = 2,        //之前代表枪械，现在直接占位符
    FashionCloth    = 3,        //时装
    Rune            = 4,        //符文
    Currency        = 5,        //货币(金币、钻石不显示在背包中)
    UseableProp     = 6,        //可使用道具
    Sundries        = 7,        //杂物
    LabyrinthProp   = 8,        //迷宫专属道具(不显示在背包中)
    HeadAvatar      = 9,        //头像框(不显示在背包中)
    IntentyProp     = 10,        //装备强化专属道具
    EvolveProp      = 11,       //装备进阶专属道具
    Debris          = 12,        //碎片
    ElementType     = 13,       //灵石种类
    BordGrade       = 14,       //无主之地积分(不显示在背包中)
    PetFood         = 15,       //宠物食物
    Pet             = 16,       //宠物
    AwakeCurrency   = 17,       //觉醒消耗货币
    SkillPoint      = 18,       //技能点
    Drawing         = 19,       //装备图纸
    NpcFashion      = 20,       //NPC时装
    DatingProp      = 21,       //约会增加心情值、体力值等道具
    WallPaper       = 22,       //壁纸
}

/// <summary>
/// 武器子类型  
/// </summary>
public enum WeaponSubType
{
    LongSword = 1,  //长剑
    Katana,         //武士刀
    Spear,          //长枪
    GiantAxe,       //大斧
    Gloves,         //拳套

    Gun = 100,      //枪械子类型
}

/// <summary>
/// 时装子类型
/// ClothGuise 装备外观是拆分出来的时装，并且装备外观会覆盖掉套装装备的模型，如果没有装备外观,则默认显示就是套装装备
/// </summary>
public enum FashionSubType
{
    None = 0,
    UpperGarment,       //上衣
    Pants,              //裤子
    Glove,              //手套
    Shoes,              //鞋子
    Hair,               //发型
    HeadDress,          //头饰 6
    TwoPieceSuit,       //两件套
    FourPieceSuit,      //四件套
    newStyle,           //新品
    limited,            //限时
    ClothGuise,         //装备外观
    HairDress,          //发饰 12
    FaceDress,          //面饰 13
    NeckDress,          //颈饰 14
    Count,
}

/// <summary>
/// 符文位置
/// </summary>
public enum RuneType
{
    One = 1,    //位置1
    Two = 2,    //位置2
    Three = 3,  //位置3
    Four = 4,   //位置4
    Five = 5,   //位置5
    Six = 6,    //位置6
}

/// <summary>
/// 货币类型
/// </summary>
public enum CurrencySubType
{
    None     = 0,
    /// <summary>
    /// 金币
    /// </summary>
    Gold     = 1,
    /// <summary>
    /// 钻石
    /// </summary>
    Diamond  = 2,
    /// <summary>
    /// 徽记
    /// </summary>
    Emblem   = 3,
    /// <summary>
    /// 冒险币
    /// </summary>
    RiskBi   = 4,
    /// <summary>
    /// 宠物召唤石
    /// </summary>
    PetSummonStone = 5,
    /// <summary>
    /// 心魂
    /// </summary>
    HeartSoul = 6,
    /// <summary>
    /// 许愿币
    /// </summary>
    WishCoin = 7,
    /// <summary>
    /// 友情点
    /// </summary>
    FriendPoint = 8,
    /// <summary>
    /// 公会贡献值
    /// </summary>
    UnionContribution = 9,

    Count
}

public enum DebrisSubType
{
    None,

    WeaponDebris      = 1,                //主武器碎片

    OffWeaponDebris   = 2,                //枪碎片

    DefendClothDebris = 3,                //防具碎片

    PetDebris         = 5,                //宠物碎片

    RuneDebrisOne     = 21,               //符文

    RuneDebrisTwo     = 22,               //符文

    RuneDebrisThree   = 23,               //符文

    RuneDebrisFour    = 24,               //符文

    RuneDebrisFive    = 25,               //符文

    RuneDebrisSix     = 26,               //符文
}

/// <summary>
/// 可使用道具子类型
/// </summary>
public enum UseablePropSubType
{
    WantedWithBlood = 1,    //染血的通缉令
    ExpCardProp,            //经验卡
    CoinBag,                //钱袋
    PropBag,                //道具包
    MonthCard,              //月卡
    QuarterCard,            //季卡
}

public enum SundriesSubType
{
    None,
    NpcgoodFeeling = 1,   //NPC好感度道具
    Bullet         = 2,   //子弹
    HelpFighting   = 3,   //助战符 
    EquipStuff     = 4,   //装备材料类道具，例如 无色晶体,升华石，寄灵水晶，还原药水等
    Ticket         = 5,   //门票类道具，例如扫荡券
    Revive         = 6,   //用于pve复活类的道具
}

public enum LabyrinthPropSubType
{
    TrapProp = 1,   //陷阱道具(只针对迷宫的层数)
    AttackProp,     //攻击道具（针对非自己的玩家）
    ProtectedProp,  //保护道具（只针对自己）
}
#endregion

/// <summary>
/// 等级对应碎片数量以及伤害百分比
/// </summary>
public enum ConsumePercentSubType
{
    D = 1,
    C,
    B,
    A,
    S,
}

/// <summary>
/// 流浪商店类型
/// </summary>
public enum ShopPos
{
    None,

    Fashion,

    Traval,

    Maze,

    Npc,

    NpcDating
}

public enum FashionType
{
    None,

    Cloth,          //衣服

    HeadDress,      //头饰

    HairDress,      //发饰

    FaceDress,      //脸饰

    NeckDress,      //颈饰

    Limited,        //折扣

}

/// <summary>
/// 品阶类型
/// </summary>
public enum QualityColor
{
    White = 1,  //白色
    Green,      //绿色
    Blue,       //蓝色
    Purple,     //紫色
    Gold,       //金色
    Orgin,      //橙色
}

/// <summary>
/// 性别枚举
/// </summary>
public enum GenderRole
{
    //女性
    Female,
    //男性
    Male,
    //全部
    All,
}

public enum NpcTypeID:int
{
    None = 0,                //无

    TravalShopNpc,       //流浪商店NPC

    BlacksmithNpc,         //铁匠NPC

    PoliceNpc,             // 治安所NPC

    WishNpc,               //许愿池NPC 
    
    HorseCarNpc,           //马车NPC

    PetSummon,              //宠物召唤Npc

    StoryNpc,             //剧情NPC

    Max
}

public enum NpcPosType
{
    None,           //无

    Head = 1,           //头

    Face = 2,           //脸

    Bar  = 3,           //胸
   
    Waist = 4,          //腰

    Hip = 5,            //臀

    Leg = 6,            //腿

    Arm = 7,            //手
}

public enum NpcLevelType
{
    Stranger,          //陌生

    Familiar,          //熟悉

    Trust,             //信任

    Like,              //喜欢
}

public enum TextForMatType
{
    PublicUIText = 200,                   //公共UI文字

    LabyrinthMailText,                    //迷宫奖励邮箱

    LolRankingMailText,                   //排位赛排名奖励邮箱

    LolDanLvMailText,                   //排位赛段位奖励邮箱邮箱

    GlobalUIText,                       //global界面提示文字

    WarehouseUiText,                    //仓库界面提示文字

    MailUIText,                          //邮箱UI用字

    TravalShopUIText,                    //流浪商店UI用字

    FashionShopUIText,                   //时装店UI用字

    RuneUIText,                          //符文UI用字

    NpcUIText,                           //NPC好感用字

    SignText,                            //运营活动界面文字
    
    AttributeUIText,                     //属性界面文字

    BorderlandUIText,                   //无主之地使用文字

    ChaseUIText,                        //追捕使用文字

    LabyrinthUIText,                    //迷宫使用文字

    EquipUIText,                        //装备界面使用文字

    LoginUIText,                        //登录界面使用文字

    FriendUIText,                        //登录界面使用文字

    MatchUIText,                         //匹配界面使用文字

    SettlementUIText,                    //结算界面使用文字

    SetUIText,                           //设置界面使用文字

    ChatUIText,                          //聊天界面使用文字

    ActiveUIText,                       //活跃度界面文字

    ForingUIText,                          //锻造界面文字

    ExChangeShopUIText,                   //迷宫兑换商店文字

    SexUIText,                              //选择性别

    NameUIText,                             //选择名字

    GiftUIText,                             //送礼界面

    AlertUIText,                           //警告窗口用字

    HomeUnlockUIText,                      //主界面解锁文字配置

    CombatUIText,                           //战斗界面文字

    BorderRankText,                         //无主之地排行榜界面文字

    NoticeUIText,                          //公告文字

    SpriteUIText = 234,                      //宠物界面文本

    PetTrainText = 235,                     //宠物历练文本

    PetSummonText = 236,                    //宠物召唤文本

    ProfessionText = 237,                     //职业选择

    PetTrainFastComplete = 238 ,            //快速完成任务选择框界面

    PetTaskText = 239,                      //宠物关卡文字
	
	SkillUIText,                              //技能UI

    PetMood     = 241,                          //宠物心情界面文字

    Guild       = 242,                         //公会文字

    AwakeStage = 243,                           //觉醒副本相关

    AwakePoint = 244,                           //觉醒加点相关

    StoryUIText = 245,                         //剧情相关文字配置

    RechargeUIText = 246,                             //充值界面

    SdkText = 247,                              //sdk用到的文字

    DecomposeUI = 250,                          //分解界面

    SoulUI = 251,                            //附灵界面

    SublimationUI   = 252,                   //升华界面

    EquipIntentyUI = 256,                   //装备强化界面(前面几个ID被其他功能占用)

    EquipEvolveUI = 257,                    //装备进阶界面

    MoppingUpUI     = 258,                  //扫荡界面

    AssistUI        = 259,                  //助战界面UI

    NpcAwakeUI      = 261,                  //npc心魂界面UI

    NpcGaidenUI     = 262,                  //npc外传界面UI

    ApplyFriendUI     = 263,                  //好友申请界面

    AllAttributeText = 299,                  //属性分类文字汇总，写死ID

    NpcDating       = 620,                  //npc约会主界面

    NpcDatingRest   = 621,                  //npc约会餐厅界面

    NpcDatingMission = 622,                 //npc约会任务列表

    NpcDatingReview = 623,                  //npc约会对话回顾界面

    NpcDatingSettlement = 624,              //npc约会结算界面

    NpcDatingDivination = 625,              //npc约会占卜屋界面

    SelectRoleUI     = 626,                  //角色选择界面

    NpcDatingGift   = 627,                  //npc约会送礼界面

    SelectDatingNpc = 628,                  //选择约会Npc界面

    FactionSignUI = 279,                    //阵营战报名界面

    FactionBattleUI = 280,                  //阵营战界面

    FactionSettlementUI = 281,              //阵营战结算界面
}

public enum UiPanelAnimation
{
    AttributeAnimation,                   //属性退出动画

    ShiZhuangAnimation,                   //时装店退出动画

    SignAnimation,                        //签到退出动画
}

/// <summary>
/// 装备界面的显示类型
/// </summary>
public enum EquipType
{
    None = 0,       

    Weapon,         //武器

    Gun,            //枪械

    Cloth,          //制服

    Hair,           //头发

    HeadDress,      //头饰

    Guise,          //时装

    HairDress,      //发饰

    FaceDress,      //面饰

    NeckDress,      //颈饰
}

public enum OpenWhichPvP
{
    None,              //无

    FreePvP,           //快速匹配

    LolPvP,            //排位赛

    FriendPvP,
}

public enum PreviewEquipType
{
    None,
    Intentify,    //强化
    Evolve,       //进阶
    Enchant,      //附魔
}

public enum BagCollectionType
{
    Default = 0,        //默认的储存器，避免丢失道具
    CurrentDress,       //当前玩家的穿戴信息（武器，副武器，穿戴）
    BagDress,           //当前玩家背包的普通装备信息（武器，副武器，穿戴）
    BagProps,           //当前玩家背包的道具（显示在仓库中）
    HideProps,          //当前玩家的道具，不显示在仓库中的
    RuneProps,          //符文
    Count,
}

public enum SkillType
{
    None,
    Click,             //点击
    Foward,            //前划
    Up,                //上划
    Down,              //下滑
    DoubleFoward,      //冲刺
    Special,           //特殊
    Single,            //专精
}
public enum OpenWhichChat
{
    None,              //无

    WorldChat,           //世界聊天

    SysChat,            //系统聊天

    UnionChat,          //公会聊天

    TeamChat,           //队伍聊天

    FactionChat,        //阵营聊天
}
public enum UnionNoticeType
{
    Add,                //加入
    Exit,               //退出
    TitlePresident,     //职位变为会长
    TitleVicePresident, //职位变为副会长
    TitleNormal,        //职位变为普通成员
    Remove,             //移除
    NoticeChange,       //公告修改
    LevelUp,            //等级上升
    LevelDown,          //等级下降
    BossOpen,           //boss开启
    BossClose,          //boss 关闭
    PopulaChange,       //人气值消耗

}
public enum OpenFriendType
{
    None,              //无

    Normal,            //系统聊天

    World,           //从世界打开
    
    Union,          //从公会打开
}

public enum WelfareType
{
    Null = 0,

    FirstFlush = 1,              //首冲 人民币  模板1

    Cumulative = 2,              //累计 充值人民币 模板3

    Continuous = 3,              //连续 充值天数 模板2

    DailyFlush = 4,              //每日 充值的人民币 模板2

    Sign = 5,                    //签到 

    DailySign = 6,               //每日登陆游戏 天数  模板1

    ContDaily = 7,               //连续登陆游戏 天数 模板4

    CumulDaily = 8,              //累计登陆游戏 天数  模板4

    DiamondFlush = 9,            //充值指定钻石  模板4

    DiamondConsum = 10,          //消耗指定钻石  模板4

    Level = 11,                  //达到对应的等级  模板4

    StrengConsum = 12,           //消耗指定体力 模板1

    SpecifiedStreng = 13,        //每消耗指定体力即可领取 模板一

    WaitTime = 14,               //达到固定时间领取奖励

    MatchStreetPvP = 15,         //匹配街头斗技

    VictoryTimes = 16,           //通关次数

    DailyNewSign = 17,           //萌新登陆游戏 天数  模板4

    ActiveNewPuzzle = 18,        //新手拼图活动 模板5
    
    DropRateUp = 30,             //掉率提升

    ChargeDailySale = 51,        //充值 日折扣

    ChargeGrowth = 52,           //充值 基金

    ChargeMonthCard = 53,        //充值 月卡

    ChargeSeasonCard = 54,       //充值 季卡

    ChargeGift = 55,             //充值 礼包售卖

    ChargeWish = 56,             //充值 祈愿

    ChargeSunmon = 57,           //充值 召唤

    ChargeWeekSale = 58,         //充值 周折扣

}

public enum WelfareReachType
{
    Null = 0,

    ReMoneyCum,            //充值人民币累计

    ReMoneyCon,            //充值人民币连续

    ReMoneyDaily,          //充值人民币每日

    ReDiamondCum,          //充值钻石累计

    CostDiamindAcc,        //花费钻石累计

    CostFatigueAcc,        //消耗体力累计

    CostFatigueDaily,      //消耗体力每日

    LevelUp,               //到达指定等级

    OnlineTime,            //累计在线时间

    StageOver,             //通过指定关卡

    PVPTimes,              //参加pvp次数

    LoInDaily,             //当日登陆

    LoInCon,               //连续登陆

    LoInAcc,               //累计登陆

    CostIconThis,          //本次活动花费金币数

    LoInTest,              //登陆领取之前所有奖励

    BlowUp,                //爆率提升调整组

    CollectList,           //收集一组ID

    StrengEquip,           //强化X件装备到X级

    AdvanceEquip,          //进阶X件装备到X级

    RuneLevel,             //拥有N个X级灵珀

    RuneStar,              //拥有N个X星灵珀

    AddFriend,             //成功添加N个好友

    PetLevel,              //拥有N个X级精灵

    JoinUnion,             //加入公会

    GrowthEquip,           //进行N次武器入魂

    GrowthLevel,           //入魂达到X级

    PetTask,               //完成N次精灵历练

    StageTeam,             //组队完成N次组队副本

    NPCLevel,              //指定npc好感

    CooperateTask,         //完成N个赏金任务

    SoulEquip,             //进行N次器灵

    LevelEquip,            //拥有N件X级装备

    AwakeSkill,            //升级N次心魂

    StageType,             //通关指定类型关卡N次

    PVPWinTimes,           //参加pvp胜利次数

    Charge,                //充值相关
}

public enum WareType
{
    None = 0,                       //不显示 默认

    Prop = 1,                       //道具

    Equip = 2,                      //装备

    Rune = 3,                       //灵珀

    Material = 4,                   //材料

    Debris = 5,                     //碎片

    Count,
}

/// <summary>
/// 装备可以操作的权限配置
/// </summary>
public enum EnumEquipOperationAuthority
{
    None = 0,
    Intenty = 1,                //装备强化
    Evolve = 2,                 //装备进阶（前置条件是装备强化到10的整数倍）
    InSoul = 4,                 //装备入魂（只有武器且不是枪械才能操作）
    DegreeElevation = 8,       //装备升阶（只有武器且不是枪械才能操作）
    Sublimation = 16,           //装备升华 (配置表中单独字段)
    SoulSprite = 32,            //器灵(配置表中单独字段)
}

public enum SysMesType
{
    None = 0,                       //不显示 默认

    Pvp = 3,                        //皇家斗技

    BordLand = 4,                   //深渊隘口开关

    BordRefresh = 5,                //深渊隘口出现boss

    Repair = 6,                     //维护通知

    Wish = 11,                      //抽卡

    Union = 12,                     //公会

    Team = 13,                      //组队
}

public enum SwitchType
{
    Fatigue = 1,                   //体力回满

    SkillPoint,                    //技能点回满

    UnionBoss,                     //联合讨伐回满

    Labyrinth,                     //迷宫开启

    RoyalPvp,                      //皇家斗技

    SystemPao,                     //跑马灯
}

