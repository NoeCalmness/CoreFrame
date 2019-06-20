/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Script for ui selectable component key event listening.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-21
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[AddComponentMenu("HYLR/UI/Input Listener")]
public class UIInputListener : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public class KeyEvent
    {
        private class _KeyEvent : UnityEvent<KeyCode> { public _KeyEvent() { } }
        private Dictionary<KeyCode, _KeyEvent> m_events = new Dictionary<KeyCode, _KeyEvent>();

        public KeyEvent() { }

        public void AddListener(KeyCode key, UnityAction<KeyCode> handler)
        {
            var e = m_events.GetDefault(key);
            e.AddListener(handler);
        }

        public void RemoveListener(KeyCode key, UnityAction<KeyCode> handler)
        {
            var e = m_events.Get(key);
            if (e == null) return;
            e.RemoveListener(handler);
        }

        public void RemoveAllListener(KeyCode key)
        {
            var e = m_events.Get(key);
            if (e == null) return;
            e.RemoveAllListeners();
        }

        public void RemoveAllListener()
        {
            m_events.Clear();
        }

        public void Invoke()
        {
            if (m_events.Count < 1) return;

            var keys = m_events.Keys;
            foreach (var key in keys)
            {
                if (!Input.GetKeyDown(key)) continue;
                var e = m_events.Get(key);
                if (e != null) e.Invoke(key);
            }
        }
    }

    public bool focused { get; protected set; }
    public KeyEvent keyEvent { get { return m_keyEvent; } }

    private KeyEvent m_keyEvent = new KeyEvent();

    private void Start()
	{
        focused = UIManager.instance.eventSystem.currentSelectedGameObject == gameObject;
	}

    // we always catch input events after update
    private void LateUpdate()
    {
        if (!focused) return;

        m_keyEvent.Invoke();
    }

    private void OnDestroy()
    {
        m_keyEvent.RemoveAllListener();
    }

    public void OnSelect(BaseEventData eventData)
    {
        focused = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        focused = false;
    }
}
