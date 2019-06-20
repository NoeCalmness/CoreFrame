/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Helper component for particle pause/resum
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-29
 * 
 ***************************************************************************************************/

using UnityEngine;

[AddComponentMenu("HYLR/Utilities/PauseResum Particles")]
public class PauseResumParticles : MonoBehaviour
{
    private ParticleSystem[] m_particles;
    private Animator[] m_animators;

    public bool paused
    {
        get { return m_paused; }
        set { PauseResum(value); }
    }

    public float speed
    {
        get { return m_speed; }
        set { UpdateSpeed(value); }
    }

    [SerializeField, Set("paused")]
    private bool m_paused;
    [SerializeField, Set("speed")]
    private float m_speed = 1.0f;

    private void Awake()
    {
        m_particles = transform.GetComponentsInChildren<ParticleSystem>(true);
        m_animators = transform.GetComponentsInChildren<Animator>(true);
    }

    public void PauseResum(bool pause)
    {
        if (m_paused == pause) return;
        m_paused = pause;

        Refresh();
    }

    public void UpdateSpeed(float _speed)
    {
        if (m_speed == _speed) return;
        m_speed = _speed;

        Refresh();
    }

    public void Initialize(bool _paused = false, float _speed = 1.0f)
    {
        m_paused = _paused;
        m_speed  = _speed;

        Refresh();
    }

    private void Refresh()
    {
        if (m_particles == null) return;

        foreach (var particle in m_particles)
        {
            if (!particle) continue;
            var m = particle.main;
            m.simulationSpeed = m_paused ? 0 : m_speed;
        }

        foreach (var animator in m_animators)
        {
            if (!animator) continue;
            animator.speed = m_paused ? 0 : m_speed;
        }
    }

    #region Editor helper

#if UNITY_EDITOR
    private bool m_logicPaused = false;
    private void Update()
    {
        if (m_logicPaused == Root.logicPaused) return;
        m_logicPaused = Root.logicPaused;

        foreach (var particle in m_particles)
        {
            if (!particle) continue;
            var m = particle.main;
            m.simulationSpeed = m_logicPaused || m_paused ? 0 : m_speed;
        }

        foreach (var animator in m_animators)
       {
            if (!animator) continue;
            animator.speed = m_logicPaused || m_paused ? 0 : m_speed;
        }
    }

#endif

    #endregion
}
