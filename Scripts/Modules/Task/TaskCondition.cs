using UnityEngine;
using System.Collections;
public enum EnumTaskType
{
    NpcType = 1,
    EngagementType,
    Max,
}
public enum EnumTaskConditionType
{
    FinishAll = 0,                     //完成所有条件 
    FinishOne,                        //完成任意条件
    FinishTwo,
    FinishThree,
    FinishFour,
    FinishFive,
    Max,
}
//条件枚举定义
public enum EnumTaskCondition
{
    None = 0,
    MoodValue = 1,                 //当前约会的 NPC 心情判断
    EnergyValue,                   //当前约会的 NPC 精力判断
    FinishAllEvent,                //完成一个事件 任务完成
    ChooseGoodAnswer,              //选择正确答案
    ConsumeGold,                   //金币消耗
    Gift,                          //送礼
    DateScore,                     //和对应NPC约会并达到指定的约会评价 0-D 1-C 2-B 3-A 4-S
    FinishSceneEvent,              //完成指定关卡
    Max,
}
//工厂方法接口
interface IFactroyCondition
{
    TaskCondition CreateCondition(int value);
}

public abstract class TaskCondition
{
    public int m_targetValue { set; get; }
    //当前进度值
    public uint m_curValue { set; get; }
    public NpcTypeID m_targetID { set; get; }
    public bool FinishedCondition { set; get; }
    public EnumTaskCondition m_taskCondition { set; get; }
    public TaskCondition(int value,uint curvalue = 0)
    {
        m_targetValue = value;
        m_curValue = curvalue;
    }
    public abstract bool Execute();
}
public class NoneCondition:TaskCondition
{
    public NoneCondition(int value) : base(value)
    {

    }
    public override bool Execute()
    {
        FinishedCondition = true;
        return FinishedCondition;
    }

}
public class NoneConditionFactory : IFactroyCondition
{
    public TaskCondition CreateCondition(int value)
    {
        return new NoneCondition(value);
    }
}
public class MoodCondition : TaskCondition
{
    //更新心情值 m_curValue
    public MoodCondition(int value):base(value)
    {
       
    }
    public override bool Execute()
    {
         //当前约会对象心情值大于等于任务配置表中设定值
         FinishedCondition = m_curValue >= m_targetValue ? true : false;
         return FinishedCondition;
    }
}
public class MoodFactoryCondition: IFactroyCondition
{
    public TaskCondition CreateCondition(int value)
    {
        return new MoodCondition(value);
    }
}
public class EnergCondition: TaskCondition
{
    //NPC 精力消耗到预定值
    public EnergCondition(int value) : base(value)
    {

    }
    public override bool Execute()
    {
        //当前约会对象体力消耗
        FinishedCondition = m_curValue >= m_targetValue ? true : false;
        return FinishedCondition;
    }

}
public class EnergFactoryCondition : IFactroyCondition
{
    public TaskCondition CreateCondition(int value)
    {
        return new EnergCondition(value);
    }
}


public class ConsumeGoldCondition:TaskCondition
{
    //金币消耗
    public ConsumeGoldCondition(int value) : base(value)
    {
    }
    public override bool Execute()
    {
        FinishedCondition = m_curValue >= m_targetValue ? true : false;
        return FinishedCondition;
    }
}

public class ConsumeGoldConditionFactroy:IFactroyCondition
{
    public TaskCondition CreateCondition(int value)
    {
        return new ConsumeGoldCondition(value);
    }
}


public class FinishAllEvent: TaskCondition
{
    //完成一个事件，该任务就算完成
    public FinishAllEvent(int value) : base(value)
    {

    }
    public override bool Execute()
    {
        //与服务器约定，配置的ID == 接受到的事件ID相等表示任务成功完成
        FinishedCondition = m_curValue == m_targetValue;
        return FinishedCondition;
    }
}
public class FinishAllEventFactroyCondition:IFactroyCondition
{
    public TaskCondition CreateCondition(int value)
    {
        return new FinishAllEvent(value);
    }
}
public class ChooseGoodAnswer:TaskCondition
{
    // m_targetValue 问题ID
    public ChooseGoodAnswer(int value) : base(value)
    {

    }
    public override bool Execute()
    {
        //服务器更新选择问题的答案，1表示正确 0表示错误
        FinishedCondition = m_curValue != 0;
        return FinishedCondition;
    }
}

public class ChooseGoodAnswerFactroy:IFactroyCondition
{
    public TaskCondition CreateCondition(int value)
    {
        return new ChooseGoodAnswer(value);
    }
}

public class Gift:TaskCondition
{
    //送礼
    public Gift(int value) : base(value)
    {

    }
    public override bool Execute()
    {
        //礼物ID 等于服务器下发的礼物ID 表示该任务条件已完成
            FinishedCondition = m_targetValue == m_curValue;
        //这里可以支持送礼物给其他NPC
        return FinishedCondition;
    }
}

public class GiftFactroy:IFactroyCondition
{
    public TaskCondition CreateCondition(int value)
    {
        return new Gift(value);
    }
}

public class DateScore:TaskCondition
{
    //约会评价
    public DateScore(int value) : base(value)
    {

    }
    public override bool Execute()
    {
        //根据约会目标值 与服务器下发的判定值做比较 表示该任务条件是否完成
        FinishedCondition =  m_curValue >= m_targetValue ? true:false;
        return FinishedCondition;
    }
}


public class DateScoreFactroy:IFactroyCondition
{
    public TaskCondition CreateCondition(int value)
    {
        return new DateScore(value);
    }
}

public class FinishSceneEvent:TaskCondition
{
    //完成指定关卡
    public FinishSceneEvent(int value) : base(value)
    {

    }
    public override bool Execute()
    {
        //根据事件ID去判断当前任务条件是否完成
        FinishedCondition = m_targetValue == m_curValue;
        return FinishedCondition;
    }
}

public class FinishSceneEventFactroy :IFactroyCondition
{
    public TaskCondition CreateCondition(int value)
    {
        return new FinishSceneEvent(value);
    }
}