// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-31      19:45
//  * LastModify：2018-11-02      10:24
//  ***************************************************************************************************/
#region

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

#endregion

#region Pet相关枚举

public enum PetTaskState
{
    None, //未开始
    Training, //历练中
    Success, //历练成功
    Defeat //历练失败
}

public enum EnumPetStatus
{
    Idle,
    Fighting,
    Training
}

public enum EnumPetMood
{
    /// <summary>
    ///     平淡
    /// </summary>
    Normal,

    /// <summary>
    ///     愉悦
    /// </summary>
    Good,

    /// <summary>
    ///     高兴
    /// </summary>
    Happy,

    /// <summary>
    ///     兴奋
    /// </summary>
    Excitement
}

#endregion

public class Module_Pet : Module<Module_Pet>
{
    #region Events
    public const string PetGradeChange          = "PetGradeChange";
    public const string LevelChange             = "LevelChange";
    public const string ExpChange               = "ExpChange";
    public const string MoodChange              = "MoodChange";
    public const string PetStatusChange         = "PetStatusChange";
    public const string EventChangeAttribute    = "EventChangeAttribute";
    public const string TaskInfoChange          = "TaskInfoChange";
    public const string PetListChange           = "PetListChange";

    public const string SummonSuccess           = "SummonSuccess";
    public const string ResponseBuySummonStone  = "ResponseBuySummonStone";
    public const string ResponseFeed            = "ResponseFeed";
    public const string ResponseStatus          = "ResponseStatus";
    public const string ResponseEvolve          = "ResponseEvolve";
    public const string ResponseTease           = "ResponseTease";
    public const string ResponseTaskOprator     = "ResponseTaskOprator";
    public const string ResponseTrainGotAward   = "TrainGotAward";
    public const string CloseToNpc              = "OnCloseToNpc";
    public const string FarAwayNpc              = "OnFarAwayNpc";
    public const string EventDropInfoUpdate     = "OnDropInfoUpdate";
    public const string EventGetNewPet          = "EventGetNewPet";//获得宠物

    public const string EventEggAnimEnd         = "EventEggAnimEnd";
    public const string EventEggClick           = "EggClick";
    #endregion

    public       int    buyStoneCostDiamond     = 100;
    public       int    costStone               = 1;

    #region functions

