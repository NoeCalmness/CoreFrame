/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Extended methods.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-07-10
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

public static partial class ExtendedMethods
{
    /// <summary>
    /// 播放或暂停制定对象及其所有子对象的粒子系统
    /// </summary>
    /// <param name="go">要播放/暂停的对象</param>
    /// <param name="pause">是否暂停</param>
    public static void PauseResumAllParticles(this GameObject go, bool pause = true)
    {
        var pauseresum = go.GetComponentDefault<PauseResumParticles>();
        pauseresum.PauseResum(pause);
    }

    /// <summary>
    /// 设置对象及其所有子对象的粒子系统的播放速度
    /// </summary>
    /// <param name="go">要设置的对象</param>
    /// <param name="speed">播放速度</param>
    public static void SetSimulationSpeed(this GameObject go, float speed = 1.0f)
    {
        var pauseresum = go.GetComponentDefault<PauseResumParticles>();
        pauseresum.UpdateSpeed(speed);
    }

    /// <summary>
    /// 设置目标特效是否使用真实时间播放
    /// 真实时间不受 Time.timeScale 影响
    /// </summary>
    /// <param name="go">目标对象</param>
    /// <param name="realtime">是否使用真实时间</param>
    public static void SetRealtimeParticle(this GameObject go, bool realtime = true)
    {
        if (!go) return;
        var r = go.GetComponentDefault<RealtimeParticle>();
        r.realtime = realtime;
    }

    /// <summary>
    /// 设置目标特效是否使用真实时间播放
    /// 真实时间不受 Time.timeScale 影响
    /// </summary>
    /// <param name="go">目标对象</param>
    /// <param name="realtime">是否使用真实时间</param>
    public static void SetRealtimeParticle(this Transform go, bool realtime = true)
    {
        if (!go) return;
        var r = go.GetComponentDefault<RealtimeParticle>();
        r.realtime = realtime;
    }

    public static CreatureInfo ToCreatureInfo(this PRoleAttr attr, CreatureInfo info)
    {
        if (attr == null) return info;

        info.strength              = (int)attr.power[0];
        info.artifice              = (int)attr.tenacious[0];
        info.vitality              = (int)attr.energy[0];
        info.maxHealth             = (int)attr.maxHp[0];
        info.health                = info.maxHealth; 
        info.attack                = (int)attr.attack[0];
        info.offAttack             = (int)attr.gunAttack[0];
        info.defense               = (int)attr.defense[0];
        info.maxRage               = attr.maxMp[0];
        info.critMul               = attr.knock[0];
        info.crit                  = attr.knockRate[0];
        info.resilience            = attr.artifice[0];
        info.attackSpeed           = attr.attackSpeed[0];
        info.moveSpeed             = attr.moveSpeed[0];
        info.regenRage             = attr.angerSec[0];
        info.firm                  = (int)attr.bone[0];
        info.brutality             = (int)attr.brutal[0];
        info.elementAttack         = (int)attr.elementAttack[0];
        info.elementDefenseWind    = (int)attr.elementDefenseWind[0];
        info.elementDefenseFire    = (int)attr.elementDefenseFire[0];
        info.elementDefenseWater   = (int)attr.elementDefenseWater[0];
        info.elementDefenseThunder = (int)attr.elementDefenseThunder[0];
        info.elementDefenseIce     = (int)attr.elementDefenseIce[0];

        return info;
    }

    public static CreatureDirection Inverse(this CreatureDirection dir)
    {
        return dir == CreatureDirection.BACK ? CreatureDirection.FORWARD : CreatureDirection.BACK;
    }

    /// <summary>
    /// Convert direction to int
    /// -1 = BACK  1 = RIGHT  0 = ERROR
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static int ToInt(this CreatureDirection dir)
    {
        return dir == CreatureDirection.BACK ? -1 : dir == CreatureDirection.FORWARD ? 1 : 0;
    }

    /// <summary>
    /// Check direction is same as check
    /// check is negative means left, positive means right, zero always return true
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static bool SameAs(this CreatureDirection dir, int check)
    {
        return check == 0 || dir == CreatureDirection.BACK && check < 0 || dir == CreatureDirection.FORWARD && check > 0;
    }

    public static Position ToPPostion(this Vector3 vector)
    {
        Position p = PacketObject.Create<Position>();
        p.x = Mathf.RoundToInt(vector.x * 100);
        p.y = Mathf.RoundToInt(vector.y * 100);
        p.z = Mathf.RoundToInt(vector.z * 100);
        return p;
    }

    static public Vector3 ToVector3(this Position pos)
    {
        return new Vector3(pos.x * 1.0f / 100f, pos.y * 1.0f / 100f, pos.z * 1.0f / 100f);
    }

    static public bool IsVailidCurve(this AnimationCurve curve)
    {
        return curve != null && curve.keys != null && curve.keys.Length > 0;
    }

    #region PItem 方法扩展

