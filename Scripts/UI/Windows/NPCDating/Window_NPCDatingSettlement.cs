/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-12-29
 * 
 ***************************************************************************************************/

using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_NPCDatingSettlement : Window
{
    private Transform m_tfPNpcModel;
    private Image m_npcIcon;
    private Text m_textNpcName;
    private Text m_textMoodValue;
    private Image m_imgDatingLevel;//约会评级
    private Transform m_tfDatingFinishedMark;//约会完成标记
    private Text m_textRelationLv;//羁绊阶段   路人。。
    private Text m_textSubRelationLv;//羁绊等级  相熟。。。
    private Text m_textAddGoodFeelValue;//增加的好感度值
    private Text m_textGoodFeelProcess;//好感度進度
    private Image m_imgGoodFeelingSlider;

    private ScrollView m_svEvent;
    private DataSource<string> m_dataSource;
    private float m_fetterDVal = 0;

    protected override void OnOpen()
    {
        m_tfPNpcModel = GetComponent<RectTransform>("npcInfo");

        m_npcIcon = GetComponent<Image>("settlementPanel/npcIcon");
        m_textNpcName = GetComponent<Text>("settlementPanel/npcName");
        m_textMoodValue = GetComponent<Text>("settlementPanel/npcMood/value");
        m_imgDatingLevel = GetComponent<Image>("settlementPanel/datingLevel");
        m_tfDatingFinishedMark = GetComponent<RectTransform>("settlementPanel/datingFinishedMark");

        m_imgGoodFeelingSlider = GetComponent<Image>("settlementPanel/goodFeeling/sliderGoodFeeling");
        m_textRelationLv = GetComponent<Text>("settlementPanel/goodFeeling/ralationShipLevel");
        m_textSubRelationLv = GetComponent<Text>("settlementPanel/goodFeeling/ralationShipSubLevel");
        m_textAddGoodFeelValue = GetComponent<Text>("settlementPanel/goodFeeling/goodFeelingAdd");
        m_textGoodFeelProcess = GetComponent<Text>("settlementPanel/goodFeeling/googFeelingLevel");

        m_svEvent = GetComponent<ScrollView>("settlementPanel/eventList");
        m_dataSource = new DataSource<string>(null, m_svEvent, OnSetItemData, null);

        InitText();
    }

    protected override void OnHide(bool forward)
    {
        moduleNPCDating.ContinueBehaviourCallBack();
        m_fetterDVal = 0;
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault(1, false);

        var uiCharacter = m_tfPNpcModel.GetComponentDefault<UICharacter>();
        uiCharacter.realTime = false;
        Level.currentMainCamera.enabled = true;
        Refresh();
    }

    private void InitText()
    {
        var textData = ConfigManager.Get<ConfigText>((int)TextForMatType.NpcDatingSettlement);
        if (textData == null) return;
        Util.SetText(GetComponent<Text>("settlementPanel/title"), textData[0]);
    }

    private void OnSetItemData(Transform node,string content)
    {
        Text t = node.Find("eventDes").GetComponent<Text>();
        Util.SetText(t, content);
    }

    private void Refresh()
    {
        Module_NPCDatingSettlement.SettlementData data = moduleNPCDatingSettlement.datingSettlement;
        if (data == null) return;

        AtlasHelper.SetNpcDateInfo(m_imgDatingLevel, data.lvIconName);
        Util.SetText(m_textMoodValue, Util.Format("×{0}", data.moodValue.ToString()));

        Module_Npc.NpcMessage npcInfo = moduleNpc.GetTargetNpc((NpcTypeID)moduleNPCDating.curDatingNpc.npcId);
        if (npcInfo != null)
        {
            AtlasHelper.SetAvatar(m_npcIcon, npcInfo.icon);
            Util.SetText(m_textNpcName, npcInfo.name);
            RefreshLastFetter(npcInfo);
        }
        string preStr = data.addGoodFeelValue == 0 ? "" : "+";
        Util.SetText(m_textAddGoodFeelValue, preStr+data.addGoodFeelValue.ToString());

        //刷新事件列表
        List<string> sourcedataList = new List<string>();
        if (data.finishedEventIds != null)
        {
            for (int i = 0; i < data.finishedEventIds.Length; i++)
            {
                sourcedataList.Add(Util.GetString(data.finishedEventIds[i]));
            }
            sourcedataList.Distinct();//去除相同元素的值
        }

        m_dataSource.SetItems(sourcedataList);

        CreateNpcModel(npcInfo.npcId);

        //显示约会完成标记
        m_tfDatingFinishedMark.SafeSetActive(true);
    }

    private void CreateNpcModel(int npcId)
    {
        string uiname = ((NpcTypeID)npcId).ToString();
        var tfNpc = Module_Home.instance.FindChild(uiname);
        if (tfNpc != null)
        {
            moduleHome.HideOthers(uiname);
            return;
        }

        moduleHome.HideOthers();
        Level.PrepareAssets(Module_Battle.BuildNpcSimplePreloadAssets(npcId), r =>
        {
            moduleGlobal.UnLockUI();
            if (!r) return;

            var npc = moduleHome.CreateNpc(npcId, Vector3.zero, new Vector3(0, 180, 0), uiname);
            if (npc)
            {
                npc.animator.enabled = false;
                npc.gameObject.SafeSetActive(true);
            }
            moduleHome.HideOthers(uiname);
        });
    }

    //刷新好感度信息
    private void RefreshLastFetter(Module_Npc.NpcMessage npcInfo)
    {
        string strVal = npcInfo.lastFetterValue + "/" + npcInfo.lastToFetterValue;

        Util.SetText(m_textGoodFeelProcess, strVal);

        Util.SetText(m_textRelationLv, npcInfo.lastStageName);
        Util.SetText(m_textSubRelationLv, npcInfo.lastLvName);

        float lastFillAmount = (float)npcInfo.lastFetterValue / npcInfo.toFetterValue;
        float nowFillAmount = (float)npcInfo.nowFetterValue / npcInfo.toFetterValue;
        if (npcInfo.lastFetterValue > npcInfo.lastToFetterValue)
        {
            m_fetterDVal = npcInfo.lastFetterValue - npcInfo.lastToFetterValue;
            nowFillAmount = 1;
        }

        FetterRoll(m_imgGoodFeelingSlider, lastFillAmount, nowFillAmount,1.0f, ()=> 
        {
            if (m_fetterDVal > 0) RefreshRollCurFetter(npcInfo);
            else RefreshCurFetter(npcInfo);
        });
    }

    private void RefreshCurFetter(Module_Npc.NpcMessage npcInfo)
    {
        string strVal = npcInfo.nowFetterValue + "/" + npcInfo.toFetterValue;

        Util.SetText(m_textGoodFeelProcess, strVal);
        Util.SetText(m_textRelationLv, npcInfo.curStageName);
        Util.SetText(m_textSubRelationLv, npcInfo.curLvName);

        float nowFillAmount = (float)npcInfo.nowFetterValue / npcInfo.toFetterValue;
    }

    private void RefreshRollCurFetter(Module_Npc.NpcMessage npcInfo)
    {
        m_imgGoodFeelingSlider.fillAmount = 0;

        string strVal = m_fetterDVal + "/" + npcInfo.toFetterValue;

        Util.SetText(m_textGoodFeelProcess, strVal);
        Util.SetText(m_textRelationLv, npcInfo.curStageName);
        Util.SetText(m_textSubRelationLv, npcInfo.curLvName);

        float nowFillAmount = (float)npcInfo.nowFetterValue / npcInfo.toFetterValue;
        m_fetterDVal = 0;//把差值重置一下
        FetterRoll(m_imgGoodFeelingSlider, 0, nowFillAmount);
    }

    private void FetterRoll(Image img, float fromVal, float toVal, float time = 1.0f, Action callback = null)
    {
        Sequence mScoreSequence = DOTween.Sequence();
        mScoreSequence.SetAutoKill(false);
        mScoreSequence.Append(
            DOTween.To((value) =>
            {
                img.fillAmount = value;
            }, fromVal, toVal, time)).AppendCallback(() => callback?.Invoke());
    }
}
