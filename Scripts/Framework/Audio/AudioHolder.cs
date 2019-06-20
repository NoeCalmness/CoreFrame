/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Audio Holder.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-10-13
 * 
 ***************************************************************************************************/

using UnityEngine;

[AddComponentMenu("HYLR/Audio/AudioHolder")]
[RequireComponent(typeof(AudioSource))]
public class AudioHolder : MonoBehaviour
{
    public static AudioHolder Create(string name)
    {
        var holder = new GameObject(name).GetComponentDefault<AudioHolder>();
        return holder;
    }

    public AudioSource source;
    public int id;
    public AudioTypes type;

    [SerializeField, Set("volume")]
    private float m_volume;
    [SerializeField, Set("loop")]
    private bool m_loop;
    [SerializeField, Set("isPaused")]
    private bool m_isPaused;

    public float volume
    {
        get { return m_volume; }
        set
        {
            if (m_volume == value) return;
            m_volume = value;

            if (source) source.volume = m_volume;
        }
    }
    public bool loop
    {
        get { return m_loop; }
        set
        {
            if (m_loop == value) return;
            m_loop = value;

            if (source) source.loop = m_loop;
        }
    }
    public bool isPlaying { get { return !m_isPaused && source && source.clip && source.isPlaying; } }
    public bool isPaused
    {
        get { return m_isPaused; }
        set
        {
            value &= source && source.clip;

            if (m_isPaused == value) return;
            m_isPaused = value;

            if (source)
            {
                if (m_isPaused) source.Pause();
                else source.UnPause();
            }
        }
    }

    public float globalBgmVolume { get; set; }
    
    public void Stop()
    {
        m_loop     = false;
        m_isPaused = false;

        source?.Stop();
    }

    private void Awake()
    {
        id     = 0;
        type   = AudioTypes.All;
        source = this.GetComponentDefault<AudioSource>();

        source.volume = m_volume;
    }

    private void OnDestroy()
    {
        id          = 0;
        type        = AudioTypes.All;
        source      = null;
        m_loop      = false;
        m_isPaused  = false;
    }
}