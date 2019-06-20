// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * Extended methods.
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-06-19      17:52
//  * LastModify：2018-07-11      14:53
//  ***************************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetProcess_Train : SubWindowBase
{
    public const int PetSelectMax = 3;

    private PetProcess_TrainSelectPet _petProcessTrainSelectPet;
    private readonly List<Condition> conditionsImpl = new List<Condition>();

    [Widget("missioninfo_panel/pre_group/item_modle", false)]
    public Transform awardsItem;

    [Widget("missioninfo_panel/pre_group")]
    public Transform awardsRoot;

    [ArrayWidget("missioninfo_panel/condition_group/condition_01",
                 "missioninfo_panel/condition_group/condition_02",
                 "missioninfo_panel/condition_group/condition_03")]
    public List<Transform> conditions;

    public SealPetTaskInfo currentTaskInfo;

    [Widget("missioninfo_panel/mission_des")]
    public Text detail;

    [Widget("missioninfo_panel/missionname_txt")]
    public Text messionName;

    [Widget("missioninfo_panel/mission_successrate_txt")]
    public Text mission_successrate_txt;

    [Widget("missioninfo_panel/btn_02")]
    public Button oneKeyDispatch;

    [ArrayWidget(true, "missioninfo_panel/select_group")]
    public List<Button> petBtns;

    [Widget("missioninfo_panel/btn_01")]
    public Button startTrain;

    [Widget("missioninfo_panel/timeremaining_txt")]
    public Text timeremaining_txt;

    [Widget("sprite_info")]
    public Transform trainSelectPet;

    public PetInfo[] ReadlyPets { get; } = new PetInfo[PetSelectMax];

    public int ReadlyPetCount
    {
        get
        {
            var c = 0;
            for (var i = 0; i < ReadlyPets.Length; i++)
            {
                if (ReadlyPets[i] != null)
                    c++;
            }
            return c;
        }
    }

    protected override void InitComponent()
    {
        base.InitComponent();
        _petProcessTrainSelectPet = CreateSubWindow<PetProcess_TrainSelectPet>(WindowCache, trainSelectPet.gameObject);

        for (var i = 0; i < petBtns.Count; i++)
        {
            petBtns[i].onClick.AddListener(OnDispatch);
        }
        conditionsImpl.Clear();
        conditionsImpl.Add(new ConditionCount(conditions[0]));
        conditionsImpl.Add(new ConditionAvgLevel(conditions[1]));
        conditionsImpl.Add(new ConditionTime(conditions[2]));
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _petProcessTrainSelectPet.Destroy();
    }

    public Condition CheckCondition(List<PetInfo> selectList)
    {
        return CheckCondition(new ConditionSourceData(currentTaskInfo, selectList.ToArray(), false));
    }

    /// <summary>
    /// 检测任务条件
    /// </summary>
    /// <param name="checkData"></param>
    /// <returns>返回第一个未达成的条件(null 表示条件全部达成)</returns>
    public Condition CheckCondition(IConditionSourceData checkData)
    {
        //第一个没有完成任务的Condition
        Condition firstCondition = null;
        foreach (var condition in conditionsImpl)
        {
            if (!condition.Check(checkData))
            {
                if(firstCondition == null)
                    firstCondition =  condition;
            }
        }
        return firstCondition;
    }

    public void SetPet(List<PetInfo> rList)
    {
        for (var i = 0; i < PetSelectMax; i++)
        {
            if(rList != null && i < rList.Count)
                SetPet(i, rList[i]);
            else
                SetPet(i, null);
        }
    }

    public void SetPet(int rIndex, PetInfo rPetInfo)
    {
        if (rIndex < 0 || rIndex >= ReadlyPets.Length)
            return;
        ReadlyPets[rIndex] = rPetInfo;

        RefreshPet(petBtns[rIndex].transform, rPetInfo);
        RefreshSuccessRate();
        CheckCondition(new ConditionSourceData(currentTaskInfo, ReadlyPets));
    }

    public override bool Initialize(params object[] p)
    {
        if (base.Initialize(p))
        {
            currentTaskInfo = p[0] as SealPetTaskInfo;
            RefreshTaskDetail();
            startTrain.onClick.AddListener(OnStartTrain);
            oneKeyDispatch.onClick.AddListener(OnOneKeyDispatch);

            for(var i = 0; i < PetSelectMax; i++)
                SetPet(i, null);

            if (currentTaskInfo == null) return false;
            var ct = ConfigManager.Get<ConfigText>((int)TextForMatType.PetTrainText);
            var a = currentTaskInfo.Task.costTime;
            var t = new TimeSpan(a * TimeSpan.TicksPerMinute);
            Util.SetText(timeremaining_txt, Util.Format(ct[6], Util.Format("{0:00}:{1:00}:{2:00}", (int)t.TotalHours, t.Minutes, t.Seconds)));
        }
        return true;
    }


    public override bool UnInitialize(bool hide = true)
    {
        if (base.UnInitialize(hide))
        {
            startTrain.onClick.RemoveListener(OnStartTrain);
            oneKeyDispatch.onClick.RemoveListener(OnOneKeyDispatch);
        }
        return true;
    }

    /// <summary>
    /// 一键派遣宠物
    /// (规则已调整。去掉了成功率的概念)
    /// 规则：尽量满足成功率为100%
    ///       优先挑选高等级、高进阶的精灵，如果1只就可以达到100%，则只挑选1只
    ///       否则就两只、三只，最多3只，最少1只。
    ///       并且，挑选的精灵的平均等级需要满足条件
    /// </summary>
    private void OnOneKeyDispatch()
    {
        var list = modulePet.PetsExcludeFightingPet;
        //        list.Sort((a, b) => -(a.Level * a.Star).CompareTo(b.Level * b.Star));
        list.Sort((a, b) => -(a.Level).CompareTo(b.Level));
        var select = new List<PetInfo>();
        foreach (var petInfo in list)
        {
            if(petInfo.IsTraining) continue;
            select.Add(petInfo);
            var condition = CheckCondition(new ConditionSourceData(currentTaskInfo, select.ToArray(), false));
            //次数不满足,再怎么派遣也没用,否则继续派遣
            if (condition is ConditionTime)
            {
                moduleGlobal.ShowMessage((int)TextForMatType.PetTrainText, 22);
                break;
            }
            //加了这个宠物平均等级就不够了。那就不能加这个宠物了。回退
            if (condition is ConditionAvgLevel)
            {
                select.RemoveAt(select.Count - 1);
                break;
            }

            if (condition is ConditionCount)
            {
                continue;
            }
            break;
//            if (currentTaskInfo.CalcSuccessRate(select) >= 1 && condition == null)
//                break;

//            if(select.Count >= PetSelectMax)
//                break;
        }

        var c = CheckCondition(new ConditionSourceData(currentTaskInfo, select.ToArray(), false));
        //派遣计算成功
        if (c == null)
        {
            //先把原来选中的宠物列表清空
            SetPet(null);
            for (var i = 0; i < select.Count; i++)
            {
                SetPet(i, select[i]);
            }
        }else
            moduleGlobal.ShowMessage((int)TextForMatType.PetTrainText, 22);
    }

    private void OnDispatch()
    {
        _petProcessTrainSelectPet.Initialize(this);
    }

    private void OnGotAward()
    {
        if (currentTaskInfo == null) return;
        var msg = PacketObject.Create<CsPetTaskOperator>();
        msg.taskId = currentTaskInfo.ID;
        msg.operatorCode = 3;
        session.Send(msg);
    }

    public void OnStartTrain()
    {
        var condition = CheckCondition(new ConditionSourceData(currentTaskInfo, ReadlyPets));
        if(condition != null)
        {
            moduleGlobal.ShowMessage(condition.WarningText);
            return;
        }
        _petProcessTrainSelectPet.UnInitialize();
        if (currentTaskInfo == null) return;
        var msg = PacketObject.Create<CsPetTaskStart>();
        msg.taskId = currentTaskInfo.ID;
        msg.trainPetId = GetReadlyPetItemID();

        session.Send(msg);
    }

    private ulong[] GetReadlyPetItemID()
    {
        var list = new List<ulong>();
        for (var i = 0; i < ReadlyPets.Length; i++)
        {
            var pet = ReadlyPets[i];
            if(pet != null)
                list.Add(pet.ItemID);
        }
        return list.ToArray();
    }

    private void RefreshTaskDetail()
    {
        if (null == currentTaskInfo) return;
        RefreshSuccessRate();
        Util.SetText(messionName, currentTaskInfo.Task.name);
        Util.SetText(detail, currentTaskInfo.Task.desc);
        RefreshTaskAwards();
    }

    private void RefreshSuccessRate()
    {
        var ct = ConfigManager.Get<ConfigText>((int)TextForMatType.PetTrainText);
        var rate = currentTaskInfo.CalcSuccessRate(ReadlyPets);
        Util.SetText(mission_successrate_txt, Util.Format(ct.text[9], rate));
        mission_successrate_txt.color = ColorGroup.GetColor(ColorManagerType.TaskSuccessRate, rate);
    }

    private void RefreshTaskAwards()
    {
        if (currentTaskInfo == null || currentTaskInfo.Task.reward.props == null)
            return;
        Util.ClearChildren(awardsRoot);

        //固定奖励
        var rList = new List<ItemPair>();
        //屏蔽掉金币和钻石的显示
        //if (this.currentTaskInfo.Task.reward.coin > 0)
        //    rList.Add(new ItemPair() { itemId = 1, count = this.currentTaskInfo.Task.reward.coin});
        //if (this.currentTaskInfo.Task.reward.diamond > 0)
        //    rList.Add(new ItemPair() { itemId = 2, count = this.currentTaskInfo.Task.reward.diamond});

        var items = currentTaskInfo.Task.previewRewardItemId;
        if (items == null) return;
        for (var i = 0; i < items.Length; i++)
        {
            rList.Add(new ItemPair {itemId = items[i], count = 1});
        }

        for (var i = 0; i < rList.Count; i++)
        {
            var propInfo = ConfigManager.Get<PropItemInfo>(rList[i].itemId);
            if (propInfo == null) continue;
            var t = awardsRoot.AddNewChild(awardsItem);
            t.SafeSetActive(true);

            Util.SetItemInfoSimple(t, propInfo);
            var number = t.Find("numberdi");
            number.SafeSetActive(rList[i].count > 1);
            if (rList[i].count > 1)
            {
                Util.SetText(t.GetComponent<Text>("numberdi/count"), rList[i].count);
            }
            var button = t.GetComponent<Button>();
            var itemId = (ushort)rList[i].itemId;
            if (button) button.onClick.AddListener(() => moduleGlobal.UpdateGlobalTip(itemId, true));
        }
    }

    private void RefreshPet(Transform t, PetInfo rPetInfo)
    {
        if (!t) return;
        for (var i = 0; i < t.childCount; i++)
        {
            t.GetChild(i).SafeSetActive(rPetInfo != null);
        }
        if (rPetInfo != null)
        {
            if (rPetInfo.UpGradeInfo != null)
            {
                UIDynamicImage.LoadImage(t.GetComponent<Transform>("sprite"), rPetInfo.UpGradeInfo.halfIcon, null, false);
            }
            Util.SetText(t.GetComponent<Text>("level_img/level_txt"), rPetInfo.Level.ToString());
            var stars = t.GetComponent<Transform>("stars/count");
            if (stars == null) return;
            for (var i = 0; i < stars.childCount; i++)
            {
                stars.GetChild(i).SafeSetActive(i < rPetInfo.Star);
            }
        }
    }


    private void _ME(ModuleEvent<Module_Pet> e)
    {
        if (currentTaskInfo == null) return;
        switch (e.moduleEvent)
        {
            case Module_Pet.TaskInfoChange:
            {
                var task = e.param1 as SealPetTaskInfo;
                if (task != null && task.Task.ID == currentTaskInfo.Task.ID)
                {
                    if (task.State == 1)
                    {
                        int index = modulePlayer.proto - 1;
                        int max = AudioInLogicInfo.audioConst.petTrain.Length - 1;
                        index = index < 0 ? 0 : index > max ? max : index;
                        AudioManager.PlayVoice(AudioInLogicInfo.audioConst.petTrain[index]);
                    }

                    UnInitialize();
                }
            }
            break;
        }
    }

    public override bool OnReturn()
    {
        if (_petProcessTrainSelectPet.UnInitialize())
            return true;
        return UnInitialize();
    }

    public interface IConditionSourceData
    {
        int PetCount { get; }
        int PetCountMin { get; }
        int AvgLevel { get; }
        int AvgLevelMin { get; }
        int FinishCount { get; }
        int FinishCountMax { get; }

        bool UpdateUIData { get; }
    }

    private class ConditionSourceData : IConditionSourceData
    {
        private readonly PetInfo[] pets;

        private readonly SealPetTaskInfo taskInfo;

        public ConditionSourceData(SealPetTaskInfo rTaskInfo, PetInfo[] rPets, bool rUpdateUIData = true)
        {
            taskInfo = rTaskInfo;
            pets = rPets;
            UpdateUIData = rUpdateUIData;
        }

        public int PetCount
        {
            get
            {
                if (pets == null || pets.Length == 0) return 0;
                var count = 0;
                for (var i = 0; i < pets.Length; i++)
                {
                    if (pets[i] != null)
                        count++;
                }
                return count;
            }
        }

        public int PetCountMin { get { return taskInfo?.Task.petCountMin ?? 0; } }

        public int AvgLevel
        {
            get
            {
                if (pets == null || pets.Length == 0) return 0;
                var count = 0;
                var level = 0;
                for (var i = 0; i < pets.Length; i++)
                {
                    if (pets[i] != null)
                    {
                        level += pets[i].Level;
                        count++;
                    }
                }
                if (count == 0) return 0;
                return level/count;
            }
        }

        public int AvgLevelMin { get { return taskInfo?.Task.level ?? 0; } }
        public int FinishCount { get { return taskInfo?.Count ?? 0; } }
        public int FinishCountMax { get { return taskInfo?.LimitCount ?? 0; } }
        public virtual bool UpdateUIData { get; }
    }

    public abstract class Condition
    {
        protected Text conditionText;
        protected Image condtionCheck;

        protected Condition(Transform rRoot)
        {
            conditionText = rRoot.GetComponent<Text>("condition_txt");
            condtionCheck = rRoot.GetComponent<Image>("condition_check");
        }

        public string ConditionText
        {
            get
            {
                if (conditionText != null)
                    return conditionText.text;
                return string.Empty;
            }
        }

        internal string WarningText { get; set; }

        public abstract bool Check(IConditionSourceData data);
    }

    internal sealed class ConditionCount : Condition
    {
        public ConditionCount(Transform rRoot) : base(rRoot)
        {
        }

        public override bool Check(IConditionSourceData data)
        {
            var flag = data.PetCount >= data.PetCountMin;
            if (data.UpdateUIData)
            {
                var ct = ConfigManager.Get<ConfigText>((int)TextForMatType.PetTrainText);
                Util.SetText(conditionText, Util.Format(ct[19], data.PetCountMin));
                WarningText = conditionText.text;
                condtionCheck.SafeSetActive(flag);
            }
            return flag;
        }
    }

    internal sealed class ConditionAvgLevel : Condition
    {
        public ConditionAvgLevel(Transform rRoot) : base(rRoot)
        {
        }

        public override bool Check(IConditionSourceData data)
        {
            var flag = data.AvgLevel >= data.AvgLevelMin;
            if (data.UpdateUIData)
            {
                var ct = ConfigManager.Get<ConfigText>((int)TextForMatType.PetTrainText);
                Util.SetText(conditionText, Util.Format(ct[20], data.AvgLevelMin));
                condtionCheck.SafeSetActive(flag);
                WarningText = Util.Format(ct[25], data.AvgLevelMin);
            }
            return flag;
        }
    }

    internal sealed class ConditionTime : Condition
    {
        public ConditionTime(Transform rRoot) : base(rRoot)
        {
        }

        public override bool Check(IConditionSourceData data)
        {
            var t = data.FinishCountMax > 0 ? 1 : 0;
            var flag = data.FinishCountMax - data.FinishCount >= t;
            if (data.UpdateUIData)
            {
                var ct = ConfigManager.Get<ConfigText>((int)TextForMatType.PetTrainText);
                Util.SetText(conditionText, Util.Format(ct[21], t));
                WarningText = ConditionText;
                condtionCheck.SafeSetActive(flag);
            }
            return flag;
        }
    }
}

