/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Base scene script.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-04
 * 
 ***************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using AssetBundles;
using Object = UnityEngine.Object;

public class Level : LogicObject
{
    private struct DelayLevelLoadInfo
    {
        public string       name;
        public string       map;
        public string       loadingWindow;
        public int          loadingWindowMode;
        public bool         async;
        public Type         type;
        public LevelInfo    info;

        public DelayLevelLoadInfo(string _name = null, string _map = null, string _loadingWindow = null, int _loadingWindowMode = 0, bool _async = false, Type _type = null, LevelInfo _info = null)
        {
            name              = _name;
            map               = _map;
            loadingWindow     = _loadingWindow;
            loadingWindowMode = _loadingWindowMode;
            async             = _async;
            type              = _type;
            info              = _info;
        }

        public void Set(string _name = null, string _map = null, string _loadingWindow = null, int _loadingWindowMode = 0, bool _async = false, Type _type = null, LevelInfo _info = null)
        {
            name              = _name;
            map               = _map;
            loadingWindow     = _loadingWindow;
            loadingWindowMode = _loadingWindowMode;
            async             = _async;
            type              = _type;
            info              = _info;

            if (_name != null)
                Logger.LogWarning("Level::Load: Try to load level {0} while in loading state...", name);
        }

        public static implicit operator bool(DelayLevelLoadInfo info)
        {
            return !string.IsNullOrEmpty(info.name);
        }
    }

    public struct PreloadAudioHolder
    {
        public string audioName;
        public List<AudioHelper> helpers;
        public List<AudioBGMTrigger> triggers;

        public void Initialize(AudioClip clip, Level level)
        {
            foreach (var h in helpers)  h.Initialize(clip, level);
            foreach (var t in triggers) t.Initialize(clip, level);
        }
    }

    #region Static functions

    /// <summary>
    /// Current actived level, assigned after loading progress reached 1.0 and before SCENE_LOAD_COMPLETE
    /// </summary>
    public static Level current { get; private set; }
    /// <summary>
    /// Current level id
    /// </summary>
    public static int currentLevel { get { return current ? current.levelID : -1; } }
    /// <summary>
    /// Current level root transform
    /// </summary>
    public static Transform currentRoot { get { return current ? current.root : null; } }
    /// <summary>
    /// Current or loading level id
    /// Note: if we are in loading state, will return current loading level's id
    /// </summary>
    public static int currentOrNextLevel { get { return next ? next.levelID : current ? current.levelID : -1; } }
    /// <summary>
    /// Current level main camera
    /// Note: if we are in loading state, will return current loading level's main camera
    /// </summary>
    public static Camera currentMainCamera { get { return next ? next.mainCamera : current ? current.mainCamera : null; } }
    /// <summary>
    /// Current loading level, assigned before SCENE_LOAD_START and null after SCENE_LOAD_COMPLETE
    /// </summary>
    public static Level next { get; private set; }
    /// <summary>
    /// Current loading level info ID
    /// </summary>
    public static int currentLoadingInfoID { get { return next && next.levelInfo ? next.levelInfo.infoID : 0; } }
    /// <summary>
    /// Next (current loading) level id
    /// </summary>
    public static int nextLevel { get { return next ? next.levelID : -1; } }
    /// <summary>
    /// Level is loading ?
    /// </summary>
    public static bool loading { get; private set; }
    /// <summary>
    /// The passed logic time since OnLoadComplete
    /// </summary>
    public static int levelTime { get; set; }
    /// <summary>
    /// The passed real time since OnLoadComplete
    /// </summary>
    public static int realTime { get; set; }
    /// <summary>
    /// Called only once when next loading event will complete
    /// </summary>
    public static Action onLoadOperationComplete = null;

    private static AsyncOperation m_async;
    private static DelayLevelLoadInfo m_pendingLoading;

    private static List<Window.WindowHolder> m_windowStackCache = new List<Window.WindowHolder>();

    public static void Load<T>(string name, string map, string loadingWindow = "", int loadingWindowMode = 0, LevelInfo info = null) where T : Level
    {
        if (loading) { m_pendingLoading.Set(name, map, loadingWindow, loadingWindowMode, false, typeof(T), info); return; }
        var level = _Create<T>(name);
        level.loadingWindow = loadingWindow;
        level.loadingWindowMode = loadingWindowMode;
        level.map = map;
        level.levelInfo = info;

        Root.instance.StartCoroutine(level.Load());
    }

    public static void LoadAsync<T>(string name, string map, string loadingWindow = "", int loadingWindowMode = 0, LevelInfo info = null) where T : Level
    {
        if (loading) { m_pendingLoading.Set(name, map, loadingWindow, loadingWindowMode, true, typeof(T), info); return; }
        var level = _Create<T>(name);
        level.loadingWindow = loadingWindow;
        level.loadingWindowMode = loadingWindowMode;
        level.map = map;
        level.levelInfo = info;

        Root.instance.StartCoroutine(level.LoadAsync());

        return;
    }

    public static void Load(string name, string map, Type type, string loadingWindow = "", int loadingWindowMode = 0, LevelInfo info = null)
    {
        if (type != typeof(Level) && !type.IsSubclassOf(typeof(Level)))
        {
            Logger.LogError("Invalid level type [{0}]", type);
            return;
        }

        if (loading) { m_pendingLoading.Set(name, map, loadingWindow, loadingWindowMode, false, type, info); return; }
        var level = (Level)_Create(name, type);
        level.loadingWindow = loadingWindow;
        level.loadingWindowMode = loadingWindowMode;
        level.map = map;
        level.levelInfo = info;
        Root.instance.StartCoroutine(level.Load());
    }

