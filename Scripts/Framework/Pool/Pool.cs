/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * A simple, basic and threadunsafe object pool class.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-02-28
 * 
 ***************************************************************************************************/

using System;
using System.Reflection;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Class)]
public class PoolSizeAttribute : Attribute { public int maxSize = 256; public PoolSizeAttribute(int _maxSize = 256) { maxSize = _maxSize; } }

public class Pool<T> where T : class
{
    public int maxSize
    {
        get { return m_maxSize; }
        set
        {
            if (value < 1) Logger.LogError($"Pool maxSize must > 0, class:<b>{typeof(T)}</b>");
            else m_maxSize = value;
        }
    }

    public int size { get { return m_size; } }

    private int      m_maxSize = 256;
    private int      m_size    = 0;
    private Stack<T> m_pool    = null;

    private List<FieldInfo> m_fields = null;
    private ConstructorInfo m_constructor = null;

    public Pool()
    {
        var attr = typeof(T).GetCustomAttribute<PoolSizeAttribute>();

        if (attr != null)
        {
            m_maxSize = attr.maxSize;
            Logger.LogDetail($"Pool <b>[{typeof(T)}]</b> size limit set to <b>[{m_maxSize}]</b> from attribute.");
        }

        if (m_maxSize < 1) m_maxSize = 1;

        m_pool = new Stack<T>(m_maxSize);

        var type = typeof(T);
        m_constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);

        if (m_constructor == null)
            Logger.LogException($"Pool object class <b>[{typeof(T)}]</b> has no default constructor!");
    }

    public Pool(int maxSize, Type type = null)
    {
        if (maxSize > 0) m_maxSize = maxSize;

        m_pool = new Stack<T>(m_maxSize);

        if (type == null) type = typeof(T);

        m_constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public  | BindingFlags.NonPublic, null, new Type[] { }, null);

        if (m_constructor == null)
            Logger.LogException($"Pool object class <b>[{typeof(T)}]</b> has no default constructor!");
    }

    public T Pop()
    {
        if (m_pool.Count > 0)
        {
            --m_size;
            return m_pool.Pop();
        }

        if (m_constructor != null)
        {
            var o = (T)m_constructor.Invoke(null);
            if (m_fields == null)
            {
                m_fields = new List<FieldInfo>();
                var fs = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var f in fs)
                {
                    if (f.GetValue(o) != null) continue;
                    m_fields.Add(f);
                }
            }
            return o;
        }

        return null;
    }

    public bool Back(T obj)
    {
        if (m_fields != null && obj != null) foreach (var field in m_fields) field.SetValue(obj, null);

        if (m_pool.Count >= m_maxSize)
        {
            //Logger.LogWarning("oops....pool is full! maxSize:{0}", m_maxSize);
            return false;
        }

        m_pool.Push(obj);
        ++m_size;

        return true;
    }

    public void Clear()
    {
        m_pool.Clear();
        m_size = 0;
    }
}
