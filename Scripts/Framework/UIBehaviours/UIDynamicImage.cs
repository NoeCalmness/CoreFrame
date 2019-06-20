/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Simple component to help load dynamic image from asset
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-03-06
 * 
 ***************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[AddComponentMenu("HYLR/UI/Dynamic Image")]
public class UIDynamicImage : MonoBehaviour
{
    #region Static functions

    #region Load operations

    /// <summary>
    /// Load an image from created texture data and set it to target
    /// Target can use RawImage or Image component
    /// </summary>
    /// <param name="target">target gameobject</param>
    /// <param name="name">target image name</param>
    /// <param name="texture">created texture data</param>
    /// <param name="useNativeSize">Use origin image size ?</param>
    public static void LoadImageCreated(Transform target, string name, Texture2D texture, bool useNativeSize = false)
    {
        if (!target) return;
        LoadImageCreated(target.gameObject, name, texture, useNativeSize);
    }

    /// <summary>
    /// Load an image from created texture data and set it to target
    /// Target can use RawImage or Image component
    /// </summary>
    /// <param name="target">target gameobject</param>
    /// <param name="name">target image name</param>
    /// <param name="texture">created texture data</param>
    /// <param name="useNativeSize">Use origin image size ?</param>
    public static void LoadImageCreated(GameObject target, string name, Texture2D texture, bool useNativeSize = false)
    {
        if (!target) return;

        var di = target.GetComponentDefault<UIDynamicImage>();
        di.UpdateImage(name, texture, useNativeSize);
    }

    /// <summary>
    /// Load an image from asset or url and set it to target
    /// Target can use RawImage or Image component
    /// </summary>
    /// <param name="target">target gameobject</param>
    /// <param name="image">target image asset name</param>
    /// <param name="onLoaded">Callback when image loaded, if failed, texture will be null</param>
    /// <param name="useNativeSize">Use origin image size ?</param>
    public static void LoadImage(Transform target, string image, Action<UIDynamicImage, Texture2D> onLoaded = null, bool useNativeSize = false)
    {
        LoadImage(target?.gameObject, image, onLoaded, useNativeSize);
    }

    /// <summary>
    /// Load an image from asset or url and set it to target
    /// Target can use RawImage or Image component
    /// </summary>
    /// <param name="target">target gameobject</param>
    /// <param name="image">target image asset name or url</param>
    /// <param name="onLoaded">Callback when image loaded, if failed, texture will be null</param>
    /// <param name="useNativeSize">Use origin image size ?</param>
    public static void LoadImage(GameObject target, string image, Action<UIDynamicImage, Texture2D> onLoaded = null, bool useNativeSize = false)
    {
        if (!target)
        {
            onLoaded?.Invoke(null,  null);
            return;
        }

        var di = target.GetComponentDefault<UIDynamicImage>();
        di.UpdateImage(image, onLoaded, useNativeSize);
    }

    /// <summary>
    /// Load an image from asset or url and set it to target
    /// Target can use RawImage or Image component
    /// </summary>
    /// <param name="image">target image asset name or url</param>
    /// <param name="onLoaded">Callback when image loaded, if failed, texture will be null</param>
    /// <param name="targets">target list</param>
    public static void LoadImage(string image, Action<Texture2D> onLoaded = null, params Transform[] targets)
    {
        LoadImage(image, onLoaded, false, targets);
    }

    /// <summary>
    /// Load an image from asset or url and set it to target
    /// Target can use RawImage or Image component
    /// </summary>
    /// <param name="image">target image asset name or url</param>
    /// <param name="onLoaded">Callback when image loaded, if failed, texture will be null</param>
    /// <param name="targets">target list</param>
    public static void LoadImage(string image, Action<Texture2D> onLoaded = null, params GameObject[] targets)
    {
        LoadImage(image, onLoaded, false, targets);
    }

