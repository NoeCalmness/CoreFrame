/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Audio Listener Helper.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-12-19
 * 
 ***************************************************************************************************/

using UnityEngine;

[AddComponentMenu("HYLR/Audio/AudioListener")]
[RequireComponent(typeof(AudioListener), typeof(BoxCollider))]
public class AudioListenerHelper : MonoBehaviour
{
    public AudioListener listener { get { return m_listener; } }
    private AudioListener m_listener = null;

    private BoxCollider m_collider = null;

    private void Awake()
    {
        m_listener = this.GetComponentDefault<AudioListener>();
        m_collider = this.GetComponentDefault<BoxCollider>();

        var rigid = this.GetComponentDefault<Rigidbody>();
        rigid.useGravity = false;
        
        m_collider.isTrigger = false;

        Util.SetLayer(gameObject, Layers.BGM_LISTENER);
    }

    public void UpdateParams(Vector3 center, Vector3 size)
    {
        m_collider.center = center;
        m_collider.size   = size;
    }
}
