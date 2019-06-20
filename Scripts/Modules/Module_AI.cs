/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-07-31
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
//using Random = AIRandom;
public class Module_AI : Module<Module_AI>
{
    #region custom class

    private class PauseMonsterData
    {
        public int monsterId;
        public int groupId;

        public PauseMonsterData()
        {

        }

        public PauseMonsterData(int mon,int group)
        {
            monsterId = mon;
            groupId = group;
        }

        public bool IsContain(MonsterCreature monster)
        {
            return IsContain(monster.monsterId,monster.monsterGroup);
        }
        
        public bool IsContain(int mon, int group)
        {
            if(mon == -1 || group == -1)
            {
                Logger.LogWarning("monster data that be checked is invalid,monsterId = {0},groupid = {1}",monsterId,groupId);
                return false;
            }

            //if current creature group is -2,then we don't need pause this monster ai
            if (monsterId == -1 && groupId == -1) return group != -2;

            if (monsterId == -1) return groupId == group;
            if (groupId == -1) return monsterId == mon;
            return IsSame(mon,group);

        }

        public bool IsSame(int mon,int group)
        {
            return monsterId == mon && groupId == group;
        }
    }

    private class SelectAICreature
    {
        public Creature creature;
        /// <summary>
        /// 时间分为三个阶段
        /// selectTime > 0 选择AI倒计时
        /// selectTime = 0 已经就绪准备选择AI(如果只延迟一帧，那么直接设置selectTime = 0)
        /// selectTime < 0 当前不能选择AI
        /// </summary>
        public double selectTime { get; private set; }

        #region 打印需要的数据
        public double duraction { get; private set; }
        #endregion

        public SelectAICreature(Creature c)
        {
            creature = c;
            SetSelectTimeByDuraction(0);
        }

        public SelectAICreature(Creature c,double time)
        {
            creature = c;
            SetSelectTimeByDuraction(time);
        }

        public void SetSelectTimeByDuraction(double time)
        {
            selectTime = time;

            FightRecordManager.RecordLog<LogDouble>(log =>
            {
                log.tag = (byte)TagType.AIselectTime;
                log.value = selectTime;
            });

#if AI_LOG
            duraction = 0;
            LogAIMsg();
#endif
        }

        public void OnSelectAIComplete(string tip = "")
        {
            selectTime = -1;

            FightRecordManager.RecordLog<LogDouble>(log =>
            {
                log.tag = (byte)TagType.AIselectTime;
                log.value = selectTime;
            });
#if AI_LOG
            LogAIMsg(tip,false);
#endif
        }

        public bool CheckSelectTimeCountDownEnd(double deltaTime)
        {
#if AI_LOG
            duraction += deltaTime;
#endif
            if (selectTime == 0) return true;

            selectTime -= deltaTime;

            FightRecordManager.RecordLog<LogDouble>(log =>
            {
                log.tag = (byte)TagType.SelectTime;
                log.value = selectTime;
            });

            if (selectTime < 0) selectTime = 0;
            return selectTime == 0;
        }

        private void LogAIMsg(string tip = "",bool init = true)
        {
            if(creature) moduleAI.LogAIMsg(creature,true,"[set next select ai time as {0}  reason : {1} {2}]",selectTime.ToString("F5"), tip, init ? "init set time" : string.Format("duraction : {0}",duraction));
        }
    }

    private class StratergyUsedCountData
    {
        public int distance;
        public int group;
        public string behaviourState;
        public int useCount;

        public StratergyUsedCountData(int dis,int gro,string state)
        {
            distance = dis;
            group = gro;
            behaviourState = state;
            useCount = 1;
        }
    }

    private class StratergyCache
    {
        public AIConfig.AIStratergy stratergy { get; private set; }
        public uint order { get; set; }

        public uint max { get { return stratergy == null ? 0 : stratergy.maxOrder; } }
        public uint min { get { return stratergy == null ? 0 : stratergy.minOrder; } }

        public StratergyCache(AIConfig.AIStratergy s)
        {
            ResetStratergy(s);
        }

        public void ResetStratergy(AIConfig.AIStratergy s)
        {
            stratergy = s;
            order = 1;
        }

        public bool IsSameCache(int disKey)
        {
            return stratergy != null && stratergy.distance == disKey;
        }

        public string GetDebugMsg()
        {
            return string.Format("[group -> {0}] [order -> {1}]", stratergy == null ? "null" : stratergy.group.ToString(), order);
        }
    }
    #endregion

    #region static function

    /// <summary>
    /// 返回ori在target的左边(-1)还是右边(1)
    /// </summary>
    /// <param name="ori"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static int GetDirection(Creature ori, Creature target)
    {
        if (ori == null || target == null || ori.x == target.x) return 0;
        if (Level.current != null && Level.current.isNormal)
            return ori.position.x < target.position.x ? -1 : 1;
        return ori.x < target.x ? -1 : 1;
    }

    public static void HandlePauseMonsterData(SSceneBehaviourBase pauseMonster)
    {
        SAIPauseStateBehaviour b = pauseMonster as SAIPauseStateBehaviour;
        instance._HandlePauseMonsterData(b);
    }
    #endregion

    #region static/const field
    /// <summary>
    /// 需要忽略的状态ID，2：StateUp ；7：StateUp2
    /// </summary>
    public static int[] IgnoreTurnStateIDs = new int[] { 2, 7 };
    public const string STATE_STAND_NAME = "StateStand";

    /// <summary>
    /// 锁敌阵营的映射关系
    /// </summary>
    private static readonly Dictionary<CreatureCamp, List<CreatureCamp>> LOCK_ENERMY_CAMP_DIC = new Dictionary<CreatureCamp, List<CreatureCamp>>
    {
        { CreatureCamp.PlayerCamp, new List<CreatureCamp> { CreatureCamp.MonsterCamp, CreatureCamp.NeutralCamp } },
        { CreatureCamp.NeutralCamp, new List<CreatureCamp> { CreatureCamp.PlayerCamp, CreatureCamp.MonsterCamp } },
        { CreatureCamp.MonsterCamp, new List<CreatureCamp> { CreatureCamp.PlayerCamp, CreatureCamp.NeutralCamp } },
    };
    #endregion

    #region field
    //每个怪物使用的原始AI (关键字定义为Creature是为了避免后期加入玩家AI)
    private Dictionary<Creature, AIConfig> m_monsterAIDic = new Dictionary<Creature, AIConfig>();
    //角色AI策略使用数量的保存字典
    private Dictionary<Creature, List<StratergyUsedCountData>> m_stratergyUseCountDic = new Dictionary<Creature, List<StratergyUsedCountData>>();
    public bool pauseAI { get; private set; }
    private Vector2_ m_aiBorder;
    private Dictionary<Creature, StratergyCache> m_stratergyCacheDic = new Dictionary<Creature, StratergyCache>();

    private Creature m_tempLockCre;
    private Creature m_tempTarCre;
    private StratergyCache m_tempStratergyCache;
    #endregion

    #region AI 相关容器

    /// <summary>
    /// 怪物的锁敌字典
    /// </summary>
    private Dictionary<Creature, Creature> m_lockEnermyDic = new Dictionary<Creature, Creature>();
    public Dictionary<Creature, Creature> lockEnermyDic { get { return m_lockEnermyDic; } }

    /// <summary>
    /// 怪物选择AI的时间
    /// 当time <= 0时，不对该怪物进行AI判断检测
    /// </summary>
    private List<SelectAICreature> m_selectAICreatureDic = new List<SelectAICreature>();

    /// <summary>
    /// 存放不同阵营玩家列表
    /// </summary>
    private Dictionary<CreatureCamp, List<Creature>> m_creatureCampDic = new Dictionary<CreatureCamp, List<Creature>>();

    /// <summary>
    /// 已经暂停AI的怪物列表
    /// </summary>
    private List<PauseMonsterData> m_pauseMonsters = new List<PauseMonsterData>();

    /// <summary>
    /// AI是否开启
    /// </summary>
    private bool m_isStartAI;

    public bool IsStartAI { get { return this.m_isStartAI; } }

    #endregion

    #region 策略使用数量字典相关

