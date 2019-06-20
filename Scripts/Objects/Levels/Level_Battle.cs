/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Basic battle level
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-11-14
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Battle level state
/// </summary>
public enum BattleStates
{
    /// <summary>
    /// Unused, we are in loading state
    /// </summary>
    None      = 0,
    /// <summary>
    /// Waiting for loading window close
    /// </summary>
    Waiting   = 1,
    /// <summary>
    /// Waiting for start
    /// </summary>
    Prepare   = 2,
    /// <summary>
    /// We are fighting!
    /// </summary>
    Fighting   = 3,
    /// <summary>
    /// Cheers!!
    /// </summary>
    Winning    = 4,
    /// <summary>
    /// We lose...
    /// </summary>
    Losing     = 5,
    /// <summary>
    /// Battle ended
    /// </summary>
    Ended      = 6,
    /// <summary>
    /// Battle paused
    /// </summary>
    Paused     = 7,
    /// <summary>
    /// Slow motion state
    /// </summary>
    SlowMotion = 8,
    /// <summary>
    /// watch others
    /// </summary>
    Watch = 9,

    Count,
}

public class Level_Battle : Level
{
    #region Static functions

    public static bool IsState(int mask, BattleStates state)
    {
        return state > 0 && state < BattleStates.Count && mask.BitMask((int)state);
    }

    public static float EvaluateSlowMotionCurve(int now, AnimationCurve curve = null)
    {
        if (now > CombatConfig.sslowMotionDuration) return 1.0f;

        if (curve == null) curve = CombatConfig.sslowMotionCurve;
        return curve.Evaluate((float)now / CombatConfig.sslowMotionDuration);
    }

    private const int Waiting = 1, Prepare = 2, Fighting = 3, Winning = 4, Losing = 5, Ended = 6, Paused = 7, SlowMotion = 8;

    #endregion

    public readonly static string[] BATTLE_UI_ASSET_NAME = new string[] { "damage_number" };

    public delegate void StateUpdateHandler(int diff);

    /// <summary>
    /// Current battle state mask
    /// </summary>
    public int stateMask { get { return m_stateMask; } }

    private BattleStates m_ns = BattleStates.None;

    protected int m_victoryTime = -1;

    protected ILoadParam_PVP m_loadParam;
    protected ILoadParam_Team m_loadParamTeam;

    protected virtual ILoadParam_PVP LoadParam
    {
        get { return m_loadParam ?? (m_loadParam = new LoadParamPVP(moduleMatch)); }
    }
    protected virtual ILoadParam_Team LoadParamTeam
    {
        get { return m_loadParamTeam ?? (m_loadParamTeam = new LoadParamTeam(moduleTeam)); }
    }

    /// <summary>
    /// Level time limit
    /// Unlimit if time less than 1
    /// </summary>
    public virtual int timeLimit { get { return m_timeLimit; } }

    protected virtual string combatWindow { get { return CombatConfig.sdefaultWindow; } }
    protected virtual string endWindow { get { return CombatConfig.sdefaultEndWindow; } }
    protected virtual string ultimateMaskWindow { get { return CombatConfig.sdefaultUltimateMaskWindow; } }
    protected virtual string prepareEffect { get { return CombatConfig.sdefaultPrepareEffect; } }
    protected virtual string heavenMusic { get { return CombatConfig.sdefaultHeavenMusic; } }
    protected virtual string hellMusic { get { return CombatConfig.sdefaultHellMusic; } }
    protected virtual Vector3_ playerStart { get { return new Vector3_(0, 0, 0); } }

    protected Camera_Combat combatCamera { get { return Camera_Combat.current; } }

    /// <summary>
    /// Current battle time
    /// </summary>
    public int battleTime { get { return m_battleTime; } }
    /// <summary>
    /// Current battle time (seconds)
    /// </summary>
    public int battleTimeSec { get { return m_battleTimeSec; } }
    /// <summary>
    /// Total level time
    /// </summary>
    public int totalTime { get { return m_totalTime; } }
    /// <summary>
    /// Battle timer paused ?
    /// </summary>
    public bool pauseTimer { get { return m_pauseTimer; } }

