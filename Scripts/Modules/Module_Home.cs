/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-01-02
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 主界面 UI 图标类型
/// 用于刷新主界面图标上的标记信息
/// 用于在打开主界面时模拟按钮行为
/// </summary>
public enum HomeIcons
{
    Unused      = 0,

    /// <summary>
    /// 运营活动
    /// </summary>
    Welfare     = 1,
    /// <summary>
    /// 邮件
    /// </summary>
    Mail        = 2,
    /// <summary>
    /// 公告
    /// </summary>
    Notice      = 3,
    /// <summary>
    /// 任务
    /// </summary>
    Quest       = 4,
    /// <summary>
    /// Buff ？
    /// </summary>
    Buff        = 5,

    /// <summary>
    /// 好友
    /// </summary>
    Friend      = 6,
    /// <summary>
    /// 聊天
    /// </summary>
    Chat        = 7,

    /// <summary>
    /// 属性
    /// </summary>
    Role        = 8,
    /// <summary>
    /// 装备
    /// </summary>
    Equipment   = 9,
    /// <summary>
    /// 符文
    /// </summary>
    Rune        = 10,
    /// <summary>
    /// 仓库
    /// </summary>
    Bag         = 11,

    /// <summary>
    /// 商业街
    /// </summary>
    Shop        = 12,
    /// <summary>
    /// 地下城
    /// </summary>
    Dungeon     = 13,
    /// <summary>
    /// 斗鸡
    /// </summary>
    Fight       = 14,
    /// <summary>
    /// 出击
    /// </summary>
    Attack      = 15,

    // 二级界面

    /// <summary>
    /// 训练场
    /// </summary>
    Train       = 16,
    /// <summary>
    /// 街头斗技
    /// </summary>
    PVP         = 17,
    /// <summary>
    /// 皇家斗技
    /// </summary>
    Match       = 18,

    /// <summary>
    /// 迷宫
    /// </summary>
    Labyrinth   = 19,
    /// <summary>
    /// 无主之地
    /// </summary>
    Bordlands   = 20,

    /// <summary>
    /// 锻造
    /// </summary>
    Forge       = 21,
    /// <summary>
    /// 时装商店
    /// </summary>
    FashionShop = 22,
    /// <summary>
    /// 流浪商店
    /// </summary>
    DrifterShop = 23,
    /// <summary>
    /// 许愿池
    /// </summary>
    WishingWell = 24,

    /// <summary>
    /// 宠物
    /// </summary>
    Pet         = 25,

    PetSummon   = 26,

    Skill       = 27,
    /// <summary>
    /// 觉醒
    /// </summary>
    Awake       = 28,

    Guild       = 29,
    /// <summary>
    /// 亚瑟夫海姆
    /// </summary>
    PetCustom   = 30,
    /// <summary>
    /// 约会
    /// </summary>
    Street      = 31,

    /// <summary>
    ///充值界面
    /// </summary>
    Charge     = 32,

    /// <summary>
    /// 图鉴
    /// </summary>
    Collection = 33,

    /// <summary>
    /// Npc手账
    /// </summary>
    NpcNote    = 34,
    /// <summary>
    /// 阵营战
    /// </summary>
    Faction    = 35,

    Count,
}

public class Module_Home : Module<Module_Home>
{
    #region Helper functions for Simple creature create, can be called from any level

    public const string PLAYER_OBJECT_NAME = "player";
    public const string TEAM_OBJECT_NAME = "teamplayer";

    public const string PET_OBJECT_NAME = "pet";

    public const string FIGHTING_PET_OBJECT_NAME = "FightingPet";
    public const string TEAM_PET_OBJECT_NAME = "teamFightingPet";

    public const int CREATURE_TEMPLATE = 999;

    private static List<string> m_visibleNodes = new List<string>();

    public string RankCountDown
    {
        get
        {
            var ts = moduleGlobal.system.rankPvpTimes;
            var t = Util.GetTimeOfDay();
            for (int i = 0, c = ts.Length; i < c; i += 2)
            {
                if (t >= ts[i] && t <= ts[i + 1])
                    return Util.GetTimeFromSec((int)(ts[i+1] - t), ":");
            }
            return string.Empty;
        }
    }

    /// <summary>
    /// 是否显示约会npc
    /// true:显示约会npc    false:显示主角
    /// </summary>
    public bool showDatingModel { private set; get; } = false;

    #region Player initialize data