    /// <summary>
    /// Load an image from asset or url and set it to target
    /// Target can use RawImage or Image component
    /// </summary>
    /// <param name="image">target image asset name or url</param>
    /// <param name="onLoaded">Callback when image loaded, if failed, texture will be null</param>
    /// <param name="useNativeSize">Use origin image size ?</param>
    /// <param name="targets">target list</param>
    public static void LoadImage(string image, Action<Texture2D> onLoaded, bool useNativeSize, params Transform[] targets)
    {
        if (targets == null || targets.Length < 1)
        {
            onLoaded?.Invoke(null);
            return;
        }

        var isWeb = !string.IsNullOrEmpty(image) && (image.StartsWith("http://") || image.StartsWith("https://"));

        foreach (var target in targets)
        {
            if (!target) continue;
            var di = target.GetComponentDefault<UIDynamicImage>();
            di.m_isWeb = isWeb;
            di.m_useNativeSize = useNativeSize;
            di.m_image = image;
        }

        var n = image;
        Action<Texture2D, byte[]> onComplete = (t, d) =>
        {
            foreach (var target in targets)
            {
                if (!target) continue;
                var di = target.GetComponentDefault<UIDynamicImage>();

                if (!di || di.image != n) continue;
                di.UpdateImage(t, d);
            }
            onLoaded?.Invoke(t);
        };

        if (isWeb)
        {
            if (Root.instance) Root.instance.StartCoroutine(RequestImage(n, onComplete));
            return;
        }

        Level.PrepareAsset<Texture2D>(n, t => onComplete(t, null));
    }

    /// <summary>
    /// Load an image from asset or url and set it to target
    /// Target can use RawImage or Image component
    /// </summary>
    /// <param name="image">target image asset name or url</param>
    /// <param name="onLoaded">Callback when image loaded, if failed, texture will be null</param>
    /// <param name="useNativeSize">Use origin image size ?</param>
    /// <param name="targets">target list</param>
    public static void LoadImage(string image, Action<Texture2D> onLoaded, bool useNativeSize, params GameObject[] targets)
    {
        if (targets == null || targets.Length < 1)
        {
            onLoaded?.Invoke(null);
            return;
        }

        var isWeb = !string.IsNullOrEmpty(image) && (image.StartsWith("http://") || image.StartsWith("https://"));

        foreach (var target in targets)
        {
            if (!target) continue;
            var di = target.GetComponentDefault<UIDynamicImage>();
            di.m_isWeb = isWeb;
            di.m_useNativeSize = useNativeSize;
            di.m_image = image;
        }

        var n = image;
        Action<Texture2D, byte[]> onComplete = (t, d) =>
        {
            foreach (var target in targets)
            {
                if (!target) continue;
                var di = target.GetComponentDefault<UIDynamicImage>();

                if (!di || di.image != n) continue;
                di.UpdateImage(t, d);
            }
            onLoaded?.Invoke(t);
        };

        if (isWeb)
        {
            if (Root.instance) Root.instance.StartCoroutine(RequestImage(n, onComplete));
            return;
        }

        Level.PrepareAsset<Texture2D>(n, t => onComplete(t, null));
    }

    #endregion

    #region Web cache

    private class DownloadHolder : PoolObject<DownloadHolder>
    {
        public string url;
        public UnityWebRequest request;
        public Action<Texture2D, byte[]> onComplete;
        public bool isDone = true;

        public static DownloadHolder Create(string url, UnityWebRequest request, Action<Texture2D, byte[]> onComplete)
        {
            var d = Create();
            d.url = url;
            d.request = request;
            d.onComplete = onComplete;
            d.isDone = false;
            return d;
        }

        protected DownloadHolder() { }
        protected override void OnDestroy()
        {
            request?.Dispose();
            isDone = true;  // Destroyed queue item always complete
            url = null;
            request = null;
            onComplete = null;
        }
    }

    private class CacheHolder : PoolObject<CacheHolder>
    {
        public string url;
        public Texture2D texture;
        public byte[] data;

        public int refCount
        {
            get { return m_refCount; }
            set
            {
                if (m_refCount == value) return;
                m_refCount = value;
                if (m_refCount < 1) Destroy();
            }
        }
        private int m_refCount = 0;

        public static CacheHolder Create(string url, Texture2D texture, byte[] data)
        {
            var d = Create();
            d.url = url;
            d.texture = texture;
            d.data = data;
            d.refCount = 1;
            return d;
        }

        protected CacheHolder() { }

        public void Destroy(bool clearTexture)
        {
            if (!clearTexture) texture = null;
            Destroy();
        }

        protected override void OnInitialize()
        {
            m_refCount = 0;
        }

