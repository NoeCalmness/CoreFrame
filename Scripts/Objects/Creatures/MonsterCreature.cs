using UnityEngine;
using UnityEngine.UI;

public sealed class MonsterCreature : Creature
{
    #region Static functions

    private static int m_originalRoomIndex = int.MinValue;

    public static void ResetMonsterRoomIndex()
    {
        m_originalRoomIndex = int.MinValue;
    }

    public static int GetMonsterRoomIndex()
    {
        FightRecordManager.RecordLog<LogInt>(log =>
        {
            log.tag = (byte)TagType.GetMonsterRoomIndex;
            log.value = m_originalRoomIndex;
        });

        return m_originalRoomIndex++;
    }

    public static MonsterCreature CreateMonster(int monsterID, int group, int level, Vector3_ pos, Vector3 rot, StageInfo stage = null, string name = "", string uiName = "", bool needHealthBar = true)
    {
        var info = ConfigManager.Get<MonsterInfo>(monsterID);
        if (info == null)
        {
            Logger.LogError("MonsterCreature::Create: Create monster failed, could not find monster [{0}]", monsterID);
            return null;
        }

        var rootNode = new GameObject().transform;
        if (!CreateMorphNodes(info.models, rootNode))
        {
            Logger.LogError("MonsterCreature::Create: Create monster [{0}:{1}] failed, main model [{2}] not loaded", monsterID, info.name, CreatureInfo.GetMorphModelName(info.models, 0));
            return null;
        }

        rootNode.position    = pos;
        rootNode.eulerAngles = rot;

        var c = Create<MonsterCreature>(string.IsNullOrEmpty(name) ? info.name : name, rootNode.gameObject);
        c.isCombat = true;
        c.InitPosition(pos);

        if (info.npcId > 0)
        {
            var npc = moduleNpc.GetTargetNpc(info.npcId);
            if (npc != null)
                CharacterEquip.ChangeNpcFashion(c, npc.mode);
            else
                Logger.LogError($"monster表里monsterID = {info.npcId} 配置的NpcID错误,没有ID = {info.npcId}的Npc");
        }

        //如果配置有属性修正，则直接使用玩家的等级作为参考模板
        int targetLevel = (stage != null && stage.levelCorrection) ? modulePlayer.roleInfo.level : level;

        double aggr = stage != null ? 1.0 + stage.aggressiveCorrection : 1.0;
        double defen = stage != null ? 1.0 + stage.defensiveCorrection : 1.0;

        bool isSuccess = false;
        MonsterAttriubuteInfo attribute = ConfigManager.Get<MonsterAttriubuteInfo>(info.AttributeConfigId);

        if (!attribute)
        {
            var attList = ConfigManager.GetAll<MonsterAttriubuteInfo>();
            attribute = attList.GetValue(0);
            Logger.LogError("Could not find MonsterAttriubuteInfo with monsterId {0} AttributeConfigId [{1}] we will use attri with id {2} to replace", monsterID, info.AttributeConfigId, attribute ? attribute.ID : -1);
        }

        if(attribute)
        {
            for (int i = 0, length = attribute.monsterAttriubutes.Length; i < length; i++)
            {
                if (targetLevel == attribute.monsterAttriubutes[i].level)
                {
                    isSuccess = true;
                    c.UpdateConfig(info, attribute.monsterAttriubutes[i], aggr, defen);
                    c._SetField(CreatureFields.Level, targetLevel);                //怪物等级在怪物创建的时候去确定,初始化不赋值
                    break;
                }
            }

            //To avoid level is invalid
            if (!isSuccess)
            {
                MonsterAttriubuteInfo.MonsterAttriubute attri = attribute.monsterAttriubutes[attribute.monsterAttriubutes.Length - 1];
                int maxLevel = attri.level;
                c.UpdateConfig(info, attri, aggr, defen);
                c._SetField(CreatureFields.Level, maxLevel);                //怪物等级在怪物创建的时候去确定,初始化不赋值
                Logger.LogError("怪物等级修正非法!player.level = {0} ,monster max level = {1} ", modulePlayer.roleInfo.level, maxLevel);
            }
        }

        c.isPlayer = false;
        c.isMonster = true;
        c.isRobot = false;
        c.roleProto = 0;

        c.uiName = string.IsNullOrEmpty(uiName) ? info ? info.name : c.name : uiName;
        //怪物属性记录
        c.monsterInfo = info;
        c.monsterId = monsterID;
        c.monsterGroup = group;
        c.creatureCamp = group >= -1 ? CreatureCamp.MonsterCamp : group == -2 ? CreatureCamp.PlayerCamp : group == -3 ? CreatureCamp.NeutralCamp : CreatureCamp.None;
        c.monsterLevel = level;
        c.isBoss = false;
        c.isSendDeathCondition = false;
        c.teamIndex = GetMonsterRoomIndex();

        c.obstacleMask = info.iobstacleMask;
        c.ignoreObstacleMask = info.iignoreObstacleMask;

        c.gameObject.name = Util.Format("{0}-> id:{1} group:{2} tIdx:{3}", info.name, info.ID, group,c.Identify);

        c.OnCreate();

        c.hpVisiableCount = needHealthBar ? 1 : 0;

        return c;
    }

