using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : BaseRestrain
{
    public bool isSelled { get; private set; }
    public bool isHaved { get; private set; }
    public bool isEquip { get; private set; }

    Transform wupin;
    Text _name;
    Text count;
    Transform cost_panel;
    Image payType;
    Text cost;
    Transform discountCostPanel;
    Text originalCost;
    Text nowCost;
    Text discountImage;
    Image grayPanel;
    Text alreadyHave;
    Text alreadySelled;
    Button tip_btn;
    Transform selectBox;
    Transform offImage;
    Transform alreadyEquip;

    private bool isInite;

    private void IniteCompent()
    {
        if (isInite) return;

        wupin             = transform.Find("item");
        _name             = transform.Find("item/name")?.GetComponent<Text>();
        count             = transform.Find("item/totalNumber")?.GetComponent<Text>();
        cost_panel        = transform.Find("priceinfo/cost");
        payType           = transform.Find("priceinfo/cost/PayType")?.GetComponent<Image>();
        cost              = transform.Find("priceinfo/cost/cost_text")?.GetComponent<Text>();
        discountCostPanel = transform.Find("priceinfo/cost/huaxian");
        originalCost      = transform.Find("priceinfo/cost/huaxian/yuanjia")?.GetComponent<Text>();
        nowCost           = transform.Find("priceinfo/cost/huaxian/zhekoujia")?.GetComponent<Text>();
        discountImage     = transform.Find("priceinfo/dazhe/Text")?.GetComponent<Text>();
        grayPanel         = transform.Find("priceinfo/Panel")?.GetComponent<Image>();
        alreadyHave       = transform.Find("priceinfo/Panel/yiyongyou")?.GetComponent<Text>();
        alreadySelled     = transform.Find("priceinfo/Panel/yishoujing")?.GetComponent<Text>();
        tip_btn           = transform.Find("item/info")?.GetComponentDefault<Button>();
        selectBox         = transform.Find("selectBox");
        offImage          = transform.Find("off");
        alreadyEquip      = transform.Find("bg/get");
        isInite = true;
    }

    public void RefreshUiData(ShopMessage msg, PShopItem data)
    {
        IniteCompent();

        var info = ConfigManager.Get<PropItemInfo>(data.itemTypeId);
        if (info == null) return;
        Util.SetItemInfoSimple(wupin, info);
        if (msg.pos == ShopPos.Npc) AtlasHelper.SetIcons(wupin?.Find("icon"), info.mesh != null && info.mesh.Length > 1 ? info.mesh[1] : info.icon);
        _name.text = info.itemName;
        count.text = "×" + data.num.ToString();
        Util.SetText(alreadyHave, (int)TextForMatType.TravalShopUIText, 9);
        Util.SetText(alreadySelled, (int)TextForMatType.TravalShopUIText, 10);
        Util.SetText(discountImage, (int)TextForMatType.TravalShopUIText, 12);
        cost_panel.SafeSetActive(true);
        grayPanel.SafeSetActive(false);
        //正常
        var currencyType = ConfigManager.Get<PropItemInfo>(data.currencyType);
        if (currencyType && payType)
        {
            payType.SafeSetActive(true);
            AtlasHelper.SetItemIcon(payType, currencyType);
        }
        cost.text = "×" + data.currencyNum.ToString();

        tip_btn.onClick.RemoveAllListeners();
        tip_btn.onClick.AddListener(() => { Module_Global.instance.UpdateGlobalTip(data.itemTypeId, true); });
        //促销
        int hash = Module_Shop.instance.GetPromoHash(msg.shopId, data.itemTypeId, data.num);

        PShopPromotion promotion = Module_Shop.instance.allPromo.Get(hash);

        if (promotion != null)
        {
            discountCostPanel.SafeSetActive(true);
            originalCost.text = "×" + data.currencyNum.ToString();
            if (promotion != null) nowCost.text = "×" + promotion.price.ToString();
            discountImage.transform.parent.SafeSetActive(true);
            cost.SafeSetActive(false);
        }
        else
        {
            discountCostPanel.SafeSetActive(false);
            cost.SafeSetActive(true);
            discountImage.transform.parent.SafeSetActive(false);
        }
        isHaved = false;
        isSelled = false;

        //随机商店显示已售罄
        if (msg.isRandom)
        {
            isSelled = data.buy == 1;

            cost_panel.SafeSetActive(!isSelled);
            grayPanel.SafeSetActive(isSelled);
            alreadyHave.SafeSetActive(false);
            alreadySelled.SafeSetActive(isSelled);
        }

        //如果是时装显示已拥有&&不能是子类型为8的时装,为8的要显示已售罄
        var have = (info.itemType == PropType.FashionCloth && (FashionSubType)info.subType != FashionSubType.FourPieceSuit) || (info.itemType == PropType.HeadAvatar && info.subType == 1);
        if (have)
        {
            isHaved = Module_Equip.instance.HasProp(data.itemTypeId);

            cost_panel.SafeSetActive(!isHaved);
            grayPanel.SafeSetActive(isHaved);
            alreadyHave.SafeSetActive(isHaved);
            alreadySelled.SafeSetActive(false);
        }

        if (msg.pos == ShopPos.Npc && Module_Npc.instance.curNpc != null && Module_Npc.instance.curNpc.npcInfo != null)
        {
            isEquip = Module_Npc.instance.curNpc._mode == data.itemTypeId;
            isHaved = Module_Equip.instance.HasProp(data.itemTypeId) || isEquip || Module_Npc.instance.curNpc.npcInfo.cloth == data.itemTypeId;

            cost_panel.SafeSetActive(!isHaved);
            grayPanel.SafeSetActive(isHaved);
            grayPanel.enabled = !isHaved;
            alreadyHave.SafeSetActive(isHaved);
            alreadySelled.SafeSetActive(false);
            offImage.SafeSetActive(!isHaved);
            alreadyEquip.SafeSetActive(isEquip);
            //选中框
            if (isEquip && Module_Shop.instance.lastClickItem == null) Module_Shop.instance.curClickItem = data;
        }

        selectBox.SafeSetActive(Module_Shop.instance.curClickItem != null && Module_Shop.instance.curClickItem == data);
    }
}