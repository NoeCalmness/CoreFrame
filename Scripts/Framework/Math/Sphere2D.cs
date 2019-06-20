/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * A 2D sphere
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-
 * 
 ***************************************************************************************************/

using UnityEngine;

[System.Serializable]
public struct Sphere2D
{
    #region Static functions

    public static bool operator ==(Sphere2D lhs, Sphere2D rhs)
    {
        return lhs.x == rhs.x && lhs.y == rhs.y && lhs.radius == rhs.radius;
    }

    public static bool operator !=(Sphere2D lhs, Sphere2D rhs)
    {
        return !(lhs == rhs);
    }

    #endregion

    public Vector2 center { get { return new Vector2_(m_x, m_y); } }
    public Vector2_ center_ { get { return new Vector2_(m_x, m_y); } }

    /// <summary>
    /// X position of the Sphere
    /// </summary>
    public double x
    {
        get { return m_x; }
        set
        {
            if (m_x == value) return;
            m_x = value;
        }
    }
    /// <summary>
    /// Y position of the sphere
    /// </summary>
    public double y
    {
        get { return m_y; }
        set
        {
            if (m_y == value) return;
            m_y = value;
        }
    }
    /// <summary>
    /// Radius of the sphere
    /// </summary>
    public double radius
    {
        get { return m_radius; }
        set
        {
            if (m_radius == value) return;
            m_radius = value;
        }
    }

    [SerializeField]
    private double m_x, m_y, m_radius;

    public Sphere2D(Vector2_ _center, double _radius)
    {
        m_x      = _center.x;
        m_y      = _center.y;
        m_radius = _radius;
    }

    public Sphere2D(double _x, double _y, double _radius)
    {
        m_x      = _x;
        m_y      = _y;
        m_radius = _radius;
    }

    public void SetCenter(double _x, double _y)
    {
        m_x = _x;
        m_y = _y;
    }

    public void SetCenter(Vector2_ _center)
    {
        m_x = _center.x;
        m_y = _center.y;
    }

    public void Set(double _x, double _y, double _radius)
    {
        SetCenter(_x, _y);

        m_radius = _radius;
    }

    public void Set(Vector2_ _center, double _radius)
    {
        SetCenter(_center);

        m_radius = _radius;
    }

    public bool Intersect(Box2D other)
    {
        if (m_radius == 0 || other.size_ == Vector2_.zero) return false;

        var dx = Mathd.Abs(m_x - other.x);
        var dy = Mathd.Abs(m_y - other.y);

        if (dx > other.width * 0.5 + m_radius || dy > other.height * 0.5 + m_radius) return false;

        if (dx <= other.width * 0.5 || dy <= other.height * 0.5) return true;

        var hdx = (dx - other.width * 0.5);
        var hdy = (dy - other.height * 0.5);
        var dsq = hdx * hdx + hdy * hdy;

        return dsq <= m_radius * m_radius;
    }

    public bool Intersect(Sphere2D other)
    {
        if (m_radius == 0 || other.radius == 0) return false;

        var d = center_ - other.center_;
        var r = radius + other.radius;
        return d.sqrMagnitude < r * r;
    }

    public bool Contains(Vector2_ point)
    {
        return (point - center_).magnitude <= radius;
    }

    /// <summary>
    /// Returns true if the given vector is exactly equal to this vector.
    /// </summary>
    public override bool Equals(object other)
    {

        if (!(other is Sphere2D)) return false;

        var sphere = (Sphere2D)other;
        return m_x.Equals(sphere.x) && m_y.Equals(sphere.y) && m_radius.Equals(sphere.radius);
    }

    public override int GetHashCode()
    {
        return m_x.GetHashCode() ^ m_y.GetHashCode() << 2 ^ m_radius.GetHashCode() >> 2;
    }

    /// <summary>
    /// Returns a nicely formatted string for this vector.
    /// </summary>
    public override string ToString()
    {
        return string.Format("({0:F1}, {1:F1}, {2:F1})", m_x, m_y, m_radius);
    }

    /// <summary>
    /// Returns a nicely formatted string for this vector.
    /// </summary>
    public string ToString(string format)
    {
        return string.Format("({0}, {1}, {2})", m_x.ToString(format), m_y.ToString(format), m_radius.ToString(format));
    }
}
