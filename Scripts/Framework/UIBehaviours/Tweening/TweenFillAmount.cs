/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 *  
* Tween script for fillamout of filled image
 *
 * Author:   Y.Moon<chglove@live.cn>
 * Version:  0.1
 * Created:  2018-05-08
 *
 ***************************************************************************************************/

using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

[AddComponentMenu("HYLR/Tween/Tween Fill Amount")]
public class TweenFillAmount : TweenBase
{
    [Space(5), Header("Fill Amount")]
    public float from = 0f;
    public float to   = 1f;

    private Image m_image;

    protected override Tweener OnTween()
    {
        if (!m_image) m_image = this.GetComponentDefault<Image>();

        var f = currentAsFrom ? m_image.fillAmount : m_forward ? from : to;
        var t = m_forward ? to : from;

        m_image.fillAmount = f;
        return m_image.DOFillAmount(t, m_duration);
    }

    protected override void OnReset()
    {
        base.OnReset();

        if (m_image) m_image.fillAmount = from;
    }
}
