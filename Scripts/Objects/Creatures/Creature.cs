/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Creature class.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-04-11
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 伤害信息
/// </summary>
public class DamageInfo : PoolObject<DamageInfo>
{
    /// <summary>
    /// 创建一个普通武器伤害 （不包含任何元素伤害）
    /// </summary>
    /// <param name="weaponDamage">伤害值</param>
    /// <param name="crit">是否暴击</param>
    /// <param name="attackInfo">伤害来源的攻击信息 可以为 null</param>
    /// <param name="fromBuff">是否由 Buff 创建 ?</param>
    /// <param name="preCalculate">是否标记为预先计算 ?</param>
    /// <returns></returns>
    public static DamageInfo Create(int weaponDamage, bool crit = false, AttackInfo attackInfo = null, bool fromBuff = false, bool preCalculate = false)
    {
        return Create(weaponDamage, CreatureElementTypes.None, 0, crit, attackInfo, fromBuff, preCalculate);
    }

    /// <summary>
    /// 创建一个包含武器伤害和元素伤害的伤害
    /// </summary>
    /// <param name="weaponDamage">武器伤害</param>
    /// <param name="elementType">元素类型</param>
    /// <param name="elementDamage">元素伤害</param>
    /// <param name="crit">是否暴击</param>
    /// <param name="attackInfo">伤害来源的攻击信息 可以为 null</param>
    /// <param name="fromBuff">是否由 Buff 创建 ?</param>
    /// <param name="preCalculate">是否标记为预先计算 ?</param>
    /// <returns></returns>
    public static DamageInfo Create(int weaponDamage, int elementType, int elementDamage = 0, bool crit = false, AttackInfo attackInfo = null, bool fromBuff = false, bool preCalculate = false)
    {
        return Create(weaponDamage, (CreatureElementTypes)elementType, elementDamage, crit, attackInfo, fromBuff, preCalculate);
    }

    /// <summary>
    /// 创建一个包含武器伤害和元素伤害的伤害
    /// </summary>
    /// <param name="weaponDamage">武器伤害</param>
    /// <param name="elementType">元素类型</param>
    /// <param name="elementDamage">元素伤害</param>
    /// <param name="crit">是否暴击</param>
    /// <param name="attackInfo">伤害来源的攻击信息 可以为 null</param>
    /// <param name="fromBuff">是否由 Buff 创建 ?</param>
    /// <param name="preCalculate">是否标记为预先计算 ?</param>
    /// <returns></returns>
    public static DamageInfo Create(int weaponDamage, CreatureElementTypes elementType, int elementDamage = 0, bool crit = false, AttackInfo attackInfo = null, bool fromBuff = false, bool preCalculate = false, BuffInfo.EffectTypes effectType = BuffInfo.EffectTypes.Unknow, BuffInfo.EffectFlags effectFlag = BuffInfo.EffectFlags.Unknow)
    {
        var info = Create();

        info.elementType        = elementType;
        info.damage             = weaponDamage + elementDamage;
        info.weaponDamage       = weaponDamage;
        info.elementDamage      = elementDamage;
        info.finalDamage        = info.damage;
        info.finalWeaponDamage  = weaponDamage;
        info.finalElementDamage = elementDamage;
        info.realDamage         = info.damage;
        info.realWeaponDamage   = weaponDamage;
        info.realElementDamage  = elementDamage;
        info.crit               = crit;
        info.attackInfo         = attackInfo;
        info.fromBuff           = fromBuff;
        info.preCalculate       = preCalculate;
        info.buffEffectType     = effectType;
        info.buffEffectFlag     = effectFlag;
        info.isHeal             = effectFlag == BuffInfo.EffectFlags.Heal;

        return info;
    }

    /// <summary>
    /// 创建一个包含武器伤害和元素伤害的伤害
    /// </summary>
    /// <param name="weaponDamage">武器伤害</param>
    /// <param name="elementType">元素类型</param>
    /// <param name="elementDamage">元素伤害</param>
    /// <param name="absorbed">被吸收的伤害</param>
    /// <param name="reduced">被减免的伤害</param>
    /// <param name="crit">是否暴击</param>
    /// <param name="attackInfo">伤害来源的攻击信息 可以为 null</param>
    /// <param name="fromBuff">是否由 Buff 创建 ?</param>
    /// <param name="preCalculate">是否标记为预先计算 ?</param>
    /// <returns></returns>
    public static DamageInfo Create(int weaponDamage, int elementType, int elementDamage, int absorbed, int reduced, bool crit, AttackInfo attackInfo = null, bool fromBuff = false, bool preCalculate = false, BuffInfo.EffectTypes effectType = BuffInfo.EffectTypes.Unknow, BuffInfo.EffectFlags effectFlag = BuffInfo.EffectFlags.Unknow)
    {
        return Create(weaponDamage, (CreatureElementTypes)elementType, elementDamage, absorbed, reduced, crit, attackInfo, fromBuff, preCalculate, effectType, effectFlag);
    }

    /// <summary>
    /// 创建一个包含武器伤害和元素伤害的伤害
    /// </summary>
    /// <param name="weaponDamage">武器伤害</param>
    /// <param name="elementType">元素类型</param>
    /// <param name="elementDamage">元素伤害</param>
    /// <param name="absorbed">被吸收的伤害</param>
    /// <param name="reduced">被减免的伤害</param>
    /// <param name="crit">是否暴击</param>
    /// <param name="attackInfo">伤害来源的攻击信息 可以为 null</param>
    /// <param name="fromBuff">是否由 Buff 创建 ?</param>
    /// <param name="preCalculate">是否标记为预先计算 ?</param>
    /// <returns></returns>
    public static DamageInfo Create(int weaponDamage, CreatureElementTypes elementType, int elementDamage, int absorbed, int reduced, bool crit, AttackInfo attackInfo = null, bool fromBuff= false, bool preCalculate = false, BuffInfo.EffectTypes effectType = BuffInfo.EffectTypes.Unknow, BuffInfo.EffectFlags effectFlag = BuffInfo.EffectFlags.Unknow)
    {
        var info = Create();

        info.elementType        = elementType;
        info.damage             = weaponDamage + elementDamage;
        info.weaponDamage       = weaponDamage;
        info.elementDamage      = elementDamage;
        info.absorbed           = absorbed;
        info.reduced            = reduced;
        info.finalDamage        = info.damage - absorbed - reduced;
        info.finalWeaponDamage  = elementDamage < 1 ? info.finalDamage : info.damage < 1 ? 0 : (int)(info.finalDamage * (double)weaponDamage / info.damage);
        info.finalElementDamage = info.finalDamage - info.finalWeaponDamage;
        info.realDamage         = info.finalDamage;
        info.realWeaponDamage   = info.finalWeaponDamage;
        info.realElementDamage  = info.finalElementDamage;
        info.crit               = crit;
        info.attackInfo         = attackInfo;
        info.fromBuff           = fromBuff;
        info.preCalculate       = preCalculate;
        info.buffEffectType     = effectType;
        info.buffEffectFlag     = effectFlag;
        info.isHeal             = effectFlag == BuffInfo.EffectFlags.Heal;

        return info;
    }

    /// <summary>
    /// 创建一个用于预先计算的伤害
    /// </summary>
    /// <param name="damage"></param>
    /// <returns></returns>
    public static DamageInfo CreatePreCalculate(int damage)
    {
        return Create(damage, 0, 0, false, null, false, true);
    }

    /// <summary>
    /// 创建一个用于 UI 显示的伤害，一般用于 Buff 伤害
    /// 该伤害不会触发任何逻辑行为
    /// </summary>
    /// <param name="damage">伤害数值</param>
    /// <param name="effectType">来自哪个 BuffEffect 如果是 Unknow 表示非 Buff 伤害</param>
    /// <param name="effectFlag">如果指定了 effectType 表示该效果对应的标志 （治疗\伤害\燃烧\中毒...）</param>
    /// <returns></returns>
    public static DamageInfo CreateUIDamage(int damage, BuffInfo.EffectTypes effectType = BuffInfo.EffectTypes.Unknow, BuffInfo.EffectFlags effectFlag = BuffInfo.EffectFlags.Unknow)
    {
        var d = Create(damage, 0, 0, false, null, effectType != BuffInfo.EffectTypes.Unknow, false, effectType, effectFlag);
        d.uiDamage = true;
        return d;
    }

    /// <summary>
    /// 伤害是否来自 Buff ？
    /// 比如反弹伤害Buff造成的伤害是来自 Buff 的
    /// </summary>
    public bool fromBuff;
    /// <summary>
    /// 预计算，仅用于特殊状况下处理因累计生命值损失改变属性的效果
    /// </summary>
    public bool preCalculate;
    /// <summary>
    /// 仅用于 UI 显示
    /// 一般用于 Buff 效果造成的伤害，Buff 伤害不会触发任何伤害逻辑，但需要显示
    /// </summary>
    public bool uiDamage;
    /// <summary>
    /// 伤害是否是一个治疗效果
    /// 治疗效果和伤害都是 Damage
    /// </summary>
    public bool isHeal;
    /// <summary>
    /// 伤害来源 BuffEffectType
    /// fromBuff 为 true 时表示是由哪种 BuffEffect 造成的
    /// </summary>
    public BuffInfo.EffectTypes buffEffectType;
    /// <summary>
    /// 伤害来源 BuffEffectType 的标志
    /// 指定了 buffEffectType 时表示该效果对应的标志类型 （伤害\治疗\燃烧\中毒...）
    /// </summary>
    public BuffInfo.EffectFlags buffEffectFlag;

    /// <summary>
    /// 伤害附带的元素伤害类型
    /// </summary>
    public CreatureElementTypes elementType;
    /// <summary>
    /// 原始伤害 weaponDamage + elementDamage
    /// </summary>
    public int damage;
    /// <summary>
    /// 原始武器伤害
    /// </summary>
    public int weaponDamage;
    /// <summary>
    /// 原始元素伤害
    /// </summary>
    public int elementDamage;
    /// <summary>
    /// 最终伤害 finalWeaponDamage + finalElementDamage
    /// </summary>
    public int finalDamage;
    /// <summary>
    /// 最终武器伤害
    /// </summary>
    public int finalWeaponDamage;
    /// <summary>
    /// 最终元素伤害
    /// </summary>
    public int finalElementDamage;
    /// <summary>
    /// 被吸收的伤害
    /// </summary>
    public int absorbed;
    /// <summary>
    /// 被减免的伤害
    /// </summary>
    public int reduced;
    /// <summary>
    /// 实际伤害 realWeaponDamage + realElementDamage
    /// </summary>
    public int realDamage;
    /// <summary>
    /// 实际武器伤害
    /// </summary>
    public int realWeaponDamage;
    /// <summary>
    /// 实际元素伤害
    /// </summary>
    public int realElementDamage;
    /// <summary>
    /// 是否暴击？
    /// </summary>
    public bool crit;
    /// <summary>
    /// 攻击信息
    /// </summary>
    public AttackInfo attackInfo;

