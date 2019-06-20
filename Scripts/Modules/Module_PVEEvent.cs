/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-05-16
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
#if PVE_EVENT_LOG
using System.Text;
#endif

public class Module_PVEEvent : Module<Module_PVEEvent>
{
    #region const definition
    public const string EventCreateMonster          = "EventCreateMonster";
    public const string EventCountDown              = "EventCountDown";
    public const string EventAddBuff                = "EventAddBuff";
    public const string EventStageClear             = "EventStageClear";
    public const string EventStageFail              = "EventStageFail";
    public const string EventKillMonster            = "EventMonsterDeath";
    public const string EventShowMessage            = "EventShowMessage";
    public const string EventLeaveMonster           = "EventLeaveMonster";
    public const string EventSetState               = "EventSetState";
    public const string EventBossComing             = "EventBossComing";
    public const string EventTransportScene         = "EventTransportScene";
    public const string EventCreateTrigger          = "EventCreateTrigger";
    public const string EventOperateTrigger         = "EventOperateTrigger";
    public const string EventOperateSceneArea       = "EventOperateSceneArea";
    public const string EventCreateSceneActor       = "EventCreateSceneActor";
    public const string EventOperateSceneActor      = "EventOperateSceneActor";
    public const string EventDelSceneActorEvent     = "EventDelSceneActorEvent";
    public const string EventCreateLittle           = "EventCreateLittle";
    public const string EventStartPVEGuide          = "EventStartPVEGuide";
    public const string EventMoveMonsterPos         = "EventMoveMonsterPos";
    public const string EventCreateAssistant        = "EventCreateAssistant";
    public const string EventCreateAssistantNpc     = "EventCreateAssistantNpc";
    public const string EventDeleteCondition        = "EventDeleteCondition";
    public const string EventIncreaseConditionAmount = "EventIncreaseConditionAmount";

    /// <summary>
    /// PVE计时器的检测帧率
    /// </summary>
    public const int PVE_TIMER_FRAME_COUNT = 10;

    /// <summary>
    /// PVE计时器的检测帧率
    /// </summary>
    public const int PVE_DEATH_STAY_TIME = 1;
    #endregion

    #region 内嵌类
    public class MonsterData
    {
        public int monsterId;

        public int group;
        public bool isNpcGroup { get { return group == -2; } }

        /// <summary>
        /// 普通怪物数量
        /// </summary>
        public int amount { get; set; }

        public EnumMonsterType monsterType { get; set; }
        public bool isDestructible { get { return monsterType == EnumMonsterType.Destructible; } }

        /// <summary>
        /// 是否还有活着的怪物
        /// </summary>
        public bool IsNormalStillLive { get { return amount > 0; } }
    }

    public class TimerData
    {
        public EnumPVETimerType type { get; private set; }
        public int timerId { get; private set; }
        /// <summary>
        /// 时间，单位秒
        /// </summary>
        public double amount { get; set; }
        public int amountInt { get { return Mathd.CeilToInt(amount); } }
        public bool isShow;

        public Action<int> countDownEnd;
        public Action<int> showCountDown;

        public TimerData() { }

        public TimerData(int id, int duraction = 0, int show = 0, EnumPVETimerType t = EnumPVETimerType.NormalTimer)
        {
            timerId = id;
            amount = duraction * 0.001;
            isShow = show == 1;
            type = t;
        }

        public TimerData(int[] paramers, EnumPVETimerType t = EnumPVETimerType.NormalTimer)
        {
            timerId = paramers[0];
            amount = paramers[1] * 0.001;
            isShow = paramers[2] == 1;
            type = t;
        }

        public void OnUpdate(double deltaTime)
        {
            amount -= deltaTime;
            if (amount < 0) amount = 0;

            CheckShowCountDownCallback();
            CheckCountEndCallback();
        }

        private void CheckCountEndCallback()
        {
            if (amount == 0) countDownEnd?.Invoke(timerId);
        }

        public void CheckShowCountDownCallback()
        {
            if (isShow) showCountDown?.Invoke(amountInt);
        }
    }

    public class FrameEventData
    {
        /// <summary>
        /// 角色
        /// </summary>
        public Creature creature;
        /// <summary>
        /// 帧事件
        /// </summary>
        public SceneFrameEventInfo.SceneFrameEventItem frameEvent;
        /// <summary>
        /// 当前还能不能检测
        /// </summary>
        public bool checkState;

        public bool validFrameEventData { get { return creature && frameEvent != null && frameEvent.behaviours != null; } }

        public bool haveResetTime { get { return frameEvent != null && frameEvent.haveRestTimes; } }

        public bool IsValidEventForCreature(Creature c)
        {
            return c != null && c == creature;
        }

        public ulong creatureRoleId { get { return creature ? creature.roleId : 0; } }

        public bool TriggerFrameEvent()
        {
            if (!checkState || frameEvent == null || !creature || !creature.currentState) return false;

            return frameEvent.TriggerFrameEvent(creature.currentState);
        }

        public override string ToString()
        {
            return Util.Format("{0} frameEventItem is [state : {1}, frameCount : {2}, limitTimes = {3}] checkState is {4}",creature.uiName,frameEvent.frameState,frameEvent.frameCount,frameEvent.limitTimes,checkState);
        }
    }

    public class SceneEventData
    {
        public List<SSceneConditionBase> conditions { get; private set; }
        public List<SSceneBehaviourBase> behaviours { get; private set; }
        public int limitTime { get; private set; }
        public bool needRemove { get { return limitTime == 0; } }
        public bool unlimit { get { return limitTime < 0; } }

        public SceneEventData(List<SSceneConditionBase> con, List<SSceneBehaviourBase> be,int times)
        {
            conditions = con;
            behaviours = be;
            limitTime = times;

            //操作限制默认为一次
            if (limitTime == 0) limitTime = 1;
        }

        public void DecreseLimitTime()
        {
            if (unlimit) return;
            limitTime--;
        }
    }
    #endregion

    #region field
    private List<SSceneConditionBase> m_currentCacheCondition = new List<SSceneConditionBase>();
    /// <summary>
    /// 当前场景的所有条件和行为字典
    /// </summary>
    public List<SceneEventData> eventList { get; private set; } = new List<SceneEventData>();
    //管理场景上活着的怪物数据，结构：<怪物ID，不同分组的相同怪物的链表(比如第一波的哥布林和第二波的哥布林都存于该链表)>
    private Dictionary<int, List<MonsterData>> m_currentMonsters = new Dictionary<int, List<MonsterData>>();
    //管理场景上已经死亡的怪物数据，结构：<怪物ID，已经死亡的不同分组的相同怪物的链表(比如第一波的哥布林和第二波的哥布林都存于该链表)>
    private Dictionary<int, List<MonsterData>> m_deathMonsters = new Dictionary<int, List<MonsterData>>();
    //管理后台的所有计时器(此处不能用字典，因为字典只能迭代会受到单线程保护)
    private List<TimerData> m_pveTimerList = new List<TimerData>();
    //计数器字典
    private Dictionary<int, int> m_counterDic = new Dictionary<int, int>();
    /// <summary>
    /// 怪物死亡条件的缓冲池
    /// </summary>
    private Queue<SMonsterDeathConditon> m_monsterDeathConditionPool = new Queue<SMonsterDeathConditon>();

    //是否需要暂停计时
    public bool pauseCountDown { get; set; }

    private List<int> m_NeedDisableCondition = new List<int>();

    public bool isEnd { get; set; }

