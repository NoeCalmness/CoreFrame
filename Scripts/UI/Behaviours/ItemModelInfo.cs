// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-11      16:36
//  * LastModify：2018-10-11      16:36
//  ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemModelInfo : MonoBehaviour
{
    public UICharacter  uiCharacter;
    public new Text     name;
    public Text         level;
    public Transform    equiped;
    public Transform[]  stars;

    private readonly List<Image> typeIcons = new List<Image>();

    private UIModel     model;

    private void Awake()
    {
        transform.GetComponent<RawImage>("RawImage")?.SafeSetActive(true);

        uiCharacter = transform.GetComponent<UICharacter>("RawImage");
        name        = transform.GetComponent<Text>       ("base/name");
        level       = transform.GetComponent<Text>       ("base/name/level_txt");
        equiped     = transform.GetComponent<Transform>  ("base/equiped_img");


        var typeIconRoot = transform.GetComponent<Transform>("base/icon");
        var iconNames = new string[]
        {
            "sword", "katana", "axe", "fist", "pistol", "suit"
        };
        for (var i = 0; i < iconNames.Length; i++)
            typeIcons.Add(typeIconRoot.GetComponent<Image>(iconNames[i]));

        model = uiCharacter.GetComponentDefault<UIModel>();

        var starGroup = transform.GetComponent<Transform>("base/qualityGrid");
        stars = starGroup.GetChildList().ToArray();
    }

    public void Init(PItem rItem, bool rIsDressOn)
    {
        var prop = rItem?.GetPropItem();
        if (null == prop)
            return;

        Util.SetText(name, prop.itemName);
        Util.SetText(level, $"+{rItem.growAttr.equipAttr.level}");
        if (stars?.Length > 0)
        {
            for (var i = 0; i < stars.Length; i++)
                stars[i].SafeSetActive(i < rItem.growAttr.equipAttr.star);
        }

        Util.SetEquipTypeIcon(typeIcons, prop.itemType, prop.subType);
        model ?.LoadItemModel(rItem);
        equiped.SafeSetActive(rIsDressOn);
    }
}
