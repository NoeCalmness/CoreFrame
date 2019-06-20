// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-10      11:03
//  * LastModify：2018-10-10      19:44
//  ***************************************************************************************************/

#region

using UnityEngine;
using UnityEngine.UI;

#endregion

public class FurnaceWindow_Setting : SubWindowBase
{
    private Button bgButton;
    private Button closeButton;
    private Toggle[] toggles;
    

    protected override void InitComponent()
    {
        base.InitComponent();
        closeButton = WindowCache.GetComponent<Button>("settingTip/settingPanel/close_button");
        bgButton    = WindowCache.GetComponent<Button>("settingTip/bg");
        var node    = WindowCache.GetComponent<Transform>("settingTip/settingPanel");
        toggles = new Toggle[6];
        for (var i = 1; i <= toggles.Length; i++)
        {
            toggles[i - 1] = node.GetComponent<Toggle>("option0" + i);
            var quality = i;
            toggles[i - 1].onValueChanged.AddListener( b => moduleFurnace.SetQualityHintState(quality, b));
        }
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;
        for (var i = 0; i < toggles.Length; i++)
        {
            var flag = moduleFurnace.GetQualityHintState(i + 1);
            if (toggles[i].isOn != flag)
                toggles[i].isOn = flag;
        }

        closeButton?.onClick.AddListener(()=>bgButton?.onClick.Invoke());
        bgButton   ?.onClick.AddListener(() => UnInitialize());
        return true;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;

        moduleFurnace.SaveHintMask();
        closeButton?.onClick.RemoveAllListeners();
        return true;
    }
}
