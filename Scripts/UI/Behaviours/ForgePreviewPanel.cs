using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForgePreviewPanel : MonoBehaviour
{
    private Image equiped;
    private Image typeIcon;
    private Text weaponName;
    private Text levelTxt;
    private Image[] stars;
    private Image elementIcon;

    private Toggle m_lockBtn;
    private Text m_lockTxt;
    private Image m_lockImg;
    private Image m_unLockImg;

    private Text m_leftLevel;

    private string[] m_element = new string[5] { "ui_forging_wind03", "ui_forging_fire03", "ui_forging_water03", "ui_forging_thunder03", "ui_forging_ice03" };
    public readonly static string[] WEAPON_TYPE_IMG_NAMES = new string[7] { "career_icon_01", "career_icon_02", "career_icon_06", "career_icon_04", "career_icon_05", "ui_equip_02", "ui_equip_04" };

    bool isInited;

    public void ForingUse()
    {
        if (isInited) return;
        isInited = true;
        equiped = transform.Find("bground/base/equiped_img")?.GetComponent<Image>();
        typeIcon = transform.Find("bground/base/icon/image")?.GetComponent<Image>();

        weaponName = transform.Find("bground/base/name")?.GetComponent<Text>();
        levelTxt = transform.Find("bground/base/name/level_txt")?.GetComponent<Text>();
        stars = transform.Find("bground/base/qualityGrid")?.GetComponentsInChildren<Image>(true);

        elementIcon = transform.Find("bground/base/elementicon")?.GetComponent<Image>();
        Util.SetText(transform.Find("bground/base/equiped_img/equiped")?.GetComponent<Text>(), 224, 30);

        m_lockBtn = transform.Find("bground/base/name/lock")?.GetComponent<Toggle>();
        m_lockTxt = transform.Find("bground/base/name/lock/txt")?.GetComponent<Text>();

        m_lockImg = transform.Find("bground/base/name/lock/img")?.GetComponent<Image>();
        m_unLockImg = transform.Find("bground/base/name/lock/unlock_img")?.GetComponent<Image>();

        m_leftLevel = transform.Find("bground/base/level")?. GetComponent<Text>();
    }

    public void ForingItem(PItem item, bool addLockEvent = true)
    {
        if (item == null) return;
        ForingUse();
        PropItemInfo prop = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
        WeaponAttribute weaponAttributes = ConfigManager.Get<WeaponAttribute>(item.itemTypeId);
        if (!prop) return;
        //名字
        CreatureElementTypes elemrntType = weaponAttributes ? (CreatureElementTypes)weaponAttributes.elementType : CreatureElementTypes.Count;

        if (weaponName) weaponName.text = prop.itemName;
        if (levelTxt) levelTxt.text = "+" + item.growAttr.equipAttr.strength.ToString();
        elementIcon.SafeSetActive(false);
        //武器属性 武器图标

        if (prop.itemType == PropType.Weapon && prop.subType != (byte)WeaponSubType.Gun)
        {
            elementIcon.SafeSetActive(true);
            AtlasHelper.SetShared(elementIcon, m_element[(int)(elemrntType) - 1]);
        }

        int index = Module_Forging.instance.GetWeaponIndex(prop);
        if (index != -1) AtlasHelper.SetShared(typeIcon, WEAPON_TYPE_IMG_NAMES[index]);

        //
        //已装备
        bool isContains = Module_Equip.instance.currentDressClothes.Contains(item);
        if (Module_Equip.instance.weapon != null && Module_Equip.instance.offWeapon != null)
            if (item.itemId == Module_Equip.instance.weapon.itemId || item.itemId == Module_Equip.instance.offWeapon.itemId) isContains = true;
        equiped.SafeSetActive(isContains);

        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].SafeSetActive(i < prop.quality);
        }

        //锻造等级
        m_leftLevel.SafeSetActive(true);
        var str = (ConsumePercentSubType)item.growAttr.equipAttr.star + ConfigText.GetDefalutString(224, 23);
        Util.SetText(m_leftLevel, str);
        
        if (addLockEvent) SetLock(item);
    }

    private void SetLock(PItem item)
    {
        if (!m_lockBtn) return;

        m_lockBtn.onValueChanged.RemoveAllListeners();

        m_lockBtn.isOn = item.isLock == 0 ? false : true;
        m_lockImg.gameObject.SetActive(item.isLock == 1);
        m_unLockImg.gameObject.SetActive(item.isLock == 0);

        if (m_lockBtn.isOn) Util.SetText(m_lockTxt, 200, 34);
        else Util.SetText(m_lockTxt, 200, 33);
        m_lockBtn.onValueChanged.RemoveAllListeners();
        m_lockBtn.onValueChanged.AddListener(delegate
        {
            if (m_lockBtn.isOn)
            {
                Util.SetText(m_lockTxt, 200, 34);
                Module_Cangku.instance.SetLock(item.itemId, 1);
                item.isLock = 1;
                m_lockImg.gameObject.SetActive(item.isLock == 1);
                m_unLockImg.gameObject.SetActive(item.isLock == 0);
            }
            else
            {
                Util.SetText(m_lockTxt, 200, 33);
                Module_Cangku.instance.SetLock(item.itemId, 0);
                item.isLock = 0;
                m_lockImg.gameObject.SetActive(item.isLock == 1);
                m_unLockImg.gameObject.SetActive(item.isLock == 0);
            }
        });
    }
}
