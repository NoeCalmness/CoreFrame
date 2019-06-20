/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Define all game config items.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-15
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public class AutoAttribute : Attribute
{
    
}

[Serializable]
public class ProtocolLoggerFilter : ConfigItem
{
    public bool disabled;
    public bool noDetail;
    public bool cpp;
}//config ProtocolLoggerFilters : ProtocolLoggerFilter

[Serializable]
public class LoadingInfo : ConfigItem
{
    #region Static functions

    public static string SelectRandomInfo(int id = 0)
    {
        var info = id == 0 ? m_defaultInfo : ConfigManager.Get<LoadingInfo>(id);
        return info?.RandomInfo() ?? string.Empty;
    }

    private static LoadingInfo m_defaultInfo = new LoadingInfo() { ID = 0, infos = new string[] { "" } };

    #endregion

    public string[] infos;

    public override void Initialize()
    {
        if (infos == null || infos.Length < 1) infos = new string[] { "" };

        if (ID == 0) m_defaultInfo = this;
    }

    public string RandomInfo() { return infos[Random.Range(0, infos.Length)]; }
}//config LoadingInfos : LoadingInfo

[Serializable]
public class CommandInfo : ConfigItem
{
    public string command;
    public string comment;
    public string usage;

    public override void OnLoad() { if (!usage.StartsWith("Usage:")) usage = "Usage: " + usage; }
}//config CommandInfos : CommandInfo

[Serializable]
public class LevelInfo : ConfigItem
{
    public enum LevelType
    {
        Normal      = 0,
        PVE         = 1,
        PVP         = 2,
    }

    public LevelType type;
    public int       timeLimit;
    public int       infoID;
    public string    name;
    public string[]  maps;
    public string    script;
    public string    loadingWindow;
    public string[]  preloadAssets;

    public string[] validMaps { get; set; }

    public string SelectMap()
    {
        if (validMaps == null || validMaps.Length < 1) return string.Empty;
        return validMaps.Length == 1 ? validMaps[0] : validMaps[Random.Range(0, validMaps.Length)];
    }

    public override void OnLoad()
    {
        if (maps == null || maps.Length < 1 || maps.FindIndex(m => !string.IsNullOrEmpty(m) && !string.IsNullOrWhiteSpace(m)) < 0)
        {
            var vm = ID == 0 ? "level_login" : "level_home";
            Logger.LogWarning("Level config [{0}:{1}] dose not have a valid map assigned, use {2}.", ID, name, vm);
            validMaps = new string[] { vm };
        }
        else
        {
            var tmpList = new List<string>();
            foreach (var m in maps)
            {
                if (string.IsNullOrEmpty(m) || string.IsNullOrWhiteSpace(m)) continue;
                tmpList.Add(m);
            }

            validMaps = tmpList.ToArray();
            tmpList = null;
        }
        
        if (string.IsNullOrEmpty(script))
        {
            Logger.LogWarning("Level config [{0}:{1}] dose not have a script assigned, use default.", ID, name);
            script = "level";
        }
    }
}//config LevelInfos : LevelInfo

[Serializable]
public class ConfigText : ConfigItem
{
    #region Static functions
    private static readonly string[] m_emptey = new string[] { "emptey" };
    public static ConfigText emptey = new ConfigText() { ID = -1,  text = m_emptey };

    private static readonly string[] m_time0   = new string[] { "{0}天", "{0}小时", "{0}分", "{0}秒" };
    private static readonly string[] m_time1   = new string[] { "{0}年", "{0}月", "{0}日", "{0}", "{0}", "{0}" };
    private static readonly string[] m_number0 = new string[] { "{0}", "{0}万", "{0}亿" };
    /// <summary>
    /// 默认时间格式标签
    /// 0 = "{0}天", 1 = "{0}小时", 2 = "{0}分", 3 = "{0}秒"
    /// </summary>
    public static ConfigText time0 = new ConfigText() { ID = 50, text = m_time0 };
    /// <summary>
    /// 默认时间格式标签
    /// 0 = "{0}年", 1 = "{0}月", 2 = "{0}日", 3 = "{0}时", 4 = "{0}分", 5 = "{0}秒"
    /// </summary>
    public static ConfigText time1 = new ConfigText() { ID = 51, text = m_time1 };
    /// <summary>
    /// 默认数字格式标签
    /// 0 = "{0}万", 1 = "{0}亿"
    /// </summary>
    public static ConfigText number0 = new ConfigText() { ID = 52, text = m_number0 };

    #endregion

    public string[] text;

    public override void OnLoad()
    {
        if (ID == -1) { emptey = this; if (text == null) text = m_emptey; }

        if (ID == 50) { time0 = this; if (text == null) text = m_time0; }
        if (ID == 51) { time1 = this; if (text == null) text = m_time1; }
        if (ID == 52) { number0 = this; if (text == null) text = m_number0; }

        if (text == null) text = new string[] { };

        for (var i = 0; i < text.Length; ++i)
            text[i] = Util.ParseStringTags(text[i]);
    }

    public string this[int index]
    {
        get { return index < 0 || index >= text.Length ? string.Empty : text[index]; }
    }

    public static string GetDefalutString(int id,int index = 0)
    {
        var t = ConfigManager.Get<ConfigText>(id);

        if (t == null) return string.Empty;

        if (index >= t.text.Length)
        {
            Logger.LogWarning("[ConfigText.Id = {0} len = {1}] error : index out of range, index = {2} ", id,t.text.Length,index);
            index = t.text.Length - 1;
        }

        return t[index];
    }

    public static string GetDefalutString(TextForMatType type, int index = 0)
    {
        return GetDefalutString((int)type,index);
    }
}//config ConfigTexts : ConfigText

[Serializable]
public class SensitiveKeywordInfo : ConfigItem
{
    public static SensitiveKeywordInfo defaultConfig = new SensitiveKeywordInfo { approximate = true, ID = 0, keywords = new string[] { } };

    public bool approximate;

    public string[] keywords;

    public override void Initialize()
    {
        if (keywords == null) keywords = new string[] { };

        if (ID == 0) defaultConfig = this;
    }
}//config SensitiveKeywordInfos : SensitiveKeywordInfo

#region Creatures

[Serializable]
public class CreatureInfo : ConfigItem
{
    #region Static functions

    public static string GetMorphModelName(string[] models, CreatureMorph morph)
    {
        return GetMorphModelName(models, (int)morph);
    }

    public static string GetMorphModelName(string[] models, int morph)
    {
        return models == null || models.Length < 1 || morph < 0 || morph >= models.Length ? string.Empty : models[morph];
    }

    #endregion

    public string[]    models;                 // 模型名称 每一个索引对应了一个形态 参考 CreatureMorphs
    public string      icon;                   // 图标名称
    public string      name;                   // 名称
    public int         gender;                 // 角色性别 1 = 男性 0 = 女性
    public string      avatar;                 // 角色头像
    public string      info;                   // 角色描述
    public Vector2_    colliderSize;           // 碰撞盒子大小（半径,高度）
    public Vector2_    hitColliderSize;        // 受击盒子大小（半径,高度）
    public double      colliderOffset;         // 碰撞盒子 Y 偏移
    public double      hitColliderOffset;      // 受击盒子 Y 偏移
    public float       scale;                  // 模型大小（缩放比）
    public int         level;                  // 等级
    public int         baseExp;                // 初始经验
    public int         type;                   // 生物类型

    public int         health;                 // 初始生命值
    public int         maxHealth;              // 最大生命值
    public double      rage;                   // 初始怒气
    public double      maxRage;                // 最大怒气
    public int         energy;                 // 初始能量
    public int         maxEnergy;              // 最大能量
    public int         awakeDuration;          // 变身持续时间
    public int         attack;                 // 攻击力
    public int         offAttack;              // 副武器攻击
    public int         defense;                // 防御力
    public int         elementType;            // 元素类型
    public int         elementAttack;          // 元素攻击力
    public int         elementDefenseWind;     // 元素防御力
    public int         elementDefenseFire;     // 元素防御力
    public int         elementDefenseWater;    // 元素防御力
    public int         elementDefenseThunder;  // 元素防御力
    public int         elementDefenseIce;      // 元素防御力

    public int         strength;               // 力量
    public int         artifice;               // 技巧
    public int         vitality;               // 体力

    public double      crit;                   // 暴击概率
    public double      critMul;                // 暴击加成
    public int         brutality;              // 残暴
    public int         firm;                   // 铁骨
    public double      resilience;             // 韧性

    public double      attackSpeed;            // 攻击速度
    public double      moveSpeed;              // 移动速度
    public double      chargeSpeed;            // 蓄力速度
    public double      leech;                  // 吸血

    public int         regenHealth;            // 每秒回复生命
    public double      regenRage;              // 每秒回复怒气

    public int         weaponID;               // 主武器 ID
    public int         weaponItemID;           // 主武器道具 ID （每一个武器 ID 对应了多个武器道具）
    public int         offWeaponID;            // 副武器 ID
    public int         offWeaponItemID;        // 副武器道具 ID （每一个武器 ID 对应了多个武器道具）

    public int         bulletCount;            // 单场战斗初始子弹数量

    public int[]       buffs;
    public PRoleAttrItem[] awakeChangeAttr;

    public PSkill[]    skills { get; set; }
}//config CreatureInfos : CreatureInfo

#endregion

#region Battle System

[Serializable]
public class InputKeyInfo : ConfigItem
{
    public InputKeyType   type;
    public TouchID        touchID;
    public int            touchIndex;
    public int            delay;
    public int            value;
    public int            bindDirection;
    public string         mutexKeys;
    public string         virtualName;
    public string         name;
    public string         desc;

    private static Dictionary<int, InputKeyInfo> m_cachedKeys = new Dictionary<int, InputKeyInfo>();

    public static InputKeyInfo GetCachedKeyInfo(int id) { return m_cachedKeys.Get(id); }

    public override void InitializeOnce()
    {
        m_cachedKeys.Clear();
        var keys = ConfigManager.GetAll<InputKeyInfo>();
        foreach (var key in keys) m_cachedKeys.Set(key.ID, key);
    }
}//config InputKeyInfos : InputKeyInfo

[Serializable]
public class AnimMotionInfo : ConfigItem
{
    public string name;
    public Vector3_ offset;
    public Vector3_[] points;
}//config AnimMotionInfos : AnimMotionInfo

[Serializable]
public class AnimClipInfo : ConfigItem
{
    public string name;
    public int frameCount;
    public bool loop;

    public double length { get { return frameCount * 0.033; } }
}//config AnimClipInfos : AnimClipInfo

[Serializable]
public class CreatureStateInfo : ConfigItem
{
    [Serializable]
    public struct StateInfo
    {
        public static readonly StateInfo empty = new StateInfo() { ID = -1, name = null, hash = -1 };
        public static readonly StateInfo any   = new StateInfo() { ID = 0, name = "any", hash = Animator.StringToHash("any") };

        public int    ID;
        public string name;

        public bool isEmpty { get { return ID < 1; } }

        public int hash { get; set; }
        public int groupMask { get; set; }
    }

    #region  Static functions

    private static Dictionary<string, int>     m_nameToID    = new Dictionary<string, int>();
    private static Dictionary<string, int>     m_nameToHash  = new Dictionary<string, int>();
    private static Dictionary<int, string>     m_IDToName    = new Dictionary<int, string>();
    private static Dictionary<int, int>        m_IDToHash    = new Dictionary<int, int>();
    private static Dictionary<int, StateInfo>  m_IDToState   = new Dictionary<int, StateInfo>();

    public static int NameToID(string name) { return name == null ? -1 : m_nameToID.Get(name, -1); }
    public static int NameToHash(string name) { return name == null ? -1 : m_nameToHash.Get(name, -1); }
    public static string IDToName(int id) { return m_IDToName.Get(id, string.Empty); }
    public static int IDToHash(int id) { return m_IDToHash.Get(id, -1); }
    public static StateInfo IDToState(int id) { StateInfo s; if (m_IDToState.TryGetValue(id, out s)) return s; return StateInfo.empty; }

    #endregion

    public StateInfo[] states;

    public StateInfo GetStateInfo(string name, int ID)
    {
        var idx = string.IsNullOrEmpty(name) ? states.FindIndex(s => s.ID == ID) : states.FindIndex(s => string.Equals(s.name, name));
        return idx < 0 ? StateInfo.empty : states[idx];
    }

    public bool HasState(string name, int state)
    {
        var idx = string.IsNullOrEmpty(name) ? states.FindIndex(s => s.ID == ID) : states.FindIndex(s => string.Equals(s.name, name));
        return idx > -1;
    }

    public override void InitializeOnce()
    {
        m_nameToID.Clear();
        m_nameToHash.Clear();
        m_IDToName.Clear();
        m_IDToHash.Clear();
        m_IDToState.Clear();

        var any = StateInfo.any;

        m_nameToID.Set(any.name, any.ID);
        m_nameToHash.Set(any.name, any.hash);
        m_IDToName.Set(any.ID, any.name);
        m_IDToHash.Set(any.ID, any.hash);
        m_IDToState.Set(any.ID, any);

        var ss = ConfigManager.GetAll<CreatureStateInfo>();
        foreach (var s in ss)
        {
            for (var i = 0; i < s.states.Length; ++i)
            {
                var state = s.states[i];
                s.states[i].hash = Animator.StringToHash(state.name);

                #if UNITY_EDITOR || DEVELOPMENT_BUILD

                if (m_nameToID.ContainsKey(state.name) && m_nameToID[state.name] != state.ID)
                    Logger.LogError("CreatureStateInfo::InitializeOnce: Invalid CreatureState ID, use new instead. Group:<b>[{3}]</b>, State:<b>[{0}]</b>, old id:<b>[{1}]</b>, new id:<b>[{2}]</b>", state.name, m_nameToID[state.name], state.ID, s.ID);

                if (m_IDToName.ContainsKey(state.ID) && m_IDToName[state.ID] != state.name)
                    Logger.LogError("CreatureStateInfo::InitializeOnce: Invalid CreatureState name, use new instead. Group:<b>[{3}]</b>, ID:<b>[{0}]</b>, old name:<b>[{1}]</b>, new name:<b>[{2}]</b>", state.ID, m_IDToName[state.ID], state.name, s.ID);
                
                #endif

                m_nameToID.Set(state.name, state.ID);
                m_nameToHash.Set(state.name, state.hash);
                m_IDToName.Set(state.ID, state.name);
                m_IDToHash.Set(state.ID, state.hash);

                var mask = 1 << s.ID;
                StateInfo prev;
                if (m_IDToState.TryGetValue(state.ID, out prev)) mask |= prev.groupMask;
                s.states[i].groupMask = mask;

                m_IDToState.Set(state.ID, s.states[i]);
            }
        }
    }
}//config CreatureStateInfos : CreatureStateInfo

[Serializable]
public class TransitionInfo : ConfigItem
{
    [Serializable]
    public class Condition
    {
        #region Static functions
        
        public static int GetConditionKeyMask(Condition kc)
        {
            if (!kc.isKey || kc.mode == ConditionMode.None) return 0;

            switch (kc.mode)
            {
                case ConditionMode.Greater:  return ~((1 << kc.intValue + 1) - 1);
                case ConditionMode.Less:     return (1 << kc.intValue) - 1;
                case ConditionMode.LEquals:  return (1 << kc.intValue + 1) - 1;
                case ConditionMode.GEquals:  return ~((1 << kc.intValue) - 1);
                case ConditionMode.Contains:
                case ConditionMode.Except:
                case ConditionMode.NotEqual:
                case ConditionMode.Equals:   return 1.BitMasks(true, kc.intValues);
                default: return 0;
            }
        }

        public static long GetConditionGroupMask(Condition gc)
        {
            if (!gc.isKey || gc.mode == ConditionMode.None) return 0;

            switch (gc.mode)
            {
                case ConditionMode.Contains:  return gc.intValue.ToLongMask();
                case ConditionMode.Except:    return ~gc.intValue.ToLongMask();
                default: return 0;
            }
        }

        public static ConditionMode ParseMode(string m)
        {
            m = m.ToLower();

            if (m == "equals")   return ConditionMode.Equals;
            if (m == "nequals")  return ConditionMode.NotEqual;
            if (m == "greater")  return ConditionMode.Greater;
            if (m == "less")     return ConditionMode.Less;
            if (m == "is")       return ConditionMode.If;
            if (m == "not")      return ConditionMode.IfNot;
            if (m == "contains") return ConditionMode.Contains;
            if (m == "except")   return ConditionMode.Except;
            if (m == "between")  return ConditionMode.Between;
            if (m == "nbetween") return ConditionMode.NotBetween;
            if (m == "gequals")  return ConditionMode.GEquals;
            if (m == "lequals")  return ConditionMode.LEquals;

            return ConditionMode.None;
        }

        #endregion

        public ConditionMode mode;
        public string param;
        public string threshold;

        public bool isKey { get; private set; }
        public bool isMask { get; private set; }
        public bool isRage { get; private set; }
        public bool isRageRate { get; private set; }
        public bool isEnergy { get; private set; }
        public bool isEnergyRate { get; private set; }
        public bool isMainItem { get; private set; }
        public bool isOffItem { get; private set; }
        public bool isGroup { get; private set; }

        public StateMachineParamTypes paramType { get; private set; }

        public double doubleValue => m_doubleValue;
        public long longValue => m_longValue;
        public int intValue => (int)(m_longValue & 0xFFFFFFFF);
        public bool boolValue => m_boolValue;
        public long[] longValues => m_longValues;
        public double[] doubleValues => m_doubleValues;
        public int[] intValues => m_intValues;

        public string GetModeString(bool xml = false)
        {
            var ms = "none";

            if      (mode == ConditionMode.Equals)     ms = "equals";
            else if (mode == ConditionMode.NotEqual)   ms = "nequals";
            else if (mode == ConditionMode.Greater)    ms = "greater";
            else if (mode == ConditionMode.Less)       ms = "less";
            else if (mode == ConditionMode.If)         ms = "is";
            else if (mode == ConditionMode.IfNot)      ms = "not";
            else if (mode == ConditionMode.Contains)   ms = "contains";
            else if (mode == ConditionMode.Except)     ms = "except";
            else if (mode == ConditionMode.Between)    ms = "between";
            else if (mode == ConditionMode.NotBetween) ms = "nbetween";
            else if (mode == ConditionMode.GEquals)    ms = "gequals";
            else if (mode == ConditionMode.LEquals)    ms = "lequals";

            return xml ? ms == "none" || ms == "equals" ? "" : "mode=\"" + ms + "\" " : ms;
        }

        #region Check helpers

        public bool CheckGreater(double dv, long lv)
        {
            return paramType == StateMachineParamTypes.Double ? dv > m_doubleValue : lv > m_longValue;
        }

        public bool CheckLess(double dv, long lv)
        {
            return paramType == StateMachineParamTypes.Double ? dv < m_doubleValue : lv < m_longValue;
        }

        public bool CheckGEquals(double dv, long lv)
        {
            return paramType == StateMachineParamTypes.Double ? dv >= m_doubleValue : lv >= m_longValue;
        }

        public bool CheckLEquals(double dv, long lv)
        {
            return paramType == StateMachineParamTypes.Double ? dv <= m_doubleValue : lv <= m_longValue;
        }

        public bool CheckEquals(double dv, long lv)
        {
            if (m_doubleValues.Length < 2) return paramType == StateMachineParamTypes.Double ? dv == m_doubleValue : lv == m_longValue;
            if (paramType == StateMachineParamTypes.Double)
            {
                foreach (var v in m_doubleValues) if (v == dv) return true;
                return false;
            }

            foreach (var v in m_longValues) if (v == lv) return true;
            return false;
        }

        public bool CheckNotEquals(double dv, long lv)
        {
            if (m_doubleValues.Length < 2) return paramType == StateMachineParamTypes.Double ? dv != m_doubleValue : lv != m_longValue;
            if (paramType == StateMachineParamTypes.Double)
            {
                foreach (var v in m_doubleValues) if (v == dv) return false;
                return true;
            }

            foreach (var v in m_longValues) if (v == lv) return false;
            return true;
        }

        public bool CheckContains(long lv)
        {
            if (m_longValues.Length < 2) return lv.BitMask((int)m_longValue);

            foreach (var v in m_longValues) if (lv.BitMask((int)v)) return true;
            return false;
        }

        public bool CheckExcept(long lv)
        {
            if (m_longValues.Length < 2) return !lv.BitMask((int)m_longValue);

            foreach (var v in m_longValues) if (lv.BitMask((int)v)) return false;
            return true;
        }

        public bool CheckContainsKey(long lv)
        {
            foreach (var v in m_longValues)
            {
                for (var i = 0; i < 4; ++i)
                {
                    var vv = lv >> i * 7 & 0x1F;
                    if (vv == v) return true;
                }
            }
            return false;
        }

        public bool CheckExceptKey(long lv)
        {
            foreach (var v in m_longValues)
            {
                for (var i = 0; i < 4; ++i)
                {
                    var vv = lv >> i * 7 & 0x1F;
                    if (vv == v) return false;
                }
            }
            return true;
        }

        public bool CheckBetween(double dv, long lv)
        {
            if (m_doubleValues.Length < 2) return false;
            if (paramType == StateMachineParamTypes.Double)
            {
                for (int i = 0, c = m_doubleValues.Length; i < c; i +=2) if (dv >= m_doubleValues[i] && dv <= m_doubleValues[i + 1]) return true;
                return false;
            }

            for (int i = 0, c = m_longValues.Length; i < c; i += 2) if (lv >= m_longValues[i] && lv <= m_longValues[i + 1]) return true;
            return false;
        }

        public bool CheckNotBetween(double dv, long lv)
        {
            if (m_doubleValues.Length < 2) return false;
            if (paramType == StateMachineParamTypes.Double)
            {
                for (int i = 0, c = m_doubleValues.Length; i < c; i +=2) if (dv >= m_doubleValues[i] && dv <= m_doubleValues[i + 1]) return false;
                return true;
            }

            for (int i = 0, c = m_longValues.Length; i < c; i += 2) if (lv >= m_longValues[i] && lv <= m_longValues[i + 1]) return false;
            return true;
        }

        #endregion

        private long   m_longValue;
        private double m_doubleValue;
        private bool   m_boolValue;

        private long[] m_longValues;
        private int[] m_intValues;
        private double[] m_doubleValues;

        public void Initialize(int group, int transition)
        {
            m_longValue   = 0;
            m_doubleValue = 0;
            m_boolValue   = false;

            m_longValues = new long[] { m_longValue };
            m_doubleValues = new double[] { m_doubleValue };
            m_intValues = new int[] { intValue };

            if (mode == ConditionMode.If || mode == ConditionMode.IfNot)
            {
                paramType         = StateMachineParamTypes.Bool;
                m_boolValue       = Util.Parse<bool>(threshold);
                m_longValue       = m_boolValue ? 1 : 0;
                m_doubleValue     = m_longValue;
                m_longValues[0]   = m_longValue;
                m_doubleValues[0] = m_doubleValue;
                m_intValues[0]    = intValue;
            }
            else if (mode == ConditionMode.Between || mode == ConditionMode.NotBetween)
            {
                var pairs = Util.ParseString<string>(threshold);
                var values = new List<string>(pairs.Length * 2);
                var isDouble = false;

                foreach (var pair in pairs)
                {
                    var pv = Util.ParseString<string>(pair, false, ',');
                    if (pv.Length != 2)
                    {
                        Logger.LogWarning("Condition::Initialize: Invalid pair length in <b>[group:{0}, transition:{1}]</b>, ignored.", group, transition);
                        continue;
                    }
                    values.AddRange(pv);

                    if (!isDouble) isDouble = pv[0].IndexOf('.') > -1;
                }

                paramType = isDouble ? StateMachineParamTypes.Double : StateMachineParamTypes.Long;

                var dv = new List<double>(values.Count);
                var lv = new List<long>(values.Count);
                for (var i = 0; i < values.Count; i += 2)
                {
                    var fd = Util.Parse<double>(values[i]);
                    var sd = Util.Parse<double>(values[i + 1]);

                    if (fd < sd) { dv.Add(fd); dv.Add(sd); }
                    else { dv.Add(sd); dv.Add(fd); }

                    var fl = Util.Parse<long>(values[i]);
                    var sl = Util.Parse<long>(values[i + 1]);

                    if (fl < sl) { lv.Add(fl); lv.Add(sl); }
                    else { lv.Add(sl); lv.Add(fl); }
                }
                m_doubleValues = dv.ToArray();
                m_longValues = lv.ToArray();

                m_intValues = new int[m_longValues.Length];
                for (var i = 0; i < m_longValues.Length; ++i) m_intValues[i] = (int)(m_longValues[i] & 0xFFFFFFFF);

                m_doubleValue  = m_doubleValues.Length < 1 ? 0 : m_doubleValues[0];
                m_longValue    = m_longValues.Length < 1 ? 0 : m_longValues[0];
                m_boolValue    = m_longValue != 0;

                values = null;
                dv = null;
                lv = null;
            }
            else
            {
                var values = Util.ParseString<string>(threshold);
                var isDouble = false;
                m_doubleValues = new double[values.Length];
                m_longValues = new long[values.Length];
                m_intValues = new int[values.Length];
                for (var i = 0; i < values.Length; ++i)
                {
                    var val = values[i];
                    m_doubleValues[i] = Util.Parse<double>(val);
                    m_longValues[i] = Util.Parse<long>(val);
                    m_intValues[i] = (int)(m_longValues[i] & 0xFFFFFFFF);

                    if (!isDouble) isDouble = val.IndexOf('.') > -1;
                }

                paramType      = isDouble ? StateMachineParamTypes.Double : StateMachineParamTypes.Long;
                m_doubleValue  = m_doubleValues.Length < 1 ? 0 : m_doubleValues[0];
                m_longValue    = m_longValues.Length < 1 ? 0 : m_longValues[0];
                m_boolValue    = m_longValue != 0;

                values = null;
            }

            isKey        = param == "key";
            isRage       = param == "rage";
            isRageRate   = param == "rageRate";
            isEnergy     = param == "energy";
            isEnergyRate = param == "energyRate";
            isGroup      = param == "groupMask";
            isMainItem   = param == "weaponItemID";
            isOffItem    = param == "offWeaponItemID";
            isMask       = mode == ConditionMode.Contains || mode == ConditionMode.Except;

            if (isKey && mode != ConditionMode.None)
            {
                if (m_longValue < 0 || m_longValue > 0xF)
                {
                    mode = ConditionMode.None;

                    Logger.LogWarning("Condition::Initialize: Invalid key condition config in <b>[group:{0}, transition:{1}]</b>, value can not be negative or greater than 0xF(15), ignored.", group, transition);

                    return;
                }

                if (mode != ConditionMode.Equals && mode != ConditionMode.NotEqual && mode != ConditionMode.Less && mode != ConditionMode.Greater && mode != ConditionMode.Contains && mode != ConditionMode.Except)
                {
                    mode = ConditionMode.None;

                    Logger.LogWarning("Condition::Initialize: Invalid key condition config in <b>[group:{0}, transition:{1}]</b>, mode can only be less, greater, equals, nequals, contains or except, ignored.", group, transition);

                    return;
                }
            }

            if (isGroup && mode != ConditionMode.None)
            {
                if (m_longValue < 0 || m_longValue > 0x3F)
                {
                    mode = ConditionMode.None;

                    Logger.LogWarning("Condition::Initialize: Invalid group condition config in <b>[group:{0}, transition:{1}]</b>, value can not be negative or greater than 0x3F(63), ignored.", group, transition);

                    return;
                }

                if (mode != ConditionMode.Contains && mode != ConditionMode.Except)
                {
                    mode = ConditionMode.None;

                    Logger.LogWarning("Condition::Initialize: Invalid group condition config in <b>[group:{0}, transition:{1}]</b>, mode can only be contains or except, ignored.", group, transition);

                    return;
                }
            }
        }
    }

    [Serializable]
    public class Transition
    {
        public bool valid { get; private set; } = true;

        public int         ID;
        public string      name;
        public int         priority;
        public string      from;
        public string      to;
        public Condition[] conditions;
        public bool        hasExitTime;
        public bool        noSelfTransition;
        public bool        noExitCondition;
        public bool        noDeadCondition;
        public bool        noFreezCondition;
        public bool        preventInputReset;         // Prevent input reset if transition passed
        public bool        preventInputTurn;          // Prevent character turn by input direction bind
        public bool        forceKeyCondition;         // Force key condition check even in ignoreFrame state
        public int         forceUIResult;             // Force check result when check failed (0 - 255)
        public float       blendTime;

        public int[] requireMainItems { get; private set; }
        public int[] requireOffItems { get; private set; }
        public int acceptKeyMask { get; private set; }
        public long acceptGroupMask { get; private set; }
        public int itemConditionEnd { get; private set; }
        public int keyConditionEnd { get; private set; }

        public Condition GetCondition(string param)
        {
            for (var i = 0; i < conditions.Length; ++i)
                if (conditions[i].param == param) return conditions[i];
            return null;
        }

        public void Initialize(int group)
        {
            if (!string.IsNullOrEmpty(from) && CreatureStateInfo.NameToID(from) < 0)
            {
                if (Application.isPlaying) Logger.LogError("Transition::Initialize: Transition [{0}:{1}] has invalid FROM state [{2}], ignored.", group, ID, from);
                valid = false;
                return;
            }

            if (string.IsNullOrEmpty(to) || CreatureStateInfo.NameToID(to) < 0)
            {
                if (Application.isPlaying) Logger.LogError("Transition::Initialize: Transition [{0}:{1}] has invalid TO state [{2}], ignored.", group, ID, to);
                valid = false;
                return;
            }

            valid = true;

            acceptKeyMask   = (1 << 16) - 2;
            acceptGroupMask = -2;

            var reqMainItems = new List<long>();
            var reqOffItems = new List<long>();

            var hasKey   = false;
            var hasGroup = false;
            foreach (var c in conditions)
            {
                c.Initialize(group, ID);

                if (c.isMainItem) reqMainItems.AddRange(c.longValues);
                if (c.isOffItem)  reqOffItems.AddRange(c.longValues);

                if (c.isKey && c.mode != ConditionMode.None)
                {
                    hasKey = true;
                    acceptKeyMask &= Condition.GetConditionKeyMask(c);
                }

                if (c.isGroup && c.mode != ConditionMode.None)
                {
                    hasGroup = true;
                    acceptGroupMask &= Condition.GetConditionGroupMask(c);
                }
            }

            requireMainItems = new int[reqMainItems.Count];
            for (int i = 0, c = reqMainItems.Count; i < c; ++i) requireMainItems[i] = (int)reqMainItems[i];
            requireOffItems  = new int[reqOffItems.Count];
            for (int i = 0, c = reqOffItems.Count; i < c; ++i) requireOffItems[i] = (int)reqOffItems[i];

            if (!hasKey) acceptKeyMask = 0;
            if (!hasGroup) acceptGroupMask = 0;

            Array.Sort(conditions, (a, b) =>
            {
                var l = a.isGroup ? -1 : a.isMainItem || a.isOffItem ? 0 : a.isKey ? 1 : a.isRage || a.isRageRate || a.isEnergy || a.isEnergyRate ? 3 : 2;
                var r = b.isGroup ? -1 : b.isMainItem || b.isOffItem ? 0 : b.isKey ? 1 : b.isRage || a.isRageRate || a.isEnergy || a.isEnergyRate ? 3 : 2;
                return l < r ? -1 : 1;
            });

            itemConditionEnd = conditions.FindIndex(c => !c.isGroup && !c.isMainItem && !c.isOffItem);
            if (itemConditionEnd < 0) itemConditionEnd = 0;

            keyConditionEnd = conditions.FindIndex(itemConditionEnd, c => !c.isKey);
            if (keyConditionEnd < 0) keyConditionEnd = itemConditionEnd;
        }
    }

    public Transition[] transitions;

    private Dictionary<int, List<Transition>> m_groupedTransitions = new Dictionary<int, List<Transition>>();

    public List<Transition> GetTransitions(int stateID)
    {
        return m_groupedTransitions.Get(stateID);
    }

    public override void Initialize()
    {
        if (m_groupedTransitions == null) m_groupedTransitions = new Dictionary<int, List<Transition>>();

        var sortedTransitions = transitions.SimpleClone();
        Array.Sort(sortedTransitions, (l, r) => r.priority < l.priority ? -1 : 1);

        m_groupedTransitions.Clear();
        var pre = m_groupedTransitions.GetDefault(-1); // Pre state transitions
        var any = m_groupedTransitions.GetDefault(0);  // Any state transitions

        foreach (var trans in sortedTransitions)
        {
            trans.Initialize(ID);

            if (!trans.valid) continue;

            var list = trans.priority <= -9999 ? pre : string.IsNullOrEmpty(trans.from) ? any : m_groupedTransitions.GetDefault(CreatureStateInfo.NameToID(trans.from));
            list.Add(trans);
        }
    }
}//config TransitionInfos : TransitionInfo

[Serializable]
public class StateMachineInfo : ConfigItem
{
    #region Static functions

    /// <summary>
    /// An empty state
    /// </summary>
    public static readonly StateDetail emptyState = new StateDetail()
    {
        ID = -1, state = "empty", animation = "",
        sections = new Section[] { }, invisibles = new FrameData[] { }, weaks = new FrameData[] { }, buffs = new FrameData[] { }, effects = new Effect[] { }, flyingEffects = new FlyingEffect[] { }, hidePets = new FrameData[] { },
        soundEffects = new SoundEffect[] { }, rimLights = new FrameData[] { }, cameraShakes = new FrameData[] { }, weaponBinds = new FrameData[] { }, hideWeapons = new FrameData[] { }, hideOffWeapons = new FrameData[] { }, darkScreens = new FrameData[] { },
        ignoreCatchPosGroups = new int[] { }
    };

    /// <summary>
    /// Default laydown state
    /// </summary>
    public static readonly StateDetail layDownState = new StateDetail()
    {
        ID = StateMachineState.STATE_LAYDOWN, state = "StateLaydown", animation = "",
        sections = new Section[] { }, invisibles = new FrameData[] { }, weaks = new FrameData[] { }, buffs = new FrameData[] { }, effects = new Effect[] { }, flyingEffects = new FlyingEffect[] { }, hidePets = new FrameData[] { },
        soundEffects = new SoundEffect[] { }, rimLights = new FrameData[] { }, cameraShakes = new FrameData[] { }, weaponBinds = new FrameData[] { }, hideWeapons = new FrameData[] { }, hideOffWeapons = new FrameData[] { }, darkScreens = new FrameData[] { },
        ignoreCatchPosGroups = new int[] { }
    };

