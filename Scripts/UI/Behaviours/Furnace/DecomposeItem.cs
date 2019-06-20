// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-09      11:25
//  * LastModify：2018-10-09      11:25
//  ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

public class DecomposeItem : MonoBehaviour
{
    public DecomposePair DataCache;
    public PItem         originData;

    public Action<DecomposePair, int> onChange;

    private Button       minusButton;
    private Text         numberText;
    private Transform    checkBox;
    private Transform    numberRoot;

    private ValueInputAssist valueInput;
    //初始化是忽略数值变化回调
    private bool initIgnoreChangeCallBack;

    private void Awake()
    {
        minusButton = transform.GetComponent<Button>    ("cancel_btn");
        checkBox    = transform.GetComponent<Transform> ("selcetBox");
        numberRoot  = transform.GetComponent<Transform>      ("add_number");
        numberText  = transform.GetComponent<Text>      ("add_number/Text");

        valueInput = ValueInputAssist.Create(null, transform.GetComponentDefault<Button>(), (InputField)null);
        valueInput.OnValueChange += SetCount;

        minusButton.onClick.AddListener(() =>
        {
            valueInput.SetValue(0);
            if (null != DataCache)
            {
                SetCount(0);
            }
        });
    }

    private void SetCount(int rCount)
    {
        int num = DataCache.count;
        DataCache.count = (ushort) Math.Max(0, rCount);
        DataCache.count = (ushort) Math.Min(DataCache.count, (int)originData.num);
        if (num != DataCache.count)
        {
            RefreshNumber();

            if (!initIgnoreChangeCallBack)
                onChange?.Invoke(DataCache, DataCache.count - num);
        }
    }

    public void OnItemClick()
    {
        if (null != DataCache)
        {
            SetCount((ushort) Math.Min(originData.num, DataCache.count + 1));
        }
    }

    public void RefreshNumber()
    {
        if (null == DataCache)
            return;
        Util.SetText(numberText, DataCache.count.ToString());

        minusButton .SafeSetActive(DataCache.count > 0);
        checkBox    .SafeSetActive(DataCache.count > 0);
        numberRoot  .SafeSetActive(DataCache.count > 0);
    }

    public void Init(PItem rItem, int rCount)
    {
        initIgnoreChangeCallBack = true;
        originData = rItem;
        DataCache  = new DecomposePair() {item = rItem, count = (ushort)rCount };
        valueInput.SetValue(DataCache.count, 0, (int)rItem.num);
        RefreshData();
        initIgnoreChangeCallBack = false;
    }

    private void RefreshData()
    {
        Window_Cangku.BindItemInfo(transform.parent.rectTransform(), originData);
        Util.SetText(transform.GetComponent<Text>("numberdi/count"), originData.num.ToString());
        RefreshNumber();
    }
}