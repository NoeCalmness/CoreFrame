// /**************************************************************************************************
//  * Copyright (C) 2018-2019 FengYunChuanShuo
//  * 
//  *           图鉴子窗口
//  * 
//  *Author:     T.Moon
//  *Version:    0.1
//  *Created:    2018-12-18      13:31
//  ***************************************************************************************************/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class Collection_Window_Rune : SubWindowBase<Window_Collection>
{
    DataSource<RuneData> m_RuneDataSource;
    private Image           m_RuneFullImage;           //灵珀全身像
    private Image           m_RuneIconImage;           //灵珀图标
    private Text            m_RuneName;                //名称
    private Text            m_RuneDescriptionText;     //描述
    private Text            m_TwoSuitDescriptionText;  //套装描述
    private Text            m_FourSuitDescriptionText; //套装描述
    private ScrollView      m_RuneScroll;               //列表
    private int m_CurClickIndex = -1;                   //当前点击索引
    private Color m_ColorGreen = new Color(155 / 255f, 255 / 255f, 138 / 255f);//变绿
    private Color m_ColorNormal = new Color(255 / 255f, 255 / 255f, 255 / 255f);//正常

    protected override void InitComponent()
    {
        base.InitComponent();
        m_RuneFullImage = WindowCache.GetComponent<Image>("rune_Panel/bigImage");
        m_RuneIconImage = WindowCache.GetComponent<Image>("rune_Panel/descreption1/Image");
        m_RuneName = WindowCache.GetComponent<Text>("rune_Panel/descreption1/name");
        m_RuneDescriptionText = WindowCache.GetComponent<Text>("rune_Panel/descreption1/descreption");
        m_TwoSuitDescriptionText = WindowCache.GetComponent<Text>("rune_Panel/descreption2/attributeSuit2_text");
        m_FourSuitDescriptionText = WindowCache.GetComponent<Text>("rune_Panel/descreption2/attributeSuit4_text");

        m_RuneScroll = WindowCache.GetComponent<ScrollView>("rune_Panel/scrollView");

        m_RuneDataSource = new DataSource<RuneData>(null, m_RuneScroll, OnSetRuneItem, OnClickBigRune);

    }
    public override bool Initialize(params object[] p)
    {
        m_RuneDataSource.SetItems(moduleCollection.GetUIRuneData());
        return base.Initialize(p);
    }

    public override bool UnInitialize(bool hide = true)
    {
        return base.UnInitialize(hide);
    }

    public override bool OnReturn()
    {
        return base.OnReturn();
    }

    private void OnSetRuneItem(RectTransform node, RuneData data)
    {
        Transform no = node.Find("noImage");
        Transform have = node.Find("haveImage");
        Transform select = node.Find("selectBox");
        if (!no || !have || !select) return;
        no.gameObject.SetActive(!data.isOwnItem);
        have.gameObject.SetActive(data.isOwnItem);
        int index = moduleCollection.GetUIRuneData().FindIndex((p) => p.itemtypeId == data.itemtypeId);
        //设置默认选中已有的 第一个符文
        if(m_CurClickIndex==-1&&data.isOwnItem)
        {
            m_CurClickIndex = index;
        }
        select.gameObject.SetActive(index == m_CurClickIndex);
        if(index == m_CurClickIndex)
        {
            UIDynamicImage.LoadImage(m_RuneFullImage.transform, data.fullImage, null, true);

            m_RuneName.text = data.itemName;
            AtlasHelper.SetRune(m_RuneIconImage, data.iconId);
            var configText = ConfigManager.Get<ConfigText>(data.descId);
            if (configText)
                m_RuneDescriptionText.text = configText.text[0].Replace("\\n", "\n");
            ShowSuiteWords(data.suite, m_TwoSuitDescriptionText, m_FourSuitDescriptionText, false, false, true);
        }
    }
    private void OnClickBigRune(RectTransform node, RuneData data)
    {
        m_CurClickIndex = moduleCollection.GetUIRuneData().FindIndex((p) => p.itemtypeId == data.itemtypeId);
        m_RuneDataSource.UpdateItems();
    }
    private void ShowSuiteWords(ushort suite, Text twoText, Text fourText, bool isTwo = false, bool isFour = false, bool isInBigShowPanel = false)
    {
        RuneBuffInfo runeBuff = ConfigManager.Get<RuneBuffInfo>(suite);
        if (runeBuff == null) return;
        string attrName = ConfigText.GetDefalutString(TextForMatType.AttributeUIText, (int)runeBuff.attrId);

        if (isTwo) twoText.color = m_ColorGreen;
        else twoText.color = m_ColorNormal;

        if (runeBuff.addType == 1)
        {
            if (runeBuff.attrId == 9 || runeBuff.attrId == 10 || runeBuff.attrId == 11 || runeBuff.attrId == 12 || runeBuff.attrId == 13)
                Util.SetText(twoText, (int)TextForMatType.RuneUIText, isInBigShowPanel ? 39 : 0, attrName, runeBuff.value.ToString("P2"));
            else
                Util.SetText(twoText, (int)TextForMatType.RuneUIText, isInBigShowPanel ? 39 : 0, attrName, runeBuff.value);
        }
        else if (runeBuff.addType == 2)
            Util.SetText(twoText, (int)TextForMatType.RuneUIText, isInBigShowPanel ? 39 : 0, attrName, runeBuff.value.ToString("P2"));

        if (isFour) fourText.color = m_ColorGreen;
        else fourText.color = m_ColorNormal;

        var buff = ConfigManager.Get<BuffInfo>(runeBuff.buffId);
        if (buff != null)
        {
            if (!isInBigShowPanel)
                Util.SetText(fourText, (int)TextForMatType.RuneUIText, 1, buff.desc);
            else
                Util.SetText(fourText, buff.desc);
        }
    }
}