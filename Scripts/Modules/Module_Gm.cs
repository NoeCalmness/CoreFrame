/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-10-14
 * 
 ***************************************************************************************************/

public class Module_Gm : Module<Module_Gm>
{
    public void SendAddRune(ushort runeId,int starLv,int level,int num)
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        string str = Util.Format("rune {0} {1}s {2}l {3}",runeId,starLv,level,num);
        gm.gmStr = str;
        session.Send(gm);
    }

    public void SendAddProp(ushort propId,int num)
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        string str = Util.Format("item {0} {1}", propId, num);
        gm.gmStr = str;
        session.Send(gm);
    }

    public void SendAddMoney(int type,int num)
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        string str = Util.Format("money {0} {1}", type, num);
        gm.gmStr = str;
        session.Send(gm);
    }

    public void AddProp(ushort propId, int num)
    {
        var prop = ConfigManager.Get<PropItemInfo>(propId);
        if (null == prop) return;
        if (prop.itemType == PropType.Currency)
            moduleGm.SendAddMoney(propId, num);
        else
            moduleGm.SendAddProp(propId, num);
    }

    public void SendAddExp(uint num)
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        string str = Util.Format("expr {0}", num);
        gm.gmStr = str;
        session.Send(gm);
    }

    public void SendChangeLv(int num)
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        string str = Util.Format("lv {0}", num);
        gm.gmStr = str;
        session.Send(gm);
    }

    public void SendCleanBag()
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = "clean_bag";
        session.Send(gm);
    }

    public void SendMazeNext()
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = "maze_next";
        session.Send(gm);

        moduleLabyrinth.SendLabyrinthOpenTime();
    }

    public void SendSetMazeHp(uint hp)
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        string str = Util.Format("maze_hp {0}", hp);
        gm.gmStr = str;
        session.Send(gm);
    }

    public void SendCleanToday()
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = "clean_daily";
        session.Send(gm);
    }

    public void SendRefreshAllShop()
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = "shop_refresh";
        session.Send(gm);
    }

    public void SendRefreshShop(int shopId)
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        string str = Util.Format("shop_refresh {0}", shopId);
        gm.gmStr = str;
        session.Send(gm);
    }

    public void SendLabyrinthMail(string lv,string promotion)
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        string str = Util.Format("maze_mail {0} {1}",lv,promotion);
        gm.gmStr = str;
        session.Send(gm);
    }

    public void SendAddScore(string score)
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        string str = Util.Format("rank_score {0}", score);
        gm.gmStr = str;
        session.Send(gm);
    }

    public void SendSentiment(string sent)
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        string str = Util.Format("union_sent {0}", sent);
        gm.gmStr = str;
        session.Send(gm);
    }

    public void SendLolMail()
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = "rank_settlement";
        session.Send(gm);
    }

    public void SendUnlockAllChase(int level)
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        string str = Util.Format("unlock_all_stage {0}", level);
        gm.gmStr = str;
        session.Send(gm);
    }

    public void SendCharge(int rProductId)
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        string str = $"recharge {rProductId}";
        gm.gmStr = str;
        session.Send(gm);
    }

    public void SendTeamPveTimes(int type)
    {
        type = type != 2 && type != 3 ? 2 : type;

        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        string str = Util.Format("reset_team_pve_times {0}", type);
        gm.gmStr = str;
        session.Send(gm);
    }

    public void SendActivePveTimes()
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = "reset_active_pve_times";
        session.Send(gm);
    }

    public void SendSpriteTrain()
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = "reset_sprite";
        session.Send(gm);
    }

    public void SendFullAnger()
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = "full_anger";
        session.Send(gm);
    }

    public void SendResetDatingState()
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = "reset_npc_engagement";
        session.Send(gm);
    }

    public void SendAddNpcMood(int mood)
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        string str = Util.Format("add_npc_engagement_mood {0}", mood);
        gm.gmStr = str;
        session.Send(gm);
    }

    public void SendEndNpcEngagement()
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = "end_npc_engagement";
        session.Send(gm);
    }

    public void SendAddNpcExp(int npcId, int value)
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        string str = Util.Format("add_npc_exp {0} {1}", npcId,  value);
        gm.gmStr = str;
        session.Send(gm);
    }

    public void SendUnLockAllNpcDating()
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = "unlock_all_npc_engagement";
        session.Send(gm);
    }

    public void SendRestSignTime()
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = "union_sign_time";
        session.Send(gm);
    }
    public void SendRestUnionCard(string str)
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = string.Format("union_card{0}", str);
        session.Send(gm);
    }
    public void SendRestUnionReward()
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = "union_reward_time";
        session.Send(gm);
    }
    public void SendRestActiveCoop()
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = "coop_refresh";
        session.Send(gm);
    }
    public void SendSpecifiedActiveCoop(string str)
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = string.Format("coop_specified{0}", str);
        session.Send(gm);
    }

    public void SendResetAwakeStage()
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = "reset_awake_stage";
        session.Send(gm);
    }

    public void SendResetNpcStage()
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = "reset_npc_stage";
        session.Send(gm);
    }

    public void SendResetNpcExp()
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = "clean_npc_list";
        session.Send(gm);
    }

    public void SendGMCmdStr(string cmdStr)
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = cmdStr;
        session.Send(gm);
    }

    public void DuplicateRole(string roleName, string accName = "")
    {
        CsCopyRole p = PacketObject.Create<CsCopyRole>();
        p.roleName = roleName;
        p.accName = accName;
        session.Send(p);
    }
    public void RestWelfareIndo(string wId)
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        gm.gmStr = string.Format("clean_act_data{0}", wId);
        session.Send(gm);
    }

    public void UnLockNpcPledge(int npcId)
    {
        CsSystemGm gm = PacketObject.Create<CsSystemGm>();
        string str = Util.Format("unlock_npc_pledge {0}", npcId);
        gm.gmStr = str;
        session.Send(gm);
    }

    private void _Packet(ScCopyRole p)
    {
        if (p.result == 1)
        {
            Logger.LogError("Failed to duplicate role");
        }
        else
        {
            Logger.LogDetail("Successful duplicate role");
        }

    }
}
