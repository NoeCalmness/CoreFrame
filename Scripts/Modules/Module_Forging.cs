/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-11-15
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class Module_Forging : Module<Module_Forging>
{
    public const string EventForingUpLoad = "EventForingUpLoad";
    public const string EventForingInSoul = "EventForingInSoul";
    /// <summary>
    /// 更改虚假的金币值
    /// </summary>
    public const string EventGoldChange = "EventGoldChange";
    /// <summary>
    /// 入魂升级音效
    /// </summary>
    public const string EventForingMargeUpAudio = "EventForingMargeUpAudio";
    /// <summary>
    /// 入魂成功音效
    /// </summary>
    public const string EventForingInsoulAudio = "EventForingInsoulAudio";
    /// <summary>
    /// 当前入魂灵石剩余数
    /// </summary>
    public int InsoulStone;

    public void GetInSoulStone(PItem item)
    {
        WeaponAttribute dInfo = ConfigManager.Get<WeaponAttribute>(item.itemTypeId);
        if (dInfo == null) return;
        InsoulStone = moduleCangku.GetItemCount(dInfo.elementId);
    }
    
    public List<Insoul> Insoul_Info { get { return ConfigManager.GetAll<Insoul>(); } }

    /// <summary>
    /// 武器合魂点击次数
    /// </summary>
    public int InsouTimes { get; set; }

    public int defalutEnhanceId { get; set; }

    public int InsoulGold { get; set; }

    public PItem InsoulItem { get; set; }

    public PItem UpLoadItem { get; set; }

    public EquipType ClickType { get; set; } = EquipType.Weapon;

    public List<PItem> GetAllItems(EquipType type)
    {
        List<PItem> r = new List<PItem>();
        r.Add(moduleEquip.GetDressedProp(type));
        r.AddRange(moduleEquip.GetBagDress(type));
        return r;
    }

    public void SendUpLevel()
    {
        CsEquipWeaponEvolved p = PacketObject.Create<CsEquipWeaponEvolved>();
        p.weaponUId = UpLoadItem.itemId;
        session.Send(p);
    }

    void _Packet(ScEquipWeaponEvolved p)
    {
        if (p.result == 0)
        {
            if (UpLoadItem.itemId != p.weaponUId) return;

            UpLoadItem.growAttr.equipAttr.star++;
            var star = UpLoadItem.growAttr.equipAttr.star;
            
            foreach (var item in moduleEquip.m_bagCollections)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    if (item.Value[i].itemId == p.weaponUId) item.Value[i].growAttr.equipAttr.star = star;
                }
            }
            moduleEquip.UpdateWeapons();
            DispatchModuleEvent(EventForingUpLoad);
        }
        else moduleGlobal.ShowMessage(ConfigText.GetDefalutString(224, 22));
    }

    public void SendMarge(ulong itemId)
    {
        CsEquipWeaponGrowth p = PacketObject.Create<CsEquipWeaponGrowth>();
        p.weaponUId = itemId;
        p.times = (byte)InsouTimes;
        session.Send(p);
        InsouTimes = 0;
    }

    void _Packet(ScEquipWeaponGrowth p)
    {
        if (p.result == 0)
        {
            ScEquipWeaponGrowth pp = p.Clone();
            foreach (var item in moduleEquip.m_bagCollections)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    if (item.Value[i].itemId == pp.weaponUId)
                    {
                        item.Value[i].growAttr.equipAttr.level = (byte)pp.level;
                        item.Value[i].growAttr.equipAttr.expr = pp.curLvExpr;
                    }
                }
            }
            moduleEquip.UpdateWeapons();
        }
        else moduleGlobal.ShowMessage(ConfigText.GetDefalutString(224, 29));
    }

    public void InsoulClick(bool levelUP = false)
    {
        var level = InsoulItem.growAttr.equipAttr.level;
        var attr = Insoul_Info[level].exp_one;

        do
        {
            InsouTimes++;//点击的武器点击次加一
            level = InsoulItem.growAttr.equipAttr.level;
            WeaponAttribute global = ConfigManager.Get<WeaponAttribute>(InsoulItem.itemTypeId);

            InsoulGold -= Insoul_Info[level].gold;
            InsoulStone -= Insoul_Info[level].lingshi;
            attr = Insoul_Info[level].exp_one;

            InsoulItem.growAttr.equipAttr.expr += (uint)Insoul_Info[level].exp_one;
        }
        while (levelUP && InsoulItem.growAttr.equipAttr.expr < Insoul_Info[level].exp);

        if (InsoulItem.growAttr.equipAttr.expr >= Insoul_Info[level].exp)
        {
            SendMarge(InsoulItem.itemId);

            InsoulItem.growAttr.equipAttr.expr -= (uint)Insoul_Info[level].exp;
            InsoulItem.growAttr.equipAttr.level++;
            DispatchModuleEvent(EventForingMargeUpAudio);
        }
        else  DispatchModuleEvent(EventForingInsoulAudio);

        DispatchModuleEvent(EventForingInSoul, attr);
        GlodChange();
    }
    public void GlodChange()
    {
        DispatchModuleEvent(EventGoldChange);//更改虚假金币
    }

    public int GetWeaponIndex(PropItemInfo info)
    {
        if (!info) return -1;

        if (info.itemType == PropType.FashionCloth) return 6;
        else
        {
            switch ((WeaponSubType)info.subType)
            {
                case WeaponSubType.LongSword: return 0;
                case WeaponSubType.Katana: return 1;
                case WeaponSubType.Spear: return 2;
                case WeaponSubType.GiantAxe: return 3;
                case WeaponSubType.Gloves: return 4;
                case WeaponSubType.Gun: return 5;
            }
        }
        return -1;
    }
    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        ClickType = EquipType.Weapon;
        Insoul_Info.Clear();
        InsoulStone = -1;
        InsouTimes = 0;
        InsoulGold = -1;
        InsoulItem = null;
        UpLoadItem = null;
    }
}
