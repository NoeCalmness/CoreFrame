using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using URandom = UnityEngine.Random;

public class Level_PVE : Level_Battle
{
    /// <summary>
    /// 等待结算的超时时间
    /// </summary>
    public const int PVE_SETTLEMENT_TIMEOUT = 15;
    /// <summary>
    /// 初始化avatar
    /// </summary>
    public const string EventInitPVERightAvatar = "EventInitPVERightAvatar";

    public readonly static string[] PVE_UI_ASSET_NAME = new string[] { "window_hpslider" , "window_pvecombat"};
    /// <summary>
    /// 最大加载几个场景资源  
    /// </summary>
    public const int MAX_LOADED_UNIT_SCENE_COUNT = 3;
    /// <summary>
    /// 最大加载几个场景资源  
    /// </summary>
    public const int MAX_UNIT_SCENE_COUNT = 10;
    /// <summary>
    /// 单位场景事件资源
    /// </summary>
    protected List<UnitSceneEventData> m_unitSceneEvents = new List<UnitSceneEventData>();
    /// <summary>
    /// 当前的单位场景事件的场景ID(用来作为key值)
    /// </summary>
    protected UnitSceneEventData m_currentUnitSceneEvent;
    /// <summary>
    /// 所有怪物的缓冲池
    /// </summary>
    protected List<MonsterCreature> m_creaturePool = new List<MonsterCreature>();
    /// <summary>
    /// 所有场景实体物的缓冲池
    /// </summary>
    protected List<MonsterCreature> m_sceneActorPool = new List<MonsterCreature>();
    /// <summary>
    /// 储存当前的每组怪物,结构：<group,当前组的怪物list>
    /// </summary>
    protected Dictionary<int, List<MonsterCreature>> m_groupMonsterDic = new Dictionary<int, List<MonsterCreature>>();
    /// <summary>
    /// 已经死亡的怪物
    /// </summary>
    protected List<MonsterCreature> m_deadMonster = new List<MonsterCreature>();
    /// <summary>
    /// 怪物的死亡时间，不能用字典在协同中操作，所以只能单独操作m_deadMonster链表
    /// </summary>
    private Dictionary<MonsterCreature, double> m_deadMonsterDeadTime = new Dictionary<MonsterCreature, double>();
    private List<MonsterCreature> m_needRemoveDeadMonsters = new List<MonsterCreature>();

    //所有的场景触发器
    private List<PVETriggerCollider> m_triggerColliders = new List<PVETriggerCollider>();
    protected List<PVETriggerCollider> m_transportColliders = new List<PVETriggerCollider>();

    #region 怪物离场所需链表
    /// <summary>
    /// 需要每帧检测的离场怪物列表
    /// </summary>
    private List<MonsterCreature> m_handleLeaveCreatures = new List<MonsterCreature>();
    /// <summary>
    /// 只用来存储当前角色离场结束的时间节点
    /// </summary>
    private Dictionary<Creature, double> m_leaveMonsterEndTimeDic = new Dictionary<Creature, double>();
    private Dictionary<Creature, SLeaveMonsterBehaviour> m_leaveMonsterBehaviourData = new Dictionary<Creature, SLeaveMonsterBehaviour>();
    #endregion

    //上一次死亡的boss缓存
    private MonsterCreature m_lastDeadBoss;
    private Creature        m_assistCreature;
    //是否已经发送pve结束消息
    protected bool m_isPVEFinish;
    protected double m_pveFinishTime;
    //是否收到了结算消息
    protected bool m_recvOverMsg = false;

    public static UnitSceneEventData CurrentSceneEventData
    {
        get
        {
            Level_PVE s = current as Level_PVE;
            return s?.m_currentUnitSceneEvent;
        }
    }

    public static bool PVEOver
    {
        get
        {
            if (current && current is Level_PVE)
            {
                Level_PVE s = current as Level_PVE;
                return s.m_isPVEFinish || s.m_recvOverMsg;
            }
            return false;
        }
    }

    public override Creature assistCreature  { get { return m_assistCreature; } }

    protected override Vector3_ playerStart { get { return new Vector3_(modulePVE.reletivePos, 0, 0); } }
    private EnumMonsterTipArrow m_monsterTip;

    //上一次的shot状态
    private bool m_lastShotState = false;

    //延迟发送切换场景成功事件
    private int m_delayTransportEvent = -1;
    /// <summary>
    /// 切换场景转移事件
    /// </summary>
    protected int m_beginTransportDelayId;
    /// <summary>
    /// 切换黑屏延迟事件
    /// </summary>
    protected int m_transportDarkId;
    private bool m_currentMonsterInvincible = false;
    public bool isEndingState { get { return IsState(BattleStates.Winning) || IsState(BattleStates.Losing) || IsState(BattleStates.Ended); } }

    private int m_bossrewardDelayEventId;

    /// <summary>
    /// 自动跑向传送门
    /// </summary>
    private bool needAutoGotoTransport = false;
    protected Vector3_ m_originalEdge;

    #region boss camera animation
    /// <summary>
    /// boss出场动画的延迟事件ID
    /// </summary>
    private MonsterCreature m_animMonster;
    private int m_animDelayId;
    private int m_animWhiteScreenFadeInId;
    private int m_animWhiteScreenFadeOutId;
    private GameObject m_cameraAnimObj;
    private Transform m_cameraParent;
    private Vector3 m_animCameraPos;
    private Vector3 m_animCameraRot;
    #endregion

    #region 流程相关
    protected override string prepareEffect{ get { return string.Empty; } }
    #endregion

    #region pve type
    /// <summary>
    /// 当前模式的pve是否可以点击暂停按钮
    /// </summary>
    public virtual bool canPause { get { return true; } }
    public virtual EnumPveLevelType pveLevelType { get { return EnumPveLevelType.NormalPVE; } }
    public bool isNormalPve { get { return pveLevelType == EnumPveLevelType.NormalPVE; } }
    public bool isUnionBoss { get { return pveLevelType == EnumPveLevelType.UnionBoss; } }
    public bool isAwakeLevel { get { return pveLevelType == EnumPveLevelType.TeamLevel; } }

    /// <summary>
    /// 所有pve行为事件操作player的地方，都必须使用该参数
    /// </summary>
    public virtual Creature eventPlayer { get { return player; } }

    protected virtual bool NeedAutoGotoTransport
    {
        get
        {
            return needAutoGotoTransport;
        }

        set
        {
            needAutoGotoTransport = value;
        }
    }
    #endregion

    protected override void OnDestroy()
    {
        base.OnDestroy();

        m_lastShotState = false;
        m_currentMonsterInvincible = false;
        InitUnitData();
        UIManager.SetCamera();
        if (moduleAI != null) moduleAI.CloseAI();
        //StopCoroutine(AfterCreatureDeath());
        modulePVEEvent.ClearAll();

        //modulePVE.ClearPveCache();
        modulePVE.SaveLocalAutoBattleRecord();
        moduleMatch.isbaning = true;
        //强制恢复timescale为1,避免玩家在慢动作的时候点击退出
        ObjectManager.timeScale = 1;
        m_pveFinishTime = 0;
        m_lastDeadBoss = null;
        m_originalEdge = Vector3_.zero;

        m_animMonster = null;
        m_assistCreature = null;
        DelayEvents.Remove(m_bossrewardDelayEventId);
        DelayEvents.Remove(m_delayTransportEvent);
        DelayEvents.Remove(m_beginTransportDelayId);
        DelayEvents.Remove(m_transportDarkId);
        DelayEvents.Remove(m_animDelayId);
        DelayEvents.Remove(m_animWhiteScreenFadeInId);
        DelayEvents.Remove(m_animWhiteScreenFadeOutId);
        moduleGlobal.SetPVELockState(string.Empty, 0, 0.1f);
        m_unitSceneEvents.Clear(); 
        Module_Story.DestoryStory();
    }

    protected virtual void InitUnitData()
    {
        m_groupMonsterDic.Clear();
        m_deadMonster.Clear();
        m_needRemoveDeadMonsters.Clear();
        m_deadMonsterDeadTime.Clear();
        m_creaturePool.Clear();
        m_sceneActorPool.Clear();

        m_handleLeaveCreatures.Clear();
        m_leaveMonsterEndTimeDic.Clear();
        m_leaveMonsterBehaviourData.Clear();
        m_triggerColliders.Clear();
        m_transportColliders.Clear();

        NeedAutoGotoTransport = false;
        PVETriggerCollider.triggerParent = null;
    }

    public override void OnUpdate(int diff)
    {
        base.OnUpdate(diff);
        UpdateLeaveMonster(diff * 0.001);
        UpdateMonsterTip();
        UpdateTransportColliderTip();
        AutoMoveToTransport();
        CheckSettleMentTimeOut(diff * 0.001);
        HandleAction();
        AfterCreatureDeath(diff * 0.001);
    }

    private void CheckSettleMentTimeOut(double deltaTime)
    {
        //当已经发送了结算消息,并且没有处于winning,losing,ending状态时，开始检测超时时间
        if(m_isPVEFinish && !isEndingState)
        {
            m_pveFinishTime += deltaTime;
            if(m_pveFinishTime >= PVE_SETTLEMENT_TIMEOUT)
            {
                Logger.LogError("超时检测，没有收到服务器消息或者任何响应，强制退出");
                m_isPVEFinish = false;

                //策划说直接断线，不做其他处理了
                moduleLogin.LogOut(true);
                m_recvOverMsg = true;
            }
        }
    }

    protected override List<string> OnBuildPreloadAssets(List<string> assets = null)
    {
        base.OnBuildPreloadAssets(assets);

        assets.AddRange(OnBuildPVEAssets());
        int loadedCount = 0;
        foreach (var item in m_unitSceneEvents)
        {
            assets.AddRange(item.allAssets);
            loadedCount++;
            if (loadedCount >= MAX_LOADED_UNIT_SCENE_COUNT) break;
        }
        OnBuildAssistantAssets(assets);
        return assets;
    }

    private void OnBuildAssistantAssets(List<string> assets)
    {
        if (null != modulePVE.assistMemberInfo)
        {
            var npcId = modulePVE.assistMemberInfo.npcId;
            if (modulePVE.assistMemberInfo.type == 1)
            {
                var npcInfo = ConfigManager.Get<NpcInfo>(npcId);
                var monsterInfo = ConfigManager.Get<MonsterInfo>(npcInfo.monsterId);
                assets.AddRange(SceneEventInfo.GetMonsterBaseAssets(monsterInfo, true));
            }
            else
            {
                Module_Battle.BuildPlayerPreloadAssets(modulePVE.assistMemberInfo.memberInfo, assets);
            }
        }
    }

    protected List<string> OnBuildPVEAssets()
    {
        var l = new List<string>();
        //加载UI资源
        l.AddRange(PVE_UI_ASSET_NAME);
        //加载迷宫会触发的buff
        l.AddRange(modulePVE.FillLabyrithBuffAsset());
        return l;
    }

    //开始启动时，加载配置表资源
    protected override void OnLoadStart()
    {
        base.OnLoadStart();
        MonsterCreature.ResetMonsterRoomIndex();
        InitUnitData();
        m_unitSceneEvents.Clear();
        CreateUnitSceneEvents();

        m_isPVEFinish = false;
    }

    /// <summary>
    /// 异步加载多场景
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator AsyncLoadOtherScene()
    {
        UnitSceneEventData defalut = null;
        int count = 1;
        foreach (var item in m_unitSceneEvents)
        {
            //默认场景已经加载完成，需要初始化组件
            if (item.defalutUnit)
            {
                item.InitUnitComponent();
                defalut = item;
                continue;
            }

            yield return item.LoadSceneAsync(); 
            count++;
            //到达最大加载数量
            if (count >= MAX_LOADED_UNIT_SCENE_COUNT) break;
        }
        if(defalut != null) Camera_Combat.current = defalut.cameraCombat;
    }

