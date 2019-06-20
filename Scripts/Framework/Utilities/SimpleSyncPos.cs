/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Simple sync position
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-07-26
 * 
 ***************************************************************************************************/

using UnityEngine;

[AddComponentMenu("HYLR/Utilities/Simple Sync Pos")]
public class SimpleSyncPos : MonoBehaviour
{
    public Transform syncTarget;
    public Vector3 offset;
    public bool    syncPosition = true;
    public bool    syncRotation = false;
    public bool    syncState    = false;

    private bool m_oldVisible = false;

    void OnStart() { offset = transform.position; }
    void OnEnable() { Update(); }

    public void Update ()
    {
        if (syncTarget)
        {
            if (syncPosition) transform.position = syncTarget.position + offset;
            if (syncRotation) transform.rotation = syncTarget.rotation;

            var visible = syncTarget.gameObject.activeInHierarchy;
            if (syncState && m_oldVisible != visible)
            {
                m_oldVisible = visible;
                for (var i = 0; i < transform.childCount; ++i)
                    transform.GetChild(i).gameObject.SetActive(m_oldVisible);
            }
        }
        else
        {
            syncTarget = null;
            if (syncState) Util.ClearChildren(transform, true);
        }
    }
}
