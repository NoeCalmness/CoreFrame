/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Manage delay events.
 * All delay events use MonoBehaviour's Coroutine function.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-04-07
 * 
 ***************************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void DelayEventHandler();
public delegate void DelayEventHandler1(object param1);

[AddComponentMenu("HYLR/Utilities/Delay Events")]
public class DelayEvents : SingletonBehaviour<DelayEvents>
{
    private class DelayEvent : IDestroyable
    {
        private static Pool<DelayEvent> m_pool = new Pool<DelayEvent>();

        public static DelayEvent Create(double delay, int id, string name, DelayEventHandler handler, bool useLogic)
        {
            var e = m_pool.Pop();

            e.destroyed         = false;
            e.delay             = delay;
            e.id                = id;
            e.name              = name;
            e.handler           = handler;
            e.handler1          = null;
            e.param1            = null;
            e.useLogicUpdate    = useLogic;
            if(!e.useLogicUpdate) e.routine   = instance.StartCoroutine(instance.Wait((float)delay, e));
            instance.m_events.Add(e);

            return e;
        }

        public static DelayEvent Create(double delay, int id, string name, DelayEventHandler1 handler, bool useLogic, object param1)
        {
            var e = m_pool.Pop();

            e.destroyed         = false;
            e.delay             = delay;
            e.id                = id;
            e.name              = name;
            e.handler           = null;
            e.handler1          = handler;
            e.param1            = param1;
            e.useLogicUpdate    = useLogic;
            if (!e.useLogicUpdate) e.routine = instance.StartCoroutine(instance.Wait((float)delay, e));

            instance.m_events.Add(e);

            return e;
        }

        public double delay;
        public int id { get; protected set; }
        public string name { get; protected set; }
        public DelayEventHandler handler { get; protected set; }
        public DelayEventHandler1 handler1 { get; protected set; }
        public Coroutine routine { get; set; }
        public bool destroyed { get; protected set; }
        public bool pendingDestroy { get { return false; } }
        public bool useLogicUpdate { get; private set; } = false;

        private object param1;

        public DelayEvent()
        {
            id = 0;
            name = "";
            handler = null;
            routine = null;
            destroyed = false;
        }

        public DelayEvent(int _id, string _name, DelayEventHandler _handler)
        {
            id   = _id;
            name = _name;
            handler = _handler;
            routine = null;
            destroyed = false;
        }

        public void Destroy()
        {
            if (destroyed) return;
            destroyed = true;

            instance.m_events.Remove(this);
            if(routine != null) instance.StopCoroutine(routine);
            useLogicUpdate = false;

            id = 0;
            name = null;
            handler = null;
            handler1 = null;
            routine = null;
            param1 = null;

            m_pool.Back(this);
        }

        public void Invoke()
        {
            if (destroyed) return;
            handler?.Invoke();
            handler1?.Invoke(param1);
        }
    }

    private List<DelayEvent> m_events = new List<DelayEvent>();

    private int m_eventID = 0;
    public double deltaTime { get; private set; }

    public static int Add(DelayEventHandler handler, float delay, string name = "")
    {
        return instance._Add(handler, delay, false, name);
    }

    public static int AddLogicDelay(DelayEventHandler handler, double delay, string name = "")
    {
        return instance._Add(handler, delay, true, name);
    }

    public static void Remove(int id)
    {
        instance._Remove(id);
    }

    public static void Remove(string name)
    {
        instance._Remove(name);
    }

    public static void Remove(DelayEventHandler handler)
    {
        instance._Remove(handler);
    }

    public static void RemoveAll(object target = null)
    {
        instance._RemoveAll(target);
    }

    public int _Add(DelayEventHandler handler, double delay, bool useLogicUpdate = false, string name = "")
    {
        if (handler == null) return 0;

        if (!string.IsNullOrEmpty(name))
        {
            var ee = m_events.Find(d => d.name == name);
            if (ee != null) ee.Destroy();
        }

        var e = DelayEvent.Create(delay, ++m_eventID, name, handler, useLogicUpdate);
        return e.id;
    }

    public void _Remove(int id)
    {
        if (id < 1) return;

        var ee = m_events.Find(d => d.id == id);
        if (ee != null) ee.Destroy();
    }

    public void _Remove(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        var ee = m_events.Find(d => d.name == name);
        if (ee != null) ee.Destroy();
    }

    public void _Remove(DelayEventHandler handler)
    {
        if (handler == null) return;

        var ee = m_events.Find(e => e.handler.Target == handler.Target && e.handler.Method == handler.Method);
        if (ee != null) ee.Destroy();
    }

    public void _RemoveAll(object target = null)
    {
        for (var i = 0; i < m_events.Count;)
        {
            var e = m_events[i];
            if (target == null || e.handler.Target == target) e.Destroy();
            else ++i;
        }
    }

    private IEnumerator Wait(float delay, DelayEvent e)
    {
        var wait = new WaitForSeconds(delay);
        yield return wait;

        e.Invoke();
        e.Destroy();
    }
    
    public static void Update(int diff)
    {
        instance._Update(diff);
    }

    public void _Update(int diff)
    {
        deltaTime = diff * 0.001;
        try
        {
            var index = 0;
            while(index < m_events.Count)
            {
                var e = m_events[index];
                if (e == null || e.destroyed || !e.useLogicUpdate)
                {
                    index++;
                    continue;
                }

                e.delay -= deltaTime;
                if (e.delay <= 0)
                {
                    e.Invoke();
                    //will change m_events.Count
                    e.Destroy();
                }
                else index++;
            }
        }
        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        catch (System.Exception e) { Logger.LogException(e); }
        #else
        catch { }
        #endif

    }
}
