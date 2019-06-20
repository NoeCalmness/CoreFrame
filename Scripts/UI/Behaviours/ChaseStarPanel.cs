using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class ChaseStarPanel : MonoBehaviour
{
    #region custom class

    private class ChaseStarPreview : CustomSecondPanel
    {
        public ChaseStarPreview(Transform trans, int index) : base(trans)
        {
            m_index = index;
            m_star = m_index + 1;
        }

        private Image greenBg;
        private Image m_starActive;
        private Text m_grayDesc;
        private Text m_greenDesc;
        private GameObject m_rewardItemObj;
        private Transform m_alreadyGet;

        private int m_index;
        private int m_star;

        public override void InitComponent()
        {
            base.InitComponent();

            greenBg         = transform.GetComponent<Image>("Image");
            m_starActive    = transform.GetComponent<Image>("star/starActive");
            m_grayDesc      = transform.GetComponent<Text>("decription/greyDecription");
            m_greenDesc     = transform.GetComponent<Text>("decription/greenDecription");
            m_rewardItemObj = transform.Find("rewardItem").gameObject;
            m_alreadyGet    = transform.Find("alreadyGet");
            Util.SetText(transform.GetComponent<Text>("alreadyGet/Text"), (int)TextForMatType.ChaseUIText, 47);
        }

        public void RefreshStarPreview(ChaseTask info)
        {
            if (info == null) return;

            SetPanelVisible(true);
            bool activeStar = info.IsStarActive(m_index);
            m_starActive.SafeSetActive(activeStar);
            TaskInfo.TaskStarDetail detail = info.taskConfigInfo?.GetStarDetail(m_star);
            string text = detail?.GetStarConditionDesc();
            //一星显示
            Util.SetText(m_grayDesc, text);
            Util.SetText(m_greenDesc, text);
            m_grayDesc.SafeSetActive(!activeStar);
            m_greenDesc.SafeSetActive(activeStar);
            RefreshReward(detail?.reward, activeStar);
        }

        public void RefreshStarPreview(TaskInfo task, int star)
        {
            if (task == null) return;

            SetPanelVisible(true);
            bool activeStar = star.BitMask(m_index);
            m_starActive.SafeSetActive(activeStar);
            TaskInfo.TaskStarDetail detail = task.GetStarDetail(m_star);
            string text = detail?.GetStarConditionDesc();
            //一星显示
            Util.SetText(m_grayDesc, text);
            Util.SetText(m_greenDesc, text);
            m_grayDesc.SafeSetActive(!activeStar);
            m_greenDesc.SafeSetActive(activeStar);
            RefreshReward(detail?.reward, activeStar);
        }

        private void RefreshReward(TaskInfo.TaskStarReward reward, bool activeStar)
        {
            m_rewardItemObj.SafeSetActive(!activeStar);
            m_alreadyGet.SafeSetActive(activeStar);
            greenBg.SafeSetActive(activeStar);

            if (reward == null) return;

            if (m_rewardItemObj && m_rewardItemObj.activeInHierarchy)
            {
                int propId = reward.diamond > 0 ? 2 : (reward.props != null && reward.props.Length > 0) ? reward.props[0].propId : reward.coin > 0 ? 1 : 0;
                int num = reward.diamond > 0 ? reward.diamond : (reward.props != null && reward.props.Length > 0) ? reward.props[0].num : reward.coin > 0 ? reward.coin : 0;

                var rewardInfo = ConfigManager.Get<PropItemInfo>(propId);
                if (rewardInfo) Util.SetItemInfo(m_rewardItemObj, rewardInfo, 0, num, false);
            }
        }
    }

    #endregion

    private string[] m_previewNams = new string[] { "one", "two", "three" };
    private List<ChaseStarPreview> m_starPreviews = new List<ChaseStarPreview>();
    private Image[] starInSettlement;

    private bool isInited;

    private void InitComponent(bool _isInSettlement)
    {
        if (isInited) return;
        isInited = true;

        m_starPreviews.Clear();
        for (int i = 0; i < m_previewNams.Length; i++)
        {
            var item = new ChaseStarPreview(transform.Find(Util.Format("parent/{0}", m_previewNams[i])), i);
            item.SetPanelVisible(false);
            m_starPreviews.Add(item);
        }

        if (_isInSettlement)
        {
            Transform parent = transform.Find("starGroupActive");
            starInSettlement = new Image[parent.childCount];
            for (int i = 0; i < parent.childCount; i++)
            {
                starInSettlement[i] = parent.GetChild(i).GetComponent<Image>();
                starInSettlement[i].gameObject.SetActive(false);
            }
        }
    }

    private void RefreshComponent(bool isInSettlement,int star)
    {
        InitComponent(isInSettlement);

        //结算界面的星级显示一
        if (isInSettlement)
        {
            for (int i = 0; i < starInSettlement.Length; i++)
                starInSettlement[i].gameObject.SetActive(i < star);
        }
    }

    public void RefreshPanel(ChaseTask info, bool isInSettlement = false)
    {
        RefreshComponent(isInSettlement,info.star);
        foreach (var item in m_starPreviews)
        {
            item.RefreshStarPreview(info);
        }
    }

    /// <summary>
    /// 显示星级奖励面板
    /// </summary>
    /// <param name="info"></param>
    /// <param name="settlementStar"></param>
    /// <param name="isInSettlement"></param>
    public void RefreshPanel(TaskInfo info,int settlementStar, bool isInSettlement = false)
    {
        int count = 0;
        for (int i = 0; i < 3; i++) count += settlementStar.BitMask(i) ? 1 : 0;

        RefreshComponent(isInSettlement, count);

        foreach (var item in m_starPreviews)
        {
            item.RefreshStarPreview(info,settlementStar);
        }
    }
}