using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDetailInfo : MonoBehaviour
{
    private GameObject m_headbg;
    private GameObject m_headBox;
    private Text m_name;
    private Text m_id;
    private Text m_level;
    private Text m_introduce;
    private Text m_contribute;
    private Text m_beNormal;
    private RectTransform m_isFriend;
    private RectTransform m_notFriend;
    private Button m_chatBtn;
    private Button m_compareBtn;
    private Button m_deleteBtn;
    private Button m_addBtn;

    private Button m_removeBtn;
    private Button m_presidentBtn;
    private Button m_vicePresidentBtn;
    private Button m_blackBtn;
    private Button m_removeBlack;

    public ulong PlayerId;

    void GetPath()
    {
        m_headbg = transform.Find("avatar/mask").gameObject;
        m_headBox = transform.Find("avatar").gameObject;
        m_name = transform.Find("infoList/name_Txt").GetComponent<Text>();
        m_id = transform.Find("infoList/id_Txt").GetComponent<Text>();
        m_level = transform.Find("infoList/level_Txt").GetComponent<Text>();
        m_contribute = transform.Find("infoList/contribute_Txt").GetComponent<Text>();

        m_introduce = transform.Find("individual_Txt").GetComponent<Text>();
        m_isFriend = transform.Find("isfriend").GetComponent<RectTransform>();
        m_notFriend = transform.Find("notfriend").GetComponent<RectTransform>();

        m_addBtn = transform.Find("notfriend/add_Btn").GetComponent<Button>();
        m_chatBtn = transform.Find("isfriend/chat_Btn").GetComponent<Button>();
        m_compareBtn = transform.Find("isfriend/compare_Btn").GetComponent<Button>();
        m_deleteBtn = transform.Find("isfriend/delete_Btn").GetComponent<Button>();
        m_blackBtn = transform.Find("button_group/blackBtn")?.GetComponent<Button>();
        m_removeBlack = transform.Find("button_group/removeBlack")?.GetComponent<Button>();
        m_removeBtn = transform.Find("button_group/remove_Btn").GetComponent<Button>();

        m_deleteBtn.gameObject.SetActive(false);
        m_compareBtn.gameObject.SetActive(false);

        m_presidentBtn = transform.Find("president_Btn").GetComponent<Button>();
        m_vicePresidentBtn = transform.Find("vicepresident_Btn").GetComponent<Button>();
        m_beNormal = transform.Find("vicepresident_Btn/delete_text").GetComponent<Text>();
    }

    public void PlayerDetailsInfo(PPlayerInfo details, long sent, int title)//加载玩家的详情
    {
        if (details == null) return;
        PlayerId = details.roleId;
        GetPath();

        m_addBtn.interactable = true;
        bool added = Module_Friend.instance.AddApplyID.Exists(a => a == details.roleId);
        var black = Module_Friend.instance.BlackList.Exists(a => a.roleId == details.roleId);
        if (added || black) m_addBtn.interactable = false;

        Module_Avatar.SetClassAvatar(m_headbg, details.proto, false, details.gender);
        headBoxFriend detaibox = m_headBox.GetComponentDefault<headBoxFriend>();
        detaibox.HeadBox(details.headBox);

        Util.SetText(m_name, details.name);
        Util.SetText(m_id, ConfigText.GetDefalutString(218, 36), details.index);
        Util.SetText(m_introduce, details.intro);
        string formatText = ConfigText.GetDefalutString(218, 2) + details.level.ToString();
        Util.SetText(m_level, formatText);
        string ss = ConfigText.GetDefalutString(242, 167) + sent;
        Util.SetText(m_contribute, ss);
        m_blackBtn.gameObject.SetActive(!black);
        m_removeBlack.gameObject.SetActive(black);

        bool isFriend = Module_Friend.instance.FriendList.Exists(a => a.roleId == details.roleId);
        if (isFriend)
        {
            m_isFriend.gameObject.SetActive(true);
            m_notFriend.gameObject.SetActive(false);
        }
        else
        {
            m_isFriend.gameObject.SetActive(false);
            m_notFriend.gameObject.SetActive(true);
        }

        SetSelfState(title);

        m_addBtn.onClick.RemoveAllListeners();
        m_chatBtn.onClick.RemoveAllListeners();
        m_addBtn.onClick.AddListener(delegate
        {
            var blacks = Module_Friend.instance.CanAddPlayer(details.roleId);
            if (blacks) return;
            Module_Friend.instance.SendAddMes(details.roleId);
            m_addBtn.interactable = false;
        });
        m_chatBtn.onClick.AddListener(delegate
        {
            //打开好友私聊界面
            Module_Union.instance.m_unionChatID = details.roleId;
            Module_Friend.instance.m_friendOpenType = OpenFriendType.Union;
            Window.ShowAsync("window_friend");
        });
        SetBtnClick(details.roleId, title);
    }
    public void SetBlackShow()
    {
        var black = Module_Friend.instance.BlackList.Find(a => a.roleId == PlayerId);
        m_blackBtn.gameObject.SetActive(black == null);
        m_removeBlack.gameObject.SetActive(black != null);
    }

    public void SetTitleNormal(int title)
    {
        GetPath();
        if (m_removeBtn == null || m_presidentBtn == null || m_vicePresidentBtn == null || m_beNormal == null)
        {
            Logger.LogError("this path is error{0},{1},{2},{3}", m_removeBtn, m_presidentBtn, m_vicePresidentBtn, m_beNormal);
            return;
        }
        SetSelfState(title);
        SetBtnClick(PlayerId, title);
    }
    private void SetSelfState(int title)
    {
        if (Module_Union.instance.inUnion == 0)//我的职位（0 会长1 副会长2普通）
        {
            m_removeBtn.gameObject.SetActive(true);
            m_presidentBtn.gameObject.SetActive(true);
            m_vicePresidentBtn.gameObject.SetActive(true);
            Util.SetText(m_beNormal, 242, 37);
            if (title == 1) m_beNormal.text = ConfigText.GetDefalutString(242, 168);
        }
        else if (Module_Union.instance.inUnion == 1)
        {
            if (title == 0 || title == 1)
            {
                m_removeBtn.gameObject.SetActive(false);
                m_presidentBtn.gameObject.SetActive(false);
                m_vicePresidentBtn.gameObject.SetActive(false);
            }
            else m_removeBtn.gameObject.SetActive(true);
        }
        else
        {
            m_removeBtn.gameObject.SetActive(false);
            m_presidentBtn.gameObject.SetActive(false);
            m_vicePresidentBtn.gameObject.SetActive(false);
        }
    }

    private void SetBtnClick(ulong id, int title)
    {
        m_removeBtn.onClick.RemoveAllListeners();
        m_presidentBtn.onClick.RemoveAllListeners();
        m_vicePresidentBtn.onClick.RemoveAllListeners();
        m_blackBtn?.onClick.RemoveAllListeners();
        m_removeBlack?.onClick.RemoveAllListeners();

        m_blackBtn?.onClick.AddListener(delegate
        {
            if (Module_Active.instance.CheckCoop(id)) Module_Global.instance.ShowMessage(218, 63);
            else Window_Alert.ShowAlert(ConfigText.GetDefalutString(218, 65), true, true, true, () => { Module_Friend.instance.SendShieDing(id); }, null, "", "");
        });
        m_removeBtn.onClick.AddListener(delegate
        {
            Window_Alert.ShowAlert(ConfigText.GetDefalutString(242, 169), true, true, true, () => { Module_Union.instance.KickedPlayer(id); }, null, "", "");
        });
        m_presidentBtn.onClick.AddListener(delegate
        {
            Window_Alert.ShowAlert(ConfigText.GetDefalutString(242, 170), true, true, true, () => { Module_Union.instance.ChangeTitle((long)id, 0); }, null, "", "");
        });
        m_vicePresidentBtn.onClick.AddListener(delegate
        {
            if (title == 1)
            {
                Window_Alert.ShowAlert(ConfigText.GetDefalutString(242, 171), true, true, true, () => { Module_Union.instance.ChangeTitle((long)id, 2); }, null, "", "");
            }
            else if (title == 2)
            {
                Window_Alert.ShowAlert(ConfigText.GetDefalutString(242, 172), true, true, true, () => { Module_Union.instance.ChangeTitle((long)id, 1); }, null, "", "");
            }
        });
        m_removeBlack?.onClick.AddListener(delegate
        {
            Window_Alert.ShowAlert(ConfigText.GetDefalutString(218, 66), true, true, true, () => { Module_Friend.instance.RecoverShieDing(id); }, null, "", "");
        });
    }
}

