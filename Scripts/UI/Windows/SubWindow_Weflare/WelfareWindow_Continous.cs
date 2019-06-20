using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WelfareWindow_Continous : SubWindowBase<Window_Welfare>
{
    private Image m_continuousIcon;
    private Text m_threeExpend;
    private Text m_continousTxt;
    private Text m_continousTime;
    private Button m_continousBtn;//前往充值按钮
    private ScrollView m_continousScroll;
    private DataSource<PWeflareAward> m_continousData;

    protected override void InitComponent()
    {
        base.InitComponent();
        m_continuousIcon  = Root.GetComponent<Image>("bg/text01_Img");
        m_continousTime   = Root.GetComponent<Text>("opentime_txt");
        m_threeExpend     = Root.GetComponent<Text>("countNumber_Txt");
        m_continousTxt    = Root.GetComponent<Text>("des_Txt");
        m_continousScroll = Root.GetComponent<ScrollView>("itemgroup");
        m_continousBtn    = Root.GetComponent<Button>("charge_Btn");
        m_continousData   = new DataSource<PWeflareAward>(null, m_continousScroll, SetContReward, OnContClick);

        Util.SetText(Root.GetComponent<Text>("charge_Btn/charge_Txt"), 211, 2);
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p)) return false;
        m_continousBtn.onClick.RemoveAllListeners();
        m_continousBtn.onClick.AddListener(delegate
        {
            parentWindow.BtnDropClick(moduleWelfare.chooseInfo?.interfaceID);
        });
        return true;
    }

    protected override void RefreshData(params object[] p)
    {
        if (moduleWelfare.chooseInfo == null) return;
        SetContinuous(moduleWelfare.chooseInfo);
    }

    private void SetContinuous(PWeflareInfo info)
    {
        var dataProgress = moduleWelfare.GetProgress(info);
        Util.SetText(m_threeExpend, ConfigText.GetDefalutString(700, 0), moduleWelfare.ShowTxt(dataProgress, 2));

        var icon = parentWindow.GetIconName(info.icon, 0);
        UIDynamicImage.LoadImage(m_continuousIcon.gameObject, icon, null, true);
        m_continousTxt.text = info.introduce.Replace("\\n", "\n");
        parentWindow.RemainTime(m_continousTime, info.closetime);
        m_continousData.SetItems(info.rewardid);

    }
    private void SetContReward(RectTransform rt, PWeflareAward info)
    {
        parentWindow.SetNormalInfo(rt, info);

    }
    private void OnContClick(RectTransform rt, PWeflareAward info)
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
                parentWindow.RemainTime(m_continousTime, Module_Welfare.instance.chooseInfo.closetime);
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
        if (info.id == moduleWelfare.chooseInfo.id) SetContinuous(info);
    }

    private void RefreshGetSucced(PWeflareInfo info, int index)
    {
        if (!parentWindow.RestWelfarePlane(info, index)) return;
        m_continousData.UpdateItems();
        if (parentWindow.m_welfareTog.isOn) parentWindow.m_lableShow.UpdateItems();
    }
}
