/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-05-08
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Announcement : Window
{
    private GameObject Infoplane;

    private GameObject Notice_item;//公告
    private GameObject Active_item;//活动

    private Toggle Active_btn;
    private Toggle Annount_btn;
    private Image Active_hint;//活动小红点
    private Image Annount_hint;//公告小红点

    private Image Title_back_img;
    private Text Subtitle_txt;
    private Chathyperlink Contxt_one;
    private Image ConImg;
    private Text conTmgTxt;
    private Chathyperlink Contxt_two;
    private Button Go_btn;

    private ConfigText anmounttext;

    private RectTransform m_rectContent;

    private DataSource<PBulletin> m_activeInfo;//活动提醒
    private DataSource<PBulletin> m_noticeInfo;//公告提醒
    
    protected override void OnOpen()
    {
        anmounttext = ConfigManager.Get<ConfigText>((int)TextForMatType.NoticeUIText);
        if (anmounttext == null)
        {
            anmounttext = ConfigText.emptey;
            Logger.LogError("this id can not");
        }
        SetText();

        m_rectContent = GetComponent<RectTransform>("right_iteminfo_panel/Scroll View/Viewport/content");

        Infoplane = GetComponent<RectTransform>("right_iteminfo_panel").gameObject;
        Notice_item = GetComponent<RectTransform>("Notic_ScrollView").gameObject;
        Active_item = GetComponent<RectTransform>("Active_ScrollView").gameObject;
        Active_btn = GetComponent<Toggle>("right_labelgroup/active");
        Annount_btn = GetComponent<Toggle>("right_labelgroup/announcement");
        Active_hint = GetComponent<Image>("right_labelgroup/active/new_img");
        Annount_hint = GetComponent<Image>("right_labelgroup/announcement/new_img");
        Title_back_img = GetComponent<Image>("right_iteminfo_panel/Scroll View/Viewport/content/title_bg_img");
        Subtitle_txt = GetComponent<Text>("right_iteminfo_panel/Scroll View/Viewport/content/subtitle_text");
        Contxt_one = GetComponent<Chathyperlink>("right_iteminfo_panel/Scroll View/Viewport/content/content_text_1");
        ConImg = GetComponent<Image>("right_iteminfo_panel/Scroll View/Viewport/content/noticecontent_img");
        conTmgTxt = GetComponent<Text>("right_iteminfo_panel/Scroll View/Viewport/content/title_bg_img/bg_txt");
        Contxt_two = GetComponent<Chathyperlink>("right_iteminfo_panel/Scroll View/Viewport/content/content_txt2");
        Go_btn = GetComponent<Button>("right_iteminfo_panel/go_btn");
        ConImg.SafeSetActive(false);
        Contxt_two.SafeSetActive(false);

        Active_btn.onValueChanged.AddListener(delegate
        {
            if (Active_btn.isOn) SetBtnClick(0, moduleAnnouncement.ActiveList);
        });
        Annount_btn.onValueChanged.AddListener(delegate
        {
            if (Annount_btn.isOn) SetBtnClick(1, moduleAnnouncement.NoticeList);
        });
        m_activeInfo = new DataSource<PBulletin>(moduleAnnouncement.ActiveList, GetComponent<ScrollView>("Active_ScrollView"), SetAllInfo, Onclick);
        m_noticeInfo = new DataSource<PBulletin>(moduleAnnouncement.NoticeList, GetComponent<ScrollView>("Notic_ScrollView"), SetAllInfo, Onclick);

        HintShow();
    }

    private void SetText()
    {
        Util.SetText(GetComponent<Text>("backgrounds/title_txt"), anmounttext[0]);
        Util.SetText(GetComponent<Text>("right_labelgroup/active/active_text"), anmounttext[1]);
        Util.SetText(GetComponent<Text>("right_labelgroup/announcement/announcement_text"), anmounttext[0]);
        Util.SetText(GetComponent<Text>("right_iteminfo_panel/go_btn/Text"), anmounttext[2]);
    }

    private void SetBtnClick(int type, List<PBulletin> annpunceList)
    {
        if (type == 0) m_activeInfo.SetItems(annpunceList);
        else if (type == 1) m_noticeInfo.SetItems(annpunceList);

        m_rectContent.anchoredPosition = Vector3.zero;
        Active_item.gameObject.SetActive(type == 0);
        Notice_item.gameObject.SetActive(type == 1);

        if (annpunceList.Count > 0) ChangeList(annpunceList[0]);
        else
        {
            moduleAnnouncement.PInfo = null;
            Infoplane.gameObject.SetActive(false);
        }
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();//top工具条的显示

        if (Active_btn.isOn) SetBtnClick(0, moduleAnnouncement.ActiveList);
        else Active_btn.isOn = true;
    }

    private void SetAllInfo(RectTransform rt, PBulletin Info)
    {
        if (Info == null) return;
        TweenColor txtmain = rt.transform.Find("txt/TXT").GetComponent<TweenColor>();
        var txtother = rt.transform.Find("txt/TXT2");
        Text Title1 = txtmain?.GetComponent<Text>();
        Text Title2 = txtother?.GetComponent<Text>();

        if (Title1 != null) Util.SetText(Title1, Info.title);
        if (Title2 != null && txtother != null)
        {
            txtother.gameObject.SetActive(!string.IsNullOrEmpty(Info.subTitle));
            Util.SetText(Title2, Info.subTitle);
        }

        GameObject SelectOn = rt.transform.Find("selectbox")?.gameObject;
        Image NewImg = rt.transform.Find("new_img")?.GetComponent<Image>();
        bool isshow = moduleAnnouncement.NotClick.Exists(a => a == Info.id);

        NewImg.SafeSetActive(isshow);
        var select = Info.id == moduleAnnouncement.PInfo?.id;
        SelectOn?.gameObject.SetActive(select);

        if (select) txtmain.Play();
        else txtmain.PlayReverse();

    }

    private void Onclick(RectTransform rt, PBulletin Info)
    {
        if (Info == null) return;
        m_rectContent.anchoredPosition = Vector3.zero;
        ChangeList(Info);
    }
    
    private void ChangeList(PBulletin Info)
    {
        if (Info == null) return;

        Infoplane.gameObject.SetActive(true);
        PBulletin Last = moduleAnnouncement.PInfo;
        moduleAnnouncement.PInfo = Info;
        string date_now = Util.GetServerLocalTime().ToString();
        string idm = "ment" + Info.id.ToString() + modulePlayer.roleInfo.roleId.ToString();
        PlayerPrefs.SetString(idm, date_now);

        bool isshow = moduleAnnouncement.NotClick.Exists(a => a == Info.id);
        if (isshow) moduleAnnouncement.NotClick.Remove(Info.id);

        if (Info.type == 0) UpdateInfo(Info, moduleAnnouncement.ActiveList, m_activeInfo);
        else if (Info.type == 1) UpdateInfo(Info, moduleAnnouncement.NoticeList, m_noticeInfo);
        if (Last != null)
        {
            UpdateInfo(Last, moduleAnnouncement.ActiveList, m_activeInfo);
            UpdateInfo(Last, moduleAnnouncement.NoticeList, m_noticeInfo);
        }

        SetPlaneInfo(Info);
        HintShow();
    }
    private void SetPlaneInfo(PBulletin info)//获取所有信息 传进plane 显示具体信息
    {
        if (info == null) return;
        foreach (Transform item in Title_back_img.transform)
        {
            item.SafeSetActive(false);
        }
        UIDynamicImage.LoadImage(Title_back_img.transform, info.icon, null, true);

        conTmgTxt.SafeSetActive(!string.IsNullOrEmpty(info.icon));
        Util.SetText(Subtitle_txt, string.Format("  {0}", info.conTitle));
        Util.SetText(conTmgTxt, info.iconDesc);
        Contxt_one.text = info.content;
        Contxt_one.Set();
        RectTransform rt = conTmgTxt.GetComponentDefault<RectTransform>();
        rt.anchoredPosition  = new Vector3(info.positionX, info.positionY, 0);

        var openWindow = -1;
        var openLable = -1;
        if (!info.turnPage.Contains("-"))
        {
            var type = Util.Parse<int>(info.turnPage);
            Go_btn.gameObject.SetActive(type > 0);
            openWindow = Util.Parse<int>(info.turnPage);
        }
        else
        {
            Go_btn.SafeSetActive(true);
            string[] str = info.turnPage.Split('-');
            openWindow = Util.Parse<int>(str[0]);
            openLable = Util.Parse<int>(str[1]);
        }
        Go_btn.onClick.RemoveAllListeners();
        Go_btn.onClick.AddListener(delegate
        {
            moduleAnnouncement.OpenWindow(openWindow, openLable);
        });

    }

    private void HintShow()//活动 公告两个页签小红点
    {
        Active_hint.SafeSetActive(false);
        Annount_hint.SafeSetActive(false);
        for (int i = 0; i < moduleAnnouncement.NotClick.Count; i++)
        {
            var active = moduleAnnouncement.ActiveList.Find(a => a.id == moduleAnnouncement.NotClick[i]);
            Active_hint.SafeSetActive(active != null);

            var annount = moduleAnnouncement.NoticeList.Find(a => a.id == moduleAnnouncement.NotClick[i]);
            Annount_hint.SafeSetActive(annount != null);
        }
    }

    private void UpdateInfo(PBulletin Info, List<PBulletin> ListInfo, DataSource<PBulletin> m_typeData)
    {
        if (Info == null) return;
        var index = ListInfo.FindIndex(a => a.id == Info.id);
        m_typeData.UpdateItem(index);
    }

    #region _ME

    void _ME(ModuleEvent<Module_Announcement> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Announcement.EventAccAllInfo:
                if (Active_btn.isOn) SetNewInfo(moduleAnnouncement.ActiveList, 0);
                else if (Annount_btn.isOn) SetNewInfo(moduleAnnouncement.NoticeList, 1);
                break;
            case Module_Announcement.EventAccNewNotice:
                var newType = Util.Parse<int>(e.param1.ToString());
                SetAllItem(newType);
                break;
            case Module_Announcement.EventAccCloseNotice:
                int type = Util.Parse<int>(e.param1.ToString());
                SetAllItem(type);
                HintShow();
                break;
        }
    }
    #endregion 

    private void SetAllItem(int type)
    {
        if (type == 0 && Active_btn.isOn) SetNewInfo(moduleAnnouncement.ActiveList, 0);
        else if (type == 1 && Annount_btn.isOn) SetNewInfo(moduleAnnouncement.NoticeList, 1);
    }
    private void SetNewInfo(List<PBulletin> bull, int type)
    {
        Infoplane.SafeSetActive(false);
        if (bull.Count > 0)
        {
            if (moduleAnnouncement.PInfo == null)
            {
                moduleAnnouncement.PInfo = bull[0];
                ChangeList(moduleAnnouncement.PInfo);
            }
            Infoplane.SafeSetActive(true);
        }
        if (type == 0) m_activeInfo.SetItems(bull);
        else if (type == 1) m_noticeInfo.SetItems(bull);
    }

    protected override void OnReturn()
    {
        base.OnReturn();
        moduleHome.UpdateIconState(HomeIcons.Notice, false);
    }

    private void SetOtherTxt(string content)
    {
        string[] split = new string[1] { "[sprit=" };
        string[] txt = content.Split(split, StringSplitOptions.RemoveEmptyEntries);
        if (txt.Length > 1)
        {
            Contxt_one.text = txt[0];
            Contxt_one.Set();

            string[] txt1 = txt[1].Split(']');
            if (txt1.Length > 1)
            {
                Contxt_two.text = txt1[1];
                Contxt_two.Set();
            }

            string Imgname = txt1[0];
            if (Imgname == "0")
            {
                ConImg.gameObject.SetActive(false);
            }
            else
            {
                string[] NameId = Imgname.Split(':');
                ConImg.gameObject.SetActive(true);
                AtlasHelper.SetIcons(ConImg, NameId[0]);
                if (NameId.Length > 1)
                {
                    Button Click = ConImg.gameObject.GetComponentDefault<Button>();
                    Click.onClick.AddListener(delegate
                    {
                        moduleGlobal.UpdateGlobalTip(ushort.Parse(NameId[1]), true);
                    });
                }
            }
        }

    }
    
}
