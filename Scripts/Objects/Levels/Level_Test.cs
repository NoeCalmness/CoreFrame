/****************************************************************************************************
 * Copyright (C) 2017-2017 FengYunChuanShuo
 * 
 * Description
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.0
 * Created:  2017-
 * 
 ***************************************************************************************************/

using UnityEngine;

public class Level_Test : Level_Battle
{
    private Creature m_enermy = null;

    protected override Vector3_ playerStart { get { return new Vector3_(CombatConfig.spvpStart.x, 0, 0); } }
    protected override string prepareEffect { get { return null; } }
    public override int timeLimit { get { return -1; } }

    protected override void OnCreateCreatures()
    {
        base.OnCreateCreatures(); // Create default player

        m_player.regenRagePercent = 0.3f; // Regen 30% rage every second

        moduleAI.AddCreatureToCampDic(m_player);

        var info = modulePlayer.BuildPlayerInfo(modulePlayer.proto);
        info.name = Util.GetString(9208);

        m_enermy = Creature.Create(info, new Vector3_(CombatConfig.spvpStart.y, 0, 0), new Vector3(0, 90, 0), false, "enermy", info.name);
        m_enermy.FaceTo(player);

        var es = Module_Equip.SelectRandomEquipments(m_player.gender, m_player.roleProto);
        m_enermy.avatar = es[0] + "_" + es[1] + "_" + es[2];
        m_enermy.avatarBox = m_player.avatarBox;
        m_enermy.roleProto = m_player.roleProto;
        moduleAI.AddCreatureToCampDic(m_enermy);

        CharacterEquip.ChangeCloth(m_enermy, es);

        m_enermy.maxHealth = (int)(player.attack + player.elementAttack) * 20;
        m_enermy.health    = m_enermy.maxHealth;
        m_enermy.regenHealthPercent = 0.3;

        player.AddEventListener(CreatureEvents.DEAD,        OnCreatureDead);
        m_enermy.AddEventListener(CreatureEvents.DEAD,      OnCreatureDead);
        EventManager.AddEventListener(CreatureEvents.Dying, OnCreatureDying);
    }

    protected override void OnLoadComplete()
    {
        base.OnLoadComplete();
        if (m_player.pet != null)
        {
            moduleAI.StartAI();
            moduleAI.AddPetAI(m_player.pet);
        }

        #if UNITY_EDITOR
        EventManager.AddEventListener("ResetHero", OnResetHero);
        #endif
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        moduleAI.CloseAI();
    }

    private void OnCreatureDead(Event_ e)
    {
        m_pauseTimer = true;

        EnterState(BattleStates.SlowMotion);
    }

    private void OnCreatureDying(Event_ e)
    {
        var c = e.sender as Creature;
        if (c) c.health = c.maxHealth;  // Prevent creature dead
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
                EnterState(player.realDead ? BattleStates.Losing : BattleStates.Winning);
            }
        }
        else ObjectManager.timeScale = EvaluateSlowMotionCurve(t);
    }

    #region Editor helper

#if UNITY_EDITOR
    //protected override List<string> OnBuildPreloadAssets(List<string> assets = null)
    //{
    //    assets = base.OnBuildPreloadAssets(assets);

    //    var weapons = ConfigManager.GetAll<WeaponInfo>();
    //    foreach (var weapon in weapons)
    //    {
    //        if (weapon.ID < Creature.MAX_WEAPON_ID && weapon.ID > 0)
    //            Module_Battle.BuildWeaponPreloadAssets(modulePlayer.gender, weapon.ID, -1, -1, -1, assets);
    //    }

    //    return assets;
    //}

    private void OnResetHero()
    {
        if (m_player) m_player.position_ = new Vector3_(CombatConfig.spvpStart.x, 0, 0);
        if (m_enermy) m_enermy.position_ = new Vector3_(CombatConfig.spvpStart.y, 0, 0);

        if (m_player && m_enermy)
        {
            m_player.FaceTo(m_enermy);
            m_enermy.FaceTo(m_player);
        }
    }
#endif

    #endregion
}
