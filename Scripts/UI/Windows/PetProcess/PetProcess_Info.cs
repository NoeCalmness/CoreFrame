// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 宠物信息界面.
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-06-28      11:45
//  * LastModify：2018-07-11      14:18
//  ***************************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PetProcess_Info : SubWindowBase
{
    const string PET_OBJECT_NAME = "uiPet";

    [Widget("info_panel/top/mood_addnumber")]
    private Transform addMoodTween;

    [Widget("info_panel/desc_bottom/attr_group")]
    private Transform AttrGroupRoot;

    [Widget("info_panel/desc_bottom/attr_group/attr_item", false)]
    private Transform AttrTemplete;

    private readonly List<int>   delayEventIDList = new List<int>();

    [Widget("info_panel/evolve_btn")]
    private Button evolveButton;

    [Widget("info_panel/feed_btn/progress_frame")]
    private Slider expSlider;

    [Widget("info_panel/feed_btn")]
    private Button feedButton;

    [Widget("info_panel/fight_btn"), Tooltip("出战按钮")]
    private Button fightButton;

    [Widget("info_panel/desc_top/stage_txt")]
    private Text gradeText;

    [Widget("info_panel/top")]
    private HorizontalLayoutGroup layoutGroup;

    [Widget("info_panel/feed_btn/Text")]
    private Text levelText;

    [ArrayWidget("info_panel/top/mood/0", "info_panel/top/mood/1", "info_panel/top/mood/2", "info_panel/top/mood/3")]
    private List<Transform> moodList;

    [Widget("info_panel/top/mood")]
    private Transform moodRoot;

    [Widget("info_panel/top/sprite_name")]
    private Text nameText;

    [Widget("info_panel/sprite_list")]
    private ScrollView petScrollView;

    public PetSelectModule petSelectModule;

    [Widget("info_panel/rest_btn"), Tooltip("休息按钮")]
    private Button restButton;

    [Widget("info_panel/sprite_list/viewport/content/selecet_img")]
    private Image selecet_img;

    public PetInfo     SelectPet;

    [Widget("info_panel/desc_middle/skill_btn")]
    private Button skillButton;

    [Widget("info_panel/desc_middle/desc_txt")]
    private Text skillDescText;

    [Widget("info_panel/desc_middle/icon_img")]
    private Image skillIcon;

    [Widget("info_panel/desc_top/star_group")]
    private Transform starGroup;

    private GameObject[] starObjects;

    [Widget("info_panel/sprite/sprite_btn")]
    private Button talkButton;

    [Widget("info_panel/sprite/talk/Text")]
    private Text talkText;

    [Widget("info_panel/sprite/talk")]
    private TweenBase talkTween;
    [Widget("info_panel/feed_btn/mark")]
    private Transform feedMark;

    [Widget("info_panel/desc_top/mark")]
    private Transform evolveMark;

    private Transform hintComposeNode;

    private Creature m_pet;
    private string m_modeName;
    private Effect m_lockEffect;
    private Coroutine m_loadPetCoroutine;
    private Coroutine m_loadEffectCoroutine;

    protected override void InitComponent()
    {
        base.InitComponent();

        hintComposeNode = WindowCache.GetComponent<Transform>("info_panel/sprite/unlockNotice");

        starObjects = new GameObject[starGroup.childCount];
        for (var i = 0; i < starGroup.childCount; i++)
        {
            starObjects[i] = starGroup.GetChild(i).gameObject;
        }

        petSelectModule = PetSelectModuleBase.Create<PetSelectModule>(petScrollView, OnSelectChange);
    }

    private void OnSelectChange(PetInfo rInfo, Transform t)
    {
        if (t)
        {
            var tween = selecet_img.GetComponentDefault<TweenPosition>();
            tween.duration = 0.15f;
            tween.from = selecet_img.transform.localPosition;
            tween.to = t.parent.localPosition;
            tween.Play();
        }
        if (null == rInfo || rInfo.Equals(SelectPet))
            return;
        SelectPet = rInfo;
        //更新界面
        Refresh();
        RefreshHintCompose(rInfo);
    }

    private void RefreshHintCompose(PetInfo rInfo)
    {
        hintComposeNode.SafeSetActive(rInfo.CanCompond() && !modulePet.Contains(rInfo.ID));
    }

    public override bool OnReturn()
    {
        UnInitialize(false);
        return true;
    }

    public override bool Initialize(params object[] p)
    {
        moduleHome.HideOthers(PET_OBJECT_NAME);
        if (base.Initialize())
        {
            SelectPet = null;
            PetInfo defaultPet = null;
            if (p.Length > 0)
            {
                defaultPet = modulePet.GetPet((int) p[0]);
                if (defaultPet != null)
                    OnSelectChange(defaultPet, null);
            }
            petSelectModule.Initalize(Module_Pet.GetAllPet(), defaultPet);
            talkButton    ?.onClick.AddListener(OnTease);
            fightButton   ?.onClick.AddListener(OnPetFight);
            restButton    ?.onClick.AddListener(RestButton);
            skillButton   ?.onClick.AddListener(() =>
            {
                if (SelectPet == null)
                {
                    Logger.LogError("检测到bug。SelectPet为空！");
                    return;
                }
                moduleGlobal.UpdateSkillTip(SelectPet.GetSkill(), SelectPet.AdditiveLevel, SelectPet.Mood);
            });
        }
        return true;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (base.UnInitialize(hide))
        {
            for (var i = 0; i < delayEventIDList.Count; i++)
            {
                var delayID = delayEventIDList[i];
                DelayEvents.Remove(delayID);
            }
            petSelectModule.UnInitalize();
            talkButton?.onClick.RemoveListener(OnTease);
            fightButton?.onClick.RemoveListener(OnPetFight);
            restButton?.onClick.RemoveListener(RestButton);
            skillButton?.onClick.RemoveAllListeners();

            m_pet?.Destroy();
            m_lockEffect?.Destroy();
            m_modeName = null;
            return true;
        }
        return false;
    }


    private void OnPetFight()
    {
        if (SelectPet == null || SelectPet.ID == modulePet.FightingPetID)
            return;

        if (SelectPet.IsTraining)
        {
            moduleGlobal.ShowMessage(Util.Format(ConfigText.GetDefalutString(297), SelectPet.CPetInfo.itemName));
            return;
        }

        var msg = PacketObject.Create<CsPetStatus>();
        msg.petId = SelectPet.ItemID;
        msg.status = 1;
        session.Send(msg);
    }

    private void RestButton()
    {
        if (SelectPet == null || SelectPet.ID != modulePet.FightingPetID)
            return;
        var msg = PacketObject.Create<CsPetStatus>();
        msg.petId = SelectPet.ItemID;
        msg.status = 0;
        session.Send(msg);
    }

    /// <summary>
    /// 响应宠物点击事件
    /// </summary>
    private void OnTease()
    {
        if (!modulePet.Contains(SelectPet.ID))
        {
            //TODO 显示宠物碎片合成界面
            var compound = ConfigManager.Find<Compound>(c => Array.Exists(c.items, pair => pair.itemId == SelectPet.ID));
            if(null == compound)
                return;
            moduleGlobal.UpdateComposeTip((ushort) compound.sourceTypeId);
            return;
        }
        //向服务器发送挑逗消息
        var msg = PacketObject.Create<CsPetTease>();
        msg.petId = SelectPet.ItemID;
        session.Send(msg);
    }

    private void Talk()
    {
        var id = 0;
        if (SelectPet.Talk(out id))
        {
            Util.SetText(talkText, id);

            talkTween.SafeSetActive(true);
            talkTween?.PlayForward();
        }
        addMoodTween.SafeSetActive(true);
    }

    private void RefreshMood()
    {
        if (null == SelectPet)
            return;
        moodRoot.SafeSetActive(modulePet.Contains(SelectPet.ID));
        layoutGroup.CalculateLayoutInputHorizontal();
        var rMood = SelectPet.Mood;
        for (var i = 0; i < moodList.Count; i++)
        {
            moodList[i].gameObject.SetActive(i == (int)rMood);
        }
    }

    /// <summary>
    /// 宠物信息更新
    /// </summary>
    public void Refresh()
    {
        if (SelectPet == null)
            return;
        Util.SetText(nameText, SelectPet.CPetInfo.itemNameId);
        RefreshSkill();
        RefreshDescColor();
        RefreshStar();
        RefreshModule();
        RefreshAttribute();
        RefreshButtonState();
        RefreshMood();
        RefreshEvolve();
        RefreshNotice();

        //要等scrollView先刷新把new标签显示出来
        DelayEvents.Add(() =>
        {
            modulePet.NewPetList.Remove(SelectPet.ID);
        }, 0.2f);
        petSelectModule.SetItem(SelectPet);
    }

    public void RefreshNotice()
    {
        petSelectModule?.RefreshNotice();
        feedMark.SafeSetActive(SelectPet.CanFeed() && modulePet.Contains(SelectPet.ID));
        evolveMark.SafeSetActive(SelectPet.CanEvolve() && modulePet.Contains(SelectPet.ID));
    }

    private void RefreshEvolve()
    {
        if (SelectPet == null)
            return;
        Util.SetText(levelText, SelectPet.Level.ToString());
        expSlider.value = SelectPet.GetExpSlider();
    }

    private void RefreshSkill()
    {
        if (SelectPet == null)
            return;
        var skill = SelectPet.GetSkill();
        if (skill == null)
            return;
        AtlasHelper.SetShared(skillIcon, skill.skillIcon);
        Util.SetText(skillDescText, SelectPet.SkillName);
    }

    private void RefreshDescColor()
    {
        if (SelectPet == null) return;
        skillDescText.color = ColorGroup.GetColor(ColorManagerType.IsFightingAttribute, SelectPet.ID == modulePet.FightingPetID);
    }

    /// <summary>
    /// 刷新出战。休息按钮状态
    /// </summary>
    private void RefreshButtonState()
    {
        if (null == SelectPet)
            return;
        var isGot = modulePet.Contains(SelectPet.ID);
        fightButton.SafeSetActive(SelectPet.ID != modulePet.FightingPetID && isGot);
        restButton.SafeSetActive(SelectPet.ID == modulePet.FightingPetID);
        feedButton.interactable = isGot;
        evolveButton.interactable = isGot;
    }

    /// <summary>
    /// 刷新属性显示列表
    /// </summary>
    private void RefreshAttribute()
    {
        if (SelectPet == null)
            return;
        AttrTemplete.SafeSetActive(false);
        Util.ClearChildren(AttrGroupRoot);
        var att = SelectPet.Attribute;
        if (null == att) return;
        var rList = att;
        foreach (var aShow in rList)
        {
            var t = AttrGroupRoot.transform.AddNewChild(AttrTemplete);
            t.SafeSetActive(true);
            var text = t.GetComponent<Text>();
            text.color = ColorGroup.GetColor(ColorManagerType.IsFightingAttribute, SelectPet.ID == modulePet.FightingPetID);
            Util.SetText(text, aShow.ShowString());
        }
    }

    /// <summary>
    /// 更新宠物模型
    /// </summary>
    public void RefreshModule()
    {
        if (SelectPet == null || m_modeName == SelectPet.UpGradeInfo.mode) return;

        if (m_loadPetCoroutine != null) Level.current.StopCoroutine(m_loadPetCoroutine);
        if (m_loadEffectCoroutine != null) Level.current.StopCoroutine(m_loadEffectCoroutine);

        m_pet?.Destroy();
        m_lockEffect?.Destroy();

        var playUnlockEffect = modulePet.NewPetList.Contains(SelectPet.ID);
        m_modeName = SelectPet.UpGradeInfo.mode;

        m_loadPetCoroutine = Level.PrepareAssets(Module_Battle.BuildPetSimplePreloadAssets(SelectPet, null, 0), b =>
        {
            var show = ConfigManager.Get<ShowCreatureInfo>(SelectPet.ID);
            if (show == null)
            {
                Logger.LogError("没有配置config_showCreatureInfo表。宠物ID = {0}, 没有出生位置信息。宠物模型创建失败", SelectPet.ID);
                return;
            }

            var showData = show.GetDataByIndex(1);
            var data = showData.data.GetValue<ShowCreatureInfo.SizeAndPos>(0);

            m_pet = moduleHome.CreatePet(SelectPet.UpGradeInfo, data.pos, data.rotation, Level.current.startPos, false, PET_OBJECT_NAME);

            m_pet.eulerAngles = data.rotation;
            m_pet.localPosition = data.pos;
            m_pet.transform.localScale *= data.size;

            if (!modulePet.Contains(SelectPet.ID))
            {
                m_loadEffectCoroutine = Level.PrepareAsset<GameObject>(GeneralConfigInfo.defaultConfig.lockEffect.effect, (t) =>
                {
                    m_lockEffect = m_pet.behaviour.effects.PlayEffect(GeneralConfigInfo.defaultConfig.lockEffect);
                    m_loadEffectCoroutine = null;
                });
            }
            else if (playUnlockEffect)
            {
                m_loadEffectCoroutine = Level.PrepareAsset<GameObject>(GeneralConfigInfo.defaultConfig.unlockEffect.effect, (t) =>
                {
                    m_pet.behaviour.effects.PlayEffect(GeneralConfigInfo.defaultConfig.unlockEffect);
                    m_loadEffectCoroutine = null;
                });
            }
            m_loadPetCoroutine = null;
        });
    }

    /// <summary>
    /// 更新宠物星级
    /// </summary>
    private void RefreshStar()
    {
        if (null == SelectPet) return;
        Util.SetText(gradeText, SelectPet.UpGradeInfo?.CombineGradeName(SelectPet.Star));

        for (var i = 0; i < starObjects.Length; i++)
        {
            starObjects[i]?.SetActive(i < SelectPet.Star);
        }
    }

    private void PlayPetFightAction()
    {
        if (modulePet.FightingPetID <= 0) return;
        var grade = modulePet.GetPet(modulePet.FightingPetID).UpGradeInfo;

        m_pet?.stateMachine?.TranslateTo(grade.fightAction);
    }

    private void _ME(ModuleEvent<Module_Pet> e)
    {
        if (!Root.activeInHierarchy)
            return;
        switch (e.moduleEvent)
        {
            case Module_Pet.PetStatusChange:
                RefreshButtonState();
                RefreshAttribute();
                RefreshDescColor();
                PlayPetFightAction();
                break;
            case Module_Pet.PetGradeChange:
                var p = e.param1 as PetInfo;
                if ((p?.ID ?? -1) == SelectPet?.ID)
                {
                    RefreshStar();
                    RefreshModule();
                    RefreshAttribute();
                    RefreshSkill();
                }
                break;
            case Module_Pet.ResponseStatus:
                ResponseChageStatus(e.msg as ScPetStatus);
                break;
            case Module_Pet.ResponseTease:
                ResponseTease(e.msg as ScPetTease);
                break;
            case Module_Pet.MoodChange:
                RefreshMood();
                break;
            case Module_Pet.PetListChange:
                petSelectModule.ResetDataSource(Module_Pet.GetAllPet());
                RefreshButtonState();
                break;
            case Module_Pet.EventGetNewPet:
                var pet = e.param1 as PetInfo;
                if (null == pet) break;
                petSelectModule.SetItem(pet);
                if (null != SelectPet && SelectPet.ID == pet.ID)
                    SelectPet = pet;
                Refresh();
                RefreshHintCompose(SelectPet);
                PlayUnlockEffect();
                break;
            default:
                Refresh();
                break;
        }
        petSelectModule?._ME(e);
    }

    private void PlayUnlockEffect()
    {
        if (m_pet == null) return;
        if (m_loadEffectCoroutine != null)
            Level.current.StopCoroutine(m_loadEffectCoroutine);

        m_lockEffect?.Destroy();

        m_loadEffectCoroutine = Level.PrepareAsset<GameObject>(GeneralConfigInfo.defaultConfig.unlockEffect.effect, (t) =>
        {
            m_pet.behaviour.effects.PlayEffect(GeneralConfigInfo.defaultConfig.unlockEffect);
            m_loadEffectCoroutine = null;
        });
    }

    private void ResponseTease(ScPetTease msg)
    {
        if (msg == null || msg.response == 0)
        {
            Talk();
            return;
        }
        //挑逗次數用完不提示。策划觉得太烦了
        if (msg.response == 1)
            return;
        //失败显示tip
        moduleGlobal.ShowMessage(9704, msg.response);
    }

    private void ResponseChageStatus(ScPetStatus msg)
    {
        //成功不用管
        if (msg == null || msg.response == 0)
            return;
        //失败显示tip
        moduleGlobal.ShowMessage(9705, msg.response);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        petSelectModule?.Destroy();
    }
}


