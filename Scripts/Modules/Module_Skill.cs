/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-06-30
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class SkillData
{
    public SkillInfo skillInfo;
    public PSkill pskill;

    public SkillData(SkillInfo skill, PSkill _skill)
    {
        skillInfo = skill;
        pskill = _skill;
    }
}

public class Module_Skill : Module<Module_Skill>
{
    #region

    public const string ReLiveState = "StateRelive";

    public const string EventUpdateSkillPanel = "EventUpdateSkillPanel";//刷新技能面板
    public const string EventUpdateSkillPoint = "EventUpdateSkillPoint";//刷新技能点
    public const string EventBuySkillPointSuccess = "EventBuySkillPointSuccess";//购买技能点成功
    public const string EventUpdateSkillUp = "EventUpdateSkillUp";//刷新升级

    public ScSkillInfo skillInfo;

    private List<SkillData> allSkillDatas = new List<SkillData>();

    /// <summary>
    /// 所有技能的分类
    /// </summary>
    public Dictionary<SkillType, List<SkillData>> skillsDic { get { return m_skillsDic; } }
    private Dictionary<SkillType, List<SkillData>> m_skillsDic = new Dictionary<SkillType, List<SkillData>>();

    /// <summary>
    /// 当前武器所包含的技能升级列表
    /// </summary>
    public List<UpSkillInfo> skillLevelInfo { get { return m_skillLevelInfo; } }
    private List<UpSkillInfo> m_skillLevelInfo = new List<UpSkillInfo>();

    public SkillData currentClickSkill { get; set; }

    public SkillType currentSkillType { get; set; } = SkillType.None;
    /// <summary>
    /// 当前武器包含的状态等级字典 key是状态id value是状态等级
    /// </summary>
    private Dictionary<int, int> m_skillStateLevelDic = new Dictionary<int, int>();

    /// <summary>
    /// 状态对应技能 ID
    /// </summary>
    private Dictionary<int, int> m_stateToSkill = new Dictionary<int, int>();

    /// <summary>
    /// 当前武器当前元素类型对应的技能状态表
    /// </summary>
    private List<SkillToStateInfo> m_currentSkillStates = new List<SkillToStateInfo>();
    /// <summary>
    /// 复活技能id特殊处理
    /// </summary>
    public int reliveSkillId { get; private set; }

    public bool NeedNotice { get { return GetCanAddSkill(); } }
    public Dictionary<SkillType, bool> isRead { get; set; } = new Dictionary<SkillType, bool>();
    public bool ReadTrue
    {
        get
        {
            return readTrue;
        }
        set
        {
            readTrue = value;
            pointNoFull = skillInfo != null && moduleGlobal.system != null ? skillInfo.skillPoint < moduleGlobal.system.skillPointLimit : false;
        }
    }
    private bool readTrue;

    private bool pointNoFull;

    public int remainTime
    {
        get
        {
            var time = (int)m_remainTime - (int)(Time.realtimeSinceStartup - m_recevieTime);
            return time > 0 ? time : 0;
        }
    }
    private ulong m_remainTime;
    private float m_recevieTime;

    #endregion

    /// <summary>
    /// 获取指定状态对应的等级
    /// </summary>
    /// <returns></returns>
    public int GetStateLevel(string state)
    {
        return GetStateLevel(CreatureStateInfo.NameToID(state));
    }

    /// <summary>
    /// 获取指定状态对应的等级
    /// </summary>
    /// <returns></returns>
    public int GetStateLevel(int state)
    {
        return m_skillStateLevelDic.Get(state, -1);   // 如果该状态不属于任何技能  返回 -1
    }

    /// <summary>
    /// 获取指定状态对应的技能
    /// </summary>
    /// <returns></returns>
    public int GetStateSkill(string state)
    {
        return GetStateSkill(CreatureStateInfo.NameToID(state));
    }

    /// <summary>
    /// 获取指定状态对应的技能
    /// </summary>
    /// <returns></returns>
    public int GetStateSkill(int state)
    {
        return m_stateToSkill.Get(state, -1);   // 如果该状态不属于任何技能  返回 -1
    }

