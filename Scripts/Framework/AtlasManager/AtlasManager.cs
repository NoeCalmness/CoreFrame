/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Atlas manager.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-01-30
 * 
 ***************************************************************************************************/

using AssetBundles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manage all in game atlas
/// </summary>
[AddComponentMenu("HYLR/Utilities/AtlasManager")]
public class AtlasManager : SingletonBehaviour<AtlasManager>
{
    #region Static functions

    private static bool m_initialized = false;

    private static Dictionary<string, Atlas> m_loadedAtlas = new Dictionary<string, Atlas>();

    /// <summary>
    /// Initialize Atlas Manager
    /// </summary>
    public static void Initialize()
    {
        if (m_initialized) return;
        m_initialized = true;
    }

    #region Helpers

    /// <summary>
    /// Get a loaded atlas
    /// </summary>
    /// <param name="atlas"></param>
    /// <returns></returns>
    public static Atlas GetAtlas(string atlas)
    {
        return atlas == null ? null : m_loadedAtlas.Get(atlas);
    }

    /// <summary>
    /// Check if atlas loaded
    /// </summary>
    /// <param name="atlas"></param>
    /// <returns></returns>
    public static bool IsAtlasLoaded(string atlas)
    {
        return atlas != null && m_loadedAtlas.ContainsKey(atlas);
    }

    /// <summary>
    /// Get a loaded atlas sprite
    /// </summary>
    /// <param name="atlas"></param>
    /// <param name="sprite"></param>
    /// <returns></returns>
    public static Sprite GetSprite(string atlas, string sprite)
    {
        var cache = GetAtlas(atlas);
        return cache?.GetSprite(sprite);
    }

    /// <summary>
    /// Get a sprite async
    /// </summary>
    /// <param name="atlas"></param>
    /// <param name="sprite"></param>
    /// <param name="oncComplete"></param>
    public static void GetSprite(string atlas, string sprite, Action<Sprite> oncComplete)
    {
        var cache = GetSprite(atlas, sprite);
        if (cache)
        {
            oncComplete?.Invoke(cache);
            return;
        }

        PrepareAtlas(atlas, a =>
        {
            cache = a?.GetSprite(sprite);
            oncComplete?.Invoke(cache);
        });
    }

    #endregion

    #region Load operations

    /// <summary>
    /// Prepare an atlas
    /// </summary>
    /// <param name="atlas"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    public static Coroutine PrepareAtlas(List<string> atlas, Action onComplete)
    {
        return instance.StartCoroutine(instance._LoadAtlas(atlas, onComplete));
    }

    /// <summary>
    /// Prepare an atlas
    /// </summary>
    /// <param name="atlas"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    public static Coroutine PrepareAtlas(string atlas, Action<Atlas> onComplete)
    {
        return instance.StartCoroutine(instance._LoadAtlas(atlas, onComplete));
    }

    /// <summary>
    /// Load atlas async
    /// </summary>
    /// <param name="atlas"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    public static IEnumerator LoadAtlas(List<string> atlas, Action onComplete = null)
    {
        return instance._LoadAtlas(atlas, onComplete);
    }

    /// <summary>
    /// Load atlas async
    /// </summary>
    /// <param name="atlas"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    public static IEnumerator LoadAtlas(string atlas, Action<Atlas> onComplete = null)
    {
        return instance._LoadAtlas(atlas, onComplete);
    }

    public static void UnloadAtlas(string atlas)
    {
        m_loadedAtlas.Remove(atlas);

        AssetManager.UnloadAssetBundle(atlas);
        Resources.UnloadUnusedAssets();
    }

    public static void UnloadAllAtlas()
    {
        foreach (var k in m_loadedAtlas.Keys) AssetManager.UnloadAssetBundle(k);
        Resources.UnloadUnusedAssets();
    }

    #endregion

    #endregion

    public IEnumerator _LoadAtlas(List<string> atlas, Action onComplete = null)
    {
        if (atlas == null || atlas.Count < 1)
        {
            onComplete?.Invoke();
            yield break;
        }

        atlas.Distinct();
        atlas.RemoveAll(a => string.IsNullOrEmpty(a) || string.IsNullOrWhiteSpace(a));

        while (atlas.Count > 0)
        {
            var a = atlas[0];

            if (!m_loadedAtlas.ContainsKey(a))
            {
                var load = AssetManager.LoadAssetAsync(a, a, typeof(GameObject));

                if (load != null)
                {
                    yield return load;

                    var obj = load.GetAsset<GameObject>();
                    var cache = obj?.GetComponent<Atlas>();
                    if (cache)
                    {
                        obj.name = obj.name.ToLower(); // Always use lowercase asset name
                        m_loadedAtlas.Set(a, cache);
                        cache.Initialize();
                    }
                }
            }

            atlas.RemoveAt(0);
        }

        onComplete?.Invoke();
    }

    public IEnumerator _LoadAtlas(string atlas, Action<Atlas> onComplete = null)
    {
        if (string.IsNullOrEmpty(atlas))
        {
            onComplete?.Invoke(null);
            yield break;
        }

        var cache = m_loadedAtlas.Get(atlas);
        if (cache != null)
        {
            onComplete?.Invoke(cache);
            yield break;
        }

        var load = AssetManager.LoadAssetAsync(atlas, atlas, typeof(GameObject));
        if (load == null)
        {
            onComplete?.Invoke(null);
            yield break;
        }

        yield return load;

        var obj = load.GetAsset<GameObject>();
        cache = obj?.GetComponent<Atlas>();
        if (cache)
        {
            obj.name = obj.name.ToLower(); // Always use lowercase asset name
            m_loadedAtlas.Set(atlas, cache);
            cache.Initialize();
        }

        onComplete?.Invoke(cache);
    }

    #region Editor helper

    [SerializeField]
    private int e_AtlasListRefreshRate = 50;
    [SerializeField]
    private List<string> e_LoadedAtlas = new List<string>();

#if UNITY_EDITOR

    private int e_AtlasListRefreshCount    = 50;

    private void LateUpdate()
    {
        if (++e_AtlasListRefreshCount >= e_AtlasListRefreshRate)
        {
            e_AtlasListRefreshCount = 0;

            e_LoadedAtlas.Clear();
            foreach (var pair in m_loadedAtlas)
                e_LoadedAtlas.Add(pair.Key);
        }
    }
#endif

    #endregion
}
