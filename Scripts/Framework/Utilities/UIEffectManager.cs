/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-11-22
 * 
 ***************************************************************************************************/

using UnityEngine;
using System;
using AssetBundles;
using System.Collections;

[AddComponentMenu("HYLR/Utilities/UI Effect Manager")]
public class UIEffectManager : SingletonBehaviour<UIEffectManager>
{
    /// <summary>
    /// Play UI effect on node
    /// </summary>
    /// <param name="node">parent node</param>
    /// <param name="effect">effect object</param>
    /// <returns></returns>
    public static void PlayEffect(RectTransform node, GameObject effect, float delay = 0, Action<GameObject> onPlay = null)
    {
        if (!node || !effect) return;

        if (delay > 0) instance.StartCoroutine(_PlayEffect(node, effect, delay, onPlay));
        else
        {
            effect.SetActive(true);
            Util.AddChild(node, effect.transform, false);

            onPlay?.Invoke(effect);
        }
    }

    /// <summary>
    /// Play UI effect on node
    /// </summary>
    /// <param name="node">parent node</param>
    /// <param name="effect">effect asset name</param>
    /// <param name="name">effect name</param>
    /// <param name="onLoaded">Callback when effect loaded, note if effect load failed, param is null, if parent node destroyed, callback will not invoke</param>
    /// <returns></returns>
    public static void PlayEffectAsync(RectTransform node, string effect, string name = null, float delay = 0, Action<GameObject> onLoaded = null)
    {
        if (!node) return;

        instance.StartCoroutine(_PlayEffectAsync(node, effect, name, delay, onLoaded));
    }

    /// <summary>
    /// Play UI effect on node
    /// </summary>
    /// <param name="node">parent node</param>
    /// <param name="effect">effect asset name</param>
    /// <param name="name">effect name</param>
    /// <param name="onLoaded">Callback when effect loaded, note if effect load failed, param is null, if parent node destroyed, callback will not invoke</param>
    /// <returns></returns>
    private static IEnumerator _PlayEffectAsync(RectTransform node, string effect, string name = null, float delay = 0, Action<GameObject> onLoaded = null)
    {
        var obj = Level.GetPreloadObject(effect);

        if (obj)
        {
            obj.SetActive(false);
            obj.name = string.IsNullOrEmpty(name) ? effect : name;
            PlayEffect(node, obj, delay, onLoaded);

            yield break;
        }

        var t = Time.time;

        var op = AssetManager.LoadAssetAsync(effect, effect, typeof(GameObject));
        yield return op;

        var ef = op != null ? op.GetAsset<GameObject>() : null;
        if (!ef)
        {
            Logger.LogWarning("UIEffectManager::PlayEffectAsync: Play effect {0} failed, could not load asset bundle [{1}]", name, effect);

            yield break;
        }

        obj = GameObject.Instantiate(ef);
        AssetManager.UnloadAssetBundle(name);

        if (!node)   // Parent node destroyed duraing effect loading, destroy effect immediately
        {
            GameObject.DestroyImmediate(obj);
            yield break;
        }

        obj.SetActive(false);
        obj.name = string.IsNullOrEmpty(name) ? effect : name;
        PlayEffect(node, obj, delay - Time.time + t, onLoaded);
    }

    private static IEnumerator _PlayEffect(RectTransform node, GameObject effect, float delay = 0, Action<GameObject> onPlay = null)
    {
        if (delay <= 0)
        {
            PlayEffect(node, effect, 0, onPlay);
            yield break;
        }

        var wait = new WaitForSeconds(delay);
        yield return wait;

        PlayEffect(node, effect, 0, onPlay);
    }
}