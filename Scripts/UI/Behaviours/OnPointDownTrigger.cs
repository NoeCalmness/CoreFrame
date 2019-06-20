using UnityEngine;
using UnityEngine.EventSystems;

public class OnPointTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public delegate void VoidDelegate(GameObject go);

    public VoidDelegate onPointDown;
    public VoidDelegate onPointUp;

    static public OnPointTrigger Get(GameObject go)
    {
        OnPointTrigger listener = go.GetComponent<OnPointTrigger>();
        if (listener == null) listener = go.AddComponent<OnPointTrigger>();
        return listener;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (onPointDown != null) onPointDown(gameObject);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (onPointUp != null) onPointUp(gameObject);
    }
}
