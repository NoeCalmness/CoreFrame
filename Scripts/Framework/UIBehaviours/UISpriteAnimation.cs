/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Launcher class.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-05
 * 
 ***************************************************************************************************/

using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("HYLR/UI/Sprite Animation")]
public class UISpriteAnimation : MonoBehaviour
{
    public Sprite[] textures;

    private Image m_image;

    private int m_index;

    public bool loop;

    public int start = 0;
    public int end = 0;

    public int fps = 30;

    private float m_time;
    private float m_frameTime = 1 / 30.0f;

    public void Restart()
    {
        if (textures == null || textures.Length < 1) return;

        m_index = start;
        m_time = 0;

        m_image.sprite = textures[m_index];

        m_image.enabled = true;
        enabled = true;
    }

	private void Awake()
	{
        m_time = 0;

        if (textures == null || textures.Length < 1)
        {
            enabled = false;
            return;
        }

        if (start < 0 || start >= textures.Length) start = 0;
        if (end <= 0 || end >= textures.Length) end = textures.Length - 1;
        if (start > end)
        {
            var t = start;
            start = end;
            end = t;
        }

        var reg = new Regex(@"\d+");
        foreach (var t in textures)
        {
            var m = reg.Match(t.name);
            if (!m.Success) continue;
            t.name = m.Groups[0].Value;
        }

        Array.Sort(textures, (a, b) => int.Parse(a.name) < int.Parse(b.name) ? -1 : 1);

        m_image = this.GetComponentDefault<Image>();

        m_image.sprite = textures[start];
        m_index = start;

        if (fps < 1) fps = 30;

        m_frameTime = 1.0f / fps;
	}
	
	private void Update()
	{
        if (m_index >= end && !loop)
        {
            if (m_index == end) m_index = end + 1;
            else
            {
                m_index = end;

                m_image.enabled = false;
                enabled = false;
                return;
            }
        }

        m_time += Time.deltaTime;

        var idx = m_index;
        while (m_time >= m_frameTime)
        {
            m_time -= m_frameTime;
            if (++idx > end) idx = start;
            if (idx == end && !loop) break;
        }

        if (idx != m_index)
        {
            m_index = idx;
            m_image.sprite = textures[m_index];
        }
	}
}
