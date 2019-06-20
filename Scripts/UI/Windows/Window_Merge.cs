/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-10-18
 * 
 ***************************************************************************************************/

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Merge : Window
{
    private string[] m_weaponType = new string[7] { "career_icon_01", "career_icon_02", "career_icon_06", "career_icon_04", "career_icon_05", "ui_equip_02", "ui_equip_04" };

    private GameObject m_eleMaxYes;

    private Text elementLevel;//元素等级
    private Text elementNum;//元素伤害
    private Text nextElementLevel;//下一次元素等级
    private Text nextElementNum;//下一次元素伤害
    private Text nextNumEqal;//相差多少

    private Text margeLevelnum;//升级获得元素伤害

    private GameObject elementicongroup;//上面的 小图
    private GameObject m_fillelement;
    private Image m_fill;

    private Text elementFenzi;
    private Text elementFenmu;
    private GameObject Tipcontent;
    private GameObject tipobj;//上浮提示 
    private Image elementdebris;//元素图
    private Text gold_num;//要消耗的金币数
    private Text elementdebris_num;//要消耗的元素数目
    private Button element_btn;//入魂按钮

    private GameObject Expend_plane;
    private GameObject Updata_plane;

    private RectTransform elementMax;
    private GameObject elementNormal;

    private GameObject InsoulUpEffect;
    private GameObject Ins_content;
    private RectTransform m_showEffect;

    private GameObject m_maxTipTxt;

    private Image m_weaponIcon;
    private Text m_weaponName;
    private Text m_strengLevel;

    private PItem item;
    private Button m_eleDropBtn;


    private int m_index = 0;
    private int mes_time = 0;
    private bool mes_isplay = false;

    //跳转的星级和等级
    List<GameObject> Ins_mesback = new List<GameObject>();

    private string[] m_elementData = new string[5] { "ui_forging_wind03", "ui_forging_fire03", "ui_forging_water03", "ui_forging_thunder03", "ui_forging_ice03" };
    private string[] m_elementFill = new string[5] { "ui_forging_wind02", "ui_forging_fire02", "ui_forging_water02", "ui_forging_thunder02", "ui_forging_ice02" };
    private string[] m_elementIcon = new string[5] { "ui_forging_wind", "ui_forging_fire", "ui_forging_water", "ui_forging_thunder", "ui_forging_ice" };

    protected override void OnOpen()
    {
        m_eleDropBtn = GetComponent<Button>("xiaohao/exp/exp_img");

        m_maxTipTxt = GetComponent<RectTransform>("max_Txt").gameObject;
        m_showEffect = GetComponent<RectTransform>("quality/circleProcessBar/element/element01/element02/fill_eff");
        InsoulUpEffect = GetComponent<Transform>("effect_ui_ruhun").gameObject;
        Ins_content = GetComponent<RectTransform>("quality/circleProcessBar/InsoulEffPlane").gameObject;

        elementLevel = GetComponent<Text>("damagenumberbackground/left/level/left_txt");
        elementNum = GetComponent<Text>("damagenumberbackground/left/hurt/left_txt");
        nextElementLevel = GetComponent<Text>("damagenumberbackground/right/level");
        nextElementNum = GetComponent<Text>("damagenumberbackground/right/hurt");
        nextNumEqal = GetComponent<Text>("damagenumberbackground/right/hurt/right_txt_02");

        margeLevelnum = GetComponent<Text>("quality/updata/Text (1)");
        Updata_plane = GetComponent<RectTransform>("quality/updata").gameObject;

        elementicongroup = GetComponent<RectTransform>("quality/updata/element").gameObject;
        m_fillelement = GetComponent<Image>("quality/circleProcessBar/element/element01").gameObject;
        m_fill = GetComponent<Image>("quality/circleProcessBar/element/element01/element02");

        elementMax = GetComponent<RectTransform>("damagenumberbackground/left");
        m_eleMaxYes = GetComponent<RectTransform>("damagenumberbackground/left/Yes").gameObject;
        elementNormal = GetComponent<RectTransform>("damagenumberbackground/right").gameObject;

        elementFenzi = GetComponent<Text>("damagenumberbackground/right/No/fenzi");
        elementFenmu = GetComponent<Text>("damagenumberbackground/right/No/fenmu");
        Tipcontent = GetComponent<RectTransform>("tips_plane").gameObject;
        tipobj = GetComponent<RectTransform>("tips_plane/tip").gameObject;
        tipobj.gameObject.SetActive(false);

        elementdebris = GetComponent<Image>("xiaohao/exp/exp_img");

        gold_num = GetComponent<Text>("xiaohao/comsume/consume_txt");
        elementdebris_num = GetComponent<Text>("xiaohao/exp/total_swallow");
        element_btn = GetComponent<Button>("insoul");
        Expend_plane = GetComponent<RectTransform>("xiaohao").gameObject;
        element_btn.onClick.AddListener(InsoulBtn);

        m_weaponIcon = GetComponent<Image>("info/icon");
        m_strengLevel = GetComponent<Text>("info/name/level");
        m_weaponName = GetComponent<Text>("info/name");

        SetText();
    }

    private void SetText()
    {
        Util.SetText(GetComponent<Text>("max_Txt"), ConfigText.GetDefalutString(224, 44));
        Util.SetText(GetComponent<Text>("damagenumberbackground/left/level"), ConfigText.GetDefalutString(224, 37));
        Util.SetText(GetComponent<Text>("damagenumberbackground/left/hurt"), ConfigText.GetDefalutString(224, 7));
        Util.SetText(GetComponent<Text>("xiaohao/comsume/consume_txt_02"), ConfigText.GetDefalutString(224, 38));
        Util.SetText(GetComponent<Text>("xiaohao/exp/total_tip"), ConfigText.GetDefalutString(224, 39));
        Util.SetText(GetComponent<Text>("damagenumberbackground/damagenumber"), ConfigText.GetDefalutString(224, 6));
        Util.SetText(GetComponent<Text>("damagenumberbackground/elementdamagenumber"), ConfigText.GetDefalutString(224, 7));
        Util.SetText(GetComponent<Text>("quality/updata/Text"), ConfigText.GetDefalutString(224, 8));
        Util.SetText(GetComponent<Text>("expend/thistime"), ConfigText.GetDefalutString(224, 9));
        Util.SetText(GetComponent<Text>("expend/two/num/remaingoldnumber/remaintext"), ConfigText.GetDefalutString(224, 10));
        Util.SetText(GetComponent<Text>("expend/two/num/remaingoldnumber/renmainnumber/ge"), ConfigText.GetDefalutString(224, 11));
        Util.SetText(GetComponent<Text>("expend/num_one/remainlingshinumber/renmainnumber/ge"), ConfigText.GetDefalutString(224, 12));
        Util.SetText(GetComponent<Text>("insoul/insoul"), ConfigText.GetDefalutString(224, 13));
        Util.SetText(GetComponent<Text>("damagenumberbackground/left/Yes/manzu"), ConfigText.GetDefalutString(224, 31));
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();//top工具条的显示
        item = moduleForging.InsoulItem;
        moduleForging.GetInSoulStone(item);
        moduleForging.InsouTimes = 0;//合魂次数
        if (item == null) return;
        moduleForging.InsoulGold = (int)modulePlayer.roleInfo.coin;//每次赋值进来
        SetInsoulInfo();
        SetBaseInfo();

        m_eleDropBtn.onClick.RemoveAllListeners();
        m_eleDropBtn.onClick.AddListener(SetJumpDrop);
    }

    private void SetJumpDrop()
    {
        WeaponAttribute dInfo = ConfigManager.Get<WeaponAttribute>(item.itemTypeId);
        if (dInfo == null) return;
        var level = item.growAttr.equipAttr.level;
        var need = 0;
        if (moduleForging.Insoul_Info[level] != null) need = moduleForging.Insoul_Info[level].lingshi;
        moduleGlobal.SetTargetMatrial(dInfo.elementId, need, null, true, null, moduleForging.InsoulStone);
    }

    private void SetBaseInfo()
    {
        PropItemInfo prop = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
        int index = Module_Forging.instance.GetWeaponIndex(prop);
        if (index != -1) AtlasHelper.SetShared(m_weaponIcon.gameObject, m_weaponType[index]);

        m_strengLevel.text = "+" + item.growAttr.equipAttr.strength.ToString();
        m_weaponName.text = prop.itemName;

    }

    private void SetInsoulInfo()//设置入魂的信息
    {
        //武器的id
        if (item == null) return;
        WeaponAttribute dInfo = ConfigManager.Get<WeaponAttribute>(item.itemTypeId);
        if (dInfo == null) return;

        //元素小图
        AtlasHelper.SetShared(elementicongroup, m_elementData[(int)(dInfo.elementType) - 1]);
        AtlasHelper.SetShared(m_fill.gameObject, m_elementFill[(int)(dInfo.elementType) - 1]);
        AtlasHelper.SetShared(m_fillelement, m_elementIcon[(int)(dInfo.elementType) - 1]);

        PropItemInfo elementInfo = ConfigManager.Get<PropItemInfo>(dInfo.elementId);
        AtlasHelper.SetItemIcon(elementdebris, elementInfo);

        m_eleMaxYes.gameObject.SetActive(false);
        elementMax.anchoredPosition = new Vector3(0, 30, 0);
        ElementNext();
        Element_up();
    }

    private void ElementNext()
    {
        int nowLevel = item.growAttr.equipAttr.level;
        elementNum.text = moduleForging.Insoul_Info[nowLevel].attribute.ToString();
        var str = nowLevel.ToString() + ConfigText.GetDefalutString(224, 23);
        Util.SetText(elementLevel, str);

        if (nowLevel < moduleForging.Insoul_Info.Count - 1)
        {
            elementNormal.gameObject.SetActive(true);
            elementMax.gameObject.SetActive(true);

            nextElementNum.text = moduleForging.Insoul_Info[nowLevel + 1].attribute.ToString();
            var str2 = (nowLevel + 1).ToString() + ConfigText.GetDefalutString(224, 23);
            Util.SetText(nextElementLevel, str2);

            var hurt = Util.Parse<int>(nextElementNum.text) - Util.Parse<int>(elementNum.text);
            Util.SetText(nextNumEqal, ConfigText.GetDefalutString(224, 41), hurt.ToString());
        }
        else
        {
            elementMax.anchoredPosition = new Vector3(120, 30, 0);
            m_eleMaxYes.gameObject.SetActive(true);
            elementNormal.gameObject.SetActive(false);
            elementMax.gameObject.SetActive(false);
        }

    }

    private void Element_up()//这里的 level是合魂的等级而不是武器当前的等级
    {
        var level = item.growAttr.equipAttr.level;
        var exp = item.growAttr.equipAttr.expr;

        WeaponAttribute dInfo = ConfigManager.Get<WeaponAttribute>(item.itemTypeId);
        if (dInfo == null) return;

        if (moduleForging.Insoul_Info[level]==null)
        {
            Logger.LogError("weapon marge level is error{0}", level);
            return;
        }
        var str = moduleForging.Insoul_Info[level].lingshi.ToString() + string.Format(ConfigText.GetDefalutString(224, 50), moduleForging.InsoulStone);
        Util.SetText(elementdebris_num, str);

        gold_num.text = moduleForging.Insoul_Info[level].gold.ToString();

        if (level < moduleForging.Insoul_Info.Count - 1)
        {
            elementNormal.gameObject.SetActive(true);
            elementFenzi.text = exp.ToString();
            elementFenmu.text = moduleForging.Insoul_Info[level].exp.ToString();
            Expend_plane.gameObject.SetActive(true);
            m_maxTipTxt.gameObject.SetActive(false);
            Updata_plane.gameObject.SetActive(true);

            int now = moduleForging.Insoul_Info[level].attribute;
            int next = moduleForging.Insoul_Info[level + 1].attribute;

            Util.SetText(margeLevelnum, ConfigText.GetDefalutString(224, 40), (next - now).ToString());//属性值

            if (Util.Parse<int>(elementFenmu.text) != 0)
            {
                m_fill.fillAmount = (float)Util.Parse<int>(elementFenzi.text) / (float)Util.Parse<int>(elementFenmu.text);
            }
        }
        else
        {
            elementNormal.gameObject.SetActive(false);
            elementMax.anchoredPosition = new Vector3(120, 30, 0);
            Updata_plane.gameObject.SetActive(false);
            Expend_plane.gameObject.SetActive(false);
            m_maxTipTxt.gameObject.SetActive(true);
            elementMax.gameObject.SetActive(true);
            m_eleMaxYes.gameObject.SetActive(true);
            m_fill.fillAmount = 1;
        }
        m_showEffect.anchoredPosition = new Vector3(0, m_fill.fillAmount * 205, 0);
        SetColor();


    }
    private void SetColor()
    {
        elementdebris_num.color = Color.white;
        gold_num.color = Color.white;
        var level = item.growAttr.equipAttr.level;

        var need = 0;
        if (moduleForging.Insoul_Info[level] != null) need = moduleForging.Insoul_Info[level].lingshi;

        if (moduleForging.InsoulGold < Util.Parse<int>(gold_num.text))
            gold_num.color = Color.red;
        if (moduleForging.InsoulStone < need)
            elementdebris_num.color = Color.red;
    }

    #region tip

    private void InsoulBtn()
    {
        if (item == null) return;

        WeaponAttribute eleminfo = ConfigManager.Get<WeaponAttribute>(item.itemTypeId);//查找对应武器的对应灵石id

        var glod = moduleForging.InsoulGold;
        var gNeed = Util.Parse<int>(gold_num.text);
        var level = item.growAttr.equipAttr.level;
        var cNeed = moduleForging.Insoul_Info[level].lingshi;

        if (glod < gNeed)
        {
            moduleGlobal.ShowMessage(224, 56);
            moduleGlobal.OpenExchangeTip(TipType.BuyGoldTip);
        }
        else if (moduleForging.InsoulStone < cNeed)
        {
            var w = ConfigManager.Get<WeaponAttribute>(item.itemTypeId);
            if (w == null) return;
            var prop = ConfigManager.Get<PropItemInfo>(w.elementId);
            if (prop == null) return;
            var str = string.Format(ConfigText.GetDefalutString(224, 32), prop.itemName);
            moduleGlobal.ShowMessage(str);
        }
        else if (item.growAttr.equipAttr.level >= moduleForging.Insoul_Info.Count - 1)
            moduleGlobal.ShowMessage(224, 45);
        else
        {
            moduleForging.InsoulClick();
            ObjToMax();
        }
    }

    private void InsoulTip(string mesgage)
    {
        if (mes_isplay == false)
        {
            m_index = 0;
            mes_time = 0;
        }
        else if (mes_isplay == true)
        {
            m_index++;
        }
        mes_time++;
        mes_isplay = true;

        if (m_index >= 1 && m_index < 5)
        {
            for (int c = 0; c < m_index; c++)
            {
                if (!DOTween.IsTweening(Ins_mesback[c]))
                {
                    Ins_mesback[c].transform.localPosition = new Vector3(0, 30f * (m_index - c), 0);
                }
            }
        }
        else if (m_index >= 5)
        {
            for (int i = m_index - 5; i < m_index; i++)
            {
                if (!DOTween.IsTweening(Ins_mesback[i]))
                {
                    Ins_mesback[i].transform.localPosition = new Vector3(0, 30f * (m_index - i), 0);
                }
            }
        }
        GameObject objmes = GameObject.Instantiate(tipobj.gameObject);
        objmes.SetActive(true);
        objmes.transform.SetParent(Tipcontent.gameObject.transform, false);
        Util.SetText(objmes.transform.Find("exp").GetComponent<Text>(), mesgage);
        MesTip mestips = objmes.GetComponentDefault<MesTip>();
        mestips.Ins_Mesplay();
        Ins_mesback.Add(objmes);
        StartCoroutine(Ins_Mesplay(m_index));
    }

    IEnumerator Ins_Mesplay(int ms_index)
    {
        yield return new WaitForSeconds(2.0f);
        mes_time--;
        if (mes_time == 0)
        {
            mes_isplay = false;
            for (int i = 0; i <= ms_index; i++)
            {
                GameObject.Destroy(Ins_mesback[i]);
            }
            Ins_mesback.Clear();
        }
    }

    private void ObjToMax()
    {
        //特效达到最大值
        GameObject obj = GameObject.Instantiate(InsoulUpEffect);
        obj.transform.SetParent(Ins_content.transform);
        obj.transform.localPosition = new Vector3(0, 0, 0);
        obj.transform.localScale = new Vector3(1, 1, 1);
        obj.gameObject.SetActive(true);
        AutoDestroy destory = obj.GetComponent<AutoDestroy>();
        destory.delay = 1000;
        destory.enabled = true;

        Queue<GameObject> Inseffobjlist = new Queue<GameObject>();
        foreach (Transform child in Ins_content.transform)
        {
            Inseffobjlist.Enqueue(child.gameObject);
        }
        int count = Inseffobjlist.Count;
        int max = GeneralConfigInfo.defaultConfig.InsoulMax;

        for (int i = 0; i < count - max; i++)
        {
            Inseffobjlist.Dequeue().SetActive(false);
        }
    }

    #endregion


    void _ME(ModuleEvent<Module_Forging> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Forging.EventForingInSoul:
                var exp_num = e.param1.ToString();
                string message = ConfigText.GetDefalutString(224, 19) + exp_num;
                ElementNext();
                InsoulTip(message);
                Element_up();
                break;
            case Module_Forging.EventForingMargeUpAudio:
                AudioManager.PlayVoice(AudioInLogicInfo.audioConst.upInsoulSucc);
                break;
            case Module_Forging.EventForingInsoulAudio:
                AudioManager.PlaySound(AudioInLogicInfo.audioConst.insoulToSucc);
                break;
        }
    }

    void _ME(ModuleEvent<Module_Player> e)
    {
        if (e.moduleEvent == Module_Player.EventCurrencyChanged)
        {
            WeaponAttribute dInfo = ConfigManager.Get<WeaponAttribute>(item.itemTypeId);
            if (dInfo == null) return;

            SetColor();
            moduleForging.GlodChange();
        }
    }

    protected override void OnReturn()
    {
        if (moduleForging.InsouTimes >0) moduleForging.SendMarge(item.itemId);
        moduleForging.InsoulItem = null;
        Hide(true);
    }
}
