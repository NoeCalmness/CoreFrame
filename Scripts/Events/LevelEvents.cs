/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * All level events
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-08-16
 * 
 ***************************************************************************************************/

public static class LevelEvents
{
    /// <summary>
    /// sender : Level          argument: Event[string] 开始特效名称
    /// trigger: 战斗关卡进入准备阶段时
    /// </summary>
    public const string PREPARE           = "OnLevelPrepare";
    /// <summary>
    /// sender : Level          argument: Event
    /// trigger: 战斗开始时
    /// </summary>
    public const string START_FIGHT       = "OnLevelStartFight";
    /// <summary>
    /// sender : Level          argument: Event[int] 当前时间（秒）如果小于 0 表示当前关卡无时间限制
    /// trigger: 战斗状态下，每秒钟触发一次
    /// </summary>
    public const string BATTLE_TIMER      = "OnLevelBattleTimer";
    /// <summary>
    /// sender : Level          argument: Event[bool] 是否处于拍照模式
    /// trigger: 当进入/退出拍照模式时触发
    /// </summary>
    public const string PHOTO_MODE_STATE  = "OnLevelPhotoModeState";
    /// <summary>
    /// sender : Level          argument: Event[bool] 当前是否处于对话状态
    /// trigger: 场景角色进入或退出对话状态时
    /// </summary>
    public const string DIALOG_STATE      = "OnLevelDialogState";
    /// <summary>
    /// sender : Level          argument: Event[Creature] 创建的助战角色
    /// trigger: 场景助战角色创建时
    /// </summary>
    public const string CREATE_ASSIST     = "OnLevelCreateAssist";
    /// <summary>
    /// sender : Level          argument: Event[bool] 进入或者退出
    /// trigger: 进入观战或退出观战状态
    /// </summary>
    public const string BATTLE_ENTER_WATCH_STATE = "OnBattleEnterWatchState";

    public const string BATTLE_END = "OnLevelBattleEnd";
}
