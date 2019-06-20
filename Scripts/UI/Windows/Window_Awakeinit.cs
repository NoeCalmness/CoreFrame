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

public class Window_Awakeinit : Window
{
    private const int AccomparyLockID = 40;

    private Button              accompary;
    private Button              energy;
    private Button              heart;

    private PropNumber_Panel    numberPanel;
    private Transform           numberPanelRoot;
    private Button              skill;

    private Transform[]         activeEffects;

    protected override void OnOpen()
    {
        InitComponent();
        MultiLangrage();
        base.OnOpen();
        numberPanel = SubWindowBase.CreateSubWindow<PropNumber_Panel>(this, numberPanelRoot?.gameObject);
        numberPanel?.Initialize();
        moduleGlobal.ShowGlobalLayerDefault();

        heart?.onClick.AddListener(OnHeartClick);
        skill?.onClick.AddListener(OnSkillClick);
        energy?.onClick.AddListener(OnEnergyClick);
        accompary?.onClick.AddListener(OnAccomparyClick);

        moduleAwake.RequestAwakeInfo();
    }

    protected override void OnClose()
    {
        base.OnClose();
        numberPanel?.Destroy();
    }
    private void MultiLangrage()
    {
        var t = ConfigManager.Get<ConfigText>((int)TextForMatType.AwakePoint);
        if (!t) return;
        Util.SetText(GetComponent<Text>("xin_Btn/Text"), t[0]);
        Util.SetText(GetComponent<Text>("ji_Btn/Text"),  t[1]);
        Util.SetText(GetComponent<Text>("ti_Btn/Text"),  t[2]);
        Util.SetText(GetComponent<Text>("ban_Btn/Text"), t[3]);
    }

    private void InitComponent()
    {
        accompary       = GetComponent<Button>    ("ban_Btn");
        energy          = GetComponent<Button>    ("ti_Btn");
        heart           = GetComponent<Button>    ("xin_Btn");
        skill           = GetComponent<Button>    ("ji_Btn");
        numberPanelRoot = GetComponent<Transform> ("propNumber_Panel");

        activeEffects = new Transform[(int)AwakeType.Max];
        activeEffects[(int)AwakeType.Accompany] = GetComponent<Transform>("ban_Btn/effnode_active");
        activeEffects[(int)AwakeType.Energy]    = GetComponent<Transform>("ti_Btn/effnode_active");
        activeEffects[(int)AwakeType.Skill]     = GetComponent<Transform>("ji_Btn/effnode_active");
        activeEffects[(int)AwakeType.Heart]     = GetComponent<Transform>("xin_Btn/effnode_active");
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();

        activeEffects[(int) AwakeType.Accompany].SafeSetActive(moduleGuide.IsActiveFunction(AccomparyLockID));
        activeEffects[(int) AwakeType.Energy].SafeSetActive(moduleAwake.CanAwake(AwakeType.Energy));
        activeEffects[(int) AwakeType.Skill].SafeSetActive(moduleAwake.CanAwake(AwakeType.Skill));
        activeEffects[(int) AwakeType.Heart].SafeSetActive(moduleAwake.CanAwake(AwakeType.Heart));

        accompary.SetInteractable(moduleGuide.IsActiveFunction(AccomparyLockID));
    }

    protected override void OnShow(bool forward)
    {
        numberPanel?.Initialize();
    }

    protected override void OnHide(bool forward)
    {
        numberPanel?.UnInitialize();
    }

    protected override void OnReturn()
    {
        var key1 = new NoticeDefaultKey(NoticeType.AwakeAccompany);
        var key2 = new NoticeDefaultKey(NoticeType.AwakeEnergy);
        var key3 = new NoticeDefaultKey(NoticeType.AwakeHeart);
        var key4 = new NoticeDefaultKey(NoticeType.AwakeSkill);
        moduleNotice.SetNoticeState(key1, moduleAwake.CanAwake(AwakeType.Accompany));
        moduleNotice.SetNoticeState(key2, moduleAwake.CanAwake(AwakeType.Energy));
        moduleNotice.SetNoticeState(key3, moduleAwake.CanAwake(AwakeType.Heart));
        moduleNotice.SetNoticeState(key4, moduleAwake.CanAwake(AwakeType.Skill));
        moduleNotice.SetNoticeReadState(key1);
        moduleNotice.SetNoticeReadState(key2);
        moduleNotice.SetNoticeReadState(key3);
        moduleNotice.SetNoticeReadState(key4);

        base.OnReturn();
    }

    private void OnAccomparyClick()
    {
        Window_Awake.currentType = AwakeType.Accompany;
        ShowAsync<Window_NpcRelationShip>();
    }

