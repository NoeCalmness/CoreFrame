// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-07-26      10:15
//  * LastModify：2018-07-26      10:15
//  ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;


public class AwakeLayerController : MonoBehaviour
{
    public const float Radius = 7;

    public int layer;

    public float angle;

    public AwakeController current;

    public List<AwakeController> Points;

    public void Watch(AwakeController rController)
    {
        //处于中心点。不能旋转
        if (rController.transform.localPosition.x.Equals(0))
            return;

        if (rController.info.nextInfoList.Count == 0)
            rController = rController.prev;

        var index = Points.IndexOf(rController);
        if (index == -1)
            return;
        var a = Mathf.Repeat(-index * angle * Mathf.Rad2Deg, 360);
        var b = Mathf.Repeat(transform.eulerAngles.y       , 360);
        if (a > b) a -= 360;

        TweenRotation3D.Start(transform, transform.eulerAngles, new Vector3(0, a, 0), 1);

        current = rController;
    }

    public void LayoutPoint(List<AwakeController> list)
    {
        Points = list;
        angle = Mathf.PI * 2 / (list.Count - 1);
        float a = 0;
        for (var i = 0; i < list.Count - 1; i++)
        {
            var go = list[i];
            go.layerController = this;
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * Radius;
            a += angle;
        }

        var last = list[list.Count - 1];
        last.layerController = this;
        last.transform.SetParent(transform);
        last.transform.localPosition = Vector3.up * 3;
    }

    public void DrawLine(AwakeHandle handle)
    {
        foreach (var point in Points)
            point.DrawLine(handle);
    }

#if UNITY_EDITOR
    [ContextMenu("RotateTo")]
    private void RotateTo()
    {
        Watch(current);
        
    }
#endif

    public AwakeController GetAwakeController(int index)
    {
        return Points.Find(item => item.info.index == index);
    }
}
