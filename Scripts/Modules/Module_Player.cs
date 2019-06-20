/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Player module
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-08-29
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;

public class Module_Player : Module<Module_Player>
{
    #region Events

    public const string EventUpdateBuffList        = "EventPlayerUpdateBuffList";
    public const string EventUpdateBulletCount     = "EventPlayerUpdateBulletCount";
    public const string EventUpdateEnergy          = "EventPlayerUpdateEnergy";
    public const string EventFatigueChanged        = "EventPlayerFatigueChanged";//体力改变
    public const string EventBuySuccessFatigue     = "EventBuySuccessFatigue";//购买体力结果
    public const string EventBuySuccessCoin        = "EventBuySuccessCoin";//购买金币成功
    public const string EventBuySuccessSweep       = "EventBuySuccessSweep";//购买扫荡卷成功
    public const string EventHaveExpBuffList       = "EventHaveExpBuffList";//有经验卡加成的时候
    public const string EventNoExpBuffList         = "EventNoExpBuffList";//没有经验卡加成的时候

    public const string EventCombatFightChnage     = "EventCombatFightChnage";//战斗力发生更改

    /// <summary>
    /// 玩家货币数量更改 param1                    = CurrencySubType  param2 = 变化的数量
    /// </summary>
    public const string EventCurrencyChanged       = "EventPlayerCurrencyChanged";
    /// <summary>
    /// 玩家经验变化时触发
    /// param1: 旧的经验值 param2: 新的经验值
    /// </summary>
    public const string EventExpChanged            = "EventPlayerExpChanged";
    /// <summary>
    /// 玩家等级变化时触发
    /// param1: 旧的等级 param2: 新的等级
    /// </summary>
    public const string EventLevelChanged          = "EventPlayerLevelChanged";
    /// <summary>
    /// 玩家名称变化时
    /// param1: 旧的名字 param2: 新的名称
    /// </summary>
    public const string EventNameChanged           = "EventPlayerNameChanged";
    /// <summary>
    /// 玩家头像变化时
    /// param1: 旧的头像 param2: 新的头像
    /// </summary>
    public const string EventAvatarChanged         = "EventPlayerAvatarChanged";
    /// <summary>
    /// 成功使用道具
    /// </summary>
    public const string EventUseProp               = "EventUseProp";
    #endregion

    #region Public role info

    public int gender
    {
        get
        {
            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            return m_lockedGender < 0 ? m_gender : m_lockedGender;
            #else
            return m_gender;
            #endif
        }
    }
    public int proto
    {
        get
        {
            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            return m_lockedClass < 1 ? m_proto : m_lockedClass;
            #else
            return m_proto;
            #endif
        }
    }
    public ulong id_ { get { return m_roleInfo.roleId; } }
    public string name_ { get { return m_roleInfo.roleName; } }
    public int level { get { return m_roleInfo.level; } }
    /// <summary>
    /// 玩家当前头像
    /// </summary>
    public string avatar { get { return m_avatar; } }
    /// <summary>
    /// 玩家职业头像
    /// </summary>
    public string classAvatar { get { return m_classAvatar; } }
    /// <summary>
    /// 玩家当前头像框
    /// </summary>
    public int avatarBox { get { return m_roleInfo.headBox; } }

    public PRoleInfo roleInfo { get { return m_roleInfo; } }

    public int[] buffs { get { return m_buffs; } }
    public int bulletCount { get { return m_bulletCount; } }
    public int energy { get { return m_energy; } }
    public int maxEnergy { get { return m_maxEnergy; } }

    public CreatureElementTypes elementType
    {
        get
        {
            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            return m_lockedElementType == CreatureElementTypes.Count ? m_elementType : m_lockedElementType;
            #else
            return m_elementType;
            #endif
        }
    }

    public uint pvpTimes
    {
        get { return m_roleInfo.pvpTimes; }
        set
        {
            m_roleInfo.pvpTimes = value;

            Module_Guide.CreatePVPChanllengeCondition(m_roleInfo.pvpTimes > 0);
        }
    }

    public bool isLevelUp { get; set; }

    public bool isHaveExpCard { get; private set; }

    public bool isClickExpBtn { get; set; }

