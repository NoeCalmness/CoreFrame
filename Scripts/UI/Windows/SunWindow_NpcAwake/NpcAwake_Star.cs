// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-12-13      10:42
//  *LastModify：2018-12-13      11:41
//  ***************************************************************************************************/
#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class NpcAwake_Star : SubWindowBase
{
    private Button                  excuteButton;

    private Text                    costLabel;

    private Text                    ownLabel;

    private Text                    costTitle;

    private Text                    descLabel;

    private Text                    descTitle;

    private Image                   costIcon;

    private NpcTypeID               _npcId;

    private INpcMessage             _npc;

    private int                     _excuteTimes;

    private Image                   _headIcon;

    private Image                   _costIcon2;



    protected override void InitComponent()
    {
        excuteButton = WindowCache.GetComponent<Button>("starPower_Panel/affuseStarPower_Btn");
        costLabel    = WindowCache.GetComponent<Text>("starPower_Panel/affuseStarPower_Btn/consume_Txt");
        ownLabel     = WindowCache.GetComponent<Text>("starPower_Panel/haveNumber/number");
        costTitle    = WindowCache.GetComponent<Text>("starPower_Panel/comsume/consume_Txt_02");
        descLabel    = WindowCache.GetComponent<Text>("starPower_Panel/content");
        descTitle    = WindowCache.GetComponent<Text>("starPower_Panel/title");
        costIcon     = WindowCache.GetComponent<Image>("starPower_Panel/affuseStarPower_Btn/consume_Img");
        _costIcon2   = WindowCache.GetComponent<Image>("starPower_Panel/haveNumber/icon");
        _headIcon    = WindowCache.GetComponent<Image>("starPower_Panel/avatar/head_icon");
    }

    public override bool Initialize(params object[] p)
    {
        var watcher = TimeWatcher.Watch("star panel init");
        if (!base.Initialize(p))
            return false;

        _npcId = (NpcTypeID) p[0];
        _npc = moduleNpc.GetTargetNpc(_npcId);
        AtlasHelper.SetAvatar(_headIcon, _npc?.icon);

        watcher.See("111");
        excuteButton?.onClick.AddListener(OnExcute);
        RefreshUI();
        watcher.See("222");
        return true;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;
        excuteButton?.onClick.RemoveAllListeners();

        if (_excuteTimes > 0)
        {
            moduleAwake.Request_NpcPerfusion(_npcId, _excuteTimes);
            _excuteTimes = 0;
        }

        return true;
    }

    private void OnExcute()
    {
        var info = ConfigManager.Get<NpcPerfusionInfo>(_npc.starLv);
        if (null == info) return;

        if (!moduleEquip.EnoughPropFromCheckItemPair(info.costs, _excuteTimes + 1))
        {
            if (_excuteTimes > 0)
                moduleAwake.Request_NpcPerfusion(_npcId, _excuteTimes);
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(9815, 1));
            return;
        }
        _excuteTimes ++;
        _npc.starExp += info.onceExp;
        if (_npc.starExp >= info.exp || _excuteTimes >= 1)
            moduleAwake.Request_NpcPerfusion(_npcId, _excuteTimes);
        else
        {
            moduleNpc.DispatchEvent(Module_Npc.NpcPerfusionChangeEvent);
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        excuteButton.SafeSetActive(!_npc.starIsMax);
        ownLabel?.transform.parent?.SafeSetActive(!_npc.starIsMax);

        var buff = ConfigManager.Get<BuffInfo>(_npc.npcInfo.constellationBuff);
        Util.SetText(descTitle, $"{buff?.name} Lv{_npc.starLv}");
        Util.SetText(descLabel, buff?.BuffDesc(_npc.starLv));

        RefreshMatrial();
    }

    private void RefreshMatrial()
    {
        var info = ConfigManager.Get<NpcPerfusionInfo>(_npc.starLv);
        int cost = 0, own = 0;
        if (info?.costs.Length > 0)
        {
            cost = info.costs[0].count;
            own = moduleEquip.GetPropCount(info.costs[0].itemId) - info.costs[0].count * _excuteTimes;
            var prop = ConfigManager.Get<PropItemInfo>(info.costs[0].itemId);
            AtlasHelper.SetIcons(costIcon, prop?.icon);
            AtlasHelper.SetIcons(_costIcon2, prop?.icon);
            Util.SetText(costTitle, Util.Format(ConfigText.GetDefalutString(TextForMatType.NpcAwakeUI, 4), prop?.itemName));

            if (prop != null)
            {
                ushort itemId = (ushort)prop.ID;
                _costIcon2?.GetComponentDefault<Button>().onClick.AddListener(() =>
                {
                    moduleGlobal.UpdateGlobalTip(itemId, true);
                });
            }
        }

        Util.SetText(costLabel, cost.ToString());
        Util.SetText(ownLabel, Util.Format(ConfigText.GetDefalutString(TextForMatType.NpcAwakeUI, 5), own));
        if (costLabel)
            costLabel.color = ColorGroup.GetColor(ColorManagerType.IsMatrialEnough, cost <= own);
    }

    private void _ME(ModuleEvent<Module_Npc> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Npc.Response_NpcPerfusionChange:
                ResponseControl(e.msg as ScNpcPerfusion);
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Player> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Player.EventCurrencyChanged:
                RefreshMatrial();
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Equip> e)
    {
        switch(e.moduleEvent)
        {
            case Module_Equip.EventItemDataChange:
            case Module_Equip.EventUpdateBagProp:
                RefreshMatrial();
                break;
        }
    }

    private void ResponseControl(ScNpcPerfusion msg)
    {
        if (null == msg)
            return;

        if (msg.result != 0)
        {
            _excuteTimes = 0;
            moduleGlobal.ShowMessage(9815, 2);
            return;
        }
        _excuteTimes -= msg.times;
        _excuteTimes = Mathf.Max(0, _excuteTimes);
        RefreshUI();
    }
}
