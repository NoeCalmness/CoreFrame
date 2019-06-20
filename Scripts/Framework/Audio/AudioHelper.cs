/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Audio Helper.
 * Manages All Scene BGM at runtime
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-12-19
 * 
 ***************************************************************************************************/

using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("HYLR/Audio/AudioHelper")]
[RequireComponent(typeof(AudioSource))]
public class AudioHelper : MonoBehaviour
{
    public float fadeInVolume    = 1.0f;
    public float fadeInDuration  = 2.5f;

    public float fadeOutVolume   = 0;
    public float fadeOutDuration = 2.5f;

    public float startDelay      = 0;
    public float mixWeight       = 5;

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
    public float m_mixedVolume = 1.0f;

    [SerializeField]
    public float m_realVolume = 1.0f;

    public VoidHandler onGlobalVolumeChanged = null;

    public AudioSource source { get { return m_source; } }
    private AudioSource m_source;

    public float globalVolume
    {
        get { return m_globalVolume; }
        set
        {
            if (m_globalVolume == value) return;
            m_globalVolume = value;

            UpdateRealVolume();

            onGlobalVolumeChanged?.Invoke();
        }
    }
    [SerializeField, Set("globalVolume")]
    private float m_globalVolume = 1.0f;

    private List<AudioBGMTrigger> m_triggers = new List<AudioBGMTrigger>();
    private List<AudioBGMTrigger> m_current  = new List<AudioBGMTrigger>();

    private Tweener m_tweener, m_gTweener;
    private bool m_muted = false;

    public string audioName = null;

    private bool m_initialized = false;
    private bool m_destroyed = false;

    public void Initialize(AudioClip clip, Level level)
    {
        if (m_destroyed || m_initialized) return;
        m_initialized = true;

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
            m_source.minDistance  = 2;
            m_source.maxDistance  = 2.01f;
        }
        else
        {
            m_source = null;
            mixWeight = 0;

            Logger.LogError("Level missing main bgm!");
        }

        if (fadeInVolume < 0) fadeInVolume = 1.0f;
        if (fadeOutVolume < 0) fadeOutVolume = 0;

        level?.AddEventListener(Events.SCENE_UNLOAD_ASSETS, OnDestroy);

        EventManager.AddEventListener(Events.AUDIO_SETTINGS_CHANGED, OnAudioSettingsChanged);

        Go();
    }

    private void Go()
    {
        if (m_destroyed) return;

        BuildBgmList();

        if (!m_source) return;

        m_source.enabled = true;
        m_source.volume  = 0;
        m_source.Play();

        m_realVolume = fadeInVolume;
        var v = m_realVolume;
        #if UNITY_EDITOR
        if (Game.started)
        #endif
        v *= SettingsManager.realBgmVolume;
        m_tweener = m_source.DOFade(v, fadeInDuration).SetEase(Ease.InExpo).OnUpdate(TweenUpdate);
    }

    private void OnDestroy()
    {
        if (m_destroyed) return;
        m_destroyed = true;

        m_tweener?.Kill();
        m_gTweener?.Kill();
        m_triggers?.Clear();
        m_current?.Clear();

        if (m_source)
        {
            m_source.clip = null;
            m_source.Stop();
        }

        m_triggers = null;
        m_current = null;
        m_tweener = null;
        m_gTweener = null;

        m_source = null;

        EventManager.RemoveEventListener(this);
    }

    private void MuteOrUnMuteMainBGM(bool mute)
    {
        if (m_muted == mute) return;
        m_muted = mute;

        FadeInOut();
    }

    private void FadeInOut()
    {
        if (!m_source) return;

        if (!m_muted)
        {
            var v = m_realVolume * SettingsManager.realBgmVolume;

            if (m_tweener != null) m_tweener.Kill();

            m_source.volume = 0;
            m_tweener = m_source.DOFade(v, fadeInDuration).SetEase(Ease.InExpo).OnUpdate(TweenUpdate).OnComplete(TweenComplete); // If we change volume in fade in mode, check current tween position
        }
        else
        {
            if (m_tweener != null) m_tweener.Kill();
            m_tweener = m_source.DOFade(fadeOutVolume, fadeOutDuration).SetEase(Ease.OutExpo).OnComplete(TweenComplete);
        }
    }

    private void TweenUpdate()
    {
        var max = m_realVolume;
        #if UNITY_EDITOR
        if (Game.started)
        #endif
        max *= SettingsManager.realBgmVolume;
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
    }

    public void SetGlobalVolume(float volume, float tweenDuration = 0.0f)
    {
        if (m_gTweener != null) m_gTweener.Kill();
        m_gTweener = null;

        if (tweenDuration == 0)
        {
            m_globalVolume = volume;
            UpdateRealVolume();
            onGlobalVolumeChanged?.Invoke();
        }
        else
        {
            var duration = tweenDuration > 0 ? tweenDuration : volume < globalVolume ? fadeOutDuration : fadeInDuration;
            m_gTweener = DOTween.To(() => globalVolume, v => globalVolume = v, volume, duration).OnComplete(() => m_gTweener = null);
        }
    }

    public void UpdateTriggerState(AudioBGMTrigger trigger, bool triggered)
    {
        var contains = m_current.Contains(trigger);
        if (contains && !triggered)
        {
            m_current.Remove(trigger);

            UpdateMixVolumes();

            return;
        }

        if (!contains && triggered)
        {
            m_current.Add(trigger);

            UpdateMixVolumes();
        }
    }

    public void UpdateMixVolumes()
    {
        var total = 0f;

        var muted = false;
        foreach (var trigger in m_current)
        {
            total += trigger.mixWeight;
            muted = trigger.muteMainBGM;
        }

        if (!muted) total += mixWeight;

        if (total <= 0) total = 1;

        foreach (var trigger in m_current)
            trigger.mixedVolume = trigger.mixWeight / total * trigger.fadeInVolume;

        mixedVolume = muted ? 0 : mixWeight / total * fadeInVolume;

        MuteOrUnMuteMainBGM(muted);
    }

    private void BuildBgmList()
    {
        var bgm = transform.Find("bgm");
        if (!bgm) bgm = transform;

        var count = bgm.childCount;
        for (var i = 0; i < count; ++i)
        {
            var child = bgm.GetChild(i);
            var trigger = child.GetComponent<AudioBGMTrigger>();
            if (!trigger)
            {
                var s = child.GetComponent<AudioSource>();
                if (s) s.enabled = false;
                continue;
            }

            trigger.helper = this;
            m_triggers.Add(trigger);
        }

        Util.SetLayer(gameObject, Layers.BGM_TRIGGER);
    }

    private void OnAudioSettingsChanged()
    {
        if (m_source && !m_muted && (m_tweener == null || !m_tweener.IsPlaying()))
            m_source.volume = m_realVolume * SettingsManager.realBgmVolume;
    }

    private void UpdateRealVolume()
    {
        m_realVolume = m_mixedVolume * m_globalVolume;
        OnAudioSettingsChanged();
    }

    #region Editor helper

#if UNITY_EDITOR
    private void Awake()
    {
        if (Game.started) return;
        
        AudioListener.volume = 1.0f;

        AudioClip clip = null;
        if (!string.IsNullOrEmpty(audioName))
        {
            var paths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(audioName);
            if (paths.Length == 0) Logger.LogError("There is no asset with name \"{0}\" in {1}.", audioName, audioName);
            else clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(paths[0]);
        }
        Initialize(clip, null);
    }
#endif

    #endregion
}
