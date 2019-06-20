/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-08-18
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using FrameData = Module_Battle.FrameData;

public class Level_PveTeam : Level_PVE
{
    public override bool canPause { get { return !modulePVE.isTeamMode ; } }
    public override EnumPveLevelType pveLevelType { get { return EnumPveLevelType.TeamLevel; } }
    public override Creature eventPlayer { get { return m_teamLeader; } }
    public bool selfIsleader { get { return player && player == m_teamLeader; } }

    protected override ILoadParam_Team LoadParamTeam
    {
        get
        {
            if (FightRecordManager.IsRecovering)
                return m_loadParamTeam ?? (m_loadParamTeam = new LoadParamRecoverTeam(FightRecordManager.gameRecover.GameRecordDataHandle));
            return base.LoadParamTeam;
        }
    }

    private bool m_isEnding = false;
    private List<Creature> m_teamMembers = new List<Creature>();

    /// <summary>
    /// 队长
    /// </summary>
    private Creature m_teamLeader;

    /// <summary>
    /// 普通成员
    /// </summary>
    private Creature m_normalMember;

    /// <summary>
    /// 所有的玩家都切换场景完成
    /// </summary>
    private bool m_allMemberTransportOver = false;
    private STransportSceneBehaviour m_tempTransportSceneBehaviour;
    private List<Creature> m_moveOverCreature = new List<Creature>();

    protected override bool NeedAutoGotoTransport
    {
        get
        {
            return base.NeedAutoGotoTransport;
        }

        set
        {
            base.NeedAutoGotoTransport = value;
            if (value)
            {
                m_moveOverCreature.Clear();
            }
        }
    }

    protected override void SyncCheck()
    {
        if (!moduleTeam.connected)
            return;

        if (modulePVE.isStandalonePVE)
            return;

        if (FightRecordManager.Frame > 0 && FightRecordManager.Frame % GeneralConfigInfo.defaultConfig.syncCheckInterval == 0)
        {
            var p = PacketObject.Create<CsSyncInfo>();
            p.frame = FightRecordManager.Frame;
            p.hp = GetHpForVerification();
            moduleTeam.Send(p);
        }
    }

    public override double[] GetHpForVerification()
    {
        return new double[] { m_teamLeader?.health ?? 0, m_normalMember?.health ?? 0};
    }

    #region overide level functions
    protected override List<string> OnBuildPreloadAssets(List<string> assets = null)
    {
        base.OnBuildPreloadAssets(assets);

        if (assets == null) assets = new List<string>();
        var member = moduleTeam.members;
        if(member != null)
        {
            foreach (var m in member)
            {
                if (m.roleId == modulePlayer.id_) continue;
                Module_Battle.BuildPlayerPreloadAssets(m, assets);
            }
        }

        return assets;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        m_isEnding = false;
        m_teamMembers.Clear();

        m_teamLeader = null;
        m_normalMember = null;
        m_tempTransportSceneBehaviour = null;
        m_allMemberTransportOver = false;
        m_moveOverCreature.Clear();
    }

    public override void OnUpdate(int diff)
    {
        base.OnUpdate(diff);
        AutoTeamStopTransport();
    }

    protected override void AutoMoveToTransport()
    {
        if (InvalidAutoMoveToTransport()) return;

        for (int i = 0; i < m_transportColliders.Count; i++)
        {
            if (m_transportColliders[i].state == PVETriggerCollider.EnumTriggerState.Close) continue;
            
            var c = m_transportColliders[i];
            var l = c.position.x - c.size.x * 0.5;
            var r = c.position.x + c.size.x * 0.5;

            foreach (var item in m_teamMembers)
            {
                if (!item || item.health <= 0 || m_moveOverCreature.Contains(item)) continue;

                if (item.position_.x <= l)
                {
                    moduleAI.RemovePlayerAI(item);
                    item.moveState = 1;
                    m_moveOverCreature.Add(item);
                }
                else if (item.position_.x >= r)
                {
                    moduleAI.RemovePlayerAI(item);
                    item.moveState = -1;
                    m_moveOverCreature.Add(item);
                }
            }
        }
    }