    public static Coroutine LoadAsync(string name, string map, Type type, string loadingWindow = "", int loadingWindowMode = 0, LevelInfo info = null)
    {
        if (type != typeof(Level) && !type.IsSubclassOf(typeof(Level)))
        {
            Logger.LogError("Invalid level type [{0}]", type);
            return null;
        }

        if (loading) { m_pendingLoading.Set(name, map, loadingWindow, loadingWindowMode, true, type, info); return null; }
        var level = (Level)_Create(name, type);
        level.loadingWindow = loadingWindow;
        level.loadingWindowMode = loadingWindowMode;
        level.map = map;
        level.levelInfo = info;
        
        return Root.instance.StartCoroutine(level.LoadAsync());
    }

    public static T GetPreloadObject<T>(string assetName, bool clone = true) where T : Object
    {
        return current._GetPreloadObject<T>(assetName, clone);
    }

    public static GameObject GetPreloadObject(string assetName, bool clone = true)
    {
        return current._GetPreloadObject(assetName, clone);
    }

    public static GameObject GetPreloadObjectFromPool(string assetName)
    {
        return current._GetPreloadObjectFromPool(assetName);
    }

    public static Material GetEffectMaterialFromPool(int id)
    {
        return current._GetEffectMaterialFromPool(id);
    }

    public static void SetEffectMaterialFromPool(int id, Material mat)
    {
        current._SetEffectMaterialFromPool(id, mat);
    }

    public static Material[] GetEffectMaterialFromPool(Material[] mm)
    {
        return current._GetEffectMaterialFromPool(mm);
    }

    public static void AppednPreloadObject(string assetName, Object asset)
    {
        current._AppednPreloadObject(assetName, asset);
    }

    public static IEnumerator PrepareAssets(List<string> assets)
    {
        return current._PrepareAssets(assets);
    }

    public static Coroutine PrepareAssets(List<string> assets, Action<bool> onComplete, Action<float> onProgress = null)
    {
        return Root.instance?.StartCoroutine(current._PrepareAssets(assets, onComplete, onProgress));
    }

    public static IEnumerator PrepareAsset<T>(string asset) where T : Object
    {
        return current._PrepareAsset<T>(asset);
    }

    public static Coroutine PrepareAsset<T>(string asset, Action<T> onComplete) where T : Object
    {
        #if UNITY_EDITOR
        if (!Game.started)
        {
            var s = AssetManager.simulateMode;
            AssetManager.simulateMode = true;
            var o = AssetManager.LoadAsset<T>(asset, asset);
            AssetManager.simulateMode = s;
            onComplete?.Invoke(o);

            return null;
        }
        #endif
        return Root.instance?.StartCoroutine(current._PrepareAsset(asset, onComplete));
    }

    public static void BackEffect(GameObject effect)
    {
        if (!current)
        {
            if (effect) Object.Destroy(effect);
            return;
        }
        current._BackEffect(effect);
    }

    public static bool ShowHideSceneObjects(bool show)
    {
        return current && current._ShowHideSceneObjects(show);
    }

    public static bool RequestDOFFocusDistance(UnityEngine.PostProcessing.PostProcessingBehaviour behaviour, out float dist, bool forceRequest = false)
    {
        dist = 20.0f;
        return current ? current._RequestDOFFocusDistance_(behaviour, out dist, forceRequest) : false;
    }

    public static void SetDOFFocusTarget(Transform target, float fix = 0, bool projectToPlan = true)
    {
        if (current) current._SetDOFFocusTarget(target, fix, projectToPlan);
    }

    #endregion

    #region Default main camera settings

    public Vector3 startCameraPosition { get { return m_cameraPosition; } }
    public Vector3 startCameraOffset { get { return m_cameraOffset; } set { m_cameraOffset = value; } }

    protected bool          m_cameraOrthographic  = false;
    protected float         m_cameraSize          = 0.75f;
    protected float         m_cameraFOV           = 26;
    protected Vector3       m_cameraRotation      = new Vector3(0, 0, 0);
    protected Vector3       m_cameraPosition      = new Vector3(0, 0, 0);
    protected Vector3       m_cameraOffset        = new Vector3(0, 0, 0);
    protected RenderTexture m_cameraRT            = null;
    protected int           m_cameraCullingMask   = 0;
    
    /// <summary>
    /// Reset the main camera to its initialize state
    /// This is useful when we need to reset level state to start but we do not want to reload current level
    /// </summary>
    public virtual void ResetMainCamera()
    {
        m_mainCamera.transform.position    = m_cameraPosition + m_cameraOffset;
        m_mainCamera.transform.eulerAngles = m_cameraRotation;
        m_mainCamera.orthographic          = m_cameraOrthographic;
        m_mainCamera.orthographicSize      = m_cameraSize;
        m_mainCamera.fieldOfView           = m_cameraFOV;
        m_mainCamera.targetTexture         = m_cameraRT;
        m_mainCamera.cullingMask           = m_cameraCullingMask;

        Logger.LogDetail("Main camera state reset. Level: [{0}:{1}]", id, name);
    }

    #endregion

    #region Public fields

    public Transform root { get; protected set; }
    public Transform pool { get; protected set; }
    public string loadingWindow { get; protected set; }
    public int loadingWindowMode { get; protected set; }
    public string map { get; protected set; }
    public Transform startPos { get; protected set; }
    public LevelInfo levelInfo
    {
        get { return m_levelInfo; }
        set
        {
            m_levelInfo = value;
            if (m_levelInfo)
            {
                levelType = m_levelInfo.type;
                levelID   = m_levelInfo.ID;
                isNormal  = levelType == LevelInfo.LevelType.Normal;
                isPvE     = levelType == LevelInfo.LevelType.PVE;
                isPvP     = levelType == LevelInfo.LevelType.PVP;
                isBattle  = isPvE || isPvP;
            }
            else
            {
                levelType = LevelInfo.LevelType.Normal;
                levelID   = -1;
                isPvE     = false;
                isPvP     = false;
                isNormal  = true;
                isBattle  = false;
            }
        }
    }
    public LevelInfo.LevelType levelType { get; private set; }
    public int levelID { get; private set; }
    public bool isPvP { get; private set; }
    public bool isPvE { get; private set; }
    public bool isNormal { get; private set; }
    public bool isBattle { get; private set; }

