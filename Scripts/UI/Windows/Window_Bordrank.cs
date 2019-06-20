/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-05-07
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Window_Bordrank : Window
{
    private Button m_closeBtn;
    
    private Toggle m_personBtn;
    private Toggle m_unionBtn;
    private Text m_nameTxt;
    private GameObject m_selfdataObj;
    private GameObject m_selfdataNull;
    private GameObject m_unionPlane;
    private GameObject m_personPlane;
    private DataSource<PUnionIntegralInfo> m_unionData;
    private DataSource<BorderRankData> m_personData;
  
    protected override void OnOpen()
    {
        isFullScreen = false;
        m_closeBtn   = GetComponent<Button>("close_btn");
        
        m_closeBtn.onClick.AddListener(()=> { Hide(true); });
        moduleBordlands.QuestRankList();

        SetDataPath();
        InitializeText();
    }

    private void SetDataPath()
    {
        m_nameTxt       = GetComponent<Text>("bg/bg_frame/nicheng_text");
        m_personBtn     = GetComponent<Toggle>("checkBox/personal");
        m_unionBtn      = GetComponent<Toggle>("checkBox/union");
        m_selfdataObj   = GetComponent<RectTransform>("data_Panel/myunion_rank").gameObject;
        m_selfdataNull  = GetComponent<RectTransform>("data_Panel/myunion_null").gameObject;
        m_personPlane   = GetComponent<RectTransform>("data_Panel/person_scroll").gameObject;
        m_unionPlane    = GetComponent<RectTransform>("data_Panel/union_scroll").gameObject;

        m_personBtn.onValueChanged.AddListener(delegate { if (m_personBtn.isOn) SetPersonNormal(); });
        m_unionBtn.onValueChanged.AddListener(delegate { if (m_unionBtn.isOn) SetUnionNormal(); });

        m_personData = new DataSource<BorderRankData>(moduleBordlands.rankList, GetComponent<ScrollView>("data_Panel/person_scroll"), SetPersonIntergl);
        m_unionData = new DataSource<PUnionIntegralInfo>(moduleUnion.UnionInterList, GetComponent<ScrollView>("data_Panel/union_scroll"), SetUnionIntergl);

    }

    private void InitializeText()
    {
        var t = ConfigManager.Get<ConfigText>((int)TextForMatType.BorderRankText);
        if (!t) return;

        Util.SetText(GetComponent<Text>("bg/bg_frame/paiming_text"), t[0]);
        Util.SetText(GetComponent<Text>("bg/bg_frame/jifen_text"), t[2]);
        Util.SetText(GetComponent<Text>("bg/bg_frame/shangjin_text"), t[3]);
        Util.SetText(GetComponent<Text>("bg/titleBg/title_shadow"), t[4]);
        Util.SetText(GetComponent<Text>("bg/title"), t[4]);
        Util.SetText(GetComponent<Text>("data_Panel/myunion_null"), t[7]);
        Util.SetText(GetComponent<Text>("checkBox/personal/xz/personal_text"), t[8]);
        Util.SetText(GetComponent<Text>("checkBox/personal/Text"), t[8]);
        Util.SetText(GetComponent<Text>("checkBox/union/Text"), t[9]);
        Util.SetText(GetComponent<Text>("checkBox/union/xz/union_text"), t[9]);
        Util.SetText(GetComponent<Text>("data_Panel/union_scroll/union_tip"), t[10]);
        Util.SetText(GetComponent<Text>("data_Panel/person_scroll/person_tip"), t[12]);

    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        SetNormal();
    }
    
    private void SetNormal()
    {
        m_personBtn.isOn = true;
        m_unionBtn.isOn = false;
        
        m_selfdataObj.gameObject.SetActive(false);

        if (!moduleGuide.IsActiveFunction(HomeIcons.Guild))//公会是否解锁
            m_unionBtn.gameObject.SetActive(false);

        SetPersonNormal();
    }

    private void SetPersonNormal()
    {
        m_personPlane.gameObject.SetActive(true);
        m_unionPlane.gameObject.SetActive(false);
        m_selfdataNull.gameObject.SetActive(false);
        m_selfdataObj.gameObject.SetActive(true);
        Util.SetText(m_nameTxt, ConfigText.GetDefalutString(232, 1));
        RefreshIntergl refresh = m_selfdataObj.GetComponentDefault<RefreshIntergl>();
        refresh.SetPersonInfo(moduleBordlands.selfData, false);
        m_personData.SetItems(moduleBordlands.rankList);
    }

    private void SetUnionNormal()
    {
        m_personPlane.gameObject.SetActive(false);
        m_unionPlane.gameObject.SetActive(true);
        m_selfdataObj.gameObject.SetActive(false);
        m_selfdataNull.gameObject.SetActive(false);

        Util.SetText(m_nameTxt, ConfigText.GetDefalutString(232, 6));
        if (modulePlayer.roleInfo.leagueID == 0)
            m_selfdataNull.gameObject.SetActive(true);
        else
        {
            if (moduleUnion.m_selfInter == null) return;
            m_selfdataObj.gameObject.SetActive(true);
            RefreshIntergl refresh = m_selfdataObj.GetComponentDefault<RefreshIntergl>();
            refresh.SetUnionInfo(moduleUnion.m_selfInter,false);
            m_unionData.SetItems(moduleUnion.UnionInterList);
        }
    }
    private void SetPersonIntergl(RectTransform rt, BorderRankData info)
    {
        RefreshIntergl refresh = rt.gameObject.GetComponentDefault<RefreshIntergl>();
        refresh.SetPersonInfo(info, true);
    }

    private void SetUnionIntergl(RectTransform rt, PUnionIntegralInfo info)
    {
        RefreshIntergl refresh = rt.gameObject.GetComponentDefault<RefreshIntergl>();
        refresh.SetUnionInfo(info, true);
    }

    #region ME

    private void _ME(ModuleEvent<Module_Bordlands> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Bordlands.EventRefreshSelfRank:
                if (!m_personBtn.isOn) return;
                m_selfdataObj.gameObject.SetActive(true);
                m_selfdataNull.gameObject.SetActive(false);
                RefreshIntergl refresh = m_selfdataObj.GetComponentDefault<RefreshIntergl>();
                refresh.SetPersonInfo(moduleBordlands.selfData,false);
                break;

            case Module_Bordlands.EventRefreshRankList:
                if (!m_personBtn.isOn) return;
                m_personData.SetItems(moduleBordlands.rankList);
                break;
        }
    }
    void _ME(ModuleEvent<Module_Union> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Union.EventUnionInterglAll:
                if (!m_unionBtn.isOn) return;
                m_unionData.SetItems(moduleUnion.UnionInterList);
                break;
            case Module_Union.EventUnionInterglSelf:
                if (moduleUnion.m_selfInter == null || !m_unionBtn.isOn) return;
                m_selfdataObj.gameObject.SetActive(true);
                m_selfdataNull.gameObject.SetActive(false);
                RefreshIntergl refresh = m_selfdataObj.GetComponentDefault<RefreshIntergl>();
                refresh.SetUnionInfo(moduleUnion.m_selfInter, false);
                break;
        }
    }
    #endregion

}
