using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BannerSetInfo : MonoBehaviour {

    private RectTransform m_banImg;
    private Button m_banBtn;
    private BannerInfo m_info;

    private void GetPath()
    {
        m_banImg = transform.GetComponentDefault<RectTransform>();
        m_banBtn = transform.GetComponentDefault<Button>();
    }

    public void SetBannerInfo(BannerInfo info)
    {
        if (info == null) return;
        m_info = info;
        GetPath();
        UIDynamicImage.LoadImage(m_banImg, info.picture);
        m_banBtn.onClick.RemoveAllListeners();
        m_banBtn.onClick.AddListener(BtnOnClick);
    }

    private void BtnOnClick()
    {
        if (m_info == null) return;
        string drop = m_info.turnPage;
        int index = -1;
        if (m_info.turnPage.Contains("-"))
        {
            drop = m_info.turnPage.Split('-')[0];
            index = Util.Parse<int>(m_info.turnPage.Split('-')[1]);
        }
        Module_Announcement.instance.OpenWindow(Util.Parse<int>(drop), index);
    }
}
