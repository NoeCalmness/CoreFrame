/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-01-13
 * 
 ***************************************************************************************************/

using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Window_Sex : Window, IBackToStory
{
    private GameObject m_maleHighLight;
    private GameObject m_femaleHighLight;
    private Button m_maleGrayBtn;
    private Button m_femaleGrayBtn;

    private Button m_confirmBtn;

    #region interface property
    public Action onBackToStory { get; set; }
    #endregion


    /// <summary>
    /// Called when window added to level
    /// </summary>
    protected override void OnOpen()
    {
        isFullScreen = false;
        m_maleHighLight = transform.Find("creatRolePanel/nanp").gameObject;
        m_femaleHighLight = transform.Find("creatRolePanel/nvp").gameObject;
        m_maleGrayBtn = GetComponent<Button>("creatRolePanel/nanHui");
        m_femaleGrayBtn = GetComponent<Button>("creatRolePanel/nvHui");
        m_confirmBtn = GetComponent<Button>("creatRolePanel/confirm");

        m_maleGrayBtn.onClick.RemoveAllListeners();
        m_maleGrayBtn.onClick.AddListener(SelectMan);
        m_femaleGrayBtn.onClick.RemoveAllListeners();
        m_femaleGrayBtn.onClick.AddListener(SelectWomen);
        m_confirmBtn.onClick.RemoveAllListeners();
        m_confirmBtn.onClick.AddListener(OnConfirmClick);
        InitializeText();
    }

    private void InitializeText()
    {
        var t = ConfigManager.Get<ConfigText>((int)TextForMatType.SexUIText);
        if (!t) return;

        Util.SetText(GetComponent<Text>("creatRolePanel/bg/equip_prop/top/equipinfo"), t[0]);
        Util.SetText(GetComponent<Text>("creatRolePanel/bg/equip_prop/top/english"), t[1]);
        Util.SetText(GetComponent<Text>("creatRolePanel/nanHui/text"), t[2]);
        Util.SetText(GetComponent<Text>("creatRolePanel/nanp/text"), t[2]);
        Util.SetText(GetComponent<Text>("creatRolePanel/nvHui/text"), t[3]);
        Util.SetText(GetComponent<Text>("creatRolePanel/nvp/text"), t[3]);
        Util.SetText(GetComponent<Text>("creatRolePanel/confirm/Text"), t[4]);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        SelectMan();
    }

    private void SelectMan()
    {
        SetMaleBtnVisible(true);
        SetFemaleBtnVisible();
    }

    private void SelectWomen()
    {
        SetMaleBtnVisible();
        SetFemaleBtnVisible(true);
    }

    private void SetFemaleBtnVisible(bool highLight = false)
    {
        m_femaleGrayBtn.gameObject.SetActive(!highLight);
        m_femaleHighLight.SetActive(highLight);
    }

    private void SetMaleBtnVisible(bool highLight = false)
    {
        m_maleGrayBtn.gameObject.SetActive(!highLight);
        m_maleHighLight.SetActive(highLight);
    }
    

    private void OnConfirmClick()
    {
        var t = ConfigManager.Get<ConfigText>((int)TextForMatType.AlertUIText);
        if (!t) return;
        Window_Alert.ShowAlert(t[1], true, true, true, OnConfirmCallback,null,"",t[2]);
    }

    private void OnConfirmCallback()
    {
        Hide(true);
        moduleLogin.createGender = m_maleHighLight.activeSelf ? 1 : 0;
        onBackToStory?.Invoke();
    }
}
