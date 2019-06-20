/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   wangyifan <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-06-20
 * 
 ***************************************************************************************************/

using UnityEngine.UI;
using UnityEngine;
using System;

public class Window_Attribute : Window
{
    public const string RoleImageName = "attr_bg_{0}_{1}";

    #region base
    private int configID;
    private Text roleName;
    private Image roleImage;
    private Text tips;
    private Text m_Lv;
    private Image lvFillAmount;
    private Button expUpBtn;

    //长按相关
    private int changePoint;
    //长按结束

    private int pointPower = 0;
    private int pointTechnique = 0;
    private int pointBodyPower = 0;
    private int total;
    private int totalPoint = 0;
    private Text m_Potential;//潜力点

    private Text m_PowerView;//力量
    private Text m_PowerChanged;
    private Button m_PowerAddBtn;
    private Button m_PowerSubBtn;

    private Text m_TechniqueView;//技巧
    private Text m_TechniqueChanged;
    private Button m_TechniqueAddBtn;
    private Button m_TechniqueSubBtn;

    private Text m_EnergyView;//体力
    private Text m_EnergyChanged;
    private Button m_EnergyAddBtn;
    private Button m_EnergySubBtn;

    private Text m_Attack;//攻击力
    private Text m_Defense;//防御力
    private Text m_KnockRate;//暴击率
    private Text m_MaxHp;//生命值
    private Text m_Knock;//暴击伤害
    private Text m_Artifice;//韧性
    private Text m_Brutal;//残暴
    private Text m_Bone;//铁骨
    private Text m_AttackSpeed;//攻击速度
    private Text m_MoveSpeed;//移动速度
    #endregion

    #region 重置属性点
    private Button m_ResetAllBtn;
    private Image m_ResetPanel;
    private Text remainText;
    private Button m_ResetBtn;
    #endregion

    #region 保存属性点
    private Button m_SavePointBtn;
    private Image m_tips;
    private Text tip_text;
    private Button m_SaveBtn;//加点确认按钮

    private Text before_ll_num;
    private Text before_jq_num;
    private Text before_tz_num;
    private Text after_ll_num;
    private Text after_jq_num;
    private Text after_tz_num;
    private Animator qian_Anim;
    #endregion

    #region calculate
    private double oldHp;
    private double oldBrutal;
    private double oldAttack;
    private int oldDefen;
    private int oldbone;

    ushort diamondPay = 0;
    private float weaponRate;//武器系数
    private float weaponAddition;//武器品质加成
    public int weaponType;
    #endregion

