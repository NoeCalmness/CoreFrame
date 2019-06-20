using System;
using UnityEngine;

public abstract class SSceneBehaviourBase
{
    public const string empty = "";

    public SceneEventInfo.SceneBehaviour behaviour { get; protected set; }
    public string eventName { get; protected set; }
    protected Action<SSceneBehaviourBase> callback;

    public SSceneBehaviourBase(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c, string e = empty)
    {
        behaviour = b;
        callback = c;
        eventName = e;
        if (behaviour == null) Logger.LogError("create SceneBehaviourBase error ,behaviour data is null");
    }

    #region properties
    public SceneEventInfo.SceneBehaviouType type { get { return behaviour == null ? SceneEventInfo.SceneBehaviouType.None : behaviour.sceneBehaviorType; } }
    #endregion

    public virtual void HandleBehaviour()
    {
        callback?.Invoke(this);
    }

    public int GetIntValue(int index)
    {
        if (behaviour == null || behaviour.parameters == null) return 0;
        return behaviour.parameters.GetValue<int>(index);
    }

    public string GetStringValue()
    {
        if (behaviour == null) return string.Empty;
        return behaviour.strParam;
    }

    public Vector4_ GetVecValue()
    {
        return behaviour == null ? Vector4_.zero : behaviour.vecParam;
    }

    public new virtual string ToString()
    {
        return Util.Format("[{0}] : detail {1} is called", GetType(), behaviour.ToXml());
    }
}

public sealed class SCreateMonsterBehaviour : SSceneBehaviourBase
{
    public SCreateMonsterBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventCreateMonster) { }

    #region properties
    public int monsterId { get { return GetIntValue(0); } }

    public int group { get { return GetIntValue(1); } }

    public int level { get { return GetIntValue(2); } }

    public bool boss { get { return GetIntValue(3) > 0; } }
    public bool showCameraAnim { get { return GetIntValue(4) > 0; } }
    public int frameEventId{ get { return GetIntValue(5);} }
    public int forceDirection { get { return GetIntValue(6); } }
    public bool getReward { get { return GetIntValue(7) > 0; } }
    
    public double reletivePos { get { return GetVecValue().x; } }
    #endregion
}

public sealed class SKillMonsterBehaviour : SSceneBehaviourBase
{
    public SKillMonsterBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c,Module_PVEEvent.EventKillMonster) { }

    #region properties
    public int monsterId { get { return GetIntValue(0); } }
    public int group { get { return GetIntValue(1); } }
    public int amount { get { return GetIntValue(2); } }
    #endregion
}

public sealed class SStartCountDownBehaviour : SSceneBehaviourBase
{
    public SStartCountDownBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventCountDown) { }

    #region properties
    public int timerId { get { return GetIntValue(0); } }
    /// <summary>
    /// 倒计时数量，单位：毫秒
    /// </summary>
    public int amount { get { return GetIntValue(1); } }
    /// <summary>
    /// 是否显示在UI上
    /// </summary>
    public bool showUI { get { return GetIntValue(2) > 0; } }
    #endregion
}

public sealed class SAddTimerValueBehaviour : SSceneBehaviourBase
{
    public SAddTimerValueBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c) { }

    #region properties
    public int timerId { get { return GetIntValue(0); } }
    public int amount { get { return GetIntValue(1); } }
    public int absoluteValue { get { return GetIntValue(2); } }
    #endregion
}

public sealed class SDelTimerBehaviour : SSceneBehaviourBase
{
    public SDelTimerBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c) { }

    #region properties
    public int timerId { get { return GetIntValue(0); } }
    #endregion
}

public sealed class SStageClearBehaviour : SSceneBehaviourBase
{
    public SStageClearBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventStageClear) { }
}

public sealed class SStageFailBehaviour : SSceneBehaviourBase
{
    public SStageFailBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventStageFail) { }
}

public sealed class SAddBufferBehaviour : SSceneBehaviourBase
{
    public SAddBufferBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventAddBuff) { }

    #region properties
    public int objectId { get { return GetIntValue(0); } }
    public int buffId { get { return GetIntValue(1); } }
    public int duraction { get { return GetIntValue(2); } }
    #endregion
}

