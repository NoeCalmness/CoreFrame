// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  *       图鉴数据  网络消息
//  * 
//  * Author:     T.Moon
//  * Version:    0.1
//  * Created:    2018-12-17      17:45
//  ***************************************************************************************************/


using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//------图鉴数据结构-----
public class CollectionInfo
{

}

//---灵珀---
public class RuneData
{
    //----ID----
    public int itemtypeId;
    //----是否拥有该物品----
    public bool isOwnItem { get; set; }
    //----物品名称ID----
    public string itemName;
    //----描述ID---
    public int descId;
   
    //-----部位---
    public int subtype;

    public string iconId { set; get; }
    //-----大图----
    public string fullImage { set; get; }

    public ushort suite { set; get; }

    public RuneData()
    {
        itemtypeId = 0;
        isOwnItem = false;
        itemName = string.Empty;
        descId = 0;
        subtype = 0;
    }

    public void SetHaveItem(int id)
    {
        if (itemtypeId == id)
            isOwnItem = true;
    }

}


public class Module_Collection : Module<Module_Collection>
{

    public const string PrepareDataForBigRune = "PrepareDataForBigRune";//大图

    //----灵珀配置数据----
    Dictionary<ushort, List<RuneData>> m_RuneDataInfoList = new Dictionary<ushort, List<RuneData>>();

    List<RuneData> m_RuneDataUI = new List<RuneData>();

    public List<RuneData> GetUIRuneData()
    {
        return m_RuneDataUI;
    }

    //-----接受网络消息----
    void _Packet()
    {

    }

    //-----发送网络消息----
    public void Send()
    {
        CsEquipRuneEnhance p = PacketObject.Create<CsEquipRuneEnhance>();
        session.Send(p);
    }

    //----求符文---
    public void SendHistoryRune()
    {
        PacketObject p = PacketObject.Create<CsHistoryRunes>();
        session.Send(p);
    }

    //---收到符文数据---
    void _Packet(ScHistoryRunes p)
    {
        if (p.runes != null && p.runes.Length >= 1)
        {
          
            for (int i = 0; i < p.runes.Length; i++)
            {
                UpdateRuneData(p.runes[i]);
            }
         
        }
        else
            //Logger.LogInfo("donot have any runes at before ! haveList's length is zero ");
        DispatchModuleEvent(PrepareDataForBigRune);
    }


    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
    }

    public override void DispatchModuleEvent(string e, object param1 = null, object param2 = null, object param3 = null, object param4 = null)
    {
        base.DispatchModuleEvent(e, param1, param2, param3, param4);
    }

    public override void DispatchModuleEvent(string e, PacketObject msg)
    {
        base.DispatchModuleEvent(e, msg);
    }

    //-----准备数据----
    public void InitRuneData()
    {
        if (m_RuneDataInfoList.Count > 0)
            return;
        List<PropItemInfo> allList = ConfigManager.GetAll<PropItemInfo>();
        for (int i = 0; i < allList.Count; i++)
        {
            if (allList[i].itemType == PropType.Rune)
            {
                PropItemInfo info = allList[i];

                List<RuneData> data = new List<RuneData>();
                RuneData rd = new RuneData();
                rd.iconId = info.icon;
                rd.itemtypeId = info.ID;
                rd.itemName = info.itemName;
                rd.subtype = info.subType;
                rd.descId = info.desc;
                rd.fullImage = info.mesh[1];
                rd.suite = info.suite;

                if (m_RuneDataInfoList.TryGetValue(info.suite,out data))
                {
                    m_RuneDataInfoList[info.suite].Add(rd);
                }
                else
                {
                    m_RuneDataUI.Add(rd);
                    m_RuneDataInfoList.Add(info.suite, new List<RuneData>() { rd});
                }

            }
        }
    }

    void UpdateRuneData(int id)
    {
        foreach(KeyValuePair<ushort, List<RuneData>> kv in m_RuneDataInfoList)
        {
            List<RuneData> data = kv.Value;
            for(int i = 0;i< data.Count; i++)
            {
                data[i].SetHaveItem(id);
            }
        }
    }


}
