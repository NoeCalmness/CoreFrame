/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-01-16
 * 
 ***************************************************************************************************/
//#define GUIDE_LOG
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using GuideItem = GuideInfo.GuideItem;

public class Module_Guide : Module<Module_Guide>
{
    /// <summary>
    /// 用来设置GM的玩家等级，保证玩家等级条件可以实现
    /// </summary>
    public static int GM_PLATER_LEVEL = 0;
    /// <summary>
    /// 存储默认的操作引导,因为不同职业完成的操作引导也是不同的，所以当玩家已经完成过默认操作引导后，就不再强行跳转到新手第一关的引导
    /// </summary>
    public static List<int> DEFALUT_OPERATE_GUIDES = new List<int>();

    /// <summary>
    /// 引导开启时间戳
    /// </summary>
    public Dictionary<int, int> guideStartTimeDic = new Dictionary<int, int>();


   

    #region const openwindow condition for window

    #region window_home
    /// <summary>  home主界面 </summary>
    public const string WindowHomeMain = "home_main";

    /// <summary>  斗技界面 </summary>
    public const string WindowHomeFight = "home_fight";

    /// <summary> 地下城界面 </summary>
    public const string WindowHomeDungeon = "home_dungeon";

    /// <summary> 商业街 </summary>
    public const string WindowHomeShop = "home_shop";
    #endregion

    #region window_chase

    /// <summary> 追捕默认界面 </summary>
    public const string WINDOWCHASEMAIN         = "chase_main";

    /// <summary> 追捕-通缉令界面 </summary>
    public const string WINDOWCHASEEMERGENCY    = "chase_emergency";

    /// <summary> 追捕-主线关卡界面  </summary>
    public const string WINDOWCHASEEASY         = "chase_easy";

    /// <summary> 追捕-困难难度 </summary>
    public const string WINDOWCHASEDIFF         = "chase_difficult";

    /// <summary> 追捕-活动界面 </summary>
    public const string WINDOWCHASEACTIVE       = "chase_active";

    /// <summary> 追捕-觉醒关卡 </summary>
    public const string WINDOWCHASEAWAKE        = "chase_awake";

    /// <summary> 追捕-噩梦关卡 </summary>
    public const string WINDOWCHASENIGHTMARE    = "chase_nightmare";
    /// <summary>
    /// NPC外传窗口
    /// </summary>
    public const string WINDOWNPCPANEL                = "npc_panel";
    public const string WINDOWNPCCHAPTERPANEL         = "npc_chapterpanel";
    public const string WINDOWNPCSTORYPANEL           = "npc_storypanel";

    public static readonly Dictionary<TaskType, string> CHASE_SUBWINDOW_DIC = new Dictionary<TaskType, string>()
    {
        { TaskType.None,        WINDOWCHASEMAIN },
        { TaskType.Emergency,   WINDOWCHASEEMERGENCY },
        { TaskType.Easy,        WINDOWCHASEEASY },
        { TaskType.Difficult,   WINDOWCHASEDIFF },
        { TaskType.Active,      WINDOWCHASEACTIVE },
        { TaskType.Awake,       WINDOWCHASEAWAKE },
        { TaskType.Nightmare,   WINDOWCHASENIGHTMARE },
    };

    public static readonly Dictionary<GaidenSubWindowNpc, string> GAIDEN_SUBWINDOW_DIC = new Dictionary<GaidenSubWindowNpc, string>()
    {
        {GaidenSubWindowNpc.Panel,WINDOWNPCPANEL },
        {GaidenSubWindowNpc.Chapter,WINDOWNPCCHAPTERPANEL },
        {GaidenSubWindowNpc.Story,WINDOWNPCSTORYPANEL },
    };

    #endregion

    #endregion

    #region 写死的引导ID
    //锻造默认ID
    public const int DEFAULT_WEAPON_ID_IN_FORGING = 1101;
    //锻造引导ID
    public const int FORGING_GUIDE_ID = 300;
    /// <summary>
    /// 觉醒变身的引导ID
    /// </summary>
    public const int AWAKE_MORPH_GUIDE_ID = 45;
    /// <summary>
    /// 抽奖ID
    /// </summary>
    public const int WISH_GUIDE_ID = 216;
    #endregion

    #region enum defination
    public enum EnumServerGuideCondition
    {
        None = 0,
        PlayerCondition = 1,            //包含玩家等级和玩家的pvptimes两种条件
        GuideCondition = 2,             //包含玩家默认引导和完成引导两种条件
        OpenBorderLandCondition = 4,    //无主之地条件
        OpenLabyrinthCondition = 8,     //迷宫条件
        TaskCondition = 16,             //包含任务是否开放和任务完成条件两种条件
        DailyFinishCondition = 32,     //日常任务完成的条件
    }

    #endregion

    #region Trigger Logic

    #region field

    #region debug mode
    private static int m_skipGuide = -1;
    public static bool skipGuide
    {
        get
        {
#if UNITY_EDITOR
            if (m_skipGuide == -1) m_skipGuide = UnityEditor.EditorPrefs.GetInt(SKIP_GUIDE, 0);
            return m_skipGuide > 0;
#elif AUTOMATIC
            return true;
#else
            return false;
#endif
        }
        set
        {
#if UNITY_EDITOR
            int v = value ? 1 : 0;
            if(v != m_skipGuide)
            {
                m_skipGuide = v;
                UnityEditor.EditorPrefs.SetInt(SKIP_GUIDE, m_skipGuide);
            }
#endif
        }
    }
    private const string SKIP_GUIDE = "SkipGuide";

    private static int m_skipStory = -1;
    public static bool skipStory
    {
        get
        {
#if UNITY_EDITOR
            if (m_skipStory == -1) m_skipStory = UnityEditor.EditorPrefs.GetInt(SKIP_STORY, 0);
            return m_skipStory > 0;
#elif AUTOMATIC
            return true;
#else
            return false;
#endif
        }
        set
        {
#if UNITY_EDITOR
            int v = value ? 1 : 0;
            if (v != m_skipStory)
            {
                m_skipStory = v;
                UnityEditor.EditorPrefs.SetInt(SKIP_STORY, m_skipStory);
            }
#endif
        }
    }
    private const string SKIP_STORY = "SkipGuideStory";
    #endregion

    private Dictionary<List<BaseGuideCondition>, GuideInfo> m_allGuideDic = new Dictionary<List<BaseGuideCondition>, GuideInfo>();
    private List<BaseGuideCondition> m_cacheConditions = new List<BaseGuideCondition>();
    private List<int> m_overGuideIds = new List<int>();
    private Window m_lastHideWindow;
    public bool needEnterBordland { get; set; }
    public bool needEnterLabyrinth { get; set; }
    private int m_willLoadGuideID = -1;
    private bool m_checkReconnetction = false;

    /// <summary>
    /// 每收到一个跟服务器相关的消息，就更改一次状态
    /// </summary>
    public EnumServerGuideCondition recvServerCondition { get; private set; }
    private int delayUnlockEventId;
    #endregion

    #region static functions

    public static void LoadGuide(int[] finishGuides)
    {
        InitDefaultOperateGuides();

        instance._LoadGuide(finishGuides);

        instance.UnlockLabyrinth();
        //init home functions
        instance.InitHomeFunctions();
        //to avoid recv msg after window_home
        instance.RefreshUnlockBtns(Window.GetOpenedWindow<Window_Home>());
        moduleGuide.RefreshUnlockBtns(Window.GetOpenedWindow<Window_Global>());
        instance.RefreshBtnUnlockState();

        instance.CreateTempCondition();
        //when we loaded guide data ,then we check imeediatly

        if (!HasFinishDefaultOperateGuides(instance.m_overGuideIds) && !skipGuide)
        {
            modulePVE.OnPVEStart(Module_Story.DEFAULT_STAGE_ID, PVEReOpenPanel.None);
        }
        else
        {
            instance._CheckTriggerGuide();
            moduleGlobal.LockUI();

            //避免其他地方调用了lockui之后导致引导开启的遮挡关不掉
            DelayEvents.Remove(instance.delayUnlockEventId);
            instance.delayUnlockEventId = DelayEvents.Add(moduleGlobal.UnLockUI,5);
        }
    }

    private void UnlockLabyrinth()
    {
        bool valid = m_overGuideIds.Exists(o => o == GeneralConfigInfo.sguideLabyrinthId);
        valid |= currentGuide != null && currentGuide.ID == GeneralConfigInfo.sguideLabyrinthId;
        valid &= modulePlayer.level >= GeneralConfigInfo.sunlockLabyrinthLevel;
        if (valid) moduleLabyrinth.SendUnlockLabyrinth();
    }

    public static void InitDefaultOperateGuides()
    {
        if (DEFALUT_OPERATE_GUIDES.Count > 0) return;

        DEFALUT_OPERATE_GUIDES.Clear();
        var stage = ConfigManager.Get<StageInfo>(Module_Story.DEFAULT_STAGE_ID);
        var eventInfo = ConfigManager.Get<SceneEventInfo>(stage == null ? 0 : stage.sceneEventId);
        if (!eventInfo) return;

        foreach (var item in eventInfo.sceneEvents)
        {
            foreach (var b in item.behaviours)
            {
                if (b.sceneBehaviorType == SceneEventInfo.SceneBehaviouType.StartGuide) DEFALUT_OPERATE_GUIDES.Add(b.parameters.GetValue<int>(0));
            }
        }
    }

    public static bool HasFinishDefaultOperateGuides(List<int> overGuides)
    {
        if (DEFALUT_OPERATE_GUIDES.Count == 0) return false;

        foreach (var item in overGuides)
        {
            if (DEFALUT_OPERATE_GUIDES.Contains(item)) return true;
        }

        return false;
    }

    public static void AddCondition(BaseGuideCondition condition, bool checkTrigger = true)
    {
        instance._AddContidion(condition, checkTrigger);
        if(condition.type == EnumGuideContitionType.StoryEnd)
            instance.OnGuideDialogEnd();
    }
    public static void RemoveCondition(BaseGuideCondition condition)
    {
        instance._RemoveCondition(condition);
    }

    public static void AddSpecialTweenEndCondition()
    {
        AddCondition(new SpecialTweenEndCondition());
    }

    public static void CheckTriggerGuide()
    {
        instance._CheckTriggerGuide();
    }
    #endregion

    #region gm

    public void LoadTargetGuideOnEditorMode(GuideInfo info)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        InitGuideCollection();
        //gm不检测重连
        m_checkReconnetction = true;

        CreatePVPChanllengeCondition(false);