    protected override void OnModuleCreated()
    {
        base.OnModuleCreated();
        session.AddEventListener(Events.SESSION_LOST_CONNECTION, OnDisconnect);
        session.AddEventListener(Events.SESSION_CONNECTED, OnConnect);
    }

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        NewPetList.Clear();
        PetList.Clear();
        PetTasks.Clear();
        TaskList.Clear();
    }

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        RequestPetTask();

        var tasks = ConfigManager.GetAll<PetTask>();
        foreach (var task in tasks)
        {
            AssertTask(task.ID);
        }
    }

    private void OnConnect()
    {
        modulePlayer.AddEventListener(Module_Player.EventLevelChanged, UpdatePetTaskList);
    }

    private void OnDisconnect()
    {
        modulePlayer.RemoveEventListener(Module_Player.EventLevelChanged, UpdatePetTaskList);
    }

    private void UpdatePetTaskList()
    {
        _petTasks = TaskList.FindAll( item =>
                    item.State != 0 || (modulePlayer.level >= item.Task.minlv && modulePlayer.level <= item.Task.maxlv));
        _petTasks.Sort(TaskCompare);

        DispatchModuleEvent(TaskInfoChange);
        moduleHome.UpdateIconState(HomeIcons.Pet, NeedNotice);
    }

    /// <summary>
    ///     是否拥有某宠物
    /// </summary>
    /// <param name="rPetID">宠物ID</param>
    /// <returns></returns>
    public bool Contains(int rPetID)
    {
        return GetPet(rPetID) != null;
    }

    public PetInfo GetPet(int rPetID)
    {
        return PetList.Find(info => info.CPetInfo.ID.Equals(rPetID));
    }

    public PetInfo GetPet(ulong rPetID)
    {
        return PetList.Find(info => info.ItemID.Equals(rPetID));
    }

    /// <summary>
    ///     增加或更新宠物列表信息
    /// </summary>
    /// <param name="rInfo"></param>
    /// <param name="isNew"></param>
    /// <returns>true 成功增加  false 更新宠物信息</returns>
    public bool AddPet(PItem rInfo, bool isNew = true)
    {
        var pet = GetPet(rInfo.itemTypeId);
        if (pet == null)
        {
            pet = PetInfo.Create(rInfo);
            if (pet != null)
            {
                PetList.Add(pet);
                if (isNew)
                {
                    NewPetList.Add(pet.ID);
                    DispatchModuleEvent(EventGetNewPet, pet, null);
                }
                return true;
            }
        }
        else
            pet.InitData(rInfo);
        return false;
    }

    #region 宠物任务

    public SealPetTaskInfo GetTask(int taskId)
    {
        return TaskList.Find(item => item.ID == taskId);
    }

    public SealPetTaskInfo AssertTask(int taskId)
    {
        var task = GetTask(taskId);
        if (null == task)
        {
            task = new SealPetTaskInfo(taskId);
            TaskList.Add(task);
        }
        return task;
    }

    #endregion

    #region Request

    public void Summon(byte rTimes)
    {
        var msg = PacketObject.Create<CsPetSummon>();
        msg.summonTimes = Math.Max((byte)1, rTimes);
        session.Send(msg);
        moduleGlobal.LockUI(0.5f, 0, 0x02);
    }

    public void BuySummonStone(bool lockUI, int rTimes)
    {
        if (lockUI) moduleGlobal.LockUI(0.2f, 0, 0x02);
        var m = PacketObject.Create<CsBuySummon>();
        m.count = Math.Max(1, (uint)rTimes);
        session.Send(m);
    }

    #endregion

    #region _Packet

    private void RequestPetTask()
    {
        var msg = PacketObject.Create<CsPetTaskInfos>();
        session.Send(msg);
    }

    private void _Packet(ScPetItemDropInfo p)
    {
        p.CopyTo(ref m_dropInfo);
        DispatchModuleEvent(EventDropInfoUpdate);
    }

    private void _Packet(ScSystemSetting p)
    {
        costStone = p.costStone;
        buyStoneCostDiamond = p.buyStoneCostDiamond;
    }

    private void _Packet(ScPetInfos msg)
    {
        if (msg.pets == null || msg.pets.Length == 0)
            return;
        PetList.Clear();
        NewPetList.Clear();
        for (var i = 0; i < msg.pets.Length; i++)
        {
            AddPet(msg.pets[i], false);
        }

        DispatchModuleEvent(PetListChange);
        moduleHome.UpdateIconState(HomeIcons.Pet, NeedNotice);
    }

    private void _Packet(ScPetFeed msg)
    {
        if (msg.response == 0)
        {
            //Logger.LogError(Util.Format("Level = {0} -- exp = {1}", msg.level, msg.exp));
            var pet = GetPet(msg.petId);
            pet.Level = msg.level;
            pet.Exp = msg.exp;
        }
        DispatchModuleEvent(ResponseFeed, msg);
    }

    private void _Packet(ScPetTease msg)
    {
        if (msg.response == 0)
        {
            var pet = GetPet(msg.petId);
            pet.MoodValue = msg.mood;
        }
        DispatchModuleEvent(ResponseTease, msg);
    }

    private void _Packet(ScPetStatus msg)
    {
        for (var i = 0; i < msg.petStatus.Length; i++)
        {
            var pet = GetPet(msg.petStatus[i].petId);
            if (pet != null)
                pet.Status = msg.petStatus[i].status;
        }
    }

    private void _Packet(ScPetUpgrade msg)
    {
        if (msg.response == 0)
        {
            var pet = GetPet(msg.petId);
            pet.Grade = msg.grade;
        }
        DispatchModuleEvent(ResponseEvolve, msg);
    }

    private void _Packet(ScPetTaskInfos msg)
    {
        if (msg == null || msg.infoList == null)
            return;
        for (var i = 0; i < msg.infoList.Length; i++)
        {
            var info = msg.infoList[i];
            var sealtask = GetTask(info.taskId);
            if (sealtask == null)
            {
                Logger.LogError("宠物任务客户端和服务器不一致。客户端没有此任务ID = {0} ", info.taskId);
                continue;
            }
            sealtask.SetPetTaskInfo(info);
        }
        UpdatePetTaskList();
    }

    private void _Packet(ScPetTaskInfoChange msg)
    {
        var taskInfo = GetTask(msg.info.taskId);
        if (taskInfo != null)
        {
            taskInfo.SetPetTaskInfo(msg.info);
            UpdatePetTaskList();
            DispatchModuleEvent(TaskInfoChange, taskInfo);
        }
        else
        {
            Logger.LogError("there is not find task:{0}", msg.info.taskId);
        }
    }

    public void _Packet(ScPetSummon msg)
    {
        DispatchModuleEvent(SummonSuccess, msg);
    }

    public void _Packet(ScBuySummon msg)
    {
        DispatchModuleEvent(ResponseBuySummonStone, msg.response);
    }


    private void _Packet(ScPetTaskOperator msg)
    {
        DispatchModuleEvent(ResponseTaskOprator, msg);
    }

    private void _Packet(ScTrainAward msg)
    {
        DispatchModuleEvent(ResponseTrainGotAward, msg);
    }

    #endregion

    #endregion

    #region Fields

    public readonly List<int> NewPetList = new List<int>();

    /// <summary>
    ///     当前拥有的宠物列表
    /// </summary>
    public readonly List<PetInfo> PetList = new List<PetInfo>();

    private readonly List<SealPetTaskInfo> TaskList = new List<SealPetTaskInfo>();

    /// <summary>
    ///     只返回角色当前能做的任务
    /// </summary>
    public List<SealPetTaskInfo> PetTasks
    {
        get
        {
            if (_petTasks == null)
                UpdatePetTaskList();
            return _petTasks;
        }
    }


    private List<SealPetTaskInfo> _petTasks;

    public PWishItemDropInfo[] dropInfo { get { return m_dropInfo != null && m_dropInfo.dropinfo != null ? m_dropInfo.dropinfo : new PWishItemDropInfo[] { }; } }

    private ScPetItemDropInfo m_dropInfo;

    #endregion

    #region property

    public List<PetInfo> PetsExcludeFightingPet
    {
        get
        {
            var list = new List<PetInfo>(modulePet.PetList);
            list.Remove(modulePet.FightingPet);
            return list;
        }
    }

    /// <summary>
    ///     当前出战的宠物
    /// </summary>
    public int FightingPetID
    {
        get
        {
            var pet = FightingPet;
            return pet != null ? pet.ID : 0;
        }
    }

    public PetInfo FightingPet
    {
        get
        {
            return PetList.Find(pet => pet.IsFighting);
        }
    }

    public bool AnyTaskFinish
    {
        get
        {
            return PetTasks.Exists(item => item.State == (int) PetTaskState.Success || item.State == (int) PetTaskState.Defeat);
        }
    }

    public bool NeedNoticeSimple
    {
        get { return (NewPetList.Count > 0 || AnyPetCanFeed || AnyPetCanEvolve || AnyPetCanCompond); }
    }

    public bool NeedNotice
    {
        get
        {
            var key = new NoticeDefaultKey(NoticeType.Pet);
            moduleNotice.SetNoticeState(key, NeedNoticeSimple);
            return moduleNotice.IsNeedNotice(key);
        }
    }

    public bool AnyPetCanEvolve
    {
        get
        {
            return PetList.Exists(pet => pet.CanEvolve());
        }
    }

    public bool AnyPetCanFeed
    {
        get
        {
            return PetList.Exists(pet => pet.CanFeed());
        }
    }

    public bool AnyPetCanCompond
    {
        get
        {
            foreach (var pet in GetAllPet())
            {
                //已经拥有的不提示了
                if (Contains(pet.ID))
                    continue;
                if (pet.CanNoticeCompond())
                    return true;
            }
            return false;
        }
    }

    #endregion

    #region static

    #region SortHandle

    private static int TaskCompare(SealPetTaskInfo a, SealPetTaskInfo b)
    {
        var aTimesUseUp = a.TimesUseUp && a.State == (int)PetTaskState.None;
        var bTimesUseUp = b.TimesUseUp && b.State == (int)PetTaskState.None;
        if (!aTimesUseUp.Equals(bTimesUseUp))
            return aTimesUseUp.CompareTo(b.TimesUseUp);
        if (a.State.Equals(b.State))
        {
            return a.ID.CompareTo(b.ID);
//            if (a.Task.type.Equals(b.Task.type))
//                return a.Task.level.CompareTo(b.Task.level);
//            return -a.Task.type.CompareTo(b.Task.type);
        }
        return -a.State.CompareTo(b.State);
    }

    #endregion

    /// <summary>
    ///     获取所有配置中的宠物信息
    /// </summary>
    /// <returns></returns>
    public static List<PetInfo> GetAllPet()
    {
        var rList = ConfigManager.FindAll<PropItemInfo>(item => item.itemType == PropType.Pet);

        var res = new List<PetInfo>();
        foreach (var info in rList)
        {
            var petInfo = modulePet.GetPet(info.ID);
            res.Add(petInfo ?? PetInfo.Create(info));
        }

        res.Sort(CompareHandle);
        return res;
    }

    private static int CompareHandle(PetInfo a, PetInfo b)
    {
        if (a.IsFighting == b.IsFighting)
        {
            var owna = modulePet.Contains(a.ID);
            var ownb = modulePet.Contains(b.ID);
            if (owna == ownb)
            {
                var composeA = a.CanNoticeCompond();
                var composeB = b.CanNoticeCompond();
                if(composeA == composeB)
                    return -a.CPetInfo.quality.CompareTo(b.CPetInfo.quality);
                return -composeA.CompareTo(composeB);
            }
            return -owna.CompareTo(ownb);
        }
        return -a.IsFighting.CompareTo(b.IsFighting);
    }

    #endregion

    #region GM functions

    public void GMAddPetMatrials(PetInfo petInfo)
    {
        var list = new List<ItemPair>();
        var info = ConfigManager.Get<PetUpGradeInfo>(petInfo.ID);
        for (var i = 0; i < info.upGradeInfos.Length; i++)
        {
            list.AddRange(info.upGradeInfos[i].upgradeCost.items);
        }
        for (var i = 0; i < list.Count; i++)
            moduleGm.AddProp((ushort)list[i].itemId, list[i].count);
    }

    public void GMAddPetLevelMatrials(PetInfo petInfo)
    {
        var items = ConfigManager.FindAll<PropItemInfo>(item => item.itemType == PropType.PetFood && item.subType == 1);
        if (items == null || items.Count == 0)
            return;
        for (var i = 0; i < items.Count; i++)
            moduleGm.AddProp((ushort)items[i].ID, 999);
    }

    #endregion
}


