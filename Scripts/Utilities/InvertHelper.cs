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

public class InvertHelper : MonoBehaviour
{
    #region Static functions

    private static Shader normalShader   = null, normalAwakeShader    = null;
    private static Shader invertShader   = null, invertAwakeShader    = null;
    private static Shader dissolveShader = null, dissolveInvertShader = null;

    private static int m_rimColorID = Shader.PropertyToID("_RimColor");
    private static int m_rimIntensityID = Shader.PropertyToID("_RimIntensity");

    public static float[] rimIntensity = new float[] { 1, 1, 1, 1 };
    public static Color[] rimColors    = new Color[] { Color.white, Color.white, Color.white, Color.white };

    private static Dictionary<int, Material[]> m_materialPool = new Dictionary<int, Material[]>();
    private static bool m_initialized = false;

    public static void Initialize()
    {
        if (m_initialized) return;
        m_initialized = true;

        EventManager.AddEventListener(Events.SCENE_DESTROY, OnSceneDestroy);
        #if UNITY_EDITOR
        EventManager.AddEventListener("EditorReloadConfig", OnEditorReloadConfig);
        #endif

        normalShader         = Shader.Find("HYLR/Toon/Toon");
        invertShader         = Shader.Find("HYLR/Toon/ToonInverted");
        dissolveShader       = Shader.Find("HYLR/Toon/ToonDissolve");
        dissolveInvertShader = Shader.Find("HYLR/Toon/ToonInvertedDissolve");
        normalAwakeShader    = Shader.Find("HYLR/Toon/ToonAwake");
        invertAwakeShader    = Shader.Find("HYLR/Toon/ToonAwakeInverted");

        rimIntensity = new float[] { 1, CombatConfig.srimIntensityInvincible, CombatConfig.srimIntensityTough, CombatConfig.srimIntensityBoth };
        rimColors    = new Color[] { Color.white, CombatConfig.scolorInvincible, CombatConfig.scolorTough, CombatConfig.scolorBoth };
    }

    public static void UpdateRimSettings()
    {
        if (m_materialPool == null) return;
        foreach (var mm in m_materialPool.Values)
        {
            for (var i = 1; i < 4; ++i)
            {
                var mn = mm[i];
                var mi = mm[i + 4];

                mn.SetColor(m_rimColorID, rimColors[i]);
                mn.SetFloat(m_rimIntensityID, rimIntensity[i]);

                mi.SetColor(m_rimColorID, rimColors[i]);
                mi.SetFloat(m_rimIntensityID, rimIntensity[i]);
            }
        }
    }

    private static void OnSceneDestroy()
    {
        foreach (var mm in m_materialPool.Values) for (var i = 1; i < mm.Length; ++i) DestroyImmediate(mm[i]);
        m_materialPool.Clear();
    }

    private static Material GetMaterialFromPool(int id, int index)
    {
        var mats = m_materialPool.Get(id);
        return mats?[index];
    }

    private static Material[] GetMaterialsFromPool(Material[] mm, int index)
    {
        var mmm = new Material[mm.Length];
        for (var i = 0; i < mm.Length; ++i)
        {
            var m = mm[i];
            if (!m) continue;
            mmm[i] = GetMaterialFromPool(m.GetInstanceID(), index);
        }
        return mmm;
    }

    private static void AddMaterialsToPool(List<Material> orgMaterials)
    {
        foreach (var m in orgMaterials)
        {
            if (!m) continue;

            var c = m_materialPool.Get(m.GetInstanceID());
            if (c == null)
            {
                var s = m.shader.name;
                var awake = s == normalAwakeShader.name || s == invertAwakeShader.name;

                c = new Material[8];
                for (var i = 0; i < 4; ++i)
                {
                    var mn = i == 0 ? m : Instantiate(m);
                    var mi = Instantiate(m);

                    mn.shader = awake ? normalAwakeShader : normalShader;
                    mi.shader = awake ? invertAwakeShader : invertShader;

                    mn.SetKeyWord("_RIM_LIGHT", i != 0);
                    mi.SetKeyWord("_RIM_LIGHT", i != 0);

                    if (i > 0)
                    {
                        mn.SetColor(m_rimColorID, rimColors[i]);
                        mn.SetFloat(m_rimIntensityID, rimIntensity[i]);

                        mi.SetColor(m_rimColorID, rimColors[i]);
                        mi.SetFloat(m_rimIntensityID, rimIntensity[i]);
                    }

                    c[i] = mn;
                    c[i + 4] = mi;
                }

                m_materialPool.Set(m.GetInstanceID(), c);
            }
        }
    }

    #endregion

    public enum RimType { None = 0, Invincible = 1, Tough = 2, Both = 3 }
    private class RendererInfo { public Renderer renderer; public Material[][] materials; }

    public bool invert
    {
        get { return m_inverted; }
        set
        {
            if (m_inverted == value) return;
            m_inverted = value;

            UpdateShaders();
        }
    }
    public RimType rimLight
    {
        get { return m_rimLight; }
        set
        {
            if (m_rimLight == value) return;
            m_rimLight = value;

            UpdateShaders();
        }
    }

    [SerializeField, Set("invert")]
    private bool m_inverted = false;
    [SerializeField, Set("rimLight")]
    private RimType m_rimLight = RimType.None;

    private List<Material> m_orgMaterials = new List<Material>();

    private List<RendererInfo> m_renderers = null;
    private List<Renderer> m_tmpRenderers = null;

    [SerializeField]
    private bool m_usePool = true;

