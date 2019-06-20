/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Single visible object container
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-03-02
 * 
 ***************************************************************************************************/

using UnityEngine;

[AddComponentMenu("HYLR/UI/Single Container")]
[ExecuteInEditMode]
public class UISingleContainer : MonoBehaviour
{
    public string currentVisible
    {
        get { return m_currentVisible; }
        set
        {
            if (m_currentVisible == value) return;
            m_currentVisible = value;

            UpdateVisible();
        }
    }
    [SerializeField, Set("currentVisible")]
    private string m_currentVisible = "";

    public string[] alwaysVisible = new string[] { };
    
    private void Awake() { UpdateVisible(); }

    public void SetCurrentFromNodeName(RectTransform node)
    {
        if (node) currentVisible = node.name;
    }

    public void UpdateVisible()
    {
        Util.DisableAllChildren(transform, alwaysVisible);

        var t = transform.Find(m_currentVisible);
        if (t) t.gameObject.SetActive(true);
    }

    public void UpdateVisible(string visibleTarget)
    {
        currentVisible = visibleTarget;
    }
}
