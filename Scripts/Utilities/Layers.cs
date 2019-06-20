/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Layer class.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-05
 * 
 ***************************************************************************************************/

using UnityEngine;

/// <summary>
/// Manager all layers
/// </summary>
public static class Layers
{
    #region Layers

    /// <summary>
    /// Default layer
    /// "Default"
    /// </summary>
    public const int DEFAULT      = 0;
    /// <summary>
    /// Used for UI
    /// "UI"
    /// </summary>
    public const int UI           = 5;
    /// <summary>
    /// Used for all creature models
    /// "Model"
    /// </summary>
    public const int MODEL        = 8;
    /// <summary>
    /// Used for all main hand weapons
    /// "Weapon"
    /// </summary>
    public const int WEAPON       = 9;
    /// <summary>
    /// Used for all off hand weapons
    /// "OffWeapon"
    /// </summary>
    public const int WEAPON_OFF   = 10;
    /// <summary>
    /// Used for all jewelry
    /// "Jewelry"
    /// </summary>
    public const int JEWELRY      = 11;
    /// <summary>
    /// Used for all attack/hit effects
    /// "Effect"
    /// </summary>
    public const int EFFECT       = 12;
    /// <summary>
    /// Used for all buff effects
    /// "EffectBuff"
    /// </summary>
    public const int EFFECT_BUFF  = 13;
    /// <summary>
    /// Used for ground
    /// "Ground"
    /// </summary>
    public const int GROUND       = 14;
    /// <summary>
    /// Used for level bgm trigger
    /// "BGMTrigger"
    /// </summary>
    public const int BGM_TRIGGER  = 15;
    /// <summary>
    /// Used for level bgm listener
    /// "BGMListener"
    /// </summary>
    public const int BGM_LISTENER = 16;
    /// <summary>
    /// Used for theary dialog
    /// "Dialog"
    /// </summary>
    public const int Dialog = 17;
    /// <summary>
    /// Used for all pet node
    /// "PetNode"
    /// </summary>
    public const int PetNode = 18;
    /// <summary>
    /// Used for awake model
    /// "Awake"
    /// </summary>
    public const int Awake = 19;
    /// <summary>
    /// Used for simple creatures (Like UI)
    /// "Character"
    /// </summary>
    public const int CHARACTER    = 20;
    /// <summary>
    /// Used for invisible targets
    /// "Invisible"
    /// </summary>
    public const int INVISIBLE    = 21;
    /// <summary>
    /// Dark mask
    /// "DarkMask"
    /// </summary>
    public const int DarkMask     = 22;
    /// <summary>
    /// 器灵模型
    /// </summary>
    public const int Soul         = 23;

    /// <summary>
    /// 普通斗技场景
    /// </summary>
    public const int PvpNode      = 24;

    /// <summary>
    /// 皇家斗技场景
    /// </summary>
    public const int RankNode     = 25;

#if UNITY_EDITOR
    /// <summary>
    /// Editor only, used for custom editor window preview
    /// "Preview"
    /// </summary>
    public const int PREVIEW      = 26;
#endif

    /// <summary>
    /// 主场景
    /// </summary>
    public const int MainNode     = 27;

    /// <summary>
    /// npc星魂场景
    /// </summary>
    public const int NpcAwakeNode = 28;

    /// <summary>
    /// 动态加载的场景
    /// </summary>
    public const int SceneObject = 29;

    public const int T4M = 30;

    /// <summary>
    /// 约会界面视差滚动
    /// </summary>
    public const int Dating = 31;

    #endregion

    #region Layer masks