    private bool m_pauseEvent = false;
    /// <summary>
    /// 设置条件：
    /// 1.玩家死亡状态，当玩家死亡时，场景的事件行为就不再触发，直到玩家复活
    /// 必须在玩家血量为0的时刻设为true
    /// 必须在玩家复活的时刻设为false
    /// 
    /// 2.boss获取奖励时
    /// 必须在怪物血量为0的时刻设为true
    /// 必须在消息出错或者奖励动画显示完毕之后的时刻设为false
    /// </summary>
    public bool pauseEvent
    {
        get { return m_pauseEvent; }
        set
        {
            if (m_pauseEvent == value) return;

            m_pauseEvent = value;
            pauseCountDown = value;

            //复活之后，需要重新检测一次当前的事件
            if (!m_pauseEvent) CheckValidBehaviours();
        }
    }

    private Dictionary<Creature, SceneFrameEventInfo> m_frameEventDic = new Dictionary<Creature, SceneFrameEventInfo>();
    private List<FrameEventData> m_checkFrameEvents = new List<FrameEventData>();

    /// <summary>
    /// 缓存提示信息显示队列
    /// </summary>
    private Queue<SShowMessageBehaviour> m_showMessageCache = new Queue<SShowMessageBehaviour>();

    /// <summary>
    /// 是否可以显示(主要取决于是不是已经全部完成了加载或者取消了黑屏显示)
    /// </summary>
    public bool readyToShowMsgLogic { get; set; }

    /// <summary>
    /// 能否开始检测showmsg行为
    /// </summary>
    public bool validCheckShowMsgbehaviour { get { return readyToShowMsgLogic && m_showMessageCache != null && m_showMessageCache.Count > 0; } }

    /// <summary>
    /// 二次检测判断是否还有行为可以触发
    /// </summary>
    private bool m_secondTriggerCheck = false;
    #endregion

    #region override
    protected override void OnWillEnterGame()
    {
        base.OnWillEnterGame();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        SceneEventInfo.CheckSceneEventValid();
        SceneEventInfo.CheckRandomBoss();
#endif
    }
    #endregion

    #region logic 
    
    public void LoadPVESceneEvent(int eventId)
    {
        SceneEventInfo info = ConfigManager.Get<SceneEventInfo>(eventId);
        if (info == null)
        {
            Logger.LogError("加载场景失败，场景事件ID = {0}", eventId);
            return;
        }
        LoadPVESceneEvent(info);
    }

    public void LoadPVESceneEvent(SceneEventInfo info)
    {
        if (info == null)
        {
            Logger.LogError("加载场景失败.....info is null");
            return;
        }
        ClearAll();

        for (int i = 0, length = info.sceneEvents.Length; i < length; i++)
        {
            SceneEventInfo.SceneEvent sceneEvent = info.sceneEvents[i];
            List<SSceneConditionBase> cs = new List<SSceneConditionBase>();
            List<SSceneBehaviourBase> bs = new List<SSceneBehaviourBase>();

            foreach (var c in sceneEvent.conditions) cs.Add(new SSceneConditionBase(c));
            foreach (var b in sceneEvent.behaviours) bs.Add(CreateSSceneBehaviour(b));
            eventList.Add(new SceneEventData(cs,bs, sceneEvent.times));
        }
    }

    private void InitData()
    {
        m_currentMonsters.Clear();
        m_deathMonsters.Clear();
        m_currentCacheCondition.Clear();
        eventList.Clear();
        m_pveTimerList.Clear();
        m_counterDic.Clear();
        m_monsterDeathConditionPool.Clear();
        m_frameEventDic.Clear();
        m_checkFrameEvents.Clear();
        m_showMessageCache.Clear();
        m_NeedDisableCondition.Clear();
        readyToShowMsgLogic = false;
        m_secondTriggerCheck = false;
    }

    public void ClearAll()
    {
        InitData();
        ClearCache();
        enableUpdate = false;
    }

    public void ClearCache()
    {
        isEnd = false;
        pauseCountDown = false;
        m_pauseEvent = false;
    }

    public void AddCondition(SSceneConditionBase c,bool trigger = true)
    {
        if (m_currentCacheCondition == null)
        {
            SceneEventInfo.SceneBehaviour cb = new SceneEventInfo.SceneBehaviour();
            cb.sceneBehaviorType = SceneEventInfo.SceneBehaviouType.StageFail;
            SStageFailBehaviour b = new SStageFailBehaviour(cb,OnStageOver);
            b.HandleBehaviour();
            return;
        }

        if (c.type == SceneEventInfo.SceneConditionType.MonsterDeath)
        {
            var mdc = c as SMonsterDeathConditon;
            RefreshMonster(mdc.monsterId, mdc.group);
            //清空上一次的所有怪物死亡缓存
            ClearAllMonsterCondition();
            //将所有可能的怪物事件添加进缓存
            m_currentCacheCondition.AddRange(GetMonsterCondition());
        }
        else if (c.type == SceneEventInfo.SceneConditionType.MonsterLeaveEnd)
        {
            var mlec = c as SMonsterLeaveEndConditon;
            //怪物离场，需要将当前存储活着的怪物的数量-1，但是该怪物不会计算到死亡怪物数量中
            RefreshLiveMonster(mlec.monsterId, mlec.group);
            //怪物离场需要生成一次怪物死亡事件
            ClearAllMonsterCondition();
            m_currentCacheCondition.AddRange(GetMonsterCondition());
        }
        else if (c.type == SceneEventInfo.SceneConditionType.CounterNumber) ClearAllCounterCondition();
        else if (c.type == SceneEventInfo.SceneConditionType.EnterTrigger) ClearSameTriggerCondition(c as SEnterTriggerConditon);
        else if (c.type == SceneEventInfo.SceneConditionType.SceneActorState) ClearSameSceneActorCondition(c as SSceneActorStateConditon);
        else if (c.type == SceneEventInfo.SceneConditionType.HitTimes) AddHitTimesCondition(c as SHitTimesConditon);

        if (c.type == SceneEventInfo.SceneConditionType.MonsterHPLess) AddMonsterHPCondition(c as SMonsterHPLessConditon);
        else if (c.type == SceneEventInfo.SceneConditionType.PlayerHPLess) AddPlayerHPCondition(c as SPlayerHPLessConditon);
        else if (c.type != SceneEventInfo.SceneConditionType.MonsterDeath)
            m_currentCacheCondition.Add(c);

        if(trigger) CheckValidBehaviours(c.type);
    }

    private void CheckValidBehaviours(SceneEventInfo.SceneConditionType type = SceneEventInfo.SceneConditionType.None)
    {
        //条件可以一直填充，但是行为只能在玩家还活着的时候去触发
        if (!m_pauseEvent)
        {
            //分发事件
            List<SSceneBehaviourBase> totalBehaviours = GetValidBehaviour();
            DispatchBehaviour(totalBehaviours);

            totalBehaviours = GetForceBehaviour(type);
            DispatchBehaviour(totalBehaviours);

            if(m_secondTriggerCheck)
            {
                m_secondTriggerCheck = false;
                CheckValidBehaviours();
            }
        }
    }

    /// <summary>
    /// 根据不同的条件类型，设置必须延迟强制执行的行为
    /// 1.收到enterscene条件之后，在执行完其余创建怪物的事件之后，会强行执行检测是否是第一次进入
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    private List<SSceneBehaviourBase> GetForceBehaviour(SceneEventInfo.SceneConditionType condition)
    {
        if (condition == SceneEventInfo.SceneConditionType.EnterScene)
        {
            SceneEventInfo.SceneBehaviour b = new SceneEventInfo.SceneBehaviour();
            b.sceneBehaviorType = SceneEventInfo.SceneBehaviouType.CheckStageFirstTime;
            return new List<SSceneBehaviourBase>() { new SCheckStageFirstTimeBehaviour(b, CheckStageFirstEnter) };
        }

        return null;
    }

