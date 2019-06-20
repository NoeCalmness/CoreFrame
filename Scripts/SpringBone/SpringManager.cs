/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * SpringBone component ported from UnityChan!
 * 
 * Original Author: N.Kobayashi, Y.Ebata, ricopin <https://twitter.com/ricopin416>
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-12-13
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

public class SpringManager : MonoBehaviour
{
    #region Weapon param holder

    [Serializable]
    public class WeaponParam
    {
        public int weaponID;

        public float dynamicRatio = 1.0f;

        public Vector3 windStrength = new Vector3(0.005f, 0, 0);

        public WeaponParam() { }

        public WeaponParam(int _weaponID, float _dynamicRatio, Vector3 _windStrength)
        {
            weaponID = _weaponID;
            Update(_dynamicRatio, _windStrength);
        }

        public void CopyFrom(WeaponParam other)
        {
            if (other == null) return;
            Update(other.dynamicRatio, other.windStrength);
        }

        public void Update(float _dynamicRatio, Vector3 _windStrength)
        {
            dynamicRatio = _dynamicRatio;
            windStrength = _windStrength;
        }
    }

    #endregion

    #region Static functions

    public const string COLLIDER_NAME_CHEST = "_collider_chest";
    public const string COLLIDER_NAME_WAIST = "_collider_waist";

    public static List<SpringBone> SearchSpringBones(Transform root, Transform node, List<SpringBone> list = null)
    {
        if (list == null) list = new List<SpringBone>();

        var manager = node.GetComponent<SpringManager>();
        if (manager && manager.transform != root) return list;

        var bone = node.GetComponent<SpringBone>();
        if (bone) list.Add(bone);

        for (int i = 0, c = node.childCount; i < c; ++i)
            SearchSpringBones(root, node.GetChild(i), list);

        return list;
    }

    public static List<SpringCollider> SearchSpringColliders(Transform root, Transform node, List<SpringCollider> list = null)
    {
        if (list == null) list = new List<SpringCollider>();

        var manager = node.GetComponent<SpringManager>();
        if (manager && manager.transform != root) return list;

        var collider = node.GetComponent<SpringCollider>();
        if (collider) list.Add(collider);

        for (int i = 0, c = node.childCount; i < c; ++i)
            SearchSpringColliders(root, node.GetChild(i), list);

        return list;
    }

    public static Transform SearchRootNode(SpringManager manager)
    {
        var boneRoot = manager.transform.Find("model/Bip001");
        if (!boneRoot) boneRoot = manager.transform.Find("Bip001");
        return boneRoot ? boneRoot : manager.transform;
    }

    public static Transform SearchParentNode(SpringManager manager)
    {
        var root = manager.transform.parent;
        while (root)
        {
            var m = root.GetComponent<SpringManager>();
            if (m) return root;
            root = root.parent;
        }

        return manager.transform;
    }

    #endregion

    public bool simulateWind
    {
        get { return m_simulateWind; }
        set
        {
            if (m_simulateWind == value) return;   
            m_simulateWind = value;

            if (!m_simulateWind) springForce.Set(0, 0, 0);
        }
    }
    [SerializeField, Set("simulateWind")]
    private bool m_simulateWind = false;

    public float dynamicRatio = 1.0f;
    public Vector3 windStrength = new Vector3(0.005f, 0, 0);
    public Vector3 springForce = new Vector3(0, 0, 0);

    private List<SpringBone> m_bones = new List<SpringBone>();
    private List<SpringCollider> m_colliders = new List<SpringCollider>();

    [SerializeField]
    private List<WeaponParam> m_weaponParams = new List<WeaponParam>();

    public SpringCollider chest { get { return m_chest; } }
    public SpringCollider waist { get { return m_waist; } }

    private SpringCollider m_chest = null, m_waist = null;

    private bool m_initialized = false;

    public int currentWeapon
    {
        get { return m_currentWeapon; }
        set
        {
            if (m_currentWeapon == value) return;

            Logger.LogInfo("Spring manager: Current weapon changed to {0} from {1}.", value, m_currentWeapon);

            m_currentWeapon = value;

            UpdateParams(m_currentWeapon);
            UpdateBonesAndColliders();
        }
    }
    private int m_currentWeapon = 0;

    public void SetExtendColliderState(bool _enable)
    {
        if (m_chest) m_chest.gameObject.SetActive(_enable);
        if (m_waist) m_waist.gameObject.SetActive(_enable);
    }

