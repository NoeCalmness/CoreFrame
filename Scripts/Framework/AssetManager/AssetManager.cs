/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Manage all assets in game.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.2
 * Created:  2017-03-03
 * 
 ***************************************************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Object = UnityEngine.Object;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AssetBundles
{
    public delegate void LoadHandler(string asset, float progress, int total, int current);

    public class Loader : PoolObject<Loader>
    {
        public static Loader Create(string bundle, string asset)
        {
            var loader = Create();
            return loader;
        }
    }

    public class Asset : PoolObject<Asset>
    {
        public static Asset Create(AssetBundle assetBundle)
        {
            var a = Create();
            a.assetBundle = assetBundle;
            a.refCount = 1;
            return a;
        }

        #if UNITY_EDITOR
        public static Asset Create(string name, Object[] assets)
        {
            if (assets == null) assets = new Object[] { };

            var a = Create();
            a.m_name = name;
            a.assetBundle = null;
            a.assets = assets;
            a.mainAsset = assets.Length < 1 ? null : assets[0];
            a.refCount = 1;
            return a;
        }

        public Object mainAsset { get; private set; }
        public Object[] assets { get; private set; }

        private string m_name = null;
        #endif

        public string name
        {
            get
            {
                #if UNITY_EDITOR
                return assetBundle ? assetBundle.name : m_name;
                #else
                return assetBundle ? assetBundle.name : null;
                #endif
            }
        }

        public AssetBundle assetBundle { get; protected set; }

        public T GetAsset<T>(string assetName) where T : Object
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            if (assetBundle) return assetBundle.LoadAsset<T>(assetName);
            #if UNITY_EDITOR
            if (assets != null) return Array.Find(assets, a => a && a.name == assetName) as T;
            #endif
            return null;
        }

        public Object GetAsset(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return null;

            if (assetBundle) return assetBundle.LoadAsset(assetName);
            #if UNITY_EDITOR
            if (assets != null) return Array.Find(assets, a => a && a.name == assetName);
            #endif
            return null;
        }

        public T[] GetAssets<T>() where T : Object
        {
            if (assetBundle) return assetBundle.LoadAllAssets<T>();
            #if UNITY_EDITOR
            if (assets != null)
            {
                var t = new List<T>();
                foreach (var asset in assets)
                {
                    var ta = asset as T;
                    if (!ta) continue;
                    t.Add(ta);
                }
                return t.ToArray();
            }
            #endif
            return null;
        }

        public Object[] GetAssets()
        {
            if (assetBundle) return assetBundle.LoadAllAssets();
            #if UNITY_EDITOR
            if (assets != null) return assets;
            #endif
            return null;
        }

        public int refCount;

        internal event Action onUnload;

        protected override void OnDestroy()
        {
            #if UNITY_EDITOR
            mainAsset = null;
            assets = null;
            m_name = null;
            if (assetBundle != null)
            #endif
            assetBundle.Unload(false);

            var t = onUnload;
            onUnload = null;
            t?.Invoke();

            assetBundle = null;
            refCount = 0;
        }
    }

    public class AssetManager : SingletonBehaviour<AssetManager>
    {
        public static readonly string hashFileName = "6fd65463df87c69f5f6926fb030f67e1";

        public const string GLOBAL_ASSETS_NAME = "global_assets";

        public static bool enableLog { get { return instance && instance.m_enableLog; } set { if (instance) instance.m_enableLog = value; } }
        [SerializeField]
        private bool m_enableLog = true;
        public static bool enableLoadingLog { get { return instance && instance.m_enableLoadingLog; } set { if (instance) instance.m_enableLoadingLog = value; } }
        [SerializeField]
        private bool m_enableLoadingLog = true;

        private static readonly string[] m_baseAssets =
        {
            "data",                  // "Assets/Data", -- Global config data
            "loadinginfo",           // "Assets/Arts/UI/Windows/loadinginfo" -- Loading window info
            "updater",               // "Assets/Arts/UI/Windows/updater" -- Updater panel
            GLOBAL_ASSETS_NAME,      // All bundle marked with prefix "global_"
        };

        private static readonly string[] m_preloadAssets = { "data", "updater" }; // Preloaded assets, load at Launcher.Awake to increase loading speed

        private static string m_serverUrl = string.Empty;
        private static string m_dataHash = null;

        private static AssetBundleManifest m_manifest = null;

        private static Dictionary<string, Asset> m_assets = new Dictionary<string, Asset>();
        private static List<AssetLoadOperation> m_operations = new List<AssetLoadOperation>();
        private static Dictionary<string, string[]> m_dependencies = new Dictionary<string, string[]>();
        private static Dictionary<string, string> m_downloadAssetHashMap = new Dictionary<string, string>();

        public static string serverURL
        {
            get { return m_serverUrl; }
            set
            {
                if (value != null && value.Length > 1 && value[value.Length - 1] != '/') value += "/";
                if (m_serverUrl == value) return;
                m_serverUrl = value;
            }
        }

        public static string dataHash
        {
            get
            {
                #if UNITY_EDITOR
                if (simulateMode) return instance && !string.IsNullOrEmpty(instance.e_SimulatedDataHash) ? instance.e_SimulatedDataHash : string.Empty;
                #endif
                
                if (m_dataHash == null) m_dataHash = GetAssestHash("data");
                return m_dataHash;
            }
        }

        public static string platform { get; protected set; }
        public static string platformAssetPath { get; protected set; }
        public static string streamingAssetPath { get; protected set; }
        public static string downloadAssetPath { get; protected set; }
        public static string downloadAssetURI { get; protected set; }
        public static string streamingAssetURI { get; protected set; }

        public static string GetAssetPath(string asset)
        {
            if (string.IsNullOrEmpty(asset)) return null;

            var ext = Path.GetExtension(asset);

            #if USE_ENCODED_BUNDLE_NAME
            asset = asset.GetMD5();
            #endif

            asset += ext;

            var path = $"{downloadAssetPath}/{asset}";
            if (File.Exists(path)) return path;
            if (HasInternalAsset(asset)) return $"{streamingAssetPath}/{asset}";
            return null;
        }

        public static string GetAssestHash(string asset)
        {
            if (string.IsNullOrEmpty(asset)) return null;

            var ext = Path.GetExtension(asset);

            #if USE_ENCODED_BUNDLE_NAME
            asset = asset.GetMD5();
            #endif

            asset += ext;

            var hash = m_downloadAssetHashMap.Get(asset);
            if (hash == null)
            {
                var idx = Array.IndexOf(m_internalAssetList.assets, asset);
                if (idx > -1) hash = m_internalAssetList.hashList[idx];
            }
            return hash ?? string.Empty;
        }

        public static string GetVideoAssetUrl(string video)
        {
            if (string.IsNullOrEmpty(video)) return string.Empty;
            if (!video.EndsWith(".mp4")) video += ".mp4";

            var url = GetAssetPath(video);
            #if UNITY_EDITOR
            if (simulateMode)
            {
                url = Application.dataPath + "/Videos/" + video;
                if (!File.Exists(url)) url = null;
            }
			#endif
            return url;
        }

        public static bool HasInternalAsset(string asset)
        {
            return Array.IndexOf(internalAssets, asset) > -1;
        }

        public static string GetAssestBundleHash(string rBundleName)
        {
            #if USE_ENCODED_BUNDLE_NAME
            var md5 = rBundleName.GetMD5();
            #else
            var md5 = rBundleName;
            #endif
            var downloadFileHash = HashListFile.ParseAssetHashListFile(hashFileName);
            string hash;
            if (downloadFileHash.TryGetValue(md5, out hash))
            {
                return hash;
            }
            var ia = internalAssets;
            var ih = internalHashList;

            for (int i = 0, c = ia.Length; i < c; ++i)
            {
                var k = ia[i]; var v = ih[i];

                if (k == md5) return v;
            }
            return string.Empty;
        }

        public static InternalAssetList internalAssetList => m_internalAssetList;
        public static string[] internalAssets => m_internalAssetList.assets;
        public static string[] internalHashList => m_internalAssetList.hashList;

        private static InternalAssetList m_internalAssetList = null;

        protected override void Awake()
        {
            base.Awake();

            m_internalAssetList = Resources.Load<InternalAssetList>("internal_assetlist");
            if (!m_internalAssetList)
            {
                Logger.LogDetail("AssetManager: Internal asset list is empty.");

                m_internalAssetList = ScriptableObject.CreateInstance<InternalAssetList>();
                m_internalAssetList.assets = new string[] { };
                m_internalAssetList.hashList = new string[] { };
            }
            Logger.LogDetail($"AssetManager: Load <color=#66EEAA><b>{m_internalAssetList.assets.Length}</b></color> internal assets.");
        }

        public static void Initialize()
        {
            platform           = Util.GetPlatformName();
            platformAssetPath  = platform;
            streamingAssetURI  = GetStreamingAssetsPath();
            downloadAssetURI   = GetDownloadAssetbundlePath();
            streamingAssetPath = GetStreamingAssetsPath(false);
            downloadAssetPath  = GetDownloadAssetbundlePath(false);
        }

        public static IEnumerator LoadAssets(bool simple, LoadHandler onProgress = null, LoadHandler onComplete = null)
        {
            var watcher = TimeWatcher.Watch("AssetManager.LoadAssets");

            m_downloadAssetHashMap = HashListFile.ParseAssetHashListFile(hashFileName);

            var main = LoadManifest();
            yield return main;
            watcher.See("LoadManifest");

            if (main != null) m_manifest = main.GetAsset<AssetBundleManifest>();

            var assets = simple ? m_preloadAssets : m_baseAssets;
            int i = 1, count = assets.Length + 1;
            foreach (var asset in assets)
            {
                var op = LoadAssetAsync(asset, typeof(Object));

                onProgress?.Invoke(asset, 0, count, ++i);
                yield return op;
                watcher.See("Load {0}", asset);
            }
            onComplete?.Invoke("", 1.0f, count, count);
            watcher.See("Load Assets");
            watcher.UnWatch(false);

            Logger.LogDetail($"AssetManager: Assets load complete! {m_assets.Count} assets loaded.");
            Logger.LogDetail($"AssetManager: Current Data Hash: <b><color=#EE6600>{dataHash}</color></b>");
        }

        public static T LoadAsset<T>(string name, string assetName) where T : Object
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(assetName)) return null;

