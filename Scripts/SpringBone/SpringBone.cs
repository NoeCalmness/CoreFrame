/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * SpringBone component ported from UnityChan!
 * 
 * Original Author: N.Kobayashi, ricopin <https://twitter.com/ricopin416>
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-12-13
 * 
 ***************************************************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;

public class SpringBone : MonoBehaviour
{
    #region Weapon param holder

    [Serializable]
    public class WeaponParam
    {
        public int weaponID;

        public float radius = 0.05f;

        public float stiffnessForce = 0.01f;
        public float dragForce = 0.4f;
        public float threshold = 0.01f;

        public bool hasChestCollider = false;
        public bool hasWaistCollider = false;

        public SpringCollider[] colliders = { };

        public WeaponParam() { }

        public WeaponParam(int _weaponID, float _radius, float _stiffnessForce, float _dragForce, float _threshold, bool _hasChestCollider, bool _hasWaistCollider, SpringCollider[] _colliders)
        {
            weaponID = _weaponID;
            Update(_radius, _stiffnessForce, _dragForce, _threshold, _hasChestCollider, _hasWaistCollider, _colliders);
        }

        public void CopyFrom(WeaponParam other)
        {
            if (other == null) return;
            Update(other.radius, other.stiffnessForce, other.dragForce, other.threshold, other.hasChestCollider, other.hasWaistCollider, other.colliders);
        }

        public void Update(float _radius, float _stiffnessForce, float _dragForce, float _threshold, bool _hasChestCollider, bool _hasWaistCollider, SpringCollider[] _colliders)
        {
            radius           = _radius;
            stiffnessForce   = _stiffnessForce;
            dragForce        = _dragForce;
            threshold        = _threshold;
            hasChestCollider = _hasChestCollider;
            hasWaistCollider = _hasWaistCollider;

            Array.Resize(ref colliders, _colliders.Length);
            Array.Copy(_colliders, colliders, colliders.Length);
        }
    }

    #endregion

    public Transform child;

    public Vector3 boneAxis = new Vector3 (0.0f, 1.0f, 0.0f);

    public bool debug { get; set; } = true;

    public bool hasChestCollider = false;
    public bool hasWaistCollider = false;

    public SpringCollider[] colliders = { };

    public float radius         = 0.05f;
    public float stiffnessForce = 0.01f;
    public float dragForce      = 0.4f;
    public float threshold      = 0.01f;

    public Quaternion originRotation;

    private float      m_springLength;
    private Vector3    m_currTipPos;
    private Vector3    m_prevTipPos;

    private Transform m_transform;

    [SerializeField]
    private List<WeaponParam> m_weaponParams = new List<WeaponParam>();
    [SerializeField]
    private List<int> m_excludeWeapons = new List<int>();

    private SpringCollider m_chest = null, m_waist = null;

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
            param = new WeaponParam(weaponID, radius, stiffnessForce, dragForce, threshold, false, false, colliders);
            m_weaponParams.Add(param);
        }

        return param;
    }

    public List<int> GetSavedWeaponIDs()
    {
        var ids = new List<int>();
        foreach (var p in m_weaponParams) ids.Add(p.weaponID);
        return ids;
    }

    public void UpdateParams(int weaponID, SpringCollider chest = null, SpringCollider waist = null)
    {
        enabled = !IsWeaponExcluded(weaponID);

        var param = GetWeaponParam(weaponID);
        if (param == null) param = GetWeaponParam(0);
        if (param == null) return;

        radius           = param.radius;
        stiffnessForce   = param.stiffnessForce;
        dragForce        = param.dragForce;
        threshold        = param.threshold;
        colliders        = param.colliders;
        hasChestCollider = param.hasChestCollider;
        hasWaistCollider = param.hasWaistCollider;

        m_chest = hasChestCollider ? chest : null;
        m_waist = hasWaistCollider ? waist : null;
    }

    public void CopyParams(int from, int to)
    {
        var fp = GetWeaponParam(from);
        if (fp == null) return;

        var tp = GetWeaponParam(to, true);
        tp.CopyFrom(fp);
    }

    public void ExculeWeapon(int weaponID, bool exclude = true)
    {
        m_excludeWeapons.Remove(weaponID);
        if (exclude) m_excludeWeapons.Add(weaponID);
    }

    public bool IsWeaponExcluded(int weaponID)
    {
        return m_excludeWeapons.Contains(weaponID);
    }

    private void Awake()
    {
        m_transform = transform;
        originRotation = m_transform.localRotation;
    }

    private void Start()
    {
        m_springLength = Vector3.Distance(transform.position, child.position);

        m_currTipPos = child.position;
        m_prevTipPos = m_currTipPos;
    }

    public void UpdateSpring(float dynamicRatio, ref Vector3 springForce)
    {
        m_transform.localRotation = originRotation;

        var rot   = m_transform.rotation;
        var pos   = m_transform.position;
        var tip   = m_currTipPos - m_prevTipPos;
        var force = rot * boneAxis * stiffnessForce - tip * dragForce + springForce;
        var temp  = m_currTipPos;

        m_currTipPos = tip + m_currTipPos + force;
        m_currTipPos = (m_currTipPos - pos).normalized * m_springLength + pos;

        m_prevTipPos = temp;

        foreach (var collider in colliders) UpdateColliders(collider, ref pos);

        if (m_chest) UpdateColliders(m_chest, ref pos);
        if (m_waist) UpdateColliders(m_waist, ref pos);

        var aimVector   = m_transform.TransformDirection(boneAxis);
        var aimRotation = Quaternion.FromToRotation(aimVector, m_currTipPos - pos);

        m_transform.rotation = Quaternion.Lerp(rot, aimRotation * rot, dynamicRatio);
    }

    private void UpdateColliders(SpringCollider collider, ref Vector3 p)
    {
        if (!collider.o.activeSelf) return;

        var r = collider.radius + radius;
        var cp = collider.t.position;
        var dist = m_currTipPos - cp;

        if (dist.magnitude <= r)
        {
            m_currTipPos = cp + dist.normalized * r;
            m_currTipPos = (m_currTipPos - p).normalized * m_springLength + p;
        }
    }

    #region Editor helper

#if UNITY_EDITOR
    public void Initialize()
    {
        Awake();
        Start();

        foreach (var collider in colliders) collider?.Initialize();

        m_chest?.Initialize();
        m_waist?.Initialize();
    }

    private void OnDrawGizmos()
    {
        if (debug)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(m_currTipPos, radius * transform.lossyScale.x);
        }
    }

    public List<WeaponParam> GetWeaponParams()
    {
        return m_weaponParams;
    }
#endif

    #endregion
}