    private void AddStratergyCount(Creature c, AIConfig.AIStratergy stratergy, string state)
    {
        if (!m_stratergyUseCountDic.ContainsKey(c))
        {
            List<StratergyUsedCountData> useList = new List<StratergyUsedCountData>();
            m_stratergyUseCountDic.Add(c, useList);
        }

        bool contain = false;
        List<StratergyUsedCountData> l = m_stratergyUseCountDic[c];
        foreach (var item in l)
        {
            if (item.distance == stratergy.distance && item.group == stratergy.group && state.Equals(item.behaviourState))
            {
                item.useCount++;
                contain = true;
            }
        }

        if(!contain) l.Add(new StratergyUsedCountData(stratergy.distance, stratergy.group, state));
    }

    private int GetStratergyUsedCount(Creature c, AIConfig.AIStratergy stratergy, string state)
    {
        if (!m_stratergyUseCountDic.ContainsKey(c)) return -1;

        List<StratergyUsedCountData> l = m_stratergyUseCountDic[c];
        foreach (var item in l)
        {
            if (item.distance == stratergy.distance && item.group == stratergy.group && state.Equals(item.behaviourState)) return item.useCount;
        }

        return -1;
    }

    #endregion

    public void InitMonsterAI(List<MonsterCreature> monsters)
    {
        for (int i = 0, count = monsters.Count; i < count; i++)
        {
            if (!monsters[i] || !monsters[i].monsterInfo) continue;

            AddMonsterAI(monsters[i]);
        }
    }

    public void AddMonsterAI(MonsterCreature monster)
    {
        if (!monster || !monster.monsterInfo) return;

        AIConfig ai = ConfigManager.Get<AIConfig>(monster.monsterInfo.AIId);
        if (ai == null)
        {
            Logger.LogWarning("AIConfig can't find!please checkout the ai ID,ID = {0} , monsterId is {1}", monster.monsterInfo.AIId,monster.monsterInfo.ID);
            return;
        }

        //按照距离排序
        Array.Sort(ai.lockEnermyInfos, (a, b) => a.distance < b.distance ? -1 : 1);
        Array.Sort(ai.AIStratergies, (a, b) => a.distance < b.distance ? -1 : 1);

        FightRecordManager.RecordLog<LogDouble>(log =>
        {
            log.tag = (byte)TagType.AddMonsterAI;
            log.value = monster.Identify;
        });

        if (m_monsterAIDic.ContainsKey(monster)) m_monsterAIDic[monster] = ai;
        else m_monsterAIDic.Add(monster, ai);

        monster.AddEventListener(CreatureEvents.QUIT_STATE,        OnMonsterQuitState);
    }

    /// <summary>
    /// 挑选锁定目标
    /// </summary>
    /// <param name="monster"></param>
    /// <param name="enermyCreatures"></param>
    /// <returns></returns>
    public Creature GetLockEnermyCreature(Creature monster,List<Creature> enermyCreatures)
    {
        if (!m_monsterAIDic.ContainsKey(monster)) return null;


        FightRecordManager.RecordLog<LogEmpty>(l =>
        {
            l.tag = (byte)TagType.GetLockEnermyCreature;
        });

        Creature tar = null;
        AIConfig.LockEnermyInfo lastLockInfo = null;

        AIConfig aiConfig = m_monsterAIDic.Get(monster);
        for (int i = 0; i < enermyCreatures.Count; i++)
        {
            //死亡后的怪物不进入锁敌目标,18.01.22增加需求：无敌的目标也不进入筛选过程,关闭锁敌的怪物也不进行怪物挑选
            if (CheckIgnoreLockEnermy(enermyCreatures[i])) continue;

            int distance = GetDistance(monster, enermyCreatures[i]);

            FightRecordManager.RecordLog<LogInt>(log =>
            {
                log.tag = (byte)TagType.AIDistance;
                log.value = distance;
            });

            AIConfig.LockEnermyInfo lockInfo = aiConfig.GetLockEnermyInfo(distance);

            //如果找到了距离更近的锁敌策略则不做替换了
            if (lastLockInfo != null && lockInfo != null && lastLockInfo.distance < lockInfo.distance) continue;

            if (lockInfo != null)
            {
                //如果当前有切换目标的概率
                //int random = Random.Range(0, 100);
                int random = moduleBattle._Range(0, 100);
                if (random <= lockInfo.lockRate)
                {
                    tar = enermyCreatures[i];
                    lastLockInfo = lockInfo;
                }
            }
        }

        //如果没有符合条件的锁敌切换，就保持默认的锁敌
        return tar;
    }

    #region 获取AI

    public AIConfig.AIBehaviour GetAIBehaviour(Creature lockCreature, Creature selectAICre)
    {
        int distance = GetDistance(selectAICre, lockCreature,true);

        AIConfig creatureAI = m_monsterAIDic.Get(selectAICre);
        if (!creatureAI)
        {
            FightRecordManager.RecordLog<LogInt>(log =>
            {
                log.tag = (byte)TagType.monsterAIDic;
                log.value = m_monsterAIDic.Count;
            });

            return null;
        }

        //set cache
        m_tempLockCre = lockCreature;
        m_tempTarCre = selectAICre;

        int disKey = creatureAI.GetRealDistanceKey(distance);

#if AI_LOG
        LogAIMsg(selectAICre,true,"[real distance -> {0}] [AI distance key -> {1} ai is {2}]", distance, disKey,creatureAI.ToXml());
#endif

        AIConfig.AIBehaviour b = null;
        m_tempStratergyCache = m_stratergyCacheDic.Get(selectAICre);

        FightRecordManager.RecordLog< LogStratergySelect>(s =>
        {
            s.tag = (byte)TagType.StratergySelect;
            s.stratergyDistance = disKey;
            s.distance = distance;
        });


        //if distance key change or there havn't choose a ai stratergy,get new stratergy
        if (m_tempStratergyCache == null || !m_tempStratergyCache.IsSameCache(disKey)) b = GetNewStratergy(creatureAI,disKey);
        else b = GetStratergyFromCache();

#if AI_LOG
        //debug
        LogAIMsg(selectAICre,"[dis -> {0}] {1} [select ai:{2}]", disKey, m_tempStratergyCache == null ? "null" : m_tempStratergyCache.GetDebugMsg(), b == null ? "null" : b.ToXml());
        bool nextgroup = b != null && b.behaviourType == AIConfig.AIBehaviour.AIBehaviourType.ToNextGroup;
#endif

        //Check if you need to jump to the next AI stratergy group
        b = CheckForNextGroupStratergy(creatureAI, disKey, b);

#if AI_LOG
        //debug
        if(nextgroup) LogAIMsg(selectAICre, "[AIBehaviourType.ToNextGroup Handle] : [dis -> {0}] [group -> {1}] [order -> {2}] [select ai:{3}]", disKey, m_tempStratergyCache.stratergy.group, m_tempStratergyCache.order, b == null ? "null" : b.ToXml());
#endif
        ClearTempData();
        return b;
    }
    
    /// <summary>
    /// 返回当前的距离 -2 左边界，-1 右边界，dis(dis > 0) 正常的距离且单位为分米
    /// </summary>
    /// <param name="oriCreature">初始怪物</param>
    /// <param name="lockCreature">锁敌目标</param>
    /// <param name="checkDir">是否检测边界</param>
    /// <returns>距离，单位分米</returns>
    public int GetDistance(Creature oriCreature, Creature lockCreature, bool checkDir = false,bool log = true)
    {
        if(oriCreature is PetCreature && Level.current != null && Level.current.isNormal)
            return GetTransformDistance(oriCreature, lockCreature);

        double dis = (oriCreature.position_.x - lockCreature.position_.x) * 10000;
        if (dis < 0) dis = -dis;
        int realDis = Mathd.RoundToInt(dis);
        realDis /= 1000;

        if(log) LogAIMsg(oriCreature, "[{0} pos -> {1}, lock creature : {2} pos ->{3} distance ->{4}]", oriCreature.gameObject ? oriCreature.gameObject.name : oriCreature.uiName, oriCreature.position_.x, lockCreature.gameObject ? lockCreature.gameObject.name : lockCreature.uiName, lockCreature.position_.x,realDis);
        if (checkDir && !(oriCreature is PetCreature))
        {
            //check border
            int borderDis = CheckBorderDis(oriCreature, realDis);
            if (borderDis < 0) return borderDis;
        }

        FightRecordManager.RecordLog<LogCalcDistance>(l =>
        {
            l.tag = (byte)TagType.CalcDistance;
            l.originX = oriCreature.position_.x;
            l.lockX = lockCreature.position_.x;
            l.realDis = realDis;
        });

        return realDis;
    }