    protected override void OnInitialize()
    {
        OnDestroy();
    }

    protected override void OnDestroy()
    {
        elementType        = CreatureElementTypes.None;
        damage             = 0;
        weaponDamage       = 0;
        elementDamage      = 0;
        finalDamage        = 0;
        finalWeaponDamage  = 0;
        finalElementDamage = 0;
        realDamage         = 0;
        realWeaponDamage   = 0;
        realElementDamage  = 0;
        absorbed           = 0;
        reduced            = 0;
        crit               = false;
        fromBuff           = false;
        preCalculate       = false;
        uiDamage           = false;
        isHeal             = false;
        attackInfo         = null;
        buffEffectType     = BuffInfo.EffectTypes.Unknow;
        buffEffectFlag     = BuffInfo.EffectFlags.Unknow;
    }
}

public struct SkilLEntry
{
    public static readonly SkilLEntry empty = new SkilLEntry() { skillId = -1, level = 0, addtion = 0 };

    public bool valid => skillId > -1;

    public int skillId;
    public int level;
    public double addtion;
}

public partial class Creature : SceneObject, IRenderObject, ILogicTransform
{
    #region Static functions

    /// <summary>
    /// Creature a simple creature from config id
    /// </summary>
    /// <param name="configID">Config ID</param>
    /// <param name="pos">The initialize position</param>
    /// <param name="rot">The initialize rotation</param>
    /// <param name="player">Player ?</param>
    /// <param name="name">Object name in object manager</param>
    /// <param name="uiName">UI name</param>
    /// <param name="useSpringBone">Use spring bone ?</param>
    /// <returns></returns>
    public static Creature Create(int configID, Vector3 pos, bool player = false, string name = "", string uiName = "", bool useSpringBone = false)
    {
        var info = player ? modulePlayer.BuildPlayerInfo(configID, false, false) : ConfigManager.Get<CreatureInfo>(configID);
        if (info) info.buffs = null;

        return Create(info, pos, new Vector3(0, 90, 0), player, name, uiName, false, useSpringBone);
    }

    /// <summary>
    /// Creature a simple creature from config id
    /// </summary>
    /// <param name="configID">Config ID</param>
    /// <param name="pos">The initialize position</param>
    /// <param name="rot">The initialize rotation</param>
    /// <param name="player">Player ?</param>
    /// <param name="name">Object name in object manager</param>
    /// <param name="uiName">UI name</param>
    /// <param name="useSpringBone">Use spring bone ?</param>
    /// <returns></returns>
    public static Creature Create(CreatureInfo info, Vector3 pos, bool player = false, string name = "", string uiName = "", bool useSpringBone = false)
    {
        return Create(info, pos, new Vector3(0, 90, 0), player, name, uiName, false, useSpringBone);
    }

    /// <summary>
    /// Creature a creature from config id
    /// </summary>
    /// <param name="configID">Config ID</param>
    /// <param name="pos">The initialize position</param>
    /// <param name="rot">The initialize rotation</param>
    /// <param name="player">Player ?</param>
    /// <param name="name">Object name in object manager</param>
    /// <param name="uiName">UI name</param>
    /// <param name="playerBuff">Init buffs when create creature ?</param>
    /// <param name="combat">Create for combat ? If not, creature will only load simple animator instead of weapon animator</param>
    /// <returns></returns>
    public static Creature Create(int configID, Vector3 pos, Vector3 rot, bool player = false, string name = "", string uiName = "", bool playerBuff = true, bool playerSkill = true, bool combat = true)
    {
        var info = player ? modulePlayer.BuildPlayerInfo(configID, playerBuff, playerSkill) : ConfigManager.Get<CreatureInfo>(configID);

        if (info)
        {
            if (!playerBuff) info.buffs = null;
            if (!playerSkill) info.skills = null;
        }

        return Create(info, pos, rot, player, name, uiName, combat);
    }

    /// <summary>
    /// Create a creature from config
    /// </summary>
    /// <param name="info">Creature config info</param>
    /// <param name="pos">The initialize position</param>
    /// <param name="rot">The initialize rotation</param>
    /// <param name="player">Player ?</param>
    /// <param name="name">Object name in object manager</param>
    /// <param name="uiName">UI name</param>
    /// <param name="playerBuff">Init buffs when create creature ?</param>
    /// <param name="combat">Create for combat ? If not, creature will only load simple animator instead of weapon animator</param>
    /// <returns></returns>
    public static Creature Create(CreatureInfo info, Vector3 pos, Vector3 rot, bool player, bool playerBuff = true, bool playerSkill = true, string name = "", string uiName = "", bool combat = true)
    {
        if (info)
        {
            if (!playerBuff) info.buffs = null;
            if (!playerSkill) info.skills = null;
        }
        return Create(info, pos, rot, player, name, uiName, combat);
    }

    /// <summary>
    /// Create a creature from config
    /// </summary>
    /// <param name="info">Creature config info</param>
    /// <param name="pos">The initialize position</param>
    /// <param name="rot">The initialize rotation</param>
    /// <param name="player">Player ?</param>
    /// <param name="name">Object name in object manager</param>
    /// <param name="uiName">UI name</param>
    /// <param name="combat">Create for combat ? If not, creature will only load UI animator instead of weapon animator</param>
    /// <param name="useSpringBone">Use spring bone ?</param>
    /// <returns></returns>
    public static Creature Create(CreatureInfo info, Vector3_ pos, Vector3 rot, bool player = false, string name = "", string uiName = "", bool combat = true, bool useSpringBone = true)
    {
        if (info == null)
        {
            Logger.LogError("Creature::Create: Create creature failed, invalid config info");
            return null;
        }

        var rootNode = new GameObject().transform;

        if (!CreateMorphNodes(info, rootNode))
        {
            Logger.LogError("Creature::Create: Create creature [{0}:{1}] failed, main model [{2}] not loaded", info.ID, info.name, CreatureInfo.GetMorphModelName(info.models, 0));
            return null;
        }

        rootNode.position    = pos;
        rootNode.eulerAngles = rot;

        var c = Create<Creature>(string.IsNullOrEmpty(name) ? info.name : name, rootNode.gameObject);

        c.InitPosition(pos);
        c.isPlayer      = player;
        c.isMonster     = false;
        c.isCombat      = combat;
        c.isRobot       = false;
        c.creatureCamp  = player ? CreatureCamp.PlayerCamp : CreatureCamp.MonsterCamp;
        c.uiName        = string.IsNullOrEmpty(uiName) ? c.name : uiName;
        c.isDead        = false;
        c.realDead      = false;
        c.useSpringBone = useSpringBone;

        c.UpdateConfig(info);
        c.OnCreate(info.buffs);

        return c;
    }

    protected static bool CreateMorphNodes(CreatureInfo info, Transform rootNode, bool onlyNormal = false)
    {
        return CreateMorphNodes(info.models, rootNode, onlyNormal);
    }

    protected static bool CreateMorphNodes(string[] models, Transform rootNode, bool onlyNormal = false)
    {
        for (int i = 0, c = (int)CreatureMorph.Count; i < c; ++i)
        {
            var modelName = CreatureInfo.GetMorphModelName(models, i);

            if (string.IsNullOrEmpty(modelName))
            {
                if (i == 0) return false;
                continue;
            }

            var model = Level.GetPreloadObjectFromPool(modelName);
            if (!model)
            {
                if (i == 0) return false;
                continue;
            }

            model.name = MORPH_NODE_NAMES[i];
            model.gameObject.SetActive(i == 0);

            Util.AddChild(rootNode, model.transform);
        }

        return true;
    }

    #endregion

    /// <summary>
    /// 队伍 索引 用于标识输入
    /// </summary>
    public int teamIndex { get; set; }
    /// <summary>
    /// 是否是玩家控制角色
    /// </summary>
    public bool isPlayer
    {
        get { return m_isPlayer; }
        set
        {
            m_isPlayer = value;
            creatureCamp = m_isPlayer ? CreatureCamp.PlayerCamp : CreatureCamp.MonsterCamp;
        }
    }
    private bool m_isPlayer;

    public PetCreature pet;

    public Creature lastAttacker { get; protected set; }

    public virtual ulong Identify { get { return roleId; } }

    /// <summary>
    /// Is this creature create for battle level ?
    /// </summary>
    public bool isCombat { get; protected set; }
    /// <summary>
    /// 是否死亡（可能处于被动动作中）
    /// </summary>
    public bool isDead { get; protected set; }
    /// <summary>
    /// 是否死亡 （死亡动画播放结束）
    /// </summary>
    public bool realDead { get; protected set; }
    public CreatureBehaviour behaviour { get; private set; }
    /// <summary>
    /// 仅用于 AI
    /// </summary>
    public PVECreatureBehavior PVEBehaviour
    {
        get
        {
            if (!m_pveBehaviour) m_pveBehaviour = m_activeRootNode?.GetComponent<PVECreatureBehavior>();
            return m_pveBehaviour;
        }
    }
    private PVECreatureBehavior m_pveBehaviour;

    public bool elementAffected { get { return m_elementAffected; } }
    protected bool m_elementAffected;

    protected List<Buff> m_buffs = new List<Buff>();

    protected Dictionary<int, SkilLEntry> m_skills = new Dictionary<int, SkilLEntry>();

    private InvertHelper m_modelInverter;

    protected Transform[] m_morphNodes = new Transform[(int)CreatureMorph.Count];
    
    /// <summary>
    /// 血量百分比 0 - 1
    /// </summary>
    public float healthRate { get { return maxHealth < 1 ? 0 : (float)health / maxHealth; } }
    /// <summary>
    /// 怒气百分比 0 - 1
    /// </summary>
    public float rageRate { get { return maxRage < 1 ? 0 : (float)(rage / maxRage); } }
    /// <summary>
    /// 能量百分比 0 - 1
    /// </summary>
    public float energyRate { get { return m_maxEnergy < 1 ? 0 : (float)m_energy / m_maxEnergy; } }
    /// <summary>
    /// 血量百分比 0 - 1
    /// </summary>
    public double healthRateL { get { return GetField(CreatureFields.Health) / GetField(CreatureFields.MaxHealth); } }
    /// <summary>
    /// 怒气百分比 0 - 1
    /// </summary>
    public double rageRateL { get { return maxRage < 1 ? 0 : rage / maxRage; } }
    /// <summary>
    /// 能量百分比 0 - 1
    /// </summary>
    public double energyRateL { get { return m_maxEnergy < 1 ? 0 : (double)m_energy / m_maxEnergy; } }

