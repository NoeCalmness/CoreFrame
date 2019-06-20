// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-18      17:40
//  * LastModify：2018-10-18      17:40
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class EquipInfoWindow_ClearSuit : SubWindowBase<Window_Equipinfo>
{
    private SuitProperty suitProperty;
    private Image consumeIcon;
    private Text consumeCount;
    private Text consumeRemain;
    private Transform costRoot;
    private Transform costTemplete;
    private Button clearButton;
    private Button closeButton;

    private bool isMatrialEnough;
    private PItem currentItem;

    protected override void InitComponent()
    {
        base.InitComponent();
        suitProperty    = new SuitProperty(WindowCache.GetComponent<Transform>("tip/content/attr"));
        consumeIcon     = WindowCache.GetComponent<Image>       ("tip/content/consume/icon");
        consumeCount    = WindowCache.GetComponent<Text>        ("tip/content/consume/count");
        consumeRemain   = WindowCache.GetComponent<Text>        ("tip/content/consume/count/remain");
        costRoot        = WindowCache.GetComponent<Transform>   ("tip/content/list");
        costTemplete    = WindowCache.GetComponent<Transform>   ("tip/content/list/0");
        clearButton     = WindowCache.GetComponent<Button>      ("tip/content/clear_Btn");
        closeButton     = WindowCache.GetComponent<Button>      ("tip/content/close");

        costTemplete.SafeSetActive(false);
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;

        currentItem = p[0] as PItem;
        var suitId = currentItem?.growAttr.suitId ?? 0;
        suitProperty.Init(suitId, moduleEquip.GetSuitNumber(suitId), moduleEquip.IsDressOn(currentItem));
        RefreshClearCost(suitId);

        clearButton?.onClick.AddListener(OnClear);
        closeButton?.onClick.AddListener(()=>UnInitialize(false));
        return true;
    }

    private void OnClear()
    {
        if (null == currentItem)
        {
            Logger.LogError("道具为null，不能执行清除操作");
            return;
        }
        moduleFurnace.RequestSublimationClear(currentItem.itemId);
    }

    private void RefreshClearCost(ushort suitId)
    {
        var suitInfo = ConfigManager.Get<SuitInfo>(suitId);
        if (null == suitInfo)
            return;

        Util.SetText(consumeCount, "0");
        Util.SetText(consumeRemain, "0");

        Util.ClearChildren(costRoot);
        for (var i = 0; i < suitInfo.clearCosts.Length; i++)
        {
            var prop = ConfigManager.Get<PropItemInfo>(suitInfo.clearCosts[i].itemId);
            if (suitInfo.clearCosts[i].itemId == 1 || suitInfo.clearCosts[i].itemId == 2)
            {
                AtlasHelper.SetIcons(consumeIcon, prop.icon);
                var a = suitInfo.clearCosts[i].count;
                var b = suitInfo.clearCosts[i].itemId == 1 ? modulePlayer.coinCount : modulePlayer.gemCount;
                Util.SetText(consumeCount, a.ToString());
                Util.SetText(consumeRemain, Util.Format(ConfigText.GetDefalutString(249, 19), b));
                isMatrialEnough = isMatrialEnough && a <= b;
                consumeCount.color = ColorGroup.GetColor(ColorManagerType.IsMoneyEnough, a <= b);
                continue;
            }

            var t = costRoot.AddNewChild(costTemplete);
            t.SafeSetActive(true);
            Util.SetItemInfo(t, prop);
            BindItemInfo(t, prop.ID, suitInfo.clearCosts[i].count);
        }
    }


    private void BindItemInfo(Transform t, int itemId, int count)
    {
        var itemInfo = ConfigManager.Get<PropItemInfo>(itemId);
        Util.SetItemInfo(t, itemInfo);
        var countText = t.GetComponent<Text>("numberdi/count");
        var own = moduleEquip.GetPropCount(itemId);
        Util.SetText(countText, $"{own}/{count}");
        countText.color = ColorGroup.GetColor(ColorManagerType.IsMatrialEnough, count <= own);
        isMatrialEnough = isMatrialEnough && count <= own;
        t.GetComponentDefault<Button>()?.onClick.RemoveAllListeners();
        t.GetComponentDefault<Button>()?.onClick.AddListener( () => moduleGlobal.SetTargetMatrial(itemId, count, Window_Equipinfo.SUB_TYPE_CLEAR));
    }

    private void _ME(ModuleEvent<Module_Furnace> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Furnace.ResponseSublimationClear:
                ResponseSublimationClear(e.msg as ScSublimationClear);
                break;
        }
    }

    private void ResponseSublimationClear(ScSublimationClear msg)
    {
        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9753, msg.result);
            return;
        }
        AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
        closeButton.onClick.Invoke();
    }
}