public class PetInfo
{
    #region Fields

    public PropItemInfo CPetInfo;
    public PItem        Item;
    private int         _grade;
    private int         _level;
    private uint        _exp;
    private uint        _moodValue;
    private byte        _status;

    #endregion

    #region Properties

    public ulong ItemID { get; set; }

    public byte Status
    {
        get { return _status; }
        set
        {
            if (!_status.Equals(value))
            {
                _status = value;
                Module_Pet.instance.DispatchModuleEvent(Module_Pet.PetStatusChange, this);
            }
        }
    }

    public string SkillName { get { return Util.Format(ConfigText.GetDefalutString(GetSkill().skillName), AdditiveLevel); } }

    /// <summary>
    ///     当前进阶等级
    /// </summary>
    public int Grade
    {
        get { return _grade; }
        set
        {
            var petGrade = ConfigManager.Get<PetUpGradeInfo>(ID);
            if (petGrade != null && petGrade.upGradeInfos != null && petGrade.upGradeInfos.Length > 0)
            {
                value = Math.Min(value, petGrade.upGradeInfos[petGrade.upGradeInfos.Length - 1].level);
            }
            if (_grade != value)
            {
                _grade = value;
                Module_Pet.instance.DispatchModuleEvent(Module_Pet.PetGradeChange, this);
            }
        }
    }