    /// <summary>
    /// 怪物标识
    /// </summary>
    public bool isMonster { get; protected set; }
    /// <summary>
    /// 所属阵营
    /// </summary>
    public CreatureCamp creatureCamp { get; protected set; }

    /// <summary>
    /// 是否是机器人
    /// </summary>
    public bool isRobot { get; protected set; }

    /// <summary>
    /// 是否检索场景边界(怪物离场或进场的时候不需要锁定场景边界)
    /// </summary>
    public bool checkEdge { get { return m_checkEdge; } set { m_checkEdge = value; } }
    private bool m_checkEdge;

    /// <summary>
    /// 是否需要停止buff的倒计时检测
    /// pve模式中需要在对话的过程中暂停掉buff的检测
    /// </summary>
    public bool pauseBuffUpdate { get; set; } = false;

    protected Creature() { }

    protected override void OnAddedToScene()
    {
        for (var i = 0; i < m_morphNodes.Length; ++i) m_morphNodes[i] = transform.Find(MORPH_NODE_NAMES[i]);

        m_morph = CreatureMorph.Normal;

        m_activeRootNode = m_morphNodes[0];

        animator  = m_activeRootNode.GetComponent<Animator>();
        model     = m_activeRootNode.GetComponent<Transform>("model");
        head      = Util.FindChild(model,"Bip001 Head");
        hairNode  = Util.FindChild(model, GetHairNodeName(gender));

        springManager = m_activeRootNode.GetComponent<SpringManager>();
        springManager?.Initialize();

        m_shadow  = m_activeRootNode.GetComponent<Projector>("shadow");

        m_modelInverter = model.GetComponentDefault<InvertHelper>();
        m_modelInverter.ResetToDefault();

        behaviour = m_activeRootNode.GetComponentDefault<CreatureBehaviour>();
        behaviour.creature = this;
        behaviour.UpdateAwakeState();
        
        transform.SetParent(Level.current.root, true);

        position = position_;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        teamIndex = -1;

        m_toughID = 0;

        ClearToughState();

        m_invincibleCount      = 0;
        m_toughCount           = 0;
        m_weakCount            = 0;
        m_etherealCount        = 0;
        m_passiveEtherealCount = 0;
        m_rimCount             = 0;
        m_clearAttackCount     = 0;

        m_toughLevel           = 0;
        m_breakToughLevel      = 0;
        m_awakeDuration        = 0;

        m_weaponID             = -1; // Prevent initialize with weapon 0
        m_offWeaponID          = -1;
        m_weaponItemID         = -1;
        m_offWeaponItemID      = -1;

        m_bulletCount          = -1;
        m_energy               = -1;
        m_maxEnergy            = 0;

        m_regenHealth          = 0;

        m_knockbackDistance    = 0;
        m_comboBreakCountDown  = -1;
        m_hpVisiableCount = 0;

        m_elementAffected = false;

        obstacleMask = ignoreObstacleMask = 0;

        invincible = tough = weak = ethereal = passiveEthereal = clearAttack = rim = false;
        stateFlag = -1;
        avatar = string.Empty;

        moveState = 0;
        onGround = false;

        m_checkEdge = true;
        enableUpdate = true;
    }
    protected void OnCreate(int[] buffs = null)
    {
        if (buffs != null)
        {
            for (var i = 0; i < buffs.Length; ++i)
                Buff.Create(buffs[i], this);
        }

        if (isPlayer)
        {
            teamIndex = 0;
            var cInfo = ConfigManager.Get<CreatureInfo>(roleProto);
            avatar = string.IsNullOrEmpty(modulePlayer.avatar) ? cInfo.avatar : modulePlayer.avatar;
            avatarBox = modulePlayer.avatarBox == 0 ? DEFAULT_AVATAR_BOX : modulePlayer.avatarBox;
            var mat = Level.GetPreloadObject<Material>(CombatConfig.sdefaultSelfShadow, false);
            if (mat) m_shadow.material = mat;
            DispatchEvent(CreatureEvents.PLAYER_ADD_TO_SCENE);
        }

        m_direction = CreatureDirection.BACK;
        direction = CreatureDirection.FORWARD;

        ResetLayers();

        m_fixedAnimationTime = 0;

        if (!isCombat) behaviour.UpdateAllColliderState(false);
        else if (!(this is PetCreature)) Buff.Create(0, this); // Element damage trigger

        if (springManager) springManager.enabled &= useSpringBone;

#if UNITY_EDITOR
        var debug = GetComponentDefault<DebugCreature>();
        debug.creature = this;

        var dm = GetComponentDefault<DebugStateMachine>();
        dm.stateMachine = m_stateMachine;

        lockLayer   = false;
        lockedLayer = 0;
        normalLayer = 0;
#endif
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        ClearBuffs();

        m_skills.Clear();
        m_morphNodes.Clear();

        PacketObject.BackArray(awakeChangeAttr);

        if (m_stateMachine) m_stateMachine.Destroy();
        if (pet) pet.Destroy();

        awakeChangeAttr  = null;
        pet              = null;
        m_shadow         = null;
        m_activeRootNode = null;

        animator      = null;
        model         = null;
        head          = null;
        hairNode      = null;
        behaviour     = null;
        springManager = null;
        lastAttacker  = null;

        avatar        = string.Empty;
        avatarBox     = DEFAULT_AVATAR_BOX;

        m_stateMachine = null;
        m_currentState = null;

        m_lastTeamIndex = -1;
        m_useAI = false;

        roleProto = 0;
        roleId    = 0;

        m_fields.Clear();
        m_tmpFields.Clear();
    }

    #region Public interface

    /// <summary>
    /// 当前对象是否和目标同一阵营
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool SameCampWith(Creature other)
    {
        return creatureCamp == other.creatureCamp;
    }
    
    /// <summary>
    /// 重置阵营
    /// </summary>
    /// <param name="camp"></param>
    public void SetCreatureCamp(CreatureCamp camp)
    {
        creatureCamp = camp;
    }

    /// <summary>
    /// 当前对象是否和目标同一方向
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool SameDirectionWith(Creature other)
    {
        return direction == other.direction;
    }

    /// <summary>
    /// 当前对象是否在目标前面
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool InFrontOf(Creature other)
    {
        return other.isForward ? m_x >= other.x : m_x <= other.x;
    }

    /// <summary>
    /// 目标是否在当前对象前面
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool TargetInFront(Creature other)
    {
        return isForward ? other.x >= m_x : other.x <= m_x;
    }

    /// <summary>
    /// 当前对象是否包含指定 ID 的状态
    /// </summary>
    /// <param name="stateID"></param>
    /// <returns></returns>
    public bool HasState(int stateID)
    {
        return m_stateMachine && m_stateMachine.GetState(stateID) != null;
    }

    /// <summary>
    /// 当前对象是否包含指定状态
    /// </summary>
    /// <param name="stateName"></param>
    /// <returns></returns>
    public bool HasState(string stateName)
    {
        return m_stateMachine && m_stateMachine.GetState(stateName) != null;
    }

    /// <summary>
    /// 当前对象如果包含指定名称的状态，返回该状态的 groupMask，否则返回 0
    /// </summary>
    /// <param name="stateName"></param>
    /// <returns></returns>
    public int GetStateMask(string stateName)
    {
        return m_stateMachine ? m_stateMachine.GetStateMask(stateName) : 0;
    }

    /// <summary>
    /// 当前对象如果包含指定 ID 的状态，返回该状态的 groupMask，否则返回 0
    /// </summary>
    /// <param name="stateID"></param>
    /// <returns></returns>
    public int GetStateMask(int stateID)
    {
        return m_stateMachine ? m_stateMachine.GetStateMask(stateID) : 0;
    }

    /// <summary>
    /// 当前对象如果包含指定 ID 的状态，返回该状态对应的当前武器技能等级
    /// </summary>
    /// <param name="stateID"></param>
    /// <returns></returns>
    public int GetStateLevel(int stateID)
    {
        var skill = SkillToStateInfo.GetSkillID(m_weaponID, stateID);
        if (skill < 0) return -1;
        return GetSkillLevel(skill);
    }

    public int GetSkillLevel(int skillID)
    {
        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (m_isPlayer && moduleEquip.lockedWeaponDifference) return 1;
        #endif

        if (m_skills.ContainsKey(skillID))
            return m_skills[skillID].level;
        return -1;
    }

    public SkilLEntry GetSkillEntry(int skillId)
    {
        return m_skills.Get(skillId, SkilLEntry.empty);
    }

    public void SetLayer(int layer, bool ignoreLock = false)
    {
#if UNITY_EDITOR
        SetLayer_(layer, ignoreLock);
#else
        if (layer < 0) ResetLayers();
        else if (model)
        {
            Util.SetLayer(model.gameObject, layer);
            Util.SetLayer(hairNode, layer);
            Util.SetLayer(m_shadow.gameObject, layer);
            Util.SetLayer(behaviour.effects.gameObject, layer);

            behaviour.SetWeaponLayer(layer, layer);
        }
#endif
    }

    /// <summary>
    /// 直接死亡
    /// </summary>
    public void Kill()
    {
        health = 0;
    }

    /// <summary>
    /// 复活
    /// </summary>
    public void Revive()
    {
        health = maxHealth;
        stateMachine?.TranslateToID(StateMachineState.STATE_REVIVE);
    }

    public void InitPosition(Vector3_ rPos)
    {
        rPos.y = 0;
        position_ = rPos;
        if(transform)
            position = position_;

        pp = position_;
        onGround = true;
    }

