/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Custom get/set serialize field
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-21
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SetAttribute))]
sealed class SetDrawer : PropertyDrawer
{
    private static GUIContent[] m_xyLabels = new GUIContent[]
    {
        new GUIContent("X"),
        new GUIContent("Y"),
    };

    private static GUIContent[] m_xyzLabels = new GUIContent[]
    {
        new GUIContent("X"),
        new GUIContent("Y"),
        new GUIContent("Z"),
    };

    private static GUIContent[] m_xyzwLabels = new GUIContent[]
    {
        new GUIContent("X"),
        new GUIContent("Y"),
        new GUIContent("Z"),
        new GUIContent("W"),
    };

    private static double[] m_vector2_ = new double[2];
    private static double[] m_vector3_ = new double[3];
    private static double[] m_vector4_ = new double[4];

    private int  m_foldout = -1;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var o = property.serializedObject.targetObject;
        if (EditorPrefs.GetBool(o.GetType().Name + ":" + property.name))
        {
            var lh = 16.0f;

            if (fieldInfo.FieldType == typeof(Vector2_)) return lh * 3 + 2 * 1;
            if (fieldInfo.FieldType == typeof(Vector3_)) return lh * 4 + 2 * 2;
            if (fieldInfo.FieldType == typeof(Vector4_)) return lh * 5 + 2 * 3;
        }

        return base.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (m_foldout < 0)
            m_foldout = fieldInfo.FieldType == typeof(Vector2_) || fieldInfo.FieldType == typeof(Vector3_) || fieldInfo.FieldType == typeof(Vector4_) ? 1 : 0;

        var att = (SetAttribute)attribute;

        position.height = 16;

        var o = property.serializedObject.targetObject;
        if (m_foldout == 1)
        {
            var toggle = EditorPrefs.GetBool(o.GetType().Name + ":" + property.name);
            toggle = EditorGUI.Foldout(position, toggle, label, true);
            EditorPrefs.SetBool(o.GetType().Name + ":" + property.name, toggle);

            if (!toggle) return;
        }

        var p = EditorUtil.GetParentObject(property.propertyPath, o);
        var f = EditorUtil.GetFieldInfoHierarchy(p.GetType(), property.name);
        var ov = f.GetValue(p);

        EditorGUI.BeginChangeCheck();

        object nv = null;

        if      (fieldInfo.FieldType == typeof(Vector2_)) nv = DrawVector2_(position, property, label);
        else if (fieldInfo.FieldType == typeof(Vector3_)) nv = DrawVector3_(position, property, label);
        else if (fieldInfo.FieldType == typeof(Vector4_)) nv = DrawVector4_(position, property, label);
        else EditorGUI.PropertyField(position, property, label);

        var changed = EditorGUI.EndChangeCheck();

        if (changed)
        {
            if (nv == null)
            {
                property.serializedObject.ApplyModifiedProperties();
                nv = f.GetValue(p);
            }
            var type   = p.GetType();
            var info   = type.GetProperty(att.name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty);
            var objs   = property.serializedObject.targetObjects;

            if (info == null)
            {
                Logger.LogError("Invalid property name {0} in {1}", att.name, type);
                foreach (var obj in objs) f.SetValue(obj, nv);
            }
            else
            {
                foreach (var obj in objs)
                {
                    p = EditorUtil.GetParentObject(property.propertyPath, obj);
                    f.SetValue(p, ov);
                    info.SetValue(p, nv, null);
                }
            }
        }
    }

    private Vector2_ DrawVector2_(Rect position, SerializedProperty property, GUIContent label)
    {
        var indent = EditorGUI.indentLevel;

        position.y += 16;
        position.height = 16f;

        var vec2 = (Vector2_)fieldInfo.GetValue(property.serializedObject.targetObject);

        m_vector2_[0] = vec2.x;
        m_vector2_[1] = vec2.y;

        MultiDoubleField(position, m_xyLabels, m_vector2_, 1);

        vec2.Set(m_vector2_[0], m_vector2_[1]);

        return vec2;
    }

    private Vector3_ DrawVector3_(Rect position, SerializedProperty property, GUIContent label)
    {
        var indent = EditorGUI.indentLevel;

        position.y += 16;
        position.height = 16f;

        var vec3 = (Vector3_)fieldInfo.GetValue(property.serializedObject.targetObject);

        m_vector3_[0] = vec3.x;
        m_vector3_[1] = vec3.y;
        m_vector3_[2] = vec3.z;

        MultiDoubleField(position, m_xyzLabels, m_vector3_, 1);

        vec3.Set(m_vector3_[0], m_vector3_[1], m_vector3_[2]);

        return vec3;
    }

    private Vector4_ DrawVector4_(Rect position, SerializedProperty property, GUIContent label)
    {
        var indent = EditorGUI.indentLevel;

        position.y += 16;
        position.height = 16f;

        var vec4 = (Vector4_)fieldInfo.GetValue(property.serializedObject.targetObject);

        m_vector4_[0] = vec4.x;
        m_vector4_[1] = vec4.y;
        m_vector4_[2] = vec4.z;
        m_vector4_[3] = vec4.w;

        MultiDoubleField(position, m_xyzwLabels, m_vector4_, 1);

        vec4.Set(m_vector4_[0], m_vector4_[1], m_vector4_[2], m_vector4_[3]);

        return vec4;
    }

    private static void MultiDoubleField(Rect position, GUIContent[] subLabels, double[] values, int indent = 0)
    {
        var count = values.Length;

        var _indext = EditorGUI.indentLevel;
        var lw = EditorGUIUtility.labelWidth;

        EditorGUIUtility.labelWidth = 35;
        EditorGUI.indentLevel = indent;

        for (int i = 0; i < values.Length; i++)
        {
            values[i] = EditorGUI.DoubleField(position, subLabels[i], values[i]);
            position.y += 16f + 2f;
        }

        EditorGUIUtility.labelWidth = lw;
        EditorGUI.indentLevel = _indext;
    }
}