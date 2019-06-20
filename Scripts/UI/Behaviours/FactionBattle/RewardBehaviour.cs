// /**************************************************************************************************
//  * Copyright (C) 2017-2018 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-05-09      11:36
//  *LastModify：2019-05-09      11:36
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class RewardBehaviour : AssertOnceBehaviour
{
    private ScrollViewEx m_scrollView;
    private DataSource<ItemPair> m_dataSource; 

    protected override void Init()
    {
        base.Init();
        m_scrollView = transform.GetComponent<ScrollViewEx>();
    }

    public void BindData(TaskInfo.TaskStarReward rItems)
    {
        AssertInit();
        var list = rItems.ToItemPairList();
        list.RemoveAll(item =>
        {
            var prop = ConfigManager.Get<PropItemInfo>(item.itemId);
            return !(prop && prop.proto != null && (prop.proto.Contains(CreatureVocationType.All) || prop.proto.Contains((CreatureVocationType)Module_Player.instance.proto)));
        });
        m_dataSource = new DataSource<ItemPair>(list, m_scrollView, OnSetData);
    }

    private void OnSetData(RectTransform node, ItemPair data)
    {
        var prop = ConfigManager.Get<PropItemInfo>(data.itemId);
        if (prop == null)
        {
            Logger.LogError($"无效的道具ID = {data.itemId}");
            m_dataSource.RemoveItem(data);
            return;
        }
        Util.SetItemInfo(node, prop, 0, data.count, false);

        var b = node.GetComponentDefault<Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() => { Module_Global.instance.UpdateGlobalTip((ushort) data.itemId, true, false); });
    }
}