    /// <summary>
    /// An empty statemachine
    /// </summary>
    public static readonly StateMachineInfo empty = new StateMachineInfo()
    {
        ID = 0, name = "", transitionGroup = 0, deadEffect = Effect.empty, hitEffects = new Effect[] { Effect.empty },
        states = new StateDetail[] { emptyState },
        defaultWeaponBinds = new FrameData[] { }, offWeaponBinds = new FrameData[] { }, passiveWeaponBinds = new FrameData[] { }
    };

    /// <summary>
    /// Create a default laydown state
    /// </summary>
    public static StateDetail GetDefaultLaydownState(long type, int parent)
    {
        var uid = type << 32 | (long)parent << 32;
        var s = m_cachedLaydowns.Get(uid);
        if (s == null)
        {
            s = new StateDetail()
            {
                ID = StateMachineState.STATE_LAYDOWN, state = "StateLaydown", animation = "",
                sections = new Section[] { }, invisibles = new FrameData[] { }, weaks = new FrameData[] { }, buffs = new FrameData[] { }, effects = new Effect[] { }, flyingEffects = new FlyingEffect[] { }, hidePets = new FrameData[] { },
                soundEffects = new SoundEffect[] { }, rimLights = new FrameData[] { }, cameraShakes = new FrameData[] { }, weaponBinds = new FrameData[] { }, hideWeapons = new FrameData[] { }, hideOffWeapons = new FrameData[] { }, darkScreens = new FrameData[] { },
                ignoreCatchPosGroups = new int[] { },
                sceneActors = new SceneActor[] {}
            };
            s.Initialize(parent);
            m_cachedLaydowns.Set(uid, s);
        }
        return s;
    }

    private static Dictionary<long, StateDetail> m_cachedLaydowns = new Dictionary<long, StateDetail>();
    private static Dictionary<int, List<int>> m_itemCheck = new Dictionary<int, List<int>>(), m_offItemCheck = new Dictionary<int, List<int>>();

    /// <summary>
    /// Collect all state assets
    /// </summary>
    /// <param name="main">Main weapon ID</param>
    /// <param name="off">Off weapon ID</param>
    /// <param name="list">Asset list</param>
    /// <param name="gender"></param>
    /// <param name="itemID"></param>
    /// <param name="offItemID"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    public static List<string> GetAllAssets(int main, int off, List<string> list = null, int proto = 0, int gender = 1, int itemID = -1, int offItemID = -1, bool player = false, bool simple = false)
    {
        if (list == null) list = new List<string>();

        StateMachineInfo m = simple ? ConfigManager.Get<StateMachineInfoSimple>(main) : ConfigManager.Get<StateMachineInfo>(main), p = m && !simple ? ConfigManager.Get<StateMachineInfoPassive>(m.passiveGroup) : null, o = off < 0 ? null : ConfigManager.Get<StateMachineInfoOff>(1/*off*/);

        if (!m) return list;

        m_itemCheck.Clear();
        m_offItemCheck.Clear();

        var checkItem = itemID > -1 || offItemID > -1;
        
        if (checkItem)
        {
            CollectItemRequirements(m, m_itemCheck, m_offItemCheck);
            CollectItemRequirements(p, m_itemCheck, m_offItemCheck);
            CollectItemRequirements(o, m_itemCheck, m_offItemCheck);
        }

        var sk = player ? Module_Skill.instance : null;
        GetAllAssets(list, m, proto, gender, itemID, offItemID, sk, checkItem);
        GetAllAssets(list, p, proto, gender, itemID, offItemID, sk, checkItem);
        GetAllAssets(list, o, proto, gender, itemID, offItemID, sk, checkItem);

        m_itemCheck.Clear();
        m_offItemCheck.Clear();

        return list;
    }

    private static void CollectItemRequirements(StateMachineInfo m, Dictionary<int, List<int>> itemCheck, Dictionary<int, List<int>> offItemCheck)
    {
        if (!m) return;

        var ti = ConfigManager.Get<TransitionInfo>(m.transitionGroup);
        if (!ti || ti.transitions == null) return;

        var trans = ti.transitions;
        foreach (var tran in trans)
        {
            var sid = CreatureStateInfo.NameToID(tran.to);
            var items = itemCheck.GetDefault(sid);
            var offItems = offItemCheck.GetDefault(sid);

            var ri = tran.requireMainItems.Length > 0;
            var ro = tran.requireOffItems.Length > 0;

            if (ri) items.AddRange(tran.requireMainItems);
            else items.Add(0);
            if (ro) offItems.AddRange(tran.requireOffItems);
            else offItems.Add(0);
        }
    }

    private static void GetAllAssets(List<string> list, StateMachineInfo m, int proto, int gender, int itemID, int offItemID, Module_Skill sk, bool checkItem)
    {
        if (!m || m.states == null) return;

        var states = m.states;
        foreach (var s in states)
        {
            if (!s.valid) continue;

            var state = s;
            if (sk)
            {
                var level = sk.GetStateLevel(state.ID);
                state = StateOverrideInfo.GetOverrideState(m, s, level);
            }

            if (!s.valid) continue;

            if (checkItem)
            {
                List<int> items = m_itemCheck.Get(state.ID), offItems = m_offItemCheck.Get(state.ID);
                if (items != null && !items.Contains(0) && !items.Contains(itemID)) continue;
                if (offItems != null && !offItems.Contains(0) && !offItems.Contains(offItemID)) continue;
            }

            var effs = state.effects;
            var fefs = state.flyingEffects;
            var snds = state.soundEffects;
            var bufs = state.buffs;
            var secs = state.sections;
            var actors = state.sceneActors;

            foreach (var eff in effs) list.Add(eff.effect);

            foreach (var fef in fefs)
            {
                var fe = ConfigManager.Get<FlyingEffectInfo>(fef.effect);
                if (fe) fe.GetAllAssets(list);
            }

            foreach (var snd in snds) snd.GetAllAssets(list, proto, gender);

            foreach (var buf in bufs)
            {
                var bf = ConfigManager.Get<BuffInfo>(buf.intValue0);
                if (bf) bf.GetAllAssets(list);
            }

            foreach (var sec in secs)
            {
                if (sec.attackBox.isEmpty) continue;
                var a1 = AttackInfo.Get(sec.attackBox.attackInfo);
                if (a1) a1.GetAllAssets(list, proto, gender);
                var a2 = AttackInfo.Get(sec.attackBox.bulletAttackInfo);
                if (a2) a2.GetAllAssets(list, proto, gender);
            }

            foreach (var actor in actors)
            {
                var info = ConfigManager.Get<MonsterInfo>(actor.actorID);
                list.AddRange(SceneEventInfo.GetMonsterBaseAssets(info, !string.IsNullOrWhiteSpace(info.cameraAnimation)));
            }
        }
        
        var iid = m is StateMachineInfoOff ? offItemID : itemID;
        var hitEffects = m.hitEffects;
        foreach (var eff in hitEffects)
            if (!eff.isEmpty && (iid < 0 || eff.itemID == iid)) list.Add(eff.effect);

        if (m is StateMachineInfoPassive && !m.deadEffect.isEmpty)
            list.Add(m.deadEffect.effect);
    }

    #endregion

    #region Definitions

    [Serializable]
    public struct AttackBox
    {
        public enum AttackBoxType { None = 0, Box = 1, Sphere = 2, Count }

        public static readonly AttackBox empty = new AttackBox() { type = AttackBoxType.None };

        public bool isEmpty { get { return type == AttackBoxType.None; } }

        public AttackBoxType type;
        public Vector3_      start;
        public Vector3_      size;
        public int           attackInfo;
        public int           bulletAttackInfo;

        public void ParseType(int _type)
        {
            type = (_type < 1 || type >= AttackBoxType.Count) ? AttackBoxType.None : (AttackBoxType)_type;
        }
    }

    [Serializable]
    public struct Section
    {
        public int        startFrame;   // 起始帧
        public AttackBox  attackBox;    // 攻击盒子
    }

    [Serializable]
    public struct Effect
    {
        public static readonly Effect empty = new Effect() { startFrame = -1 };

        public bool isEmpty { get { return startFrame < 0; } }

        public bool markedHit { get; set; }

        public int         itemID;            // 对应的武器道具 ID
        public int         startFrame;        // 起始帧
        public string      effect;            // 特效名称
        public string      spawnAt;           // 特效播放节点名
        public bool        self;              // 特效是否只有自己可见
        public bool        interrupt;         // 是否可打断
        public bool        follow;            // 是否跟随节点
        public bool        syncRotation;      // 是否同步节点方向
        public bool        inherit;           // 当从当前状态迁移到相同的目标状态时，是否继承当前的特效，如果继承，该特效将不再播放，一般用于第一次进入循环动作时播放一次特效
        public Vector3     offset;            // 偏移
        public Vector3     rotation;          // 旋转
        public float       lockY;             // 锁定 Y 位置 仅针对非飞行类特效有效

        public void Initialize() { if (string.IsNullOrEmpty(effect)) startFrame = -1; }
    }

    [Serializable]
    public struct FlyingEffect
    {
        public static readonly FlyingEffect empty = new FlyingEffect() { startFrame = -1 };

        public bool isEmpty { get { return startFrame < 0; } }

        public int         startFrame;        // 起始帧 在作为子特效时 此值将表示为其实时间（毫秒）
        public int         effect;            // 特效配置 ID 对应 config_flyingEffectInfos 表
        public string      spawnAt;           // 特效播放节点名
        public Vector3_    direction;         // 特效飞行方向
        public double      velocity;          // 初速度
        public double      acceleration;      // 加速度
        public Vector3_    offset;            // 偏移
        public Vector3     scale;             // 缩放
        public Vector3     rotation;          // 旋转
    }

    [Serializable]
    public struct FollowTargetEffect
    {
        public enum TriggerType
        {
            None,                             //不需触发，直接激活
            ComboBreak,                       //连招中断
        }
        public enum TargetDivision
        {
            Owner,                            //拥有者
            OwnerPet,                         //拥有者的宠物
            Target,                           //目标或条件触发者
            TargetPet,                        //目标宠物
        }
        public static readonly FollowTargetEffect empty = new FollowTargetEffect() { startFrame = -1 };

        public bool        isEmpty { get { return startFrame < 0; } }
        public int         startFrame;        // 起始帧 在作为子特效时 此值将表示为其实时间（毫秒）
        public string      effect;            // 特效配置 ID 对应 config_flyingEffectInfos 表
        public string      spawnAt;           // 特效播放初始位置节点名
        public Vector3     scale;             // 缩放
        public Vector3     rotation;          // 旋转
        public TriggerType triggerType;       // 触发类型
        public bool        randomPosition;    // 是否需要随机出生点
        public TargetDivision initBind;       // 初始化绑定对象
        public TargetDivision targetBind;     // 目标绑定对象
        public SlantingThrowData motionData;    //曲线数据
    }

    [Serializable]
    public struct SingleSound
    {
        public static readonly SingleSound empty = new SingleSound() { isEmpty = true, isVoice = false, sound = null, weight = 0 };

        public bool isEmpty { get; private set; }
        
        public int    proto;    // 音效对应的职业  0 = 通用
        public bool   isVoice;  // 音效是否是语音  语音音效受到语音音量的控制
        public string sound;    // 音效资源名
        public int    weight;   // 权重，在同一个 SoundEffect 中随机选择时所占权重

        public void Initialize()
        {
            isEmpty = string.IsNullOrEmpty(sound) || string.IsNullOrWhiteSpace(sound);
            if (weight < 0) weight = 0;
        }
    }

    [Serializable]
    public class SoundEffect
    {
        public static readonly SoundEffect empty = new SoundEffect() { isEmpty = true };

        public bool isEmpty { get; private set; }

        public bool IsEmpty(int proto, int gender)
        {
            var group = m_sounds?.Get(!checkProto || proto == 0 ? 0 : proto);
            return group == null || (!checkGender || gender == 1 ? group.validSounds.Length < 1 : group.validFemaleSounds.Length < 1);
        }

        public int           startFrame;        // 起始帧 用于特效时 表示起始时间
        public int           overrideType;      // 覆盖类型   0 = 叠加  1 = 覆盖  2 = 丢弃
        public double        rate;              // 触发概率
        public SingleSound[] sounds;            // 男性音效列表
        public SingleSound[] femaleSounds;      // 女性音效列表
        public bool          checkGender;       // 是否检查性别  只有在检查性别的时候，才会区分男女角色
        public bool          checkProto;        // 是否检查职业  只有在检查职业的时候，才会区分角色职业
        public bool          interrupt;         // 切换状态时是否打断
        public bool          global;            // 所有人可见
        public bool          isVoice;           // 是否是语音？如果是 所有 SingleSound 都被当作语音处理

        private class SoundGroup
        {
            public int proto;

            public SingleSound[] validSounds { get; set; }
            public SingleSound[] validFemaleSounds { get; set; }
            public int weight { get; set; }
            public int femaleWeight { get; set; }

            public bool isEmpty { get; set; }
        }

        [NonSerialized]
        private Dictionary<int, SoundGroup> m_sounds = new Dictionary<int, SoundGroup>();

        public void Initialize()
        {
            if (rate == 0) rate = 1;
            if (sounds == null)       sounds       = new SingleSound[] { };
            if (femaleSounds == null) femaleSounds = new SingleSound[] { };

            Array.Sort(sounds,       (a, b) => a.weight < b.weight ? -1 : 1);
            Array.Sort(femaleSounds, (a, b) => a.weight < b.weight ? -1 : 1);

            isEmpty = true;

            m_sounds = new Dictionary<int, SoundGroup>();
            m_sounds.Clear();

            for (var i = 0; i < 10; ++i)
            {
                InitializeSounds(i, 0);
                InitializeSounds(i, 1);
            }
        }

        public SingleSound SelectSound(int proto = 0, int gender = 1)
        {
            return SelectSound(Random.Range(0, 1f), proto, gender);
        }

        public SingleSound SelectSound(float _weight, int proto = 0, int gender = 1)
        {
            if (isEmpty) return SingleSound.empty;

            var group = m_sounds?.Get(!checkProto || proto == 0 ? 0 : proto);
            if (group == null || group.isEmpty) return SingleSound.empty;

            SingleSound[] ss;
            var ww = 0;
            if (!checkGender || gender == 1)
            {
                ss = group.validSounds;
                ww = (int)(group.weight * _weight);
            }
            else
            {
                ss = group.validFemaleSounds;
                ww = (int)(group.femaleWeight * _weight);
            }

            if (ss == null || ss.Length < 1) return SingleSound.empty;
            if (ss.Length == 1) return ss[0];
            if (_weight >= 1) return ss[ss.Length - 1];

            foreach (var snd in ss)
                if (ww <= snd.weight) return snd;

            return SingleSound.empty;
        }

        public List<string> GetAllAssets(List<string> list = null, int proto = 0, int gender = 1)
        {
            if (list == null) list = new List<string>();

            var group = m_sounds?.Get(!checkProto || proto == 0 ? 0 : proto);

            if (group == null) return list;

            var snds = !checkGender || gender == 1 ? group.validSounds : group.validFemaleSounds;
            foreach (var snd in snds) list.Add(snd.sound);

            return list;
        }

        private void InitializeSounds(int proto, int gender)
        {
            var group = m_sounds.GetDefault(proto);
            group.proto = proto;

            var snds = gender == 1 ? sounds : femaleSounds;

            var we = 0;
            var count = 0;
            for (var i = 0; i < snds.Length; ++i)
            {
                if (snds[i].proto != proto) continue;

                snds[i].Initialize();
                var snd = snds[i];
                if (snd.isEmpty) continue;

                we += snd.weight;
                count++;
            }

            var vsnds = new SingleSound[count];

            for (int i = 0, j = 0, w = 0; j < count; ++i)
            {
                var snd = snds[i];
                if (snd.proto != proto || snd.isEmpty) continue;

                snd.weight += w;
                w = snd.weight;

                vsnds[j++] = snd;
            }

            if (gender == 1)
            {
                group.weight = we;
                group.validSounds = vsnds;
            }
            else
            {
                group.femaleWeight = we;
                group.validFemaleSounds = vsnds;
            }

            group.isEmpty = vsnds.Length < 1;
            if (!group.isEmpty) isEmpty = false;
        }
    }

    /// <summary>
    /// 通用帧数据
    /// </summary>
    [Serializable]
    public struct FrameData
    {
        public int  startFrame;        // 起始帧
        public bool disable;           // 布尔开关  用于但选择开关
        /// <summary>抖动配置 ID 对应 config_camerashakeinfos 表 | 要绑定的武器索引 对应 WeaponInfo.SingleWeapon.index | 霸体等级 | 添加的 Buff ID</summary>
        public int intValue0;
        /// <summary>绑定点配置 ID 对应 config_bindinfos 表 | Buff 持续时间</summary>
        public int intValue1;
        /// <summary>透明度 | 生效几率 0 - 1</summary>
        public double doubleValue0;
    }

    public struct SceneActor
    {
        /// <summary>
        /// MonsterInfo 表ID。所有actor都以MonsterInfo的配置表配置
        /// </summary>
        public int actorID;

        public int groupID;

        public int level;
        /// <summary>
        /// 相对偏移，与角色方向一致的方向为正，反之为负
        /// </summary>
        public Vector3_ offset;
        /// <summary>
        /// 创建帧
        /// </summary>
        public int frame;
        /// <summary>
        /// 生命时间
        /// </summary>
        public int lifeTime;
    }

    [Serializable]
    public class StateDetail : ICloneable
    {
        public int            ID;                      // 状态 ID    对应 config_creaturestateinfos 表 ID 字段     若配置了 state 会忽略 ID
        public string         state;                   // 状态 名称  对应 config_creaturestateinfos 表 name 字段
        public string         animation;               // 动画
        public int            level;                   // 状态等级   仅用于 StateOverride 配置，此处默认为 -1
        public double         damageMul;               // 伤害因子   影响当前状态的所有攻击盒子产生的 AttackInfo 默认为 1
        public Section[]      sections;                // 分段
        public FrameData[]    invisibles;              // 是否不可见
        public FrameData[]    weaks;                   // 是否虚弱
        public FrameData[]    buffs;                   // Buffs
        public Effect[]       effects;                 // 特效
        public FlyingEffect[] flyingEffects;           // 飞行特效
        public SoundEffect[]  soundEffects;            // 音效
        public FrameData[]    cameraShakes;            // 相机抖动设置
        public FrameData[]    weaponBinds;             // 武器绑定
        public FrameData[]    hideWeapons;             // 隐藏武器
        public FrameData[]    hideOffWeapons;          // 隐藏副武器
        public FrameData[]    hidePets;                // 隐藏所有宠物
        public FrameData[]    darkScreens;             // 黑屏
        public FrameData[]    rimLights;               // 隐藏边缘光
        public FrameData[]    toughs;                  // 霸体
        public SceneActor[]   sceneActors;             // 创建实体
        public Vector2        ethereal;                // 虚灵（是否可穿越其他阻挡） 起始帧和结束帧
        public Vector2        passiveEthereal;         // 虚灵（是否可被穿越）       起始帧和结束帧
        public Vector2        invincible;              // 无敌 起始帧和结束帧
        public Vector3        animMotion;              // 位移信息 [ID 起始帧 结束帧]
        public Vector2        processes;               // 状态阶段 一个状态被分为 3 段：演出段->迁移段->结束段
        public Vector2        ignoreInput;             // 忽略输入 处于该区域的帧将忽略所有按键迁移 起始帧和结束帧
        public Vector2        hideHpSlider;            // 是否隐藏血条 起始帧和结束帧
        public int            landingFrame;            // 着陆帧 当前状态如果会离开地面 则表示着陆动作开始的帧位置
        public int            layDownTime;             // 退出状态前是否进入特殊状态 StateLayDown 若 > 0 状态正常退出时将进入特殊状态 LayDown 持续 layDownTime 毫秒
        public int            coolDown;                // 冷却时间
        public int            coolGroup;               // 共享冷却分组，相同分组的状态将共享冷却时间，即其中一个状态进入CD时，所有相同组的状态都进入冷却，但它们的冷却时间仍然是独立的
        public double         fatalRadius;             // 当处于虚弱状态时，被处决的触发范围
        public bool           noDefaultTrans;          // 是否移除默认迁移 默认情况下，所有状态都会迁移到 默认状态
        public int            catchState;              // 是否是一个抓取目标的行为  0 = 非抓取  1 = 主动抓取（将对方抓到自己的位置） 2 = 被动抓取（将自己移动到目标的位置） -1 = 抱摔后续，一般用于一套抱摔连招的后续状态
        public int[]          ignoreCatchPosGroups;    // 忽略抓取行为的位置设置的状态组，默认情况下，抓取行为会将目标和当前对象的位置重合
        public bool           ignoreElementTrigger;    // 忽略元素伤害出发 处于该状态下 不触发元素伤害 Buff 只累计伤害
        public double         trackDistance;           // 追踪距离，如果大于 0，在进入状态之前会寻找在改距离内的最近敌方目标，若找不到，则放弃迁移
        public bool           ignoreTough;             // 是否忽略霸体状态
        public bool           preventTurn;             // 是否禁止迁移到主动状态之前转向
        public bool           ignoreAttackSpeed;       // 是否忽略攻击速度影响 默认情况下 所有主动动作都受到攻击速度影响（Run Idle Die 例外）
        public bool           showOffweapon;           // 强制显示副武器 默认情况下 所有非副武器状态都不显示副武器
        public bool           hideShadow;              // 强制关闭阴影 默认情况下 所有状态都会显示阴影
        public bool           disableSpringManager;    // 关闭 SpringManager
        public bool           execution;               // 是否是处决的主动状态
        public bool           ultimate;                // 是否是大招

        public string[] __overrideParams;

        /// <summary>
        /// Does this state has AttackBox ?
        /// </summary>
        public bool           hasAttackBox { get; private set; }
        /// <summary>
        /// The animator state name hash bind to this state
        /// </summary>
        public int            hash { get; private set; }
        /// <summary>
        /// Is this state valid ?
        /// </summary>
        public bool           valid { get; private set; }
        /// <summary>
        /// Group mask.
        /// Usage: groupMask.Contains(groupID)
        /// </summary>
        public int            groupMask { get; private set; }

        public void Initialize(int parent)
        {
            if (parent > 0)
            {
                level = -1;
                damageMul = 1.0;
            }

            if (landingFrame < 1) landingFrame = -1;

            if (sections == null) sections = new Section[] { };
            if (effects == null) effects = new Effect[] { };
            if (flyingEffects == null) flyingEffects = new FlyingEffect[] { };
            if (soundEffects == null) soundEffects = new SoundEffect[] { };
            if (cameraShakes == null) cameraShakes = new FrameData[] { };
            if (weaponBinds == null) weaponBinds = new FrameData[] { };
            if (hideWeapons == null) hideWeapons = new FrameData[] { };
            if (hideOffWeapons == null) hideOffWeapons = new FrameData[] { };
            if (hidePets == null) hidePets = new FrameData[] { };
            if (darkScreens == null) darkScreens = new FrameData[] { };
            if (rimLights == null) rimLights = new FrameData[] { };
            if (invisibles == null) invisibles = new FrameData[] { };
            if (weaks == null) weaks = new FrameData[] { };
            if (buffs == null) buffs = new FrameData[] { };
            if (sceneActors == null) sceneActors = new SceneActor[] {};

            Array.Sort(sections,       (l, r) => l.startFrame < r.startFrame ? -1 : 1);
            Array.Sort(effects,        (l, r) => l.startFrame < r.startFrame ? -1 : 1);
            Array.Sort(flyingEffects,  (l, r) => l.startFrame < r.startFrame ? -1 : 1);
            Array.Sort(soundEffects,   (l, r) => l.startFrame < r.startFrame ? -1 : 1);
            Array.Sort(cameraShakes,   (l, r) => l.startFrame < r.startFrame ? -1 : 1);
            Array.Sort(weaponBinds,    (l, r) => l.startFrame < r.startFrame ? -1 : 1);
            Array.Sort(hideWeapons,    (l, r) => l.startFrame < r.startFrame ? -1 : 1);
            Array.Sort(hideOffWeapons, (l, r) => l.startFrame < r.startFrame ? -1 : 1);
            Array.Sort(darkScreens,    (l, r) => l.startFrame < r.startFrame ? -1 : 1);
            Array.Sort(rimLights,      (l, r) => l.startFrame < r.startFrame ? -1 : 1);
            Array.Sort(invisibles,     (l, r) => l.startFrame < r.startFrame ? -1 : 1);
            Array.Sort(weaks,          (l, r) => l.startFrame < r.startFrame ? -1 : 1);
            Array.Sort(buffs,          (l, r) => l.startFrame < r.startFrame ? -1 : 1);
            Array.Sort(sceneActors,    (l, r) => l.frame < r.frame ? -1 : 1);


            var si = CreatureStateInfo.IDToState(!string.IsNullOrEmpty(state) ? CreatureStateInfo.NameToID(state) : ID);
            groupMask = si.groupMask;

            for (var i = 0; i< effects.Length; ++i) effects[i].Initialize();
            foreach (var snd in soundEffects) snd.Initialize();

            valid = !si.isEmpty;

            if (valid)
            {
                ID    = si.ID;
                state = si.name;
                hash  = si.hash;

                hasAttackBox = sections.FindIndex(section => !section.attackBox.isEmpty) > -1;

                processes.x = (int)processes.x;
                processes.y = (int)processes.y;

                ignoreInput.x = (int)ignoreInput.x;
                ignoreInput.y = (int)ignoreInput.y;

                if (processes.x < 0 || processes.y < 0)
                {
                    Logger.LogError("StateMachineInfo::StateDetail::Initialize: Invalid processes configuration, <b>X or Y is negative</b> StateMachine: {0}, Stages:[{4},{5}], State: [{1}:{2}], Animation: {3}", parent, ID, state, animation, processes.x, processes.y);
                    if (processes.x < 0) processes.x = 0;
                    if (processes.y < 0) processes.y = 0;
                }

                if (processes.x > processes.y)
                {
                    Logger.LogError("StateMachineInfo::StateDetail::Initialize: Invalid processes configuration, <b>X is greater than Y</b> StateMachine: {0}, Stages:[{4},{5}], State: [{1}:{2}], Animation: {3}", parent, ID, state, animation, processes.x, processes.y);
                    processes.x = processes.y;
                }

                if (ignoreInput.x < 0 || ignoreInput.y < 0)
                {
                    Logger.LogError("StateMachineInfo::StateDetail::Initialize: Invalid ignoreInput configuration, <b>X or Y is negative</b> StateMachine: {0}, IgnoreInput:[{4},{5}], State: [{1}:{2}], Animation: {3}", parent, ID, state, animation, ignoreInput.x, ignoreInput.y);
                    if (processes.x < 0) processes.x = 0;
                    if (processes.y < 0) processes.y = 0;
                }

                if (ignoreInput.x > ignoreInput.y || ignoreInput.x == ignoreInput.y && ignoreInput.x != 0)
                {
                    Logger.LogError("StateMachineInfo::StateDetail::Initialize: Invalid ignoreInput configuration, <b>X is equals or greater than Y</b> StateMachine: {0}, IgnoreInput:[{4},{5}], State: [{1}:{2}], Animation: {3}", parent, ID, state, animation, ignoreInput.x, ignoreInput.y);
                    ignoreInput.y = processes.x + 1;
                }
            }
            else if (Application.isPlaying) // Do not log error if we are in editor mode but not in playing mode
                Logger.LogError("StateMachineInfo::StateDetail::Initialize: Invalid state configuration, unknow state name or ID. StateMachine: {0}, State: [{1}:{2}], Animation: {3}", parent, ID, state, animation);
        }

        public object Clone()
        {
            var detail = MemberwiseClone() as StateDetail;
            if (null == detail) return null;

            ClonePart(ref sections,       ref detail.sections);
            ClonePart(ref invisibles,     ref detail.invisibles);
            ClonePart(ref weaks,          ref detail.weaks);
            ClonePart(ref buffs,          ref detail.buffs);
            ClonePart(ref effects,        ref detail.effects);
            ClonePart(ref flyingEffects,  ref detail.flyingEffects);
            ClonePart(ref soundEffects,   ref detail.soundEffects);
            ClonePart(ref cameraShakes,   ref detail.cameraShakes);
            ClonePart(ref weaponBinds,    ref detail.weaponBinds);
            ClonePart(ref hideWeapons,    ref detail.hideWeapons);
            ClonePart(ref hideOffWeapons, ref detail.hideOffWeapons);
            ClonePart(ref darkScreens,    ref detail.darkScreens);
            ClonePart(ref sceneActors,    ref detail.sceneActors);

            return detail;
        }

        private void ClonePart<T>(ref T[] source, ref T[] dest)
        {
            if (source == null) return;
            dest = new T[source.Length];
            Array.Copy(source, dest, dest.Length);
        }
    }

    #endregion

    public string          name;               // 状态机名称
    public StateDetail[]   states;             // 状态列表
    public int             transitionGroup;    // 状态迁移列表
    public int             aiTransitionGroup;  // 该武器AI状态迁移列表
    public int             passiveGroup;       // 被动状态机
    public Effect[]        hitEffects;         // 被动特效  仅在主动状态机和副武器状态机有效
    public Effect          deadEffect;         // 死亡特效  仅在被动状态机有效
    public FrameData[]     defaultWeaponBinds; // 默认武器绑定
    public FrameData[]     passiveWeaponBinds; // 被动武器绑定
    public FrameData[]     offWeaponBinds;     // 副武器绑定

    public bool StateAcceptInput(int state, int key)
    {
        var ts = ConfigManager.Get<TransitionInfo>(transitionGroup);
        if (!ts || ts.transitions.Length < 1) return false;

        foreach (var t in ts.transitions)
        {
            var acc = false;
            var fromID = CreatureStateInfo.NameToID(t.from);
            if (fromID == -1 || fromID == state)
            {
                var cs  = t.conditions;
                foreach (var c in cs)
                {
                    if (c.param != "key") continue;

                    var vl = Util.Parse<int>(c.threshold);
                    if      (c.mode == ConditionMode.Equals)   acc = vl == key;
                    else if (c.mode == ConditionMode.NotEqual) acc = vl != key;
                    else if (c.mode == ConditionMode.Greater)  acc = key > vl;
                    else if (c.mode == ConditionMode.Less)     acc = key < vl;
                    else acc = false;

                    if (!acc) break;
                    continue;
                }
            }

            if (acc) return acc;
        }

        return false;
    }

    public StateDetail GetState(int id)
    {
        return Array.Find(states, d => d.ID == id);
    }

    public StateDetail GetState(string name)
    {
        return Array.Find(states, d => d.state == name);
    }

    public AttackBox GetAttackBox(int stateIndex, int section)
    {
        if (states.Length < 1 || stateIndex < 0 || stateIndex >= states.Length) return AttackBox.empty;
        var s = states[stateIndex];
        section -= 1;
        if (s.sections.Length < 1 || section < 0 || section >= s.sections.Length) return AttackBox.empty;
        return s.sections[section].attackBox;
    }

    public override void Initialize()
    {
        if (defaultWeaponBinds == null) defaultWeaponBinds = new FrameData[] { };
        if (passiveWeaponBinds == null) passiveWeaponBinds = new FrameData[] { };
        if (offWeaponBinds == null)     offWeaponBinds     = new FrameData[] { };
        if (hitEffects == null)         hitEffects         = new Effect[] { };

        deadEffect.Initialize();

        for (var i = 0; i < hitEffects.Length; ++i) hitEffects[i].Initialize();
        for (var i = 0; i < states.Length; ++i) states[i].Initialize(ID);
    }
}//config StateMachineInfos : StateMachineInfo

[Serializable]
public class StateMachineInfoPassive : StateMachineInfo
{
}//config StateMachineInfosPassive : StateMachineInfoPassive

[Serializable]
public class StateMachineInfoSimple : StateMachineInfo
{
    public static Dictionary<int, List<string>> soundsDic { get; private set; } = new Dictionary<int, List<string>>();

    public override void InitializeOnce()
    {
        base.InitializeOnce();

        soundsDic.Clear();
        var simple = ConfigManager.GetAll<StateMachineInfoSimple>();
        if (simple == null) return;

        for (int i = 0; i < simple.Count; i++)
        {
            var states = simple[i].states;
            if (states == null || states.Length < 1) continue;

            for (int k = 0; k < states.Length; k++)
            {
                var soundEff = states[k].soundEffects;
                if (soundEff == null || soundEff.Length < 1) continue;

                for (int j = 0; j < soundEff.Length; j++)
                {
                    if (!soundEff[j].isVoice) continue;

                    var sounds = soundEff[j].sounds;
                    if (sounds == null || sounds.Length < 1) continue;

                    for (int h = 0; h < sounds.Length; h++)
                    {
                        if (string.IsNullOrEmpty(sounds[h].sound)) continue;

                        if (!soundsDic.ContainsKey(simple[i].ID)) soundsDic.Add(simple[i].ID, new List<string>());
                        soundsDic[simple[i].ID].Add(sounds[h].sound);
                    }
                }
            }
        }
    }
}//config StateMachineInfosSimple : StateMachineInfoSimple

[Serializable]
public class StateMachineInfoOff : StateMachineInfo
{
}//config StateMachineInfosOff : StateMachineInfoOff

[Serializable, ConfigSortOrder(1)]
public class StateOverrideInfo : ConfigItem
{
    #region Static functions

    private static Dictionary<long, StateOverride> m_cachedOverrides = new Dictionary<long, StateOverride>();

    public static StateMachineInfo.StateDetail GetOverrideState(int weapon, string state, int level, bool usePrev = true)
    {
        var si = CreatureStateInfo.NameToID(state);
        if (si < 0) return StateMachineInfo.emptyState;

        return GetOverrideState(weapon, si, level, usePrev);
    }

    public static StateMachineInfo.StateDetail GetOverrideState(int weapon, int state, int level, bool usePrev = true)
    {
        var wi = (long)weapon;
        StateMachineInfo.StateDetail st = null;
        var o = m_cachedOverrides.Get(wi | (long)state << 32);
        if (o != null)
        {
            var os = o.GetOverride(level, usePrev);
            if (os != null) st = os;
        }

        if (st == null)
        {
            var sm = ConfigManager.Get<StateMachineInfo>(weapon);
            if (sm) st = sm.GetState(state);
        }

        return st ?? StateMachineInfo.emptyState;
    }

    public static StateMachineInfo.StateDetail GetOverrideState(StateMachineInfo sm, StateMachineInfo.StateDetail current, int level, bool usePrev = true)
    {
        var wi = (long)sm.ID;
        var o = m_cachedOverrides.Get(wi | (long)current.ID << 32);
        if (o != null)
        {
            var os = o.GetOverride(level, usePrev);
            if (os != null) return os;
        }

        return current;
    }

