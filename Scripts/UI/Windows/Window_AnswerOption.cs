/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-12-26
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_AnswerOption : Window
{
    private ScrollView m_scrollView;
    private DataSource<DialogueAnswersConfig.AnswerItem> m_dataSource;
    private bool m_bStartCountTime = false;

    protected override void OnOpen()
    {
        isFullScreen = false;
        m_scrollView = GetComponent<ScrollView>("scrollView");
        m_dataSource = new DataSource<DialogueAnswersConfig.AnswerItem>(null, m_scrollView, SetItemData, OnClickItem);
    }

    protected override void OnHide(bool forward)
    {
        m_dataSource.Clear();
        countTime = 0;
        m_bStartCountTime = false;
    }

    private float countTime = 0;
    public override void OnRootUpdate(float diff)
    {
        if (m_bStartCountTime)
        {
            if (moduleAnswerOption.CurDialogueAnswersData.timeLimit > 0 && countTime >= moduleAnswerOption.CurDialogueAnswersData.timeLimit / 1000) //以毫秒为单位
            {
                countTime = 0;
                OnClickItem();
            }
            else countTime += diff;
        }
    }

    void _ME(ModuleEvent<Module_AnswerOption> e)
    {
        if (e.moduleEvent == Module_AnswerOption.EventRefreshAnswerOptionPanel)
        {
            var dataList = moduleAnswerOption.AllAnswerItemDataList;
            RefreshItems(dataList);
            m_bStartCountTime = true;
        }
    }
    private void RefreshItems(List<DialogueAnswersConfig.AnswerItem> dataList)
    {
        m_dataSource.SetItems(dataList);
    }

    private void SetItemData(RectTransform node, DialogueAnswersConfig.AnswerItem data)
    {
        Text t = node.Find("Text").GetComponent<Text>();
        Util.SetText(t, data.nameId);
    }

    private void OnClickItem(RectTransform node, DialogueAnswersConfig.AnswerItem data)
    {
        OnClickItem(data);
    }

    private void OnClickItem(DialogueAnswersConfig.AnswerItem data = null)
    {
        m_bStartCountTime = false;
        m_dataSource.Clear();
        Hide<Window_AnswerOption>();
        moduleAnswerOption.ClickItem(data);
    }

}
