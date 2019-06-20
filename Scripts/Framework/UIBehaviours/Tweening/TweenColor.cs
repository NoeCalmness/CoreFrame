/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Tween script for color
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-01-20
 * 
 ***************************************************************************************************/

using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

[AddComponentMenu("HYLR/Tween/Tween Color")]
public class TweenColor : TweenBase
{
    [Space(5), Header("Color")]
    public Color from;
    public Color to;
    [Space(5), Header("RichText")]
    public bool richText = false;
    private Graphic m_graphic;
    private Material mat = null;
    protected override Tweener OnTween()
    {
        if (!m_graphic) m_graphic = GetComponent<Graphic>();
        if (!m_graphic) return null;

        var f = currentAsFrom ? m_graphic.color : m_forward ? from : to;
        var t = m_forward ? to : from;

        m_graphic.color = f;
        if(richText)
        {
            if(mat == null)
            {
                mat = new Material(m_graphic.material);
                mat.hideFlags = HideFlags.HideAndDontSave;
                m_graphic.material = mat; 
            }
            m_graphic.materialForRendering.color = f;
            return m_graphic.materialForRendering.DOColor(t, duration);
        }
        return m_graphic.DOColor(t, duration);
    }

    protected override void OnReset()
    {
        base.OnReset();

        if (m_graphic) m_graphic.color = from;
    }
}