    private void OnGetAllSkillData()
    {
        if (skillInfo == null || skillInfo.skills.Length < 1) return;
        var upSkillinfos = ConfigManager.GetAll<UpSkillInfo>();
        if (upSkillinfos.Count < 1) return;

        m_skillLevelInfo.Clear();
        allSkillDatas.Clear();
        m_skillsDic.Clear();
        for (int i = 0; i < skillInfo.skills.Length; i++)
        {
            var _skillinfo = ConfigManager.Get<SkillInfo>(skillInfo.skills[i].skillId);
            if (!_skillinfo) continue;

            SkillType type = (SkillType)_skillinfo.skillType;
            if (!m_skillsDic.ContainsKey(type))
                m_skillsDic.Add(type, new List<SkillData>());

            SkillData skill = new SkillData(_skillinfo, skillInfo.skills[i]);
            allSkillDatas.Add(skill);
            m_skillsDic[type].Add(skill);

            var lists = upSkillinfos.FindAll(p => p.skillId == skillInfo.skills[i].skillId);
            m_skillLevelInfo.AddRange(lists);
        }

        foreach (var item in m_skillsDic)
        {
            item.Value.Sort((a, b) => a.skillInfo.skillPosition.CompareTo(b.skillInfo.skillPosition));
        }
    }

    public void OnAddDataToSkillStateDic()
    {
        var allSkillStates = ConfigManager.GetAll<SkillToStateInfo>();
        m_currentSkillStates.Clear();
        m_currentSkillStates = allSkillStates.FindAll(p =>
        {
            var skill_info = GetSkillInfoByID(p.skillId);
            if (!skill_info) return false;
            var propItemInfo = moduleEquip.weapon?.GetPropItem();
            if (propItemInfo != null)
            {
                var type = (WeaponSubType)propItemInfo.subType;
                var weaponAttr = ConfigManager.Get<WeaponAttribute>(moduleEquip.weapon.itemTypeId);
                if (!weaponAttr) return false;
                if (skill_info.protoId == modulePlayer.proto && skill_info.weaponType.Contains(type) && (p.elmentType == weaponAttr.elementType || p.elmentType == 0))
                    return true;
            }

            return false;
        });

        m_skillStateLevelDic.Clear();
        m_stateToSkill.Clear();

        List<string> states = new List<string>();
        for (int i = 0; i < m_currentSkillStates.Count; i++)
        {
            states.Clear();
            if (m_currentSkillStates[i].stateNames == null || m_currentSkillStates[i].addStates == null) continue;
            states.AddRange(m_currentSkillStates[i].stateNames);
            states.AddRange(m_currentSkillStates[i].addStates);

            for (int k = 0; k < states.Count; k++)
            {
                var stateID = CreatureStateInfo.NameToID(states[k]);
                if (!m_skillStateLevelDic.ContainsKey(stateID))
                {
                    var data = allSkillDatas.Find(p => p.pskill.skillId == m_currentSkillStates[i].skillId);
                    if (data == null)
                    {
                        Logger.LogError("can not Find id=={0} in config_upskillInfo ,please check out config_skillInfo", m_currentSkillStates[i].skillId);
                        continue;
                    }

                    m_skillStateLevelDic.Add(stateID, data.pskill.level);
                    m_stateToSkill.Add(stateID, m_currentSkillStates[i].skillId);
                }
                else
                    Logger.LogError("config_skillToState have same state={0} in different skillID", states[k]);
            }
        }
    }

