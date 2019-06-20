using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chatclick : MonoBehaviour
{
    private Button textBtn;
    private int clicktype;

    void Start()
    {
        textBtn = transform.parent.GetComponentDefault<Button>();
        var textPic = gameObject.GetComponentDefault<Chathyperlink>();
        textBtn.enabled = false;
        if (textPic == null || string.IsNullOrEmpty(textPic.Value())) return;
        textBtn.enabled = true;
        textBtn.onClick.RemoveAllListeners();
        textBtn.onClick.AddListener(() => { Click(textPic.Value()); });
    }

    private void Click(string hrefName)
    {
        //Module_Bordlands.instance.Enter();
        string[] types = hrefName.Split(':');
        if (types.Length == 2)
        {
            if (types[0] == "s")//场景
            {
                int typeid = Util.Parse<int>(types[1]);
                Module_Announcement.instance.OpenWindow(typeid);
            }
            else if (types[0] == "p")//道具
            {
                Module_Global.instance.UpdateGlobalTip(ushort.Parse(types[1]), true);
            }
            else if (types[0] == "w")//网址
            {
                Module_Announcement.instance.Skip(types[1]);
            }
            else if (types[0] == "un")
            {
                if (Module_Player.instance.roleInfo?.leagueID == 0)
                {
                    Module_Global.instance.ShowMessage(631, 27);
                    Module_Announcement.instance.OpenWindow((int)HomeIcons.Guild);
                }
                else Module_Announcement.instance.OpenWindow(51);
            }
            else
            {
                Logger.LogError("ChatClick drop type {0} is error ", types[0]);
            }
        }
        else if (types.Length == 3)//un:name:id
        {
            if (types[0] == "s")//场景
            {
                var typeid = Util.Parse<int>(types[1]);
                var tableId = Util.Parse<int>(types[2]);
                Module_Announcement.instance.OpenWindow(typeid, tableId);
            }
            if (types[0] == "un")//是公会邀请
            {
                Module_Union.instance.SelectUnion(types[2], 1);
            }
            else if (types[0] == "coop")
            {
                //发送同意协作任务邀请
                Module_Active.instance.SendCoopInvateApply(Util.Parse<ulong>(types[1]));
            }
        }
        else if (types.Length > 0)
        {
            if (types[0] == "ac")
            {
                if (types.Length == 5)
                    Module_AwakeMatch.instance.Request_Agree(Util.Parse<int>(types[1]), Util.Parse<ulong>(types[2]), Util.Parse<ulong>(types[3]), false, types[4]);
                else if (types.Length == 6)
                    Module_AwakeMatch.instance.Request_Agree(Util.Parse<int>(types[1]), Util.Parse<ulong>(types[2]), Util.Parse<ulong>(types[3]), Util.Parse<int>(types[5]) > 0, types[4]);
            }
        }
        else
        {
            Logger.LogError("loss ;");
        }
    }

}
