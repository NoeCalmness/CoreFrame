// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-21      16:09
//  * LastModify：2018-08-21      16:09
//  ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

public class RechargeAccumulativeTemplete : MonoBehaviour
{
    public enum State
    {
        Got,        //已领取
        CanGot,     //可领取
        Process,    //进行中
    }

    public State state;

    private Transform[] states;
    private Image       icon;
    private Text        title;
    private Text        processText;
    private Text        desc;
    private Button      detail;
    private Image       process;
    private Button      mainBody;
    private Image       toggle;

    public PTotalCostReward Data;

    public Action<RechargeAccumulativeTemplete> onClick; 
    public Action<RechargeAccumulativeTemplete> onShowDetail; 

    public bool selected { set { toggle.enabled = value; } }

    private void Awake()
    {
        states = new Transform[3];
        states[0]   = transform.GetComponent<Transform>("xiaxian/2_Txt");
        states[1]   = transform.GetComponent<Transform>("xiaxian/1_Txt");
        states[2]   = transform.GetComponent<Transform>("xiaxian/3_Txt");

        icon        = transform.GetComponent<Image>    ("icon");
        title       = transform.GetComponent<Text>     ("title_Img/title_Txt");
        processText = transform.GetComponent<Text>     ("xiaxian/3_Txt");
        desc        = transform.GetComponent<Text>     ("des_Txt");
        detail      = transform.GetComponent<Button>   ("detail");
        process     = transform.GetComponent<Image>    ("xiaxian/3_Txt/progress_Img");
        mainBody    = transform.GetComponent<Button>   ("xiaxian");
        toggle      = transform.GetComponent<Image>    ("selected");
        toggle.enabled = false;
    }

    public void InitData(PTotalCostReward item)
    {
        Data = item;
        if (Data == null) return;
        if (Module_Charge.instance.totalCost >= item.total)
            SetState(Data.draw ? State.Got : State.CanGot);
        else
            SetState(State.Process);

        if(!string.IsNullOrEmpty(Data.icon))
            AtlasHelper.SetChargeLarge(icon, Data.icon);
        Util.SetText(title, Util.Format(ConfigText.GetDefalutString(TextForMatType.RechargeUIText, 6), Data.total));
        Util.SetText(desc, Data.desc);

        detail?.onClick.RemoveAllListeners();
        detail?.onClick.AddListener(() =>
        {
            onShowDetail?.Invoke(this);
        });

        mainBody?.onClick.RemoveAllListeners();
        mainBody?.onClick.AddListener(() =>
        {
            onClick?.Invoke(this);
            selected = true;
        });
    }

    private void SetState(State rState)
    {
        state = rState;

        mainBody.SetInteractable(state == State.CanGot);

        for (var i = 0; i < states.Length; i++)
            states[i]?.SafeSetActive(false);

        if(rState >= 0 && (int)rState < states.Length)
            states[(int) rState].SafeSetActive(true);

        if (rState == State.Process)
        {
            process.fillAmount = Module_Charge.instance.totalCost/ (float)Data.total;
            Util.SetText(processText,
                        Util.Format(ConfigText.GetDefalutString(TextForMatType.RechargeUIText, 7),
                                      Data.total - Module_Charge.instance.totalCost));
        }
    }
}
