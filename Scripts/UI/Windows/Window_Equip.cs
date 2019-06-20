/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-05-23
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using EvolveMaterial = EvolveEquipInfo.EvolveMaterial;
using DG.Tweening;
using UnityEngine.PostProcessing;

public class Window_Equip : Window
{
    public const int NEW_EQUIP_WINDOW_TEXT = -300;
    public readonly static string[] BG_NAMES = new string[] { "white", "green", "blue", "purple", "golden", "orange" };

    private UICharacter uiCharacter;
    private Creature m_player;
    private EquipMainPanel m_equipMainPanel { get { return m_panelDic.Get(EnumSubEquipWindowType.MainPanel) as EquipMainPanel; } }

    private Dictionary<EnumSubEquipWindowType, CustomSecondPanel> m_panelDic = new Dictionary<EnumSubEquipWindowType, CustomSecondPanel>();
    private Camera m_mainCamra;
    private List<Vector3> m_cameraVectors = new List<Vector3>();

    private EquipAnimationInfo.AnimationData m_displayAnimData;
    private EquipAnimationInfo.GotoData m_displayStateData;

    private int m_delayPlayerRotId;
    private Tween m_playerRotTween;
    private int m_delayCameraAnimId;
    private Tween m_cameraAnimTween;
    private Tween m_cameraRotationAnimTween;

    private Transform m_mainPlane;
    private Transform m_intentyPlane;
    private Transform m_changeClothPlane;
    private Transform m_changeEquipPlane;

    #region window_base

    protected override void OnOpen()
    {
        transform.Find("middlecentre")?.gameObject?.SetActive(true);
        uiCharacter = GetComponent<UICharacter>("RawImage");
        m_player = ObjectManager.FindObject<Creature>(c => c.isPlayer);

        #region second panel

        m_mainPlane = transform.Find("equip_Panel");
        m_intentyPlane = transform.Find("middlecentre/preview_panel");
        m_changeClothPlane = transform.Find("change_panel_02");
        m_changeEquipPlane = transform.Find("middlecentre/change_panel");

        m_panelDic.Clear();
        m_panelDic.Add(EnumSubEquipWindowType.MainPanel, new EquipMainPanel(m_mainPlane, OpenChangeEquipPanel));
        m_panelDic.Add(EnumSubEquipWindowType.IntentyDetaiPanel, new IntentyEquipDetailPanel(m_intentyPlane));
        m_panelDic.Add(EnumSubEquipWindowType.ChangeNonIntentyEquipPanel, new ChangeNonIntentyEquipPanel(m_changeClothPlane, m_player, uiCharacter?.camera));
        m_panelDic.Add(EnumSubEquipWindowType.ChangeIntentyEquipPanel, new ChangeIntentyEquipPanel(m_changeEquipPlane));
        #endregion

        InitializeText();
        m_mainCamra = Level.current.mainCamera;
        m_cameraVectors.Clear();
        var uiChracter = GetComponent<UICharacter>("RawImage");
        if (uiChracter)
        {
            m_cameraVectors.Add(uiChracter.cameraPosition);
            m_cameraVectors.Add(uiChracter.cameraRotation);
        }
    }

    private void InitializeText()
    {
        var t = ConfigManager.Get<ConfigText>((int)TextForMatType.EquipUIText);
        if (!t) return;

        Util.SetText(GetComponent<Text>("bg/biaoti/text1"), (int)TextForMatType.EquipUIText, 0);
        Util.SetText(GetComponent<Text>("bg/biaoti/text2"), (int)TextForMatType.EquipUIText, 1);
        Util.SetText(GetComponent<Text>("bg/biaoti/english"), (int)TextForMatType.EquipUIText, 2);

        Util.SetText(GetComponent<Text>("equip_Panel/weapon_btn/type_txt"), (int)TextForMatType.EquipUIText, 4);
        Util.SetText(GetComponent<Text>("equip_Panel/gun_btn/type_txt"), (int)TextForMatType.EquipUIText, 5);
        Util.SetText(GetComponent<Text>("equip_Panel/suit_btn/type_txt"), (int)TextForMatType.EquipUIText, 6);
        Util.SetText(GetComponent<Text>("equip_Panel/decoration_btn/Text"), (int)TextForMatType.EquipUIText, 7);

        Util.SetText(GetComponent<Text>("middlecentre/preview_panel/right/bground/base/equiped_img/equiped"), (int)TextForMatType.EquipUIText, 8);
        Util.SetText(GetComponent<Text>("middlecentre/preview_panel/right/lock/txt"), (int)TextForMatType.EquipUIText, 10);
        Util.SetText(GetComponent<Text>("middlecentre/preview_panel/right/change_btn/get_text"), (int)TextForMatType.EquipUIText, 11);
        Util.SetText(GetComponent<Text>("middlecentre/preview_panel/right/checkBox/richang/Text"), (int)TextForMatType.EquipUIText, 12);
        Util.SetText(GetComponent<Text>("middlecentre/preview_panel/right/checkBox/richang/xz/richang_text"), (int)TextForMatType.EquipUIText, 12);
        Util.SetText(GetComponent<Text>("middlecentre/preview_panel/right/checkBox/qiangxie/Text"), (int)TextForMatType.EquipUIText, 13);
        Util.SetText(GetComponent<Text>("middlecentre/preview_panel/right/checkBox/qiangxie/xz/richang_text"), (int)TextForMatType.EquipUIText, 13);
        Util.SetText(GetComponent<Text>("middlecentre/preview_panel/right/checkBox/suit/Text"), (int)TextForMatType.EquipUIText, 14);
        Util.SetText(GetComponent<Text>("middlecentre/preview_panel/right/checkBox/suit/xz/richang_text"), (int)TextForMatType.EquipUIText, 14);

        Util.SetText(GetComponent<Text>("middlecentre/change_panel/left/top/suit/nothing"), (int)TextForMatType.EquipUIText, 15);
        Util.SetText(GetComponent<Text>("middlecentre/change_panel/left/bottom/title"), (int)TextForMatType.EquipUIText, 16);
        Util.SetText(GetComponent<Text>("middlecentre/change_panel/right/title_txt"), (int)TextForMatType.EquipUIText, 17);
        Util.SetText(GetComponent<Text>("middlecentre/change_panel/right/equip/text"), (int)TextForMatType.EquipUIText, 21);
        Util.SetText(GetComponent<Text>("middlecentre/change_panel/right/info/text"), (int)TextForMatType.EquipUIText, 22);

        Util.SetText(GetComponent<Text>("change_panel_02/right/title_txt"), (int)TextForMatType.EquipUIText, 23);
        Util.SetText(GetComponent<Text>("change_panel_02/right/equip/text"), (int)TextForMatType.EquipUIText, 25);
        Util.SetText(GetComponent<Text>("change_panel_02/right/info/text"), (int)TextForMatType.EquipUIText, 26);
        Util.SetText(GetComponent<Text>("change_panel_02/checkBox/suit/Text"), (int)TextForMatType.EquipUIText, 24);
        Util.SetText(GetComponent<Text>("change_panel_02/checkBox/suit/xz/suit_text"), (int)TextForMatType.EquipUIText, 24);
        Util.SetText(GetComponent<Text>("change_panel_02/left/chongzhi/chongzhi_text"), (int)TextForMatType.EquipUIText, 27);
        //装备按钮文本
        Util.SetText(GetComponent<Text>("change_panel_02/checkBox/head/Text"), 216, 40);
        Util.SetText(GetComponent<Text>("change_panel_02/checkBox/head/xz/jewelry_text"), 216, 40);
        Util.SetText(GetComponent<Text>("change_panel_02/checkBox/hair/Text"), 216, 41);
        Util.SetText(GetComponent<Text>("change_panel_02/checkBox/hair/xz/jewelry_text"), 216, 41);
        Util.SetText(GetComponent<Text>("change_panel_02/checkBox/face/Text"), 216, 42);
        Util.SetText(GetComponent<Text>("change_panel_02/checkBox/face/xz/jewelry_text"), 216, 42);
        Util.SetText(GetComponent<Text>("change_panel_02/checkBox/neck/Text"), 216, 43);
        Util.SetText(GetComponent<Text>("change_panel_02/checkBox/neck/xz/jewelry_text"), 216, 43);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleEquip.RefreshEquipHintInfo();
        moduleEquip.PrepareEquipData();
        moduleGlobal.ShowGlobalLayerDefault();

        SetPlaneState();
        OnRefreshButtonMark();
    }

    private void SetPlaneState()
    {
        Module_Equip.accesssReturnWay = 0;
        if ((Module_Equip.selectEquipType == EnumSubEquipWindowType.ChangeIntentyEquipPanel || Module_Equip.selectEquipType == EnumSubEquipWindowType.IntentyDetaiPanel) && Module_Equip.selectEquipSubType == EquipType.None)
            Module_Equip.selectEquipSubType = EquipType.Weapon;
        else if (Module_Equip.selectEquipType == EnumSubEquipWindowType.ChangeNonIntentyEquipPanel)
        {
            Module_Equip.accesssReturnWay = 2;
            m_subTypeLock = -1;
        }
        Module_Equip.OpenEquipSubWindow(Module_Equip.selectEquipType, Module_Equip.selectEquipSubType);
    }

    protected void OnRefreshButtonMark()
    {
        foreach (var item in m_panelDic)
        {
            if (item.Value != null) item.Value.RefreshButtonMark();
        }
    }

    protected override void OnReturn()
    {
        moduleHome.UpdateIconState(HomeIcons.Equipment, false);
        foreach (var item in m_panelDic)
        {
            if (item.Value.enable)
            {
                if (item.Value == m_equipMainPanel) Hide(true);
                else item.Value.OnReturnClick();
            }
        }
    }

    protected override void OnClose()
    {
        base.OnClose();
        foreach (var item in m_panelDic)
        {
            item.Value.Destory();
        }
    }

    public override void OnUpdate(int diff)
    {
        base.OnUpdate(diff);
        foreach (var item in m_panelDic)
        {
            if (item.Value != null && item.Value.canUpdate) item.Value.Update();
        }
    }
    #endregion

    #region subwindow  manage
    private void OpenChangeEquipPanel(EquipType type)
    {
        switch (type)
        {
            case EquipType.Weapon:
            case EquipType.Gun:
            case EquipType.Cloth:
                Module_Equip.OpenEquipSubWindow(EnumSubEquipWindowType.IntentyDetaiPanel, type);
                break;
            case EquipType.Hair:
            case EquipType.HeadDress:
            case EquipType.HairDress:
            case EquipType.FaceDress:
            case EquipType.NeckDress:
            case EquipType.Guise:
                Module_Equip.OpenEquipSubWindow(EnumSubEquipWindowType.ChangeNonIntentyEquipPanel);
                break;
        }
    }

