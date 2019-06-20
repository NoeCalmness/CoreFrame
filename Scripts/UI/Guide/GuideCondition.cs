using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnumGuideContitionType
{
    None,

    /// <summary>
    /// 打开某个窗口(除打开主界面之外的窗口)
    /// 参数：WindowName
    /// </summary>
    OpenWindow,

    /// <summary>
    /// 进入关卡
    /// 参数: StageId
    /// </summary>
    EnterStage,

    /// <summary>
    /// 某段引导完成
    /// 参数：GuideId
    /// </summary>
    GuideEnd,

    /// <summary>
    /// 某段对话完成
    /// 参数：StoryId
    /// </summary>
    StoryEnd,

    /// <summary>
    /// 玩家等级
    /// 参数：Level
    /// </summary>
    PlayerLevel,

    /// <summary>
    /// 符文到达满级
    /// </summary>
    RuneMaxLevel,

    /// <summary>
    /// 获得指定道具
    /// 参数:PropId
    /// </summary>
    GetProp,

    /// <summary>
    /// 迷宫活动开放 
    /// </summary>
    OpenLabyrinth,

    /// <summary>
    /// 无主之地活动开放
    /// </summary>
    OpenBorderland,

    /// <summary>
    /// 任务是否开放
    /// 参数：TaskId，Finish
    /// </summary>
    TaskFinish,

    /// <summary>
    /// 任务是否已经挑战
    /// 参数:TaskId,Chanllenge
    /// </summary>
    TaskChanllenge,

    /// <summary>
    /// pvp挑战次数
    /// 参数:Chanllenge
    /// </summary>
    PVPChanllenge,

    /// <summary>
    /// 等待特殊的界面动画结束
    /// 1.许愿结果页面关闭
    /// </summary>
    SpecialTweenEnd,

    /// <summary>
    /// 进入训练场
    /// </summary>
    EnterTrain,

    /// <summary>
    /// 日常任务完成
    /// </summary>
    DailyFinish,

    /// <summary>
    /// 默认的操作引导完成
    /// </summary>
    DefalutOperateGuideEnd,
    /// <summary>
    /// NPC 约会
    /// </summary>
    NpcDating,
}

public class BaseGuideCondition
{
    public EnumGuideContitionType type { get; protected set; }

    //条件是否允许多个同时存在
    public virtual bool allowMultiple { get { return false; } }

    protected int[] intParames;
    public string strParames { get; protected set; }

    public int GetIntParames(int index)
    {
        return intParames.GetValue<int>(index);
    }

    public void SetIntParames(int index,int value)
    {
        if (intParames == null || index >= intParames.Length) return;
        intParames[index] = value;
    }

    public virtual bool IsSameCondition(BaseGuideCondition condition)
    {
        if (type == condition.type && IsSameIntArray(condition.intParames) 
            && strParames.Equals(condition.strParames)) return true;
        return false;
    }

    public bool IsSameIntArray(int[] parames)
    {
        if ((intParames == null || intParames.Length == 0) && (parames == null || parames.Length == 0)) return true;

        if(intParames != null && parames != null && intParames.Length == parames.Length)
        {
            for (int i = 0; i < intParames.Length; i++)
            {
                if (intParames[i] != parames[i]) return false;
            }
            return true;
        }

        return false;
    }

    public virtual bool IsSameConditionType(EnumGuideContitionType targetType)
    {
        return type == targetType;
    }

    public new virtual string ToString()
    {
        return Util.Format("condition type is {0}",type);
    }

    #region construction

    public BaseGuideCondition(EnumGuideContitionType t)
    {
        type = t;
        intParames = new int[0];
        strParames = string.Empty;
    }

    public BaseGuideCondition(GuideInfo.GuideConfigCondition cc)
    {
        type = cc.type;
        if(cc.intParames != null)
        {
            intParames = new int[cc.intParames.Length];
            Array.Copy(cc.intParames, intParames, cc.intParames.Length);
        }
        strParames = cc.strParames;
    }

    public BaseGuideCondition(EnumGuideContitionType t,params int[] parames)
    {
        type = t;
        List<int> list = new List<int>();
        if (parames != null) list.AddRange(parames);
        intParames = list.ToArray();
        strParames = string.Empty;
    }

    public BaseGuideCondition(EnumGuideContitionType t,string parames)
    {
        type = t;
        intParames = new int[0];
        strParames = parames;
    }
    #endregion
}

public class OpenWindowCondition : BaseGuideCondition
{
    public string openWindowName { get { return strParames; } }
    bool isNotFullWindow = false;
    public OpenWindowCondition(string windowName) : base(EnumGuideContitionType.OpenWindow,windowName) { }
    public OpenWindowCondition(string windowName, bool allowMul) : base(EnumGuideContitionType.OpenWindow, windowName)
    {
#if GUIDE_LOG
Logger.LogError("This Window: {0} When is  Open Need Remove GuideCacheConditions By Type :{1} ", openWindowName, !isNotFullWindow);
#endif
        isNotFullWindow = allowMul;
        
    }
    public override bool allowMultiple
    {
        get { return isNotFullWindow; }
    }

    public override string ToString()
    {
        return Util.Format("{0},window name is {1}",base.ToString(),openWindowName);
    }
}

public class EnterStageContition : BaseGuideCondition
{
    public int stageId { get { return GetIntParames(0); } }

    public EnterStageContition(int id) : base(EnumGuideContitionType.EnterStage,id) { }