    public double[] GetCurrentSkillDamge(int skillId, int level)
    {
        double[] defaultDouble = new double[] { 0, 0 };
        WeaponAttribute attr = ConfigManager.Get<WeaponAttribute>(moduleEquip.weapon.itemTypeId);
        if (!attr) return defaultDouble;
        SkillToStateInfo states = m_currentSkillStates.Find(p => p.skillId == skillId && (p.elmentType == attr.elementType || p.elmentType == 0));
        if (!states || states.stateNames == null || states.stateNames.Length < 1) return defaultDouble;

        double damage = 0;
        double spcail = 0;
        int weapon = moduleEquip.weapon.GetPropItem().subType;
        for (int i = 0; i < states.stateNames.Length; i++)
        {
            int stateID = CreatureStateInfo.NameToID(states.stateNames[i]);
            var _state = StateOverrideInfo.GetOverrideState(weapon, stateID, level);

            //int reliveID = CreatureStateInfo.NameToID(ReLiveState);
            //if (reliveID == stateID)
            //{
            //    reliveSkillId = states.skillId;
            //    if (_state.buffs.Length > 0)
            //    {
            //        defaultDouble[0] = (double)_state.buffs[0].duration / 1000;
            //        return defaultDouble;
            //    }
            //}

            for (int j = 0; j < _state.sections.Length; j++)
            {
                if (_state.sections[j].attackBox.isEmpty) continue;

                var attack = AttackInfo.Get(_state.sections[j].attackBox.attackInfo);
                if (attack == null || attack.isIgnoreDamge) continue;
                if (attack.execution)
                {
                    spcail += attack.damage;
                    continue;
                }
                damage += attack.damage;
            }

            int fly = _state.flyingEffects.Length;
            if (fly > 0)
            {
                for (int k = 0; k < fly; k++)
                {
                    var effect = ConfigManager.Get<FlyingEffectInfo>(_state.flyingEffects[k].effect);
                    if (effect == null) continue;

                    //统计hiteffect的伤害,必须满足subeffect中不包含相同的effectid
                    if (!effect.hitEffect.isEmpty)
                    {
                        StateMachineInfo.FlyingEffect exist = StateMachineInfo.FlyingEffect.empty;
                        if (effect.subEffects != null && effect.subEffects.Length > 0)
                            exist = effect.subEffects.Find(p => p.effect == effect.hitEffect.effect);

                        if (exist.isEmpty)
                        {
                            double[] hit = HitEffectDamage(effect.hitEffect);
                            damage += hit[0];
                            spcail += hit[1];
                        }
                    }

                    if (effect.subEffects != null && effect.subEffects.Length > 0)
                    {
                        double[] sub = SubEffectDamage(effect.subEffects);
                        damage += sub[0];
                        spcail += sub[1];
                    }

                    for (int p = 0; p < effect.sections.Length; p++)
                    {
                        if (effect.sections[p].attackBox.isEmpty) continue;

                        var attack = AttackInfo.Get(effect.sections[p].attackBox.attackInfo);
                        if (attack == null || attack.isIgnoreDamge) continue;
                        if (attack.execution)
                        {
                            spcail += attack.damage;
                            continue;
                        }
                        damage += attack.damage;
                    }
                }
            }
        }
        double _d = UpSkillInfo.GetOverrideDamage(skillId, level) * (1 + modulePlayer.GetSkillDamageAddtion(skillId));
        defaultDouble[0] = damage * (1 + _d);
        defaultDouble[1] = spcail * (1 + _d);
        return defaultDouble;
    }

    private double[] HitEffectDamage(StateMachineInfo.FlyingEffect hitfEffect)
    {
        double[] damage = { 0, 0 };
        if (hitfEffect.isEmpty) return damage;

        var hit = ConfigManager.Get<FlyingEffectInfo>(hitfEffect.effect);
        if (hit == null) return damage;

        double[] hitdamage = HitEffectDamage(hit.hitEffect);
        damage[0] += hitdamage[0];
        damage[1] += hitdamage[1];

        for (int i = 0; i < hit.sections.Length; i++)
        {
            if (hit.sections[i].attackBox.isEmpty) continue;

            var attack = AttackInfo.Get(hit.sections[i].attackBox.attackInfo);
            if (attack == null || attack.isIgnoreDamge) continue;
            if (attack.execution)
            {
                damage[1] += attack.damage;
                continue;
            }
            damage[0] += attack.damage;
        }

        if (hit.subEffects != null && hit.subEffects.Length > 0)
        {
            double[] sub = SubEffectDamage(hit.subEffects);
            damage[0] += sub[0];
            damage[1] += sub[1];
        }
        return damage;
    }

