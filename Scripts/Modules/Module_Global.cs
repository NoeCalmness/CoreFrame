/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Global module
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-04-07
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// @TODO: Move to a single module
public class DropInfo
{
    public int windowType;
    public string desc;
    public int chaseId;
    public bool open;
    public string wName;
    public int lable;
}

public class Module_Global : Module<Module_Global>
{
    public static Regex factionRegex = new Regex("<faction([0-9])>");

    /// <summary>
    /// 新手引导和剧情的遮挡优先级
    /// </summary>
    public const int GUIDE_LOCK_PRIORITY = 1;
    /// <summary>
    /// 场景加载的遮挡优先级
    /// </summary>
    public const int LEVEL_LOCK_PRIORITY = 100;
    /// <summary>
    /// 锁屏转圈超时时间
    /// </summary>
    public const float LOCK_TIMEOUT = 20;

    public const string EventUILockStateChanged = "EventUILockStateChanged";
    public const string EventUIScreenFadeIn     = "EventUIScreenFadeIn";
    public const string EventUIScreenFadeOut    = "EventUIScreenFadeOut";
    public const string EventUIScreenFadeInEnd  = "EventUIScreenFadeInEnd";
    public const string EventUIScreenFadeOutEnd = "EventUIScreenFadeOutEnd";
    public const string EventBuyMatirals        = "EventBuyMatirals";

    /// <summary>
    /// 显示一个消息弹窗
    /// param1: string[4] title content ok cancle
    /// param2: buttonFlag 0x00 = default, 0x01 = Close, 0x02 = OK, 0x04 = Cancle
    /// param3: callback
    /// </summary>
    public const string EventShowMessageBox     = "EventGlobalShowMessageBox";
    /// <summary>
    /// 当 Global Layer 的显示状态更改时触发
    /// param1: 类型 0 = layer 1 = 头像和返回 2 = 右上方工具栏状态
    /// param2: 状态
    /// </summary>
    public const string EventGlobalLayerShowState = "EventGlobalLayerShowState";
    public const string EventGlobalLayerInitialize = "EventGlobalLayerInitialize";
    /// <summary>
    /// 当 Global Layer 的渲染层级更改时触发
    /// param1: 新的参考层级，新的层级总是等于 参考层级+1 当参考层级小于 0 时 还原为默认值
    /// </summary>
    public const string EventGlobalLayerIndexChanged = "EventGlobalLayerIndexChanged";
    /// <summary>
    /// 当 Global Layer 的 Tween 动画状态变化时触发
    /// param1: Tween 动画是否以 forward 方式播放  param2: 要播放的动画 0 = 全部 1 = Fade 其它 = Position
    /// </summary>
    public const string EventGlobalLayerTweenAnimation = "EventGlobalLayerTweenAnimation";
    /// <summary>
    /// 执行一个在 GlobalLayer 上的按钮点击事件
    /// param1: 按钮 ID See <see cref="HomeIcons"/>
    /// </summary>
    public const string EventGlobalLayerIconAction = "EventGlobalLayerIconAction";
    /// <summary>
    /// 显示道具分解信息
    /// param1: 被分解的道具 ID  param2: 分解得到的道具 ID  param3: 分解得到的道具数量  param4: 分解信息窗口关闭时回掉
    /// </summary>
    public const string EventShowDecomposeInfo = "EventGlobalShowDecomposeInfo";
    /// <summary>
    /// 显示一条全局通知
    /// 该通知显示在屏幕中央，并置于所有 UI 之上
    /// param1: 要显示的文本信息  param2: 停留时间  param3: 淡出时间
    /// </summary>
    public const string EventShowGlobalNotice = "EventGlobalShowGlobalNotice";
    /// <summary>
    /// 导航栏好友按钮
    /// </summary>
    public const string EventShowGlobalFriend = "EventGlobalShowGlobalFriend";
    /// <summary>
    /// 配置资源版本更新时
    /// </summary>
    public const string EventServerDataHashUpdate = "EventGlobalServerDataHashUpdate";

    public const string EventShowNotice = "EventShownotice";//公告
    public const string EventCloseShowNotice = "EventCloseShowNotice";//公告

    public const string EventShowawardmes = "EventShowawardmes";//奖励领取悬浮框

    /// <summary>
    /// 显示一条浮动提示消息
    /// param1 = message  param2 = duration  param3 = show next frame (thread safe)
    /// </summary>
    public const string EventShowMessage = "EventShowMessage";//显示悬浮框

    public const string EventScenceswitchover = "EventScenceswitchover";//当界面切换的时候
    
    public const string EventShowDropChase = "EventShowDropChase";//该物品对应的关卡掉落

    public const string EventShowRuneDtail = "EventShowRuneDtail";//符文对应的详情

    //mesbox
    public const string EventArgeeFailed = "EventArgeeFailed";//自己接受好友邀请失败
    public const string EventArgeesucced = "EventArgeesucced";//自己接受好友邀请成功
    public const string EventBeinvited = "EventBeinvited";//自己被邀请
    // public const string EventRoomKey = "EventRoomKey";//接受房间号

    /// <summary>
    /// pve在进行新的资源加载或者切换预加载场景的事件
    /// </summary>
    public const string EventPVELoadAssetLockState = "EventPVELoadAssetLockState";
    /// <summary>
    /// pve在进行新的资源加载或者切换预加载场景时，更换提示文字
    /// </summary>
    public const string EventPVEChangeLockText = "EventPVEChangeLockText";

    public const string EventShowShareTool = "EventShowShareTool";
    public const string EventHideShareTool = "EventHideShareTool";

    public PMatchInfo roleinfo;
    public sbyte p_type;

    public Queue<string> noticeStack = new Queue<string>();
    public bool noticPlay = false;
    public int noticeIndex = 0;

    private Window_Global m_globalWindow;

    #region SystemSettings

    public ScSystemSetting system { get { return m_system; } }
    private ScSystemSetting m_system = null;

    public bool IsStayLoLTime()
    {
        var now = Util.GetTimeOfDay();
        var ts = m_system.rankPvpTimes;
        for (int i = 0, c = ts.Length; i < c; i += 2) if (now >= ts[i] && now <= ts[i + 1]) return true;
        return false;
    }

    void _Packet(ScSystemSetting p)
    {
        p.CopyTo(ref m_system);
        moduleSet.spend_num = m_system.updateNamePrice;
        moduleUnion.createUnionInfo = m_system.createLeague;
    }

    #endregion

    public Queue<List<int>> _type = new Queue<List<int>>();
    public Queue<List<int>> _num = new Queue<List<int>>();
    public Queue<List<int>> _star = new Queue<List<int>>();


    #region global_tip
    public const string EventUpdateTip = "EventUpdateTip";//更新tip
    public const string EventUpdateItemTip = "EventUpdateItemTip";//更新tip
    public const string EventUpdateTimeTip = "EventUpdateTimeTip";//更新tip(仅仓库与装备界面调用)
    public const string EventUpdateExchangeTip = "EventUpdateExchangeTip";//打开兑换tip
	public const string EventSkillTip = "EventSkillTip";

    public const string EventComposeTip = "EventComposeTip";

    public PropItemInfo currentOpenInfo { get; set; }

    #endregion

    #region Item decompose

    public void ShowItemDecomposeInfo(ScEquipWeaponDecompose p, Action onHide = null)
    {
        if (p == null) return;
        ShowItemDecomposeInfo(p.weaponId, p.pieceId, p.pieceNum, onHide);
    }