    public override string ToString()
    {
        return Util.Format("{0},stage id is {1}", base.ToString(), stageId);
    }
}

public class GuideEndContition : BaseGuideCondition
{
    public int guideId { get { return GetIntParames(0); } }

    public GuideEndContition(int id) : base(EnumGuideContitionType.GuideEnd, id) { }
    public override bool allowMultiple { get { return true; } }

    public override string ToString()
    {
        return Util.Format("{0},the ended guide id is {1}", base.ToString(), guideId);
    }
}

public class StoryEndContition : BaseGuideCondition
{
    public int storyId { get { return GetIntParames(0); } }

    public StoryEndContition(int id) : base(EnumGuideContitionType.StoryEnd,id) { }

    public override string ToString()
    {
        return Util.Format("{0},the ended story id is {1}", base.ToString(), storyId);
    }
}

public class PlayerLevelContition : BaseGuideCondition
{
    public int level
    {
        get { return GetIntParames(0); }
        set
        {
            if (intParames == null) intParames = new int[1];
            intParames[0] = value;
        }
    }

    public PlayerLevelContition(int id) : base(EnumGuideContitionType.PlayerLevel,id) { }

    public override bool IsSameCondition(BaseGuideCondition condition)
    {
        if (type != condition.type) return false;

        int needLevel = condition.GetIntParames(0);
        if (type == condition.type && level >= needLevel) return true;
        return false;
    }

    public override string ToString()
    {
        return Util.Format("{0},player level is change to {1}", base.ToString(), level);
    }
}

public class RuneMaxLevelContition : BaseGuideCondition
{
    public RuneMaxLevelContition() : base(EnumGuideContitionType.RuneMaxLevel) { }
}

public class GetPropContition : BaseGuideCondition
{
    public int propId { get { return GetIntParames(0); } }
    public GetPropContition(int id) : base(EnumGuideContitionType.GetProp, id) { }

    public override string ToString()
    {
        return Util.Format("{0},new get prop is {1}", base.ToString(), propId);
    }
}

public class OpenLabyrinthContition : BaseGuideCondition
{
    public OpenLabyrinthContition() : base(EnumGuideContitionType.OpenLabyrinth) { }
}

public class OpenBordlandContition : BaseGuideCondition
{
    public OpenBordlandContition() : base(EnumGuideContitionType.OpenBorderland) { }
}

public class TaskFinishCondition : BaseGuideCondition
{
    public int taskId { get { return GetIntParames(0); } }
    public bool finish
    {
        get { return GetIntParames(1) > 0; }
        set { intParames[1] = value ? 1 : 0; }
    }

    public override bool allowMultiple { get { return true; } }

    public TaskFinishCondition(int taskId,bool finish) : base(EnumGuideContitionType.TaskFinish,taskId,finish ? 1 : 0) { }

    public override string ToString()
    {
        return Util.Format("{0},task is {1}, finish is {2}", base.ToString(), taskId,finish);
    }
}

public class TaskChanllengeCondition : BaseGuideCondition
{
    public int taskId { get { return GetIntParames(0); } }
    public bool chanllenge
    {
        get { return GetIntParames(1) > 0; }
        set { intParames[1] = value ? 1 : 0; }
    }

    public override bool allowMultiple { get { return true; } }

    public TaskChanllengeCondition(int taskId, bool chanllenge) : base(EnumGuideContitionType.TaskChanllenge, taskId, chanllenge ? 1 : 0) { }

    public override string ToString()
    {
        return Util.Format("{0},task is {1}, chanllenge is {2}", base.ToString(), taskId, chanllenge);
    }
}

public class PVPChanllengeCondition : BaseGuideCondition
{
    public bool chanllenge
    {
        get { return GetIntParames(0) > 0; }
        set { intParames[0] = value ? 1 : 0; }
    }

    public PVPChanllengeCondition(bool chanllenge) : base(EnumGuideContitionType.PVPChanllenge, chanllenge ? 1 : 0) { }

    public override string ToString()
    {
        return Util.Format("{0},pvp chanllenge state is {1}", base.ToString(), chanllenge);
    }
}

public class SpecialTweenEndCondition : BaseGuideCondition
{
    public SpecialTweenEndCondition() : base(EnumGuideContitionType.SpecialTweenEnd) { }
}

public class EnterTrainCondition : BaseGuideCondition
{
    public EnterTrainCondition() : base(EnumGuideContitionType.EnterTrain) { }
}

public class DailyFinishCondition : BaseGuideCondition
{
    public DailyFinishCondition() : base(EnumGuideContitionType.DailyFinish) { }
}

public class DefalutOperateGuideEndCondition : BaseGuideCondition
{
    public DefalutOperateGuideEndCondition() : base(EnumGuideContitionType.DefalutOperateGuideEnd) { }
}

public class NpcDatingCondition:BaseGuideCondition
{
    

    public int taskId { get { return GetIntParames(0); } }
    public bool finish
    {
        get { return GetIntParames(1) > 0; }
        set { intParames[1] = value ? 1 : 0; }
    }
    public NpcDatingCondition(int taskid,bool finish) : base(EnumGuideContitionType.NpcDating,taskid, finish ? 1 : 0) { }

    public override bool IsSameCondition(BaseGuideCondition condition)
    {
        if (type != condition.type) return false;
        return base.IsSameCondition(condition);
    }
}


