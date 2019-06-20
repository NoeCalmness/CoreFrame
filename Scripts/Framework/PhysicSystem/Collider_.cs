/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Base class for all custom colliders
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-21
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The base class of all custom collider
/// </summary>
public abstract class Collider_ : MonoBehaviour
{
    protected static PhysicsManager m_physics = PhysicsManager.instance;

    /// <summary>
    /// Is this collider ignore same group colliders ?
    /// </summary>
    public bool ignoreSameGroup = true;
    /// <summary>
    /// Is this collider a trigger ?
    /// </summary>
    public bool isTrigger = false;
    public bool isActive  = true;
    /// <summary>
    /// Collider layer
    /// </summary>
    public int layer
    {
        get { return m_layer; }
        set
        {
            if (value < 0 || value > PhysicsManager.MAX_LAYER_COUNT)
            {
                Logger.LogError("Layer must in [0 - 31]...");
                return;
            }

            m_layer = value;
        }
    }
    /// <summary>
    /// Target collision layer mask
    /// </summary>
    public int targetMask { get { return m_targetMask; } set { m_targetMask = value; } }
    /// <summary>
    /// Auto sync transform position from collider position ?
    /// </summary>
    public bool syncPosition
    {
        get { return m_syncPosition; }
        set
        {
            m_syncPosition = value;

            if (m_syncPosition) transform.position = m_position;
        }
    }
    /// <summary>
    /// The Collider root position
    /// </summary>
    public Vector3_ position
    {
        get { return m_position; }
        set
        {
            m_position = value;
            m_center   = (Vector2_)m_position + m_offset;

            if (syncPosition) transform.position = position;

            OnPositionChanged();
        }
    }
    /// <summary>
    /// The Collider offset relative to root position
    /// </summary>
    public Vector2_ offset
    {
        get { return m_offset; }
        set
        {
            m_offset = value;
            m_center = (Vector2_)m_position + m_offset;

            OnOffsetChanged();
        }
    }
    /// <summary>
    /// The collider center
    /// </summary>
    public Vector2_ center => m_center;
    /// <summary>
    /// Collider use snapshot ?
    /// </summary>
    public bool enableSnapshot { get { return m_enableSnapshot; } set { m_enableSnapshot = value; } }

    public Vector3_ lastCheckPosition { get; set; }

    [SerializeField, Set("layer")]
    private int m_layer;
    [SerializeField, Set("targetMask")]
    private int m_targetMask;
    [SerializeField, Set("syncPosition")]
    private bool m_syncPosition = true;
    [SerializeField, Set("position")]
    private Vector3_ m_position;
    [SerializeField, Set("offset")]
    private Vector2_ m_offset;
    [SerializeField]
    private Vector2_ m_center;
    [SerializeField, Set("enableSnapshot")]
    protected bool m_enableSnapshot = false;

    private int m_touchIdx = 0;
    private int m_touchCount = 0;
    private List<Collider_> m_touched = new List<Collider_>();

    protected virtual void Awake()
    {
        isActive = true;

        if (!m_physics) m_physics = PhysicsManager.instance;
        if (m_physics) m_physics.Register(this);
    }

    protected virtual void OnDestroy()
    {
        m_touched.Clear();
        m_touched = null;
    }

    /// <summary>
    /// Add a layer to target collision detection mask
    /// </summary>
    /// <param name="layer"></param>
    public void AddLayerToTarget(int layer)
    {
        if (layer < 0 || layer > PhysicsManager.MAX_LAYER_COUNT) return;

        targetMask |= layer.ToMask();
    }

    /// <summary>
    /// Remove a layer from target collision detection mask
    /// </summary>
    /// <param name="layer"></param>
    public void RemoveLayerFromTarget(int layer)
    {
        if (layer < 0 || layer > PhysicsManager.MAX_LAYER_COUNT) return;

        targetMask ^= targetMask & ( 1 << layer);
    }

    /// <summary>
    /// Event callback when begin collision with other collider
    /// </summary>
    protected virtual void OnCollisionBegin(Collider_ other) { }
    /// <summary>
    /// Event callback when end collision with other collider
    /// </summary>
    protected virtual void OnCollisionEnd(Collider_ _other) { }
    /// <summary>
    /// Event callback per frame when stay with other collider
    /// </summary>
    protected virtual void OnCollisionTouch(Collider_ _other) { }

    protected virtual void OnPositionChanged() { }
    protected virtual void OnOffsetChanged() { }

    /// <summary>
    /// Event callback when check collision with other Collider
    /// </summary>
    /// <param name="other"></param>
    public abstract bool CollisionTest(Collider_ other);

    /// <summary>
    /// Extended collision snapshot for custom usage
    /// Only active when collider marked isTrigger
    /// </summary>
    /// <returns>true = continue false = break</returns>
    public virtual bool UpdateSnapshot(int idx) { return idx == 0; }

    public virtual void OnQuitFrame() { }

    public void BeginCollision(Collider_ other)
    {
        if (m_touched.Contains(other)) return;

        m_touched.Add(other);
        OnCollisionBegin(other);
    }

    public void EndCollision(Collider_ other)
    {
        var idx = m_touched.IndexOf(other);
        if (idx < 0) return;

        m_touched.RemoveAt(idx);
        if (idx <= m_touchIdx)
        {
            --m_touchIdx;
            --m_touchCount;
        }

        OnCollisionEnd(other);
    }

    public void TouchUpdate(int diff)
    {
        m_touchCount = m_touched.Count;
        for (m_touchIdx = 0; m_touchIdx < m_touchCount;)
        {
            var touched = m_touched[m_touchIdx];
            if (!touched)
            {
                m_touched.RemoveAt(m_touchIdx);
                --m_touchCount;
            }
            else
            {
                ++m_touchIdx;
                OnCollisionTouch(touched);
            }
        }
    }

    public bool Touched(Collider_ other)
    {
        return m_touched.Contains(other);
    }

    public void ClearTouchedColliders()
    {
        m_touchCount = m_touched.Count;
        for (m_touchIdx = 0; m_touchIdx < m_touchCount; ++m_touchIdx)
        {
            var touched = m_touched[m_touchIdx];

            EndCollision(touched);
            touched.EndCollision(this);
        }

        m_touched.Clear();
        m_touchIdx   = 0;
        m_touchCount = 0;
    }

    public abstract bool CollisionTest(ref Box2D other);
    public abstract bool CollisionTest(ref Sphere2D other);
}
