/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Behaviour script of SceneObject
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-03
 * 
 ***************************************************************************************************/

using UnityEngine;

public class SceneObjectBehaviour : MonoBehaviour
{
    public SceneObject sceneObject
    {
        get { return m_sceneObject; }
        set
        {
            m_sceneObject = value;
            m_renderObject = m_sceneObject as IRenderObject;

            enabled = m_renderObject != null;
        }
    }

    /// <summary>Time.smoothDeltaTime * sceneObject.localTimeScale</summary>
    public float delta { get { return m_delta; } }
    /// <summary>Time.smoothDeltaTime </summary>
    public float globalDelta { get { return m_globalDelta; } }
    /// <summary>Time.unscaledDeltaTime</summary>
    public float unscaledDelta { get { return m_unscaledDelta; } }

    protected SceneObject m_sceneObject = null;
    protected IRenderObject m_renderObject = null;

    /// <summary>Time.smoothDeltaTime * sceneObject.localTimeScale</summary>
    [SerializeField] protected float m_delta = 0;  
    /// <summary>Time.smoothDeltaTime </summary>
    [SerializeField] protected float m_globalDelta = 0;
    /// <summary>Time.unscaledDeltaTime</summary>
    [SerializeField] protected float m_unscaledDelta = 0; 

    protected virtual void OnEnable()
    {
        if (m_sceneObject) m_sceneObject.OnEnable();
    }

    protected virtual void OnDisable()
    {
        if (m_sceneObject) m_sceneObject.OnDisable();
    }

    protected virtual void OnDestroy()
    {     
        if (Root.instance && m_sceneObject) m_sceneObject.Destroy();

        enabled = false;
        m_sceneObject = null;
        m_renderObject = null;
    }

    private void Update()
    {
        #if UNITY_EDITOR
        if (Root.logicPaused) return;
        #endif

        m_unscaledDelta = Util.GetMillisecondsTime(Time.unscaledDeltaTime);
        m_globalDelta   = Util.GetMillisecondsTime(Time.smoothDeltaTime);
        m_delta         = (float)(Util.GetMillisecondsTime(Time.smoothDeltaTime) * sceneObject.localTimeScale);

        m_renderObject.OnRenderUpdate();

        OnUpdate();
    }

    private void LateUpdate()
    {
        #if UNITY_EDITOR
        if (Root.logicPaused) return;
        #endif

        m_renderObject.OnPostRenderUpdate();

        OnLateUpdate();
    }

    protected virtual void OnUpdate() { }
    protected virtual void OnLateUpdate() { }
}
