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
/// ������ UI ͼ������
/// ����ˢ��������ͼ���ϵı����Ϣ
/// �����ڴ�������ʱģ�ⰴť��Ϊ
/// </summary>
public enum HomeIcons
{
    Unused      = 0,

    /// <summary>
    /// ��Ӫ�
    /// </summary>
    Welfare     = 1,
    /// <summary>
    /// �ʼ�
    /// </summary>
    Mail        = 2,
    /// <summary>
    /// ����
    /// </summary>
    Notice      = 3,
    /// <summary>
    /// ����
    /// </summary>
    Quest       = 4,
    /// <summary>
    /// Buff ��
    /// </summary>
    Buff        = 5,

    /// <summary>
    /// ����
    /// </summary>
    Friend      = 6,
    /// <summary>
    /// ����
    /// </summary>
    Chat        = 7,

    /// <summary>
    /// ����
    /// </summary>
    Role        = 8,
    /// <summary>
    /// װ��
    /// </summary>
    Equipment   = 9,
    /// <summary>
    /// ����
    /// </summary>
    Rune        = 10,
    /// <summary>
    /// �ֿ�
    /// </summary>
    Bag         = 11,

    /// <summary>
    /// ��ҵ��
    /// </summary>
    Shop        = 12,
    /// <summary>
    /// ���³�
    /// </summary>
    Dungeon     = 13,
    /// <summary>
    /// ����
    /// </summary>
    Fight       = 14,
    /// <summary>
    /// ����
    /// </summary>
    Attack      = 15,

    // ��������

    /// <summary>
    /// ѵ����
    /// </summary>
    Train       = 16,
    /// <summary>
    /// ��ͷ����
    /// </summary>
    PVP         = 17,
    /// <summary>
    /// �ʼҶ���
    /// </summary>
    Match       = 18,

    /// <summary>
    /// �Թ�
    /// </summary>
    Labyrinth   = 19,
    /// <summary>
    /// ����֮��
    /// </summary>
    Bordlands   = 20,

    /// <summary>
    /// ����
    /// </summary>
    Forge       = 21,
    /// <summary>
    /// ʱװ�̵�
    /// </summary>
    FashionShop = 22,
    /// <summary>
    /// �����̵�
    /// </summary>
    DrifterShop = 23,
    /// <summary>
    /// ��Ը��
    /// </summary>
    WishingWell = 24,

    /// <summary>
    /// ����
    /// </summary>
    Pet         = 25,

    PetSummon   = 26,

    Skill       = 27,
    /// <summary>
    /// ����
    /// </summary>
    Awake       = 28,

    Guild       = 29,
    /// <summary>
    /// ��ɪ��ķ
    /// </summary>
    PetCustom   = 30,
    /// <summary>
    /// Լ��
    /// </summary>
    Street      = 31,

    /// <summary>
    ///��ֵ����
    /// </summary>
    Charge     = 32,

    /// <summary>
    /// ͼ��
    /// </summary>
    Collection = 33,

    /// <summary>
    /// Npc����
    /// </summary>
    NpcNote    = 34,
    /// <summary>
    /// ��Ӫս
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
    /// �Ƿ���ʾԼ��npc
    /// true:��ʾԼ��npc    false:��ʾ����
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
    /// ��������ͼ��ĺ��״̬����ʱ����
    /// param1 = HomeIcons  param2 = index  param3 = state
    /// </summary>
    public const string EventIconState = "EventHomeIconState";
    /// <summary>
    /// ����Լ��״̬�£���������ģ���л���ťʱ�������¼�
    /// </summary>
    public const string EventSwitchDatingModel     = "EventHomeSwitchDatingModel";
    #endregion

    #region Window_Home icon public interface

    /// <summary>
    /// ��ȡ����������ͼ��ı�� 0 ״̬
    /// </summary>
    public int iconMask0 { get { return m_iconMask0; } }
    /// <summary>
    /// ��ȡ����������ͼ��ı�� 1 ״̬
    /// </summary>
    public int iconMask1 { get { return m_iconMask1; } }

    /// <summary>
    /// ������ͼ���� 0 ״̬
    /// </summary>
    private int m_iconMask0 = 0;
    /// <summary>
    /// ������ͼ���� 1 ״̬
    /// </summary>
    private int m_iconMask1 = 0;

