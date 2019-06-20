/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * SpringCollier component editor class.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-12-13
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(SpringCollider))]
[CanEditMultipleObjects]
public class SpringColliderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var collider = (SpringCollider)target;
        var manager = SpringManagerEditor.GetCurrentSpringManager(collider.transform);

        if (!manager)
        {
            SpringManagerEditor.DrawErrorMessage("错误：从当前节点或其任何父节点都找不到 SpringManager。请先添加 SpringMnager 脚本到任意父节点。", 15);
            return;
        }

        EditorGUILayout.Space();

        SpringManagerEditor.DrawWeaponName(manager.currentWeapon, 5);
        SpringManagerEditor.DrawLine(5);

        Undo.RecordObject(collider, "Edit Spring Collider");

        if (GUILayout.Button("删除所有武器设置", GUI.skin.customStyles[235]))
            collider.RemoveWeaponParam();

        SpringManagerEditor.DrawLine(5, 2);

        var param = collider.GetWeaponParam(manager.currentWeapon, true);
        param.radius = EditorGUILayout.FloatField("Radius", param.radius);

        if (GUI.changed)
        {
            collider.UpdateParams(manager.currentWeapon);

            if (!EditorApplication.isPlaying)
            {
                EditorUtility.SetDirty(collider);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }
}
