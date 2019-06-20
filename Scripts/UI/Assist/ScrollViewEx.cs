using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollViewEx : ScrollView
{
    private ScrollView parentScrollView;

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        parentScrollView = transform.parent?.GetComponentInParent<ScrollView>();
        parentScrollView?.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if(scrollType == ScrollType.Horizontal)
        {
            if (Mathf.Abs(eventData.delta.x) >= Mathf.Abs(eventData.delta.y))
                base.OnDrag(eventData);
            else
                parentScrollView?.OnDrag(eventData);
        }
        else if (scrollType == ScrollType.Vertical)
        {
            if (Mathf.Abs(eventData.delta.x) <= Mathf.Abs(eventData.delta.y))
                base.OnDrag(eventData);
            else
                parentScrollView?.OnDrag(eventData);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        parentScrollView?.OnEndDrag(eventData);
    }
}