    public static StateMachineInfo.StateDetail GetOriginOverrideState(StateMachineInfo sm, StateMachineInfo.StateDetail current, int level, bool usePrev = true)
    {
        var wi = (long)sm.ID;
        var o = m_cachedOverrides.Get(wi | (long)current.ID << 32);
        return o?.GetOriginOverride(level, usePrev);
    }

    public static double GetOverrideDamage(int weapon, int state, int level, bool usePrev = true)
    {
        var s = GetOverrideState(weapon, state, level, usePrev);
        return s != null ? s.damageMul : 1.0;
    }

    public static double GetOverrideDamage(int weapon, string state, int level, bool usePrev = true)
    {
        var s = GetOverrideState(weapon, state, level, usePrev);
        return s != null ? s.damageMul : 1.0;
    }

    #endregion

    #region Override info definition

    [Serializable]
    public class StateOverride
    {
        #region Static functions

        public static string[] ignoreFields { get { if (m_ignoreFields == null) m_ignoreFields = new string[] { "ID", "state", "animation" }; return m_ignoreFields; } }

        private static FieldInfo[] m_detailFields;
        private static string[] m_ignoreFields;
        
        #endregion

        public string state;
        
        public int stateID { get; private set; }
        public int weaponID { get; private set; }

        public StateMachineInfo.StateDetail[] overrides;

        public StateMachineInfo.StateDetail[] originOverrides;

        public StateMachineInfo.StateDetail GetOverride(int level, bool usePrev = true)
        {
            if (!usePrev) return Array.Find(overrides, o => o.level == level);
            return Array.FindLast(overrides, o => o.level <= level);
        }

        public StateMachineInfo.StateDetail GetOriginOverride(int level, bool usePrev = true)
        {
            if (!usePrev) return Array.Find(originOverrides, o => o.level == level);
            return Array.FindLast(originOverrides, o => o.level <= level);
        }

        public static StateMachineInfo.StateDetail Combin(StateMachineInfo.StateDetail source, List<StateMachineInfo.StateDetail> ss)
        {
            if (null == ss || ss.Count == 0) return source;
            StateMachineInfo.StateDetail res = source.Clone() as StateMachineInfo.StateDetail;

            if (m_detailFields == null) m_detailFields = source.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var s in ss)
            {
                foreach (var field in m_detailFields)
                {
                    var fn = field.Name;
                    if (fn[0] == '<' || fn.Length >= 2 && fn[0] == '_' && fn[1] == '_' || ignoreFields.Contains(fn, true) || !s.__overrideParams.Contains(fn, true)) continue;

                    field.SetValue(res, Util.SameTypeObjectAdd(field.GetValue(res), field.GetValue(s)));
                }
            }
            res.Initialize(-source.ID);
            return res;
        }

        public void Initialize(int weapon, StateMachineInfo sm)
        {
            if (overrides == null) overrides = new StateMachineInfo.StateDetail[] { };

            weaponID = weapon;

            stateID = CreatureStateInfo.NameToID(state);
            if (stateID < 0)
            {
                if (Application.isPlaying) Logger.LogError("StateOverride::Initialize: Invalid override state: <b>Weapon:[{0}, State:{1}]</b>", weaponID, state);
                return;
            }

            var s = sm.GetState(stateID);
            if (s == null)
            {
                if (Application.isPlaying) Logger.LogError("StateOverride::Initialize: Invalid override state, could not find state<b>[{0}:{1}]</b> from statemachine <b>[{2}:{3}]</b>", stateID, state, sm.ID, sm.name);
                return;
            }

            Array.Sort(overrides, (a, b) => a.level < b.level ? -1 : 1);
            Array.Sort(originOverrides, (a, b) => a.level < b.level ? -1 : 1);

            StateMachineInfo.StateDetail prev = s;
            foreach (var o in overrides)
            {
                if (m_detailFields == null) m_detailFields = prev.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var field in m_detailFields)
                {
                    var fn = field.Name;
                    if (fn[0] == '<' || fn.Length >= 2 && fn[0] == '_' && fn[1] == '_' || !ignoreFields.Contains(fn, true) && o.__overrideParams.Contains(fn, true)) continue;

                    field.SetValue(o, field.GetValue(prev));
                }

                o.Initialize(-sm.ID);

                prev = o;
            }
        }
    }

    #endregion

    public StateOverride[] states;
    public StateOverride[] awakeStates;

    public StateOverride GetState(string name)
    {
        return Array.Find(states, s => s.state == name);
    }

    public StateOverride GetState(int ID)
    {
        return Array.Find(states, s => s.stateID == ID);
    }

    public override void Initialize()
    {
        if (states == null) states = new StateOverride[] { };

        var sm = ConfigManager.Get<StateMachineInfo>(ID);
        if (!sm)
        {
            if (Application.isPlaying) Logger.LogError("StateOverrideInfo::Initialize: Invalid weapon ID, could not find <b>StateMachineInfo [{0}]</b>", ID);
            return;
        }

        foreach (var state in states) state.Initialize(-ID, sm);
    }

    public override void InitializeOnce()
    {
        m_cachedOverrides.Clear();
        var os = ConfigManager.GetAll<StateOverrideInfo>();
        foreach (var o in os)
        {
            var wi = (long)o.ID;
            var ss = o.states;
            if (ss == null) continue;

            foreach (var s in ss)
            {
                var si = CreatureStateInfo.NameToID(s.state);
                if (si < 0) continue;
                var key = wi | (long) si << 32;
                if (m_cachedOverrides.ContainsKey(key))
                {
                    Logger.LogError("存在相同的动作{0}  {1}，【请保证唯一性。策划检查配置】", wi, s.state);
                    continue;
                }
                m_cachedOverrides.Set(key, s);
            }
        }
    }
}//config StateOverrideInfos : StateOverrideInfo

[Serializable]
public class AttackInfo : ConfigItem
{
    public static readonly PassiveTransition hitProtectTrans = new PassiveTransition() { fromGroup = 0, toState = "StateProtect", toStateID = 1 };

    private static Dictionary<int, AttackInfo> m_cachedInfos = new Dictionary<int, AttackInfo>();

    public static AttackInfo Get(int id)
    {
        return m_cachedInfos.Get(id);
    }

    [Serializable]
    public struct PassiveTransition
    {
        public int    fromGroup;
        public string toState;
        public string toToughState;

        public int toStateID { get; set; }
        public int toToughStateID { get; set; }
    }

    public float                        acttackSlide;        // Knockback distance
    public int[]                        directionReference;  // Target direction change type. 0 = self  (creature or effect)  1 = created (source when attack collider created)  2 = source (source when hited)  others = none
    public int                          preventTurn;         // Prevent turn around if attacted
    public int                          comboLimit;          // Max allowed times in single combo state
    public int                          targetLimit;         // Max allowed hit target count
    public double                       damage;              // Damage multiplier
    public int                          freezTime;           // Freez attacker and victim in milliseconds
    public float                        selfRageReward;      // Self rage reward when hit
    public float                        targetRageReward;    // Target rage reward when hit
    public bool                         execution;           // Attack is execution damage ?
    public bool                         ignoreInvincible;    // Attack can apply to invincible targets ?
    public bool                         lockTarget;          // Attack only apply to current lock target ?
    public bool                         ignoreHitEffect;     // Ignore hit effect ?
    public int                          breakToughLevel;     // Break tough level if target is in tough state
    public bool                         isIgnoreDamge;       // 是否忽略伤害参与计算到技能说明里,默认不忽略,为true时忽略
    public StateMachineInfo.SoundEffect soundEffect;         // Sound effect when hit
    public StateMachineInfo.FrameData   selfBuff;            // Self buff when hit
    public StateMachineInfo.FrameData   targetBuff;          // Target buff when hit
    public PassiveTransition[]          targetStates;        // Passive state transitions

    public bool fromEffect { get; set; }
    public bool fromBullet { get; set; }

    public List<string> GetAllAssets(List<string> list = null, int proto = 0, int gender = 1)
    {
        if (list == null) list = new List<string>();

        var sb = ConfigManager.Get<BuffInfo>(selfBuff.intValue0);
        if (sb) sb.GetAllAssets(list);
        var tb = ConfigManager.Get<BuffInfo>(targetBuff.intValue0);
        if (tb) tb.GetAllAssets(list);

        if (!soundEffect.isEmpty) soundEffect.GetAllAssets(list, proto, gender);

        return list;
    }

    public override void Initialize()
    {
        if (targetStates == null) targetStates = new PassiveTransition[] { };
        if (soundEffect == null) soundEffect = StateMachineInfo.SoundEffect.empty;
        else soundEffect.Initialize();

        if (directionReference == null) directionReference = new int[] { 0, 0 };
        else Array.Resize(ref directionReference, 2);

        for (var i = 0; i < targetStates.Length; ++i)
        {
            var s = 0;
            ConfigManager.Find<CreatureStateInfo>(ci =>
            {
                if (s == 0 || s == 1)
                {
                    var si = ci.GetStateInfo(targetStates[i].toState, 0);
                    if (!si.isEmpty)
                    {
                        targetStates[i].toStateID = si.ID;
                        if (s == 1) return true;
                        s = 2;
                    }
                }
                if (s == 0 || s == 2)
                {
                    var si = ci.GetStateInfo(targetStates[i].toToughState, 0);
                    if (!si.isEmpty)
                    {
                        targetStates[i].toToughStateID = si.ID;
                        if (s == 2) return true;
                        s = 1;
                    }
                }
                return false;
            });
        }
    }

    public override void InitializeOnce()
    {
        m_cachedInfos.Clear();
        ConfigManager.Find<AttackInfo>(i => { m_cachedInfos.Set(i.ID, i); return false; });
    }
}//config AttackInfos : AttackInfo

[Serializable]
public class FlyingEffectInfo : ConfigItem
{
    [Serializable]
    public struct Section
    {
        public int startTime;     // 起始时间
        public int endTime;       // 结束时间
        public StateMachineInfo.AttackBox attackBox;    // 攻击盒子
    }

    public string effect;     // 特效资源名
    public bool bullet;       // 子弹？
    public StateMachineInfo.FlyingEffect hitEffect;       // 命中特效
    public StateMachineInfo.FlyingEffect[] subEffects;    // 子特效
    public StateMachineInfo.SoundEffect[] soundEffects;   // 音效
    public StateMachineInfo.FrameData[] cameraShakes;   // 震屏
    public Section[] sections;
    public int lifeTime;
    public int removeOnHit; // 撞击后是否移除  0 = 不移除  1 = 移除  2 = 撞击地面移除

    public override void Initialize()
    {
        if (sections == null)     sections     = new Section[] { };
        if (subEffects == null)   subEffects   = new StateMachineInfo.FlyingEffect[] { };
        if (soundEffects == null) soundEffects = new StateMachineInfo.SoundEffect[] { };
        if (cameraShakes == null) cameraShakes = new StateMachineInfo.FrameData[] { };

        Array.Sort(sections,     (l, r) => l.startTime  < r.startTime  ? -1 : 1);
        Array.Sort(subEffects,   (l, r) => l.startFrame < r.startFrame ? -1 : 1);
        Array.Sort(soundEffects, (l, r) => l.startFrame < r.startFrame ? -1 : 1);
        Array.Sort(cameraShakes, (l, r) => l.startFrame < r.startFrame ? -1 : 1);

        foreach (var snd in soundEffects) snd.Initialize();
    }

    public List<string> GetAllAssets(List<string> list = null)
    {
        if (list == null) list = new List<string>();

        foreach (var sub in subEffects)
        {
            var fe = ConfigManager.Get<FlyingEffectInfo>(sub.effect);
            if (fe) fe.GetAllAssets(list);
        }

        if (!hitEffect.isEmpty)
        {
            var fe = ConfigManager.Get<FlyingEffectInfo>(hitEffect.effect);
            if (fe) fe.GetAllAssets(list);
        }

        foreach (var sound in soundEffects)
            sound.GetAllAssets(list);

        foreach (var sec in sections)
        {
            if (sec.attackBox.isEmpty) continue;
            var a1 = AttackInfo.Get(sec.attackBox.attackInfo);
            if (a1) a1.GetAllAssets(list);
            var a2 = AttackInfo.Get(sec.attackBox.bulletAttackInfo);
            if (a2) a2.GetAllAssets(list);
        }

        list.Add(effect);

        return list;
    }
}//config FlyingEffectInfos : FlyingEffectInfo

[Serializable]
public class CameraShakeInfo : ConfigItem
{
    public float intensity;  // 抖动强度
    public int   duration;   // 持续时间（毫秒）
    public float range;      // 生效距离   默认为 0 表示仅触发该震动的玩家自己可见
}//config CameraShakeInfos : CameraShakeInfo

[Serializable]
public class CombatConfig : ConfigItem
{
    [Serializable]
    public struct ShowText
    {
        public float delay;     // 延迟
        public string assets;   // 资源名
        public string showText;  // 显示的文本ID
        public bool showSkillName;  // 显示技能名
        public Vector2 offset;  // 偏移值
    }

    #region Static functions

    public static CombatConfig defaultConfig { get; private set; } = new CombatConfig()
    {
        groundSmooth = 0.1f, fallSmooth = 0.2f, jumpSmooth = 0.2f, fixedFollowHeight = 0.88f, jumpFollowHeight = 1.5f, darkFadeDuration = 0.25f, maskFadeDuration = 0.2f,
        hitShakeFrames = 2, hitShakeDistance = 0.01f, deadComboLimit = 5,
        standardRunSpeed = 46.46f,
        blendFromIdle = 0, blendDefault = 0, blendToIdle = 0.1f, blendToPassive = 0, blendPassive = 0, blendFromPassive = 0.1f,
        slowMotionDuration = 6000, prepareDuration = 2000, defaultTimeLimit = 180000, defaultBurstButtonRage = 33.0f,
        defaultPrepareEffect = "", defaultWindow = "window_combat", defaultEndWindow = "window_settlement",
        defaultFactionEndWindow="window_factionsettlement",
        defaultHeavenMusic = "audio_bgm_battle_victory", defaultHellMusic = "defaultHellMusic",
        __slowMotionCurve = AnimationCurve.Linear(0, 1, 1, 1),
        screenFadeIn = new Vector2(0.3f, 1.0f), screenFadeOut = new Vector2(0.3f, 0), endWindowDelay = -1f, endWindowFadeInSpeed = 2.5f,
        comboInterval = 2000, defaultSelfShadow = "character_shadow_self",
        healthBarTween = 0.2f, healthBarDelayTween = 0.35f, healthDelayStart = 1.0f, rageBarTween = 0.2f,
        defaultMovementType = 0, defaultTouchSensitivity = 75.0f, touchSensitivityRange = new Vector2(25f, 150f), movementTypeOffset = new Vector3(0, -20.0f, 20.0f), touchSplitRatio = 1,
        ID = 0, 
    };

    #region Camera config
    public static float sgroundSmooth { get { return defaultConfig.groundSmooth; } }
    public static float sjumpSmooth { get { return defaultConfig.jumpSmooth; } }
    public static float sfallSmooth { get { return defaultConfig.fallSmooth; } }
    public static float sjumpFollowHeight { get { return defaultConfig.jumpFollowHeight; } }
    public static float sfixedFollowHeight { get { return defaultConfig.fixedFollowHeight; } }
    public static float sdarkFadeDuration { get { return defaultConfig.darkFadeDuration; } }
    public static float smaskFadeDuration { get { return defaultConfig.maskFadeDuration; } }
    #endregion

    public static double sstandardRunSpeed { get { return defaultConfig.standardRunSpeed * 0.1; } }
    public static int    shitShakeFrames { get { return defaultConfig.hitShakeFrames; } }
    public static float  shitShakeDistance { get { return defaultConfig.hitShakeDistance; } }

    public static int sdeadComboLimit { get { return defaultConfig.deadComboLimit; } }

    #region Default Animation Blend Time

    /// <summary>
    /// Blend time translate from idle to any non-passive state
    /// </summary>
    public static float sblendFromIdle { get { return defaultConfig.blendFromIdle; } }
    /// <summary>
    /// Blend time translate from non-passive to non-passive
    /// </summary>
    public static float sblendDefault { get { return defaultConfig.blendDefault; } }
    /// <summary>
    /// Blend time translate from passive to non-passive
    /// </summary>
    public static float sblendFromPassive { get { return defaultConfig.blendFromPassive; } }
    /// <summary>
    /// Blend time translate from any non-passive to idle
    /// </summary>
    public static float sblendToIdle { get { return defaultConfig.blendToIdle; } }
    /// <summary>
    /// Blend time translate from non-passive to passive
    /// </summary>
    public static float sblendToPassive { get { return defaultConfig.blendToPassive; } }
    /// <summary>
    /// Blend time translate from passive to passive
    /// </summary>
    public static float sblendPassive { get { return defaultConfig.blendPassive; } }

    #endregion

    #region Bar tween

    public static float shealthBarTween { get { return defaultConfig.healthBarTween; } }
    public static float shealthBarDelayTween { get { return defaultConfig.healthBarDelayTween; } }
    public static float shealthDelayStart { get { return defaultConfig.healthDelayStart; } }
    public static float srageBarTween { get { return defaultConfig.rageBarTween; } }

    #endregion

    public static int sslowMotionDuration { get { return defaultConfig.slowMotionDuration; } }
    public static int sprepareDuration { get { return defaultConfig.prepareDuration; } }

    public static int sdefaultTimeLimit { get { return defaultConfig.defaultTimeLimit; } }

    public static string sdefaultSelfShadow { get { return defaultConfig.defaultSelfShadow; } }

    public static string sdefaultPrepareEffect { get { return defaultConfig.defaultPrepareEffect; } }
    public static string sdefaultWindow { get { return defaultConfig.defaultWindow; } }
    public static string sdefaultEndWindow { get { return defaultConfig.defaultEndWindow; } }
    public static string sdefaultFactionEndWindow { get { return defaultConfig.defaultFactionEndWindow; } }
    public static string sdefaultUltimateMaskWindow { get { return defaultConfig.defaultUltimateWindow; } }
    public static string sdefaultHeavenMusic { get { return defaultConfig.defaultHeavenMusic; } }
    public static string sdefaultHellMusic { get { return defaultConfig.defaultHellMusic; } }
    public static int    sdefaultComboInterval { get { return defaultConfig.comboInterval <= 0 ? 2000: defaultConfig.comboInterval; } }
    public static float  sdefaultBurstButtonRage { get { return defaultConfig.defaultBurstButtonRage; } }
    public static int    sdefaultEffectPoolCount { get { return defaultConfig.defaultEffectPoolCount; } }

    public static int sdefaultMovementType { get { return defaultConfig.defaultMovementType; } }
    public static float sdefaultTouchSensitivity { get { return defaultConfig.defaultTouchSensitivity; } }
    public static Vector2 stouchSensitivityRange { get { return defaultConfig.touchSensitivityRange; } }
    public static Vector3 smovementTypeOffset { get { return defaultConfig.movementTypeOffset; } }
    public static float stouchSplitRatio { get { return defaultConfig.touchSplitRatio; } }

    public static Vector2_ spvpStart { get { return defaultConfig.pvpStart; } }

    public static Vector2 sawakeButtonState { get { return defaultConfig.awakeButtonState; } }

    public static AnimationCurve sslowMotionCurve { get { return defaultConfig.__slowMotionCurve; } }

    public static ShowText sshowText { get { return defaultConfig.showText; } }

    #region  transport scene time
    public static double spveTransportInTime { get { return defaultConfig.pveTransportIn <= 0 ? 0.5 : defaultConfig.pveTransportIn; } }
    public static double spveTransportOutTime { get { return defaultConfig.pveTransportOut <= 0 ? 0.5 : defaultConfig.pveTransportOut; } }
    #endregion

    #region  pve boss anim time
    public static double spveBossAnimWhiteAhead { get { return defaultConfig.pveBossAnimWhiteAhead < 0 ? 0 : defaultConfig.pveBossAnimWhiteAhead; } }
    public static double spveBossAnimWhiteFadeIn { get { return defaultConfig.pveBossAnimWhiteFadeIn <= 0 ? 1.0 : defaultConfig.pveBossAnimWhiteFadeIn; } }
    public static double spveBossAnimWhiteFadeOut { get { return defaultConfig.pveBossAnimWhiteFadeOut <= 0 ? 0.5 : defaultConfig.pveBossAnimWhiteFadeOut; } }
    #endregion

    #region End & Victory state

    public static Vector2 sscreenFadeIn { get { return defaultConfig.screenFadeIn; } }
    public static Vector2 sscreenFadeOut { get { return defaultConfig.screenFadeOut; } }

    public static float screenFadeInAlpha { get { return sscreenFadeIn.x; } }
    public static float screenFadeInDuration { get { return sscreenFadeIn.y; } }
    public static float screenFadeOutAlpha { get { return sscreenFadeOut.x; } }
    public static float screenFadeOutDuration { get { return sscreenFadeOut.y; } }
    public static float sendWindowDelay { get { return defaultConfig.endWindowDelay; } }
    public static float sendWindowFadeInSpeed { get { return defaultConfig.endWindowFadeInSpeed; } }

    #endregion

    #region Rim Light

    public static Color scolorInvincible { get { return defaultConfig.colorInvincible; } }
    public static Color scolorTough { get { return defaultConfig.colorTough; } }
    public static Color scolorBoth { get { return defaultConfig.colorBoth; } }
    public static float srimIntensityInvincible { get { return defaultConfig.rimIntensityInvincible; } }
    public static float srimIntensityTough { get { return defaultConfig.rimIntensityTough; } }
    public static float srimIntensityBoth { get { return defaultConfig.rimIntensityBoth; } }

    #endregion

    #endregion

    #region Camera Config

    public float groundSmooth;        // Ground state smooth time
    public float jumpSmooth;          // Jump state smooth time
    public float fallSmooth;          // Fall state smooth time
    public float jumpFollowHeight;
    public float fixedFollowHeight;
    public float darkFadeDuration;
    public float maskFadeDuration;

    #endregion

    public int   hitShakeFrames;
    public float hitShakeDistance;

    public int deadComboLimit;

    #region Default Animation Blend Time

    public float blendFromIdle    = 0;
    public float blendToIdle      = 0.1f;
    public float blendDefault     = 0;
    public float blendFromPassive = 0.15f;
    public float blendToPassive   = 0;
    public float blendPassive     = 0f;

    #endregion

    public double standardRunSpeed;   // The standard movement speed for default run animation, in dm

    public Vector2_ pvpStart = new Vector2_(-2.0, 2.0);

    public int slowMotionDuration;
    public int prepareDuration;

    public Vector2 awakeButtonState = new Vector2(1.0f, 0.3f);

    public string defaultSelfShadow = "mat_character_shadow_self";

    public string defaultPrepareEffect;
    public string defaultWindow = "window_combat";
    public string defaultEndWindow = "window_settlement";
    public string defaultFactionEndWindow = "window_factionsettlement";
    public string defaultUltimateWindow = "window_ultimatemask";
    public string defaultHeavenMusic = "audio_bgm_battle_victory";
    public string defaultHellMusic = "audio_bgm_battle_fail";
    public int    defaultTimeLimit = 180000;
    public float  defaultBurstButtonRage = 33.0f;
    public int    defaultEffectPoolCount = 2;
    public int    comboInterval = 2000;

    #region transport scene time
    public double pveTransportIn = 1;
    public double pveTransportOut = 0.5;
    #endregion

    #region pve boss anim time
    public double pveBossAnimWhiteAhead = 0;
    public double pveBossAnimWhiteFadeIn = 1.5;
    public double pveBossAnimWhiteFadeOut = 1.0;
    #endregion

    #region Bar tween

    public float healthBarTween = 0.2f;
    public float healthBarDelayTween = 0.35f;
    public float healthDelayStart = 1.0f;
    public float rageBarTween = 0.2f;

    #endregion

    #region Victory state

    public Vector2 screenFadeIn;
    public Vector2 screenFadeOut;
    public float endWindowDelay;
    public float endWindowFadeInSpeed;

    #endregion

    #region Rim Light

    public Color colorInvincible;
    public Color colorTough;
    public Color colorBoth;
    public float rimIntensityInvincible = 1.1f;
    public float rimIntensityTough = 1.1f;
    public float rimIntensityBoth = 1.2f;

    #endregion

    #region Input settings

    public int defaultMovementType = 0;
    public float defaultTouchSensitivity = 75.0f;
    public Vector2 touchSensitivityRange = new Vector2(25f, 150f);
    public Vector3 movementTypeOffset = new Vector3(0, -30.0f, 50.0f);
    public float touchSplitRatio = 1;

    #endregion

    public ShowText showText;

    public AnimationCurve __slowMotionCurve;

    public override void Initialize()
    {
        if (__slowMotionCurve == null || __slowMotionCurve.length < 1) __slowMotionCurve = AnimationCurve.Linear(0, 1, 1, 1);
        if (ID == 0) defaultConfig = this;

        if (endWindowFadeInSpeed <= 0) endWindowFadeInSpeed = 2.5f;

        if (touchSensitivityRange.x < 0) touchSensitivityRange.x = 0;
        if (touchSensitivityRange.y < 0) touchSensitivityRange.y = 0;
        if (touchSensitivityRange.x >= touchSensitivityRange.y) touchSensitivityRange.y = touchSensitivityRange.x + 50;
    }
}//config CombatConfigs : CombatConfig

[Serializable]
public class WeaponInfo : ConfigItem
{
    public static readonly WeaponInfo empty = new WeaponInfo() { ID = 0, name = "null", weapons = new Weapon[] { Weapon.empty } };

    /// <summary>
    /// 单个武器信息
    /// 一个武器组可能包含数个子武器
    /// </summary>
    [Serializable]
    public struct SingleWeapon
    {
        public static readonly SingleWeapon empty = new SingleWeapon() { index = 0, model = "" };

        public int    index;    // 在当前武器组里的索引
        public string model;    // 该武器的模型名称
        public int    bindID;   // 该武器的绑定点
        public string effects;  // 该武器对应的特效列表
    }

    [Serializable]
    public class Weapon
    {
        public static readonly Weapon empty = new Weapon() { weaponItemId = 0, singleWeapons = new SingleWeapon[] { } };

        public string name;
        public int weaponItemId;
        public int defaultAI = 1;   //每把武器可能会对应不同的AI

        public SingleWeapon[] singleWeapons;

        public int weaponID { get; private set; }
        public bool isEmpty { get; private set; }

        public void OnLoad(int _weaponID)
        {
            weaponID = _weaponID;
            isEmpty  = weaponItemId == 0;
        }

        public List<string> GetAllAssets(List<string> list = null)
        {
            if (list == null) list = new List<string>();
            foreach (var w in singleWeapons)
            {
                if (!string.IsNullOrEmpty(w.model)) list.Add(w.model);
                if (!string.IsNullOrEmpty(w.effects))
                {
                    var es = Util.ParseString<string>(w.effects);
                    list.AddRange(es);
                }
            }
            return list;
        }
    }

    public string name;
    public string victoryCameraAnimation;
    public Weapon[] weapons;

    public override void OnLoad()
    {
        foreach (var w in weapons)
            w.OnLoad(ID);
    }

    private static Dictionary<int, Weapon> m_itemHash = new Dictionary<int, Weapon>();

    public override void InitializeOnce()
    {
        m_itemHash.Clear();

        var infos = ConfigManager.GetAll<WeaponInfo>();
        foreach (var info in infos)
        {
            var ws = info.weapons;
            foreach (var w in ws)
            {
                if (w.weaponItemId == 0) continue;  // ItemId == 0 means this weapon does not have any item

                var ow = m_itemHash.Get(w.weaponItemId);
                if (ow != null)
                {
                    Logger.LogError("WeaponInfo::InitializeOnce: Duplicated weapon: [itemID:{0}, weapon:{1}, type:{2}], ignore.", w.weaponItemId, info.name, info.ID);
                    continue;
                }
                m_itemHash.Add(w.weaponItemId, w);
            }
        }
    }

    public static SingleWeapon[] GetSingleWeapons(int weaponID, int itemID)
    {
        var w = GetWeapon(weaponID, itemID);
        return w.singleWeapons;
    }

    public static Weapon GetWeapon(int weaponID, int itemID)
    {
        var w = m_itemHash.Get(itemID);
        if (w == null)
        {
            var i = ConfigManager.Get<WeaponInfo>(weaponID);
            if (i && i.weapons.Length > 0) w = i.weapons[0];
            else w = Weapon.empty;
        }
        return w;
    }

    public static string GetVictoryAnimation(int weaponID, int gender)
    {
        var w = ConfigManager.Get<WeaponInfo>(weaponID);
        if (!w || string.IsNullOrEmpty(w.victoryCameraAnimation)) return "";
        return gender != 1 ? w.victoryCameraAnimation.Replace("_nan_", "_nv_") : w.victoryCameraAnimation.Replace("_nv_", "_nan_");
    }
}//config WeaponInfos : WeaponInfo

[Serializable]
public class BindInfo : ConfigItem
{
    public string bone;        // 骨骼名称
    public Vector3 offset;     // 偏移
    public Vector3 rotation;   // 旋转
}//config BindInfos : BindInfo

[Serializable]
public class BuffInfo : ConfigItem
{
    /// <summary>
    /// Max effect count in single buff
    /// </summary>
    public const int MAX_BUFF_EFFECT_COUNT   = 5;
    /// <summary>
    /// Max effect param count
    /// </summary>
    public const int MAX_EFFECT_PARAM_COUNT  = 6;
    /// <summary>
    /// Max buff trigger param count
    /// </summary>
    public const int MAX_TRIGGER_PARAM_COUNT = 7;

    public enum ApplyTypes
    {
        Additive = 0,
        Override = 1,
        Discard  = 2,
        OverrideEx = 3,
    }

    public enum EffectTypes
    {
        Unknow          = 0,
        ModifyAttribute = 1,
        Invincible      = 2,
        Tough           = 3,
        Revive          = 4,
        Dispel          = 5,
        AddBuff         = 6,
        Shield          = 7,
        ReflectDamage   = 8,
        NightWatch      = 9,
        Steady          = 10,
        ImmuneDeath     = 11,
        Berserker       = 12,
        Heal            = 13,
        Rage            = 14,
        ForceState      = 15,
        Freez           = 16,
        ClearAttackBox  = 17,
        ShieldForBurst  = 18,
        ResetPetSkill   = 19,
        BreakTough      = 20,
        SetDuration     = 21,
        SlowClock       = 22,
        HawkEye         = 23,   //鹰眼
        Mark            = 24,   //标记
        MaskColor       = 25,   //更改遮罩颜色
        SwitchMorph     = 26,
        DarkScreen      = 27,
        SwitchMorphModel = 28,  //更换变身模型
        DamageTargetLimit = 29, //修改本身属性但是目标的属性会对修改属性有个上限限制
        DamageForTargetProperty = 30, //基于攻击者属性对目标造成伤害
        Steal           = 31,   //属性偷取
        Count,
    }

    /// <summary>
    /// 用于区别 BuffEffectType 的标志
    /// 同一个 Type 可以标记成多种类型
    /// 比如 Type = Heal(13) 可以是 Heal，也可以是 Damage 或者 Flame
    /// </summary>
    public enum EffectFlags
    {
        Unknow        = 0,
        /// <summary>
        /// 普通伤害
        /// </summary>
        Damage        = 1,
        /// <summary>
        /// 治疗
        /// </summary>
        Heal          = 2,
        /// <summary>
        /// 燃烧
        /// </summary>
        Flame         = 3,
        /// <summary>
        /// 中毒
        /// </summary>
        Poisoning     = 4,
        Count,
    }

    public enum BuffTriggerTypes
    {
        Normal          = 0,
        StartFight      = 1,
        Dead            = 2,
        TakeDamage      = 3,
        Attacked        = 4,
        Shooted         = 5,
        CritHurted      = 6,
        Health          = 7,
        TargetHealth    = 8,
        Rage            = 9,
        TargetRage      = 10,
        Field           = 11,
        TargetField     = 12,
        TotalDamage     = 13,
        ElementDamage   = 14,
        FirstHit        = 15,   //攻击盒子首次命中
        FollowEffectEnd = 16,   //等待跟随特效动画结束
        Kill            = 17,   //击杀目标
        Combo           = 18,   //连击
        ComboBreak      = 19,   //连击结束
        PercentDamage   = 20,   //每受多少伤害就会触发一次
        UseSkill        = 21,   //使用技能 1 奥义 2 处决 0 其他
        DealDamage      = 22,   //对别人造成伤害
        Attack          = 23,   //对别人发动攻击
        Crit            = 24,   //对别人造成暴击
        DamageOverFlow  = 25,   //单次伤害与目标值比较满足条件时触发
        EnterState      = 26,   //进入特定状态或状态组
        TargetGroup     = 27,   //目标是指定状态组
        BeKill          = 28,   //被击杀
        Count,
    }

    public enum ParamTriggerTypes
    {
        None,
        TakeDamage,
    }

    [Serializable] [Auto]
    public struct ExpressionVariable
    {
        public string   name;
        public int      level;
        public double   value;
        public string   showValue;

        public override string ToString()
        {
            return string.IsNullOrEmpty(showValue) ? value.ToString() : showValue;
        }
    }

    [Serializable] [Auto]
    public struct ExpressionInfo
    {
        public ExpressionVariable[] variables;
        public string body;
        [NonSerialized]
        private NoeExpression.Expression Inst;

        public bool IsValid { get { return Inst != null; } }

        public void Instance()
        {
            Inst = NoeExpression.Expression.Resolve(body);
        }

        public bool IsTrue(NoeExpression.DataHandler rHandler)
        {
            if (Inst == null)
                Instance();

            if (rHandler == null)
                return false;

            if (Inst == null)
                return true;

            return Inst.IsTrue(rHandler);
        }

        public double GetValue(NoeExpression.DataHandler rHandler)
        {
            if (Inst == null)
                Instance();

            if (rHandler == null || Inst == null)
                return 0;

            var v = Inst.GetValue(rHandler);
            return v.To<double>();
        }

        public ExpressionVariable[] GetVariables(int rLevel)
        {
            if (null == variables || variables.Length == 0)
                return null;

            var dict = new Dictionary<string, ExpressionVariable>();

            for (var i = 0; i < variables.Length; i++)
            {
                var v = variables[i];
                if (v.level <= rLevel)
                {
                    if (!dict.ContainsKey(v.name))
                        dict.Add(v.name, v);
                    else if (v.level > dict[v.name].level)
                        dict[v.name] = v;
                }
            }

            var arr = new ExpressionVariable[dict.Count];
            var o = 0;
            foreach (var kv in dict)
                arr[o++] = kv.Value;
            return arr;
        }

        public ExpressionVariable GetVariable(string name, int rLevel)
        {
            var arr = GetVariables(rLevel);
            if (arr == null)
                return default(ExpressionVariable);
            var index = Array.FindIndex(arr, v => v.name == name);
            if (index != -1)
                return arr[index];
            return default(ExpressionVariable);
        }
    }