        List<GuideInfo> guides = ConfigManager.GetAll<GuideInfo>();
        foreach (var item in guides)
        {
            if (item.ID >= info.ID)
            {
                List<BaseGuideCondition> cs = new List<BaseGuideCondition>();
                foreach (var c in item.conditions) cs.Add(new BaseGuideCondition(c));
                m_allGuideDic.Add(cs, item);
            }
            else m_overGuideIds.Add(item.ID);
        }

        InitHomeFunctions();
        RefreshBtnUnlockState();
#endif
    }

    public void CreateLevelConditionOnEditorMode(int tarLevel)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (tarLevel <= 0) return;
        RemoveTargetTypeConditon(EnumGuideContitionType.PlayerLevel);
        m_cacheConditions.Add(new PlayerLevelContition(tarLevel));
        _CheckTriggerGuide();
#endif
    }

    #endregion

    #region init

    public void InitGuideCollection()
    {
        m_loadingGuide = false;
        m_checkReconnetction = false;
        recvServerCondition = EnumServerGuideCondition.None;
        delayUnlockEventId = 0;
        m_overGuideIds.Clear();
        m_allGuideDic.Clear();
        m_cacheConditions.Clear();
        m_lastHideWindow = null;

        InitGuideData();
    }

    public void _LoadGuide(int[] overGuides)
    {
        if (overGuides != null && overGuides.Length > 0) InitFinishGuides(overGuides);

        List<GuideInfo> allGuides = ConfigManager.GetAll<GuideInfo>();
        allGuides.Sort((a,b)=> { return a.ID.CompareTo(b.ID); });
        foreach (var item in allGuides)
        {
            if(!IsValidGuide(item)) continue;

            if (item.conditions == null || item.conditions.Length == 0) continue;

            List<BaseGuideCondition> cs = new List<BaseGuideCondition>();
            foreach (var c in item.conditions) cs.Add(new BaseGuideCondition(c));
            m_allGuideDic.Add(cs, item);
        }

#if GUIDE_LOG
        StringBuilder sb = new StringBuilder();
        foreach (var item in m_allGuideDic)
        {
            sb.AppendFormat("{0}\t",item.Value.ID);
        }
        Logger.LogDetail("loaded guide is {0}",sb.ToString());
#endif
    }

    private bool IsValidGuide(GuideInfo info)
    {
        if (m_overGuideIds == null || m_overGuideIds.Count == 0) return true;
        if (info == null) return false;

        //如果是重复的新手引导,需要检测互斥ID
        if (info.repeatGuide) return !m_overGuideIds.Contains(info.mutexId);
        else if(info.ReconnectCheckGuide) return !m_overGuideIds.Contains(info.mutexId);
        else return !m_overGuideIds.Contains(info.ID);
    }

    private void InitFinishGuides(int[] overGuides)
    {
        foreach (var item in overGuides)
        {
            if (m_overGuideIds.Contains(item)) continue;
            m_overGuideIds.Add(item);
        }
    }

    private void InitHomeFunctions()
    {
        List<int> unlockFuncs = new List<int>();

        if(GeneralConfigInfo.defaultConfig.defaultHomeFunc != null) unlockFuncs.AddRange(GeneralConfigInfo.defaultConfig.defaultHomeFunc);

        foreach (var id in m_overGuideIds)
        {
            GuideInfo info = ConfigManager.Get<GuideInfo>(id);
            if (info)
            {
                foreach (var item in info.guideItems)
                {
                    unlockFuncs.AddRange(item.unlockFunctionId);
                }
            }
        }

        if (skipGuide)
        {
            unlockFuncs.Clear();
            List<UnlockFunctionInfo> l = ConfigManager.GetAll<UnlockFunctionInfo>();
            foreach (var item in l) unlockFuncs.Add(item.ID);
        }

        InitUnlockFunctions(unlockFuncs);
    }

    public bool IsGuideTask()
    {
        if (skipGuide || modulePVE.reopenPanelType != PVEReOpenPanel.ChasePanel || moduleChase.lastStartChase == null || !moduleChase.lastStartChase.taskConfigInfo || moduleChase.lastStartChase.taskConfigInfo.ID <= 0) return false;

        return GeneralConfigInfo.NeedForbideRuneAndForgeWhenSettlement(moduleChase.lastStartChase.taskConfigInfo.ID);
    }
    
    #endregion

    #region condition from server

    #region check recv all msg that guide have
    public void AddRecvServerGuideState(EnumServerGuideCondition condition)
    {
        recvServerCondition |= condition;

        var valid = CheckAllGuideConditionFromServer();

        Logger.LogInfo("Guide: Recev server data [type:{0}], all msg recv state [{1}], current level [{2}]", condition, valid, Level.current?.name);

        if (valid && Level.current && (Level.current is Level_Home))
        {
            moduleGlobal.UnLockUI();
            DelayEvents.Remove(instance.delayUnlockEventId);
            CheckReconnection();
        }
    }

    /// <summary>
    /// 检测是不是所有的重连需要的服务器消息都已经收到了
    /// </summary>
    /// <returns></returns>
    public bool CheckAllGuideConditionFromServer()
    {
        bool valid = true;
        valid &= (recvServerCondition & EnumServerGuideCondition.PlayerCondition) > 0;
        valid &= (recvServerCondition & EnumServerGuideCondition.GuideCondition) > 0;
        valid &= (recvServerCondition & EnumServerGuideCondition.OpenBorderLandCondition) > 0;
        valid &= (recvServerCondition & EnumServerGuideCondition.OpenLabyrinthCondition) > 0;
        valid &= (recvServerCondition & EnumServerGuideCondition.TaskCondition) > 0;
        valid &= (recvServerCondition & EnumServerGuideCondition.DailyFinishCondition) > 0;
        return valid;
    }
    #endregion

    #region player
    void _Packet(ScRoleInfo p)
    {
        CreatePlayerLevelCondition();
        CreatePVPChanllengeCondition(modulePlayer.pvpTimes > 0);
        AddRecvServerGuideState(EnumServerGuideCondition.PlayerCondition);
    }

    void _Packet(ScRoleLevelUp p)
    {
        CreatePlayerLevelCondition();
        AddRecvServerGuideState(EnumServerGuideCondition.PlayerCondition);
    }

    private void CreatePlayerLevelCondition()
    {
        int level = modulePlayer.level;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        level = GM_PLATER_LEVEL <= 0 ? modulePlayer.level : GM_PLATER_LEVEL;
#endif
        bool isContain = false;
        foreach (var item in m_cacheConditions)
        {
            if (item.type == EnumGuideContitionType.PlayerLevel && modulePlayer.roleInfo != null)
            {
                item.SetIntParames(0, level);
                isContain = true;
                break;
            }
        }
        if (modulePlayer.roleInfo != null && !isContain) m_cacheConditions.Add(new PlayerLevelContition(level));

        UnlockLabyrinth();
        _CheckTriggerGuide();
    }

    public static void CreatePVPChanllengeCondition(bool chanllenge)
    {
        instance.RemoveTargetTypeConditon(EnumGuideContitionType.PVPChanllenge);
        AddCondition(new PVPChanllengeCondition(chanllenge));
    }

    #endregion

    #region guide

    private void RequestGuideInfo()
    {
        CsGuideInfo p = PacketObject.Create<CsGuideInfo>();
        session.Send(p);
    }

    void _Packet_999(ScGuideInfo p)
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        var ff = p.finishGuides.SimpleClone();
        Array.Sort(ff);
        Logger.LogInfo("Receive finish guides: [{0}]", ff.PrettyPrint());
        #endif
        OnRecvGuideInfo(p.finishGuides);
        AddRecvServerGuideState(EnumServerGuideCondition.GuideCondition); 
    }

    private void SendFinishGuide(int finishGuide, int finishEndTime = -1)
    {
        if (finishEndTime < 0) finishEndTime = Util.GetTimeStamp() - guideStartTimeDic.Get(finishGuide, Util.GetTimeStamp());

        guideStartTimeDic.Remove(finishGuide);

        var p = PacketObject.Create<CsGuideFinish>();
        p.finishGuide = finishGuide;
        p.consumeTime = finishEndTime;
        session.Send(p);
    }
    /// <summary>
    /// 动态调整新手引导执行序号
    /// </summary>
    /// <param name="guidelist"></param>
    public void SendChangeGuide(List<int>guidelist)
    {
        CsGuideSet p = PacketObject.Create<CsGuideSet>();
        p.guides = guidelist.ToArray();
        session.Send(p);
    }

    /// <summary>
    /// 初始化的时候需要将可能需要的条件全部创建一次
    /// </summary>
    public void CreateTempCondition()
    {
        CreateGuideEndCondition();
        CreatePlayerLevelCondition();
    }

    public void CreateGuideEndCondition()
    {
        foreach (var item in m_overGuideIds)
        {
            BaseGuideCondition c = m_cacheConditions.Find<BaseGuideCondition>(o => o.type == EnumGuideContitionType.GuideEnd && o.GetIntParames(0) == item);
            if (c == null) m_cacheConditions.Add(new GuideEndContition(item));
        }

        if (HasFinishDefaultOperateGuides(m_overGuideIds))
        {
            BaseGuideCondition c = m_cacheConditions.Find<BaseGuideCondition>(o => o.type == EnumGuideContitionType.DefalutOperateGuideEnd);
            if (c == null) m_cacheConditions.Add(new DefalutOperateGuideEndCondition());
        }
    }

    #endregion

    #region story
    /// <summary>
    /// send finish story
    /// </summary>
    /// <param name="finishStory"></param>
    public void SendFinishStory(int finishStory)
    {
        CsStoryFinish p = PacketObject.Create<CsStoryFinish>();
        p.finishStory = finishStory;
        session.Send(p);
    }

    /// <summary>
    /// query all finished story
    /// </summary>
    private void SendGetFinishStory()
    {
        CsStoryInfo p = PacketObject.Create<CsStoryInfo>();
        session.Send(p);
    }

    void _Packet(ScStoryInfo info)
    {
        if(info != null && info.finishStories != null)
        {
            #if UNITY_EDITOR || UNITY_DEVELOPMENT
                Logger.LogInfo("Receive finish stories: [{0}]", info.finishStories.PrettyPrint());
            #endif
            foreach (var id in info.finishStories)
            {
                m_cacheConditions.Add(new StoryEndContition(id));
            }
            _CheckTriggerGuide();
        }
    }

    void _Packet(ScStoryFinish rsp)
    {
        //Debug.Log("save story finish ok " + rsp.result);
    }
    #endregion

    #region labyrinth

    void _Packet(ScMazeOpenTime p)
    {
        //先加入条件，再去check condition，
        if (moduleLabyrinth.currentLabyrinthStep == EnumLabyrinthTimeStep.Chanllenge || moduleLabyrinth.currentLabyrinthStep == EnumLabyrinthTimeStep.Rest)
            AddCondition(new OpenLabyrinthContition());
        AddRecvServerGuideState(EnumServerGuideCondition.OpenLabyrinthCondition);
  
    }

    #endregion

    #region borderland
    void _Packet(ScNmlOpenTime p)
    {    
        if (moduleBordlands.isValidBordlandTime) AddCondition(new OpenBordlandContition());
        AddRecvServerGuideState(EnumServerGuideCondition.OpenBorderLandCondition);
    }
    #endregion

    #region task condition

    void _Packet(ScNpcTaskInfo p)
    {
        if (p.npcCopyInfo != null)
        {
            PNpcCopy[] gaidens = null;
            p.npcCopyInfo.CopyTo(ref gaidens);
            foreach (var item in gaidens)
            {
                AddTaskCondition(item.stageInfo);
            }
        }
    }

    void _Packet(ScChaseInfo p)
    {   
        RemoveTargetTypeConditon(EnumGuideContitionType.TaskFinish);
        RemoveTargetTypeConditon(EnumGuideContitionType.TaskChanllenge);
        if (p.chaseList != null) AddTaskCondition(p.chaseList);
        AddRecvServerGuideState(EnumServerGuideCondition.TaskCondition);
    }

    void _Packet(ScChaseTaskUnlock p)
    {
        if (p.chaseList != null) AddTaskCondition(p.chaseList);
        AddRecvServerGuideState(EnumServerGuideCondition.TaskCondition);
    }

    void _Packet(ScChaseStateChange p)
    {
        
        TaskFinishCondition tc = null;
        bool isFinded = false;
        for (int i = 0; i < m_cacheConditions.Count; i++)
        {
            BaseGuideCondition c = m_cacheConditions[i];
            if (c.type != EnumGuideContitionType.TaskFinish) continue;
            tc = (TaskFinishCondition)c;
            if (tc.taskId == p.taskId)
            {
                tc.finish = p.state == 2;
                isFinded = true;
                break;
            }
        }

        if (!isFinded) m_cacheConditions.Add(new TaskFinishCondition(p.taskId, p.state == 2));
        DebugAllCondition("recv ScChaseStateChange......");
        AddRecvServerGuideState(EnumServerGuideCondition.TaskCondition);
        _CheckTriggerGuide();
    }

    private void AddTaskCondition(PChaseTask[] tasks)
    {
        if (tasks == null || tasks.Length == 0) return;

        foreach (var item in tasks)
        {
            m_cacheConditions.Add(new TaskFinishCondition(item.taskId, item.state == (byte)EnumChaseTaskFinishState.Finish));
            m_cacheConditions.Add(new TaskChanllengeCondition(item.taskId, item.times > 0));
        }
        DebugAllCondition("AddTaskCondition......");
        _CheckTriggerGuide();
    }

    public void UpdateTaskChanllengeCondition(ChaseTask task)
    {
        TaskChanllengeCondition tc = null;
        TaskFinishCondition tf = null;
        bool isFindedChanllenge = false;
        bool isFindedTaskFinish = false;
        for (int i = 0; i < m_cacheConditions.Count; i++)
        {
            BaseGuideCondition c = m_cacheConditions[i];
            if (!isFindedChanllenge && c.type == EnumGuideContitionType.TaskChanllenge)
            {
                tc = (TaskChanllengeCondition)c;
                if (tc.taskId == task.taskData.taskId)
                {
                    tc.chanllenge = task.taskData.times > 0;
                    isFindedChanllenge = true;
                }
            }

            if(!isFindedTaskFinish && c.type == EnumGuideContitionType.TaskFinish)
            {
                tf = (TaskFinishCondition)c;
                if (tf.taskId == task.taskData.taskId)
                {
                    tf.finish = task.taskData.successTimes > 0;
                    isFindedTaskFinish = true;
                }
            }

            if (isFindedTaskFinish && isFindedChanllenge)
                break;
        }

        if (!isFindedChanllenge)
            m_cacheConditions.Add(new TaskChanllengeCondition(task.taskData.taskId, task.taskData.times > 0));
        if(!isFindedTaskFinish)
            m_cacheConditions.Add(new TaskFinishCondition(task.taskData.taskId, task.taskData.successTimes>0));
        _CheckTriggerGuide();
    }

    public void RecreateAllTaskCondition()
    {
        if (moduleChase.allChaseTasks == null) return;

        RemoveTargetTypeConditon(EnumGuideContitionType.TaskFinish);
        RemoveTargetTypeConditon(EnumGuideContitionType.TaskChanllenge);

        foreach (var item in moduleChase.allChaseTasks)
        {
            m_cacheConditions.Add(new TaskFinishCondition(item.taskData.taskId, item.taskData.state == (byte)EnumChaseTaskFinishState.Finish));
            m_cacheConditions.Add(new TaskChanllengeCondition(item.taskData.taskId, item.taskData.times > 0));
        }
        DebugAllCondition("RecreateAllTaskCondition.....");
        _CheckTriggerGuide();
    }

    #endregion

    #region dailyCondition

    void _Packet(ScDailyTaskInfo p)
    {
        CheckDailyTasks(p.infoList);
        AddRecvServerGuideState(EnumServerGuideCondition.DailyFinishCondition);
    }

    void _Packet(ScDailyStateChange p)
    {
        if (p.newState == 1) AddDailyFinishCondition();
        AddRecvServerGuideState(EnumServerGuideCondition.DailyFinishCondition);
    }

    void _Packet(ScDailyTaskOpen p)
    {
        CheckDailyTasks(p.tasks);
        AddRecvServerGuideState(EnumServerGuideCondition.DailyFinishCondition);
    }

    private void CheckDailyTasks(PDailyInfo[] dailys)
    {
        bool isAdd = false;
        foreach (var item in dailys)
        {
            if (item.state == 1)
            {
                isAdd = true;
                break;
            }
        }

        if (isAdd) AddDailyFinishCondition();
    }

    private void AddDailyFinishCondition()
    {
        RemoveTargetTypeConditon(EnumGuideContitionType.DailyFinish);
        _AddContidion(new DailyFinishCondition());
    }

    #endregion

    #endregion

    #region check

    private void _AddContidion(BaseGuideCondition condition,bool checkTrigger = true)
    {
        if (!condition.allowMultiple)
            RemoveTargetTypeConditon(condition.type);
        m_cacheConditions.Add(condition);
        DebugAllCondition(Util.Format("[add new guide condition : {0}]", condition.ToString()));
        if (checkTrigger) _CheckTriggerGuide();
    }
    private void _RemoveCondition(BaseGuideCondition condition)
    {
        int index = 0;
        while (index != m_cacheConditions.Count)
        {
            if (m_cacheConditions[index].IsSameCondition(condition)) m_cacheConditions.RemoveAt(index);
            else index++;
        }
    }

    public void RemoveTargetTypeConditon(EnumGuideContitionType type)
    {
        int index = 0;
        while (index != m_cacheConditions.Count)
        {
            if (m_cacheConditions[index].type == type) m_cacheConditions.RemoveAt(index);
            else index++;
        }
    }

    private bool CheckWillGuide()
    {
        bool suceess = false;
        List<BaseGuideCondition> conditions = null;

        foreach (var item in m_allGuideDic)
        {
            suceess = true;
            //check conditions
            foreach (var c in item.Key)
            {

                if (c.type == EnumGuideContitionType.OpenWindow || c.type == EnumGuideContitionType.SpecialTweenEnd)
                    continue;
                else
                    suceess = HasSameConditionInCache(c);
                if (!suceess) break;
            }

            //二次检索，如果是重复引导，检索条件(Playerlevel)只能是等于
            if (suceess && CheckMutexGuideLevel(item.Value))
            {
                conditions = item.Key;
                break;
            }
        }

        //DebugAllCondition(Util.Format("[是否:{0}满足条件的引导需要触发!]",suceess));
        if (suceess && conditions != null)
            return true;
        return false;
    }
    
    private void _CheckTriggerGuide()
    {
        //we are already in guide
        if (SkipGuide()) return;

        bool suceess = true;
        List<BaseGuideCondition> removeKey = null;

        foreach (var item in m_allGuideDic)
        {
            suceess = true;
            //check conditions
            foreach (var c in item.Key)
            {
                suceess = HasSameConditionInCache(c);
                if (!suceess) break;
            }

            //二次检索，如果是重复引导，检索条件(Playerlevel)只能是等于
            if (suceess && CheckMutexGuideLevel(item.Value))
            {
                removeKey = item.Key;
                break;
            }

           
        }
        TriggerGuide(removeKey);
    }

    private bool SkipGuide()
    {
        if (currentGuideItem != null && currentGuideItem.canBeBreaked) return false;

        return skipGuide || m_allGuideDic == null;
    }

    private void TriggerGuide(List<BaseGuideCondition> key)
    {
        if (key == null || key.Count == 0)
        {
            DispatchModuleEvent(EventTriggerDifferentGuideStory);
            return;
        }

        GuideInfo info = m_allGuideDic.Get(key);
        if(info == null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            foreach (var item in key)
            {
                sb.AppendLine(item.ToString());
            }
            Logger.LogError("key is valid but the guideinfo is null,key is : {0}", sb.ToString());
            DispatchModuleEvent(EventTriggerDifferentGuideStory);
            return;
        }
        
        DebugAllCondition("---before trigger guide ,all condition has :");
        if (currentGuide != null && !IsGuideEnd(currentGuide.ID)) return;
        if (key != null)
        {
            //remove used conditions
            //RemoveConditions(key); 不能删除已经用的条件，这样
           
            if (!info.repeatGuide)
            { 
                //remove normal guide
                m_allGuideDic.Remove(key);
                //remove mutex guide
                RemoveMutexGuide(info.ID);
            }
            else
            {
                //remove menu condition from cacheConditions
                foreach(var tmp in key)
                {
                    if(tmp.type == EnumGuideContitionType.OpenWindow)
                    {
                        foreach(var cc in m_cacheConditions)
                        {
                            if(cc.IsSameCondition(tmp))
                            {
                                m_cacheConditions.Remove(cc);
                                break;
                            }
                        }
                    }
                }
            }
        }

#if GUIDE_LOG
        if (info)
        {
            Logger.LogInfo("---trigger guide info ,info is {0}", info.ID);
            DebugAllCondition("---after trigger guide ,all condition has :");
        }
#endif

        //cancel the guide story mask
        if (info && m_willLoadGuideID > 0 && info.ID != m_willLoadGuideID) DispatchModuleEvent(EventTriggerDifferentGuideStory);
        m_willLoadGuideID = -1;

        //special handle for target guide
        if(info && moduleRune)
        {
            if(info.ID == GeneralConfigInfo.sguideRuneEvolveId)
            {
                //fill up the rune that need display first
                foreach (var item in info.guideItems)
                {
                    if (item.hotAreaData.restrainType == EnumGuideRestrain.CheckID) moduleRune.excludeSortID.Add((ushort)item.hotAreaData.restrainId);
                }
            }
            else if(info.ID == GeneralConfigInfo.sguideForceEquipRuneId)
            {
                //fill up the rune that need force equip
                foreach (var item in info.guideItems)
                {
                    if (item.hotAreaData.restrainType == EnumGuideRestrain.Rune && item.hotAreaData.validRuneCondition)
                    {
                        moduleRune.forceRune = PacketObject.Create<PItem2>();
                        moduleRune.forceRune.itemTypeId = (ushort)item.hotAreaData.restrainParames.GetValue<int>(0);
                        moduleRune.forceRune.level = (byte)item.hotAreaData.restrainParames.GetValue<int>(1);
                        moduleRune.forceRune.star = (byte)item.hotAreaData.restrainParames.GetValue<int>(2);
                        break;
                    }
                }
            }
        }
      
        if(currentGuide != null && info != null && currentGuide.ID == info.ID)
        {
            //skip load guide 
            return;
        }
        //open guide window
        if (info) ShowGuide(info);
    }

    private void RemoveMutexGuide(int id)
    {
        if (id == 0) return;

        List<BaseGuideCondition> key = null;
        foreach (var item in m_allGuideDic)
        {
            if(item.Value.repeatGuide && item.Value.mutexId == id)
            {
                key = item.Key;
                break;
            }
        }

        if(key != null) m_allGuideDic.Remove(key);
    }

    /// <summary>
    /// 检查重连，每次启动有且仅有一次执行，不管之前执行了任何操作
    /// 重连的时候肯定已经收到所有的消息了，所以可以知道是不是已经能够触发了
    /// </summary>
    public void CheckReconnection()
    {
        //消息没收完的话，就不执行重连操作
        if (!CheckAllGuideConditionFromServer()) return;

        if (SkipGuide() || !HasFinishDefaultOperateGuides(instance.m_overGuideIds) || m_checkReconnetction)
        {
            m_checkReconnetction = true;
            return;
        }
        
        m_checkReconnetction = true;

        //if current guide is not null. return
        if(currentGuide != null)
        {
            return;
        }

        var validGuides = GetAllValidGuide();
#if GUIDE_LOG
        StringBuilder s = new StringBuilder("valid guides has:\t ");
        foreach (var item in validGuides)
        {
            s.AppendFormat("{0}\t",item.ID);
        }
        Logger.LogDetail(s.ToString());
#endif

        GuideInfo guide = null;
        List <BaseGuideCondition> key = null;
        if (validGuides != null && validGuides.Count > 0)
        {
            guide = validGuides[0];
            Logger.LogInfo("success reconnection guide with id {0}", guide.ID);
            foreach (var item in m_allGuideDic)
            {
                if(item.Value.ID == guide.ID)
                {
                    key = item.Key;
                    break;
                }
            }
        }
        if(currentGuide!=null&&guide==null)
        {
            guide = currentGuide;
            List<BaseGuideCondition> cs = new List<BaseGuideCondition>();
            foreach (var c in currentGuide.conditions) cs.Add(new BaseGuideCondition(c));
            key = cs;
        }
        
        if (key != null && guide)
        {
            string openWindow = string.Empty;
            foreach (var item in key)
            {
                if (item.type == EnumGuideContitionType.OpenWindow) openWindow = item.strParames;
                else if (item.type == EnumGuideContitionType.SpecialTweenEnd) _AddContidion(new SpecialTweenEndCondition());
            }
            Logger.LogInfo("success reconnection guide with id {0} OpenWindow :{1}", guide.ID, openWindow);
            if (Game.GetDefaultName<Window_PVP>() == openWindow)
            {
                modulePVP.opType = OpenWhichPvP.FreePvP;
                Window.ShowAsync<Window_PVP>();
            }
            else if (IsOpenWindowHome(openWindow))
            {
                // 主界面需要打开指定的子界面
                Window.GotoSubWindow<Window_Home>(GetSubwindowTypeForWindowHome(openWindow));
            }
            else if (IsOpenWindowChase(openWindow))
            {
                // 跳转到指定追捕子界面
                //Window.GotoSubWindow<Window_Chase>((int)GetSubwindowTypeForWindowChase(openWindow));
                Window.ShowAsync<Window_Chase>(null,
                         (w) => (w as Window_Chase)?.GoToSubWindow(GetSubwindowTypeForWindowChase(openWindow)));
            }
            else if (IsOpenWindowNpcGaiden(openWindow))
            {
                //跳转到外传子界面
                Window.ShowAsync<Window_NpcGaiden>(null,
                            (w) => (w as Window_NpcGaiden)?.GotoSubWindow(moduleGuide.GetSubWindowTypeForGaidenWindow(openWindow), TaskType.GaidenChapterOne));
            }
            else if (!string.IsNullOrEmpty(openWindow))
            {
                if (openWindow.Equals(Game.GetDefaultName<Window_Bordlands>())) moduleBordlands.Enter();
                else if (openWindow.Equals(Game.GetDefaultName<Window_Labyrinth>())) moduleLabyrinth.SendLabyrinthEnter();
                else Window.ShowAsync(openWindow);
            }
        }
    }

    private List<GuideInfo> GetAllValidGuide()
    {
        List<GuideInfo> validGuides = new List<GuideInfo>();
        
        foreach (var item in m_allGuideDic)
        {
            if (!item.Value || item.Value.conditions == null || item.Value.conditions.Length == 0 ||
               item.Value.skipReconnect || !IsValidGuide(item.Value) || !CheckMutexGuideLevel(item.Value)) continue;

            if (WillSuccessTriggerGuide(item.Key)) validGuides.Add(item.Value);
        }
        validGuides.Sort((a,b)=>a.ID.CompareTo(b.ID));
        return validGuides;
    }

    private bool WillSuccessTriggerGuide(List<BaseGuideCondition> info)
    {
        var valid = true;
        foreach (var item in info)
        {
            //忽略以下条件的检测 1.打开界面（包括场景） 2.特殊动画完成
            if (item.type == EnumGuideContitionType.OpenWindow || item.type == EnumGuideContitionType.SpecialTweenEnd) continue;

            valid = HasSameConditionInCache(item);
            if (!valid) break;
        }

        return valid;
    }

    /// <summary>
    /// 重复引导需要验证等级是否合法，而等级从该引导的mutexId去获取
    /// 主要是为了解决比如5-10级的时候，需要一直有提示性引导
    /// </summary>
    /// <param name="guide"></param>
    /// <returns></returns>
    private bool CheckMutexGuideLevel(GuideInfo guide)
    {
        if (!guide.repeatGuide || guide.mutexId <= 0) return true;

        GuideInfo mutexGuide = ConfigManager.Get<GuideInfo>(guide.mutexId);
        if (!mutexGuide) return true;

        int tarLevel = 0;
        foreach (var item in mutexGuide.conditions)
        {
            if(item.type == EnumGuideContitionType.PlayerLevel)
            {
                tarLevel = item.intParames[0];
                break;
            }
        }

        if (tarLevel == 0) return true;

        return modulePlayer.level < tarLevel;
    }

    private void DebugAllCondition(string msg = "")
    {
#if GUIDE_LOG
        StringBuilder s = new StringBuilder();
        if (!string.IsNullOrEmpty(msg)) s.AppendLine(msg);

        s.AppendLine();
        foreach (var item in m_cacheConditions)
        {
            s.Append(item.ToString());
            s.AppendLine();
        }
        Logger.LogInfo(s.ToString());
#endif
    }

    private bool HasSameConditionInCache(BaseGuideCondition condition)
    {
        foreach (var item in m_cacheConditions)
        {
            if (item.IsSameCondition(condition)) return true;
        }

        return false;
    }

    private void RemoveConditions(List<BaseGuideCondition> conditions)
    {
        foreach (var item in conditions)
        {
            for (int i = 0; i < m_cacheConditions.Count; i++)
            {
                if (m_cacheConditions[i].IsSameCondition(item))
                {
                    m_cacheConditions.RemoveAt(i);
                    break;
                }
            }
        }
    }

    #endregion

    #region hanle module guide

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();

        if(guideComponentDic != null)
        {
            foreach (var item in guideComponentDic) item.Value.OnGameDataReset();
        }
        InitGuideCollection();
        ResetAllUnlockBtn();

        EventManager.RemoveEventListener(Events.UI_WINDOW_VISIBLE, OnWindowDisplay);
        EventManager.RemoveEventListener(Events.UI_WINDOW_HIDE, OnWindowHide);
        EventManager.RemoveEventListener(Events.UI_WINDOW_ON_DESTORY, OnWindowDestroy);
        EventManager.RemoveEventListener(Events.SCENE_LOAD_COMPLETE, OnSceneLoadComlete);

        UIManager.instance.RemoveEventListener(Events.UI_WINDOW_WILL_OPEN, OnWindowWillOpen);

        //cancel the fullscreen lock image
        moduleGlobal.UnLockUI(Module_Global.GUIDE_LOCK_PRIORITY);
        DispatchModuleEvent(EventTriggerDifferentGuideStory);
    }

    protected override void OnWillEnterGame()
    {
        base.OnWillEnterGame();

        InitGuideCollection();
        EventManager.AddEventListener(Events.UI_WINDOW_VISIBLE, OnWindowDisplay);
        EventManager.AddEventListener(Events.UI_WINDOW_HIDE, OnWindowHide);
        EventManager.AddEventListener(Events.SCENE_LOAD_COMPLETE, OnSceneLoadComlete);
        EventManager.AddEventListener(Events.UI_WINDOW_ON_DESTORY, OnWindowDestroy);
        UIManager.instance.AddEventListener(Events.UI_WINDOW_WILL_OPEN, OnWindowWillOpen);
    }

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        RequestGuideInfo();
        SendGetFinishStory();
    }

    private void OnRecvGuideInfo(int[] finishGuide)
    {
        if(skipGuide)
        {
            List<int> list = new List<int>();
            List<GuideInfo> guides = ConfigManager.GetAll<GuideInfo>();
            foreach (var item in guides) list.Add(item.ID);
            finishGuide = list.ToArray();
        }
        
        LoadGuide(finishGuide);
        //必须要收到消息添加解锁功能之后再判断是否是需要打开活动界面
        moduleWelfare.IsFristEnter();
    }

    private void OnWindowWillOpen(Event_ e)
    {
        string windowName = (string)e.param1;

        bool isWindowHome = windowName.Equals(Game.GetDefaultName<Window_Home>());
        if (isWindowHome)
        {
            //主界面打开后刷新条件(提前)
            CreateGuideEndCondition();
            CreatePlayerLevelCondition();
        }
        
        if (CheckWillTriggerStory(windowName)) DispatchModuleEvent(EventWillTriggerGuideStory);
    }

    private bool CheckWillTriggerStory(string windowName)
    {
        if (string.IsNullOrEmpty(windowName) || skipGuide || skipStory 
            || !HasFinishDefaultOperateGuides(instance.m_overGuideIds) 
            || !CheckValidTriggerStoryWindow(windowName)) return false;

        GuideInfo info = null;
        bool openStory = false;
        string str = "";
        if(currentGuide)
        {
            //repeat guide is invalid
            if (currentGuide.repeatGuide) return false;

            //the last guide,then currentGuide will be end,so we need check a new guide
            if(m_guideIndex == currentGuide.guideItems.Length - 1)
            {
                //tip guide is skip check
                GuideItem lastItem = currentGuide.GetGuideItem(m_guideIndex);
                if (lastItem.type == EnumGuideType.TipGuide) return false;
                if (lastItem.hotWindow != Game.GetDefaultName<Window_Global>() && lastItem.hotWindow != Game.GetDefaultName<Window_Home>()) return false;

                info = GetWillTriggerGuideInfo(windowName,currentGuide.ID);
                openStory = CanWeStartStory(info, 0);
                str = "will guide end";
            }
            //new guide
            else if(m_guideIndex == 0 && currentGuideItem == null)
            {
                info = currentGuide;
                //因为跳转界面的时候还未处理m_guideInde++的操作，都还是在跳转场景这一帧，所以检测需要从m_guideIndex + 1 开始检测
                openStory = CanWeStartStory(info, 0);
                str = "current guide check index is 0";
            }
            //other guide
            else
            {
                info = currentGuide;
                //因为跳转界面的时候还未处理m_guideInde++的操作，都还是在跳转场景这一帧，所以检测需要从m_guideIndex + 1 开始检测
                openStory = CanWeStartStory(info,m_guideIndex + 1);
                str = Util.Format("current guide check ,index is {0}", m_guideIndex + 1);
            }
        }
        else
        {
            info = GetWillTriggerGuideInfo(windowName);
            openStory = CanWeStartStory(info, 0);
            str = "new guide check";
        }

        m_willLoadGuideID = info == null ? -1 : info.ID;
        Logger.LogDetail(" check for window [{3}], reason is {2} guide info.id = {0}  result is {1}",info == null ? -1 : info.ID,openStory ? "open story" : "nothing",str,windowName);
        return openStory;
    }

    private static readonly Type[] InvalidTriggerStoryWindows = new Type[]
    {
        typeof(Window_Alert),   typeof(Window_Countdown),       typeof(Window_Debug),
        typeof(Window_Gm),      typeof(Window_DefaultLoading),  typeof(Window_Name),
        typeof(Window_Sex),     typeof(Window_Interlude),       typeof(Window_ItemTip),
        typeof(Window_Charge),  typeof(Window_Gift),            typeof(Window_ApplyFriend),
    };

    private bool CheckValidTriggerStoryWindow(string windowName)
    {
        //如果幕间动画在播放的时候，不进行其他检查，因为幕间动画会强行将界面切回window_home_main
        var interludeWindow = Window.GetOpenedWindow<Window_Interlude>();
        bool interActive = interludeWindow && interludeWindow.actived;

        var valid = true;
        foreach (var item in InvalidTriggerStoryWindows)
        {
            if(windowName.Equals(Game.GetDefaultName(item)))
            {
                valid = false;
                break;
            }
        }

        //Logger.LogDetail("interActive = {0},check window is {1}  valid window is {2}", interActive,windowName,valid);
        return !interActive && valid;
    }

    private GuideInfo GetWillTriggerGuideInfo(string windowName,int willEndGuide = -1)
    {
        GuideInfo info = null;
        bool suceess = true;
        if (m_allGuideDic == null)
            return info;
        foreach (var item in m_allGuideDic)
        {
            suceess = true;
            foreach (var c in item.Key)
            {
                if (c.type == EnumGuideContitionType.OpenWindow)
                {
                    if (IsOpenWindowHome(c.strParames)) suceess = JudgeWindowHomeCondition(c.strParames);
                    else if (IsOpenWindowNpcGaiden(c.strParames, windowName)) suceess = JudgeWindowGaidenCondition(c.strParames);
                    else suceess = c.strParames.Equals(windowName);
                }
                else if (c.type == EnumGuideContitionType.GuideEnd && willEndGuide > 0) suceess = c.GetIntParames(0) == willEndGuide;
                else suceess = HasSameConditionInCache(c);

                if (!suceess) break;
            }

            if (suceess)
            {
                info = item.Value;
                break;
            }
        }
        return info;
    }

    private bool JudgeWindowHomeCondition(string subWindow)
    {
        var panel = moduleHome.windowPanel;
        var sub = panel > -1 && panel < m_homePanles.Length ? m_homePanles[panel] : string.Empty;
        return !string.IsNullOrEmpty(sub) && sub.Equals(subWindow) && Level.current && Level.current.isNormal;
    }
    private bool JudgeWindowGaidenCondition(string subWindow)
    {
        string sub = IsGaidenSubPanel(subWindow);
        return !string.IsNullOrEmpty(sub) && sub.Equals(subWindow) && Level.current && Level.current.isNormal; ;
    }

    private string IsGaidenSubPanel(string name)
    {
        foreach (var item in GAIDEN_SUBWINDOW_DIC)
        {
            if (name.Equals(item.Value)) return item.Value;
        }
        return string.Empty;
    }

    private bool CanWeStartStory(GuideInfo info,int beginIndex)
    {
        if (!info) return false;

        //只检查guide item index = beginIndex 和 beginIndex + 1 的元素会不会成功打开对话,因为返回到主界面可能有二级面板的检查
        GuideItem item = info.GetGuideItem(beginIndex);
        if (item != null && item.type == EnumGuideType.Dialog) return true;

        return false;
    }
    private void OnWindowDestroy(Event_ e)
    {
         Window w = e.param1 as Window;
        if(w!=null)
        _RemoveCondition(new OpenWindowCondition(w.name));
    }

    private void OnWindowHide(Event_ e)
    {
        Window w = e.sender as Window;
        if (w && currentGuide && currentGuideItem != null)
        {
            GuideBase c = guideComponentDic.Get(currentGuideItem.type);
            if (c)
            {
                //refresh guideItem on window
                c.gameObject.SetActive(true);
                if (!string.IsNullOrEmpty(currentGuideItem.hotWindow))
                {
                    if (w.name.Equals(currentGuideItem.hotWindow)) c.gameObject.SetActive(false);
                }
                Logger.LogInfo("in guide[id：{0}] window:{1} is Display,,guidePanel:{2} set visible {3}", currentGuide.ID, w.name, c.gameObject.name, c.gameObject.activeSelf);
            }
        }
    }

    private void OnWindowDisplay(Event_ e)
    {
        Window w = e.sender as Window;
        //Logger.LogInfo("{1} : display window = {0}", w.name, Time.realtimeSinceStartup);
        if (w is Window_DefaultLoading || w is Window_Gm) return;

        RefreshUnlockBtns(w);
        RefreshBtnUnlockState();

        if (w is Window_Gift && currentGuide && currentGuide.ID == GeneralConfigInfo.sguideGoodFeelingId) (w as Window_Gift).giftCavas.sortingOrder = 2;

        //主界面特殊处理，外部决定调用
        if (!(w is Window_Home) && !(w is Window_Chase)) _AddContidion(new OpenWindowCondition(w.name,!w.isFullScreen));
        else moduleLabyrinth.SendLabyrinthOpenTime();//每次主界面的时候都请求刷新一次迷宫时间

        if (currentGuideItem!= null && currentGuideItem.type == EnumGuideType.Dialog && !m_lastHideWindow)
        {
            m_lastHideWindow = w;
            if(currentGuide != null && currentGuide.ID == WISH_GUIDE_ID)
                m_lastHideWindow.Hide(true);
        }

        //if we are in guidding,refresh
        if (currentGuide && currentGuideItem != null)
        {
            GuideBase c = guideComponentDic.Get(currentGuideItem.type);
            if (c)
            {
                //refresh guideItem on window
                c.gameObject.SetActive(true);
                if (!string.IsNullOrEmpty(currentGuideItem.hotWindow))
                {
                    if (w.name.Equals(currentGuideItem.hotWindow)) c.RefreshGuideWindow(currentGuideItem);
                }
                Logger.LogInfo("in guide[id：{0}] window:{1} is Display,,guidePanel:{2} set visible {3}", currentGuide.ID, w.name, c.gameObject.name, c.gameObject.activeSelf);
            }
        }
    }

    private void OnSceneLoadComlete(Event_ e)
    {
        if (m_allGuideDic == null || m_allGuideDic.Count == 0) return;

        var level = e.sender as Level;
        if (level.isPvE)
        {
            if(level is Level_Test) _AddContidion(new EnterTrainCondition());
            else if(modulePVE.stageEventId > 0) _AddContidion(new EnterStageContition(modulePVE.stageEventId));
        }
    }

    #endregion

    #region window_home sub event

    private static readonly string[] m_homePanles = { WindowHomeMain, WindowHomeFight, WindowHomeDungeon, WindowHomeShop };

    public void AddWindowHomeEvent(int subWindowType)
    {
        if (subWindowType > -1 && subWindowType < m_homePanles.Length)
            _AddContidion(new OpenWindowCondition(m_homePanles[subWindowType]));
        else
            Logger.LogError($"Guide::AddWindowHomeEvent: Invalid sub window type <b>[{subWindowType}]</b>, ignored.");
    }

    public int GetSubwindowTypeForWindowHome(string name)
    {
        var idx = m_homePanles.FindIndex(p => p == name);
        return idx < 0 ? 0 : idx;
    }

    public bool IsOpenWindowHome(string name)
    {
        return name.Equals(WindowHomeMain) || name.Equals(WindowHomeFight) || name.Equals(WindowHomeDungeon) || name.Equals(WindowHomeShop);
    }

    #endregion

    #region window_chase sub event
    
    public void AddWindowChaseEvent(TaskType type)
    {
        var subwindow = GetChaseSubwindowName(type);

        if (!string.IsNullOrEmpty(subwindow))
        {
            _AddContidion(new OpenWindowCondition(subwindow));
        }
    }

    public string GetChaseSubwindowName(TaskType type)
    {
        string subwindow = CHASE_SUBWINDOW_DIC.Get(type);
        return subwindow;
    }

    public TaskType GetSubwindowTypeForWindowChase(string name)
    {
        foreach (var item in CHASE_SUBWINDOW_DIC)
        {
            if (name.Equals(item.Value)) return item.Key;
        }
        return TaskType.Count;
    }
    public GaidenSubWindowNpc GetSubWindowTypeForGaidenWindow(string name)
    {
        foreach (var item in GAIDEN_SUBWINDOW_DIC)
        {
            if (name.Equals(item.Value)) return item.Key;
        }
        return GaidenSubWindowNpc.Count;
    }

    public bool IsOpenWindowChase(string name)
    {
        var type = GetSubwindowTypeForWindowChase(name);
        return type >= TaskType.None && type < TaskType.Count;
    }

    public bool IsOpenWindowNpcGaiden(string name, string windowName = "window_npcGaiden")
    {
        var type = GetSubWindowTypeForGaidenWindow(name);
        bool bret = type >= GaidenSubWindowNpc.Panel && type < GaidenSubWindowNpc.Count;
        if (!string.IsNullOrEmpty(name)&&bret&&windowName.Equals("window_npcGaiden"))
        {
            _AddContidion(new OpenWindowCondition(name));
        }
        return bret;
    }

    #endregion

    #endregion

    #region UI Logic

    #region static fields

    public const string EventWillTriggerGuideStory      = "EventWillTriggerGuideStory";
    public const string EventTriggerDifferentGuideStory = "EventTriggerDifferentGuideStory";

    public const string EventUpdateGuideStep            = "EventUpdateGuideStep";
    public const string EventGuideOver                  = "EventGuideOver";
    public const string EventGuidePositionSuccess       = "EventGuidePositionSuccess";
    public const string EventGuideInputLock             = "EventGuideInputLock";
    public const string EventCheckGuideEnable           = "EventCheckGuideEnable";
    public const string GUIDE_PANEL_ASSET               = "guidepanel";

    /// <summary>
    /// 当新手引导初始化解锁的时候触发
    /// </summary>
    public const string EventInitUnlockFunction = "EventInitUnlockFunction";
    /// <summary>
    /// 当新手引导解锁功能动画 播放时触发
    /// </summary>
    public const string EventUnlockFunctionStart = "EventUnlockFunctionStart";
    /// <summary>
    /// 当新手引导解锁功能动画 播放完毕时触发
    /// </summary>
    public const string EventUnlockFunctionComplete = "EventUnlockFunctionComplete";

    //1:click;          2:front_slide;          3:up_slide;             4: down_slide; 
    //5:behind_slide;   6:left_double_click     7:right_double_click 
    //8:power           11:left_move            12:right_move
    public readonly static int[] CHECK_KEYS = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 11, 12 };
    public readonly static Dictionary<EnumGuideType, Type> GuidePanelDic = new Dictionary<EnumGuideType, Type>
    {
        {EnumGuideType.NormalGuide, typeof(ForceGuide) },
        {EnumGuideType.TipGuide,    typeof(DisparkGuide) },
    };
    public static bool inCheckConditionTime { get { return instance.m_scaleTime > 0 || !instance.afterPause; } }

    #endregion

    #region private field

    private int m_guideIndex;

    private float m_scaleTime = 0f;
    private float m_keyDuraction = 0f;

    private Creature m_player;
    private Vector2 m_targetPos;
    private bool m_loadingGuide;
    private float m_pauseStartTime;
    private float m_pauseCountDown;
    #endregion

    #region public field

    public Dictionary<EnumGuideType,GuideBase> guideComponentDic { get; private set; }
    public GuideInfo currentGuide { get; private set; }
    public GuideItem currentGuideItem { get; private set; }
    public int currentGuideIndex { get; private set; }
    public bool inGuideProcess { get { return currentGuide != null || m_loadingGuide||moduleStory.inStoryMode|| CheckWillGuide(); } }
    public bool isGuiding { get { return currentGuide != null || m_loadingGuide; } }
    public bool afterPause { get { return m_pauseCountDown == 0 || Time.realtimeSinceStartup - m_pauseStartTime >= m_pauseCountDown; } }
    #endregion

    #region static function

    public static void ShowGuide(GuideInfo info)
    {
        Logger.LogInfo("------load guide info id = {0}",info.ID);
        instance._ShowGuide(info);
        instance.UnlockLabyrinth();
    }

    #endregion

    #region open guide window
    private void _ShowGuide(GuideInfo info)
    {
        //Debug.LogError("start guide   "+info .ID + "  ------> " +Util.GetTimeStamp());
        if(guideStartTimeDic .ContainsKey (info.ID))
        {
            guideStartTimeDic[info.ID] = Util.GetTimeStamp();
        }
        else
        {
            guideStartTimeDic.Add(info.ID, Util.GetTimeStamp());
        }
        InitGuideData();
        
        currentGuide = info;
        if (!currentGuide) return;
        m_loadingGuide = true;
        //Logger.LogWarning("begin load guide asset");
        moduleGlobal.LockUI("", 0, 0, Module_Global.GUIDE_LOCK_PRIORITY);
        Level.PrepareAssets(GetPreLoadAssets(info),(flag)=> {
            m_loadingGuide = false;
            if (!flag)
            {
                Logger.LogWarning("--------load guide asset over,flag is false");
                moduleGlobal.UnLockUI(Module_Global.GUIDE_LOCK_PRIORITY);
                return;
            }

            InputManager.onInputChanged += OnInputChanged;
            enableUpdate = true;

            int result = info.GetAllGuideItem();
            if (IsNeedCreate(result, EnumGuideType.NormalGuide)) CreateGuide(EnumGuideType.NormalGuide);
            if (IsNeedCreate(result, EnumGuideType.TipGuide))    CreateGuide(EnumGuideType.TipGuide);
            InitGuideWindow();

            //Logger.LogWarning("---------load guide asset over");
            GuideItem item = info.GetGuideItem(m_guideIndex);
            //only other type unlock ui
            if(item.type != EnumGuideType.Dialog) moduleGlobal.UnLockUI(Module_Global.GUIDE_LOCK_PRIORITY);
            RefreshGuideItem(item);
        });
    }

    public List<string> GetPreLoadAssets(GuideInfo info)
    {
        List<string> list = new List<string>();
        list.Add(GUIDE_PANEL_ASSET);

        foreach (var item in info.guideItems)
        {
            if(item.hotAreaData.hasEffect) list.Add(item.hotAreaData.effect);
            if (item.playUnlockAudio) list.Add(item.unlockAudio);
            if (!string.IsNullOrEmpty(item.audio)) list.Add(item.audio);
            if (item.type == EnumGuideType.Dialog) list.AddRange(Module_Story.GetPerStoryPreAssets(item.dialogId,EnumStoryType.TheatreStory));
        }

        return list;
    }

    private bool IsNeedCreate(int result,EnumGuideType type)
    {
        if (type == EnumGuideType.Dialog) return false;
        int checkId = 1;
        if(type == EnumGuideType.TipGuide) checkId = 2;
        GuideBase c = guideComponentDic.Get(type);
        return !c && (result & checkId) > 0;
    }

    private void CreateGuide(EnumGuideType type)
    {
        if (!guideComponentDic.ContainsKey(type)) guideComponentDic.Add(type, null);
        GuideBase component = GetNewGuidePanel(type);
        guideComponentDic[type] = component;
    }
    
    private GuideBase GetNewGuidePanel(EnumGuideType type)
    {
        GameObject prefab = Level.GetPreloadObject<GameObject>(GUIDE_PANEL_ASSET, false);
        if (prefab == null) return null;

        Window_Global global = Window.GetOpenedWindow<Window_Global>();
        if (!global)
        {
            Logger.LogError("cannot finded Window_Global,please check out");
            return null;
        }

        Transform newItem = global.transform.AddNewChild(prefab);
        newItem.Strech();
        newItem.name = type == EnumGuideType.NormalGuide ? "force_guide_panel" : "dispark_guide_panel";
        return newItem.gameObject.GetComponentDefault(GuidePanelDic[type]) as GuideBase;
    }
    #endregion

    #region refresh
    private void InitGuideData()
    {
        currentGuide = null;
        currentGuideItem = null;
        m_guideIndex = 0;
        enableUpdate = false;
        m_player = null;
        if (guideComponentDic == null) guideComponentDic = new Dictionary<EnumGuideType, GuideBase>();

        ResetCondition();
    }

    private void InitGuideWindow()
    {
        foreach (var item in guideComponentDic)
        {
            if (item.Value) item.Value.InitGuideWindow();
        }
    }

    private void RefreshGuideWindow()
    {
        foreach (var item in guideComponentDic) item.Value.ResetGuideData();
        foreach (var item in guideComponentDic) item.Value.RefreshGuideWindow(currentGuideItem);
    }

    private void RefreshGuideDialog()
    {
        if(!skipStory)
        {
            Module_Story.ShowStory(currentGuideItem.dialogId, EnumStoryType.TheatreStory);
            RefreshGuideWindow();
        }
        else
        {
            moduleGlobal.UnLockUI(Module_Global.GUIDE_LOCK_PRIORITY);
            StoryInfo info = ConfigManager.Get<StoryInfo>(currentGuideItem.dialogId);
            if (info)
            {
                foreach (var item in info.storyItems)
                {
                    if (item.giveDatas != null && item.giveDatas.Length > 0)
                    {
                        moduleStory.RequestProp(info.ID, item.giveDatas);
                    }
                }
            }
            _AddContidion(new StoryEndContition(currentGuideItem.dialogId));
            UpdateStep();
        }
    }

    public void RefreshGuideInterlude()
    {
        RefreshGuideWindow();
        Window_Interlude.OpenInterlude(currentGuideItem,UpdateStep);
    }

    public void UpdateStep()
    {
        //if is loading guide. do not update step.
        if (m_loadingGuide)
        {
            RefreshGuideWindow();
            return;
        }
        if (currentGuide == null) return;

        ResetCondition();
        m_guideIndex++;

        Logger.LogInfo("Module_guide：{0} UpdateStep.... current step is {1}", currentGuide.ID, m_guideIndex);
        RefreshGuideItem(currentGuide.GetGuideItem(m_guideIndex));
    }

    private void OnGuideDialogEnd()
    {
        if (currentGuideItem == null || currentGuide == null) return;

        //设置锻造的武器常量
        if (m_lastHideWindow is Window_Forging && currentGuide.ID == FORGING_GUIDE_ID) moduleForging.defalutEnhanceId = DEFAULT_WEAPON_ID_IN_FORGING;

        if (m_lastHideWindow)
        {
            if(currentGuide != null && currentGuide.ID == WISH_GUIDE_ID)
                m_lastHideWindow.Show(true);
            m_lastHideWindow = null;
        }
        if (currentGuideItem.type != EnumGuideType.NormalGuide)
            UpdateStep();
        else
            Logger.LogDetail("当前引导为{0}只有点击其热区才能更新下一步", currentGuideItem.type);
    }

    private void RefreshGuideItem(GuideItem item)
    {
        currentGuideItem = item;

        if (currentGuideItem != null && currentGuideItem.type == EnumGuideType.Dialog) RefreshGuideDialog();
        else if (currentGuideItem != null && currentGuideItem.type == EnumGuideType.Interlude) RefreshGuideInterlude();
        else RefreshGuideWindow();

        if (currentGuideItem != null) HandleGuideInfoSuceess();
        else HandleGuideInfoEnd();
    }

    private void HandleGuideInfoSuceess()
    {
        UpdateCondition(currentGuideItem);
        DispatchModuleEvent(EventUpdateGuideStep, currentGuideItem);
        if (currentGuideItem.isEnd) AddFinishGuide(currentGuide.ID);
    }

    public void HandleGuideInfoEnd()
    {
        int endGuideId = currentGuide ? currentGuide.ID : 0;
        InputManager.onInputChanged -= OnInputChanged;
        DispatchModuleEvent(EventGuideOver);
        InitGuideData();
        Time.timeScale = 1f;
        if (endGuideId != 0)
        {
            //添加引导完成条件
            BaseGuideCondition c = m_cacheConditions.Find(o=>o.type == EnumGuideContitionType.GuideEnd && o.GetIntParames(0) == endGuideId);
            if(c == null) _AddContidion(new GuideEndContition(endGuideId));
            else _CheckTriggerGuide();

            if (DEFALUT_OPERATE_GUIDES.Contains(endGuideId)) _AddContidion(new DefalutOperateGuideEndCondition());
            AddFinishGuide(endGuideId);
            AddPveCondition(endGuideId);
        }
    }

    public void HandleGuideStoryGiveData()
    {
        if (!currentGuide || currentGuide.guideItems == null) return;

        for (int i = currentGuideIndex,len = currentGuide.guideItems.Length; i < len; i++)
        {
            GuideItem item = currentGuide.guideItems[i];
            if (item != null && item.type == EnumGuideType.Dialog)
            {
                StoryInfo info = ConfigManager.Get<StoryInfo>(item.dialogId);
                if (info)
                {
                    foreach (var story in info.storyItems)
                    {
                        if (story.giveDatas != null && story.giveDatas.Length > 0)
                        {
                            moduleStory.RequestProp(info.ID, story.giveDatas);
                        }
                    }
                }
                _AddContidion(new StoryEndContition(currentGuideItem.dialogId));
            }
            if(item!=null&&item.type==EnumGuideType.NormalGuide)
            {
                if(!string.IsNullOrEmpty(item.hotWindow))
                {
                    _AddContidion(new OpenWindowCondition(item.hotWindow));
                }
            }
        }
    }

    private void AddPveCondition(int guideId)
    {
        if (!Level.current.isPvE) return;

        //Logger.LogInfo("GuideEnd contition ,GuideId = {0}", guideId);
        modulePVEEvent.AddCondition(new SGuideEndConditon(guideId));
    }

    private void AddFinishGuide(int guideId)
    {
        if (m_overGuideIds.Contains(guideId))
        {
            DebugAllCondition(Util.Format("[引导 : {0}已完成,即完成条件已加，不进行重复添加完成条件]", guideId.ToString()));
            return;
        }
        m_overGuideIds.Add(guideId);
        SendFinishGuide(guideId);
        BaseGuideCondition c = m_cacheConditions.Find(o => o.type == EnumGuideContitionType.GuideEnd && o.GetIntParames(0) == guideId);
        if (c == null) _AddContidion(new GuideEndContition(guideId));
    }

    private bool IsGuideEnd(int guideId)
    {
        return m_cacheConditions.Find(o => o.type == EnumGuideContitionType.GuideEnd && o.GetIntParames(0) == guideId) != null ? true : false;
    }

    private void ResetCondition()
    {
        m_keyDuraction = 0;
        InputManager.lockedKeyValue = 0;
        m_scaleTime = 0;
        m_pauseStartTime = 0;
        m_pauseCountDown = 0;
    }

    private void UpdateCondition(GuideItem item)
    {
        UpdatePositionCondition(item);
        UpdateInputCondition(item);
        UpdatePauseCountDown(item);
        ResetTimeScale(item);
    }
     
    private void UpdateInputCondition(GuideItem item)
    {
        if(item.hasCondition && item.successCondition.intParams.Length > 1 && item.successCondition.type == EnumGuideCondition.InputKey)
        {
            m_keyDuraction = item.successCondition.intParams[1] * 0.001f;
            InputManager.lockedKeyValue = item.successCondition.intParams[0];
        }
    }

    private void OnInputChanged(InputKey[] changedKeys, int count)
    {
        //if we are locking scaletime,then return
        if (InputManager.lockedKeyValue <= 0 || m_scaleTime > 0 || !afterPause) return;

        bool needPause = false;
        foreach (var item in changedKeys)
        {
            if (item != null && Array.Exists(CHECK_KEYS,o=>o == item.value))
            {
                needPause = true;
                break;
            }
        }
        if (!needPause) return;

        DispatchModuleEvent(EventGuideInputLock,true);
        if (Time.timeScale == 0) Time.timeScale = 1;
        m_scaleTime = Time.realtimeSinceStartup;

        //一旦成功达成，就先关闭guide item 的显示
        if(currentGuideItem != null && currentGuideItem.hasCondition && currentGuideItem.successCondition.type == EnumGuideCondition.InputKey)
        {
            foreach (var item in guideComponentDic) item.Value.ResetGuideData();
        }
        //Logger.LogWarning("start check guide input condition......");
    }

    private void CheckInputKey()
    {
        if (m_scaleTime > 0 && m_keyDuraction > 0 && Time.realtimeSinceStartup - m_scaleTime >= m_keyDuraction)
        {
            //Logger.LogWarning("input key duraction come on,time scale is set as 0.................");
            InputManager.instance.ResetInputKeys();
            m_scaleTime = 0f;
            Time.timeScale = 0;
            UpdateStep();
            DispatchModuleEvent(EventGuideInputLock, false);
        }
    }

    private void UpdatePositionCondition(GuideItem item)
    {
        if (m_player == null) m_player = ObjectManager.FindObject<Creature>(o => o.isPlayer);
        m_targetPos = Vector2.zero;

        if(item.hasCondition && item.successCondition.floatParams == null)
        {
            Logger.LogError("guide id = {0} ,index = {1} has a wrong position condition",currentGuide?.ID,m_guideIndex);
        }

        if (item.hasCondition && item.successCondition.floatParams != null && item.successCondition.floatParams.Length > 1 && item.successCondition.type == EnumGuideCondition.Position)
        {//这里默认右击移动所以对于inputkeyinfos表中的ID为3
            m_targetPos.Set(item.successCondition.floatParams[0], item.successCondition.floatParams[1]);
            InputManager.lockedKeyValue = 31;
        }
    }

    private void CheckPostion()
    {
        if(m_player && m_targetPos != Vector2.zero)
        {
            if(m_player.position.x >= m_targetPos.x && m_player.position.x <= m_targetPos.y)
            {
                m_targetPos = Vector2.zero;
                DispatchModuleEvent(EventGuidePositionSuccess);
                UpdateStep();
            }
        }
    }

    private void UpdatePauseCountDown(GuideItem item)
    {
        m_pauseCountDown = item.pauseCountDown * 0.001f;
        m_pauseStartTime = m_pauseCountDown > 0 ? Time.realtimeSinceStartup : 0;
    }

    private void ResetTimeScale(GuideItem item)
    {
        if (Level.current && Level.current is Level_Test) return;
        if (item.hasCondition && item.hasCondition && item.successCondition.type == EnumGuideCondition.InputKey && item.pauseCountDown <= 0) return;

        Time.timeScale = 1f;
    }

    private void CheckPauseCountDown()
    {
        if (m_pauseCountDown > 0 && m_pauseStartTime > 0 && afterPause)
        {
            Time.timeScale = 0;
            //only set one frame
            m_pauseCountDown = 0f;
        }
    }

    public override void OnUpdate(int diff)
    {
        base.OnUpdate(diff);
        CheckInputKey();
        CheckPostion();
        CheckPauseCountDown();
    }
    #endregion

    #region event

    public void DispatchCheckGuideEnable(BaseRestrain c)
    {
        Event_ e = Event_.Pop();
        e.param1 = c;
        DispatchEvent(EventCheckGuideEnable,e);
    }

    #endregion

    public bool HasFinishGuide(int guideId)
    {
        return m_overGuideIds != null && m_overGuideIds.Contains(guideId);
    }

    #endregion

    #region unlock logic
    /// <summary>
    /// 关卡内觉醒变身功能是否解锁
    /// </summary>
    public bool awakeUnlocked => IsActiveFunction(111);

    /// <summary>
    /// 所有的解锁功能
    /// </summary>
    public List<int> hasUnlockFuncs { get; private set; } = new List<int>();
    private Dictionary<int[], int> COMBINE_FUNCTIONS_DIC = new Dictionary<int[], int>()
    {
        { new int[] {(int)HomeIcons.Forge,      (int)HomeIcons.FashionShop, (int)HomeIcons.DrifterShop, (int)HomeIcons.WishingWell},    (int)HomeIcons.Shop },
        { new int[] {(int)HomeIcons.Labyrinth,  (int)HomeIcons.Bordlands},                                                              (int)HomeIcons.Dungeon },
        { new int[] {(int)HomeIcons.Train,      (int)HomeIcons.PVP,         (int)HomeIcons.Match},                                      (int)HomeIcons.Fight },
    };
    private Dictionary<int, UnlockFunctionBtn> m_unlockBtns = new Dictionary<int, UnlockFunctionBtn>();
    private List<UnlockFunctionInfo> m_infos;

    /// <summary>
    /// 每次界面开始时进行按钮刷新
    /// </summary>
    /// <param name="openWindow"></param>
    public void RefreshUnlockBtns(Window openWindow)
    {
        if (!openWindow) return;

        if (m_infos == null) m_infos = ConfigManager.GetAll<UnlockFunctionInfo>();
        var unlockBtns = m_infos.FindAll(o => o.windowName.Equals(openWindow.name));
        if (unlockBtns == null || unlockBtns.Count == 0) return;

        foreach (var item in unlockBtns)
        {
            if(!ValidUnlockBtn(item.ID) && !string.IsNullOrEmpty(item.buttonName))
            {
                Transform t = openWindow.transform;
                if(!string.IsNullOrEmpty(item.prefixName)) t = Util.FindChild(t, item.prefixName);
                if (!t) continue;
                t = Util.FindChild(t, item.buttonName);
                if (!t) continue;

                if (!m_unlockBtns.ContainsKey(item.ID)) m_unlockBtns.Add(item.ID, null);
                m_unlockBtns[item.ID] = new UnlockFunctionBtn(t,item.unlockTweenType, item.ID);
                //Logger.LogInfo("Id ; {0} finded a button with path {1}",item.ID, m_unlockBtns[item.ID].transform.GetPath());
            }
        }
    }

    public void RefreshBtnUnlockState()
    {
        foreach (var item in m_unlockBtns)
        {
            if(item.Value != null && item.Value.gameObject!= null) item.Value.InitLockState(IsActiveFunction(item.Key));
        }
    }

    private bool ValidUnlockBtn(int btnId)
    {
        return m_unlockBtns.ContainsKey(btnId) && m_unlockBtns[btnId] != null && m_unlockBtns[btnId].gameObject != null;
    }

    public void InitUnlockFunctions(List<int> list)
    {
        hasUnlockFuncs.Clear();
        ActiveFunctions(list);
        hasUnlockFuncs.Sort();

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Logger.LogInfo("Unlocked functions: [{0}]", hasUnlockFuncs.PrettyPrint());
        #endif

        DispatchModuleEvent(EventInitUnlockFunction);
    }

    public void ActiveFunctions(List<int> list)
    {
        ActiveFunctions(list.ToArray());
    }

    public void ActiveFunctions(int[] list)
    {
        foreach (var func in list)
        {
            ActiveFunction(func);
        }
    }

    public void ActiveFunction(int function)
    {
        if(!hasUnlockFuncs.Contains(function)) hasUnlockFuncs.Add(function);

        int combineId = GetCombineFunctionIndex(function);
        if (!hasUnlockFuncs.Contains(combineId)) hasUnlockFuncs.Add(combineId);
    }

    private int GetCombineFunctionIndex(int function)
    {
        foreach (var item in COMBINE_FUNCTIONS_DIC)
        {
            if (item.Key.Contains(function)) return item.Value;
        }
        return 0;
    }

    public bool IsActiveFunction(HomeIcons type)
    {
        return IsActiveFunction((int)type);
    }

    public bool IsActiveFunction(int funcId)
    {
        if (m_infos == null) m_infos = ConfigManager.GetAll<UnlockFunctionInfo>();
        var unlockInfo = m_infos.Find(o=>o.ID == funcId);
        if (!unlockInfo) return true;

        return hasUnlockFuncs != null && hasUnlockFuncs.Contains(funcId);
    }

    public void PlayUnlockNewFuncAnimation(int[] unlockFuncs)
    {
        DispatchModuleEvent(EventUnlockFunctionStart, unlockFuncs);
        if ((unlockFuncs == null || unlockFuncs.Length == 0))
        {
            UnlockFunctionComplete();
            return;
        }

        bool unlockOperation = false;
        int unlockId = 0;
        for (int i = 0; i < unlockFuncs.Length; i++)
        {
            unlockId = unlockFuncs[i];
            if (!m_unlockBtns.ContainsKey(unlockId)) continue;

            m_unlockBtns[unlockId].Unlock(i == 0);
            unlockOperation = true;
        }

        //如果配置的ID没法进行解锁
        if(!unlockOperation)
        {
            Logger.LogWarning("unlock function id array = {0} connot finded,method :1. check config [guideinfo.xml] and [unlockinfo.xml] 2.window havn't loaded",unlockFuncs.ToXml());
            UnlockFunctionComplete();
        }
    }

    public void UnlockFunctionComplete()
    {
        DispatchEvent(EventUnlockFunctionComplete);

    }

    private void ResetAllUnlockBtn()
    {
        foreach (var item in m_unlockBtns)
        {
            if (item.Value != null && item.Value.gameObject != null) item.Value.ResetUnlockBtn();
        }
    }
    /// <summary>
    /// 通过ID获取界面提示文本ID
    /// </summary>
    /// <param name="funcId"></param>
    /// <returns></returns>
    public int GetUnlockFunctionNoteID(int funcId)
    {
        if (m_infos == null) m_infos = ConfigManager.GetAll<UnlockFunctionInfo>();
        var unlockInfo = m_infos.Find(o => o.ID == funcId);
        if(unlockInfo != null)
        return unlockInfo.unlockNote;
        return 0;
    }
    public int GetUnlockFunctionNoteTipID(int funcId)
    {
        if (m_infos == null) m_infos = ConfigManager.GetAll<UnlockFunctionInfo>();
        var unlockInfo = m_infos.Find(o => o.ID == funcId);
        if(unlockInfo!=null)
        return unlockInfo.unlockNoteTip;
        return 0;
    }
    #endregion
}

