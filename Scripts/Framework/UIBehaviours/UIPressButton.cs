/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Script for ui press button.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-08
 * 
 ***************************************************************************************************/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[AddComponentMenu("HYLR/UI/Press Button")]
public class UIPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Serializable]
    public class ButtonPressedEvent : UnityEvent<bool> { public ButtonPressedEvent() { } }

    public float pressDelay = 0.5f;

    public ButtonPressedEvent onPressed { get { return m_onPressed; } }
    public bool pressed { get; set; }

    private bool m_pointerDown = false;

    [SerializeField]
    private ButtonPressedEvent m_onPressed = new ButtonPressedEvent();

    public void OnPointerDown(PointerEventData eventData)
    {
        pressed = false;
        m_pointerDown = true;

        StartCoroutine(WaitForPressed());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_pointerDown = false;
        if (pressed)
        {
            pressed = false;
            onPressed.Invoke(false);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        m_pointerDown = false;
        if (pressed)
        {
            pressed = false;
            onPressed.Invoke(false);
        }
    }

    private IEnumerator WaitForPressed()
    {
        var wait = new WaitForSeconds(pressDelay);
        yield return wait;

        if (m_pointerDown)
        {
            pressed = true;
            onPressed.Invoke(true);
        }
    }

    #region Editor helper

    public bool e_Pressed     = false;
    public bool e_PointerDown = false;

#if UNITY_EDITOR
    private void LateUpdate()
    {
        e_Pressed     = pressed;
        e_PointerDown = m_pointerDown;
    }
#endif

    #endregion
}