    private Vector3 m_playerPosition;
    private Vector3 m_playerRotation;
    private Vector3 m_playerScale;

    #endregion

    /// <summary>
    /// Create a player creature by default.
    /// Create at start pos, forward and use Layer.Character
    /// </summary>
    /// <returns></returns>
    public Creature CreatePlayer(bool buff = false, bool skill = false)
    {
        return CreatePlayer(new Vector3(0, 0, 0), CreatureDirection.FORWARD, buff, skill);
    }

    /// <summary>
    /// Create a player creature at position pos and set direction to direction.
    /// </summary>
    /// <returns></returns>
    public Creature CreatePlayer(Vector3 pos, CreatureDirection direction, bool buff = false, bool skill = false)
    {
        var node = Level.current.startPos;

        var info = modulePlayer.BuildPlayerInfo(-1, buff, skill);
        var player = Creature.Create(info, pos, true, PLAYER_OBJECT_NAME, modulePlayer.name_);

        player.roleId = modulePlayer.id_;
        player.roleProto = modulePlayer.proto;
        player.direction = direction;

        Util.AddChild(node, player.transform, true);

        CharacterEquip.ChangeCloth(player, moduleEquip.currentDressClothes);

        m_playerPosition = player.position;
        m_playerRotation = player.eulerAngles;
        m_playerScale    = player.localScale;

        var visible = ChildIsVisible(player.name);
        player.gameObject.SetActive(visible);

        if (visible) Level.SetDOFFocusTarget(player.transform);

        return player;
    }

    /// <summary>
    /// Create a creature at position pos and set direction to direction from PvP match info.
    /// </summary>
    /// <returns></returns>
    public Creature CreatePlayer(PMatchInfo pi, Vector3 pos, CreatureDirection direction, bool buff = false, bool skill = false)
    {
        var node = Level.current.startPos;

        var info = modulePlayer.BuildPlayerInfo(pi, buff, skill);
        var creature = Creature.Create(info, pos, false, pi.roleId + ":" + pi.roleName);

        creature.roleId = pi.roleId;
        creature.roleProto = pi.roleProto;
        creature.direction = direction;

        Util.AddChild(node, creature.transform, true);

        CharacterEquip.ChangeCloth(creature, pi.fashion);

        return creature;
    }

    public Creature CreatePlayer(PMatchProcessInfo pi, Vector3 pos, CreatureDirection direction, bool buff = false, bool skill = false)
    {
        var node = Level.current.startPos;

        var info = modulePlayer.BuildPlayerInfo(pi, buff, skill);
        var creature = Creature.Create(info, pos, false, pi.roleId + ":" + pi.roleName);

        creature.roleId = pi.roleId;
        creature.roleProto = pi.roleProto;
        creature.direction = direction;

        Util.AddChild(node, creature.transform, true);

        CharacterEquip.ChangeCloth(creature, pi.fashion);

        return creature;
    }

    /// <summary>
    /// Create a creature at position pos and set direction to direction from creature info.
    /// </summary>
    /// <returns></returns>
    public Creature Create(CreatureInfo info, Vector3 pos, CreatureDirection direction)
    {
        var node = Level.current.startPos;

        var creature = Creature.Create(info, pos, false, info.name);

        creature.direction = direction;

        Util.AddChild(node, creature.transform, true);

        return creature;
    }

    public Creature CreateNpc(int npcInfoId, Vector3 pos, Vector3 rot, string name)
    {
        return CreateNpc(npcInfoId, pos, rot, Level.current.startPos, name);
    }

    public Creature CreateNpc(int npcInfoId, Vector3 pos, Vector3 rot, Transform node, string name = "")
    {
        var npcInfo = ConfigManager.Get<NpcInfo>(npcInfoId);
        if (npcInfo == null) return null;
        var i = ConfigManager.Get<CreatureInfo>(CREATURE_TEMPLATE).Clone<CreatureInfo>();

        i.models = new string[] { npcInfo.mode };
        i.weaponID = npcInfo.stateMachine;
        i.weaponItemID = 0;
        i.offWeaponItemID = 0;
        i.gender = 0;

        var npc = Creature.Create(i, pos, false, name, name);
        if (!npc) return null;

        npc.transform.localEulerAngles = rot;

        var wt = npc.transform.Find("model/weapon");
        if (wt) wt.gameObject.SetActive(false);

        if (npcInfo.type == 1)
        {
            var _npc = moduleNpc.GetTargetNpc((NpcTypeID)npcInfoId);
            if (_npc != null)
            {
                CharacterEquip.ChangeNpcFashion(npc, _npc.mode);
                _npc.UpdateCurCreature(npc);
            }
        }
        Util.AddChild(node, npc.transform, true);

        DispatchEvent(EventLoadNpc, Event_.Pop(npc));

        return npc;
    }

