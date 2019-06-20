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
using System.IO;
using System.Reflection;
using LitJson;
using UnityEngine;

#endregion

[ExecuteInEditMode]
public class LocalServer : Singleton<LocalServer>
{
    private LSDataSource _dataSource;

    public Type currentRequestType;

    public string dataPath;
    public bool isWorking;

    public LSDataSource dataSource
    {
        get
        {
            if (_dataSource == null)
                _dataSource = ScriptableObject.CreateInstance<LSDataSource>();
            return _dataSource;
        }
    }

    public bool SetCurrentRequestType(Type rType)
    {
        currentRequestType = rType;
        return true;
    }

    public bool AutoHandlePacket(Type rType, Session rSession)
    {
        if (!isWorking || rSession == null || !IsProtocolType(rType))
            return false;
        var protocols = dataSource.GetProtocols(rType);
        if (protocols == null || protocols.Count == 0)
            return false;

        Logger.Log(LogType.RECV, "LocalServer.instance Request : {0}", rType.Name);
        foreach (var packet in protocols)
        {
            Logger.Log(LogType.RECV, "LocalServer.instance Handle {0} : {1}", packet._name,
                JsonMapper.ToJson(packet, true));
            rSession.HandlePacket(packet);
        }
        return true;
    }

    private bool IsProtocolType(Type rType)
    {
        return rType?.GetCustomAttribute<PacketInfo>() != null;
    }

    public void SaveToFile(string rPath)
    {
        if (null == dataSource.entrys || dataSource.entrys.Length == 0)
            return;

        foreach (var entry in dataSource.entrys)
            entry.UpdatePacketObject();

        var str = JsonMapper.ToJson(dataSource.entrys, true);
        var stream = File.CreateText(rPath);
        stream.Write(str);
        stream.Close();
    }

    public void Load(string rPath)
    {
        var str = File.ReadAllText(rPath);
        dataSource.entrys = JsonMapper.ToObject<ProtocolGroup[]>(str);

        foreach (var entry in dataSource.entrys)
            entry.UpdateString();
    }
}

#endif