    private void OpenSubWindow(Event_ e)
    {
        enableUpdate = false;
        EnumSubEquipWindowType type = (EnumSubEquipWindowType)e.param1;
        foreach (var item in m_panelDic)
        {
            if (item.Value == null) continue;
            item.Value.SetPanelVisible(item.Key == type);
        }

        uiCharacter.enableDrag = type != EnumSubEquipWindowType.IntentyDetaiPanel;
        //special handle
        switch (type)
        {
            case EnumSubEquipWindowType.IntentyDetaiPanel:
                ((IntentyEquipDetailPanel)m_panelDic[type]).OnSwitchEquipType((EquipType)e.param2);

                break;

            case EnumSubEquipWindowType.ChangeIntentyEquipPanel:
                ((ChangeIntentyEquipPanel)m_panelDic[type]).RefreshPanel((EquipType)e.param2);
                break;
        }
    }
    #endregion

    #region camera control

    private void ControlCamera(ushort curItemTypeId, ushort gotoItemTypeId)
    {
        if (curItemTypeId == 0) ResetCameraToInit();
        else ShowCameraView(curItemTypeId, gotoItemTypeId);
    }

    private void StopAllEquipAnim()
    {
        m_cameraAnimTween?.Kill();
        m_playerRotTween?.Kill();
        DelayEvents.Remove(m_delayCameraAnimId);
        DelayEvents.Remove(m_delayPlayerRotId);
    }

    /// <summary>
    ///还原摄像机镜头
    /// </summary>
    private void ResetCameraToInit()
    {
        StopAllEquipAnim();
        float time = 0.2f;
        if (m_mainCamra)
        {
            m_cameraAnimTween = m_mainCamra.transform.DOLocalMove(m_cameraVectors[0], time);
            m_cameraRotationAnimTween = m_mainCamra.transform.DOLocalRotate(m_cameraVectors[1], time);
        }

        ChangePlayerState("StateSimpleIdle");
        if (m_player) m_playerRotTween = m_player.transform.DOLocalRotate(new Vector3(0f, 90f, 0f), time);

        OnTweenEnd();
    }

    /// <summary>
    /// 开始摄像头的动画显示
    /// </summary>
    private void ShowCameraView(ushort curItemTypeId, ushort gotoItemTypeId)
    {
        m_displayAnimData = null;
        m_displayStateData = null;

        var animData = ConfigManager.Get<EquipAnimationInfo>(modulePlayer.proto);
        if (animData == null || animData.animDatas == null || animData.animDatas.Length < 1) return;

        m_displayAnimData = animData.animDatas.Find(p => p.nowIds.Contains(curItemTypeId));
        if (m_displayAnimData == null || m_displayAnimData.gotoDatas == null || m_displayAnimData.gotoDatas.Length < 1)
        {
            ResetCameraToInit();
            return;
        }

        m_displayStateData = m_displayAnimData.gotoDatas.Find(p => p.gotoIds.Contains(gotoItemTypeId));
        if (m_displayStateData == null)
        {
            ResetCameraToInit();
            return;
        }

        StopAllEquipAnim();

        SetDetailPanelInteractable(false);
        ChangePlayerState();
        PlayPlayerRotTween();
        PlayCameraTween();
    }

    private void ChangePlayerState()
    {
        var state = string.IsNullOrEmpty(m_displayStateData.state) ? "StateSimpleIdle" : m_displayStateData.state;
        ChangePlayerState(m_displayStateData.state);
    }

    private void ChangePlayerState(string state)
    {
        if (!m_player) m_player = ObjectManager.FindObject<Creature>(c => c.isPlayer);

        if (m_player)
        {
            var curState = m_player.stateMachine?.currentState?.name;
            if (!state.Equals(curState) && !string.IsNullOrEmpty(state)) m_player.stateMachine?.TranslateTo(state);
        }
    }

    private void PlayPlayerRotTween()
    {
        if (m_displayStateData == null) return;

        DelayEvents.Remove(m_delayPlayerRotId);
        m_delayPlayerRotId = 0;
        if (m_displayStateData.playerRotDelay <= 0f) _PlayPlayerRotTween();
        else m_delayPlayerRotId = DelayEvents.Add(_PlayPlayerRotTween, m_displayStateData.playerRotDelay);
    }

    private void _PlayPlayerRotTween()
    {
        if (!m_player) return;

        //每20度的旋转时间
        float playerY = m_player.localEulerAngles.y;
        playerY = playerY < 0 ? playerY + 360 : playerY;
        float dataY = m_displayStateData.playerEndRotate.y;
        dataY = dataY < 0 ? dataY + 360 : dataY;
        float duraction = Mathf.Abs(playerY - dataY) * m_displayStateData.playerRotUnitTime * 0.05f;
        //Logger.LogDetail("player rotationtween --- ori is {0}  tar is {1} duraction is {2}", m_player.localEulerAngles.y, m_displayAnimData.playerEndRotate.y, duraction);
        if (m_playerRotTween != null) DOTween.Kill(m_playerRotTween);
        m_playerRotTween = m_player.transform.DOLocalRotate(m_displayStateData.playerEndRotate, duraction).OnComplete(OnTweenEnd);
    }

    private void PlayCameraTween()
    {
        if (m_displayStateData == null) return;

        DelayEvents.Remove(m_delayCameraAnimId);
        m_delayCameraAnimId = 0;
        if (m_displayStateData.cameraDelay <= 0f) _PlayCameraTween();
        else m_delayCameraAnimId = DelayEvents.Add(_PlayCameraTween, m_displayStateData.cameraDelay);
    }

    private void _PlayCameraTween()
    {
        if (!m_mainCamra) return;

        float duraction = Vector3.Distance(m_mainCamra.transform.localPosition, m_displayStateData.cameraEndPos) * m_displayStateData.cameraUnitTime;
        if (m_cameraAnimTween != null) DOTween.Kill(m_cameraAnimTween);
        if (m_cameraRotationAnimTween != null) DOTween.Kill(m_cameraRotationAnimTween);
        m_cameraAnimTween = m_mainCamra.transform.DOLocalMove(m_displayStateData.cameraEndPos, duraction);
    }

    private void OnTweenEnd()
    {
        SetDetailPanelInteractable(true);
    }

    private void SetDetailPanelInteractable(bool enable)
    {
        var panel = m_panelDic.Get(EnumSubEquipWindowType.IntentyDetaiPanel);
        if (panel != null && panel.activeInHierarchy) ((IntentyEquipDetailPanel)panel).SetButtonInteractable(enable);
    }

    #endregion

    #region module_ME
    private void _ME(ModuleEvent<Module_Equip> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Equip.EventOpenSubPanel:    if (actived) OpenSubWindow(e);               break;
            case Module_Equip.EventCameraView: if (actived) ControlCamera((ushort)e.param1, (ushort)e.param2); break;
            case Module_Equip.EventCameraPause:     if (actived) StopAllEquipAnim();             break;

            //背包数据准备好
            //case Module_Equip.EventBagInfo: if (actived) RefreshCurrentDress(); break;
            //换装刷新
            case Module_Equip.EventRefreshCurrentDress:
                RefreshCurrentDress();
                CustomSecondPanel panel = m_panelDic.Get(EnumSubEquipWindowType.IntentyDetaiPanel);
                if (panel != null && panel.enable) ((IntentyEquipDetailPanel)panel).RefreshPanel(true);
                break;

            case Module_Equip.EventItemDataChange:
                if (m_equipMainPanel.enable) m_equipMainPanel.RefreshMainItem();
                break;
        }
    }

    private void RefreshCurrentDress()
    {
        if (uiCharacter.dragTarget != null && uiCharacter.dragTarget.gameObject.activeInHierarchy)
        {
            if (!m_player) m_player = ObjectManager.FindObject<Creature>(c => c.isPlayer);

            if (m_player)
            {
                //change weapon
                PItem p = moduleEquip.GetDressedPropExcept(PropType.Weapon, (int)WeaponSubType.Gun);
                var weapon = p.GetPropItem();
                if (weapon)
                {
                    Level.PrepareAssets(Module_Battle.BuildWeaponSimplePreloadAssets(modulePlayer.proto, modulePlayer.gender, weapon.subType, p.itemTypeId), (r) =>
                    {
                        if (!r) return;
                        m_player.UpdateWeapon(weapon.subType, weapon.ID);
                    });
                }

                //change gun
                p = moduleEquip.GetDressedProp(PropType.Weapon, (int)WeaponSubType.Gun);
                var gun = p.GetPropItem();
                if (gun)
                {
                    var gunInfo = WeaponInfo.GetWeapon(gun.subType, gun.ID);
                    Level.PrepareAssets(gunInfo.GetAllAssets(), (r) =>
                    {
                        if (!r) return;
                        m_player.UpdateWeapon(gun.subType, gun.ID, true);
                    });
                }

                //change cloth
                CharacterEquip.ChangeCloth(m_player, moduleEquip.currentDressClothes);
            }
        }
    }

    //private void _ME(ModuleEvent<Module_Player> e)
    //{
    //    if (e.moduleEvent == Module_Player.EventCurrencyChanged)
    //    {
    //        var t = (CurrencySubType)e.param1;
    //        if (t == CurrencySubType.Gold)
    //        {
    //            var inten = m_panelDic.Get(EnumSubEquipWindowType.IntentyPanel);
    //            var evolve = m_panelDic.Get(EnumSubEquipWindowType.EvolvePanel);
    //            //var enchant = m_panelDic.Get(EnumSubEquipWindowType.EnchantPanel);

    //            if (inten != null && inten.enable)              (inten as IntentifyPanel).RefreshTotalExp();
    //            else if(evolve != null && evolve.enable)        (evolve as EvolvePanel).RefreshSelf();
    //            //else if (enchant != null && enchant.enable) (inten as IntentifyPanel).RefreshTotalExp();
    //        }
    //    }
    //}
    #endregion
}

#region custom class

#region enum

