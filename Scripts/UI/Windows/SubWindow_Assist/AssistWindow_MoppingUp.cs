// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-11-20      14:41
//  *LastModify：2018-11-21      17:38
//  ***************************************************************************************************/

#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SubWindowBase;

#endregion

public class AssistWindow_MoppingUp : SubWindowBase<Window_Assist>
{
    public const int SingleMoppingUpMax = 5;
    private readonly List<PReward> waitList = new List<PReward>();
    private DataSource<Entry>   dataSource;
    private Text                energy;
    private Button              excuteMulti;
    private Button              excuteOne;
    private Button              resetSweepTimes;
    private ScrollView          scrollView;
    private Text                sweepCost;
    private ChaseTask           task;
    private Button              sweepIconButton;

    private SubWindow_LevelUp   levelUpWindow;
    private byte oldLv;
    private ushort oldPoint;
    private ushort oldFatigue;
    private ushort oldMaxFatigue;

    protected override void InitComponent()
    {
        base.InitComponent();
        energy              = WindowCache.GetComponent<Text>        ("sweep_panel/remainEnergyCount/remainCount");
        sweepCost           = WindowCache.GetComponent<Text>        ("sweep_panel/remainSweepItemCount/remainCount");
        excuteOne           = WindowCache.GetComponent<Button>      ("sweep_panel/sweepOnce_Btn");
        excuteMulti         = WindowCache.GetComponent<Button>      ("sweep_panel/sweepFiveTimes_Btn");
        resetSweepTimes     = WindowCache.GetComponent<Button>      ("sweep_panel/remainSweepItemCount/resetBtn");
        sweepIconButton     = WindowCache.GetComponent<Button>      ("sweep_panel/remainSweepItemCount/icon");
        scrollView          = WindowCache.GetComponent<ScrollView>  ("sweep_panel/sweepResultList");

        levelUpWindow       = CreateSubWindow<SubWindow_LevelUp>(WindowCache, WindowCache.GetComponent<Transform>("upLvPanel")?.gameObject);
        levelUpWindow.Set(false);
    }

    public override void MultiLanguage()
    {
        base.MultiLanguage();
        Util.SetText(WindowCache.GetComponent<Text>("sweep_panel/titleBg/title_Txt"),                           ConfigText.GetDefalutString(TextForMatType.AssistUI, 8));
        Util.SetText(WindowCache.GetComponent<Text>("sweep_panel/sweepDes (1)"),                                ConfigText.GetDefalutString(TextForMatType.MoppingUpUI, 6));
        Util.SetText(WindowCache.GetComponent<Text>("sweep_panel/sweepDes"),                                    ConfigText.GetDefalutString(TextForMatType.MoppingUpUI, 7));
        Util.SetText(WindowCache.GetComponent<Text>("sweep_panel/sweepOnce_Btn/sweepOnce_Txt"),                 ConfigText.GetDefalutString(TextForMatType.MoppingUpUI, 10));
        Util.SetText(WindowCache.GetComponent<Text>("sweep_panel/sweepFiveTimes_Btn/sweepFiveTimes_Txt"),       ConfigText.GetDefalutString(TextForMatType.MoppingUpUI, 11));
        Util.SetText(WindowCache.GetComponent<Text>("sweep_panel/consumetitle"),                                ConfigText.GetDefalutString(TextForMatType.MoppingUpUI, 12));
        Util.SetText(WindowCache.GetComponent<Text>("sweep_panel/remainSweepItemCount/Text"),                   ConfigManager.Get<PropItemInfo>(602)?.itemName);
        Util.SetText(WindowCache.GetComponent<Text>("sweep_panel/remainEnergyCount/Text"),                      ConfigText.GetDefalutString(TextForMatType.MoppingUpUI, 13));
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
            return false;

        excuteOne           ?.onClick.AddListener(OnExcuteOne);
        excuteMulti         ?.onClick.AddListener(OnExcuteMulti);
        resetSweepTimes     ?.onClick.AddListener(OnBuySweepTimes);
        
        sweepIconButton     ?.onClick.AddListener(()=>moduleGlobal.UpdateGlobalTip(602));

        if (p.Length > 0)
            task = p[0] as ChaseTask;
        else
            task = parentWindow.chaseTask;
        var exception = Module_Chase.CheckMoppingUp(task, SingleMoppingUpMax);
        excuteMulti.SetInteractable(exception != MoppingUpException.NoChallengeTimes);

        excuteMulti.SafeSetActive(exception != MoppingUpException.MaxNotEnough);

        RefreshMatrial();
        EnableExcuteButton(true);
        return true;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;

        waitList.Clear();
        excuteOne           ?.onClick.RemoveAllListeners();
        excuteMulti         ?.onClick.RemoveAllListeners();
        resetSweepTimes     ?.onClick.RemoveAllListeners();
        sweepIconButton     ?.onClick.RemoveAllListeners();

        return true;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        levelUpWindow?.Destroy();
    }

