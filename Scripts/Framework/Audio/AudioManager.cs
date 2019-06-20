/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Global Audio Manager
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-12-20
 * 
 ***************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;
using UnityEngine;

/// <summary>
/// Marked audio types
/// </summary>
public enum AudioTypes
{
    /// <summary>
    /// Used for stop/resum/pause
    /// </summary>
    All  = -1,

    /// <summary>
    /// Used for sound effects
    /// e.g: weapon attack, hit
    /// </summary>
    Sound = 0,
    /// <summary>
    /// Used for character voice
    /// </summary>
    Voice = 1,
    /// <summary>
    /// Used for long duration audio
    /// </summary>
    Music = 2
}

[AddComponentMenu("HYLR/Audio/AudioManager")]
public class AudioManager : SingletonBehaviour<AudioManager>
{
    #region Static function

    public const int ALL = -1, SOUND = 0, VOICE = 1, MUSIC = 2;

    public const int MAX_CACHED_AUDIOS = 50;
    public const int MAX_CACHED_AUDIO_HOLDERS = 100;

    /// <summary>
    /// Audio Manager initialized ?
    /// </summary>
    public static bool initialized { get; private set; }

    /// <summary>
    /// Initialize AudioManager
    /// </summary>
    public static void Initialize()
    {
        if (initialized || !instance) return;
        initialized = true;

        instance._Initialize();
    }

    /// <summary>
    /// Play an audio clip
    /// </summary>
    /// <param name="audio">Asset name</param>
    /// <param name="type">Audio type, default is sound</param>
    /// <param name="loop">loop ?</param>
    /// <param name="overrideType">0 = Additive  1 = Override  2 = Discard</param>
    /// <param name="onPlay">Callback when asset loaded and begin to play, param is null if load failed</param>
    /// <param name="bundle">Asset bundle name if assigned</param>
    public static void PlayAudio(string audio, AudioTypes type, bool loop = false, int overrideType = 0, Action<AudioHolder> onPlay = null, string bundle = null)
    {
        instance._PlayAudio(audio, loop, onPlay, bundle, (int)type, overrideType);
    }

    /// <summary>
    /// Play an audio clip
    /// </summary>
    /// <param name="audio">Asset name</param>
    /// <param name="loop">loop ?</param>
    /// <param name="onPlay">Callback when asset loaded and begin to play, param is null if load failed</param>
    /// <param name="bundle">Asset bundle name if assigned</param>
    /// <param name="type">Audio type, default is sound</param>
    /// <param name="overrideType">0 = Additive  1 = Override  2 = Discard</param>
    /// <param name="muteBGM">Mute current level bgm</param>
    /// <param name="bgmFadeDuration">If mute bgm, use fade ? 0 = do not fade, negative = use bgm default fade time</param>
    public static void PlayAudio(string audio, bool loop = false, Action<AudioHolder> onPlay = null, string bundle = null, AudioTypes type = AudioTypes.Sound, int overrideType = 0, bool muteBGM = false, float bgmFadeDuration = -1.0f)
    {
        instance._PlayAudio(audio, loop, onPlay, bundle, (int)type, overrideType, muteBGM, bgmFadeDuration);
    }

    /// <summary>
    /// Play a sound audio clip
    /// </summary>
    /// <param name="audio">Asset name</param>
    /// <param name="loop">loop ?</param>
    /// <param name="overrideType">0 = Additive  1 = Override  2 = Discard</param>
    /// <param name="onPlay">Callback when asset loaded and begin to play, param is null if load failed</param>
    /// <param name="bundle">Asset bundle name if assigned</param>
    public static void PlaySound(string audio, bool loop = false, int overrideType = 0, Action<AudioHolder> onPlay = null, string bundle = null)
    {
        PlayAudio(audio, loop, onPlay, bundle, AudioTypes.Sound, overrideType);
    }

