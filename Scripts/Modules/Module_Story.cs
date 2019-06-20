/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-12-19
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using DatingFoodOrder = PShopItem; 
using DivinationData = PDatingDivinationResultData;
using UnityEngine.PostProcessing;
public class Module_Story : Module<Module_Story>
{
    public const int DEFAULT_STORY_ID = 1001;
    public const int DEFAULT_STAGE_ID = 1;

    //the battle dialog item padding to screen
    private static Vector2 m_padding = new Vector2(10,40);

    public const string EventStoryStart         = "EventStoryStart";
    public const string EventStoryEnd           = "EventStoryEnd";
    public const string EventStoryMaskEnd       = "EventStoryMaskEnd";
    public const string EventCombatCameraMove   = "EventCombatCameraMove";
    public const string EventStoryClosed = "EventStoryClosed";

    public Dictionary<Creature, BaseBattleItem> battleStoryDic { get { return m_battleStoryDic; } }
    private Dictionary<Creature, BaseBattleItem> m_battleStoryDic = new Dictionary<Creature, BaseBattleItem>();
    private Camera m_monsterCamera;
    private Camera m_uiCamera;
    private Rect m_screenRect;
    private float m_scalerFactor;

    //current cached battlestory
    private Dictionary<EnumStoryType, List<BaseStory>> m_loadedStory = new Dictionary<EnumStoryType, List<BaseStory>>();
    private Dictionary<EnumStoryType, Type> m_storyTypeDic = new Dictionary<EnumStoryType, Type>();
    //the top bar of globle_panel
    private int m_globalBarState = -1;
    public bool combatCameraMoving { get; private set; }
    public Transform npcCameraTrans { get; private set; }
    /// <summary>
    /// 上一帧的战斗镜头的shot功能
    /// </summary>
    private bool m_lastCameraCombatShotState = false;

    /// <summary>对话结束后的回调</summary>
    private Action<int> m_storyEndCallBack = null;

    #region properties

    private bool m_debugStory = false;
    public bool debugStory
    {
        get { return m_debugStory; }
        set
        {
#if UNITY_EDITOR
            m_debugStory = value;
#else
            m_debugStory = false;
#endif
        }
    }

    public bool workOffline { get; set; }

    /// <summary>
    /// 当前是否处于剧情模式
    /// </summary>
    public bool inStoryMode { get { return m_inStoryMode; } }
    private bool m_inStoryMode = false;
    /// <summary>
    /// 是否正在加载剧情资源
    /// </summary>
    public bool loading { get { return m_loading; } }
    private bool m_loading = false;

    /// <summary> 当前对话的索引 </summary>
    public int currentStoryIndex { get; set; }

    /// <summary> 当前对话数据 </summary>
    public StoryInfo currentStory { get; set; }

    /// <summary> 当前对话的类型 </summary>
    public EnumStoryType currentStoryType { get; set; }

    /// <summary> 当前对话内容 </summary>
    public StoryInfo.StoryItem currentStoryItem
    {
        get
        {
            if (currentStory == null || currentStory.storyItems == null)
                return null;
            if (currentStory.storyItems.Length <= currentStoryIndex || currentStoryIndex < 0)
                return null;

            return currentStory.storyItems.GetValue<StoryInfo.StoryItem>(currentStoryIndex);
        }
    }

    public static string genderDialogContent
    {
        get
        {
            int m_gender = modulePlayer.roleInfo.gender;
            m_gender = m_gender < 0 || m_gender > 1 ? 0 : m_gender;

            return ConfigText.GetDefalutString(GeneralConfigInfo.defaultConfig.storyData.genderTextId, m_gender);
        }
    }

    public bool hasNotRenderItem
    {
        get
        {
            if (currentStory == null || currentStory.storyItems == null) return false;

            return currentStory.storyItems.Length - 1 > currentStoryIndex;
        }
    }

    #region 缓存数据

    public DatingFoodOrder foodOrder;
    public DivinationData divinationData;

    #endregion

    #endregion
    #region 剧情默认后处理效果
    public PostProcessingProfile postProcessingDefalutProfile { set; get; }
    #endregion

    #region get content/replaceName functions
    
    public string GetReplaceName()
    {
        return GetReplaceName(currentStoryItem);
    }

    public static string GetReplaceName(StoryInfo.StoryItem item)
    {
        if (item == null) return string.Empty;

        string str = item.replaceName.Replace(StoryConst.PLAYER_NAME_PARAM, modulePlayer.name_);
        return str;
    }
    
    public string GetReplaceContent()
    {
        return GetReplaceContent(currentStoryItem, foodOrder, divinationData);
    }