    private void OnBuySweepTimes()
    {
        moduleGlobal.OpenExchangeTip(TipType.MoppingUpTip);
    }

    private void RefreshMatrial()
    {
        RefreshFatigueCost();
        RefreshSweepCost();
    }

    private void RefreshSweepCost()
    {
        var own = moduleEquip.GetPropCount(602);
        Util.SetText(sweepCost, $"{own}/1");
        sweepCost.color = ColorGroup.GetColor(ColorManagerType.IsMatrialEnough, own > 0);
    }

    private void RefreshFatigueCost()
    {
        Util.SetText(energy, task.taskConfigInfo.fatigueCount.ToString());
        energy.color = ColorGroup.GetColor(ColorManagerType.IsMatrialEnough,
            task.taskConfigInfo.fatigueCount <= modulePlayer.roleInfo.fatigue);
    }

    private void OnExcuteMulti()
    {
        var error = Module_Chase.CheckMoppingUp(task, SingleMoppingUpMax);
        if (error == MoppingUpException.None)
        {
            Window_Alert.ShowAlertDefalut(Util.Format(ConfigText.GetDefalutString(TextForMatType.MoppingUpUI, 0), task.taskConfigInfo.fatigueCount * SingleMoppingUpMax, SingleMoppingUpMax), () =>
            {
                //TODO: 开始扫荡
                moduleChase.RequestMoppingUp((ushort)task.taskConfigInfo.ID, SingleMoppingUpMax);
            }, null, ConfigText.GetDefalutString(TextForMatType.MoppingUpUI, 1), ConfigText.GetDefalutString(TextForMatType.MoppingUpUI, 2));
        }
        else
            ProcessException(error);
    }

    private void OnExcuteOne()
    {
        var error = Module_Chase.CheckMoppingUp(task, 1);
        if (error == MoppingUpException.None)
        {
            //TODO: 开始扫荡
            moduleChase.RequestMoppingUp((ushort)task.taskConfigInfo.ID, 1);
        }
        else
            ProcessException(error);
    }