public sealed class SStartStoryBehaviour : SSceneBehaviourBase
{
    public SStartStoryBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c) { }

    #region properties
    public int plotId { get { return GetIntValue(0); } }
    public EnumStoryType storyType { get { return (EnumStoryType)GetIntValue(1); } }
    #endregion
}

public sealed class SShowMessageBehaviour : SSceneBehaviourBase
{
    public SShowMessageBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventShowMessage) { }

    #region properties
    public int duraction { get { return GetIntValue(0); } }
    public int textId { get { return GetIntValue(1); } }
    #endregion
}

public sealed class SOperatingCounterBehaviour : SSceneBehaviourBase
{
    public SOperatingCounterBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c) { }

    #region properties
    public int counterId { get { return GetIntValue(0); } }
    public int numberChange { get { return GetIntValue(1); } }
    #endregion
}

public sealed class SLeaveMonsterBehaviour : SSceneBehaviourBase
{
    public SLeaveMonsterBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventLeaveMonster) { }

    #region properties
    public int monsterId { get { return GetIntValue(0); } }
    public int group { get { return GetIntValue(1); } }
    /// <summary>
    /// 持续时间,只针对循环动作,单位：毫秒
    /// </summary>
    public int leaveTime { get { return GetIntValue(2); } }

    public string state { get { return behaviour == null ? string.Empty : behaviour.strParam; } }
    #endregion
}

public sealed class SSetStateBehaviour : SSceneBehaviourBase
{
    public SSetStateBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventSetState) { }

    public enum EnumSetStateType
    {
        Force       = 0,
        OnGround    = 1,
        InTheAir       = 2,
    }

    #region properties
    public int monsterId { get { return GetIntValue(0); } }
    public int group { get { return GetIntValue(1); } }
    public EnumSetStateType setStateType { get { return (EnumSetStateType)GetIntValue(2); } }
    public string state { get { return behaviour == null ? string.Empty : behaviour.strParam; } }
    #endregion
}

public sealed class SCheckStageFirstTimeBehaviour : SSceneBehaviourBase
{
    public SCheckStageFirstTimeBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c) { }
}

public sealed class SAIPauseStateBehaviour : SSceneBehaviourBase
{
    public SAIPauseStateBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c) { }

    #region properties
    public int monsterId { get { return GetIntValue(0); } }
    public int group { get { return GetIntValue(1); } }
    public bool pauseAI { get { return GetIntValue(2) > 0; } }
    #endregion
}

public sealed class SBackToHomeBehaviour : SSceneBehaviourBase
{
    public SBackToHomeBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c) { }
}

public sealed class SStartGuideBehaviour : SSceneBehaviourBase
{
    public SStartGuideBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventStartPVEGuide) { }

    #region properties
    public int guideId { get { return GetIntValue(0); } }
    #endregion
}

public sealed class SPlayAudioBehaviour : SSceneBehaviourBase
{
    public SPlayAudioBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c) { }

    #region properties
    public AudioTypes audioType { get { return (AudioTypes)GetIntValue(0); } }
    public bool loop { get { return GetIntValue(1) > 0; } }
    public string audioName { get { return behaviour == null ? string.Empty : behaviour.strParam; } }
    #endregion
}

public sealed class SStopAudioBehaviour : SSceneBehaviourBase
{
    public SStopAudioBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c) { }

    #region properties
    public string audioName { get { return behaviour == null ? string.Empty : behaviour.strParam; } }
    #endregion
}

public sealed class SBossComingBehaviour : SSceneBehaviourBase
{
    public SBossComingBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventBossComing) { }

    #region properties
    public int timerId { get { return GetIntValue(0); } }
    public int amount { get { return GetIntValue(1); } }
    public int bossId1 { get { return GetIntValue(2); } }
    public int bossId2 { get { return GetIntValue(3); } }
    #endregion
}