    public long fatigueRemainTime
    {
        get
        {
            var time = m_remainTime - (long)(Time.realtimeSinceStartup - m_recevieTime);
            return time > 0 ? time : 0;
        }
    }
    private long m_remainTime;
    private float m_recevieTime;
    #endregion

    #region Internal

    private int m_proto;
    private int m_gender;
    private int[] m_buffs;
    private PSkill[] m_skills;
    private int m_bulletCount;
    private int m_energy;
    private int m_maxEnergy;
    private int m_awakeDuration;
    private string m_avatar;
    private string m_classAvatar;
    private PRoleInfo m_roleInfo;
    private CreatureElementTypes m_elementType;

    protected override void OnModuleCreated()
    {
        m_roleInfo = PacketObject.Create<PRoleInfo>();
        m_buffs = new int[] { };
        m_skills = new PSkill[] { };

        m_proto         = m_roleInfo.roleProto;
        m_gender        = m_roleInfo.gender;
        m_bulletCount   = 5;
        m_energy        = 0;
        m_maxEnergy     = 100;
        m_coinCount     = 0;
        m_gemCount      = 0;
        m_wishCoinCount = 0;

        #region Debug
        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        EventManager.AddEventListener("OnGmLockClass", OnGMLockClass);
        #endif
        #endregion
    }

    public double GetSkillDamageAddtion(int skillId)
    {
        var skill = Array.Find(m_skills, item => item.skillId == skillId);
        return skill?.addition * 0.0001 ?? 0;
    }

    #endregion

    #region Currency

    private uint m_coinCount, m_gemCount, m_wishCoinCount, m_petSummonStone, m_heartSoul,m_friendPoint, m_unionContribution;

    /// <summary>
    /// 玩家金币数量
    /// </summary>
    public uint coinCount { get { return m_coinCount; } }
    /// <summary>
    /// 玩家钻石数量
    /// </summary>
    public uint gemCount { get { return m_gemCount; } }
    /// <summary>
    /// 玩家许愿币数量
    /// </summary>
    public uint wishCoinCount { get { return m_wishCoinCount; } }

    public uint petSummonStone { get { return m_petSummonStone; } }

    public uint heartSoul { get { return m_heartSoul; } }
    /// <summary>
    /// 货币友情点数量
    /// </summary>
    public uint friendPoints { get { return m_friendPoint; } }
    /// <summary>
    /// 货币公会贡献值
    /// </summary>
    public uint unionContribution { get { return m_unionContribution; } }

    /// <summary>
    /// 体力上限
    /// </summary>
    public ushort maxFatigue
    {
        get
        {
            if (m_maxFatigue == 0) m_maxFatigue = moduleGlobal.system.fatigueLimit;
            return m_maxFatigue;
        }
    }
    private ushort m_maxFatigue;

    public uint GetMoneyCount(CurrencySubType type)
    {
        switch (type)
        {
            case CurrencySubType.Gold:     return m_coinCount;
            case CurrencySubType.Diamond:  return m_gemCount;
            case CurrencySubType.Emblem:   return (uint)moduleEquip.signetCount;
            case CurrencySubType.WishCoin: return m_wishCoinCount;
            case CurrencySubType.RiskBi:   return (uint)moduleEquip.adventureCount;
            case CurrencySubType.PetSummonStone:    return m_petSummonStone;
            case CurrencySubType.HeartSoul: return m_heartSoul;
            case CurrencySubType.FriendPoint: return m_friendPoint;
            case CurrencySubType.UnionContribution: return m_unionContribution;
            default:                       return 0;
        }
    }

