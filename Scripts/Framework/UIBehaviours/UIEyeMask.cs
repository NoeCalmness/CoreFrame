/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Custom exp bar image
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-01-18
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("HYLR/UI/Eye Mask")]
public class UIEyeMask : Image
{
    private static readonly int _Rage = Shader.PropertyToID("_Rage");
    
    public float maskRage
    {
        get { return m_maskRage; }
        set
        {
            if (m_maskRage == value) return;
            m_maskRage = Mathf.Clamp01(value);

            UpdateMask();
        }
    }

    [SerializeField, _Range(0, 1.0f, "maskRage")]
    private float m_maskRage = 0;

    private void UpdateMask()
    {
        material.SetFloat(_Rage, m_maskRage);
    }
}
