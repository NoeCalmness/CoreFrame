using System;
using UnityEngine;

public class ColliderTriggerListener : MonoBehaviour
{
    #region static functions

    static public ColliderTriggerListener Get(GameObject go)
    {
        if(!go)
        {
            Logger.LogError("cannot add ColliderTriggerListener Component,the gameObject is null");
            return null;
        }
        ColliderTriggerListener listener = go.GetComponentDefault<ColliderTriggerListener>();
        return listener;
    }

    public static void AddEnterListener(GameObject go, Action<Collider> callback)
    {
        ColliderTriggerListener c = Get(go);
        if(c) c.onTriggerEnter = callback;
    }

    public static void AddStayListener(GameObject go, Action<Collider> callback)
    {
        ColliderTriggerListener c = Get(go);
        if (c) c.onTriggerStay = callback;
    }

    public static void AddExitListener(GameObject go, Action<Collider> callback)
    {
        ColliderTriggerListener c = Get(go);
        if (c) c.onTriggerExit = callback;
    }

    #endregion

    public Action<Collider> onTriggerEnter;
    public Action<Collider> onTriggerStay;
    public Action<Collider> onTriggerExit;

    private void OnTriggerEnter(Collider other)
    {
        //Logger.LogInfo("OnTriggerEnter other is {0}", other.name);
        onTriggerEnter?.Invoke(other);
    }

    private void OnTriggerStay(Collider other)
    {
        //Logger.LogInfo("OnTriggerStay other is {0}", other.name);
        onTriggerStay?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        //Logger.LogInfo("OnTriggerExit other is {0}", other.name);
        onTriggerExit?.Invoke(other);
    }
    
}
