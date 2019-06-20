// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * Extended methods.
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-06-06      19:28
//  * LastModify：2018-07-11      14:11
//  ***************************************************************************************************/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Window_Sprite : Window
{
    public const int SUB_MAIN = 0;
    public const int SUB_TYPE_FEED = 1;
    public const int SUB_TYPE_EVOLVE = 2;

    private PetProcess_Evolve               _petProcessEvolve;
    private PetProcess_Feed                 _petProcessFeed;
    private PetProcess_Info                 _petProcessInfo;

    [Widget("evolve_panel")]
    private Transform evolve_panel;

    [Widget("info_panel/evolve_btn")]
    private Button evolveButton;

    [Widget("feed_panel")]
    private Transform feed_panel;

    [Widget("info_panel/feed_btn")]
    private Button feedButton;

    [Widget("info_panel")]
    private Transform info_panel;

    [Widget("info_panel")]
    private CanvasGroup infoCanvasGroup;


    public PetSelectModule SelectModule { get { return _petProcessInfo.petSelectModule; } }

    public PetInfo         SelectPetInfo { get { return SelectModule.selectInfo; } }

    protected override void OnOpen()
    {
        if (info_panel)
        {
            _petProcessInfo   = SubWindowBase               .CreateSubWindow<PetProcess_Info>(this, info_panel.gameObject);
            _petProcessInfo?.Set(false);
        }
        if(feed_panel)        
            _petProcessFeed   = SubWindowBase<Window_Sprite>.CreateSubWindow<PetProcess_Feed>(this, feed_panel.gameObject);
        if(evolve_panel)
            _petProcessEvolve = SubWindowBase<Window_Sprite>.CreateSubWindow<PetProcess_Evolve>(this, evolve_panel.gameObject);

        feedButton.onClick.AddListener(OnFeedClick);
        evolveButton.onClick.AddListener(OnEvolveClick);

        MultLanguage();
        RequestData();
    }

    private void RequestData()
    {
        var msg = PacketObject.Create<CsPetInfos>();
        session.Send(msg);
    }

    private void MultLanguage()
    {
        var t = ConfigManager.Get<ConfigText>((int)TextForMatType.SpriteUIText);
        Util.SetText(GetComponent<Text>("info_panel/feed_btn/feed_btn_txt"), t[0]);
        Util.SetText(GetComponent<Text>("info_panel/evolve_btn/evolve_txt"), t[1]);
        Util.SetText(GetComponent<Text>("info_panel/rest_btn/rest_btn_txt"), t[3]);
        Util.SetText(GetComponent<Text>("info_panel/fight_btn/fight_btn_txt"), t[2]);
        Util.SetText(GetComponent<Text>("info_panel/desc_bottom/title_txt"), t[7]);
        Util.SetText(GetComponent<Text>("info_panel/desc_middle/title_txt"), t[8]);
        Util.SetText(GetComponent<Text>("evolve_panel/outsideframe_top/title_txt"), t[4]);
        Util.SetText(GetComponent<Text>("evolve_panel/outsideframe_top/evolve_btn/rest_btn_txt"), t[1]);
        Util.SetText(GetComponent<Text>("feed_panel/feed_btn/Text"), t[5]);
        Util.SetText(GetComponent<Text>("feed_panel/level/level_txt_01"), t[9]);
        Util.SetText(GetComponent<Text>("info_panel/sprite/unlockNotice/content"), t[13]);

        t = ConfigManager.Get<ConfigText>((int)TextForMatType.PetMood);
        Util.SetText(GetComponent<Text>("mood_tip/content/title"), t[0]);
        Util.SetText(GetComponent<Text>("mood_tip/content/mood_01/Text"), t[1]);
        Util.SetText(GetComponent<Text>("mood_tip/content/mood_02/Text"), t[2]);
        Util.SetText(GetComponent<Text>("mood_tip/content/mood_03/Text"), t[3]);
        Util.SetText(GetComponent<Text>("mood_tip/content/mood_04/Text"), t[4]);
        Util.SetText(GetComponent<Text>("mood_tip/content/des_txt"), t[5]);
    }

    protected override void OnReturn()
    {
        infoCanvasGroup.interactable = true;
        infoCanvasGroup.blocksRaycasts = true;

        if (_petProcessEvolve.OnReturn())
        {
            if (!_petProcessEvolve.isInit)
            {
                _petProcessInfo.SetEnable(true);
                _petProcessInfo.Refresh();
            }
            return;
        }
        if (_petProcessFeed.OnReturn())
        {
            _petProcessInfo.SetEnable(true);
            _petProcessInfo.Refresh();
            return;
        }
        _petProcessInfo.OnReturn();
        base.OnReturn();
    }

    public void OnEvolveClick()
    {
        _petProcessInfo.SetEnable(false);
        _petProcessEvolve?.Initialize();
    }

    public void OnFeedClick()
    {
        _petProcessInfo.SetEnable(false);
        _petProcessFeed?.Initialize();
    }

    protected override void OnClose()
    {
        base.OnClose();
        _petProcessFeed?.Destroy();
        _petProcessEvolve?.Destroy();
        _petProcessInfo?.Destroy();
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();//top工具条的显示

        if (moduleGlobal.targetMatrial.isProcess &&
            moduleGlobal.targetMatrial.isFinish &&
            moduleGlobal.targetMatrial.windowName == name)
        {
            var t = (Tuple<int, int>) moduleGlobal.targetMatrial.data;
            m_petId = t.Item2;
            m_subWindowID = t.Item1;
            moduleGlobal.targetMatrial.Clear();
        }
        {
            _petProcessInfo.Initialize(m_petId);

            if (m_subWindowID == SUB_TYPE_EVOLVE)
                OnEvolveClick();
            else if (m_subWindowID == SUB_TYPE_FEED)
                OnFeedClick();
        }
    }

    protected override void OnHide(bool forward)
    {
        base.OnHide(forward);

        _petProcessInfo.UnInitialize();

        var key = new NoticeDefaultKey(NoticeType.Pet);
        moduleNotice.SetNoticeState(key, modulePet.NeedNoticeSimple);
        moduleNotice.SetNoticeReadState(key);
    }


    public void SetSelect(int petId)
    {
        SelectModule.Watch(petId);
    }

    #region RestoreData

    private int m_subWindowID;

    private int m_petId;
    protected override void GrabRestoreData(WindowHolder holder)
    {
        m_petId = _petProcessInfo.SelectPet?.ID ?? 0;

        if (_petProcessFeed.isInit)
            m_subWindowID = SUB_TYPE_FEED;
        else if (_petProcessEvolve.isInit)
            m_subWindowID = SUB_TYPE_EVOLVE;
        else
            m_subWindowID = SUB_MAIN;

        holder.SetData(m_subWindowID, m_petId);
    }

    protected override void ExecuteRestoreData(WindowHolder holder)
    {
        m_petId = holder.GetData<int>(1);
        m_subWindowID = holder.GetData<int>(0);
    }
    #endregion

    public void RefreshModel()
    {
        _petProcessInfo.RefreshModule();
    }
}


