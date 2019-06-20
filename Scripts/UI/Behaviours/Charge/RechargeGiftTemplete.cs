// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-21      15:13
//  * LastModify：2018-08-21      15:13
//  ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RechargeGiftTemplete : MonoBehaviour
{
    private Text        nameText;
    private Text        desc;
    private Text        costNumber;
    private Text        symbol;
    private Text        remainTime;
    private Text        limitLevel;
    private Transform   gotNode;
    private Image       icon;
    private Button      detailButton;
    private Image       toggle;

    public PChargeItem Data;
    public Action<RechargeGiftTemplete> onSelect;
    public Action<RechargeGiftTemplete> onDetail;
    public List<ItemPair> previewList = new List<ItemPair>();
    private bool inited;

    public bool selected { set { if (value && !toggle.enabled) onSelect?.Invoke(this); toggle.enabled = value; } }


    private void Awake()
    {
        if (inited)
            return;
        inited = true;
        nameText         = transform.GetComponent<Text>     ("name_Txt");
        desc             = transform.GetComponent<Text>     ("des_Txt");
        costNumber       = transform.GetComponent<Text>     ("xiaxian/number");
        symbol           = transform.GetComponent<Text>     ("xiaxian/number/type");
        limitLevel       = transform.GetComponent<Text>     ("halfframe_Img/halfframe_Txt");
        gotNode          = transform.GetComponent<Transform>("xiaxian/alreadyget_Txt");
        remainTime       = transform.GetComponent<Text>     ("remaintime_Txt");
        icon             = transform.GetComponent<Image>    ("frame/icon");
        detailButton     = transform.GetComponent<Button>   ("info_Btn");
        toggle           = transform.GetComponent<Image>    ("selected");

        toggle.enabled = false;
        var b = GetComponent<Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() => selected = true);
    }

    public void InitData(PChargeItem item)
    {
        if (!inited)
            Awake();
        Data = item;
        Refresh();

        previewList.Clear();
        var itemArr = item.info.reward?.rewardList;
        if (itemArr != null && itemArr.Length > 0)
        {
            for (int i = 0; i < itemArr.Length; i++)
            {
                var gift = ConfigManager.Get<PropItemInfo>(itemArr[i].itemTypeId);
                if(gift && gift.previewItems != null)
                    previewList.AddRange(gift.previewItems);
            }
        }
        detailButton?.SafeSetActive(previewList.Count > 0);
        detailButton?.onClick.RemoveAllListeners();
        detailButton?.onClick.AddListener(() =>
        {
            onDetail?.Invoke(this);
        });
    }

    public void Refresh()
    {
        Util.SetText(nameText, Data.info.name);
        Util.SetText(desc, Data.info.desc);
        Util.SetText(costNumber, Data.info.cost.ToString());
        Util.SetText(symbol, Util.GetChargeCurrencySymbol((ChargeCurrencyTypes) Data.info.currencyType));
        Util.SetText(remainTime,
            Util.Format(ConfigText.GetDefalutString(TextForMatType.RechargeUIText, 2),
                Util.GetTimeStampDiff((int) Data.info.endTime, true)));
        Util.SetText(limitLevel,
            Util.Format(ConfigText.GetDefalutString(TextForMatType.RechargeUIText, 3), Data.info.minLevelLimit,
                Data.info.maxLevelLimit));

        remainTime.SafeSetActive(Data.info.endTime > 0);
        limitLevel.SafeSetActive(Data.info.maxLevelLimit > 0);

        if (!string.IsNullOrEmpty(Data.info.icon))
        {
            AtlasHelper.SetChargeLarge(icon, Data.info.icon);
        }
        else
        {
            icon.SafeSetActive(false);
            Logger.LogError($"{Data.info.name} 无效的道具图标 {Data.info.icon}");
        }

        RefreshCount();
    }

    public void RefreshCount()
    {
        gotNode.SafeSetActive((Data.info.cycleTotalBuyLimit > 0 && Data.DailyBuyTimes() >= Data.info.cycleTotalBuyLimit) || 
                              (Data.info.allTotalBuyLimit > 0 && Data.TotalBuyTimes() >= Data.info.allTotalBuyLimit));
        costNumber.SafeSetActive(!gotNode.gameObject.activeSelf);
    }

    private void Update()
    {
        if (Data == null) return;

        Util.SetText(remainTime, Util.Format(ConfigText.GetDefalutString(TextForMatType.RechargeUIText, 2), Util.GetTimeStampDiff((int)Data.info.endTime, true)));
    }
}