public abstract class PetSelectModuleBase : LogicObject
{
    public delegate void OnSelectHandle(PetInfo rInfo, Transform node);

    public Action<Transform, PetInfo> onSetDefault;

    protected DataSource<PetInfo>       dataSource;
    protected OnSelectHandle            onSelectCallBack;
    protected bool                      setDefault;         //是否已经设置过默认宠物
    protected ScrollView                scrollView;
    protected PetItemBehavior           currentBehaviour;
    protected PetInfo                   defaultPet;

    public void Initalize(List<PetInfo> rList, PetInfo rDefault = null)
    {
        defaultPet = rDefault;
        setDefault = rDefault != null;
        if (dataSource == null)
            dataSource = new DataSource<PetInfo>(rList, scrollView, OnSetData, OnClick);
        else
            dataSource.SetItems(rList);

        if (defaultPet != null) SetDefault(null, defaultPet);

        if (null != rDefault)
        {
            for (var i = 0; i < dataSource.count; i++)
            {
                if (dataSource.GetItem(i)?.ID == rDefault.ID)
                {
                    scrollView.currentIndex = i;
                    break;
                }
            }
        }
    }


    public void ResetDataSource(List<PetInfo> pets)
    {
        setDefault = false;
        if (dataSource == null)
            dataSource = new DataSource<PetInfo>(pets, scrollView, OnSetData, OnClick);
        else
            dataSource.SetItems(pets);
    }

