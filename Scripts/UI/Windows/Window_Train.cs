// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-06-13      16:07
//  * LastModify：2018-07-16      10:03
//  ***************************************************************************************************/

#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class Window_Train : Window
{
    private CostComfirmBox _costComfirmBox;

    private PetProcess_Train _petProcessTrain;

    [Widget("fastforward")]
    private Transform confirmBoxRoot;

    [Widget("missioninfo_panel")]
    private Transform missionInfo;

    [Widget("missionlist_panel/bg/subtitle")]
    private Text processText;

    [Widget("missionlist_panel/mission_list")]
    private ScrollView scrollView;

    private DataSource<SealPetTaskInfo> taskDataSource;

    protected override void OnOpen()
    {
        base.OnOpen();
        MultiLanguage();
        _petProcessTrain = SubWindowBase              .CreateSubWindow<PetProcess_Train>(this, missionInfo.gameObject);
        _costComfirmBox  = SubWindowBase<Window_Train>.CreateSubWindow<CostComfirmBox>(this, confirmBoxRoot.gameObject);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();
        if (taskDataSource != null)
            taskDataSource.SetItems(modulePet.PetTasks);
        else
            taskDataSource = new DataSource<SealPetTaskInfo>(modulePet.PetTasks, scrollView, OnSetData);

        scrollView.SetProgress(0);
    }

    protected override void OnClose()
    {
        base.OnClose();
        _petProcessTrain?.Destroy();
        _costComfirmBox?.Destroy();
    }

    protected override void OnHide(bool forward)
    {
        _petProcessTrain.UnInitialize();
        _costComfirmBox.UnInitialize();
    }

    protected override void OnShow(bool forward)
    {
        RefreshProcessText();
    }


    private void MultiLanguage()
    {
        var t = ConfigManager.Get<ConfigText>((int)TextForMatType.PetTrainText);
        if (t == null)
        {
            Logger.LogError("无法找到文本配置：ID = {0}", TextForMatType.PetTrainText);
            return;
        }
        Util.SetText(GetComponent<Text>("missionlist_panel/bg/title"),                              t[0]);
        Util.SetText(GetComponent<Text>("missionlist_panel/mission_list/template/0/Text"),          t[2]);
        Util.SetText(GetComponent<Text>("missionlist_panel/mission_list/template/0/btn_01/Text"),   t[4]);
        Util.SetText(GetComponent<Text>("missionlist_panel/mission_list/template/0/btn_02/Text"),   t[3]);
        Util.SetText(GetComponent<Text>("missionlist_panel/mission_list/template/0/btn_03/Text"),   t[5]);
        Util.SetText(GetComponent<Text>("missioninfo_panel/prereward_txt"),                         t[13]);
        Util.SetText(GetComponent<Text>("missioninfo_panel/btn_01/Text"),                           t[15]);
        Util.SetText(GetComponent<Text>("missioninfo_panel/btn_02/Text"),                           t[14]);
        Util.SetText(GetComponent<Text>("missioninfo_panel/condition_group/conditions_txt"),        t[12]);
        Util.SetText(GetComponent<Text>("sprite_info/equipinfo"),                                   t[16]);
        Util.SetText(GetComponent<Text>("sprite_info/go/Text"),                                     t[17]);
        Util.SetText(GetComponent<Text>("missionlist_panel/mission_list/template/0/missionover/missionover_Txt"),t[26]);
        Util.SetText(GetComponent<Text>("missioninfo_panel/chosenpet"),                             t[28]);

        var ct = ConfigManager.Get<ConfigText>((int) TextForMatType.PetTrainFastComplete);
        Util.SetText(GetComponent<Text>("fastforward/bg/equipinfo"),                                ct[0]);
        Util.SetText(GetComponent<Text>("fastforward/bg/detail_content"),                           ct[1]);
        Util.SetText(GetComponent<Text>("fastforward/bg/xiaohao_tip/xiaohao"),                      ct[2]);
        Util.SetText(GetComponent<Text>("fastforward/bg/yes/yes_text"),                             ct[4]);
        Util.SetText(GetComponent<Text>("fastforward/bg/nobtn/no_text"),                            ct[5]);

    }

    private void OnSetData(RectTransform node, SealPetTaskInfo data)
    {
        var item = node.GetComponentDefault<PetTaskItem>();
        item.SetData(data);
        item.OnCancel = OnCancelRequest;
        item.OnGotAward = OnGotAward;
        item.OnStartTask = OnStartTask;
        item.OnSpeedUp= OnSpeedUp;
    }

    private void RefreshProcessText()
    {
        var ct = ConfigManager.Get<ConfigText>((int)TextForMatType.PetTrainText);
        var count = 0;
        foreach (var task in modulePet.PetTasks)
        {
            if (task.State == (int)PetTaskState.Training)
                count++;
        }
        Util.SetText(processText, Util.Format(ct[1], Util.Format("{0}", count)));
    }

    private void OnSpeedUp(PetTaskItem obj)
    {
        var msg = PacketObject.Create<CsPetTaskOperator>();
        msg.operatorCode = 2;
        msg.comfirm = false;
        msg.taskId = obj.Task.ID;
        session.Send(msg);
    }

    private void OnStartTask(PetTaskItem obj)
    {
        _petProcessTrain.Initialize(obj.Task);
        _petProcessTrain.Root.SetActive(true);
    }

    private void OnGotAward(PetTaskItem obj)
    {
        var msg = PacketObject.Create<CsPetTaskOperator>();
        msg.operatorCode = 3;
        msg.taskId = obj.Task.ID;
        session.Send(msg);
    }

    private void OnCancelRequest(PetTaskItem obj)
    {
        Window_Alert.ShowAlertDefalut(ConfigText.GetDefalutString(TextForMatType.PetTrainFastComplete, 6),
            () =>
            {
                var msg = PacketObject.Create<CsPetTaskOperator>();
                msg.operatorCode = 1;
                msg.taskId = obj.Task.ID;
                session.Send(msg);
            },
            () => { },
            ConfigText.GetDefalutString(TextForMatType.PetTrainFastComplete, 4),
            ConfigText.GetDefalutString(TextForMatType.PetTrainFastComplete, 5));
    }

    protected override void OnReturn()
    {
        if (_petProcessTrain.Root.activeInHierarchy)
        {
            if (_petProcessTrain.OnReturn())
                return;
            _petProcessTrain.Root.SetActive(false);
            _petProcessTrain.UnInitialize();
            return;
        }
        base.OnReturn();
    }

    private void _ME(ModuleEvent<Module_Pet> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Pet.TaskInfoChange:
                RefreshProcessText();
                taskDataSource.SetItems(modulePet.PetTasks);
                scrollView.SetProgress(0);
                break;
            case Module_Pet.ResponseTaskOprator:
                ResponseTaskOprator(e.msg as ScPetTaskOperator);
                break;
            case Module_Pet.ResponseTrainGotAward:
                ResponseTrainGotAward(e.msg as ScTrainAward);
                break;
        }
    }

    private void ResponseTrainGotAward(ScTrainAward scTrainAward)
    {
        Window_ItemTip.Show(ConfigText.GetDefalutString(TextForMatType.PetTrainText, 27), scTrainAward.awards);
    }

    private void ResponseTaskOprator(ScPetTaskOperator msg)
    {
        if (msg == null)
        {
            return;
        }
        //需要消耗钻石，进行二级确认
        if (msg.cost > 0)
        {
            _costComfirmBox.Initialize(msg);
            return;
        }

        AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
        if (msg.response == 0) return;

        //失败显示tip
        moduleGlobal.ShowMessage(9706, msg.response);
    }

    public void EnterTask(SealPetTaskInfo info)
    {
        _petProcessTrain.Initialize(info);
        _petProcessTrain.Root.SetActive(true);
    }
}