    /// <summary>
    /// 返回当前的Transform距离
    /// </summary>
    /// <param name="oriCreature">初始怪物</param>
    /// <param name="lockCreature">锁敌目标</param>
    /// <returns>距离，单位分米</returns>
    private int GetTransformDistance(Creature oriCreature, Creature lockCreature)
    {
         return Mathf.RoundToInt(Mathf.Abs(oriCreature.position.x - lockCreature.position.x) * 10) ;
    }

    /// <summary>
    /// 判断怪物是否在场景边界-2 左边界，-1 右边界，0,不在边界
    /// </summary>
    /// <param name="mon"></param>
    /// <returns></returns>
    private int CheckBorderDis(Creature mon, int realDis)
    {
        if (!mon || m_aiBorder.y <= m_aiBorder.x || realDis > GeneralConfigInfo.AIPMDis) return 0;

        if (mon.position_.x <= m_aiBorder.x) return -2;
        else if (mon.position_.x >= m_aiBorder.y) return -1;
        else return 0;
    }

    private AIConfig.AIBehaviour GetNewStratergy(AIConfig ai,int disKey,int group = 0)
    {
        //when we need get new stratergy for every time,we need reset temp stratergy cache first 
        m_tempStratergyCache?.ResetStratergy(null);

        LogAIMsg(m_tempTarCre, "[GetNewStratergy disKey -> {0} group -> {1}]", disKey, group);
        if (ai == null) return null;

        AIConfig.AIStratergy stratergy = ai.GetAIStratergy(disKey,group);
        if (stratergy == null) return null;

#if AI_LOG
        LogAIMsg(m_tempTarCre, "[GetNewStratergy success ,[ disKey -> {0} group -> {1}] stratergies  is {2}]", disKey, group, stratergy.stratergies == null ? "null" : stratergy.stratergies.ToXml());
#endif
        SaveCurrentStratergy(stratergy);
        return GetSingleStratergy();
    }

    private void SaveCurrentStratergy(AIConfig.AIStratergy stratergy)
    {
        if(!m_stratergyCacheDic.ContainsKey(m_tempTarCre)) m_stratergyCacheDic.Add(m_tempTarCre,new StratergyCache(stratergy));
        m_stratergyCacheDic[m_tempTarCre].ResetStratergy(stratergy);
        m_tempStratergyCache = m_stratergyCacheDic[m_tempTarCre];
    }

    private AIConfig.AIBehaviour GetSingleStratergy()
    {
        AIConfig.AIStratergy s = m_tempStratergyCache?.stratergy;
        if(s == null)
        {
            Logger.LogError("cache the stratergy is null,please check out.....");
            return null;
        }
        //havn't order stratergy or current order is rager than max order
        if (!s.hasOrderStatergy || m_tempStratergyCache.order > s.maxOrder) return GetRandomSingleStratergy();
        else
        {
            LogAIMsg(m_tempTarCre, "[GetOrderStratergy order is -> {0}]", m_tempStratergyCache.order);
            return s.GetOrderStratergy(m_tempStratergyCache.order)?.behaviour;
        }
    }
    
    private AIConfig.AIBehaviour GetRandomSingleStratergy()
    {
        AIConfig.AIBehaviour b = null;
        List<AIConfig.SingleAIStratergy> list = GetAllRandomStratergies();

        LogAIMsg(m_tempTarCre, "[GetRandomSingleStratergy random list count is -> {0} modulePVEEvent.isEnd -> {1}]", list.Count, modulePVEEvent.isEnd);
        if (list.Count <= 0 || modulePVEEvent.isEnd) return b;

        uint totalRate = 0;
        for (int i = 0; i < list.Count; i++)
        {
            list[i].minRate = totalRate;
            totalRate += list[i].rate;
            list[i].maxRate = totalRate;
        }

        //将总值放大，便于取到更加随机的数值
        uint maxRate = totalRate * 10;
        //just protect
        if (totalRate == 0) totalRate = 1;
        //uint randomRate = (uint)Random.Range(0, maxRate) % totalRate;
        uint randomRate = (uint)moduleBattle._Range(0, maxRate) % totalRate;

        for (int i = 0; i < list.Count; i++)
        {
            //符合条件的行为
            if (list[i].isValidRage(randomRate))
            {
                //增加字典中使用次数
                AddStratergyCount(m_tempTarCre,m_tempStratergyCache.stratergy, list[i].behaviour.state);
                b = list[i].behaviour;
                break;
            }
        }

#if AI_LOG
        StringBuilder debugS = new StringBuilder();
        debugS.AppendFormat("[totalRate is {0}; maxRate is {1}; random value is {2}]",totalRate,maxRate,randomRate);
        foreach (var item in list)
        {
            debugS.AppendLine();
            debugS.AppendFormat("[valid stratergy has: {0}_{1}; min Rate is {2}; max Rate is {3}]", item.behaviour.state, item.behaviour.behaviourType,item.minRate, item.maxRate);
        }
        LogAIMsg(m_tempTarCre, debugS.ToString());
#endif

        return b;
    }

    private List<AIConfig.SingleAIStratergy> GetAllRandomStratergies()
    {
        StateMachineState state = null;
        List<AIConfig.SingleAIStratergy> list = new List<AIConfig.SingleAIStratergy>();
        foreach (var item in m_tempStratergyCache.stratergy.stratergies)
        {
#if AI_LOG
            if(!string.IsNullOrEmpty(item.behaviour.state)) state = m_tempTarCre.stateMachine.GetState(item.behaviour.state);
            LogBattleMsg(m_tempTarCre,true, "GetAllRandomStratergies stratergy is {0} this stratergy use count is {1}, state is {2} state.coolingDown is {3} lastExitTime = {4} ", 
                item.ToXml(), GetStratergyUsedCount(m_tempTarCre, m_tempStratergyCache.stratergy, item.behaviour.state),state ? "valid":"null",state ? state.coolingDown : false, state ? state.lastExitTime : -1);
#endif

            //如果该条策略的可执行次数存在，并且该条策略
            if (item.repeatTimes >= 0 && item.repeatTimes <= GetStratergyUsedCount(m_tempTarCre, m_tempStratergyCache.stratergy, item.behaviour.state)) continue;
            
            //顺序执行的策略不进行随机选择 
            if (item.order > 0) continue;

            //如果状态不存在或者正在冷却，则不挑选该状态
            if(!string.IsNullOrEmpty(item.behaviour.state))
            {
                state = m_tempTarCre.stateMachine.GetState(item.behaviour.state);
                if (!state || state.coolingDown) continue;
            }

            //当前行为没有条件,直接添加
            if (item.conditions == null || item.conditions.Length == 0)
            {
                list.Add(item);
                continue;
            }
            //宠物的条件检测目标是宠物拥有者
            var checkTarget = m_tempTarCre;
            if (checkTarget is PetCreature)
                checkTarget = (checkTarget as PetCreature).ParentCreature;
            bool isValid = true;
            for (int j = 0; j < item.conditions.Length; j++)
            {
                #region ai condition check
                switch (item.conditions[j].conditionType)
                {
                    case AIConfig.AICondition.AIConditionType.HPHigh:
                        isValid = checkTarget.healthRateL > item.conditions[j].paramers[0] * 0.01;
                        break;

                    case AIConfig.AICondition.AIConditionType.HPLow:
                        isValid = checkTarget.healthRateL < item.conditions[j].paramers[0] * 0.01;
                        break;

                    case AIConfig.AICondition.AIConditionType.CheckAttackState:
                        isValid = m_tempLockCre.stateMachine.currentState.info.hasAttackBox;
                        break;

                    case AIConfig.AICondition.AIConditionType.HasBuff:
                        isValid = checkTarget.HasBuff(item.conditions[j].paramers[0]);
                        break;

                    case AIConfig.AICondition.AIConditionType.Direction:
                        isValid = checkTarget.direction == (CreatureDirection)item.conditions[j].paramers[0];
                        break;
                    case AIConfig.AICondition.AIConditionType.MoveState:
                        var b = checkTarget.PVEBehaviour;
                            isValid = b ? b.moveState == item.conditions[j].paramers[0] : checkTarget.moveState == item.conditions[j].paramers[0];
                        break;

                    case AIConfig.AICondition.AIConditionType.MonOnThePlayerLeft:
                        var left = checkTarget.position_.x < m_tempLockCre.position_.x ? 1 : 0;
                        isValid = left == item.conditions[j].paramers[0];
                        break;
                }
                #endregion

                if (!isValid) break;
            }

            //符合所有条件，就将该策略备选
            if (isValid) list.Add(item);
        }

        return list;
    }

