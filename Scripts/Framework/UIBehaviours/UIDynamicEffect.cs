/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Single visible object container
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-03-02
 * 
 ***************************************************************************************************/

using UnityEngine;
using System.Collections;

[AddComponentMenu("HYLR/UI/Dynamic Effect")]
public class UIDynamicEffect : MonoBehaviour
{
    #region Static functions

    /// <summary>
    /// Load an effect from asset and add it to target
    /// </summary>
    /// <param name="target">target gameobject</param>
    /// <param name="effectName">target effect asset name</param>
    /// <param name="onLoaded">callback when effect loaded, null if load failed</param>
    public static void LoadEffect(RectTransform target, string effectName, System.Action<UIDynamicEffect, GameObject> onLoaded = null)
    {
        if (!target)
        {
            onLoaded?.Invoke(null, null);
            return;
        }

        var de = target.GetComponentDefault<UIDynamicEffect>();
        de.m_effectName = effectName;

        Level.PrepareAsset<GameObject>(effectName, e =>
        {
            if (de && effectName == de.m_effectName)
                de.UpdateEffect(e);

            onLoaded?.Invoke(de, e);
        });
    }

    /// <summary>
    /// Load an effect from asset and add it to target
    /// </summary>
    /// <param name="target">target gameobject</param>
    /// <param name="effectName">target effect asset name</param>
    /// <param name="onLoaded">callback when effect loaded, param is null if failed</param>
    public static void LoadEffect(RectTransform target, string effectName, string sortingLayer, int sortingOrder, System.Action<UIDynamicEffect, GameObject> onLoaded = null)
    {
        if (!target)
        {
            onLoaded?.Invoke(null, null);
            return;
        }

        var de = target.GetComponentDefault<UIDynamicEffect>();
        de.m_effectName = effectName;
        de.m_sortingLayer = sortingLayer;
        de.m_sortingOrder = sortingOrder;

        Level.PrepareAsset<GameObject>(effectName, e =>
        {
            if (de && effectName == de.effectName)
                de.UpdateEffect(e);

            onLoaded?.Invoke(de, e);
        });
    }

    /// <summary>
    /// Load an effect from asset and add it to target
    /// </summary>
    /// <param name="effectName">target effect asset name</param>
    /// <param name="onLoaded">callback when effect loaded, param is null if failed</param>
    /// <param name="targets">target gameobjects</param>
    public static void LoadEffect(string effectName, System.Action<GameObject> onLoaded = null, params Transform[] targets)
    {
        if (targets == null || targets.Length < 1)
        {
            onLoaded?.Invoke(null);
            return;
        }

        foreach (var target in targets)
        {
            if (!target) continue;
            var de = target.GetComponentDefault<UIDynamicEffect>();
            de.m_effectName = effectName;
        }

        Level.PrepareAsset<GameObject>(effectName, e =>
        {
            foreach (var target in targets)
            {
                if (!target) continue;
                var de = target.GetComponent<UIDynamicEffect>();

                if (!de || de.effectName != effectName) continue;
                de.UpdateEffect(e);
            }
            onLoaded?.Invoke(e);
        });
    }

    /// <summary>
    /// Load an effect from asset and add it to target
    /// </summary>
    /// <param name="effectName">target effect asset name</param>
    /// <param name="onLoaded">callback when effect loaded, param is null if failed</param>
    /// <param name="targets">target gameobjects</param>
    public static void LoadEffect(string effectName, string sortingLayer, int sortingOrder, System.Action<GameObject> onLoaded = null, params Transform[] targets)
    {
        if (targets == null || targets.Length < 1)
        {
            onLoaded?.Invoke(null);
            return;
        }

        foreach (var target in targets)
        {
            if (!target) continue;
            var de = target.GetComponentDefault<UIDynamicEffect>();
            de.m_effectName = effectName;
            de.m_sortingLayer = sortingLayer;
            de.m_sortingOrder = sortingOrder;
        }

        Level.PrepareAsset<GameObject>(effectName, e =>
        {
            foreach (var target in targets)
            {
                if (!target) continue;
                var de = target.GetComponent<UIDynamicEffect>();

                if (!de || de.effectName != effectName) continue;
                de.UpdateEffect(e);
            }
            onLoaded?.Invoke(e);
        });
    }

    /// <summary>
    /// Load an effect from asset and add it to target
    /// </summary>
    /// <param name="target">target gameobject</param>
    /// <param name="effectName">target effect asset name</param>
    /// <param name="onLoaded">callback when effect loaded, param is null if failed</param>
    public static void LoadEffect(GameObject target, string effectName, System.Action<UIDynamicEffect, GameObject> onLoaded = null)
    {
        if (!target)
        {
            onLoaded?.Invoke(null, null);
            return;
        }

        var de = target.GetComponentDefault<UIDynamicEffect>();
        de.m_effectName = effectName;

        Level.PrepareAsset<GameObject>(effectName, e =>
        {
            if (de && de.effectName == effectName)
                de.UpdateEffect(e);

            onLoaded?.Invoke(de, e);
        });
    }

    /// <summary>
    /// Load an effect from asset and add it to target
    /// </summary>
    /// <param name="target">target gameobject</param>
    /// <param name="effectName">target effect asset name</param>
    /// <param name="onLoaded">callback when effect loaded, param is null if failed</param>
    public static void LoadEffect(GameObject target, string effectName, string sortingLayer, int sortingOrder, System.Action<UIDynamicEffect, GameObject> onLoaded = null)
    {
        if (!target)
        {
            onLoaded?.Invoke(null, null);
            return;
        }

        var de = target.GetComponentDefault<UIDynamicEffect>();
        de.m_effectName = effectName;
        de.m_sortingLayer = sortingLayer;
        de.m_sortingOrder = sortingOrder;

        Level.PrepareAsset<GameObject>(effectName, e =>
        {
            if (de && de.effectName == effectName)
                de.UpdateEffect(e);

            onLoaded?.Invoke(de, e);
        });
    }


