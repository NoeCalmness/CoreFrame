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
public class Module_AnswerOption : Module<Module_AnswerOption>
{
    public const string EventRefreshAnswerOptionPanel = "EventAnswerOptionRefreshAnswerOptionPanel";

    private bool m_bDoEvent = false;

    private int m_getTypeAnswerId = 0;
    private int m_lastAnswerId = 0;

    /// <summary>当前问题下所有选项数据</summary>
    public List<DialogueAnswersConfig.AnswerItem> AllAnswerItemDataList{ private set; get; }

    /// <summary>当前问题数据</summary>
    public DialogueAnswersConfig CurDialogueAnswersData { private set; get; }

    /// <summary>上一次选择的问题Id</summary>
    public int lastSelectAnswerId { private set; get; }

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        ClearData();
    }

    private void ClearData()
    {
        AllAnswerItemDataList = null;
        CurDialogueAnswersData = null;
        lastSelectAnswerId = 0;
        m_bDoEvent = false;
        m_getTypeAnswerId = 0;
        m_lastAnswerId = 0;
        dicClickId.Clear();
        dicClickType.Clear();
        m_OnCompleteCallBack = null;
    }

    public void ClearReconnectData()
    {
        dicClickId.Clear();
        dicClickType.Clear();
    }

    private Action m_OnCompleteCallBack;

    /// <summary>选择的问题id组</summary>
    public Dictionary<int, int> dicClickId = new Dictionary<int, int>();
    /// <summary>选择的问题类型组</summary>
    public Dictionary<int, int> dicClickType = new Dictionary<int, int>();

    public void OpenAnswerWindow(int answerId,Action callBack)
    {
        DialogueAnswersConfig dac = ConfigManager.Get<DialogueAnswersConfig>(answerId);
        if (dac == null)
        {
            Logger.LogError("Dating::  在DialogueAnswersConfig表中,找不到 id={0} 的配置，请检查配置", answerId);
            return;
        }
        CurDialogueAnswersData = dac;
        m_OnCompleteCallBack = callBack;
        var listAllAnswerItems = new List<DialogueAnswersConfig.AnswerItem>();
        //检查有无问题选项
        bool bHaveAnswerItem = CheckAnserItems(dac.answerItems);

        if (bHaveAnswerItem)
        {
            Module_Npc.NpcMessage npc = null;
            if (moduleNpc.curNpc == null) npc = moduleNpc.GetTargetNpc((NpcTypeID)moduleNPCDating.curDatingNpc?.npcId);
            else npc = moduleNpc.curNpc;

            //有问题选项，判断每个问题是否符合限制条件
            for (int i = 0; i < dac.answerItems.Length; i++)
            {
                bool bGoodFeeling = CheckGoodFeelingLimit(npc.nowFetterValue, dac.answerItems[i].goodFeelingLimits);
                bool bMotion = CheckMotionLimit(npc.mood, dac.answerItems[i].motionLimit);
                if (bGoodFeeling && bMotion) listAllAnswerItems.Add(dac.answerItems[i]);
            }

            //如果有符合条件的选项,进一步筛选
            if (listAllAnswerItems.Count > 0)
            {
                AllAnswerItemDataList = GetFinalAnswerItemsList(listAllAnswerItems, dac);
                if(AllAnswerItemDataList.Count > 0) OpenAnswerWindow();
            }
            else Logger.LogError("Dating::  answerId={0} 的配置中，符合条件的问题选项为空，请检查配置条件", answerId);
        }
        else OnChooseAnswerCallBack();

    }

    /// <summary>
    /// 选择问题之后的回调，如果没有问题选项则直接回调
    /// </summary>
    private void OnChooseAnswerCallBack()
    {
        m_OnCompleteCallBack?.Invoke();
        m_OnCompleteCallBack = null;
    }

    public void ClickItem(DialogueAnswersConfig.AnswerItem data)
    {
        int itemId = data == null ? 0 : data.id;
        if (dicClickId.ContainsKey(CurDialogueAnswersData.ID)) dicClickId[CurDialogueAnswersData.ID] = itemId;
        else dicClickId.Add(CurDialogueAnswersData.ID, itemId);

        EnumAnswerType itemType = data == null ? EnumAnswerType.None : data.answerType;
        if (dicClickType.ContainsKey(CurDialogueAnswersData.ID)) dicClickType[CurDialogueAnswersData.ID] = (int)itemType;
        else dicClickType.Add(CurDialogueAnswersData.ID, (int)itemType);

        //只有可以检查的时候才会记录上一次选择的问题选项
        if (CurDialogueAnswersData.needCheckCount == 1)
        {
            Logger.LogDetail("Dating::  在配置表DialogueAnswersConfig id={0} 的配置中，会记录本次选择问题的选项，请知悉", CurDialogueAnswersData.ID);
            lastSelectAnswerId = data == null ? 0 : data.id;
        }

        //把选项发送给服务器
        SendSelectAnswer(CurDialogueAnswersData.ID, data == null ? 0 : data.id);

        //如果图书馆选择检测错误答案，给服务器发送消息
        if (data != null && data.answerType == EnumAnswerType.Library_Wrong) SendLibWrongAnswer();

        //占卜屋占卜
        if (data != null && data.answerType == EnumAnswerType.Divination_CrystalBall) moduleNPCDating.GetDivinationResult(EnumDivinationType.CrystalDevine, OnChooseAnswerCallBack);
        else if(data != null && data.answerType == EnumAnswerType.Divination_ImperialGuard) moduleNPCDating.GetDivinationResult(EnumDivinationType.LotDevine, OnChooseAnswerCallBack);
        else OnChooseAnswerCallBack();

    }

    private void OpenAnswerWindow()
    {
        //打开选择问题面板,然后刷新问题选项
        Window.ShowAsync<Window_AnswerOption>(w => { DispatchModuleEvent(EventRefreshAnswerOptionPanel); });
    }

    private List<DialogueAnswersConfig.AnswerItem> GetFinalAnswerItemsList(List<DialogueAnswersConfig.AnswerItem> allListItems, DialogueAnswersConfig dac)
    {
        List<DialogueAnswersConfig.AnswerItem> listFinalAnswerItem = new List<DialogueAnswersConfig.AnswerItem>();
        //需要计算权重的Item
        List<DialogueAnswersConfig.AnswerItem> listNeedCalWeightAnswerItem = new List<DialogueAnswersConfig.AnswerItem>();
        for (int i = 0; i < allListItems.Count; i++)
        {
            if (allListItems[i].rate == 0) listFinalAnswerItem.Add(allListItems[i]);
            else listNeedCalWeightAnswerItem.Add(allListItems[i]);
        }

        //如果需要执行出现次数检查，如果上次有出现的选项，这次不再出现
        if (dac.needCheckCount == 1)
        {
            Logger.LogDetail("Dating::  Answer表 id={0} 中needCheckCount = 1，这里会筛选掉上一次出现的问题选项，请留意", dac.ID);
            DialogueAnswersConfig.AnswerItem d = listNeedCalWeightAnswerItem.Find((item) => item.id == lastSelectAnswerId);
            if (d != null)
            {
                listNeedCalWeightAnswerItem.Remove(d);
                lastSelectAnswerId = 0;
            }
        }

        //通过权重筛选出符合数量的item集合
        List<DialogueAnswersConfig.AnswerItem> weightItems = GetItemByWeight(listNeedCalWeightAnswerItem, dac);
        if (weightItems != null && weightItems.Count > 0) listFinalAnswerItem.AddRange(weightItems);

        return listFinalAnswerItem;
    }

    private bool CheckAnserItems(DialogueAnswersConfig.AnswerItem[] answers)
    {
        bool bCan = false;
        bCan = answers != null && answers.Length > 0;
        return bCan;
    }

    private bool CheckGoodFeelingLimit(float goodFeelingValue, float[] limits)
    {
        return CheckFloatLimit(goodFeelingValue, limits);
    }

    private bool CheckMotionLimit(float motionValue, float[] limits)
    {
        return CheckFloatLimit(motionValue, limits);
    }

    private bool CheckFloatLimit(float value, float[] limits)
    {
        if (limits == null || limits.Length == 0) return true;

        if (limits.Length == 1) return value >= limits[0];
        else return value >= limits[0] && value < limits[1];
    }

    private List<DialogueAnswersConfig.AnswerItem> GetItemByWeight(List<DialogueAnswersConfig.AnswerItem> items, DialogueAnswersConfig dac)
    {
        if (items == null || items.Count <= 0) return null;

        if (dac.randAnswerCount == 0)
        {
            Logger.LogError("Dating::  在DialogueAnswersConfig id={0}的配置中，需要计算权重的问题数量为0，请检查配置", dac.ID);
            return null;
        }

        if (dac.randAnswerCount > items.Count)
        {
            Logger.LogError("Dating::  在DialogueAnswersConfig id={0}的配置中，需要计算权重的问题数量超过配置的问题数量，请检查配置", CurDialogueAnswersData.ID);
            return null;
        }

        List<DialogueAnswersConfig.AnswerItem> sourceItems = items;
        List<DialogueAnswersConfig.AnswerItem> listItems = new List<DialogueAnswersConfig.AnswerItem>();
        DialogueAnswersConfig.AnswerItem lastItem = null;
        int answerCount = 0;//用于计数
        while (answerCount < dac.randAnswerCount)
        {
            DialogueAnswersConfig.AnswerItem item = _GetItemByWeight(sourceItems);
            if (lastItem != null && lastItem.id != item.id)
            {
                listItems.Add(item);
                sourceItems.Remove(item);
                answerCount++;
            }
            lastItem = item;
        }

        return listItems;
    }

    private DialogueAnswersConfig.AnswerItem _GetItemByWeight(List<DialogueAnswersConfig.AnswerItem> items)
    {
        if (items.Count == 1) return items[0];
        //计算总权重
        float sumWeight = 0;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].rate == 0) return items[i];
            sumWeight += items[i].rate;
        }

        //产生随机数
        float randomNumber = UnityEngine.Random.Range(0, sumWeight);

        float d1 = 0;
        float d2 = 0;
        DialogueAnswersConfig.AnswerItem item = null;
        for (int i = 0; i < items.Count; i++)
        {
            d2 += items[i].rate;
            if (i == 0) d1 = 0;
            else d1 += items[i - 1].rate;
            if (randomNumber >= d1 && randomNumber <= d2)
            {
                item = items[i];
                break;
            }
        }
        return item;
    }

    #region Server
    /// <summary>
    /// 图书馆选择错误答案时发送消息
    /// </summary>
    public void SendLibWrongAnswer()
    {
        var p = PacketObject.Create<CsNpcDatingSceneLibAnswerWrong>();
        session.Send(p);
    }

    private void SendSelectAnswer(int answerId,int answerItemId)
    {
        var p = PacketObject.Create<CsNpcAnswer>();
        PAnswer panswer = PacketObject.Create<PAnswer>();
        panswer.groupID = answerId;
        panswer.answerID = (byte)answerItemId;
        p.answer = panswer;
        session.Send(p);
    }

    #endregion

    #region GM
    public void GMResetDatingState()
    {
        ClearData();
    }
    #endregion
}
