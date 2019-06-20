using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using PreloadAudioHolder = Level.PreloadAudioHolder;

/*
 * 场景分事件逻辑采用二叉树的结构，最大峰值为4个场景，最大加载为3个场景（初始化时），平时一般加载最多两个场景（因为游戏只有x，y平面）。
 * 存储容器：List<UnitSceneEventData>  只存储当前结点和当前节点的子节点的分场景
 * 流程：
 * 1.init (1.构建list，2.预加载场景（此时最多预加载三个）)
 * 
 * 2.begin transport
 * 2.1: 找到即将跳转的分场景 S1  
 * 2.2: 卸载与S1互斥的场景  
 * 2.3: 以S1为节点，构建S1的子场景信息（此时存储链表中就有当前节点，S1节点和S1的所有子节点））
 * 2.4: 若S1节点已经预加载，则直接跳转到S1(此时内存峰值低，只有S1场景是加载激活状态) ，若S1子节点未预加载，则S1及其子节点都进行预加载(此时峰值最高，因为当前场景节点还未释放，所有最多会有4个场景)
 * 
 * 3.transport finish 加载完成后，将人物特效动画等资源转移到当前场景，释放掉旧场景，激活当前场景
 */

/// <summary>
/// 对应每个PVE分场景里面的信息汇总
/// 每一个分场景事件组是绝对不可能使用同一个场景，即levelinfo.id绝对不同
/// </summary>
public class UnitSceneEventData
{
    #region tag
    public const string MainCameraTag = "MainCamera";
    public const string Untag = "Untagged";
    #endregion

    #region constructor

    public UnitSceneEventData(STransportSceneBehaviour behaviour)
    {
        levelId = behaviour.levelId;
        position = behaviour.position;
        eventId = behaviour.eventId;
        state = behaviour.state;
        defalutUnit = false;
        RefreshAfterSetConfigData();
    }

    public UnitSceneEventData(StageInfo info)
    {
        levelId = info.levelInfoId;
        position = info.playerPos;
        eventId = Module_PVE.TEST_SCENE_EVENT_ID > 0 ? Module_PVE.TEST_SCENE_EVENT_ID : info.sceneEventId;
        state = Module_AI.STATE_STAND_NAME;
        defalutUnit = true;
        RefreshAfterSetConfigData();
    }
    #endregion

    #region  property
    //config data
    public int levelId { get; private set; }
    public string levelMap { get; private set; }
    public double position { get; private set; }
    public int eventId { get; private set; }
    public SceneEventInfo sceneEventInfo { get; private set; }
    public string state { get; private set; }

    /// <summary>
    /// 互斥的场景事件ID
    /// 因为场景事件和UnitSceneEventData是一对一映射的，所以可以将此直接用作UnitSceneEventData.Id
    /// </summary>
    public int mutexUnitEventId { get; set; } 

    //asset
    private List<string> m_allAssets;
    public List<string> allAssets
    {
        get
        {
            if (sceneEventInfo == null)
            {
                m_allAssets = new List<string>();
                Logger.LogError("To UnitSceneEventData,sceneEventInfo is null eventId = {0}", eventId);
            }
            else if (m_allAssets == null || m_allAssets.Count == 0)
            {
                m_allAssets = sceneEventInfo.GetAllAssets();
                m_allAssets.Distinct();
            }
            return m_allAssets;
        }
    }
    public List<MonsterCreature> creaturePool { get; private set; } = new List<MonsterCreature>();
    public List<MonsterCreature> sceneActorPool { get; private set; } = new List<MonsterCreature>();

    public Scene sceneMap { get; set; }
    public Transform sceneRoot { get; private set; }
    public LevelBehavior levelBehaviour { get; private set; }
    public Camera camera { get; private set; }
    public Camera_Combat cameraCombat { get; private set; }

    private Transform m_poolTrans;
    public Transform poolTrans
    {
        get
        {
            if (m_poolTrans == null) m_poolTrans = sceneRoot?.Find("pool");
            return m_poolTrans;
        }
    }

    //other
    public bool defalutUnit { get; private set; }
    public bool hasLoadedScene { get; private set; }
    public bool hasDestoryed { get; private set; }

    public bool isActiveScene { get { return hasLoadedScene && !hasDestoryed; } }

    /// <summary>
    /// 必须要这三个组件都是有效的，场景才是合法的
    /// </summary>
    public bool validUnitSceneData { get { return sceneRoot && camera && cameraCombat; } }