    public int Level
    {
        get { return _level; }
        set
        {
            if (!_level.Equals(value))
            {
                _level = value;
                Module_Pet.instance.DispatchModuleEvent(Module_Pet.LevelChange, this);
            }
        }
    }

    public uint Exp
    {
        get { return _exp; }
        set
        {
            if (!_exp.Equals(value))
            {
                _exp = value;
                Module_Pet.instance.DispatchModuleEvent(Module_Pet.ExpChange, this);
            }
        }
    }

    /// <summary>
    ///     心情
    /// </summary>
    public EnumPetMood Mood
    {
        get
        {
            var list = ConfigManager.GetAll<PetMood>();
            list.Sort((a, b) => -a.moodValue.CompareTo(b.moodValue));
            foreach (var m in list)
            {
                if (m.moodValue <= _moodValue)
                    return m.mood;
            }
            return EnumPetMood.Normal;
        }
    }

    public uint MoodValue
    {
        get { return _moodValue; }
        set
        {
            if (!_moodValue.Equals(value))
            {
                var o = Mood;
                _moodValue = value;
                if (!o.Equals(Mood))
                {
                    Module_Pet.instance.DispatchModuleEvent(Module_Pet.MoodChange, this);
                }
            }
        }
    }

    /// <summary>
    ///     是否处于历练中
    /// </summary>
    public bool IsTraining
    {
        get { return _status == (byte) EnumPetStatus.Training; }
    }