    /// <summary>
    /// ����������ͼ��ı��״̬ �� index != 0 && index != 1 ��ʾ����ȫ�����״̬
    /// </summary>
    /// <param name="icon">ͼ������</param>
    /// <param name="mark">����Ƿ�ɼ���</param>
    /// <param name="index">0 �� 1 һ��ͼ����԰�������״̬</param>
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
    /// ��ȡָ��ͼ��ı��״̬
    /// </summary>
    /// <param name="icon">ͼ������</param>
    /// <param name="index">���λ�� 0 �� 1 һ��ͼ����԰����������</param>
    /// <returns></returns>
    public bool GetIconState(HomeIcons icon, int index = 0)
    {
        if (icon < HomeIcons.Welfare || icon >= HomeIcons.Count) return false;

        return index == 0 ? m_iconMask0.BitMask((int)icon) : index == 1 ? m_iconMask1.BitMask((int)icon) : false;
    }

    /// <summary>
    /// ��ȡָ��ͼ��ı��״̬
    /// </summary>
    /// <param name="icon">ͼ������</param>
    /// <param name="index">���λ�� 0 �� 1 һ��ͼ����԰����������</param>
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
    /// ����Ĵ��ڶ�ջ�����б�����һ������������ʱ�ָ�
    /// <para></para>
    /// <see cref="Level_Home.OnRestoreWindowStack(List{Window.WindowHolder})"/>
    /// <seealso cref="Level_Home.OnClearWindowStack(List{Window.WindowHolder})"/>
    /// <seealso cref="PushWindowStack(string)"/>
    /// </summary>
    public List<Window.WindowHolder> cachedWindowStack => m_cachedWindowStack;
    private List<Window.WindowHolder> m_cachedWindowStack = new List<Window.WindowHolder>();

    /// <summary>
    /// ��һ��������ӵ���һ�����Ǽ��غ��Ĭ�ϴ��ڶ�ջ��
    /// �ô��ڽ�����һ�����Ǽ���ʱǿ��������ʾ�����ǵ����� <see cref="ClearWindowStackCache"/>
    /// <para></para>
    /// <see cref="Level_Home.OnRestoreWindowStack(List{Window.WindowHolder})"/>
    /// <seealso cref="Level_Home.OnClearWindowStack(List{Window.WindowHolder})"/>
    /// <seealso cref="PushWindowStack(string)"/>
    /// </summary>
    /// <param name="name">�����ӵĴ�������</param>
    public void PushWindowStack(string name)
    {
        m_cachedWindowStack.RemoveAll(w => w.windowName == name, true);
        m_cachedWindowStack.Insert(0, Window.WindowHolder.Create(name));
    }

    /// <summary>
    /// ��һ�����ڴӴ��ڻ������Ƴ�
    /// </summary>
    /// <param name="name"></param>
    public void RemoveWindowStack(string name)
    {
        m_cachedWindowStack.RemoveAll(w => w.windowName == name, true);
    }

    /// <summary>
    /// �Ƴ����д��ڶ�ջ����
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


    public bool PetTaskopen { get; set; }//��ǰ�Ƿ��о������������
    public bool m_showBossTip { get; set; }//�Ƿ񵽴�boss�ؿ�
    
    public ScPetCopyinfo  LocalPetInfo { get; set; }

    public string CloseKey()
    {
        return "TipCloseTime" + modulePlayer.roleInfo.roleId;
    }

    public int SetState()
    {
        int state = 0;
        //0 ���մ����Ѿ����� �ر� 1 ��ǰ����ֵΪ�� ���� 2 ����ֵ��Ϊ�� �н��ȵİ�ť

        //times  ����ʣ��
        if (LocalPetInfo.times <= 0 && !PetTaskopen) state = 0;
        else
        {
            //��δ�������߽���ֵΪ0ʱΪ����
            if (LocalPetInfo.progress == 0 || !PetTaskopen) state = 1;
            else state = 2;
        }
        return state;
    }

    public void Entermodule(short taskid)//���ͽ���pve����
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

    public void GetAllPettaskInfo()//��øþ����������йؿ�
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

    void _Packet(ScPetProgessValue p)//����ֵ����
    {
        if (p.response == 1) return;
        sbyte newvalue = p.newvalue;
        LocalPetInfo.progress = newvalue;
        LocalPetInfo.taskid = p.taskid;
        m_showBossTip = false;
        if (newvalue >= 100)
        {
            m_showBossTip = true;
            //�ÿ���boss�ؿ�
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

    public void Opentask()//������
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
    /// ��������Ϣ
    /// </summary>
    public List<BannerInfo> bannnerList = new List<BannerInfo>();
    List<BannerInfo> m_allBannnerList = new List<BannerInfo>();// ����bannner��Ϣ

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
    public string remark;     //����
    public int level;         //���Ƶȼ�
    public string picture;    //ͼƬ
    public int sort;          //���ȼ�
    public long startTime;
    public long endTime;
    public string turnPage;
}