    public List<PreloadAudioHolder> audioHolders { get; private set; } = new List<PreloadAudioHolder>();
    public List<string> audioAssets { get; private set; } = new List<string>();

    #endregion

    #region functions
    public void RefreshAfterSetConfigData()
    {
        var levelInfo = ConfigManager.Get<LevelInfo>(levelId);
        if (!levelInfo)
        {
            Logger.LogError("level_id = {0} cannot be finded,please check out config", levelId);
            return;
        }

        levelMap = levelInfo.SelectMap();
        sceneEventInfo = ConfigManager.Get<SceneEventInfo>(eventId);
        if (!sceneEventInfo)
        {
            Logger.LogError("sceneEventInfo Id = {0} cannot be finded,please check out config", eventId);
            return;
        }
    }

    public IEnumerator LoadSceneAsync()
    {
        if (string.IsNullOrEmpty(levelMap)) yield break;

        AsyncOperation m_async = SceneManager.LoadSceneAsync(levelMap,LoadSceneMode.Additive);
        while (!m_async.isDone)
        {
            //DispatchEvent(Events.SCENE_LOAD_PROGRESS, Event_.Pop(m_async.progress * 0.39f));
            yield return null;
        }
        
        InitUnitComponent();
        InactiveAudioListner();
        SetSceneActive(false);
        PreloadAudioAssets();
    }

    public void InitUnitComponent()
    {
        hasLoadedScene = true;
        hasDestoryed = false;
        if (string.IsNullOrEmpty(levelMap)) return;

        //获取最近一次加载的scene,因为场景每次只有加载完成才会执行该方法，所以，每次加载完场景后的初始化总能保证是取到最新的关卡
        sceneMap = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

        //进行二次验证,保证场景的正确性
        if(!sceneMap.isLoaded || !sceneMap.name.Equals(levelMap))
        {
            Logger.LogWarning("Into second verification, the level map that we want is {0}", levelMap);
            
            //用循环保证获取到最近一次加载到的同名场景，避免策划配置一些稀奇古怪的东西,
            //使用SceneManager.GetSceneByName(levelMap)这个api不能保证获取到正确的策划配置的同名场景
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if(s.name.Equals(levelMap))
                {
                    sceneMap = s;
                    break;
                }
            }
            
        }

