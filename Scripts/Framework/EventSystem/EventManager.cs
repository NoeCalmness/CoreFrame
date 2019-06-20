/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * EventManager manages all game logic events/messages send by IEventDispatcher.
 * LObject already implements IEventDispatcher, all class derived from it will automatically add to
 * EventManager.
 * If you implement IEventDispatcher, you should manually add it.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-02-28
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;

[PoolSize(1000)]
public class EventListener : PoolObject<EventListener>
{
    public static EventListener Create(string name, Delegate handler, IEventDispatcher sender, string extendInfo = null)
    {
        if (handler == null)
        {
            Logger.LogException("EventListener::Create: Event handler can not be null. [event: {0}, sender: {1}, extendInfo: {2}]", name, sender?.GetType().ToString() ?? "null", extendInfo);
            return null;
        }

        var ls = Create();
        ls.Set(name, handler, sender, extendInfo);
        return ls;
    }

    public string             name           = null;
    public string             extendInfo     = null;
    public IEventDispatcher   sender         = null;
    public VoidHandler        voidHandler    = null;
    public NormalHandler      normalHandler  = null;
    public Delegate           moduleHandler  = null;
    public Delegate           dynamicHandler = null;

    public Type paramType => m_paramType;
    public Delegate handler => m_handler;

    private Type m_paramType   = null;
    private Delegate m_handler = null;

    private EventListener() { }

    public void Set(string _name, Delegate _handler, IEventDispatcher _sender, string _extendInfo = null)
    {
        name            = _name;
        extendInfo      = _extendInfo;
        voidHandler     = null;
        normalHandler   = null;
        moduleHandler   = null;
        dynamicHandler  = null;
        m_handler       = null;
        m_paramType     = null;
        sender          = _sender;

        var ps = _handler.Method.GetParameters();
        if (ps.Length == 0)
        {
            voidHandler = _handler as VoidHandler;
            m_handler = voidHandler;
        }
        else if (ps.Length > 1)
        {
            Logger.LogError("Event handler can only have one parameter. handler:{0}", _handler.Method);
            return;
        }
        else if (ps[0].ParameterType.IsSubclassOf(typeof(ModuleEvent)))
        {
            moduleHandler = _handler;
            m_paramType   = ps[0].ParameterType;
            m_handler     = moduleHandler;
        }
        else if (ps[0].ParameterType == Event_.defaultType)
        {
            normalHandler = _handler as NormalHandler;
            m_paramType   = Event_.defaultType;
            m_handler     = normalHandler;
        }
        else
        {
            dynamicHandler = _handler;
            m_paramType    = ps[0].ParameterType;
            m_handler      = dynamicHandler;
        }
    }

    public bool Invoke(Event_ e)
    {
        if (voidHandler != null) voidHandler.Invoke();
        else if (normalHandler != null) normalHandler.Invoke(e);
        else
        {
            if (moduleHandler != null)
            {
                var me = e as ModuleEvent;
                if (me == null || !me.baseModule)
                {
                    Logger.LogError("Module Event invalid! [handler:{0}, event:{1}, targetModule:{2}, evenModule:{3}]", handler.Method, me != null ? me.moduleEvent : e.name, paramType.Name, me != null && me.baseModule ? me.baseModule.GetType().Name : "null");
                    return false;
                }
                if (extendInfo != ModuleEvent.GLOBAL && me.moduleEvent != extendInfo || me.GetType() != paramType) return false;
                handler.DynamicInvoke(e);
            }
            else
            {
                if (paramType != e.GetType())
                {
                    Logger.LogError("Event handler [{0}] has wrong param type, ignore. [event:{3}, targetType:{1}, eventType:{2}]", handler.Method, paramType.Name, e.GetType().Name, e.name);
                    return false;
                }
                handler.DynamicInvoke(e);
            }
        }

        return true;
    }

    public bool EqualsTo(EventListener r)
    {
        return r == null ? false : EqualsTo(r.handler);
    }

    public bool EqualsTo(Delegate r)
    {
        return r != null && handler.Target == r.Target && handler.Method == r.Method;
    }
}

public static class EventManager
{
    private static List<IEventDispatcher>                   m_dispatchers     = new List<IEventDispatcher>();
    private static Dictionary<string, List<EventListener>>  m_removeListeners = new Dictionary<string, List<EventListener>>();
    private static Dictionary<string, List<EventListener>>  m_globalListeners = new Dictionary<string, List<EventListener>>();
    private static Dictionary<string, List<DispatchState>>  m_linkedStates    = new Dictionary<string, List<DispatchState>>();

    public const int MAX_ALLOWED_QUEUE = 3; // max event recursion queue size

    public static int listenersCount { get; private set; }

    public static void RegisterDispatcher(IEventDispatcher dispatcher)
    {
        if (dispatcher == null || m_dispatchers.Contains(dispatcher)) return;
        m_dispatchers.Add(dispatcher);
    }

