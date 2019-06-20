// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-09-15      13:26
//  * LastModify：2018-09-15      13:26
//  ***************************************************************************************************/

using System.IO;

public class Command : IDestroyable
{
    private static ThreadSafePool<Command> m_pool = new ThreadSafePool<Command>(256);


    public int Frame;
    public PacketObject cache;
    private Packet packet;

    public void Init(PacketObject msg, int rFrame)
    {
        var p = msg.BuildPacket();
        packet = Packet.Build(p.header, p.bytes, false);
        cache = PacketObject.Create(packet);
        Frame = rFrame;
    }

    public void Serialize(Stream rWriter)
    {
        rWriter.Write(Frame);
        if (null == packet)
        {
            rWriter.Write(0);
            return;
        }
        rWriter.Write(1);

        rWriter.Write(packet.header);
        rWriter.Write(packet.bytes);
    }

    public Command UnSerialize(Stream rReader)
    {
        rReader.Read(out Frame);
        int b = 0;
        rReader.Read(out b);
        if (b == 0)
            return this;
        long header;
        rReader.Read(out header);
        byte[] datas;
        rReader.Read(out datas);
        var p = Packet.Build(header, datas, false);
        cache = PacketObject.Create(p);
        return this;
    }

    public Command Reset(PacketObject rObject)
    {
        if (null == rObject)
            return this;
        var p = rObject.BuildPacket();
        packet = Packet.Build(p.header, p.bytes, false);
        cache = PacketObject.Create(packet);
        return this;
    }

    public static Command Create()
    {
        return m_pool.Pop();
    }

    public void OnDestroy()
    {
        cache.Destroy();
    }

    public bool destroyed { get; protected set; }
    public bool pendingDestroy { get { return false; } }

    public void Destroy()
    {
        if (destroyed) return;

        destroyed = true;
        Frame = 0;
        cache = null;
        packet = null;

        m_pool.Back(this);
    }
}

public static class StreamExpends
{
    public static void Write(this Stream rStream, long rValue)
    {
        var buffer = ByteConverter.GetBytes(rValue);
        rStream.Write(buffer, 0, buffer.Length);
    }

    public static void Write(this Stream rStream, int rValue)
    {
        var buffer = ByteConverter.GetBytes(rValue);
        rStream.Write(buffer, 0, buffer.Length);
    }
    public static void Write(this Stream rStream, ulong rValue)
    {
        var buffer = ByteConverter.GetBytes(rValue);
        rStream.Write(buffer, 0, buffer.Length);
    }

    public static void Write(this Stream rStream, byte[] rValue)
    {
        rStream.Write(rValue.Length);
        rStream.Write(rValue, 0, rValue.Length);
    }

    public static void Write(this Stream rStream, string rValue)
    {
        rStream.Write(System.Text.Encoding.UTF8.GetBytes(rValue.ToCharArray()));
    }

    public static void Read(this Stream rStream, out int rValue)
    {
        var buffer = new byte[4];
        rStream.Read(buffer, 0, 4);
        rValue = ByteConverter.ToInt32(buffer, 0);
    }

    public static void Read(this Stream rStream, out ulong rValue)
    {
        var buffer = new byte[8];
        rStream.Read(buffer, 0, 8);
        rValue = ByteConverter.ToUInt64(buffer, 0);
    }

    public static void Read(this Stream rStream, out long rValue)
    {
        var buffer = new byte[8];
        rStream.Read(buffer, 0, 8);
        rValue = ByteConverter.ToInt64(buffer, 0);
    }

    public static void Read(this Stream rStream, out byte[] rValue)
    {
        var buffer = new byte[4];
        rStream.Read(buffer, 0, 4);
        var size = ByteConverter.ToInt32(buffer, 0);
        rValue = new byte[size];
        rStream.Read(rValue, 0, size);
    }

    public static void Read(this Stream rStream, out string rValue)
    {
        byte[] temp;
        rStream.Read(out temp);
        rValue = System.Text.Encoding.UTF8.GetString(temp);
    }
}