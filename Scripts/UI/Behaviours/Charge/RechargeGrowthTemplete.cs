// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-21      11:01
//  * LastModify：2018-08-21      11:01
//  ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

public class RechargeGrowthTemplete : MonoBehaviour
{

    public enum State
    {
        Got,        //已领取
        CanGot,     //可领取
        Process,    //进行中
    }
    public State state;

    private Transform[] states;
    private Button      recieveButton;
    private Button      detailButton;
    private Text        title;
    private Text        desc;
    private Image       bigIcon;
    private Image       toggle;

    public Action<RechargeGrowthTemplete> OnRecieve;
    public Action<RechargeGrowthTemplete> OnShowDetail;

    public PGrowthFund Data;
    public Action<RechargeGrowthTemplete> onSelect;
    public bool selected { set { if (value) onSelect?.Invoke(this); toggle.enabled = value; } }

    private bool isInited = false;
    private void Init()
    {
        if (isInited) return;
        isInited = true;

        states = new Transform[3];
        states[0] = transform.GetComponent<Transform>("xiaxian/2_Txt");
        states[1] = transform.GetComponent<Transform>("xiaxian/1_Txt");
        states[2] = transform.GetComponent<Transform>("xiaxian/3_Txt");

        recieveButton   = transform.GetComponent<Button>  ("xiaxian");
        detailButton    = transform.GetComponent<Button>  ("detail");
        bigIcon         = transform.GetComponent<Image>   ("icon");
        title           = transform.GetComponent<Text>    ("title_Img/title_Txt");
        desc            = transform.GetComponent<Text>    ("des_Txt");
        toggle          = transform.GetComponent<Image>   ("selected");

        toggle.enabled = false;
        var b = GetComponent<Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() => selected = true);

        recieveButton.onClick.AddListener(() => Module_Charge.instance.RequestGetGrowthFund(Data.id));
        detailButton.onClick.AddListener(() => OnShowDetail?.Invoke(this));
    }

    public void InitData(PGrowthFund item)
    {
        Init();

        Data = item;
        if (Data == null) return;

        if (Module_Player.instance.level >= item.level)
            SetState(Data.draw ? State.Got : State.CanGot);
        else
            SetState(State.Process);

        if(!string.IsNullOrEmpty(Data.icon))
            AtlasHelper.SetChargeLarge(bigIcon, Data.icon);

        Util.SetText(title, Util.Format(ConfigText.GetDefalutString(TextForMatType.RechargeUIText, 1), Data.level));
        Util.SetText(desc, Data.desc);
    }

    private void SetState(State rState)
    {
        state = rState;

        recieveButton?.SetInteractable(rState == State.CanGot);

        for (var i = 0; i < states.Length; i++)
            states[i]?.SafeSetActive(false);

        if (rState >= 0 && (int)rState < states.Length)
            states[(int)rState].SafeSetActive(true);
    }
}
