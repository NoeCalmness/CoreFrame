/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Main  scene.
 * 
 * Author:    wangyifan
 * Version:  0.0
 * Created:  2017-07-05
 * 
 ***************************************************************************************************/
using UnityEngine;
using UnityEngine.EventSystems;

public class RotateOnDrop : MonoBehaviour, IDragHandler
{
    public Transform target;
    public float speed=1f;
    public void OnDrag(PointerEventData eventData)
    {
        target.localRotation = Quaternion.Euler(0f, -0.5f * eventData.delta.x * speed, 0f)* target.localRotation;
    }
}