    public bool IsFighting
    {
        get { return _status == (byte) EnumPetStatus.Fighting; }
    }

    public bool IsIdle
    {
        get { return _status == (byte) EnumPetStatus.Idle; }
    }

    public int AdditiveLevel
    {
        get { return Grade + (int)Mood; }
    }

    public int ID
    {
        get { return CPetInfo?.ID ?? 0; }
    }

    public List<ItemAttachAttr> Attribute
    {
        get { return GetAttribute(Level, Grade); }
    }

    public PetUpGradeInfo.UpGradeInfo UpGradeInfo
    {
        get { return GetUpGradeInfo(Grade); }
    }

    /// <summary>
    ///     获得当前宠物进阶等级的星星。
    /// </summary>
    public int Star
    {
        get { return GetStar(Grade); }
    }

    #endregion

    #region functions

    public CreatureInfo BuildCreatureInfo()
    {
        if (UpGradeInfo == null) return null;

        var i = ConfigManager.Get<CreatureInfo>(Module_Home.CREATURE_TEMPLATE).Clone<CreatureInfo>();
        i.ID                = ID;
        i.weaponItemID      = 0;
        i.offWeaponItemID   = 0;
        i.gender            = 0;
        i.models            = new[] { UpGradeInfo.fightMode };
        i.weaponID          = UpGradeInfo.stateMachine;
        i.offWeaponID       = -1;  // 宠物没有副武器
        i.colliderSize      = Vector2_.zero;
        i.hitColliderSize   = Vector2_.zero;

        return i;
    }

    public void InitData(PItem rInfo)
    {
        ItemID = rInfo.itemId;
        rInfo.CopyTo( ref Item);
        //Item = rInfo;
        if (rInfo.growAttr?.petAttr != null)
        {
            Grade = rInfo.growAttr.petAttr.grade;
            Level = rInfo.growAttr.petAttr.level;
            Exp = rInfo.growAttr.petAttr.exp;
            MoodValue = rInfo.growAttr.petAttr.moodValue;
            Status = rInfo.growAttr.petAttr.status;
        }
    }

