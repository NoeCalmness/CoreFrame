/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Tween script for anchored position
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-02-24
 * 
 ***************************************************************************************************/

using UnityEngine;
using DG.Tweening;

[AddComponentMenu("HYLR/Tween/Tween Position")]
public class TweenPosition : TweenBase
{
    [Space(5), Header("Position")]
    public Vector2 from = new Vector2(0, 0);
    public Vector2 to   = new Vector2(0, 0);
    public bool toAsAdditive = false;

    public bool lockX = false;
    public bool lockY = false;

    protected override int resetType { get { return 1 << RESET_POSITION; } }

    protected override Tweener OnTween()
    {
        if (!m_rc || lockX && lockY) return null;

        var n = m_rc.anchoredPosition;
        var f = currentAsFrom ? n : m_forward ? from : toAsAdditive ? from + to : to;
        var t = m_forward ? toAsAdditive ? from + to : to : from;
        
        if (lockX)
        {
            f.x = n.x;
            t.x = n.x;
        }

        if (lockY)
        {
            f.y = n.y;
            t.y = n.y;
        }

        m_rc.anchoredPosition = f;
        return m_rc.DOAnchorPos(t, duration);
    }
}