    private void ClearSameSceneActorCondition(SSceneActorStateConditon c)
    {
        for (int i = 0, count = m_currentCacheCondition.Count; i < count; i++)
        {
            SSceneConditionBase oc = m_currentCacheCondition[i];
            if (c.IsSameCondition(oc))
            {
                m_currentCacheCondition.Remove(oc);
                break;
            }
        }
    }

    /// <summary>
    /// 获取怪物死亡的详情
    /// </summary>
    /// <returns></returns>
    public Dictionary<int,int> GetMonsterDeathDetail()
    {
        Dictionary<int, int> dic = new Dictionary<int, int>();
        foreach (var item in m_deathMonsters)
        {
            dic.Add(item.Key,0);
            foreach (var d in item.Value)
            {
                dic[item.Key] += d.amount;
            }
        }

        return dic;
    }
    #endregion

    #region 怪物相关数据


    /// <summary>
    /// 当怪物复活的时候，必须执行该事件
    /// </summary>
    /// <param name="behaviour"></param>
    public void ReviveMonster(MonsterCreature monster)
    {
        if (!monster) return;
        
        var reviveMonId = monster.monsterId;
        var reciveGroup = monster.monsterGroup;

        //从死亡分组移除，然后将其添加到现有怪物分组
        RefreshLiveMonster(reviveMonId, reciveGroup, 1);
        RefreshDeathMonster(reviveMonId, reciveGroup, -1);

        //添加怪物事件
        ClearAllMonsterCondition();
        m_currentCacheCondition.AddRange(GetMonsterCondition());

        //重新触发一次事件
        CheckValidBehaviours();
    }

    /// <summary>
    /// 怪物死亡更新(每一只怪物死亡的时候)
    /// </summary>
    /// <param name="deathMonsterId"></param>
    /// <returns></returns>
    private void RefreshMonster(int deathMonsterId, int groupId)
    {
        RefreshLiveMonster(deathMonsterId, groupId);
        RefreshDeathMonster(deathMonsterId, groupId);
    }

    /// <summary>
    /// 刷新仍在场上的怪物数量
    /// </summary>
    /// <param name="deathMonsterId"></param>
    /// <param name="groupId"></param>
    /// <param name="count"></param>
    private void RefreshLiveMonster(int deathMonsterId, int groupId,int count = -1)
    {
        //找到死亡的怪物
        if (!m_currentMonsters.ContainsKey(deathMonsterId))
        {
            Logger.LogWarning("ID = {0}的怪物死亡处理失败，已经不存在于怪物列表", deathMonsterId);
            return;
        }
        
        List<MonsterData> monsters = m_currentMonsters[deathMonsterId];
        if (monsters == null || monsters.Count == 0) return;

        bool success = false;
        foreach (var m in monsters)
        {
            if (m.group == groupId)
            {
                success = true;
                m.amount += count;
                break;
            }
        }

        if (!success) Logger.LogWarning("ID = {0}的怪物死亡处理失败，已经不存在于怪物列表", deathMonsterId);
    }

    /// <summary>
    /// 刷新死亡怪物数量
    /// </summary>
    /// <param name="deathMonsterId"></param>
    /// <param name="groupId"></param>
    private void RefreshDeathMonster(int deathMonsterId, int groupId, int count = 1)
    {
        if (count == 0) Logger.LogError("monster id= {0} group = {1} dead count is zero!!!!!!!",deathMonsterId,groupId,count);
        if (!m_deathMonsters.ContainsKey(deathMonsterId)) m_deathMonsters.Add(deathMonsterId, new List<MonsterData>());

        List<MonsterData> deathList = m_deathMonsters[deathMonsterId];
        bool isContain = false;
        for (int i = 0; i < deathList.Count; i++)
        {
            if (deathList[i].group == groupId)
            {
                deathList[i].amount += count;
                isContain = true;
                break;
            }
        }

        if (!isContain)
        {
            MonsterData data = new MonsterData();
            data.monsterId = deathMonsterId;
            data.group = groupId;
            data.amount += count;
            deathList.Add(data);
        }

#if PVE_EVENT_LOG

        StringBuilder s = new StringBuilder();
        if (count < 0) s.AppendFormat("复活怪物：monId = {0} group  = {1}",deathMonsterId, groupId);
        else if (count > 0) s.AppendFormat("死亡怪物：monId = {0} group  = {1}", deathMonsterId, groupId);
        s.AppendLine("存活的怪物列表:");
        foreach (var item in m_currentMonsters)
        {
            foreach (var mon in item.Value)
            {
                s.AppendFormat("怪物id:{0} 怪物分组：{1} 怪物数量{2}", mon.monsterId, mon.group, mon.amount);
                s.AppendLine();
            }
        }
        s.AppendLine("当前死亡的怪物列表:");
        foreach (var item in m_deathMonsters)
        {
            foreach (var mon in item.Value)
            {
                s.AppendFormat("怪物id:{0} 怪物分组：{1} 怪物数量{2}", mon.monsterId, mon.group, mon.amount);
                s.AppendLine();
            }
        }
        Logger.LogDetail(s.ToString());

#endif
    }

    /// <summary>
    /// 每一次都直接拿到怪物死亡时候能组合成的条件
    /// tip;由于每次怪物死亡都会重新生成怪物检索条件，所以每次只需要针对deathMonsterId和groupId来检查
    /// 如果当前生成的怪物检索条件不满足事件触发，则下一次怪物死亡事件会将怪物事件清空，然后生成新的怪物死亡检索条件
    /// </summary>
    /// <returns></returns>
    private List<SSceneConditionBase> GetMonsterCondition()
    {
        List<SSceneConditionBase> conditions = new List<SSceneConditionBase>();

        //先添加所有怪物-所有组-全部死亡条件
        SMonsterDeathConditon allDeathCondition = GetAllMonsterDeathCondition();
        if (allDeathCondition != null) conditions.Add(allDeathCondition);

        //再添加所有怪物-指定组-全部死亡数量
        List<SMonsterDeathConditon> deathConditions = GetAllMonsterDeathForGroupConditions();
        if (deathConditions.Count > 0) conditions.AddRange(deathConditions);

        //再添加任意怪物-指定组-当前死亡数量
        deathConditions = GetGroupDeathCountForAnyMonsterConditions();
        if (deathConditions.Count > 0) conditions.AddRange(deathConditions);

        //再添加指定怪物-所有组-全部死亡/当前死亡数量
        deathConditions = GetAllGroupDeathForMonsterConditions();
        if (deathConditions.Count > 0) conditions.AddRange(deathConditions);

        //再添加指定怪物 - 指定组 - 全部死亡/当前死亡数量
        deathConditions = GetSpecifyMonsterSpecifyGroupConditions();
        if (deathConditions.Count > 0) conditions.AddRange(deathConditions);

#if PVE_EVENT_LOG
        StringBuilder s = new StringBuilder();
        s.AppendLine("死亡条件列举如下:");
        foreach (var item in conditions)
        {
            s.AppendLine(item.ToString());
        }
        Logger.LogDetail(s.ToString());
#endif

        return conditions;
    }

    #region 怪物死亡条件细节检索
    /// <summary>
    /// 获取全部怪物死亡条件
    /// </summary>
    /// <returns></returns>
    private SMonsterDeathConditon GetAllMonsterDeathCondition()
    {
        bool isAllDeath = true;
        foreach (var item in m_currentMonsters)
        {
            List<MonsterData> monsters = item.Value;
            for (int i = 0; i < monsters.Count; i++)
            {
                //npc 不检查-1,-1,-1
                if (monsters[i].isNpcGroup || monsters[i].isDestructible) continue;

                if (monsters[i].IsNormalStillLive)
                {
                    isAllDeath = false;
                    break;
                }
            }

            //如果有一个元素已经检查到有怪物存活，直接中断检测
            if (!isAllDeath) break;
        }

        //所有怪物所有组的全部死亡
        if (isAllDeath) return GetValidMonsterDeathCondition(-1,-1,-1);
        return null;
    }

