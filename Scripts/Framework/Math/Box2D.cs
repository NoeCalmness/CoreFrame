/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * A 2D Box
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-21
 * 
 ***************************************************************************************************/

using UnityEngine;

[System.Serializable]
public struct Box2D
{
    #region Static functions

    public static bool operator ==(Box2D lhs, Box2D rhs)
    {
        return lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width && lhs.height == rhs.height;
    }

    public static bool operator !=(Box2D lhs, Box2D rhs)
    {
        return !(lhs == rhs);
    }

    #endregion

    public Vector2 center       { get { return new Vector2_(m_x, m_y); } }
    public Vector2 size         { get { return new Vector2_(m_width, m_height); } }
    public Vector2_ center_     { get { return new Vector2_(m_x, m_y); } }
    public Vector2_ size_       { get { return new Vector2_(m_width, m_height); } }
    public Vector2_ topLeft     { get { return new Vector2_(m_x - m_width * 0.5, m_y + m_height * 0.5); } }
    public Vector2_ topRight    { get { return new Vector2_(m_x + m_width * 0.5, m_y + m_height * 0.5); } }
    public Vector2_ bottomLeft  { get { return new Vector2_(m_x - m_width * 0.5, m_y - m_height * 0.5); } }
    public Vector2_ bottomRight { get { return new Vector2_(m_x + m_width * 0.5, m_y - m_height * 0.5); } }

    public double leftEdge    { get { return m_x - m_width * 0.5; } }
    public double rightEdge   { get { return m_x + m_width * 0.5; } }
    public double topEdge     { get { return m_y + m_height * 0.5; } }
    public double bottomEdge  { get { return m_y - m_height * 0.5; } }

    public double x
    {
        get { return m_x; }
        set
        {
            if (m_x == value) return;
            m_x = value;
        }
    }
    public double y
    {
        get { return m_y; }
        set
        {
            if (m_y == value) return;
            m_y = value;
        }
    }
    public double width
    {
        get { return m_width; }
        set
        {
            if (m_width == value) return;
            m_width = value;
        }
    }
    public double height
    {
        get { return m_height; }
        set
        {
            if (m_height == value) return;
            m_height = value;
        }
    }

    [SerializeField]
    private double m_x, m_y, m_width, m_height;

    public Box2D(Vector2_ _center, Vector2_ _size)
    {
        m_x      = _center.x;
        m_y      = _center.y;
        m_width  = _size.x;
        m_height = _size.y;
    }

    public Box2D(Vector2_ _center, double _width, double _height)
    {
        m_x      = _center.x;
        m_y      = _center.y;
        m_width  = _width;
        m_height = _height;
    }

    public Box2D(double _x, double _y, double _width, double _height)
    {
        m_x      = _x;
        m_y      = _y;
        m_width  = _width;
        m_height = _height;
    }

    public void SetCenter(double _x, double _y)
    {
        m_x = _x;
        m_y = _y;
    }

    public void SetSize(double _widht, double _height)
    {
        m_width = _widht;
        m_height = _height;
    }

    public void SetCenter(Vector2_ _center)
    {
        m_x = _center.x;
        m_y = _center.y;
    }

    public void SetSize(Vector2_ _size)
    {
        m_width = _size.x;
        m_height = _size.y;
    }

    public void Set(double _x, double _y, double _width, double _height)
    {
        SetCenter(_x, _y);
        SetSize(_width, _height);
    }

    public void Set(Vector2_ _center, double _width, double _height)
    {
        SetCenter(_center);
        SetSize(_width, _height);
    }

    public void Set(Vector2_ _center, Vector2_ _size)
    {
        SetCenter(_center);
        SetSize(_size);
    }

    public bool Intersect(Box2D other)
    {
        if (m_width == 0 && m_height == 0 || other.size_ == Vector2_.zero) return false;

        return rightEdge > other.leftEdge && leftEdge < other.rightEdge && topEdge > other.bottomEdge && bottomEdge < other.topEdge;
    }

    public bool Intersect(Sphere2D other)
    {
        if (m_width == 0 && m_height == 0 || other.radius == 0) return false;

        return other.Intersect(this);
    }

    public bool Contains(Box2D other)
    {
        return leftEdge < other.leftEdge && rightEdge > other.rightEdge && topEdge > other.topEdge && bottomEdge < other.bottomEdge;
    }

    public bool ContainsBy(Box2D other)
    {
        return other.leftEdge < leftEdge && other.rightEdge > rightEdge && other.topEdge > topEdge && other.bottomEdge < bottomEdge;
    }

    public bool ContainsOrContained(Box2D other)
    {
        return Contains(other) || ContainsBy(other);
    }

    /// <summary>
    /// Returns true if the given vector is exactly equal to this vector.
    /// </summary>
    public override bool Equals(object other)
    {

        if (!(other is Box2D)) return false;

        var box = (Box2D)other;
        return m_x.Equals(box.x) && m_y.Equals(box.y) && m_width.Equals(box.width) && m_height.Equals(box.height);
    }

    public override int GetHashCode()
    {
        return m_x.GetHashCode() ^ m_y.GetHashCode() << 2 ^ m_width.GetHashCode() >> 2 ^ m_height.GetHashCode() >> 1;
    }

    /// <summary>
    /// Returns a nicely formatted string for this vector.
    /// </summary>
    public override string ToString()
    {
        return string.Format("({0:F1}, {1:F1}, {2:F1}, {3:F1})", m_x, m_y, m_width, m_height);
    }

    /// <summary>
    /// Returns a nicely formatted string for this vector.
    /// </summary>
    public string ToString(string format)
    {
        return string.Format("({0}, {1}, {2}, {3})", m_x.ToString(format), m_y.ToString(format), m_width.ToString(format), m_height.ToString(format));
    }
}