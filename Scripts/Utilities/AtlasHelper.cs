/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * UI atlas helper
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-04-10
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

public static class AtlasHelper
{
    #region Atlas names

    public static readonly string Shared          = "atlas_shared";
    public static readonly string Icons           = "atlas_icons";
    public static readonly string Rune            = "atlas_rune";
    public static readonly string Avatars         = "atlas_avatars";
    public static readonly string chargeLarge     = "atlas_chargelarge";
    public static readonly string Skillicon       = "atlas_skillicon";
    public static readonly string PetIcons        = "atlas_peticons";
    public static readonly string NpcDateInfo     = "atlas_npcdateinfo";
    public static readonly string HuaZha          = "atlas_huazha";
    public static readonly string PetMission      = "atlas_petmission";
    public static readonly string Puzzle          = "atlas_puzzle";

    #endregion

    #region Sprite names

    /// <summary>
    /// 货币图标名称
    /// </summary>
    public static readonly string[] icon_currencies = new string[] { "", "s_gold", "s_diamond", "s_hunteremblem", "", "", "s_praycoin", "s_praycoin" };

    #endregion


    #region PetMission atlas helper
    /// <summary>
    /// 设置图集 chargeLarge 的指定 Sprite 到目标对象
    /// 如果目标对象没有 Image 组件 将会自动创建一个
    /// </summary>
    /// <param name="o">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetPetMission(GameObject o, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(o, PetMission, sprite, onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置图集 chargeLarge 的指定 Sprite 到目标对象
    /// 如果目标对象没有 Image 组件 将会自动创建一个
    /// </summary>
    /// <param name="t">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetPetMission(Transform t, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(t, PetMission, sprite, onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置图集 chargeLarge 的指定 Sprite 到目标对象
    /// </summary>
    /// <param name="i">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetPetMission(Image i, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(i, PetMission, sprite, onComplete, useNativeSize);
    }
    #endregion

    #region ChargeLarge atlas helper
    /// <summary>
    /// 设置图集 chargeLarge 的指定 Sprite 到目标对象
    /// 如果目标对象没有 Image 组件 将会自动创建一个
    /// </summary>
    /// <param name="o">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetChargeLarge(GameObject o, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(o, chargeLarge, sprite, onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置图集 chargeLarge 的指定 Sprite 到目标对象
    /// 如果目标对象没有 Image 组件 将会自动创建一个
    /// </summary>
    /// <param name="t">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetChargeLarge(Transform t, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(t, chargeLarge, sprite, onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置图集 chargeLarge 的指定 Sprite 到目标对象
    /// </summary>
    /// <param name="i">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetChargeLarge(Image i, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(i, chargeLarge, sprite, onComplete, useNativeSize);
    }
    #endregion

    #region Huazha atlas
    /// <summary>
    /// 设置图集 Huazha 的指定 Sprite 到目标对象
    /// 如果目标对象没有 Image 组件 将会自动创建一个
    /// </summary>
    /// <param name="o">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetHuazha(GameObject o, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(o, HuaZha, sprite, onComplete, useNativeSize);
    }

    #endregion

    #region Puzzle atlas
    /// <summary>
    /// 设置图集 Puzzle 的指定 Sprite 到目标对象
    /// 如果目标对象没有 Image 组件 将会自动创建一个
    /// </summary>
    /// <param name="o">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetPuzzle(GameObject o, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(o, Puzzle, sprite, onComplete, useNativeSize);
    }

    #endregion

    #region Shared atlas helper
    /// <summary>
    /// 设置图集 Shared 的指定 Sprite 到目标对象
    /// 如果目标对象没有 Image 组件 将会自动创建一个
    /// </summary>
    /// <param name="o">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetShared(GameObject o, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(o, Shared, sprite, onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置图集 Shared 的指定 Sprite 到目标对象
    /// 如果目标对象没有 Image 组件 将会自动创建一个
    /// </summary>
    /// <param name="t">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetShared(Transform t, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(t, Shared, sprite, onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置图集 Shared 的指定 Sprite 到目标对象
    /// </summary>
    /// <param name="i">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetShared(Image i, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(i, Shared, sprite, onComplete, useNativeSize);
    }

    #endregion

    #region Icons atlas helper

    /// <summary>
    /// 设置图集 Icons 的指定 Sprite 到目标对象
    /// 如果目标对象没有 Image 组件 将会自动创建一个
    /// </summary>
    /// <param name="o">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetIcons(GameObject o, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(o, Icons, sprite, onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置图集 Icons 的指定 Sprite 到目标对象
    /// 如果目标对象没有 Image 组件 将会自动创建一个
    /// </summary>
    /// <param name="t">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetIcons(Transform t, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(t, Icons, sprite, onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置图集 Icons 的指定 Sprite 到目标对象
    /// </summary>
    /// <param name="i">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetIcons(Image i, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(i, Icons, sprite, onComplete, useNativeSize);
    }

    #endregion

    #region Rune atlas helper

    /// <summary>
    /// 设置图集 Rune 的指定 Sprite 到目标对象
    /// 如果目标对象没有 Image 组件 将会自动创建一个
    /// </summary>
    /// <param name="o">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetRune(GameObject o, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(o, Rune, sprite, onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置图集 Rune 的指定 Sprite 到目标对象
    /// 如果目标对象没有 Image 组件 将会自动创建一个
    /// </summary>
    /// <param name="t">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetRune(Transform t, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(t, Rune, sprite, onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置图集 Rune 的指定 Sprite 到目标对象
    /// </summary>
    /// <param name="i">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetRune(Image i, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(i, Rune, sprite, onComplete, useNativeSize);
    }

    #endregion

    #region skill atlas helper
    public static void SetSkillIcon(Transform t, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(t, Skillicon, sprite, onComplete, useNativeSize);
    }

    #endregion

    #region NPC avatar atlas helper

    /// <summary>
    /// 设置图集 Avatars (仅包含 NPC 头像和职业头像，玩家动态头像请使用 Module_Avatar.SetAvatar) 的指定 Sprite 到目标对象
    /// 如果目标对象没有 Image 组件 将会自动创建一个
    /// </summary>
    /// <param name="o">目标对象</param>
    /// <param name="avatar">头像名称（等于该 NPC 的资源名）</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetAvatar(GameObject o, string avatar, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(o, Avatars, avatar, onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置图集 Avatars (仅包含 NPC 头像和职业头像，玩家动态头像请使用 Module_Avatar.SetAvatar) 的指定 Sprite 到目标对象
    /// 如果目标对象没有 Image 组件 将会自动创建一个
    /// </summary>
    /// <param name="t">目标对象</param>
    /// <param name="avatar">头像名称（等于该 NPC 的资源名）</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetAvatar(Transform t, string avatar, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(t, Avatars, avatar, onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置图集 Avatars (仅包含 NPC 头像和职业头像，玩家动态头像请使用 Module_Avatar.SetAvatar) 的指定 Sprite 到目标对象
    /// </summary>
    /// <param name="i">目标对象</param>
    /// <param name="avatar">头像名称（等于该 NPC 的资源名）</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetAvatar(Image i, string avatar, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(i, Avatars, avatar, onComplete, useNativeSize);
    }

    #endregion

    #region NpcDateInfo atlas helper
    /// <summary>
    /// 设置图集 NpcDateInfo (包含 约会NPC的头像、选中框、名字图片) 的指定 Sprite 到目标对象
    /// 如果目标对象没有 Image 组件 将会自动创建一个
    /// </summary>
    /// <param name="o">目标对象</param>
    /// <param name="sprite">资源名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetNpcDateInfo(GameObject o, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(o, NpcDateInfo, sprite, onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置图集 NpcDateInfo (包含 约会NPC的头像、选中框、名字图片) 的指定 Sprite 到目标对象
    /// 如果目标对象没有 Image 组件 将会自动创建一个
    /// </summary>
    /// <param name="t">目标对象</param>
    /// <param name="sprite">资源名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetNpcDateInfo(Transform t, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(t, NpcDateInfo, sprite, onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置图集 NpcDateInfo (包含 约会NPC的头像、选中框、名字图片) 的指定 Sprite 到目标对象
    /// </summary>
    /// <param name="i">目标对象</param>
    /// <param name="sprite">资源名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetNpcDateInfo(Image i, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(i, NpcDateInfo, sprite, onComplete, useNativeSize);
    }
    #endregion

    #region Quick links

    #region Item

    /// <summary>
    /// 设置指定道具图标到目标
    /// </summary>
    /// <param name="o">目标对象</param>
    /// <param name="item">道具</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetItemIcon(GameObject o, PropItemInfo item, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        if (item && item.itemType == PropType.Currency && string.IsNullOrEmpty(item.icon)) SetCurrencyIcon(o, (CurrencySubType)item.subType, onComplete, useNativeSize);
        else
            UIAtlasTarget.SetSprite(o, !item ? null : item.itemType == PropType.Rune ? Rune : Icons, item ? item.icon : null, onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置指定道具图标到目标
    /// </summary>
    /// <param name="t">目标对象</param>
    /// <param name="item">道具</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetItemIcon(Transform t, PropItemInfo item, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        if (item && item.itemType == PropType.Currency && string.IsNullOrEmpty(item.icon)) SetCurrencyIcon(t, (CurrencySubType)item.subType, onComplete, useNativeSize);
        else
            UIAtlasTarget.SetSprite(t, !item ? null : item.itemType == PropType.Rune ? Rune : Icons, item ? item.icon : null, onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置指定道具图标到目标
    /// </summary>
    /// <param name="i">目标对象</param>
    /// <param name="item">道具</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetItemIcon(Image i, PropItemInfo item, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        if (item && item.itemType == PropType.Currency && string.IsNullOrEmpty(item.icon)) SetCurrencyIcon(i, (CurrencySubType)item.subType, onComplete, useNativeSize);
        else
            UIAtlasTarget.SetSprite(i, !item ? null : item.itemType == PropType.Rune ? Rune : Icons, item ? item.icon : null, onComplete, useNativeSize);
    }

    #endregion

    #region Currency

    /// <summary>
    /// 设置指定货币图标到目标
    /// </summary>
    /// <param name="o">目标对象</param>
    /// <param name="currency">货币类型</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetCurrencyIcon(GameObject o, CurrencySubType currency, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        if (currency < CurrencySubType.None || currency >= CurrencySubType.Count) currency = CurrencySubType.None;
        UIAtlasTarget.SetSprite(o, Icons, icon_currencies[(int)currency], onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置指定道具图标到目标
    /// </summary>
    /// <param name="t">目标对象</param>
    /// <param name="currency">货币类型</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetCurrencyIcon(Transform t, CurrencySubType currency, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        if (currency < CurrencySubType.None || currency >= CurrencySubType.Count) currency = CurrencySubType.None;
        UIAtlasTarget.SetSprite(t, Icons, icon_currencies[(int)currency], onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置指定道具图标到目标
    /// </summary>
    /// <param name="i">目标对象</param>
    /// <param name="currency">货币类型</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetCurrencyIcon(Image i, CurrencySubType currency, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        if (currency < CurrencySubType.None || currency >= CurrencySubType.Count) currency = CurrencySubType.None;
        UIAtlasTarget.SetSprite(i, Icons, icon_currencies[(int)currency], onComplete, useNativeSize);
    }


    #endregion

    #endregion

    #region PetIcon

    /// <summary>
    /// 设置图集 PetIcons 的指定 Sprite 到目标对象
    /// 如果目标对象没有 Image 组件 将会自动创建一个
    /// </summary>
    /// <param name="o">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetPetIcon(GameObject o, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(o, PetIcons, sprite, onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置图集 PetIcons 的指定 Sprite 到目标对象
    /// 如果目标对象没有 Image 组件 将会自动创建一个
    /// </summary>
    /// <param name="t">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetPetIcon(Transform t, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(t, PetIcons, sprite, onComplete, useNativeSize);
    }

    /// <summary>
    /// 设置图集 PetIcons 的指定 Sprite 到目标对象
    /// </summary>
    /// <param name="i">目标对象</param>
    /// <param name="sprite">图片名称</param>
    /// <param name="onComplete">设置结束回调</param>
    /// <param name="useNativeSize">是否使用原始尺寸</param>
    public static void SetPetIcon(Image i, string sprite, Action<UIAtlasTarget> onComplete = null, bool useNativeSize = false)
    {
        UIAtlasTarget.SetSprite(i, PetIcons, sprite, onComplete, useNativeSize);
    }
    #endregion


}
