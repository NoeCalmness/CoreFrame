/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Base Singleton class template.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-02-27
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;

public abstract class Singleton<T> : IEventDispatcher where T : Singleton<T>, new()
{
    public static readonly T instance = new T();

    protected Singleton()
    {
        EventManager.RegisterDispatcher(this);
    }

    #region IEventDispatcher implementation

    public bool dispatching { get; private set; }
    public int listenersCount { get { return m_listeners.Count; } }
    public int dispatchCount { get; private set; }

    private Dictionary<string, DispatchState[]> m_dispatchStates = new Dictionary<string, DispatchState[]>();
    private Dictionary<string, List<EventListener>> m_listeners = new Dictionary<string, List<EventListener>>();
    private Dictionary<string, int> m_states = new Dictionary<string, int>();

    public EventListener AddEventListener(string name, VoidHandler handler)
    {
        return _AddEventListener(name, handler);
    }

    public EventListener AddEventListener(string name, NormalHandler handler)
    {
        return _AddEventListener(name, handler);
    }

    public EventListener AddEventListener<T1>(string name, DynamicHandler<T1> handler) where T1 : Event_
    {
        return _AddEventListener(name, handler);
    }

    public EventListener AddModuleListener<T1>(string name, ModuleHandler<T1> handler) where T1 : Module
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

    public void RemoveEventListener<T1>(string name, DynamicHandler<T1> handler) where T1 : Event_
    {
        _RemoveEventListener(name, handler);
    }

    public void RemoveModuleListener<T1>(ModuleHandler<T1> handler) where T1 : Module
    {
        _RemoveEventListener(ModuleEvent.GLOBAL, handler);
    }

    public void RemoveModuleListener(Delegate handler)
    {
        _RemoveEventListener(ModuleEvent.GLOBAL, handler);
    }

    public void RemoveEventListener(object receiver)
    {
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

            if (e.cancle) break;
        }

        if (gsize > 0) EventManager.RemoveLinkedState(name, s);

        glss = null;
        dispatching = --dispatchCount < 1;

        queue = m_states[name] - 1;
        m_states[name] = queue;
    }

    #endregion
}