    public Light directionalLight { get { return m_directionalLight; } }

    public Camera mainCamera { get { return m_mainCamera; } }

    public Vector3_ edge { get { return m_edge ? new Vector3_(m_edge.left, m_edge.right, m_edge.fix) : Vector3_.right; } }

    public Vector2_ zEdge { get { return m_edge ? new Vector2_(m_edge.up, m_edge.down) : Vector2_.zero; } }

    public AudioHelper audioHelper { get { return m_audioHelper; } }

    public LevelEffect effect { get { return m_effect; } }

    #endregion

    #region Fields
   
    protected List<Camera> m_cameras = new List<Camera>();
    protected List<string> m_preloadAssets = new List<string>();
    private List<Object> m_preloadObjects = new List<Object>();
    protected Camera m_mainCamera = null;

    protected Light m_directionalLight;

    protected Transform m_dofFocusTarget = null;
    protected float m_dofFix = 0;
    protected bool m_dofProjectToPlan = true;
    protected Vector3 m_cachedDOFPosition = Vector3.zero;

    private LevelInfo m_levelInfo;
    private LevelEdge m_edge;
    private LevelEffect m_effect;
    private AudioHelper m_audioHelper;
    protected GameObject m_objects;

    private List<Coroutine> m_coroutines = new List<Coroutine>();

    List<GameObject> m_effectPool = new List<GameObject>();
    Dictionary<int, Material> m_effectMaterialPool = new Dictionary<int, Material>();

    #endregion

    #region Public interface

    public GameObject _GetPreloadObjectFromPool(string assetName)
    {
        var n = assetName + "__";
        var obj = m_effectPool.Find(o => o && o.name == n);
        if (obj)
        {
            m_effectPool.Remove(obj);
            return obj;
        }

        var ef = _GetPreloadObject(assetName);
        if (ef) AddEffectInvertMaterials(ef);

        return ef;
    }

    public Material _GetEffectMaterialFromPool(int id)
    {
        return m_effectMaterialPool.Get(id);
    }

    public void _SetEffectMaterialFromPool(int id, Material mat)
    {
        m_effectMaterialPool.Set(id,mat);
    }

    public Material[] _GetEffectMaterialFromPool(Material[] mm)
    {
        var mmm = new Material[mm.Length];
        for (var i = 0; i < mm.Length; ++i)
        {
            var m = mm[i];
            if (!m) continue;
            mmm[i] = m_effectMaterialPool.Get(m.GetInstanceID());
        }
        return mmm;
    }

    public void _AppednPreloadObject(string assetName, Object asset)
    {
        if (!asset || string.IsNullOrEmpty(assetName)) return;

        var obj = m_preloadObjects.Find(o => o.name == assetName);
        if (obj)
        {
            Object.DestroyImmediate(asset);
            return;
        }

        m_preloadAssets.Add(assetName);
        m_preloadObjects.Add(asset);
    }

    public IEnumerator _PrepareAssets(List<string> assets, Action<bool> onComplete = null, Action<float> onProgress = null)
    {
        if (assets == null || assets.Count < 1)
        {
            onComplete?.Invoke(true);
            yield break;
        }

        var watcher = TimeWatcher.Watch("Level._PrepareAssets");

        assets.Distinct();
        assets.RemoveAll(a => string.IsNullOrEmpty(a) || string.IsNullOrWhiteSpace(a));
        watcher.See("Validate Assets");

        var total = assets.Count;
        var sp = 1.0f / total;
        while (assets.Count > 0)
        {
            var asset = assets[0];

            if (!m_preloadObjects.Find(o => o && o.name == asset))
            {
                var load = AssetManager.LoadAssetAsync(asset, asset, typeof(Object));

                if (load != null)
                {
                    yield return load;

                    onProgress?.Invoke((total - assets.Count + load.progress) * sp);

                    if (destroyed || !root)  // Level destroyed before load complete, just break
                    {
                        AssetManager.UnloadAssetBundle(load.name);
                        onComplete?.Invoke(false);
                        watcher.UnWatch();

                        yield break;
                    }

                    var obj = load.GetAsset();
                    if (obj)
                    {
                        obj.name = obj.name.ToLower(); // Always use lowercase asset name
                        m_preloadObjects.Add(obj);
                        m_preloadAssets.Add(load.name);

                        CreateEffectPoolObject(obj);
                    }
                }
            }

            assets.RemoveAt(0);
        }
        watcher.See("Load Assets");
        watcher.UnWatch(false);

        onProgress?.Invoke(1.0f);
        onComplete?.Invoke(true);
    }

    public IEnumerator _PrepareAsset<T>(string asset, Action<T> onComplete = null) where T : Object
    {
        if (string.IsNullOrEmpty(asset))
        {
            onComplete?.Invoke(null);
            yield break;
        }

        var loaded = m_preloadObjects.Find(o => o && o.name == asset);
        if (loaded)
        {
            onComplete?.Invoke(loaded as T);
            yield break;
        }

        var load = AssetManager.LoadAssetAsync(asset, asset, typeof(T));
        if (load == null)
        {
            onComplete?.Invoke(null);
            yield break;
        }

        var watcher = TimeWatcher.Watch("Level._PrepareAsset<T>");
        yield return load;
        watcher.See(asset);
        watcher.UnWatch(false);

        if (destroyed || !root)  // Level destroyed before load complete, just break
        {
            AssetManager.UnloadAssetBundle(load.name);
            onComplete?.Invoke(null);
            yield break;
        }

        var obj = load.GetAsset<T>();
        if (obj)
        {
            obj.name = obj.name.ToLower(); // Always use lowercase asset name
            m_preloadObjects.Add(obj);
            m_preloadAssets.Add(load.name);

            CreateEffectPoolObject(obj);
        }

        onComplete?.Invoke(obj);
    }

