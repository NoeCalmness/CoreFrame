/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Behaviour script of Scene
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.0
 * Created:  2017-03-07
 * 
 ***************************************************************************************************/

using UnityEngine;

public class LevelBehavior : MonoBehaviour
{
    public Level level
    {
        get { return m_level; }
        set
        {
            if (!value)
            {
                Logger.LogError("LevelBehaviour: level can not be null, ignore");
                return;
            }

            m_level = value;

            FixCameras();
        }
    }
    private Level m_level = null;

    /// <summary>
    /// pve多场景时，可能会销毁之前的场景，而此时并不需要销毁level
    /// </summary>
    public bool canDestroyLevel { get; set; } = true;

    protected virtual void OnDestroy()
    {
        EventManager.RemoveEventListener(this);

        // We do not need to destroy level if level is loading, current level will be destroyed when loading complete
        if (!Root.instance || Level.loading || !m_level || !canDestroyLevel) return;

        m_level = null;

        Game.GoHome();
    }

    private void Awake()
    {
        EventManager.AddEventListener(Events.RESOLUTION_CHANGED, FixCameras);
    }

    public void FixCameras()
    {
        if (!m_level || !m_level.root) return;

        var root = m_level.root?.Find("cameras");
        if (!root) return;

        var cameras = root.GetComponentsInChildren<Camera>(true);
        foreach (var camera in cameras) UIManager.FixCameraRect(camera);
    }
}