    private void AutoTeamStopTransport()
    {
        if (InvalidAutoMoveToTransport()) return;

        for (int i = 0; i < m_transportColliders.Count; i++)
        {
            if (m_transportColliders[i].state == PVETriggerCollider.EnumTriggerState.Close) continue;

            var c = m_transportColliders[i];
            var l = c.position.x - c.size.x * 0.5 - 0.2;
            var r = c.position.x + c.size.x * 0.5 + 0.2;

            foreach (var item in m_teamMembers)
            {
                if (!item || item.health <= 0 || item.moveState == 0 || !m_moveOverCreature.Contains(item)) continue;
                
                if (item.position_.x > l && item.position_.x < r)
                {
                    item.moveState = 0;
                    item.stateMachine?.TranslateToID(StateMachineState.STATE_IDLE);
                    moduleAI.RemovePlayerAI(item);
                    m_moveOverCreature.Remove(item);
                }
            }
        }

        var allStop = true;
        foreach (var item in m_teamMembers)
        {
            if (item.moveState != 0)
            {
                allStop = false;
                break;
            }
        }
        NeedAutoGotoTransport = !allStop; 
    }

    protected override bool InvalidAutoMoveToTransport()
    {
        var invalid = !NeedAutoGotoTransport || m_transportColliders == null || m_transportColliders.Count == 0;
        if (modulePVE.useTeamAutoBattle) return invalid;
        else return modulePVE.isTeamMode || invalid;
    }

    protected override void OnCreateCreatures()
    {
        m_isEnding = false;

        if (modulePVE.isStandalonePVE) CreateStandalonePlayer();
        else CreateTeamPlayer();

        if(m_teamMembers.Count > 1) m_teamMembers.Sort((a, b) => a.roleId < b.roleId ? -1 : 1);
        
        //ChangePlayerAtt();
    }

    private void ChangePlayerAtt()
    {
        if(modulePVE.stageId != 1)
        {
            foreach (var mem in m_teamMembers)
            {
                mem.attack *= 100;
                mem.maxHealth *= 100;
                mem.health = mem.maxHealth;
                mem.maxEnergy = 100;
                mem.energy = mem.maxEnergy;
            }
        }
    }

    private void CreateStandalonePlayer()
    {
        base.OnCreateCreatures();
        m_teamMembers.Add(m_player);
        m_teamLeader = m_player;

        //需要设置组队单人的teamIndex
        if(modulePVE.isTeamMode && moduleTeam.members != null)
        {
            for (int i = 0; i < moduleTeam.members.Length; i++)
            {
                if (moduleTeam.members[i].roleId == modulePlayer.id_)
                {
                    m_player.teamIndex = i;
                    moduleBattle.SetPlayerTeamIndex(i);
                    break;
                }
            }
        }
    }

