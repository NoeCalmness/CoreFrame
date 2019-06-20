/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-10-18
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Upload : Window
{
    private List<string> audioup = new List<string>();

    private RectTransform m_leftObj;
    private RectTransform m_rightObj;
    private GameObject m_itemLeft;
    private GameObject m_itemRight;
    private GameObject m_levelMax;
    private GameObject m_levelNormal;

    private Text m_levelLeft;
    private Text m_levelRight;
    private Text m_hurtLeft;
    private Text m_hurtRight;
    private Text m_hurtGap;
    private Button m_uploadProp;
    private Text m_propHave;
    private Text m_propNeed;
    private Button m_uploadBtn;
    private Text m_ipLoadText;

    private Button Center_plane;
    private GameObject Succedtip;
    private Text WeaponName;
    private Text Levelbefore;
    private Text levelNow;
    private Text attackbefore;
    private Text acctacknow;
    private GameObject wupinbefore;
    private GameObject wupinnow;
    public Text m_hurtTxt;

    private PItem m_item;

    private Image m_weaponIcon;
    private Text m_weaponName;
    private Text m_strengLevel;
    private WeaponAttribute weaponAttr;
    private int wType;
    private Text m_upLeftTxt;
    private Text m_upRightTxt;

    private string[] m_weaponType = new string[7] { "career_icon_01", "career_icon_02", "career_icon_06", "career_icon_04", "career_icon_05", "ui_equip_02", "ui_equip_04" };

    protected override void OnOpen()
    {
        audioup.Clear();
        audioup.AddRange(AudioInLogicInfo.audioConst.upWeaStageSucc);

        m_leftObj = GetComponent<RectTransform>("preview/damagenumberbackground/left");
        m_rightObj = GetComponent<RectTransform>("preview/damagenumberbackground/right");
        m_itemLeft = GetComponent<RectTransform>("preview/damagenumberbackground/left/item_left").gameObject;
        m_itemRight = GetComponent<RectTransform>("preview/damagenumberbackground/right/item_right").gameObject;
        m_levelNormal = GetComponent<RectTransform>("preview/weaponprogresswhite/no").gameObject;
        m_levelMax = GetComponent<RectTransform>("preview/weaponprogresswhite/yes").gameObject;

        m_levelLeft = GetComponent<Text>("preview/damagenumberbackground/left/level/left_txt");
        m_levelRight = GetComponent<Text>("preview/damagenumberbackground/right/level");
        m_hurtTxt = GetComponent<Text>("preview/damagenumberbackground/left/hurt");
        m_hurtLeft = GetComponent<Text>("preview/damagenumberbackground/left/hurt/left_txt");
        m_hurtRight = GetComponent<Text>("preview/damagenumberbackground/right/hurt");
        m_hurtGap = GetComponent<Text>("preview/damagenumberbackground/right/hurt/right_txt_02");
        m_uploadProp = GetComponent<Button>("preview/weaponprogresswhite/no/0");
        m_propHave = GetComponent<Text>("preview/weaponprogresswhite/no/0/numberdi/fenzi");
        m_propNeed = GetComponent<Text>("preview/weaponprogresswhite/no/0/numberdi/fenmu");
        m_uploadBtn = GetComponent<Button>("UPload_btn");
        m_ipLoadText = GetComponent<Text>("UPload_btn/insoul");

        m_uploadProp.onClick.AddListener(SetJumpInfo);
        m_uploadBtn.onClick.AddListener(delegate
        {
            moduleForging.SendUpLevel();
        });

        Center_plane = GetComponent<Button>("center");
        Succedtip = GetComponent<RectTransform>("center/tipsignsccced").gameObject;
        WeaponName = GetComponent<Text>("center/tipsignsccced/success/kuang/namenum/name");
        Levelbefore = GetComponent<Text>("center/tipsignsccced/success/kuang/dengjishangxian/levelbefore");
        levelNow = GetComponent<Text>("center/tipsignsccced/success/kuang/dengjishangxian/levelafter");
        attackbefore = GetComponent<Text>("center/tipsignsccced/success/kuang/gongjilichange/gongjilibefore");
        acctacknow = GetComponent<Text>("center/tipsignsccced/success/kuang/gongjilichange/gongjiliafter");
        wupinbefore = GetComponent<RectTransform>("center/tipsignsccced/success/kuang/wupin/zb1").gameObject;
        wupinnow = GetComponent<RectTransform>("center/tipsignsccced/success/kuang/wupin/zb2").gameObject;

        Center_plane.onClick.AddListener(delegate
        {
            Center_plane.gameObject.SetActive(false);
            Succedtip.gameObject.SetActive(false);
        });

        m_weaponIcon = GetComponent<Image>("info/icon");
        m_strengLevel = GetComponent<Text>("info/name/level");
        m_weaponName = GetComponent<Text>("info/name");

        m_upLeftTxt = GetComponent<Text>("center/tipsignsccced/success/kuang/gongjilichange/gongjilibefore/gongjilitext");
        m_upRightTxt = GetComponent<Text>("center/tipsignsccced/success/kuang/gongjilichange/gongjiliafter/gongjilitext");
        SetText();
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        m_item = moduleForging.UpLoadItem;
        moduleGlobal.ShowGlobalLayerDefault();
        if (m_item == null) return;
        weaponAttr = ConfigManager.Get<WeaponAttribute>(m_item.itemTypeId);
        if (weaponAttr == null || weaponAttr.quality == null)
        {
            Logger.LogError("weaponAttribute not find this id{0}", m_item.itemTypeId);
            return;
        }
        if (weaponAttr.quality.Length <= 0 || weaponAttr.quality[0].attributes.Length <= 0)
        {
            Logger.LogError("id{0} weaponAttribute quality or this.attributes length = 0",m_item.itemTypeId);
            return;
        }
        wType = weaponAttr.quality[0].attributes[0].type;

        SetBaseIndo(m_item);
        SetNewUpLoad(m_item);
    }

    private void SetText()
    {
        Util.SetText(GetComponent<Text>("center/tipsignsccced/success/signsucced/up_h/up"), 224, 52);
        Util.SetText(GetComponent<Text>("center/tipsignsccced/success/signsucced/up_h/up (1)"), 224, 53);
        Util.SetText(GetComponent<Text>("center/tipsignsccced/success/signsucced/up_h/up (2)"), 224, 54);
        Util.SetText(GetComponent<Text>("center/tipsignsccced/success/signsucced/up_h/up (3)"), 224, 55);
        Util.SetText(GetComponent<Text>("evolutionbtn/Text"), ConfigText.GetDefalutString(224, 5));
        Util.SetText(GetComponent<Text>("preview/damagenumberbackground/left/level"), ConfigText.GetDefalutString(224, 34));
        Util.SetText(GetComponent<Text>("preview/title01"), ConfigText.GetDefalutString(224, 37));
        Util.SetText(GetComponent<Text>("preview/title02"), ConfigText.GetDefalutString(224, 36));
    }

    private void SetBaseIndo(PItem info)
    {
        PropItemInfo prop = ConfigManager.Get<PropItemInfo>(info.itemTypeId);
        int index = Module_Forging.instance.GetWeaponIndex(prop);
        if (index != -1) AtlasHelper.SetShared(m_weaponIcon.gameObject, m_weaponType[index]);

        m_strengLevel.text = "+" + info.growAttr.equipAttr.strength.ToString();
        m_weaponName.text = prop.itemName;

    }

    private void SetNewUpLoad(PItem item)
    {
        PropItemInfo debrInfos = ConfigManager.Get<PropItemInfo>(weaponAttr.debrisId);// 武器碎片信息
        PropItemInfo weaponInfos = ConfigManager.Get<PropItemInfo>(item.itemTypeId);// 武器信息
        if (debrInfos == null || weaponInfos == null) return;

        int lv = item.growAttr.equipAttr.star;
        if (lv < 1)
        {
            Logger.LogError("server weapon upload level is error {0},id {1}", lv, item.itemTypeId);
            return;
        }
        var attr = weaponAttr.quality[lv - 1];
        if (attr.attributes.Length < 0)
        {
            Logger.LogError("have no attr ");
            return;
        }

        Util.SetItemInfo(m_itemLeft, weaponInfos, 0, 0, false);
        var str = ((ConsumePercentSubType)attr.quality).ToString() + ConfigText.GetDefalutString(224, 23);
        Util.SetText(m_levelLeft, str);
        Util.SetText(m_hurtTxt,299, attr.attributes[0].id);

        float nowhurt = attr.attributes[0].value;
        SetValue(m_hurtLeft, nowhurt);

        //设置是否满级
        m_levelNormal.SetActive(false);
        m_levelMax.SetActive(false);
        m_rightObj.gameObject.SetActive(false);
        m_itemRight.gameObject.SetActive(false);
        m_uploadBtn.interactable = false;
        m_leftObj.anchoredPosition = new Vector3(-130, -90, 0);

        if (lv < weaponAttr.quality.Length)
        {
            m_rightObj.gameObject.SetActive(true);
            m_itemRight.SetActive(true);
            m_levelNormal.SetActive(true);

            Util.SetItemInfo(m_itemRight, weaponInfos, 0, 0, false);
            var str1 = ((ConsumePercentSubType)weaponAttr.quality[lv].quality).ToString() + ConfigText.GetDefalutString(224, 23);
            Util.SetText(m_levelRight, str1);

            if (weaponAttr.quality[lv].attributes.Length < 0)
            {
                Logger.LogError("have no attr ");
                return;
            }
            float nexthurt = weaponAttr.quality[lv].attributes[0].value;
            SetValue(m_hurtRight, nexthurt);
            float lerp = nexthurt - nowhurt;
            if (wType == 1) Util.SetText(m_hurtGap, "[+{0}]", lerp);
            else if (wType == 2)
            {
                lerp = lerp * 100;
                Util.SetText(m_hurtGap, ConfigText.GetDefalutString(224, 42), lerp);
            }

            Util.SetItemInfo(m_uploadProp.gameObject, debrInfos, 0, 0, false);
            int suipian = moduleEquip.GetPropCount(weaponAttr.debrisId);
            Util.SetText(m_propHave, GeneralConfigInfo.GetNoEnoughColorString(suipian.ToString()));
            m_propNeed.text = attr.debrisNum.ToString();

            if (suipian >= attr.debrisNum)
            {
                m_propHave.text = suipian.ToString();
                m_propHave.color = GeneralConfigInfo.defaultConfig.PropEnough;
                m_uploadBtn.interactable = true;
            }
            Util.SetText(m_ipLoadText, ConfigText.GetDefalutString(224, 5));
        }
        else
        {
            m_leftObj.anchoredPosition = new Vector3(0, -90, 0);
            m_levelMax.SetActive(true);
            Util.SetText(m_ipLoadText, ConfigText.GetDefalutString(224, 31));
        }
    }

    void _ME(ModuleEvent<Module_Forging> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Forging.EventForingUpLoad:
                int random1 = Random.Range(0, 2);
                string playaudio1 = audioup[random1];
                AudioManager.PlayVoice(playaudio1);
                SuccedTipShow(m_item);
                SetNewUpLoad(m_item);
                break;
        }
    }

    private void SuccedTipShow(PItem info)
    {
        var id = moduleEquip.ChangeToItemTypeID(info.itemId);
        var level = info.growAttr.equipAttr.star;

        Center_plane.gameObject.SetActive(true);
        Succedtip.gameObject.SetActive(true);
        PropItemInfo dWinfo = ConfigManager.Get<PropItemInfo>(id);

        var before = weaponAttr.quality[level - 2];
        var after = weaponAttr.quality[level - 1];

        Util.SetItemInfo(wupinbefore.gameObject, dWinfo, 0, 0, false);
        Util.SetItemInfo(wupinnow.gameObject, dWinfo, 0, 0, false);
        Util.SetText(m_upLeftTxt, 299, before.attributes[0].id);
        Util.SetText(m_upRightTxt, 299, after.attributes[0].id);
        WeaponName.text = dWinfo.itemName;

        if (weaponAttr.quality[level - 2] == null || weaponAttr.quality[level - 1] == null)
        {
            Logger.LogError("server weapon upload level is error {0},id {1}", level, id);
            return;
        }
        var str1 = string.Format(ConfigText.GetDefalutString(224, 51), ((ConsumePercentSubType)before.quality));
        Util.SetText(Levelbefore, str1);
        var now = string.Format(ConfigText.GetDefalutString(224, 51), ((ConsumePercentSubType)after.quality));
        Util.SetText(levelNow, now);

        //只改变了攻击力 元素伤害是不会跟随武器等级改变的
        if (before.attributes.Length <0|| after.attributes.Length <0)
        {
            Logger.LogError("have no attr ");
            return;
        }
        SetValue(attackbefore, before.attributes[0].value);
        SetValue(acctacknow, after.attributes[0].value);
    }
    private void SetValue(Text txt, float num)
    {
        if (wType == 1) Util.SetText(txt, num.ToString());
        else if (wType == 2)
        {
            num = num * 100;
            Util.SetText(txt, "{0}%", num);
        }
    }

    private void SetJumpInfo()
    {
        WeaponAttribute DeInfos = ConfigManager.Get<WeaponAttribute>(m_item.itemTypeId);
        if (DeInfos == null) return;
        PropItemInfo debrInfos = ConfigManager.Get<PropItemInfo>(DeInfos.debrisId);// 武器碎片信息
        if (debrInfos == null) return;
        var need = 0;
        if (m_item.growAttr.equipAttr.star - 1 >= DeInfos.quality.Length || m_item.growAttr.equipAttr.star == 0)
        {
            Logger.LogDetail("this star is error");
            return;
        }
        if (DeInfos.quality[m_item.growAttr.equipAttr.star - 1] != null)
        {
            need = DeInfos.quality[m_item.growAttr.equipAttr.star - 1].debrisNum;
        }
        moduleGlobal.SetTargetMatrial(debrInfos.ID, need);
    }
}
