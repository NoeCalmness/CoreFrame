using UnityEngine;
using DG.Tweening;
using System;

public class UIEyeTween : MonoBehaviour, IUIAnimationComplete
{
    public const string BLUR_PARAMETER_NAME = "_Distortion";

    private Tween m_t;
    private UIEyeMask m_eyeComponent;
    public AnimationCurve curve;
    public Action onComplete { get; set; }

    public Material gaussMat { get; private set; }
    public float gaussBeginValue = 8;
    public float gaussEndValue = 2;
    public AnimationCurve gaussCurve;
    private Tween m_gaussTween;

    private void Awake()
    {
        m_eyeComponent = GetComponentInChildren<UIEyeMask>();
        if (!m_eyeComponent) Logger.LogError("UIEyeMask component is null");
    }

    // Use this for initialization
    void Start ()
    {
        PlayAnim();
    }
    
    [ContextMenu("Play")]
    public void PlayAnim()
    {
        float duraction = (curve.keys == null || curve.keys.Length == 0) ? 0.1f : curve.keys[curve.keys.Length - 1].time;
        if (!m_eyeComponent) return;

        m_t?.Kill();
        m_eyeComponent.maskRage = 0;
        m_t = DOTween.To(() => m_eyeComponent.maskRage, x => m_eyeComponent.maskRage = x, 1, duraction).SetEase(curve).OnComplete(()=> {
            onComplete?.Invoke();
        });
    }

    public void PlayMatTween(Material mat)
    {
        gaussMat = mat;
        m_gaussTween?.Kill();

        float duraction = (gaussCurve.keys == null || gaussCurve.keys.Length == 0) ? 0 : gaussCurve.keys[gaussCurve.keys.Length - 1].time;
        if (duraction == 0) return;

        if (gaussMat && gaussMat.HasProperty(BLUR_PARAMETER_NAME))
        {
            gaussMat.SetFloat(BLUR_PARAMETER_NAME, gaussBeginValue);
            m_gaussTween = DOTween.To(() => gaussMat.GetFloat(BLUR_PARAMETER_NAME), x => gaussMat.SetFloat(BLUR_PARAMETER_NAME, x), gaussEndValue, duraction).SetEase(gaussCurve);
        }
    }

    public void OnDestroy()
    {
        if (m_t != null) m_t.Kill();
        if (m_gaussTween != null) m_gaussTween.Kill();
    }
}
