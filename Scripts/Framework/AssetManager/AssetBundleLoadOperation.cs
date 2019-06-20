using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections;

namespace AssetBundles
{
    /// <summary>
    /// Asset load operation state
    /// </summary>
    public enum LoadState
    {
        Preparing      = 0,   // Load request created, waiting for request
        Downloading    = 1,   // Asset not cached, we are downloading it from remote server
        DownloadingDep = 2,   // Downloading asset dependencies
        Loading        = 3,   // We are loading assetbundle from cached file or downloaded memory data
        Processing     = 4,   // We are processing actual asset type from loaded assetbundle
    }

    public enum LoadType
    {
        Download  = 0,    // Just download from web
        Load      = 1,    // Load assetbundle
        LoadAsset = 2,    // Load assetbundle and asset from it
#if UNITY_EDITOR
        Simulate  = 3,    // Editor simulate type
#endif
    }

    public class AssetLoadOperation : CustomYieldInstruction
    {
        public int refCount = 1;

        public LoadType type { get; set; }
        public LoadState state { get; protected set; }

        public override bool keepWaiting { get { return !isDone; } }

        public bool isDone { get; protected set; }
        public bool fromCache { get; protected set; }
        public AssetBundle asset { get; protected set; }
        public string source { get; protected set; }
        public string name { get; protected set; }
        public string error { get { return m_error; } protected set { m_error = value; } }
        public bool isError { get { return !string.IsNullOrEmpty(error); } }

        public float progress { get; protected set; }

        protected string m_error = null;

        internal AssetBundleSaveOperation m_save = null;

        public AssetLoadOperation(string _name, string _uri, LoadType _type = LoadType.Download)
        {
            type   = _type;
            state  = LoadState.Preparing;
            name   = _name;
            #if USE_ENCODED_BUNDLE_NAME
            source      = _uri + name.GetMD5();
            #else
            source      = _uri + name;
            #endif
        }

        #region Basic operation

        public void Request()
        {
            switch (type)
            {
                case LoadType.Download:  RequestDownload();  break;
                case LoadType.Load:      RequestLoad();      break;
                case LoadType.LoadAsset: RequestLoadAsset(); break;
                default: return;
            }
        }

        public bool Update()
        {
#if UNITY_EDITOR
            if (type == LoadType.Simulate) return false;
#endif

            if (state == LoadState.Preparing)
                Request();

            if (state == LoadState.Downloading)
            {
                if (UpdateDownload()) return true;
                if (type < LoadType.Load || isError)
                {
                    m_request = null;
                    if (m_save != null) m_save.dispose = true;
                    m_save = null;

                    return false;
                }

                state = LoadState.Loading;
                m_load = AssetBundle.LoadFromMemoryAsync(m_request.downloadHandler.data);
            }

            if (state ==  LoadState.Loading)
            {
                if (UpdateLoad()) return true;

                if (m_save != null) m_save.dispose = true;
                m_save = null;

                if (type < LoadType.LoadAsset || isError)
                {
                    m_request = null;
                    return false;
                }

                state = LoadState.Processing;
                m_process = m_assetName == null ? asset.LoadAllAssetsAsync(m_assetType) : asset.LoadAssetAsync(m_assetName, m_assetType);
            }

            if (state == LoadState.Processing)
                return UpdateLoadAsset();

            return false;
        }

        public void Complete()
        {
            if (isDone) return;
            isDone = true;

            if (isError)
                Logger.LogError("AssetManager: Load asset {0} from {1} failed. {2}", name, source, error);

            OnComplete();
        }

        protected virtual void OnComplete() { }

        #endregion

        #region Download operation

        protected UnityWebRequest m_request;

        protected void RequestDownload()
        {
            state = LoadState.Downloading;

            m_request = UnityWebRequest.Get(source);
            m_request.SendWebRequest();

            if ((!Level.loading || AssetManager.enableLoadingLog) && AssetManager.enableLog)
                Logger.LogDetail("AssetManager: Downloading {0} from {1}", name, source);
        }

