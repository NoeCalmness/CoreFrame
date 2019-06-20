// /**************************************************************************************************
//  * Copyright (C) 2017-2018 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-05-21      18:07
//  *LastModify：2019-05-21      18:07
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public struct SelfSettlement
{
    public Text factionName;
    public Text rank;
    public Text score;
    public Text comboKill;
    public Text maxComboKill;
    public Image applique;

    public void BindComponent(Transform rRoot)
    {
        factionName     = rRoot.GetComponent<Text>("myfaction");
        rank            = rRoot.GetComponent<Text>("rank");
        score           = rRoot.GetComponent<Text>("score");
        comboKill       = rRoot.GetComponent<Text>("degree_txt/txt");
        maxComboKill    = rRoot.GetComponent<Text>("degree_txt/degree/Text");
        applique        = rRoot.GetComponent<Image>("degree_txt/degree/kill");
    }

    public void SetMaxComboKillBg(int rMaxComboKill)
    {
        var info = ConfigManager.Get<FactionKillRewardInfo>(rMaxComboKill);
        if (!string.IsNullOrEmpty(info?.applique))
            AtlasHelper.SetIcons(applique, info.applique);
    }
}

public class FactionWindow_Result: SubWindowBase
{
    private Transform m_defeat;
    private Transform m_victory;
    private SelfSettlement m_settlement = new SelfSettlement();

    protected override void InitComponent()
    {
        base.InitComponent();
        m_defeat    = WindowCache.GetComponent<Transform>("factionbattle/result/defeat");
        m_victory   = WindowCache.GetComponent<Transform>("factionbattle/result/victory");
        m_settlement.BindComponent(WindowCache.GetComponent<Transform>("factionbattle/result/settlement"));
    }

    protected override void RefreshData(params object[] p)
    {
        base.RefreshData(p);
        var lonly = p.Length > 0 && (bool)p[0];

        var children = Root.transform.GetChildList();
        foreach (var t in children)
            t.gameObject.SetActive(!lonly);

        m_defeat.SafeSetActive(!moduleFactionBattle.IsWin && moduleFactionBattle.SelfFaction != Module_FactionBattle.Faction.None);
        m_victory.SafeSetActive(moduleFactionBattle.IsWin && moduleFactionBattle.SelfFaction != Module_FactionBattle.Faction.None);

        if (lonly)
            return;

        Util.SetText(m_settlement.factionName, moduleFactionBattle.SelfFactionName);
        Util.SetText(m_settlement.rank, Util.Format(ConfigText.GetDefalutString(TextForMatType.FactionBattleUI, 5), moduleFactionBattle.SelfRank));
        Util.SetText(m_settlement.score, moduleFactionBattle.SelfScore.ToString());
        Util.SetText(m_settlement.comboKill, Module_FactionBattle.GetKillString(moduleFactionBattle.ComboKill));
        Util.SetText(m_settlement.maxComboKill, Module_FactionBattle.GetKillString(moduleFactionBattle.MaxComboKill));
        m_settlement.maxComboKill?.transform.parent.SafeSetActive(!string.IsNullOrEmpty(m_settlement.maxComboKill.text));
        m_settlement.SetMaxComboKillBg(moduleFactionBattle.MaxComboKill);
    }
}
