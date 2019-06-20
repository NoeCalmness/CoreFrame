// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 觉醒系统上下翻页控制器
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-07-25      17:58
//  * LastModify：2018-07-25      17:58
//  ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class AwakePageController : EventTrigger
{
    public Transform dragger;

    public Vector3 targetPos;

    public bool isDraging;

    public AnimationCurve curve;

    public Vector2 range;
    [Range(0, 10)]
    public float smoothTime = 0.3f;

    [Range(0, 0.2f)]
    public float dragSpeed = 0.02f;

    [Range(0, 1)]
    public float threshold = 0.3f;

    private float timer;
    public AwakeHandle Handle { get; set; }

    private void Awake()
    {
        if (!dragger)
            dragger = transform.Find("Root");
    }

    private void OnEnable()
    {
        isDraging = false;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        isDraging = true;
    }

    public override void OnDrag(PointerEventData eventData)
    {
        isDraging = true;

        if (!dragger || Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y))
            return;

        var pos = dragger.localPosition;
        dragger.localPosition = new Vector3(pos.x, Mathf.Clamp(pos.y + eventData.delta.y * dragSpeed, range.x, range.y), pos.z);

        var layer = Mathf.RoundToInt(dragger.localPosition.y/AwakeHandle.DistanceLayer);
        Handle.FocusLayer = Mathf.Abs(layer);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        timer = 0;
        isDraging = false;
        if (!dragger)
            return;

        var offset = eventData.position - eventData.pressPosition;
        if (Math.Abs(offset.y) < 0.001f)
            return;

        var layer = 0;
        if (offset.y > 0)
        {
            var floorIndex = Mathf.FloorToInt(dragger.localPosition.y/AwakeHandle.DistanceLayer);
            if (dragger.localPosition.y - AwakeHandle.DistanceLayer*floorIndex > threshold * AwakeHandle.DistanceLayer)
                layer = floorIndex + 1;
            else
                layer = floorIndex;
        }
        else
        {
            var ceilIndex = Mathf.CeilToInt(dragger.localPosition.y/AwakeHandle.DistanceLayer);
            if (ceilIndex*AwakeHandle.DistanceLayer - dragger.localPosition.y > threshold*AwakeHandle.DistanceLayer)
                layer = ceilIndex - 1;
            else
                layer = ceilIndex;
        }

        targetPos = new Vector3(0, Mathf.Clamp(layer*AwakeHandle.DistanceLayer, range.x, range.y));

        Handle.FocusLayer = Mathf.Abs(layer);

        curve = new AnimationCurve();
        curve.AddKey(new Keyframe(0, dragger.localPosition.y, 0.3f, 0.4f));
        curve.AddKey(new Keyframe(smoothTime, targetPos.y, 0.1f, 0.2f));
        curve.preWrapMode = WrapMode.Once;
        curve.postWrapMode = WrapMode.Once;
    }

    private void Update()
    {
        if (isDraging || !dragger || null == curve) return;
        if (Vector3.Distance(dragger.localPosition, targetPos) > 0.05f)
        {
            timer += Time.deltaTime;
            dragger.localPosition = new Vector3(0, curve.Evaluate(timer), 0);
        }
        else
            curve = null;
    }
}
