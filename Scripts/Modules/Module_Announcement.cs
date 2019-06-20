/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-05-08
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

public class Module_Announcement : Module<Module_Announcement>
{
    /// <summary>
    /// ������ַ
    /// </summary>
    public static string officialURL = "http://kzwg.jingzheshiji.com/site/index/index";
    public const string EventAccAllInfo = "EventAccAllInfo";
    public const string EventAccNewNotice = "EventAccNewNotice";
    public const string EventAccCloseNotice = "EventAccCloseNotice";

    public List<PBulletin> ActiveList { get { return activeList; } }

    public List<PBulletin> NoticeList { get { return noticeList; } }

    public List<int> NotClick { get { return notClick; } }

    /// <summary>
    /// ��ǰѡ�еĻ
    /// </summary>
    public PBulletin PInfo { get; set; }

    private List<PBulletin> activeList = new List<PBulletin>();
    private List<PBulletin> noticeList = new List<PBulletin>();
    private List<int> notClick = new List<int>();//δ�����

    protected override void OnEnterGame()
    {
        base.OnEnterGame();
        activeList.Clear();
        noticeList.Clear();
        notClick.Clear();
        GetAllBulletin();
    }

    private void GetAllBulletin()
    {
        //��������������еĹ���
        CsSystemBulletin p = PacketObject.Create<CsSystemBulletin>();
        session.Send(p);
    }

    void _Packet(ScSystemBulletin p)//����������Ϣ
    {
        ScSystemBulletin Info = p.Clone();
        if (Info == null || Info?.bulletins == null) return;

        PInfo = null;
        activeList.Clear();
        noticeList.Clear();
        notClick.Clear();

        for (int i = 0; i < Info.bulletins.Length; i++)
        {
            PBulletin thisInfo = Info.bulletins[i];
            if (thisInfo != null)
            {
                if (thisInfo.type == 0) activeList.Add(thisInfo);
                else if (thisInfo.type == 1) noticeList.Add(thisInfo);
            }
        }
        
        LocalSelect(noticeList);
        LocalSelect(activeList);
        if (activeList.Count > 0)  PInfo = ActiveList[0];
        DispatchModuleEvent(EventAccAllInfo);
        if (notClick.Count > 0) moduleHome.UpdateIconState(HomeIcons.Notice, true);
    }

    void _Packet(ScSystemNewBulletin p)//���߽ӵ�������
    {
        PBulletin info = p.Clone().bulletin;

        //����µĹ�����Ż����
        if (info == null) return;
        if (info.type == 0)
        {
            var active = activeList.Find(a => a.id == info.id);
            if (active != null) activeList.Remove(active);
            activeList.Add(info);
        }
        else if (info.type == 1)
        {
            var notice = noticeList.Find(a => a.id == info.id);
            if (notice != null) noticeList.Remove(notice);
            noticeList.Add(info);
        }
        var click = notClick.Exists(a => a == info.id);
        if (click) notClick.Remove(info.id);
        if (info.menuHot != 0)
        {
            notClick.Add(info.id);
            moduleHome.UpdateIconState(HomeIcons.Notice, true);
        }
        DispatchModuleEvent(EventAccNewNotice, info.type);
    }

    void _Packet(ScSystemBulletinExpire p)//����ر�
    {
        InitClickInfo(p.bulletinId);
        bool have = notClick.Exists(a => a == p.bulletinId);
        if (have) notClick.Remove(p.bulletinId);
        PBulletin Ahave = activeList.Find(a => a.id == p.bulletinId);
        if (Ahave != null)
        {
            activeList.Remove(Ahave);
            DispatchModuleEvent(EventAccCloseNotice, 0);
        }
        PBulletin Nhave = noticeList.Find(a => a.id == p.bulletinId);
        if (Nhave != null)
        {
            noticeList.Remove(Nhave);
            DispatchModuleEvent(EventAccCloseNotice, 1);
        }
        if (notClick.Count <= 0) moduleHome.UpdateIconState(HomeIcons.Notice, false);
    }

    private void InitClickInfo(int infoId)
    {
        if (PInfo == null) return;
        if (PInfo.id == infoId) PInfo = null;
    }
    
    private void LocalSelect(List<PBulletin> list)//�鿴�����Ƿ�鿴���û
    {
        //�ӷ�����������еĻ�б�
        for (int i = 0; i < list.Count; i++)
        {
            int type = list[i].menuHot;//0 ����ʾ 1 ������ʾ 2 һ��ʱ����ʾ 
            if (type != 0)
            {
                string ids = "ment" + list[i].id.ToString() + modulePlayer.roleInfo.roleId.ToString();
                string tt = PlayerPrefs.GetString(ids);
                if (tt == null || tt == "") notClick.Add(list[i].id);
                else
                {
                    //�����ǵ��컹��һ��ʱ�䶼Ҫ���ÿ�ʼ���������ж�
                    DateTime dtnow = DateTime.Parse(tt);
                    DateTime dtend = Util.GetDateTime(list[i].endTime);
                    DateTime dtopen = Util.GetDateTime(list[i].openTime);

                    if (DateTime.Compare(dtnow, dtopen) < 0 || DateTime.Compare(dtnow, dtend) > 0)
                    {
                        notClick.Add(list[i].id);
                    }
                }
            }
        }
    }

