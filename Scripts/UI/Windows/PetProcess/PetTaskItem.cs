// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-13      16:29
//  * LastModify：2018-08-30      16:29
//  ***************************************************************************************************/

#region

using System;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class PetTaskItem : BindWidgetBehavior
{
    public Action<PetTaskItem> OnCancel;
    public Action<PetTaskItem> OnGotAward;
    public Action<PetTaskItem> OnSpeedUp;
    public Action<PetTaskItem> OnStartTask;

    public SealPetTaskInfo Task;

    private void Start()
    {
        speedUp     .onClick.AddListener(OnSpeedUpClick);
        startTrain  .onClick.AddListener(OnStartTrainClick);
        gotAward    .onClick.AddListener(OnGotAwardClick);
        cancel      .onClick.AddListener(OnCancelClick);
    }

    private void OnSpeedUpClick()
    {
        OnSpeedUp?.Invoke(this);
    }

    private void OnStartTrainClick()
    {
        OnStartTask?.Invoke(this);
    }

    private void OnGotAwardClick()
    {
        OnGotAward?.Invoke(this);
    }

    private void OnCancelClick()
    {
        OnCancel?.Invoke(this);
    }

    public void SetData(SealPetTaskInfo rTask)
    {
        BindWidget();
        Task = rTask;
        Refresh();
        if(Task != null) BaseRestrain.SetRestrainData(gameObject,Task.ID);
    }

    public void Refresh()
    {
        if (Task.Task == null)
            return;
        
        var ct = ConfigManager.Get<ConfigText>((int)TextForMatType.PetTrainText);
        Util.SetText(title, Task.Task.name);
        if(Task.LimitCount == 0)
            Util.SetText(count, ConfigText.GetDefalutString(TextForMatType.PetTrainText, 24));
        else
            Util.SetText(count, Util.Format(ct[8], $"{Task.LimitCount - Task.Count}/{Task.LimitCount}"));
        count.color = ColorGroup.GetColor(ColorManagerType.TimesUseUp, Task.TimesUseUp);
        Util.SetText(levelLimit, Util.Format(ct[7], Task.Task.minlv));
        AtlasHelper.SetPetMission(taskIcon, Task.Task.icon);
        Util.ClearChildren(itemGroup);
        for (var i = 0; i < Task.Task.previewRewardItemId.Length; i++)
        {
            var itemData = Task.Task.previewRewardItemId[i];
            var propInfo = ConfigManager.Get<PropItemInfo>(itemData);
            if (propInfo == null)
                continue;
            var t = itemGroup.AddNewChild(item);
            t.SafeSetActive(true);
            Util.SetItemInfoSimple(t, propInfo);

            var button = t.GetComponent<Button>();
            if(button)
                button.onClick.AddListener(() => Module_Global.instance.UpdateGlobalTip((ushort)itemData, true));
        }

        speedUp        .SafeSetActive(Task.State == (int)PetTaskState.Training);
        cancel         .SafeSetActive(Task.State == (int)PetTaskState.Training);
        hourglass      .SafeSetActive(Task.State == (int)PetTaskState.Training);
        gotAward       .SafeSetActive(Task.State == (int)PetTaskState.Success || Task.State == (int)PetTaskState.Defeat);
        startTrain     .SafeSetActive(Task.State == (int)PetTaskState.None && !Task.TimesUseUp);
        completeImage  .SafeSetActive(Task.State == (int)PetTaskState.Success);
        defeatImage    .SafeSetActive(Task.State == (int)PetTaskState.Defeat);
        over           .SafeSetActive(Task.TimesUseUp && Task.State == (int)PetTaskState.None);

        if (over.gameObject.activeInHierarchy)
        {
            Util.SetText(overText, ConfigText.GetDefalutString(TextForMatType.PetTrainText, Task.IsWeekTask ? 29 : 26));
        }

        if (Task.State == (int) PetTaskState.None && !Task.TimesUseUp)
        {

            var a = Task.Task.costTime;
            var t = new TimeSpan(a * TimeSpan.TicksPerMinute);
            Util.SetText(timeremaining, Util.Format(ct[6], $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}"));
        }
        else
        {
            Util.SetText(timeremaining, string.Empty);
        }

        prtListRoot.SafeSetActive(Task.GetTrainingPetCount() > 0);
        for (var i = 0; i < petList.Length; i++)
        {
            var petId = Task.GetTrainingPet(i);
            var petInfo = petId != 0 ? Module_Pet.instance.GetPet(petId) : null;
            if (petId == 0 || petInfo == null)
            {
                petList[i].SafeSetActive(false);
            }
            else
            {
                AtlasHelper.SetPetIcon(petList[i], petInfo.UpGradeInfo.icon);
                petList[i].SafeSetActive(true);
            }
        }

        taskTypeColor.color = ColorGroup.GetColor(ColorManagerType.TaskType, Task.Task.type);
    }

    private void Update()
    {
        if (Task.State == (int)PetTaskState.Training)
        {
            var a = Task.RestTime();
            var t = new TimeSpan( (long)(a * TimeSpan.TicksPerSecond));
            Util.SetText(timeremaining, $"{(int) t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}");
        }
    }

    #region 组件绑定

    [Widget("missiontitle_txt")]
    private Text title;

    [Widget("missionlevel_txt")]
    private Text levelLimit;

    [Widget("missioncount_txt")]
    private Text count;

    [Widget("timeremaining_txt")]
    private Text timeremaining;

    [Widget("item_group")]
    private Transform itemGroup;

    [Widget("item_group/item", false)]
    private Transform item;

    [Widget("Image")]
    private Button speedUp;

    [Widget("btn_01")]
    private Button startTrain;

    [Widget("btn_02")]
    private Button gotAward;

    [Widget("btn_03")]
    private Button cancel;

    [Widget("icon")]
    private Image taskIcon;

    [Widget("icon/Image")]
    private Image taskQualityIcon;

    [Widget("missioncomplete_img")]
    private Image completeImage;

    [Widget("missionover")]
    private Image over;

    [Widget("missionover/missionover_Txt")]
    private Text overText;

    [Widget("missiondefeat_img")]
    private Image defeatImage;

    [Widget("hourglass")]
    private Transform hourglass;

    [Widget("icon/Image")]
    private Image taskTypeColor;

    [ArrayWidget("prtList/0", "prtList/1", "prtList/2")]
    private Image[] petList;

    [Widget("prtList")]
    private Transform prtListRoot;

    #endregion
}