    protected override IEnumerator AsyncCreateObjectBeforeLoadComplete()
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        
        foreach (var item in m_unitSceneEvents)
        {
            if (!item.isActiveScene) continue;

            yield return item.CreateMonsterAsyn();
            yield return wait;
        }

        //多等一帧，等待渲染帧能处理过来，避免组队的时候一边机子过于卡顿
        yield return wait;
    }

    protected override void OnLoadComplete()
    {
        moduleAI.ResetAILog();
        base.OnLoadComplete();
        Window.ShowAsync<Window_HpSlider>();
        Window.ShowAsync<Window_PVECombat>();

        m_timeLimit = 0; //modulePVE.stageEventId == 1 ? 0 : levelInfo && levelInfo.timeLimit != 0 ? levelInfo.timeLimit : CombatConfig.sdefaultTimeLimit;

        m_isPVEFinish = false;
        m_recvOverMsg = false;
        m_pveFinishTime = 0;

        //进入迷宫的时候要还原血量
        if (modulePVE.reopenPanelType == PVEReOpenPanel.Labyrinth)
        {
            int damage = (int)(player.maxHealth * (1.0f - moduleLabyrinth.labyrinthSelfInfo.healthRate * 0.01f));
            player.TakeDamage(null, DamageInfo.CreatePreCalculate(damage));
            player.rage += moduleLabyrinth.labyrinthSelfInfo.angerRate;
            //Logger.LogDetail("进入关卡赋值血量和怒气health = {0},anger = {1},设置后怒气 = {2}", player.healthRate, moduleLabyrinth.labyrinthSelfInfo.angerRate, player.rageRate);
        }

        //set initial health and rage
        modulePVE.SetPveGameData(EnumPVEDataType.Health,    m_player.healthRate);
        modulePVE.SetPveGameData(EnumPVEDataType.Rage,      m_player.rageRate);
        
        m_lastShotState = false;

        EventManager.AddEventListener(Events.CAMERA_SHOT_UI_STATE,      OnCameraShotUIState);
        EventManager.AddEventListener(Events.UI_WINDOW_VISIBLE,         OnWindowDisplay);
        EventManager.AddEventListener(CreatureEvents.DEAD,              OnCreatureDead);

        if (player)
        {
            player.AddEventListener(CreatureEvents.MORPH_CHANGED,OnCreatureMorphChange);
            player.AddEventListener(CreatureEvents.AUTO_BATTLE_CHANGE,OnCreatureUseAIChange);
        }

        RefreshAllBuff();
    }

    protected void OnCreatureUseAIChange(Event_ e)
    {
        var useAi = (bool) e.param1;
        //重新更新是否自动前往传送点
        if (useAi)
        {
            NeedAutoGotoTransport = GetLiveMonsterCount() <= 0;
        }
    }

    protected override void OnLoadingWindowClose()
    {
        base.OnLoadingWindowClose();
        OnPVELoadingWindowClose();
    }

    protected override void YouAreLuckyBaby()
    {
        base.YouAreLuckyBaby();
        Window.Hide<Window_PVECombat>(true);
        SetAllMonsterHealthSliderVisible(false);
    }

    protected void OnPVELoadingWindowClose()
    {
        m_originalEdge = edge;
        Logger.LogDetail("m_originalEdge is {0}", m_originalEdge);
        StartCoroutine(ActiveUnitSceneEvent(modulePVE.stageEventId));
        modulePVEEvent.AddCondition(new SPlayerVocationConditon(modulePlayer.roleInfo == null ? (int)CreatureVocationType.Vocation1 : modulePlayer.proto));
    }

    #region PVE 初始化

    /// <summary>
    /// 由于pve场景会直接跳转到相同的pve场景，并且destroy会关闭AI处理，所以，初始化AI只能放在WaitBeforeLoadComplete及之后的步骤
    /// 避免重新StartAI（比如OnLoadStart中调用）之后再关闭AI，可能导致AI不能正常操作
    /// </summary>
    private void InitMonsterAI()
    {
        //无论NPC还是怪物都初始化AI
        moduleAI.InitMonsterAI(m_creaturePool);
    }

    protected virtual void InitPetAI()
    {
        if (m_player.pet != null)
        {
            moduleAI.AddPetAI(m_player.pet);
        }
    }

    #endregion

    #region PVE事件

    #region create monster
    private void OnCreateMonster(SCreateMonsterBehaviour behaviour)
    {
        MonsterInfo info = ConfigManager.Get<MonsterInfo>(behaviour.monsterId);
        if (info)
        {
            //缓冲池获取
            MonsterCreature monster = GetMonsterFromPool(behaviour.monsterId, behaviour.group, behaviour.level);
            //资源创建
            if (!monster)
            {
                monster = MonsterCreature.CreateMonster(behaviour.monsterId, behaviour.group, behaviour.level, Vector3_.right * behaviour.reletivePos, new Vector3(0, 90f, 0), Module_PVE.instance?.currrentStage);
                //动态创建的，手动添加AI
                moduleAI.AddMonsterAI(monster);
            }
            if (!monster || !monster.gameObject) return;

            monster.position_ = Vector3_.right * behaviour.reletivePos;
            monster.isBoss = behaviour.boss;
            if (monster.isDestructible) monster.SetHealthBarVisible(false);
            monster.forceDirection = behaviour.forceDirection;
            monster.getReward = behaviour.getReward;

            //添加阵营 
            moduleAI.AddCreatureToCampDic(monster);
            //切换当前怪物的锁敌信息
            moduleAI.ChangeLockEnermy(monster, true);

            //先注册帧事件,避免策划将帧事件注册在出生动画中
            if (behaviour.frameEventId > 0) modulePVEEvent.RegisterFrameEvent(monster, behaviour.frameEventId);

            //添加锁敌信息后才能开始处理怪物的出生播放的状态
            if (!string.IsNullOrEmpty(info.bornStateName))
            {
                //Logger.LogDetail("{0} 播放出生动画 {1}",monster.gameObject.name,info.bornStateName);
                monster.stateMachine.TranslateTo(info.bornStateName);
                moduleAI.ChangeTurn(monster);
            }

            if (behaviour.showCameraAnim) PlayMonsterCameraAnimation(monster,info);

            //添加初始化BUFF
            if (info.initBuffs != null && info.initBuffs.Length > 0)
            {
                for (int i = 0; i < info.initBuffs.Length; i++)
                {
                    //Logger.LogDetail("{0} 添加初始化BuffId = {1}", monster.gameObject.name, info.initBuffs[i]);
                    Buff.Create(info.initBuffs[i],monster);
                }
            }

            monster.AddEventListener(CreatureEvents.DEAD_ANIMATION_END, OnMonsterRealDeath);
            monster.AddEventListener(CreatureEvents.HEALTH_CHANGED,     OnMonsterHealthChange);
            monster.AddEventListener(CreatureEvents.ATTACK,             OnMonsterHitCreature);
            monster.AddEventListener(CreatureEvents.BUFF_TRIGGER,       OnMonsterBuffTrigger);
            monster.AddEventListener(CreatureEvents.DEAD,               OnNormalMonsterDeadImme);
            if(monster.creatureCamp == CreatureCamp.MonsterCamp)
            {
                monster.AddEventListener(CreatureEvents.ATTACKED, OnMonsterWasAttacked);
            }

            if (monster.getReward) monster.AddEventListener(CreatureEvents.DEAD, OnRewardMonsterDeadImme);

            moduleAI.LogAIMsg(monster, true, "[level -> {0}] [boss -> {1}] [stateMachine -> {2}] [AI -> {3}]",monster.leech,monster.isBoss,Creature.GetAnimatorName(monster.monsterInfo.montserStateMachineId,1), monster.monsterInfo.AIId);
            moduleAI.LogAIMsg(monster, true, "[AddEventListener ENTER_STATE]");
            
            List<MonsterCreature> monsterList = null;
            if (m_groupMonsterDic.TryGetValue(behaviour.group, out monsterList))
            {
                monsterList.Add(monster);
            }
            else
            {
                monsterList = new List<MonsterCreature>();
                monsterList.Add(monster);
                m_groupMonsterDic.Add(behaviour.group, monsterList);
            }

            if (monster.creatureCamp != CreatureCamp.PlayerCamp && !monster.isDestructible && (behaviour.boss || CanInitRight()))
            {
                Event_ initMonster = Event_.Pop(monster);
                DispatchEvent(EventInitPVERightAvatar, initMonster);
            }

            AfterCreateMonster(monster);
            monster.enabledAndVisible = true;
        }
    }
    /// <summary>
    /// 当怪物被攻击时
    /// </summary>
    /// <param name="e"></param>
    private void OnMonsterWasAttacked(Event_ e)
    {
        modulePVEEvent.AddHitTimesCondition(e.sender as MonsterCreature);
    }

    #region boss 镜头动画 

    private void PlayMonsterCameraAnimation(MonsterCreature mon,MonsterInfo info)
    {
        Module_AI.LogBattleMsg(mon, "PlayMonsterCameraAnimation......");
        DelayEvents.Remove(m_animDelayId);
        DelayEvents.Remove(m_animWhiteScreenFadeInId);
        DelayEvents.Remove(m_animWhiteScreenFadeOutId);

        var an = info.cameraAnimation;
        if (string.IsNullOrEmpty(an)) return;

        var ww = GetPreloadObject<GameObject>(an);
        if (!ww)
        {
            Logger.LogError("monster camera animation {0} cannot be finded...", an);
            return;
        }

        m_cameraAnimObj = ww.gameObject;
        var anim = ww.GetComponent<Animation>();
        if (!anim || !anim.clip) return;

        var camPos = ww.transform.childCount > 0 && ww.transform.GetChild(0).childCount > 0 ? ww.transform.GetChild(0).GetChild(0) : null;

        if (!camPos) return;

        var animClip = ConfigManager.Find<AnimClipInfo>(a=>a.name == info.cameraAnimation);
        if (!animClip) Logger.LogError("cannot find valid monster camera Animation with name {0} from config_animclipinfo.asset");
        double clipLen = animClip ? animClip.length : anim.clip.length;
        double time = clipLen - CombatConfig.spveBossAnimWhiteAhead;

        if(combatCamera == null)
        {
            Logger.LogError("combatCamera is null,we can't play camera animation for monster {0}",mon.uiName);
            return;
        }

        m_animMonster = mon;
        combatCamera.enabled = false;
        OnMonsterAnimation(false);
        HideObjectsWhenCameraAnimation(false);
        m_animMonster.SetHealthBarVisible(false);
        UIManager.SetCameraLayer(Layers.Dialog);
        SetWindowInputState(false);
        m_cameraParent = combatCamera.transform.parent;
        m_animCameraPos = combatCamera.transform.localPosition;
        m_animCameraRot = combatCamera.transform.localEulerAngles;
        Util.AddChild(mon.model, ww.transform, false);
        Util.AddChild(camPos, combatCamera.transform, Vector3.zero, Vector3.one, new Vector3(0, 180, 0));
    
        m_animDelayId = DelayEvents.AddLogicDelay(OpenWhiteScreen, time);
    }

    private void SetWindowInputState(bool inputState)
    {
        Window_Combat w = Window.GetOpenedWindow<Window_Combat>();
        if (w) w.inputState = inputState;

        Window_PVECombat pw = Window.GetOpenedWindow<Window_PVECombat>();
        if (pw) pw.inputState = inputState;
    }

    private void OpenWhiteScreen()
    {
        Module_AI.LogBattleMsg(m_animMonster, "OpenWhiteScreen......");
        modulePVE.DispathBossWhiteAnim();
        m_animWhiteScreenFadeInId = DelayEvents.AddLogicDelay(OnMonsterCameraAnimWhiteFadeInComplete, CombatConfig.spveBossAnimWhiteFadeIn);
        m_animWhiteScreenFadeOutId = DelayEvents.AddLogicDelay(OnMonsterCameraAnimComplete, CombatConfig.spveBossAnimWhiteFadeIn + CombatConfig.spveBossAnimWhiteFadeOut);
    }

    /// <summary>
    /// 白屏完成
    /// 恢复相机位置，恢复玩家显示
    /// </summary>
    private void OnMonsterCameraAnimWhiteFadeInComplete()
    {
        Module_AI.LogBattleMsg(m_animMonster, "OnMonsterCameraAnimWhiteFadeInComplete......");
        Util.AddChild(m_cameraParent, combatCamera.transform, m_animCameraPos, Vector3.one, m_animCameraRot);
        if (m_cameraAnimObj) Object.Destroy(m_cameraAnimObj);
        combatCamera.enabled = true;
        HideObjectsWhenCameraAnimation(true);
        m_animMonster.SetHealthBarVisible(true);
        UIManager.SetCameraLayer(Layers.UI); 
    }

    /// <summary>
    /// 白屏淡出完成(也是所有动画阶段完成)
    /// 开启AI，开启玩家控制
    /// </summary>
    private void OnMonsterCameraAnimComplete()
    {
        Module_AI.LogBattleMsg(m_animMonster, "OnMonsterCameraAnimComplete......");
        SetWindowInputState(true);
        m_animMonster = null;
        DelayEvents.Remove(m_animDelayId);
        DelayEvents.Remove(m_animWhiteScreenFadeInId);
        DelayEvents.Remove(m_animWhiteScreenFadeOutId);
        OnMonsterAnimation(true);
    }

    protected virtual void HideObjectsWhenCameraAnimation(bool visible)
    {
        SetAllMonsterVisible(visible);
        m_player?.model.SafeSetActive(visible);
    }

    protected virtual void OnMonsterAnimation(bool complete)
    {
        moduleBattle.parseInput = complete;
        moduleAI.SetAllAIPauseState(!complete);
        modulePVEEvent.pauseCountDown = !complete;
    }

    #endregion 

    protected virtual void AfterCreateMonster(MonsterCreature mon)
    {

    }

    private bool CanInitRight()
    {
        int monsterGroupCount = 0;
        foreach (var item in m_groupMonsterDic)
        {
            if (item.Key != -2) monsterGroupCount++;
        }
        return monsterGroupCount > 0;
    }

    private MonsterCreature GetMonsterFromPool(int monsterId, int group,int level)
    {
        MonsterCreature creature = m_creaturePool.Find(o=>o.monsterId == monsterId && o.monsterGroup == group && o.monsterLevel == level);

        if (creature == null) Logger.LogWarning("monsterId = {0},group = {1}的怪物已经不存在对象池中", monsterId, group);
        else m_creaturePool.Remove(creature);

        return creature;
    }

    private void OnMonsterHitCreature(Event_ e)
    {
         modulePVEEvent.AddMonsterAttackCondition(e.sender as MonsterCreature);
    }

    private void OnNormalMonsterDeadImme(Event_ e)
    {
        MonsterCreature mon = e.sender as MonsterCreature;
        if (mon && mon.health == 0) modulePVE.RefreshTotalDeathMonster(mon.monsterId,1);
    }

    private void OnRewardMonsterDeadImme(Event_ e)
    {
        MonsterCreature mon = e.sender as MonsterCreature;
        if (mon && mon.health == 0)
        {
            modulePVE.SendBossReward(mon.monsterId);
            //组队模式不暂停场景行为事件
            if (modulePVE.isTeamMode) return;
            DelayEvents.Remove(m_bossrewardDelayEventId);
            m_bossrewardDelayEventId = DelayEvents.Add(OnSendBossRewardTimeOut,PVE_SETTLEMENT_TIMEOUT);
        }
    }

    private void OnSendBossRewardTimeOut()
    {
        Logger.LogError("等待随机boss奖励消息回复超时...");
        DelayEvents.Remove(m_bossrewardDelayEventId);
        modulePVEEvent.pauseEvent = false;
    }
    #endregion

    #region kill monster

    private void OnKillMonster(SKillMonsterBehaviour behaviour)
    {
        if (behaviour.type == SceneEventInfo.SceneBehaviouType.KillMonster)
        {
            List<MonsterCreature> targetMonster = GetTargetGroupMonsters(behaviour.group, behaviour.monsterId);

            int deathCount = behaviour.amount == -1 ? targetMonster.Count : behaviour.amount < targetMonster.Count ? behaviour.amount : targetMonster.Count;
            //随机死亡
            while (deathCount > 0)
            {
                int index = URandom.Range(0, targetMonster.Count);
                targetMonster[index].Kill();
                targetMonster.RemoveAt(index);
                deathCount--;
            }
        }
    }

    private List<MonsterCreature> GetTargetGroupMonsters(int group,int monsterId)
    {
        List<MonsterCreature> monsters = new List<MonsterCreature>();

        if (group == -1)
        {
            foreach (var item in m_groupMonsterDic)
            {
                if (monsterId == -1) monsters.AddRange(item.Value.FindAll(o => o.health > 0 && o.monsterGroup != -2 && o.gameObject.activeInHierarchy));
                else monsters.AddRange(item.Value.FindAll(o => o.health > 0 && o.monsterGroup != -2 && o.monsterId == monsterId && o.gameObject.activeInHierarchy));
            }
        }
        //group = -2跟 group = 1没有任何区别，都是指定的分组
        else
        {
            List<MonsterCreature> temp = null;
            if (m_groupMonsterDic.TryGetValue(group, out temp))
            {
                if(monsterId == -1) monsters.AddRange(temp.FindAll(o => o.health > 0 && o.gameObject.activeInHierarchy));
                else monsters.AddRange(temp.FindAll(o => o.health > 0 && o.monsterId == monsterId && o.gameObject.activeInHierarchy));
            }
        }

        return monsters;
    }
    #endregion

    #region add buff
    private void OnAddBuff(SAddBufferBehaviour behaviour)
    {
        if(behaviour.type == SceneEventInfo.SceneBehaviouType.AddBuffer)
        {
            BuffInfo buff = ConfigManager.Get<BuffInfo>(behaviour.buffId);
            if(buff == null)
            {
                Logger.LogError("on the SceneBehaviour, buffId = {0} cannot be finded ,please check out", behaviour.buffId);
                return;
            }

            if(behaviour.objectId == -2) AddBuffToAllPlayer(buff, behaviour.duraction);
            else if(behaviour.objectId == -1) AddBuffToAllMonster(buff, behaviour.duraction);
            else AddBuffToTargetMonster(buff, behaviour.objectId, behaviour.duraction);
        }
    }

    protected virtual void AddBuffToAllPlayer(BuffInfo buff,int duraction)
    {
        Buff.Create(buff, player,player,duraction);
    }

    private void AddBuffToAllMonster(BuffInfo buff, int duraction)
    {
        Dictionary<int, List<MonsterCreature>>.Enumerator e = m_groupMonsterDic.GetEnumerator();
        while (e.MoveNext())
        {
            List<MonsterCreature> monsters = e.Current.Value;
            for (int i = 0; i < monsters.Count; i++)
            {
                if(monsters[i].health > 0 && monsters[i].monsterGroup != -2)
                {
                    Buff.Create(buff, monsters[i], monsters[i], duraction);
                }
            }
        }
    }

    private void AddBuffToTargetMonster(BuffInfo buff, int monsterId, int duraction)
    {
        Dictionary<int, List<MonsterCreature>>.Enumerator e = m_groupMonsterDic.GetEnumerator();
        while (e.MoveNext())
        {
            List<MonsterCreature> monsters = e.Current.Value;
            for (int i = 0; i < monsters.Count; i++)
            {
                if (monsters[i].monsterId == monsterId && monsters[i].health > 0)
                {
                    Buff.Create(buff, monsters[i], monsters[i], duraction);
                }
            }
        }
    }

    #endregion

    #region stage over
    protected virtual void OnStageClear(SStageClearBehaviour behaviour)
    {
        //only send over msg one time
        if (m_isPVEFinish) return;

        moduleActive.SendMaxCombo();
        Logger.LogInfo("-------PVEEvnetHandle Callback-----Send     OnStageClear-----------------");
        modulePVE.SendPVEState(PVEOverState.Success, player);
        OnStageOver();
    }

    protected virtual void OnStageFail(SStageFailBehaviour behaviour)
    {
        //only send over msg one time
        if (m_isPVEFinish) return;

        moduleActive.SendMaxCombo();
        Logger.LogInfo("-----PVEEvnetHandle Callback-------Send   OnStageFail-----------------");
        modulePVE.SendPVEState(PVEOverState.GameOver, player);
        OnStageOver();
    }

    public virtual void OnStageOver()
    {
        //Logger.LogInfo("-----OnStageOver called-----------------");
        modulePVE.DebugPveGameData();
        m_pauseTimer = true;
        m_isPVEFinish = true;
        moduleBattle.parseInput = false;
        moduleAI.SetAllAIPauseState(true);
        modulePVEEvent.pauseCountDown = true;
        if(!player.isDead) player.invincibleCount += 1;
        if (player.useAI && player.stateMachine) player.stateMachine.TranslateTo(Module_AI.STATE_STAND_NAME);
        Module_Story.DestoryStory();
        m_leaveMonsterEndTimeDic.Clear();
        m_leaveMonsterBehaviourData.Clear();
    }
    #endregion

    #region monster death
    protected virtual void OnMonsterRealDeath(Event_ e)
    {
        Creature c = (Creature)e.sender;
        //如果是PVE模式的怪物，死亡的时候进行判断
        if (current.isPvE && c.isMonster)
        {
            MonsterCreature monster = (MonsterCreature)c;

            if (!monster.isSendDeathCondition)
            {
                //展示boss奖励
                if (monster.getReward) modulePVE.DisplayBossReward();
                
                moduleAI.LogAIMsg(monster, true, "[OnMonsterRealDeath.................]");
                monster.isSendDeathCondition = true;
                modulePVEEvent.AddCondition(new SMonsterDeathConditon(monster.monsterId, monster.monsterGroup));
                //Logger.LogInfo("monster {0} SendCondition......", monster.name);

                //如果要销毁模型
                if (!monster.deathAppear)
                {
                    m_deadMonster.Add(monster);
                    m_deadMonsterDeadTime.Add(monster, Module_PVEEvent.PVE_DEATH_STAY_TIME);
                    monster.enableUpdate = false;

                    var de = monster.stateMachine.GetDeadEffect();
                    if (!de.isEmpty) monster.behaviour.effects.PlayEffect(de);
                }

                NeedAutoGotoTransport = GetLiveMonsterCount() <= 0;
                //从阵营列表中移除
                moduleAI.RemoveCreatureFromCampDic(monster);
            }
        }
    }

    private void OnSceneActorRealDeath(Event_ e)
    {
        Creature c = (Creature)e.sender;
        //如果是PVE模式的怪物，死亡的时候进行判断
        if (current.isPvE && c.isMonster)
        {
            MonsterCreature monster = (MonsterCreature)c;
            monster.enabledAndVisible = false;
            Debug.LogError("scene actor real death " + monster.name);
        }
       
    }

    private void AfterCreatureDeath(double diff)
    {
        m_needRemoveDeadMonsters.Clear();
        foreach (var item in m_deadMonster)
        {
            if (!m_deadMonsterDeadTime.ContainsKey(item))
            {
                m_needRemoveDeadMonsters.Add(item);
                continue;
            }

            m_deadMonsterDeadTime[item] -= diff;
            if(m_deadMonsterDeadTime[item] <= 0)
            {
                m_needRemoveDeadMonsters.Add(item);
                //策划《宋》需求不播死亡特效
//                CreatureBehaviour behaviour = item.behaviour;
//                //死亡特效
//                if (behaviour && behaviour.effects && item.stateMachine)
//                {
//                    behaviour.effects.PlayEffect(item.stateMachine.GetHitEffect());
//                }
                item.enabledAndVisible = false;
            }

        }

        //移除
        for (int i = 0; i < m_needRemoveDeadMonsters.Count; i++)
        {
            m_deadMonster.Remove(m_needRemoveDeadMonsters[i]);
            m_deadMonsterDeadTime.Remove(m_needRemoveDeadMonsters[i]);
            //m_needRemoveDeadMonsters[i].Destroy();
        }
    }

    private void OnMonsterHealthChange(Event_ e)
    {
        var creature = e.sender as MonsterCreature;
        modulePVEEvent.AddCondition(new SMonsterHPLessConditon(creature.monsterId, creature.monsterGroup, Mathf.RoundToInt((float)(creature.healthRateL * 100))));
    }
    #endregion

    #region monster revive

    //主要用于检测复活
    private void OnMonsterBuffTrigger(Event_ e)
    {
        var buff = e.param1 as Buff;
        var monster = e.sender as MonsterCreature;
        if (!buff || !monster) return;

        //成功复活了
        if (buff.HasEffect(BuffInfo.EffectTypes.Revive))
        {
            monster.isSendDeathCondition = false;
            moduleAI.AddCreatureToCampDic(monster);
            modulePVEEvent.ReviveMonster(monster);
            modulePVE.RefreshTotalDeathMonster(monster.monsterId,-1);
        }
    }
    #endregion

    #region leave monster
    private void OnCreatureLeave(SLeaveMonsterBehaviour behaviour)
    {
        if (behaviour.type != SceneEventInfo.SceneBehaviouType.LeaveMonster) return;
        
        List<MonsterCreature> targetMonster = GetTargetGroupMonsters(behaviour.group, behaviour.monsterId);
        if(targetMonster.Count > 0)
        {
            //默认只取一个角色
            MonsterCreature creature = targetMonster[0];

            //Logger.LogInfo("收到怪物离场事件......{0}开始处理", creature.gameObject.name);
            //添加缓存数据
            if (!m_leaveMonsterBehaviourData.ContainsKey(creature)) m_leaveMonsterBehaviourData.Add(creature, null);
            m_leaveMonsterBehaviourData[creature] = behaviour;

            //离场需要立即处理的行为
            HandleMonsterLeaveBehaviour(creature);

            //如果不能立即处理到结束，则需要等待其他动画做完,直接检测enterstate事件
            if (CheckHandleMonsterEndImmediate(creature)) HandleMonsterLeaveState(creature);
            else creature.AddEventListener(CreatureEvents.ENTER_STATE, CheckWhenMonCloseAI);
        }
    }
    
    private void CheckWhenMonCloseAI(Event_ e)
    {
        MonsterCreature creature = (MonsterCreature)e.sender;

        //可以执行离场时，就只执行一次该回调事件
        if (CheckHandleMonsterEndImmediate(creature))
        {
            //must remove listener first
            creature.RemoveEventListener(CreatureEvents.ENTER_STATE, CheckWhenMonCloseAI);
            HandleMonsterLeaveState(creature);
        }
    }

    /// <summary>
    /// 当前是否可以立即切换到离场 
    /// </summary>
    /// <param name="creature"></param>
    /// <returns></returns>
    private bool CheckHandleMonsterEndImmediate(MonsterCreature creature)
    {
        //应策划要求，离场时只需要检测是否是处于地面就行了，放宽限制条件
        if (creature && creature.onGround) return true;

        //if (creature.stateMachine != null)
        //{
        //    var curState = creature.stateMachine.currentState;
        //    //直接处理离场动画
        //    if (curState.isIdle || curState.isRun) return true;
        //}

        return false;
    }

    /// <summary>
    /// 一旦收到离场就进行的部分处理
    /// 1.关闭ai;2.将角色设置为无敌(去除无敌盒子？);3.去除阻挡盒子;4.解除场景边界的限制;5.清除所有buff;
    /// 6.将原本锁定为该怪物的角色重置一个锁敌目标
    /// </summary>
    /// <param name="monster"></param>
    private void HandleMonsterLeaveBehaviour(MonsterCreature monster)
    {
        //close ai
        moduleAI.RemovMonsterAI(monster);
        //must remove all buffs first
        monster.ClearBuffs();
        //set monster invincible
        monster.invincibleCount = int.MaxValue;
        //close creature collider
        monster.behaviour.collider_.enabled = false;
        //close creature hitcollider
        monster.behaviour.hitCollider.enabled = false;
        //close edge of scene
        monster.checkEdge = false;

        //remove monster from creature campdic and reset lock enermy 
        moduleAI.RemoveCreatureFromCampDic(monster);
        List<Creature> creatures = new List<Creature>();
        foreach (var item in moduleAI.lockEnermyDic)
        {
            if (item.Value && item.Value.Equals(monster)) creatures.Add(item.Key);
        }
        foreach (var item in creatures)
        {
            moduleAI.lockEnermyDic[item] = null;
            moduleAI.ChangeLockEnermy(item,true);
        }
    }

    /// <summary>
    /// 开始处理怪物离场的动画，并且开始对怪物离场完毕时间检索
    /// </summary>
    /// <param name="creature"></param>
    private void HandleMonsterLeaveState(MonsterCreature creature)
    {
        SLeaveMonsterBehaviour behaviour = m_leaveMonsterBehaviourData.Get(creature);
        if (behaviour == null || behaviour.type != SceneEventInfo.SceneBehaviouType.LeaveMonster)
        {
            Logger.LogError("{0} cannot be finded leave_monster_behaciour data",creature.gameObject.name);
            return;
        }
        m_leaveMonsterBehaviourData.Remove(creature);
        CreatureChangeToState(creature, behaviour.state, GetLeaveMonsterDirection(creature));

        //set leave end time
        var duraction = behaviour.leaveTime * 0.001;
        if (duraction <= 0) Logger.LogError("SceneEventId: {0}--Behaviour:[LeaveMonster] is wrong,LeaveTime is 0s. behaviour is {1}", modulePVE.stageEventId, behaviour.ToXml());
        if (!m_leaveMonsterEndTimeDic.ContainsKey(creature)) m_leaveMonsterEndTimeDic.Add(creature,0f);
        m_leaveMonsterEndTimeDic[creature] = duraction;
        if(!m_handleLeaveCreatures.Contains(creature)) m_handleLeaveCreatures.Add(creature);
    }

    /// <summary>
    /// 获取离场怪物的最后方向（离屏幕最近的方向）
    /// </summary>
    /// <returns></returns>
    private CreatureDirection GetLeaveMonsterDirection(MonsterCreature monster)
    {
        Vector3 screenPos = current.mainCamera.WorldToScreenPoint(monster.transform.position);
        float threshold = Screen.width / 2;
        //更靠近屏幕右侧,则方向是向右
        if (screenPos.x >= threshold)
            return CreatureDirection.FORWARD;
        else
            return CreatureDirection.BACK;
    }

    private void UpdateLeaveMonster(double dt)
    {
        if (m_handleLeaveCreatures == null || m_handleLeaveCreatures.Count == 0)
            return;

        MonsterCreature mon = null;
        for (int i = 0; i < m_handleLeaveCreatures.Count; i++)
        {
            mon = m_handleLeaveCreatures[i];
            if (!m_leaveMonsterEndTimeDic.ContainsKey(mon) || m_leaveMonsterEndTimeDic[mon] <= 0) continue;

            m_leaveMonsterEndTimeDic[mon] -= dt;
            //per frame only handle one monster
            if (m_leaveMonsterEndTimeDic[mon] <= 0)
            {
                m_leaveMonsterEndTimeDic.Remove(mon);
                m_handleLeaveCreatures.Remove(mon);
                HandleLeaveMonsterEnd(mon);
                break;
            }
        }
    }

    private void HandleLeaveMonsterEnd(MonsterCreature monster)
    {
        CreatureChangeToState(monster,Module_AI.STATE_STAND_NAME);
        modulePVEEvent.AddCondition(new SMonsterLeaveEndConditon(monster.monsterId, monster.monsterGroup));
        monster.visible = false;
    }

    private void CreatureChangeToState(Creature creature, string state,CreatureDirection dir = CreatureDirection.FORWARD)
    {
        creature.stateMachine.TranslateTo(state);

        creature.moveState = 0;
        //Special handling of running state
        if ((CreatureStateInfo.NameToID(state) == 4)) creature.moveState = dir == CreatureDirection.FORWARD ? 1 : -1;
    }

    #endregion

    #region monster set state
    private void OnMonsterSetState(SSetStateBehaviour behaviour)
    {
        if (behaviour.type != SceneEventInfo.SceneBehaviouType.SetState || string.IsNullOrEmpty(behaviour.state)) return;

        List<MonsterCreature> targetMonster = GetTargetGroupMonsters(behaviour.group, behaviour.monsterId);
        for (int i = 0; i < targetMonster.Count; i++)
        {
            if(behaviour.setStateType == SSetStateBehaviour.EnumSetStateType.OnGround && !targetMonster[i].onGround || 
                behaviour.setStateType == SSetStateBehaviour.EnumSetStateType.InTheAir && targetMonster[i].onGround)
            {
                Logger.LogWarning("set state to creature[{0}] failed,behaviour set type is {1},but creature ground state is {2}"
                    , targetMonster[i].uiName,behaviour.setStateType, targetMonster[i].onGround ? "on the ground":"in the air");
                continue;
            }
            CreatureChangeToState(targetMonster[i], behaviour.state);
            Logger.LogDetail("{0} force change to state {1}", targetMonster[i].name, behaviour.state);

            //In principle, no cyclic action is allowed.So,if a state is loop state,we need debug the error
            if (targetMonster[i].stateMachine.currentState.loop)
                Logger.LogError("SceneEventId: {0}--Behaviour:[SetState] is wrong,state is loop state. behaviour is {1}", modulePVE.stageEventId, behaviour.ToString());
        }
    }
    #endregion

    #region transportScene
    
    #region 场景分事件相关

    private void CreateUnitSceneEvents()
    {
        UnitSceneEventData defaultSceneEvent = null;
        if (!modulePVE.currrentStage) Logger.LogError("module pve stage is null....");
        //加载默认场景事件
        if (m_unitSceneEvents.Count == 0 && modulePVE.currrentStage)
        {
            defaultSceneEvent = new UnitSceneEventData(modulePVE.currrentStage);
            m_unitSceneEvents.Add(defaultSceneEvent);
        }

        //加载分场景事件
        CreateChildUnitDatas(defaultSceneEvent);
    }

    /// <summary>
    /// 创建子单元场景数据
    /// </summary>
    /// <param name="data"></param>
    private void CreateChildUnitDatas(UnitSceneEventData data)
    {
        if(modulePVE.isTeamMode)
            data.PreBuildRandomValue(moduleTeam.RoomID);
        else
            data.PreBuildRandomValue((ulong)DateTime.Now.Ticks);

        List<STransportSceneBehaviour> list = new List<STransportSceneBehaviour>();

        if (data != null && data.sceneEventInfo)
        {
            List<int> validTriggers = new List<int>();
            foreach (var e in data.sceneEventInfo.sceneEvents)
            {
                foreach (var b in e.behaviours)
                {
                    if (b.sceneBehaviorType == SceneEventInfo.SceneBehaviouType.CreateTrigger)
                    {
                        bool isValidTrigger = true;
                        foreach (var c in e.conditions)
                        {
                            if (c.sceneEventType == SceneEventInfo.SceneConditionType.RandomInfo)
                            {
                                if (!data.ContainsCondition(new SSceneConditionBase(c)))
                                {
                                    isValidTrigger = false;
                                    break;
                                }
                            }
                        }
                        if (isValidTrigger)
                        {
                            var tb = new SCreateTriggerBehaviour(b, null);
                            validTriggers.Add(tb.triggerId);
                        }
                    }
                }
            }

            foreach (var e in data.sceneEventInfo.sceneEvents)
            {
                foreach (var b in e.behaviours)
                {
                    if (b.sceneBehaviorType == SceneEventInfo.SceneBehaviouType.TransportScene)
                    {
                        bool isValidBehaviours = true;
                        foreach (var c in e.conditions)
                        {
                            if (c.sceneEventType == SceneEventInfo.SceneConditionType.EnterTrigger)
                            {
                                int triggerId = c.parameters.GetValue<int>(0);
                                if (!validTriggers.Contains(triggerId))
                                {
                                    isValidBehaviours = false;
                                    break;
                                }
                            }
                        }
                        if (!isValidBehaviours)
                            continue;

                        int levelId = b.parameters.GetValue<int>(0);
                        int eventId = b.parameters.GetValue<int>(2);
                        //过滤配置多个相同的场景和相同的事件
                        if (list.Find(o => o.levelId == levelId && o.eventId == eventId) != null) continue;

                        list.Add(new STransportSceneBehaviour(b, null));
                    }
                }
            }
        }

        if(list.Count > 2)
        {
            Logger.LogError("scene event id = {0} is the wrong data,TransportScene count more than {1}", data.eventId, MAX_LOADED_UNIT_SCENE_COUNT);
        }

        if (m_unitSceneEvents.Count > MAX_UNIT_SCENE_COUNT)
            return;

        foreach (var item in list)
        {
            UnitSceneEventData d = new UnitSceneEventData(item);
            m_unitSceneEvents.Add(d);
            var mutexBehaviour = list.Find(o => o.eventId != item.eventId);
            d.mutexUnitEventId = mutexBehaviour == null ? 0 : mutexBehaviour.eventId;
            CreateChildUnitDatas(d);
        }

#if PVE_EVENT_LOG
        System.Text.StringBuilder s = new System.Text.StringBuilder();
        s.AppendLine("after create unit scene datas");
        foreach (var item in m_unitSceneEvents)
        {
            s.AppendLine(Util.Format("unitSceneEvent :eventid = {0},levelId = {1},levelMap = {2} hasloaded = {3} hasdestoryed = {4} mutexId = {5}", item.eventId, item.levelId, item.levelMap, item.hasLoadedScene, item.hasDestoryed,item.mutexUnitEventId));
        }
        Logger.LogDetail(s.ToString());
#endif
    }

    protected IEnumerator ActiveUnitSceneEvent(int eventId)
    {
        UnitSceneEventData data = m_unitSceneEvents.Find(o => o.eventId == eventId);
        if (data == null)
        {
            Logger.LogError("cannot change to unit event ,event id = {0},we will force stage false", eventId);
            //OnStageFail(null);
            yield break;
        }

#if PVE_EVENT_LOG
        Logger.LogDetail("enter new unitSceneEvent :eventid = {0},levelId = {1},levelMap = {2}", data.eventId, data.levelId, data.levelMap);
#endif

        //loading unitSceneData
        UnitSceneEventData lastData = m_currentUnitSceneEvent;
        //change scene map and scene event
        m_currentUnitSceneEvent = data;
        //must call this function first
        m_currentUnitSceneEvent.BeforeSceneActive();
        OnTransportPlayer();
        m_currentUnitSceneEvent.TransportCache(lastData);
        m_currentUnitSceneEvent.LoadSceneAudios();

        //卸载老数据
        lastData?.Unload();
        m_unitSceneEvents.Remove(lastData);
#if PVE_EVENT_LOG
        if (lastData != null) Logger.LogDetail("unload old unitSceneEvent :eventid = {0},levelId = {1},levelMap = {2}", lastData.eventId, lastData.levelId, lastData.levelMap);
#endif

        //初始化
        InitUnitData();
        modulePVEEvent.LoadPVESceneEvent(m_currentUnitSceneEvent.sceneEventInfo);

        m_creaturePool.Clear();
        m_creaturePool.AddRange(m_currentUnitSceneEvent.creaturePool);

        m_sceneActorPool.Clear();
        m_sceneActorPool.AddRange(m_currentUnitSceneEvent.sceneActorPool);

        //call this function at last
        m_currentUnitSceneEvent.SetSceneActive(true);
        //call this function must before Any AI-Functions
        moduleAI.StartAI();
        InitMonsterAI();
        InitPetAI();
        moduleAI.SetAllAIPauseState(true);

        if (m_currentUnitSceneEvent.defalutUnit)
        {
            //逻辑的更新需要单独处理，避免UI造成逻辑帧更新延迟
            OnLogicTransportFinish();
            OnTransportTweenFinish();
        }
        else
        {
            DispatchEvent(Events.SCENE_LOAD_PROGRESS, Event_.Pop(1.0f));
            
            //组队模式 异步等待
            if (modulePVE.isTeamMode) yield return new WaitUntil(WaitBeforeChangeToNewScene);

#region 单机或者UI的操作才可以在一下代码段添加，任何组队时候的逻辑处理必须在OnLogicTransportFinish 中执行，保证帧事件的时间节点统一
            //单机模式直接开启逻辑更新
            if (modulePVE.isStandalonePVE) OnLogicTransportFinish();

#if AI_LOG
            Module_AI.LogBattleMsg(player, "[start delay function of tween finish time is {0} ....]", CombatConfig.spveTransportOutTime);
#endif
            DelayEvents.Remove(m_delayTransportEvent);
            m_delayTransportEvent = DelayEvents.AddLogicDelay(OnTransportTweenFinish, CombatConfig.spveTransportOutTime);
#endregion
        }
    }

    /// <summary>
    /// 场景切换完成的时候，统一开启逻辑帧的更新
    /// </summary>
    protected void OnLogicTransportFinish()
    {
#if AI_LOG
        Module_AI.LogBattleMsg(player, "[OnLogicTransportFinish  ....]");
#endif
        moduleBattle.parseInput = true;
        modulePVEEvent.enableUpdate = true;
        moduleAI.SetAllAIPauseState(false);
        OnLogicPlayerTransport();
        //must call this function at last(this condition will be call createmonster or startdialog behaviour)
        modulePVEEvent.AddCondition(new SEnterSceneConditon());
    }

    protected virtual void OnLogicPlayerTransport()
    {
        moduleAI.AddCreatureToCampDic(player);
        moduleAI.AddCreatureToCampDic(m_assistCreature);
    }

    /// <summary>
    /// 切换多场景时对玩家的操作
    /// </summary>
    protected virtual void OnTransportPlayer()
    {
        m_currentUnitSceneEvent.SetPlayerData(m_player);
        m_currentUnitSceneEvent.SetPlayerData(assistCreature, 1);
    }

    protected virtual void OnTransportTweenFinish()
    {
        modulePVE.DispathTransportSceneToUI(false);
        if (!m_currentUnitSceneEvent.validUnitSceneData)
        {
            Logger.LogError("load scene is error, will be failed with stage info : scene event id = [{0}],levelMap is [{1}]",
                m_currentUnitSceneEvent.eventId, m_currentUnitSceneEvent.levelMap);

            OnStageFail(null);
            Game.GoHome();
        }

        AssistStartAI();
    }