    public void UpdateConfig(CreatureInfo i)
    {
        awakeChangeAttr = i.awakeChangeAttr;

        standardRunSpeed = CombatConfig.sstandardRunSpeed;
        
        // Must before statemachine initialize
        gender                 = i.gender;
        roleProto              = i.ID;

        // Must before statemachine create
        UpdateSkills(i.skills);

        // First we initialize statemachine
        UpdateStateMachine(i.weaponID);

        offWeaponID = i.offWeaponID;

        // Must after statemachine initialize
        weaponItemID    = i.weaponItemID;
        offWeaponItemID = i.offWeaponItemID;

        bulletCount            = i.bulletCount;

        configID               = i.ID;
        icon                   = i.icon;
        info                   = i.info;
        localScale             = i.scale * localScale;
        baseExp                = i.baseExp;
        type                   = i.type;
        elementType            = i.elementType;
        maxRage                = i.maxRage;
        maxHealth              = i.maxHealth;
        health                 = i.health;
        rage                   = i.rage;
        attack                 = i.attack;
        offAttack              = i.offAttack;
        elementAttack          = i.elementAttack;
        elementDefenseWind     = i.elementDefenseWind;
        elementDefenseFire     = i.elementDefenseFire;
        elementDefenseWater    = i.elementDefenseWater;
        elementDefenseThunder  = i.elementDefenseThunder;
        elementDefenseIce      = i.elementDefenseIce;
        defense                = i.defense;
        chargeSpeed            = i.chargeSpeed;
        leech                  = i.leech;
        regenHealth            = i.regenHealth;
        regenRage              = i.regenRage;
        colliderSize           = i.colliderSize.x;
        hitColliderSize        = i.hitColliderSize.x;
        height                 = i.hitColliderSize.y;
        colliderHeight         = i.colliderSize.y == 0 ? height : i.colliderSize.y;
        colliderOffset         = i.colliderOffset;
        hitColliderOffset      = i.hitColliderOffset;
        regenHealthPercent     = 0;
        regenRagePercent       = 0;
        maxEnergy              = i.maxEnergy;
        energy                 = i.energy;
        AwakeDuration          = i.awakeDuration;

        _SetField(CreatureFields.Level,           i.level);
        _SetField(CreatureFields.Strength,        i.strength);
        _SetField(CreatureFields.Artifice,        i.artifice);
        _SetField(CreatureFields.Vitality,        i.vitality);
        _SetField(CreatureFields.Crit,            i.crit);
        _SetField(CreatureFields.CritMul,         i.critMul);
        _SetField(CreatureFields.Resilience,      i.resilience);
        _SetField(CreatureFields.Brutality,       i.brutality);
        _SetField(CreatureFields.Firm,            i.firm);
        _SetField(CreatureFields.AttackSpeed,     i.attackSpeed);
        _SetField(CreatureFields.MoveSpeed,       i.moveSpeed);
        _SetField(CreatureFields.AttackRageMul,   0);
        _SetField(CreatureFields.AttackedRageMul, 0);

        UpdateColliders();
    }

    public void UpdateSkills(PSkill[] skills)
    {
        m_skills.Clear();
        if (skills == null || skills.Length < 1) return;

        foreach (var skill in skills) m_skills.Set(skill.skillId, new SkilLEntry() { skillId = skill.skillId, level = skill.level, addtion = skill.addition * 0.0001});
    }

    /// <summary>
    /// Update all creature mesh/skinnedmesh invert state
    /// </summary>
    /// <param name="refreshRenderers">Update renderers ? (if you add/remove hair node or other render node, set it to true)</param>
    public void UpdateInverter(bool refreshRenderers = false)
    {
        if (destroyed) return;

        m_modelInverter.Refresh(refreshRenderers);
    }

    /// <summary>
    /// Updtae current rim light state
    /// </summary>
    public void UpdateRimLight()
    {
        if (destroyed) return;

        var t = rim ? invincible && tough ? 3 : invincible ? 1 : 2 : 0;
        m_modelInverter.UpdateRimLight((InvertHelper.RimType)t);
    }

    public void UpdateColliders()
    {
        behaviour.collider_.size     = new Vector2_(colliderSize * 2.0, colliderHeight);
        behaviour.collider_.offset   = new Vector2_(0, colliderHeight * 0.5 + colliderOffset);
        behaviour.hitCollider.size   = new Vector2_(hitColliderSize * 2.0, height);
        behaviour.hitCollider.offset = new Vector2_(0, height * 0.5 + hitColliderOffset);
    }

    /// <summary>
    /// 重置当前角色的 Layer 到默认状态
    /// 模型/头发 使用 Layers.Model
    /// 武器使用 Layers.WEAPON
    /// 副武器使用 Layers.WEAPON_OFF
    /// 饰品使用 Layers.Jewelry
    /// </summary>
    public void ResetLayers()
    {
        if (!model) return;

        Util.SetLayer(model, Layers.MODEL);
        Util.SetLayer(hairNode, Layers.MODEL);
        Util.SetLayer(m_shadow.gameObject, Layers.MODEL);
        Util.SetLayer(behaviour.effects.gameObject, Layers.EFFECT);

        behaviour.SetWeaponLayer();

        DispatchEvent(CreatureEvents.RESET_LAYERS);
    }

    public void SwitchMorphModel(CreatureMorph _morph = CreatureMorph.Normal)
    {
        if (_morph < 0 || _morph >= CreatureMorph.Count)
        {
            Logger.LogError("Creature::SwitchMorph: Could not switch to morph <b><color=#FF0000FF>[{0}]</color></b> from <b><color=#FF0000FF>[{1}]</color></b>, invalid morph state. Use Normal instead.", _morph, m_morph);
            _morph = CreatureMorph.Normal;
        }
        var node = m_morphNodes[(int)_morph];
        if (!node)
        {
            Logger.LogWarning("Creature::SwitchMorph: Creature [{0}:{1}] does not have morph node [{2}], use normal node.", id, name, m_morph);
            node = activeRootNode;
        }
        if (node == m_activeRootNode) return;

        m_activeRootNode.gameObject.SetActive(false);

        m_activeRootNode = node;
        m_activeRootNode.gameObject.SetActive(true);

        behaviour.UpdateAllColliderState(false);
        behaviour.attackCollider.Clear();
        behaviour.hitCollider.Clear();

        animator = m_activeRootNode.GetComponent<Animator>();
        model = m_activeRootNode.GetComponent<Transform>("model");
        head  = Util.FindChild(model, "Bip001 Head");
        hairNode = Util.FindChild(model, GetHairNodeName(gender));

        animator.enabled = false;
        m_stateMachine.animator = animator;

        springManager = m_activeRootNode.GetComponent<SpringManager>();
        springManager?.Initialize();

        var shadowProjector = m_activeRootNode.GetComponent<Projector>("shadow");
        if (m_isPlayer) shadowProjector.material = m_shadow.material;
        m_shadow = shadowProjector;

        m_modelInverter = model.GetComponentDefault<InvertHelper>();
        m_modelInverter.invert = !m_isForward;

        var old = behaviour;

        behaviour = m_activeRootNode.GetComponentDefault<CreatureBehaviour>();
        behaviour.creature = this;
        behaviour.UpdateWeapon();
        behaviour.UpdateAllColliderState(isCombat);
        behaviour.UpdateAwakeState();
        behaviour.CopyStateFrom(old);

        m_currentState.RefreshCurrentFrameState();

        UpdateRimLight();
        UpdateColliders();
        UpdateAnimator();

        DispatchEvent(CreatureEvents.MORPH_MODEL_CHANGED);
    }

    public void SwitchMorph(CreatureMorph _morph = CreatureMorph.Normal, bool switchModel = false)
    {
        if (_morph < 0 || _morph >= CreatureMorph.Count)
        {
            Logger.LogError("Creature::SwitchMorph: Could not switch to morph <b><color=#FF0000FF>[{0}]</color></b> from <b><color=#FF0000FF>[{1}]</color></b>, invalid morph state. Use Normal instead.", _morph, m_morph);
            _morph = CreatureMorph.Normal;
        }
        if (_morph == m_morph) return;

        var e = Event_.Pop(m_morph, _morph);
        m_morph = _morph;

        m_stateMachine.SetParam(StateMachineParam.morph, (int)m_morph);

        if (null != awakeChangeAttr)
        {
            for (var i = 0; i < awakeChangeAttr.Length; i++)
            {
                var att = awakeChangeAttr[i];
                if (att?.value == null || att.value.Length <= 0)
                    continue;
                if (Math.Abs(att.value[0]) < double.Epsilon)
                    continue;
                _SetField((CreatureFields)att.id, GetField((CreatureFields)att.id) + att.value[0] * (_morph == CreatureMorph.Awake ? 1 : -1));
            }
        }

        if (switchModel) SwitchMorphModel(m_morph);

        Logger.LogInfo("[{0}:{1}-{2}], Creature [{3}:{4}] switch to morph <b><color=#CC33FFFF>[{5}]</color></b> from <b><color=#CC33FFFF>[{6}]</color></b>.", Level.levelTime, frameTime, frameCount, id, name, m_morph, e.param1);

        DispatchEvent(CreatureEvents.MORPH_CHANGED, e);
    }

    #endregion

    #region Damage & Combat

    public void TakeDamage(Creature source, DamageInfo damage)
    {
        if (isDead || damage.finalDamage == 0) return;

        lastAttacker = source;

        var back = m_fields;
        m_fields = m_tmpFields;

        System.Array.Copy(back, m_fields, m_fields.Length);

        var e = Event_.Pop(source, damage);
        DispatchEvent(CreatureEvents.WILL_TAKE_DAMAGE, e, false);

        m_fields = back;

        var ep = damage.damage < 1 ? 0 : (double)damage.elementDamage / damage.damage;

        damage.reduced = (int)(damage.finalDamage * damageReduce + (!source ? 0 : source.GetSpecialProperty(this, CreatureFields.DamageIncrease)));

        damage.finalDamage = damage.damage - damage.absorbed - damage.reduced;
        damage.finalWeaponDamage = (int)(damage.finalDamage * (1 - ep));
        damage.finalElementDamage = damage.finalDamage - damage.finalWeaponDamage;

        damage.realDamage = damage.finalDamage > _health ? _health : damage.finalDamage;
        damage.realWeaponDamage = (int)(damage.realDamage * (1 - ep));
        damage.realElementDamage = damage.realDamage - damage.realWeaponDamage;

        if (source) source.DispatchEvent(CreatureEvents.DEAL_DAMAGE, Event_.Pop(this, damage));

        _health -= damage.realDamage;

        FightRecordManager.RecordLog< LogInt>(log =>
        {
            log.tag = (byte)TagType.realDamage;
            log.value = damage.realDamage;
        });

        #region Debug log
        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        Logger.LogDetail("[{13}:{14}-{15}], TakeDamage: [{11}:{12}], originDamage:[{0},{1},{2}], finalDamage:[{3},{4},{5}], realDamage:[{6},{7},{8}], absorbed:{9}, reduced:{10}",
            damage.damage, damage.weaponDamage, damage.elementDamage, damage.finalDamage, damage.finalWeaponDamage, damage.finalElementDamage, damage.realDamage, damage.realWeaponDamage, damage.realElementDamage, damage.absorbed, damage.reduced,
            id, name, Level.levelTime, frameTime, frameCount);
        #endif

        #if AI_LOG
        Module_AI.LogBattleMsg(this, "[TakeDamage: [originDamage:[{0},{1},{2}], finalDamage:[{3},{4},{5}], realDamage:[{6},{7},{8}], absorbed:{9}, reduced:{10}",
            damage.damage, damage.weaponDamage, damage.elementDamage, damage.finalDamage, damage.finalWeaponDamage, damage.finalElementDamage, damage.realDamage, damage.realWeaponDamage, damage.realElementDamage, damage.absorbed, damage.reduced);
        #endif
        #endregion

        e.param3 = 0; // Used to calculate reflect damage
        DispatchEvent(CreatureEvents.TAKE_DAMAGE, e, false);

        var reflect = (int)e.param3;
        Event_.Back(e);

        if (source && isDead)
            source.DispatchEvent(CreatureEvents.KILL, Event_.Pop(this, damage));

        if (isDead)
            DispatchEvent(CreatureEvents.BE_KILL, Event_.Pop(source, damage));

        damage.Destroy();

        if (reflect > 0 && source) source.TakeUIDamage(this, reflect, BuffInfo.EffectTypes.ReflectDamage, BuffInfo.EffectFlags.Damage);
    }

