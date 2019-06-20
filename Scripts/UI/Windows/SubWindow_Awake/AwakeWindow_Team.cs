// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-03      16:46
//  * LastModify：2018-08-04      10:15
//  ***************************************************************************************************/
#region

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class AwakeWindow_Team : SubWindowBase
{

    private int Index
    {
        get { return index; }
        set { if(index != value) { index = value; AssertIndex();} }
    }

    #region Fields

    private Transform                       confirmPanel;
    private ChaseStarPanel                  chaseStar;
    private Button                          leftButton;
    private Button                          rightButton;
    private Image                           taskIcon;
    private Image                           readyStateImage;
    private Button                          taskButton;
    private Button                          inviteButton;
    private Button                          chatButton;
    private Transform                       invitePanel;
    private Toggle                          openToggle;
    private Button                          buyTimeButton;
    private Button                          readyButton;
    private Button                          prepareButton;
    private Button                          cancelButton;
    private Button                          TickOutButton;
    private Button                          broadButton;
    private Transform                       timesPanel;
    private Transform                       costBloodPanel;
    private Text                            enterTimes;
    private Text                            costBloodCard;
    private Color                           noEnough;
    private Text                            stageName;
    private Text                            levelText;
    private Text                            captainName;
    private Text                            energyCost;
    private Text                            energyCost2;
    private Text                            tipText;
    private Text                            startButtonText;
    private Text                            noRewardHint;
    private Text                            recommend;
    private Transform                       chatNotice;
    private readonly Transform[]            stars = new Transform[3];
    private readonly Transform[]            InfoPanel = new Transform[2];

    private List<GameObject>                m_chaseRewardItems;//item的奖励
    private IReadOnlyList<ChaseTask>        taskList;
    private int                             index;
    private AwakeWindow_Invite              inviteWindow;
    private AwakeWindow_BuyTimes            confirmWindow;
    private Creature                        teamPlayer;
    private Creature                        teamPlayerPet;
    private Coroutine                       m_memberLoadCoroutine;
    private Coroutine                       m_masterSetPosCoroutine;
    #endregion

    #region override fucntion

    protected override void InitComponent()
    {
        base.InitComponent();

        var parent = WindowCache.GetComponent<Transform> ("team_panel/preaward_Panel/parent");
        m_chaseRewardItems = ChaseTaskItem.InitRewardItem(parent, m_chaseRewardItems);

        var t               = WindowCache.transform.Find            ("team_panel/starReward_Panel");
        leftButton          = WindowCache.GetComponent<Button>      ("team_panel/awakeMissionImage_scrollView/changePage_Btn_left");
        rightButton         = WindowCache.GetComponent<Button>      ("team_panel/awakeMissionImage_scrollView/changePage_Btn_right");
        taskIcon            = WindowCache.GetComponent<Image>       ("team_panel/awakeMissionImage_scrollView/template/0/map_Img");
        readyStateImage     = WindowCache.GetComponent<Image>       ("team_panel/memberFrame_Img/ready_Img");
        taskButton          = WindowCache.GetComponent<Button>      ("team_panel/awakeMissionImage_scrollView/template/0/map_Img");
        chatButton          = WindowCache.GetComponent<Button>      ("team_panel/chat");
        invitePanel         = WindowCache.GetComponent<Transform>   ("invite_panel");
        InfoPanel[0]        = WindowCache.GetComponent<Transform>   ("team_panel/captainFrame_Img");
        InfoPanel[1]        = WindowCache.GetComponent<Transform>   ("team_panel/memberFrame_Img");
        inviteButton        = WindowCache.GetComponent<Button>      ("team_panel/invite_Panel");
        openToggle          = WindowCache.GetComponent<Toggle>      ("team_panel/open_Toggle");
        buyTimeButton       = WindowCache.GetComponent<Button>      ("team_panel/remainchallengeCount/resetBtn");
        readyButton         = WindowCache.GetComponent<Button>      ("team_panel/start_Btn");
        prepareButton       = WindowCache.GetComponent<Button>      ("team_panel/ready_Btn");
        cancelButton        = WindowCache.GetComponent<Button>      ("team_panel/cancel_Btn");
        TickOutButton       = WindowCache.GetComponent<Button>      ("team_panel/memberFrame_Img/TickOut");
        broadButton         = WindowCache.GetComponent<Button>      ("team_panel/postTeamInfo");
        timesPanel          = WindowCache.GetComponent<Transform>   ("team_panel/remainchallengeCount");
        costBloodPanel      = WindowCache.GetComponent<Transform>   ("team_panel/costBloodCard");
        enterTimes          = WindowCache.GetComponent<Text>        ("team_panel/remainchallengeCount/remainCount_Txt");
        costBloodCard       = WindowCache.GetComponent<Text>        ("team_panel/costBloodCard/Image/Text");
        noEnough            = costBloodCard.color;
        stageName           = WindowCache.GetComponent<Text>        ("team_panel/awakeMissionName_Txt");
        levelText           = WindowCache.GetComponent<Text>        ("team_panel/captainFrame_Img/captainLevel_Txt");
        captainName         = WindowCache.GetComponent<Text>        ("team_panel/captainFrame_Img/captainName_Txt");
        energyCost          = WindowCache.GetComponent<Text>        ("team_panel/start_Btn/Text");
        energyCost2         = WindowCache.GetComponent<Text>        ("team_panel/ready_Btn/Text");
        tipText             = WindowCache.GetComponent<Text>        ("team_panel/memberRequire_Txt");
        startButtonText     = WindowCache.GetComponent<Text>        ("team_panel/start_Btn/start_text");
        noRewardHint        = WindowCache.GetComponent<Text>        ("team_panel/remainchallengeCount/noRewardHint");
        recommend           = WindowCache.GetComponent<Text>        ("team_panel/awakeMissionImage_scrollView/recommend/Text");
        confirmPanel        = WindowCache.GetComponent<Transform>   ("tip");
        chatNotice          = WindowCache.GetComponent<Transform>   ("team_panel/chat/newinfo");

        stars[0]            = WindowCache.GetComponent<Transform>   ("team_panel/awakeMissionImage_scrollView/starframe/star_01");
        stars[1]            = WindowCache.GetComponent<Transform>   ("team_panel/awakeMissionImage_scrollView/starframe/star_02");
        stars[2]            = WindowCache.GetComponent<Transform>   ("team_panel/awakeMissionImage_scrollView/starframe/star_03");

        if (t) chaseStar = t.GetComponentDefault<ChaseStarPanel>();


        if (openToggle.isOn) openToggle.isOn = false;
    }

    public override void MultiLanguage()
    {
        base.MultiLanguage();

        var ct = ConfigManager.Get<ConfigText>((int)TextForMatType.AwakeStage);
        if (ct == null) return;
        Util.SetText(WindowCache.GetComponent<Text>("team_panel/remainchallengeCount/Text"),            ct.text[1]);
        Util.SetText(WindowCache.GetComponent<Text>("team_panel/starReward_panel/starReward_Txt"),      ct.text[2]);
        Util.SetText(WindowCache.GetComponent<Text>("team_panel/pre_award_parent/pre_award_Txt"),       ct.text[3]);
        Util.SetText(WindowCache.GetComponent<Text>("team_panel/remainchallengeCount/noRewardHint"),    ct.text[46]);
        Util.SetText(WindowCache.GetComponent<Text>("team_panel/postTeamInfo/Text"),                    ct.text[49]);
    }

    public override bool Initialize(params object[] p)
    {
        if (moduleAwakeMatch.CurrentTask == null) return false;
        if (!base.Initialize(p))
        {
            moduleHome.HideOthersBut(Module_Home.FIGHTING_PET_OBJECT_NAME, Module_Home.PLAYER_OBJECT_NAME, Module_Home.TEAM_OBJECT_NAME, Module_Home.TEAM_PET_OBJECT_NAME);
            return true;
        }
        moduleGlobal.ShowGlobalLayerDefault();

        index = -1;
        UpdateTaskList();
        RefreshOpenButtonState();
        Index    = taskList.FindIndex(item => item?.taskConfigInfo?.ID == moduleAwakeMatch.StageId);
        leftButton      ?.onClick.AddListener(PrevTask);
        rightButton     ?.onClick.AddListener(NextTask);
        taskButton      ?.onClick.AddListener(onTaskIconClick);
        inviteButton    ?.onClick.AddListener(onInviteClick);
        buyTimeButton   ?.onClick.AddListener(onBuyTimeClick);
        readyButton     ?.onClick.AddListener(onStartGame);
        prepareButton   ?.onClick.AddListener(onPrepare);
        cancelButton    ?.onClick.AddListener(onCancel);
        openToggle      ?.onValueChanged.AddListener(onToggleRoom);
        TickOutButton   ?.onClick.AddListener(onTickOut);
        broadButton     ?.onClick.AddListener(onBroadAssistClick);

        broadButton.SafeSetActive(moduleAwakeMatch.MasterIsCaptain);

        chatButton?.onClick.AddListener(() =>
        {
            moduleChat.opChatType = OpenWhichChat.TeamChat;
            Window.ShowAsync<Window_Chat>();
        });
        Window_TeamMatch.IsChooseStage = false;
        chatNotice.SafeSetActive(false);

        if (inviteWindow == null && invitePanel != null)
            inviteWindow = CreateSubWindow<AwakeWindow_Invite>(WindowCache, invitePanel?.gameObject);

        if (confirmPanel)
            confirmWindow = CreateSubWindow<AwakeWindow_BuyTimes>(WindowCache, confirmPanel?.gameObject);

        moduleHome.HideOthersBut(Module_Home.FIGHTING_PET_OBJECT_NAME, Module_Home.PLAYER_OBJECT_NAME, Module_Home.TEAM_OBJECT_NAME, Module_Home.TEAM_PET_OBJECT_NAME);

        RefreshMember();

        return true;
    }

    private void onBroadAssistClick()
    {
        if (!moduleAwakeMatch.CanBroad)
        {
            moduleGlobal.ShowMessageFormat((int)TextForMatType.AwakeStage, 48, 1, (int)moduleAwakeMatch.BroadCountDown);
            return;
        }

        if (moduleAwakeMatch.matchInfos.Length == 2)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.AwakeStage, 47));
            return;
        }

        moduleAwakeMatch.Request_Broad();
    }


    public override bool UnInitialize(bool hide = true)
    {
        if (base.UnInitialize(hide))
        {
            moduleGlobal.ShowGlobalLayerDefault(1, false);

            leftButton?.onClick.RemoveAllListeners();
            rightButton     ?.onClick.RemoveAllListeners();
            taskButton      ?.onClick.RemoveAllListeners();
            inviteButton    ?.onClick.RemoveAllListeners();
            chatButton      ?.onClick.RemoveAllListeners();
            buyTimeButton   ?.onClick.RemoveAllListeners();
            readyButton     ?.onClick.RemoveAllListeners();
            prepareButton   ?.onClick.RemoveAllListeners();
            cancelButton    ?.onClick.RemoveAllListeners();
            TickOutButton   ?.onClick.RemoveAllListeners();
            openToggle      ?.onValueChanged.RemoveAllListeners();
            broadButton     ?.onClick.RemoveAllListeners();
            return true;
        }
        return false;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        inviteWindow?.Destroy();
        confirmWindow?.Destroy();
    }

    public override bool OnReturn()
    {
        if (inviteWindow != null && inviteWindow.OnReturn())
            return true;

        if (isInit)
        {
            moduleAwakeMatch.Request_ExitRoom(modulePlayer.id_);
            return true;
        }
        return false;
    }

    #endregion

    #region Click Events

    private void onTickOut()
    {
        if(moduleAwakeMatch.HasTeamMember)
            moduleAwakeMatch.Request_ExitRoom(moduleAwakeMatch.matchInfos[1].roleId);
    }


    private void onStartGame()
    {
        if (!moduleAwakeMatch.MasterIsCaptain)
            return;
        
        var result = Module_AwakeMatch.CheckTask(moduleAwakeMatch.CurrentTask);
        if (result != 0)
        {
                 if ((result & 4) > 0)
                moduleGlobal.ShowMessage((int)TextForMatType.AwakeStage, 5);
            else if ((result & 2) > 0)
                moduleGlobal.ShowMessage((int)TextForMatType.AwakeStage, 6);
            else if ((result & 1) > 0)
                moduleGlobal.ShowMessage((int)TextForMatType.AwakeStage, 7);
            else if ((result & 8) > 0)
                moduleGlobal.ShowMessage((int)TextForMatType.AwakeStage, 12);
            return;
        }
		moduleChase.lastStartChase = moduleAwakeMatch.CurrentTask;
        moduleAwakeMatch.Request_Readly(true);
    }

    private void onPrepare()
    {
        if (!moduleAwakeMatch.HasTeamMember)
            return;
        if (!moduleAwakeMatch.CanEnter(modulePlayer.id_, moduleAwakeMatch.StageId))
            return;

        moduleAwakeMatch.Request_Readly(true);
    }
    private void onCancel()
    {
        if (!moduleAwakeMatch.HasTeamMember)
            return;
        if (!moduleAwakeMatch.CanEnter(modulePlayer.id_, moduleAwakeMatch.StageId))
            return;

        moduleAwakeMatch.Request_Readly(false);
    }

    private void onBuyTimeClick()
    {
        confirmWindow?.Initialize();
    }

    private void onToggleRoom(bool state)
    {
        moduleAwakeMatch.Request_OpenRoom(state);
    }

    private void onInviteClick()
    {
        if (moduleAwakeMatch.HasTeamMember)
            return;

        if (moduleAwakeMatch.CurrentTask.taskConfigInfo.teamType == TeamType.Single)
        {
            moduleGlobal.ShowMessage((int)TextForMatType.AwakeStage, 14);
            return;
        }

        inviteWindow?.Initialize();
    }

    private void onTaskIconClick()
    {
        if (!moduleAwakeMatch.MasterIsCaptain)
            return;
        Window_TeamMatch.IsChooseStage = true;
        UnInitialize(false);
        WindowCache.Hide();
    }

    private void NextTask()
    {
        moduleAwakeMatch.Request_SwitchStage(PreviewStage(Index + 1));
    }

    private void PrevTask()
    {
        moduleAwakeMatch.Request_SwitchStage(PreviewStage(Index - 1));
    }

    #endregion

    #region NetWork

    private void _ME(ModuleEvent<Module_AwakeMatch> e)
    {
        switch (e.moduleEvent)
        {
            case Module_AwakeMatch.Notice_CurrentTaskChange:
                AssertIndex();
                break;
            case Module_AwakeMatch.Response_SwitchStage:
                ResponseSwitchStage(e.msg as ScTeamPveSwitchStage);
                break;
            case Module_AwakeMatch.Response_ExitRoom:
                ResponseExitRoom(e.msg as ScTeamPveExitRoom);
                break;
            case Module_AwakeMatch.Notice_MatchSuccess:
                OnMatchSuccess(e.msg as ScTeamPveMatchSuccess);
                break;
            case Module_AwakeMatch.Response_OpenRoom:
                var msg = e.msg as ScTeamPveOpenRoom;
                if (msg.result != 0) moduleGlobal.ShowMessage(9813, msg.result);
                RefreshOpenButtonState();
                break;
            case Module_AwakeMatch.Response_BuyEnterTime:
                RefreshTimes();
                break;
            case Module_AwakeMatch.Notice_ReadlyStateChange:
                ResponseReadly(e.msg as ScTeamPveReady);
                break;
        }
    }
    
    private void _ME(ModuleEvent<Module_Chat> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Chat.EventChatRecFriendMes:
                RefreshChatNotice(e.msg as ScChatPrivate);
                break;
            case Module_Chat.EventChatSeeTeamMsg:
                chatNotice.SafeSetActive(false);
                break;
        }
    }

    private void ResponseReadly(ScTeamPveReady msg)
    {
        if (msg.result != 0) return;
        RefreshButtonState();
    }

    private void OnMatchSuccess(ScTeamPveMatchSuccess msg)
    {
        if (msg.result != 0)
            return;
        UpdateTaskList();
        RefreshMember();

        broadButton.SafeSetActive(moduleAwakeMatch.MasterIsCaptain);
    }

    private void UpdateTaskList()
    {
        var type = moduleAwakeMatch.CurrentTask.taskType;
        taskList = type == TaskType.Nightmare
            ? moduleChase.CanEnterNightmareList
            : type == TaskType.Emergency ? moduleChase.emergencyList : moduleAwakeMatch.CanEnterActiveList;
    }


    private void ResponseExitRoom(ScTeamPveExitRoom msg)
    {
        if (msg.result != 0)
        {
            return;
        }
        UnInitialize(false);
        WindowCache.Hide();
    }

    private void ResponseSwitchStage(ScTeamPveSwitchStage msg)
    {
        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9807, msg.result);
            return;
        }
        Index = taskList.FindIndex(item => item.taskConfigInfo.ID == msg.stageId);
    }

    #endregion

    #region private functions

    private void RefreshOpenButtonState()
    {
        openToggle.isOn = moduleAwakeMatch.IsOpen;
    }

    private void AssertIndex()
    {
        RefreshButtonState();

        RefreshTimes();

        var task = moduleAwakeMatch.CurrentTask.taskConfigInfo;
        if (task != null)
        {
            var stageInfo = ConfigManager.Get<StageInfo>(task.stageId);
            var str = stageInfo.icon.Split(';');
            if (str.Length >= 1)
                UIDynamicImage.LoadImage(taskIcon.transform, str[0]);
        }

        RefreshTaskDesc();
    }

    private void RefreshMember()
    {
        inviteButton.SetInteractable(!moduleAwakeMatch.HasTeamMember);
        inviteButton.SafeSetActive(inviteButton.IsInteractable());
        InfoPanel[1].SafeSetActive(moduleAwakeMatch.HasTeamMember);

        if (moduleAwakeMatch.matchInfos == null) return;

        if (teamPlayer)
        {
            teamPlayer.Destroy();
            teamPlayer = null;
        }
        if (teamPlayerPet)
        {
            teamPlayerPet.Destroy();
            teamPlayerPet = null;
        }
        if (m_memberLoadCoroutine != null)
        {
            Level.current.StopCoroutine(m_memberLoadCoroutine);
            m_memberLoadCoroutine = null;
        }
        if (m_masterSetPosCoroutine != null)
        {
            Level.current.StopCoroutine(m_masterSetPosCoroutine);
            m_masterSetPosCoroutine = null;
        }

        for (var i = 0; i < moduleAwakeMatch.matchInfos.Length; i++)
        {
            if (moduleAwakeMatch.matchInfos[i].roleId != modulePlayer.id_)
                CreatTeamCreature(moduleAwakeMatch.matchInfos[i], i);
            else
                m_masterSetPosCoroutine = global::Root.instance.StartCoroutine(SetMasterPosition(i));

            var binder = new RoleInfoBinder(InfoPanel[i]);
            binder.Bind(moduleAwakeMatch.matchInfos[i]);
        }

        Util.SetText(levelText, moduleAwakeMatch.matchInfos[0].level.ToString());
        Util.SetText(captainName, moduleAwakeMatch.matchInfos[0].roleName);

        RefreshButtonState();
    }

    private IEnumerator SetMasterPosition(int rIndex)
    {
        var level = Level.current as Level_Home;
        while (!level || !level.master)
            yield return 0;

        yield return 0;

        //避免玩家异常退出
        if (moduleAwakeMatch.matchInfos != null && moduleAwakeMatch.matchInfos.Length > rIndex)
        {
            moduleHome.DispatchEvent(Module_Home.EventSetMasterPosition,
                Event_.Pop(GetTransform(moduleAwakeMatch.matchInfos[rIndex], rIndex)));
        }
        else
        {
            RefreshMember();
        }
    }

    private void RefreshButtonState()
    {
        readyButton.SafeSetActive(true);
        readyButton.transform.SetGray(false);
        readyStateImage.SafeSetActive(moduleAwakeMatch.HasTeamMember && moduleAwakeMatch.matchInfos[1].state);

        var chase = moduleAwakeMatch.CurrentTask;
        prepareButton       .SafeSetActive(!moduleAwakeMatch.MasterIsCaptain && !moduleAwakeMatch.MasterIsReady);
        cancelButton        .SafeSetActive(!moduleAwakeMatch.MasterIsCaptain &&  moduleAwakeMatch.MasterIsReady);
        openToggle          .SafeSetActive( moduleAwakeMatch.MasterIsCaptain && chase.taskConfigInfo.teamType != TeamType.Single);
        TickOutButton       .SafeSetActive( moduleAwakeMatch.MasterIsCaptain);

        leftButton.SafeSetActive(chase.taskType != TaskType.Emergency);
        rightButton.SafeSetActive(chase.taskType != TaskType.Emergency);
        taskButton.interactable = false/*chase.taskType == TaskType.Awake*/; //觉醒本也不需要通过图标选关了
        if (chase.taskType != TaskType.Emergency)
        {
            leftButton.SetInteractable(Index > 0 && moduleAwakeMatch.MasterIsCaptain);
            leftButton.transform.SetGray(!leftButton.IsInteractable());
            rightButton.SetInteractable(Index < taskList.Count - 1 && moduleAwakeMatch.MasterIsCaptain);
            rightButton.transform.SetGray(!rightButton.IsInteractable());
        }

        if (!moduleAwakeMatch.MasterIsCaptain)
        {
            readyButton.SafeSetActive(false);
            return;
        }
        if (chase.taskConfigInfo.teamType == TeamType.Double && !moduleAwakeMatch.HasTeamMember)
        {
            readyButton.transform.SetGray(true);
        }

        var info = chase.taskConfigInfo;
        Util.SetText(startButtonText,
                ConfigText.GetDefalutString((int)TextForMatType.AwakeStage, info.teamType == TeamType.Double && !moduleAwakeMatch.HasTeamMember ? 17 : 16));

    }

    private void RefreshChatNotice(ScChatPrivate msg)
    {
        if(moduleAwakeMatch.IsTeamMember(msg.sendId))
            chatNotice.SafeSetActive(true);
    }

    private int PreviewStage(int rIndex)
    {
        rIndex = Mathf.Clamp(rIndex, 0, taskList.Count - 1);
        return taskList[rIndex].taskConfigInfo.ID;
    }

    private void RefreshTimes()
    {
        if (moduleAwakeMatch.CurrentTask == null) return;
        costBloodPanel.SafeSetActive(moduleAwakeMatch.CurrentTask.taskType == TaskType.Emergency);
        timesPanel.SafeSetActive(moduleAwakeMatch.CurrentTask.taskType != TaskType.Emergency);
        if (moduleAwakeMatch.CurrentTask.taskType == TaskType.Emergency)
        {
            int cost = 0;
            if (moduleAwakeMatch.CurrentTask.taskConfigInfo.cost.Length > 0)
            {
                string[] _cost = moduleAwakeMatch.CurrentTask.taskConfigInfo.cost[0].Split('-');
                cost = _cost != null && _cost.Length > 1 ? Util.Parse<int>(_cost[1]) : 0;
            }

            Util.SetText(costBloodCard, "×" + cost);
            if (cost <= moduleEquip.bloodCardNum) costBloodCard.color = Color.green;
            else costBloodCard.color = noEnough;
        }
        else
        {
            bool isSelfTime = moduleAwakeMatch.CurrentTask.taskType == TaskType.Nightmare || (moduleAwakeMatch.CurrentTask.taskType == TaskType.Awake && moduleAwakeMatch.CurrentTaskIsActive);
            //自有次数
            if (isSelfTime)
            {
                var time = moduleAwakeMatch.CurrentTask.canEnterTimes;
                Util.SetText(enterTimes, time.ToString());
                buyTimeButton.SafeSetActive(time <= 0);
                noRewardHint.SafeSetActive(time <= 0 && !moduleAwakeMatch.CurrentTask.taskConfigInfo.limitEnter);
            }
            //公有次数
            else
            {
                Util.SetText(enterTimes, moduleAwakeMatch.enterTimes.ToString());
                buyTimeButton.SafeSetActive(moduleAwakeMatch.enterTimes <= 0);
                noRewardHint.SafeSetActive(moduleAwakeMatch.enterTimes <= 0 && !moduleAwakeMatch.CurrentTask.taskConfigInfo.limitEnter);
            }
        }
    }

    private void RefreshTaskDesc()
    {
        var info = moduleAwakeMatch.CurrentTask.taskConfigInfo;
        if (info)
        {
            Util.SetText(tipText, ConfigText.GetDefalutString(9811, (int)info.teamType));
            ChaseTaskItem.ShowRewards(info, m_chaseRewardItems, true);
            Util.SetText(stageName, info.name);
            Util.SetText(energyCost, $"(     ×{info.fatigueCount})");
            Util.SetText(energyCost2, energyCost.text);
            Util.SetText(recommend, Util.Format(ConfigText.GetDefalutString(TextForMatType.AwakeStage, 50), info.recommend));
            recommend.color = ColorGroup.GetColor(ColorManagerType.Recommend, modulePlayer.roleInfo.fight >= info.recommend);
            recommend?.transform.parent.SafeSetActive(info.recommend > 0);
        }

        var task = moduleAwakeMatch.CurrentTask;
        if (task != null)
        {
            chaseStar?.RefreshPanel(task);
        }

        var t = moduleAwakeMatch.CurrentTask;
        for (var i = 0; i < stars.Length; i++)
        {
            stars[i].SafeSetActive( t != null && t.star >= i + 1);
        }
    }

    private void CreatTeamCreature(PMatchProcessInfo info, int index)
    {
        //创建敌人
        var t = GetTransform(info, index);
        var pos = t.pos;
        var rot = t.rotation;

        var weaponInfo = ConfigManager.Get<PropItemInfo>(info.fashion.weapon);
        if (weaponInfo == null) return;

        moduleGlobal.LockUI("", 0.5f);
        m_memberLoadCoroutine =  Level.PrepareAssets(Module_Battle.BuildPlayerSimplePreloadAssets(info), r =>
        {
            if (!r) return;

            teamPlayer = moduleHome.CreatePlayer(info, pos, CreatureDirection.FORWARD);
            teamPlayer.uiName = Module_Home.TEAM_OBJECT_NAME;
            teamPlayer.gameObject.name = Module_Home.TEAM_OBJECT_NAME;
            teamPlayer.transform.localEulerAngles = rot;
            teamPlayer.transform.localScale = Vector3.one*t.size;
            moduleGlobal.UnLockUI();
        });

        if (info.pet != null && info.pet.itemTypeId != 0)
        {
            var rPet = PetInfo.Create(info.pet);
            var assets = new List<string>();
            Level.PrepareAssets(Module_Battle.BuildPetSimplePreloadAssets(rPet, assets, 2), b =>
            {
                var rGradeInfo = rPet.UpGradeInfo;
                var show = ConfigManager.Get<ShowCreatureInfo>(rPet.ID);
                if (show == null)
                {
                    Logger.LogError("没有配置config_showCreatureInfo表。宠物ID = {0}, 没有出生位置信息。宠物模型创建失败", rPet.ID);
                    return;
                }
                var showData = show.GetDataByIndex(0);
                var data = showData.data.GetValue<ShowCreatureInfo.SizeAndPos>(0);
                teamPlayerPet = moduleHome.CreatePet(rGradeInfo, pos + data.pos, data.rotation, Level.current.startPos, true,
                    Module_Home.TEAM_PET_OBJECT_NAME);
                teamPlayerPet.transform.localScale *= data.size;
                teamPlayerPet.transform.localEulerAngles = data.rotation;
            });
        }
    }

    private static ShowCreatureInfo.SizeAndPos GetTransform(PMatchProcessInfo info, int index)
    {
        var showInfo = ConfigManager.Get<ShowCreatureInfo>(10000 + info.roleProto);
        var pos = new Vector3(1.65f*index, 0, 0);
        var rot = new Vector3(0, 90, 0);
        var data = showInfo?.GetDataByIndex(index);
        if (data?.data != null && data.data.Length > 0)
        {
            return data.data[0];
        }
        return new ShowCreatureInfo.SizeAndPos() {size= 1, pos = pos, rotation = rot};
    }

    #endregion
}

public class RoleInfoBinder
{
    private Text    name;
    private Text    level;
    private Image   career;
    public RoleInfoBinder(Transform t)
    {
        if (!t)
            return;
        name   = t.GetComponent<Text>("Name_Txt");
        level  = t.GetComponent<Text>("Level_Txt");
        career = t.GetComponent<Image>("Career_Img");
    }

    public void Bind(PMatchProcessInfo info)
    {
        Util.SetText(name, info.roleName);
        Util.SetText(level, Util.Format(ConfigText.GetDefalutString((int)TextForMatType.AwakeStage, 18),  info.level));
        AtlasHelper.SetShared(career, $"career_icon_{info.roleProto:00}");
    }
}