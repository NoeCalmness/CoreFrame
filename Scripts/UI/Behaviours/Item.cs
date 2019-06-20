using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class Item : BaseRestrain
{
    #region public properties

    public EquipType equipType { get; private set; }
    public PItem data { get; private set; }
    #endregion

    #region public-part
    private Image icon;
    private Image[] starsImages;
    private Image[] bgQualityImages;

    private Text weapon_name;
    private Text other_name;

    private Transform havePanel;
    private Transform empty;
    private Text empty_text;

    private Button changeEquip_btn;
    private Button intentifyOrDetail_btn;
    private Text intentifyOrDetail_text;
    public Action<Item> onChangeEquipClick { get; set; }
    #endregion

    #region 强化和附魔的装备
    private Text weapon_attbitue;
    private Image level_max;
    private Image enchant_iamge;
    private Text remain_enchantTime;
    private Text intentify_level;
    private Image level_tweenUp;

    private Dictionary<WeaponSubType, Image> leftAttrDic = new Dictionary<WeaponSubType, Image>();//左边属性--种类类型
    #endregion

    private bool isInitedMain;

    private void IniteMainCompent(EquipType type)
    {
        if (isInitedMain) return;
        isInitedMain = true;

        InitePublicPart();

        IniteSpcialPart(type);

        changeEquip_btn = transform.Find("spec_btn/change_btn").GetComponent<Button>();
        changeEquip_btn.onClick.RemoveAllListeners();
        changeEquip_btn.onClick.AddListener(()=> {
            onChangeEquipClick?.Invoke(this);
        });
        intentifyOrDetail_btn = transform.Find("spec_btn/strenth_info_btn").GetComponent<Button>();
        intentifyOrDetail_text = transform.Find("spec_btn/strenth_info_btn/Text").GetComponent<Text>();
    }

    private void InitePublicPart()
    {
        icon = transform.Find("item/have_panel/wupin").GetComponent<Image>();
        starsImages = transform.Find("item/have_panel/qualityGrid").GetComponentsInChildren<Image>(true);

        Image whiteQuality = transform.Find("item/have_panel/quality/white").GetComponent<Image>();
        Image greenQuality = transform.Find("item/have_panel/quality/green").GetComponent<Image>();
        Image blueQuality = transform.Find("item/have_panel/quality/blue").GetComponent<Image>();
        Image purpleQuality = transform.Find("item/have_panel/quality/purple").GetComponent<Image>();
        Image goldenQuality = transform.Find("item/have_panel/quality/golden").GetComponent<Image>();
        Image orangeQuality = transform.Find("item/have_panel/quality/orange").GetComponent<Image>();
        bgQualityImages = new Image[] { whiteQuality, greenQuality, blueQuality, purpleQuality, goldenQuality, orangeQuality };

        havePanel = transform.Find("item/have_panel");
        
        weapon_name = transform.Find("item/have_panel/name/name_weapon")?.GetComponent<Text>();
        other_name = transform.Find("item/have_panel/name/name_others")?.GetComponent<Text>();
    }

    private void IniteSpcialPart(EquipType type)
    {
        if (type == EquipType.Weapon || type == EquipType.Gun)
        {
            Transform t = transform.Find("item/have_panel/name/name_weapon/strengthen_txt");
            if (!t) t = transform.Find("item/have_panel/name/strengthen_txt");
            if (t) intentify_level = t.GetComponent<Text>();

            weapon_attbitue = transform.Find("item/have_panel/name/name_weapon/Text")?.GetComponent<Text>();
            level_max = transform.Find("item/level_max")?.GetComponent<Image>();
            enchant_iamge = transform.Find("item/have_panel/buff/buff_icon")?.GetComponent<Image>();
            remain_enchantTime = transform.Find("item/have_panel/buff/buff_time")?.GetComponent<Text>();
            level_tweenUp = transform.Find("item/have_panel/level_canup")?.GetComponent<Image>();

            Image axe = transform.Find("item/have_panel/name/name_weapon/icons/axe").GetComponent<Image>();
            Image fist = transform.Find("item/have_panel/name/name_weapon/icons/fist").GetComponent<Image>();
            Image pistol = transform.Find("item/have_panel/name/name_weapon/icons/pistol").GetComponent<Image>();
            Image katana = transform.Find("item/have_panel/name/name_weapon/icons/katana").GetComponent<Image>();
            Image sword = transform.Find("item/have_panel/name/name_weapon/icons/sword").GetComponent<Image>();
            leftAttrDic.Clear();
            leftAttrDic.Add(WeaponSubType.GiantAxe, axe);
            leftAttrDic.Add(WeaponSubType.Gloves, fist);
            leftAttrDic.Add(WeaponSubType.Gun, pistol);
            leftAttrDic.Add(WeaponSubType.Katana, katana);
            leftAttrDic.Add(WeaponSubType.LongSword, sword);
        }
        else
        {
            intentify_level = transform.Find("item/have_panel/name/strengthen_txt")?.GetComponent<Text>();
            level_max = transform.Find("item/level_max")?.GetComponent<Image>();
            enchant_iamge = transform.Find("item/have_panel/buff/buff_icon")?.GetComponent<Image>();
            remain_enchantTime = transform.Find("item/have_panel/buff/buff_time")?.GetComponent<Text>();
            level_tweenUp = transform.Find("item/have_panel/level_canup")?.GetComponent<Image>();
        }

        if (type == EquipType.HeadDress || type == EquipType.HairDress || type == EquipType.FaceDress || type == EquipType.NeckDress)
        {
            empty = transform.Find("item/empty");
            empty_text = transform.Find("item/empty/Text").GetComponent<Text>();
        }
    }

    //--------------------------------------------------------------------------------------

    public void RefreshMainPart(EquipType equipType, PItem item = null, Action<PItem> OnPreview = null)
    {
        IniteMainCompent(equipType);

        data = item;
        this.equipType = equipType;
        if (item != null)
        {
            restrainId = item.itemTypeId;
            havePanel.gameObject.SetActive(true);
            if(empty) empty.gameObject.SetActive(false);

            PropItemInfo propInfo = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
            if (propInfo == null) return;

            RefreshPublic(propInfo);
            
            //refresh intent
            if (equipType == EquipType.Weapon || equipType == EquipType.Gun || equipType == EquipType.Cloth)
            {
                RefreshIntentComponent(equipType);

                intentifyOrDetail_btn.onClick.RemoveAllListeners();
                intentifyOrDetail_btn.onClick.AddListener(() =>
                {
                    OnPreview?.Invoke(item);
                });
            }
            else
            {
                intentifyOrDetail_btn.SetInteractable(true);
                intentifyOrDetail_text.text = "详情";
                intentifyOrDetail_btn.onClick.RemoveAllListeners();
                intentifyOrDetail_btn.onClick.AddListener(() => 
                {
                    Module_Global.instance.UpdateGlobalTip(item.itemTypeId);
                });
            }
            
        }
        else//空位栏显示--仅有配饰才会没有
        {
            if (empty)
            {
                empty.gameObject.SetActive(true);
                empty_text.text = ConfigText.GetDefalutString(TextForMatType.EquipUIText, 9);
            }
            havePanel.gameObject.SetActive(false);
            intentifyOrDetail_text.text = "详情";
            intentifyOrDetail_btn.onClick.RemoveAllListeners();
            intentifyOrDetail_btn.SetInteractable(false);
        }
    }

    private void RefreshPublic(PropItemInfo propInfo)
    {
        if (!propInfo) return;
        
        if (propInfo.itemType == PropType.Weapon)
        {
            WeaponAttribute weaponAttributes = ConfigManager.Get<WeaponAttribute>(propInfo.ID);
            //武器名字
            if (weapon_name)
            {
                weapon_name.gameObject.SetActive(true);
                weapon_name.text = propInfo.itemName;
            }

            //其他名字
            if (other_name) other_name.gameObject.SetActive(false);

            CreatureElementTypes type = weaponAttributes ? (CreatureElementTypes)weaponAttributes.elementType : CreatureElementTypes.Count;

            //武器后面的属性描述
            string attributeName = string.Empty;
            switch (type)
            {
                case CreatureElementTypes.Wind: attributeName = ConfigText.GetDefalutString(TextForMatType.EquipUIText, 15); break;
                case CreatureElementTypes.Fire: attributeName = ConfigText.GetDefalutString(TextForMatType.EquipUIText, 16); break;
                case CreatureElementTypes.Water: attributeName = ConfigText.GetDefalutString(TextForMatType.EquipUIText, 17); break;
                case CreatureElementTypes.Thunder: attributeName = ConfigText.GetDefalutString(TextForMatType.EquipUIText, 18); break;
                case CreatureElementTypes.Ice: attributeName = ConfigText.GetDefalutString(TextForMatType.EquipUIText, 19); break;
                default: break;
            }
            weapon_attbitue.text = attributeName;

            //左边图标
            foreach (var attr in leftAttrDic)
                attr.Value.gameObject.SetActive((int)attr.Key == propInfo.subType);

            if (propInfo.subType == (int)WeaponSubType.Gun)
            {
                //星级
                for (int i = 0; i < starsImages.Length; i++)
                    starsImages[i].gameObject.SetActive(i < propInfo.quality + 1);
                //底板
                for (int i = 0; i < bgQualityImages.Length; i++)
                    bgQualityImages[i].gameObject.SetActive(i + 1 == propInfo.quality);
            }
            else//武器要特殊处理底板
            {
                int index = (data.InvalidGrowAttr() ? 0 : data.growAttr.equipAttr.star) + 1;
                //底板
                for (int i = 0; i < bgQualityImages.Length; i++) bgQualityImages[i].gameObject.SetActive(i + 1 == index);
                //星级
                for (int i = 0; i < starsImages.Length; i++) starsImages[i].gameObject.SetActive(i < index);
            }
        }
        else
        {
            if (weapon_name) weapon_name.gameObject.SetActive(false);
            if (other_name)
            {
                other_name.gameObject.SetActive(true);
                other_name.text = propInfo.itemName;
            }

            //星级
            for (int i = 0; i < starsImages.Length; i++)
                starsImages[i].gameObject.SetActive(i < propInfo.quality);
        }
        
        //换icon
        if (propInfo) AtlasHelper.SetItemIcon(icon, propInfo);
    }

    public void RefreshIntentComponent(EquipType type)
    {
        PreviewEquipType previewType = Module_Equip.instance.GetCurrentPreType(type, data.GetIntentyLevel(), data.HasEvolved());
        if(intentifyOrDetail_text) intentifyOrDetail_text.text = GetBtnName(type);
        if(level_max) level_max.gameObject.SetActive(previewType == PreviewEquipType.Enchant);

        if(intentify_level)
        {
            intentify_level.text = Util.Format("+{0}", data.GetIntentyLevel());
            intentify_level.gameObject.SetActive(data.GetIntentyLevel() > 0);
        }

        //暂时屏蔽
        if (enchant_iamge) enchant_iamge.gameObject.SetActive(false);
        if (level_tweenUp) level_tweenUp.gameObject.SetActive(false);
        if (remain_enchantTime) remain_enchantTime.gameObject.SetActive(false);
    }

    private string GetBtnName(EquipType equipType)
    {
        PreviewEquipType type = Module_Equip.instance.GetCurrentPreType(equipType,data.GetIntentyLevel(),data.HasEvolved());
        switch (type)
        {
            case PreviewEquipType.Intentify: return "强化";
            case PreviewEquipType.Evolve: return "进阶";
            case PreviewEquipType.Enchant: return "附魔";
        }
        return string.Empty;
    }

    /// <summary>
    /// 新手引导步骤，只能特殊处理
    /// </summary>
    private void AsGuideItemClick()
    {
        Module_Guide mg = Module_Guide.instance;
        if (mg && mg.currentGuide && mg.currentGuideItem != null && mg.currentGuideItem.hotAreaData.restrainType == EnumGuideRestrain.CheckID)
        {
            mg.UpdateStep();
            EventTriggerListener e = GetComponent<EventTriggerListener>();
            if (e) Destroy(e);
        }
    }

    //---------------------------------------------------------------------------------------

    #region ChangeEquip-part
    private Image m_equipedImage;
    public Toggle toggle { get; private set; }
    #endregion

    private bool isInitedChange;

    private void IniteChangeEquipPart(EquipType type)
    {
        IniteSpcialPart(type);
        if (isInitedChange) return;
        isInitedChange = true;

        InitePublicPart();
        m_equipedImage = transform.Find("item/have_panel/equiped_iamge").GetComponent<Image>();
        if (m_equipedImage) m_equipedImage.gameObject.SetActive(false);
        toggle = transform.GetComponent<Toggle>();
    }

    public void RefreshChangeEquipPart(EquipType equipType, PItem item = null,bool equiped = false)
    {
        data = item;
        this.equipType = equipType;

        IniteChangeEquipPart(equipType);
        RefreshIntentComponent(equipType);
        if (m_equipedImage) m_equipedImage.gameObject.SetActive(equiped);
        if (toggle) toggle.isOn = false;

        if (item == null) return;

        PropItemInfo info = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
        RefreshPublic(info);
    }
}