    /// <summary>
    /// 继续沿用上一次的AI策略组
    /// </summary>
    /// <returns></returns>
    private AIConfig.AIBehaviour GetStratergyFromCache()
    {
        m_tempStratergyCache.order++;
        if(m_tempStratergyCache != null && m_tempStratergyCache.stratergy != null)
        {
            LogAIMsg(m_tempTarCre, "[GetStratergyFromCache disKey -> {0}; group -> {1}; current order = {2}; minOrder is{3}; maxOrder is {4}]", m_tempStratergyCache.stratergy.distance, m_tempStratergyCache.stratergy.group, m_tempStratergyCache.order, m_tempStratergyCache.stratergy.minOrder, m_tempStratergyCache.stratergy.maxOrder);
        }
        else
        {
            LogAIMsg(m_tempTarCre, "[GetStratergyFromCache error ,m_tempStratergyCache is {0} m_tempStratergyCache.stratergy is {1}]", m_tempStratergyCache == null ? "null" : "valid", m_tempStratergyCache.stratergy.distance, (m_tempStratergyCache == null || m_tempStratergyCache.stratergy == null) ? "null" : "valid");
        }
        return GetSingleStratergy();
    }

    private AIConfig.AIBehaviour CheckForNextGroupStratergy(AIConfig creatureAI, int dis,AIConfig.AIBehaviour b)
    {
        if (b == null || b.behaviourType != AIConfig.AIBehaviour.AIBehaviourType.ToNextGroup) return b;

        //get new group
        AIConfig.AIBehaviour newB = GetNewStratergy(creatureAI, dis, b.group);
        return newB ?? b;
    }

    private void ClearTempData()
    {
        m_tempLockCre = null;
        m_tempTarCre = null;
        m_tempStratergyCache = null;
    }

    #endregion

    #region AI日志输出

    #region ai log

#if AI_LOG
    
    private Dictionary<AILogCreature, StringBuilder> m_logAIDic = new Dictionary<AILogCreature, StringBuilder>();
    private bool isDebugAILog = false;
    //private StringBuilder m_totalAI = new StringBuilder();

    public bool IsContainLogCreature(Creature c)
    {
        return GetLogCreature(c) != null;
    }

    public AILogCreature GetLogCreature(Creature c)
    {
        foreach (var item in m_logAIDic)
        {
            if (item.Key != null && item.Key.IsSameCreature(c)) return item.Key;
        }
        return null;
    }
#endif

    public void LogAIMsg(Creature c,bool flag, string msg, params object[] p)
    {
#if AI_LOG
        if (isDebugAILog) Logger.LogInfo(msg, p);

        AILogCreature aic = GetLogCreature(c);
        if(aic == null)
        {
            aic = new AILogCreature(c);
            m_logAIDic.Add(aic, new StringBuilder());
        }

        StringBuilder s = m_logAIDic[aic];
        if (flag) s.AppendLine();
        s.AppendFormat("[{0}]--> creature : [{1}]--> {2}", GetAITimeString(c), aic.debugInfo, string.Format(msg, p));
        s.AppendLine();

        LogBattleMsg(c,true, string.Format(msg, p));

        //m_totalAI.AppendFormat("[ {0} ]--> [creature : {1}]--> [ {2} ]", GetAITimeString(c), c == null || !c.gameObject ? "null" : c.gameObject.name, string.Format(msg, p));
        //m_totalAI.AppendLine();
        //var stackTrace = new System.Diagnostics.StackTrace();
        //m_totalAI.AppendLine(stackTrace.ToString());
        //for (int i = 0; i < 2; i++)
        //{
        //    m_totalAI.AppendLine();
        //}
#endif
    }

    public void LogAIMsg(Creature c, string msg,params object[] p)
    {
#if AI_LOG
        if (isDebugAILog) Logger.LogInfo(msg, p);

        AILogCreature aic = GetLogCreature(c);
        if (aic == null)
        {
            aic = new AILogCreature(c);
            m_logAIDic.Add(aic, new StringBuilder());
        }

        StringBuilder s = m_logAIDic[aic];
        s.AppendFormat("[{0}]--> creature : [{1}]--> {2}", GetAITimeString(c), aic.debugInfo,string.Format(msg, p));
        s.AppendLine();

        LogBattleMsg(c, true, string.Format(msg, p));

        //m_totalAI.AppendFormat("[ {0} ]--> [creature : {1}]--> [ {2} ]", GetAITimeString(c), c == null || !c.gameObject ? "null" : c.gameObject.name, string.Format(msg, p));
        //m_totalAI.AppendLine();
        //var stackTrace = new System.Diagnostics.StackTrace();
        //m_totalAI.AppendLine(stackTrace.ToString());
        //for (int i = 0; i < 2; i++)
        //{
        //    m_totalAI.AppendLine();
        //}
#endif
    }

    public void LogRandomMsg(string msg, params object[] p)
    {
#if AI_LOG
        if (!Level.current || !Level.current.isPvE) return;

        if (isDebugAILog) Logger.LogInfo(msg, p);

        LogBattleMsg(null,true, string.Format(msg, p));
        
        //m_totalAI.AppendFormat("[ {0} ]--> [ {1} ]", GetAITimeString(), string.Format(msg, p));
        //m_totalAI.AppendLine();
        
        //var stackTrace = new System.Diagnostics.StackTrace();
        //m_totalAI.AppendLine(stackTrace.ToString());
        //for (int i = 0; i < 2; i++)
        //{
        //    m_totalAI.AppendLine();
        //}
#endif
    }

    public string GetAITimeString(Creature c = null)
    {
        return string.Format(" LT:{0}", Level.levelTime);
        //if (!c) return string.Format(" LT:{0}", Level.levelTime);
        //return string.Format("LT:{0}; CreatureFT:{1}; CreatureCt:{2}", Level.levelTime,c.frameTime,c.frameCount);
    }

    public void ResetAILog()
    {
#if AI_LOG
        //m_totalAI.Remove(0,m_totalAI.Length);
        m_logAIDic.Clear();
        m_battleLog.Remove(0,m_battleLog.Length);
#endif
    }

    public void SaveAILog()
    {
#if AI_LOG

        string logPath = string.Empty;
        //if (m_totalAI.Length != 0)
        //{
        //    logPath = string.Format("AILogFiles/{0}_TotalAI.txt", DateTime.Now.ToString("yy-MM-dd-HH-mm-ss"));
        //    Logger.LogDetail("AI log file saved to <b><color=#00FF00>[{0}]</color></b>", Util.SaveFile(logPath, m_totalAI.ToString()));

        //}
        
        if (m_logAIDic.Count > 0)
        {
            logPath = string.Format("AILogFiles/{0}_AI.txt", DateTime.Now.ToString("yy-MM-dd-HH-mm-ss"));
            StringBuilder s = GetLogMsg();
            if (s.Length > 0) Logger.LogDetail("AI log file saved to <b><color=#00FF00>[{0}]</color></b>", Util.SaveFile(logPath, s.ToString()));
        }

        //m_totalAI.Remove(0, m_totalAI.Length);
        m_logAIDic.Clear();
#endif
    }

