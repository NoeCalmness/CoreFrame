using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class NpcMono : LinkedWindowBehaviour
{
    public const string NPC_IDLE_STATE = "StateNpcIdle";//IDLE状态
    private Creature npc_creatrue;
    //回调
    private Action<Creature> m_OnCreatedNpc;

    #region prefrab
    public NpcTypeID npc_Type = NpcTypeID.None;
    public Transform tweenTarget;
    #endregion

    private Module_Npc.NpcMessage currentNpc;

    public string uiName
    {
        get
        {
            //var currentNpc = Module_Npc.instance.GetTargetNpc(npc_Type);
            //string mode = currentNpc != null ? currentNpc.mode : "";
            //string name = npc_Type.ToString() + "_" + mode;
            //return name;
            return npc_Type.ToString();
        }
    }
    protected Dictionary<Button, NpcPosType> positionExpDic;//部位button与部位id的字典

    protected Image fullLvImage;
    protected Transform slider;
    protected Image topSlider;
    protected Image bottomSlider;
    protected Text expNumber;
    protected Transform tweenNumberParent;
    protected Text tweenNumber;
    protected Vector2 pos;//text的初始位置
    protected Text lvDec;
    protected Text lv;
    protected Button giftBtn;
    protected Button clickBtn;
    protected int delayIndex;
    private bool isAddListener;
    private bool isCreating;
    private string enterKey = DateTime.Now.ToString("d") + "_{0}";
    private float currentStateLength;//当前动画的长度
    private List<string> states = new List<string>();
    private List<Button> buttons = new List<Button>();

    #region 新增功能
    private Transform monologueTran;
    private Text monoText;
    private Transform fetterMsg;
    private Text fetterLv;
    private Text fetterStage;
    private Image missionImage;
    private Button missionBtn;
    private bool isClicked;
    private Transform missionMsg;
    private Text missionTittle;
    private Text missionContent;
    private Button beginTask;
    #endregion

    /// <summary>
    /// 绑定回调
    /// </summary>
    /// <param name="OnCreatedNpc">创建NPC成功时回调</param>
    public virtual void InitAction(Action<Creature> OnCreatedNpc = null)
    {
        m_OnCreatedNpc = OnCreatedNpc;

        if (npc_creatrue) m_OnCreatedNpc?.Invoke(npc_creatrue);
    }

    protected override void OnInitialize()
    {
        fullLvImage       = transform.Find("fullLevelImage").GetComponent<Image>();
        slider            = transform.Find("slider");
        topSlider         = transform.Find("slider/topSlider").GetComponent<Image>();
        bottomSlider      = transform.Find("slider/bottomSlider").GetComponent<Image>();
        expNumber         = transform.Find("slider/expNumber").GetComponent<Text>();
        tweenNumberParent = transform.Find("slider/tweenNumber");
        tweenNumber       = transform.Find("slider/tweenNumber/Text").GetComponent<Text>();
        lvDec             = transform.Find("slider/levelDec/Text").GetComponent<Text>();
        lv                = transform.Find("slider/level/Text").GetComponent<Text>();
        giftBtn           = transform.Find("giftBox").GetComponent<Button>();
        monologueTran     = transform.Find("talk");
        monoText          = transform.Find("talk/bg/content")?.GetComponent<Text>();
        fetterMsg         = transform.Find("fetterMsg");
        fetterLv          = transform.Find("fetterMsg/lv")?.GetComponent<Text>();
        fetterStage       = transform.Find("fetterMsg/stage")?.GetComponent<Text>();
        missionImage      = transform.Find("missionBtn")?.GetComponent<Image>();
        missionBtn        = transform.Find("missionBtn")?.GetComponentDefault<Button>();
        missionMsg        = transform.Find("missionMsg");
        missionTittle     = transform.Find("missionMsg/tittle")?.GetComponent<Text>();
        missionContent    = transform.Find("missionMsg/content")?.GetComponent<Text>();
        beginTask         = transform.Find("missionMsg/beginTask")?.GetComponent<Button>();

        RectTransform btnParent = transform.Find("npcRawImage/click")?.GetComponent<RectTransform>();
        Button[] _buttons = btnParent?.GetComponentsInChildren<Button>(true);
        positionExpDic = new Dictionary<Button, NpcPosType>();
        if (_buttons != null)
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                Button button = _buttons[i];
                if (button.name.Contains("arm")) positionExpDic.Add(button, NpcPosType.Arm);
                else positionExpDic.Add(button, (NpcPosType)(i + 1));

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => { OnButtonClick(button); });
            }
        }

        pos = tweenNumber.rectTransform().anchoredPosition;
        tweenNumber.SafeSetActive(false);
        slider.SafeSetActive(false);

        giftBtn.onClick.RemoveAllListeners();
        giftBtn.onClick.AddListener(OnOpenWindowGift);

        #region 新增功能
        missionMsg.SafeSetActive(false);
        Util.SetText(transform.Find("missionMsg/beginTask/beginTask_Text")?.GetComponent<Text>(), (int)TextForMatType.GiftUIText, 16);
        #endregion
    }

    private void OnOpenWindowGift()
    {
        if (tweenNumberParent != null && tweenNumberParent.childCount > 1)
        {
            int number = tweenNumberParent.childCount;

            for (int i = number - 1; i >= 1; i--)
                Destroy(tweenNumberParent.GetChild(i).gameObject);
        }
        if (tweenTarget && npc_Type == NpcTypeID.WishNpc)
        {
            TweenPosition target = tweenTarget.GetComponent<TweenPosition>();
            target?.Play();
        }
        Window.ShowAsync<Window_Gift>();
        slider.SafeSetActive(false);
    }

    private void AddListenerEvent()
    {
        if (isAddListener) return;
        isAddListener = true;

        EventManager.AddEventListener(Module_Npc.NpcAddExpSuccessEvent, OnAddExp);
        EventManager.AddEventListener(Module_Npc.NpcLvUpEvent, OnNpcLvUp);
        EventManager.AddEventListener(Module_Gift.NpcPlayGiveGiftEvent, OnPlayGift);

        EventManager.AddEventListener(Module_Task.EventTaskBeginMessgage, OnRefreshTask);
        EventManager.AddEventListener(Module_Task.EventTaskFinishMessage, OnRefreshTask);
        EventManager.AddEventListener(Module_Task.EventTaskNotificationProgess, OnRefreshTask);
    }

    /// <summary>
    /// 第一次窗口enable的时候并且这个脚本在窗口enable的时候是显示的,会走这个方法去创建NPC
    /// </summary>
    protected override void OnWindowShow()
    {
        if (!isActiveAndEnabled) return;

        Create_Npc();
    }

    public void SwitchNpc(NpcTypeID rNpcId)
    {
        if (currentNpc?.npcType == rNpcId)
            return;
        npc_Type = rNpcId;

        npc_creatrue?.Destroy();
        Create_Npc();
    }

    private void CreateClickBox()
    {
        var clickRoot = transform.Find("npcRawImage/click");
        if (null == clickRoot)
            return;

        var clickBox = ConfigManager.Get<NpcClickBox>((int)npc_Type);
        if (clickBox == null) return;

        Util.ClearChildren(clickRoot, true);
        positionExpDic.Clear();
        buttons.Clear();

        GameObject go = new GameObject();
        go.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        go.AddComponent<Button>();
        go.SetActive(false);
        for (var i = 0; i < clickBox?.boxs.Length; i++)
        {
            var box = clickBox.boxs[i];
            var t = clickRoot.AddNewChild(go);
            t.gameObject.SetActive(true);
            t.localPosition = box.position;
            t.localRotation = Quaternion.Euler(box.euler);
            t.rectTransform().sizeDelta = box.size;
            var b = t.GetComponentDefault<Button>();
            positionExpDic.Add(b, box.npcPosType);
            buttons.Add(b);

            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => { OnButtonClick(b); });
            t.gameObject.name = box.npcPosType.ToString();
        }

        GameObject.Destroy(go);
    }

    public void Create_Npc()
    {
        Module_Home.instance.HideOthers(uiName);

        CreateClickBox();
        if (npc_creatrue || isCreating) return;

        //在创建一个NPC之前,先找有没有已经创建了这个NPC,避免重复创建
        var npc = Module_Home.instance.FindChild(uiName);
        if (npc)
        {
            var behaviour = npc.GetComponent<SceneObjectBehaviour>();
            npc_creatrue = behaviour?.sceneObject as Creature;
            if (npc_creatrue)
            {
                npc_creatrue.gameObject.SafeSetActive(false);

                SetCurNpc();
                OnComplate(npc_creatrue);
                return;
            }

            Destroy(npc);
        }

        isCreating = true;
        Module_Global.instance.LockUI("", 0.2f);

        Level.PrepareAssets(Module_Battle.BuildNpcSimplePreloadAssets((int)npc_Type), r =>
        {
            Module_Global.instance.UnLockUI();
            if (!r) return;

            npc_creatrue = Module_Home.instance.CreateNpc((int)npc_Type, Vector3.zero, new Vector3(0, 180, 0), uiName);
            if (npc_creatrue)
            {
                SetCurNpc();

                npc_creatrue.animator.enabled = false;
                npc_creatrue.gameObject.SafeSetActive(false);

                OnComplate(npc_creatrue);
            }
            isCreating = false;
        });
    }

    private void SetCurNpc()
    {
        Module_Npc.instance.SetCurNpc(npc_Type);
        currentNpc = Module_Npc.instance.curNpc ?? Module_Npc.instance.GetTargetNpc(npc_Type);
    }

    /// <summary>
    /// 这个OnComplate和onenable只会执行一个
    /// </summary>
    /// <param name="npc"></param>
    private void OnComplate(Creature npc)
    {     
        m_OnCreatedNpc?.Invoke(npc);
        HideVoicesAndEnterAction();
    }

    private void HideVoicesAndEnterAction()
    {
        if (isActiveAndEnabled) Module_Home.instance.HideOthers(uiName);

        if (currentNpc != null && currentNpc.npcInfo != null)
            states = StateMachineInfoSimple.soundsDic.Get(currentNpc.npcInfo.stateMachine);

        EnterAction();
        CheckHaveMission();
        RefreshFetterMsg();
    }

    private void OnEnable()
    {
        if (!Game.started) return;
        AddListenerEvent();

        if (npc_Type == NpcTypeID.None)
            return;
        if (npc_creatrue)
        {
            SetCurNpc();
            HideVoicesAndEnterAction();           
        }
        else if (m_initialized)
        {
            //窗口enable之后,但是这个脚本并没有enable,就会在脚本enable的时候创建NPC
            //如果已经创建过,就不会在创建了,因为Create_Npc里面有限制            
            Create_Npc();
        }
    }

    private void OnDisable()
    {
        if (!Game.started) return;

        StopSounds();
        OnHideEffect();

        slider.SafeSetActive(false);
        if (tweenNumberParent?.childCount > 1)
        {
            int number = tweenNumberParent.childCount;
            for (int i = number - 1; i >= 1; i--)
                Destroy(tweenNumberParent.GetChild(i).gameObject);
        }
        RemoveListener();

        if (npc_creatrue)
        {
            _PlayAnimation(NPC_IDLE_STATE);
            npc_creatrue.gameObject.SetActive(false);
        }
        OnOperationInDisable();
    }

    private void OnHideEffect()
    {
        if (buttons == null || buttons.Count < 1) return;
        for (int i = 0; i < buttons.Count; i++)
        {
            var tran = buttons[i]?.transform;
            if (tran.childCount > 0) Util.DisableAllChildren(tran);
        }
    }

    private void RemoveListener()
    {
        isAddListener = false;
        EventManager.RemoveEventListener(Module_Npc.NpcAddExpSuccessEvent, OnAddExp);
        EventManager.RemoveEventListener(Module_Npc.NpcLvUpEvent, OnNpcLvUp);
        EventManager.RemoveEventListener(Module_Gift.NpcPlayGiveGiftEvent, OnPlayGift);

        EventManager.RemoveEventListener(Module_Task.EventTaskBeginMessgage, OnRefreshTask);
        EventManager.RemoveEventListener(Module_Task.EventTaskFinishMessage, OnRefreshTask);
        EventManager.RemoveEventListener(Module_Task.EventTaskNotificationProgess, OnRefreshTask);
    }

    protected void OnButtonClick(Button button)
    {
        if (GeneralConfigInfo.defaultConfig.isTouchNpc == 0) return;

        //防止玩家抽风点, 应是语音和动作播放完之后才能点
        if (!npc_creatrue.currentState.info.state.Equals(NPC_IDLE_STATE)) return;
        clickBtn = button;
        Module_Npc.instance.SendAddExp((ushort)npc_Type, (sbyte)positionExpDic[button]);
        PlayTouchAnimation();
        UIEffectManager.PlayEffectAsync(button.GetComponent<RectTransform>(), "ui_dianji_1");
    }

    /// <summary>
    /// 触摸动作
    /// </summary>
    private void PlayTouchAnimation()
    {
        if (currentNpc == null || currentNpc.actionInfo == null) return;

        //抚摸播动画
        NpcActionInfo.NpcPosition[] Pos = currentNpc.actionInfo.npcPos;
        NpcActionInfo.AnimAndVoice[] action = null;
        //确定抚摸部位
        for (int i = 0; i < Pos.Length; i++)
        {
            if (Pos[i].posType == positionExpDic[clickBtn])
            {
                action = Pos[i].normalTouch;
                break;
            }
        }

        string _stateName = string.Empty;
        int index = 0;
        //确定抚摸动作
        for (int i = 0; i < action.Length; i++)
        {
            if (action[i].npcLvType == (NpcLevelType)currentNpc.stateStage)
            {
                _stateName = action[i].state;
                currentStateLength = npc_creatrue.currentState.floatLength;
                index = action[i].stateMonologue;
                break;
            }
        }
        Logger.LogDetail("_stateName={0}", _stateName);

        _PlayAnimation(_stateName);
        if (index >= 0) PlayActiveMonologue(index);

        clickBtn = null;
    }

    private void _PlayAnimation(string state)
    {
        if (string.IsNullOrEmpty(state)) return;
        if (!npc_creatrue || !npc_creatrue.stateMachine || npc_creatrue.stateMachine.currentState.name.Equals(state)) return;

        StopSounds();
        npc_creatrue.stateMachine.TranslateTo(state);
    }

    private void StopSounds()
    {
        if (states == null || states.Count < 1) return;
        for (int i = 0; i < states.Count; i++)
        {
            if (AudioManager.IsPlaying(states[i]))
            {
                AudioManager.Stop(states[i]);
                break;
            }
        }
    }

    /// <summary>
    /// 每天第一次进入场景会自动打招呼
    /// </summary>
    private void EnterAction()
    {
        if (currentNpc == null || currentNpc.actionInfo == null) return;

        PlayRandomMono();

        bool isFull = currentNpc.fetterLv >= currentNpc.maxFetterLv;
        fullLvImage.SafeSetActive(isFull);

        string str = Util.Format(enterKey, (int)npc_Type);
        if (PlayerPrefs.GetInt(str) == 1) return;

        NpcActionInfo.AnimAndVoice[] enter = currentNpc.actionInfo.enterPanel;
        if (enter != null && enter.Length > 0)
        {
            for (int i = 0; i < enter.Length; i++)
            {
                if (enter[i].npcLvType != (NpcLevelType)currentNpc.stateStage || enter[i].state == null)
                    continue;

                npc_creatrue.stateMachine.TranslateTo(enter[i].state);
                currentStateLength = npc_creatrue.currentState.floatLength;

                PlayerPrefs.SetInt(str, 1);
                break;
            }
        }
    }

    /// <summary>
    /// 加经验
    /// </summary>
    /// <param name="e"></param>
    private void OnAddExp(Event_ e)
    {
        if (!enabled) return;

        uint addExp = (uint)e.param1;
        if (addExp == 0) return;

        slider.SafeSetActive(true);
        PlayTween(addExp);
        SetSlider();
    }

    private void PlayTween(uint addExp)
    {
        if (tweenNumber == null) return;

        Transform tran = tweenNumberParent.AddNewChild(tweenNumber.gameObject);
        Text target = tran.GetComponent<Text>();
        if (target == null) return;

        DOTween.Kill(target);
        Util.SetText(target, "+" + addExp);
        target.rectTransform().anchoredPosition = pos;
        target.SafeSetActive(true);
        if (currentStateLength == 0) currentStateLength = 3;
        target.rectTransform().DOLocalMoveY(pos.y + 40, currentStateLength).OnComplete(() =>
        {
            if (target) DestroyImmediate(target.gameObject);
            if (tweenNumberParent.childCount <= 1)
                slider.SafeSetActive(false);
        });
    }

    private void SetSlider()
    {
        if (!enabled || currentNpc == null) return;

        RefreshFetterMsg();
        //已满级
        if (currentNpc.fetterLv >= currentNpc.maxFetterLv)
        {
            fullLvImage.SafeSetActive(true);
            slider.SafeSetActive(false);
            return;
        }

        //未满级
        bottomSlider.fillAmount = 1;
        topSlider.fillAmount = currentNpc.fetterProgress;

        Util.SetText(expNumber, Util.Format("{0}/{1}", currentNpc.nowFetterValue, currentNpc.toFetterValue));
        Util.SetText(lv, currentNpc.fetterLv.ToString());
        Util.SetText(lvDec, currentNpc.curLvName);
    }

    /// <summary>
    /// NPC升级
    /// </summary>
    /// <param name="e"></param>
    private void OnNpcLvUp(Event_ e)
    {
        var info = e.param1 as ScNpcLv;
        if (info == null) return;

        missionImage.SafeSetActive(false);
        missionMsg.SafeSetActive(false);

        SetSlider();
        //显示提升面板,要构建一个新的window,要传一个点击回调,在点击回调里调用剧情对话
        if (currentNpc != null && currentNpc.npcInfo != null && currentNpc.fetterLv != currentNpc.npcInfo.unlockLv)
            Window_NpcUpLv.ShowUpWindow(currentNpc, OnClickUpWindow);
    }

    private void OnClickUpWindow(Module_Npc.NpcMessage msg)
    {
        if (msg != null && msg.npcInfo != null && msg.fetterLv == msg.npcInfo.unlockLv)
            Module_NPCDating.instance.DoDatingEvent(msg.npcInfo.unlockStoryID);
    }

    /// <summary>
    /// 送礼动画
    /// </summary>
    private void OnPlayGift()
    {
        if (!enabled || currentNpc == null) return;

        TweenPosition target = null;
        if (tweenTarget && npc_Type == NpcTypeID.WishNpc)
        {
            target = tweenTarget.GetComponent<TweenPosition>();
            target?.PlayReverse();
        }
        if (!Module_Gift.instance.isGiveGift) return;

        if (target)
        {
            delayIndex = DelayEvents.Add(() =>
            {
                npc_creatrue.stateMachine.TranslateTo(currentNpc.actionInfo?.sendGiftState);
                Logger.LogDetail("播送礼动画={0}", currentNpc.actionInfo?.sendGiftState);
                Module_Gift.instance.isGiveGift = false;
                DelayEvents.Remove(delayIndex);
            }, target.duration);

        }
        else
        {
            npc_creatrue.stateMachine.TranslateTo(currentNpc.actionInfo?.sendGiftState);
            Logger.LogDetail("播送礼动画={0}", currentNpc.actionInfo?.sendGiftState);
            Module_Gift.instance.isGiveGift = false;
        }
    }

    #region 新增功能

    private void OnRefreshTask(Event_ e)
    {
        var taskid = (byte)e.param1;
        Logger.LogDetail("刷新任务:id=={0}", taskid);
        CheckHaveMission(taskid);
    }

    private void OnShowMissionContent(Task curMis)
    {
        if (curMis.taskState == EnumTaskState.Finish) return;

        missionMsg.SafeSetActive(!isClicked);
        if (!isClicked) RefreshMissionProgress(curMis);
        isClicked = !isClicked;
    }

    private void OnOperationInDisable()
    {
        isClicked = false;
        missionMsg.SafeSetActive(false);
    }

    private void CheckHaveMission(byte taskId = 0)
    {
        var task = Module_Npc.instance.GetTargetTask(currentNpc, taskId);

        RefreshMission(task);
    }

    private void RefreshMission(Task mission)
    {
        missionImage.SafeSetActive(mission != null);
        if (mission != null)
        {
            if (mission.taskState == EnumTaskState.None) return;
            Logger.LogDetail("mission.taskState={0}", mission.taskState);

            bool isFinish = mission.taskState == EnumTaskState.Finish;
            missionBtn.onClick.RemoveAllListeners();
            missionBtn.onClick.AddListener(() =>
            {
                if (!isFinish) OnShowMissionContent(mission);
                //else Module_Npc.instance.UnLockFetter((ushort)npc_Type);
            });
            if (mission.taskState == EnumTaskState.Finish) missionMsg.SafeSetActive(false);
            missionImage.saturation = mission.taskState == EnumTaskState.Finish ? 1 : 0;
            missionImage.SafeSetActive(false);
            missionImage.SafeSetActive(true);
        }
    }

    private void RefreshFetterMsg()
    {
        if (currentNpc == null) return;

        fetterMsg.SafeSetActive(currentNpc.fetterLv < currentNpc.maxFetterLv);
        Util.SetText(fetterLv, Util.Format(Util.GetString((int)TextForMatType.NpcUIText, 5), currentNpc.curLvName));
        Util.SetText(fetterStage, Util.Format(Util.GetString((int)TextForMatType.NpcUIText, 4), currentNpc.curStageName));
    }

    private void RefreshMissionProgress(Task mission)
    {
        if (mission == null) return;

        //刷新任务进度
        var name = Util.GetString(mission.taskNameID) + Util.Format("({0}/{1})", mission.taskCurProgess, mission.taskMaxProgess);
        Util.SetText(missionTittle, name);
        Util.SetText(missionContent, mission.taskDescID);

        var task = currentNpc.npcInfo.tasks.Find(o => o.taskId == mission.taskID);
        var eventId = task.eventId;
        beginTask?.onClick.RemoveAllListeners();
        beginTask?.onClick.AddListener(() => Module_NPCDating.instance.DoDatingEvent(eventId));
    }

    /// <summary>
    /// 播放动作独白
    /// </summary>
    /// <param name="index"></param>
    private void PlayActiveMonologue(int index)
    {
        if (monoText == null || monoText.gameObject.activeInHierarchy) return;

        var text = ConfigManager.Get<ConfigText>(index);
        if (text == null || text.text == null || text.text.Length < 1) return;
        int _index = UnityEngine.Random.Range(0, text.text.Length - 1);
        var str = text.text[_index];
        Util.SetText(monoText, str);
        monoText.transform.parent.SafeSetActive(true);
    }

    /// <summary>
    /// 播随机独白
    /// </summary>
    private void PlayRandomMono()
    {
        if (monologueTran == null) return;

        var random = monologueTran.GetComponentDefault<RandomMonologue>();
        if (random && currentNpc != null) random.InitializedData(currentNpc.npcInfo.randomMonologue, npc_creatrue);
    }

    #endregion
}