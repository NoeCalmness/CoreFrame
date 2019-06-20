using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChaseTaskItem : BaseRestrain
{
    #region public function

    public static void ShowRewards(TaskInfo taskInfo, List<GameObject> items, bool isDetail, bool canGoto = true)
    {
        //预览奖励
        var stageInfo = ConfigManager.Get<StageInfo>(taskInfo.stageId);

        if (stageInfo == null)
        {
            for (int i = 0; i < items.Count; i++)
                items[i].SetActive(false);
            Logger.LogError("refresh the chase task,stageId = {0} connot be finded!", taskInfo.stageId);
        }
        else
        {
            int index = 0;
            for (int i = 0; i < stageInfo.previewRewardItemId.Length; i++)
            {
                if (index >= items.Count) break;

                if (stageInfo.previewRewardItemId[i] == -1)
                {
                    if (!isDetail) break;
                    index = 0;
                    continue;
                }

                PropItemInfo propInfo = ConfigManager.Get<PropItemInfo>(stageInfo.previewRewardItemId[i]);
                if (propInfo == null) continue;

                if (!propInfo.IsValidVocation(Module_Player.instance.proto)) continue;

                Util.SetItemInfoSimple(items[index], propInfo);

                if (isDetail)
                {
                    Button btn = items[index].GetComponentDefault<Button>();
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        Module_Global.instance.UpdateGlobalTip((ushort)propInfo.ID, true, canGoto);
                    });
                }

                items[index].SetActive(true);
                index++;
            }

            for (int i = index; i < items.Count; i++)
                items[i].SetActive(false);
        }
    }

    public static List<GameObject> InitRewardItem(Transform parent, List<GameObject> rewardItmes = null)
    {
        if (rewardItmes == null)
            rewardItmes = new List<GameObject>();
        rewardItmes.Clear();

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            rewardItmes.Add(child?.gameObject);
        }
        return rewardItmes;
    }
    #endregion

    #region content_items

    private Button itemButton;
    private Transform m_chaseIcon;
    private Text m_chaseName;
    private Image[] stars;
    private GameObject bossIcon;
    private List<GameObject> m_chaseRewardItems;//item的奖励
    private Text challengeText;
    private Transform dropUp;
    private Transform rateUp;
    private Transform numberUp;
    private Transform dropItem;
    private Image accept_Image;

    /// <summary>
    /// item的id
    /// </summary>
    public int chaseTaskItemId { get; private set; }

    private bool m_isInit = false;
    private void InitComponent()
    {
        if (m_isInit) return;
        m_isInit = true;

        itemButton         = GetComponent<Button>();
        m_chaseIcon        = transform.Find("mask/chaseIcon");
        m_chaseName        = transform.Find("nameText")?.GetComponent<Text>();
        Transform parent   = transform.Find("pre_award");
        m_chaseRewardItems = InitRewardItem(parent, m_chaseRewardItems);
        stars              = transform.Find("star_parent")?.GetComponentsInChildren<Image>(true);
        bossIcon           = transform.Find("bossicon")?.gameObject;
        bossIcon.SetActive(false);
        challengeText      = transform.Find("challenge/Text")?.GetComponent<Text>();
        challengeText.transform.parent.gameObject.SetActive(false);
        dropUp             = transform.Find("up");
        rateUp             = transform.Find("up/rateUp");
        numberUp           = transform.Find("up/numberUp");
        dropItem           = transform.Find("up/item");

        accept_Image = transform.Find("bg_01/arresting_bottom_img_01").GetComponent<Image>();
    }

    public void RefreshTaskItem(ChaseTask info, int index, Action<ChaseTask> itemCilck)
    {
        InitComponent();
        if (info == null) return;
        var finish = info.taskData.state == (byte)EnumChaseTaskFinishState.Finish;

        restrainId = info.taskConfigInfo.ID;
        chaseTaskItemId = info.taskConfigInfo.ID;

        accept_Image.gameObject.SetActive((info.taskType == TaskType.Easy|| info.taskType == TaskType.Difficult || info.taskType == TaskType.Nightmare) && !finish);

        //icon
        m_chaseIcon.SafeSetActive(false);
        string[] str = info.stageInfo.icon.Split(';');
        if (str != null && str.Length >= 1)
            UIDynamicImage.LoadImage(m_chaseIcon, str[0], (a, b) => { m_chaseIcon.SafeSetActive(true); });
        //name
        if (info.taskType == TaskType.Emergency || info.taskType == TaskType.Awake) m_chaseName.text = info.taskConfigInfo.name;
        else m_chaseName.text = Module_Chase.instance.allTasks_Name == null ? "" : Module_Chase.instance.allTasks_Name[info.taskConfigInfo.ID];
        //奖励
        ShowRewards(info.taskConfigInfo, m_chaseRewardItems, false);
        //星级
        for (int i = 0; i < stars.Length; i++)
            stars[i].gameObject.SetActive(i < info.star);

        //bossicon=每页的最后一关显示
        bossIcon.SetActive(index == 5);

        itemButton.onClick.RemoveAllListeners();
        itemButton.onClick.AddListener(() =>
        {
            if (itemCilck != null && info != null) itemCilck(info);
        });
        //困难关卡
        challengeText.transform.parent.gameObject.SetActive(info.taskType == TaskType.Difficult|| info.taskType == TaskType.Nightmare);
        int remainCount = info.canEnterTimes;
        challengeText.text = Util.Format("{0}/{1}", remainCount >= 0 ? remainCount : 0, info.taskConfigInfo.challengeCount);

        //掉落活动
        var drop = Module_Welfare.instance.GetCurDropUp(info.taskData.taskId);
        dropUp.SafeSetActive(drop != null);
        if (drop != null)
        {
            rateUp.SafeSetActive(drop.type == 1);
            numberUp.SafeSetActive(drop.type == 2);

            //如果为-1全部提升 默认显示第一个道具
            PropItemInfo prop = null;
            if (drop.itemTypeId == -1)
            {
                var stageInfo = ConfigManager.Get<StageInfo>(info.stageId);
                if (stageInfo == null || stageInfo.previewRewardItemId == null || stageInfo.previewRewardItemId.Length < 1 || stageInfo.previewRewardItemId[0] == -1) return;

                prop = ConfigManager.Get<PropItemInfo>(stageInfo.previewRewardItemId[0]);
            }
            else
                prop = ConfigManager.Get<PropItemInfo>(drop.itemTypeId);

            if (prop && dropItem)
            {
                Util.SetItemInfoSimple(dropItem, prop);
                dropItem.Find("quality").SafeSetActive(false);
                dropItem.Find("qualityRune").SafeSetActive(false);
                dropItem.Find("intentify").SafeSetActive(false);
            }
            else dropItem.SafeSetActive(false);
        }
    }

    public void RefreshTaskItem(TaskInfo info, int index)
    {
        InitComponent();
        if (info == null) return;

        restrainId = info.ID;
        chaseTaskItemId = info.ID;

        TaskType type = Module_Chase.instance.GetCurrentTaskType(info);

        accept_Image.gameObject.SetActive(false);

        //icon
        var stageInfo = ConfigManager.Get<StageInfo>(info.stageId);
        if (stageInfo != null)
        {
            string[] str = stageInfo.icon.Split(';');
            if (str != null && str.Length >= 1)
                UIDynamicImage.LoadImage(m_chaseIcon, str[0]);
        }
        //name
        if (type == TaskType.Emergency || type == TaskType.Awake) m_chaseName.text = info.name;
        else m_chaseName.text = Module_Chase.instance.allTasks_Name == null ? "" : Module_Chase.instance.allTasks_Name[info.ID];

        itemButton.onClick.RemoveAllListeners();
        itemButton.onClick.AddListener(() =>
        {
            Module_Chase.instance.ClickUnlock(info);
        });

        //奖励
        ShowRewards(info, m_chaseRewardItems, false);
        //星级
        for (int i = 0; i < stars.Length; i++)
            stars[i].gameObject.SetActive(false);

        //bossicon=每页的最后一关显示
        bossIcon.SetActive(index == 5);

        //困难关卡
        challengeText.transform.parent.gameObject.SetActive(type == TaskType.Difficult || type == TaskType.Nightmare);
        int remainCount = info.challengeCount;
        challengeText.text = Util.Format("{0}/{1}", remainCount >= 0 ? remainCount : 0, info.challengeCount);

        //掉落
        dropUp.SafeSetActive(false);
    }

    #endregion

    #region 详细界面
    private Text detail_DescriptionText;
    private Transform detail_Icon;
    private Image[] detail_stars;
    private Text detail_taskName;
    private Text fatigue_text;
    private Button toChaseBtn;
    private Button closeBtn;
    private List<GameObject> detail_items;//详情界面的奖励
    private ChaseStarPanel chaseStarPanel;
    private Text remainRestTime;
    private Button resetBtn;
    private Transform awakePanel;

    private bool isInited;

    private void IniteDetailPanel()
    {
        if (isInited) return;
        detail_DescriptionText  = transform.GetComponent<Text>("kuang/describion/zongjiText");
        detail_Icon             = transform.Find("kuang/map/icon");
        detail_stars            = transform.Find("kuang/diff_parent").GetComponentsInChildren<Image>(true);
        detail_taskName         = transform.GetComponent<Text>("kuang/name");
        fatigue_text            = transform.GetComponent<Text>("kuang/normal_Btn/zhuibu/consume/number");
        Util.SetText(transform.GetComponent<Text>("kuang/Text"), (int)TextForMatType.ChaseUIText, 21);
        toChaseBtn              = transform.GetComponent<Button>("kuang/normal_Btn/zhuibu");
        closeBtn                = transform.GetComponent<Button>("close_button");
        Transform parent        = transform.Find("kuang/pre_award_parent");
        detail_items            = InitRewardItem(parent, detail_items);
        chaseStarPanel          = transform.Find("kuang/starDesc").GetComponentDefault<ChaseStarPanel>();
        remainRestTime          = transform.GetComponent<Text>("kuang/remainchallengeCount/remainCount");
        resetBtn                = transform.GetComponent<Button>("kuang/remainchallengeCount/resetBtn");
        awakePanel              = transform.GetComponent<Transform>("kuang/normal_Btn/awake");

        if(!closeBtn)
            closeBtn = transform.GetComponent<Button>("kuang/close_button");
        isInited = true;
    }

    public void RefreshDetailPanel(ChaseTask info, Action<ChaseTask> chaseFunc)
    {
        IniteDetailPanel();
        if (info == null) return;
        this.gameObject.SetActive(true);
        awakePanel?.gameObject.SetActive(false);
        //描述
        Util.SetText(detail_DescriptionText, info.taskConfigInfo.desc);
        //name
        detail_taskName.text = info.taskConfigInfo.name;
        //map
        string[] str = info.stageInfo.icon.Split(';');
        if (str != null && str.Length >= 1)
            UIDynamicImage.LoadImage(detail_Icon, str[0]);
        //奖励
        ShowRewards(info.taskConfigInfo, detail_items, true);
        //星级
        for (int i = 0; i < detail_stars.Length; i++)
            detail_stars[i].gameObject.SetActive(i < info.star);
        //消耗体力
        var s = Module_Player.instance.roleInfo.fatigue < info.taskConfigInfo.fatigueCount ? GeneralConfigInfo.GetNoEnoughColorString("×" + info.taskConfigInfo.fatigueCount) : "×" + info.taskConfigInfo.fatigueCount;
        Util.SetText(fatigue_text, (int)TextForMatType.ChaseUIText, 50, s);
        if (toChaseBtn)
        {
            toChaseBtn.gameObject.SetActive(info.taskType != TaskType.Nightmare && info.taskType != TaskType.Awake);
            //追捕
            toChaseBtn.onClick.RemoveAllListeners();
            toChaseBtn.onClick.AddListener(() =>
            {
                if (info.taskType == TaskType.Active)
                {
                    var activeInfo = Module_Chase.instance.allActiveItems.Find((p) => p.taskLv == info.taskConfigInfo.level);
                    if (activeInfo == null) return;

                    if (!Module_Chase.instance.activeChallengeCount.ContainsKey(activeInfo.taskLv)) return;

                    if (Module_Chase.instance.activeChallengeCount[activeInfo.taskLv] >= activeInfo.crossLimit)
                    {
                        Module_Global.instance.ShowMessage(ConfigText.GetDefalutString(TextForMatType.ChaseUIText, 39));
                        return;
                    }
                }

                this.gameObject.SetActive(false);
                chaseFunc?.Invoke(info);
            });
        }
        //关闭
        closeBtn?.onClick.RemoveAllListeners();
        closeBtn?.onClick.AddListener(() =>
        {
            if (Module_Chase.instance.targetTaskFromForge != null)
                Module_Chase.instance.SetTargetTask(null);
        });
        //刷新关卡星级奖励
        chaseStarPanel.RefreshPanel(info);

        int remainCount = info.taskConfigInfo.challengeCount - info.taskData.dailyOverTimes;
        //重置次数显示
        if (remainRestTime)
        {
            remainRestTime.transform.parent.gameObject.SetActive(info.taskType == TaskType.Difficult);
            remainRestTime.text = $"{(remainCount >= 0 ? remainCount : 0)}/{info.taskConfigInfo.challengeCount}";
        }

        if (resetBtn)
        {
            resetBtn.gameObject.SetActive(info.taskType == TaskType.Difficult && remainCount <= 0);
            resetBtn.onClick.RemoveAllListeners();
            resetBtn.onClick.AddListener(() =>
            {
                if (chaseFunc != null)
                {
                    chaseFunc(info);
                    this.gameObject.SetActive(false);
                }
            });
        }
    }
    #endregion
}