    public static string GetReplaceContent(StoryInfo.StoryItem item, DatingFoodOrder order, DivinationData divination)
    {
        if (item == null) return string.Empty;

        string str = item.content.Replace(StoryConst.PLAYER_NAME_PARAM, modulePlayer.name_);
        str = str.Replace(StoryConst.PLAYER_GENDER_PARAM, genderDialogContent);

        if(divination != null)
        {
            var type = (EnumDivinationType)divination.type;
            str = str.Replace(StoryConst.DEVINE_TYPE_PARAM, GetDivinationName(type));

            if (type == EnumDivinationType.CrystalDevine) str = str.Replace(StoryConst.CRYSTAL_DEVINE_RESULT_PARAM, Util.GetString(divination.contentId));
            else if (type == EnumDivinationType.LotDevine) str = str.Replace(StoryConst.LOT_DEVINE_RESULT_PARAM, Util.GetString(divination.contentId));
            else
            {
                str = str.Replace(StoryConst.CRYSTAL_DEVINE_RESULT_PARAM, string.Empty);
                str = str.Replace(StoryConst.LOT_DEVINE_RESULT_PARAM, string.Empty);
            }

            str = str.Replace(StoryConst.DEVINE_INT_PARAM, divination.addMood.ToString());
            str = str.Replace(StoryConst.DEVINE_PERCENT_PARAM, divination.addMood.ToString("P"));
        }
        else
        {
            str = str.Replace(StoryConst.DEVINE_TYPE_PARAM, string.Empty);
            str = str.Replace(StoryConst.CRYSTAL_DEVINE_RESULT_PARAM, string.Empty);
            str = str.Replace(StoryConst.LOT_DEVINE_RESULT_PARAM, string.Empty);
            str = str.Replace(StoryConst.DEVINE_INT_PARAM, "0");
            str = str.Replace(StoryConst.DEVINE_PERCENT_PARAM, "0%");
        }

        var p = order == null ? string.Empty : order.itemName;
        str = str.Replace(StoryConst.SHOPITEM_NAME_PARAM, p);
        p = order == null ? string.Empty : order.currencyNum.ToString();
        str = str.Replace(StoryConst.SHOPITEM_PRICE_PARAM, p);

        return str;
    }

    public static string GetDivinationName(EnumDivinationType type)
    {
        return GetDivinationName((int)type);
    }

    public static string GetDivinationName(int type)
    {
        return ConfigText.GetDefalutString(294, type);
    }

    public List<DialogReviewData> GetDialogReviewDatas(int storyId, DatingFoodOrder order = null, DivinationData divination = null)
    {
        var info = ConfigManager.Get<StoryInfo>(storyId);
        if (info == null) Logger.LogError("story data is null, and storyId={0} in storyinfo", storyId);
        if (!info || info.storyItems == null || info.storyItems.Length == 0) return null;

        var isCurrentDialog = currentStory && storyId == currentStory.ID;

        var list = new List<DialogReviewData>();
        for (int i = 0; i < info.storyItems.Length; i++)
        {
            var item = info.storyItems[i];
            //必须检测回顾的是否是当前对话，由策划保证一个约会事件下的所有对化ID不重复
            if (string.IsNullOrEmpty(item.content) || (isCurrentDialog && i > currentStoryIndex)) continue;

            var data = DialogReviewData.Create();
            data.RefreshDialogReviewData(item, order, divination);
            list.Add(data);
        }
        return list;
    }

    public List<DialogReviewData> GetDialogReviewDatas(List<ReviewSourceData> reviews)
    {
        var list = new List<DialogReviewData>();
        foreach (var item in reviews)
        {
            if (item == null) continue;

            list.AddRange(GetDialogReviewDatas(item.storyId, item.orderData, item.diviData));
        }
        return list;
    }

    #endregion

    #region create and destroy story item

    /// <summary>
    /// 显示对话的接口
    /// </summary>
    /// <param name="plotID">对话ID</param>
    /// <param name="type">对话类型，1是剧场对话，2是战斗剧情</param>
    public static void ShowStory(int plotID,int type,bool unlock = true, Action<int> callback = null)
    {
        instance._ShowStory(plotID,(EnumStoryType)type, unlock, callback);
    }

    public static void ShowStory(int plotID, EnumStoryType type, bool unlock = true, Action<int> callback = null)
    {
        instance._ShowStory(plotID, type, unlock, callback);
    }
    
