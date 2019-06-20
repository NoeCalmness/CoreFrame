using System;
using System.Text;

public class SSceneConditionBase
{
    public SceneEventInfo.SceneConditionType type { get; protected set; }

    //条件是否允许多个同时存在
    public virtual bool allowMultiple { get { return false; } }

    protected int[] intParames;
    public string strParam { get; protected set; }

    public int conditionId { get; protected set; }

    public bool isEnable { get; protected set; }

    public int GetIntParames(int index)
    {
        return intParames.GetValue<int>(index);
    }

    public void SetIntParames(int index, int value)
    {
        if (intParames == null || index >= intParames.Length) return;
        intParames[index] = value;
    }

    /// <summary>
    /// 是否是相同条件
    /// </summary>
    /// <param name="condition">基于配置表创建的 SSceneConditionBase </param>
    /// <returns></returns>
    public virtual bool IsSameCondition(SSceneConditionBase condition)
    {
        if (condition == null) return false;
         
        if (type == condition.type && IsSameIntArray(condition.intParames) && IsSameStrParam(condition.strParam)) return true;
        return false;
    }

    public bool IsSameStrParam(string str)
    {
        if (string.IsNullOrEmpty(strParam) && string.IsNullOrEmpty(str)) return true;

        return strParam.Equals(str);
    }

    public bool IsSameIntArray(int[] parames)
    {
        if ((intParames == null || intParames.Length == 0) && (parames == null || parames.Length == 0)) return true;

        if (intParames != null && parames != null && intParames.Length == parames.Length)
        {
            for (int i = 0; i < intParames.Length; i++)
            {
                if (intParames[i] != parames[i]) return false;
            }
            return true;
        }

        return false;
    }

    public new string ToString()
    {
        return Util.Format("condition type is {0},parames : {1}", type, GetParamesToString());
    }

    public virtual string GetParamesToString()
    {
        StringBuilder s = new StringBuilder();
        if (intParames == null) s.Append("null");
        else if (intParames.Length == 0) s.Append("len is zero");
        else
        {
            foreach (var item in intParames)
            {
                s.AppendFormat("{0}\t",item);
            }
        }

        s.AppendFormat("str parames is {0}",string.IsNullOrEmpty(strParam) ? "empty" : strParam);

        return s.ToString();
    }


    #region construction

    public SSceneConditionBase(SceneEventInfo.SceneCondition c)
    {
        type = c.sceneEventType;
        if (c.parameters != null)
        {
            intParames = new int[c.parameters.Length];
            Array.Copy(c.parameters, intParames, c.parameters.Length);
        }
        strParam = c.strParam;
        conditionId = c.conditionId;
        isEnable = true;
    }

    public void EnableCondition(bool bEnable)
    {
        isEnable = bEnable;
    }

    public SSceneConditionBase(SceneEventInfo.SceneConditionType t, params int[] parames)
    {
        type = t;
        if (parames == null) intParames = new int[0];
        else intParames = (int[])parames.Clone();
        strParam = string.Empty;
    }

    public SSceneConditionBase(SceneEventInfo.SceneConditionType t, string str, params int[] parames)
    {
        type = t;
        if (parames == null) intParames = new int[0];
        else intParames = (int[])parames.Clone();
        strParam = str;
    }

    #endregion
}

public sealed class SEnterSceneConditon : SSceneConditionBase
{
    public SEnterSceneConditon() : base(SceneEventInfo.SceneConditionType.EnterScene) { }
}

public sealed class SCountDownEndConditon : SSceneConditionBase
{
    public SCountDownEndConditon(int timerId) : base(SceneEventInfo.SceneConditionType.CountDownEnd, timerId) { }

    #region properties

    public int timerId { get { return GetIntParames(0); } }

    #endregion

    public override string GetParamesToString()
    {
        return Util.Format("timerId = {0}",timerId);
    }
}

public sealed class SMonsterDeathConditon : SSceneConditionBase
{
    public SMonsterDeathConditon(int mon,int group, int amount = 1, int autoIncrease = 0) : base(SceneEventInfo.SceneConditionType.MonsterDeath, mon,group,amount) { }

    #region properties

    public int monsterId { get { return GetIntParames(0); } }
    public int group { get { return GetIntParames(1); } }
    public int amount { get { return GetIntParames(2); } }
    #endregion

    public void ResetData(int mon, int group, int amoun)
    {
        if(intParames == null || intParames.Length < 3) intParames = new int[3];

        intParames[0] = mon;
        intParames[1] = group;
        intParames[2] = amoun;
    }

