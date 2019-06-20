/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-02-22
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

public class Window_RuneMain : Window
{
    PItem _item;
    Transform runeIcon;
    Transform alreadyEquip;
    Text runeName;
    Image[] stars;
    Button sendLock;
    Image lockImage;
    Image unlockImage;
    Text lockDesc;
    Text runeLv;
    Text runeExpDesc;
    RectTransform fillamount;
    Transform[] attrPanel;
    Text runeSubType;
    Text twoSuiteDesc;
    Text fourSuiteDesc;
    Button replaceBtn;
    Button upRuneBtn;
    Text upBtnText;
    float width;

    protected override void OnOpen()
    {
        runeIcon = GetComponent<Transform>("lingpo_img");
        alreadyEquip = GetComponent<Transform>("get_img");
        runeName = GetComponent<Text>("message_panel/title/Text");
        stars = GetComponent<Transform>("message_panel/title/star_parent")?.GetComponentsInChildren<Image>(true);
        sendLock = GetComponent<Button>("message_panel/title/lock");
        lockImage = GetComponent<Image>("message_panel/title/lock/lock");
        unlockImage = GetComponent<Image>("message_panel/title/lock/unlock");
        lockDesc = GetComponent<Text>("message_panel/title/lock/Text");
        runeLv = GetComponent<Text>("message_panel/middle_panel/level");
        runeExpDesc = GetComponent<Text>("message_panel/middle_panel/slider/Text");
        fillamount = GetComponent<RectTransform>("message_panel/middle_panel/slider/fill_img");
        var attrPanel_1 = GetComponent<Transform>("message_panel/middle_panel/panel/count1");
        var attrPanel_2 = GetComponent<Transform>("message_panel/middle_panel/panel/count2");
        var attrPanel_3 = GetComponent<Transform>("message_panel/middle_panel/panel/count3");
        attrPanel = new Transform[] { attrPanel_1, attrPanel_2, attrPanel_3 };
        runeSubType = GetComponent<Text>("message_panel/middle_panel/panel/icon_img/Text");
        twoSuiteDesc = GetComponent<Text>("message_panel/two_count/panel/Text");
        fourSuiteDesc = GetComponent<Text>("message_panel/four_count/panel/Text");
        replaceBtn = GetComponent<Button>("message_panel/change_btn");
        upRuneBtn = GetComponent<Button>("message_panel/add_btn");
        upBtnText = GetComponent<Text>("message_panel/add_btn/Text");

        width = fillamount.parent.rectTransform().rect.width;
        replaceBtn?.onClick.RemoveAllListeners();
        replaceBtn?.onClick.AddListener(() => OpenEquipWindow(moduleRune.curOpItem));
        upRuneBtn?.onClick.RemoveAllListeners();
        upRuneBtn?.onClick.AddListener(() => OnUpCurRune(moduleRune.curOpItem));
        sendLock?.onClick.RemoveAllListeners();
        sendLock?.onClick.AddListener(()=> OnLockOrUnlock(moduleRune.curOpItem));

        MultiLanguage();
    }

    private void OnLockOrUnlock(PItem item)
    {
        if (item == null) return;
        moduleCangku.SetLock(item.itemId, item.isLock == 0 ? 1 : 0);
    }

    private void OnUpCurRune(PItem curRune)
    {
        if (curRune == null || curRune.growAttr == null || curRune.growAttr.runeAttr == null) return;
        var isMax = moduleRune.IsMaxLv(curRune.growAttr.runeAttr.level);
        if (isMax)
        {
            moduleGlobal.ShowMessage(Util.GetString((int)TextForMatType.RuneUIText, 9));
            return;
        }
        var iscurMax = moduleRune.IsCurMaxLv(curRune.growAttr.runeAttr.level, curRune.growAttr.runeAttr.star);

        moduleRune.runeOpType = iscurMax ? RuneInWhichPanel.Evolve : RuneInWhichPanel.Intentify;
        moduleRune.SetCurOpItem(curRune);
        ShowAsync<Window_RuneEquip>();
    }

    private void OpenEquipWindow(PItem curRune)
    {
        if (curRune == null) return;
        moduleRune.runeOpType = RuneInWhichPanel.Equip;
        moduleRune.SetCurOpItem(curRune);
        ShowAsync<Window_RuneEquip>();
    }

