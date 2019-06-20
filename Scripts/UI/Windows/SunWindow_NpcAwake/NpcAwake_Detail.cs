// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-12-11      16:28
//  *LastModify：2018-12-11      16:28
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class NpcAwake_Detail : SubWindowBase
{
    private Button                  _perfusionButton;
    private Button                  _excuteButton;
    private NpcTypeID               _npcId;
    private Image                   _icon;
    private Transform               _attributeRoot;
    private Transform               _templete;
    private Text                    _desc;
    private Text                    _npcName;
    private Image                   _slider;
    private Text                    _exp;
    private Text                    _level;
    private Transform               _lock;

    protected override void InitComponent()
    {
        base.InitComponent();
        _perfusionButton = WindowCache.GetComponent<Button>         ("npcInfo_Panel/bottom/inject_Btn");
        _excuteButton    = WindowCache.GetComponent<Button>         ("npcInfo_Panel/active_Btn");
        _icon            = WindowCache.GetComponent<Image>          ("npcInfo_Panel/avatar_back/mask/head_icon");
        _attributeRoot   = WindowCache.GetComponent<Transform>      ("npcInfo_Panel/top");
        _templete        = WindowCache.GetComponent<Transform>      ("npcInfo_Panel/top/attribute");
        _desc            = WindowCache.GetComponent<Text>           ("npcInfo_Panel/bottom/content");
        _npcName         = WindowCache.GetComponent<Text>           ("npcInfo_Panel/npcName");
        _exp             = WindowCache.GetComponent<Text>           ("npcInfo_Panel/relationshipSlider/expNumber");
        _level           = WindowCache.GetComponent<Text>           ("npcInfo_Panel/relationshipSlider/level/Text");
        _lock            = WindowCache.GetComponent<Transform>      ("npcInfo_Panel/off");
        _slider          = WindowCache.GetComponent<Image>          ("npcInfo_Panel/relationshipSlider/topSlider");

        _templete.SafeSetActive(false);
    }

    public override void MultiLanguage()
    {
        base.MultiLanguage();
        Util.SetText(WindowCache.GetComponent<Text>("npcInfo_Panel/frame/title"),               ConfigText.GetDefalutString(TextForMatType.NpcAwakeUI, 6));
        Util.SetText(WindowCache.GetComponent<Text>("npcInfo_Panel/bottom/title"),              ConfigText.GetDefalutString(TextForMatType.NpcAwakeUI, 3));
        Util.SetText(WindowCache.GetComponent<Text>("npcInfo_Panel/relationshipSlider/text"),   ConfigText.GetDefalutString(TextForMatType.NpcAwakeUI, 7));
    }

    public override bool Initialize(params object[] p)
    {
        if (!base.Initialize(p))
        {
            if (_npcId == (NpcTypeID) p[0])
                return false;
        }
        else
        {
            _excuteButton   ?.onClick.AddListener(OnExcuteClick);
        }

        _npcId = (NpcTypeID) p[0];
        var npcInfo = moduleNpc.GetTargetNpc(_npcId);
        if (_icon)
        {
            AtlasHelper.SetAvatar(_icon.gameObject, npcInfo.icon);
        }
        Util.SetText(_npcName, npcInfo.name);

        if (npcInfo.fetterLv == npcInfo.maxFetterLv)
        {
            Util.SetText(_exp, ConfigText.GetDefalutString(TextForMatType.NpcAwakeUI, 9));
            if (_slider)
                _slider.fillAmount = 1;
        }
        else
        {
            Util.SetText(_exp, $"{npcInfo.nowFetterValue}/{npcInfo.toFetterValue}");
            Util.SetText(_level, npcInfo.fetterLv.ToString());
            if (_slider)
                _slider.fillAmount = npcInfo.fetterProgress;
        }
        Util.SetText(_level, npcInfo.fetterLv.ToString());

        RefreshAttribute();
        RefreshExcuteButton();

        _lock.SafeSetActive(!npcInfo.isUnlockEngagement);
        return true;
    }

    private void RefreshExcuteButton(ScNpcActive msg)
    {
        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9854, msg.result);
            return;
        }
        RefreshExcuteButton();
    }
    private void RefreshExcuteButton()
    {
        var npcInfo = moduleNpc.GetTargetNpc(_npcId);
        _excuteButton.SetInteractable(npcInfo.fetterStage >= npcInfo.maxFetterStage && moduleAwake.activeNpc != _npcId);
        Util.SetText(_excuteButton?.transform.GetComponentInChildren<Text>(),
            ConfigText.GetDefalutString(TextForMatType.NpcAwakeUI, moduleAwake.activeNpc == _npcId ? 1 : npcInfo.fetterStage >= npcInfo.maxFetterStage ? 0 : 11));
    }

    private void RefreshAttribute()
    {
        //TODO 通过NpcId获得Npc信息
        INpcMessage npc = moduleNpc.GetTargetNpc(_npcId);
        if (null == npc)
            return;

        Util.ClearChildren(_attributeRoot);
        var info = ConfigManager.Get<NpcInfo>((int) _npcId);
        for (var i = 0; i < info.attributes.Length; i++)
        {
            var t = _attributeRoot.AddNewChild(_templete);
            t.SafeSetActive(true);
            using (var binder = new RelationShipAttribute()
            {
                name = t.GetComponent<Text>(),
                relationShip = t.GetComponent<Text>("relaiton"),
                value = t.GetComponent<Text>("left_txt")
            })
            {
                binder.Bind(i, !npc.isUnlockEngagement || npc.fetterStage < i, info.attributes[i]);
            }
        }

        using (var binder = new RelationShipAttribute() {value = _desc})
        {
            var index = info.attributes.Length;
            binder.Bind(index, npc.fetterStage < index, ConfigManager.Get<BuffInfo>(info.constellationBuff), npc.starLv);
        }
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;

        _perfusionButton ?.onClick.RemoveAllListeners();
        _excuteButton    ?.onClick.RemoveAllListeners();
        return true;
    }

    private void OnExcuteClick()
    {
        moduleAwake.Request_NpcActive(_npcId, _npcId != moduleAwake.activeNpc);
    }

    private void _ME(ModuleEvent<Module_Awake> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Awake.Response_NpcActive:
                RefreshExcuteButton(e.msg as ScNpcActive);
                break;
        }
    }

    public class RelationShipAttribute : AttributeBinder
    {
        public Text      relationShip;
        public Transform isLock;

        public void Bind(int rRelationShip, bool rIsLock, ItemAttachAttr rAttr)
        {
            Util.SetText(relationShip, ConfigText.GetDefalutString(169, rRelationShip));
            isLock.SafeSetActive(rIsLock);
            Bind(rAttr);
            SetColor(rIsLock);
        }

        public void Bind(int rRelationShip, bool rIsLock, BuffInfo rBuffInfo, int rLevel)
        {
            Util.SetText(relationShip, ConfigText.GetDefalutString(169, rRelationShip - 1));
            isLock.SafeSetActive(rIsLock);
            Util.SetText(name, rBuffInfo?.name);
            Util.SetText(value, rBuffInfo?.BuffDesc(rLevel));
            SetColor(rIsLock);
        }
        private void SetColor(bool rIsLock)
        {
            var c = ColorGroup.GetColor(ColorManagerType.NpcAttributeActive, !rIsLock);
            if (name)
                name.color = c;

            if (value)
                value.color = c;

            if (relationShip)
                relationShip.color = c;
        }
    }
}
