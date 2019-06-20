using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WelfareWindow_Cumulative : SubWindowBase<Window_Welfare>
{
    private Image m_cumlativeIcon;
    private Text m_cunlativeTxt;
    private Text m_twoExpend;
    private Text m_cunlativeTime;
    private Button m_cunlativeBtn;
    private ScrollView m_cunlativeScroll;
    private DataSource<PWeflareAward> m_cunlativeData;

    protected override void InitComponent()
    {
        base.InitComponent();
        m_cumlativeIcon   = Root.GetComponent<Image>("bg/text01_Img");
        m_cunlativeTime   = Root.GetComponent<Text>("opentime_txt");
        m_twoExpend       = Root.GetComponent<Text>("countNumber_Txt");
        m_cunlativeTxt    = Root.GetComponent<Text>("des_Txt");
        m_cunlativeScroll = Root.GetComponent<ScrollView>("itemgroup");
        m_cunlativeBtn    = Root.GetComponent<Button>("charge_Btn");
        m_cunlativeData   = new DataSource<PWeflareAward>(null, m_cunlativeScroll, SetCumulReward, OnCumulClick);
        m_twoExpend.SafeSetActive(true);

        Util.SetText(Root.GetComponent<Text>("charge_Btn/charge_Txt"), 211, 2);
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p)) return false;
        m_cunlativeBtn.onClick.RemoveAllListeners();
        m_cunlativeBtn.onClick.AddListener(delegate
        {
            parentWindow.BtnDropClick(moduleWelfare.chooseInfo?.interfaceID);
        });
        return true;
    }

    protected override void RefreshData(params object[] p)
    {
        if (moduleWelfare.chooseInfo == null) return;
        SetCumulative(moduleWelfare.chooseInfo);
    }

    private void SetCumulative(PWeflareInfo info)
    {
        if (info == null) return;

        var dataProgress = moduleWelfare.GetProgress(info);
        if ((WelfareType)info.type == WelfareType.Continuous)
            Util.SetText(m_twoExpend, ConfigText.GetDefalutString(700, 1), moduleWelfare.ShowTxt(dataProgress, 1));
        else if ((WelfareType)info.type == WelfareType.DailyFlush)
            Util.SetText(m_twoExpend, ConfigText.GetDefalutString(700, 2), moduleWelfare.ShowTxt(dataProgress, 2));

        var icon = parentWindow.GetIconName(info.icon, 0);

        UIDynamicImage.LoadImage(m_cumlativeIcon.gameObject, icon, null, true);
        m_cunlativeTxt.text = info.introduce.Replace("\\n", "\n");
        parentWindow.RemainTime(m_cunlativeTime, info.closetime);
        m_cunlativeData.SetItems(info.rewardid);
    }

    private void SetCumulReward(RectTransform rt, PWeflareAward info)
    {
        parentWindow.SetNormalInfo(rt, info);
    }
    private void OnCumulClick(RectTransform rt, PWeflareAward info)
    {
        parentWindow.SetNormalClick(info);
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
                parentWindow.RemainTime(m_cunlativeTime, Module_Welfare.instance.chooseInfo.closetime);
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
        if (info.id == moduleWelfare.chooseInfo.id) SetCumulative(info);
    }

    private void RefreshGetSucced(PWeflareInfo info, int index)
    {
        if (!parentWindow.RestWelfarePlane(info, index)) return;
        m_cunlativeData.UpdateItems();
        if (parentWindow.m_welfareTog.isOn) parentWindow.m_lableShow.UpdateItems();
    }
}
