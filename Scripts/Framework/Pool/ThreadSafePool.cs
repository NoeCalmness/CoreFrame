/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * A simple, basic and threadsafe object pool class.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-02-28
 * 
 ***************************************************************************************************/

using System;
using System.Reflection;
using System.Collections.Generic;

public class ThreadSafePool<T> where T : class
{
    public int maxSize
    {
        get { return m_maxSize; }
        set
        {
            if (value < 1) Logger.LogError("maxSize could not < 1");
            else m_maxSize = value;
        }
    }

    public int size { get { return m_size; } }

    private int      m_maxSize = 256;
    private int      m_size    = 0;
    private Stack<T> m_pool    = null;
    private object   m_guard   = null;

    private ConstructorInfo m_constructor = null;

    public ThreadSafePool()
    {
        m_pool = new Stack<T>();
        m_guard = new object();

        var type = typeof(T);
        m_constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);

        if (m_constructor == null)
            Logger.LogException("Pool object class has no default constructor!");
    }

    public ThreadSafePool(int maxSize, Type type = null)
    {
        if (maxSize > 0) m_maxSize = maxSize;

        m_pool = new Stack<T>();
        m_guard = new object();

        if (type == null) type = typeof(T);

        m_constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public  | BindingFlags.NonPublic, null, new Type[] { }, null);

        if (m_constructor == null)
            Logger.LogException("Pool object class has no default constructor!");
    }

    public T Pop()
    {
        lock (m_guard)
        {
            if (m_pool.Count > 0)
            {
                --m_size;
                return m_pool.Pop();
            }
        }

        if (m_constructor != null)
            return (T)m_constructor.Invoke(null);

        return null;
    }

    public bool Back(T obj)
    {
        lock (m_guard)
        {
            if (m_pool.Count >= m_maxSize)
            {
                //Logger.LogWarning("oops....pool is full! maxSize:{0}", m_maxSize);
                return false;
            }

            m_pool.Push(obj);
            ++m_size;
        }

        return true;
    }

    public void Clear()
    {
        lock (m_guard)
        {
            m_pool.Clear();
            m_size = 0;
        }
    }
}
