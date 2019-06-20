// /**************************************************************************************************
//  * Copyright (C) 2017-2018 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-05-09      13:33
//  *LastModify：2019-05-09      13:33
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class FactionRankBehaviour : AssertOnceBehaviour
{
    private Text m_rank;
    private Text m_name;
    private Text m_maxComboKill;
    private Text m_score;
    private Text m_comboKill;
    private Transform[] m_specialRank; 
    private PFactionPlayerInfo data;
    private Button m_clickButton;
    private Image m_applique;
    private Image m_bg;


    private void Start()
    {
        AssertInit();
    }

    protected override void Init()
    {
        base.Init();
        m_specialRank = new Transform[3];
        m_specialRank[0] = transform.GetComponent<Transform>("first");
        m_specialRank[1] = transform.GetComponent<Transform>("second");
        m_specialRank[2] = transform.GetComponent<Transform>("third");
        m_rank          = transform.GetComponent<Text>("munber_txt");
        m_name          = transform.GetComponent<Text>("name");
        m_maxComboKill  = transform.GetComponent<Text>("name/degree/Text");
        m_score         = transform.GetComponent<Text>("score");
        m_comboKill     = transform.GetComponent<Text>("degree_txt");
        m_applique      = transform.GetComponent<Image>("name/degree/kill_icon");
        m_bg            = transform.GetComponent<Image>("bg");

        m_clickButton   = transform.GetComponentDefault<Button>();
        m_clickButton?.onClick.AddListener(() =>
        {
            if (data.info.roleId == Module_Player.instance?.id_)
                return;
            Window.SetWindowParam<Window_ApplyFriend>(data.info);
            Window.ShowAsync<Window_ApplyFriend>();
        });
    }

    public void BindData(PFactionPlayerInfo rData)
    {
        AssertInit();
        data = rData;
        if (rData.battleInfo.rank <= m_specialRank.Length)
        {
            for (var i = 0; i < m_specialRank.Length; i++)
                m_specialRank[i].SafeSetActive(rData.battleInfo.rank == i + 1);
            m_rank.SafeSetActive(false);
        }
        else
        {
            for (var i = 0; i < m_specialRank.Length; i++)
                m_specialRank[i].SafeSetActive(false);
            m_rank.SafeSetActive(true);
            Util.SetText(m_rank, Module_FactionBattle.GetRankLabel(rData.battleInfo.rank));
        }
        Util.SetText(m_name, data.info.name);
        m_bg.color = ColorGroup.GetColor(Module_FactionBattle.instance.SelfFaction == Module_FactionBattle.Faction.Red ? ColorManagerType.FactionBgLeft : ColorManagerType.FactionBgRight, rData.info.roleId == Module_Player.instance.id_);
        Util.SetText(m_maxComboKill, Module_FactionBattle.GetKillString(data.battleInfo.maxCombokill));
        Util.SetText(m_comboKill,    Module_FactionBattle.GetKillString(data.battleInfo.comboKill));
        Util.SetText(m_score,        data.battleInfo.score.ToString());

        m_maxComboKill?.transform.parent.SafeSetActive(!string.IsNullOrEmpty(m_maxComboKill?.text));
        var info = ConfigManager.Get<FactionKillRewardInfo>(data.battleInfo.maxCombokill);
        if (!string.IsNullOrEmpty(info?.applique))
            AtlasHelper.SetIcons(m_applique, info.applique);
    }
}
