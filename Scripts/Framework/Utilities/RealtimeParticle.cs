/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Realtime particle system component
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-03-28
 * 
 ***************************************************************************************************/

using UnityEngine;

[AddComponentMenu("HYLR/Utilities/Realtime Particle")]
public class RealtimeParticle : MonoBehaviour
{
    public bool realtime
    {
        get { return m_realtime; }
        set
        {
            if (m_realtime == value) return;
            m_realtime = value;

            UpdateState();
        }
    }
    [SerializeField, Set("realtime")]
    private bool m_realtime = true;

    private ParticleSystem[] m_particles = null;

    public void SetRealtime(bool _realtime)
    {
        m_realtime = _realtime;
        UpdateState();
    }

    private void UpdateState()
    {
        m_particles = GetComponentsInChildren<ParticleSystem>(true);
        enabled = m_realtime && m_particles.Length > 0;
        if (m_particles == null) return;
        foreach (var particle in m_particles)
        {
            if (!particle) continue;
            var m = particle.main;
            m.simulationSpeed = m_realtime ? 0.0f : 1.0f;
        }

        var anims = GetComponentsInChildren<Animator>();
        foreach (var anim in anims) anim.updateMode = m_realtime ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;
    }

    private void Awake()
    {
        UpdateState();
    }

    private void Update()
    {
        if (!m_realtime || m_particles.Length < 1)
        {
            enabled = false;
            return;
        }

        foreach (var particle in m_particles)
        {
            if (!particle) continue;
            var m = particle.main;
            m.simulationSpeed = 1.0f;
            particle.Simulate(Time.unscaledDeltaTime, false, false);
            particle.Play();
            m.simulationSpeed = 0.0f;
        }
    }
}