    /// <summary>
    /// 显示道具分解窗口
    /// </summary>
    /// <param name="i">被分解的道具 ID</param>
    /// <param name="p">分解得到的道具 ID</param>
    /// <param name="c">分解得到的道具数量</param>
    public void ShowItemDecomposeInfo(int i, int p, int c, Action onHide = null)
    {
        DispatchModuleEvent(EventShowDecomposeInfo, i, p, c, onHide);
    }

    #endregion

    #region 邀请

    void _Packet(ScMatchInviteInfo p)
    {
        //当自己被好友邀请时候自己是否在pvp或者皇家斗技界面中

        //自己被好友邀请

        roleinfo = p.roleInfo;
        p_type = p.pvpType;

        var canshow = moduleGuide.IsActiveFunction(HomeIcons.Fight);
        if (canshow) canshow = moduleGuide.IsActiveFunction(HomeIcons.PVP);

        if (moduleMatch.isbaning && Level.current.isNormal && canshow)
        {
            DispatchModuleEvent(EventBeinvited);
        }
        else
        {
            Refuse(p.roleInfo.roleId);
        }
    }

    void _Packet(ScAgreeInvite p)
    {
        if (p.result == 0)
        {
            moduleMatch.beiInvated = true;
            Window.HideAllNonFullScreenWindows();
            DispatchModuleEvent(EventArgeesucced);
        }
        else
        {
            DispatchModuleEvent(EventArgeeFailed, p.result);
        }
    }

    //void _Packet(ScMatchInviteSuccess p)
    //{
    //    Debug.Log("进入战斗准备 被邀请者拿到room1");
    //    p.CopyTo(ref Info_sss);

    //    DispatchModuleEvent(EventRoomKey, p);
    //}

    public void Argee(ulong invateID)
    {
        var p = PacketObject.Create<CsAgreeInvite>();
        p.roleId = invateID;
        session.Send(p);

    }

    public void Refuse(ulong invateID)
    {
        var p = PacketObject.Create<CsRefuseInvite>();
        p.roleId = invateID;
        p.reason = " ";
        session.Send(p);
    }

    #endregion

    #region 浮动提示

    public void ShowMessage(string message, float duration = -1)//悬浮框的方法(1)
    {
        DispatchModuleEvent(EventShowMessage, message, duration, false);
    }

    public void ShowMessage(int text, int index = 0, float duration = -1)
    {
        DispatchModuleEvent(EventShowMessage, Util.GetString(text, index), duration, false);
    }

    public void ShowMessageFormat(int text, int index, params object[] args)
    {
        var t = ConfigManager.Get<ConfigText>(text);
        if (t == null) return;
        DispatchModuleEvent(EventShowMessage, Util.Format(t[index], args), -1, false);
    }

    public void ShowMessageFormat(int text, int index, float duration, params object[] args)
    {
        var t = ConfigManager.Get<ConfigText>(text);
        if (t == null) return;
        DispatchModuleEvent(EventShowMessage, Util.Format(t[index], args), duration, false);
    }

    public void ShowMessageNextFrame(string message, float duration = -1)
    {
        DispatchModuleEvent(EventShowMessage, message, duration, true);
    }

    public void ShowMessageNextFrame(int text, int index = 0, float duration = -1)
    {
        var t = ConfigManager.Get<ConfigText>(text);
        if (t == null) return;
        DispatchModuleEvent(EventShowMessage, t[index], duration, true);
    }

    public void ShowMessageFormatNextFrame(int text, int index, params object[] args)
    {
        var t = ConfigManager.Get<ConfigText>(text);
        if (t == null) return;
        DispatchModuleEvent(EventShowMessage, Util.Format(t[index], args), -1, true);
    }

    public void ShowMessageFormatNextFrame(int text, int index, float duration, params object[] args)
    {
        var t = ConfigManager.Get<ConfigText>(text);
        if (t == null) return;
        DispatchModuleEvent(EventShowMessage, Util.Format(t[index], args), duration, true);
    }

    public bool isEnd = true;
    public void Showawardmes(List<PItem> m_award)//奖励
    {
        List<int> a_type = new List<int>();
        List<int> a_num = new List<int>();
        List<int> a_star = new List<int>();
        for (int i = 0; i < m_award.Count; i++)
        {
            var info = m_award[i].GetPropItem();
            if (!info) continue;

            a_type.Add(m_award[i].itemTypeId);
            a_num.Add((int)m_award[i].num);

            int star = m_award[i].growAttr == null ? 0 : info.itemType == PropType.Rune ? m_award[i].growAttr.runeAttr.star :
                info.itemType == PropType.Weapon && info.subType != (int)WeaponSubType.Gun ? m_award[i].growAttr.equipAttr.star : 0;

            a_star.Add(star);
        }
        getAward(a_type, a_num, a_star);
    }

    public void ShowAwardmes(PItem2[] awards)
    {
        if (null == awards || awards.Length == 0)
            return;
        List<int> a_type = new List<int>();
        List<int> a_num = new List<int>();
        List<int> a_star = new List<int>();
        for (int i = 0; i < awards.Length; i++)
        {
            a_type.Add(awards[i].itemTypeId);
            a_num.Add((int)awards[i].num);
            var info = ConfigManager.Get<PropItemInfo>(awards[i].itemTypeId);
            if (info.itemType == PropType.Rune)
                a_star.Add(awards[i].star);
            else
                a_star.Add(0);
        }
        getAward(a_type, a_num, a_star);
    }

    public void Showawardmes(List<PItem2> m_award)//奖励
    {
        ShowAwardmes(m_award.ToArray());
    }

    public void getAward(List<int> type, List<int> num, List<int> star)
    {
        if (isEnd)
        {
            List<PropItemInfo> infos = new List<PropItemInfo>();
            for (int i = 0; i < type.Count; i++)
            {
                PropItemInfo info = ConfigManager.Get<PropItemInfo>(type[i]);
                infos.Add(info);
            }

            DispatchModuleEvent(EventShowawardmes, infos, num, star);
            isEnd = false;
        }
        else
        {
            _type.Enqueue(type);
            _num.Enqueue(num);
            _star.Enqueue(star);
        }
    }

    public string TobeString(string notice)
    {
        string replaceNotice = "";

        string[] strsplit = new string[1] { "<item" };

        string[] f = notice.Split(strsplit, StringSplitOptions.RemoveEmptyEntries);
        if (f.Length >= 2)
        {
            string[] tt = f[1].Split('>');
            int str = Util.Parse<int>(tt[0]);

            ConfigText info = ConfigManager.Get<ConfigText>(str);
            string name = info[0];

            string aa = notice.Replace("<item", null);
            string bb = aa.Replace(">", null);
            string cc = bb.Replace(tt[0], name);
            replaceNotice = cc;
        }
        else replaceNotice = notice;
        
        replaceNotice = replaceNotice.Replace("(", "<color=");
        replaceNotice = replaceNotice.Replace(")", ">");
        replaceNotice = replaceNotice.Replace("/", "</color>");

        do
        {
            var mr = factionRegex.Match(replaceNotice);
            if (!mr.Success) break;
            replaceNotice = replaceNotice.Replace(mr.Groups[0].Value,
                Module_FactionBattle.FactionName((Module_FactionBattle.Faction)Util.Parse<int>(mr.Groups[1].Value)));
        } while (true);
        return replaceNotice;
    }