    public Coroutine StartCoroutine(IEnumerator routine)
    {
        if (!Root.instance || destroyed || !root) return null;

        var c = Root.instance.StartCoroutine(routine);
        m_coroutines.Add(c);

        return c;
    }

    public void StopCoroutine(Coroutine routine)
    {
        if (!Root.instance || destroyed) return;
        m_coroutines.Remove(routine);
        Root.instance.StopCoroutine(routine);
    }

    public void _BackEffect(GameObject effect)
    {
        if (!effect) return;
        effect.SetActive(false);
        m_effectPool.Add(effect);
        Util.AddChild(pool, effect.transform);

        if (!effect.name.EndsWith("__")) effect.name += "__";
    }

    public GameObject _GetPreloadObject(string assetName, bool clone = true)
    {
        var obj = m_preloadObjects.Find(o => o && o.name == assetName);
        if (!clone) return obj as GameObject;
        if (obj)
        {
            var o = Object.Instantiate(obj) as GameObject;
            if (!o)
            {
                Logger.LogError($"asset is not gameobject : assetName = {assetName}");
                return o;
            }
            o.name = assetName;
            return o;
        }
        return null;
    }

    public T _GetPreloadObject<T>(string assetName, bool clone = true) where T : Object
    {
        var obj = m_preloadObjects.Find(o => o &&  o.name == assetName);
        if (!clone) return obj as T;
        if (obj)
        {
            var o = Object.Instantiate (obj) as T;
            o.name = assetName;
            return o;
        }
        return null;
    }

    public bool _ShowHideSceneObjects(bool show)
    {
        if (!m_objects || !m_effect || m_objects.activeSelf == show) return false;

        m_objects.SetActive(show);
        m_effect.gameObject.SetActive(show);

        return true;
    }

    public virtual void _SetDOFFocusTarget(Transform target, float fix = 0, bool projectToPlan = true)
    {
        m_dofFocusTarget = target;
        m_dofFix = fix;
        m_dofProjectToPlan = projectToPlan;
        m_cachedDOFPosition = m_dofFocusTarget && m_dofFocusTarget.position == Vector3.zero ? Vector3.one : Vector3.zero;
    }

    public virtual bool _RequestDOFFocusDistance_(UnityEngine.PostProcessing.PostProcessingBehaviour behaviour, out float dist, bool forceRequest = false)
    {
        dist = 20.0f;
        if (!behaviour || !m_dofFocusTarget || !forceRequest && m_dofFocusTarget.position == m_cachedDOFPosition) return false;
        Vector3 a = behaviour.transform.position, b = m_dofFocusTarget.position;
        m_cachedDOFPosition = b;
        if (m_dofProjectToPlan)
        {
            a.y = 0;
            b.y = 0;
        }
        dist = Vector3.Distance(a, b) + m_dofFix;
        return true;
    }

    /// <summary>
    /// unload assts
    /// </summary>
    /// <param name="remainAssets"></param>
    public void UnloadAssets(List<string> remainAssets)
    {
        if (remainAssets == null)
        {
            UnloadAllAssets();
            return;
        }

        remainAssets.Distinct();

        int index = 0;
        while (index < m_preloadObjects.Count)
        {
            if (!m_preloadObjects[index]) m_preloadObjects.RemoveAt(index);
            else if (!remainAssets.Contains(m_preloadObjects[index].name))
            {
                //remove asset bundle
                string name = m_preloadObjects[index].name;
                AssetManager.UnloadAssetBundle(name);
                m_preloadObjects.RemoveAt(index);

                //remove effect instance
                Object obj = m_effectPool.Find(o=> o && o.name.Equals(name));
                if (obj) Object.DestroyImmediate(obj);

                //remove effect material
                int keyId = int.MinValue;
                foreach (var item in m_effectMaterialPool)
                {
                    if(item.Value.name.Equals(name))
                    {
                        keyId = item.Key;
                        break;
                    }
                }
                if (keyId != int.MinValue)
                {
                    if(m_effectMaterialPool[keyId]) Object.DestroyImmediate(m_effectMaterialPool[keyId]);
                    m_effectMaterialPool.Remove(keyId);
                }
            }
            else index++;
        }
    }

    public void UnloadAllAssets()
    {
        DispatchEvent(Events.SCENE_UNLOAD_ASSETS);

        foreach (var p in m_preloadAssets) AssetManager.UnloadAssetBundle(p);
        foreach (var oo in m_effectPool) Object.DestroyImmediate(oo);
        foreach (var mm in m_effectMaterialPool.Values) if (mm) Object.DestroyImmediate(mm);

        m_preloadAssets.Clear();
        m_preloadObjects.Clear();
        m_effectPool.Clear();
        m_effectMaterialPool.Clear();
    }

    public void SetEdge(double left,double right)
    {
        SetLeftEdge(left);
        SetRightEdge(right);
    }

    public void SetLeftEdge(double left)
    {
        if (m_edge) m_edge.left = left;
    }

    public void SetRightEdge(double right)
    {
        if (m_edge) m_edge.right = right;
    }

    #endregion

    #region Virtual functions

    protected virtual bool WaitBeforeLoadComplete() { return true; }
    protected virtual bool WaitBeforeLoadingWindowClose() { return true; }

