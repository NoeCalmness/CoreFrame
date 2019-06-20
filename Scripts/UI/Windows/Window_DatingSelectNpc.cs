/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-01-28
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_DatingSelectNpc : Window
{
    private Transform m_tfPNpcItems;
    private Text m_textLeftPageNum, m_textRightPageNum;
    private Button m_btnClose,m_btnPageUp,m_btnPageDown,m_btnStartDating;
    private ToggleGroup m_togGroup;
    private Transform m_tfFetterTipPanel;//好感度不足的界面提示
    private Text m_txtFetterTipContent;
    private Transform m_tfAlertPanel;//选择Npc之后的提示框
    private Text m_textAlertContent,m_textBtnConfirm,m_textBtnCancel;
    private Button m_btnAlertConfirm, m_btnAlertCancel,m_btnAlertClose;
    private Action OnConfirmCallback;
    private Action OnCancelCallback;

    private int m_curIndex = 0;
    private int m_nPerRefreshCount = 6;//每一页刷新的Npc数量
    private Dictionary<int, Module_Npc.NpcMessage> m_itemDataDic = new Dictionary<int, Module_Npc.NpcMessage>();
    private Module_Npc.NpcMessage m_curClickNpcData;
    private ConfigText m_textData = null;

    protected override void OnOpen()
    {
        isFullScreen = false;
        m_tfPNpcItems = GetComponent<RectTransform>("npcInfoGroup");
        m_togGroup = GetComponent<ToggleGroup>("npcInfoGroup");
        m_textLeftPageNum = GetComponent<Text>("pageUp/pageNumber");
        m_textRightPageNum = GetComponent<Text>("pageDown/pageNumber");
        m_btnClose = GetComponent<Button>("bg/btnClose"); m_btnClose.onClick.AddListener(HidePanel);
        m_btnPageUp = GetComponent<Button>("pageUp"); m_btnPageUp.onClick.AddListener(OnClickPageUp);
        m_btnPageDown = GetComponent<Button>("pageDown");m_btnPageDown.onClick.AddListener(OnClickPageDown);
        m_btnStartDating = GetComponent<Button>("bg/titleBg"); m_btnStartDating.onClick.AddListener(OnClickStartDating);
        m_tfFetterTipPanel = GetComponent<RectTransform>("tip_notice");
        m_txtFetterTipContent = GetComponent<Text>("tip_notice/content");

        m_tfAlertPanel = GetComponent<RectTransform>("alertPanel");
        m_textAlertContent = GetComponent<Text>("alertPanel/content");
        m_textBtnConfirm = GetComponent<Text>("alertPanel/buttons/confirm_btn/Text");
        m_textBtnCancel = GetComponent<Text>("alertPanel/buttons/cancel_btn/Text");
        m_btnAlertConfirm = GetComponent<Button>("alertPanel/buttons/confirm_btn");
        m_btnAlertCancel = GetComponent<Button>("alertPanel/buttons/cancel_btn");
        m_btnAlertClose = GetComponent<Button>("alertPanel/close_btn");
        m_btnAlertConfirm.onClick.RemoveAllListeners();
        m_btnAlertConfirm.onClick.AddListener(OnConfirmClick);
        m_btnAlertCancel.onClick.RemoveAllListeners();
        m_btnAlertCancel.onClick.AddListener(OnCancelClick);
        m_btnAlertClose.onClick.RemoveAllListeners();
        m_btnAlertClose.onClick.AddListener(OnCancelClick);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        OnClickPageDown();
    }

    protected override void OnHide(bool forward)
    {
        for (int i = 0; i < m_tfPNpcItems.childCount; i++)
        {
            m_tfPNpcItems.GetChild(i).SafeSetActive(false);
        }
    }

    private void InitTextComponent()
    {
        var textData = ConfigManager.Get<ConfigText>((int)TextForMatType.SelectDatingNpc);
        if (textData == null) return;
        Util.SetText(GetComponent<Text>("pageUp/Text"), textData[11]);
        Util.SetText(GetComponent<Text>("pageDown/Text"), textData[12]);
        Util.SetText(GetComponent<Text>("btnConfirm/Text"), textData[13]);
        Util.SetText(GetComponent<Text>("tip_notice/top/equipinfo"), textData[19]);
        Util.SetText(GetComponent<Text>("alertPanel/top/equipinfo"), textData[23]);
    }

    void _ME(ModuleEvent<Module_NPCDating> e)
    {
        if (!actived) return;
        if (e.moduleEvent == Module_NPCDating.EventChooseDatingNpc)
        {
            sbyte result = (sbyte)e.param1;
            if (result == 0)
            {
                HidePanel();//关闭窗口
                int startEventId = (int)e.param2;
                DoDatingStartEvent(startEventId);
            }
            else if (result == 3)
            {
                //当前羁绊等级无法进行约会
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.NpcDating, 8));
            }
        }
    }

    private void OnClickPageUp()
    {
        Index--;
    }

    private void OnClickPageDown()
    {
        Index++;
    }

    private int Index
    {
        get { return m_curIndex; }
        set { _Index(value); }
    }

    private void _Index(int val)
    {
        List<Module_Npc.NpcMessage> tmpDataList = moduleNpc.allNpcs;
        if (tmpDataList == null || tmpDataList.Count == 0) return;

        m_curIndex = val;

        if (m_curIndex < 1)
        {
            m_curIndex = 1;
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString((int)TextForMatType.SelectDatingNpc, 9));//第一页提示
            return;
        }

        int maxPage = Mathf.CeilToInt((float)tmpDataList.Count / m_nPerRefreshCount);
        if (m_curIndex > maxPage)
        {
            m_curIndex = maxPage;
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString((int)TextForMatType.SelectDatingNpc, 10));//最后一页提示
            return;
        }

        List<Module_Npc.NpcMessage> segmentData = SpliceNpcDataList(tmpDataList);

        RefreshData(segmentData);

        ShowPage(m_curIndex, maxPage);
    }

    /// <summary>
    /// 显示页码
    /// </summary>
    /// <param name="curPage">当前页码</param>
    /// <param name="totalPage">总页码</param>
    private void ShowPage(int curPage, int totalPage)
    {
        int leftPage = curPage * 2 - 1;
        int rightPage = curPage * 2;
        Util.SetText(m_textLeftPageNum, leftPage.ToString());
        Util.SetText(m_textRightPageNum, rightPage.ToString());
    }

    private void RefreshData(List<Module_Npc.NpcMessage> dataList)
    {
        if (dataList == null || dataList.Count == 0)
        {
            for (int i = dataList.Count; i < m_tfPNpcItems.childCount; i++)
            {
                m_tfPNpcItems.GetChild(i).SafeSetActive(false);
            }
            return;
        }
        if(m_tfPNpcItems!=null)
        {
            var component = m_tfPNpcItems.gameObject.GetComponentDefault<DynamicallyCreateObject>();
            component.haveChildCount = dataList.Count;
        }

        for (int i = m_tfPNpcItems.childCount; i < dataList.Count; i++)
        {
            GameObject templete = m_tfPNpcItems.GetChild(0).gameObject;
            templete.SafeSetActive(false);
            var toggle01 = templete.transform.GetComponent<Toggle>();
            toggle01?.onValueChanged.AddListener((isOn) => OnNpcTogValueChanged(toggle01, isOn));

            GameObject obj = GameObject.Instantiate(templete);
            obj.transform.SetParent(m_tfPNpcItems);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = templete.transform.localScale;
            var toggle = obj.transform.GetComponent<Toggle>();
            toggle?.onValueChanged.AddListener((isOn) => OnNpcTogValueChanged(toggle, isOn));
        }

        for (int i = 0; i < m_tfPNpcItems.childCount; i++)
        {
            Transform tfItem = m_tfPNpcItems.GetChild(i);
            var tog = tfItem.GetComponent<Toggle>();
            tog.group = null;
            tog.isOn = false;

            if (i <= dataList.Count - 1)
            {
                SetItemData(tfItem, dataList[i]);
                tfItem.SafeSetActive(true);
            }
            else tfItem.SafeSetActive(false);
        }
    }

    private void SetItemData(Transform node, Module_Npc.NpcMessage data)
    {
        if (data == null) return;
        PropItemInfo pinfo = new PropItemInfo();
        pinfo.ID = data.npcId;
        BaseRestrain.SetRestrainData(node.gameObject, pinfo, 0, 0);
        node.name = data.npcId.ToString();
        Text textName = node.Find("Text").GetComponent<Text>();
        Image imgRelationShip = node.Find("dateLevelBg/fill").GetComponent<Image>();
        Text textTel = node.Find("phoneNumberBg/phoneNumber").GetComponent<Text>();
        Text textDesc = node.Find("intro").GetComponent<Text>();
        Text textLabel = node.Find("labelBg/label").GetComponent<Text>();
        Image icon = node.Find("avatar_back/mask/head_icon").GetComponent<Image>();
        Image pledge = node.Find("pledgeState").GetComponent<Image>();//誓约状态
        Text textFetterStage = node.Find("goodfeelingLevel").GetComponent<Text>();
        Toggle toggle = node.GetComponent<Toggle>();
        toggle.group = m_togGroup;

        //名字
        Util.SetText(textName, data.name);
        //设置关系
        SetRelationShip(imgRelationShip, data);
        //设置Npc标签
        Util.SetText(textLabel, data.npcInfo.labelMark);
        //电话
        SetTel(textTel, data);
        //描述
        SetDesc(textDesc, data);
        //头像
        AtlasHelper.SetAvatar(icon, data.icon);
        //AtlasHelper.SetNpcDateInfo(icon, data.npcInfo.datingAvatar);
        //誓约状态
        pledge.SafeSetActive(data.fetterStage == data.maxFetterStage);

        //羁绊等级名称
        Util.SetText(textFetterStage, data.curStageName);

        int togIndex = toggle.transform.GetSiblingIndex();
        if (m_itemDataDic.ContainsKey(togIndex)) m_itemDataDic.Remove(togIndex);
        m_itemDataDic.Add(togIndex, data);
    }

    private void SetRelationShip(Image img, Module_Npc.NpcMessage data)
    {
        if (data.toFetterValue == 0) img.fillAmount = 0;
        else if (data.fetterLv >= data.maxFetterLv) img.fillAmount = 1;
        else img.fillAmount = (float)data.nowFetterValue / data.toFetterValue;
    }

    private void SetTel(Text t, Module_Npc.NpcMessage data)
    {
        if (data.isUnlockEngagement) Util.SetText(t, data.npcInfo.telephone);
        else Util.SetText(t, ConfigText.GetDefalutString(TextForMatType.SelectDatingNpc,18));
    }

    private void SetDesc(Text t, Module_Npc.NpcMessage data)
    {
        if (data.isUnlockEngagement) Util.SetText(t, data.npcInfo.descID);
        else Util.SetText(t, Util.Format(ConfigText.GetDefalutString(TextForMatType.SelectDatingNpc, 8), data.name));
    }

    private void OnNpcTogValueChanged(Toggle toggle,bool isOn)
    {
        if(!isOn) return;
        int togIndex = toggle.transform.GetSiblingIndex();
        if (m_itemDataDic.ContainsKey(togIndex)) m_curClickNpcData = m_itemDataDic[togIndex];
    }

    /// <summary>
    /// 点击正式开始约会按钮
    /// </summary>
    private void OnClickStartDating()
    {
        if(moduleNPCDating.curDatingNpc != null)
        {
            if (moduleNPCDating.curDatingNpc.datingEnd == 0) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.SelectDatingNpc,17));//正在约会中
            else moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.SelectDatingNpc, 22));//约会已经结束
            return;
        }

        if (m_curClickNpcData == null)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.SelectDatingNpc, 6));
            return;
        }
        //判断与当前Npc是否可以进行约会
        if (m_curClickNpcData.isUnlockEngagement)
        {
            if (m_curClickNpcData.fetterLv < m_curClickNpcData.maxFetterLv) OnOpenAlertPanel(StrText(0), true, true, true, Open2ConfirmCallBack, null, StrText(1), StrText(2));
            else OnOpenAlertPanel(StrText(3), true, true, true, Open1ConfirmCallBack, null, StrText(1), StrText(2));
        }
        else
        {
            m_tfFetterTipPanel.SafeSetActive(true);

            if (m_curClickNpcData.npcType != NpcTypeID.PoliceNpc) Util.SetText(m_txtFetterTipContent, StrText(21), m_curClickNpcData.name);//如果不是伊利丝，弹出不能约会提示
            else
            {
                var unLockLv = m_curClickNpcData.npcInfo.unlockLv;
                var curLv = m_curClickNpcData.fetterLv;
                var unLockExp = ConfigManager.Get<NpcGoodFeelingInfo>(unLockLv).exp;
                var curTotalExp = ConfigManager.Get<NpcGoodFeelingInfo>(curLv).exp + m_curClickNpcData.nowFetterValue;

                var dVal = unLockExp - curTotalExp;

                var curStageName = m_curClickNpcData.GetStageName(m_curClickNpcData.fetterStage);
                var unLockDatingStageName = m_curClickNpcData.GetStageName(m_curClickNpcData.datingFetterStage);

                if (dVal > 0) Util.SetText(m_txtFetterTipContent, StrText(4), m_curClickNpcData.name, unLockDatingStageName, curStageName, dVal);
                else Util.SetText(m_txtFetterTipContent, Util.Format(StrText(20), m_curClickNpcData.name));
            }

        }
    }

    #region alert
    /// <summary>
    /// 打开选择Npc之后的提示面板
    /// </summary>
    private void OnOpenAlertPanel(string content, bool showConfirm = true, bool showCancel = false, bool showClose = false, Action confirm = null, Action cancel = null, string confirmText = "", string cancelText = "")
    {
        m_tfAlertPanel.SafeSetActive(true);
        if (showCancel && cancel == null) cancel = () => { };
        m_textAlertContent.text = content.Replace("\\n", "\n");//Util.Format(@"{0}", content);
        SetButtonVisible(showConfirm, showCancel, showClose);
        SetButtonCallback(confirm, cancel);
        ChangeButtonText(confirmText, cancelText);
    }

    private void SetButtonVisible(bool confirmVisible, bool cancelVisible, bool closeVisible)
    {
        m_btnAlertConfirm.gameObject.SetActive(confirmVisible);
        m_btnAlertCancel.gameObject.SetActive(cancelVisible);
        m_btnAlertClose.gameObject.SetActive(closeVisible);
    }

    private void SetButtonCallback(Action confirmCallback, Action cancelCallback)
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

    public void SetButtonText(string confirm, string cancel)
    {
        if (m_textBtnConfirm && !string.IsNullOrEmpty(confirm)) m_textBtnConfirm.text = confirm;
        if (m_textBtnCancel && !string.IsNullOrEmpty(cancel)) m_textBtnCancel.text = cancel;
    }

    public void ChangeButtonText(string confirm, string cancel = "")
    {
        SetButtonTextDefault();
        SetButtonText(confirm, cancel);
    }

    private void OnConfirmClick()
    {
        m_tfAlertPanel.SafeSetActive(false);
        OnConfirmCallback?.Invoke();
    }

    private void OnCancelClick()
    {
        m_tfAlertPanel.SafeSetActive(false);
        OnCancelCallback?.Invoke();
    }

    #endregion

    private void Open1ConfirmCallBack()
    {
        OnOpenAlertPanel(StrText(5), true, true, true, Open2ConfirmCallBack, Open2CancleCallBack, StrText(1), StrText(2));
    }

    private void Open2ConfirmCallBack()
    {
        moduleNPCDating.SendSelectDatingNPC(m_curClickNpcData.npcId);
    }

    private void Open2CancleCallBack()
    {
        OnOpenAlertPanel(StrText(3), true, true, true, Open1ConfirmCallBack, null, StrText(1), StrText(2));
    }

    private void DoDatingStartEvent(int eventId)
    {
        moduleNPCDating.DoDatingEvent(eventId);
    }

    private void HidePanel()
    {
        Hide<Window_DatingSelectNpc>();

        m_curIndex = 0;
        m_curClickNpcData = null;
        m_itemDataDic.Clear();
        m_tfFetterTipPanel.SafeSetActive(false);
    }

    #region tools
    private string StrText(int index)
    {
        if (m_textData == null) m_textData = ConfigManager.Get<ConfigText>((int)TextForMatType.SelectDatingNpc);
        return m_textData == null ? "" : m_textData[index];
    }

    private List<Module_Npc.NpcMessage> SpliceNpcDataList(List<Module_Npc.NpcMessage> allDataList)
    {
        int startIndex = (m_curIndex - 1) * m_nPerRefreshCount;
        int count = allDataList.Count - startIndex < m_nPerRefreshCount ? allDataList.Count - startIndex : m_nPerRefreshCount;
        return allDataList.GetRange(startIndex, count);
    }
    #endregion

}
