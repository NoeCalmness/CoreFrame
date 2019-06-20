/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Used for wish window.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-02-028
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("HYLR/UI/Wish Item Info Normal")]
public class WishItemInfoNormal : WishItemInfo
{
    public override void UpdateTexts()
    {
        var t = ConfigManager.Get<ConfigText>(9404);
        if (!t) return;

        Util.SetText(gameObject.GetComponent<Text>("back/notice"), t[0]);
        Util.SetText(gameObject.GetComponent<Text>("item/title/up"), t[1]);
    }

    public override void UpdateItemInfo()
    {
        Util.SetItemInfo(transform.Find("item"), itemInfo, m_item.level, (int)m_item.num);
    }
}
