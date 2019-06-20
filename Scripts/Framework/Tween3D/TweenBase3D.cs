// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-07-26      10:33
//  * LastModify：2018-07-26      10:33
//  ***************************************************************************************************/

using UnityEngine;

public abstract class TweenBase3D : MonoBehaviour
{
    public Transform target;
    public float duration;

    protected float timer;

    protected abstract void OnTween();

    private void Update()
    {
        if (timer > duration)
        {
            enabled = false;
            return;
        }
        timer += Time.deltaTime;
        OnTween();
    }

    public void Play()
    {
        timer = 0;
        enabled = true;
    }
}