    [Serializable]
    public struct Grow
    {
        public int level;
        public double value;
        public string showValue;

        public string ShowString { get { return string.IsNullOrEmpty(showValue) ? value.ToString() : showValue; } }

        public static bool FindGrow(List<Grow> list, int rLevel, ref Grow g)
        {
            return FindGrow(list.ToArray(), rLevel, ref g);
        }

        public static bool FindGrow(Grow[] arr, int rLevel, ref Grow g)
        {
            if (arr == null || arr.Length == 0)
                return false;
            var lv = 0;
            foreach (var grow in arr)
            {
                if (grow.level <= rLevel && grow.level > lv)
                {
                    g = grow;
                    lv = grow.level;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// 直接使用模板类无法进行序列化。所以才用这种方式定义蛋疼的实例对象
    /// </summary>
    [Serializable]
    public class DefaultGrowInt : DefaultGrow<int>
    {
        public DefaultGrowInt(int a) : base(a) { }
    }

    [Serializable]
    public class DefaultGrowDouble : DefaultGrow<double>
    {
        public DefaultGrowDouble(double value) : base(value)
        {
        }
    }

    [Serializable]
    public class DefaultGrow<T>
    {
        public T defaultValue;
        public Grow currentGrow;

        public DefaultGrow(T value)
        {
            currentGrow = default(Grow);
            defaultValue = value;
        }

        public T GetValue()
        {
            if (Mathd.Approximately(currentGrow.value, 0))
                return defaultValue;
            return Util.Parse<T>(currentGrow.value.ToString());
        }

        public void ClearGrow()
        {
            currentGrow = default(Grow);
        }

        public string ShowString()
        {
            if (currentGrow.value.Equals(default(T)))
                return this.defaultValue.ToString();
            return currentGrow.ShowString;
        }

        public static implicit operator T(DefaultGrow<T> value)
        {
            return value.GetValue();
        }
    }

    [Serializable]
    public class ParamsGrow
    {
        public List<Grow> Datas = new List<Grow>();
        public static implicit operator List<Grow>(ParamsGrow param)
        {
            return param.Datas;
        } 
    }

    [Serializable]
    public class EffectParamTrigger
    {
        public ParamTriggerTypes type;
        public bool listenSource;
        public double param0;
        public double param1;
        public double param2;
        public double param3;
        public double param4;
    }

    [Serializable]
    public class BuffEffect
    {
        public static readonly BuffEffect empty = new BuffEffect() { type = EffectTypes.Unknow, flag = EffectFlags.Unknow, paramss = new double[MAX_EFFECT_PARAM_COUNT], isEmpty = true };
        
        public EffectTypes type;
        public EffectFlags flag;

        public bool ignoreTrigger;
        /// <summary>
        /// Mark this effect as Key Effect
        /// By default, a buff may contains multiple buff effects, buff will only remove when all buff effects are dead,
        /// but if a effect is marked as key effect, buff will remove when the key effect dead.
        /// </summary>
        public bool keyEffect;
        /// <summary>
        /// 此值如果为true，此效果会等到keyEffect触发的时候同时触发
        /// </summary>
        public bool waitKeyEffect;

        public EffectParamTrigger paramTrigger;
        /// <summary>
        /// Effect apply interval in milliseconds
        /// Apply only once at buff create if <= 0
        /// </summary>
        public DefaultGrowInt interval = new DefaultGrowInt(0);
        /// <summary>
        /// Max apply count
        /// </summary>
        public DefaultGrowInt applyCount = new DefaultGrowInt(0);

        public double[]     paramss;
        public string[]     sparamss;
        public Grow[]       intervalGrow;
        public Grow[]       applyCountGrow;
        public Grow[]       coefficientGrow;
        public double conditionCoefficient;

        public ExpressionInfo expression;

        public bool isEmpty { get; private set; }
        public bool specialTrigger { get; private set; }

        public double param0 { get; set; }
        public double param1 { get ;set;}
        public double param2 { get ;set;}
        public double param3 { get ;set;}
        public double param4 { get ;set;}
        public double param5 { get; set; }

        public string GetString(int i)
        {
            return i < 0 || i >= sparamss.Length ? "" : sparamss[i];
        }

        public void Initialize()
        {
            if (type < 0 || type >= EffectTypes.Count) type = EffectTypes.Unknow;
            if (flag < 0 || flag >= EffectFlags.Count) flag = EffectFlags.Unknow;

            isEmpty = type == EffectTypes.Unknow;
            specialTrigger = type == EffectTypes.Shield || type == EffectTypes.ShieldForBurst || type == EffectTypes.ReflectDamage || type == EffectTypes.NightWatch || type == EffectTypes.Steady || type == EffectTypes.ImmuneDeath || type == EffectTypes.Berserker;

            if (paramss == null) paramss = new double[MAX_EFFECT_PARAM_COUNT];
            else if (paramss.Length < MAX_EFFECT_PARAM_COUNT) Array.Resize(ref paramss, MAX_EFFECT_PARAM_COUNT);

            if (coefficientGrow == null) coefficientGrow = new Grow[0];

            param0 = paramss[0];
            param1 = paramss[1];
            param2 = paramss[2];
            param3 = paramss[3];
            param4 = paramss[4];
            param5 = paramss[5];

            if (type == EffectTypes.ForceState)
            {
                if (!string.IsNullOrEmpty(sparamss[0]))
                {
                    param0 = CreatureStateInfo.NameToID(sparamss[0]);
                    paramss[0] = param0;
                }
            }
            else if (type == EffectTypes.Freez || type == EffectTypes.SwitchMorph)
            {
                if (!string.IsNullOrEmpty(sparamss[0]))
                {
                    param0 = CreatureStateInfo.NameToID(sparamss[0]);
                    paramss[0] = param0;
                }

                if (!string.IsNullOrEmpty(sparamss[1]))
                {
                    param1 = CreatureStateInfo.NameToID(sparamss[1]);
                    paramss[1] = param1;
                }
            }

            expression.Instance();
        }
    }

    [Serializable]
    public class BuffTrigger
    {
        public static readonly BuffTrigger normal = new BuffTrigger() { type = BuffTriggerTypes.Normal, paramss = new double[MAX_TRIGGER_PARAM_COUNT], isNormal = true };

        public BuffTriggerTypes type;
        public double chance;
        public double[] paramss;
        public string[] sparamss;
        /// <summary>
        /// 触发器可触发次数。0 表示不限次数
        /// </summary>
        public int triggerMax;
        public int triggerCount;
        public int followTriggerCount;
        public bool listenSource;
        public bool useExpression;
        public ExpressionInfo expression;

        public bool isNormal { get; private set; }

        public double param0 { get;set; }
        public double param1 { get;set; }
        public double param2 { get;set; }
        public double param3 { get;set; }
        public double param4 { get;set; }
        public double param5 { get;set; }
        public double param6 { get; set; }

        public string GetString(int i)
        {
            return i < 0 || i >= sparamss.Length ? "" : sparamss[i];
        }
        

        public void Initialize()
        {
            if (type < 0 || type >= BuffTriggerTypes.Count) type = BuffTriggerTypes.Normal;

            isNormal = type == BuffTriggerTypes.Normal;

            if (chance == 0) chance = 1;
            if (triggerCount == 0) triggerCount = 1;
            if (followTriggerCount == 0) followTriggerCount = 1;

            if (paramss == null) paramss = new double[MAX_TRIGGER_PARAM_COUNT];
            else if (paramss.Length < MAX_TRIGGER_PARAM_COUNT) Array.Resize(ref paramss, MAX_TRIGGER_PARAM_COUNT);

            if (type == BuffTriggerTypes.TotalDamage)
            {
                param6 = paramss[6];
                paramss[6] = 0;
                Array.Sort(paramss, (l, r) => l <= 0 ? 1 : r <= 0 ? -1 : l < r ? -1 : 1);
                paramss[6] = param6;
            }

            if (type == BuffTriggerTypes.ElementDamage)
            {
                paramss[6] = paramss[3] * 0.001;  // damping/sec -> damping/millisec
            }

            param0 = paramss[0];
            param1 = paramss[1];
            param2 = paramss[2];
            param3 = paramss[3];
            param4 = paramss[4];
            param5 = paramss[5];
            param6 = paramss[6];

            expression.Instance();
        }
    }

    public int textID;
    public string icon;

    public DefaultGrowInt duration;
    public DefaultGrowInt delay;
    public int priority;
    public ApplyTypes applyType;
    public bool removeOnDead;
    public bool dependSource;
    public bool petEffect;
    public BuffTrigger trigger;
    public BuffTrigger parameTrigger;
    public BuffTrigger destoryTrigger;
    public BuffEffect[] buffEffects;
    public StateMachineInfo.Effect[] effects;
    public StateMachineInfo.Effect[] endEffects;
    public StateMachineInfo.Effect[] stepEffects;
    public StateMachineInfo.FollowTargetEffect[] followEffects;

    public Grow[] durations;
    public Grow[] delays;
    public string[] runeIcons;

    public string name { get; private set; }
    public string desc { get; private set; }
    public bool resetFields { get; private set; }

    private Dictionary<int, string> m_cachedDesc = null;

    public List<string> GetAllAssets(List<string> list = null)
    {
        if (list == null) list = new List<string>();

        foreach (var eff in effects) list.Add(eff.effect);
        foreach (var eff in endEffects) list.Add(eff.effect);
        foreach (var eff in followEffects) list.Add(eff.effect);

        return list;
    }

    public override void Initialize()
    {
        if (buffEffects == null) buffEffects = new BuffEffect[] { };
        if (effects == null) effects = new StateMachineInfo.Effect[] { };
        if (endEffects == null) endEffects = new StateMachineInfo.Effect[] { };
        if (followEffects == null) followEffects = new StateMachineInfo.FollowTargetEffect[0];
        if (durations == null) durations = new Grow[] { };
        if (delays == null) delays = new Grow[] { };

        if (buffEffects.Length > MAX_BUFF_EFFECT_COUNT)
        {
            Logger.LogWarning("BuffEffect::Initialize: Too many buff effects.... Buff: {0}, count: {1}, limit: {2}", ID, buffEffects.Length, MAX_BUFF_EFFECT_COUNT);
            Array.Resize(ref buffEffects, MAX_BUFF_EFFECT_COUNT);
        }

        Array.Sort(effects,         (l, r) => l.startFrame < r.startFrame ? -1 : 1);
        Array.Sort(endEffects,      (l, r) => l.startFrame < r.startFrame ? -1 : 1);
        Array.Sort(followEffects,   (l, r) => l.startFrame < r.startFrame ? -1 : 1);

        for (var i = 0; i < buffEffects.Length; ++i) buffEffects[i].Initialize();

        trigger.Initialize();
        destoryTrigger.Initialize();
        resetFields = trigger.type != BuffTriggerTypes.TargetField && trigger.type != BuffTriggerTypes.TargetHealth && trigger.type != BuffTriggerTypes.TargetRage;

        if (textID < 1)
        {
            name = string.Empty;
            desc = string.Empty;
        }

        var text = ConfigManager.Get<ConfigText>(textID);
        if (!text)
        {
            name = string.Empty;
            desc = string.Empty;

            if (Application.isPlaying) Logger.LogWarning("BuffInfo::Initialize: Buff <b>[{0}]</b> has invalid textID <b>[{1}]</b>", ID, textID);
        }
        else
        {
            name = text[0];
            desc = text[1];
            if (!string.IsNullOrEmpty(desc) && desc.Contains("{")) desc = BuffDesc();  // If desc has dynamic params, parse it with default level
        }
    }

    public BuffInfo CalcLevel(int rLevel)
    {
        duration.ClearGrow();
        delay.ClearGrow();
        Grow.FindGrow(durations, rLevel, ref duration.currentGrow);
        Grow.FindGrow(delays, rLevel, ref delay.currentGrow);
        return this;
    }

    public string BuffDesc(int rLevel = 1)
    {
        if (m_cachedDesc == null) m_cachedDesc = new Dictionary<int, string>();

        if (m_cachedDesc.ContainsKey(rLevel)) return m_cachedDesc[rLevel];

        List<object> p = new List<object>();
        var buff = this.CalcLevel(rLevel);
        p.Add(duration.ShowString());

        var text = ConfigText.GetDefalutString(textID, 1);
        Regex r = new Regex("trigger_([a-z|0-9]*)_?([0-9]*)");
        var match = r.Match(text);
        while (match.Success)
        {
            if (match.Groups.Count > 2)
            {
                var b = ConfigManager.Get<BuffInfo>(Util.Parse<int>(match.Groups[2].Value));
                if(b)
                    buff = b.CalcLevel(rLevel);
            }
            text = text.Replace(match.Groups[0].Value, p.Count.ToString());
            var v = buff.trigger.expression.GetVariable(match.Groups[1].Value, rLevel);
            p.Add(v.ToString());
            match = r.Match(text);
        }

        r = new Regex("type([0-9])_([0-9])_?([0-9]*)");
        match = r.Match(text);
        while (match.Success)
        {
            text = text.Replace(match.Groups[0].Value, p.Count.ToString());

            if (match.Groups.Count > 3 && !string.IsNullOrWhiteSpace(match.Groups[3].Value))
            {
                var b = ConfigManager.Get<BuffInfo>(Util.Parse<int>(match.Groups[3].Value));
                if (b) buff = b.CalcLevel(rLevel);
            }

            if (buff.buffEffects.Length == 0)
            {
                p.Add("--");
                break;
            }
            var index = Convert.ToInt32(match.Groups[1].Value);
            index = Mathd.Clamp(index, 0, buff.buffEffects.Length - 1);
            var buffEffect = buff.buffEffects[index];

            int a = Util.Parse<int>(match.Groups[2].Value);
            a = Mathd.Clamp(a, 0, buffEffect.sparamss.Length - 1);
            p.Add(buffEffect.sparamss[a]);
            match = r.Match(text);
        }
        r = new Regex("([0-9])_([a-z|0-9]*)?_?([0-9]*)");
        match = r.Match(text);
        while (match.Success)
        {
            text = text.Replace(match.Groups[0].Value, p.Count.ToString());

            if (match.Groups.Count > 3 && !string.IsNullOrWhiteSpace(match.Groups[3].Value))
            {
                var b = ConfigManager.Get<BuffInfo>(Util.Parse<int>(match.Groups[3].Value));
                if (b) buff = b.CalcLevel(rLevel);
            }

            var index = Util.Parse<int>(match.Groups[1].Value);
            index = Mathd.Clamp(index, 0, buff.buffEffects.Length - 1);
            var buffEffect = buff.buffEffects[index];
            var v = buffEffect.expression.GetVariable(match.Groups[2].Value, rLevel);
            p.Add(v.ToString());
            match = r.Match(text);
        }

        text = Util.Format(text, p.ToArray());
        m_cachedDesc.Add(rLevel, text);

        return text;
    }
}//config BuffInfos : BuffInfo

[Serializable]
public class ComboInputInfo : ConfigItem
{
    [Serializable]
    public class SingleSpell
    {
        /// <summary>
        /// 输入名称
        /// </summary>
        public string spellName;
        /// <summary>
        /// 消耗几格怒气
        /// </summary>
        public int rage;

        public Color textColor = Color.white;
        public Color backColor = Color.white;

        /// <summary>
        /// 输入列表
        /// 每一项对应一个输入
        /// 1 = 点击  2 = 前划  3 = 上划 4 = 下滑 5 = 后划 6 = 方向键双击 7 = 蓄力 8 = 枪击按钮 9 = 长蓄力
        /// </summary>
        public int[] inputs;

        public void Initialize(ConfigText t)
        {
            if (inputs == null) inputs = new int[] { };

            if (!t || !string.IsNullOrEmpty(spellName)) return;

            for (var i = 0; i < inputs.Length; ++i)
                spellName += t[inputs[i]];
        }
    }

    [Serializable]
    public class SpellGroup
    {
        /// <summary>
        /// 招式组名称
        /// </summary>
        public string group;

        public SingleSpell[] spells;

        public void Initialize(ConfigText t)
        {
            group = Util.ParseStringTags(group);

            if (spells == null) spells = new SingleSpell[] { };
            foreach (var spell in spells) spell.Initialize(t);
        }
    }

    public SpellGroup[] groups;

    public override void Initialize()
    {
        if (groups == null) groups = new SpellGroup[] { };
        var t = ConfigManager.Get<ConfigText>(9205);
        foreach (var group in groups) group.Initialize(t);
    }
}//config ComboInputInfos : ComboInputInfo

[Serializable]
public class CameraShotInfo : ConfigItem
{
    [Serializable]
    public struct ShotState
    {
        #region Static functions

        public static readonly ShotState empty = new ShotState(-1);

        public static void Lerp(ref ShotState now, ref ShotState from, ref ShotState to, float t, float smooth)
        {
            var p = to.time == from.time ? 1.0f : Mathf.Clamp01((t - from.time) / (to.time - from.time));

            p = from.blend.Evaluate(p);

            now.time           = p;
            now.offset         = Vector3.Lerp(from.offset, to.offset, p);
            now.fieldOfView    = Mathf.Lerp(from.fieldOfView, to.fieldOfView, p);
            now.euler          = Util.Lerp(from.euler, to.euler, p);
            now.overrideSmooth = Mathf.Lerp(from.overrideSmooth < 0 ? smooth : from.overrideSmooth, to.overrideSmooth < 0 ? smooth : to.overrideSmooth, p);
        }

        public static ShotState Lerp(ref ShotState from, ref ShotState to, float t, float smooth)
        {
            var state = new ShotState();
            Lerp(ref state, ref from, ref to, t, smooth);
            return state;
        }

        #endregion

        public bool isEmpty { get { return time < 0; } }

        public float time;
        public Vector3 offset;
        public Vector3 euler;
        public float fieldOfView;
        public float overrideSmooth;
        public bool forceCut;
        public CameraBlend blend;
        public bool hideScene;
        public bool removeCameraEdge;
        public bool hideCombatUI;
        public string maskAsset;
        public float maskDuration;

        public ShotState(float _time)
        {
            time             = _time;
            offset           = Vector3.zero;
            euler            = Vector3.zero;
            fieldOfView      = 30.0f;
            overrideSmooth   = -0.01f;
            forceCut         = false;
            hideScene        = false;
            removeCameraEdge = false;
            hideCombatUI     = false;
            maskAsset        = null;
            maskDuration     = 0;
            blend            = new CameraBlend();
        }

        public ShotState(float _time, Vector3 _offset, Vector3 _euler, float _fieldOfView, bool _forceCut, bool _hideScene, bool _removeCameraEdge, bool _hideCombatUI, string _maskAsset, float _maskDuration)
        {
            time             = _time;
            offset           = _offset;
            euler            = _euler;
            fieldOfView      = _fieldOfView;
            blend            = CameraBlend.defaultBlend;
            overrideSmooth   = -1;
            forceCut         = _forceCut;
            hideScene        = _hideScene;
            removeCameraEdge = _removeCameraEdge;
            hideCombatUI     = _hideCombatUI;
            maskAsset        = _maskAsset;
            maskDuration     = _maskDuration;

            Validate();
        }

        public ShotState(float _time, Vector3 _offset, Vector3 _euler, float _fieldOfView, bool _forceCut, bool _hideScene, bool _removeCameraEdge, bool _hideCombatUI, string _maskAsset, float _maskDuration, CameraBlendType _blendType)
        {
            time             = _time;
            offset           = _offset;
            euler            = _euler;
            fieldOfView      = _fieldOfView;
            blend            = new CameraBlend() { blendType = _blendType };
            overrideSmooth   = -1;
            forceCut         = _forceCut;
            hideScene        = _hideScene;
            removeCameraEdge = _removeCameraEdge;
            hideCombatUI     = _hideCombatUI;
            maskAsset        = _maskAsset;
            maskDuration     = _maskDuration;

            Validate();
        }

        public ShotState(float _time, Vector3 _offset, Vector3 _euler, float _fieldOfView, bool _forceCut, bool _hideScene, bool _removeCameraEdge, bool _hideCombatUI, string _maskAsset, float _maskDuration, AnimationCurve _curve)
        {
            time             = _time;
            offset           = _offset;
            euler            = _euler;
            fieldOfView      = _fieldOfView;
            blend            = new CameraBlend() { blendType = _curve != null ? CameraBlendType.Custom : CameraBlendType.Cut, curve = _curve };
            overrideSmooth   = -1;
            forceCut         = _forceCut;
            hideScene        = _hideScene;
            removeCameraEdge = _removeCameraEdge;
            hideCombatUI     = _hideCombatUI;
            maskAsset        = _maskAsset;
            maskDuration     = _maskDuration;

            Validate();
        }

        public void Inverse()
        {
            offset.x = -offset.x;
            euler.Set(euler.x, -euler.y, -euler.z);
        }

        public void SetBlendType(CameraBlendType blendType, AnimationCurve curve = null)
        {
            blend.blendType = blendType;
            blend.curve = curve;
        }

        public void Set(Vector3 _offset, Vector3 _euler, CameraBlendType _blendType = CameraBlendType.Cut)
        {
            offset      = _offset;
            euler       = _euler;

            blend.blendType = _blendType;
        }

        public void Set(Vector3 _offset, Vector3 _euler, float _fieldOfView, CameraBlendType _blendType = CameraBlendType.Cut)
        {
            offset      = _offset;
            euler       = _euler;
            fieldOfView = _fieldOfView;

            blend.blendType = _blendType;

            Validate();
        }

        public void Set(Vector3 _offset, Vector3 _euler, float _fieldOfView, float _time, CameraBlendType _blendType = CameraBlendType.Cut)
        {
            offset      = _offset;
            euler       = _euler;
            fieldOfView = _fieldOfView;
            time        = _time;

            blend.blendType = _blendType;

            Validate();
        }

        public void Set(Vector3 _offset, Vector3 _euler, float _fieldOfView, float _overrideSmooth, float _time, CameraBlendType _blendType = CameraBlendType.Cut)
        {
            offset         = _offset;
            euler          = _euler;
            fieldOfView    = _fieldOfView;
            time           = _time;
            overrideSmooth = _overrideSmooth;

            blend.blendType = _blendType;

            Validate();
        }

        public void Validate()
        {
            time = Mathf.Clamp01(time);
            fieldOfView = Mathf.Clamp(fieldOfView, 1.0f, 179.0f);
        }

        public ShotState Clone()
        {
            var clone = this;
            clone.blend = blend.Clone();
            return clone;
        }

        public override string ToString()
        {
            return Util.Format("  time:{10:F2}, offset:[{0:F2}, {1:F2}, {2:F2}], euler:[{3:F2}, {4:F2}, {5:F2}]\n   fov:{6:F2}, smooth:{7:F2}, forceCut:{8}, blendType:{9}", offset.x, offset.y, offset.z, euler.x, euler.y, euler.z, fieldOfView, overrideSmooth, forceCut, blend.blendType, time);
        }
    }

    #region Static functions

    public static readonly CameraShotInfo empty = new CameraShotInfo() { valid = false, ID = 0, uid = 0, stateID = 0, weaponID = 0, shots = new ShotState[] { }, __sortedShots = new ShotState[] { } };

    private static Dictionary<int, CameraShotInfo> m_cachedShots = new Dictionary<int, CameraShotInfo>();

    public static void UpdateCaches()
    {
        m_cachedShots.Clear();

        var shots = ConfigManager.GetAll<CameraShotInfo>();
        foreach (var shot in shots)
        {
            if (!shot.valid)
            {
                Logger.LogError("CameraShotInfo::InitializeOnce: Invalid shot uid:<b>[{0}]</b>, weapon:<b>{1}</b>, state:<b>{2}</b> use newer one.", shot.uid, shot.weaponID, shot.stateID);
                continue;
            }

            if (m_cachedShots.ContainsKey(shot.uid)) Logger.LogError("CameraShotInfo::InitializeOnce: Duplicated shot uid:<b>[{0}]</b>, use newer one.", shot.uid);
            m_cachedShots.Set(shot.uid, shot);
        }
    }

    public static bool ShotEquals(ref ShotState a, ref ShotState b, bool checkTime = true, bool checkFOV = true)
    {
        if (checkTime)
        {
            var td = Mathf.Abs(a.time - b.time);
            if (td > 0.01f) return false;
        }

        if (checkFOV)
        {
            var fd = Mathf.Abs(a.fieldOfView - b.fieldOfView);
            if (fd > 0.01f) return false;
        }

        if ((a.offset - b.offset).magnitude > 0.01f) return false;
        if ((a.euler - b.euler).magnitude > 0.01f)   return false;

        return true;
    }

    public static CameraShotInfo GetShot(int weapon, int state)
    {
        var shot = m_cachedShots.Get(weapon << 16 | state & 0xFFFF);
        return shot ? shot : empty;
    }

    public static CameraShotInfo GetShot(int uid)
    {
        var shot = m_cachedShots.Get(uid);
        return shot ? shot : empty;
    }

    public static CameraShotInfo GetShot(int weapon, string state)
    {
        var si = CreatureStateInfo.NameToID(state);
        if (si < 0) return empty;
        var shot = m_cachedShots.Get(weapon << 16 | si & 0xFFFF);
        return shot ? shot : empty;
    }

    public static bool GetShotState(int weapon, int state, float time, out ShotState shotState)
    {
        shotState = ShotState.empty;
        var shot = GetShot(weapon, state);
        if (!shot.valid) return false;
        return shot.GetShotState(time, out shotState);
    }

    public static bool GetShotState(int uid, float time, out ShotState shotState)
    {
        shotState = ShotState.empty;
        var shot = GetShot(uid);
        if (!shot.valid) return false;
        return shot.GetShotState(time, out shotState);
    }

    public static bool GetShotState(int weapon, string state, float time, out ShotState shotState)
    {
        shotState = ShotState.empty;
        var shot = GetShot(weapon, state);
        if (!shot.valid) return false;
        return shot.GetShotState(time, out shotState);
    }

    public static bool GetShotState(int weapon, int state, float time, out CameraShotInfo shot, out ShotState shotState)
    {
        shotState = ShotState.empty;
        shot = GetShot(weapon, state);

        if (!shot.valid) return false;
        return shot.GetShotState(time, out shotState);
    }

    public static bool GetShotState(int uid, float time, out CameraShotInfo shot, out ShotState shotState)
    {
        shotState = ShotState.empty;
        shot = GetShot(uid);

        if (!shot.valid) return false;
        return shot.GetShotState(time, out shotState);
    }

    public static bool GetShotState(int weapon, string state, float time, out CameraShotInfo shot, out ShotState shotState)
    {
        shotState = ShotState.empty;
        shot = GetShot(weapon, state);
        if (!shot.valid) return false;
        return shot.GetShotState(time, out shotState);
    }

    public static bool GetShotState(int weapon, int state, float time, int curIndex, out ShotState shotState)
    {
        shotState = ShotState.empty;
        var shot = GetShot(weapon, state);
        if (!shot.valid) return false;
        return shot.GetShotState(time, curIndex, out shotState);
    }

    public static bool GetShotState(int uid, float time, int curIndex, out ShotState shotState)
    {
        shotState = ShotState.empty;
        var shot = GetShot(uid);
        if (!shot.valid) return false;
        return shot.GetShotState(time, curIndex, out shotState);
    }

    public static bool GetShotState(int weapon, string state, float time, int curIndex, out ShotState shotState)
    {
        shotState = ShotState.empty;
        var shot = GetShot(weapon, state);
        if (!shot.valid) return false;
        return shot.GetShotState(time, curIndex, out shotState);
    }

    public static bool GetShotState(int weapon, int state, float time, int curIndex, out CameraShotInfo shot, out ShotState shotState)
    {
        shotState = ShotState.empty;
        shot = GetShot(weapon, state);

        if (!shot.valid) return false;
        return shot.GetShotState(time, curIndex, out shotState);
    }

    public static bool GetShotState(int uid, float time, int curIndex, out CameraShotInfo shot, out ShotState shotState)
    {
        shotState = ShotState.empty;
        shot = GetShot(uid);

        if (!shot.valid) return false;
        return shot.GetShotState(time, curIndex, out shotState);
    }

    public static bool GetShotState(int weapon, string state, float time, int curIndex, out CameraShotInfo shot, out ShotState shotState)
    {
        shotState = ShotState.empty;
        shot = GetShot(weapon, state);
        if (!shot.valid) return false;
        return shot.GetShotState(time, curIndex, out shotState);
    }

    #endregion

    public bool valid { get; private set; }
    public int weaponID { get; private set; }
    public int stateID { get; private set; }

    public int uid;   // Unique ID, 1-16: State ID, 17-32: Weapon ID

    public ShotState[] shots;

    public ShotState[] sortedShots
    {
        get
        {
            if (__sortedShots == null)
            {
                if (shots == null) shots = new ShotState[] { };
                __sortedShots = shots.SimpleClone();
                Array.Sort(__sortedShots, (a, b) => a.time < b.time ? -1 : 1);

            }
            return __sortedShots;
        }
    }
    private ShotState[] __sortedShots = null;

    public ShotState[] UpdateSortedShots()
    {
        __sortedShots = null;
        return sortedShots;
    }

    public ShotState GetShotState(int index)
    {
        return index < 0 || index >= shots.Length ? ShotState.empty : shots[index];
    }

    public ShotState GetSortedShotState(int index)
    {
        return index < 0 || index >= __sortedShots.Length ? ShotState.empty : __sortedShots[index];
    }

    public bool HasShotAt(float time, float threshold = 0.01f)
    {
        return shots.FindIndex(s => Mathf.Abs(s.time - time) <= threshold) > -1;
    }

    public bool GetShotState(float time, int curIndex, out ShotState state)
    {
        state = ShotState.empty;
        var idx = __sortedShots.FindIndex(curIndex, __sortedShots.Length, s => s.time <= time);
        if (idx > -1) state = __sortedShots[idx];
        return state.time >= 0;
    }

    public bool GetShotState(float time, out ShotState state)
    {
        state = ShotState.empty;
        var idx = __sortedShots.FindLastIndex(s => s.time <= time);
        if (idx > -1) state = __sortedShots[idx];
        return state.time >= 0;
    }

    public override void OnLoad()
    {
        weaponID = uid >> 16;
        stateID  = uid & 0xFFFF;

        valid = weaponID != 0 && stateID != 0;

        if (shots == null) shots = new ShotState[] { };
        for (var i = 0; i < shots.Length; ++i) shots[i].Validate();

        UpdateSortedShots();
    }

    public override void InitializeOnce()
    {
        UpdateCaches();
    }
}//config CameraShotInfos : CameraShotInfo

#endregion

#region PVE模块

[Serializable]
public class StageInfo : ConfigItem
{
    public int levelInfoId;                 //关卡对应的场景ID,关联到levelInfo.Id
    public int sceneEventId;                //关联的事件组的ID
    public uint exp;                        //通关后的经验奖励
    public uint coin;                       //通关后的金币奖励
    public uint awardItemGroupId;           //道具组Id
    public int[] previewRewardItemId;       //预览的道具奖励ID
    public bool levelCorrection;            //关卡等级修正
    public double aggressiveCorrection;     //怪物攻击性质的属性模板修正(只修正MonsterAttriubute 的值)，与levelCorrection没有直接关系
    public double defensiveCorrection;      //怪物防御性质的属性模板修正(只修正MonsterAttriubute 的值)，与levelCorrection没有直接关系
    public string icon;                     //关卡默认音乐
    public EnumPVEAutoBattleType autoFight; //自动战斗类型
    public double playerPos = 0;             //玩家的相对位置
    public bool again;                      //是否可再次挑战
    public int  rush;                       //扫荡条件0 不能扫荡 1通关即可扫荡 2 三星才能扫荡
    public int npcAssist;                   //npc助战显示类型。0 无助战 1 未通关不助战，通关助战 2必然可助战
    public int addNpcExpr;                    //通关后添加NPC好感度
}//config StageInfos : StageInfo

[Serializable]
public class SceneFrameEventInfo : ConfigItem
{
    [Serializable]
    public class SceneFrameEventItem
    {
        public string frameState;                               //注册的帧事件状态
        public int frameCount;                               //注册的帧事件的帧数
        public int limitTimes;                                  //当前帧事件的可执行次数,如果不限制，配置为-1
        public SceneEventInfo.SceneBehaviour[] behaviours;      //触发的行为

        public bool haveRestTimes { get { return limitTimes < 0 || limitTimes > 0; } }
        public void DecreseTimes()
        {
            if (limitTimes < 0) return;
            limitTimes--;
        }

        public bool IsValidState(string state)
        {
            return frameState.Equals(state);
        }

        public bool TriggerFrameEvent(StateMachineState state)
        {
            if (!state) return false;
            return IsValidState(state.name) && state.currentFrame >= frameCount;
        }
    }

    /// <summary>
    /// 多个状态的帧事件
    /// </summary>
    public SceneFrameEventItem[] eventItems;

    public SceneFrameEventItem GetTarFrameEventItem(StateMachineState state)
    {
        if (eventItems == null || eventItems.Length == 0 || !state) return null;

        return eventItems.Find(item =>item.frameState.Equals(state.name));
    }

}//config SceneFrameEventInfos : SceneFrameEventInfo

[Serializable]
public class SceneEventInfo : ConfigItem
{
    public enum SceneBehaviouType
    {
        None = 0,

        /// <summary>
        /// 创建怪物行为，子配置：MonsterID,Group(-2代表NPC，-1代表所有敌方怪物，剩下所有代表指定第Group波的怪物),
        /// Level（怪物等级）,ReletivePos（出生的相对位置）,IsBoss是否是BOSS，BOSS = 1，else = 0 
        /// CameraAnim (默认为0,不显示 1表示开启出场摄像机动画) FrameEventID(检测的帧事件ID)
        /// ForceDirection 强制方向，当强制方向不为0时，表示AI不能对其设置方向  -1为朝向左边（back） 1为朝向右边(forward)
        /// </summary>
        CreateMonster,

        /// <summary>
        /// 杀死怪物行为，子配置：MonsterID,Group,Amout
        /// </summary>
        KillMonster,

        /// <summary>
        /// 开始倒计时行为，子配置：TimerID,TimeAmout(毫秒)，IsShow是否显示在UI上（因为UI上目前只能接受1个)，1为显示，0为不显示
        /// </summary>
        StartCountDown,

        /// <summary>
        /// 增加/重置倒计时行为，子配置：TimerID,TimeAmout(毫秒),AbsoluteValue(单位：毫秒,如果该值有数值就直接将倒计时设置为这个值)
        /// </summary>
        AddTimerValue,

        /// <summary>
        /// 删除计时器行为，子配置：TimerID
        /// </summary>
        DelTimer,

        /// <summary>
        /// 关卡通过，空参
        /// </summary>
        StageClear,

        /// <summary>
        /// 关卡失败
        /// </summary>
        StageFail,

        /// <summary>
        /// 增加BUFF行为
        /// 子配置：ObjectID(-1表示所有怪物，-2表示玩家，其余表示场景上的怪物ID),
        /// BuffID,Duraction
        /// </summary>
        AddBuffer,

        /// <summary>
        /// 剧情动画，子配置：PlotID,PlotType(1代表剧场对话,2代表战斗对话)
        /// </summary>
        StartStoryDialogue,

        /// <summary>
        /// 显示提示信息,子配置:Time(毫秒),TexID(对应config_text)
        /// </summary>
        ShowMessage,

        /// <summary>
        /// 操作计数器，子配置：ConterID,NumberChange
        /// </summary>
        OperatingCounter,

        /// <summary>
        /// 怪物退场行为,子配置：MonsterID,Group,LeaveTime(持续时间,只针对循环动作,单位：毫秒),State
        /// </summary>
        LeaveMonster,

        /// <summary>
        /// 强制设置状态行为,子配置:MonsterID,Group,State,SetType(0 强制 1只在地面才会成功播放 2只在空中可以成功播放)
        /// </summary>
        SetState,

        /// <summary>
        /// 检查关卡的首次条件分为以下几种:
        /// 1.如果关卡还未成功通关，返回 StageFirstTime 事件
        /// 2.如果关卡还从未进入过 返回  EnterForFirstTime 事件(只有跟追捕相关的关卡才可以使用改配置，无主之地和迷宫没法使用)
        /// 子配置：空参
        /// </summary>
        CheckStageFirstTime,

        /// <summary>
        /// 设置AI的暂停状态,子配置：MonsterID,Group,Pause:1表示暂停AI，0表示恢复AI
        /// </summary>
        AIPauseState,

        /// <summary>
        /// 强制返回大厅(新手引导可能会使用)，空参
        /// </summary>
        BackToHome,

        /// <summary>
        /// 开启一段新手引导,子配置 GuideID
        /// </summary>
        StartGuide,

        /// <summary>
        /// 播放音频文件，子配置AudioType :0(音效) 1（语音） 2（音乐） ；Loop ：1（循环），0（不循环）；AudioName(音频文件) 
        /// </summary>
        PlayAudio,

        /// <summary>
        /// 停止播放音频文件，子配置：AudioName(音频文件) 
        /// PS;如果播放后需要恢复默认音乐，直接配置PlayAudio行为
        /// </summary>
        StopAudio,

        /// <summary>
        /// BOSS预警动画出现,子配置:BossTimerID,TimeAmout(毫秒),BossId1,BossId2
        /// </summary>
        BossComing,

        /// <summary>
        /// 跳转新的场景，并且加载新的场景事件组
        /// 子配置：LevelId,Position,EventId,DelayTime,State
        /// </summary>
        TransportScene,

        /// <summary>
        /// 创建触发器
        /// 子配置：TriggerID, 
        /// State : 0 表示关闭 1 表示激活
        /// Flag  0 = 普通触发器 1 = 切换场景触发器
        /// Range:触发器的范围，（vector4）分别代表x,y,w,h（x坐标，y坐标，宽，高）
        /// Effects 用分号隔开，0表示激活的特效，1表示关闭或者未激活的特效
        /// </summary>
        CreateTrigger,

        /// <summary>
        /// 操作触发器
        /// 子配置：TriggerID,  State : 0 表示关闭 1 表示激活
        /// </summary>
        OperateTrigger,

        /// <summary>
        /// 场景分区
        /// 子配置:Direction (分为)左边（int = 0），右边（int = 1）;AdditiveValue : 需要叠加的边界值（edge += value）  AbsoluteValue : 设置边界的绝对值 (edge = absoluteValue)
        /// </summary>
        OperateSceneArea,

        /// <summary>
        /// 创建创景实体物体
        /// 子配置：SceneActorId(对应monsterinfo)，LogicID(唯一ID),ReletivePos（出生的相对位置），Level（怪物等级）
        /// State(string) 需要忽略MonsterInfo.bordState
        /// </summary>
        CreateSceneActor,

        /// <summary>
        /// 操作场景实体物体
        /// 子配置：LogicID (唯一ID)，State（string，切换到的状态）
        /// </summary>
        OperateSceneActor,

        /// <summary>
        /// 删除场景实体物体事件
        /// 子配置：LogicID, State(指定的状态),StateType = 1表示状态开始  = 2表示状态结束
        /// </summary>
        DelSceneActorEvent,

        /// <summary>
        /// 创建小怪行为
        /// 子配置：MonsterID,Group(-2代表NPC，-1代表所有敌方怪物，剩下所有代表指定第Group波的怪物), Level（怪物等级）
        /// Offset (相对于初始位置的偏移值 vec4)
        /// </summary>
        CreateLittle,

        /// <summary>
        /// 创建一个随机数条件
        /// 子配置：RandomID,MaxValue(策划的需求是从1开始，并且最大能够取到MaxValue)
        /// </summary>
        BuildRandom,

        /// <summary>
        /// 移动怪物位置
        /// 子配置：MonsterID,Group(-2代表NPC，-1代表所有敌方怪物，剩下所有代表指定第Group波的怪物),ReletivePos(vector4参数)
        /// </summary>
        MoveMonsterPos,

        /// <summary>
        /// 创建助战角色，子配置：LogicID,ReletivePos（出生的相对位置）,BornState (出场动画，怪物的出场动画再monsterinfo中，助战角色需要单独配置)
        /// </summary>
        CreateAssistant,

        /// <summary>
        /// 创建助战Npc角色，子配置：LogicID,ReletivePos（出生的相对位置）,BornState (出场动画，怪物的出场动画再monsterinfo中，助战角色需要单独配置)
        /// </summary>
        CreateAssistantNpc,

        /// <summary>
        /// 删除尚未触发的condition
        /// </summary>
        DeleteCondition,

        /// <summary>
        /// 动态增加条件的amount值
        /// </summary>
        IncreaseConditionAmount,

        /// <summary>
        /// 创建一个随机数条件
        /// 子配置：RandomID,MaxValue(策划的需求是从1开始，并且最大能够取到MaxValue)
        /// </summary>
        RebuildRandom,
    }

    public enum SceneConditionType
    {
        None,

        /// <summary>
        /// 进入场景,空参
        /// </summary>
        EnterScene,

        /// <summary>
        /// 倒计时，包含一个字段TimerID
        /// </summary>
        CountDownEnd,

        /// <summary>
        /// 怪物死亡,子配置：MonsterId,Group,Amout
        /// </summary>
        MonsterDeath,

        /// <summary>
        /// 玩家初次进入关卡,空参
        /// </summary>
        StageFirstTime,

        /// <summary>
        /// 剧情动画结束，子配置：PlotId（哪一个剧情对话结束）
        /// </summary>
        StoryDialogueEnd,

        /// <summary>
        /// 计数器,子配置,CounterID,NumberChange(代表的是数值的增量而不是数值的绝对值)
        /// </summary>
        CounterNumber,

        /// <summary>
        /// 怪物离场事件,子配置:MonsterID,Group
        /// </summary>
        MonsterLeaveEnd,

        /// <summary>
        /// 怪物血量小于某个值，子配置：MonsterID,Group,LessThan:(配置为50--代表低于50%)，
        /// </summary>
        MonsterHPLess,

        /// <summary>
        /// 新手引导结束，子配置 GuideID
        /// </summary>
        GuideEnd,

        /// <summary>
        /// BOSS预警动画完毕，子配置 : BossTimerID
        /// </summary>
        BossComingEnd,

        /// <summary>
        /// 玩家职业限定
        /// </summary>
        PlayerVocation,

        /// <summary>
        /// 触发器被触发，子配置 : TriggerID  ; TriggerType: 1 进入触发器 2 离开触发器 ; PlayerNum 触发当前触发器操作时的玩家数量
        /// </summary>
        EnterTrigger,

        /// <summary>
        /// 场景实体物体状态变化事件
        /// 子配置：LogicID (唯一ID)；State(string,状态),StateType = 1表示状态开始  = 2表示状态结束
        /// </summary>
        SceneActorState,

        /// <summary>
        /// window_combat窗口打开之后
        /// 参数，空参
        /// </summary>
        WindowCombatVisible,

        /// <summary>
        /// 第一次 进入 关卡，，一旦进入之后，该值就会变为false
        /// </summary>
        EnterForFirstTime,

        /// <summary>
        /// 随机条件
        /// 子配置 RandomID, MinValue 符合条件的最小值（包含）, MaxValue符合条件的最大值（包含），Value(生成的随机值)
        /// </summary>
        RandomInfo,

        /// <summary>
        /// 怪物成功击中敌对角色,子配置：MonsterId,Group
        /// </summary>
        MonsterAttack,

        /// <summary>
        /// 玩家血量小于某个值，LessThan:(配置为50--代表低于50%)，
        /// </summary>
        PlayerHPLess,

        /// <summary>
        /// 受击次数
        /// </summary>
        HitTimes,
    }

    //整合关卡行为信息,目前只需要五个参数
    [Serializable]
    public class SceneBehaviour
    {
        public static readonly SceneBehaviour empty = new SceneBehaviour()
        {
            sceneBehaviorType = SceneBehaviouType.None,
            parameters = new int[4],
            strParam = string.Empty,
        };

        public SceneBehaviouType sceneBehaviorType = SceneBehaviouType.None;

        /// <summary>
        /// 当前场景行为的参数
        /// </summary>
        public int[] parameters;
        public Vector4_ vecParam;
        public string strParam;

        public virtual SceneBehaviour DeepClone()
        {
            SceneBehaviour b = new SceneBehaviour();
            b.sceneBehaviorType = sceneBehaviorType;
            b.parameters = (int[])parameters.Clone();
            b.vecParam = new Vector4_(vecParam.x, vecParam.y, vecParam.z, vecParam.w);
            b.strParam = strParam;
            return b;
        }
    }

    /// <summary>
    /// 整合关卡条件信息，目前只需要三个数据
    /// </summary>
    [Serializable]
    public class SceneCondition
    {
        public static readonly SceneCondition empty = new SceneCondition()
        {
            sceneEventType = SceneConditionType.None,
            parameters = new int[3],
        };

        public SceneConditionType sceneEventType = SceneConditionType.None;

        /// <summary>
        /// 当前场景条件信息的参数
        /// </summary>
        public int[] parameters;
        public string strParam;
        public int conditionId;
    }

    [Serializable]
    public class SceneEvent
    {
        public static readonly SceneEvent empty = new SceneEvent() {
            conditions = new SceneCondition[] { },
            behaviours = new SceneBehaviour[] { },
        };

        public int times = 1;
        //触发该事件的所有条件
        public SceneCondition[] conditions;
        //成功触发该事件的所有行为
        public SceneBehaviour[] behaviours;
    }

    public SceneEvent[] sceneEvents;

    public override void OnLoad()
    {
        base.OnLoad();
        foreach (var item in sceneEvents)
        {
            item.times = item.times == 0 ? 1 : item.times;
        }
    }

    public List<int> GetAllMonsters()
    {
        var list = new List<int>();
        if (sceneEvents == null) return list;
        bool onlyFirst = false;
        foreach (var item in sceneEvents)
        {
            onlyFirst = false;
            foreach (var b in item.conditions)
            {
                if (b.sceneEventType == SceneConditionType.StageFirstTime || b.sceneEventType == SceneConditionType.StoryDialogueEnd) onlyFirst = true;
            }
            if (onlyFirst) continue;
            foreach (var b in item.behaviours)
            {
                if (b.sceneBehaviorType != SceneBehaviouType.CreateMonster && b.sceneBehaviorType != SceneBehaviouType.CreateLittle) continue;

                if (!list.Contains(b.parameters.GetValue<int>(0))) list.Add(b.parameters.GetValue<int>(0));
            }
        }
        return list;
    }

    public List<int> GetAllTranScenceID()
    {
        var list = new List<int>();
        if (sceneEvents == null) return list;
        foreach (var item in sceneEvents)
        {
            foreach (var b in item.behaviours)
            {
                if (b.sceneBehaviorType == SceneBehaviouType.TransportScene)
                {
                    int eventId = b.parameters.GetValue<int>(2);
                    var newEventInfo = ConfigManager.Get<SceneEventInfo>(eventId);
                    if (!newEventInfo) continue;
                    if (!list.Contains(eventId)) list.Add(eventId);
                }
            }
        }
        return list;
    }

    #region check config valid

    public static void CheckSceneEventValid()
    {
        var all = ConfigManager.GetAll<SceneEventInfo>();
        var mons = ConfigManager.GetAll<MonsterInfo>();
        var stories = ConfigManager.GetAll<StoryInfo>();
        var levels = ConfigManager.GetAll<LevelInfo>();
        var npcs = ConfigManager.GetAll<NpcInfo>();

        var perSceneAllMonsters = new List<int>();
        var battleStoryMons = new Dictionary<int, List<int>>();
        var validEnd = false;

        foreach (var item in all)
        {
            perSceneAllMonsters.Clear();
            battleStoryMons.Clear();
            validEnd = false;

            for (int i = 0, length = item.sceneEvents.Length; i < length; i++)
            {
                //一个组的行为与事件
                SceneEvent events = item.sceneEvents[i];

                if (events.conditions == null || events.conditions.Length == 0) Logger.LogError("sceneEvent id [{0}] has the invalid contidions with index [{1}]", item.ID, i);
                if (events.behaviours == null || events.behaviours.Length == 0) Logger.LogError("sceneEvent id [{0}] has the invalid behaviours with index [{1}]", item.ID, i);

                foreach (var be in events.behaviours)
                {
                    if (be.sceneBehaviorType == SceneBehaviouType.CreateMonster || be.sceneBehaviorType == SceneBehaviouType.CreateSceneActor)
                    {
                        int monsterId = be.parameters.GetValue<int>(0);
                        MonsterInfo m = mons.Find(o => o.ID == monsterId);
                        if (!m) Logger.LogError("sceneEvent id {0} has a invalid monster with monsterId = {1}", item.ID, monsterId);
                        perSceneAllMonsters.Add(monsterId);
                    }
                    else if (be.sceneBehaviorType == SceneBehaviouType.StartStoryDialogue)
                    {
                        int storyId = be.parameters.GetValue<int>(0);
                        EnumStoryType st = (EnumStoryType)be.parameters.GetValue<int>(1);
                        StoryInfo s = stories.Find(o => o.ID == storyId);
                        if (!s)
                        {
                            Logger.LogError("sceneEvent id {0} has a invalid Stroy with storyId = {1}", item.ID, storyId);
                            continue;
                        }

                        //获取所有的对话怪物
                        foreach (var ps in s.storyItems)
                        {
                            if(ps.talkingRoles != null && ps.talkingRoles.Length > 0)
                            {
                                foreach (var tr in ps.talkingRoles)
                                {
                                    //策划需求：0表示配置创建角色自己
                                    if (tr.roleId == 0)
                                        continue;
                                    if (st == EnumStoryType.TheatreStory)
                                    {
                                        NpcInfo n = npcs.Find(o => o.ID == tr.roleId);
                                        if (!n) Logger.LogError("sceneEvent id {0} storyId {1} has a invalid npc with npcId = {2}", item.ID, storyId, tr.roleId);
                                    }
                                    else
                                    {
                                        if (!battleStoryMons.ContainsKey(storyId)) battleStoryMons.Add(storyId,new List<int>());

                                        if (!battleStoryMons[storyId].Contains(tr.roleId)) battleStoryMons[storyId].Add(tr.roleId);
                                    }
                                }
                            }
                        }
                    }
                    else if (be.sceneBehaviorType == SceneBehaviouType.TransportScene)
                    {
                        validEnd = true;
                        int levelId = be.parameters.GetValue<int>(0);
                        LevelInfo l = levels.Find(o => o.ID == levelId);
                        if (!l) Logger.LogError("sceneEvent id {0} has a invalid TransportScene event with levelId = {1}", item.ID, levelId);
                    }
                    else if (be.sceneBehaviorType == SceneBehaviouType.BackToHome || be.sceneBehaviorType == SceneBehaviouType.StageClear) validEnd = true;
                }
            }

            //检查缓存的所有战斗对话的怪物
            foreach (var sm in battleStoryMons)
            {
                foreach (var mon in sm.Value)
                {
                    int index = perSceneAllMonsters.IndexOf(mon);
                    if (index < 0) Logger.LogError("sceneEvent id {0} havn't create monster with monId = {2} that be used in story {3}", item.ID, sm.Key, mon, sm.Key);
                }
            }

            //最后检查是否没有合法的胜利结算
            if (!validEnd) Logger.LogError("sceneEvent id [{0}] has the invalid end(without backtohome or stageclear event)", item.ID);
        }
    }

    public static void CheckRandomBoss()
    {
        var events = ConfigManager.GetAll<SceneEventInfo>();
        var tasks = ConfigManager.GetAll<TaskInfo>();
        var stageinfos = ConfigManager.GetAll<StageInfo>();

        Dictionary<int, List<int>> rewardMonsterDic = new Dictionary<int, List<int>>();
        Dictionary<int, TaskInfo> taskEventDic = new Dictionary<int, TaskInfo>();

        foreach (var task in tasks)
        {
            var stage = stageinfos.Find(o=>o.ID == task.stageId);
            if (!stage)
            {
                Logger.LogError("task {0}:{1} has a invalid stage with stageID {2}",task.ID,task.name,task.stageId);
                continue;
            }

            var e = events.Find(ev=>ev.ID == stage.sceneEventId);
            if(!e)
            {
                Logger.LogError("task {0} has a invalid stage with stageID {1}", task.ID, task.stageId);
                continue;
            }

            if (!taskEventDic.ContainsKey(e.ID)) taskEventDic.Add(e.ID,null);
            taskEventDic[e.ID] = task;
        }

        List<int> list = new List<int>();
        foreach (var item in taskEventDic)
        {
            var eventInfo = events.Find(e => e.ID == item.Key);
            list.Clear();
            rewardMonsterDic.Add(item.Key, GetRewardBoss(eventInfo,list));
        }

        foreach (var item in rewardMonsterDic)
        {
            if (item.Value == null || item.Value.Count == 0) continue;

            TaskInfo t = taskEventDic.Get(item.Key);
            if (!t)
            {
                Logger.LogError("scene event id = {0} has the reward boss[{1}],but thear is no relevant task",item.Key,item.Value.ToXml());
                continue;
            }

            foreach (var mon in item.Value)
            {
                if(!t.bossID.Contains(mon))
                {
                    Logger.LogError("scene event id = {0} has the reward boss[{1}],but task bossId is [{2}]", item.Key, item.Value.ToXml(),t.bossID.ToXml());
                    break;
                }
            }
        }
    }

    private static List<int> GetRewardBoss(SceneEventInfo info,List<int> loadedData)
    {
        List<int> monsters = new List<int>();
        if (loadedData.Contains(info.ID)) return monsters;

        loadedData.Add(info.ID);
        
        foreach (var item in info.sceneEvents)
        {
            foreach (var b in item.behaviours)
            {
                if (b.sceneBehaviorType == SceneBehaviouType.CreateMonster && b.parameters.GetValue<int>(7) > 0)
                    monsters.Add(b.parameters.GetValue<int>(0));
                else if (b.sceneBehaviorType == SceneBehaviouType.TransportScene)
                {
                    int eventId = b.parameters.GetValue<int>(2);
                    var newEventInfo = ConfigManager.Get<SceneEventInfo>(eventId);
                    if(!newEventInfo)
                    {
                        Logger.LogError("sceneevent.id = {0} has a invalid TransportScene behaviour with new eventid {1}",info.ID,eventId);
                        continue;
                    }
                    
                    monsters.AddRange(GetRewardBoss(newEventInfo, loadedData));
                }
            }
        }
        return monsters;
    }

#endregion

    #region assets
    public List<string> GetAllAssets()
    {
        List<string> l = new List<string>();
        foreach (var e in sceneEvents) l.AddRange(GetAllAssets(e.behaviours));
        return l;
    }

    public static List<string> GetAllAssets(SceneBehaviour[] behaviours)
    {
        var l = new List<string>();
        if (behaviours == null || behaviours.Length == 0) return l;

        foreach (var b in behaviours)
        {
            if (b.sceneBehaviorType == SceneBehaviouType.CreateMonster)
            {
                l.AddRange(GetMonsterAssets(b));
                //check frame event
                int frameEventId = b.parameters.GetValue<int>(5);
                var fe = ConfigManager.Get<SceneFrameEventInfo>(frameEventId);
                if (fe)
                {
                    if (fe.eventItems == null || fe.eventItems.Length == 0) continue;

                    foreach (var f in fe.eventItems)
                    {
                        l.AddRange(GetAllAssets(f.behaviours));
                    }
                }

                var getReward = b.parameters.GetValue<int>(7) > 0;
                if (getReward) l.Add(Window_RandomReward.ASSET_NAME);
            }
            else if (b.sceneBehaviorType == SceneBehaviouType.CreateSceneActor || b.sceneBehaviorType == SceneBehaviouType.CreateLittle) l.AddRange(GetMonsterAssets(b));
            else if (b.sceneBehaviorType == SceneBehaviouType.AddBuffer) l.AddRange(GetAddBuffAssets(b));
            else if (b.sceneBehaviorType == SceneBehaviouType.CreateTrigger) l.AddRange(GetTriggerBuffAssets(b));
            else if (b.sceneBehaviorType == SceneBehaviouType.StartStoryDialogue) l.AddRange(Module_Story.GetPerStoryPreAssets(b.parameters.GetValue<int>(0), (EnumStoryType)b.parameters.GetValue<int>(1)));
        }
        return l;
    }

    public static List<string> GetMonsterAssets(SceneBehaviour b)
    {
        if (b.sceneBehaviorType != SceneBehaviouType.CreateMonster && b.sceneBehaviorType != SceneBehaviouType.CreateSceneActor
            && b.sceneBehaviorType != SceneBehaviouType.CreateLittle) return new List<string>();

        MonsterInfo info = ConfigManager.Get<MonsterInfo>(b.parameters.GetValue<int>(0));
        var loadAnim = false;
        if (b.sceneBehaviorType == SceneBehaviouType.CreateMonster) loadAnim = b.parameters.GetValue<int>(4) > 0 && !string.IsNullOrEmpty(info.cameraAnimation);
        
        return GetMonsterBaseAssets(info, loadAnim);
    }

    public static List<string> GetMonsterBaseAssets(MonsterInfo mInfo,bool loadCameraAnim)
    {
        List<string> l = new List<string>();
        if (!mInfo) return l;
        
        //怪物模型
        l.AddRange(mInfo.models, true);
        //怪物状态机
        l.Add(Util.Format("animator_weapon_{0}_1", mInfo.montserStateMachineId));
        //怪物初始BUFF
        if (mInfo.initBuffs != null && mInfo.initBuffs.Length > 0)
        {
            for (int j = 0; j < mInfo.initBuffs.Length; j++)
            {
                BuffInfo buff = ConfigManager.Get<BuffInfo>(mInfo.initBuffs[j]);
                if (buff) l.AddRange(buff.GetAllAssets());
            }
        }
        //加载状态机资源
        l.AddRange(StateMachineInfo.GetAllAssets(mInfo.montserStateMachineId, 1));

        //camera animation
        if (loadCameraAnim) l.Add(mInfo.cameraAnimation);

        return l;
    }

    public static List<string> GetAddBuffAssets(SceneBehaviour b)
    {
        List<string> l = new List<string>();
        if (b.sceneBehaviorType != SceneBehaviouType.AddBuffer) return l;

        int buffid = b.parameters.GetValue<int>(1);
        BuffInfo buff = ConfigManager.Get<BuffInfo>(buffid);
        if (buff) l.AddRange(buff.GetAllAssets());
        return l;
    }

    public static List<string> GetTriggerBuffAssets(SceneBehaviour b)
    {
        List<string> l = new List<string>();
        if (b.sceneBehaviorType != SceneBehaviouType.CreateTrigger || string.IsNullOrEmpty(b.strParam)) return l;

        string[] effects = b.strParam.Split(';');
        l.AddRange(effects);
        return l;
    }
    
    #endregion

}//config SceneEventInfos : SceneEventInfo

[Serializable]
public class MonsterInfo : ConfigItem
{
    public static readonly MonsterInfo empty = new MonsterInfo()
    {
        initBuffs = new int[] { }, models = new string[] { }
    };

    public string name;                                                // 怪物昵称
    public string[] models;                                            // 怪物的模型 每一个索引对应了一个形态 参考 CreatureMorphs
    public int gender;                                                 // 怪物性别 
    public string avatar;                                              // 怪物对应的2d头像
    public int avatarBox;                                              // 怪物使用的头像框
    public string info;                                                // 描述
    public Vector2_ colliderSize;                                      // 碰撞盒子大小（半径,高度）
    public Vector2_ hitColliderSize;                                   // 受击盒子大小（半径,高度）
    public double   colliderOffset;                                    // 碰撞盒子 Y 偏移
    public double   hitColliderOffset;                                 // 受击盒子 Y 偏移
    public float scale;                                                // 怪物缩放比例
    public int AttributeConfigId;                                      // 对应的属性配置表的ID
    public int[] initBuffs;                                            // 怪物的初始buff
    public int montserStateMachineId;                                  // 怪物使用的动画状态机配置
    public string bornStateName;                                       // 怪物的出生状态
    public int AIId;                                                   // 怪物的AI表对应的ID
    public double moveSpeed = 42;                                      // 针对怪物的移动速度(绝对值)
    public Vector2 bloodOffset = new Vector2(0,2f);                    // 怪物血条的偏移值
    public Vector2 bloodBarSize = new Vector2(180,16);                 // 怪物血条尺寸
    public bool ignoreTurn;                                            // 忽略AI转身
    public bool deathAppear;                                           // 角色死亡时需要保留模型？ >=1 就会保留
	public EnumMonsterType monsterType = EnumMonsterType.NormalType;   // 怪物的类型
    public string cameraAnimation;                                     // 角色镜头动画
    public EnumLockMonsterType lockType;                               // 被锁敌方式
    public string obstacleMask;                                        // 阻挡类型标记位
    public string ignoreObstacleMask;                                  // 忽略阻挡类型的标记位
    public NpcTypeID npcId;                                            // 如果是npc，需要指定NpcID用于换装

    public string montserAvatar { get; private set; }
    public string montserLargeIcon { get; private set; }

    /// <summary>
    /// 阻挡类型 mask<para></para>
    /// See <see cref="Creature.ignoreObstacleMask"/>
    /// </summary>
    public int iobstacleMask { get; private set; }
    /// <summary>
    /// 忽略阻挡类型 mask<para></para>
    /// See <see cref="Creature.ignoreObstacleMask"/>
    /// </summary>
    public int iignoreObstacleMask { get; private set; }

    public string GetModel(int index = 0)
    {
        return models == null || models.Length < 1 || index >= models.Length ? string.Empty : models[index];
    }

    public override void Initialize()
    {
        montserAvatar = Util.Format("{0}{1}", Module_Avatar.npcAvatarPrefix, avatar);
        montserLargeIcon = Util.Format("{0}{1}", Module_Avatar.npcAvatarPrefixLarge, avatar);

        iobstacleMask = obstacleMask.ToMask();
        iignoreObstacleMask = ignoreObstacleMask.ToMask();
    }

}//config MonsterInfos : MonsterInfo


[Serializable]
public class MonsterAttriubuteInfo : ConfigItem
{
    public static readonly MonsterAttriubuteInfo empty = new MonsterAttriubuteInfo()
    {
        monsterAttriubutes = new MonsterAttriubute[] { },
    };
    
    [Serializable]
    public class MonsterAttriubute
    {
        public int level;               //等级
        public int health;              //血量
        public int attack;              //攻击力
        public int defence;             //防御力
        public double critical;         //暴击率
        public double criticalMul;      //暴伤
        public double resilience;       //韧性
        public int firm;                //铁骨
        public double attackSpeed;      //攻击速度
    }

    public MonsterAttriubute[] monsterAttriubutes;
}//config MonsterAttriubuteInfos : MonsterAttriubuteInfo

[Serializable]
public class AIConfig : ConfigItem
{
    #region custom class
    [Serializable]
    public class LockEnermyInfo
    {
        //怪物与敌对角色的距离
        public int distance;

        /// <summary>
        /// 怪物锁敌概率
        /// </summary>
        public int lockRate;
    }

    [Serializable]
    public class AICondition
    {
        public enum AIConditionType
        {
            /// <summary> 检查怪物血量是否高于阈值 </summary>
            HPHigh,

            /// <summary> 检查怪物血量是否低于阈值 </summary>
            HPLow,

            /// <summary> 检查锁敌目标是否处于攻击状态 </summary>
            CheckAttackState,

            /// <summary> 检查怪物是否有某一类buff </summary>
            HasBuff,

            /// <summary> 检查怪物朝向 0为右，1为左 </summary>
            Direction,

            /// <summary> 检查怪物的移动状态 -1为左移动，0为idle 1为右移动 </summary>
            MoveState,

            /// <summary> 检查怪物是否在玩家的左边 0为不在玩家的左边 1为在左边 </summary>
            MonOnThePlayerLeft,
        }

        public AIConditionType conditionType;

        public int[] paramers;
    }

    [Serializable]
    public class AIBehaviour
    {
        public enum AIBehaviourType
        {
            None,

            CloseTo,        //靠近，只针对runstate

            FarAway,        //远离，只针对runstate

            ToNextGroup,    //切换到下一个分组
        }

        public AIBehaviourType behaviourType;

        /// <summary>
        /// 该行为的状态
        /// </summary>
        public string state;

        /// <summary>
        /// 动作的循环时间（单位毫秒）
        /// </summary>
        public int[] loopDuraction = new int[2] { 0,0};

        /// <summary>
        /// AI的分组信息
        /// </summary>
        public int group;
        
        /// <summary>
        /// 判断是否需要序列化 
        /// </summary>
        /// <returns></returns>
        public bool IsLoop()
        {
            if (loopDuraction == null || loopDuraction.Length < 2)
                return false;

            for (int i = 0; i < loopDuraction.Length; i++)
            {
                if (loopDuraction[i] > 0)
                    return true;
            }

            return false;
        }
    }

    [Serializable]
    public class SingleAIStratergy
    {
        public AICondition[] conditions;

        public AIBehaviour behaviour;

        /// <summary>
        /// 该行为占得权重
        /// </summary>
        public uint rate;

        /// <summary>
        /// 强制按顺序执行
        /// 需要注意，如果该值大于0，则会被强制使用，所有的条件判断都会被忽略
        /// </summary>
        public uint order;

        /// <summary>
        /// 该策略在本次AI中可执行的次数
        /// (-1代表无限制，0代表不能使用，n(n>0)代表当前AI能被使用n次)
        /// </summary>
        public int repeatTimes = -1;

        public uint minRate { get; set; }
        public uint maxRate { get; set; }
        public bool isValidRage(uint value)
        {
            //Logger.LogDetail("{0}的最小范围是{1},最大范围是{2}", behaviour.state,minRate, maxRate);
            return minRate <= value && value < maxRate;
        }
    }

    [Serializable]
    public class AIStratergy
    {
        public int distance;

        public int group;

        public SingleAIStratergy[] stratergies;

        public uint minOrder { get; set; }
        /// <summary>
        /// 设置最大的强制执行序列号
        /// </summary>
        public uint maxOrder { get; set; }

        public bool hasOrderStatergy { get; set; } = false;

        public SingleAIStratergy GetOrderStratergy(uint order)
        {
            foreach (var item in stratergies)
            {
                if (item.order == order) return item;
            }

            return null;
        }
    }

    #endregion

    #region field
    //锁敌信息
    public LockEnermyInfo[] lockEnermyInfos;

    //AI不同距离的不同策略组
    public AIStratergy[] AIStratergies;

    public List<int> distanceKeys { get; private set; } = new List<int>();

    #endregion

    #region functions
    
    public LockEnermyInfo GetLockEnermyInfo(int distance)
    {
        for (int i = 0, length = lockEnermyInfos.Length; i < length; i++)
        {
            if (distance <= lockEnermyInfos[i].distance)
                return lockEnermyInfos[i];
        }

        return null;
    }

    public SingleAIStratergy[] GetSingleAIStratergies(int distance,int group)
    {
        AIStratergy a = GetAIStratergy(distance,group);
        return a?.stratergies;
    }

    public AIStratergy GetAIStratergy(int distance, int group)
    {
        //has valid group
        foreach (var item in AIStratergies)
        {
            if (distance <= item.distance && item.group == group) return item;
        }

        //current group is invalid,then we only check distance and return whitch is valid first
        foreach (var item in AIStratergies)
        {
            if (distance <= item.distance) return item;
        }

        return null;
    }

    public int GetRealDistanceKey(float realDis)
    {
        foreach (var item in distanceKeys)
        {
            if (realDis <= item) return item;
        }

        return int.MaxValue;
    }

    public override void Initialize()
    {
        Array.Sort(AIStratergies, (a, b) => a.distance < b.distance ? -1 : 1);
        foreach (var item in AIStratergies)
        {
            //load valid key
            if (!distanceKeys.Contains(item.distance)) distanceKeys.Add(item.distance);

            //set max/min order
            foreach (var s in item.stratergies)
            {
                if (s.order > item.maxOrder) item.maxOrder = s.order;
                if (s.order < item.minOrder) item.minOrder = s.order;
                if (!item.hasOrderStatergy) item.hasOrderStatergy = item.maxOrder > 0;
            }
        }
        distanceKeys.Sort((a, b) => a < b ? -1 : 1);
    }
    #endregion

}//config AIConfigs : AIConfig

public enum TeamType
{
    Single,
    Double,
    Any,
}

[Serializable]
public class TaskInfo : ConfigItem
{
    public static int TASK_CONDITION_DESCRIPTION_ID = 300;

    #region 星级条件判断
    [Serializable]
    public class TaskStarDetail
    {
        public int star;                //星级

        public TaskStarCondition[] conditions;
        public TaskStarReward reward;   //预览的奖励

        public string GetStarConditionDesc()
        {
            var c = GetDefalutStarCondition();
            if (c == null) return string.Empty;
            return GetStarDescription(c.type, c.value);
        }

        public TaskStarCondition GetDefalutStarCondition()
        {
            if (conditions == null || conditions.Length < 1) return null;

            return conditions[0];
        }
    }

    [Serializable]
    public class TaskStarCondition
    {
        public EnumPVEDataType type;    //星级条件判断类型
        public int value;               //星级条件判断的数值
    }

    [Serializable]
    public class TaskStarReward
    {
        public int coin;
        public int diamond;
        public int activePoint;
        public int expr;
        public int fatigue;
        public TaskStarProp[] props;

        public List<ItemPair> ToItemPairList()
        {
            var list = new List<ItemPair>();
            if (coin > 0)
                list.Add(new ItemPair {itemId = 1, count = coin});
            if (diamond > 0)
                list.Add(new ItemPair {itemId = 2, count = diamond});
            if (activePoint > 0)
                list.Add(new ItemPair {itemId = PropItemInfo.activepoint.ID, count = activePoint});
            if(expr > 0)
                list.Add(new ItemPair { itemId = PropItemInfo.exp.ID, count = expr });
            if (fatigue > 0)
                list.Add(new ItemPair { itemId = 15, count = fatigue });

            for (var i = 0; i < props.Length; i++)
            {
                list.Add(new ItemPair {itemId = props[i].propId, count = props[i].num});
            }
            return list;
        }
    }

    /// <summary>
    /// 预览奖励的道具结构，等同于PItem2
    /// </summary>
    [Serializable]
    public class TaskStarProp
    {
        public int propId;
        public int level;
        public int star;
        public int num;
    }
    #endregion

    public string name { get { return ConfigText.GetDefalutString(nameId); } }
    public int nameId;                                                // 任务名称ID
    public string icon;                                               // 任务图标
    public int difficult;                                             // 任务难度
    public int descId;                                                // 描述id,对应configtext 条件描述ID
    public int stageId;                                               // 关联的场景PVE场景ID
    public int level;                                                 //任务等阶
    public int isDefault;                                             //是否是默认
    public int isSpecial;                                             //是否是默认任务
    public int fatigueCount;                                          //体力值
    public int isRare;                                                //是否是困难关卡 0不是 1是
    public string[] cost;                                             //格式为{0}-{1} 0是type 1是数量
    public int dayRemainCount;                                        //当日重置的次数
    public int challengeCount;                                        //困难关卡的挑战上限次数
    public int unlockLv;                                              //开放等级
    public bool limitEnter;                                           //限制进入。为1时次数不足无法进入。为0可以进入不消耗体力也无法获得奖励
    public TeamType teamType;                                         //组队类型
    public int[] bossID;                                              //boss的id 
    public int[] dependId;                                            //依赖任务ID
    public int maxChanllengeTimes;                                    //该关卡最大挑战次数 <=0 表示无限制
    public TaskStarDetail[] taskStarDetails;                          //星级奖励判断条件
    [System.NonSerialized]
    public ItemPair[] costItem;                                       //进入花费
    public string bgIcon;                                             //关卡背景图
    public string checkBoxIcon;                                       //选择关卡的背景图
    public int recommend;                                             //推荐战力

    public bool isSpecialTask { get { return isSpecial > 0; } }
    public bool isGaidenTask { get { return level >= (int)TaskType.GaidenChapterOne && level <= (int)TaskType.GaidenChapterSix; } }
    public string desc { get { return ConfigText.GetDefalutString(descId); } }

    public override void Initialize()
    {
        base.Initialize();
        List<ItemPair> list = new List<ItemPair>();
        for (int i = 0; i < cost.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(cost[i]))
                continue;
            string[] str = cost[i].Split('-');
            if (str.Length == 2)
                list.Add(new ItemPair() {itemId = Util.Parse<int>(str[0]), count = Util.Parse<int>(str[1])});
            else
                Logger.LogError("任务消耗配置格式错误。任务id = {0}", ID);
        }
        costItem = list.ToArray();

        List<TaskStarDetail> detail = new List<TaskStarDetail>();
        for (var i = 0; i < taskStarDetails.Length; i++)
        {
            var conditions = taskStarDetails[i]?.conditions;
            if (conditions == null || conditions.Length == 0)
                continue;
            var flag = true;
            for (var m = conditions.Length - 1; m >= 0; m--)
            {
                if (conditions[m].type == EnumPVEDataType.Invalid)
                {
                    flag = false;
                    break;
                }
            }
            if (!flag)
                continue;
            detail.Add(taskStarDetails[i]);
        }
        taskStarDetails = detail.ToArray();
    }

    /// <summary>
    /// 获取星级详细描述，star = [1,3]
    /// </summary>
    /// <param name="star"></param>
    /// <returns></returns>
    public TaskStarDetail GetStarDetail(int star)
    {
        if (taskStarDetails == null || star < 1 || star > taskStarDetails.Length) return null;

        return taskStarDetails[star - 1];
    }

    public static string GetStarDescription(EnumPVEDataType type,int value)
    {
        return GetStarDescription((int)type, value);
    }

    public static string GetStarDescription(int type, int value)
    {
        return Util.Format(ConfigText.GetDefalutString(TASK_CONDITION_DESCRIPTION_ID, type), value);
    }
}//config TaskInfos : TaskInfo

[Serializable]
public class ActiveTaskInfo : ConfigItem
{
    public int activeName;           //活动名称
    public int type;                 //0-活动任务。 1-觉醒关卡
    public int taskLv;               //对应的task等级
    public int activeDesc;           //活动描述
    public string activeIcon;        //活动图片
    public string activeSubIcon;     //活动图片子图片
    public Color[] activeTextColor;  //文字背景框的icon
    public int[] dropProps;          //掉落物品
    public int openLv;               //开放等级
    public int crossLimit;           //通关限次
    public int[] openWeek;           //开放时间(星期)
    public int openTimeDesc;         //开放时间描述
    public int startTime;            //开放时间(日期)
    public int endTime;              //结束时间
}//config ActiveTaskInfos : ActiveTaskInfo

[Serializable]
public class GaidenTaskInfo : ConfigItem
{
    public int gaidenNameId;        //活动名称
    public TaskType taskType;       //对应的taskinfo.taskLevel
    public int descId;              //活动描述
    public string icon;             //活动图片
    public string bgIcon;           //活动名称的底板的名字
    public int openLv;              //开放等级
    public int order;               //显示排序
    public string[] lampstand;       //底座图片
    public Color[] nameColor;         //[名字字体颜色,描边颜色]
    public Color descColor;         //描述颜色
}//config GaidenTaskInfos : GaidenTaskInfo

[Serializable]
public class LabyrinthInfo : ConfigItem
{
    [Serializable]
    public class LabyrinthReward
    {
        public int propId;      //道具ID
        public float rate;      //获得几率
    }

    public string labyrinthName { get { return ConfigText.GetDefalutString(labyrinthNameId); } }

    public int labyrinthNameId;
    public string icon;
    public int levelId;                         //对应的关卡ID

    public string trigger;                      //场景中迷宫触发器的名称（必须在场景中保证该名称是唯一的）
    public Vector3 triggerSize;                 //触发器的尺寸
    public Vector3 triggerOffset;               //触发器的偏移
    public string activeEffect;                 //触发器激活时的特效
    public string inactiveEffect;               //触发器关闭时的特效

    public LabyrinthReward[] rewards;             //迷宫中可获得的奖励列表(对应道具ID)
}//config LabyrinthInfos : LabyrinthInfo

[Serializable]
public class ChaseLevelStarInfo : ConfigItem
{
    public int level;           //对应不同等阶的任务
    public int isRare;          //是否是精英关卡,0不是 1是
    public int starCount;       //满足星星的数量
    public string[] rewards;    //奖励

    public bool isRareTask { get { return isRare > 0; } }
}//config ChaseLevelStarInfos : ChaseLevelStarInfo
#endregion

#region 剧情模块

[Serializable]
public class StoryInfo : ConfigItem
{
    [Serializable]
    public class TalkingRoleData
    {
        /// <summary>
        /// 该ID对两种对话模式均有效，所以需要根据模式决定到底从Monster表还是Npc表获取数据
        /// </summary>
        public int roleId;
        public int rolePos;
        public int highLight;

        public bool isHighlight { get { return highLight > 0; } }

        public void SetData(List<string> data)
        {
            roleId = 0;
            rolePos = 0;

            if (data.Count > 0) int.TryParse(data[0], out roleId);
            if (data.Count > 1) int.TryParse(data[1], out rolePos);
        }
    }

    [Serializable]
    public class TalkingRoleState
    {
        /// <summary>
        /// 该ID对两种对话模式均有效，所以需要根据模式决定到底从Monster表还是Npc表获取数据
        /// </summary>
        public int roleId;
        public string state;

        public void SetData(List<string> data)
        {
            if (data.Count > 0) roleId  = Util.Parse<int>(data[0]); ;
            if (data.Count > 1) state   = data[1];
        }
    }

    [Serializable]
    public class BlackScreenData
    {
        public int isBlackScreen;                           //当前对话是否显示黑屏,1表示显示黑屏，-1表示取消黑屏
        public int imme;                                    //是否立即显示黑屏或者取消显示黑屏 0,表示需要淡入淡出动画，其余则表示直接显示/隐藏黑屏

        public bool blackScreen { get { return isBlackScreen > 0; } }
        public bool cancelBlack { get { return isBlackScreen < 0; } }

        public bool immediately { get { return imme != 0; } }

        public void SetData(List<string> data)
        {
            if (data.Count > 0) isBlackScreen    = Util.Parse<int>(data[0]);
            if (data.Count > 1) imme             = Util.Parse<int>(data[1]);
        }
    }

    [Serializable]
    public class CameraShakeData
    {
        public int delayTime;
        public int shakeId;

        public void SetData(List<string> data)
        {
            if (data.Count > 0) delayTime   = Util.Parse<int>(data[0]);
            if (data.Count > 1) shakeId     = Util.Parse<int>(data[1]);
        }
    }

    [Serializable]
    public class StorySoundEffect
    {
        //音效延迟时间
        public int delayTime;
        public string soundName;

        public void SetData(List<string> data)
        {
            if (data.Count > 0) delayTime = Util.Parse<int>(data[0]);
            if (data.Count > 1) soundName = data[1];


            if (data.Count < 2)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in data) sb.AppendFormat("{0}\t",item);
                Logger.LogWarning("load sound name error,,,sound data is {0}---format is like [ 1000,soundname1;2000,soundname2 ]", sb.ToString());
            }
        }
    }