    public void SetItem(PetInfo rInfo)
    {
        if (null == dataSource)
            return;
        for (var i = 0; i < dataSource.count; i++)
        {
            if (dataSource.GetItem(i)?.ID == rInfo.ID)
            {
                dataSource.SetItem(rInfo, i);
                dataSource.UpdateItem(i);
                break;
            }
        }
    }

    public void RefreshNotice()
    {
        currentBehaviour?.RefreshNotice();
    }

    protected virtual void OnClick(RectTransform node, PetInfo data)
    {
        onSelectCallBack?.Invoke(data, node);
    }

    protected virtual void OnSetData(RectTransform node, PetInfo data)
    {
        var itemBehavior = node.GetComponentDefault<PetItemBehavior>();
        itemBehavior.SetData(data);

        if (defaultPet != null)
        {
            if (defaultPet == data)
            {
                SetDefault(node, data);
                defaultPet = null;
            }
        }
        else if (!setDefault)
        {
            setDefault = true;
            SetDefault(itemBehavior.transform, itemBehavior.DataCache);
        }
    }

    public virtual void UnInitalize()
    {
    }

    protected abstract void SetDefault(Transform t, PetInfo rPetInfo);

    public static T Create<T>(ScrollView rScrollView, OnSelectHandle rOnSelectCallBack) where T : PetSelectModuleBase
    {
        var module = _Create<T>();
        module.scrollView = rScrollView;
        module.onSelectCallBack = rOnSelectCallBack;
        return module;
    }

