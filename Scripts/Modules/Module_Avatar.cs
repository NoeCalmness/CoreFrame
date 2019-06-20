/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Avatar!
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-03-21
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class Module_Avatar : Module<Module_Avatar>
{
    #region Static functions

    public static readonly string hashFileName = "f823190430a51404f0da401cfcbec5a0";

    public static readonly string npcAvatarPrefix = "avatar_npc_";
    public static readonly string npcAvatarPrefixLarge = "avatar_large_npc_";
    public static readonly string defaultAvatarPrefix = "avatar_default_";

    /// <summary>
    /// 默认男性角色头像
    /// </summary>
    public static readonly string defaultMale = defaultAvatarPrefix + "1";
    /// <summary>
    /// 默认女性角色头像
    /// </summary>
    public static readonly string defaultFemale = defaultAvatarPrefix + "0";

    private static Dictionary<string, Texture2D> m_cachedAvatars = new Dictionary<string, Texture2D>();

    /// <summary>
    /// 设置玩家自己的橘色头像到目标对象
    /// 目标对象应当是一个 UI 元素
    /// </summary>
    /// <param name="o">要显示头像的目标对象</param>
    /// <param name="useNativeSize">是否使用原始头像大小</param>
    public static void SetPlayerAvatar(GameObject o, bool useNativeSize = false)
    {
        if (!o) return;

        var a = Module_Player.instance.classAvatar ?? string.Empty;

        AtlasHelper.SetAvatar(o, a, null, useNativeSize);
    }

    /// <summary>
    /// 设置职业头像到目标对象
    /// 目标对象应当是一个 UI 元素
    /// 注意：如果头像加载失败并且未指定性别，头像将显示空白
    /// </summary>
    /// <param name="o">要显示头像的目标对象</param>
    /// <param name="_class">职业</param>
    /// <param name="useNativeSize">是否使用原始头像大小</param>
    /// <param name="gender">性别 可选，主要用于头像加载失败时确定男性还是女性默认头像</param>
    public static void SetClassAvatar(GameObject o, CreatureVocationType _class = CreatureVocationType.All, bool useNativeSize = false, int gender = -1)
    {
        SetClassAvatar(o, (int)_class, useNativeSize, gender);
    }

    /// <summary>
    /// 设置职业头像到目标对象
    /// 目标对象应当是一个 UI 元素
    /// 注意：如果头像加载失败并且未指定性别，头像将显示空白
    /// </summary>
    /// <param name="o">要显示头像的目标对象</param>
    /// <param name="_class">职业</param>
    /// <param name="useNativeSize">是否使用原始头像大小</param>
    /// <param name="gender">性别 可选，主要用于头像加载失败时确定男性还是女性默认头像</param>
    public static void SetClassAvatar(GameObject o, sbyte _class, bool useNativeSize = false, int gender = -1)
    {
        SetClassAvatar(o, (int)_class, useNativeSize, gender);
    }

    /// <summary>
    /// 设置职业头像到目标对象
    /// 目标对象应当是一个 UI 元素
    /// 注意：如果头像加载失败并且未指定性别，头像将显示空白
    /// </summary>
    /// <param name="o">要显示头像的目标对象</param>
    /// <param name="_class">职业</param>
    /// <param name="useNativeSize">是否使用原始头像大小</param>
    /// <param name="gender">性别 可选，主要用于头像加载失败时确定男性还是女性默认头像</param>
    public static void SetClassAvatar(GameObject o, byte _class, bool useNativeSize = false, int gender = -1)
    {
        SetClassAvatar(o, (int)_class, useNativeSize, gender);
    }

    /// <summary>
    /// 设置职业头像到目标对象
    /// 目标对象应当是一个 UI 元素
    /// 注意：如果头像加载失败并且未指定性别，头像将显示空白
    /// </summary>
    /// <param name="o">要显示头像的目标对象</param>
    /// <param name="_class">职业</param>
    /// <param name="useNativeSize">是否使用原始头像大小</param>
    /// <param name="gender">性别 可选，主要用于头像加载失败时确定男性还是女性默认头像</param>
    public static void SetClassAvatar(GameObject o, int _class, bool useNativeSize = false, int gender = -1)
    {
        if (!o) return;

        if (_class <= 0)
        {
            var dt = gender < 0 ? null : gender == 0 ? defaultFemale : defaultMale;
            AtlasHelper.SetAvatar(o, dt);
            return;
        }

        AtlasHelper.SetAvatar(o, Creature.GetClassAvatarName(_class), null, useNativeSize);
    }

    /// <summary>
    /// 设置指定角色职业头像到目标对象
    /// 目标对象应当是一个 UI 元素
    /// </summary>
    /// <param name="o">要显示头像的目标对象</param>
    /// <param name="c">要显示头像的角色</param>
    /// <param name="useNativeSize">是否使用原始头像大小</param>
    public static void SetClassAvatar(GameObject o, Creature c, bool useNativeSize = false)
    {
        if (!o) return;

        if (c && (c.isMonster || c.avatar.StartsWith(npcAvatarPrefix)))
        {
            AtlasHelper.SetAvatar(o, c.avatar);
            return;
        }

        SetClassAvatar(o, c ? c.roleProto : 0, useNativeSize, c ? c.gender : -1);
    }

    /// <summary>
    /// 设置指定角色头像到目标对象
    /// 目标对象应当是一个 UI 元素
    /// </summary>
    /// <param name="o">要显示头像的目标对象</param>
    /// <param name="c">要显示头像的角色</param>
    /// <param name="useNativeSize">是否使用原始头像大小</param>
    public static void SetAvatar(GameObject o, Creature c, bool useNativeSize = false)
    {
        if (!o) return;

        if (c && (c.isMonster || c.avatar.StartsWith(npcAvatarPrefix)))
        {
            AtlasHelper.SetAvatar(o, c.avatar);
            return;
        }

        var a = c ? c.avatar : null;
        var g = c ? c.gender : -1;
        SetAvatar(o, a, useNativeSize, g);
    }
    
    /// <summary>
    /// 设置头像 avatar 到目标对象
    /// 目标对象应当是一个 UI 元素
    /// 注意：如果头像加载失败并且未指定性别，头像将显示空白
    /// </summary>
    /// <param name="o">要显示头像的目标对象</param>
    /// <param name="avatar">头像</param>
    /// <param name="useNativeSize">是否使用原始头像大小</param>
    /// <param name="gender">性别 可选，主要用于头像加载失败时确定男性还是女性默认头像</param>
    public static void SetAvatar(GameObject o, string avatar, bool useNativeSize = false, int gender = -1)
    {
        if (!o) return;

        if (string.IsNullOrEmpty(avatar))
        {
            var dt = gender < 0 ? null : gender == 0 ? defaultFemale : defaultMale;
            AtlasHelper.SetAvatar(o, dt);
            return;
        }

        var cached = m_cachedAvatars.Get(avatar);
        if (cached)
        {
            UIDynamicImage.LoadImageCreated(o, avatar, cached, useNativeSize);
            return;
        }

        var fp = LocalFilePath.AVATAR + "/" + avatar + ".avatar";
        cached = Util.LoadImage(fp);
        if (cached)
        {
            m_cachedAvatars.Set(avatar, cached);
            UIDynamicImage.LoadImageCreated(o, avatar, cached, useNativeSize);
            return;
        }

        var url = URLs.avatar + avatar + ".png";
        UIDynamicImage.LoadImage(o, url, (di, t) =>
        {
            if (!t)
            {
                if (di && gender > -1)
                    AtlasHelper.SetAvatar(di.gameObject, gender == 0 ? defaultFemale : defaultMale);
                return;
            }
            m_cachedAvatars.Set(avatar, t);

            var data = UIDynamicImage.RemoveCache(url, false);

            if (data != null)
            {
                var hash = data.GetMD5();
                if ((fp = Util.SaveFile(fp, data)) != null)
                    HashListFile.ReplaceHashListFile(hashFileName, avatar, hash);
            }
        }, useNativeSize);
    }

    #endregion
}