using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RuneAttributeTip : MonoBehaviour
{

    private Color color_green; //变绿
    private Color color_normal;//正常

    private Transform m_goods;
    private Text m_name;
    private Text m_level;
    private RectTransform m_attrGrid;
    private Text m_twoSuit;
    private Text m_fourSuit;

    private Toggle m_lockBtn;
    private Text m_lockTxt;
    private Image m_lockImg;
    private Image m_unLockImg;

    private void Get()
    {
        m_goods = transform.Find("wupin");
        m_name = transform.Find("intensifyLevel").GetComponent<Text>();
        m_level = transform.Find("intensifyLevel/intentifyText").GetComponent<Text>();
        m_attrGrid = transform.Find("attributeGrid").GetComponent<RectTransform>();
        m_twoSuit = transform.Find("suitattributeGrid/twoSuite_text").GetComponent<Text>();
        m_fourSuit = transform.Find("suitattributeGrid/fourSuite_text").GetComponent<Text>();
        m_lockBtn = transform.Find("wupin/lock").GetComponent<Toggle>();
        m_lockTxt = transform.Find("wupin/lock/txt").GetComponent<Text>();
        m_lockImg = transform.Find("wupin/lock/img").GetComponent<Image>();
        m_unLockImg = transform.Find("wupin/lock/unlock_img").GetComponent<Image>();

    }

    public void SetAttrInfo(PItem item, bool isEquip = true)
    {
        var info = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
        if (info == null) return;
        Get();

        Util.SetItemInfo(m_goods, info, 0, 0, true, item.growAttr.runeAttr.star);
        m_name.text = info.itemName;
        m_level.text = Util.Format("+{0}", item.growAttr.runeAttr.level);

        RefreshChildrenAttribute(m_attrGrid, item);
        ShowSuiteWords(info.suite, m_twoSuit, m_fourSuit);

        if (isEquip)
        {
            foreach (var keyAndValue in Module_Rune.instance.suiteDic)
            {
                if (keyAndValue.Key == 0) continue;
                ushort key = keyAndValue.Key;
                if (info.suite == key)
                {
                    if (keyAndValue.Value >= 2)
                    {
                        ShowSuiteWords(info.suite, m_twoSuit, m_fourSuit, true, false);

                        if (keyAndValue.Value >= 4)
                            ShowSuiteWords(info.suite, m_twoSuit, m_fourSuit, true, true);
                    }
                }
            }
        }
        SetLock(item);
    }

    private void SetLock(PItem item)
    {
        m_lockBtn.onValueChanged.RemoveAllListeners();

        m_lockBtn.isOn = item.isLock == 0 ? false : true;
        if (m_lockBtn.isOn) Util.SetText(m_lockTxt, 200, 34);
        else Util.SetText(m_lockTxt, 200, 33);
        m_lockImg.gameObject.SetActive(item.isLock == 1);
        m_unLockImg.gameObject.SetActive(item.isLock == 0);

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

            var info = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
            if (info == null) return;
            Util.SetItemInfo(m_goods, info, 0, 0, true, item.growAttr.runeAttr.star);

        });
    }

    private void ShowSuiteWords(ushort suite, Text twoText, Text fourText, bool isTwo = false, bool isFour = false, bool isInBigShowPanel = false)
    {
        RuneBuffInfo runeBuff = ConfigManager.Get<RuneBuffInfo>(suite);
        if (runeBuff == null) return;
        string attrName = ConfigText.GetDefalutString(TextForMatType.AttributeUIText, (int)runeBuff.attrId);
        color_green = GeneralConfigInfo.defaultConfig.RuneConclude;
        color_normal = GeneralConfigInfo.defaultConfig.RuneNormal;
        if (isTwo) twoText.color = color_green;
        else twoText.color = color_normal;

        if (runeBuff.addType == 1)
        {
            if (runeBuff.attrId == 9 || runeBuff.attrId == 10 || runeBuff.attrId == 11 || runeBuff.attrId == 12 || runeBuff.attrId == 13)
                Util.SetText(twoText, (int)TextForMatType.RuneUIText, isInBigShowPanel ? 39 : 0, attrName, runeBuff.value.ToString("P2"));
            else
                Util.SetText(twoText, (int)TextForMatType.RuneUIText, isInBigShowPanel ? 39 : 0, attrName, runeBuff.value);
        }
        else if (runeBuff.addType == 2)
            Util.SetText(twoText, (int)TextForMatType.RuneUIText, isInBigShowPanel ? 39 : 0, attrName, runeBuff.value.ToString("P2"));
      
        if (isFour) fourText.color = color_green;
        else fourText.color = color_normal;

        var buff = ConfigManager.Get<BuffInfo>(runeBuff.buffId);
        if (buff != null)
        {
            if (!isInBigShowPanel)
                Util.SetText(fourText, (int)TextForMatType.RuneUIText, 1, buff.desc);
            else
                Util.SetText(fourText, buff.desc);
        }
    }
    private void RefreshChildrenAttribute(RectTransform attributeGrid, PItem item)
    {
        PItemGrowAttr itemGrow = item.growAttr;
        PItemRandomAttr[] attrs = itemGrow.runeAttr.randAttrs;

        for (int i = 0; i < attributeGrid.childCount; i++)
            attributeGrid.GetChild(i).gameObject.SetActive(i < attrs.Length);

        for (int j = 0; j < attrs.Length; j++)
        {
            var info = ConfigManager.Get<GrowAttributeInfo>(attrs[j].itemAttrId);
            if (info != null)
            {
                Text attrName = attributeGrid.GetChild(j).Find("attName").GetComponent<Text>();
                attrName.text = ConfigText.GetDefalutString(TextForMatType.AttributeUIText, info.attrId);

                Text target = attributeGrid.GetChild(j).Find("Text").GetComponent<Text>();
                if (info.attrId == 9 || info.attrId == 10 || info.attrId == 11 || info.attrId == 12 || info.attrId == 13)
                    Util.SetText(target, "+" + attrs[j].attrVal.ToString("P2"));
                else
                    Util.SetText(target, "+" + ((int)attrs[j].attrVal).ToString());
            }
        }
    }

}
