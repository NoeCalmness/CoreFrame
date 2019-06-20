using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MangerUnionPlayer : MonoBehaviour
{

    private GameObject m_headBox;
    private GameObject m_headImg;
    private Text m_name;
    private Text m_level;
    private Text m_contribution;
    private Text m_state;
    private Image m_rankImg;
    private Text m_rankTxt;

    private void Get()
    {
        m_headImg = transform.Find("avatar_back/mask").gameObject;
        m_headBox = transform.Find("avatar_back").gameObject;
        m_name = transform.Find("name_Txt").GetComponent<Text>();
        m_level = transform.Find("level_Txt").GetComponent<Text>();
        m_contribution = transform.Find("contribution_Txt").GetComponent<Text>();
        m_state = transform.Find("online_Txt").GetComponent<Text>();
        m_rankImg = transform.Find("rank_Img").GetComponent<Image>();
        m_rankTxt = transform.Find("rank_Img/rank_Txt").GetComponent<Text>();
    }

    // Update is called once per frame
    public void SetPlayerInfo(PUnionPlayer playerInfo)
    {
        Get();
        PPlayerInfo baseInfo = playerInfo.info;
        if (baseInfo == null) return;

        Module_Avatar.SetClassAvatar(m_headImg, baseInfo.proto, false, baseInfo.gender);

        headBoxFriend detailBox = m_headBox.GetComponentDefault<headBoxFriend>();
        detailBox.HeadBox(baseInfo.headBox);
        m_name.text = baseInfo.name;
        m_level.text = "LV." + baseInfo.level;
        m_contribution.text = ConfigText.GetDefalutString(242, 183) + playerInfo.sentiment.ToString();
        
        if (baseInfo.state == 0)
        {
            m_state.text = ConfigText.GetDefalutString(218, 30);
            m_state.color = Color.gray;
        }
        else
        {
            m_state.text = ConfigText.GetDefalutString(218, 29);
            m_state.color = Color.green;
        }

        m_rankImg.gameObject.SetActive(false);
        if (playerInfo.title == 0)
        {
            m_rankImg.gameObject.SetActive(true);
            AtlasHelper.SetShared(m_rankImg.gameObject, "ui_union_level01");
            m_rankTxt.text = ConfigText.GetDefalutString(242, 184);
        }
        else if (playerInfo.title == 1)
        {
            m_rankImg.gameObject.SetActive(true);
            AtlasHelper.SetShared(m_rankImg.gameObject, "ui_union_level02");
            m_rankTxt.text = ConfigText.GetDefalutString(242, 185);
        }


        if (baseInfo.roleId == Module_Player.instance.roleInfo.roleId)
        {
            detailBox.HeadBox(Module_Player.instance.roleInfo.headBox);
            m_name.text = Module_Player.instance.name_;
            m_level.text = "LV." + Module_Player.instance.level;
        }

    }

}
