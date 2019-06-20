/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Vector2_ use double params
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-20
 * 
 ***************************************************************************************************/

using UnityEngine;

[System.Serializable]
public struct Vector2_
{
    #region Static functions

    public const double kEpsilon = 1E-05;

    /// <summary>
    /// Shorthand for writing Vector2_(0, 0).
    /// </summary>
    public static readonly Vector2_ zero = new Vector2_(0, 0);
    /// <summary>
    /// Shorthand for writing Vector2_(1, 1).
    /// </summary>
    public static readonly Vector2_ one = new Vector2_(1, 1);
    /// <summary>
    /// Shorthand for writing Vector2_(-1, 0).
    /// </summary>
    public static readonly Vector2_ left = new Vector2_(-1, 0);
    /// <summary>
    /// Shorthand for writing Vector2_(1, 0).
    /// </summary>
    public static readonly Vector2_ right = new Vector2_(1, 0);
    /// <summary>
    /// Shorthand for writing Vector2_(0, -1).
    /// </summary>
    public static readonly Vector2_ down = new Vector2_(0, -1);
    /// <summary>
    /// Shorthand for writing Vector2_(0, 1).
    /// </summary>
    public static readonly Vector2_ up = new Vector2_(0, 1);
    /// <summary>
    /// Shorthand for writing Vector2_(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity).
    /// </summary>
    public static readonly Vector2_ positiveInfinity = new Vector2_(double.PositiveInfinity, double.PositiveInfinity);
    /// <summary>
    /// Shorthand for writing Vector2_(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity).
    /// </summary>
    public static readonly Vector2_ negativeInfinity = new Vector2_(double.NegativeInfinity, double.NegativeInfinity);

    /// <summary>
    /// Returns the angle in degrees between from and to.
    /// </summary>
    /// <param name="from">The vector from which the angular difference is measured.</param>
    /// <param name="to">The vector to which the angular difference is measured.</param>   
    public static double Angle(Vector2_ from, Vector2_ to)
    {
        return Mathd.Acos(Mathd.Clamp(Dot(from.normalized, to.normalized), -1, 1)) * 57.29578;
    }

    public static double SignedAngle(Vector2_ from, Vector2_ to)
    {
        var nf = from.normalized;
        var nt = to.normalized;

        var num = Mathd.Acos(Mathd.Clamp(Dot(nf, nt), -1, 1)) * 57.29578;
        var num2 = Mathd.Sign(nf.x * nt.y - nf.y * nt.x);

        return num * num2;
    }

    /// <summary>
    /// Returns a copy of vector with its magnitude clamped to maxLength.
    /// </summary>
    public static Vector2_ ClampMagnitude(Vector2_ vector, double maxLength)
    {
        return vector.sqrMagnitude > maxLength * maxLength ? vector.normalized * maxLength : vector;
    }

    /// <summary>
    /// Returns the distance between a and b.
    /// </summary>
    public static double Distance(Vector2_ a, Vector2_ b)
    {
        var vector = new Vector2_(a.x - b.x, a.y - b.y);
        return Mathd.Sqrt(vector.x * vector.x + vector.y * vector.y);
    }

    /// <summary>
    /// Dot Product of two vectors.
    /// </summary>
    public static double Dot(Vector2_ lhs, Vector2_ rhs)
    {
        return lhs.x * rhs.x + lhs.y * rhs.y;
    }

