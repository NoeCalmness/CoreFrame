/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-09-04
 * 
 ***************************************************************************************************/

using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Labyrinth : Window
{
    #region 二级面板自定义

    /// <summary>
    /// 玩家列表界面
    /// </summary>
    public class LabyrinthPlayerPanel : CustomSecondPanel
    {
        public LabyrinthPlayerPanel(Transform trans) : base(trans)
        {
        }
        private Transform m_areaParent;
        private GameObject m_areaPrefab;
        public Dictionary<EnumLabyrinthArea, LabyrinthAreaItem> areaDic;
        public ScrollRect scroll { get; private set; }

        private Toggle m_propToggle;
        private Toggle m_reportToggle;
        private Text m_currentStageText;
        private Image m_stateImg;
        private Text m_countDownText;

        public bool openState { get; private set; }

        public GameObject m_openBtn;

        public Action<PMazePlayer> onPlayerChoose;
        public Action<PMazePlayer> onPlayerSneak;
        public Action onPropClick;
        public Action onReportClick;
        public Action<bool> onPanelOpenStateChange;
        
        public override void InitComponent()
        {
            base.InitComponent();
            
            openState = false;
            m_openBtn = transform.Find("left/open").gameObject;
            scroll = transform.Find("left/playerlist").GetComponent<ScrollRect>();
            m_areaParent = transform.Find("left/playerlist/viewport/content");
            m_areaPrefab = m_areaParent.transform.Find("area_0").gameObject;
            m_areaPrefab.SetActive(false);

            m_propToggle = transform.GetComponent<Toggle>("left/playerlist/prop_btn");
            m_reportToggle = transform.GetComponent<Toggle>("left/playerlist/battle_field_btn");
            m_currentStageText = transform.GetComponent<Text>("left/playerlist/Image/myRankNumber_Txt");
            m_stateImg = transform.GetComponent<Image>("left/playerlist/stateSign_Txt/stateSign_Img");
            m_countDownText = transform.GetComponent<Text>("left/playerlist/stateSign_Txt");

            areaDic = new Dictionary<EnumLabyrinthArea, LabyrinthAreaItem>();
            //从晋级区的开始创建,保证UI的显示顺序
            for (EnumLabyrinthArea i = EnumLabyrinthArea.PromotionArea; i >= EnumLabyrinthArea.DemotionArea; i--)
            {
                Transform newArea = m_areaParent.AddNewChild(m_areaPrefab);
                newArea.gameObject.SetActive(true);
                LabyrinthAreaItem componnet = new LabyrinthAreaItem(newArea);
                areaDic.Add(i, componnet);
            }

            SetPlayerItemsInteractive(false);
        }

        public override void AddEvent()
        {
            base.AddEvent();

            //设置玩家点击事件
            Dictionary<EnumLabyrinthArea, LabyrinthAreaItem>.Enumerator e = areaDic.GetEnumerator();
            while (e.MoveNext())
            {
                List<LabyrinthPlayerItem> players = e.Current.Value.playerItems;
                for (int i = 0, count = players.Count; i < count; i++)
                {
                    players[i].onPlayerClick = OnItemClickCallback;
                    players[i].onSneakClick = OnSneakClickCallback;
                }
            }

            EventTriggerListener.Get(m_openBtn).onClick = OnOpenBtnClick;
            m_reportToggle.onValueChanged.RemoveAllListeners();
            m_reportToggle.onValueChanged.AddListener(OnReportBtnClick);
            m_propToggle.onValueChanged.RemoveAllListeners();
            m_propToggle.onValueChanged.AddListener(OnPropBtnClick);
        }

        private void OnPropBtnClick(bool isOn)
        {
            if (!isOn) return;

            onPropClick?.Invoke();
        }

        private void OnReportBtnClick(bool isOn)
        {
            if (!isOn) return;

            onReportClick?.Invoke();
        }

        public void OnOpenBtnClick(GameObject sender)
        {
            openState = !openState;
            onPanelOpenStateChange?.Invoke(openState);
            if (openState) scroll.verticalNormalizedPosition = 1f;
        }

        public void RefreshPlayerPanel(Dictionary<EnumLabyrinthArea, List<PMazePlayer>> areaPlayers)
        {
            m_openBtn.SetActive(true);
            int startIndex = 0;

            Dictionary<EnumLabyrinthArea, LabyrinthAreaItem>.Enumerator e = areaDic.GetEnumerator();
            while (e.MoveNext())
            {
                List<PMazePlayer> players = areaPlayers.Get(e.Current.Key);
                e.Current.Value.RefreshPlayer(startIndex, players);
                e.Current.Value.RefreshAward(e.Current.Key);

                startIndex += players == null ? 0 : players.Count;
            }

            EnumLabyrinthArea area = moduleLabyrinth.GetPlayerArea(modulePlayer.roleInfo.roleId);
            int id = 13;
            switch (area)
            {
                case EnumLabyrinthArea.DemotionArea: id = 15; break;
                case EnumLabyrinthArea.RelegationArea: id = 14; break;
                case EnumLabyrinthArea.PromotionArea: id = 13; break;
            }
            Util.SetText(m_currentStageText, ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 36), moduleLabyrinth.GetPlayerRank(modulePlayer.roleInfo.roleId), ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, id));
        }

        public void SetPlayerItemsInteractive(bool interactive)
        {
            m_openBtn.SetActive(!interactive);

            //设置玩家item是否可交互
            Dictionary<EnumLabyrinthArea, LabyrinthAreaItem>.Enumerator e = areaDic.GetEnumerator();
            while (e.MoveNext())
            {
                List<LabyrinthPlayerItem> players = e.Current.Value.playerItems;
                for (int i = 0, count = players.Count; i < count; i++)
                {
                    players[i].Interactive = interactive;
                }
            }
        }

        private void OnItemClickCallback(PMazePlayer playerData)
        {
            //Logger.LogInfo("点击了的玩家是  {0}", playerData.roleName);
            onPlayerChoose?.Invoke(playerData);
        }

        private void OnSneakClickCallback(PMazePlayer playerData)
        {
            //Logger.LogInfo("点击了的玩家是  {0}", playerData.roleName);
            onPlayerSneak?.Invoke(playerData);
        }

        public void RefreshSelfAttackProp()
        {
            foreach (var item in areaDic)
            {
                foreach (var p in item.Value.playerItems)
                {
                    if (p.playerData == null) continue;

                    if (p.playerData.roleId == modulePlayer.roleInfo.roleId) p.RefreshBuffIcon();
                }
            }
        }

        public void RefreshStateImg()
        {
            SetLabyrinthImgAsCurrent(m_stateImg);
        }

        public void RefreshCountDown(string msg)
        {
            Util.SetText(m_countDownText, msg);
        }
    }

    /// <summary>
    ///  道具界面
    /// </summary>
    public class LabyrinthPropPanel : CustomSecondPanel
    {
        private GameObject m_exitBtn;
        private ScrollView m_scroll;
        private DataSource<PItem2> m_dataSource;
        
        public GameObject maskPanel { get; private set; }
        private GameObject m_maskBg;
        private Text propName;
        private Transform propTypeParent;
        private Text propDecription;
        private Button usePropBtn;

        private GameObject noneProps = null;

        /// <summary>
        /// 在页面中缓存是为了避免玩家在没有收到消息之前又点击过其他item
        /// </summary>
        private RectTransform m_selectTrans;
        private PItem2 m_selectData;

        public Action<ushort, LabyrinthPropSubType> OnPropItemClick;
        public Action onCancelChoosePlayer;

        public ushort clickPropId { get; set; }
        private string typeName;
        private LabyrinthPropSubType m_subtype;

        private string lockText = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 56);
        public LabyrinthPropPanel(Transform trans) : base(trans)
        {
        }

        public LabyrinthPropSubType subtype
        {
            get { return m_subtype; }
            set
            {
                m_subtype = value;
                switch (m_subtype)
                {
                    case LabyrinthPropSubType.TrapProp:         typeName = "trap";      break;
                    case LabyrinthPropSubType.AttackProp:       typeName = "attack";    break;
                    case LabyrinthPropSubType.ProtectedProp:    typeName = "protect";   break;
                    default: break;
                }
            }
        }

        public override void InitComponent()
        {
            base.InitComponent();

            maskPanel = transform.Find("middle").gameObject;
            m_maskBg = maskPanel.transform.Find("mask").gameObject;
            m_scroll = transform.GetComponent<ScrollView>("prop_list/scrollView");
            m_exitBtn = transform.Find("prop_list/close_button").gameObject;
            propName = transform.Find("prop_list/info/name_bg/name").GetComponent<Text>();
            propName.gameObject.SetActive(false);
            propTypeParent = transform.Find("prop_list/info/type_bg");
            Util.DisableAllChildren(propTypeParent);
            propDecription = transform.Find("prop_list/info/des").GetComponent<Text>();
            propDecription.gameObject.SetActive(false);
            usePropBtn = transform.Find("prop_list/ues_button").gameObject.GetComponentDefault<Button>();
            m_dataSource = new DataSource<PItem2>(null,m_scroll,OnItemRefresh,OnItemClick);
            noneProps = transform.Find("prop_list/nothing").gameObject;
            noneProps.SafeSetActive(false);
            m_selectData = null;
            m_selectTrans = null;
        }

        public override void AddEvent()
        {
            base.AddEvent();

            EventTriggerListener.Get(m_exitBtn).onClick = OnBackgroundClick;
            EventTriggerListener.Get(m_maskBg).onClick = OnCancelChoosePlayerClick;
        }

        private void OnItemRefresh(RectTransform rt, PItem2 data)
        {
            PropItemInfo info = ConfigManager.Get<PropItemInfo>(data.itemTypeId);
            if (!info) return;

            Util.SetItemInfo(rt,info,data.level,(int)data.num,false);
            usePropBtn.SetInteractable(m_selectTrans && RecheckInteractive());
            rt.Find("selectBox")?.gameObject.SetActive(rt == m_selectTrans);
            Text numtText = rt.GetComponent<Text>("totalNumber");
            if(numtText) Util.SetText(numtText, data.num.ToString());
            Transform lockTrans = rt.Find("lock");
            lockTrans.SafeSetActive(moduleLabyrinth.currentLabyrinthStep == EnumLabyrinthTimeStep.Rest);
            if (lockTrans && !string.IsNullOrEmpty(lockText)) Util.SetText(lockTrans.GetComponent<Text>("Text"), lockText);
        }

        private void OnItemClick(RectTransform rt, PItem2 data)
        {
            m_selectTrans = rt;
            m_selectData = data;
            
            m_dataSource?.UpdateItems();

            var propInfo = ConfigManager.Get<PropItemInfo>(data.itemTypeId);
            if (propInfo == null) return;
            LabyrinthPropSubType type = (LabyrinthPropSubType)propInfo.subType;
            //Logger.LogDetail("当前使用 name = {0} 类型是：{1} 的道具", propInfo.itemName, type);

            subtype = type;

            propName.gameObject.SetActive(true);
            propName.text = propInfo.itemName;
            Util.DisableAllChildren(propTypeParent, typeName);
            propDecription.gameObject.SetActive(true);
            propDecription.text = ConfigText.GetDefalutString(propInfo.desc);
            
            bool interactable = RecheckInteractive();
            usePropBtn.SetInteractable(interactable);

            usePropBtn.onClick.RemoveAllListeners();
            if (interactable)
            {
                usePropBtn.onClick.AddListener(() =>
                {
                    //缓存，选择好友的时候会使用
                    if (type == LabyrinthPropSubType.AttackProp) clickPropId = data.itemTypeId;
                    maskPanel.SetActive(type == LabyrinthPropSubType.AttackProp);
                    OnPropItemClick?.Invoke(data.itemTypeId, type);
                });
            }
        }

        public void RefreshProp(Dictionary<ushort, PItem2> propDic)
        {
            if (propDic == null || propDic.Count < 1)
            {
                propName.gameObject.SetActive(false);
                Util.DisableAllChildren(propTypeParent);
                propDecription.gameObject.SetActive(false);
            }

            if (propDic != null)
            {
                //刷新所有数据到item上
                List<PItem2> props = new List<PItem2>();
                foreach (var item in propDic)
                {
                    props.Add(item.Value);
                }

                props.Sort((a, b) =>
                {
                    return a.itemTypeId.CompareTo(b.itemTypeId);
                });

                m_dataSource.SetItems(props);
                m_dataSource.UpdateItems();
                noneProps.SafeSetActive(props.Count <= 0);
            }

            OnCancelChoosePlayerClick(null);
        }

        private void OnBackgroundClick(GameObject sender)
        {
            propName.gameObject.SetActive(false);
            propDecription.gameObject.SetActive(false);
            Util.DisableAllChildren(propTypeParent);

            m_selectData = null;
            m_selectTrans = null;
        }
        
        private void OnCancelChoosePlayerClick(GameObject sender)
        {
            maskPanel.SetActive(false);
            onCancelChoosePlayer?.Invoke();
        }

        private bool RecheckInteractive()
        {
            bool interactable = true;
            if (subtype == LabyrinthPropSubType.TrapProp)
            {
                PMazePlayer self = moduleLabyrinth.GetSelfMazeData();
                interactable = self.mazeLayer > 0 && !moduleLabyrinth.IsAlreadySetTrap(self.mazeLayer);
            }
            else if (m_selectData != null && m_selectData.itemTypeId == HUNTER_HERB_ID)
            {
                interactable = moduleLabyrinth.labyrinthSelfInfo.healthRate < 100;
            }
            return interactable;
        }

        public void RefreshLastClickItem()
        {
            if(m_selectTrans && m_selectData != null && (subtype == LabyrinthPropSubType.TrapProp || m_selectData.itemTypeId == HUNTER_HERB_ID))
            {
                OnItemClick(m_selectTrans,m_selectData);
            }
            OnCancelChoosePlayerClick(null);
        }
    }

    /// <summary>
    /// 战报界面
    /// </summary>
    public class LabyrinthReportPanel : CustomSecondPanel
    {
        //初始化创建的战报条数
        private const int INIT_REPORT_ITEM_COUNT = 30;
        
        private ConfigText m_configText;
        private GameObject m_nonePanel;
        private List<PMazeReport> m_reportDatas = new List<PMazeReport>();
        private ScrollView m_scroll;
        private DataSource<PMazeReport> m_dataSource;

        public LabyrinthReportPanel(Transform trans) : base(trans)
        {
        }

        public override void InitComponent()
        {
            base.InitComponent();
            m_scroll            = transform.GetComponent<ScrollView>("prop_list/scrollView");
            m_dataSource        = new DataSource<PMazeReport>(m_reportDatas, m_scroll, OnRefreshItem);
            m_nonePanel         = transform.Find("nothing").gameObject;
            m_configText        = ConfigManager.Get<ConfigText>(10012);
        }

        private void OnRefreshItem(RectTransform r,PMazeReport data)
        {
            Text t = r.GetComponent<Text>();
            Util.SetText(t,GetReportDescription(data));
        }

        public void RefreshReportPanel(List<PMazeReport> reports)
        {
            m_reportDatas.Clear();
            m_reportDatas.AddRange(reports);

            if (m_nonePanel) m_nonePanel.gameObject.SetActive(m_reportDatas == null || m_reportDatas.Count == 0);
            SetPanelVisible(true);
            m_dataSource.SetItems(m_reportDatas);
            m_dataSource.UpdateItems();
        }

        private string GetReportDescription(PMazeReport report)
        {
            string msg = m_configText[report.reportType];
            EnumLabyrinthReportType type = (EnumLabyrinthReportType)report.reportType;
            switch (type)
            {
                //只有一个参数，并且参数是道具ID的解析
                case EnumLabyrinthReportType.OtherUseTrapProp:
                case EnumLabyrinthReportType.OtherAttackSelf:
                case EnumLabyrinthReportType.SelfUseProtectProp:
                case EnumLabyrinthReportType.SelfBeTrappedAfterUseDog:
                    PropItemInfo info = ConfigManager.Get<PropItemInfo>((int)report.parameters[0]);
                    msg = Util.Format(msg, info == null ? "" : info.itemName);
                    break;

                //只有一个参数，并且参数是玩家ID的解析
                case EnumLabyrinthReportType.SelfSneakFailed:
                case EnumLabyrinthReportType.OtherUseHunterBell:
                    PMazePlayer playera = moduleLabyrinth.GetTargetPlayer(report.parameters[0]);
                    msg = Util.Format(msg, playera == null ? "" : playera.roleName);
                    break;

                //只有一个参数，并且参数是掉落的层数
                case EnumLabyrinthReportType.BeSneakedSuccess:
                case EnumLabyrinthReportType.SelfBombExplode:
                    msg = Util.Format(msg, report.parameters[0]);
                    break;

                case EnumLabyrinthReportType.SelfUseTrapProp:
                    LabyrinthInfo laInfo = moduleLabyrinth.labyrinthInfo;
                    PropItemInfo info1 = ConfigManager.Get<PropItemInfo>((int)report.parameters[1]);
                    msg = Util.Format(msg, laInfo.labyrinthName, report.parameters[0], info1 == null ? "" : info1.itemName);
                    break;

                case EnumLabyrinthReportType.SelfAttackOther:
                    PMazePlayer playerc = moduleLabyrinth.GetTargetPlayer(report.parameters[0]);
                    PropItemInfo info2 = ConfigManager.Get<PropItemInfo>((int)report.parameters[1]);
                    msg = Util.Format(msg, playerc == null ? "" : playerc.roleName, info2 == null ? "" : info2.itemName);
                    break;

                case EnumLabyrinthReportType.SelfSneakSuccess:
                    PMazePlayer playerb = moduleLabyrinth.GetTargetPlayer(report.parameters[0]);
                    msg = Util.Format(msg, playerb == null ? "" : playerb.roleName, report.parameters[1]);
                    break;

                //无参的解析
                case EnumLabyrinthReportType.BeSneakFailed:
                case EnumLabyrinthReportType.SelfForceRest:
                case EnumLabyrinthReportType.SelfRecoveryFormRest:
                    break;
            }

            return msg;
        }
    }

    /// <summary>
    /// 道具使用界面
    /// </summary>
    public class LabyrinthUsePropPanel : CustomSecondPanel
    {
        public LabyrinthUsePropPanel(Transform trans) : base(trans)
        {
        }

        private Text m_titleText;
        private Image m_icon;
        private Button m_confirmBtn;

        private LabyrinthPropSubType type;
        private PMazePlayer playerData;
        private ushort propId;

        private List<GameObject> m_bgObjs = new List<GameObject>();

        public Action<LabyrinthPropSubType> onCancelClick;

        public override void InitComponent()
        {
            base.InitComponent();

            m_titleText = transform.Find("tip").GetComponent<Text>();
            m_icon = transform.Find("use/icon").GetComponent<Image>();
            m_confirmBtn = transform.Find("confirm_btn").GetComponent<Button>();

            Transform bgTrans = transform.Find("use/bg");
            m_bgObjs.Clear();
            for (int i = 0; i < Window_Equip.BG_NAMES.Length; i++)
                m_bgObjs.Add(bgTrans.Find(Window_Equip.BG_NAMES[i]).gameObject);
        }

        public override void AddEvent()
        {
            base.AddEvent();

            EventTriggerListener.Get(m_confirmBtn.gameObject).onClick = OnConfirmClick;
        }

        private void OnConfirmClick(GameObject sender)
        {
            switch (type)
            {
                case LabyrinthPropSubType.TrapProp:
                    moduleLabyrinth.SendUseTrapProp(propId);
                    break;
                case LabyrinthPropSubType.AttackProp:
                    moduleLabyrinth.SendUsePlayerProp(propId, playerData.roleId);
                    break;
                case LabyrinthPropSubType.ProtectedProp:
                    moduleLabyrinth.SendUsePlayerProp(propId, modulePlayer.roleInfo.roleId);
                    break;
            }

            OnCancelClick();
        }

        private void OnCancelClick()
        {
            onCancelClick?.Invoke(type);
            SetPanelVisible(false);
            transform.parent.gameObject.SetActive(false);
        }

        public void ShowUserPropPanel(ushort propId, LabyrinthPropSubType type, PMazePlayer playerData = null)
        {
            SetPanelVisible(true);
            transform.parent.gameObject.SetActive(true);

            ConfigText tip = ConfigManager.Get<ConfigText>(215);
            PropItemInfo info = ConfigManager.Get<PropItemInfo>(propId);

            if (tip == null || info == null)
            {
                SetPanelVisible(false);
                Logger.LogError("propid = {0} cannot be loaded", propId);
                return;
            }

            if (type == LabyrinthPropSubType.AttackProp && playerData == null)
            {
                Logger.LogError("choosed the playerData is null");
                return;
            }

            this.type = type;
            this.playerData = playerData;
            this.propId = propId;

            AtlasHelper.SetItemIcon(m_icon,info);
            bool interactable = true;
            switch (type)
            {
                case LabyrinthPropSubType.TrapProp:
                    LabyrinthInfo laInfo = moduleLabyrinth.labyrinthInfo;
                    PMazePlayer self = moduleLabyrinth.GetSelfMazeData();
                    interactable = self.mazeLayer > 0 && !moduleLabyrinth.IsAlreadySetTrap(self.mazeLayer);
                    m_titleText.text = Util.Format(tip[22], laInfo.labyrinthName, self.mazeLayer, info.itemName);
                    break;
                case LabyrinthPropSubType.AttackProp:
                    m_titleText.text = Util.Format(tip[23], playerData.roleName, info.itemName);
                    break;
                case LabyrinthPropSubType.ProtectedProp:
                    m_titleText.text = Util.Format(tip[24], info.itemName);
                    break;
            }
            
            m_confirmBtn.SetInteractable(interactable);
            for (int i = 0; i < m_bgObjs.Count; i++) m_bgObjs[i].SetActive(i == info.quality - 1);
        }
    }

    /// <summary>
    /// 退出提示界面
    /// </summary>
    public class LabyrinthExitPanel : CustomSecondPanel
    {
        private GameObject m_confirmBtn;

        public Action onConfirmClick;

        public LabyrinthExitPanel(Transform trans) : base(trans)
        {
        }

        public override void InitComponent()
        {
            base.InitComponent();

            m_confirmBtn = transform.Find("confirm_btn").gameObject;
        }

        public override void AddEvent()
        {
            base.AddEvent();
            EventTriggerListener.Get(m_confirmBtn).onClick = OnConfirmBtnClick;
        }

        private void OnConfirmBtnClick(GameObject sender)
        {
            onConfirmClick?.Invoke();

            SetPanelVisible(false);
            transform.parent.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 偷袭面板
    /// </summary>
    public class LabyrinthSneakPanel : CustomSecondPanel
    {
        private Text m_tipText;
        private GameObject m_confirmBtn;
        private PMazePlayer playerData;
        private string msgFormat;

        public LabyrinthSneakPanel(Transform trans) : base(trans)
        {
        }

        public override void InitComponent()
        {
            base.InitComponent();

            m_tipText = transform.Find("tip").GetComponent<Text>();
            m_confirmBtn = transform.Find("confirm_btn").gameObject;
            msgFormat = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 20);
            if (!string.IsNullOrEmpty(msgFormat))
            {
                msgFormat = msgFormat.Replace("\\n", "\n");
            }
        }

        public override void AddEvent()
        {
            base.AddEvent();

            EventTriggerListener.Get(m_confirmBtn).onClick = OnConfirmBtnClick;
        }

        private void OnConfirmBtnClick(GameObject sender)
        {
            if (playerData != null)
                moduleLabyrinth.SendLabyrinthSneak(playerData);
            SetPanelVisible(false);
            transform.parent.gameObject.SetActive(false);
        }

        public void OpenSneakPanel(PMazePlayer player)
        {
            SetPanelVisible(true);
            transform.parent.gameObject.SetActive(true);
            playerData = player;
            m_tipText.text = Util.Format(msgFormat, playerData.roleName);
        }
    }

    /// <summary>
    /// 挑战迷宫显示界面
    /// </summary>
    public class LabyrinthChallengePanel : CustomSecondPanel
    {
        public LabyrinthChallengePanel(Transform trans) : base(trans)
        {
        }

        public LabyrinthInfo laInfo { get; private set; }
        public Image labtrinthIcon { get; private set; }
        private Text m_tipText;
        private Button chanllengeBtn;

        public override void InitComponent()
        {
            base.InitComponent();

            labtrinthIcon = transform.Find("icon").GetComponent<Image>();
            m_tipText = transform.Find("laby_bg/stage_name").GetComponent<Text>();
            chanllengeBtn = transform.Find("enter_btn").GetComponent<Button>();
            laInfo = moduleLabyrinth.labyrinthInfo;
        }

        public override void AddEvent()
        {
            base.AddEvent();

            EventTriggerListener.Get(chanllengeBtn.gameObject).onClick = OnChanllengeClick;
        }

        private void OnChanllengeClick(GameObject sender)
        {
            SetPanelVisible(false);
            moduleLabyrinth.SendChallengeLabyrinth();
        }

        public override void SetPanelVisible(bool visible = true)
        {
            base.SetPanelVisible(visible);

            if (visible)
            {
                int nextLayer = moduleLabyrinth.labyrinthSelfInfo.mazeLayer + 1;
                bool CanChanllenge = nextLayer <= moduleLabyrinth.mazeMaxLayer;
                nextLayer = CanChanllenge ? nextLayer : moduleLabyrinth.mazeMaxLayer;
                m_tipText.text = Util.Format(ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 30), laInfo.labyrinthName, nextLayer);
                chanllengeBtn.SetInteractable(CanChanllenge);
            }
        }

        public void LoadLabyrinthImage()
        {
            if (labtrinthIcon && laInfo) UIDynamicImage.LoadImage(labtrinthIcon.transform, laInfo.icon);
        }
    }

    public class DropInfoPanel : CustomSecondPanel
    {
        public DropInfoPanel(Transform trans) : base(trans)
        {
        }

        private ScrollView m_scroll;
        private DataSource<LabyrinthInfo.LabyrinthReward> m_dataSource;
        private List<LabyrinthInfo.LabyrinthReward> m_rewards = new List<LabyrinthInfo.LabyrinthReward>();

        public override void InitComponent()
        {
            base.InitComponent();
            m_scroll = transform.GetComponent<ScrollView>("info/inner/items");
            m_dataSource = new DataSource<LabyrinthInfo.LabyrinthReward>(null,m_scroll, OnRefreshItem);
        }
        
        private void OnRefreshItem(RectTransform t, LabyrinthInfo.LabyrinthReward data)
        {
            Text rateText = t.GetComponent<Text>("rate");
            Util.SetText(rateText,data.rate.ToString("P2"));
            PropItemInfo info = ConfigManager.Get<PropItemInfo>(data.propId);
            if (!info) info = PropItemInfo.exp;
            Util.SetItemInfo(t, info, 0, 0, false);
        }

        public override void SetPanelVisible(bool visible = true)
        {
            base.SetPanelVisible(visible);
            if (visible && m_rewards.Count == 0 && moduleLabyrinth.labyrinthInfo && moduleLabyrinth.labyrinthInfo.rewards != null)
            {
                m_rewards.AddRange(moduleLabyrinth.labyrinthInfo.rewards);
                m_dataSource?.SetItems(m_rewards);
                m_dataSource.UpdateItems();
            }

            int type = visible ? 1 : 2;
            moduleGlobal.ShowGlobalLayerDefault(type,false);
        }
    }

    public class MapTipPanel : CustomSecondPanel
    {
        public MapTipPanel(Transform trans) : base(trans)
        {
        }

        private List<LabyrinthMapItem> m_labyrinthMapItems = new List<LabyrinthMapItem>();
        private Text m_currentStageText;
        private Text m_lastStageText;
        private string childPrefix = "stage_item_{0}";
        private Text m_countDownText;

        public override void InitComponent()
        {
            base.InitComponent();
            m_labyrinthMapItems.Clear();
            Transform item = null;
            for (int i = 0; i < 8; i++)
            {
                item = transform.Find(Util.Format(childPrefix,i));
                LabyrinthMapItem component = new LabyrinthMapItem(item);
                m_labyrinthMapItems.Add(component);
                component.RefreshLabyrinth(i + 1, moduleLabyrinth.currentLabyrinthID);
            }

            m_currentStageText = transform.GetComponent<Text>("stageNow_Txt");
            Util.SetText(m_currentStageText,ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText,37),moduleLabyrinth.labyrinthInfo.labyrinthName);

            m_lastStageText = transform.GetComponent<Text>("stageLast_Txt");
            m_countDownText = transform.GetComponent<Text>("stateTime_Txt");
            PMazePlayer self = moduleLabyrinth.GetSelfMazeData();
            if(self != null)
            {
                LabyrinthInfo last = ConfigManager.Get<LabyrinthInfo>(self.lastMazeLevel);
                m_lastStageText.gameObject.SetActive(last != null);
                if (last) Util.SetText(m_lastStageText, ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 43), last.labyrinthName);
            }
        }
        
        public void RefreshMapItem()
        {
            int labyrinthId = moduleLabyrinth.currentLabyrinthID;
            for (int i = 0; i < m_labyrinthMapItems.Count; i++)
            {
                m_labyrinthMapItems[i].RefreshState(labyrinthId);
            }
        }

        public void RefreshCountDown(string msg)
        {
            Util.SetText(m_countDownText,msg);
        }
    }

    public class AreaRewardPanel : CustomSecondPanel
    {
        public AreaRewardPanel(Transform trans) : base(trans) { }
        private Text m_titleText;

        private DataSource<PItem2> m_dataSource;
        private ScrollView m_scroll;

        private GameObject noneRoot;

        public override void InitComponent()
        {
            base.InitComponent();
            m_titleText = transform.GetComponent<Text>("stateReward_Text");
            m_scroll = transform.GetComponent<ScrollView>("rewardgroup");
            Transform trans = transform.Find("nothing");
            if(trans != null)
            {
                Text m_noneText = trans.GetComponent<Text>("Text");
                if(m_noneText != null)
                    Util.SetText(m_noneText, ConfigText.GetDefalutString((int)TextForMatType.LabyrinthUIText, 57));
                noneRoot = trans.gameObject;
                noneRoot.SetActive(false);
            }

            m_dataSource = new DataSource<PItem2>(new List<PItem2>(), m_scroll, OnItemRefresh, OnItemClick);
        }

        public void RefreshRewardPanel(EnumLabyrinthArea area)
        {
            int index = area == EnumLabyrinthArea.PromotionArea ? 13 : area == EnumLabyrinthArea.RelegationArea ? 14 : 15;
            Util.SetText(m_titleText,ConfigText.GetDefalutString((int)TextForMatType.LabyrinthUIText,54), ConfigText.GetDefalutString((int)TextForMatType.LabyrinthUIText, index));
            var rewards = moduleLabyrinth.areaRewardDic.Get(area);
            if (rewards == null) rewards = new List<PItem2>();
            if(noneRoot != null )
            {
                noneRoot.SetActive(rewards.Count <= 0);
            }
            m_dataSource.SetItems(rewards);
            m_dataSource.UpdateItems();
        }

        private void OnItemRefresh(RectTransform rt, PItem2 data)
        {
            PropItemInfo info = ConfigManager.Get<PropItemInfo>(data.itemTypeId);
            if (!info) return;

            Util.SetItemInfo(rt, info, data.level, (int)data.num, false);
            Text numtText = rt.GetComponent<Text>("totalNumber");
            if (numtText) Util.SetText(numtText, data.num.ToString());
        }

        private void OnItemClick(RectTransform rt, PItem2 data)
        {
            Logger.LogDetail("refresh............");
        }
    }

    #endregion

    #region item定义 

    /// <summary>
    /// 地图item显示
    /// </summary>
    public class LabyrinthMapItem : CustomSecondPanel
    {
        public LabyrinthMapItem(Transform trans) : base(trans)
        {
        }

        private LabyrinthInfo info;
        private Text m_nameText;
        private GameObject m_overObj;
        private Image m_battleImage;
        private Image m_lockImage;

        public void RefreshLabyrinth(int id, int currentLabyrinthId)
        {
            info = ConfigManager.Get<LabyrinthInfo>(id);
            if (info)
            {
                m_nameText.text = info.labyrinthName;
            }
            RefreshState(currentLabyrinthId);
        }

        public void RefreshState(int currentLabyrinthId)
        {
            if (info == null) return;

            m_overObj.gameObject.SetActive(info.ID < currentLabyrinthId);
            m_battleImage.gameObject.SetActive(info.ID == currentLabyrinthId);
            m_lockImage.gameObject.SetActive(info.ID > currentLabyrinthId);
        }

        public override void InitComponent()
        {
            base.InitComponent();
            m_nameText = transform.Find("texthalo_Img/name_Txt").GetComponent<Text>();
            m_overObj = transform.Find("over").gameObject;
            m_battleImage = transform.Find("battle").GetComponent<Image>();
            m_lockImage = transform.Find("lock").GetComponent<Image>();
        }
    }

    public class LabyrinthPlayerItem : CustomSecondPanel
    {
        public PMazePlayer playerData { get; private set; }
        public int rank { get; private set; }

        private Button playerBtn;
        private Image m_selfBg;
        private Text m_nameText;
        private Text m_rankText;
        private Text m_layersText;
        private GameObject m_stateParent;
        private List<GameObject> m_stateImages = new List<GameObject>();
        string[] stateNames = new string[] { "rest", "battle", "injured", "protected" };
        private Button m_sneakBtn;
        private Transform buffTrans;

        public Action<PMazePlayer> onPlayerClick;
        public Action<PMazePlayer> onSneakClick;

        public bool Interactive
        {
            get { return playerBtn.interactable; }
            set { playerBtn.interactable = value; }
        }

        public LabyrinthPlayerItem(Transform trans) : base(trans)
        {
        }

        public override void InitComponent()
        {
            base.InitComponent();

            playerBtn = gameObject.GetComponent<Button>();
            m_selfBg = transform.Find("self").GetComponent<Image>();
            m_nameText = transform.Find("name").GetComponent<Text>();
            m_rankText = transform.Find("rank").GetComponent<Text>();
            m_layersText = transform.Find("levels").GetComponent<Text>();

            Transform t = transform.Find("state");
            m_stateParent = t.gameObject;
            m_stateImages.Clear();
            for (int i = 0; i < stateNames.Length; i++)
                m_stateImages.Add(t.Find(stateNames[i]).gameObject);

            m_sneakBtn = transform.Find("attack").GetComponent<Button>();
            transform.Find("attack/Text").GetComponent<Text>().text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 18);
            buffTrans = transform.Find("buffs");
            Util.DisableAllChildren(buffTrans);

            playerBtn.interactable = false;
        }

        public override void AddEvent()
        {
            base.AddEvent();

            EventTriggerListener.Get(m_sneakBtn.gameObject).onClick = OnSneakBtnClick;
            playerBtn.onClick.RemoveAllListeners();
            playerBtn.onClick.AddListener(OnPlayerClick);
        }

        public void RefreshLabyrinthPlayer(PMazePlayer data, int rank)
        {
            playerData = data;
            this.rank = rank;
            m_nameText.text = data.roleName;
            m_rankText.text = rank.ToString();
            m_layersText.text = Util.Format(ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 17), data.mazeLayer);

            m_stateParent.SetActive(true);
            int index = GetStateSpriteIndex(data.mazeState);
            for (int i = 0; i < m_stateImages.Count; i++)
                m_stateImages[i].SetActive(i == index);

            if (moduleLabyrinth.labyrinthSelfInfo != null)
                m_sneakBtn.gameObject.SetActive(data.mazeLayer > moduleLabyrinth.labyrinthSelfInfo.mazeLayer && moduleLabyrinth.HasState(data.mazeState, EnumLabyrinthPlayerState.Idle));
            else
                m_sneakBtn.gameObject.SetActive(false);

            bool interactable = true;
            if (moduleLabyrinth.HasState(data.mazeState, EnumLabyrinthPlayerState.ProtectByBell)) interactable &= false;
            if (moduleLabyrinth.HasState(data.mazeState, EnumLabyrinthPlayerState.SneakProtect)) interactable &= false;
            if (moduleLabyrinth.HasState(data.mazeState, EnumLabyrinthPlayerState.InPveBattle)) interactable &= false;
            //判断是否处于结算阶段
            interactable &= (moduleLabyrinth.currentLabyrinthStep == EnumLabyrinthTimeStep.Chanllenge);
            m_sneakBtn.SetInteractable(interactable);
            m_selfBg.gameObject.SetActive(data.roleId == modulePlayer.roleInfo.roleId);
            RefreshBuffIcon();
        }

        public void RefreshBuffIcon()
        {
            Util.DisableAllChildren(buffTrans);
            //自己的显示
            if (playerData.roleId != modulePlayer.roleInfo.roleId) return;

            m_stateParent.SetActive(false);
            m_sneakBtn.gameObject.SetActive(false);
            if (moduleLabyrinth.labyrinthSelfInfo != null)
            {
                ushort[] propids = moduleLabyrinth.labyrinthSelfInfo.attackProps;
                if (propids.Length > 0)
                {
                    //筛选出所有攻击道具
                    List<string> icons = new List<string>();
                    for (int i = 0; i < propids.Length; i++)
                    {
                        PropItemInfo info = ConfigManager.Get<PropItemInfo>(propids[i]);
                        if (info == null || info.itemType != PropType.LabyrinthProp || info.subType != (int)LabyrinthPropSubType.AttackProp)
                            continue;

                        icons.Add(info.icon);
                    }

                    Util.DisableAllChildren(buffTrans, icons.ToArray());
                }
            }
        }

        private int GetStateSpriteIndex(byte state)
        {
            if (moduleLabyrinth.HasState(state, EnumLabyrinthPlayerState.ProtectByBell))        return 3;
            else if (moduleLabyrinth.HasState(state, EnumLabyrinthPlayerState.SneakProtect))    return 2;
            else if (moduleLabyrinth.HasState(state, EnumLabyrinthPlayerState.Battle))          return 1;
            else if (moduleLabyrinth.HasState(state, EnumLabyrinthPlayerState.Idle))            return 0;

            return 0;
        }

        private void OnSneakBtnClick(GameObject sender)
        {
            //Logger.LogDetail("偷袭的玩家的昵称是 {0}", playerData.roleName);

            if (onSneakClick != null && playerData != null)
                onSneakClick(playerData);
        }

        private void OnPlayerClick()
        {
            if (onPlayerClick != null && playerData != null && playerData.roleId != modulePlayer.roleInfo.roleId) onPlayerClick(playerData);
        }
    }

    public class LabyrinthAreaItem : CustomSecondPanel
    {
        //单个显示区域最大不会超过20个玩家信息
        private const int INIT_CREATE_COUNT = 20;

        private LayoutElement m_areaElement;
        private RectTransform m_titleTrans;

        private string[] titleNames = new string[] { "title_buttom", "title_middle", "title_top" };
        private List<GameObject> m_titles = new List<GameObject>();
        private List<Button> m_buttons = new List<Button>();
        private RectTransform m_playersParent;
        public GridLayoutGroup m_playerGrid;
        private GameObject m_playersPrefab;
        public List<LabyrinthPlayerItem> playerItems { get; private set; }
        public EnumLabyrinthArea area { get; private set; }
        public Action<EnumLabyrinthArea> onRewardClick;

        public float GetHeight
        {
            get { return m_titleTrans.sizeDelta.y + m_playersParent.sizeDelta.y; }
        }

        public LabyrinthAreaItem(Transform trans) : base(trans)
        {
        }

        public override void InitComponent()
        {
            base.InitComponent();
            m_areaElement = transform.GetComponent<LayoutElement>();
            m_titleTrans = transform.Find("title").rectTransform();

            m_titles.Clear();
            m_buttons.Clear();
            for (int i = 0; i < titleNames.Length; i++)
            {
                Transform t = m_titleTrans.Find(titleNames[i]);
                if (!t) continue;

                m_titles.Add(t.gameObject);
                m_buttons.Add(t.GetComponent<Button>("reward_Btn"));
            }
            m_playersParent = transform.Find("players").rectTransform();
            m_playerGrid = m_playersParent.GetComponent<GridLayoutGroup>();
            m_playersPrefab = transform.Find("players/player_item").gameObject;
            m_playersPrefab.SetActive(false);

            playerItems = new List<LabyrinthPlayerItem>();
            for (int i = 0; i < INIT_CREATE_COUNT; i++)
            {
                Transform newItem = m_playersParent.AddNewChild(m_playersPrefab);
                newItem.gameObject.SetActive(false);
                LabyrinthPlayerItem component = new LabyrinthPlayerItem(newItem);
                playerItems.Add(component);
            }

            m_areaElement.preferredHeight = GetHeight;
            m_playerGrid.enabled = false;
        }

        public override void AddEvent()
        {
            base.AddEvent();
            foreach (var item in m_buttons)
            {
                item.onClick.RemoveAllListeners();
                item.onClick.AddListener(OnRewardButtonClick);
            }
        }

        private void OnRewardButtonClick()
        {
            onRewardClick?.Invoke(area);
        }

        public void RefreshAward(EnumLabyrinthArea area)
        {
            this.area = area;
            SetAreaTitle(area);
        }

        public void SetAreaTitle(EnumLabyrinthArea area)
        {
            int index = (int)area - 1;
            for (int i = 0; i < m_titles.Count; i++) m_titles[i].SetActive(i == index);
        }

        public void RefreshPlayer(int startIndex, List<PMazePlayer> playerDatas)
        {
            int maxCount = playerDatas == null ? 0 : playerDatas.Count;

            for (int i = 0; i < playerItems.Count; i++)
            {
                playerItems[i].gameObject.SetActive(i < maxCount);
                if (i < maxCount)
                {
                    playerItems[i].RefreshLabyrinthPlayer(playerDatas[i], startIndex + i + 1);
                }
            }

            //重新计算整体区域的高度
            m_areaElement.preferredHeight = m_titleTrans.sizeDelta.y;
            float childHight = (maxCount - 1) * m_playerGrid.spacing.y + maxCount * m_playerGrid.cellSize.y;
            m_areaElement.preferredHeight += childHight;
            m_playersParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, childHight);
            m_playerGrid.enabled = true;
        }

    }

    #endregion

    #region static function
    public static void SetLabyrinthImgAsCurrent(Image img)
    {
        if (!img) return;

        string sprite = moduleLabyrinth.currentLabyrinthStep == EnumLabyrinthTimeStep.Chanllenge ? "ui_labyrinth_chanllenge" : "ui_labyrinth_settlement";
        AtlasHelper.SetShared(img, sprite);
    }

    #endregion

    /// <summary>
    /// 猎人草药ID
    /// </summary>
    private const int HUNTER_HERB_ID = 8302;

    private Button m_exitBtn;
    private Button m_shopBtn;
    private Button m_testBtn;
    private Button m_chatBtn;
    private Button m_helpBtn;
    private Button m_showPlayerBtn;
    private Button m_hidePlayerBtn;

    private Text roleName;
    private Slider m_healthSlider;
    private Slider m_angerSlider;
    private List<GameObject> m_angerEffect = new List<GameObject>();
    private Text m_healthText;
    private Text m_recoveryTip;
    private string m_recoveryFormat = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 2);
    private Transform m_petIcon;
    private TimeSpan m_recoveryCountDown;
    private float m_lastTime;

    private Image m_stateImg;
    private Text m_stateCountdownText;

    private Text m_stageInfoText;
    private Text m_stageProgressText;
    private Text m_curLevelTexts;
    private Image m_stageProgressBar;
    private GameObject m_newStageObj;

    private UIMovementPad m_movementPad;
    private CanvasRenderer m_leftCR, m_rightCR, m_upCR, m_bottomCR;
    private int m_lastMoveKey;

    private LabyrinthPlayerPanel m_playerPanel;
    private LabyrinthPropPanel m_propPanel;
    private LabyrinthUsePropPanel m_userPropPanel;
    private LabyrinthChallengePanel m_challengePanel;
    private LabyrinthExitPanel m_exitPanel;
    private LabyrinthReportPanel m_reportPanel;
    private LabyrinthSneakPanel m_sneakPanel;
    private DropInfoPanel m_dropInfoPanel;
    private MapTipPanel m_mapTipPanel;
    private AreaRewardPanel m_areaRewardPanel;
    private bool m_inAlertWindow = false;
    private EnumLabyrinthTimeStep m_lastStep = EnumLabyrinthTimeStep.None;

    protected override void OnOpen()
    {
        roleName        = GetComponent<Text>("labyrinth_panel/top/Text");
        m_exitBtn       = GetComponent<Button>("labyrinth_panel/top/exit");
        m_shopBtn       = GetComponent<Button>("labyrinth_panel/bottom/shop_btn");
        m_testBtn       = GetComponent<Button>("labyrinth_panel/bottom/test_btn");

        m_stageInfoText = GetComponent<Text>("stageInfo/stageName_Txt");
        m_stageProgressText = GetComponent<Text>("stageInfo/stageLayerNow");
        m_curLevelTexts = GetComponent<Text>("stageInfo/stageRankNumber_Txt");
        m_stageProgressBar = GetComponent<Image>("stageInfo/stageInfoProgressBar_Img");
        m_newStageObj = transform.Find("stageInfo/newinfo_Img").gameObject;
        m_newStageObj?.gameObject.SetActive(false);

        m_stateCountdownText = GetComponent<Text>("stateSign/stateSign_Txt");
        m_stateImg = GetComponent<Image>("stateSign/stateSign_Txt/stateSignIcon");

        m_petIcon       = GetComponent<Transform>("labyrinth_panel/top/sprite_avatar");
        m_chatBtn       = GetComponent<Button>("labyrinth_panel/bottom/chat_btn");
        m_helpBtn       = GetComponent<Button>("labyrinth_panel/bottom/help_btn");
        m_showPlayerBtn = GetComponent<Button>("labyrinth_panel/bottom/show_player_btn");
        m_hidePlayerBtn = GetComponent<Button>("labyrinth_panel/bottom/hide_player_btn");

        if (modulePet.FightingPet != null)
        {
            m_petIcon.SafeSetActive(true);
            Util.SetPetSimpleInfo(m_petIcon, modulePet.FightingPet);
        }
        else
            m_petIcon.SafeSetActive(false);

        if (m_testBtn != null) m_testBtn.gameObject.SetActive(false);
#if UNITY_EDITOR
        if (m_testBtn != null) m_testBtn.gameObject.SetActive(true);
#endif

        EventTriggerListener.Get(m_exitBtn.gameObject).onClick          = OnExitBtnClick;
        EventTriggerListener.Get(m_shopBtn.gameObject).onClick          = OnShopBtnClick;
        EventTriggerListener.Get(m_testBtn.gameObject).onClick          = OnTestBtnClick;
        EventTriggerListener.Get(m_chatBtn.gameObject).onClick          = OnChatClick;
        EventTriggerListener.Get(m_hidePlayerBtn.gameObject).onClick    = OnHidePlayerClick;
        EventTriggerListener.Get(m_showPlayerBtn.gameObject).onClick    = OnShowPlayerClick;
        EventTriggerListener.Get(m_helpBtn.gameObject).onClick          = OnHelpBtnClick;


        m_healthSlider = GetComponent<Slider>("labyrinth_panel/top/role_bg/hp/hp_bar");
        m_angerSlider = GetComponent<Slider>("labyrinth_panel/top/role_bg/anger/anger_bar");
        m_healthText = GetComponent<Text>("labyrinth_panel/top/role_bg/hp/hp");
        m_recoveryTip = GetComponent<Text>("labyrinth_panel/top/role_bg/recover_tip");
        m_angerEffect.Clear();
        string[] effects = new string[] { "fengetiao", "fengetiao2", "fengetiao3" };
        for (int i = 0; i < effects.Length; i++)
        {
            Transform t = transform.Find(Util.Format("labyrinth_panel/top/role_bg/anger/{0}", effects[i]))?.GetChild(0);
            if(t) m_angerEffect.Add(t.gameObject);
        }

        IniteText();

        m_playerPanel = new LabyrinthPlayerPanel(transform.Find("player_panel"));
        m_playerPanel.SetPanelVisible(true);
        m_playerPanel.onPlayerChoose = OnPlayerChooseForProp;
        m_playerPanel.onPlayerSneak = OnPlayerSneakCallback;
        m_playerPanel.onReportClick = OnReportBtnClick;
        m_playerPanel.onPropClick = OnPropBtnClick;
        m_playerPanel.onPanelOpenStateChange = OnPlayerPanelOpenStateChange;
        foreach (var item in m_playerPanel.areaDic)
        {
            item.Value.onRewardClick = OnRewardClick;
        }

        m_propPanel = new LabyrinthPropPanel(transform.Find("proplist_panel"));
        m_propPanel.SetPanelVisible(false);
        m_propPanel.OnPropItemClick = OpenUsePropPanel;
        m_propPanel.onCancelChoosePlayer = OnPropPanelCancle;

        m_reportPanel = new LabyrinthReportPanel(transform.Find("battle_report_panel"));
        m_reportPanel.SetPanelVisible(false);

        m_userPropPanel = new LabyrinthUsePropPanel(transform.Find("center/use_prop_panel"));
        m_userPropPanel.SetPanelVisible(false);
        m_userPropPanel.onCancelClick = OnUserPropPanelCancel;

        m_challengePanel = new LabyrinthChallengePanel(transform.Find("enter_labyrinth_panel"));
        m_challengePanel.LoadLabyrinthImage();
        m_challengePanel.SetPanelVisible(false);

        m_exitPanel = new LabyrinthExitPanel(transform.Find("center/exit_confirm_panel"));
        m_exitPanel.SetPanelVisible(false);
        m_exitPanel.onConfirmClick = OnExitCallback;

        m_sneakPanel = new LabyrinthSneakPanel(transform.Find("center/sneak_confirm_panel"));
        m_sneakPanel.SetPanelVisible(false);

        m_dropInfoPanel = new DropInfoPanel(transform.Find("dropInfo"));
        m_dropInfoPanel.SetPanelVisible(false);

        m_mapTipPanel = new MapTipPanel(transform.Find("tips"));
        m_mapTipPanel.SetPanelVisible(false);
        m_mapTipPanel.RefreshMapItem();

        m_areaRewardPanel = new AreaRewardPanel(transform.Find("preview_panel"));
        m_areaRewardPanel.SetPanelVisible(false);

        m_movementPad = GetComponent<UIMovementPad>("movementPad");
        m_leftCR = GetComponent<CanvasRenderer>("movementPad/left");
        m_rightCR = GetComponent<CanvasRenderer>("movementPad/right");
        m_upCR = GetComponent<CanvasRenderer>("movementPad/up");
        m_bottomCR = GetComponent<CanvasRenderer>("movementPad/bottom");
        SetCanvasRenderAlpha(0.6f);
        m_movementPad.onTouchMove.AddListener(OnMovementPadMove);
        m_movementPad.onTouchEnd.AddListener(OnMovementPadStop);
    }

    private void IniteText()
    {
        GetComponent<Text>("labyrinth_panel/top/role_bg/recover_tip").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 2);
        GetComponent<Text>("labyrinth_panel/exit/Text").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 3);
        GetComponent<Text>("labyrinth_panel/bottom/shop_btn/Text/").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 7);
        GetComponent<Text>("player_panel/left/playerlist/prop_btn/Text").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 6);
        GetComponent<Text>("player_panel/left/playerlist/battle_field_btn/Text").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 5);

        GetComponent<Text>("player_panel/left/open/Text").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 8);
        GetComponent<Text>("player_panel/left/open/Text (1)").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 9);

        GetComponent<Text>("player_panel/left/playerlist/viewport/content/area_0/title/title_top/name").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 13);
        GetComponent<Text>("player_panel/left/playerlist/viewport/content/area_0/title/title_middle/name").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 14);
        GetComponent<Text>("player_panel/left/playerlist/viewport/content/area_0/title/title_buttom/name").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 15);
        
        GetComponent<Text>("center/tip_prop/top/equipinfo").text = ConfigText.GetDefalutString(TextForMatType.PublicUIText, 25);
        GetComponent<Text>("center/exit_confirm_panel/tip").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 19);
        GetComponent<Text>("center/exit_confirm_panel/confirm_btn/Text").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 31);

        GetComponent<Text>("center/sneak_confirm_panel/confirm_btn/Text").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 18);

        GetComponent<Text>("center/use_prop_panel/use/tip").text = ConfigText.GetDefalutString(TextForMatType.PublicUIText, 15);
        GetComponent<Text>("center/use_prop_panel/confirm_btn/Text").text = ConfigText.GetDefalutString(TextForMatType.PublicUIText, 4);

        GetComponent<Text>("enter_labyrinth_panel/enter_btn/Text").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 25);

        GetComponent<Text>("battle_report_panel/prop_list/title_Txt").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 26);
        GetComponent<Text>("battle_report_panel/nothing/nothing_text").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 35);

        GetComponent<Text>("proplist_panel/prop_list/title_Txt").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 33);
        GetComponent<Text>("proplist_panel/prop_list/info/type_bg/trap/Text").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 27);
        GetComponent<Text>("proplist_panel/prop_list/info/type_bg/attack/Text").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 28);
        GetComponent<Text>("proplist_panel/prop_list/info/type_bg/protect/Text").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 29);
        GetComponent<Text>("proplist_panel/prop_list/ues_button/Text").text = ConfigText.GetDefalutString(TextForMatType.PublicUIText, 23);
        GetComponent<Text>("proplist_panel/prop_list/close_button/Text").text = ConfigText.GetDefalutString(TextForMatType.PublicUIText, 0);
        GetComponent<Text>("proplist_panel/middle/mask/tip_bg/tip").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 11);
        GetComponent<Text>("proplist_panel/middle/mask/tip_bg/closetip").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 12);
        GetComponent<Text>("preview_panel/bg/equip_prop/top/equipinfo").text = ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 55);

        var t = ConfigManager.Get<ConfigText>((int)TextForMatType.LabyrinthUIText);
        if (!t) return;
        Util.SetText(GetComponent<Text>("dropInfo/info/title"), t[38]);
        Util.SetText(GetComponent<Text>("dropInfo/info/inner/rulestitle"), t[39]);
        Util.SetText(GetComponent<Text>("dropInfo/info/inner/labyrinth_rules/Viewport/Content/rulescontent"), t[40]);
        Util.SetText(GetComponent<Text>("dropInfo/info/inner/rewardTitle"), t[41]);

        Util.SetText(GetComponent<Text>("tips/top/equipinfo"), t[42]);

    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        roleName.text = modulePlayer.roleInfo.roleName;
        moduleLabyrinth.SendLabyrinthSelfInfo();
        moduleGlobal.ShowGlobalLayerDefault(2, false);
        moduleLabyrinth.moveMentKey = 0;
        SetControlPlayerBtnVisible();
    }

    public override void OnUpdate(int diff)
    {
        base.OnUpdate(diff);
        RecoveryCountDown();
    }

    private void SetPlayerRecovery(bool enable,float health)
    {
        m_recoveryTip.gameObject.SetActive(enable);
        enableUpdate = health < 100;
        m_lastTime = enableUpdate ? Time.realtimeSinceStartup : 0;
        if (health < 100)
        {
            ushort unitTime = moduleLabyrinth.pmazeInfo == null ? ushort.MinValue : moduleLabyrinth.pmazeInfo.hpUnitTime;
            if (unitTime == 0) unitTime = 360;
            m_recoveryCountDown = new TimeSpan(0,0,Mathf.CeilToInt((100 - health) * unitTime));
            Util.SetText(m_recoveryTip, m_recoveryFormat, m_recoveryCountDown.Hours, m_recoveryCountDown.Minutes, m_recoveryCountDown.Seconds);
        }
    }

    private void RecoveryCountDown()
    {
        if (m_lastTime == 0 || m_healthSlider.value == 1) return;

        if(Time.realtimeSinceStartup - m_lastTime >= 1.0f)
        {
            m_recoveryCountDown -= Module_Labyrinth.ONE_SECOND_TIME_SPAN;
            m_lastTime = Time.realtimeSinceStartup;
            Util.SetText(m_recoveryTip, m_recoveryFormat, m_recoveryCountDown.Hours, m_recoveryCountDown.Minutes, m_recoveryCountDown.Seconds);
        }
    }

    private bool IsCanClick()
    {
        if (moduleLabyrinth.labyrinthSelfInfo == null ||
            moduleLabyrinth.HasState(moduleLabyrinth.labyrinthSelfInfo.mazeState, EnumLabyrinthPlayerState.ForceRest))
            return false;

        return true;
    }

    #region 迷宫界面按钮点击事件

    private void OnExitBtnClick(GameObject sender)
    {
        m_exitPanel.SetPanelVisible(true);
        m_exitPanel.transform.parent.gameObject.SetActive(true);
    }

    private void OnExitCallback()
    {
        moduleLabyrinth.SendExitLabyrinth();
        Hide(true);
        Game.GoHome();
    }

    private void OnShopBtnClick(GameObject sender)
    {
        ShowAsync<Window_Exchangeshop>();
    }

    private void OnPropBtnClick()
    {
        moduleLabyrinth.SendLabyrinthProps();
    }

    private void OnReportBtnClick()
    {
        moduleLabyrinth.SendRequestBattleReports();
    }

    private void OnTestBtnClick(GameObject sender)
    {
        moduleLabyrinth.SendRequestTestData();
    }

    private void OnChanllengeBtnClick(GameObject sender)
    {
        if(moduleLabyrinth.labyrinthSelfInfo.mazeLayer >= moduleLabyrinth.mazeMaxLayer)
        {
            moduleGlobal.ShowMessage(10004,1);
            return;
        }
		
        if (!IsCanClick())
        {
            m_inAlertWindow = true;
            Window_Alert.ShowAlert(ConfigText.GetDefalutString(TextForMatType.AlertUIText, 6),true,false,false,()=>
            {
                m_inAlertWindow = false;
            });
            return;
        }
		
        m_challengePanel.SetPanelVisible(true);
    }

    private void OnChatClick(GameObject sender)
    {
        moduleChat.opChatType = OpenWhichChat.WorldChat;
        ShowAsync<Window_Chat>();
    }

    private void OnShowPlayerClick(GameObject sender)
    {
        moduleLabyrinth.playerVisible = true;
    }

    private void OnHidePlayerClick(GameObject sender)
    {
        moduleLabyrinth.playerVisible = false;
    }

    private void SetControlPlayerBtnVisible()
    {
        m_hidePlayerBtn.gameObject.SetActive(moduleLabyrinth.playerVisible);
        m_showPlayerBtn.gameObject.SetActive(!moduleLabyrinth.playerVisible);
    }

    private void OnPlayerPanelOpenStateChange(bool open)
    {
        if (open) OnReportBtnClick();
    }

    private void OnHelpBtnClick(GameObject sender)
    {
        m_dropInfoPanel?.SetPanelVisible(true);
    }

    protected override void OnReturn()
    {
        if (m_dropInfoPanel != null && m_dropInfoPanel.enable) m_dropInfoPanel.SetPanelVisible(false);
    }

    private void OnRewardClick(EnumLabyrinthArea area)
    {
        if (m_areaRewardPanel != null) m_areaRewardPanel.RefreshRewardPanel(area);
    }
    #endregion

    #region 二级界面回调

    private void OnPlayerChooseForProp(PMazePlayer playerData)
    {
        m_userPropPanel.ShowUserPropPanel(m_propPanel.clickPropId, LabyrinthPropSubType.AttackProp, playerData);
    }

    private void OnPlayerSneakCallback(PMazePlayer playerData)
    {
        m_sneakPanel.OpenSneakPanel(playerData);
    }

    private void OpenUsePropPanel(ushort propId, LabyrinthPropSubType type)
    {
        if (type == LabyrinthPropSubType.AttackProp)
        {
            m_playerPanel.SetPlayerItemsInteractive(true);
            m_playerPanel.scroll.verticalNormalizedPosition = 1f;
        }
        else
        {
            m_userPropPanel.ShowUserPropPanel(propId, type);
        }
    }

    private void OnUserPropPanelCancel(LabyrinthPropSubType type)
    {
        OnPropPanelCancle();
    }

    private void OnPropPanelCancle()
    {
        m_propPanel.clickPropId = 0;
        m_propPanel.maskPanel.SetActive(false);
        m_playerPanel.SetPlayerItemsInteractive(false);
    }

    #endregion


    #region touch event

    private void OnMovementPadMove(Vector2 dir, Vector2 delta)
    {
        if (m_challengePanel.enable || m_inAlertWindow) return;

        moduleLabyrinth.moveMentKey = 0x00;
        if (dir.x < -Window_Bordlands.MOVEPAD_THRESHOLD) moduleLabyrinth.moveMentKey |= 0x02;
        if (dir.x > Window_Bordlands.MOVEPAD_THRESHOLD) moduleLabyrinth.moveMentKey |= 0x08;
        if (dir.y > Window_Bordlands.MOVEPAD_THRESHOLD) moduleLabyrinth.moveMentKey |= 0x01;
        if (dir.y < -Window_Bordlands.MOVEPAD_THRESHOLD) moduleLabyrinth.moveMentKey |= 0x04;

        if (m_lastMoveKey == moduleLabyrinth.moveMentKey) return;

        m_lastMoveKey = moduleLabyrinth.moveMentKey;
        if (moduleLabyrinth.moveMentKey != 0)
        {
            m_leftCR.SetAlpha(dir.x < -Window_Bordlands.MOVEPAD_THRESHOLD ? 1 : 0.6f);
            m_rightCR.SetAlpha(dir.x > Window_Bordlands.MOVEPAD_THRESHOLD ? 1 : 0.6f);
            m_upCR.SetAlpha(dir.y > Window_Bordlands.MOVEPAD_THRESHOLD ? 1 : 0.6f);
            m_bottomCR.SetAlpha(dir.y < -Window_Bordlands.MOVEPAD_THRESHOLD ? 1 : 0.6f);
        }
    }

    private void OnMovementPadStop(Vector2 p)
    {
        SetCanvasRenderAlpha(0.6f);
        moduleLabyrinth.moveMentKey = 0;
        m_lastMoveKey = 0;
    }

    private void SetCanvasRenderAlpha(float alpha)
    {
        m_leftCR.SetAlpha(alpha);
        m_rightCR.SetAlpha(alpha);
        m_upCR.SetAlpha(alpha);
        m_bottomCR.SetAlpha(alpha);
    }

    #endregion

    private void _ME(ModuleEvent<Module_Labyrinth> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Labyrinth.EventRefreshSelf:
                ScMazeSelfInfo self = e.msg as ScMazeSelfInfo;

                float newHealthRate = self.healthRate * 0.01f;
                if (m_healthSlider.value != newHealthRate)
                {
                    DOTween.To(() => m_healthSlider.value, x => m_healthSlider.value = x, newHealthRate, 0.2f);
                }
                m_healthText.text = Util.Format("{0}%", self.healthRate.ToString("f0"));
                
                var anger = self.angerRate * 0.01f;
                int count = (int)(anger / 0.33f);
                var percent = anger % 0.33f / 0.33f;
                percent = count > 0 && percent < 0.033f ? 1.0f : percent;
                m_angerSlider.value = percent;
                for (int i = 0; i < m_angerEffect.Count; i++) m_angerEffect[i].SetActive(i < count);

                bool forceRest = moduleLabyrinth.HasState(moduleLabyrinth.labyrinthSelfInfo.mazeState, EnumLabyrinthPlayerState.ForceRest);
                SetPlayerRecovery(forceRest, self.healthRate);

                LabyrinthInfo info = moduleLabyrinth.labyrinthInfo;
                Util.SetText(m_stageInfoText, info.labyrinthName);
                Util.SetText(m_stageProgressText, ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 10),self.mazeLayer,moduleLabyrinth.mazeMaxLayer);
                if(m_stageProgressBar) m_stageProgressBar.fillAmount = self.mazeLayer * 1.0f / moduleLabyrinth.mazeMaxLayer;

                EnumLabyrinthArea area = moduleLabyrinth.GetPlayerArea(modulePlayer.roleInfo.roleId);
                int id = 13;
                switch (area)
                {
                    case EnumLabyrinthArea.DemotionArea: id = 15; break;
                    case EnumLabyrinthArea.RelegationArea: id = 14; break;
                    case EnumLabyrinthArea.PromotionArea: id = 13; break;
                }
                Util.SetText(m_curLevelTexts, ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, 36),moduleLabyrinth.GetPlayerRank(modulePlayer.roleInfo.roleId) ,ConfigText.GetDefalutString(TextForMatType.LabyrinthUIText, id));
               
                if (m_propPanel.enable) m_propPanel.RefreshLastClickItem();
                break;

            case Module_Labyrinth.EventRefreshPlayers:
                Dictionary<EnumLabyrinthArea, List<PMazePlayer>> dic = e.param1 as Dictionary<EnumLabyrinthArea, List<PMazePlayer>>;
                m_playerPanel.RefreshPlayerPanel(dic);
                break;

            case Module_Labyrinth.EventRefreshProps:
                m_propPanel.SetPanelVisible(true);
                Dictionary<ushort, PItem2> propDic = e.param1 as Dictionary<ushort, PItem2>;
                m_propPanel.RefreshProp(propDic);
                break;

            case Module_Labyrinth.EventPropChange:

                Dictionary<ushort, PItem2> changePropDic = e.param1 as Dictionary<ushort, PItem2>;

                //道具改变的时候只有当道具界面打开的时候才进行刷新
                if (m_propPanel.enable) m_propPanel.RefreshProp(changePropDic);
                break;

            case Module_Labyrinth.EventRefreshUseButtonState:
                //道具改变的时候只有当道具界面打开的时候才进行刷新
                if (m_propPanel.enable) m_propPanel.RefreshLastClickItem();
                break;

            case Module_Labyrinth.EventBattleReport:
                List<PMazeReport> reports = e.param1 as List<PMazeReport>;
                m_reportPanel.RefreshReportPanel(reports);
                break;

            case Module_Labyrinth.EventRefreshReport:
                List<PMazeReport> rps = e.param1 as List<PMazeReport>;
                if (m_reportPanel.enable) m_reportPanel.RefreshReportPanel(rps);
                break;

            //偷袭成功跳转场景
            case Module_Labyrinth.EventSneakPlayer:
                //跳转到偷袭的场景
                Game.LoadLevel(7);
                break;

            //进入结算阶段，需要强制退出
            case Module_Labyrinth.EventEnterSettlementStep:
                Game.GoHome();
                break;

            case Module_Labyrinth.EventRefreshSelfAttackProp:
                m_playerPanel.RefreshSelfAttackProp();
                break;

            case Module_Labyrinth.EventTriggerSceneCollider:
                HandleClickSceneObj((e.param1 as LabyrinthCreature).transform);
                OnMovementPadStop(Vector2.zero);
                break;

            case Module_Labyrinth.EventLabyrinthTimeRefresh: 
                UpdateLabyrinthStep((EnumLabyrinthTimeStep)e.param1);
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Bordlands> e)
    {
        if (!actived) return;

        switch (e.moduleEvent)
        {
            case Module_Bordlands.EventClickScenePlayerSuccess:
                HandleClickPlayer(e.param1 as LabyrinthCreature);
                break;

            case Module_Bordlands.EventClickSceneObjSuccess:
                HandleClickSceneObj(e.param1 as Transform);
                break;
        }
    }

    private void HandleClickPlayer(LabyrinthCreature lc)
    {
        if (lc && lc.roleInfo != null && lc.playerType != LabyrinthCreature.LabyrinthCreatureType.Self )
        {
            int index = -1;
            if (moduleLabyrinth.HasState(lc.roleInfo.mazeState, EnumLabyrinthPlayerState.ProtectByBell)) index = 0;
            else if (moduleLabyrinth.HasState(lc.roleInfo.mazeState, EnumLabyrinthPlayerState.SneakProtect)) index = 1;
            else if (moduleLabyrinth.HasState(lc.roleInfo.mazeState, EnumLabyrinthPlayerState.ForceRest)) index = 2;
            else if (moduleLabyrinth.HasState(lc.roleInfo.mazeState, EnumLabyrinthPlayerState.Battle)) index = 3;
            else if (moduleLabyrinth.labyrinthSelfInfo != null && moduleLabyrinth.labyrinthSelfInfo.mazeLayer >= lc.roleInfo.mazeLayer) index = 4;
            else if (moduleLabyrinth.labyrinthSelfInfo != null && moduleLabyrinth.HasState(moduleLabyrinth.labyrinthSelfInfo.mazeState, EnumLabyrinthPlayerState.ForceRest)) index = 5;
            else if (moduleLabyrinth.labyrinthSelfInfo != null && moduleLabyrinth.labyrinthSelfInfo.healthRate > 0)
            {
                OnPlayerSneakCallback(lc.roleInfo);
            }

            if(index >= 0)
            {
                string msg = ConfigText.GetDefalutString(10010, index);
                moduleGlobal.ShowMessage(msg);
            }

        }
    }

    private void HandleClickSceneObj(Transform trans)
    {
        //Logger.LogInfo("{1} click scene obj name is {0}",trans.name,Time.time);
        OnChanllengeBtnClick(trans.gameObject);
    }

    private void UpdateLabyrinthStep(EnumLabyrinthTimeStep step)
    {
        //refresh count down
        string str = moduleLabyrinth.GetStepAndTimeString(false);
        Util.SetText(m_stateCountdownText, str);
        if (m_mapTipPanel != null) m_mapTipPanel.RefreshCountDown(str);
        if (m_playerPanel != null) m_playerPanel.RefreshCountDown(str);

        //refresh stateImg
        if (m_lastStep != step)
        {
            m_lastStep = step;
            if (m_playerPanel != null) m_playerPanel.RefreshStateImg();
            SetLabyrinthImgAsCurrent(m_stateImg);
        }
    }
}