    private void _ShowStory(int plotID, EnumStoryType type, bool unlock = true, Action<int> callback = null)
    {
        m_storyEndCallBack = callback;

        if (unlock) moduleGlobal.LockUI("", 0, 0, Module_Global.GUIDE_LOCK_PRIORITY);

        m_loading = true;
        Level.PrepareAssets(GetPerStoryPreAssets(plotID, type), (flag) =>
        {
            m_loading = false;
            if (!flag || !moduleLogin.loggedIn && !debugStory && !workOffline)  // Lost connection before loading complete
            {
                moduleGlobal.UnLockUI(Module_Global.GUIDE_LOCK_PRIORITY);
                Logger.LogError(!flag ? "prepare story assets failed..." : "Module_Story::_ShowStory: Lost connection to server before assets loading complete.");
                return;
            }

            ShowStoryOnLoadOver(plotID, type);
            moduleGlobal.UnLockUI(Module_Global.GUIDE_LOCK_PRIORITY);
        });
    }
    
    private void ShowStoryOnLoadOver(int plotID, EnumStoryType type)
    {
        StoryInfo info = ConfigManager.Get<StoryInfo>(plotID);
        if (!info)
        {
            Logger.LogError("story id = {0} connot be loaded please check story_config....",plotID);
            return;
        }
        //如果当前已经有了globalBarState,则不在缓存
        if (m_globalBarState < 0)
        {
            m_globalBarState = moduleGlobal.GetGlobalLayerShowState();
            moduleGlobal.ShowGlobalLayerDefault(2, false);
        }
        
        BaseStory story = LoadStoryItem(type);
        if (!story) Logger.LogError("plotID = {0},type = {1} asset_name = {2} cannot be loaded", plotID, type, StoryConst.THEATRE_STORY_ASSET_NAME);
        else
        {
            int layer = type == EnumStoryType.NpcTheatreStory || type == EnumStoryType.TheatreStory ? Layers.Dialog : Layers.UI;
            Util.SetLayer(story.gameObject, layer);
            moduleNPCDating.AddEventListener(Module_NPCDating.EventAllStoryEnd, OnAllStoryEnd);
            currentStoryType = type;
            currentStoryIndex = 0;
            currentStory = info;
            story.ShowDialog(plotID, type);
            Logger.LogInfo($"开始剧情对话 {plotID}");
        }
    }

    private BaseStory LoadStoryItem(EnumStoryType type)
    {
        if (!m_storyTypeDic.ContainsKey(type)) return null;

        BaseStory s = GetFromLoaded(type);
        if (s) return s;

        s = GetFromNewItem(type);
        if (!s) return null;

        if (!m_loadedStory.ContainsKey(type)) m_loadedStory.Add(type, new List<BaseStory>());
        m_loadedStory[type].Add(s);
        s.name = Util.Format("{0}_{1}", type, m_loadedStory[type].Count.ToString("D2")); 
        return s;
    }

    private BaseStory GetFromLoaded(EnumStoryType type)
    {
        if (!m_loadedStory.ContainsKey(type) || m_loadedStory[type] == null || m_loadedStory[type].Count == 0) return null;
        BaseStory story = m_loadedStory[type].GetValue(0);
        if (story != null && story.gameObject.activeInHierarchy) Logger.LogWarning("a new story use the avtive BaseStory GameObject of type :[{0}]",type);
        return story;
    }

    private BaseStory GetFromNewItem(EnumStoryType type)
    {
        string assetName = type == EnumStoryType.TheatreStory || type == EnumStoryType.NpcTheatreStory ? StoryConst.THEATRE_STORY_ASSET_NAME : StoryConst.BATTLE_STORY_ASSET_NAME;
        GameObject prefab = Level.GetPreloadObject<GameObject>(assetName, false);
        if (prefab == null) return null;

        Transform newItem = Root.instance.ui.transform.AddNewChild(prefab);
        newItem.Strech();
        newItem.SafeSetActive(true);


        return newItem.gameObject.GetComponentDefault(m_storyTypeDic[type]) as BaseStory;
    }

    public static void DestoryStory()
    {
        instance.SetCameraTransToGlobal();
        instance.ClearLoadedStory();
        instance.ClearBattleCache();
        UIManager.SetCameraLayer(Layers.UI);
    }

    public BaseStory GetCurrentValidStory()
    {
        foreach (var item in m_loadedStory)
        {
            if (item.Value == null || item.Value.Count == 0) continue;

            foreach (var s in item.Value)
            {
                if (s.gameObject && s.gameObject.activeInHierarchy) return s;

            }
        }

        return null;
    }

#endregion

    #region get preload asset

    /// <summary>
    /// 获取场景事件的所有的剧情相关的预加载资源
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public List<string> GetPreLoadAsset(int stageEventId)
    {
        List<string> list = new List<string>();

        SceneEventInfo info = ConfigManager.Get<SceneEventInfo>(stageEventId);
        if (!info) return list;

        return GetPreLoadAsset(info);
    }