    public void IncreaseAomunt(int increase)
    {
        intParames[2] += increase;
    }
    public override string GetParamesToString()
    {
        return Util.Format("monId = {0},group = {1},Amount = {2}", monsterId,group,amount);
    }
}

public sealed class SStageFirstTimeConditon : SSceneConditionBase
{
    public SStageFirstTimeConditon(bool first) : base(SceneEventInfo.SceneConditionType.StageFirstTime, first ? 0 : 1)
    {
    }
}

public sealed class SStoryDialogueEndConditon : SSceneConditionBase
{
    public SStoryDialogueEndConditon(int plotId) : base(SceneEventInfo.SceneConditionType.StoryDialogueEnd, plotId) { }

    #region properties

    public int plotId { get { return GetIntParames(0); } }

    #endregion

    public override string GetParamesToString()
    {
        return Util.Format("plotId = {0}", plotId);
    }
}

public sealed class SCounterNumberConditon : SSceneConditionBase
{
    public SCounterNumberConditon(int id, int numberChange) : base(SceneEventInfo.SceneConditionType.CounterNumber, id, numberChange) { }

    #region properties

    public int CounterId { get { return GetIntParames(0); } }
    public int numberChange { get { return GetIntParames(1); } }
    #endregion

    public override string GetParamesToString()
    {
        return Util.Format("CounterId = {0},numberChange = {1}", CounterId, numberChange);
    }
}

public sealed class SMonsterLeaveEndConditon : SSceneConditionBase
{
    public SMonsterLeaveEndConditon(int mon, int group) : base(SceneEventInfo.SceneConditionType.MonsterLeaveEnd, mon, group) { }

    #region properties

    public int monsterId { get { return GetIntParames(0); } }
    public int group { get { return GetIntParames(1); } }
    #endregion

    public override string GetParamesToString()
    {
        return Util.Format("monId = {0},group = {1}", monsterId, group);
    }
}

public sealed class SMonsterHPLessConditon : SSceneConditionBase
{
    public SMonsterHPLessConditon(int mon, int group, int hp) : base(SceneEventInfo.SceneConditionType.MonsterHPLess, mon, group, hp) { }

    #region properties

    public int monsterId { get { return GetIntParames(0); } }
    public int group { get { return GetIntParames(1); } }
    public int hp { get { return GetIntParames(2); } set { SetIntParames(2, value); } }
    #endregion

    public override string GetParamesToString()
    {
        return Util.Format("monId = {0},group = {1},hp is = {2}", monsterId, group, hp);
    }

    public override bool IsSameCondition(SSceneConditionBase condition)
    {
        if (condition == null) return false;

        if(type == condition.type && monsterId == condition.GetIntParames(0) && group == condition.GetIntParames(1))
        {
            return hp < condition.GetIntParames(2);
        }
        return false;
    }
}

public sealed class SPlayerHPLessConditon : SSceneConditionBase
{
    public SPlayerHPLessConditon(int hp) : base(SceneEventInfo.SceneConditionType.PlayerHPLess,  hp) { }

    #region properties

    public int hp { get { return GetIntParames(0); } set { SetIntParames(0, value); } }
    #endregion

    public override string GetParamesToString()
    {
        return Util.Format("hp is = {0}",hp);
    }

    public override bool IsSameCondition(SSceneConditionBase condition)
    {
        if (condition == null) return false;

        if (type == condition.type)
        {
            return hp < condition.GetIntParames(0);
        }
        return false;
    }
}

public sealed class SHitTimesConditon : SSceneConditionBase
{
    public SHitTimesConditon(int times, int logicID) : base(SceneEventInfo.SceneConditionType.HitTimes, times, logicID) { }

    #region properties

    public int times { get { return GetIntParames(0); } set { SetIntParames(0, value); } }

    public int logicID { get { return GetIntParames(1); } set { SetIntParames(1, value); } }
    #endregion

    public override string GetParamesToString()
    {
        return Util.Format("times is = {0},  logicID is {1}", times, logicID);
    }

    public override bool IsSameCondition(SSceneConditionBase condition)
    {
        if (condition == null) return false;
        if (type == condition.type &&  condition.IsSameIntArray(intParames))  
        {
            return true;
        }
        return false;
    }
}

public sealed class SGuideEndConditon : SSceneConditionBase
{
    public SGuideEndConditon(int id) : base(SceneEventInfo.SceneConditionType.GuideEnd, id) { }

    #region properties