    public static MonsterCreature CreateBordlandMonster(PNmlMonster monsterData, Vector3_ pos, Vector3 rot, string name = "", string uiName = "")
    {
        if (monsterData == null)
        {
            Logger.LogError("MonsterCreature::CreateBordlandMonster: Create monster failed, invalid monster data");
            return null;
        }

        var rootNode = new GameObject().transform;

        if (!CreateMorphNodes(new string[] { monsterData.mesh.ToLower() }, rootNode))
        {
            Logger.LogError("MonsterCreature::CreateBordlandMonster: Create monster failed, model [{0}] not loaded", monsterData.mesh.ToLower());
            return null;
        }

        rootNode.position    = pos;
        rootNode.eulerAngles = rot;

        var c = Create<MonsterCreature>(string.IsNullOrEmpty(name) ? Util.Format("bordland_monster_{0}",monsterData.uid) : name , rootNode.gameObject);
        
        c.InitPosition(pos);
        c.isPlayer = false;
        c.isMonster = true;

        c.uiName = string.IsNullOrEmpty(uiName) ? c.name : uiName;
        c.monsterId = monsterData.monster;
        //怪物属性记录
        c.monsterInfo = ConfigManager.Find<MonsterInfo>(config => config.GetModel() == monsterData.mesh.ToLower());
        c.height = (c.monsterInfo && c.monsterInfo.hitColliderSize.y > 0 ) ? c.monsterInfo.hitColliderSize.y : 2.0f;
        c.monsterUID = monsterData.uid;
        c.monsterGroup = 0;
        c.creatureCamp = CreatureCamp.MonsterCamp;
        c.monsterLevel = 0;
        c.isBoss = false;
        c.isSendDeathCondition = false;
        return c;
    }
    #endregion

    #region PVE相关属性
    
    public ulong monsterUID;

    public int monsterId;

    public int monsterGroup;

    public int monsterLevel;

    public bool isBoss { get; set; } = false;
    
    //避免砍死尸体造成多次抛进事件
    public bool isSendDeathCondition;

    //怪物配置表信息
    public MonsterInfo monsterInfo;

    public override ulong Identify { get { return (ulong)teamIndex; } }

    /// <summary>
    /// 忽略AI转身
    /// </summary>
    public bool ignoreAITurn { get { return monsterInfo ? monsterInfo.ignoreTurn : false; } }
    public EnumMonsterType monsterType { get { return monsterInfo ? monsterInfo.monsterType : EnumMonsterType.NormalType; } }

    /// <summary>
    /// 是否是可破坏物
    /// </summary>
    public bool isDestructible { get { return monsterType == EnumMonsterType.Destructible; } }

    /// <summary>
    /// 死亡保留模型
    /// </summary>
    public bool deathAppear { get { return monsterInfo ? monsterInfo.deathAppear : false; } }

