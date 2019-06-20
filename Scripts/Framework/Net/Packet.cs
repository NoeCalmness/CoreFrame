/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Basic Network Packet definition.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-27
 * 
 ***************************************************************************************************/

using System;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class PacketInfo : Attribute
{
    /// <summary>
    /// Protocol ID
    /// </summary>
    public ushort ID { get; set; }
    /// <summary>
    /// Protocol Group
    /// </summary>
    public byte group { get; set; }
    ///// <summary>
    ///// Is protocols an early send protocol ?
    ///// </summary>
    //public bool early { get; set; }
}

public class Packet : IDestroyable
{
    #region Static functions

    public static readonly Type infoType     = typeof(PacketInfo);
    public static readonly int  maxID        = ushort.MaxValue;
    public static readonly int  headerSize   = 6;
    public static readonly int  maxSize      = 1 << 24;
    public static readonly int  maxArraySize = ushort.MaxValue;

    public static readonly Type[] supportedTypes =
    {
        typeof(bool),   typeof(bool[]),                                       // bool
        typeof(byte),   typeof(sbyte),    typeof(byte[]),  typeof(sbyte[]),   // int8,  uint8
        typeof(short),  typeof(ushort),   typeof(short[]), typeof(ushort[]),  // int16, uint16
        typeof(int),    typeof(uint),     typeof(int[]),   typeof(uint[]),    // int32, uint32
        typeof(long),   typeof(ulong),    typeof(long[]),  typeof(ulong[]),   // int64, uint64
        typeof(float),  typeof(float[]),                                      // single
        typeof(double), typeof(double[]),                                     // double
        typeof(string), typeof(string[]),                                     // string
    };

    private static ThreadSafePool<Packet> m_pool = new ThreadSafePool<Packet>(256);

    public static PacketInfo GetPacketInfo(Type t)
    {
        var infos = t.GetCustomAttributes(infoType, true);
        if (infos.Length < 1) return new PacketInfo() { ID = 0, group = 0 };
        return (PacketInfo)infos[0];
    }

    public static bool isSupportedType(Type t)
    {
        return Array.IndexOf(supportedTypes, t) > -1;
    }

    public static Packet Build(long header, byte[] bytes, bool bytesHasHeader = true)
    {
        var p = m_pool.Pop();
        p.Set(header, bytes, bytesHasHeader);
        return p;
    }

    public static Packet Build(ushort ID, int capacity = 100)
    {
        var p = m_pool.Pop();
        p.Set(ID, capacity);
        return p;
    }

    #endregion

    public ushort ID { get; protected set; }
    public bool writable { get; protected set; }
    public bool readable { get; protected set; }
    public int dataSize { get { var s = writable ? m_wbytes.Count : readable ? m_rbytes.Length : 0; return m_flushed && s > 0 ? s - headerSize : s; } }
    public int size { get { return writable ? m_wbytes.Count : readable ? m_rbytes.Length : 0; } }
    public long header { get { return ID | (long)(dataSize + headerSize - 4) << 16; } }
    public int readPos { get { return m_readPos; } set { if (!readable) return; m_readPos = value > -1 && value < m_rbytes.Length ? value : m_rbytes.Length; } }
    public int writePos { get { return m_writePos; } set { if (!writable) return; m_writePos = value > -1 && value < m_wbytes.Count ? value : m_wbytes.Count; } }
    public byte[] bytes { get { return writable ? m_wbytes.ToArray() : readable ? m_rbytes : null; } }

    public bool destroyed { get; protected set; }
    public bool pendingDestroy { get { return false; } }

    private bool m_flushed;
    private int m_readPos;
    private int m_writePos;
    private List<byte> m_wbytes;
    private byte[] m_rbytes;

    protected Packet()
    {
        m_readPos  = 0;
        m_writePos = 0;
        m_flushed  = false;
        writable   = false;
        readable   = false;
        destroyed  = false;
        ID         = 0;
    }

    public virtual void Set(ushort _ID, int capacity = 100)
    {
        m_readPos  = 0;
        m_writePos = 0;
        m_wbytes   = new List<byte>(capacity);
        m_flushed  = false;
        writable   = true;
        readable   = false;
        destroyed  = false;
        ID         = _ID;
    }