    public void _Packet(ScSystemNotice p)
    {
        var enter = Level.current as Level_Login;
        if (moduleLogin.ok && enter == null)//不在登录界面
        {
            string replace = TobeString(p.notice);
            SysMes(replace, p.type);

            if (p != null && p.type == (int)SysMesType.Wish)
            {
                if (moduleSet.pushState.ContainsKey(SwitchType.SystemPao) && moduleSet.pushState[SwitchType.SystemPao] == 0)
                    return;
            }
            
            noticeIndex++;
            if (noticPlay) noticeStack.Enqueue(replace);
            else
            {
                noticPlay = true;
                DispatchModuleEvent(EventShowNotice, replace);
            }
        }
    }
    public void Hidepao()
    {
        DispatchModuleEvent(EventCloseShowNotice);
        InitializeMes();
    }
    public void DelayNext()
    {
        if (noticeStack.Count > 0)
        {
            string notice = moduleGlobal.noticeStack.Dequeue();
            DispatchModuleEvent(EventShowNotice, notice);
        }
        else noticPlay = false;
    }
    public void CloseNotice()
    {
        noticeIndex--;
        if (noticeStack.Count <= 0 && noticeIndex <= 0)
        {
            Hidepao();
        }
    }
    private void SysMes(string content,int type)
    {
        //显示到世界聊天的系统中
        ScChatRoomMessage p = PacketObject.Create<ScChatRoomMessage>();
        p.sendId = 110;
        p.type = 0;
        p.content = content;
        moduleChat.SysChatMes(p, type);
    }

    private void InitializeMes()
    {
        noticeIndex = 0;
        noticeStack.Clear();
        noticPlay = false;
    }

    #endregion

    #region Initialize

    private double m_lastTimeScale = 1.0;

    protected override void OnModuleCreated()
    {
        m_globalWindow = Window.ShowImmediately<Window_Global>(UIManager.instance.window_global) as Window_Global;
        m_globalWindow.markedGlobal = true;
        m_globalWindow.isFullScreen = false; // Ignore global hide event
        
    }

    

    protected override void OnGameDataReset()
    {
        DelayEvents.Remove(m_lockTimeOutEvent);
        DelayEvents.Remove(m_delayLockEvent);
        DelayEvents.Remove(m_fadeInOutEvent);

        m_onFadeInOut = null;

        isFadeIn = false;
        isFadeOut = false;
        currentOpenInfo = null;
        targetMatrial.Clear();
    }

    protected override void OnEnterGame()
    {
        InitializeMes();
        DispatchModuleEvent(EventGlobalLayerInitialize);
    }

    protected override void OnGamePauseResum()
    {
        if (Game.paused)
        {
            m_lastTimeScale = ObjectManager.timeScale;
            ObjectManager.timeScale = 0;
        }
        else ObjectManager.timeScale = m_lastTimeScale;
    }

    #endregion

    #region Locker

    public bool isLocked { get; private set; }

    private int m_uiLockPriority = 0;

    public string shareImagePath { get; private set; }
    public string shareText { get; private set; }

    /// <summary>
    /// 锁定 UI，并在 UI 上层显示一个载图标/黑色背景的遮罩（默认不显示）
    /// </summary>
    /// <param name="delay">锁定 UI 后延迟多久显示遮罩</param>
    /// <param name="delayUnlock">锁定 UI 后自动解除锁定延时</param>
    /// <param name="priority">优先级</param>
    /// <param name="maskType">遮罩类型 0x0 = 默认（无） 0x01 = 背景  0x02 = 图标  0x04 = 信息  0x07 = 背景/图标/信息</param>
    public void LockUI(float delay, float delayUnlock = 0, int maskType = 0, int priority = 0)
    {
        LockUI("", delay, delayUnlock, priority, maskType);
    }

    /// <summary>
    /// 锁定 UI，并在 UI 上层显示一个带有提示信息和加载图标/黑色背景的遮罩
    /// </summary>
    /// <param name="text">文本配置 ID</param>
    /// <param name="index">文本配置索引</param>
    /// <param name="delay">锁定 UI 后延迟多久显示遮罩</param>
    /// <param name="delayUnlock">锁定 UI 后自动解除锁定延时</param>
    /// <param name="priority">优先级</param>
    /// <param name="maskType">遮罩类型 0x0 = 无  0x01 = 背景  0x02 = 图标  0x04 = 信息  0x07 = 默认（背景/图标/信息）</param>
    public void LockUI(int text, int index = 0, float delay = 0.0f, float delayUnlock = 0.0f, int priority = 0)
    {
        var info = Util.GetString(text, index);

        LockUI(info, delay, delayUnlock, priority);
    }

    /// <summary>
    /// 锁定 UI，并在 UI 上层显示一个带有提示信息和加载图标/黑色背景的遮罩
    /// </summary>
    /// <param name="info">要显示的提醒文字</param>
    /// <param name="delay">锁定 UI 后延迟多久显示遮罩</param>
    /// <param name="delayUnlock">锁定 UI 后自动解除锁定延时</param>
    /// <param name="priority">优先级</param>
    /// <param name="maskType">遮罩类型 0x0 = 无  0x01 = 背景  0x02 = 图标  0x04 = 信息  0x07 = 默认（背景/图标/信息）</param>
    public void LockUI(string info = "", float delay = 0.0f, float delayUnlock = 0.0f, int priority = 0, int maskType = 0x07)
    {
        if (m_uiLockPriority > 0 && priority < m_uiLockPriority) return;

        DelayEvents.Remove(m_delayLockEvent);
        DelayEvents.Remove(m_lockTimeOutEvent);

        isLocked = true;

        Logger.LogDetail("<color=#FFFFFF>Global locker state changed to <b><color={3}>[{0}]</color></b> Priority:<b><color=#45D9FF>[{1}]</color></b> Old Priority:<b><color=#45D9FF>[{2}]</color></b></color>", isLocked, priority, m_uiLockPriority, isLocked ? "#FF1111" : "#11FF11");

        m_uiLockPriority = priority;

        DispatchModuleEvent(EventUILockStateChanged, delay <= 0, info, maskType);

        if (delay > 0)
        {
            m_delayLockEvent = DelayEvents.Add(() =>
            {
                isLocked = true;
                DispatchModuleEvent(EventUILockStateChanged, true, info, maskType);
                if (delayUnlock > 0.0f) m_delayLockEvent = DelayEvents.Add(UnLockUI, delayUnlock);
                else m_delayLockEvent = 0;
            }, delay);
        }
        else
        {
            if (delayUnlock > 0.0f) m_delayLockEvent = DelayEvents.Add(UnLockUI, delayUnlock);
            else m_delayLockEvent = 0;
        }

        if (delayUnlock + delay < LOCK_TIMEOUT)
            m_lockTimeOutEvent = DelayEvents.Add(UnlockTimeOut, LOCK_TIMEOUT + delay);
    }

    public void UnLockUI()
    {
        UnLockUI(0);
    }

