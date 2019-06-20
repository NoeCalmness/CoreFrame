using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WelfareWindow_Flush : SubWindowBase<Window_Welfare>
{
    private Image m_firstIcon;
    private Text m_firstTxt;
    private Text m_firstTime;
    private Button m_getBtn;
    private Text m_getBtnTxt;
    private Text m_oneExpend;
    private Button m_firstBtn;
    private ScrollView m_firstScroll;
    private PWeflareData m_data;
    private DataSource<PItem2> m_firstData;

    protected override void InitComponent()
    {
        base.InitComponent();
        m_firstTime   = Root.GetComponent<Text>("opentime_txt");
        m_firstBtn    = Root.GetComponent<Button>("charge_Btn");
        m_firstIcon   = Root.GetComponent<Image>("bg/text_Img");
        m_firstTxt    = Root.GetComponent<Text>("des_Txt");
        m_firstScroll = Root.GetComponent<ScrollView>("itemgroup");
        m_getBtn      = Root.GetComponent<Button>("get_Btn");
        m_getBtnTxt   = Root.GetComponent<Text>("get_Btn/charge_Txt");
        m_oneExpend   = Root.GetComponent<Text>("numbertxt");
        m_firstData   = new DataSource<PItem2>(null, m_firstScroll, SetFirstReward, SetFirstClick);

        Util.SetText(Root.GetComponent<Text>("charge_Btn/charge_Txt"), 211, 20);
        Util.SetText(Root.GetComponent<Text>("get_Btn/charge_Txt"), 211, 3);
    }

    //必须调用才可以
    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p)) return false;
        return true;
    }

    protected override void RefreshData(params object[] p)
    {
        if (moduleWelfare.chooseInfo == null) return;
        RefreshAllInfo(moduleWelfare.chooseInfo);
    }

    private void RefreshAllInfo(PWeflareInfo info)
    {
        m_data = moduleWelfare.GetProgress(info);
        m_oneExpend.SafeSetActive(false);
        m_oneExpend.SafeSetActive(false);

        if ((WelfareType)info.type == WelfareType.StrengConsum || (WelfareType)info.type == WelfareType.SpecifiedStreng) SetStreng(info);
        else if ((WelfareType)info.type == WelfareType.MatchStreetPvP) SetMatchStreePvP(info);
        else if ((WelfareType)info.type == WelfareType.FirstFlush) SetFirstFlush(info);

        m_getBtn.onClick.RemoveAllListeners();
        m_getBtn.onClick.AddListener(delegate
        {
            if (moduleWelfare.chooseInfo.rewardid != null && moduleWelfare.chooseInfo.rewardid.Length > 0)
            {
                parentWindow.GetAward(moduleWelfare.chooseInfo.rewardid[0], 1);
            }
        });

        m_firstBtn.onClick.RemoveAllListeners();
        m_firstBtn.onClick.AddListener(delegate
        {
            parentWindow.BtnDropClick(info.interfaceID);
        });
    }
    
    private void SetMatchStreePvP(PWeflareInfo info)
    {
        m_oneExpend.SafeSetActive(true);
        SetFirstFlush(info);
        //使用 days代替taskinfo中的taskid 来寻找通次数
        if (info.rewardid == null || info.rewardid.Length == 0 || info.rewardid[0] == null || info.rewardid[0].reachnum.Length == 0 || info.rewardid[0].reachnum[0] == null)
            Util.SetText(m_oneExpend, ConfigText.GetDefalutString(700, 9), "0");
        else Util.SetText(m_oneExpend, ConfigText.GetDefalutString(700, 9), moduleWelfare.ShowTxt(m_data, 3, info.rewardid[0].reachnum[0].days));
    }

    private void SetStreng(PWeflareInfo info)
    {
        if (info == null) return;
        m_oneExpend.SafeSetActive(false);
        SetFirstFlush(info);
        m_oneExpend.SafeSetActive(true);
        var dataProgress = moduleWelfare.GetProgress(info);
        Util.SetText(m_oneExpend, ConfigText.GetDefalutString(700, 8), moduleWelfare.ShowTxt(dataProgress, 2));
        m_firstBtn.onClick.RemoveAllListeners();
        m_firstBtn.onClick.AddListener(delegate
        {
            parentWindow.BtnDropClick(moduleWelfare.chooseInfo?.interfaceID);
        });
    }

    private void SetFirstFlush(PWeflareInfo firstInfo)
    {
        if (firstInfo == null) return;
        var icon = parentWindow.GetIconName(firstInfo.icon, 0);
        UIDynamicImage.LoadImage(m_firstIcon.gameObject, icon, null, true);

        if (firstInfo.rewardid.Length <= 0) return;

        m_firstTxt.text = firstInfo.introduce.Replace("\\n", "\n");
        m_getBtn.SafeSetActive(false);
        m_getBtn.interactable = false;
        m_firstBtn.SafeSetActive(false);
        parentWindow.RemainTime(m_firstTime, firstInfo.closetime);

        var list = moduleWelfare.CheckProto(firstInfo.rewardid[0].reward);
        m_firstData.SetItems(list);

        var state = firstInfo.rewardid[0].state;
        if (state == 0) m_firstBtn.SafeSetActive(true);
        else if (state == 1)
        {
            m_getBtn.SafeSetActive(true);
            m_getBtn.interactable = true;
            Util.SetText(m_getBtnTxt, ConfigText.GetDefalutString(211, 3));
        }
        else if (state == 2)
        {
            m_getBtn.SafeSetActive(true);
            Util.SetText(m_getBtnTxt, ConfigText.GetDefalutString(211, 4));
        }
    }
    private void SetFirstReward(RectTransform rt, PItem2 info)
    {
        if (info == null) return;
        PropItemInfo prop = ConfigManager.Get<PropItemInfo>(info.itemTypeId);

        Util.SetItemInfo(rt, prop, info.level, (int)info.num, false, info.star);

        GameObject get = rt.Find("get").gameObject;
        GameObject light = rt.Find("light").gameObject;

        light.SafeSetActive(false);
        get.SafeSetActive(false);

    }
    private void SetFirstClick(RectTransform rt, PItem2 info)
    {
        if (info == null) return;
        moduleGlobal.UpdateGlobalTip(info, true, false);
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
            case Module_Welfare.EventRemainTimeRresh:
                if (Module_Welfare.instance.chooseInfo == null) return;
                parentWindow.RemainTime(m_firstTime, Module_Welfare.instance.chooseInfo.closetime);
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
        if (info.id == moduleWelfare.chooseInfo.id) RefreshAllInfo(info);
    }

    private void RefreshGetSucced(PWeflareInfo info, int index)
    {
        if (!parentWindow.RestWelfarePlane(info, index)) return;

        SetFirstFlush(info);
        if (parentWindow.m_welfareTog.isOn) parentWindow.m_lableShow.UpdateItems();
    }

}