    /// <summary>
    /// 所有怪物-指定的分组-死亡条件
    /// </summary>
    /// <returns></returns>
    private List<SMonsterDeathConditon> GetAllMonsterDeathForGroupConditions()
    {
        List<SMonsterDeathConditon> conditions = new List<SMonsterDeathConditon>();
        Dictionary<int, int> groupAliveCountDic = new Dictionary<int, int>();
        foreach (var item in m_currentMonsters)
        {
            List<MonsterData> monsters = item.Value;
            for (int i = 0; i < monsters.Count; i++)
            {
                //统计指定分组的时候，过滤到可破坏物
                if (monsters[i].isDestructible) continue;

                if (!groupAliveCountDic.ContainsKey(monsters[i].group))
                {
                    groupAliveCountDic.Add(monsters[i].group, monsters[i].amount);
                }
                else
                {
                    groupAliveCountDic[monsters[i].group] += monsters[i].amount;
                }
            }
        }

        foreach (var item in groupAliveCountDic)
        {
            //当前分组没有怪物了
            if (item.Value <= 0) conditions.Add(GetValidMonsterDeathCondition(-1, item.Key, -1));
        }

        return conditions;
    }

    /// <summary>
    /// 获取指定组的所有怪物死亡数量
    /// eg: monster id = -1,group = 2,amount = 5(表示第二组死亡任意怪物达到五只)
    /// </summary>
    /// <returns></returns>
    private List<SMonsterDeathConditon> GetGroupDeathCountForAnyMonsterConditions()
    {
        List<SMonsterDeathConditon> conditions = new List<SMonsterDeathConditon>();
        //结构:<group,death_count>
        Dictionary<int, int> groupDeathCountDic = new Dictionary<int, int>();
        foreach (var item in m_deathMonsters)
        {
            foreach (var monster in item.Value)
            {
                if (monster.isDestructible || monster.amount == 0) continue;

                if (!groupDeathCountDic.ContainsKey(monster.group)) groupDeathCountDic.Add(monster.group, 0);

                groupDeathCountDic[monster.group] += monster.amount;
            }
        }

        foreach (var item in groupDeathCountDic) conditions.Add(GetValidMonsterDeathCondition(-1, item.Key, item.Value));

        return conditions;
    }

    /// <summary>
    /// 指定怪物-所有分组-（全部死亡/怪物当前死亡数量）
    /// </summary>
    /// <returns></returns>
    private List<SMonsterDeathConditon> GetAllGroupDeathForMonsterConditions()
    {
        List<SMonsterDeathConditon> conditions = new List<SMonsterDeathConditon>();
        int currentMonsterCount = 0;
        foreach (var item in m_currentMonsters)
        {
            currentMonsterCount = GetMonsterAmountForAllGoup(item.Key, m_currentMonsters);
            //获取当前怪物已经死亡的数量
            int deathCount = GetMonsterAmountForAllGoup(item.Key, m_deathMonsters);

            //该怪物在所有分组中已经死完
            if (currentMonsterCount == 0 && deathCount != int.MaxValue) conditions.Add(GetValidMonsterDeathCondition(item.Key, -1, -1));

            if (deathCount != int.MaxValue && deathCount > 0) conditions.Add(GetValidMonsterDeathCondition(item.Key, -1, deathCount));
        }

        return conditions;
    }

    /// <summary>
    /// 指定怪物-指定分组-（全部死亡/怪物当前死亡数量）
    /// </summary>
    /// <returns></returns>
    private List<SMonsterDeathConditon> GetSpecifyMonsterSpecifyGroupConditions()
    {
        List<SMonsterDeathConditon> conditions = new List<SMonsterDeathConditon>();
        foreach (var item in m_currentMonsters)
        {
            List<MonsterData> data = item.Value;
            for (int i = 0; i < data.Count; i++)
            {
                //拿到当前怪物当前分组的死亡数量，然后添加进条件
                int deathCount = GetMonsterAmountForSpecifyGroup(data[i].monsterId, data[i].group, m_deathMonsters);

                //当前怪物当前分组已经全部死亡
                if (data[i].amount == 0 && deathCount != int.MaxValue) conditions.Add(GetValidMonsterDeathCondition(data[i].monsterId, data[i].group, -1));

                // 假设死亡n只，策划需要收到的死亡条件是1~n只怪物的
                if (deathCount != int.MaxValue)
                {
                    for (int j = 1; j <= deathCount; j++)
                    {
                        conditions.Add(GetValidMonsterDeathCondition(data[i].monsterId, data[i].group, j));
                    }
                }
            }
        }

        return conditions;
    }

    #endregion

    private SMonsterDeathConditon GetValidMonsterDeathCondition(int mon, int group, int amount)
    {
        SMonsterDeathConditon c = null;
        if (m_monsterDeathConditionPool.Count <= 0)
        {
            c = new SMonsterDeathConditon(mon,group,amount);
        }
        else
        {
            c = m_monsterDeathConditionPool.Dequeue();
            c.ResetData(mon,group,amount);
        }
        return c;
    }

    /// <summary>
    /// 获取在数据集中的制定怪物数量(忽略分组)
    /// </summary>
    /// <param name="monsterId"></param>
    /// <param name="dataDic"></param>
    /// <returns></returns>
    private int GetMonsterAmountForAllGoup(int monsterId, Dictionary<int, List<MonsterData>> dataDic)
    {
        List<MonsterData> monsters = null;
        if (!dataDic.TryGetValue(monsterId, out monsters)) return int.MaxValue;

        int count = 0;
        for (int i = 0; i < monsters.Count; i++)
        {
            count += monsters[i].amount >= 0 ? monsters[i].amount : 0;
        }
        return count;
    }

    /// <summary>
    /// 获取指定怪物指定分组的数量
    /// </summary>
    /// <param name="monsterId"></param>
    /// <param name="groupId"></param>
    /// <param name="originalDic">获取数量的数据源</param>
    /// <returns></returns>
    private int GetMonsterAmountForSpecifyGroup(int monsterId, int groupId, Dictionary<int, List<MonsterData>> originalDic)
    {
        List<MonsterData> monsters = null;
        if (!originalDic.TryGetValue(monsterId, out monsters)) return int.MaxValue;

        for (int i = 0; i < monsters.Count; i++)
        {
            if (monsters[i].group == groupId)
                return monsters[i].amount;
        }

        return int.MaxValue;
    }

    /// <summary>
    /// 清空当前所有的怪物条件
    /// </summary>
    private void ClearAllMonsterCondition()
    {
        int checkIndex = 0;
        while (checkIndex != m_currentCacheCondition.Count)
        {
            //对于符合条件的元素只删除元素，不增加检索索引
            if (m_currentCacheCondition[checkIndex].type == SceneEventInfo.SceneConditionType.MonsterDeath)
            {
                //移进缓冲池
                m_monsterDeathConditionPool.Enqueue(m_currentCacheCondition[checkIndex] as SMonsterDeathConditon);
                m_currentCacheCondition.RemoveAt(checkIndex);
            }
            else
            {
                checkIndex++;
            }
        }
    }