        protected bool UpdateDownload()
        {
            if (m_request.isNetworkError || m_request.isHttpError)
            {
                error = m_request.responseCode + ":" + m_request.error;
                return false;
            }

            if (m_request.isDone)
            {
                progress = type == LoadType.Load ? 0.5f : type == LoadType.LoadAsset ? 0.33f : 1.0f;

                if (!isError)
                    OnDownloadComplete();

                return false;
            }

            progress = m_request.downloadProgress / ((int)type + 1f);

            return true;
        }

        protected virtual void OnDownloadComplete()
        {
            m_save = new AssetBundleSaveOperation(AssetManager.downloadAssetPath, name, m_request);
            AssetManager.instance.StartCoroutine(m_save.DoSave());
        }

        #endregion

        #region Load operation

        protected AssetBundleCreateRequest m_load = null;

        protected void RequestLoad()
        {
            var loaded = AssetManager.GetLoadedAssetBundle(name);
            if (loaded == null)
            {
                var path = AssetManager.GetAssetPath(name);
                if (path != null)
                {
                    state = LoadState.Loading;
                    m_load = AssetBundle.LoadFromFileAsync(path);

                    if ((!Level.loading || AssetManager.enableLoadingLog) && AssetManager.enableLog)
                        Logger.LogDetail("AssetManager: Load {0} from file {1}", name, path);
                }
                else RequestDownload();
            }
            else
            {
                loaded.refCount++;

                fromCache = true;
                progress = 1.0f;
                state = LoadState.Loading;
                asset = loaded.assetBundle;
            }
        }

        protected bool UpdateLoad()
        {
            if (asset) return false;

            if (m_load == null)
            {
                error = Util.Format("AssetManager: Could not load asset {0} from memory cache. source: {1}", name, source);
                return false;
            }

            if (m_load.isDone)
            {
                if (!AssetManager.CheckDependencies(name)) return true;

                progress = type == LoadType.LoadAsset ? 0.66f : 1.0f;

                OnLoadComplete();

                return false;
            }

            progress = m_load.progress * 0.5f + 0.5f;
            if (type == LoadType.LoadAsset) progress *= 0.66f;

            return true;
        }

        protected virtual void OnLoadComplete()
        {
            asset = m_load.assetBundle;
            m_load = null;

            if (!asset) error = Util.Format("Asset bundle [{0}] already loaded.", name);
        }

        #endregion

        #region Load asset operation

        private string m_assetName;
        private AssetBundleRequest m_process = null;
        private System.Type m_assetType = null;

#if UNITY_EDITOR
        private Object[] m_simulatedObjects;

        public AssetLoadOperation(string _name, Object[] simulatedObjects)
        {
            m_simulatedObjects = simulatedObjects;

            type     = LoadType.Simulate;
            name     = _name;
            isDone   = true;
            progress = 1.0f;
        }

        private Object m_simulatedObject;

        public AssetLoadOperation(string _name, Object simulatedObject)
        {
            m_simulatedObject = simulatedObject;

            type     = LoadType.Simulate;
            name     = _name;
            isDone   = true;
            progress = 1.0f;
        }
#endif

        public AssetLoadOperation(string _name, string _uri, string _assetName, System.Type _assetType)
        {
            type        = LoadType.LoadAsset;
            state       = LoadState.Preparing;
            name        = _name;
            #if USE_ENCODED_BUNDLE_NAME
            source      = _uri + name.GetMD5();
            #else
            source      = _uri + name;
            #endif

            m_assetType = _assetType;
            m_assetName = _assetName;
        }

        public AssetLoadOperation(string _name, string _uri, System.Type _assetType)
        {
            type        = LoadType.LoadAsset;
            state       = LoadState.Preparing;
            name        = _name;
            #if USE_ENCODED_BUNDLE_NAME
            source      = _uri + name.GetMD5();
            #else
            source      = _uri + name;
            #endif

            m_assetType = _assetType;
            m_assetName = null;
        }