    protected override void OnOpen()
    {
        #region base
        configID = (int)TextForMatType.AttributeUIText;
        roleImage = GetComponent<Image>("left/roleImage");
        tips = GetComponent<Text>("bg/xiafangblack/xiafangwenzi");
        m_Lv = GetComponent<Text>("button/level/lv_back/label");
        lvFillAmount = GetComponent<Image>("button/level/exp/expbar/exp_back (1)");

        changePoint = GeneralConfigInfo.defaultConfig.addPoint;
        m_Potential = GetComponent<Text>("qiannengdianshu/shuzi");
        expUpBtn = GetComponent<Button>("button/level/exp/buff");
        expUpBtn.SafeSetActive(false);
        expUpBtn.onClick.RemoveAllListeners();
        expUpBtn.onClick.AddListener(() => modulePlayer.ClickExpBtn());
        roleName = GetComponent<Text>("button/name_back/text");
        m_PowerView = GetComponent<Text>("right/ll_suxingkuang/yellow_text");
        m_PowerChanged = GetComponent<Text>("right/ll_suxingkuang/greed_Text");
        m_PowerAddBtn = GetComponent<Button>("right/ll_suxingkuang/jiahao");
        m_PowerSubBtn = GetComponent<Button>("right/ll_suxingkuang/jianhao");
        m_PowerAddBtn.onClick.RemoveAllListeners();
        m_PowerAddBtn.onClick.AddListener(OnPowerAdd);
        m_PowerSubBtn.onClick.RemoveAllListeners();
        m_PowerSubBtn.onClick.AddListener(OnPowerSub);
        EventTriggerListener.Get(m_PowerAddBtn?.gameObject).onPress = OnPowerPressAdd;
        EventTriggerListener.Get(m_PowerSubBtn?.gameObject).onPress = OnPowerPressSub;

        m_TechniqueView = GetComponent<Text>("right/jq_suxingkuang/yellow_text");
        m_TechniqueChanged = GetComponent<Text>("right/jq_suxingkuang/greed_Text");
        m_TechniqueAddBtn = GetComponent<Button>("right/jq_suxingkuang/jiahao");
        m_TechniqueSubBtn = GetComponent<Button>("right/jq_suxingkuang/jianhao");
        m_TechniqueAddBtn.onClick.RemoveAllListeners();
        m_TechniqueAddBtn.onClick.AddListener(OnTechniqueAdd);
        m_TechniqueSubBtn.onClick.RemoveAllListeners();
        m_TechniqueSubBtn.onClick.AddListener(OnTechniqueSub);
        EventTriggerListener.Get(m_TechniqueAddBtn?.gameObject).onPress = OnTechniquePressAdd;
        EventTriggerListener.Get(m_TechniqueSubBtn?.gameObject).onPress = OnTechniquePressSub;

        m_EnergyView = GetComponent<Text>("right/tl_suxingkuang/yellow_text");
        m_EnergyChanged = GetComponent<Text>("right/tl_suxingkuang/greed_Text");
        m_EnergyAddBtn = GetComponent<Button>("right/tl_suxingkuang/jiahao");
        m_EnergySubBtn = GetComponent<Button>("right/tl_suxingkuang/jianhao");
        m_EnergyAddBtn.onClick.RemoveAllListeners();
        m_EnergyAddBtn.onClick.AddListener(OnEnergyAdd);
        m_EnergySubBtn?.onClick.RemoveAllListeners();
        m_EnergySubBtn?.onClick.AddListener(OnEnergySub);
        EventTriggerListener.Get(m_EnergyAddBtn?.gameObject).onPress = OnEnergyPressAdd;
        EventTriggerListener.Get(m_EnergySubBtn?.gameObject).onPress = OnEnergyPressSub;

        m_Attack = GetComponent<Text>("right/zd_frame/gj_shuxingkuang/gj_Text");
        m_Defense = GetComponent<Text>("right/zd_frame/fanyuli_shuxingkuang/fyl_Text");
        m_KnockRate = GetComponent<Text>("right/zd_frame/bjl_shuxingkuang/bjl_Text");
        m_MaxHp = GetComponent<Text>("right/zd_frame/smz_shuxingkuang/smz_Text");
        m_Knock = GetComponent<Text>("right/zd_frame/bjsh_shuxingkuang/bjsh_Text");
        m_Artifice = GetComponent<Text>("right/zd_frame/rx_shuxingkuang/rx_Text");
        m_Brutal = GetComponent<Text>("right/zd_frame/cb_shuxingkuang/cb_Text");
        m_Bone = GetComponent<Text>("right/zd_frame/tg_shuxingkuang/tg_Text");
        m_AttackSpeed = GetComponent<Text>("right/zd_frame/gjsd_shuxingkuang/gjsd_Text");
        m_MoveSpeed = GetComponent<Text>("right/zd_frame/ydsd_shuxingkuang/ydsd_Text");
        #endregion

        #region 重置
        m_ResetAllBtn = GetComponent<Button>("right/xidian");
        m_ResetAllBtn.onClick.RemoveAllListeners();
        m_ResetAllBtn.onClick.AddListener(OnResetPanel);
        m_ResetPanel = GetComponent<Image>("center/2_Panel");
        remainText = GetComponent<Text>("center/2_Panel/info/yongmukuang_frame/remain");
        m_ResetBtn = GetComponent<Button>("center/2_Panel/info/equiped");
        m_ResetBtn.onClick.RemoveAllListeners();
        m_ResetBtn.onClick.AddListener(OnReset);
        #endregion

        #region 保存
        m_tips = GetComponent<Image>("center/1_Panel");
        tip_text = GetComponent<Text>("center/1_Panel/equiped/Text");
        m_SaveBtn = GetComponent<Button>("center/1_Panel/equiped");
        m_SaveBtn.onClick.RemoveAllListeners();
        m_SaveBtn.onClick.AddListener(OnSave);
        m_SavePointBtn = GetComponent<Button>("right/save");
        m_SavePointBtn.onClick.RemoveAllListeners();
        m_SavePointBtn.onClick.AddListener(SavePoint);

        before_ll_num = GetComponent<Text>("center/1_Panel/middle/liliang/before");
        before_jq_num = GetComponent<Text>("center/1_Panel/middle/jiqiao/before");
        before_tz_num = GetComponent<Text>("center/1_Panel/middle/tizhi/before");
        after_ll_num = GetComponent<Text>("center/1_Panel/middle/liliang/after");
        after_jq_num = GetComponent<Text>("center/1_Panel/middle/jiqiao/after");
        after_tz_num = GetComponent<Text>("center/1_Panel/middle/tizhi/after");
        qian_Anim = GetComponent<Animator>("qiannengdianshu");
        #endregion
             
        InitText();
    }

