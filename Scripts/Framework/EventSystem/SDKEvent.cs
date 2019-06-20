/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * SDKEvent used for SDKManager.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-04-23
 * 
 ***************************************************************************************************/

public class SDKEvent : Event_
{
    #region push
    /// <summary>
    /// 推送添加别名,选择角色,进入游戏  argument:arg1:ulong类型的roleId
    /// </summary>
    public const string SELECT_ROLE = "SelectRole";

    /// <summary>
    /// 通知tag操作(工会boss不通过此事件) argument:arg1:unit类型的 0-删除 1-添加 arg2:byte类型开关id 1-体力 2-技能点 4-迷宫开启 5-皇家斗技 6-跑马灯
    /// </summary>
    public const string TAG = "Tag";

    /// <summary>
    /// 推送本地通知 argument:enum类型的 SwitchType.SkillPoint和SwitchType.Fatigue
    /// </summary>
    public const string LOCAL_NOTIFY = "LocalNotify";

    /// <summary>
    /// 推送工会boss的tag argument:arg1:ulong类型工会ID arg2:uint类型 0-删除 1-添加
    /// </summary>
    public const string UNION_CHANGE = "UnionChange";
    #endregion

    public const string GLOBAL = "GLOBAL_SDK_EVENT";
    public static readonly System.Type type = typeof(SDKEvent);
    public string eventName = string.Empty;

    public static SDKEvent PopSdk(string name, object _param1 = null, object _param2 = null, object _param3 = null, object _param4 = null)
    {
        var e = Pop(type) as SDKEvent;
        e.eventName = name;
        e.param1 = _param1;
        e.param2 = _param2;
        e.param3 = _param3;
        e.param4 = _param4;
        return e;
    }

    public override void Reset()
    {
        eventName = string.Empty;
        base.Reset();
    }
}
