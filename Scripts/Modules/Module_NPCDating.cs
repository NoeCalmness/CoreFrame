/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-12-17
 * 
 ***************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Module_NPCDating : Module<Module_NPCDating>
{
    #region window

    #region window npcdating
    /// <summary>进入约会主界面</summary>
    public const string EventEnterDatingPanel = "EventNPCDatingEnterDatingPanel";

    /// <summary>进入约会场景</summary>
    public const string EventRealEnterDatingScene = "EventNPCDatingRealEnterDatingScene";

    /// <summary>通知滚动视差场景 param1:通知类型(EnumDatingNotifyType)  param2:各种参数</summary>
    public const string EventNotifyDatingMapObject = "EventNPCDatingNotifyDatingMapObject";

    /// <summary>收到在场景中点击建筑物的事件,执行界面UI逻辑</summary>
    public const string EventClickDatingBuild = "EventNPCDatingClickDatingBuild";

    #region serverMsg

    /// <summary>
    /// 打开约会主界面发送消息
    /// </summary>
    public void SendOpenDatingWindow()
    {
        var p = PacketObject.Create<CsNpcDatingOpenWindow>();
        session.Send(p);
    }
    void _Packet(ScNpcDatingOpenWindow msg)
    {
        msg.CopyTo(ref m_ScNpcDatingOpenWindow);
        DispatchModuleEvent(EventEnterDatingPanel, msg.result);
    }

    /// <summary>
    /// 进入场景
    /// </summary>
    /// <param name="sceneId"></param>
    public void SendEnterScene(int sceneId)
    {
        var p = PacketObject.Create<CsNpcDatingEnterScene>();
        p.sceneId = sceneId;
        session.Send(p);

    }
    void _Packet(ScNpcDatingEnterScene msg)
    {
        msg.CopyTo(ref m_ScNpcDatingEnterScene);
        DispatchModuleEvent(EventRealEnterDatingScene, msg.result, msg.sceneId,msg);
    }

    /// <summary>
    /// 约会结束
    /// </summary>
    /// <param name="msg"></param>
    void _Packet(ScNpcEngagementOver msg)
    {
        if (msg != null) forceSettlement = msg.result == 1;
        m_curDatingNpc.datingEnd = 1;

        moduleNPCDatingSettlement.SendGetData();

        isNpcBattelSettlement = curDatingNpc!= null && !isDating && moduleChase.LastComrade != null && moduleChase.LastComrade.type == 1;
        if (isNpcBattelSettlement)
        {
            moduleHome.ClearWindowStackCache();
            moduleHome.PushWindowStack("window_home");
            moduleHome.PushWindowStack("window_npcdating");
        }
    }

    /// <summary>羁绊等级提升 </summary>
    void _Packet(ScNpcLv p)
    {
        if (p == null || curDatingNpc == null) return;
        isFetterUp = p.npcId == curDatingNpc.npcId;
    }

    void _Packet(ScNpcInfo p)
    {
        if (p != null && p.npcinfos != null && p.npcinfos.Length > 0)
        {
            if (p.engagementNpc != null) p.engagementNpc.CopyTo(ref m_curDatingNpc);
        }
    }

    #endregion

    public Creature CreateDatingNpc(string modelName)
    {
        if (curDatingNpc == null) return null;
        Creature npcCreature = null;
        var datingNpc = Module_Home.instance.FindChild(modelName);
        if (datingNpc != null) moduleHome.HideOthers(modelName);
        else
        {
            Level.PrepareAssets(Module_Battle.BuildNpcSimplePreloadAssets(moduleNPCDating.curDatingNpc.npcId), r =>
            {
                moduleGlobal.UnLockUI();
                if (!r) return;

                npcCreature = moduleHome.CreateNpc(moduleNPCDating.curDatingNpc.npcId, Vector3.zero, new Vector3(0, 180, 0), modelName);
                if (npcCreature)
                {
                    npcCreature.animator.enabled = false;
                    npcCreature.gameObject.SafeSetActive(true);
                }
                moduleHome.HideOthers(modelName);
            });
        }

        return npcCreature;
    }

    /// <summary>
    /// 点击场景对应的功能建筑，进入约会场景
    /// </summary>
    public void EnterDatingScene(EnumNPCDatingSceneType sceneType)
    {
        DispatchModuleEvent(EventClickDatingBuild, sceneType);
    }

    #region Model

    private ScNpcDatingEnterScene m_ScNpcDatingEnterScene = null;
    private ScNpcDatingOpenWindow m_ScNpcDatingOpenWindow = null;

    /// <summary>在约会场景完成的任务</summary>
    public Dictionary<EnumNPCDatingSceneType, List<Task>> dicSceneFinishedTask { private set; get; } = new Dictionary<EnumNPCDatingSceneType, List<Task>>();
    /// <summary>在约会场景接到的任务</summary>
    public Dictionary<EnumNPCDatingSceneType, List<Task>> dicSceneAcceptTask { private set; get; } = new Dictionary<EnumNPCDatingSceneType, List<Task>>();

    private PNpcEngagement m_curDatingNpc = null;
    public PNpcEngagement curDatingNpc { get { return m_curDatingNpc; } }

    public Module_Npc.NpcMessage curDatingNpcMsg { get { return Module_Npc.instance.GetTargetNpc(m_curDatingNpc == null ? NpcTypeID.None : (NpcTypeID)m_curDatingNpc.npcId); } }

    /// <summary>
    /// 当前约会Npc的模型名称
    /// </summary>
    public string datingNpcModelName { get { return ((NpcTypeID)curDatingNpc.npcId).ToString(); } }

    /// <summary>是否正在约会</summary>
    public bool isDating { private set { } get { return curDatingNpc != null && curDatingNpc.datingEnd == 0; } }

    /// <summary>羁绊等级是否提升</summary>
    public bool isFetterUp { set; get; } = false;

    /// <summary>
    /// Npc助战之后体力值不足导致约会结算
    /// </summary>
    public bool isNpcBattelSettlement { set; get; } = false;

    private EnumNPCDatingSceneType m_curDatingScene = EnumNPCDatingSceneType.None;
    /// <summary>当前约会场景</summary>
    public EnumNPCDatingSceneType curDatingScene {get { return m_curDatingScene; }}

    private EnumNPCDatingSceneType m_lastDatingScene = EnumNPCDatingSceneType.None;
    /// <summary>上一次约会场景</summary>
    public EnumNPCDatingSceneType lastDatingScene { get { return m_lastDatingScene; }}

    /// <summary>所有约会任务 List</summary>
    public List<Task> allMissionsList
    { get
        {
            List<Task> tList = moduleTask.GetTasks(EnumTaskType.EngagementType);
            tList.Sort(CompareMissionList);
            return tList;
        }
    }

    /// <summary>进入场景数据</summary>
    public ScNpcDatingEnterScene enterSceneData { get { return m_ScNpcDatingEnterScene; } }

    /// <summary>进入约会主界面数据 </summary>
    public ScNpcDatingOpenWindow openWindowData { get { return m_ScNpcDatingOpenWindow; } }

    /// <summary>是否进入过约会场景的标记 </summary>
    public bool isEnterSceneMark { set; get; } = false;

    public bool isFirstDating { set; get; } = false;

    /// <summary>到第二天清算时间点强制结算</summary>
    public bool forceSettlement { set; get; } = false;

    private int CompareMissionList(Task t1, Task t2)
    {
        //未完成的任务在最前面
        if (t1.taskState > t2.taskState) return 1;
        if (t1.taskState == t2.taskState) return 0;
        return -1;
    }

    /// <summary>
    /// 设置当前约会场景
    /// </summary>
    /// <param name="sceneType"></param>
    public void SetCurDatingScene(EnumNPCDatingSceneType sceneType)
    {
        m_lastDatingScene = m_curDatingScene;
        m_curDatingScene = sceneType;
    }

    #endregion

    #endregion

    #region 对话回顾界面
    public void AddReviewDialogue(EnumDatingReviewType type,int dialogueId, EnumNPCDatingSceneType sceneType = EnumNPCDatingSceneType.DatingHall)
    {
        SendDialogueReview(dialogueId, sceneType);
    }

    public List<string> listReviewContents { get; private set; } = new List<string>();

    private Dictionary<int, List<DialogReviewData>> m_dicDialogueReviewData = new Dictionary<int, List<DialogReviewData>>();

    /// <summary>
    /// 发送对话记录，暂时通过客户端组装数据，如后期需要调整则改为直接给服务器发送对话id，场景类型，以及其他相关数据
    /// </summary>
    /// <param name="dialogueId"></param>
    /// <param name="sceneType"></param>
    private void SendDialogueReview(int dialogueId, EnumNPCDatingSceneType sceneType)
    {
        CsNpcDatingSetDialogueReview p = PacketObject.Create<CsNpcDatingSetDialogueReview>();

        PDatingReviewData copyP = null;
        PDatingReviewData prd = PacketObject.Create<PDatingReviewData>();
        prd.CopyTo(ref copyP);
        copyP.sceneId = (int)sceneType;
        copyP.dialogueId = dialogueId;
        copyP.divResultData = moduleNPCDating.DivinationResult(dialogueId);
        copyP.shopItem = GetHotelOrderData(dialogueId);

        PDatingReviewData[] tmpArray = new PDatingReviewData[1];
        tmpArray[0] = copyP;
        p.datas = tmpArray;

        session.Send(p);
    }

    private void GetDialogueContents(PDatingReviewData[] datas)
    {
        if (datas == null || datas.Length == 0) return;
        int lastSceneId = -1;
        listReviewContents.Clear();
        List<DialogReviewData> dialogueDatas = new List<DialogReviewData>();
        for (int i = 0; i < datas.Length; i++)
        {
            if (m_dicDialogueReviewData.ContainsKey(datas[i].dialogueId)) dialogueDatas = m_dicDialogueReviewData[datas[i].dialogueId];
            else
            {
                dialogueDatas = moduleStory.GetDialogReviewDatas(datas[i].dialogueId, datas[i].shopItem, datas[i].divResultData);
                if (dialogueDatas != null) m_dicDialogueReviewData.Add(datas[i].dialogueId, dialogueDatas);
            }

            if (lastSceneId != datas[i].sceneId)
            {
                string strTitle = "";
                if (i == 0) strTitle = Util.Format(ConfigText.GetDefalutString(TextForMatType.NpcDatingReview, 5), moduleNPCDating.GetDatingSceneName((EnumNPCDatingSceneType)datas[i].sceneId));
                else strTitle = Util.Format(ConfigText.GetDefalutString(TextForMatType.NpcDatingReview, 5), "\n\r" + moduleNPCDating.GetDatingSceneName((EnumNPCDatingSceneType)datas[i].sceneId));
                listReviewContents.Add(strTitle);
                lastSceneId = datas[i].sceneId;
            }

            for (int j = 0; j < dialogueDatas.Count; j++)
            {
                string content = dialogueDatas[j].content;
                listReviewContents.Add(content);
            }
        }
    }


    /// <summary>
    /// 获取对话记录
    /// </summary>
    public void OpenReviewWindow()
    {
        var p = PacketObject.Create<CsNpcDatingGetDialogueReview>();
        session.Send(p);
    }

    void _Packet(ScNpcDatingGetDialogueReview msg)
    {
        GetDialogueContents(msg.datas);
        Window.ShowAsync<Window_DatingDialogueReview>();
    }

    #region 延迟
    //协程队列
    private Queue mCoroutineQueue = new Queue();
    //当前处理的协程 
    private IEnumerator mCurrentWork = null;

    Action m_reviewCallBack = null;

    public void EnqueueWork(IEnumerator work, Action callback = null)
    {
        m_reviewCallBack = callback;

        enableUpdate = true;
        mCoroutineQueue.Enqueue(work);
    }

    private void OnReviewUpdate(int diff)
    {
        while (mCoroutineQueue.Count > 20)
            DoWork();

        //如果协程队列>10个,则一次执行3个语块
        if (mCoroutineQueue.Count > 10)
            DoWork();

        //如果协程队列>5个,则一次执行2个语块
        if (mCoroutineQueue.Count > 5)
            DoWork();

        DoWork();
    }

    private void DoWork()
    {
        if (mCurrentWork == null && mCoroutineQueue.Count == 0)
        {
            enableUpdate = false;
            m_reviewCallBack?.Invoke();
            m_reviewCallBack = null;
            return;
        }

        if (mCurrentWork == null)
        {
            mCurrentWork = (IEnumerator)mCoroutineQueue.Dequeue();
        }

        //这个协程片段是否执行完毕
        bool mElementFinish = !mCurrentWork.MoveNext();

        if (mElementFinish)
        {
            mCurrentWork = null;
        }
        //如果协程里面嵌套协程
        else if (mCurrentWork.Current is IEnumerator)
        {
            mCurrentWork = (mCurrentWork.Current as IEnumerator);
        }
    }
    #endregion

    #endregion

    #region Npc选择界面
    /// <summary>选择约会Npc</summary>
    public const string EventChooseDatingNpc = "EventNPCDatingChooseDatingNpc";

    /// <summary>Home主界面检测红点用的</summary>
    public bool NeedNotice {
        get
        {
            bool bUnLockDating = false;
            var allNpcs = Module_Npc.instance.allNpcs;
            for (int i = 0; i < allNpcs.Count; i++)
            {
                bUnLockDating = allNpcs[i].isUnlockEngagement;
                if (bUnLockDating) break;
            }
            return bUnLockDating && !isDating;
        }
    }

    /// <summary>
    /// 发送选择约会NPC消息
    /// </summary>
    /// <param name="npcId"></param>
    public void SendSelectDatingNPC(int npcId)
    {
        var p = PacketObject.Create<CsNpcDatingSelectNpc>();
        p.selectNpcId = npcId;
        session.Send(p);

    }

    void _Packet(ScNpcDatingSelectNpc msg)
    {
        if (msg.npc == null) return;
        isFirstDating = true;
        msg.npc.CopyTo(ref m_curDatingNpc);
        DispatchModuleEvent(EventChooseDatingNpc, msg.result, msg.startEventId);
    }

    #endregion

    #region 特效界面

    /// <summary>过渡特效名</summary>
    public string tranEffectName { set; get; }
    #endregion

    #endregion

    #region 约会场景

    #region 占卜场景
    private PDatingDivinationResultData m_resultData = null;
    private Action m_divinationCallBack;

    /// <summary>当前选择的占卜类型</summary>
    public EnumDivinationType divinationType { private set; get; }

    /// <summary>当前占卜结果</summary>
    public PDatingDivinationResultData divinationResult { private set { m_resultData = value; } get { return m_resultData; } }

    private Dictionary<int, PDatingDivinationResultData> m_dicDivinationResultData = new Dictionary<int, PDatingDivinationResultData>();
    public void SetResultData(int dialogueId, PDatingDivinationResultData data)
    {
        if (m_dicDivinationResultData.ContainsKey(dialogueId)) m_dicDivinationResultData[dialogueId] = data;
        else m_dicDivinationResultData.Add(dialogueId, data);
    }

    public PDatingDivinationResultData DivinationResult(int dialogueId)
    {
        if (m_dicDivinationResultData.ContainsKey(dialogueId)) return m_dicDivinationResultData[dialogueId];
        return null;
    }

    /// <summary>
    /// 获取占卜结果
    /// </summary>
    public void GetDivinationResult(EnumDivinationType type, Action callBack)
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        //如果没有剧情对话面板则直接回调，不用弹出GM输入框
        if(moduleStory.currentStory == null) m_divinationCallBack = callBack;
        else DispatchEvent(EventNotifyGMCommond, Event_.Pop(DatingGmType.SetDivinationResult));
#else
        m_divinationCallBack = callBack;
#endif
        divinationType = type;

        var p = PacketObject.Create<CsNpcDatingSceneGetDivinationResult>();
        p.type = (byte)type;
        session.Send(p);
    }

    void _Packet(ScNpcDatingSceneGetDivinationResult msg)
    {
        msg.data.CopyTo(ref m_resultData);

        SetResultData(m_lastStoryId, m_resultData);

        m_divinationCallBack?.Invoke();
        m_divinationCallBack = null;
    }
    #endregion

    #region 餐厅场景
    private Dictionary<int, PShopItem> m_hotelOrderData = new Dictionary<int, PShopItem>();

    /// <summary>
    /// 餐厅商店物品列表
    /// </summary>
    public List<PShopItem> HotelShopItemList { set; get; }

    /// <summary>当前选择的菜品</summary>
    public PShopItem curClickOrderData { set; get; }

    /// <summary>
    /// 根据对话Id获取餐厅菜单
    /// </summary>
    /// <param name="dialogueId">story Id</param>
    /// <returns></returns>
    private PShopItem GetHotelOrderData(int dialogueId)
    {
        if (m_hotelOrderData.ContainsKey(dialogueId)) return m_hotelOrderData[dialogueId];
        return null;
    }

    public void SetHotelOrderData(int dialogueId, PShopItem data)
    {
        if (m_hotelOrderData.ContainsKey(dialogueId))
        {
            m_hotelOrderData.Remove(dialogueId);
        }
        m_hotelOrderData.Add(dialogueId, data);
    }
    #endregion

    #endregion

    #region module override function
    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        ClearDatingData();
    }

    public override void OnUpdate(int diff)
    {
        base.OnUpdate(diff);
        OnReviewUpdate(diff);
    }

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        Module_Task.instance.RemoveEventListener(Module_Task.EventTaskFinishMessage, OnTaskFinished);
        Module_Task.instance.AddEventListener(Module_Task.EventTaskFinishMessage, OnTaskFinished);
        Module_Task.instance.RemoveEventListener(Module_Task.EventTaskBeginMessgage, OnTaskBegin);
        Module_Task.instance.AddEventListener(Module_Task.EventTaskBeginMessgage, OnTaskBegin);
    }

    private void OnTaskBegin(Event_ e)
    {
        if (curDatingScene == EnumNPCDatingSceneType.None) return;
        Task ta = (Task)e.param2;
        if (dicSceneAcceptTask.ContainsKey(curDatingScene)) dicSceneAcceptTask[curDatingScene].Add(ta);
        else
        {
            var keyList = new List<Task> { ta };
            dicSceneAcceptTask.Add(curDatingScene, keyList);
        }
    }

    private void OnTaskFinished(Event_ e)
    {
        if (curDatingScene == EnumNPCDatingSceneType.None) return;
        Task ta = (Task)e.param2;
        if (dicSceneFinishedTask.ContainsKey(curDatingScene)) dicSceneFinishedTask[curDatingScene].Add(ta);
        else
        {
            var keyList = new List<Task>{ ta };
            dicSceneFinishedTask.Add(curDatingScene, keyList);
        }

        //如果在同一个场景接到任务又完成了，那么只提示完成的任务
        if (dicSceneAcceptTask.ContainsKey(curDatingScene))
        {
            var tt = dicSceneAcceptTask[curDatingScene].Find((t) => t.taskID == ta.taskID);
            if (tt != null) dicSceneAcceptTask[curDatingScene].Remove(tt);
        }
    }

    #endregion

    # region 数据清除

    /// <summary>
    /// 清除约会所有缓存数据，包括界面数据
    /// </summary>
    public void ClearDatingData()
    {
        ClearDatingWindowData();
        ClearDatingEventData();
    }

    private void ClearDatingWindowData()
    {
        m_ScNpcDatingEnterScene = null;
        m_ScNpcDatingOpenWindow = null;
        m_curDatingNpc = null;
        isFetterUp = false;
        m_curDatingScene = EnumNPCDatingSceneType.None;
        m_lastDatingScene = EnumNPCDatingSceneType.None;
        isEnterSceneMark = false;
        isNpcBattelSettlement = false;
        dicSceneFinishedTask.Clear();
        dicSceneAcceptTask.Clear();

        //对话回顾界面
        m_dicDialogueReviewData.Clear();

        isFirstDating = false;
        forceSettlement = false;

        //特效界面
        tranEffectName = "";
    }

    private void ClearDatingEventData()
    {
        m_curDatingEventInfo = null;
        m_curDatingEventItem = null;
        m_curDatingEventId = 0;
        m_eventItemIndex = 0;

        //条件
        m_lastStoryId = 0;
        m_settlementState = 0;
        m_giveGiftState = 0;
        m_divinationType = EnumDivinationType.None;
        m_chaseResultState = 0;
        m_settlementResultId = 0;
        m_dicRandomValue.Clear();
        m_curCondition = null;

        //行为
        m_curBehaviours = null;
        m_behaviourIndex = 0;
        datingBehaviourCallBack = null;
        m_curBehaviour = null;

        //断线重连
        moduleAnswerOption.ClearReconnectData();
        reconnectData = null;

        //GM
        isDebug = false;
    }
    #endregion

    #region 配置表

    #region dating Scene Config
    public DatingSceneConfig GetDatingSceneData(EnumNPCDatingSceneType t)
    {
        DatingSceneConfig tmpData = ConfigManager.GetAll<DatingSceneConfig>().Find((item) => item.sceneType == t);
        return tmpData;
    }

    public DatingSceneConfig GetDatingSceneData(int levelId)
    {
        DatingSceneConfig tmpData = ConfigManager.GetAll<DatingSceneConfig>().Find((item) => item.levelId == levelId);
        if (tmpData == null) Logger.LogError("data is null by levelId={0} in datingsceneconfig", levelId);
        return tmpData;
    }

    public EnumNPCDatingSceneType GetDatingSceneType(int levelId)
    {
        DatingSceneConfig tmpData = ConfigManager.GetAll<DatingSceneConfig>().Find((item) => item.levelId == levelId);
        if (tmpData == null) Logger.LogError("data is null by levelId={0} in datingsceneconfig", levelId);
        else return tmpData.sceneType;
        return EnumNPCDatingSceneType.None;
    }

    /// <summary>
    /// 场景名称
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public string GetDatingSceneName(EnumNPCDatingSceneType t)
    {
        if (t == EnumNPCDatingSceneType.DatingHall || t == EnumNPCDatingSceneType.GameHall) return Util.GetString(604);

        DatingSceneConfig dsc = GetDatingSceneData(t);
        if (dsc != null) return Util.GetString(dsc.sceneNameId);
        return string.Empty;
    }
    #endregion

    #region dating event info config

    private DatingEventInfo m_curDatingEventInfo;
    private DatingEventInfo.DatingEventItem m_curDatingEventItem;
    private int m_curDatingEventId = 0;
    private int m_eventItemIndex = 0;
    /// <summary>
    /// 约会事件执行
    /// </summary>
    public void DoDatingEvent(int eventId)
    {
        var data = ConfigManager.Get<DatingEventInfo>(eventId);
        if (data == null)
        {
            Logger.LogError("Dating::  datingEventInfo is null and id = {0}", eventId);
            return;
        }
        m_curDatingEventInfo = data;
        m_curDatingEventId = data.ID;
        m_eventItemIndex = 0;
        CheckNextDatingEventItem();
    }

    public void DoDatingEvent(int eventId,int eventItemId)
    {
        var data = ConfigManager.Get<DatingEventInfo>(eventId);
        if (data == null)
        {
            Logger.LogError("Dating::  datingEventInfo is null and id = {0}", eventId);
            return;
        }
        m_curDatingEventInfo = data;
        m_curDatingEventId = data.ID;
        m_eventItemIndex = GetEventItemIndex(data,eventItemId);
        CheckNextDatingEventItem();
    }


    /// <summary>
    /// 通过数据id获取当前子事件当前的索引，防止策划不按常理出牌，把id顺序打乱
    /// </summary>
    /// <param name="data"></param>
    /// <param name="id"></param>
    /// <returns>默认输出-1</returns>
    private int GetEventItemIndex(DatingEventInfo data, int id)
    {
        for (int i = 0; i < data.datingEventItems.Length; i++) if (data.datingEventItems[i].id == id) return i;
        return 0;
    }

    /// <summary>
    /// 获取下一条约会事件数据
    /// </summary>
    /// <returns></returns>
    public DatingEventInfo.DatingEventItem GetNextEventItemData()
    {
        if (m_curDatingEventInfo == null || m_curDatingEventInfo.datingEventItems == null) return null;
        if (m_eventItemIndex >= m_curDatingEventInfo.datingEventItems.Length) return null;
        return m_curDatingEventInfo.datingEventItems[m_eventItemIndex];
    }

    private void CheckNextDatingEventItem()
    {
        if (m_curDatingEventInfo == null || m_eventItemIndex >= m_curDatingEventInfo.datingEventItems.Length)
        {
            Logger.LogError("Dating::  约会流程未能正常结束，请检查事件配置的条件是否正确，是否能正常进入结束流程或者退出约会场景！！");
            return;
        }

        var data = m_curDatingEventInfo.datingEventItems[m_eventItemIndex];
        m_curDatingEventItem = data;
        if (data == null) CheckNextDatingEventItem();//如果当前这个事件为空，跳过当前执行下一个
        else
        {
            bool bConditionPass = CheckEventCondition(data.conditions);
            if (bConditionPass || isDebug) DatingEventBehaviour(data.behaviours);
            else
            {
                //如果没有符合条件的，索引+1，执行下一个DatingEventItem
                m_eventItemIndex++;
                CheckNextDatingEventItem();
            }
        }
    }

    #region 条件检测

    #region fields
    /// <summary>上一次剧情对话Id</summary>
    private int m_lastStoryId = 0;

    /// <summary>是否需要结算的条件状态 1表示不需要结算，2表示需要结算</summary>
    private sbyte m_settlementState = 0;
    /// <summary>是否需要送礼 1表示不需要送礼，2表示需要送礼 </summary>
    private sbyte m_giveGiftState = 0;
    /// <summary>占卜类型 </summary>
    private EnumDivinationType m_divinationType = EnumDivinationType.None;
    /// <summary>关卡挑战结束后的状态 1表示成功，2表示失败 </summary>
    private sbyte m_chaseResultState = 0;
    /// <summary>约会结算结果</summary>
    private sbyte m_settlementResultId = 0;

    /// <summary>随机出来的值，根据随机组区分</summary>
    private Dictionary<int, int> m_dicRandomValue = new Dictionary<int, int>();

    private DatingEventInfo.Condition m_curCondition;

    #endregion

    /// <summary>检测事件符合条件</summary>
    private bool CheckEventCondition(DatingEventInfo.Condition[] conditions)
    {
        if (conditions == null || conditions.Length == 0) return true;

        int passCount = 0;
        for (int i = 0; i < conditions.Length; i++)
        {
            bool bSubConditionPass = CheckEventCondition(conditions[i]);
            if (bSubConditionPass) passCount++;
        }

        return passCount == conditions.Length;//所有子条件检测通过才算检测通过
    }

    private bool CheckEventCondition(DatingEventInfo.Condition condition)
    {
        m_curCondition = condition;
        bool bCheckPass = false;
        switch (condition.conditionType)
        {
            case DatingEventConditionType.NpcInvite:                bCheckPass = true; break;
            case DatingEventConditionType.EnterDatingScene:         bCheckPass = true; break;
            case DatingEventConditionType.MoodValue:                if (curDatingNpcMsg != null) bCheckPass = CheckMotionLimit(curDatingNpcMsg.mood, condition.moodLimits); break;
            case DatingEventConditionType.FettersValue:             if (curDatingNpcMsg != null) bCheckPass = CheckGoodFeelingLimit(curDatingNpcMsg.nowFetterValue, condition.fettersLimits); break;
            case DatingEventConditionType.Energy:                   if (curDatingNpcMsg != null) bCheckPass = CheckEnergyLimit(curDatingNpcMsg.bodyPower, condition.energyLimits); break;
            case DatingEventConditionType.StoryEnd:                 bCheckPass = condition.storyId == m_lastStoryId; break;
            case DatingEventConditionType.ChooseAnswerItemId:       bCheckPass = CheckLastChooseAnswer(condition.answerId, condition.answerItemId); break;
            case DatingEventConditionType.ChooseAnswerType:         bCheckPass = CheckLastChooseAnswer(condition.answerId, condition.answerType); break;
            case DatingEventConditionType.DivinationType:           bCheckPass = m_divinationType == condition.divinationType; break;
            case DatingEventConditionType.DivinationResult:         bCheckPass = CheckDivinationResult(divinationResult, condition.divinationResultId); break;
            case DatingEventConditionType.SettlementState:          bCheckPass = m_settlementState == condition.settlementState; break;
            case DatingEventConditionType.SettlementResult:         bCheckPass = m_settlementResultId == condition.settlementResultId; break;
            case DatingEventConditionType.GiveGiftState:            bCheckPass = m_giveGiftState == condition.giveGiftState; break;
            case DatingEventConditionType.SettlementEnd:            bCheckPass = true; break;
            case DatingEventConditionType.RandomNumber:             bCheckPass = CheckRandomRange(condition.randomGroupId,condition.randomRange); break;
            case DatingEventConditionType.ChaseFinish:              bCheckPass = m_chaseResultState == condition.chaseResultState; break;
            case DatingEventConditionType.OpenAnswerWindow:         bCheckPass = true; break;
            case DatingEventConditionType.OrderItem:                if (curClickOrderData == null) bCheckPass = true; else bCheckPass = curClickOrderData.itemTypeId == condition.orderItemId; break;
            default:break;
        }

        //如果条件检测不通过，报出提示日志
        //if (!bCheckPass) Logger.LogDetail("Dating::  约会事件表 id={0}----{1} 的条件组，条件id={2},类型 = {3}  的检测结果未通过", m_curDatingEventInfo.ID, curDatingEventItem.id, condition.id, condition.conditionType);

        return bCheckPass;
    }

    private bool CheckGoodFeelingLimit(int goodFeelingValue, int[] limits)
    {
        return CheckIntLimit(goodFeelingValue, limits);
    }

    private bool CheckMotionLimit(int motionValue, int[] limits)
    {
        return CheckIntLimit(motionValue, limits);
    }

    private bool CheckEnergyLimit(int energyValue, int[] limits)
    {
        return CheckIntLimit(energyValue, limits);
    }

    private bool CheckIntLimit(int value, int[] limits)
    {
        if (limits == null || limits.Length == 0) return true;

        if (limits.Length == 1) return value >= limits[0];
        else return value >= limits[0] && value <= limits[1];
    }

    private bool CheckRandomRange(int randomGroupId,int[] range)
    {
        if(range == null || range.Length == 0) Logger.LogError("Dating:: 事件id={0}, 子事件id={1} ,条件id={2}  检测随机值的范围没有配置", m_curDatingEventId, m_curDatingEventItem.id, m_curCondition.id);
        if (m_dicRandomValue.ContainsKey(randomGroupId))
        {
            if (range.Length == 1) return m_dicRandomValue[randomGroupId] == range[0];
            else if(range.Length == 2) return m_dicRandomValue[randomGroupId] >= range[0] && m_dicRandomValue[randomGroupId] <= range[1];
        }
        return false;
    }

    private bool CheckDivinationResult(PDatingDivinationResultData result,int resultId)
    {
        if (result == null)
        {
            Logger.LogError("Dating:: divinationResult is null ");
            return true;
        }
        return resultId == result.id;
    }

    private bool CheckLastChooseAnswer(int answerId,int answerItemId)
    {
        var dicAnswers = moduleAnswerOption.dicClickId;
        bool bPass = dicAnswers.ContainsKey(answerId) && dicAnswers[answerId] == answerItemId;
        return bPass;
    }

    private bool CheckLastChooseAnswer(int answerId, EnumAnswerType answerItemType)
    {
        var dicAnswers = moduleAnswerOption.dicClickType;
        bool bPass = dicAnswers.ContainsKey(answerId) && dicAnswers[answerId] == (int)answerItemType;
        return bPass;
    }

    #endregion

    #region 行为
    /// <summary>所有约会流程结束 </summary>
    public const string EventAllStoryEnd = "EventNpcDatingAllStoryEnd";
    /// <summary>即将开始执行行为</summary>
    public const string EventWillDoBehaviour = "EventNpcDatingWillDoBehaviour";
    /// <summary>GM指令</summary>
    public const string EventNotifyGMCommond = "EventNpcDatingGMCommond";

    #region fields
    private int m_behaviourIndex = 0;
    private DatingEventInfo.Behaviour[] m_curBehaviours;
    private DatingEventInfo.Behaviour m_curBehaviour;
    /// <summary>约会行为公开回调，主要用于从约会场景回到约会主界面时候继续执行整个流程未结束的行为 </summary>
    private Action datingBehaviourCallBack { set; get; }
    #endregion

    private void DatingEventBehaviour(DatingEventInfo.Behaviour[] behaviours)
    {
        if(behaviours == null || behaviours.Length == 0) Logger.LogError("Dating:: 当前执行 事件id={0}, 子事件id={1} 的表没有配置行为", m_curDatingEventId, m_curDatingEventItem.id);

        m_curBehaviours = behaviours;

        //从事件过来执行的行为之前，把行为索引置零
        m_behaviourIndex = 0;
        CheckNextBehaviour();
    }

    /// <summary>
    /// 获取下一个执行的行为数据
    /// </summary>
    /// <returns></returns>
    public DatingEventInfo.Behaviour GetNextBehaviourData()
    {
        if (m_curBehaviours == null) return null;
        int nextBehaviourIndex = m_behaviourIndex + 1;
        if (nextBehaviourIndex >= m_curBehaviours.Length)
        {
            var eventItemData = GetNextEventItemData();
            return eventItemData != null ? eventItemData.behaviours[0] : null;
        }
       return m_curBehaviours[nextBehaviourIndex];
    }

    #region 执行指定行为

    /// <summary>
    /// 跳转执行当前事件的某一个行为
    /// </summary>
    public void SkipToEventBehaviour(DatingEventBehaviourType btype)
    {
        //检查当前事件中所有符合的行为类型
        var beahviour = GetBehaviour(m_curDatingEventId, btype);
        if(beahviour != null) DatingEventBehaviour(beahviour);
    }

    private DatingEventInfo.Behaviour GetBehaviour(int eventId, DatingEventBehaviourType btype)
    {
        var data = ConfigManager.Get<DatingEventInfo>(m_curDatingEventId);
        if (data == null)
        {
            Logger.LogError("Dating::  datingEventInfo is null and id = {0}", m_curDatingEventId);
            return null;
        }

        DatingEventInfo.Behaviour be = m_curBehaviour;
        bool bGetBe = false;

        for (int i = m_eventItemIndex; i < data.datingEventItems.Length; i++)
        {
            var eventItem = data.datingEventItems[i];
            for (int j = 0; j < eventItem.behaviours.Length; j++)
            {
                if (eventItem.behaviours[j].behaviourType == btype)
                {
                    m_eventItemIndex = i;//需要给m_eventItemIndex赋值,不影响后续流程
                    m_curDatingEventItem = data.datingEventItems[i];
                    be = eventItem.behaviours[j];
                    bGetBe = true;
                    break;
                }
            }
            if (bGetBe) break;
        }

        return be;
    }

    #endregion

    private void CheckNextBehaviour()
    {
        if (m_curBehaviours == null) return;
        if (m_behaviourIndex >= m_curBehaviours.Length)
        {
            m_eventItemIndex++;
            CheckNextDatingEventItem();
        }
        else DatingEventBehaviour(m_curBehaviours[m_behaviourIndex]);
    }

    private void DatingEventBehaviour(DatingEventInfo.Behaviour behaviour)
    {
        if (behaviour == null) Logger.LogError("Dating:: behaviour is null,please check it !!!");
        m_curBehaviour = behaviour;

        DispatchEvent(EventWillDoBehaviour, Event_.Pop(behaviour.behaviourType));

        SaveReconnectData();//每次执行行为的时候保存一下数据
        Logger.LogDetail("Dating:: 当前执行 事件id={0}, 子事件id={1} ,行为id={2},行为类型是:{3}", m_curDatingEventId, m_curDatingEventItem.id, behaviour.id, behaviour.behaviourType);
        switch (behaviour.behaviourType)
        {
            case DatingEventBehaviourType.Story:                    ShowStory(behaviour.storyId, behaviour.storyType, NextBehaviourCallBack); break;
            case DatingEventBehaviourType.OpenWindow:               DatingEventBehaviourOpenWindow(behaviour.openWindowName, NextBehaviourCallBack); break;
            case DatingEventBehaviourType.OpenAnswerWindow:         Module_AnswerOption.instance.OpenAnswerWindow(behaviour.answerId, NextBehaviourCallBack); break;
            case DatingEventBehaviourType.Mission:                  DatingEventBehaviourMission(behaviour.createMissionId, NextBehaviourCallBack); break;
            case DatingEventBehaviourType.NotifyFinishMission:      NotifyFinishMission(behaviour.notifyFinishMissionId, NextBehaviourCallBack); break;
            case DatingEventBehaviourType.Chase:                    DatingEventBehaviourChase(behaviour.chaseTaskId); break;
            case DatingEventBehaviourType.TranstionEffect:          PlayTransitionEffect(behaviour.transitionEffectName, NextBehaviourCallBack); break;
            case DatingEventBehaviourType.GetSettlementState:       m_settlementState = curDatingNpc == null ? (sbyte)1 : (sbyte)(curDatingNpc.datingEnd + 1); NextBehaviourCallBack(); break;
            case DatingEventBehaviourType.StartSettlement:          StartSettlement(); break;
            case DatingEventBehaviourType.GetSettlementGiftState:   GetSettlementGiftState(NextBehaviourCallBack); break;
            case DatingEventBehaviourType.StartSettlementGift:      datingBehaviourCallBack = NextBehaviourCallBack; Window.ShowAsync<Window_GiftDating>();break;
            case DatingEventBehaviourType.CreateRandomNumber:       CreateRandomNumber(behaviour.randomGroupId,behaviour.randomRange, NextBehaviourCallBack); break;
            case DatingEventBehaviourType.QuitScene:                QuitScene(NextBehaviourCallBack); break;
            case DatingEventBehaviourType.ReachEvent:               SendFinishEventId(behaviour.reachEventNameId, NextBehaviourCallBack); break;
            case DatingEventBehaviourType.DatingEvent:              DoDatingEvent(behaviour.datingEventId); break;
            case DatingEventBehaviourType.AllStoryEnd:              AllStoryEnd(); break;
            default: break;
        }
    }

    /// <summary>
    /// 继续行为的回调，主要用在约会场景回到约会主界面之后需要继续执行离开场景时的下一个行为
    /// </summary>
    public void ContinueBehaviourCallBack(Action callback = null)
    {
        //如果没有后续的回调，则说明没有后续的行为可执行，直接回调给调用者
        if (datingBehaviourCallBack == null) callback?.Invoke();
        else
        {
            datingBehaviourCallBack.Invoke();
            datingBehaviourCallBack = null;
        }
    }

    private void NextBehaviourCallBack()
    {
        m_behaviourIndex++;//在每次行为回调之后吧行为索引值+1
        CheckNextBehaviour();
    }

    /// <summary>
    /// 通知服务器约会达成的某个事件Id
    /// </summary>
    /// <param name="eventId"></param>
    private void SendFinishEventId(int eventId,Action callback)
    {
        var p = PacketObject.Create<CsNpcDatingEnterSceneEventId>();
        p.eventId = eventId;
        session.Send(p);

        callback?.Invoke();
    }

    private void ShowStory(int storyId,EnumStoryType storyType,Action callBack)
    {
        EnumStoryType tp = EnumStoryType.NpcTheatreStory;
        if (storyType != EnumStoryType.None) tp = storyType;
        AddReviewDialogue(EnumDatingReviewType.Story,storyId, curDatingScene);//记录对话数据
        Module_Story.ShowStory(storyId, tp, true, (sId) => { m_lastStoryId = sId; callBack?.Invoke(); });
    }

    /// <summary>
    /// 退出约会场景
    /// </summary>
    private void QuitScene(Action callBack)
    {
        datingBehaviourCallBack = callBack;
        SendQuitScene();
        //因为约会退出场景的时候，对话无法判断当前对话是否结束，所以在退出场景的时候强行把camera的cull mask设置为UI
        UIManager.SetCameraLayer(Layers.UI);
        Game.GoHome();
    }

    /// <summary>
    /// 约会结算
    /// </summary>
    private void StartSettlement()
    {
        datingBehaviourCallBack = NextBehaviourCallBack;
        Window.ShowAsync<Window_NPCDatingSettlement>();
    }

    /// <summary>
    /// 产生随机数
    /// </summary>
    private void CreateRandomNumber(int randomGroupId,int[] range,Action callBack)
    {
        int randomNumber = 0;
        if (range == null || range.Length == 0)
        {
            Logger.LogError("Dating:: 事件id={0}, 子事件id={1} ,行为id={2}  没有配置生成随机数的范围",m_curDatingEventId, m_curDatingEventItem.id,m_curBehaviour.id);
            randomNumber = 0;
        }

        if (range.Length == 1) randomNumber = range[0];
        else randomNumber = UnityEngine.Random.Range(range[0], range[1]);

        if (m_dicRandomValue.ContainsKey(randomGroupId)) m_dicRandomValue[randomGroupId] = randomNumber;
        else m_dicRandomValue.Add(randomGroupId, randomNumber);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (moduleStory.currentStory == null) callBack?.Invoke();
        else DispatchEvent(EventNotifyGMCommond, Event_.Pop(DatingGmType.SetRandomValue,randomGroupId, range));
#else
        callBack?.Invoke();
#endif
    }

    /// <summary>
    /// 检测结算后送礼
    /// </summary>
    private void GetSettlementGiftState(Action callBack)
    {
        m_giveGiftState = moduleGiftDating.isFetterFullLv || !moduleGiftDating.haveSettlementGift ? (sbyte)1 : (sbyte)2;
        callBack?.Invoke();
    }

    /// <summary>
    /// 播放过渡特效
    /// </summary>
    private void PlayTransitionEffect(string effectName,Action callBack)
    {
        tranEffectName = effectName;
        //如果特效资源名为空则跳过这个行为;
        if (string.IsNullOrEmpty(effectName)) callBack?.Invoke();
        else
        {
            datingBehaviourCallBack = callBack;
            Window.ShowAsync<Window_Datingeffect>();
        }
    }

    private void DatingEventBehaviourOpenWindow(string openWindowName,Action callBack)
    {
        var type = Game.GetType(openWindowName);
        if (type == null) Logger.LogError("window_name = {0} cannot find,please check out", openWindowName);
        else
        {
            datingBehaviourCallBack = callBack;
            Window.ShowAsync(openWindowName);
        }
    }

    private void DatingEventBehaviourMission(int createMissionId,Action callBack)
    {
        byte missionId = (byte)createMissionId;
        //调用任务系统
        moduleTask.SendAccputTask(missionId);
        callBack?.Invoke();
    }

    /// <summary>
    /// 通知服务器当前任务绑定的事件已经完成
    /// </summary>
    /// <param name="eventId"></param>
    private void NotifyFinishMission(int eventId,Action callBack)
    {
        var p = PacketObject.Create<CsNpcDatingLinkTaskEventId>();
        p.eventId = (uint)eventId;
        session.Send(p);
        callBack?.Invoke();
    }

    /// <summary>
    /// PVE关卡挑战
    /// </summary>
    /// <param name="chaseTaskId"></param>
    private void DatingEventBehaviourChase(int chaseTaskId)
    {
        TaskInfo taskConfigInfo = ConfigManager.Get<TaskInfo>(chaseTaskId);
        if (taskConfigInfo == null) Logger.LogError("start a npc dating task is wrong,taskInfo data is null by id={0}", chaseTaskId);
        else modulePVE.OnPVEStart(taskConfigInfo.stageId, PVEReOpenPanel.Dating);
        SendQuitScene();
        SendDatingEventEnd();
    }

    /// <summary>
    /// 关卡挑战结束
    /// </summary>
    /// <param name="chaseState">挑战结果 Success：成功  GameOver：失败</param>
    public void ChaseFinish(PVEOverState chaseState)
    {
        m_chaseResultState = chaseState == PVEOverState.Success ? (sbyte)1 : (sbyte)2;
        NextBehaviourCallBack();
    }

    /// <summary>
    /// 所有剧情流程结束,即当前的事件id执行完成
    /// </summary>
    private void AllStoryEnd()
    {
        SendDatingEventEnd();
        DispatchEvent(EventAllStoryEnd);//某些地方只能接收到全局事件
        DispatchModuleEvent(EventAllStoryEnd);

        //清空相关重连数据
        ClearDatingEventData();
        SaveReconnectData();
        Logger.LogDetail("Dating::  本段约会流程结束");
    }

    #endregion

    #region 预加载的资源
    /// <summary>
    /// 获取约会事件表中的资源
    /// </summary>
    public static List<string> GetDatingPreAssets(int eventId)
    {
        var assetsList = new List<string>();
        DatingEventInfo data = ConfigManager.Get<DatingEventInfo>(eventId);
        if (data == null) return assetsList;

        for (int i = 0; i < data.datingEventItems.Length; i++)
        {
            assetsList.AddRange(_GetDatingPreAssets(data.datingEventItems[i].behaviours));
        }
        assetsList.Distinct();

        return assetsList;
    }

    private static List<string> _GetDatingPreAssets(DatingEventInfo.Behaviour[] behaviours)
    {
        var assetsList = new List<string>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            assetsList.AddRange(_GetDatingPreAssets(behaviours[i]));
        }
        return assetsList;
    }

    private static List<string> _GetDatingPreAssets(DatingEventInfo.Behaviour behaviour)
    {
        var assetsList = new List<string>();
        if (behaviour == null) return assetsList;
        //特效预加载资源
        if (behaviour.behaviourType == DatingEventBehaviourType.TranstionEffect)
        {
            assetsList.Add(behaviour.transitionEffectName);
        }

        //窗口资源
        if (behaviour.behaviourType == DatingEventBehaviourType.OpenWindow)
        {
            assetsList.Add(behaviour.openWindowName);
        }

        //剧情预加载资源
        if (behaviour.behaviourType == DatingEventBehaviourType.Story)
        {
            assetsList.AddRange(Module_Story.GetPerStoryPreAssets(behaviour.storyId, behaviour.storyType));
        }
        return assetsList;
    }

    #endregion

    #region 服务器消息
    /// <summary>
    /// 发送退出场景消息
    /// </summary>
    private void SendQuitScene()
    {
        var p = PacketObject.Create<CsNpcDatingQuitScene>();
        DatingSceneConfig d = GetDatingSceneData(curDatingScene);
        p.levelId = d == null ? 0 : d.levelId;
        session.Send(p);
    }


    /// <summary>
    /// 告诉服务器当前事件id，服务器需要完成成就
    /// </summary>
    private void SendDatingEventEnd()
    {
        var p = PacketObject.Create<CsNpcDatingEventEnd>();
        p.datingEvent = m_curDatingEventInfo.ID;
        session.Send(p);
    }

    #endregion

    #region 约会断线

    public ReconnectData reconnectData { set; get; }

    /// <summary>
    /// 检测约会断线重连
    /// </summary>
    public bool CheckDatingReconnect(EnumNPCDatingSceneType scenetype)
    {
        if (reconnectData == null || reconnectData.bReconnect || scenetype != reconnectData.sceneType) return false;
        DatingEventInfo eventData = ConfigManager.Get<DatingEventInfo>(reconnectData.datingEventId);
        if (eventData == null) return false;
        DatingEventInfo.DatingEventItem eventItemData = eventData.datingEventItems[reconnectData.datingEventItemIndex];
        if (eventItemData == null) return false;
        DatingEventInfo.Behaviour beData = eventItemData.behaviours[reconnectData.datingBehaviourIndex];
        if (beData == null) return false;
        if (beData.behaviourType == DatingEventBehaviourType.OpenAnswerWindow 
            || Level.current is Level_NpcDating
            || (beData.behaviourType == DatingEventBehaviourType.OpenWindow && beData.openWindowName == "window_giftdating"))
            UIManager.SetCameraLayer(Layers.Dialog);//如果刚好是重连到问题面板，需要把相机层级设置到剧情模式

        //恢复相关数据
        m_curDatingEventInfo = eventData;
        m_curDatingEventId = eventData.ID;
        m_curDatingEventItem = eventItemData;
        m_curBehaviours = eventItemData.behaviours;

        reconnectData.bReconnect = true;
        DatingEventBehaviour(beData);
        return true;
    }

    void _Packet(ScNpcDatingDocument p)
    {
        if (p.sceneDocument == null) return;
        PDatingsceneDocument sDoc = null;
        p.sceneDocument.CopyTo(ref sDoc);

        RecoveryDatingData(sDoc);
    }

    /// <summary>
    /// 恢复约会数据
    /// </summary>
    private void RecoveryDatingData(PDatingsceneDocument sDoc)
    {
        if (reconnectData == null) reconnectData = new ReconnectData();
        reconnectData.bReconnect = false;
        reconnectData.sceneType = (EnumNPCDatingSceneType)sDoc.curSceneType;
        reconnectData.datingEventId = sDoc.eventId;
        reconnectData.datingEventItemIndex = sDoc.eventItemIndex;
        reconnectData.datingBehaviourIndex = sDoc.behaviourIndex;
        reconnectData.settlementState = sDoc.settlementState;
        reconnectData.giveGiftState = sDoc.giveGiftState;
        reconnectData.settlementResultId = sDoc.settlementResult;
        reconnectData.lastStoryId = sDoc.lastStoryId;
        reconnectData.divinationType = (EnumDivinationType)sDoc.divinationType;
        reconnectData.chaseResultState = sDoc.chaseResultState;
        PDatingDivinationResultData divResult = null;
        if(sDoc.divResult != null) sDoc.divResult.CopyTo(ref divResult);
        reconnectData.divinationResult = divResult;

        reconnectData.SetDicByArray(ref m_dicRandomValue, sDoc.randomKey, sDoc.randomValue);
        reconnectData.SetDicByArray(ref moduleAnswerOption.dicClickId, sDoc.answerIdKey, sDoc.answerIdValue);
        reconnectData.SetDicByArray(ref moduleAnswerOption.dicClickType, sDoc.answerTypeKey, sDoc.answerTypeValue);
    }

    /// <summary>
    /// 保存数据
    /// </summary>
    private void SaveReconnectData()
    {
        if (reconnectData == null) reconnectData = new ReconnectData();
        reconnectData.bReconnect = true;
        PDatingsceneDocument sDoc = PacketObject.Create<PDatingsceneDocument>();
        sDoc.curSceneType = (int)curDatingScene;
        reconnectData.sceneType = curDatingScene;
        sDoc.eventId = reconnectData.datingEventId;
        sDoc.eventItemIndex = reconnectData.datingEventItemIndex;
        sDoc.behaviourIndex = reconnectData.datingBehaviourIndex;
        sDoc.settlementState = reconnectData.settlementState;
        sDoc.giveGiftState = reconnectData.giveGiftState;
        sDoc.settlementResult = reconnectData.settlementResultId;
        sDoc.lastStoryId = reconnectData.lastStoryId;
        sDoc.divinationType = (int)reconnectData.divinationType;
        sDoc.chaseResultState = reconnectData.chaseResultState;
        sDoc.divResult = reconnectData.divinationResult;

        reconnectData.GetArrayByDic(m_dicRandomValue, ref sDoc.randomKey, ref sDoc.randomValue);
        reconnectData.GetArrayByDic(moduleAnswerOption.dicClickId, ref sDoc.answerIdKey, ref sDoc.answerIdValue);
        reconnectData.GetArrayByDic(moduleAnswerOption.dicClickType, ref sDoc.answerTypeKey, ref sDoc.answerTypeValue);

        var p = PacketObject.Create<CsNpcDatingDocument>();
        p.sceneDocument = sDoc;
        session.Send(p);
    }

    /// <summary>
    /// 约会断线时的数据
    /// </summary>
    public class ReconnectData
    {
        public bool bReconnect = true;
        /// <summary>当前处于哪个场景</summary>
        public EnumNPCDatingSceneType sceneType { set; get; } = EnumNPCDatingSceneType.None;
        /// <summary>约会事件id </summary>
        public int datingEventId { set { instance.m_curDatingEventId = value; } get { return instance.m_curDatingEventId; }}

        /// <summary>事件索引</summary>
        public int datingEventItemIndex { set { instance.m_eventItemIndex = value; } get { return instance.m_eventItemIndex; } }

        /// <summary>行为索引</summary>
        public int datingBehaviourIndex { set { instance.m_behaviourIndex = value; } get { return instance.m_behaviourIndex; } }

        /// <summary>结算状态</summary>
        public sbyte settlementState { set { instance.m_settlementState = value; } get { return instance.m_settlementState; } }

        /// <summary>送礼状态</summary>
        public sbyte giveGiftState { set { instance.m_giveGiftState = value; } get { return instance.m_giveGiftState; } }

        /// <summary>约会结果</summary>
        public sbyte settlementResultId { set { instance.m_settlementResultId = value; } get { return instance.m_settlementResultId; } }

        /// <summary>上一段剧情id</summary>
        public int lastStoryId { set { instance.m_lastStoryId = value; } get { return instance.m_lastStoryId; } }

        /// <summary>占卜类型</summary>
        public EnumDivinationType divinationType { set { instance.m_divinationType = value; } get { return instance.m_divinationType; } }

        /// <summary>关卡挑战结束后的状态</summary>
        public sbyte chaseResultState { set { instance.m_chaseResultState = value; } get { return instance.m_chaseResultState; } }

        /// <summary>占卜结果</summary>
        public PDatingDivinationResultData divinationResult { set { instance.divinationResult = value; } get { return instance.divinationResult; } }

        /// <summary>随机出来的值，根据随机组区分</summary>
        public Dictionary<int, int> dicRandomValue { set { instance.m_dicRandomValue = value; } get{ return instance.m_dicRandomValue; } }

        /// <summary>选择的问题id组</summary>
        public Dictionary<int, int> dicClickId { set { moduleAnswerOption.dicClickId = value; } get { return moduleAnswerOption.dicClickId; } }
        /// <summary>选择的问题类型组</summary>
        public Dictionary<int, int> dicClickType { set { moduleAnswerOption.dicClickType = value; } get { return moduleAnswerOption.dicClickType; } }

        public void SetDicByArray<Tkey, TValue>(ref Dictionary<Tkey, TValue> dict, Tkey[] key, TValue[] val)
        {
            dict = new Dictionary<Tkey, TValue>();
            for (int i = 0; i < key.Length; i++)
            {
                if (dict.ContainsKey(key[i])) dict[key[i]] = val[i];
                else dict.Add(key[i], val[i]);
            }
        }

        public void GetArrayByDic<Tkey,TValue>(Dictionary<Tkey, TValue> dict, ref Tkey[] keys, ref TValue[] values)
        {
            if (dict == null || dict.Count == 0)
            {
                keys = new Tkey[] { };
                values = new TValue[] { };
            }
            else
            {
                keys = new Tkey[dict.Count];
                values = new TValue[dict.Count];
                int count = 0;
                foreach (var pair in dict)
                {
                    keys[count] = pair.Key;
                    values[count] = pair.Value;
                    count++;
                }
            }
        }

    }

    #endregion

    #endregion

    #endregion

    #region GM指令

    /// <summary>当前是否处于debug模式</summary>
    public bool isDebug = false;

    public void GM_DatingCommand(DatingGmType gmType, params object[] args)
    {
        isDebug = true;
        switch (gmType)
        {
            case DatingGmType.ResetDating: GMResetDating(); break;
            case DatingGmType.KickOutLib: GMKickOutLib(); break;
            case DatingGmType.QuitScene: GMQuitScene(); break;
            case DatingGmType.DatingOver: GMDatingOver(); break;
            case DatingGmType.SetDivinationResult: GMSetDivinationResult((int)args[0]); break;
            case DatingGmType.DoDatingEvent: GMDatingEvent((int)args[0], (int)args[1], (int)args[2],(EnumNPCDatingSceneType)args[3]); break;
            case DatingGmType.OpenAnswer: GMOpenAnswer((int)args[0]); break;
            case DatingGmType.SetRandomValue: GMSetRandomValue((int)args[0], (int)args[1]); break;
            case DatingGmType.ForceQuitScene: GMForceQuitScene(); break;
            default:
                break;
        }
    }

    private void GMQuitScene()
    {
        isDebug = false;
        SkipToEventBehaviour(DatingEventBehaviourType.QuitScene);
    }

    /// <summary>
    /// 强制退出约会场景
    /// </summary>
    private void GMForceQuitScene()
    {
        if (Level.current is Level_NpcDating)
        {
            UIManager.SetCameraLayer(Layers.UI);
            Game.GoHome();
        }
    }

    private void GMResetDating()
    {
        ClearDatingData();
        moduleAnswerOption.GMResetDatingState();
    }

    /// <summary>
    /// 踢出图书馆
    /// </summary>
    private void GMKickOutLib()
    {
        moduleAnswerOption.SendLibWrongAnswer();
        GMQuitScene();
    }

    /// <summary>
    /// 约会结束,相当于强制结算
    /// </summary>
    private void GMDatingOver()
    {
        if (Level.current is Level_NpcDating)
        {
            Window.Hide<Window_AnswerOption>();
            SkipToEventBehaviour(DatingEventBehaviourType.QuitScene);
        }
        else StartSettlement();

        isDebug = false;
    }

    /// <summary>
    /// 设置约会占卜结果
    /// </summary>
    private void GMSetDivinationResult(int resultid)
    {
        if (resultid >= 0)
        {
            if (m_resultData == null) m_resultData = PacketObject.Create<PDatingDivinationResultData>();
            m_resultData.id = (uint)resultid;
        }
        isDebug = false;
        NextBehaviourCallBack();
    }

    /// <summary>
    /// 打开问题选项面板
    /// </summary>
    private void GMOpenAnswer(int answerId)
    {
        if (answerId > 0) UIManager.SetCameraLayer(Layers.Dialog);
        Module_AnswerOption.instance.OpenAnswerWindow(answerId, null);
        isDebug = false;
    }

    private void GMSetRandomValue(int groupId,int randomNumber)
    {
        if (groupId >= 0)
        {
            if (m_dicRandomValue.ContainsKey(groupId)) m_dicRandomValue[groupId] = randomNumber;
            else m_dicRandomValue.Add(groupId, randomNumber);
        }
        isDebug = false;
        NextBehaviourCallBack();
    }

    #region 执行GM事件或者行为
    private Action<int, int> m_gmCallBack = null;
    private int m_gmEventId, m_gmEventItemId;
    private void GMDatingEvent(int eventId, int eventItemId, int npcId,EnumNPCDatingSceneType sType)
    {
        //Gm命令创建约会Npc数据
        if (m_curDatingNpc == null)
        {
            m_curDatingNpc = PacketObject.Create<PNpcEngagement>();
            m_curDatingNpc.bodyPower = 10000;
            m_curDatingNpc.datingEnd = 0;
            m_curDatingNpc.mood = 10000;
            m_curDatingNpc.npcId = (ushort)npcId;
        }

        m_gmEventId = eventId;
        m_gmEventItemId = eventItemId;
        m_gmCallBack = DoDatingEvent;
        var data = GetDatingSceneData(sType);
        if (data == null) GMDatingEventCallBack();
        else Game.LoadLevel(data.levelId);
    }

    public void GMDatingEventCallBack()
    {
        //清除相关缓存数据
        ClearDatingEventData();

        m_gmCallBack?.Invoke(m_gmEventId, m_gmEventItemId);
        m_gmCallBack = null;
        m_gmEventId = 0;
        m_gmEventItemId = 0;
        isDebug = false;
    }

    #endregion


    /// <summary>
    /// 检测约会配置表
    /// </summary>
    public static void GMCheckConfig()
    {
        var eventdata = ConfigManager.GetAll<DatingEventInfo>();
        var answerdata = ConfigManager.GetAll<DialogueAnswersConfig>();
        var storydata = ConfigManager.GetAll<StoryInfo>();
        var missiondata = ConfigManager.GetAll<TaskConfig>();
        var chasetaskdata = ConfigManager.GetAll<TaskInfo>();

        for (int i = 0; i < eventdata.Count; i++)
        {
            var eventItem = eventdata[i];
            for (int j = 0; j < eventItem.datingEventItems.Length; j++)
            {
                //条件组
                var item = eventItem.datingEventItems[j];
                for (int k = 0; k < item.conditions.Length; k++)
                {
                    var condition = item.conditions[k];

                    if (condition.conditionType == DatingEventConditionType.None)
                    {
                        Logger.LogError("Dating:: 约会配置表(datingeventinfo.xml)中 id= {0} 子事件id= {1}  条件id= {2} 条件类型= {3}  条件类型配置错误！！！！",
                                 eventItem.ID, item.id, condition.id, condition.conditionType);
                    }
                    //剧情id条件
                    if (condition.conditionType == DatingEventConditionType.StoryEnd)
                    {
                        if (condition.storyId <= 0) Logger.LogError("Dating:: 约会配置表(datingeventinfo.xml)中 id= {0} 子事件id= {1}  条件id= {2} 条件类型= {3} 的配置中 字段:storyId= {4} 配置错误，不应该<= 0！！！！",
                                 eventItem.ID, item.id, condition.id, condition.conditionType,condition.storyId);
                    }
                    //检查问题选项条件
                    else if (condition.conditionType == DatingEventConditionType.ChooseAnswerItemId || condition.conditionType == DatingEventConditionType.ChooseAnswerType)
                    {
                        if (condition.answerId <= 0) Logger.LogError("Dating:: 约会配置表(datingeventinfo.xml)中 id= {0} 子事件id= {1}  条件id= {2}  条件类型= {3} 的配置中 字段:answerId= {4} 配置错误，不应该<= 0！！！！",
                                 eventItem.ID, item.id, condition.id, condition.conditionType,condition.answerId);
                    }
                    //检查占卜类型条件
                    else if (condition.conditionType == DatingEventConditionType.DivinationType)
                    {
                        if (condition.divinationType <= 0) Logger.LogError("Dating:: 约会配置表(datingeventinfo.xml)中 id= {0} 子事件id= {1}  条件id= {2}  条件类型= {3} 的配置中 字段:divinationType= {4} 配置错误，不应该<= 0！！！！",
                                  eventItem.ID, item.id, condition.id, condition.conditionType, condition.divinationType);
                    }
                    //检查占卜结果id条件
                    else if (condition.conditionType == DatingEventConditionType.DivinationResult)
                    {
                        if (condition.divinationResultId <= 0) Logger.LogError("Dating:: 约会配置表(datingeventinfo.xml)中 id= {0} 子事件id= {1}  条件id= {2}  条件类型= {3} 的配置中 字段:divinationResultId= {4} 配置错误，不应该<= 0！！！！",
                                  eventItem.ID, item.id, condition.id, condition.conditionType,condition.divinationResultId);
                    }
                    //检查约会结算状态条件
                    else if (condition.conditionType == DatingEventConditionType.SettlementState)
                    {
                        if (condition.settlementState < 1 || condition.settlementState > 2) Logger.LogError("Dating:: 约会配置表(datingeventinfo.xml)中 id= {0} 子事件id= {1}  条件id= {2}  条件类型= {3} 的配置中 字段:settlementState= {4} 配置错误，结算只有1和2两种状态！！！！",
                                  eventItem.ID, item.id, condition.id, condition.conditionType,condition.settlementState);
                    }
                    //检查约会结算结果条件
                    else if (condition.conditionType == DatingEventConditionType.SettlementResult)
                    {
                        if (condition.settlementResultId < 0) Logger.LogError("Dating:: 约会配置表(datingeventinfo.xml)中 id= {0} 子事件id= {1}  条件id= {2}  条件类型= {3} 的配置中 字段:settlementResultId= {4} 配置错误，不应该 <0！！！！",
                                  eventItem.ID, item.id, condition.id, condition.conditionType,condition.settlementResultId);
                    }
                    //检查送礼状态条件
                    else if (condition.conditionType == DatingEventConditionType.GiveGiftState)
                    {
                        if (condition.giveGiftState < 1 || condition.giveGiftState > 2) Logger.LogError("Dating:: 约会配置表(datingeventinfo.xml)中 id= {0} 子事件id= {1}  条件id= {2}  条件类型= {3} 的配置中 字段:giveGiftState= {4} 配置错误，送不送礼只有1和2两种状态！！！！",
                                  eventItem.ID, item.id, condition.id, condition.conditionType,condition.giveGiftState);
                    }
                    //检查挑战结束状态条件
                    else if (condition.conditionType == DatingEventConditionType.ChaseFinish)
                    {
                        if (condition.chaseResultState < 1 || condition.chaseResultState > 2) Logger.LogError("Dating:: 约会配置表(datingeventinfo.xml)中 id= {0} 子事件id= {1}  条件id= {2}   条件类型= {3} 的配置中 字段:chaseResultState= {4} 配置错误，送不送礼只有1和2两种状态！！！！",
                                  eventItem.ID, item.id, condition.id, condition.conditionType,condition.chaseResultState);
                    }

                }

                //行为组
                if (item.behaviours.Length == 0) Logger.LogError("Dating::  约会配置表(datingeventinfo.xml) id= {0} 子事件id= {1} 没有配置行为！！！！",eventItem.ID, item.id);
                for (int k = 0; k < item.behaviours.Length; k++)
                {
                    var behavior = item.behaviours[k];
                    if (behavior.behaviourType == DatingEventBehaviourType.None)
                    {
                        Logger.LogError("Dating:: 约会配置表(datingeventinfo.xml)中 id= {0} 子事件id= {1}  行为id= {2} 行为类型= {3} 行为类型配置错误！！！！",
                                eventItem.ID, item.id, behavior.id, behavior.behaviourType);
                    }
                    //检查answer
                    if (behavior.behaviourType == DatingEventBehaviourType.OpenAnswerWindow)
                    {
                        var a = answerdata.Find((aItem) => behavior.answerId == aItem.ID);
                        if (a == null)
                        {
                            Logger.LogError("Dating:: 约会配置表(datingeventinfo.xml)中 id= {0} 子事件id= {1}  行为id= {2} 行为类型= {3} 的配置中 字段:answerId= {4}  在dialogueAnswersConfig表中没有找到对应配置！！！！",
                                eventItem.ID, item.id, behavior.id, behavior.behaviourType,behavior.answerId);
                        }
                    }
                    //检查story
                    else if (behavior.behaviourType == DatingEventBehaviourType.Story)
                    {
                        var s = storydata.Find((sItem) => behavior.storyId == sItem.ID);
                        if (s == null)
                        {
                            Logger.LogError("Dating:: 约会配置表(datingeventinfo.xml)中 id= {0} 子事件id= {1}  行为id= {2} 行为类型= {3} 的配置中 字段:storyId= {4}  在storyinfo表中没有找到对应配置！！！！",
                                eventItem.ID, item.id, behavior.id, behavior.behaviourType,behavior.storyId);
                        }
                        else
                        {
                            if (behavior.storyType == EnumStoryType.None)
                            {
                                Logger.LogError("Dating:: 约会配置表(datingeventinfo.xml)中 id= {0} 子事件id= {1}  行为id= {2} 行为类型= {3} 的配置中 字段:storyType= {4}  配置错误！！！！",
                                eventItem.ID, item.id, behavior.id, behavior.behaviourType, (int)behavior.storyType);
                            }
                            else if (behavior.storyType != EnumStoryType.NpcTheatreStory)
                            {
                                Logger.LogDetail("Dating:: 约会配置表(datingeventinfo.xml)中 id= {0} 子事件id= {1}  行为id= {2} 行为类型= {3} 的配置中 字段:storyType= {4}  配置的不是约会剧情类型！！！！",
                               eventItem.ID, item.id, behavior.id, behavior.behaviourType, (int)behavior.storyType);
                            }
                        }
                    }
                    //检查任务列表
                    else if (behavior.behaviourType == DatingEventBehaviourType.Mission)
                    {
                        var m = missiondata.Find((mItem) => behavior.createMissionId == mItem.ID);
                        if (m == null)
                        {
                            Logger.LogError("Dating:: 约会配置表(datingeventinfo.xml)中 id= {0} 子事件id= {1}  行为id= {2} 行为类型= {3} 的配置中 字段:createMissionId= {4}  在taskconfig表中没有找到对应配置！！！！",
                                eventItem.ID, item.id, behavior.id, behavior.behaviourType, behavior.createMissionId);
                        }
                    }
                    //检查PVE挑战
                    else if (behavior.behaviourType == DatingEventBehaviourType.Mission)
                    {
                        var chase = chasetaskdata.Find((c) => behavior.chaseTaskId == c.ID);
                        if (chase == null)
                        {
                            Logger.LogError("Dating:: 约会配置表(datingeventinfo.xml)中 id= {0} 子事件id= {1}  行为id= {2}  行为类型= {3} 的配置中 字段:chaseTaskId= {4}  在taskinfo表中没有找到对应配置！！！！",
                                eventItem.ID, item.id, behavior.id, behavior.behaviourType,behavior.chaseTaskId);
                        }
                    }
                    //检查窗口配置
                    else if (behavior.behaviourType == DatingEventBehaviourType.OpenWindow)
                    {
                        if(string.IsNullOrEmpty(behavior.openWindowName)) Logger.LogError("Dating:: 约会配置表(datingeventinfo.xml)中 id= {0} 子事件id= {1}  行为id= {2} 行为类型= {3} 的配置中 字段:openWindowName 配置错误！！！！",
                                eventItem.ID, item.id, behavior.id, behavior.behaviourType);
                    }
                    //检查通知任务完成配置
                    else if (behavior.behaviourType == DatingEventBehaviourType.NotifyFinishMission)
                    {
                        if (behavior.notifyFinishMissionId <= 0) Logger.LogError("Dating:: 约会配置表(datingeventinfo.xml)中 id= {0} 子事件id= {1}  行为id= {2} 行为类型= {3} 的配置中 字段:notifyFinishMissionId={4} 配置错误,不应该<=0！！！！",
                                 eventItem.ID, item.id, behavior.id, behavior.behaviourType,behavior.notifyFinishMissionId);
                    }
                    //检查特效配置
                    else if (behavior.behaviourType == DatingEventBehaviourType.TranstionEffect)
                    {
                        if (string.IsNullOrEmpty(behavior.transitionEffectName)) Logger.LogError("Dating:: 约会配置表(datingeventinfo.xml)中 id= {0} 子事件id= {1}  行为id= {2} 行为类型= {3} 的配置中 字段:transitionEffectName 配置错误！！！！",
                                 eventItem.ID, item.id, behavior.id, behavior.behaviourType);
                    }
                }
            }
        }

    }

    #endregion

}

