using UnityEngine;
using UnityEngine.EventSystems;
public class EventTriggerListener : EventTrigger
{
    public delegate void VoidDelegate(GameObject go);
    public delegate void EventDelegate(PointerEventData go);
    public VoidDelegate onClick;
    public VoidDelegate onPress;
    public VoidDelegate onDrop;
    public VoidDelegate onDown;
    public VoidDelegate onEnter;
    public VoidDelegate onExit;
    public VoidDelegate onUp;
    public VoidDelegate onSelect;
    public VoidDelegate onUpdateSelect;

    public EventDelegate onPressBegin;
    public EventDelegate onPressEnd;
    public EventDelegate onPressMove;

    private bool pointerDown, pointerUp;
    private float pressTime;

    static public EventTriggerListener Get(GameObject go)
    {
        EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
        if (listener == null) listener = go.AddComponent<EventTriggerListener>();
        return listener;
    }

    public override void OnDrop(PointerEventData eventData)
    {
        if (onDrop !=null) onDrop(gameObject);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null) onClick(gameObject);
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (onDown != null) onDown(gameObject);
        pointerDown = true;
        pointerUp = false;
        pressTime = 0;
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null) onEnter(gameObject);
        
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null) onExit(gameObject);
        
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        if (onUp != null) onUp(gameObject);
        pointerUp = true;
        pressTime = 0;
    }
    public override void OnSelect(BaseEventData eventData)
    {
        if (onSelect != null) onSelect(gameObject);
    }
    public override void OnUpdateSelected(BaseEventData eventData)
    {
        if (onUpdateSelect != null) onUpdateSelect(gameObject);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (onPressBegin != null) onPressBegin(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (onPressMove != null) onPressMove(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (onPressEnd != null) onPressEnd(eventData);
    }

    void Update()
    {
        if(pointerDown && !pointerUp)
        {
            pressTime += Time.deltaTime;
            float _press = pressTime - GeneralConfigInfo.defaultConfig.downTime;
            if (onPress != null && _press >= 0) onPress(gameObject);
        }
    }
}