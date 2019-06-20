// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-20      9:44
//  * LastModify：2018-10-20      9:44
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class FurnaceWindow_SoulSummon : SubWindowBase<Window_Soul>
{
    private Transform itemRoot;
    private Image portrait;
    private Button closeButton;

    protected override void InitComponent()
    {
        base.InitComponent();
        itemRoot    = WindowCache.GetComponent<Transform>("summon_Panel/item");
        portrait    = WindowCache.GetComponent<Image>    ("summon_Panel/sprite");
        closeButton = WindowCache.GetComponent<Button>   ("summon_Panel/btn");
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;
        var soulAttr = (PSoulAttr) p[0];
        Util.SetItemInfoSimple(itemRoot, moduleFurnace.currentSoulItem.GetPropItem());
        var soulInfo = ConfigManager.Get<SoulInfo>(soulAttr?.soulId ?? 0);
        if (null != soulInfo)
        {
            UIDynamicImage.LoadImage(portrait?.transform, soulInfo.portrait);
        }

        moduleGlobal.ShowGlobalLayerDefault(-1);

        closeButton?.onClick.AddListener(() =>
        {
            parentWindow.ShowTipWindow();
            UnInitialize();
        });
        return true;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;
        closeButton?.onClick.RemoveAllListeners();
        return true;
    }
}
