using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class RobotCreature : Creature
{
    public static RobotCreature CreateRobot(int configID, Vector3 pos, Vector3 rot,int weaponItemId = -1,int offWeaponItemId = -1, string name = "", string uiName = "")
    {
        var info = ConfigManager.Get<CreatureInfo>(configID);
        if (info == null)
        {
            Logger.LogError("Could not find creature config [{0}]", configID);
            return null;
        }

        // Protect invalid weapon config
        if (info.weaponID < 1)
        {
            info.weaponID = 1;
            info.weaponItemID = 1101;
        }

        if (weaponItemId != -1)
        {
            info.weaponItemID = weaponItemId;
            PropItemInfo item = ConfigManager.Get<PropItemInfo>(weaponItemId);
            if (item) info.weaponID = item.subType;
        }
        if (offWeaponItemId != -1)
        {
            info.offWeaponItemID = offWeaponItemId;
            PropItemInfo item = ConfigManager.Get<PropItemInfo>(offWeaponItemId);
            if (item) info.offWeaponID = item.subType;
        }

        return Create(info,pos,rot,name,uiName);
    }

    public static RobotCreature Create(CreatureInfo info, Vector3_ pos, Vector3 rot, string name = "", string uiName = "")
    {
        if (info == null)
        {
            Logger.LogError("RobotCreature::Create: Create robot failed, invalid config");
            return null;
        }

        var rootNode = new GameObject().transform;
        if (!CreateMorphNodes(info, rootNode))
        {
            Logger.LogError("RobotCreature::Create: Create robot [{0}:{1}] failed, main model [{2}] not loaded", info.ID, info.name, CreatureInfo.GetMorphModelName(info.models, 0));
            return null;
        }

        rootNode.position    = pos;
        rootNode.eulerAngles = rot;

        // Protect invalid weapon config
        if (info.weaponID < 1)
        {
            info.weaponID = 1;
            info.weaponItemID = 1101;
        }

        var c = Create<RobotCreature>(string.IsNullOrEmpty(name) ? info.name : name, rootNode.gameObject);

        c.InitPosition(pos);
        c.isPlayer      = false;
        c.isMonster     = false;
        c.isRobot       = true;
        c.isCombat      = true;
        c.creatureCamp  = CreatureCamp.MonsterCamp; // 机器人默认必须是怪物的阵营
        c.uiName        = string.IsNullOrEmpty(uiName) ? c.name : uiName;
        
        c.isDead        = false;
        c.realDead      = false;
        c.useSpringBone = true;
        c.teamIndex     = MonsterCreature.GetMonsterRoomIndex();

        c.UpdateConfig(info);
        c.OnCreate(info.buffs);

        return c;
    }

    protected override void UpdateInputActions()
    {

    }

    /// <summary>
    /// 当服务器没返回机器人属性时，默认从怪物配置表中读取出跟玩家等级相符合的默认属性
    /// </summary>
    /// <param name="playerLevel"></param>
    public void ResetRobotAttribute(int playerLevel)
    {
        Logger.LogDetail("{0} 's attribute change to id: {1} -level: {2} from config_monster_Attribute",gameObject.name, ROBOT_ATTRIBUTE_ID,playerLevel);

        MonsterAttriubuteInfo attribute = ConfigManager.Get<MonsterAttriubuteInfo>(ROBOT_ATTRIBUTE_ID);
        for (int i = 0, length = attribute.monsterAttriubutes.Length; i < length; i++)
        {
            if (level == attribute.monsterAttriubutes[i].level)
            {
                UpdateAttribute(playerLevel,attribute.monsterAttriubutes[i]);
                break;
            }
        }
    }

    private void UpdateAttribute(int level,MonsterAttriubuteInfo.MonsterAttriubute attr)
    {
        maxHealth = attr.health;
        health = attr.health;
        attack = attr.attack;
        defense = attr.defence;
        critMul = attr.criticalMul;
        firm = attr.firm;
        resilience = attr.resilience;
        
        _SetField(CreatureFields.Level, level);                //怪物等级通过场景行为去确定,初始化不赋值
        _SetField(CreatureFields.Crit, attr.critical);
        _SetField(CreatureFields.CritMul, attr.criticalMul);
        _SetField(CreatureFields.Firm, attr.firm);
        _SetField(CreatureFields.Resilience, attr.resilience);
    }

    public void ResetRobotAttribute(PRoleAttr roleAttrs,PFashion fashion = null)
    {
        CreatureInfo originalInfo = ConfigManager.Get<CreatureInfo>(configID);
        if(originalInfo == null)
        {
            Logger.LogError("configID = {0} connot be loaded,please check out!!",configID);
            return;
        }

        CreatureInfo info = roleAttrs.ToCreatureInfo(originalInfo.Clone<CreatureInfo>());
        //如果有时装的话，刷新武器数据
        if (fashion != null)
        {
            PropItemInfo item = ConfigManager.Get<PropItemInfo>(fashion.weapon);
            if(item != null)
            {
                info.weaponID = item.subType;
                info.weaponItemID = item.ID;
            }

            item = ConfigManager.Get<PropItemInfo>(fashion.gun);
            if (item != null)
            {
                info.offWeaponID = item.subType;
                info.offWeaponItemID = item.ID;
            }
        }
        UpdateConfig(info);
    }
}
