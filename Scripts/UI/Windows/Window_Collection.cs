using System.Collections;
using UnityEngine;
using UnityEngine.UI;
// /****************************************************************************************************
//  * Copyright (C) 2018-2019 FengYunChuanShuo
//  * 
//  *               游戏图鉴UI窗口
//  * 
//  * Author:     T.Moon 
//  * Version:    0.1
//  * Created:    2018-12-17      15:00
//  ***************************************************************************************************/

public class Window_Collection : Window
{

    private GameObject m_npcPanel = null;

    private GameObject m_runePanel = null;

    private Button m_npcBtn = null;

    private Button m_runeBtn = null;

    //子窗口
    private Collection_Window_Npc m_collection_NpcPanel;

    private Collection_Window_Rune m_collection_RunePanel;


    protected override void OnInitialized()
    {
        base.OnInitialized();

       
        //请求数据
 
    }

    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        moduleCollection.InitRuneData();
        //发符文
        moduleCollection.SendHistoryRune();
        moduleGlobal.ShowGlobalLayerDefault(1,false);
    }

    //当对象被添加到场景的时候，进行回调
    protected override void OnOpen()
    {
        base.OnOpen();
       
        m_npcPanel = transform.Find("npc_Panel").gameObject;
        m_runePanel = transform.Find("rune_Panel").gameObject;

        m_collection_NpcPanel = SubWindowBase.CreateSubWindow<Collection_Window_Npc>(this, m_npcPanel?.gameObject);
        m_collection_RunePanel = SubWindowBase.CreateSubWindow<Collection_Window_Rune>(this,m_runePanel?.gameObject);

        m_npcBtn = GetComponent<Button>("0");
        m_npcBtn.onClick.AddListener(OnClickNpcBtn);
        m_runeBtn = GetComponent<Button>("1");
        m_runeBtn.onClick.AddListener(OnClickRuneBtn);

    }

    protected override void OnReturn()
    {

        if (m_collection_NpcPanel.OnReturn())
            return;
        if (m_collection_RunePanel.OnReturn())
            return;
        base.OnReturn();
    }


    #region 界面内部

    private void OnClickNpcBtn()
    {
        m_collection_NpcPanel.Initialize();
        m_collection_RunePanel.UnInitialize();
        Module_Task.instance.SendAccputTask(13);
        //Module_Task.instance.DispatchModuleEvent(Module_Task.TaskGiftMessage,null);
    }

    private void OnClickRuneBtn()
    {
        m_collection_RunePanel.Initialize();
        m_collection_NpcPanel.UnInitialize();
        Module_Task.instance.SendTaskFinish(6);

    }


    public override void OnDisable()
    {
        base.OnDisable();
    }



    void _ME(ModuleEvent<Module_Collection> e)
    {
        if (!actived) return;
        switch (e.moduleEvent)
        {
            case Module_Collection.PrepareDataForBigRune:
                {
                   
                }
                break;
            default:
                break;
        }
    }



    #endregion
 }