    public void UnLockUI(int priority)
    {
        if (priority != LEVEL_LOCK_PRIORITY && m_uiLockPriority > 0 && priority < m_uiLockPriority) return;

        DelayEvents.Remove(m_delayLockEvent);
        DelayEvents.Remove(m_lockTimeOutEvent);

        isLocked = false;

        Logger.LogDetail("<color=#FFFFFF>Global locker state changed to <b><color={3}>[{0}]</color></b> Priority:<b><color=#45D9FF>[{1}]</color></b> Old Priority:<b><color=#45D9FF>[{2}]</color></b></color>", isLocked, priority, m_uiLockPriority, isLocked ? "#FF1111" : "#11FF11");

        m_delayLockEvent = 0;
        m_lockTimeOutEvent = 0;
        m_uiLockPriority = 0;

        DispatchModuleEvent(EventUILockStateChanged, false, null, 0);

    }
    
    private void UnlockTimeOut()
    {
        Logger.LogWarning("Global locker timed out, force unlock. Current state <b><color=#CCDDEE>[priority:{0}, isLocked:{1}]</color></b>", m_uiLockPriority, isLocked);

        m_uiLockPriority = 0;
        UnLockUI();
    }

    /// <summary>
    /// 显示PVE的黑屏遮罩
    /// </summary>
    /// <param name="info"></param>
    /// <param name="show"> 小于等于0：开始淡出 ; 1:只显示黑屏;  2:显示黑屏、文字信息、旋转图标</param>
    /// <param name="duraction"></param>
    public void SetPVELockState(string info, int show, float duraction)
    {
        DispatchModuleEvent(EventPVELoadAssetLockState, info, show, duraction);
    }

    /// <summary>
    /// 更换PVE遮罩文字
    /// 只能在已经显示遮罩后设置才会生效
    /// </summary>
    /// <param name="info"></param>
    /// <param name="duraction"></param>
    public void SetPVELockText(string info)
    {
        DispatchModuleEvent(EventPVEChangeLockText, info);
    }

    #endregion

    #region Fade inout

    public bool isFadeIn { get; private set; }
    public bool isFadeOut { get; private set; }

    private int m_lockTimeOutEvent = 0;
    private int m_delayLockEvent = 0;
    private int m_fadeInOutEvent = 0;
    private Action m_onFadeInOut = null;

    public void FadeInOutScreen(float inAlpha, float inDuration, float outAlpha, float outDuration, float stayDuration, Action onComplete = null, Action onFadeIn = null, Action onFadeOutStart = null)
    {
        var tmp = m_onFadeInOut;
        m_onFadeInOut = null;

        tmp?.Invoke();
        DelayEvents.Remove(m_fadeInOutEvent);

        isFadeIn = true;
        isFadeOut = false;

        m_onFadeInOut = onComplete;
        m_fadeInOutEvent = DelayEvents.Add(() =>           // FadeIn
        {
            onFadeIn?.Invoke();
            isFadeIn = false;

            m_fadeInOutEvent = DelayEvents.Add(() =>       // Stay
            {
                onFadeOutStart?.Invoke();
                isFadeOut = true;

                DispatchModuleEvent(EventUIScreenFadeOut, outAlpha, outDuration);

                m_fadeInOutEvent = DelayEvents.Add(() =>   // FadeOut
                {
                    isFadeOut = false;
                    m_fadeInOutEvent = 0;

                    tmp = m_onFadeInOut;
                    m_onFadeInOut = null;

                    DispatchModuleEvent(EventUIScreenFadeOutEnd);

                    tmp?.Invoke();
                }, outDuration);
            }, stayDuration);
        }, inDuration);

        DispatchModuleEvent(EventUIScreenFadeIn, inAlpha, inDuration);
    }

    public void FadeIn(float alpha, float duration, bool reset = false, Action onComplete = null)
    {
        var tmp = m_onFadeInOut;
        m_onFadeInOut = null;

        tmp?.Invoke();
        DelayEvents.Remove(m_fadeInOutEvent);

        isFadeIn = true;

        m_onFadeInOut = onComplete;
        m_fadeInOutEvent = DelayEvents.Add(() =>
        {
            isFadeIn = false;
            m_fadeInOutEvent = 0;
            tmp = m_onFadeInOut;
            m_onFadeInOut = null;

            DispatchModuleEvent(EventUIScreenFadeInEnd, reset);

            tmp?.Invoke();
        }, duration);

        DispatchModuleEvent(EventUIScreenFadeIn, alpha, duration);
    }

    public void FadeOut(float alpha, float duration, bool reset = false, Action onComplete = null)
    {
        var tmp = m_onFadeInOut;
        m_onFadeInOut = null;

        tmp?.Invoke();
        DelayEvents.Remove(m_fadeInOutEvent);

        isFadeOut = true;

        m_onFadeInOut = onComplete;
        m_fadeInOutEvent = DelayEvents.Add(() =>
        {
            isFadeOut = false;
            m_fadeInOutEvent = 0;
            tmp = m_onFadeInOut;
            m_onFadeInOut = null;

            DispatchModuleEvent(EventUIScreenFadeOutEnd, reset);

            tmp?.Invoke();
        }, duration);

        DispatchModuleEvent(EventUIScreenFadeOut, alpha, duration);
    }

    #endregion

    #region Globa window layer

    /// <summary>
    /// 左上角返回按钮点击时触发
    /// </summary>
    private Action m_onReturn = null;

    /// <summary>
    /// 增加一个返回按钮回掉
    /// </summary>
    /// <param name="handler"></param>
    public void AddReturnCallback(Action handler)
    {
        m_onReturn -= handler;
        m_onReturn += handler;
    }

    /// <summary>
    /// 移除一个返回按钮回掉
    /// </summary>
    /// <param name="handler"></param>
    public void RemoveReturnCallback(Action handler)
    {
        m_onReturn -= handler;
    }

    /// <summary>
    /// 显示左上角头像或者返回按钮  0 = 显示头像 1 = 显示返回按钮 其它 = 都不显示
    /// </summary>
    /// <param name="type">0 = 显示头像 1 = 显示返回按钮 其它 = 都不显示</param>
    public void ShowGlobalAvatarOrReturn(int type = 0)
    {
        UpdateGlobalLayerShowState(1, type);
    }

    /// <summary>
    /// 显示/隐藏右上方工具条
    /// </summary>
    /// <param name="show"></param>
    public void ShowGlobalTopRightBar(bool show = true)
    {
        UpdateGlobalLayerShowState(2, show ? 1 : 0);
    }

    /// <summary>
    /// 显示/隐藏 GlobalLayer
    /// </summary>
    /// <param name="show"></param>
    public void ShowGlobalLayer(bool show = true)
    {
        UpdateGlobalLayerShowState(0, show ? 1 : 0);
    }

    /// <summary>
    /// 显示/隐藏 GlobalLayer, 并设置头像和工具条状态
    /// </summary>
    /// <param name="avatarOrReturn">0 = 显示头像  1 = 显示返回 其它 = 都不显示</param>
    /// <param name="topRight">是否显示工具条</param>
    public void ShowGlobalLayerDefault(int avatarOrReturn = 1, bool topRight = true)
    {
        var actived = avatarOrReturn == 0 || avatarOrReturn == 1 || topRight;
        if (!actived)
        {
            ShowGlobalLayer(false);
            return;
        }

        UpdateGlobalLayerShowState(0, 1);
        ShowGlobalAvatarOrReturn(avatarOrReturn);
        ShowGlobalTopRightBar(topRight);
    }

    /// <summary>
    /// 设置 global layer 的渲染层级
    /// 小于 0 时还原到默认层级
    /// 注意，调用此方法后需要记得还原
    /// </summary>
    /// <param name="now"></param>
    public void SetGlobalLayerIndex(int now = -1)
    {
        DispatchModuleEvent(EventGlobalLayerIndexChanged, now);
    }

