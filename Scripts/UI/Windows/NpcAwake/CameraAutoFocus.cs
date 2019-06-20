// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-01-22      13:30
//  *LastModify：2019-01-22      13:30
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.Events;

public class FocusFinish : UnityEvent
{
}

public class CameraAutoFocus : MonoBehaviour
{
    public CameraData       data;
    public float            focusTime = 1;
    public FocusFinish      onFocusFinish = new FocusFinish();
    private Camera          target;
    private float           timer;

    public void StartFocus(Camera rCamera, CameraData rData, float rTime = 0.3f)
    {
        data        = rData;
        target      = rCamera;
        focusTime   = rTime;
        timer       = 0;
        enabled     = true;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        var t = timer/focusTime;
        target.transform.position = Vector3.Lerp(target.transform.position, data.position, t);
        var a = target.transform.rotation.eulerAngles;
        var x = Mathf.LerpAngle(a.x, data.euler.x, t);
        var y = Mathf.LerpAngle(a.y, data.euler.y, t);
        var z = Mathf.LerpAngle(a.z, data.euler.z, t);
        target.transform.rotation = Quaternion.Euler(x, y, z);
        target.fieldOfView = Mathf.Lerp(target.fieldOfView, data.fov, t);
        if (timer >= focusTime)
        {
            enabled = false;
            onFocusFinish.Invoke();
        }
    }
}