    void MultiLanguage()
    {
        var index = (int)TextForMatType.RuneUIText;
        Util.SetText(GetComponent<Text>("get_img/Text"), index, 10);
        Util.SetText(GetComponent<Text>("message_panel/two_count/Text"), index, 11);
        Util.SetText(GetComponent<Text>("message_panel/four_count/Text"), index, 12);
        Util.SetText(GetComponent<Text>("message_panel/change_btn/Text"), index, 13);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();

        var param = GetWindowParam(name);
        if (param != null)
        {
            moduleRune.sourceType = param.param2 == null ? 0 : (int)param.param2;
            _item = param.param1 as PItem;
        }
        if (_item == null) return;

        moduleRune.SetCurOpItem(_item);
        Refresh(_item);
    }

    protected override void GrabRestoreData(WindowHolder holder)
    {
        holder.SetData(_item);
    }

    protected override void ExecuteRestoreData(WindowHolder holder)
    {
        _item = holder.GetData<PItem>(0);
    }

    void Refresh(PItem item)
    {
        if (item == null || item.growAttr == null || item.growAttr.runeAttr == null) return;

        var prop = ConfigManager.Get<PropItemInfo>(item.itemTypeId);
        if (prop == null) return;

        moduleRune.UpdateRead(1, true, prop.subType);

        var isEquip = moduleRune.IsEquip(item);
        alreadyEquip.SafeSetActive(isEquip);

        if (prop.mesh != null && prop.mesh.Length > 1) UIDynamicImage.LoadImage(runeIcon, prop.mesh[1]);

        Util.SetText(runeName, prop.itemName);
        if (stars != null)
        {
            for (int i = 0; i < stars.Length; i++)
                stars[i].SafeSetActive(i < item.growAttr?.runeAttr?.star);
        }

        RefreshLockState(item);
        Util.SetText(runeLv, $"Lv.{item.growAttr?.runeAttr?.level}");
        Util.SetText(runeExpDesc, moduleRune.GetExpDescString(item));
        Util.SetText(runeSubType, moduleRune.GetRomaString(prop));

        var progress = moduleRune.GetExpProgress(item);
        progress = moduleRune.IsCurMaxLv(item.growAttr.runeAttr.level, item.growAttr.runeAttr.star) ? 1 : progress;
        fillamount.sizeDelta = new Vector2(width * progress, fillamount.sizeDelta.y);

        if (attrPanel != null)
        {
            for (int i = 0; i < attrPanel.Length; i++)
            {
                attrPanel[i].SafeSetActive(i < item.growAttr?.runeAttr?.randAttrs?.Length);
                if (i >= item.growAttr?.runeAttr?.randAttrs?.Length) continue;
                if (item.growAttr == null || item.growAttr.runeAttr == null || item.growAttr.runeAttr.randAttrs == null) continue;

                var attrId = item.growAttr.runeAttr.randAttrs[i].itemAttrId;
                var _atrrId = moduleRune.GetCurAttrId(attrId);
                var name = Util.GetString((int)TextForMatType.AttributeUIText, _atrrId);
                Util.SetText(attrPanel[i].Find("name_txt")?.gameObject, name);
                Util.SetText(attrPanel[i].Find("count_txt")?.gameObject, "+" + moduleRune.GetCurRuneAttr(item, attrId));
            }
        }

        Util.SetText(twoSuiteDesc, moduleRune.GetSuiteDesc(prop.suite, 2));
        Util.SetText(fourSuiteDesc, moduleRune.GetSuiteDesc(prop.suite, 4));

        var isMax = moduleRune.IsCurMaxLv(item.growAttr.runeAttr.level, item.growAttr.runeAttr.star);
        Util.SetText(upBtnText, isMax ? Util.GetString((int)TextForMatType.RuneUIText, 8) : Util.GetString((int)TextForMatType.RuneUIText, 7));
    }

    void RefreshLockState(PItem item)
    {
        if (item == null) return;
        lockImage.SafeSetActive(item.isLock == 1);
        unlockImage.SafeSetActive(item.isLock == 0);
        Util.SetText(lockDesc, item.isLock == 1 ? Util.GetString((int)TextForMatType.RuneUIText, 14) : Util.GetString((int)TextForMatType.RuneUIText, 15));
    }

    void _ME(ModuleEvent<Module_Cangku> e)
    {
        if (actived && e.moduleEvent == Module_Cangku.EventCangkuItemLock)
        {
            RefreshLockState(moduleRune.curOpItem);
        }
    }
}
