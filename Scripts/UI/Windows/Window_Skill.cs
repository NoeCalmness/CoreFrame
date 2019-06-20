/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-06-30
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Skill : Window
{
    private const string TYPENAME = "position_{0:D2}";
    private const string POSITIONNAME = "frame_{0:D2}";

    Toggle[] toggles;
    Button add_sp, upBtn, closeDetailBtn;
    Image[] inputsInMain;
    Image[] inputsInDetail;
    //主界面panel
    Transform fist_panel, sword_panel, katana_panel, axe_panel;
    //详细界面panel
    Transform detail_panel, iconParent, inputDetailParent, consumeTran, maxLvTran, effectUp, effectLock, lvParent, coinParent, spParent, comboNode;
    Text sp_text, skillName, remainPoint, skillDesc, expendSkillPoint, expendCoin, needLvText, upOrLearnText, needLvText01, remainTime, explainText, explainDetail;
    bool isLockUP;
    List<Transform> uptipPoints = new List<Transform>();
    SkillData detaildata;
    TimeSpan time;

    protected override void OnOpen()
    {
        #region mainPanel
        uptipPoints.Clear();
        toggles = GetComponent<ToggleGroup>("checkBox")?.GetComponentsInChildren<Toggle>(true);
        if (toggles != null && toggles.Length > 0)
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                var newimage = toggles[i].transform.Find("new");
                if (newimage) uptipPoints.Add(newimage);
                toggles[i].onValueChanged.RemoveAllListeners();
                toggles[i].onValueChanged.AddListener(OnToggleValueChanged);
            }
        }

        sp_text = GetComponent<Text>("spellpoint/bg/Text");
        add_sp = GetComponent<Button>("spellpoint/add_btn");
        remainTime = GetComponent<Text>("spellpoint/Text");
        add_sp.onClick.RemoveAllListeners();
        add_sp.onClick.AddListener(() => moduleGlobal.OpenExchangeTip(TipType.BuySkillPointTip));

        Transform inputParent = GetComponent<Transform>("template_des");
        if (inputParent) inputParent.gameObject.SetActive(false);
        inputsInMain = inputParent?.GetComponentsInChildren<Image>(true);
        fist_panel = GetComponent<Transform>("fist");
        sword_panel = GetComponent<Transform>("sword");
        katana_panel = GetComponent<Transform>("katana");
        axe_panel = GetComponent<Transform>("axe");
        #endregion

        #region detailPanel
        detail_panel = GetComponent<Transform>("skillDetail");
        detail_panel?.gameObject.SetActive(false);
        closeDetailBtn = GetComponent<Button>("skillDetail/content/close_button");
        closeDetailBtn.onClick.RemoveAllListeners();
        closeDetailBtn.onClick.AddListener(() =>
        {
            moduleGlobal.ShowGlobalLayerDefault();
        });
        skillName = GetComponent<Text>("skillDetail/content/skillname");
        remainPoint = GetComponent<Text>("skillDetail/content/spellpoint/txt_02");
        skillDesc = GetComponent<Text>("skillDetail/content/content_tip");
        consumeTran = GetComponent<Transform>("skillDetail/content/levelUpAvailable");
        maxLvTran = GetComponent<Transform>("skillDetail/content/levelMax");
        lvParent = GetComponent<Transform>("skillDetail/content/levelUpAvailable/consume/consume_03");
        coinParent = GetComponent<Transform>("skillDetail/content/levelUpAvailable/consume/consume_02");
        spParent = GetComponent<Transform>("skillDetail/content/levelUpAvailable/consume/consume_01");
        expendSkillPoint = GetComponent<Text>("skillDetail/content/levelUpAvailable/consume/consume_01/txt_02");
        expendCoin = GetComponent<Text>("skillDetail/content/levelUpAvailable/consume/consume_02/txt_02");
        needLvText = GetComponent<Text>("skillDetail/content/levelUpAvailable/consume/consume_03/txt_02");
        needLvText01 = GetComponent<Text>("skillDetail/content/levelUpAvailable/consume/consume_03/txt_01");
        comboNode= GetComponent<Transform>("skillDetail/content/combolist");
        explainText = GetComponent<Text>("skillDetail/content/combolist/txt");
        explainDetail = GetComponent<Text>("skillDetail/content/combolist/txt (1)");

        Transform _inputParent = GetComponent<Transform>("skillDetail/content/combolist/template_des");
        if (_inputParent) _inputParent.gameObject.SetActive(false);
        inputsInDetail = _inputParent?.GetComponentsInChildren<Image>(true);
        inputDetailParent = GetComponent<Transform>("skillDetail/content/combolist/des");
        inputDetailParent.gameObject.SetActive(true);
        iconParent = GetComponent<Transform>("skillDetail/content/skillicon");
        effectUp = GetComponent<Transform>("skillDetail/content/eff_node_upLv");
        effectUp.gameObject.SetActive(false);
        effectLock = GetComponent<Transform>("skillDetail/content/eff_node_Unlock");
        effectLock.gameObject.SetActive(false);
        upBtn = GetComponent<Button>("skillDetail/content/levelUpAvailable/yes_button");
        upOrLearnText = GetComponent<Text>("skillDetail/content/levelUpAvailable/yes_button/duihuan_text");
        #endregion

        InitializeText();
    }

    private void InitializeText()
    {
        var skillTexts = ConfigManager.Get<ConfigText>((int)TextForMatType.SkillUIText);
        Util.SetText(GetComponent<Text>("title/title_01"), skillTexts[0]);
        Util.SetText(GetComponent<Text>("title/title_02"), skillTexts[1]);
        Util.SetText(GetComponent<Text>("title/des_01"), skillTexts[2]);
        Util.SetText(GetComponent<Text>("title/des_01/des_02"), skillTexts[3]);
        Util.SetText(GetComponent<Text>("title/des_01/des_03"), skillTexts[4]);
        Util.SetText(GetComponent<Text>("spellpoint/txt"), skillTexts[5]);
        Util.SetText(GetComponent<Text>("checkBox/1/Text"), skillTexts[6]);
        Util.SetText(GetComponent<Text>("checkBox/1/xz/richang_text"), skillTexts[6]);
        Util.SetText(GetComponent<Text>("checkBox/2/Text"), skillTexts[7]);
        Util.SetText(GetComponent<Text>("checkBox/2/xz/qiangxie_text"), skillTexts[7]);
        Util.SetText(GetComponent<Text>("checkBox/3/Text"), skillTexts[8]);
        Util.SetText(GetComponent<Text>("checkBox/3/xz/zahuo_text"), skillTexts[8]);
        Util.SetText(GetComponent<Text>("checkBox/4/Text"), skillTexts[9]);
        Util.SetText(GetComponent<Text>("checkBox/4/xz/zahuo_text"), skillTexts[9]);
        Util.SetText(GetComponent<Text>("checkBox/5/Text"), skillTexts[10]);
        Util.SetText(GetComponent<Text>("checkBox/5/xz/zahuo_text"), skillTexts[10]);
        Util.SetText(GetComponent<Text>("checkBox/6/Text"), skillTexts[11]);
        Util.SetText(GetComponent<Text>("checkBox/6/xz/zahuo_text"), skillTexts[11]);
        Util.SetText(GetComponent<Text>("checkBox/7/Text"), skillTexts[12]);
        Util.SetText(GetComponent<Text>("checkBox/7/xz/zahuo_text"), skillTexts[12]);
        Util.SetText(GetComponent<Text>("skillDetail/content/equipinfo"), skillTexts[13]);
        Util.SetText(GetComponent<Text>("skillDetail/content/spellpoint/txt_01"), skillTexts[14]);
        Util.SetText(GetComponent<Text>("skillDetail/content/combolist/txt"), skillTexts[15]);
        Util.SetText(GetComponent<Text>("skillDetail/content/levelUpAvailable/consume/consume_01/txt_01"), skillTexts[16]);
        Util.SetText(GetComponent<Text>("skillDetail/content/levelUpAvailable/consume/consume_02/txt_01"), skillTexts[17]);
        Util.SetText(GetComponent<Text>("skillDetail/content/levelMax/txt"), skillTexts[35]);

        Util.SetText(GetComponent<Text>("skill_txt/click/Text"), 9213, 0);
        Util.SetText(GetComponent<Text>("skill_txt/forward/Text"), 9213, 1);
        Util.SetText(GetComponent<Text>("skill_txt/upslide/Text"), 9213, 2);
        Util.SetText(GetComponent<Text>("skill_txt/downslide/Text"), 9213, 3);
        Util.SetText(GetComponent<Text>("skill_txt/rush/Text"), 9213, 4);
        Util.SetText(GetComponent<Text>("skill_txt/power/Text"), 9213, 5);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();

        if (moduleSkill.ReadTrue != true) moduleSkill.ReadTrue = true;

        SelectVocationPanel();
        moduleSkill.UpdateSkillPanel();
    }

    protected override void OnReturn()
    {
        base.OnReturn();
        detaildata = null;
    }

    private void SelectVocationPanel()
    {
        var subType = moduleEquip.weapon?.GetPropItem().subType;
        if (subType != null)
        {
            var type = (WeaponSubType)subType;
            fist_panel.gameObject.SetActive(type == WeaponSubType.Gloves);
            sword_panel.gameObject.SetActive(type == WeaponSubType.LongSword);
            katana_panel.gameObject.SetActive(type == WeaponSubType.Katana);
            axe_panel.gameObject.SetActive(type == WeaponSubType.GiantAxe);
        }
    }

    private void RefreshRedPoint()
    {
        for (int i = 0; i < toggles.Length; i++)
        {
            bool isShow = moduleSkill.GetCurrentTypeState((SkillType)i + 1);
            uptipPoints[i].gameObject.SetActive(isShow);
        }
    }

    private void OnToggleValueChanged(bool arg0)
    {
        if (!arg0) return;

        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i].isOn)
            {
                moduleSkill.currentSkillType = (SkillType)i + 1;

                if (!moduleSkill.isRead.ContainsKey(moduleSkill.currentSkillType)) moduleSkill.isRead.Add(moduleSkill.currentSkillType, true);
                else moduleSkill.isRead[moduleSkill.currentSkillType] = true;

                if (!moduleSkill.skillsDic.ContainsKey(moduleSkill.currentSkillType)) return;
                var currentList = moduleSkill.skillsDic[moduleSkill.currentSkillType];

                var position = OnSelectPositionParent(i + 1);
                if (!position) return;

                RefreshItem(position, currentList, moduleSkill.currentSkillType);
            }
        }

        RefreshRedPoint();
    }

    private Transform OnSelectPositionParent(int toggleId)
    {
        var subType = moduleEquip.weapon?.GetPropItem().subType;
        if (subType != null)
        {
            var panel = GetCurrentTranParent((WeaponSubType)subType);
            if (!panel) return null;
            string _name = Util.Format(TYPENAME, toggleId);
            var position = panel.Find(_name);
            return position;
        }
        return null;
    }

    private Transform GetCurrentTranParent(WeaponSubType type)
    {
        switch (type)
        {
            case WeaponSubType.LongSword: return sword_panel;
            case WeaponSubType.Katana: return katana_panel;
            case WeaponSubType.Spear: return null;
            case WeaponSubType.GiantAxe: return axe_panel;
            case WeaponSubType.Gloves: return fist_panel;
            default: return null;
        }
    }

    private void RefreshItem(Transform positionParent, List<SkillData> currentTypeList, SkillType type)
    {
        if (currentTypeList == null || currentTypeList.Count < 1) return;

        for (int k = 0; k < currentTypeList.Count; k++)
        {
            string _positionName = Util.Format(POSITIONNAME, currentTypeList[k].skillInfo.skillPosition);
            Transform positionFrame = positionParent.Find(_positionName);
            if (!positionFrame) continue;

            var haveName = currentTypeList[k].skillInfo.skillFrame + "(Clone)";
            var skill = positionFrame.Find(haveName);
            if (skill == null) skill = positionFrame.AddNewChild(GetComponent<Transform>(currentTypeList[k].skillInfo.skillFrame));

            if (skill == null) continue;
            Refresh_skillItem(currentTypeList[k], positionFrame, skill, type);
        }
    }

    private void Refresh_skillItem(SkillData info, Transform position, Transform item, SkillType type)
    {
        if (info == null || position == null || item == null) return;

        BaseRestrain.SetRestrainData(item.gameObject, info.pskill.skillId, info.skillInfo.protoId);
        int currentLv = info.pskill.level;
        bool isLockNow = moduleSkill.GetIsCurrentLock(info);
        currentLv = isLockNow ? 1 : currentLv;

        UpSkillInfo _info = moduleSkill.skillLevelInfo.Find(p => p.skillId == info.pskill.skillId && p.skillLevel == currentLv);
        UpSkillInfo _nextInfo = moduleSkill.skillLevelInfo.Find(p => p.skillId == info.pskill.skillId && p.skillLevel == currentLv + 1);
        if (_info == null) return;
        //icon相关
        Util.DisableAllChildren(position, item.name);
        SetIconItem(info, item, _info, _nextInfo, isLockNow);

        //desc相关
        var textParent = position.Find("txt_frame");
        textParent.SafeSetActive(type == SkillType.Special);
        var text = textParent?.GetComponent<Text>("Text");

        if (type == SkillType.Special)
        {
            double[] damages = moduleSkill.GetCurrentSkillDamge(_info.skillId, _info.skillLevel);

            //弱点打击要记两种伤害,普通攻击和弱点打击伤害,复活不显示百分数
            string _str    = info.pskill.skillId == moduleSkill.reliveSkillId ? damages[0].ToString() : damages[0].ToString("P2");
            string _spcail = info.pskill.skillId == moduleSkill.reliveSkillId ? damages[1].ToString() : damages[1].ToString("P2");
            if (isLockNow)
            {
                int index = info.skillInfo.skillDesc.Length >= 2 ? info.skillInfo.skillDesc[1] : info.skillInfo.skillDesc[0];
                if (damages[1] != 0) Util.SetText(text, index, _str, _spcail);
                else Util.SetText(text, index, _str);
            }
            else
            {
                ConfigText config = ConfigManager.Get<ConfigText>(info.skillInfo.skillDesc[0]);
                bool isContains = config != null && config.text != null && config.text.Length > 0 ? config.text[0].Contains("-") : false;

                if (isContains)
                {
                    string[] strs = config.text[0].Split('-');
                    if (strs.Length == 2)
                    {
                        if (_nextInfo)
                        {
                            double[] _nextDamages = moduleSkill.GetCurrentSkillDamge(_nextInfo.skillId, _nextInfo.skillLevel);

                            string _nextStr    = info.pskill.skillId == moduleSkill.reliveSkillId ? _nextDamages[0].ToString() : _nextDamages[0].ToString("P2");
                            string _nextSpcail = info.pskill.skillId == moduleSkill.reliveSkillId ? _nextDamages[1].ToString() : _nextDamages[1].ToString("P2");

                            var s = strs[0] + strs[1];
                            var length = s.Split('{');
                            if (length != null && length.Length == 5) Util.SetText(text, s, _str, _spcail, _nextStr, _nextSpcail);
                            else if (length != null && length.Length == 3) Util.SetText(text, s, _str, _nextStr);
                        }
                        else
                        {
                            if (strs[0].Contains("{"))
                            {
                                var length = strs[0].Split('{');
                                if (length != null && length.Length == 3) Util.SetText(text, strs[0], _str, _spcail);
                                else if (length != null && length.Length == 2) Util.SetText(text, strs[0], _str);
                            }
                            else Util.SetText(text, strs[0]);
                        }
                    }
                }
            }
        }

        //出招方式
        var _desc = item.Find("des");
        if (!_desc) return;
        if (info.skillInfo.inputs == null || info.skillInfo.inputs.Length == 0 || info.skillInfo.inputs.Contains(0) || isLockNow)
            _desc.gameObject.SetActive(false);
        else
            SetInputKeys(info, _desc, inputsInMain);

        //btn_Event
        var btn = item.GetComponentDefault<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => OnClickSkillItem(info));
    }

    private void OnClickSkillItem(SkillData info)
    {
        if (info==null) return;
        Logger.LogDetail("current skill id={0}", info.skillInfo.ID);
        if (modulePlayer.roleInfo.level < info.skillInfo.unLockLv)
        {
            moduleGlobal.ShowMessage(Util.Format(ConfigText.GetDefalutString(TextForMatType.SkillUIText, 33), info.skillInfo.unLockLv));
            return;
        }
        detail_panel.gameObject.SetActive(true);
        effectUp.gameObject.SetActive(false);
        effectLock.gameObject.SetActive(false);
        RefreshSkillDetailPanel(info);
    }

    private void RefreshSkillDetailPanel(SkillData info)
    {
        if (info == null || info.pskill == null || moduleSkill.skillInfo == null) return;
        int currentLv = info.pskill.level;
        bool isLockNow = moduleSkill.GetIsCurrentLock(info);
        currentLv = isLockNow ? 1 : currentLv;

        UpSkillInfo _info = moduleSkill.skillLevelInfo.Find(p => p.skillId == info.pskill.skillId && p.skillLevel == currentLv);
        UpSkillInfo _nextInfo = moduleSkill.skillLevelInfo.Find(p => p.skillId == info.pskill.skillId && p.skillLevel == currentLv + 1);
        if (!_info) return;
        detaildata = info;

        #region desc相关
        skillName.text = info.skillInfo._name;
        Util.SetText(remainPoint, (int)TextForMatType.SkillUIText, 30, moduleSkill.skillInfo.skillPoint);
        double[] damages = moduleSkill.GetCurrentSkillDamge(_info.skillId, _info.skillLevel);

        string _str    = info.pskill.skillId == moduleSkill.reliveSkillId ? damages[0].ToString() : damages[0].ToString("P2");
        string _spcail = info.pskill.skillId == moduleSkill.reliveSkillId ? damages[1].ToString() : damages[1].ToString("P2");
        if (isLockNow)
        {
            if (info.skillInfo.skillType == (int)SkillType.Single)
            {
                double s = _info.attributes != null && _info.attributes.Length > 0 && _info.attributes[0] != null ? _info.attributes[0].value : 0;
                Util.SetText(skillDesc, info.skillInfo.skillDesc.Length >= 2 ? info.skillInfo.skillDesc[1] : info.skillInfo.skillDesc[0], s);
            }
            else
            {
                int index = info.skillInfo.skillDesc.Length >= 2 ? info.skillInfo.skillDesc[1] : info.skillInfo.skillDesc[0];
                if (damages[1] != 0) Util.SetText(skillDesc, index, _str, _spcail);
                else Util.SetText(skillDesc, index, _str);
            }

            SetExpsendText(_info);
        }
        else
        {
            ConfigText config = ConfigManager.Get<ConfigText>(info.skillInfo.skillDesc[0]);
            if (GeneralConfigInfo.defaultConfig.percentAttriIds == null) return;

            bool isContains = config.text[0].Contains("-");

            if (isContains)
            {
                string[] strs = config.text[0].Split('-');
                if (strs.Length == 2)
                {
                    if (_nextInfo)
                    {
                        if (info.skillInfo.skillType == (int)SkillType.Single)
                        {
                            string infoValue = "0";
                            string nextValue = "0";
                            if (_info.attributes != null && _info.attributes.Length > 0 && _info.attributes[0] != null)
                            {
                                double _value = _info.attributes[0].value;
                                infoValue = GeneralConfigInfo.defaultConfig.percentAttriIds.Contains(_info.attributes[0].id) ? _value.ToString("p2") : _value.ToString();
                            }
                            if (_nextInfo.attributes != null && _nextInfo.attributes.Length > 0 && _nextInfo.attributes[0] != null)
                            {
                                double _nextValue = _nextInfo.attributes[0].value;
                                nextValue = GeneralConfigInfo.defaultConfig.percentAttriIds.Contains(_nextInfo.attributes[0].id) ? _nextValue.ToString("p2") : _nextValue.ToString();
                            }
                            Util.SetText(skillDesc, strs[0] + strs[1], infoValue, nextValue);
                        }
                        else
                        {
                            double[] _nextDamages = moduleSkill.GetCurrentSkillDamge(_nextInfo.skillId, _nextInfo.skillLevel);

                            string _nextStr    = info.pskill.skillId == moduleSkill.reliveSkillId ? _nextDamages[0].ToString() : _nextDamages[0].ToString("P2");
                            string _nextSpcail = info.pskill.skillId == moduleSkill.reliveSkillId ? _nextDamages[1].ToString() : _nextDamages[1].ToString("P2");

                            string format = strs[0] + strs[1];
                            var length = format.Split('{');
                            if (length != null && length.Length == 5) Util.SetText(skillDesc, format, _str, _spcail, _nextStr, _nextSpcail);
                            else if (length != null && length.Length == 3) Util.SetText(skillDesc, format, _str, _nextStr);
                        }

                        SetExpsendText(_nextInfo);
                    }
                    else
                    {
                        if (info.skillInfo.skillType == (int)SkillType.Single)
                        {
                            string infoValue = "0";
                            if (_info.attributes != null && _info.attributes.Length > 0 && _info.attributes[0] != null)
                            {
                                double _infoValue = _info.attributes[0].value;
                                if (GeneralConfigInfo.defaultConfig.percentAttriIds.Contains(_info.attributes[0].id))
                                    infoValue = _infoValue.ToString("P2");
                                else infoValue = _infoValue.ToString();
                            }
                            if (strs[0].Contains("{")) Util.SetText(skillDesc, strs[0], infoValue);
                            else Util.SetText(skillDesc, strs[0]);
                        }
                        else
                        {
                            if (strs[0].Contains("{"))
                            {
                                var length = strs[0].Split('{');
                                if (length != null && length.Length == 3) Util.SetText(skillDesc, strs[0], _str, _spcail);
                                else if (length != null && length.Length == 2) Util.SetText(skillDesc, strs[0], _str);
                            }
                            else Util.SetText(skillDesc, strs[0]);
                        }
                    }
                }
            }
        }

        maxLvTran.gameObject.SetActive(_nextInfo == null);
        consumeTran.gameObject.SetActive(_nextInfo != null);
        #endregion

        #region icon相关
        string haveName = info.skillInfo.skillFrame + "(Clone)";
        Util.DisableAllChildren(iconParent, haveName);
        Transform item = iconParent.Find(haveName);
        if (!item)
        {
            Transform template = GetComponent<Transform>(info.skillInfo.skillFrame);
            if (!template) return;
            item = iconParent.AddNewChild(template);
        }
        Transform des = item.Find("des");
        if (des) des.gameObject.SetActive(false);
        SetIconItem(info, item, _info, _nextInfo, isLockNow);
        if (info.skillInfo.inputs == null || info.skillInfo.inputs.Length == 0 || info.skillInfo.inputs.Contains(0) || isLockNow)
        {
            Util.DisableAllChildren(inputDetailParent);
            if (info.skillInfo.skillDesc.Length == 3)
            {
                explainDetail.SafeSetActive(true);
                Util.SetText(explainText, (int)TextForMatType.SkillUIText, 39);
                Util.SetText(explainDetail, info.skillInfo.skillDesc[2]);
            }
            else
            {
                explainDetail.SafeSetActive(false);
                Util.SetText(explainText, (int)TextForMatType.SkillUIText, 15);
            }
        }
        else
        {
            Util.SetText(explainText, (int)TextForMatType.SkillUIText, 15);
            explainDetail.SafeSetActive(false);
            SetInputKeys(info, inputDetailParent, inputsInDetail);
        }
        #endregion

        #region btn相关
        Util.SetText(upOrLearnText, (int)TextForMatType.SkillUIText, isLockNow ? 32 : 18);
        if (isLockNow) upBtn.interactable = _info && modulePlayer.roleInfo.level >= _info.needLv && modulePlayer.roleInfo.level >= info.skillInfo.unLockLv;
        else upBtn.interactable = _nextInfo && modulePlayer.roleInfo.level >= _nextInfo.needLv;
        upBtn.onClick.RemoveAllListeners();
        upBtn.onClick.AddListener(() =>
        {
            if (_nextInfo != null)
            {
                int gold = isLockNow ? _info.expendGold : _nextInfo.expendGold;
                if (modulePlayer.roleInfo.coin < gold)
                {
                    moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.SkillUIText, 24));
                    moduleGlobal.OpenExchangeTip(TipType.BuyGoldTip);
                    return;
                }

                if (moduleSkill.skillInfo.skillPoint < _nextInfo.expendSp)
                {
                    moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.SkillUIText, 23));
                    moduleGlobal.OpenExchangeTip(TipType.BuySkillPointTip);
                    return;
                }
            }
            upBtn.interactable = false;
            moduleSkill.currentClickSkill = info;
            isLockUP = isLockNow;
            effectUp.gameObject.SetActive(false);
            effectLock.gameObject.SetActive(false);
            moduleSkill.SendUpSkillLv((ushort)info.pskill.skillId);
        });
        #endregion
    }

    private void SetExpsendText(UpSkillInfo info)
    {
        spParent.gameObject.SetActive(modulePlayer.roleInfo.level >= info.needLv);
        coinParent.gameObject.SetActive(modulePlayer.roleInfo.level >= info.needLv);
        lvParent.gameObject.SetActive(modulePlayer.roleInfo.level < info.needLv);

        if (modulePlayer.roleInfo.level < info.needLv)
        {
            string lv = Util.Format(ConfigText.GetDefalutString(TextForMatType.SkillUIText, 36), info.needLv);
            Util.SetText(needLvText, modulePlayer.roleInfo.level >= info.needLv ? lv : GeneralConfigInfo.GetNoEnoughColorString(lv));
            var needStr = Util.GetString((int)TextForMatType.SkillUIText, 34);
            Util.SetText(needLvText01, GeneralConfigInfo.GetNoEnoughColorString(needStr));
        }
        else
        {
            if (moduleSkill.skillInfo == null) return;
            string str = Util.Format(ConfigText.GetDefalutString(TextForMatType.SkillUIText, 30), info.expendSp);
            Util.SetText(expendSkillPoint, moduleSkill.skillInfo.skillPoint >= info.expendSp ? str : GeneralConfigInfo.GetNoEnoughColorString(str));
            Util.SetText(expendCoin, modulePlayer.roleInfo.coin >= info.expendGold ? info.expendGold.ToString() : GeneralConfigInfo.GetNoEnoughColorString(info.expendGold.ToString()));
        }
    }

    private void SetInputKeys(SkillData info, Transform parent, Image[] images)
    {
        parent.gameObject.SetActive(true);
        if (parent.childCount > 0)
        {
            List<GameObject> objs = new List<GameObject>();
            for (int i = 0; i < parent.childCount; i++)
                objs.Add(parent.GetChild(i)?.gameObject);

            for (int i = 0; i < parent.childCount; i++)
                GameObject.Destroy(objs[i]);            
        }

        for (int i = 0; i < info.skillInfo.inputs.Length; i++)
        {
            int index = info.skillInfo.inputs[i];
            index = index - 1 >= 0 ? index - 1 : 0;
            var a = images != null && images.Length > i ? images[index] : null;
            if (a)
            {
                var clone_a = GameObject.Instantiate(a)?.transform;
                if (!clone_a) continue;
                clone_a.SetParent(parent);
                clone_a.localPosition = Vector3.zero;
                clone_a.localScale = Vector3.one;
                clone_a.gameObject.SetActive(true);
            }
        }
    }

    private void SetIconItem(SkillData info, Transform skill, UpSkillInfo _info, UpSkillInfo _nextInfo, bool isLockNow)
    {
        if (_info == null || moduleSkill.skillInfo == null) return;
        skill.gameObject.SetActive(true);
        AtlasHelper.SetSkillIcon(skill.Find("icon/img"), info.skillInfo.skillIcon);

        Transform lvParent = skill.Find("level");
        if (lvParent) lvParent.gameObject.SetActive(!isLockNow);
        Transform mask = skill.Find("icon/mask");
        if (mask) mask.gameObject.SetActive(isLockNow);

        Transform newLock = skill.Find("newLock");
        if (newLock) newLock.gameObject.SetActive(isLockNow && modulePlayer.roleInfo.level >= info.skillInfo.unLockLv);

        if (!isLockNow)
        {
            Transform lv_text = skill.Find("level/bg");
            Transform lv_max = skill.Find("level/max");
            if (lv_text && lv_max)
            {
                lv_text.gameObject.SetActive(_nextInfo != null);
                lv_max.gameObject.SetActive(_nextInfo == null);
            }
            Util.SetText(skill.Find("level/bg/txt")?.GetComponent<Text>(), _nextInfo != null ? _info.skillLevel.ToString() : "-1");

            Transform up_image = skill.Find("level/bg/level_up");
            Transform noMoneyImage = skill.Find("level/bg/level_up_02");
            if (!up_image || !noMoneyImage) return;
            if (_nextInfo != null)
            {
                int lv = _nextInfo.needLv;
                int coin = _nextInfo.expendGold;
                int sp = _nextInfo.expendSp;
                up_image.gameObject.SetActive(modulePlayer.roleInfo.level >= lv && modulePlayer.roleInfo.coin >= coin && moduleSkill.skillInfo.skillPoint >= sp);
                noMoneyImage.gameObject.SetActive(modulePlayer.roleInfo.level >= lv && (modulePlayer.roleInfo.coin < coin || moduleSkill.skillInfo.skillPoint < sp));
            }
            else
            {
                up_image.gameObject.SetActive(false);
                noMoneyImage.gameObject.SetActive(false);
            }
        }
    }

    private void RefreshSpPoint()
    {
        if (moduleSkill.skillInfo == null) return;
        var str = $"{moduleSkill.skillInfo.skillPoint}/{moduleGlobal.system.skillPointLimit}";
        Util.SetText(sp_text, str);
        if (detail_panel.gameObject.activeInHierarchy)
            RefreshSkillDetailPanel(detaildata);
    }

    public override void OnRenderUpdate()
    {
        time = new TimeSpan(0, 0, moduleSkill.remainTime);
        if (moduleSkill.skillInfo != null && moduleGlobal.system != null && moduleSkill.skillInfo.skillPoint < moduleGlobal.system.skillPointLimit)
            Util.SetText(remainTime, Util.Format(Util.GetString(240, 38), time.Minutes.ToString("D2"), time.Seconds.ToString("D2")));
        else Util.SetText(remainTime, 240, 37);
    }

    void _ME(ModuleEvent<Module_Skill> e)
    {
        if (!actived) return;

        if (e.moduleEvent == Module_Skill.EventUpdateSkillUp)
        {
            AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.SkillUIText, 22));
            upBtn.interactable = true;
            //播特效
            if (isLockUP) effectLock.gameObject.SetActive(true);
            else effectUp.gameObject.SetActive(true);
            //刷新
            if (detail_panel.gameObject.activeInHierarchy) RefreshSkillDetailPanel(moduleSkill.currentClickSkill);

            OnToggleValueChanged(true);
        }
        else if (e.moduleEvent == Module_Skill.EventUpdateSkillPanel)
        {
            if (!toggles[0].isOn) toggles[0].isOn = true;
            else OnToggleValueChanged(true);
        }

        RefreshSpPoint();
    }

    void _ME(ModuleEvent<Module_Player> e)
    {
        if (!actived) return;
        if (e.moduleEvent == Module_Player.EventCurrencyChanged)
        {
            var type = (CurrencySubType)e.param1;
            if (type == CurrencySubType.Gold)
                OnToggleValueChanged(true);
        }
    }
}