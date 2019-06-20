using System;
using UnityEngine;
using UnityEngine.UI;

public class FriendPrecast : AssertOnceBehaviour, IScrollViewData<ISourceItem>
{
    private Text name_friend;
    private Text level_friend;
    private Text ID_friend;
    private Text goodFeelingValue;
    private Image isonline_friend;
    private Image offonline_friend;
    private Image busy_friend;
    private Transform friend_hint;
    private GameObject Head_img;
    private GameObject checkObj;//选中高亮
    private Toggle toggle;
    private Transform isFriend;
    private Transform isUnionMember;
    private Transform isStranger;

    private headBoxFriend friebdBox;
    private ConfigText friend_txt;

    private bool isopen = false;
    private PPlayerInfo m_playerInfo;
    private SourceFriendInfo m_Source;
    private int m_index;
    private int m_type;
    private int m_sub;

    public Action<FriendPrecast, bool> onToggle;
    public PPlayerInfo playerInfo { get { return m_playerInfo; } }

    public ISourceItem GetItemData()
    {
        return m_Source;
    }

    public void SetToggleState(bool isOn)
    {
        if (toggle)
            toggle.isOn = isOn;
    }

    protected override void Init()
    {
        friend_txt = ConfigManager.Get<ConfigText>((int)TextForMatType.FriendUIText);
        if (friend_txt == null)
        {
            friend_txt = ConfigText.emptey;
            Logger.LogError("this id can not");
        }
        isFriend = transform.GetComponent<Transform>("isFriend");
        isUnionMember = transform.GetComponent<Transform>("isGuildMember");
        isStranger = transform.GetComponent<Transform>("isStranger");
        friend_hint = transform.GetComponent<Transform>("hint_");
        friend_hint?.SafeSetActive(false);

        name_friend = transform.GetComponent<Text>("name");
        level_friend = transform.GetComponent<Text>("level");
        ID_friend = transform.GetComponent<Text>("ID");
        isonline_friend = transform.GetComponent<Image>("isonline");
        offonline_friend = transform.GetComponent<Image>("offline");
        busy_friend = transform.GetComponent<Image>("busy");
        Head_img = transform.Find("bg/mask")?.gameObject;
        friebdBox = transform.Find("bg")?.gameObject.GetComponentDefault<headBoxFriend>();
        checkObj = transform.Find("selectbox")?.gameObject;
        toggle = transform.GetComponent<Toggle>("toggle");
        goodFeelingValue = transform.GetComponent<Text>("goodFeeling_Text/goodFeelingValue");
        if (toggle)
        {
            //为什么要这么做呢？
            //因为摆预制的时候可能isOn是false toggle的标志图片确实显示着的
            //至少会调用一次OnValueChange。确保isOn的状态和toggle的标志图片显示一致
            toggle.isOn = !toggle.isOn;
            toggle.isOn = false;
        }
        toggle?.onValueChanged.RemoveAllListeners();
        toggle?.onValueChanged.AddListener((t)=> onToggle?.Invoke(this, toggle.isOn));

        Util.SetText(gameObject.GetComponent<Text>("isonline/isonline_text"), friend_txt[29]);
        Util.SetText(gameObject.GetComponent<Text>("offline/offline_text"), friend_txt[30]);
        Util.SetText(gameObject.GetComponent<Text>("busy/offline_text"), friend_txt[39]);
        Util.SetText(isFriend?.GetComponent<Text>("friend_Txt"), ConfigText.GetDefalutString(9502));
        Util.SetText(isUnionMember?.GetComponent<Text>("guildMember_Txt"), ConfigText.GetDefalutString(9502, 1));
        Util.SetText(isStranger?.GetComponent<Text>("text"), ConfigText.GetDefalutString(9502, 2));
        Util.SetText(gameObject?.GetComponent<Text>("goodFeeling_Text"), ConfigText.GetDefalutString(9502, 4));
    }

    public void DelayAddData(SourceFriendInfo info, int index = 1, int type = -1,int sub = -1)
    {
        // type -1其他页面不可出现红点  0 是好友界面 可以出现红点 1公会悬赏记录 不出现状态及公会好友标识
        m_Source = info;
        m_playerInfo = info.PlayerInfo;
        m_index = index;
        isopen = true;
        m_type = type;
        m_sub = sub;
    }
    
    public void SetToggleGroup(ToggleGroup rGroup)
    {
        if(toggle)
            toggle.group = rGroup;
    }
    
    private void AddData()
    {
        AssertInit();
        Module_Avatar.SetClassAvatar(Head_img, m_playerInfo.proto, false, m_playerInfo.gender);
        friebdBox.HeadBox(m_playerInfo.headBox);

        checkObj        .SafeSetActive(false);
        isonline_friend .SafeSetActive(false);
        offonline_friend.SafeSetActive(false);
        busy_friend     .SafeSetActive(false);

        if (m_type == 0)
        {
            PPlayerInfo now = Module_Friend.instance.checkPlayer;
            if (now != null)
            {
                if (now.roleId == m_playerInfo.roleId) checkObj.SafeSetActive(true);//现在选中 高亮
            }
            if (m_index == 0) friend_hint.SafeSetActive(false);
            bool iscunthia = Module_Chat.instance.Past_mes.Exists(a => a == m_playerInfo.roleId);
            friend_hint.SafeSetActive(iscunthia);//如果在past里出现红点

        }
        else if (m_type != -1)
        {
            var have = false;
            if (m_type == 1) have = Module_Match.instance.m_invateCheck.Exists(a => a.roleId == m_playerInfo.roleId);
            if (m_type == 2) have = Module_Active.instance.m_coopCheckList.Exists(a => a == m_playerInfo.roleId);
            if (have) checkObj.SafeSetActive(true);
        }

        var nn = Module_Active.instance.CutString(m_playerInfo.name, m_sub);
        name_friend.text = nn;
        level_friend.text = string.Format(ConfigText.GetDefalutString(218, 37),m_playerInfo.level.ToString());
        ID_friend.text = m_playerInfo.roleId.ToString();

        if (m_type != 1)
        {
            if (m_playerInfo.state > 1) busy_friend.SafeSetActive(true);
            else if (m_playerInfo.state == 0) offonline_friend.SafeSetActive(true);
            else if (m_playerInfo.state == 1) isonline_friend.SafeSetActive(true);
        }
        
        isFriend     ?.SafeSetActive(m_Source.Relation == CommonRelation.Friend);
        isUnionMember?.SafeSetActive(m_Source.Relation == CommonRelation.UnionMember);
        isStranger   ?.SafeSetActive(m_Source.Relation == CommonRelation.Stranger);
        Util.SetText(goodFeelingValue, $"+{m_Source.addPoint}");

        if (m_playerInfo.roleId == Module_Player.instance.id_ || m_type == 1)
        {
            isUnionMember.SafeSetActive(false);
            isFriend.SafeSetActive(false);
        }
    }

    void LateUpdate()
    {
        if (isopen && m_playerInfo != null)
        {
            AddData();
            isopen = false;
        }            
    }
}