    public void SetState(bool _enable)
    {
        if (enabled == _enable) return;

        if (!_enable)
        {
            foreach (var bone in m_bones)
                bone.transform.localRotation = bone.originRotation;
        }
        enabled = _enable;
    }

    public void RemoveWeaponParam(int weaponID = -1)
    {
        if (weaponID < 0) m_weaponParams.Clear();
        else m_weaponParams.RemoveAll(p => p.weaponID == weaponID);
    }

    public WeaponParam GetWeaponParam(int weaponID, bool create = false)
    {
        var param = m_weaponParams.Find(p => p.weaponID == weaponID);
        if (create && param == null)
        {
            param = new WeaponParam(weaponID, dynamicRatio, windStrength);
            m_weaponParams.Add(param);
        }

        return param;
    }

    public void UpdateBonesAndColliders()
    {
        foreach (var bone in m_bones)
            bone.UpdateParams(m_currentWeapon, m_chest, m_waist);

        foreach (var collider in m_colliders)
            collider.UpdateParams(m_currentWeapon);

        if (m_chest) m_chest.UpdateParams(m_currentWeapon);
        if (m_waist) m_waist.UpdateParams(m_currentWeapon);
    }

    public void UpdateParams(int weaponID)
    {
        var param = GetWeaponParam(weaponID);
        if (param == null) param = GetWeaponParam(0);
        if (param == null) return;

        dynamicRatio = param.dynamicRatio;
        windStrength = param.windStrength;
    }

    public void CopyParams(int from, int to)
    {
        var fp = GetWeaponParam(from);
        if (fp == null) return;

        var tp = GetWeaponParam(to, true);
        tp.CopyFrom(fp);
    }

    public List<int> GetSavedWeaponIDs()
    {
        var ids = new List<int>();
        foreach (var p in m_weaponParams) ids.Add(p.weaponID);
        return ids;
    }

    public void UpdateBones(float t)
    {
        if (dynamicRatio == 0) return;

        if (simulateWind) springForce = Mathf.PerlinNoise(t, 0.0f) * windStrength;

        foreach (var bone in m_bones)
        {
            if (!bone.isActiveAndEnabled || dynamicRatio < bone.threshold) continue;

            bone.UpdateSpring(dynamicRatio, ref springForce);
        }
    }

    public void Initialize()
    {
        if (!m_initialized) return;
        m_initialized = true;

        var boneRoot = SearchRootNode(this);
        SearchSpringBones(boneRoot, boneRoot, m_bones);
        SearchSpringColliders(boneRoot, boneRoot, m_colliders);

        UpdateParams(m_currentWeapon);
        UpdateBonesAndColliders();
    }

    private void Awake()
    {
        var pb = SearchParentNode(this);
        var n = Util.FindChild(pb, COLLIDER_NAME_CHEST);
        if (n) m_chest = n.GetComponent<SpringCollider>();
        n = Util.FindChild(pb, COLLIDER_NAME_WAIST);
        if (n) m_waist = n.GetComponent<SpringCollider>();
    }

    private void Start()
    {
        Initialize();
    }
    
    private void LateUpdate()
    {
        if (Game.paused) return;
        UpdateBones(Time.time);
    }

    #region Editor helper

    [SerializeField, Set("drawBones")]
    private bool e_DrawBones = true;
    [SerializeField, Set("drawColliders")]
    private bool e_DrawColliders = true;

#if UNITY_EDITOR
    public bool drawBones
    {
        get { return e_DrawBones; }
        set
        {
            if (e_DrawBones == value) return;
            e_DrawBones = value;

            var root = SearchRootNode(this);
            var bones = SearchSpringBones(root, root);

            foreach (var bone in bones)
                bone.debug = e_DrawBones;
        }
    }
    public bool drawColliders
    {
        get { return e_DrawColliders; }
        set
        {
            if (e_DrawColliders == value) return;
            e_DrawColliders = value;

            var root = SearchRootNode(this);
            var colliders = SearchSpringColliders(root, root);

            foreach (var collider in colliders)
                collider.debug = e_DrawColliders;
        }
    }

    public void Refresh()
    {
        var root = SearchRootNode(this);
        var bones = SearchSpringBones(root, root);
        var colliders = SearchSpringColliders(root, root);

        foreach (var bone in bones)
            bone.UpdateParams(m_currentWeapon, m_chest, m_waist);

        foreach (var collider in colliders)
            collider.UpdateParams(m_currentWeapon);
    }

    public void ForceInitialize()
    {
        Awake();
        Start();

        foreach (var bone in m_bones) bone.Initialize();
    }

    private void OnValidate()
    {
        Refresh();
    }
#endif

    #endregion
}