    public virtual void Set(long _header, byte[] _bytes, bool bytesHasHeader = true)
    {
        if (bytesHasHeader && _bytes.Length < headerSize)
        {
            Logger.LogError("Packet::Set: Invalid header length, required: {0}, current: {1}", headerSize, _bytes.Length);
            m_rbytes  = new byte[] { };
            m_readPos = 0;
            m_flushed = false;
        }
        else
        {
            m_rbytes   = _bytes;
            m_readPos  = bytesHasHeader ? headerSize : 0;
            m_flushed  = bytesHasHeader;
        }

        m_writePos = 0;
        writable   = false;
        readable   = true;
        destroyed  = false;
        ID         = (ushort)(_header & 0xFFFF);
    }

    public virtual void Clear()
    {
        m_readPos  = 0;
        m_writePos = 0;
        m_flushed  = false;

        if (writable) m_wbytes.Clear();
        else Array.Clear(m_rbytes, 0, m_rbytes.Length);
    }

    public virtual void Reset()
    {
        m_readPos  = 0;
        m_writePos = 0;
        m_rbytes   = null;
        m_wbytes   = null;
        m_flushed  = false;
        writable   = false;
        readable   = false;
        ID         = 0;

        destroyed = true;
    }

    public virtual void Destroy()
    {
        if (destroyed) return;

        Reset();

        m_pool.Back(this);
    }

    /// <summary>
    /// Must be called before send to server
    /// </summary>
    public void Flush()
    {
        if (m_flushed) return;

        var t = m_writePos;
        m_writePos = 0;
        WriteHeader();
        m_writePos = t + headerSize;

        m_flushed = true;
    }

    private void WriteHeader()
    {
        Write(dataSize + headerSize - 4);   // 4 byte data contains length(lower 24 bit) and bitmask(higher 8 bit), note that server side packet header does not contain length (4 byte)
        Write(ID);                          // 2 byte data contains ID
    }

    #region Write operation

    public void Write(bool v)
    {
        _WriteByte((byte)(v ? 1 : 0));
    }

    public void Write(byte v)
    {
        _WriteByte(v);
    }

    public void Write(sbyte v)
    {
        _WriteByte((byte)v);
    }

    public void Write(short v)
    {
        _Write(ByteConverter.GetBytes(v));
    }

    public void Write(ushort v)
    {
        _Write(ByteConverter.GetBytes(v));
    }

    public void Write(int v)
    {
        _Write(ByteConverter.GetBytes(v));
    }

    public void Write(uint v)
    {
        _Write(ByteConverter.GetBytes(v));
    }

    public void Write(long v)
    {
        _Write(ByteConverter.GetBytes(v));
    }

    public void Write(ulong v)
    {
        _Write(ByteConverter.GetBytes(v));
    }

    public void Write(float v)
    {
        _Write(ByteConverter.GetBytes(v));
    }

    public void Write(double v)
    {
        _Write(ByteConverter.GetBytes(v));
    }

    public void Write(string v)
    {
        if (!CheckWrite()) return;
        var vv = System.Text.Encoding.UTF8.GetBytes(v == null ? "" : v);
        _WriteChecked(vv);
        if (vv.Length < 1 || vv[vv.Length - 1] != 0)
            _WriteByteChecked(0);
    }

    public void Write(bool[] v)
    {
        var count = CheckArraySize(v.Length);
        if (!CheckWrite()) return;

        var cv = new byte[count];
        for (var i = 0; i < count; ++i) cv[i] = (byte)(v[i] ? 1 : 0);

        _WriteByteChecked((byte)(count >> 8 & 0xFF));
        _WriteByteChecked((byte)(count & 0xFF));
        _WriteChecked(cv);
    }

    public void Write(byte[] v)
    {
        var count = CheckArraySize(v.Length);
        if (!CheckWrite()) return;

        var cv = new byte[count];
        Array.Copy(v, cv, count);

        _WriteByteChecked((byte)(count >> 8 & 0xFF));
        _WriteByteChecked((byte)(count & 0xFF));
        _WriteChecked(cv);
    }