        protected override void OnDestroy()
        {
            if (texture) DestroyImmediate(texture);
            url = null;
            texture = null;
            data = null;
            m_refCount = 0;
        }
    }

    private static List<DownloadHolder> m_downloadQueue = new List<DownloadHolder>();
    private static List<CacheHolder> m_cached = new List<CacheHolder>();

    /// <summary>
    /// Download an image from web
    /// </summary>
    /// <param name="url">http url, must start with http:// or https://</param>
    /// <param name="onComplete">callback when download complete</param>
    /// <param name="timeout">request timeout</param>
    /// <returns></returns>
    public static IEnumerator RequestImage(string url, Action <Texture2D, byte[]> onComplete = null, int timeout = 10)
    {
        var ourl = url;
        url = url.ToLower();

        var cache = m_cached.Find(c => c.url == url);
        if (cache != null)
        {
            onComplete?.Invoke(cache.texture, cache.data);
            yield break;
        }

        var n = m_downloadQueue.Find(d => d.url == url);
        if (n != null)
        {
            n.onComplete += onComplete;
            var wait = new WaitUntil(() => n.isDone);
            yield return wait;
        }
        else
        {
            var request = UnityWebRequestTexture.GetTexture(url, true);
            request.timeout = timeout;

            n = DownloadHolder.Create(url, request, onComplete);
            m_downloadQueue.Add(n);

            yield return request.SendWebRequest();

            m_downloadQueue.Remove(n);

            Texture2D t = null;
            byte[] data = null;

            if (request.isHttpError || request.isNetworkError) Logger.LogError("UIDynamicImage::RequestImage: Failed to download image from [{0}], err: {1}", url, request.error);
            else if (request.responseCode != 200) Logger.LogError("UIDynamicImage::RequestImage: Failed to download image from [{0}], invalid response code: {1}", url, request.error);
            else
            {
                t = DownloadHandlerTexture.GetContent(request);
                if (!t) Logger.LogError("UIDynamicImage::RequestImage: Failed to decode image data from [{0}], data length: [{1}]", url, request.downloadedBytes);
                else 
                {
                    t.name = ourl;
                    data = request.downloadHandler.data;
                    Logger.LogInfo("UIDynamicImage::RequestImage: Download complete, url:[{0}]", ourl);
                }
            }

            n.onComplete?.Invoke(t, data);
            n.Destroy();
        }
    }

    /// <summary>
    /// Remove an image web cache
    /// </summary>
    /// <param name="url">cached url</param>
    /// <param name="destroy">if true, destroy cached texture data</param>
    public static byte[] RemoveCache(string url, bool destroy = true)
    {
        byte[] data = null;
        var cache = m_cached.Find(c => string.Compare(c.url, url, true) == 0);
        if (cache)
        {
            data = cache.data;
            cache.Destroy(destroy);
            m_cached.Remove(cache);
        }
        return data;
    }

    #endregion

    #endregion

    public string image
    {
        get { return m_image; }
        set
        {
            if (string.Compare(m_image, value, true) == 0) return;  // Always ignore case
            m_image = value;

            m_isWeb = !string.IsNullOrEmpty(m_image) && (m_image.StartsWith("http://") || m_image.StartsWith("https://"));

            UpdateImage();
        }
    }
    [SerializeField, Set("image")]
    private string m_image;

    public bool useNativeSize
    {
        get { return m_useNativeSize; }
        set
        {
            if (m_useNativeSize == value) return;
            m_useNativeSize = value;

            if (m_useNativeSize)
            {
                if (m_ri && m_ri.texture) m_ri.SetNativeSize();
                if (m_i && m_i.sprite) m_i.SetNativeSize();
            }
        }
    }
    [SerializeField, Set("useNativeSize")]
    private bool m_useNativeSize;

    public int timeout;

    private RawImage m_ri;
    private Image m_i;

    private Action<UIDynamicImage, Texture2D> m_onLoaded = null;

    private bool m_isWeb = false;
    private bool m_initialized = false;
    private bool m_loading = false;

    private CacheHolder m_refCache;

    private void Start()
    {
        if (m_initialized) return;  // UpdateImage called before start

        m_isWeb = !string.IsNullOrEmpty(m_image) && (m_image.StartsWith("http://") || m_image.StartsWith("https://"));

        m_ri = GetComponent<RawImage>();
        if (!m_ri) m_i = this.GetComponentDefault<Image>();

        UpdateImage();
    }

