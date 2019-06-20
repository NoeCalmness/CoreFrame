/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Basic pool object.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-27
 * 
 ***************************************************************************************************/

using System.Collections.Generic;

public class PoolObject<T> : IDestroyable where T : PoolObject<T>
{
    #region Static functions

    public static implicit operator bool(PoolObject<T> o)
    {
        return o != null && !o.destroyed;
    }

    private static Dictionary<int, Pool<T>> m_pool = new Dictionary<int, Pool<T>>();

    public static T Create(bool initialize = true)
    {
        var p = m_pool.GetDefault(typeof(T).GetHashCode()).Pop() as T;
        p.destroyed = false;
        p.pendingDestroy = false;
        p.version++;
        if (initialize) p.OnInitialize();
        return p;
    }

    #endregion

    public bool destroyed { get; protected set; }
    public bool pendingDestroy { get; protected set; }
    public int hash { get { return m_hash; } }
    public int version { get; private set; }

    private int m_hash = typeof(T).GetHashCode();

    protected virtual void OnDestroy() { }
    protected virtual void OnInitialize() { }

    protected PoolObject() { }

    public void Destroy()
    {
        if (destroyed) return;
        destroyed = true;

        pendingDestroy = false;

        OnDestroy();

        m_pool.GetDefault(m_hash).Back(this as T);
    }
}