    protected virtual void OnLoadStart() { }
    protected virtual void OnLoadComplete() { }
    protected virtual void OnLoadingWindowClose() { }
    protected virtual void OnClearWindowStack(List<Window.WindowHolder> stack) { }
    protected virtual void OnRestoreWindowStack(List<Window.WindowHolder> stack) { }

    protected virtual IEnumerator AsyncLoadOtherScene() { yield return null; }
    protected virtual IEnumerator AsyncCreateObjectBeforeLoadComplete() { yield return null; }

    #endregion

    #region Internal functions

    protected override void OnDestroy()
    {
        Window.GrabCurrentRestoreData();

        OnClearWindowStack(Window.stack);
        Window.DestroyWindows();

        Logger.LogInfo("Level [id:{0}, name:{1}, map:{2}] destroyed!", levelID, name, map);

        DispatchEvent(Events.SCENE_DESTROY);

        UnloadAllAssets();
        m_cameras.Clear();

        root = null;
        pool = null;
        startPos = null;

        m_edge        = null;
        m_effect      = null;
        m_audioHelper = null;
        m_levelInfo   = null;
        m_effect      = null;
        m_mainCamera  = null;

        if (Root.instance)
        {
            foreach (var op in m_coroutines)
                if (op != null) Root.instance.StopCoroutine(op);
        }
        m_coroutines.Clear();

        Resources.UnloadUnusedAssets();
        GC.Collect();

        if (current == this) current = null;
    }

    private IEnumerator Load()
    {
        var watch = TimeWatcher.Watch("Level.Load");

        Logger.LogInfo("Level [id: {0}, name:{1}, map:{2}] load started!", levelID, name, map);

        loading = true;
        next = this;

        Game.paused = false;
        CGManager.Stop();
        ObjectManager.timeScale = 1.0;  // Reset timescale
        UIManager.SetFixCameraState(1);
        InputManager.Enable(false);

        CollectModuleCallbacks();
        watch.See("CollectModuleCallbacks");

        moduleGlobal.UnLockUI(Module_Global.LEVEL_LOCK_PRIORITY); // Unlock UI when we start loading a new level

        DispatchEvent(Events.SCENE_LOAD_START);
        watch.See("Events.SCENE_LOAD_START");

        yield return null;

        OnLoadStart();
        watch.See("OnLoadStart");

        SceneManager.LoadScene(map);
        watch.See("SceneManager.LoadScene");

        yield return AsyncLoadOtherScene();
        watch.See("AsyncLoadOtherScene");

        CreateEnvironments();
        watch.See("CreateEnvironments");

        AudioManager.Clear();

        yield return null;

        m_windowStackCache.Clear();
        OnRestoreWindowStack(m_windowStackCache);
        watch.See("OnRestoreWindowStack");

        Logger.LogInfo($"Level [id:{levelID}, name:{name}, map:{map}] restored window stack [{m_windowStackCache.PrettyPrint()}]");

        yield return LoadPreloadAssets();
        watch.See("LoadPreloadAssets");

        DispatchEvent(Events.SCENE_LOAD_PROGRESS, Event_.Pop(0.99f));
        watch.See("Events.SCENE_LOAD_PROGRESS");

        if (current) current.Destroy();
        watch.See("current.Destroy");

        current = this;
        next = null;

        Window.stack.AddRange(m_windowStackCache);
        m_windowStackCache.Clear();

        Logger.LogInfo("Level [id:{0}, name:{1}, map:{2}] created!", levelID, name, map);

        UIManager.SetFixCameraState(-1);

        yield return new WaitForEndOfFrame(); // Wait next graphic rebuilding

        yield return new WaitUntil(WaitBeforeLoadComplete);
        watch.See("WaitBeforeLoadComplete");

        levelTime = 0;
        realTime  = 0;

        yield return AsyncCreateObjectBeforeLoadComplete();
        watch.See("AsyncCreateObjectBeforeLoadComplete");

        OnLoadComplete();
        watch.See("OnLoadComplete");

        DispatchEvent(Events.SCENE_LOAD_PROGRESS, Event_.Pop(1.0f));
        watch.See("Events.SCENE_LOAD_PROGRESS");

        yield return new WaitUntil(WaitBeforeLoadingWindowClose);
        watch.See("WaitBeforeLoadingWindowClose");

        InputManager.Enable();

        OnLoadingWindowClose();
        watch.See("OnLoadingWindowClose");

        var tmp = onLoadOperationComplete;
        onLoadOperationComplete = null;
        tmp?.Invoke();
        watch.See("onLoadOperationComplete");

        loading = false;

        DispatchEvent(Events.SCENE_LOAD_COMPLETE);
        watch.See("Events.SCENE_LOAD_COMPLETE");

        watch.UnWatch();

        LoadPendingLevel();
    }

