/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Tween script for rotation
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-04-07
 * 
 ***************************************************************************************************/

using UnityEngine;
using DG.Tweening;

[AddComponentMenu("HYLR/Tween/Tween Rotation")]
public class TweenRotation : TweenBase
{
    [Space(5), Header("Rotation")]
    public Vector3 from;
    public Vector3 to;

    public bool useWorldSpace = false;

    protected override int resetType { get { return 1 << RESET_ROTATION; } }

    protected override Tweener OnTween()
    {
        var f = m_forward ? from : to;
        var t = m_forward ? to : from;

        if (useWorldSpace)
        {
            transform.eulerAngles = f;
            return transform.DORotate(t, duration);
        }
        transform.localEulerAngles = f;
        return transform.DOLocalRotate(t, duration);
    }
}
