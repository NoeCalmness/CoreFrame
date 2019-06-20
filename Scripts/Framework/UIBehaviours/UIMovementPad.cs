/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Launcher class.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-05
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("HYLR/UI/Movement Pad")]
public class UIMovementPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    #region Event handlers

    [System.Serializable] public class OnTouchBegin : UnityEvent<Vector2> { }
    [System.Serializable] public class OnTouchEnd : UnityEvent<Vector2> { }
    [System.Serializable] public class OnTouchMove : UnityEvent<Vector2, Vector2> { }
    [System.Serializable] public class OnTouchMoveSimple : UnityEvent<Vector3> { }

    [SerializeField] public OnTouchBegin onTouchBegin;
    [SerializeField] public OnTouchEnd   onTouchEnd;
    [SerializeField] public OnTouchMove  onTouchMove;
    [SerializeField] public OnTouchMoveSimple onTouchMoveSimple;

    #endregion

    #region Public fields

    public float simpleMoveSpeed = 0.1f;
    public bool LockX = false;
    public bool LockY = false;

    #endregion

    #region Private fields

    private RectTransform m_rect;

    private Vector2 m_touchDirection;
    private Vector2 m_lastPoint;
    private Vector2 m_delta;

    private int m_pointerID = int.MinValue;

    #endregion

    #region Event interface

    public void OnPointerDown(PointerEventData eventData)
    {
        BeginTouch(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != m_pointerID) return;

        EndTouch();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (m_pointerID == int.MinValue) BeginTouch(eventData);
        else if (m_pointerID != eventData.pointerId) return;

        CalculateTouchDirection(eventData.position, eventData.pressEventCamera);
    }

    private void CalculateTouchDirection(Vector2 screenPoint, Camera camera)
    {
        Vector2 lp;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_rect, screenPoint, camera, out lp);

        if (!LockX)
        {
            var w = lp.x > 0 ? m_rect.sizeDelta.x * (1 - m_rect.pivot.x) : m_rect.sizeDelta.x * m_rect.pivot.x;
            m_touchDirection.x = w == 0 ? lp.x > 0 ? 1 : -1 : lp.x / w;
        }
        else m_touchDirection.x = 0;

        if (!LockY)
        {
            var h = lp.y > 0 ? m_rect.sizeDelta.y * (1 - m_rect.pivot.y) : m_rect.sizeDelta.y * m_rect.pivot.y;
            m_touchDirection.y = h == 0 ? lp.y > 0 ? 1 : -1 : lp.y / h;
        }
        else m_touchDirection.y = 0;

        m_delta += screenPoint - m_lastPoint;
        m_lastPoint = screenPoint;
    }

    #endregion

    #region Behaviour

    private void Awake()
    {
        m_rect = transform as RectTransform;
        this.GetComponentDefault<Image>();
    }

	private void Update()
	{
        if (m_pointerID == int.MinValue) return;

        onTouchMove?.Invoke(m_touchDirection, m_delta);
        onTouchMoveSimple?.Invoke(m_touchDirection * simpleMoveSpeed);

        m_delta.Set(0, 0);
    }

    private void OnDisable()
    {
        EndTouch();
    }

    private void OnDestroy()
    {
        EndTouch();

        onTouchBegin = null;
        onTouchEnd   = null;
        onTouchMove  = null;
    }

    private void BeginTouch(PointerEventData eventData)
    {
        m_pointerID = eventData.pointerId;

        m_lastPoint = eventData.position;
        m_touchDirection.Set(0, 0);
        m_delta.Set(0, 0);

        CalculateTouchDirection(eventData.position, eventData.pressEventCamera);

        onTouchBegin?.Invoke(m_touchDirection);
    }

    private void EndTouch()
    {
        if (m_pointerID == int.MinValue) return;

        onTouchEnd?.Invoke(m_touchDirection);

        m_pointerID = int.MinValue;

        m_delta.Set(0, 0);
        m_touchDirection.Set(0, 0);
        m_lastPoint.Set(0, 0);
    }

    #endregion

    #region Editor helper

    [Header("Editor Helper")]
    public bool e_DrawEdge = true;

#if UNITY_EDITOR
    private Vector3[] m_worldCornners = new Vector3[4];

    private void OnDrawGizmos()
    {
        if (e_DrawEdge)
        {
            if (m_worldCornners == null || m_worldCornners.Length != 4) m_worldCornners = new Vector3[4];
            if (!m_rect) m_rect = transform as RectTransform;

            m_rect.GetLocalCorners(m_worldCornners);

            for (var i = 0; i < m_worldCornners.Length; ++i)
            {
                var p = m_worldCornners[i];

                if (i == 0 || i == 2) p.y = 0;
                if (i == 1 || i == 3) p.x = 0;

                m_worldCornners[i] = m_rect.TransformPoint(p);
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(m_worldCornners[0], m_worldCornners[2]);
            Gizmos.DrawLine(m_worldCornners[1], m_worldCornners[3]);
        }
    }
#endif

    #endregion
}
