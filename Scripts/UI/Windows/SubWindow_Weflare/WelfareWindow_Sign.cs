using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WelfareWindow_Sign : SubWindowBase<Window_Welfare>
{
    private GameObject m_signHeight;
    private ScrollView m_signScroll;
    private Text m_datetime;
    private DataSource<SignStateInfo> m_signInfo;

    protected override void InitComponent()
    {
        base.InitComponent();
        m_datetime   = Root.GetComponent<Text>("date/date_Txt"); ;//用来显示当前时间
        m_signHeight = Root.GetComponent<Image>("scrollView/highlight").gameObject;
        m_signScroll = Root.GetComponent<ScrollView>("scrollView");
        m_signInfo   = new DataSource<SignStateInfo>(moduleWelfare.SetInfo, m_signScroll, SetSignInfo, OnSignClick);

        Util.SetText(Root.GetComponent<Text>("sign_In/sign_text"), 211, 1);
        Util.SetText(Root.GetComponent<Text>("Date/Text"), 211, 0);
    }
    
    //必须调用才可以
    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p)) return false;
        m_signInfo?.SetItems(moduleWelfare.SetInfo);
        return true;
    }

    private void SetSignInfo(RectTransform rt, SignStateInfo info)
    {
        if (info == null) return;
        m_datetime.text = Util.GetServerLocalTime().ToString("d");
        var item = ConfigManager.Get<PropItemInfo>(info.wupin.itemTypeId);
        Util.SetItemInfo(rt, item, info.wupin.level, (int)info.wupin.num, false, info.wupin.star);

        var signed = rt.transform.Find("signed");
        var effect = rt.transform.Find("quality/6/effectnode");

        effect.SafeSetActive(info.state == 1);
        signed.SafeSetActive(info.state == 1);

        if (info.state == 2)
        {
            m_signHeight.transform.SetParent(rt);
            m_signHeight.transform.localScale = new Vector3(1, 1, 1);
            m_signHeight.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            m_signHeight.SafeSetActive(true);
        }
    }

    private void OnSignClick(RectTransform rt, SignStateInfo info)
    {
        if (info == null) return;
        if (info.state == 2 && !moduleWelfare.isget) moduleWelfare.SendSign();
        else moduleGlobal.UpdateGlobalTip(info.wupin, true, false);
    }

    private void SignSuccedPlane()
    {
        m_signHeight.SafeSetActive(false);

        SignStateInfo signGet = moduleWelfare.SetInfo[moduleWelfare.already];
        if (signGet == null) return;
        var succedInfo = signGet.wupin;
        List<PItem2> item = new List<PItem2>();
        item.Add(succedInfo);
        Window_ItemTip.Show(ConfigText.GetDefalutString(211, 19), item);
        m_signInfo.SetItem(signGet, moduleWelfare.already);
    }

    void _ME(ModuleEvent<Module_Welfare> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Welfare.EventWelfareSignData:
                m_signInfo.SetItems(moduleWelfare.SetInfo);
                break;
            case Module_Welfare.EventWelfareSign:
                SignSuccedPlane();
                parentWindow.SetToggleHint();
                if (parentWindow.m_welfareTog.isOn) parentWindow.m_lableShow.UpdateItems();
                break;
        }
    }
}
