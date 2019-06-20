/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Vector3_ use double params
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-06
 * 
 ***************************************************************************************************/

using UnityEngine;

/// <summary>
/// Custom Vector3 implementation.
/// </summary>
[System.Serializable]
public struct Vector3_
{
    #region Static functions

    public const double kEpsilon = 1E-05;

    /// <summary>
    /// Shorthand for writing Vector3_(0, 0, 0).
    /// </summary>
    public static readonly Vector3_ zero = new Vector3_(0, 0, 0);
    /// <summary>
    /// Shorthand for writing Vector3_(1, 1, 1).
    /// </summary>
    public static readonly Vector3_ one = new Vector3_(1, 1, 1);
    /// <summary>
    /// Shorthand for writing Vector3_(0, 0, 1).
    /// </summary>
    public static readonly Vector3_ forward = new Vector3_(0, 0, 1);
    /// <summary>
    /// Shorthand for writing Vector3_(0, 0, -1).
    /// </summary>
    public static readonly Vector3_ back = new Vector3_(0, 0, -1);
    /// <summary>
    /// Shorthand for writing Vector3_(-1, 0, 0).
    /// </summary>
    public static readonly Vector3_ left = new Vector3_(-1, 0, 0);
    /// <summary>
    /// Shorthand for writing Vector3_(0, -1, 0).
    /// </summary>
    public static readonly Vector3_ down = new Vector3_(0, -1, 0);
    /// <summary>
    /// Shorthand for writing Vector3_(1, 0, 0).
    /// </summary>
    public static readonly Vector3_ right = new Vector3_(1, 0, 0);
    /// <summary>
    /// Shorthand for writing Vector3_(0, 1, 0).
    /// </summary>
    public static readonly Vector3_ up = new Vector3_(0, 1, 0);
    /// <summary>
    /// Shorthand for writing Vector3_(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity).
    /// </summary>
    public static readonly Vector3_ positiveInfinity = new Vector3_(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
    /// <summary>
    /// Shorthand for writing Vector3_(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity).
    /// </summary>
    public static readonly Vector3_ negativeInfinity = new Vector3_(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);

    /// <summary>
    /// Returns the angle in degrees between from and to.
    /// </summary>
    /// <param name="from">The vector from which the angular difference is measured.</param>
    /// <param name="to">The vector to which the angular difference is measured.</param>   
    public static double Angle(Vector3_ from, Vector3_ to)
    {
        return Mathd.Acos(Mathd.Clamp(Dot(from.normalized, to.normalized), -1, 1)) * 57.29578;
    }

    /// <summary>
    /// Returns a copy of vector with its magnitude clamped to maxLength.
    /// </summary>
    public static Vector3_ ClampMagnitude(Vector3_ vector, double maxLength)
    {
        return vector.sqrMagnitude > maxLength * maxLength ? vector.normalized * maxLength : vector;
    }

    /// <summary>
    /// Cross Product of two vectors.
    /// </summary>
    public static Vector3_ Cross(Vector3_ lhs, Vector3_ rhs)
    {
        return new Vector3_(lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.y - lhs.y * rhs.x);
    }

    /// <summary>
    /// Returns the distance between a and b.
    /// </summary>
    public static double Distance(Vector3_ a, Vector3_ b)
    {
        var vector = new Vector3_(a.x - b.x, a.y - b.y, a.z - b.z);
        return Mathd.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
    }

    /// <summary>
    /// Dot Product of two vectors.
    /// </summary>
    public static double Dot(Vector3_ lhs, Vector3_ rhs)
    {
        return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
    }

    /// <summary>
    /// Linearly interpolates between two vectors.
    /// </summary>
    public static Vector3_ Lerp(Vector3_ a, Vector3_ b, double t)
    {
        t = Mathd.Clamp01(t);
        return new Vector3_(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
    }

    /// <summary>
    /// Linearly interpolates between two vectors.
    /// </summary>
    public static Vector3_ LerpUnclamped(Vector3_ a, Vector3_ b, double t)
    {
        return new Vector3_(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
    }

    /// <summary>
    /// Returns a vector that is made from the largest components of two vectors.
    /// </summary>
    public static Vector3_ Max(Vector3_ lhs, Vector3_ rhs)
    {
        return new Vector3_(Mathd.Max(lhs.x, rhs.x), Mathd.Max(lhs.y, rhs.y), Mathd.Max(lhs.z, rhs.z));
    }

    /// <summary>
    /// Returns a vector that is made from the smallest components of two vectors.
    /// </summary>
    public static Vector3_ Min(Vector3_ lhs, Vector3_ rhs)
    {
        return new Vector3_(Mathd.Min(lhs.x, rhs.x), Mathd.Min(lhs.y, rhs.y), Mathd.Min(lhs.z, rhs.z));
    }

    /// <summary>
    /// Moves a point current in a straight line towards a target point.
    /// </summary>
    public static Vector3_ MoveTowards(Vector3_ current, Vector3_ target, double maxDistanceDelta)
    {
        var a = target - current;
        var magnitude = a.magnitude;

        return (magnitude <= maxDistanceDelta || magnitude == 0) ? target : current + a / magnitude * maxDistanceDelta;
    }

    /// <summary>
    /// Projects a vector onto another vector.
    /// </summary>
    public static Vector3_ Project(Vector3_ vector, Vector3_ onNormal)
    {
        var dot = Dot(onNormal, onNormal);

        return dot < Mathd.Epsilon ? zero : onNormal * Dot(vector, onNormal) / dot;
    }

    /// <summary>
    /// Projects a vector onto a plane defined by a normal orthogonal to the plane.
    /// </summary>
    public static Vector3_ ProjectOnPlane(Vector3_ vector, Vector3_ planeNormal)
    {
        return vector - Project(vector, planeNormal);
    }

    /// <summary>
    /// Reflects a vector off the plane defined by a normal.
    /// </summary>
    public static Vector3_ Reflect(Vector3_ inDirection, Vector3_ inNormal)
    {
        return -2 * Dot(inNormal, inDirection) * inNormal + inDirection;
    }

    /// <summary>
    /// Multiplies two vectors component-wise.
    /// </summary>
    public static Vector3_ Scale(Vector3_ a, Vector3_ b)
    {
        return new Vector3_(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    #region Operators

    public static Vector3_ operator +(Vector3_ a, Vector3_ b)
    {
        return new Vector3_(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static Vector3_ operator -(Vector3_ a)
    {
        return new Vector3_(-a.x, -a.y, -a.z);
    }

    public static Vector3_ operator -(Vector3_ a, Vector3_ b)
    {
        return new Vector3_(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static Vector3_ operator *(double d, Vector3_ a)
    {
        return new Vector3_(a.x * d, a.y * d, a.z * d);
    }

    public static Vector3_ operator *(Vector3_ a, double d)
    {
        return new Vector3_(a.x * d, a.y * d, a.z * d);
    }

    public static Vector3_ operator /(Vector3_ a, double d)
    {
        return new Vector3_(a.x / d, a.y / d, a.z / d);
    }

    public static bool operator ==(Vector3_ lhs, Vector3_ rhs)
    {
        return (lhs - rhs).sqrMagnitude < 9.99999944E-11;
    }

    public static bool operator !=(Vector3_ lhs, Vector3_ rhs)
    {
        return !(lhs == rhs);
    }

    public static implicit operator Vector3(Vector3_ v)
    {
        return new Vector3((float)v.x, (float)v.y, (float)v.z);
    }

    public static implicit operator Vector3_(Vector3 v)
    {
        return new Vector3_(v.x, v.y, v.z);
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

    [SerializeField]
    private double m_x, m_y, m_z;
    private double m_sqrMagnitude, m_magnitude, m_dx, m_dy, m_dz;

    /// <summary>
    /// Creates a new vector with given x, y components and sets z to zero.
    /// </summary>
    public Vector3_(double _x, double _y)
    {
        m_x = _x;
        m_y = _y;
        m_z = 0;

        m_sqrMagnitude = m_x * m_x + m_y * m_y + m_z * m_z;
        m_magnitude    = Mathd.Sqrt(m_sqrMagnitude);

        if (m_magnitude > kEpsilon)
        {
            m_dx = m_x / m_magnitude;
            m_dy = m_y / m_magnitude;
            m_dz = m_z / m_magnitude;
        }
        else
            m_dx = m_dy = m_dz = 0;
    }

    /// <summary>
    /// Creates a new vector with given x, y, z components.
    /// </summary>
    public Vector3_(double _x, double _y, double _z)
    {
        m_x = _x;
        m_y = _y;
        m_z = _z;

        m_sqrMagnitude = m_x * m_x + m_y * m_y + m_z * m_z;
        m_magnitude    = Mathd.Sqrt(m_sqrMagnitude);

        if (m_magnitude > kEpsilon)
        {
            m_dx = m_x / m_magnitude;
            m_dy = m_y / m_magnitude;
            m_dz = m_z / m_magnitude;
        }
        else
            m_dx = m_dy = m_dz = 0;
    }

    private void _CalculateParams()
    {
        m_sqrMagnitude = m_x * m_x + m_y * m_y + m_z * m_z;
        m_magnitude    = Mathd.Sqrt(m_sqrMagnitude);

        if (m_magnitude > kEpsilon)
        {
            m_dx = m_x / m_magnitude;
            m_dy = m_y / m_magnitude;
            m_dz = m_z / m_magnitude;
        }
        else
            m_dx = m_dy = m_dz = 0;
    }

    public double this[int index]
    {
        get
        {
            if (index == 0) return m_x;
            if (index == 1) return m_y;
            if (index == 2) return m_z;

            return 0;
        }
        set
        {
            if (index == 0) m_x = value;
            if (index == 1) m_y = value;
            if (index == 2) m_z = value;
        }
    }

    /// <summary>
    /// Returns the squared length of this vector (Read Only).
    /// </summary>
    public double sqrMagnitude { get { return m_sqrMagnitude; } }
    /// <summary>
    /// Returns this vector with a magnitude of 1 (Read Only).
    /// </summary>
    public Vector3_ normalized { get { return new Vector3_(m_dx, m_dy, m_dz); } }
    /// <summary>
    /// Returns the length of this vector (Read Only).
    /// </summary>
    public double magnitude { get { return m_magnitude; } }

    /// <summary>
    /// Returns true if the given vector is exactly equal to this vector.
    /// </summary>
    public override bool Equals(object other)
    {

        if (!(other is Vector3_)) return false;

        var vector = (Vector3_)other;
        return m_x.Equals(vector.x) && m_y.Equals(vector.y) && m_z.Equals(vector.z);
    }

    public override int GetHashCode()
    {
        return m_x.GetHashCode() ^ m_y.GetHashCode() << 2 ^ m_z.GetHashCode() >> 2;
    }

    /// <summary>
    /// Makes this vector have a magnitude of 1.
    /// </summary>
    public void Normalize()
    {
        m_x = m_dx;
        m_y = m_dy;
        m_z = m_dz;

        _CalculateParams();
    }

    /// <summary>
    /// Multiplies every component of this vector by the same component of scale.
    /// </summary>
    public void Scale(Vector3_ scale)
    {
        m_x *= scale.x;
        m_y *= scale.y;
        m_z *= scale.z;

        _CalculateParams();
    }

    /// <summary>
    /// Set x, y and z components of an existing Vector3_.
    /// </summary>
    public void Set(double newX, double newY, double newZ)
    {
        m_x = newX;
        m_y = newY;
        m_z = newZ;

        _CalculateParams();
    }

    /// <summary>
    /// Returns a nicely formatted string for this vector.
    /// </summary>
    public override string ToString()
    {
        return string.Format("({0:F1}, {1:F1}, {2:F1})", m_x, m_y, m_z);
    }

    /// <summary>
    /// Returns a nicely formatted string for this vector.
    /// </summary>
    public string ToString(string format)
    {
        return string.Format("({0}, {1}, {2})", m_x.ToString(format), m_y.ToString(format), m_z.ToString(format));
    }
}