    // 玩家模块总是最优先捕获游戏货币变化消息
    void _Packet_999(ScRoleMoneyInfo p)
    {
        var type = (CurrencySubType)p.type;

        uint old = 0;
        switch (type)
        {
            case CurrencySubType.Gold:
                old = m_roleInfo.coin;
                m_roleInfo.coin = p.num;
                m_coinCount = p.num;
                if (m_coinCount > old && moduleForging.InsoulItem != null)
                {
                    moduleForging.InsoulGold += (int)(m_coinCount - old);
                }
                break;
            case CurrencySubType.Diamond:
                old = m_roleInfo.diamond;
                m_roleInfo.diamond = p.num;
                m_gemCount = p.num;
                break;
            case CurrencySubType.WishCoin:
                old = m_roleInfo.wishCoin;
                m_roleInfo.wishCoin = p.num;
                m_wishCoinCount = p.num;
                break;
            case CurrencySubType.PetSummonStone:
                old = m_roleInfo.summonStone;
                m_roleInfo.summonStone = p.num;
                m_petSummonStone = p.num;
                break;
            case CurrencySubType.HeartSoul:
                old = m_roleInfo.mind;
                m_roleInfo.mind = p.num;
                m_heartSoul = p.num;
                break;
            case CurrencySubType.FriendPoint:
                old = m_roleInfo.friendShipPoint;
                m_roleInfo.friendShipPoint = p.num;
                m_friendPoint = p.num;
                break;
            case CurrencySubType.UnionContribution:
                old = m_roleInfo.unionContribution;
                m_roleInfo.unionContribution = p.num;
                m_unionContribution = p.num;
                break;
        }

        DispatchModuleEvent(EventCurrencyChanged, type, (int)p.num - (int)old);
    }

    #endregion

    #region Role

    public CreatureInfo BuildPlayerInfo(PRoleSummary rRole)
    {
        var info = ConfigManager.Get<CreatureInfo>(rRole.proto);

        if (!info)
        {
            Logger.LogError("Module_Player::BuildPlayerInfo: Build CreatureInfo from PMatchInfo failed, could not find proto template config: {0}", rRole.proto);
            return null;
        }

        info = info.Clone<CreatureInfo>();

        var w = WeaponInfo.GetWeapon(0, rRole.fashion.weapon);
        var ww = WeaponInfo.GetWeapon(0, rRole.fashion.gun);

        if (w.isEmpty)
            Logger.LogError("Creature::BuildPlayerInfo: MatchInfo has invalid weapon item:<b>[{0}]</b>", rRole.fashion.weapon);

        if (ww.isEmpty)
            Logger.LogError("Creature::BuildPlayerInfo: MatchInfo has invalid off weapon item:<b>[{0}]</b>", rRole.fashion.gun);

        info.weaponID = w.weaponID;
        info.offWeaponID = ww.weaponID;
        info.weaponItemID = rRole.fashion.weapon;
        info.offWeaponItemID = rRole.fashion.gun;

        return info;
    }

    public CreatureInfo BuildPlayerInfo(int configID = -1, bool playerBuff = true, bool playerSkill = true)
    {
        if (configID < 0) configID = proto;
        var info = ConfigManager.Get<CreatureInfo>(configID);

        if (!info)
        {
            Logger.LogError("Module_Player::BuildPlayerInfo: Build CreatureInfo failed, could not find template config: {0}", configID);
            return null;
        }

        info = moduleAttribute.attrInfo.ToCreatureInfo(info.Clone<CreatureInfo>());
        moduleAttribute.awakeChangeAttr.CopyTo(ref info.awakeChangeAttr);

        info.elementType     = (int)elementType;
        info.weaponID        = moduleEquip.weaponID;
        info.offWeaponID     = moduleEquip.offWeaponID;
        info.weaponItemID    = moduleEquip.weaponItemID;
        info.offWeaponItemID = moduleEquip.offWeaponItemID;
        info.bulletCount     = m_bulletCount;
        info.energy          = m_energy;
        info.maxEnergy       = m_maxEnergy;
        info.awakeDuration   = m_awakeDuration;

        if (playerBuff) info.buffs = m_buffs.SimpleClone();
        else info.buffs = null;

        if (playerSkill && m_skills != null && m_skills.Length > 0)
        {
            PSkill[] skills = null;
            m_skills.CopyTo(ref skills);
            info.skills = skills;
        }
        else info.skills = null;

        return info;
    }