    /// <summary>
    /// 当抛出了创建怪物事件的时候，必须在该脚本执行此方法
    /// </summary>
    /// <param name="behaviour"></param>
    private void AddMonster(SCreateMonsterBehaviour behaviour)
    {
        if (behaviour == null||behaviour.type != SceneEventInfo.SceneBehaviouType.CreateMonster) return;

        if (!m_currentMonsters.ContainsKey(behaviour.monsterId)) m_currentMonsters.Add(behaviour.monsterId, new List<MonsterData>());
        MonsterInfo mon = ConfigManager.Get<MonsterInfo>(behaviour.monsterId);
        if (!mon) Logger.LogError("monster id is {0} connot be finded",behaviour.monsterId);
        
        List<MonsterData> monsters = m_currentMonsters[behaviour.monsterId];
        bool isContain = false;
        for (int i = 0; i < monsters.Count; i++)
        {
            if (monsters[i].group == behaviour.group)
            {
                monsters[i].amount++;
                isContain = true;
                break;
            }
        }

        //当前没有该怪物的信息，直接添加
        if (!isContain)
        {
            MonsterData monster = new MonsterData();
            monster.monsterId = behaviour.monsterId;
            monster.group = behaviour.group;
            monster.amount++;
            monster.monsterType = mon ? mon.monsterType : EnumMonsterType.NormalType;
            monsters.Add(monster);
        }
#if PVE_EVENT_LOG
        StringBuilder s = new StringBuilder();
        s.AppendFormat("创建怪物 {0}",behaviour.ToString());
        s.AppendLine();
        
        foreach (var item in m_currentMonsters)
        {
            foreach (var data in item.Value)
            {
                s.AppendFormat("怪物id:{0} -怪物分组：{1} -怪物数量{2}", data.monsterId, data.group, data.amount);
                s.AppendLine();
            }
        }
        Logger.LogDetail(s.ToString());
#endif
    }

    #endregion

    #region 计时器相关

    private void AddTimer(SSceneBehaviourBase behaviour)
    {
        SStartCountDownBehaviour b = behaviour as SStartCountDownBehaviour;
        TimerData td = null;
        for (int i = 0; i < m_pveTimerList.Count; i++)
        {
            if (m_pveTimerList[i].timerId == b.timerId && m_pveTimerList[i].type == EnumPVETimerType.NormalTimer)
            {
                td = m_pveTimerList[i];
                td.amount = b.amount * 0.001;
                td.isShow = b.showUI;
                break;
            }
        }

        if (td == null)
        {
            td = new TimerData(b.behaviour.parameters);

            //register count down--ui display
            td.showCountDown = (time) =>
            {
                DispatchModuleEvent(EventCountDown, time);
            };

            //register count down end callback
            td.countDownEnd = (timeId) =>
            {
                AddCondition(new SCountDownEndConditon(timeId));
            };

            m_pveTimerList.Add(td);
        }

        td.CheckShowCountDownCallback();
    }

    private void DelTimer(SSceneBehaviourBase behaviour)
    {
        SDelTimerBehaviour b = behaviour as SDelTimerBehaviour;
        for (int i = 0; i < m_pveTimerList.Count; i++)
        {
            if (m_pveTimerList[i].timerId == b.timerId)
            {
                if (m_pveTimerList[i].isShow)
                {
                    DispatchModuleEvent(EventCountDown, 0);
                }

                m_pveTimerList.RemoveAt(i);
                break;
            }
        }
    }

    private void RefreshTimerValue(SSceneBehaviourBase behaviour)
    {
        SAddTimerValueBehaviour b = behaviour as SAddTimerValueBehaviour;
        double timerAmount = b.amount * 0.001;
        double absoluteValue = b.absoluteValue * 0.001;

        for (int i = 0; i < m_pveTimerList.Count; i++)
        {
            if (m_pveTimerList[i].timerId == b.timerId)
            {
                m_pveTimerList[i].amount = absoluteValue > 0 ? absoluteValue : m_pveTimerList[i].amount + timerAmount;

                //如果绑定了UI则直接发送UI事件
                if (m_pveTimerList[i].isShow)
                {
                    DispatchModuleEvent(EventCountDown, m_pveTimerList[i].amountInt);
                }
                break;
            }
        }
    }

    private void UpdateCountDownTimer(int diff)
    {
        if (!Level.current.isPvE || pauseCountDown) return;

        var deltaTime = diff * 0.001;
        modulePVE.AddPveGameData(EnumPVEDataType.GameTime, (float)deltaTime);
        //DispatchModuleEvent(EventCountDown, (int)modulePVE.GetPveGameData(EnumPVEDataType.GameTime));

        for (int i = 0; i < m_pveTimerList.Count; i++)
        {
            TimerData data = m_pveTimerList[i];
            if (data.amount <= 0) continue;

            data.OnUpdate(deltaTime);
        }
    }

    #endregion

    #region 计数器相关

    private void ClearAllCounterCondition()
    {
        int checkIndex = 0;
        while (checkIndex != m_currentCacheCondition.Count)
        {
            //对于符合条件的元素只删除元素，不增加检索索引
            if (m_currentCacheCondition[checkIndex].type == SceneEventInfo.SceneConditionType.CounterNumber)
            {
                m_currentCacheCondition.RemoveAt(checkIndex);
            }
            else
            {
                checkIndex++;
            }
        }
    }
    #endregion

    #region 怪物血量百分比相关

    /// <summary>
    ///  怪物血量条件会被复用，不会考虑复数个怪物的情况,由策划配置来保障。
    /// </summary>
    /// <param name="hpCondition"></param>
    private void AddMonsterHPCondition(SMonsterHPLessConditon condition)
    {
        var lastCondition = GetMonsterHpCondition(condition);

        if (lastCondition == null) m_currentCacheCondition.Add(condition);
        else lastCondition.hp = condition.hp;
    }

    private void AddHitTimesCondition(SHitTimesConditon condition)
    {
        SHitTimesConditon c = null;
        foreach (var item in m_currentCacheCondition)
        {
            if (item.type == SceneEventInfo.SceneConditionType.HitTimes )
            {
                c = item as SHitTimesConditon;
                if(c.logicID == condition.logicID)
                {
                    break;
                }
                c = null;
            }
        }
        if (c == null) m_currentCacheCondition.Add(condition);
        else c.times += condition.times;
    }

    /// <summary>
    /// 玩家血量条件
    /// </summary>
    /// <param name="condition"></param>
    private void AddPlayerHPCondition(SPlayerHPLessConditon condition)
    {
        SPlayerHPLessConditon c = null;
        foreach (var item in m_currentCacheCondition)
        {
            if (item.type == SceneEventInfo.SceneConditionType.PlayerHPLess)
            {
                c = item as SPlayerHPLessConditon;
                break;
            }
        }
        if (c == null) m_currentCacheCondition.Add(condition);
        else c.hp = condition.hp;
    }

    /// <summary>
    /// 获取当前已经存在的血量条件，循环复用
    /// </summary>
    /// <param name="monsterId"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    private SMonsterHPLessConditon GetMonsterHpCondition(SMonsterHPLessConditon hp)
    {
        foreach (var item in m_currentCacheCondition)
        {
            if (item.type != SceneEventInfo.SceneConditionType.MonsterHPLess) continue;

            SMonsterHPLessConditon c = item as SMonsterHPLessConditon;
            if (c.monsterId == hp.monsterId && c.group == hp.group) return c;
        }

        return null;
    }

    #endregion

    #region 触发器相关

    private void ClearSameTriggerCondition(SEnterTriggerConditon c)
    {
        for (int i = 0,count = m_currentCacheCondition.Count; i < count; i++)
        {
            SSceneConditionBase oc = m_currentCacheCondition[i];
            if (oc.type == c.type && c.triggerId == oc.GetIntParames(0))
            {
                m_currentCacheCondition.Remove(oc);
                break;
            }
        }
    }