    public Creature player { get { return m_player; } }

    public virtual Creature assistCreature { get { return null; } }

    protected int m_stateMask     = 0;
    protected int m_timeLimit     = 0;
    protected int m_battleTimeSec = 0;
    protected int m_battleTime    = 0;
    protected int m_totalTime     = 0;
    protected bool m_pauseTimer   = false;
    protected Creature m_player   = null;

    private StateUpdateHandler[] m_stateHandlers = new StateUpdateHandler[(int)BattleStates.Count];
    private int[] m_stateTimers = new int[(int)BattleStates.Count];

    /// <summary>
    /// Are we in state ?
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public bool IsState(BattleStates state) { return IsState(stateMask, state); }

    /// <summary>
    /// Get last state timer
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public int GetStateTimer(BattleStates state) { return m_stateTimers[(int)state]; }

    public void EnterState(BattleStates state)
    {
        var mask = ((int)state).ToMask();

        if (mask == stateMask) return;

        var old = stateMask;
        if (state > BattleStates.Ended) m_stateMask |= mask;
        else
        {
            if (m_ns > 0) QuitState(m_ns);
            m_stateMask |= mask;

            m_ns = state;
        }

        if ((old & mask) == 0)
        {
            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            Logger.LogInfo("[{3}], Level [{0}] enter state [{1}] from mask {2}.", name, state, old, levelTime);
            #endif

            m_stateTimers[(int)state] = 0;
            OnEnterState(old, state);

            if (state == BattleStates.Prepare)  DispatchEvent(LevelEvents.PREPARE, Event_.Pop(prepareEffect));
            if (state == BattleStates.Fighting) DispatchEvent(LevelEvents.START_FIGHT);
            if (state == BattleStates.Ended)    DispatchEvent(LevelEvents.BATTLE_END);
            if (state == BattleStates.Watch)
            {
                DispatchEvent(LevelEvents.BATTLE_ENTER_WATCH_STATE, Event_.Pop(true));

                var target = GetWatchTarget(player);
                if (target != null)
                    Camera_Combat.current.SetFollowTransform(target.transform);
            }
        }
    }

    public void QuitState(BattleStates state)
    {
        if (!IsState(state)) return;

        var old = stateMask;
        m_stateMask &= ~((int)state).ToMask();

        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        Logger.LogInfo("[{4}], Level [{0}] quit state [{1}] from mask [{2}], state time: [{3}].", name, state, old, m_stateTimers[(int)state], levelTime);
        #endif

        OnQuitState(old, state);

        if (state == BattleStates.Watch)
        {
            DispatchEvent(LevelEvents.BATTLE_ENTER_WATCH_STATE, Event_.Pop(false));
            Camera_Combat.current.SetFollowTransform(player.transform);
        }
    }

    public void AddStateUpdateCallback(BattleStates state, StateUpdateHandler handler)
    {
        if (state <= 0 || state >= BattleStates.Count || handler == null) return;

        m_stateHandlers[(int)state] -= handler;
        m_stateHandlers[(int)state] += handler;
    }

    protected override List<string> BuildPreloadAssets()
    {
        var assets = base.BuildPreloadAssets();

        assets.Add(combatWindow);       // Add combat window
        assets.Add(endWindow);          // Add end window
        assets.Add(ultimateMaskWindow); // Add ultimate spell mask window
        assets.Add(prepareEffect);      // Add prepare effect
        assets.Add(heavenMusic);        // Add victory music
        assets.Add(hellMusic);          // Add lose music
        assets.AddRange(BATTLE_UI_ASSET_NAME);
        assets.Add(GeneralConfigInfo.ssceneTextObject);

        OnBuildPreloadAssets(assets);

        return assets;
    }

