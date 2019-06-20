// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * Extended methods.
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-06-13      18:27
//  * LastModify：2018-07-11      14:44
//  ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

public class PetProcess_TrainSelectPet : SubWindowBase
{
    private PetSelectMultiModule _petSelectModule;
    private PetProcess_Train _petProcessTrain;
    private SealPetTaskInfo _task;

    [Widget("sprite_info/close_button")]
    private Button closeButton;

    [Widget("sprite_info/go")]
    private Button go;

    [Widget("sprite_info/sprite_stage")]
    private Text gradeText;

    [Widget("sprite_info/sprite_level")]
    private Text levelText;

    [Widget("sprite_info/sprite_name")]
    private Text nameText;

    [Widget("sprite_info/scrollView")]
    private ScrollView scrollView;

    [Widget("sprite_info/success_rate")]
    private Text successRate;

    protected override void InitComponent()
    {
        base.InitComponent();
        _petSelectModule = PetSelectMultiModule.Create(scrollView, OnSelectChange, PetProcess_Train.PetSelectMax);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _petSelectModule.Destroy();
    }

    public override bool Initialize(params object[] p)
    {
        if (base.Initialize(p))
        {
            _petProcessTrain = p[0] as PetProcess_Train;
            if (_petProcessTrain == null) return false;
            _task = _petProcessTrain.currentTaskInfo;
            var rlist = modulePet.PetList;
            rlist.Sort(SortHandle);
            _petSelectModule.Initalize(rlist, _petProcessTrain.ReadlyPets);
            go.onClick.AddListener(Excute);
            closeButton.onClick.AddListener(OnCloseClick);
            Refresh();
            return true;
        }
        return false;
    }

    private int SortHandle(PetInfo a, PetInfo b)
    {
        if (_petProcessTrain.ReadlyPets != null)
        {
            var aa = Array.FindIndex(_petProcessTrain.ReadlyPets, item => item?.ID == a.ID) >= 0;
            var bb = Array.FindIndex(_petProcessTrain.ReadlyPets, item => item?.ID == b.ID) >= 0;
            if (!aa.Equals(bb))
                return -aa.CompareTo(bb);
        }
        if (a.IsTraining == b.IsTraining)
            return -a.Level.CompareTo(b.Level);
        return a.IsTraining.CompareTo(b.IsTraining);
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (base.UnInitialize(hide))
        {
            _petSelectModule.UnInitalize();
            go.onClick.RemoveListener(Excute);
            closeButton.onClick.RemoveListener(OnCloseClick);
            return true;
        }
        return false;
    }

    private void OnCloseClick()
    {
        UnInitialize();
    }

    private void Excute()
    {
        //策划需求：选择宠物时不判定条件
//        var condition = _petProcessTrain.CheckCondition(_petSelectModule.SelectList);
//        if (condition != null)
//        {
//            moduleGlobal.ShowMessage(condition.WarningText);
//            return;
//        }
        _petProcessTrain.SetPet(_petSelectModule.SelectList);
        UnInitialize();
    }

    private void Refresh()
    {
        var lv = 0;
        for (var i = 0; i < _petSelectModule.Max; i++)
        {
            if (i < _petSelectModule.SelectList.Count)
                lv += _petSelectModule.SelectList[i].Level;
        }
        var avgLv = _petSelectModule.SelectList.Count == 0 ? 0 : (float)lv / _petSelectModule.SelectList.Count;
        Util.SetText(levelText, ((int)avgLv).ToString());
        var ct = ConfigManager.Get<ConfigText>((int)TextForMatType.PetTrainText);
        Util.SetText(successRate, Util.Format(ct.text[9], _task.CalcSuccessRate(_petSelectModule.SelectList)));
    }

    private void OnSelectChange(PetInfo rinfo, Transform t)
    {
        Refresh();
    }
}