    public void Write(sbyte[] v)
    {
        var count = CheckArraySize(v.Length);
        if (!CheckWrite()) return;

        var cv = new byte[count];
        for (var i = 0; i < count; ++i) cv[i] = (byte)v[i];

        _WriteByteChecked((byte)(count >> 8 & 0xFF));
        _WriteByteChecked((byte)(count & 0xFF));
        _WriteChecked(cv);
    }

    public void Write(short[] v)
    {
        var count = CheckArraySize(v.Length);
        if (!CheckWrite()) return;
        _WriteByteChecked((byte)(count >> 8 & 0xFF));
        _WriteByteChecked((byte)(count & 0xFF));
        for (var i = 0; i < count; ++i)
            _WriteChecked(ByteConverter.GetBytes(v[i]));
    }

    public void Write(ushort[] v)
    {
        var count = CheckArraySize(v.Length);
        if (!CheckWrite()) return;
        _WriteByteChecked((byte)(count >> 8 & 0xFF));
        _WriteByteChecked((byte)(count & 0xFF));
        for (var i = 0; i < count; ++i)
            _WriteChecked(ByteConverter.GetBytes(v[i]));
    }

    public void Write(int[] v)
    {
        var count = CheckArraySize(v.Length);
        if (!CheckWrite()) return;
        _WriteByteChecked((byte)(count >> 8 & 0xFF));
        _WriteByteChecked((byte)(count & 0xFF));
        for (var i = 0; i < count; ++i)
            _WriteChecked(ByteConverter.GetBytes(v[i]));
    }

    public void Write(uint[] v)
    {
        var count = CheckArraySize(v.Length);
        if (!CheckWrite()) return;
        _WriteByteChecked((byte)(count >> 8 & 0xFF));
        _WriteByteChecked((byte)(count & 0xFF));
        for (var i = 0; i < count; ++i)
            _WriteChecked(ByteConverter.GetBytes(v[i]));
    }

    public void Write(long[] v)
    {
        var count = CheckArraySize(v.Length);
        if (!CheckWrite()) return;
        _WriteByteChecked((byte)(count >> 8 & 0xFF));
        _WriteByteChecked((byte)(count & 0xFF));
        for (var i = 0; i < count; ++i)
            _WriteChecked(ByteConverter.GetBytes(v[i]));
    }

    public void Write(ulong[] v)
    {
        var count = CheckArraySize(v.Length);
        if (!CheckWrite()) return;
        _WriteByteChecked((byte)(count >> 8 & 0xFF));
        _WriteByteChecked((byte)(count & 0xFF));
        for (var i = 0; i < count; ++i)
            _WriteChecked(ByteConverter.GetBytes(v[i]));
    }

    public void Write(float[] v)
    {
        var count = CheckArraySize(v.Length);
        if (!CheckWrite()) return;
        _WriteByteChecked((byte)(count >> 8 & 0xFF));
        _WriteByteChecked((byte)(count & 0xFF));
        for (var i = 0; i < count; ++i)
            _WriteChecked(ByteConverter.GetBytes(v[i]));
    }

    public void Write(double[] v)
    {
        var count = CheckArraySize(v.Length);
        if (!CheckWrite()) return;
        _WriteByteChecked((byte)(count >> 8 & 0xFF));
        _WriteByteChecked((byte)(count & 0xFF));
        for (var i = 0; i < count; ++i)
            _WriteChecked(ByteConverter.GetBytes(v[i]));
    }

    public void Write(string[] v)
    {
        var count = CheckArraySize(v.Length);

        if (!CheckWrite()) return;
        _WriteByteChecked((byte)(count >> 8 & 0xFF));
        _WriteByteChecked((byte)(count & 0xFF));
        for (var i = 0; i < count; ++i)
        {
            var vv = System.Text.Encoding.UTF8.GetBytes(v[i]);
            _WriteChecked(vv);
            if (vv.Length < 1 || vv[vv.Length - 1] != 0)
                _WriteByteChecked(0);
        }
    }

    public void Write(IPacketObject v)
    {

        if (v == null)
        {
            Write((byte)0);
            return;
        }

        Write((byte)1);
        v.WriteToPacket(this);
    }

