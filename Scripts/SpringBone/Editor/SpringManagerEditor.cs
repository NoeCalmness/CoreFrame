/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * SpringManager component editor class.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-12-13
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;

[CustomEditor(typeof(SpringManager))]
public class SpringManagerEditor : Editor
{
    #region Static functions

    public static GUIStyle styleError
    {
        get
        {
            if (m_styleError != null) return m_styleError;
            m_styleError = new GUIStyle(GUI.skin.customStyles[73]);

            return m_styleError;
        }
    }
    private static GUIStyle m_styleError;

    public static GUIStyle styleWeaponName
    {
        get
        {
            if (m_styleWeaponName != null) return m_styleWeaponName;
            m_styleWeaponName = new GUIStyle(GUI.skin.customStyles[533]);
            m_styleWeaponName.normal.textColor = Color.green;
            m_styleWeaponName.richText = true;
            m_styleWeaponName.fontSize = 17;

            return m_styleWeaponName;
        }
    }
    private static GUIStyle m_styleWeaponName;

    public static GUIStyle styleLine
    {
        get
        {
            if (m_styleLine != null) return m_styleLine;
            m_styleLine = new GUIStyle(GUI.skin.customStyles[266]);
            m_styleLine.stretchHeight = false;
            m_styleLine.fixedHeight = 1;

            return m_styleLine;
        }
    }
    private static GUIStyle m_styleLine;

    public static WeaponInfo GetWeaponInfo(int weaponID)
    {
        if (m_weapons == null) m_weapons = AssetDatabase.LoadAssetAtPath<WeaponInfos>("Assets/Data/config_weaponinfos.asset");
        if (m_weapons == null) return null;

        return m_weapons.FindItem(weaponID);
    }

    public static string GetWeaponString(int weaponID)
    {
        var wi = GetWeaponInfo(weaponID);
        return GetWeaponString(weaponID , wi ? wi.name : "Unknow");
    }

    public static string GetWeaponString(int weaponID, string name)
    {
        return "武器：[" + weaponID + "," + name + "]";
    }

    public static SpringManager GetCurrentSpringManager(Transform node)
    {
        if (!node) return null;
        var manager = node.GetComponentsInParent<SpringManager>(true);
        return manager.Length < 1 ? null : manager[0];
    }

    public static WeaponInfos m_weapons = null;

    public static void DrawErrorMessage(string message, int nextSpace = 0)
    {
        EditorGUILayout.BeginVertical(GUI.skin.customStyles[39]);
        EditorGUILayout.LabelField(message, styleError);
        EditorGUILayout.EndVertical();

        GUILayout.Space(nextSpace);
    }

    public static void DrawWeaponName(int weaponID, int nextSpace = 0)
    {
        EditorGUILayout.LabelField("<b>" + GetWeaponString(weaponID) + "</b>", styleWeaponName, GUILayout.Height(32));

        GUILayout.Space(nextSpace);
    }

    public static void DrawLine(int nextSpace = 0, int prevSpace = 0)
    {
        GUILayout.Space(prevSpace);
        EditorGUILayout.LabelField("", styleLine, GUILayout.Height(1));
        GUILayout.Space(nextSpace);
    }

    #endregion

    public override void OnInspectorGUI()
    {
        var manager = (SpringManager)target;

        EditorGUILayout.Space();

        DrawWeaponName(manager.currentWeapon, 5);
        DrawLine(7);

        Undo.RecordObject(manager, "Edit Spring Manager");

        if (GUILayout.Button("删除所有武器设置", GUI.skin.customStyles[235]))
            manager.RemoveWeaponParam();

        var menu = GetWeaponListMenu(manager, manager.currentWeapon);
        GUI.enabled = menu != null;
        if (GUILayout.Button("选择当前武器", GUI.skin.customStyles[241]))
            menu.ShowAsContext();
        GUI.enabled = true;

        menu = GetWeaponsMenu(manager, manager.currentWeapon);
        GUI.enabled = menu != null;
        if (GUILayout.Button("从已有武器复制设置", GUI.skin.customStyles[241]))
            menu.ShowAsContext();
        GUI.enabled = true;

        DrawLine(7, 2);

        manager.simulateWind  = EditorGUILayout.ToggleLeft("Simulate Wind", manager.simulateWind);
        manager.drawBones     = EditorGUILayout.ToggleLeft("Draw Bones", manager.drawBones);
        manager.drawColliders = EditorGUILayout.ToggleLeft("Draw Colliders", manager.drawColliders);
        manager.springForce   = EditorGUILayout.Vector3Field("Spring Force", manager.springForce);

        DrawLine(7, 2);

        var param = manager.GetWeaponParam(manager.currentWeapon, true);

        param.dynamicRatio = EditorGUILayout.Slider("Dynamic Ratio", param.dynamicRatio, 0, 1);
        param.windStrength = EditorGUILayout.Vector3Field("Wind Strength", param.windStrength);

        if (GUI.changed)
        {
            if (EditorApplication.isPlaying) manager.UpdateParams(manager.currentWeapon);
            else
            {
                EditorUtility.SetDirty(manager);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }

    private GenericMenu GetWeaponsMenu(SpringManager manager, int current)
    {
        if (!manager) return null;

        var ids = manager.GetSavedWeaponIDs();

        if (ids.Count < 1) return null;

        var menu = new GenericMenu();
        foreach (var id in ids)
            menu.AddItem(new GUIContent(GetWeaponString(id)), current == id, i => manager.CopyParams((int)i, current), id);

        return menu;
    }

    private GenericMenu GetWeaponListMenu(SpringManager manager, int current)
    {
        GetWeaponInfo(0);

        var ws = m_weapons.GetItems();

        if (ws.Count < 1) return null;

        var menu = new GenericMenu();
        foreach (var w in ws)
        {
            if (w.ID >= Creature.MAX_WEAPON_ID) continue;

            menu.AddItem(new GUIContent(GetWeaponString(w.ID, w.name)), current == w.ID, i =>
            {
                manager.currentWeapon = (int)i;
                if (!EditorApplication.isPlaying) manager.Refresh();
                InternalEditorUtility.RepaintAllViews(); }, w.ID);
        }
        return menu;
    }
}
