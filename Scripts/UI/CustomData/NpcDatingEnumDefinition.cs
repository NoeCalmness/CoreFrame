/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 主要用于标识所有约会的枚举
 * 
 * Author:   H
 * Version:  0.1
 * Created:  2019-05-06
 * 
 ***************************************************************************************************/


/// <summary>
/// 选项答案类型
/// </summary>
public enum EnumAnswerType
{
    /// <summary>
    /// 坏答案
    /// </summary>
    Bad = 0,

    /// <summary>
    /// 普通答案
    /// </summary>
    Normal = 1,

    /// <summary>
    /// 好答案
    /// </summary>
    Good = 2,

    /// <summary>
    /// 图书馆错误答案
    /// </summary>
    Library_Wrong = 3,

    /// <summary>
    /// 图书馆正确答案
    /// </summary>
    Library_Right = 4,

    /// <summary>
    /// 水晶球占卜
    /// </summary>
    Divination_CrystalBall = 5,

    /// <summary>
    /// 抽签占卜
    /// </summary>
    Divination_ImperialGuard = 6,

    None
}

public enum DatingEventType
{
    /// <summary>
    /// 约会
    /// </summary>
    Dating,
    Other
}

/// <summary>占卜屋 </summary>
public enum EnumDivinationType
{
    None,
    /// <summary>水晶球</summary>
    CrystalDevine,
    /// <summary>御神签</summary>
    LotDevine
}

/// <summary>礼物类型 </summary>
public enum EnumDatingGiftType
{
    UnUsed,
    /// <summary>心情礼物</summary>
    Mood,
    /// <summary>体力礼物</summary>
    BodyPower
}

/// <summary>
/// 约会主界面与约会建筑场景事件交互类型
/// </summary>
public enum EnumDatingNotifyType
{
    RefreshRedDot,
    ClickBuild,
    ClickBuildDown,
    ClickBuildUp,
    OnEndDrag,
}

/// <summary>
/// 约会主界面场景各个物体类型
/// </summary>
public enum EnumDatingMapObjectType
{
    Other,
    /// <summary>功能建筑</summary>
    Build,
}

public enum DatingGmType
{
    UnUsed,
    /// <summary>重置约会</summary>
    ResetDating,
    /// <summary>踢出图书馆</summary>
    KickOutLib,
    /// <summary>退出场景</summary>
    QuitScene,
    /// <summary>结束约会</summary>
    DatingOver,
    /// <summary>设置占卜结果 </summary>
    SetDivinationResult,
    /// <summary>执行约会事件</summary>
    DoDatingEvent,
    /// <summary>打开问题面板</summary>
    OpenAnswer,
    /// <summary>设置随机值</summary>
    SetRandomValue,
    /// <summary>强制退出约会场景</summary>
    ForceQuitScene,
}






#region 新版约会事件相关枚举

/// <summary>
/// 约会事件类型
/// </summary>
public enum NewDatingEventType
{
    /// <summary>
    /// 约会
    /// </summary>
    Dating,
    /// <summary>
    /// 羁绊
    /// </summary>
    Fetter,
    /// <summary>
    /// 结算
    /// </summary>
    Settlement,
}

/// <summary>
/// 约会事件表条件类型
/// </summary>
public enum DatingEventConditionType
{
    None,
    /// <summary>Npc邀约</summary>
    NpcInvite,
    /// <summary>进入约会场景</summary>
    EnterDatingScene,
    /// <summary>Npc心情值</summary>
    MoodValue,
    /// <summary>Npc好感度(羁绊值)</summary>
    FettersValue,
    /// <summary>Npc体力值</summary>
    Energy,
    /// <summary>一段剧情对话结束</summary>
    StoryEnd,
    /// <summary>根据之前选择answer的id作为条件筛选</summary>
    ChooseAnswerItemId,
    /// <summary>根据之前选择answer的id类型作为条件筛选</summary>
    ChooseAnswerType,
    /// <summary>占卜类型条件</summary>
    DivinationType,
    /// <summary>占卜结果</summary>
    DivinationResult,
    /// <summary>是否需要结算</summary>
    SettlementState,
    /// <summary>约会结算结果</summary>
    SettlementResult,
    /// <summary>检测是否需要送礼</summary>
    GiveGiftState,
    /// <summary>结算流程结束，关闭结算界面</summary>
    SettlementEnd,
    /// <summary>检测筛选随机数</summary>
    RandomNumber,
    /// <summary>关卡挑战结果</summary>
    ChaseFinish,
    /// <summary>打开问题选项窗口</summary>
    OpenAnswerWindow,
    /// <summary>餐厅菜单菜品</summary>
    OrderItem,
    Count,
}

public enum DatingEventBehaviourType
{
    None,
    /// <summary>打开剧情</summary>
    Story,
    /// <summary>执行约会事件</summary>
    DatingEvent,
    /// <summary>打开窗口</summary>
    OpenWindow,
    /// <summary>创建任务</summary>
    Mission,
    /// <summary>通知服务器完成任务</summary>
    NotifyFinishMission,
    /// <summary>Pve挑战</summary>
    Chase,
    /// <summary>过渡特效</summary>
    TranstionEffect,
    /// <summary>打开问题选项窗口</summary>
    OpenAnswerWindow,
    /// <summary>获取结算状态</summary>
    GetSettlementState,
    /// <summary>開始结算</summary>
    StartSettlement,
    /// <summary>获取结算后送礼状态</summary>
    GetSettlementGiftState,
    /// <summary>开始弹出送礼界面</summary>
    StartSettlementGift,
    /// <summary>产生随机数</summary>
    CreateRandomNumber,
    /// <summary>所有剧情结束，即整个流程结束</summary>
    AllStoryEnd,
    /// <summary>退出约会场景</summary>
    QuitScene,
    /// <summary>约会中达成某个事件</summary>
    ReachEvent,
}

public enum EnumNPCDatingSceneType
{
    None,

    /// <summary>茶餐厅</summary>
    TeaRestaurant = 1,

    /// <summary>海滩</summary>
    Beach = 2,

    /// <summary>教堂</summary>
    Church = 3,

    /// <summary>占卜屋 </summary>
    DivinationHouse = 4,

    /// <summary>花园</summary>
    Garden = 5,

    /// <summary>图书馆</summary>
    Library = 6,

    /// <summary>约会大厅</summary>
    DatingHall,

    /// <summary>游戏大厅</summary>
    GameHall,

    /// <summary>其他场景</summary>
    Other

}


public enum EnumDatingReviewType
{
    None,
    Story,
    Answer
}

#endregion
