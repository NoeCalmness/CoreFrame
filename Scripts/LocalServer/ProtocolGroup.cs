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

#endregion

[Serializable]
public class ProtocolGroup
{
    public List<PacketObject> packetObjects;

    [ExcludeFromJson] public List<string> packets;

    public string requestType;

    public ProtocolGroup()
    {
        packets = new List<string>();
    }

    public void UpdatePacketObject()
    {
        packetObjects = new List<PacketObject>();
        foreach (var s in packets)
            packetObjects.Add(JsonMapper.ToObject<PacketObject>(s));
    }

    public void UpdateString()
    {
        if (packetObjects != null)
        {
            foreach (var p in packetObjects)
                packets.Add(JsonMapper.ToJson(p, true));
        }
    }
}
#endif