/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Settings Window
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.2
 * Created:  2018-01-19
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Settings : Window
{
    #region Buttons

    private Button m_btnReset, m_btnSave, m_btnBack, m_btnQuit;

    #endregion

    #region Initialize

    private int m_actived = 0, m_globalLayerState = -1; // active: 0 = video 1 = audio, 2 = level condition, 3 = combo, 4 = input
    private bool m_fromSystem = false;
    private bool m_fromCombat = false;
    private bool m_lock = false;
    private RectTransform m_marker;
    private Vector2 m_markerPos;

    private RectTransform[][] m_tabs;

    private VideoSettings m_current;
    private AudioSettings m_currentAudio;
    private InputSettings m_currentInput;

    protected override void OnOpen()
    {
        m_actived = 0;
        m_globalLayerState = -1;
        m_fromSystem = false;
        m_fromCombat = false;
        m_conditionInitialized = false;
        m_comboInitialized = false;

        m_menu = GetComponent<ToggleGroup>("menu");

        #region Tabs

        // Tabs: 0 = video, 1 = audio, 2 = condition, 3 = combo, 4 = input
        var mt = m_menu.rectTransform();
        var pt = GetComponent<RectTransform>("panel");
        m_tabs = new RectTransform[mt.childCount][];
        for (var i = 0; i < m_tabs.Length; ++i)
        {
            m_tabs[i] = new RectTransform[2];
            var mm = mt.Find(i.ToString());
            m_tabs[i][0] = mm as RectTransform;
            m_tabs[i][1] = pt.Find(i.ToString()) as RectTransform;
        }
        m_marker = GetComponent<RectTransform>("menu/2/selectbox");
        m_markerPos = m_marker.anchoredPosition;

        #endregion

        #region Vedieo

        m_quality    = GetComponent<ToggleGroup>("panel/0/game_quality/gametoggle_group");
        m_fps        = GetComponent<ToggleGroup>("panel/0/fps_quality/fpstoggle_group");
        m_msaa       = GetComponent<ToggleGroup>("panel/0/aa_quality/aatoggle_group");
        m_effect     = GetComponent<ToggleGroup>("panel/0/effect_quality/effecttoggle_group");
        m_postEffect = GetComponent<Toggle>("panel/0/houchuli_quality/houchuli_switch");
        m_hdr        = GetComponent<Toggle>("panel/0/HDR_quality/HDR_switch");
        m_dof        = GetComponent<Toggle>("panel/0/DOF_quality/DOF_switch");
        m_hd         = GetComponent<Toggle>("panel/0/gaoqing_quality/gaoqing_switch");
        m_notch      = GetComponent<Toggle>("panel/0/screen_adaptation/adaptation_switch");

        #endregion

        #region Audio

        m_volume      = GetComponent<Slider>("panel/1/game_all/phonetics_slider");
        m_bgmVolume   = GetComponent<Slider>("panel/1/game_music/music_slider");
        m_voiceVolume = GetComponent<Slider>("panel/1/game_phonetics/phonetics_slider");
        m_soundVolume = GetComponent<Slider>("panel/1/game_sound/game_slider");

        #endregion

        #region Input

        m_movementType     = GetComponent<ToggleGroup>("panel/4/movement/toggles");
        m_touchSensitivity = GetComponent<Slider>("panel/4/touch/slider");

        #endregion

        m_btnReset   = GetComponent<Button>("panel/buttons/group1/btnReset");
        m_btnSave    = GetComponent<Button>("panel/buttons/group1/btnSave");
        m_btnBack    = GetComponent<Button>("panel/buttons/group0/backtogame");
        m_btnQuit    = GetComponent<Button>("panel/buttons/group0/endcombat");

        m_btnReset.onClick.AddListener(Reset);
        m_btnSave.onClick.AddListener(OnBtnSave);
        m_btnBack.onClick.AddListener(OnReturn);
        m_btnQuit.onClick.AddListener(OnBtnQuit);

        UpdateTexts();

        InitializePanels();

        PushInitalized();

        #if UNITY_EDITOR
        EventManager.AddEventListener("EditorReloadConfig", OnEditorReloadConfig);
        #endif
    }

    private void InitializePanels()
    {
        m_touchSensitivity.minValue = CombatConfig.stouchSensitivityRange.x;
        m_touchSensitivity.maxValue = CombatConfig.stouchSensitivityRange.y;

        m_volume.value            = SettingsManager.volume;
        m_bgmVolume.value         = SettingsManager.bgmVolume;
        m_voiceVolume.value       = SettingsManager.voiceVolume;
        m_soundVolume.value       = SettingsManager.soundVolume;
        m_touchSensitivity.value  = SettingsManager.touchSensitivity;

        SetCurrent(SettingsManager.current, true);
        SetCurrentAudio(SettingsManager.currentAudio, true);
        SetCurrentInput(SettingsManager.currentInput, true);

        m_menu.onAnyToggleStateOn.AddListener(OnMenuChanged);
        m_quality.onAnyToggleStateOn.AddListener(t => SetCurrent(VideoSettings.GetFromLevel(t.name, m_current)));
        m_fps.onAnyToggleStateOn.AddListener(t => { m_current.FPS = Util.Parse<int>(t.name); CheckPresetType(); });
        m_msaa.onAnyToggleStateOn.AddListener(t => { m_current.MSAA = Util.Parse<int>(t.name); CheckPresetType(); });
        m_effect.onAnyToggleStateOn.AddListener(t => { m_current.effectLevel = Util.Parse<int>(t.name); CheckPresetType(); });
        m_postEffect.onValueChanged.AddListener(e => { m_current.postEffect = e; CheckPresetType(); });
        m_hdr.onValueChanged.AddListener(e => { m_current.HDR = e; CheckPresetType(); });
        m_dof.onValueChanged.AddListener(e => { m_current.DOF = e; CheckPresetType(); });
        m_hd.onValueChanged.AddListener(e => { m_current.HD = e; CheckPresetType(); });
        m_notch.onValueChanged.AddListener(e => { m_current.notch = e; CheckPresetType(); });
        m_movementType.onAnyToggleStateOn.AddListener(t => { m_currentInput.movementType = Util.Parse<int>(t.name); CheckBtnSave(); });

        m_volume.onValueChanged.AddListener          (v => { SettingsManager.volume = v; m_currentAudio.volume = v; CheckBtnSave(); });
        m_bgmVolume.onValueChanged.AddListener       (v => { SettingsManager.bgmVolume = v; m_currentAudio.bgmVolume = v; CheckBtnSave(); });
        m_voiceVolume.onValueChanged.AddListener     (v => { SettingsManager.voiceVolume = v; m_currentAudio.voiceVolume = v; CheckBtnSave(); });
        m_soundVolume.onValueChanged.AddListener     (v => { SettingsManager.soundVolume = v; m_currentAudio.soundVolume = v; CheckBtnSave(); });
        m_touchSensitivity.onValueChanged.AddListener(v => { SettingsManager.touchSensitivity = v; m_currentInput.touchSensitivity = v; CheckBtnSave(); });
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        var args = GetWindowParam(name);
        var source = args != null && args.param1 is int ? (int)args.param1 : -1;

        m_fromSystem = source == 0;
        m_fromCombat = source == 1;

        m_globalLayerState = moduleGlobal.GetGlobalLayerShowState();

        moduleGlobal.SetGlobalLayerIndex(m_behaviour.canvas.sortingOrder);
        moduleGlobal.ShowGlobalLayerDefault(1, !m_fromCombat);

        Util.SetText(GetComponent<Text>("publicbg/setting_title"), 9100, m_fromCombat ? 0 : 1);

        SetCurrent(SettingsManager.current);
        SetCurrentAudio(SettingsManager.currentAudio);

        UpdateTabs();

        var isPvP = Level.current && Level.current.isPvP;
        if (m_fromCombat && !isPvP) Game.paused = true;
        m_btnQuit.SetInteractable(modulePVE.stageId != Module_Story.DEFAULT_STAGE_ID);
        if(moduleChase.lastStartChase != null && moduleChase.lastStartChase.taskConfigInfo != null)
        {
            int id = moduleChase.lastStartChase.taskConfigInfo.ID;
            bool valid = !GeneralConfigInfo.NeedForbideRuneAndForgeWhenSettlement(id);
            valid |= Module_Guide.skipGuide;
            if (moduleChase.lastStartChase.taskData != null) valid |= moduleChase.lastStartChase.taskData.state == (byte)EnumChaseTaskFinishState.Finish;
            m_btnQuit.SetInteractable(m_btnQuit.interactable && valid);
        }
    }

    protected override void OnReturn()
    {
        if (animating) return;

        ResetGlobalLayer();

        if (!m_currentAudio.EqualsTo(SettingsManager.currentAudio)) SettingsManager.UpdateCurrentAudio();

        SetCurrent(SettingsManager.current);
        SetCurrentAudio(SettingsManager.currentAudio);
        SetCurrentInput(SettingsManager.currentInput);
        UpdatePushPanel(true);

        Hide();

        if (m_fromSystem) ShowAsync<Window_System>();
        m_fromSystem = false;

        if (m_fromCombat && !(Level.current && Level.current.isPvP)) Game.paused = false;
    }

    protected override void OnClose()
    {
        ResetGlobalLayer();
    }

    #endregion

    #region Logic update

    private void ResetGlobalLayer()
    {
        moduleGlobal.RestoreGlobalLayerState(m_globalLayerState);
        moduleGlobal.SetGlobalLayerIndex();
    }

    private void OnMenuChanged(Toggle toggle)
    {
        m_actived = Util.Parse<int>(toggle.name);
        if (m_actived < 2 || m_actived > 3) CheckBtnSave();
    }

    private void OnBtnQuit()
    {
        ResetGlobalLayer();

        if (modulePVE.duringTranspostScene) return;

        if (Level.current && Level.current is Level_PVE)
        {
            modulePVE.SendPVEState(PVEOverState.GameOver, moduleBattle.current);
            (Level.current as Level_PVE).OnStageOver();
        }

        if (modulePVE.reopenPanelType == PVEReOpenPanel.Borderlands)
        {
            modulePVE.reopenPanelType = PVEReOpenPanel.None;
            Game.LoadLevel(Module_Bordlands.BORDERLAND_LEVEL_ID);
            return;
        }
        else if (modulePVE.reopenPanelType == PVEReOpenPanel.Labyrinth)
        {
            modulePVE.reopenPanelType = PVEReOpenPanel.None;
            Game.LoadLevel(Module_Labyrinth.LABYRINTH_LEVEL_ID);
        }
        else
        {
            Game.GoHome();
        }
    }

    private void UpdateTabs()
    {
        TaskInfo info = moduleChase.lastStartChase == null ? null : moduleChase.lastStartChase.taskConfigInfo;
        int starDetailsCount = info?.taskStarDetails?.Length ?? 0;


        var trainLevel = Level.currentLevel == GeneralConfigInfo.sTrainLevel;
        var chaseLevel = modulePVE.reopenPanelType == PVEReOpenPanel.ChasePanel;
        var defaultTab = m_fromCombat ? chaseLevel ? trainLevel ? 3 : starDetailsCount == 0 ? 3 : 2  : 3 : 0;
        for (var i = 0; i < m_tabs.Length; ++i)
        {
            var activeSelf = i < 2 || i > 3 || m_fromCombat && (i == 3 || chaseLevel);
            if(i == 2)
                activeSelf &= starDetailsCount > 0;
            RectTransform mm = m_tabs[i][0], pp = m_tabs[i][1];
            mm.gameObject.SetActive(activeSelf);
            pp.gameObject.SetActive(i == defaultTab);

            if (i == defaultTab)   // Set default tab and reset marker
            {
                m_marker.SetParent(mm);
                m_marker.anchoredPosition = m_markerPos;
                mm.GetComponent<Toggle>().isOn = true;
            }

            if (activeSelf && i == 2) UpdateConditionPanel();
            if (activeSelf && i == 3) UpdateComboPanel();
            if (activeSelf && i == 5) UpdatePushPanel();
        }
    }

    private void UpdateTexts()
    {
        var text = ConfigManager.Get<ConfigText>(9100);    // Menu
        if (text)
        {
            Util.SetText(GetComponent<Text>("publicbg/setting_title"), m_fromCombat ? text[0] : text[1]);
            Util.SetText(GetComponent<Text>("menu/0/label"),           text[2]);
            Util.SetText(GetComponent<Text>("menu/1/label"),           text[3]);
            Util.SetText(GetComponent<Text>("menu/2/label"),           text[4]);
            Util.SetText(GetComponent<Text>("menu/3/label"),           text[5]);        
            Util.SetText(GetComponent<Text>("menu/4/label"),           text[6]);
        }

        text = ConfigManager.Get<ConfigText>(9101);        // Buttons
        if (text)
        {
            Util.SetText(GetComponent<Text>("panel/buttons/group1/btnReset/label"),   text[0]);
            Util.SetText(GetComponent<Text>("panel/buttons/group1/btnSave/label"),    text[1]);
            Util.SetText(GetComponent<Text>("panel/buttons/group0/backtogame/label"), text[2]);
            Util.SetText(GetComponent<Text>("panel/buttons/group0/endcombat/label"),  Level.currentLevel == GeneralConfigInfo.sTrainLevel ? text[3] : text[4]);
        }

        text = ConfigManager.Get<ConfigText>(9102);        // Video panel
        if (text)
        {
            Util.SetText(GetComponent<Text>("panel/0/game_quality/quality_text"),             text[1]);
            Util.SetText(GetComponent<Text>("panel/0/game_quality/gametoggle_group/0/Text"),  text[2]);
            Util.SetText(GetComponent<Text>("panel/0/game_quality/gametoggle_group/1/Text"),  text[3]);
            Util.SetText(GetComponent<Text>("panel/0/game_quality/gametoggle_group/2/Text"),  text[4]);
            Util.SetText(GetComponent<Text>("panel/0/game_quality/gametoggle_group/3/Text"),  text[5]);
            Util.SetText(GetComponent<Text>("panel/0/game_quality/gametoggle_group/4/Text"),  text[6]);

            Util.SetText(GetComponent<Text>("panel/0/fps_quality/fps_text"),                  text[7]);
            Util.SetText(GetComponent<Text>("panel/0/fps_quality/thirtyText"),                text[9]);
            Util.SetText(GetComponent<Text>("panel/0/fps_quality/sixtyText"),                 text[10]);

            Util.SetText(GetComponent<Text>("panel/0/aa_quality/aa_text"),                    text[11]);
            Util.SetText(GetComponent<Text>("panel/0/aa_quality/closeText"),                  text[12]);
            Util.SetText(GetComponent<Text>("panel/0/aa_quality/fourText"),                   text[13]);
            Util.SetText(GetComponent<Text>("panel/0/aa_quality/eightText"),                  text[14]);

            Util.SetText(GetComponent<Text>("panel/0/houchuli_quality/houchuli_text"),        text[15]);
            Util.SetText(GetComponent<Text>("panel/0/HDR_quality/HDR_text"),                  text[16]);
            Util.SetText(GetComponent<Text>("panel/0/HDR_quality/DOF_text"),                  text[22]);
            Util.SetText(GetComponent<Text>("panel/0/gaoqing_quality/gaoqing_text"),          text[17]);
            Util.SetText(GetComponent<Text>("panel/0/screen_adaptation/adaptation_text"),     text[23]);

            Util.SetText(GetComponent<Text>("panel/0/effect_quality/0_Text"),                 text[18]);
            Util.SetText(GetComponent<Text>("panel/0/effect_quality/1_Text"),                 text[19]);
            Util.SetText(GetComponent<Text>("panel/0/effect_quality/2_Text"),                 text[20]);
            Util.SetText(GetComponent<Text>("panel/0/effect_quality/3_Text"),                 text[21]);
        }

        text = ConfigManager.Get<ConfigText>(9103);        // Audio panel
        if (text)
        {
            Util.SetText(GetComponent<Text>("panel/1/sound_text"),          text[0]);
            Util.SetText(GetComponent<Text>("panel/1/game_all/Text"),       text[1]);
            Util.SetText(GetComponent<Text>("panel/1/game_sound/Text"),     text[2]);
            Util.SetText(GetComponent<Text>("panel/1/game_music/Text"),     text[3]);
            Util.SetText(GetComponent<Text>("panel/1/game_phonetics/Text"), text[4]);
        }

        text = ConfigManager.Get<ConfigText>(9104);        // Input panel
        if (text)
        {
            Util.SetText(GetComponent<Text>("panel/4/movement/text"),            text[0]);
            Util.SetText(GetComponent<Text>("panel/4/movement/toggles/0/label"), text[1]);
            Util.SetText(GetComponent<Text>("panel/4/movement/toggles/1/label"), text[2]);
            Util.SetText(GetComponent<Text>("panel/4/movement/toggles/2/label"), text[3]);
            Util.SetText(GetComponent<Text>("panel/4/touch/text"),               text[4]);
        }

        text = ConfigManager.Get<ConfigText>(9206);        // Combo panel
        if (text)
        {
            Util.SetText(GetComponent<Text>("panel/3/des/content"),  text[2]);
        }

        text = ConfigManager.Get<ConfigText>(9212);        //push panel
        if (text)
        {
            Util.SetText(GetComponent<Text>("panel/5/systemNotification/title"),                                  text[0]);
            Util.SetText(GetComponent<Text>("panel/5/systemNotification/fatigueNotice/fatigue_text"),             text[1]);
            Util.SetText(GetComponent<Text>("panel/5/systemNotification/skillpointNotice/skillpoint_text"),       text[2]);
            Util.SetText(GetComponent<Text>("panel/5/systemNotification/globalNotice/globalNotice_text"),         text[3]);
            Util.SetText(GetComponent<Text>("panel/5/activityNotification/title"),                                text[4]);
            Util.SetText(GetComponent<Text>("panel/5/activityNotification/unionBossActNotice/unionBossAct_text"), text[5]);
            Util.SetText(GetComponent<Text>("panel/5/activityNotification/pvpNotice/pvpNotice_text"),             text[6]);
            Util.SetText(GetComponent<Text>("panel/5/activityNotification/labyrinthNotice/labyrinthNotice_text"), text[7]);
        }
    }

    #region Video & Audio & Input

    private ToggleGroup m_menu, m_quality, m_fps, m_msaa, m_effect, m_movementType;
    private Toggle m_postEffect, m_hdr, m_dof, m_hd, m_notch;

    private Slider m_volume, m_bgmVolume, m_voiceVolume, m_soundVolume, m_touchSensitivity;

    private void OnBtnSave()
    {
        if (m_actived == 0) SettingsManager.current = m_current;
        if (m_actived == 1) SettingsManager.currentAudio = m_currentAudio;
        if (m_actived == 4) SettingsManager.currentInput = m_currentInput;
        if (m_actived == 5) SendChangePushState();

        CheckBtnSave();
        PlayerPrefs.Save();
        moduleGlobal.ShowMessage(9105);
    }

    private void Reset()
    {
        if (m_actived == 0) SetCurrent(SettingsManager.recommend, true);
        if (m_actived == 1) SetCurrentAudio(SettingsManager.recommendAudio);
        if (m_actived == 4) SetCurrentInput(SettingsManager.recommendInput, true);
        if (m_actived == 5) UpdatePushPanel(true);
    }

    private void SetCurrent(VideoSettings settings, bool force = false)
    {
        if (m_lock || !force && settings.EqualsTo(m_current)) return;

        m_current = settings;
        CheckPresetType();

        m_fps.SetOne(m_current.FPS < 50 ? "30" : "60");
        m_msaa.SetOne(m_current.MSAA < 2 ? "0" : m_current.MSAA < 4 ? "2" : "4");
        m_effect.SetOne(Mathf.Clamp(m_current.effectLevel, 0, 3).ToString());

        m_postEffect.isOn = m_current.postEffect;
        m_hdr.isOn        = m_current.HDR;
        m_dof.isOn        = m_current.DOF;
        m_hd.isOn         = m_current.HD;
        m_notch.isOn      = m_current.notch;
    }

    private void SetCurrentAudio(AudioSettings settings, bool force = false)
    {
        if (!force && settings.EqualsTo(m_currentAudio)) return;

        m_currentAudio = settings;

        m_volume.value      = m_currentAudio.volume;
        m_bgmVolume.value   = m_currentAudio.bgmVolume;
        m_voiceVolume.value = m_currentAudio.voiceVolume;
        m_soundVolume.value = m_currentAudio.soundVolume;

        CheckBtnSave();
    }

    private void SetCurrentInput(InputSettings settings, bool force = false)
    {
        if (!force && settings.EqualsTo(m_currentInput)) return;

        m_currentInput = settings;

        m_movementType.SetOne(Mathf.Clamp(m_currentInput.movementType, 0, 2).ToString());
        m_touchSensitivity.value = m_currentInput.touchSensitivity;

        CheckBtnSave();
    }

    private void CheckPresetType()
    {
        m_lock = true;
        m_current.Check();
        m_quality.SetOne(((int)m_current.type).ToString());
        m_lock = false;

        CheckBtnSave();
    }

    private void CheckBtnSave()
    {
        if (m_actived == 2 || m_actived == 3) return;
        var result = m_actived == 0 ? !m_current.EqualsTo(SettingsManager.current) : m_actived == 1 ? !m_currentAudio.EqualsTo(SettingsManager.currentAudio) : !m_currentInput.EqualsTo(SettingsManager.currentInput);
        m_btnSave.interactable = m_actived == 5 ? IsNoEqualsPush() : result;
        m_btnReset.interactable = m_actived == 5 ? IsNoEqualsPush() : true;
    }

    #endregion

    #region Level condition

    #region custom class

    public sealed class ChaseConditionItem : CustomSecondPanel
    {
        public ChaseConditionItem(Transform trans, TaskInfo.TaskStarCondition condition) : base(trans)
        {
            SetPreviewCondition(condition);
        }

        private static EnumPVEDataType[] CHECK_LESS_VALUE = new EnumPVEDataType[] { EnumPVEDataType.BeHitedTimes, EnumPVEDataType.GameTime };

        private GameObject m_completeObj, m_uncompleteObj;
        private Text m_descriptionText;
        private Text m_processText;
        private EnumPVEDataType type;
        private int tarValue;

        public bool complete { get; private set; }

        public override void InitComponent()
        {
            base.InitComponent();

            m_uncompleteObj = transform.Find("frame_uncomplete").gameObject;
            m_processText = transform.GetComponent<Text>("frame_uncomplete/progress");
            m_descriptionText = transform.GetComponent<Text>("content_txt");
            m_completeObj = transform.Find("frame_complete").gameObject;
        }

        private void SetPreviewCondition(TaskInfo.TaskStarCondition condition)
        {
            if (condition == null) return;

            type = condition.type;
            tarValue = condition.value;
            Util.SetText(m_descriptionText, TaskInfo.GetStarDescription(type, tarValue));
        }

        public void Refresh()
        {
            float value = modulePVE.GetPveGameData(type);
            if (type == EnumPVEDataType.GameTime) value = Mathf.Floor(value);
            complete = IsComplete(value);
            m_completeObj.SetActive(complete);
            m_uncompleteObj.SetActive(!complete);
            Util.SetText(m_processText, Util.Format("{0}/{1}", value, tarValue));
        }

        private bool IsComplete(float value)
        {
            if (CHECK_LESS_VALUE.Contains(type)) return value < tarValue;
            else return value >= tarValue;
        }
    }
    #endregion

    private string[] m_childNames = new string[] { "condition_01", "condition_02", "condition_03" };
    private List<ChaseConditionItem> m_conditionItems = new List<ChaseConditionItem>();
    private Transform m_conditionTrans;

    private bool m_conditionInitialized = false;
    private void UpdateConditionPanel()
    {
        if (modulePVE.reopenPanelType != PVEReOpenPanel.ChasePanel) return;
        if(!m_conditionInitialized)
        {
            m_conditionInitialized = true;
            m_conditionTrans = transform.Find("panel/2");
            m_conditionItems.Clear();
            for (int i = 0; i < m_childNames.Length; i++)
            {
                Transform t = m_conditionTrans.Find(m_childNames[i]);

                ChaseConditionItem c = new ChaseConditionItem(t, GetChaseCondition(i));
                m_conditionItems.Add(c);
            }
        }

        RefreshConditionPanel();
    }

    private TaskInfo.TaskStarCondition GetChaseCondition(int index)
    {
        if(moduleChase.lastStartChase != null)
            return moduleChase.lastStartChase?.taskConfigInfo?.GetStarDetail(index + 1)?.GetDefalutStarCondition();
        return moduleAwakeMatch.CurrentTask?.taskConfigInfo?.GetStarDetail(index + 1)?.GetDefalutStarCondition();
    }

    public void RefreshConditionPanel()
    {
        foreach (var item in m_conditionItems)
        {
            item.Refresh();
        }
    }

    #endregion

    #region Combo panel

    private bool m_comboInitialized = false;
    private void UpdateComboPanel()
    {
        if (m_comboInitialized) return;
        m_comboInitialized = true;

        var w = moduleBattle.current ? ConfigManager.Get<PropItemInfo>(moduleBattle.current.weaponItemID) : null;
        Util.SetText(GetComponent<Text>("panel/3/title"), 9207, 0, w ? w.itemName : "null");
        var clist = w ? ConfigManager.Get<ComboInputInfo>(w.ID) : null;
        var slist = clist ? clist.groups : new ComboInputInfo.SpellGroup[] { };

        var template = GetComponent<RectTransform>("panel/3/combolist/template").gameObject;
        var content  = GetComponent<RectTransform>("panel/3/combolist/Viewport/Content");
        for (int i = 0, c = Mathf.Max(slist.Length, content.childCount); i < c; ++i)
        {
            if (i >= slist.Length)
            {
                content.GetChild(i).gameObject.SetActive(false);
                continue;
            }
            var s = i < content.childCount ? content.GetChild(i) : Object.Instantiate(template).transform;
            s.name = i.ToString();
            s.gameObject.SetActive(true);
            Util.AddChild(content, s);

            SetSingleComboSpell(s, slist[i]);
        }
        template.SetActive(false);
    }

    private Image[] m_comboIcons = null;

    private void SetSingleComboSpell(Transform g, ComboInputInfo.SpellGroup gg)
    {
        Util.SetText(g.GetComponent<Text>("group"), 9207, 1, gg.group);

        var ss = g.Find("spells");
        var spells = gg.spells;
        var hh = 35;
        for (int i = 0, c = Mathf.Max(spells.Length, ss.childCount); i < c; ++i)
        {
            if (i >= spells.Length)
            {
                ss.GetChild(i).gameObject.SetActive(false);
                continue;
            }
            var s = i < ss.childCount ? ss.GetChild(i) : Object.Instantiate(ss.GetChild(0)).transform;
            s.name = i.ToString();
            s.gameObject.SetActive(true);
            Util.AddChild(ss, s);
            s.rectTransform().anchoredPosition = new Vector2(0, -i * 50);

            var spell = spells[i];
            Util.SetText(s.GetComponent<Text>("spellName"), 9207, 2, "<color=" + spell.textColor.ToXml() + ">" + spell.spellName + "</color>");
            var r = s.GetComponent<Text>("rage");
            r.gameObject.SetActive(spell.rage > 0);
            if (spell.rage > 0) Util.SetText(r, 9207, 3, spell.rage);
            s.GetComponent<Image>().color = spell.backColor;
            var icons = s.Find("icons");
            Util.ClearChildren(icons, true);

            if (m_comboIcons == null)
            {
                m_comboIcons = new Image[10];
                for (var j = 0; j < m_comboIcons.Length; ++j) m_comboIcons[j] = GetComponent<Image>("panel/3/des/i" + j);
            }

            var sss = spell.inputs;
            for (int j = 0; j < sss.Length; ++j)
            {
                var iid = sss[j];
                var icon = iid < 1 || iid >= m_comboIcons.Length ? null : m_comboIcons[iid];
                if (!icon) continue;
                var io = Object.Instantiate(icon.gameObject);
                io.name = iid.ToString();
                io.gameObject.SetActive(true);
                Util.AddChild(icons, io.transform, false);
                io.transform.localEulerAngles = icon.transform.localEulerAngles;
            }
            hh += 50;
        }
        g.GetComponentDefault<LayoutElement>().minHeight = hh;
    }

    #endregion

    #region push

    private Dictionary<SwitchType, Toggle> toggleState = new Dictionary<SwitchType, Toggle>();
    private Toggle fatigueToggle, skillToggle, unionToggle, pvpToggle, labyrinthToggle, systemPao;

    private Dictionary<SwitchType, uint> tempState = new Dictionary<SwitchType, uint>();
    private void PushInitalized()
    {
        fatigueToggle   = GetComponent<Toggle>("panel/5/systemNotification/fatigueNotice/fatigue_switch");
        skillToggle     = GetComponent<Toggle>("panel/5/systemNotification/skillpointNotice/skillpoint_switch");
        unionToggle     = GetComponent<Toggle>("panel/5/activityNotification/unionBossActNotice/unionBossAct_switch");
        pvpToggle       = GetComponent<Toggle>("panel/5/activityNotification/pvpNotice/pvpNotice_switch");
        labyrinthToggle = GetComponent<Toggle>("panel/5/activityNotification/labyrinthNotice/labyrinthNotice_switch");
        systemPao       = GetComponent<Toggle>("panel/5/systemNotification/globalNotice/globalNotice_switch");
        toggleState.Clear();
        toggleState.Add(SwitchType.Fatigue, fatigueToggle);
        toggleState.Add(SwitchType.SkillPoint, skillToggle);
        toggleState.Add(SwitchType.UnionBoss, unionToggle);
        toggleState.Add(SwitchType.Labyrinth, pvpToggle);
        toggleState.Add(SwitchType.RoyalPvp, labyrinthToggle);
        toggleState.Add(SwitchType.SystemPao, systemPao);

        fatigueToggle.onValueChanged.RemoveAllListeners();
        fatigueToggle.onValueChanged.AddListener((r) => OnValueChanged(r, SwitchType.Fatigue));
        skillToggle.onValueChanged.RemoveAllListeners();
        skillToggle.onValueChanged.AddListener((r) => OnValueChanged(r, SwitchType.SkillPoint));
        unionToggle.onValueChanged.RemoveAllListeners();
        unionToggle.onValueChanged.AddListener((r) => OnValueChanged(r, SwitchType.UnionBoss));
        pvpToggle.onValueChanged.RemoveAllListeners();
        pvpToggle.onValueChanged.AddListener((r) => OnValueChanged(r, SwitchType.Labyrinth));
        labyrinthToggle.onValueChanged.RemoveAllListeners();
        labyrinthToggle.onValueChanged.AddListener((r) => OnValueChanged(r, SwitchType.RoyalPvp));
        systemPao.onValueChanged.RemoveAllListeners();
        systemPao.onValueChanged.AddListener((r) => OnValueChanged(r, SwitchType.SystemPao));
    }

    private void OnValueChanged(bool isOn, SwitchType type)
    {
        if (!tempState.ContainsKey(type)) return;
        tempState[type] = isOn ? (uint)1 : 0;
        CheckBtnSave();
    }

    void _ME(ModuleEvent<Module_Guide> e)
    {
        if (e.moduleEvent == Module_Guide.EventUnlockFunctionStart)
        {
            var ids = e.param1 as int[];
            if (ids == null && ids.Length < 1) return;

            for (int i = 0; i < ids.Length; i++)
            {
                var type = (HomeIcons)ids[i];
                var _type = SwitchType.Fatigue;

                if (type == HomeIcons.Labyrinth) _type = SwitchType.Labyrinth;
                else if (type == HomeIcons.Match) _type = SwitchType.RoyalPvp;
                else if (type == HomeIcons.Skill) _type = SwitchType.SkillPoint;
                else if (type == HomeIcons.Guild) _type = SwitchType.UnionBoss;

                if (_type != SwitchType.Fatigue && moduleSet.pushState.ContainsKey(_type) && moduleSet.pushState[_type] == 1)
                    DispatchEvent(SDKEvent.GLOBAL, SDKEvent.PopSdk(SDKEvent.TAG, (uint)1, (byte)_type));
            }
        }
    }

    private void UpdatePushPanel(bool reset = false)
    {
        if (!reset)
        {
            tempState.Clear();
            foreach (var item in moduleSet.pushState)
            {
                if (tempState.ContainsKey(item.Key)) continue;
                tempState.Add(item.Key, item.Value);
            }
        }
        RefreshPushPanel(reset);
    }

    private void RefreshPushPanel(bool reset)
    {
        var dic = reset ? moduleSet.pushState : tempState;
        var _dic = new Dictionary<SwitchType, uint>();
        foreach (var item in dic)
        {
            if (_dic.ContainsKey(item.Key)) continue;
            _dic.Add(item.Key, item.Value);
        }

        foreach (var item in _dic)
        {
            var toggle = toggleState.ContainsKey(item.Key) ? toggleState[item.Key] : null;
            if (toggle == null) continue;
            toggle.isOn = item.Value == 0 ? false : true;
        }
    }

    public void SendChangePushState()
    {
        foreach (var item in moduleSet.pushState)
        {
            if (!tempState.ContainsKey(item.Key)) continue;
            var state = tempState[item.Key];
            if (state != item.Value)
                moduleSet.ChangePushState(item.Key, state);
        }
    }

    private bool IsNoEqualsPush()
    {
        foreach (var item in tempState)
        {
            if (!moduleSet.pushState.ContainsKey(item.Key)) continue;
            var state = moduleSet.pushState[item.Key];
            if (state != item.Value) return true;
        }
        return false;
    }

    void _ME(ModuleEvent<Module_Set> e)
    {
        if (!actived) return;
        if (e.moduleEvent == Module_Set.EventSavePushState) CheckBtnSave();
    }

    #endregion

    #endregion

    #region Editor helper

#if UNITY_EDITOR
    private void OnEditorReloadConfig(Event_ e)
    {
        var config = (string)e.param1;
        if (config != "config_combatconfigs") return;

        m_touchSensitivity.minValue = CombatConfig.stouchSensitivityRange.x;
        m_touchSensitivity.maxValue = CombatConfig.stouchSensitivityRange.y;
    }
    #endif

    #endregion
}