#endregion

    protected virtual void OnTransportScene(STransportSceneBehaviour behaviour)
    {
        NeedAutoGotoTransport = false;
        HandleTransportScene(behaviour);
    }

    protected virtual void HandleTransportScene(STransportSceneBehaviour behaviour)
    {
        Logger.LogDetail("handle STransportSceneBehaviour:{0}", behaviour.ToString());
		
#if PVE_EVENT_LOG
        System.Text.StringBuilder s = new System.Text.StringBuilder();
        s.AppendLine("HandleTransportScene");
        foreach (var item in m_unitSceneEvents)
        {
            s.AppendLine(Util.Format("unitSceneEvent :eventid = {0},levelId = {1},levelMap = {2} hasloaded = {3} hasdestoryed = {4} mutexId = {5}", item.eventId, item.levelId, item.levelMap, item.hasLoadedScene, item.hasDestoryed, item.mutexUnitEventId));
        }
        Logger.LogDetail(s.ToString());
#endif

        modulePVEEvent.ClearAll();
        moduleAI.CloseAI();
        moduleBattle.parseInput = false;
        if (eventPlayer && eventPlayer.health > 0)
        {
            eventPlayer.moveState = 0;
            eventPlayer.stateMachine?.TranslateTo(behaviour.state);
        }
        modulePVE.DispathTransportSceneToUI(true);
        m_beginTransportDelayId = DelayEvents.Add(() => OnTransportDark(behaviour), behaviour.delayTime * 0.001f);
    }

    private void OnTransportDark(STransportSceneBehaviour behaviour)
    {
        UnitSceneEventData data = m_unitSceneEvents.Find(o => o.eventId == behaviour.eventId);
        if (data == null)
        {
            Logger.LogError("cannot change to unit event ,level id = {0},we will force stage false", behaviour.levelId);
            OnStageFail(null);
            moduleGlobal.SetPVELockState(string.Empty, 0, 0.1f);
            return;
        }
        if (data.hasDestoryed) Logger.LogError("change to a scene which has beed destoried");
        Module_Story.DestoryStory();
        moduleStory.SetCameraTransToGlobal();
        
        //卸载互斥数据,提前卸载，避免内存峰值过高
        if (data.mutexUnitEventId > 0)
        {
            UnitSceneEventData mutexData = m_unitSceneEvents.Find(o => o.eventId == data.mutexUnitEventId);
            if(mutexData != null)
            {
                mutexData.Unload();
                m_unitSceneEvents.Remove(mutexData);
                Logger.LogDetail("unload mutex unitSceneEvent :eventid = {0},levelId = {1},levelMap = {2}", mutexData.eventId, mutexData.levelId, mutexData.levelMap);
            }
        }

        //创建即将打开的节点对应的子节点信息（如果即将打开的节点没有预加载的话，这步操作可以保证该节点以及该节点的子节点场景都预加载）
        CreateChildUnitDatas(data);

        int showState = data == null || data.isActiveScene ? 1 : 2;
        moduleGlobal.SetPVELockState(showState == 1 ? string.Empty : ConfigText.GetDefalutString(0, 0), showState, (float)CombatConfig.spveTransportInTime);
        m_transportDarkId = DelayEvents.Add(() =>
        {
            //load assets
            if (!data.isActiveScene) DownloadNextScene(behaviour);
            else ChangeToNewScene(behaviour);
        }, (float)CombatConfig.spveTransportInTime);
    }

    private void DownloadNextScene(STransportSceneBehaviour behaviour)
    {
        StartCoroutine(_DownloadNextScene(behaviour));
    }

    private IEnumerator _DownloadNextScene(STransportSceneBehaviour behaviour)
    {
        DispatchEvent(Events.SCENE_LOAD_PROGRESS, Event_.Pop(0f));
        int finishCount = 0;

        List<string> allAssets = new List<string>();
        List<UnitSceneEventData> l = new List<UnitSceneEventData>();
        foreach (var item in m_unitSceneEvents)
        {
            if (item.hasLoadedScene) continue;

            l.Add(item);
            allAssets.AddRange(item.allAssets);
            yield return item.LoadSceneAsync();
            finishCount++;
            float process = 0.3f + (finishCount * 1.0f / MAX_LOADED_UNIT_SCENE_COUNT) * 0.4f;
            DispatchEvent(Events.SCENE_LOAD_PROGRESS, Event_.Pop(process));
            if (finishCount == MAX_LOADED_UNIT_SCENE_COUNT) break;
        }

        DispatchEvent(Events.SCENE_LOAD_PROGRESS, Event_.Pop(0.7f));
        allAssets.AddRange(OnBuildPVEAssets());
        allAssets.AddRange(Module_Battle.BuildPlayerPreloadAssets());
        OnBuildAssistantAssets(allAssets);

        // Remove loaded assets to decrease memory usage
        current.UnloadAssets(allAssets);

        // Release memory usage
        System.GC.Collect();
        Resources.UnloadUnusedAssets();

        
        yield return _PrepareAssets(allAssets);

        DispatchEvent(Events.SCENE_LOAD_PROGRESS, Event_.Pop(0.9f));
        foreach (var item in l)
        {
            yield return item.CreateMonsterAsyn();
        }
        OnLoadAssetsOverOnTransport();
        ChangeToNewScene(behaviour);
    }

    protected virtual void OnLoadAssetsOverOnTransport()
    {
        DispatchEvent(Events.SCENE_LOAD_PROGRESS, Event_.Pop(1.0f));
    }
    
    protected virtual bool WaitBeforeChangeToNewScene() { return true; }

    private void ChangeToNewScene(STransportSceneBehaviour behaviour)
    {
        //Window.ShowAsync<Window_Combat>();
        moduleGlobal.SetPVELockState(string.Empty, 0, (float)CombatConfig.spveTransportOutTime);
        StartCoroutine(ActiveUnitSceneEvent(behaviour.eventId));
    }
    #endregion

    #region trigger
    private void OnCreateTrigger(SCreateTriggerBehaviour behaviour)
    {
        PVETriggerCollider c = m_triggerColliders.Find(o => o.triggerId == behaviour.triggerId);
        if (c)
        {
            
            c.ClearTriggerCreatures();
            if (!behaviour.isRandom)
            {
                Logger.LogWarning("Repeat create the trigger with id = {0},will reset range with {1}", behaviour.triggerId, behaviour.range);
                c.SetRange(behaviour.range);
            }
            else
            {
                Vector4_ range = Vector4_.zero;
                range.x = (new PseudoRandom()).Range(behaviour.range.x, behaviour.range.y);
                range.y = 0;
                range.w = behaviour.range.w;
                range.z = behaviour.range.z;
                c.ReActive(behaviour, range);
            }
        }
        else
        {
            c = PVETriggerCollider.Create(behaviour);
            m_triggerColliders.Add(c);
        }

        if (behaviour.triggerFlag == SCreateTriggerBehaviour.TriggerFlag.TransportScene && !m_transportColliders.Contains(c)) m_transportColliders.Add(c);
    }

    private void OnOperateTrigger(SOperateTriggerBehaviour behaviour)
    {
        PVETriggerCollider c = m_triggerColliders.Find(o => o.triggerId == behaviour.triggerId);
        if (c) c.SetState(behaviour.state);
        else Logger.LogError("cannot find a trigger with id {0}",behaviour.triggerId);
    }

    private void UpdateTransportColliderTip()
    {
        if (m_transportColliders == null || m_transportColliders.Count == 0 || !mainCamera)
        {
            modulePVE.DispathTransportTriggerTip(0);
            return;
        }

        int tip = 0;

        for (int i = 0; i < m_transportColliders.Count; i++)
        {
            if (m_transportColliders[i].state == PVETriggerCollider.EnumTriggerState.Close) continue;

            Vector3 v = mainCamera.WorldToScreenPoint(m_transportColliders[i].position);
            if (v.x < 0) tip |= (int)EnumMonsterTipArrow.MonserLeft;
            else if (v.x > Screen.width) tip |= (int)EnumMonsterTipArrow.MonsterRight;
        }
        modulePVE.DispathTransportTriggerTip(tip);
    }

    protected virtual void AutoMoveToTransport()
    {
        if (InvalidAutoMoveToTransport()) return;

        for (int i = 0; i < m_transportColliders.Count; i++)
        {
            if (m_transportColliders[i].state == PVETriggerCollider.EnumTriggerState.Close) continue;

            if (m_transportColliders[i].position.x > player.position_.x) player.moveState = 1;
           else if (m_transportColliders[i].position.x < player.position_.x) player.moveState = -1;
        }
    }

    protected virtual bool InvalidAutoMoveToTransport()
    {
        var invalid = !NeedAutoGotoTransport || m_transportColliders == null || m_transportColliders.Count == 0 || !player || player.health == 0 || player.moveState != 0;
        if (modulePVE.useTeamAutoBattle) return invalid;
        else return modulePVE.isTeamMode || invalid;
    }
