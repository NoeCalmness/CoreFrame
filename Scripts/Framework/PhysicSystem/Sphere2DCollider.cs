/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * A simple 2D sphere collider
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-21
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;

public class Sphere2DCollider : Collider_
{
    public Sphere2D sphere { get { return m_sphere; } }

    /// <summary>
    /// The Collider radius
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

    [SerializeField, Set("radius")]
    private double m_radius;

    private Sphere2D m_sphere;

    protected override void Awake()
    {
        base.Awake();

        UpdatSphere();

        lastCheckPosition = position;
    }

    protected override void OnPositionChanged()
    {
        m_sphere.SetCenter((Vector2_)position + offset);
    }

    protected override void OnOffsetChanged()
    {
        m_sphere.SetCenter((Vector2_)position + offset);
    }

    public override bool CollisionTest(Collider_ other)
    {
        return other.CollisionTest(ref m_sphere);
    }

    public override bool CollisionTest(ref Box2D other)
    {
        return m_sphere.Intersect(other);
    }

    public override bool CollisionTest(ref Sphere2D other)
    {
        return m_sphere.Intersect(other);
    }

    private void UpdatSphere()
    {
        m_sphere.Set((Vector2_)position + offset, m_radius);
    }
#if UNITY_EDITOR || DEVELOPMENT_BUILD

    private void OnDrawGizmosSelected()
    {
        UpdatSphere();

        Gizmos.color = Color.green;

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