    public void TakeUIDamage(Creature source, int damage, BuffInfo.EffectTypes effectType = BuffInfo.EffectTypes.Unknow, BuffInfo.EffectFlags flag = BuffInfo.EffectFlags.Unknow)
    {
        if (damage <= 0) return;
        var beforeTakeDamageIsDead = isDead;
        var di = DamageInfo.CreateUIDamage(damage, effectType, flag);

        int _mh = maxHealth, _h = _health, _d = _mh - _h;
        di.realDamage = di.isHeal ? di.finalDamage > _d ? _d : di.finalDamage : di.finalDamage > _h ? _h : di.finalDamage;
        di.realWeaponDamage = di.realDamage;

        var c = source is PetCreature ? ((PetCreature)source).ParentCreature : source;
        if (c) c.DispatchEvent(CreatureEvents.DEAL_UI_DAMAGE, Event_.Pop(this, di));

        if (flag == BuffInfo.EffectFlags.Heal)
            _health += di.realDamage;
        else
            _health -= di.realDamage;


        FightRecordManager.RecordLog< LogInt>(log =>
        {
            log.tag = (byte)TagType.uiDamage;
            log.value = di.realDamage;
        });

        if (c && isDead && !beforeTakeDamageIsDead)
            c.DispatchEvent(CreatureEvents.KILL, Event_.Pop(this, damage));
        if (isDead && !beforeTakeDamageIsDead)
            DispatchEvent(CreatureEvents.BE_KILL, Event_.Pop(c, damage));

        #region Debug log
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Logger.LogDetail("[{15}:{16}-{17}], TakeUI<color={11}>{12}</color>: [{13}:{14}], originDamage:[{0},{1},{2}], finalDamage:[{3},{4},{5}], realDamage:[{6},{7},{8}], absorbed:{9}, reduced:{10}]",
            di.damage, di.weaponDamage, di.elementDamage, di.finalDamage, di.finalWeaponDamage, di.finalElementDamage, di.realDamage, di.realWeaponDamage, di.realElementDamage, di.absorbed, di.reduced,
            di.isHeal ? "#00FF00" : "#FF0000", di.isHeal ? "Heal" : "Damage", id, name, Level.levelTime, frameTime, frameCount);
        #endif
        #endregion

        DispatchEvent(CreatureEvents.TAKE_DAMAGE, Event_.Pop(this, di));
    }

    public DamageInfo CalculateDamage(AttackInfo a, Creature victim, double damageMul = 1.0)
    {
        var back = m_fields;
        m_fields = m_tmpFields;

        Array.Copy(back, m_fields, m_fields.Length);

        DispatchEvent(CreatureEvents.CALCULATE_DAMAGE, Event_.Pop(victim, a));

        var de = a.execution ? victim.firm * 0.06 : victim.defense * 0.06;
        var re = de / (de + 20);
        var at = a.execution ? brutality : a.fromBullet ? offAttack : attack;
        var wd = a.damage * damageMul * at * (1 - re);  // weapon damage
        var ed = elementType != None ? a.damage * elementAttack - victim.GetElementDefense(elementType) : 0;  // element damage
        var cc = Mathd.Clamp01(crit - victim.resilience);
        var isCrit = moduleBattle._Range() < cc;

        var inc = damageIncrease;

        if (isCrit && critMul > 0)
        {
            wd *= critMul;
            ed *= critMul;
        }

        if (inc != 0)
        {
            wd += wd * inc;
            ed += ed * inc; 
        }

        if (wd < 0) wd = 0;
        if (ed < 0) ed = 0;

        m_fields = back;

        #region Debug log
        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        Logger.LogDetail("[{9}:{10}-{11}], Calculated damage: [{7}:{8}], elementType: {0}, attackInfo:{1}, type:{2}, damage:{3:F2}, weaponDamage:{4:F2}, elementDamage:{5:F2}, isCrit:{6}", eelementType, a.ID, a.execution ? "Execution" : a.fromBullet ? "Bullet" : "Normal", wd + ed, wd, ed, isCrit, id, name, Level.levelTime, frameTime, frameCount);
        #endif

        #if AI_LOG
        Module_AI.LogBattleMsg(this, "[Calculated damage: [{7}:{8}], elementType: {0}, attackInfo:{1}, type:{2}, damage:{3:F2}, weaponDamage:{4:F2}, elementDamage:{5:F2}, isCrit:{6}", eelementType, a.ID, a.execution ? "Execution" : a.fromBullet ? "Bullet" : "Normal", wd + ed, wd, ed, isCrit, name, teamIndex);
        #endif
        #endregion

        return DamageInfo.Create(Mathd.CeilToInt(wd), eelementType, Mathd.CeilToInt(ed), isCrit, a);
    }

    /// <summary>
    /// Warning: Do not call this function manually!
    /// </summary>
    /// <param name="victim"></param>
    /// <param name="a"></param>
    public void DispatchAttackEvent(Creature victim, AttackInfo a)
    {
        comboCount += 1;
        m_comboBreakCountDown = CombatConfig.sdefaultComboInterval;

        DispatchEvent(CreatureEvents.ATTACK, Event_.Pop(victim, a));
    }

    protected void UpdateComboState(int diff)
    {
        if (m_comboBreakCountDown < 0 || (m_comboBreakCountDown -= diff) > 0) return;

        var e = Event_.Pop(comboCount);

        comboCount = 0;
        m_comboBreakCountDown = -1;

        DispatchEvent(CreatureEvents.COMBO_BREAK, e);
    }

    #endregion

    #region Logic update

    public void UpdateWeapon(int _weaponID, int _itemID, bool _off = false)
    {
        if (!_off)
        {
            weaponID     = _weaponID;
            weaponItemID = _itemID;
        }
        else
        {
            offWeaponID     = _weaponID;
            offWeaponItemID = _itemID;
        }
    }

    public void Knockback(double d)
    {
        m_knockbackDistance = d;
        behaviour.Knockback(d); 
    }

    public void TurnBack()
    {
        direction = direction.Inverse();
    }

    public void FaceTo(Creature other)
    {
        var xx = other.x;
        var d = xx < m_x ? CreatureDirection.BACK : xx > m_x ? CreatureDirection.FORWARD : other.direction.Inverse();
        direction = d;
    }

    public void FaceTo(Transform other)
    {
        var d = other.position.x < m_x ? CreatureDirection.BACK : CreatureDirection.FORWARD;
        direction = d;
    }

    public void FaceTo(Vector3_ other)
    {
        var d = other.x < m_x ? CreatureDirection.BACK : CreatureDirection.FORWARD;
        direction = d;
    }

    public void UpdateDirection()
    {
        if (moveState == -1) direction = CreatureDirection.BACK;
        else if (moveState == 1) direction = CreatureDirection.FORWARD;
    }

    /// <summary>
    /// Callback when a state reached its maxFrame
    /// </summary>
    /// <param name="state"></param>
    protected virtual void OnStateEnded(StateMachineState state)
    {
        if ((state.isDie || state.isLaydown) && isDead && !realDead)
        {
            m_stateMachine.SetParam(StateMachineParam.realDead, true);

            realDead = true;

            DispatchEvent(CreatureEvents.DEAD_ANIMATION_END);

            if (lastAttacker)
                lastAttacker.DispatchEvent(CreatureEvents.KILL_TARGET_DEAD_ANIMATION_END, Event_.Pop(this));
        }
    }

    /// <summary>
    /// Callback when statemachine quit a state
    /// </summary>
    /// <param name="old"></param>
    /// <param name="now"></param>
    protected virtual void OnQuitState(StateMachineState old, StateMachineState now, float overrideBlendTime)
    {
        DispatchEvent(CreatureEvents.QUIT_STATE, Event_.Pop(old, now));

        if (!old || !old.passive && !now.passive && !now.preventTurn)
            UpdateDirection();

        RemoveBuff(BuffInfo.EffectTypes.Freez, true); // Always remove freez effect if creature enter new state
    }

    /// <summary>
    /// Callback when statemachine begin a new state
    /// </summary>
    /// <param name="old"></param>
    /// <param name="now"></param>
    protected virtual void OnEnterState(StateMachineState old, StateMachineState now, float overrideBlendTime)
    {
        m_currentState = now;

        if (m_currentState.off) --bulletCount;

        var blend = overrideBlendTime >= 0 ? overrideBlendTime : 0;

        if (old && old != m_currentState)
        {
            var toPassive   = m_currentState.passive;

            if (!toPassive) behaviour.hitCollider.Clear();
            
            if (overrideBlendTime < 0)
            {
                var fromPassive = old.passive;
                var fromIdle    = old.isIdle;
                var toIdle      = m_currentState.isIdle;

                blend = fromPassive ^ toPassive ? fromPassive ? CombatConfig.sblendFromPassive : CombatConfig.sblendToPassive :
                    fromPassive ? CombatConfig.sblendPassive : fromIdle ? CombatConfig.sblendFromIdle : toIdle ? CombatConfig.sblendToIdle : CombatConfig.sblendDefault;
            }
        }

        behaviour.ResetTimer();
        behaviour.attackCollider.gameObject.SetActive(false);

        stateFlag = m_currentState.off ? 1 : m_currentState.passive ? 2 : 0;

        position_ = m_currentState.currentMotionPos;
        pp = position_;
        motionOrigin = pp;

        m_fixedAnimationTime = m_currentState.time * 0.001f;

        if (animator.runtimeAnimatorController && !m_currentState.isLaydown && animator.gameObject.activeInHierarchy)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash != m_currentState.stateNameHash) animator.CrossFade(m_currentState.stateNameHash, blend);
            else animator.Play(m_currentState.stateNameHash, 0,  m_currentState.normalizedTime);