    private IEnumerator LoadAsync()
    {
        var watch = TimeWatcher.Watch("Level.LoadAsync");

        Logger.LogInfo("Level [id:{0}, name:{1}, map:{2}] load started!", levelID, name, map);

        loading = true;
        next = this;

        Game.paused = false;
        CGManager.Stop();
        ObjectManager.timeScale = 1.0;  // Reset timescale
        UIManager.SetFixCameraState(1);
        InputManager.Enable(false);

        CollectModuleCallbacks();
        watch.See("CollectModuleCallbacks");

        moduleGlobal.UnLockUI(Module_Global.LEVEL_LOCK_PRIORITY); // Unlock UI when we start loading a new level

        DispatchEvent(Events.SCENE_LOAD_START);
        watch.See("Events.SCENE_LOAD_START");

        yield return null;

        OnLoadStart();
        watch.See("OnLoadStart");

        yield return new WaitForSeconds(0);

        m_async = SceneManager.LoadSceneAsync(map, LoadSceneMode.Single);
        while (!m_async.isDone)
        {
            DispatchEvent(Events.SCENE_LOAD_PROGRESS, Event_.Pop(m_async.progress * 0.39f));
            yield return null;
        }
        watch.See("SceneManager.LoadSceneAsync");

        yield return AsyncLoadOtherScene();
        watch.See("AsyncLoadOtherScene");

        CreateEnvironments();
        watch.See("CreateEnvironments");

        AudioManager.Clear();

        m_windowStackCache.Clear();
        OnRestoreWindowStack(m_windowStackCache);
        watch.See("OnRestoreWindowStack");

        Logger.LogInfo($"Level [id:{levelID}, name:{name}, map:{map}] restored window stack [{m_windowStackCache.PrettyPrint()}]");

        yield return LoadPreloadAssets();
        watch.See("LoadPreloadAssets");

        DispatchEvent(Events.SCENE_LOAD_PROGRESS, Event_.Pop(0.99f));
        m_async = null;
        watch.See("Events.SCENE_LOAD_PROGRESS");

        if (current) current.Destroy();
        watch.See("current.Destroy");

        current = this;
        next = null;

        Window.stack.AddRange(m_windowStackCache);
        m_windowStackCache.Clear();

        Logger.LogInfo("Level [id:{0}, name:{1}, map:{2}] created!", levelID, name, map);

        UIManager.SetFixCameraState(-1);

        yield return new WaitForEndOfFrame(); // Wait next graphic rebuilding

        yield return new WaitUntil(WaitBeforeLoadComplete);
        watch.See("WaitBeforeLoadComplete");

        levelTime = 0;
        realTime  = 0;
        
        yield return AsyncCreateObjectBeforeLoadComplete();
        watch.See("AsyncCreateObjectBeforeLoadComplete");

        OnLoadComplete();

        DispatchEvent(Events.SCENE_LOAD_PROGRESS, Event_.Pop(1.0f));
        watch.See("Events.SCENE_LOAD_PROGRESS");

        yield return new WaitUntil(WaitBeforeLoadingWindowClose);
        watch.See("WaitBeforeLoadingWindowClose");

        InputManager.Enable();

        OnLoadingWindowClose();
        watch.See("OnLoadingWindowClose");

        var tmp = onLoadOperationComplete;
        onLoadOperationComplete = null;
        tmp?.Invoke();
        watch.See("onLoadOperationComplete");

        loading = false;

        DispatchEvent(Events.SCENE_LOAD_COMPLETE);
        watch.See("Events.SCENE_LOAD_COMPLETE");

        watch.UnWatch();

        LoadPendingLevel();
    }

    private IEnumerator LoadPreloadAssets()
    {
        var watch = TimeWatcher.Watch("Level.LoadPreloadAssets");

        m_preloadAssets = BuildPreloadAssets();
        watch.See("BuildPreloadAssets");

        var holders = new List<PreloadAudioHolder>();
        var audios = CollectAudios(holders);
        var audioCount = audios.Count;

        m_preloadAssets.InsertRange(0, audios);   // Load level audios before any assets, we need to play level bgm first
        watch.See("Validate Audios");

        if (m_preloadAssets.Count < 1)
        {
            watch.UnWatch();
            yield break;
        }

        m_preloadAssets.Distinct(); // Remove duplicated asset names..
        m_preloadAssets.RemoveAll(a => string.IsNullOrEmpty(a) || string.IsNullOrWhiteSpace(a)); // Remove invalid asset names..
        watch.See("Validate Assets");

        var cached = current ? current.m_preloadAssets : null;
        if (cached != null && cached.Count > 0)  // Remove loaded assets to decrease memory usage
        {
            for (var i = 0; i < cached.Count; ++i)
            {
                var cache = cached[i];
                if (m_preloadAssets.Contains(cached[i])) continue;  // These assets will unload later

                cached[i] = null;
                AssetManager.UnloadAssetBundle(cache);
            }

            yield return Resources.UnloadUnusedAssets();  // Release memory usage
            GC.Collect();
        }
        watch.See("Optimize Cached Assets");

        var ops = new AssetLoadOperation[m_preloadAssets.Count];

        for (var i = 0; i < ops.Length; ++i)
        {
            var asset = m_preloadAssets[i];
            var op = AssetManager.LoadAssetAsync(asset, asset, typeof(Object));
            if (i < audioCount && op == null) holders[i].Initialize(null, this);

            ops[i] = op;
        }
        watch.See("Build Assets");

        var loaded = false;
        while (!loaded)
        {
            loaded = true;
            var p = 0f;
            for (var i = 0; i < ops.Length; ++i)
            {
                var op = ops[i];
                p += op == null ? 1f : op.progress;

                if (op == null) continue;

                if (op.isDone)
                {
                    ops[i] = null;

                    var obj = op.GetAsset();
                    if (obj)
                    {
                        obj.name = obj.name.ToLower(); // Always use lowercase asset name
                        m_preloadObjects.Add(obj);

                        CreateEffectPoolObject(obj);
                    }

                    if (i < audioCount) holders[i].Initialize(obj as AudioClip, this);
                }
                else loaded = false;
            }

            p = p / ops.Length;

            DispatchEvent(Events.SCENE_LOAD_PROGRESS, Event_.Pop(0.39f + p * 0.6f));

            yield return null;
        }
        watch.See("Load Assets");
        watch.UnWatch();

        holders.Clear();

        holders = null;
        ops = null;
    }

