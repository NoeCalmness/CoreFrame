using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuildRefreshInfo : MonoBehaviour
{

    private Text m_name;
    private Text m_id;
    private Text m_needLevel;
    private Text m_playerNum;
    private Text m_requiredTxt;
    private Text m_requiredLevel;
    private Text m_fullApproveText;
    private Text m_addBtnTxt;
    private Text m_applyBtnTxt;
    private Text m_levelInsufficient;
    private Button m_addBtn;
    private Action<PRefreshInfo,int> m_join;
    PRefreshInfo m_info;
    public int m_type;

    void Get()
    {
        m_name = transform.Find("guildName_Txt").GetComponent<Text>();
        m_playerNum = transform.Find("guildMember_Txt").GetComponent<Text>();
        m_requiredTxt = transform.Find("guildRate_Txt").GetComponent<Text>();
        m_fullApproveText = transform.Find("autoApprove_Txt").GetComponent<Text>();
        m_id = transform.Find("guildID_Txt").GetComponent<Text>();
        m_requiredLevel = transform.Find("guildRate_Txt/rate_Txt").GetComponent<Text>();
        m_addBtn = transform.Find("join_Btn").GetComponent<Button>();
        m_addBtnTxt = transform.Find("join_Btn/join_Txt").GetComponent<Text>();
        m_applyBtnTxt = transform.Find("join_Btn/alreadyApply_Txt").GetComponent<Text>();
        m_levelInsufficient = transform.Find("join_Btn/cannotadd_Txt").GetComponent<Text>();
        m_needLevel= transform.Find("guildLevelRequired_Txt").GetComponent<Text>();


        Util.SetText(m_addBtnTxt, ConfigText.GetDefalutString(242, 150));
        Util.SetText(m_applyBtnTxt, ConfigText.GetDefalutString(242, 68));
        Util.SetText(m_levelInsufficient, ConfigText.GetDefalutString(242, 151));

    }
    public void SetInfo(PRefreshInfo baseInfo,int type,Action<PRefreshInfo,int> joinInfo)
    {
        if (baseInfo.refreshinfo == null) return;
        m_info = baseInfo;
        m_join = joinInfo;
        m_type = type;
     
        Get();
        m_name.text = baseInfo.refreshinfo.unionname;
        m_id.text = "ID:" + baseInfo.refreshinfo.id;

        Util.SetText(m_requiredTxt, ConfigText.GetDefalutString(242, 152));
        Util.SetText (m_requiredLevel, Module_Union.instance.SetUnionLevelTxt(baseInfo.refreshinfo.level));   

        string str = baseInfo.refreshinfo.playernum + "/" + baseInfo.refreshinfo.playertop;
        Util.SetText(m_playerNum, ConfigText.GetDefalutString(242, 153), str);

        string str1 = "LV." + baseInfo.refreshinfo.minlevel;
        Util.SetText(m_needLevel, ConfigText.GetDefalutString(242, 154), str1);

        if (baseInfo.refreshinfo.automatic == 0)
        {
            string str2 = "LV." + baseInfo.refreshinfo.automaticlevel;
            Util.SetText(m_fullApproveText, ConfigText.GetDefalutString(242, 155), str2);
        }
        else
        {
            Util.SetText(m_fullApproveText, ConfigText.GetDefalutString(242, 156));
        }

        AddClickState(baseInfo.state, baseInfo.refreshinfo.id);
        
        if (Module_Player.instance.roleInfo.level < baseInfo.refreshinfo.minlevel)
        {
            m_addBtn.interactable = false;
            m_applyBtnTxt.gameObject.SetActive(false);
            m_addBtnTxt.gameObject.SetActive(false);
            m_levelInsufficient.gameObject.SetActive(true);
        }

        if (baseInfo.refreshinfo.playernum>= baseInfo.refreshinfo.playertop)
        {
            m_addBtn.interactable = false;
            m_addBtnTxt.gameObject.SetActive(true);
        }
        
        m_addBtn.onClick.RemoveAllListeners();
        m_addBtn.onClick.AddListener(AddClick);
    }

    private void AddClickState(int state, long unionId)
    {
        m_addBtn.interactable = false;
        m_addBtnTxt.gameObject.SetActive(true);
        m_applyBtnTxt.gameObject.SetActive(false);
        m_fullApproveText.gameObject.SetActive(true);
        m_levelInsufficient.gameObject.SetActive(false);
        
        if (state == 2) Util.SetText(m_fullApproveText, ConfigText.GetDefalutString(242, 157));
        else
        {
            var have = Module_Union.instance.ApplyUnionList.Exists(a => a == unionId);
            if (have || state == 1)
            {
                m_applyBtnTxt.gameObject.SetActive(true);
                m_addBtnTxt.gameObject.SetActive(false);
            }
            else m_addBtn.interactable = true;
        }
    }

    private void AddClick()
    {
        m_join?.Invoke(m_info, m_type);
    }
}
