using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class PassEvent : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public Action OnClick { get; set; }

	// Use this for initialization
	void Start ()
    {
		
	}

    //监听按下
    public void OnPointerDown(PointerEventData eventData)
    {
        Pass(eventData, ExecuteEvents.pointerDownHandler, EventInterfaceTypes.PointerDown);
    }

    //监听抬起
    public void OnPointerUp(PointerEventData eventData)
    {
        Pass(eventData, ExecuteEvents.pointerUpHandler, EventInterfaceTypes.PointerUp);
    }

    //监听点击
    public void OnPointerClick(PointerEventData eventData)
    {
        Pass(eventData, ExecuteEvents.submitHandler, EventInterfaceTypes.Submit);
        Pass(eventData, ExecuteEvents.pointerClickHandler, EventInterfaceTypes.PointerClick);

        OnClick?.Invoke();
    }
    
    //把事件透下去
    private void Pass<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function, EventInterfaceTypes type) where T : IEventSystemHandler
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current?.RaycastAll(data, results);
        GameObject current = data.pointerCurrentRaycast.gameObject;
        for (int i = 0; i < results.Count; i++)
        {
            if (current != results[i].gameObject)
            {
                Logger.LogInfo("{0} handle click event...", results[i].gameObject);
                ExecuteEvents.Execute(results[i].gameObject, data, function, type);
                break;
            }
        }
    }
}
