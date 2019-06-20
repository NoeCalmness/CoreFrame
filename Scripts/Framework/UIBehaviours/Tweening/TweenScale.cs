/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Tween script for scale
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-01-20
 * 
 ***************************************************************************************************/

using UnityEngine;
using DG.Tweening;

[AddComponentMenu("HYLR/Tween/Tween Scale")]
public class TweenScale : TweenBase
{
    [Space(5), Header("Scale")]
    public Vector3 from;
    public Vector3 to;
    public bool startVisible = true;

    protected override int resetType { get { return 1 << RESET_SCALE; } }

    protected override Tweener OnTween()
    {
        if (startVisible) gameObject.SetActive(true);

        var f = currentAsFrom ? transform.localScale : m_forward ? from : to;
        var t = m_forward ? to : from;

        transform.localScale = f;
        return transform.DOScale(t, duration);
    }
}
