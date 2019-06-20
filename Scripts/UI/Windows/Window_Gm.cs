/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-10-14
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Window_Gm : Window
{
    #region custom class
    public enum EnumOnInputPanelType
    {
        StagePanel,

        DialogPanel,

        GuidePanel,

        ChasePanel,
    }

    public enum EnumDatingInputPanelType
    {
        DatingEventPanel,
        DatingAnswerPanel,
    }

    public class OneInputParamPanel : CustomSecondPanel
    {
        private Text m_tipText;
        private InputField m_input;
        private GameObject confirmBtn;
        private GameObject cancelBtn;
        private Text m_tipText1;
        private InputField m_input1;

        public OneInputParamPanel(Transform trans) : base(trans)
        {
        }
        
        public EnumOnInputPanelType panelType { get; set; }

        public Action cancelCallback;

        public override void InitComponent()
        {
            base.InitComponent();
            m_tipText = transform.GetComponent<Text>("text/Text");
            m_input = transform.GetComponent<InputField>("input");
            confirmBtn = transform.Find("sure").gameObject;
            cancelBtn = transform.Find("unSure").gameObject;
        }

        public override void AddEvent()
        {
            base.AddEvent();
            EventTriggerListener.Get(confirmBtn).onClick = OnConfirmClick;
            EventTriggerListener.Get(cancelBtn).onClick = OnCancelClick;
        }

        public override void SetPanelVisible(bool visible = true)
        {
            base.SetPanelVisible(visible);
            if(visible)
            {
                m_input.text = string.Empty;
                switch (panelType)
                {
                    case EnumOnInputPanelType.StagePanel:
                        m_tipText.text = "强制加载的场景ID(还原只需设置为负数):";
                        break;
                    case EnumOnInputPanelType.DialogPanel:
                        m_tipText.text = "打开测试的对话ID:";
                        break;

                    case EnumOnInputPanelType.GuidePanel:
                        m_tipText.text = "测试新手引导ID:";
                        break;

                    case EnumOnInputPanelType.ChasePanel:
                        m_tipText.text = "需要的星级通关(0-3):";
                        m_input.text = "3";
                        break;
                }
            }
        }

        private void OnConfirmClick(GameObject sender)
        {
            bool success = false;

            int id = 0;
            string[] strp = new string[] { m_input.text };
            if (m_input.text.Contains(",")) strp = m_input.text.Split(',');
            else if(m_input.text.Contains(";")) strp = m_input.text.Split(';');

            if (int.TryParse(strp[0], out id))
            {
                switch (panelType)
                {
                    case EnumOnInputPanelType.StagePanel:   success = GMStage(id);      break;
                    case EnumOnInputPanelType.GuidePanel:   success = GMGuide(id);      break;
                    case EnumOnInputPanelType.ChasePanel:   success = GMChase(id);      break;
                    case EnumOnInputPanelType.DialogPanel:
                        int type = Util.Parse<int>(strp.GetValue<string>(1));
                        type = type == 4 ? 4 : 1;
                        success = GMDialog(id, type == 0 ? 1 : type);
                        break;
                }
            }
            
            if (success)
            {
                SetPanelVisible(false);
                Hide<Window_Gm>();
            }
        }

        private bool GMStage(int id)
        {
            bool success = false;
            if (id < 0)
            {
                Module_PVE.TEST_SCENE_EVENT_ID = id;
                success = true;
            }
            else if (id > 0)
            {
                var info = ConfigManager.Get<StageInfo>(id);
                if (info)
                {
                    Module_PVE.TEST_SCENE_EVENT_ID = info.sceneEventId;
                    success = true;
                }
                else Logger.LogError("StageInfo.Id = {0} connot be finded", id);
            }

            return success;
        }

        private bool GMDialog(int id,int type = 1)
        {
            bool success = false;
            StoryInfo story = ConfigManager.Get<StoryInfo>(id);
            if (story)
            {
                success = true;
                moduleStory.debugStory = true;
                Module_Story.ShowStory(id, (EnumStoryType)type);
            }
            else Logger.LogError("StoryInfo.Id = {0} connot be finded", id);
            return success;
        }

        public static bool GMGuide(int id)
        {
            bool success = false;
            GuideInfo guide = ConfigManager.Get<GuideInfo>(id);
            if (guide)
            {
                success = true;
                moduleGuide.LoadTargetGuideOnEditorMode(guide);

                if (Module_Guide.DEFALUT_OPERATE_GUIDES.Contains(guide.ID))
                {
                    modulePVE.OnPVEStart(Module_Story.DEFAULT_STAGE_ID, PVEReOpenPanel.None);
                    return success;
                }

                //cache condition paramers
                string windowName = string.Empty;
                string conditionStr = string.Empty;
                Module_Guide.GM_PLATER_LEVEL = 0;

                //find condition
                foreach (var item in guide.conditions)
                {
                    if (item.type == EnumGuideContitionType.OpenWindow)
                    {
                        windowName = item.strParames;
                        conditionStr = item.strParames;
                        if (moduleGuide.IsOpenWindowHome(conditionStr)) windowName = Game.GetDefaultName<Window_Home>();
                        else if (moduleGuide.IsOpenWindowChase(conditionStr)) windowName = Game.GetDefaultName<Window_Chase>();
                        else if (moduleGuide.IsOpenWindowNpcGaiden(conditionStr)) windowName = Game.GetDefaultName<Window_NpcGaiden>();

                        Window w = GetOpenedWindow(windowName);
                        if (w && w.actived) Module_Guide.AddCondition(new BaseGuideCondition(item));

                        if (w && w.actived && w is Window_Home) ((Window_Home)w).SwitchTo((int)moduleGuide.GetSubwindowTypeForWindowHome(conditionStr));
                        if (w && w.actived && w is Window_Chase) ((Window_Chase)w).SwitchToSubWindow(/*moduleGuide.GetSubwindowTypeForWindowChase(conditionStr)*/);
                    }
                    else if (item.type == EnumGuideContitionType.PlayerLevel) Module_Guide.GM_PLATER_LEVEL = item.intParames[0];
                    else Module_Guide.AddCondition(new BaseGuideCondition(item));
                }

                //create condition
                if (Module_Guide.GM_PLATER_LEVEL > 0) moduleGuide.CreateLevelConditionOnEditorMode(Module_Guide.GM_PLATER_LEVEL);
                if (!string.IsNullOrEmpty(windowName))
                {
                    if(windowName.Equals(Game.GetDefaultName<Window_Home>())) ShowAsync<Window_Home>(null,(w)=> {
                        if (w) (w as Window_Home).SwitchTo((int)moduleGuide.GetSubwindowTypeForWindowHome(conditionStr));
                    });
                    else if (windowName.Equals(Game.GetDefaultName<Window_Chase>()))
                    {
                        GotoSubWindow<Window_Chase>((int)moduleGuide.GetSubwindowTypeForWindowChase(conditionStr));
                    }
                    else if (windowName.Equals(Game.GetDefaultName<Window_NpcGaiden>()))
                    {
                        ShowAsync<Window_NpcGaiden>(null, 
                            (w) => (w as Window_NpcGaiden)?.GotoSubWindow(moduleGuide.GetSubWindowTypeForGaidenWindow(conditionStr), TaskType.GaidenChapterOne ));
                    }
                    else ShowAsync(windowName);
                }
            }
            else Logger.LogError("GuideInfo.Id = {0} connot be finded", id);

            return success;
        }

        private bool GMChase(int id)
        {
            bool suceess = false;
            int level = Mathf.Clamp(id,0,3);
            if (level == 1) level = 1;
            else if (level == 2) level = 3;
            else if (level == 3) level = 7;
            moduleGm.SendUnlockAllChase(level);

            moduleChase.InitChaseData();
            moduleChase.InitLevelForType();
            moduleChase.SendChaseInfo();

            return suceess;
        }

        private void OnCancelClick(GameObject sender)
        {
            cancelCallback?.Invoke();
            SetPanelVisible(false);
        }
    }

    public class StandalonePVE : CustomSecondPanel
    {
        public StandalonePVE(Transform trans) : base(trans)
        {
        }

        private InputField m_weapon;
        private InputField m_stage;
        private InputField m_sceneEvent;
        private GameObject confirm;
        private GameObject cancel;

        private int weapon = 1101;
        private int stageId = 101;
        private int sceneEventId = 2;

        private PropItemInfo weaponInfo;
        private StageInfo stageInfo;
        private int gender;
        private int proto;

        public override void InitComponent()
        {
            base.InitComponent();
            m_weapon = transform.GetComponent<InputField>("runeItemTypeId");
            m_stage = transform.GetComponent<InputField>("runeStar");
            m_sceneEvent = transform.GetComponent<InputField>("runeLevel");
            confirm = transform.GetComponent<Button>("sure").gameObject;
            cancel = transform.GetComponent<Button>("unSure").gameObject;

            m_weapon.text = weapon.ToString();
            m_stage.text = stageId.ToString();
            m_sceneEvent.text = sceneEventId.ToString();
        }

        public override void AddEvent()
        {
            base.AddEvent();
            EventTriggerListener.Get(confirm).onClick = OnConfirmClick;
            EventTriggerListener.Get(cancel).onClick = OnCancelClick;
        }

        private void OnConfirmClick(GameObject sender)
        {
            weapon = Util.Parse<int>(m_weapon.text);
            stageId = Util.Parse<int>(m_stage.text);
            sceneEventId = Util.Parse<int>(m_sceneEvent.text);

            PropItemInfo i = ConfigManager.Get<PropItemInfo>(weapon);
            StageInfo s = ConfigManager.Get<StageInfo>(stageId);
            SceneEventInfo si = ConfigManager.Get<SceneEventInfo>(sceneEventId);

            if (!i) Logger.LogError("weapon id error");
            if (!s) Logger.LogError("stage id error");
            if (!si) Logger.LogError("event id error");

            if (!i || !s || !si) return;
            weaponInfo = i;
            stageInfo = s;

            CreateRole();
            CreateEquips();
            CreateStage();
            Hide<Window_Gm>(true);
        }
        
        private void CreateRole()
        {
            ScRoleInfo p = PacketObject.Create<ScRoleInfo>();
            PRoleInfo role = PacketObject.Create<PRoleInfo>();
            PRoleAttr attr = PacketObject.Create<PRoleAttr>();
            PRoleCustomAttr ca = PacketObject.Create<PRoleCustomAttr>();
            p.role = role;
            p.attr = attr;
            p.customAttr = ca;


            role.roleName = "test_001";
            role.roleProto = (byte)weaponInfo.subType;
            proto = role.roleProto;
            role.gender = (byte)((role.roleProto == 1 || role.roleProto == 4) ? 1 : 0);
            gender = role.gender;
            role.level = 10;
            role.expr = 0;
            role.coin = 100000;
            role.avatar = "31205_0_31801";

            attr.power = new double[] { 19, 19, 19 };
            attr.energy = new double[] { 19, 19, 19 };
            attr.attack = new double[] { 1900, 1900, 1900 };
            attr.defense = new double[] { 190, 190, 190 };
            attr.moveSpeed = new double[] { 1, 1, 1 };
            attr.attackSpeed = new double[] { 1, 1, 1 };
            attr.tenacious = new double[] { 1, 1, 1 };
            attr.maxHp = new double[] { 10000, 10000, 10000 };
            attr.gunAttack = new double[] { 1, 1, 1 };
            attr.maxMp = new double[] { 1, 1, 1 };
            attr.knock = new double[] { 1, 1, 1 };
            attr.knockRate = new double[] { 1, 1, 1 };
            attr.artifice = new double[] { 1, 1, 1 };
            attr.angerSec = new double[] { 1, 1, 1 };
            attr.bone = new double[] { 1, 1, 1 };
            attr.brutal = new double[] { 1, 1, 1 };
            attr.elementAttack = new double[] { 1, 1, 1 };
            attr.elementDefenseWind = new double[] { 1, 1, 1 };
            attr.elementDefenseFire = new double[] { 1, 1, 1 };
            attr.elementDefenseWater = new double[] { 1, 1, 1 };
            attr.elementDefenseThunder = new double[] { 1, 1, 1 };
            attr.elementDefenseIce = new double[] { 1, 1, 1 };

            modulePlayer.GMCreatePlayer(p);
            moduleAttribute.GMCreatePlayer(p);
        }

        private void CreateEquips()
        {
            List<PropItemInfo> props = ConfigManager.GetAll<PropItemInfo>().FindAll(o => o.itemType == PropType.FashionCloth);

            ScRoleEquipedItems p = PacketObject.Create<ScRoleEquipedItems>();
            List<PItem> l = new List<PItem>();

            PropItemInfo hair = props.Find(o => o.subType == (int)FashionSubType.Hair && o.sex == gender && o.IsValidVocation(proto));
            PropItemInfo cloth = props.Find(o => o.subType == (int)FashionSubType.FourPieceSuit && o.sex == gender && o.IsValidVocation(proto));

            l.Add(moduleEquip.GetNewPItem((ulong)MonsterCreature.GetMonsterRoomIndex(),(ushort)weaponInfo.ID));
            l.Add(moduleEquip.GetNewPItem((ulong)MonsterCreature.GetMonsterRoomIndex(), (ushort)hair.ID));
            l.Add(moduleEquip.GetNewPItem((ulong)MonsterCreature.GetMonsterRoomIndex(), (ushort)cloth.ID));
            l.Add(moduleEquip.GetNewPItem((ulong)MonsterCreature.GetMonsterRoomIndex(), 2112));

            p.itemInfo = l.ToArray();
            moduleEquip.GMCreateEquip(p);
        }

        private void CreateStage()
        {
            modulePVE.OnPVEStart(stageInfo.ID,PVEReOpenPanel.ChasePanel);
        }

        private void OnCancelClick(GameObject sender)
        {
            SetPanelVisible(false);
        }

    }

    public class DatingInputParamPanel : CustomSecondPanel
    {
        private Text m_tipText;
        private InputField m_input;
        private GameObject confirmBtn;
        private GameObject cancelBtn;
        private Transform m_tfTipText;
        private Text m_tipText1;
        private InputField m_input1;

        public DatingInputParamPanel(Transform trans) : base(trans)
        {
        }

        public EnumDatingInputPanelType panelType { get; set; }

        public Action cancelCallback;

        public override void InitComponent()
        {
            base.InitComponent();
            m_tipText = transform.GetComponent<Text>("text/Text");
            m_input = transform.GetComponent<InputField>("input");
            confirmBtn = transform.Find("sure").gameObject;
            cancelBtn = transform.Find("unSure").gameObject;
            m_tfTipText = transform.GetComponent<RectTransform>("text1");
            m_tipText1 = transform.GetComponent<Text>("text1/Text");
            m_input1 = transform.GetComponent<InputField>("input1");
        }

        public override void AddEvent()
        {
            base.AddEvent();
            EventTriggerListener.Get(confirmBtn).onClick = OnConfirmClick;
            EventTriggerListener.Get(cancelBtn).onClick = OnCancelClick;
        }

        public override void SetPanelVisible(bool visible = true)
        {
            base.SetPanelVisible(visible);
            if (visible)
            {
                m_input.text = string.Empty;
                m_input1.text = string.Empty;
                switch (panelType)
                {
                    case EnumDatingInputPanelType.DatingEventPanel:
                        m_tipText.text = "打开测试的约会事件ID(Event表):";
                        m_tipText1.text = "输入约会场景id(1,2,3,4,5,6)";
                        m_tfTipText.SafeSetActive(true);
                        m_input1.SafeSetActive(true);
                        break;

                    case EnumDatingInputPanelType.DatingAnswerPanel:
                        m_tipText.text = "打开测试的约会问题ID(Answer表):";
                        m_tfTipText.SafeSetActive(false);
                        m_input1.SafeSetActive(false);
                        break;
                }
            }
        }

        private void OnConfirmClick(GameObject sender)
        {
            bool success = false;

            string[] strp = new string[] { m_input.text };
            if (m_input.text.Contains(",")) strp = m_input.text.Split(',');
            else if (m_input.text.Contains(";")) strp = m_input.text.Split(',');

            int id = 0;
            int eventItemId = 0;
            int npcId = 0;
            int sceneId = 0;

            bool bid = int.TryParse(strp[0], out id);
            bool bitemId = int.TryParse(strp.Length > 1 ? strp[1] : "0", out eventItemId);
            bool bnpcId = int.TryParse(strp.Length > 2 ? strp[2] : "3", out npcId);
            bool bsceneId = int.TryParse(m_input1.text, out sceneId);

            if (bid)
            {
                switch (panelType)
                {
                    case EnumDatingInputPanelType.DatingEventPanel:
                        success = GMDatingEvent(id, eventItemId, npcId, sceneId);
                        break;
                    case EnumDatingInputPanelType.DatingAnswerPanel:
                        success = GMDatingAnswer(id);
                        break;

                }
            }

            if (success)
            {
                SetPanelVisible(false);
                Hide<Window_Gm>();
            }
        }
        private bool GMDatingEvent(int id, int eventItemId, int npcId, int sceneId)
        {
            moduleNPCDating.GM_DatingCommand(DatingGmType.DoDatingEvent, id, eventItemId, npcId, (EnumNPCDatingSceneType)sceneId);
            return true;
        }
        private bool GMDatingAnswer(int answerId)
        {
            moduleNPCDating.GM_DatingCommand(DatingGmType.OpenAnswer, answerId);
            return true;
        }

        private void OnCancelClick(GameObject sender)
        {
            cancelCallback?.Invoke();
            SetPanelVisible(false);
        }
    }

    #endregion

    private RectTransform panel;
    private Button moneyBtn;
    private Button propBtn;
    private Button spcailProp;
    private Button runeBtn;
    private Button spcailRune;
    private Button refreshBtn;
    private Button labyrinthBtn;
    private Button labyrinthHp;
    private Button levelUpBtn;
    private Button addExpBtn;
    private Button cleanBag;
    private Button refreshAllShop;
    private Button refreshShop;
    private Button closeBtn;
    private Button labyrinthMail;
    private Button npcGift;
    private Button addWeaponDebris;
    private Button resetTeamPve;
    private Button resetActivePve;
    private Button resetSpriteTrain;
    private Button fullAnger;

    private RectTransform resetTeamPvePanel;
    private InputField teamPveType;
    private Button teamSure;
    private Button teamNoSure;

    private Button unionSent;
    private InputField unionSentientNum;
    private Button unionSentSure;
    
    #region this functions of Lee
    private Button stageBtn;
    private Button dialogBtn;
    private Button guideBtn;
    private Button addIntentyPropBtn;
    private Button pveLevelBtn;
    private Button finishGuideBtn;
    private Button pveLeaderBtn;
    private Button pveMemberBtn;
    private Button pveAutoBattleBtn;

    private StandalonePVE standPvePanel;
    private OneInputParamPanel oneInputPanel;
    #endregion

    private RectTransform addMoneyPanel;
    private InputField moneyType;
    private InputField moneyNumber;
    private Button moneySure;
    private Button moneyUnSure;

    private RectTransform addTypePanel;
    private InputField input;
    private Text title;
    private Button expSure;
    private Button expUnSure;
    private GmAddType addType;

    private RectTransform addRunePanel;
    private InputField runeId;
    private InputField runeLevel;
    private InputField runeStar;
    private InputField runeNumber;
    private Button runeSure;
    private Button runeUnSure;

    private RectTransform addPropPanel;
    private InputField propId;
    private InputField propNumber;
    private Button propSure;
    private Button propUnSure;

    private RectTransform addNpcExpPanel;
    private InputField npcId;
    private InputField npcExpNumber;
    private Button npcExpSure;
    private Button npcExpUnSure;

    private RectTransform addMailPanel;
    private InputField labLv;
    private Text promotion;
    private Button labSure;
    private Button labUnSure;

    private Button loginOut;


    //带参数GM命令
    private Button SendCmd;
    private RectTransform sendGMCmdPanel;
    private InputField cmdStr;
    private Button labCmdSure;
    private Button labCmdUnSure;

    //排位赛加积分
    private Button AddScore;
    private RectTransform addScorePanel;
    private InputField add_Text;
    private Button addSure;
    private Button addNoSure;

    //段位赛邮件
    private Button lolMailBtn;

    //解锁所有任务
    public Button unlockAllTaskBtn;
    public Button addPetMatrials;
    public Button addPetLevelMatrials;
    public Button sublimationMatrials;

    #region 约会
    //重置约会状态
    private Button m_btnResetDatingState;
    //增加Npc心情值
    private Button m_btnAddNpcMood;
    //结束约会
    private Button m_btnEndNpcEngagement;
    //解锁所有Npc约会
    private Button m_btnUnLockAllNpcDating;
    //测试约会DatingEvent
    private Button m_btnDoDatingEvent;
    //解锁Npc誓约状态
    private Button m_unLockNpcPledge;
    private Button m_bCheckDatingConfig;
    //增加npc好感度
    private Button m_btnAddNpcExp;
    private Button m_btnResetNpcExp;

    DatingInputParamPanel datingInputPanel;
    #endregion

    //工会签到
    private Button m_signTimeReset;
    private Button m_unionSignCard;
    private InputField m_cardInfo;

    //公会悬赏
    private Button m_rewardTimeReset;

    //活动赏金
    private Button m_coopReset;
    private Button m_coopSpecified;
    private InputField m_coopId;

    private Button m_resetAwakeStage, m_resetNpcStage;
    
    private bool m_fpsPanel, m_debugPanel;

    //复制角色
    private Button m_duplicateRole, m_duplicateOk, m_duplicateCancel, m_duplicateRandom;
    private InputField m_duplicateInput, m_duplicateInputAcc;
    private GameObject m_duplicateRoot;

    private Button m_requestAllRunes;

    private Button m_restWelfareBtn;
    private InputField m_weflareIndex;

    protected override void OnOpen()
    {
        isFullScreen = false;
        panel = GetComponent<RectTransform>("Panel");
        //添加金币
        addMoneyPanel = GetComponent<RectTransform>("addMoneyPanel");
        moneyType = GetComponent<InputField>("addMoneyPanel/moneyTypeId");
        moneyNumber = GetComponent<InputField>("addMoneyPanel/moneyNumber");
        moneySure = GetComponent<Button>("addMoneyPanel/sure");
        moneySure.onClick.AddListener(OnAddMoney);
        moneyUnSure = GetComponent<Button>("addMoneyPanel/unSure");
        moneyUnSure.onClick.AddListener(() =>
        {
            moneyType.text = null;
            moneyNumber.text = null;
            BackToMain(addMoneyPanel);
        });

        addTypePanel = GetComponent<RectTransform>("addOneLinePanel");
        input = GetComponent<InputField>("addOneLinePanel/oneLine");
        title = GetComponent<Text>("addOneLinePanel/oneLineTitle/Text");
        expSure = GetComponent<Button>("addOneLinePanel/sure");
        expSure.onClick.AddListener(OnAddExpOrLv);
        expUnSure = GetComponent<Button>("addOneLinePanel/unSure");
        expUnSure.onClick.AddListener(() => 
        {
            input.text = null;
            BackToMain(addTypePanel);
        });
        //添加指定符文
        addRunePanel = GetComponent<RectTransform>("addRunePanel");
        runeId = GetComponent<InputField>("addRunePanel/runeItemTypeId");
        runeLevel = GetComponent<InputField>("addRunePanel/runeLevel");
        runeStar = GetComponent<InputField>("addRunePanel/runeStar");
        runeNumber = GetComponent<InputField>("addRunePanel/runeNumber");
        runeSure = GetComponent<Button>("addRunePanel/sure");
        runeSure.onClick.AddListener(OnAddRune);
        runeUnSure = GetComponent<Button>("addRunePanel/unSure");
        runeUnSure.onClick.AddListener(() =>
        {
            runeId.text = null;
            runeStar.text = null;
            runeLevel.text = null;
            runeNumber.text = null;
            BackToMain(addRunePanel);
        });
        //添加指定道具
        addPropPanel = GetComponent<RectTransform>("addPropPanel");
        propId = GetComponent<InputField>("addPropPanel/propItemTypeId");
        propNumber = GetComponent<InputField>("addPropPanel/propNumber");
        propSure = GetComponent<Button>("addPropPanel/sure");
        propSure.onClick.AddListener(OnAddProp);

        propUnSure = GetComponent<Button>("addPropPanel/unSure");
        propUnSure.onClick.AddListener(() => 
        {
            propId.text = null;
            propNumber.text = null;
            BackToMain(addPropPanel);
        });

        //panel界面按钮操作
        moneyBtn = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/money");
        moneyBtn.onClick.AddListener(() =>
        {
            panel.gameObject.SetActive(false);
            addMoneyPanel.gameObject.SetActive(true);
        });
        
        spcailProp = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/propOneByOne");
        spcailProp.onClick.AddListener(() =>
        {
            panel.gameObject.SetActive(false);
            addPropPanel.gameObject.SetActive(true);
        });

        runeBtn = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/rune");
        runeBtn.onClick.AddListener(OnAddRandomRune);

        spcailRune = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/runeOneByOne");
        spcailRune.onClick.AddListener(() =>
        {
            panel.gameObject.SetActive(false);
            addRunePanel.gameObject.SetActive(true);
        });

        levelUpBtn = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/levelUp");
        levelUpBtn.onClick.AddListener(() => 
        {
            addType = GmAddType.CreatureLv;
            title.text = "请输入等级:";
            panel.gameObject.SetActive(false);
            addTypePanel.gameObject.SetActive(true);
        });

        addExpBtn = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/addExp");
        addExpBtn.onClick.AddListener(() => {
            addType = GmAddType.CreatureExp;
            title.text = "请输入经验值:";
            panel.gameObject.SetActive(false);
            addTypePanel.gameObject.SetActive(true);
        });

        labyrinthBtn = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/labyrinth");
        labyrinthBtn.onClick.AddListener(()=> { moduleGm.SendMazeNext(); });

        labyrinthHp = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/labyrinthHp");
        labyrinthHp.onClick.AddListener(() =>
        {
            addType = GmAddType.CreatureHp;
            title.text = "请输入血量:";
            panel.gameObject.SetActive(false);
            addTypePanel.gameObject.SetActive(true);
        });

        cleanBag = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/cleanBag");
        cleanBag.onClick.AddListener(()=> 
        {
            moduleGm.SendCleanBag();
            moduleEquip.SendRequestProp(RequestItemType.DressedClothes);
            moduleEquip.SendRequestProp(RequestItemType.InBag);
        });

        refreshAllShop = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/refreshAllShop");
        refreshAllShop.onClick.AddListener(() => { moduleGm.SendRefreshAllShop(); });

        refreshShop = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/refreshShop");
        refreshShop.onClick.AddListener(() =>
        {
            addType = GmAddType.ShopId;
            title.text = "请输入商店ID:";
            panel.gameObject.SetActive(false);
            addTypePanel.gameObject.SetActive(true);
        });

        labyrinthMail = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/labyrinthMail");
        labyrinthMail.onClick.AddListener(() => 
        {
            panel.gameObject.SetActive(false);
            addMailPanel.gameObject.SetActive(true);
        });
        addMailPanel = GetComponent<RectTransform>("addLabrinthMail");
        labLv = GetComponent<InputField>("addLabrinthMail/mailItemTypeId");
        promotion = GetComponent<Text>("addLabrinthMail/toggleGrounp/Image/Text");
        labSure = GetComponent<Button>("addLabrinthMail/sure");
        labSure.onClick.AddListener(OnLayAddMail);
        labUnSure = GetComponent<Button>("addLabrinthMail/unSure");
        labUnSure.onClick.AddListener(() =>
        {
            labLv.text = null;
            promotion.text = "1-晋级";
            BackToMain(addMailPanel);
        });

        refreshBtn = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/refresh");
        refreshBtn.onClick.AddListener(() => { moduleGm.SendCleanToday(); });

        closeBtn = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/close");
        closeBtn.onClick.AddListener(() => 
        {
            Hide(true);
            lolMailBtn.interactable = true;
        });

        #region init item from lee
        standPvePanel = new StandalonePVE(transform.Find("testPveScenePanel"));

        oneInputPanel = new OneInputParamPanel(transform.Find("oneInputParamPanel"));
        oneInputPanel.cancelCallback = ()=>{ panel.gameObject.SetActive(true); };

        dialogBtn = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/dialog");
        dialogBtn.onClick.AddListener(()=> {
            panel.gameObject.SetActive(false);
            oneInputPanel.panelType = EnumOnInputPanelType.DialogPanel;
            oneInputPanel.SetPanelVisible(true);
        });

        stageBtn = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/stage");
        stageBtn.onClick.AddListener(() =>
        {
            oneInputPanel.panelType = EnumOnInputPanelType.StagePanel;
            panel.gameObject.SetActive(false);
            oneInputPanel.SetPanelVisible(true);
        });

        guideBtn = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/guide");
        guideBtn.onClick.AddListener(() =>
        {
            oneInputPanel.panelType = EnumOnInputPanelType.GuidePanel;
            panel.gameObject.SetActive(false);
            oneInputPanel.SetPanelVisible(true);
        });

        unlockAllTaskBtn = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/unlockAllTask");
        if (unlockAllTaskBtn)
        {
            unlockAllTaskBtn.onClick.AddListener(() =>
            {
                oneInputPanel.panelType = EnumOnInputPanelType.ChasePanel;
                panel.gameObject.SetActive(false);
                oneInputPanel.SetPanelVisible(true);
            });
        }

        addPetMatrials = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/AddPetMatrials");
        addPetMatrials?.onClick.AddListener(() =>
        {
            if (Window.current is Window_Sprite)
            {
                var w = Window.current as Window_Sprite;
                modulePet.GMAddPetMatrials(w.SelectPetInfo);
            }
        });

        addPetLevelMatrials = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/AddPetLevelMatrials");
        addPetLevelMatrials?.onClick.AddListener(() =>
        {
            if (Window.current is Window_Sprite)
            {
                var w = Window.current as Window_Sprite;
                modulePet.GMAddPetLevelMatrials(w.SelectPetInfo);
            }
        });
        sublimationMatrials = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/sublimationMatrials");
        sublimationMatrials?.onClick.AddListener(() =>
        {
            moduleFurnace.GmGetSublimationMatrials();
        });

        addIntentyPropBtn = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/intenty");
        if (addIntentyPropBtn)
        {
            addIntentyPropBtn.onClick.AddListener(() =>
            {
                moduleEquip.GMAddIntentProps();
            });
        }

        pveLevelBtn = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/pveLevel");
        if (pveLevelBtn)
        {
            pveLevelBtn.onClick.AddListener(() =>
            {
                standPvePanel?.SetPanelVisible(true);
            });
        }

        finishGuideBtn = GetComponent<Button>("finishGuidePanel/ok");
        if (finishGuideBtn)
        {
            finishGuideBtn.onClick.AddListener(() =>
            {
                var ids = GetComponent<Text>("finishGuidePanel/guideID/Text");
                var time = GetComponent<Text>("finishGuidePanel/time/Text");
                var IDs = Util.ParseString<int>(ids.text, false, ',', ';', ' ');
                var t = Util.Parse<int>(time.text);

                ids.text = string.Empty;
                time.text = string.Empty;

                FinishGuide(t, IDs);
            });
        }

        pveLeaderBtn = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/pveLeader");
        if (pveLeaderBtn)
        {
            pveLeaderBtn.onClick.AddListener(() =>
            {
                modulePVE.TestSendPveStartAsLeader();
                Hide(true);
            });
        }

        pveMemberBtn = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/pveMember");
        if (pveMemberBtn)
        {
            pveMemberBtn.onClick.AddListener(() =>
            {
                modulePVE.TestSendPveStartAsMember();
                Hide(true);
            });
        }

        pveAutoBattleBtn = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/pveAutoBattle");
        if (pveAutoBattleBtn)
        {
            Util.SetText(pveAutoBattleBtn.transform.GetComponent<Text>("Text"), modulePVE.gmAutoBattle ? "关闭pve组队自动战斗" : "开启pve组队自动战斗");
            pveAutoBattleBtn.onClick.AddListener(() =>
            {
                if (!Level.current)
                {
                    Logger.LogError("level.current is null");
                    return;
                }

                if(Level.current is Level_PVE)
                {
                    Logger.LogError("请先退出pve关卡再使用该功能");
                    return;
                }

                modulePVE.gmAutoBattle = !modulePVE.gmAutoBattle;
                Util.SetText(pveAutoBattleBtn.transform.GetComponent<Text>("Text"), modulePVE.gmAutoBattle ? "关闭pve组队自动战斗":"开启pve组队自动战斗");
            });
        }

        #endregion

        //退出登录
        loginOut = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/loginout");
        loginOut.onClick.AddListener(() => moduleLogin.LogOut(true));


        //发送带参数GM命令
        SendCmd = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/sendGMCmd");
        SendCmd.onClick.AddListener(() =>
        {
            sendGMCmdPanel.gameObject.SetActive(true);
            panel.gameObject.SetActive(false);
        });
        sendGMCmdPanel = GetComponent<RectTransform>("sendGMCmdPanel");
        cmdStr = GetComponent<InputField>("sendGMCmdPanel/input");
        labCmdSure = GetComponent<Button>("sendGMCmdPanel/sure");
        labCmdSure.onClick.AddListener(OnSendGmCmd);
        labCmdUnSure = GetComponent<Button>("sendGMCmdPanel/unSure");
        labCmdUnSure.onClick.AddListener(() =>
        {
            BackToMain(sendGMCmdPanel);
            cmdStr.text = string.Empty;
        });

        //排位赛加积分
        AddScore = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/addScore");
        AddScore.onClick.AddListener(()=> 
        {
            addScorePanel.gameObject.SetActive(true);
            panel.gameObject.SetActive(false);
        });
        addScorePanel = GetComponent<RectTransform>("addScorePanel");
        add_Text = GetComponent<InputField>("addScorePanel/input");
        addSure = GetComponent<Button>("addScorePanel/sure");
        addSure.onClick.AddListener(OnAddScore);
        addNoSure = GetComponent<Button>("addScorePanel/unSure");
        addNoSure.onClick.AddListener(() => 
        {
            BackToMain(addScorePanel);
            add_Text.text = string.Empty;
        });

        //段位赛邮件
        lolMailBtn = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/lolMail");
        lolMailBtn.onClick.RemoveAllListeners();
        lolMailBtn.onClick.AddListener(()=> 
        {
            moduleGm.SendLolMail();
            lolMailBtn.interactable = false;
        });

        GetComponent<Button>("spawnCreaturePanel/ok").onClick.AddListener(SpawnCreature);
        GetComponent<Button>("lockClassPanel/ok").onClick.AddListener(LockClass);
        GetComponent<Button>("Panel/ScrollRect/Viewport/Content/sos").onClick.AddListener(CreateAssist);
        npcGift = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/npcGift");
        addWeaponDebris = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/addWeapondebris");
        resetTeamPve = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/teamPveTimes");
        resetTeamPvePanel = GetComponent<RectTransform>("resetTeamPvePanel");
        teamPveType = GetComponent<InputField>("resetTeamPvePanel/teamInput");
        teamSure = GetComponent<Button>("resetTeamPvePanel/sure");
        teamNoSure = GetComponent<Button>("resetTeamPvePanel/unSure");
        resetTeamPve.onClick.AddListener(() =>
        {
            resetTeamPvePanel.SafeSetActive(true);
            panel.SafeSetActive(false);
        });
        teamSure.onClick.AddListener(() => moduleGm.SendTeamPveTimes(Util.Parse<int>(teamPveType.text)));
        teamNoSure.onClick.AddListener(() =>
        {
            teamPveType.text = string.Empty;
            BackToMain(resetTeamPvePanel);
        });

        resetActivePve = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/activePveTimes");
        resetSpriteTrain = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/resetSpritePve");
        fullAnger = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/fullAnger");

        npcGift.onClick.AddListener(OnAddNpcGifts);
        addWeaponDebris.onClick.AddListener(OnAddWeaponDebris);
        resetActivePve.onClick.AddListener(() => moduleGm.SendActivePveTimes());
        resetSpriteTrain.onClick.AddListener(() => moduleGm.SendSpriteTrain());
        fullAnger.onClick.AddListener(() => moduleGm.SendFullAnger());

        unionSent = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/addSentiment");
        unionSentSure = GetComponent<Button>("addSentientPlane/sure");
        unionSentientNum = GetComponent<InputField>("addSentientPlane/input");
        unionSentSure.onClick.AddListener(AddSentiment);
        unionSent.onClick.AddListener(() => { unionSentientNum.text = string.Empty; });
        InitializeSearchPanel();


        InitilizeDuplicateRolePanel();
        m_duplicateRole = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/duplicateRole");
        m_duplicateRole.onClick.AddListener(() => {
            m_duplicateInput.text = "";
            m_duplicateInputAcc.text = "";
            m_duplicateRoot.SafeSetActive(true);
        });

        #region init dating item
        m_btnResetDatingState = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/resetDatingState");
        m_btnResetDatingState.onClick.AddListener(ResetDatingState);

        //增加Npc约会心情值
        m_btnAddNpcMood = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/addNpcMood");
        m_btnAddNpcMood.onClick.AddListener(() =>
        {
            addType = GmAddType.NpcEngagementMood;
            title.text = "请输入心情值:";
            panel.gameObject.SetActive(false);
            addTypePanel.gameObject.SetActive(true);
        });

        m_btnEndNpcEngagement = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/endNpcEngagement");
        m_btnEndNpcEngagement.onClick.AddListener(EndNpcEngagement);

        m_btnUnLockAllNpcDating = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/unLockAllNpcDating");
        m_btnUnLockAllNpcDating.onClick.AddListener(UnLockAllNpcDating);

        datingInputPanel = new DatingInputParamPanel(transform.Find("datingInputParamPanel"));
        datingInputPanel.cancelCallback = () => { panel.gameObject.SetActive(true); };

        m_btnDoDatingEvent = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/doDatingEvent");
        m_btnDoDatingEvent.onClick.AddListener(() => {
            panel.gameObject.SetActive(false);
            datingInputPanel.panelType = EnumDatingInputPanelType.DatingEventPanel;
            datingInputPanel.SetPanelVisible(true);
        });

        m_unLockNpcPledge = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/unLockNpcPledge");
        m_unLockNpcPledge.onClick.AddListener(() => {
            addType = GmAddType.NpcPledge;
            string strIdRange = ((int)NpcTypeID.None + 1) + " - " + ((int)NpcTypeID.Max - 2);
            title.text = $"输入NpcId({strIdRange}):";
            panel.gameObject.SetActive(false);
            addTypePanel.gameObject.SetActive(true);
        });
        m_bCheckDatingConfig = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/checkDatingConfig");
        m_bCheckDatingConfig.onClick.AddListener(Module_NPCDating.GMCheckConfig);
        #endregion

        m_requestAllRunes = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/allRunes");
        m_requestAllRunes.onClick.AddListener(() => RequestAllRunes());

        //添加指定道具
        addNpcExpPanel = GetComponent<RectTransform>("addNpcExpPanel");
        npcId = GetComponent<InputField>("addNpcExpPanel/propItemTypeId");
        npcExpNumber = GetComponent<InputField>("addNpcExpPanel/propNumber");
        npcExpSure = GetComponent<Button>("addNpcExpPanel/sure");
        npcExpSure.onClick.AddListener(OnAddNpcExp);

        npcExpUnSure = GetComponent<Button>("addNpcExpPanel/unSure");
        npcExpUnSure.onClick.AddListener(() =>
        {
            npcId.text = null;
            npcExpNumber.text = null;
            BackToMain(addNpcExpPanel);
        });
        m_btnAddNpcExp = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/addNpcExp");
        m_btnAddNpcExp.onClick.AddListener(() =>
        {
            addNpcExpPanel.gameObject.SetActive(true);
        }); 

        m_btnResetNpcExp = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/cleanNpcExp");
        m_btnResetNpcExp.onClick.AddListener(() =>
        {
            moduleGm.SendResetNpcExp();
        }); 

        m_signTimeReset = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/refreshUnionSign");
        m_signTimeReset.onClick.AddListener(RestUnionSignTimes);
        m_unionSignCard= GetComponent<Button>("unionCardPlane/sure");
        m_cardInfo= GetComponent<InputField>("unionCardPlane/input");
        m_unionSignCard.onClick.AddListener(RestCardInfo);

        m_rewardTimeReset = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/unionRewardTime");
        m_rewardTimeReset.onClick.AddListener(delegate
        {
            moduleGm.SendRestUnionReward();
        });

        m_coopReset = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/coopRefresh");
        m_coopSpecified = GetComponent<Button>("activeCoopPlane/sure");
        m_coopId = GetComponent<InputField>("activeCoopPlane/input");
        m_coopReset.onClick.AddListener(delegate
        {
            moduleGm.SendRestActiveCoop();
        });
        m_coopSpecified.onClick.AddListener(SpecifiedCoop);

        m_resetAwakeStage = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/resetAwakeStage");
        m_resetAwakeStage.onClick.AddListener(() => moduleGm.SendResetAwakeStage());

        m_resetNpcStage = GetComponent<Button>("Panel/ScrollRect/Viewport/Content/resetNpcStage");
        m_resetNpcStage.onClick.AddListener(() => moduleGm.SendResetNpcStage());

        m_restWelfareBtn = GetComponent<Button>("restWelfarePlane/sure");
        m_weflareIndex = GetComponent<InputField>("restWelfarePlane/input");
        m_restWelfareBtn?.onClick.AddListener(RestWelfareSate);
    }


    private static char[] SPECIAL_CHARACTOR = new char[] { '·' };
    public const string MATCH_STRING = @"^[a-zA-Z0-9·\u4e00-\u9fa5]+$";
    public const string MATCH_NUM = @"^[0-9]+$";

    private bool IsRoleNameValid(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 14));
            return false;
        }

        if (text.Length < GeneralConfigInfo.MIN_NAME_LEN || text.Length > GeneralConfigInfo.MAX_NAME_LEN)
        {
            moduleGlobal.ShowMessage(Util.Format(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 46), GeneralConfigInfo.MIN_NAME_LEN, GeneralConfigInfo.MAX_NAME_LEN));
            return false;
        }

        var onlyNum = Regex.IsMatch(text, MATCH_NUM);
        if (onlyNum)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.NameUIText, 6));
            name = string.Empty;
            return false;
        }

        var match = Regex.IsMatch(text, MATCH_STRING);
        if (!match)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.LoginUIText, 15));
            text = string.Empty;
            return false;
        }

        if (Util.ContainsSensitiveWord(text))
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.NameUIText, 5));
            return false;
        }
        return true;
    }
    private void InitilizeDuplicateRolePanel()
    {

        m_duplicateRoot = GetComponent<RectTransform>("duplicateRolePanel").gameObject;
        m_duplicateOk = GetComponent<Button>("duplicateRolePanel/sure");
        m_duplicateCancel = GetComponent<Button>("duplicateRolePanel/unSure");
        m_duplicateRandom = GetComponent<Button>("duplicateRolePanel/random");
        m_duplicateInput = GetComponent<InputField>("duplicateRolePanel/input");
        m_duplicateInputAcc = GetComponent<InputField>("duplicateRolePanel/input_acc");
        m_duplicateRoot.SafeSetActive(false);

        m_duplicateOk.onClick.AddListener(() => {

            if (IsRoleNameValid(m_duplicateInput.text) && IsRoleNameValid(m_duplicateInputAcc.text))
            {
                moduleGm.DuplicateRole(m_duplicateInput.text, m_duplicateInputAcc.text);
                m_duplicateRoot.SafeSetActive(false);
            }
        });

        m_duplicateCancel.onClick.AddListener(() => {
            m_duplicateRoot.SafeSetActive(false);
        });

        m_duplicateRandom.onClick.AddListener(() => {
            m_duplicateInput.text = NameConfigInfo.GetRandomName(1);
        });
    }

    private bool IsValidChar(string s)
    {
        foreach (var c in s)
        {

            if (SPECIAL_CHARACTOR.Contains(c)) continue;

            if (!char.IsLetter(c) && !char.IsNumber(c)) return false;
        }

        return true;
    }


    private void OnAddWeaponDebris()
    {
        var prop = ConfigManager.GetAll<PropItemInfo>();
        if (prop == null || prop.Count < 1) return;
        for (int i = 0; i < prop.Count; i++)
        {
            if (prop[i].itemType == PropType.Debris)
            {
                moduleGm.SendAddProp((ushort)prop[i].ID, 100);
            }
        }
        moduleEquip.SendRequestProp(RequestItemType.InBag);
    }

    private void OnAddNpcGifts()
    {
        var prop = ConfigManager.GetAll<PropItemInfo>();
        if (prop == null || prop.Count < 1) return;
        for (int i = 0; i < prop.Count; i++)
        {
            if (prop[i].itemType == PropType.Sundries && prop[i].subType == (int)SundriesSubType.NpcgoodFeeling)
            {
                moduleGm.SendAddProp((ushort)prop[i].ID, 1000);
            }
        }
        moduleEquip.SendRequestProp(RequestItemType.InBag);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        panel.gameObject.SetActive(true);

        m_fpsPanel = Root.showFPS;
        m_debugPanel = Root.showDebugPanel;

        Root.showFPS = false;
        Root.showDebugPanel = false;
    }

    protected override void OnHide(bool forward)
    {
        Root.showFPS = m_fpsPanel;
        Root.showDebugPanel = m_debugPanel;
    }

    private void SpawnCreature()
    {
        var id     = Util.Parse<int>(GetComponent<InputField>("spawnCreaturePanel/creatureID").text);
        var offset = Util.Parse<float>(GetComponent<InputField>("spawnCreaturePanel/creatureOffset").text);
        var group  = Util.Parse<int>(GetComponent<Text>("spawnCreaturePanel/creatureGroup/Text").text);
        var level  = Util.Parse<int>(GetComponent<Text>("spawnCreaturePanel/creatureLevel/Text").text);
        var ai     = GetComponent<Toggle>("spawnCreaturePanel/ai").isOn;
        var invert = GetComponent<Toggle>("spawnCreaturePanel/invert").isOn;

        SpawnCreature(id, offset, group, level < 1 ? 1 : level, ai, invert);
    }

    private void SpawnCreature(int id, float offset = 0, int group = 0, int level = 1, bool ai = true, bool inveret = false)
    {
        if (id == 0)
        {
            moduleGlobal.ShowMessage("ID can not be zero.");
            return;
        }

        var m = ConfigManager.Get<MonsterInfo>(id);
        if (!m)
        {
            moduleGlobal.ShowMessage("Can not find monster <color=#FF0000FF>[" + id + "]</color>.");
            return;
        }

        var assets = Module_Battle.BuildWeaponPreloadAssets(0, 1, m.montserStateMachineId);
        assets.AddRange(m.models);

        Level.PrepareAssets(assets, r =>
        {
            if (!r) return;
            var mon = MonsterCreature.CreateMonster(id, group, level, new Vector3_(offset, 0, 0), new Vector3(0, inveret ? -90 : 90, 0), Module_PVE.instance?.currrentStage);
            if (mon)
            {
                var buffs = mon.monsterInfo?.initBuffs;
                if (buffs != null) foreach (var buff in buffs) Buff.Create(buff, mon, mon);

                mon.direction = inveret ? CreatureDirection.BACK : CreatureDirection.FORWARD;
                if (ai)
                {
                    moduleAI.SafeStartAI();
                    moduleAI.AddCreatureToCampDic(mon);
                    moduleAI.ChangeLockEnermy(mon, true);
                    moduleAI.AddMonsterAI(mon);
                }
            }
        });
    }

    private void LockClass()
    {
        var class_ = Util.Parse<int>(GetComponent<InputField>("lockClassPanel/classID").text);
        var weapon = Util.Parse<int>(GetComponent<InputField>("lockClassPanel/weaponItem").text);

        if (class_ > 0)
        {
            var c = ConfigManager.Get<CreatureInfo>(class_);
            if (!c)
            {
                moduleGlobal.ShowMessage("Invalid class ID <color=#FF0000FF>[" + class_ + "]</color>.");
                return;
            }
        }

        if (weapon > 0)
        {
            var w = WeaponInfo.GetWeapon(class_, weapon);
            if (w.isEmpty) w = WeaponInfo.GetWeapon(class_, 0);

            if (w.isEmpty)
            {
                moduleGlobal.ShowMessage("Invalid weapon item ID <color=#FF0000FF>[" + weapon + "]</color>.");
                return;
            }
        }

        DispatchEvent("OnGmLockClass", Event_.Pop(class_, weapon));
    }

    private void CreateAssist()
    {
        var pve = Level.current as Level_PVE;
        pve?.CreateAssistMember();
    }

    private void OnLayAddMail()
    {
        moduleGm.SendLabyrinthMail(labLv.text, promotion.text.Substring(0, 1));
    }

    private void OnAddProp()
    {
        if (!string.IsNullOrEmpty(propId.text))
        {
            var count = Util.Parse<int>(propNumber.text);
            var iis = Util.ParseString<int>(propId.text, false, ',', ';', ' ');
            foreach (var ii in iis)
            {
                var info = ConfigManager.Get<PropItemInfo>(ii);
                if (!info)
                {
                    Logger.LogError("Invalid item id {0}", ii);
                    continue;
                }
                moduleGm.SendAddProp((ushort)info.ID, info.itemType == PropType.FashionCloth || info.itemType == PropType.Weapon || count < 1 ? 1 : count);
            }

            moduleEquip.SendRequestProp(RequestItemType.DressedClothes);
            moduleEquip.SendRequestProp(RequestItemType.InBag);
        }
    }

    private void OnAddNpcExp()
    {
        if (!string.IsNullOrEmpty(npcId.text))
        {
            var count = Util.Parse<int>(npcExpNumber.text);
            var npc_id = Util.Parse<int>(npcId.text);
            moduleGm.SendAddNpcExp(npc_id, count);
            BackToMain(addNpcExpPanel);
        }
    }

    private void OnAddExpOrLv()
    {
        if (string.IsNullOrEmpty(input.text)) return;
        switch (addType)
        {
            case GmAddType.CreatureExp:
                moduleGm.SendAddExp(Util.Parse<uint>(input.text));
                break;
            case GmAddType.CreatureLv:
                moduleGm.SendChangeLv(Util.Parse<int>(input.text));
                break;
            case GmAddType.CreatureHp:
                moduleGm.SendSetMazeHp(Util.Parse<uint>(input.text));
                break;
            case GmAddType.ShopId:
                moduleGm.SendRefreshShop(Util.Parse<int>(input.text));
                break;
            case GmAddType.NpcEngagementMood:
                moduleGm.SendAddNpcMood(Util.Parse<int>(input.text));
                break;
            case GmAddType.NpcPledge:
                moduleGm.UnLockNpcPledge(Util.Parse<int>(input.text));
                break;
            default:
                break;
        }
    }

    private void OnAddRune()
    {
        if (string.IsNullOrEmpty(runeStar.text))
            runeStar.text = "1";
        if (string.IsNullOrEmpty(runeLevel.text))
            runeLevel.text = "0";
        if (string.IsNullOrEmpty(runeNumber.text))
            runeNumber.text = "1";

        if (!string.IsNullOrEmpty(runeId.text))
        {
            moduleGm.SendAddRune(Util.Parse<ushort>(runeId.text), Util.Parse<int>(runeStar.text), Util.Parse<int>(runeLevel.text), Util.Parse<int>(runeNumber.text));
            moduleRune.swallowedIdList.Clear();
            moduleRune.evolveList.Clear();
            moduleEquip.SendRequestProp(RequestItemType.InBag);
        }
    }

    private void OnAddRandomRune()
    {
        moduleRune.swallowedIdList.Clear();
        moduleRune.evolveList.Clear();
        moduleRune.TestRun();
    }

    private void OnAddMoney()
    {
        if (!string.IsNullOrEmpty(moneyType.text) && !string.IsNullOrEmpty(moneyNumber.text))
            moduleGm.SendAddMoney(Util.Parse<int>(moneyType.text), Util.Parse<int>(moneyNumber.text));
    }

    private void BackToMain(Transform now)
    {
        panel.gameObject.SetActive(true);
        now.gameObject.SetActive(false);
    }

    private void OnAddScore()
    {
        string str = add_Text.text;
        if (!string.IsNullOrEmpty(str))
            moduleGm.SendAddScore(str);
    }

    private void OnSendGmCmd()
    {
        string str = cmdStr.text;
        if (!string.IsNullOrEmpty(str))
            moduleGm.SendGMCmdStr(str);
    }

    private void AddSentiment()
    {
        string str = unionSentientNum.text;
        if (!string.IsNullOrEmpty(str))
            moduleGm.SendSentiment(str);
    }

    private void ResetDatingState()
    {
        moduleGm.SendResetDatingState();
        moduleNPCDating.GM_DatingCommand(DatingGmType.ResetDating);
    }

    private void EndNpcEngagement()
    {
        moduleGm.SendEndNpcEngagement();
        moduleNPCDating.GM_DatingCommand(DatingGmType.DatingOver);
        //约会结束关闭gm面板，否则会影响约会结束剧情的点击流程
        Hide(true);
        lolMailBtn.interactable = true;
    }

    private void UnLockAllNpcDating()
    {
        moduleGm.SendUnLockAllNpcDating();
    }

    private void RestUnionSignTimes()
    {
        moduleGm.SendRestSignTime();
    }
    private void RestCardInfo()
    {
        if (string.IsNullOrEmpty(m_cardInfo.text)) moduleGlobal.ShowMessage("empty");
        else moduleGm.SendRestUnionCard(m_cardInfo.text);
    }

    private void SpecifiedCoop()
    {
        if (string.IsNullOrEmpty(m_coopId.text)) moduleGlobal.ShowMessage("empty");
        else moduleGm.SendSpecifiedActiveCoop(m_coopId.text);
    }

    private void RequestAllRunes()
    {
        var runes = ConfigManager.GetAll<PropItemInfo>();
        runes.RemoveAll(r => r.itemType != PropType.Rune);

        foreach (var rune in runes)
            moduleGm.SendAddProp((ushort)rune.ID, 10);
    }

    private void RestWelfareSate()
    {
        if (string.IsNullOrEmpty(m_weflareIndex.text)) return;
        var index = Util.Parse<int>(m_weflareIndex.text);
        if (index < 1 || index > moduleWelfare.puzzleList.Count) return;

        var wId = moduleWelfare.puzzleList[index - 1]?.id;
        moduleGm.RestWelfareIndo(wId.ToString());
    }

    private void FinishGuide(int time, params int[] IDs)
    {
        var guides = ConfigManager.GetAll<GuideInfo>();

        foreach (var item in guides)
        {
            if (IDs == null || IDs.Length < 1 || IDs.FindIndex(i => i == item.ID) > -1)
            {
                var p = PacketObject.Create<CsGuideFinish>();

                p.finishGuide = item.ID;
                p.consumeTime = time;

                session.Send(p);
            }
        }
    }

    public enum GmAddType
    {
        CreatureExp,

        CreatureLv,

        CreatureHp,

        ShopId,

        NpcEngagementMood,

        NpcPledge,
    }

    #region Search panel

    private InputField m_searchInput;
    private UISingleContainer m_tabs;
    private ScrollView m_viewItems, m_viewCreatures, m_viewMonsters;
    private DataSource<PropItemInfo> m_dataItems;
    private DataSource<CreatureInfo> m_dataCreatures;
    private DataSource<MonsterInfo> m_dataMonsters;

    private List<PropItemInfo> m_cachedItems;
    private List<CreatureInfo> m_cachedCreatures;
    private List<MonsterInfo> m_cachedMonsters;

    private PropItemInfo m_selectedItem;
    private RectTransform m_selectedItemNode;

    private MultiDropdown m_itemType, m_itemSubType, m_itemGender, m_itemQuality, m_itemClass;
    private Toggle m_itemCompose, m_itemDecompose, m_itemUpgrade, m_itemEnchant, m_itemSet;
    private InputField m_itemIcon, m_itemMesh, m_itemField, m_itemDrop;

    private bool m_lockFilterEvent = false;
    private int m_searched = 0;
    
    private Color[] m_qualityColors = new Color[] { Util.BuildColor("#ccccccff"), Util.BuildColor("#ffffffff"), Util.BuildColor("#1eff01ff"), Util.BuildColor("#0070dcff"), Util.BuildColor("#a335eeff"), Util.BuildColor("#ff7f00ff"), Util.BuildColor("#e5cb81ff") };
    private string[] m_itemTypeNames = new string[] { "Unknow0", "武器", "Unknow2", "时装", "灵珀", "货币", "可使用", "杂物", "迷宫", "头像框", "装备强化", "装备进阶", "碎片", "元素", "无主之地积分", "宠物食物", "宠物", "觉醒消耗", "技能点", "图纸" };
    private string[][] m_itemSubTypeNames = new string[][]
    {
        new string[] { "-" },
        new string[] { "-", "长剑", "武士刀", "长枪", "大斧", "拳套", "远程" },
        new string[] { "-" },
        new string[] { "-", "上装", "下装", "手套", "鞋子", "发型", "饰品", "两件套", "四件套", "新品", "限时", "外观" },
        new string[] { "-", "一", "二", "三", "四", "五", "六" },
        new string[] { "-", "金币", "钻石", "徽记", "冒险币", "召唤石", "心魂", "祈愿币","友情点" },
        new string[] { "-", "通缉令", "经验卡", "钱袋", "箱子", "月卡", "季卡" },
        new string[] { "-", "好感度道具", "子弹", "助战符","装备材料","票券","复活道具" },
        new string[] { "-", "陷阱", "攻击", "防护" },
        new string[] { "-" },
        new string[] { "-" },
        new string[] { "-" },
        new string[] { "-", "主武器", "副武器", "防具", "-", "宠物", "-", "-", "-", "-", "符文" },
        new string[] { "-" },
        new string[] { "-" },
        new string[] { "-" },
        new string[] { "-" },
        new string[] { "-" },
        new string[] { "-" },
        new string[] { "-" },
    };

    private string GetItemTypeName(PropType type)
    {
        return GetItemTypeName((int)type);
    }

    private string GetItemTypeName(int type)
    {
        return m_itemTypeNames[type < 0 || type >= m_itemTypeNames.Length ? 0 : type];
    }

    private string GetItemSubTypeNames(PropType type, int subType)
    {
        return GetItemSubTypeNames((int)type, subType);
    }

    private string GetItemSubTypeNames(int type, int subType)
    {
        if (type < 0 || type >= m_itemSubTypeNames.Length) return "Unknow";
        var ns = m_itemSubTypeNames[type];

        var n = subType < 0 ? "-" : subType >= ns.Length ? type == (int)PropType.Weapon ? ns[ns.Length - 1] : "-" : ns[subType];
        return n == "-" && subType > 0 ? "未使用" + subType : n;
    }

    private string GetItemSubTypeTypeName(PropType type, int subType)
    {
        var n = GetItemSubTypeNames(type, subType);
        if (string.IsNullOrEmpty(n) || n.StartsWith("未使用")) return n.Replace("未使用", "Unknow");

        switch (type)
        {
            case PropType.Weapon:         return ((WeaponSubType)subType).ToString();
            case PropType.FashionCloth:   return ((FashionSubType)subType).ToString();
            case PropType.Rune:           return ((RuneType)subType).ToString();
            case PropType.Currency:       return ((CurrencySubType)subType).ToString();
            case PropType.UseableProp:    return ((UseablePropSubType)subType).ToString();
            case PropType.Sundries:       return ((SundriesSubType)subType).ToString();
            case PropType.LabyrinthProp:  return ((LabyrinthPropSubType)subType).ToString();
            case PropType.HeadAvatar:     return "-";
            case PropType.IntentyProp:    return "-";
            case PropType.EvolveProp:     return "-";
            case PropType.Debris:         return ((DebrisSubType)subType).ToString();
            case PropType.ElementType:    return "-";
            case PropType.BordGrade:      return "-";
            case PropType.PetFood:        return "-";
            case PropType.Pet:            return "-";
            case PropType.AwakeCurrency:  return "-";
            case PropType.SkillPoint:     return "-";
            case PropType.Drawing:        return "-";
            case PropType.None:
            case PropType.Placeholder:
            default:                      return "Unknow";
        }
    }

    private void InitializeSearchPanel()
    {
        GetComponent<Button>("searchPanel/content/input/search/Image").onClick.AddListener(OnSearch);
        GetComponent<InputField>("searchPanel/content/input").onEndEdit.AddListener(s => OnSearch());

        InitializeFilters();
    }

    private void InitializeFilters()
    {
        m_searched        = 0;
        m_lockFilterEvent = false;

        m_itemType      = GetComponent<MultiDropdown>("searchPanel/content/filters/items/type");    
        m_itemSubType   = GetComponent<MultiDropdown>("searchPanel/content/filters/items/subType"); 
        m_itemGender    = GetComponent<MultiDropdown>("searchPanel/content/filters/items/gender");  
        m_itemQuality   = GetComponent<MultiDropdown>("searchPanel/content/filters/items/quality");
        m_itemClass     = GetComponent<MultiDropdown>("searchPanel/content/filters/items/class");

        m_itemCompose   = GetComponent<Toggle>("searchPanel/content/filters/items/compose");
        m_itemDecompose = GetComponent<Toggle>("searchPanel/content/filters/items/decompose");
        m_itemUpgrade   = GetComponent<Toggle>("searchPanel/content/filters/items/upgrade");
        m_itemEnchant   = GetComponent<Toggle>("searchPanel/content/filters/items/enchant");
        m_itemSet       = GetComponent<Toggle>("searchPanel/content/filters/items/set");

        m_itemIcon      = GetComponent<InputField>("searchPanel/content/filters/items/icon");
        m_itemMesh      = GetComponent<InputField>("searchPanel/content/filters/items/mesh");
        m_itemField     = GetComponent<InputField>("searchPanel/content/filters/items/field");
        m_itemDrop      = GetComponent<InputField>("searchPanel/content/filters/items/drop");

        GetComponent<Button>("searchPanel/content/filters/items/reset").onClick.AddListener(OnResetItemFilters);

        m_itemQuality.options = new List<Dropdown.OptionData>()
        {
            new Dropdown.OptionData() { text = "所有", color = Util.BuildColor("#FF8E00FF") },
            new Dropdown.OptionData() { text = "垃圾", color = m_qualityColors[0] },
            new Dropdown.OptionData() { text = "普通", color = m_qualityColors[1] },
            new Dropdown.OptionData() { text = "优秀", color = m_qualityColors[2] },
            new Dropdown.OptionData() { text = "精良", color = m_qualityColors[3] },
            new Dropdown.OptionData() { text = "史诗", color = m_qualityColors[4] },
            new Dropdown.OptionData() { text = "传说", color = m_qualityColors[5] },
            new Dropdown.OptionData() { text = "神器", color = m_qualityColors[6] },
        };

        var classes = new List<Dropdown.OptionData>();
        for (var i = 0; i < (int)CreatureVocationType.Count; ++i)
            classes.Add(new Dropdown.OptionData() { text = i == 0 ? "所有" : "职业" + i });
        m_itemClass.options = classes;

        m_itemType.value = m_itemSubType.value = m_itemGender.value = m_itemQuality.value = m_itemClass.value = 1;
        m_itemCompose.isOn = m_itemDecompose.isOn = m_itemUpgrade.isOn = m_itemEnchant.isOn = m_itemSet.isOn = false;
        m_itemIcon.text = m_itemMesh.text = m_itemField.text = m_itemDrop.text = "";

        m_itemType.onValueChanged.AddListener(OnFilterItemType);
        m_itemSubType.onValueChanged.AddListener(FilterItems);
        m_itemGender.onValueChanged.AddListener(FilterItems);
        m_itemQuality.onValueChanged.AddListener(FilterItems);
        m_itemClass.onValueChanged.AddListener(FilterItems);

        m_itemCompose.onValueChanged.AddListener(FilterItems);
        m_itemDecompose.onValueChanged.AddListener(FilterItems);
        m_itemUpgrade.onValueChanged.AddListener(FilterItems);
        m_itemEnchant.onValueChanged.AddListener(FilterItems);
        m_itemSet.onValueChanged.AddListener(FilterItems);

        m_itemIcon.onEndEdit.AddListener(FilterItems);
        m_itemMesh.onEndEdit.AddListener(FilterItems);
        m_itemField.onEndEdit.AddListener(FilterItems);
        m_itemDrop.onEndEdit.AddListener(FilterItems);

        OnFilterItemType(m_itemType.value, false);
    }

    private void OnSearch()
    {
        if (!m_searchInput) m_searchInput = GetComponent<InputField>("searchPanel/content/input");
        if (!m_tabs) m_tabs = GetComponent<UISingleContainer>("searchPanel/content/tabs");

        var text = m_searchInput.text;
        //if (string.IsNullOrEmpty(text))
        //{
        //    moduleGlobal.ShowGlobalNotice("Please input a valid id, name or description info.", 1.0f, 0.25f);
        //    return;
        //}
        
        var current = m_tabs.currentVisible;
        switch (current)
        {
            case "items":      SearchItems(text); break;
            case "creatures":
            case "monsters":
            default: break;
        }
    }

    private void SearchItems(string s)
    {
        m_searched = m_searched.BitMask(0, true);

        s = s.ToLower();
        if (m_cachedItems != null) m_cachedItems.Clear();
        else m_cachedItems = new List<PropItemInfo>();

        ConfigManager.Foreach<PropItemInfo>(i => { if ((string.IsNullOrEmpty(s) || i.itemName.ToLower().Contains(s)) && FilterItem(i)) m_cachedItems.Add(i); return true; });

        var id = Util.Parse<int>(s);
        if (id != 0)
        {
            var it = ConfigManager.Get<PropItemInfo>(id);
            if (it && FilterItem(it)) m_cachedItems.Insert(0, it);
        }

        if (!m_cachedItems.Contains(m_selectedItem)) m_selectedItem = null;

        if (!m_viewItems) m_viewItems = GetComponent<ScrollView>("searchPanel/content/tabs/items/list");

        if (m_dataItems == null) m_dataItems = new DataSource<PropItemInfo>(m_cachedItems, m_viewItems, OnSetItem, OnSelectItem);
        else
        {
            m_selectedItemNode?.GetComponent<TweenColor>("back").ReversePlayComplete();
            m_dataItems.UpdateItems();
        }
    }

    private void OnResetItemFilters()
    {
        m_lockFilterEvent = true;

        m_itemType.value = m_itemSubType.value = m_itemGender.value = m_itemQuality.value = m_itemClass.value = 1;
        m_itemCompose.isOn = m_itemDecompose.isOn = m_itemUpgrade.isOn = m_itemEnchant.isOn = m_itemSet.isOn = false;
        m_itemIcon.text = m_itemMesh.text = m_itemField.text = m_itemDrop.text = "";

        m_lockFilterEvent = false;

        if (m_searched.BitMask(0)) SearchItems(m_searchInput.text);
    }

    private void OnFilterItemType(int v)
    {
        OnFilterItemType(v, true);
    }

    private void OnFilterItemType(int v, bool refresh)
    {
        var t = v.Unique();
        if (t < 0 || t >= m_itemTypeNames.Length) m_itemSubType.interactable = false;
        else
        {
            var ss = m_itemSubTypeNames[t];
            var os = m_itemSubType.options;
            os.Clear();
            for (int i = 0, c = ss.Length; i < c; ++i) os.Add(new Dropdown.OptionData() { text = ss[i] == "-" ? i == 0 ? "所有" : "未使用" + i : ss[i] });
            m_itemSubType.options = os;
            m_itemSubType.interactable = ss.Length > 1;
            m_lockFilterEvent = true;
            m_itemSubType.value = 1;
            m_lockFilterEvent = false;
        }

        if (refresh) FilterItems();
    }

    private void FilterItems(int v) { FilterItems(); }

    private void FilterItems(bool b) { FilterItems(); }

    private void FilterItems(string s) { FilterItems(); }

    private void FilterItems()
    {
        if (m_lockFilterEvent) return;

        if (!m_searchInput) m_searchInput = GetComponent<InputField>("searchPanel/content/input");
        if (!m_tabs) m_tabs = GetComponent<UISingleContainer>("searchPanel/content/tabs");

        var text = m_searchInput.text;
        SearchItems(text);
    }

    private List<PropItemInfo> FilterItems(List<PropItemInfo> items)
    {
        if (items == null || items.Count < 1) return items;

        items.RemoveAll(i => !FilterItem(i));

        return items;
    }

    private bool FilterItem(PropItemInfo item)
    {
        if (!item) return false;

        var v = m_itemType.value;
        if (!v.BitMask(0) && !v.BitMask((int)item.itemType)) return false;

        if (v.Unique() > -1 && m_itemSubType.interactable)
        {
            v = m_itemSubType.value;
            if (!v.BitMask(0) && !v.BitMask(m_itemType.value.Unique() == (int)PropType.Weapon && item.subType > 5 ? 6 : item.subType)) return false;
        }

        v = m_itemGender.value;
        if (!v.BitMask(0) && (!v.BitMask(1) && item.sex != 0 && item.sex != 1 || !v.BitMask(2) && item.sex == 1 || !v.BitMask(3) && item.sex == 0)) return false;

        v = m_itemQuality.value;
        if (!v.BitMask(0) && !v.BitMask(item.quality + 1)) return false;

        v = m_itemClass.value;
        if (item.hasClassLimit && !v.BitMask(0) && item.proto.FindIndex(c => v.BitMask((int)c)) < 0) return false;

        if (m_itemCompose.isOn && item.compose == 0) return false;
        if (m_itemDecompose.isOn && item.decompose == 0) return false;
        if (m_itemUpgrade.isOn && !item.sublimation) return false;
        if (m_itemEnchant.isOn && !item.soulable) return false;
        if (m_itemSet.isOn && item.suite == 0) return false;

        var ss = Util.ParseString<string>(m_itemIcon.text, false, ',', ';', ' ');
        var cs = string.IsNullOrEmpty(item.icon) || string.IsNullOrWhiteSpace(item.icon) ? "null" : item.icon;
        var valid = ss.Length < 1 || ss.FindIndex(s => !string.IsNullOrEmpty(s)) < 0;

        foreach (var icon in ss)
        {
            if (string.IsNullOrEmpty(icon)) continue;
            if (cs == "null" && icon == cs || cs.Contains(icon))
            {
                valid = true;
                break;
            }
        }
        if (!valid) return false;


        ss = Util.ParseString<string>(m_itemMesh.text, false, ',', ';', ' ');
        valid = ss.Length < 1 || ss.FindIndex(s => !string.IsNullOrEmpty(s)) < 0;

        var ms = item.mesh;
        foreach (var mesh in ss)
        {
            if (ms.FindIndex(s => string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s) ? mesh == "null" : s.Contains(mesh)) < 0) continue;
            valid = true;
            break;
        }
        if (!valid) return false;

        var vs = Util.ParseString<int>(m_itemField.text, false, ',', ';', ' ');
        var fs = item.attributes;

        valid = vs.Length < 1 || vs.FindIndex(i => i > 0) < 0;
        foreach (var vv in vs)
        {
            if (fs.FindIndex(a => a.id == vv) < 0) continue;
            valid = true;
            break;
        }
        if (!valid) return false;

        vs = Util.ParseString<int>(m_itemDrop.text, false, ',', ';', ' ');
        valid = vs.Length < 1 || vs.FindIndex(i => i > 0) < 0;

        var ds = item.dropType;
        foreach (var d in vs)
        {
            if (ds.FindIndex(a => Util.Parse<int>(a.Split('-')[0]) == d) < 0) continue;
            valid = true;
            break;
        }
        if (!valid) return false;

        return true;
    }

    private void OnSetItem(RectTransform node, PropItemInfo data)
    {
        Util.SetItemInfoSimple(node, data);
        node.GetComponent<Text>("name").color   = m_qualityColors[data.quality < 0 ? 0 : data.quality >= m_qualityColors.Length ? m_qualityColors.Length - 1 : data.quality];
        node.GetComponent<Text>("type").text    = Util.Format("{0}\n<size=11><color=white>({1})</color></size>", GetItemTypeName(data.itemType), data.itemType);
        node.GetComponent<Text>("subType").text = Util.Format("{0}\n<size=11><color=white>({1})</color></size>", GetItemSubTypeNames(data.itemType, data.subType), GetItemSubTypeTypeName(data.itemType, data.subType));
        node.GetComponent<Text>("class").text   = !data.hasClassLimit ? "所有职业" : Util.Format("职业\n<size=11><color=white>({0})</color></size>", data.classLimit);

        var hold = moduleCangku.GetItemCount(data.ID);
        node.GetComponent<Text>("buttons/send/Text").text = Util.Format("<b>Request</b>\n<color=#{0}><size=10>Bag:{1}</size></color>", hold < 1 ? "ff0000" : "22ff22", hold);

        var btn = node.GetComponent<Button>("buttons/send");
        btn.onClick1.RemoveListener(OnRequestItem);
        btn.onClick1.AddListener(OnRequestItem);

        var t = node.GetComponent<TweenColor>("back");
        if (m_selectedItem == data)
        {
            t.PlayComplete();
            m_selectedItemNode = node;
        }
        else if (m_selectedItemNode == node) t.ReversePlayComplete();
    }

    private void OnSelectItem(RectTransform node, PropItemInfo data)
    {
        var selected = m_selectedItem ? m_cachedItems.IndexOf(m_selectedItem) : -1;

        if (selected > -1)
        {
            var n = m_viewItems.GetNodeByIndex(selected);
            if (n) n.GetComponent<TweenColor>("back").PlayReverse();
        }

        m_selectedItem = data;
        m_selectedItemNode = node;

        node.GetComponent<TweenColor>("back").PlayForward();
    }

    private void OnRequestItem(Button btn)
    {
        var node  = btn.transform.parent.parent;
        var id    = Util.Parse<int>(node.GetComponent<Text>("id").text);
        var count = Util.Parse<int>(node.GetComponent<Text>("buttons/count/Text").text);

        var i = ConfigManager.Get<PropItemInfo>(id);
        if (i.stackNum > 0 && i.stackNum * 20 < count) count = i.stackNum * 20;

        moduleGm.SendAddProp((ushort)id, count < 1 ? 1 : count);
    }

    void _ME(ModuleEvent<Module_Cangku> e)
    {
        if (e.moduleEvent == Module_Cangku.EventCangkuInfo)
        {
            m_selectedItemNode?.GetComponent<TweenColor>("back").ReversePlayComplete();
            m_dataItems?.UpdateItems();
            return;
        }

        if (e.moduleEvent == Module_Cangku.EventCangkuRemoveItem || e.moduleEvent == Module_Cangku.EventCangkuAddItem)
        {
            var pi = e.param1 as PItem;
            var i = pi != null ? m_cachedItems?.Find(p => p.ID == pi.itemTypeId) : null;
            if (i == null) return;
            m_dataItems.UpdateItem(m_cachedItems.IndexOf(i));
            return;
        }
    }

    #endregion
}