    private void OnEnergyPressSub(GameObject go)
    {
        OnEnergyAddState(false, changePoint);
    }

    private void OnEnergyPressAdd(GameObject go)
    {
        OnEnergyAddState(true, changePoint);
    }

    private void OnTechniquePressSub(GameObject go)
    {
        OnTechniqueAddState(false, changePoint);
    }

    private void OnTechniquePressAdd(GameObject go)
    {
        OnTechniqueAddState(true, changePoint);
    }

    private void OnPowerPressSub(GameObject go)
    {
        OnPowerAddState(false, changePoint);
    }

    private void OnPowerPressAdd(GameObject go)
    {
        OnPowerAddState(true, changePoint);
    }

    private void InitText()
    {
        Util.SetText(GetComponent<Text>("bg/biaoti/text1"), configID, 25);
        Util.SetText(GetComponent<Text>("bg/biaoti/text2"), configID, 26);
        Util.SetText(GetComponent<Text>("qiannengdianshu/hanzi"), configID, 27);

        Util.SetText(GetComponent<Text>("center/1_Panel/top/equipinfo"), (int)TextForMatType.PublicUIText, 6);
        Util.SetText(GetComponent<Text>("center/1_Panel/middle/liliang/text"), configID, 2);
        Util.SetText(GetComponent<Text>("center/1_Panel/middle/jiqiao/text"), configID, 3);
        Util.SetText(GetComponent<Text>("center/1_Panel/middle/tizhi/text"), configID, 4);
        Util.SetText(GetComponent<Text>("center/1_Panel/middle/Text"), configID, 24);

        Util.SetText(GetComponent<Text>("right/ll_suxingkuang/liliang"), configID, 2);
        Util.SetText(GetComponent<Text>("right/jq_suxingkuang/jiqiao"), configID, 3);
        Util.SetText(GetComponent<Text>("right/tl_suxingkuang/tili"), configID, 4);

        Util.SetText(GetComponent<Text>("right/zd_frame/gj_shuxingkuang/demage"), configID, 7);
        Util.SetText(GetComponent<Text>("right/zd_frame/fanyuli_shuxingkuang/defence"), configID, 8);
        Util.SetText(GetComponent<Text>("right/zd_frame/bjl_shuxingkuang/criticaldemage"), configID, 10);
        Util.SetText(GetComponent<Text>("right/zd_frame/smz_shuxingkuang/health"), configID, 5);
        Util.SetText(GetComponent<Text>("right/zd_frame/bjsh_shuxingkuang/criticalchance"), configID, 9);
        Util.SetText(GetComponent<Text>("right/zd_frame/rx_shuxingkuang/resilience"), configID, 11);
        Util.SetText(GetComponent<Text>("right/zd_frame/cb_shuxingkuang/criticaldemage"), configID, 15);
        Util.SetText(GetComponent<Text>("right/zd_frame/tg_shuxingkuang/firm"), configID, 14);
        Util.SetText(GetComponent<Text>("right/zd_frame/gjsd_shuxingkuang/attackspeed"), configID, 12);
        Util.SetText(GetComponent<Text>("right/zd_frame/ydsd_shuxingkuang/movespeed"), configID, 13);

        Util.SetText(GetComponent<Text>("center/2_Panel/info/top/equipinfo"), (int)TextForMatType.PublicUIText, 6);
        Util.SetText(GetComponent<Text>("center/2_Panel/info/frame_Text"), configID, 18);
        Util.SetText(GetComponent<Text>("center/2_Panel/info/yongmukuang_frame/xiaofei_Text"), configID, 19);
        Util.SetText(GetComponent<Text>("center/2_Panel/info/equiped/Text"), configID, 1);
        Util.SetText(GetComponent<Text>("center/2_Panel/info/frame_Text"), configID, 18);

        Util.SetText(GetComponent<Text>("right/save/save_Text"), (int)TextForMatType.PublicUIText, 4);

        Util.SetText(GetComponent<Text>("right/ll_suxingkuang/content"), 260, 10);
        Util.SetText(GetComponent<Text>("right/jq_suxingkuang/content"), 260, 11);
        Util.SetText(GetComponent<Text>("right/tl_suxingkuang/content"), 260, 12);
        Util.SetText(GetComponent<Text>("right/zd_frame/gj_shuxingkuang/content"), 260, 0);
        Util.SetText(GetComponent<Text>("right/zd_frame/fanyuli_shuxingkuang/content"), 260, 1);
        Util.SetText(GetComponent<Text>("right/zd_frame/smz_shuxingkuang/content"), 260, 2);
        Util.SetText(GetComponent<Text>("right/zd_frame/cb_shuxingkuang/content"), 260, 3);
        Util.SetText(GetComponent<Text>("right/zd_frame/tg_shuxingkuang/content"), 260, 4);
        Util.SetText(GetComponent<Text>("right/zd_frame/bjl_shuxingkuang/content"), 260, 5);
        Util.SetText(GetComponent<Text>("right/zd_frame/bjsh_shuxingkuang/content"), 260, 6);
        Util.SetText(GetComponent<Text>("right/zd_frame/rx_shuxingkuang/content"), 260, 7);
        Util.SetText(GetComponent<Text>("right/zd_frame/gjsd_shuxingkuang/content"), 260, 8);
        Util.SetText(GetComponent<Text>("right/zd_frame/ydsd_shuxingkuang/content"), 260, 9);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();//top工具条的显示
        moduleAttribute.isRead = true;
        SetRoleImage();
        RefreshUiAttribute();

        DefaultStata();
        SetEnterState();
    }

