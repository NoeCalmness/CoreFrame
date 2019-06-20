/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * A simple 2D collider with a Box2D and Sphere2D
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-21
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;

public class BoxSphereCollider : Collider_
{
    public Box2D box { get { return m_box; } }
    public Sphere2D sphere { get { return m_sphere; } }

    /// <summary>
    /// The Collider size
    /// </summary>
    public Vector2_ boxSize
    {
        get { return m_boxSize; }
        set
        {
            m_boxSize = value;

            m_box.SetSize(m_boxSize);
        }
    }
    /// <summary>
    /// The Collider Sphere2D offset relative to root position
    /// </summary>
    public Vector2_ sphereOffset
    {
        get { return m_sphereOffset; }
        set
        {
            m_sphereOffset = value;

            m_sphere.SetCenter((Vector2_)position + m_sphereOffset);
        }
    }
    /// <summary>
    /// The Collider Sphere2D radius
    /// </summary>
    public double radius
    {
        get { return m_radius; }
        set
        {
            m_radius = value;

            m_sphere.radius = m_radius;
        }
    }

    [SerializeField, Set("boxSize")]
    private Vector2_ m_boxSize;
    [SerializeField, Set("sphereOffset")]
    private Vector2_ m_sphereOffset;
    [SerializeField, Set("radius")]
    private double m_radius;

    private Box2D m_box;
    private Sphere2D m_sphere;

    protected override void Awake()
    {
        base.Awake();

        UpdateBoxSphere();

        lastCheckPosition = position;
    }

    protected override void OnPositionChanged()
    {
        m_box.SetCenter((Vector2_)position + offset);
        m_sphere.SetCenter((Vector2_)position + m_sphereOffset);
    }

    protected override void OnOffsetChanged()
    {
        m_box.SetCenter((Vector2_)position + offset);
    }

    public override bool CollisionTest(Collider_ other)
    {
        return other.CollisionTest(ref m_box) || other.CollisionTest(ref m_sphere);
    }

    public override bool CollisionTest(ref Box2D other)
    {
        return m_box.Intersect(other) || m_sphere.Intersect(other);
    }

    public override bool CollisionTest(ref Sphere2D other)
    {
        return m_box.Intersect(other) || m_sphere.Intersect(other);
    }

    private void UpdateBoxSphere()
    {
        m_box.Set((Vector2_)position + offset, m_boxSize);
        m_sphere.Set((Vector2_)position + m_sphereOffset, m_radius);
    }
#if UNITY_EDITOR || DEVELOPMENT_BUILD

    private void OnDrawGizmosSelected()
    {
        UpdateBoxSphere();

        Gizmos.color = Color.green;

        if (m_box.size_ != Vector2_.zero)
        {
            Gizmos.DrawLine((Vector2)m_box.topLeft,     (Vector2)m_box.topRight);
            Gizmos.DrawLine((Vector2)m_box.topRight,    (Vector2)m_box.bottomRight);
            Gizmos.DrawLine((Vector2)m_box.bottomRight, (Vector2)m_box.bottomLeft);
            Gizmos.DrawLine((Vector2)m_box.bottomLeft,  (Vector2)m_box.topLeft);
        }

        if (m_sphere.radius != 0)
        {
            var lp     = Vector3.negativeInfinity;
            var pc     = (int)(1f / 0.01 + 1f);
            var angle  = 0f;
            var offset = (Vector3)sphere.center;

            for (var i = 0; i < pc; ++i)
            {
                var x = (float)m_sphere.radius * Mathf.Cos(angle);
                var y = (float)m_sphere.radius * Mathf.Sin(angle);
                var p = offset + new Vector3(x, y, 0);

                if (lp != Vector3.negativeInfinity) Gizmos.DrawLine(lp, p);
                lp = p;

                angle += 2.0f * Mathf.PI * 0.01f;
            }
        }
    }
#endif
}