    public int guideId { get { return GetIntParames(0); } }
    #endregion

    public override string GetParamesToString()
    {
        return Util.Format("guideId = {0}", guideId);
    }
}

public sealed class SBossComingEndConditon : SSceneConditionBase
{
    public SBossComingEndConditon(int id) : base(SceneEventInfo.SceneConditionType.BossComingEnd, id) { }

    #region properties

    public int bossTimerId { get { return GetIntParames(0); } }
    #endregion

    public override string GetParamesToString()
    {
        return Util.Format("bossTimerId = {0}", bossTimerId);
    }
}

public sealed class SPlayerVocationConditon : SSceneConditionBase
{
    public SPlayerVocationConditon(int vocation) : base(SceneEventInfo.SceneConditionType.PlayerVocation, vocation) { }

    #region properties

    public int bossTimerId { get { return GetIntParames(0); } }
    #endregion

    public override string GetParamesToString()
    {
        return Util.Format("bossTimerId = {0}", bossTimerId);
    }
}

public sealed class SEnterTriggerConditon : SSceneConditionBase
{
    public SEnterTriggerConditon(int triggerId, PVETriggerCollider.EnumTriggerType type, int playerNum = 1, int monsterId = 0) : base(SceneEventInfo.SceneConditionType.EnterTrigger, triggerId, (int)type, playerNum, monsterId) { }

    #region properties

    public int triggerId { get { return GetIntParames(0); } }
    public int triggerType { get { return GetIntParames(1); } }
    public int playerNum { get { return GetIntParames(2); } }

    public int monsterId { get { return GetIntParames(3); } }
    #endregion

    public override string GetParamesToString()
    {
        if(playerNum > 0)
        {
            return Util.Format("PlayerEnterTrigger:: triggerId = {0},triggerType = {1}, playerNum = {2}", triggerId, triggerType, playerNum);
        }
        return Util.Format("MonsterEnterTrigger:: triggerId = {0},triggerType = {1}, monsterId = {2}", triggerId, triggerType, monsterId);
    }
}

public sealed class SSceneActorStateConditon : SSceneConditionBase
{
    public SSceneActorStateConditon(int logicId, string state, EnumActorStateType stateType) : base(SceneEventInfo.SceneConditionType.SceneActorState, state, logicId, (int)stateType) { }

    #region properties

    public int logicId { get { return GetIntParames(0); } }
    public EnumActorStateType stateType { get { return (EnumActorStateType)GetIntParames(1); } }
    public string state { get { return strParam; } }
    #endregion

    public override string GetParamesToString()
    {
        return Util.Format("logicId = {0},stateType = {1}, state = {2}", logicId, stateType, state);
    }
}

public sealed class SWindowCombatVisibleConditon : SSceneConditionBase
{
    public SWindowCombatVisibleConditon() : base(SceneEventInfo.SceneConditionType.WindowCombatVisible) { }
}

public sealed class SEnterForFirstTimeConditon : SSceneConditionBase
{
    public SEnterForFirstTimeConditon(bool first) : base(SceneEventInfo.SceneConditionType.EnterForFirstTime, first ? 0 : 1) { }
}

public sealed class SRandomInfoConditon : SSceneConditionBase
{
    public SRandomInfoConditon(int randomId, int value) : base(SceneEventInfo.SceneConditionType.RandomInfo, randomId, 0, 0,value) { }

    #region properties

    public int randomId { get { return GetIntParames(0); } }

    public int minValue
    {
        get { return GetIntParames(1); }
    }

    public int maxValue
    {
        get { return GetIntParames(2); }
    }

    public int value
    {
        get { return GetIntParames(3); }
        set { SetIntParames(3,value); }
    }

    public override bool IsSameCondition(SSceneConditionBase condition)
    {
        return type == condition.type && randomId == condition.GetIntParames(0) && value >= condition.GetIntParames(1) && value <= condition.GetIntParames(2);
    }
    #endregion

    public override string GetParamesToString()
    {
        return string.Format("randomId = {0},value = {1}", randomId, value);
    }
}

public sealed class SMonsterAttackConditon : SSceneConditionBase
{
    public SMonsterAttackConditon(int mon, int group) : base(SceneEventInfo.SceneConditionType.MonsterAttack, mon, group) { }

    #region properties

    public int monsterId { get { return GetIntParames(0); } }
    public int group { get { return GetIntParames(1); } }
    #endregion

    public override string GetParamesToString()
    {
        return Util.Format("monId = {0},group = {1} attack success", monsterId, group);
    }
}