    [Serializable]
    public class MusicData
    {
        public string musicName;
        public int loop;

        public bool validMusic { get { return !string.IsNullOrEmpty(musicName); } }
        public bool loopMusic { get { return loop > 0; } }

        public void SetData(List<string> data)
        {
            if (data.Count > 0) musicName = data[0];

            //默认为循环
            if (data.Count > 1) loop = Util.Parse<int>(data[1]);
            else loop = 1;
        }
    }

    [Serializable]
    public class GivePropData
    {
        public ushort itemTypeId;
        public byte level;
        public byte star;
        public uint num;

        public void SetData(List<string> data)
        {
            if (data.Count == 2) ParseProp(data);
            else if (data.Count == 4) ParseRune(data);
        }

        private void ParseProp(List<string> data)
        {
            if (data.Count > 0) itemTypeId  = Util.Parse<ushort>(data[0]);
            if (data.Count > 1) num         = Util.Parse<uint>(data[1]);
        }

        private void ParseRune(List<string> data)
        {
            if (data.Count > 0) itemTypeId  = Util.Parse<ushort>(data[0]);
            if (data.Count > 1) level       = Util.Parse<byte>(data[1]);
            if (data.Count > 2) star        = Util.Parse<byte>(data[2]);
            if (data.Count > 3) num         = Util.Parse<uint>(data[3]);
        }