        public T GetAsset<T>() where T : Object
        {
#if UNITY_EDITOR
            if (type == LoadType.Simulate) return m_simulatedObject as T;
#endif

            if (m_process != null && m_process.isDone)
                return m_process.asset as T;
            else
                return null;
        }

        public Object GetAsset()
        {
#if UNITY_EDITOR
            if (type == LoadType.Simulate) return m_simulatedObject;
#endif

            if (m_process != null && m_process.isDone)
                return m_process.asset;
            else
                return null;
        }

        public Object[] GetAssets()
        {
#if UNITY_EDITOR
            if (type == LoadType.Simulate) return m_simulatedObjects;
#endif

            if (m_process != null && m_process.isDone)
                return m_process.allAssets;
            else
                return null;
        }

        protected void RequestLoadAsset()
        {
            RequestLoad();
        }

        protected bool UpdateLoadAsset()
        {
            if (m_process == null)
            {
                error = Util.Format("AssetManager: Could not load asset type {0} from assetbundle. source: {1}", name, source);
                return false;
            }

            if (m_process.isDone)
            {
                progress = 1.0f;
                                      
                OnProcessComplete();

                return false;
            }

            progress = m_process.progress * 0.33f + 0.66f;
            return true;
        }

        protected virtual void OnProcessComplete() { }

        #endregion

        protected virtual bool OnUpdate() { return false; }
    }

    public class AssetBundleLoadFromFileOperation
    {
        public AssetBundle bundle;

        public AssetBundleLoadFromFileOperation(string _name)
        {
            var path = AssetManager.GetAssetPath(_name);

            if (path != null)
            {
                bundle = AssetBundle.LoadFromFile(path);
                if ((!Level.loading || AssetManager.enableLoadingLog) && AssetManager.enableLog)
                    Logger.LogDetail("AssetManager: Load {0} from {1}", _name, path);
            }
            else Logger.LogWarning("AssetManager: Could not find asset [{0}]", _name);
        }
    }

    internal class AssetBundleSaveOperation
    {
        public bool dispose = false;

        private UnityWebRequest m_request;
        private string m_path;
        private bool m_isDone;

        public AssetBundleSaveOperation(string _path, string _name, UnityWebRequest _request)
        {
            #if USE_ENCODED_BUNDLE_NAME
            _name = _name.GetMD5();
            #endif

            m_request = _request;

            m_path = _path + "/" + _name;
            if (File.Exists(m_path)) File.Delete(m_path);
            else
            {
                if (File.Exists(_path)) File.Delete(_path);
                if (!Directory.Exists(_path)) Directory.CreateDirectory(_path);
            }
        }

        public bool isDone() { return m_isDone; }
        public bool canDispose() { return dispose; }

        public IEnumerator DoSave()
        {
            var data = m_request.downloadHandler.data;
            var hash = data.GetMD5(false, 0, (int)m_request.downloadedBytes);
            var fs = File.Create(m_path, (int)m_request.downloadedBytes);
            fs.BeginWrite(data, 0, (int)m_request.downloadedBytes, OnSave, fs);

            yield return new WaitUntil(isDone);

            data = null;

            if ((!Level.loading || AssetManager.enableLoadingLog) && AssetManager.enableLog)
                Logger.LogDetail("AssetManager: File saved to {0}", m_path);

            HashListFile.ReplaceHashListFile(AssetManager.hashFileName, Path.GetFileName(m_path), hash);

            yield return new WaitUntil(canDispose);

            m_request.Dispose();
            m_request = null;

            //Logger.LogDetail("AssetManager: Web request handler disposed! File: {0}", m_path);
        }

        protected void OnSave(System.IAsyncResult r)
        {
            var fs = r.AsyncState as FileStream;
            fs.EndWrite(r);
            fs.Flush();
            fs.Close();
            fs = null;

            m_isDone = true;
        }
    }
}
