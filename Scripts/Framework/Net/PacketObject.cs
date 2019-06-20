/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Basic Network PacketObject definition.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-27
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;

public interface IPacketObject
{
    void WriteToPacket(Packet p);
    void ReadFromPacket(Packet p);
};

public abstract class PacketObject : IPacketObject, IDestroyable
{
    #region Static functions

    private const int MAX_POOL_SIZE = 256;

    private static Dictionary<int, ThreadSafePool<PacketObject>> m_pool = new Dictionary<int, ThreadSafePool<PacketObject>>();
    private static Dictionary<int, Type> m_definedPackets = new Dictionary<int, Type>();

    public static T Create<T>() where T : PacketObject
    {
        var t = typeof(T);
        var hash = t.GetHashCode();
        var p = m_pool.Get(hash);
        if (p == null) { p = new ThreadSafePool<PacketObject>(MAX_POOL_SIZE, t); m_pool.Add(hash, p); }

        var packet = (T)p.Pop();
        packet.destroyed = false;
        packet.dontDestroyOnSend = false;
        packet.dontDestroyOnRecv = false;

        return packet;
    }

    public static PacketObject Create(Type type = null)
    {
        var hash = type.GetHashCode();
        var p    = m_pool.Get(hash);
        if (p == null) { p = new ThreadSafePool<PacketObject>(MAX_POOL_SIZE, type); m_pool.Add(hash, p); }

        var packet = p.Pop();
        packet.destroyed = false;
        packet.dontDestroyOnSend = false;
        packet.dontDestroyOnRecv = false;

        return packet;
    }

    /// <summary>
    /// Create a packet object from packet data
    /// Note: check packet param before calling this method!
    /// </summary>
    /// <param name="packet">The packet data</param>
    /// <returns></returns>
    public static PacketObject Create(Packet packet)
    {
        var type = GetDefinedPacketType(packet.ID);
        if (type == null) return null;

        var p = Create(type);
        p.ReadFromPacket(packet);

        return p;
    }

    public static void BackArray(PacketObject[] arr)
    {
        if (arr == null || arr.Length < 1) return;
        for (var i = 0; i < arr.Length; ++i)
            arr[i].Destroy();
    }

    public static void ReNew<T>(ref T p) where T : PacketObject
    {
        if (p == null || p.destroyed) p = Create<T>();
        else p.Clear();
    }

    private static bool m_packetRegistered = false;

    public static Type GetDefinedPacketType(int packetID)
    {
        return m_definedPackets.Get(packetID);
    }

    public static string GetDefinedPacketName(int packetID)
    {
        var type = m_definedPackets.Get(packetID);
        return type != null ? type.Name : "Undefined";
    }

    public static void CollectAllRegisteredPackets()
    {
        if (m_packetRegistered) return;
        m_packetRegistered = true;

        var type = typeof(PacketObject);
        var types = type.Assembly.GetTypes();

        foreach (var t in types)
        {
            if (!t.IsSubclassOf(type) || t.IsAbstract) continue;  // Ignore abstract class

            var info = Packet.GetPacketInfo(t);
            if (info.ID == 0)
            {
                Logger.LogError("PacketObject::CollectAllRegisteredPackets: Packet [{0}] has invalid packet definition, ID can not be zero.", t);
                continue;
            }
            var ot = m_definedPackets.Get(info.ID);
            if (ot != null)
            {
                Logger.LogError("PacketObject::CollectAllRegisteredPackets: Packet [{0}:{1}] has duplicated packet ID. Old packet: [{1}]", info.ID, t, ot);
                continue;
            }

            m_definedPackets.Add(info.ID, t);
        }
    }

    #endregion

    [LitJson.ExcludeFromJson]
    public ushort _ID { get; protected set; }
    [LitJson.ExcludeFromJson]
    public byte _group { get; protected set; }
    [LitJson.ExcludeFromJson]
    public string _name { get; protected set; }
    [LitJson.ExcludeFromJson]
    public int _hash { get; protected set; }
    [LitJson.ExcludeFromJson]
    public bool destroyed { get; protected set; }
    [LitJson.ExcludeFromJson]
    public bool pendingDestroy { get { return false; } }
    [LitJson.ExcludeFromJson]
    public bool dontDestroyOnSend { get; set; }
    [LitJson.ExcludeFromJson]
    public bool dontDestroyOnRecv { get; set; }

    public abstract void ReadFromPacket(Packet p);
    public abstract void WriteToPacket(Packet p);

    protected PacketObject()
    {
        var info = Packet.GetPacketInfo(GetType());

        _ID            = info.ID;
        _group         = info.group;
        _name          = GetType().Name;
        _hash          = GetType().GetHashCode();

    }
    public virtual void Clear() { }

    public virtual Packet BuildPacket()
    {
        var p = Packet.Build(_ID);
        WriteToPacket(p);
        return p;
    }

    public void Destroy()
    {
        if (destroyed) return;
        destroyed = true;

        Clear();

        m_pool.GetDefault(_hash).Back(this);
    }
}

public abstract class PacketObject<T> : PacketObject where T : PacketObject<T>
{
    public override void ReadFromPacket(Packet p) { }
    public override void WriteToPacket(Packet p) { }

    public virtual void CopyTo(ref T dst)
    {
        ReNew(ref dst);
    }

    public void CopyTo(T dst)
    {
        if (dst == null) return;
        CopyTo(ref dst);
    }

    public T Clone()
    {
        T t = null;
        CopyTo(ref t);
        return t;
    }
}