    /// <summary>
    /// 获取当前 Global Layer 的显示状态
    /// Mask: 0： layer 状态 1 - 2: 左边状态 3: 右边状态
    /// </summary>
    /// <returns></returns>
    public int GetGlobalLayerShowState()
    {
        return m_globalWindow.GetGlobalLayerShowState();
    }

    /// <summary>
    /// 还原通过 GetGlobalLayerShowState 获取的状态
    /// </summary>
    public void RestoreGlobalLayerState(int state)
    {
        m_globalWindow.RestoreGlobalLayerState(state);
    }

    /// <summary>
    /// 触发一个返回按钮事件
    /// </summary>
    public void OnGlobalReturnButton()
    {
        m_onReturn?.Invoke();
    }

    /// <summary>
    /// 出发一个顶部 Layer 的 Tween 动画事件
    /// </summary>
    /// <param name="forward"></param>
    /// <param name="type">0 = Fade and Position 1 = Fade other = Position</param>
    public void OnGlobalTween(bool forward = true, int type = 0)
    {
        DispatchModuleEvent(EventGlobalLayerTweenAnimation, forward, type);
    }

    /// <summary>
    /// type: 0 = layer, 1 = 头像或返回 2 = 右上方工具条
    /// </summary>
    /// <param name="type"></param>
    /// <param name="state"></param>
    private void UpdateGlobalLayerShowState(int type, int state)
    {
        DispatchModuleEvent(EventGlobalLayerShowState, type, state);
    }

    /// <summary>
    /// 执行一个在 globalLayer 上的按钮点击事件
    /// </summary>
    /// <param name="ID"></param>
    public void ExecuteIconAction(int ID)
    {
        DispatchModuleEvent(EventGlobalLayerIconAction, ID);
    }

    #endregion

    #region Global notice message

    /// <summary>
    /// 显示一条全局通知
    /// 该通知显示在屏幕中央，并置于所有 UI 之上
    /// </summary>
    /// <param name="message">要显示的消息</param>
    /// <param name="stayDuration">停留时间</param>
    /// <param name="fadeDuration">淡出时间</param>
    public void ShowGlobalNotice(int id, float stayDuration = 3.0f, float fadeDuration = 1.5f)
    {
        ShowGlobalNotice(Util.GetString(id), stayDuration, fadeDuration);
    }

    /// <summary>
    /// 显示一条全局通知
    /// 该通知显示在屏幕中央，并置于所有 UI 之上
    /// </summary>
    /// <param name="message">要显示的消息</param>
    /// <param name="stayDuration">停留时间</param>
    /// <param name="fadeDuration">淡出时间</param>
    public void ShowGlobalNotice(int id, int index, float stayDuration = 3.0f, float fadeDuration = 1.5f)
    {
        ShowGlobalNotice(Util.GetString(id, index), stayDuration, fadeDuration);
    }

    /// <summary>
    /// 显示一条全局通知
    /// 该通知显示在屏幕中央，并置于所有 UI 之上
    /// </summary>
    /// <param name="message">要显示的消息</param>
    /// <param name="stayDuration">停留时间</param>
    /// <param name="fadeDuration">淡出时间</param>
    public void ShowGlobalNotice(string message, float stayDuration = 3.0f, float fadeDuration = 1.5f)
    {
        if (string.IsNullOrEmpty(message)) return;
        DispatchModuleEvent(EventShowGlobalNotice, message, stayDuration, fadeDuration);
    }

    #endregion

    #region Global message box

    private string[] m_messageTexts = new string[] { "", "", "", "" };

    /// <summary>
    /// Show a global popup message box
    /// </summary>
    public void ShowMessageBox(int content, Action<bool> callback = null)
    {
        ShowMessageBox(content, 0, callback);
    }

    /// <summary>
    /// Show a global popup message box
    /// </summary>
    public void ShowMessageBox_(int content, int index, Action<bool> callback = null)
    {
        var t = Util.GetString(9, 2);
        var c = Util.GetString(content, index);
        var o = Util.GetString(9, 0);
        var a = Util.GetString(9, 1);

        ShowMessageBox(t, c, o, a, 0, callback);
    }

    /// <summary>
    /// Show a global popup message box
    /// </summary>
    public void ShowMessageBox(string content, Action<bool> callback = null)
    {
        var t = Util.GetString(9, 2);
        var o = Util.GetString(9, 0);
        var a = Util.GetString(9, 1);

        ShowMessageBox(t, content, o, a, 0, callback);
    }

    /// <summary>
    /// Show a global popup message box
    /// </summary>
    public void ShowMessageBox(int title, int content, Action<bool> callback = null)
    {
        var t = Util.GetString(title, 0);
        var c = Util.GetString(content, 0);
        var o = Util.GetString(9, 0);
        var a = Util.GetString(9, 1);

        ShowMessageBox(t, c, o, a, 0, callback);
    }

    /// <summary>
    /// Show a global popup message box
    /// </summary>
    public void ShowMessageBox(string title, string content, Action<bool> callback = null)
    {
        var o = Util.GetString(9, 0);
        var a = Util.GetString(9, 1);

        ShowMessageBox(title, content, o, a, 0, callback);
    }

    /// <summary>
    /// Show a global popup message box
    /// </summary>
    public void ShowMessageBox(int title, int titleIndex, int content, int contentIndex, int ok, int okIndex, int cancle, int cancleIndex, int buttonFlag, Action<bool> callback = null)
    {
        var t = Util.GetString(title,   titleIndex);
        var c = Util.GetString(content, contentIndex);
        var o = Util.GetString(ok,      okIndex);
        var a = Util.GetString(cancle,  cancleIndex);

        ShowMessageBox(t, c, o, a, buttonFlag, callback);
    }

    /// <summary>
    /// Show a global popup message box
    /// </summary>
    /// <param name="title">Title</param>
    /// <param name="content">Content text</param>
    /// <param name="buttonFlag">0x00 = default, 0x01 = Close, 0x02 = OK, 0x04 = Cancle</param>
    /// <param name="callback"></param>
    public void ShowMessageBox(string title, string content, string ok, string cancle, int buttonFlag, Action<bool> callback = null)
    {
        m_messageTexts[0] = title;
        m_messageTexts[1] = content;
        m_messageTexts[2] = ok;
        m_messageTexts[3] = cancle;

        DispatchModuleEvent(EventShowMessageBox, m_messageTexts, buttonFlag, callback);
    }

    #endregion

    #region global_tip
    
    public void UpdateComposeTip(ushort itemTypeId)//碎片的id
    {
        DispatchModuleEvent(EventComposeTip, itemTypeId);
    }

    public void UpdateGlobalTip(PItem info, bool showDrop = true)
    {
        if (info == null) return;
        var prop = PropItemInfo.Get(info.itemTypeId);
        if (prop == null) return;
        var show = ShowBtn(info.itemTypeId, prop);
        DispatchModuleEvent(EventUpdateTimeTip, info, show, showDrop);
    }

    public void UpdateGlobalTip(PItem2 info, bool showDrop = true, bool showBtn = false)
    {
        if (info == null) return;
        var prop = PropItemInfo.Get(info.itemTypeId);
        if (prop == null) return;
        if (showBtn) showBtn = ShowBtn(info.itemTypeId, prop);
        DispatchModuleEvent(EventUpdateItemTip, info, showBtn, showDrop);
    }

