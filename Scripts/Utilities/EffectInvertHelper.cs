/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Helper component for toon shading invert.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-03-30
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class EffectInvertHelper : MonoBehaviour
{
    public static EffectInvertHelper AddToEffect(GameObject eff)
    {
        var i = eff.GetComponentDefault<EffectInvertHelper>();
        i.Initialize();
        return i;
    }

    public List<Material> materials { get { return m_materials; } }
    private List<Material> m_materials = new List<Material>();

    private Dictionary<Renderer, Material[][]> m_renderers = null;
    private List<Renderer> m_uvRenderers = null;

    public bool inverted
    {
        get { return m_inverted; }
        set
        {
            if (m_inverted == value) return;
            m_inverted = value;

            UpdateInvert();
        }
    }
    [SerializeField, Set("inverted")]
    private bool m_inverted = false;

    public void UpdateInvert()
    {
        if (m_renderers == null) return;

        var ks = m_renderers.Keys;
        foreach (var r in m_renderers)
        {
            if (!r.Key) continue;

            var mm = r.Value[m_inverted ? 1 : 0];
            if (mm == null)
            {
                mm = Level.GetEffectMaterialFromPool(r.Value[0]);
                r.Value[1] = mm;
            }
            r.Key.materials = mm;
        }

        foreach (var r in m_uvRenderers)
        {
            if (!r) continue;
            var mm = r.materials;
            foreach (var m in mm)
            {
                if (!m) continue;
                if (m_inverted) m.EnableKeyword("_INVERTED");
                else m.DisableKeyword("_INVERTED");
            }
        }
    }

    private void Initialize()
    {
        m_materials.Clear();

        if (m_renderers == null) m_renderers = new Dictionary<Renderer, Material[][]>();
        else m_renderers.Clear();

        if (m_uvRenderers == null) m_uvRenderers = new List<Renderer>();
        else m_uvRenderers.Clear();

        var rs = GetComponentsInChildren<Renderer>(true);

        foreach (var r in rs)
        {
            var uvanim = r.GetComponent<UVAnimation>();
            if (uvanim)
            {
                m_uvRenderers.Add(r);
                continue;
            }

            var ms = r.sharedMaterials;
            m_materials.AddRange(ms);
            m_renderers.Set(r, new Material[2][] { ms, null });
        }
        m_materials.Distinct();
    }

    private void OnDestroy()
    {
        m_renderers   = null;
        m_uvRenderers = null;
        m_materials   = null;
    }
}
