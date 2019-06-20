/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Tween script for alpha
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-01-20
 * 
 ***************************************************************************************************/

using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("HYLR/Tween/Tween ScrollView")]
[RequireComponent(typeof(ScrollView))]
public class TweenScrollView : TweenBase
{
    public enum TargetType { Grid = 0, Index = 1 }

    [Space(5), Header("ScrollView")]
    public int count;
    public TargetType targetType;
    public bool random = false;

    public int snapIndex = 0;

    private ScrollView m_view;
    private List<RectTransform> m_items  = new List<RectTransform>();
    private List<RectTransform> m_grids  = new List<RectTransform>();
    private List<TweenBase> m_tweens     = new List<TweenBase>();

    private float m_now = 0;
    private int m_index = -1;

    protected override Tweener OnTween()
    {
        if (targetType == TargetType.Grid && count < 1) return null;

        if (!m_view) m_view = GetComponent<ScrollView>();
        if (!m_view) return null;

        if (!m_view.poolCreated)
        {
            StartCoroutine(WaitForPoolCreated());
            return null;
        }

        if (snapIndex > -1) m_view.ScrollToIndex(snapIndex, 0);

        m_view.GetVisiblePoolItems(m_items);
        m_grids.Clear();

        if (m_items.Count < 1) return null;

        if (targetType == TargetType.Index) count = m_items.Count;
        
        foreach (var item in m_items)
        {
            if (targetType == TargetType.Index)
            {
                Prewarm(item);
                continue;
            }

            for (int i = 0, c = m_view.gridCount; i < c; ++i)
            {
                var grid = item.GetComponent<RectTransform>(i.ToString());

                if (!grid || !grid.gameObject.activeSelf) continue;

                m_grids.Add(grid);
                Prewarm(grid);

                if (m_grids.Count >= count) break;
            }
            if (m_grids.Count >= count) break;
        }

        if (random)
        {
            var list = targetType == TargetType.Grid ? m_grids : m_items;
            for (int i = 0, c = list.Count; i < c; ++i)
            {
                int idx0 = Random.Range(0, c), idx1 = Random.Range(0, c);
                var tmp = list[idx0];
                list[idx0] = list[idx1];
                list[idx1] = tmp;
            }
        }

        var f = currentAsFrom ? m_now : m_forward ? 0 : 1.0f;
        var t = m_forward ? 1.0f : 0;

        m_now = f;
        m_index = m_forward ? -1 : count;
        return DOTween.To(() => m_now, OnProgress, t, duration);
    }

    private void OnProgress(float progress)
    {
        m_now = progress;
        var index = Mathf.FloorToInt(m_now * count);
        if (index >= count) index = count - 1;

        var c = m_forward ? index - m_index : m_index - index;
        var add = m_forward ? 1 : -1;
        while (c-- > 0) PlayIndex(m_index + add);
    }

    private void Prewarm(RectTransform node)
    {
        m_tweens.Clear();
        node.GetComponents(m_tweens);

        foreach (var tween in m_tweens)
        {
            tween?.Play(m_forward);
            tween.Pause();
        }
    }

    private void PlayIndex(int index)
    {
        if (m_index == index) return;
        m_index = index;

        var list = targetType == TargetType.Grid ? m_grids : m_items;
        var idx = m_forward ? m_index : m_index - count + list.Count;
        if (idx < 0 || idx >= list.Count) return;

        var item = list[idx];

        m_tweens.Clear();
        item?.GetComponents(m_tweens);

        foreach (var tween in m_tweens) tween?.Resum();
    }

    private void OnDestroy()
    {
        m_tweens.Clear();
        m_items.Clear();
        m_grids.Clear();

        m_tweens = null;
        m_items = null;
        m_grids = null;
        m_view = null;
    }

    private IEnumerator WaitForPoolCreated()
    {
        yield return m_view && m_view.poolCreated;

        Play(m_forward);
    }
}
