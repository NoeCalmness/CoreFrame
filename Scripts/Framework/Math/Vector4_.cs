/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Vector4_ use double params
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-15
 * 
 ***************************************************************************************************/

using UnityEngine;

/// <summary>
/// Custom Vector4 implementation.
/// </summary>
[System.Serializable]
public struct Vector4_
{
    #region Static function

    public const double kEpsilon = 1E-05;

    /// <summary>
    /// Shorthand for writing Vector4(0,0,0,0).
    /// </summary>
    public static readonly Vector4_ zero = new Vector4_(0, 0, 0, 0);
    /// <summary>
    /// Shorthand for writing Vector4(1,1,1,1).
    /// </summary>
    public static readonly Vector4_ one = new Vector4_(1, 1, 1, 1);
    /// <summary>
    /// Shorthand for writing Vector4(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity).
    /// </summary>
    public static readonly Vector4_ positiveInfinity = new Vector4_(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
    /// <summary>
    /// Shorthand for writing Vector4(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity).
    /// </summary>
    public static readonly Vector4_ negativeInfinity = new Vector4_(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);

    /// <summary>
    /// Returns a copy of vector with its magnitude clamped to maxLength.
    /// </summary>
    public static Vector4_ ClampMagnitude(Vector4_ vector, double maxLength)
    {
        return vector.sqrMagnitude > maxLength * maxLength ? vector.normalized * maxLength : vector;
    }

    /// <summary>
    /// Cross Product of two vectors.
    /// </summary>
    public static Vector4_ Cross(Vector4_ lhs, Vector4_ rhs)
    {
        return new Vector4_(lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.y - lhs.y * rhs.x);
    }

    /// <summary>
    /// Returns the distance between a and b.
    /// </summary>
    public static double Distance(Vector4_ a, Vector4_ b)
    {
        var vector = a - b;
        return vector.magnitude;
    }

    /// <summary>
    /// Dot Product of two vectors.
    /// </summary>
    public static double Dot(Vector4_ a, Vector4_ b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
    }

    /// <summary>
    /// Linearly interpolates between two vectors.
    /// </summary>
    public static Vector4_ Lerp(Vector4_ a, Vector4_ b, double t)
    {
        t = Mathd.Clamp01(t);
        return new Vector4_(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t, a.w + (b.w - a.w) * t);
    }

    /// <summary>
    /// Linearly interpolates between two vectors.
    /// </summary>
    public static Vector4_ LerpUnclamped(Vector4_ a, Vector4_ b, double t)
    {
        return new Vector4_(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t, a.w + (b.w - a.w) * t);
    }

    /// <summary>
    /// Returns a vector that is made from the largest components of two vectors.
    /// </summary>
    public static Vector4_ Max(Vector4_ lhs, Vector4_ rhs)
    {
        return new Vector4_(Mathd.Max(lhs.x, rhs.x), Mathd.Max(lhs.y, rhs.y), Mathd.Max(lhs.z, rhs.z), Mathd.Max(lhs.w, rhs.w));
    }

    /// <summary>
    /// Returns a vector that is made from the smallest components of two vectors.
    /// </summary>
    public static Vector4_ Min(Vector4_ lhs, Vector4_ rhs)
    {
        return new Vector4_(Mathd.Min(lhs.x, rhs.x), Mathd.Min(lhs.y, rhs.y), Mathd.Min(lhs.z, rhs.z), Mathd.Min(lhs.w, rhs.w));
    }

    /// <summary>
    /// Moves a point current in a straight line towards a target point.
    /// </summary>
    public static Vector4_ MoveTowards(Vector4_ current, Vector4_ target, double maxDistanceDelta)
    {
        var a = target - current;
        var magnitude = a.magnitude;

        return (magnitude <= maxDistanceDelta || magnitude == 0) ? target : current + a / magnitude * maxDistanceDelta;
    }

    /// <summary>
    /// Projects a vector onto another vector.
    /// </summary>
    public static Vector4_ Project(Vector4_ vector, Vector4_ onNormal)
    {
        var dot = Dot(onNormal, onNormal);

        return dot < Mathd.Epsilon ? zero : onNormal * Dot(vector, onNormal) / dot;
    }

    /// <summary>
    /// Projects a vector onto a plane defined by a normal orthogonal to the plane.
    /// </summary>
    public static Vector4_ ProjectOnPlane(Vector4_ vector, Vector4_ planeNormal)
    {
        return vector - Project(vector, planeNormal);
    }

    /// <summary>
    /// Reflects a vector off the plane defined by a normal.
    /// </summary>
    public static Vector4_ Reflect(Vector4_ inDirection, Vector4_ inNormal)
    {
        return -2 * Dot(inNormal, inDirection) * inNormal + inDirection;
    }

    /// <summary>
    /// Multiplies two vectors component-wise.
    /// </summary>
    public static Vector4_ Scale(Vector4_ a, Vector4_ b)
    {
        return new Vector4_(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
    }

    #region Operators

    public static Vector4_ operator +(Vector4_ a, Vector4_ b)
    {
        return new Vector4_(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
    }

    public static Vector4_ operator -(Vector4_ a, Vector4_ b)
    {
        return new Vector4_(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
    }

    public static Vector4_ operator -(Vector4_ a)
    {
        return new Vector4_(-a.x, -a.y, -a.z, -a.w);
    }

    public static Vector4_ operator *(Vector4_ a, double d)
    {
        return new Vector4_(a.x * d, a.y * d, a.z * d, a.w * d);
    }

    public static Vector4_ operator *(double d, Vector4_ a)
    {
        return new Vector4_(a.x * d, a.y * d, a.z * d, a.w * d);
    }

    public static Vector4_ operator /(Vector4_ a, double d)
    {
        return new Vector4_(a.x / d, a.y / d, a.z / d, a.w / d);
    }

    public static bool operator ==(Vector4_ lhs, Vector4_ rhs)
    {
        return (lhs - rhs).sqrMagnitude < 9.99999944E-11;
    }

    public static bool operator !=(Vector4_ lhs, Vector4_ rhs)
    {
        return !(lhs == rhs);
    }

    public static implicit operator Vector4_(Vector3_ v)
    {
        return new Vector4_(v.x, v.y, v.z);
    }

    public static implicit operator Vector3_(Vector4_ v)
    {
        return new Vector3_(v.x, v.y, v.z);
    }

    public static implicit operator Vector4_(Vector3 v)
    {
        return new Vector4_(v.x, v.y, v.z);
    }

    public static implicit operator Vector3(Vector4_ v)
    {
        return new Vector3_(v.x, v.y, v.z);
    }

    public static implicit operator Vector4_(Vector4 v)
    {
        return new Vector4_(v.x, v.y, v.z, v.w);
    }

    public static implicit operator Vector4(Vector4_ v)
    {
        return new Vector4((float)v.x, (float)v.y, (float)v.z, (float)v.w);
    }

    public static implicit operator Vector4_(Vector2_ v)
    {
        return new Vector4_(v.x, v.y);
    }

    public static implicit operator Vector2_(Vector4_ v)
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
    /// <summary>
    /// Z component of the vector.
    /// </summary>
    public double z
    {
        get { return m_z; }
        set
        {
            if (m_z == value) return;
            m_z = value;

            _CalculateParams();
        }
    }
    /// <summary>
    /// Z component of the vector.
    /// </summary>
    public double w
    {
        get { return m_w; }
        set
        {
            if (m_w == value) return;
            m_w = value;

            _CalculateParams();
        }
    }

    [SerializeField]
    private double m_x, m_y, m_z, m_w;
    private double m_sqrMagnitude, m_magnitude, m_dx, m_dy, m_dz, m_dw;

    /// <summary>
    /// Creates a new vector with given x, y, z, w components.
    /// </summary>
    public Vector4_(double _x, double _y, double _z, double _w)
    {
        m_x = _x;
        m_y = _y;
        m_z = _z;
        m_w = _w;

        m_sqrMagnitude = m_x * m_x + m_y * m_y + m_z * m_z + m_w * m_w;
        m_magnitude = Mathd.Sqrt(m_sqrMagnitude);

        if (m_magnitude > kEpsilon)
        {
            m_dx = m_x / m_magnitude;
            m_dy = m_y / m_magnitude;
            m_dz = m_z / m_magnitude;
            m_dw = m_w / m_magnitude;
        }
        else
            m_dx = m_dy = m_dz = m_dw = 0;
    }

    /// <summary>
    /// Creates a new vector with given x, y, z components and sets w to zero.
    /// </summary>
    public Vector4_(double _x, double _y, double _z)
    {
        m_x = _x;
        m_y = _y;
        m_z = _z;
        m_w = 0;

        m_sqrMagnitude = m_x * m_x + m_y * m_y + m_z * m_z;
        m_magnitude = Mathd.Sqrt(m_sqrMagnitude);

        if (m_magnitude > kEpsilon)
        {
            m_dx = m_x / m_magnitude;
            m_dy = m_y / m_magnitude;
            m_dz = m_z / m_magnitude;
            m_dw = 0;
        }
        else
            m_dx = m_dy = m_dz = m_dw = 0;
    }

    /// <summary>
    /// Creates a new vector with given x, y components and sets z, w to zero.
    /// </summary>
    public Vector4_(double _x, double _y)
    {
        m_x = _x;
        m_y = _y;
        m_z = 0;
        m_w = 0;

        m_sqrMagnitude = m_x * m_x + m_y * m_y;
        m_magnitude = Mathd.Sqrt(m_sqrMagnitude);

        if (m_magnitude > kEpsilon)
        {
            m_dx = m_x / m_magnitude;
            m_dy = m_y / m_magnitude;
            m_dz = 0;
            m_dw = 0;
        }
        else
            m_dx = m_dy = m_dz = m_dw = 0;
    }

    private void _CalculateParams()
    {
        m_sqrMagnitude = m_x * m_x + m_y * m_y + m_z * m_z + m_w * m_w;
        m_magnitude = Mathd.Sqrt(m_sqrMagnitude);

        if (m_magnitude > kEpsilon)
        {
            m_dx = m_x / m_magnitude;
            m_dy = m_y / m_magnitude;
            m_dz = m_z / m_magnitude;
            m_dw = m_w / m_magnitude;
        }
        else
            m_dx = m_dy = m_dz = m_dw = 0;
    }

    public double this[int index]
    {
        get
        {
            if (index == 0) return m_x;
            if (index == 1) return m_y;
            if (index == 2) return m_z;
            if (index == 3) return m_w;

            return 0;
        }
        set
        {
            if (index == 0) m_x = value;
            if (index == 1) m_y = value;
            if (index == 2) m_z = value;
            if (index == 3) m_w = value;
        }
    }

    /// <summary>
    /// Returns the squared length of this vector (Read Only).
    /// </summary>
    public double sqrMagnitude { get { return m_sqrMagnitude; } }
    /// <summary>
    /// Returns this vector with a magnitude of 1 (Read Only).
    /// </summary>
    public Vector4_ normalized { get { return new Vector4_(m_dx, m_dy, m_dz, m_dw); } }
    /// <summary>
    /// Returns the length of this vector (Read Only).
    /// </summary>
    public double magnitude { get { return m_magnitude; } }

    public void Set(double newX, double newY, double newZ, double newW)
    {
        m_x = newX;
        m_y = newY;
        m_z = newZ;
        m_w = newW;

        _CalculateParams();
    }

    public void Scale(Vector4_ scale)
    {
        m_x *= scale.x;
        m_y *= scale.y;
        m_z *= scale.z;
        m_w *= scale.w;

        _CalculateParams();
    }

    /// <summary>
    /// Makes this vector have a magnitude of 1.
    /// </summary>
    public void Normalize()
    {
        m_x = m_dx;
        m_y = m_dy;
        m_z = m_dz;
        m_w = m_dw;

        _CalculateParams();
    }

    public override int GetHashCode()
    {
        return m_x.GetHashCode() ^ m_y.GetHashCode() << 2 ^ m_z.GetHashCode() >> 2 ^ m_w.GetHashCode() >> 1;
    }

    /// <summary>
    /// Returns true if the given vector is exactly equal to this vector.
    /// </summary>
    public override bool Equals(object other)
    {

        if (!(other is Vector4_)) return false;

        var vector = (Vector4_)other;
        return m_x.Equals(vector.x) && m_y.Equals(vector.y) && m_z.Equals(vector.z) && m_w.Equals(vector.w);
    }

    /// <summary>
    /// Returns a nicely formatted string for this vector.
    /// </summary>
    public override string ToString()
    {
        return string.Format("({0:F1}, {1:F1}, {2:F1}, {3:F1})", m_x, m_y, m_z, m_w);
    }

    /// <summary>
    /// Returns a nicely formatted string for this vector.
    /// </summary>
    public string ToString(string format)
    {
        return string.Format("({0}, {1}, {2}, {3})", m_x.ToString(format), m_y.ToString(format), m_z.ToString(format), m_w.ToString(format));
    }
}