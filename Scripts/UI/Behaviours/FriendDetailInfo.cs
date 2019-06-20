using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendDetailInfo : MonoBehaviour
{
    private GameObject details_headbg;
    private GameObject Details_box;
    private Text details_name;
    private Text details_id;
    private Text details_level;
    private Text details_introduce;
    private Button is_chat;
    private Button is_compare;
    private Button is_delete;
    private Button not_add;
    private Button m_invateUnion;//工会邀请
    private Button m_blackBtn;//拉黑按钮
    private Button m_removeBtn;//移除按钮

    private Action<int, string, ulong> m_invate;//公会邀请事件
    public ulong PlayerId;
    // Use this for initialization
    void Get()
    {
        details_headbg = transform.Find("Rawback/bg/mask").gameObject;
        Details_box = transform.Find("Rawback/bg").gameObject;
        details_name = transform.Find("Rawback/name").GetComponent<Text>();
        details_id = transform.Find("Rawback/id").GetComponent<Text>();
        details_level = transform.Find("Rawback/level").GetComponent<Text>();
        details_introduce = transform.Find("Rawback/individual/Text").GetComponent<Text>();

        is_chat = transform.Find("Rawback/button_group/chat").GetComponent<Button>();
        is_compare = transform.Find("Rawback/button_group/compare").GetComponent<Button>();
        is_delete = transform.Find("Rawback/button_group/delete").GetComponent<Button>();
        not_add = transform.Find("Rawback/button_group/add").GetComponent<Button>();
        m_invateUnion = transform.Find("Rawback/button_group/invite").GetComponent<Button>();
        m_blackBtn = transform.Find("Rawback/button_group/blackBtn")?.GetComponent<Button>();
        m_removeBtn = transform.Find("Rawback/button_group/removeBtn")?.GetComponent<Button>();

        SetText();
    }

    private void SetText()
    {
        Util.SetText(transform.Find("Rawback/button_group/removeBtn/delete_text").GetComponent<Text>(), 218, 68);
        Util.SetText(transform.Find("Rawback/button_group/blackBtn/delete_text").GetComponent<Text>(), 218, 67);
        Util.SetText(transform.Find("Rawback/button_group/chat/chat_text").GetComponent<Text>(), 218, 27);
        Util.SetText(transform.Find("Rawback/button_group/add/add_text").GetComponent<Text>(), 218, 28);
        Util.SetText(transform.Find("Rawback/button_group/delete/delete_text").GetComponent<Text>(), 218, 24);
        Util.SetText(transform.Find("Rawback/button_group/compare/compare_text").GetComponent<Text>(), 218, 26);
        Util.SetText(transform.Find("bg/zhi_text").GetComponent<Text>(), 221, 38);
    }
    public void IsfriendDetails(PPlayerInfo playerInfo, bool world = false, Action<int, string, ulong> invate = null)
    {
        if (playerInfo == null) return;

        Get();
        m_invate = invate;
        PlayerId = playerInfo.roleId;

        //isMyfriend 0 好友 1 不是好友但是显示添加 2 黑名单
        SetButtonShow(world);

        //在这加载玩家的详情
        PlayerDetailsInfo(playerInfo, world);
    }

    public void SetButtonShow(bool world)
    {
        if (is_chat == null) Get();
        var type = Module_Friend.instance.PlayerType(PlayerId);

        is_chat.SafeSetActive(type == 0 && world);
        is_compare.SafeSetActive(type == 0);
        is_delete.SafeSetActive(type == 0);
        not_add.SafeSetActive(type != 0 && type != 2);
        m_blackBtn.SafeSetActive(type != 2);
        m_removeBtn.SafeSetActive(type == 2);

        m_invateUnion.SafeSetActive(false);
        if (type == 0 && m_invate != null && Module_Player.instance.roleInfo.leagueID != 0 && Module_Union.instance.UnionBaseInfo != null)
        {
            var union = Module_Union.instance.m_unionPlayer.Exists(a => a.info?.roleId == PlayerId);
            if (union) m_invateUnion.gameObject.SetActive(true);
            else Module_Friend.instance.CheckFriendUnion(PlayerId);
        }
        var add = Module_Friend.instance.AddApplyID.Exists(a => a == PlayerId);
        not_add.interactable = add || type == 2 ? false : true;

    }

    private void PlayerDetailsInfo(PPlayerInfo details, bool world)//加载玩家的详情
    {
        if (details == null) return;
        Module_Avatar.SetClassAvatar(details_headbg, details.proto, false, details.gender);
        headBoxFriend detaibox = Details_box.GetComponentDefault<headBoxFriend>();
        detaibox.HeadBox(details.headBox);
        is_compare.interactable = true;
        Util.SetText(details_name, details.name);
        Util.SetText(details_id, ConfigText.GetDefalutString(218, 36), details.index);
        Util.SetText(details_introduce, Module_Set.instance.SetSignTxt(details.intro));
        string formatText = ConfigText.GetDefalutString(218, 2) + details.level.ToString();
        Util.SetText(details_level, formatText);

        is_compare.onClick.RemoveAllListeners();
        is_compare.onClick.AddListener(delegate
        {
            var canshow = Module_Guide.instance.IsActiveFunction(HomeIcons.Fight);
            if (canshow) canshow = Module_Guide.instance.IsActiveFunction(HomeIcons.PVP);
            if (!canshow)
            {
                Module_Global.instance.ShowMessage(ConfigText.GetDefalutString(223, 12));
                return;
            }

            int remain = Module_Match.instance.CanInvation(details.roleId);

            if (remain > 0)
            {
                Module_Global.instance.ShowMessage(ConfigText.GetDefalutString(218, 51));
                return;
            }
            if (details.state == 1)
            {
                Module_PVP.instance.opType = OpenWhichPvP.FriendPvP;
                Window.ShowAsync("window_pvp");

                List<ulong> this_F = new List<ulong>();
                this_F.Add(details.roleId);

                Module_Match.instance.FriendFree(this_F);

                var inKey = "invation" + PlayerId;
                PlayerPrefs.SetString(inKey, Util.GetServerLocalTime().ToString());
            }
            else if (details.state == 0) Module_Global.instance.ShowMessage(218, 46);
            else Module_Global.instance.ShowMessage(218, 47);

        });
        
        m_invateUnion.onClick.RemoveAllListeners();
        m_invateUnion.onClick.AddListener(delegate
        {
            if (Module_Friend.instance.CanInviteUnion == 0)
            {
                Window_Alert.ShowAlert(ConfigText.GetDefalutString(248, 2), true, true, true, () => { SetInvate(); }, null, "", "");
            }
            else if (Module_Friend.instance.CanInviteUnion == 1) Module_Global.instance.ShowMessage(218, 55);
            else if (Module_Friend.instance.CanInviteUnion == 2) Module_Global.instance.ShowMessage(218, 57);
            else if (Module_Friend.instance.CanInviteUnion == 3) Module_Global.instance.ShowMessage(218, 56);
        });

        is_delete.onClick.RemoveAllListeners();
        not_add.onClick.RemoveAllListeners();
        m_blackBtn?.onClick.RemoveAllListeners();
        m_removeBtn?.onClick.RemoveAllListeners();
        m_blackBtn?.onClick.AddListener(SetBLack);
        m_removeBtn?.onClick.AddListener(RemoveBlack);
        is_delete.onClick.AddListener(SetDelete);
        not_add.onClick.AddListener(DeltailAdd);
        
        if (world)
        {
            is_compare.SafeSetActive(false);
            is_chat.onClick.RemoveAllListeners();
            is_chat.onClick.AddListener(delegate
            {
                gameObject.SafeSetActive(false);
                Module_Friend.instance.m_friendOpenType = OpenFriendType.World;
                Window.ShowAsync("window_friend");
            });
        }
    }

    private void DeltailAdd()
    {
        var blacks = Module_Friend.instance.CanAddPlayer(PlayerId);
        if (blacks) return;
        if (Module_Friend.instance.FriendList.Count < 80)
        {
            Module_Friend.instance.SendAddMes(PlayerId);
            not_add.SetInteractable(false);
        }
        else Module_Global.instance.ShowMessage(ConfigText.GetDefalutString(218, 13));//提示 好友已满
    }

    private void SetDelete()
    {
        if (Module_Active.instance.CheckCoop(PlayerId)) Module_Global.instance.ShowMessage(218, 63);
        else Window_Alert.ShowAlert(ConfigText.GetDefalutString(218, 64), true, true, true, () => { Module_Friend.instance.SendDeleteMes(PlayerId); }, null, "", "");
    }
    private void SetBLack()
    {
        if (Module_Active.instance.CheckCoop(PlayerId)) Module_Global.instance.ShowMessage(218, 63);
        else Window_Alert.ShowAlert(ConfigText.GetDefalutString(218, 65), true, true, true, () => { Module_Friend.instance.SendShieDing(PlayerId); }, null, "", "");
    }
    private void RemoveBlack()
    {
        Window_Alert.ShowAlert(ConfigText.GetDefalutString(218, 66), true, true, true, () => { Module_Friend.instance.RecoverShieDing(PlayerId); }, null, "", "");
    }

    private void SetInvate()
    {
        if (Module_Player.instance.roleInfo.leagueID == 0)
        {
            Module_Global.instance.ShowMessage(ConfigText.GetDefalutString(242, 197));
            m_invateUnion.gameObject.SetActive(false);
            return;
        }

        if (m_invate != null && PlayerId != 0)
        {
            var key = Util.Format("un:{0}:{1}", Module_Union.instance.UnionBaseInfo.unionname, Module_Player.instance.roleInfo.leagueID.ToString());
            var cont = ConfigText.GetDefalutString(248, 0) + key + "}" + Module_Union.instance.UnionBaseInfo.unionname + ConfigText.GetDefalutString(248, 1);

            m_invate(0, cont, PlayerId);
        }
    }
}
