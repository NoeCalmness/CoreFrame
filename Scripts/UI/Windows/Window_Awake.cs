// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-04      10:24
//  * LastModify：2018-08-04      14:35
//  ***************************************************************************************************/
#region

using UnityEngine;
using UnityEngine.UI;

#endregion

public class Window_Awake : Window
{
    public static AwakeType currentType = AwakeType.Skill;

    public AwakeHandle       awakeHandle;
    private Transform        skillAwakeRoot;
    private Transform        numberPanelRoot;
    public PropNumber_Panel _numberPanel;
    public SkillAwakePanel  _skillAwake;

    protected override void OnOpen()
    {
        InitComponent();

        base.OnOpen();
        if(skillAwakeRoot)
            _skillAwake = SubWindowBase.CreateSubWindow<SkillAwakePanel, Window_Awake>(this, skillAwakeRoot.gameObject);
        _numberPanel = SubWindowBase.CreateSubWindow<PropNumber_Panel>(this, numberPanelRoot?.gameObject);
        _numberPanel.Initialize();
        _numberPanel.Set(false);
    }

    protected override void OnClose()
    {
        base.OnClose();
        _skillAwake?.Destroy();
        _numberPanel?.Destroy();
    }

    private void InitComponent()
    {
        skillAwakeRoot  = GetComponent<Transform>("skillDetail");
        numberPanelRoot = GetComponent<Transform>("propNumber_Panel");
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();

        moduleHome.DispatchEvent(Module_Home.EventSwitchCameraMode, Event_.Pop(CameraShowType.Awake));

        Refresh();
    }

    protected override void OnHide(bool forward)
    {
        moduleHome.DispatchEvent(Module_Home.EventSwitchCameraMode, Event_.Pop(CameraShowType.Home));

        if (awakeHandle)
            awakeHandle.Destroy();
        awakeHandle = null;
    }

    private void _ME(ModuleEvent<Module_Awake> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Awake.Notice_AwakeInfoChange:
                Refresh();
                break;
            case Module_Awake.Response_SkillOpen:
                ResponseSkillOpen(e.msg as ScAwakeSkillOpen);
                break;
            case Module_Awake.Notice_ShowSkillOpenPanel:
                if(_skillAwake == null)
                    Module_Awake.instance.RequestAwakeSkill(e.param1 as AwakeInfo);
                else
                    _skillAwake.Initialize(e.param1, e.param2);
                    _numberPanel.UnInitialize();
                break;
        }
    }

    private void ResponseSkillOpen(ScAwakeSkillOpen msg)
    {
        if (msg == null)
            return;
        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9801, msg.result);
            return;    
        }
        AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
        awakeHandle.DispatchEvent(AwakeHandle.FocusPointChange, Event_.Pop(msg.lastAwakeSkill));
        if (_skillAwake.isInit)
        {
            _skillAwake.Refresh();
        }
    }

    private void Refresh()
    {
        awakeHandle = AwakeHandle.Create(currentType, (Level.current as Level_Home).awakeRoot.gameObject);
    }
}

public class SkillAwakePanel : SubWindowBase<Window_Awake>
{
    private Button  awakeButton;
    private Button  cancelButton;
    private Button  closeButton;
    private Button  bgButton;
    private Image   costIcon;
    private Text    costNum;
    private Text    ownNum;
    private Text    buttonText;

    private AwakeInfo info;
    private Text    skillDesc;
    private Image   skillIcon;
    private Text    skillName;
    private int     state;

    protected override void InitComponent()
    {
        base.InitComponent();
        awakeButton     = WindowCache.GetComponent<Button>  ("skillDetail/content/activation_btn");
        cancelButton    = WindowCache.GetComponent<Button>  ("skillDetail/content/levelUpAvailable/close_btn");
        closeButton     = WindowCache.GetComponent<Button>  ("skillDetail/content/top/close_button");
        bgButton        = WindowCache.GetComponent<Button>  ("skillDetail/bg");
        skillName       = WindowCache.GetComponent<Text>    ("skillDetail/content/skillicon/skillicon_name");
        skillDesc       = WindowCache.GetComponent<Text>    ("skillDetail/content/content_tip/Text");
        costNum         = WindowCache.GetComponent<Text>    ("skillDetail/content/levelUpAvailable/Tex_green");
        ownNum          = WindowCache.GetComponent<Text>    ("skillDetail/content/levelUpAvailable/Tex_green/Text");
        costIcon        = WindowCache.GetComponent<Image>   ("skillDetail/content/levelUpAvailable/ball");
        skillIcon       = WindowCache.GetComponent<Image>   ("skillDetail/content/skillicon/skillicon_ball");
        buttonText      = WindowCache.GetComponent<Text>    ("skillDetail/content/activation_btn/Text");
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;

        info =　p[0] as AwakeInfo;
        state = (int)p[1];
        
        awakeButton?.onClick.AddListener(OnAwakeClick);
        cancelButton?.onClick.AddListener(OnCancelClick);
        closeButton?.onClick.AddListener(() => UnInitialize());
        bgButton?.onClick.AddListener(() => UnInitialize());

        RefreshInfo();
        return true;
    }

    private void RefreshInfo()
    {
        awakeButton?.transform.SetGray(state != 0);
        awakeButton.SetInteractable(state == 0);
        Util.SetText(buttonText,
            ConfigText.GetDefalutString(TextForMatType.AwakePoint, state == 0 ? 11 : state == -1 ? 10 : 12));

        if (info != null)
        {
            Util.SetText(skillName, info.NameString);
            Util.SetText(skillDesc, info.DescString);
            AtlasHelper.SetShared(skillIcon, info.icon, null, true);
            if (info.cost != null)
            {
                var prop = ConfigManager.Get<PropItemInfo>(info.cost.itemId);
                AtlasHelper.SetIcons(costIcon, prop.icon);
                Util.SetText(costNum, info.cost.count.ToString());
                uint own = 0;
                if (prop.itemType == PropType.Currency)
                    own = modulePlayer.GetMoneyCount((CurrencySubType) prop.subType);
                else
                    own = moduleAwake.GetBeadCount(prop.subType);
                Util.SetText(ownNum, Util.Format(ConfigText.GetDefalutString((int) TextForMatType.AwakePoint, 9), own));
                costNum.color = ColorGroup.GetColor(ColorManagerType.IsMatrialEnough, info.cost.count <= own);
            }
        }
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;
        awakeButton ?.onClick.RemoveAllListeners();
        cancelButton?.onClick.RemoveAllListeners();
        closeButton ?.onClick.RemoveAllListeners();
        bgButton    ?.onClick.RemoveAllListeners();

        var awake = WindowCache as Window_Awake;
        awake?._numberPanel.Initialize();
        return true;
    }

    private void OnCancelClick()
    {
        UnInitialize();
    }

    private void OnAwakeClick()
    {
        if (null == info)
            return;
        Module_Awake.instance.RequestAwakeSkill(info.ID);
        UnInitialize();
    }

    public void Refresh()
    {
        state = moduleAwake.CheckAwakeState(info);
        RefreshInfo();
    }
}
