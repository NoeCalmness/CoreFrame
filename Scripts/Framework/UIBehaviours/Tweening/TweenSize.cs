/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Tween script for RectTransform size delta
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-06-07
 * 
 ***************************************************************************************************/

using UnityEngine;
using DG.Tweening;

[AddComponentMenu("HYLR/Tween/Tween Size")]
public class TweenSize : TweenBase
{
    [Space(5), Header("Size")]
    public Vector2 from = new Vector2(0, 0);
    public Vector2 to   = new Vector2(0, 0);
    public bool toAsAdditive = false;

    private Vector2 m_now;

    protected override int resetType { get { return 1 << RESET_SIZE; } }

    protected override Tweener OnTween()
    {
        if (!m_rc) return null;

        var f = currentAsFrom ? m_rc.sizeDelta : m_forward ? from : toAsAdditive ? from + to : to;
        var t = m_forward ? toAsAdditive ? from + to : to : from;

        m_rc.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, f.x);
        m_rc.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   f.y);

        m_now = f;
        var tween = DOTween.To(() => m_now, SetAnchoredSize, t, duration);

        return tween;
    }

    private void SetAnchoredSize(Vector2 size)
    {
        m_now = size;
        m_rc.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_now.x);
        m_rc.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   m_now.y);
    }
}
