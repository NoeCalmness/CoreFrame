/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Launcher class.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-05
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[AddComponentMenu("HYLR/UI/Charge Item Info")]
public class ChargeItemInfo : MonoBehaviour
{
    private static PChargeItem emptyItem
    {
        get
        {
            if (m_emptyItem == null) m_emptyItem = PacketObject.Create<PChargeItem>();
            return m_emptyItem;
        }
    }
    private static PChargeItem m_emptyItem = null;

    public System.Action<ChargeItemInfo> onSelect;

    public PChargeItem item
    {
        get { return m_item; }
        set
        {
            if (m_item == value) return;

            m_item = value == null ? emptyItem : value;
            UpdateItemInfo();
        }
    }
    private PChargeItem m_item;

    public TweenPosition tween { get { return m_tween; } }
    private TweenPosition m_tween;

    public bool selected { set { if (value) onSelect?.Invoke(this); m_toggle.enabled = value; } }

     private Text       desc;
    private Text        number;
    private Text        symbol;
    private Image       bigIcon;
    private Text        gotNumber;
    private Image       m_toggle;
    private Transform   doubleReward;

    private void Awake()
    {
        if (m_item == null) m_item = emptyItem;

        desc             = transform.GetComponent<Text>  ("des_Txt");
        number           = transform.GetComponent<Text>  ("xiaxian/number");
        symbol           = transform.GetComponent<Text>  ("xiaxian/number/type");
        bigIcon          = transform.GetComponent<Image> ("frame/icon");
        gotNumber        = transform.GetComponent<Text>  ("frame/totalNumber");
        doubleReward     = transform.Find("title_Img");
        m_toggle         = transform.GetComponent<Image>("selected");
        m_tween          = transform.GetComponent<TweenPosition>("highlight");

        m_toggle.enabled = false;
        var b = GetComponent<Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() => selected = true);

        UpdateItemInfo();
    }

    private void OnDestroy()
    {
        m_item = null;
        onSelect = null;
    }

    public void UpdateItemInfo()
    {
        if (item?.info == null)      return;

        Util.SetText(desc, item.info.desc);
        Util.SetText(number, item.info.cost.ToString());
        Util.SetText(symbol, Util.GetChargeCurrencySymbol((ChargeCurrencyTypes)item.info.currencyType));
        if (!string.IsNullOrEmpty(item.info.icon))
            AtlasHelper.SetChargeLarge(bigIcon, item.info.icon);
        Util.SetText(gotNumber, item.info.reward.diamond.ToString());
        doubleReward?.SafeSetActive(item.TotalBuyTimes() == 0);
    }
}
