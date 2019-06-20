/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Used for Scene BGM.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-05
 * 
 ***************************************************************************************************/

using UnityEngine;
using DG.Tweening;

[AddComponentMenu("HYLR/Audio/AudioBGMTrigger")]
[RequireComponent(typeof(AudioSource))]
public class AudioBGMTrigger : MonoBehaviour
{
    public float fadeInVolume    = 1.0f;
    public float fadeInDuration  = 2.5f;

    public float fadeOutDuration = 2.5f;

    public float mixWeight       = 10;

    public float mixedVolume
    {
        get { return m_mixedVolume; }
        set
        {
            if (m_mixedVolume == value) return;
            m_mixedVolume = value;

            UpdateRealVolume();
        }
    }
    [SerializeField, Set("mixedVolume")]
    public float m_mixedVolume     = 1.0f;

    [SerializeField]
    public float m_realVolume = 1.0f;

    public bool muteMainBGM
    {
        get { return m_muteMainBGM; }
        set
        {
            if (m_muteMainBGM == value) return;
            m_muteMainBGM = value;

            if (m_helper && m_triggered)
                m_helper.UpdateMixVolumes();
        }
    }
    [SerializeField, Set("muteMainBGM")]
    private bool m_muteMainBGM = false;

    public bool triggered
    {
        get { return m_triggered; }
        set
        {
            if (m_triggered == value) return;
            m_triggered = value;

            if (m_helper)
            {
                m_helper.UpdateTriggerState(this, m_triggered);
                FadeInOut();
            }
        }
    }

    [SerializeField, Set("triggered")]
    private bool m_triggered = false;

    public AudioSource source { get { return m_source; } }
    private AudioSource m_source;

    public AudioHelper helper
    {
        get { return m_helper; }
        set
        {
            if (m_helper)
            {
                Logger.LogError("Audio helper already assigned!");
                return;
            }

            m_helper = value;
            if (m_helper)
            {
                UpdateRealVolume();
                m_helper.onGlobalVolumeChanged += UpdateRealVolume;
            }

            if (m_triggered)
            {
                m_helper.UpdateTriggerState(this, true);
                FadeInOut();
            }
        }
    }
    private AudioHelper m_helper = null;

    private Tweener m_tweener = null;

    private bool m_destroyed = false;

    public string audioName = null;

    public void Initialize(AudioClip clip, Level level)
    {
        if (m_destroyed) return;

        enabled = true;

        m_source = GetComponent<AudioSource>();
        if (m_source && !m_source.clip) m_source.clip = clip;

        if (m_source && m_source.clip)
        {
            m_source = GetComponent<AudioSource>();
            m_source.enabled      = false;
            m_source.playOnAwake  = false;
            m_source.spatialBlend = 0;
            m_source.loop         = true;
            m_source.maxDistance  = m_source.minDistance + 0.01f;

            var collider = this.GetComponentDefault<BoxCollider>();

            collider.size      = new Vector3(m_source.minDistance * 2, 40, 0.1f);
            collider.center    = new Vector3(0, 0, 0);
            collider.enabled   = true;
            collider.isTrigger = true;
        }
        else
        {
            m_source = null;
            var collider = GetComponent<SphereCollider>();
            if (collider) collider.enabled = false;
        }

        if (fadeInVolume < 0) fadeInVolume = 1.0f;

        level?.AddEventListener(Events.SCENE_UNLOAD_ASSETS, OnDestroy);

        EventManager.AddEventListener(Events.AUDIO_SETTINGS_CHANGED, OnAudioSettingsChanged);
    }

    private void OnTriggerEnter(Collider other)
    {
        UpdateState(true);
    }

    private void OnTriggerExit(Collider other)
    {
        UpdateState(false);

        enabled = true;
    }

    private void OnDestroy()
    {
        if (m_destroyed) return;
        m_destroyed = true;

        m_tweener?.Kill();

        if (m_source)
        {
            m_source.clip = null;
            m_source.Stop();
        }

        m_tweener = null;
        m_helper  = null;
        m_source  = null;

        EventManager.RemoveEventListener(this);
    }

    private void UpdateState(bool newState)
    {
        if (m_triggered == newState) return;
        m_triggered = newState;

        if (!m_helper) return;

        m_helper.UpdateTriggerState(this, m_triggered);
        FadeInOut();
    }

    private void FadeInOut()
    {
        if (m_triggered)
        {
            var v = m_realVolume * SettingsManager.realBgmVolume;

            if (m_tweener != null) m_tweener.Kill();

            m_source.enabled = true;
            m_source.volume = 0;
            m_source.Play();

            m_tweener = m_source.DOFade(v, fadeInDuration).SetEase(Ease.InExpo).OnUpdate(TweenUpdate).OnComplete(TweenComplete);

            return;
        }

        if (m_tweener != null) m_tweener.Kill();
        m_tweener = m_source.DOFade(0, fadeOutDuration).SetEase(Ease.OutExpo).OnComplete(TweenComplete);
    }

    private void TweenUpdate()
    {
        var max = m_realVolume * SettingsManager.realBgmVolume;
        if (m_source.volume > max)
        {
            m_source.volume = max;
            m_tweener.Kill();
            m_tweener = null;
        }
    }

    private void TweenComplete()
    {
        m_tweener = null;
        var max = m_realVolume * SettingsManager.realBgmVolume;
        if (m_source.volume > max) m_source.volume = max;

        if (!m_triggered)
        {
            m_source.Stop();
            m_source.enabled = false;
        }
    }

    private void OnAudioSettingsChanged()
    {
        if (m_source && m_triggered && (m_tweener == null || !m_tweener.IsPlaying()))
            m_source.volume = m_realVolume * SettingsManager.realBgmVolume;
    }

    private void UpdateRealVolume()
    {
        m_realVolume = m_mixedVolume *(m_helper ? m_helper.globalVolume : 1.0f);
        OnAudioSettingsChanged();
    }

    #region Editor helper

#if UNITY_EDITOR

    private void Awake()
    {
        if (Game.started) return;
        Initialize(null, null);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Util.BuildColor(m_triggered ? "#FDA601FF" : "#FDA60190");

        if (!m_source) m_source = GetComponent<AudioSource>();

        var l = transform.position; l.x -= m_source.minDistance;
        var r = transform.position; r.x += m_source.minDistance;

        var up = Vector3.up * 5;

        Gizmos.DrawLine(l, r);
        Gizmos.DrawLine(l, l + up);
        Gizmos.DrawLine(r, r + up);
        Gizmos.DrawLine(l + up, r + up);
    }
#endif

    #endregion
}