    public BuffInfo[] GetBuff(int rStar, PetUpGradeInfo.UpGradeInfo gardeInfo = null)
    {
        var list = new List<BuffInfo>();
        var skill = GetSkill();
        if (skill != null)
        {
            foreach (var buffId in skill.buffs)
            {
                var buff = ConfigManager.Get<BuffInfo>(buffId);
                if (buff == null) continue;
                list.Add(buff.CalcLevel(AdditiveLevel));
            }
        }

        return list.ToArray();
    }
    public int[] GetInitBuff()
    {
        var skill = GetSkill();
        return skill?.initBuffs;
    }

    public PetSkill.Skill GetSkill(PetUpGradeInfo.UpGradeInfo gardeInfo = null)
    {
        if (gardeInfo == null)
            gardeInfo = UpGradeInfo;

        var skills = ConfigManager.Get<PetSkill>(ID);
        if (skills == null)
        {
            Logger.LogError("该宠物没有配置技能。宠物ID = {0}", ID);
            return null;
        }

        var skill = skills.GetSkill(gardeInfo.skillID);
        return skill;
    }

    public PetUpGradeInfo.UpGradeInfo GetUpGradeInfo(int rGrade)
    {
        var infos = ConfigManager.Get<PetUpGradeInfo>(ID);
        if (infos == null) return null;
        var index = Mathf.Clamp(rGrade - 1, 0, infos.upGradeInfos.Length - 1);
        return infos.upGradeInfos[index];
    }

    public bool IsEvolveMax()
    {
        var infos = ConfigManager.Get<PetUpGradeInfo>(ID);
        if (infos == null) return false;
        var index = infos.upGradeInfos.Length - 1;
        return infos.upGradeInfos[index].level == Grade;
    }

    public int GetStar(int rGrade)
    {
        var petGrade = ConfigManager.Get<PetUpGradeInfo>(ID);
        if (null == petGrade || null == petGrade.upGradeInfos)
            return 0;
        var index = Mathf.Clamp(rGrade - 1, 0, petGrade.upGradeInfos.Length - 1);
        return petGrade.upGradeInfos[index].star;
    }

    public bool Talk(out int oTalkID)
    {
        var petInfo = ConfigManager.Get<ConfigPetInfo>(ID);
        if (Random.Range(0, 1.0f) < petInfo.TalkRate)
        {
            oTalkID = petInfo.Words.Random();
            return true;
        }
        oTalkID = 0;
        return false;
    }

    public List<ItemAttachAttr> GetAttribute(int rLevel, int rGrade)
    {
        var petAttr = ConfigManager.Get<PetAttributeInfo>(ID);
        if (null == petAttr || petAttr.PetAttributes == null)
            return null;

        var growList = new List<ItemAttachAttr>();
        for (int i = 0, iMax = petAttr.PetAttributes.Length; i < iMax && petAttr.PetAttributes[i].level <= rLevel; i++)
        {
            growList.AddRange(petAttr.PetAttributes[i].attributes);
        }

        var infos = ConfigManager.Get<PetUpGradeInfo>(ID);
        for (int i = 0, iMax = infos.upGradeInfos.Length; i < iMax && infos.upGradeInfos[i].level <= rGrade; i++)
        {
            growList.AddRange(infos.upGradeInfos[i].attributes);
        }

        //从基础数据上拷贝一个列表出来。不要修改基础属性
        var res = new List<ItemAttachAttr>();
        for (var i = 0; i < CPetInfo.attributes.Length; i++)
        {
            var att = CPetInfo.attributes[i];
            var a = new ItemAttachAttr {id = att.id, type = att.type, value = att.value};
            foreach (var attr in growList)
            {
                a += att.CalcGrow(attr);
            }
            res.Add(a);
        }

        return res;
    }