public enum EnumSubEquipWindowType
{
    /// <summary>
    /// 主界面
    /// </summary>
    MainPanel,
    /// <summary>
    /// 可强化装备（武器，枪，服饰）展示页面
    /// </summary>
    IntentyDetaiPanel,
    /// <summary>
    /// 可强化装备（武器，枪，服饰）换装界面
    /// </summary>
    ChangeIntentyEquipPanel,
    /// <summary>
    /// 不可强化装备（发型，视频）换装界面
    /// </summary>
    ChangeNonIntentyEquipPanel,
}

/// <summary>
/// 装备操作的类型
/// </summary>
public enum EnumOperateEquipType
{
    None,

    /// <summary>
    /// 强化
    /// </summary>
    Intenty,

    /// <summary>
    /// 进阶
    /// </summary>
    Evolve,

    Count,
}

#endregion

#region equip main panel / weapon detail panel

public sealed class EquipMainPanel : CustomSecondPanel
{
    public EquipMainPanel(Transform trans, Action<EquipType> c) : base(trans)
    {
        callback = c;
    }

    private Text m_playerNameText;
    private Text m_levelText;
    private Image avatar;
    private EquipMainBtnItem m_weaponItem;
    private EquipMainBtnItem m_gunItem;
    private EquipMainBtnItem m_suitItem;
    private MarkButton m_decorationBtn;

    private Module_Equip moduleEquip;
    private Action<EquipType> callback;

    public override void InitComponent()
    {
        base.InitComponent();
        moduleEquip = Module_Equip.instance;

        m_playerNameText = transform.GetComponent<Text>("playerinfo/name");
        m_levelText = transform.GetComponent<Text>("playerinfo/lvl");
        avatar = transform.GetComponent<Image>("playerinfo/avatar/head_icon");
        m_decorationBtn = new MarkButton(transform.Find("decoration_btn"));

        m_weaponItem = new EquipMainBtnItem(transform.Find("weapon_btn"), 12);
        m_gunItem = new EquipMainBtnItem(transform.Find("gun_btn"), 13);
        m_suitItem = new EquipMainBtnItem(transform.Find("suit_btn"), 14);

        RefreshMainItem();
    }

    public override void AddEvent()
    {
        base.AddEvent();
        EventTriggerListener.Get(m_weaponItem.gameObject).onClick = OnBtnClick;
        EventTriggerListener.Get(m_gunItem.gameObject).onClick = OnBtnClick;
        EventTriggerListener.Get(m_suitItem.gameObject).onClick = OnBtnClick;
        EventTriggerListener.Get(m_decorationBtn.gameObject).onClick = OnBtnClick;
    }

    private void SetSelfInfo()
    {
        PRoleInfo r = Module_Player.instance.roleInfo;
        m_playerNameText.text = r.roleName;
        Util.SetText(m_levelText, Util.Format(ConfigText.GetDefalutString(TextForMatType.EquipUIText, 3), r.level));
        Module_Avatar.SetPlayerAvatar(avatar.gameObject);
    }

    protected override void _OnBecameVisible()
    {
        SetSelfInfo();
        RefreshMainItem();
        //reset camera
        Module_Equip.SetCameraView();
        Module_Home.instance.HideOthers("player");
    }

    public void RefreshMainItem()
    {
        m_weaponItem.RefreshItem(moduleEquip.GetDressedProp(EquipType.Weapon));
        m_gunItem.RefreshItem(moduleEquip.GetDressedProp(EquipType.Gun));
        m_suitItem.RefreshItem(moduleEquip.GetDressedProp(EquipType.Cloth));

        bool wear = Module_Cangku.instance.TypeHint(EquipType.HeadDress) || Module_Cangku.instance.TypeHint(EquipType.HairDress) || Module_Cangku.instance.TypeHint(EquipType.FaceDress) || Module_Cangku.instance.TypeHint(EquipType.NeckDress);
        bool newDecoration = Module_Cangku.instance.TypeHint(EquipType.Guise) || wear;
        m_decorationBtn.SetMarkVisible(newDecoration);
    }

    public void OnBtnClick(GameObject sender)
    {
        EquipType t = EquipType.None;
        if (sender == m_weaponItem.gameObject) t = EquipType.Weapon;
        else if (sender == m_gunItem.gameObject) t = EquipType.Gun;
        else if (sender == m_suitItem.gameObject) t = EquipType.Cloth;
        else if (sender == m_decorationBtn.gameObject) t = EquipType.Hair;

        callback?.Invoke(t);
    }
}

public sealed class IntentyEquipDetailPanel : CustomSecondPanel
{
    public IntentyEquipDetailPanel(Transform trans) : base(trans) { }

    private Module_Equip moduleEquip;
    private string[] m_iconNames = new string[] { "sword", "katana", "axe", "fist", "pistol", "suit" };
    private string[] m_toggleNames = new string[] { "richang", "qiangxie", "suit" };

    private List<Image> m_icons = new List<Image>();
    private List<Image> m_stars = new List<Image>();
    private Text m_nameText, m_levelText;
    private MarkButton m_replaceBtn, m_detailBtn, m_weaponBtn, m_gunBtn, m_suitBtn;

    private List<Toggle> m_toggles = new List<Toggle>();
    private UICharacter m_uiCharacter;
    private RawImage m_weaponRawImage;
    private ForgePreviewPanel m_previewPanel;

    private Camera m_mainCamera;
    private Camera m_cloneCamera;

    public PItem pitemData { get; private set; }
    public PropItemInfo info { get; private set; }
    public EquipType equipType { get; private set; }
    public EquipType lastEquipType { get; private set; }
    public PreviewEquipType currentPreviewType { get; private set; }

    public override void InitComponent()
    {
        base.InitComponent();
        moduleEquip = Module_Equip.instance;
        m_icons.Clear();
        foreach (var item in m_iconNames)
        {
            m_icons.Add(transform.GetComponent<Image>(Util.Format("right/bground/base/icon/{0}", item)));
        }

        m_stars.Clear();
        m_stars.AddRange(transform.Find("right/bground/base/qualityGrid")?.GetComponentsInChildren<Image>(true));

        m_uiCharacter = transform.GetComponent<UICharacter>("right/RawImage");
        m_weaponRawImage = transform.GetComponent<RawImage>("right/RawImage");
        m_weaponRawImage.raycastTarget = true;
        m_uiCharacter.enableDrag = true;
        m_nameText = transform.GetComponent<Text>("right/bground/base/name");
        m_levelText = transform.GetComponent<Text>("right/bground/base/name/level_txt");

        m_previewPanel = transform.GetComponent<ForgePreviewPanel>("right");
        m_replaceBtn = new MarkButton(transform.Find("right/change_btn"));
        m_detailBtn = new MarkButton(transform.Find("right/info_btn"));
        m_weaponBtn = new MarkButton(transform.Find("right/checkBox/richang"));
        m_gunBtn = new MarkButton(transform.Find("right/checkBox/qiangxie"));
        m_suitBtn = new MarkButton(transform.Find("right/checkBox/suit"));

        m_toggles.Clear();
        foreach (var item in m_toggleNames)
        {
            Toggle t = transform.GetComponent<Toggle>(Util.Format("right/checkBox/{0}", item));
            m_toggles.Add(t);
        }
    }

    public override void AddEvent()
    {
        base.AddEvent();
        if (null != m_replaceBtn) EventTriggerListener.Get(m_replaceBtn.gameObject).onClick = OnReplaceClick;
        if (null != m_detailBtn) EventTriggerListener.Get(m_detailBtn.gameObject).onClick = OnDetailBtnClick;
        EventTriggerListener.Get(m_weaponRawImage.gameObject).onClick = OnDetailBtnClick;
        foreach (var item in m_toggles)
        {
            EventTriggerListener.Get(item.gameObject).onClick = OnToggleValueClick;
        }
    }

    public void OnSwitchEquipType(EquipType type)
    {
        lastEquipType = equipType;
        equipType = type;

        for (int i = 0; i < m_toggles.Count; i++)
        {
            m_toggles[i].isOn = i == (int)equipType - 1; ;
        }
        RefreshPanel();
    }

    protected override void _OnBecameVisible()
    {
        base._OnBecameVisible();
        ResetCamera(true);
        RefreshPanel();
    }

    public void RefreshPanel(bool isChangeEquip = false)
    {
        if (equipType == EquipType.None) return;
        PItem data = moduleEquip.GetDressedProp(equipType);
        RefreshPanel(data);

        m_detailBtn?.SetMarkVisible(Module_Equip.HasAnyEquipOperation(data));
        m_replaceBtn?.SetMarkVisible(Module_Cangku.instance.TypeHint(equipType));

        m_weaponBtn?.SetMarkVisible(GetBtnMarkState(EquipType.Weapon));
        m_gunBtn?.SetMarkVisible(GetBtnMarkState(EquipType.Gun));
        m_suitBtn?.SetMarkVisible(GetBtnMarkState(EquipType.Cloth));

        if (lastEquipType == equipType && !isChangeEquip) return;

        ushort gotoItemTypeId = 0;
        if (lastEquipType != equipType)
        {
            if (lastEquipType == EquipType.None) lastEquipType = equipType;
            var lastData = moduleEquip.GetDressedProp(lastEquipType);
            if (lastData != null) moduleEquip.lastItemTypeId = lastData.itemTypeId;

            if (data != null) gotoItemTypeId = data.itemTypeId;
    }

        Module_Equip.SetCameraView(moduleEquip.lastItemTypeId, gotoItemTypeId);
    }

    private bool GetBtnMarkState(EquipType type)
    {
        PItem data = moduleEquip.GetDressedProp(type);

        return Module_Equip.HasAnyEquipOperation(data) || Module_Cangku.instance.TypeHint(type);
    }

    private void RefreshPanel(PItem item)
    {
        pitemData = item;
        info = pitemData?.GetPropItem();
        if (pitemData == null || !info) return;

        currentPreviewType = moduleEquip.GetCurrentPreType(equipType, pitemData.GetIntentyLevel(), pitemData.HasEvolved());
        RefreshBaseDetail();
        Util.SetEquipTypeIcon(m_icons, info.itemType, info.subType);
        InitCamera();
        moduleEquip.LoadModel(item, Layers.GROUND, false);
        if (m_previewPanel) m_previewPanel.ForingItem(item, false);
    }

    public override void SetPanelVisible(bool visible = true)
    {
        base.SetPanelVisible(visible);

        ResetCamera(visible);
        if (visible)
        {
            Module_Global.instance.ShowGlobalLayerDefault();
            lastEquipType = EquipType.None;
        }
    }

