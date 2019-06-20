// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-07-26      10:36
//  * LastModify：2018-07-26      10:36
//  ***************************************************************************************************/

using System;
using UnityEngine;

public class TweenPosition3D : TweenBase3D
{
    public AnimationCurve xCurve;
    public AnimationCurve yCurve;
    public AnimationCurve zCurve;

    protected override void OnTween()
    {
        target.position = new Vector3(xCurve.Evaluate(timer), yCurve.Evaluate(timer), zCurve.Evaluate(timer));
    }

    public static TweenPosition3D Start(Transform target, float duration, Vector3 form, Vector3 to, float radian = 0)
    {
        var tween = target.GetComponentDefault<TweenPosition3D>();
        tween.target = target;
        tween.duration = duration;

        tween.xCurve = new AnimationCurve();
        tween.xCurve.AddKey(new Keyframe(       0, form.x, radian, radian));
        tween.xCurve.AddKey(new Keyframe(duration,   to.x, radian, radian));

        tween.yCurve = new AnimationCurve();
        tween.yCurve.AddKey(new Keyframe(       0, form.y, radian, radian));
        tween.yCurve.AddKey(new Keyframe(duration,   to.y, radian, radian));


        tween.zCurve = new AnimationCurve();
        tween.zCurve.AddKey(new Keyframe(       0, form.z, radian, radian));
        tween.zCurve.AddKey(new Keyframe(duration,   to.z, radian, radian));

        tween.Play();
        return tween;
    }
}