    public void UpdateGlobalTip(ushort itemTypeId, bool forceHideCompose = false, bool showDrop = true, int haveNum = -1)
    {
        var info = PropItemInfo.Get(itemTypeId);
        if (!info) return;

        if (itemTypeId == 601 || info.itemType == PropType.Rune)
        {
            DispatchModuleEvent(EventUpdateTip, itemTypeId, false, showDrop, haveNum);
            return;
        }

        if (forceHideCompose)
        {
            DispatchModuleEvent(EventUpdateTip, itemTypeId, false, showDrop, haveNum);
            return;
        }
        if (info.itemType == PropType.UseableProp || info.itemType == PropType.Debris || info.compose > 0)
        {
            //已获得的宠物不显示合成按钮(修改需求，要显示合成按钮。点击后提示“已经拥有此宠物，不能合成”)
            //            var PropInfo = ConfigManager.Get<Compound>(info.compose); //合成/分解信息
            //            if (PropInfo != null)
            //            {
            //                var targetinfo = ConfigManager.Get<PropItemInfo>(PropInfo.targetTypeId);
            //                if (targetinfo != null &&
            //                    targetinfo.itemType == PropType.Pet &&
            //                    modulePet.GetPet(targetinfo.ID) != null)
            //                {
            //                    DispatchModuleEvent(EventUpdateTip, itemTypeId, false);
            //                    return;
            //                }
            //            }
            DispatchModuleEvent(EventUpdateTip, itemTypeId, true, showDrop, haveNum);
        }
        else
            DispatchModuleEvent(EventUpdateTip, itemTypeId, false, showDrop, haveNum);
    }

    public void UpdateGlobalTip(Window_Exchange.ExchangeContent rContent, bool rHideCompose, int haveNum = -1)
    {
        var prop = ConfigManager.Get<PropItemInfo>(rContent.itemId);
        if (prop == null) return;
        if (rContent.ownCount < rContent.demandCount && prop.diamonds > 0)
            Window_Exchange.Show(rContent);
        else
            UpdateGlobalTip((ushort)rContent.itemId, rHideCompose, true, haveNum);
    }

    private bool ShowBtn(ushort itemTypeId, PropItemInfo info, bool forceHideCompose = false)
    {
        if (itemTypeId == 601 || info.itemType == PropType.Rune) return false;
        if (forceHideCompose) return false;
        if (info.itemType == PropType.UseableProp || info.itemType == PropType.Debris || info.compose > 0) return true;
        else return false;
    }

    public void UpdateSkillTip(PetSkill.Skill rSkill, int rLevel, EnumPetMood rMood)
    {
        DispatchModuleEvent(EventSkillTip, rSkill, rLevel, rMood);
    }
	
    public void OpenExchangeTip(TipType type, bool ischange = false)
    {
        DispatchModuleEvent(EventUpdateExchangeTip, type, ischange);
    }

    #endregion

    #region drop level

    public class TargetMatrial
    {
        public DropInfo                 dropInfo;
        public string                   windowName;
        public object                   data;
        public bool                     isFinish;
        public Action<Window, object>   onShow;
        public int                      itemId { get; private set; }
        public int                      targetCount { get; private set; }
        public bool isProcess { get; private set; }

        public int OwnCount
        {
            get
            {
                var prop = ConfigManager.Get<PropItemInfo>(itemId);
                if (null == prop)
                    return 0;
                if (prop.itemType == PropType.Currency)
                    return (int)Module_Player.instance.GetMoneyCount((CurrencySubType)prop.subType);

                return Module_Equip.instance.GetPropCount(itemId);
            }
        }

        public void Clear()
        {
            itemId = 0;
            dropInfo = null;
            data = null;
            isFinish = false;
            isProcess = false;
        }

        public void Init(int rItemId, int rTargetCount, object rData, Action<Window, object> rOnShow = null)
        {
            isFinish = false;
            isProcess = true;
            data   = rData;
            itemId = rItemId;
            onShow = rOnShow;
            targetCount = rTargetCount;
            windowName = Window.current?.name;
        }

        public void InvokeOnShow(Window rWindow)
        {
            onShow?.Invoke(rWindow, data);
        }
    }

    public TargetMatrial targetMatrial = new TargetMatrial();
    private List<PetTask> m_petTask { get { return ConfigManager.GetAll<PetTask>(); } }
    public List<DropInfo> m_dropList = new List<DropInfo>();
    private List<LabyrinthInfo> m_allMaze { get { return ConfigManager.GetAll<LabyrinthInfo>(); } }

    List<DropInfo> m_openList = new List<DropInfo>();
    List<DropInfo> m_closeList = new List<DropInfo>();

    Regex HrefRegex = new Regex(@"<size=([^>\n\s]+)>", RegexOptions.Singleline);

    public void SetTargetMatrial(int itemTypeId, int count, object data = null, bool rHideCompose = true, Action<Window, object> onShow = null, int haveNum = -1)
    {
        targetMatrial.Init(itemTypeId, count, data, onShow);
        UpdateGlobalTip(new Window_Exchange.ExchangeContent
        {
            itemId = itemTypeId,
            demandCount = count,
            ownCount = (int)modulePlayer.GetCount(itemTypeId)
        }, rHideCompose, haveNum);
    }

    public List<DropInfo> GetDropJump(int itemTypeId)
    {
        m_dropList.Clear();
        m_openList.Clear();
        m_closeList.Clear();

        bool show = true;

        PropItemInfo info = ConfigManager.Get<PropItemInfo>(itemTypeId);
        if (info == null) return m_dropList;
        for (int i = 0; i < info.dropType.Length; i++)
        {
            var openWindow = -1;
            var openLable = -1;
            if (info.dropType[i].Contains("-"))
            {
                string[] str = info.dropType[i].Split('-');
                openWindow = Util.Parse<int>(str[0]);
                openLable = Util.Parse<int>(str[1]);
            }
            else openWindow = Util.Parse<int>(info.dropType[i]);
            if (info.dropType[i] == "-1") openWindow = -1;

            if (openWindow == -1) show = false;
            DropWindow w = ConfigManager.Get<DropWindow>(openWindow);
            if (w == null) continue;
            var open = moduleGuide.IsActiveFunction(openWindow);
            if (openWindow == 1)
            {
                var index = moduleWelfare.allWeflarInfo.FindIndex(a => a.id == openLable);
                if (index == -1) open = false;
            }
            DropInfo drop = GetNewDrop(openWindow, ConfigText.GetDefalutString(w.descID), open, 0, w.windowName, openLable);

            if (drop.open) m_openList.Add(drop);
            else m_closeList.Add(drop);
        }
        
        if (show)
        {
            SetActiveList(itemTypeId);
            SetPlotList(itemTypeId);
            SetEmergencyList(itemTypeId);
            SetAawakeList(itemTypeId);
            SetNpcTaskList(itemTypeId);
            SetExperience(itemTypeId);
            GetSpriteInfo(itemTypeId);
            SeMazeList(itemTypeId);
        }

        m_dropList.AddRange(m_openList);
        m_dropList.AddRange(m_closeList);

        return m_dropList;
    }