    public float GetExpSlider()
    {
        var upLevelData = GetUpLevelData(Level);
        if (upLevelData == null)
            return 0;
        return (float) Exp/upLevelData.exp;
    }

    public float GetExpProcess()
    {
        return GetExpSlider() + Level;
    }

    public PetAttributeInfo.PetAttribute GetUpLevelData(int rLevel)
    {
        var upInfo = ConfigManager.Get<PetAttributeInfo>(ID);
        if (upInfo == null) return null;
        var idx = Mathf.Clamp(rLevel - 1, 0, upInfo.PetAttributes.Length - 1);
        var upLevelData = upInfo.PetAttributes[idx];
        return upLevelData;
    }

    public bool CanEvolve()
    {
        RefreshEvolveState();
        return Module_Notice.instance.IsNeedNotice(new PetNoticeKey(NoticeType.PetEvolve, ID));
    }

    public bool CanFeed()
    {
        RefreshFeedState();
        if (Level < Module_Player.instance.level)
        {
            PetAttributeInfo.PetAttribute levelAttr = GetLevelCanFeed();
            if (levelAttr == null) return false;
            var result =  Module_Notice.instance.IsNeedNotice(new PetNoticeKey(NoticeType.PetFeed, ID), levelAttr.level.ToString());
            return result;
        }
        return false;
    }

    private PetAttributeInfo.PetAttribute GetLevelCanFeed()
    {
        var items = ConfigManager.FindAll<PropItemInfo>(item => item.itemType == PropType.PetFood && item.subType == 1);
        if (items == null || items.Count == 0)
            return null;
        List<PItem> matrials = new List<PItem>();
        foreach (var item in items)
        {
            var pItem = Module_Equip.instance?.GetProp(item.ID);
            if (pItem?.num > 0)
            {
                matrials.Add(pItem);
            }
        }

        uint fragment;
        return GetPreviewLevel(this, matrials, out fragment);
    }

    public void RefreshFeedState()
    {
        var levelAttr = GetLevelCanFeed();
        if (levelAttr == null) return;
        var key = new PetNoticeKey(NoticeType.PetFeed, ID);
        Module_Notice.instance.SetNoticeState(key, levelAttr.level > Level);
    }

    public void RefreshEvolveState()
    {
        PetNoticeKey key = new PetNoticeKey(NoticeType.PetEvolve, ID);
        if (IsEvolveMax())
            Module_Notice.instance.SetNoticeState(key, false);

        var costInfo = UpGradeInfo.upgradeCost;
        if (null == costInfo)
            Module_Notice.instance.SetNoticeState(key, false);
        else
            Module_Notice.instance.SetNoticeState(key,
                costInfo.gold <= Module_Player.instance?.coinCount && PetProcess_Evolve.IsMatrialEnough(costInfo.items));
    }

    public void RefreshFeedReadState()
    {
        var levelAttr = GetLevelCanFeed();
        if (levelAttr == null) return;
        var key = new PetNoticeKey(NoticeType.PetFeed, ID);
        Module_Notice.instance.SetNoticeState(key, levelAttr.level > Level);
        Module_Notice.instance.SetNoticeReadState(key, levelAttr.level.ToString());
    }

    public void RefreshEvolveReadState()
    {
        RefreshEvolveState();
        var key = new PetNoticeKey(NoticeType.PetEvolve, ID);
        Module_Notice.instance.SetNoticeReadState(key);
    }

    public void RefreshCommondState()
    {
        Module_Notice.instance?.SetNoticeState(new PetNoticeKey(NoticeType.PetCompond, ID), CanCompond());
    }

    public bool CanCompond()
    {
        bool complete = false;
        if (Module_Pet.instance?.Contains(ID) ?? false)
            return false;
        var com = ConfigManager.Find<Compound>(item => Array.Exists(item.items, a => a.itemId == ID));
        if (null != com)
            complete = Module_Equip.instance?.GetPropCount(com.sourceTypeId) >= com.sourceNum &&
                       Module_Player.instance?.coinCount >= com.coin &&
                       Module_Player.instance?.gemCount >= com.diamond;
        return complete;
    }

