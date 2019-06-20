/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * UIDraggble component.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-15
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(GraphicRaycaster))]
[AddComponentMenu("HYLR/UI/Drag Resize Object")]
public class UIDragResizeObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Flags]
    public enum ResizeType { NONE = 0, LEFT = 0x01, TOP = 0x2, RIGHT = 0x04, BOTTOM = 0x08, TOP_LEFT = TOP | LEFT, TOP_RIGHT = TOP | RIGHT, BOTTOM_RIGHT = BOTTOM | RIGHT, BOTTOM_LEFT = BOTTOM | LEFT }

    public Vector2 dragEnd { get { return m_dragEnd; } }

    public bool  draggble   = true;
    public bool  resizable  = true;
    public float resizeEdge = 10.0f;

    public Vector2 maxSize;
    public Vector2 minSize = new Vector2(100, 100);

    public RectTransform titleBar;

    /// <summary>
    /// Ensure this object always visible in its parent rect.
    /// </summary>
    public bool clampEdge = true;

    [SerializeField]
    private Vector2 m_dragBegin;
    [SerializeField]
    private Vector2 m_dragEnd;
    [SerializeField]
    private Vector2 m_originPosition;
    [SerializeField]
    private Vector2 m_originSize;

    [SerializeField]
    private bool m_dragging = false;
    [SerializeField]
    private bool m_resizing = false;
    [SerializeField]
    private ResizeType m_resizeType = ResizeType.NONE;

    private RectTransform rect;

    private void Start()
    {
        rect = transform as RectTransform;

        var size = new Vector2(rect.rect.width, rect.rect.height);
        var pos  = rect.TransformPoint(new Vector2(rect.rect.position.x, rect.rect.position.y + size.y));

        if (maxSize == Vector2.zero) maxSize = UIManager.referenceResolution;

        size.x = Mathf.Clamp(size.x, 0, maxSize.x);
        size.y = Mathf.Clamp(size.y, 0, maxSize.y);

        rect.anchorMin = new Vector2(0, 1.0f);
        rect.anchorMax = new Vector2(0, 1.0f);
        rect.pivot     = new Vector2(0, 1.0f);

        rect.position   = pos;
        rect.sizeDelta  = size;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!draggble && !resizable) return;

        m_resizing   = false;
        m_dragging   = false;
        m_resizeType = ResizeType.NONE;

        m_dragBegin      = eventData.position;
        m_originPosition = rect.anchoredPosition;
        m_originSize     = rect.sizeDelta;

        if (resizable && (!titleBar || !eventData.selectedObject || eventData.selectedObject != titleBar.gameObject && !eventData.selectedObject.transform.IsChildOf(titleBar)))
        {
            Vector2 pos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, m_dragBegin, UIManager.worldCamera, out pos))
            {
                var re = resizeEdge;
                var hw = rect.sizeDelta.x - re;
                var hh = rect.sizeDelta.y - re;

                if (pos.x <  re) m_resizeType |= ResizeType.LEFT;
                if (pos.x >  hw) m_resizeType |= ResizeType.RIGHT;
                if (pos.y > -re) m_resizeType |= ResizeType.TOP;
                if (pos.y < -hh) m_resizeType |= ResizeType.BOTTOM;

                m_resizing = m_resizeType != ResizeType.NONE;
                if (m_resizing) return;
            }
        }

        if (draggble && (!titleBar || eventData.selectedObject == titleBar.gameObject)) m_dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!m_resizing && !m_dragging) return;

        var offset  = eventData.position - m_dragBegin;
        var pos     = m_originPosition;
        var refSize = UIManager.referenceResolution;
        var curSize = UIManager.viewResolution;
        var maxPosX = (refSize.x - rect.sizeDelta.x);
        var minPosY = (rect.sizeDelta.y - refSize.y);

        if (m_resizing)
        {
            var size  = m_originSize;
            var delta = offset;

            if ((m_resizeType & ResizeType.LEFT) != 0) pos.x += offset.x;
            if ((m_resizeType & ResizeType.TOP)  != 0) pos.y += offset.y;

            if (pos.x < 0) delta.x -= pos.x;
            if (pos.y > 0) delta.y -= pos.y;

            pos.x = Mathf.Clamp(pos.x, 0, maxPosX);
            pos.y = Mathf.Clamp(pos.y, minPosY, 0);

            if ((m_resizeType & ResizeType.LEFT)   != 0) size.x -= delta.x;
            if ((m_resizeType & ResizeType.RIGHT)  != 0) size.x += delta.x;
            if ((m_resizeType & ResizeType.BOTTOM) != 0) size.y -= delta.y;
            if ((m_resizeType & ResizeType.TOP)    != 0) size.y += delta.y;

            size.x = Mathf.Clamp(size.x, minSize.x, Mathf.Min(maxSize.x, refSize.x - pos.x));
            size.y = Mathf.Clamp(size.y, minSize.y, Mathf.Min(maxSize.y, refSize.y + pos.y));

            rect.sizeDelta = size;
        }
        else if (m_dragging)
        {
            pos = m_originPosition + offset;

            if (clampEdge)
            {
                pos.x = Mathf.Clamp(pos.x, 0, maxPosX);
                pos.y = Mathf.Clamp(pos.y, minPosY, 0);
            }
        }

        rect.anchoredPosition = pos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        m_dragEnd    = eventData.position;
        m_resizing   = false;
        m_dragging   = false;
        m_resizeType = ResizeType.NONE;
    }

    private void OnDisable()
    {
        m_dragging   = false;
        m_resizing   = false;
        m_resizeType = ResizeType.NONE;
    }

    #region Editor helper

    public bool e_DrawResizeEdge = true;
    public bool e_DrawWorldEdge  = true;
    [SerializeField]
    private Vector3[] m_worldCornners = new Vector3[4];

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void OnDrawGizmosSelected()
    {
        if (!rect) rect = transform as RectTransform;
        if (m_worldCornners == null || m_worldCornners.Length != 4) m_worldCornners = new Vector3[4];

        if (e_DrawWorldEdge)
        {
            rect.GetWorldCorners(m_worldCornners);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(m_worldCornners[0], m_worldCornners[1]);
            Gizmos.DrawLine(m_worldCornners[1], m_worldCornners[2]);
            Gizmos.DrawLine(m_worldCornners[2], m_worldCornners[3]);
            Gizmos.DrawLine(m_worldCornners[3], m_worldCornners[0]);
        }

        if (e_DrawResizeEdge)
        {
            rect.GetLocalCorners(m_worldCornners);
            for (var i = 0; i < m_worldCornners.Length; ++i)
            {
                var off = new Vector3(resizeEdge, resizeEdge);
                if (i == 2 || i == 3) off.x = -off.x;
                if (i == 1 || i == 2) off.y = -off.y;
                m_worldCornners[i] = rect.TransformPoint(m_worldCornners[i] + off);
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(m_worldCornners[0], m_worldCornners[1]);
            Gizmos.DrawLine(m_worldCornners[1], m_worldCornners[2]);
            Gizmos.DrawLine(m_worldCornners[2], m_worldCornners[3]);
            Gizmos.DrawLine(m_worldCornners[3], m_worldCornners[0]);
        }
    }
#endif

    #endregion
}