        public PItem2 GetPItem2()
        {
            PItem2 p        = PacketObject.Create<PItem2>();
            p.itemTypeId    = itemTypeId;
            p.level         = level;
            p.star          = star;
            p.num           = num;
            return p;
        }
    }

    [Serializable]
    public class ModelData 
    {
        public string model;

        /// <summary>
        /// 对应showCreatureInfo 中ID = 1001的配置
        /// </summary>
        public int positionIndex;

        public void SetData(List<string> data)
        {
            model = data.GetValue(0);
            positionIndex = Util.Parse<int>(data.GetValue(1));
        }
    }

    [Serializable]
    public class StoryItem
    {
        public string background;                           //背景
        public TalkingRoleData[] talkingRoles;              //说话的对象，根据对话类型决定是monster还是npc，受到延时影响
        public int showHead;                                //是否显示的头像(战斗剧情专属)
        public string content;                              //文字内容，受到延时影响

        //当前说话对象需要切换的状态,根据对话类型决定是monster还是npc 
        //(针对同一个角色，只有当上一段对话和下一段对话配置的状态相同时，才可以继续播放该状态，否则状态会被打断，切换到新的状态或者回到idle)
        public TalkingRoleState[] talkingRoleStates;        

        public int cameraLockRoleId;                        //镜头锁定的角色ID，只对战斗剧情有效 (配置为-10000表示结束当前的异常镜头，将镜头移动回到主角身上)
        public int turnLockRoleId;                          //需要锁定的角色ID（可能会转身），只对战斗剧情有效
        public BlackScreenData blackData;                   //黑屏数据
        public CameraShakeData cameraShake;                 //摄像机震动数据
        public int forceTime;                               //强制对话时间
        public int contentDelayTime;                        //文字对话出现的延迟时间
        public string effect;                               //对话出现时屏幕中央播放的特效
        public MusicData musicData;                         //当前背景音乐 （如果填0，表示停止当前的场景背景音乐）
        public StorySoundEffect[] soundEffect;              //当前播放的音效
        public string voiceName;                            //当前对话的语音，受到延时影响
        public string openWindow;                           //当前对话结束后需要打开的窗口，特别的，只有该窗口关闭后才能跳转到下一句对话，只针对剧场对话
        public string replaceName;                          //当前模型的替换显示昵称
        public GivePropData[] giveDatas;                    //当前对话需要赠送的道具(只针对剧场动画)

        //新增
        public int cameraPosIndex;                          //摄像机的特殊镜头展示
        public string contentBg;                            //需要替换的文字背景框
        public string playerIcon;                           //需要显示的玩家表情贴图
        public int answerId;                                //需要打开的答案ID,跟策划约定后，该ID 只会在对话结束之前触发一次，无论配置到哪句话
        public ModelData[] models;                          //需要加载的3D模型
        
        public Color maskColor;                             //遮罩颜色

        #region properties
        public bool canChangeNextDialog { get { return string.IsNullOrEmpty(openWindow); } }

        public bool needReplaceName { get { return !string.IsNullOrEmpty(replaceName); } }

        public bool canPlayEffect { get { return !string.IsNullOrEmpty(effect); } }

        public bool showHeadIcon { get { return showHead > 0; } }

        public string playerIconDetail {
            get {
                return playerIcon;
            } }
        #endregion

        #region functions

        public bool IsNeedChangeState(int roleId)
        {
            if (talkingRoleStates == null || talkingRoleStates.Length == 0)
                return false;

            for (int i = 0; i < talkingRoleStates.Length; i++)
            {
                if (talkingRoleStates[i].roleId == roleId) return !string.IsNullOrEmpty(talkingRoleStates[i].state);
            }

            return false;
        }

        #endregion

        #region load config
        public void LoadRoleData(string msg)
        {
            List<List<string>> datas = GetDatas(msg);

            talkingRoles = new TalkingRoleData[datas.Count];
            for (int i = 0; i < talkingRoles.Length; i++)
            {
                talkingRoles[i] = new TalkingRoleData();
                talkingRoles[i].SetData(datas[i]);
            }
        }

        /// <summary>
        /// call this function must after LoadRoleData
        /// </summary>
        /// <param name="msg"></param>
        public void LoadRoleHighLight(string msg)
        {
            List<List<string>> datas = GetDatas(msg);
            Dictionary<int, int> highDic = new Dictionary<int, int>();
            foreach (var item in datas)
            {
                int roleId = Util.Parse<int>(item.GetValue(0));
                int h = Util.Parse<int>(item.GetValue(1));

                if(highDic.ContainsKey(roleId))
                {
                    Logger.LogError("high light role id is repeat, role Id is {0}",roleId);
                    continue;
                }
                highDic.Add(roleId,h);
            }
            
            if(talkingRoles == null)
            {
                Logger.LogError("we dont have the avalid talkingRoles data,please check out");
                return;
            }

            int high = 0;
            for (int i = 0; i < talkingRoles.Length; i++)
            {
                high = highDic.Get(talkingRoles[i].roleId);
                talkingRoles[i].highLight = high > 0 ? 1 : 0;
            }
        }

        public void LoadRoleState(string msg)
        {
            List<List<string>> datas = GetDatas(msg);

            talkingRoleStates = new TalkingRoleState[datas.Count];
            for (int i = 0; i < talkingRoleStates.Length; i++)
            {
                talkingRoleStates[i] = new TalkingRoleState();
                talkingRoleStates[i].SetData(datas[i]);
            }
        }

        public void LoadBlackData(string msg)
        {
            List<List<string>> datas = GetDatas(msg);

            blackData = new BlackScreenData();
            if (datas.Count > 0) blackData.SetData(datas[0]);
        }

        public void LoadCameraShake(string msg)
        {
            List<List<string>> datas = GetDatas(msg);

            cameraShake = new CameraShakeData();
            if (datas.Count > 0) cameraShake.SetData(datas[0]);
        }

        public void LoadStorySounds(string msg)
        {
            List<List<string>> datas = GetDatas(msg);

            soundEffect = new StorySoundEffect[datas.Count];
            for (int i = 0; i < soundEffect.Length; i++)
            {
                soundEffect[i] = new StorySoundEffect();
                soundEffect[i].SetData(datas[i]);
            }
        }

        public void LoadMusicData(string msg)
        {
            List<List<string>> datas = GetDatas(msg);

            musicData = new MusicData();
            if (datas.Count > 0) musicData.SetData(datas[0]);
        }

        public void LoadGiveDatas(string msg)
        {
            List<List<string>> datas = GetDatas(msg);

            giveDatas = new GivePropData[datas.Count];
            for (int i = 0; i < giveDatas.Length; i++)
            {
                giveDatas[i] = new GivePropData();
                giveDatas[i].SetData(datas[i]);
            }
        }

        public void LoadModelDatas(string msg)
        {
            List<List<string>> datas = GetDatas(msg);

            models = new ModelData[datas.Count];
            for (int i = 0; i < models.Length; i++)
            {
                models[i] = new ModelData();
                models[i].SetData(datas[i]);
            }
        }
        
        public void SetTheatryMaskColor(string msg,int stroyId)
        {
            var cs = msg.Split(',');
            if (cs.Length != 3) Logger.LogError("color config is wrong,config is {0} with story id {1}", msg, stroyId);
            float[] cf = new float[3];
            for (int i = 0; i < cf.Length; i++)
            {
                cf[i] = Util.Parse<float>(cs.GetValue<string>(i));
            }
            maskColor = new Color(cf[0] / 255f, cf[1] / 255f, cf[2] / 255f);
        }

        public List<List<string>> GetDatas(string msg)
        {
            List<List<string>> list = new List<List<string>>();
            if (string.IsNullOrEmpty(msg)) return list;

            string[] datas = msg.Split(';');
            foreach (var item in datas)
            {
                if (string.IsNullOrEmpty(item)) continue;

                List<string> dataList = new List<string>(); 
                string[] data = item.Split(',');
                foreach (var d in data)
                {
                    if (!string.IsNullOrEmpty(d)) dataList.Add(d);
                }
                list.Add(dataList);
            }

            return list;
        }

        #endregion
    }

    public StoryItem[] storyItems;

    public bool NeedChangeBackground()
    {
        if (storyItems == null || storyItems.Length == 0)
            return false;

        for (int i = 0; i < storyItems.Length; i++)
        {
            if (!string.IsNullOrEmpty(storyItems[i].background)) return true;
        }

        return false;
    }

    public bool NeedChangeHead()
    {
        if (storyItems == null || storyItems.Length == 0)
            return false;

        for (int i = 0; i < storyItems.Length; i++)
        {
            if (storyItems[i].showHead > 0) return true;
        }

        return false;
    }
    //是否允许跳过该剧情
    public bool IsSkipStory()
    {
        if (storyItems == null || storyItems.Length == 0)
            return true;

        for (int i = 0; i < storyItems.Length; i++)
        {
            if (!string.IsNullOrEmpty(storyItems[i].openWindow)) return false;
        }
        return true;
    }
    //获取赠送道具列表
    public List<List<StoryInfo.GivePropData>> GetGivePropDatas()
    {
        List<List<StoryInfo.GivePropData>> mGivePropData = new List<List<GivePropData>>();
        if (storyItems == null || storyItems.Length == 0)
            return null;
        for (int i = 0; i < storyItems.Length; i++)
        {
            if (storyItems[i].giveDatas != null)
            {
                List<GivePropData> mdata = new List<GivePropData>();
                for(int col = 0;col<storyItems[i].giveDatas.Length;col++)
                {
                    mdata.Add(storyItems[i].giveDatas[col]);
                }
                mGivePropData.Add(mdata);
            }
        }
        return mGivePropData;
    }
    public override void Initialize()
    {
        base.Initialize();
        foreach (var item in storyItems) item.content = item.content.Trim();
    }
}//config StoryInfos : StoryInfo

#endregion

#region 新手引导

[Serializable]
public class GuideInfo : ConfigItem
{
    #region class define
    
    [Serializable]
    public class GuideSuccessCondition
    {
        public EnumGuideCondition type;
        public int[] intParams;
        public float[] floatParams;

        public Vector3 tipPos;
    }

    [Serializable]
    public class GuideIcon
    {
        public string icon;
        public Vector3 position;
    }

    [Serializable]
    public class HotAreaData
    {
        //热点区域的窗口名称(约束的窗口名称，如果该窗口并未开启，那该步新手引导也不会执行 )
        public string hotWindow;
        //热点区域的名称，该选项会自动挑选滑动面板第一个item显示
        public string[] hotArea;
        //热点区域条件约束
        public EnumGuideRestrain restrainType;
        //约束的ID
        public int[] restrainParames;
        //当前组件的子组件(用于响应事件以及特效播放)
        public string restrainChild;
        //热点区域的效果(策划配置的例子效果)
        public string effect;
        //使用遮罩作为点击事件(仅仅将该步作为提示引导)
        public int tipHotArea;
        //优先读取职业的限定热点区域
        public string[] protoArea;

        public bool hasRestrainCondition { get { return restrainType != EnumGuideRestrain.None; } }

        public bool validRuneCondition { get { return restrainParames.Length >= 3; } }

        public int restrainId { get { return restrainParames != null && restrainParames.Length > 0 ? restrainParames[0] : 0; } }

        public bool hasEffect { get { return !string.IsNullOrEmpty(effect); } }
        public bool isTipHotArea { get { return tipHotArea > 0; } }

        public bool hasRestrainChild { get { return !string.IsNullOrEmpty(restrainChild); } }

        /// <summary>
        /// 获取职业指定的ID
        /// </summary>
        /// <param name="protoId"></param>
        /// <returns></returns>
        public int GetProtoCheckId(byte? protoId)
        {
            if (restrainParames == null || restrainParames.Length == 0) return 0;
            int idx = protoId == null ? 0 : (int)protoId - 1;
            return restrainParames.GetValue<int>(idx);
        }

        public bool canFindByCheckID { get { return restrainType == EnumGuideRestrain.CheckID || restrainType == EnumGuideRestrain.CurrentWeapon || restrainType == EnumGuideRestrain.ProtoID; } }
    }

    [Serializable]
    public class GuideItem
    {
        public EnumGuideType type;                          //新手引导类型 

        public int contentId;                               //文本内容的ID
        public Vector3 contentPos;                          //文本内容的位置 
        public HotAreaData hotAreaData;                     //热点区域相关
        public string beginAnim;                            //该引导最开始的时候需要播放的动画（一般在主界面才会播放激活按钮动画，播放动画时蒙版需要透明）
        public GuideIcon iconInfo;                          //需要显示的新手引导图片(eg.提示玩家操作)
        public GuideSuccessCondition successCondition;      //需要完成指定条件后才能跳转
        public int maskFade;                                //遮罩是否隐藏
        public int end;                                     //是否是新手引导的结束，结束后不会再次进入该引导
        public int[] unlockFunctionId;                      //每次新手引导前需要解锁的功能
        public string unlockAudio;                          //解锁音频
        public int pauseCountDown;                          //暂停倒计时（单位毫秒）,假设设定为t毫秒，则0-t毫秒不能进行点击操作，t毫秒后暂停（timescale = 0），最后点击成功后恢复timescale
        public int isBreak;                                 //当前引导是否可以被其他引导顶掉(一般来说只能是提示性的引导才会被顶掉，强制引导根本没机会触发其他的引导)
        public float protectTime;                           //当前引导的保护时间  0 表示使用 GeneralConfigInfo.sguideInvalidTime 否则使用此配置

        //对话类型字段
        public int dialogId;                                 //需要触发的对话ID

        //幕间功能字段
        public int titleId;                              //幕间标题图片
        public string audio;                                //音频文件
        
        public string content { get { return ConfigText.GetDefalutString(contentId); } }
        public bool isEnd { get { return end > 0; } }
        public bool canBeBreaked { get { return isBreak > 0; } }
        public bool isMaskFade { get { return maskFade > 0; } }

        public bool hasCondition { get { return successCondition != null && successCondition.type != EnumGuideCondition.None; } }
        public bool hasHotArea { get { return hotAreaData != null && hotAreaData.hotArea != null && hotAreaData.hotArea.Length > 0;} }
        public bool hasProtoArea { get { return hotAreaData != null && hotAreaData.protoArea != null && hotAreaData.protoArea.Length > 0; } }
        public string defalutHotArea
        {
            get
            {
                if (hasHotArea)
                {
                    for (int i = hotAreaData.hotArea.Length - 1; i >= 0; i--)
                    {
                        if (!string.IsNullOrEmpty(hotAreaData.hotArea[i])) return hotAreaData.hotArea[i];
                    }
                }
                return string.Empty;
            }
        }
        public string hotWindow { get { return hotAreaData == null ? string.Empty : hotAreaData.hotWindow; } }
        //public bool checkHomeSecondPanel { get { return checkSecondPanel > 0; } }
        public bool hasUnlockFuncs { get { return unlockFunctionId != null && unlockFunctionId.Length > 0; } }
        public bool playUnlockAudio { get { return !string.IsNullOrEmpty(unlockAudio); } }

        /// <summary>
        /// 获取职业指定的路径
        /// </summary>
        /// <param name="protoId">职业ID</param>
        /// <returns></returns>
        public string GetProtoArea(int protoId)
        {
            if(hasProtoArea && protoId > 0)
            {
                int index = protoId - 1;
                return hotAreaData.protoArea.GetValue<string>(index);
            }
            return string.Empty;
        }
    }

    [Serializable]
    public class GuideConfigCondition
    {
        public EnumGuideContitionType type;
        public int[] intParames;
        public string strParames;
    }
    
    #endregion

    public GuideConfigCondition[] conditions;
    public GuideItem[] guideItems;
    
    //重复检查 
    public int repeat;
    //互斥的引导ID,只针对重复新手引导
    public int mutexId;
    //跳过重连检测
    public int skipReconnection;
    //表示重复引导
    public bool repeatGuide { get { return repeat > 0&&repeat < 2; } }
    //表示只有在重连的时候，进行该引导检查
    public bool ReconnectCheckGuide { get { return repeat == 2; } }
    public bool skipReconnect { get { return skipReconnection > 0; } }

    public GuideItem GetGuideItem(int index)
    {
        return index < 0 || index >= guideItems.Length ? null : guideItems[index];
    }

    public GuideItem GetDefalutGuideItem(bool includeDialogItem = false)
    {
        GuideItem guide = null;
        foreach (var item in guideItems)
        {
            if (includeDialogItem && item.type == EnumGuideType.Dialog) guide = item;
            if (!includeDialogItem && item.type != EnumGuideType.Dialog) guide = item; 
            if (guide != null) break;
        }
        return guide;
    }

    /// <summary>
    /// 1 for normal_guide,2 for tip_guide, 3 for all
    /// </summary>
    /// <returns></returns>
    public int GetAllGuideItem()
    {
        int result = 0;
        foreach (var item in guideItems)
        {
            if (item.type == EnumGuideType.NormalGuide) result = result | 1;
            if (item.type == EnumGuideType.TipGuide)    result = result | 2;
        }
        return result;
    }
}//config GuideInfos : GuideInfo

[Serializable]
public class UnlockFunctionInfo : ConfigItem
{
    public string windowName;                       //界面约束
    public string prefixName;                       //前缀（避免同名按钮）
    public string buttonName;                       //按钮路径
    public EnumUnlockTweenType unlockTweenType;     //解锁动画类型
    public int unlockNote;                       //解锁文本UI显示
    public int unlockNoteTip;                    //未解锁提示文本
}//config UnlockFunctionInfos : UnlockFunctionInfo

#endregion

[Serializable]
public class TipInfo : ConfigItem
{
    public enum TipType
    {
        Diamond,

        Token,

        Gold,
    }

    public static readonly TipInfo empty = new TipInfo()
    {
        tipType = TipType.Diamond,
        color = new float[4],
    };

    public string icon;
    public string msgFormat;
    public float[] color;
    public TipType tipType;
}//config TipInfos : TipInfo

/// <summary>
/// 道具信息
/// 道具ID对应消息协议中的itemTypeId
/// </summary>
[Serializable]
public class PropItemInfo : ConfigItem, IExp
{
    #region Static functions

    public static PropItemInfo Get(ushort _itemTypeId)
    {
        if (_itemTypeId == 14) return activepoint;
        else if (_itemTypeId == 13) return exp;

        var info = ConfigManager.Get<PropItemInfo>(_itemTypeId);
        return info;
    }

    public static readonly PropItemInfo empty = new PropItemInfo() { ID = 0, itemNameId = 0, itemName = string.Empty, itemType = PropType.None, subType = 0, stackNum = 0, icon = string.Empty, quality = 1};
    public static PropItemInfo exp
    {
        get
        {
            if (!m_exp)
            {
                m_exp = new PropItemInfo() { ID = 13, itemNameId = 540, itemName = string.Empty, itemType = PropType.Sundries, icon = string.Empty, quality = 1, desc = 253 };
                m_exp.Initialize();
            }
            return m_exp;
        }
    }
    public static PropItemInfo activepoint
    {
        get
        {
            if (!m_activepoint)
            {
                m_activepoint = new PropItemInfo() { ID = 14, itemNameId = 542, itemName = string.Empty, itemType = PropType.Sundries, icon = "active_activeicon", quality = 1, desc = 254 };
                m_activepoint.Initialize();
            }
            return m_activepoint;
        }

    }

    public static PropItemInfo FriendPointProp
    {
        get
        {
            if (m_friendPoint == null)
                m_friendPoint = ConfigManager.Get<PropItemInfo>(6);
            return m_friendPoint;
        }
    }

    private static PropItemInfo m_exp = null, m_activepoint = null, m_friendPoint;

    #endregion

    #region interface
    /// <summary>
    /// 计算的经验总额
    /// </summary>
    public uint gainExp { get { return swallowedExpr; } set { } }
    public uint ownNum { get; set; } = 0;
    public int useNum { get; set; } = 0;
    #endregion

    /// <summary>
    /// 道具名称
    /// </summary>
    public string itemName { get; private set; }
    /// <summary>
    /// 是否有职业限制
    /// 默认所有物品都没有职业限制，再 proto 中配置了 All 等效于默认
    /// </summary>
    public bool hasClassLimit { get; private set; }
    /// <summary>
    /// 职业限制
    /// 1,2,3
    /// </summary>
    public string classLimit { get; private set; } = string.Empty;
    /// <summary>
    /// 道具名称ID
    /// </summary>
    public int itemNameId;
    /// <summary>
    ///  道具类型 1为武器、2为枪械、3为时装、4为符文、5为货币、6为可使用道具、7为杂物，后续有其他类型再添加
    ///</summary>
    public PropType itemType;
    /// <summary>
    ///  itemType细分，例如武器类Type 1，里面还要分 1: 长剑、 2:武士刀、 3:长枪、4:大斧、5:拳套等等；符文也要分位置1、2、3、4、5、6
    ///</summary>
    public int subType;
    /// <summary>
    ///  道具图标
    ///</summary>
    public string icon;
    /// <summary>
    ///  是男性道具: 1, 女性道具:0, 通用: 2
    ///</summary>
    public byte sex;
    /// <summary>
    ///  武器系数
    ///</summary>
    public float powerCovertRate;
    /// <summary>
    ///  堆叠数量, 0表示不限制
    ///</summary>
    public ushort stackNum;
    /// <summary>
    ///  是否是套装,  如果是套装, 则为套装的id, 否则为0
    ///</summary>
    public ushort suite;
    /// <summary>
    ///  物品品阶属性，1为白、2为绿、3为篮、4为紫、5为金、6为橙
    ///</summary>
    public int quality;
    /// <summary>
    ///  出售的货币类型, 1:金币; 2: 钻石;  3: 赏金猎人徽记
    ///</summary>
    public int costType;
    /// <summary>
    ///  出售的基础价格
    ///</summary>
    public uint itemCost;
    /// <summary>
    ///  时间限制
    ///</summary>
    public uint timeLimit;
    /// <summary>
    ///  吞噬经验
    ///</summary>
    public uint swallowedExpr;
    /// <summary>
    /// npc道具经验 
    /// </summary>
    public string npcPropExp;
    /// <summary>
    ///  描述id,对应configtext
    ///</summary>
    public int desc;
    /// <summary>
    ///  模型
    ///</summary>
    public string[] mesh;
    /// <summary>
    /// 预览模型
    /// 主要用于装备的预览
    /// </summary>
    public string previewModel;
    /// <summary>
    /// 道具镜头控制的ID
    /// </summary>
    public int cameraControlId;
    /// <summary>
    /// 自带的属性
    /// </summary>
    public ItemAttachAttr[] attributes;
    /// <summary>
    /// 是否可合成 0不是 1是
    /// </summary>
    public int compose;
    /// <summary>
    /// 职业限制
    /// </summary>
    public CreatureVocationType[] proto;

    public ItemPair[] previewItems;
    /// <summary>
    /// 是否可分解
    /// </summary>
    public int decompose;
    /// <summary>
    /// 是否可升华
    /// </summary>
    public bool sublimation;
    /// <summary>
    /// 是否可附灵
    /// </summary>
    public bool soulable;
    /// <summary>
    /// 仓库标签
    /// </summary>
    public int lableType;
    /// <summary>
    /// 掉落界面
    /// </summary>
    public string[] dropType;
    /// <summary>
    /// 购买等级限制(材料)
    /// </summary>
    public int buyLevel;

    public int diamonds;
    /// <summary>
    /// 可悬赏数量
    /// </summary>
    public int rewardnum;
    /// <summary>
    /// 工会贡献点
    /// </summary>
    public int contribution;

