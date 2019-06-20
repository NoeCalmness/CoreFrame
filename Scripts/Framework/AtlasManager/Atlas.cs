/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Atlas sprite holder.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-01-30
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class Atlas : MonoBehaviour
{
    [SerializeField]
    private Sprite[] m_sprites = new Sprite[] { };

    private Dictionary<string, Sprite> m_spriteMap = new Dictionary<string, Sprite>();
    
    /// <summary>
    /// Get sprite by name
    /// </summary>
    /// <param name="spriteName"></param>
    /// <returns></returns>
    public Sprite GetSprite(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName)) return null;

        return m_spriteMap.Get(spriteName);
    }

    /// <summary>
    /// Get all sprites
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public List<Sprite> GetSprites(List<Sprite> list = null)
    {
        if (list == null) list = new List<Sprite>();
        list.AddRange(m_sprites);
        return list;
    }

    /// <summary>
    /// Get all sprites match func
    /// </summary>
    /// <param name="func"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public List<Sprite> GetSprites(System.Func<Sprite, bool> func, List<Sprite> list = null)
    {
        if (list == null) list = new List<Sprite>();
        if (func == null) return list;

        foreach (var s in m_sprites)
            if (func(s)) list.Add(s);

        return list;
    }

    public void Initialize()
    {
        m_spriteMap.Clear();
        foreach (var s in m_sprites)
        {
            if (!s) continue;

            if (m_spriteMap.Get(s.name))
                Logger.LogWarning("Atlas::Awake: Atlas {0} has duplicated sprite name, override.", name);

            m_spriteMap.Set(s.name, s);
        }
    }

    private void OnDestroy()
    {
        m_spriteMap.Clear();
        m_sprites.Clear();

        m_sprites   = null;
        m_spriteMap = null;
    }
}