    public CreatureInfo BuildPlayerInfo(PMatchInfo matchInfo, bool playerBuff = true, bool playerSkill = true)
    {
        var info = ConfigManager.Get<CreatureInfo>(matchInfo.roleProto);

        if (!info)
        {
            Logger.LogError("Module_Player::BuildPlayerInfo: Build CreatureInfo from PMatchInfo failed, could not find proto template config: {0}", matchInfo.roleProto);
            return null;
        }

        info = matchInfo.attrInfo.ToCreatureInfo(info.Clone<CreatureInfo>());

        var w  = WeaponInfo.GetWeapon(0, matchInfo.fashion.weapon);
        var ww = WeaponInfo.GetWeapon(0, matchInfo.fashion.gun);

        if (w.isEmpty)
            Logger.LogError("Creature::BuildPlayerInfo: MatchInfo has invalid weapon item:<b>[{0}]</b>", matchInfo.fashion.weapon);

        if (ww.isEmpty)
            Logger.LogError("Creature::BuildPlayerInfo: MatchInfo has invalid off weapon item:<b>[{0}]</b>", matchInfo.fashion.gun);

        if(matchInfo.currentHp > 0)
            info.health          = matchInfo.currentHp;
        info.elementType     = matchInfo.elementType;
        info.weaponID        = w.weaponID;
        info.offWeaponID     = ww.weaponID;
        info.weaponItemID    = matchInfo.fashion.weapon;
        info.offWeaponItemID = matchInfo.fashion.gun;
        info.bulletCount     = matchInfo.bulletCount;
        info.energy          = matchInfo.energy;
        info.maxEnergy       = matchInfo.maxEnergy;
        info.awakeDuration   = matchInfo.awakeDuration;
        matchInfo.awakeChangeAttr.CopyTo(ref info.awakeChangeAttr);

        if (playerBuff) info.buffs = matchInfo.buffs.SimpleClone();
        else info.buffs = null;

        if (playerSkill && matchInfo.skills != null && matchInfo.skills.Length > 0)
        {
            PSkill[] skills = null;
            matchInfo.skills.CopyTo(ref skills);
            info.skills = skills;
        }
        else info.skills = null;

        return info;
    }

    public CreatureInfo BuildPlayerInfo(PTeamMemberInfo memberInfo, bool playerBuff = true, bool playerSkill = true)
    {
        var info = ConfigManager.Get<CreatureInfo>(memberInfo.roleProto);

        if (!info)
        {
            Logger.LogError("Module_Player::BuildPlayerInfo: Build CreatureInfo from PTeamMemberInfo failed, could not find proto template config: {0}", memberInfo.roleProto);
            return null;
        }

        info = memberInfo.attrInfo.ToCreatureInfo(info.Clone<CreatureInfo>());

        var w  = WeaponInfo.GetWeapon(0, memberInfo.fashion.weapon);
        var ww = WeaponInfo.GetWeapon(0, memberInfo.fashion.gun);

        if (w.isEmpty)
            Logger.LogError("Creature::BuildPlayerInfo: MatchInfo has invalid weapon item:<b>[{0}]</b>", memberInfo.fashion.weapon);

        if (ww.isEmpty)
            Logger.LogError("Creature::BuildPlayerInfo: MatchInfo has invalid off weapon item:<b>[{0}]</b>", memberInfo.fashion.gun);

        info.elementType     = memberInfo.elementType;
        info.weaponID        = w.weaponID;
        info.offWeaponID     = ww.weaponID;
        info.weaponItemID    = memberInfo.fashion.weapon;
        info.offWeaponItemID = memberInfo.fashion.gun;
        info.bulletCount     = memberInfo.bulletCount;
        info.energy          = memberInfo.energy;
        info.maxEnergy       = memberInfo.maxEnergy;
        info.awakeDuration   = memberInfo.awakeDuration;
        memberInfo.awakeChangeAttr.CopyTo(ref info.awakeChangeAttr);

        if (playerBuff) info.buffs = memberInfo.buffs.SimpleClone();
        else info.buffs = null;

        if (playerSkill && memberInfo.skills != null && memberInfo.skills.Length > 0)
        {
            PSkill[] skills = null;
            memberInfo.skills.CopyTo(ref skills);
            info.skills = skills;
        }
        else info.skills = null;

        return info;
    }