    /// <summary>
    /// Play a voice audio clip
    /// </summary>
    /// <param name="audio">Asset name</param>
    /// <param name="loop">loop ?</param>
    /// <param name="onPlay">Callback when asset loaded and begin to play, param is null if load failed</param>
    /// <param name="bundle">Asset bundle name if assigned</param>
    public static void PlayVoice(string audio, bool loop = false, int overrideType = 0, Action<AudioHolder> onPlay = null, string bundle = null)
    {
        PlayAudio(audio, loop, onPlay, bundle, AudioTypes.Voice, overrideType);
    }

    /// <summary>
    /// Play a music audio clip and mute bgm
    /// If audio is playing, stop it and restart
    /// </summary>
    /// <param name="audio">Asset name</param>
    /// <param name="bgmFadeDuration">use fade ? 0 = do not fade, negative = use bgm default fade time</param>
    public static void PlayMusicMix(string audio, float bgmFadeDuration = -1.0f)
    {
        PlayAudio(audio, false, null, null, AudioTypes.Music, 1, true, bgmFadeDuration);
    }

    /// <summary>
    /// Play a music audio clip and mute bgm
    /// </summary>
    /// <param name="audio">Asset name</param>
    /// <param name="loop">loop ?</param>
    /// <param name="bgmFadeDuration">If mute bgm, use fade ? 0 = do not fade, negative = use bgm default fade time</param>
    public static void PlayMusicMix(string audio, bool loop, float bgmFadeDuration = -1.0f)
    {
        PlayAudio(audio, loop, null, null, AudioTypes.Music, 1, true, bgmFadeDuration);
    }

    /// <summary>
    /// Play a music audio clip
    /// </summary>
    /// <param name="audio">Asset name</param>
    /// <param name="loop">loop ?</param>
    /// <param name="onPlay">Callback when asset loaded and begin to play, param is null if load failed</param>
    /// <param name="bundle">Asset bundle name if assigned</param>
    /// <param name="muteBGM">Mute current level bgm</param>
    /// <param name="bgmFadeDuration">If mute bgm, use fade ? 0 = do not fade, negative = use bgm default fade time</param>
    public static void PlayMusic(string audio, bool loop = false, Action<AudioHolder> onPlay = null, string bundle = null, bool muteBGM = true, float bgmFadeDuration = -1.0f)
    {
        PlayAudio(audio, loop, onPlay, bundle, AudioTypes.Music, 1, muteBGM, bgmFadeDuration);
    }

    /// <summary>
    /// Play an audio clip with custom AudioSource
    /// Important: Custom source will not managed by AudioManager, you should listen Event.AUDIO_SETTING_CHANGED to manage volum yourself
    /// </summary>
    /// <param name="audio">Clip name</param>
    /// <param name="source">Custom AudioSource Component</param>
    /// <param name="bundle">Asset Bundle name if assigned</param>
    /// <param name="overrideType">0 = Additive  1 = Override  2 = Discard</param>
    /// <returns></returns>
    public static IEnumerator PlayAudio(string audio, AudioSource source, string bundle = null, int overrideType = 0)
    {
        return instance._PlayAudio(audio, source, bundle, overrideType);
    }

    /// <summary>
    /// Destroy all playing and cached AudioSource and AudioClip
    /// </summary>
    public static void Clear()
    {
        instance._Clear();
    }

    /// <summary>
    /// AudioSource is in loading state or in play list ?
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool IsInPlayListOrLoading(string name)
    {
        return instance._IsInPlayListOrLoading(name);
    }

    /// <summary>
    /// AudioSource is paused ?
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool IsPaused(string name)
    {
        return instance._IsPaused(name);
    }

    /// <summary>
    /// AudioSource is playing ?
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool IsPlaying(string name)
    {
        return instance._IsPlaying(name);
    }

    /// <summary>
    /// Pause all current playing audio source with name
    /// </summary>
    /// <param name="name"></param>
    public static void Pause(string name)
    {
        instance._Pause(name);
    }