    public static List<string> GetPreLoadAsset(SceneEventInfo info)
    {
        List<string> list = new List<string>();
        if (!info) return list;

        SceneEventInfo.SceneEvent e = null;
        for (int i = 0; i < info.sceneEvents.Length; i++)
        {
            e = info.sceneEvents[i];
            for (int j = 0; j < e.behaviours.Length; j++)
            {
                if (e.behaviours[j].sceneBehaviorType == SceneEventInfo.SceneBehaviouType.StartStoryDialogue)
                {
                    EnumStoryType type = (EnumStoryType)e.behaviours[j].parameters[1];
                    list.AddRange(GetPerStoryPreAssets(e.behaviours[j].parameters[0],type));
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 针对不同的模式可能会加载不同的资源
    /// </summary>
    /// <param name="plotId"></param>
    /// <param name="type">1代表剧场对话,2代表战斗对话</param>
    /// <returns></returns>
    public static List<string> GetPerStoryPreAssets(int plotId, EnumStoryType type)
    {
        List<string> list = new List<string>();
        switch (type)
        {
            case EnumStoryType.TheatreStory:
            case EnumStoryType.NpcTheatreStory:
                list.Add(StoryConst.THEATRE_STORY_ASSET_NAME);
                list.Add(StoryConst.THEATRE_MASK_ASSET_NAME);
                list.Add(StoryConst.THEATRE_GM_ASSET_NAME);
                //list.Add(StoryConst.DEFALUT_CONTENT_BG);
                break;
            case EnumStoryType.FreeBattleStory:
            case EnumStoryType.PauseBattleStory:
                list.Add(StoryConst.BATTLE_STORY_ASSET_NAME);
                break;
        }

        StoryInfo story = ConfigManager.Get<StoryInfo>(plotId);
        if(story)
        {
            //if (type == EnumStoryType.TheatreStory) list.Add("character_render_camera");
            //load 
            StoryInfo.StoryItem item = null;
            for (int i = 0; i < story.storyItems.Length; i++)
            {
                if (i > GeneralConfigInfo.sstoryPreLoadNum && GeneralConfigInfo.sstoryPreLoadNum > 0)
                    break;

                item = story.storyItems[i];

                #region theatre story
                if (type == EnumStoryType.TheatreStory || type == EnumStoryType.NpcTheatreStory)
                {
                    //npc assets
                    for (int j = 0; j < item.talkingRoles.Length; j++)
                    {
                        NpcInfo npc = ConfigManager.Get<NpcInfo>(item.talkingRoles[j].roleId);
                        if (npc == null) continue;

                        var npcAssets = Module_Battle.BuildNpcSimplePreloadAssets(npc.ID);
                        for (int m = 0; m < npcAssets.Count; m++)
                        {
                            list.Add(npcAssets[m]);
                        }
                    }
                    //texture assets
                    list.Add(item.background);
                    list.Add(item.contentBg);
                    if (!item.playerIconDetail.Equals("PlayerIcon"))
                    list.Add(item.playerIconDetail);
                    //model assets
                    if(item.models != null && item.models.Length > 0)
                    {
                        for (int ii = 0; ii < item.models.Length; ii++)
                        {
                            list.Add(item.models[ii].model);
                        }
                    }
                }
                #endregion

                #region common data 
                    list.Add(item.effect);
                    if (item.effect.Equals(StoryConst.EYE_CLIP_EFFECT))
                    list.Add(StoryConst.EYE_GAUSS_MATERIAL_ASSET);

                //music assets
                if (item.musicData.validMusic && !item.musicData.musicName.Equals(StoryConst.STOP_CURRENT_MUSIC_FLAG)
                    && !item.musicData.musicName.Equals(StoryConst.RESET_CURRENT_MUSIC_FLAG) && !item.musicData.musicName.Equals(StoryConst.RESET_LAST_MUSIC_FLAG))
                    list.Add(item.musicData.musicName);
                //voice assets
                list.Add(item.voiceName);
                //sound effect assets
                for (int j = 0; j < item.soundEffect.Length; j++)
                {
                    list.Add(item.soundEffect[j].soundName);
                }
                #endregion
            }
        }

        return list;
    }

    public static List<string> GetPerStoryPreAssets(int plotId, EnumStoryType type,int index)
    {
        List<string> list = new List<string>();
        StoryInfo story = ConfigManager.Get<StoryInfo>(plotId);
        index += GeneralConfigInfo.sstoryPreLoadNum;
        if (story)
        {
            if (story.storyItems.Length - 1 < index || index < 0)
                return null;
            StoryInfo.StoryItem item = story.storyItems[index];

            #region theatre story
            if (type == EnumStoryType.TheatreStory || type == EnumStoryType.NpcTheatreStory)
            {
                //npc assets
                for (int j = 0; j < item.talkingRoles.Length; j++)
                {
                    NpcInfo npc = ConfigManager.Get<NpcInfo>(item.talkingRoles[j].roleId);
                    if (npc == null) continue;

                    var npcAssets = Module_Battle.BuildNpcSimplePreloadAssets(npc.ID);
                    for(int m = 0;m<npcAssets.Count;m++)
                    {
                        list.Add(npcAssets[m]);
                    }
                }

                //texture assets
                list.Add(item.background);
                list.Add(item.contentBg);
                if (!item.playerIconDetail.Equals("PlayerIcon"))
                    list.Add(item.playerIconDetail);

                //model assets
                if (item.models != null && item.models.Length > 0)
                {
                    for(int ii = 0;ii<item.models.Length;ii++)
                    {
                        list.Add(item.models[ii].model);
                    }
                }
            }
            #endregion

            #region common data 
            //full screen effect
                list.Add(item.effect);
                if (item.effect.Equals(StoryConst.EYE_CLIP_EFFECT)) list.Add(StoryConst.EYE_GAUSS_MATERIAL_ASSET);
            //music assets
            if (item.musicData.validMusic && !item.musicData.musicName.Equals(StoryConst.STOP_CURRENT_MUSIC_FLAG)
                && !item.musicData.musicName.Equals(StoryConst.RESET_CURRENT_MUSIC_FLAG) && !item.musicData.musicName.Equals(StoryConst.RESET_LAST_MUSIC_FLAG)
                )
                list.Add(item.musicData.musicName);
            //voice assets
             list.Add(item.voiceName);
            //sound effect assets
            for (int j = 0; j < item.soundEffect.Length; j++)
            {
                list.Add(item.soundEffect[j].soundName);
            }
            #endregion
        }
        return list;
    }

    #endregion

    #region dispath events
    public void DispatchStoryMaskEnd(int storyId, EnumStoryType type)
    {
        DispatchModuleEvent(EventStoryMaskEnd, storyId, type);
    }

    public void DispatchStoryStart(int storyId, EnumStoryType type)
    {
        m_inStoryMode = true;
        
        if (type == EnumStoryType.TheatreStory || type == EnumStoryType.NpcTheatreStory) UIManager.SetCameraLayer(Layers.Dialog);
        if (type == EnumStoryType.PauseBattleStory)
        {
            m_lastCameraCombatShotState = Camera_Combat.enableShotSystem;
            Camera_Combat.enableShotSystem = false;
        }
        DispatchModuleEvent(EventStoryStart,storyId,type);
    }
    public void DispatchStoryClosed(int storyId, EnumStoryType type)
    {
        if (m_inStoryMode == false) return;
        moduleNPCDating.RemoveEventListener(Module_NPCDating.EventAllStoryEnd, OnAllStoryEnd);
        DispatchModuleEvent(EventStoryClosed, storyId, type);
    }

    public bool DispatchStoryWillEnd(int storyId, EnumStoryType type)
    {
        
        if (m_storyEndCallBack == null)
        {
            Debug.Log("DispatchStoryWillEnd failure because callback is null");
            return false;
        }
        Action<int> tmp_cb = m_storyEndCallBack;
        m_storyEndCallBack = null;
        tmp_cb?.Invoke(storyId);
        return true;
    }

    public void DispatchStoryEnd(int storyId, EnumStoryType type)
    {
        ResetGlobalState();
        if (m_inStoryMode == false)
            return;
        UIManager.SetCameraLayer(Layers.UI);
        m_inStoryMode = false;

        if (type == EnumStoryType.PauseBattleStory) Camera_Combat.enableShotSystem = m_lastCameraCombatShotState;
       
        DispatchModuleEvent(EventStoryEnd, storyId, type);
        
        ResetStoryData();
        Logger.LogInfo($"剧情对话结束 {storyId}");
        //must before addpvecondition
        moduleGuide.SendFinishStory(storyId);
        Module_Guide.AddCondition(new StoryEndContition(storyId));
        AddPveCondition(storyId, type);

        //reset debug mode(only editor useable)
        if (debugStory)
        {
            debugStory = false;
            return;
        }

        //第一段对话完毕的时候，需要强制引导进入stageId = 1,GoHome函数内部发送消息，需要等待玩家数据返回后，才可以进入
        if (storyId == DEFAULT_STORY_ID)
        {
            if(Module_Guide.skipGuide) moduleLogin.GoHome();
            else
            {
                //moduleGlobal.LockUI();
                DispatchEvent(Events.SHOW_LOADING_WINDOW);
                moduleLogin.GoHome(true);
            }
        }
    }

    public void ResetStoryData()
    {
        currentStory = null;
        currentStoryIndex = 0;
        currentStoryType = EnumStoryType.None;
    }

    public void OnRecvRoleInfo()
    {
        if (string.IsNullOrEmpty(moduleLogin.createName) || Module_Guide.skipGuide) return;

        moduleLogin.createName = string.Empty;
        modulePVE.OnPVEStart(DEFAULT_STAGE_ID, PVEReOpenPanel.None);
        moduleGlobal.UnLockUI();
    }

    private void AddPveCondition(int storyId, EnumStoryType type)
    {
        if (!Level.current.isPvE) return;
        Logger.LogInfo("StoryDialogueEnd contition ,story id = {0}", storyId);
        modulePVEEvent.AddCondition(new SStoryDialogueEndConditon(storyId));
    }

    private void ResetGlobalState()
    {
        if (m_globalBarState >= 0)
        {
            moduleGlobal.RestoreGlobalLayerState(m_globalBarState);
            if(Level.current && Level.current.isBattle) moduleGlobal.ShowGlobalLayerDefault(2, false);
        }
        m_globalBarState = -1;
    }
#endregion

    #region battle story item

    public void SetSwitchPointCamera(Camera main,Camera ui)
    {
        m_monsterCamera = main;
        m_uiCamera = ui;
    }

    public void ClearBattleCache()
    {
        m_battleStoryDic.Clear();
        enableUpdate = false;
    }

    public void UpdateBattleCache(Creature monster, BaseBattleItem item)
    {
        if(monster == null)
        {
            Logger.LogError("battle item update error,talking monster is null");
            return;
        }
        
        if (!m_battleStoryDic.ContainsKey(monster)) m_battleStoryDic.Add(monster,null);
        m_battleStoryDic[monster] = item;

        if (item != null)
        {
            if (m_scalerFactor == 0f)
            {
                CanvasScaler cs = UIManager.canvasScaler;
                m_scalerFactor = cs.matchWidthOrHeight == 1 ? cs.referenceResolution.y / Screen.height : cs.referenceResolution.x / Screen.width;

                var ss = new Vector2(Screen.width * m_scalerFactor, Screen.height * m_scalerFactor);
                m_screenRect = new Rect(-ss.x * 0.5f, -ss.y * 0.5f , ss.x , ss.y);
            }
        }

        enableUpdate = m_battleStoryDic.Count > 0;
        UpdateBattleItemPos();
    }

    public void RemoveBattleCache(Creature creature)
    {
        m_battleStoryDic.Remove(creature);
        enableUpdate = m_battleStoryDic.Count > 0;
    }

    public void UpdateBattleItemPos()
    {
        if (m_battleStoryDic == null || m_battleStoryDic.Count == 0 || _CameraShake.isShaking || combatCameraMoving)
            return;

        Dictionary<Creature,BaseBattleItem>.Enumerator e = m_battleStoryDic.GetEnumerator();
        while (e.MoveNext())
        {
            if(e.Current.Key is MonsterCreature)
            {
                MonsterCreature monster = e.Current.Key as MonsterCreature;
                UIFollowTarget uiFollow = Window_HpSlider.GetFollowScript(monster);
                if (e.Current.Value == null || !e.Current.Value.transform || uiFollow == null || !m_monsterCamera)
                    continue;

                e.Current.Value.transform.position = uiFollow.transform.position;
                //pos = e.Current.Value.transform.localPosition + m_offset;
                //e.Current.Value.transform.localPosition = pos;
                SetUIArea(e.Current.Value.rectTransform, m_screenRect, UIManager.canvas.rectTransform());
            }
            else
            {
                if (e.Current.Value == null || !e.Current.Value.transform ||!m_monsterCamera)
                    continue;
                Vector3 worldpos = e.Current.Key.position;
                worldpos.y += 2.15f;//设置气泡高度
                Vector3 pos = m_monsterCamera.WorldToScreenPoint(worldpos);
                //must change build.screenPos.z as item position.z
                pos.z = e.Current.Value.transform.position.z;
                pos = m_uiCamera.ScreenToWorldPoint(pos);

                e.Current.Value.transform.position = pos;
                //pos = e.Current.Value.transform.localPosition + m_offset;
                //e.Current.Value.transform.localPosition = pos;
                SetUIArea(e.Current.Value.rectTransform, m_screenRect, UIManager.canvas.rectTransform());
            }
           
        }
    }

    public void SetUIArea(RectTransform target, Rect area, Transform canvas)
    {
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvas, target);

        Vector2 delta = m_padding;
        if (bounds.center.x - bounds.extents.x < area.x)//target超出area的左边框
        {
            delta.x += Mathf.Abs(bounds.center.x - bounds.extents.x - area.x);
        }
        else if (bounds.center.x + bounds.extents.x > area.width / 2)//target超出area的右边框
        {
            delta.x -= Mathf.Abs(bounds.center.x + bounds.extents.x - area.width / 2);
        }

        if (bounds.center.y - bounds.extents.y < area.y)//target超出area上边框
        {
            delta.y += Mathf.Abs(bounds.center.y - bounds.extents.y - area.y);
        }
        else if (bounds.center.y + bounds.extents.y > area.height / 2)//target超出area的下边框
        {
            delta.y -= Mathf.Abs(bounds.center.y + bounds.extents.y - area.height / 2);
        }

        //加上偏移位置算出在屏幕内的坐标
        target.anchoredPosition += delta;
    }
    #endregion

    #region on load level start

    protected override void OnModuleCreated()
    {
        base.OnModuleCreated();
        EventManager.AddEventListener(Events.SCENE_LOAD_START,      OnSceneLoadStart);
        EventManager.AddEventListener(Events.SCENE_LOAD_COMPLETE,   OnSceneLoadComplete);
        ClearLoadedStory();
        RegisterStoryType();
    }

    private void OnSceneLoadStart(Event_ e)
    {
        ClearLoadedStory();
        SetCameraTransToGlobal();
    }

    private void OnSceneLoadComplete(Event_ e)
    {
        if(e.sender is Level_Login && !npcCameraTrans)
        {
            npcCameraTrans = Level.current.root.AddNewChild(Level.current.root.Find("cameras/stroyCamera").gameObject);
            postProcessingDefalutProfile = npcCameraTrans.GetComponent<PostProcessingBehaviour>().profile;
            npcCameraTrans.gameObject.SetActive(false);
            AudioListener audio = npcCameraTrans.GetComponent<AudioListener>();
            if (audio) Object.DestroyImmediate(audio);
            //reset camera tag
            npcCameraTrans.gameObject.tag = "Untagged";
            SetCameraTransToGlobal();
        }
    }

    public void SetCameraTransToGlobal()
    {
        Window_Global w = Window.GetOpenedWindow<Window_Global>();
        if (!w) return;

        if (!npcCameraTrans) return;

        npcCameraTrans.SetParent(w.transform);
        npcCameraTrans.gameObject.SetActive(false);
    }

    private void RegisterStoryType()
    {
        m_storyTypeDic.Clear();
        m_storyTypeDic.Add(EnumStoryType.TheatreStory,      typeof(TheatreStory));
        m_storyTypeDic.Add(EnumStoryType.FreeBattleStory,   typeof(FreeBattleStory));
        m_storyTypeDic.Add(EnumStoryType.PauseBattleStory,  typeof(PauseBattleStory));
        m_storyTypeDic.Add(EnumStoryType.NpcTheatreStory,   typeof(DataTheatry));
    }

    private void ClearLoadedStory()
    {
        foreach (var item in m_loadedStory)
        {
            foreach (var o in item.Value)
            {
                Object.DestroyImmediate(o.gameObject);
            }
        }
        m_loadedStory.Clear();
    }

    #endregion

    #region message

    public void RequestProp(int storyId,StoryInfo.GivePropData[] props)
    {
        if (props == null || props.Length == 0 || debugStory) return;

        CsGuideItem p = PacketObject.Create<CsGuideItem>();
        p.guideId = moduleGuide.currentGuide ? moduleGuide.currentGuide.ID : 0;
        p.storyId = storyId;
        int len = props.Length;
        p.items = new PItem2[len];
        for (int i = 0; i < len; i++)
        {
            p.items[i] = props[i].GetPItem2();
        }
        session.Send(p);

        moduleGlobal.LockUI(string.Empty,0.5f);
    }

    void _Packet(ScGuideItem item)
    {
        moduleGlobal.UnLockUI();
    }

    void _Packet(ScLogout p)
    {
        OnLostConnection();
    }

    #endregion

    #region override module
    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();

        m_inStoryMode = false;

        foreach (var item in m_loadedStory)
        {
            foreach (var story in item.Value)
            {
                story.gameObject.SetActive(false);
            }
        }

        //cancel the fullscreen lock image
        moduleGlobal.UnLockUI(Module_Global.GUIDE_LOCK_PRIORITY);
        DispatchStoryMaskEnd(0, EnumStoryType.None);

        UIManager.SetCameraLayer(Layers.UI);
        var nameWindow = Window.GetOpenedWindow<Window_Name>();
        if (nameWindow) nameWindow.Hide(true);
        var professionWindow = Window.GetOpenedWindow<Window_Profession>();
        if (professionWindow) professionWindow.Hide(true);
        var alertWindow = Window.GetOpenedWindow<Window_Alert>();
        if (alertWindow) alertWindow.Hide(true);
    }

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        Root.instance.AddEventListener(Events.COMBAT_CAMERA_MOVE_CHANGE, OnCombatCameraMove);
    }

    protected override void OnGameStarted()
    {
        session.AddEventListener(Events.SESSION_LOST_CONNECTION, OnLostConnection);
    }
   
    private void OnLostConnection()
    {
        UIManager.SetCameraLayer(Layers.UI, Layers.Dialog);
    }

    private void OnCombatCameraMove(Event_ e)
    {
        if(Level.current is Level_PVE) combatCameraMoving = (bool)e.param1;
    }

    #endregion
    
    public override void OnUpdate(int diff)
    {
        base.OnUpdate(diff);

        UpdateBattleItemPos();
    }

    private void OnAllStoryEnd(Event_ e)
    {
        if(GetCurrentValidStory() == null && currentStory != null)
        {
            DispatchStoryClosed(currentStory.ID, currentStoryType);
            DispatchStoryEnd(currentStory.ID, currentStoryType);
        }
        else
        {
            moduleNPCDating.RemoveEventListener(Module_NPCDating.EventAllStoryEnd, OnAllStoryEnd);
        }
    }
}

#region custom class

public class DialogReviewData : PoolObject<DialogReviewData>
{
    private string m_format;
    
