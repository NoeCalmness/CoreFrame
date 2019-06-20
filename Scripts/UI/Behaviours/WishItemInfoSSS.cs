/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Used for wish window
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-02-28
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("HYLR/UI/Wish Item Info SSS")]
public class WishItemInfoSSS : WishItemInfo
{
    public Transform AttRoot;
    public GameObject AttTemplete;

    public override void UpdateTexts()
    {
        var t = ConfigManager.Get<ConfigText>(9405);
        if (!t) return;

        Util.SetText(gameObject.GetComponent<Text>("back/notice"), t[0]);
        Util.SetText(gameObject.GetComponent<Text>("title"), t[1]);
    }

    public override void UpdateItemInfo()
    {
        Transform n = transform.Find("item"), s = n.Find("stars");
        Util.SetText(gameObject.GetComponent<Text>("item/name"), 9405, m_item.num > 0 ? 6 : 5, itemInfo ? itemInfo.itemName : "null", m_item.num);
        Util.SetItemInfo(n, itemInfo, item.level, (int)item.num, true, item.star);

        n.Find("sprite").gameObject.SetActive(false);
        n.Find("rune").gameObject.SetActive(false);

        if (itemInfo)
        {
            var it = itemInfo.itemType;
            var idx = it == PropType.Rune || it == PropType.FashionCloth ? 1 : 0;
            var spriteName = idx >= itemInfo.mesh.Length ? null : itemInfo.mesh[idx];

            if (it == PropType.Rune)
            {
                var shadowSprite = itemInfo.mesh.Length > 7 ? itemInfo.mesh[6] : null;
                UIDynamicImage.LoadImage(spriteName, t =>
                {
                    n.Find("rune").SafeSetActive(t);
                }, false, n.Find("rune"));

                UIDynamicImage.LoadImage( shadowSprite, t =>
                {
                    n.Find("spriteShadow").SafeSetActive(t);
                }, false, n.Find("spriteShadow"));
            }
            else
            {
                UIDynamicImage.LoadImage(spriteName, t =>
                {
                    n.Find("sprite").SafeSetActive(t);
                }, false, n.Find("sprite"));

                AtlasHelper.SetShared(n.Find("spriteShadow"), "ui_wish_" + Module_Player.instance.proto, t =>
                {
                    n.Find("spriteShadow").SafeSetActive(true);
                });
            }
        }

        var qq = itemInfo ? item.star : 0;
        for (int i = 0; i < s.childCount; ++i)
        {
            var sn = s.Find(i.ToString());
            if (sn) sn.gameObject.SetActive(i < qq);
        }

        Util.ClearChildren(AttRoot);
        for (var i = 0; i < itemInfo.attributes.Length; i++)
        {
            var t = AttRoot.AddNewChild(AttTemplete);
            Util.SetText(t.GetComponent<Text>("name"), itemInfo.attributes[i].TypeString());
            Util.SetText(t.GetComponent<Text>("number"), itemInfo.attributes[i].ValueString());
            t.SafeSetActive(true);
        }
        //没有属性才显示描述
        var desNode = n.GetComponent<Transform>("des");
        if (itemInfo.attributes.Length == 0)
        {
            Util.SetText(desNode?.GetComponent<Text>("txt"), itemInfo.desc);
            desNode?.SafeSetActive(true);
        }
        else
            desNode?.SafeSetActive(false);

    }
}
