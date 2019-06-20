/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Tween script for alpha
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-01-20
 * 
 ***************************************************************************************************/

using UnityEngine;
using DG.Tweening;

[AddComponentMenu("HYLR/Tween/Tween Alpha Particle")]
public class TweenAlphaParticle : TweenBase
{
    [Space(5), Header("Alpha")]
    public float from = 0;
    public float to   = 1;
    public bool  startVisible = true;

    private ParticleSystem[] m_particles = null;
    private int m_waitAction = -1;
    private int m_tweenCount = 0;
    private UIDynamicEffect m_de;

    private void Awake()
    {
        m_de = GetComponent<UIDynamicEffect>();
    }

    private void OnDestroy()
    {
        m_de = null;
        m_particles = null;
    }

    private void OnDynamicEffectLoaded(UIDynamicEffect de)
    {
        if (m_waitAction < 0) return;
        Play(m_waitAction == 0);
    }

    protected override Tweener OnTween()
    {
        if (m_de && !m_de.current)
        {
            m_waitAction = m_forward ? 0 : 1;
            m_de.onLoaded -= OnDynamicEffectLoaded;
            m_de.onLoaded += OnDynamicEffectLoaded;
            return null;
        }

        if (startVisible) gameObject.SetActive(true);

        var f = m_forward ? from : to;
        var t = m_forward ? to : from;

        if (m_particles == null || m_particles.Length < 1) m_particles = GetComponentsInChildren<ParticleSystem>(true);

        m_tweenCount = 0;
        foreach (var a in m_particles)
        {
            var r = a.GetComponent<Renderer>();
            if (!r) continue;

            var mats = r.materials;
            foreach (var mat in mats)
            {
                if (!mat.HasProperty("_AlphaChannel")) continue;
                if (!currentAsFrom) mat.SetFloat("_AlphaChannel", f);
                var tween = SetTweener(mat.DOFloat(t, "_AlphaChannel", duration));
                if (tween != null) ++m_tweenCount;
            }
        }

        return null;
    }

    protected override void OnReset()
    {
        base.OnReset();

        if (m_particles == null) return;
        foreach (var a in m_particles)
        {
            var r = a.GetComponent<Renderer>();
            var mats = r.materials;
            foreach (var mat in mats)
            {
                if (!mat.HasProperty("_AlphaChannel")) continue;
                mat.SetFloat("_AlphaChannel", from);
            }
        }
    }

    protected override void OnComplete()
    {
        if (--m_tweenCount > 0) return;
        base.OnComplete();
    }
}