    /// <summary>
    /// Linearly interpolates between two vectors.
    /// </summary>
    public static Vector2_ Lerp(Vector2_ a, Vector2_ b, double t)
    {
        t = Mathd.Clamp01(t);
        return new Vector2_(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
    }

    /// <summary>
    /// Linearly interpolates between two vectors.
    /// </summary>
    public static Vector2_ LerpUnclamped(Vector2_ a, Vector2_ b, double t)
    {
        return new Vector2_(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
    }

    /// <summary>
    /// Returns a vector that is made from the largest components of two vectors.
    /// </summary>
    public static Vector2_ Max(Vector2_ lhs, Vector2_ rhs)
    {
        return new Vector2_(Mathd.Max(lhs.x, rhs.x), Mathd.Max(lhs.y, rhs.y));
    }

    /// <summary>
    /// Returns a vector that is made from the smallest components of two vectors.
    /// </summary>
    public static Vector2_ Min(Vector2_ lhs, Vector2_ rhs)
    {
        return new Vector2_(Mathd.Min(lhs.x, rhs.x), Mathd.Min(lhs.y, rhs.y));
    }

    /// <summary>
    /// Moves a point current in a straight line towards a target point.
    /// </summary>
    public static Vector2_ MoveTowards(Vector2_ current, Vector2_ target, double maxDistanceDelta)
    {
        var a = target - current;
        var magnitude = a.magnitude;

        return (magnitude <= maxDistanceDelta || magnitude == 0) ? target : current + a / magnitude * maxDistanceDelta;
    }

    /// <summary>
    /// Projects a vector onto another vector.
    /// </summary>
    public static Vector2_ Project(Vector2_ vector, Vector2_ onNormal)
    {
        var dot = Dot(onNormal, onNormal);

        return dot < Mathd.Epsilon ? zero : onNormal * Dot(vector, onNormal) / dot;
    }

    /// <summary>
    /// Reflects a vector off the plane defined by a normal.
    /// </summary>
    public static Vector2_ Reflect(Vector2_ inDirection, Vector2_ inNormal)
    {
        return -2 * Dot(inNormal, inDirection) * inNormal + inDirection;
    }

    /// <summary>
    /// Multiplies two vectors component-wise.
    /// </summary>
    public static Vector2_ Scale(Vector2_ a, Vector2_ b)
    {
        return new Vector2_(a.x * b.x, a.y * b.y);
    }

    #region Operators

    public static Vector2_ operator +(Vector2_ a, Vector2_ b)
    {
        return new Vector2_(a.x + b.x, a.y + b.y);
    }

    public static Vector2_ operator -(Vector2_ a)
    {
        return new Vector2_(-a.x, -a.y);
    }

    public static Vector2_ operator -(Vector2_ a, Vector2_ b)
    {
        return new Vector2_(a.x - b.x, a.y - b.y);
    }

    public static Vector2_ operator *(double d, Vector2_ a)
    {
        return new Vector2_(a.x * d, a.y * d);
    }

    public static Vector2_ operator *(Vector2_ a, double d)
    {
        return new Vector2_(a.x * d, a.y * d);
    }

    public static Vector2_ operator /(Vector2_ a, double d)
    {
        return new Vector2_(a.x / d, a.y / d);
    }

    public static bool operator ==(Vector2_ lhs, Vector2_ rhs)
    {
        return (lhs - rhs).sqrMagnitude < 9.99999944E-11;
    }

    public static bool operator !=(Vector2_ lhs, Vector2_ rhs)
    {
        return !(lhs == rhs);
    }

    public static implicit operator Vector2_(Vector3_ v)
    {
        return new Vector2_(v.x, v.y);
    }

    public static implicit operator Vector3_(Vector2_ v)
    {
        return new Vector3_(v.x, v.y);
    }

    public static implicit operator Vector2(Vector2_ v)
    {
        return new Vector2((float)v.x, (float)v.y);
    }

    public static implicit operator Vector2_(Vector2 v)
    {
        return new Vector2_(v.x, v.y);
    }

    #endregion

    #endregion

    /// <summary>
    /// X component of the vector.
    /// </summary>
    public double x
    {
        get { return m_x; }
        set
        {
            if (m_x == value) return;
            m_x = value;

            _CalculateParams();
        }
    }
    /// <summary>
    /// Y component of the vector.
    /// </summary>
    public double y
    {
        get { return m_y; }
        set
        {
            if (m_y == value) return;
            m_y = value;

            _CalculateParams();
        }
    }

    [SerializeField]
    private double m_x, m_y;
    private double m_sqrMagnitude, m_magnitude, m_dx, m_dy;

    /// <summary>
    /// Creates a new vector with given x and sets y to zero.
    /// </summary>
    public Vector2_(double _x)
    {
        m_x = _x;
        m_y = 0;

        m_sqrMagnitude = m_x * m_x + m_y * m_y;
        m_magnitude    = Mathd.Sqrt(m_sqrMagnitude);

        if (m_magnitude > kEpsilon)
        {
            m_dx = m_x / m_magnitude;
            m_dy = m_y / m_magnitude;
        }
        else
            m_dx = m_dy = 0;
    }

    /// <summary>
    /// Creates a new vector with given x, y components.
    /// </summary>
    public Vector2_(double _x, double _y)
    {
        m_x = _x;
        m_y = _y;

        m_sqrMagnitude = m_x * m_x + m_y * m_y;
        m_magnitude    = Mathd.Sqrt(m_sqrMagnitude);

        if (m_magnitude > kEpsilon)
        {
            m_dx = m_x / m_magnitude;
            m_dy = m_y / m_magnitude;
        }
        else
            m_dx = m_dy = 0;
    }

    private void _CalculateParams()
    {
        m_sqrMagnitude = m_x * m_x + m_y * m_y;
        m_magnitude    = Mathd.Sqrt(m_sqrMagnitude);

        if (m_magnitude > kEpsilon)
        {
            m_dx = m_x / m_magnitude;
            m_dy = m_y / m_magnitude;
        }
        else
            m_dx = m_dy = 0;
    }

    public double this[int index]
    {
        get
        {
            if (index == 0) return m_x;
            if (index == 1) return m_y;

            return 0;
        }
        set
        {
            if (index == 0) m_x = value;
            if (index == 1) m_y = value;
        }
    }

    /// <summary>
    /// Returns the squared length of this vector (Read Only).
    /// </summary>
    public double sqrMagnitude { get { return m_sqrMagnitude; } }
    /// <summary>
    /// Returns this vector with a magnitude of 1 (Read Only).
    /// </summary>
    public Vector2_ normalized { get { return new Vector2_(m_dx, m_dy); } }
    /// <summary>
    /// Returns the length of this vector (Read Only).
    /// </summary>
    public double magnitude { get { return m_magnitude; } }

    /// <summary>
    /// Returns true if the given vector is exactly equal to this vector.
    /// </summary>
    public override bool Equals(object other)
    {

        if (!(other is Vector2_)) return false;

        var vector = (Vector2_)other;
        return m_x.Equals(vector.x) && m_y.Equals(vector.y);
    }

    public override int GetHashCode()
    {
        return m_x.GetHashCode() ^ m_y.GetHashCode() << 2;
    }

    /// <summary>
    /// Makes this vector have a magnitude of 1.
    /// </summary>
    public void Normalize()
    {
        m_x = m_dx;
        m_y = m_dy;

        _CalculateParams();
    }

    /// <summary>
    /// Multiplies every component of this vector by the same component of scale.
    /// </summary>
    public void Scale(Vector2_ scale)
    {
        m_x *= scale.x;
        m_y *= scale.y;

        _CalculateParams();
    }

    /// <summary>
    /// Set x, y components of an existing Vector2_.
    /// </summary>
    public void Set(double newX, double newY)
    {
        m_x = newX;
        m_y = newY;

        _CalculateParams();
    }

    /// <summary>
    /// Returns a nicely formatted string for this vector.
    /// </summary>
    public override string ToString()
    {
        return string.Format("({0:F1}, {1:F1})", m_x, m_y);
    }

    /// <summary>
    /// Returns a nicely formatted string for this vector.
    /// </summary>
    public string ToString(string format)
    {
        return string.Format("({0}, {1})", m_x.ToString(format), m_y.ToString(format));
    }
}