    protected override void OnLoadStart()
    {
        m_timeLimit     = levelInfo && levelInfo.timeLimit != 0 ? levelInfo.timeLimit : CombatConfig.sdefaultTimeLimit;
        m_ns            = BattleStates.None;
        m_stateMask     = 0;
        m_pauseTimer    = false;
        m_battleTime    = 0;
        m_battleTimeSec = timeLimit < 1 ? -1 : timeLimit / 1000;

        m_victoryTime = -1;

        m_player = null;

        for (var i = 0; i < m_stateTimers.Length; ++i) m_stateTimers[i] = 0;

        AddStateUpdateCallback(BattleStates.Prepare,    OnPrepareStateUpdate);
        AddStateUpdateCallback(BattleStates.Fighting,   OnFightingStateUpdate);
        AddStateUpdateCallback(BattleStates.Winning,    OnWinningStateUpdate);
        AddStateUpdateCallback(BattleStates.Losing,     OnLosingStateUpdate);
        AddStateUpdateCallback(BattleStates.Ended,      OnEndedStateUpdate);
        AddStateUpdateCallback(BattleStates.SlowMotion, OnSlowMotionStateUpdate);

        m_stateMask = 0;
    }

    protected override void OnLoadComplete()
    {
        enableUpdate = true;

        OnCreateCreatures();

        CreatePlayerAudioListener();

        EnterState(BattleStates.Waiting);

        Window.ShowAsync(combatWindow);
        Window.ShowAsync(ultimateMaskWindow);
    }

    protected override void OnLoadingWindowClose()
    {
        EnterState(BattleStates.Prepare);
    }

    public override void OnUpdate(int diff)
    {
        m_totalTime += diff;

        if (Game.paused) return;

        SyncCheck();

        for (int i = 1, c = m_stateHandlers.Length; i < c; ++i) // Ignore BattleStates.None
        {
            if (!m_stateMask.BitMask(i)) continue;

            if (i == SlowMotion)
            {
                diff = ObjectManager.deltaTime; // Slow motion timer always use unscaled delta time
                m_stateTimers[i] += diff;
            }
            else if (i == Fighting)
            {
                if (!m_pauseTimer && !m_stateMask.BitMask(Paused)) m_stateTimers[i] += diff;

                m_battleTime = m_stateTimers[i];

                m_stateHandlers[i]?.Invoke(diff);

                if (timeLimit > 0)
                {
                    var tsec = (timeLimit - m_battleTime) / 1000;
                    if (tsec != m_battleTimeSec)
                    {
                        m_battleTimeSec = tsec;
                        if (m_battleTimeSec < 0) m_battleTimeSec = 0;
                        DispatchEvent(LevelEvents.BATTLE_TIMER, Event_.Pop(m_battleTimeSec));

                        if (m_battleTimeSec == 0) OnBattleTimerEnd();
                    }
                }
                else if (m_battleTimeSec != -1)
                {
                    m_battleTimeSec = -1;
                    DispatchEvent(LevelEvents.BATTLE_TIMER, Event_.Pop(m_battleTimeSec));
                }

                continue;
            }
            else m_stateTimers[i] += diff;

            m_stateHandlers[i]?.Invoke(diff);
        }
    }

    protected virtual void SyncCheck()
    {
        
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        m_player = null;

        FightRecordManager.EndRecord(false, false);

        for (var i = 0; i < m_stateHandlers.Length; ++i) m_stateHandlers[i] = null;
    }

    protected virtual List<string> OnBuildPreloadAssets(List<string> assets = null)
    {
        if (assets == null) assets = new List<string>();

        Module_Battle.BuildPlayerPreloadAssets(assets);

        return assets;
    }

    protected virtual void OnCreateCreatures()
    {
        var info = modulePlayer.BuildPlayerInfo();
        m_player = Creature.Create(info, playerStart, new Vector3(0, 90, 0), true, "player", modulePlayer.name_);
        m_player.roleId = modulePlayer.id_;
        m_player.roleProto = modulePlayer.proto;

        CharacterEquip.ChangeCloth(m_player, moduleEquip.currentDressClothes);

        _SetDOFFocusTarget(m_player.transform);

        //创建宠物
        if (modulePet.FightingPetID <= 0) return;
        var pet = modulePet.FightingPet;
        if (pet != null)
        {
            var show = ConfigManager.Get<ShowCreatureInfo>(pet.ID);
            if (show == null)
            {
                Logger.LogError("没有配置config_showCreatureInfo表。宠物ID = {0}, 没有出生位置信息。宠物模型创建失败", pet.ID);
                return;
            }
            var showData = show.GetDataByIndex(0);
            var data = showData.data.GetValue<ShowCreatureInfo.SizeAndPos>(0);
            m_player.pet = PetCreature.Create(m_player, pet, playerStart + data.pos, data.rotation, false, Module_Home.PET_OBJECT_NAME);
            m_player.pet.localScale *= data.size;
            m_player.pet.localEulerAngles = data.rotation;
        }
    }