#if UNITY_EDITOR
            if (simulateMode)
            {
                var assetPaths = string.IsNullOrEmpty(assetName) ? AssetDatabase.GetAssetPathsFromAssetBundle(name) : AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(name, assetName);
                if (assetPaths.Length == 0)
                {
                    Logger.LogError("There is no asset with name \"{0}\" in {1}.", assetName, name);
                    return null;
                }

                return (T)AssetDatabase.LoadAssetAtPath(assetPaths[0], typeof(T));
            }
            else
#endif
            {
                if (!m_manifest) return null;

                var b = GetLoadedAssetBundle(name);
                if (b != null)
                {
                    b.refCount++;
                    return b.assetBundle.LoadAsset<T>(name);
                }

                var bundle = new AssetBundleLoadFromFileOperation(name).bundle;
                if (!bundle) return null;

                // Dependencies should load befor asset load
                var deps = m_manifest.GetAllDependencies(name);
                if (deps != null && deps.Length > 0)
                {
                    m_dependencies.Add(name, deps);

                    foreach (var dep in deps)
                    {
                        var db = new AssetBundleLoadFromFileOperation(dep).bundle;
                        if (!db) continue;
                        m_assets.Add(name, Asset.Create(db));
                    }
                }

                // All dependencies loaded, now load actural asset
                m_assets.Add(name, Asset.Create(bundle));

                return bundle.LoadAsset<T>(name);
            }
        }

        /// <summary>
        /// Unloads assetbundle and its deps.
        /// </summary>
        public static void UnloadAssetBundle(string name)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name)) return;
