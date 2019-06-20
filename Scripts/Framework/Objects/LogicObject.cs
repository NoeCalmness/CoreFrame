/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Base logical object class.
 * In order to use ObjectManager, all game object should derive from LObject.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.2
 * Created:  2018-03-28
 * 
 ***************************************************************************************************/

using System.Collections.Generic;

public struct ObjectGUID
{
    public int    hash;
    public ushort id;
    public byte   version;

    public ObjectGUID(ushort _id, byte _version)
    {
        id      = _id;
        version = _version;
        hash    = 0;
    }

    public void Set(ushort _id)
    {
        id = _id;
    }

    public void Set(ushort _id, byte _version)
    {
        id      = _id;
        version = _version;
    }

    public static implicit operator bool(ObjectGUID guid) {  return guid.id > 0; }
    public static bool operator ==(ObjectGUID l, ObjectGUID r) { return l.hash == r.hash && l.id == r.id && l.version == r.version; }
    public static bool operator !=(ObjectGUID l, ObjectGUID r) { return !(l == r); }
    public override bool Equals(object obj) { return this == (ObjectGUID)obj; }
    public override int GetHashCode() { return base.GetHashCode(); }

    public override string ToString()
    {
        return string.Format("LObjectGuid {{ hash: {0}, id: {1}, version: {2} }}", hash, id, version);
    }

    public string ToString(bool format)
    {
        if (!format) return ToString();

        return string.Format("LObjectGuid {{{{ hash: {0}, id: {1}, version: {2} }}}}", hash, id, version);
    }
}

public abstract partial class LogicObject : EventDispatcher
{
    #region Static functions

    private static Dictionary<int, Pool<LogicObject>> m_pool = new Dictionary<int, Pool<LogicObject>>();
    private static Event_ m_globalEvent = Event_.defaultEvent;

    protected static T _Create<T>(string name = null, bool initialize = true) where T : LogicObject
    {
        var t    = typeof(T);
        var hash = t.GetHashCode();
        var p = m_pool.Get(hash);
        if (p == null) { p = new Pool<LogicObject>(256, t); m_pool.Add(hash, p); }

        var obj = (T)p.Pop();
        obj.name = name ?? string.Empty;

        if (initialize) obj.Initialize();

        ObjectManager.AddObject(obj);

        return obj;
    }

    protected static LogicObject _Create(string name = null, System.Type type = null, bool initialize = true)
    {
        var hash = type.GetHashCode();
        var p = m_pool.Get(hash);
        if (p == null) { p = new Pool<LogicObject>(256, type); m_pool.Add(hash, p); }

        var obj = p.Pop();
        obj.name = name ?? string.Empty;

        if (initialize) obj.Initialize();

        ObjectManager.AddObject(obj);

        return obj;
    }

    public static implicit operator bool(LogicObject obj)
    {
        return obj != null && !obj.destroyed && !obj.pendingDestroy;
    }

    #endregion

    public string name = string.Empty;
    public bool enableUpdate { get; set; }
    public bool updating { get; private set; }
    public ushort id { get { return m_guid.id; } }
    public byte version { get { return m_guid.version; } }
    public ObjectGUID guid { get { return m_guid; } set { m_guid = value; } }
    public int frameTime { get; protected set; }
    public ushort frameCount { get; protected set; }
    public float createTime { get; private set; }
    public double localTimeScale { get { return m_localTimeScale; } set { if (m_localTimeScale == value) return; m_localTimeScale = value; OnLocalTimeScaleChanged(); } }

    private ObjectGUID m_guid = new ObjectGUID();

    private int m_hash = 0;
    private double m_localTimeScale = 1.0;

    protected LogicObject() { m_hash = GetType().GetHashCode(); }

    private void Initialize()
    {
        destroyed         = false;
        enableUpdate      = false;
        createTime        = UnityEngine.Time.time;
        frameTime         = 0;
        frameCount        = 0;
        m_localTimeScale  = 1.0;

        m_guid.id = ObjectManager.GenerateNextObjectID();
        m_guid.version++;

        EventManager.RegisterDispatcher(this);

        OnInitialized();
    }

    protected virtual void OnInitialized() { }
    protected virtual void OnDestroy() { }
    protected virtual void OnLocalTimeScaleChanged() { }

    public virtual void EnterFrame()
    {
        updating = true;
        DispatchEvent(Events.ENTER_FRAME, m_globalEvent.inUse ? null : m_globalEvent, false);
    }

    public void Update(int diff)
    {
        frameTime += diff;
        frameCount++;
        OnUpdate(diff);
    }

    public virtual void OnRootUpdate(float diff) { }
    public virtual void OnUpdate(int diff) { }

    public virtual void QuitFrame()
    {
        DispatchEvent(Events.QUIT_FRAME, m_globalEvent.inUse ? null : m_globalEvent, false);
        updating = false;
    }

    public sealed override void Destroy()
    {
        if (destroyed) return;

        DispatchEvent(Events.ON_DESTROY);

        if (updating) { pendingDestroy = true; return; }

        OnDestroy();

        base.Destroy();

        ObjectManager.BackReusedID(this);

        name        = string.Empty;
        updating    = false;
        m_guid.id   = 0;

        m_pool.GetDefault(m_hash).Back(this);
    }
}
