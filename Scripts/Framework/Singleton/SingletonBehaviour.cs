﻿/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Singleton class for MonoBehaviour script.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-05
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonBehaviour : MonoBehaviour, IEventDispatcher
{
    public bool dispatching { get; private set; }
    public int listenersCount { get { return m_listeners.Count; } }
    public int dispatchCount { get; private set; }
    private bool destroyed = false;

    private Dictionary<string, DispatchState[]> m_dispatchStates = new Dictionary<string, DispatchState[]>();
    private Dictionary<string, List<EventListener>> m_listeners = new Dictionary<string, List<EventListener>>();
    private Dictionary<string, int> m_states = new Dictionary<string, int>();

    protected virtual void Awake()
    {
        EventManager.RegisterDispatcher(this);
    }

    protected virtual void OnDestroy()
    {
        if (destroyed) return;

        destroyed   = true;
        dispatching = false;
        dispatchCount = 0;

        foreach (var pair in m_listeners) pair.Value.Clear(true);

        m_dispatchStates.Clear();
        m_listeners.Clear();
        m_states.Clear();

        EventManager.RemoveEventListener(this);
        EventManager.UnregisterDispatcher(this);
    }

    #region IEventDispatcher implementation

    public EventListener AddEventListener(string name, VoidHandler handler)
    {
        return _AddEventListener(name, handler);
    }

    public EventListener AddEventListener(string name, NormalHandler handler)
    {
        return _AddEventListener(name, handler);
    }

    public EventListener AddEventListener<T>(string name, DynamicHandler<T> handler) where T : Event_
    {
        return _AddEventListener(name, handler);
    }

    public EventListener AddModuleListener<T>(string name, ModuleHandler<T> handler) where T : Module
    {
        if (handler == null || handler.Method.GetParameters().Length != 1 || !handler.Method.GetParameters()[0].ParameterType.IsSubclassOf(typeof(ModuleEvent))) return null;
        return _AddEventListener(ModuleEvent.GLOBAL, handler, name);
    }

    public EventListener AddModuleListener(string name, Delegate handler)
    {
        if (handler == null || handler.Method.GetParameters().Length != 1 || !handler.Method.GetParameters()[0].ParameterType.IsSubclassOf(typeof(ModuleEvent))) return null;
        return _AddEventListener(ModuleEvent.GLOBAL, handler, name);
    }

    public void RemoveEventListener(string name, VoidHandler handler)
    {
        _RemoveEventListener(name, handler);
    }

    public void RemoveEventListener(string name, NormalHandler handler)
    {
        _RemoveEventListener(name, handler);
    }

    public void RemoveEventListener<T>(string name, DynamicHandler<T> handler) where T : Event_
    {
        _RemoveEventListener(name, handler);
    }

    public void RemoveModuleListener<T>(ModuleHandler<T> handler) where T : Module
    {
        _RemoveEventListener(ModuleEvent.GLOBAL, handler);
    }

    public void RemoveModuleListener(Delegate handler)
    {
        _RemoveEventListener(ModuleEvent.GLOBAL, handler);
    }

    public void RemoveEventListener(object receiver)
    {
        if (destroyed || receiver == null) return;

        var lsss = m_listeners.Values;
        foreach (var lss in lsss)
        {
            for (var i = lss.Count - 1; i > -1; --i)
            {
                var ls = lss[i];
                var rc = ls?.handler?.Target;
                if (ls && rc == receiver)
                {
                    var si = m_states[ls.name] - 1;
                    if (si > -1)
                    {
                        var ss = m_dispatchStates.Get(ls.name);
                        if (ss != null) ss[si].UpdateState(i);
                    }
                    lss.Remove(ls);
                    ls.Destroy();
                }
            }
        }
    }

    public void DispatchEvent(string name, Event_ e = null, bool pool = true)
    {
        _DispatchEvent(name, ref pool, ref e);
        if (pool && e != null) Event_.Back(e);
    }

    private EventListener _AddEventListener(string name, Delegate handler, string extendInfo = null)
    {
        if (destroyed) return null;

        var lss = m_listeners.GetDefault(name);
        var ls = lss.Find(l => l.EqualsTo(handler));
        if (ls) return ls;

        ls = EventListener.Create(name, handler, this, extendInfo);
        lss.Add(ls);

        m_states.GetDefault(name);    // we always add default event queue state when add a new event listener

        return ls;
    }

    private void _RemoveEventListener(string name, Delegate handler)
    {
        if (destroyed) return;

        var lss = m_listeners.Get(name);
        if (lss == null) return;

        var ri = lss.FindIndex(l => l.EqualsTo(handler));

        if (ri < 0) return;

        var si = m_states[name] - 1;
        if (si > -1)
        {
            var ss = m_dispatchStates.Get(name);
            if (ss != null) ss[si].UpdateState(ri);
        }

        var ls = lss[ri];
        lss.RemoveAt(ri);
        ls.Destroy();
    }

    private void _DispatchEvent(string name, ref bool pool, ref Event_ e)
    {
        if (destroyed) return;

        var queue = m_states.Get(name);
        if (queue >= EventManager.MAX_ALLOWED_QUEUE)
        {
            Logger.LogError("EventDispatcher event [{0}] reached the max allowed event recursion queue size, this may cause stack overflow, ignored. Max allowed queue size: [{1}]", name, EventManager.MAX_ALLOWED_QUEUE);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPaused = true;
#endif
            return;
        }

        var glss = EventManager.GetGlobalListeners(name);
        var lss  = m_listeners.Get(name);

        int size = lss == null ? 0 : lss.Count, gsize = glss == null ? 0 : glss.Count, total = size + gsize;
        if (total < 1) return;

        m_states[name] = queue + 1;

        var ss = m_dispatchStates.Get(name);
        if (ss == null)
        {
            ss = new DispatchState[EventManager.MAX_ALLOWED_QUEUE];
            for (var i = 0; i < ss.Length; ++i) ss[i] = new DispatchState();
            m_dispatchStates.Add(name, ss);
        }

        var s = ss[queue];
        s.Set(size, gsize, total, 0);

        if (gsize > 0) EventManager.AddLinkedState(name, s);

        dispatchCount++;
        dispatching = true;

        if (e == null) { e = Event_.defaultEvent; pool = true; }
        e.SetInfo(name, this, false, true);

        for (; s.i < s.t; ++s.i)
        {
            var ls = s.i < s.s ? lss[s.i] : glss[s.i - s.s];
            if (!ls || !ls.Invoke(e)) continue;

            if (destroyed || e.cancle) break;
        }

        if (gsize > 0) EventManager.RemoveLinkedState(name, s);

        glss = null;
        dispatching = --dispatchCount < 1;

        if (destroyed) return;

        queue = m_states[name] - 1;
        m_states[name] = queue;
    }

    #endregion
}

public abstract class SingletonBehaviour<T> : SingletonBehaviour where T : SingletonBehaviour<T>
{
    public static T instance { get; private set; }

    protected override void Awake()
    {
        if (instance)
        {
            Logger.LogError("Could not create SingletonBehaviour<{0}> component twice, ignore.", typeof(T).Name);
            Destroy(this);
            return;
        }

        if (transform.parent)
        {
            var singleton = transform.parent.GetComponent<SingletonBehaviour>();
            if (!singleton)
            {
                Logger.LogException("SingletonBehaviour only work for root GameObjects or GameObjects under SingletonBehaviour GameObjects, ignore.");
                Destroy(this);
                return;
            }
        }
        else DontDestroyOnLoad(transform.gameObject);

        base.Awake();

        instance = (T)this;
    }

    override protected void OnDestroy()
    {
        base.OnDestroy();

        if (!instance || instance != this) return;
        instance = null;
    }
}
