// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-07-26      10:50
//  * LastModify：2018-07-26      10:56
//  ***************************************************************************************************/

#region

using System;
using UnityEngine;

#endregion

public class TweenRotation3D : TweenBase3D
{
    public Vector3 form;
    public Vector3 to;
    private Action mOnComplete;

    protected override void OnTween()
    {
        target.transform.localRotation = Quaternion.Euler(Mathf.Lerp(form.x, to.x, timer/duration),
                                                          Mathf.Lerp(form.y, to.y, timer/duration), 
                                                          Mathf.Lerp(form.z, to.z, timer/duration));

        if (timer >= duration)
            mOnComplete?.Invoke();
    }

    public static TweenRotation3D Start(Transform target, Vector3 form, Vector3 to, float duration, Action onComplete = null)
    {
        var tween = target.GetComponentDefault<TweenRotation3D>();
        tween.target = target;
        tween.form = form;
        tween.to = to;
        tween.duration = duration;
        tween.Play();
        tween.mOnComplete = onComplete;
        return tween;
    }
}