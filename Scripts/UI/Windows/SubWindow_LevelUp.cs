// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-11-24      16:38
//  *LastModify：2018-11-24      16:38
//  ***************************************************************************************************/

using UnityEngine.UI;

public class SubWindow_LevelUp : SubWindowBase
{
    private Text beforeLv;
    private Text currentLv;
    private Text beforePotential;
    private Text currentPotential;
    private Text beforeFatigue;
    private Text currentFatigue;
    private Text beforeFatigueLimit;
    private Text currentFatigueLimit;
    private Button closeButton;

    protected override void InitComponent()
    {
        base.InitComponent();
        beforeLv            = WindowCache.GetComponent<Text>("upLvPanel/dengji/Text");
        currentLv           = WindowCache.GetComponent<Text>("upLvPanel/dengji/Text1");
        beforePotential     = WindowCache.GetComponent<Text>("upLvPanel/qiannengdian/Text");
        currentPotential    = WindowCache.GetComponent<Text>("upLvPanel/qiannengdian/Text1");
        beforeFatigue       = WindowCache.GetComponent<Text>("upLvPanel/tili/Text");
        currentFatigue      = WindowCache.GetComponent<Text>("upLvPanel/tili/Text1");
        beforeFatigueLimit  = WindowCache.GetComponent<Text>("upLvPanel/tiliLimit/Text");
        currentFatigueLimit = WindowCache.GetComponent<Text>("upLvPanel/tiliLimit/Text1");
        closeButton         = WindowCache.GetComponent<Button>("upLvPanel");
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;
        closeButton?.onClick.AddListener(() => UnInitialize(false));
        if (p != null && p.Length >= 4)
        {
            var oldLv = (byte)p[0];
            var oldPoint = (ushort)p[1];
            var oldFatigue = (ushort)p[2];
            var oldMaxFatigue = (ushort)p[3];
            BindData(oldLv, oldPoint, oldFatigue, oldMaxFatigue);
        }
        return true;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;
        closeButton?.onClick.RemoveAllListeners();
        return true;
    }

    private void BindData(byte oldLv, ushort oldPoint, ushort oldFatigue, ushort oldMaxFatigue)
    {
        Util.SetText(beforeLv, oldLv.ToString());
        Util.SetText(currentLv, modulePlayer.level.ToString());
        Util.SetText(beforePotential, oldPoint.ToString());
        Util.SetText(currentPotential, modulePlayer.roleInfo.attrPoint.ToString());
        Util.SetText(beforeFatigue, oldFatigue.ToString());
        Util.SetText(currentFatigue, modulePlayer.roleInfo.fatigue.ToString());
        Util.SetText(beforeFatigueLimit, oldMaxFatigue.ToString());
        Util.SetText(currentFatigueLimit, modulePlayer.maxFatigue.ToString());
    }
}