    /// <summary>
    /// Pause all playing audio sources
    /// </summary>
    /// <param name="type">Audio type</param>
    public static void PauseAll(AudioTypes type = AudioTypes.All)
    {
        instance._PauseAll((int)type);
    }

    /// <summary>
    /// Resum a paused audio source
    /// </summary>
    /// <param name="name"></param>
    public static void Resum(string name)
    {
        instance._Resum(name);
    }

    /// <summary>
    /// Resum all paused audio sources
    /// </summary>
    /// <param name="type">Audio type</param>
    public static void ResumAll(AudioTypes type = AudioTypes.All)
    {
        instance._ResumAll((int)type);
    }

    /// <summary>
    /// Stop playing audio source with name
    /// </summary>
    /// <param name="name"></param>
    public static void Stop(string name)
    {
        instance._Stop(name);
    }

    /// <summary>
    /// Stop playing audio source with id
    /// </summary>
    /// <param name="name"></param>
    public static void Stop(int id)
    {
        instance._Stop(id);
    }

    /// <summary>
    /// Stop all playing audio sources
    /// </summary>
    /// <param name="type">Audio type</param>
    public static void StopAll(AudioTypes type = AudioTypes.All)
    {
        instance._StopAll((int)type);
    }

    #endregion

    private Transform m_pool = null;
    private Transform m_playing = null;

    private Queue<AudioHolder> m_cachedAudioSources = new Queue<AudioHolder>();

    [SerializeField]
    private int m_cachedID = 0;
    [SerializeField, Space(10)]
    private List<AudioClip> m_cachedAudioClips = new List<AudioClip>();
    [SerializeField]
    private List<AudioHolder> m_playList = new List<AudioHolder>();
    private List<AudioHolder> m_musics   = new List<AudioHolder>();
    private List<AudioHolder> m_voices   = new List<AudioHolder>();
    private List<AudioHolder> m_sounds   = new List<AudioHolder>();

    private Dictionary<int, AudioHolder> m_hashList = new Dictionary<int, AudioHolder>();

    private List<string> m_loading = new List<string>();

    public void _PlayAudio(string audio, bool loop = false, Action<AudioHolder> onPlay = null, string bundle = null, int type = SOUND, int overrideType = 0, bool muteBGM = false, float bgmFadeDuration = -1.0f)
    {
        if (!isActiveAndEnabled || string.IsNullOrEmpty(audio)) return;

        if (overrideType == 2)
        {
            if (_IsPlaying(audio)) return;
            if (_IsPaused(audio))
            {
                _Resum(audio);
                return;
            }
        }

        m_loading.Add(audio);
        StartCoroutine(_LoadAudioClip(bundle, audio, clip =>
        {
            if (!m_loading.Contains(audio)) return; // Stopped before loading complete

            m_loading.Remove(audio);
            if (!clip)
            {
                Logger.LogError("AudioManager::_PlayAudio: Load Audio Clip [{1}:{0}] failed.", audio, bundle);
                onPlay?.Invoke(null);
            }
            else
            {
                if (overrideType == 1) _Stop(audio);
                else if (overrideType == 2)
                {
                    if (_IsPlaying(audio)) return;
                    if (_IsPaused(audio))
                    {
                        _Resum(audio);
                        return;
                    }
                }

                var holder = CreateAudioHolder(audio, clip, true, type);
                holder.loop = loop;
                holder.volume = type == SOUND ? SettingsManager.realSoundVolume : type == VOICE ? SettingsManager.realVoiceVolume : SettingsManager.realBgmVolume;

                holder.globalBgmVolume = -1.0f;
                if (muteBGM)
                {
                    var helper = Level.current?.audioHelper ?? null;
                    if (helper)
                    {
                        holder.globalBgmVolume = helper.globalVolume;
                        helper.SetGlobalVolume(0, bgmFadeDuration);
                    }
                }

                StartCoroutine(WaitPlayComplete(holder, type, bgmFadeDuration));

                onPlay?.Invoke(holder);
            }
        }));
    }

