/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-07-19
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

public class Module_Union : Module<Module_Union>
{
    public bool OpenBossWindow { get; set; } = false;

    public bool EnterBossTask { get; set; } = false;

    #region  union

    public const string EventUnionAddSucced = "EventUnionAddSucced";
    public const string EventUnionAddFailed = "EventUnionAddFailed";

    public const string EventUnionCreateSucced = "EventUnionCreateSucced";
    /// <summary>
    /// ˢ�¹����б�
    /// </summary>
    public const string EventUnionRefreshList = "EventUnionRefreshList";
    public const string EventUnionSelectResult = "EventUnionSelectResult";
    /// <summary>
    /// �������
    /// </summary>
    public const string EventUnionApplyNotice = "EventUnionApplyNotice";
    public const string EventUnionApplySolveResult = "EventUnionApplySolveResult";
    public const string EventUnionApplyOverdue = "EventUnionApplyOverdue";
    public const string EventUnionSelfExit = "EventUnionSelfExit";
    /// <summary>
    /// �߳������ɹ�
    /// </summary>
    public const string EventUnionRemoveSucced = "EventUnionRemoveSucced";
    /// <summary>
    /// ְλ���Ľ��
    /// </summary>
    public const string EventUnionChangeTitle = "EventUnionChangeTitle";
    /// <summary>
    /// ְλ������
    /// </summary>
    public const string EventUnionTitleBeChange = "EventUnionTitleBeChange";
    /// <summary>
    /// �����������
    /// </summary>
    public const string EventUnionSetLevel = "EventUnionMinLevel";
    public const string EventUnionNoticeChange = "EventUnionNoticeChange";//����ĸ�������
    public const string EventUnionPlayerState = "EventUnionPlayerState";//��Ա������
    public const string EventUnionLevelChange = "EventUnionLevelChange";//����ȼ�����
    /// <summary>
    /// ����ֵ����
    /// </summary>
    public const string EventUnionSentimntExpend = "EventUnionSentimntExpend";
    /// <summary>
    /// ��Ϣ������
    /// </summary>
    public const string EventUnionSysUpdate = "EventUnionSysUpdate";
    /// <summary>
    /// ����ֵ����
    /// </summary>
    public const string EventUnionSentimentUpdate = "EventUnionSentimentUpdate";
    /// <summary>
    /// ��ɢ����
    /// </summary>
    public const string EventUnionDissolution = "EventUnionDissolution";
    public const string EventUnionPlayerAdd = "EventUnionPlayerAdd";
    public const string EventUnionPlayerExit = "EventUnionPlayerExit";
    /// <summary>
    /// ���й������ˢ��
    /// </summary>
    public const string EventUnionInterglAll = "EventUnionInterglAll";
    /// <summary>
    /// �Լ��������ˢ��
    /// </summary>
    public const string EventUnionInterglSelf = "EventUnionInterglSelf";
    
    public PUnionInfo UnionBaseInfo { get { return m_unionBaseInfo; } }
    private PUnionInfo m_unionBaseInfo;//������Ϣ

    public ulong m_unionChatID { get; set; }
    private bool m_addChat = true;

    /// <summary>
    /// �����������Ĳ���
    /// </summary>
    public PPrice[] createUnionInfo { get; set; }
    /// <summary>
    /// �Ƿ��ڶ�������
    /// </summary>
    public bool m_inOtherPalne { get; set; }
    /// <summary>
    /// ���ĵ�����ֵ
    /// </summary>
    public long m_expendSentiment { get; set; }
    /// <summary>
    /// �������빫��ʣ�����
    /// </summary>
    public int m_lossTimes { get; set; }
    /// <summary>
    /// �����б�
    /// </summary>
    public List<PRefreshInfo> selectUnionList = new List<PRefreshInfo>();
    /// <summary>
    /// ����ˢ���б�
    /// </summary>
    public List<PRefreshInfo> m_refrshList = new List<PRefreshInfo>();
    /// <summary>
    /// �����Ա��Ϣ
    /// </summary>
    public List<PUnionPlayer> m_unionPlayer = new List<PUnionPlayer>();
    /// <summary>
    /// ���������б�
    /// </summary>
    public List<PPlayerInfo> m_unionApply = new List<PPlayerInfo>();
    /// <summary>
    /// ��������
    /// </summary>
    public Queue<ScChatRoomMessage> m_chatChatSys = new Queue<ScChatRoomMessage>();
    /// <summary>
    ///  0�᳤ 1 ���᳤2 ��ͨ -1���ڹ��� 
    /// </summary>
    public int inUnion { get; set; } = -1;
    /// <summary>
    /// ������������
    /// </summary>
    public List<Sentiment> m_UnionSentiment { get { return ConfigManager.GetAll<Sentiment>(); } }

    /// <summary>
    /// ������Ϣ�Ķ�̬
    /// </summary>
    public Queue<PUnionNotice> m_unionDynamic = new Queue<PUnionNotice>();
    public PUnionIntegralInfo m_selfInter { get; set; } = null;
    public List<PUnionIntegralInfo> UnionInterList { get { return SortUnionIntergl(); } }
    private List<PUnionIntegralInfo> m_unionInterList = new List<PUnionIntegralInfo>();//������ֶ���

    public List<long> ApplyUnionList = new List<long>();

    #endregion

    #region boss
    public const string EventUnionBossOpen = "EventUnionBossOpen";
    public const string EventUnionBossClose = "EventUnionBossClose";
    public const string EventUnionBossOver = "EventUnionBossOver";
    public const string EventUnionBossChange = "EventUnionBossChange";
    public const string EventUnionBossHurt = "EventUnionBossHurt";
    public const string EventUnionBossCanGet = "EventUnionBossCanGet";
    public const string EventUnionBossSortHurt = "EventUnionBossSortHurt";
    public const string EventUnionBossKillCool = "EventUnionBossKillCool";
    public const string EventUnionRewardGet = "EventUnionRewardGet";
    public const string EventUnionBuyEffect = "EventUnionBuyEffect";

    public bool m_isUnionBossTask { get; set; } = false;
    /// <summary>
    /// ��һ����˺�
    /// </summary>
    public int lastTime { get; set; } = 0;
    /// <summary>
    /// ��ε��˺�
    /// </summary>
    public int m_thisHurt { get; set; }

    public bool m_bossOpen { get; set; } = false;

    public float m_openScenceTime { get; set; }
    public float m_killTime { get; set; }
    /// <summary>
    /// boss����ʣ����
    /// </summary>
    public float m_bossCloseTime { get; set; }
    public PPlayerOnlyInfo m_onlyInfo { get; set; }
    /// <summary>
    /// ����ʱ���Լ��Ƿ��Ѿ���������pve
    /// </summary>
    public bool m_inUnionPve { get; set; } = true;
    /// <summary>
    /// �˺����а�
    /// </summary>
    public List<PPlayerHurt> m_playerHurt = new List<PPlayerHurt>();

    public PBossInfo BossInfo { get { return m_bossInfo; } }
    private PBossInfo m_bossInfo;   //boss��Ϣ

    public PBossSet BossSet { get { return m_bossSet; } }
    private PBossSet m_bossSet;     //boss ����

    public BossStageInfo m_bossStage { get; set; }
    public List<BossBoxReward> m_bossReward = new List<BossBoxReward>();
    /// <summary>
    /// ����״̬
    /// </summary>
    public Dictionary<int, EnumActiveState> m_boxStae = new Dictionary<int, EnumActiveState>();
    /// <summary>
    /// �������������Ч��
    /// </summary>
    public List<PUnionBossEffect> BossBuffInfo = new List<PUnionBossEffect>(); 
    /// <summary>
    /// �����������
    /// </summary>
    public List<BossEffectInfo> AllBossBuff { get { return ConfigManager.GetAll<BossEffectInfo>(); } }
    public List<int> AnimationBox = new List<int>();
    #endregion

    #region boss

    public uint BossLossTime(float thistime)//���յ�Э�鵽���ڹ�ȥ�˶��
    {
        return (uint)(Time.realtimeSinceStartup - thistime);
    }

    private void BossConfigInfo()//��ǰboss��Ӧ������
    {
        if (m_onlyInfo == null || m_bossInfo == null)
        {
            Logger.LogError("info is null");
            return;
        }

        m_bossReward.Clear();
        m_boxStae.Clear();
        m_bossStage = ConfigManager.Get<BossStageInfo>(m_bossInfo.bossid);
        if (m_bossStage == null || m_bossStage.reward == null)
        {
            Logger.LogError(" config no this id");
            return;
        }

        for (int i = 0; i < m_bossStage.reward.Length; i++)
        {
            BossBoxReward configInfo = ConfigManager.Get<BossBoxReward>(m_bossStage.reward[i]);
            if (configInfo == null)
            {
                Logger.LogError(" config no this id");
                continue;
            }
            m_bossReward.Add(configInfo);
            float tag = ((float)configInfo.condition / (float)100) * m_bossStage.bossHP;
            if (m_bossInfo.remianblood <= tag)
            {
                m_boxStae.Add(configInfo.ID, EnumActiveState.CanPick);
            }
            else
            {
                m_boxStae.Add(configInfo.ID, EnumActiveState.NotPick);
            }
        }

        if (m_onlyInfo.rewardid == null) return;
        for (int i = 0; i < m_onlyInfo.rewardid.Length; i++)
        {
            bool have = m_boxStae.ContainsKey(m_onlyInfo.rewardid[i]);
            if (!have) continue;
            m_boxStae[m_onlyInfo.rewardid[i]] = EnumActiveState.AlreadPick;
        }

        if (m_bossInfo.bossstate == 0)//����Ƿǿ���״̬���о��ǲ�����ȡ״̬
        {
            for (int i = 0; i < m_bossStage.reward.Length; i++)
            {
                BossBoxReward configInfo = ConfigManager.Get<BossBoxReward>(m_bossStage.reward[i]);
                if (configInfo == null) continue;
                m_boxStae[configInfo.ID] = EnumActiveState.NotPick;
            }
        }
    }

