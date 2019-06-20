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

using UnityEngine;

[AddComponentMenu("HYLR/UI/Audio Simple")]
public class UIAudioSimple : MonoBehaviour
{
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
}