    private void RefreshBaseDetail()
    {
        m_nameText.text = info.itemName;
        Util.SetText(m_levelText, Util.Format(ConfigText.GetDefalutString(TextForMatType.EquipUIText, 9), pitemData.GetIntentyLevel()));
        m_levelText?.gameObject.SetActive(pitemData.GetIntentyLevel() > 0);
        int star = info.quality;
        for (int i = 0; i < m_stars.Count; i++) m_stars[i].gameObject.SetActive(i < star);
    }

    private void OnToggleValueClick(GameObject sender)
    {
        EquipType t = sender.Equals(m_toggles[0].gameObject) ? EquipType.Weapon : sender.Equals(m_toggles[1].gameObject) ? EquipType.Gun : EquipType.Cloth;

        if (t == EquipType.None || t == equipType) return;
        OnSwitchEquipType(t);
    }

    private void OnReplaceClick(GameObject sender)
    {
        lastEquipType = equipType;
        Module_Equip.OpenEquipSubWindow(EnumSubEquipWindowType.ChangeIntentyEquipPanel, equipType);
    }

    private void OnDetailBtnClick(GameObject sender)
    {
        if (pitemData == null)
        {
            Logger.LogError("current data is  null,please check out....");
            return;
        }
        Module_Equip.PauseCameraAnimation();
       lastEquipType = EquipType.None;
        Module_Cangku.instance.m_detailItem = pitemData;
        Window.ShowAsync<Window_Equipinfo>();
    }

    public override void OnReturnClick()
    {
        Module_Equip.OpenEquipSubWindow(EnumSubEquipWindowType.MainPanel);
        lastEquipType = EquipType.None;
        equipType = EquipType.None;
    }

    public void SetButtonInteractable(bool enable)
    {
        m_replaceBtn.button.targetGraphic.raycastTarget = enable;
        m_detailBtn.button.targetGraphic.raycastTarget = enable;
        foreach (var item in m_toggles)
        {
            item.targetGraphic.raycastTarget = enable;
        }
    }

    #region camera control

    private void InitCamera()
    {
        if (m_cloneCamera) return;

        m_mainCamera = Camera.main;
        if (!m_mainCamera) return;
        Transform t = m_mainCamera.transform.parent.AddNewChild(m_mainCamera.gameObject);
        Util.ClearChildren(t);
        Component[] cs = t.GetComponents<Component>();
        foreach (var item in cs)
        {
            if (item is Camera || item is Transform || item is PostProcessingBehaviour) continue;
            UnityEngine.Object.Destroy(item);
        }
        t.name = "weapon_camera";

        t.position = m_mainCamera.transform.position;
        t.rotation = m_mainCamera.transform.rotation;
        t.localScale = m_mainCamera.transform.localScale;
        t.tag = UnitSceneEventData.Untag;

        m_cloneCamera = t.GetComponent<Camera>();
        m_cloneCamera.cullingMask = 1 << Layers.GROUND;

        m_cloneCamera.gameObject.SetActive(true);
        {
            m_cloneCamera.targetTexture = UICharacter.GetCachedTexture(m_cloneCamera);
            m_uiCharacter.Initialize();
            m_uiCharacter.cameraName = t.name;
        }

        var showInfo = ConfigManager.Get<ShowCreatureInfo>(998);
        if (showInfo)
        {
            var cameraData = showInfo.GetDataByIndex(0);
            var sp = cameraData == null ? null : cameraData.data.GetValue<ShowCreatureInfo.SizeAndPos>(0);
            if (sp != null)
            {
                t.position = sp.pos;
                t.localEulerAngles = sp.rotation;
                t.localScale = new Vector3(sp.size, sp.size, sp.size);
            }
        }
    }

    public void ResetCamera(bool visible)
    {
        if (!visible) Module_Home.instance.HideOthersBut(Module_Home.PLAYER_OBJECT_NAME);
        if (m_cloneCamera) m_cloneCamera.SafeSetActive(visible);
    }

    public override void Destory()
    {
        base.Destory();
        if (m_cloneCamera)
        {
            UICharacter.RemoveCachedTexture(m_cloneCamera);
            GameObject.Destroy(m_cloneCamera.gameObject);
        }
    }
    #endregion
}

#endregion

#region change cloth panel

public class BaseChangeEquipPanel : CustomSecondPanel
{
    public BaseChangeEquipPanel(Transform trans) : base(trans) { }

    protected Level_Home m_levelHome;
    protected Module_Equip moduleEquip;

    protected DataSource<PItem> m_dataSource;
    protected ScrollView m_scroll;
    protected MarkButton m_detailBtn;

    protected Text m_selectName;
    protected RectTransform m_selectTrans;
    protected PItem m_selectData;

    public override void InitComponent()
    {
        base.InitComponent();
        moduleEquip = Module_Equip.instance;
        m_scroll = transform.GetComponent<ScrollView>("right/scrollView");
        m_detailBtn = new MarkButton(transform.Find("right/info"));
        m_selectName = transform.GetComponent<Text>("right/name_txt");
        Util.SetText(m_selectName, string.Empty);
        CreateDataSource();
    }

    public override void AddEvent()
    {
        base.AddEvent();
        EventTriggerListener.Get(m_detailBtn.gameObject).onClick = OnDetailClick;
    }

    #region data source
    protected virtual List<PItem> GetAllItems()
    {
        return new List<PItem>();
    }

    protected void CreateDataSource()
    {
        m_dataSource = new DataSource<PItem>(new List<PItem>(), m_scroll, RefreshItem, OnItemClick);
    }

    protected virtual void RefreshItem(RectTransform tr, PItem info) { }
    protected virtual void OnItemClick(RectTransform tr, PItem info)
    {
        m_selectTrans = tr;
        m_selectData = info;

        if (tr) tr.Find("new")?.SafeSetActive(false);
        Util.SetText(m_selectName, info.GetPropItem().itemName);
    }
    #endregion

    #region Clickevent

    private void OnDetailClick(GameObject sender)
    {
        if (m_selectData != null)
        {
            var t = Module_Equip.GetEquipTypeByItem(m_selectData);
            if (t == EquipType.Weapon || t == EquipType.Gun || t == EquipType.Cloth)
            {
                Module_Equip.PauseCameraAnimation();
                Module_Cangku.instance.m_detailItem = m_selectData;
                Window.ShowAsync<Window_Equipinfo>();
            }
            else
            {
                Module_Global.instance.UpdateGlobalTip(m_selectData.itemTypeId);
            }
        }
    }
    #endregion
}

public sealed class ChangeIntentyEquipPanel : BaseChangeEquipPanel
{
    public ChangeIntentyEquipPanel(Transform trans) : base(trans) { }

    //top
    private Text m_nameText;
    private Text m_elementText;
    private string[] m_iconNames = new string[] { "sword", "katana", "axe", "fist", "pistol", "suit" };
    private List<Image> m_icons = new List<Image>();

    //top_suit
    private GameObject m_noneSuitObj;
    private SuitProperty m_haveSuitProperty;

    //bottom
    private GameObject m_attriPrefab;
    private Transform m_attriParent;
    private List<AttributeItem> m_attributes = new List<AttributeItem>();

    //right
    private Button m_equipBtn;
    private PItem m_currentItem;
    private EquipType m_equipType;
    private Text m_titleText;
    private string[] m_titles = new string[]
    {
        ConfigText.GetDefalutString(TextForMatType.EquipUIText,17),
        ConfigText.GetDefalutString(TextForMatType.EquipUIText,18),
        ConfigText.GetDefalutString(TextForMatType.EquipUIText,19),
    };

    public override void InitComponent()
    {
        base.InitComponent();
        m_nameText = transform.GetComponent<Text>("left/top/name");
        m_elementText = transform.GetComponent<Text>("left/top/name/element");
        m_icons.Clear();
        foreach (var item in m_iconNames)
        {
            m_icons.Add(transform.GetComponent<Image>(Util.Format("left/top/icon/{0}", item)));
        }

        Transform t = transform.Find("left/top/suit/nothing");
        if (t) m_noneSuitObj = t.gameObject;

        t = transform.Find("left/top/suit/have");
        if (t) m_haveSuitProperty = new SuitProperty(t);

        m_attriParent = transform.Find("left/bottom");
        m_attriPrefab = transform.Find("left/bottom/attribute")?.gameObject;
        m_attriPrefab?.SetActive(false);
        m_attributes.Clear();

        m_equipBtn = transform.GetComponent<Button>("right/equip");
        m_titleText = transform.GetComponent<Text>("right/title_txt");
    }

    public override void AddEvent()
    {
        base.AddEvent();
        EventTriggerListener.Get(m_equipBtn.gameObject).onClick = OnEquipClick;
    }

    protected override void _OnBecameVisible()
    {
        base._OnBecameVisible();
        Module_Home.instance.HideOthers("player");
        if (m_currentItem != null) RefreshLeft(m_currentItem);
    }

    public override void OnReturnClick()
    {
        base.OnReturnClick();
        Module_Equip.OpenEquipSubWindow(EnumSubEquipWindowType.IntentyDetaiPanel, m_equipType);
    }

    private string GetTitle()
    {
        switch (m_equipType)
        {
            case EquipType.Weapon:  return m_titles[0];
            case EquipType.Gun: return m_titles[1];
            case EquipType.Cloth: return m_titles[2];
        }
        return string.Empty;
    }

    #region data source
    protected override List<PItem> GetAllItems()
    {
        List<PItem> l = new List<PItem>();
        l.Add(moduleEquip.GetDressedProp(m_equipType));
        l.AddRange(moduleEquip.GetBagDress(m_equipType));
        return l;
    }

    protected override void RefreshItem(RectTransform tr, PItem info)
    {
        base.RefreshItem(tr, info);

        int level = info.GetIntentyLevel();
        PreviewEquipType t = moduleEquip.GetCurrentPreType(m_equipType, level, info.HasEvolved());
        var config = info?.GetPropItem();
        if (config == null) return;
        //int star = m_equipType == EquipType.Weapon ? info.growAttr.equipAttr.star : config.quality;
        int star = config.quality;

        Util.SetItemInfo(tr, config, level, 0, true, star);

        GameObject equiped = tr.Find("Image").gameObject;
        equiped?.SetActive(info == m_currentItem);

        GameObject maxObj = tr.Find("levelmax").gameObject;
        maxObj?.SetActive(t == PreviewEquipType.Enchant);

        GameObject selectObj = tr.Find("checkbox")?.gameObject;
        selectObj?.SetActive(info == m_selectData);

        GameObject newObj = tr.Find("new").gameObject;
        newObj?.SetActive(info != m_currentItem && Module_Cangku.instance && Module_Cangku.instance.NewsProp(info.itemId));

        GameObject lockObj = tr.Find("lock")?.gameObject;
        lockObj?.SetActive(info.isLock > 0);

        if (m_selectTrans == null && info == m_currentItem)
        {
            base.OnItemClick(tr, info);
            RefreshAttibutes();
            selectObj?.SetActive(true);
            m_equipBtn.SetInteractable(false);
            m_detailBtn.SetMarkVisible(Module_Equip.HasAnyEquipOperation(m_selectData));
        }
    }