    public Creature CreatePet(PetUpGradeInfo.UpGradeInfo rGradeInfo, string name, bool fighting)
    {
        return CreatePet(rGradeInfo, Vector3.zero, new Vector3(0, 180, 0), Level.current.startPos, fighting, name);
    }

    public Creature CreatePet(PetUpGradeInfo.UpGradeInfo rGradeInfo, Vector3 pos, Vector3 rot, Transform node, bool fighting, string name = "")
    {
        if (rGradeInfo == null) return null;

        var i = ConfigManager.Get<CreatureInfo>(CREATURE_TEMPLATE).Clone<CreatureInfo>();

        i.models = new string[] { fighting ? rGradeInfo.fightMode : rGradeInfo.mode };
        i.weaponID = fighting ? rGradeInfo.UIstateMachine : rGradeInfo.stateMachine;
        i.weaponItemID = 0;
        i.offWeaponItemID = 0;
        i.gender = 0;
        i.colliderSize = Vector2_.zero;
        i.hitColliderSize = Vector2_.zero;

        var pet = Creature.Create(i, pos, rot, false, name, name, false, false);

        if (pet == null)
            return null;

        var wt = pet.transform.Find("model/weapon");
        if (wt) wt.gameObject.SetActive(false);

        Util.AddChild(node, pet.transform, true);

        pet.gameObject.SetActive(ChildIsVisible(pet.name));

        return pet;
    }

    /// <summary>
    /// Child created ?
    /// </summary>
    /// <param name="childName"></param>
    /// <returns></returns>
    public bool HasChild(string childName)
    {
        return Util.FindSelfChild(Level.current.startPos, childName);
    }

    /// <summary>
    /// Find a created child
    /// </summary>
    /// <param name="childName"></param>
    /// <returns></returns>
    public Transform FindChild(string childName)
    {
        return Util.FindSelfChild(Level.current.startPos, childName);
    }

    /// <summary>
    /// Is child node visible ?
    /// </summary>
    /// <param name="child"></param>
    /// <returns></returns>
    public bool ChildIsVisible(string child)
    {
        if (string.IsNullOrEmpty(child)) return false;
        return m_visibleNodes.Contains(child);
    }

    /// <summary>
    /// Hide others character
    /// </summary>
    public void HideOthers(string[] excludes = null)
    {
        m_visibleNodes.Clear();
        if (excludes != null) m_visibleNodes.AddRange(excludes);

        Util.DisableAllChildren(Level.current.startPos, excludes);

        if (ChildIsVisible(PLAYER_OBJECT_NAME))
        {
            var p = FindChild(PLAYER_OBJECT_NAME);
            if (p) Level.SetDOFFocusTarget(p);

            return;
        }

        foreach (var v in m_visibleNodes)
        {
            var n = FindChild(v);
            if (!n) continue;

            Level.SetDOFFocusTarget(n);
            break;
        }
    }

    /// <summary>
    /// Hide others character
    /// </summary>
    public void HideOthers(string exclude)
    {
        m_visibleNodes.Clear();
        m_visibleNodes.Add(exclude);

        Util.DisableAllChildren(Level.current.startPos, exclude);

        var n = FindChild(exclude);
        if (n) Level.SetDOFFocusTarget(n);
    }

    /// <summary>
    /// Hide all characters not in excludes
    /// </summary>
    /// <param name="excludes"></param>
    public void HideOthersBut(params string[] excludes)
    {
        HideOthers(excludes);
    }

    /// <summary>
    /// Reset main camera to its default state (only avaliable in main level)
    /// </summary>
    public void ResetCamera()
    {
        var level = Level.current;
        if (!level || level.levelID != GeneralConfigInfo.shomeLevel) return;

        level.ResetMainCamera();

        ResetPlayer();
    }

    /// <summary>
    /// Reset player state to create state
    /// </summary>
    public void ResetPlayer()
    {
        var player = ObjectManager.FindObject<Creature>(c => c.isPlayer);
        if (player)
        {
            player.position         = player.position_   = m_playerPosition;
            player.localEulerAngles = m_playerRotation;
            player.localScale       = m_playerScale;

            DispatchEvent(EventRevertMasterPosition);

            Level.SetDOFFocusTarget(player.transform);
        }

        var pet = FindChild(FIGHTING_PET_OBJECT_NAME);
        if(pet) pet.gameObject.SetActive(true);
    }