    public void ResetToDefault()
    {
        m_inverted = false;
        m_rimLight = RimType.None;

        Refresh();
    }

    public void Refresh(bool refreshRenderers = false)
    {
        Initialize();

        if (refreshRenderers)
        {
            if (m_renderers == null) m_renderers = new List<RendererInfo>();
            if (m_tmpRenderers == null) m_tmpRenderers = new List<Renderer>();

            GetRenderers(m_tmpRenderers);

            m_orgMaterials.Clear();
            foreach (var r in m_tmpRenderers)
            {
                var s = r?.sharedMaterial?.shader?.name;
                if (s == null ||
                    s != normalShader.name && s != invertShader.name &&
                    s != normalAwakeShader.name && s != invertAwakeShader.name &&
                    s != dissolveShader.name && s != dissolveInvertShader.name) continue;

                var ms = GetMaterials(r);
                var ri = m_renderers.Find(rr => rr.renderer == r);
                if (ri == null)
                {
                    ri = new RendererInfo() { renderer = r, materials = new Material[8][] { ms, null, null, null, null, null, null, null } };
                    m_renderers.Add(ri);
                    m_orgMaterials.AddRange(ms);
                }
                else
                {
                    var mss = ri.materials;
                    if (CheckDirty(ms, mss))
                    {
                        mss.Clear();
                        mss[0] = ms;
                        m_orgMaterials.AddRange(ms);
                    }
                    else m_orgMaterials.AddRange(mss[0]);
                }
            }

            m_tmpRenderers.Clear();

            m_orgMaterials.Distinct();

            if (m_usePool) AddMaterialsToPool(m_orgMaterials);
        }

        UpdateShaders();
    }

    public void UpdateRimLight(RimType type)
    {
        m_rimLight = type;

        UpdateShaders();
    }

    private void Start()
    {
        Refresh(true);
    }

    private void OnDestroy()
    {
        m_renderers = null;
        m_orgMaterials = null;
        m_tmpRenderers = null;
    }

    private void GetRenderers(List<Renderer> list, Transform node = null)
    {
        var n = node ?? transform;
        for (int i = 0, c= n.childCount; i < c; ++i)
        {
            var child = n.GetChild(i);
            var ei = child.GetComponent<EffectInvertHelper>();
            if (ei) continue;

            var mi = child.GetComponent<InvertHelper>();
            if (mi) continue;

            var r = child.GetComponent<Renderer>();
            if (r) list.Add(r);

            if (child.childCount > 0) GetRenderers(list, child);
        }
    }

    private bool CheckDirty(Material[] now, Material[][] check)
    {
        foreach (var c in check)
        {
            if (c == null || c.Length != now.Length) continue;
            if (c.SequenceEqual(now)) return false;
        }

        return true;
    }

    private Material[] GetMaterials(Renderer renderer)
    {
#if UNITY_EDITOR
        if (!m_usePool && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
            var mm = renderer.sharedMaterials;
            if (mm.Length < 1 || mm[0].name.EndsWith("__Instance")) return mm;

            var ms = new Material[mm.Length];
            for (var i = 0; i < mm.Length; ++i)
            {
                var m = mm[i];
                var ii = -1;
                for (var j = 0; j < i; ++j) if (mm[j] == m) ii = j;

                var c = ii < 0 ? m ? Instantiate(m) : null : ms[ii];
                if (c) c.name = m.name + "__Instance";

                ms[i] = c;
            }
            renderer.materials = ms;
            return ms;
        }
#endif

        return m_usePool ? renderer.sharedMaterials : renderer.materials;
    }

    private void UpdateShaders()
    {
        if (m_renderers == null) return;

        var rim = (int)m_rimLight;
        if (m_usePool)
        {
            var idx = rim + (m_inverted ? 4 : 0);
            foreach (var r in m_renderers)
            {
                var mm = r.materials[idx];
                if (mm == null)
                {
                    mm = GetMaterialsFromPool(r.materials[0], idx);
                    r.materials[idx] = mm;
                }
                r.renderer.materials = mm;
            }
            return;
        }

        foreach (var mat in m_orgMaterials)
        {
            if (!mat) continue;

            var s = mat.shader;
            var sn = s.name;
            var awake = sn == normalAwakeShader.name || sn == invertAwakeShader.name;
            var dissolve = sn == dissolveShader.name || sn == dissolveInvertShader.name;
            var ss = awake ? m_inverted ? invertAwakeShader : normalAwakeShader : dissolve ? m_inverted ? dissolveInvertShader : dissolveShader : m_inverted ? invertShader : normalShader;
            if (s != ss) mat.shader = ss;

            mat.SetKeyWord("_RIM_LIGHT", rim > 0);
            if (rim > 0)
            {
                mat.SetColor(m_rimColorID, rimColors[rim]);
                mat.SetFloat(m_rimIntensityID, rimIntensity[rim]);
            }
        }
    }
    
    #region Editor helper

#if UNITY_EDITOR
    public void Refresh_()
    {
        m_usePool = false;
        Refresh(true);
    }

    private static void OnEditorReloadConfig(Event_ e)
    {
        var config = (string)e.param1;
        if (config == "config_combatconfigs")
        {
            rimIntensity = new float[] { 1, CombatConfig.srimIntensityInvincible, CombatConfig.srimIntensityTough, CombatConfig.srimIntensityBoth };
            rimColors = new Color[] { Color.white, CombatConfig.scolorInvincible, CombatConfig.scolorTough, CombatConfig.scolorBoth };

            UpdateRimSettings();
        }
    }
#endif

    #endregion
}