    protected override void OnItemClick(RectTransform tr, PItem info)
    {
        if (info == m_selectData) return;

        base.OnItemClick(tr, info);
        m_equipBtn.SetInteractable(info != m_currentItem);
        m_dataSource?.UpdateItems();
        RefreshLeft(m_selectData);
        m_detailBtn.SetMarkVisible(Module_Equip.HasAnyEquipOperation(m_selectData));
    }
    #endregion

    #region public functions
    public void RefreshPanel(EquipType type)
    {
        m_selectData = null;
        m_selectTrans = null;
        m_equipType = type;
        m_currentItem = moduleEquip.GetDressedProp(m_equipType);
        m_detailBtn.SetMarkVisible(false);

        Util.SetText(m_titleText, GetTitle());
        m_dataSource?.SetItems(GetAllItems());
        m_dataSource?.UpdateItems();
        RefreshLeft(m_currentItem);
        Module_Cangku.instance.RemveNewItem(type);
    }
    #endregion

    #region left panel
    private void RefreshLeft(PItem item)
    {
        PropItemInfo info = item?.GetPropItem();
        if (!info) return;
        Util.SetEquipTypeIcon(m_icons, info.itemType, info.subType);
        m_nameText.text = info.itemName;
        SetElementText(item);
        RefreshSuit(item);
        RefreshAttibutes();
    }

    private void SetElementText(PItem item)
    {
        WeaponAttribute weaponAttributes = ConfigManager.Get<WeaponAttribute>(item.itemTypeId);
        //名字
        string weaAttr = string.Empty;
        CreatureElementTypes subtype = weaponAttributes ? (CreatureElementTypes)weaponAttributes.elementType : CreatureElementTypes.Count;
        switch (subtype)
        {
            case CreatureElementTypes.Wind: weaAttr = ConfigText.GetDefalutString(TextForMatType.EquipUIText, 33); break;
            case CreatureElementTypes.Fire: weaAttr = ConfigText.GetDefalutString(TextForMatType.EquipUIText, 34); break;
            case CreatureElementTypes.Water: weaAttr = ConfigText.GetDefalutString(TextForMatType.EquipUIText, 35); break;
            case CreatureElementTypes.Thunder: weaAttr = ConfigText.GetDefalutString(TextForMatType.EquipUIText, 36); break;
            case CreatureElementTypes.Ice: weaAttr = ConfigText.GetDefalutString(TextForMatType.EquipUIText, 37); break;
        }
        Util.SetText(m_elementText, weaAttr);
    }

    public void RefreshSuit(PItem item)
    {
        int suitId = item.GetSuitId();
        bool dress = moduleEquip.IsDressOn(item);

        if (m_noneSuitObj) m_noneSuitObj.SafeSetActive(suitId <= 0);
        if (m_haveSuitProperty != null)
        {
            m_haveSuitProperty.Root.SafeSetActive(suitId > 0);
            if (suitId > 0) m_haveSuitProperty.Init(suitId, moduleEquip.GetSuitNumber(suitId), dress);
        }
    }

    private void RefreshAttibutes()
    {
        foreach (var item in m_attributes)
        {
            item.gameObject.SetActive(false);
        }
        if (m_selectData == null) return;
        var l = AttributePreviewDetail.GetAllChangeAttributes(m_currentItem, m_selectData);
        CreateNewAttriItem(l.Count - m_attributes.Count);
        for (int i = 0; i < l.Count; i++)
        {
            m_attributes[i].RefreshDetail(l[i]);
            m_attributes[i].SetRightEnable(m_currentItem != m_selectData);
        }
    }

    private void CreateNewAttriItem(int count)
    {
        if (count <= 0) return;
        for (int i = 0; i < count; i++)
        {
            Transform t = m_attriParent.AddNewChild(m_attriPrefab);
            t.gameObject.SetActive(false);
            m_attributes.Add(new AttributeItem(t));
        }
    }
    #endregion

    #region click event

    private void OnEquipClick(GameObject sender)
    {
        if (m_selectData != m_currentItem && m_selectData != null)
        {
            List<ulong> up = new List<ulong>();
            up.Add(m_selectData.itemId);
            moduleEquip.SendChangeClothes(up);
        }
        Module_Equip.OpenEquipSubWindow(EnumSubEquipWindowType.IntentyDetaiPanel, m_equipType);
    }
    #endregion
}

public sealed class ChangeNonIntentyEquipPanel : BaseChangeEquipPanel
{
    public ChangeNonIntentyEquipPanel(Transform trans, Creature player, Camera uiCamera) : base(trans)
    {
        m_player = player;
        m_uiCamera = uiCamera;
    }

    //当前装扮的外观
    private PItem m_currentGuise;
    //当前装扮的饰品
    private PItem m_currentHead;
    private PItem m_currentHair;
    private PItem m_currentFace;
    private PItem m_currentNeck;

    //当前选择装备的额发型
    private PItem m_previewGuise;
    private RectTransform m_previewGuiseTrans;
    //当前选择的饰品
    private PItem m_previewHead;
    private PItem m_previewHair;
    private PItem m_previewFace;
    private PItem m_previewNeck;
    private RectTransform m_previewHeadTrans;
    private RectTransform m_previewHairTrans;
    private RectTransform m_previewFaceTrans;
    private RectTransform m_previewNeckTrans;

    private Button m_saveBtn;
    private Creature m_player;
    private Camera m_uiCamera;
    private ShowCreatureInfo m_showCreatureInfo;
    private int m_cameraLevel;
    private Tween m_cameraTweenRot;
    private Tween m_cameraTweenPos;

    private Toggle m_headToggle;
    private Toggle m_hairToggle;
    private Toggle m_faceToggle;
    private Toggle m_neckToggle;
    private Toggle m_guiseToggle;
    private MarkButton m_headDressBtn;
    private MarkButton m_hairDressBtn;
    private MarkButton m_faceDressBtn;
    private MarkButton m_neckDressBtn;
    private MarkButton m_guiseBtn;
    private FashionSubType m_currentToggleType = FashionSubType.None;
    private FashionSubType m_lastToggleType = FashionSubType.None;
    private Text m_title;
    private string[] m_titleNames = new string[]
    {
        ConfigText.GetDefalutString(TextForMatType.EquipUIText,24),//服装
        ConfigText.GetDefalutString(TextForMatType.EquipUIText,40),//头饰
        ConfigText.GetDefalutString(TextForMatType.EquipUIText,41),//发饰
        ConfigText.GetDefalutString(TextForMatType.EquipUIText,42),//脸饰
        ConfigText.GetDefalutString(TextForMatType.EquipUIText,43),//颈饰
    };

    //左侧滑动相关
    private Button m_changeBig;
    private Button m_changeSmall;
    private Slider m_changeSlider;
    private Button m_resetBtn;
    //private Button m_goShopBtn;

    private List<PItem> curDress = new List<PItem>();

    public override void InitComponent()
    {
        base.InitComponent();
        m_saveBtn = transform.GetComponent<Button>("right/equip");
        m_headToggle = transform.GetComponent<Toggle>("checkBox/head");
        m_headDressBtn = new MarkButton(transform.Find("checkBox/head"));
        m_hairToggle = transform.GetComponent<Toggle>("checkBox/hair");
        m_hairDressBtn = new MarkButton(transform.Find("checkBox/hair"));
        m_faceToggle = transform.GetComponent<Toggle>("checkBox/face");
        m_faceDressBtn = new MarkButton(transform.Find("checkBox/face"));
        m_neckToggle = transform.GetComponent<Toggle>("checkBox/neck");
        m_neckDressBtn = new MarkButton(transform.Find("checkBox/neck"));
        m_guiseToggle = transform.GetComponent<Toggle>("checkBox/suit");
        m_guiseBtn = new MarkButton(transform.Find("checkBox/suit"));
        m_changeBig = transform.GetComponent<Button>("left/_jiahao");
        m_changeSmall = transform.GetComponent<Button>("left/_jianhao");
        m_changeSlider = transform.GetComponent<Slider>("left/slider");
        m_resetBtn = transform.GetComponent<Button>("left/chongzhi");
        m_title = transform.GetComponent<Text>("right/title_txt");
        //m_goShopBtn = transform.GetComponent<Button>("left/goshop");
        //m_goShopBtn.onClick.RemoveAllListeners();
        //m_goShopBtn.onClick.AddListener(delegate
        //{
        //    Module_Equip.accesssReturnWay = 1;
        //    if (CanReturn()) GoClothShop();
        //    else Window_Alert.ShowAlert(ConfigText.GetDefalutString(229, 3), true, true, true, OnSaveBtnClick, GoClothShop);
        //});

        m_showCreatureInfo = ConfigManager.Get<ShowCreatureInfo>(Module_Player.instance.proto);

        InitCurrentEquip();
    }

    public override void AddEvent()
    {
        base.AddEvent();
        m_saveBtn.onClick.RemoveAllListeners();
        m_saveBtn.onClick.AddListener(OnSaveBtnClick);

        EventTriggerListener.Get(m_guiseToggle.gameObject).onClick = OnGuiseToggleValueChange;
        EventTriggerListener.Get(m_headToggle.gameObject).onClick = OnHeadToggleValueChange;
        EventTriggerListener.Get(m_hairToggle.gameObject).onClick = OnHairToggleValueChange;
        EventTriggerListener.Get(m_faceToggle.gameObject).onClick = OnFaceToggleValueChange;
        EventTriggerListener.Get(m_neckToggle.gameObject).onClick = OnNeckToggleValueChange;
        EventTriggerListener.Get(m_changeBig.gameObject).onClick = OnChangeBigClick;
        EventTriggerListener.Get(m_changeSmall.gameObject).onClick = OnChangeSmallClick;
        EventTriggerListener.Get(m_resetBtn.gameObject).onClick = OnResetClick;
    }

