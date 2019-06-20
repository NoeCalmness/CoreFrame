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
    /// Ĭ�����Խ�ɫͷ��
    /// </summary>
    public static readonly string defaultMale = defaultAvatarPrefix + "1";
    /// <summary>
    /// Ĭ��Ů�Խ�ɫͷ��
    /// </summary>
    public static readonly string defaultFemale = defaultAvatarPrefix + "0";

    private static Dictionary<string, Texture2D> m_cachedAvatars = new Dictionary<string, Texture2D>();

    /// <summary>
    /// ��������Լ�����ɫͷ��Ŀ�����
    /// Ŀ�����Ӧ����һ�� UI Ԫ��
    /// </summary>
    /// <param name="o">Ҫ��ʾͷ���Ŀ�����</param>
    /// <param name="useNativeSize">�Ƿ�ʹ��ԭʼͷ���С</param>
    public static void SetPlayerAvatar(GameObject o, bool useNativeSize = false)
    {
        if (!o) return;

        var a = Module_Player.instance.classAvatar ?? string.Empty;

        AtlasHelper.SetAvatar(o, a, null, useNativeSize);
    }

    /// <summary>
    /// ����ְҵͷ��Ŀ�����
    /// Ŀ�����Ӧ����һ�� UI Ԫ��
    /// ע�⣺���ͷ�����ʧ�ܲ���δָ���Ա�ͷ����ʾ�հ�
    /// </summary>
    /// <param name="o">Ҫ��ʾͷ���Ŀ�����</param>
    /// <param name="_class">ְҵ</param>
    /// <param name="useNativeSize">�Ƿ�ʹ��ԭʼͷ���С</param>
    /// <param name="gender">�Ա� ��ѡ����Ҫ����ͷ�����ʧ��ʱȷ�����Ի���Ů��Ĭ��ͷ��</param>
    public static void SetClassAvatar(GameObject o, CreatureVocationType _class = CreatureVocationType.All, bool useNativeSize = false, int gender = -1)
    {
        SetClassAvatar(o, (int)_class, useNativeSize, gender);
    }

    /// <summary>
    /// ����ְҵͷ��Ŀ�����
    /// Ŀ�����Ӧ����һ�� UI Ԫ��
    /// ע�⣺���ͷ�����ʧ�ܲ���δָ���Ա�ͷ����ʾ�հ�
    /// </summary>
    /// <param name="o">Ҫ��ʾͷ���Ŀ�����</param>
    /// <param name="_class">ְҵ</param>
    /// <param name="useNativeSize">�Ƿ�ʹ��ԭʼͷ���С</param>
    /// <param name="gender">�Ա� ��ѡ����Ҫ����ͷ�����ʧ��ʱȷ�����Ի���Ů��Ĭ��ͷ��</param>
    public static void SetClassAvatar(GameObject o, sbyte _class, bool useNativeSize = false, int gender = -1)
    {
        SetClassAvatar(o, (int)_class, useNativeSize, gender);
    }

    /// <summary>
    /// ����ְҵͷ��Ŀ�����
    /// Ŀ�����Ӧ����һ�� UI Ԫ��
    /// ע�⣺���ͷ�����ʧ�ܲ���δָ���Ա�ͷ����ʾ�հ�
    /// </summary>
    /// <param name="o">Ҫ��ʾͷ���Ŀ�����</param>
    /// <param name="_class">ְҵ</param>
    /// <param name="useNativeSize">�Ƿ�ʹ��ԭʼͷ���С</param>
    /// <param name="gender">�Ա� ��ѡ����Ҫ����ͷ�����ʧ��ʱȷ�����Ի���Ů��Ĭ��ͷ��</param>
    public static void SetClassAvatar(GameObject o, byte _class, bool useNativeSize = false, int gender = -1)
    {
        SetClassAvatar(o, (int)_class, useNativeSize, gender);
    }

    /// <summary>
    /// ����ְҵͷ��Ŀ�����
    /// Ŀ�����Ӧ����һ�� UI Ԫ��
    /// ע�⣺���ͷ�����ʧ�ܲ���δָ���Ա�ͷ����ʾ�հ�
    /// </summary>
    /// <param name="o">Ҫ��ʾͷ���Ŀ�����</param>
    /// <param name="_class">ְҵ</param>
    /// <param name="useNativeSize">�Ƿ�ʹ��ԭʼͷ���С</param>
    /// <param name="gender">�Ա� ��ѡ����Ҫ����ͷ�����ʧ��ʱȷ�����Ի���Ů��Ĭ��ͷ��</param>
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
    /// ����ָ����ɫְҵͷ��Ŀ�����
    /// Ŀ�����Ӧ����һ�� UI Ԫ��
    /// </summary>
    /// <param name="o">Ҫ��ʾͷ���Ŀ�����</param>
    /// <param name="c">Ҫ��ʾͷ��Ľ�ɫ</param>
    /// <param name="useNativeSize">�Ƿ�ʹ��ԭʼͷ���С</param>
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
    /// ����ָ����ɫͷ��Ŀ�����
    /// Ŀ�����Ӧ����һ�� UI Ԫ��
    /// </summary>
    /// <param name="o">Ҫ��ʾͷ���Ŀ�����</param>
    /// <param name="c">Ҫ��ʾͷ��Ľ�ɫ</param>
    /// <param name="useNativeSize">�Ƿ�ʹ��ԭʼͷ���С</param>
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
    /// ����ͷ�� avatar ��Ŀ�����
    /// Ŀ�����Ӧ����һ�� UI Ԫ��
    /// ע�⣺���ͷ�����ʧ�ܲ���δָ���Ա�ͷ����ʾ�հ�
    /// </summary>
    /// <param name="o">Ҫ��ʾͷ���Ŀ�����</param>
    /// <param name="avatar">ͷ��</param>
    /// <param name="useNativeSize">�Ƿ�ʹ��ԭʼͷ���С</param>
    /// <param name="gender">�Ա� ��ѡ����Ҫ����ͷ�����ʧ��ʱȷ�����Ի���Ů��Ĭ��ͷ��</param>
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