    public IEnumerator _PlayAudio(string audio, AudioSource source, string bundle = null, int overrideType = 0)
    {
        if (!isActiveAndEnabled || string.IsNullOrEmpty(audio)) yield break;

        if (!source)
        {
            Logger.LogError("AudioManager::_PlayAudio: Custom AudioSource can not be null.");
            yield break;
        }

        if (overrideType == 2)
        {
            if (_IsPlaying(audio)) yield break;
            if (_IsPaused(audio))
            {
                _Resum(audio);
                yield break;
            }
        }

        yield return StartCoroutine(_LoadAudioClip(bundle, audio, clip =>
        {
            if (!source) return;

            if (overrideType == 1) _Stop(audio);
            else if (overrideType == 2)
            {
                if (_IsPlaying(audio)) return;
                if (_IsPaused(audio))
                {
                    _Resum(audio);
                    return;
                }
            }

            source.clip = clip;
            source.Play();
        }));
    }

    public void _Clear()
    {
        Util.ClearChildren(m_playing, true);

        m_loading.Clear();
        m_playList.Clear();
        m_hashList.Clear();
        m_sounds.Clear();
        m_voices.Clear();
        m_musics.Clear();

        Util.ClearChildren(m_pool, true);

        m_cachedAudioSources.Clear();
        m_cachedAudioClips.Clear();
    }

    public bool _IsInPlayListOrLoading(string name)
    {
        return m_loading.Contains(name) || m_playList.FindIndex(s => s && s.name == name) > -1;
    }

    public bool _IsPaused(string name)
    {
        return m_playList.Find(s => s && s.isPaused && s.name == name);
    }

    public bool _IsPlaying(string name)
    {
        return m_playList.Find(s => s && s.name == name && s.isPlaying);
    }

    public bool _IsPaused(int id)
    {
        var holder = m_hashList.Get(id);
        return holder && holder.isPaused;
    }

    public bool _IsPlaying(int id)
    {
        var holder = m_hashList.Get(id);
        return holder && holder.isPlaying;
    }

    public void _Pause(string name)
    {
        foreach (var holder in m_playList)
        {
            if (!holder || holder.name != name) continue;
            holder.isPaused = true;
        }
    }

    public void _Pause(int id)
    {
        var holder = m_hashList.Get(id);
        if (holder) holder.isPaused = true;
    }

    public void _PauseAll(int type = -1)
    {
        var l = type == ALL ? m_playList : type == SOUND ? m_sounds : type == VOICE ? m_voices : m_musics;
        foreach (var holder in l)
        {
            if (!holder) continue;
            holder.isPaused = true;
        }
    }

    public void _Resum(string name)
    {
        foreach (var holder in m_playList)
        {
            if (!holder || holder.name != name) continue;
            holder.isPaused = false;
        }
    }

    public void _Resum(int id)
    {
        var holder = m_hashList.Get(id);
        if (holder) holder.isPaused = false;
    }

    public void _ResumAll(int type = -1)
    {
        var l = type == ALL ? m_playList : type == SOUND ? m_sounds : type == VOICE ? m_voices : m_musics;
        foreach (var holder in l)
        {
            if (!holder) continue;
            holder.isPaused = false;
        }
    }

    public void _Stop(string name)
    {
        m_loading.Remove(name);
        foreach (var holder in m_playList)
        {
            if (!holder || holder.name != name) continue;
            holder.Stop();
        }
    }

    public void _Stop(int id)
    {
        var holder = m_hashList.Get(id);
        holder?.Stop();
    }

    public void _StopAll(int type = -1)
    {
        var l = type == ALL ? m_playList : type == SOUND ? m_sounds : type == VOICE ? m_voices : m_musics;
        foreach (var holder in l) holder?.Stop();
    }

