// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-10-15      14:44
//  * LastModify：2018-10-18      16:43
//  ***************************************************************************************************/

#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class Window_Sublimation : Window
{
    private int          drawingId;
    private Transform    drawingItem;
    private Button       excuteButton;
    private Image        icon;
    private bool         isMatrialEnough;
    private Text         levelText;
    private Transform    matrialRoot;
    private Transform    matrialTemp;
    private Text         nameText;
    private Transform    nothingNode;
    private Transform    suitNode;
    private SuitProperty previewSuit;
    private Button       selectDrawingButton;
    private Button       cancelButton;
    private Transform    rawImage;

    private Image consumeIcon;
    private Text consumeCount;

    private FurnaceWindow_SublimationSelectDrawing selectDrawingWindow;
    private FurnaceWindow_SublimationSuccess       successWindow;

    protected override void OnOpen()
    {
        base.OnOpen();
        InitCompoments();
        MultiLangrage();

        excuteButton?.onClick.AddListener(OnExcute);
        selectDrawingButton?.onClick.AddListener(OnSelectDrawing);
        cancelButton?.onClick.AddListener(OnCancel);
    }

    private void InitCompoments()
    {
        excuteButton        = GetComponent<Button>      ("disintergrade_Btn");
        nameText            = GetComponent<Text>        ("info/name");
        levelText           = GetComponent<Text>        ("info/name/level");
        icon                = GetComponent<Image>       ("info/icon");
        drawingItem         = GetComponent<Transform>   ("right/bottom/0");
        matrialRoot         = GetComponent<Transform>   ("right/bottom/list");
        matrialTemp         = GetComponent<Transform>   ("right/bottom/list/0");
        nothingNode         = GetComponent<Transform>   ("nothing");
        suitNode            = GetComponent<Transform>   ("right");
        selectDrawingButton = GetComponent<Button>      ("nothing/bottom/Image");
        cancelButton        = GetComponent<Button>      ("right/bottom/0/cancel");
        rawImage            = GetComponent<Transform>   ("npcInfo");

        consumeIcon         = GetComponent<Image>       ("right/bottom/consume1/icon");
        consumeCount        = GetComponent<Text>        ("right/bottom/consume1/value");

        matrialTemp.SafeSetActive(false);

        previewSuit         = new SuitProperty(GetComponent<Transform>("right/top/attr"));
        successWindow       = SubWindowBase                    .CreateSubWindow<FurnaceWindow_SublimationSuccess>(this, GetComponent<Transform>("success_Panel")?.gameObject);
        selectDrawingWindow = SubWindowBase<Window_Sublimation>.CreateSubWindow<FurnaceWindow_SublimationSelectDrawing>(this, GetComponent<Transform>("tip")?.gameObject);
        selectDrawingWindow.Set(false);
    }

    protected override void OnClose()
    {
        base.OnClose();
        successWindow?.Destroy();
        selectDrawingWindow?.Destroy();
    }

    private void MultiLangrage()
    {
        var ct = ConfigManager.Get<ConfigText>((int) TextForMatType.SublimationUI);
        Util.SetText(GetComponent<Text>("right/top/title"),         ct[3]);
        Util.SetText(GetComponent<Text>("right/bottom/consume1"),   ct[4]);
        Util.SetText(GetComponent<Text>("nothing/top/nothing"),     ct[5]);
        Util.SetText(GetComponent<Text>("disintergrade_Btn/Text"),  ct[6]);
        Util.SetText(GetComponent<Text>("tip/content/equipinfo"),   ct[7]);
        Util.SetText(GetComponent<Text>("tip/content/yes_button/Text"),   ct[8]);
        Util.SetText(GetComponent<Text>("tip/content/getBtn/Text"), ct[9]);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();

        RefreshTitle();
        
        if(forward)
            RefreshItemInfo();

        if (moduleGlobal.targetMatrial.isProcess)
        {
            if (moduleGlobal.targetMatrial.isFinish)
                BindDrawingItem((int) moduleGlobal.targetMatrial.data);
            moduleGlobal.targetMatrial.Clear();
        }
    }
    protected override void GrabRestoreData(WindowHolder holder)
    {
        holder.SetData(drawingId);
    }

    protected override void ExecuteRestoreData(WindowHolder holder)
    {
        var bind = holder.GetData(0, -1);
        if (bind == -1) return;

        BindDrawingItem(bind);
    }

    protected override void OnReturn()
    {
        moduleGlobal.targetMatrial?.Clear();
        if (successWindow.OnReturn())
        {
            rawImage.SafeSetActive(true);
            if (!moduleFurnace.IsSublimationMax(moduleFurnace.currentSublimationItem))
                return;
        }

        var key = new NoticeDefaultKey(NoticeType.Sublimation);
        moduleNotice.SetNoticeState(key, moduleEquip.CheckSublimation(moduleFurnace.currentSublimationItem));
        moduleNotice.SetNoticeReadState(key);
        base.OnReturn();
    }

    private void OnSelectDrawing()
    {
        selectDrawingWindow.Initialize();
    }

    private void OnCancel()
    {
        BindDrawingItem(0);
    }

    private void OnExcute()
    {
        var item = moduleEquip.GetProp(drawingId);
        if (null == item)
        {
            moduleGlobal.ShowMessage((int)TextForMatType.SublimationUI, 2);
            return;
        }

        if (!isMatrialEnough)
        {
            moduleGlobal.ShowMessage((int)TextForMatType.SublimationUI, 1);
            return;
        }

        moduleFurnace.RequestSublimationExcute(item.itemId);
    }

    private void RefreshItemInfo()
    {
        var nowSuitId = moduleFurnace.currentSublimationItem.growAttr.suitId;
        SetMax(false);
        if ( nowSuitId > 0)
        {
            var suitInfo = ConfigManager.Find<SuitInfo>(item => item.prevSuitId == nowSuitId);
            //没有后置套装就认为已经满级了
            if (suitInfo == null)
            {
                SetMax(true);
                //如果已经满级显示现有图标
                suitInfo = ConfigManager.Get<SuitInfo>(nowSuitId);
            }
            BindDrawingItem(suitInfo?.drawingId ?? 0);
        }
        else
            ShowSuitState(false);

        cancelButton.SafeSetActive(nowSuitId == 0);
    }

    public bool RefreshTitle()
    {
        var prop = moduleFurnace.currentSublimationItem?.GetPropItem();
        if (null == prop)
            return false;
        int index = Module_Forging.instance.GetWeaponIndex(prop);
        if (index != -1) AtlasHelper.SetShared(icon, ForgePreviewPanel.WEAPON_TYPE_IMG_NAMES[index]);
        Util.SetText(nameText, prop.itemName);
        Util.SetText(levelText, "+" + moduleFurnace.currentSublimationItem.GetIntentyLevel());
        return true;
    }

    private void SetMax(bool isMax)
    {
        excuteButton.transform.SetGray(isMax);
        excuteButton.SetInteractable(!isMax);
    }

    public void BindDrawingItem(int rDrawingId, SuitInfo rSuitInfo = null)
    {
        drawingId = rDrawingId;
        if (rDrawingId <= 0)
        {
            ShowSuitState(false);
            return;
        }

        ShowSuitState(true);

        if (rSuitInfo == null)
            rSuitInfo = ConfigManager.Find<SuitInfo>(item => item.drawingId == drawingId);
        var suitId = rSuitInfo?.ID ?? 0;
        if(excuteButton.interactable)
            previewSuit.Init(suitId, moduleEquip.GetSuitNumber(suitId) + 1, moduleEquip.IsDressOn(moduleFurnace.currentSublimationItem), true);
        else
            previewSuit.Init(suitId, moduleEquip.GetSuitNumber(suitId), moduleEquip.IsDressOn(moduleFurnace.currentSublimationItem), false);

        ShowSuitMatrial(rSuitInfo);
    }

    private void ShowSuitState(bool rHasSuit)
    {
        nothingNode?.SafeSetActive(!rHasSuit);
        suitNode?.SafeSetActive(rHasSuit);
    }

    private void ShowSuitMatrial(SuitInfo rSuitInfo)
    {
        if (null == rSuitInfo)
            return;
        isMatrialEnough = true;

        Util.SetText(consumeCount, "0");

        BindItemInfo(drawingItem, drawingId, 1);

        Util.ClearChildren(matrialRoot);
        for (var i = 0; i < rSuitInfo.costs.Length; i++)
        {
            var itemId = rSuitInfo.costs[i].itemId;

            var prop = ConfigManager.Get<PropItemInfo>(itemId);
            if (itemId == 1)
            {
                AtlasHelper.SetIcons(consumeIcon, prop.icon);
                var a = rSuitInfo.costs[i].count;
                var b = modulePlayer.coinCount;
                Util.SetText(consumeCount, a.ToString());
                isMatrialEnough = isMatrialEnough && a <= b;
                consumeCount.color = ColorGroup.GetColor(ColorManagerType.IsMoneyEnough, a <= b);
                continue;
            }

            var t = matrialRoot.AddNewChild(matrialTemp);
            t.SafeSetActive(true);
            BindItemInfo(t, itemId, rSuitInfo.costs[i].count);
        }
    }

    private void BindItemInfo(Transform t, int itemId, int count)
    {
        var itemInfo = ConfigManager.Get<PropItemInfo>(itemId);
        Util.SetItemInfo(t, itemInfo);
        var countText = t.GetComponent<Text>("numberdi/count");
        var own = moduleEquip.GetPropCount(itemId);
        Util.SetText(countText, $"{own}/{count}");
        countText.color = ColorGroup.GetColor(ColorManagerType.IsMatrialEnough, count <= own);
        isMatrialEnough = isMatrialEnough && count <= own;

        t.GetComponentDefault<Button>()?.onClick.RemoveAllListeners();
        t.GetComponentDefault<Button>()?.onClick.AddListener(delegate
        {
            moduleGlobal.SetTargetMatrial(itemId, count, drawingId);
        });
    }

    private void _ME(ModuleEvent<Module_Furnace> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Furnace.ResponseSublimationExcute:
                ResponseSublimationExcute(e.msg as ScSublimationExcute);
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Equip> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Equip.EventUpdateBagProp:
            case Module_Equip.EventBagInfo:
                BindDrawingItem(drawingId);
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Player> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Player.EventBuySuccessCoin:
            case Module_Player.EventCurrencyChanged:
                BindDrawingItem(drawingId);
                break;
        }
    }

    private void ResponseSublimationExcute(ScSublimationExcute msg)
    {
        if (msg.result != 0)
        {
            moduleGlobal.ShowMessage(9751, msg.result);
            return;
        }

        moduleFurnace.currentSublimationItem = moduleEquip.GetProp(moduleFurnace.currentSublimationItem.itemId);
        RefreshItemInfo();
        rawImage.SafeSetActive(false);
        successWindow.Initialize(moduleFurnace.currentSublimationItem, msg.prevSuitId);
    }
}