    private StringBuilder GetLogMsg()
    {
        StringBuilder s = new StringBuilder();
#if AI_LOG

        foreach (var item in m_logAIDic)
        {
            s.AppendLine("*************************************************************************************************************************************");
            s.AppendLine("*************************************************************强**********************************************************************");
            s.AppendLine("*************************************************************制**********************************************************************");
            s.AppendLine("*************************************************************分**********************************************************************");
            s.AppendLine("*************************************************************隔**********************************************************************");
            s.AppendLine("*************************************************************************************************************************************");
            s.AppendFormat("*********************************************** AI-DETAIL {0} ****************************************", item.Key.debugInfo);
            s.AppendLine();
            s.Append(item.Value.ToString());
            s.AppendLine("***********************************************************************************************************************************");
            s.AppendLine();
        }

#endif
        return s;
    }
#endregion

#region battle log

#if AI_LOG
    private StringBuilder m_battleLog = new StringBuilder();
#endif

    public static void LogBattleMsg(Creature c, string msg, params object[] p)
    {
        if (!Level.current || !Level.current.isPvE) return;

        instance._LogBattleMsg(c,true,msg,p);
    }

    public static void LogBattleMsg(Creature c, bool stackTrace, string msg, params object[] p)
    {
        if (!Level.current || !Level.current.isPvE) return;

        instance._LogBattleMsg(c, stackTrace, msg, p);
    }

    private void _LogBattleMsg(Creature c, bool stackTrace, string msg, params object[] p)
    {
#if AI_LOG
        if (isDebugAILog) Logger.LogInfo(msg, p);

        if (m_battleLog.Length == 0)
        {
            m_battleLog.AppendFormat("*********************************************** Battle-DETAIL ****************************************");
        }

        m_battleLog.AppendLine();
        m_battleLog.AppendFormat("[ {0} ]--> [creature : {1}]--> [ {2} ]", GetAITimeString(c), c == null || !c.gameObject ? "null" : c.gameObject.name, string.Format(msg, p));
        m_battleLog.AppendLine();

        if(stackTrace)
        {
            m_battleLog.AppendLine(GetStackTrace());
            for (int i = 0; i < 2; i++)
            {
                m_battleLog.AppendLine();
            }
        }
#endif
    }

    public static string GetStackTrace(System.Diagnostics.StackTrace trace = null)
    {
#if AI_LOG
        if (trace == null) trace = new System.Diagnostics.StackTrace();

        StringBuilder s = new StringBuilder();
        System.Reflection.MethodBase m = null;
        var frames = trace.GetFrames();
        for (int i = 0; i < frames.Length; i++)
        {
            if (i < 2 || frames[i] == null) continue;

            m = frames[i].GetMethod();

            s.AppendLine();
            s.AppendFormat("at {0}->{1}", m.DeclaringType,m.ToString());
        }
        return s.ToString();
#else
        return string.Empty;
#endif
    }

    public void SaveBattleLog()
    {
#if AI_LOG
        if (m_battleLog.Length == 0) return;
        
        var logPath = string.Format("AILogFiles/{0}_Battle.txt", DateTime.Now.ToString("yy-MM-dd-HH-mm-ss"));
        Logger.LogDetail("AI log file saved to <b><color=#00FF00>[{0}]</color></b>", Util.SaveFile(logPath, m_battleLog.ToString()));

        m_battleLog.Remove(0, m_battleLog.Length);
#endif
    }

    #endregion

    #endregion

    #region AI 处理逻辑

    private void InitAICollection()
    {
        m_lockEnermyDic.Clear();
        m_selectAICreatureDic.Clear();
        m_creatureCampDic.Clear();
        m_monsterAIDic.Clear();
        m_stratergyUseCountDic.Clear();
        m_pauseMonsters.Clear();
        m_stratergyCacheDic.Clear();
        ClearTempData();
    }

    public void SafeStartAI()
    {
        if (m_isStartAI) return;
        StartAI();
    }

    /// <summary>
    /// 开启AI,在调用其他功能时，必须首先执行该方法
    /// </summary>
    public void StartAI()
    {
        FightRecordManager.RecordLog< LogEmpty>(l =>
        {
            l.tag = (byte)TagType.StartAI;
        });

        //must before m_isStartAI
        InitAICollection();
        enableUpdate = true;
        m_isStartAI = true;
        pauseAI = false;
        modulePVEEvent.ClearCache();
        double delta = GeneralConfigInfo.AIBorderDis * 0.1;
        double x = Level.current.edge.x + delta;
        double y = Level.current.edge.y - delta;
        m_aiBorder = new Vector2_(x,y);
    }

    public void SetAllAIPauseState(bool isPause)
    {
        FightRecordManager.RecordLog<LogBool>(l =>
        {
            l.tag = (byte)TagType.SetAllAIPauseState;
            l.value = isPause;
        });

        pauseAI = isPause;
        LogBattleMsg(null,true, "SetAllAIPauseState    pauseAI = {0}",isPause);
        //Logger.LogDetail("module ai set pause ai ,pauseAI = {0}",pauseAI);
    }

    /// <summary>
    /// 关闭AI
    /// </summary>
    public void CloseAI()
    {
        RemoveAllAI();
        InitAICollection();
        enableUpdate = false;
        m_isStartAI = false;
        ClearTempData();
        //只在调试模式下保存AI日志
#if AI_LOG
        SaveAILog();
        SaveBattleLog();
        //Random.SaveRandomLog();
#endif
    }

    /// <summary>
    /// 注册玩家的AI
    /// call this function need after 'AddCreatureToCampDic'
    /// </summary>
    /// <param name="player"></param>
    /// <param name="AIID"></param>
    public void AddPlayerAI(Creature player)
    {
        if (player == null)
        {
            Logger.LogError("creature is null!! are you kidding me ???");
            return;
        }

        WeaponInfo.Weapon w = WeaponInfo.GetWeapon(player.weaponID,player.weaponItemID);
        AIConfig ai = ConfigManager.Get<AIConfig>(w.defaultAI);
        if(ai == null)
        {
            Logger.LogError("creature : {0} weapon id = {1},itemTypeId = {2},aiid = {3} connot be loaded,please check out!!"
                ,player.gameObject.name,player.weaponID,player.weaponItemID,w.defaultAI);
            enableUpdate = false;
            m_isStartAI = false;
            return;
        }
        
        FightRecordManager.RecordLog< LogUlong>(log =>
        {
            log.tag = (byte)TagType.AddPlayerAI;
            log.value = player.Identify;
        });

        if (!m_monsterAIDic.ContainsKey(player)) m_monsterAIDic.Add(player, ai);
        m_monsterAIDic[player] = ai;
        //recovery to idle when player in run state
        SetCreatureIdle(player);
        ChangeLockEnermy(player, true);
        player.AddEventListener(CreatureEvents.QUIT_STATE, OnPlayerQuitState);
        moduleAI.LogAIMsg(player, true, "[Register AI for Player {0} AIId = {1}]",player.uiName,ai.ID);
    }
    /// <summary>
    /// 所有宠物固定使用AI 3001 (宠物现在做成道具了。没有地方配置AI，由于宠物AI都一样，所以暂时写在这)
    /// </summary>
    /// <param name="pet"></param>
    public void AddPetAI(PetCreature pet)
    {
        if (pet == null) return;
        AIConfig ai = ConfigManager.Get<AIConfig>(3001);
        if (ai == null)
        {
            Logger.LogError("creature : {0} weapon id = {1},itemTypeId = {2},aiid = {3} connot be loaded,please check out!!"
                , pet.gameObject.name, pet.weaponID, pet.weaponItemID, 3001);
            enableUpdate = false;
            m_isStartAI = false;
            return;
        }


        FightRecordManager.RecordLog< LogDouble>(l =>
        {
            l.tag = (byte)TagType.AddPetAI;
            l.value = pet.Identify;
        });

        if (!m_monsterAIDic.ContainsKey(pet)) m_monsterAIDic.Add(pet, ai);
        m_monsterAIDic[pet] = ai;

        ChangeLockEnermy(pet, true);
        pet.AddEventListener(CreatureEvents.QUIT_STATE, OnPlayerQuitState);
        moduleAI.LogAIMsg(pet, true, "[Register AI for Pet {0} AIId = {1}]", pet.uiName, ai.ID);
        AddCreatureToSelectAIList(pet);
    }

