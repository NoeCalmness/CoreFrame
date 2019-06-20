/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-09-04
 * 
 ***************************************************************************************************/

using UnityEngine.UI;
using System;
using GuideItem = GuideInfo.GuideItem;
using UnityEngine;

public class Window_Interlude : Window
{
    #region static functions

    public static void OpenInterlude(GuideItem item, Action callback)
    {
        if (item.type != EnumGuideType.Interlude) return;

        OpenInterlude(item.titleId, item.audio, callback);
    }

    public static void OpenInterlude(int configId, string audioName, Action callback)
    {
        ShowAsync<Window_Interlude>((w)=>{
            (w as Window_Interlude).InitData(configId, audioName, callback);
        });
    }
    
    #endregion

    private Text m_title;
    private TweenAlpha m_titleTween;
    private CanvasGroup m_titleCanvas;
    private TweenAlpha m_windowTween;
    private float m_bgmVolume;

    public int titleId { get; private set; }
    public string audio { get; private set; }

    private Action m_onComplete;

    /// <summary>
    /// Called when window added to level
    /// </summary>
    protected override void OnOpen()
    {
        base.OnOpen();
        isFullScreen = false;
        m_title = GetComponent<Text>("info");
        CreateTitleTween();
        //must after CreateTitleTween
        m_titleCanvas = m_title.GetComponentDefault<CanvasGroup>();
        if (m_behaviour && m_behaviour.tween) m_windowTween = m_behaviour.tween;
    }

    private void CreateTitleTween()
    {
        if(!m_title)
        {
            Logger.LogError("title text component is null");
            return;
        }
        m_titleTween = m_title.GetComponentDefault<TweenAlpha>();
        m_titleTween.ease = DG.Tweening.Ease.Linear;
        m_titleTween.enabled = true;
        m_titleTween.oneShot = false;
        m_titleTween.autoStart = false;
        m_titleTween.loop = false;
        m_titleTween.startVisible = true;
        m_titleTween.currentAsFrom = true;
        m_titleTween.ignoreTimeScale = true;
    }
    
    public void InitData(int configId,string audioName,Action callback)
    {
        titleId = configId;
        audio = audioName;
        m_onComplete = callback;
        RefreshInterlude();
        //play sound
        AudioManager.PauseAll(AudioTypes.Music);
        SetBGMPlayState(false);
        if (!string.IsNullOrEmpty(audio)) AudioManager.PlaySound(audio);
    }
    
    private void SetBGMPlayState(bool play)
    {
        if (!Level.current.audioHelper) return;

        if (!play) m_bgmVolume = Level.current.audioHelper.globalVolume;
        float volume = play ? m_bgmVolume : 0f;
        if (play && volume == 0) volume = SettingsManager.realBgmVolume;
        Level.current.audioHelper.SetGlobalVolume(volume , StoryConst.BGM_CROSS_FADR_DURACTION);
    }

    private void RefreshInterlude()
    {
        Util.SetText(m_title, ConfigText.GetDefalutString(titleId));
        if(m_windowTween) m_windowTween.duration = GeneralConfigInfo.sinterludeWindowTime.fadeInTime;
        RefreshTitleFofward();
    }

    private void RefreshTitleFofward()
    {
        if(m_titleCanvas) m_titleCanvas.alpha = 0;

        m_titleTween.delayStart = GeneralConfigInfo.sinterludeWindowTime.remainInTime;
        m_titleTween.duration = GeneralConfigInfo.sinterludeTextTime.fadeInTime;
        m_titleTween.from = 0;
        m_titleTween.to = 1;

        m_titleTween.onComplete.RemoveAllListeners();
        m_titleTween.onComplete.AddListener(OnTitlePlayFadeInComplete);
    }

    private void OnTitlePlayFadeInComplete(bool forward)
    {
        if (m_titleCanvas) m_titleCanvas.alpha = 1;

        m_titleTween.delayStart = GeneralConfigInfo.sinterludeTextTime.remainInTime;
        m_titleTween.duration = GeneralConfigInfo.sinterludeTextTime.fadeOutTime;
        m_titleTween.from = 1;
        m_titleTween.to = 0;

        m_titleTween.onComplete.RemoveAllListeners();
        m_titleTween.onComplete.AddListener(OnTitlePlayFadeOutComplete);
        m_titleTween.PlayForward();
        
        ChangeToWindowHome();
    }

    private void OnTitlePlayFadeOutComplete(bool forward)
    {
        if (m_windowTween)
        {
            m_windowTween.delayStart = GeneralConfigInfo.sinterludeTextTime.remainOutTime + GeneralConfigInfo.sinterludeWindowTime.remainOutTime;
            m_windowTween.duration = GeneralConfigInfo.sinterludeWindowTime.fadeOutTime;
        }
        Hide();
    }

    protected override void OnShow(bool forward)
    {
        m_titleTween.PlayForward();
    }

    private void ChangeToWindowHome()
    {
        ShowAsync<Window_Home>(null, (w) =>
        {
            if (w) (w as Window_Home).SwitchTo(Window_Home.Main);
        });
    }

    protected override void OnHide(bool forward)
    {
        ClearData();
        if (!string.IsNullOrEmpty(audio)) AudioManager.Stop(audio);
        AudioManager.ResumAll(AudioTypes.Music);
        SetBGMPlayState(true);
        m_onComplete?.Invoke();
    }

    private void ClearData()
    {
        m_titleTween.Kill();
    }
}
