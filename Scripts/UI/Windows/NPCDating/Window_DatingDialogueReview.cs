/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-01-07
 * 
 ***************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_DatingDialogueReview : Window
{
    private Button m_btnPageUp, m_btnPageDown;
    private Button m_btnClose;
    private Text m_textLeftPageNum, m_textRightPageNum;//页码
    private Text m_textL, m_textR;
    private RectTransform m_tfPL, m_tfPR;

    private Dictionary<int, int[]> m_dicSegment = new Dictionary<int, int[]>();
    private List<string> m_listContents = new List<string>();
    private int m_nRightStartIndex = 0;
    private int m_nSegment = 0;
    private bool m_bRefrshEnd = false;//所有对话是否已经刷新完毕
    private bool m_bLocationEnd = true;
    private float m_perHeight = 0;
    private int m_calCount = 0;
    protected override void OnOpen()
    {
        isFullScreen = false;

        m_btnPageUp = GetComponent<Button>("panel_history/pageUp"); m_btnPageUp.onClick.AddListener(OnClickBtnPageUp);
        m_btnPageDown = GetComponent<Button>("panel_history/pageDown"); m_btnPageDown.onClick.AddListener(OnClickBtnPageDown);
        m_textLeftPageNum = GetComponent<Text>("panel_history/pageUp/pageNumber");
        m_textRightPageNum = GetComponent<Text>("panel_history/pageDown/pageNumber");

        m_btnClose = GetComponent<Button>("panel_history/close"); m_btnClose.onClick.AddListener(() => { Hide<Window_DatingDialogueReview>(); });

        m_tfPL = GetComponent<RectTransform>("panel_history/TextPL");
        m_tfPR = GetComponent<RectTransform>("panel_history/TextPR");
        m_textL = GetComponent<Text>("panel_history/TextPL/TextL");
        m_textR = GetComponent<Text>("panel_history/TextPR/TextR");

        m_textL.text = "";
        m_perHeight = m_textL.preferredHeight;
        Init();
    }

    protected override void OnHide(bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault(1, false);
        Clear();
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        m_listContents = moduleNPCDating.listReviewContents;
        PreCalTextContent(m_listContents);

        if (m_bLocationEnd) Segment = m_dicSegment.Count;
        else Segment = 1;
    }

    private void Init()
    {
        InitText();
    }

    private void InitText()
    {
        var textData = ConfigManager.Get<ConfigText>((int)TextForMatType.NpcDatingReview);
        if (textData == null) return;
        Util.SetText(GetComponent<Text>("panel_history/title"), textData[0]);
        Util.SetText(GetComponent<Text>("panel_history/pageUp/Text"), textData[1]);
        Util.SetText(GetComponent<Text>("panel_history/pageDown/Text"), textData[2]);
    }

    private int Segment
    {
        set { PageSegment(value); }
        get { return m_nSegment; }
    }

    private void OnClickBtnPageUp()
    {
        Segment--;
    }

    private void OnClickBtnPageDown()
    {
        Segment++;
    }

    private void PageSegment(int val)
    {
        if (val < 1)
        {
            val = 1;
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString((int)TextForMatType.NpcDatingReview, 3));//第一页提示
            return;
        }

        if (val > m_dicSegment.Count && m_bRefrshEnd)
        {
            val = m_dicSegment.Count;
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString((int)TextForMatType.NpcDatingReview, 4));//最后一页提示
            return;
        }

        m_nSegment = val;

        ResetContentText();

        int startIndex = m_nRightStartIndex;

        if (m_dicSegment.ContainsKey(m_nSegment))
        {
            startIndex = m_dicSegment[m_nSegment][0];
        }

        RefreshLeft(m_listContents, startIndex);

        ShowPage(m_nSegment, m_dicSegment.Count);
    }

    private void RefreshLeft(List<string> datas, int startIndex)
    {
        bool bContinue = UpdateContentText(datas, startIndex, m_textL, m_tfPL, true);
        m_bRefrshEnd = !bContinue;
        if (bContinue) RefreshRight(datas, m_nRightStartIndex);
        else m_bLocationEnd = false;
    }

    private void RefreshRight(List<string> datas, int startIndex)
    {
        bool bContinue = UpdateContentText(datas, startIndex, m_textR, m_tfPR, false);
        m_bRefrshEnd = !bContinue;
        if (bContinue) { }
        else m_bLocationEnd = false;
    }

    private bool UpdateContentText(List<string> datas, int startIndex, Text t, RectTransform tp, bool bLeft)
    {
        string strContent = "";
        bool bContinue = false;
        for (int i = startIndex; i < datas.Count; i++)
        {
            if (t.preferredHeight > tp.sizeDelta.y)
            {
                m_nRightStartIndex = i;
                strContent = "";
                bContinue = true;
                break;
            }
            strContent += datas[i] + "\n\r\n\r";
            Util.SetText(t, strContent);
        }
        return bContinue;
    }

    private int SubstringCount(string str, string substring)
    {
        if (str.Contains(substring))
        {
            string strReplaced = str.Replace(substring, "");
            return (str.Length - strReplaced.Length) / substring.Length;
        }

        return 0;
    }

    private string RemoveStrMark(string str, string substring)
    {
        string strReplaceed = "";
        if (str.Contains(substring))
        {
            strReplaceed = str.Replace(substring, "");
            RemoveStrMark(strReplaceed, substring);
        }
        return strReplaceed;
    }

    /// <summary>
    /// 显示页码
    /// </summary>
    /// <param name="curPage">当前页码</param>
    /// <param name="totalPage">总页码</param>
    private void ShowPage(int curPage, int totalPage)
    {
        int leftPage = curPage * 2 - 1;
        int rightPage = curPage * 2;
        m_textLeftPageNum.SafeSetActive(leftPage > 0);
        m_textRightPageNum.SafeSetActive(leftPage > 0);

        Util.SetText(m_textLeftPageNum, leftPage.ToString());
        Util.SetText(m_textRightPageNum, rightPage.ToString());
    }

    private void PreCalTextContent(List<string> contentDatas)
    {
        TextGenerator tg = m_textL.cachedTextGeneratorForLayout;
        TextGenerationSettings settings = m_textL.GetGenerationSettings(Vector2.zero);

        float lineCount = 0;
        float totalHeight = 0;
        int lastIndex = 0;
        for (int i = 0; i < contentDatas.Count; i++)
        {
            if (i == 0)
            {
                m_nSegment++;
                AddSegmentDic(i, lastIndex);
            }
            if (totalHeight > m_tfPR.sizeDelta.y)
            {
                if (m_calCount % 2 != 0)
                {
                    m_nSegment++;
                    AddSegmentDic(i, lastIndex);
                }
                m_calCount++;
                lineCount = 0;
                totalHeight = 0;
                lastIndex = i - 1;
                continue;
            }
            Canvas.ForceUpdateCanvases();
            string newContent = contentDatas[i] + "\n\r\n\r";

            //去除富文本颜色标记开头部分
            int subStartIndex1 = newContent.IndexOf("<");
            int subLength1 = newContent.IndexOf(">") - subStartIndex1 + 1;
            string tStr1 = newContent.Remove(subStartIndex1, subLength1);

            //去除富文本颜色标记结尾部分
            int subStartIndex2 = tStr1.IndexOf("<");
            int subLength2 = tStr1.IndexOf(">") - subStartIndex2 + 1;
            string tStr2 = tStr1.Remove(subStartIndex2, subLength2);

            int nrLine = SubstringCount(tStr2, "\n\r") / 2;

            //去掉多余的换行符和回车符，保证计算的宽度准确性
            newContent = RemoveStrMark(tStr2, "\n\r");

            var strWidth = tg.GetPreferredWidth(newContent, settings) / m_textL.pixelsPerUnit;
            var strLine = Math.Ceiling(strWidth / m_textL.rectTransform.sizeDelta.x);
            lineCount += (int)strLine + nrLine;
            totalHeight = lineCount * m_perHeight;
        }
    }

    private void AddSegmentDic(int startIndex, int endIndex)
    {
        if (!m_dicSegment.ContainsKey(m_nSegment))
        {
            int[] tempIndexs = new int[2];
            tempIndexs[0] = startIndex;
            tempIndexs[1] = endIndex;
            m_dicSegment.Add(m_nSegment, tempIndexs);
        }

    }

    private IEnumerator DelayDo()
    {
        yield return null;
        OnClickBtnPageDown();
    }

    private void ResetContentText()
    {
        m_textL.text = "";
        m_textR.text = "";
    }

    private void Clear()
    {
        ResetContentText();
        m_nSegment = 0;
        m_nRightStartIndex = 0;

        m_dicSegment.Clear();

        m_bRefrshEnd = false;
        m_bLocationEnd = true;

        m_calCount = 0;
    }

}
