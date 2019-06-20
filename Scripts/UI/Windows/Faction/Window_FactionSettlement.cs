// /**************************************************************************************************
//  * Copyright (C) 2017-2018 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-04-30      9:39
//  *LastModify：2019-04-30      9:39
//  ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class Window_FactionSettlement : Window
{
    public class DoubleText
    {
        public Text a;
        public Text b;

        public void SetText(string rStr)
        {
            Util.SetText(a, rStr);
            Util.SetText(b, rStr);
        }
    }

    private DoubleText m_rankText = new DoubleText();
    private DoubleText m_comboKill = new DoubleText();
    private DoubleText m_currentScore = new DoubleText();
    private DoubleText m_changeScore = new DoubleText();
    private DoubleText m_factionName = new DoubleText();
    private Button m_again;
    private Button m_confirm;
    private Button m_confirm2;
    private Transform m_defeatRoot;
    private Transform m_victoryRoot;

    protected override void OnOpen()
    {
        base.OnOpen();
        InitComponents();
        MultiLangurage();

        m_again?.onClick.AddListener(() =>
        {
            moduleFactionBattle.AutoMatch = true;
            Game.GoHome();
        });

        m_confirm?.onClick.AddListener(() =>
        {
            Game.GoHome();
        });

        m_confirm2?.onClick.AddListener(() =>
        {
            Game.GoHome();
        });
    }

    private void InitComponents()
    {
        m_factionName.a = GetComponent<Text>("win_panel/content/center/preward/number_txt/faction_txt");
        m_factionName.b = GetComponent<Text>("lose_panel/content/center/preward/number_txt/faction_txt");
        m_comboKill.a   = GetComponent<Text>("win_panel/content/center/achieve_txt");
        m_comboKill.b   = GetComponent<Text>("lose_panel/content/center/achieve_txt");
        m_rankText.a      = GetComponent<Text>("win_panel/content/center/preward/number_txt");
        m_rankText.b      = GetComponent<Text>("lose_panel/content/center/preward/number_txt");
        m_currentScore.a  = GetComponent<Text>("win_panel/content/preaward/arrow2/Text");
        m_currentScore.b  = GetComponent<Text>("lose_panel/content/preaward/arrow2/Text");
        m_changeScore.a   = GetComponent<Text>("win_panel/content/preaward/arrow2/Text/add_txt");
        m_changeScore.b   = GetComponent<Text>("lose_panel/content/preaward/arrow2/Text/add_txt");

        m_again         = GetComponent<Button>("win_panel/content/preaward/but01");
        m_confirm       = GetComponent<Button>("win_panel/content/preaward/but02");
        m_confirm2      = GetComponent<Button>("lose_panel/content/preaward/but02");
        m_defeatRoot    = GetComponent<Transform>("lose_panel");
        m_victoryRoot   = GetComponent<Transform>("win_panel");
    }

    private void MultiLangurage()
    {
        Util.SetText(GetComponent<Text>("lose_panel/content/center/arrow1/Text"), ConfigText.GetDefalutString(TextForMatType.FactionSettlementUI));
        Util.SetText(GetComponent<Text>("win_panel/content/center/arrow1/Text"), ConfigText.GetDefalutString(TextForMatType.FactionSettlementUI));
        Util.SetText(GetComponent<Text>("lose_panel/content/preaward/but02/Text"), ConfigText.GetDefalutString(TextForMatType.FactionSettlementUI, 1));
        Util.SetText(GetComponent<Text>("win_panel/content/preaward/but02/Text"), ConfigText.GetDefalutString(TextForMatType.FactionSettlementUI, 1));
        Util.SetText(GetComponent<Text>("win_panel/content/preaward/but01/Text"), ConfigText.GetDefalutString(TextForMatType.FactionSettlementUI, 2));
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        base.OnBecameVisible(oldState, forward);

        m_defeatRoot.SafeSetActive(!modulePVP.isWinner);
        m_victoryRoot.SafeSetActive(modulePVP.isWinner);

        m_rankText      .SetText(Util.Format(ConfigText.GetDefalutString(TextForMatType.FactionSettlementUI, 4), modulePVP.settlementData.self.rank));
        m_comboKill     .SetText(Module_FactionBattle.GetKillString(modulePVP.settlementData.self.comboKill));
        m_currentScore  .SetText(modulePVP.settlementData.self.score.ToString());
        m_changeScore   .SetText(Util.Format(ConfigText.GetDefalutString(TextForMatType.FactionSettlementUI, 3), modulePVP.settlementData.addScore));
        m_factionName   .SetText(moduleFactionBattle.SelfFactionName);
    }
}