#endregion

    #region scene actor
    private void OnCreateSceneActor(SCreateSceneActorBehaviour b)
    {
        MonsterInfo info = ConfigManager.Get<MonsterInfo>(b.sceneActorId);
        if (info)
        {
            MonsterCreature actor = GetSceneActorFromPool(b.logicId);
            if (!actor) return;

            actor.isBoss = false;
            actor.enabledAndVisible = true;
            actor.invincibleCount = int.MaxValue;
            actor.behaviour.hitCollider.isTrigger = true;
            actor.checkEdge = false;
            actor.SetHealthBarVisible(false);
            actor.forceDirection = b.forceDirection;
            actor.RemoveEventListener(CreatureEvents.DEAD_ANIMATION_END);
            actor.RemoveEventListener(CreatureEvents.ENTER_STATE);
            //must add this callback before change monster state
            actor.AddEventListener(CreatureEvents.ENTER_STATE, OnSceneActorEnterState);
            //actor.AddEventListener(CreatureEvents.DEAD_ANIMATION_END, OnSceneActorRealDeath);
            actor.position_ = Vector3_.right * b.reletivePos;
            if (!string.IsNullOrEmpty(b.state))
            {
                actor.stateMachine?.TranslateTo(b.state);
            }
            else if (!string.IsNullOrEmpty(info.bornStateName))
            {
                actor.stateMachine?.TranslateTo(info.bornStateName);
            }
            
        }
    }

    private MonsterCreature GetSceneActorFromPool(int logicId)
    {
        MonsterCreature creature = m_sceneActorPool.Find(o=>o.roleId == (ulong)logicId);
        if (!creature) Logger.LogError("get scene actor from pool error,scene actor logic id is {0}",logicId);

        return creature;
    }

    private void OnSceneActorEnterState(Event_ e)
    {
        MonsterCreature mon = e.sender as MonsterCreature;
        var oldState = e.param1 as StateMachineState;
        var newState = e.param2 as StateMachineState;
        if (!mon) return;

        CreateSceneActorCondition(mon.roleId, oldState, true);
        CreateSceneActorCondition(mon.roleId, newState, false);
    }
    
    /// <summary>
    /// 创建scene actor
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="state"></param>
    /// <param name="isOld"></param>
    private void CreateSceneActorCondition(ulong roleId, StateMachineState state,bool isOld)
    {
        if (!state) return;

        SSceneActorStateConditon c = new SSceneActorStateConditon((int)roleId,state.name,isOld ? EnumActorStateType.ExitState : EnumActorStateType.EnterState);
        modulePVEEvent.AddCondition(c);
    }

    /// <summary>
    /// 操作scene actor
    /// </summary>
    /// <param name="b"></param>
    private void OnOperateSceneActor(SOperateSceneActorBehaviour b)
    {
        foreach (var item in m_sceneActorPool)
        {
            if(item.roleId == (ulong)b.logicId)
            {
                item.stateMachine?.TranslateTo(b.state);
            }
        }
    }

    /// <summary>
    /// 移除SceneActor
    /// </summary>
    /// <param name="be"></param>
    private void OnDeleteSceneActor(SDelSceneActorEventBehaviour b)
    {
        if (b == null)
            return;
        foreach (var item in m_sceneActorPool)
        {
            if (item.isMonster && item.monsterId == b.sceneActorId &&  item.roleId == (ulong)b.logicId)
            {
                item.Kill();
                //item.enabledAndVisible = false;
                break;
            }
        }
    }
    #endregion

    #region create little

    private void OnCreateLittle(SCreateLittleBehaviour behaviour)
    {
        MonsterInfo info = ConfigManager.Get<MonsterInfo>(behaviour.monsterId);
        if (info)
        {
            MonsterCreature monster = MonsterCreature.CreateMonster(behaviour.monsterId, behaviour.group, behaviour.level, new Vector3_(behaviour.createPosX,0,0), new Vector3(0, 90f, 0), Module_PVE.instance?.currrentStage);
            if (!monster) return;

            monster.enabledAndVisible = false;
            Logger.LogInfo("OnCreateLittle success ,little monster [{0}-{1}] have been created.", monster.monsterId, monster.uiName);
            if (currentRoot) monster.transform.SetParent(currentRoot);
            monster.isBoss = false;
            if (monster.isDestructible) monster.SetHealthBarVisible(false);

            //添加AI
            moduleAI.AddMonsterAI(monster);
            //添加阵营 
            moduleAI.AddCreatureToCampDic(monster);
            //切换当前怪物的锁敌信息
            moduleAI.ChangeLockEnermy(monster, true);

            //添加锁敌信息后才能开始处理怪物的出生播放的状态
            if (!string.IsNullOrEmpty(info.bornStateName))
            {
                //Logger.LogDetail("{0} 播放出生动画 {1}",monster.gameObject.name,info.bornStateName);
                monster.stateMachine.TranslateTo(info.bornStateName);
                moduleAI.ChangeTurn(monster);
            }

            //添加初始化BUFF
            if (info.initBuffs != null && info.initBuffs.Length > 0)
            {
                for (int i = 0; i < info.initBuffs.Length; i++)
                {
                    //Logger.LogDetail("{0} 添加初始化BuffId = {1}", monster.gameObject.name, info.initBuffs[i]);
                    Buff.Create(info.initBuffs[i], monster);
                }
            }

            monster.AddEventListener(CreatureEvents.DEAD_ANIMATION_END,         OnMonsterRealDeath);
            monster.AddEventListener(CreatureEvents.HEALTH_CHANGED,             OnMonsterHealthChange);
            monster.AddEventListener(CreatureEvents.ATTACK,                     OnMonsterHitCreature);
            monster.AddEventListener(CreatureEvents.BUFF_TRIGGER,               OnMonsterBuffTrigger);

            moduleAI.LogAIMsg(monster, true, "[level -> {0}] [boss -> {1}] [stateMachine -> {2}] [AI -> {3}]", monster.leech, monster.isBoss, Creature.GetAnimatorName(monster.monsterInfo.montserStateMachineId, 1), monster.monsterInfo.AIId);
            moduleAI.LogAIMsg(monster, true, "[AddEventListener ENTER_STATE]");

            List<MonsterCreature> monsterList = null;
            if (m_groupMonsterDic.TryGetValue(behaviour.group, out monsterList))
            {
                monsterList.Add(monster);
            }
            else
            {
                monsterList = new List<MonsterCreature>();
                monsterList.Add(monster);
                m_groupMonsterDic.Add(behaviour.group, monsterList);
            }

            if (monster.creatureCamp != CreatureCamp.PlayerCamp && !monster.isDestructible && CanInitRight())
            {
                Event_ initMonster = Event_.Pop(monster);
                DispatchEvent(EventInitPVERightAvatar, initMonster);
            }
            monster.enabledAndVisible = true;
        }
    }

    #endregion

    #region movemonster pos

    private void OnMoveMonsterPos(SMoveMonsterPosBehaviour be)
    {
        var x = Mathd.Clamp(be.reletivePosX, current.edge.x, current.edge.y);

        //如果从帧事件直接注册
        if(be.creature)
        {
            if (be.creature.health > 0)
            {
                var pos = be.creature.position_;
                be.creature.position_ = new Vector3_(x, be.reletivePosY != 0 ? be.reletivePosY : pos.y, pos.z);
            }
            return;
        }

        //通过配置ID的方式执行
        var monsters = GetTargetGroupMonsters(be.group, be.monsterId);
        foreach (var item in monsters)
        {
            if (!item || item.health == 0 || !item.activeRootNode || !item.activeRootNode.gameObject.activeInHierarchy) continue;

            item.moveState = 0;
            var pos = item.position_;
            item.position_ = new Vector3_(x, be.reletivePosY != 0 ? be.reletivePosY : pos.y, pos.z);
        }
    }

    #endregion

    #region guide

    protected virtual void OnStartPVEGuide(SStartGuideBehaviour be)
    {
        if (modulePVE.isTeamMode) return;

        //变身引导触发的时候，强制将玩家的能量填充满
        if (be.guideId == Module_Guide.AWAKE_MORPH_GUIDE_ID) player.energy = player.maxEnergy;
    }
    #endregion

    #region operate scene area
    
    private void OnOperateSceneArea(SOperateSceneAreaBehaviour b)
    {
        var e = Vector3_.zero;
        if (b.useAbsoluteValue) e = OperateSceneAreaAbsolute(b.direction,b.absoluteValue);
        else e = OperateSceneAreaAdditive(b.direction, b.addtiveValue);

        if (e != Vector3_.zero)
        {
            e.x = Mathd.Clamp(e.x, m_originalEdge.x, m_originalEdge.y);
            e.y = Mathd.Clamp(e.y, m_originalEdge.x, m_originalEdge.y);
            SetEdge(e.x, e.y);
        }
    }

    private Vector3_ OperateSceneAreaAdditive(SOperateSceneAreaBehaviour.EnumAreaDirction dir,int value)
    {
        var e = edge;
        if (dir == SOperateSceneAreaBehaviour.EnumAreaDirction.Left) e.x += value;
        else if (dir == SOperateSceneAreaBehaviour.EnumAreaDirction.Right) e.y += value;
        return e;
    }

    private Vector3_ OperateSceneAreaAbsolute(SOperateSceneAreaBehaviour.EnumAreaDirction dir, int absoluteValue)
    {
        var e = edge;
        if (dir == SOperateSceneAreaBehaviour.EnumAreaDirction.Left) e.x = absoluteValue;
        else if (dir == SOperateSceneAreaBehaviour.EnumAreaDirction.Right) e.y = absoluteValue;
        return e;
    }

    #endregion

    #region create assistant

    public void OnCreateAssistMember(SCreateAssistantBehaviour be)
    {
        if (be == null) return;

        OnCreateAssistMember((ulong)be.logicId, be.reletivePos, be.bornState);
    }

    public void OnCreateAssistNpc(SCreateAssistantNpcBehaviour be)
    {
        if (be == null) return;

        OnCreateAssistMember((ulong)be.logicId, be.reletivePos, be.bornState, true);
    }

    public void CreateAssistMember()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (m_currentUnitSceneEvent == null) return;

        SceneEventInfo.SceneBehaviour behaviour = null;
        foreach (var item in m_currentUnitSceneEvent.sceneEventInfo.sceneEvents)
        {
            foreach (var b in item.behaviours)
            {
                if(b.sceneBehaviorType == SceneEventInfo.SceneBehaviouType.CreateAssistant)
                {
                    behaviour = b;
                    break;
                }
            }
        }

        if (behaviour != null) OnCreateAssistMember(new SCreateAssistantBehaviour(behaviour,null));
        else OnCreateAssistMember((ulong)MonsterCreature.GetMonsterRoomIndex(), player.position.x - 1, string.Empty);
