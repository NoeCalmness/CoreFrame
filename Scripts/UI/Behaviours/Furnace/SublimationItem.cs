// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-09      14:10
//  * LastModify：2018-10-09      14:10
//  ***************************************************************************************************/

using UnityEngine;

public class SublimationItem : MonoBehaviour
{
    public PItem OriginData;

    public void Init(PItem rItem, bool isEquiped)
    {
        OriginData = rItem;
        RefreshData(isEquiped);
    }

    public void RefreshData(bool isEquiped)
    {
        Util.SetItemInfo(transform, OriginData.GetPropItem(), OriginData.growAttr.equipAttr.level, (int)OriginData.num, true, OriginData.growAttr.equipAttr.star);
        transform.GetComponent<Transform>("equiped").SafeSetActive(isEquiped);
    }
}