#region 新手引导解锁
public enum EnumUnlockTweenType
{
    None,

    //主界面大功能图标以及二级界面需要的解锁流程
    //流程：init(button interactable false,lock gameobject visible)->tweenalpha and particle ->effect end(button interactable true)
    LockVisible,

    //左上角和右下角的解锁流程
    //流程:init(button invisible,button interactable false -> tweenalpha and particle ->effect end(button interacable true))
    ButtonInvisible,

    //追捕困难按钮特殊流程
    //流程：未解锁：按钮显示可交互但饱和度为0 （点击事件的处理交由UI逻辑处理），解锁完成后，按钮可交互并正常点击
    ButtonSaturation,
}


public sealed class UnlockFunctionBtn : CustomSecondPanel
{
    public UnlockFunctionBtn(Transform trans, EnumUnlockTweenType type,int functionID) : base(trans)
    {
        InitUnlockType(type);
        funcId = functionID;
        InitBtnText();//需要用到funcId 如果写在InitComponent时，获取的funcId是不正确的
    }

    private Module_Guide moduleGuide;

    public Button button;
    public Button tipesbutton;
    public Text uitipes;
    public EnumUnlockTweenType unlockType;
    public float unlockDuraction;
    public GameObject lockObj;
    public TweenAlpha unlockTween;
    private int m_delayEventId;
    private DelayEventHandler handle;
    private bool m_dispatchEvent;
    public bool unlock { get; private set; }
    public int funcId { get; private set; }