    protected Creature CreateTeamPlayer(PTeamMemberInfo rInfo, Vector3 pos, int index = 0)
    {
        if (rInfo == null)
            return null;
        var info = modulePlayer.BuildPlayerInfo(rInfo);
        var isPlayer = rInfo.roleId == LoadParamTeam.MasterId;

        var p = Creature.Create(info, pos, new Vector3(0, 90, 0), isPlayer, index + ":" + rInfo.roleId, rInfo.roleName);

        p.roleId = rInfo.roleId;
        p.roleProto = rInfo.roleProto;
        p.teamIndex = index;
        p.avatar = rInfo.avatar;
        p.enableUpdate = false;

        if (isPlayer)
        {
            m_player = p;
            moduleBattle.SetPlayerTeamIndex(p.teamIndex);
        }

        CharacterEquip.ChangeCloth(p, rInfo.fashion);

        if (rInfo.pet == null || rInfo.pet.itemTypeId == 0) return p;
        var show = ConfigManager.Get<ShowCreatureInfo>(rInfo.pet.itemTypeId);
        if (show == null)
        {
            Logger.LogError("没有配置config_showCreatureInfo表。宠物ID = {0}, 没有出生位置信息。宠物模型创建失败", rInfo.pet.itemTypeId);
            return p;
        }

        var showData = show.GetDataByIndex(0);
        var data = showData.data.GetValue<ShowCreatureInfo.SizeAndPos>(0);
        p.pet = PetCreature.Create(p, PetInfo.Create(rInfo.pet), p.position_ + data.pos, p.eulerAngles, false, Module_Home.PET_OBJECT_NAME);
        p.pet.enableUpdate = false;
        return p;
    }

    protected virtual void OnBattleTimerEnd()
    {
        EnterState(BattleStates.Winning);
    }

    protected virtual void CreatePlayerAudioListener()
    {
        if (!player) return;

        combatCamera._EnableDisableAudioListener(false);

        var pr = m_player.transform.Find("root");
        if (!pr) pr = m_player.transform;

        var listener = new GameObject("audioListener").GetComponentDefault<AudioListenerHelper>();
        listener.UpdateParams(new Vector3(0, (float)m_player.colliderSize * 2, 0), new Vector3((float)m_player.colliderSize * 2f, (float)m_player.colliderSize * 4, (float)m_player.colliderSize * 2));

        Util.AddChild(pr, listener.transform);
    }

    protected virtual bool PlayVictoryAnimation()
    {
        player.enableUpdate = true;
        if(player.pet)
            player.pet.enableUpdate = true;

        var vn = WeaponInfo.GetVictoryAnimation(player.weaponID, player.gender);
        if (string.IsNullOrEmpty(vn)) return false;

        var ww = GetPreloadObject<GameObject>(vn);
        if (!ww) return false;

        var anim = ww.GetComponent<Animation>();
        if (!anim || !anim.clip) return false;

        var camPos = ww.transform.childCount > 0 && ww.transform.GetChild(0).childCount > 0 ? ww.transform.GetChild(0).GetChild(0) : null;

        if (!camPos) return false;

        m_player.direction = CreatureDirection.FORWARD;
        m_player.stateMachine.TranslateToID(StateMachineState.STATE_VICTORY);

        m_victoryTime = GetStateTimer(BattleStates.Winning) + (int)((anim.clip.length + CombatConfig.sendWindowDelay) * 1000);
        if (m_victoryTime < 0) m_victoryTime = 0;

        combatCamera.enabled = false;

        Util.AddChild(player.model, ww.transform, false);
        Util.AddChild(camPos, combatCamera.transform, Vector3.zero, Vector3.one, new Vector3(0, 180, 0));
        OnPlayLightVictoryAnimation(combatCamera.transform.localEulerAngles, m_victoryTime);

        OnPlayVictoryAnimation();

        return true;
    }

