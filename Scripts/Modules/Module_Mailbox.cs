/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-10-17
 * 
 ***************************************************************************************************/

using System.Collections.Generic;

public class Module_Mailbox : Module<Module_Mailbox>
{
    public const string AllMailsEvent = "AllMailsEvent";//所有邮件
    public const string NewMailEvent = "NewMailEvent";//新邮件
    public const string NoMailsEvent = "NoMailsEvent";//没有邮件
    public const string GetAttachmentSuccessEvent = "GetAttachmentSuccessEvent";//获取一个邮件的附件
    public const string GetAttachmentFailedEvent = "GetAttachmentFailedEvent";//领取失败
    public const string ReadMailEvent = "ReadMailEvent";//已读

    public List<PMail> allMails { get { return m_allMails; } }
    private List<PMail> m_allMails = new List<PMail>();

    public PMail currentMail { get; set; }//当前邮件
    public int isGetAll { get; set; } = 0;//0是默认状态 1是单个领取 2是全部领取

    private List<ulong> mailsId = new List<ulong>();//获取所有附件

    public Dictionary<ulong, bool> alreadySendRead { get { return m_alreadySendRead; } }
    private Dictionary<ulong, bool> m_alreadySendRead = new Dictionary<ulong, bool>();

    public List<PItem2> allNoGetRewards { get { return m_allNoGetRewards; } }
    private List<PItem2> m_allNoGetRewards = new List<PItem2>();

    public int noGetMailNum { get; private set; }

    public bool mailRead { get; set; }
    public bool NeedNotice
    {
        get
        {
            if (mailRead) return false;
            var isNoRead = m_allMails.Find(p => (p.attachment == null || p.attachment.Length < 1) && p.isRead == 0);
            var isNoGet = m_allMails.Find(p => (p.attachment != null && p.attachment.Length > 0) && p.isGet == 0);
            return isNoRead == null && isNoGet == null ? false : true;
        }
    }

    private void OnAddRewards()
    {
        if (m_allMails == null || m_allMails.Count < 1) return;

        m_allNoGetRewards.Clear();
        noGetMailNum = 0;
        for (int i = 0; i < m_allMails.Count; i++)
        {
            if (m_allMails[i].attachment == null || m_allMails[i].attachment.Length < 1 || m_allMails[i].isGet == 1) continue;

            var items = m_allMails[i].attachment;
            for (int k = 0; k < items.Length; k++)
            {
                var prop = ConfigManager.Get<PropItemInfo>(items[k].itemTypeId);
                if (!prop || !prop.IsValidVocation(modulePlayer.proto)) continue;

                var m = m_allNoGetRewards.Find(p => p.itemTypeId == items[k].itemTypeId);
                if (m == null)
                {
                    PItem2 newItme = null;
                    items[k].CopyTo(ref newItme);
                    m_allNoGetRewards.Add(newItme);
                }
                else m.num += items[k].num;
            }
            noGetMailNum++;
        }

        m_allNoGetRewards.Sort((a, b) =>
        {
            var propA = ConfigManager.Get<PropItemInfo>(a.itemTypeId);
            var propB = ConfigManager.Get<PropItemInfo>(b.itemTypeId);
            if (propA == null || propB == null) return -1;

            int result = propB.quality.CompareTo(propA.quality);
            if (result == 0)
                result = a.itemTypeId.CompareTo(b.itemTypeId);
            return result;
        });
    }

    public void OnDeleteRewards(bool isAll, ulong _mailId = 0)
    {
        if (isAll)
        {
            var mail = m_allMails.Find(p => p.mailId == _mailId);
            if (mail == null)
            {
                Logger.LogError("Can Not Find Mail In All MailList Just Now,MailID=[{0}]", _mailId);
                return;
            }

            if (mail.attachment == null || mail.attachment.Length < 1)
            {
                Logger.LogError("This MaiL Have No Rewards,But You Send GetReward Just Now! mailID=[{0}]", _mailId);
                return;
            }

            for (int i = 0; i < mail.attachment.Length; i++)
            {
                var prop = ConfigManager.Get<PropItemInfo>(mail.attachment[i].itemTypeId);
                if (!prop || !prop.IsValidVocation(modulePlayer.proto)) continue;

                var m = m_allNoGetRewards.Find(p => p.itemTypeId == mail.attachment[i].itemTypeId);
                if (m == null) continue;

                m.num -= mail.attachment[i].num;
                if (m.num <= 0) m_allNoGetRewards.Remove(m);
            }
            noGetMailNum--;
        }
        else
        {
            m_allNoGetRewards.Clear();
            noGetMailNum = 0;
        }
    }

    //获取所有邮件
    public void SendGetAllMail()
    {
        CsSystemMailList allmail = PacketObject.Create<CsSystemMailList>();
        session.Send(allmail);
    }

    void _Packet(ScSystemMailList p)
    {
        PMail[] mails = null;
        p.mailList.CopyTo(ref mails);

        if (mails.Length > 0)
        {
            GetAllMails(mails);
        }
    }

