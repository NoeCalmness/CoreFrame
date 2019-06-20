/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Buff class
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-08-16
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using NoeExpression;

public struct BuffContext
{
    public int id;
    public Creature owner;
    public Creature source;
    public Creature target;
    public int duration;
    public Buff sourceBuff;
    public int sourceBuffID;
    public int level;
    public int delay;

    public void Reset()
    {
        id = 0;
        owner = null;
        source = null;
        target = null;
        duration = 0;
        sourceBuff = null;
        sourceBuffID = 0;
        level = 0;
        delay = -1;
    }
}
/// <summary>
 /// Buff class
 /// 
 /// Usage: Buff.Create(), Buff.Remove()
 /// Do not call Buff.Destroy(), use Remove instead
 /// </summary>
public class Buff : PoolObject<Buff>
{
    #region buff name map
    public static string[][] nameRemap = new string[][]
    {
        new string[] { },                                                                     // Unknow type
        new string[] { "attributeID", "value", "isPercent", "notRest", "isSpecial", "targetAttributeID" }, // ModifyAttribute
        new string[] { "noRimLight" },                                                        // Invincible
        new string[] { "level", "noRimLight" },                                               // Tough
        new string[] { "health", "rage", "isPercent" },                                       // Revive
        new string[] { "buffID", "buffType","count" },                                        // Dispel
        new string[] { "buffID", "toTrigger","rate"},                                         // AddBuff
        new string[] { "value", "isValue" },                                                  // Shield
        new string[] { "value", "isValue", "chance" },                                        // ReflectDamage
        new string[] { "damage", "attributeID", "value", "isValue" },                         // NightWatch
        new string[] { "diff", "attributeID", "value", "isValue" },                           // Steady
        new string[] { "health", "rage", "isValue" },                                         // ImmuneDeath
        new string[] { "chance" },                                                            // Berserker
        new string[] { "value", "isValue", "limit", "limitIsValue", "max", "maxIsValue" },    // Heal
        new string[] { "value", "isValue" },                                                  // Rage
        new string[] { "state", "ignoreCooldown", "ignoreCatch" },                            // ForceState
        new string[] { "stateAir", "stateGround" },                                           // Freez
        new string[] { },                                                                     // ClearAttackBox
        new string[] { "value", "isValue" },                                                  // ShieldForBurst
        new string[] { "useCount", "cd" },                                                    // ResetPetSkill
        new string[] { "level"},                                                              // BreakTough
        new string[] { "value","setOrModify","isPercent","max"},                              // SetDuration
        new string[] { "value", "scope", "SelectType"},                                       // SlowClock
        new string[] { },                                                                     // HawkEye
        new string[] { },                                                                     // Mark
        new string[] { "r", "g", "b", "a"},                                                   // MarkColor
        new string[] { "stateEnter", "stateQuit", "morph" },                                  // SwitchMorph
        new string[] { "alpha", "fade", "global" },                                           // DarkScreen
        new string[] {  "morph"},                                                             // SwitchMorphModel
        new string[] {  "propertyType", "value", "isValue", "targetType", "targetValue", "targetIsValue"},
                                                                                                // DamageTargetLimit
        new string[] { "propertyType", "value", "isValue"},                                   // DamageForTargetProperty
        new string[] { "attributeID", "value", "isPercent", "isRemove","timeMax"},            // steal
        new string[] { },                                                                     // Count, unused
    };
    #endregion
    #region Static functions

    public static Buff Create(BuffContext rContext)
    {
        var info = ConfigManager.Get<BuffInfo>(rContext.id);
        if (!info)
        {
            Logger.LogWarning("Buff::Create: Create buff failed, invalid config ID [{0}].", rContext.id);
            return null;
        }

        if (info.applyType == BuffInfo.ApplyTypes.Discard)
        {
            var o = rContext.owner is PetCreature ? ((PetCreature)rContext.owner).ParentCreature : rContext.owner;
            if (o.HasBuff(b => b.info.ID == info.ID && (!info.dependSource || b.source == rContext.source)))
            {
                return null;
            }
        }

        var buff = Create(false);

        info.CalcLevel(rContext.level);

        buff.context.Reset();
        if(rContext.sourceBuff != null)
            buff.context.sourceBuffID = rContext.sourceBuff.ID;
        buff.info = info;
        buff.owner = rContext.owner;
        buff.length = rContext.duration != 0 ? rContext.duration : info.duration;
        buff.duration = buff.length;
        buff.m_level = rContext.level;
        buff.context = rContext;

        buff.OnInitialize();

        return buff;
    }

    public static Buff Create(int id, Creature owner, Creature source = null, int duration = 0, int sourceBuffID = -1, int level = 1)
    {
        if (!owner)
        {
            Logger.LogWarning("Buff::Create: Create buff [{0}] failed, owner can not be null.", id);
            return null;
        }

        var info = ConfigManager.Get<BuffInfo>(id);
        if (!info)
        {
            Logger.LogWarning("Buff::Create: Create buff failed, invalid config ID [{0}].", id);
            return null;
        }

        return _Create(info, owner, source ? source : owner, duration, sourceBuffID, level);
    }

    public static Buff Create(BuffInfo info, Creature owner, Creature source = null, int duration = 0, int sourceBuffID = -1, int level = 1)
    {
        if (!owner)
        {
            Logger.LogWarning("Buff::Create: Create buff failed, owner can not be null.");
            return null;
        }

        if (!info)
        {
            Logger.LogWarning("Buff::Create: Create buff failed, invalid config info.");
            return null;
        }

        return _Create(info, owner, source ? source : owner, duration, sourceBuffID, level);
    }

    private static Buff _Create(BuffInfo info, Creature owner, Creature source, int duration, int sourceBuffID, int level)
    {
        
        if (info.applyType == BuffInfo.ApplyTypes.Discard)
        {
            var o = owner is PetCreature ? ((PetCreature) owner).ParentCreature : owner;
            if (o.HasBuff(b => b.info.ID == info.ID && (!info.dependSource || b.source == source)))
            {
                return null;
            }
        }

        var buff = Create(false);

        info.CalcLevel(level);
        buff.info = info;
        buff.owner = owner;
        buff.length = duration != 0 ? duration : info.duration;
        buff.duration = buff.length; 
        buff.m_level = level;

        buff.context.Reset();
        buff.context.id = buff.ID;
        buff.context.level = level;
        buff.context.owner = owner;
        buff.context.sourceBuffID = sourceBuffID;
        buff.context.source = source;

        buff.OnInitialize();

        return buff;
    }

    #endregion

    private delegate bool EffectHandler(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse);

    [Flags]
    protected enum SpecialFlag
    {
        None           = 0x000,
        WillTakeDamage = 0x001,
        TakeDamage     = 0x002,
        Crit           = 0x004,
        Dying          = 0x008,
        Dead           = 0x010,
        Attacked       = 0x020,
        Shooted        = 0x040,
        WillPreDamage  = 0x080,
        PreDamage      = 0x100,
    }

    /// <summary>
    /// buff的附加参数
    /// </summary>
    protected class AdditionalParameters : PoolObject<AdditionalParameters>
    {
        /// <summary>
        /// 条件触发者
        /// </summary>
        public Creature trigger;

        public double   triggerParam;

        private AdditionalParameters() { }

        public static AdditionalParameters Create(Creature rTrigger, double rTriggerParam)
        {
            var p = Create();
            p.trigger = rTrigger;
            p.triggerParam = rTriggerParam;
            return p;
        }
    }

    protected class BuffEffectInfo
    {
        public double     userParam0;
        public Creature   userParam1;
        public DamageInfo damageInfo;
        public AttackInfo attackInfo;

        public int refToughID;
        public int refBreakToughID;

        public double     maxAmount0;
        public double     amount0;
        public double     maxAmount1;
        public double     amount1;

        private double _param;
        private double param
        {
            get { return _param; }
            set
            {
                _param = value;
                m_sourceBuff?.dataHandler?.SetVariable("param", _param);
            }
        }
        /// <summary>
        /// 条件触发参数，如条件是连招结束时。参数就是连招数
        /// </summary>
        public AdditionalParameters triggerParam;

        public int applyCount;
        public int maxApplyACount;

        public bool actived;
        public bool keyEffect;

        public int time;
        public bool ignoreTrigger;

        public SpecialFlag flag;

        public BuffInfo.BuffEffect effect;
        //开始作用的对象列表。revert时需要对这些对象做数据恢复
        public IReadOnlyList<Creature> effectList;

        public BuffInfo.ExpressionVariable[] variables;