#endif
    }

    /// <summary>
    /// 创建助战玩家
    /// </summary>
    /// <param name="logicId">覆盖roleId数据，保证策划可以通过配置来控制该角色</param>
    /// <param name="reletivePos">相对于0点的绝对位置</param>
    /// <param name="bornState">出生状态</param>
    /// <param name="isNpc">只创建助战Npc</param>
    public void OnCreateAssistMember(ulong logicId, double reletivePos, string bornState, bool isNpc = false)
    {
        if (modulePVE.assistMemberInfo == null) return;

        if (isNpc)
        {
            if (modulePVE.assistMemberInfo.type != 1)
                return;
        }
        else if (modulePVE.assistMemberInfo.type == 1)
            return;

        if (m_assistCreature)
        {
            Logger.LogWarning("Repeat the creation of a combat assistant creature");
            return;
        }

        var pos = Vector3_.right * reletivePos;
        if (modulePVE.assistMemberInfo.type == 1)
        {
            Logger.LogInfo("助战Npc:{0}", modulePVE.assistMemberInfo.npcId);
            var npcInfo = ConfigManager.Get<NpcInfo>(modulePVE.assistMemberInfo.npcId);
            if (!npcInfo)
            {
                Logger.LogError("create assistant creature failed, npcInfo is null ,npcId = {0}", modulePVE.assistMemberInfo.npcId);
                return;
            }

            m_assistCreature = MonsterCreature.CreateMonster(npcInfo.monsterId, -2, modulePlayer.level, pos, new Vector3(0, 90, 0), null, "", "", false);
            var npc = moduleNpc.GetTargetNpc((NpcTypeID)modulePVE.assistMemberInfo.npcId);
            if (npc != null)
                CharacterEquip.ChangeNpcFashion(m_assistCreature, npc.mode);
        }
        else
        {
            Logger.LogInfo("助战玩家:{0}", modulePVE.assistMemberInfo.memberInfo.roleName);

            m_assistCreature = CreateTeamPlayer(modulePVE.assistMemberInfo.memberInfo, pos, 2);
        }

        if (!m_assistCreature) return;

        m_assistCreature.roleId = logicId;
        m_assistCreature.SetCreatureCamp(CreatureCamp.PlayerCamp);
        //注册listener
        m_assistCreature.RemoveEventListener(CreatureEvents.SHOOTED, OnAssistCreatureWasAttacked);
        m_assistCreature.RemoveEventListener(CreatureEvents.ATTACKED, OnAssistCreatureWasAttacked);
        m_assistCreature.AddEventListener(CreatureEvents.SHOOTED, OnAssistCreatureWasAttacked);
        m_assistCreature.AddEventListener(CreatureEvents.ATTACKED, OnAssistCreatureWasAttacked);
        //注册listener end
        AssistStartAI();
        AssistBornState(bornState);

        DispatchEvent(LevelEvents.CREATE_ASSIST, Event_.Pop(m_assistCreature));
    }

    /// <summary>
    /// on assist creature was attacked
    /// </summary>
    /// <param name="e"></param>
    private void OnAssistCreatureWasAttacked(Event_ e)
    {
        if (m_assistCreature != null && modulePVEEvent != null)
        {
            modulePVEEvent.AddCondition(new SHitTimesConditon(1, (int)m_assistCreature.roleId));
        }
    }

    private void AssistStartAI()
    {
        if (!m_assistCreature) return;

        moduleAI.AddCreatureToCampDic(m_assistCreature);
        m_assistCreature.enableUpdate = true;
        if (m_assistCreature is MonsterCreature) moduleAI.AddMonsterAI(m_assistCreature as MonsterCreature);
        else
        {
            moduleAI.AddPlayerAI(m_assistCreature);
            m_assistCreature.teamIndex = 1;
            m_assistCreature.useAI = true;
        }

        if (m_assistCreature.pet != null)
        {
            m_assistCreature.pet.enableUpdate = true;
            moduleAI.AddPetAI(m_assistCreature.pet);
        }
    }

    private void AssistBornState(string state)
    {
        if (string.IsNullOrEmpty(state) || !m_assistCreature || !m_assistCreature.stateMachine) return;

        m_assistCreature.stateMachine.TranslateTo(state);
    }

    #endregion

    #endregion

    #region 迷宫BUFF

    /// <summary>
    /// 刷新所有的buff.
    /// </summary>
    private void RefreshAllBuff()
    {
        if (modulePVE.labyrinthBuffPropIds == null || modulePVE.labyrinthBuffPropIds.Count == 0)
            return;

        for (int i = 0; i < modulePVE.labyrinthBuffPropIds.Count; i++)
        {
            PropToBuffInfo info = modulePVE.propToBuffDic.Get(modulePVE.labyrinthBuffPropIds[i]);
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

#endregion

    #region other functions

    /// <summary>
    /// 强制恢复当前场景所有的怪物状态
    /// 1.强制切换到statestand,2.y轴变到0
    /// </summary>
    public void ForceResetAllMonsters()
    {
        moduleAI.SetAllAIPauseState(!moduleAI.pauseAI);

        if (!moduleAI.pauseAI)
            return;

        MonsterCreature mon = null;
        foreach (var item in m_groupMonsterDic)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                mon = item.Value[i];
                if (mon.isDead || mon.health == 0) continue;

                mon.stateMachine.TranslateTo(Module_AI.STATE_STAND_NAME);
            }
        }
    }

    private void UpdateMonsterTip()
    {
        m_monsterTip = EnumMonsterTipArrow.None;
        foreach (var item in m_groupMonsterDic)
        {
            foreach (var m in item.Value)
            {
                if (!m || !m.gameObject || m.isDead || !m.gameObject.activeSelf || m.health <= 0 || m.lockEnermyType == EnumLockMonsterType.Close) continue;
                Vector3 v = mainCamera.WorldToScreenPoint(m.position);
                if (v.x < 0)
                {
                    if(m.monsterGroup == -2) m_monsterTip |= EnumMonsterTipArrow.NpcLeft;
                    else m_monsterTip |= m.isBoss ? EnumMonsterTipArrow.BossLeft : EnumMonsterTipArrow.MonserLeft;
                }
                else if (v.x > Screen.width)
                {
                    if (m.monsterGroup == -2) m_monsterTip |= EnumMonsterTipArrow.NpcRight;
                    else m_monsterTip |= m.isBoss ? EnumMonsterTipArrow.BossRight : EnumMonsterTipArrow.MonsterRight;
                }
            }
        }
        modulePVE.DispathMonsterTip(m_monsterTip);
    }

    private void OnCameraShotUIState(Event_ e)
    {
        var hide = (bool)e.param1;
        if (hide == m_lastShotState) return;

        m_lastShotState = hide;
        SetAllMonsterHealthSliderVisible(!m_lastShotState);
    }

    private void OnWindowDisplay(Event_ e)
    {
        Window w = e.sender as Window;
        if ((w is Window_Combat || w is Window_PVECombat))
        {
            if (moduleBattle.inDialog) w.Hide(true);

            //如果在做boss动画，就不让玩家能点击
            if (m_animMonster) w.inputState = false;
        }
    }

    private void SetAllMonsterHealthSliderVisible(bool visible)
    {
        foreach (var item in m_groupMonsterDic)
        {
            foreach (var mon in item.Value)
            {
                if (mon.isDead || mon.health <= 0) continue;

                mon.hpVisiableCount += visible ? 1 : -1;
            }
        }
        //未创建出来的怪也需要血条显示计数，否则隐藏过程中创建出来的怪会显示血条
        foreach (var mon in m_creaturePool)
        {
            mon.hpVisiableCount += visible ? 1 : -1;
        }
    }

    private void SetAllMonsterVisible(bool visible)
    {
        foreach (var item in m_groupMonsterDic)
        {
            foreach (var mon in item.Value)
            {
                if (mon.isDead || mon.health <= 0) continue;

                mon.enabledAndVisible = visible;
            }
        }
    }

    public void SetLevelBehaviourCanDestory()
    {
        m_currentUnitSceneEvent?.SetComponentDestoryState(true);
    }

    protected virtual void OnCreatureDead(Event_ e)
    {
        var c = e.sender as Creature;

        if (c.isMonster)
        {
            var monster = c as MonsterCreature;
            //Logger.LogInfo("---------------------------monster.name is {0}   isBoss = {1}", monster.name,monster.isBoss);
            if (monster.isBoss) m_lastDeadBoss = monster;
        }

        if (player.isDead || m_lastDeadBoss != null)
        {
            //Logger.LogDetail("OnPlayerCreature or Boss_Monster Dead......");
            m_pauseTimer = m_lastDeadBoss == null;
            EnterState(BattleStates.SlowMotion);
        }

        if(player.isDead && !modulePVE.isStandalonePVE)  
        {
            SetAllMonsterInvincible(true);
            modulePVEEvent.pauseEvent = true;
        }
    }

    protected void SetAllMonsterInvincible(bool invincible)
    {
        if (invincible == m_currentMonsterInvincible) return;

        //Logger.LogDetail("Set All Monster {0}",invincible ? "Invincible" : "Normal");
        m_currentMonsterInvincible = invincible;
        int delta = invincible ? 1 : -1;
        foreach (var item in m_groupMonsterDic)
        {
            foreach (var mon in item.Value)
            {
                if (mon.isDead || !mon.gameObject || !mon.gameObject.activeInHierarchy) continue;

                mon.invincibleCount += delta;
            }
        }
    }

    protected int GetLiveMonsterCount()
    {
        int count = 0;
        foreach (var item in m_groupMonsterDic)
        {
            foreach (var mon in item.Value)
            {
                if (mon.monsterGroup != -2 && mon.lockEnermyType != EnumLockMonsterType.Close && mon.health > 0 && mon.gameObject && mon.gameObject.activeInHierarchy) count++;
            }
        }
        return count;
    }

    protected virtual void OnCreatureMorphChange(Event_ e)
    {
        var c = e.sender as Creature;
        if (c.isPlayer) modulePVE.SendPVEAwake();
        Logger.LogInfo("OnCreatureMorphChange......");
    }
