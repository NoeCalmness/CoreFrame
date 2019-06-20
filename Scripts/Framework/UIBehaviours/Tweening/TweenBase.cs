/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Tween script for alpha
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-01-20
 * 
 ***************************************************************************************************/

using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public abstract class TweenBase : MonoBehaviour
{
    protected const int RESET_POSITION = 0, RESET_ROTATION = 1, RESET_SCALE = 2, RESET_SIZE = 3;

    [System.Serializable] public class OnState : UnityEvent<bool> { }

    public float duration
    {
        get { return m_duration; }
        set
        {
            if (m_duration == value) return;
            m_duration = value;

            if (loop && m_t != null && m_t.IsPlaying()) Tween();
        }
    }
    public Ease ease
    {
        get { return m_ease; }
        set
        {
            if (m_ease == value) return;
            m_ease = value;

            if (loop) SetEase(m_t);
        }
    }
    public float timeScale
    {
        get { return m_timeScale; }
        set
        {
            if (m_timeScale == value) return;
            m_timeScale = value;

            if (m_t != null) m_t.timeScale = m_timeScale;
        }
    }

    public bool playing { get { return m_t != null; } }

    [Header("Base")]
    public bool  autoStart       = true;
    public bool  oneShot         = true;
    public bool  currentAsFrom   = false;
    public bool  ignoreTimeScale = false;
    public float delayStart      = 0;

    [SerializeField, Set("duration")]
    protected float m_duration = 1.0f;
    [SerializeField, Set("timeScale")]
    protected float m_timeScale = 1.0f;

    public OnState onStart    = new OnState();
    public OnState onComplete = new OnState();

    [Space(5), Header("Loop")]
    public bool loop = false;
    public int loopCount = 1;
    public LoopType loopType = LoopType.Restart;

    [Space(5), Header("Animation")]
    [SerializeField, Set("ease")]
    protected Ease m_ease = Ease.Linear;
    public AnimationCurve curve = new AnimationCurve();

    protected Tweener m_t;
    protected RectTransform m_rc;

    protected bool m_firstEnable = true;
    protected bool m_forward = true;
    protected bool m_paused = false;
    protected Vector2 m_startPosition = Vector2.zero, m_startSize = Vector2.zero;
    protected Vector3 m_startRotation = Vector3.zero, m_startScale = Vector3.one;

    protected virtual int resetType { get { return 0; } }
    
    public void Pause()
    {
        m_paused = true;
        m_t?.Pause();
    }

    public void Resum()
    {
        m_paused = false;
        m_t?.Play();
    }

    public void PauseResum(bool pause)
    {
        m_paused = pause;

        if (m_t == null) return;
        if (pause) m_t.Pause();
        else m_t.Play();
    }

    public void ResumPause(bool resum)
    {
        PauseResum(!resum);
    }

    public void TogglePause()
    {
        m_paused = !m_paused;
        m_t?.TogglePause();
    }

    public void Play(bool forward = true)
    {
        Tween(forward);
    }

    public void ReversePlay(bool reverse = true)
    {
        Tween(!reverse);
    }

    public void PlayComplete(bool forward = true)
    {
        Tween(forward);

        m_t?.Kill(true);
        m_t = null;
    }

    public void ReversePlayComplete(bool reverse = true)
    {
        Tween(!reverse);

        m_t?.Kill(true);
        m_t = null;
    }

    public void Kill(bool reset = false)
    {
        m_paused = false;
        m_t?.Kill();
        m_t = null;

        if (reset) OnReset();
    }

    [ContextMenu("Play Reverse")]
    public void PlayReverse()
    {
        Tween(false);
    }

    [ContextMenu("Play Forward")]
    public void PlayForward()
    {
        Tween(true);
    }

    protected virtual void Start()
    {
        m_rc = this.rectTransform();

        m_startPosition = m_rc ? m_rc.anchoredPosition : Vector2.zero;
        m_startSize     = m_rc ? m_rc.rect.size : Vector2.zero;
        m_startRotation = transform.localEulerAngles;
        m_startScale    = transform.localScale;

        if (!autoStart) return;

        Tween();
    }

    private void OnEnable()
    {
        if (m_t != null)
        {
            if (!m_paused)
                m_t.Play();
        }
        else if (autoStart && !oneShot && !m_firstEnable) Tween();

        m_firstEnable = false;
    }

    private void OnDisable()
    {
        if (m_t == null) return;
        if (!oneShot && !gameObject.activeInHierarchy)
        {
            if (autoStart)
            {
                m_t.Kill();
                m_t = null;
            }

            OnReset();
        }
        else m_t.Pause();
    }

    private void OnDestroy()
    {
        onStart.RemoveAllListeners();
        onComplete.RemoveAllListeners();
    }

    private void Tween(bool _forward = true)
    {
        #region Editor

        #if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying) return;
        #endif

        #endregion

        m_paused = false;
        m_forward = _forward;
        m_t?.Kill();

        m_t = OnTween();
        SetTweener(m_t);

        onStart?.Invoke(m_forward);
    }

    protected virtual void SetEase(Tweener t)
    {
        if (t == null) return;
        if (m_ease != Ease.Unset || curve == null || curve.length < 1) t.SetEase(m_ease == Ease.Unset ? Ease.Linear : m_ease);
        else t.SetEase(curve);
    }

    protected virtual Tweener SetTweener(Tweener t)
    {
        if (t == null) return t;
        SetEase(t);
        t.timeScale = m_timeScale;
        t.SetLoops(loopCount, loopType).SetDelay(delayStart).SetUpdate(ignoreTimeScale).OnComplete(OnComplete);
        return t;
    }

    protected virtual Tweener OnTween()
    {
        return null;
    }

    protected virtual void OnComplete()
    {
        m_paused = false;
        onComplete?.Invoke(m_forward);
    }

    protected virtual void OnReset()
    {
        var rt = resetType;
        if (rt == 0) return;
        
        if (m_rc)
        {
            if (rt.BitMask(RESET_POSITION)) m_rc.anchoredPosition = m_startPosition;
            if (rt.BitMask(RESET_SIZE))
            {
                m_rc.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_startSize.x);
                m_rc.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_startSize.y);
            }
        }

        if (rt.BitMask(RESET_ROTATION)) transform.localEulerAngles = m_startRotation;
        if (rt.BitMask(RESET_SCALE))    transform.localScale       = m_startScale;      
    }
}