    public void Write(IPacketObject[] v)
    {
        var count = CheckArraySize(v.Length);
        if (!CheckWrite()) return;
        _WriteByteChecked((byte)(count >> 8 & 0xFF));
        _WriteByteChecked((byte)(count & 0xFF));
        for (var i = 0; i < count; ++i)
            v[i].WriteToPacket(this);
    }

    private int CheckArraySize(int count)
    {
        if (count > maxArraySize)
        {
#if UNITY_EDITOR
            Logger.LogException("Net::Packet::CheckArraySize: Toooooooooo large array size!! Clamp to {2}. Packet: [ID: {0}, name:{1}], ArraySize: {2}", ID, PacketObject.GetDefinedPacketName(ID), count, maxArraySize);
            UnityEditor.EditorApplication.isPaused = true;
#endif
            return maxArraySize;
        }

        return count;
    }

    private bool CheckWrite()
    {
        if (!writable)
        {
            Logger.LogException("Net::Packet::CheckWrite: Packet not writable!! Packet: [ID: {0}, name: {1}]", ID, PacketObject.GetDefinedPacketName(ID));
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPaused = true;
#endif
            return false;
        }

        return true;
    }

    private void _WriteByte(byte b)
    {
        if (!CheckWrite()) return;
        if (m_writePos < m_wbytes.Count && m_writePos > -1) m_wbytes.Insert(m_writePos, b);
        else m_wbytes.Add(b);
        m_writePos += 1;
    }

    private void _WriteByteChecked(byte b)
    {
        if (m_writePos < m_wbytes.Count && m_writePos > -1) m_wbytes.Insert(m_writePos, b);
        else m_wbytes.Add(b);
        m_writePos += 1;
    }

    private void _Write(byte[] _bytes)
    {
        if (!CheckWrite()) return;
        if (m_writePos < m_wbytes.Count && m_writePos > -1) m_wbytes.InsertRange(m_writePos, _bytes);
        else m_wbytes.AddRange(_bytes);

        m_writePos += _bytes.Length;
    }

    private void _WriteChecked(byte[] _bytes)
    {
        if (m_writePos < m_wbytes.Count && m_writePos > -1) m_wbytes.InsertRange(m_writePos, _bytes);
        else m_wbytes.AddRange(_bytes);

        m_writePos += _bytes.Length;
    }

    #endregion

    #region Read operation

    public byte Peek()
    {
        return !writable && m_readPos < size ? m_rbytes[m_readPos] : (byte)0;
    }

    public bool ReadBoolean()
    {
        return ReadByte() == 1;
    }

    public byte ReadByte()
    {
       return !writable && m_readPos < size ? m_rbytes[m_readPos++] : (byte)0;
    }

    public sbyte ReadSByte()
    {
        return (sbyte)ReadByte();
    }

    public short ReadInt16()
    {
        var rcount = CheckRead(2);
        var v = rcount < 2 ? (short)0 : ByteConverter.ToInt16(m_rbytes, m_readPos);
        m_readPos += rcount;
        return v;
    }

    public ushort ReadUInt16()
    {
        var rcount = CheckRead(2);
        var v = rcount < 2 ? (ushort)0 : ByteConverter.ToUInt16(m_rbytes, m_readPos);
        m_readPos += rcount;
        return v;
    }

    public int ReadInt32()
    {
        var rcount = CheckRead(4);
        var v = rcount < 4 ? 0 : ByteConverter.ToInt32(m_rbytes, m_readPos);
        m_readPos += rcount;
        return v;
    }

    public uint ReadUInt32()
    {
        var rcount = CheckRead(4);
        var v = rcount < 4 ? 0 : ByteConverter.ToUInt32(m_rbytes, m_readPos);
        m_readPos += rcount;
        return v;
    }

    public long ReadInt64()
    {
        var rcount = CheckRead(8);
        var v = rcount < 8 ? 0 : ByteConverter.ToInt64(m_rbytes, m_readPos);
        m_readPos += rcount;
        return v;
    }

