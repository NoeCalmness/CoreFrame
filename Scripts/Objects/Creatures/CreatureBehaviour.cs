/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Base creature behaviours
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-04-25
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class CreatureBehaviour : MonoBehaviour
{
    private static readonly Material[] m_guardMaterials = new Material[] { };
    private static readonly int[] m_tmpIndices = new int[] { 0, 1, 2, 1, 0, 3 };
    private static readonly Vector3[] m_tmpVertices = new Vector3[4];

    public Creature creature
    {
        get { return m_creature; }
        set
        {
            if (m_creature == value) return;
            m_creature = value;

            m_attackCollider.creature = m_creature;
            m_hitCollider.creature = m_creature;
            m_collider.creature = m_creature;
            m_effects.creature = m_creature;

            UpdateColliderPositions();
            CreateGuardRenderer();
        }
    }

    [SerializeField]
    private WeaponInfo.Weapon m_weaponInfo;
    [SerializeField]
    private WeaponInfo.Weapon m_offWeaponInfo;

    public CreatureAttackCollider attackCollider { get { return m_attackCollider; } }
    public CreatureHitCollider hitCollider { get { return m_hitCollider; } }
    public CreatureCollider collider_ { get { return m_collider; } }
    public CreatureEffects effects { get { return m_effects; } }

    private Creature m_creature;

    [SerializeField]
    private CreatureAttackCollider m_attackCollider;
    [SerializeField]
    private CreatureHitCollider m_hitCollider;
    [SerializeField]
    private CreatureCollider m_collider;
    [SerializeField]
    private CreatureEffects m_effects;
    [SerializeField]
    private Transform m_weapon;

    private List<GameObject> m_weapons = new List<GameObject>();
    private Dictionary<int, GameObject> m_mainWeapons = new Dictionary<int, GameObject>(), m_offWeapons = new Dictionary<int, GameObject>();
    private Dictionary<int, List<EffectInvertHelper>> m_weaponEffects = new Dictionary<int, List<EffectInvertHelper>>();

    [SerializeField]
    private float m_knockbackDistance = 0;

    [SerializeField]
    private bool m_shake = false;
    [SerializeField]
    private int m_shakeIdx = 0;
    [SerializeField]
    private float m_lastShake = 0;
    [SerializeField]
    [Tooltip("Current state motion time")]
    private float m_motionTime = 0;
    [SerializeField]
    [Tooltip("Current falling time")]
    private float m_fallTime   = 0;

    private bool m_awaked = false;

    private Mesh m_renderGuardMesh = null;

    public void ResetTimer()
    {
        m_motionTime = 0;
        m_fallTime   = 0;
    }

    public void CopyStateFrom(CreatureBehaviour other)
    {
        if (!other || this == other) return;

        m_fallTime = other.m_fallTime;
        m_motionTime = other.m_motionTime;
        m_lastShake = other.m_lastShake;
        m_shakeIdx = other.m_shakeIdx;
        m_shake = other.m_shake;

        m_knockbackDistance = other.m_knockbackDistance;
    }

    public void Shake(bool shake)
    {
        m_shake = shake;

        creature.model.localPosition = Vector3.zero;

        m_shakeIdx  = 2 * CombatConfig.shitShakeFrames - 1;
        m_lastShake = m_shake ? CombatConfig.shitShakeDistance : 0;
    }

    public void Knockback(double d)
    {
        m_knockbackDistance = (float)d;
    }

    public void UpdateColliderPositions()
    {
        m_attackCollider.position = m_creature.position_;
        m_hitCollider.position = m_creature.position_;
        m_collider.position = m_creature.position_;
    }

    public void UpdateColliderViewPositions()
    {
        m_attackCollider.transform.position = m_creature.position_;
        m_hitCollider.transform.position = m_creature.position_;
        m_collider.transform.position = m_creature.position_;
    }

    public void UpdateAllColliderState(bool _enabled)
    {
        m_attackCollider.enabled = _enabled;
        m_hitCollider.enabled = _enabled;
        m_collider.enabled = _enabled;

        if (_enabled) UpdateColliderPositions();
    }

    public void UpdateAwakeState()
    {
        if (m_awaked) return;
        m_awaked = true;

        if (!m_attackCollider.gameObject.activeSelf) { m_attackCollider.gameObject.SetActive(true); m_attackCollider.gameObject.SetActive(false); }
        if (!m_hitCollider.gameObject.activeSelf)    { m_hitCollider.gameObject.SetActive(true);    m_hitCollider.gameObject.SetActive(false); }
        if (!m_collider.gameObject.activeSelf)       { m_collider.gameObject.SetActive(true);       m_collider.gameObject.SetActive(false); }
    }

    public bool UpdateWeaponAnimator()
    {
        var animatorName = Creature.GetAnimatorName(m_creature.weaponID, m_creature.gender, !m_creature.isCombat);

        var ani = Level.GetPreloadObject<Object>(animatorName, false) as RuntimeAnimatorController;
        if (!ani)
        {
            var msg = "Could not load weapon animator [" + animatorName + "]";
            Logger.LogError("ChangeWeapon: {0}", msg);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayDialog("Change weapon", msg, "OK");
#endif
            return false;
        }

        var animator = m_creature.animator;
        animator.runtimeAnimatorController = ani;
        if (gameObject.activeInHierarchy)
        {
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.Update(0);
        }
        animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        return true;
    }

    /// <summary>
    /// Update current weapon model
    /// </summary>
    /// <param name="type">0 = Main weapon  1 = Off weapon  other = All</param>
    public bool UpdateWeaponModel(int type)
    {
        var success = true;
        if (type == 0 || type != 1) success &= UpdateWeaponModel(false);
        if (type == 1 || type != 0) success &= UpdateWeaponModel(true);

        SetWeaponLayer();
        if (m_creature)
        {
            m_creature.UpdateInverter(true);
            UpdateWeaponEffectInvert();
        }

        return success;
    }

    private bool UpdateWeaponModel(bool off)
    {
        var weaponID = off ? m_creature.offWeaponID : m_creature.weaponID;
        var itemId   = off ? m_creature.offWeaponItemID : m_creature.weaponItemID;

        var weapon = WeaponInfo.GetWeapon(weaponID, itemId);

        if (!off && weapon.isEmpty && weaponID < Creature.MAX_WEAPON_ID) // Only main weapon need error log
        {
            Logger.LogError("Could not find weapon config [{0}:{1}]", weaponID, itemId);
            return false;
        }

        if (off) m_offWeaponInfo = weapon;
        else m_weaponInfo = weapon;

        var ws = off ? m_offWeapons : m_mainWeapons;

        foreach (var w in ws)
        {
            w.Value?.transform.SetParent(m_weapon, false);
            m_weapons.Remove(w.Value);
        }
        ws.Clear();

        Util.DisableAllChildren(m_weapon);

        var sws = weapon.singleWeapons;
        if (sws.Length < 1) return true;

        var visible = !m_creature.currentState || !off || m_creature.currentState.showOffWeapon;

        foreach (var sw in sws)
        {
            if (string.IsNullOrEmpty(sw.model))
            {
                Logger.LogError("Invalid weapon config [weapon:{0}, itemID:{1}, index:{2}, name:{3}]", weapon.weaponID, weapon.weaponItemId, sw.index, weapon.name);
                continue;
            }

            var t = m_weapon.Find(sw.model);
            var w = t?.gameObject;

            if (!w)
            {
                w = Level.GetPreloadObject(sw.model);
                if (!w) Logger.LogError("Could not load weapon model [{0}]", sw.model);
                else
                {
                    w.name = sw.model;
                    UpdateWeaponEffects(w, weapon, sw);
                }
            }

            if (w)
            {
                w.SetActive(visible);
                Util.AddChild(m_weapon, w.transform);

                ws.Set(sw.index, w);
                m_weapons.Add(w);
            }
        }

        if (off)
        {
            var sm = ConfigManager.Get<StateMachineInfoOff>(1);
            if (sm) SetWeaponBind(sm.defaultWeaponBinds, true);
        }
        else if (m_creature.currentState)
            SetWeaponBind(m_creature.currentState.bindInfo);

        Logger.LogInfo("{2}eapon changed to [{0}: {3}:{1}]", weapon.weaponID, weapon.weaponItemId, off ? "Off W" : "W", weapon.name);

        return true;
    }

    private void UpdateWeaponEffects(GameObject w, WeaponInfo.Weapon wi, WeaponInfo.SingleWeapon sw)
    {
        var effs = m_weaponEffects.GetDefault(w.GetInstanceID());
        var es = Util.ParseString<string>(sw.effects);
        foreach (var e in es)
        {
            var eff = Level.GetPreloadObjectFromPool(e);
            if (!eff) Logger.LogError("Could not load weapon effect [{0}] in weapon [{1}:{2}]", e, wi.weaponID, sw.index);
            else
            {
                var i = eff.GetComponent<EffectInvertHelper>();
                if (!i)
                {
                    Logger.LogError("Invalid weapon effect [{0}] in weapon [{1}:{2}]", e, wi.weaponID, sw.index);
                    Level.BackEffect(eff);
                    continue;
                }

                eff.name = e;
                eff.SetActive(true);

                var ad = GetComponent<AutoDestroy>();
                if (ad) ad.enabled = false;

                effs.Add(i);
                Util.AddChild(w.transform, eff.transform);
            }
        }
    }

    public void UpdateWeaponEffectInvert()
    {
        foreach (var w in m_weapons)
        {
            if (!w) continue;
            var es = m_weaponEffects.Get(w.GetInstanceID());
            if (es == null || es.Count < 1) continue;
            foreach (var e in es)
            {
                if (!e) continue;
                e.inverted = !m_creature.isForward;
            }
        }
    }

    public bool UpdateWeapon()
    {
        var success = true;
        success &= UpdateWeaponModel(2);
        success &= UpdateWeaponAnimator();
        return success;
    }

    public void UpdateRenderGuard()
    {
        if (!m_renderGuardMesh) return;

        var xx = (float)m_creature.hitColliderSize * 0.5f;
        var yy = (float)m_creature.height;

        m_tmpVertices[0] = new Vector3(0,  0,  xx);
        m_tmpVertices[1] = new Vector3(0, yy, -xx);
        m_tmpVertices[2] = new Vector3(0,  0, -xx);
        m_tmpVertices[3] = new Vector3(0, yy,  xx);

        m_renderGuardMesh.vertices = m_tmpVertices;
        m_renderGuardMesh.SetIndices(m_tmpIndices, MeshTopology.Triangles, 0);
        m_renderGuardMesh.RecalculateBounds();
        m_renderGuardMesh.RecalculateNormals();
    }

    public void SetWeaponBind(StateMachineInfo.FrameData[] binds, bool off = false)
    {
        for (var i = 0; i < binds.Length; ++i)
        {
            var bind = binds[i];
            SetWeaponBind(bind.intValue0, bind.intValue1, off);
        }
    }

    public void SetWeaponBind(int index, int bindID, bool off = false)
    {
        var ws = off ? m_offWeapons : m_mainWeapons;

        if (ws.Count < 1) return;  // We do not have any weapon...

        var w = ws.Get(index);
        if (!w)
        {
            var wi = off ? m_offWeaponInfo : m_weaponInfo;
            if (!off) Logger.LogWarning("Set weapon bind failed, could not find weapon index {0} from current weapon [{1}: {3}:{2}]", index, wi.weaponID, wi.weaponItemId, wi.name);
            return;
        }

        var bind = ConfigManager.Get<BindInfo>(bindID);
        if (!bind) return;

        var node = Util.FindChild(transform, bind.bone);
        if (!node)
        {
            Logger.LogWarning("CreatureBehaviour::SetWeaponBind: Could not bind weapon, could not find bind config [{0}]", bindID);
            return;
        }

        var wt = w.transform;
        wt.SetParent(node);
        wt.localPosition    = bind.offset * 0.1f;
        wt.localEulerAngles = bind.rotation;
    }

    public void SetWeaponVisibility(int index, bool visible, bool off)
    {
        var ws = off ? m_offWeapons : m_mainWeapons;

        if (index < 0) foreach (var w in ws) w.Value?.SetActive(visible);
        else
        {
            var w = ws.Get(index);
            if (!w)
            {
                var wi = off ? m_offWeaponInfo : m_weaponInfo;
                if (!off) Logger.LogWarning("Set weapon visibility failed, could not find weapon index {0} from current weapon [{1}: {3}:{2}]", index, wi.weaponID, wi.weaponItemId, wi.name);
                return;
            }
            w.SetActive(visible);
        }
    }

    public void SetWeaponLayer(int layer = Layers.WEAPON, int layerOff = Layers.WEAPON_OFF)
    {
        foreach (var w in m_mainWeapons)
        {
            if (!w.Value) continue;
            Util.SetLayer(w.Value.transform, layer);
        }

        foreach (var w in m_offWeapons)
        {
            if (!w.Value) continue;
            Util.SetLayer(w.Value.transform, layerOff);
        }
    }

    private void CreateGuardRenderer()
    {
        var m = this.GetComponentDefault<MeshFilter>();

        m_renderGuardMesh = m.mesh;
        if (!m_renderGuardMesh)
        {
            m_renderGuardMesh = new Mesh() { name = name, hideFlags = HideFlags.HideAndDontSave };
            m.mesh = m_renderGuardMesh;
        }

        UpdateRenderGuard();
        
        var r = this.GetComponentDefault<MeshRenderer>();

        r.lightProbeUsage      = UnityEngine.Rendering.LightProbeUsage.Off;
        r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        r.shadowCastingMode    = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows       = false;
        r.materials            = m_guardMaterials;
    }

    [ContextMenu("WAWAWA!")]
    void LogVerts()
    {
        var m = this.GetComponentDefault<MeshFilter>();
        var verts = m.mesh.vertices;
        for (var i = 0; i < verts.Length; ++i)
        {
            Logger.LogWarning("{0:00} -- {1}", i, verts[i]);
        }

        var inde = m.mesh.GetIndices(0);
        for (var i = 0; i < inde.Length; ++i)
        {
            Logger.LogDetail("{0:00} -- {1}", i, inde[i]);
        }

        Logger.LogDetail("Type: {0}", m.mesh.indexFormat);
    }

    private void Update()
    {
        #if UNITY_EDITOR
        if (Root.logicPaused) return;
        #endif

        var delta = (float)(Util.GetMillisecondsTime(Time.smoothDeltaTime) * m_creature.localTimeScale);

        if (!m_creature.enableUpdate)
        {
            m_creature.position = m_creature.position_;
            UpdateColliderViewPositions();
            return;
        }

        var s = m_creature.currentState;
        var mt = m_motionTime;
        if (m_creature.stateMachine.playable)
        {
            if (s.falling) m_fallTime += delta;
            else m_motionTime = Util.GetMillisecondsTime(m_motionTime + delta * m_creature.currentState.animationSpeed);
        }

        if (m_creature.syncLogicPosition) m_creature.position = m_creature.position_;  // If we hit edge or collider in prev logic frame, set current position to logic position
        else
        {
            var tp = (Vector3)m_creature.position_;

            if (m_creature.stateMachine.playable)
            {
                tp = m_creature.position;

                if (!s.inMotion)
                {
                    //只能使用实际高度。不能使用逻辑高度，否则会出现逻辑高度到0后，实际高度一致在半空中不往下掉
                    if (m_creature.position.y > 0)
                    {
                        var df = delta * s.fallSpeed;
                        tp += (Vector3)df;
                    }
                    else
                    {
                        if (m_creature.realMoving)
                        {
                            var moveDst = m_creature.speedRun * m_creature.standardRunSpeed * delta;
                            if (!m_creature.isForward) moveDst = -moveDst;
                            tp.x += (float)moveDst;
                        }
                        else tp = m_creature.position_;
                    }
                }
                else
                {
                    
                    tp = s.SimulateMotion(m_motionTime, m_creature.direction, m_creature.motionOrigin);
                    var lt = s.info.landingFrame * 33 * 0.001f;
                    if (lt > 0 && tp.y > 0)
                    {
                        if (mt < lt && m_motionTime > lt)
                        {
                            m_fallTime += m_motionTime - lt;
                            m_motionTime = lt;
                            tp = s.SimulateMotion(m_motionTime, m_creature.direction, m_creature.motionOrigin);
                        }
                    }
                }

                // knockback
                if (m_knockbackDistance != 0)
                {
                    var kd = m_knockbackDistance * 10 * delta;
                    m_knockbackDistance -= kd;
                    if (kd < 0 && m_knockbackDistance > -0.01f || kd > 0 && m_knockbackDistance < 0.01f)
                        m_knockbackDistance = 0;

                    tp.x += kd;
                }

                if (tp.y < 0) tp.y = 0;
            }
            m_creature.position = tp;
        }

        if (m_shake && Time.timeScale > 0)
        {
            var frames = (int)(CombatConfig.shitShakeFrames / Time.timeScale);
            var off = m_creature.model.localPosition;

            if (frames < 1) frames = 1;
            if ((++m_shakeIdx % frames) == 0)
            {
                off.z += m_lastShake;

                if (m_shakeIdx % (2 * frames) == 0)
                    m_lastShake = -m_lastShake;
            }

            m_creature.model.localPosition = off;
        }
        UpdateColliderViewPositions();
    }

    protected void OnDestroy()
    {
        foreach (var es in m_weaponEffects) es.Value.Clear();
        m_weaponEffects.Clear();

        m_weaponInfo    = null;
        m_offWeaponInfo = null;
        m_weapons       = null;
        m_mainWeapons   = null;
        m_offWeapons    = null;
        m_creature      = null;
        m_weaponEffects = null;
        
        m_normalDamageTrans?.Clear();
        m_buffDamageTrans?.Clear();

        m_normalDamageTrans = null;
        m_buffDamageTrans = null;

        if (m_renderGuardMesh) Destroy(m_renderGuardMesh);
        m_renderGuardMesh = null;
    }

    #region Damage number node

    private List<Transform> m_normalDamageTrans;
    private List<Transform> m_buffDamageTrans;
    private bool m_damageNodeInitialized = false;

    public void InitDamagePoint()
    {
        if (m_damageNodeInitialized) return;
        m_damageNodeInitialized = true;

        m_normalDamageTrans = new List<Transform>();
        m_buffDamageTrans   = new List<Transform>();

        m_normalDamageTrans.Clear();
        m_buffDamageTrans.Clear();

        var rootTrans = transform.Find(Creature.DAMAGE_POINT_ROOT_NAME);
        if (!rootTrans) return;

        rootTrans.gameObject.SetActive(false);

        var nodes = Creature.DAMAGE_CHILD_POINT_NAMES;
        for (var i = 0; i < nodes.Length; ++i)
        {
            var t = rootTrans.Find(nodes[i]);
            if (!t) continue;

            if (i == 0) m_normalDamageTrans = t.GetChildList();
            else m_buffDamageTrans = t.GetChildList();
        }
    }

    public Vector3 GetDamagePos(bool isBuff = false)
    {
        InitDamagePoint();

        var nodes = isBuff ? m_buffDamageTrans : m_normalDamageTrans;
        if (nodes.Count == 0) return Vector3.zero;

        var t = nodes[Random.Range(0, nodes.Count)];
        return t.position;
    }

    #endregion
}