    #endregion

    #region Events

    public const string EventSwitchCameraMode      = "SwitchCameraMode";
    public const string EventSummonSuccess         = "EventSummonSuccess";
    public const string EventLoadNpc               = "EventLoadNpc";
    public const string EventCloseSubWindow        = "EventCloseSubWindow";
    public const string EventSetMasterPosition     = "EventSetMasterPosition";
    public const string EventRevertMasterPosition  = "EventRevertMasterPosition";
    public const string EventSummon                = "EventSummon";
    public const string EventSetTexture            = "EventSetTexture";
    public const string EventSetScene              = "EventSetScene";
    public const string EventUpdateMesh            = "EventUpdateMesh";
    public const string EventInterruptSummonEffect = "EventHomeInterruptSummonEffect";
    /// <summary>
    /// 当主界面图标的红点状态更改时触发
    /// param1 = HomeIcons  param2 = index  param3 = state
    /// </summary>
    public const string EventIconState = "EventHomeIconState";
    /// <summary>
    /// 当在约会状态下，主界面点击模型切换按钮时触发该事件
    /// </summary>
    public const string EventSwitchDatingModel     = "EventHomeSwitchDatingModel";
    #endregion

    #region Window_Home icon public interface

    /// <summary>
    /// 获取主界面所有图标的标记 0 状态
    /// </summary>
    public int iconMask0 { get { return m_iconMask0; } }
    /// <summary>
    /// 获取主界面所有图标的标记 1 状态
    /// </summary>
    public int iconMask1 { get { return m_iconMask1; } }

    /// <summary>
    /// 主界面图标标记 0 状态
    /// </summary>
    private int m_iconMask0 = 0;
    /// <summary>
    /// 主界面图标标记 1 状态
    /// </summary>
    private int m_iconMask1 = 0;

    /// <summary>
    /// 更新主界面图标的标记状态 若 index != 0 && index != 1 表示更改全部标记状态
    /// </summary>
    /// <param name="icon">图标类型</param>
    /// <param name="mark">标记是否可见？</param>
    /// <param name="index">0 或 1 一个图标可以包含两个状态</param>
    public void UpdateIconState(HomeIcons icon, bool mark, int index = 0)
    {
        if (icon < HomeIcons.Welfare || icon >= HomeIcons.Count)
        {
            Logger.LogWarning("Module_Home::UpdateIconState: Unknow icon type {0}", icon);
            return;
        }

        var id = (int)icon;

        if (index == 0 || index != 1)
        {
            var old = m_iconMask0.BitMask(id);
            m_iconMask0 = m_iconMask0.BitMask(id, mark);
            if (old ^ mark) DispatchModuleEvent(EventIconState, icon, 0, mark);
        }

        if (index == 1 || index != 0)
        {
            var old = m_iconMask1.BitMask(id);
            m_iconMask1 = m_iconMask1.BitMask(id, mark);
            if (old ^ mark) DispatchModuleEvent(EventIconState, icon, 1, mark);
        }
    }
    /// <summary>
    /// 获取指定图标的标记状态
    /// </summary>
    /// <param name="icon">图标类型</param>
    /// <param name="index">标记位置 0 或 1 一个图标可以包含两个标记</param>
    /// <returns></returns>
    public bool GetIconState(HomeIcons icon, int index = 0)
    {
        if (icon < HomeIcons.Welfare || icon >= HomeIcons.Count) return false;

        return index == 0 ? m_iconMask0.BitMask((int)icon) : index == 1 ? m_iconMask1.BitMask((int)icon) : false;
    }

    /// <summary>
    /// 获取指定图标的标记状态
    /// </summary>
    /// <param name="icon">图标类型</param>
    /// <param name="index">标记位置 0 或 1 一个图标可以包含两个标记</param>
    /// <returns></returns>
    public bool GetIconState(int icon, int index = 0)
    {
        if (icon < 1 || icon >= (int)HomeIcons.Count) return false;

        return index == 0 ? m_iconMask0.BitMask(icon) : index == 1 ? m_iconMask1.BitMask(icon) : false;
    }

    public int windowPanel => m_windowPanel;
    public HomeIcons windowIcon => m_windowIcon;

    private int m_windowPanel;
    private HomeIcons m_windowIcon;