#if UNITY_EDITOR
            // If we're in Editor simulation mode, we don't need to destroy the loaded assetBundle.
            //if (simulateMode) return;
#endif
            UnloadAssetBundleInternal(name);
            UnloadDependencies(name);
        }

        public static string GetStreamingAssetsPath(bool isUri = true)
        {
            return (isUri ? "file://" : "") +  Application.streamingAssetsPath;
        }

        public static string GetDownloadAssetbundlePath(bool isUri = true)
        {
            if (Application.isEditor)
                return (isUri ? "file://" : "") + Environment.CurrentDirectory.Replace("\\", "/") + "/DownloadAssets";
            else if (Application.isMobilePlatform || Application.isConsolePlatform)
                return (isUri ? "file://" : "") + Application.persistentDataPath + "/DownloadAssets";
            else // For standalone player.
                return (isUri ? "file://" : "") + Application.dataPath + "/DownloadAssets";
        }

        public static void SetAssetBundleDirectory(string relativePath)
        {
            serverURL = streamingAssetURI + relativePath;
        }

        public static void SetAssetBundleURL(string absolutePath)
        {
            if (!absolutePath.EndsWith("/")) absolutePath += "/";

            serverURL = absolutePath + platformAssetPath + "/";

            Logger.LogDetail("Asset server set to <b><color=#00DDFF>[{0}]</color></b>", serverURL);
        }

        public static void SetDevelopmentAssetBundleServer()
        {
#if UNITY_EDITOR
            if (simulateMode) return;
#if USE_ENCODED_BUNDLE_NAME
            platformAssetPath += "_Encoded";
#endif
#endif
            SetAssetBundleURL("http://localhost:7888/");
        }

        public static Asset GetLoadedAssetBundle(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            var bundle = m_assets.Get(name);
            if (bundle == null || !CheckDependencies(name)) return null;

            return bundle;
        }

        public static T GetLoadedAsset<T>(string name, bool clone = true) where T : Object
        {
            T a = null;
            var asset = GetLoadedAssetBundle(name);

            a = asset?.GetAsset<T>(name);
            if (a && clone)
            {
                a = Instantiate(a);
                a.name = name;
            }
            return a;
        }

        public static T GetLoadedGlobalAsset<T>(string name, bool clone = true) where T : Object
        {
            T a = null;
            var asset = GetLoadedAssetBundle(GLOBAL_ASSETS_NAME);

            a = asset?.GetAsset<T>(name);
            if (a && clone)
            {
                a = Instantiate(a);
                a.name = name;
            }
            return a;
        }

        public static T[] GetLoadedAssets<T>(string name) where T : Object
        {
            var asset = GetLoadedAssetBundle(name);
            return asset?.GetAssets<T>();
        }

        public static Object[] GetLoadedAssets(string name)
        {
            var asset = GetLoadedAssetBundle(name);
            return asset?.GetAssets();
        }

        public static bool CheckDependencies(string name)
        {
            if (string.IsNullOrEmpty(name)) return true;

            // Check dependencies
            // Dependencies may have been unloaded even asset loaded
            var deps = m_dependencies.Get(name);
            if (deps == null) return true;

            foreach (var dep in deps) if (!m_assets.ContainsKey(dep)) return false;

            return true;
        }

        public static bool Loaded(string name)
        {
            if (!m_assets.ContainsKey(name)) return false;
            return CheckDependencies(name);
        }

        public static AssetLoadOperation LoadManifest()
        {
#if UNITY_EDITOR
            Logger.LogDetail("AssetManager: Simulation Mode is [{0}]", simulateMode ? "Enabled" : "Disabled");

            if (simulateMode) return null;
#endif

            Logger.LogDetail("AssetManager: Loading manifest file...");

            var operation = new AssetLoadOperation(platform, serverURL, "AssetBundleManifest", typeof(AssetBundleManifest));
            m_operations.Add(operation);
            return operation;
        }

        private static void _AddOperation(AssetLoadOperation op)
        {
            LoadDependencies(op.name);     // Load dependencies first

            m_operations.Add(op);
        }