    public StoryInfo.StoryItem storyItem { get; private set; }

    /// <summary> 需要替换的商品数据 </summary>
    public DatingFoodOrder orderData { get; private set; }

    public DivinationData divinationData { get; private set; }

    /// <summary> npc数据 </summary>
    public NpcInfo npcData { get; private set; }

    /// <summary> 是否是自己说话  </summary>
    public bool IsSelf { get { return !npcData; } }

    /// <summary> 昵称 npc则是npc名字 如果是自己就是玩家昵称 如果item中有替换名，则显示为替换名 </summary>
    public string nickName { get; private set; }

    /// <summary> 正文内容 </summary>
    public string mainBody { get; private set; }

    /// <summary> 最终的文字显示 eg: 艾丽卡：我正在说话 </summary>
    public string content { get { return Util.Format(m_format, nickName, mainBody); } }


    protected override void OnInitialize()
    {
        base.OnInitialize();
        ResetAllData();
    }
    
    public void RefreshDialogReviewData(StoryInfo.StoryItem item, DatingFoodOrder order, DivinationData divination)
    {
        storyItem = item;
        orderData = order;
        divinationData = divination;
        RefreshDialogReviewData();
    }
    
    public void RefreshDialogReviewData()
    {
        if (storyItem == null)
        {
            ResetAllData();
            return;
        }

        if(storyItem.replaceName.Equals(StoryConst.PLAYER_NAME_PARAM))
        {
            npcData = null;
            nickName = Module_Story.GetReplaceName(storyItem);
            m_format = ConfigText.GetDefalutString(TextForMatType.StoryUIText,5);
        }
        else
        {
            m_format = ConfigText.GetDefalutString(TextForMatType.StoryUIText, 4);
            if (storyItem.talkingRoles != null )
            {
                if (storyItem.talkingRoles.Length > 0)
                {
                    var d = storyItem.talkingRoles.GetValue<StoryInfo.TalkingRoleData>(0);
                    if (d != null)
                    {
                        npcData = ConfigManager.Get<NpcInfo>(d.roleId);
                        nickName = npcData ? Util.GetString(npcData.name) : string.Empty;
                    }
                }
            }
            else
            {
                Logger.LogError("配置表丢失剧情对话对象字段, 请检查配置表.... ");
            }

            if (storyItem.needReplaceName) nickName = storyItem.replaceName;
        }

        if (string.IsNullOrEmpty(m_format)) m_format = "{0}/{1}";
        mainBody = Module_Story.GetReplaceContent(storyItem, orderData, divinationData);
    }

    private void ResetAllData()
    {
        orderData = null;
        npcData = null;
        m_format = string.Empty;
        nickName = string.Empty;
        mainBody = string.Empty;
    }
}

public class ReviewSourceData : PoolObject<ReviewSourceData>
{
    /// <summary> 剧情对话的ID </summary>
    public int storyId { get; private set; }

    /// <summary> 菜单订单数据 </summary>
    public DatingFoodOrder orderData { get; private set; }

    /// <summary> 占卜数据 </summary>
    public DivinationData diviData { get; private set; }

    private ReviewSourceData() { }

    public static ReviewSourceData Create(int storyId)
    {
        return Create(storyId, null, null);
    }

    public static ReviewSourceData Create(int storyId, DivinationData divination)
    {
        return Create(storyId, null, divination);
    }

    public static ReviewSourceData Create(int storyId, DatingFoodOrder order)
    {
        return Create(storyId, order, null);
    }

    public static ReviewSourceData Create(int storyId, DatingFoodOrder order, DivinationData divination)
    {
        var review = Create();
        review.storyId = storyId;
        review.orderData = order;
        review.diviData = divination;
        return review;
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        storyId = 0;
        orderData = null;
        diviData = null;
    }
}

#endregion
