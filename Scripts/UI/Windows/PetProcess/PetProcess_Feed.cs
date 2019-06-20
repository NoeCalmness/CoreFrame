// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 宠物喂养界面
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-06-27      17:16
//  * LastModify：2018-07-23      10:30
//  ***************************************************************************************************/

#region

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#endregion

public class PetProcess_Feed : SubWindowBase<Window_Sprite>
{
    /// <summary>
    /// 属性显示列表节点缓存
    /// </summary>
    private readonly Dictionary<CreatureFields, Transform> cache = new Dictionary<CreatureFields, Transform>();

    private readonly HashSet<MatrialBehavior> matrialList = new HashSet<MatrialBehavior>();

    private ValueInputAssist    ValueInput;

    [Widget("feed_panel/add_bg")]
    private Button addButton;

    private MatrialBehavior     curMatrial;

    [Widget("feed_panel/progressbar/progressbar_txt")]
    private Text expProcess;

    [Widget("feed_panel/feed_btn")]
    private Button feedButton;

    [Widget("feed_panel/textbg_img")]
    private InputField input;

    [Widget("feed_panel/level/level_txt_02")]
    private Text levelLeft;

    [Widget("feed_panel/level/level_txt_03")]
    private Text levelRight;

    private DataSource<PropItemInfo> matrialDataSource;


    [Widget("feed_panel/material_list")]
    private ScrollView matrialScrollView;

    [Widget("feed_panel/minus_bg")]
    private Button minusButton;

    [Widget("feed_panel/number_txt")]
    private Text number;

    [Widget("feed_panel/attr_list")]
    private Transform propertyRoot;

    [Widget("feed_panel/attr_list/attr_content")]
    private Transform propertyTemp;

    [Widget("feed_panel/progressbar")]
    private Slider slider;
    [Widget("feed_panel/progressbar/current")]
    private Slider currentSlider;
    [Widget("feed_panel/level/level_txt_03/max")]
    private Transform maxLabel;
    [Widget("feed_panel/notice_txt")]
    private Text noticeLabel;
    [Widget("feed_panel/FlyEffects")]
    private Transform flyEffect;
    [Widget("feed_panel/sprite_feed_effnode")]
    private Transform feedEffect;
    [Widget("feed_panel/sprite_levelup_effnode")]
    private Transform levelUpEffect;

    private bool levelUp;

    private PreviewSlider ps;

    protected override void InitComponent()
    {
        base.InitComponent();

        ValueInput = ValueInputAssist.Create(minusButton, addButton, input);
        ValueInput.OnValueChange += OnInputValueChange;
        ValueInput.SetLimit(0, 999);

        ps = new PreviewSlider(new SliderAnim(currentSlider, OnLevelUp), (SliderAnim2)slider);
    }

    private void OnLevelUp(int level)
    {
        Util.SetText(levelLeft, level.ToString());
    }

    private void OnInputValueChange(int rValue)
    {
        if(curMatrial)
            curMatrial.SetAddNumber(rValue);
        RefreshPreview();
        feedButton.interactable = rValue > 0;
    }

    private void RefreshPreview()
    {
        uint fragment = 0;
        var levelInfo = GetPreviewLevel(out fragment);
        RefreshAttribute();
        ps.SetPreviewUniformAnim((float) fragment/levelInfo.exp + levelInfo.level);
        Util.SetText(expProcess, $"{fragment}/{levelInfo.exp}");
        maxLabel.SafeSetActive(levelInfo.level == modulePlayer.level);
    }