    #endregion

    #region 分发事件

    /// <summary>
    /// 对当前场景的条件进行检索，如果条件符合就抛出事件，并且移除掉会使用的条件和行为
    /// </summary>
    /// <returns></returns>
    private List<SSceneBehaviourBase> GetValidBehaviour()
    {
#if PVE_EVENT_LOG
        Logger.LogDetail("**************************************************************************");
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("current has condition : ");
        foreach (var item in m_currentCacheCondition) sb.AppendLine(item.ToString());
        Logger.LogDetail(sb.ToString());
        Logger.LogDetail("**************************************************************************");
#endif
        if(m_NeedDisableCondition != null && m_NeedDisableCondition.Count > 0)
        {
            RealDeleteCondition();
        }
        List<SSceneBehaviourBase> behavious = new List<SSceneBehaviourBase>();

        List<SceneEventData> needRemoveEvents = new List<SceneEventData>();
        bool isSuccess = false;
        foreach (var item in eventList)
        {
            isSuccess = true;
            for (int i = 0; i < item.conditions.Count; i++)
            {
                if (!IsContainCondition(m_currentCacheCondition, item.conditions[i]))
                {
                    isSuccess = false;
                    break;
                }
                else
                {
                    if(item.conditions[i].isEnable == false)
                    {
                        isSuccess = false;
                    }
                }
            }

            if (isSuccess)
            {
                //移除已经使用过得条件
                for (int j = 0; j < item.conditions.Count; j++)
                {
                    RemoveCondition(m_currentCacheCondition, item.conditions[j]);
                }
                item.DecreseLimitTime();
                if(item.needRemove) needRemoveEvents.Add(item);
                behavious.AddRange(item.behaviours);
            }
        }

#if PVE_EVENT_LOG
        sb.Remove(0, sb.Length);
        sb.AppendLine("current find valid behaviour :");
        foreach (var c in behavious) sb.AppendLine(c.ToString());
        Logger.LogInfo(sb.ToString());
        Logger.LogDetail("-----------------------------------------------------");
#endif

        //当找到了符合条件的事件,则移除这些事件
        if (needRemoveEvents.Count > 0)
        {
            foreach (var item in needRemoveEvents)
            {
                eventList.Remove(item);
            }
        }

        return behavious;
    }

    /// <summary>
    /// 判断缓存条件是否已经存储了检测条件
    /// </summary>
    /// <param name="cacheConditions">存储了SSceneConditionBase 的派生类，通过代码创建</param>
    /// <param name="checkCondition">从配置表中读取的条件，全部是SSceneConditionBase</param>
    /// <returns></returns>
    private bool IsContainCondition(List<SSceneConditionBase> cacheConditions, SSceneConditionBase checkCondition)
    {
        bool isContain = false;
        foreach (var item in cacheConditions)
        {
            if(item.IsSameCondition(checkCondition))
            {
                isContain = true;
                break;
            }
        }

        return isContain;
    }

    private void RemoveCondition(List<SSceneConditionBase> conditions, SSceneConditionBase checkCondition)
    {
        for (int i = 0; i < conditions.Count; i++)
        {
            if(conditions[i].IsSameCondition(checkCondition))
            {
                conditions.RemoveAt(i);
                break;
            }
        }
    }

    /// <summary>
    ///  分发事件
    /// </summary>
    /// <param name="behaviours"></param>
    private void DispatchBehaviour(List<SSceneBehaviourBase> behaviours)
    {
        if (behaviours == null || behaviours.Count == 0) return;

#if AI_LOG
        System.Text.StringBuilder s = new System.Text.StringBuilder();
        s.AppendLine("DispatchBehaviour handle,all behaviours has:");
        foreach (var item in behaviours) s.AppendLine(item.ToString());

        Module_AI.LogBattleMsg(null, s.ToString());
#endif

        foreach (var item in behaviours)
        {
            if (item == null) continue;
            item.HandleBehaviour();
        }
    }

    private void DispathPVEEvent(SSceneBehaviourBase b)
    {
        DispatchModuleEvent(b.eventName, b);
    }

    private void CreateMonster(SSceneBehaviourBase b)
    {
        //添加怪物操作(该脚本的方法)
        AddMonster(b as SCreateMonsterBehaviour);
        DispathPVEEvent(b);
    }

    private void OnStageOver(SSceneBehaviourBase b)
    {
        DispathPVEEvent(b);
        //关卡完成时，清空数据，关闭计时器
        ClearAll();
        isEnd = true;
    }

    private void CheckStageFirstEnter(SSceneBehaviourBase b)
    {
        //是否是首次通关
        AddCondition(new SStageFirstTimeConditon(modulePVE.isFirstEnterStage));

        //是否是首次进入
        AddCondition(new SEnterForFirstTimeConditon(modulePVE.enterForFirstTime));
    }

    private void BackToHome(SSceneBehaviourBase b)
    {
        if (Level.current is Level_PVE) (Level.current as Level_PVE).SetLevelBehaviourCanDestory();
        Game.GoHome();
    }

    private void OnStartGuide(SSceneBehaviourBase be)
    {
        SStartGuideBehaviour b = be as SStartGuideBehaviour;
        GuideInfo info = ConfigManager.Get<GuideInfo>(b.guideId);
        if (info)
        {
            Module_Guide.ShowGuide(info);
            DispathPVEEvent(b);
        }
        else Logger.LogError("pve event:[StartGuide]----guide id = {0} cannot be loaded", id);
    }
    private void OnStartStory(SSceneBehaviourBase be)
    {
        SStartStoryBehaviour b = be as SStartStoryBehaviour;
        Module_Story.ShowStory(b.plotId, b.storyType, false);
    }

    private void HandleCounterBehaviour(SSceneBehaviourBase be)
    {
        SOperatingCounterBehaviour b = be as SOperatingCounterBehaviour;

        if (!m_counterDic.ContainsKey(b.counterId)) m_counterDic.Add(b.counterId, 0);
        m_counterDic[b.counterId] += b.numberChange;
        AddCondition(new SCounterNumberConditon(b.counterId, m_counterDic[b.counterId]));
    }
	
    private void PlayBossAudio(SSceneBehaviourBase be)
    {
        SPlayAudioBehaviour b = be as SPlayAudioBehaviour;
        if (!string.IsNullOrEmpty(b.audioName)) AudioManager.PlayAudio(b.audioName, b.audioType, b.loop);
    }

    private void StopBossAudio(SSceneBehaviourBase be)
    {
        SStopAudioBehaviour b = be as SStopAudioBehaviour;
        if (!string.IsNullOrEmpty(b.audioName)) AudioManager.Stop(b.audioName);
    }

    private void OnBossComing(SSceneBehaviourBase be)
    {
        SBossComingBehaviour b = be as SBossComingBehaviour;

        var td = new TimerData(b.timerId, b.amount, 0, EnumPVETimerType.BossComing);
        //register count down end callback
        td.countDownEnd = (timeId) =>
        {
            AddCondition(new SBossComingEndConditon(timeId));
        };

        m_pveTimerList.Add(td);
        DispathPVEEvent(b);
    }

    /// <summary>
    /// 删除condition
    /// </summary>
    /// <param name="be"></param>
    private void OnDeleteCondition(SSceneBehaviourBase be)
    {
        SDeleteConditionBehaviour b = be as SDeleteConditionBehaviour;
        if (b == null)
            return;
        m_NeedDisableCondition.Add(b.conditionId);
    }

