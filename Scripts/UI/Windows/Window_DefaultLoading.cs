/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Default loading window.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-05
 * 
 ***************************************************************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Window_DefaultLoading : Window
{
    public const string defaultName = "window_defaultLoading";

    private Image            m_progress      = null;
    private float            m_startProgress = 0.0f;
    private GameObject       m_loadingInfo   = null;
    private Text             m_tipText       = null;
    private GameObject       m_firstMark     = null;
    private GameObject       m_blackMark     = null;
    private bool             m_reset         = false;
    private Text             m_waitTipText   = null;
    private Coroutine        m_waitNextInfo  = null;
    private int              m_infoID        = 0;

    protected override void OnOpen()
    {
        enableUpdate   = false;

        m_firstMark = GetComponent<RectTransform>("decoration")?.gameObject;
        m_blackMark = GetComponent<Image>("blackMark")?.gameObject;

        if (!m_blackMark)
        {
            m_blackMark = transform.AddUINodeStrech("blackMark").gameObject;
            var mask = m_blackMark.GetComponentDefault<Image>();
            mask.color = Color.black;
        }

        m_loadingInfo = AssetBundles.AssetManager.GetLoadedAsset<GameObject>("loadinginfo");
        m_loadingInfo.name = "loadinginfo";

        Util.AddChild(transform, m_loadingInfo.transform);
        m_loadingInfo.rectTransform().anchoredPosition = new Vector2(0, 0);

        m_loadingInfo.SetActive(true);

        m_progress    = GetComponent<Image>("loadinginfo/progress/bar");
        m_tipText     = GetComponent<Text>("loadinginfo/info");
        m_waitTipText = GetComponent<Text>("loadinginfo/wait_info");

        m_progress.fillAmount = 0.0f;

        m_waitTipText.SafeSetActive(false);

        m_blackMark.transform.SetAsLastSibling();
        m_blackMark.SetActive(false);

        EventManager.AddEventListener(Events.SCENE_LOAD_PROGRESS, OnProgress);
    }

    protected override void OnWillBecameVisible(bool oldState, bool forward)
    {
        var p = GetWindowParam<Window_DefaultLoading>();
        var mode = p != null && p.param1 != null ? (int)p.param1 : 0;
        m_blackMark.SetActive(mode == 1);

        Util.SetLayer(gameObject, mode != 0 && mode != 1 ? Layers.INVISIBLE : Layers.UI);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        if (moduleLoading.firstLoading)
        {
            var r = GetComponent<UIRandomEvent>("bg");
            if (r) r.oneShot = false;
        }

        var infoActive = !moduleLoading.firstLoading || Level.nextLevel != 0;

        m_infoID = Level.currentLoadingInfoID;

        m_loadingInfo.SetActive(infoActive);
        m_firstMark?.SetActive(!infoActive);

        m_startProgress = !m_reset ? m_progress.fillAmount : 0;
        if (!oldState) StartCoroutine(UpdateInfoText(GeneralConfigInfo.sloadingInfoSwitch));

        m_reset = false;
    }

    protected override void _Hide(bool forward, bool immediately, bool destroy)
    {
        if (m_waitNextInfo != null) StopCoroutine(m_waitNextInfo);
        m_waitNextInfo = null;

        m_reset = true;
        base._Hide(forward, immediately, destroy);
    }

    protected override void OnHide(bool forward)
    {
        if (m_progress) m_progress.fillAmount = 0;

        Util.SetText(m_tipText,     string.Empty);
        Util.SetText(m_waitTipText, string.Empty);

        m_waitTipText.SafeSetActive(false);
    }

    protected override void OnReturn() { }

    private void OnProgress(Event_ e)
    {
        if (!m_progress) return;

        var p = (float)e.param1;
        m_progress.fillAmount = m_startProgress + (1.0f - m_startProgress) * p;
    }

    private void SetLoadingInfo(string info)
    {
        if (!actived) return;

        if (m_waitNextInfo != null) StopCoroutine(m_waitNextInfo);

        m_waitTipText.SafeSetActive(true);
        if (info == null) m_waitNextInfo = StartCoroutine(UpdateInfoText(GeneralConfigInfo.sloadingInfoSwitch));
        else
        {
            m_waitNextInfo = null;
            Util.SetText(m_tipText, info);
        }
    }

    private IEnumerator UpdateInfoText(float next)
    {
        if (!m_tipText) yield break;

        Util.SetText(m_tipText, LoadingInfo.SelectRandomInfo(m_infoID));

        if (next <= 0) yield break;

        var wait = new WaitForSeconds(next);
        yield return wait;

        StartCoroutine(UpdateInfoText(next));
    }
    
    private void _ME(ModuleEvent<Module_Loading> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Loading.EventSetLoadingInfo: SetLoadingInfo(e.param1 as string); break;
            default: break;
        }
    }
}
