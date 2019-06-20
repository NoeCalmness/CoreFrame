using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WelfareWindow_Level : SubWindowBase<Window_Welfare>
{
    private Image m_levelUpBgICon;
    private Image m_levelUpICon;
    private Text m_levleUpTxt;
    private Text m_fourExpend;
    private Text m_levelUpTime;
    private ScrollView m_levelUpScroll;
    private DataSource<PWeflareAward> m_levelUpData;

    protected override void InitComponent()
    {
        base.InitComponent();
        m_levelUpTime   = Root.GetComponent<Text>("txt_layout/countDown_Txt");
        m_levelUpBgICon = Root.GetComponent<Image>("bg/titleBg_Img");
        m_levelUpICon   = Root.GetComponent<Image>("bg/titleFrame_Img");
        m_levleUpTxt    = Root.GetComponent<Text>("des_Txt");
        m_fourExpend    = Root.GetComponent<Text>("txt_layout/des2_Txt");
        m_levelUpScroll = Root.GetComponent<ScrollView>("recommendList_ScrollView");
        m_levelUpData   = new DataSource<PWeflareAward>(null, m_levelUpScroll, SetLevelReward);
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p)) return false;
        return true;
    }

    protected override void RefreshData(params object[] p)
    {
        if (moduleWelfare.chooseInfo == null) return;
        SetLevelInfo(moduleWelfare.chooseInfo); 
    }

    private void SetLevelInfo(PWeflareInfo info)
    {
        SetTipTxt(info);
        var backImg = parentWindow.GetIconName(info.icon, 0);
        var txtImg = parentWindow.GetIconName(info.icon, 1);
        UIDynamicImage.LoadImage(m_levelUpBgICon.gameObject, backImg, null, true);
        UIDynamicImage.LoadImage(m_levelUpICon.gameObject, txtImg, null, true);
        m_levleUpTxt.text = info.introduce.Replace("\\n", "\n");
        parentWindow.RemainTime(m_levelUpTime, info.closetime);
        m_levelUpData.SetItems(info.rewardid);
    }

    private void SetTipTxt(PWeflareInfo info)
    {
        var dataProgress = moduleWelfare.GetProgress(info);
        m_fourExpend.SafeSetActive(true);
        switch ((WelfareType)info.type)
        {
            case WelfareType.ContDaily: Util.SetText(m_fourExpend, ConfigText.GetDefalutString(700, 3), moduleWelfare.ShowTxt(dataProgress, 1)); break;
            case WelfareType.CumulDaily: Util.SetText(m_fourExpend, ConfigText.GetDefalutString(700, 4), moduleWelfare.ShowTxt(dataProgress, 1)); break;
            case WelfareType.DiamondFlush: Util.SetText(m_fourExpend, ConfigText.GetDefalutString(700, 5), moduleWelfare.ShowTxt(dataProgress, 2)); break;
            case WelfareType.DiamondConsum: Util.SetText(m_fourExpend, ConfigText.GetDefalutString(700, 6), moduleWelfare.ShowTxt(dataProgress, 2)); break;
            case WelfareType.Level: Util.SetText(m_fourExpend, ConfigText.GetDefalutString(700, 7), modulePlayer.roleInfo.level.ToString()); break;
            case WelfareType.WaitTime:
            case WelfareType.VictoryTimes:
            case WelfareType.DailyNewSign: m_fourExpend.SafeSetActive(false); break;
        }
    }

    private void SetLevelReward(RectTransform rt, PWeflareAward info)
    {
        if (info == null) return;
        Text upTitleTxt = rt.transform.Find("title_Txt").GetComponent<Text>();
        Text upDesc = rt.transform.Find("des_Txt").GetComponent<Text>();
        GameObject upContent = rt.transform.Find("content").gameObject;
        GameObject upProp = rt.transform.Find("content/0").gameObject;
        Button getBtn = rt.transform.Find("get_Btn").GetComponent<Button>();
        Text getBtnTxt = rt.transform.Find("get_Btn/get_Txt").GetComponent<Text>();

        upDesc.text = info.desc;
        upDesc.SafeSetActive(true);
        upTitleTxt.text = info.rewardname;
        getBtn.onClick.RemoveAllListeners();
        getBtn.onClick.AddListener(delegate { parentWindow.GetAward(info, info.index); });

        parentWindow.SetBtnState(info.state, getBtn, getBtnTxt);

        List<Transform> contList = new List<Transform>();
        contList.Clear();
        foreach (Transform item in upContent.transform)
        {
            item.SafeSetActive(false);
            contList.Add(item);
        }

        if (info.reward.Length <= 0) return;

        var index = 0;
        for (int i = 0; i < info.reward.Length; i++)
        {
            if (index >= contList.Count || info.reward[i] == null) continue;

            PItem2 award = info.reward[i];
            PropItemInfo prop = ConfigManager.Get<PropItemInfo>(award.itemTypeId);

            if (!prop || !prop.IsValidVocation(modulePlayer.proto)) continue;

            Util.SetItemInfo(contList[index], prop, award.level, (int)award.num, false, award.star);

            contList[index].SafeSetActive(true);

            if (moduleWelfare.chooseInfo?.type == (int)WelfareType.VictoryTimes)
            {
                PWeflareData dataProgress = null;
                if (info.reachnum != null && info.reachnum.Length > 0)
                    dataProgress = moduleWelfare.GetProgress(moduleWelfare.chooseInfo, info.reachnum[0].type);
                if (dataProgress == null || info.reachnum[0] == null) Util.SetText(upDesc, ConfigText.GetDefalutString(211, 21), "0");
                else Util.SetText(upDesc, ConfigText.GetDefalutString(211, 21), moduleWelfare.ShowTxt(dataProgress, 3, info.reachnum[0].days));
            }
            else if (moduleWelfare.chooseInfo?.type == (int)WelfareType.WaitTime)
            {
                if (info.time < 0 || info.state != 0) upDesc.SafeSetActive(false);
                Util.SetText(upDesc, ConfigText.GetDefalutString(211, 22), Util.GetTimeMarkedFromSec(info.time));
            }

            Button upBtn = contList[index].transform.GetComponentDefault<Button>();
            upBtn.onClick.RemoveAllListeners();
            upBtn.onClick.AddListener(delegate
            {
                moduleGlobal.UpdateGlobalTip(award, true, false);
            });
            index++;
        }
        parentWindow.SetPostion(index, contList);
    }

    void _ME(ModuleEvent<Module_Welfare> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Welfare.EventWelfareGetSucced:
                var info = e.param1 as PWeflareInfo;
                var index = Util.Parse<int>(e.param2.ToString());
                if (info == null) return;
                RefreshGetSucced(info, index);
                parentWindow.WelfareSuccedShow(info.rewardid, index, info.type);
                break;
            case Module_Welfare.EventWaiteTimeRresh:
                if (moduleWelfare.chooseInfo == null) return;
                m_levelUpData.SetItems(moduleWelfare.chooseInfo.rewardid);
                break;
            case Module_Welfare.EventRemainTimeRresh:
                if (Module_Welfare.instance.chooseInfo == null) return;
                parentWindow.RemainTime(m_levelUpTime, Module_Welfare.instance.chooseInfo.closetime);
                break;
            case Module_Welfare.EventWelfareMoneyChange:
                var money = e.msg as PWeflareInfo;
                RefreshChangeInfo(money, false);
                break;
            case Module_Welfare.EventWelfareCanGet:
            case Module_Welfare.EventWelfareSpecificInfo:
                var newInfo = e.msg as PWeflareInfo;
                RefreshChangeInfo(newInfo, true);
                break;
        }
    }

    private void RefreshChangeInfo(PWeflareInfo info, bool refresh)
    {
        if (refresh) parentWindow.RefreshLablePlane();
        if (moduleWelfare.chooseInfo == null) return;
        if (info.id == moduleWelfare.chooseInfo.id) SetLevelInfo(info);
    }

    private void RefreshGetSucced(PWeflareInfo info, int index)
    {
        if (!parentWindow.RestWelfarePlane(info, index)) return;
        m_levelUpData.UpdateItems();
        if (parentWindow.m_welfareTog.isOn) parentWindow.m_lableShow.UpdateItems();
    }

}
