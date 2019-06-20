/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Manage all logical objects at runtime, this is a singleton class, and the root object in scene.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-02-27
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnRootUpdate(float diff);
public delegate void OnLogicUpdate(int diff);

public class ObjectManager : SingletonBehaviour<ObjectManager>
{
    #region Static functions

    public static void RootUpdate(float diff) { instance._RootUpdate(diff); }

    public static void LogicUpdate(int diff)
    {
        //启用客户端逻辑更新后，不再执行服务器的逻辑的逻辑更新,同时更新会导致动作加速
        if (enableUpdate)
            return;
        instance._LogicUpdate((int)(diff * m_timeScale), diff);
    }

    public static LogicObject FindObject(string name) { return instance._FindObject(name); }
    public static LogicObject FindObject(Predicate<LogicObject> match) { return instance._FindObject(match); }
    public static List<LogicObject> FindObjects(Predicate<LogicObject> match) { return instance._FindObjects(match); }
    public static T FindObject<T>(string name) where T : LogicObject { return instance._FindObject<T>(name); }
    public static T FindObject<T>(Predicate<T> match) where T : LogicObject { return instance._FindObject(match); }
    public static List<T> FindObjects<T>(Predicate<T> match) where T : LogicObject { return instance._FindObjects(match); }
    public static List<T> GetAll<T>(List<T> list = null) where T : LogicObject { return instance._GetAll(list); }
    public static void Foreach<T>(Predicate<T> call) where T : LogicObject { instance._Foreach(call); }
    public static bool AddObject(LogicObject obj) { return instance._AddObject(obj); }
    public static ushort GenerateNextObjectID() { return instance._GenerateNextObjectID(); }
    /// <summary>
    /// Internal function. Important: do not call this function manually!
    /// </summary>
    public static void BackReusedID(LogicObject obj)
    {
        if (!instance) return;
        if (instance.m_reuseIDs.Count < 1) instance.m_reuseIDs.Enqueue(0);
        instance.m_reuseIDs.Enqueue(obj.id);
    }

    public static bool updating { get { return instance.m_logicUpdating; } }
    public static int currentObjectID { get { return instance.m_currentObjectID; } }
    public static int frameCount { get { return instance.m_frameCount; } }

    public static OnRootUpdate onRootUpdate;
    public static OnLogicUpdate onLogicUpdate;

    /// <summary>
    /// enable or disable root object manager's update list.
    /// when update disabled, all objects in update list will not receive update event,
    /// but all root event such as ENTER_FRAME and QUIT_FRAME will be dispatched normally.
    /// </summary>
    public static bool enableUpdate
    {
        get { return instance.m_enableUpdate; }
        set
        {
            if (instance.m_enableUpdate == value) return;
            instance.m_enableUpdate = value;
        }
    }
    /// <summary>
    /// Global time scale
    /// </summary>
    public static double timeScale
    {
        get { return m_timeScale; }
        set
        {
            if (value < 0.001) value = 0.001;
            m_timeScale = value;
            Time.timeScale = (float)(m_timeScale <= 0.001 ? 0 : m_timeScale);
        }
    }
    private static _Double m_timeScale = 1.0;

    /// <summary>
    /// Logic delta time (Unscaled)
    /// </summary>
    public static int deltaTime { get; private set; } = 0;
    /// <summary>
    /// Global scaled delta time (Ignore LogicObject.localTimeScale)
    /// </summary>
    public static int globalDeltaTime { get; private set; } = 0;

    #endregion

    private List<LogicObject>   m_objects    = new List<LogicObject>();
    private Queue<ushort>       m_reuseIDs   = new Queue<ushort>();

    private Event_ m_globalEvent = Event_.defaultEvent;

    private bool   m_logicUpdating   = false;
    private bool   m_rootUpdating    = false;
    private bool   m_enableUpdate    = true;
    private ushort m_currentObjectID = 0;
    private int    m_frameCount      = 0;
    private int    m_rootIdx         = 0;
    private int    m_rootSize        = 0;

    public LogicObject _FindObject(string name)
    {
        return m_objects.Find(l => l.name == name);
    }

    public LogicObject _FindObject(Predicate<LogicObject> match)
    {
        return m_objects.Find(match);
    }

    public List<LogicObject> _FindObjects(Predicate<LogicObject> match)
    {
        return m_objects.FindAll(match);
    }

    public T _FindObject<T>(string name) where T : LogicObject
    {
        return (T)m_objects.Find(l => l.name == name && l as T);
    }

    public T _FindObject<T>(Predicate<T> match) where T : LogicObject
    {
        return (T)m_objects.Find(l =>
        {
            var o = l as T;
            return o && match(o);
        });
    }

    public List<T> _FindObjects<T>(Predicate<T> match) where T : LogicObject
    {
        var l = new List<T>();

        foreach (var o in m_objects)
        {
            var _o = o as T;
            if (!_o || !match(_o)) continue;
            l.Add(_o);
        }
        return l;
    }

    public List<T> _GetAll<T>(List<T> list = null) where T : LogicObject
    {
        if (list == null) list = new List<T>();

        foreach (var o in m_objects)
        {
            var _o = o as T;
            if (_o) list.Add(_o);
        }
        return list;
    }

    public void _Foreach<T>(Predicate<T> call) where T : LogicObject
    {
        foreach (var obj in m_objects)
        {
            if (!obj) continue;
            var t = obj as T;
            if (!t) continue;
            if (!call(t)) return;
        }
    }

    /// <summary>
    /// Add object to update list.
    /// Object will add to update list immediately, but will receive update event from next frame.
    /// If obj already in update list, it will be moved to the end of update list.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public bool _AddObject(LogicObject obj)
    {
        if (!obj) return false;

        m_objects.Remove(obj);
        m_objects.Add(obj);

        return true;
    }

