// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 宠物进阶界面.
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-06-20      13:37
//  * LastModify：2018-07-11      14:23
//  ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

public class PetProcess_Evolve : SubWindowBase<Window_Sprite>
{
    private PetProcess_EvolveSuccess _petProcessEvolveSuccess;

    [Widget("evolve_panel/evolve/outsideframe_top/evolve_btn")]
    private Button evolveButton;

    [Widget("evolve_panel/evolve_success")]
    private Transform evolveSuccess;

    [Widget("evolve_panel/evolve/outsideframe_top/cost/cost_txt")]
    private Text goldText;

    [Widget("evolve_panel/evolve/outsideframe_top/left/stage_left_txt")]
    private Text leftGradeText;

    [Widget("evolve_panel/evolve/outsideframe_top/left/stage_leftstar_group")]
    private Transform leftStarGroup;

    [Widget("evolve_panel/evolve/outsideframe_top/material_group")]
    private ScrollView matrialScrollView;

    [Widget("evolve_panel/evolve/outsideframe_top/material_group")]
    private Transform matrilGroup;

    [Widget("evolve_panel/evolve/outsideframe_top/right/stage_right_txt")]
    private Text rightGradeText;

    [Widget("evolve_panel/evolve/outsideframe_top/right/stage_rightstar_group")]
    private Transform rightStarGroup;

    private Transform evolveNode;
    private Transform left;
    private Transform leftAttr;
    private Transform rightAttr;
    private Transform leftAttrTemp;
    private Transform rightAttrTemp;
    private Transform maxHint;