    private void RemoveAllAI()
    {
        if (m_creatureCampDic == null) return;

        foreach (var item in m_creatureCampDic)
        {
            foreach (var c in item.Value)
            {
                if (item.Key == CreatureCamp.MonsterCamp) c.RemoveEventListener(CreatureEvents.QUIT_STATE, OnMonsterQuitState);
                else if (item.Key == CreatureCamp.PlayerCamp) c.RemoveEventListener(CreatureEvents.QUIT_STATE, OnPlayerQuitState);
            }
        }
    }

    /// <summary>
    /// 移除玩家 AI
    /// </summary>
    /// <param name="player"></param>
    public void RemovePlayerAI(Creature player)
    {
        if (!Contains(player)) return;
        
        FightRecordManager.RecordLog< LogUlong>(log =>
        {
            log.tag = (byte)TagType.RemovePlayerAi;
            log.value = player.Identify;
        });

        m_monsterAIDic.Remove(player);
        player.RemoveEventListener(CreatureEvents.QUIT_STATE, OnPlayerQuitState);
        RemoveAICache(player);
        moduleAI.LogAIMsg(player, true, "[Remove AI for Player {0}]", player.uiName);
    }
    /// <summary>
    /// 移除宠物 AI
    /// </summary>
    /// <param name="pet"></param>
    public void RemovePetAI(PetCreature pet)
    {
        if (!Contains(pet)) return;

        FightRecordManager.RecordLog< LogUlong>(log =>
        {
            log.tag = (byte)TagType.RemovePlayerAi;
            log.value = pet.Identify;
        });

        m_monsterAIDic.Remove(pet);
        pet.RemoveEventListener(CreatureEvents.QUIT_STATE, OnPlayerQuitState);
        RemoveAICache(pet);
        moduleAI.LogAIMsg(pet, true, "[Remove AI for Pet {0}]", pet.uiName);
    }
    /// <summary>
    /// 移除怪物 AI
    /// tip:移除ai的时候会将CreatureEvents.QUIT_STATE 事件注销
    /// </summary>
    /// <param name="player"></param>
    public void RemovMonsterAI(MonsterCreature monster)
    {
        monster.RemoveEventListener(CreatureEvents.QUIT_STATE, OnMonsterQuitState);
        if (!Contains(monster)) return;

        FightRecordManager.RecordLog< LogUlong>(log =>
        {
            log.tag = (byte)TagType.RemovePlayerAi;
            log.value = monster.Identify;
        });

        m_monsterAIDic.Remove(monster);
        RemoveAICache(monster);
        moduleAI.LogAIMsg(monster, true, "[Remove AI for Monster]");
    }

    private void RemoveAICache(Creature creature)
    {
        RemoveCreaturFromSelectAIList(creature);
    }

    /// <summary>
    /// 怪物的状态回调
    /// </summary>
    /// <param name="e"></param>
    public void OnMonsterQuitState(Event_ e)
    {
        if (!CheckForAIState()) return;

        MonsterCreature monster = (MonsterCreature) e.sender;
        //怪物死亡或者玩家死亡均不处理怪物的AI
        if (monster.isPlayer || monster.isDead || !IsAnyPlayerAlive()) return;

        LogAIMsg(monster, true, "[OnMonsterEnterState current state :{0},loop = {1},wait select ai time {2}]",
            monster.stateMachine.currentState.name, monster.stateMachine.currentState.loop,
            GetCreatureSelectAITime(monster));
        if (IsNeedAIPause(monster))
        {
            SetCreatureIdle(monster);
            return;
        }
        DoQuitState(monster, (StateMachineState)e.param2);
    }

    /// <summary>
    /// 玩家进入状态的回调
    /// </summary>
    /// <param name="e"></param>
    public void OnPlayerQuitState(Event_ e)
    {
        if (!CheckForAIState())
            return;

        Creature player = (Creature)e.sender;
        //怪物死亡或者玩家死亡均不处理怪物的AI
        if (player.isMonster || player.isDead)
            return;

        FightRecordManager.RecordLog< LogUlong>(log =>
        {
            log.tag = (byte)TagType.RemovePlayerAi;
            log.value = player.Identify;
        });

        LogAIMsg(player, true, "[OnPlayerEnterState current state :{0},loop is {1},wait select ai time {2}]", player.stateMachine.currentState.name, player.stateMachine.currentState.loop, GetCreatureSelectAITime(player));
        DoQuitState(player, (StateMachineState)e.param2);
    }

    /// <summary>
    /// 所有public方法调用之前必须使用该方法进行检测
    /// </summary>
    /// <returns></returns>
    private bool CheckForAIState()
    {
        if(!m_isStartAI) Logger.LogWarning("AI state is off,please call function 'StartAI' as first function");
        return m_isStartAI;
    }

    /// <summary>
    /// 判断是否需要转身
    /// </summary>
    /// <param name="creature"></param>
    public void ChangeTurn(Creature creature)
    {
        if (!CheckForAIState() || (creature.isMonster && (creature as MonsterCreature).ignoreAITurn))
            return;

        //挑选AI
        Creature lockCreature = null;

        if (m_lockEnermyDic.TryGetValue(creature, out lockCreature))
        {
            //锁定目标死亡后不再转向
            if (!lockCreature || lockCreature.isDead || lockCreature.health <= 0)
                return;

            //怪物在玩家右边并且怪物当前朝向朝前
            if ((GetDirection(creature, lockCreature) > 0 && creature.direction == CreatureDirection.FORWARD) ||
                (GetDirection(creature, lockCreature) < 0 && creature.direction == CreatureDirection.BACK))
            {
                creature.TurnBack();
            }
        }
    }

#region 暂停指定怪物AI处理逻辑

    private void _HandlePauseMonsterData(SAIPauseStateBehaviour pauseMonster)
    {
        //pause
        if (pauseMonster.pauseAI) AddPauseMonsterData(pauseMonster);
        else RemovePauseMonsterData(pauseMonster);
    }

    private void AddPauseMonsterData(SAIPauseStateBehaviour pauseMonster)
    {
        if(!IsContainPauseMonsterData(pauseMonster))
        {
            PauseMonsterData data = new PauseMonsterData(pauseMonster.monsterId, pauseMonster.group);
            m_pauseMonsters.Add(data);
        }
    }
    
    private void RemovePauseMonsterData(SAIPauseStateBehaviour pauseMonster)
    {
        if (m_pauseMonsters == null || m_pauseMonsters.Count == 0)
            return;

        for (int i = 0; i < m_pauseMonsters.Count; i++)
        {
            if (m_pauseMonsters[i].IsSame(pauseMonster.monsterId, pauseMonster.group))
            {
                m_pauseMonsters.RemoveAt(i);
                break;
            }
        }
    }

    private bool IsContainPauseMonsterData(SAIPauseStateBehaviour pauseMonster)
    {
        if (m_pauseMonsters == null || m_pauseMonsters.Count == 0)
            return false;

        for (int i = 0; i < m_pauseMonsters.Count; i++)
        {
            if (m_pauseMonsters[i].IsSame(pauseMonster.monsterId, pauseMonster.group))
                return true;
        }

        return false;
    }

    private bool IsNeedAIPause(MonsterCreature monster)
    {
        for (int i = 0; i < m_pauseMonsters.Count; i++)
        {
            if (m_pauseMonsters[i].IsContain(monster))
                return true;
        }

        return false;
    }

    private void SetCreatureIdle(Creature creature)
    {
        var state = creature.stateMachine.currentState;
        //recovery to idle
        if (state.isRun) creature.moveState = 0;
    }

#endregion

#region 阵营相关处理逻辑