    public override void Initialize()
    {
        itemName = ConfigText.GetDefalutString(itemNameId);

        if (mesh == null) mesh = new string[] { };
        for (var i = 0; i < mesh.Length; ++i) mesh[i] = mesh[i].ToLower();

        if (attributes == null) attributes = new ItemAttachAttr[] { };
        if (previewItems == null) previewItems = new ItemPair[] { };
        if (dropType == null) dropType = new string[] { };

        classLimit = "";
        if (proto == null) proto = new CreatureVocationType[] { CreatureVocationType.All };
        foreach (var p in proto)
        {
            if (p != CreatureVocationType.All) classLimit += (int)p + ",";
            else
            {
                classLimit = "";
                break;
            }
        }
        if (classLimit.Length > 1) classLimit = classLimit.Remove(classLimit.Length - 1);
        hasClassLimit = classLimit != "";
    }

    public bool IsValidVocation(int voca)
    {
        if (proto == null || proto.Length < 1) return false;
        var v = (CreatureVocationType)voca;
        return proto.FindIndex(vv => vv == CreatureVocationType.All || vv == v) > -1;
    }

    public ItemAttachAttr GetAttributeById(ushort id)
    {
        if (attributes == null) return null;
        foreach (var item in attributes)
        {
            if (item.id == id) return item;
        }
        return null;
    }

    public static ItemAttachAttr GetBaseAttribute(PropItemInfo info,ushort id)
    {
        return info == null ? null : info.GetAttributeById(id);
    }

    public static bool IsValidVocation(PropItemInfo info, int voca)
    {
        if (!info) return false;

        return info.IsValidVocation(voca);
    }
}//config PropItemInfos : PropItemInfo

[Serializable]
public class WeaponAttribute : ConfigItem
{
    [Serializable]
    public class WeaponLevel
    {
        public ConsumePercentSubType quality;           //等级
        public int debrisNum;                           //需要的数量
        public LevelInfo[] attributes;                      //属性信息
    }
    [Serializable]
    public class LevelInfo
    {
        public int id;                              //id
        public int type;                            //类型
        public float value;                           //数值
    }

    public int elementType;  
    public int debrisId;                                //对应的碎片id
    public int compositionNum;                          //对应的多少碎片能合成
    public int decompositionNum;                        //对应分解得到的碎片
    public int elementId;                               //对应灵石id
    public WeaponLevel[] quality;                       //每级对应的武器信息 
  
}//config WeaponAttributes : WeaponAttribute

[Serializable]
public class FriendTop : ConfigItem
{
    public int topClamp;                                       // 好友上限
}//config FriendTops : FriendTop

#region ui模块--王一帆

[Serializable]
public class GrowAttributeInfo : ConfigItem
{
    public int attrId;              //属性id对应pitem中的PItemRandomAttr中的属性ID    
    public int growType;            //该属性成长的类型 1为递增 2为百分比
    public double growValue;           //该属性成长的值
}//config GrowAttributeInfos : GrowAttributeInfo

[Serializable]
public class LevelExpInfo : ConfigItem
{
    public uint exp;   //每一级对应的经验值 
}//config LevelExpInfos : LevelExpInfo

[Serializable]
public class UpStarInfo : ConfigItem
{
    public uint Lv;                    //达到升星需要的等级 
}//config UpStarInfos : UpStarInfo

[Serializable]
public class ShowCreatureInfo : ConfigItem
{
    public CreatureOrNpcData[] forData;

    [Serializable]
    public class CreatureOrNpcData
    {
        public int index;                 //界面id
        public SizeAndPos[] data;         //data
    }

    [Serializable]
    public class SizeAndPos
    {
        public float size;               //camera的size
        public float fov;                //fieldOfView
        public Vector3_ pos;             //rendercamera的pos
        public Vector3 rotation;         //rendercamera的rotation
    }
	
    public CreatureOrNpcData GetDataByIndex(int index)
    {
        if(forData != null)
        {
            for (int i = 0; i < forData.Length; i++)
            {
                if (forData[i].index == index) return forData[i];
            }
        }
        return null;
    }
}//config ShowCreatureInfos : ShowCreatureInfo

[Serializable]
public class CreatureEXPInfo : ConfigItem
{
    public uint exp;            // 每级对应的经验值

    private static uint[][] m_levelExps;

    public override void InitializeOnce()
    {
        var exps = ConfigManager.GetAll<CreatureEXPInfo>();
        exps.Sort((a, b) => a.ID > b.ID ? -1 : a.ID < b.ID ? 1 : 0);

        var max = exps.Count > 0 ? exps[0].ID : 0;
        m_levelExps = new uint[max][];

        for (var i = 0; i < max; ++i)
        {
            var p = exps.Find(e => e.ID == i);
            var n = exps.Find(e => e.ID == i + 1);

            m_levelExps[i] = new uint[] { p ? p.exp : 0, n && p && n.exp > p.exp ? n.exp - p.exp : 0 };
        }
    }

    public static int GetExpGrogressInt(int level, uint exp)
    {
        if (level < 0 || level >= m_levelExps.Length) return 100;
        var ee = m_levelExps[level];

        if (ee[1] < 1) return 100;

        var e = exp > ee[0] ? exp - ee[0] : 0;
        return Mathf.Clamp((int)((float)e / ee[1] * 100), 0, 100);
    }

    public static float GetExpGrogress(int level, uint exp)
    {
        if (level < 0 || level >= m_levelExps.Length) return 1.0f;
        var ee = m_levelExps[level];

        if (ee[1] < 1) return 1.0f;

        var e = exp > ee[0] ? exp - ee[0] : 0;
        return Mathf.Clamp01((float)e / ee[1]);
    }
}//config CreatureEXPInfos : CreatureEXPInfo

[Serializable]
public class RuneBuffInfo : ConfigItem
{
    public uint attrId;            // 属性id
    public byte addType;           //增加类型
    public double value;           //增加的值
    public byte buffId;           //四件套id
}//config RuneBuffInfos : RuneBuffInfo

[Serializable]
public class NpcGoodFeelingInfo : ConfigItem
{
    //ConfigItem.id对应NPC的等级
    public uint exp;              //每级对应的经验值

    public static List<NpcGoodFeelingInfo> npcExps { get; private set; } = new List<NpcGoodFeelingInfo>();

    public override void InitializeOnce()
    {
        base.InitializeOnce();
        npcExps = ConfigManager.GetAll<NpcGoodFeelingInfo>();
    }

}//config NpcGoodFeelingInfos : NpcGoodFeelingInfo

[Serializable]
public class NpcActionInfo : ConfigItem
{
    //ConfigItem.id对应NPC的ID
    public string kickOutState;                  //踢出界面动画
    public string sendGiftState;                 //送礼动画
    public int giftMonologue;                    //送礼独白
    public AnimAndVoice[] enterPanel;            //进界面的动作与声音
    public NpcPosition[] npcPos;                 //抚摸部位

    [Serializable]
    public struct NpcPosition
    {
        public NpcPosType posType;               //抚摸部位
        public AnimAndVoice[] normalTouch;       //普通摸NPC的动作与声音       
    }

    [Serializable]
    public struct AnimAndVoice
    {
        public NpcLevelType npcLvType;           //NPC等阶
        public string state;                     //摸的动作
        public int stateMonologue;               //动作独白(在一个id里面随机)
    }
}//config NpcActionInfos : NpcActionInfo

[Serializable]
public class NpcInfo : ConfigItem
{
    public static NpcInfo defaultNpc = new NpcInfo()
    {
        name=0,
        type = 2,
        stateMachine = 2001,
        randomMonologue = 0,
    };

    //ConfigItem.id对应NPC的等级
    public int name;                                 //名字
    public int type;                                 //type=1:界面npc有各种属性 type=2:剧情NPC没有各种属性
    public string mode;                              //模型道具
    public int cloth;                                //默认时装
    public string icon;                              //图标
    public Vector3 position;                         //生成位置
    public int stateMachine;                         //状态机名字
    public int monsterId;                            //把Npc创建战斗实体所需要的所有数据都配到Monster表里。
    public ItemAttachAttr[] attributes;              //星魂属性
    public int constellation;                        //星座名称
    public int constellationDesc;                    //星座描述
    public int constellationBuff;                    //星座效果buff
    public string allbodyicon;                       //立绘
    public int nation;                               //民族
    public int belief;                               //信仰
    public int biography;                            //传记
    public int randomMonologue;                      //随机独白
    public int randomDatingMonologue;                //约会随机独白
    public NpcFetterTask[] tasks;                    //npc的羁绊任务
    public int unlockStoryID;                        //解锁的系统剧情对话ID
    public int unlockLv;                             //解锁约会等级
    public string artNameOne;                        //名字图片1
    public string artNameTwo;                        //名字图片2
    public string avatar;                            //头像
    public string checkBox;                          //选中框
    public int voiceActor;                           //声优
    public string bigBodyIcon;                       //大立绘
    public string datingAvatar;                     //Npc约会头像
    public int labelMark;                        //标签
    public int telephone;                        //电话号码
    public int descID;                              //介绍

    [Serializable]
    public struct NpcFetterTask
    {
        public int fetterLv;
        public int taskId;
        public int eventId;                         //开启任务的剧情ID
        public int hintStoryID;                    //npc任务提示的剧情对话ID
    }
}//config NpcInfos : NpcInfo

[Serializable]
public class PvpLolInfo : ConfigItem
{
    //configItem.id==段位 从1开始
    public string icon;                       //icon
    public ushort integral;                   //积分  
    public string[] propIdAndNumber;          //奖励道具id和数量
}//config PvpLolInfos : PvpLolInfo


#region 升阶 强化 镜头控制
[Serializable]
public class IntentifyEquipInfo : ConfigItem
{    
    public EquipType type;                  //装备type
    public int level;                       //等级
    public int exp;                        //升级需要的经验
    public int costNumber;                  //金币消耗
    public ItemAttachAttr[] attributes;    //累加的属性值

    public override string ToString()
    {
        return Util.Format($"[type:{type} level:{level}]");
    }
}//config IntentifyEquipInfos : IntentifyEquipInfo

[Serializable]
public class EvolveEquipInfo : ConfigItem
{    
    [Serializable]
    public class EvolveMaterial
    {
        public int propId;      //道具ID
        public uint num;        //道具数量
    }

    public EquipType type;                  //装备type
    public int level;                       //进阶等级(配置n表示10 * n级的进阶)
    public int costNumber;                  //消耗数量
    public EvolveMaterial[] materials;      //消耗材料 id-number;
    public ItemAttachAttr[] attributes;     //累加的属性值
}//config EvolveEquipInfos : EvolveEquipInfo

[Serializable]
public class EquipAnimationInfo : ConfigItem
{
    public AnimationData[] animDatas;

    [Serializable]
    public class AnimationData
    {
        public ushort[] nowIds;
        public GotoData[] gotoDatas;
    }

    [Serializable]
    public class GotoData
    {
        public ushort[] gotoIds;
        public string state;
        public Vector3 cameraEndPos;
        public Vector3 playerEndRotate;
        public float cameraDelay;
        public float playerRotDelay;
        public float cameraUnitTime;
        public float playerRotUnitTime;
    }
}//config EquipAnimationInfos : EquipAnimationInfo

#endregion

[Serializable]
public class ProfessionInfo : ConfigItem
{  
    public int professionNameID;                    //职业名字
    public int gender;                             //性别
    public string descId;                          //描述

    public int identity
    {
        get
        {
            string[] str = descId.Split(new char[] { ',', ';' });
            return str != null && str.Length > 0 ? Util.Parse<int>(str[0]) : 0;
        }
    }

    public int dub
    {
        get
        {
            string[] str = descId.Split(new char[] { ',', ';' });
            return str != null && str.Length > 1 ? Util.Parse<int>(str[1]) : 0;
        }
    }

    public int desc
    {
        get
        {
            string[] str = descId.Split(new char[] { ',', ';' });
            return str != null && str.Length > 2 ? Util.Parse<int>(str[2]) : 0;
        }
    }

    public CreatureVocationType vocationType { get { return (CreatureVocationType)ID; } }
}//config ProfessionInfos : ProfessionInfo

[Serializable]
public class SkillInfo : ConfigItem
{
    public int protoId;                       //职业id
    public WeaponSubType[] weaponType;        //武器类型
    public int skillType;                     //技能分类 是关于技能戳法的分组
    public string skillFrame;                 //技能背景框
    public string skillIcon;                  //技能icon
    public int skillName;                     //技能名字
    public int[] skillDesc;                   //技能描述
    public int skillPosition;                 //技能位置
    public int unLockLv;                      //技能解锁需要人物等级
    public int maxLv;                         //技能最大等级
    public int isZxc;                         //是否是奥义

    /// <summary>
    /// 1 = 点击  2 = 前划  3 = 上划 4 = 下滑 5 = 后划 6 = 方向键双击 7 = 蓄力 8 = 枪击按钮 9 = 长蓄力
    /// </summary>
    public int[] inputs;         //技能戳法 

    public string _name { get { return ConfigManager.Get<ConfigText>(skillName) ? ConfigManager.Get<ConfigText>(skillName)[0] : ""; } }

}//config SkillInfos : SkillInfo

[Serializable]
public class UpSkillInfo : ConfigItem
{
    #region Static functions

    private static Dictionary<long, double> m_cachedDamage = null;

    public override void InitializeOnce()
    {
        if (m_cachedDamage == null) m_cachedDamage = new Dictionary<long, double>();
        else m_cachedDamage.Clear();

        var skills = ConfigManager.GetAll<UpSkillInfo>();
        foreach (var skill in skills)
        {
            long sid = skill.skillId;
            var uid = sid | (long)skill.skillLevel << 32;
            m_cachedDamage.Set(uid, skill.damageMul);
        }
    }

    public static double GetOverrideDamage(int skill, int level, bool usePrev = true)
    {
        long sid = skill;
        var uid = sid | (long)level << 32;
        if (!usePrev) return m_cachedDamage.Get(uid);

        double damageMul = 0;
        while (level > -1)
        {
            uid = sid | (long)level << 32;
            if (m_cachedDamage.TryGetValue(uid, out damageMul)) return damageMul;
            --level;
        }
        return 0;
    }

    #endregion

    public int skillId;                          //技能id
    public int skillLevel;                       //技能等级
    public int expendSp;                         //技能消耗的技能点
    public int expendGold;                       //技能消耗的金币
    public int needLv;                           //升级需要的角色等级
    public double damageMul;                     //伤害因子增量 影响当前技能下的所有状态的所有攻击盒子产生的 AttackInfo 默认为 0
    public ItemAttachAttr[] attributes;          //属性变化
}//config UpSkillInfos : UpSkillInfo

[Serializable, ConfigSortOrder(1)]
public class SkillToStateInfo : ConfigItem
{
    #region Static functions

    private static Dictionary<long, int> m_stateToSKill = new Dictionary<long, int>();

    public static int GetSkillID(int weapon, int state)
    {
        var w = (long)weapon;
        return m_stateToSKill.Get(w | (long)state << 32, -1);
    }

    public static int GetSkillID(int weapon, string state)
    {
        var w = (long)weapon;
        return m_stateToSKill.Get(w | (long)CreatureStateInfo.NameToID(state) << 32, -1);
    }

    public override void InitializeOnce()
    {
        m_stateToSKill.Clear();

        var si = ConfigManager.GetAll<SkillToStateInfo>();
        foreach (var s in si)
        {
            var ss = s.states;
            var wt = (long)s.weaponType;
            foreach (var sn in ss)
            {
                var id = CreatureStateInfo.NameToID(sn);
                if (id < 0)
                {
                    Logger.LogError("SkillToStateInfo::Initialize: Skill[{0}, weapon:{1}] has invalid state[{2}]", s.skillId, sn, id);
                    continue;
                }
                var guid = wt | (long)id << 32;
                if (m_stateToSKill.ContainsKey(guid))
                    Logger.LogError("SkillToStateInfo::Initialize: State[{0}:{1}, weapon:{2}] belongs to multiple skills [{3}, {4}], use newer one.", id, CreatureStateInfo.IDToName(id), (WeaponSubType)s.weaponType, m_stateToSKill[guid], s.skillId);
                m_stateToSKill.Set(guid, s.skillId);
            }
        }
    }

    #endregion

    public int skillId;               // 技能id
    public int elmentType;            // 元素类型
    public int weaponType;            // 对应的武器类型
    public string[] stateNames;       // 状态名
    public string[] addStates;        // 总状态状态名

    public string[] states { get { return m_states; } }
    private string[] m_states;

    public override void  OnLoad()
    {
        if (stateNames == null) stateNames = new string[] { };
        if (addStates == null) addStates = new string[] { };

        var ss = new List<string>();
        ss.AddRange(stateNames);
        ss.AddRange(addStates);

        ss.Distinct();
        m_states = ss.ToArray();
    }
}//config SkillToStateInfos : SkillToStateInfo
#endregion

[Serializable]
public class DropProbable : ConfigItem
{
    public string details;            // 文字
    public string color;               //文字颜色
    public int bold;                    //粗细 1 粗 0 平常
    public int size;                     //对应字号
    public int feed;                    //0 不换行 1 换行
}//config DropProbables : DropProbable

[Serializable]
public class PropToBuffInfo : ConfigItem
{
    public string description;

    public int propId;

    public int buffId;

    public string effect;
}//config PropToBuffInfos : PropToBuffInfo

[Serializable]
public class FaceName : ConfigItem
{
    #region Static functions

    public static List<string> cachedAssets => m_cachedAssets;
    private static List<string> m_cachedAssets = new List<string>();

    public override void InitializeOnce()
    {
        m_cachedAssets.Clear();

        var fs = ConfigManager.GetAll<FaceName>();
        foreach (var f in fs) m_cachedAssets.Add(f.head_icon);

        m_cachedAssets.Distinct();
        m_cachedAssets.Remove(string.Empty);
    }

    #endregion

    public string head_icon;        //表情名字

    public override void OnLoad()
    {
        if (string.IsNullOrEmpty(head_icon) || string.IsNullOrWhiteSpace(head_icon))
            head_icon = string.Empty;
    }
}//config FaceNames : FaceName


/// <summary>
/// 所有通用配置都包含在这里面
/// </summary>
[Serializable, ConfigSortOrder(-1)]
public class GeneralConfigInfo : ConfigItem
{
    #region Static functions

    public static GeneralConfigInfo defaultConfig { get; private set; } = new GeneralConfigInfo()
    {
        appName = "空之挽歌",
        sensitiveReplace = "**", maxAspectRatio = 2.2f, storyMaskRenderQueue = 2110,
        showLevel = 2, homeLevel = 1, trainLevel = 2, waitLoginTime = 1.0f, waitToQuit = 3.0f, globalMessageDuration = 1.5f, sceneTextObject = "textpanel",
        roleLevel = 9,
        petHomeLevel = 8,
        maxVolume = 1.0f, maxBgmVolume = 1.0f, maxVoiceVolume = 1.0f, maxSoundVolume = 1.0f,
        loadingInfoSwitch = 5, windowFadeDuration = 0.25f,
        expbarUnitTime = 1f,
        homeUnlockTime = new float[] {1.5f,1.5f},
        monHealthColor = new Color(1.0f, 0.255f, 0.376f),
        npcHealthColor = new Color(0.596f, 0.847f, 0.027f),
        percentAttriIds = new ushort[] { 9, 10, 11, 12, 13 },
        forbidRuneAndForgeChaseIds = new int[] {101,102,103,104,105},
        playerLimitInScene = new int[] { 3, 5, 10, 20 },
        unlockLabyrinthLevel = 15,
        defaultLevel = 1,
        downTime = 2,
        addPoint = 1,
        waitLogueTime = 15,
        noEnoughColor = "#FF0000FF",
        subtractAttr = "#1EFF00FF",
        norRColor = new Color[] { Color.white },
        twoSOColor = new Color[] { Color.white },
        twoSTColor = new Color[] { Color.white },
        twoSThColor = new Color[] { Color.white },
        fourSColor = new Color[] { Color.white },
        qualitys = new Color[] { Util.BuildColor("#b6b8baff"), Util.BuildColor("#6c9863ff"), Util.BuildColor("#4981c7ff"), Util.BuildColor("#6f467bff"), Util.BuildColor("#ddc585ff"), Util.BuildColor("#fcb271ff") },
        textQualitys = new Color[] { Util.BuildColor("#b6b8baff"), Util.BuildColor("#6c9863ff"), Util.BuildColor("#4981c7ff"), Util.BuildColor("#6f467bff"), Util.BuildColor("#ddc585ff"), Util.BuildColor("#fcb271ff") },
        syncCheckInterval = 300,
    };

    public static string sappName => defaultConfig.appName;
    public static int sfactionScoreBase => defaultConfig.factionScoreBase;
    public static int sshowLevel => defaultConfig.showLevel;
    public static int sroleLevel => defaultConfig.roleLevel;
    public static int shomeLevel => defaultConfig.homeLevel;
    public static int sTrainLevel => defaultConfig.trainLevel;
    public static int sPetHomeLevel => defaultConfig.petHomeLevel;
    /// <summary>
    /// 登陆时切换账号的等待时间
    /// </summary>
    public static int swaitLoginTime { get { return (int)(defaultConfig.waitLoginTime * 1000); } }
    /// <summary>
    /// 加载界面提示信息切换间隔
    /// </summary>
    public static float sloadingInfoSwitch { get { return defaultConfig.loadingInfoSwitch; } }
    /// <summary>
    /// 窗口默认淡入/淡出时间
    /// 可以通过手动为窗口的预设添加 TweenAlpha 组件来自定义
    /// </summary>
    public static float swindowFadeDuration { get { return defaultConfig.windowFadeDuration; } }

    /// <summary>
    /// 移动平台下退出按钮等待时间
    /// 只有在按下退出键后在此时间内再次按下退出按钮才会退出游戏
    /// </summary>
    public static float swaitToQuit { get { return defaultConfig.waitToQuit; } }

    /// <summary> 默认提示文字停留时间 </summary>
    public static float sglobalMessageDuration { get { return defaultConfig.globalMessageDuration; } }

    /// <summary> 场景 3D 文本对象 </summary>
    public static string ssceneTextObject { get { return defaultConfig.sceneTextObject; } }

    /// <summary>
    /// 默认敏感字替换字符串
    /// </summary>
    public static string ssensitiveReplace { get { return defaultConfig.sensitiveReplace; } }

    /// <summary>
    /// 最大设计 UI 比例
    /// </summary>
    public static float smaxAspectRatio { get { return defaultConfig.maxAspectRatio; } }

    public static float sexpbarUnitTime { get { return defaultConfig.expbarUnitTime; } }

    #region Audios

    public static float svolume { get { return defaultConfig.volume; } }
    public static float sbgmVolume { get { return defaultConfig.bgmVolume; } }
    public static float svoiceVolume { get { return defaultConfig.voiceVolume; } }
    public static float ssoundVolume { get { return defaultConfig.soundVolume; } }

    public static float smaxVolume { get { return defaultConfig.maxVolume; } }
    public static float smaxBgmVolume { get { return defaultConfig.maxBgmVolume; } }
    public static float smaxVoiceVolume { get { return defaultConfig.maxVoiceVolume; } }
    public static float smaxSoundVolume { get { return defaultConfig.maxSoundVolume; } }

    #endregion

    #region stroy
    /// <summary>
    /// 剧情遮罩渲染队列 默认为 2110
    /// 注意 该渲染队列必须大于角色渲染队列，并留一定空间 比如角色队列为 2101 那么 遮罩队列应当使用 2110 或者更高，而不能使用 2102 因为角色渲染队列可能占用 2101 - 2109
    /// </summary>
    public static int sstoryMaskRenderQueue { get { return defaultConfig.storyMaskRenderQueue; } }
    public static float scontextInterval { get { return defaultConfig.storyData == null ? 0.08f : defaultConfig.storyData.contextInterval; } }
    public static float snpcLeaveTime { get { return defaultConfig.storyData == null ? 0.5f : defaultConfig.storyData.npcLeaveTime; } }
    public static float sstorySpeed { get { return defaultConfig.storyData == null || defaultConfig.storyData.storySpeed <= 0f ? 1f : defaultConfig.storyData.storySpeed; } }
    public static int sstoryPreLoadNum { get { return defaultConfig.storyData == null || defaultConfig.storyData.storyPreLoadNum <= 0f ? -1 : defaultConfig.storyData.storyPreLoadNum; } }
    #endregion

    #region home unlock time

    public static float homeUnlockTime1 { get { if (defaultConfig && defaultConfig.homeUnlockTime != null && defaultConfig.homeUnlockTime.Length > 0) return defaultConfig.homeUnlockTime[0]; return 0f; } }

    public static float homeUnlockTime2 { get { if (defaultConfig && defaultConfig.homeUnlockTime != null && defaultConfig.homeUnlockTime.Length > 1) return defaultConfig.homeUnlockTime[1]; return 0f; } }

    public static float homeUnlockTime3 { get { if (defaultConfig && defaultConfig.homeUnlockTime != null && defaultConfig.homeUnlockTime.Length > 2) return defaultConfig.homeUnlockTime[2]; return 0f; } }
    #endregion

    #region pve
    public static Color monsterHealthColor { get { return defaultConfig.monHealthColor; } }
    public static Color playerHealthColor { get { return defaultConfig.npcHealthColor; } }
    public static float AIBorderDis { get { return defaultConfig.borderDis; } }
    public static float AIPMDis { get { return defaultConfig.borderPMDis; } }
    #endregion

    #region register
    public static int MIN_NAME_LEN { get { return defaultConfig.validNameLimit ? defaultConfig.nameLimit[0] : 2; } }
    public static int MAX_NAME_LEN { get { return defaultConfig.validNameLimit ? defaultConfig.nameLimit[1] : 11; } }
    #endregion

    #region guide
    public static bool NeedForbideRuneAndForgeWhenSettlement(int taskId)
    {
        if (defaultConfig.forbidRuneAndForgeChaseIds == null || defaultConfig.forbidRuneAndForgeChaseIds.Length == 0) return false;
        return defaultConfig.forbidRuneAndForgeChaseIds.Contains(taskId);
    }

    public static bool NeedIgnoreToChaseList(int guideId)
    {
        if (defaultConfig.ignoreToChaseListGuideIds == null || defaultConfig.ignoreToChaseListGuideIds.Length == 0) return false;
        return defaultConfig.ignoreToChaseListGuideIds.Contains(guideId);
    }

    public static int sunlockLabyrinthLevel { get { return defaultConfig.unlockLabyrinthLevel; } }
    public static int sguideLabyrinthId { get { return defaultConfig.guideLabyrinthId; } }
    public static int sguideGoodFeelingId { get { return defaultConfig.guideGoodFeelingId; } }
    public static int sguideRuneEvolveId { get { return defaultConfig.guideRuneEvolveId; } }
    public static int sguideForceEquipRuneId { get { return defaultConfig.guideForceEquipRuneId; } }
    public static int sguideShopTheatry { get { return defaultConfig.guideShopTheatry; } }
    public static int sguidePVPId { get { return defaultConfig.guidePVPId; } }
    public static float sguideInvalidTime { get { return defaultConfig.guideInvalidTime <= 0 ? 2f : defaultConfig.guideInvalidTime; } }
    public static float sguideStoryMaskTime { get { return defaultConfig.guideStoryMaskTime <= 0 ? 2f : defaultConfig.guideStoryMaskTime; } }

    public static InterludeTime sinterludeWindowTime { get { return defaultConfig.interludeWindowTime == null ? InterludeTime.empty : defaultConfig.interludeWindowTime; } }
    public static InterludeTime sinterludeTextTime { get { return defaultConfig.interludeTextTime == null ? InterludeTime.empty : defaultConfig.interludeTextTime; } }
    #endregion

    #region labyrinth
    public static float slabyrinthTriggerTime { get { return defaultConfig.labyrinthTriggerTime; } }
    public static int[] splayerLimitInScene { get { return defaultConfig.playerLimitInScene; } }

    #endregion

    #region dating
    public static DatingMapCamera sdatingMapCamera { get { return defaultConfig.datingMapCamera; } }
    #endregion

    #endregion

    #region Definitions

    [Serializable]
    public class StoryData
    {
        public float contextInterval = 0.08f;           //剧情蹦字时间间隔
        public float npcLeaveTime = 0.5f;               //剧场动画npc离场动画的时间
        public int genderTextId;                        //{1}表示取玩家性别，并对应使用“大哥哥”或者“姐姐”，卡琳对话专用
        public float storySpeed = 1;                      //快进加速
        public int storyPreLoadNum = 3;                   //预加载条目
    }

    [Serializable]
    public class InterludeTime
    {
        public static InterludeTime empty = new InterludeTime() { fadeInTime = 1, remainInTime = 0.5f, remainOutTime = 0.5f, fadeOutTime = 1 };

        public float fadeInTime = 1f;                       //淡入时间
        public float remainInTime = 0.5f;                     //持续时间
        public float remainOutTime = 0.5f;
        public float fadeOutTime = 1;                       //淡出时间
    }

    [Serializable]
    public struct LineParam
    {
        public float startWidth;
        public float endWidth;
        public Color startColor;
        public Color endColor;
    }

    [Serializable]
    public class DatingMapCamera
    {
        public Vector3 leftPos;
        public Vector3 rightPos;
        public Vector3 maxLeftPos;
        public Vector3 maxRightPos;
    }

    #endregion

    #region Audio config

    public float volume;
    public float bgmVolume;
    public float voiceVolume;
    public float soundVolume;

    public float maxVolume;
    public float maxBgmVolume;
    public float maxVoiceVolume;
    public float maxSoundVolume;

    #endregion

    #region attribute
    public ushort[] percentAttriIds;        //需要显示成百分比的属性ID集合
    #endregion

    /// <summary>
    /// 应用名称
    /// </summary>
    public string appName;

    public string sensitiveReplace;

    /// <summary>
    /// Designed max aspect ratio, default 2.2 (1584 / 720)
    /// </summary>
    public float maxAspectRatio = 1584 / 720.0f; // 2.2

    public int showLevel;   // 演示关卡 第一次安装游戏时运行游戏进入
    public int homeLevel;   // 主场景关卡 ID
    public int roleLevel;   // 主场景关卡 ID
    public int trainLevel;  // 教学关 ID
    public int petHomeLevel;
    /// <summary>
    /// 登陆时切换账号的等待时间
    /// </summary>
    public float waitLoginTime;
    /// <summary>
    /// 加载界面提示信息切换间隔
    /// </summary>
    public float loadingInfoSwitch;
    /// <summary>
    /// 窗口默认淡入/淡出时间
    /// 可以通过手动为窗口的预设添加 TweenAlpha 组件来自定义
    /// </summary>
    public float windowFadeDuration;

    /// <summary>
    /// 移动平台下退出按钮等待时间
    /// 只有在按下退出键后在此时间内再次按下退出按钮才会退出游戏
    /// </summary>
    public float waitToQuit;

    /// <summary> 默认提示文字停留时间 </summary>
    public float globalMessageDuration;

    /// <summary> 场景 3D 文本对象 </summary>
    public string sceneTextObject;

    public StateMachineInfo.Effect lockEffect;
    public StateMachineInfo.Effect unlockEffect;
    public StateMachineInfo.Effect evolveEffect;
    public StateMachineInfo.Effect roleSelectEffect;
   
    public float mes_moveup_strat;                                 //等待上移时间
    public float mes_moveup_all;                                   //上移所需时间
    public float mes_moveup_all_dis;                               //上移的距离
    public float mes_hidden_start;                                 //开始隐藏的时间
    public float mes_hidden_all;                                   //隐藏需要的时间

    public float awd_moveup_strat;                                 //等待上移时间
    public float awd_moveup_all;                                   //上移所需时间
    public float awd_moveup_all_dis;                               //上移的距离
    public float awd_hidden_start;                                 //开始隐藏的时间
    public float awd_hidden_all;                                   //隐藏需要的时间

    public float move_diatance;                                    //公告移动距离
    public float move_time;                                        //公告移动时间
    public float strat_diatance;                                   //启动距离

    #region story
    public int storyMaskRenderQueue;                               //剧情对话遮罩渲染队列  注意 该渲染队列必须大于角色渲染队列，并留一定空间 比如角色队列为 2101 那么 遮罩队列应当使用 2110 或者更高，而不能使用 2102 因为角色渲染队列可能占用 2101 - 2109
    public StoryData storyData;                                    //剧情相关常量配置
    public int[] defaultHomeFunc;                                  //大厅初始默认需要开启的功能
    public float[] homeUnlockTime;                                 //解锁时间，0 = 出击大按钮和二级界面按钮解锁时间；1 = 主界面左上角和右上角的解锁时间
    #endregion

    #region guide

    public int[] forbidRuneAndForgeChaseIds;                       //失败后需要禁止跳转到符文和锻造的追捕任务ID（关联到taskinfo）
    public int unlockLabyrinthLevel;                               //解锁迷宫的玩家等级
    public int guideLabyrinthId;                                   //迷宫解锁的引导ID（关联到guideinfo）（该ID和玩家等级共同决定是否客户端需要发送解锁迷宫消息）
    public int guideGoodFeelingId;                                 //好感度道具的引导ID（关联到guideinfo）（因为会涉及到动态更换window_gift窗口的显示层级）
    public int guideForceEquipRuneId;                              //符文进化的引导ID（关联到guideinfo）（该ID决定了需要强行装备的灵柏，该灵柏必须能进化 ）
    public int guideRuneEvolveId;                                  //符文进化的引导ID（关联到guideinfo）（该ID决定了将需要吞噬进化的狗粮优先显示到开始的几个位置）
    public int guideShopTheatry;                                   //商店引导需要特殊处理不打开遮罩的引导ID
    public int guidePVPId;                                         //pvp引导的ID，重连需要特殊判断
    public int[] ignoreToChaseListGuideIds;                        //重连需要忽略跳转到追捕列表显示的引导ID（关联到guideinfo）
    public float guideInvalidTime;                                 //强制引导找到非法热点区域的超时时间
    public float guideStoryMaskTime;                               //剧情遮挡黑屏遮罩的强制时间

                                                                   //幕间动画相关
    public InterludeTime interludeWindowTime;                      //幕间动画窗口时间控制
    public InterludeTime interludeTextTime;                        //幕间动画标题时间控制

    #endregion

    public int InsoulMax;                                          //入魂特效最大值

    public float WewaponScale;                                     //缩放比例

    #region 武器旋转
    public float rotationSpeed;                                    //武器旋转速度
    public Vector3 aroundTarget;                                   //围绕目标向量旋转
    #endregion

    public Color monHealthColor;                                   //怪物血条颜色
    public Color npcHealthColor;                                   //npc血条颜色

    public int[] nameLimit = new int[] {2,11 };                    //取名的限制

    public float waitDragTime;                                     //装备界面等待滑动的时间
    public int isTouchNpc;                                         //是否开启摸NPC 0==on 1==off

    public float borderDis = 20;                                   //AI选择时，距离多少分米会被认定为处于边界
    public float borderPMDis = 20;                                 //AI边界选择时，距离小于该值才会被认定为边界