    public void OpenBoss()
    {
        CsUnionBossOpen p = PacketObject.Create<CsUnionBossOpen>();
        session.Send(p);
    }
    void _Packet(ScUnionBossOpen p)//boss ����
    {
        if (m_onlyInfo == null || m_bossInfo == null)
        {
            Logger.LogError("no boss info  but boss open");
            return;
        }
        var pp = p.Clone();
        if (pp.result == 0)
        {
            ClearBoxOpen();
            moduleHome.UpdateIconState(HomeIcons.Guild, true);

            m_bossCloseTime = Time.realtimeSinceStartup;

            m_onlyInfo.hurt = 0;//�����Լ����˺�
            m_onlyInfo.cooltime = 0;

            //�����Ѿ���ȡ����Ʒ
            int[] reward = new int[0];
            m_onlyInfo.rewardid = reward;
            m_bossInfo = pp.bossinfo;
            BossBuffInfo.Clear();
            BossConfigInfo();
            SetNotice(9);
            DispatchModuleEvent(EventUnionBossOpen);
        }
        else if (pp.result == 1)
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 176));//177 ����û����  178����û����
        else if (pp.result == 2)
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 178));
        else if (pp.result == 3)
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 178));
        else if (pp.result == 4)
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 179));
    }

    public void ChangeBossSet(int type, int matic, sbyte[] openDay, int openTime)//����boss ����
    {
        CsUnionBossSet p = PacketObject.Create<CsUnionBossSet>();
        p.type = (sbyte)type;
        p.bossautomatic = (sbyte)matic;
        p.openday = openDay;
        p.opentime = openTime;
        session.Send(p);
    }
    void _Packet(ScUnionBossSet p)//boss ��Ϣ����
    {
        ScUnionBossSet pp = p.Clone();
        // 1 Ȩ�޲��� 2 boss������ 3 �Ѷ�δ����
        if (p.result == 0)
        {
            m_bossSet.diffculttype = (sbyte)pp.type;
            m_bossSet.bossautomatic = pp.bossautomatic;
            m_bossSet.openday = pp.openday;
            m_bossSet.opentime = pp.opentime;
            m_bossInfo = pp.bossinfo;
            BossConfigInfo();
            m_openScenceTime = Time.realtimeSinceStartup;//�õ���ȡ��Э���ʱ��
            DispatchModuleEvent(EventUnionBossChange);

            if (p.roleid == modulePlayer.id_)
                moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 145));
        }
        else if (p.result == 1)
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 176));
        else if (p.result == 2)
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 175));
        else if (p.result == 3)
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 202));
        else if (p.result == 4)
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 203));
    }
    void _Packet(ScUnionBossClose p)//boss �ر�
    {
        if (m_bossInfo == null)
        {
            Logger.LogError("no boss info  but boss close");
            return;
        }
        ScUnionBossClose pp = p.Clone();
        moduleHome.UpdateIconState(HomeIcons.Guild, false);

        m_playerHurt.Clear();
        BossBuffInfo.Clear();
        m_playerHurt.AddRange(pp.player);
        m_playerHurt = SortHurt();
        //����Ҫ��������˵��˺�ֵ �Լ���������˺�ֵ
        m_bossInfo.remianblood = pp.remiablood;
        m_bossInfo.bossstate = 0;
        moduleUnion.BossInfo.remaintimes = 1;//0 δ��� 1 ���

        m_bossOpen = false;

        if (m_bossInfo.bossid >= m_bossSet.diffcuttop && pp.remiablood <= 0)
            m_bossSet.diffcuttop++;

        SetNotice(10);
        DispatchModuleEvent(EventUnionBossClose);
    }

    public void SendGetEnd(PVEOverState overState)
    {
        if (overState == PVEOverState.RoleDead) return;

        m_thisHurt = (int)modulePVE.GetPveGameData(EnumPVEDataType.Attack);
        CsUnionBossOver p = PacketObject.Create<CsUnionBossOver>();
        p.overdata = modulePVE.GetPveDatas();
        p.overState = (sbyte)overState;
        session.Send(p);

    }
    void _Packet(ScUnionBossOver p)
    {
        if (p.result == 0)
        {
            m_inUnionPve = false;
            m_bossOpen = false;

            m_killTime = Time.realtimeSinceStartup;//��Ϸһ��ʼ����������ʱ��
            m_onlyInfo.cooltime = p.cooltime;
            if (p.remiablood <= 0) m_onlyInfo.cooltime = 0;

            if (m_bossInfo.bossstate == 1) BossInfo.remianblood = p.remiablood;

            moduleUnion.m_onlyInfo.hurt += m_thisHurt;
            if (moduleUnion.m_onlyInfo.hurt > m_bossStage.bossHP)
            {
                moduleUnion.m_onlyInfo.hurt = m_bossStage.bossHP;
            }
            DispatchModuleEvent(EventUnionBossOver);
        }
        else if (p.result == 1) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 125));
        else if (p.result == 2) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 126));
        else if (p.result == 3) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 127));
    }
    public void SetNewBoss()
    {
        if (m_bossInfo.bossstate == 0)
        {
            m_bossInfo.bossid = BossSet.diffculttype;
            if (m_onlyInfo != null && m_onlyInfo?.rewardid != null) m_onlyInfo.rewardid.Clear();

            m_bossInfo.remianblood = m_bossStage.bossHP;

            BossConfigInfo();
        }
    }

    public void SendMyAttack(int attack)
    {
        CsUnionBossHurt p = PacketObject.Create<CsUnionBossHurt>();
        p.myhurt = attack;
        session.Send(p);
    }

    void _Packet(ScUnionBossHurt p)//��boss�������˺�ֵ�仯
    {
        if (m_bossInfo == null) return;
        m_bossInfo.remianblood = p.remiablood;

        DispatchModuleEvent(EventUnionBossHurt);
    }
    void _Packet(ScBossRewardOpen p)//boss ���俪��
    {
        for (int i = 0; i < p.boxid.Length; i++)
        {
            int boxIndex = p.boxid[i];

            bool have = m_boxStae.ContainsKey(boxIndex);
            if (!have) continue;

            m_boxStae[boxIndex] = EnumActiveState.CanPick;
        }
        DispatchModuleEvent(EventUnionBossCanGet, p.Clone().boxid);
    }
    public void SendGetHurtSort()//��ȡ����
    {
        CsBossHurtSort p = PacketObject.Create<CsBossHurtSort>();
        session.Send(p);
    }
    void _Packet(ScBossHurtSort p)
    {
        m_playerHurt.Clear();
        m_playerHurt.AddRange(p.Clone().player);
        m_playerHurt = SortHurt();

        DispatchModuleEvent(EventUnionBossSortHurt);
    }
    public void EnterBoss()//���뿪ʼ��boss
    {
        m_isUnionBossTask = true;
        lastTime = 0;
        CsUnionBossKill p = PacketObject.Create<CsUnionBossKill>();
        p.bossid = m_bossInfo.bossid;
        session.Send(p);
    }
    void _Packet(ScUnionBossKill p)
    {
        if (p.result == 0 && m_isUnionBossTask)
        {
            EnterBossTask = true;
            AudioManager.PlaySound(AudioInLogicInfo.audioConst.enterStageSucc);
            modulePVE.OnPVEStart(p.stageid, PVEReOpenPanel.UnionBoss);
            // DispatchModuleEvent(EventBossEnter);
        }
        else if (p.result == 1) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 198));
        else if (p.result == 2) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 199));
        else if (p.result == 3) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 200));
        else if (p.result == 4) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 201));
        else moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 128));
    }
    void _Packet(ScBossKillCool p)//����
    {
        //�ڹ���bossս���������ȴ��ٽ�ȥ
        DispatchModuleEvent(EventUnionBossKillCool);
    }
    public void GetBoxReward(int boxId)//��ȡ����
    {
        CsBossBoxGet p = PacketObject.Create<CsBossBoxGet>();
        p.id = boxId;
        session.Send(p);
    }
    void _Packet(ScBossBoxGet p)
    {
        if (p.result == 0)
        {
            BossBoxReward boss = moduleUnion.m_bossReward.Find(a => a.ID == p.id);
            if (boss == null)
            {
                Logger.LogError("this id info is null");
            }

            bool have = m_boxStae.ContainsKey(p.id);
            if (!have) return;
            m_boxStae[p.id] = EnumActiveState.AlreadPick;

            List<int> getList = new List<int>();
            for (int i = 0; i < m_onlyInfo.rewardid.Length; i++)
            {
                getList.Add(m_onlyInfo.rewardid[i]);
            }
            bool gethave = getList.Exists(a => a == p.id);
            if (!gethave)
            {
                getList.Add(p.id);
            }
            m_onlyInfo.rewardid = getList.ToArray();

            DispatchModuleEvent(EventUnionRewardGet, p.reward.Clone(), p.id);
        }
        else  moduleGlobal.ShowMessage(242, 129);
        
    }

    #endregion

    #region get all info

    public bool IsUnionMember(ulong rRoleId)
    {
        return m_unionPlayer.Exists(item => item.info.roleId == rRoleId);
    }

    public PUnionPlayer GetUnionPlayer(ulong rRoleId)
    {
        return m_unionPlayer.Find(item => item.info.roleId == rRoleId);
    }

    private void RequestUnionInfo()//���󹫻���Ϣ������������ǰ��
    {
        // ����roleinfo�еĹ���id�ж��Ƿ��ڹ�����
        CsUnionInfo p = PacketObject.Create<CsUnionInfo>();
        session.Send(p);
        //��ȡ�Լ��ķ�������
        GeSelfClaimsInfo();
    }
    void _Packet(ScUnionInfo p)//������Ϣ
    {
        ScUnionInfo pp = p.Clone();
        if (pp.baseinfo == null || pp.detailinfo == null)
        {
            Logger.LogError("Module_Union union baseInfo or detailInfo is null");
            return;
        }

        m_bossCloseTime = Time.realtimeSinceStartup;
        m_killTime = Time.realtimeSinceStartup;//��Ϸһ��ʼ����������ʱ��

        m_openScenceTime = Time.realtimeSinceStartup;//�õ���ȡ��Э���ʱ��
        m_unionPlayer.Clear();
        m_playerHurt.Clear();
        m_unionDynamic.Clear();

        m_unionBaseInfo = pp.baseinfo;
        m_expendSentiment = pp.detailinfo.expendsentiment;
        m_bossInfo = pp.detailinfo.bossinfo;
        m_bossSet = pp.detailinfo.bossset;
        m_onlyInfo = pp.onlyinfo;
        m_playerHurt.AddRange(pp.allhurt);
        m_playerHurt = SortHurt();
        GetEffectState();

        m_unionPlayer.AddRange(pp.detailinfo.player);
        m_unionPlayer = ListSort(m_unionPlayer);
        for (int i = pp.notice.Length - 1; i >= 0; i--)
        {
            m_unionDynamic.Enqueue(pp.notice[i]);
        }

        BossConfigInfo();
        SetUnionHint();

        for (int i = 0; i < p.detailinfo.player.Length; i++)
        {
            if (p.detailinfo.player[i].info == null) continue;
            if (p.detailinfo.player[i].info.roleId == modulePlayer.roleInfo.roleId)
            {
                PUnionPlayer president = p.detailinfo.player[i];
                inUnion = president.title;
                if (inUnion == 0 || inUnion == 1)
                {
                    GetAllUnionApply();
                }
                break;
            }
        }
    }

    private void SetUnionHint()
    {
        if (m_bossInfo == null) return;

        if (m_bossInfo.bossstate == 0)
            moduleHome.UpdateIconState(HomeIcons.Guild, false);
        else if (m_bossInfo.bossstate == 1)
            moduleHome.UpdateIconState(HomeIcons.Guild, true);
    }

    private void GetApplyTimes()
    {
        moduleHome.UpdateIconState(HomeIcons.Guild, false);
        //��ȡ���������
        CsUnionApplyTimes p = PacketObject.Create<CsUnionApplyTimes>();
        session.Send(p);
    }

    void _Packet(ScUnionApplyTimes p)
    {
        m_lossTimes = p.times; //ʣ�����
    }

    #endregion

    #region   δ�ڹ���Ĳ���   

    private void EnterUnion( PUnionInfo baseInfo,PUnionDetailInfo detailInfo,PPlayerOnlyInfo only,PPlayerHurt[] hurt)
    {
        RemoveFirstSign();
        InitClamis();
        ClearBoxOpen();
        m_unionDynamic.Clear();
        m_unionPlayer.Clear();
        m_playerHurt.Clear();
        modulePlayer.roleInfo.leagueID = (ulong)baseInfo.id;
        m_unionBaseInfo = baseInfo;
        m_expendSentiment = detailInfo.expendsentiment;
        m_onlyInfo = only;
        m_bossInfo = detailInfo.bossinfo;
        m_unionPlayer.AddRange(detailInfo.player);
        m_unionPlayer = ListSort(m_unionPlayer);
        m_bossSet = detailInfo.bossset;
        m_playerHurt.AddRange(hurt);
        m_playerHurt = SortHurt();
        m_bossCloseTime = Time.realtimeSinceStartup;
        m_openScenceTime = Time.realtimeSinceStartup;//�õ���ȡ��Э���ʱ��

        //��ȡ�Լ��ķ������� ���빫��ʱ��
        GeSelfClaimsInfo();

        if (moduleSet.pushState.ContainsKey(SwitchType.UnionBoss) && moduleSet.pushState[SwitchType.UnionBoss] != 0)
            DispatchEvent(SDKEvent.GLOBAL, SDKEvent.PopSdk(SDKEvent.UNION_CHANGE, modulePlayer.roleInfo.leagueID, (uint)1));
    }

    public void RefreshUnionList()//ˢ���б��Ƽ�
    {
        CsUnionRefresh p = PacketObject.Create<CsUnionRefresh>();
        session.Send(p);
    }
    void _Packet(ScUnionRefresh p)//��ʾ����Ϣ�б�
    {
        ApplyUnionList.Clear();
        m_refrshList.Clear();
        m_refrshList.AddRange(p.Clone().refreshinfo);
        SortUnionList();
        DispatchModuleEvent(EventUnionRefreshList);
    }

    private void SortUnionList()
    {
        List<PRefreshInfo> fullList = new List<PRefreshInfo>();
        List<PRefreshInfo> notList = new List<PRefreshInfo>();

        for (int i = 0; i < m_refrshList.Count; i++)
        {
            if (m_refrshList[i] == null || m_refrshList[i]?.refreshinfo == null) continue;
            if (m_refrshList[i].refreshinfo.playernum >= m_refrshList[i].refreshinfo.playertop)
            {
                fullList.Add(m_refrshList[i]);
            }
            else notList.Add(m_refrshList[i]);
        }
        fullList = SortLevelSent(fullList);
        notList = SortLevelSent(notList);
        m_refrshList.Clear();
        m_refrshList.AddRange(fullList);
        m_refrshList.AddRange(notList);
    }

    private List<PRefreshInfo> SortLevelSent(List<PRefreshInfo> list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            for (int j = i + 1; j < list.Count; j++)
            {
                PRefreshInfo temp;
                if (list[j].refreshinfo.level > list[i].refreshinfo.level)
                {
                    temp = list[i];
                    list[i] = list[j];
                    list[j] = temp;
                }
                else if (list[j].refreshinfo.level == list[i].refreshinfo.level)
                {
                    if (list[j].refreshinfo.sentimentnow > list[i].refreshinfo.sentimentnow)
                    {
                        temp = list[i];
                        list[i] = list[j];
                        list[j] = temp;
                    }
                }
            }
        }
        return list;
    }

    public void CreateUnion(string name)//���ʹ���������Ϣ
    {
        CsUnionCreate p = PacketObject.Create<CsUnionCreate>();
        p.name = name;
        session.Send(p);
    }

    void _Packet(ScUnionCreate p) //����������
    {
        ScUnionCreate pp = p.Clone();
        if (pp.result == 0)
        {
            ApplyUnionList.Clear();
            if (pp.baseinfo == null)
            {
                Logger.LogError(" create union but no info");
                return;
            }
            inUnion = 0;
            EnterUnion(pp.baseinfo, pp.detailinfo, pp.onlyinfo, pp.allhurt);
            BossConfigInfo();

            DispatchModuleEvent(EventUnionCreateSucced);
        }
        else if (p.result == 1) moduleGlobal.ShowMessage(242, 130);
        else if (p.result == 2) moduleGlobal.ShowMessage(242, 131);
        else if (p.result == 3) moduleGlobal.ShowMessage(242, 132);
        else if (p.result == 4) moduleGlobal.ShowMessage(242, 133);
        else if (p.result == 5) moduleGlobal.ShowMessage(242, 134);
        else if (p.result == 6) moduleGlobal.ShowMessage(242, 135);

    }

    public void AddUnion(int type, long unionid = 0) //���빫�� 0 ָ������ 1 ���ټ���
    {
        CsUnionAdd p = PacketObject.Create<CsUnionAdd>();
        p.type = (sbyte)type;
        p.unionid = unionid;
        session.Send(p);
        if (unionid != 0)
        {
            var have = ApplyUnionList.Exists(a => a == unionid);
            if (!have) ApplyUnionList.Add(unionid);
        }

        if (m_lossTimes <= 0) return;
        else m_lossTimes--;
    }
    void _Packet(ScUnionAdd p) //���빤��Ļظ�(0  �ɹ����빫��)
    {
        ScUnionAdd pp = p.Clone();
        if (p.result == 0)
        {
            ApplyUnionList.Clear();
            if (pp.baseinfo == null)
            {
                Logger.LogError(" add union but no info");
                return;
            }
            inUnion = 2;
            EnterUnion(pp.baseinfo, pp.detailinfo, pp.onlyinfo, pp.allhurt);
            GetEffectState();

            for (int i = pp.notice.Length - 1; i >= 0; i--)
            {
                m_unionDynamic.Enqueue(pp.notice[i]);
            }
            SetAllNotice();

            BossConfigInfo();
            DispatchModuleEvent(EventUnionAddSucced);
        }
        else if (p.result == 1) moduleGlobal.ShowMessage(242, 130);//�й���         
        else if (p.result == 2) moduleGlobal.ShowMessage(242, 60);//û����        
        else if (p.result == 3) moduleGlobal.ShowMessage(242, 186);//��cdʱ��           
        else if (p.result == 5) moduleGlobal.ShowMessage(242, 195);//��������ù���           
        else moduleGlobal.ShowMessage(242, 136);

        //����ʧ�� ��������(���ټ���ʧ�ܽ�����ʾ)
        if (p.result != 0)
        {
            m_lossTimes++;
            var have = ApplyUnionList.Exists(a => a == p.unionid);
            if (have) ApplyUnionList.Remove(p.unionid);

            var indexs = UpdateInfo(p.unionid, moduleUnion.m_refrshList);
            if (indexs != -1) moduleUnion.m_refrshList[indexs].state = 0;
            var indexss = moduleUnion.UpdateInfo(p.unionid, moduleUnion.selectUnionList);
            if (indexss != -1) moduleUnion.selectUnionList[indexss].state = 0;

            DispatchModuleEvent(EventUnionAddFailed, p.unionid, indexs, indexss);
        }
    }
    public int UpdateInfo(long id, List<PRefreshInfo> listInfo)
    {
        int index = -1;
        for (int i = 0; i < listInfo.Count; i++)
        {
            if (listInfo[i].refreshinfo == null) continue;
            if (id == listInfo[i].refreshinfo.id)
            {
                index = i;
            }
        }
        return index;
    }


    public void SelectUnion(string name, int type)//�������� ����/id
    {
        CsUnionSelect p = PacketObject.Create<CsUnionSelect>();
        p.name = name;
        p.type = (sbyte)type;
        session.Send(p);
        ApplyUnionList.Clear();
    }
    void _Packet(ScUnionSelect p)
    {
        ScUnionSelect pp = p.Clone();
        selectUnionList.Clear();
        selectUnionList.AddRange(pp.baseinfo);
        DispatchModuleEvent(EventUnionSelectResult, pp.result);

        SetAddPlane(pp.baseinfo, pp.result);
    }
    private void SetAddPlane(PRefreshInfo[] info, int result)
    {
        Window_Friend w = Window.GetOpenedWindow<Window_Friend>();
        if (w != null && w.actived)//�ù����Ѳ�����
        {
            if (result == 0)
            {
                if (info == null || info.Length <= 0) return;
                if (info[0].refreshinfo == null) return;

                Window_Alert.ShowAlert(Util.Format(ConfigText.GetDefalutString(248, 3), info[0].refreshinfo.unionname), true, true, true, () =>
                {
                    Module_Union.instance.AddUnion(0, info[0].refreshinfo.id);
                }, null, "", "");
            }
            else moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 196));
        }
    }

    #endregion

    #region �ڹ����������

    public void GetAllUnionApply()//�����������������б�����������뵽��
    {
        m_unionApply.Clear();
        CsUnionApplyNotice p = PacketObject.Create<CsUnionApplyNotice>();
        session.Send(p);
    }

    void _Packet(ScUnionApplyNotice p)
    {
        PPlayerInfo[] pp = p.Clone().playerinfo;
        if (inUnion == 0 || inUnion == 1)
        {
            //home�г��ֺ��
            for (int i = 0; i < pp.Length; i++)
            {
                bool have = m_unionApply.Exists(a => a.roleId == pp[i].roleId);
                if (!have)
                {
                    m_unionApply.Add(pp[i]);
                }
            }
            DispatchModuleEvent(EventUnionApplyNotice);
        }
    }

    public void SloveApply(int type, long[] ids)//�������
    {
        moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 130));
        CsUnionApplySolve p = PacketObject.Create<CsUnionApplySolve>();
        p.type = (sbyte)type;
        p.playerid = ids;//����Ҫ����idת��
        session.Send(p);
        if (type == 2)
        {
            PPlayerInfo playerInfo = m_unionApply.Find(a => a.roleId == (ulong)ids[0]);
            m_unionApply.Remove(playerInfo);
            DispatchModuleEvent(EventUnionApplySolveResult);
        }
    }

    void _Packet(ScUnionApplySolve p)//���봦����
    {
        if (p.result == 0 || p.result == 1)
        {
            long[] ids = p.playerid;
            if (ids.Length == 0)
            {
                Logger.LogError("agree id is null ");
                return;
            }
            if (p.type == 0)
            {
                if (p.result == 0) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 137));
                else moduleGlobal.ShowMessage(242, 130);

                PPlayerInfo playerInfo = m_unionApply.Find(a => a.roleId == (ulong)ids[0]);
                m_unionApply.Remove(playerInfo);
            }
            else if (p.type == 1)
            {
                string str = p.num.ToString() + ConfigText.GetDefalutString(242, 138);
                moduleGlobal.ShowMessage(str);
                m_unionApply.Clear();
            }
            else if (p.type == 3) m_unionApply.Clear();

            DispatchModuleEvent(EventUnionApplySolveResult);
        }
        else moduleGlobal.ShowMessage(242, 209);

    }

    void _Packet(ScUnionApplyOverdue p)//�������
    {
        PPlayerInfo playerInfo = m_unionApply.Find(a => a.accId == p.playerid);
        if (playerInfo == null) return;
        m_unionApply.Remove(playerInfo);
        DispatchModuleEvent(EventUnionApplyOverdue, playerInfo);
    }

    public void ChangeTitle(long playerid, int title)//����ְλ ������ԱȨ����
    {
        CsUnionTitleChange p = PacketObject.Create<CsUnionTitleChange>();
        p.playerid = playerid;
        p.title = (sbyte)title;
        session.Send(p);
    }

    void _Packet(ScUnionTitleChange p)//���Ľ��
    {
        // 1���ǻ᳤2���᳤����3���Ѿ��Ǹ��᳤��4�����Ҳ���
        if (p.result == 0)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 139));
            PUnionPlayer m_playerInfo = m_unionPlayer.Find(a => a.info.roleId == (ulong)p.playerid);
            if (m_playerInfo == null) return;

            for (int i = 0; i < m_unionPlayer.Count; i++)
            {
                if (m_unionPlayer[i].info == null) continue;
                if (m_unionPlayer[i].info.roleId == (ulong)p.playerid)
                {
                    m_unionPlayer[i].title = p.title;
                    if (p.title == 0)
                    {
                        if (m_unionBaseInfo == null) return;
                        m_unionBaseInfo.presidentname = m_playerInfo.info.name;
                    }
                    DispatchModuleEvent(EventUnionChangeTitle, p.playerid, p.title);
                    break;
                }
            }
        }
        else if (p.result == 1) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 205));
        else if (p.result == 2) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 206));
        else if (p.result == 3) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 207));
        else if (p.result == 4) moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 208));

    }

    void _Packet(ScPlayerTitleChange p)//ְλ������
    {
        PUnionPlayer changeInfo = m_unionPlayer.Find(a => a.info.roleId == (ulong)p.roleid);
        if (changeInfo == null) return;

        for (int i = 0; i < m_unionPlayer.Count; i++)
        {
            if (m_unionPlayer[i].info == null) continue;
            if (m_unionPlayer[i].info.roleId == (ulong)p.roleid)
            {
                m_unionPlayer[i].title = p.title;
            }
        }
        if (p.title == 0)
        {
            if (m_unionBaseInfo == null) return;
            m_unionBaseInfo.presidentname = changeInfo.info.name;
            inUnion = 0;
        }
        if (p.roleid == (long)modulePlayer.roleInfo.roleId)
        {
            if (p.title == 0)
            {
                if (m_unionBaseInfo == null) return;

                GetAllUnionApply();

                m_unionBaseInfo.presidentname = changeInfo.info.name;
            }
            else if (p.title == 1)
            {
                GetAllUnionApply();
                inUnion = 1;
            }
            else if (p.title == 2)
            {
                m_unionApply.Clear();//��Ϊ��ͨ��Ա
                inUnion = 2;
            }
        }
        //����ˢ�½������ʾ�����ػ��߳��ֹ���Ա�����б�֮�ࣩ
        DispatchModuleEvent(EventUnionTitleBeChange, p.title, p.roleid);

        if (p.title == 0)
            SetNotice(2, changeInfo.info.name);
        else if (p.title == 1)
            SetNotice(3, changeInfo.info.name);
        else if (p.title == 2)
            SetNotice(4, changeInfo.info.name);

    }

    private void LeaveUnion()
    {
        if (moduleSet.pushState.ContainsKey(SwitchType.UnionBoss) && moduleSet.pushState[SwitchType.UnionBoss] != 0)
            DispatchEvent(SDKEvent.GLOBAL, SDKEvent.PopSdk(SDKEvent.UNION_CHANGE, modulePlayer.roleInfo.leagueID, (uint)0));

        InitClamis();
        GetApplyTimes();
        ClearBoxOpen();
        inUnion = -1;
        m_unionBaseInfo = null;
        CardSignInfo = null;
        m_expendSentiment = 0;
        m_unionPlayer.Clear();
        m_unionApply.Clear();
        m_unionDynamic.Clear();
        m_playerHurt.Clear();
        BossBuffInfo.Clear();
        moduleChat.m_unionChat.Clear();
        modulePlayer.roleInfo.leagueID = 0;
        moduleUnion.m_inOtherPalne = false;
    }

    public void ExitUnion()//�˳�����
    {
        CsUnionExit p = PacketObject.Create<CsUnionExit>();
        session.Send(p);
    }

    void _Packet(ScUnionExit p)//�Լ��˳�����
    {
        if (p.result == 0)
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 141));
            //û�й��� ������������Ϣ
            var pro = inUnion;
            LeaveUnion();

            DispatchModuleEvent(EventUnionSelfExit, 1, pro);
        }
        else
        {
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 142));
            //�Լ��ǻ᳤
        }
    }
    public void KickedPlayer(ulong id)//�����߳���Ա
    {
        CsUnionKicked p = PacketObject.Create<CsUnionKicked>();
        p.playerid = (long)id;
        session.Send(p);
    }
    void _Packet(ScUnionKicked p)//�߳��Ƿ�ɹ�
    {
        if (p.result == 0)
        {
            //�߳��ɹ�
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 143));
            DispatchModuleEvent(EventUnionRemoveSucced);
        }
        else//�߳�ʧ��
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 144));
    }


    void _Packet(ScUnionBeExit p)//���˱��߳����ᣨ�����˽ӵ���
    {
        if (p.roleid == 0)
        {
            Logger.LogError("id is 0");
            return;
        }

        RemoveLateInfo((ulong)p.roleid);

        if (p.roleid == (long)modulePlayer.id_)
        {
            //û�й��� ������������Ϣ
            var pro = inUnion;
            LeaveUnion();
            DispatchModuleEvent(EventUnionSelfExit, 0, pro);
        }
        else
        {
            PUnionPlayer info = m_unionPlayer.Find(a => a.info.roleId == (ulong)p.roleid);
            if (info == null) return;
            m_unionPlayer.Remove(info);
            m_unionBaseInfo.playernum--;
            if (m_unionBaseInfo.playernum < 0) m_unionBaseInfo.playernum = 0;
            SetNotice(5, info.info.name);
            DispatchModuleEvent(EventUnionPlayerExit, 1);
            RemoveClaimsInfo(0, (ulong)p.roleid);
        }
    }

    private void RemoveLateInfo(ulong id)
    {
        if (id == modulePlayer.id_)
        {
            for (int i = 0; i < m_unionPlayer.Count; i++)
            {
                if (m_unionPlayer[i] == null || m_unionPlayer[i].info == null || m_unionPlayer[i].info.roleId == modulePlayer.id_) continue;
                PPlayerInfo friend = moduleFriend.FriendList.Find(a => a.roleId == m_unionPlayer[i].info.roleId);
                if (friend != null) continue;
                bool r = moduleChat.Past_mes.Exists(a => a == m_unionPlayer[i].info.roleId);
                if (r) moduleChat.Past_mes.Remove(m_unionPlayer[i].info.roleId);
                PPlayerInfo info = moduleChat.Late_ListAllInfo.Find(a => a.roleId == m_unionPlayer[i].info.roleId);
                if (info != null) moduleChat.Late_ListAllInfo.Remove(info);
            }
        }
        else
        {
            PPlayerInfo friend = moduleFriend.FriendList.Find(a => a.roleId == id);
            if (friend != null) return;
            bool r = moduleChat.Past_mes.Exists(a => a == id);
            if (r) moduleChat.Past_mes.Remove(id);
            PPlayerInfo info = moduleChat.Late_ListAllInfo.Find(a => a.roleId == id);
            if (info != null) moduleChat.Late_ListAllInfo.Remove(info);
        }
        moduleFriend.FriendHintShow();
    }

    void _Packet(ScPlayerSentimentChange p)//����ֵ����
    {
        //ˢ�³�Ա�б�
        for (int i = 0; i < m_unionPlayer.Count; i++)
        {
            if (m_unionPlayer[i].info.roleId == (ulong)p.playerid)
            {
                m_unionPlayer[i].sentiment = (int)p.sentiment;
                break;
            }
        }
        m_unionPlayer = ListSort(m_unionPlayer);
    }

    public void SetUnionLevel(int minLevel, int open, int automaticLevel)//�������ȼ�
    {
        CsUnionLevelSet p = PacketObject.Create<CsUnionLevelSet>();
        p.minlevel = minLevel;
        p.automaticlevel = automaticLevel;
        p.isopen = (sbyte)open;
        session.Send(p);
    }

    void _Packet(ScUnionLevelSet p)
    {
        if (p.result == 0)
        {
            m_unionBaseInfo.minlevel = p.minlevel;
            m_unionBaseInfo.automaticlevel = p.automaticlevel;
            m_unionBaseInfo.automatic = p.isopen;
            moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 145));
            DispatchModuleEvent(EventUnionSetLevel);
        }
        else if (p.result == 1)  moduleGlobal.ShowMessage(Util.Format(ConfigText.GetDefalutString(242, 191), p.minlevel)); 
        else moduleGlobal.ShowMessage(ConfigText.GetDefalutString(242, 146));
        
    }

    void _Packet(ScPlayerAutomaticAdd p)//���˼��빫�ᣨ���͸������ˣ�
    {
        PUnionPlayer[] pp = p.Clone().player;
        if (m_unionBaseInfo == null)
        {
            Logger.LogError(" no guild");
            return;
        }
        
        List<PPlayerHurt> addList = new List<PPlayerHurt>();
        for (int i = 0; i < pp.Length; i++)
        {
            if (pp[i] == null || pp[i]?.info == null) continue;

            var info = m_unionPlayer.Find(a => a.info.roleId == pp[i].info.roleId);
            if (info != null) m_unionPlayer.Remove(info);
            m_unionPlayer.Add(pp[i]);

            var hurt = m_playerHurt.Find(a => a.roleid == pp[i].info.roleId);
            if (hurt != null) m_playerHurt.Remove(hurt);
            PPlayerHurt hurtInfo = PacketObject.Create<PPlayerHurt>();
            hurtInfo.roleid = pp[i].info.roleId;
            hurtInfo.name = pp[i].info.name;
            hurtInfo.hurt = 0;
            m_playerHurt.Add(hurtInfo);
        }
        m_playerHurt = SortHurt();
        m_unionPlayer = ListSort(m_unionPlayer);

        m_unionBaseInfo.playernum = m_unionPlayer.Count;
        DispatchModuleEvent(EventUnionPlayerAdd, addList);
        for (int i = 0; i < pp.Length; i++)
        {
            if (pp == null) continue;
            SetNotice(0, pp[i].info.name);
        }
    }

    void _Packet(ScUnionPlayerExit p)//�����˳����ᣨ���͸������ˣ�
    {
        if (m_unionPlayer == null) return;
        PUnionPlayer info = m_unionPlayer.Find(a => a.info.roleId == (ulong)p.playerid);
        if (info == null) return;
        SetNotice(1, info.info.name);
        m_unionPlayer.Remove(info);
        m_unionBaseInfo.playernum--;
        if (m_unionBaseInfo.playernum < 0) m_unionBaseInfo.playernum = 0;

         PPlayerHurt hurt = m_playerHurt.Find(a => a.roleid == (ulong)p.playerid);
        if (hurt == null) return;
        m_playerHurt.Remove(hurt);

        RemoveClaimsInfo(0, (ulong)p.playerid);
        DispatchModuleEvent(EventUnionPlayerExit, 1);

    }
    public void ChangeUnionNotice(string notice)///�޸Ĺ���
    {
        CsUnionNoticeChange p = PacketObject.Create<CsUnionNoticeChange>();
        p.notice = notice;
        session.Send(p);
    }

    void _Packet(ScUnionNoticeChange p)//�޸Ľ��
    {
        if (p.result == 0)  moduleGlobal.ShowMessage(242, 147);
        else moduleGlobal.ShowMessage(242, 148);
        
    }
    void _Packet(ScUnionNoticeBechange p)//���汻�޸�
    {
        SetNotice(6);
        m_unionBaseInfo.announcentment = p.Clone().notice;
        DispatchModuleEvent(EventUnionNoticeChange);
    }

    void _Packet(ScUnionPlayerLine p)//������
    {
        for (int i = 0; i < m_unionPlayer.Count; i++)
        {
            if (m_unionPlayer[i].info == null) continue;
            if (m_unionPlayer[i].info.roleId == (ulong)p.id)
            {
                m_unionPlayer[i].info.state = p.state;
            }
        }
        m_unionPlayer = ListSort(m_unionPlayer);
        DispatchModuleEvent(EventUnionPlayerState);
    }

    //�ȼ�����
    void _Packet(ScUnionLevelChange p)
    {
        m_unionBaseInfo.level = (sbyte)p.level;
        m_unionBaseInfo.sentimentnow = p.sentimentnow;
        DispatchModuleEvent(EventUnionLevelChange);
        if (p.level > m_unionBaseInfo.level) SetNotice(8, p.level.ToString());
        else SetNotice(7, p.level.ToString());
    }

    void _Packet(ScUnionSentimentExpend p)
    {
        var sent = p.expendsentiment;
        m_unionBaseInfo.level = p.level;
        m_expendSentiment = sent;
        m_unionBaseInfo.sentimentnow = p.sentimentnow;
        DispatchModuleEvent(EventUnionSentimntExpend);
        SetNotice(11, sent.ToString());
    }

    public void GetNewSentiment()//ÿ�ν�����ˢ�µ�ǰ�ȼ�����ֵ
    {
        CsUnionSentimentChange p = PacketObject.Create<CsUnionSentimentChange>();
        session.Send(p);
    }
    void _Packet(ScUnionSentimentChange p)
    {
        m_unionBaseInfo.sentimentnow = p.sentimentnow;
        DispatchModuleEvent(EventUnionSentimentUpdate);
    }

    public void DissolutionUnion()//��ɢ����
    {
        CsUnionDissolution p = PacketObject.Create<CsUnionDissolution>();
        session.Send(p);
    }
    void _Packet(ScUnionDissolution p)
    {
        if (p.result == 0)
        {
            //û�й��� ������������Ϣ
            LeaveUnion();
            DispatchModuleEvent(EventUnionDissolution);
        }
        else moduleGlobal.ShowMessage(242, 149);
    }

    #endregion

    #region �������켰 ��Ϣ����

    private void SetNotice(int type, string str = "")
    {
        SetUnionNotice(type, str);
        SetNoticeType((UnionNoticeType)type, str);
    }
    private void SetUnionNotice(int type, string str = "")
    {
        PUnionNotice p = PacketObject.Create<PUnionNotice>();
        p.type = type;
        p.str = str;
        m_unionDynamic.Enqueue(p);
    }

    private void SetNoticeType(UnionNoticeType type, string str = "")
    {
        var notice = Util.Format(ConfigText.GetDefalutString(160, (int)type), str);
        SetNotice(notice);
    }

    private void SetNotice(string txt)
    {
        if (string.IsNullOrEmpty(txt)) return;
        //m_unionDynamic.Dequeue();
        moduleChat.AddChatList(txt, m_addChat);
        DispatchModuleEvent(EventUnionSysUpdate, txt);
    }

    public void SetAllNotice()
    {
        m_addChat = false;
        PUnionNotice[] info = m_unionDynamic.ToArray();
        for (int i = 0; i < info.Length; i++)
        {
            SetNoticeType((UnionNoticeType)info[i].type, info[i].str);
        }
        m_addChat = true;
    }
    #endregion

    #region other
    //��Ա���� ������ǰ �����ں� ����ֵ��ǰ

    private List<PUnionPlayer> ListSort(List<PUnionPlayer> playerList)
    {
        List<PUnionPlayer> sentimentList = new List<PUnionPlayer>();
        List<PUnionPlayer> onLine = new List<PUnionPlayer>();
        List<PUnionPlayer> offLine = new List<PUnionPlayer>();

        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i] == null) continue;
            if (playerList[i].info.state == 0) offLine.Add(playerList[i]);
            else onLine.Add(playerList[i]);
        }

        onLine = SentimentSort(onLine);
        offLine = SentimentSort(offLine);
        for (int i = 0; i < onLine.Count; i++)
        {
            sentimentList.Add(onLine[i]);
        }
        for (int i = 0; i < offLine.Count; i++)
        {
            sentimentList.Add(offLine[i]);
        }
        return sentimentList;
    }

    private List<PUnionPlayer> SentimentSort(List<PUnionPlayer> sortSentiment)
    {
        for (int i = 0; i < sortSentiment.Count; i++)
        {
            for (int j = i + 1; j < sortSentiment.Count; j++)
            {
                if (sortSentiment[i].sentiment < sortSentiment[j].sentiment)
                {
                    PUnionPlayer temp = sortSentiment[i];
                    sortSentiment[i] = sortSentiment[j];
                    sortSentiment[j] = temp;
                }
                else if (sortSentiment[i].sentiment == sortSentiment[j].sentiment)
                {
                    if (sortSentiment[i].info.level < sortSentiment[i].info.level)
                    {
                        PUnionPlayer temp = sortSentiment[i];
                        sortSentiment[i] = sortSentiment[j];
                        sortSentiment[j] = temp;
                    }
                }
            }
        }
        return sortSentiment;
    }

    private List<PUnionPlayer> LevelSort(List<PUnionPlayer> sortSentiment)
    {
        for (int i = 0; i < sortSentiment.Count; i++)
        {
            for (int j = i + 1; j < sortSentiment.Count; j++)
            {
                if (sortSentiment[i].info.level < sortSentiment[j].info.level)
                {
                    PUnionPlayer temp = sortSentiment[i];
                    sortSentiment[i] = sortSentiment[j];
                    sortSentiment[j] = temp;
                }
            }
        }
        return sortSentiment;
    }

    private List<PPlayerHurt> SortHurt()//�˺�������
    {
        for (int i = 0; i < m_playerHurt.Count; i++)
        {
            for (int j = i + 1; j < m_playerHurt.Count; j++)
            {
                //�����sentiment���� hurt
                if (m_playerHurt[i].hurt < m_playerHurt[j].hurt)
                {
                    PPlayerHurt temp = m_playerHurt[i];
                    m_playerHurt[i] = m_playerHurt[j];
                    m_playerHurt[j] = temp;
                }
            }
        }
        return m_playerHurt;
    }


    //list תΪ����
    public long[] ChangeApplyList()
    {
        long[] ids = new long[moduleUnion.m_unionApply.Count];
        for (int i = 0; i < moduleUnion.m_unionApply.Count; i++)
        {
            ids[i] = (long)moduleUnion.m_unionApply[i].roleId;
        }
        return ids;
    }
    #endregion

    #region enter pve

    #endregion

    #region ����ȼ���ʾ

    public string SetUnionLevelTxt(int level)
    {
        switch (level)
        {
            case 1: return ConfigText.GetDefalutString(90001);
            case 2: return ConfigText.GetDefalutString(90002);
            case 3: return ConfigText.GetDefalutString(90003);
            case 4: return ConfigText.GetDefalutString(90004);
            case 5: return ConfigText.GetDefalutString(90005);
            default: return string.Empty;
        }
    }

    #endregion

    #region �����������

    public void GetAllIntergel()
    {
        CsUnionIntergral p = PacketObject.Create<CsUnionIntergral>();
        session.Send(p);
    }

    void _Packet(ScUnionIntergral p)
    {
        ScUnionIntergral pp = p.Clone();
        m_unionInterList.Clear();
        m_selfInter = pp.myinteral;
        m_unionInterList.AddRange(pp.allinteral);
        SetSelfNull();
        DispatchModuleEvent(EventUnionInterglSelf);
        DispatchModuleEvent(EventUnionInterglAll);
    }
    public void GetSelfIntergel()
    {
        CsUnionSelfIntergral p = PacketObject.Create<CsUnionSelfIntergral>();
        session.Send(p);
    }

    void _Packet(ScUnionSelfIntergral p)
    {
        m_selfInter = p.Clone().myinteral;
        SetSelfNull();
        DispatchModuleEvent(EventUnionInterglSelf);
    }

    private void SetSelfNull()
    {
        if (m_selfInter == null && m_unionBaseInfo != null)
        {
            PUnionIntegralInfo self = PacketObject.Create<PUnionIntegralInfo>();
            self.unionId = (int)modulePlayer.roleInfo.leagueID;
            self.unionname = m_unionBaseInfo.unionname;
            self.intergl = 0;
            self.bounty = 0;
            m_selfInter = self;
        }
    }

    private List<PUnionIntegralInfo> SortUnionIntergl()
    {
        for (int i = 0; i < m_unionInterList.Count; i++)
        {
            for (int j = i + 1; j < m_unionInterList.Count; j++)
            {
                if (m_unionInterList[i].intergl < m_unionInterList[j].intergl)
                {
                    PUnionIntegralInfo temp = m_unionInterList[i];
                    m_unionInterList[i] = m_unionInterList[j];
                    m_unionInterList[j] = temp;
                }
            }
        }
        return m_unionInterList;
    }

    #endregion

    #region effect

    public void ClearBoxOpen()
    {
        //�˳����߱��� ����ʱ������
        for (int i = 0; i < m_bossReward.Count; i++)
        {
            var key = "union" + modulePlayer.id_.ToString() + m_bossReward[i].ID.ToString();
            var info = PlayerPrefs.GetString(key);
            if (info != null) PlayerPrefs.DeleteKey(key);
        }
    }

    public void GetEffectState()
    {
        //ֻ������ ���� �ر� ʱ�����һ�� 
        BossBuffInfo.Clear();
        if (m_bossInfo == null || m_bossInfo?.effectState == null) return;
        BossBuffInfo.AddRange(m_bossInfo.effectState);
    }

    public void SendBuyEffect(int effId)
    {
        //����boss����
        CsUnionBuyBuff p = PacketObject.Create<CsUnionBuyBuff>();
        p.buffId = effId;
        session.Send(p);
    }
    void _Packet(ScUnionBuyBuff p)
    {
        // 1 id������ 2 Ǯ���� 3 �������� 4 δ���� 
        if (p.result == 0) moduleGlobal.ShowMessage(242, 212);
        else if (p.result == 1) moduleGlobal.ShowMessage(242, 213);
        else if (p.result == 2) moduleGlobal.ShowMessage(242, 214);
        else if (p.result == 3) moduleGlobal.ShowMessage(242, 215);
        else if (p.result == 4) moduleGlobal.ShowMessage(242, 216);
    }
    void _Packet(ScUnionBuffApply p)
    {
        if (p.buffInfo == null) return;
        PUnionBossEffect effect = p.buffInfo.Clone();
        var info = BossBuffInfo.Find(a => a.effectId == effect.effectId);
        if (info != null) info.times = effect.times;
        else BossBuffInfo.Add(effect);
        if (effect.effectId == 0)//ȷ���Ǹ���cd buff Լ��id
        {
            BossEffectInfo eff = ConfigManager.Get<BossEffectInfo>(effect.effectId);
            if (eff != null) m_onlyInfo.cooltime -= eff.effect;
        }
        DispatchModuleEvent(EventUnionBuyEffect, effect.effectId);
    }
    #endregion

    #region ������ǩ��

    public const string EventUnionCardSign = "EventUnionCardSign";
    public const string EventUnionCardChange = "EventUnionCardChange";
    public const string EventUnionCardReward = "EventUnionCardReward";
    public ScUnionCardInfo CardSignInfo;
    public List<CardTypeInfo> TypeList = new List<CardTypeInfo>();

    public void RemoveFirstSign()
    {
        string signKey = "unionSign" + modulePlayer.id_.ToString() + modulePlayer.roleInfo?.leagueID.ToString();
        PlayerPrefs.DeleteKey(signKey);
    }
    
    public void GetAllInfo()
    {
        TypeList.Clear();
        TypeList = ConfigManager.GetAll<CardTypeInfo>();
    }

    public void GetCardSignInfo()
    {
        CsUnionCardInfo p = PacketObject.Create<CsUnionCardInfo>();
        session.Send(p);
    }
    void _Packet(ScUnionCardInfo p)
    {
        p.CopyTo(ref CardSignInfo);
        SortCard();
        DispatchModuleEvent(EventUnionCardSign);
    }

    private void SortCard()
    {
        if (CardSignInfo == null)
        {
            Logger.LogError("UnionSign Card Info is Null");
            return;
        }
        List<ushort> r = new List<ushort>();
        r.AddRange(CardSignInfo.cardId);
        r.Sort((a, b) => b.CompareTo(a));
        CardSignInfo.cardId = r.ToArray();
    }

    public void ChangeCard(ushort[] cardId)
    {
        CsUnionCardChange p = PacketObject.Create<CsUnionCardChange>();
        p.cardId = cardId;
        session.Send(p);
    }
    void _Packet(ScUnionCardChange p)
    {
        if (p.result == 0)
        {
            CardSignInfo.changeTimes--;
            CardSignInfo.points = p.points;
            for (int i = 0; i < CardSignInfo.cardId.Length; i++)
            {
                for (int j = 0; j < p.oldCardId.Length; j++)
                {
                    if (CardSignInfo.cardId[i] == p.oldCardId[j])
                    {
                        CardSignInfo.cardId[i] = p.newCardId[j];
                    }
                }
            }
            p.typeInfo.CopyTo(ref CardSignInfo.typeInfo);
            DispatchModuleEvent(EventUnionCardChange);
        }
        else if (p.result == 1) moduleGlobal.ShowMessage(629, 10);
        else if (p.result == 2) moduleGlobal.ShowMessage(629, 11);
        else if (p.result == 3) moduleGlobal.ShowMessage(629, 9);

    }
    public void GetCardReward()
    {
        CsUnionCardReward p = PacketObject.Create<CsUnionCardReward>();
        session.Send(p);
    }
    void _Packet(ScUnionCardReward p)
    {
        if (p.result == 0)
        {
            CardSignInfo.changeTimes = 0;
            DispatchModuleEvent(EventUnionCardReward, p.points);
        }
        else if (p.result == 1) moduleGlobal.ShowMessage(629, 9);
    }
    public void GetNextCard()
    {
        CsUnionCardRefresh p = PacketObject.Create<CsUnionCardRefresh>();
        session.Send(p);
    }
    void _Packet(ScUnionCardRefresh p)
    {
        if (p.result == 1) moduleGlobal.ShowMessage(629,13);
    }

    #endregion

    #region ��������ϵͳ
    
    /// <summary>
    /// �����б���Ϣ
    /// </summary>
    public const string EventUnionClamisInfo = "EventUnionClamisInfo";
    /// <summary>
    /// ������˳ɹ�
    /// </summary>
    public const string EventUnionClamisGive = "EventUnionClamisGive";
    /// <summary>
    /// �Լ����񷢲��ɹ�
    /// </summary>
    public const string EventUnionRelaseSelf = "EventUnionRelaseSelf";
    /// <summary>
    /// ɾ������ ������ɾ�� �й����Ա�˳� ���¼��㹫��������Ϣ
    /// </summary>
    public const string EventUnionClamisRemove = "EventUnionClamisRemove";
    /// <summary>
    /// ��¼
    /// </summary>
    public const string EventUnionClamisRecord = "EventUnionClamisRecord";
    /// <summary>
    /// �����Լ�����Ϣ
    /// </summary>
    public const string EventUnionClamisSelf = "EventUnionClamisSelf";

    /// <summary>
    /// �����Լ��Ƿ�ɷ��� 1 ���� 0 ������
    /// </summary>
    public int ReleaseTime;
    /// <summary>
    /// ��ǰ�Լ�����������
    /// </summary>
    public PReleaseReward SelfClaims;
    /// <summary>
    /// ������Ϣ
    /// </summary>
    public List<PReleaseReward> UnionClaimsInfo = new List<PReleaseReward>();
    /// <summary>
    /// ����������Ϣ
    /// </summary>
    public List<PReleaseReward> FriendClaimsInfo = new List<PReleaseReward>();
    /// <summary>
    /// ���͹����¼
    /// </summary>
    public List<PReleaseRecord> UnionGive = new List<PReleaseRecord>();
    /// <summary>
    /// ���ͺ��Ѽ�¼
    /// </summary>
    public List<PReleaseRecord> FriendGive = new List<PReleaseRecord>();
    /// <summary>
    /// ���������¼
    /// </summary>
    public List<PReleaseRecord> UnionRecived = new List<PReleaseRecord>();
    /// <summary>
    /// �������Ѽ�¼
    /// </summary>
    public List<PReleaseRecord> FriendRecived = new List<PReleaseRecord>();
    /// <summary>
    /// �ɷ���װ����Ƭ
    /// </summary>
    public List<PropItemInfo> ReleaseEquip = new List<PropItemInfo>();
    /// <summary>
    /// �ɷ���������Ƭ
    /// </summary>
    public List<PropItemInfo> ReleaseRune = new List<PropItemInfo>();
    /// <summary>
    /// �ɷ���������Ƭ
    /// </summary>
    public List<PropItemInfo> ReleaseOther = new List<PropItemInfo>();
    /// <summary>
    /// ����������ͼ�¼
    /// </summary>
    public List<int> RecordType = new List<int>();


    public void RemoveClaimsInfo(int type, ulong playerId)
    {
        //ɾ�������ں���/����

        var player = UnionClaimsInfo.Find(a => a.playerId == playerId);
        if (player != null && type == 0) UnionClaimsInfo.Remove(player);

        if (type == 1)
        {
            player = FriendClaimsInfo.Find(a => a.playerId == playerId);
            if (player != null) FriendClaimsInfo.Remove(player);
        }
        if (player != null) DispatchModuleEvent(EventUnionClamisRemove, type, player);
    }
    
    public void GeSelfClaimsInfo()
    {
        CsUnionClaimsSelf p = PacketObject.Create<CsUnionClaimsSelf>();
        session.Send(p);
    }
    void _Packet(ScUnionClaimsSelf p)
    {
        var pp = p.Clone();
        if (pp == null) return;
        SelfClaims = pp.selfInfo;
        ReleaseTime = pp.release;
        if (pp.selfInfo != null && UnionClaimsInfo.Count > 0)
        {
            var self = UnionClaimsInfo.Find(a => a.playerId == pp.selfInfo.playerId);
            if (self != null) self.receivedNum = pp.selfInfo.receivedNum;
        }
        if (SelfClaims == null)
        {
            var self = UnionClaimsInfo.Find(a => a.playerId == modulePlayer.id_);
            if (self != null) UnionClaimsInfo.Remove(self);
        }

        DispatchModuleEvent(EventUnionClamisSelf);
    }

    public void GetRelaseInfo(int type)
    {
        CsUnionClaimsInfo p = PacketObject.Create<CsUnionClaimsInfo>();
        p.type = (sbyte )type;
        session.Send(p);
    }
    void _Packet(ScUnionClaimsInfo p)
    {
        var pp = p.Clone();
        if (pp == null) return;
        if (pp.type == 0) AddAllInfo(pp.claimsInfo, UnionClaimsInfo);
        else if (pp.type == 1) AddAllInfo(pp.claimsInfo, FriendClaimsInfo);

        DispatchModuleEvent(EventUnionClamisInfo);
    }

    private void AddAllInfo(PReleaseReward[] info, List<PReleaseReward> list)
    {
        list.Clear();
        for (int i = 0; i < info.Length; i++)
        {
            if (info[i] == null) continue;
            var prop = ConfigManager.Get<PropItemInfo>(info[i].itemTypeId);
            if (prop == null) continue;
            if (!prop.IsValidVocation(modulePlayer.proto)) continue;
            var black = moduleFriend.BlackList.Exists(a => a.roleId == info[i].playerId);
            if (black) continue;
            list.Add(info[i]);
        }
        SortCliamsInfo(list);
    }

    private void SortCliamsInfo(List<PReleaseReward> infoList)
    {
        List<PReleaseReward> selfCan = new List<PReleaseReward>();
        List<PReleaseReward> selfNot = new List<PReleaseReward>();
        List<PReleaseReward> alrealy = new List<PReleaseReward>();

        var self = infoList.Find(a => a.playerId == modulePlayer.id_);
        if (self != null)
        {
            SelfClaims = self;
            infoList.Remove(self);
        }

        for (int i = 0; i < infoList.Count; i++)
        {
            if (infoList[i] == null) continue;
            var prop = ConfigManager.Get<PropItemInfo>(infoList[i].itemTypeId);
            if (prop == null) continue;
            if (infoList[i].receivedNum >= prop.rewardnum) alrealy.Add(infoList[i]);
            else
            {
                var have = moduleCangku.GetItemCount(infoList[i].itemTypeId);
                if (have > 0) selfCan.Add(infoList[i]);
                else selfNot.Add(infoList[i]);
            }
        }
        infoList.Clear();
        if (self != null) infoList.Add(self);
        infoList.AddRange(selfCan);
        infoList.AddRange(selfNot);
        infoList.AddRange(alrealy);
    }

    public void GetRelaseRecord(int type)
    {
        //ÿ�ε����ť����
        CsUnionClaimsRecord p = PacketObject.Create<CsUnionClaimsRecord>();
        p.type = (sbyte)type;
        session.Send(p);
    }
    void _Packet(ScUnionClaimsRecord p)
    {
        var pp = p.Clone();
        if (pp == null) return;
        var have = RecordType.Exists(a => a == pp.type);
        if (!have) RecordType.Add(pp.type);

        if (pp.type == 0) SetCordList(UnionRecived, pp.record);
        else if (pp.type == 1) SetCordList(FriendRecived, pp.record);
        else if (pp.type == 2) SetCordList(UnionGive, pp.record);
        else if (pp.type == 3) SetCordList(FriendGive, pp.record);

        DispatchModuleEvent(EventUnionClamisRecord, pp.type);
    }

    private void SetCordList(List<PReleaseRecord> record,PReleaseRecord[] release)
    {
        //ʱ���ֵԽ��Խ�����ִ�
        record.Clear();
        record.AddRange(release);
        record.Sort((a, b) => b.time.CompareTo(a.time));
    }

    public void ReleaseSelfTask(int itemTypeId)
    {
        CsUnionClaimsRelease p = PacketObject.Create<CsUnionClaimsRelease>();
        p.itemTypeId = itemTypeId;
        session.Send(p);
    }
    void _Packet(ScUnionClaimsRelease p)
    {
        if (p.result == 0)
        {
            moduleGlobal.ShowMessage(631, 20);
            DispatchModuleEvent(EventUnionRelaseSelf);
        }
        else if (p.result == 1) moduleGlobal.ShowMessage(631, 21);
        else if (p.result == 2) moduleGlobal.ShowMessage(631, 22);
        else if (p.result == 3) moduleGlobal.ShowMessage(631, 23);
    }

    public void GivePlayerItem(int type, ulong playerId,ushort itemTypeId,ulong leagueId)
    {
        CsUnionClaimsGive p = PacketObject.Create<CsUnionClaimsGive>();
        p.playerId = playerId;
        p.type = (sbyte)type;
        p.itemTypeId = itemTypeId;
        p.leagueID = leagueId;
        session.Send(p);
    }
    void _Packet(ScUnionClaimsGive p)
    {
        if (p.result == 0)
        {
            //����һ�������͵ļ�¼
            if (p.type == 0) AddClainsInfo(UnionClaimsInfo, UnionGive, p.playerId, p.type);
            else if (p.type == 1) AddClainsInfo(FriendClaimsInfo, FriendGive, p.playerId, p.type);

            var reward = PlayerReward(p.playerId);
            if (reward == null) return;
            var prop = ConfigManager.Get<PropItemInfo>(reward.itemTypeId);
            if (prop == null) return;
            moduleGlobal.ShowMessage(string.Format(ConfigText.GetDefalutString(631, 24), prop.itemName, prop.contribution));

            // ����һ��������Ϣ ˢ�¸��˵���Ϣ
            var str = string.Format(ConfigText.GetDefalutString(631, 26), modulePlayer.name_, prop.itemName, -1);
            str = str.Replace("[", "{");
            str = str.Replace("]", "}");
            moduleChat.SendFriendMessage(str, 0, p.playerId, 4);

            DispatchModuleEvent(EventUnionClamisGive);
        }
        else if (p.result == 1) moduleGlobal.ShowMessage(631, 33);
        else if (p.result == 2) moduleGlobal.ShowMessage(631, 25);
        else if (p.result == 3) moduleGlobal.ShowMessage(631, 34);
        else if (p.result == 4) moduleGlobal.ShowMessage(631, 35);
        else if (p.result == 5) moduleGlobal.ShowMessage(631, 36);
        else if (p.result == 6) moduleGlobal.ShowMessage(631, 38);
        else if (p.result == 7) moduleGlobal.ShowMessage(631, 39);

        if (p.result == 1 || p.result == 2 || p.result == 4 || p.result == 5 || p.result == 6 || p.result == 7)
        {
            GetRelaseInfo(p.type);
        }
    }
    void _Packet(ScUnionClaimsRecived p)
    {
        //����һ��˭�����ҵļ�¼ 
        if (p.type == 0)
        {
            AddClainsInfo(UnionClaimsInfo, UnionRecived, p.playerId, p.type + 2);
        }
        else if (p.type == 1)
        {
            AddClainsInfo(UnionClaimsInfo, FriendRecived, p.playerId, p.type + 2);
        }
        SelfClaims = UnionClaimsInfo.Find(a => a.playerId == modulePlayer.id_);
    }

    private void AddClainsInfo(List<PReleaseReward> claims, List<PReleaseRecord> rercord, ulong playerId, int type)
    {
        var claimsId = playerId;
        if (type >= 2) claimsId = modulePlayer.id_;
        var info = claims.Find(a => a.playerId == claimsId);
        if (info == null) return;

        var r = RecordInfo(playerId, info.itemTypeId);
        if (r == null || r?.playerInfo == null)
        {
            Logger.LogError("Give succed but not have this player in union or friend, id is {0}", playerId);
            return;
        }
        rercord.Insert(0, r);
        if (type < 2) info.receivedNum++;
    }
    
    private PReleaseRecord RecordInfo(ulong playerId,int itemTypeId)
    {
        PReleaseRecord p = PacketObject.Create<PReleaseRecord>();
        PPlayerInfo player = PlayerInfo(playerId);
        p.playerInfo = player;
        p.itemTypeId = itemTypeId;
        p.time = Util.GetTimeStamp(false, true);
        return p;
    }

    public PPlayerInfo PlayerInfo(ulong playerId)
    {
        PPlayerInfo player = m_unionPlayer.Find(a => a.info?.roleId == playerId)?.info;
        if (player == null) player = moduleFriend.FriendList.Find(a => a.roleId == playerId);
        return player;
    }
    public PReleaseReward PlayerReward(ulong playerId)
    {
        PReleaseReward reward = UnionClaimsInfo .Find(a => a.playerId == playerId);
        if (reward == null) reward = FriendClaimsInfo.Find(a => a.playerId == playerId);
        return reward;
    }

    #endregion

    void _Packet_998(ScRoleInfo p)
    {
        if (modulePlayer.roleInfo?.leagueID == 0) GetApplyTimes();
        else RequestUnionInfo();
    }

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        GetComposeType();
        SetNormalOpen();
    }
    private void GetComposeType()
    {
        ReleaseEquip.Clear();
        ReleaseRune.Clear();
        ReleaseOther.Clear();
        var prop = ConfigManager.GetAll<PropItemInfo>();
        for (int i = 0; i < prop.Count; i++)
        {
            if (prop[i] == null || prop[i]?.lableType != (int)WareType.Debris || prop[i]?.rewardnum <= 0) continue;
            if (!prop[i].IsValidVocation(modulePlayer.proto)) continue;
            if (prop[i].lableType == (int)WareType.Debris)
            {
                if (prop[i].subType > (int)DebrisSubType.None && prop[i].subType <= (int)DebrisSubType.DefendClothDebris)
                {
                    ReleaseEquip.Add(prop[i]);
                }
                else if (prop[i].subType >= (int)DebrisSubType.RuneDebrisOne && prop[i].subType <= (int)DebrisSubType.RuneDebrisSix)
                {
                    ReleaseRune.Add(prop[i]);
                }
                else ReleaseOther.Add(prop[i]);
            }
        }
    }

    private void InitClamis()
    {
        ReleaseTime = 0;
        SelfClaims = null;
        UnionClaimsInfo.Clear();
        UnionGive.Clear();
        FriendGive.Clear();
        UnionRecived.Clear();
        FriendRecived.Clear();
        RecordType.Clear();
    }
    private void SetNormalOpen()
    {
        InitClamis();
        CardSignInfo = null;
        m_selfInter = null;
        m_unionBaseInfo = null;
        m_unionPlayer.Clear();
        m_expendSentiment = 0;
        selectUnionList.Clear();
        m_refrshList.Clear();
        m_unionPlayer.Clear();
        m_unionApply.Clear();
        m_unionDynamic.Clear();
        m_chatChatSys.Clear();
        m_playerHurt.Clear();
        m_bossReward.Clear();
        ApplyUnionList.Clear();
        m_boxStae.Clear();
        BossBuffInfo.Clear();
        AnimationBox.Clear();
        m_isUnionBossTask = false;
        m_addChat = true;
        m_bossInfo = null;
        m_bossSet = null;
        m_bossStage = null;
        m_onlyInfo = null;
        OpenBossWindow = false;
        EnterBossTask = false;
        lastTime = 0;
        inUnion = -1;

    }

}