    private bool LoadPendingLevel()
    {
        if (!m_pendingLoading) return false;
        if (m_pendingLoading.type == GetType() && m_pendingLoading.name == name && m_pendingLoading.map == map)  // Pending level is same as current
        {
            m_pendingLoading.Set();
            return false;
        }

        Logger.LogWarning("Load pending level [{0}:{1}]", m_pendingLoading.name, m_pendingLoading.map);

        var loadInfo = m_pendingLoading;
        m_pendingLoading.Set();

        if (loadInfo.async) LoadAsync(loadInfo.name, loadInfo.map, loadInfo.type, loadInfo.loadingWindow, loadInfo.loadingWindowMode, loadInfo.info);
        else Load(loadInfo.name, loadInfo.map, loadInfo.type, loadInfo.loadingWindow, loadInfo.loadingWindowMode, loadInfo.info);

        return true;
    }

    private void CollectModuleCallbacks()
    {
        var types = new List<Type>();
        Type t = GetType();
        do
        {
            types.Add(t);
            t = t.BaseType;
        } while (t != null && t != typeof(Level));
        
        foreach (var item in types)
        {
            var methods = item.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            for (var i = 0; i < methods.Length; ++i)
            {
                var method = methods[i];
                if (method.Name != "_ME") continue;

                var ps = method.GetParameters();
                if (ps.Length != 1 || !ps[0].ParameterType.IsSubclassOf(typeof(ModuleEvent)) || !ps[0].ParameterType.IsGenericType)
                {
                    Logger.LogWarning("Level::CollectModuleCallbacks: ModuleEventHandler has invalid parameters: [level: {0}, method: {1}, paramCount: {2}, paramType: {3}]", name, method.Name, ps.Length, ps.Length > 0 ? ps[0].ParameterType.Name : "");
                    continue;
                }

                var handler = Delegate.CreateDelegate(typeof(ModuleHandler<>).MakeGenericType(new Type[] { ps[0].ParameterType.GetGenericArguments()[0] }), this, method, false);
                EventManager.AddModuleListener(ModuleEvent.GLOBAL, handler);
            }
        }
    }

