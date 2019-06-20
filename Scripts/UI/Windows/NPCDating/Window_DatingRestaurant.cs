/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-12-26
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_DatingRestaurant : Window
{
    private Transform m_tfPOrderItems;
    private Button m_btnPageUp, m_btnPageDown;
    private Text m_textLeftPageNum, m_textRightPageNum;//页码

    //提示
    private Transform m_tfTip;
    private Text m_textTipContent;
    private Button m_btnTipOK;

    private int m_curIndex = 0;
    private int m_nPerRefreshCount = 4;//每一页刷新的菜品数量
    private ShopMessage m_shopMessage;
    private Dictionary<int, List<PShopItem>> m_segmentDataDic = new Dictionary<int, List<PShopItem>>();
    private Dictionary<int, PShopItem> m_itemDataDic = new Dictionary<int, PShopItem>();

    protected override void OnOpen()
    {
        m_tfPOrderItems = GetComponent<RectTransform>("menuItems");
        m_nPerRefreshCount = m_tfPOrderItems.childCount;
        for (int i = 0; i < m_tfPOrderItems.childCount; i++)
        {
            var button = m_tfPOrderItems.GetChild(i).GetComponent<Button>("bug_Btn");
            button?.onClick.AddListener(() => OnClickBuy(button));
        }

        m_btnPageUp = GetComponent<Button>("pageUp"); m_btnPageUp.onClick.AddListener(OnClickPageUp);
        m_btnPageDown = GetComponent<Button>("pageDown"); m_btnPageDown.onClick.AddListener(OnClickPageDown);
        m_textLeftPageNum = GetComponent<Text>("pageUp/pageNumber");
        m_textRightPageNum = GetComponent<Text>("pageDown/pageNumber");

        //提示
        m_tfTip = GetComponent<RectTransform>("tip"); m_tfTip.SafeSetActive(false);
        m_textTipContent = GetComponent<Text>("tip/panel/content");
        m_btnTipOK = GetComponent<Button>("tip/panel/btnOK"); m_btnTipOK.onClick.AddListener(() => m_tfTip.SafeSetActive(false));

        InitText();
    }

    protected override void OnHide(bool forward)
    {
        m_shopMessage = null;
        m_segmentDataDic.Clear();
        m_itemDataDic.Clear();

        moduleNPCDating.ContinueBehaviourCallBack();
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        //设置商店类型
        moduleShop.SetCurShopPos(ShopPos.NpcDating);
    }

    private void InitText()
    {
        var textData = ConfigManager.Get<ConfigText>((int)TextForMatType.NpcDatingRest);
        if (textData == null) return;
        Util.SetText(GetComponent<Text>("orderName"), textData[0]);
        Util.SetText(GetComponent<Text>("pageUp/Text"), textData[3]);
        Util.SetText(GetComponent<Text>("pageDown/Text"), textData[4]);

        Util.SetText(GetComponent<Text>("tip/panel/title"), textData[8]);
        Util.SetText(GetComponent<Text>("tip/panel/btnOK/label"), textData[9]);
    }

    void _ME(ModuleEvent<Module_Shop> e)
    {
        if (moduleShop.curShopPos != ShopPos.NpcDating) return;
        switch (e.moduleEvent)
        {
            case Module_Shop.EventShopData:
                var list = e.param1 as List<ShopMessage>;
                if (list == null || list.Count < 1) break;
                moduleShop.SetCurrentShop(list[0]);
                break;
            case Module_Shop.EventTargetShopData:
                m_shopMessage = (ShopMessage)e.param1;
                List<PShopItem> tempList = new List<PShopItem>(m_shopMessage.items);
                moduleNPCDating.HotelShopItemList = tempList;

                _Index(1);//默认刷新第一页
                break;
            case Module_Shop.EventPaySuccess:
                Window_ItemTip.Show(ConfigText.GetDefalutString(TextForMatType.TravalShopUIText, 13), moduleShop.curClickItem);
                Hide<Window_DatingRestaurant>();//关闭菜单界面
                break;
            default:
                break;
        }
    }

    void _ME(ModuleEvent<Module_Story> e)
    {
        if (!actived) return;
        if (e.moduleEvent == Module_Story.EventStoryEnd)
        {
            if ((EnumStoryType)e.param2 != EnumStoryType.NpcTheatreStory) return;
            moduleGlobal.ShowGlobalLayerDefault(2, false);
        }
    }

    private void OnClickPageUp()
    {
        Index--;
    }

    private void OnClickPageDown()
    {
        Index++;
    }

    private int Index
    {
        get { return m_curIndex; }
        set {_Index(value);}
    }

    private void _Index(int val)
    {
        List<PShopItem> tmpDataList = moduleNPCDating.HotelShopItemList;
        if (tmpDataList == null || tmpDataList.Count == 0) return;

        m_curIndex = val;

        if (m_curIndex < 1)
        {
            m_curIndex = 1;
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString((int)TextForMatType.NpcDatingRest,5));//第一页提示
            return;
        }

        int maxPage = Mathf.CeilToInt((float)tmpDataList.Count / m_nPerRefreshCount);
        if (m_curIndex > maxPage)
        {
            m_curIndex = maxPage;
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString((int)TextForMatType.NpcDatingRest, 6));//最后一页提示
            return;
        }

        List<PShopItem> segmentData = new List<PShopItem>();
        if (m_segmentDataDic.ContainsKey(m_curIndex)) segmentData = m_segmentDataDic[m_curIndex];
        else
        {
            segmentData = SpliceShopItemList(tmpDataList);
            m_segmentDataDic.Add(m_curIndex, segmentData);
        }

        RefreshData(segmentData);

        ShowPage(m_curIndex, maxPage);
    }

    /// <summary>
    /// 显示页码
    /// </summary>
    /// <param name="curPage">当前页码</param>
    /// <param name="totalPage">总页码</param>
    private void ShowPage(int curPage,int totalPage)
    {
        int leftPage = curPage * 2 - 1;
        int rightPage = curPage * 2;
        Util.SetText(m_textLeftPageNum, leftPage.ToString());
        Util.SetText(m_textRightPageNum, rightPage.ToString());
    }

    private void RefreshData(List<PShopItem> dataList)
    {
        if (dataList == null || dataList.Count == 0)
        {
            for (int i = dataList.Count; i < m_tfPOrderItems.childCount; i++)
            {
                m_tfPOrderItems.GetChild(i).SafeSetActive(false);
            }
            return;
        }

        for (int i = 0; i < m_tfPOrderItems.childCount; i++)
        {
            Transform tfItem = m_tfPOrderItems.GetChild(i);
            if (i <= dataList.Count - 1)
            {
                tfItem.SafeSetActive(true);
                SetItemData(tfItem, dataList[i]);
            }
            else tfItem.SafeSetActive(false);
        }

    }

    private void SetItemData(Transform node, PShopItem data)
    {
        if (data == null) return;

        PropItemInfo psi = ConfigManager.Get<PropItemInfo>(data.itemTypeId);
        if (psi == null) return;

        Image icon = node.Find("icon").GetComponent<Image>();
        Text orderName = node.Find("title").GetComponent<Text>();
        Image priceIcon = node.Find("consume/number/icon").GetComponent<Image>();
        Text price = node.Find("consume/number").GetComponent<Text>();
        Image newMark = node.Find("newInfo").GetComponent<Image>();
        Button btnBuy = node.Find("bug_Btn").GetComponent<Button>();
        Text textBtnBuy = node.Find("bug_Btn/buy_Txt").GetComponent<Text>();
        Text textdesc = node.Find("des").GetComponent<Text>();
        Util.SetText(textBtnBuy, ConfigText.GetDefalutString(TextForMatType.NpcDatingRest, 1));

        Util.SetText(textdesc, psi.desc);//菜品描述
        Util.SetText(orderName, psi.itemNameId);
        AtlasHelper.SetItemIcon(icon, psi);//菜品图标

        PropItemInfo itemInfo = ConfigManager.Get<PropItemInfo>(data.currencyType);
        AtlasHelper.SetItemIcon(priceIcon, itemInfo);//设置货币的icon
        string strPrice = "×" + data.currencyNum;

        //如果是免费的，则显示免费字样
        priceIcon.SafeSetActive(data.currencyNum > 0);
        if (data.currencyNum > 0) Util.SetText(price, strPrice);
        else Util.SetText(price, ConfigText.GetDefalutString(TextForMatType.NpcDatingRest, 2));//免费字样看文本

        bool enoughMoney = CheckMoney(data.currencyType, data.currencyNum);
        price.color = ColorGroup.GetColor(ColorManagerType.NpcDatingRestOrder, enoughMoney);

        newMark.SafeSetActive(data.isNew == 0);

        var btnIndex = btnBuy.transform.parent.GetSiblingIndex();
        if (m_itemDataDic.ContainsKey(btnIndex)) m_itemDataDic.Remove(btnIndex);
        m_itemDataDic.Add(btnIndex, data);
    }

    private void OnClickBuy(Button button)
    {
        int btnIndex = button.transform.parent.GetSiblingIndex();
        if (m_itemDataDic.ContainsKey(btnIndex))
        {
            moduleNPCDating.curClickOrderData = m_itemDataDic[btnIndex];
            bool enoughMoney = CheckMoney(moduleNPCDating.curClickOrderData.currencyType, moduleNPCDating.curClickOrderData.currencyNum);
            if (enoughMoney) moduleShop.SendBuyInfo(m_shopMessage.shopId, moduleNPCDating.curClickOrderData.itemTypeId, moduleNPCDating.curClickOrderData.num);
            else
            {
                m_tfTip.SafeSetActive(true);
                Util.SetText(m_textTipContent, ConfigText.GetDefalutString((int)TextForMatType.NpcDatingRest, 7));//货币不足
            }
        }
    }

    private bool CheckMoney(int itemId, uint priceNum)
    {
        uint moneyCount = modulePlayer.GetCount(itemId);
        return moneyCount >= priceNum;
    }

    private List<PShopItem> SpliceShopItemList(List<PShopItem> allDataList)
    {
        int startIndex = (m_curIndex-1) * m_nPerRefreshCount;
        int count = allDataList.Count - startIndex < m_nPerRefreshCount ? allDataList.Count - startIndex : m_nPerRefreshCount;
        return allDataList.GetRange(startIndex, count);
    }
}