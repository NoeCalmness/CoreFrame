// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-10      14:11
//  * LastModify：2018-08-10      14:11
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class AwakeWindow_Match : SubWindowBase
{
    private Text countDownText;
    private Button cancel;

    private float overTime;
    private int countTime;

    protected override void InitComponent()
    {
        base.InitComponent();
        countDownText   = WindowCache.GetComponent<Text>    ("waiting_panel/ditu/dountdown_Txt");
        cancel          = WindowCache.GetComponent<Button>  ("waiting_panel/back");
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;
        overTime = System.Convert.ToSingle(p[0]);
        countTime = 0;
        cancel?.onClick.AddListener(OnClickCancel);

        return true;
    }

    private void OnClickCancel()
    {
        moduleAwakeMatch.Request_CancelMatch();
    }

    public override void OnRootUpdate(float diff)
    {
        if (overTime >= Time.realtimeSinceStartup)
        {
            var t = Mathf.CeilToInt(overTime - Time.realtimeSinceStartup);
            if (t != countTime)
            {
                countTime = t;
                Util.SetText(countDownText, Util.Format(ConfigText.GetDefalutString((int)TextForMatType.AwakeStage, 15), countTime));
            }
        }
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;
        cancel?.onClick.RemoveAllListeners();
        countTime = 0;
        return true;
    }
}