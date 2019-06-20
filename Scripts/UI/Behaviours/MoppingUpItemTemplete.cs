// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-11-20      10:14
//  *LastModify：2018-11-20      10:14
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class MoppingUpItemTemplete : MonoBehaviour
{
    private ScrollView scrollView;
    private Text      exp;
    private Text      coin;
    private Transform targetRoot;
    private Transform targetIcon;
    private Text      targetProcess;
    private Text      times;

    private DataSource<PItem2> dataSource;


    private bool isInited;
    private void InitComponent()
    {
        if (isInited) return;
        isInited = true;
        scrollView      = transform.GetComponent<ScrollView>("rewardList");
        exp             = transform.GetComponent<Text>("getExp_Text/numebr");
        coin            = transform.GetComponent<Text>("getGold_Text/numebr");
        targetRoot      = transform.GetComponent<Transform>("target");
        targetIcon      = transform.GetComponent<Transform>("target/item");
        targetProcess   = transform.GetComponent<Text>("target/need_txt/number_txt");
        times           = transform.GetComponent<Text>("count/number");
    }

    public void Init(AssistWindow_MoppingUp.Entry data)
    {
        InitComponent();
        if (dataSource == null)
            dataSource = new DataSource<PItem2>(data.reward.rewardList, scrollView, OnSetData, OnItemClick);
        else
            dataSource.SetItems(data.reward.rewardList);

        Util.SetText(exp, data.reward.expr.ToString());
        Util.SetText(coin, data.reward.coin.ToString());
        Util.SetText(times, Util.Format(ConfigText.GetDefalutString(TextForMatType.MoppingUpUI, 9), data.times));

        if (Module_Global.instance.targetMatrial.isProcess && data.getCount > 0)
        {
            targetRoot.SafeSetActive(true);
            Util.SetItemInfo(targetIcon, ConfigManager.Get<PropItemInfo>(Module_Global.instance.targetMatrial.itemId), 0, data.getCount, false);
            Util.SetText(targetProcess, $"{data.nowMatrialCount}/{data.targetMatrialCount}");
            if(targetProcess)
                targetProcess.color = ColorGroup.GetColor(ColorManagerType.IsMatrialEnough, data.nowMatrialCount >= data.targetMatrialCount);
            targetIcon?.GetComponentDefault<Button>()?
                .onClick.AddListener(
                    () => Module_Global.instance.UpdateGlobalTip((ushort)Module_Global.instance.targetMatrial.itemId, false, false));
        }
        else
            targetRoot.SafeSetActive(false);
    }

    private void OnItemClick(RectTransform node, PItem2 data)
    {
         Module_Global.instance.UpdateGlobalTip(data, false);
    }

    private void OnSetData(RectTransform node, PItem2 data)
    {
        Util.SetItemInfo(node, ConfigManager.Get<PropItemInfo>(data.itemTypeId), 0, (int)data.num, false);
    }
}
