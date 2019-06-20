/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-06-26
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Profession : Window, IBackToStory
{
    #region interface property
    public Action onBackToStory { get; set; }
    #endregion

    HorizontalLayoutGroup horizotal;
    Toggle[] toggles;
    float _totalWidth;
    float _offset;
    float _spcialWidth;
    float _normalWidth;
    private List<ProfessionInfo> allProfession = new List<ProfessionInfo>();

    protected override void OnOpen()
    {
        horizotal = GetComponent<HorizontalLayoutGroup>("item_list");

        Toggle vocation_01 = GetComponent<Toggle>("item_list/item_01");
        Toggle vocation_02 = GetComponent<Toggle>("item_list/item_02");
        Toggle vocation_03 = GetComponent<Toggle>("item_list/item_03");
        Toggle vocation_04 = GetComponent<Toggle>("item_list/item_04");
        Toggle vocation_05 = GetComponent<Toggle>("item_list/item_05");
        Toggle vocation_06 = GetComponent<Toggle>("item_list/item_06");
        toggles = new Toggle[] { vocation_01, vocation_02, vocation_03, vocation_04, vocation_05, vocation_06 };
        for (int i = 0; i < toggles.Length; i++)
        {
            toggles[i].onValueChanged.RemoveAllListeners();
            toggles[i].onValueChanged.AddListener(OnToggleValueChanged);
        }

        CanvasScaler canvas = UIManager.instance._canvasScaler;
        _totalWidth = Screen.width * canvas.referenceResolution.y / Screen.height;
        _offset = _totalWidth / canvas.referenceResolution.x;
        var width_1280 = GetComponent<Image>("item_list/item_01/profession_img").rectTransform().sizeDelta.x;
        _spcialWidth = width_1280 * _offset;
        _normalWidth = (_totalWidth - _spcialWidth - (toggles.Length - 1) * Mathf.Abs(horizotal.spacing)) / (toggles.Length - 1);

        allProfession.Clear();
        allProfession = ConfigManager.GetAll<ProfessionInfo>();
    }

    private void OnToggleValueChanged(bool arg0)
    {
        if (!arg0) return;

        for (int i = 0; i < toggles.Length; i++)
        {
            var _item = toggles[i].GetComponentDefault<VocationItem>();
            _item.RefreshVocationItem(allProfession.Count > i ? allProfession[i].ID : 0, toggles[i].isOn, _spcialWidth, _normalWidth, OnConfirmCallback);
        }
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        if (!toggles[0].isOn) toggles[0].isOn = true;
        else OnToggleValueChanged(true);
    }

    private void OnConfirmCallback()
    {
        Hide(true);
        onBackToStory?.Invoke();
    }
}