        public int type;

        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return effect.param0;
                    case 1: return effect.param1;
                    case 2: return effect.param2;
                    case 3: return effect.param3;
                    case 4: return effect.param4;
                    case 5: return effect.param5;
                }
                return 0;
            }
        }

        public bool Update(int diff)
        {
            if (!actived || effect.specialTrigger || effect.interval < 1) return false;
            if ((time += diff) >= effect.interval)
            {
                time -= effect.interval;
                return true;
            }
            return false;
        }

        public void Set(BuffInfo.BuffEffect _effect, Buff sourceBuff)
        {
            if (_effect != null)
            {
                effect = _effect;
                variables = _effect.expression.GetVariables(sourceBuff.BuffLevel);
            }
            else
            {
                UnInitTrigger();
                effect         = null;
                variables      = null;
            }
            time           = 0;
            amount0        = 0;
            maxAmount0     = 0;
            amount1        = 0;
            maxAmount1     = 0;
            userParam0     = 0;
            userParam1     = null;
            damageInfo     = null;
            attackInfo     = null;
            refToughID     = 0;
            flag           = 0;
            applyCount     = 0;
            param         = 0;
            actived        = effect != null;
            keyEffect      = effect != null && effect.keyEffect;
            type           = effect != null ? (int)effect.type : 0;
            maxApplyACount = effect != null ? effect.applyCount : 0;
            ignoreTrigger  = effect ?.ignoreTrigger ?? false;
            m_sourceBuff   = sourceBuff;

            InitTrigger();
        }

        private void UnInitTrigger()
        {
            if (effect.paramTrigger == null || !m_sourceBuff)
                return;
            var listener = effect.paramTrigger.listenSource && m_sourceBuff.source != null
                ? m_sourceBuff.source
                : m_sourceBuff.owner;

            switch (effect.paramTrigger.type)
            {
                case BuffInfo.ParamTriggerTypes.TakeDamage:
                    listener.RemoveEventListener(CreatureEvents.ATTACKED, OnListenerAttacked);
                    break;
            }
        }

        protected Buff m_sourceBuff;
        private void InitTrigger()
        {
            if (effect == null) return;

            if (effect.paramTrigger.type == BuffInfo.ParamTriggerTypes.None)
                return;
            if (effect.paramTrigger == null || !m_sourceBuff)
                return;
            var listener = effect.paramTrigger.listenSource && m_sourceBuff.source != null
                ? m_sourceBuff.source
                : m_sourceBuff.owner;

            if (listener is PetCreature)
                listener = ((PetCreature) listener).ParentCreature;

            switch (effect.paramTrigger.type)
            {
                case BuffInfo.ParamTriggerTypes.TakeDamage:
                    listener.AddEventListener(CreatureEvents.TAKE_DAMAGE, OnListenerAttacked);
                    break;
            }
        }

        private void OnListenerAttacked(Event_ e)
        {
            if (effect == null) return;
            var source = m_sourceBuff.source ?? m_sourceBuff.owner;

            if (source is PetCreature)
            source = ((PetCreature)source).ParentCreature;
            if (effect.paramTrigger.param0.Equals(0) || (e.param1 as Creature) == source)
            {
                var info = e.param2 as DamageInfo;
                if (info == null)
                    return;
                param += info.finalDamage;
            }
        }
    };

    protected class BuffEffectInfoSteal : BuffEffectInfo
    {
        public class Entry
        {
            public Creature creature;
            public double effectValue;
            public int times;
        }
        public List<Entry> effectValues = new List<Entry>();

        private CreatureFields m_field;

        public void AddListen( Creature target)
        {
            var idx = effectValues.FindIndex(item => item.creature == target);
            if (idx > -1)
                return;
            target?.AddEventListener(CreatureEvents.DEAD_ANIMATION_END, OnCreatureDied);
        }

        private void OnCreatureDied(Event_ e)
        {
            var s = e.sender as Creature;
            if (null == s)
                return;

            var idx = effectValues.FindIndex(item => item.creature = s);
            if (idx < 0)
                return;
            var v = effectValues[idx].effectValue;
            amount0 -= v;
            m_sourceBuff.owner.ModifyField(m_field, -v);
            s.ModifyField(m_field, v);
            s.RemoveEventListener(this);
            effectValues.RemoveAt(idx);
        }

        public void StealReverse(CreatureFields attributeType)
        {
            m_field = attributeType;
            for (var i = 0; i < effectValues.Count; i++)
            {
                var e = effectValues[i];
                e.creature?.ModifyField(m_field, e.effectValue);
            }
            effectValues.Clear();
            m_sourceBuff.owner.ModifyField(m_field, -amount0);
            amount0 = 0;
        }

        public bool CanStealTarget(Creature target, int timeMax)
        {
            if (timeMax <= 0)
                return true;

            var e = effectValues.Find(item => item.creature = target);
            return e == null ||  e.times < timeMax;
        }

        public void Steal(CreatureFields attributeType, Creature target, double effectValue)
        {
            m_field = attributeType;

            var realEffectValue = Mathd.Min(effectValue, target.GetField(attributeType));
            m_sourceBuff.owner.ModifyField(attributeType, realEffectValue);
            target.ModifyField(attributeType, -realEffectValue);
            amount0 += realEffectValue;
            var idx = effectValues.FindIndex(item => item.creature == target);
            if (idx > -1)
            {
                effectValues[idx].effectValue += realEffectValue;
                effectValues[idx].times ++;
                return;
            }
            effectValues.Add(new Entry() { creature = target, effectValue = realEffectValue, times = 1 });
        }
    }

    protected class BuffTriggerInfo
    {
        /// <summary>
        /// 触发总次数
        /// </summary>
        public int triggerAmount;
        /// <summary>
        /// 当前条件触发次数. 类似三环。打出三环后会被重置
        /// </summary>
        public int triggerCount;
        /// <summary>
        /// 触发逻辑效果需要的触发次数（三环or四环？）
        /// </summary>
        public int conditionValue;

        public BuffInfo.BuffTrigger info;

        public bool isTrigger
        {
            get { return triggerCount >= conditionValue; }
        }

        public void Set(BuffInfo.BuffTrigger rTrigger)
        {
            Reset();
            info = rTrigger;
            triggerAmount = 0;
        }

        public void Trigger()
        {
            triggerAmount++;
            triggerCount ++;
        }

        public void Reset()
        {
            triggerCount = 0;
        }

        public void ResetAll()
        {
            Reset();
            triggerAmount = 0;
        }
    }

    public int ID { get { return info.ID; } }

    public int sourceBuffID { get { return m_sourceBuffID; } }

    public BuffInfo info { get; private set; }
    /// <summary>
    /// buff实际作用对象
    /// </summary>
    public Creature owner
    {
        get
        {
            //如果是需要加到宠物身上的buff。直接返回原始目标
            if (info.petEffect) return buffOwner;
            return buffOwner is PetCreature ? (buffOwner as PetCreature).ParentCreature : buffOwner;
        }
        set { buffOwner = value; }
    }
    public Creature source { get { return context.source; } }
    /// <summary>
    /// Buff 持续时间
    /// </summary>
    public int length { get; protected set; }
    /// <summary>
    /// Buff 剩余时间
    /// </summary>
    public int duration { get; protected set; }
    public bool actived { get { return m_actived; } }
    public bool infinity { get { return m_infinity; } }
    public int delayTime { get { return m_delayTime; } }
    public bool isSwitchMorph { get { return m_isSwitchMorph; } }

    public int BuffLevel { get { return m_level; } }

    private readonly EffectHandler[] m_effectHandlers = null;

    private static Dictionary<BuffInfo.EffectTypes, Type> m_effectInfoDict = null;

    private readonly BuffEffectInfo[] m_effInfos = null;

    private readonly BuffTriggerInfo m_triggerInfo = null;

    private readonly BuffTriggerInfo m_followTriggerInfo = null;

    private List<Effect> stepEffectList = new List<Effect>(); 
    /// <summary>
    /// buff的拥有者。可能是宠物。如果是宠物Owner则返回此宠物的召唤者。
    /// 因为宠物buff效果都是对召唤者生效的。监听也是召唤者
    /// buff加在宠物身上，是因为部分逻辑会对宠物生效。如宠物被动释放技能
    /// </summary>
    private Creature buffOwner;

    private Creature conditionTrigger;

    private BuffContext context;

    private NoeExpression.DataHandler dataHandler;

    private double[] m_totalDamage = new double[5];
    private double m_currentStatisicDamage;
    private int m_maxStep = 0;

    private double[] m_elementDamage = new double[(int)CreatureElementTypes.Count - 1]; // Ignore CreatureElementTypes.None
    private int m_sourceBuffID {get { return context.sourceBuffID; } }

    private int m_effectCount = 0;
    private int m_activeEffectCount = 0;
    /// <summary> 0 = normal buff end  1 = interrupt  2 = owner dead  3 = override by other buff, 4 = discarded</summary>
    private int m_interrupted = 0;
    private bool m_infinity = false;
    private bool m_actived = false;
    private bool m_isSwitchMorph = false;
    private bool m_pending = true;
    private int m_level;                    //buff等級
    private int m_delayTime;

    static Buff()
    {
        m_effectInfoDict = new Dictionary<BuffInfo.EffectTypes, Type>
        {
            {BuffInfo.EffectTypes.Steal, typeof (BuffEffectInfoSteal)}
        };
    }

    protected Buff()
    {
        m_effectHandlers = new EffectHandler[]
        {
            HandleEffectUnKnow,
            HandleEffectModifyAttribute,
            HandleEffectInvincible,
            HandleEffectTough,
            HandleEffectRevive,
            HandleEffectDispel,
            HandleEffectAddBuff,
            HandleEffectShield,
            HandleEffectReflectDamage,
            HandleEffectNightWatch,
            HandleEffectSteady,
            HandleEffectImmuneDeath,
            HandleEffectBerserker,
            HandleEffectHeal,
            HandleEffectRage,
            HandleEffectForceState,
            HandleEffectFreez,
            HandleEffectClearAttackBox,
            HandleEffectShieldForBurst,
            HandleEffectResetPetSkill,
            HandleEffectBreakTough,
            HandleEffectSetDuration,
            HandleEffectSlowClock,
            HandleEffectHawkEye,
            HandleEffectMark,
            HandleEffectMarkColor,
            HandleEffectSwitchMorph,
            HandleEffectDarkScreen,
            HandleEffectSwitchMorphModel,
            HandleEffectDamageTargetLimit,
            HandleEffectDamageForTargetProperty,
            HandleEffectSteal,
        };

        m_effInfos = new BuffEffectInfo[BuffInfo.MAX_BUFF_EFFECT_COUNT];

        m_triggerInfo = new BuffTriggerInfo();
        m_followTriggerInfo = new BuffTriggerInfo();
    }

    protected override void OnInitialize()
    {
        m_infinity = duration < 1;
        m_actived = true;
        m_isSwitchMorph = info.buffEffects.FindIndex(e => e.type == BuffInfo.EffectTypes.SwitchMorph) > -1;
        m_pending = true;
        m_interrupted = 0;
        m_currentStatisicDamage = 0;

        m_totalDamage.Clear();
        m_elementDamage.Clear();

        m_maxStep = 0;
        m_triggerInfo.Set(info.trigger);
        m_followTriggerInfo.Set(info.trigger);
        m_triggerInfo.conditionValue = info.trigger.triggerCount;
        m_followTriggerInfo.conditionValue = info.trigger.followTriggerCount;

        if (!owner.AddBuff(this)) return;

        m_delayTime = context.delay > 0 ? context.delay : info.delay;
        if (m_delayTime <= 0) ActiveBuff();
    }

    private void ActiveBuff()
    {
        owner.DispatchEvent(CreatureEvents.BUFF_CREATE, Event_.Pop(this));

        InitDataHandler();
        InitializeListeners();
        InitializeDestoryListeners();

        m_effectCount = info.buffEffects.Length;
        m_activeEffectCount = m_effectCount;

        for (int i = 0; i < m_effectCount; ++i)
        {
            var eff = info.buffEffects[i];
            Type effInfoType;
            m_effectInfoDict.TryGetValue(eff.type, out effInfoType);
            if(null == effInfoType)
                effInfoType = typeof (BuffEffectInfo);

            var effInfo = Activator.CreateInstance(effInfoType) as BuffEffectInfo ?? new BuffEffectInfo();
            m_effInfos[i] = effInfo;
            effInfo.Set(eff, this);

            CalculateEffectAmount(effInfo, eff);

            if (eff.specialTrigger)
            {
                InitializeEffectListener(effInfo, eff);
                continue;
            }
            if (!eff.ignoreTrigger && !info.trigger.isNormal)
                continue;

            if (eff.interval < 1 && !eff.waitKeyEffect)
            {
                effInfo.userParam1 = context.target;
                Apply(effInfo);
            }
        }

        for (var i = 0; i < info.effects.Length; ++i)
            owner.behaviour.effects.PlayEffect(info.effects[i], this);

        var tt = info.trigger.type;
        var ft = tt == BuffInfo.BuffTriggerTypes.Health ? CreatureFields.Health : tt == BuffInfo.BuffTriggerTypes.Rage ? CreatureFields.Rage : tt == BuffInfo.BuffTriggerTypes.Field ? (CreatureFields) (int) info.trigger.param0 : CreatureFields.Unused0;
        if (ft != CreatureFields.Unused0)
        {
            m_actived = false;
            OnListenerFieldChanged(Event_.Pop(ft));
        }

        if (tt == BuffInfo.BuffTriggerTypes.TotalDamage)
        {
            var pss = info.trigger.paramss;
            var sc = BuffInfo.MAX_TRIGGER_PARAM_COUNT - 1; // Max step count
            for (var i = 0; i < BuffInfo.MAX_BUFF_EFFECT_COUNT && i < sc; ++i)
                if (pss[i] > 0) m_maxStep = i + 1;
        }

        #region Debug log

        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        Logger.LogDetail("[{4}:{5}-{6}], Buff [{0}:{1}] added to creature [{2}:{3}]", ID, info.name, owner.id, owner.uiName, Level.levelTime, owner.frameTime, owner.frameCount);
        #endif

        #if AI_LOG
        Module_AI.LogBattleMsg(owner, "[Buff [{0}:{1}] added to creature [{2}:{3}]", ID, info.name, owner.uiName, owner.teamIndex);
        #endif

        #endregion
    }

    private void InitDataHandler()
    {
        if (dataHandler == null)
            dataHandler = new DataHandler();
        else
        {
            dataHandler.Clear();
            CommonMath.AttachCommonMath(dataHandler);
        }

        dataHandler.AttachMethod("Owner", Owner);
        dataHandler.AttachMethod("Trigger", Trigger);
        dataHandler.AttachMethod("Source", Source);
        dataHandler.AttachMethod("Random", Random);
    }

    private void InitializeDestoryListeners()
    {
        if (info.destoryTrigger.isNormal) return;

        switch (info.destoryTrigger.type)
        {
            case BuffInfo.BuffTriggerTypes.ComboBreak:
                owner.AddEventListener(CreatureEvents.COMBO_BREAK, this.OnDestoryTrigger_ComboBreak);
                break;
        }
    }

    private void OnDestoryTrigger_ComboBreak(Event_ e)
    {
        if ((int) e.param1 < info.destoryTrigger.param0)
            return;
        Remove();
    }

    private Creature GetCreatureByTargetDivision(AdditionalParameters rParam, StateMachineInfo.FollowTargetEffect.TargetDivision rDivision)
     {
         var trigger = (rParam != null ? rParam.trigger : owner) ?? owner;

         switch (rDivision)
         {
            case StateMachineInfo.FollowTargetEffect.TargetDivision.Owner:      return owner;
            case StateMachineInfo.FollowTargetEffect.TargetDivision.OwnerPet:   return owner?.pet ?? owner;
            case StateMachineInfo.FollowTargetEffect.TargetDivision.Target:     return trigger;
            case StateMachineInfo.FollowTargetEffect.TargetDivision.TargetPet:  return trigger?.pet ?? trigger;
        }
        return owner;
     }

    private void OnFollowEffectFinish(Event_ e)
    {
        m_followTriggerInfo.Trigger();
        if (!m_followTriggerInfo.isTrigger)
            return;
        dataHandler.SetVariable("triggerParam", Convert.ToDouble(e.param1));
        OnTrigger(false, AdditionalParameters.Create(null, Convert.ToDouble(e.param1)), true);
    }
    
    public bool HasEffect(BuffInfo.EffectTypes type)
    {
        for (var i = 0; i < info.buffEffects.Length; ++i)
            if (info.buffEffects[i].type == type) return true;

        return false;
    }

    public BuffInfo.EffectTypes HasSameEffectType(BuffInfo _info)
    {
        for (var i = 0; i < info.buffEffects.Length; ++i)
        {
            for (var j = 0; j < _info.buffEffects.Length; ++j)
                if (info.buffEffects[i].type == _info.buffEffects[j].type) return info.buffEffects[i].type;
        }

        return BuffInfo.EffectTypes.Unknow;
    }

    public void Update(int diff)
    {
        m_pending = false;

        if (destroyed) return;

        if (pendingDestroy)
        {
            _Remove();
            return;
        }

        if (m_delayTime > 0)
        {
            m_delayTime -= diff;
            if (m_delayTime <= 0)
                ActiveBuff();
            else
                return;
        }

        if (info.trigger.type == BuffInfo.BuffTriggerTypes.ElementDamage && info.trigger.param6 > 0) // Calculate element damage damping
        {
            for (int i = 0, c = m_elementDamage.Length; i < c; ++i)
            {
                var d = m_elementDamage[i];
                if ((d -= info.trigger.param6 * diff) < 0) d = 0;
                m_elementDamage[i] = d;
            }
        }

        if (actived && info.trigger.isNormal)
        {
            for (var i = 0; i < m_effectCount; ++i)
            {
                var effInfo = m_effInfos[i];
                if (effInfo.Update(diff))
                {
                    effInfo.userParam0 = 0;
                    effInfo.userParam1 = context.target;
                    effInfo.damageInfo = null;
                    effInfo.attackInfo = null;

                    Apply(effInfo);
                }
            }
        }

        if (m_effectCount > 0 && m_activeEffectCount < 1)
        {
            Remove();
            return;
        }

        if (m_infinity) return;

        duration -= diff;

        if (duration <= 0) Remove();
    }

    /// <summary>
    /// Remove buff
    /// </summary>
    /// <param name="interrupt">Remove type 0 = normal buff end  1 = interrupt  2 = owner dead  3 = override by other buff, 4 = discarded</param>
    /// <param name="ignorePending"></param>
    public void Remove(int interrupt = 0, bool ignorePending = false)
    {
        if (destroyed || pendingDestroy || ignorePending && m_pending) return;
        pendingDestroy = true;

        m_interrupted = interrupt;

        owner?.DispatchEvent(CreatureEvents.BUFF_REMOVE, Event_.Pop(this));
    }

    private void _Remove()
    {
        for (var i = 0; i < info.endEffects.Length; ++i)
            owner.behaviour.effects.PlayEffect(info.endEffects[i]);

        Destroy();
    }

    protected void SetActiveState(bool state)
    {
        if (m_actived == state) return;
        m_actived = state;

        for (int i = 0; i < m_effectCount; ++i)
        {
            var effInfo = m_effInfos[i];
            if (effInfo.effect.interval < 1)
                Apply(effInfo, !m_actived);
        }
    }

    protected override void OnDestroy()
    {
        EventManager.RemoveEventListener(this);

        if (!owner)
        {
            owner  = null;
            context.Reset();

            return;
        }

        for (var i = 0; i < m_effectCount; ++i)
        {
            var effInfo = m_effInfos[i];
            if (effInfo.effect == null) continue;

            switch (effInfo.effect.type)
            {
                case BuffInfo.EffectTypes.ModifyAttribute: if (info.resetFields && effInfo.effect.param3 == 0) owner.ModifyField((int)effInfo.effect.param0, -effInfo.amount0); break;
                case BuffInfo.EffectTypes.Invincible:
                case BuffInfo.EffectTypes.Tough:
                case BuffInfo.EffectTypes.Freez:
                case BuffInfo.EffectTypes.ClearAttackBox:
                case BuffInfo.EffectTypes.SlowClock:
                case BuffInfo.EffectTypes.MaskColor:
                case BuffInfo.EffectTypes.Mark:
                case BuffInfo.EffectTypes.HawkEye:
                case BuffInfo.EffectTypes.NightWatch:
                case BuffInfo.EffectTypes.SwitchMorph:
                case BuffInfo.EffectTypes.Steal:
                case BuffInfo.EffectTypes.DarkScreen:   Apply(effInfo, true); break;
                default: break;
            }

            effInfo.Set(null, this);
        }

        #region Debug log
        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        Logger.LogDetail("[{4}:{5}-{6}], Buff [{0}:{1}] removed from creature [{2}:{3}]", ID, info.name, owner.id, owner.uiName, Level.levelTime, owner.frameTime, owner.frameCount);
        #endif

        #if AI_LOG
        Module_AI.LogBattleMsg(owner, "[ Buff [{0}:{1}] removed from creature [{2}:{3}]", ID, info.name, owner.uiName, owner.teamIndex);
        #endif
        #endregion

        owner = null;
        context.Reset();
    }

    private void CalculateEffectAmount(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff)
    {
        switch (eff.type)
        {
            case BuffInfo.EffectTypes.Shield:
            case BuffInfo.EffectTypes.ShieldForBurst:
                if (eff.expression.IsValid)
                {
                    effInfo.maxAmount0 = eff.expression.GetValue(dataHandler);
                    break;
                }
                effInfo.maxAmount0 = eff.param1 == 0 ? owner.maxHealth * eff.param0 : eff.param0;
                break;
            case BuffInfo.EffectTypes.Heal:
                effInfo.maxAmount0 = eff.param4 <= 0 ? -1 : eff.param5 == 0 ? owner.maxHealth * eff.param4 : eff.param4;   // Total amount limit
                effInfo.maxAmount1 = eff.param2 <= 0 ? -1 : eff.param3 == 0 ? owner.maxHealth * eff.param2 : eff.param2;   // Per tick amount limit
                break;
            default:
                break;
        }
    }

    #region Trigger

    private void Apply(BuffEffectInfo effInfo, bool reverse = false)
    {
        FightRecordManager.RecordLog<LogBuffApply>(log =>
        {
            log.tag = (byte)TagType.BuffApply;
            log.buffId = info.ID;
            log.type = (sbyte)effInfo.type;
        });

        if (effInfo.effect.isEmpty) return;
        using (new VariableScope(dataHandler, effInfo.variables))
        {
            var applied = m_effectHandlers[effInfo.type](effInfo, effInfo.effect, reverse);
            if (applied && !reverse)
            {
                owner.DispatchEvent(CreatureEvents.BUFF_TRIGGER, Event_.Pop(this));

                effInfo.applyCount++;
                if (effInfo.maxApplyACount > 0 && effInfo.applyCount >= effInfo.maxApplyACount)
                {
                    effInfo.actived = false;
                    m_activeEffectCount = effInfo.keyEffect ? 0 : m_activeEffectCount - 1;
                }
            }
        }
        #region Debug log
#if (DEVELOPMENT_BUILD || UNITY_EDITOR) && DETAIL_BATTLE_LOG
        Logger.LogDetail("[{7}:{8}-{9}], Buff::Apply: Apply buff effect [{0}:{1}:{6}] on creature [{2}:{3}], source [{4}:{5}]", info.ID, effInfo.effect.type, owner.id, owner.uiName, source.id, source.name, reverse ? "reverse" : "apply", Level.levelTime, owner.frameTime, owner.frameCount);
#endif

#if AI_LOG
        Module_AI.LogBattleMsg(owner, "[Buff::Apply: Apply buff effect [{0}:{1}:{2}] on creature [{3}], source [{4}]", info.ID, effInfo.effect.type, reverse ? "reverse" : "apply", !owner ? "null" : owner.name, !source ? "null" : source.name);
#endif
        #endregion
    }

    private void OnTrigger(bool reverse = false, AdditionalParameters rParam = null, bool followEnd = false)
    {
        if (!reverse && Module_Battle.Range() >= info.trigger.chance) return;

        if (!reverse && info.trigger.triggerMax > 0 && m_triggerInfo.triggerAmount >= info.trigger.triggerMax)
        {
            return;
        }

        if (!followEnd)
        {
            m_triggerInfo.Trigger();

            if (info.stepEffects != null && info.stepEffects.Length >= m_triggerInfo.triggerCount)
            {
                var e = owner.behaviour.effects.PlayEffect(info.stepEffects[m_triggerInfo.triggerCount - 1], this);
                stepEffectList.Add(e);
            }

            if (!m_triggerInfo.isTrigger) return;

            m_triggerInfo.Reset();
            for (var m = 0; m < stepEffectList.Count; m++)
            {
                stepEffectList[m].Destroy();
            }
            stepEffectList.Clear();

            if (!reverse && info.followEffects != null && info.followEffects.Length > 0)
            {
                for (var i = 0; i < info.followEffects.Length; ++i)
                {
                    var effect = owner.behaviour.effects.PlayEffect(info.followEffects[i], this,
                        GetCreatureByTargetDivision(rParam, info.followEffects[i].targetBind),
                        GetCreatureByTargetDivision(rParam, info.followEffects[i].initBind));
                    effect.AddEventListener(Effect.FOLLOW_EFFECT_DESTORY, OnFollowEffectFinish);
                }
                //如果有follow特效。其他逻辑效果需要等到follow特效结束再执行
                return;
            }
        }
        else
        {
            m_followTriggerInfo.Trigger();
            if (!m_followTriggerInfo.isTrigger)
                return;
            m_followTriggerInfo.Reset();
        }

        for (var i = 0; i < m_effectCount; ++i)
        {
            var effInfo = m_effInfos[i];
            if (effInfo.effect.specialTrigger) continue;

            effInfo.userParam0 = 0;
            effInfo.damageInfo = null;
            effInfo.attackInfo = null;
            if (rParam != null)
            {
                effInfo.userParam1 = rParam.trigger;
                effInfo.triggerParam = rParam;
            }
            Apply(effInfo, reverse);
        }

        rParam?.Destroy();
    }

    private void OnTrigger(SpecialFlag flag, ref double userParam0, Creature userParam1 = null, DamageInfo userParam2 = null, AttackInfo userParam3 = null)
    {
        if (Module_Battle.Range() >= info.trigger.chance) return;

        bool isKeyEffect = false;
        for (var i = 0; i < m_effectCount; ++i)
        {
            var effInfo = m_effInfos[i];

            if (effInfo == null || effInfo.ignoreTrigger)
                continue;

            if ((effInfo.flag & flag) == flag)
            {
                isKeyEffect |= effInfo.keyEffect;
                effInfo.userParam0 = userParam0;
                effInfo.userParam1 = userParam1;
                effInfo.damageInfo = userParam2;
                effInfo.attackInfo = userParam3;
                Apply(effInfo);
                userParam0 = effInfo.userParam0;
            }
        }
        //处理关键效果触发时，同时触发的效果
        if (isKeyEffect)
        {
            for (var i = 0; i < m_effectCount; ++i)
            {
                var effInfo = m_effInfos[i];
                if (effInfo.effect.waitKeyEffect)
                {
                    effInfo.userParam0 = userParam0;
                    effInfo.userParam1 = userParam1;
                    effInfo.damageInfo = userParam2;
                    effInfo.attackInfo = userParam3;
                    Apply(effInfo);
                    userParam0 = effInfo.userParam0;
                }
            }
        }
    }

    #endregion

    #region Event listeners

    private void InitializeListeners()
    {
        owner.AddEventListener(CreatureEvents.DEAD, OnOwnerDead);

        if (info.trigger.isNormal) return;
        var listener = info.trigger.listenSource && source != null ? source : owner;
        if (listener is PetCreature)
            listener = ((PetCreature) listener).ParentCreature;

        listener.AddEventListener(CreatureEvents.QUIT_STATE, OnListenerQuitState);

        switch (info.trigger.type)
        {
            case BuffInfo.BuffTriggerTypes.StartFight:    EventManager.AddEventListener(LevelEvents.START_FIGHT,                    OnLevelStartFight);      break;
            case BuffInfo.BuffTriggerTypes.CritHurted:
            case BuffInfo.BuffTriggerTypes.TotalDamage:
            case BuffInfo.BuffTriggerTypes.PercentDamage:
            case BuffInfo.BuffTriggerTypes.ElementDamage:
            case BuffInfo.BuffTriggerTypes.TakeDamage:    listener.AddEventListener(CreatureEvents.TAKE_DAMAGE,                        OnListenerTakeDamage);      break;
            case BuffInfo.BuffTriggerTypes.Attacked:      listener.AddEventListener(CreatureEvents.ATTACKED,                           OnListenerAttacked);        break;
            case BuffInfo.BuffTriggerTypes.Shooted:       listener.AddEventListener(CreatureEvents.SHOOTED,                            OnListenerShooted);         break;
            case BuffInfo.BuffTriggerTypes.Health:
            case BuffInfo.BuffTriggerTypes.Rage:
            case BuffInfo.BuffTriggerTypes.Field:         listener.AddEventListener(CreatureEvents.FIELD_CHANGED,                      OnListenerFieldChanged);    break;
            case BuffInfo.BuffTriggerTypes.TargetHealth:
            case BuffInfo.BuffTriggerTypes.TargetRage:
            case BuffInfo.BuffTriggerTypes.TargetGroup:
            case BuffInfo.BuffTriggerTypes.TargetField:   listener.AddEventListener(CreatureEvents.CALCULATE_DAMAGE,                   OnListenerCalculateDamage); break;
            case BuffInfo.BuffTriggerTypes.FirstHit:      listener.AddEventListener(CreatureEvents.FIRST_HIT,                          OnAttackBoxFirstHit);    break;
            case BuffInfo.BuffTriggerTypes.Kill:          listener.AddEventListener(CreatureEvents.KILL_TARGET_DEAD_ANIMATION_END,     OnKill);                 break;
            //            case BuffInfo.BuffTriggerTypes.Dying:          listener.AddEventListener(CreatureEvents.Dying,                              OnDying);                break;
            case BuffInfo.BuffTriggerTypes.EnterState:    listener.AddEventListener(CreatureEvents.ENTER_STATE, OnListenerEnterState); break;
            case BuffInfo.BuffTriggerTypes.Crit:
            case BuffInfo.BuffTriggerTypes.DamageOverFlow:
            case BuffInfo.BuffTriggerTypes.DealDamage:    listener.AddEventListener(CreatureEvents.DEAL_DAMAGE,                        OnListenerDealDamage);      break;
            case BuffInfo.BuffTriggerTypes.Combo:
            case BuffInfo.BuffTriggerTypes.Attack:        listener.AddEventListener(CreatureEvents.ATTACK,                             OnListenerAttack);      break;
            case BuffInfo.BuffTriggerTypes.BeKill:        listener.AddEventListener(CreatureEvents.BE_KILL,                            OnListenerBeKill); break;
            default: break;
        }
    }

    private void InitializeEffectListener(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff)
    {
        if (!eff.specialTrigger) return;

        switch (eff.type)
        {
            case BuffInfo.EffectTypes.Shield:
            case BuffInfo.EffectTypes.Steady:
            case BuffInfo.EffectTypes.ShieldForBurst:
                effInfo.flag |= SpecialFlag.WillTakeDamage;
                owner.AddEventListener(CreatureEvents.WILL_TAKE_DAMAGE, OnOwnerWillTakeDamage);
                break;
            case BuffInfo.EffectTypes.ReflectDamage:
                effInfo.flag |= SpecialFlag.TakeDamage;
                owner.AddEventListener(CreatureEvents.TAKE_DAMAGE, OnListenerTakeDamage);
                break;
            case BuffInfo.EffectTypes.NightWatch:
                effInfo.flag |= SpecialFlag.TakeDamage | SpecialFlag.PreDamage;
                owner.AddEventListener(CreatureEvents.TAKE_DAMAGE, OnListenerTakeDamage);
                break;
            case BuffInfo.EffectTypes.ImmuneDeath:
                effInfo.flag |= SpecialFlag.Dying;
                owner.AddEventListener(CreatureEvents.Dying, OnOwnerDying);
                break;
            case BuffInfo.EffectTypes.Berserker:
                effInfo.flag |= SpecialFlag.Shooted;
                owner.AddEventListener(CreatureEvents.SHOOTED, OnListenerShooted);
                break;
            default: break;
        }
    }

    protected bool CheckConditionExpression()
    {
        if (info.trigger.expression.IsValid)
            return true;

        using (new VariableScope(dataHandler, info.trigger.expression.GetVariables(m_level)))
        {
            return info.trigger.expression.IsTrue(dataHandler);
        }
    }

    protected void OnOwnerDead()
    {
        if (info.trigger.type == BuffInfo.BuffTriggerTypes.Dead) OnTrigger();

        double u0 = 0;
        OnTrigger(SpecialFlag.Dead, ref u0);

        if (owner.isDead && info.removeOnDead)
            Remove(2);
    }

    protected void OnOwnerDying()
    {
        double u0 = 0;
        OnTrigger(SpecialFlag.Dying, ref u0);
    }

    protected void OnLevelStartFight()
    {
        OnTrigger();
    }

    protected void OnListenerTakeDamage(Event_ e)
    {
        var d = e.param2 as DamageInfo;

        if (d != null && d.uiDamage) return; // Ignore ui damage

        var s = e.param1 as Creature;
        conditionTrigger = s;
        var r = (double)(int)e.param3;

        if (d == null) return;  // Damage can not be null

        if (UseExpression()) return;

        var tt = info.trigger.type;
        if (tt == BuffInfo.BuffTriggerTypes.TakeDamage || tt == BuffInfo.BuffTriggerTypes.CritHurted && d.crit) OnTrigger();
        else if (!d.preCalculate)
        {
            if (m_maxStep > 0) // BuffTrigger type == TotalDamage
            {
                FightRecordManager.RecordLog< LogInt>(log =>
                {
                    log.tag = (byte)TagType.maxStep;
                    log.value = m_maxStep;
                });

                for (int i = 0, c = m_totalDamage.Length; i < c; ++i)
                    m_totalDamage[i] += d.finalDamage / owner.GetField(CreatureFields.MaxHealth);
                OnOwnerTotalDamage();
            }
            else if (tt == BuffInfo.BuffTriggerTypes.ElementDamage && !owner.elementAffected && d.elementType > 0 && d.elementType < CreatureElementTypes.Count && d.finalElementDamage > 0)
            {
                var idx = (int)d.elementType - 1; // Ignore CreatureElementTypes.None
                m_elementDamage[idx] += d.finalElementDamage;
                OnOwnerElementDamage(idx);

                FightRecordManager.RecordLog< LogElementDamage>(log =>
                {
                    log.tag = (byte)TagType.maxStep;
                    log.elementType = (byte)d.elementType;
                    log.damage = d.finalElementDamage;
                });
            }
            else if (tt == BuffInfo.BuffTriggerTypes.PercentDamage)
            {
                m_currentStatisicDamage += d.finalDamage;
                double conditionValue = info.trigger.param1;
                if(info.trigger.param2.Equals(0))
                    conditionValue = info.trigger.param1*owner.GetField((CreatureFields)info.trigger.param0);
                int max = 100;
                //Logger.LogError("受到伤害 {0} 条件值 {1}", d.finalDamage, conditionValue);

                FightRecordManager.RecordLog<LogPercentDamage>(log =>
                {
                    log.tag = (byte)TagType.PercentDamage;
                    log.currentStatisicDamage = m_currentStatisicDamage;
                    log.param0 = info.trigger.param0;
                    log.param1 = info.trigger.param1;
                    log.property = owner.GetField((CreatureFields)info.trigger.param0);
                    log.damage = d.finalDamage;
                });

                while (m_currentStatisicDamage >= conditionValue && max > 0)
                {
                    m_currentStatisicDamage -= conditionValue;
                    OnTrigger();
                    //防止配置错误造成死循环。最多只循环100次
                    max--;
                }
            }
        }

        var flag = d.preCalculate ? SpecialFlag.PreDamage : SpecialFlag.TakeDamage;

        OnTrigger(flag, ref r, s, d);

        e.param3 = (int)r;
    }

    private bool UseExpression()
    {
        if (info.trigger.useExpression)
        {
            if (CheckConditionExpression())
                OnTrigger();
            return true;
        }
        return false;
    }

    /// <summary>
    /// { "lockTrigger"，"actionMask"}
    /// </summary>
    protected void OnListenerAttacked(Event_ e)
    {
        var attacker   = e.param1 as Creature;
        conditionTrigger = attacker;
        var attackInfo = e.param2 as AttackInfo;

        if (UseExpression()) return;

        if (info.trigger.type == BuffInfo.BuffTriggerTypes.Attacked)
        {
            if (info.trigger.param0.Equals(0) || attacker == source)
            {
                if (attacker != null && (info.trigger.param1.Equals(0) ||
                (attacker.stateMachine.currentState.isUltimate && ((int)info.trigger.param1 & 1) > 0) ||
                (attacker.stateMachine.currentState.isExecution && ((int)info.trigger.param1 & 2) > 0)))
                    OnTrigger();
            }
        }

        double u0 = 0;
        OnTrigger(SpecialFlag.Attacked | SpecialFlag.Shooted, ref u0, attacker, null, attackInfo);
    }

    protected void OnListenerShooted(Event_ e)
    {
        var attacker   = e.param1 as Creature;
        conditionTrigger = attacker;
        var attackInfo = e.param2 as AttackInfo;

        if (info.trigger.type == BuffInfo.BuffTriggerTypes.Shooted) OnTrigger();

        double u0 = 0;
        OnTrigger(SpecialFlag.Shooted, ref u0, attacker, null, attackInfo);
    }

    /// <summary>
    /// { "value", "isValue" }
    /// { "attributeID", "value" }
    /// </summary>
    protected void OnListenerFieldChanged(Event_ e)
    {
        double compVal = 0, val = 0;

        var tt     = info.trigger.type;
        var field  = (CreatureFields)e.param1;

        if (tt == BuffInfo.BuffTriggerTypes.Field)
        {
            if ((int)info.trigger.param0 != (int)field) return;

            val = owner.GetField(field);
            compVal = info.trigger.param1;
        }
        else
        {
            if (tt == BuffInfo.BuffTriggerTypes.Health)
            {
                if (field != CreatureFields.Health) return;
                val = info.trigger.param1 != 0 ? owner.health : owner.healthRate;
            }
            else
            {
                if (field != CreatureFields.Rage) return;
                val = info.trigger.param1 != 0 ? owner.rage : owner.rageRate;
            }
            compVal = info.trigger.param0;
        }

        SetActiveState(compVal < 0 && val < -compVal || compVal > 0 && val > compVal);
    }

    private void OnDying(Event_ e)
    {
        OnTrigger();
    }

    private void OnListenerQuitState(Event_ e)
    {
        //触发者只针对当前动作有效。切换动作后需要清空
        conditionTrigger = null;
        dataHandler.RemoveVariable("triggerParam");
        var state = e.param1 as StateMachineState;
        if (state == null) return;

        if (info.trigger.type != BuffInfo.BuffTriggerTypes.UseSkill)
            return;

        if (state.isUltimate && ((int)info.trigger.param0 & 1) > 0)
            OnTrigger();

        if (state.isExecution && ((int)info.trigger.param0 & 2) > 0)
            OnTrigger();
    }

    private void OnListenerEnterState(Event_ e)
    {
        var state = e.param2 as StateMachineState;
        if (state == null) return;

        if (info.trigger.type == BuffInfo.BuffTriggerTypes.EnterState)
        {
            var group = (int)info.trigger.param0;
            if (state.info.groupMask.BitMask(group))
                OnTrigger();
        }
    }

    /// <summary>
    /// { actionMask , mark}
    /// </summary>
    /// <param name="e"></param>
    private void OnListenerAttack(Event_ e)
    {
        var target = e.param1 as Creature;
        conditionTrigger = target;
        if (!target) return;

        if (info.trigger.type == BuffInfo.BuffTriggerTypes.Combo)
        {
            dataHandler.SetVariable("triggerParam", owner.comboCount);
            if (owner.comboCount == (int)info.trigger.param0) OnTrigger(false, AdditionalParameters.Create(target, owner.comboCount));
            return;
        }

        if (!info.trigger.param1.Equals(0) && !owner.markList.Contains(target))
            return;

        if (info.trigger.param0.Equals(0))
            OnTrigger(false, AdditionalParameters.Create(target, 0));
        var state = owner.stateMachine.currentState;
        if (state == null) return;

        if (state.isUltimate && ((int)info.trigger.param0 & 1) > 0)
            OnTrigger(false, AdditionalParameters.Create(target, 0));

        if (state.isExecution && ((int)info.trigger.param0 & 2) > 0)
            OnTrigger(false, AdditionalParameters.Create(target, 0));
    }

    private void OnListenerDealDamage(Event_ e)
    {
        conditionTrigger = e.param1 as Creature;
        if (info.trigger.type == BuffInfo.BuffTriggerTypes.Crit)
        {
            var d = e.param2 as DamageInfo;
            if (d && d.crit)
            {
                dataHandler.SetVariable("triggerParam", d.finalDamage);
                OnTrigger(false, AdditionalParameters.Create(e.param1 as Creature, d.finalDamage));
            }
        }
        else if (info.trigger.type == BuffInfo.BuffTriggerTypes.DamageOverFlow)
        {
            var type = (CreatureFields)info.trigger.param0;
            var value = info.trigger.param1;
            var isValue = Mathd.Abs(info.trigger.param2) > Mathd.Epsilon;
            var target = e.param1 as Creature;
            var d = e.param2 as DamageInfo;
            if (d && target && d.finalDamage >= (isValue ? value : target.GetField(type)*value))
            {
                dataHandler.SetVariable("triggerParam", d.finalDamage);
                OnTrigger(false, AdditionalParameters.Create((Creature) e.param1, d.finalDamage));
            }
        }
        else
            OnTrigger(false, AdditionalParameters.Create(e.param1 as Creature, 0));
    }


    private void OnKill(Event_ e)
    {
        conditionTrigger = e.param1 as Creature;
        OnTrigger(false, AdditionalParameters.Create(e.param1 as Creature, 0));
    }

    private void OnListenerBeKill(Event_ e)
    {
        conditionTrigger = e.param1 as Creature;
        OnTrigger(false, AdditionalParameters.Create(e.param1 as Creature, 0));
    }

    /// <summary>
    /// { "value", "isValue" }
    /// { "attributeID", "value" }
    /// </summary>
    protected void OnListenerCalculateDamage(Event_ e)
    {
        var victim  = e.param1 as Creature;
        if (victim == null) return;
        conditionTrigger = victim;

        var tt = info.trigger.type;
        double compVal = 0, val = 0;

        if (tt == BuffInfo.BuffTriggerTypes.TargetField)
        {
            val = victim.GetField((int)info.trigger.param0);
            compVal = info.trigger.param1;
        }
        else if (tt == BuffInfo.BuffTriggerTypes.TargetGroup)
        {
            if (victim.stateMachine.currentState.info.groupMask.BitMask((int) info.trigger.param0))
            {
                dataHandler.SetVariable("triggerParam", val);
                OnTrigger(false, AdditionalParameters.Create(victim, val));
            }
            return;
        }
        else
        {
            compVal = info.trigger.param0;
            if (!info.trigger.param1.Equals(0))
                val = tt == BuffInfo.BuffTriggerTypes.TargetHealth ? victim.health : victim.rage;
            else
                val = tt == BuffInfo.BuffTriggerTypes.TargetHealth ? victim.healthRate : victim.rageRate;
        }

        if (compVal < 0 && val < -compVal || compVal > 0 && val > compVal || (compVal.Equals(0) && val.Equals(compVal)))
        {
            dataHandler.SetVariable("triggerParam", val);
            OnTrigger(false, AdditionalParameters.Create(victim, val));
        }
    }

    protected void OnAttackBoxFirstHit(Event_ e)
    {
        conditionTrigger = (Creature) e.param1;
        OnTrigger(false, AdditionalParameters.Create((Creature)e.param1, 0));
    }

    protected void OnOwnerWillTakeDamage(Event_ e)
    {
        var c = e.param1 as Creature;
        var d = e.param2 as DamageInfo;

        if (d == null) return; // Damage can not be null

        double u0 = 0;
        OnTrigger(d.preCalculate ? SpecialFlag.WillPreDamage : SpecialFlag.WillTakeDamage, ref u0, c, d);

        if (d.finalDamage < 1) e.cancle = true;
    }

    /// <summary>
    /// { "step1", "step2", "step3", "step4", "step5", "isValue" }
    /// </summary>
    protected void OnOwnerTotalDamage()
    {
        var cmp = info.trigger.paramss;

        for (var i = m_maxStep - 1; i > -1; --i)
        {
            if (m_totalDamage[i] >= cmp[i])
            {
                for (var j = i; j > -1; --j) m_totalDamage[j] = 0;

                if (i < m_effectCount)
                {
                    var effInfo = m_effInfos[i];

                    effInfo.userParam0 = 0;
                    effInfo.userParam1 = null;
                    effInfo.damageInfo = null;
                    effInfo.attackInfo = null;
                    Apply(effInfo, false);
                }
            }
        }
    }

    /// <summary>
    /// { "value", "isPercent", "notRest", "damping" }
    /// </summary>
    protected void OnOwnerElementDamage(int index)
    {
        if (owner.isDead || owner.invincible || owner.currentState.ignoreElementTrigger) return; // Ignore dead, invincible

        var pt = owner.stateMachine.GetLong(StateMachineParam.forcePassive) >> 48 & 0xFFFF;
        if (pt.BitMask(StateMachine.PASSIVE_CATCH) || pt.BitMask(StateMachine.PASSIVE_WEAK)) return;

        var c = info.trigger.param1 != 0 ? info.trigger.param0 * owner.GetField(CreatureFields.MaxHealth) : info.trigger.param0;
        var v = m_elementDamage[index];

        if (v >= c)
        {
            if (info.trigger.param2 == 0) m_elementDamage[index] = 0;
            if (index < m_effectCount)
            {
                var effInfo = m_effInfos[index];

                effInfo.userParam0 = 0;
                effInfo.userParam1 = null;
                effInfo.damageInfo = null;
                effInfo.attackInfo = null;
                Apply(effInfo, false);
            }
        }
    }

    #endregion

    #region Effect Handlers

    protected bool HandleEffectUnKnow(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        return true;
    }

    /// <summary>
    /// params = { "attributeID", "value", "isPercent" , "notRest", "isSpecial", "targetAttributeID"}
    /// </summary>
    protected bool HandleEffectModifyAttribute(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        //修改角色属性逻辑。当角色死亡后不处理
        if (owner.isDead) return true;

        var field = (int)effInfo[0];
        var targetField = (int)effInfo[5];
        if (targetField == (int)CreatureFields.Unused0)
            targetField = field;

        if (reverse)
        {
            if (effInfo[4].Equals(0))
                owner.ModifyField(field, -effInfo.amount0);
            else
                owner.AddSpecialProperty(source, (CreatureFields)field, -effInfo.amount0);
            effInfo.amount0 = 0;
            return false;
        }

        var cv = 0d;
        if (effInfo.effect.expression.IsValid)
        {
            var watcher = TimeWatcher.Watch("计算表达式");
            cv = effInfo.effect.expression.GetValue(dataHandler);
            watcher.See($"值{cv},表达式消耗");
            watcher.Stop();
            watcher.UnWatch();
        }
        else
        {
            if (effInfo[1].Equals(0)) return true;
            var conditionValue = effInfo.triggerParam?.triggerParam ?? 0;
            var ov = owner.GetField(targetField);
            var effectValue = effInfo[1] + conditionValue * eff.conditionCoefficient;
            cv = effInfo[2].Equals(0) ? effectValue : effectValue*ov;
        }

        if (targetField == (int)CreatureFields.Health)
            cv = HandleHealth(eff, cv);
        else if (effInfo[4].Equals(0))
            cv = owner.ModifyField(field, cv);
        else
            cv = owner.AddSpecialProperty(source, (CreatureFields)field, cv);

        effInfo.amount0 += cv;
        return true;
    }

    private double HandleHealth(BuffInfo.BuffEffect eff, double hv)
    {
        var old = owner.health;

        owner.TakeUIDamage(source, (int)Mathd.Abs(hv), eff.type, eff.flag == BuffInfo.EffectFlags.Unknow ? hv < 0 ? BuffInfo.EffectFlags.Damage : BuffInfo.EffectFlags.Heal : eff.flag);

        return owner.health - old;
    }

    protected bool HandleEffectInvincible(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        owner.invincibleCount += reverse ? -1 : 1;
        if (eff.param0 == 0) owner.rimCount += reverse ? -1 : 1;
        return true;
    }

    /// <summary>
    /// params = { level, noRimLight }
    /// </summary>
    protected bool HandleEffectTough(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        if (!reverse)
        {
            effInfo.refToughID = owner.AddToughState((int)eff.param0);
            if (eff.param1 == 0) owner.rimCount += 1;
        }
        else
        {
            owner.RemoveToughState(effInfo.refToughID);
            if (eff.param1 == 0) owner.rimCount -= 1;
        }
        return true;
    }

    /// <summary>
    /// params = { value }
    /// </summary>
    protected bool HandleEffectBreakTough(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        var value = (int)eff.param0;
        if (eff.expression.IsValid)
        {
            value = (int)eff.expression.GetValue(dataHandler);
        }
        if (!reverse)
            effInfo.refBreakToughID = owner.AddBreakToughState(value);
        else
            owner.RemoveBreakToughState(effInfo.refBreakToughID);
        return true;
    }

    /// <summary>
    /// params = { value，setOrModify，isPercent,max }
    /// </summary>
    protected bool HandleEffectSetDuration(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        int value = 0;
        value = eff.param2.Equals(0) ? (int)eff.param0 : (int)(eff.param0*info.duration);
        if (eff.param1 > 0)
        {
            duration = value;
        }
        else
        {
            value = Math.Min(value, (int) eff.param3);
            duration += value;
        }
        return true;
    }

    /// <summary>
    /// params = { "value", "scope", "SelectType" }
    /// </summary>
    protected bool HandleEffectSlowClock(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        switch ((int) eff.param2)
        {
            case 0: //敌人
            {
                
                if(!reverse)
                    effInfo.effectList = ObjectManager.FindObjects<Creature>(
                    c => !c.SameCampWith(owner) && Vector3_.Distance(c.position_, owner.position_) <= eff.param1);
                if (effInfo.effectList != null)
                {
                    foreach (var c in effInfo.effectList)
                    {
                        if (c.isDead) continue;
                        c.stateMachine.speed += reverse ? -eff.param0 : eff.param0;
                        VoidHandler a = delegate { c.stateMachine.speed -= eff.param0; };
                        if (!reverse)
                            c.AddEventListener(CreatureEvents.DEAD, a);
                        else
                            c.RemoveEventListener(a);
                    }
                }
            }
            break;
            case 1: //自己
                owner.stateMachine.speed += reverse ? -eff.param0 : eff.param0;
                break;
            //找队友
        }
        return true;
    }

    /// <summary>
    /// params = { }
    /// </summary>
    protected bool HandleEffectHawkEye(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        owner.HawkEyeCount += reverse ? -1 : 1;
        return true;
    }

    /// <summary>
    /// params = { }
    /// </summary>
    protected bool HandleEffectMark(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        var creature = source as PetCreature;
        var marker = creature ? creature.ParentCreature : source;
        if (!marker) return true;

        if (reverse)
            marker.markList.Remove(owner);
        else
            marker.markList.Add(owner);
        return true;
    }

    /// <summary>
    /// params = { r, g, b, a}
    /// </summary>
    protected bool HandleEffectMarkColor(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        if(!reverse)
            Camera_Combat.SetMaskColor(effInfo[0], effInfo[1], effInfo[2], effInfo[3]);
        else
            Camera_Combat.ResetMaskColor();
        return true;
    }

    /// <summary>
    /// params = { "health", "rage", "isPercent" }
    /// </summary>
    protected bool HandleEffectRevive(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        if (!owner.isDead) return false;

        var health = eff.param2 == 1 ? owner.maxHealth * eff.param0 : eff.param0;
        var rage = eff.param2 == 1 ? owner.maxRage * eff.param1 : eff.param1;

        var oh = owner.health;
        var or = owner.rage;

        owner.health = (int)health;
        owner.rage = rage;

        effInfo.amount0 += owner.health - oh;
        effInfo.amount1 += owner.rage - or;

        return true;
    }

    /// <summary>
    /// params = { "buffID", "buffType" }
    /// </summary>
    protected bool HandleEffectDispel(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        if (eff.param0 != 0)
            owner.RemoveBuff((int)eff.param0, false, (int)eff.param2);
        else
            owner.RemoveBuff((BuffInfo.EffectTypes)(int)eff.param1);

        return true;
    }

    /// <summary>
    /// { "buffID" "toTrigger", "rate"}
    /// </summary>
    protected bool HandleEffectAddBuff(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        if (eff.param2 > 0 && Module_Battle.Range() > eff.param2)
            return true;

        var buffId = (int) eff.param0;
        if (eff.expression.IsValid)
        {
            buffId = (int)eff.expression.GetValue(dataHandler);
        }

        BuffContext context = new BuffContext();
        context.id = buffId;
        context.level = m_level;
        context.source = source ? source : owner;
        context.sourceBuff = this;
        context.target = effInfo.userParam1;

        if (eff.param1.Equals(0) || effInfo.userParam1 == null)
            context.owner = owner;
        else
            context.owner = effInfo.userParam1;

        Create(context);

        return true;
    }

    /// <summary>
    /// { "value", "isPercent" }
    /// </summary>
    protected bool HandleEffectShield(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        if (effInfo.maxAmount0 < 1 || effInfo.damageInfo == null || effInfo.damageInfo.finalDamage < 1) return false;

        var d = effInfo.damageInfo;
        var cv = d.finalDamage <= effInfo.maxAmount0 ? d.finalDamage : (int)effInfo.maxAmount0;

        d.finalDamage -= cv;
        d.absorbed += cv;

        effInfo.maxAmount0 -= cv;
        effInfo.amount0 += cv;

        if (effInfo.maxAmount0 < 1)
            m_activeEffectCount = effInfo.keyEffect ? 0 : m_activeEffectCount - 1;

        return true;
    }

    /// <summary>
    /// { "value", "isPercent" }
    /// </summary>
    protected bool HandleEffectShieldForBurst(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        if (effInfo.maxAmount0 < 1 || effInfo.damageInfo == null || effInfo.damageInfo.finalDamage < 1 || effInfo.damageInfo.finalDamage <= effInfo.maxAmount0) return false;

        var d = effInfo.damageInfo;
        var cv = d.finalDamage - (int)effInfo.maxAmount0;

        d.finalDamage = (int)effInfo.maxAmount0;
        d.absorbed += cv;

        effInfo.maxAmount0 = 0;
        effInfo.amount0 += cv;

        if (effInfo.maxAmount0 < 1)
            m_activeEffectCount = effInfo.keyEffect ? 0 : m_activeEffectCount - 1;

        return true;
    }
    /// <summary>
    /// { "useCount", "cd" }
    /// </summary>
    /// <param name="effInfo"></param>
    /// <param name="eff"></param>
    /// <param name="reverse"></param>
    /// <returns></returns>
    protected bool HandleEffectResetPetSkill(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        if (this.buffOwner is PetCreature)
        {
            var pet = buffOwner as PetCreature;
            pet.ResetSkill((int) eff.param0, (int)eff.param1);
        }
        return true;
    }

    protected bool HandleEffectResetDuration(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        if (eff.param0 > 0)
        {
            int value = 0;
            value = eff.param2 > 0 ? (int)(eff.param0*info.duration) : (int)eff.param0;
            duration += value;
            duration = (int)Mathd.Min(duration, eff.param3);
        }
        else if (eff.param1 > 0)
        {
            int value = 0;
            value = eff.param2 > 0 ? (int) eff.param1*info.duration : (int) eff.param1;
            duration = value;
        }
        return true;
    }

    /// <summary>
    /// { "value", "isValue", "chance" }
    /// </summary>
    protected bool HandleEffectReflectDamage(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        if (effInfo.damageInfo == null || effInfo.damageInfo.realDamage < 1 || Module_Battle.Range() > eff.param2) return false;

        if (eff.param0 == 0) return true;

        var rv = eff.param1 == 0 ? effInfo.damageInfo.realDamage * eff.param0 : eff.param0;
        effInfo.amount0 += rv;
        effInfo.userParam0 = rv;

        return true;
    }

    /// <summary>
    /// { "damage", "attributeID", "value", "isValue" }
    /// </summary>
    protected bool HandleEffectNightWatch(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        if (reverse)
        {
            owner.ModifyField((int)eff.param1, -effInfo.amount0);
            return false;
        }

        if (effInfo.damageInfo == null || effInfo.damageInfo.realDamage < 1) return false;

        if (eff.param0 == 0 || eff.param2 == 0) return true;

        var count = eff.param3 == 1 ? eff.param2 / eff.param0 : eff.param2 / (eff.param0 * owner.maxHealth);
        var rv = count * effInfo.damageInfo.realDamage;

        rv = owner.ModifyField((int)eff.param1, rv);

        effInfo.amount0 += rv;

        return true;
    }

    /// <summary>
    /// { "diff", "attributeID", "value", "isValue" }
    /// </summary>
    protected bool HandleEffectSteady(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        if (!effInfo.userParam1) return false;

        if (eff.param2 == 0) return true;

        var diff = eff.param3 == 1 ? owner.health - effInfo.userParam1.health : owner.healthRate - effInfo.userParam1.healthRate;
        if (eff.param0 < 0 && diff < eff.param0 || eff.param0 > 0 && diff > eff.param0)
        {
            var cv = owner.ModifyField((int)eff.param1, eff.param2);
            effInfo.amount0 += cv;
        }

        return true;
    }

    /// <summary>
    /// { "health", "rage", "isValue" }
    /// </summary>
    protected bool HandleEffectImmuneDeath(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        if (eff.param0 == 0 && eff.param1 == 0) return true;

        var hv = eff.param2 == 1 ? eff.param0 : eff.param0 * owner.maxHealth;
        var rv = eff.param2 == 1 ? eff.param1 : eff.param1 * owner.maxRage;

        var oldh = owner.health;
        var oldr = owner.rage;

        owner.health += (int)hv;
        owner.rage   += rv;

        var amounth = owner.health - oldh;
        effInfo.amount0 += amounth;

        var amountr = owner.rage - oldr;
        effInfo.amount1 += amountr;

        owner.behaviour.hitCollider.Clear();

        owner.stateMachine.SetParam(StateMachineParam.forcePassive, 0);
        owner.stateMachine.SetParam(StateMachineParam.immuneDeath, true);

        return true;
    }

    /// <summary>
    /// { "chance" }
    /// </summary>
    protected bool HandleEffectBerserker(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        if (owner.weak || effInfo.attackInfo == null || !effInfo.attackInfo.fromBullet || Module_Battle.Range() > eff.param0) return false;

        owner.stateMachine.SetParam(StateMachineParam.forcePassive, 0);

        return true;
    }

    /// <summary>
    /// { "value", "isValue", "limit", "limitIsValue", "max", "maxIsValue" }
    /// </summary>
    protected bool HandleEffectHeal(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        //治疗逻辑。当角色死亡后不处理
        if (owner.isDead) return true;
        double hv = 0;
        if (effInfo.effect.expression.IsValid)
        {
            hv = effInfo.effect.expression.GetValue(dataHandler);

            FightRecordManager.RecordLog<LogBuffExpression>(log =>
            {
                log.tag = (byte)TagType.BuffExpression;
                log.buffId = info.ID;
                log.value = hv;
            });
        }
        else
        {
            if (eff.param0 == 0) return true;

            if (effInfo.maxAmount0 == 0) return false;

            var conditionValue = effInfo.triggerParam?.triggerParam ?? 0;
            var effectValue = effInfo[0] + conditionValue * eff.conditionCoefficient;

            hv = eff.param1.Equals(1) ? effectValue : effectValue * owner.maxHealth;
            if (effInfo.maxAmount1 > 0 && Mathd.Abs(hv) > effInfo.maxAmount1)
                hv = hv < 0 ? -effInfo.maxAmount1 : effInfo.maxAmount1;

            if (effInfo.maxAmount0 > 0 && Mathd.Abs(hv) > effInfo.maxAmount0)
                hv = hv < 0 ? -effInfo.maxAmount0 : effInfo.maxAmount0;
        }
        
        var amount = HandleHealth(eff, hv);

        effInfo.amount0 += amount;

        if (effInfo.maxAmount0 > 0 && (effInfo.maxAmount0 -= Mathd.Abs(amount)) <= 0)
        {
            effInfo.maxAmount0 = 0;
            m_activeEffectCount = effInfo.keyEffect ? 0 : m_activeEffectCount - 1;
        }

        return true;
    }

    /// <summary>
    /// { "value", "isValue" }
    /// </summary>
    protected bool HandleEffectRage(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        if (eff.param0 == 0) return true;

        var rv = eff.param1 == 1 ? eff.param0 : eff.param0 * owner.maxRage;

        var old = owner.rage;
        owner.rage += rv;

        var amount = owner.rage - old;
        effInfo.amount0 += amount;

        return true;
    }

    /// <summary>
    /// { "state", "ignoreCooldown", "ignoreCatch" }
    /// </summary>
    protected bool HandleEffectForceState(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        if (eff.param0 == 0 || owner.isDead) return true; // Ignore if owner dead

        var sm = buffOwner.stateMachine;

        var sid = (int)eff.param0;
        var state = sm.GetState(sid);

        if (!state)
        {
            Logger.LogWarning("Buff::HandleEffectForceState: Apply Effect [{5}:{6}] failed, can not find state [{0}:{1}] from current creature [{2}:{3}, statemachine:{4}], ignore.", sid, eff.GetString(0), owner.configID, owner.uiName, owner.stateMachine.info.ID, info.ID, info.name);
            return true;
        }

        var opt = sm.GetLong(StateMachineParam.forcePassive) >> 48 & 0xFFFF;
        if (opt != 0) return true;

        if (eff.param2 == 0 && sm.GetBool(StateMachineParam.passiveCatchState)) return true;

        return sm.TranslateToID(sid, eff.param1 != 0);
    }

    /// <summary>
    /// { "stateAir", "stateGround" }
    /// </summary>
    protected bool HandleEffectFreez(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        owner.stateMachine.freezCount += reverse ? -1 : 1;

        if (reverse && m_interrupted == 0)
        {
            if (!owner.onGround && eff.param0 != 0)
            {
                owner.stateMachine.TranslateToID((int)eff.param0);
                return true;
            }

            if (owner.onGround && eff.param1 != 0)
            {
                owner.stateMachine.TranslateToID((int)eff.param1);
                return true;
            }
        }

        return true;
    }

    /// <summary>
    /// { }
    /// </summary>
    protected bool HandleEffectClearAttackBox(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        owner.clearAttackCount += reverse ? -1 : 1;
        return true;
    }

    /// <summary>
    /// { "stateEnter", "stateQuit", "morph" }
    /// </summary>
    protected bool HandleEffectSwitchMorph(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        if (!reverse || m_interrupted != 3)
        {
            var morph = (CreatureMorph) ((int) eff.param2);
            //觉醒时间到后需要更换模型
            if (reverse)
                owner.SwitchMorphModel();
            else if( morph == CreatureMorph.Awake && !(owner is MonsterCreature))
            {
                //觉醒需要使用角色的属性决定变身时间
                length = duration = owner.AwakeDuration;
            }
            owner.SwitchMorph(reverse ? CreatureMorph.Normal : morph);
        }

        var sid = (int)(!reverse ? eff.param0 : eff.param1);

        if (sid == 0) return true;

        var sm = buffOwner.stateMachine;

        var opt = sm.GetLong(StateMachineParam.forcePassive) >> 48 & 0xFFFF;
        if (opt != 0) return true;

        if (sm.GetBool(StateMachineParam.passiveCatchState)) return true;

        var state = sm.GetState(sid);
        if (!state)
        {
            Logger.LogWarning("Buff::HandleEffectSwitchMorph: Apply Effect [{5}:{6}] failed, can not find state [{0}:{1}] from current creature [{2}:{3}, statemachine:{4}], ignore.", sid, eff.GetString(0), owner.configID, owner.uiName, owner.stateMachine.info.ID, info.ID, info.name);
            return true;
        }

        return sm.TranslateToID(sid);
    }

    protected bool HandleEffectSwitchMorphModel(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        if (owner.isDead) return true; // Ignore if owner dead
        if (!reverse || m_interrupted != 3)
            owner.SwitchMorphModel(reverse ? CreatureMorph.Normal : (CreatureMorph)((int)eff.param0));
        return true;
    }

    /// <summary>
    /// { "propertyType", "value", "isValue", "targetType", "targetValue", "targetIsValue"}
    /// </summary>
    protected bool HandleEffectDamageTargetLimit(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        var type            = (CreatureFields)eff.param0;
        var value           = eff.param1;
        var isValue         = Math.Abs(eff.param2) > Mathd.Epsilon;
        var targetType      = (CreatureFields) eff.param3;
        var targetValue     = eff.param4;
        var targetIsValue   = Math.Abs(eff.param5) > Mathd.Epsilon;

        var v = owner.GetField(type)*(isValue ? (value > 0 ? 1 : -1) : value);
        v = Mathd.AbsMin(v, source.GetField(targetType)*(targetIsValue ? 1 : targetValue));

        var old = owner.health;

        owner.TakeUIDamage(source, (int)Mathd.Abs(v), eff.type, eff.flag == BuffInfo.EffectFlags.Unknow ? v < 0 ? BuffInfo.EffectFlags.Damage : BuffInfo.EffectFlags.Heal : eff.flag);

        var amount = owner.health - old;

        effInfo.amount0 += amount;

        if (effInfo.maxAmount0 > 0 && (effInfo.maxAmount0 -= Mathd.Abs(amount)) <= 0)
        {
            effInfo.maxAmount0 = 0;
            m_activeEffectCount = effInfo.keyEffect ? 0 : m_activeEffectCount - 1;
        }

        return true;
    }

    /// <summary>
    /// { "propertyType", "value", "isValue"}
    /// </summary>
    protected bool HandleEffectDamageForTargetProperty(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        var type = (CreatureFields)eff.param0;
        var value = eff.param1;
        var isValue = Math.Abs(eff.param2) > Mathd.Epsilon;

        var v = source.GetField(type) * (isValue ? value > 0 ? 1 : -1 : value);

        var old = owner.health;

        owner.TakeUIDamage(source, (int)Mathd.Abs(v), eff.type, eff.flag == BuffInfo.EffectFlags.Unknow ? v < 0 ? BuffInfo.EffectFlags.Damage : BuffInfo.EffectFlags.Heal : eff.flag);

        var amount = owner.health - old;

        effInfo.amount0 += amount;

        if (effInfo.maxAmount0 > 0 && (effInfo.maxAmount0 -= Mathd.Abs(amount)) <= 0)
        {
            effInfo.maxAmount0 = 0;
            m_activeEffectCount = effInfo.keyEffect ? 0 : m_activeEffectCount - 1;
        }

        return true;
    }

    /// <summary>
    /// { "alpha", "fade", "global" }
    /// </summary>
    protected bool HandleEffectDarkScreen(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        if (eff.param2 == 0 && !owner.isPlayer) return true;
        Camera_Combat.Dark(reverse ? -1 : 1, (float)eff.param0, eff.param1 != 0);

        return true;
    }

    /// <summary>
    /// { "attributeID", "value", "isPercent", "isRemove","timeMax" }
    /// </summary>
    protected bool HandleEffectSteal(BuffEffectInfo effInfo, BuffInfo.BuffEffect eff, bool reverse)
    {
        var attributeType = (CreatureFields) eff.param0;
        var value = eff.param1;
        bool isPercent = eff.param2 > 0;
        bool isRemove = eff.param3 > 0;
        var timeMax = (int)eff.param4;
        var stealEffect = effInfo as BuffEffectInfoSteal;

        if (reverse)
        {
            if (effInfo.amount0 != 0)
            {
                stealEffect?.StealReverse(attributeType);
            }
            return true;
        }
        
        var target = effInfo.userParam1;
        if (target == null)
            return false;

        if (stealEffect == null || !stealEffect.CanStealTarget(target, timeMax))
            return false;

        var effectValue = 0d;
        if (effInfo.effect.expression.IsValid)
            effectValue = effInfo.effect.expression.GetValue(dataHandler);
        else
            effectValue = isPercent ? target.GetField(attributeType) * value : value;

        stealEffect.Steal(attributeType, target, effectValue);

        if (isRemove)
        {
            stealEffect.AddListen(target);
        }

        return true;
    }

    private NValue Owner(NValue[] rParams)
    {
        if (owner == null)
            return NValue.Zero;

        if (rParams.Length == 1)
            return owner.GetField(rParams[0].To<int>());
        if (rParams.Length == 2)
            return owner.GetField(rParams[0].To<int>()) * rParams[1].To<double>();
        return NValue.Zero;
    }

    private NValue Trigger(NValue[] rParams)
    {
        var c = conditionTrigger ?? context.target;
        if (c == null)
            return NValue.Zero;

        if (rParams.Length == 1)
            return c.GetField(rParams[0].To<int>());
        if (rParams.Length == 2)
            return c.GetField(rParams[0].To<int>()) * rParams[1].To<double>();
        return NValue.Zero;
    }

    private NValue Source(NValue[] rParams)
    {
        if (source == null)
            return NValue.Zero;

        if (rParams.Length == 1)
            return source.GetField(rParams[0].To<int>());
        if (rParams.Length == 2)
            return source.GetField(rParams[0].To<int>()) * rParams[1].To<double>();
        return NValue.Zero;
    }

    private NValue Random(NValue[] rParams)
    {
        if (rParams == null || rParams.Length == 0)
            return default(NValue);
        var dom = Module_Battle.Range(0, rParams.Length);
        return rParams[dom];
    }

    #endregion
}


public class VariableScope : System.IDisposable
{
    private readonly DataHandler handler;
    private readonly BuffInfo.ExpressionVariable[] arr;
    public VariableScope(DataHandler rHandler, BuffInfo.ExpressionVariable[] rVariables)
    {
        handler = rHandler;
        arr = rVariables;
        Apply(true);
    }

    private void Apply(bool rApply)
    {
        if (arr == null || arr.Length == 0)
            return;
        for (var i = 0; i < arr.Length; i++)
        {
            if (rApply)
                handler.SetVariable(arr[i].name, arr[i].value);
            else
                handler.RemoveVariable(arr[i].name);
        }
    }

    public void Dispose()
    {
        Apply(false);
    }
}