    private void ProcessException(MoppingUpException e)
    {
        switch (e)
        {
            case MoppingUpException.NoMatrial:
                moduleGlobal.ShowMessage((int)TextForMatType.MoppingUpUI, 3);
                moduleGlobal.OpenExchangeTip(TipType.MoppingUpTip);
                break;
            case MoppingUpException.NoEnergy:
                moduleGlobal.ShowMessage((int)TextForMatType.MoppingUpUI, 4);
                moduleGlobal.OpenExchangeTip(TipType.BuyEnergencyTip);
                break;
            case MoppingUpException.NoChallengeTimes:
                moduleGlobal.ShowMessage((int)TextForMatType.MoppingUpUI, 5);
                parentWindow.RefreshPayPanel(task);
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Chase> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Chase.ResponseMoppingUp:
                ResponseMoppingUp(e.msg as ScChaseMoppingUp);
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Player> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Player.EventBuySuccessSweep:
                RefreshSweepCost();
                break;
            case Module_Player.EventFatigueChanged:
                RefreshFatigueCost();
                break;
            case Module_Player.EventLevelChanged:
                oldLv = (byte)e.param1;
                oldPoint = (ushort)e.param2;
                oldFatigue = (ushort)e.param3;
                oldMaxFatigue = (ushort)e.param4;
                break;
        }
    }
    private void ResponseMoppingUp(ScChaseMoppingUp msg)
    {
        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9605, msg.result);
            return;
        }
        moduleChase.ProcessTimes(task, msg.rewards?.Length ?? 0);
        parentWindow.RefreshChallengeTimes();
        RefreshMatrial();
        PReward[] arr = null;
        msg.rewards.CopyTo(ref arr);
        waitList.AddRange(arr);
        DelayEvents.Add(StartShow, 0.05f);
    }

    private void StartShow()
    {
        if (waitList.Count <= 0)
        {
            OnShowRewardEnd();
            return;
        }
        EnableExcuteButton(false);

        var t = new Entry
        {
            isNew = true,
            reward = waitList[0],
            times = (dataSource?.count ?? 0) + 1
        };
        waitList.RemoveAt(0);
        if (moduleGlobal.targetMatrial.isProcess)
        {
            t.targetMatrialCount = moduleGlobal.targetMatrial.targetCount;
            t.nowMatrialCount    = moduleGlobal.targetMatrial.OwnCount - WaitShowNumber();
            t.getCount           = t.reward.GetItemCount(moduleGlobal.targetMatrial.itemId);
        }

        if (dataSource == null)
            dataSource = new DataSource<Entry>(new List<Entry> {t}, scrollView, OnSetData);
        else
            dataSource.AddItem(t);

        dataSource.view.ScrollToIndex(dataSource.count -1, 0.5f);
        dataSource.UpdateItem(dataSource.count - 1);
    }

    private void OnShowRewardEnd()
    {
        EnableExcuteButton(true);
        if (modulePlayer.isLevelUp)
        {
            levelUpWindow.Initialize(oldLv, oldPoint, oldFatigue, oldMaxFatigue);
            modulePlayer.isLevelUp = false;
        }
        //更新一下上一次的经验条
        moduleLogin.lastExpProcess = moduleLogin.expProcess;
    }

    private void EnableExcuteButton(bool rEnable)
    {
        excuteOne  .SetInteractable(rEnable);
        excuteMulti.SetInteractable(rEnable && Module_Chase.CheckMoppingUp(task, SingleMoppingUpMax) != MoppingUpException.MaxNotEnough);
    }

    /// <summary>
    /// 计算等待显示获得目标道具数量
    /// </summary>
    /// <returns></returns>
    private int WaitShowNumber()
    {
        if (!moduleGlobal.targetMatrial.isProcess)
            return 0;
        var num = 0;
        foreach (var p in waitList)
        {
            num += p.GetItemCount(moduleGlobal.targetMatrial.itemId);
        }
        return num;
    }

    private void OnSetData(RectTransform node, Entry data)
    {
        var temp = node.GetComponentDefault<MoppingUpItemTemplete>();
        temp.Init(data);

        if (data.isNew)
        {
            //播动画。动画完成后调用StartShow
            var tween = temp.GetComponent<TweenBase>();
            if (tween)
            {
                tween.onComplete.RemoveAllListeners();
                tween.onComplete.AddListener(b => { StartShow(); });
                tween.Play();
            }
            else
                StartShow();
        }

        data.isNew = false;
    }

    public class Entry
    {
        public bool     isNew;
        public int      getCount;
        public int      nowMatrialCount;
        public PReward  reward;
        public int      targetMatrialCount;
        public int      times;
    }

    public void ClearRecord()
    {
        dataSource?.Clear();
    }
}
