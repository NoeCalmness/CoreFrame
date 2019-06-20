using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ApplyFriendInfo : MonoBehaviour {

    private GameObject apply_bg;
    private GameObject apply_bg_mask;
    private Text name_apply;
    private Text level_apply;
    private Text ID_apply;
    private Button add_Ok;
    private Button add_No;
    
    private Action<ulong> agree;
    private ulong ID;

    private Action<long> m_agreeUnion;
    private long m_idUnion;

    // 好友申请列表的信息
    private void Get()
    {
         apply_bg = transform.Find("bg").gameObject;
         apply_bg_mask = transform.Find("bg/mask").gameObject;
         name_apply = transform.Find("name").gameObject.GetComponent<Text>();
         level_apply = transform.Find("level").gameObject.GetComponent<Text>();
         ID_apply = transform.Find("ID").gameObject.GetComponent<Text>();
         add_Ok = transform.Find("add").gameObject.GetComponent<Button>();
         add_No = transform.Find("refuse").gameObject.GetComponent<Button>();
    }
    public  void InitItem(ulong IDID,Action<ulong> agr)
    {
        ID = IDID;
        agree = agr;
    }
    public void UnionInitItem(long unionId, Action<long> unionAgr)
    {
        m_idUnion = unionId;
        m_agreeUnion = unionAgr;
    }
    public void SetInfo(PPlayerInfo Info, int type = 0)
    {
        Get();

        Module_Avatar.SetClassAvatar(apply_bg_mask, Info.proto, false, Info.gender);
        headBoxFriend applybox = apply_bg.GetComponentDefault<headBoxFriend>();
        applybox.HeadBox(Info.headBox);

        name_apply.text = Info.name;
        level_apply.text = string.Format(ConfigText.GetDefalutString(218, 37), Info.level.ToString());
        ID_apply.text = Info.roleId.ToString();


        add_Ok.onClick.RemoveAllListeners();
        add_No.onClick.RemoveAllListeners();

        if (type == 0)
        {
            transform.GetComponent<Button>().onClick.AddListener(delegate
            {
                Module_Friend.instance.SendLookDetails(Info.roleId);
            });
            add_Ok.onClick.AddListener(AgreeClick);
            add_No.onClick.AddListener(delegate { Module_Friend.instance.SendReplyRefusedMes(ID); });
        }
        else
        {
            add_Ok.onClick.AddListener(UnionAgreeClick);
            add_No.onClick.AddListener(delegate
            {
                long[] ids = new long[1];
                ids[0] = m_idUnion;
                Module_Union.instance.SloveApply(2, ids);
            });
        }
    }
    private void AgreeClick()
    {
        agree?.Invoke(ID);
    }
    private void UnionAgreeClick()
    {
        m_agreeUnion?.Invoke(m_idUnion);
    }
}