    public ulong ReadUInt64()
    {
        var rcount = CheckRead(8);
        var v = rcount < 8 ? 0 : ByteConverter.ToUInt64(m_rbytes, m_readPos);
        m_readPos += rcount;
        return v;
    }

    public float ReadSingle()
    {
        var rcount = CheckRead(4);
        var v = rcount < 4 ? 0 : ByteConverter.ToSingle(m_rbytes, m_readPos);
        m_readPos += rcount;
        return v;
    }

    public double ReadDouble()
    {
        var rcount = CheckRead(8);
        var v = rcount < 8 ? 0 : ByteConverter.ToDouble(m_rbytes, m_readPos);
        m_readPos += rcount;
        return v;
    }

    public string ReadString()
    {
        if (writable) return string.Empty;
        var s = System.Text.Encoding.UTF8.GetString(ReadUntil(0));
        #if SHADOW_PACK
        if (!string.IsNullOrEmpty(s)) s = s.Replace(GeneralConfigInfo.sappName, Root.shadowAppName);
        #endif
        return s;
    }

    public bool[] ReadBooleanArray()
    {
        var count = ReadUInt16();
        if (count < 1) return new bool[] { };

        var rcount = CheckRead(count);

        var v = new bool[count];
        for (var i = 0; i < rcount; ++i) v[i] = m_rbytes[m_readPos + i] != 0;
        m_readPos += rcount;
        return v;
    }

    public byte[] ReadByteArray()
    {
        var count = ReadUInt16();
        if (count < 1) return new byte[] { };

        var rcount = CheckRead(count);

        var v = new byte[count];
        Array.Copy(m_rbytes, m_readPos, v, 0, rcount);
        m_readPos += rcount;
        return v;
    }

    public sbyte[] ReadSByteArray()
    {
        var bytes = ReadByteArray();
        var v = Array.ConvertAll(bytes, b => (sbyte)b);
        return v;
    }

    public short[] ReadInt16Array()
    {
        var count = ReadUInt16();
        if (count < 1) return new short[] { };

        var rcount = CheckRead(count * 2) / 2;

        var v = new short[count];
        for (var i = 0; i < rcount; ++i)
        {
            v[i] = ByteConverter.ToInt16(m_rbytes, m_readPos);
            m_readPos += 2;
        }
        if (rcount < count) m_readPos = size;

        return v;
    }

    public ushort[] ReadUInt16Array()
    {
        var count = ReadUInt16();
        if (count < 1) return new ushort[] { };

        var rcount = CheckRead(count * 2) / 2;

        var v = new ushort[count];
        for (var i = 0; i < rcount; ++i)
        {
            v[i] = ByteConverter.ToUInt16(m_rbytes, m_readPos);
            m_readPos += 2;
        }
        if (rcount < count) m_readPos = size;

        return v;
    }

    public int[] ReadInt32Array()
    {
        var count = ReadUInt16();
        if (count < 1) return new int[] { };

        var rcount = CheckRead(count * 4) / 4;

        var v = new int[count];
        for (var i = 0; i < rcount; ++i)
        {
            v[i] = ByteConverter.ToInt32(m_rbytes, m_readPos);
            m_readPos += 4;
        }
        if (rcount < count) m_readPos = size;

        return v;
    }

    public uint[] ReadUInt32Array()
    {
        var count = ReadUInt16();
        if (count < 1) return new uint[] { };

        var rcount = CheckRead(count * 4) / 4;

        var v = new uint[count];
        for (var i = 0; i < rcount; ++i)
        {
            v[i] = ByteConverter.ToUInt32(m_rbytes, m_readPos);
            m_readPos += 4;
        }
        if (rcount < count) m_readPos = size;

        return v;
    }

    public long[] ReadInt64Array()
    {
        var count = ReadUInt16();
        if (count < 1) return new long[] { };

        var rcount = CheckRead(count * 8) / 8;

        var v = new long[count];
        for (var i = 0; i < rcount; ++i)
        {
            v[i] = ByteConverter.ToInt64(m_rbytes, m_readPos);
            m_readPos += 8;
        }
        if (rcount < count) m_readPos = size;

        return v;
    }

