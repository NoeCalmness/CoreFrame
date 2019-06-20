// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 颜色管理组
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-06-08      16:14
//  * LastModify：2018-07-11      19:56
//  ***************************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;

public enum ColorManagerType
{
    /// <summary>
    /// 显示出战状态的属性
    /// </summary>
    IsFightingAttribute     = 0,
    /// <summary>
    /// 材料是否足够
    /// </summary>
    IsMatrialEnough         = 1,
    /// <summary>
    /// 钱是否足够
    /// </summary>
    IsMoneyEnough           = 2,
    /// <summary>
    /// 任务成功率
    /// </summary>
    TaskSuccessRate         = 3,
    /// <summary>
    /// 历练次数使用完了
    /// </summary>
    TimesUseUp              = 4,
    /// <summary>
    /// 没有解锁的宠物名字颜色
    /// </summary>
    UnlockPetNameColor      = 5,
    /// <summary>
    /// 宠物的品质框颜色
    /// </summary>
    PetQuality              = 6,
    /// <summary>
    /// 任务类型颜色
    /// </summary>
    TaskType                = 7,

    PetSummonMatrialEnough  = 8,
    /// <summary>
    /// 套装效果激活颜色，以及未激活颜色
    /// </summary>
    SuitActive              = 9,
    /// <summary>
    /// Npc的属性激活/未激活颜色
    /// </summary>
    NpcAttributeActive      = 10,

    /// <summary>
    /// Npc约会餐厅菜单道具数量足够/不足颜色
    /// </summary>
    NpcDatingRestOrder = 11,

    /// <summary>
    /// 推荐战力颜色。不符合/符合
    /// </summary>
    Recommend = 12,
    /// <summary>
    /// 阵营战对战界面排行榜中，自己的底板颜色/其他人的底板颜色
    /// </summary>
    FactionBgLeft = 13,
    /// <summary>
    /// 阵营战对战界面排行榜中，自己的底板颜色/其他人的底板颜色
    /// </summary>
    FactionBgRight = 14,
}
[Serializable]
public class ColorGroup
{
    public List<Entry> Colors;

    public static Color GetColor(ColorManagerType rType, bool rCondition)
    {
        var group = ColorGroupConfig.Default;
        return group._GetColor(rType, rCondition);
    }

    public static Color GetColor(ColorManagerType rType, float rValue)
    {

        var group = ColorGroupConfig.Default;
        return group._GetColor(rType, rValue);
    }

    public Color _GetColor(ColorManagerType rType, bool rCondition)
    {
        if (Colors == null || Colors.Count == 0)
            return Color.white;
        var entry = Colors.Find(item => item.Type == rType);
        if (entry != null) return entry.Get(rCondition);
        return Color.white;
    }

    public Color _GetColor(ColorManagerType rType, float rValue)
    {
        if (Colors == null || Colors.Count == 0)
            return Color.white;
        var entry = Colors.Find(item => item.Type == rType);
        if (entry == null)
            return Color.white;
        return entry.Get(rValue);
    }

    [Serializable]
    public class ValueColorEntry
    {
        public Color    Color;
        public Vector2  Value;
    }

    [Serializable]
    public class Entry
    {
        public Color                Color1;
        public Color                Color2;
        public ValueColorEntry[]    Segments;
        public ColorManagerType     Type;

        public Color Get(bool rCondition)
        {
            return rCondition ? Color1 : Color2;
        }

        public Color Get(float rValue)
        {
            for (var i = 0; i < Segments.Length; i++)
            {
                if (rValue >= Segments[i].Value.x && rValue <= Segments[i].Value.y)
                {
                    return Segments[i].Color;
                }
            }
            return Color1;
        }
    }
}