public sealed class STransportSceneBehaviour : SSceneBehaviourBase
{
    public STransportSceneBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventTransportScene) { }

    #region properties
    public int levelId { get { return GetIntValue(0); } }
    public int position { get { return GetIntValue(1); } }
    public int eventId { get { return GetIntValue(2); } }

    private int m_delayTime;
    public int delayTime
    {
        get
        {
            if (m_delayTime <= 0)
            {
                m_delayTime = GetIntValue(3);
                m_delayTime = m_delayTime <= 0 ? 1000 : m_delayTime;
            }
            return m_delayTime;
        }
    }

    private string m_state;
    public string state
    {
        get
        {
            if (string.IsNullOrEmpty(m_state))
            {
                m_state = GetStringValue();
                m_state = string.IsNullOrEmpty(m_state) ? Module_AI.STATE_STAND_NAME : m_state;
            }
            return m_state;
        }
    }
    #endregion
}

public sealed class SCreateTriggerBehaviour : SSceneBehaviourBase
{
    public SCreateTriggerBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventCreateTrigger) { }

    public enum TriggerFlag
    {
        Normal = 0,

        TransportScene,
    }

    #region properties
    public int triggerId { get { return GetIntValue(0); } }

    public int state
    {
        get { return GetIntValue(1); }
        set
        {
            if (behaviour != null && behaviour.parameters.Length > 1) behaviour.parameters[1] = value;
        }
    }

    public TriggerFlag triggerFlag { get { return (TriggerFlag)GetIntValue(2); } }

    /// <summary>
    /// 是否随机创建触发器
    /// </summary>
    public bool isRandom { get { return GetIntValue(3) == 1; } }

    public Vector4_ m_range = Vector4_.zero;
    public Vector4_ range
    {
        get
        {
            if(m_range == Vector4_.zero) m_range = GetVecValue();
            if (m_range == Vector4_.zero || (m_range.z == m_range.w && m_range.z == 0))
            {
                Logger.LogError("{0} is invalid behaviour");
                m_range = new Vector4_(m_range.x, m_range.y, 1, 2);
            }
            return m_range;
        }
    }
    
    private string m_activeEffect;
    public string activeState
    {
        get
        {
            CheckSplitStr();
            return m_activeEffect;
        }
    }

    private string m_inactiveEffect;
    public string inactiveEffect
    {
        get
        {
            CheckSplitStr();
            return m_inactiveEffect;
        }
    }

    private void CheckSplitStr()
    {
        if(string.IsNullOrEmpty(m_activeEffect) && !string.IsNullOrEmpty(GetStringValue()))
        {
            string[] effects = GetStringValue().Split(';');
            if(effects.Length > 0)
                m_activeEffect = effects.GetValue<string>(0);
            if(effects.Length > 1)
                m_inactiveEffect = effects.GetValue<string>(1);
        }
    }
    #endregion
}

public sealed class SOperateTriggerBehaviour : SSceneBehaviourBase
{
    public SOperateTriggerBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventOperateTrigger) { }

    #region properties
    public int triggerId { get { return GetIntValue(0); } }
    public int state { get { return GetIntValue(1); } }
    #endregion
}

public sealed class SOperateSceneAreaBehaviour : SSceneBehaviourBase
{
    public SOperateSceneAreaBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventOperateSceneArea) { }

    public enum EnumAreaDirction
    {
        Left = 0,
        Right = 1,
    }

    #region properties
    public EnumAreaDirction direction { get { return (EnumAreaDirction)GetIntValue(0); } }

    public int addtiveValue { get { return GetIntValue(1); } }

    public int absoluteValue { get { return GetIntValue(2); } }

    public bool useAbsoluteValue { get { return addtiveValue == 0; } }
    #endregion
}

public sealed class SCreateSceneActorBehaviour : SSceneBehaviourBase
{
    public SCreateSceneActorBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventCreateSceneActor) { }

    #region properties
    /// <summary>
    /// 对应monsterinfo.id
    /// </summary>
    public int sceneActorId { get { return GetIntValue(0); } }

    /// <summary>
    /// 唯一ID
    /// </summary>
    public int logicId { get { return GetIntValue(1); } }
    public int reletivePos { get { return GetIntValue(2); } }
    public int level { get { return GetIntValue(3); } }
    public int forceDirection { get { return GetIntValue(4); } }
    public int group { get { return GetIntValue(5); } }
    public string state { get { return GetStringValue(); } }
    #endregion
}

