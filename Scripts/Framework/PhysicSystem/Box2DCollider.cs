/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * A simple 2D box collider
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-21
 * 
 ***************************************************************************************************/

using UnityEngine;

/// <summary>
/// Custom 2D Box collider
/// </summary>
public class Box2DCollider : Collider_
{
    public Box2D box { get { return m_box; } }

    /// <summary>
    /// The Collider size
    /// </summary>
    public Vector2_ size
    {
        get { return m_size; }
        set
        {
            m_size = value;

            m_box.SetSize(m_size);
        }
    }

    [SerializeField, Set("size")]
    private Vector2_ m_size;

    protected Box2D m_box;

    protected override void Awake()
    {
        base.Awake();

        UpdateBox();

        lastCheckPosition = position;
    }

    protected override void OnPositionChanged()
    {
        m_box.SetCenter((Vector2_)position + offset);
    }

    protected override void OnOffsetChanged()
    {
        m_box.SetCenter((Vector2_)position + offset);
    }

    public override bool CollisionTest(Collider_ other)
    {
        return other.CollisionTest(ref m_box);
    }

    public override bool CollisionTest(ref Box2D other)
    {
        return m_box.Intersect(other);
    }

    public override bool CollisionTest(ref Sphere2D other)
    {
        return m_box.Intersect(other);
    }

    protected void UpdateBox()
    {
        m_box.Set((Vector2_)position + offset, m_size);
    }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void OnDrawGizmosSelected()
    {
        UpdateBox();

        Gizmos.color = Color.green;

        if (m_box.size_ != Vector2_.zero)
        {
            Gizmos.DrawLine((Vector2)m_box.topLeft,     (Vector2)m_box.topRight);
            Gizmos.DrawLine((Vector2)m_box.topRight,    (Vector2)m_box.bottomRight);
            Gizmos.DrawLine((Vector2)m_box.bottomRight, (Vector2)m_box.bottomLeft);
            Gizmos.DrawLine((Vector2)m_box.bottomLeft,  (Vector2)m_box.topLeft);
        }
    }
#endif
}
