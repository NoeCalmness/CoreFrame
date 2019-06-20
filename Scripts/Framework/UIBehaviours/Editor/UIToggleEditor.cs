/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * UIToggle editor
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-02-11
 * 
 ***************************************************************************************************/

namespace UnityEditor.UI
{
    [CustomEditor(typeof(UIToggle), true)]
    [CanEditMultipleObjects]
    public class UIToggleEditor : SelectableEditor
    {
        SerializedProperty m_OnValueChangedProperty;
		SerializedProperty m_OnValueChangedInvertProperty;
        SerializedProperty m_TransitionProperty;
        SerializedProperty m_GraphicProperty;
        SerializedProperty m_GroupProperty;
        SerializedProperty m_IsOnProperty;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_TransitionProperty = serializedObject.FindProperty("toggleTransition");
            m_GraphicProperty = serializedObject.FindProperty("graphic");
            m_GroupProperty = serializedObject.FindProperty("m_Group");
            m_IsOnProperty = serializedObject.FindProperty("m_IsOn");
            m_OnValueChangedProperty = serializedObject.FindProperty("onValueChanged");
			m_OnValueChangedInvertProperty = serializedObject.FindProperty("onValueChangedInvert");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_IsOnProperty);
            EditorGUILayout.PropertyField(m_TransitionProperty);
            EditorGUILayout.PropertyField(m_GraphicProperty);
            EditorGUILayout.PropertyField(m_GroupProperty);

            EditorGUILayout.Space();

            // Draw the event notification options
            EditorGUILayout.PropertyField(m_OnValueChangedProperty);
			EditorGUILayout.PropertyField(m_OnValueChangedInvertProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}