    public void RefreshCompondReadState()
    {
        RefreshCommondState();
        Module_Notice.instance.SetNoticeReadState(new PetNoticeKey(NoticeType.PetCompond, ID));
    }

    public bool CanNoticeCompond()
    {
        RefreshCommondState();
        return Module_Notice.instance.IsNeedNotice(new PetNoticeKey(NoticeType.PetCompond, ID));
    }

    #endregion

    #region static

    public static PetInfo Create(PropItemInfo rConfig)
    {
        var res = new PetInfo();
        res._grade      = 1;
        res._level      = 1;
        res._exp        = 0;
        res._status     = 0;
        res.CPetInfo    = rConfig;

        return res;
    }

    public static PetInfo Create(PItem item)
    {
        var cInfo = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
        if (cInfo == null)
        {
            Logger.LogError("未能找到宠物{0}的相关配置。请策划检查", item.itemTypeId);
            return null;
        }
        var pet = Create(cInfo);
        pet.InitData(item);
        return pet;
    }


    public static PetAttributeInfo.PetAttribute GetPreviewLevel(PetInfo petInfo, List<PItem> rMatrials, out uint rFragment)
    {
        var exp = GetTotalExp(rMatrials);

        var levelInfo = ConfigManager.Get<PetAttributeInfo>(petInfo.ID);
        if (levelInfo == null ||
            levelInfo.PetAttributes == null ||
            levelInfo.PetAttributes.Length == 0)
        {
            rFragment = 0;
            return new PetAttributeInfo.PetAttribute { exp = 0, level = 1 };
        }
        for (var i = petInfo.Level - 1; i < levelInfo.PetAttributes.Length && exp >= 0; i++)
        {
            var info = levelInfo.PetAttributes[i];
            var inteval = info.exp - (petInfo.Level == info.level ? petInfo.Exp : 0);
            if (exp < inteval)
            {
                rFragment = info.exp - (inteval - exp);
                //升至角色最大等级就不能涨经验了
                if (info.level >= Module_Player.instance.level)
                {
                    rFragment = 0;
                    info.level = Module_Player.instance.level;
                    return info;
                }
                return info;
            }
            exp -= inteval;
        }
        var max = levelInfo.PetAttributes[levelInfo.PetAttributes.Length - 1];
        rFragment = max.exp;
        return max;
    }

    private static uint GetTotalExp(List<PItem> rMatrials)
    {
        if(rMatrials == null || rMatrials.Count == 0)
            return 0;
        uint exp = 0;
        foreach (var m in rMatrials)
        {
            var prop = ConfigManager.Get<PropItemInfo>(m.itemTypeId);
            if (prop == null)
                continue;
            exp += prop.swallowedExpr*m.num;
        }
        return exp;
    }

    #endregion

    public struct PetNoticeKey : INoticeKey
    {
        public readonly int PetId;
        public NoticeType Type { get; }

        public PetNoticeKey(NoticeType rType, int rPetId)
        {
            Type = rType;
            PetId = rPetId;
        }

        public override bool Equals(object obj)
        {
            if (obj?.GetType() != typeof (PetNoticeKey))
                return false;
            var k = (PetNoticeKey) obj;
            return k.Type == Type && k.PetId == PetId;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() | PetId.GetHashCode();
        }
    }
}

public static class GradeInfoExpend
{
    public static string CombineGradeName(this PetUpGradeInfo.UpGradeInfo rInfo, int star)
    {
        string[] starNum = {"Ⅰ", "Ⅱ", "Ⅲ", "Ⅳ", "Ⅴ", "Ⅵ", "Ⅶ", "Ⅷ", "Ⅸ"};
        var index = Mathf.Clamp(star - 1, 0, starNum.Length - 1);
        return ConfigText.GetDefalutString(60000, rInfo.grade - 1) + starNum[index];
    }
}