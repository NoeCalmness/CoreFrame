/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Description
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.0
 * Created:  2017-
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    public float moveSpeed   = 0.1f;
    public float rotateSpeed = 0.5f;
    public float zoomSpeed   = 0.5f;

    public bool updateInput = true;
    public bool lockMovement = false;

    private Vector3 m_targetEuler, m_eulerVel;
    private float m_erlerDamp = 0.0f;

    private Dictionary<KeyCode, bool> m_keyState = new Dictionary<KeyCode, bool>();
    private Dictionary<string, float> m_axisState = new Dictionary<string, float>();
    private Dictionary<int, bool> m_mouseState = new Dictionary<int, bool>();

    private Camera m_camera;
    private Transform m_t;

    public void SetCurrent(Vector3 position, Vector3 euler)
    {
        transform.position = position;
        transform.eulerAngles = euler;
        m_targetEuler = euler;
    }

    public void SimulateInput(System.Action<Dictionary<KeyCode, bool>, Dictionary<string, float>, Dictionary<int, bool>> setter = null)
    {
        setter?.Invoke(m_keyState, m_axisState, m_mouseState);
    }

    private void Start()
    {
        m_t = transform;
        m_camera = GetComponent<Camera>();
        m_targetEuler = m_t.eulerAngles;
    }

    private void LateUpdate()
    {
        if (updateInput)
        {
            m_keyState.Clear();
            m_axisState.Clear();
            m_mouseState.Clear();

            SetInputState();
            ParseInput(Time.smoothDeltaTime, Time.timeScale);
        }

        if (m_camera && Camera_Combat.current && Camera_Combat.current.transform != m_t) Camera_Combat.ForceMaskRepaint(m_camera, Layers.INVISIBLE);
    }

    private void SetInputState()
    {
        m_keyState.Set(KeyCode.W, Input.GetKey(KeyCode.W));
        m_keyState.Set(KeyCode.S, Input.GetKey(KeyCode.S));
        m_keyState.Set(KeyCode.A, Input.GetKey(KeyCode.A));
        m_keyState.Set(KeyCode.D, Input.GetKey(KeyCode.D));
        m_keyState.Set(KeyCode.Q, Input.GetKey(KeyCode.Q));
        m_keyState.Set(KeyCode.E, Input.GetKey(KeyCode.E));

        m_axisState.Set("x", Input.GetAxis("x"));
        m_axisState.Set("y", Input.GetAxis("y"));
        m_axisState.Set("ScrollWheel", Input.GetAxis("ScrollWheel"));

        m_mouseState.Set(1, Input.GetMouseButton(1));
        m_mouseState.Set(2, Input.GetMouseButton(2));
    }

    public void ParseInput(float delta, float scale = 1.0f)
    {
        if (!lockMovement || m_mouseState.Get(1))
        {
            if (m_keyState.Get(KeyCode.W)) transform.Translate(Vector3.forward * moveSpeed);
            if (m_keyState.Get(KeyCode.S)) transform.Translate(Vector3.back    * moveSpeed);
            if (m_keyState.Get(KeyCode.A)) transform.Translate(Vector3.left    * moveSpeed);
            if (m_keyState.Get(KeyCode.D)) transform.Translate(Vector3.right   * moveSpeed);
            if (m_keyState.Get(KeyCode.Q)) transform.Translate(Vector3.down    * moveSpeed);
            if (m_keyState.Get(KeyCode.E)) transform.Translate(Vector3.up      * moveSpeed);
        }

        var deltaX = m_axisState.Get("x");
        var deltaY = m_axisState.Get("y");
        var deltaZ = m_axisState.Get("z");

        if (m_mouseState.Get(0) && Mathf.Abs(deltaZ) > 0.01f || m_mouseState.Get(1) && (Mathf.Abs(deltaX) > 0.01f || Mathf.Abs(deltaY) > 0.01f))
        {
            m_eulerVel = transform.eulerAngles;
            m_targetEuler = m_eulerVel + new Vector3(-deltaY * rotateSpeed, deltaX * rotateSpeed, deltaZ * rotateSpeed);
            m_erlerDamp = 0.0f;
        }

        if (m_mouseState.Get(2))
        {
            transform.Translate(Vector3.left * deltaX * 0.05f * zoomSpeed);
            transform.Translate(Vector3.down * deltaY * 0.05f * zoomSpeed);
        }
        else
        {

            var zoom = m_axisState.Get("ScrollWheel");
            if (zoom != 0)
                transform.Translate(Vector3.forward * zoom * zoomSpeed);
        }

        if (Vector3.Distance(transform.eulerAngles, m_targetEuler) > 0.01f)
        {
            m_erlerDamp += delta / scale;
            transform.eulerAngles = Vector3.Lerp(m_eulerVel, m_targetEuler, m_erlerDamp / 0.1f);
        }
    }
}
