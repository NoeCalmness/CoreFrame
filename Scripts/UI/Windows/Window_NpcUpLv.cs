/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-01-02
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

public class Window_NpcUpLv : Window
{
    private Button     m_bgBtn;
    private Transform  m_npcIcon;
    private Image      m_npcName;
    private Text       m_cvName;
    private Text       m_curStage;
    private Text       m_lvName1;
    private Text       m_lvName2;
    private Text       m_lvName3;
    private Image      m_curImage1;
    private Image      m_curImage2;
    private Image      m_curImage3;
    private Text       m_nextStage;
    private Text       m_unlockSystem;

    private static Module_Npc.NpcMessage m_msg;
    private static Action<Module_Npc.NpcMessage> m_callback;

    protected override void OnOpen()
    {
        isFullScreen = false;
        m_bgBtn = GetComponent<Button>("bg");

        m_npcIcon      = GetComponent<Transform>("bg/left/npcicon");
        m_npcName      = GetComponent<Image>("bg/left/name");
        m_cvName       = GetComponent<Text>("bg/left/cvinfo");

        m_curStage     = GetComponent<Text>("bg/right/curStage");
        m_lvName1      = GetComponent<Text>("bg/right/curStage/name_1");
        m_lvName2      = GetComponent<Text>("bg/right/curStage/name_2");
        m_lvName3      = GetComponent<Text>("bg/right/curStage/name_3");
        m_curImage1    = GetComponent<Image>("bg/right/curStage/name_1/levelNode_Highlight");
        m_curImage2    = GetComponent<Image>("bg/right/curStage/name_2/levelNode_Highlight");
        m_curImage3    = GetComponent<Image>("bg/right/curStage/name_3/levelNode_Highlight");

        m_nextStage    = GetComponent<Text>("bg/right/nextStage");
        m_unlockSystem = GetComponent<Text>("bg/right/unlockText");

        m_bgBtn?.onClick.RemoveAllListeners();
        m_bgBtn?.onClick.AddListener(OnClickBg);

        InitText();
    }

    public static void ShowUpWindow(Module_Npc.NpcMessage msg,Action<Module_Npc.NpcMessage> callback)
    {
        if (msg == null) return;

        m_msg = msg;
        m_callback = callback;

        ShowAsync<Window_NpcUpLv>();
    }

    private void OnClickBg()
    {
        Hide(true);
        m_callback?.Invoke(m_msg);
    }

    private void InitText()
    {
        Util.SetText(GetComponent<Text>("bg/left/tittle"), (int)TextForMatType.NpcUIText, 2);
        Util.SetText(GetComponent<Text>("bg/right/arrow/text"), (int)TextForMatType.NpcUIText, 3);
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        RefreshWindow(m_msg);
    }

    private void RefreshWindow(Module_Npc.NpcMessage msg)
    {
        if (msg == null) return;

        if (msg.npcInfo != null)
        {
            AtlasHelper.SetNpcDateInfo(m_npcName, msg.npcInfo.artNameTwo);
            UIDynamicImage.LoadImage(m_npcIcon, msg.npcInfo.bigBodyIcon);
            Util.SetText(m_cvName, msg.npcInfo.voiceActor);
        }
        Util.SetText(m_curStage, msg.curStageName);

        //当前阶段刷新
        if (msg.belongStageName != null && msg.belongStageName.Length >= 3)
        {
            Util.SetText(m_lvName1, msg.belongStageName[0]);
            Util.SetText(m_lvName2, msg.belongStageName[1]);
            Util.SetText(m_lvName3, msg.belongStageName[2]);
        }

        int remainder = msg.fetterLv % 3;

        m_curImage1.SafeSetActive(remainder == 1);
        m_curImage2.SafeSetActive(remainder == 2);
        m_curImage3.SafeSetActive(remainder == 0);

        //下一阶段刷新
        m_nextStage.SafeSetActive(msg.fetterStage < msg.maxFetterStage);

        if (msg.fetterStage < msg.maxFetterStage)
            Util.SetText(m_nextStage, msg.GetStageName(msg.fetterStage + 1));

        //解锁系统刷新
        m_unlockSystem.SafeSetActive(msg.isUnlockEngagement);
        if (msg.isUnlockEngagement) Util.SetText(m_unlockSystem, 177);
    }
}