    public override void InitComponent()
    {
        base.InitComponent();
        moduleGuide = Module_Guide.instance;
        button = transform.GetComponent<Button>();
        tipesbutton = transform.GetComponent<Button>("lock/lockBtn");
        uitipes = transform.GetComponent<Text>("lock/unlockNote");
        tipesbutton?.onClick.AddListener(LockTipes);
        Transform t = transform.Find("lock");
        unlockTween = t?.GetComponent<TweenAlpha>();
        lockObj = t?.gameObject;
       
    }

    private void InitUnlockType(EnumUnlockTweenType type)
    {
        unlockType = type;
        unlockDuraction = 0f;
        switch (unlockType)
        {
            case EnumUnlockTweenType.LockVisible:
                unlockDuraction = GeneralConfigInfo.homeUnlockTime1;
                break;
            case EnumUnlockTweenType.ButtonInvisible:
                unlockDuraction = GeneralConfigInfo.homeUnlockTime2;
                break;
            case EnumUnlockTweenType.ButtonSaturation:
                unlockDuraction = GeneralConfigInfo.homeUnlockTime3;
                break;
        }
    }

    #region lock / unlock
    public void InitLockState(bool unlock = true)
    {
        this.unlock = unlock;
        switch (unlockType)
        {
            case EnumUnlockTweenType.LockVisible:       InitLock1(unlock);  break;
            case EnumUnlockTweenType.ButtonInvisible:   InitLock2(unlock);  break;
            case EnumUnlockTweenType.ButtonSaturation:  InitLock3(unlock);  break;
            default:                                    InitLock2(unlock);  break;
        }
        //Logger.LogInfo("button {0} set visible {1} ,type is {2}", transform.GetPath(), unlock,unlockType);
    }