public class SuitProperty
{
    private readonly Transform[] activePoints;
    private readonly Transform   group;
    public  readonly Transform   Root;
    private readonly Text        suitName;
    private readonly Transform   templete;

    private List<GameObject> attributeList = new List<GameObject>(); 

    public SuitProperty(Transform rRoot)
    {
        if (rRoot == null)
            return;
        Root = rRoot;
        suitName = rRoot.GetComponent<Text>("name");
        group    = rRoot.GetComponent<Transform>();
        templete = rRoot.GetComponent<Transform>("0");
        templete?.SafeSetActive(false);

        activePoints = new Transform[3];
        activePoints[0] = rRoot.GetComponent<Transform>("name/0/active");
        activePoints[1] = rRoot.GetComponent<Transform>("name/1/active");
        activePoints[2] = rRoot.GetComponent<Transform>("name/2/active");
    }

    public void Init(int  suitId, 
                     int  num, 
                     bool isDressOn, 
                     bool breath = false)
    {
        if (!isDressOn) num = 0;

        for (var i = 0; i < activePoints.Length; i++)
            activePoints[i].SafeSetActive(i < num);

        foreach (var go in attributeList)
            GameObject.Destroy(go);

        attributeList.Clear();

        if (suitId <= 0)
        {
            Root.SafeSetActive(false);
            return;
        }
        
        var suitInfo = ConfigManager.Get<SuitInfo>(suitId);
        if (null == suitInfo)
        {
            Root.SafeSetActive(false);
            return;
        }
        Root.SafeSetActive(true);

        Util.SetText(suitName, $"{ConfigText.GetDefalutString(suitInfo.nameId)}({num}/3)");

        for (var i = 0; i < suitInfo.effectDescs.Length; i++)
        {
            var t = group.AddNewChild(templete);
            t.SafeSetActive(true);
            attributeList.Add(t.gameObject);
            var text = t?.GetComponent<Text>("content");
            Util.SetText(text, suitInfo.effectDescs[i]);
            if (text)
                text.color = ColorGroup.GetColor(ColorManagerType.SuitActive, i < num && isDressOn);

            Util.SetText(t?.GetComponent<Text>("type"),
                Util.Format(ConfigText.GetDefalutString(TextForMatType.SublimationUI, 0), i + 1));
            
            var skillIconRoot = t?.GetComponent<Transform>("skill");
            if (null != skillIconRoot)
            {
                //不显示技能图标了
                skillIconRoot.SafeSetActive(false /*suitInfo.skillEffects[i].Length > 0*/);
                if (skillIconRoot.gameObject.activeInHierarchy)
                {
                    var skillInfo = ConfigManager.Get<SkillInfo>(suitInfo.skillEffects[i][0].skillId);
                    AtlasHelper.SetSkillIcon(skillIconRoot.GetComponent<Transform>("frame/icon"), skillInfo?.skillIcon);
                }
            }

            if (!isDressOn)
                continue;

            var tweens = t.GetComponentsInChildren<TweenBase>();
            if (null == tweens || tweens.Length == 0)
                continue;
            if (i == num - 1 && breath)
            {
                for (var index = 0; index < tweens.Length; index++)
                {

                    tweens[index].enabled = true;
                    tweens[index].Play();
                }
            }
            else
            {
                for (var index = 0; index < tweens.Length; index++)
                {
                    tweens[index].enabled = false;
                }
            }
        }
    }
}