    public static PropItemInfo GetPropItem(this PItem item)
    {
        if (item == null) return PropItemInfo.empty;

        var info = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
        return info ? info : PropItemInfo.empty;
    }

    /// <summary>
    /// 是否是异常属性
    /// </summary>
    /// <param name="pitemData"></param>
    /// <returns></returns>
    public static bool InvalidGrowAttr(this PItem pitemData)
    {
        return pitemData == null || pitemData.growAttr == null || pitemData.growAttr.equipAttr == null;
    }

    public static int GetIntentyLevel(this PItem pitemData)
    {
        return InvalidGrowAttr(pitemData) ? 0 : pitemData.growAttr.equipAttr.strength;
    }
    public static int GetEvolveLevel(this PItem pitemData)
    {
        return InvalidGrowAttr(pitemData) ? 0 : pitemData.growAttr.equipAttr.advance;
    }

    /// <summary>
    /// 获取当前总经验
    /// </summary>
    /// <param name="pitemData"></param>
    /// <returns></returns>
    public static int GetCurrentTotalExp(this PItem pitemData)
    {
        int level = pitemData.GetIntentyLevel();
        EquipType type = Module_Equip.GetEquipTypeByItem(pitemData);
        var info = ConfigManager.Find<IntentifyEquipInfo>(o => o.type == type && o.level == level);

        return pitemData.GetCurrentLevelExp() + (info == null ? 0 : info.exp);
    }

    /// <summary>
    /// 获取当前超过等级的经验
    /// </summary>
    /// <param name="pitemData"></param>
    /// <returns></returns>
    public static int GetCurrentLevelExp(this PItem pitemData)
    {
        return InvalidGrowAttr(pitemData) ? 0 : (int)pitemData.growAttr.equipAttr.strengthExpr;
    }

    public static bool HasEvolved(this PItem pitemData)
    {
        int intentyLevel = GetIntentyLevel(pitemData);
        int evolveLevel = GetEvolveLevel(pitemData);
        return intentyLevel > 0 && intentyLevel / Module_Equip.EVOLVE_LEVEL == evolveLevel;
    }

    public static double GetFixedAttirbute(this PItem data, int id)
    {
        if (!data.InvalidGrowAttr() && data.growAttr.equipAttr.fixedAttrs != null)
        {
            foreach (var item in data.growAttr.equipAttr.fixedAttrs)
            {
                if (item.id == id) return item.value;
            }
        }

        return 0;
    }

    public static List<ushort> GetAllFixedAttributrIds(this PItem data)
    {
        List<ushort> l = new List<ushort>();
        if (!data.InvalidGrowAttr() && data.growAttr.equipAttr.fixedAttrs != null)
        {
            foreach (var item in data.growAttr.equipAttr.fixedAttrs)
            {
                l.Add(item.id);
            }
        }
        return l;
    }

    public static int GetSuitId(this PItem pitemData)
    {
        return pitemData == null || pitemData.growAttr == null ? 0 : (int)pitemData.growAttr.suitId;
    }
    #endregion

    #region mazeplayer 和 nmlPlayer
    public static int GetProto(this PMazePlayer data)
    {
        if (data == null) return (int)CreatureVocationType.Vocation1;

        if(data.roleProto == (byte)CreatureVocationType.All) data.roleProto = (byte)data.fashion.GetProto();
        return data.roleProto;
    }

    public static int GetProto(this PNmlPlayer data)
    {
        if (data == null) return (int)CreatureVocationType.Vocation1;

        if (data.roleProto == (byte)CreatureVocationType.All) data.roleProto = (byte)data.fashion.GetProto();
        return data.roleProto;
    }

    public static int GetProto(this PFashion fashion)
    {
        if (fashion != null)
        {
            var w = ConfigManager.Get<PropItemInfo>(fashion.weapon);
            if (w) return w.subType;
        }
        return (int)CreatureVocationType.Vocation1;
    }

    #endregion

    #region pchaseTask方法扩展

    public static bool IsStarActive(this PChaseTask task,int index)
    {
        if (task == null) return false;
        return ((int)task.curStar).BitMask(index);
    }

    public static int GetStarCount(this PChaseTask task)
    {
        int count = 0;

        if (task != null)
        {
            for (int i = 0; i < 3; i++) count += task.IsStarActive(i) ? 1 : 0;
        }
        return count;
    }

    #endregion

    public static int GetItemCount(this PReward reward, int targetItemId)
    {
        var count = 0;
        var prop = ConfigManager.Get<PropItemInfo>(targetItemId);
        if (null == prop)
            return count;
        if (prop.itemType == PropType.Currency)
        {
            if (prop.subType == (int)CurrencySubType.Gold)
                count += reward.coin;
            else if (prop.subType == (int)CurrencySubType.Diamond)
                count += reward.diamond;
        }

        if (reward.rewardList == null)
            return count;

        for (var i = 0; i < reward.rewardList.Length; i++)
        {
            if (reward.rewardList[i].itemTypeId == targetItemId)
                count += (int)reward.rewardList[i].num;
        }
        return count;
    }
}