#region Internal load/unload operation

        // Sets up download operation for the given asset bundle if it's not downloaded already.
        protected static bool LoadAssetBundleInternal(string name)
        {
            // Already loaded.
            var bundle = m_assets.Get(name);
            if (bundle != null)
            {
                bundle.refCount++;
                return true;
            }

            // Already in loading queue
            var operation = m_operations.Find(o => o.name == name);
            if (operation != null)
            {
                operation.refCount++;
                return true;
            }

            // Add new load operation
            operation = new AssetLoadOperation(name, m_serverUrl, LoadType.Load);

            m_operations.Add(operation);

            return false;
        }

        // Where we get all the deps and load them all.
        protected static void LoadDependencies(string name)
        {
            if (m_manifest == null)
            {
                Logger.LogError("AssetManager: Please initialize AssetBundleManifest by calling AssetManager.Initialize()");
                return;
            }

            // Get dependecies from the AssetBundleManifest object..
            var deps = m_manifest.GetAllDependencies(name);
            if (deps.Length < 1) return;

            // Record and load all deps.
            m_dependencies.Set(name, deps);
            for (var i = deps.Length - 1; i > -1; --i) LoadAssetBundleInternal(deps[i]);
        }

        protected static void UnloadAssetBundleInternal(string name)
        {            
            var bundle = m_assets.Get(name);
            if (bundle == null) return;

            if (--bundle.refCount <= 0)
            {
                bundle.Destroy();
                m_assets.Remove(name);

                if (enableLog && (!Level.loading || enableLoadingLog)) Logger.LogDetail("AssetManager: {0} has been unloaded successfully", name);
            }
        }

        protected static void UnloadDependencies(string name)
        {
            var deps = m_dependencies.Get(name);
            if (deps == null) return;

            foreach (var dep in deps) UnloadAssetBundleInternal(dep);

            m_dependencies.Remove(name);
        }