    public virtual void _ME(ModuleEvent<Module_Pet> e)
    {
        dataSource.UpdateItems();
    }
}

public class PetSelectModule : PetSelectModuleBase
{
    public PetInfo selectInfo;

    protected override void SetDefault(Transform t, PetInfo rPetInfo)
    {
        setDefault = true;
        selectInfo = rPetInfo;
        onSetDefault?.Invoke(t, rPetInfo);
        onSelectCallBack?.Invoke(rPetInfo, t);
        currentBehaviour = t?.GetComponent<PetItemBehavior>();
        rPetInfo.RefreshCompondReadState();
    }

    protected override void OnSetData(RectTransform node, PetInfo data)
    {
        base.OnSetData(node, data);
        //重新更新数据源的数据。需要把当前数据更新
        if (selectInfo?.ID == data?.ID)
            selectInfo = data;
    }

    protected override void OnClick(RectTransform node, PetInfo data)
    {
        selectInfo = data;
        onSelectCallBack?.Invoke(data, node);
        currentBehaviour = node.GetComponent<PetItemBehavior>();
        
        data.RefreshCompondReadState();
    }

    public void Watch(int petId)
    {
        var pet = modulePet.GetPet(petId);
        if (null == pet)
            return;
        for (var i = 0; i < dataSource.count; i++)
        {
            if (dataSource.GetItem(i)?.ID == pet.ID)
            {
                scrollView.currentIndex = i;
                selectInfo = pet;
                setDefault = false;
                dataSource.UpdateItem(i);
                break;
            }
        }
    }
}

