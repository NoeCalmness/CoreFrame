/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Common platform native plugins and SDK support
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-04-01
 * 
 ***************************************************************************************************/

using UnityEngine;

using cn.sharesdk.unity3d;
using com.mob.mobpush;
using System;
using System.Collections;
using System.IO;
using ShareResponse = cn.sharesdk.unity3d.ResponseState;
using System.Collections.Generic;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

/// <summary>
/// Manage all common platform native plugins and SDK support
/// </summary>
[AddComponentMenu("HYLR/Utilities/SDK Manager")]
public partial class SDKManager : SingletonBehaviour<SDKManager>
{
    #region Manager

    private delegate void EventHandler(string eventName, Event_ e);

    private static Dictionary<string, EventHandler> m_eventHandlers = new Dictionary<string, EventHandler>();

    /// <summary>
    /// 注册事件监听
    /// </summary>
    /// <param name="handler">回调</param>
    /// <param name="eventName">事件，可以为空，为空时表示 Module Event 和 SDKE vent， 若指定事件名，表示普通事件</param>
    private static void RegisterEventListener(EventHandler handler, string eventName = null)
    {
        if (string.IsNullOrEmpty(eventName)) eventName = string.Empty;

        var h = m_eventHandlers.Get(eventName);
        h -= handler;
        h += handler;
        m_eventHandlers[eventName] = h;
    }

    /// <summary>
    /// Initialize SDK Manager
    /// </summary>
    public static void Initialize()
    {
        instance._Initialize();
    }

    public static bool initialized { get; private set; }

    private void _Initialize()
    {
        if (initialized) return;
        initialized = true;

        SetRealFullScreen();

        InitializeShareSDK();
        InitializeMobPush();
        InitializeTracking();
        InitializeThinking();

        AddEventListeners();
    }

    private static void AddEventListeners()
    {
        #if !UNITY_EDITOR
        EventManager.AddEventListener(ModuleEvent.GLOBAL,    OnEvent);
        EventManager.AddEventListener(SDKEvent.GLOBAL,       OnEvent);

        foreach (var pair in m_eventHandlers)
        {
            if (string.IsNullOrEmpty(pair.Key)) continue;
            EventManager.AddEventListener(pair.Key, OnEvent);
        }
        #endif
    }

    private static void OnEvent(Event_ e)
    {
        var eventName = e.name;
        var me = e as ModuleEvent;
        var se = me == null ? e as SDKEvent : null;

        if (me != null) eventName = me.moduleEvent;
        else if (se != null) eventName = se.eventName;
    
        var h = e.name != eventName ? m_eventHandlers[string.Empty] as EventHandler : m_eventHandlers[eventName] as EventHandler;
        h?.Invoke(eventName, e);
    }

    #region Modules
    private static Module_Global moduleGlobal => Module_Global.instance;
    private static Module_Set moduleSet => Module_Set.instance;
    private static Module_Player modulePlayer => Module_Player.instance;
    private static Module_Skill moduleSkill => Module_Skill.instance;
    private static Module_Guide moduleGuide => Module_Guide.instance;
    private static Module_Login moduleLogin => Module_Login.instance;
    #endregion

    #endregion

    #region Platform helpers

    #region iOS
    #if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern string ValidateHost(string host);
    [DllImport("__Internal")] private static extern bool   SaveImageToAlbum(string path);
    #endif

    #endregion

    #region Android
    #if UNITY_ANDROID
    private static AndroidJavaObject aliya
    {
        get
        {
            if (m_aliya == null) m_aliya = new AndroidJavaObject("com.fycsgame.cutecat.Aliya");
            return m_aliya;
        }
    }
    private static AndroidJavaObject m_aliya = null;

    public static T CallAliya<T>(string method, params object[] args)
    {
        try
        {
            return aliya.CallStatic<T>(method, args);
        }
        catch (Exception e)
        {
            Logger.LogException(e);
        }

        return default(T);
    }

    public static void CallAliya(string method, params object[] args)
    {
        try
        {
            aliya.CallStatic(method, args);
        }
        catch (Exception e)
        {
            Logger.LogException(e);
        }
    }
    #endif