    /// <summary>
    /// Default layer
    /// "Default"
    /// </summary>
    public const int MASK_DEFAULT      = 1 << DEFAULT;
    /// <summary>
    /// Used for UI
    /// "UI"
    /// </summary>
    public const int MASK_UI           = 1 << UI;
    /// <summary>
    /// Used for all creature models
    /// "Model"
    /// </summary>
    public const int MASK_MODEL        = 1 << MODEL;
    /// <summary>
    /// Used for all main hand weapons
    /// "Weapon"
    /// </summary>
    public const int MASK_WEAPON       = 1 << WEAPON;
    /// <summary>
    /// Used for all off hand weapons
    /// "OffWeapon"
    /// </summary>
    public const int MASK_WEAPON_OFF   = 1 << WEAPON_OFF;
    /// <summary>
    /// Used for all jewelry
    /// "Jewelry"
    /// </summary>
    public const int MASK_JEWELRY      = 1 << JEWELRY;
    /// <summary>
    /// Used for all attack/hit effects
    /// "Effect"
    /// </summary>
    public const int MASK_EFFECT       = 1 << EFFECT;
    /// <summary>
    /// Used for all buff effects
    /// "EffectBuff"
    /// </summary>
    public const int MASK_EFFECT_BUFF  = 1 << EFFECT_BUFF;
    /// <summary>
    /// Used for ground
    /// "Ground"
    /// </summary>
    public const int MASK_GROUND       = 1 << GROUND;
    /// <summary>
    /// Used for level bgm trigger
    /// "BGMTrigger"
    /// </summary>
    public const int MASK_BGM_TRIGGER  = 1 << BGM_TRIGGER;
    /// <summary>
    /// Used for level bgm listener
    /// "BGMListener"
    /// </summary>
    public const int MASK_BGM_LISTENER = 1 << BGM_LISTENER;
    /// <summary>
    /// Used for simple creatures (Like UI)
    /// "Character"
    /// </summary>
    public const int MASK_CHARACTER    = 1 << CHARACTER;
    /// <summary>
    /// Used for invisible targets
    /// "Invisible"
    /// </summary>
    public const int MASK_INVISIBLE    = 1 << INVISIBLE;
    /// <summary>
    /// Used for dark mask
    /// "DarkMask"
    /// </summary>
    public const int MASK_DARK_MASK    = 1 << DarkMask;

    /// <summary>
    /// Used for dark soul
    /// </summary>
    public const int MASK_SOUL = 1 << Soul;

    /// <summary>
    /// Used for dark pvpNode
    /// </summary>
    public const int MASK_PVPNODE = 1 << PvpNode;

    /// <summary>
    /// Used for dark rankNode
    /// </summary>
    public const int MASK_RANKNODE = 1 << RankNode;

#if UNITY_EDITOR
    /// <summary>
    /// Editor only, used for custom editor window preview
    /// "Preview"
    /// </summary>
    public const int MASK_PREVIEW      = 1 << PREVIEW;
#endif

    /// <summary>
    /// use for mainNode mask
    /// </summary>
    public const int MASK_MAINNODE     = 1 << MainNode;

    /// <summary>
    /// Used for Dating object
    /// </summary>
    public const int MASK_DATING       = 1 << Dating;

    #endregion

    #region Sorting layers

    /// <summary>
    /// Default sorting layer
    /// </summary>
    public const string SORTING_DEFAULT  = "Default";
    /// <summary>
    /// Top most sorting layer
    /// </summary>
    public const string SORTING_TOP_MOST = "TopMost";

    #endregion

    #region Helpers

    public static int LayerToMask(params int[] layers)
    {
        var mask = 0;
        foreach (var layer in layers) mask |= 1 << layer;
        return mask;
    }

    public static int LayerToMask(params string[] layers)
    {
        return LayerMask.GetMask(layers);
    }

    public static string LayerToName(int layer)
    {
        return LayerMask.LayerToName(layer);
    }

    public static int NameToLayer(string name)
    {
        return LayerMask.NameToLayer(name);
    }

    public static int NameToSortingLayer(string name)
    {
        return SortingLayer.NameToID(name);
    }

    public static string SortingLayerToName(int layer)
    {
        return SortingLayer.IDToName(layer);
    }

    #endregion
}
