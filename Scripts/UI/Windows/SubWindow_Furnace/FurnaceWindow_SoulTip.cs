// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-20      10:12
//  * LastModify：2018-10-20      10:12
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class FurnaceWindow_SoulTip : SubWindowBase<Window_Soul>
{
    private Image spriteShadow;
    private Image sprite;
    private SoulEntry soulEntry;
    private Button clickButton;

    protected override void InitComponent()
    {
        base.InitComponent();
        sprite          = WindowCache.GetComponent<Image>   ("jipin/item/rune");
        spriteShadow    = WindowCache.GetComponent<Image>   ("jipin/item/spriteShadow");
        clickButton     = WindowCache.GetComponent<Button>  ("jipin");
        soulEntry       = new SoulEntry(WindowCache.GetComponent<Transform>("jipin/item"));
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;
        var soulAttr = (PSoulAttr)p[0];
        RefreshInfo(soulAttr);
        clickButton?.onClick.AddListener(() =>
        {
            parentWindow.ShowSuccessWindow();
            UnInitialize();
        });
        return true;
    }

    private void RefreshInfo(PSoulAttr rAttr)
    {
        soulEntry.Init(rAttr);

        var soulInfo = ConfigManager.Get<SoulInfo>(rAttr.soulId);
        if (null == soulInfo)
            return;

        UIDynamicImage.LoadImage(soulInfo.portrait, t =>
        {
            sprite.SafeSetActive(t);
        }, false, sprite.transform);

        AtlasHelper.SetRune(spriteShadow, soulInfo.shodow);
    }
}
