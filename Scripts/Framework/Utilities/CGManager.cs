/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Play Game CG
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-05-14
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[AddComponentMenu("HYLR/Utilities/CG Manager")]
public class CGManager : SingletonBehaviour<CGManager>
{
    #region Static functions

    public const string CG_TITLE = "video_game_title";

    /// <summary>
    /// 播放指定名称的 CG 动画
    /// </summary>
    /// <param name="cg">要播放的 CG 动画资源名</param>
    /// <param name="onComplete">播放结束时回调</param>
    public static void Play(string cg, Action onComplete = null)
    {
        instance?._Play(cg, onComplete);
    }

    /// <summary>
    /// 停止当前正在播放的 CG 动画
    /// </summary>
    public static void Stop()
    {
        instance?._Stop();
    }

    /// <summary>
    /// 暂停当前正在播放的 CG 动画
    /// </summary>
    public static void Pause()
    {
        instance?._Pause();
    }

    #endregion

    #region CG manager

    [SerializeField]
    private string m_currentPlaying = string.Empty;

    private VideoPlayer m_video;
    [SerializeField]
    private float m_oldVolume;
    private Action m_onVideoComplete;

    private Camera m_CGCamera;

    protected override void Awake()
    {
        base.Awake();

        m_CGCamera = GetComponent<Canvas>().worldCamera;

        m_video = transform.AddUINodeStrech("video").GetComponentDefault<VideoPlayer>();
        var audio = m_video.GetComponentDefault<AudioSource>();
        audio.playOnAwake = false;

        m_video.playOnAwake               = false;
        m_video.waitForFirstFrame         = true;
        m_video.source                    = VideoSource.Url;
        m_video.renderMode                = VideoRenderMode.CameraFarPlane;
        m_video.targetCamera              = m_CGCamera;
        m_video.aspectRatio               = VideoAspectRatio.Stretch;
        m_video.audioOutputMode           = VideoAudioOutputMode.AudioSource;
        m_video.controlledAudioTrackCount = 1;

        m_video.EnableAudioTrack(0, true);
        m_video.SetTargetAudioSource(0, audio);

        var vimg = m_video.GetComponentDefault<Image>();
        var vbtn = m_video.GetComponentDefault<Button>();

        vimg.color = Color.black.SetAlpha(0);
        vbtn.targetGraphic = vimg;
        vbtn.onClick.AddListener(_Stop);

        m_video.loopPointReached += OnCGComplete;
        m_video.errorReceived += OnCGError;

        gameObject.SetActive(false);
        m_CGCamera.gameObject.SetActive(false);

        OnUICanvasFit();

        EventManager.AddEventListener(Events.UI_CANVAS_FIT, OnUICanvasFit);
    }
    
    /// <summary>
    /// 播放指定的 CG
    /// </summary>
    /// <param name="cg"></param>
    public void _Play(string cg, Action onComplete = null)
    {
        _Stop();

        m_onVideoComplete = onComplete;
        m_currentPlaying = cg;

        var path = AssetBundles.AssetManager.GetVideoAssetUrl(m_currentPlaying);
        if (string.IsNullOrEmpty(path))
        {
            Logger.LogError($"CGManager: Failed to play CG <color=#66EEAA><b>[{m_currentPlaying}]</b></color>");

            m_currentPlaying = string.Empty;

            var tmp = m_onVideoComplete;
            m_onVideoComplete = null;
            tmp?.Invoke();

            return;
        }

        Logger.LogDetail($"CGManager: Play CG <color=#66EEAA><b>[{m_currentPlaying}|{path}]</b></color>");

        if (Level.current)
        {
            m_oldVolume = Level.current.audioHelper.globalVolume;
            Level.current.audioHelper.SetGlobalVolume(0);
        }

        gameObject.SetActive(true);
        m_CGCamera.gameObject.SetActive(true);

        m_video.url = path;
        m_video.Play();
    }

    /// <summary>
    /// 停止正在播放的 CG
    /// </summary>
    public void _Stop()
    {
        if (string.IsNullOrEmpty(m_currentPlaying)) return;

        Logger.LogDetail("CGManager: Stop CG.");

        m_video.Stop();
        OnCGComplete(m_video);
    }

    /// <summary>
    /// 暂停正在播放的 CG
    /// </summary>
    public void _Pause()
    {
        Logger.LogDetail("CGManager: Pause CG.");
        m_video.Pause();
    }

    private void OnCGError(VideoPlayer v, string message)
    {
        Logger.LogError($"CGManager: Failed to play CG <color=#66EEAA><b>[{m_currentPlaying}]</b></color>");
        Logger.LogError(message);

        m_video.Stop();
        OnCGComplete(m_video);
    }

    private void OnCGComplete(VideoPlayer v)
    {
        if (!gameObject.activeSelf) return;
        gameObject.SetActive(false);
        m_CGCamera.gameObject.SetActive(false);

        Level.current?.audioHelper?.SetGlobalVolume(m_oldVolume);

        Logger.LogDetail($"CGManager: CG <color=#66EEAA><b>[{m_currentPlaying}]</b></color> completed.");

        m_currentPlaying = string.Empty;

        var tmp = m_onVideoComplete;
        m_onVideoComplete = null;
        tmp?.Invoke();
    }

    private void OnUICanvasFit()
    {
        if (!m_video) return;

        var aspect = UIManager.aspect;
        var refAspect = 2.35f;

        m_video.aspectRatio = aspect > refAspect ? VideoAspectRatio.FitVertically : VideoAspectRatio.FitHorizontally;
    }

    #endregion
}