    private PetSelectModule petSelectModule { get { return parentWindow.SelectModule; } }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _petProcessEvolveSuccess.Destroy();
    }

    protected override void InitComponent()
    {
        base.InitComponent();
        _petProcessEvolveSuccess = CreateSubWindow<PetProcess_EvolveSuccess>(parentWindow, evolveSuccess.gameObject);

        left = WindowCache.GetComponent<Transform>("evolve_panel/evolve/outsideframe_top/left");
        leftAttr        = WindowCache.GetComponent<Transform>("evolve_panel/evolve/outsideframe_top/left/attr");
        rightAttr       = WindowCache.GetComponent<Transform>("evolve_panel/evolve/outsideframe_top/right/attr");
        leftAttrTemp    = WindowCache.GetComponent<Transform>("evolve_panel/evolve/outsideframe_top/left/attr/0");
        rightAttrTemp   = WindowCache.GetComponent<Transform>("evolve_panel/evolve/outsideframe_top/right/attr/0");
        evolveNode      = WindowCache.GetComponent<Transform>("evolve_panel/evolve");
        maxHint         = WindowCache.GetComponent<Transform>("evolve_panel/evolve/nothing");

        leftAttrTemp?.SafeSetActive(false);
        rightAttrTemp?.SafeSetActive(false);
    }

    public override bool Initialize(params object[] p)
    {
        if (base.Initialize(p))
        {
            var pet = petSelectModule.selectInfo;
            evolveButton.onClick.AddListener(OnEvolve);
            var max = pet.IsEvolveMax();
            evolveButton.transform.SetGray(max);
            evolveButton.SetInteractable(!max);
            maxHint.SafeSetActive(max);
            OnSelect(pet);
        }
        return true;
    }

    public override bool UnInitialize(bool hide = true)
    {
        if (!base.UnInitialize(hide))
            return false;

        evolveButton.onClick.RemoveListener(OnEvolve);
        parentWindow.SelectPetInfo?.RefreshEvolveReadState();
        return true;
    }

    public override bool OnReturn()
    {
        if (_petProcessEvolveSuccess.Root.activeInHierarchy)
        {
            _petProcessEvolveSuccess.Root.SetActive(false);
            _petProcessEvolveSuccess.UnInitialize();
            evolveNode.SafeSetActive(true);
            return true;
        }
        if (base.OnReturn())
        {
            moduleGlobal.targetMatrial?.Clear();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 点击进化按钮
    /// </summary>
    private void OnEvolve()
    {
        if (null == petSelectModule.selectInfo) return;

        var costInfo = petSelectModule.selectInfo.UpGradeInfo.upgradeCost;
        if (costInfo.gold > modulePlayer.coinCount)
        {
            modulePlayer.DispatchModuleEvent(Module_Player.EventBuySuccessCoin);
            return;
        }
        // TODO NETWORK发送进化消息
        var msg = PacketObject.Create<CsPetUpgrade>();
        msg.petId = petSelectModule.selectInfo.ItemID;
        session.Send(msg);
    }


    private void OnSelect(PetInfo rInfo)
    {
        RefreshLeft(rInfo);
        RefreshRight(rInfo);
        RefreshMatrial();
        RefreshEvolveState();
        if(rInfo.IsEvolveMax() && maxHint != null)
        {
            maxHint.gameObject.SafeSetActive(true);
        }
    }

    private void RefreshEvolveState()
    {
        if (petSelectModule.selectInfo == null) return;
        evolveButton.SafeSetActive(modulePet.Contains(petSelectModule.selectInfo.ID));
    }

    private void RefreshLeft(PetInfo rInfo)
    {
        left.SafeSetActive(!rInfo.IsEvolveMax());

        var grade = rInfo.UpGradeInfo;
        Util.SetText(leftGradeText, grade.CombineGradeName(rInfo.Star));
        SetStar(leftStarGroup, rInfo.Star);
        Util.ClearChildren(leftAttr);
        var showList = rInfo.Attribute;
        for (var i = 0; i < showList.Count; i++)
        {
            var t = leftAttr.AddNewChild(leftAttrTemp);
            t.SafeSetActive(true);
            BindProperty(t, showList[i]);
        }
    }
    private void BindProperty(Transform t, ItemAttachAttr show, ItemAttachAttr change = null)
    {
        Util.SetText(t.GetComponent<Text>("value"), show.ValueString());
        Util.SetText(t.GetComponent<Text>("type"), show.TypeString());
        var c = t.GetComponent<Text>("value/change");
        if (change != null)
            Util.SetText(c, $"【+{change.ValueString()}】");
        var isMax = petSelectModule.selectInfo.IsEvolveMax();
        c.SafeSetActive(!isMax);
        t.GetComponent<Transform>("value/max").SafeSetActive(isMax);
    }

    private void RefreshRight(PetInfo rInfo)
    {
        var gradeInfo = rInfo.GetUpGradeInfo(rInfo.Grade + 1);
        var star = rInfo.GetStar(rInfo.Grade + 1);
        Util.SetText(rightGradeText, gradeInfo?.CombineGradeName(star));
        SetStar(rightStarGroup, star);

        Util.ClearChildren(rightAttr);
        //创建属性条
        var prevList = rInfo.Attribute;

        var showList = rInfo.GetAttribute(rInfo.Level, rInfo.Grade + 1);
        for (var i = 0; i < showList.Count; i++)
        {
            var t = rightAttr.AddNewChild(rightAttrTemp);
            t.SafeSetActive(true);
            var change = new ItemAttachAttr()
            {
                id = showList[i].id,
                type = showList[i].type,
                value = AttributeShowHelper.ValueForShow(showList[i].id, showList[i].value) - AttributeShowHelper.ValueForShow(prevList[i].id, prevList[i].value)
            };
            BindProperty(t, showList[i], change);
        }
    }

    /// <summary>
    /// 更新进阶消耗材料
    /// </summary>
    private void RefreshMatrial()
    {
        if (petSelectModule.selectInfo == null) return;
        var costInfo = petSelectModule.selectInfo.UpGradeInfo.upgradeCost;
        if (null == costInfo) return;
#if UNITY_EDITOR
        object[] a = new object[costInfo.items.Length + 1];
        a[0] = petSelectModule.selectInfo.Grade;
        for (var i = 0; i < costInfo.items.Length; i++)
        {
            a[i + 1] = costInfo.items[i].itemId;
        }
#endif
        AddItem(costInfo.items);
        Util.SetText(goldText, costInfo.gold.ToString());
        goldText.color = ColorGroup.GetColor(ColorManagerType.IsMoneyEnough, costInfo.gold <= modulePlayer.coinCount);

        evolveButton.interactable = costInfo.gold <= modulePlayer.coinCount && IsMatrialEnough(costInfo.items);
    }

    public static bool IsMatrialEnough(ItemPair[] items)
    {
        if (items == null || items.Length == 0)
            return false;
        for (var i = 0; i < items.Length; i++)
        {
            if (!IsMatrialEnough(items[i]))
                return false;
        }
        return true;
    }

    private static bool IsMatrialEnough(ItemPair data)
    {
        return data.count <= moduleEquip.GetPropCount(data.itemId);
    }

    private void AddItem(ItemPair[] items)
    {
        new DataSource<ItemPair>(items, matrialScrollView, OnSetMatrial, OnMatrialClick);
    }

    private void OnSetMatrial(RectTransform node, ItemPair data)
    {
        var propInfo = ConfigManager.Get<PropItemInfo>(data.itemId);
        Util.SetItemInfo(node, propInfo, 0, data.count);
        var countText = node.GetComponent<Text>("numberdi/count");
        Util.SetText(countText, Util.Format("{0}/{1}", moduleEquip.GetPropCount(data.itemId), data.count ));
        countText.color = ColorGroup.GetColor(ColorManagerType.IsMatrialEnough, IsMatrialEnough(data));
    }

    private void OnMatrialClick(RectTransform node, ItemPair data)
    {
        int petId = petSelectModule.selectInfo.ID;
        moduleGlobal.SetTargetMatrial((ushort) data.itemId, data.count, Tuple.Create(Window_Sprite.SUB_TYPE_EVOLVE, petId));
    }


    private void _ME(ModuleEvent<Module_Pet> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Pet.ResponseEvolve:
                ResponseEvolve(e.msg as ScPetUpgrade);
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Player> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Player.EventBuySuccessCoin:
            case Module_Player.EventCurrencyChanged:
                RefreshMatrial();
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Equip> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Equip.EventBagInfo:
            case Module_Equip.EventUpdateBagProp:
                RefreshMatrial();
                break;
        }
    }

    private void ResponseEvolve(ScPetUpgrade msg)
    {
        if (msg == null)
            return;
        if (msg.response == 0)
        {
            evolveNode.SafeSetActive(false);
            var pet = modulePet.GetPet(msg.petId);
            maxHint.SafeSetActive(pet.IsEvolveMax());
            OnSelect(pet);
            _petProcessEvolveSuccess.Initialize();
            return;
        }
        moduleGlobal.ShowMessage(9703, msg.response);
    }

    private void SetStar(Transform starGroup, int star)
    {
        if (starGroup == null) return;
        for (var i = 0; i < starGroup.childCount; i++)
        {
            var t = starGroup.GetChild(i);
            t.SafeSetActive(i < star);
        }
    }
}