    private void OnEnergyClick()
    {
        Window_Awake.currentType = AwakeType.Energy;
        ShowAsync<Window_Awake>();
    }

    private void OnSkillClick()
    {
        Window_Awake.currentType = AwakeType.Skill;
        ShowAsync<Window_Awake>();
    }

    private void OnHeartClick()
    {
        Window_Awake.currentType = AwakeType.Heart;
        ShowAsync<Window_Awake>();
    }
}

public class PropNumber_Panel : SubWindowBase
{
    public Text accomparyCoin;

    public Text energyCoin;

    public Text heartCoin;

    public Text heartSoul;

    public Text skillCoin;

    private Button accomparyCoinButton;
    private Button energyCoinButton;
    private Button heartCoinButton;
    private Button heartSoulButton;
    private Button skillCoinButton;

    protected override void InitComponent()
    {
        accomparyCoin  = WindowCache.GetComponent<Text>("propNumber_Panel/huizhu_Img/huizhu_Text");
        energyCoin     = WindowCache.GetComponent<Text>("propNumber_Panel/tizhu_Img/tizhu_Text");
        heartCoin      = WindowCache.GetComponent<Text>("propNumber_Panel/xinzhu_Img/xinzhu_Text");
        heartSoul      = WindowCache.GetComponent<Text>("propNumber_Panel/xinhun_Img/xinhun_Text");
        skillCoin      = WindowCache.GetComponent<Text>("propNumber_Panel/jizhu_Img/jizhu_Text");

        accomparyCoinButton = WindowCache.GetComponent<Button>("propNumber_Panel/huizhu_Img");
        energyCoinButton    = WindowCache.GetComponent<Button>("propNumber_Panel/tizhu_Img");
        heartCoinButton     = WindowCache.GetComponent<Button>("propNumber_Panel/xinzhu_Img");
        heartSoulButton     = WindowCache.GetComponent<Button>("propNumber_Panel/xinhun_Img");
        skillCoinButton     = WindowCache.GetComponent<Button>("propNumber_Panel/jizhu_Img");

        BindEvent();
    }

    private void BindEvent()
    {
        accomparyCoinButton?.onClick.AddListener(() =>
        {
            var prop = ConfigManager.Find<PropItemInfo>(item => item.itemType == PropType.AwakeCurrency && item.subType == (int) AwakeType.Accompany - 1);
            moduleGlobal.UpdateGlobalTip((ushort)prop.ID);
        });

        energyCoinButton?.onClick.AddListener(() =>
        {
            var prop =ConfigManager.Find<PropItemInfo>(item => item.itemType == PropType.AwakeCurrency && item.subType == (int) AwakeType.Energy - 1);
            moduleGlobal.UpdateGlobalTip((ushort)prop.ID);
        });

        heartCoinButton?.onClick.AddListener(() =>
        {
            var prop = ConfigManager.Find<PropItemInfo>(item => item.itemType == PropType.AwakeCurrency && item.subType == (int)AwakeType.Heart - 1);
            moduleGlobal.UpdateGlobalTip((ushort)prop.ID);
        });

        heartSoulButton?.onClick.AddListener(() =>
        {
            var prop =ConfigManager.Find<PropItemInfo>(item => item.itemType == PropType.Currency && item.subType == (int) CurrencySubType.HeartSoul);
            moduleGlobal.UpdateGlobalTip((ushort)prop.ID);
        });

        skillCoinButton?.onClick.AddListener(() =>
        {
            var prop =ConfigManager.Find<PropItemInfo>(item => item.itemType == PropType.AwakeCurrency && item.subType == (int) AwakeType.Skill - 1);
            moduleGlobal.UpdateGlobalTip((ushort)prop.ID);
        });
    }

    public override bool Initialize(params object[] p)
    {
        if (base.Initialize(p))
            Refresh();
        return true;
    }

    private void Refresh()
    {
        Util.SetText(heartCoin,     moduleAwake.GetBeadCount(AwakeType.Heart).ToString());
        Util.SetText(skillCoin,     moduleAwake.GetBeadCount(AwakeType.Skill).ToString());
        Util.SetText(energyCoin,    moduleAwake.GetBeadCount(AwakeType.Energy).ToString());
        Util.SetText(accomparyCoin, moduleAwake.GetBeadCount(AwakeType.Accompany).ToString());
        Util.SetText(heartSoul,     modulePlayer.GetMoneyCount(CurrencySubType.HeartSoul).ToString());
    }

    private void _ME(ModuleEvent<Module_Player> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Player.EventCurrencyChanged:
                Refresh();
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Equip> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Equip.EventBagInfo:
            case Module_Equip.EventUpdateBagProp:
                Refresh();
                break;
        }
    }
}