public class CostComfirmBox : SubWindowBase<Window_Train>
{
    [Widget("fastforward/bg/no")]
    private Button Close;

    [Widget("fastforward/bg/xiaohao_tip/remain")]
    public Text CostText;

    [Widget("fastforward/bg/nobtn")]
    private Button NoButton;

    private ScPetTaskOperator Operator;

    [Widget("fastforward/bg/yes")]
    private Button YesButton;
    
    protected override void InitComponent()
    {
        base.InitComponent();

        YesButton?.onClick.AddListener(OnYes);
        NoButton?.onClick.AddListener(() => UnInitialize());
        Close?.onClick.AddListener(() => UnInitialize(false));
    }

    private void OnYes()
    {
        var msg = PacketObject.Create<CsPetTaskOperator>();
        msg.operatorCode = 2;
        msg.comfirm = true;
        msg.taskId = Operator.taskId;
        session.Send(msg);
        UnInitialize();
    }

    public override bool Initialize(params object[] p)
    {
        if (base.Initialize(p))
        {
            Operator = p[0] as ScPetTaskOperator;
            if (Operator != null)
            {
                Util.SetText(CostText, Util.Format(ConfigText.GetDefalutString(TextForMatType.PetTrainFastComplete, 3), Operator.cost, modulePlayer.gemCount));
                if(null != CostText)
                    CostText.color = ColorGroup.GetColor(ColorManagerType.IsMoneyEnough, Operator.cost <= modulePlayer.gemCount);
            }
            return true;
        }
        return false;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (base.UnInitialize(hide))
        {
            return true;
        }
        return false;
    }

    public override bool OnReturn()
    {
        if (Root.activeInHierarchy)
        {
            UnInitialize();
            return true;
        }
        return false;
    }
}