        InitComponent(sceneMap);
    }

    private void InitComponent(Scene scene)
    {
        sceneMap = scene;
        if(!sceneMap.isLoaded)
        {
            Logger.LogError("init scene is null,please check out,level map is {0}",levelMap);
            return;
        }

        //initial component
        var roots = sceneMap.GetRootGameObjects();

        var failed = true;
        foreach (var root in roots)
        {
            camera = root?.GetComponent<Transform>("cameras")?.GetComponentInChildren<Camera>();

            if (!camera) continue;
            failed = false;

            sceneRoot = root.transform;

            cameraCombat = camera.GetComponentDefault<Camera_Combat>();
            cameraCombat.transform.name = scene.name +"_main_camera";

            m_poolTrans = sceneRoot.Find("pool");
            if (!m_poolTrans)
            {
                m_poolTrans = new GameObject("pool").transform;
                m_poolTrans.SetParent(sceneRoot);
            }

            break;
        }

        if (failed) Logger.LogError("level map  = {0} cannot find a valid root obj", levelMap);
        else SetComponentDestoryState(false);
    }

    /// <summary>
    /// 设置场景脚本销毁时是否可以销毁Level管理类 
    /// </summary>
    /// <param name="canDestroy"></param>
    public void SetComponentDestoryState(bool canDestroy)
    {
        if (!levelBehaviour)
        {
            levelBehaviour = sceneRoot.GetComponentDefault<LevelBehavior>();
            levelBehaviour.level = Level.next ?? Level.current;
        }
        levelBehaviour.canDestroyLevel = canDestroy;

        if (cameraCombat) cameraCombat.canDestroyCurrent = canDestroy;
    }

    public void BeforeSceneActive()
    {
        //must set tag before RecreateEnvironments
        camera.tag = MainCameraTag;
        Camera_Combat.current = cameraCombat;
        cameraCombat?._EnableDisableAudioListener(false);
        //we must refresh the level main camera
        Level.current.RecreateEnvironments(sceneMap);
    }
    
    public void SetSceneActive(bool active)
    {
        if(sceneRoot) sceneRoot.gameObject.SetActive(active);

        if(active)
            SceneManager.SetActiveScene(sceneMap);
    }

    private List<string> CollectAudios(List<PreloadAudioHolder> holders = null)
    {
        var audios = new List<string>();

        var ah = sceneRoot.GetComponent<AudioHelper>("audios");
        if (!ah) return audios;

        if (holders == null) audios.Add(ah.audioName);
        else
        {
            ah.enabled = false;
            var idx = audios.IndexOf(ah.audioName);
            var holder = idx > -1 ? holders[idx] : new PreloadAudioHolder() { audioName = ah.audioName, helpers = new List<AudioHelper>(), triggers = new List<AudioBGMTrigger>() };
            holder.helpers.Add(ah);

            if (idx < 0)
            {
                audios.Add(ah.audioName);
                holders.Add(holder);
            }
        }

        var bgms = ah.transform.Find("bgm");
        if (!bgms) return audios;

        for (int i = 0, c = bgms.childCount; i < c; ++i)
        {
            var at = bgms.GetChild(i).GetComponent<AudioBGMTrigger>();
            if (!at) continue;

            if (holders == null) audios.Add(at.audioName);
            else
            {
                at.enabled = false;

                var idx = audios.IndexOf(at.audioName);
                var holder = idx > -1 ? holders[idx] : new PreloadAudioHolder() { audioName = at.audioName, helpers = new List<AudioHelper>(), triggers = new List<AudioBGMTrigger>() };
                holder.triggers.Add(at);

                if (idx < 0)
                {
                    audios.Add(at.audioName);
                    holders.Add(holder);
                }
            }
        }

        if (holders == null) audios.Distinct();

        return audios;
    }

    private void PreloadAudioAssets()
    {
        audioHolders.Clear();
        audioAssets = CollectAudios(audioHolders);
    }

    public void LoadSceneAudios()
    {
        if (Level.current && Level.current is Level_PVE)
        {
            Level.PrepareAssets(audioAssets, (flag) =>
            {
                if (!flag)
                {
                    Logger.LogError("scene audio loaded errot...");
                    return;
                }

                foreach (var item in audioHolders)
                {
                    var clip = Level.GetPreloadObject<AudioClip>(item.audioName.ToLower(), false);
                    item.Initialize(clip, Level.current);
                }
            });
        }
    }

    public void SetAllPlayers(List<Creature> players)
    {
        if (players == null) return;

        for (int i = 0; i < players.Count; i++)
        {
            SetPlayerData(players[i], i);
        }
    }

    public void SetAllPlayer(Creature p,params Creature[] other)
    {
        SetPlayerData(p);
        if(other != null && other.Length > 0)
        {
            for (int i = 0; i < other.Length; i++)
            {
                SetPlayerData(other[i],i);
            }
        }
    }

    public void SetPlayerData(Creature player,int offset = 0)
    {
        if (!player) return;

        player.transform.SetParent(sceneRoot);
        player.moveState = 0;
        player.stateMachine?.TranslateToID(StateMachineState.STATE_IDLE);
        player.position_ = new Vector3_(position - offset, 0, 0);

        if(player.pet)
        {
            player.pet.transform.SetParent(sceneRoot);
            var show = ConfigManager.Get<ShowCreatureInfo>(player.pet.petInfo.ID);
            if (show == null)
            {
                Logger.LogError("没有配置config_showCreatureInfo表。宠物ID = {0}, 没有出生位置信息。宠物模型创建失败", player.pet.petInfo.ID);
                return;
            }
            var showData = show.GetDataByIndex(0);
            var data = showData.data.GetValue<ShowCreatureInfo.SizeAndPos>(0);
            player.pet.position_ = player.position_ + data.pos;
            player.pet.localEulerAngles = data.rotation;
        }

    }

    public void TransportCache(UnitSceneEventData last)
    {
        if (last == null) return;

        Transform lastPool = last.poolTrans;
        if (lastPool && poolTrans)
        {
            for (int i = 0; i < lastPool.transform.childCount; i++)
            {
                Transform t = lastPool.GetChild(i);
                t?.SetParent(poolTrans);
                t?.gameObject.SetActive(false);
            }
        }
    }

    public IEnumerator CreateEffectPool()
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        //remove all
        if (m_poolTrans && m_poolTrans.childCount > 0)
        {
            for (int i = 0; i < m_poolTrans.childCount; i++)
            {
                Object.DestroyImmediate(m_poolTrans.GetChild(0)?.gameObject);
            }
        }

        foreach (var item in allAssets)
        {
            CreateEffectPoolObject(Level.GetPreloadObject(item,false));
        }
        yield return wait;
    }

    private void CreateEffectPoolObject(Object bundle)
    {
        if (!bundle || !bundle.name.StartsWith("effect_") && !bundle.name.StartsWith("eff_")) return;
        var o = bundle as GameObject;
        if (!o) return;

        var count = CombatConfig.sdefaultEffectPoolCount;
        for (var i = 0; i < count; ++i)
        {
            var ef = Object.Instantiate(o);
            if (ef)
            {
                var d = ef.GetComponent<AutoDestroy>();
                if (d) d.enabled = false;
                ef.SetActive(false);
                ef.name = bundle.name + "__";

                Util.AddChild(poolTrans, ef.transform);

                var iv = EffectInvertHelper.AddToEffect(ef);
                var mm = iv.materials;

                foreach (var m in mm)
                {
                    if (!m) continue;

                    var c = Level.GetEffectMaterialFromPool(m.GetInstanceID());
                    if (!c)
                    {
                        c = Object.Instantiate(m);
                        Level.SetEffectMaterialFromPool(m.GetInstanceID(), c);
                    }

                    m.DisableKeyword("_INVERTED");
                    c.EnableKeyword("_INVERTED");
                }
            }
        }
    }

    private void InactiveAudioListner()
    {
        if(camera)
        {
            camera.tag = Untag;
            AudioListener a = camera.GetComponent<AudioListener>();
            if (a) a.enabled = false;
        }
    }

    public void ActiveAudioListener()
    {
        if (camera)
        {
            camera.tag = MainCameraTag;
            AudioListener a = camera.GetComponentDefault<AudioListener>();
            a.enabled = true;
        }
    }

    /// <summary>
    /// 得到所有的Behaviour
    /// </summary>
    /// <param name="b"></param>
    /// <returns>如果Behaviour没有包含FrameEventID返回本身，如果包含了FrameEventID那么返回本身以及FrameEvent里面包含的所有Behaviour</returns>
    private List<SceneEventInfo.SceneBehaviour> GetFrameEventBehaviours(SceneEventInfo.SceneBehaviour b)
    {
        List<SceneEventInfo.SceneBehaviour> behaviours = new List<SceneEventInfo.SceneBehaviour>();
        if (b == null)
            return behaviours;
        behaviours.Add(b);

        //目前只有创建怪物有帧事件
        if (b.sceneBehaviorType != SceneEventInfo.SceneBehaviouType.CreateMonster)
            return behaviours;

        int frameEventId = b.parameters.GetValue<int>(5);
        var fe = ConfigManager.Get<SceneFrameEventInfo>(frameEventId);
        if (fe)
        {
            if (fe.eventItems != null && fe.eventItems.Length > 0)
            {
                foreach (var f in fe.eventItems)
                {
                    if (f.behaviours == null)
                        continue;
                    foreach (var be in f.behaviours)
                    {
                        behaviours.AddRange(GetFrameEventBehaviours(be));
                    }
                }
            }
        }

        return behaviours;
    }

    public IEnumerator CreateMonsterAsyn()
    {
        if (sceneEventInfo == null) yield break;

        creaturePool.Clear();
        sceneActorPool.Clear();
        List<SceneEventInfo.SceneBehaviour> bes = new List<SceneEventInfo.SceneBehaviour>();
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        foreach (var item in sceneEventInfo.sceneEvents)
        {
            //var enterScene = item.conditions.FindIndex(o => o.sceneEventType == SceneEventInfo.SceneConditionType.EnterScene) >= 0;
            //var first = item.conditions.FindIndex(o => o.sceneEventType == SceneEventInfo.SceneConditionType.StageFirstTime) >= 0;

            ////只预先创建第一波的怪物
            //if (!enterScene && !(first && Module_PVE.instance.isFirstEnterStage))
            //{
            //    foreach (var be in item.behaviours)
            //    {
            //        if (be.sceneBehaviorType == SceneEventInfo.SceneBehaviouType.CreateMonster)
            //        {
            //            SCreateMonsterBehaviour csab = new SCreateMonsterBehaviour(be, null);
            //            sss.Add(csab);
            //        }
            //    }
            //    continue;
            //}

            foreach (var b in item.behaviours)
            {
                bes.Clear();
                bes.AddRange(GetFrameEventBehaviours(b));
                foreach(var be in bes)
                {
                    RealCreateMonster(be);
                    yield return wait;
                }
            }
        }
    }

    /// <summary>
    /// 创建monster/sceneactor
    /// </summary>
    /// <param name="b"></param>
    private void RealCreateMonster(SceneEventInfo.SceneBehaviour b)
    {
        if (b.sceneBehaviorType == SceneEventInfo.SceneBehaviouType.CreateMonster)
        {
            SCreateMonsterBehaviour cmb = new SCreateMonsterBehaviour(b, null);
            MonsterCreature monster = MonsterCreature.CreateMonster(cmb.monsterId, cmb.group, cmb.level, Vector3_.right * cmb.reletivePos, new Vector3(0, 90f, 0), Module_PVE.instance?.currrentStage);
            if (!monster) return;

            monster.enabledAndVisible = false;
            if (sceneRoot) monster.transform.SetParent(sceneRoot);
            creaturePool.Add(monster);
        }
        else if (b.sceneBehaviorType == SceneEventInfo.SceneBehaviouType.CreateSceneActor)
        {
            SCreateSceneActorBehaviour csab = new SCreateSceneActorBehaviour(b, null);
            MonsterCreature monster = MonsterCreature.CreateMonster(csab.sceneActorId, csab.group, csab.level, Vector3_.zero + Vector3_.right * csab.reletivePos, new Vector3(0, 90f, 0), Module_PVE.instance?.currrentStage);
            monster.roleId = (ulong)csab.logicId;
            if (!monster) return;

            monster.enabledAndVisible = false;
            if (sceneRoot) monster.transform.SetParent(sceneRoot);
            sceneActorPool.Add(monster);
        }
    }

    public void Unload(Action callback = null)
    {
        SetSceneActive(false);
        Root.instance.StartCoroutine(_UnloadScene(callback));
        hasDestoryed = true;
        foreach (var item in creaturePool) item.Destroy();
        foreach (var item in sceneActorPool) item.Destroy();
    }

    private IEnumerator _UnloadScene(Action callback)
    {
        if(sceneMap.isLoaded) yield return SceneManager.UnloadSceneAsync(sceneMap);
        Resources.UnloadUnusedAssets();
        GC.Collect();
        callback?.Invoke();
        if (Level.current) Level.current.RemoveNullLevelData();
    }

    #endregion

    private readonly List<SRandomInfoConditon> m_preBuildRandom = new List<SRandomInfoConditon>();
    public void PreBuildRandomValue(ulong rSeed)
    {
        PseudoRandom m_random = new PseudoRandom();
        m_random.seed = rSeed;

        foreach (var e in sceneEventInfo.sceneEvents)
        {
            foreach (var b in e.behaviours)
            {
                if (b.sceneBehaviorType == SceneEventInfo.SceneBehaviouType.BuildRandom)
                {
                    SBuildRandomBehaviour be = new SBuildRandomBehaviour(b, null);
                    int maxValue = be.maxValue;
                    //最后的随机数需要加1，因为策划需要从1~maxValue
                    int random = m_random.Range(1, maxValue == 0 ? 2 : maxValue + 1);
                    m_preBuildRandom.Add(new SRandomInfoConditon(be.randomId, random));
                }
            }
        }
    }

    public bool ContainsCondition(SSceneConditionBase rCondition)
    {
        return m_preBuildRandom.FindIndex(item => item.IsSameCondition(rCondition)) > -1;
    }

    /// <summary>
    /// build random. 
    /// </summary>
    /// <param name="rBehaviour"></param>
    /// <param name="bRebuild"></param>
    /// <returns></returns>
    public SRandomInfoConditon BuildRandom(int randomId, int maxValue, bool bRebuild = false)
    {
 
        var c = m_preBuildRandom.Find(item => item.randomId == randomId);
        if (c == null)
        {
            //最后的随机数需要加1，因为策划需要从1~maxValue
            int random = Module_Battle.Range(1, maxValue == 0 ? 2 : maxValue + 1);
            m_preBuildRandom.Add(new SRandomInfoConditon(randomId, random));
        }
        else
        {
            if(bRebuild)
            {
                int random = Module_Battle.Range(1, maxValue == 0 ? 2 : maxValue + 1);
                c.value = random;
            }
        }
        return c;
    }
}

