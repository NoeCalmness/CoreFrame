// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-06-21      13:59
//  * LastModify：2018-07-24      11:30
//  ***************************************************************************************************/
#region

using System.Collections.Generic;
using UnityEngine;

#endregion

public class PetCreature : Creature
{
    #region properties

    public override bool realMoving
    {
        get { return m_realMoving; }
    }

    public override ulong Identify
    {
        get
        {
            return petInfo?.ItemID ?? 0;
        }
    }

    public override int buffLevel
    {
        get
        {
            return petInfo.AdditiveLevel;
        }
    }

    #endregion

    #region static functions

    public static PetCreature Create(Creature parent, PetInfo info, Vector3_ pos, Vector3 rot, bool player = false, string name = "", string uiName = "", bool combat = true, bool useSpringBone = true)
    {
        if (info == null)
        {
            Logger.LogError("PetCreature::Create: Create pet failed, invalid config info");
            return null;
        }

        var petInfo = info.BuildCreatureInfo();
        if (petInfo == null)
        {
            Logger.LogError("PetCreature::Create Create pet [{0}] failed, invalid config info", info.ID);
            return null;
        }

        var rootNode = new GameObject().transform;
        if (!CreateMorphNodes(petInfo, rootNode))
        {
            Logger.LogError("PetCreature::Create: Create pet [{0}:{1}] failed, main model [{2}] not loaded", info.ID, petInfo.name, CreatureInfo.GetMorphModelName(petInfo.models, 0));
            return null;
        }

        rootNode.position    = pos;
        rootNode.eulerAngles = rot;

        var c = Create<PetCreature>(string.IsNullOrEmpty(name) ? petInfo.name : name, rootNode.gameObject);

        c.InitPosition(pos);
        c.petInfo        = info;
        c.ParentCreature = parent;
        c.isPlayer       = player;
        c.isMonster      = false;
        c.isCombat       = combat;
        c.isRobot        = false;
        c.creatureCamp   = parent ? parent.creatureCamp : CreatureCamp.PlayerCamp;
        c.uiName         = string.IsNullOrEmpty(uiName) ? c.name : uiName;
        c.isDead         = false;
        c.realDead       = false;
        c.useSpringBone  = useSpringBone;

        c.UpdateConfig(petInfo);

        c.behaviour.UpdateAllColliderState(false);
        c.behaviour.attackCollider.enabled = true;
        c.teamIndex = MonsterCreature.GetMonsterRoomIndex();

        c.Buffs = info.GetBuff(info.AdditiveLevel);
        c.OnCreate(info.GetInitBuff());


        c.avatar = info.UpGradeInfo.icon;
        c.skills.Clear();
        var skill = info.GetSkill();
        if (skill != null)
        {
            c.skills.Add(skill.state, PetSkillData.Create(skill));
        }
        return c;
    }

    #endregion

    #region Fields

    private readonly Dictionary<string, PetSkillData> skills = new Dictionary<string, PetSkillData>();
    public BuffInfo[]       Buffs;

    public bool             m_realMoving;
    public bool             disableSkill;

    /// <summary>
    /// 召唤宠物的目标角色
    /// </summary>
    public Creature         ParentCreature;

    public PetInfo          petInfo;

    public Vector2_         followEdge;

    #endregion

    #region protected

    protected override void OnInitialized()
    {
        base.OnInitialized();
        disableSkill = false;
    }

    protected override void UpdateInputActions()
    {

    }

    protected override void OnEnterState(StateMachineState old, StateMachineState now, float overrideBlendTime)
    {
        base.OnEnterState(old, now, overrideBlendTime);

        if (skills.ContainsKey(now.name))
            skills[now.name].OnUseSkill();
    }

    #endregion

    #region public functions

    /// <summary>
    /// 禁用技能
    /// </summary>
    /// <param name="flag"></param>
    public void DisableSkills(bool flag)
    {
        disableSkill = flag;
    }

    public bool CanUseSkill(string stateName)
    {
        if (disableSkill) return false;

        var skill = skills.Get(stateName);
        return skill && skill.CanUse;
    }

    public PetSkillData GetSkillByState(string state)
    {
        return skills.Get(state);
    }

    public override void OnUpdate(int diff)
    {
        //更新方向
        if (ParentCreature != null && Level.current)
        {
            if (moduleAI.GetDistance(this, ParentCreature, false, false) > 4)
            {
                if (Level.current != null && Level.current.isNormal)
                {
                    direction = ParentCreature.position.x > position.x
                        ? CreatureDirection.FORWARD
                        : CreatureDirection.BACK;

                    if (direction == CreatureDirection.FORWARD)
                        followEdge = new Vector2_(Level.current.edge.x, ParentCreature.position.x - 0.5);
                    else
                        followEdge = new Vector2_(ParentCreature.position.x + 0.5, Level.current.edge.y);
                }
                else
                {
                    direction = ParentCreature.position_.x > position_.x
                        ? CreatureDirection.FORWARD
                        : CreatureDirection.BACK;

                    if (direction == CreatureDirection.FORWARD)
                        followEdge = new Vector2_(Level.current?.edge.x ?? -1000, ParentCreature.position_.x - 0.5);
                    else
                        followEdge = new Vector2_(ParentCreature.position_.x + 0.5, Level.current?.edge.y ?? 1000);
                }
            }
            else
            {
                if (Level.current != null && Level.current.isNormal)
                    followEdge = new Vector2_(ParentCreature.position.x - 0.5, ParentCreature.position.x + 0.5);
                else
                    followEdge = new Vector2_(ParentCreature.position_.x - 0.5, ParentCreature.position_.x + 0.5);
            }
        }
        var dis = moduleAI.GetDistance(this, ParentCreature, false, false);

        ///距离数值不要随便改。与AI配置有关
        m_realMoving = !m_currentState.isIdle && (dis <= 4 || dis >= 6);

        base.OnUpdate(diff);
    }

    public void ResetSkill(int rCount, int cd)
    {
        foreach (var kv in skills)
        {
            kv.Value.ResetUseCountMax(rCount);
            kv.Value.ResetCD(cd);
        }
    }

    #endregion
}