    /// <summary>
    /// 修改条件amount 值
    /// </summary>
    /// <param name="be"></param>
    private void OnIncreaseConditionAmount(SSceneBehaviourBase be)
    {
        SIncreaseConditionAmountBehaviour b = be as SIncreaseConditionAmountBehaviour;
        if (b == null)
            return;
        //increase amount 
        int conditionId = b.conditionId;
        foreach (var item in eventList)
        {
            for (int i = 0; i < item.conditions.Count; ++i)
            {
                if (item.conditions[i] != null && item.conditions[i].conditionId == conditionId)
                {
                    if(item.conditions[i].type == SceneEventInfo.SceneConditionType.MonsterDeath)
                    {
                        int amount = item.conditions[i].GetIntParames(2) + b.amount;
                        item.conditions[i].SetIntParames(2, amount);
                    }
                    break;
                }
            }
        }

    }

    /// <summary>
    /// 
    /// </summary>
    private void RealDeleteCondition()
    {
        int len = m_NeedDisableCondition.Count;
        int conditionId = 0;
        for (int j = 0; j < len; j++)
        {
            conditionId = m_NeedDisableCondition[j];
            if (eventList != null)
            {
                foreach (var item in eventList)
                {
                    for (int i = 0; i < item.conditions.Count; ++i)
                    {
                        if (item.conditions[i] != null && item.conditions[i].conditionId == conditionId)
                        {

                            item.conditions[i].EnableCondition(false);
                        }
                    }
                }

                if (m_currentCacheCondition != null)
                {
                    for (int idx = 0; idx < m_currentCacheCondition.Count; ++idx)
                    {
                        if (m_currentCacheCondition[idx] != null && m_currentCacheCondition[idx].conditionId == conditionId)
                        {
                            m_currentCacheCondition[idx].EnableCondition(false);
                        }
                    }
                }
            }
        }

        m_NeedDisableCondition.RemoveRange(0, len);
    }

    private void OnShowMessage(SSceneBehaviourBase be)
    {
        if (be.type != SceneEventInfo.SceneBehaviouType.ShowMessage || !(be is SShowMessageBehaviour)) return;

        m_showMessageCache.Enqueue(be as SShowMessageBehaviour);
    }

    public SShowMessageBehaviour GetShowMessageBehaviour()
    {
        if (validCheckShowMsgbehaviour) return m_showMessageCache.Dequeue();

        return null;
    }

    #endregion

    #region register behaviour

    public SSceneBehaviourBase CreateSSceneBehaviour(SceneEventInfo.SceneBehaviour b)
    {
        switch (b.sceneBehaviorType)
        {
            case SceneEventInfo.SceneBehaviouType.CreateMonster:        return new SCreateMonsterBehaviour(b, CreateMonster);
            case SceneEventInfo.SceneBehaviouType.KillMonster:          return new SKillMonsterBehaviour(b, DispathPVEEvent);
            case SceneEventInfo.SceneBehaviouType.StartCountDown:       return new SStartCountDownBehaviour(b, AddTimer);
            case SceneEventInfo.SceneBehaviouType.AddTimerValue:        return new SAddTimerValueBehaviour(b, RefreshTimerValue);
            case SceneEventInfo.SceneBehaviouType.DelTimer:             return new SDelTimerBehaviour(b, DelTimer);
            case SceneEventInfo.SceneBehaviouType.StageClear:           return new SStageClearBehaviour(b, OnStageOver);
            case SceneEventInfo.SceneBehaviouType.StageFail:            return new SStageFailBehaviour(b, OnStageOver);
            case SceneEventInfo.SceneBehaviouType.AddBuffer:            return new SAddBufferBehaviour(b,DispathPVEEvent);
            case SceneEventInfo.SceneBehaviouType.StartStoryDialogue:   return new SStartStoryBehaviour(b, OnStartStory);
            case SceneEventInfo.SceneBehaviouType.ShowMessage:          return new SShowMessageBehaviour(b, OnShowMessage);
            case SceneEventInfo.SceneBehaviouType.OperatingCounter:     return new SOperatingCounterBehaviour(b, HandleCounterBehaviour);
            case SceneEventInfo.SceneBehaviouType.LeaveMonster:         return new SLeaveMonsterBehaviour(b,DispathPVEEvent);
            case SceneEventInfo.SceneBehaviouType.SetState:             return new SSetStateBehaviour(b,DispathPVEEvent);
            case SceneEventInfo.SceneBehaviouType.CheckStageFirstTime:  return new SSetStateBehaviour(b, DispathPVEEvent);
            case SceneEventInfo.SceneBehaviouType.AIPauseState:         return new SAIPauseStateBehaviour(b, Module_AI.HandlePauseMonsterData);
            case SceneEventInfo.SceneBehaviouType.BackToHome:           return new SBackToHomeBehaviour(b,BackToHome);
            case SceneEventInfo.SceneBehaviouType.StartGuide:           return new SStartGuideBehaviour(b, OnStartGuide);
            case SceneEventInfo.SceneBehaviouType.PlayAudio:            return new SPlayAudioBehaviour(b, PlayBossAudio);
            case SceneEventInfo.SceneBehaviouType.StopAudio:            return new SStopAudioBehaviour(b, StopBossAudio);
            case SceneEventInfo.SceneBehaviouType.BossComing:           return new SBossComingBehaviour(b, OnBossComing);
            case SceneEventInfo.SceneBehaviouType.TransportScene:       return new STransportSceneBehaviour(b, DispathPVEEvent);
            case SceneEventInfo.SceneBehaviouType.CreateTrigger:        return new SCreateTriggerBehaviour(b, DispathPVEEvent);
            case SceneEventInfo.SceneBehaviouType.OperateTrigger:       return new SOperateTriggerBehaviour(b, DispathPVEEvent);
            case SceneEventInfo.SceneBehaviouType.OperateSceneArea:     return new SOperateSceneAreaBehaviour(b,DispathPVEEvent);
            case SceneEventInfo.SceneBehaviouType.CreateSceneActor:     return new SCreateSceneActorBehaviour(b,DispathPVEEvent);
            case SceneEventInfo.SceneBehaviouType.OperateSceneActor:    return new SOperateSceneActorBehaviour(b, DispathPVEEvent);
            case SceneEventInfo.SceneBehaviouType.DelSceneActorEvent:   return new SDelSceneActorEventBehaviour(b, DispathPVEEvent);
            case SceneEventInfo.SceneBehaviouType.BuildRandom:          return new SBuildRandomBehaviour(b, OnBuildRandomValue);
            case SceneEventInfo.SceneBehaviouType.RebuildRandom:        return new SRebuildRandomBehaviour(b, OnRebuildRandomValue);
            case SceneEventInfo.SceneBehaviouType.MoveMonsterPos:       return new SMoveMonsterPosBehaviour(b, DispathPVEEvent);
            case SceneEventInfo.SceneBehaviouType.CreateAssistant:      return new SCreateAssistantBehaviour(b, DispathPVEEvent);
            case SceneEventInfo.SceneBehaviouType.CreateAssistantNpc:   return new SCreateAssistantNpcBehaviour(b, DispathPVEEvent);
            case SceneEventInfo.SceneBehaviouType.DeleteCondition:      return new SDeleteConditionBehaviour(b, OnDeleteCondition);
            case SceneEventInfo.SceneBehaviouType.IncreaseConditionAmount: return new SIncreaseConditionAmountBehaviour(b, OnIncreaseConditionAmount);

            //创建小怪行为比较特殊，因为需要从召唤者的位置开始计算，而不能直接派发事件，所以只能特殊处理
            case SceneEventInfo.SceneBehaviouType.CreateLittle:         return new SCreateLittleBehaviour(b, null);
        }
        return null;
    }

    #endregion

    #region 动作帧事件相关