    private void CreateTeamPlayer()
    {
        var members = moduleTeam.members;
        if (members != null)
        {
            for (var i = 0; i < members.Length; ++i)
            {
                var pi = members[i];
                var info = modulePlayer.BuildPlayerInfo(pi);
                var isPlayer = pi.roleId == LoadParamTeam.MasterId;
                var pos = new Vector3_(playerStart.x - i, 0, 0);

                var p = Creature.Create(info, pos, new Vector3(0, 90, 0), isPlayer, i + ":" + pi.roleId, pi.roleName);

                p.roleId = pi.roleId;
                p.roleProto = pi.roleProto;
                p.teamIndex = i;
                p.avatar = pi.avatar;
                p.enableUpdate = false;
                p.SetCreatureCamp(CreatureCamp.PlayerCamp);

                if (isPlayer)
                {
                    m_player = p;
                    moduleBattle.SetPlayerTeamIndex(p.teamIndex);
                }

                if (pi.teamLeader > 0) m_teamLeader = p;
                else m_normalMember = p;

                m_teamMembers.Add(p);

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
                }
            }
        }
    }

    protected override bool WaitBeforeLoadingWindowClose()
    {
        if (!modulePVE.isTeamMode) return true;  // We are not in team mode

        if (moduleTeam.loading) return false;    // Wait for loading complete
        
        // Loading complete, start logic update!
        foreach (var member in m_teamMembers)
        {
            if(moduleTeam.onlineMembers.FindIndex(o=>o.roleId == member.roleId) < 0)
            {
                if (member == m_teamLeader) m_teamLeader = null;

                //如果被踢出的不是自己，才销毁该对象，如果是自己被踢出，此时可能window_combat还没有打开，会导致界面报错
                //并且如果是自己退出的话，会直接返回到levelhome
                if(!member.isPlayer) member.Destroy();
            }
        }

        if(!m_teamLeader && m_normalMember)
        {
            m_teamLeader = m_normalMember;
            m_normalMember = null;
        }

        if (!m_teamLeader) Logger.LogError("WaitBeforeLoadingWindowClose ... teamleader is null....please check out");

        return true;
    }

    protected override void OnLoadingWindowClose()
    {
        if (modulePVE.isTeamMode && moduleTeam.ended)
        {
            EnterState(moduleTeam.result == 0 ? BattleStates.Winning : BattleStates.Losing);
        }
        if (!modulePVE.isTeamMode) base.OnLoadingWindowClose();
    }

    protected override void OnLoadComplete()
    {
        base.OnLoadComplete();
        AddFrameAction(Module_Battle.FrameAction.StartTransport,    StartTransportScene);
        AddFrameAction(Module_Battle.FrameAction.TransportOver,     OnTransportSceneOver);
        AddFrameAction(Module_Battle.FrameAction.PlayerReborn,      OnMemberReborn);
        AddFrameAction(Module_Battle.FrameAction.MemberQuit,        OnMemberQuitEarlySettlement);
        AddFrameAction(Module_Battle.FrameAction.StoryChangeStep,   OnStoryStepChange);
    }

    protected override void InitUnitData()
    {
        base.InitUnitData();
        m_moveOverCreature.Clear();
    }

    protected override void InitPetAI()
    {
        foreach (var item in m_teamMembers)
        {
            if (item.destroyed || item.health == 0 || item.pet == null) continue;

            moduleAI.AddPetAI(item.pet);
        }
    }

    protected override void HideObjectsWhenCameraAnimation(bool visible)
    {
        base.HideObjectsWhenCameraAnimation(visible);
        foreach (var item in m_teamMembers)
        {
            //玩家如果已经离线，并且此时需要恢复玩家
            if (visible && !moduleTeam.IsOnlineMember(item.roleId)) continue;

            item.model.SafeSetActive(visible);
        }
    }
    #endregion

    #region override scene event

    #region add buff

    protected override void AddBuffToAllPlayer(BuffInfo buff, int duraction)
    {
        foreach (var item in m_teamMembers)
        {
            Buff.Create(buff, item, item, duraction);
        }
    }

    #endregion

    #region transport scene
    protected override void OnTransportScene(STransportSceneBehaviour behaviour)
    {
        //单机直接跳转
        if(modulePVE.isStandalonePVE)
        {
            base.OnTransportScene(behaviour);
            return;
        }

        m_tempTransportSceneBehaviour = behaviour;
        moduleTeam.StartTransportScene();
        moduleBattle.parseInput = false;
        modulePVE.DispathTransportSceneToUI(true);
#if AI_LOG
        Module_AI.LogBattleMsg(player, "[begin to transport scene......behaviour is {0}]",behaviour.ToString());
#endif
    }

    protected void StartTransportScene(FrameData d)
    {
        HandleTransportScene(m_tempTransportSceneBehaviour);
        moduleTeam.HandleStartTransportFrameEvent();
    }

    /// <summary>
    /// 开始组队模式开始转移场景
    /// 必须收到服务器的所有玩家可以转移场景通知后才可调用
    /// </summary>
    /// <param name="behaviour"></param>
    protected override void HandleTransportScene(STransportSceneBehaviour behaviour)
    {
        base.HandleTransportScene(behaviour);
        //foreach (var member in m_teamMembers) member.enableUpdate = false;
    }

    protected override void OnLoadAssetsOverOnTransport()
    {
        base.OnLoadAssetsOverOnTransport();
        if(!modulePVE.isStandalonePVE) moduleGlobal.SetPVELockText(ConfigText.GetDefalutString(8,0));
    }

    protected override bool WaitBeforeChangeToNewScene()
    {
        return m_allMemberTransportOver || modulePVE.isStandalonePVE;
    }

    protected override void OnTransportPlayer()
    {
        if (modulePVE.isStandalonePVE)
            base.OnTransportPlayer();
        else
            m_currentUnitSceneEvent.SetAllPlayers(m_teamMembers);
    }

    protected override void OnLogicPlayerTransport()
    {
        foreach (var member in m_teamMembers)
        {
            moduleAI.AddCreatureToCampDic(member);
            member.enableUpdate = true;
        }

        moduleAI.SortCreatureCamp(CreatureCamp.PlayerCamp);
        AllPlayerAutoBattle();
    }

    private void AllPlayerAutoBattle()
    {
        if (modulePVE.useTeamAutoBattle)
        {
            foreach (var member in m_teamMembers)
            {
                member.useAI = true;
                moduleAI.AddPlayerAI(member);
                moduleAI.ChangeLockEnermy(member, true);
            }
        }
    }

    protected override void OnTransportTweenFinish()
    {
        base.OnTransportTweenFinish();
        
        m_allMemberTransportOver = false;
#if AI_LOG
        Module_AI.LogBattleMsg(player, "[finish to transport scene......]");
#endif
    }
    #endregion

    #region creature
    protected override void OnCreatureDead(Event_ e)
    {
        FightRecordManager.RecordLog<LogOnCreatureDied>(log =>
        {
            log.tag = (byte)TagType.OnCreatureDied;
            log.isTeamMode = modulePVE.isTeamMode;
            log.onlineCount = moduleTeam.onlineMembers?.Count ?? 0;
        });

        if(!modulePVE.isTeamMode) base.OnCreatureDead(e);

        var c = e.sender as Creature;
        if (c.creatureCamp == CreatureCamp.PlayerCamp)
        {
            bool allDead = m_teamMembers.Count > 0;
            foreach (var item in m_teamMembers)
            {
                if (item && !item.isDead) allDead = false;
            }

            //全部死亡之后,设置怪物为无敌状态
            if (allDead)
            {
                SetAllMonsterInvincible(true);
                QuitState(BattleStates.Watch);
            }
        }
    }

    public override bool IsPlayerAllDead()
    {
        bool allDead = m_teamMembers.Count > 0;
        foreach (var item in m_teamMembers)
        {
            if (item && !item.isDead) allDead = false;
        }
        return allDead;
    }

    public override Creature GetWatchTarget(Creature rWatcher)
    {
        foreach (var item in m_teamMembers)
        {
            if (item != rWatcher)
                return item;
        }
        return null;
    }
    #endregion

    protected override void OnStageClear(SStageClearBehaviour behaviour)
    {
        base.OnStageClear(behaviour);
        //如果通关成功的时候玩家还处于死亡状态，则玩家自动复活
        if (m_player && m_player.health == 0) m_player.Revive();
    }
    #endregion

    #region module event
    void _ME(ModuleEvent<Module_Team> e)
    {
        if (!modulePVE.isTeamMode) return;

        switch (e.moduleEvent)
        {
            case Module_Team.EventStart:
            {
                // Loading complete, start logic update!
                foreach (var member in m_teamMembers)
                {
                    member.enableUpdate = true;
                    if (member.pet) member.pet.enableUpdate = true;
                }
                EnterState(BattleStates.Prepare);

                //active pve process
                OnPVELoadingWindowClose();
                break;
            }
            case Module_Team.EventEnd:
                if(modulePVE.lastSendState == PVEOverState.GameOver || modulePVE.lastSendState == PVEOverState.Success)
                {
                    m_isEnding = true;
                    if (!IsState(BattleStates.SlowMotion) && !loading)  // Wait slowmotion state or loading end
                        EnterState(modulePVE.isSendWin ? BattleStates.Winning : BattleStates.Losing);
                }
                break;

            case Module_Team.EventQuit:
                OnMemberQuit(e.param1 as PTeamMemberInfo,(EnumTeamQuitState)e.param2);
                break;

            
        }

        if (session.connected && e.moduleEvent == Module_Team.EventRoomLostConnection && !m_isEnding && !m_isPVEFinish && !moduleTeam.useGameSession) // Do not handle disconnect event if use gamesession
        {
            foreach (var item in m_unitSceneEvents)
            {
                if (item.hasDestoryed) continue;

                if (m_currentUnitSceneEvent == null) m_currentUnitSceneEvent = item;
                //必须要把场景打开，否则会导致objectmanager里面部分资源不会被释放，导致内存泄漏
                item.SetSceneActive(true);
            }
            SetLevelBehaviourCanDestory();
            Game.GoHome();
        }
    }

    private void OnMemberQuit(PTeamMemberInfo member, EnumTeamQuitState reason)
    {
        if (moduleTeam.members == null) return;
        
        var c = m_teamMembers.Find(o => o.roleId == member.roleId);

        //掉线,角色直接死亡
        bool alive = c && c.health > 0;
        if (reason == EnumTeamQuitState.DroppedDuringGame && !isEndingState && !m_isPVEFinish && !m_recvOverMsg && alive) c?.Kill();

        //切换队长
        if (member.teamLeader > 0)
        {
            foreach (var item in moduleTeam.members)
            {
                item.teamLeader = (byte)(item.roleId == member.roleId ? 0 : 1);
            }

            if (c)
            {
                var temp = m_normalMember;
                m_normalMember = c;
                m_teamLeader = temp;
                Logger.LogInfo("OnMemberQuit... team leader change to {0}",m_teamLeader ? m_teamLeader.uiName : "null");
            }
        }
    }

    private void OnMemberQuitEarlySettlement(FrameData d)
    {
        if (moduleTeam.members == null) return;

        var reason = (EnumTeamQuitState)d.parameters.GetValue<int>(0);
        if (reason != EnumTeamQuitState.EarlySettlement) return;

        var member = moduleTeam.members.GetValue<PTeamMemberInfo>(d.parameters.GetValue<int>(1));
        if (member == null) return;

        FightRecordManager.RecordLog<LogTeamQuit>(log =>
        {
            log.tag = (byte)TagType.TeamQuit;
            log.roleId = member.roleId;
            log.reason = (sbyte)reason;
        });

        var c = m_teamMembers.Find(o => o.roleId == member.roleId);
        if (!c) return;

        //掉线,角色直接死亡
        bool alive = c && c.health > 0;
        if (reason == EnumTeamQuitState.EarlySettlement && !isEndingState && !m_isPVEFinish && !m_recvOverMsg && alive) c?.Kill();

        //切换队长
        if (member.teamLeader > 0)
        {
            foreach (var item in moduleTeam.members)
            {
                item.teamLeader = (byte)(item.roleId == member.roleId ? 0 : 1);
            }

            if (c)
            {
                var temp = m_normalMember;
                m_normalMember = c;
                m_teamLeader = temp;
                Logger.LogInfo("OnMemberQuit... team leader change to {0}", m_teamLeader ? m_teamLeader.uiName : "null");
            }
        }
    }

    private void OnTransportSceneOver(FrameData d)
    {
        //组队的时候，一旦开始处理切换场景完毕，就统一开启逻辑帧
        OnLogicTransportFinish();
        m_allMemberTransportOver = true;
        moduleTeam.TransportOver();
#if AI_LOG
        Module_AI.LogBattleMsg(player, "[set m_allMemberTransportOver as true.....]");
#endif
    }

    private void OnMemberReborn(FrameData d)
    {
        if(d.parameters == null || d.parameters.Length == 0)
        {
            Logger.LogError("call frame action [PlayerReborn] ,but the parameters is null,please check out");
            return;
        }

        int guid = d.parameters.GetValue<int>(0);
        if (moduleTeam.members == null)
        {
            Logger.LogError("moduleTeam.members is null,please check out");
            return;
        }

        var memberData =  moduleTeam.members.GetValue<PTeamMemberInfo>(guid);
        if (memberData == null)
        {
            Logger.LogError("Team member [{0}] cannot be finded", guid);
            return;
        }

        OnMemberReborn(memberData);
        moduleTeam.DispatchReborn(memberData);
    }

    private void OnMemberReborn(PTeamMemberInfo memberData)
    {
        if (memberData == null) return;

        foreach (var item in m_teamMembers)
        {
            if(item.roleId == memberData.roleId && item.health == 0)
            {
                item.Revive();
                if (item == player)
                    QuitState(BattleStates.Watch);
                break;
            }
        }

        SetAllMonsterInvincible(false);

        FightRecordManager.RecordLog<LogUlong>(l =>
        {
            l.tag = (byte)TagType.Reborn;
            l.value = memberData.roleId;
        });
    }

    private void OnStoryStepChange(FrameData d)
    {
        int storyId = d.parameters.GetValue<int>(0);
        int storyIndex = d.parameters.GetValue<int>(1);
        EnumContextStep step = (EnumContextStep)d.parameters.GetValue<int>(2);

        BaseStory story = moduleStory.GetCurrentValidStory();
        
        if (!story)
        {
            Logger.LogWarning("there is no active story...please check out data is [id = {0},index = {1} step = {2}]");
            return;
        }

        story.RecvFrameData(storyId,storyIndex,step);
    }
    #endregion
}
