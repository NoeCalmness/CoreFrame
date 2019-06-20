/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-08-14
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System;

public class Window_Alert : Window
{
    #region 静态方法
    public static void ShowAlertDefalut(string content,Action confirm = null, Action cancel = null, string confirmText = "", string cancelText = "", bool showClose = true)
    {
        ShowAlert(content,confirm != null, cancel != null, showClose, confirm, cancel, confirmText, cancelText);
    }

    public static void ShowAlert(string content, bool showConfirm = true, bool showCancel = false,bool showClose = false, Action confirm = null, Action cancel = null, string confirmText = "", string cancelText = "")
    {
        ShowImmediatelyAsync("window_alert", null, window =>
        {
            Window_Alert alert = window as Window_Alert;
            if (alert == null)
            {
                //打开错误的时候关闭界面
                if (window != null) window.gameObject.SetActive(false);
                return;
            }
            if (showCancel && cancel == null) cancel = () => { }; 
            alert.content.text = content.Replace("\\n", "\n");//Util.Format(@"{0}", content);
            alert.SetButtonVisible(showConfirm, showCancel,showClose);
            alert.SetButtonCallback(confirm, cancel);
            alert.ChangeButtonText(confirmText,cancelText);
        });
    }

    #endregion

    public Text content { get; private set; }

    private Button m_cancelBtn;
    private Text m_cancelText;
    private Button m_confirmBtn;
    private Text m_confirmText;
    private Button m_closeBtn;
    private Action OnConfirmCallback;
    private Action OnCancelCallback;

    protected override void OnOpen()
    {
        isFullScreen = false;
        content                 = GetComponent<Text>("content");
        m_closeBtn              = GetComponent<Button>("close_btn");
        m_confirmBtn            = GetComponent<Button>("buttons/confirm_btn");
        m_cancelBtn             = GetComponent<Button>("buttons/cancel_btn");
        m_confirmText           = GetComponent<Text>("buttons/confirm_btn/Text");
        m_cancelText            = GetComponent<Text>("buttons/cancel_btn/Text");

        m_confirmBtn.onClick.RemoveAllListeners();
        m_confirmBtn.onClick.AddListener(OnConfirmClick);
        m_cancelBtn.onClick.RemoveAllListeners();
        m_cancelBtn.onClick.AddListener(OnCancelClick);
        m_closeBtn.onClick.RemoveAllListeners();
        m_closeBtn.onClick.AddListener(OnCancelClick);
    }

    private void OnConfirmClick()
    {
        Hide(true);
        OnConfirmCallback?.Invoke();
    }

    private void OnCancelClick()
    {
        Hide(true);
        OnCancelCallback?.Invoke();
    }

    public void SetButtonVisible(bool confirmVisible, bool cancelVisible,bool closeVisible)
    {
        m_confirmBtn.gameObject.SetActive(confirmVisible);
        m_cancelBtn.gameObject.SetActive(cancelVisible);
        m_closeBtn.gameObject.SetActive(closeVisible);
    }

    public void SetButtonCallback(Action confirmCallback,Action cancelCallback)
    {
        OnConfirmCallback = confirmCallback;
        OnCancelCallback = cancelCallback;
    }

    public void SetButtonTextDefault()
    {
        var t = ConfigManager.Get<ConfigText>((int)TextForMatType.PublicUIText);
        if (!t) return;
        
        SetButtonText(t[4], t[5]);
    }

    public void SetButtonText(string confirm,string cancel)
    {
        if (m_confirmText && !string.IsNullOrEmpty(confirm)) m_confirmText.text = confirm;
        if (m_cancelText && !string.IsNullOrEmpty(cancel)) m_cancelText.text = cancel;
    }

    public void ChangeButtonText(string confirm, string cancel = "")
    {
        SetButtonTextDefault();
        SetButtonText(confirm,cancel);
    }
}