    public CreatureInfo BuildPlayerInfo(PMatchProcessInfo matchInfo, bool playerBuff = true, bool playerSkill = true)
    {
        var info = ConfigManager.Get<CreatureInfo>(matchInfo.roleProto);

        if (!info)
        {
            Logger.LogError("Module_Player::BuildPlayerInfo: Build CreatureInfo from PMatchInfo failed, could not find proto template config: {0}", matchInfo.roleProto);
            return null;
        }

        info = info.Clone<CreatureInfo>();

        var w = WeaponInfo.GetWeapon(0, matchInfo.fashion.weapon);
        var ww = WeaponInfo.GetWeapon(0, matchInfo.fashion.gun);

        if (w.isEmpty)
            Logger.LogError("Creature::BuildPlayerInfo: MatchInfo has invalid weapon item:<b>[{0}]</b>", matchInfo.fashion.weapon);

        if (ww.isEmpty)
            Logger.LogError("Creature::BuildPlayerInfo: MatchInfo has invalid off weapon item:<b>[{0}]</b>", matchInfo.fashion.gun);

        info.weaponID = w.weaponID;
        info.offWeaponID = ww.weaponID;
        info.weaponItemID = matchInfo.fashion.weapon;
        info.offWeaponItemID = matchInfo.fashion.gun;
        info.buffs = null;

        return info;
    }

    /// <summary>
    /// 获取玩家经验条进度
    /// 经验条进度每 10% 作为分段，即结果只包含 0.1, 0.2, 0.3 ... 0.9, 1.0
    /// </summary>
    /// <returns></returns>
    public float GetExpBarProgress()
    {
        return GetExpBarProgress(m_roleInfo.level, m_roleInfo.expr);
    }

    /// <summary>
    /// 获取指定等级在指定经验下的进度条进都
    /// 经验条进度每 10% 作为分段，即结果只包含 0.1, 0.2, 0.3 ... 0.9, 1.0
    /// </summary>
    /// <returns></returns>
    public float GetExpBarProgress(int level, uint exp)
    {
        var p = CreatureEXPInfo.GetExpGrogressInt(level, exp) + 5;
        return (p / 10) * 0.1f;
    }

    /// <summary>
    /// 获取当前玩家的经验值进度 0 - 1.0
    /// </summary>
    /// <returns></returns>
    public float GetExpProgress()
    {
        return GetExpProgress(roleInfo.level, roleInfo.expr);
    }

    /// <summary>
    /// 获取指定等级在指定经验下的经验进度
    /// </summary>
    /// <param name="level"></param>
    /// <param name="exp"></param>
    /// <returns></returns>
    public float GetExpProgress(int level, uint exp)
    {
        return CreatureEXPInfo.GetExpGrogress(level, exp);
    }

    public void RequestBuffs()
    {
        session.Send(PacketObject.Create<CsRoleBuffList>());
    }

    public void GMCreatePlayer(ScRoleInfo p)
    {
#if UNITY_EDITOR
        _Packet_999(p);
#endif
    }

    // 玩家模块总是最优先捕获游戏货币变化消息
    void _Packet_999(ScRoleInfo p)
    {
        p.role.CopyTo(ref m_roleInfo);
        
        m_proto          = m_roleInfo.roleProto;
        m_gender         = m_roleInfo.gender;
        m_coinCount      = m_roleInfo.coin;
        m_gemCount       = m_roleInfo.diamond;
        m_wishCoinCount  = m_roleInfo.wishCoin;
        m_petSummonStone = m_roleInfo.summonStone;
        m_heartSoul      = m_roleInfo.mind;
        m_friendPoint    = m_roleInfo.friendShipPoint;
        m_classAvatar    = Creature.GetClassAvatarName(proto);
        m_unionContribution = m_roleInfo.unionContribution;

        GetSignBanSate();

        var oa = m_avatar;
        m_avatar = m_roleInfo.avatar;
        if (oa != m_avatar)
        {
            DispatchModuleEvent(EventAvatarChanged, oa, m_avatar);
            DispatchEvent(EventAvatarChanged, Event_.Pop(oa, m_avatar));  // Used for external listeners
        }

        moduleStory.OnRecvRoleInfo();
    }

