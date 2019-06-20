/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Helper Component for Level Effect
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-04-27
 * 
 ***************************************************************************************************/

using UnityEngine;

public class LevelEffect : MonoBehaviour
{
    public enum EffectLevel { Low = 0, Medium = 1, High = 2, Fantasic = 3, Count }

    public EffectLevel effectLevel
    {
        get { return m_effectLevel; }
        set
        {
            if (m_effectLevel == value) return;
            m_effectLevel = value;

            UpdateEffect();
        }
    }

    [SerializeField, Set("effectLevel")]
    private EffectLevel m_effectLevel = EffectLevel.Medium;

    private GameObject[] m_nodes;

    private void Awake()
    {
        m_nodes = new GameObject[(int)EffectLevel.Count];
        for (var i = 0; i < m_nodes.Length; ++i)
        {
            var node = transform.Find(i.ToString());
            m_nodes[i] = node?.gameObject;
        }

        m_effectLevel = SettingsManager.instance ? (EffectLevel)SettingsManager.effectLevel : EffectLevel.Low;
        UpdateEffect();

        EventManager.AddEventListener(Events.VEDIO_SETTINGS_CHANGED, OnVideoSettingsChanged);
    }

    private void OnDestroy()
    {
        m_nodes = null;
        EventManager.RemoveEventListener(this);
    }

    private void OnVideoSettingsChanged()
    {
        effectLevel = (EffectLevel)SettingsManager.effectLevel;
    }

    public void UpdateEffect()
    {
        if (m_nodes == null) return;
        for (int i = 0, c = m_nodes.Length, l = Mathf.Clamp((int)m_effectLevel, 0, 3); i < c; ++i)
        {
            var node = m_nodes[i];
            if (!node) continue;
            node.SetActive(i <= l);
        }
    }
}