    /// <summary>
    /// 判断是否还有玩家存活
    /// </summary>
    /// <returns></returns>
    private bool IsAnyPlayerAlive()
    {
        if (m_creatureCampDic == null || !m_creatureCampDic.ContainsKey(CreatureCamp.PlayerCamp))
            return false;

        List<Creature> players = m_creatureCampDic[CreatureCamp.PlayerCamp];
        if (players == null || players.Count == 0)
            return false;

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].health > 0)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 目标是否在 AI 列表中
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public bool Contains(Creature c)
    {
        if (!c || m_monsterAIDic == null) return false;
        return m_monsterAIDic.ContainsKey(c);
    }

    /// <summary>
    /// 将怪物和玩家或者玩家和机器人进行分组
    /// 必须执行该方法！！
    /// </summary>
    /// <param name="c"></param>
    public void AddCreatureToCampDic(Creature c)
    {
        if (!c) return;

        List<Creature> campList = null;
        if (m_creatureCampDic.TryGetValue(c.creatureCamp, out campList))
        {
            if (!campList.Contains(c)) campList.Add(c);
        }
        else
        {
            campList = new List<Creature>();
            campList.Add(c);
            m_creatureCampDic.Add(c.creatureCamp, campList);
        }
    }

    /// <summary>
    /// 死亡后从阵营列表中移除
    /// </summary>
    /// <param name="c"></param>
    public void RemoveCreatureFromCampDic(Creature c)
    {
        if (!CheckForAIState())
            return;

        List<Creature> campList = null;
        if (m_creatureCampDic.TryGetValue(c.creatureCamp, out campList))
        {
            if (campList.Contains(c)) campList.Remove(c);
        }
    }

    public void SortCreatureCamp(CreatureCamp camp)
    {
        List<Creature> campList = m_creatureCampDic.Get(camp);
        if (campList != null && campList.Count > 0) campList.Sort((a, b) => a.roleId < b.roleId ? -1 : 1);
    }
#endregion
    
#region m_selectAICreatureDic 增/删/查/改

    private void AddCreatureToSelectAIList(Creature c, double dur = 0)
    {
        if (!c) return;

        bool contain = false;
        for (int i = 0; i < m_selectAICreatureDic.Count; i++)
        {
            if(c == m_selectAICreatureDic[i].creature)
            {
                contain = true;
                //只能完成了上一次的循环AI后，才可以进行设置
                if(m_selectAICreatureDic[i].selectTime <= 0) m_selectAICreatureDic[i].SetSelectTimeByDuraction(dur);
                break;
            }
        }

        if (!contain)
        {
            m_selectAICreatureDic.Add(new SelectAICreature(c, dur));
            m_selectAICreatureDic.Sort((a, b) => a.creature.roleId < b.creature.roleId ? -1 : 1);
        }
    }
    
    private void RemoveCreaturFromSelectAIList(Creature c)
    {
        for (int i = 0; i < m_selectAICreatureDic.Count; i++)
        {
            if(m_selectAICreatureDic[i].creature == c)
            {
                m_selectAICreatureDic.RemoveAt(i);
                break;
            }
        }
        m_selectAICreatureDic.Sort((a, b) => a.creature.roleId < b.creature.roleId ? -1 : 1);
    }

    private double GetCreatureSelectAITime(Creature c)
    {
        SelectAICreature sc = GetCreatureFromSelectAIList(c);
        return sc == null ? -1 : sc.selectTime;
    }
    
    private SelectAICreature GetCreatureFromSelectAIList(Creature c)
    {
        foreach (var item in m_selectAICreatureDic)
        {
            if (item.creature == c) return item;
        }
        return null;
    }
#endregion

#region operate ai
    private void DoQuitState(Creature creature, StateMachineState state)
    {
        FightRecordManager.RecordLog< LogEmpty>(log =>
        {
            log.tag = (byte) TagType.DoQuitState;
        });

        if (modulePVEEvent.isEnd || modulePVEEvent.pauseCountDown || pauseAI)
        {
            SetCreatureIdle(creature);
            return;
        }

        //站立循环动作处理：(挑选AI阶段延后处理)
        if (state.isIdle)
        {
            //状态切换回stand的时候，或者还未锁敌，或者锁敌目标已经阵亡的时候，就切换锁敌目标
            ChangeLockEnermy(creature);
            //先切换锁敌目标，然后重新计算转向
            ChangeTurn(creature);
            //在OnUpdate中延后处理循环AI
            AddCreatureToSelectAIList(creature);
        }
  
        //进入非循环动作检测
        else if (state && !state.loop)
        {
            if(CanCheckChangeTurn(state, creature)) ChangeTurn(creature);
            //must cancel the delay check
            SelectAICreature sc = GetCreatureFromSelectAIList(creature);
            sc?.OnSelectAIComplete("normal state enter");
            LogBattleMsg(creature, "enter unloop functions,select time is {0}", sc == null ? "null" : sc.selectTime.ToString());
        }
    }
    
    /// <summary>
    /// 更改怪物锁定目标
    /// 没有AI的怪物不去自动锁敌
    /// </summary>
    /// <param name="creature"></param>
    /// <param name="isForceTarget"> 是否是初始化，初始化的时候必须强制锁敌</param>
    public void ChangeLockEnermy(Creature creature, bool isForceTarget = false)
    {
        if(!CheckForAIState() || !m_monsterAIDic.ContainsKey(creature))
            return;

        if (creature is PetCreature)
        {
            var pet = creature as PetCreature;
            m_lockEnermyDic[creature] = pet.ParentCreature;
            return;
        }

        //如果当前玩家锁敌目标已经死亡，则需要强制重新挑选目标
        Creature lockCreature = m_lockEnermyDic.Get(creature);
        if (lockCreature == null || lockCreature != null && lockCreature.isDead) isForceTarget = true;

        Creature newTarget = null;
        var enermyCamps = LOCK_ENERMY_CAMP_DIC.Get(creature.creatureCamp);
        foreach (var camp in enermyCamps)
        {
            var list = m_creatureCampDic.Get(camp);
            if (list != null && list.Count > 0) newTarget = GetLockEnermyCreature(creature, list);

            if (newTarget) break;
        }

        //如果初始化的时候又没有筛选到锁敌目标，强制赋值
        if (isForceTarget && !newTarget)
        {
            var allEnermys = new List<Creature>();
            foreach (var camp in enermyCamps)
            {
                var list = m_creatureCampDic.Get(camp);
                if (list != null && list.Count > 0) allEnermys.AddRange(list);
            }

            newTarget = GetRandomLockEnermy(allEnermys);
        }

        //更新锁敌目标
        if (newTarget)
        {
            if (!m_lockEnermyDic.ContainsKey(creature)) m_lockEnermyDic.Add(creature, null);
            m_lockEnermyDic[creature] = newTarget;
        }
        LogAIMsg(creature, "[切换锁敌目标:{0}，强制锁敌状态 : {1}]", newTarget ? newTarget.gameObject.name : "null", isForceTarget);
    }

    private Creature GetRandomLockEnermy(List<Creature> enermyList)
    {
        if (enermyList == null || enermyList.Count == 0) return null;

        List<Creature> list = new List<Creature>();
        foreach (var item in enermyList)
        {
            if (CheckIgnoreLockEnermy(item)) continue;
            list.Add(item);
        }

        if (list.Count == 0) return null;

        //return list[Random.Range(0, list.Count)];
        return list[moduleBattle._Range(0, list.Count)];
    }

    /// <summary>
    /// 是否需要忽略锁敌目标的检测
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    private bool CheckIgnoreLockEnermy(Creature c)
    {
        if (!c || c.isDead || c.HasBuffType(BuffInfo.EffectTypes.Invincible)) return true;
        if (c is MonsterCreature) return (c as MonsterCreature).lockEnermyType == EnumLockMonsterType.Close;

        return false;
    }

    private void SelectNewAIForCreature(Creature monster)
    {
        AIConfig.AIBehaviour behaviour = GetAIBehaviour(monster);

#if AI_LOG
        LogBattleMsg(monster, true, "SelectNewAIForCreature behaviour is {0}",behaviour == null ? "null" : behaviour.ToXml());
#endif

        if (behaviour != null)
        {
            double time = 0;
            if (behaviour.IsLoop())
            {
                var ts = behaviour.loopDuraction;
                //int random = Random.Range(ts.GetValue<int>(0), ts.GetValue<int>(1));
                int random = moduleBattle._Range(ts.GetValue<int>(0), ts.GetValue<int>(1));
                //省略到随机数的个位数
                time = random / 10 * 10 * 0.001;
                LogAIMsg(monster, "[AiState {0}_{1} is loop State,random value is  {2} ]", behaviour.state, behaviour.behaviourType, random);

                FightRecordManager.RecordLog<LogAi>(l=>
                {
                    l.tag = (byte)TagType.AddPetAI;
                    l.behaviourType = (byte)behaviour.behaviourType;
                    l.random = random;
                    l.rangeMin = ts.GetValue<int>(0);
                    l.rangeMax = ts.GetValue<int>(1);
                });
            }
            AddCreatureToSelectAIList(monster, time);
            OperateAI(monster, behaviour);
        }
        //如果没有筛选到AI（可能由于玩家已经死光了，这时候循环AI的计时才到达），还原到StateStand
        else
        {
            monster.moveState = 0;
            LogAIMsg(monster, "[handle SelectNewAIForCreature,并未选择到ai, moveState 强制设为0]");
        }
    }

    private void OperateAI(Creature creature, AIConfig.AIBehaviour behaviour)
    {
        if (behaviour == null || creature.isDead) return;

        //如果宠物的技能释放次数已经达到最大值，排除宠物的技能释放动作
        if (creature is PetCreature)
        {
            var pet = creature as PetCreature;
            var state = creature.stateMachine.GetState(behaviour.state);
            if (state.IsSkill && !pet.CanUseSkill(state.name))
                return;
        }

        Creature lockCreature = m_lockEnermyDic.Get(creature);
        if (IsSameLoopAnim(creature, lockCreature, behaviour)) return;

        if (!creature.stateMachine.GetBool(StateMachineParam.exit))
            return;
        //还原为IDLE修改此处可能会造成StateMachine切换成IdleState
        creature.moveState = 0;
        if (lockCreature != null && (CreatureStateInfo.NameToID(behaviour.state) == 4)) HandleStateRunState(creature,lockCreature,behaviour.behaviourType);

        FightRecordManager.RecordLog<LogOperateAi>(log =>
        {
            log.tag = (byte)TagType.OperateAI;
            log.roldId = creature.Identify;
            log.state = behaviour.state;
            log.currentState = creature.stateMachine.currentState.name;
            log.lockCreatute = lockCreature?.Identify ?? 0;
        });

        LogAIMsg(creature,"[开始执行AI状态:{0},当前状态:{1}]",behaviour.state, creature.stateMachine.currentState.name);
        //播放状态,检查是否处于冷却
        bool handleSuccess = creature.stateMachine.TranslateTo(behaviour.state, true);
        //还在冷却状态
        if (!handleSuccess)
        {
            AddCreatureToSelectAIList(creature);
            LogAIMsg(creature, "[执行 {0} 失败,状态正在冷却，等待下一帧筛选]", behaviour.state);
        }
    }

    private void HandleStateRunState(Creature creature,Creature lockCreature, AIConfig.AIBehaviour.AIBehaviourType type)
    {
        int curDir = GetDirection(creature, lockCreature);
        creature.moveState = type == AIConfig.AIBehaviour.AIBehaviourType.FarAway ? curDir : -curDir;
    }

    /// <summary>
    /// 如果是相同的循环动作，则不强制切换
    /// </summary>
    /// <param name="creature"></param>
    /// <param name="behaviour"></param>
    /// <returns></returns>
    private bool IsSameLoopAnim(Creature creature, Creature lockCreature, AIConfig.AIBehaviour behaviour)
    {
        if (lockCreature == null)
            return false;

        StateMachineState cs = creature.stateMachine.currentState;
        creature.moveState = 0;
        //循环动作判断
        if (cs.loop && cs.name == behaviour.state)
        {
            //同为跑步状态也不需要重新播放，只需要将方向改变
            if (CreatureStateInfo.NameToID(cs.name) == 4) HandleStateRunState(creature,lockCreature,behaviour.behaviourType);
            return true;
        }
        return false;
    }

    private AIConfig.AIBehaviour GetAIBehaviour(Creature monster)
    {
        //挑选AI
        Creature lockCreature = m_lockEnermyDic.Get(monster);
        AIConfig.AIBehaviour behaviour = null;
        if (lockCreature != null && !lockCreature.isDead)
        {
            behaviour = moduleAI.GetAIBehaviour(lockCreature, monster);
        }

        return behaviour;
    }

    private bool CanCheckChangeTurn(StateMachineState state,Creature creature)
    {
        //被动、循环、当前处于空中、或者当前状态本身禁止转身的时候不能转身
        if (state == null || state.passive || state.loop || (creature == null || !creature.onGround) || (state.info != null && state.info.preventTurn) || IsStateUp(state))
            return false;
        
        return true;
    }

    private bool IsStateUp(StateMachineState state)
    {
        if (!state) return false;

        //检测忽略转身的列表
        for (int i = 0; i < IgnoreTurnStateIDs.Length; i++)
        {
            if (CreatureStateInfo.NameToID(state.name) == IgnoreTurnStateIDs[i]) return true;
        }

        return false;
    }

