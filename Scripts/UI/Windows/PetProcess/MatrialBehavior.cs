// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-06-27      17:16
//  *LastModify：2019-02-20      11:48
//  ***************************************************************************************************/

#region

using UnityEngine;
using UnityEngine.UI;

#endregion

public class MatrialBehavior : BindWidgetBehavior
{
    private Button _button;
    public int                      AddNumber;

    [Widget("add_number")]
    private Transform               addNumberRoot;

    [Widget("add_number/Text")]
    private Text                    addNumberText;

    public PItem                    ItemCache;

    [Widget("numberdi/count")]
    private Text                    numberText;

    public PropItemInfo             PropItem;

    [Widget("selectBox")]
    private Transform               selectBox;

    private void Start()
    {
        //addNumberRoot?.SafeSetActive(true);
        SetAddNumber(1);
    }

    public void Bind(PItem pItem, PropItemInfo configItem)
    {
        ItemCache = pItem;
        PropItem = configItem;
        if (pItem == null)
        {
            numberText.text = "0";
        }else
            numberText.text = pItem.num.ToString();

        transform.SetGray(pItem == null);
    }

    public void SetAddNumber(int rValue)
    {
        AddNumber = rValue;
        if(null != addNumberText)
            addNumberText.text = rValue.ToString();
    }

    public void SetSelect(bool bSelect)
    {
        selectBox.SafeSetActive(bSelect);
    }
}
