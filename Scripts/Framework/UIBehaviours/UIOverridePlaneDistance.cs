/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-07-17
 * 
 ***************************************************************************************************/

using UnityEngine;

[AddComponentMenu("HYLR/UI/Override Plane Distance")]
public class UIOverridePlaneDistance : MonoBehaviour
{
    private Vector3 m_oap;  // original anchored position
    private Vector3 m_olp;  // original local position
    private Vector3 m_ols;  // original global scale

    [SerializeField]
    private float m_ofv = 60; // last camera fieldOfView
    [SerializeField]
    private float m_opd = 5;  // override plane distance

    private Canvas m_canvas; // root ui canvas

    private float m_npd = 5;

    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = transform as RectTransform;

        m_canvas = UIManager.canvas;

        m_oap = rectTransform.anchoredPosition3D;
        m_olp = rectTransform.localPosition;
        m_ols = rectTransform.localScale;

        m_ofv = UIManager.worldCamera.fieldOfView;
	}

    private void Update()
    {
        var c = UIManager.worldCamera;

        if (!c.orthographic && (c.fieldOfView != m_ofv || m_npd != m_opd))
        {
            m_npd = m_opd;
            m_ofv   = c.fieldOfView;

            var wp = rectTransform.parent.TransformPoint(m_olp);

            var on = wp - c.transform.position; on = on.normalized;
            var co = Vector3.Dot(c.transform.forward, on);
            var n = m_npd / co;
            var np = rectTransform.parent.InverseTransformPoint(on * n + c.transform.position);

            rectTransform.anchoredPosition3D = np - m_olp + m_oap;
            rectTransform.localScale = m_ols * m_npd / m_canvas.planeDistance;
        }
    }
}