    private void _Initialize()
    {
        m_pool = Util.AddChild(transform, new GameObject("pool").transform);
        m_playing = Util.AddChild(transform, new GameObject("playing").transform);

        EventManager.AddEventListener(Events.AUDIO_SETTINGS_CHANGED, OnAudioSettingsChanged);

        AudioListener.volume = SettingsManager.realVolume;
    }

    private IEnumerator _LoadAudioClip(string bundle, string audio, Action<AudioClip> onLoad)
    {
        var clip = m_cachedAudioClips.Find(c => c.name == audio);
        if (!clip) clip = Level.current?._GetPreloadObject<AudioClip>(audio, false);

        if (!clip)
        {
            if (string.IsNullOrEmpty(bundle)) bundle = audio;

            var op = AssetManager.LoadAssetAsync(bundle, audio, typeof(AudioClip));
            yield return op;

            clip = op?.GetAsset<AudioClip>();

            AssetManager.UnloadAssetBundle(bundle);

            if (clip) m_cachedAudioClips.Add(clip);
            if (m_cachedAudioClips.Count >= MAX_CACHED_AUDIOS)
                m_cachedAudioClips.RemoveAt(0);
        }

        onLoad(clip);
    }

    private void AddRemoveTypeList(AudioHolder holder, int type, bool add = true)
    {
        var l = type == SOUND ? m_sounds : type == VOICE ? m_voices : m_musics;
        if (add) l.Add(holder);
        else l.Remove(holder);
    }

    private AudioHolder CreateAudioHolder(string name, AudioClip clip, bool play, int type)
    {
        AudioHolder holder = null;
        while (m_cachedAudioSources.Count > 0 && !holder) holder = m_cachedAudioSources.Dequeue();
        if (!holder) holder = AudioHolder.Create(name);

        holder.id = ++m_cachedID;
        holder.type = (AudioTypes)type;

        if (!holder.source) holder.source = holder.GetComponentDefault<AudioSource>();

        var source = holder.source;

        source.playOnAwake = false;
        source.enabled = true;
        source.name = name;
        source.transform.SetParent(m_playing);
        source.spatialBlend = 0;
        source.loop = false;

        source.clip   = clip;
        source.volume = holder.volume;

        if (play) source.Play();

        m_playList.Add(holder);
        m_hashList.Set(holder.id, holder);

        AddRemoveTypeList(holder, type);

        return holder;
    }

    private IEnumerator WaitPlayComplete(AudioHolder holder, int type, float bgmFadeDuration)
    {
        var id = holder.id;
        var wait = new WaitUntil(() => { return !holder || !holder.isPlaying && !holder.isPaused; } );
        yield return wait;

        if (holder && holder.globalBgmVolume >= 0) Level.current?.audioHelper?.SetGlobalVolume(holder.globalBgmVolume, bgmFadeDuration);

        m_playList.Remove(holder);
        m_hashList.Remove(id);

        AddRemoveTypeList(holder, type, false);

        if (holder)
        {
            if (holder.source) holder.source.clip = null;

            if (m_cachedAudioSources.Count >= MAX_CACHED_AUDIO_HOLDERS) DestroyImmediate(holder.gameObject);
            else
            {
                holder.Stop();

                holder.transform.SetParent(m_pool);
                holder.enabled = false;

                m_cachedAudioSources.Enqueue(holder);
            }
        }
    }

    private void OnAudioSettingsChanged()
    {
        foreach (var holder in m_sounds) if (holder) holder.volume = SettingsManager.realSoundVolume;
        foreach (var holder in m_voices) if (holder) holder.volume = SettingsManager.realVoiceVolume;
        foreach (var holder in m_musics) if (holder) holder.volume = SettingsManager.realBgmVolume;

        AudioListener.volume = SettingsManager.realVolume;
    }
}
