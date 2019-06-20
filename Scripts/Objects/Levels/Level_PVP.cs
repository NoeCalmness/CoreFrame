/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-05-17
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class Level_PVP : Level_Battle
{
    protected List<Creature> m_players = new List<Creature>();

    public IReadOnlyList<Creature> players { get { return m_players; } }

    public override int timeLimit { get { return LoadParam.TimeLimit; } }

    private bool m_isEnding = false;

    protected override ILoadParam_PVP LoadParam
    {
        get
        {
            if (FightRecordManager.IsRecovering)
                return m_loadParam ?? (m_loadParam = new LoadParamRecoverPVP(FightRecordManager.gameRecover.GameRecordDataHandle as GameRecordDataPvp));
            return base.LoadParam;
        }
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();

        m_isEnding = false;

        m_players.Clear();

        if (moduleAI.IsStartAI) moduleAI.CloseAI();
        if (modulePVP.connected) modulePVP.Disconnect();
    }

    protected override List<string> OnBuildPreloadAssets(List<string> assets = null)
    {
        if (assets == null) assets = new List<string>();

        var players = LoadParam.players;
        foreach (var player in players) Module_Battle.BuildPlayerPreloadAssets(player, assets);

        return assets;
    }

    protected override bool WaitBeforeLoadingWindowClose()
    {
        if (modulePVP.loading) return false;

        return true;
    }

    protected override void OnLoadComplete()
    {
        base.OnLoadComplete();
        if (modulePVP.ended)
        {
            m_isEnding = true;
            if (!IsState(BattleStates.SlowMotion) && !loading)  // Wait slowmotion state or loading end
                EnterState(modulePVP.isWinner ? BattleStates.Winning : BattleStates.Losing);
        }
    }

    protected override bool WaitEndState()
    {
        if (!modulePVP.isInvation)
        {
            if (modulePVP.reward != null)
            {
                moduleGlobal.UnLockUI();

                return true;
            }

            moduleGlobal.LockUI("等待结算信息...");
            return false;
        }
        FightRecordManager.Set(PacketObject.Create<ScRoomReward>());
        return true;
    }

    protected override void OnLoadingWindowClose()
    {
        if (modulePVP.ended)
            EnterState(modulePVP.isWinner ? BattleStates.Winning : BattleStates.Losing); // Battle ended before loading complete
    }

    protected override void SyncCheck()
    {
        if (!modulePVP.connected)
            return;

        if (moduleMatch.isMatchRobot)
            return;

        if (FightRecordManager.Frame > 0 && FightRecordManager.Frame % GeneralConfigInfo.defaultConfig.syncCheckInterval == 0)
        {
            var p = PacketObject.Create<CsSyncInfo>();
            p.frame = FightRecordManager.Frame;
            p.hp = GetHpForVerification();
            modulePVP.Send(p);
        }
    }

    public override double[] GetHpForVerification()
    {
        double[] hp = new double[m_players.Count];
        for (var i = 0; i < m_players.Count; ++i)
        {
            var c = m_players[i];
            hp[i] = c.health;
        }
        return hp;
    }

    protected override void OnCreateCreatures()
    {
        m_isEnding = false;

        EventManager.AddEventListener(CreatureEvents.DEAD, OnCreatureDead);

        if (!moduleAI.IsStartAI) moduleAI.StartAI();

        var players = LoadParam.players;

        for (var i = 0; i < players.Length; ++i)
        {
            var pi = players[i];
            var info = modulePlayer.BuildPlayerInfo(pi);
            var isPlayer = pi.roleId == LoadParam.MasterId;
            var pos = new Vector3_(i == 0 ? CombatConfig.spvpStart.x : CombatConfig.spvpStart.y, 0, 0);

            var p = isPlayer || !LoadParam.IsMatchRobot ? Creature.Create(info, pos, new Vector3(0, 90, 0), isPlayer, i + ":" + pi.roleId, pi.roleName) : RobotCreature.Create(info, pos, new Vector3(0, 90, 0), i + ":" + pi.roleId, pi.roleName);

            if (i != 0) p.direction = CreatureDirection.BACK;

            p.roleId = pi.roleId;
            p.roleProto = pi.roleProto;
            p.teamIndex = i;
            p.avatar = pi.avatar;
            p.enableUpdate = false;

            if (isPlayer) m_player = p;

            m_players.Add(p);

            if (LoadParam.IsMatchRobot)
            {
                if (!isPlayer)
                    moduleAI.AddPlayerAI(p);
                moduleAI.AddCreatureToCampDic(p);
            }

            CharacterEquip.ChangeCloth(p, pi.fashion);

            if (pi.pet != null && pi.pet.itemTypeId != 0)
            {
                var show = ConfigManager.Get<ShowCreatureInfo>(pi.pet.itemTypeId);
                if (show == null)
                {
                    Logger.LogError("没有配置config_showCreatureInfo表。宠物ID = {0}, 没有出生位置信息。宠物模型创建失败", pi.pet.itemTypeId);
                    continue;
                }

                var showData = show.GetDataByIndex(0);
                var data = showData.data.GetValue<ShowCreatureInfo.SizeAndPos>(0);
                p.pet = PetCreature.Create(p, PetInfo.Create(pi.pet), p.position_ + data.pos, p.eulerAngles, false, Module_Home.PET_OBJECT_NAME);
                p.pet.enableUpdate = false;

                if (!moduleAI.IsStartAI) moduleAI.StartAI();
                moduleAI.AddPetAI(p.pet);
            }
        }

        combatCamera.enabled = false;
        combatCamera.LookTo(new Vector3((float)((CombatConfig.spvpStart.x + CombatConfig.spvpStart.y) * 0.5), 0, 0));
    }

    private void OnCreatureDead(Event_ e)
    {
        moduleActive.SendMaxCombo();

        foreach (var p in m_players)
            if (!p.isDead) p.invincibleCount += 1;   // Make winner invincible

        SendBattleEnd();

        m_pauseTimer = true;

        EnterState(BattleStates.SlowMotion);
    }

    protected override void OnEnterState(int oldMask, BattleStates state)
    {
        base.OnEnterState(oldMask, state);

        moduleAI.SetAllAIPauseState(!IsState(BattleStates.Fighting));
    }

    protected override void OnBattleTimerEnd()
    {
        moduleActive.SendMaxCombo();
        SendBattleEnd();
    }

    protected override void OnPrepareStateUpdate(int diff)
    {
        var t = GetStateTimer(BattleStates.Prepare);
        if (t > CombatConfig.sprepareDuration * 0.8)
        {
            if (combatCamera && !combatCamera.enabled)
            {
                combatCamera.OverrideSmooth(0.3f);
                combatCamera.enabled = true;
            }
        }
        if (t >= CombatConfig.sprepareDuration)
        {
            combatCamera?.OverrideSmooth(-1);
            EnterState(BattleStates.Fighting);
        }
    }

    protected override void OnSlowMotionStateUpdate(int diff)
    {
        var t = GetStateTimer(BattleStates.SlowMotion);
        if (t > CombatConfig.sslowMotionDuration)
        {
            if (ObjectManager.timeScale != 1.0) ObjectManager.timeScale = 1.0;

            if (m_players.Find(c => c.realDead))
            {
                QuitState(BattleStates.SlowMotion);
                EnterState(modulePVP.isWinner ? BattleStates.Winning : BattleStates.Losing);
            }
        }
        else ObjectManager.timeScale = EvaluateSlowMotionCurve(t);
    }

    private void SendBattleEnd()
    {
        m_isEnding = true;

        var hp = new double[m_players.Count];
        var hpp = new double[m_players.Count];

        for (var i = 0; i < m_players.Count; ++i)
        {
            var c = m_players[i];
            hp[i] = c.health;
            hpp[i] = c.healthRateL;
        }

        var p = PacketObject.Create<CsRoomOver>();
        p.hp = hp;
        p.hpPer = hpp;

        modulePVP.Send(p);
    }

    void _ME(ModuleEvent<Module_PVP> e)
    {
        if (e.moduleEvent == Module_PVP.EventLoadingComplete)
        {
            foreach (var p in m_players)
            {
                p.enableUpdate = true;
                if (p.pet != null)
                    p.pet.enableUpdate = true;
            }
            EnterState(BattleStates.Prepare);
        }

        if (e.moduleEvent == Module_PVP.EventRoomOver)
        {
            m_isEnding = true;
            if (!IsState(BattleStates.SlowMotion) && !loading)  // Wait slowmotion state or loading end
                EnterState(modulePVP.isWinner ? BattleStates.Winning : BattleStates.Losing);
        }

        if (session.connected && e.moduleEvent == Module_PVP.EventRoomLostConnection && !m_isEnding && !modulePVP.useGameSession) // Do not handle disconnect event if use gamesession
            Game.GoHome();
    }
}