    public static void UnregisterDispatcher(IEventDispatcher dispatcher)
    {
        if (dispatcher == null) return;
        m_dispatchers.Remove(dispatcher);
    }

    public static List<EventListener> GetGlobalListeners(string name)
    {
        return m_globalListeners.Get(name);
    }

    public static void AddLinkedState(string name, DispatchState linkedState)
    {
        m_linkedStates[name].Add(linkedState);
    }

    public static void RemoveLinkedState(string name, DispatchState linkedState)
    {
        var ss = m_linkedStates.Get(name);
        if (ss == null || ss.Count < 1) return;
        ss.Remove(linkedState);
    }

    public static EventListener AddEventListener(string name, VoidHandler handler)
    {
        return _AddEventListener(name, handler);
    }

    public static EventListener AddEventListener(string name, NormalHandler handler)
    {
        return _AddEventListener(name, handler);
    }

    public static EventListener AddEventListener<T>(string name, DynamicHandler<T> handler) where T : Event_
    {
        return _AddEventListener(name, handler);
    }

    public static EventListener AddModuleListener<M>(string name, ModuleHandler<M> handler) where M : Module
    {
        if (handler == null || handler.Method.GetParameters().Length != 1 || !handler.Method.GetParameters()[0].ParameterType.IsSubclassOf(typeof(ModuleEvent))) return null;
        return _AddEventListener(ModuleEvent.GLOBAL, handler, name);
    }

    public static EventListener AddModuleListener(string name, Delegate handler)
    {
        if (handler == null || handler.Method.GetParameters().Length != 1 || !handler.Method.GetParameters()[0].ParameterType.IsSubclassOf(typeof(ModuleEvent))) return null;
        return _AddEventListener(ModuleEvent.GLOBAL, handler, name);
    }

    public static void RemoveEventListener(string name, VoidHandler handler)
    {
        _RemoveEventListener(name, handler);
    }

    public static void RemoveEventListener(string name, NormalHandler handler)
    {
        _RemoveEventListener(name, handler);
    }

    public static void RemoveEventListener<T>(string name, DynamicHandler<T> handler) where T : Event_
    {
        _RemoveEventListener(name, handler);
    }

    public static void RemoveModuleListener<M>(ModuleHandler<M> handler) where M : Module
    {
        _RemoveEventListener(ModuleEvent.GLOBAL, handler);
    }

    public static void RemoveModuleListener(Delegate handler)
    {
        _RemoveEventListener(ModuleEvent.GLOBAL, handler);
    }

    public static void RemoveEventListener(EventListener ls)
    {
        if (!ls) return;

        var lss = m_globalListeners.Get(ls.name);
        if (lss == null) return;

        var ri = lss.IndexOf(ls);
        if (ri < 0) return;

        var lst = m_linkedStates[ls.name];
        foreach (var t in lst) t.UpdateState(ri, true);

        lss.RemoveAt(ri);
        ls.Destroy();
    }

    /// <summary>
    /// Remove all listeners bind to receiver.
    /// This function should be called when receiver destroy.
    /// </summary>
    /// <param name="receiver">event receiver</param>
    public static void RemoveEventListener(object receiver)
    {
        if (receiver == null) return;

        foreach (var dispatcher in m_dispatchers)
            if (dispatcher != receiver) dispatcher.RemoveEventListener(receiver);

        foreach (var lss in m_globalListeners.Values)
        {
            for (var i = 0; i < lss.Count;)
            {
                var ls = lss[i];
                var rc = !ls ? null : ls.handler != null ? ls.handler.Target : null;
                if (rc != receiver) ++i;
                else if (ls)
                {
                    var lst = m_linkedStates[ls.name];
                    for (var j = 0; j < lst.Count; ++j) lst[j].UpdateState(i, true);

                    lss.RemoveAt(i);
                    ls.Destroy();
                }
            }
        }
    }

    private static EventListener _AddEventListener(string name, Delegate handler, string extendInfo = null)
    {
        var lss = m_globalListeners.GetDefault(name);
        var ls = lss.Find(l => l.EqualsTo(handler));
        if (ls) return ls;

        ls = EventListener.Create(name, handler, null, extendInfo);
        lss.Add(ls);

        m_linkedStates.GetDefault(name);     // we always add global linkedState queue when add a new event listener
        m_removeListeners.GetDefault(name);  // and also remove queue...

        ++listenersCount;

        return ls;
    }

    private static void _RemoveEventListener(string name, Delegate handler)
    {
        var lss = m_globalListeners.Get(name);
        if (lss == null) return;

        var ri = lss.FindIndex(l => l.EqualsTo(handler));

        if (ri < 0) return;

        var lst = m_linkedStates[name];
        foreach ( var t in lst) t.UpdateState(ri, true);

        var ls = lss[ri];
        lss.RemoveAt(ri);
        ls.Destroy();

        --listenersCount;
    }
}