// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-11-07      17:05
//  *LastModify：2018-11-07      17:05
//  ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

public class TargetMatrialBehaviour : MonoBehaviour
{
    public Transform IconRoot;
    public Text GetCount;
    public Text OwnCount;

    private bool isInitComponent = false;

    private void Awake()
    {
        InitComponent();
    }

    private void InitComponent()
    {
        if (isInitComponent) return;
        isInitComponent = true;
        IconRoot = transform.GetComponent<Transform>("item");
        GetCount = transform.GetComponent<Text>("get_txt/number_txt");
        OwnCount = transform.GetComponent<Text>("need_txt/number_txt");

        Util.SetText(GetCount, "0");
        Util.SetText(OwnCount, "0");
    }

    public void Init(PReward reward, Module_Global.TargetMatrial target)
    {
        InitComponent();
        var prop = ConfigManager.Get<PropItemInfo>(target.itemId);
        if (null == prop)
            return;
        var getCount = 0;
        Util.SetItemInfo(IconRoot, prop);
             if (prop.itemType == PropType.Currency && prop.subType == (int) CurrencySubType.Gold)
            getCount = reward.coin;
        else if (prop.itemType == PropType.Currency && prop.subType == (int) CurrencySubType.Diamond)
            getCount = reward.diamond;
        else
        {
            var p = Array.Find(reward.rewardList, item => item.itemTypeId == target.itemId);
            getCount = (int)(p?.num ?? 0);
        }

        Util.SetText(GetCount, getCount.ToString());
        Util.SetText(OwnCount, $"{target.OwnCount}/{target.targetCount}");
        target.isFinish = target.OwnCount >= target.targetCount;
        OwnCount.color = ColorGroup.GetColor(ColorManagerType.IsMatrialEnough, target.isFinish);
    }
}
