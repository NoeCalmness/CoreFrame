/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Helper component for atlas manager
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-04-10
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("HYLR/UI/Atlas Target")]
public class UIAtlasTarget : MonoBehaviour
{
    #region Static functions

    /// <summary>
    /// Set sprite to target
    /// </summary>
    /// <param name="o">Target gameobject</param>
    /// <param name="atlas">Sprite atlas</param>
    /// <param name="sprite">Sprite name</param>
    public static void SetSprite(GameObject o, string atlas, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        if (!o) return;
        SetSprite(o.GetComponentDefault<UIAtlasTarget>(), atlas, sprite, onComplete, useNativeSize);
    }

    /// <summary>
    /// Set sprite to target
    /// </summary>
    /// <param name="o">Target object</param>
    /// <param name="atlas">Sprite atlas</param>
    /// <param name="sprite">Sprite name</param>
    public static void SetSprite(Transform t, string atlas, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        if (!t) return;
        SetSprite(t.GetComponentDefault<UIAtlasTarget>(), atlas, sprite, onComplete, useNativeSize);
    }

    /// <summary>
    /// Set sprite to target
    /// </summary>
    /// <param name="o">Target image</param>
    /// <param name="atlas">Sprite atlas</param>
    /// <param name="sprite">Sprite name</param>
    public static void SetSprite(Image i, string atlas, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        if (!i) return;
        SetSprite(i.GetComponentDefault<UIAtlasTarget>(), atlas, sprite, onComplete, useNativeSize);
    }

    private static void SetSprite(UIAtlasTarget t, string atlas, string sprite, Action<UIAtlasTarget> onComplete, bool useNativeSize)
    {
        t.m_atlas = atlas;
        t.m_sprite = sprite;
        t.m_useNativeSize = useNativeSize;

        t.UpdateSprite(onComplete);
    }

    #endregion

    public string sprite
    {
        get { return m_sprite; }
        set
        {
            if (m_sprite == value) return;
            m_sprite = value;

            UpdateSprite();
        }
    }
    [SerializeField, Set("sprite")]
    private string m_sprite;

    public string atlas
    {
        get { return m_atlas; }
        set
        {
            if (m_atlas == value) return;
            m_atlas = value;

            UpdateSprite();
        }
    }
    [SerializeField, Set("atlas")]
    private string m_atlas;

    public bool useNativeSize
    {
        get { return m_useNativeSize; }
        set
        {
            if (m_useNativeSize == value) return;
            m_useNativeSize = value;

            m_image.SetNativeSize();
        }
    }
    [SerializeField, Set("useNativeSize")]
    private bool m_useNativeSize = false;

    /// <summary>
    /// Current loaded sprite
    /// </summary>
    public Sprite current { get { return m_current; } }
    private Sprite m_current = null;

    private Image m_image;

    private void Start()
    {
        UpdateSprite();
    }

    private void OnDestroy()
    {
        m_current = null;
        m_image   = null;
    }

    private void UpdateSprite(Action<UIAtlasTarget> onComplete = null)
    {
        if (!m_image) m_image = this.GetComponentDefault<Image>();

        var an = m_atlas; var sn = m_sprite;
        AtlasManager.GetSprite(m_atlas, m_sprite, s =>
        {
            if (!this || an != m_atlas || sn != m_sprite) return;

            m_current = s;
            m_image.enabled = m_current;
            m_image.sprite = m_current;

            if (m_useNativeSize) m_image.SetNativeSize();

            onComplete?.Invoke(this);
        });
    }
}
