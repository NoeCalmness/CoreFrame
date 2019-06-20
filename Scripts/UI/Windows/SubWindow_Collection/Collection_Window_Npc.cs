// /**************************************************************************************************
//  * Copyright (C) 2018-2019 FengYunChuanShuo
//  * 
//  *           图鉴子窗口
//  * 
//  *Author:     T.Moon
//  *Version:    0.1
//  *Created:    2018-12-18      13:31
//  ***************************************************************************************************/


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class NpcDesc
{
    public int npcid
    {
        set;
        get;

    }

    public NpcDesc()
    {

    }
   
}

public class Collection_Window_Npc : SubWindowBase<Window_Collection>
{

    //NPCINFO
    private Image m_NpcIcon;                //图标
    private Text m_NpcName;                 //名称
    private Text m_NpcNation;               //名族       
    private Text m_NpcFaith;                //信仰
    private Text m_NpcFetterValue;          //羁绊值
    private Text m_NpcFetterLevel;          //羁绊等级
    private Text m_NpcFetterState;          //当前羁绊状态

    private Text m_NpcFetterStage_0;        //羁绊阶段1
    private Image m_NpcFetterStage_0_Flag;  //羁绊阶段1标记
    private Text m_NpcFetterStage_1;        //羁绊阶段2
    private Text m_NpcFetterStage_2;        //羁绊阶段3

    private ScrollView m_NpcScroll;         //NPC列表

    private ScrollView m_NpcDescScroll;

    private DataSource<Module_Npc.NpcMessage> m_Npclist;
    private DataSource<string> m_NpcDescList;
    private Module_Npc.NpcMessage m_curNpc;
    private int m_CurClickIndex = -1;                   //当前点击索引
    private PageNavigator m_pageNavigator;

    protected override void InitComponent()
    {
        base.InitComponent();

        m_NpcIcon = WindowCache.GetComponent<Image>("npc_Panel/bg/Image");
        m_NpcName = WindowCache.GetComponent<Text>("npc_Panel/npcInfo/name");
        m_NpcNation = WindowCache.GetComponent<Text>("npc_Panel/npcInfo/nation");
        m_NpcFaith = WindowCache.GetComponent<Text>("npc_Panel/npcInfo/faith");
        m_NpcFetterValue = WindowCache.GetComponent<Text>("npc_Panel/npcInfo/npcGoodFeelingValue");
        m_NpcFetterLevel = WindowCache.GetComponent<Text>("npc_Panel/npcInfo/npcGoodFeelingLevel");
        m_NpcFetterState = WindowCache.GetComponent<Text>("npc_Panel/npcInfo/npcGoodFeelingState");
        m_NpcFetterStage_0 = WindowCache.GetComponent<Text>("npc_Panel/npcInfo/npcGoodFeelingState_0");
        m_NpcFetterStage_0_Flag = WindowCache.GetComponent<Image>("npc_Panel/npcInfo/npcGoodFeelingState_0/onState");
        m_NpcFetterStage_1 = WindowCache.GetComponent<Text>("npc_Panel/npcInfo/npcGoodFeelingState_1");
        m_NpcFetterStage_2 = WindowCache.GetComponent<Text>("npc_Panel/npcInfo/npcGoodFeelingState_2");
        m_NpcScroll = WindowCache.GetComponent<ScrollView>("npc_Panel/scrollView");
        m_NpcDescScroll = WindowCache.GetComponent<ScrollView>("npc_Panel/bg/descreptionList");
        m_pageNavigator = WindowCache.GetComponent<PageNavigator>("npc_Panel/bg/descreptionList/pageNavigator");
        m_NpcDescList = new DataSource<string>(null, m_NpcDescScroll, OnSetNpcDescData, null);
        m_Npclist = new DataSource<Module_Npc.NpcMessage>(null, m_NpcScroll, OnSetData, OnClick);
    }


    public override bool Initialize(params object[] p)
    {
        m_Npclist.SetItems(Module_Npc.instance.allNpcs);
        //默认取第一个NPC的信息
        if(m_CurClickIndex == -1)
        {
            m_curNpc = Module_Npc.instance.allNpcs[0];
        }
        SetNpcBiography();
        return base.Initialize(p);
    }

    void SetNpcBiography()
    {
        if (m_curNpc != null)
        {
            int id = m_curNpc.npcInfo.biography;
            var c = ConfigManager.Get<ConfigText>(id);
            m_NpcDescList.SetItems(c.text);
        }
        m_pageNavigator.SetOne("0");
    }
    public override bool OnReturn()
    {
        return base.OnReturn();
    }

    public override void OnUpdate(int diff)
    {
        base.OnUpdate(diff);
    }


    private void OnSetData(RectTransform rt, Module_Npc.NpcMessage info)
    {
        Transform no = rt.Find("noImage");
        Transform have = rt.Find("haveImage");
        Transform select = rt.Find("selectBox");
        if (!no || !have || !select) return;

        no.gameObject.SetActive(false);
        have.gameObject.SetActive(true);

        int index = Module_Npc.instance.allNpcs.FindIndex((p) => p.npcId == info.npcId);

        //设置默认选中已有的 第一个符文
        if (m_CurClickIndex == -1)
        {
            m_CurClickIndex = index;
        }

        select.gameObject.SetActive(index == m_CurClickIndex);

        if (index == m_CurClickIndex)
        {
            AtlasHelper.SetShared(m_NpcIcon, info.icon);
            Util.SetText(m_NpcName, info.npcInfo.name);
            Util.SetText(m_NpcNation, info.npcInfo.nation);
            Util.SetText(m_NpcFaith, info.npcInfo.belief);
            //UIDynamicImage.LoadImage(m_NpcFullImage.transform, info.npcInfo.allbobyicon, null, true); //这里需要全身像数据
            Util.SetText(m_NpcFetterValue, info.nowFetterValue);
            Util.SetText(m_NpcFetterLevel, info.fetterLv);
            Util.SetText(m_NpcFetterState, info.curLvName);
            int c = info.fetterLv % 3;
            m_NpcFetterStage_0_Flag.SafeSetActive(c == 1);
            m_NpcFetterStage_0_Flag.SafeSetActive(c == 2);
            m_NpcFetterStage_0_Flag.SafeSetActive(c == 0);

            if (info.belongStageName.Length>=3)
            {
                Util.SetText(m_NpcFetterStage_0, info.belongStageName[0]);
                Util.SetText(m_NpcFetterStage_1, info.belongStageName[1]);
                Util.SetText(m_NpcFetterStage_2, info.belongStageName[2]);
            }
        }

    }

    private void OnClick(RectTransform rt, Module_Npc.NpcMessage info)
    {
        m_CurClickIndex = Module_Npc.instance.allNpcs.FindIndex((p) => p.npcId == info.npcId);
        m_Npclist.UpdateItems();
        m_curNpc = info;
        SetNpcBiography();
    }
    private void OnSetNpcDescData(RectTransform rt,string info)
    {
        Text desc = rt.GetComponent<Text>();
        if (desc != null)
            desc.text = info;
    }

}