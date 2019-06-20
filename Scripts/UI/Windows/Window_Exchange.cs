// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-01-03      16:24
//  *LastModify：2019-01-03      16:24
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class Window_Exchange : Window
{
    public struct ExchangeContent
    {
        /// <summary>
        /// 物品ID
        /// </summary>
        public int itemId;
        /// <summary>
        /// 需求数量
        /// </summary>
        public int demandCount;
        /// <summary>
        /// 拥有数量
        /// </summary>
        public int ownCount;
    }

    private Button exchangeButton;
    private Button dropButton;
    private Button closeButton;
    private Text needCountText;
    private Text priceText;
    private Text levelText;
    private Transform itemRoot;
    private ExchangeContent _content;

    private int BuyCount
    {
        get { return _content.demandCount - _content.ownCount; }
    }

    private bool IsMoneyEnough
    {
        get
        {
            var prop = ConfigManager.Get<PropItemInfo>(_content.itemId);
            if (null == prop) return false;
            return BuyCount * prop.diamonds <= modulePlayer.gemCount;
        }
    }


    private void InitComponent()
    {
        closeButton     = GetComponent<Button>      ("top/global_tip_button");
        itemRoot        = GetComponent<Transform>   ("middle");
        needCountText   = GetComponent<Text>        ("middle/need/count");
        priceText       = GetComponent<Text>        ("middle/price/count");
        levelText       = GetComponent<Text>        ("middle/level/count");
        exchangeButton  = GetComponent<Button>      ("charge_Btn");
        dropButton      = GetComponent<Button>      ("get_Btn");
    }

    private void MultiLanguage()
    {
        Util.SetText(GetComponent<Text>("top/equipinfo"), ConfigText.GetDefalutString(199, 3));
        Util.SetText(GetComponent<Text>("charge_Btn/get_Txt"), ConfigText.GetDefalutString(199, 4));
        Util.SetText(GetComponent<Text>("get_Btn/get_Txt"), ConfigText.GetDefalutString(199, 5));
    }

    protected override void OnOpen()
    {
        base.OnOpen();

        InitComponent();

        MultiLanguage();

        isFullScreen = false;

        exchangeButton?.onClick.AddListener(OnExchange);
        dropButton?.onClick.AddListener(() =>
        {
            Hide();
            moduleGlobal.DispatchModuleEvent(Module_Global.EventShowDropChase, _content.itemId);
        });

        closeButton?.onClick.AddListener( ()=> Hide());
    }


    private void OnExchange()
    {
        var prop = ConfigManager.Get<PropItemInfo>(_content.itemId);
        if (prop == null)
            return;

        Window_Alert.ShowAlertDefalut(Util.Format(ConfigText.GetDefalutString(199, 0), BuyCount * prop.diamonds, BuyCount, prop.itemName), () =>
        {
            moduleShop.FreeBuy((ushort)_content.itemId, (uint)BuyCount);
        }, ()=> {}, ConfigText.GetDefalutString(199, 1), ConfigText.GetDefalutString(199, 2));
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        var args = GetWindowParam(name);

        if (args != null && args.param1 is ExchangeContent)
            _content = (ExchangeContent)args.param1;

        var prop = ConfigManager.Get<PropItemInfo>(_content.itemId);
        Util.SetItemInfo(itemRoot       , prop, 0, _content.ownCount, false);
        Util.SetText(itemRoot.GetComponent<Text>("numberdi/count"),
        Util.Format(ConfigText.GetDefalutString(199, 8), _content.ownCount));
        Util.SetText    (needCountText  , Util.Format(ConfigText.GetDefalutString(199, 6), BuyCount.ToString()));
        Util.SetText    (priceText      , Util.Format(ConfigText.GetDefalutString(199, 7), BuyCount * prop.diamonds));
        Util.SetText    (levelText      , Util.Format(ConfigText.GetDefalutString(199, 10), prop.buyLevel));
        priceText.color = ColorGroup.GetColor(ColorManagerType.IsMoneyEnough, IsMoneyEnough);
        bool level = modulePlayer.level >= prop.buyLevel ? true : false;
        levelText.color = level ? Color.white : Color.red;

        exchangeButton?.SetInteractable(IsMoneyEnough && level);
    }

    private void _ME(ModuleEvent<Module_Shop> e)
    {
        if (e.moduleEvent == Module_Shop.ResponseFreeBuy)
        {
            if (((e.msg as ScFreeBuy)?.result ?? -1) == 0)
            {
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(199, 9));
                Hide();
            }
        }
    }

    #region static

    public static void Show(ExchangeContent rContent)
    {
        SetWindowParam<Window_Exchange>(rContent);
        ShowAsync<Window_Exchange>();
    }
    #endregion

}
