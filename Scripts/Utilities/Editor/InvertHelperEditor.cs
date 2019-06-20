/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * InvertHelper component editor class.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-04-03
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(InvertHelper))]
[CanEditMultipleObjects]
public class InvertHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Rim Light Settings", GUI.skin.customStyles[429]);

        EditorGUI.BeginChangeCheck();

        InvertHelper.rimColors[1] = EditorGUILayout.ColorField("Invincible Color", InvertHelper.rimColors[1]);
        InvertHelper.rimIntensity[1] = EditorGUILayout.Slider("Invincible Intensity",  InvertHelper.rimIntensity[1], 0, 5.0f);

        GUILayout.Space(5);

        InvertHelper.rimColors[2]  = EditorGUILayout.ColorField("Tough Color", InvertHelper.rimColors[2]);
        InvertHelper.rimIntensity[2] = EditorGUILayout.Slider("Tough Intensity", InvertHelper.rimIntensity[2], 0, 5.0f);

        GUILayout.Space(5);

        InvertHelper.rimColors[3] = EditorGUILayout.ColorField("Both Color", InvertHelper.rimColors[3]);
        InvertHelper.rimIntensity[3] = EditorGUILayout.Slider("Both Intensity", InvertHelper.rimIntensity[3], 0, 5.0f);

        if (EditorGUI.EndChangeCheck())
            InvertHelper.UpdateRimSettings();
    }
}
