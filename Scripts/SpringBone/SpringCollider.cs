/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * SpringBone component ported from UnityChan!
 * 
 * Original Author: ricopin <https://twitter.com/ricopin416>
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-12-13
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

public class SpringCollider : MonoBehaviour
{
    #region Weapon param holder

    [Serializable]
    public class WeaponParam
    {
        public int weaponID;

        public float radius = 0.02f;

        public WeaponParam() { }

        public WeaponParam(int _weaponID, float _radius)
        {
            weaponID = _weaponID;
            Update(_radius);
        }

        public void CopyFrom(WeaponParam other)
        {
            if (other == null) return;
            Update(other.radius);
        }

        public void Update(float _radius)
        {
            radius   = _radius;
        }
    }

    #endregion

    public bool debug { get; set; } = true;
    public float radius = 0.02f;

    [NonSerialized]
    public Transform t;
    [NonSerialized]
    public GameObject o;

    [SerializeField]
    private List<WeaponParam> m_weaponParams = new List<WeaponParam>();

    private void Awake()
    {
        t = transform;
        o = gameObject;
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
            param = new WeaponParam(weaponID, radius);
            m_weaponParams.Add(param);
        }

        return param;
    }

    public void UpdateParams(int weaponID)
    {
        var param = GetWeaponParam(weaponID);
        if (param == null) param = GetWeaponParam(0);
        if (param == null) return;

        radius = param.radius;
    }

    #region Editor helper

#if UNITY_EDITOR
    public void Initialize()
    {
        Awake();
    }

    private void OnDrawGizmos()
    {
        if (debug)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, radius * transform.lossyScale.x);
        }
    }
#endif

    #endregion
}