    public override void SetPanelVisible(bool visible = true)
    {
        base.SetPanelVisible(visible);

        if (visible)
        {
            InitCurrentEquip();
            Util.SetText(m_selectName, string.Empty);
            m_detailBtn.SetMarkVisible(false);
            m_detailBtn.button.SetInteractable(false);
            m_currentToggleType = FashionSubType.ClothGuise;
            m_lastToggleType = FashionSubType.None;
            m_guiseToggle.isOn = true;

            Util.SetText(m_title, m_titleNames[0]);
            SetFashionClothState();
            RefreshScrollView();
            m_saveBtn.SetInteractable(CanSave());
            m_cameraLevel = GeneralConfigInfo.defaultConfig.defaultLevel;
            ChangeCameraData(m_cameraLevel);
            m_lastToggleType = FashionSubType.ClothGuise;
        }
    }

    private bool CanSave()
    {
        return m_currentHead != m_previewHead || m_currentHair != m_previewHair
            || m_currentFace != m_previewFace || m_currentNeck != m_previewNeck || m_previewGuise != m_currentGuise;
    }

    protected override void _OnBecameVisible()
    {
        base._OnBecameVisible();
        ChangeCameraData(m_cameraLevel);
    }

    private void RefreshScrollView()
    {
        m_dataSource?.SetItems(GetAllItems());
        m_dataSource?.UpdateItems();
        Module_Cangku.instance.RemveNewItem(m_lastToggleType);
    }

    #region data souorce

    protected override void RefreshItem(RectTransform tr, PItem info)
    {
        base.RefreshItem(tr, info);

        var prop = info?.GetPropItem();
        if (!prop) return;

        //set catch data first
        if (m_previewGuise == info && !m_previewGuiseTrans) m_previewGuiseTrans = tr;
        if (m_previewHead == info && !m_previewHeadTrans) m_previewHeadTrans = tr;
        if (m_previewHair == info && !m_previewHairTrans) m_previewHairTrans = tr;
        if (m_previewFace == info && !m_previewFaceTrans) m_previewFaceTrans = tr;
        if (m_previewNeck == info && !m_previewNeckTrans) m_previewNeckTrans = tr;

        m_detailBtn.button.SetInteractable(m_selectTrans);

        Util.SetItemInfo(tr, prop, 0, 0, true, prop.quality);

        GameObject checkBox = tr.Find("checkbox").gameObject;
        checkBox.SetActive(ShowEquip(info));

        GameObject equiped = tr.Find("equiped").gameObject;
        equiped.SetActive(ShowEquip(info));
        if (ShowEquip(info)) m_selectTrans = tr;

        GameObject maxObj = tr.Find("levelmax").gameObject;
        maxObj?.SetActive(false);

        GameObject newObj = tr.Find("new").gameObject;
        newObj?.SetActive(Module_Cangku.instance && Module_Cangku.instance.NewsProp(info.itemId));

        GameObject lockObj = tr.Find("lock")?.gameObject;
        lockObj?.SetActive(info.isLock > 0);
    }

    private bool ShowEquip(PItem info)
    {
        if (m_currentToggleType == FashionSubType.ClothGuise) return m_previewGuise == info;
        else if (m_currentToggleType == FashionSubType.HeadDress) return m_previewHead == info;
        else if (m_currentToggleType == FashionSubType.HairDress) return m_previewHair == info;
        else if (m_currentToggleType == FashionSubType.FaceDress) return m_previewFace == info;
        else if (m_currentToggleType == FashionSubType.NeckDress) return m_previewNeck == info;

        return false;
    }

    protected override void OnItemClick(RectTransform tr, PItem info)
    {
        base.OnItemClick(tr, info);
        Module_Cangku.instance.RemveNewItem(info.itemId);
        if (!m_player) m_player = ObjectManager.FindObject<Creature>("player");

        switch ((FashionSubType)info.GetPropItem().subType)
        {
            case FashionSubType.ClothGuise: OnGuiseClick(tr, info, info == m_previewGuise); break;
            case FashionSubType.HeadDress: OnHeadClick(tr, info, info == m_previewHead); break;
            case FashionSubType.HairDress: OnHairClick(tr, info, info == m_previewHair); break;
            case FashionSubType.FaceDress: OnFaceClick(tr, info, info == m_previewFace); break;
            case FashionSubType.NeckDress: OnNeckClick(tr, info, info == m_previewNeck); break;
        }
        
        m_detailBtn.button.SetInteractable(m_selectTrans);
        m_dataSource.UpdateItems();
        m_saveBtn.SetInteractable(CanSave());
    }

    private void OnGuiseClick(RectTransform rectTrans, PItem item, bool equip)
    {
        m_previewGuise =  item;
        m_previewGuiseTrans =  rectTrans;
        if (equip)
        {
            m_selectTrans = null;
            m_selectData = null;
            m_previewGuise = null;
            m_previewGuiseTrans = null;
        }
        if (m_player) CharacterEquip.ChangeCloth(m_player, GetPreviewDress());
        Util.SetText(m_selectName, m_previewGuise == null ? string.Empty : m_previewGuise.GetPropItem().itemName);
    }

    private void OnHeadClick(RectTransform rectTrans, PItem item, bool equip)
    {
        m_previewHead = item;
        m_previewHeadTrans = rectTrans;
        if (equip)
        {
            m_selectTrans = null;
            m_selectData = null;
            m_previewHead = null;
            m_previewHeadTrans = null;
        }
        if (m_player) CharacterEquip.ChangeCloth(m_player, GetPreviewDress());
        Util.SetText(m_selectName, m_previewHead == null ? string.Empty : m_previewHead.GetPropItem().itemName);
    }

    private void OnHairClick(RectTransform rectTrans, PItem item, bool equip)
    {
        m_previewHair = item;
        m_previewHairTrans = rectTrans;
        if (equip)
        {
            m_selectTrans = null;
            m_selectData = null;
            m_previewHair = null;
            m_previewHeadTrans = null;
        }
        if (m_player) CharacterEquip.ChangeCloth(m_player, GetPreviewDress());
        Util.SetText(m_selectName, m_previewHair == null ? string.Empty : m_previewHair.GetPropItem().itemName);
    }

    private void OnFaceClick(RectTransform rectTrans, PItem item, bool equip)
    {
        m_previewFace = item;
        m_previewFaceTrans = rectTrans;
        if (equip)
        {
            m_selectTrans = null;
            m_selectData = null;
            m_previewFace = null;
            m_previewFaceTrans = null;
        }
        if (m_player) CharacterEquip.ChangeCloth(m_player, GetPreviewDress());
        Util.SetText(m_selectName, m_previewFace == null ? string.Empty : m_previewFace.GetPropItem().itemName);
    }

    private void OnNeckClick(RectTransform rectTrans, PItem item, bool equip)
    {
        m_previewNeck = item;
        m_previewNeckTrans = rectTrans;
        if (equip)
        {
            m_selectTrans = null;
            m_selectData = null;
            m_previewNeck = null;
            m_previewNeckTrans = null;
        }
        if (m_player) CharacterEquip.ChangeCloth(m_player, GetPreviewDress());
        Util.SetText(m_selectName, m_previewNeck == null ? string.Empty : m_previewNeck.GetPropItem().itemName);
    }

    private List<PItem> GetPreviewDress()
    {
        GetPreview(FashionSubType.ClothGuise, m_previewGuise);
        GetPreview(FashionSubType.HeadDress, m_previewHead);
        GetPreview(FashionSubType.HairDress, m_previewHair);
        GetPreview(FashionSubType.FaceDress, m_previewFace);
        GetPreview(FashionSubType.NeckDress, m_previewNeck);

        return curDress;
    }

    private void GetPreview(FashionSubType type, PItem item)
    {
        int index = curDress.FindIndex(0, curDress.Count, o => o.GetPropItem().itemType == PropType.FashionCloth && o.GetPropItem().subType == (byte)type);
        if (index >= 0 && index < curDress.Count)
        {
            if (item == null) curDress.RemoveAt(index);
            else curDress[index] = item;
        }
        else if (index < 0 && item != null) curDress.Add(item);
    }

    protected override List<PItem> GetAllItems()
    {
        List<PItem> datas = new List<PItem>();
        if (m_currentToggleType == FashionSubType.ClothGuise) SetGetItems(m_currentGuise, m_currentToggleType, datas);
        else if (m_currentToggleType == FashionSubType.HeadDress) SetGetItems(m_currentHead, m_currentToggleType, datas);
        else if (m_currentToggleType == FashionSubType.HairDress) SetGetItems(m_currentHair, m_currentToggleType, datas);
        else if (m_currentToggleType == FashionSubType.FaceDress) SetGetItems(m_currentFace, m_currentToggleType, datas);
        else if (m_currentToggleType == FashionSubType.NeckDress) SetGetItems(m_currentNeck, m_currentToggleType, datas);
        return datas;
    }

    private void SetGetItems(PItem item, FashionSubType type, List<PItem> data)
    {
        if (item != null) data.Add(item);
        data.AddRange(moduleEquip.GetBagDress(PropType.FashionCloth, (int)type));
    }

    #endregion

    private void InitCurrentEquip()
    {
        m_currentGuise = moduleEquip.GetDressedProp(PropType.FashionCloth, (int)FashionSubType.ClothGuise);
        m_previewGuise = m_currentGuise;
        m_currentHead = moduleEquip.GetDressedProp(PropType.FashionCloth, (int)FashionSubType.HeadDress);
        m_previewHead = m_currentHead;
        m_currentHair = moduleEquip.GetDressedProp(PropType.FashionCloth, (int)FashionSubType.HairDress);
        m_previewHair = m_currentHair;
        m_currentFace = moduleEquip.GetDressedProp(PropType.FashionCloth, (int)FashionSubType.FaceDress);
        m_previewFace = m_currentFace;
        m_currentNeck = moduleEquip.GetDressedProp(PropType.FashionCloth, (int)FashionSubType.NeckDress);
        m_previewNeck = m_currentNeck;

        m_previewHeadTrans = null;
        m_previewHairTrans = null;
        m_previewFaceTrans = null;
        m_previewNeckTrans = null;

        m_selectData = null;
        m_selectTrans = null;
        curDress.Clear();
        curDress.AddRange(moduleEquip.currentDressClothes);
        ResetSelectTrans();
    }

