// /**************************************************************************************************
//  * Copyright (C) 2017-2018 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-05-09      11:21
//  *LastModify：2019-05-09      11:21
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class PreviewRewards : AssertOnceBehaviour
{
    private ScrollViewEx m_killScrollView;
    private ScrollViewEx m_rankScrollView;
    private DataSource<FactionKillRewardInfo> m_killDataSource;
    private DataSource<FactionRankRewardInfo> m_rankDataSource;

    private void Start()
    {
        AssertInit();
        var list = ConfigManager.FindAll<FactionKillRewardInfo>(item => RewardIsValid(item.reward));
        list.Sort((a, b) => -a.kill.CompareTo(b.kill));
        list.RemoveAll(item => !item.isDisplay);
        m_killDataSource = new DataSource<FactionKillRewardInfo>(list, m_killScrollView, OnSetKillData);
        var rankList = ConfigManager.FindAll<FactionRankRewardInfo>(item => RewardIsValid(item.reward));
        rankList.Sort((a, b) => a.rank.CompareTo(b.rank));
        m_rankDataSource = new DataSource<FactionRankRewardInfo>(rankList, m_rankScrollView, OnSetRankData);
    }

    private bool RewardIsValid(TaskInfo.TaskStarReward rReward)
    {
        return rReward.coin > 0 ||
               rReward.diamond > 0 ||
               rReward.expr > 0||
               rReward.fatigue > 0 ||
               rReward.props.Length > 0;
    }

    private void OnSetRankData(RectTransform node, FactionRankRewardInfo data)
    {
        var rankLabel = node.GetComponent<Text>("title_bg/Text");
        var d1 = ConfigManager.Get<FactionRankRewardInfo>(data.ID);
        var d2 = ConfigManager.Get<FactionRankRewardInfo>(data.ID-1);
        string s = d1.rank.ToString();
        if (d2 != null)
            s = (d2.rank + 1) + "~" + s;
        Util.SetText(rankLabel, Util.Format(ConfigText.GetDefalutString(TextForMatType.FactionSignUI, 4), s));
        var item = node.GetComponentDefault<RewardBehaviour>("rewardList");
        item.BindData(data.reward);
    }

    private void OnSetKillData(RectTransform node, FactionKillRewardInfo data)
    {
        var killLabel = node.GetComponent<Text>("title_bg/Text");
        Util.SetText(killLabel, ConfigText.GetDefalutString(data.name));
        var item = node.GetComponentDefault<RewardBehaviour>("rewardList");
        item.BindData(data.reward);
    }

    protected override void Init()
    {
        base.Init();

        m_killScrollView = transform.GetComponent<ScrollViewEx>("scrollView_left");
        m_rankScrollView = transform.GetComponent<ScrollViewEx>("scrollView_right");
    }


}
