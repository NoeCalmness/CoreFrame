// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-23      16:15
//  * LastModify：2018-08-23      16:15
//  ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChargeWindow_Detail : SubWindowBase
{
    private ScrollView scrollView;
    private Button confirmButton;
    private Button closeButton;

    protected DataSource<ItemPair> dataSource;

    protected override void InitComponent()
    {
        base.InitComponent();
        this.scrollView    = WindowCache.GetComponent<ScrollView>("preview_panel/scrollView");
        this.confirmButton = WindowCache.GetComponent<Button>("preview_panel/confirm_button");
        this.closeButton   = WindowCache.GetComponent<Button>("preview_panel/bg/equip_prop/top/button");
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;

        confirmButton?.onClick.RemoveAllListeners();
        confirmButton?.onClick.AddListener(() => UnInitialize(false));
        closeButton  ?.onClick.RemoveAllListeners();
        closeButton  ?.onClick.AddListener(() => UnInitialize(false));

        var list = p[0] as List<ItemPair>;
        list?.RemoveAll(item =>
        {
            var prop = ConfigManager.Get<PropItemInfo>(item.itemId);
            return !(prop && prop.proto != null && (prop.proto.Contains(CreatureVocationType.All) || prop.proto.Contains((CreatureVocationType)modulePlayer.proto)));
        });

        dataSource = new DataSource<ItemPair>(list, scrollView, OnSetData);
        return true;
    }

    private void OnSetData(RectTransform node, ItemPair data)
    {
        if (data == null) return;
        var prop = ConfigManager.Get<PropItemInfo>(data.itemId);
        Util.SetItemInfo(node, prop);

        Util.SetText(node.GetComponent<Text>("numberdi/count"), data.count.ToString());
    }
}