    /// <summary>
    /// Load an effect from asset and add it to target
    /// </summary>
    /// <param name="effectName">target effect asset name</param>
    /// <param name="onLoaded">callback when effect loaded, param is null if failed</param>
    /// <param name="targets">target gameobjects</param>
    public static void LoadEffect(string effectName, System.Action<GameObject> onLoaded = null, params GameObject[] targets)
    {
        if (targets == null || targets.Length < 1)
        {
            onLoaded?.Invoke(null);
            return;
        }

        foreach (var target in targets)
        {
            if (!target) continue;
            var de = target.GetComponentDefault<UIDynamicEffect>();
            de.m_effectName = effectName;
        }

        Level.PrepareAsset<GameObject>(effectName, e =>
        {
            foreach (var target in targets)
            {
                if (!target) continue;
                var de = target.GetComponent<UIDynamicEffect>();

                if (!de || de.effectName != effectName) continue;
                de.UpdateEffect(e);
            }
            onLoaded?.Invoke(e);
        });
    }

    /// <summary>
    /// Load an effect from asset and add it to target
    /// </summary>
    /// <param name="effectName">target effect asset name</param>
    /// <param name="onLoaded">callback when effect loaded, param is null if failed</param>
    /// <param name="targets">target gameobjects</param>
    public static void LoadEffect(string effectName, string sortingLayer, int sortingOrder, System.Action<GameObject> onLoaded = null, params GameObject[] targets)
    {
        if (targets == null || targets.Length < 1)
        {
            onLoaded?.Invoke(null);
            return;
        }

        foreach (var target in targets)
        {
            if (!target) continue;
            var de = target.GetComponentDefault<UIDynamicEffect>();
            de.m_effectName = effectName;
            de.m_sortingLayer = sortingLayer;
            de.m_sortingOrder = sortingOrder;
        }

        Level.PrepareAsset<GameObject>(effectName, e =>
        {
            foreach (var target in targets)
            {
                if (!target) continue;
                var de = target.GetComponent<UIDynamicEffect>();

                if (!de || de.effectName != effectName) continue;
                de.UpdateEffect(e);
            }
            onLoaded?.Invoke(e);
        });
    } 
    
    #endregion

    public string effectName
    {
        get { return m_effectName; }
        set
        {
            if (m_effectName == value) return;
            m_effectName = value;

            UpdateEffect();
        }
    }
    [SerializeField, Set("effectName")]
    private string m_effectName;
    public string sortingLayer
    {
        get { return m_sortingLayer; }
        set
        {
            if (m_sortingLayer == value) return;
            m_sortingLayer = value;

            var o = !Game.started ? gameObject : m_current;
            Util.SetEffectSortingLayer(o, m_sortingLayer, m_sortingOrder);
        }
    }
    [SerializeField, Set("sortingLayer")]
    private string m_sortingLayer = Layers.SORTING_DEFAULT;
    public int sortingOrder
    {
        get { return m_sortingOrder; }
        set
        {
            if (m_sortingOrder == value) return;
            m_sortingOrder = value;

            var o = !Game.started ? gameObject : m_current;
            Util.SetEffectSortingLayer(o, m_sortingLayer, m_sortingOrder);
        }
    }
    [SerializeField, Set("sortingOrder")]
    private int m_sortingOrder = 1;
    public bool realtime
    {
        get { return m_realtime; }
        set
        {
            if (m_realtime == value) return;
            m_realtime = value;

            gameObject.SetRealtimeParticle(m_realtime);
        }
    }
    [SerializeField, Set("realtime")]
    private bool m_realtime = false;

    /// <summary>
    /// Current loaded effect object
    /// </summary>
    public GameObject current { get { return m_current; } }
    private GameObject m_current = null;

    private RealtimeParticle m_realtimeParticle = null;

    public System.Action<UIDynamicEffect> onLoaded = null;

    private void Start()
    {
        m_current = transform.Find(m_effectName)?.gameObject;
        if (m_current && m_current.name == m_effectName)
        {
            UpdateEffectLayers();
            onLoaded?.Invoke(this);
            return;
        }
        UpdateEffect();
    }

    private void OnDestroy()
    {
        BackEffect();

        m_current = null;
        m_realtimeParticle = null;
        onLoaded = null;
    }

    private void BackEffect()
    {
        if (m_current && m_effectName != null && (m_effectName.StartsWith("effect_") || m_effectName.StartsWith("eff_")))
            Level.BackEffect(m_current);
        else Destroy(m_current);
    }

    private void UpdateEffect()
    {
        var n = m_effectName;
        Level.PrepareAsset<GameObject>(m_effectName, e =>
        {
            if (!this || m_effectName != n) return;

            UpdateEffect(e);
            onLoaded?.Invoke(this);
        });
    }

    private void UpdateEffect(GameObject e)
    {
        BackEffect();

        m_current = Level.current ? Level.GetPreloadObjectFromPool(m_effectName) : e ? Instantiate(e) : null;
        UpdateEffectLayers();
    }

    private void UpdateEffectLayers()
    {
        if (!m_current) return;

        Util.AddChild(transform, m_current.transform);
        m_current.name = m_effectName;
        m_current.gameObject.SetActive(true);
        Util.SetEffectSortingLayer(m_current, m_sortingLayer, m_sortingOrder);

        if (!m_realtimeParticle && m_realtime) m_realtimeParticle = this.GetComponentDefault<RealtimeParticle>();
        if (m_realtimeParticle) m_realtimeParticle.SetRealtime(m_realtime);
    }
}
