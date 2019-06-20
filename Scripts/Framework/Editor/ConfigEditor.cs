/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Custom config editor
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-07-03
 * 
 ***************************************************************************************************/

using System;
using UnityEditor;
using Object = UnityEngine.Object;

[CustomEditor(typeof(Config), true)]
[CanEditMultipleObjects]
public class ConfigEditor : Editor
{
    private static Action m_onForceRepaint = null;
    private static bool m_initialized = false;
    private static Object m_delaySelect = null;
    private static int m_delayFrame = 0;

    private static Type[] m_forceRepaintTargets = null;
    private static bool ForceRepaintTarget(Type type)
    {
        if (m_forceRepaintTargets == null)
        {
            m_forceRepaintTargets = new Type[]
            {
                typeof(CameraShotInfo),
            };
        }
        return m_forceRepaintTargets.Contains(type);
    }

    public static void ForceRepaint()
    {
        Initialize();
        m_onForceRepaint?.Invoke();
    }

    private static void Initialize()
    {
        if (m_initialized) return;

        m_initialized = true;
        SceneView.onSceneGUIDelegate -= OnGlobalSceneGUI;
        SceneView.onSceneGUIDelegate += OnGlobalSceneGUI;
    }

    private static void OnGlobalSceneGUI(SceneView view)
    {
        if (m_delaySelect && !Selection.activeObject && --m_delayFrame < 0)
        {
            Selection.activeObject = m_delaySelect;
            m_delaySelect = null;
            m_delayFrame = 2;
        }
    }

    private void OnEnable()
    {
        m_onForceRepaint -= OnForceRepaint;
        m_onForceRepaint += OnForceRepaint;
    }

    private void OnDisable()
    {
        m_onForceRepaint -= OnForceRepaint;
    }

    private void OnForceRepaint()
    {
        if (Selection.activeObject != serializedObject.targetObject) return;

        var config = serializedObject.targetObject as Config;
        if (!ForceRepaintTarget(config.itemType)) return;

        m_delaySelect = serializedObject.targetObject;
        m_delayFrame = 2;
        Selection.activeObject = null;

        EditorWindow.FocusWindowIfItsOpen<SceneView>();
    }

    public override void OnInspectorGUI()
    {
        var config = serializedObject.targetObject as Config;
        EditorGUILayout.LabelField(config.itemType.Name, EditorStyles.boldLabel);

        base.OnInspectorGUI();
    }
}
