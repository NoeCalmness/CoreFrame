// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-08-30      18:20
//  *LastModify：2018-11-07      9:52
//  ***************************************************************************************************/

#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class PetItemBehavior : BindWidgetBehavior
{
    public delegate void OnClickHandle(PetItemBehavior rInfo);

    public PetInfo DataCache;

    [Widget("fight")]
    public Image Fighting;

    [Widget("icon")]
    private Transform Icon;

    [Widget("new")]
    public Transform IsNew;

    [Widget("spriteavatar_level_img")]
    private Image levelBg;

    [Widget("spriteavatar_level_img/spriteavatar_level_txt")]
    public Text LevelText;

    [Widget("lockmask_img")]
    public Image LockMask;

    [Widget("mark")]
    public Transform mark;

    [ArrayWidget("spriteavatar_mood/mood_04",
                 "spriteavatar_mood/mood_03",
                 "spriteavatar_mood/mood_02",
                 "spriteavatar_mood/mood_01")]
    private List<Transform> moodList;

    [Widget("spriteavatar_mood")]
    private Transform moodRoot;

    [Widget("spritename_txt")]
    public Text Name;

    public OnClickHandle onClickHandle;

    [Widget("checkmask_img")]
    public Image SelectStateMark;

    [Widget("trainingmask_img")]
    public Image TrainingMask;

    protected void Start()
    {
         var button = GetComponent<Button>();
        if(button)
            button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        onClickHandle?.Invoke(this);
    }


    public void SetData(PetInfo rInfo)
    {
        BindWidget();
        DataCache = rInfo;
        Refresh();

        RefreshNotice();

        IsNew.SafeSetActive(Module_Pet.instance.NewPetList.Contains(rInfo.ID));
    }

    public void RefreshNotice()
    {
        if (Module_Pet.instance.Contains(DataCache.ID))
            mark.SafeSetActive(DataCache.CanFeed() || DataCache.CanEvolve());
        else
            mark.SafeSetActive(DataCache.CanNoticeCompond());
    }

    private void RefreshMood(EnumPetMood rMood)
    {
        moodRoot?.SafeSetActive(Module_Pet.instance.Contains(DataCache.ID));

        for (var i = 0; i < moodList.Count; i++)
        {
            moodList[i].gameObject.SetActive(i == (int)rMood);   
        }
    }

    public void SetSelectState(bool bSelect)
    {
        SelectStateMark.SafeSetActive(bSelect);
    }

    public void Refresh()
    {
        Util.SetText(Name, DataCache.CPetInfo.itemNameId);
        Util.SetText(LevelText, DataCache.Level.ToString());

        var isGot = Module_Pet.instance.Contains(DataCache.ID);
        Name.color = ColorGroup.GetColor(ColorManagerType.UnlockPetNameColor, isGot);
        LockMask       ?.SafeSetActive(!isGot);
        SelectStateMark?.SafeSetActive(false);
        TrainingMask   ?.SafeSetActive(DataCache.IsTraining);
        Fighting       ?.SafeSetActive(DataCache.ID == Module_Pet.instance.FightingPetID);
        levelBg        ?.SafeSetActive(isGot);
        if(levelBg != null) levelBg.color = ColorGroup.GetColor(ColorManagerType.PetQuality, DataCache.CPetInfo.quality);
        Icon?.SetGray(!isGot);
        Util.SetPetSimpleInfo(transform, DataCache);

        RefreshMood(DataCache.Mood);
    }
}