    private void GoClothShop()
    {
        if (Module_Equip.accesssReturnWay == 1) Module_Announcement.instance.OpenWindow(22);
        else if (Module_Equip.accesssReturnWay == 2)
        {
            Window w = Window.GetOpenedWindow<Window_Equip>();
            w?.Hide();
        }
    }

    private void OnRecoveryDressAndReturn()
    {
        if (m_player) CharacterEquip.ChangeCloth(m_player, moduleEquip.currentDressClothes);
        OnReturn();
    }

    private void OnReturn()
    {
        if (Module_Equip.accesssReturnWay == 0)
        {
            Module_Cangku.instance.RemveNewItem(m_lastToggleType);
            Module_Equip.OpenEquipSubWindow(EnumSubEquipWindowType.MainPanel);
        }
        else GoClothShop();
        Module_Equip.accesssReturnWay = 0;
    }

    #region click event

    private void OnSaveBtnClick()
    {
        if (CanSave())
        {
            List<ulong> upClothes = new List<ulong>();
            List<ulong> takeOffAcces = new List<ulong>();

            SaveClick(m_currentGuise, m_previewGuise, upClothes, takeOffAcces);
            SaveClick(m_currentHead, m_previewHead, upClothes, takeOffAcces);
            SaveClick(m_currentHair, m_previewHair, upClothes, takeOffAcces);
            SaveClick(m_currentFace, m_previewFace, upClothes, takeOffAcces);
            SaveClick(m_currentNeck, m_previewNeck, upClothes, takeOffAcces);

            if (upClothes.Count > 0) moduleEquip.SendChangeClothes(upClothes);
            if (takeOffAcces.Count > 0) moduleEquip.SendTakeOffClothes(takeOffAcces);
        }
        OnReturn();
    }

    private void SaveClick(PItem currItem, PItem priveItem, List<ulong> up, List<ulong> take)
    {
        if (priveItem != currItem)
        {
            if (priveItem == null && currItem != null) take.Add(currItem.itemId);
            else up.Add(priveItem.itemId);
        }
    }

    public override void OnReturnClick()
    {
        if (CanReturn()) OnReturn();
        else
        {
            Window_Alert.ShowAlert(ConfigText.GetDefalutString(TextForMatType.AlertUIText, 3), true, true, true, OnSaveBtnClick, OnRecoveryDressAndReturn);
        }
    }

    private bool CanReturn()
    {
        return m_currentHead == m_previewHead && m_currentHair == m_previewHair
            && m_currentFace == m_previewFace && m_currentNeck == m_previewNeck && m_previewGuise == m_currentGuise;
    }

    private void OnGuiseToggleValueChange(GameObject sender)
    {
        SetToggleChange(FashionSubType.ClothGuise, 0);
    }

    private void OnHeadToggleValueChange(GameObject sender)
    {
        SetToggleChange(FashionSubType.HeadDress, 1);
    }
    private void OnHairToggleValueChange(GameObject sender)
    {
        SetToggleChange(FashionSubType.HairDress, 2);
    }
    private void OnFaceToggleValueChange(GameObject sender)
    {
        SetToggleChange(FashionSubType.FaceDress, 3);
    }
    private void OnNeckToggleValueChange(GameObject sender)
    {
        SetToggleChange(FashionSubType.NeckDress, 4);
    }

    private void SetToggleChange(FashionSubType type, int index)
    {
        if (m_currentToggleType == type) return;

        m_currentToggleType = type;
        if (index < m_titleNames.Length) Util.SetText(m_title, m_titleNames[index]);
        ResetSelectTrans();
        RefreshScrollView();
        m_lastToggleType = m_currentToggleType;
        SetFashionClothState();
        m_detailBtn.button.SetInteractable(m_selectTrans);
    }

    private void SetFashionClothState()
    {
        m_guiseBtn.SetMarkVisible(Module_Cangku.instance.TypeHint(EquipType.Guise) && m_currentToggleType != FashionSubType.ClothGuise);
        m_headDressBtn.SetMarkVisible(Module_Cangku.instance.TypeHint(EquipType.HeadDress) && m_currentToggleType != FashionSubType.HeadDress);
        m_hairDressBtn.SetMarkVisible(Module_Cangku.instance.TypeHint(EquipType.HairDress) && m_currentToggleType != FashionSubType.HairDress);
        m_faceDressBtn.SetMarkVisible(Module_Cangku.instance.TypeHint(EquipType.FaceDress) && m_currentToggleType != FashionSubType.FaceDress);
        m_neckDressBtn.SetMarkVisible(Module_Cangku.instance.TypeHint(EquipType.NeckDress) && m_currentToggleType != FashionSubType.NeckDress);
    }

    private void RestToggleType(RectTransform rect, PItem item)
    {
        m_selectTrans = rect;
        m_selectData = item;
        Util.SetText(m_selectName, item == null ? string.Empty : item.GetPropItem().itemName);
    }

    private void ResetSelectTrans()
    {
        switch (m_currentToggleType)
        {
            case FashionSubType.HeadDress: RestToggleType(m_previewHeadTrans, m_previewHead); break;
            case FashionSubType.HairDress: RestToggleType(m_previewHairTrans, m_previewHair); break;
            case FashionSubType.FaceDress: RestToggleType(m_previewFaceTrans, m_previewFace); break;
            case FashionSubType.NeckDress: RestToggleType(m_previewNeckTrans, m_previewNeck); break;
            case FashionSubType.ClothGuise:
                m_selectTrans = m_previewGuiseTrans;
                m_selectData = m_previewGuise;
                Util.SetText(m_selectName, m_previewGuise == null ? string.Empty : m_previewGuise.GetPropItem().itemName);
                break;
            default:
                break;
        }
    }

    private void OnChangeBigClick(GameObject sender)
    {
        m_cameraLevel++;
        m_cameraLevel = Mathf.Clamp(m_cameraLevel, 0, 4);
        ChangeCameraData(m_cameraLevel);
    }

    private void OnChangeSmallClick(GameObject sender)
    {
        m_cameraLevel--;
        m_cameraLevel = Mathf.Clamp(m_cameraLevel, 0, 4);
        ChangeCameraData(m_cameraLevel);
    }

    private void OnResetClick(GameObject sender)
    {
        InitCurrentEquip();
        m_dataSource?.UpdateItems();
        if (m_player) CharacterEquip.ChangeCloth(m_player, moduleEquip.currentDressClothes);
        m_cameraLevel = GeneralConfigInfo.defaultConfig.defaultLevel;
        ChangeCameraData(m_cameraLevel);
    }

    private void ChangeCameraData(int level)
    {
        m_cameraTweenPos?.Kill();
        m_cameraTweenRot?.Kill();

        if (!m_showCreatureInfo || m_showCreatureInfo.forData.Length < 0) return;

        ShowCreatureInfo.SizeAndPos data = null;
        for (int i = 0; i < m_showCreatureInfo.forData.Length; i++)
        {
            if (m_showCreatureInfo.forData[i].index == level)
            {
                data = m_showCreatureInfo.forData[i].data[0];
                break;
            }
        }

        if (!m_uiCamera) m_uiCamera = Camera.main;
        if (data != null && m_uiCamera)
        {
            m_cameraTweenPos = m_uiCamera.transform.DOLocalMove(data.pos, 0.2f).SetEase(Ease.Linear);
            m_cameraTweenRot = m_uiCamera.transform.DOLocalRotate(data.rotation, 0.2f).SetEase(Ease.Linear);
        }

        m_changeSlider.value = 0.25f * level;
    }
    #endregion
}
#endregion

#region base panel

public class BaseSuccessPanel : CustomSecondPanel
{
    public BaseSuccessPanel(Transform trans) : base(trans) { }

    protected Text m_nameText;

    protected PItem itemData;
    protected PItem lastCache;
    protected EquipType equipType;
    protected Module_Equip moduleEquip;
    protected GameObject bgObj;
    protected bool canClick = false;
    public UICharacter m_charactor { get; protected set; }
    public Action<bool> beforePanelVisible;
    public Action<bool> afterPanelVisible;

    public override void InitComponent()
    {
        base.InitComponent();
        moduleEquip = Module_Equip.instance;

        bgObj = transform.Find("bg").gameObject;
        m_charactor = transform.GetComponent<UICharacter>("item_evolve");
    }

    public override void AddEvent()
    {
        base.AddEvent();

        EventTriggerListener.Get(bgObj).onClick = OnExitClick;
    }

    private void OnExitClick(GameObject sender)
    {
        //还在做动画, 就不允许返回
        if (!canClick) return;

        OnExitClickSuccess();
    }

    public virtual void OnExitClickSuccess()
    {
        SetPanelVisible(false);
    }

    public virtual void RefreshPanel(PItem item, PItem cache)
    {
        enableUpdate = false;
        canClick = false;

        SetPanelVisible(true);
        itemData = item;
        lastCache = cache;
        equipType = Module_Equip.GetEquipTypeByItem(item);
        moduleEquip.LoadModel(item, Layers.WEAPON);

        PropItemInfo prop = cache?.GetPropItem();
        Util.SetText(m_nameText, prop == null ? string.Empty : prop.itemName);
    }

    public override void SetPanelVisible(bool visible = true)
    {
        beforePanelVisible?.Invoke(visible);
        base.SetPanelVisible(visible);

        //由于三个面板操作了同一个bg，所以每次打开的时候需要刷新点击事件，避免初始化的时候冲突掉
        if (visible) Module_Global.instance.ShowGlobalLayerDefault(2, false);
        else Module_Global.instance.ShowGlobalLayerDefault();
        afterPanelVisible?.Invoke(visible);
    }
}

public class PreviewBasePanel : CustomSecondPanel
{
    public PreviewBasePanel(Transform trans) : base(trans) { }

    #region properties
    public PItem data { get; private set; }
    public EquipType equipType { get; private set; }
    public PropItemInfo itemInfo { get; private set; }
    #endregion

    protected Transform m_matParent;
    protected GameObject m_matPrefab;

    protected List<PreviewMatItem> m_matList = new List<PreviewMatItem>();
    protected List<PreviewMatItem> m_matPoolList = new List<PreviewMatItem>();

    protected Module_Equip moduleEquip;

    protected Text m_nameText;
    protected Text m_levelText;
    protected Image m_iconImage;

    public UICharacter uiCharactor { get; protected set; }

