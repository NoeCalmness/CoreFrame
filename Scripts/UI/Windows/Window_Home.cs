/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Home Window Class
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-12-29
 * 
 ***************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MarkableIcon : PoolObject<MarkableIcon>
{
    public static MarkableIcon Create(int ID, Button button, UnityEngine.Events.UnityAction onClick = null)
    {
        var btn = Create();
        btn.Update(ID, button, onClick);
        return btn;
    }

    public static void UpdateIconState(MarkableIcon icon, int index, bool state)
    {
        if (!icon) return;

        if (index == 0 || index != 1) icon.UpdateMark0(state);
        if (index == 1 || index != 0) icon.UpdateMark1(state);
    }

    public static void UpdateState(MarkableIcon icon, bool state0, bool state1)
    {
        if (!icon) return;

        icon.UpdateMark0(state0);
        icon.UpdateMark1(state1);
    }

    public int ID;
    public Button button;
    public Image icon;
    public Text text;
    public Transform mark0, mark1;
    public GameObject o;

    private MarkableIcon() { }

    public void UpdateMark0(bool mark) { if (mark0) mark0.gameObject.SetActive(mark); }
    public void UpdateMark1(bool mark) { if (mark1) mark1.gameObject.SetActive(mark); }

    public void Update(string name) { if (text) text.text = name; }

    public void Update(int _ID, Button _button, UnityEngine.Events.UnityAction onClick = null)
    {
        button = _button;
        if (!button)
        {
            ID              = 0;
            o               = null;
            button          = null;
            icon            = null;
            text            = null;
            mark0           = null;
            mark1           = null;
        }
        else
        {
            ID     = _ID;
            o      = button.gameObject;
            button = o.GetComponent<Button>();
            icon   = o.GetComponent<Image>("icon");
            text   = o.GetComponent<Text>("text");
            mark0  = o.GetComponent<Transform>("mark");
            mark1  = o.GetComponent<Transform>("mark1");

            UpdateMark0(false);
            UpdateMark1(false);

            if (onClick != null) button.onClick.AddListener(onClick);
        }
    }

    public void Invoke() { if (button) button.onClick.Invoke(); }
}

public class Window_Home : Window
{
    #region Internal structs

    private struct ShowTypeInfo { public GameObject o; public Action handler; }

    #endregion

    #region Static functions

    public const int Main = 0, Fight = 1, Dungeon = 2, CommStreet = 3, Count = 4;

    /// <summary>
    /// Show home window
    /// <para>
    /// panel: Main = 0, Fight = 1, Dungeon = 2, CommStreet = 3
    /// </para>
    /// </summary>
    /// <param name="panel">Sub panel</param>
    /// <returns></returns>
    public static void Show(int panel, HomeIcons icon)
    {
        GotoSubWindow<Window_Home>(panel << 16 | (int)icon & 0xFFFF);
    }

    #endregion

    #region Properties

    #region Default visible UI elements
    // Top-left buttons
    private Button m_btnWelfare, m_btnNotice, m_btnQuest, m_btnNote;

    // Bottom left buttons
    private Button m_btnChat, m_btnCollection;

    // Bottom right buttons
    private Button m_btnPet, m_btnRole, m_btnSkill, m_btnEquipment, m_btnRune, m_btnBag, m_btnTakePhoto;
    private Toggle m_btnHideBottom;

    // Right buttons
    private Button m_btnShop, m_btnDungeon, m_btnFight, m_btnAttack, m_btnUnion, m_btnStreet;

    private Transform m_factionTint,m_rankTint;

    private MarkableIcon[] m_homeIcons = new MarkableIcon[(int)HomeIcons.Count];

    private Toggle m_showPetToggle;

    //Top-left tweens
    private TweenPosition m_tweenTLIcons;
    private TweenAlpha m_tweenDatingNpc;
    private NpcMono m_datingNpcMono;

    private RectTransform m_tfEffectNode;
    #endregion

    #region Second layer buttons

    private Button m_train, m_pvp, m_match, m_factionBattle;  // Fight
    private Button m_labyrinth, m_bordlands; // Dungeons
    private Button m_forge, m_fashionShop, m_drifterShop, m_wishingWell, m_petSummon, m_awake; // Comm street

    #endregion

    private Level_Home level;
    private ShowTypeInfo[] m_showTypes = new ShowTypeInfo[Count];
    private int m_currentWindow = Main;
    #endregion

    #region pettip
    private GameObject m_pettip;
    private Button m_canenter;
    private Button m_canopen;
    private Button m_waite;
    private Image m_progressvalue;
    private GameObject m_waitprogress;
    //战斗力
    private Text m_combatValue;
    //公会boss
    private Button m_unionBossBtn;
    #endregion

    #region labyrinth

    private Image m_mazeReadyImg;
    private Image m_mazeChallengeImg;
    private Image m_mazeRestImg;
    private Image m_mazeSettlementImg;
    private Text m_labyrinthCountDown;

    #endregion

    #region banner
    private RectTransform m_banPlane;
    private RectTransform m_banPrefab;
    private RectTransform m_pagePrefab;
    private ToggleGroup m_pagePlane;

    List<GameObject> m_banList = new List<GameObject>();
    List<GameObject> m_pageList = new List<GameObject>();
    #endregion

    #region faction_battle

    private Transform m_factionLock;
    private Transform m_stateTintRoot;
    private Text      m_factionActiveTime;
    private Text      m_factionOpenTime;
    private Text      m_stateTint;

    #endregion

    #region 皇家斗技

    private Transform m_rankActiveRoot;
    private Text m_rankCountDown;
    private Text m_rankOpenTime;
    #endregion

    #region dating
    private Toggle m_tgSwitchDating;
    #endregion