    private void SetActiveList(int itemTypeId)// 活动 
    {
        for (int i = 0; i < moduleChase.allTasks.Count; i++)
        {
            var info = moduleChase.allTasks[i];
            var type = moduleChase.GetCurrentTaskType(info);
            if (!CanDrop(info.stageId, itemTypeId) || type != TaskType.Active) continue;
            var active = moduleChase.allActiveItems.Find(a => a.taskLv == info.level);
            if (active == null) continue;
            SetActive(info);
        }
    }

    public DropInfo SetActive(TaskInfo info, bool getDrop = true)
    {
        var wType = HomeIcons.Attack;
        var open = moduleChase.CanInCurType(TaskType.Active);
        var desc = ConfigText.GetDefalutString(204, 24);
        var str = info.name;
        str = info.name.Replace("</size>", string.Empty);
        desc += HrefRegex.Replace(str, string.Empty);

        if (open)
        {
            var IsOpen = moduleChase.allChaseTasks.Find(a => a.taskConfigInfo.ID == info.ID);
            open = IsOpen == null ? false : true;
            if (open)
            {
                if (modulePlayer.level < IsOpen.taskConfigInfo.unlockLv) open = false;
            }
        }
        DropInfo drop = GetNewDrop((int)wType, desc, open, info.ID);
        if (getDrop) SetAddList(open, drop);
        else if (!open) return null;

        return drop;
    }
    
    private void SetAawakeList(int itemTypeId)// 觉醒
    {
        for (int i = 0; i < moduleAwakeMatch.DependTaskInfoList.Count; i++)
        {
            var info = moduleAwakeMatch.DependTaskInfoList[i].Item1;
            if (!CanDrop(info.stageId, itemTypeId)) continue;
            SetAwake(info);
        }
        for (int i = 0; i < moduleAwakeMatch.CanEnterActiveList.Count; i++)
        {
            var info = moduleAwakeMatch.CanEnterActiveList[i].taskConfigInfo;
            if (!CanDrop(info.stageId, itemTypeId)) continue;
            SetAwake(info);
        }
    }
    public DropInfo SetAwake(TaskInfo info, bool getDrop = true)
    {
        var wType = 105;
        var desc = ConfigText.GetDefalutString(204, 25) + info.name;
        var open = moduleChase.CanInCurType(TaskType.Awake);

        if (open)
        {
            var IsOpen = moduleAwakeMatch.CanEnterList.Find(a => a.taskConfigInfo.ID == info.ID);
            open = IsOpen == null ? false : true;
        }
        DropInfo drop = GetNewDrop((int)wType, desc, open, info.ID);
        if (getDrop) SetAddList(open, drop);
        else if (!open) return null;
        return drop;
    }

    private void SetPlotList(int itemTypeId)//普通 困难 噩梦
    {
        for (int i = 0; i < moduleChase.allTasks.Count; i++)
        {
            var info = moduleChase.allTasks[i];
            if (!CanDrop(info.stageId, itemTypeId)) continue;
            SetDiffcut(info);
        }
    }
    public DropInfo SetDiffcut(TaskInfo info, bool getDrop = true)
    {
        var desc = "";
        var type = moduleChase.GetCurrentTaskType(info);
        var wType = 15;
        var open = moduleGuide.IsActiveFunction(HomeIcons.Attack);
        if (type == TaskType.Easy || type == TaskType.Difficult || type == TaskType.Nightmare)
        {
            if (info.isRare == 0) desc = ConfigText.GetDefalutString(204, 21);
            else if (info.isRare == 1)
            {
                open = moduleGuide.IsActiveFunction(102);
                desc = ConfigText.GetDefalutString(204, 22);
            }
            else if (info.isRare == 2)
            {
                wType = 115;
                open = moduleGuide.IsActiveFunction(115);
                desc = ConfigText.GetDefalutString(204, 23);
            }

            desc += moduleChase.GetCurrentLevelString(info.level);
            desc += info.name;
        }
        else return null;

        if (open)
        {
            var IsOpen = moduleChase.allChaseTasks.Find(a => a.taskConfigInfo.ID == info.ID);
            open = IsOpen == null ? false : true;
        }
        DropInfo drop = GetNewDrop(wType, desc, open, info.ID);
        if (getDrop) SetAddList(open, drop);
        else if (!open) return null;

        return drop;
    }

