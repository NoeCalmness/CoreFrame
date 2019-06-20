/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-08-21
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Welfare : Window
{
    private RectTransform m_welfPlane;
    private RectTransform m_chargePlane;

    private RectTransform m_root;

    private Transform m_detailRoot;
    private ChargeWindow_Detail m_detailWindow;

    public Toggle m_welfareTog;
    public Toggle m_chargeTog;
    private Image m_welfareHint;
    private Image m_chargeHint;

    private ScrollView m_lableScroll;
    private TweenPosition m_checkImg;
    public DataSource<PWeflareInfo> m_lableShow;

    private bool m_setChoose;
    string[] recommend = new string[3] { "ui_welfare_hoticon", "ui_welfare_recommendicon", "ui_welfare_valueicon" };
    Dictionary<WelfareType, SubWindowBase> subWindows = new Dictionary<WelfareType, SubWindowBase>();

    protected override void OnOpen()
    {
        m_setChoose = false;

        m_welfPlane = GetComponent<RectTransform>("wbg");
        m_chargePlane = GetComponent<RectTransform>("cbg");

        m_root = GetComponent<RectTransform>("AssetRoot");
        m_detailRoot = GetComponent<RectTransform>("preview_panel");
        m_checkImg = GetComponent<TweenPosition>("checkBox/viewport/content/check_Img");

        m_welfareTog = GetComponent<Toggle>("toggleGrop/welfareTog");
        m_chargeTog = GetComponent<Toggle>("toggleGrop/chargeTog");
        m_welfareHint = GetComponent<Image>("toggleGrop/welfareTog/hint");
        m_chargeHint = GetComponent<Image>("toggleGrop/chargeTog/hint");

        m_lableScroll = GetComponent<ScrollView>("checkBox");
        m_lableShow = new DataSource<PWeflareInfo>(null, m_lableScroll, SetlableInfo, OnLableClick);

        m_welfareTog.onValueChanged.AddListener(delegate
        {
            if (!m_welfareTog.isOn) return;
            SetTogState(moduleWelfare.allWeflarInfo, 0);
        });
        m_chargeTog.onValueChanged.AddListener(delegate
        {
            if (!m_chargeTog.isOn) return;
            SetTogState(moduleWelfare.allChargeInfo, 1);
        });

        m_detailWindow = SubWindowBase.CreateSubWindow<ChargeWindow_Detail>(this, m_detailRoot.gameObject);

        SetSubWindow();
    }

    private void SetTogState(List<PWeflareInfo> info, int type)
    {
        if (info.Count > 0 && (type != 0 || !moduleWelfare.isfirst) && m_setChoose)
        {
            moduleWelfare.chooseInfo = info[0];
        }

        m_welfPlane.gameObject.SetActive(type == 0);
        m_chargePlane.gameObject.SetActive(type == 1);

        m_setChoose = true;
        m_checkImg.SafeSetActive(info.Count > 0);
        SetTypePlane(moduleWelfare.chooseInfo);
        LableClickHint(moduleWelfare.chooseInfo);
        m_lableShow.SetItems(info);
        var index = info.IndexOf(moduleWelfare.chooseInfo);
        if (index == -1) index = 0;
        m_lableScroll.ScrollToIndex(index, 0);
    }

    private void SetSubWindow()
    {
        subWindows.Clear();

        var sign = SubWindowBase<Window_Welfare>.CreateSubWindow<WelfareWindow_Sign>(this, m_root, "welfare_sign");
        subWindows.Add(WelfareType.Sign, sign);

        var puzzle = SubWindowBase<Window_Welfare>.CreateSubWindow<WelfareWindow_Puzzle>(this, m_root, "welfare_puzzle");
        subWindows.Add(WelfareType.ActiveNewPuzzle, puzzle);

        var firstFlush = SubWindowBase<Window_Welfare>.CreateSubWindow<WelfareWindow_Flush>(this, m_root, "welfare_firstflush");
        subWindows.Add(WelfareType.FirstFlush, firstFlush);
        subWindows.Add(WelfareType.MatchStreetPvP, firstFlush);
        subWindows.Add(WelfareType.DailySign, firstFlush);
        subWindows.Add(WelfareType.StrengConsum, firstFlush);
        subWindows.Add(WelfareType.SpecifiedStreng, firstFlush);

        var cumulative = SubWindowBase<Window_Welfare>.CreateSubWindow<WelfareWindow_Cumulative>(this, m_root, "welfare_cumulative");
        subWindows.Add(WelfareType.Continuous, cumulative);
        subWindows.Add(WelfareType.DailyFlush, cumulative);

        var continous = SubWindowBase<Window_Welfare>.CreateSubWindow<WelfareWindow_Continous>(this, m_root, "welfare_continuous");
        subWindows.Add(WelfareType.Cumulative, continous);

        var level = SubWindowBase<Window_Welfare>.CreateSubWindow<WelfareWindow_Level>(this, m_root, "welfare_levelup");
        subWindows.Add(WelfareType.ContDaily, level);
        subWindows.Add(WelfareType.CumulDaily, level);
        subWindows.Add(WelfareType.DiamondFlush, level);
        subWindows.Add(WelfareType.DiamondConsum, level);
        subWindows.Add(WelfareType.Level, level);
        subWindows.Add(WelfareType.WaitTime, level);
        subWindows.Add(WelfareType.VictoryTimes, level);
        subWindows.Add(WelfareType.DailyNewSign, level);

        var card = SubWindowBase<Window_Welfare>.CreateSubWindow<ChargeWindow_Card>(this, m_root, "welfare_monthlycard");
        subWindows.Add(WelfareType.ChargeMonthCard, card);
        subWindows.Add(WelfareType.ChargeSeasonCard, card);
        var sale = SubWindowBase<Window_Welfare>.CreateSubWindow<ChargeWindow_Sale>(this, m_root, "welfare_costbag");
        subWindows.Add(WelfareType.ChargeDailySale, sale);
        subWindows.Add(WelfareType.ChargeWeekSale, sale);
        subWindows.Add(WelfareType.ChargeGrowth, SubWindowBase<Window_Welfare>.CreateSubWindow<ChargeWindow_Growth>(this, m_root, "welfare_growfund"));
        subWindows.Add(WelfareType.ChargeGift, SubWindowBase<Window_Welfare>.CreateSubWindow<ChargeWindow_Gift>(this, m_root, "welfare_giftbag"));
        subWindows.Add(WelfareType.ChargeWish, SubWindowBase<Window_Welfare>.CreateSubWindow<ChargeWindow_Sign>(this, m_root, "welfare_pray"));
        subWindows.Add(WelfareType.ChargeSunmon, SubWindowBase<Window_Welfare>.CreateSubWindow<ChargeWindow_Sign>(this, m_root, "welfare_pet"));

    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault(1, false);//top工具条的显示
        SetEnterChooseInfo();
    }

    private void SetEnterChooseInfo()
    {
        m_welfPlane.gameObject.SetActive(true);
        m_checkImg.SafeSetActive(false);
        m_welfareTog.isOn = false;
        m_chargeTog.isOn = false;
        SetToggleShow();

        var index = 0;
        if (m_subTypeLock == -1)
        {
            if (moduleWelfare.allWeflarInfo.Count == 0) return;
            if (moduleWelfare.isfirst && moduleWelfare.m_signWefare != null)
            {
                index = moduleWelfare.allWeflarInfo.IndexOf(moduleWelfare.m_signWefare);
                moduleWelfare.chooseInfo = moduleWelfare.m_signWefare;
            }
            else moduleWelfare.chooseInfo = moduleWelfare.allWeflarInfo[index];
            m_setChoose = false;
            m_welfareTog.isOn = true;
            m_lableScroll.ScrollToIndex(index, 0);
        }
        else
        {
            index = moduleWelfare.allChargeInfo.FindIndex(a => a.id == m_subTypeLock);
            if (index != -1 && moduleWelfare.allChargeInfo.Count > 0)
            {
                moduleWelfare.chooseInfo = moduleWelfare.allChargeInfo[index];
                m_setChoose = false;
                m_chargeTog.isOn = true;
                m_lableScroll.ScrollToIndex(index, 0);
            }
            else if (moduleWelfare.allWeflarInfo.Count > 0)
            {
                index = moduleWelfare.allWeflarInfo.FindIndex(a => a.id == m_subTypeLock);
                if (index == -1)
                    index = moduleWelfare.allWeflarInfo.FindIndex(a => a.type == (int)WelfareType.ActiveNewPuzzle);

                if (index == -1) index = 0;
                moduleWelfare.chooseInfo = moduleWelfare.allWeflarInfo[index];
                m_lableScroll.ScrollToIndex(index, 0);
            }
        }
        moduleWelfare.isfirst = false;
    }

    #region SetLable

    private void SetlableInfo(RectTransform rt, PWeflareInfo info)
    {
        if (moduleWelfare.chooseInfo == null || info == null) return;
        var lable = rt.Find("label_Txt").GetComponent<Text>();
        var tip = rt.Find("tip");

        Util.SetText(lable, info.title);
        tip.gameObject.SetActive(false);
        if (info.title.Contains("&"))
        {
            var t = info.title.Split('&')[0];
            var txt = info.title.Split('&')[1];
            tip.gameObject.SetActive(true);
            Util.SetText(lable, txt);
            var index = Util.Parse<int>(t);
            if (index < 0 || index >= recommend.Length) index = 0;
            AtlasHelper.SetShared(tip, recommend[index], null, true);
        }

        if (info.type == (int)WelfareType.ActiveNewPuzzle) Util.SetText(lable, info.introduce);

        if (info.id == moduleWelfare.chooseInfo.id || (moduleWelfare.chooseInfo.type == (int)WelfareType.ActiveNewPuzzle && info.type == (int)WelfareType.ActiveNewPuzzle))
        {
            var tog = rt.parent.GetComponentDefault<Toggle>();
            tog.isOn = false;
            m_checkImg.duration = 0.15f;
            m_checkImg.from = m_checkImg.transform.localPosition;
            var pos = rt.parent.localPosition;
            pos.y -= 4;
            m_checkImg.to = pos;
            m_checkImg.Play();
            tog.isOn = true;
        }
        else if (lable.color != Color.white)
        {
            var teween = lable.transform.GetComponent<TweenColor>();
            teween.PlayReverse();
        }
        var hint = rt.Find("hint");
        hint.SafeSetActive(moduleWelfare.GeLableHint(info));
    }

    private void OnLableClick(RectTransform rt, PWeflareInfo info)
    {
        if (moduleWelfare.chooseInfo == null || info == null) return;
        if (info.id == moduleWelfare.chooseInfo.id) return;

        moduleWelfare.chooseInfo = info;
        SetTypePlane(info);
        LableClickHint(info);

        m_checkImg.duration = 0.15f;
        m_checkImg.from = m_checkImg.transform.localPosition;
        var pos = rt.parent.localPosition;
        pos.y -= 4;
        m_checkImg.to = pos;
        m_checkImg.Play();

        var tog = rt.parent.GetComponentDefault<Toggle>();
        tog.isOn = true;
    }

    private void SetTypePlane(PWeflareInfo info)//并设置对应的道具
    {
        if (info == null || !subWindows.ContainsKey((WelfareType)info.type)) return;

        foreach (var window in subWindows)
            window.Value?.UnInitialize();

        subWindows[(WelfareType)info.type].Initialize_Async();
        SetToggleHint();
    }

    public void LableClickHint(PWeflareInfo Info)
    {
        if (Info == null) return;
        string date_now = Util.GetServerLocalTime().ToString();
        string idm = "welf" + Info.id.ToString() + modulePlayer.roleInfo.roleId.ToString();
        PlayerPrefs.SetString(idm, date_now);

        bool isShow = moduleWelfare.notClick.Exists(a => a == Info.id);
        if (isShow) moduleWelfare.notClick.Remove(Info.id);

        int index = moduleWelfare.allWeflarInfo.IndexOf(Info);
        if (Info.type == (int)WelfareType.ActiveNewPuzzle && index == -1)
        {
            index = moduleWelfare.allWeflarInfo.FindIndex(a => a.type == Info.type);
        }
        if (Info.type >= (int)WelfareType.ChargeDailySale && index == -1)
        {
            index = moduleWelfare.allChargeInfo.IndexOf(Info);
        }
        if (index != -1) m_lableShow.UpdateItem(index);
    }

    #endregion

    #region SameInfo

    public void SetBtnState(int state, Button btn, Text txt)
    {
        btn.interactable = (state == 1 || state == 3) ? true : false;
        if (state == 0) Util.SetText(txt, ConfigText.GetDefalutString(211, 10));
        if (state == 1) Util.SetText(txt, ConfigText.GetDefalutString(211, 3));
        if (state == 2) Util.SetText(txt, ConfigText.GetDefalutString(211, 4));
        if (state == 3) Util.SetText(txt, ConfigText.GetDefalutString(211, 24));
    }


    public void GetAward(PWeflareAward info, int index)
    {
        if (info == null) return;
        if (info.state == 3)
        {
            var cost = moduleWelfare.GetCostMoney(info);
            if (cost == 0)
            {
                moduleWelfare.SendBuyReward(index);
                return;
            }
            Window_Alert.ShowAlert(string.Format(ConfigText.GetDefalutString(211, 25), cost), true, true, true,
            () =>
            {
                moduleWelfare.SendBuyReward(index);
            }, null, "", "");
        }
        else if (info.state == 1) moduleWelfare.SendGetReward(index);
    }

    public void SetNormalInfo(RectTransform rt, PWeflareAward info)
    {
        if (info == null) return;
        GameObject get = rt.Find("get").gameObject;
        GameObject light = rt.Find("light").gameObject;
        Text dateTxt = rt.Find("date_Txt").GetComponent<Text>();
        Util.SetText(dateTxt, info.rewardname);
        light.SafeSetActive(info.state == 1);
        get.SafeSetActive(info.state == 2);

        List<PItem2> twoThreeReward = new List<PItem2>();
        for (int i = 0; i < info.reward.Length; i++)
        {
            if (info.reward[i] == null) continue;
            PropItemInfo propItem = ConfigManager.Get<PropItemInfo>(info.reward[i].itemTypeId);
            if (!propItem || !propItem.IsValidVocation(modulePlayer.proto)) continue;
            twoThreeReward.Add(info.reward[i]);
        }
        if (twoThreeReward.Count <= 0 || info.reward == null || info.reward?.Length <= 0) return;

        PropItemInfo prop = ConfigManager.Get<PropItemInfo>(twoThreeReward[0].itemTypeId);
        Util.SetItemInfo(rt, prop, info.reward[0].level, (int)info.reward[0].num, false, info.reward[0].star);

    }

    public void SetNormalClick(PWeflareAward info)
    {
        if (info == null) return;
        if (info.state == 0)
        {
            if (info.reward == null || info.reward?.Length <= 0) return;
            moduleGlobal.UpdateGlobalTip(info.reward[0], true, false);
        }
        else if (info.state == 1 || info.state == 3) GetAward(info, info.index);
        else if (info.state == 2) moduleGlobal.ShowMessage(211, 23);
    }

    public string GetIconName(string icon, int index)
    {
        var str = icon;
        if (icon.Contains(";"))
        {
            if (index == 0) str = icon.Split(';')[0];
            if (index == 1 || string.IsNullOrEmpty(str)) str = icon.Split(';')[1];
        }
        return str;
    }
    #endregion

    #region _ME

    void _ME(ModuleEvent<Module_Welfare> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Welfare.EventWelfareAllInfo: SetEnterChooseInfo(); break;
            case Module_Welfare.EventWelfareOpen:
                var info = e.param1 as List<PWeflareInfo>;
                RefreshShowPlane(info);
                SetItemType();
                SetToggleHint();
                SetToggleShow();
                break;
            case Module_Welfare.EventWelfareClose: SetClosePlane(); break;
        }
    }

    private void RefreshShowPlane(List<PWeflareInfo> info)
    {
        if (moduleWelfare.chooseInfo == null) return;
        var show = info.Find(a => a.id == moduleWelfare.chooseInfo.id);
        if (show != null)
        {
            moduleWelfare.chooseInfo = show;
            SetTypePlane(moduleWelfare.chooseInfo);
        }
    }
    
    private void SetClosePlane()
    {
        if (m_welfareTog.isOn && moduleWelfare.allWeflarInfo.Count > 0)
        {
            SetChooseInfo(moduleWelfare.allWeflarInfo,m_welfareTog);
        }
        else if (m_chargeTog.isOn && moduleWelfare.allChargeInfo.Count > 0)
        {
            SetChooseInfo(moduleWelfare.allChargeInfo, m_chargeTog);
        }
        SetToggleHint();
        SetToggleShow();
    }

    private void SetChooseInfo(List<PWeflareInfo> list,Toggle tog)
    {
        bool have = list.Exists(a => a.id == moduleWelfare.chooseInfo.id);
        if (moduleWelfare.chooseInfo.type == (int)WelfareType.ActiveNewPuzzle)
            have = moduleWelfare.puzzleList.Exists(a => a.id == moduleWelfare.chooseInfo.id);
        if (tog.isOn)
        {
            if (!have)
            {
                moduleWelfare.chooseInfo = list[0];
                LableClickHint(list[0]);
                SetTypePlane(moduleWelfare.chooseInfo);
            }
            m_lableShow.SetItems(list);
        }
    }
    
    public void SetToggleHint()
    {
        m_welfareHint.gameObject.SetActive(moduleWelfare.GetTypeState(0));
        m_chargeHint.gameObject.SetActive(moduleWelfare.GetTypeState(1));
    }

    public void RefreshLablePlane()
    {
        SetItemType();
        SetToggleHint();
    }

    private void SetItemType()
    {
        if (m_welfareTog.isOn) m_lableShow.SetItems(moduleWelfare.allWeflarInfo);
        else if (m_chargeTog.isOn) m_lableShow.SetItems(moduleWelfare.allChargeInfo);
    }
    #endregion

    #region Succed Tip

    public bool RestWelfarePlane(PWeflareInfo info, int index)
    {
        SetToggleHint();
        if (moduleWelfare.chooseInfo?.id != info.id || moduleWelfare.allWeflarInfo.Count == 0) return false;
        var have = moduleWelfare.allWeflarInfo.Exists(a => a.id == moduleWelfare.chooseInfo?.id);
        if (info.type == (int)WelfareType.ActiveNewPuzzle) have = moduleWelfare.puzzleList.Exists(a => a.id == moduleWelfare.chooseInfo?.id);
        if (!have && m_welfareTog.isOn)
        {
            moduleWelfare.chooseInfo = moduleWelfare.allWeflarInfo[0];
            LableClickHint(moduleWelfare.allWeflarInfo[0]);
            SetTypePlane(moduleWelfare.allWeflarInfo[0]);
            m_lableShow.SetItems(moduleWelfare.allWeflarInfo);
            return false;
        }
        return true;
    }

    public void WelfareSuccedShow(PWeflareAward[] info, int index, int type)//获得物品提示
    {
        if (info.Length <= 0 || (WelfareType)type == WelfareType.Sign) return;

        if ((WelfareType)type == WelfareType.FirstFlush || (WelfareType)type == WelfareType.StrengConsum || (WelfareType)type == WelfareType.DailySign || (WelfareType)type == WelfareType.SpecifiedStreng || (WelfareType)type == WelfareType.MatchStreetPvP)
        {
            var list = moduleWelfare.CheckProto(info[0].reward);
            if ((WelfareType)type == WelfareType.SpecifiedStreng)//只有该类型index为倍数
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].num = list[i].num * (uint)index;
                }
            }
            Window_ItemTip.Show(ConfigText.GetDefalutString(211, 18), list);
        }
        else
        {
            for (int i = 0; i < info.Length; i++)
            {
                if (info[i].index == index)
                {
                    Window_ItemTip.Show(ConfigText.GetDefalutString(211, 18), moduleWelfare.CheckProto(info[i].reward));
                }
            }
        }
    }

    public void SetPostion(int num, List<Transform> rewardLis)
    {
        if (num > 4) num = 4;

        //设置坐标
        if (num == 1)
        {
            rewardLis[0].GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        }
        else if (num == 2)
        {
            rewardLis[0].GetComponent<RectTransform>().anchoredPosition = new Vector3(-55, 0, 0);
            rewardLis[1].GetComponent<RectTransform>().anchoredPosition = new Vector3(55, 0, 0);
        }
        else if (num == 3)
        {
            rewardLis[0].GetComponent<RectTransform>().anchoredPosition = new Vector3(-110, 0, 0);
            rewardLis[1].GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            rewardLis[2].GetComponent<RectTransform>().anchoredPosition = new Vector3(110, 0, 0);
        }
        else if (num == 4)
        {
            rewardLis[0].GetComponent<RectTransform>().anchoredPosition = new Vector3(-165, 0, 0);
            rewardLis[1].GetComponent<RectTransform>().anchoredPosition = new Vector3(-55, 0, 0);
            rewardLis[2].GetComponent<RectTransform>().anchoredPosition = new Vector3(55, 0, 0);
            rewardLis[3].GetComponent<RectTransform>().anchoredPosition = new Vector3(165, 0, 0);
        }
    }

    #endregion

    public void RemainTime(Text remainTxt, long closeTime)
    {
        if (closeTime == 0)
        {
            remainTxt.SafeSetActive(false);
            return;
        }

        remainTxt.SafeSetActive(true);
        DateTime nowTime = Util.GetServerLocalTime();
        DateTime endTime = Util.GetDateTime(closeTime);

        if (DateTime.Compare(nowTime, endTime) > 0) remainTxt.SafeSetActive(false);

        TimeSpan allRemain = endTime - nowTime;
        if (allRemain.Days > 0) Util.SetText(remainTxt, ConfigText.GetDefalutString(211, 11), allRemain.Days);
        else
        {
            if (allRemain.Hours > 0) Util.SetText(remainTxt, ConfigText.GetDefalutString(211, 12), allRemain.Hours);
            else if (allRemain.Minutes > 0) Util.SetText(remainTxt, ConfigText.GetDefalutString(211, 13), allRemain.Minutes);
        }
    }

    #region charge

    public List<int> GetChargeID()
    {
        List<int> pId = new List<int>();
        for (int i = 0; i < moduleWelfare.chooseInfo.rewardid.Length; i++)
        {
            var item = moduleWelfare.chooseInfo.rewardid[i];
            if (item == null || item.reachnum == null || item.reachnum.Length == 0) continue;
            if (!pId.Contains(item.reachnum[0].progress)) pId.Add(item.reachnum[0].progress);
        }
        return pId;
    }

    public void ShowDetail(PReward reward)
    {
        ShowDetail(ItemList(reward));
    }

    public List<ItemPair> ItemList(PReward reward)
    {
        List<ItemPair> list = new List<ItemPair>();

        if (reward.diamond > 0)
        {
            var p = new ItemPair
            {
                itemId = 2,
                count = reward.diamond
            };
            list.Add(p);
        }

        if (reward.coin > 0)
        {
            var p = new ItemPair
            {
                itemId = 1,
                count = reward.coin
            };
            list.Add(p);
        }

        if (reward.fatigue > 0)
        {
            var p = new ItemPair
            {
                itemId = 15,
                count = reward.fatigue
            };
            list.Add(p);
        }
        foreach (var p in reward.rewardList)
        {
            list.Add(new ItemPair { itemId = p.itemTypeId, count = (int)p.num });
        }
        return list;
    }

    public void ShowDetail(List<ItemPair> list)
    {
        if (list == null || list.Count == 0)
            return;

        m_detailWindow.Initialize(list);
    }

    #endregion

    private void SetToggleShow()
    {
        m_welfareTog.gameObject.SetActive(moduleWelfare.allWeflarInfo.Count > 0);
        m_chargeTog.gameObject.SetActive(moduleWelfare.allChargeInfo.Count > 0);
    }

    public void BtnDropClick(string str)
    {
        if (str.Contains("-"))
        {
            var drop = str.Split('-');
            moduleAnnouncement.OpenWindow(Util.Parse<int>(drop[0]), Util.Parse<int>(drop[1]));
        }
        else moduleAnnouncement.OpenWindow(Util.Parse<int>(str));
    }

    protected override void OnClose()
    {
        base.OnClose();
        foreach (var kv in subWindows)
        {
            kv.Value.Destroy();
        }
        subWindows.Clear();
        m_detailWindow.Destroy();
    }

    protected override void OnReturn()
    {
        base.OnReturn();
        m_welfareTog.gameObject.SetActive(false);
        m_chargeTog.gameObject.SetActive(false);
        moduleHome.UpdateIconState(HomeIcons.Welfare, false);
    }

}