public class PetSelectMultiModule : PetSelectModuleBase
{
    public readonly List<PetInfo> SelectList = new List<PetInfo>();
    public Action<PetInfo, bool> OnSelectChangeEvent;
    public int Max { get; set; }

    protected override void SetDefault(Transform t, PetInfo rPetInfo)
    {
        
    }

    public void Initalize(List<PetInfo> rList, PetInfo[] selectPet)
    {
        SelectList.Clear();
        if (selectPet != null && selectPet.Length > 0)
        {
            for (var i = 0; i < selectPet.Length; i++)
            {
                var petInfo = selectPet[i];
                if(petInfo != null) SelectList.Add(petInfo);
            }
        }
        base.Initalize(rList);
    }

    protected override void OnSetData(RectTransform node, PetInfo data)
    {
        base.OnSetData(node, data);
        node.GetComponent<PetItemBehavior>().SetSelectState(SelectList.Contains(data));
    }


    protected override void OnClick(RectTransform node, PetInfo data)
    {
        //只有闲置的宠物才能派出历练
        if (!data.IsIdle)
        {
            moduleGlobal.ShowMessage(235, 23);
            return;
        }
        if (SelectList.Contains(data))
        {
            SelectList.Remove(data);
            OnSelectChangeEvent?.Invoke(data, false);
            node.GetComponent<PetItemBehavior>().SetSelectState(false);
        }
        else if(SelectList.Count < Max)
        {
            SelectList.Add(data);
            OnSelectChangeEvent?.Invoke(data, true);
            node.GetComponent<PetItemBehavior>().SetSelectState(true);
        }
        base.OnClick(node, data);
    }

    public short[] GetSelectPet()
    {
        var arr = new short[SelectList.Count];
        for (var i = 0; i < arr.Length; i++)
            arr[i] = (sbyte) SelectList[i].ID;
        return arr;
    }

    public override void UnInitalize()
    {
        SelectList.Clear();
        base.UnInitalize();
    }

    public static PetSelectMultiModule Create(ScrollView rScrollView, OnSelectHandle rOnSelectCallBack, int rMax)
    {
        var module = Create<PetSelectMultiModule>(rScrollView, rOnSelectCallBack);
        module.Max = rMax;
        return module;
    }
}