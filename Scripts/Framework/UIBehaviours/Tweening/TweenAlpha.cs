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

[AddComponentMenu("HYLR/Tween/Tween Alpha")]
public class TweenAlpha : TweenBase
{
    [Space(5), Header("Alpha")]
    public float from = 0;
    public float to   = 1;
    public bool  startVisible = true;

    private CanvasGroup m_canvas;

    protected override Tweener OnTween()
    {
        if (!m_canvas) m_canvas = this.GetComponentDefault<CanvasGroup>();
        if (startVisible) gameObject.SetActive(true);

        var f = currentAsFrom ? m_canvas.alpha : m_forward ? from : to;
        var t = m_forward ? to : from;

        m_canvas.alpha = f;
        return m_canvas.DOFade(t, m_duration);
    }

    protected override void OnReset()
    {
        base.OnReset();

        if (m_canvas) m_canvas.alpha = from;
    }
}
