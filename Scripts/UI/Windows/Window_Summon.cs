// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-09-19      20:06
//  * LastModify：2018-09-25      18:20
//  ***************************************************************************************************/
#region

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class Window_Summon : Window
{
    private PetSummonSSS        _petSummonSuccess;
    private Button              addStoneButton;
    private Button              confirmButton;
    private Text                costCountText;
    private Text                costCountText2;
    private NpcMono             felling;
    private Transform           mainContent;
    private Transform           npcUIRoot;

    private RewardShow          rewardShow;
    private Text                stoneCountText;
    private ScPetSummon         summonCache;
    private Button              summonOnce;
    private Button              summonOnce2;
    private Button              summonTen;
    private Button              summonTen2;
    private int                 summonTimes;
    private int                 buyTimes;
    private ScrollView          scrollView;
    private Transform           dropNode;
    private Button              maskButton;

    protected override void OnOpen()
    {
        base.OnOpen();

        rewardShow      = GetComponent<RewardShow>  ("topLayer/tips");
        summonOnce      = GetComponent<Button>      ("main/summon_01/summon_Btn");
        summonTen       = GetComponent<Button>      ("main/summon_01/summon_Btn02");
        summonOnce2     = GetComponent<Button>      ("topLayer/tips/wish_goods/main/summon_01/summon_Btn");
        summonTen2      = GetComponent<Button>      ("topLayer/tips/wish_goods/main/summon_01/summon_Btn02");
        addStoneButton  = GetComponent<Button>      ("topLayer/stone/add");
        confirmButton   = GetComponent<Button>      ("popup/yes");
        costCountText   = GetComponent<Text>        ("main/summon_01/cost_txt/count");
        costCountText2  = GetComponent<Text>        ("topLayer/tips/wish_goods/main/summon_01/cost_txt/count");
        felling         = GetComponent<NpcMono>     ("content/npcInfo");
        mainContent     = GetComponent<Transform>   ("main");
        npcUIRoot       = GetComponent<Transform>   ("content/role_render/Root");
        stoneCountText  = GetComponent<Text>        ("topLayer/stone/text");
        _petSummonSuccess = GetComponent<PetSummonSSS> ("topLayer/tips/jipin");
        scrollView      = GetComponent<ScrollView>  ("dropInfo/info/inner/items");
        dropNode        = GetComponent<Transform>   ("dropInfo");
        maskButton      = GetComponent<Button>      ("mask");

        MultiLanguage();
        RefreshCost();

        confirmButton?.onClick.AddListener(OnBuyConfirm);
        summonOnce   ?.onClick.AddListener(OnSummonOnce);
        summonTen    ?.onClick.AddListener(OnSummonTen);
        summonOnce2  ?.onClick.AddListener(OnSummonOnce);
        summonTen2   ?.onClick.AddListener(OnSummonTen);
        maskButton   ?.onClick.AddListener(OnMaskClick);
        maskButton.SafeSetActive(false);
        CloseToNpc(true);

        rewardShow.Regirest(ItemIsPet, _petSummonSuccess);
        rewardShow.OnClose += OnFocus;
        rewardShow.onAnimEnd.AddListener(() =>
        {
            summonOnce2.transform.parent.SafeSetActive(!moduleGuide.inGuideProcess);
            addStoneButton.transform.parent.SafeSetActive(true);
        });
        rewardShow.SetBinder(new SummonTempleteBind());

        addStoneButton.onClick.AddListener(() =>
        {
            UpdateGemText(1);
        });

        new DataSource<PWishItemDropInfo>(modulePet.dropInfo, scrollView, SetDropItem);

    }

    private void OnMaskClick()
    {
        moduleHome.DispatchEvent(Module_Home.EventInterruptSummonEffect);
        maskButton?.SafeSetActive(false);
    }

    private void SetDropItem(RectTransform rt, PWishItemDropInfo data)
    {
        var item = ConfigManager.Get<PropItemInfo>(data.itemID);

        Util.SetItemInfoSimple(rt, item);
        Util.SetText(rt.GetComponent<Text>("rate"), 9406, (data.dropRate * 100).ToString("F2"));
    }

    private bool ItemIsPet(PItem2 rItem)
    {
        if (rItem == null)
            return false;

        var prop = ConfigManager.Get<PropItemInfo>(rItem.source != 0 ? rItem.source : rItem.itemTypeId);
        return prop.itemType == PropType.Pet;
    }

    private void OnSummonTen()
    {
        OnSummonClick(10);
    }

    private void OnSummonOnce()
    {
        OnSummonClick(1);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault(1, false);
        moduleHome.DispatchEvent(Module_Home.EventSwitchCameraMode, Event_.Pop(CameraShowType.PetSummon));
    }

    protected override void OnHide(bool forward)
    {
        base.OnHide(forward);
        moduleHome.DispatchEvent(Module_Home.EventSwitchCameraMode, Event_.Pop(CameraShowType.Home));
    }

    protected override void OnReturn()
    {
        if (rewardShow.gameObject.activeSelf)
        {
            rewardShow.OnReturn();
            return;
        }

        if (dropNode.gameObject.activeInHierarchy)
        {
            dropNode.SafeSetActive(false);
            return;
        }
        base.OnReturn();
    }

    private void OnBuyConfirm()
    {
        if (modulePet.buyStoneCostDiamond > modulePlayer.gemCount)
        {
            moduleGlobal.ShowMessage(9402, 1);
            return;
        }
        modulePet.BuySummonStone(true, (int)buyTimes);
    }

    /// <summary>
    /// 点击开始召唤按钮响应
    /// </summary>
    private void OnSummonClick(int rTimes)
    {
        summonTimes = rTimes;
        if (!CostEnough())
        {
            addStoneButton?.onClick?.Invoke();
            //覆盖默认的1次
            UpdateGemText();
            return;
        }
        summonCache?.Destroy();
        summonCache = null;
        modulePet.Summon((byte)summonTimes);
    }


    private void MultiLanguage()
    {
        var ct = ConfigManager.Get<ConfigText>((int) TextForMatType.PetSummonText);

        if (ct == null)
        {
            Logger.LogError("无法找到文本配置：ID = {0}", TextForMatType.PetSummonText);
            return;
        }
        Util.SetText(GetComponent<Text>("summon_01/summon_btn/summon_txt"),                              ct[0]);
        Util.SetText(GetComponent<Text>("summon_01/cost_txt"),                                           ct[1]);
        Util.SetText(GetComponent<Text>("popup/top/equipinfo"),                                          ct[3]);
        Util.SetText(GetComponent<Text>("popup/content1/cost"),                                          ct[4]);
        Util.SetText(GetComponent<Text>("tipsignsccced/success/signsucced/up"),                          ct[6]);
        Util.SetText(GetComponent<Text>("main/summon_01/summon_Btn/text"),                               ct[7]);
        Util.SetText(GetComponent<Text>("topLayer/tips/wish_goods/main/summon_01/summon_Btn/text"),      ct[7]);
        Util.SetText(GetComponent<Text>("main/summon_01/summon_Btn02/text"),                             ct[8]);
        Util.SetText(GetComponent<Text>("topLayer/tips/wish_goods/main/summon_01/summon_Btn02/text"),    ct[8]);
        Util.SetText(GetComponent<Text>("topLayer/tips/wish_goods/back/des"),                            ct[9]);
        Util.SetText(GetComponent<Text>("main/summon_01/cost_txt"),                                      ct[10]);
        Util.SetText(GetComponent<Text>("topLayer/tips/wish_goods/main/summon_01/cost_txt"),             ct[10]);
        Util.SetText(GetComponent<Text>("topLayer/tips/wish_goods/back/title/up_h/up_1"),    ct[6].Substring(0, 1));
        Util.SetText(GetComponent<Text>("topLayer/tips/wish_goods/back/title/up_h/up_2"),    ct[6].Substring(1, 1));
        Util.SetText(GetComponent<Text>("topLayer/tips/wish_goods/back/title/up_h/up_3"),    ct[6].Substring(2, 1));
        Util.SetText(GetComponent<Text>("topLayer/tips/wish_goods/back/title/up_h/up_4"),    ct[6].Substring(3, 1));

        var t = ConfigManager.Get<ConfigText>(9407);
        if (null == t)
            return;
        Util.SetText(GetComponent<Text>("dropInfo/info/title"),                                             t[0]);
        Util.SetText(GetComponent<Text>("dropInfo/info/inner/rulestitle"),                                  t[1]);
        Util.SetText(GetComponent<Text>("dropInfo/info/inner/wish_rules/Viewport/Content/rulescontent"),    t[2]);
        Util.SetText(GetComponent<Text>("dropInfo/info/inner/dropTitle"),                                   t[3]);
    }

    private void UpdateGemText()
    {
        var num = summonTimes - (int)modulePlayer.petSummonStone;
        UpdateGemText(num);
    }

    private void UpdateGemText(int rTimes)
    {
        buyTimes = rTimes;
        var cost = modulePet.buyStoneCostDiamond * buyTimes;
        var ct = ConfigManager.Get<ConfigText>((int)TextForMatType.PetSummonText);
        var str = $"<color=#{Util.ColorToInt(ColorGroup.GetColor(ColorManagerType.IsMatrialEnough, cost <= modulePlayer.gemCount)):X00000000}>x{cost}</color>";
        Util.SetText(GetComponent<Text>("popup/content1/cost/icon/now"),
            Util.Format(ct[5], str, modulePlayer.gemCount));

        Util.SetText(GetComponent<Text>("popup/content1/info"), Util.Format(ConfigText.GetDefalutString(TextForMatType.PetSummonText, 2), buyTimes));
    }


    private bool CostEnough(int rTimes)
    {
        return modulePlayer.petSummonStone >= modulePet.costStone * rTimes;
    }

    private bool CostEnough()
    {
        return CostEnough(summonTimes);
    }

    private void RefreshCost()
    {
        Util.SetText(stoneCountText, modulePlayer.petSummonStone.ToString());
        Util.SetText(costCountText, modulePet.costStone.ToString());
        costCountText.color = ColorGroup.GetColor(ColorManagerType.IsMatrialEnough, CostEnough(modulePet.costStone));
        Util.SetText(costCountText2, modulePet.costStone.ToString());
        costCountText2.color = ColorGroup.GetColor(ColorManagerType.IsMatrialEnough, CostEnough(modulePet.costStone));
    }


    private void CloseToNpc(bool rEnable)
    {
        npcUIRoot.SafeSetActive(rEnable);
    }


    private void _ME(ModuleEvent<Module_Pet> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Pet.SummonSuccess:
                moduleGlobal.OnGlobalTween(true, 1);
                moduleGlobal.UnLockUI();
                var msg = e.msg as ScPetSummon;
                if (msg != null && msg.response == 0 && msg.itemList.Length > 0)
                {
                    msg.CopyTo(ref summonCache);

                    summonOnce2.transform.parent.SafeSetActive(false);
                    addStoneButton.transform.parent.SafeSetActive(false);
                    if (rewardShow.gameObject.activeSelf)
                    {
                        rewardShow.Clear();
                        rewardShow.Show(summonCache.itemList, false, (bool?) e.param1 ?? false);
                    }
                    else
                    {
                        maskButton.SafeSetActive(true);
                        moduleHome.DispatchEvent(Module_Home.EventSummonSuccess);
                    }
                }
                break;
            case Module_Pet.ResponseBuySummonStone:
            {
                int code = (byte)e.param1;
                if (code == 0) AudioManager.PlaySound(AudioInLogicInfo.audioConst.clickToSucc);
                moduleGlobal.UnLockUI();
                moduleGlobal.ShowMessage(9402, code);
                break;
            }
            case Module_Pet.EventEggAnimEnd:
                mainContent.SafeSetActive(false);
                felling.enabled = false;
                moduleGlobal.ShowGlobalLayerDefault(2, false);
                rewardShow.Show(summonCache.itemList, false, (bool?) e.param1 ?? false);
                maskButton.SafeSetActive(false);
                break;
        }
    }

    private void _ME(ModuleEvent<Module_Player> e)
    {
        if (e.moduleEvent == Module_Player.EventCurrencyChanged)
        {
            var t = (CurrencySubType)e.param1;
            if (t == CurrencySubType.PetSummonStone) RefreshCost();
            if (t == CurrencySubType.Diamond) UpdateGemText();
        }
    }

    public void OnFocus()
    {
        Level.current.mainCamera.enabled = false;
        felling.enabled = true;
        mainContent.SafeSetActive(true);
        moduleHome.DispatchEvent(Module_Home.EventCloseSubWindow);
        moduleGlobal.ShowGlobalLayerDefault(1, false);
    }
}
