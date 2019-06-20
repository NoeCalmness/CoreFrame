/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * UI Audio helper
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-03-12
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;

[AddComponentMenu("HYLR/UI/Audio")]
public class UIAudio : MonoBehaviour
{
    private UISoundEffect m_sound;

    [NonSerialized]
    public int lastPlayed = -1;

    private int m_current = 0;

    private void OnEnable()
    {
        if (lastPlayed != 0) PlayAudio(0);
        lastPlayed = -1;
    }

    private void OnDisable()
    {
        if (m_current > 0) AudioManager.Stop(m_current);
        m_current = 0;

        if (lastPlayed != 1) PlayAudio(1);
        lastPlayed = -1;
    }

    public void PlayAudio(int type)
    {
        if (string.IsNullOrEmpty(tag)) return;

        if (m_sound == null || m_sound.element != name) m_sound = UISoundEffect.GetSound(tag, name);

        var a = UISoundEffect.GetSound(m_sound == null ? null : type == 0 ? m_sound.onEnable : m_sound.onDisable);

        if (string.IsNullOrEmpty(a)) return;

        AudioManager.PlayAudio(a, m_sound.type, false, 0, s => m_current = s ? s.id : 0);

        lastPlayed = type;
    }

    public void PlaySound(string audioName)
    {
        if (string.IsNullOrEmpty(audioName)) return;
        AudioManager.PlaySound(audioName);
    }

    public void PlayVoice(string audioName)
    {
        if (string.IsNullOrEmpty(audioName)) return;
        AudioManager.PlayVoice(audioName);
    }

    public void PlayMusic(string audioName)
    {
        if (string.IsNullOrEmpty(audioName)) return;
        AudioManager.PlayMusic(audioName);
    }

    public void PlayMusicMix(string audioName)
    {
        if (string.IsNullOrEmpty(audioName)) return;
        AudioManager.PlayMusicMix(audioName);
    }

    protected virtual void OnDestroy()
    {
        if (m_current > 0 && AudioManager.instance) AudioManager.Stop(m_current);
        m_current = 0;

        m_sound = null;

        #if UNITY_EDITOR
        EventManager.RemoveEventListener(this);
        #endif
    }

    #region Editor helper

#if UNITY_EDITOR
    private void Awake()
    {
        EventManager.AddEventListener("EditorReloadConfig", OnEditorReloadConfig);
    }

    private void OnEditorReloadConfig(Event_ e)
    {
        if (string.IsNullOrEmpty(tag)) return;

        var config = (string)e.param1;
        if (config != "config_uisoundeffects") return;

        m_sound = UISoundEffect.GetSound(tag, name);
    }

#endif

    #endregion
}
