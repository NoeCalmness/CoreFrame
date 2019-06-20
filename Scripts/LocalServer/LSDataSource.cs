// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-02-16      9:38
//  *LastModify：2019-02-20      11:07
//  ***************************************************************************************************/

#if UNITY_EDITOR

#region

using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

#endregion

[Serializable]
public class LSDataSource : ScriptableObject
{
    public ProtocolGroup[] entrys;

    public int Count
    {
        get { return entrys.Length; }
    }

    public bool ContainsKey(Type rType)
    {
        return Array.Exists(entrys, p => p.requestType == rType.Name);
    }

    public void Add(Type rType, ProtocolGroup rData)
    {
        if (rData != null && rType != null)
            rData.requestType = rType.Name;

        if (entrys == null)
        {
            entrys = new ProtocolGroup[1];
            entrys[0] = rData;
            return;
        }
        var index = Array.FindIndex(entrys, p => p.requestType == rType.Name);
        if (index == -1)
        {
            Array.Resize(ref entrys, entrys.Length + 1);
            entrys[entrys.Length - 1] = rData;
        }
        else
            entrys[index] = rData;
    }

    public IReadOnlyList<PacketObject> GetProtocols(Type rType)
    {
        var index = Array.FindIndex(entrys, p => p.requestType == rType.Name);

        if (index == -1)
            return Util.EmptyList<PacketObject>();

        var packets = new List<PacketObject>();
        foreach (var s in entrys[index].packets)
            packets.Add(JsonMapper.ToObject<PacketObject>(s));
        return packets;
    }

    public void RefreshPacketObject()
    {
        if (null == entrys)
            return;

        for (var i = 0; i < entrys.Length; i++)
            entrys[i].UpdatePacketObject();
    }
}
#endif