    #endregion

    public static bool SaveImageToPhotoLibrary(string path, string message = null)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return false;

        var success = true;
        #if UNITY_EDITOR
        if (string.IsNullOrEmpty(message)) message = Util.GetString(9005, 2, path);
        #else
        if (string.IsNullOrEmpty(message)) message = Util.GetString(9005, 3, Path.GetFileNameWithoutExtension(path));
        #if UNITY_IOS
        success = SaveImageToAlbum(path);
        #elif UNITY_ANDROID
        success = CallAliya<bool>("SaveImageToAlbum", path);
        #endif
        #endif

        if (!success) moduleGlobal.ShowMessage(9005, 1);
        else moduleGlobal.ShowMessage(message);

        return success;
    }

    public static string GetCPUInfo()
    {
        var cpuInfo = string.Empty;
        #if UNITY_EDITOR
        cpuInfo = SystemInfo.processorType;
        #elif UNITY_ANDROID
        cpuInfo = CallAliya<string>("GetCPUInfo");
        #elif UNITY_IOS
        cpuInfo = SystemInfo.processorType;
        #endif

        return string.IsNullOrEmpty(cpuInfo) ? "unknow" : cpuInfo;
    }

    /// <summary>
    /// Validate a host
    /// return: "Family,validHost", e.g: "0,192.168.3.254"
    /// Family: 0 = IPv4  1 = IPv6  other = invalid
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public static string GetValidHost(string host)
    {
        string validHost = null;
        #if UNITY_IOS && !UNITY_EDITOR
        validHost = ValidateHost(host);
        #elif UNITY_ANDROID && !UNITY_EDITOR
        validHost = CallAliya<string>("ValidateHost", host);
        #else
        validHost = "0," + host;
        #endif

        return string.IsNullOrEmpty(validHost) ? "0," + host : validHost;
    }

    /// <summary>
    /// Get device safe insets
    /// return: "top;bottom"
    /// </summary>
    /// <returns></returns>
    public static string GetSafeInsets()
    {
        string insets = "";
        #if UNITY_IOS && !UNITY_EDITOR
        #elif UNITY_ANDROID && !UNITY_EDITOR
        insets = CallAliya<string>("GetSafeInsets");
        #endif

        return insets;
    }

    /// <summary>
    /// Get device status bar height
    /// </summary>
    /// <returns></returns>
    public static int GetStatusBarHeight()
    {
        int h = 25;
        #if UNITY_IOS && !UNITY_EDITOR
        #elif UNITY_ANDROID && !UNITY_EDITOR
        h = CallAliya<int>("GetStatusBarHeight");
        #endif

        return h;
    }

    public static void SetRealFullScreen()
    {
        #if !UNITY_EDITOR && UNITY_ANDROID
        CallAliya("SetFullScreen");
        #elif !UNITY_EDITOR && UNITY_IOS
        #endif
    }

    #endregion

    #region Tracking

    [Space(5)]
    [Header("Tracking")]
    [SerializeField, Set("trackingDebug")]
    private bool m_trackingDebug = false;

    public string trackingAppKeyAndroid = string.Empty;
    public string trackingAppKeyIOS = string.Empty;
    public string trackingChannelID = "_default_";
    private static string channelID = "0";

    public bool trackingDebug
    {
        get { return m_trackingDebug; }
        set
        {
            if (m_trackingDebug == value) return;
            m_trackingDebug = value;

            SetPrintLog(m_trackingDebug);
        }
    }

    #if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void   _internalInitWithAppKeyAndChannel_Tracking(string appKey, string channelId);
    [DllImport("__Internal")] private static extern void   _internalSetRegisterWithAccountID_Tracking(string account);
    [DllImport("__Internal")] private static extern void   _internalSetLoginWithAccountID_Tracking(string account);
    [DllImport("__Internal")] private static extern void   _internalSetRyzfStart_Tracking(string transactionId, string ryzfType, string currencyType, float currencyAmount);
    [DllImport("__Internal")] private static extern void   _internalSetRyzf_Tracking(string transactionId, string ryzfType, string currencyType, float currencyAmount);
    [DllImport("__Internal")] private static extern void   _internalSetOrder_Tracking(string transactionId, string currencyType, float currencyAmount);
    [DllImport("__Internal")] private static extern void   _internalSetEvent_Tracking(string EventName);
    [DllImport("__Internal")] private static extern string _internalGetDeviceId_Tracking();
    [DllImport("__Internal")] private static extern void   _internalSetPrintLog_Tracking(bool print);
    #endif

    #if UNITY_ANDROID
    public static AndroidJavaClass tracking
    {
        get
        {
            if (m_tracking == null)
            {
                try
                {
                    m_tracking = new AndroidJavaClass("com.reyun.tracking.sdk.Tracking");
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            }
            return m_tracking;
        }
    }
    public static AndroidJavaClass trackingConst
    {
        get
        {
            if (m_trackingConst == null)
            {
                try
                {
                    m_trackingConst = new AndroidJavaClass("com.reyun.tracking.common.ReYunConst");
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            }
            return m_trackingConst;
        }
    }
    public static AndroidJavaObject applicationContext
    {
        get
        {
            if (m_applicationContext == null)
            {
                try
                {
                    m_applicationContext = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity").Call<AndroidJavaObject>("getApplicationContext");
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            }
            return m_applicationContext;
        }
    }

    private static AndroidJavaClass m_tracking = null;
    private static AndroidJavaClass m_trackingConst = null;
    private static AndroidJavaObject m_applicationContext = null;

    public static T CallTracking<T>(string method, params object[] args)
    {
        try
        {
            return tracking.CallStatic<T>(method, args);
        }
        catch (Exception e)
        {
            Logger.LogException(e);
        }

        return default(T);
    }

    public static void CallTracking(string method, params object[] args)
    {
        try
        {
            tracking.CallStatic(method, args);
        }
        catch (Exception e)
        {
            Logger.LogException(e);
        }
    }

    public static void SetTrackingConst<T>(string name, T val)
    {
        try
        {
            trackingConst.SetStatic(name, val);
        }
        catch (Exception e)
        {
            Logger.LogException(e);
        }
    }
    #endif

    /// <summary>
    /// 初始化方法   
    /// </summary>
    /// <param name="appId">appKey</param>
    /// <param name="channelId">标识推广渠道的字符</param>
    public static void InitializeTracking()
    {
        channelID = instance.trackingChannelID == "_default_" || string.IsNullOrEmpty(instance.trackingChannelID) ? WebAPI.PLATFORM_TYPE.ToString() : instance.trackingChannelID;

        if (!string.IsNullOrEmpty(WebAPI.platformSubType)) channelID = $"{channelID}-{WebAPI.platformSubType}";

        SetPrintLog(instance.trackingDebug);

        var key = string.Empty;
        #if UNITY_IOS && !UNITY_EDITOR
        key = instance.trackingAppKeyIOS;
        _internalInitWithAppKeyAndChannel_Tracking(key, channelID);
        #elif UNITY_ANDROID && !UNITY_EDITOR
        key = instance.trackingAppKeyAndroid;
        CallTracking("initWithKeyAndChannelId", applicationContext, key, channelID);
        #endif

        Logger.LogDetail("SDKManager:: Initialize Tracking. channelID: [{0}], key: [{1}]", channelID, key);
    }

    /// <summary>
    /// 玩家服务器注册
    /// </summary>
    /// <param name="account">账号ID</param>
    public static void Register(string account)
    {
        #if UNITY_IOS && !UNITY_EDITOR
        _internalSetRegisterWithAccountID_Tracking(account);    
        #elif UNITY_ANDROID && !UNITY_EDITOR
        CallTracking("setRegisterWithAccountID", account);
        #endif
    }

    /// <summary>
    /// 玩家的账号登陆服务器
    /// </summary>
    /// <param name="account">账号</param>
    public static void Login(string account)
    {
        #if UNITY_IOS && !UNITY_EDITOR
        _internalSetLoginWithAccountID_Tracking(account);
        #elif UNITY_ANDROID && !UNITY_EDITOR
        CallTracking("setLoginSuccessBusiness", account);
        #endif
    }

    /// <summary>
    /// 玩家开始充值数据
    /// </summary>
    /// <param name="transactionId">交易的流水号</param>
    /// <param name="paymentType">支付类型</param>
    /// <param name="currencyType">货币类型</param>
    /// <param name="currencyAmount">支付的真实货币的金额</param>
    public static void SetryzfStart(string transactionId, string ryzfType, byte currencyType, float currencyAmount)
    {
        #if !UNITY_EDITOR
        var _currencyType = currencyType == 0 ? "CNY" : currencyType == 1 ? "USD" : currencyType.ToString();
        #if UNITY_IOS
        _internalSetRyzfStart_Tracking(transactionId, ryzfType, _currencyType, currencyAmount);
        #elif UNITY_ANDROID
        CallTracking("setPaymentStart", transactionId, ryzfType, _currencyType, currencyAmount);
        #endif
        #endif
    }

    /// <summary>
    /// 玩家的充值数据
    /// </summary>
    /// <param name="transactionId">交易的流水号</param>
    /// <param name="paymentType">支付类型</param>
    /// <param name="currencyType">0 = CNY 1 = USD</param>
    /// <param name="currencyAmount">支付的真实货币的金额</param>
    public static void Setryzf(string transactionId, string ryzfType, byte currencyType, float currencyAmount)
    {
        #if !UNITY_EDITOR
        var _currencyType = currencyType == 0 ? "CNY" : currencyType == 1 ? "USD" : currencyType.ToString();
        #if UNITY_IOS
        _internalSetRyzf_Tracking(transactionId, ryzfType, _currencyType, currencyAmount);
        #elif UNITY_ANDROID
        CallTracking("setPayment", transactionId, ryzfType, _currencyType, currencyAmount);
        #endif
        #endif
    }

    public static void SetOrder(string transactionId, byte currencyType, float currencyAmount)
    {
        #if !UNITY_EDITOR
        var _currencyType = currencyType == 0 ? "CNY" : currencyType == 1 ? "USD" : currencyType.ToString();
        #if UNITY_IOS
        _internalSetOrder_Tracking(transactionId, _currencyType, currencyAmount);
        #elif UNITY_ANDROID
        CallTracking("setOrder", transactionId, _currencyType, currencyAmount);
        #endif
        #endif
    }

    /// <summary>
    /// 统计玩家的自定义事件
    /// </summary>
    /// <param name="eventName">事件名</param>    
    public static void SetEvent(string eventName)
    {
        #if UNITY_IOS && !UNITY_EDITOR
        _internalSetEvent_Tracking(eventName);
        #elif UNITY_ANDROID && !UNITY_EDITOR
        CallTracking("setEvent", eventName, null);
        #endif
    }

    /// <summary>
    /// 获取用户的设备ID信息
    /// </summary>
    public static string GetDeviceId()
    {
        var dev = "unknown";
        #if UNITY_IOS && !UNITY_EDITOR
        dev = _internalGetDeviceId_Tracking();
        #elif UNITY_ANDROID && !UNITY_EDITOR
        dev = CallTracking<string>("getDeviceId");
        #endif
        return dev;
    }

    /// 开启日志打印
    public static void SetPrintLog(bool print)
    {
        #if UNITY_IOS && !UNITY_EDITOR
        _internalSetPrintLog_Tracking(print);
        #elif UNITY_ANDROID && !UNITY_EDITOR
        SetTrackingConst("DebugMode", print);
        #endif
    }

    #endregion

    #region Mob shared config (Share and Push)

    [Space(5)]
    [Header("Mob")]
    public string mobAppKey = string.Empty;
    public string mobAppSecret = string.Empty;

    #endregion

    #region Share

    private static ShareSDK m_shareSDK = null;
    private static ShareContent m_shareContent = null;
    private static Action<int, int> m_onShare = null;

    private void InitializeShareSDK()
    {
        m_shareSDK = this.GetComponentDefault<ShareSDK>();
        m_shareSDK.shareHandler = OnShareCallback;
        m_shareSDK.InitSDK(mobAppKey, mobAppSecret);

        Logger.LogDetail("SDKManager:: Initialize ShareSDK. key: [{0}], secret: [{1}]", mobAppKey, mobAppSecret);
    }

    /// <summary>
    /// Share a local image
    /// <para>
    /// Callback result: 0 = Begin  1 = Success  2 = Failed  3 = Cancled  4 = Begin Upload
    /// </para>
    /// </summary>
    public static int ShareImage(PlatformType type, string path, string text, Action<int, int> callback = null, int contentType = -1)
    {
        #if !UNITY_EDITOR
        if (m_shareContent == null) m_shareContent = new ShareContent();

        var title = Util.GetString(9005, 5);
        var titleUrl = Util.GetString(9005, 6);
        m_shareContent.SetImagePath(path);

        if (type == PlatformType.SinaWeibo) m_shareContent.SetText(title + titleUrl);
        else if (type == PlatformType.QZone || type == PlatformType.QQ)
        {
            m_shareContent.SetTitle(title);
            m_shareContent.SetTitleUrl(titleUrl);
            m_shareContent.SetText(text);
            if (type == PlatformType.QZone)
            {
        #if UNITY_ANDROID
                m_shareContent.SetSite("");
                m_shareContent.SetSiteUrl(titleUrl);
        #endif
            }
        }
        else if (type == PlatformType.WeChat || type == PlatformType.WeChatMoments)
        {
            if (contentType == -1) contentType = ContentType.Image;

            m_shareContent.SetShareType(contentType);
            m_shareContent.SetTitle(title);
            if (contentType == ContentType.Webpage) m_shareContent.SetUrl(titleUrl);
            m_shareContent.SetText(text);
        }

        m_onShare = callback;

        return m_shareSDK.ShareContent(type, m_shareContent);
        #endif
        return 0;
    }

    private static void OnShareCallback(int reqID, ShareResponse state, PlatformType type, Hashtable data)
    {
        Logger.LogDetail($"OnShareCallback: [ID:{reqID},state:{state},type:{type},data:{data}]");
        if (state == ShareResponse.Success) m_onShare?.Invoke(reqID, (int)state);
    }

    #endregion

    #region Push

    public const string PushTagType_1 = "tag_switch_{0}";
    public const string PushTagType_2 = "tag_unionId_{0}";

    private static MobPush m_mobPush;
    private static readonly string[] m_pushResponseNames = { "CoutomMessage", "MessageRecvice", "MessageOpened" };

    private void InitializeMobPush()
    {
        m_mobPush = this.GetComponentDefault<MobPush>();

        #if !UNITY_EDITOR
        m_mobPush.initPushSDK(mobAppKey, mobAppSecret);

        m_mobPush.addPushReceiver();

        m_mobPush.onNotifyCallback = OnPushNotifyHandler;
        m_mobPush.onTagsCallback   = OnPushTagsHandler;
        m_mobPush.onAliasCallback  = OnPushAliasHandler;
        m_mobPush.onRegIdCallback  = OnPushRegIdHandler;

        #if UNITY_IPHONE    
        m_mobPush.setAPNsForProduction(false); // 真机调试 false , 上线 true
        var style = new CustomNotifyStyle();
        style.setType(CustomNotifyStyle.AuthorizationType.Badge | CustomNotifyStyle.AuthorizationType.Sound | CustomNotifyStyle.AuthorizationType.Alert);
        m_mobPush.setCustomNotification(style);
        #endif

        m_mobPush.getRegistrationId();

        #if UNITY_ANDROID
        m_mobPush.setClickNotificationToLaunchPage(true);
        m_mobPush.setAppForegroundHiddenNotification(true);
        #endif

        RegisterEventListener(OnPushEvent);
        #endif

        Logger.LogDetail("SDKManager:: Initialize MobPush. key: [{0}], secret: [{1}]", mobAppKey, mobAppSecret);
    }

    #region Callbacks

    private void OnPushRegIdHandler(string regId)
    {
        Logger.LogDetail($"SDKManager::OnPushRegIdHandler: Register ID:{regId}");
    }

    /// <summary>
    /// action = 4 operation 0:getAlias 1:添加操作 2:删除操作
    /// </summary>
    /// <param name="action"></param>
    /// <param name="alias"></param>
    /// <param name="operation"></param>
    /// <param name="errorCode"></param>
    private void OnPushAliasHandler(int action, string alias, int operation, int errorCode)
    {
        Logger.LogDetail("SDKManager::OnPushAliasHandler: {0} alias <b>[{1}]</b> result: <b>{2}</b>", operation == 0 ? "Get" : operation == 1 ? "Add" : "Remove", alias, errorCode);
    }

    /// <summary>
    /// action = 3 operation 0:getTags 1:添加操作 2:删除操作
    /// 删除所有tag是没有回调
    /// </summary>
    /// <param name="action"></param>
    /// <param name="tags"></param>
    /// <param name="operation"></param>
    /// <param name="errorCode"></param>
    private void OnPushTagsHandler(int action, string[] _tags, int operation, int errorCode)
    {
        Logger.LogDetail("SDKManager::OnPushTagsHandler: {0} tag <b>[{1}]</b> result: <b>{2}</b>", operation == 0 ? "Get" : operation == 1 ? "Add" : "Remove", _tags.PrettyPrint(','), errorCode);
    }

    /// <summary>
    /// 回调action:0:透传 1:通知 2:打开通知
    /// </summary>
    /// <param name="action"></param>
    /// <param name="resulte"></param>
    private void OnPushNotifyHandler(int action, Hashtable resulte)
    {
        var actionName = action < 0 || action >= m_pushResponseNames.Length ? action.ToString() : m_pushResponseNames[action];
        Logger.LogDetail("SDKManager::OnPushNotifyHandler: Action<b>[{0}({1})], data:[{2}]", action, actionName, resulte != null ? MiniJSON.jsonEncode(resulte) : "null");
    }

    #endregion

    #region Events

    private static void OnPushEvent(string eventName, Event_ e)
    {
        switch (eventName)
        {
            #region Module events
            case Module_Player.EventFatigueChanged:
            {
                var time = modulePlayer.fatigueRemainTime;
                DelayTimeToNitifyFatigue(time);
                break;
            }
            case Module_Skill.EventUpdateSkillPoint:
            {
                var time = moduleSkill.remainTime;
                DelayTimeToNitifySkill(time);
                break;
            }
            #endregion

            #region SDK events      
            case SDKEvent.SELECT_ROLE:    // 添加别名
            {
                var roleId = Convert.ToString((ulong)e.param1);
                m_mobPush.addAlias(roleId);
                break;
            }
            case SDKEvent.TAG:    // tag操作不会操作工会tag
            {
                var type = (uint)e.param1;
                var _id = (byte)e.param2;
                var tag = Util.Format(PushTagType_1, _id);

                if (type == 1) m_mobPush.addTags(new string[] { tag });
                else m_mobPush.deleteTags(new string[] { tag });

                Logger.LogDetail("SDKManager:: {0} tag:{1}", type == 0 ? "删除" : "添加", tag);
                break;
            }
            case SDKEvent.LOCAL_NOTIFY:    // 设置体力和技能点的本地通知
            {
                var type = (SwitchType)e.param1;
                if (type == SwitchType.Fatigue)
                {
                    var timeStamp = modulePlayer.fatigueRemainTime;
                    DelayTimeToNitifyFatigue(timeStamp);
                }
                else if (type == SwitchType.SkillPoint)
                {
                    if (!moduleGuide.IsActiveFunction(HomeIcons.Skill)) break;
                    var timeStamp = moduleSkill.remainTime;
                    DelayTimeToNitifySkill(timeStamp);
                }
                break;
            }     
            case SDKEvent.UNION_CHANGE:   // 设置工会tag
            {
                var key = modulePlayer.id_ + SwitchType.UnionBoss.ToString();
                var prefsTag = PlayerPrefs.GetString(key);

                var unionId = (ulong)e.param1;
                var type = (uint)e.param2;

                if (unionId == 0)
                {
                    if (!string.IsNullOrEmpty(prefsTag))
                        m_mobPush.deleteTags(new string[] { prefsTag });
                    return;
                }

                var tag = Util.Format(PushTagType_2, unionId);

                Logger.LogDetail("SDKManager:: {0} 工会 tag:{1}", type == 0 ? "删除" : "添加", tag);

                if (type == 0)
                {
                    m_mobPush.deleteTags(new string[] { tag });
                    PlayerPrefs.DeleteKey(key);
                }
                else
                {
                    m_mobPush.addTags(new string[] { tag });
                    PlayerPrefs.SetString(key, tag);
                }
                break;
            }
            #endregion

            default: break;
        }
    }

    private static void DelayTimeToNitifyFatigue(long time)
    {
        if (modulePlayer.roleInfo == null || modulePlayer.id_ == 0 || moduleGlobal.system == null) return;

        Logger.LogDetail("SDKManager:: Send <b><color=#00FF00>[{0}]</color></b> local notification!", SwitchType.Fatigue);

        var fatigue = modulePlayer.roleInfo.fatigue;
        var max     = modulePlayer.maxFatigue;
        var index   = modulePlayer.roleInfo.index;

        if (index <= 0) return;

        var value = PlayerPrefs.GetInt(index + SwitchType.Fatigue.ToString());
        if (value > 0) m_mobPush.removeLocalNotification(value);

        if (!moduleSet.pushState.ContainsKey(SwitchType.Fatigue)) return;

        var state = moduleSet.pushState[SwitchType.Fatigue];
        if (state == 0) return;

        if (fatigue >= max) return;

        var intervalTime = moduleGlobal.system.fatigue;
        var remainTime = ((max - fatigue) * intervalTime) + time - intervalTime;
        if (remainTime > 0) SetMobPushFatigue(index, remainTime);
    }

    private static void SetMobPushFatigue(uint index, long time)
    {
        var value = (int)index + 1;
        PlayerPrefs.SetInt(index + SwitchType.Fatigue.ToString(), value);

        var style = new LocalNotifyStyle();

        style.setNotifyId(value.ToString());
        style.setTitle(Util.GetString(278, 0));
        style.setContent(Util.GetString(278, 1));
        style.setTimestamp(time * 1000);

        m_mobPush.setMobPushLocalNotification(style);

        Logger.LogDetail("SDKManager:: Set <b><color=#00FF00>[{0}]</color></b> notification delay time to <b><color=#FFFFFF>{1}</color></b>", SwitchType.Fatigue, time * 1000);
    }

    private static void DelayTimeToNitifySkill(long time)
    {
        if (modulePlayer.roleInfo == null || modulePlayer.id_ == 0 || moduleGlobal.system == null || moduleSkill.skillInfo == null) return;

        Logger.LogDetail("SDKManager:: Send <b><color=#00FF00>[{0}]</color></b> local notification!", SwitchType.SkillPoint);

        var skill = moduleSkill.skillInfo.skillPoint;
        var max = moduleGlobal.system.skillPointLimit;
        var index = modulePlayer.roleInfo.index;

        if (index <= 0) return;

        var value = PlayerPrefs.GetInt(index + SwitchType.SkillPoint.ToString());
        if (value > 0) m_mobPush.removeLocalNotification(value);

        if (!moduleSet.pushState.ContainsKey(SwitchType.SkillPoint)) return;

        var state = moduleSet.pushState[SwitchType.SkillPoint];
        if (state == 0) return;

        if (skill >= max) return;

        var intervalTime = moduleGlobal.system.skillpoint;
        var remainTime = ((max - skill) * intervalTime) + time - intervalTime;
        if (remainTime > 0) SetMobPushSkill(index, remainTime);
    }

    private static void SetMobPushSkill(uint index, long time)
    {
        var value = (int)index + 22;
        PlayerPrefs.SetInt(index + SwitchType.SkillPoint.ToString(), value);

        var style = new LocalNotifyStyle();
        style.setNotifyId(value.ToString());
        style.setTitle(Util.GetString(278, 0));
        style.setContent(Util.GetString(278, 2));
        style.setTimestamp(time * 1000);

        m_mobPush.setMobPushLocalNotification(style);

        Logger.LogDetail("SDKManager:: Set <b><color=#00FF00>[{0}]</color></b> notification delay time to <b><color=#FFFFFF>{1}</color></b>", SwitchType.SkillPoint, time * 1000);
    }

    #endregion

    #endregion

    #region Thinking

    [Space(5)]
    [Header("Thinking")]
    [SerializeField, Set("thinkingDebug")]
    private bool m_thinkingDebug = false;

    public string thinkingAppKey = string.Empty;
    public string thinkingUrl = string.Empty;
    
    public bool thinkingDebug
    {
        get { return m_thinkingDebug; }
        set
        {
            if (m_thinkingDebug == value) return;
            m_thinkingDebug = value;
        }
    }

    private void InitializeThinking()
    {
        #if UNITY_IOS && !UNITY_EDITOR
        InitializeThinking(thinkingAppKey, thinkingUrl);
        #elif UNITY_ANDROID && !UNITY_EDITOR
        CallThinking("initThinking", thinkingAppKey, thinkingUrl);
        #endif

        #if !UNITY_EDITOR
        RegisterEventListener(OnThinkingEvent, Events.LAUNCH_PROCESS);
        #endif

        CallThinkingData("ChannelId", channelID);

        Logger.LogDetail("SDKManager:: Initialize Thinking. key: [{0}], url: [{1}]", thinkingAppKey, thinkingUrl);
    }

    #if UNITY_IOS
    [DllImport("__Internal")] private static extern void InitializeThinking(string appKey, string url);
    [DllImport("__Internal")] private static extern void TrackThinking(string eventName, string value);
    #endif

    #if UNITY_ANDROID
    public static AndroidJavaObject thinking
    {
        get
        {
            if (m_thinking == null)
            {
                try
                {
                    m_thinking = new AndroidJavaObject("com.thinkingdata.release.ThinkingManager");
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            }
            return m_thinking;
        }
    }
    private static AndroidJavaObject m_thinking = null;

    public static T CallThinking<T>(string method, params object[] args)
    {
        try
        {
            return thinking.CallStatic<T>(method, args);
        }
        catch (Exception e)
        {
            Logger.LogException(e);
        }

        return default(T);
    }

    public static void CallThinking(string method, params object[] args)
    {
        try
        {
            thinking.CallStatic(method, args);
        }
        catch (Exception e)
        {
            Logger.LogException(e);
        }
    }
    #endif

    public static void TrackThinkingData(string eventName, string value)
    {
        #if UNITY_IOS && !UNITY_EDITOR
        TrackThinking(eventName, value);    
        #elif UNITY_ANDROID && !UNITY_EDITOR
        CallThinking("trackThinking", eventName, value);
        #endif
    }

    private static void OnThinkingEvent(string eventName, Event_ e)
    {
        var process = (Launch.LaunchProcess)e.param1;

        string result = "0";
        if (process == Launch.LaunchProcess.ShowLevelTime) result = ((float)e.param2).ToString("F2");
        else result = ((int)e.param2).ToString();

        CallThinkingData(process.ToString(), result.ToString());
    }

    private static void CallThinkingData(string key, string value)
    {
        Dictionary<string, string> temp = new Dictionary<string, string>();
        temp.Set(key, value);

        var str = GetStringFormatJson(temp);
        TrackThinkingData("client_before_login", str);

        Logger.LogDetail("Update ThinkingData process:[{0}],value:[{1}]", key, value);
    }

    /// <summary>
    /// 将DIC转化成json格式的字符串
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private static string GetStringFormatJson(Dictionary<string, string> target)
    {
        if (target == null || target.Count < 1) return "";
        return MiniJSON.jsonEncode(target);
    }
    #endregion
}