    private void SetEnterState()
    {
        Util.SetText(roleName, modulePlayer.roleInfo.roleName);
        Util.SetText(m_Lv, modulePlayer.roleInfo.level.ToString());
        SetPotentialText(modulePlayer.roleInfo.attrPoint);

        I_Hidden(modulePlayer.roleInfo.attrPoint > 0);
        lvFillAmount.fillAmount = modulePlayer.GetExpBarProgress();

        int point = modulePlayer.roleInfo.level * 3 - 3;
        m_ResetAllBtn.SafeSetActive(totalPoint != point);
        expUpBtn.SafeSetActive(modulePlayer.isHaveExpCard);

        var prop = ConfigManager.Get<PropItemInfo>(moduleEquip.weaponItemID);
        weaponRate = prop != null ? prop.powerCovertRate : 0;

        weaponAddition = 0;
        weaponType = 0;
        WeaponAttribute info = ConfigManager.Get<WeaponAttribute>(moduleEquip.weapon.itemTypeId);
        if (info != null)
        {
            int index = moduleEquip.weapon.growAttr != null && moduleEquip.weapon.growAttr.equipAttr != null ? moduleEquip.weapon.growAttr.equipAttr.star - 1 : 0;
            if (index < 0 || info.quality == null || index >= info.quality?.Length || info.quality[index].attributes == null || info.quality[index].attributes?.Length <= 0)
            {
                //Logger.LogError("config is error");
                return;
            }
            weaponAddition = info.quality != null && info.quality.Length > index + 1 ? info.quality[index].attributes[0].value : 0;
            weaponType = info.quality[index].attributes[0].type;
        }
    }

    private void SavePoint()
    {
        if (moduleAttribute.attrInfo == null)
        {
            Logger.LogError("player attr info is null");
            return;
        }

        m_tips.SafeSetActive(true);
        m_SaveBtn.SafeSetActive(true);
        Util.SetText(tip_text, configID, 16);

        Util.SetText(before_ll_num, ((int)moduleAttribute.attrInfo.power[0]).ToString());
        Util.SetText(before_jq_num, ((int)moduleAttribute.attrInfo.tenacious[0]).ToString());
        Util.SetText(before_tz_num, ((int)moduleAttribute.attrInfo.energy[0]).ToString());

        int ll_num = 0;
        int jq_num = 0;
        int tz_num = 0;
        if (!string.IsNullOrEmpty(m_PowerChanged.text)) ll_num = Util.Parse<int>(m_PowerChanged.text);
        if (!string.IsNullOrEmpty(m_TechniqueChanged.text)) jq_num = Util.Parse<int>(m_TechniqueChanged.text);
        if (!string.IsNullOrEmpty(m_EnergyChanged.text)) tz_num = Util.Parse<int>(m_EnergyChanged.text);

        Util.SetText(after_ll_num, ((int)moduleAttribute.attrInfo.power[0] + ll_num).ToString());
        Util.SetText(after_jq_num, ((int)moduleAttribute.attrInfo.tenacious[0] + jq_num).ToString());
        Util.SetText(after_tz_num, ((int)moduleAttribute.attrInfo.energy[0] + tz_num).ToString());
    }