#endregion

        private void Update()
        {
            for (int i = 0, c = m_operations.Count; i < c;)
            {
                var operation = m_operations[i];
                if (operation.Update()) ++i;
                else
                {
                    operation.Complete();
                    ProcessFinishedOperation(operation);
                    m_operations.RemoveAt(i);
                    --c;
                }
            }
        }

        private void ProcessFinishedOperation(AssetLoadOperation operation)
        {
            if (!operation.fromCache && operation.asset)
            {
                var asset = Asset.Create(operation.asset);
                asset.refCount = operation.refCount;
                m_assets.Set(operation.name, asset);
            }
        }

        /// <summary>
        /// Starts a load operation for an asset from the given asset bundle.
        /// </summary>
        public static AssetLoadOperation LoadAssetAsync(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            if (enableLog && (!Level.loading || enableLoadingLog)) Logger.LogDetail("AssetManager: Loading asset bundle {0}", name);

            AssetLoadOperation operation = null;
#if UNITY_EDITOR
            if (simulateMode)
            {
                var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(name);
                if (assetPaths.Length == 0)
                {
                    Logger.LogError("There is no asset bundle with name \"{0}\".", name);
                    return null;
                }

                var target = AssetDatabase.LoadMainAssetAtPath(assetPaths[0]);
                operation = new AssetLoadOperation(name, target);
                operation.Complete();
                instance.ProcessSimulateOperation(operation);
            }
            else
#endif
            {
                operation = m_operations.Find(op => op.name == name);

                if (operation != null) operation.refCount++;
                else
                {
                    operation = new AssetLoadOperation(name, m_serverUrl, LoadType.Load);

                    _AddOperation(operation);
                }
            }

            return operation;
        }

        /// <summary>
        /// Starts a load operation for an asset from the given asset bundle.
        /// </summary>
        public static AssetLoadOperation LoadAssetAsync(string name, Type type)
        {
            return LoadAssetAsync(name, null, type);
        }

        /// <summary>
        /// Starts a load operation for an asset from the given asset bundle and asset name.
        /// </summary>
        public static AssetLoadOperation LoadAssetAsync(string name, string assetName, Type type)
        {
            if (string.IsNullOrEmpty(name)) return null;

            if (enableLog && (!Level.loading || enableLoadingLog)) Logger.LogDetail("AssetManager: Loading {0} [{1}] from bundle {2}", string.IsNullOrEmpty(assetName) ? "all" : assetName, type.Name, name);

            AssetLoadOperation operation = null;
#if UNITY_EDITOR
            if (simulateMode)
            {
                var assetPaths = string.IsNullOrEmpty(assetName) ? AssetDatabase.GetAssetPathsFromAssetBundle(name) : AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(name, assetName);

                if (!string.IsNullOrEmpty(assetName))
                {
                    var target = assetPaths.Length < 1 ? null : AssetDatabase.LoadMainAssetAtPath(assetPaths[0]);
                    if (!target || target.GetType() != type && !target.GetType().IsSubclassOf(type))
                    {
                        Logger.LogError("There is no asset with name \"{0}\" in {1} matche type {2}", assetName, name, type);
                        return null;
                    }
                    operation = new AssetLoadOperation(name, target);
                    operation.Complete();
                    instance.ProcessSimulateOperation(operation);
                }
                else
                {
                    var targets = new Object[assetPaths.Length];
                    for (var i = 0; i < assetPaths.Length; ++i) targets[i] = AssetDatabase.LoadMainAssetAtPath(assetPaths[i]);

                    if (targets.Length < 1)
                    {
                        Logger.LogError("Load asset {0} failed, bundle is empty", name);
                        return null;
                    }

                    operation = new AssetLoadOperation(name, targets);
                    operation.Complete();
                    instance.ProcessSimulateOperation(operation);
                }
            }
            else
#endif
            {
                operation = m_operations.Find(op => op.name == name);

                if (operation != null) operation.refCount++;
                else
                {
                    operation = new AssetLoadOperation(name, m_serverUrl, assetName, type);

                    _AddOperation(operation);
                }
            }

            return operation;
        }