    private void Initialize()
    {
        if (!m_initialized)
        {
            m_ri = GetComponent<RawImage>();
            if (!m_ri) m_i = this.GetComponentDefault<Image>();

            m_initialized = true;
        }
    }

    private void OnDestroy()
    {
        if (m_refCache)
        {
            m_refCache.refCount--;
            if (!m_refCache) m_cached.Remove(m_refCache);
        }

        m_i        = null;
        m_ri       = null;
        m_refCache = null;
        m_onLoaded = null;
    }

    private void RequestImage(Action<Texture2D, byte[]> onComplete)
    {
        if (Root.instance) Root.instance.StartCoroutine(RequestImage(m_image, onComplete, timeout < 1 ? 1 : timeout));
        else StartCoroutine(RequestImage(m_image, onComplete, timeout < 1 ? 1 : timeout));
    }

    public void UpdateImage(string _image, Action<UIDynamicImage, Texture2D> onLoaded = null, bool _useNativeSize = false)
    {
        if (string.Compare(m_image, _image, true) == 0)
        {
            useNativeSize = _useNativeSize;
            if (onLoaded != null)
            {
                if (!m_loading)
                {
                    var t = m_ri ? m_ri.texture as Texture2D : m_i && m_i.sprite ? m_i.sprite.texture : null;
                    onLoaded?.Invoke(this, t);
                }
                else
                {
                    m_onLoaded -= onLoaded;
                    m_onLoaded += onLoaded;
                }
            }
            return;
        }
        m_image         = _image;
        m_isWeb         = !string.IsNullOrEmpty(m_image) && (m_image.StartsWith("http://") || m_image.StartsWith("https://"));
        m_useNativeSize = _useNativeSize;

        UpdateImage(onLoaded);
    }

    public void UpdateImage(string name, Texture2D texture, bool _useNativeSize)
    {
        m_image = name;
        m_isWeb = false;
        m_useNativeSize = _useNativeSize;

        UpdateImage(texture, null);
    }

    private void UpdateImage(Action<UIDynamicImage, Texture2D> onLoaded = null)
    {
        if (onLoaded != null)
        {
            m_onLoaded -= onLoaded;
            m_onLoaded += onLoaded;
        }

        var n = m_image;
        Action<Texture2D, byte[]> onComplete = (t, d) =>
        {
            if (m_i) m_i.enabled = true;
            if (m_ri) m_ri.enabled = true;

            m_loading = false;

            if (this && m_image == n) UpdateImage(t, d);

            m_onLoaded?.Invoke(this, t);
            m_onLoaded = null;
        };

        Initialize();

        if (m_i) m_i.enabled = false;
        if (m_ri) m_ri.enabled = false;

        if (string.IsNullOrEmpty(n)) onComplete(null, null);
        else
        {
            m_loading = true;
            if (m_isWeb) RequestImage(onComplete);
            else Level.PrepareAsset<Texture2D>(n, t => onComplete(t, null));
        }
    }

    private void UpdateImage(Texture2D t, byte[] data)
    {
        Initialize();

        Texture ot = null;

        if (m_ri)
        {
            ot = m_ri.texture;

            m_ri.texture = t;
            m_ri.enabled = t;
            if (m_useNativeSize && t) m_ri.SetNativeSize();
        }
        else if (m_i)
        {
            ot = m_i.sprite ? m_i.sprite.texture : null;

            var ss = t ? t.ToSprite() : null;
            m_i.sprite = ss;
            m_i.enabled = ss;
            if (m_useNativeSize && ss) m_i.SetNativeSize();
        }

        if (ot != t)
        {
            if (m_refCache)
            {
                m_refCache.refCount--;
                if (!m_refCache) m_cached.Remove(m_refCache);
            }
            m_refCache = null;

            if (m_isWeb && t)
            {
                var l = m_image.ToLower();
                var cache = m_cached.Find(c => c.url == l);
                if (cache == null)
                {
                    cache = CacheHolder.Create(l, t, data);
                    m_cached.Add(cache);
                }
                else cache.refCount++;
                m_refCache = cache;
            }
        }
    }
}
