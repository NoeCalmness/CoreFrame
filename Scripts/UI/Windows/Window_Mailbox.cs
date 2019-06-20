/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-10-17
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_Mailbox : Window
{
    //左
    private int selectIndex;
    private Image nothing;
    //中
    private Text mailTitle;
    private Text mailTitleTime;
    private Text mailSender;
    private Text mailContent;
    private Transform awardGoods;
    private GameObject itemObj;
    private Button getBtn;

    private RectTransform deleteTip;
    private Button sureBtn;
    private Button unsureBtn;

    private Transform noticeTip;
    private Button getSureBtn;
    private Button noGetBtn;
    private Button closeBtn;
    private Text getAllNum;
    DataSource<PItem2> data;

    //左下
    private Button getAll;
    private Button deleteRead;
    private Text mailNumber;

    private ScrollView view;
    private DataSource<PMail> dataSource;

    protected override void OnOpen()
    {
        nothing = GetComponent<Image>("mail/left/nothing");

        mailTitle = GetComponent<Text>("mail/center/title/title_text");
        mailTitleTime = GetComponent<Text>("mail/center/title/time_text");
        mailSender = GetComponent<Text>("mail/center/title/sender");
        mailContent = GetComponent<Text>("mail/center/content/Text");
        awardGoods = GetComponent<Transform>("mail/center/goods/content");
        itemObj = GetComponent<RectTransform>("mail/center/goods/item").gameObject;
        getBtn = GetComponent<Button>("mail/center/get");

        deleteTip = GetComponent<RectTransform>("mail/center/deleteTip");
        sureBtn = GetComponent<Button>("mail/center/deleteTip/sure");
        unsureBtn = GetComponent<Button>("mail/center/deleteTip/unsure");
        getAll = GetComponent<Button>("mail/bottomLeft/getAll");
        deleteRead = GetComponent<Button>("mail/bottomLeft/deleteRead");
        mailNumber = GetComponent<Text>("mail/bottomLeft/readText/Text");

        getAll.onClick.RemoveAllListeners();
        getAll.onClick.AddListener(OnClickGetAllBtn);
        deleteRead.onClick.RemoveAllListeners();
        deleteRead.onClick.AddListener(() =>
        {
            deleteTip.SafeSetActive(true);
            sureBtn.onClick.RemoveAllListeners();
            sureBtn.onClick.AddListener(OnDeleteAllRead);
            unsureBtn.onClick.RemoveAllListeners();
            unsureBtn.onClick.AddListener(() => { deleteTip.SafeSetActive(false); });
        });

        view = GetComponent<ScrollView>("mail/left/scrollView");
        dataSource = new DataSource<PMail>(null, view, OnSetItemInfo, OnMailClick);

        noticeTip = GetComponent<Transform>("mail/center/noticeTip");
        getSureBtn = GetComponent<Button>("mail/center/noticeTip/bottom/getBtn");
        noGetBtn = GetComponent<Button>("mail/center/noticeTip/bottom/cancelBtn");
        closeBtn = GetComponent<Button>("mail/center/noticeTip/top/global_tip_button");
        getAllNum = GetComponent<Text>("mail/center/noticeTip/Text");

        getSureBtn.onClick.RemoveAllListeners();
        getSureBtn.onClick.AddListener(() => moduleMailbox.SendGetAllAttachment());
        noGetBtn.onClick.RemoveAllListeners();
        noGetBtn.onClick.AddListener(() => noticeTip.SafeSetActive(false));
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() => noticeTip.SafeSetActive(false));

        data = new DataSource<PItem2>(null, GetComponent<ScrollView>("mail/center/noticeTip/itemList/scrollView"), OnSetItems, null);

        IniteText();
    }

    private void OnClickGetAllBtn()
    {
        noticeTip.SafeSetActive(true);
        Util.SetText(getAllNum, 206, 17, moduleMailbox.noGetMailNum);
        data.SetItems(moduleMailbox.allNoGetRewards);
    }

    private void OnSetItems(RectTransform node, PItem2 data)
    {
        if (data == null) return;
        var prop = ConfigManager.Get<PropItemInfo>(data.itemTypeId);
        if (prop == null)
        {
            Logger.LogError("propItemInfo do not have prop id={0}", data.itemTypeId);
            return;
        }
        if (!prop.IsValidVocation(modulePlayer.proto)) return;

        if (prop.itemType == PropType.Rune) Util.SetItemInfo(node, prop, data.level, (int)data.num, false);
        else Util.SetItemInfo(node, prop, 0, (int)data.num, false);
    }

    private void IniteText()
    {
        ConfigText mailText = ConfigManager.Get<ConfigText>((int)TextForMatType.MailUIText);
        Util.SetText(GetComponent<Text>("mail/center/get/Text"), mailText[4]);
        Util.SetText(GetComponent<Text>("mail/bottomLeft/getAll/Text"), mailText[5]);
        Util.SetText(GetComponent<Text>("mail/bottomLeft/deleteRead/Text"), mailText[6]);
        Util.SetText(GetComponent<Text>("mail/center/deleteTip/Text"), mailText[8]);
        Util.SetText(GetComponent<Text>("mail/left/nothing/Text"), mailText[9]);
        Util.SetText(GetComponent<Text>("title"), mailText[10]);
        Util.SetText(GetComponent<Text>("mail/left/scrollView/template/0/yidu/text"), mailText[11]);
        Util.SetText(GetComponent<Text>("mail/left/scrollView/template/0/weidu/text"), mailText[12]);
        Util.SetText(GetComponent<Text>("mail/center/noticeTip/top/equipinfo"), mailText[14]);
        Util.SetText(GetComponent<Text>("mail/center/noticeTip/bottom/cancelBtn/Text"), mailText[15]);
        Util.SetText(GetComponent<Text>("mail/center/noticeTip/bottom/getBtn/Text"), mailText[16]);

        ConfigText publicText = ConfigManager.Get<ConfigText>((int)TextForMatType.PublicUIText);
        Util.SetText(GetComponent<Text>("mail/center/deleteTip/sure/Text"), publicText[1]);
        Util.SetText(GetComponent<Text>("mail/center/deleteTip/cancel/Text"), publicText[2]);
        Util.SetText(GetComponent<Text>("mail/center/deleteTip/bg/equip_prop/top/equipinfo"), publicText[6]);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleMailbox.GetTargetMailsList();
        moduleGlobal.ShowGlobalLayerDefault(1, false);
        moduleHome.UpdateIconState(HomeIcons.Mail, false);
        view.progress = 0;
        moduleMailbox.mailRead = true;
    }

    private void OnSetItemInfo(RectTransform node, PMail data)
    {
        if (data == null) return;

        MailItem item = new MailItem(node);
        item.RefreshUi(data);
        Transform select = node.Find("selectImage");
        if (select && moduleMailbox.currentMail != null) select.SafeSetActive(data.mailId == moduleMailbox.currentMail.mailId);
    }

    private void OnMailClick(RectTransform tr, PMail data)
    {
        moduleMailbox.currentMail = data;

        int lastSelectIndex = selectIndex;
        selectIndex = moduleMailbox.allMails.FindIndex((p) => p.mailId == data.mailId);
        if (lastSelectIndex != selectIndex) dataSource.SetItem(moduleMailbox.allMails[lastSelectIndex], lastSelectIndex);
        dataSource.SetItem(data, selectIndex);

        OnClick(data);
    }

    private void OnClick(PMail data)
    {
        if (data.type == 201)
        {
            var info = ConfigManager.Get<ConfigText>(data.type);
            if (info != null)
            {
                mailTitle.text = info.text[1];
                mailSender.text = info.text[0];
                //显示内容
                if (data.type == (byte)TextForMatType.LabyrinthMailText)
                {
                    string[] strs = data.content.Split('-');//0:迷宫ID 1:在迷宫的几层  2:晋级情况(1:晋级 2:保级 3:降级) 3:小组排名
                    var labInfo = ConfigManager.Get<LabyrinthInfo>(Util.Parse<int>(strs[0]));
                    if (labInfo == null)
                    {
                        Logger.LogError("LabyrinthInfo config id=={0} is null", strs[0]);
                        return;
                    }

                    var allLabInfo = ConfigManager.GetAll<LabyrinthInfo>();
                    int maxId = allLabInfo?[allLabInfo.Count - 1].ID ?? 0;
                    string maxStr = string.Empty;
                    if (strs[0].Equals(maxId.ToString()))
                        maxStr = info.text[10];
                    else
                    {
                        var nextLabInfo = ConfigManager.Get<LabyrinthInfo>(Util.Parse<int>(strs[0]) + 1);
                        if (nextLabInfo != null)
                            maxStr = Util.Format(info.text[7], nextLabInfo.labyrinthName);
                    }

                    int minId = allLabInfo?[0].ID ?? 0;
                    string minStr = string.Empty;
                    if (strs[0].Equals(minId.ToString()))
                        minStr = Util.Format(info.text[9], labInfo.labyrinthName);
                    else
                    {
                        var lastLabyInfo = ConfigManager.Get<LabyrinthInfo>(Util.Parse<int>(strs[0]) - 1);
                        if (lastLabyInfo != null)
                            minStr = Util.Format(info.text[9], lastLabyInfo.labyrinthName);
                    }

                    string str = strs[2] == null ? "" : strs[2] == "1" ? maxStr : strs[2] == "2" ? info.text[8] : minStr;

                    mailContent.text = Util.Format(info.text[5].Replace("\\n", "\n").Replace("\\t", "\t"), labInfo.labyrinthName, strs[1], strs[3], str);
                }
            }
        }
        else if (data.type >= 204 && data.type <= 207)
        {
            var info = ConfigManager.Get<ConfigText>(data.type - 204 + 33); //映射多语言文本
            if (info != null)
            {
                Util.SetText(mailTitle, info.text[0]);
                Util.SetText(mailSender, info.text[1]);
                switch (data.type)
                {
                    case 204:
                    Util.SetText(mailContent, Util.Format(info.text[2], data.content));
                        break;
                    case 205:
                    case 206:
                        Util.SetText(mailContent, Util.Format(info.text[2], Module_FactionBattle.FactionName((Module_FactionBattle.Faction)Util.Parse<int>(data.content))));
                        break;
                    case 207:
                        Util.SetText(mailContent, Util.Format(info.text[2], Module_FactionBattle.GetKillString(Util.Parse<int>(data.content))));
                        break;
                }
            }
        }
        else
        {
            mailTitle.text = data.title;
            mailContent.text = data.content;
            string text = ConfigText.GetDefalutString(TextForMatType.PublicUIText, 32);
            if (!string.IsNullOrEmpty(text))
            {
                string str = Util.Format(text, data.name);
                mailSender.text = str;
            }
        }
        DateTime localdt = Util.GetDateTime(data.time);
        Util.SetText(mailTitleTime, (int)TextForMatType.LabyrinthMailText, 4, localdt.Year, localdt.Month, localdt.Day);

        if (data.isGet == 0 && data.attachment != null && data.attachment.Length > 0)//有附件并且未领取的情况
        {
            getBtn.SafeSetActive(true);
            awardGoods.SafeSetActive(true);
            if (awardGoods.childCount > 0)
            {
                for (int i = 0; i < awardGoods.childCount; i++)
                    awardGoods.GetChild(i).SafeSetActive(false);
            }

            for (int i = 0; i < data.attachment.Length; i++)
            {
                var prop = ConfigManager.Get<PropItemInfo>(data.attachment[i].itemTypeId);
                if (!prop || !prop.IsValidVocation(modulePlayer.proto)) continue;

                Transform tran = awardGoods.childCount > i ? awardGoods.GetChild(i) : null;
                if (tran == null) tran = awardGoods.AddNewChild(itemObj);

                tran.SafeSetActive(true);


                Util.SetItemInfo(tran, prop, 0, (int)data.attachment[i].num, false);
                var btn = tran?.GetComponentDefault<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => moduleGlobal.UpdateGlobalTip((ushort)prop.ID, true, false));
            }

            getBtn.onClick.RemoveAllListeners();
            getBtn.onClick.AddListener(() => { OnGetGoods(moduleMailbox.currentMail); });
        }
        else //没有附件的情况
        {
            awardGoods.SafeSetActive(false);
            getBtn.SafeSetActive(false);
        }

        if (moduleMailbox.alreadySendRead != null && !moduleMailbox.alreadySendRead.ContainsKey(data.mailId))
        {
            moduleMailbox.AddAlreadySendDic(data.mailId);
            moduleMailbox.SendReadMail(data.mailId);
        }

        SetButtonEnable();
    }

    private void OnDeleteAllRead()
    {
        moduleMailbox.SendDeleteMails();
        deleteTip.SafeSetActive(false);
    }

    private void OnGetGoods(PMail obj)
    {
        moduleMailbox.SendGetOneAttachment(obj.mailId);
    }

    protected override void OnReturn()
    {
        moduleMailbox.currentMail = null;
        Hide();
    }

    void _ME(ModuleEvent<Module_Mailbox> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Mailbox.NoMailsEvent:
            {
                if (moduleMailbox.allMails != null && moduleMailbox.allMails.Count > 0) return;

                nothing.SafeSetActive(true);
                mailTitle.text = null;
                mailSender.text = null;
                mailTitleTime.text = null;
                mailContent.text = null;
                getAll.interactable = false;
                deleteRead.interactable = false;
                getBtn.SafeSetActive(false);
                mailNumber.text = "0/100";
                selectIndex = 0;
                dataSource.SetItems(null);
                SetButtonEnable();
                break;
            }
            case Module_Mailbox.AllMailsEvent:
            {
                selectIndex = 0;
                nothing.SafeSetActive(false);
                dataSource.SetItems(moduleMailbox.allMails);
                OnClick(moduleMailbox.allMails[0]);
                mailNumber.text = Util.Format("{0}/100", moduleMailbox.allMails.Count);
                SetButtonEnable();
                break;
            }
            case Module_Mailbox.ReadMailEvent: dataSource.UpdateItem(selectIndex); break;
            case Module_Mailbox.GetAttachmentSuccessEvent:
            {
                ulong id = (ulong)e.param1;
                if (moduleMailbox.isGetAll == 1)//不是全部领取
                {
                    //RefreshGetRewardPanel(moduleMailbox.currentMail.attachment);
                    List<PItem2> lists = new List<PItem2>();
                    lists.AddRange(moduleMailbox.currentMail.attachment);
                    string str = ConfigText.GetDefalutString(TextForMatType.MailUIText, 13);
                    Window_ItemTip.Show(str, lists);
                    moduleMailbox.OnDeleteRewards(true, id);
                    moduleMailbox.isGetAll = 0;
                }
                else if (moduleMailbox.isGetAll == 2)
                {
                    noticeTip.SafeSetActive(false);
                    moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.MailUIText, 0));
                    moduleMailbox.OnDeleteRewards(false);
                    moduleMailbox.isGetAll = 0;
                }

                for (int i = 0; i < moduleMailbox.allMails.Count; i++)
                {
                    if (moduleMailbox.allMails[i].mailId == id)
                    {
                        moduleMailbox.allMails[i].isRead = 1;
                        moduleMailbox.allMails[i].isGet = 1;
                    }
                }

                SetButtonEnable();

                getBtn.SafeSetActive(false);
                awardGoods.SafeSetActive(false);
                deleteRead.interactable = true;

                dataSource.UpdateItems();
                break;
            }
            case Module_Mailbox.GetAttachmentFailedEvent:
            {
                sbyte result = (sbyte)e.param1;
                switch (result)
                {
                    case 1:
                        moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.MailUIText, 2));
                        break;
                    case 2:
                        moduleGlobal.ShowMessage(ConfigText.GetDefalutString(TextForMatType.MailUIText, 3));
                        break;
                }
                getAll.interactable = true;
                break;
            }
            default: break;
        }
    }

    private void SetButtonEnable()
    {
        //全部获取按钮
        bool isContains = false;
        for (int i = 0; i < moduleMailbox.allMails.Count; i++)
        {
            if (moduleMailbox.allMails[i].attachment != null && moduleMailbox.allMails[i].attachment.Length > 0 && moduleMailbox.allMails[i].isGet == 0)
            {
                isContains = true;
                break;
            }
        }
        getAll.interactable = isContains;

        //删除已读按钮
        bool isRead = false;
        for (int i = 0; i < moduleMailbox.allMails.Count; i++)
        {
            bool haveAttachment = moduleMailbox.allMails[i].attachment != null && moduleMailbox.allMails[i].attachment.Length >= 1 && moduleMailbox.allMails[i].isGet == 1;
            bool noAttachment = (moduleMailbox.allMails[i].attachment == null || moduleMailbox.allMails[i].attachment.Length < 1) && moduleMailbox.allMails[i].isRead == 1;

            if (haveAttachment || noAttachment)
            {
                isRead = true;
                break;
            }
        }
        deleteRead.interactable = isRead;
    }

    public class MailItem
    {
        private Transform alreadyGet;
        private Transform noGet;
        private Transform readTran;
        private Transform noReadTran;
        private Text title;
        private Text attachment;
        private Text remainingTime;

        public MailItem(Transform transform)
        {
            alreadyGet = transform.Find("yilingqv");
            noGet = transform.Find("weilingqv");
            readTran = transform.Find("yidu");
            noReadTran = transform.Find("weidu");
            title = transform.Find("title").GetComponent<Text>();
            attachment = transform.Find("attachment").GetComponent<Text>();
            remainingTime = transform.Find("remainingTime").GetComponent<Text>();
        }

        public void RefreshUi(PMail mail)
        {            
            if (mail.type > 200)
            {
                if (mail.type >= 204 && mail.type <= 207)
                {
                    var info = ConfigManager.Get<ConfigText>(mail.type - 204 + 33); //映射多语言文本
                    Util.SetText(title, info.text[0]);
                }
                else
                {
                    var info = ConfigManager.Get<ConfigText>(mail.type);
                    if (info != null)
                        title.text = info.text[1];
                }
            }
            else
                title.text = mail.title;

            //奖励为空 看未读已读显示 奖励有,看是否领取
            bool isShowAlready = (mail.attachment == null || mail.attachment.Length < 1) || (mail.attachment.Length > 0 && mail.isGet == 1);

            readTran.SafeSetActive(mail.isRead == 1);
            noReadTran.SafeSetActive(mail.isRead != 1);
            //已读的情况下 ,看是否领取了奖励来管理
            if (mail.isRead == 1)
            {
                alreadyGet.SafeSetActive(isShowAlready);
                noGet.SafeSetActive(!isShowAlready);
            }
            else
            {
                alreadyGet.SafeSetActive(false);
                noGet.SafeSetActive(true);
            }

            attachment.SafeSetActive(!isShowAlready);
            int num = 0;
            for (int i = 0; i < mail.attachment.Length; i++)
            {
                var prop = ConfigManager.Get<PropItemInfo>(mail.attachment[i].itemTypeId);
                if (!prop || !prop.IsValidVocation(modulePlayer.proto)) continue;

                num++;
            }

            Util.SetText(attachment, (int)TextForMatType.LabyrinthMailText, 2, num);//{0}个附件

            //剩余时间
            DateTime localdt = Util.GetDateTime(mail.time);
            DateTime now = Util.GetServerLocalTime();
            TimeSpan remaining = now - localdt;
            int day = moduleGlobal.system.mailDaysLimit - remaining.Days - 1;
            day = day < 0 ? 0 : day;
            int hours = 24 - remaining.Hours;
            hours = hours < 0 ? 0 : hours;
            Util.SetText(remainingTime, (int)TextForMatType.LabyrinthMailText, 3, day, 24 - remaining.Hours);
        }
    }
}
