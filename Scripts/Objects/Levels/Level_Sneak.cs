/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-15
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class Level_Sneak : Level_Battle
{
    private Creature m_enermy = null;
    private Dictionary<int, PropToBuffInfo> propToBuffDic = new Dictionary<int, PropToBuffInfo>();

    #region 流程

    protected override void OnDestroy()
    {
        base.OnDestroy();
        moduleAI.CloseAI();
    }

    protected override List<string> OnBuildPreloadAssets(List<string> assets = null)
    {
        base.OnBuildPreloadAssets(assets);

        PropItemInfo weaponInfo = ConfigManager.Get<PropItemInfo>(moduleLabyrinth.sneakPlayerDetail.fashion.weapon);
        LoadStateMachineEffect(assets, weaponInfo.subType, weaponInfo.ID, 0, 1);  // @TODO: 尽管现在性别绑定到职业了，但逻辑上必须使用性别
        LoadAllBuffAsset(assets);

        // enermy model
        var model = ConfigManager.Get<CreatureInfo>(moduleLabyrinth.lastSneakPlayer.GetProto());
        if (model) assets.AddRange(model.models);
        //添加装备和时装
        assets.AddRange(CharacterEquip.GetEquipAssets(moduleLabyrinth.sneakPlayerDetail.fashion));
        assets.AddRange(CharacterEquip.GetAnimatorAssets(moduleLabyrinth.sneakPlayerDetail.fashion));

        var pet = moduleLabyrinth.lastSneakPlayer.pet;
        if (pet != null && pet.itemId > 0)
        {
            var petInfo = PetInfo.Create(pet);
            if(petInfo != null) Module_Battle.BuildPetPreloadAssets(assets, petInfo);
        }

        assets.AddRange(FillLabyrithBuffAsset());
        return assets;
    }

    public List<string> FillLabyrithBuffAsset()
    {
        List<string> list = new List<string>();
        short[] props = moduleLabyrinth.sneakPlayerDetail.propIds;
        if (props == null || props.Length == 0) return list;

        List<PropToBuffInfo> configs = ConfigManager.GetAll<PropToBuffInfo>();
        foreach (var item in configs)
        {
            propToBuffDic.Add(item.propId,item);
        }

        for (int i = 0; i < props.Length; i++)
        {
            PropToBuffInfo info = propToBuffDic.Get(props[i]);
            if (!info) continue;

            if (info.buffId > 0)
            {
                BuffInfo buff = ConfigManager.Get<BuffInfo>(info.buffId);
                if (buff) list.AddRange(buff.GetAllAssets());
            }

            if (!string.IsNullOrEmpty(info.effect)) list.Add(info.effect);
        }

        return list;
    }

    protected override void OnCreateCreatures()
    {
        base.OnCreateCreatures();
        EventManager.AddEventListener(CreatureEvents.DEAD, OnCreatureDead);

        //进入迷宫的时候要还原血量
        if (modulePVE.reopenPanelType == PVEReOpenPanel.Labyrinth)
        {
            int damage = (int)(player.maxHealth * (1.0f - moduleLabyrinth.labyrinthSelfInfo.healthRate / 100f));
            player.TakeDamage(null, DamageInfo.CreatePreCalculate(damage));
            player.rage = moduleLabyrinth.labyrinthSelfInfo.angerRate;

            //Logger.LogDetail("进入关卡赋值血量和怒气health = {0},anger = {1}", player.healthRate, player.rageRate);
        }

        moduleAI.StartAI();

        if (m_player.pet != null)
        {
            moduleAI.AddPetAI(m_player.pet);
        }

        //创建机器人
        int enermyCreatureId = moduleLabyrinth.lastSneakPlayer.GetProto();
        PFashion enermyFashion = moduleLabyrinth.sneakPlayerDetail.fashion;
        m_enermy = RobotCreature.CreateRobot(enermyCreatureId, Vector3.zero + Vector3.right * 5, new Vector3(0f, 90f, 0f), enermyFashion.weapon, enermyFashion.gun, "hero2", moduleLabyrinth.lastSneakPlayer.roleName);
        m_enermy.gameObject.name = "enermy_robot";
        m_enermy.direction = CreatureDirection.BACK;
        m_enermy.avatar = moduleLabyrinth.lastSneakPlayer.avatar;
        m_enermy.roleProto = enermyCreatureId;

        CharacterEquip.ChangeCloth(m_enermy, moduleLabyrinth.sneakPlayerDetail.fashion);
        //重置属性
        ((RobotCreature)m_enermy).ResetRobotAttribute(moduleLabyrinth.sneakPlayerDetail.roleAttrs, enermyFashion);

        //创建宠物
        PItem pet = moduleLabyrinth.lastSneakPlayer.pet;
        if (pet != null && pet.itemTypeId != 0)
        {
            var show = ConfigManager.Get<ShowCreatureInfo>(pet.itemTypeId);
            if (show == null)
            {
                Logger.LogError("没有配置config_showCreatureInfo表。宠物ID = {0}, 没有出生位置信息。宠物模型创建失败", pet.itemTypeId);
            }

            var showData = show.GetDataByIndex(0);
            var data = showData.data.GetValue<ShowCreatureInfo.SizeAndPos>(0);
            m_enermy.pet = PetCreature.Create(m_enermy, PetInfo.Create(pet), m_enermy.position_ + data.pos, m_enermy.eulerAngles, false, Module_Home.PET_OBJECT_NAME);
            
            moduleAI.AddPetAI(m_enermy.pet);
        }

        //设置被偷袭玩家BUFF 
        if (moduleLabyrinth.sneakPlayerDetail.buffs != null)
        {
            ushort[] buffIds = moduleLabyrinth.sneakPlayerDetail.buffs;
            for (int i = 0, length = buffIds.Length; i < length; i++)
            {
                Buff.Create(buffIds[i], m_enermy);
            }
        }
        moduleAI.AddCreatureToCampDic(m_enermy);
        moduleAI.AddCreatureToCampDic(player);
        moduleAI.AddPlayerAI(m_enermy);

        combatCamera.enabled = false;
        combatCamera.LookTo(new Vector3((float)((CombatConfig.spvpStart.x + CombatConfig.spvpStart.y) * 0.5), 0, 0));

        //set initial health and rage
        modulePVE.SetPveGameData(EnumPVEDataType.Health, m_player.healthRate);
        modulePVE.SetPveGameData(EnumPVEDataType.Rage, m_player.rageRate);
    }

    protected override void OnLoadComplete()
    {
        base.OnLoadComplete();
        RefreshAllBuff();
    }

    private void RefreshAllBuff()
    {
        short[] props = moduleLabyrinth.sneakPlayerDetail.propIds;
        if (props == null || props.Length == 0) return;

        for (int i = 0; i < props.Length; i++)
        {
            PropToBuffInfo info = propToBuffDic.Get(props[i]);
            if (!info) continue;

            if (info.buffId > 0)
            {
                BuffInfo buff = ConfigManager.Get<BuffInfo>(info.buffId);
                if (buff) Buff.Create(buff, player);
            }

            if (!string.IsNullOrEmpty(info.effect))
            {
                StateMachineInfo.Effect e = new StateMachineInfo.Effect();
                e.effect = info.effect;
                e.follow = true;
                if (player && player.behaviour) player.behaviour.effects.PlayEffect(e);
            }
        }
    }

    private void OnCreatureDead(Event_ e)
    {
        if (player.isDead) m_enermy.invincibleCount += 1;
        else player.invincibleCount += 1;

        SendSneakEnd();
        m_pauseTimer = true;
        EnterState(BattleStates.SlowMotion);
    }

    protected override void OnEnterState(int oldMask, BattleStates state)
    {
        base.OnEnterState(oldMask,state);
        moduleAI.SetAllAIPauseState(!IsState(BattleStates.Fighting));
        if(state == BattleStates.Winning || state == BattleStates.Losing)
        {
            modulePVEEvent.pauseCountDown = true;
            moduleAI.SetAllAIPauseState(true);
        }
    }

    protected override void OnBattleTimerEnd()
    {
        moduleLabyrinth.SendLabyrinthSneakOver(PVEOverState.GameOver);
    }

    protected override void OnPrepareStateUpdate(int diff)
    {
        var t = GetStateTimer(BattleStates.Prepare);
        if (!combatCamera.enabled && t > CombatConfig.sprepareDuration * 0.8)
        {
            combatCamera.OverrideSmooth(0.3f);
            combatCamera.enabled = true;
        }
        if (t >= CombatConfig.sprepareDuration)
        {
            combatCamera.OverrideSmooth(-1);
            EnterState(BattleStates.Fighting);
        }
    }

    protected override void OnSlowMotionStateUpdate(int diff)
    {
        var t = GetStateTimer(BattleStates.SlowMotion);
        if (t > CombatConfig.sslowMotionDuration)
        {
            if (ObjectManager.timeScale != 1.0) ObjectManager.timeScale = 1.0;

            if (player.realDead || m_enermy.realDead)
            {
                QuitState(BattleStates.SlowMotion);
                EnterState(moduleLabyrinth.isSneakSuccess ? BattleStates.Winning : BattleStates.Losing);
            }
        }
        else ObjectManager.timeScale = EvaluateSlowMotionCurve(t);
    }

    private void SendSneakEnd()
    {
        //判断是否是玩家死亡
        PVEOverState overState = player.health <= 0 ? PVEOverState.GameOver : PVEOverState.Success;
        moduleLabyrinth.SendLabyrinthSneakOver(overState);
    }

    #endregion

    #region 注册事件


    private void _ME(ModuleEvent<Module_Labyrinth> e)
    {
        if(e.moduleEvent == Module_Labyrinth.EventSneakOver)
        {
            // Wait slowmotion state end
            if (!IsState(BattleStates.SlowMotion)) EnterState(moduleLabyrinth.isSneakSuccess ? BattleStates.Winning : BattleStates.Losing);
        }
    }

    #endregion
    
    #region 加载buff资源
    private void LoadAllBuffAsset(List<string> assets)
    {
        FillPlayerBuff(assets);
        FillEnermyBuffEffect(assets);
    }

    private void FillPlayerBuff(List<string> assets)
    {
        //加载玩家buff
        if (modulePlayer.buffs != null)
        {
            for (int i = 0; i < modulePlayer.buffs.Length; i++)
            {
                BuffInfo buff = ConfigManager.Get<BuffInfo>(modulePlayer.buffs[i]);
                if (buff) buff.GetAllAssets(assets);
            }
        }
        
    }

    private void FillEnermyBuffEffect(List<string> assets)
    {
        //加载被偷袭玩家buff
        if (moduleLabyrinth.sneakPlayerDetail.buffs != null)
        {
            for (int i = 0; i < moduleLabyrinth.sneakPlayerDetail.buffs.Length; i++)
            {
                BuffInfo buff = ConfigManager.Get<BuffInfo>(moduleLabyrinth.sneakPlayerDetail.buffs[i]);
                if (buff) buff.GetAllAssets(assets);
            }
        }
    }
    #endregion

    #region 加载状态机特效资源

    private void LoadStateMachineEffect(List<string> assets, int weaponId, int weaponItemID, int proto, int gender)
    {
        StateMachineInfo.GetAllAssets(weaponId, -1, assets, proto, gender, weaponItemID);
    }

    #endregion
}
