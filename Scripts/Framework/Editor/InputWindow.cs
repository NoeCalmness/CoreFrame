/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Global Input window to get input data from a popup window.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-04-10
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

public class InputWindow : EditorWindow
{
    public string data = "";

    private bool m_destroyed = false;

    public string desc
    {
        get { return m_desc; }
        set
        {
            m_desc = value;
            if (!string.IsNullOrEmpty(m_desc)) minSize = maxSize = new Vector2(600, 80);
        }
    }
    private string m_desc = "";

    public UnityAction<string> onInput = null;
    public System.Func<string, bool> onValidate = null;

    public InputWindow Input(UnityAction<string> _onInput)
    {
        onInput += _onInput;
        return this;
    }

    public InputWindow Validate(System.Func<string, bool> _onValidate)
    {
        onValidate += _onValidate;
        return this;
    }

    public InputWindow SetHandlers(UnityAction<string> _onInput, System.Func<string, bool> _onValidate)
    {
        onInput += _onInput;
        onValidate += _onValidate;

        return this;
    }

    private void Awake()
    {
        minSize = maxSize = new Vector2(600, 40);
    }

    void OnGUI()
    {
        if (!string.IsNullOrEmpty(m_desc))
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(m_desc, GUI.skin.customStyles[20]);
            EditorGUILayout.Space();
        }

        data = EditorGUILayout.TextField("", data);

        if (GUILayout.Button("OK"))
        {
            if (onValidate != null)
            {
                if (onValidate.Invoke(data))
                    Close();
            }
            else if (onInput != null)
            {
                onInput.Invoke(data);
                Close();
            }
            else Close();
        }
    }

    private void OnLostFocus()
    {
        Focus();
    }

    private void OnDestroy()
    {
        if (m_destroyed) return;
        m_destroyed = true;

        onInput = null;
    }
}