    private void GetAllMails(PMail[] mails)
    {
        m_allMails.Clear();
        for (int i = 0; i < mails.Length; i++)
        {
            if (i >= 100) break;
            var mail = m_allMails.Find(p => p.mailId == mails[i].mailId);
            if (mail == null) m_allMails.Add(mails[i]);
        }
        SortList();
    }

    void _Packet(ScSystemMailNew p)
    {
        PMail mail = null;
        p.mail.CopyTo(ref mail);

        var _mail = m_allMails.Find(o => o.mailId == mail.mailId);
        if (_mail == null && m_allMails.Count < 100)
            m_allMails.Add(mail);

        SortList();
    }

    private void SortList()
    {
        m_allMails.Sort((a, b) =>
        {
            int result = b.time.CompareTo(a.time);

            return result;
        });

        OnAddRewards();
    }

    public void GetTargetMailsList()
    {
        if (m_allMails.Count > 0)
        {
            m_allMails[0].isRead = 1;
            currentMail = m_allMails[0];
            DispatchModuleEvent(AllMailsEvent);
        }
        else
            DispatchModuleEvent(NoMailsEvent);
    }

    //已读
    public void SendReadMail(ulong id)
    {
        CsSystemMailRead read = PacketObject.Create<CsSystemMailRead>();
        ulong[] ids = new ulong[] { id };
        read.mailIds = ids;
        session.Send(read);

        if (currentMail == null) return;
        for (int i = 0; i < m_allMails.Count; i++)
        {
            if (m_allMails[i].mailId == currentMail.mailId)
            {
                m_allMails[i].isRead = 1;
            }
        }
        DispatchModuleEvent(ReadMailEvent);
    }

    //领取附件一个
    public void SendGetOneAttachment(ulong id)
    {
        CsSystemMailReward getOne = PacketObject.Create<CsSystemMailReward>();
        ulong[] ids = new ulong[] { id };
        getOne.mailIds = ids;
        session.Send(getOne);

        isGetAll = 1;
    }

    void _Packet(ScSystemMailReward p)
    {
        if (p.result == 0)
        {
            DispatchModuleEvent(GetAttachmentSuccessEvent, p.mailId);
        }
        else
            DispatchModuleEvent(GetAttachmentFailedEvent, p.result);
    }

    //获取所有附件
    public void SendGetAllAttachment()
    {
        mailsId = GetAllHaveAttachment();
        CsSystemMailReward getAllAttachment = PacketObject.Create<CsSystemMailReward>();
        getAllAttachment.mailIds = mailsId.ToArray();
        session.Send(getAllAttachment);

        isGetAll = 2;
    }

    private List<ulong> GetAllHaveAttachment()
    {
        List<ulong> mailsId = new List<ulong>();
        for (int i = 0; i < m_allMails.Count; i++)
        {
            if (m_allMails[i].isGet == 0)
            {
                mailsId.Add(m_allMails[i].mailId);
            }
        }
        return mailsId;
    }

    //删除没有附件的已读邮件和有附件已领取的邮件
    public void SendDeleteMails()
    {
        List<ulong> mails = new List<ulong>();
        for (int i = 0; i < m_allMails.Count; i++)
        {
            bool haveAttachment = m_allMails[i].attachment != null && m_allMails[i].attachment.Length >= 1 && m_allMails[i].isGet == 1;
            bool noAttachment = (m_allMails[i].attachment == null || m_allMails[i].attachment.Length < 1) && m_allMails[i].isRead == 1;

            if (haveAttachment || noAttachment)
                mails.Add(m_allMails[i].mailId);
        }
        if (mails.Count < 1)
        {
            var info = ConfigManager.Get<ConfigText>((int)TextForMatType.MailUIText);
            moduleGlobal.ShowMessage(info.text[2]);
            return;
        }

        CsSystemMailDel delete = PacketObject.Create<CsSystemMailDel>();
        delete.mailIds = mails.ToArray();
        session.Send(delete);
    }

    void _Packet(ScSystemMailDel p)
    {
        for (int i = 0; i < m_allMails.Count; i++)
        {
            if (m_allMails[i].mailId == p.mailId)
                m_allMails.Remove(m_allMails[i]);

            if (m_alreadySendRead.ContainsKey(p.mailId)) m_alreadySendRead.Remove(p.mailId);
        }

        if (m_allMails.Count > 0)
        {
            m_allMails[0].isRead = 1;
            currentMail = m_allMails[0];
            DispatchModuleEvent(AllMailsEvent);
        }
        else
            DispatchModuleEvent(NoMailsEvent);
    }

    public void AddAlreadySendDic(ulong id)
    {
        if (m_alreadySendRead == null)
            m_alreadySendRead.Add(id, true);
        else
        {
            if (m_alreadySendRead.ContainsKey(id)) return;
            else m_alreadySendRead.Add(id, true);
        }
    }

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        m_allMails.Clear();
        currentMail = null;
        isGetAll = 0;
        mailsId.Clear();
        m_alreadySendRead.Clear();
        m_allNoGetRewards.Clear();
        noGetMailNum = 0;
        mailRead = false;
    }
}
