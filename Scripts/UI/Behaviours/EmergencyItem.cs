using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmergencyItem : SubWindowBase<Window_Chase>
{
    private int cost;
    private Text stageName;
    private Text stageDesc;
    private Text remainNum;
    private Text challengeLv;
    private Text expendNum;
    private Text recommend;

    private Button fastMatch;
    private Button createTeam;
    private Button left_btn;
    private Button right_btn;
    private Button bloodCardBtn01, bloodCardBtn02;
    private Color notEnough;
    private ChaseStarPanel starPanel;
    private Transform rewardParent;
    private ScrollView view;
    private DataSource<int> data;
    private List<int> icons = new List<int>();
    
    protected override void InitComponent()
    {
        base.InitComponent();
        stageName   = Root.GetComponent<Text>("left/guanqia_bg/Text");
        stageDesc   = Root.GetComponent<Text>("left/text_bg/Text");
        remainNum   = Root.GetComponent<Text>("time_bg/Text");
        challengeLv = Root.GetComponent<Text>("middle/center/bg/Level");
        expendNum   = Root.GetComponent<Text>("middle/center/bg/text");
        notEnough   = expendNum.color;
        starPanel   = Root.GetComponent<ChaseStarPanel>("middle/starDesc");
        rewardParent= Root.GetComponent<Transform>("middle/award/pre_content");
        fastMatch   = Root.GetComponent<Button>("middle/btn/join_Btn");
        createTeam  = Root.GetComponent<Button>("middle/btn/create_Btn");
        view        = Root.GetComponent<ScrollView>("left/scrollBoss");
        data        = new DataSource<int>(null, view, OnSetIcon, null);
        left_btn    = Root.GetComponent<Button>("left/left_btn");
        right_btn   = Root.GetComponent<Button>("left/right_btn");
        bloodCardBtn01= Root.GetComponent<Button>("middle/center/bg/icon");
        bloodCardBtn02 = Root.GetComponent<Button>("time_bg/icon");
        recommend = Root.GetComponent<Text>("left/recommend/Text");
    }

    private void OnSetIcon(RectTransform node, int data)
    {
        var monster = ConfigManager.Get<MonsterInfo>(data);
        UIDynamicImage.LoadImage(node, monster.montserLargeIcon);
    }

    public override void MultiLanguage()
    {
        var text = ConfigManager.Get<ConfigText>((int)TextForMatType.ChaseUIText);
        if (text == null) return;

        Util.SetText(Root.GetComponent<Text>("left/bossPreview/Text"), text[56]);
        Util.SetText(Root.GetComponent<Text>("middle/starDesc/tittle_left/Text"), text[58]);
        Util.SetText(Root.GetComponent<Text>("middle/starDesc/tittle_right"), text[59]);
        Util.SetText(Root.GetComponent<Text>("middle/center/tittle/Text"), text[60]);
        Util.SetText(Root.GetComponent<Text>("middle/center/bg/Text"), text[61]);
        Util.SetText(Root.GetComponent<Text>("middle/center/bg/xiaohao"), text[62]);
        Util.SetText(Root.GetComponent<Text>("middle/award/tittle/Text"), text[63]);
        Util.SetText(Root.GetComponent<Text>("time_bg/text"), text[57]);
        Util.SetText(Root.GetComponent<Text>("middle/btn/create_Btn/Txt"), text[65]);
        Util.SetText(Root.GetComponent<Text>("middle/btn/join_Btn/Text"), text[64]);
    }

    public override bool Initialize(params object[] p)
    {
        if (base.Initialize(p))
        {
            var task = p[0] as ChaseTask;
            if (task == null) return true;
            starPanel.RefreshPanel(task);

            icons.Clear();
            icons.AddRange(task.taskConfigInfo.bossID);
            data.SetItems(icons);
            view?.onProgress.AddListener(OnDirectionBtnState);
            view?.ScrollToPage(0);
            left_btn.SafeSetActive(view?.currentPage > 0);
            right_btn.SafeSetActive(view.currentPage < view.pageCount - 1);
            left_btn?.onClick.AddListener(() => OnSkipIcon(true));
            right_btn?.onClick.AddListener(() => OnSkipIcon(false));

            if (task.taskConfigInfo.cost.Length > 0)
            {
                string[] _cost = task.taskConfigInfo.cost[0].Split('-');
                cost = _cost != null && _cost.Length > 1 ? Util.Parse<int>(_cost[1]) : 0;
            }
            Util.SetText(stageName, task.taskConfigInfo.name);
            Util.SetText(stageDesc, task.taskConfigInfo.desc);
            Util.SetText(remainNum, "×" + moduleEquip.bloodCardNum);
            Util.SetText(expendNum, "×" + cost);
            if (cost <= moduleEquip.bloodCardNum) expendNum.color = Color.green;
            else expendNum.color = notEnough;

            Util.SetText(challengeLv, "Lv. " + task.taskConfigInfo.unlockLv);
            Util.SetText(recommend, Util.Format(ConfigText.GetDefalutString((int)TextForMatType.ChaseUIText, 66), task.taskConfigInfo.recommend));
            recommend.color = ColorGroup.GetColor(ColorManagerType.Recommend, modulePlayer.roleInfo.fight >= task.taskConfigInfo.recommend);
            recommend?.transform.parent.SafeSetActive(task.taskConfigInfo.recommend > 0);

            var gameObjects =ChaseTaskItem.InitRewardItem(rewardParent);
            ChaseTaskItem.ShowRewards(task.taskConfigInfo, gameObjects, true);

            fastMatch?.onClick.AddListener(() => EnterRoom(true, task));
            createTeam?.onClick.AddListener(() => EnterRoom(false, task));

            var prop = ConfigManager.Find<PropItemInfo>(item => item.itemType == PropType.UseableProp && item.subType == (int)UseablePropSubType.WantedWithBlood);
            int id = prop != null ? prop.ID : 601;
            bloodCardBtn01?.onClick.AddListener(() => moduleGlobal.UpdateGlobalTip((ushort)id));
            bloodCardBtn02?.onClick.AddListener(() => moduleGlobal.UpdateGlobalTip((ushort)id));
        }
        return true;
    }

    private void OnDirectionBtnState(float arg0)
    {
        left_btn.SafeSetActive(view.currentPage > 0);
        right_btn.SafeSetActive(view.currentPage < view.pageCount - 1);
    }

    private void EnterRoom(bool noCreat, ChaseTask task)
    {
        if (moduleEquip.bloodCardNum < cost)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.ChaseUIText,55));
            return;
        }
     
        Window.SetWindowParam<Window_CreateRoom>(1, task);
        Window.ShowAsync<Window_CreateRoom>(null, w => moduleAwakeMatch.Request_EnterRoom(noCreat, task));
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (base.UnInitialize(hide))
        {
            fastMatch?.onClick.RemoveAllListeners();
            createTeam?.onClick.RemoveAllListeners();
            view?.onProgress.RemoveAllListeners();
            left_btn?.onClick.RemoveAllListeners();
            right_btn?.onClick.RemoveAllListeners();
            bloodCardBtn01.onClick.RemoveAllListeners();
            bloodCardBtn02.onClick.RemoveAllListeners();
            icons.Clear();
        }
        return false;
    }

    /// <summary>
    /// direction=false 向右 direction=true 向左
    /// </summary>
    /// <param name="direction"></param>
    private void OnSkipIcon(bool direction)
    {
        int index = view.currentPage;
        if (direction)
        {
            index--;
            if (index < 0) index = 0;
        }
        else
        {
            index++;
            if (index > view.pageCount - 1) index = view.pageCount - 1;
        }

        view.ScrollToPage(index);
    }
}