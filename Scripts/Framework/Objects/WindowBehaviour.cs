/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Behaviour script of Window.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-05
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class WindowBehaviour : SceneObjectBehaviour
{
    public Window window
    {
        get { return m_window; }
        set
        {
            m_window = value;

            if (m_window) Initialize();
        }
    }
    public int animationType { get; private set; }
    /// <summary>
    /// Window is in show/hide animation (Showing or Hiding)
    /// </summary>
    public bool animating { get { return showing || hiding; } }
    /// <summary>
    /// Window is showing
    /// </summary>
    public bool showing { get; private set; }
    /// <summary>
    /// Window is hiding
    /// </summary>
    public bool hiding { get; private set; }
    /// <summary>
    /// Get/Set current window UI input state
    /// </summary>
    public bool inputState { get { return m_inputState; } set { m_inputState = value; if (rayCaster && !animating) rayCaster.enabled = m_inputState; } }
    /// <summary>
    /// If true, input event will be locked when this window enter show/hide animation state
    /// </summary>
    public bool globalLock = true;
    /// <summary>
    /// Transition animation speed
    /// <para></para>
    /// Reset when window trigger a show/hide event
    /// </summary>
    public float animationSpeed = 1.0f;

    public System.Action<bool> onAnimationComplete;

    protected bool m_inputState = true;

    protected Window m_window = null;
    protected Animator m_animator = null;

    public Canvas canvas = null;
    public CanvasGroup canvasGroup = null;
    public GraphicRaycaster rayCaster = null;
    public TweenAlpha tween = null;

    public void Show(bool immediately)
    {
        hiding = false;

        var speed = animationSpeed;
        animationSpeed = 1.0f;

        gameObject.SetActive(true);
        if (showing && !immediately) return;

        if (showing = !immediately)
        {
            if (m_animator)
            {
                canvasGroup.alpha = 1.0f;
                m_animator.speed = speed;
                m_animator.enabled = true;
                m_animator.Play(UIAnimatorState.hashIn, 0, 0);
                m_animator.Update(0);
            }
            else
            {
                tween.enabled = true;
                tween.timeScale = speed;
                tween.PlayForward();
            }
        }
        else OnAnimationState(true, true);
    }

    public void Hide(bool immediately)
    {
        showing = false;

        var speed = animationSpeed;
        animationSpeed = 1.0f;

        if (hiding && !immediately || !gameObject.activeSelf) return;

        if (hiding = !immediately)
        {
            if (m_animator)
            {
                m_animator.enabled = true;
                m_animator.speed = speed;
                m_animator.Play(UIAnimatorState.hashOut, 0, 0);
                m_animator.Update(0);
            }
            else
            {
                tween.enabled = true;
                tween.timeScale = speed;
                tween.PlayReverse();
            }
        }
        else OnAnimationState(false, true);
    }

    public void SetAnimatorCallback(UIAnimatorState state)
    {
        state.onStateChange -= OnAnimationState;
        state.onStateChange += OnAnimationState;
    }

    private void Initialize()
    {
        canvas      = this.GetComponentDefault<Canvas>();
        canvasGroup = this.GetComponentDefault<CanvasGroup>();
        rayCaster   = GetComponent<GraphicRaycaster>();

        canvasGroup.alpha = 0.0f;
        inputState = true;

        DetectAnimationType();

        this.GetComponentDefault<UIAudio>();
    }

    private void DetectAnimationType()
    {
        animationType = 0;
        if (m_animator = GetComponent<Animator>())
        {
            m_animator.enabled = false;
            m_animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animationType = 1;
        } 
        else CreateTween();
    }

    private void CreateTween()
    {
        tween = GetComponent<TweenAlpha>();
        if (!tween)
        {
            tween = this.GetComponentDefault<TweenAlpha>();
            tween.ease = DG.Tweening.Ease.Linear;
            tween.duration = GeneralConfigInfo.swindowFadeDuration;
        }

        tween.enabled         = false;
        tween.autoStart       = false;
        tween.delayStart      = 0;
        tween.loop            = false;
        tween.startVisible    = true;
        tween.currentAsFrom   = true;
        tween.ignoreTimeScale = true;
        tween.from            = 0;
        tween.to              = 1;

        tween.onStart.AddListener(show => OnAnimationState(show, false));
        tween.onComplete.AddListener(show => OnAnimationState(show, true));
    }

    private bool m_animGuard = false;
    public void OnAnimationState(bool show, bool end)
    {
        if (m_animGuard) return;
        m_animGuard = true;

        if (rayCaster) rayCaster.enabled = end && m_inputState;

        if (globalLock) UIManager.inputState = end;

        if (end)
        {
            showing = false;
            hiding  = false;

            canvasGroup.alpha = show ? 1.0f : 0.0f;

            if (tween) tween.enabled = false;

            if (m_animator)
            {
                m_animator.enabled = false;
                m_animator.Play(show ? UIAnimatorState.hashIn : UIAnimatorState.hashOut, 0, 1.1f);
                m_animator.Update(0);
            }
            gameObject.SetActive(show);
            onAnimationComplete?.Invoke(show);
        }

        m_animGuard = false;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        onAnimationComplete = null;

        tween?.Kill();

        m_window    = null;
        m_animator  = null;
        tween       = null;
        canvas      = null;
        canvasGroup = null;
        rayCaster   = null;
    }
}
