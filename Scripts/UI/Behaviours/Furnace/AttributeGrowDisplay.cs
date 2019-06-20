// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-10      15:11
//  * LastModify：2018-10-10      15:11
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class AttributeGrowDisplay : MonoBehaviour
{
    public Text attributeName;
    public Text leftValue;

    private void Awake()
    {
        attributeName   = transform.GetComponent<Text>();
        leftValue       = transform.GetComponent<Text>("left_txt");
    }

    public void Init(PItemAttachAttr rAttribute)
    {
        Util.SetText(attributeName, rAttribute.TypeString());
        Util.SetText(leftValue, rAttribute.ValueString());
    }
}