    /// <summary>
    /// 强制锁定方向.
    /// 设置强制锁定方向后，怪物的锁敌方向不再会发生变化
    /// </summary>
    private int m_forceDirection = 0;
    public int forceDirection
    {
        get { return m_forceDirection; }
        set
        {
            m_forceDirection = value;
            if (m_forceDirection == 0) return;

            base.direction = m_forceDirection == -1 ? CreatureDirection.BACK : CreatureDirection.FORWARD;
        }
    }

    /// <summary>
    /// 是否是
    /// </summary>
    public bool isForceDirection { get { return forceDirection != 0; } }

    /// <summary>
    /// 是否是需要中途领奖的boss
    /// </summary>
    public bool getReward { get; set; }

    /// <summary>
    /// 锁敌状态，如果锁敌状态是关闭的话，就不再进行锁敌目标切换检测和场景怪物提示
    /// </summary>
    public EnumLockMonsterType lockEnermyType { get { return monsterInfo ? monsterInfo.lockType : EnumLockMonsterType.LockEnermyCamp; } }
    #endregion

    public override CreatureDirection direction
    {
        get
        {
            return base.direction;
        }

        set
        {
            //如果有强制锁定方向，则不再操作怪物转向
            if (isForceDirection) return;

            base.direction = value;
        }
    }


    public void SetHealthBarVisible(bool visible)
    {
        hpVisiableCount += visible ? 1 : -1;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        m_forceDirection = 0;
    }

    public void UpdateConfig(MonsterInfo i, MonsterAttriubuteInfo.MonsterAttriubute attr, double aggressive = 1.0f, double defensive = 1.0f)
    {
        configID            = i.ID;
        gender              = i.gender;
        gender              = (gender > 2 || gender < 1) ? 1 : gender;
        icon                = i.avatar;
        avatar              = i.montserAvatar;
        avatarBox           = i.avatarBox;
        info                = i.info;
        localScale          = Vector3.one;
        model.localScale    = Vector3.one * (i.scale == 0 ? 1 : i.scale);
        maxHealth           = Mathf.CeilToInt((float)(attr.health * defensive));
        health              = Mathf.CeilToInt((float)(attr.health * defensive));
        attack              = attr.attack * aggressive;
        defense             = Mathf.CeilToInt((float)(attr.defence * defensive));
        critMul             = attr.criticalMul * aggressive;
        firm                = attr.firm * defensive;
        resilience          = attr.resilience * defensive;
        weaponID            = i.montserStateMachineId;
        colliderSize        = i.colliderSize.x;
        hitColliderSize     = i.hitColliderSize.x == 0 ? 0.5 : i.hitColliderSize.x;
        height              = i.hitColliderSize.y == 0 ? 2.0 : i.hitColliderSize.y;
        colliderHeight      = i.colliderSize.y == 0 ? height : i.colliderSize.y;
        colliderOffset      = i.colliderOffset;
        hitColliderOffset   = i.hitColliderOffset;
        standardRunSpeed    = i.moveSpeed * 0.1;

        _SetField(CreatureFields.Level,          0);                //怪物等级通过场景行为去确定,初始化不赋值
        _SetField(CreatureFields.Crit,           attr.critical * aggressive);
        _SetField(CreatureFields.CritMul,        attr.criticalMul * aggressive);
        _SetField(CreatureFields.Firm,           attr.firm * defensive);
        _SetField(CreatureFields.Resilience,     attr.resilience * defensive);
        _SetField(CreatureFields.AttackSpeed,    attr.attackSpeed <= 0 ? 1 : attr.attackSpeed);
        _SetField(CreatureFields.MoveSpeed,      1.0);

        UpdateColliders();
    }

    protected override void UpdateInputActions()
    {

    }

    protected override void OnDestroy()
    {
        FightRecordManager.RecordLog<LogUlong>(p =>
        {
            p.tag = (byte) TagType.MonsterDie;
            p.value = Identify;
        });

        base.OnDestroy();
    }

    protected override void OnEnterState(StateMachineState old, StateMachineState now, float overrideBlendTime)
    {
        base.OnEnterState(old, now, overrideBlendTime);
        animator.speed = (float)localTimeScale;
    }

    public override void UpdateVisibility()
    {
        base.UpdateVisibility();

        gameObject.SafeSetActive(visible);
    }
}