    private void InitLock1(bool unlock)
    {
        lockObj?.SetActive(!unlock);
        button?.SetInteractable(unlock);
    }

    private void InitLock2(bool unlock)
    {
        SetPanelVisible(unlock);
        button?.SetInteractable(unlock);
    }

    private void InitLock3(bool unlock)
    {
        SetPanelVisible(true);
        button?.gameObject?.SetSaturation(unlock ? 1 : 0);
    }

    public void Unlock(bool dispatch = false)
    {
        m_dispatchEvent = dispatch;
        float endTime = !unlockTween ? 0 : unlock ? 0 : unlockDuraction;

        if (endTime == 0) UnlockComplete();
        else
        {
            unlockTween?.PlayForward();
            DelayEvents.Remove(m_delayEventId);
            m_delayEventId = DelayEvents.Add(UnlockComplete, endTime);
        }
    }

    private void UnlockComplete()
    {
        unlock = true;
        lockObj?.SetActive(false);
        button?.SetInteractable(true);
        SetPanelVisible(true);

        moduleGuide.ActiveFunction(funcId);
        if (unlockType == EnumUnlockTweenType.ButtonSaturation) button?.gameObject.SetSaturation(1.0f);
        if (m_dispatchEvent) moduleGuide?.UnlockFunctionComplete();
    }

    public void ResetUnlockBtn()
    {
        DelayEvents.Remove(m_delayEventId);
        unlockTween?.Kill();
    }
    private void LockTipes()
    {
        if(!unlock)
        {
            int tipes = moduleGuide.GetUnlockFunctionNoteTipID(funcId);
            Module_Global.instance.ShowMessage(tipes);
        }
        
    }
    private void InitBtnText()
    {
        if(uitipes && !unlock)
        {
            int tipes = moduleGuide.GetUnlockFunctionNoteID(funcId);
            uitipes.text = tipes>0?ConfigText.GetDefalutString(tipes):string.Empty;
        }
    }
    #endregion
}

#endregion