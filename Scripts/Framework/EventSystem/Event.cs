/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Basic event class contains simple event arguments for an event.
 * For complex event you should derive Event class to implement custom event.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-02-28
 * 
 ***************************************************************************************************/
using System.Collections.Generic;

public class Event_
{
    #region Static functions

    private static Dictionary<int, Pool<Event_>>  m_eventPool   = new Dictionary<int, Pool<Event_>>();
    private static Pool<Event_>                   m_defaultPool = new Pool<Event_>(40);

    public static readonly System.Type defaultType = typeof(Event_);

    public static Event_ defaultEvent { get { return Pop(); } }

    public static void Back(Event_ e)
    {
        e.Reset();

        var t = e.GetType();
        if (t == defaultType) { m_defaultPool.Back(e); return; }

        m_eventPool.GetDefault(t.GetHashCode()).Back(e);
    }

    public static Event_ Pop(object _param1 = null, object _param2 = null, object _param3 = null, object _param4 = null)
    {
        var e = m_defaultPool.Pop();
        e.SetParams(_param1, _param2, _param3, _param4);
        return e;
    }

    public static T Pop<T>() where T : Event_
    {
        var t = typeof(T);
        if (t == defaultType) return (T)Pop();

        var hash = t.GetHashCode();
        var p = m_eventPool.Get(hash);
        if (p == null) { p = new Pool<Event_>(20, typeof(T)); m_eventPool.Add(hash, p); }

        return (T)p.Pop();
    }

    public static Event_ Pop(System.Type t)
    {
        if (!t.IsSubclassOf(defaultType)) return defaultEvent;

        if (t == defaultType) return Pop();

        var hash = t.GetHashCode();
        var p = m_eventPool.Get(hash);
        if (p == null) { p = new Pool<Event_>(20, t); m_eventPool.Add(hash, p); }

        return p.Pop();
    }

    #endregion

    public IEventDispatcher sender;
    public string           name;
    public bool             cancle;
    public bool             inUse;

    public object param1;
    public object param2;
    public object param3;
    public object param4;

    protected Event_() { }

    public virtual void Reset()
    {
        cancle = false;
        inUse  = false;
        name   = null;
        sender = null;

        SetParams();
    }

    public void SetInfo(string _name, IEventDispatcher _sender, bool _cancle, bool _inUse)
    {
        name   = _name;
        sender = _sender;
        cancle = _cancle;
        inUse  = _inUse;
    }

    public void SetParams(object _param1 = null, object _param2 = null, object _param3 = null, object _param4 = null)
    {
        param1 = _param1;
        param2 = _param2;
        param3 = _param3;
        param4 = _param4;
    }
}
