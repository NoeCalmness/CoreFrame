// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-01-22      16:32
//  *LastModify：2019-01-22      16:32
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof (EventTriggerListener))]
public class ControlNodeBase : MonoBehaviour
{
    public  CameraControler Controler;
    private bool            isDrag;

    private void Awake()
    {
        var listener = EventTriggerListener.Get(gameObject);
        listener.onClick += OnClickInternal;
        listener.onPressBegin += OnBeginDrag;
        listener.onPressMove += OnDrag;
        listener.onPressEnd += OnDragEnd;

        if (Controler == null)
            Controler = GetComponentInParent<CameraControler>();
    }

    private void OnClickInternal(GameObject go)
    {
        if (isDrag) return;
        OnClick(go);
    }

    protected virtual void OnClick(GameObject go)
    {
    }

    protected virtual void OnBeginDrag(PointerEventData data)
    {
        Controler?.OnBeginDrag(data);
    }

    protected virtual void OnDrag(PointerEventData data)
    {
        Controler?.OnDrag(data);
        isDrag = true;
    }

    protected virtual void OnDragEnd(PointerEventData data)
    {
        Controler?.OnDragEnd(data);
        isDrag = false;
    }
}