// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-18      15:27
//  * LastModify：2018-10-18      15:27
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class FurnaceWindow_SublimationSelectDrawing : SubWindowBase<Window_Sublimation>
{
    private Button comfirmButton;
    private Button closeButton;
    private Button bgButton;
    private Button pathButton;
    private Text  itemName;
    private Text  itemDesc;
    private ItemPair drawingItem;
    private ScrollView scrollView;
    private Transform selectBox;
    private Transform currentNode;

    protected DataSource<ItemPair> dataSource;

    protected override void InitComponent()
    {
        base.InitComponent();

        comfirmButton = WindowCache.GetComponent<Button>("tip/content/yes_button");
        closeButton   = WindowCache.GetComponent<Button>("tip/content/close_button");
        bgButton      = WindowCache.GetComponent<Button>("tip/bg");
        pathButton    = WindowCache.GetComponent<Button>("tip/content/getBtn");
        scrollView    = WindowCache.GetComponent<ScrollView>("tip/content/scrollView");
        itemName      = WindowCache.GetComponent<Text>("tip/content/name");
        itemDesc      = WindowCache.GetComponent<Text>("tip/content/des");
        selectBox     = WindowCache.GetComponent<Transform>("tip/content/scrollView/selectbox");
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;

        currentNode = null;
        Util.SetText(itemName, string.Empty);
        Util.SetText(itemDesc, string.Empty);
        selectBox?.SafeSetActive(false);
        comfirmButton?.onClick.AddListener(OnConfirm);
        closeButton?.onClick.AddListener(() => UnInitialize(false));
        bgButton?.onClick.AddListener(() => UnInitialize(false));
        pathButton?.onClick.AddListener(() =>
        {
            if (drawingItem == null)
                return;
            moduleGlobal.UpdateGlobalTip((ushort)drawingItem.itemId);
        });
        dataSource = new DataSource<ItemPair>(moduleFurnace.GetAllDrawingList(null), scrollView, OnSetData, OnItemClick);
        return true;
    }


    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;

        selectBox?.SetParent(scrollView?.transform);
        selectBox?.SafeSetActive(false);

        comfirmButton?.onClick.RemoveAllListeners();
        closeButton  ?.onClick.RemoveAllListeners();
        return true;
    }

    private void OnConfirm()
    {
        if (null == drawingItem)
            return;
        parentWindow.BindDrawingItem(drawingItem.itemId);
        UnInitialize();
    }

    private void OnItemClick(RectTransform node, ItemPair data)
    {
        OnSelectItem(node, data);
    }

    private void OnSelectItem(RectTransform node, ItemPair data)
    {
        drawingItem = data;
        currentNode = node;
        var prop = ConfigManager.Get<PropItemInfo>(data.itemId);
        if (null == prop)
            return;

        selectBox?.SetParent(node);
        selectBox?.SafeSetActive(true);
        if(null != selectBox)
            selectBox.localPosition = Vector3.zero;
        Util.SetText(itemName, prop.itemName);
        Util.SetText(itemDesc, prop.desc);
    }

    private void OnSetData(RectTransform node, ItemPair data)
    {
        Util.SetItemInfo(node, ConfigManager.Get<PropItemInfo>(data.itemId), 0, data.count);
        if (currentNode == null)
        {
            OnSelectItem(node, data);
        }
    }
}