    public ushort _GenerateNextObjectID()
    {
        ushort id = 0;
        if (m_reuseIDs.Count > 0) id = m_reuseIDs.Dequeue();
        if (id == 0) id = ++m_currentObjectID;
        return id;
    }

    public void _RootUpdate(float diff)
    {
        if (m_rootUpdating) return;
        m_rootUpdating = true;

        Level.realTime += (int)(diff * 1000);

        m_rootSize = m_objects.Count;

        for (m_rootIdx = 0; m_rootIdx < m_rootSize; ++m_rootIdx)
        {
            var o = m_objects[m_rootIdx];
            if (!o.destroyed)
            {
                try { o.OnRootUpdate(diff * (float)o.localTimeScale); }
                #if DEVELOPMENT_BUILD || UNITY_EDITOR
                catch (Exception e) { Logger.LogException(e); }
                #else
                catch { }
                #endif
            }
        }

        m_rootIdx = 0;

        onRootUpdate?.Invoke(diff);

        m_rootUpdating = false;
    }

    public void _LogicUpdate(int diff, int unscaledDiff)
    {
        if (m_logicUpdating) return;
        m_logicUpdating = true;

        deltaTime = unscaledDiff;
        globalDeltaTime = diff;

        Level.levelTime += unscaledDiff;
        ++m_frameCount;

        DispatchEvent(Events.ROOT_ENTER_FRAME, m_globalEvent, false);

        PhysicsManager.Update(diff);
        DelayEvents.Update(diff);

        var size = m_objects.Count;

        for (var i = 0; i < size;)
        {
            var o = m_objects[i];

            try
            {
                if (!o.destroyed && o.enableUpdate)
                {
                    o.EnterFrame();
                    o.Update((int)(diff * o.localTimeScale));
                    o.QuitFrame();
                }
                if (o.pendingDestroy) o.Destroy();
            }
            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            catch (Exception e) { Logger.LogException(e); }
            #else
            catch { }
            #endif
            if (o.destroyed)
            {
                m_objects.RemoveAt(i);
                size--;

                m_rootSize--;
                if (i <= m_rootIdx)
                    --m_rootIdx;
            }
            else ++i;
        }

        DispatchEvent(Events.ROOT_QUIT_FRAME, m_globalEvent, false);

        onLogicUpdate?.Invoke(diff);

        m_logicUpdating = false;
    }

    #region Monobehaviour

    override protected void Awake()
    {
        base.Awake();

        transform.name = "_objectManager";
    }

    private void Update()
    {
        _RootUpdate(Time.unscaledDeltaTime);

        if (!m_enableUpdate) return;
        _LogicUpdate(Util.GetMillisecondsTimeMS(Time.smoothDeltaTime), Util.GetMillisecondsTimeMS(Time.unscaledDeltaTime));
    }

    override protected void OnDestroy()
    {
        // We do not need call LogicObject.Destroy at this point, because the game is closed
        // for (var i = 0; i < m_objects.Count; ++i) m_objects[i].Destroy();

        // We need to destroy socket connection in editor mode...
#if UNITY_EDITOR
        Session.instance?.Destroy();
        Module_PVP.instance?.Destroy();
        Module_Chat.instance?.Destroy();
        Module_Team.instance?.Destroy();
#endif

        base.OnDestroy();

        m_objects.Clear();
        m_reuseIDs.Clear();

        m_logicUpdating   = false;
        m_rootUpdating    = false;
        m_currentObjectID = 0;
        m_frameCount      = 0;

        onLogicUpdate = null;
    }

    #endregion

    #region Editor helper

#if UNITY_EDITOR
    public bool   e_EnableUpdate     = true;
    public bool   e_Dispatching      = false;
    public bool   e_Updating         = false;
    public int    e_ObjectCount      = 0;
    public int    e_CurrentObjectID  = 0;
    public int    e_ListenersCount   = 0;
    public int    e_GListenersCount  = 0;
    public int    e_FrameCount       = 0;
    public double e_TimeScale        = 1.0;
    public int    e_MaxTypeList      = 100;

    public List<string> E_ObjectTypes = new List<string>();

    private int e_TypeListRefreshRate = 50;

    private double e_TimeScale_old = 1.0;
    private bool  e_EnableUpdate_old = true;

    private void LateUpdate()
    {
        e_Updating        = updating;
        e_Dispatching     = dispatching;
        e_ObjectCount     = m_objects.Count;
        e_CurrentObjectID = m_currentObjectID;
        e_ListenersCount  = listenersCount;
        e_GListenersCount = EventManager.listenersCount;
        e_FrameCount      = m_frameCount;

        if (e_TimeScale != e_TimeScale_old) timeScale = e_TimeScale;
        else e_TimeScale = m_timeScale;
        e_TimeScale_old = e_TimeScale;

        if (e_EnableUpdate != e_EnableUpdate_old) enableUpdate = e_EnableUpdate;
        else e_EnableUpdate = enableUpdate;
        e_EnableUpdate_old = e_EnableUpdate;

        if (++e_TypeListRefreshRate > 50)
        {
            e_TypeListRefreshRate = 0;

            E_ObjectTypes.Clear();
            var max = e_MaxTypeList < 1 || e_MaxTypeList >= m_objects.Count ? m_objects.Count : e_MaxTypeList;
            for (var i = 0; i < max; ++i)
                E_ObjectTypes.Add(Util.Format("{0}[{1}]", m_objects[i].name, m_objects[i].GetType().Name));
        }
    }
#endif

    #endregion
}