    private PetAttributeInfo.PetAttribute GetPreviewLevel(out uint rFragment)
    {
        var exp = GetTotalExp();
        
        var petInfo = parentWindow.SelectPetInfo;
        var levelInfo = ConfigManager.Get<PetAttributeInfo>(petInfo.ID);
        if (levelInfo == null || 
            levelInfo.PetAttributes == null || 
            levelInfo.PetAttributes.Length == 0)
        {
            rFragment = 0;
            return new PetAttributeInfo.PetAttribute { exp = 0, level = 1};
        }
        for (var i = petInfo.Level - 1; i < levelInfo.PetAttributes.Length && exp >= 0; i++)
        {
            var info = levelInfo.PetAttributes[i];
            var inteval = info.exp - (petInfo.Level == info.level ? petInfo.Exp : 0);
            if (exp < inteval)
            {
                rFragment = info.exp - (inteval - exp);
                //升至角色最大等级就不能涨经验了
                if (info.level >= modulePlayer.level)
                {
                    rFragment = 0;
                    info.level = modulePlayer.level;
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

    public override bool Initialize(params object[] p)
    {
        if (base.Initialize(p))
        {
            ps.SetPreview(0);
            propertyTemp.SafeSetActive(false);
            feedButton?.onClick.AddListener( OnFeedChick);
            Refresh();
            ValueInput.OnOverFlow += () =>
            {
                if (ValueInput.LimitMax != (curMatrial?.ItemCache?.num ?? 0))
                    moduleGlobal.ShowMessage(9707, 0);
            };
            return true;
        }
        return false;
    }
    public override bool OnReturn()
    {
        if (!base.OnReturn())
            return false;
            moduleGlobal.targetMatrial?.Clear();
        return true;
    }

    private void RefreshFeedState()
    {
        if (parentWindow.SelectPetInfo == null) return;
        feedButton.SafeSetActive(modulePet.Contains(parentWindow.SelectPetInfo.ID));
        feedButton.interactable = ValueInput.Value > 0;
    }


    private void OnFeedChick()
    {
        if (curMatrial?.ItemCache == null)
            return;
        var msg = PacketObject.Create<CsPetFeed>();
        msg.petId = parentWindow.SelectPetInfo.ItemID;
        var list = new List<PMaterial>();
        if (curMatrial.AddNumber > 0)
        {
            var matrial = PacketObject.Create<PMaterial>();
            matrial.uid = curMatrial.ItemCache.itemId;
            matrial.num = Convert.ToUInt16(curMatrial.AddNumber);
            list.Add(matrial);
        }
        msg.matrials = list.ToArray();
        session.Send(msg);
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;
        ValueInput.OnOverFlow = null;
        feedButton?.onClick.RemoveListener(OnFeedChick);
        foreach (var entry in effectEntryList)
            entry.Destory();
        effectEntryList.Clear();
        ps.Interrupt();

        parentWindow.SelectPetInfo?.RefreshFeedReadState();
        return true;
    }

    private void Refresh()
    {
        if(parentWindow.SelectPetInfo == null) return;
        cache.Clear();

        ps.SetCurrent(parentWindow.SelectPetInfo.GetExpProcess());

        Util.SetText(levelLeft, parentWindow.SelectPetInfo.Level.ToString());
        Util.SetText(noticeLabel, Util.Format(ConfigText.GetDefalutString((int)TextForMatType.SpriteUIText, 12), modulePlayer.level));

        RefreshMatrial();
        RefreshPreview();
        RefreshFeedState();
        RefreshAttribute();
        if(curMatrial != null)
            ProcessMatrial(curMatrial.rectTransform(), curMatrial.PropItem, false);
    }

    private void RefreshAttribute()
    {
        Util.ClearChildren(propertyRoot);
        cache.Clear();

        //先计算出升级前和升级后的属性 合并升级前后的属性条目
        var hashAttribute = new HashSet<CreatureFields>();
        var leftShowList = parentWindow.SelectPetInfo.Attribute;
        foreach (var afs in leftShowList)
        {
            hashAttribute.Add((CreatureFields)afs.id);
        }
        uint fragment = 0;
        var levelInfo = GetPreviewLevel(out fragment);
        var rightShowList = parentWindow.SelectPetInfo.GetAttribute(levelInfo.level,
                parentWindow.SelectPetInfo.Grade);
        foreach (var afs in rightShowList)
        {
            hashAttribute.Add((CreatureFields)afs.id);
        }
        foreach (var afs in hashAttribute)
        {
            if(cache.ContainsKey(afs))
                continue;
            var t = propertyRoot.AddNewChild(propertyTemp);
            t.SafeSetActive(true);
            cache.Add(afs, t);
        }
        RefreshLeftAttribute(leftShowList);
        RefreshRightAttribute(rightShowList, leftShowList);
        Util.SetText(levelRight, levelInfo.level.ToString());
    }

    private void RefreshLeftAttribute(List<ItemAttachAttr> showList)
    {
        foreach (var kv in cache)
        {
            var afs = showList.Find(item => item.id == (ushort) kv.Key);
            BindPropertyLeft(kv.Value, afs);
        }
    }

    private void RefreshRightAttribute(List<ItemAttachAttr> showList, List<ItemAttachAttr> left)
    {
        foreach (var kv in cache)
        {
            var afs = showList.Find(item => item.id == (ushort)kv.Key);
            var leftAfs = left.Find(item => item.id == (ushort) kv.Key);
            if (leftAfs == null)
                BindPropertyRight(kv.Value, afs, afs);
            else
            {
                var change = new ItemAttachAttr
                {
                    id = afs.id,
                    type = afs.type,
                    value =
                        AttributeShowHelper.ValueForShow(afs.id, afs.value, afs.type == 2) -
                        AttributeShowHelper.ValueForShow(leftAfs.id, leftAfs.value, leftAfs.type == 2)
                };
                BindPropertyRight(kv.Value, afs, change);
            }
        }
    }

    private void RefreshMatrial()
    {
        if (!matrialScrollView) return;

        var items = ConfigManager.FindAll<PropItemInfo>(item => item.itemType == PropType.PetFood && item.subType == 1);
        if (items == null || items.Count == 0)
            return;
        matrialDataSource = new DataSource<PropItemInfo>(items, matrialScrollView, OnSetMatrial, OnMatrialClick);
    }

    private void OnSetMatrial(RectTransform node, PropItemInfo data)
    {
        Util.SetItemInfoSimple(node, data);
        var matrial = node.gameObject.GetComponentDefault<MatrialBehavior>();

        var pItem = moduleEquip.GetProp(data.ID);
        matrial.Bind(pItem, data);

        matrialList.Add(matrial);

        if(curMatrial == null)
            ProcessMatrial(node, data, false);
    }

    private void OnMatrialClick(RectTransform node, PropItemInfo data)
    {
        ProcessMatrial(node, data, true);
    }

    private void ProcessMatrial(RectTransform node, PropItemInfo data, bool isClick)
    {
        if (curMatrial)
            curMatrial.SetSelect(false);
        curMatrial = node.GetComponent<MatrialBehavior>();
        if (curMatrial.ItemCache != null)
        {
            curMatrial.SetSelect(true);
            ValueInput.SetLimit(1, (int)Mathf.Min(curMatrial.ItemCache.num, CalcLimitMax()));
        }
        else
        {
            ValueInput.SetLimit(0, 0);
            ValueInput.SetValue(0);

            if (isClick)
                moduleGlobal.SetTargetMatrial(data.ID, 1, Tuple.Create(Window_Sprite.SUB_TYPE_FEED, parentWindow.SelectPetInfo.ID));
        }
        RefreshPreview();
    }

    /// <summary>
    /// 根据当前材料计算升级到限制等级最多能消耗多少材料
    /// </summary>
    /// <returns></returns>
    private int CalcLimitMax()
    {
        var pet = parentWindow.SelectPetInfo;
        var levelInfo = ConfigManager.Get<PetAttributeInfo>(pet.ID);
        uint exp = 0;
        for (var i = pet.Level - 1; i < Math.Min(levelInfo.PetAttributes.Length, modulePlayer.level - 1); i++)
        {
            if (i == pet.Level - 1)
                exp += levelInfo.PetAttributes[i].exp - pet.Exp;
            else
                exp += levelInfo.PetAttributes[i].exp;
        }
        //向上取整
        return Mathf.CeilToInt((float)exp / curMatrial.PropItem.swallowedExpr);
    }

    private void BindPropertyLeft(Transform t, ItemAttachAttr show)
    {
        Util.SetText(t.GetComponent<Text>("attr_txt_02"), show != null ? show.ValueString() :"0");
        Util.SetText(t.GetComponent<Text>("attr_txt_01"), show != null ? show.TypeString() : "");
    }

    private void BindPropertyRight(Transform t, ItemAttachAttr show, ItemAttachAttr change)
    {
        Util.SetText(t.GetComponent<Text>("attr_txt_03"), show != null ? show.ValueString() : "0");
        Util.SetText(t.GetComponent<Text>("attr_txt_03/attr_txt_04"), $"【+{change.ValueString()}】");
        Util.SetText(t.GetComponent<Text>("attr_txt_01"), show != null ? show.TypeString() : "");
    }

    public void _ME(ModuleEvent<Module_Pet> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Pet.LevelChange:
                levelUp = true;
                break;
            case Module_Pet.ResponseFeed:
                ResponseFeed(e.msg as ScPetFeed);
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Equip> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Equip.EventBagInfo:
            case Module_Equip.EventUpdateBagProp:
                matrialDataSource.UpdateItems();
                break;
        }
    }

    private void ResponseFeed(ScPetFeed msg)
    {
        if (msg == null)
            return;

        if (msg.response == 0)
        {
            ps.PlayUniformAnimToPreview(parentWindow.SelectPetInfo.GetExpProcess(), 1, Refresh);

            if (curMatrial.ItemCache != null && curMatrial.ItemCache.num > 0)
            {
                var maxLimit = CalcLimitMax();
                if (maxLimit == 0)
                    moduleGlobal.ShowMessage(9707);

                ValueInput.SetLimit(1, (int)Mathf.Min(curMatrial.ItemCache.num, maxLimit));
            }
            else
                ValueInput.SetLimit(0, 0);
            ValueInput.SetValue(0);
            RefreshFeedState();

            var entry = new EffectEntry() { levelUp = levelUp };
            levelUp = false;
            entry.coroutine = global::Root.instance.StartCoroutine(PlayEffect(entry));
            effectEntryList.Add(entry);
        }
        else
        {
            moduleGlobal.ShowMessage(9702, msg.response);
            Refresh();
        }
    }

    private IEnumerator PlayEffect(EffectEntry entry)
    {
        var t = flyEffect.GetChild(matrialDataSource.GetItemIndex(curMatrial.PropItem));
        entry.go = PerfectInstantiate(t?.gameObject);
        entry.go.SafeSetActive(true);
        yield return new WaitForSeconds(0.9f);
        
        entry.go1 = PerfectInstantiate(feedEffect?.gameObject);
        entry.go1.SafeSetActive(true);

        if (entry.levelUp)
        {
            entry.levelUpEffect = PerfectInstantiate(levelUpEffect?.gameObject);
            entry.levelUpEffect.SafeSetActive(true);
        }

        yield return new WaitForSeconds(3);
        entry.Destory();
        effectEntryList.Remove(entry);
    }

    private GameObject PerfectInstantiate(GameObject temp)
    {
        if (!temp)
            return null;
        var go = Object.Instantiate(temp);
        go.transform.SetParent(temp.transform.parent);
        go.transform.localPosition = temp.transform.localPosition;
        go.transform.rotation = temp.transform.rotation;
        go.transform.localScale = temp.transform.localScale;
        return go;
    }

    private uint GetTotalExp()
    {
        if (curMatrial == null) return 0;
        return curMatrial.PropItem.swallowedExpr * ValueInput.Value;
    }

    private class EffectEntry
    {
        public bool levelUp;
        public GameObject go;
        public GameObject go1;
        public GameObject levelUpEffect;
        public Coroutine coroutine;

        public void Destory()
        {
            if (go) Object.Destroy(go);
            if (go1) Object.Destroy(go1);
            if (levelUpEffect) Object.Destroy(levelUpEffect);
            if (null != coroutine) global::Root.instance.StopCoroutine(coroutine);
        }
    }

    private List<EffectEntry> effectEntryList = new List<EffectEntry>();
}