    public override void InitComponent()
    {
        base.InitComponent();

        moduleEquip = Module_Equip.instance;

        ClearList();
        m_matParent = transform.Find("material_list");
        m_matPrefab = m_matParent.Find("item")?.gameObject;
        m_matPrefab?.SetActive(false);

        m_nameText = transform.GetComponent<Text>("info/name");
        m_levelText = transform.GetComponent<Text>("info/name/level");
        m_iconImage = transform.GetComponent<Image>("info/icon");

        uiCharactor = transform.GetComponent<UICharacter>("npcRawImage");
    }

    #region mat items

    public void RefreshMatItem(List<PItem> datas, Action<PreviewMatItem> select, Action<PreviewMatItem> del)
    {
        if (datas == null) datas = new List<PItem>();
        for (int i = 0; i < m_matList.Count; i++)
        {
            m_matList[i].gameObject.SetActive(true);
            m_matList[i].RegisterCallback(select, del);

            if (i < datas.Count) m_matList[i].RefreshSelectableExpItem(datas[i], 0);
            else m_matList[i].InitSelectableExpItem();
        }
    }

    public void RefreshMatItem(EvolveMaterial[] datas, List<PItem> bagProps)
    {
        if (datas == null) datas = new EvolveMaterial[0];
        if (bagProps == null) bagProps = new List<PItem>();

        Array.Sort(datas, (a, b) =>
        {
            var propa = ConfigManager.Get<PropItemInfo>(a.propId);
            if (!propa) return -1;

            var propb = ConfigManager.Get<PropItemInfo>(b.propId);
            if (!propb) return -1;

            return propa.quality.CompareTo(propb.quality);
        });

        AddNewMatItem(datas.Length - m_matList.Count);
        DisableAllMats();

        PItem temp = null;
        for (int i = 0; i < datas.Length; i++)
        {
            m_matList[i].gameObject.SetActive(true);
            temp = bagProps?.Find(o => o.itemTypeId == datas[i].propId);
            m_matList[i].RefreshDisselectableExpItem(datas[i], temp == null ? 0 : temp.num);
        }
    }

    public void AddNewMatItem(int count)
    {
        if (count <= 0 || !m_matPrefab) return;

        int c = count > m_matPoolList.Count ? m_matPoolList.Count : count;
        for (int i = 0; i < c; i++)
        {
            PreviewMatItem item = m_matPoolList[0];
            item.transform.SetParent(m_matParent);
            item.gameObject.SetActive(false);

            m_matList.Add(item);
            m_matPoolList.RemoveAt(0);
        }

        int createCount = count - m_matList.Count;
        for (int i = 0; i < createCount; i++)
        {
            Transform t = m_matParent.AddNewChild(m_matPrefab);
            t.gameObject.SetActive(false);
            m_matList.Add(t.GetComponentDefault<PreviewMatItem>());
        }
    }

    public void ResetAllMats()
    {
        m_matPoolList.AddRange(m_matList);
        m_matList.Clear();
        foreach (var item in m_matPoolList)
        {
            item.gameObject.SetActive(false);
            item.transform.SetParent(m_matParent);
        }
    }

    public void DisableAllMats()
    {
        foreach (var item in m_matList)
        {
            item.gameObject.SetActive(false);
        }
    }
    #endregion

    #region protected functions

    protected bool ContainSameAttirbutePreview(List<AttributePreviewDetail> list, int id)
    {
        if (list == null || list.Count == 0) return false;
        AttributePreviewDetail d = list.Find(o => o.id == id);
        return d != null;
    }

    #endregion

    #region refresh panel

    protected void ClearList()
    {
        m_matList.Clear();
        m_matPoolList.Clear();
    }

    protected virtual void InitPanel()
    {
        ResetAllMats();
    }

    public void RefreshPanel(PItem item)
    {
        if (item == null) return;

        EquipType t = Module_Equip.GetEquipTypeByItem(item);
        RefreshPanel(item, t);
    }

    public virtual void RefreshPanel(PItem item, EquipType type)
    {
        data = item;
        equipType = type;
        itemInfo = data?.GetPropItem();

        InitPanel();
        SetPanelVisible(true);

        Util.SetText(m_nameText, itemInfo ? itemInfo.itemName : string.Empty);
        int idx = Module_Forging.instance.GetWeaponIndex(itemInfo);
        if (idx >= 0) AtlasHelper.SetShared(m_iconImage, ForgePreviewPanel.WEAPON_TYPE_IMG_NAMES.GetValue<string>(idx));
    }
    #endregion
}

#endregion

#region items

public sealed class EquipMainBtnItem : CustomSecondPanel
{
    public EquipMainBtnItem(Transform trans, int index) : base(trans)
    {
        Util.SetText(m_titleText, ConfigText.GetDefalutString(TextForMatType.EquipUIText, index));
    }

    private static readonly string[] ELEMENT_IMG = new string[] { "ui_element_wind", "ui_element_fire", "ui_element_water", "ui_element_thunder", "ui_element_ice" };
    private Text m_nameText;
    private Text m_titleText;
    private Image m_markImg;
    private Image m_elementImg;
    private PItem m_data;

    public override void InitComponent()
    {
        base.InitComponent();
        m_titleText = transform.GetComponent<Text>("type_txt");
        m_nameText = transform.GetComponent<Text>("name_txt");
        m_markImg = transform.GetComponent<Image>("mark");
        m_elementImg = transform.GetComponent<Image>("weapon_bg/elementicon");
    }

    public void RefreshItem(PItem item)
    {
        m_data = item;
        if (item == null)
        {
            m_nameText.text = string.Empty;
            return;
        }

        PropItemInfo info = item?.GetPropItem();
        m_nameText.text = info?.itemName;
        RefreshMark();
        RefreshElementImg();
    }

    public void RefreshMark()
    {
        if (m_markImg)
        {
            var type = Module_Equip.GetEquipTypeByItem(m_data);
            m_markImg.SafeSetActive(Module_Equip.HasAnyEquipOperation(m_data) || Module_Cangku.instance.TypeHint(type));
        }
    }

    private void RefreshElementImg()
    {
        EquipType t = Module_Equip.GetEquipTypeByItem(m_data);
        if (!m_elementImg || t != EquipType.Weapon) return;

        WeaponAttribute weaponAttributes = ConfigManager.Get<WeaponAttribute>(m_data.itemTypeId);
        if (!weaponAttributes) return;

        //名字
        CreatureElementTypes elemrntType = weaponAttributes ? (CreatureElementTypes)weaponAttributes.elementType : CreatureElementTypes.Count;
        string imgName = ELEMENT_IMG.GetValue<string>((int)elemrntType - 1);

        if (string.IsNullOrEmpty(imgName)) return;

        UIDynamicImage.LoadImage(m_elementImg.transform, imgName);
    }
}

public class AttributeItem : CustomSecondPanel
{
    public AttributeItem(Transform trans) : base(trans) { }

    private Text m_attrNameText;
    private Text m_leftAttrText;
    private Text m_rightAttrText;
    private Text m_deltaText;
    private string[] m_deltaFormat = new string[]
    {
        ConfigText.GetDefalutString(TextForMatType.EquipUIText,38),
        ConfigText.GetDefalutString(TextForMatType.EquipUIText,39),
    };
    private List<GameObject> m_arrows = new List<GameObject>();
    //保留几位小数
    private int digit = 0;

    public override void InitComponent()
    {
        base.InitComponent();
        m_attrNameText = transform.GetComponent<Text>();
        m_leftAttrText = transform.GetComponent<Text>("left_txt");
        m_rightAttrText = transform.GetComponent<Text>("right_txt");
        m_deltaText = transform.GetComponent<Text>("right_txt_02");
        m_arrows.Clear();
        m_arrows.Add(transform.Find("arrow_left").gameObject);
        m_arrows.Add(transform.Find("arrow_right").gameObject);
    }

    public void RefreshDetail(AttributePreviewDetail detail, bool showDelta = true)
    {
        if (detail == null) return;

        SetPanelVisible(true);
        Util.SetText(m_attrNameText, ConfigText.GetDefalutString(TextForMatType.AllAttributeText, detail.id));
        m_leftAttrText.text = AttributeShowHelper.ValueString(detail.id, GeneralConfigInfo.IsPercentAttribute(detail.id) ? detail.oldValue * 0.01 : detail.oldValue);
        m_rightAttrText.text = AttributeShowHelper.ValueString(detail.id, GeneralConfigInfo.IsPercentAttribute(detail.id) ? detail.newValue * 0.01 :
            detail.newValue);
        double delta = AttributeShowHelper.ValueForShow(detail.id, detail.newValue) - AttributeShowHelper.ValueForShow(detail.id, detail.oldValue);
        string format = delta > 0 ? m_deltaFormat[0] : m_deltaFormat[1];
        if (showDelta && m_deltaText) Util.SetText(m_deltaText, format, AttributeShowHelper.ValueString(detail.id, GeneralConfigInfo.IsPercentAttribute(detail.id) ? delta * 0.01 : delta));
        SetRightEnable(detail.newValue, delta);
    }

    public void RefreshDetail(int leftData, int rightData, bool showDelta = true)
    {
        SetPanelVisible(true);
        m_leftAttrText.text = leftData.ToString();
        m_rightAttrText.text = rightData.ToString();
        int delta = rightData - leftData;
        string format = delta >= 0 ? m_deltaFormat[0] : m_deltaFormat[1];
        if (showDelta && m_deltaText) m_deltaText.text = Util.Format(format, delta);
        m_deltaText?.gameObject.SetActive(delta != 0);
        SetRightEnable(rightData, delta);
    }

    private void SetRightEnable(double rightData, double delta)
    {
        SetRightEnable(rightData >= 0, delta != 0);
    }

    private void SetRightEnable(int rightData, int delta)
    {
        SetRightEnable(rightData >= 0, delta != 0);
    }

    public void SetRightEnable(bool enable, bool deltaEnable = true)
    {
        m_rightAttrText.gameObject.SetActive(enable);
        m_deltaText?.gameObject.SetActive(enable & deltaEnable);
        foreach (var item in m_arrows) item.gameObject?.SetActive(enable);
    }
}

public class MarkButton : CustomSecondPanel
{
    public MarkButton(Transform trans) : base(trans)
    {
    }

    public Button button { get; private set; }
    public Image markImage { get; private set; }


    public override void InitComponent()
    {
        base.InitComponent();
        button = transform.GetComponent<Button>();
        markImage = transform.GetComponent<Image>("mark");
    }

    public void SetMarkVisible(bool visible)
    {
        if (markImage) markImage.SafeSetActive(visible);
    }
}
#endregion

#endregion
