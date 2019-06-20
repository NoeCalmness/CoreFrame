/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-02-20
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class Window_DatingGift : Window
{
    private Transform m_tfHaveGoods;
    private Transform m_tfNoneGoods;
    private ScrollView m_svScrollView;
    private DataSource<PItem> m_dataSource;

    private Text m_textDesc;
    private Button m_btnSendGift;

    private Button m_btnClose;

    private PItem m_curClickData = null;

    protected override void OnOpen()
    {
        m_tfHaveGoods = GetComponent<RectTransform>("haveGoods");
        m_tfNoneGoods = GetComponent<RectTransform>("noneGoods");

        m_svScrollView = GetComponent<ScrollView>("haveGoods/itemList");
        m_dataSource = new DataSource<PItem>(null, m_svScrollView, OnSetData, OnClickItem);

        m_textDesc = GetComponent<Text>("haveGoods/itemDesc");
        m_btnSendGift = GetComponent<Button>("haveGoods/giveBtn"); m_btnSendGift.onClick.AddListener(OnClickSendGift);
        
        m_btnClose = GetComponent<Button>("bg"); m_btnClose.onClick.AddListener(()=> { Hide<Window_DatingGift>(); });

        InitTextComponent();
    }

    protected override void OnShow(bool forward)
    {
        ClearData();
        Refresh();
    }

    private void InitTextComponent()
    {
        var textData = ConfigManager.Get<ConfigText>((int)TextForMatType.NpcDatingGift);
        if (textData == null) return;

        Util.SetText(GetComponent<Text>("noneGoods/tipback/Text"), textData[0]);
        Util.SetText(GetComponent<Text>("haveGoods/giveBtn/Text"), textData[1]);
    }

    private void _ME(ModuleEvent<Module_DatingGift> e)
    {
        if (e.moduleEvent == Module_DatingGift.EVENT_SENDGIFT_SUCEESE_NAME)
        {
            m_dataSource.UpdateItems();
        }
    }

    private void OnSetData(RectTransform node, PItem data)
    {
        if (data == null) return;

        var info = ConfigManager.Get<PropItemInfo>(data.itemTypeId);
        Util.SetItemInfo(node, info, 0, (int)data.num, false);

        var select = node.Find("selectBox");
        select.SafeSetActive(m_curClickData != null && m_curClickData.itemId == data.itemId);
    }

    private void OnClickItem(RectTransform node, PItem data)
    {
        if (m_curClickData != null && m_curClickData.itemId == data.itemId) return;
        m_curClickData = data;
        m_dataSource.UpdateItems();//点击之后显示选择框，这里待优化。可以尝试使用toggle
        RefreshDdesc(data);
    }

    private void Refresh()
    {
        var list = moduleDatingGift.ItemDatas;

        m_tfHaveGoods.SafeSetActive(list.Count > 0);
        m_tfNoneGoods.SafeSetActive(list == null || list.Count == 0);
        m_dataSource.SetItems(list);
    }

    private void RefreshDdesc(PItem data)
    {
        var p = data.GetPropItem();
        if (p == null) return;
        Util.SetText(m_textDesc, p.desc);
    }


    private void OnClickSendGift()
    {
        if(m_curClickData!=null) moduleDatingGift.SendGift(m_curClickData.itemTypeId);
    }

    private void ClearData()
    {
        m_dataSource.Clear();
        m_curClickData = null;
        m_textDesc.text = "";
    }
}
