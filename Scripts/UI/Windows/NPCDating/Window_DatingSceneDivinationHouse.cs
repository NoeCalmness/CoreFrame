/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-12-26
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class Window_DatingSceneDivinationHouse : Window
{
    private Transform m_tfResultCrystalDevine;//水晶球面板
    private Text m_textCrystalDevineResult;
    private Text m_textCrystalDevineMood;
    private Image m_CrystalDevineMoodNpcIcon;

    private Transform m_tfResultLotDevine;//御神签面板
    private Text m_textLotDevineResult;
    private Text m_textLotDevineMood;
    private Image m_LotDevineMoodNpcIcon;

    private Button m_btnClose;

    protected override void OnOpen()
    {
        m_tfResultCrystalDevine = GetComponent<RectTransform>("resultPanel_0");
        m_textCrystalDevineResult = GetComponent<Text>("resultPanel_0/resultName");
        m_textCrystalDevineMood = GetComponent<Text>("resultPanel_0/resultMood");
        m_CrystalDevineMoodNpcIcon = GetComponent<Image>("resultPanel_0/npcIcon");

        m_tfResultLotDevine = GetComponent<RectTransform>("resultPanel_1");
        m_textLotDevineResult = GetComponent<Text>("resultPanel_1/resultName");
        m_textLotDevineMood = GetComponent<Text>("resultPanel_1/resultMood");
        m_LotDevineMoodNpcIcon = GetComponent<Image>("resultPanel_1/npcIcon");

        m_btnClose = GetComponent<Button>("close"); m_btnClose.onClick.AddListener(() => { Hide<Window_DatingSceneDivinationHouse>(); });

        Init();
    }

    protected override void OnClose()
    {
        //执行下一个行为事件
    }

    private void Init()
    {
        EnumDivinationType type = moduleNPCDating.divinationType;
        m_tfResultCrystalDevine.SafeSetActive(type == EnumDivinationType.CrystalDevine);
        m_tfResultLotDevine.SafeSetActive(type == EnumDivinationType.LotDevine);
        UpdateResult(moduleNPCDating.divinationResult, type);
    }

    private void UpdateResult(PDatingDivinationResultData data, EnumDivinationType divType)
    {
        if (data == null) return;
        int curDatingNpcId = moduleNPCDating.curDatingNpc != null ? moduleNPCDating.curDatingNpc.npcId : 0;
        Module_Npc.NpcMessage npc = moduleNpc.GetTargetNpc((NpcTypeID)curDatingNpcId);

        if (divType == EnumDivinationType.CrystalDevine)
        {
            UpdateTextResult(m_textCrystalDevineResult, data);
            UpdateTextMood(m_textCrystalDevineMood, 275, data);
            UpdateNpcIcon(m_CrystalDevineMoodNpcIcon, data, npc);
        }
        else if (divType == EnumDivinationType.LotDevine)
        {
            UpdateTextResult(m_textLotDevineResult, data);
            UpdateTextMood(m_textLotDevineMood, 276, data,npc);
            UpdateNpcIcon(m_LotDevineMoodNpcIcon, data, npc);
        }
    }

    private void UpdateTextResult(Text t, PDatingDivinationResultData data)
    {
        Util.SetText(t, data.contentId);
    }

    private void UpdateTextMood(Text t, int id,PDatingDivinationResultData data, Module_Npc.NpcMessage npc = null)
    {
        if (npc == null)
        {
            Util.SetText(t, id, data.addMood.ToString(), data.addMood.ToString());
        }
        else
        {
            Util.SetText(t, id, npc.name, data.addMood.ToString(), data.addMood.ToString());
        }
        
    }

    private void UpdateNpcIcon(Image img, PDatingDivinationResultData data, Module_Npc.NpcMessage npc)
    {
        if (npc == null) return;
        AtlasHelper.SetNpcDateInfo(img, npc.icon);
    }
}