    public ulong[] ReadUInt64Array()
    {
        var count = ReadUInt16();
        if (count < 1) return new ulong[] { };

        var rcount = CheckRead(count * 8) / 8;

        var v = new ulong[count];
        for (var i = 0; i < rcount; ++i)
        {
            v[i] = ByteConverter.ToUInt64(m_rbytes, m_readPos);
            m_readPos += 8;
        }
        if (rcount < count) m_readPos = size;

        return v;
    }

    public float[] ReadSingleArray()
    {
        var count  = ReadUInt16();
        if (count < 1) return new float[] { };

        var rcount = CheckRead(count * 4) / 4;

        var v = new float[count];
        for (var i = 0; i < rcount; ++i)
        {
            v[i] = ByteConverter.ToSingle(m_rbytes, m_readPos);
            m_readPos += 4;
        }
        if (rcount < count) m_readPos = size;

        return v;
    }

    public double[] ReadDoubleArray()
    {
        var count = ReadUInt16();
        if (count < 1) return new double[] { };

        var rcount = CheckRead(count * 8) / 8;

        var v = new double[count];
        for (var i = 0; i < rcount; ++i)
        {
            v[i] = ByteConverter.ToDouble(m_rbytes, m_readPos);
            m_readPos += 8;
        }
        if (rcount < count) m_readPos = size;

        return v;
    }

    public string[] ReadStringArray()
    {
        var count = ReadUInt16();
        if (count < 1) return new string[] { };

        var v = new string[count];
        for (var i = 0; i < count; ++i)
        {
            var s = System.Text.Encoding.UTF8.GetString(ReadUntil(0));
            #if SHADOW_PACK
            if (!string.IsNullOrEmpty(s)) s = s.Replace(GeneralConfigInfo.sappName, Root.shadowAppName);
            #endif
            v[i] = s;
        }

        return v;
    }

    public T ReadIPacketObject<T>(bool checkMask = true) where T : IPacketObject, new()
    {
        if (checkMask)
        {
            var mask = ReadByte();
            if (mask == 0) return default(T);
        }

        var v = new T();
        v.ReadFromPacket(this);
        return v;
    }

    public T[] ReadIPacketObjectArray<T>() where T : IPacketObject, new()
    {
        var count = ReadUInt16();
        var v = new T[count];
        for (var i = 0; i < count; ++i)
            v[i] = ReadIPacketObject<T>(false);

        return v;
    }

    public T ReadPacketObject<T>(bool checkMask = true) where T : PacketObject
    {
        if (checkMask)
        {
            var mask = ReadByte();
            if (mask == 0) return null;
        }

        var v = PacketObject.Create<T>();
        v.ReadFromPacket(this);
        return v;
    }

    public T[] ReadPacketObjectArray<T>() where T : PacketObject
    {
        var count = ReadUInt16();
        var v = new T[count];
        for (var i = 0; i < count; ++i)
            v[i] = ReadPacketObject<T>(false);

        return v;
    }

    public byte[] ReadUntil(byte v, bool withEnd = false)
    {
        if (writable) return new byte[] { };

        var end = Array.IndexOf(m_rbytes, v, m_readPos);
        var len = end < 0 ? size - m_readPos : end - m_readPos + 1;
        var ll  = end < 0 || withEnd ? len : len - 1;
        var vv  = new byte[ll];

        Array.Copy(m_rbytes, m_readPos, vv, 0, ll);
        m_readPos += len;

        return vv;
    }

    private int CheckRead(int count)
    {
        if (!readable)
        {
            Logger.LogException("Net::Packet::CheckRead: Packet not readable!! Packet: [ID: {0}, name: {1}], state: {2}", ID, PacketObject.GetDefinedPacketName(ID), destroyed ? "destroyed" : "normal");
            return 0;
        }

        if (m_readPos + count > size)
        {
            Logger.LogException("Net::Packet::CheckRead: Packet length too short!! Packet: [ID: {0}, name: {1}, curSize: {2}, readPos: {3}, readCount: {4}]", ID, PacketObject.GetDefinedPacketName(ID), size, readPos, count);
            return size - m_readPos;
        }

        return count;
    }

    #endregion
}