#region Editor helper

#if UNITY_EDITOR

        private static int m_simulate = -1;
        const string cache_assetbundle_simulate = "cache_assetbundle_simulate";

        public static bool simulateMode
        {
            get
            {
                if (m_simulate == -1) m_simulate = EditorPrefs.GetBool(cache_assetbundle_simulate, true) ? 1 : 0;
                return m_simulate != 0;
            }
            set
            {
                var simulate = value ? 1 : 0;
                if (simulate != m_simulate)
                {
                    m_simulate = simulate;
                    EditorPrefs.SetBool(cache_assetbundle_simulate, value);
                }
            }
        }

        void ProcessSimulateOperation(AssetLoadOperation operation)
        {
            var asset = Asset.Create(operation.name, operation.GetAssets());
            asset.refCount = operation.refCount;
            m_assets.Set(operation.name, asset);
        }

        [Header("Editor Helper")]
        [SerializeField]
        private string e_SimulatedDataHash = string.Empty;
        [SerializeField]
        private int e_AssetListRefreshRate = 50;
        [SerializeField]
        private int e_OperationListRefreshRate = 50;
        [SerializeField]
        private List<string> e_LoadedAssets = new List<string>();
        [SerializeField]
        private List<string> e_Operations  = new List<string>();

        private int e_AssetListRefreshCount    = 50;
        private int e_OperationListRefreshCount = 50;

        private void LateUpdate()
        {
            if (++e_AssetListRefreshCount >= e_AssetListRefreshRate)
            {
                e_AssetListRefreshCount = 0;

                e_LoadedAssets.Clear();
                foreach (var pair in m_assets)
                    e_LoadedAssets.Add("[" + pair.Value.refCount + "]" + pair.Key + ":" + pair.Value.name);
            }

            if (++e_OperationListRefreshCount >= e_OperationListRefreshRate)
            {
                e_OperationListRefreshCount = 0;

                e_Operations.Clear();
                foreach (var operation in m_operations)
                    e_Operations.Add("[" + operation.refCount + "]" + operation.name + ":" + operation.state + "," + operation.progress);
            }
        }
#endif

#endregion
    }
}