    private void SetNpcTaskList(int itemTypeId)//npc
    {
        var list = moduleNpcGaiden.GetGaidenTasks();
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null) continue;
            if (!CanDrop(list[i].stageId, itemTypeId)) continue;
            SetNpcInfo(list[i]);
        }
    }

    public DropInfo SetNpcInfo(ChaseTask info, bool getDrop = true)
    {
        if (info == null || info?.taskConfigInfo == null) return null;
        var npcInfo = moduleNpcGaiden.GetGaidenInfo((TaskType)info.taskConfigInfo.level);
        if (npcInfo == null) return null;
        var wType = 116;
        var desc =string.Format ( ConfigText.GetDefalutString(204, 41), ConfigText.GetDefalutString(npcInfo.gaidenNameId));
        desc += info.taskConfigInfo.name;

        var open = moduleGuide.IsActiveFunction(15) && moduleGuide.IsActiveFunction(116);
        if (open)
        {
            open = info.CanEnter;
            if (open) open = modulePlayer.level >= npcInfo.openLv && npcInfo.openLv > 0;
        }

        DropInfo drop = GetNewDrop(wType, desc, open, info.taskConfigInfo.ID);

        if (getDrop) SetAddList(open, drop);
        else if (!open) return null;

        return drop;
    }

    private void SetEmergencyList(int itemTypeId)//染血令 - D级通缉令 - 名字
    {
        for (int i = 0; i < moduleChase.emergencyList.Count; i++)
        {
            var info = moduleChase.emergencyList[i].taskConfigInfo;
            if (!CanDrop(info.stageId, itemTypeId)) continue;
            SetEmer(info);
        }
    }
    public DropInfo SetEmer(TaskInfo info, bool getDrop = true)
    {
        var wType = 110;
        var desc = ConfigText.GetDefalutString(204, 26);
        desc += ConfigText.GetDefalutString(296, info.level - 1);
        desc += info.name;
        var open = moduleChase.CanInCurType(TaskType.Emergency);

        if (open)
        {
            var IsOpen = moduleChase.allChaseTasks.Find(a => a.taskConfigInfo.ID == info.ID);
            open = IsOpen == null ? false : true;
            if (IsOpen == null || (modulePlayer.level < IsOpen.taskConfigInfo.unlockLv)) open = false;
        }
        DropInfo drop = GetNewDrop(wType, desc, open, info.ID);

        if (getDrop) SetAddList(open, drop);
        else if (!open) return null;

        return drop;
    }

    private void SetAddList(bool open, DropInfo drop,bool add=true)
    {
        if (!add) return;

        if (!open) m_closeList.Add(drop);
        else m_openList.Add(drop);
    }

    private void SetExperience(int itemTypeId)//精灵历练
    {
        for (int i = 0; i < m_petTask.Count; i++)
        {
            bool next = false;
            int[] award = m_petTask[i].previewRewardItemId;
            for (int j = 0; j < award.Length; j++)
                if (award[j] == itemTypeId) next = true;

            if (!next) continue;

            var desc = ConfigText.GetDefalutString(204, 27) + ConfigText.GetDefalutString(m_petTask[i].name);
            var open = moduleGuide.IsActiveFunction(103);
            if (open)
            {
                var task = modulePet.PetTasks.Find(a => a.ID == m_petTask[i].ID);
                open = task != null && !task.TimesUseUp && task.State == 0;
            }

            DropInfo drop = GetNewDrop(103, desc, open, m_petTask[i].ID);
            if (!open) m_closeList.Add(drop);
            else m_openList.Add(drop);
        }
    }

    private void GetSpriteInfo(int itemTypeId)//亚瑟夫海姆
    {
        var info = moduleChase.allTasks.Find(a => a.ID == moduleHome.LocalPetInfo.taskid);
        if (info == null || !CanDrop(info.stageId, itemTypeId)) return;
        bool open = true;
        if (moduleHome.SetState() == 0) open = false;
        open = moduleGuide.IsActiveFunction(HomeIcons.Dungeon);
        if (open) open = moduleGuide.IsActiveFunction(HomeIcons.PetCustom);
        DropInfo drop = GetNewDrop(30, ConfigText.GetDefalutString(204, 28), open, info.ID);

        if (!open) m_closeList.Add(drop);
        else m_openList.Add(drop);
    }

    private void SeMazeList(int itemTypeId)//迷宫
    {
        for (int i = 0; i < m_allMaze.Count; i++)
        {
            bool have = false;
            LabyrinthInfo.LabyrinthReward[] award = m_allMaze[i].rewards;
            for (int j = 0; j < award.Length; j++)
                if (award[j].propId == itemTypeId) have = true;

            if (!have) continue;

            var open = moduleLabyrinth.openTimeInterval == 0 ? true : false;

            var desc = ConfigText.GetDefalutString(204, 29) + m_allMaze[i].labyrinthName;
            DropInfo drop = GetNewDrop((int)HomeIcons.Labyrinth, desc, open);

            if (!open) m_closeList.Add(drop);
            else m_openList.Add(drop);
        }
    }

    private bool CanDrop(int stageId, int itemTypeId)
    {
        bool drop = false;
        StageInfo stage = ConfigManager.Get<StageInfo>(stageId);
        if (stage == null) return false;

        int[] award = stage.previewRewardItemId;

        for (int j = 0; j < award.Length; j++)
            if (award[j] == itemTypeId) drop = true;

        return drop;
    }

    public DropInfo GetNewDrop(int wType, string desc, bool open, int id = 0, string wName = "", int lable = -1)
    {
        DropInfo drop = new DropInfo();
        drop.windowType = wType;
        drop.open = open;
        drop.desc = desc;
        drop.chaseId = id;
        drop.wName = wName;
        drop.lable = lable;
        return drop;
    }

    #endregion

    #region go to drop
    public void SetGoToDrop(DropInfo info)
    {
        if (info.windowType == 115 || info.windowType == 110)//噩梦 染血令
        {
            ChaseTask chase = moduleChase.allChaseTasks.Find(a => a.taskConfigInfo.ID == info.chaseId);
            if (chase == null) return;
            moduleChase.isShowDetailPanel = true;
            moduleChase.SetTargetTask(chase);
            moduleAnnouncement.OpenWindow(15, 0);
            return;
        }
        else if (info.windowType == 105)//觉醒组队
        {
            ChaseTask chase = moduleAwakeMatch.CanEnterList.Find(a => a.taskConfigInfo.ID == info.chaseId);
            if (chase == null) return;
            if (chase.taskConfigInfo.teamType != TeamType.Single)
            {
                Window.GotoSubWindow<Window_Awaketeam>(-1, (w) => ((Window_Awaketeam)w).SetCurrentTask(chase));
                return;
            }
            else
            {
                Window.SetWindowParam<Window_Assist>(chase);
                Window.ShowAsync<Window_Assist>();
            }
        }
        else if (info.windowType == 103)//精灵历练
        {
            var IsOpen = modulePet.PetTasks.Find(a => a.ID == info.chaseId);
            if (IsOpen == null) Logger.LogError("this task not in pet task id{0} ", info.chaseId);
            else Window.GotoSubWindow<Window_Train>(0, (w) => ((Window_Train)w).EnterTask(IsOpen));
        }
        else if (info.windowType == 15)//出击里的其他
        {
            ChaseTask chase = moduleChase.allChaseTasks.Find(a => a.taskConfigInfo.ID == info.chaseId);
            if (chase == null) return;
            Window.SetWindowParam<Window_Assist>(chase);
            Window.GotoSubWindow<Window_Assist>(0);
        }
        else if (info.windowType == 116)
        {
            ChaseTask chase = moduleNpcGaiden.GetGaidenTasks().Find(a => a.taskConfigInfo.ID == info.chaseId);
            if (chase != null)
            {
                Window.SetWindowParam<Window_NpcGaiden>((int)chase.taskType, chase, Window.current.GetType() == typeof(Window_NpcGaiden));
                Window.GotoSubWindow<Window_NpcGaiden>((int)GaidenSubWindowNpc.Story);
            }
            else moduleAnnouncement.OpenWindow(info.windowType, info.lable);
        }
        else moduleAnnouncement.OpenWindow(info.windowType, info.lable);

        var window = Window.GetOpenedWindow<Window_CreateRoom>();
        if (window && window.actived) window.Hide(true);
    }
    #endregion

    #region Share

    // @TODO: Move to single window 
    public void ShowShareTool(string path, string text)
    {
        shareImagePath = path;
        shareText = text;
        DispatchModuleEvent(EventShowShareTool);
    }

    public void HideShareTool()
    {
        DispatchModuleEvent(EventHideShareTool);
    }

    
    public void SendGetShareAward()
    {
        var packet = PacketObject.Create<CsGetShareAward>();
        session.Send(packet);
    }

    void _Packet(ScGetShareAward packet)
    {
        List<ItemPair> list = new List<ItemPair>();
        var item = new ItemPair();
        item.itemId = (int)packet.itemTypeId;
        item.count = (int)packet.itemNum;
        list.Add(item);
        var active_text = ConfigText.GetDefalutString(9005, 4);
        Window_ItemTip.Show(active_text, list);
    }

    #endregion

    #region Asset hash validation

    public string serverDataHash => m_serverDataHash;
    private string m_serverDataHash = string.Empty;

    void _Packet(ScSystemVersionOrMd5Change p)
    {
        Logger.LogDetail($"Server data hash update from <b>[{m_serverDataHash}]</b> to <b>[{p.serverSourceMd5}]</b>");

        m_serverDataHash = p.serverSourceMd5;

        DispatchModuleEvent(EventServerDataHashUpdate);
    }

    public bool CheckSourceMd5(bool showAlert = true)
    {
        if (string.IsNullOrWhiteSpace(m_serverDataHash))
            return true;

        var ok = m_serverDataHash == AssetBundles.AssetManager.dataHash;

        #if UNITY_EDITOR
        ok = ok || string.IsNullOrEmpty(AssetBundles.AssetManager.dataHash);
        #endif

        Logger.LogInfo($"serverHash = {m_serverDataHash} localHash = {AssetBundles.AssetManager.dataHash}");

        if (!showAlert) return ok;
        var ct = ConfigManager.Get<ConfigText>(25);
        if (ok)
            Window_Alert.ShowAlertDefalut(ct[1], ()=> {}, null, ct[5]);
        else
            Window_Alert.ShowAlertDefalut(ct[0], () => Game.Quit(), () => { }, ct[4], ct[5]);

        return ok;
    }

    #endregion
    
}