    protected virtual void YouAreLuckyBaby()
    {
        combatCamera._enableShotSystem = false;
        Window.Hide(combatWindow, true);

        moduleGlobal.FadeOut(CombatConfig.screenFadeOutAlpha, CombatConfig.screenFadeOutDuration, true);

        PlayVictoryAnimation();
    }

    protected virtual bool WaitEndState() { return true; }

    protected virtual void OnEnterState(int oldMask, BattleStates state)
    {
        if (state != BattleStates.Watch)
        {
            FightRecordManager.RecordLog< LogInt>(log =>
            {
                log.tag = (byte)TagType.EnterState;
                log.value = (int)state;
            });
        }

        moduleBattle.parseInput = m_stateMask.BitMask(Fighting);

        switch (state)
        {
            case BattleStates.Ended:
            {
                if (modulePlayer.isLevelUp) Window.SetWindowParam<Window_Settlement>(oldLv, oldPoint, oldFatigue, oldMaxFatigue);
                Window.ShowAsync(endWindow, w => { if (w) w.animationSpeed = CombatConfig.sendWindowFadeInSpeed; });
                break;
            }
            case BattleStates.Winning:
            case BattleStates.Losing:
            {
                ObjectManager.Foreach<Creature>(c => { c.ClearBuffs(); c.moveState = 0; return true; });   // Clear all buffs, reset movement state

                Window.Hide(combatWindow, true);
                Window.Hide(ultimateMaskWindow, true);

                AudioManager.PauseAll(AudioTypes.Music);
                m_victoryTime = (int)((CombatConfig.screenFadeInDuration + CombatConfig.screenFadeOutDuration) * 1000) - 500;
                if (state == BattleStates.Winning)
                {
                    moduleGlobal.FadeIn(CombatConfig.screenFadeInAlpha, CombatConfig.screenFadeInDuration, false, YouAreLuckyBaby);
                    AudioManager.PlayMusicMix(heavenMusic);
                }
                else
                {
                    Camera_Combat.enableShotSystem = false;
                    AudioManager.PlayMusicMix(hellMusic);
                }
                break;
            }
            default: break;
        }
    }

    protected virtual void OnQuitState(int oldMask, BattleStates state) { }

    protected virtual void OnPrepareStateUpdate(int diff)
    {
        // Default we do not have any prepare event
        EnterState(BattleStates.Fighting);
    }

    protected virtual void OnFightingStateUpdate(int diff) { }

    protected virtual void OnWinningStateUpdate(int diff)
    {
        if ((m_victoryTime < 1 || m_stateTimers[Winning] > m_victoryTime) && WaitEndState())
            EnterState(BattleStates.Ended);
    }

    protected virtual void OnLosingStateUpdate(int diff)
    {
        if (WaitEndState())
            EnterState(BattleStates.Ended);
    }

    protected virtual void OnEndedStateUpdate(int diff) { }

    protected virtual void OnSlowMotionStateUpdate(int diff)
    {
        var t = m_stateTimers[SlowMotion];
        if (t > CombatConfig.sslowMotionDuration)
        {
            if (ObjectManager.timeScale != 1.0) ObjectManager.timeScale = 1.0;
            QuitState(BattleStates.SlowMotion);
        }
        else ObjectManager.timeScale = EvaluateSlowMotionCurve(t);
    }

    protected virtual void OnPlayVictoryAnimation()
    {
        // Hide all other creatures
        ObjectManager.Foreach<Creature>(c =>
        {
            if (c != player)
                c.enabledAndVisible = false;  // Disable update, player not visible
            return true;
        });
    }