    public void SetWindowPanelAndIcon(int panel, HomeIcons icon)
    {
        m_windowPanel = panel;
        m_windowIcon = icon;

        Logger.LogDetail($"Module_Home::SetWindowPanelAndIcon: <color=#00FF00><b>[Panel:{m_windowPanel}, Icon:{m_windowIcon}]</b></color>");
    }

    protected override void OnGameDataReset()
    {
        showDatingModel = false;

        SetWindowPanelAndIcon(Window_Home.Main, HomeIcons.Unused);

        ClearWindowStackCache();
    }

    /// <summary>
    /// 缓存的窗口堆栈，该列表在下一次主场景加载时恢复
    /// <para></para>
    /// <see cref="Level_Home.OnRestoreWindowStack(List{Window.WindowHolder})"/>
    /// <seealso cref="Level_Home.OnClearWindowStack(List{Window.WindowHolder})"/>
    /// <seealso cref="PushWindowStack(string)"/>
    /// </summary>
    public List<Window.WindowHolder> cachedWindowStack => m_cachedWindowStack;
    private List<Window.WindowHolder> m_cachedWindowStack = new List<Window.WindowHolder>();

    /// <summary>
    /// 将一个窗口添加到下一次主城加载后的默认窗口堆栈中
    /// 该窗口将在下一次主城加载时强制优先显示，除非调用了 <see cref="ClearWindowStackCache"/>
    /// <para></para>
    /// <see cref="Level_Home.OnRestoreWindowStack(List{Window.WindowHolder})"/>
    /// <seealso cref="Level_Home.OnClearWindowStack(List{Window.WindowHolder})"/>
    /// <seealso cref="PushWindowStack(string)"/>
    /// </summary>
    /// <param name="name">演增加的窗口名称</param>
    public void PushWindowStack(string name)
    {
        m_cachedWindowStack.RemoveAll(w => w.windowName == name, true);
        m_cachedWindowStack.Insert(0, Window.WindowHolder.Create(name));
    }

    /// <summary>
    /// 将一个窗口从窗口缓存中移除
    /// </summary>
    /// <param name="name"></param>
    public void RemoveWindowStack(string name)
    {
        m_cachedWindowStack.RemoveAll(w => w.windowName == name, true);
    }

    /// <summary>
    /// 移除所有窗口堆栈缓存
    /// </summary>
    public void ClearWindowStackCache()
    {
        m_cachedWindowStack.Clear(true);
    }

    #endregion

    #region sprite tip

    public const string EventAllPetTaskInfo = "EventAllPetTaskInfo";
    public const string EventPetProgressNewValue = "EventPetProgressNewValue";

    public const string EventPetOpenSucced = "EventPetOpenSucced";


    public bool PetTaskopen { get; set; }//当前是否有精灵任务进行中
    public bool m_showBossTip { get; set; }//是否到达boss关卡
    
    public ScPetCopyinfo  LocalPetInfo { get; set; }

    public string CloseKey()
    {
        return "TipCloseTime" + modulePlayer.roleInfo.roleId;
    }

    public int SetState()
    {
        int state = 0;
        //0 今日次数已经用完 关闭 1 当前进度值为零 高亮 2 进度值不为零 有进度的按钮

        //times  今日剩余
        if (LocalPetInfo.times <= 0 && !PetTaskopen) state = 0;
        else
        {
            //当未开启或者进度值为0时为高亮
            if (LocalPetInfo.progress == 0 || !PetTaskopen) state = 1;
            else state = 2;
        }
        return state;
    }

    public void Entermodule(short taskid)//发送进入pve请求
    {
        moduleChase.isPetLevelTask = true;
        CsChaseTaskStart p = PacketObject.Create<CsChaseTaskStart>();
        p.taskId = (ushort)taskid;
        session.Send(p);
    }
    void _Packet(ScChaseTaskStart chaseTasks)
    {
        if (chaseTasks.result == 6)
        {
            Logger.LogError("this pet task not open");
        }
        else if (moduleChase.isPetLevelTask && chaseTasks.result == 0)
        {
            AudioManager.PlaySound(AudioInLogicInfo.audioConst.enterStageSucc);
            modulePVE.OnScRoleStartChase(chaseTasks, false);
        }
    }

    private int SetTopTimes()
    {
        var top = 0;
        if (LocalPetInfo == null) return 0;
        var task = ConfigManager.Get<TaskInfo>(LocalPetInfo.taskid);
        if (task != null) top = task.dayRemainCount - 1;
        return top;
    }