    private double[] SubEffectDamage(StateMachineInfo.FlyingEffect[] subEffects)
    {
        double[] damage = {0, 0};
        if (subEffects == null || subEffects.Length < 1) return damage;

        for (int i = 0; i < subEffects.Length; i++)
        {
            var effect = ConfigManager.Get<FlyingEffectInfo>(subEffects[i].effect);
            if(!effect)continue;

            if (!effect.hitEffect.isEmpty)
            {
                double[] hitdamages = HitEffectDamage(effect.hitEffect);
                damage[0] += hitdamages[0];
                damage[1] += hitdamages[1];
            }

            for (int j = 0; j < effect.sections.Length; j++)
            {
                if (effect.sections[j].attackBox.isEmpty) continue;

                var attack = AttackInfo.Get(effect.sections[j].attackBox.attackInfo);
                if (attack == null || attack.isIgnoreDamge) continue;
                if (attack.execution)
                {
                    damage[1] += attack.damage;
                    continue;
                }
                damage[0] += attack.damage;
            }

            if (effect.subEffects!=null&&effect.subEffects.Length>0)
            {
                double[] sub = SubEffectDamage(effect.subEffects);
                damage[0] += sub[0];
                damage[1] += sub[1];
            }
        }
        return damage;
    }


    private SkillInfo GetSkillInfoByID(int _id)
    {
        var info = ConfigManager.Get<SkillInfo>(_id);
        return info ? info : null;
    }

    protected override void OnGameDataReset()
    {
        skillInfo = null;
        m_skillsDic.Clear();
        currentClickSkill = null;
        currentSkillType = SkillType.None;
        m_skillLevelInfo.Clear();
        m_skillStateLevelDic.Clear();
        m_stateToSkill.Clear();
        m_currentSkillStates.Clear();
        reliveSkillId = 0;
        allSkillDatas.Clear();
        isRead.Clear();
        readTrue = false;
        pointNoFull = false;
        m_recevieTime = 0;
        m_remainTime = 0;
    }

    public void UpdateSkillPanel()
    {
        if (skillInfo == null)
        {
            SendSkillInfo();
            return;
        }

        DispatchModuleEvent(EventUpdateSkillPanel);
    }

    public void SendSkillInfo()
    {
        CsSkillInfo p = PacketObject.Create<CsSkillInfo>();
        session.Send(p);
    }

    void _Packet(ScSkillInfo p)
    {
        p.CopyTo(ref skillInfo);

        OnGetAllSkillData();
        //OnAddDataToSkillLvDic();
        OnAddDataToSkillStateDic();

        DispatchModuleEvent(EventUpdateSkillPanel);

        DispatchEvent(SDKEvent.GLOBAL, SDKEvent.PopSdk(SDKEvent.LOCAL_NOTIFY, SwitchType.SkillPoint));
    }

    void _Packet(ScSkillPointChange p)
    {
        if (skillInfo == null) return;

        ChangeSkillPoint(p.curSkillPoint);
        SetPointState();
    }

    public void SendBuySkillPoint()
    {
        CsSkillBuyPoint p = PacketObject.Create<CsSkillBuyPoint>();
        session.Send(p);
    }