#endregion

    #region PVE流程

    protected override void OnCreateCreatures()
    {
        base.OnCreateCreatures();
        player.RemoveEventListener(CreatureEvents.ATTACKED, OnPlayerWasAttacked);
        player.RemoveEventListener(CreatureEvents.SHOOTED, OnPlayerWasAttacked);
        player.RemoveEventListener(CreatureEvents.HEALTH_CHANGED, OnPlayerHealthChange);
        player.AddEventListener(CreatureEvents.HEALTH_CHANGED, OnPlayerHealthChange);
        player.AddEventListener(CreatureEvents.ATTACKED, OnPlayerWasAttacked);
        player.AddEventListener(CreatureEvents.SHOOTED, OnPlayerWasAttacked);
        //player.attack *= 100;
        //player.maxHealth *= 100;
        //player.health = player.maxHealth;
        //player.maxEnergy = 100;
        //player.energy = player.maxEnergy;
    }

    private void OnPlayerHealthChange(Event_ e)
    {
        var creature = e.sender as Creature;
        modulePVEEvent.AddCondition(new SPlayerHPLessConditon(Mathf.RoundToInt((float)(creature.healthRateL * 100))));
    }

    private void OnPlayerWasAttacked(Event_ e)
    {
        modulePVEEvent.AddCondition(new SHitTimesConditon(1,-2));
    }

    protected override void OnQuitState(int oldMask, BattleStates state)
    {
        base.OnQuitState(oldMask, state);
        switch (state)
        {
            case BattleStates.Watch:
                InputManager.instance.enabled = true;
                break;
            case BattleStates.SlowMotion:
                //怪物boss死亡
                if (m_lastDeadBoss != null)
                {
                    m_lastDeadBoss = null;

                    //慢镜头结束时，如果还未发送结算消息，则恢复到战斗状态
                    if (!m_isPVEFinish)
                    {
                        //Logger.LogInfo("boss dead slow is over,return to Fighting");
                        EnterState(BattleStates.Fighting);
                    }
                }

                //慢镜头结束，玩家如果死亡，1.没有复活条件 2.复活次数用完 3.有复活条件并且收到了结束消息
                if (player.isDead)
                {
                    if(m_recvOverMsg || !modulePVE.canRevive)
                    { 
                        //Logger.LogInfo("player dead slow is over,return to Losing");
                        EnterState(BattleStates.Losing);
                    }
                }
                //发送结算消息,则根据发送情况进入状态
                else if ((m_isPVEFinish || m_recvOverMsg) && !player.isDead)
                {
                    //Logger.LogInfo("chanllenge over and player alive --- slow is over,return to {0}", modulePVE.isSendWin ? BattleStates.Winning : BattleStates.Losing);
                    EnterState(modulePVE.isSendWin ? BattleStates.Winning : BattleStates.Losing);
                }
                break;
        }
    }

    protected override void OnEnterState(int oldMask, BattleStates state)
    {
        base.OnEnterState(oldMask, state);
        //切换观战状态不影响任何逻辑
        if (state == BattleStates.Watch)
        {
            InputManager.instance.enabled = false;
            return;
        }
        
        if (!moduleBattle.inDialog) moduleAI.SetAllAIPauseState(!IsState(BattleStates.Fighting));
        moduleBattle.parseInput &= m_animMonster == null;

        if (state == BattleStates.Winning || state == BattleStates.Losing)
        {
            modulePVEEvent.pauseCountDown = true;
            SetAllMonsterHealthSliderVisible(false);
            moduleAI.SetAllAIPauseState(true);
            Window.Hide<Window_PVECombat>(true);
            Window.Hide<Window_Combat>(true);
            Window.Hide<Window_HpSlider>(true);
        }
        else if (state == BattleStates.Fighting) modulePVEEvent.enableUpdate = true;
    }

    protected override void OnWinningStateUpdate(int diff)
    {
        CheckEndedTimeOut(BattleStates.Winning);
        base.OnWinningStateUpdate(diff);
    }

    protected override void OnLosingStateUpdate(int diff)
    {
        CheckEndedTimeOut(BattleStates.Losing);
        base.OnLosingStateUpdate(diff);
    }

    private void CheckEndedTimeOut(BattleStates state)
    {
        int time = GetStateTimer(state);
        int totalTime = PVE_SETTLEMENT_TIMEOUT * 1000;
        if (time >= totalTime) m_recvOverMsg = true;
    }

    protected override bool WaitEndState()
    {
        if (!m_recvOverMsg) return false;

        m_recvOverMsg = false;
        return true;
    }

    protected override void OnBattleTimerEnd()
    {
        OnStageFail(null);
    }
