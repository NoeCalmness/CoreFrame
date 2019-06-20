/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Camera shot system blend definitions
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-06-20
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;

/// <summary>Supported predefined shapes for the blend curve.</summary>
public enum CameraBlendType
{
    /// <summary>Zero-length blend</summary>
    Cut          = 0,
    /// <summary>S-shaped curve, giving a gentle and smooth transition</summary>
    EaseInOut    = 1,
    /// <summary>Linear out of the outgoing shot, and easy into the incoming</summary>
    EaseIn       = 2,
    /// <summary>Easy out of the outgoing shot, and linear into the incoming</summary>
    EaseOut      = 3,
    /// <summary>Easy out of the outgoing, and hard into the incoming</summary>
    HardIn       = 4,
    /// <summary>Hard out of the outgoing, and easy into the incoming</summary>
    HardOut      = 5,
    /// <summary>Linear blend.  Mechanical-looking.</summary>
    Linear       = 6,
    /// <summary>Custom blend</summary>
    Custom       = 7
};

[Serializable]
public struct CameraBlend
{
    private static AnimationCurve[] m_curvePresets = null;

    public static AnimationCurve GetDefaultCurve(CameraBlendType type = CameraBlendType.Cut)
    {
        if (type < 0 || type >= CameraBlendType.Custom) type = CameraBlendType.Cut;
        
        if (m_curvePresets == null)
        {
            m_curvePresets = new AnimationCurve[(int)CameraBlendType.Custom];

            var curve = new AnimationCurve();
            m_curvePresets[0] = curve;

            curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            m_curvePresets[1] = curve;

            curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            var keys = curve.keys;
            keys[1].inTangent = 0;
            curve.keys = keys;
            m_curvePresets[2] = curve;

            curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            keys = curve.keys;
            keys[0].outTangent = 0;
            curve.keys = keys;
            m_curvePresets[3] = curve;

            curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            keys = curve.keys;
            keys[0].outTangent = 0;
            keys[1].inTangent = 1.5708f; // pi/2 = up
            curve.keys = keys;
            m_curvePresets[4] = curve;

            curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            keys = curve.keys;
            keys[0].outTangent = 1.5708f; // pi/2 = up
            keys[1].inTangent = 0;
            curve.keys = keys;
            m_curvePresets[5] = curve;

            curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            m_curvePresets[6] = curve;
        }

        return m_curvePresets[(int)type];
    }

    public static readonly CameraBlend defaultBlend = new CameraBlend() { blendType = CameraBlendType.Cut };

    /// <summary>The shape of the blend curve.</summary>
    [Tooltip("Shape of the blend curve")]
    public CameraBlendType blendType;

    /// <summary>The shape of the blend curve.</summary>
    [Tooltip("Custom curve when blendTyoe set to Custom")]
    public AnimationCurve curve;

    public AnimationCurve validCurve { get { return blendType == CameraBlendType.Custom ? curve ?? GetDefaultCurve(CameraBlendType.Cut) : GetDefaultCurve(blendType); } }

    public float Evaluate(float progress)
    {
        return validCurve.Evaluate(progress);
    }

    public CameraBlend Clone()
    {
        var blend = new CameraBlend
        {
            blendType = blendType,
            curve     = curve != null ? new AnimationCurve(curve.keys) : null
        };

        return blend;
    }
}