    public Color FriendNormal;                                     //未达到最高限制颜色
    public Color FriendTop;                                        //达到最高限制颜色
    #region 聊天底框
    public float ChatMax;                                          //最长多长之后要换行
                                                                   //不足一行时
    public float ChatTxtWidth;                                     //文字图片的宽(比文字长度长多少)
    public float ChatTxtHeight;                                    //文字图片的高
    public float ChatBackHeight;                                   //背景图片的高
    public float ChatAllHeight;                                    // 全部高度
                                                                   //超出一行时
    public float OverChatTxtWidth;                                 //文字图片的宽
    public float OverChatTxtHeight;                                //文字图片的高(比文字高多少)
    public float OverChatBackHeight;                               //背景图片的高（比文字高多少）
                                                                   //图片
    public float ChatImgAllHeight;                                 //发图片时候的高度
    #endregion

    public int PetTimes;                                           //每日宠物关卡最高限制次数

    public Color CreateUnionEnough;                                //创建公会足够颜色
    public Color CreateUnionLess;                                  //创建公会不足颜色

    public Color RuneNormal;                                       //符文套装未达成颜色
    public Color RuneConclude;                                     //符文套装达成颜色

    public Color PropEnough;                                       //道具足够

    public Color WareHouse;                                        //仓库数量常规色

    public float friendLerp;                                       // 好友差多少出现提示
    public float chatLerp;                                         // 世界差多少出现提示

    public float fightTime;                                        //战斗力动画时间
    public float fightDelayClose;                                  //战斗力延迟关闭时间

    public int invateInterval;                                     //邀请切磋的间隔 秒
    public bool validNameLimit { get { return nameLimit != null && nameLimit.Length > 1; } }

    public float expbarUnitTime;                                   //经验条升级动画单位时间（升满一级的时间）
    public string noEnoughColor;                                   //金币不够的颜色改变
    public int defaultLevel;                                       //时装店默认角色的档位
    public float activeScrollTime;                                 //活动拖动时间
    public Color[] activeShadowColors;                             //活动界面shadow
    public float[] soulMapRange;                                   //附灵程度映射范围
    #region labyrinth
    public float labyrinthTriggerTime = 0.5f;                      //当玩家处于StateRun状态时，如果持续时间超过该数值，则判定为触发进入事件
    public int[] playerLimitInScene = new int[]{ 3, 5, 10, 20};    //场景中的玩家数量限制
    #endregion

    public float downTime;                                         //属性按下的时间
    public int addPoint;                                           //每帧增加的属性点

    public int dropOpenID;                                         //跳转开启的引导id
    public LineParam[] lineParams;

    public int coopInvateTop;                                      //协作邀请人数
    public Color[] UnionBossBlood;                                 //公会boss血条颜色
    public float waitLogueTime;                                    //随机独白的间隔时间
    public float wstOnEngagement;                                  //羁绊任务在约会结束提示的延迟时间
    public float wstOnPve;                                         //羁绊任务在pve结束提示的延迟时间

    public float TeamHintCD;                                       //好友邀请主界面显示时间

    public int syncCheckInterval;                                  //同步检测频率（帧数）

    public float teamBroadCD;                                      //组队时广播的cd

    public Color rankSelfColor;                                    //排行榜是自己背景色

    #region  迭代灵珀
    public Color[] norRColor;                                       //默认符文颜色
    public Color[] twoSOColor;                                      //第一个两件套的颜色
    public Color[] twoSTColor;                                      //第二个两件套的颜色
    public Color[] twoSThColor;                                     //第三个两件套的颜色
    public Color[] fourSColor;                                      //四件套的颜色

    public Color[] qualitys;                                        //品质颜色
    public Color[] textQualitys;                                    //字体品质颜色
    public string subtractAttr;                                      //减属性的颜色
    #endregion

    #region Home界面切换约会Npc头像过渡特效
    public string homeScreenEffect;                                 //Home界面切屏特效名字
    public float screenEffectPlayTime;                             //切屏特效播放时间
    #endregion

    public int datingNpcMaxMood;                                //约会Npc最大心情值

    public DatingMapCamera datingMapCamera;                     //约会主界面滚动视差相机

    public string datingStartEffect;                            //约会开始的特效名

    public float bannerInterval;                                //banner的更换间隔

    public int datingEndEventId;                                //Npc助战约会结算的事件id

    public float factionReadlyCountDown;                        //阵营战展示双方数据的倒计时

    public int factionScoreBase;                                //阵营战积分计算基数

    public override void Initialize()
    {
        if (ID == 0) defaultConfig = this;
    }

    #region static attribute functions
    public static bool IsPercentAttribute(int attrId)
    {
        return IsPercentAttribute((ushort)attrId);
    }

    public static bool IsPercentAttribute(ushort attrId)
    {
        return defaultConfig.percentAttriIds != null && defaultConfig.percentAttriIds.Contains(attrId);
    }

    public static string GetNoEnoughColorString(string str)
    {
        if (defaultConfig.noEnoughColor != null)
        {
            if (defaultConfig.noEnoughColor.Contains("#"))
                return Util.Format("<color={0}>{1}</color>", defaultConfig.noEnoughColor, str);
            else
                return Util.Format("<color=#{0}>{1}</color>", defaultConfig.noEnoughColor, str);
        }
        return "";
    }

    public static string GetSubAttrColorString(string str)
    {
        if (defaultConfig.subtractAttr != null)
        {
            if (defaultConfig.subtractAttr.Contains("#"))
                return Util.Format("<color={0}>{1}</color>", defaultConfig.subtractAttr, str);
            else
                return Util.Format("<color=#{0}>{1}</color>", defaultConfig.subtractAttr, str);
        }
        return "";
    }
    #endregion
}//config GeneralConfigInfos : GeneralConfigInfo

/// <summary>
/// UI 音效配置
/// 包含所有 UI 用户输入事件出发的音效
/// </summary>
[Serializable]
public class UISoundEffect : ConfigItem
{
    #region Static functions

    public const string TAG_DEFAULT = "Normal";
    public const string TAG_WINDOW  = "Window";

    public static UISoundEffect empty { get; } = new UISoundEffect() { ID = -1, element = null, tag = null, isEmpty = true };

    public static UISoundEffect defaultConfig { get; private set; } = new UISoundEffect()
    {
        tag = "Normal", element = null, onPointerClick = new string [] { "audio_ui_button_click" }, isEmpty = false
    };

    private static Dictionary<string, UISoundEffect> m_uiSounds = new Dictionary<string, UISoundEffect>();

    public static UISoundEffect GetSound(GameObject target)
    {
        if (!target || string.IsNullOrEmpty(target.tag)) return empty;

        UISoundEffect d = null;

        if (string.IsNullOrEmpty(target.name)) d = target.tag == TAG_DEFAULT ? defaultConfig : m_uiSounds.Get(target.tag);
        else
        {
            d = m_uiSounds.Get(target.tag + ":" + target.name);
            if (d == null) d = target.tag == TAG_DEFAULT ? defaultConfig : m_uiSounds.Get(target.tag);
        }
        return d ?? empty;
    }

    public static UISoundEffect GetSound(string tag, string element)
    {
        UISoundEffect d = null;

        if (string.IsNullOrEmpty(element)) d = tag == TAG_DEFAULT ? defaultConfig : m_uiSounds.Get(tag);
        else
        {
            d = m_uiSounds.Get(tag + ":" + element);
            if (d == null) d = tag == TAG_DEFAULT ? defaultConfig : m_uiSounds.Get(tag);
        }
        return d ?? empty;
    }
    
    /// <summary>
    /// 根据指定职业选择对应的音效
    /// 数组索引即职业 ID
    /// </summary>
    /// <param name="ss"></param>
    /// <param name="proto"></param>
    /// <returns></returns>
    public static string GetSound(string[] ss, int proto)
    {
        if (ss == null || ss.Length < 1) return null;
        if (ss.Length == 1) return ss[0];

        if (proto < 0 || proto >= ss.Length) return null;

        return ss[proto];
    }

    /// <summary>
    /// 根据玩家当前职业选择对应的音效
    /// 数组索引即职业 ID
    /// </summary>
    /// <param name="ss"></param>
    /// <param name="proto"></param>
    /// <returns></returns>
    public static string GetSound(string[] ss)
    {
        if (ss == null || ss.Length < 1) return null;
        if (ss.Length == 1) return ss[0];

        var proto = (int)Module_Player.instance?.proto - 1;
        if (proto < 0 || proto >= ss.Length) return null;

        return ss[proto];
    }

    #endregion

    ///<summary> 是否为空？ </summary>
    public bool isEmpty { get; private set; }

    ///<summary> 使用此音效配置的 UI 元素 tag </summary>
    public string tag;
    ///<summary> 使用此音效配置的 UI 元素名称 </summary>
    public string element;
    ///<summary> 音效类型 </summary>
    public AudioTypes type;
    ///<summary> 是否覆盖相同名称的音乐/音效 </summary>
    public bool @override;

    ///<summary> 当 UI 元素可见时 </summary>
    public string[] onEnable;
    ///<summary> 当 UI 元素不可见时 </summary>
    public string[] onDisable;
    ///<summary> 鼠标指针/Touch 进入 </summary>
    public string[] onPointerEnter;
    ///<summary> 鼠标指针/Touch 离开 </summary>
    public string[] onPointerExit;
    ///<summary> 鼠标指针/Touch 按下 </summary>
    public string[] onPointerDown;
    ///<summary> 鼠标指针/Touch 弹起 </summary>
    public string[] onPointerUp;
    ///<summary> 鼠标指针/Touch 点击 </summary>
    public string[] onPointerClick;
    ///<summary> 拖拽事件初始化 </summary>
    public string[] onInitializePotentialDrag;
    ///<summary> 开始拖拽 </summary>
    public string[] onBeginDrag;
    ///<summary> 拖拽 </summary>
    public string[] onDrag;
    ///<summary> 拖拽结束 </summary>
    public string[] onEndDrag;
    ///<summary> 控件接受到拖拽目标 </summary>
    public string[] onDrop;
    ///<summary> 滚动 </summary>
    public string[] onScroll;
    ///<summary> 焦点目标发生更改 </summary>
    public string[] onUpdateSelected;
    ///<summary> 获得焦点 </summary>
    public string[] onSelect;
    ///<summary> 失去焦点 </summary>
    public string[] onDeselect;
    ///<summary> 指针/Touch 移动 </summary>
    public string[] onMove;
    ///<summary> 发送行为 </summary>
    public string[] onSubmit;
    ///<summary> 取消行为 </summary>
    public string[] onCancel;

    public override void OnLoad()
    {
        isEmpty = string.IsNullOrEmpty(tag);

        if (isEmpty) Logger.LogWarning("UISoundEffect::Initialize: Invalid UISoundEffect config [{0}], tag can not be null or empty", ID);

        if (ID == 0) defaultConfig = this;
    }

    public override void InitializeOnce()
    {
        var ss = ConfigManager.GetAll<UISoundEffect>();
        foreach (var s in ss)
        {
            if (s.isEmpty) continue;
            m_uiSounds.Set(string.IsNullOrEmpty(s.element) ? s.tag : s.tag + ":" + s.element, s);
        }
    }
}//config UISoundEffects : UISoundEffect

/// <summary>
/// 代码中写死的音效
/// </summary>
[Serializable]
public class AudioInLogicInfo : ConfigItem
{
    public static AudioInLogicInfo audioConst { get; private set; } = new AudioInLogicInfo()
    {
        clickToSucc    = "audio_ui_button_pay",
        sliderToFull   = "audio_ui_progress",
        insoulToSucc   = "audio_ui_doinsoul",
        countDown      = "audio_ui_countdown_5s",
        cardOpen       = "audio_ui_button_getreward",
        upInsoulSucc   = "audio_voc_ui_jingliu_work01",
        upWeaStageSucc = new string[] { "audio_voc_ui_jingliu_work02", "audio_voc_ui_jingliu_work03" },
        enterStageSucc = "audio_ui_enterstage",
        petTrain       = new string[] { "audio_voc_ui_xiao_chongwu01", "audio_voc_ui_xi_chongwu01", "XX", "audio_voc_ui_qiao_chongwu01", "audio_voc_ui_weila_chongwu01" },
        petSummon      = "audio_ui_snd_chongwu01"
    };

    public string clickToSucc;
    public string sliderToFull;
    public string insoulToSucc;
    public string countDown;
    public string cardOpen;
    public string upInsoulSucc;
    public string[] upWeaStageSucc;
    public string enterStageSucc;
    public string[] petTrain;
    public string petSummon;

    public override void InitializeOnce()
    {
        if (ID == 0)
            audioConst = this;
    }
}//config AudioInLogicInfos : AudioInLogicInfo

/// <summary>
/// 玩家昵称表
/// </summary>
[Serializable]
public class NameConfigInfo : ConfigItem
{
    public string[] maleFirstName;           //男姓

    public string[] maleSecondName;          //男名

    public string[] femaleFirstName;           //女姓

    public string[] femaleSecondName;          //女名

    //默认名称列表
    private static NameConfigInfo m_defaultNameConfig;
    public static NameConfigInfo defaultNameConfig
    {
        get
        {
            if (m_defaultNameConfig == null)
                m_defaultNameConfig = ConfigManager.Get<NameConfigInfo>(0);
            return m_defaultNameConfig;
        }
    }

    public static string GetRandomName(GenderRole gender)
    {
        StringBuilder sb = new StringBuilder();
        string[] first = defaultNameConfig.femaleFirstName;
        string[] second = defaultNameConfig.femaleSecondName;
        if(gender == GenderRole.Male)
        {
            first = defaultNameConfig.maleFirstName;
            second = defaultNameConfig.maleSecondName;
        }

        if (first != null && first.Length > 0)
            sb.Append(first[Random.Range(0, first.Length)]);
        if (second != null && second.Length > 0)
            sb.Append(second[Random.Range(0, second.Length)]);

        return sb.ToString();
    }

    public static string GetRandomName(int gender)
    {
        return GetRandomName((GenderRole)gender);
    }
}//config NameConfigInfos : NameConfigInfo

[Serializable]

public class Insoul : ConfigItem
{
    public int exp;    //对应的经验 累积的
    public int lingshi;        //灵石数量
    public int gold;           //金币数量
    public int exp_one;        //每一次的经验值
    public int attribute;      //属性增量

}//config Insouls : Insoul
[Serializable]
public class Upload : ConfigItem
{
    public int debris_num;    //对应的碎片数量

}//config Uploads : Upload

[Serializable]
public class CooperationTask : ConfigItem
{
    [Serializable]
    public class KillMonster
    {
        public int[] monsterId;//需要击杀的怪物id
        public int number;     //击杀数量
        public int type;       //击杀类型
        public int text;       //怪物名称
    }
    public int type;            //任务类型
    public int difficult;       //难度
    public int name;            //名称
    public int desc;            //描述
    public string icon;         //图片
    public int limitLevel;      //限制等级
    public TaskInfo.TaskStarReward reward;      //展示奖励
    public KillMonster[] conditions;  //需要击杀怪物信息 0 本身 1 被邀请  
}//config CooperationTasks : CooperationTask

[Serializable]
public class BaseAttribute
{
    public ItemAttachAttr[] attributes;     //累加的属性值

    /// <summary>
    /// 属性累计加成。如果基础属性不包含加成属性。那么久不加成 否者以基础属性为基数按值/百分比加成
    /// </summary>
    /// <param name="a"></param>
    /// <param name="addition"></param>
    /// <returns></returns>
    public static List<ItemAttachAttr> operator + (BaseAttribute a, ItemAttachAttr[] addition)
    {
        List<ItemAttachAttr> attrs = new List<ItemAttachAttr>(a.attributes);
        attrs.AddRange(addition);
        return attrs;
    }
}

[Serializable]
public class PetAttributeInfo : ConfigItem
{
    [Serializable]
    public class PetAttribute : BaseAttribute
    {
        public int level;               //等级
        public uint exp;
    }

    public PetAttribute[] PetAttributes;
}//config PetAttributeInfos : PetAttributeInfo

public enum EnumCreatureQuality
{
    White,
    Green,
    Blue,
    Cyan,
    Gold,
}

public enum EnumFightType
{
    Treat,
    Defend,
    Attack,
}

[Serializable]
public class ConfigPetInfo : ConfigItem
{
    public int fightType;           //战斗类型
    public float TalkRate;          //说话的几率
    public int[] Words;             //随机的话数组
    public EnumFightType FightType { get { return (EnumFightType)fightType; } }
}//config ConfigPetInfos : ConfigPetInfo

[Serializable]
public class ItemPair
{
    public int itemId;
    public int count;
}

[Serializable]
public class PetUpGradeInfo : ConfigItem
{

    [Serializable]
    public class UpGradeCost
    {
        public int gold;
        public ItemPair[] items;
    }
    [Serializable]
    public class UpGradeInfo
    {
        public int level;
        public int grade;                       //星阶
        public int star;                        //星级
        public string mode;                     //当前阶段的模型
        public string fightMode;                //战斗中模型
        public string icon;                     //图标
        public string halfIcon;                 //图标
        public int stateMachine;                //状态机名字
        public int UIstateMachine;                //状态机名字
        public string fightAction;              //出战时演示动作
        public int skillID;                     //当前阶段的技能ID
        public UpGradeCost upgradeCost;
        public ItemAttachAttr[] attributes;    //累加的属性值
    }

    public UpGradeInfo[] upGradeInfos;
}//config PetUpGradeInfos : PetUpGradeInfo

[Serializable]
public class PetMood : ConfigItem
{
    public EnumPetMood mood;
    public int         moodValue;
}//config PetMoods : PetMood
[Serializable]
public class PetTask : ConfigItem
{
    /// <summary>
    /// 任务名称配置ID
    /// </summary>
    public int name;
    /// <summary>
    /// 任务描述配置ID
    /// </summary>
    public int desc;
    /// <summary>
    /// 图标名
    /// </summary>
    public string icon;
    /// <summary>
    /// 任务类型
    /// </summary>
    public byte type;
    /// <summary>
    /// 完成任务耗时,单位（分）
    /// </summary>
    public uint costTime;
    /// <summary>
    /// 奖励
    /// </summary>
    public TaskInfo.TaskStarReward reward;
    /// <summary>
    /// 平均等级限制
    /// </summary>
    public byte level;
    /// <summary>
    /// 等级限制最小等级
    /// </summary>
    public int minlv;
    /// <summary>
    /// 等级限制最大等级
    /// </summary>
    public int maxlv;
    /// <summary>
    /// 宠物数量最低限制
    /// </summary>
    public byte petCountMin;
    /// <summary>
    /// 任务难度
    /// </summary>
    public int diffcult;
    /// <summary>
    /// 每日可完成次数
    /// </summary>
    public int daily;
    /// <summary>
    /// 每周可完成次数
    /// </summary>
    public int week;
    /// <summary>
    /// 快速完成任务消耗金币
    /// </summary>
    public int fastCost;

    public int[] previewRewardItemId;

    /// <summary>
    /// 限制完成次数
    /// </summary>
    public int count { get { return Mathf.Max(daily, week); } }
}//config PetTasks : PetTask

[Serializable]
public class PetSkill : ConfigItem
{
    [Serializable]
    public class Skill
    {
        public int              skillID;
        public int              skillName;
        public string           skillIcon;
        public string           state;
        public int              cd;             //技能cd。单位毫秒
        public int              limitCount;     //单局使用次数限制
        public int[]            buffs;
        public int[]            initBuffs;

        public string GetDesc(int rLevel)
        {
            var str = string.Empty;
            for (int i = 0; i < buffs.Length; i++)
            {
                var info = ConfigManager.Get<BuffInfo>(buffs[i]);
                if (!info)
                {
                    Logger.LogError("请检查配置 宠物技能ID：{0} 中的buff{1}未在BuffInfo配置表中。", skillID, buffs[i]);
                    continue;
                }
                str += info.BuffDesc(rLevel);
            }
            return str;
        }
    }
    [Serializable]
    public class skillBuffParam
    {
        public int          buffId;
        public string       property;
        public double       value;
        public string       showValue;
    }

    public Skill[]          skills;

    public Skill GetSkill(int skillId)
    {
        for (int i = 0; i < skills.Length; i++)
        {
            if (skills[i].skillID == skillId)
                return skills[i];
        }
        Logger.LogError("无效的技能ID，请检查配置。ID = {0}", skillId);
        return null;
    }
}//config PetSkills : PetSkill
[Serializable]
public class Compound: ConfigItem
{
    public int sourceTypeId;                       //消耗的材料
    public int sourceNum;                          //消耗数量
    public int coin;                               //花费金币
    public int diamond;                            //花费钻石
    public ItemPair[] items;                       //得到的材料

}//config Compounds : Compound


public enum AwakeType
{
    None,
    Heart,
    Skill,
    Energy,
    Accompany,

    Max
}
[Serializable]
public class StateLevel
{
    public string   state;
    public int      level;
}

[Serializable]
public class AwakeInfo : ConfigItem
{
    public int protoId;                         //职业id
    public int name;
    public int desc;
    public string icon;
    public string lockEffect;                   //加锁特效
    public string unlockEffect;                 //解锁特效
    public int dependId;                        //依赖id
    public int dependlv;                        //依赖等级
    public AwakeType type;                      //觉醒类型
    public NpcTypeID npcId;                     //npcID
    public int layer;                           //层
    public int index;                           //序列
    public int reduceSoul;                      //减少魂之力上限
    public string statesString;                 //状态字符串
    public StateLevel[] states;                 //要修改的状态
    public double damageMul;                    //伤害因子增量 影响当前技能下的所有状态的所有攻击盒子产生的 AttackInfo 默认为 0
    public double additionTime;                 //觉醒加成时间
    public ItemAttachAttr[] attributes;         //属性加成
    public ItemAttachAttr[] awakeAttributes;    //觉醒状态属性加成
    public ItemPair cost;                       //觉醒消耗

    [NonSerialized]
    public List<AwakeInfo> nextInfoList;
    [NonSerialized]
    public string NameString;
    [NonSerialized]
    public string DescString;
    //一个状态可能对应多个觉醒点。每个觉醒点的等级属性都需要应用
    private static readonly Dictionary<int, List<int>> StateToID = new Dictionary<int, List<int>>();

    public static List<int> GetSkillIDList(int key)
    {
        if (StateToID.ContainsKey(key))
            return StateToID[key];
        return null;
    }

    public override void Initialize()
    {
        base.Initialize();
        if (!string.IsNullOrWhiteSpace(statesString))
        {
            var strs = statesString.Split(';');
            states = new StateLevel[strs.Length];
            for (var i = 0; i < states.Length; i++)
            {
                var ss = strs[i].Split(',');
                if (ss.Length >= 2)
                {
                    states[i] = new StateLevel() { state = ss[0], level = Util.Parse<int>(ss[1]) };
                }
            }
        }
        if (states != null && states.Length > 0)
        {
            foreach (var s in states)
            {
                var key = Animator.StringToHash(s.state); 
                if (!StateToID.ContainsKey(key))
                    StateToID.Add(key, new List<int>());
                StateToID[key].Add(ID);
            }
        }

        nextInfoList = ConfigManager.FindAll<AwakeInfo>(item => item.dependId == ID);
        
        List<object> p = new List<object>();
        if (attributes != null && attributes.Length > 0)
        {
            for (var i = 0; i < attributes.Length; i++)
            {
                p.Add(attributes[i].TypeString());
                p.Add(attributes[i].ValueString());
            }
        }

        if (awakeAttributes != null && awakeAttributes.Length > 0)
        {
            for (var i = 0; i < awakeAttributes.Length; i++)
            {
                p.Add(awakeAttributes[i].TypeString());
                p.Add(awakeAttributes[i].ValueString());
            }
        }

        if (additionTime > 0)
            p.Add(additionTime);

        if (damageMul > 0)
            p.Add(damageMul);

        if (reduceSoul > 0)
            p.Add(reduceSoul);

        try
        {
            NameString = Util.Format(ConfigText.GetDefalutString(name), p.ToArray());
            DescString = Util.Format(ConfigText.GetDefalutString(desc), p.ToArray());
        }
        catch
        {
            Logger.LogError("AwakeInfo::Initialize: 配置参数与字符串参数不匹配， ID = {0}  名字 = {1} 描述={2} 参数个数：{3}", ID, name, desc, p.Count);
        }
    }

}//config AwakeInfos : AwakeInfo
[Serializable]
public class Sentiment : ConfigItem
{
    public int sentimentTop;                       //人气值

}//config Sentiments : Sentiment

[Serializable]
public class BossStageInfo : ConfigItem
{
    public int bossId;
    public int diffcut;
    public int nameId;
    public int descId;
    public int bossHP;
    public int stageId;//关卡id
    public string bossIcon;
    public int[] reward;
}//config BossStageInfos : BossStageInfo
[Serializable]
public class BossBoxReward : ConfigItem
{
    public int descID;          //说明
    public int condition;       //显示的达成条件（血量百分比）
    public int boxPos;          //宝箱位置
    public int[] preview;   
}//config BossBoxRewards : BossBoxReward

[Serializable]
public class SoulInfo : ConfigItem
{
    public string icon;
    public string portrait;     //肖像。写真
    public string shodow;       
    public int nameId;          //名字ID
    public int descId;          //描述ID
    public ItemPair[] discardCosts; //保留原器灵消耗材料
}//config SoulInfos : SoulInfo

[Serializable]
public class SoulCost : ConfigItem
{
    public ItemPair[] costs;          
}//config SoulCosts : SoulCost

[Serializable]
public class SuitInfo : ConfigItem
{
    [Serializable]
    public class SkillEffect
    {
        public int    skillId;
        public double addtion;
    }
    public int nameId;
    public ItemPair[] costs;
    public int drawingId;
    public int[] effectDescs;
    public ItemPair[] clearCosts;
    public int   prevSuitId;
    public int[] nextSuitId;
    public int[] oneBuffs;
    public int[] twoBuffs;
    public int[] threeBuffs;
    public ItemAttachAttr[] oneAttributes;
    public ItemAttachAttr[] twoAttributes;
    public ItemAttachAttr[] threeAttributes;
    public SkillEffect[] oneSkillEffects;
    public SkillEffect[] twoSkillEffects;
    public SkillEffect[] threeSkillEffects;

    public SkillEffect[][] skillEffects { get; set; }
    public ItemAttachAttr[][] attributes { get; set; }
    public int[][] buffs { get; set; }

    public override void Initialize()
    {
        base.Initialize();
        skillEffects = new SkillEffect[3][];
        skillEffects[0] = oneSkillEffects;
        skillEffects[1] = twoSkillEffects;
        skillEffects[2] = threeSkillEffects;

        attributes = new ItemAttachAttr[3][];
        attributes[0] = oneAttributes;
        attributes[1] = twoAttributes;
        attributes[2] = threeAttributes;

        buffs = new int[3][];
        buffs[0] = oneBuffs;
        buffs[1] = twoBuffs;
        buffs[2] = threeBuffs;
    }

}//config SuitInfos : SuitInfo

[Serializable]
public class DropWindow : ConfigItem
{
    public string windowName;
    public int descID;
}//config DropWindows : DropWindow

[Serializable]
public class ColorGroupConfig : ConfigItem
{
    public static ColorGroup Default = new ColorGroup();
    public ColorGroup colorGroup;

    public override void Initialize()
    {
        base.Initialize();
        Default = colorGroup;
    }
}//config ColorGroupConfigs : ColorGroupConfig

[Serializable]
public class NpcPerfusionInfo : ConfigItem
{
    public int exp;
    public uint onceExp;
    public int buffId;
    public ItemPair[] costs;
}//config NpcPerfusionInfos : NpcPerfusionInfo
[Serializable]
public class TaskConfig : ConfigItem
{
    public int taskNameID;
    public string taskIconID;
    public string taskPressIcon;      //当任务被点击时显示的图片
    public int taskDescID;
    public int []markSceneID;         //约会任务场景小红点标记

    public int addMood;             //添加NPC心情值

    public int targetID;             //指定对象ID;这个地方如果NPC比较多的话，就需要更改

    public EnumTaskType taskType;    //是约会还是Npc

    public EnumTaskConditionType taskFinishType;

    public TaskFinishCondition[] taskFinishConditions;

    [Serializable]
    public class TaskFinishCondition
    {
        public EnumTaskCondition condition;
        public int value; //目标值
        public uint progress;//当前进度
    }

    public string desc { get { return ConfigText.GetDefalutString(taskDescID); } }

    public override void Initialize()
    {
        base.Initialize();
    }

}//config TaskConfigs : TaskConfig
[Serializable]
public class BossEffectInfo : ConfigItem
{
    [Serializable]
    public class money
    {
        public int itemId;        //货币id
        public int count;         //多少
    }

    public int nameId;
    public int descId;
    public string icon;
    public int effect;        //效果(/降低复活时间)
    public int count;         //可叠加次数
    public money[] cost;      //花费 
}//config BossEffectInfos : BossEffectInfo

#region NPC约会 by  HJC
[Serializable]
public class DialogueAnswersConfig : ConfigItem
{
    public int randAnswerCount;//需要随机出来的问题个数
    public int needCheckCount;//是否需要执行选择次数检查
    public float timeLimit;//时间限制
    public AnswerItem[] answerItems;  //问题组
    [Serializable]
    public class AnswerItem
    {
        public int id;
        public int nameId;   //对应文字列表
        public float[] goodFeelingLimits;   //好感度限制条件
        public float[] motionLimit; //心情限制条件
        public int addMotion; //增加心情值
        public EnumAnswerType answerType;
        public int rate;  //该问题的权重
    }
}//config DialogueAnswersConfigs : DialogueAnswersConfig

[Serializable]
public class DatingSceneConfig : ConfigItem
{
    public EnumNPCDatingSceneType sceneType;    //约会场景类型
    public int levelId;     //对应的场景ID
    public string uiBtnName;
    public string openWindow;
    public int sceneNameId;
    public int nameID;
    public int sceneDescID;
    public string sceneIcon;
    public string startTime;
    public string endTime;
    public int consumePower;//体力值
    public string strTopMarkImage;//二级场景界面上面显示的标签图标
    public string strBottomMarkImage;
    public int datingMapBuildConfigID;//对应的MapBuildConfig表id(sceneType)
}//config DatingSceneConfigs : DatingSceneConfig

[Serializable]
public class DatingMapBuildConfig : ConfigItem
{
    public string objectName;
    public string atlasImage;//小图，主要针对图集的
    public string bigImage;//大图
    public EnumDatingMapObjectType objectType;//物体类型，比如可点击建筑
    public ClickPolygonEdge[] polygonEdges;
    public string pressImage;//物体按下时显示的图片
    public Effect[] effects;

    [Serializable]
    public class ClickPolygonEdge
    {
        public int orderNumber;
        public Vector3 position;
    }

    [Serializable]
    public class Effect
    {
        public string effectNames;
        public string effectPath;
        public Vector3 position;
        public Vector3 scale = Vector3.one;
        public Vector3 rotation;
    }
}//config DatingMapBuildConfigs : DatingMapBuildConfig


[Serializable]
public class DatingEventInfo : ConfigItem
{
    public NewDatingEventType eventType;
    public int nameId;//事件名id
    public DatingEventItem[] datingEventItems;

    [Serializable]
    public class DatingEventItem
    {
        public int id;
        public int nameId;//事件名id
        public Condition[] conditions;//条件
        public Behaviour[] behaviours;//行为
    }

    [Serializable]
    public class Condition
    {
        public int id;
        public DatingEventConditionType conditionType;

        //Npc羁绊值
        public int[] fettersLimits;

        //Npc心情值
        public int[] moodLimits;

        //体力
        public int[] energyLimits;

        //剧情
        public int storyId;

        //answer
        public int answerId;
        public int answerItemId;
        public EnumAnswerType answerType;

        //settlement
        public byte settlementState;
        public byte settlementResultId;//约会结算的结果

        //settlement gift
        public byte giveGiftState;

        //占卜
        public int divinationResultId;
        public EnumDivinationType divinationType;

        //挑战
        public byte chaseResultState;

        //随机
        public int randomGroupId;//随机组id
        public int[] randomRange;

        //餐厅菜单
        public int orderItemId;//菜单食品id

    }

    [Serializable]
    public class Behaviour
    {
        public int id;
        public DatingEventBehaviourType behaviourType;

        //Story
        public int storyId;
        public EnumStoryType storyType = EnumStoryType.NpcTheatreStory; //对话类型
        public float showDialogueDelayTime; //显示对话需要的延迟时间
        public int[] preLoadRes;//需要提前预加载对话资源

        //OpenWindow
        public string openWindowName;

        //Mission
        public int createMissionId;
        public int notifyFinishMissionId;

        //TransitionEffect 过渡特效
        public string transitionEffectName;

        //Chase
        public int chaseTaskId;

        //问题表的id
        public int answerId;

        //创建随机数行为
        public int randomGroupId;
        public int[] randomRange;

        //约会中达成某个事件的名字文本id
        public int reachEventNameId;

        //执行的约会事件id
        public int datingEventId;

    }

}//config DatingEventInfos : DatingEventInfo

#endregion

[Auto]
[Serializable]
public class NpcClickBox : ConfigItem
{
    public NodeClickBox[] boxs;

    [Auto]
    [Serializable]
    public struct NodeClickBox
    {
        public NpcPosType npcPosType;
        public Vector3 position;
        public Vector3 euler;
        public Vector2 size;
    }
}//config NpcClickBoxs : NpcClickBox

[Serializable]
public class CardInfo : ConfigItem
{
    public int nameId;            //名称
    public string icon;           //图片
}//config CardInfos : CardInfo

[Serializable]
public class CardTypeInfo : ConfigItem
{
    public int point;             //牌型点数
    public int nameId;            //牌型名称
    public int descId;            //规则描述
    public int[] cardId;          //牌型示例
}//config CardTypeInfos : CardTypeInfo


[Serializable]
public class FactionKillRewardInfo : ConfigItem
{
    public int name;                    //连杀称号
    public int getMsg;
    public int overMsg;
    public string applique;                     //多杀的底图贴花
    public TaskInfo.TaskStarReward reward;      //展示奖励
    public bool isDisplay;

    public int kill
    {
        get { return ID; }
    }
}//config FactionKillRewardInfos : FactionKillRewardInfo

[Serializable]
public class FactionRankRewardInfo : ConfigItem
{
    public int rank;                    //排名
    public TaskInfo.TaskStarReward reward;      //展示奖励
}//config FactionRankRewardInfos : FactionRankRewardInfo 

[Serializable]
public class FactionResultRewardInfo : ConfigItem
{
    public TaskInfo.TaskStarReward reward;      //展示奖励
}//config FactionResultRewardInfos : FactionResultRewardInfo