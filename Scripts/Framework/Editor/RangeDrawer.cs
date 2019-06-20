/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Custom range get/set serialize field
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-01-19
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(_RangeAttribute))]
sealed class RangeDrawer : PropertyDrawer
{
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
        var att = (_RangeAttribute)attribute;

        position.height = 16;

        EditorGUI.BeginChangeCheck();

        object value = null;

        if (property.propertyType == SerializedPropertyType.Float) value = EditorGUI.Slider(position, label, property.floatValue, att.min, att.max);
        else if (property.propertyType == SerializedPropertyType.Integer) value = EditorGUI.IntSlider(position, label, property.intValue, (int)att.min, (int)att.max);
        else EditorGUI.LabelField(position, label.text, "Use Range with float or int.");

        var changed = EditorGUI.EndChangeCheck();
        if (changed && value == null)
        {
            att.dirty = true;
            return;
        }

        if (changed && value != null || !changed && att.dirty)
        {
            var parent = EditorUtil.GetParentObject(property.propertyPath, property.serializedObject.targetObject);

            if (!string.IsNullOrEmpty(att.name) && !string.IsNullOrWhiteSpace(att.name))
            {
                var type = parent.GetType();
                var info = type.GetProperty(att.name);

                if (info == null)
                    Logger.LogError("Invalid property name {0} in {1}", att.name, type);
                else
                    info.SetValue(parent, value ?? fieldInfo.GetValue(parent), null);
            }

            att.dirty = false;
        }
    }
}