    #region Initialzie

    protected override void OnInitialized()
    {
        base.OnInitialized();
        m_currentWindow = Main;
    }

    protected override void OnOpen()
    {
        //临时关闭图鉴入口2019.1.8 TZJ
        GetComponent<Transform>("main/top_left/icons/collection").gameObject.SetActive(false);

        level = Level.current as Level_Home;

        m_rankActiveRoot = GetComponent<Transform>("fight/rank/bg");
        m_rankCountDown = GetComponent<Text>("fight/rank/bg/Text");
        m_rankOpenTime = GetComponent<Text>("fight/rank/decription_text");

        m_btnWelfare = GetComponent<Button>("main/top_left/icons/welfare"); m_homeIcons[1] = MarkableIcon.Create(1, m_btnWelfare, () => { CanEnterWelfre(); });
        m_btnNotice = GetComponent<Button>("main/top_left/icons/announce"); m_homeIcons[3] = MarkableIcon.Create(3, m_btnNotice, ShowWindow<Window_Announcement>);
        m_btnQuest = GetComponent<Button>("main/top_left/icons/mission"); m_homeIcons[4] = MarkableIcon.Create(4, m_btnQuest, () => { moduleActive.ActiveClick = 0; ShowWindow<Window_Active>(); });
        m_btnNote = GetComponent<Button>("main/top_left/icons/note"); m_homeIcons[34] = MarkableIcon.Create(34, m_btnNote, ShowWindow<Window_DatingSelectNpc>);
        m_btnChat = GetComponent<Button>("main/bottom_left/chat"); m_homeIcons[7] = MarkableIcon.Create(7, m_btnChat, () => { moduleChat.opChatType = OpenWhichChat.WorldChat; ShowWindow<Window_Chat>(); });

        m_btnCollection = GetComponent<Button>("main/top_left/icons/collection"); m_homeIcons[33] = MarkableIcon.Create(33, m_btnCollection, ShowWindow<Window_Collection>);
        m_btnPet = GetComponent<Button>("main/bottom_right/icons/sprite"); m_homeIcons[25] = MarkableIcon.Create(25, m_btnPet, ShowWindow<Window_Sprite>);
        m_btnRole = GetComponent<Button>("main/bottom_right/icons/attribute"); m_homeIcons[8] = MarkableIcon.Create(8, m_btnRole, ShowWindow<Window_Attribute>);
        m_btnSkill = GetComponent<Button>("main/bottom_right/icons/skill"); m_homeIcons[27] = MarkableIcon.Create(27, m_btnSkill, ShowWindow<Window_Skill>);
        m_btnEquipment = GetComponent<Button>("main/bottom_right/icons/bag"); m_homeIcons[9] = MarkableIcon.Create(9, m_btnEquipment, () => { Module_Equip.selectEquipType = EnumSubEquipWindowType.MainPanel; ShowWindow<Window_Equip>(); });
        m_btnRune = GetComponent<Button>("main/bottom_right/icons/rune"); m_homeIcons[10] = MarkableIcon.Create(10, m_btnRune, ShowWindow<Window_RuneStart>);
        m_btnBag = GetComponent<Button>("main/bottom_right/icons/closet"); m_homeIcons[11] = MarkableIcon.Create(11, m_btnBag, () => { moduleCangku.chickType = WareType.Prop; ShowWindow<Window_Cangku>(); });
        m_awake = GetComponent<Button>("main/bottom_right/icons/awake"); m_homeIcons[28] = MarkableIcon.Create(28, m_awake, ShowWindow<Window_Awakeinit>);

        m_btnTakePhoto = GetComponent<Button>("main/bottom_right/shot/go"); m_btnTakePhoto.onClick.AddListener(TakePhoto);
        m_btnHideBottom = GetComponent<Toggle>("main/bottom_right/home"); m_btnHideBottom.onValueChanged.AddListener(YouAreBeautyBaby);

        m_btnShop = GetComponent<Button>("main/middle_right/street"); m_homeIcons[12] = MarkableIcon.Create(12, m_btnShop, () => { SwitchTo(CommStreet); });
        m_btnDungeon = GetComponent<Button>("main/middle_right/dungeons_btn"); m_homeIcons[13] = MarkableIcon.Create(13, m_btnDungeon, () => { SwitchTo(Dungeon); });
        m_btnFight = GetComponent<Button>("main/middle_right/battle"); m_homeIcons[14] = MarkableIcon.Create(14, m_btnFight, () => { SwitchTo(Fight); });
        m_btnAttack = GetComponent<Button>("main/middle_right/attack"); m_homeIcons[15] = MarkableIcon.Create(15, m_btnAttack, ShowWindow<Window_Chase>);
        m_btnUnion = GetComponent<Button>("main/middle_right/union"); m_homeIcons[29] = MarkableIcon.Create(29, m_btnUnion, ShowWindow<Window_Union>);
        m_btnStreet = GetComponent<Button>("main/middle_right/npcStreet"); m_homeIcons[31] = MarkableIcon.Create(31, m_btnStreet, OnClickDatingStreet);

        m_train = GetComponent<Button>("fight/train"); m_homeIcons[16] = MarkableIcon.Create(16, m_train, () => { Game.LoadLevel(GeneralConfigInfo.sTrainLevel); });
        m_pvp = GetComponent<Button>("fight/match"); m_homeIcons[17] = MarkableIcon.Create(17, m_pvp, () => { modulePVP.Enter(OpenWhichPvP.FreePvP); });
        m_match = GetComponent<Button>("fight/rank"); m_homeIcons[18] = MarkableIcon.Create(18, m_match, () => { modulePVP.Enter(OpenWhichPvP.LolPvP); });
        m_labyrinth = GetComponent<Button>("dungeons/labyrinth"); m_homeIcons[19] = MarkableIcon.Create(19, m_labyrinth, () => { moduleLabyrinth.SendLabyrinthEnter(); });
        m_bordlands = GetComponent<Button>("dungeons/bordlands"); m_homeIcons[20] = MarkableIcon.Create(20, m_bordlands, () => { moduleBordlands.Enter(); });
        m_forge = GetComponent<Button>("commercialstreet/forge"); m_homeIcons[21] = MarkableIcon.Create(21, m_forge, () => { moduleForging.ClickType = EquipType.Weapon; ShowWindow<Window_Forging>(); });
        m_fashionShop = GetComponent<Button>("commercialstreet/shizhuangdian"); m_homeIcons[22] = MarkableIcon.Create(22, m_fashionShop, ShowWindow<Window_Shizhuangdian>);
        m_drifterShop = GetComponent<Button>("commercialstreet/liulangshangdian"); m_homeIcons[23] = MarkableIcon.Create(23, m_drifterShop, ShowWindow<Window_Liulangshangdian>);
        m_wishingWell = GetComponent<Button>("commercialstreet/wish"); m_homeIcons[24] = MarkableIcon.Create(24, m_wishingWell, ShowWindow<Window_Wish>);
        m_petSummon = GetComponent<Button>("commercialstreet/summon"); m_homeIcons[26] = MarkableIcon.Create(26, m_petSummon, ShowWindow<Window_Summon>);
		m_factionBattle  = GetComponent<Button>("fight/faction");      m_homeIcons[35] = MarkableIcon.Create(35, m_factionBattle,() =>
        {
            if (moduleFactionBattle.state >= Module_FactionBattle.State.Processing) ShowWindow<Window_FactionBattle>();
            else ShowWindow<Window_FactionSign>();
        });

        m_pettip = GetComponent<RectTransform>("tips").gameObject;
        m_canenter = GetComponent<Button>("dungeons/sprite/highlight");
        m_canopen = GetComponent<Button>("dungeons/sprite/progress");
        m_waite = GetComponent<Button>("dungeons/sprite/countdown");
        m_progressvalue = GetComponent<Image>("dungeons/sprite/progress/progressframe_01/progressframe_02");
        m_waitprogress = GetComponent<RectTransform>("dungeons/sprite/countdown/countdownframe_img_01").gameObject;

        m_mazeReadyImg = GetComponent<Image>("dungeons/labyrinth/icon/ready");
        m_mazeChallengeImg = GetComponent<Image>("dungeons/labyrinth/icon/chanllenge");
        m_mazeRestImg = GetComponent<Image>("dungeons/labyrinth/icon/rest");
        m_mazeSettlementImg = GetComponent<Image>("dungeons/labyrinth/icon/settlement");
        m_labyrinthCountDown = GetComponent<Text>("dungeons/labyrinth/stateSign_Txt");

        #region faction
        m_factionTint        = GetComponent<Transform>("main/middle_right/battle/faction_battle");
        m_rankTint           = GetComponent<Transform>("main/middle_right/battle/rank_battle");
        m_factionLock        = GetComponent<Transform>("fight/faction/lock");
        m_stateTintRoot      = GetComponent<Transform>("fight/faction/bg");
        m_stateTint          = GetComponent<Text>("fight/faction/bg/Text (1)");
        m_factionActiveTime  = GetComponent<Text>("fight/faction/bg/Text");
        m_factionOpenTime    = GetComponent<Text>("fight/faction/Text");
        #endregion


        m_canenter.onClick.AddListener(SetTip);
        m_waite.onClick.AddListener(() =>
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(239, 6));
        });
        m_canopen.onClick.AddListener(SetTip);

        m_unionBossBtn = GetComponent<Button>("main/middle_right/union/mark");
        m_unionBossBtn.onClick.RemoveAllListeners();
        m_unionBossBtn.onClick.AddListener(delegate
        {
            moduleUnion.OpenBossWindow = true;
            ShowWindow<Window_Union>();
        });

        m_showTypes[0] = new ShowTypeInfo() { o = transform.Find("main").gameObject, handler = UpdateMain };
        m_showTypes[1] = new ShowTypeInfo() { o = transform.Find("fight").gameObject, handler = UpdateFight };
        m_showTypes[2] = new ShowTypeInfo() { o = transform.Find("dungeons").gameObject, handler = UpdateDungeon };
        m_showTypes[3] = new ShowTypeInfo() { o = transform.Find("commercialstreet").gameObject, handler = UpdateCommStreet };

        m_showPetToggle = GetComponent<Toggle>("main/bottom_right/showsprite");
        m_showPetToggle.isOn = true;
        m_showPetToggle.onValueChanged.AddListener(b => ToggleShowPet(b, true));

        m_combatValue = GetComponent<Text>("main/top_left/icons/combatEffectiveness/value");

        m_tweenTLIcons = GetComponent<TweenPosition>("main/top_left/icons");
        m_tfEffectNode = GetComponent<RectTransform>("effectnode");

        m_tweenDatingNpc = GetComponent<TweenAlpha>("main/content");
        m_datingNpcMono = GetComponent<NpcMono>("main/content/npcInfo");
        
        m_banPrefab = GetComponent<RectTransform>("main/banner/templte");
        m_banPlane = GetComponent<RectTransform>("main/banner/banScroll");
        m_pagePrefab = GetComponent<RectTransform>("main/banner/pageTog");
        m_pagePlane = GetComponent<ToggleGroup>("main/banner/pageScroll");
        GetChildenObj();

        m_tgSwitchDating = GetComponent<Toggle>("main/top_left/lanternTop");
        m_tgSwitchDating.isOn = moduleHome.showDatingModel;
        m_tgSwitchDating.onValueChanged.AddListener(b => SwitchDatingModel(b));

        InitializeIcons();
        InitializeUnlockText();
        MultiLangrage();
    }

    private void MultiLangrage()
    {
        Util.SetText(GetComponent<Text>("fight/rank/bg/Text (1)"), ConfigText.GetDefalutString(558, 9));
    }

    private void CanEnterWelfre()
    {
        if (moduleWelfare.allWeflarInfo.Count <= 0)
        {
            moduleGlobal.ShowMessage(211, 29);
            return;
        }
        ShowWindow<Window_Welfare>();
    }
    #endregion

    #region 

    private void ToggleShowPet(bool toggle, bool hint = false)
    {
        //没有出战的宠物，显示无效
        if (modulePet.FightingPet == null)
        {
            if (hint && toggle)
                moduleGlobal.ShowMessage(298);
            return;
        }
        if (level) level.ToggleFightPet(toggle, m_btnHideBottom.isOn);
    }

    protected override void OnClose()
    {
        m_homeIcons.Clear(true);
        m_showTypes.Clear();
    }

    protected override void OnHide(bool forward)
    {
        HideDatingEffect();
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        if (m_subTypeLock > -1) m_currentWindow = m_subTypeLock >> 16;

        SwitchTo(m_currentWindow, true);

        if (m_currentWindow != Main)
        {
            moduleHome.ResetCamera();
            if (actived)
                m_behaviour.Show(true);        // Show immediately if we are not in Main state
        }
        else if (!oldState && forward) moduleGlobal.OnGlobalTween(false);  // First time, play global layer tween

        Util.SetText(m_combatValue, modulePlayer.roleInfo.fight.ToString());

        moduleMatch.isbaning = true;

        UpdateIconState();

        moduleGlobal.ShowGlobalLayerDefault(m_currentWindow == Main ? 0 : 1, true);

        var subTypeIcon = m_subTypeLock > -1 ? m_subTypeLock & 0xFFFF : (int)moduleHome.windowIcon;
        if (subTypeIcon > 0)
        {
            Logger.LogInfo("Window_Home::OnBecameVisible: Execute action [{0}]", subTypeIcon);
            if (moduleGuide.IsActiveFunction(subTypeIcon))
            {
                var i = m_homeIcons[subTypeIcon];
                if (i) i.Invoke();
                else moduleGlobal.ExecuteIconAction(subTypeIcon);
            }
        }
        else moduleGuide.AddWindowHomeEvent(m_currentWindow);

        moduleHome.DispatchEvent(Module_Home.EventSwitchCameraMode, Event_.Pop(CameraShowType.Home));

        //强制刷新
        moduleGuide.RefreshUnlockBtns(this);
        moduleGuide.RefreshUnlockBtns(GetOpenedWindow<Window_Global>());
        moduleGuide.RefreshBtnUnlockState();

        if (moduleFriend.offlineGetFriendPoint > 0)
        {
            moduleGlobal.ShowMessage(Util.Format(ConfigText.GetDefalutString(9106), moduleFriend.offlineGetFriendPoint));
            moduleFriend.offlineGetFriendPoint = 0;
        }

        m_factionTint.SafeSetActive(moduleFactionBattle.IsProcessing);
        m_rankTint.SafeSetActive(!moduleFactionBattle.IsProcessing && moduleGlobal.IsStayLoLTime());

        m_subTypeLock = -1;
        moduleCangku.UseSubType = true;

        moduleHome.SetWindowPanelAndIcon(Main, HomeIcons.Unused);

        moduleNPCDating.SetCurDatingScene(EnumNPCDatingSceneType.GameHall);
        //检测约会开始邀请剧情对话断线重连
        moduleNPCDating.CheckDatingReconnect(EnumNPCDatingSceneType.GameHall);
    }

    private void InitializeIcons()
    {
        var text = ConfigManager.Get<ConfigText>(9002);
        if (!text) return;

        for (int i = 1, c = m_homeIcons.Length; i < c; ++i)
            m_homeIcons[i]?.Update(text[i]);
        Util.SetText(m_btnTakePhoto.gameObject.GetComponent<Text>("text"), text[m_homeIcons.Length]);
        Util.SetText(GetComponent<Text>("dungeons/sprite/text"), 9002, 30);
    }

    private void InitializeUnlockText()
    {
        var t = ConfigManager.Get<ConfigText>((int)TextForMatType.HomeUnlockUIText);
        if (!t) return;

        Util.SetText(GetComponent<Text>("main/middle_right/street/lock/streetlcokframe/attacklocktext"), t[0]);
        Util.SetText(GetComponent<Text>("main/middle_right/dungeons_btn/lock/dungeonslcokframe/attacklocktext"), t[1]);
        Util.SetText(GetComponent<Text>("main/middle_right/battle/lock/fightlcokframe/attacklocktext"), t[2]);
        Util.SetText(GetComponent<Text>("main/middle_right/attack/lock/attacklcokframe/attacklocktext"), t[3]);

        Util.SetText(GetComponent<Text>("fight/train/lock/blackmask/lock_text"), t[4]);
        Util.SetText(GetComponent<Text>("fight/match/lock/blackmask/lock_text"), t[5]);
        Util.SetText(GetComponent<Text>("fight/rank/lock/blackmask/lock_text"), t[6]);

        Util.SetText(GetComponent<Text>("dungeons/labyrinth/lock/blackmask/lock_text"), t[7]);
        Util.SetText(GetComponent<Text>("dungeons/bordlands/lock/blackmask/lock_text"), t[8]);
        Util.SetText(GetComponent<Text>("dungeons/sprite/lock/blackmask/lock_text"), t[13]);

        Util.SetText(GetComponent<Text>("commercialstreet/forge/lock/blackmask/lock_text"), t[9]);
        Util.SetText(GetComponent<Text>("commercialstreet/shizhuangdian/lock/blackmask/lock_text"), t[10]);
        Util.SetText(GetComponent<Text>("commercialstreet/liulangshangdian/lock/blackmask/lock_text"), t[11]);
        Util.SetText(GetComponent<Text>("commercialstreet/wish/lock/blackmask/lock_text"), t[12]);
        Util.SetText(GetComponent<Text>("commercialstreet/summon/lock/blackmask/lock_text"), t[13]);

        Util.SetText(GetComponent<Text>("main/top_left/icons/combatEffectiveness/type"), 204, 36);
        Util.SetText(GetComponent<Text>("main/top_left/icons/combatEffectiveness/type"), 204, 36);
    }

    #endregion

    #region Icon interface

    public void UpdateIconState(HomeIcons icon, int index, bool state)
    {
        var iid = (int)icon;
        MarkableIcon.UpdateIconState(iid < 1 || iid >= m_homeIcons.Length ? null : m_homeIcons[(int)icon], index, state);
    }

    public void UpdateIconState(HomeIcons icon)
    {
        var iid = (int)icon;
        MarkableIcon.UpdateState(iid < 1 || iid >= m_homeIcons.Length ? null : m_homeIcons[(int)icon], moduleHome.GetIconState(icon, 0), moduleHome.GetIconState(icon, 1));
    }

    public void UpdateIconState()
    {
        moduleHome.UpdateIconState(HomeIcons.Awake, moduleAwake.NeedNotice);
        moduleHome.UpdateIconState(HomeIcons.Pet, modulePet.NeedNotice);
        moduleHome.UpdateIconState(HomeIcons.Mail, moduleMailbox.NeedNotice);
        moduleHome.UpdateIconState(HomeIcons.Role, moduleAttribute.NeedNotice);
        moduleHome.UpdateIconState(HomeIcons.Skill, moduleSkill.NeedNotice);
        moduleHome.UpdateIconState(HomeIcons.Match, moduleGlobal.IsStayLoLTime());
        moduleHome.UpdateIconState(HomeIcons.Rune, moduleRune.IsShowNotice());
        moduleCangku.GetAllNew();

        if (moduleEquip.FirstEnter)
        {
            moduleEquip.FirstEnter = false;
            moduleHome.UpdateIconState(HomeIcons.Equipment, moduleEquip.CheckHomeEquipMark());
        }
        else moduleHome.UpdateIconState(HomeIcons.Equipment, moduleEquip.AddRefrshEquipHint());

        var mask0 = moduleHome.iconMask0;
        var mask1 = moduleHome.iconMask1;
        for (int i = 1, c = m_homeIcons.Length; i < c; ++i)
        {
            var icon = m_homeIcons[i];
            if (!icon) continue;

            icon.UpdateMark0(mask0.BitMask(i));
            icon.UpdateMark1(mask1.BitMask(i));
        }
    }

    #endregion

    #region UI logic

    public void SwitchTo(int type, bool force = false)
    {
        if (!force && m_currentWindow == type) return;

        var changed = m_currentWindow != type;
        m_currentWindow = type;

        foreach (var show in m_showTypes) show.o.SetActive(false);

        UpdateCurrentShowType();

        if (changed) moduleGuide.AddWindowHomeEvent(m_currentWindow);

        HideDatingEffect();
    }

    public void SwitchToPetTip()
    {
        SwitchTo(Dungeon);
        SetTip();
    }

    private void ShowWindow<T>() where T : Window
    {
        ShowAsync<T>();
    }

    private void UpdateCurrentShowType()
    {
        moduleGlobal.ShowGlobalAvatarOrReturn(m_currentWindow == Main ? 0 : 1);

        var show = m_showTypes[m_currentWindow];
        show.o.SetActive(true);
        show.handler.Invoke();
    }

    private void UpdateMain()
    {
        moduleHome.ResetCamera();

        bool bHavePet = modulePet.FightingPet != null;
        UpdateFightingPet(bHavePet);

        bool bShowNpc = moduleNPCDating.isDating && moduleHome.showDatingModel;
        UpdateDatingModel(bShowNpc);

        if (bShowNpc)
        {
            level?.ToggleFightPet(true, false);
            moduleHome.HideOthers(moduleNPCDating.datingNpcModelName);
        }
        else
        {
            level?.ToggleFightPet(true, true);
            if (bHavePet) moduleHome.HideOthersBut(Module_Home.PLAYER_OBJECT_NAME, Module_Home.FIGHTING_PET_OBJECT_NAME);
            else moduleHome.HideOthers(Module_Home.PLAYER_OBJECT_NAME);
        }

    }

    private void UpdateFightingPet(bool bHavePet)
    {
        if (level != null) level.DestroyFightPet();
    }

    private void UpdateDatingModel(bool bShowNpc)
    {
        if (bShowNpc)
        {
            m_tweenTLIcons.PlayForward();
            moduleNPCDating.CreateDatingNpc(moduleNPCDating.datingNpcModelName);
            level?.ToggleFightPet(true, false);
        }
        else m_tweenTLIcons.PlayReverse();
        m_tgSwitchDating.gameObject.SetActive(moduleNPCDating.isDating);

        moduleHome.DispatchSwitchDatingModel();
    }

    private void UpdateFight()
    {
        var str = Util.GetString(219, 37);
        var ts = moduleGlobal.system.rankPvpTimes;
        for (int i = 0, c = ts.Length; i < c; i += 2) str += Util.GetTimeFromSecDuration((int)ts[i], (int)ts[i + 1]);
        Util.SetText(m_rankOpenTime, str);

        RefreshFactionInfo();
        var factionActive = moduleGuide.IsActiveFunction(HomeIcons.Faction);
        m_factionLock.SafeSetActive(!factionActive);
        m_stateTintRoot.SafeSetActive(factionActive && moduleFactionBattle.IsActive);
    }

    private void UpdateDungeon()
    {
        SetButtonState();
    }

    private void UpdateCommStreet()
    {

    }

    private void YouAreBeautyBaby(bool on)
    {
        moduleGlobal.OnGlobalTween(on);
        if (on)
        {
            m_showPetToggle.isOn = true;
            ToggleShowPet(true);
            moduleGlobal.ShowMessage(9005);
        }
        else
        {
            moduleHome.ResetPlayer();
            ToggleShowPet(true);
        }


        moduleGlobal.HideShareTool();

        DispatchEvent(LevelEvents.PHOTO_MODE_STATE, Event_.Pop(on));
    }

    private void TakePhoto()
    {
        var c = level.mainCamera;
        var y = Mathf.CeilToInt(Screen.height * c.rect.y);
        var h = Mathf.CeilToInt(Screen.height * c.rect.height);
        var t = RenderTexture.GetTemporary(Screen.width, h);

        var rc = c.rect;
        var ot = c.targetTexture;

        c.rect = new Rect(0, 0, 1, 1);
        c.targetTexture = t;

        var at = RenderTexture.active;
        RenderTexture.active = t;

        c.Render();

        c.rect = rc;
        c.targetTexture = ot;

        var tt = new Texture2D(Screen.width, Screen.height);

        for (var yy = 0; yy < y; ++yy)
        {
            for (int xx = 0, ww = tt.width; xx < ww; ++xx)
                tt.SetPixel(xx, yy, Color.black);
        }

        for (int yy = h, hh = tt.height; yy < hh; ++yy)
        {
            for (int xx = 0, ww = tt.width; xx < ww; ++xx)
                tt.SetPixel(xx, yy, Color.black);
        }

        tt.ReadPixels(new Rect(0, 0, Screen.width, h), 0, y);
        tt.Apply();

        RenderTexture.active = at;
        RenderTexture.ReleaseTemporary(t);

        var r = Util.SaveFile(LocalFilePath.SCREENSHOT + "/ScreentShot.png", tt.EncodeToPNG(), true, true);

        UnityEngine.Object.Destroy(tt);

        if (r == null) moduleGlobal.ShowMessage(9005, 1);  // Create image failed
        else
        {
            var success = SDKManager.SaveImageToPhotoLibrary(r);
            if (success) moduleGlobal.ShowShareTool(r, Util.GetString(9005, 7));
        }
    }

    private void UpdateLabyrinthStep()
    {
        if (!actived) return;
        m_mazeReadyImg.SafeSetActive(moduleLabyrinth.currentLabyrinthStep == EnumLabyrinthTimeStep.Ready);
        m_mazeChallengeImg.SafeSetActive(moduleLabyrinth.currentLabyrinthStep == EnumLabyrinthTimeStep.Chanllenge);
        m_mazeRestImg.SafeSetActive(moduleLabyrinth.currentLabyrinthStep == EnumLabyrinthTimeStep.Rest);
        m_mazeSettlementImg.SafeSetActive(moduleLabyrinth.currentLabyrinthStep == EnumLabyrinthTimeStep.Close || moduleLabyrinth.currentLabyrinthStep == EnumLabyrinthTimeStep.SettleMent || moduleLabyrinth.currentLabyrinthStep == EnumLabyrinthTimeStep.None);

        if (moduleLabyrinth.currentLabyrinthStep != EnumLabyrinthTimeStep.None) Util.SetText(m_labyrinthCountDown, moduleLabyrinth.GetStepAndTimeString());
        else Util.SetText(m_labyrinthCountDown, (int)TextForMatType.LabyrinthUIText, 1);
    }

    private void SwitchDatingModel(bool bShowNpc)
    {
        moduleHome.SwitchDatingModel(bShowNpc);
        StartCoroutine(PlayEffect(UpdateMain));
    }

    /// <summary>
    /// 约会开始
    /// </summary>
    private void DatingStart()
    {
        m_tgSwitchDating.gameObject.SetActive(true);
        m_tgSwitchDating.onValueChanged.RemoveAllListeners();
        m_tgSwitchDating.isOn = false;
        m_tgSwitchDating.onValueChanged.AddListener(b=>SwitchDatingModel(b));
        if (m_tfEffectNode.childCount > 0) return;
        m_tfEffectNode.SafeSetActive(true);
        UIEffectManager.PlayEffectAsync(m_tfEffectNode, GeneralConfigInfo.defaultConfig.datingStartEffect);
    }

    /// <summary>
    /// 隐藏约会相关特效
    /// </summary>
    private void HideDatingEffect()
    {
        Util.ClearChildren(m_tfEffectNode);
    }

    /// <summary>
    /// 点击约会娱乐街按钮
    /// </summary>
    private void OnClickDatingStreet()
    {
        if (moduleNPCDating.curDatingNpc != null)
        {
            if (moduleNPCDating.curDatingNpc.datingEnd == 1)
            {
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.NpcDating, 6));//约会已经结束
            }
            else ShowWindowAsLevel<Window_NPCDating>(DatingMapCtrl.GetPreRes());
        }
        else ShowWindow<Window_DatingSelectNpc>();
    }

    private void ShowWindowAsLevel<T>(List<string> assets) where T : Window
    {
        PrepareWindowAssetsAsLevel<T>(assets, f =>
        {
            if (!f) Logger.LogError($"Window_Home::ShowWindowAsLevel: Prepare assets failed. Window:[{typeof(T).Name}]");
            else ShowAsync<T>();
        });
    }

    private void ShowWindowAsLevel(string n, List<string> assets)
    {
        PrepareWindowAssetsAsLevel(n, assets, f =>
        {
            if (!f) Logger.LogError($"Window_Home::ShowWindowAsLevel: Prepare assets failed. Window:[{n}]");
            else ShowAsync(n);
        });
    }

    #endregion

    #region switch dating model effect
    private IEnumerator PlayEffect(Action callback)
    {
        m_tfEffectNode.SafeSetActive(true);
        string effectName = GeneralConfigInfo.defaultConfig.homeScreenEffect;
        UIEffectManager.PlayEffectAsync(m_tfEffectNode, effectName);

        float effectTime = GeneralConfigInfo.defaultConfig.screenEffectPlayTime / 1000;//配置是毫秒单位
        yield return new WaitForSeconds(effectTime);

        callback?.Invoke();
    }

    #endregion

    #region pettip

    private void SetButtonState()
    {
        m_canenter.gameObject.SetActive(false);
        m_waite.gameObject.SetActive(false);
        m_canopen.gameObject.SetActive(false);
        if (moduleHome.LocalPetInfo == null)
        {
            Logger.LogError("local pet task info is null");
            return;
        }
        int btn_state = moduleHome.SetState();
        if (btn_state == 0)
        {
            m_waite.gameObject.SetActive(true);
            //计算时间以及百分比
            SetTime();
        }
        else if (btn_state == 1)
        {
            m_canenter.gameObject.SetActive(true);
        }
        else if (btn_state == 2)
        {
            m_canopen.gameObject.SetActive(true);
            m_progressvalue.fillAmount = (float)moduleHome.LocalPetInfo.progress / (float)100;
        }
    }

    private void SetTime()
    {
        DateTime noewtime = Util.GetServerLocalTime();
        string closetime = PlayerPrefs.GetString(moduleHome.CloseKey());
        DateTime check = Util.GetServerLocalTime();
        if (string.IsNullOrEmpty(closetime) || !DateTime.TryParse(closetime, out check))
        {
            closetime = Util.GetServerLocalTime().ToString();
            PlayerPrefs.SetString(moduleHome.CloseKey(), closetime);
            Logger.LogError("Module_Home : Pet task close time is null");
        }
        DateTime closeend = Convert.ToDateTime(closetime);

        string month = "/" + closeend.Month.ToString();
        if (closeend.Month < 10)
        {
            month = "/" + "0" + closeend.Month.ToString();
        }
        string days = "/" + closeend.Day.ToString();
        if (closeend.Day < 10)
        {
            days = "/" + "0" + closeend.Day.ToString();
        }
        string refresh = closeend.Year.ToString() + month + days + " " + "05:" + "00:" + "00";
        DateTime refreshtime = DateTime.ParseExact(refresh, "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

        DateTime nexttime = refreshtime;
        if (closeend.Hour >= 5)
        {
            //距离日期加一
            nexttime = refreshtime.AddDays(1);
        }
        TimeSpan refreshremain = nexttime - noewtime;//当前与刷新相差多少
        TimeSpan allremain = nexttime - closeend;//关闭时与刷新相差多少

        float durationtime = (float)refreshremain.TotalSeconds;
        float alltime = (float)allremain.TotalSeconds;

        TweenFillAmount amount = m_waitprogress.GetComponent<TweenFillAmount>();
        amount.duration = durationtime;
        amount.from = durationtime / alltime;
    }

    private void SetTip()
    {
        m_pettip.gameObject.SetActive(true);
        SpriteTip tip = m_pettip.GetComponentDefault<SpriteTip>();
        tip.SetInfo();
    }
    private void ValueChange(int newvalue)
    {
        m_progressvalue.fillAmount = (float)newvalue / (float)100;
        SpriteTip tip = m_pettip.GetComponentDefault<SpriteTip>();
        tip.ChangeValue(newvalue);
    }
    #endregion

    #region Faction

    private void RefreshFactionState()
    {
        m_factionTint.SafeSetActive(moduleFactionBattle.IsProcessing);
        Util.SetText(m_stateTint, ConfigText.GetDefalutString(558, Mathf.Min(4, (int) moduleFactionBattle.state)));
    }

    private void RefreshFactionInfo()
    {
        RefreshFactionState();
        if(moduleFactionBattle.state == Module_FactionBattle.State.Close || 
            moduleFactionBattle.state == Module_FactionBattle.State.End ||
            moduleFactionBattle.state == Module_FactionBattle.State.Settlement)
            Util.SetText(m_factionActiveTime, Util.Format(ConfigText.GetDefalutString(558, 5), moduleFactionBattle.ActiveTime));
        Util.SetText(m_factionOpenTime, Util.Format(ConfigText.GetDefalutString(558, 5), moduleFactionBattle.ActiveTime));
    }

    public override void OnRenderUpdate()
    {
        base.OnRenderUpdate();

        var factionActive = moduleGuide.IsActiveFunction(HomeIcons.Faction);
        m_stateTintRoot.SafeSetActive(factionActive && moduleFactionBattle.IsActive);
        switch (moduleFactionBattle.state)
        {
            case Module_FactionBattle.State.Sign:
                Util.SetText(m_factionActiveTime, Util.Format(ConfigText.GetDefalutString(558, 6), moduleFactionBattle.SignCountDown));
                break;
            case Module_FactionBattle.State.Readly:
                Util.SetText(m_factionActiveTime, Util.Format(ConfigText.GetDefalutString(558, 7), moduleFactionBattle.ReadlyCountDown));
                break;
            case Module_FactionBattle.State.Processing:
                Util.SetText(m_factionActiveTime, Util.Format(ConfigText.GetDefalutString(558, 8), moduleFactionBattle.CountDown));
                break;
        }

        m_rankActiveRoot.SafeSetActive(moduleGlobal.IsStayLoLTime());
        if (moduleGlobal.IsStayLoLTime())
            Util.SetText(m_rankCountDown, Util.Format(ConfigText.GetDefalutString(558, 8), moduleHome.RankCountDown));
    }
    #endregion


    #region Module event handlers

    void _ME(ModuleEvent<Module_FactionBattle> e)
    {
        switch (e.moduleEvent)
        {
            case Module_FactionBattle.EventStateChange:
                RefreshFactionState();
                break;
            case Module_FactionBattle.EventGotFactionInfo:
                RefreshFactionInfo();
                break;
        }
    }

    void _ME(ModuleEvent<Module_Match> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Match.EventRankingStart: 
            case Module_Match.EventRankingEnd: m_rankTint.SafeSetActive(!moduleFactionBattle.IsProcessing && moduleGlobal.IsStayLoLTime()); break;
            default:
                break;
        }
    }

    void _ME(ModuleEvent<Module_Home> e)
    {
        if (e.moduleEvent == Module_Home.EventIconState)
        {
            UpdateIconState((HomeIcons)e.param1, (int)e.param2, (bool)e.param3);
            return;
        }
        else if (e.moduleEvent == Module_Home.EventAllPetTaskInfo)
        {
            SetButtonState();//设置按钮状态
            SpriteTip tip = m_pettip.GetComponentDefault<SpriteTip>();
            tip.SetInfo();//设置具体内容
        }
        else if (e.moduleEvent == Module_Home.EventPetProgressNewValue)
        {
            int newvalue = Util.Parse<int>(e.param1.ToString());
            ValueChange(newvalue);
        }
        else if (e.moduleEvent == Module_Home.EventPetOpenSucced)
        {
            SetTip();
        }
        else if (e.moduleEvent == Module_Home.EventBannerInfo) SetBannerInfo();
    }

    void _ME(ModuleEvent<Module_Labyrinth> e)
    {
        if (e.moduleEvent == Module_Labyrinth.EventLabyrinthTimeRefresh) UpdateLabyrinthStep();
    }

    void _ME(ModuleEvent<Module_Story> e)
    {
        if (!actived) return;
        if (e.moduleEvent == Module_Story.EventStoryClosed)
        {
            DatingStart();
        }
    }

    protected override void OnReturn()
    {
        SwitchTo(Main);
    }

    #endregion

    #region Bannner

    private void GetChildenObj()
    {
        m_banList.Clear();
        m_pageList.Clear();
        foreach (Transform item in m_banPlane)
        {
            item.SafeSetActive(true);
            m_banList.Add(item.gameObject);
        }
        foreach (Transform item in m_pagePlane.transform)
        {
            item.SafeSetActive(true);
            m_pageList.Add(item.gameObject);
        }
        SetBannerInfo();
    }

    private void SetBannerInfo()
    {
        moduleHome.bannnerList.Sort((a, b) => a.sort.CompareTo(b.sort));
        m_banPrefab.SafeSetActive(false);
        m_pagePrefab.SafeSetActive(false);

        if (m_banList.Count > moduleHome.bannnerList.Count)
        {
            for (int i = moduleHome.bannnerList.Count; i < m_banList.Count; i++)
            {
                m_banList[i].SafeSetActive(false);
                m_pageList[i].SafeSetActive(false);
            }
        }
        else
        {
            var count = moduleHome.bannnerList.Count - m_banList.Count;
            for (int i = 0; i < count; i++)
            {
                var obj = GameObject.Instantiate(m_banPrefab);
                var page = GameObject.Instantiate(m_pagePrefab);
                page.name = string.Format("page{0}", i);
                obj.name = string.Format("ban{0}", i);
                SetPostion(obj, m_banPlane);
                SetPostion(page, m_pagePlane.transform);
                m_banList.Add(obj.gameObject);
                m_pageList.Add(page.gameObject);
                var tog = page.GetComponentDefault<Toggle>();
                tog.group = m_pagePlane;
            }
        }
        for (int i = 0; i < moduleHome.bannnerList.Count; i++)
        {
            if (moduleHome.bannnerList[i] == null) continue;
            if (i >= m_banList.Count || m_banList[i] == null || !m_banList[i].activeSelf) continue;
            var ban = m_banList[i].GetComponentDefault<BannerSetInfo>();
            ban.SetBannerInfo(moduleHome.bannnerList[i]);
        }
        CycleBanner cb = m_banPlane.GetComponentDefault<CycleBanner>();
        if (cb.mPage == null) cb.mPage = m_pagePlane?.gameObject.GetComponent<RectTransform>();
        if (cb.cellSize == Vector2.zero) cb.cellSize = m_banPrefab.sizeDelta;
        if (cb.tweenStepNum == 0) cb.tweenStepNum = 25;
        cb.SetBannerView();
    }

    private void SetPostion(Transform obj,Transform parent)
    {
        obj.SetParent(parent);
        obj.localScale = new Vector3(1, 1, 1);
        obj.localPosition = Vector3.zero;
        obj.SafeSetActive(true);
    }
    #endregion

    protected override void GrabRestoreData(WindowHolder holder)
    {
        holder.SetData(m_currentWindow);
    }

    protected override void ExecuteRestoreData(WindowHolder holder)
    {
        m_currentWindow = holder.GetData<int>(0, m_currentWindow);
    }
}