    public void GetAllPettaskInfo()//获得该精灵任务所有关卡
    {
        CsPetCopyinfo p = PacketObject.Create<CsPetCopyinfo>();
        session.Send(p);
    }
    void _Packet(ScPetCopyinfo p)
    {
        LocalPetInfo = p.Clone();
        PetTaskopen = false;
        if (LocalPetInfo.open == 1)
        {
            PetTaskopen = true;
        }
        m_showBossTip = false;
        if (LocalPetInfo.progress >=100)
        {
            m_showBossTip = true;
        }
        DispatchModuleEvent(EventAllPetTaskInfo);
    }

    void _Packet(ScPetProgessValue p)//进度值更改
    {
        if (p.response == 1) return;
        sbyte newvalue = p.newvalue;
        LocalPetInfo.progress = newvalue;
        LocalPetInfo.taskid = p.taskid;
        m_showBossTip = false;
        if (newvalue >= 100)
        {
            m_showBossTip = true;
            //该开启boss关卡
        }
        else if (newvalue == 0)
        {
            PetTaskopen = false;
            LocalPetInfo.progress = 0;
            if (LocalPetInfo.times <= 0)
            {
                string CloseTime = Util.GetServerLocalTime().ToString();
                Logger.LogDetail(CloseTime + "toady can not enter pet task");
                PlayerPrefs.SetString(CloseKey(), CloseTime);
            }
        }
        DispatchModuleEvent(EventPetProgressNewValue, newvalue);
    }

    public void Opentask()//请求开启
    {
        CsPetCopyopen p = PacketObject.Create<CsPetCopyopen>();
        session.Send(p);
    }

    void _Packet(ScPetCopyopen p)
    {
        int result = p.result;
        if (result == 0)
        {
            PetTaskopen = true;
            DispatchModuleEvent(EventPetOpenSucced);
        }
        else if (result == 2) moduleGlobal.ShowMessage(239, 6);
        else if (result == 3) moduleGlobal.ShowMessage(239, 7);
        else if (result == 4) Logger.LogError("this is open");
        else Logger.LogError("have somthing unknown error");
    }

    #endregion

    #region switch dating Model
    public void SwitchDatingModel(bool bShowDatingNpc)
    {
        showDatingModel = bShowDatingNpc;
    }

    public void DispatchSwitchDatingModel()
    {
        DispatchModuleEvent(EventSwitchDatingModel, showDatingModel);
    }
    #endregion

    #region Banner
    
    public const string EventBannerInfo = "EventBannerInfo";
    /// <summary>
    /// 滑动条信息
    /// </summary>
    public List<BannerInfo> bannnerList = new List<BannerInfo>();
    List<BannerInfo> m_allBannnerList = new List<BannerInfo>();// 所有bannner信息

    private void GetAllBannerInfo()
    {
        WebRequestHelper.GetBanner(reply =>
       {
           if (reply.code == 0)
           {
               m_allBannnerList = reply.data;
               GetOpenBanner();
               DispatchModuleEvent(EventBannerInfo);
           }
       });
    }

    public void GetOpenBanner()
    {
        bool change = false;
        for (int i = 0; i < m_allBannnerList.Count; i++)
        {
            var have = bannnerList.Find(a => a?.id == m_allBannnerList[i].id);

            if (have != null)
            {
                if (m_allBannnerList[i].level > modulePlayer.level || Util.GetTimeStamp() < m_allBannnerList[i].startTime || Util.GetTimeStamp() >= m_allBannnerList[i].endTime)
                {
                    change = true;
                    bannnerList.Remove(have);
                }
            }
            else if (modulePlayer.level >= m_allBannnerList[i].level && Util.GetTimeStamp() >= m_allBannnerList[i].startTime && Util.GetTimeStamp() < m_allBannnerList[i].endTime)
            {
                change = true;
                bannnerList.Add(m_allBannnerList[i]);
            }
        }
        if (change) DispatchModuleEvent(EventBannerInfo);
    }

    #endregion

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        GetAllPettaskInfo();
        m_allBannnerList.Clear();
        bannnerList.Clear();
        GetAllBannerInfo();
    }

}

public class BannerInfo
{
    public int id;
    public string remark;     //描述
    public int level;         //限制等级
    public string picture;    //图片
    public int sort;          //优先级
    public long startTime;
    public long endTime;
    public string turnPage;
}
