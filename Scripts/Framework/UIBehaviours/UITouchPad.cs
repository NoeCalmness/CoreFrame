/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Touch Pad
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-03-04
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("HYLR/UI/Touch Pad")]
public class UITouchPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    #region Event handlers

    [System.Serializable] public class OnTouchBegin : UnityEvent<Vector2> { }
    [System.Serializable] public class OnTouchEnd : UnityEvent<Vector2> { }
    [System.Serializable] public class OnTouchMove : UnityEvent<Vector2, Vector2> { }
    [System.Serializable] public class OnTouchStay : UnityEvent<float> { }

    [SerializeField] public OnTouchBegin onTouchBegin;
    [SerializeField] public OnTouchEnd   onTouchEnd;
    [SerializeField] public OnTouchMove  onTouchMove;
    [SerializeField] public OnTouchStay  onTouchStay;

    #endregion

    #region Public interface

    public void ResetMoveState()
    {
        if (m_pointerID == int.MinValue) return;

        m_startTouchTime = Time.time;
        m_startPoint = m_lastPoint;
        m_dist = 0;
        m_moved = false;
        m_resetMoveState = true;
    }

    #endregion

    #region Public fields

    public bool lockX = false;
    public bool lockY = false;
    public float moveFix = 0;

    #endregion

    #region Private fields

    private Vector2 m_direction;
    private Vector2 m_lastPoint;
    private Vector2 m_delta;
    private Vector2 m_startPoint;
    private Vector2 m_endPoint;

    private float m_startTouchTime = 0;
    private float m_dist = 0;
    private bool m_moved = false;
    private bool m_resetMoveState = false;

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

        if (m_resetMoveState)
        {
            m_startPoint = eventData.position;
            m_lastPoint  = m_startPoint;

            m_resetMoveState = false;
        }

        CalculateTouchDirection(eventData.position);
    }

    private void CalculateTouchDirection(Vector2 now)
    {
        m_delta = now - m_lastPoint;

        m_lastPoint = now;
        m_endPoint  = m_lastPoint;

        m_direction = m_endPoint - m_startPoint;
        m_dist = m_direction.magnitude;

        var mm = m_moved;
        if (!m_moved) m_moved = m_dist >= moveFix;

        if (lockX) m_direction.x = 0;
        if (lockY) m_direction.y = 0;
    }

    #endregion

    #region Behaviour

	private void Update()
	{
        if (m_pointerID == int.MinValue) return;

        if (!m_moved) onTouchStay.Invoke(Time.time - m_startTouchTime);
        else onTouchMove.Invoke(m_direction, m_delta);
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
        onTouchStay  = null;
    }

    private void BeginTouch(PointerEventData eventData)
    {
        m_pointerID = eventData.pointerId;

        m_startTouchTime = Time.time;
        m_dist = 0;
        m_moved = false;
        m_resetMoveState = false;

        m_lastPoint  = eventData.position;
        m_startPoint = m_lastPoint;

        CalculateTouchDirection(m_lastPoint);

        onTouchBegin?.Invoke(m_startPoint);
    }

    private void EndTouch()
    {
        if (m_pointerID == int.MinValue) return;
        m_pointerID = int.MinValue;

        onTouchEnd?.Invoke(m_endPoint);

        m_startTouchTime = 0;
        m_dist = 0;
        m_moved = false;
        m_resetMoveState = false;

        m_delta.Set(0, 0);
        m_direction.Set(0, 0);
        m_lastPoint.Set(0, 0);
        m_startPoint.Set(0, 0);
        m_endPoint.Set(0, 0);
    }

    #endregion
}
