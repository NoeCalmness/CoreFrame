/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-03-15
 * 
 ***************************************************************************************************/

using cn.sharesdk.unity3d;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Unionreward : Window
{
    private Toggle m_unionReward;
    private Toggle m_friendReward;
    private Button m_releaseBtn;
    private Button m_recordBtn;
    private RectTransform m_rewardPlane;
    private RectTransform m_releasePlane;
    private RectTransform m_recordPlane;
    private Toggle m_releaseEquip;
    private Toggle m_releaseRune;
    private Toggle m_releaseOther;
    private Toggle m_recordGive;
    private Toggle m_recordRecived;
    private Toggle m_recordUnion;
    private Toggle m_recordFriend;

    private RectTransform m_recordNothing;
    private RectTransform m_rewardNothing;
    private RectTransform m_recordInfoPlane;
    private RectTransform m_rewardInfoPlane;

    private DataSource<PReleaseReward> m_rewardData;
    private DataSource<PReleaseRecord> m_recordData;
    private DataSource<PropItemInfo> m_releaseData;
    private bool m_canReturn;

    public string PATH_SHARE_REWARD = Application.dataPath + "/Arts/UI/Atlas/Icons";
    protected override void OnOpen()
    {
        m_canReturn = true;
        m_unionReward = GetComponent<Toggle>("reward_panel/checkBox/union");
        m_friendReward = GetComponent<Toggle>("reward_panel/checkBox/friend");
        m_releaseBtn = GetComponent<Button>("reward_panel/issue");
        m_recordBtn = GetComponent<Button>("reward_panel/record");
        m_rewardPlane = GetComponent<RectTransform>("reward_panel");
        m_releasePlane = GetComponent<RectTransform>("issue_panel");
        m_recordPlane = GetComponent<RectTransform>("record_panel");
        m_releaseEquip = GetComponent<Toggle>("issue_panel/checkBox/equip");
        m_releaseRune = GetComponent<Toggle>("issue_panel/checkBox/rune");
        m_releaseOther = GetComponent<Toggle>("issue_panel/checkBox/other");
        m_recordGive = GetComponent<Toggle>("record_panel/checkBox/give");
        m_recordRecived = GetComponent<Toggle>("record_panel/checkBox/revice");
        m_recordUnion = GetComponent<Toggle>("record_panel/record/checkBox/union");
        m_recordFriend = GetComponent<Toggle>("record_panel/record/checkBox/friend");

        m_recordNothing = GetComponent<RectTransform>("record_panel/record/nothing");
        m_rewardNothing = GetComponent<RectTransform>("reward_panel/nothing");
        m_rewardInfoPlane = GetComponent<RectTransform>("reward_panel/panel");
        m_recordInfoPlane = GetComponent<RectTransform>("record_panel/record/recordScroll");

        m_releaseEquip.onValueChanged.AddListener(delegate
        {
            if (m_releaseEquip.isOn) m_releaseData.SetItems(moduleUnion.ReleaseEquip);
        });
        m_releaseRune.onValueChanged.AddListener(delegate
        {
            if (m_releaseRune.isOn) m_releaseData.SetItems(moduleUnion.ReleaseRune);
        });
        m_releaseOther.onValueChanged.AddListener(delegate
        {
            if (m_releaseOther.isOn) m_releaseData.SetItems(moduleUnion.ReleaseOther);
        });
        m_recordGive.onValueChanged.AddListener(delegate
        {
            if (m_recordGive.isOn)
            {
                m_recordUnion.isOn = false;
                m_recordUnion.isOn = true;
            }
        });
        m_recordRecived.onValueChanged.AddListener(delegate
        {
            if (m_recordRecived.isOn)
            {
                m_recordUnion.isOn = false;
                m_recordUnion.isOn = true;
            }
        });
        m_recordUnion.onValueChanged.AddListener(delegate
        {
            if (m_recordUnion.isOn)
            {
                if (m_recordGive.isOn) SetRecordList(2);
                else if (m_recordRecived.isOn) SetRecordList(0);
            }
        });
        m_recordFriend.onValueChanged.AddListener(delegate
        {
            if (m_recordFriend.isOn)
            {
                if (m_recordRecived.isOn) SetRecordList(1);
                else if (m_recordGive.isOn) SetRecordList(3);
            }
        });
        m_unionReward.onValueChanged.AddListener(delegate
        {
            if (m_unionReward.isOn) moduleUnion.GetRelaseInfo(0);
        });
        m_friendReward.onValueChanged.AddListener(delegate
        {
            if (m_friendReward.isOn) moduleUnion.GetRelaseInfo(1);
        });
        m_releaseBtn.onClick.RemoveAllListeners();
        m_recordBtn.onClick.RemoveAllListeners();
        m_releaseBtn.onClick.AddListener(delegate
        {
            m_canReturn = false;
            m_rewardPlane.gameObject.SetActive(false);
            m_releasePlane.gameObject.SetActive(true);
            m_releaseEquip.isOn = true;
        });
        m_recordBtn.onClick.AddListener(delegate
        {
            m_canReturn = false;
            m_rewardPlane.gameObject.SetActive(false);
            m_recordPlane.gameObject.SetActive(true);
            m_recordGive.isOn = true;
        });

        m_rewardData = new DataSource<PReleaseReward>(null, GetComponent<ScrollView>("reward_panel/panel"), SetRewardData);
        m_recordData = new DataSource<PReleaseRecord>(null, GetComponent<ScrollView>("record_panel/record/recordScroll"), SetRecordData);
        m_releaseData = new DataSource<PropItemInfo>(null, GetComponent<ScrollView>("issue_panel/propPlane"), SetReleaseData, SetReleaseClick);

        SetText();
    }

    private void SetText()
    {
        Util.SetText(GetComponent<Text>("bottom_img/Text"), 631, 0);
        Util.SetText(GetComponent<Text>("issue_panel/checkBox/equip/Text"), 631, 1);
        Util.SetText(GetComponent<Text>("issue_panel/checkBox/rune/Text"), 631, 2);
        Util.SetText(GetComponent<Text>("issue_panel/checkBox/other/Text"), 631, 3);
        Util.SetText(GetComponent<Text>("record_panel/checkBox/give/Text"), 631, 4);
        Util.SetText(GetComponent<Text>("record_panel/checkBox/give/xz/give"), 631, 4);
        Util.SetText(GetComponent<Text>("record_panel/checkBox/revice/Text"), 631, 5);
        Util.SetText(GetComponent<Text>("record_panel/checkBox/revice/xz/revice"), 631, 5);
        Util.SetText(GetComponent<Text>("record_panel/record/checkBox/union/Text"), 631, 6);
        Util.SetText(GetComponent<Text>("record_panel/record/checkBox/union/xz/union"), 631, 6);
        Util.SetText(GetComponent<Text>("record_panel/record/checkBox/friend/Text"), 631, 7);
        Util.SetText(GetComponent<Text>("record_panel/record/checkBox/friend/xz/friend"), 631, 7);
        Util.SetText(GetComponent<Text>("reward_panel/checkBox/union/Text"), 631, 6);
        Util.SetText(GetComponent<Text>("reward_panel/checkBox/union/xz/union"), 631, 6);
        Util.SetText(GetComponent<Text>("reward_panel/checkBox/friend/Text"), 631, 7);
        Util.SetText(GetComponent<Text>("reward_panel/checkBox/friend/xz/friend"), 631, 7);
        Util.SetText(GetComponent<Text>("reward_panel/issue/Text"), 631, 8);
        Util.SetText(GetComponent<Text>("reward_panel/record/Text"), 631, 9);
        Util.SetText(GetComponent<Text>("issue_panel/tip_txt"), 631, 43);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleGlobal.ShowGlobalLayerDefault();
        m_rewardPlane.gameObject.SetActive(true);
        m_releasePlane.gameObject.SetActive(false);
        m_recordPlane.gameObject.SetActive(false);
        m_unionReward.isOn = true;
    }

    /// <summary>
    /// share something
    /// </summary>
    private IEnumerator Share(int x, int y, int width, int height, string text)
    {
        yield return new WaitForEndOfFrame();
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(x, y, width, height), 0, 0);
        tex.Apply();
        string img_file = Util.SaveFile(LocalFilePath.SCREENSHOT + "/ScreentShot.png", tex.EncodeToPNG(), false, true);
        GameObject.Destroy(tex);
        SDKManager.ShareImage(PlatformType.WeChat, img_file, text, (i, r) => { if (r == 1) SendGetShareReward(); }, ContentType.Webpage);
    }

    private void SetRewardData(RectTransform rt, PReleaseReward info)
    {
        if (info == null) return;

        FriendPrecast pracest = rt.gameObject.GetComponentDefault<FriendPrecast>();
        PPlayerInfo player = moduleUnion.PlayerInfo(info.playerId);
        if (player == null) return;
        pracest.DelayAddData(player);

        var prop = ConfigManager.Get<PropItemInfo>(info.itemTypeId);
        if (prop == null) return;

        var item = rt.Find("item");
        var itemBtn = item.GetComponentDefault<Button>();
        var have = rt.Find("count/Text").GetComponent<Text>();
        var slider = rt.Find("count/Slider/bg/Fill").GetComponent<Image>();
        var now = rt.Find("count/Slider/value").GetComponent<Text>();
        var need = rt.Find("count/Slider/need").GetComponent<Text>();
        var helpBtn = rt.Find("help_btn").GetComponent<Button>();
        var giveBtn = rt.Find("giveBtn").GetComponent<Button>();
        var lack = rt.Find("lack_txt").GetComponent<Text>();
        var over = rt.Find("over_txt").GetComponent<Text>();

        Transform icon = rt.Find("item/icon");
        Util.SetItemInfoSimple(item, prop);
        var selfhave = moduleCangku.GetItemCount(info.itemTypeId);
        Util.SetText(have, string.Format(ConfigText.GetDefalutString(631, 10), selfhave));
        Util.SetText(now, info.receivedNum.ToString());
        Util.SetText(need, prop.rewardnum.ToString());
        slider.fillAmount = (float)info.receivedNum / (float)prop.rewardnum;
        if (info.receivedNum > prop.rewardnum) slider.fillAmount = 1;
        Util.SetText(lack, 631, 11);
        Util.SetText(over, 631, 12);
        Util.SetText(rt.Find("giveBtn/Text").GetComponent<Text>(), 631, 13);
        Util.SetText(rt.Find("help_btn/Text").GetComponent<Text>(), 631, 14);
        helpBtn.SafeSetActive(info.playerId == modulePlayer.id_ && info.receivedNum < prop.rewardnum);
        giveBtn.SafeSetActive(info.playerId != modulePlayer.id_ && info.receivedNum < prop.rewardnum && selfhave > 0);
        lack.gameObject.SetActive(info.playerId != modulePlayer.id_ && info.receivedNum < prop.rewardnum && selfhave <= 0);
        over.gameObject.SetActive(info.receivedNum == prop.rewardnum);

        var title = rt.Find("isGuildMember/guildMember_Txt").GetComponent<Text>();
        var unionInfo = moduleUnion.m_unionPlayer.Find(a => a.info?.roleId == info.playerId);
        if (unionInfo != null) Util.SetText(title, 631, (unionInfo.title + 30));

        itemBtn.onClick.RemoveAllListeners();
        itemBtn.onClick.AddListener(delegate
        {
            moduleGlobal.UpdateGlobalTip(info.itemTypeId, true);
        });

        helpBtn.onClick.RemoveAllListeners();
        helpBtn.onClick.AddListener(delegate
        {
            var imagePath = PATH_SHARE_REWARD + "/" + prop.icon + ".png";
            var shareText = Util.Format(Util.GetString(631, 37), prop.itemName, prop.rewardnum);
            Vector3[] cornors = new Vector3[4];
            icon.GetComponent<RectTransform>().GetWorldCorners(cornors);
            if (UIManager.canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                for (int idx = 0; idx < cornors.Length; ++idx)
                {
                    cornors[idx] = UIManager.canvas.worldCamera.WorldToScreenPoint(cornors[idx]);
                }
            }
            int x = (int)cornors[0].x;
            int y = (int)cornors[0].y;
            int width = Mathf.CeilToInt((cornors[3].x - cornors[0].x));
            int height = Mathf.CeilToInt((int)(cornors[1].y - cornors[0].y));
            StartCoroutine(Share(x, y, width, height, shareText));
        });
        giveBtn.onClick.RemoveAllListeners();
        giveBtn.onClick.AddListener(delegate
        {
            var type = 0;
            if (m_friendReward.isOn) type = 1;
            moduleUnion.GivePlayerItem(type, info.playerId, info.itemTypeId, info.leagueID);
        });
    }

    protected override void OnClose()
    {
        StopCoroutine("Share");
    }

    void SendGetShareReward()
    {
        moduleGlobal.SendGetShareAward();
    }

    private void SetRecordData(RectTransform rt, PReleaseRecord info)
    {
        if (info == null || info?.playerInfo == null) return;
        FriendPrecast pracest = rt.gameObject.GetComponentDefault<FriendPrecast>();
        pracest.DelayAddData(info.playerInfo, -1, 1);

        var prop = ConfigManager.Get<PropItemInfo>(info.itemTypeId);
        if (prop == null) return;
        
        var item = rt.Find("item");
        Util.SetItemInfoSimple(item, prop);
        var timeTxt = rt.Find("record_txt").GetComponent<Text>();
        SetTimeTxt(info.time, timeTxt);
    }

    private void SetTimeTxt(int recordTime, Text timeTxt)
    {
        DateTime old = Util.GetDateTime(recordTime);
        DateTime now = Util.GetServerLocalTime();
        TimeSpan span = now - old;
        if (span.Days < 0)
        {
            Logger.LogError("Window_Unionreward record info time {0}", old);
            return;
        }

        if (span.Days > 30) Util.SetText(timeTxt, ConfigText.GetDefalutString(631, 40));
        else if (span.Days > 0) Util.SetText(timeTxt, ConfigText.GetDefalutString(631, 15), span.Days);
        else if (span.Days == 0 && span.Hours > 0) Util.SetText(timeTxt, ConfigText.GetDefalutString(631, 41), span.Hours);
        else if (span.Days == 0 && span.Hours == 0) Util.SetText(timeTxt, ConfigText.GetDefalutString(631, 42), span.Minutes);
    }

    private void SetReleaseData(RectTransform rt, PropItemInfo info)
    {
        if (info == null) return;
        Util.SetItemInfoSimple(rt, info);
        var now= rt.Find("slider/Text").GetComponent<Text>();
        var all= rt.Find("slider/Text (2)").GetComponent<Text>();
        var have = moduleCangku.GetItemCount(info.ID);
        Util.SetText(now, have.ToString());
        var PropInfo = ConfigManager.Get<Compound>(info.compose);
        if (PropInfo == null) return;
        Util.SetText(all, PropInfo.sourceNum.ToString());
        var fill = rt.Find("slider/fill").GetComponent<Image>();
        fill.fillAmount = (float)have / (float)PropInfo.sourceNum;
        if (have > PropInfo.sourceNum) fill.fillAmount = 1;
    }
    private void SetReleaseClick(RectTransform rt, PropItemInfo info)
    {
        if (info == null) return;
        if (moduleUnion.ReleaseTime == 0)
        {
            moduleGlobal.ShowMessage(631, 18);
            return;
        }
        Window_Alert.ShowAlert(string.Format(ConfigText.GetDefalutString(631, 19), info.itemName, info.rewardnum), true, true, true, () =>
        {
            moduleUnion.ReleaseSelfTask(info.ID);
        }, null, "", "");
    }

    private void SetRecordList(int type)
    {
        if (!m_recordPlane.gameObject.activeInHierarchy) return;
        var have = moduleUnion.RecordType.Exists(a => a == type);
        if (have)
        {
            if (type == 0 && m_recordUnion.isOn && m_recordRecived.isOn) ShowRecordPlane(moduleUnion.UnionRecived);
            else if (type == 1 && m_recordFriend.isOn && m_recordRecived.isOn) ShowRecordPlane(moduleUnion.FriendRecived);
            else if (type == 2 && m_recordUnion.isOn && m_recordGive.isOn) ShowRecordPlane(moduleUnion.UnionGive);
            else if (type == 3 && m_recordFriend.isOn && m_recordGive.isOn) ShowRecordPlane(moduleUnion.FriendGive);
        }
        else moduleUnion.GetRelaseRecord(type);
    }

    private void ShowRecordPlane(List<PReleaseRecord> record)
    {
        m_recordInfoPlane.SafeSetActive(record.Count > 0);
        m_recordNothing.SafeSetActive(record.Count == 0);
        if (record.Count > 0) m_recordData.SetItems(record);
    }
    private void ShowRewardPlane(List<PReleaseReward> reward)
    {
        m_rewardInfoPlane.SafeSetActive(reward.Count > 0);
        m_rewardNothing.SafeSetActive(reward.Count == 0);
        if (reward.Count > 0) m_rewardData.SetItems(reward);
    }
    
    void _ME(ModuleEvent<Module_Union> e)
    {
        if (!actived) return;
        switch (e.moduleEvent)
        {
            case Module_Union.EventUnionClamisInfo:
                if (m_unionReward.isOn) ShowRewardPlane(moduleUnion.UnionClaimsInfo);
                else if (m_friendReward.isOn) ShowRewardPlane(moduleUnion.FriendClaimsInfo);
                break;
            case Module_Union.EventUnionClamisGive:
                if (m_unionReward.isOn) ShowRewardPlane(moduleUnion.UnionClaimsInfo);
                else if (m_friendReward.isOn) ShowRewardPlane(moduleUnion.FriendClaimsInfo);
                break;
            case Module_Union.EventUnionRelaseSelf:
                //自己任务发布成功
                ReturnInfo();
                break;
            case Module_Union.EventUnionClamisRemove:
                var mType = Util.Parse<int>(e.param1.ToString());
                var playerInfo = (PReleaseReward)e.param2;
                if (mType == 0) RecivedRemove(playerInfo, mType, moduleUnion.UnionClaimsInfo);
                else if (mType == 1) RecivedRemove(playerInfo, mType, moduleUnion.FriendClaimsInfo);
                break;
            case Module_Union.EventUnionClamisRecord:
                var rType = Util.Parse<int>(e.param1.ToString());
                SetRecordList(rType);
                break;
            case Module_Union.EventUnionClamisSelf:
                //刷新自己信息
                if (m_unionReward.isOn) m_rewardData.UpdateItem(0);
                break;

        }
    }

    private void RecivedRemove(PReleaseReward playerInfo,int mType,List<PReleaseReward> record)
    {
        if (playerInfo == null) return;
        if ((m_unionReward.isOn && mType == 0) || (m_friendReward.isOn && mType == 1))
            m_rewardData.RemoveItem(playerInfo);

        m_recordInfoPlane.SafeSetActive(record.Count > 0);
        m_recordNothing.SafeSetActive(record.Count == 0);
    }

    private void ReturnInfo()
    {
        m_canReturn = true;
        m_friendReward.isOn = false;
        m_unionReward.isOn = false;
        m_rewardPlane.gameObject.SetActive(true);
        m_releasePlane.gameObject.SetActive(false);
        m_recordPlane.gameObject.SetActive(false);
        m_unionReward.isOn = true;
    }

    protected override void OnReturn()
    {
        if (!m_canReturn) ReturnInfo();
        else base.OnReturn();
    }
}