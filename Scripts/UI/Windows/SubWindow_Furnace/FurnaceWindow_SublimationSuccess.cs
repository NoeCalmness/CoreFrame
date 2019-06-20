// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-18      14:26
//  * LastModify：2018-10-18      14:26
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class FurnaceWindow_SublimationSuccess : SubWindowBase
{
    private SuitProperty prevSuit;
    private SuitProperty nowSuit;
    private new Text name;

    protected override void InitComponent()
    {
        base.InitComponent();
        prevSuit = new SuitProperty(WindowCache.GetComponent<Transform>("success_Panel/sublimation/left"));
        nowSuit  = new SuitProperty(WindowCache.GetComponent<Transform>("success_Panel/sublimation/right"));
        name = WindowCache.GetComponent<Text>("success_Panel/sublimation/name_txt");
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;
        var item = p[0] as PItem;
        var suitId = (int)p[1];

        if (null == item)
            return true;

        Util.SetText(name, item.GetPropItem().itemName);
        var isDress = moduleEquip.IsDressOn(item);
        prevSuit.Init(suitId, moduleEquip.GetSuitNumber(suitId) + 1, isDress);
        nowSuit.Init(item.growAttr.suitId, moduleEquip.GetSuitNumber(item.growAttr.suitId), isDress, true);
        moduleEquip.LoadModel(item, Layers.WEAPON, true, 0);
        return true;
    }

    public override bool OnReturn()
    {
        return UnInitialize();
    }
}