    void _Packet(ScRoleBuffList p)
    {
        m_buffs = p.buffs;

        DispatchModuleEvent(EventUpdateBuffList);
    }

    void _Packet_999(ScSkillInfo p)
    {
        if (p.skills == null) m_skills = new PSkill[] { };
        else p.skills.CopyTo(ref m_skills);
    }

    void _Packet_999(ScSkillAddtionChange p)
    {
        if (p.skills == null) m_skills = new PSkill[] { };
        else p.skills.CopyTo(ref m_skills);
    }

    void _Packet_999(ScSkillAddPoint p)
    {
        if (p.result != 0) return;

        var skill = m_skills?.Find(s => s.skillId == p.skillId);
        if (skill == null)
        {
            var s = ConfigManager.Get<SkillInfo>(p.skillId);
            //觉醒天赋
            if (s == null)
            {
                if (m_skills != null)
                    Array.Resize(ref m_skills, m_skills.Length + 1);
                else
                    m_skills = new PSkill[1];
                m_skills[m_skills.Length - 1] = PacketObject.Create<PSkill>();
                m_skills[m_skills.Length - 1].skillId = p.skillId;
                m_skills[m_skills.Length - 1].level = p.curLv;
            }
            return;
        }

        skill.level = p.curLv;
    }

    void _Packet(ScRoleBulletCount p)
    {
        m_bulletCount = p.bulletCount;

        DispatchModuleEvent(EventUpdateBulletCount);
    }

    void _Packet(ScRoleEnergyInfo p)
    {
        m_energy    = p.energy;
        m_maxEnergy = p.maxEnergy;
        m_awakeDuration = p.awakeDuration;

        DispatchModuleEvent(EventUpdateEnergy);
    }

    void _Packet(ScRoleElementType p)
    {
        m_elementType = (CreatureElementTypes)p.elementType;
    }

    void _Packet(ScRoleExprChange p)
    {
        var old = m_roleInfo.expr;
        m_roleInfo.expr = p.expr;

        DispatchModuleEvent(EventExpChanged, old, m_roleInfo.expr);
    }

    void _Packet_999(ScRoleLevelUp p)
    {
        var oldLv = m_roleInfo.level;
        m_roleInfo.level = (byte)p.level;

        var oldPoint = m_roleInfo.attrPoint;
        m_roleInfo.attrPoint += p.attrPoint;

        var oldMaxFatigue = m_maxFatigue;
        m_maxFatigue = p.fatigue;
        var oldFatigue = m_roleInfo.fatigue;
        ChangeFatigue(true, p.addFatigue);

        isLevelUp = true;

        DispatchModuleEvent(EventLevelChanged, oldLv, oldPoint, oldFatigue, oldMaxFatigue);
        DispatchEvent(EventLevelChanged, Event_.Pop(oldLv, oldPoint, oldFatigue, oldMaxFatigue));
    }

    void _Packet(ScRoleNameUpdate p)
    {
        if (p.result == 0)
        {
            var old = m_roleInfo.roleName;
            m_roleInfo.roleName = p.newName;
            DispatchModuleEvent(EventNameChanged, old, m_roleInfo.roleName);
        }
    }

    void _Packet(ScRoleAvatarChange p)
    {
        var old = m_avatar;
        m_avatar = p.avatar;

        DispatchModuleEvent(EventAvatarChanged, old, m_roleInfo.roleName);
        DispatchEvent(EventAvatarChanged);  // Used for external listeners
    }

    #endregion

    #region functions about fatigue

    public void SendBuyFatigue()
    {
        CsRoleBuyFatigue p = PacketObject.Create<CsRoleBuyFatigue>();
        session.Send(p); 
    }

    void _Packet(ScRoleBuyFatigue p)
    {
        if (p.result == 0)
        {
            AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
            if(moduleChase.chaseInfo!=null) moduleChase.chaseInfo.fatigueBuyNum++;
            ChangeFatigue(false, p.curFatigue);
        }
        DispatchModuleEvent(EventBuySuccessFatigue);
        moduleGlobal.ShowMessage(ConfigText.GetDefalutString(404,p.result));
    }

    void _Packet(ScRoleFatigueChange p)
    {
        ChangeFatigue(false, p.curFatigue);
    }