    private void CreateEffectPoolObject(Object bundle)
    {
        if (!bundle.name.StartsWith("effect_") && !bundle.name.StartsWith("eff_")) return;
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
                m_effectPool.Add(ef);

                Util.AddChild(pool, ef.transform);

                AddEffectInvertMaterials(ef);
            }
        }
    }

    private void AddEffectInvertMaterials(GameObject ef)
    {
        var iv = EffectInvertHelper.AddToEffect(ef);
        var mm = iv.materials;

        foreach (var m in mm)
        {
            if (!m) continue;

            var c = m_effectMaterialPool.Get(m.GetInstanceID());
            if (!c)
            {
                c = Object.Instantiate(m);
                m_effectMaterialPool.Set(m.GetInstanceID(), c);
            }

            m.DisableKeyword("_INVERTED");
            c.EnableKeyword("_INVERTED");
        }
    }

    protected virtual List<string> BuildPreloadAssets()
    {
        m_preloadAssets.Clear();

        if (m_windowStackCache.Count > 0)
            m_preloadAssets.Add(m_windowStackCache[0].windowName.ToLower());

        if (!levelInfo || levelInfo.preloadAssets == null) return m_preloadAssets;
        foreach (var a in levelInfo.preloadAssets)
        {
            if (string.IsNullOrEmpty(a)) continue;
            if (a.Length > 5 && string.Compare(a, 0, "_npc_", 0, 5) == 0)            // NPC assets
            {
                var id = Util.Parse<int>(a.Substring(5));
                if (id != 0) Module_Battle.BuildNpcSimplePreloadAssets(id, m_preloadAssets);
                else m_preloadAssets.Add(a);

                continue;
            }

            m_preloadAssets.Add(a);
        }
        return m_preloadAssets;
    }

    private List<string> CollectAudios(List<PreloadAudioHolder> holders = null)
    {
        var audios = new List<string>();

        var ah = root.GetComponent<AudioHelper>("audios");
        if (!ah) return audios;

        if (holders == null) audios.Add(ah.audioName);
        else if (string.IsNullOrEmpty(ah.audioName) || string.IsNullOrWhiteSpace(ah.audioName)) ah.Initialize(null, this);
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
            else if (string.IsNullOrEmpty(at.audioName) || string.IsNullOrWhiteSpace(at.audioName)) at.Initialize(null, this);
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

    /// <summary>
    /// 移除空物体（移除跟场景绑定在一起的空物体）
    /// </summary>
    public void RemoveNullLevelData()
    {
        if (m_preloadObjects != null)
        {
            int index = 0;
            while (index < m_preloadObjects.Count)
            {
                if (!m_preloadObjects[index]) m_preloadObjects.RemoveAt(index);
                else index++;
            }
        }
    }

    public virtual void RecreateEnvironments(Scene scene)
    {
        m_cameras.Clear();
        m_mainCamera = null;
        map = scene.name;
        
        CreateEnvironmentComponents(scene);
    }

    protected virtual void CreateEnvironments()
    {
        m_cameras.Clear();

        var scene = SceneManager.GetActiveScene();
        CreateEnvironmentComponents(scene);
        RecreateEffectPool();
    }

    private void RecreateEffectPool()
    {
        m_effectPool.Clear();
        Transform cache = null;
        for (int i = 0, count = pool ? pool.childCount : 0; i < count; i++)
        {
            cache = pool.GetChild(i);
            if (cache && cache.name.EndsWith("__")) m_effectPool.Add(cache.gameObject);
        }
    }

    protected virtual void CreateEnvironmentComponents(Scene scene)
    {
        if (!scene.isLoaded || string.Compare(scene.name, map, true) != 0)
        {
            Logger.LogException("Cuould not create environments, current level map is not loaded, will destroy latter! Level:[name:{0}, map:{1}], Loaded:[{2}]", name, map, scene.name);
            return;
        }

        var roots = scene.GetRootGameObjects();

        if (scene.rootCount > 1)
        {
            var nodes = "[";
            foreach (var r in roots) nodes += r.name + ", ";
            nodes = nodes.Remove(nodes.Length - 2) + "]";
            Logger.LogWarning("Level can only have one root object, Level: [name:{0}, map:{1}], roots:{2}", name, map, nodes);
        }
        else if (scene.rootCount < 1)
        {
            Logger.LogWarning("Could not find any root object in current level, create default. Level: [name:{0}, map:{1}]", name, map);
            new GameObject();
        }


        var ro = Array.Find(roots, r => string.Compare(r.name, "sceneRoot", true) == 0 || string.Compare(r.name, "levelRoot", true) == 0);
        if (!ro) ro = roots[0];

        root = ro.transform;
        root.name = "levelRoot";

        pool = root.Find("pool");
        if (!pool)
        {
            pool = new GameObject("pool").transform;
            pool.SetParent(root);
        }

        var nul = new GameObject("__null") { hideFlags = HideFlags.HideInHierarchy };
        nul.transform.SetParent(root);
        m_preloadObjects.Add(nul);

        var lights = root.Find("lights");
        if(!lights)
        {
            lights = new GameObject("lights").transform;
            lights.SetParent(root);
        }

        for (int i = 0, count = lights.childCount; i < count; ++i)
        {
            var light = lights.GetChild(i).GetComponent<Light>();
            if (light && light.type == LightType.Directional)
            {
                m_directionalLight = light;
                break;
            }
        }

        var cameras = root.Find("cameras");
        if (!cameras)
        {
            cameras = new GameObject("cameras").transform;
            cameras.SetParent(root);
        }

        for (int i = 0, c = cameras.childCount; i < c; ++i)
        {
            var camera = cameras.GetChild(i).GetComponent<Camera>();
            if (camera)
            {
                m_cameras.Add(camera);
                if (camera.CompareTag("MainCamera"))
                {
                    if (!m_mainCamera) m_mainCamera = camera;
                    else camera.tag = "Untagged";
                }
            }
        }

        if (!m_mainCamera)
        {
            m_mainCamera = Camera.main;
            if (m_mainCamera)
            {
                m_cameras.Add(m_mainCamera);
                m_mainCamera.transform.SetParent(cameras, true);
            }
        }

        if (!m_mainCamera)
        {
            Logger.LogWarning("Could not find any camera attached to current level, use default camera. Level: [name:{0}, map:{1}]", name, map);

            m_mainCamera = new GameObject("main", typeof(Camera)).GetComponent<Camera>();
            m_mainCamera.transform.SetParent(cameras);
            m_mainCamera.transform.localPosition = new Vector3(0, 0, -5.0f);

            m_mainCamera.tag = "MainCamera";
        }

        var behaviour = root.GetComponentDefault<LevelBehavior>();
        behaviour.level = this;

        m_mainCamera.cullingMask &= ~(Layers.MASK_UI | Layers.MASK_INVISIBLE);

        m_cameraOrthographic = m_mainCamera.orthographic;
        m_cameraSize         = m_mainCamera.orthographicSize;
        m_cameraFOV          = m_mainCamera.fieldOfView;
        m_cameraPosition     = m_mainCamera.transform.position;
        m_cameraRotation     = m_mainCamera.transform.eulerAngles;
        m_cameraRT           = m_mainCamera.targetTexture;
        m_cameraCullingMask  = m_mainCamera.cullingMask;

        startPos = root.Find("startPos");
        if (!startPos)
        {
            Logger.LogWarning("Could not find a valid startPos, use default. Level: [name:{0}, map:{1}]", name, map);
            startPos = new GameObject("startPos").transform;
            startPos.SetParent(root);
        }
        m_edge = startPos.GetComponentDefault<LevelEdge>();

        var effects = root.Find("effects");
        if (!effects)
        {
            effects = new GameObject("effects").transform;
            effects.SetParent(root);
        }
        m_effect = effects.GetComponentDefault<LevelEffect>();

        var audios = root.Find("audios");
        if (!audios)
        {
            audios = new GameObject("audios").transform;
            audios.SetParent(root);
        }
        m_audioHelper = audios.GetComponentDefault<AudioHelper>();

        if (startPos.position != Vector3.zero)
        {
            Logger.LogError("Level [{0}:{1}] has invalid startPos [{2}], please fix it in editor.", id, name, startPos.position);

            for (var i = 0; i < roots.Length; ++i)
            {
                var r = roots[i].transform;

                if (startPos.parent == r) r.eulerAngles -= startPos.eulerAngles;
                r.position -= startPos.position;
            }
        }

        var objects = root.Find("objects");
        if (!objects)
        {
            objects = new GameObject("objects").transform;
            objects.SetParent(root);
        }
        m_objects = objects.gameObject;

        DispatchEvent(Events.SCENE_CREATE_ENVIRONMENT);
    }

    #endregion

    #region Editor helper

#if UNITY_EDITOR
    public void RemoveDestroyedPreloadObjects()
    {
        m_preloadObjects.RemoveAll(o => !o);
    }

    public Object UpdatePreloadObject(string assetName, string path, bool destroy = true)
    {
        if (!m_preloadAssets.Contains(assetName)) return null;

        var obj = UnityEditor.AssetDatabase.LoadMainAssetAtPath(path);
        var oo = m_preloadObjects.Find(o => o && o.name == assetName);
        if (!oo) m_preloadObjects.Add(obj);
        else
        {
            m_preloadObjects.Remove(oo);
            if (destroy) Object.Destroy(oo);
            m_preloadObjects.Add(obj);
        }

        return obj;
    }
#endif

    #endregion
}
