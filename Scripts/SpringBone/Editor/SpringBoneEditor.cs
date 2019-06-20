/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * SpringBone component editor class.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-12-13
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(SpringBone))]
[CanEditMultipleObjects]
public class SpringBoneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var bone = (SpringBone)target;
        var manager = SpringManagerEditor.GetCurrentSpringManager(bone.transform);

        if (!manager)
        {
            SpringManagerEditor.DrawErrorMessage("错误：从当前节点或其任何父节点都找不到 SpringManager。请先添加 SpringMnager 脚本到任意父节点。", 15);
            return;
        }

        EditorGUILayout.Space();

        SpringManagerEditor.DrawWeaponName(manager.currentWeapon, 5);
        SpringManagerEditor.DrawLine(7);

        Undo.RecordObject(bone, "Edit Spring Bone");

        var excluded = bone.IsWeaponExcluded(manager.currentWeapon);

        if (GUILayout.Button("删除所有武器设置", GUI.skin.customStyles[235]))
            bone.RemoveWeaponParam();

        excluded = GUILayout.Toggle(excluded, "从当前武器排除", GUI.skin.customStyles[235]);
        bone.ExculeWeapon(manager.currentWeapon, excluded);

        if (EditorApplication.isPlaying) bone.enabled = !excluded;

        EditorGUI.BeginDisabledGroup(excluded);
        {
            var menu = GetWeaponsMenu(bone, manager.currentWeapon);
            GUI.enabled = !excluded && menu != null;
            if (GUILayout.Button("从已有武器复制设置", GUI.skin.customStyles[241]))
                menu.ShowAsContext();
            GUI.enabled = !excluded;

            SpringManagerEditor.DrawLine(7, 2);

            serializedObject.Update();
            SerializedProperty child = serializedObject.FindProperty("child");
            EditorGUILayout.PropertyField(child, true, null);
            serializedObject.ApplyModifiedProperties();

            bone.boneAxis = EditorGUILayout.Vector3Field("boneAxis", bone.boneAxis);

            SpringManagerEditor.DrawLine(7, 2);

            var param = bone.GetWeaponParam(manager.currentWeapon, true);

            param.radius         = EditorGUILayout.FloatField("Radius", param.radius);
            param.stiffnessForce = EditorGUILayout.FloatField("Stiffness Force", param.stiffnessForce);
            param.dragForce      = EditorGUILayout.FloatField("Drag Force", param.dragForce);
            param.threshold      = EditorGUILayout.FloatField("Threshold", param.threshold);

            EditorGUILayout.Space();

            param.hasChestCollider = EditorGUILayout.Toggle("Chest Collider", param.hasChestCollider);
            param.hasWaistCollider = EditorGUILayout.Toggle("Waist Collider", param.hasWaistCollider);

            serializedObject.Update();
            var colliders = GetCurrentParam(manager.currentWeapon);
            EditorGUILayout.PropertyField(colliders, true, null);
            serializedObject.ApplyModifiedProperties();

            EditorGUI.EndDisabledGroup();
        }

        if (GUI.changed)
        {
            bone.UpdateParams(manager.currentWeapon, manager.chest, manager.waist);

            if (!EditorApplication.isPlaying)
            {
                EditorUtility.SetDirty(bone);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }

    private GenericMenu GetWeaponsMenu(SpringBone bone, int current)
    {
        if (!bone) return null;

        var ids = bone.GetSavedWeaponIDs();

        if (ids.Count < 1) return null;

        var menu = new GenericMenu();
        foreach (var id in ids)
            menu.AddItem(new GUIContent(SpringManagerEditor.GetWeaponString(id)), current == id, i => bone.CopyParams((int)i, current), id);

        return menu;
    }

    private SerializedProperty GetCurrentParam(int weaponID)
    {
        var ws = serializedObject.FindProperty("m_weaponParams");
        for (var i = 0; i < ws.arraySize; ++i)
        {
            var element = ws.GetArrayElementAtIndex(i);
            if (element.FindPropertyRelative("weaponID").intValue == weaponID) return element.FindPropertyRelative("colliders");
        }
        return null;
    }
}