    public void RegisterFrameEvent(Creature c,int frameEventId)
    {
        if (!c) return;

        var fe = ConfigManager.Get<SceneFrameEventInfo>(frameEventId);
        if(!fe)
        {
            Logger.LogError("cannot find a valid SceneFrameEventInfo with id {0}", frameEventId);
        }

        if (!m_frameEventDic.ContainsKey(c)) m_frameEventDic.Add(c,null);
        m_frameEventDic[c] = fe.Clone() as SceneFrameEventInfo;
        c.AddEventListener(CreatureEvents.ENTER_STATE, OnMonsterCheckFrameEvent);
    }

    public void RemoveFrameEvent(Creature c)
    {
        if (!c) return;

        if (!m_frameEventDic.ContainsKey(c)) return;

        m_frameEventDic.Remove(c);
        c.RemoveEventListener(CreatureEvents.ENTER_STATE, OnMonsterCheckFrameEvent);
        RemoveFrameEventFromCheckList(c);
    }

    /// <summary>
    /// 还有可使用的帧事件
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    private bool HaveAnyValidFrameEvent(Creature c)
    {
        if (!m_frameEventDic.ContainsKey(c)) return false;

        var info = m_frameEventDic.Get(c);
        if (!info) return false;

        bool valid = false;
        foreach (var item in info.eventItems)
        {
            if (item.haveRestTimes)
            {
                valid = true;
                break;
            }
        }

        return valid;
    }

    private void OnMonsterCheckFrameEvent(Event_ e)
    {
        Creature c = e.sender as Creature;

        if (!c || c.health == 0 || !c.gameObject || !c.gameObject.activeInHierarchy || !c.currentState) return;

        SceneFrameEventInfo info = m_frameEventDic.Get(c);
        if (!info) return;
        
        var frameItem = info.GetTarFrameEventItem(c.currentState);
        if (frameItem == null || !frameItem.haveRestTimes)
        {
            //当前帧没有事件，移除之前的事件
            RemoveFrameEventFromCheckList(c);
            return;
        }

        //顶替帧事件
        AddFrameEventToCheckList(c, frameItem);
    }

    private void AddFrameEventToCheckList(Creature c, SceneFrameEventInfo.SceneFrameEventItem item)
    {
        if (!c || item == null) return;

        FrameEventData data = m_checkFrameEvents.Find(o=>o.IsValidEventForCreature(c));

        if (data == null)
        {
            data = new FrameEventData();
            data.creature = c;
            data.frameEvent = item;
            data.checkState = true;
            m_checkFrameEvents.Add(data);
            m_checkFrameEvents.Sort((a, b) => a.creatureRoleId < b.creatureRoleId ? -1 : 1);
        }
        else if(data.frameEvent != item)
        {
            data.frameEvent = item;
            data.checkState = true;
        }
        //DebugFrameList();
    }

    private void DebugFrameList()
    {
        foreach (var item in m_checkFrameEvents)
        {
            Logger.LogInfo(item.ToString());
        }
    }

    private void RemoveFrameEventFromCheckList(Creature c)
    {
        if (m_checkFrameEvents == null || m_checkFrameEvents.Count == 0) return;

        for (int i = 0; i < m_checkFrameEvents.Count; i++)
        {
            if (m_checkFrameEvents[i].IsValidEventForCreature(c))
            {
                m_checkFrameEvents.RemoveAt(i);
                break;
            }
        }

        //DebugFrameList();
    }

    private void CheckFrameEvent()
    {
        if (m_checkFrameEvents == null || m_checkFrameEvents.Count == 0) return;

        for (int i = 0; i < m_checkFrameEvents.Count; i++)
        {
            if (m_checkFrameEvents[i].TriggerFrameEvent()) DispatchFrameEvent(m_checkFrameEvents[i]);
        }
    }

    private void DispatchFrameEvent(FrameEventData data)
    {
        if (data == null || !data.validFrameEventData) return;

        data.checkState = false;
        data.frameEvent.DecreseTimes();
        List<SSceneBehaviourBase> list = new List<SSceneBehaviourBase>();
        foreach (var item in data.frameEvent.behaviours)
        {
            SSceneBehaviourBase b = CreateSSceneBehaviour(item);

            //移动位置，设值
            if (b.type == SceneEventInfo.SceneBehaviouType.MoveMonsterPos) (b as SMoveMonsterPosBehaviour).creature = data.creature;

            //特殊处理little的事件分发
            if (b.type == SceneEventInfo.SceneBehaviouType.CreateLittle) DispathCreateLittle(b,data.creature);
            else list.Add(b);
        }

        DispatchBehaviour(list);
    }

    private void DispathCreateLittle(SSceneBehaviourBase b, Creature c)
    {
        if (b is SCreateLittleBehaviour)
        {
            (b as SCreateLittleBehaviour).createPos = c ? c.position_ : Vector3_.zero;
            DispathPVEEvent(b);
        }
    }

    #endregion

    #region random event

    private void OnBuildRandomValue(SSceneBehaviourBase b)
    {
        SBuildRandomBehaviour be = b as SBuildRandomBehaviour;
        if (be == null) return;
        AddRandomInfoCondition(Level_PVE.CurrentSceneEventData.BuildRandom(be.randomId, be.maxValue));
    }

    private void OnRebuildRandomValue(SSceneBehaviourBase b)
    {
        SRebuildRandomBehaviour be = b as SRebuildRandomBehaviour;
        if (be == null) return;
        AddRandomInfoCondition(Level_PVE.CurrentSceneEventData.BuildRandom(be.randomId, be.maxValue, true));
    }

    private void AddRandomInfoCondition(SRandomInfoConditon rCondition)
    {
        if(null == rCondition) return;
        //延后处理
        m_secondTriggerCheck = true;
        bool contain = false;
        foreach (var item in m_currentCacheCondition)
        {
            if (item.type != SceneEventInfo.SceneConditionType.RandomInfo) continue;

            SRandomInfoConditon c = item as SRandomInfoConditon;
            if (c.randomId == rCondition.randomId)
            {
                contain = true;
                c.value = rCondition.value;
                break;
            }
        }

        if(!contain) AddCondition(rCondition, false);
    }

    #endregion

    #region monster attack condition

    public void AddMonsterAttackCondition(MonsterCreature creature)
    {
        if (!creature) return;
        
        foreach (var item in m_currentCacheCondition)
        {
            if (item.type != SceneEventInfo.SceneConditionType.MonsterAttack) continue;

            SMonsterAttackConditon c = item as SMonsterAttackConditon;
            //条件已经存在未使用就不添加了
            if (creature.monsterId == c.monsterId && creature.monsterGroup == c.group) return;
        }

        AddCondition(new SMonsterAttackConditon(creature.monsterId, creature.monsterGroup));
    }

    #endregion

    /// <summary>
    /// 新增或者修改怪物受击次数条件
    /// </summary>
    /// <param name="creature"></param>
    public void AddHitTimesCondition(MonsterCreature creature)
    {
        //@todo 
        if (!creature) return;
        SHitTimesConditon condition = null;
        foreach (var item in m_currentCacheCondition)
        {
            if (item.type != SceneEventInfo.SceneConditionType.HitTimes) continue;

            SHitTimesConditon c = item as SHitTimesConditon;
            //策划强烈要求，用怪物的monsterId来记录怪物的受击次数
            if (creature.monsterId == c.logicID) { condition = c; break; }
        }
        if(condition == null )
        {
            condition = new SHitTimesConditon(1, creature.monsterId);
            AddCondition(condition);
        }
        else
        {
            condition.times += 1;
        }
        CheckValidBehaviours();
    }

    public override void OnUpdate(int diff)
    {
        base.OnUpdate(diff);
        UpdateCountDownTimer(diff);
        CheckFrameEvent();
    }
    
}
