// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-14      15:37
//  * LastModify：2018-08-14      15:40
//  ***************************************************************************************************/

#region

using UnityEngine;
using UnityEngine.UI;

#endregion

public class AwakeWindow_BuyTimes : SubWindowBase
{
    private Button confirmButton;
    private Button cancelButton;
    private Button closeButton;
    private Text remainText;
    private Text costText;
    private Image costIcon;

    protected override void InitComponent()
    {
        base.InitComponent();
        confirmButton   = WindowCache.GetComponent<Button>("tip/detail/confirm_btn");
        cancelButton    = WindowCache.GetComponent<Button>("tip/detail/cancel_btn");
        closeButton     = WindowCache.GetComponent<Button>("tip/detail/close");
        remainText      = WindowCache.GetComponent<Text>  ("tip/detail/remain");
        costText        = WindowCache.GetComponent<Text>  ("tip/detail/cost/icon/now");
        costIcon        = WindowCache.GetComponent<Image> ("tip/detail/cost/icon");
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;

        confirmButton   ?.onClick.AddListener(OnConfirm);
        cancelButton    ?.onClick.AddListener(() => UnInitialize());
        closeButton     ?.onClick.AddListener(() => UnInitialize());

        if (moduleAwakeMatch.CurrentTask != null)
        {
            var remainTime = moduleAwakeMatch.maxbuyTimes - moduleAwakeMatch.buyTimes;
            var time = moduleAwakeMatch.buyTimes;
            var costNum = 0;
            ItemPair costItem = null;
            bool isMyself = moduleAwakeMatch.CurrentTask.taskType == TaskType.Nightmare || (moduleAwakeMatch.CurrentTask.taskType == TaskType.Awake && moduleAwakeMatch.CurrentTaskIsActive);
            if (isMyself)
            {
                remainTime = moduleAwakeMatch.CurrentTask.taskConfigInfo.dayRemainCount -
                             moduleAwakeMatch.CurrentTask.taskData.resetTimes;
                time = moduleAwakeMatch.CurrentTask.taskData.resetTimes;
                var arr = moduleAwakeMatch.CurrentTask.taskConfigInfo.costItem;
                if (arr != null)
                {
                    var index = Mathd.Clamp(time, 0, arr.Length - 1);
                    costItem = arr[index];
                }
            }
            else
            {
                var arr = moduleAwakeMatch.costItem;
                if (arr != null)
                {
                    var index = Mathd.Clamp(time, 0, arr.Length - 1);
                    costItem = arr[index];
                }
            }
            if (costItem != null)
            {
                costNum = costItem.count;
                var prop = ConfigManager.Get<PropItemInfo>(costItem.itemId);
                if (prop)
                {
                    AtlasHelper.SetIcons(costIcon, prop.icon);
                }
            }
            Util.SetText(remainText, Util.Format(ConfigText.GetDefalutString((int) TextForMatType.AwakeStage, 43), remainTime));
            Util.SetText(costText,   Util.Format(ConfigText.GetDefalutString((int) TextForMatType.AwakeStage, 44), costNum, modulePlayer.gemCount));
            if(costText)
                costText.color = ColorGroup.GetColor(ColorManagerType.IsMatrialEnough, costNum <= modulePlayer.gemCount);
        }
        return true;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;

        confirmButton   ?.onClick.RemoveAllListeners();
        cancelButton    ?.onClick.RemoveAllListeners();
        closeButton     ?.onClick.RemoveAllListeners();
        return true;
    }
    private void OnConfirm()
    {
        moduleAwakeMatch.RequestBuyEnterTime();
    }
    private void _ME(ModuleEvent<Module_AwakeMatch> e)
    {
        switch (e.moduleEvent)
        {
            case Module_AwakeMatch.Response_BuyEnterTime:
                ResponseBuyTimes(e.msg as ScTeamPveBuyEnterTime);
                break;
        }
    }

    private void ResponseBuyTimes(ScTeamPveBuyEnterTime msg)
    {
        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9812, msg.result);
            return;
        }
        UnInitialize();
    }
}