#endregion

#region Update 延迟处理AI
    public override void OnUpdate(int diff)
    {
        base.OnUpdate(diff);
        if (!m_isStartAI || pauseAI) return;

        UpdateChangeMonsterToStateStand(diff * 0.001);
    }

    private void UpdateChangeMonsterToStateStand(double deltaTime)
    {
        if (m_selectAICreatureDic == null || m_selectAICreatureDic.Count <= 0 || pauseAI) return;

        FightRecordManager.RecordLog< LogInt>(log =>
        {
            log.tag = (byte)TagType.selectAICreatureDicCount;
            log.value = m_selectAICreatureDic.Count;
        });

        foreach (var item in m_selectAICreatureDic)
        {
            FightRecordManager.RecordLog<LogDouble>(l =>
            {
                l.tag = (byte)TagType.itemSelectTime;
                l.value = item.selectTime;
            });

            if (item.selectTime < 0 || item.creature.gameObject == null || !item.creature.gameObject.activeSelf || item.creature.isDead) continue;
            
            if(item.CheckSelectTimeCountDownEnd(deltaTime) && IsCanForceToChange(item.creature))
            {
                //must called oncomplete before select new ai
                item.OnSelectAIComplete("selectTime arrived");


                FightRecordManager.RecordLog< LogUlong>(ul =>
                {
                    ul.tag = (byte)TagType.aiRoleId;
                    ul.value = item.creature.Identify;
                });

                SelectNewAIForCreature(item.creature);

                //宠物强制每0.1s更新一次AI
                if (item.creature is PetCreature)
                    item.SetSelectTimeByDuraction(0.1);
            }
        }
    }

    private bool IsCanForceToChange(Creature monster)
    {
        if (monster.isMonster && IsNeedAIPause(monster as MonsterCreature))
            return false;

        // 获取当前受击状态
        var mask = monster.stateMachine.GetInt(StateMachineParam.forcePassive) >> 48;
        //状态包含被抱摔，则不挑选新AI
        if (mask.BitMask(StateMachine.PASSIVE_CATCH)) return false;

        StateMachineState state = monster.stateMachine.currentState;
        return !state.passive && monster.onGround && !IsStateUp(state);
    }
#endregion
#endregion
}


#region custom class

/// <summary>
/// 
/// </summary>
public class AILogCreature
{
    public ulong roleId;
    public int configId;
    public string name;
    public int teamIndex;
    public bool isPlayer;
    public bool isRobot;
    public CreatureCamp camp;
    
    public bool isPet;

    public bool isMonster;
    public bool isBoss;
    public int group;
    public int configLevel;
    public int tarLevel;

    private string m_debugInfo = string.Empty;
    public string debugInfo
    {
        get
        {
            if (string.IsNullOrEmpty(m_debugInfo)) m_debugInfo = string.Format("[{0}->id:{1} group:{2} tIdx:{3}",name,configId,group,teamIndex);
            return m_debugInfo;
        }
    }

    public AILogCreature(Creature c)
    {
        roleId = c.roleId;
        configId = c.configID;
        name = c.name;
        teamIndex = c.teamIndex;
        isPlayer = c.isPlayer;
        isRobot = c.isRobot;
        camp = c.creatureCamp;

        isPet = c is PetCreature;

        isMonster = c.isMonster;
        if (isMonster)
        {
            MonsterCreature mon = c as MonsterCreature;
            isBoss = mon.isBoss;
            group = mon.monsterGroup;
            configLevel = mon.monsterLevel;
            tarLevel = mon.level;
        }
    }

    public bool IsSameCreature(Creature c)
    {
        if (!c) return false;
        return c.teamIndex == teamIndex;
    }
}
#endregion