public sealed class SDeleteConditionBehaviour : SSceneBehaviourBase
{
    public SDeleteConditionBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventDeleteCondition) { }

    #region properties
    /// <summary>
    /// 条件ID
    /// </summary>
    public int conditionId { get { return GetIntValue(0); } }
    #endregion
}

public sealed class SIncreaseConditionAmountBehaviour : SSceneBehaviourBase
{
    public SIncreaseConditionAmountBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventIncreaseConditionAmount) { }

    #region properties
    /// <summary>
    /// 条件ID
    /// </summary>
    public int conditionId { get { return GetIntValue(0); } }

    public int amount { get { return GetIntValue(1); } }
    #endregion
}

public sealed class SOperateSceneActorBehaviour : SSceneBehaviourBase
{
    public SOperateSceneActorBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventOperateSceneActor) { }

    #region properties
    public int logicId { get { return GetIntValue(0); } }
    public string state { get { return GetStringValue(); } }
    #endregion
}

public sealed class SDelSceneActorEventBehaviour : SSceneBehaviourBase
{
    public SDelSceneActorEventBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventDelSceneActorEvent) { }

    #region properties
    public int sceneActorId { get { return GetIntValue(0); } }
    public int logicId { get { return GetIntValue(1); } }
    public EnumActorStateType stateType { get { return (EnumActorStateType)stateInt; } }
    public int stateInt { get { return GetIntValue(2); } }
    public string state { get { return GetStringValue(); } }
    #endregion
}

public sealed class SCreateLittleBehaviour : SSceneBehaviourBase
{
    public SCreateLittleBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventCreateLittle) { }

    #region properties
    public int monsterId { get { return GetIntValue(0); } }
    
    public int group { get { return GetIntValue(1); } }

    public int level { get { return GetIntValue(2); } }
    public Vector4_ offset { get { return GetVecValue(); } }

    public Vector3_ createPos { get; set; }

    public double createPosX { get { return createPos.x + offset.x; } }
    public double createPosY { get { return createPos.y + offset.y; } }
    public double createPosZ { get { return createPos.z + offset.z; } }
    #endregion
}

public sealed class SBuildRandomBehaviour : SSceneBehaviourBase 
{
    public SBuildRandomBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c) { }

    #region properties
    public int randomId { get { return GetIntValue(0); } }

    public int maxValue { get { return GetIntValue(1); } }
    #endregion
}

public sealed class SRebuildRandomBehaviour : SSceneBehaviourBase
{
    public SRebuildRandomBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c) { }

    #region properties
    public int randomId { get { return GetIntValue(0); } }

    public int maxValue { get { return GetIntValue(1); } }
    #endregion
}

public sealed class SMoveMonsterPosBehaviour : SSceneBehaviourBase
{
    public SMoveMonsterPosBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventMoveMonsterPos) { }

    #region properties

    public int monsterId { get { return GetIntValue(0); } }

    public int group { get { return GetIntValue(1); } }

    public double reletivePosX { get { return GetVecValue().x; } }
    public double reletivePosY { get { return GetVecValue().y; } }

    public Creature creature { get; set; }
    #endregion
}

public sealed class SCreateAssistantBehaviour : SSceneBehaviourBase
{
    public SCreateAssistantBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventCreateAssistant) { }

    #region properties
    public int logicId { get { return GetIntValue(0); } }
    public double reletivePos { get { return GetVecValue().x; } }
    public string bornState { get { return GetStringValue(); } }
    #endregion
}

public sealed class SCreateAssistantNpcBehaviour : SSceneBehaviourBase
{

    public SCreateAssistantNpcBehaviour(SceneEventInfo.SceneBehaviour b, Action<SSceneBehaviourBase> c) : base(b, c, Module_PVEEvent.EventCreateAssistantNpc) { }

    #region properties
    public int logicId { get { return GetIntValue(0); } }
    public double reletivePos { get { return GetVecValue().x; } }
    public string bornState { get { return GetStringValue(); } }
    #endregion
}
