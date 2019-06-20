// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-04-19      15:26
//  *LastModify：2019-04-19      15:26
//  ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

public class RechargeDailySign : AssertOnceBehaviour
{
    public enum State
    {
        Got,        //已领取
        CanGot,     //可领取
        Process,    //进行中
        Overdue,       //已过期

        Max
    }
    public State state;

    private PChargeItem m_chargeItem;
    private PChargeDailyOneInfo m_info;
    private int m_day;

    private Transform[] states;
    private Button recieveButton;
    private Button detailButton;
    private Text title;
    private Text desc;
    private Image bigIcon;
    private Image toggle;

    public Action<RechargeDailySign> OnRecieve;
    public Action<RechargeDailySign> OnShowDetail;
    public Action<RechargeDailySign> onSelect;

    public PChargeDailyOneInfo Info { get { return m_info; } }
    public bool selected { set { if (value) onSelect?.Invoke(this); toggle.enabled = value; } }

    protected override void Init()
    {
        states = new Transform[(int)State.Max];
        states[(int)State.Got]     = transform.GetComponent<Transform>("xiaxian/2_Txt");
        states[(int)State.CanGot]  = transform.GetComponent<Transform>("xiaxian/1_Txt");
        states[(int)State.Process] = transform.GetComponent<Transform>("xiaxian/3_Txt");
        states[(int)State.Overdue] = transform.GetComponent<Transform>("xiaxian/3_Txt");

        recieveButton   = transform.GetComponent<Button>("xiaxian");
        detailButton    = transform.GetComponent<Button>("detail");
        bigIcon         = transform.GetComponent<Image>("icon");
        title           = transform.GetComponent<Text>("title_Img/title_Txt");
        desc            = transform.GetComponent<Text>("des_Txt");
        toggle          = transform.GetComponent<Image>("selected");

        toggle.enabled = false;
        var b = GetComponent<Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() => selected = true);

        recieveButton.onClick.AddListener(() => Module_Charge.instance.RequestGetSign(m_chargeItem.productId, m_info.id));
        detailButton.onClick.AddListener(() => OnShowDetail?.Invoke(this));
    }

    private void SetState(State rState)
    {
        state = rState;

        recieveButton?.SetInteractable(rState == State.CanGot);

        switch (rState)
        {
            case State.Overdue:
                Util.SetText(states[(int) State.Overdue].gameObject, ConfigText.GetDefalutString(9856, 4));
                break;
            case State.Got:
                Util.SetText(states[(int) State.Got].gameObject, ConfigText.GetDefalutString(9856, 0));
                break;
            case State.CanGot:
                Util.SetText(states[(int) State.CanGot].gameObject, ConfigText.GetDefalutString(9856, 1));
                break;
            case State.Process:
                Util.SetText(states[(int) State.Process].gameObject, m_day + 1 == m_info.id  ? ConfigText.GetDefalutString(9856, 2) : Util.Format(ConfigText.GetDefalutString(9856, 3), m_info.id - m_day - 1));
                break;
        }

        for (var i = 0; i < states.Length; i++)
            states[i]?.SafeSetActive(false);

        if (rState >= 0 && (int)rState < states.Length)
            states[(int)rState].SafeSetActive(true);
    }

    public void InitData(PChargeItem rChargeItem, PChargeDailyOneInfo rInfo, int rDay)
    {
        AssertInit();

        m_chargeItem = rChargeItem;
        m_info = rInfo;
        m_day = rDay;

        Refresh();
    }

    private void Refresh()
    {
        if (m_day > m_info.id)
            SetState(m_info.draw ? State.Got : State.Overdue);
        else if ( m_day == m_info.id)
            SetState(m_info.draw ? State.Got : State.CanGot);
        else
            SetState(State.Process);

        if (!string.IsNullOrEmpty(m_info.icon))
            AtlasHelper.SetChargeLarge(bigIcon, m_info.icon);

        Util.SetText(title, Util.Format(ConfigText.GetDefalutString(TextForMatType.RechargeUIText, 25), m_info.id));
        Util.SetText(desc, m_info.desc);
    }
}