    //----做灯光旋转----
    protected virtual void OnPlayLightVictoryAnimation(Vector3 dst,int time)
    {

       
        Quaternion calculaterot = Quaternion.FromToRotation(/*Level.current.directionalLight.transform.eulerAngles*/Vector3.zero, dst);

        Level.current.directionalLight.transform.rotation = Quaternion.Euler(Vector3.zero);
        //if (Level.current.directionalLight != null)
        Level.current.directionalLight.transform.DOBlendableRotateBy(calculaterot.eulerAngles - (player.roleProto == 2?new Vector3(0,35,0):Vector3.zero), time == 0 ? 25: time/200).SetEase(Ease.OutBounce);
        
    }

    public virtual double[] GetHpForVerification()
    {
        return new double[] { 0 };
    }

    public Creature CreateSceneActor(int rMonsterId, Vector3_ rPos, CreatureDirection rDir, int rGroup, int rLevel)
    {
        MonsterCreature actor = MonsterCreature.CreateMonster(rMonsterId, rGroup, rLevel, rPos, Vector3_.zero);
        if (!actor) return null;

        actor.isBoss = false;
        actor.enabledAndVisible = true;
        actor.invincibleCount = int.MaxValue;
        actor.behaviour.hitCollider.isTrigger = false;
        actor.checkEdge = false;
        actor.SetHealthBarVisible(false);
        actor.forceDirection = (int)rDir;

        return actor;
    }

    #region 观战相关

    public virtual Creature GetWatchTarget(Creature rWatcher)
    {
        return null;
    }

    public virtual bool IsPlayerAllDead()
    {
        return player.isDead;
    }
    #endregion


    #region Debug helper

#if DEVELOPMENT_BUILD || UNITY_EDITOR

    protected override void OnInitialized()
    {
        #region Debug
        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        EventManager.AddEventListener("OnPostGmLockClass", OnPostGmLockClass);
        #endif
        #endregion
    }

    private void OnPostGmLockClass(Event_ e)
    {
        var mm = (bool)e.param1;
        moduleGlobal.LockUI("拼命加载中...", 0.3f, 35.0f);
        PrepareAssets(Module_Battle.BuildPlayerPreloadAssets(), f =>
        {
            moduleGlobal.UnLockUI();

            if (!f || !this) return;

            if (!mm || !m_player)
            {
                var info = modulePlayer.BuildPlayerInfo();

                var old = m_player;
                var pet = m_player?.pet;
                var pos = m_player ? m_player.position_ : playerStart;
                var dir = m_player ? m_player.direction : CreatureDirection.FORWARD;
                
                m_player = Creature.Create(info, pos, new Vector3(0, 90, 0), true, "player", modulePlayer.name_);
                m_player.roleId = modulePlayer.id_;
                m_player.roleProto = modulePlayer.proto;
                m_player.position = pos;
                m_player.direction = dir;
                m_player.teamIndex = old ? old.teamIndex : 0;
                m_player.roleId = old ? old.roleId : modulePlayer.id_;

                if (old)
                {
                    m_player.regenRage          = old.regenRage;
                    m_player.regenRagePercent   = old.regenRagePercent;
                    m_player.regenHealth        = old.regenHealth;
                    m_player.regenHealthPercent = old.regenHealthPercent;
                    m_player.attack             = old.attack;
                }

                m_player.pet = pet;
                if (pet) pet.ParentCreature = m_player;

                if (old) old.Destroy();

                CharacterEquip.ChangeCloth(m_player, moduleEquip.currentDressClothes);

                _SetDOFFocusTarget(m_player.transform);

                CreatePlayerAudioListener();
            }
            else
            {
                m_player.elementType = (int)modulePlayer.elementType;
                m_player.UpdateWeapon(moduleEquip.weaponID, moduleEquip.weaponItemID);
            }
        });
    }

#endif

    #endregion

    #region 结算升级参数
    private byte oldLv;
    private ushort oldPoint;
    private ushort oldFatigue;
    private ushort oldMaxFatigue;

    void _ME(ModuleEvent<Module_Player> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Player.EventLevelChanged:
                oldLv = (byte)e.param1;
                oldPoint = (ushort)e.param2;
                oldFatigue = (ushort)e.param3;
                oldMaxFatigue = (ushort)e.param4;
                break;
            default: break;
        }
    }

    #endregion
}