#endregion

    #region module event

    private void _ME(ModuleEvent<Module_PVE> e)
    {
        switch (e.moduleEvent)
        {
            case Module_PVE.EventGameOverSettlement:
                //Logger.LogInfo("----------Recv EventChaseSettlement Event...............");
                HandleToPVERecvMsg();
                break;

            //复活
            case Module_PVE.EventRebornState:
                bool isSuccess = (bool)e.param1;
                //复活成功并且设置过无敌状态(避免组队模式多次操作)
                if (isSuccess && m_currentMonsterInvincible) SetAllMonsterInvincible(false);

                if (moduleAI.pauseAI)
                    moduleAI.SetAllAIPauseState(false);
                break;

            case Module_PVE.EventRecvBossReward: DelayEvents.Remove(m_bossrewardDelayEventId); break;
        }
    }

    private void _ME(ModuleEvent<Module_Bordlands> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Bordlands.EventBordlandSettlement:
                //Logger.LogInfo("----------Recv EventBordlandSettlement Event...............");
                HandleToPVERecvMsg();
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Labyrinth> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Labyrinth.EventChallengeOver:
                //Logger.LogInfo("----------Recv EventLabyrinthSettlement Event...............");
                HandleToPVERecvMsg();
                break;
        }
    }

    protected void HandleToPVERecvMsg()
    {
        m_recvOverMsg = true;
        if (!IsState(BattleStates.SlowMotion))  // Wait slowmotion state end
            EnterState(modulePVE.isSendWin ? BattleStates.Winning : BattleStates.Losing);
    }

    private void _ME(ModuleEvent<Module_Story> e)
    {
        EnumStoryType type = (EnumStoryType)e.param2;
        switch (e.moduleEvent)
        {
            case Module_Story.EventStoryStart: OnStoryStart(type);  break;
            case Module_Story.EventStoryEnd: OnStoryEnd(type);      break;
        }
    }

    private void OnStoryStart(EnumStoryType type)
    {
        if (type == EnumStoryType.TheatreStory || type == EnumStoryType.PauseBattleStory)
        {
            DispatchEvent(LevelEvents.DIALOG_STATE, Event_.Pop(true));

            moduleBattle.parseInput = false;
            moduleAI.SetAllAIPauseState(true);
            modulePVEEvent.pauseCountDown = true;
            m_pauseTimer = true;
            Window.Hide<Window_Combat>();
            Window.Hide<Window_PVECombat>();
            SetAllMonsterHealthSliderVisible(false);

            if (player) player.moveState = 0;
        }
    }

    private void OnStoryEnd(EnumStoryType type)
    {
        if (type == EnumStoryType.TheatreStory || type == EnumStoryType.PauseBattleStory)
        {
            DispatchEvent(LevelEvents.DIALOG_STATE, Event_.Pop(false));

            moduleBattle.parseInput = true;
            //如果玩家血量小于0 ，并且对话结束的时候，直接暂停AI
            moduleAI.SetAllAIPauseState(player.health <= 0);
            modulePVEEvent.pauseCountDown = false;
            m_pauseTimer = false;
            Window.ShowAsync<Window_Combat>();
            Window.ShowAsync<Window_PVECombat>();
            SetAllMonsterHealthSliderVisible(true);
        }
    }

    protected void _ME(ModuleEvent<Module_PVEEvent> e)
    {
        switch (e.moduleEvent)
        {
            case Module_PVEEvent.EventCreateMonster:        OnCreateMonster(e.param1 as SCreateMonsterBehaviour);           break;
            case Module_PVEEvent.EventKillMonster:          OnKillMonster(e.param1 as SKillMonsterBehaviour);               break;
            case Module_PVEEvent.EventAddBuff:              OnAddBuff(e.param1 as SAddBufferBehaviour);                     break;
            case Module_PVEEvent.EventStageClear:           OnStageClear(e.param1 as SStageClearBehaviour);                 break;
            case Module_PVEEvent.EventStageFail:            OnStageFail(e.param1 as SStageFailBehaviour);                   break;
            case Module_PVEEvent.EventLeaveMonster:         OnCreatureLeave(e.param1 as SLeaveMonsterBehaviour);            break;
            case Module_PVEEvent.EventSetState:             OnMonsterSetState(e.param1 as SSetStateBehaviour);              break;
            case Module_PVEEvent.EventTransportScene:       OnTransportScene(e.param1 as STransportSceneBehaviour);         break;
            case Module_PVEEvent.EventCreateTrigger:        OnCreateTrigger(e.param1 as SCreateTriggerBehaviour);           break;
            case Module_PVEEvent.EventOperateTrigger:       OnOperateTrigger(e.param1 as SOperateTriggerBehaviour);         break;
            case Module_PVEEvent.EventOperateSceneArea:     OnOperateSceneArea(e.param1 as SOperateSceneAreaBehaviour);     break;
            case Module_PVEEvent.EventCreateSceneActor:     OnCreateSceneActor(e.param1 as SCreateSceneActorBehaviour);     break;
            case Module_PVEEvent.EventOperateSceneActor:    OnOperateSceneActor(e.param1 as SOperateSceneActorBehaviour);   break;
            case Module_PVEEvent.EventCreateLittle:         OnCreateLittle(e.param1 as SCreateLittleBehaviour);             break;
            case Module_PVEEvent.EventStartPVEGuide:        OnStartPVEGuide(e.param1 as SStartGuideBehaviour);              break;
            case Module_PVEEvent.EventMoveMonsterPos:       OnMoveMonsterPos(e.param1 as SMoveMonsterPosBehaviour);         break;
            case Module_PVEEvent.EventCreateAssistant:      OnCreateAssistMember(e.param1 as SCreateAssistantBehaviour);    break;
            case Module_PVEEvent.EventCreateAssistantNpc:   OnCreateAssistNpc(e.param1 as SCreateAssistantNpcBehaviour);    break;
            case Module_PVEEvent.EventDelSceneActorEvent:   OnDeleteSceneActor(e.param1 as SDelSceneActorEventBehaviour);   break;
        }
    }
#endregion

    #region frame event

    private Dictionary<Module_Battle.FrameAction, System.Action<Module_Battle.FrameData>> m_frameCallbackAction = new Dictionary<Module_Battle.FrameAction, System.Action<Module_Battle.FrameData>>();

    public void AddFrameAction(Module_Battle.FrameAction action, System.Action<Module_Battle.FrameData> callback)
    {
        if (!m_frameCallbackAction.ContainsKey(action)) m_frameCallbackAction.Add(action, null);
        m_frameCallbackAction[action] = callback;
    }

    public void HandleAction()
    {
        if (moduleBattle.frameActionData == null || moduleBattle.frameActionData.action == Module_Battle.FrameAction.None) return;

        Module_AI.LogBattleMsg(player,"handle frame action {0}", moduleBattle.frameActionData.action);
        var callback = m_frameCallbackAction.Get(moduleBattle.frameActionData.action);
        callback?.Invoke(moduleBattle.frameActionData);

        moduleBattle.ResetFrameAction();
    }

#endregion

}