            animator.Update(0); // Prevent pose freez
        }

        if (m_currentState.isDie || isDead && m_currentState.isLaydown) behaviour.UpdateAllColliderState(false);

        DispatchEvent(CreatureEvents.ENTER_STATE, Event_.Pop(old, now));
    }

    /// <summary>
    /// Movement state.
    /// 0 = idle, -1 = left, 1 = right
    /// </summary>
    public int moveState
    {
        get { return m_moveState; }
        set
        {
            if (m_moveState == value) return;
            m_moveState = value;

            moving      = m_moveState != 0;
            rightMoving = m_moveState == 1;
            leftMoving  = m_moveState == -1;

            if (canMove) UpdateDirection();
            if (m_stateMachine) m_stateMachine.SetParam(StateMachineParam.moving, moving);
        }
    }
    private int m_moveState = 0;

    private Vector3_ m_position_;

    /// <summary>
    /// State flag
    /// -1 = none, 0 = main weapon state, 1 = off weapon state, 2 = passive state
    /// </summary>
    public int stateFlag { get; private set; }

    public bool leftMoving { get; private set; }
    public bool rightMoving { get; private set; }
    public bool moving { get; private set; }

    public bool canMove { get; protected set; }
    public bool onGround { get; protected set; }

    public virtual bool realMoving { get { return canMove && moving && m_currentState.isRun; } }

    public bool syncLogicPosition { get; protected set; }

    public Vector3_ position_
    {
        get { return m_position_; }
        set
        {
            if (m_position_ == value) return;
            m_position_ = value;
            m_x = m_position_.x;
            m_y = m_position_.y;

            behaviour.UpdateColliderPositions();
        }
    }

    /// <summary>
    /// The x position of creature, equals to position_.x
    /// </summary>
    public double x { get { return m_x; } }
    /// <summary>
    /// The y position of creature, equals to position_.y
    /// </summary>
    public double y { get { return m_y; } }

    private double m_x = 0, m_y = 0;

    /// <summary>
    /// The collider y position, equals to y + colliderOffset
    /// </summary>
    public double cy { get { return m_y + colliderOffset; } }

    public StateMachine stateMachine { get { return m_stateMachine; } }
    protected StateMachine m_stateMachine;

    public StateMachineState currentState { get { return m_currentState; } }
    protected StateMachineState m_currentState;

    public Animator animator { get; protected set; }

    public bool useSpringBone { get; protected set; }

    public SpringManager springManager { get; protected set; }

    public Transform model { get; protected set; }

    public Transform head { get; protected set; }

    public Projector shadow { get { return m_shadow; } }
    private Projector m_shadow;

    public Transform activeRootNode { get { return m_activeRootNode; } }
    private Transform m_activeRootNode;

    public Transform hairNode { get; protected set; }

    public bool isForward { get { return m_isForward; } }
    private bool m_isForward = false;

    public HashSet<Creature> markList = new HashSet<Creature>();

    public virtual CreatureDirection direction
    {
        get { return m_direction; }
        set
        {
            if (m_direction == value) return;
            m_direction = value;


            FightRecordManager.RecordLog< LogSetDirection>(l =>
            {
                l.tag = (byte)TagType.SetDirection;
                l.roldId = Identify;
                l.direction = (sbyte)value;
            });

            m_isForward = m_direction == CreatureDirection.FORWARD;

            #region Debug log
            #if (DEVELOPMENT_BUILD || UNITY_EDITOR) && DETAIL_BATTLE_LOG
            //Logger.LogDetail("[{2}:{3}-{4}], Creature {0} forward set to {1}", name, m_isForward, Level.levelTime, frameTime, frameCount);
            #endif

            #if AI_LOG
            Module_AI.LogBattleMsg(this, "[forward set to {0}]", m_isForward);
            #endif
            #endregion

            transform.forward = m_isForward ? Vector3.right : Vector3.left;

            m_modelInverter.invert = !m_isForward;
            behaviour?.UpdateWeaponEffectInvert();
            position = position_;

            DispatchEvent(CreatureEvents.DIRECTION_CHANGED);
        }
    }
    private CreatureDirection m_direction = CreatureDirection.FORWARD;

    public Vector3_ motionOrigin { get; protected set; }

    private double m_regenHealth = 0;

    private int m_lastTeamIndex = -1;
    private bool m_useAI = false;
    public bool useAI
    { 
        get { return m_useAI; }
        set
        {
            if (!m_stateMachine || m_useAI == value) return;

            m_useAI = value;
            m_stateMachine.ResetTransitions(m_useAI);
            if (m_lastTeamIndex == -1) m_lastTeamIndex = teamIndex;
            teamIndex = m_useAI ? MonsterCreature.GetMonsterRoomIndex() : m_lastTeamIndex;
            DispatchEvent(CreatureEvents.AUTO_BATTLE_CHANGE, Event_.Pop(value));
        }
    }

    public override void OnUpdate(int diff)
    {
        UpdateInputActions();  // We always handle input before logic update complete
        UpdateRegen(diff);

        if (m_stateMachine == null) return;

        m_stateMachine.Update(diff);
        canMove = m_currentState.isIdle || m_currentState.isRun;

        UpdateMovement(diff);
        UpdateGroundState();
        UpdateComboState(diff);
        UpdateBuffs(diff);
    }

    public override void UpdateVisibility()
    {
        if (!m_activeRootNode) return;
        var v = m_activeRootNode.gameObject.activeSelf;
        m_activeRootNode.gameObject.SetActive(visible);
        if (!v && visible) OnEnable();
    }

    public virtual bool UpdateStateMachine(int _weaponID = -1)
    {
        var wid = _weaponID < 0 ? m_weaponID : _weaponID;

        StateMachineInfo sm = null;
        if (isCombat)
        {
            sm = ConfigManager.Get<StateMachineInfo>(wid);

            if (!sm)
            {
                Logger.LogError("Creature::UpdateStateMachine: Invalid weapon ID <b>{0}</b>, creature: <b>[{1}:{2}-{3}]</b>", wid, configID, name, uiName);
                return false;
            }

            m_weaponID = wid;


            var of = ConfigManager.Get<StateMachineInfoOff>(1);
            var pa = ConfigManager.Get<StateMachineInfoPassive>(sm.passiveGroup);
            
            if (m_stateMachine) m_stateMachine.Destroy();
            m_stateMachine = StateMachine.Create(sm, this, isRobot, of, pa);
            m_stateMachine.Rebuild();
        }
        else
        {
            sm = ConfigManager.Get<StateMachineInfoSimple>(wid);
            if (!sm)
            {
                Logger.LogError("Creature::UpdateStateMachine: Invalid simple weapon ID <b>{0}</b>, creature: <b>[{1}:{2}-{3}]</b>", wid, configID, name, uiName);
                return false;
            }

            m_weaponID = wid;

            if (m_stateMachine) m_stateMachine.Destroy();
            m_stateMachine = StateMachine.Create(sm, this, isRobot);
            m_stateMachine.Rebuild();
        }

        m_stateMachine.onStateEnded += OnStateEnded;
        m_stateMachine.onEnterState += OnEnterState;
        m_stateMachine.onQuitState  += OnQuitState;

        m_stateMachine.SetParam(StateMachineParam.configID, m_configID);
        m_stateMachine.SetParam(StateMachineParam.morph, (int)m_morph);
        m_stateMachine.SetParam(StateMachineParam.isDead, isDead);
        m_stateMachine.SetParam(StateMachineParam.realDead, false);
        m_stateMachine.SetParam(StateMachineParam.rage, rage + 0.0001);
        m_stateMachine.SetParam(StateMachineParam.energy, m_energy);
        m_stateMachine.SetParam(StateMachineParam.energyRate, energyRateL);
        m_stateMachine.SetParam(StateMachineParam.speedRun, speedRun);
        m_stateMachine.SetParam(StateMachineParam.speedAttack, speedAttack);
        m_stateMachine.SetParam(StateMachineParam.weaponItemID, weaponItemID);
        m_stateMachine.SetParam(StateMachineParam.offWeaponItemID, offWeaponItemID);
        m_stateMachine.SetParam(StateMachineParam.onGround, onGround);

        var spm = GetComponent<SpringManager>();
        if (spm) spm.currentWeapon = m_weaponID;

        behaviour?.UpdateWeaponAnimator();

        OnEnterState(null, m_stateMachine.currentState, -1);

        return true;
    }

    protected virtual void UpdateInputActions()
    {
        if (useAI) return;

        var s = moduleBattle.GetInput(teamIndex);

        moveState = s.BitMask(Module_Battle.KEY_LEFT) ? -1 : s.BitMask(Module_Battle.KEY_RIGHT) ? 1 : 0;  // Must before we parse input key due to key direction bind

        if (m_currentState.acceptInput)
            ParseInputKeys(s);
    }

    protected virtual void UpdateRegen(int diff)
    {
        if (isDead || moduleBattle.inDialog) return;

        var dt = diff * 0.001;
        if (regenHealth != 0 || regenHealthPercent != 0)
        {
            if (regenHealth != 0) m_regenHealth += regenHealth * (1 + regenHealthMul) * dt;
            if (regenHealthPercent != 0) m_regenHealth += regenHealthPercent * maxHealth * dt;

            var re = (int)m_regenHealth;
            if (re != 0)
            {
                m_regenHealth -= re;
                health += re;
            }
        }

        var r = 0.0;
        if (regenRage != 0) r += regenRage * (1 + regenRageMul) * dt;
        if (regenRagePercent != 0) r += regenRagePercent * maxRage * dt;

        if (r != 0) rage += r;
    }

    private void UpdateGroundState()
    {
        var t = onGround;
        onGround = position_.y <= 0;

        if (t ^ onGround) m_stateMachine?.SetParam(StateMachineParam.onGround, onGround);
    }

    private void ParseInputKeys(int inputs)
    {
        var key = m_stateMachine.GetInt(StateMachineParam.key);  // Get current cached keys
        key &= 0xFFFFFFF;

        // Inputs bitmask:The first 20 bits contains 4 input id. every 5 bit stored one input id, 20-29 contains hold input, last 2 bits: left and right movement state
        // Key bitmask:The first 28 bits contains 4 key value. every 7 bit stored one key, 0 - 4 = value  5 = negative ?  6 = mark, 28-30: new key count, last bit: unused

        int tmp = 0, h1 = 0, h2 = 0; // Clear hold input
        for (int i = 0, c = 0; i < 4; ++i)
        {
            var k = (key >> i * 7) & 0x7F;
            if (k == 0) break;
            if (k.BitMask(6))
            {
                if (h1 == 0) h1 = k;
                else h2 = k;
                continue;
            }
            tmp |= k << c++ * 7;
        }
        key = tmp;

        if ((inputs &= 0x3FFFFFFF) == 0) // We do not have any input
        {
            m_stateMachine.SetParam(StateMachineParam.key, key);
            return;
        }

        int offset = inputs.BitMask(24) ? inputs.BitMask(29) ? 2 : 1 : 0, limit = 4 + offset, ncc = 0;
        for (var j = 0; j < limit; ++j)
        {
            var hold = j < offset;

            int imask = 0, iid = 0;
            if (hold)
            {
                var hoff = Module_Battle.INSTANT_KEY_BITS + (offset - j - 1) * 5;
                imask = inputs >> hoff & 0x1F;
                iid = imask & 0x0F;   // Hold input key id always use 4 bits, means hold input id must less than 16
            }
            else
            {
                imask = inputs >> (j - offset) * 5 & 0x1F;
                iid = imask;
            }

            if (imask == 0) break;

            var ic = InputKeyInfo.GetCachedKeyInfo(iid);
            if (!ic) continue;
            if (!m_stateMachine.AcceptKeyNow(ic.value)) continue;

            var val = (ic.value < 0 ? -ic.value : ic.value) & 0x1F; // Only 5 bit used for key value
            var negative = direction.SameAs(ic.bindDirection) ? ic.value < 0 : ic.value > 0; // Character direction bind

            if (negative) val = val.BitMask(5, true);
            if (hold)     val = val.BitMask(6, imask.BitMask(4));

            key = key << 7 & 0xFFFFFFF | val;
            if (h1 != val && h2 != val) ncc++;
        }

        //if (isPlayer) Logger.LogError("Input set to {0}", key);

        key |= (ncc & 0x07) << 28;

        m_stateMachine.SetParam(StateMachineParam.key, key);
    }

    public Vector3_ pp, cp;  // prev frame position / self changed position to simulate motion

    private double m_knockbackDistance;

    private void UpdateMovement(int diff)
    {
        syncLogicPosition = false;

        cp.Set(0, 0, 0);

        var tp = position_;

        // if position changed external, we should add changed position to current target
        if (pp != tp) cp += tp - pp;

        if (m_stateMachine.playable)
        {
            // movement and animation motion
            if (!m_currentState.inMotion)
            {
                if (!onGround)
                {
                    var df = diff * m_currentState.fallSpeed * 0.001;
                    tp += df;
                    cp += df;
                }
                else if (realMoving)
                {
                    var moveDst = speedRun * standardRunSpeed * diff * 0.001;
                    if (direction == CreatureDirection.BACK) moveDst = -moveDst;

                    tp.x += moveDst;
                    cp.x += moveDst;
                }
            }
            else
                tp = m_currentState.currentMotionPos;

            // knockback
            if (m_knockbackDistance != 0)
            {
                var kd = m_knockbackDistance * 0.01 * diff;
                m_knockbackDistance -= kd;
                if (kd < 0 && m_knockbackDistance > -0.01 || kd > 0 && m_knockbackDistance < 0.01)
                    m_knockbackDistance = 0;

                tp.x += kd;
                cp.x += kd;
            }
        }
        else syncLogicPosition = true;

        // collision check
        if (tp != position_)
        {
            Creature cc = null;
            var dd = double.MaxValue;
             
            double bottom = cy, top = bottom + colliderHeight;
            ObjectManager.Foreach<Creature>(c =>
            {
                if (!c.visible || c.isDead || IgnoreObstacleMask(c.obstacleMask) && (!behaviour.collider_.isActiveAndEnabled || c.passiveEthereal || c.SameCampWith(this))) return true;

                //如果要移动的方向和阻挡所在方向不一致忽略阻挡（向右移动时不检测左侧的阻挡）
                if ((tp.x - position_.x) * (c.x - position_.x) < 0) return true;

                var cbottom = c.cy;
                if (bottom >= cbottom + c.colliderHeight || top <= cbottom) return true;
                //盒子之间的缝隙
                var xx = Mathd.Abs(x  - c.x) - (colliderSize + c.colliderSize);

                var d = xx * xx;
                //这里存在一个精度问题。Check2D返回的值可能就会导致两个盒子交在一起。间隙变为-1E20等
                //故这里判断相交设定一个精度值
                if ((xx < 0 && Mathd.Abs(xx) > 0.0001) || d > dd) return true;


                cc = c;
                dd = d;

                return true;
            });

            if (cc)
            {
                var ttp = behaviour.collider_.Check2D(tp, cc);
                if (ttp != tp)
                {
                    cp += ttp - tp;
                    tp = ttp;

                    syncLogicPosition = true;
                }
            }
        }

        PetCreature pet = this as PetCreature;
        if (pet != null)
        {
            var c = Mathd.Clamp(tp.x, pet.followEdge.x, pet.followEdge.y);
            if (c != tp.x)
            {
                cp.x += c - tp.x;
                tp.x = c;

                syncLogicPosition = true;
            }
        }

        // edge check
        if (m_checkEdge)
        {      
            var edge = Level.current.edge;
            if (edge.x <= edge.y)
            {
                var c = Mathd.Clamp(tp.x, edge.x, edge.y);

                if (c != tp.x)
                {
                    cp.x += c - tp.x;
                    tp.x = c;

                    syncLogicPosition = true;
                }
            }
        }

        // ground check
        if (tp.y < 0)
        {
            cp.y -= tp.y;
            tp.y = 0;
        }

        if (Math.Abs(cp.sqrMagnitude) > double.Epsilon)
        {
            motionOrigin += cp;
            FightRecordManager.RecordLog<LogVector3>(l =>
            {
                l.tag = (byte) TagType.motionOrigin;
                l.pos = new double[] {motionOrigin.x, motionOrigin.y, motionOrigin.z};
            });
        }

        position_ = tp;
        pp = position_;

        FightRecordManager.RecordLog< LogUpdateMovement>(log =>
        {
            log.tag = (byte)TagType.UpdateMovement;
            log.roleId = Identify;
            log.position_ = new double[3];
            log.position_[0] = position_.x;
            log.position_[1] = position_.y;
            log.position_[2] = position_.z;
            log.realMoving = realMoving;
            log.standardRunSpeed = standardRunSpeed;
            log.direction = (sbyte)direction;
        });
    }

    #endregion

    #region Special states (tough...)

    private struct SpecialState { public int id; public int level; }

    private int m_toughID = 0;
    private int m_breakToughID = 0;
    private List<SpecialState> m_toughState = new List<SpecialState>();
    private List<SpecialState> m_breakToughState = new List<SpecialState>();

    public int AddToughState(int level)
    {
        var toughID = ++m_toughID;
        m_toughState.Add(new SpecialState() { id = toughID, level = level });
        
        if (level > m_toughLevel) m_toughLevel = level;

        UpdateToughState(false);

        return toughID;
    }

    public int AddBreakToughState(int level)
    {
        var toughID = ++m_breakToughID;
        m_breakToughState.Add(new SpecialState() { id = toughID, level = level });

        if (level > m_breakToughLevel) m_breakToughLevel = level;

        UpdateBreakToughState(false);

        return toughID;
    }

    public void RemoveToughState(int toughID)
    {
        var idx = m_toughState.FindIndex(s => s.id == toughID);
        if (idx > -1)
        {
            m_toughState.RemoveAt(idx);
            UpdateToughState(true);
        }
    }

    public void RemoveBreakToughState(int toughID)
    {
        var idx = m_breakToughState.FindIndex(s => s.id == toughID);
        if (idx > -1)
        {
            m_breakToughState.RemoveAt(idx);
            UpdateBreakToughState(true);
        }
    }

    public void RemoveToughState(List<int> ids)
    {
        if (ids == null || ids.Count < 1 || m_toughState.Count < 1) return;

        foreach (var tid in ids)
        {
            for (int i = m_toughState.Count - 1; i > -1; --i)
            {
                if (m_toughState[i].id != tid) continue;
                m_toughState.RemoveAt(i);
            }
        }

        UpdateToughState(true);
    }

    public void RemoveToughState(int[] ids)
    {
        if (ids == null || ids.Length < 1 || m_toughState.Count < 1) return;

        foreach (var tid in ids)
        {
            for (int i = m_toughState.Count - 1; i > -1; --i)
            {
                if (m_toughState[i].id != tid) continue;
                m_toughState.RemoveAt(i);
            }
        }

        UpdateToughState(true);
    }

    public void RemoveBreakToughState(List<int> ids)
    {
        if (ids == null || ids.Count < 1 || m_breakToughState.Count < 1) return;

        foreach (var tid in ids)
        {
            for (int i = m_breakToughState.Count - 1; i > -1; --i)
            {
                if (m_breakToughState[i].id != tid) continue;
                m_breakToughState.RemoveAt(i);
            }
        }

        UpdateBreakToughState(true);
    }

    public void RemoveBreakToughState(int[] ids)
    {
        if (ids == null || ids.Length < 1 || m_breakToughState.Count < 1) return;

        foreach (var tid in ids)
        {
            for (int i = m_breakToughState.Count - 1; i > -1; --i)
            {
                if (m_breakToughState[i].id != tid) continue;
                m_breakToughState.RemoveAt(i);
            }
        }

        UpdateBreakToughState(true);
    }

    public void ClearToughState()
    {
        m_toughState.Clear();
        m_breakToughState.Clear();
        m_toughCount = 0;
        m_toughLevel = 0;
        m_breakToughLevel = 0;
    }

    private void UpdateToughState(bool updateLevel)
    {
        m_toughCount = m_toughState.Count;

        var old = tough;
        tough = m_toughCount > 0;

        if (updateLevel)
        {
            m_toughLevel = int.MinValue;
            if (tough) m_toughState.ForEach(s => { if (s.level > m_toughLevel) m_toughLevel = s.level; });
            else m_toughLevel = 0;
        }

        if (old ^ tough) UpdateRimLight();
    }

    private void UpdateBreakToughState(bool updateLevel)
    {
        if (updateLevel)
        {
            m_breakToughLevel = int.MinValue;
            if (tough) m_breakToughState.ForEach(s => { if (s.level > m_breakToughLevel) m_breakToughLevel = s.level; });
            else m_breakToughLevel = 0;
        }
    }

    #endregion

    #region Special Fields
    private class SpecialField
    {
        public Creature target;
        public double[] fields;

        public double this[CreatureFields fieldType]
        {
            get { AssertArray(); return fields[(int)fieldType]; }
            set { AssertArray(); fields[(int)fieldType] = value; }
        }

        private void AssertArray()
        {
            if (fields == null)
                fields = new double[(int) CreatureFields.Count];
        }
    }
    //特殊属性只针对目标角色有效
    private readonly Dictionary<Creature, SpecialField> specialFields = new Dictionary<Creature, SpecialField>();

    public double AddSpecialProperty(Creature rCreature, CreatureFields rType, double rValue)
    {
        if (rCreature == null)
            return 0;
        if (!specialFields.ContainsKey(rCreature))
            specialFields.Add(rCreature, new SpecialField() {target = rCreature});

        var sf = specialFields[rCreature];
        sf[rType] += rValue;
        return rValue;
    }

    public double GetSpecialProperty(Creature rCreature, CreatureFields rType)
    {
        if (!specialFields.ContainsKey(rCreature))
            return 0;
        return specialFields[rCreature][rType];
    }
    #endregion

    #region Render update

    private float m_fixedAnimationTime = 0;

    /// <summary>
    /// Force current animator state sync with current state machine state
    /// </summary>
    public void UpdateAnimatorForce()
    {
        if (!animator || !animator.runtimeAnimatorController || !animator.gameObject.activeInHierarchy) return;

        var normalize = m_fixedAnimationTime / m_currentState.floatLength;

        animator.Play(m_currentState.stateNameHash, 0, normalize);

        var cull = animator.cullingMode; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.Update(0);
        animator.cullingMode = cull;
    }

    public void UpdateAnimator()
    {
        if (currentState.isLaydown) return;
        UpdateAnimatorForce();
    }

    public void OnRenderUpdate()
    {
        if (!enableUpdate || m_stateMachine == null || !m_stateMachine.playable || !onGround && m_currentState.falling) return;

        var delta = Util.GetMillisecondsTime(m_baseBehaviour.delta * m_currentState.animationSpeed);
        var diff = m_currentState.waitToLoop ? 0 : m_currentState.time * 0.001f - m_fixedAnimationTime;
        if (diff > 0.1f || diff < -0.1f) delta += diff * delta;

        m_fixedAnimationTime += delta;

        if (!m_currentState.isLaydown && animator.gameObject.activeInHierarchy) animator.Update(delta);
    }

    public void OnPostRenderUpdate() { }

    public override void OnEnable()
    {
        if (m_currentState)
        {
            m_fixedAnimationTime = m_currentState.time * 0.001f;
            UpdateAnimator();
        }
    }

    #endregion

    #region Obstacle mask

    /// <summary>
    /// 当前角色是否包含指定阻挡类型
    /// <para>See <see cref="obstacleMask"/></para>
    /// </summary>
    /// <param name="layer">阻挡类型</param>
    /// <returns></returns>
    public bool IsObstacle(int layer)
    {
        return obstacleMask != 0 && obstacleMask.BitMask(layer);
    }

    /// <summary>
    /// 当前角色是否包含指定列表的阻挡类型
    /// <para>See <see cref="obstacleMask"/></para>
    /// </summary>
    /// <param name="layers">阻挡类型列表</param>
    /// <returns></returns>
    public bool IsObstacles(int[] layers)
    {
        if (obstacleMask == 0 || layers == null || layers.Length < 0) return false;
        foreach (var layer in layers)
        {
            if (obstacleMask.BitMask(layer)) continue;
            return false;
        }
        return true;
    }

    /// <summary>
    /// 当前角色是否包含指定阻挡类型
    /// <para>See <see cref="obstacleMask"/></para>
    /// </summary>
    /// <param name="mask">阻挡类型</param>
    /// <returns></returns>
    public bool IsObstacleMask(int mask)
    {
        return (obstacleMask & mask) == mask;
    }

    /// <summary>
    /// 当前角色是否忽略指定阻挡类型
    /// <para>See <see cref="ignoreObstacleMask"/></para>
    /// </summary>
    /// <param name="layer">阻挡类型</param>
    /// <returns></returns>
    public bool IgnoreObstacle(int layer)
    {
        return ignoreObstacleMask != 0 && ignoreObstacleMask.BitMask(layer);
    }

    /// <summary>
    /// 当前角色是否忽略指定列表的阻挡类型
    /// <para>See <see cref="ignoreObstacleMask"/></para>
    /// </summary>
    /// <param name="layers">阻挡类型列表</param>
    /// <returns></returns>
    public bool IgnoreObstacles(int[] layers)
    {
        if (ignoreObstacleMask == 0 || layers == null || layers.Length < 0) return false;
        foreach (var layer in layers)
        {
            if (ignoreObstacleMask.BitMask(layer)) continue;
            return false;
        }
        return true;
    }

    /// <summary>
    /// 当前角色是否忽略指定阻挡类型
    /// <para>See <see cref="ignoreObstacleMask"/></para>
    /// </summary>
    /// <param name="mask">阻挡类型</param>
    /// <returns></returns>
    public bool IgnoreObstacleMask(int mask)
    {
        return (ignoreObstacleMask & mask) == mask;
    }

    #endregion

    #region Buffs

    private void UpdateBuffs(int diff)
    {
        if (!moduleBattle.fightStarted || moduleBattle.inDialog) return;  // Do not update buffs in prepare state or dialog state

        for (var i = 0; i < m_buffs.Count;) 
        {
            var buff = m_buffs[i];
            buff.Update(diff);
            if (buff.destroyed) _RemoveBuff(buff);
            else ++i;

            if (buff.isSwitchMorph) DispatchEvent(CreatureEvents.MORPH_PROGRESS, Event_.Pop(m_morph, buff));
        }
    }

    public bool AddBuff(Buff buff)
    {
        /*var old = m_buffs.FindIndex(b => b.ID == buff.ID && b.source == buff.source);
        if (old > -1)
        {
            m_buffs[old].Remove();
            m_buffs[old] = buff;
        }
        else */if (buff.info.applyType == BuffInfo.ApplyTypes.Override)
        {
            var old = m_buffs.FindLastIndex(b => b && b.HasSameEffectType(buff.info) != BuffInfo.EffectTypes.Unknow 
                                                              && b.info.applyType == BuffInfo.ApplyTypes.Override
                                                              && (!buff.info.dependSource || b.source == buff.source));
            if (old > -1 && m_buffs[old].info.priority <= buff.info.priority)
                m_buffs[old].Remove(3);
        }
        else if (buff.info.applyType == BuffInfo.ApplyTypes.OverrideEx)
        {
            var old = m_buffs.FindLastIndex(b => b && b.info.ID == buff.ID
                                          && (!buff.info.dependSource || b.source == buff.source));
            if (old > -1)
                m_buffs[old].Remove(3);
        }
        m_buffs.Add(buff);
        m_elementAffected = m_buffs.FindLastIndex(b => b && b.sourceBuffID == 0) > -1;

        return true;
    }

    public void RemoveBuff(int ID, bool ignorePending = false, int count = 0)
    {
        var c = 0;
        foreach (var buff in m_buffs)
        {
            if (buff.destroyed) continue;
            if (buff.ID == ID)
            {
                buff.Remove(1, ignorePending);
                if (count > 0 && ++c <= count)
                    break;
            }
        }
    }

    public void RemoveBuff(BuffInfo.EffectTypes type, bool ignorePending = false)
    {
        foreach (var buff in m_buffs)
        {
            if (buff.destroyed) continue;
            if (buff.HasEffect(type))
                buff.Remove(1, ignorePending);
        }
    }

    public Buff GetBuffByID(int id)
    {
        return m_buffs.Find(b => b.ID == id);
    }

    public List<Buff> GetBuffsByType(BuffInfo.EffectTypes type)
    {
        var buffs = new List<Buff>();
        foreach (var buff in m_buffs)
        {
            if (buff.destroyed) continue;
            if (buff.HasEffect(type))
                buffs.Add(buff);
        }

        return buffs;
    }

    public List<Buff> GetBuffList()
    {
        return m_buffs;
    }

    public bool HasBuffType(BuffInfo.EffectTypes type)
    {
        foreach (var buff in m_buffs)
            if (buff.HasEffect(type)) return true;

        return false;
    }

    public bool HasBuffType(BuffInfo info)
    {
        foreach (var buff in m_buffs)
            if (buff.HasSameEffectType(info) != BuffInfo.EffectTypes.Unknow) return true;

        return false;
    }

    public bool HasBuff(int id)
    {
        foreach (var buff in m_buffs)
            if (buff && buff.ID == id) return true;
        return false;
    }

    public bool HasBuff(BuffInfo info)
    {
        if (!info) return false;

        foreach (var buff in m_buffs)
            if (buff.info == info) return true;
        return false;
    }

    public bool HasBuff(System.Predicate<Buff> match)
    {
        return m_buffs.FindIndex(match) > 0;
    }

    private void _RemoveBuff(Buff buff)
    {
        m_buffs.Remove(buff);
        m_elementAffected = m_buffs.FindIndex(b => b && b.sourceBuffID == 0) > -1;
    }

    public void ClearBuffs()
    {
        foreach (var buff in m_buffs) buff.Destroy();
        m_buffs.Clear();
        //因为觉醒时先切模型，后加觉醒buff。变回正常模型由觉醒buff控制。如果切完模型后角色就胜利了。觉醒buff就不会加了也就无法切回正常形态了
        //清除buff时，强制切换为正常形态
        SwitchMorphModel();
        m_elementAffected = false;
    }

    #endregion

    #region ILogicTransform

    public Vector3_ LogicPosition
    {
        get { return position_; }
        set { position_ = value; }
    }

    #endregion

    #region Editor Helper

#if UNITY_EDITOR
    public bool lockLayer = false;
    public int normalLayer = 0;
    public int lockedLayer = 0;

    public void SetLayer_(int layer, bool ignoreLock = false)
    {
        normalLayer = layer;

        layer = lockLayer && !ignoreLock ? lockedLayer : layer;

        if (layer < 0) ResetLayers();
        else if (model)
        {
            Util.SetLayer(model.gameObject, layer);
            Util.SetLayer(hairNode, layer);
            Util.SetLayer(m_shadow.gameObject, layer);
            Util.SetLayer(behaviour.effects.gameObject, layer);

            behaviour.SetWeaponLayer(layer, layer);
        }
    }
#endif

    #endregion
}