    private void SetPotentialText(int point)
    {
        Util.SetText(m_Potential, point.ToString());
        if (point > 0)
        {
            Util.SetText(tips, configID, 28);
            qian_Anim.enabled = true;
            qian_Anim.Play("ui_attribute_normal");
            I_Hidden(true);
        }
        else
        {
            Util.SetText(tips, configID, 29);
            qian_Anim.Play("ui_attribute_stop");
        }
    }

    private void I_Hidden(bool hide)
    {
        //加减号 保存按钮隐藏（找其他地方显示洗点）
        m_PowerAddBtn.SafeSetActive(hide);
        m_PowerSubBtn.SafeSetActive(hide);
        m_TechniqueAddBtn.SafeSetActive(hide);
        m_TechniqueSubBtn.SafeSetActive(hide);
        m_EnergyAddBtn.SafeSetActive(hide);
        m_EnergySubBtn.SafeSetActive(hide);
    }

    private void SetRoleImage()
    {
        string str = Util.Format(RoleImageName, modulePlayer.gender, moduleEquip.weapon?.GetPropItem()?.subType);

        UIDynamicImage.LoadImage(roleImage.transform, str);
    }

    private void OnResetPanel()
    {
        var info = moduleGlobal.system.resetPrice;
        bool isContains = false;
        for (int i = 0; i < info.Length; i++)
        {
            if (info[i].times == modulePlayer.roleInfo.resetTimes + 1)
            {
                isContains = true;
                break;
            }
        }
        diamondPay = isContains ? info[modulePlayer.roleInfo.resetTimes + 1].price : info[0].price;
        if (modulePlayer.roleInfo.diamond >= diamondPay) Util.SetText(remainText, Util.Format(Util.GetString((int)TextForMatType.TravalShopUIText, 11), diamondPay, modulePlayer.roleInfo.diamond));
        else Util.SetText(remainText, GeneralConfigInfo.GetNoEnoughColorString(ConfigText.GetDefalutString(configID, 34)), diamondPay, modulePlayer.roleInfo.diamond);
        int point = 0;
        if (!string.IsNullOrEmpty(m_Lv.text)) point = Util.Parse<int>(m_Lv.text) * 3 - 3;
        if (totalPoint == point)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.AttributeUIText, 21));
            return;
        }
        m_ResetPanel.SafeSetActive(true);
    }

    private void OnReset()
    {
        if (modulePlayer.roleInfo.diamond < diamondPay)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.PublicUIText, 21));
            return;
        }
        moduleAttribute.ResetAttr();
        m_ResetPanel.SafeSetActive(false);
    }

    private void OnSave()
    {
        m_tips.SafeSetActive(false);
        moduleAttribute.SendAddAttr((ushort)pointPower, (ushort)pointTechnique, (ushort)pointBodyPower);
    }

    private void OnPowerAddState(bool isAdd, int num = 1)
    {
        int power = Util.Parse<int>(m_PowerView.text);

        if (isAdd)
        {
            if (total < totalPoint)
            {
                int number = totalPoint - total;
                num = number >= num ? num : number;

                pointPower += num;
                total = pointPower + pointTechnique + pointBodyPower;
                power += num;
            }
        }
        else
        {
            if (pointPower > 0 && pointPower <= totalPoint)
            {
                num = pointPower >= num ? num : pointPower;

                pointPower -= num;
                total = pointPower + pointTechnique + pointBodyPower;
                power -= num;
            }
        }

        Util.SetText(m_PowerView, power.ToString());
        Util.SetText(m_PowerChanged, pointPower > 0 ? pointPower.ToString() : string.Empty);
        SetPotentialText(totalPoint - total);

        Util.SetText(m_Attack, ((int)(oldAttack + GetFinallyValue(7, pointPower * moduleGlobal.system.powerToAttack * weaponRate, weaponAddition))).ToString());
        ChangeButtonStata();
    }

    private void OnPowerAdd()
    {
        OnPowerAddState(true);
    }

    private void OnPowerSub()
    {
        OnPowerAddState(false);
    }

    private void OnTechniqueAddState(bool isAdd, int num = 1)
    {
        int technique = Util.Parse<int>(m_TechniqueView.text);
        if (isAdd)
        {
            if (total < totalPoint)
            {
                int number = totalPoint - total;
                num = number >= num ? num : number;

                pointTechnique += num;
                total = pointPower + pointTechnique + pointBodyPower;
                technique += num;

                oldDefen += num;
                oldbone += num;
            }
        }
        else
        {
            if (pointTechnique > 0 && pointTechnique <= totalPoint)
            {
                num = pointTechnique >= num ? num : pointTechnique;

                pointTechnique -= num;
                total = pointPower + pointTechnique + pointBodyPower;
                technique -= num;

                oldDefen -= num;
                oldbone -= num;
            }
        }

        Util.SetText(m_TechniqueView, technique.ToString());
        Util.SetText(m_TechniqueChanged, pointTechnique > 0 ? pointTechnique.ToString() : string.Empty);
        SetPotentialText(totalPoint - total);

        Util.SetText(m_Defense, oldDefen.ToString());
        Util.SetText(m_Bone, oldbone.ToString());
        Util.SetText(m_Brutal, ((int)(oldBrutal + GetFinallyValue(15, pointTechnique * moduleGlobal.system.tenaciousToBrutal))).ToString());
        ChangeButtonStata();
    }

    private void OnTechniqueAdd()
    {
        OnTechniqueAddState(true);
    }

    private void OnTechniqueSub()
    {
        OnTechniqueAddState(false);
    }

    private void OnEnergyAddState(bool isAdd, int num = 1)
    {
        int energy = Util.Parse<int>(m_EnergyView.text);

        if (isAdd)
        {
            if (total < totalPoint)
            {
                int number = totalPoint - total;
                num = number >= num ? num : number;

                pointBodyPower += num;
                total = pointPower + pointTechnique + pointBodyPower;
                energy += num;
            }
        }
        else
        {
            if (pointBodyPower > 0 && pointBodyPower <= totalPoint)
            {
                num = pointBodyPower >= num ? num : pointBodyPower;

                pointBodyPower -= num;
                total = pointPower + pointTechnique + pointBodyPower;
                energy -= num;
            }
        }

        Util.SetText(m_EnergyView, energy.ToString());
        Util.SetText(m_EnergyChanged, pointBodyPower > 0 ? pointBodyPower.ToString() : string.Empty);
        SetPotentialText(totalPoint - total);

        Util.SetText(m_MaxHp, ((int)(oldHp + GetFinallyValue(5, pointBodyPower * moduleGlobal.system.energyToHp))).ToString());
        ChangeButtonStata();
    }

    private void OnEnergyAdd()
    {
        OnEnergyAddState(true);
    }

    private void OnEnergySub()
    {
        OnEnergyAddState(false);
    }

    private double GetFinallyValue(ushort id, double beginValue, float weaponAddRate = 0)
    {
        if (moduleRune.runeSuiteEffect == null) return beginValue * (1 + weaponAddRate);

        foreach (var item in moduleRune.runeSuiteEffect)
        {
            if (item.Key == 1)//递增
            {
                foreach (var value in item.Value)
                    if (id == value.Key) return weaponType == 1 ? beginValue + value.Value + weaponAddRate : (beginValue + value.Value) * (1 + weaponAddRate);
            }
            else if (item.Key == 2)//递乘
            {
                foreach (var value in item.Value)
                    if (id == value.Key) return weaponType == 1 ? beginValue * (1 + value.Value) + weaponAddRate : beginValue * (1 + value.Value + weaponAddRate);
            }
        }
        return beginValue * (1 + weaponAddRate);
    }

    private void _ME(ModuleEvent<Module_Attribute> e)
    {
        if (!actived) return;
        switch (e.moduleEvent)
        {
            case Module_Attribute.EventAddSuccess:
                I_Hidden(modulePlayer.roleInfo.attrPoint > 0);
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.AttributeUIText, 33));
                RefreshUiAttribute();
                DefaultStata();
                m_ResetAllBtn.SafeSetActive(true);
                break;
            case Module_Attribute.EventAddFailed:
                var p = e.msg as ScAddCustomAttr;
                if (p.result == 1) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.AttributeUIText, 22));
                if (p.result == 2) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.AttributeUIText, 23));
                break;
            case Module_Attribute.EventLevelUp:
                var roleLvUp = e.msg as ScRoleLevelUp;
                if (roleLvUp!=null) Util.SetText(m_Lv, roleLvUp.level.ToString());                
                SetPotentialText(modulePlayer.roleInfo.attrPoint);

                RefreshUiAttribute();
                DefaultStata();
                break;
            case Module_Attribute.EventResetAttributeSuceess:
                //洗点成功 
                AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.AttributeUIText, 32));
                SetPotentialText(modulePlayer.roleInfo.attrPoint);

                RefreshUiAttribute();
                DefaultStata();
                m_ResetAllBtn.SafeSetActive(false);
                modulePlayer.roleInfo.resetTimes++;
                break;
            case Module_Attribute.EventResetAttributeFailed:
                sbyte result = (sbyte)e.param1;
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.PublicUIText, result == 1 ? 21 : 31));
                break;
            default:
                break;
        }
    }

    private void DefaultStata()
    {
        totalPoint = modulePlayer.roleInfo.attrPoint;
        total = 0;
        pointPower = 0;
        pointTechnique = 0;
        pointBodyPower = 0;
        Util.SetText(m_PowerChanged, string.Empty);
        Util.SetText(m_TechniqueChanged, string.Empty);
        Util.SetText(m_EnergyChanged, string.Empty);

        ChangeButtonStata();
    }

    private void ChangeButtonStata()
    {
        m_PowerAddBtn.interactable = totalPoint > 0 && total < totalPoint;
        m_TechniqueAddBtn.interactable = totalPoint > 0 && total < totalPoint;
        m_EnergyAddBtn.interactable = totalPoint > 0 && total < totalPoint;
        m_PowerSubBtn.interactable = pointPower > 0;
        m_TechniqueSubBtn.interactable = pointTechnique > 0;
        m_EnergySubBtn.interactable = pointBodyPower > 0;
        m_SavePointBtn.SafeSetActive(total != 0);
    }

    private void RefreshUiAttribute()
    {
        if (moduleAttribute.attrInfo == null)
        {
            Logger.LogError("player attr info is null");
            return;
        }
        Util.SetText(m_PowerView    , ((int)moduleAttribute.attrInfo.power[0]).ToString());
        Util.SetText(m_TechniqueView, ((int)moduleAttribute.attrInfo.tenacious[0]).ToString());
        Util.SetText(m_EnergyView   , ((int)moduleAttribute.attrInfo.energy[0]).ToString());
        Util.SetText(m_Attack       , ((int)moduleAttribute.attrInfo.attack[0]).ToString());
        Util.SetText(m_Defense      , ((int)moduleAttribute.attrInfo.defense[0]).ToString());
        Util.SetText(m_KnockRate    , moduleAttribute.attrInfo.knockRate[0].ToString("P2"));
        Util.SetText(m_MaxHp        , ((int)moduleAttribute.attrInfo.maxHp[0]).ToString());
        Util.SetText(m_Knock        , moduleAttribute.attrInfo.knock[0].ToString("P2"));
        Util.SetText(m_Artifice     , moduleAttribute.attrInfo.artifice[0].ToString("P2"));
        Util.SetText(m_Brutal       , ((int)moduleAttribute.attrInfo.brutal[0]).ToString());
        Util.SetText(m_Bone         , ((int)moduleAttribute.attrInfo.bone[0]).ToString());
        Util.SetText(m_AttackSpeed  , moduleAttribute.attrInfo.attackSpeed[0].ToString("P2"));
        Util.SetText(m_MoveSpeed    , moduleAttribute.attrInfo.moveSpeed[0].ToString("P2"));

        SetOldThreeAttribute();
    }

    private void SetOldThreeAttribute()
    {
        oldHp = moduleAttribute.attrInfo.maxHp[0];
        oldBrutal = moduleAttribute.attrInfo.brutal[0];
        oldAttack = moduleAttribute.attrInfo.attack[0];
        oldDefen = (int)moduleAttribute.attrInfo.defense[0];
        oldbone = (int)moduleAttribute.attrInfo.bone[0];
    }
}