    public void OpenWindow(int type, int lable = -1)//����typeǰ����ͬ�Ľ���
    {
        //lable ���ô�0��ʼ
        if (type <= 0) return;
        bool canshow = true;

        switch (type)
        {
            case 1:
                //ǩ��
                canshow = moduleGuide.IsActiveFunction(HomeIcons.Welfare);
                if (canshow)
                {
                    var index = lable;
                    if (lable == 0) index = moduleWelfare.allWeflarInfo.FindIndex(a => a.type == (int)WelfareType.Sign);
                    else
                    {
                        index = moduleWelfare.allWeflarInfo.FindIndex(a => a.id == lable);
                        if (index == -1) index = moduleWelfare.puzzleList.FindIndex(a => a.id == lable);
                        if (index == -1) index = moduleWelfare.allChargeInfo.FindIndex(a => a.id == lable);
                    }
                    if (index == -1) moduleGlobal.ShowMessage(233, 3);
                    else Window.GotoSubWindow("window_welfare", lable);
                }
                else moduleGlobal.ShowMessage(223, 12);
                break;
            case 2: SkipWindow(canshow, "window_mailbox", lable, 2); break;//�ʼ�
            case 3: SkipWindow(canshow, "window_announcement", lable, 3); break;//����
            case 4:
                //����
                moduleActive.ActiveClick = 0;
                if (lable == 2) canshow = moduleGuide.IsActiveFunction(125);
                SkipWindow(canshow, "window_active", lable, 4);
                break;
            case 6:
                //����
                moduleFriend.m_friendOpenType = OpenFriendType.Normal;
                SkipWindow(canshow, "window_friend", lable, 6);
                break;
            case 7: SkipWindow(canshow, "window_chat", lable, 7); break; //����
            case 8: SkipWindow(canshow, "window_attribute", lable, 8); break; //����
            case 9:
                //װ��
                Module_Equip.selectEquipType = EnumSubEquipWindowType.MainPanel;
                if (lable != -1) Module_Equip.selectEquipType = (EnumSubEquipWindowType)lable;
                SkipWindow(canshow, "window_equip", lable, 9);
                break;
            case 10: SkipWindow(canshow, "window_runeStart", lable, 10); break;//����
            case 11:
                //�ֿ�
                moduleCangku.chickType = WareType.Prop;
                SkipWindow(canshow, "window_cangku", lable, 11);
                break;
            case 15:
                //����
                if (lable == -1) canshow = moduleGuide.IsActiveFunction(15);
                else canshow = moduleChase.CanInCurType((TaskType)lable);
                SkipWindow(canshow, "window_chase", lable);
                break;
            case 16:
                //ѵ����
                canshow = moduleGuide.IsActiveFunction(HomeIcons.Fight);//����
                if (canshow) canshow = moduleGuide.IsActiveFunction(HomeIcons.Train);

                if (canshow) Game.LoadLevel(GeneralConfigInfo.sTrainLevel);
                else moduleGlobal.ShowMessage(223, 12);
                break;
            case 17:
                //��ͷ����
                canshow = moduleGuide.IsActiveFunction(HomeIcons.Fight);
                if (canshow) canshow = moduleGuide.IsActiveFunction(HomeIcons.PVP);
                modulePVP.opType = OpenWhichPvP.FreePvP;
                SkipWindow(canshow, "window_pvp", lable);
                break;
            case 18:
                //�ʼҶ���  ��λ�� ��Ҫ�ж��Ƿ��ڻ�ڼ� ������ʾ
                canshow = moduleGuide.IsActiveFunction(HomeIcons.Fight);
                if (canshow) canshow = moduleGuide.IsActiveFunction(HomeIcons.Match);

                if (canshow)
                {
                    if (!moduleGlobal.IsStayLoLTime()) moduleGlobal.ShowMessage(223, 13);
                    else
                    {
                        modulePVP.opType = OpenWhichPvP.LolPvP;
                        SkipWindow(canshow, "window_pvp", lable);
                    };
                }
                else moduleGlobal.ShowMessage(223, 12);
                break;
            case 19:
                //�Թ� �Ƿ��ܽ� ������ʾ
                canshow = moduleGuide.IsActiveFunction(HomeIcons.Dungeon);
                if (canshow) canshow = moduleGuide.IsActiveFunction(HomeIcons.Labyrinth);
                if (canshow) moduleLabyrinth.SendLabyrinthEnter();
                else moduleGlobal.ShowMessage(223, 12);
                break;
            case 20:
                //����֮��
                canshow = moduleGuide.IsActiveFunction(HomeIcons.Dungeon);
                if (canshow) canshow = moduleGuide.IsActiveFunction(HomeIcons.Bordlands);
                if (canshow) moduleBordlands.Enter();
                else moduleGlobal.ShowMessage(223, 12);
                break;
            case 21:
                //����
                canshow = moduleGuide.IsActiveFunction(HomeIcons.Shop);
                SkipWindow(canshow, "window_forging", lable, 21);
                break;
            case 22:
                //ʱװ�̵�
                canshow = moduleGuide.IsActiveFunction(HomeIcons.Shop);
                SkipWindow(canshow, "window_shizhuangdian", lable, 22);
                break;
            case 23:
                //�����̵�
                canshow = moduleGuide.IsActiveFunction(HomeIcons.Shop);
                SkipWindow(canshow, "window_liulangshangdian", lable, 23);
                break;
            case 24:
                //��Ը��
                canshow = moduleGuide.IsActiveFunction(HomeIcons.Shop);
                SkipWindow(canshow, "window_wish", lable, 24);
                break;
            case 25: SkipWindow(canshow, "window_sprite", lable, 25); break;//����
            case 26:
                //��ȡ����
                canshow = moduleGuide.IsActiveFunction(HomeIcons.Shop);
                SkipWindow(canshow, "window_summon", lable, 26);
                break;
            case 27: SkipWindow(canshow, "window_skill", lable, 27); break;//����
            case 28: SkipWindow(canshow, "window_awakeinit", lable, 28); break;//����(�Ļ�)
            case 29:
                //����
                canshow = moduleGuide.IsActiveFunction(HomeIcons.Guild);
                moduleUnion.OpenBossWindow = false;
                Window_Union w = Window.GetOpenedWindow<Window_Union>();
                if (w && w.actived)
                {
                    Window_Unionboss boss = Window.GetOpenedWindow<Window_Unionboss>();
                    if (boss && boss.actived) return;
                    Window.ShowAsync<Window_Unionboss>();
                    moduleChat.HideChatWindow();
                }
                else
                {
                    if (lable != -1) moduleUnion.OpenBossWindow = true;
                    SkipWindow(canshow, "window_union", lable);
                }
                break;
            case 30:
                //��ɪ��ķ
                canshow = moduleGuide.IsActiveFunction(HomeIcons.Dungeon) && moduleGuide.IsActiveFunction(HomeIcons.PetCustom);
                if (canshow) Window_Home.Show(Window_Home.Dungeon, HomeIcons.PetCustom);
                else moduleGlobal.ShowMessage(223, 12);
                break;
            case 31: SkipWindow(canshow, "window_goodFeeling", lable, 31); break;//Լ��
            case 32: SkipWindow(canshow, "window_charge", lable, 32); break;//��ֵ
            case 50: Skip(type.ToString()); break;
            case 51: SkipWindow(canshow, "window_unionreward", lable, 51); break;//�������ͽ���
            case 103:
                //��������
                canshow = moduleGuide.IsActiveFunction(101);
                SkipWindow(canshow, "window_train", lable, 103);
                break;
            case 105:
                //�������ѡ��
                canshow = moduleGuide.IsActiveFunction(HomeIcons.Attack);
                SkipWindow(canshow, "window_awaketeam", lable, 105);
                break;
            case 116:
                //npc�⴫
                canshow = moduleGuide.IsActiveFunction(HomeIcons.Attack);
                SkipWindow(canshow, "window_npcgaiden", lable, 116);
                break;
        }
    }

    public void SkipWindow(bool isUnLock, string wName, int _subtype, int lockId = -1)
    {
        if (isUnLock && lockId != -1) isUnLock = moduleGuide.IsActiveFunction(lockId);
        if (isUnLock)
        {
            Window.GotoSubWindow(wName, _subtype);
            moduleChat.HideChatWindow();
        }
        else moduleGlobal.ShowMessage(223, 12);
    }

    #region  ��ת�ٷ���ַ

    public void Skip(string name)
    {
        CPlatfromFactory cp = new CPlatfromFactory();
        IPlatformUpdate iu = cp.CreatePlatfrom(name);
        iu.DoPlatformUpdate();
    }

    public interface IPlatformUpdate
    {
        void DoPlatformUpdate();
    }
    public class CPlatfromFactory
    {
        public IPlatformUpdate CreatePlatfrom(string platfrom)
        {
            IPlatformUpdate platform = null;
            if (platfrom.Equals("50"))
            {
                platform = new CEXEUpdate();
            }
            return platform;
        }
    }
    public class CEXEUpdate : IPlatformUpdate
    {
        public void DoPlatformUpdate()
        {
            Application.OpenURL(officialURL);
        }
    }

    #endregion 
}


