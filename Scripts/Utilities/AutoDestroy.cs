/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Auto destruct
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-08-31
 * 
 ***************************************************************************************************/

using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [Tooltip("自动销毁延时（毫秒）")]
    public int delay = 1500;

    [SerializeField]
    [Tooltip("是否启用"), Set("enabled")]
    private bool m_enabled = true;

    public new bool enabled
    {
        get { return m_enabled; }
        set
        {
            if (m_enabled == value) return;
            m_enabled = value;

            ObjectManager.onLogicUpdate -= LogicUpdate;

            if (m_enabled) ObjectManager.onLogicUpdate += LogicUpdate;
        }
    }

    private void Awake()
    {
        if (m_enabled)
            ObjectManager.onLogicUpdate += LogicUpdate;
    }

    private void LogicUpdate(int diff)
    {
        if (!this || !gameObject || (delay -= diff) > 0) return;
        Destroy(gameObject);
        ObjectManager.onLogicUpdate -= LogicUpdate;
    }

    private void OnDestroy()
    {
        ObjectManager.onLogicUpdate -= LogicUpdate;
    }
}
