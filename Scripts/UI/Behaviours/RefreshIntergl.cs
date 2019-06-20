using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RefreshIntergl : MonoBehaviour
{
    private Text m_name;
    private Text m_intergl;
    private Text m_icon;
    private Text m_dataNormal;
    private Image m_dataBefor;
    private Text m_dataNo;

    void Get()
    {
        m_name = transform.Find("name_text").GetComponent<Text>();
        m_intergl = transform.Find("integral_text").GetComponent<Text>();
        m_icon = transform.Find("coin_text").GetComponent<Text>();
        m_dataNormal = transform.Find("rank_txt").GetComponent<Text>();
        m_dataBefor = transform.Find("rank_img").GetComponent<Image>();
        m_dataNo = transform.Find("unrank").GetComponent<Text>();
        Util.SetText(m_dataNo, ConfigText.GetDefalutString(232, 5));
    }

    public void SetUnionInfo(PUnionIntegralInfo info, bool set)
    {
        Get();

        if (info == null) return;
        m_name.text = info.unionname;
        m_intergl.text = info.intergl.ToString();
        m_icon.text = info.bounty.ToString();

        var index = -1;
        for (int i = 0; i < Module_Union.instance.UnionInterList.Count; i++)
        {
            if (info.unionId == Module_Union.instance.UnionInterList[i].unionId)
            {
                index = i;
            }
        }
        ShowRank(index, 1, (ulong)info.unionId, set);
    }

    public void SetPersonInfo(BorderRankData info, bool set)
    {
        Get();

        if (info == null) return;
        m_name.text = info.name;
        m_intergl.text = info.score.ToString();
        m_icon.text = info.money.ToString();
        ShowRank(info.rank - 1, 0, info.roleId, set);
    }

    public void SetUnionHurt(PPlayerHurt info, bool set)
    {
        Get();

        if (info == null) return;

        m_name.text = info.name;
        m_intergl.text = info.hurt.ToString();
        int index = -1;
        for (int i = 0; i < Module_Union.instance.m_playerHurt.Count; i++)
        {
            if (Module_Union.instance.m_playerHurt[i].roleid == info.roleid)
            {
                index = i;
            }
        }
        ShowRank(index, 0, info.roleid, set);
    }

    private void ShowRank(int index, int type, ulong setId, bool set)
    {
        m_dataNormal.gameObject.SetActive(false);
        m_dataBefor.gameObject.SetActive(false);
        m_dataNo.gameObject.SetActive(false);

        if (index == 0)
        {
            m_dataBefor.gameObject.SetActive(true);
            AtlasHelper.SetShared(m_dataBefor.gameObject, "ui_bordland_ranklevel_01");
        }
        else if (index == 1)
        {
            m_dataBefor.gameObject.SetActive(true);
            AtlasHelper.SetShared(m_dataBefor.gameObject, "ui_bordland_ranklevel_02");
        }
        else if (index == 2)
        {
            m_dataBefor.gameObject.SetActive(true);
            AtlasHelper.SetShared(m_dataBefor.gameObject, "ui_bordland_ranklevel_03");
        }
        else if (index == -1)
        {
            m_dataNo.gameObject.SetActive(true);
        }
        else
        {
            m_dataNormal.gameObject.SetActive(true);
            m_dataNormal.text = (index + 1).ToString();
        }

        if (!set) return;
        var m_back = transform.Find("back").GetComponent<Image>();
        m_back.color = Color.white;
        if (type == 0 && setId == Module_Player.instance.id_) m_back.color = GeneralConfigInfo.defaultConfig.rankSelfColor;
        else if (type == 1 && setId == Module_Player.instance.roleInfo.leagueID) m_back.color = GeneralConfigInfo.defaultConfig.rankSelfColor;
    }
}