    private void ChangeFatigue(bool isAdd, ushort value)
    {
        m_roleInfo.fatigue = isAdd ? (ushort)(m_roleInfo.fatigue + value) : value;

        DispatchModuleEvent(EventFatigueChanged);
    }

    void _Packet(ScFatigueRemainTimes p)
    {
        m_remainTime = (long)p.remainTimes;
        m_recevieTime = Time.realtimeSinceStartup;

        DispatchEvent(SDKEvent.GLOBAL, SDKEvent.PopSdk(SDKEvent.LOCAL_NOTIFY, SwitchType.Fatigue));
    }

    protected override void OnGameDataReset()
    {
        m_maxFatigue = 0;
        m_remainTime = 0;
        isLevelUp = false;

        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        m_lockedClass = -1;
        m_lockedGender = -1;
        m_lockedElementType = CreatureElementTypes.Count;
        #endif
    }

    #endregion

    #region 兑换金币

    public void SendBuyCoin()
    {
        CsRoleBuyCoin p = PacketObject.Create<CsRoleBuyCoin>();
        session.Send(p);
    }

    void _Packet(ScRoleBuyCoin p)
    {
        if (p.result == 1) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.GlobalUIText,4));
        else if (p.result == 2) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.PublicUIText, 21));
        else
        {
            AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
            m_roleInfo.coin = p.currentCoin;
            m_roleInfo.dayBuyCoinTimes++;
            DispatchModuleEvent(EventBuySuccessCoin);
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.PublicUIText, 7));
        }
    }

    public void SendBuySweep()
    {
        var p = PacketObject.Create<CsBuyMoppingUp>();
        session.Send(p);
    }

    void _Packet(ScBuyMoppingUp p)
    {
        if (p.result == 0)
        {
            AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
            m_roleInfo.dayBuySweepTimes++;
            DispatchModuleEvent(EventBuySuccessSweep);
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.MoppingUpUI, 8));
        }
        else
            moduleGlobal.ShowMessage(9606, p.result);
    }
    #endregion

    #region 使用道具包

    public void SendUsePropBag(ulong m_itemId,ushort m_num)
    {
        CsRoleUseProp p = PacketObject.Create<CsRoleUseProp>();
        p.itemId = m_itemId;
        p.num = m_num;
        session.Send(p);
    }

    void _Packet(ScRoleUseProp p)
    {
        switch (p.result)
        {
            case 0:
                //AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
                if (moduleGlobal.currentOpenInfo.itemType == PropType.Sundries &&
                    moduleGlobal.currentOpenInfo.subType == (int)UseablePropSubType.MonthCard ||
                    moduleGlobal.currentOpenInfo.subType == (int)UseablePropSubType.QuarterCard ||
                    moduleGlobal.currentOpenInfo.subType == (int)UseablePropSubType.ExpCardProp)
                    moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.WarehouseUiText, 0));
                DispatchModuleEvent(EventUseProp, p);
                break;
            case 1 : moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.WarehouseUiText, 1)); break;
            case 2 : moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.WarehouseUiText, 2)); break;
            case 3 : moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.WarehouseUiText, 3)); break;
            case 4 : moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.WarehouseUiText, 4)); break;
            default:
                break;
        }
    }

    public void SendExpBuffList()
    {
        CsRoleEffectList p = PacketObject.Create<CsRoleEffectList>();
        session.Send(p);
    }

    void _Packet(ScRoleEffectList p)
    {
        if (p.effectList != null && p.effectList.Length > 0)
        {
            PEffect exp = null;
            isHaveExpCard = true;
            p.effectList[0].CopyTo(ref exp);
            DispatchModuleEvent(EventHaveExpBuffList, exp);
        }
        else
        {
            isHaveExpCard = false;
            DispatchModuleEvent(EventNoExpBuffList);
        }
    }

    void _Packet(ScRoleEffectLost p)
    {
        switch (p.effectId)
        {
            case 1: moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.WarehouseUiText, 5)); break;
            case 2: moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.WarehouseUiText, 6)); break;
            case 3: moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.WarehouseUiText, 7)); break;
            case 4: moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.WarehouseUiText, 8)); break;
            default:
                break;
        }
        isHaveExpCard = false;
        DispatchModuleEvent(EventNoExpBuffList);
    }

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        SendExpBuffList();
    }

    public void ClickExpBtn()
    {
        isClickExpBtn = true;
        SendExpBuffList();
    }

    #endregion

    #region 战斗力更改

    void _Packet(ScItemFightChange p)
    {
        if (p == null)
        {
            Logger.LogError("fight info is null ");
            return;
        }
        var fight = m_roleInfo.fight;
        DispatchModuleEvent(EventCombatFightChnage, p.fight, fight);
        m_roleInfo.fight = p.fight;
    }

    #endregion

    public uint GetCount(int itemId)
    {
        var prop = ConfigManager.Get<PropItemInfo>(itemId);
        if (prop == null)
            return 0;
        switch (prop.itemType)
        {
            case PropType.Currency:
            {
                if (prop.subType == (int) CurrencySubType.Diamond)
                    return roleInfo.diamond;
                if (prop.subType == (int) CurrencySubType.Gold)
                    return roleInfo.coin;
                if (prop.subType == (int) CurrencySubType.HeartSoul)
                    return heartSoul;
                if (prop.subType == (int) CurrencySubType.PetSummonStone)
                    return petSummonStone;
                if (prop.subType == (int) CurrencySubType.WishCoin)
                    return wishCoinCount;
                    if (prop.subType == (int)CurrencySubType.FriendPoint )
                        return friendPoints;
                }
                break;
            default:
                return (uint)moduleEquip.GetPropCount(itemId);
        }
        return 0;
    }

    #region 禁言

    /// <summary>
    /// 当前禁言状态 1 可发言 2 禁言状态
    /// </summary>
    public int BanChat;

    void _Packet(ScSystemToggleBanChat p)
    {
        if (roleInfo == null) return;
        BanChat = p.chatState;
    }

    private void GetSignBanSate()
    {
        //上线时候获取发言状态 
        var player = moduleSelectRole.GetRoleSummary(modulePlayer.id_);
        if (player == null) return;
        if (player.banState == 1) BanChat = 2;
        else if (player.banState == 0) BanChat = 1;
    }
    void _Packet(ScSystemBanAction p)
    {
        var player = moduleSelectRole.GetRoleSummary(modulePlayer.id_);
        if (player == null) return;
        player.banState = p.banCode;
        if (player.banState == 1) BanChat = 2;
        else if (player.banState == 0) BanChat = 1;
    }

    #endregion

    #region Debug helper

    #if DEVELOPMENT_BUILD || UNITY_EDITOR
    public bool lockedClassDifference { get { return m_lockedClass > -1 && m_lockedClass != m_proto; } }
    public bool lockedGenderDifference { get { return m_lockedGender > -1 && m_lockedGender != m_gender; } }

    private int m_lockedClass = -1, m_lockedGender = -1;
    private CreatureElementTypes m_lockedElementType = CreatureElementTypes.Count;

    private void OnGMLockClass(Event_ e)
    {
        var nc = proto;

        var c = (int)e.param1;
        var i = (int)e.param2;

        var ci = c > 0 ? ConfigManager.Get<CreatureInfo>(c) : null;

        m_lockedClass  = ci ? ci.ID : m_proto;
        m_lockedGender = ci ? ci.gender : m_gender;

        var w = m_lockedClass == m_proto && i < 1 ? null : WeaponInfo.GetWeapon(m_lockedClass, i);

        if (w == null || w.isEmpty) m_lockedElementType = m_elementType;
        else
        {
            var a = ConfigManager.Get<WeaponAttribute>(w.weaponItemId);
            m_lockedElementType = a == null ? m_elementType : (CreatureElementTypes)a.elementType;
        }

        Logger.LogDetail("Lock class <b><color=#66EECC>[weapon:{0}, gender:{1}, weaponItem:{2}, elementType:{3}]</color></b>", proto, gender, w != null && !w.isEmpty ? w.weaponItemId : -1, elementType);

        DispatchEvent("OnPostGmLockClass", Event_.Pop(nc == proto));
    }
    #endif

    #endregion
}