    void _Packet(ScSkillBuyPoint p)
    {
        if (p.result != 0)
        {
            if (p.result == 1) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.SkillUIText, 20));
            else if (p.result == 2) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.SkillUIText, 21));
            return;
        }
        AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
        moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.SkillUIText, 19));
        if (skillInfo == null) return;

        skillInfo.buyTimes = p.times;
        ChangeSkillPoint(p.curSkillPoint);
        SetPointState();

        DispatchModuleEvent(EventBuySkillPointSuccess);
    }

    public void SendUpSkillLv(ushort _id)
    {
        CsSkillAddPoint p = PacketObject.Create<CsSkillAddPoint>();
        p.skillId = _id;
        session.Send(p);
    }

    void _Packet(ScSkillAddPoint p)
    {
        if (p.result != 0)
        {
            if (p.result == 1) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.SkillUIText, 23));
            else if (p.result == 2) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.SkillUIText, 24));
            else if (p.result == 3) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.SkillUIText, 25));
            else if (p.result == 4) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.SkillUIText, 26));
            return;
        }

        var skill = allSkillDatas.Find(o => o.pskill.skillId == p.skillId);
        if (skill == null)
        {
            var s = ConfigManager.Get<SkillInfo>(p.skillId);
            if(s != null)
                Logger.LogError("uplevel a no exist skill,skillid={0}",p.skillId);
            return;
        }

        skill.pskill.level = p.curLv;
        ChangeSkillPoint(p.curSkillPoint);
        SetPointState();

        OnAddDataToSkillStateDic();
        DispatchModuleEvent(EventUpdateSkillUp);
    }

    void ChangeSkillPoint(ushort curPoint)
    {
        if (skillInfo == null) return;
        skillInfo.skillPoint = curPoint;

        DispatchModuleEvent(EventUpdateSkillPoint);
    }

    public bool GetIsCurrentLock(SkillData info)
    {
        return info.skillInfo.unLockLv > 0 && info.pskill.level == 0;
    }

    public bool GetCurrentTypeState(SkillType type)
    {
        if (!m_skillsDic.ContainsKey(type)) return false;
        var list = m_skillsDic[type];

        var data = list.Find(p => p.skillInfo.isZxc == 1);
        var read = isRead.ContainsKey(type) ? isRead[type] : false;
        if (data == null || read) return false;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].skillInfo.isZxc != 1) continue;

            bool isLockNow = GetIsCurrentLock(list[i]);
            int lv = list[i].pskill.level;
            lv = isLockNow ? 1 : lv;

            var upInfo = m_skillLevelInfo.Find(p => p.skillId == list[i].pskill.skillId && p.skillLevel == lv);
            var nextInfo = m_skillLevelInfo.Find(p => p.skillId == list[i].pskill.skillId && p.skillLevel == lv + 1);
            if (!upInfo || !nextInfo) continue;

            int level = isLockNow ? upInfo.needLv : nextInfo.needLv;
            int money = isLockNow ? upInfo.expendGold : nextInfo.expendGold;
            int sp = isLockNow ? upInfo.expendSp : nextInfo.expendSp;
            if (modulePlayer.roleInfo.level >= level && modulePlayer.roleInfo.level >= list[i].skillInfo.unLockLv
                && modulePlayer.coinCount >= money && skillInfo != null && skillInfo.skillPoint >= sp) return true;
        }
        return false;
    }

    private bool GetCanAddSkill()
    {
        if (skillInfo == null) return false;

        if (ReadTrue) return false;

        bool isNoMax = false;
        foreach (var item in m_skillsDic)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                bool isLockNow = GetIsCurrentLock(item.Value[i]);
                int lv = item.Value[i].pskill.level;
                var nextInfo = m_skillLevelInfo.Find(p => p.skillId == item.Value[i].pskill.skillId && p.skillLevel == lv + 1);
                if (nextInfo != null || isLockNow)
                {
                    isNoMax = true;
                    break;
                }
            }
            bool isTrue = GetCurrentTypeState(item.Key);
            if (isTrue) return isTrue;
        }

        if (skillInfo.skillPoint >= moduleGlobal.system.skillPointLimit && isNoMax) return true;
        return false;
    }

    private void SetPointState()
    {
        if (skillInfo == null || moduleGlobal.system == null) return;

        if (skillInfo.skillPoint >= moduleGlobal.system.skillPointLimit && pointNoFull)
        {
            var window = Window.GetOpenedWindow<Window_Skill>();
            if (window && window.actived) return;
            readTrue = false;
        }
    }

    void _Packet(ScRoleLevelUp p)
    {
        moduleHome.UpdateIconState(HomeIcons.Skill, NeedNotice);
    }

    void _Packet(ScRoleMoneyInfo p)
    {
        moduleHome.UpdateIconState(HomeIcons.Skill, NeedNotice);
    }

    void _Packet(ScSkillPointRemainTimes p)
    {
        if (p.remainTimes == 0) return;
        m_recevieTime = Time.realtimeSinceStartup;
        m_remainTime = p.remainTimes;

        if (skillInfo != null)
            DispatchEvent(SDKEvent.GLOBAL, SDKEvent.PopSdk(SDKEvent.LOCAL_NOTIFY, SwitchType.SkillPoint));
    }
}